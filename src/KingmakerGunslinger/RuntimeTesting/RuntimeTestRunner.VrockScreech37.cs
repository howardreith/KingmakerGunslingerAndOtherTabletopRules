using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// Stunning Screech on live units. The printed exemption is tested by
        /// identity and not by a Boolean: the two released demons the first
        /// hand-written set omitted are positive controls, a registered devil
        /// is a negative control, and an allied non-demon is affected, so the
        /// exemption is demonstrably neither an alignment, an outsider, a
        /// faction nor an ally rule.
        /// </summary>
        private IEnumerable<int> ReviewVrockScreech37(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = turnBased
                    ? ExpandedSummoningNativeProfilePolicy
                        .Sprint37VrockTurnsPhase
                    : ExpandedSummoningNativeProfilePolicy
                        .Sprint37VrockScreechPhase,
                ["turnBased"] = turnBased,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            UnitEntityData ally = fixture.Caster;
            int oldTargetFort = target.Stats.SaveFortitude.BaseValue;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldAllyFort = ally.Stats.SaveFortitude.BaseValue;
            Vector3 oldTargetPosition = target.Position;
            Vector3 oldAllyPosition = ally.Position;
            bool oldMode = Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                .EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            string suffix = turnBased ? "-turnbased" : "";
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    row["turnBased"] = turnBased;
                    // The requested mode is a label; this is the engine's own
                    // answer. Every cell records it, and a turn-based cell
                    // after the mode switch refuses to report at all if the
                    // game is actually running in real time - so a -turnbased
                    // assertion cannot stand for a mode never entered. The
                    // roster cell is identity-only and is read before the
                    // switch, which the receipt shows.
                    row["nativeTurnBasedMode"] = TurnBased.Controllers
                        .CombatController.IsInTurnBasedCombat();
                    if (turnBased && !TurnBased.Controllers.CombatController
                            .IsInTurnBasedCombat() &&
                        id != "screech-control-roster")
                        throw new InvalidOperationException(
                            "Sprint37 turn-based screech cell observed real-time combat: "
                            + id);
                    cells.Add(row);
                    Sprint25Check("sprint37-" + id + suffix, pass, row,
                        description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint37 vrock screech cell failed: " + id);
                };

            try
            {
                var screech = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                        .Vrock37Prefix + "Screech"];
                var stunned = (BlueprintBuff)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                        .Vrock37Prefix + "Stunned"];
                var recharge = (BlueprintBuff)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                        .Vrock37Prefix + "ScreechRecharge"];

                // A screech is a save-or-lose, so the controls are set up to
                // fail honestly rather than to be forced after the fact.
                target.Stats.SaveFortitude.BaseValue = -100;
                target.Stats.HitPoints.BaseValue = 100000;
                ally.Stats.SaveFortitude.BaseValue = -100;

                UnitEntityData vrock = CastExpandedSummoningOwnTier(
                    fixture, "vrock");
                owned.Add(vrock);
                VrockQuiesce37(vrock);
                foreach (int frame in Outsider30NativeAppearance(vrock,
                    vrock.Blueprint.name)) yield return frame;

                // Positive exemption controls and one negative control, each a
                // real registered creature rather than a flag on a fixture.
                var controls = new List<Tuple<string, UnitEntityData, bool>>();
                foreach (string key in new[] { "succubus", "shadow-demon",
                    "bone-devil" })
                {
                    UnitEntityData unit = CastExpandedSummoningOwnTier(
                        fixture, key);
                    owned.Add(unit);
                    VrockQuiesce37(unit);
                    unit.Stats.SaveFortitude.BaseValue = -100;
                    unit.Stats.HitPoints.BaseValue = 100000;
                    controls.Add(Tuple.Create(key, unit,
                        SummonDemonIdentityCatalog.IsDemonKey(key)));
                    // Deliberately no appearance settling for the controls.
                    // This phase observes the printed exemption by identity
                    // and by whether a save is rolled; a control's body is a
                    // released creature qualified in its own sprint, and
                    // waiting on three donor views to settle added a
                    // precondition this observation does not depend on - the
                    // first screech run failed on the Succubus view before any
                    // screech was cast. Their readiness to be saved against is
                    // what matters, and that is waited for below.
                    foreach (int frame in WaitInfernalSupport34(unit))
                        yield return frame;
                }
                // No daemon is registered in this roster, so a daemon control
                // cannot be observed here. The implementation never treats one
                // as a demon because membership is an explicit six-key set,
                // which the domain roster case covers by identity.
                check("screech-control-roster",
                    controls.Count == 3 &&
                    controls.Count(value => value.Item3) == 2 &&
                    !SummonDemonIdentityCatalog.IsDemonKey("bone-devil") &&
                    !SummonDemonIdentityCatalog.ExcludedOutsiderKeys
                        .Any(SummonDemonIdentityCatalog.IsDemonKey),
                    new JObject {
                        ["controls"] = new JArray(controls.Select(value =>
                            new JObject {
                                ["key"] = value.Item1,
                                ["unit"] = value.Item2.UniqueId,
                                ["blueprint"] =
                                    value.Item2.Blueprint.AssetGuid,
                                ["expectedExempt"] = value.Item3,
                                ["inState"] = value.Item2.IsInState,
                                ["canAct"] =
                                    value.Item2.Descriptor.State.CanAct,
                                ["alive"] =
                                    !value.Item2.Descriptor.State.IsDead,
                                ["fortitudeBase"] = value.Item2.Stats
                                    .SaveFortitude.BaseValue
                            })),
                        ["daemonRegistered"] = false,
                        ["excludedOutsiders"] = new JArray(
                            SummonDemonIdentityCatalog.ExcludedOutsiderKeys)
                    }, "Two released demons and one released devil stand as real controls; no daemon exists in this roster to observe.");

                // All controls plus the enemy and the ally inside the radius.
                UnitEntityData[] inside = new[] { target, ally }
                    .Concat(controls.Select(value => value.Item2)).ToArray();
                VrockRing37(vrock, inside,
                    VrockRules37Policy.ScreechRadiusFeet - 5f);

                if (turnBased)
                {
                    // Real turn-based play, through the engine's own supported
                    // toggle. Until this landed, the turns phase differed from
                    // the real-time phase only by a suffix on its assertion
                    // names - a label, not an observation of the second mode.
                    // Every cast below already waits for its caster's own
                    // native turn, so the switch was the only thing missing.
                    var admission = new JObject();
                    foreach (int frame in VrockEnterTurnBased37(vrock, target,
                        admission)) yield return frame;
                    check("screech-turn-admission",
                        (bool)admission["admitted"] &&
                        (bool)admission["ownerActing"] &&
                        (bool)admission["nativeTurnBasedMode"], admission,
                        "The vrock is enrolled in native turn-based combat through the engine's own real-time enrollment and turn-order toggle, and holds its own turn before any screech is ordered.");
                }

                // The manual path, observed as a player would reach it.
                // The owner decision keeps automatic use conservative, so the
                // player's own command is the route that must stay open - and
                // a claim of manual availability is only honest if the ability
                // is actually on the action bar rather than merely invocable
                // by a fixture.
                var granted = vrock.Descriptor.Abilities.GetAbility(screech);
                var manual = granted == null ? null : new AbilityData(granted);
                check("screech-manual-interface",
                    granted != null && granted.Active &&
                    !screech.Hidden && !screech.ActionBarAutoFillIgnored &&
                    screech.Type == AbilityType.Supernatural &&
                    screech.ActionType == UnitCommand.CommandType.Standard &&
                    screech.CanTargetSelf && manual != null &&
                    manual.IsAvailable &&
                    vrock.Buffs.GetBuff(recharge) == null,
                    new JObject {
                        ["owner"] = vrock.UniqueId,
                        ["ability"] = screech.name,
                        ["granted"] = granted != null,
                        ["factActive"] = granted != null && granted.Active,
                        ["hiddenFromUi"] = screech.Hidden,
                        ["actionBarAutoFillIgnored"] =
                            screech.ActionBarAutoFillIgnored,
                        ["abilityType"] = screech.Type.ToString(),
                        ["actionType"] = screech.ActionType.ToString(),
                        ["availableBeforeUse"] = manual?.IsAvailable,
                        ["hourMarkerBeforeUse"] =
                            vrock.Buffs.GetBuff(recharge) != null,
                        ["rechargeHours"] =
                            VrockRules37Policy.ScreechRechargeHours,
                        ["scope"] =
                            "The ability is granted, active, visible to the action bar and not auto-fill-excluded, so the player's own standard action can reach it; the volley below issues the same UnitUseAbility command the action bar issues, and the refusal cell observes the hour rule afterwards."
                    }, "Stunning Screech is reachable by the player's own standard action through the real action bar, not only by a fixture, and is available before its hour marker exists.");

                foreach (int frame in VrockScreechVolley37(fixture, vrock,
                    screech, stunned, recharge, inside, target, ally,
                    controls, turnBased, check)) yield return frame;

                foreach (int frame in VrockScreechLiveDc37(fixture, vrock,
                    screech, stunned, recharge, target, turnBased, check))
                    yield return frame;

                foreach (int frame in VrockScreechBoundary37(fixture, vrock,
                    screech, stunned, recharge, target, ally, turnBased,
                    check)) yield return frame;

                foreach (int frame in VrockScreechTwoOwners37(fixture, vrock,
                    screech, stunned, recharge, target, owned, turnBased,
                    check)) yield return frame;
            }
            finally
            {
                // The native mode and pause state belong to the player, not to
                // this observation.
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                    .EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
                receipt["nativeModeRestored"] = TurnBased.Controllers
                    .CombatController.IsInTurnBasedCombat() == oldMode;
                foreach (Buff buff in new[] { target, ally }
                    .SelectMany(unit => unit.Buffs.Enumerable).ToArray())
                    if (buff.Blueprint != null && buff.Blueprint.name
                        .StartsWith("KMG_", StringComparison.Ordinal))
                        buff.Remove();
                target.Stats.SaveFortitude.BaseValue = oldTargetFort;
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                ally.Stats.SaveFortitude.BaseValue = oldAllyFort;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                PlaceExpandedSummoningUnit(ally, oldAllyPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory, turnBased
                        ? "sprint37-vrock-turns.json"
                        : "sprint37-vrock-screech.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        /// <summary>
        /// Refuses with the real reason if a summoned observer subject has
        /// outlived its own duration. The spores phase learned this the
        /// expensive way: an expired summon reports only "Ability not
        /// granted", which reads as a registration failure and is not one.
        /// </summary>
        private static void VrockRequireLiveSubject37(UnitEntityData unit,
            BlueprintAbility ability, string step)
        {
            if (unit == null || unit.Destroyed || !unit.IsInState)
                throw new InvalidOperationException(
                    "The summoned subject no longer exists at step '" + step +
                    "': its summoned lifetime ended. Split the phase or " +
                    "summon a fresh unit; this is not a registration failure.");
            if (unit.Descriptor.Abilities.GetAbility(ability) == null)
                throw new InvalidOperationException(
                    "The summoned subject no longer holds " + ability.name +
                    " at step '" + step + "': its summoned lifetime ended. " +
                    "This is not a registration failure.");
        }

        /// <summary>
        /// Enrolls the vrock and its victim in native turn-based combat the
        /// way the released turn-based reviews do: establish real combat in
        /// real time first, then let the turn-based controller rebuild its
        /// order from those combat states. Nothing writes initiative, the
        /// current turn, cooldowns or command state.
        /// </summary>
        private IEnumerable<int> VrockEnterTurnBased37(UnitEntityData vrock,
            UnitEntityData target, JObject evidence)
        {
            vrock.Memory.Add(target);
            target.Memory.Add(vrock);
            vrock.SightCache.ClearIfNeeded();
            target.SightCache.ClearIfNeeded();
            new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
            vrock.JoinCombat();
            target.JoinCombat();
            Game.Instance.Player.UpdateIsInCombat();
            new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
            new Kingmaker.Controllers.Combat.UnitCombatPrepareController()
                .Tick();
            foreach (int frame in Sprint22EnrollTurnBasedOwners(
                new[] { vrock }, target, evidence)) yield return frame;
            DateTime deadline = DateTime.UtcNow.AddSeconds(90);
            int frames = 0;
            while (frames++ < 900 && DateTime.UtcNow < deadline &&
                !Sprint18OwnerIsActing(vrock))
            {
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            evidence["ownerActing"] = Sprint18OwnerIsActing(vrock);
            evidence["ownerVisible"] = vrock.IsVisibleForPlayer;
            evidence["nativeTurnBasedMode"] = TurnBased.Controllers
                .CombatController.IsInTurnBasedCombat();
            evidence["round"] = Game.Instance.TurnBasedCombatController
                .RoundNumber;
            evidence["framesWaited"] = frames;
        }

        /// <summary>
        /// Brings one further unit into the combat and native turn order this
        /// observation already established, without letting it act.
        /// </summary>
        private IEnumerable<int> VrockJoinTurnOrder37(UnitEntityData unit,
            UnitEntityData target, JObject evidence)
        {
            unit.Memory.Add(target);
            target.Memory.Add(unit);
            unit.SightCache.ClearIfNeeded();
            new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
            unit.JoinCombat();
            Game.Instance.Player.UpdateIsInCombat();
            new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
            new Kingmaker.Controllers.Combat.UnitCombatPrepareController()
                .Tick();
            Game.Instance.TurnBasedCombatController.Activate();
            for (int frame = 0; frame < 8; frame++) yield return 0;
            evidence["unit"] = unit.UniqueId;
            evidence["inCombat"] = unit.IsInCombat;
            evidence["sorted"] = Game.Instance.TurnBasedCombatController
                .SortedUnits.Any(value => ReferenceEquals(value, unit));
            evidence["nativeTurnBasedMode"] = TurnBased.Controllers
                .CombatController.IsInTurnBasedCombat();
        }

        private static void VrockQuiesce37(UnitEntityData unit)
        {
            Sprint28SetBrainActivePreservingActions(unit, false);
            unit.Commands.InterruptAll(true);
            unit.Commands.RemoveFinishedAndUpdateQueue();
            unit.Stats.HitPoints.BaseValue = 100000;
        }

        /// <summary>
        /// Places units evenly around the caster at a requested distance from
        /// the caster's centre, so a ring of controls is inside one area
        /// without any of them standing on top of each other.
        /// </summary>
        private static void VrockRing37(UnitEntityData centre,
            UnitEntityData[] units, float radiusFeet)
        {
            float metres = new Feet(radiusFeet).Meters;
            for (int index = 0; index < units.Length; index++)
            {
                float angle = index * (2f * Mathf.PI / units.Length);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f,
                    Mathf.Sin(angle)) * metres;
                PlaceExpandedSummoningUnit(units[index],
                    centre.Position + offset);
            }
        }

        /// <summary>
        /// One real screech against the whole ring, then the printed one-round
        /// expiry and the recharge refusal, all from the same volley.
        /// </summary>
        private IEnumerable<int> VrockScreechVolley37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData vrock,
            BlueprintAbility screech, BlueprintBuff stunned,
            BlueprintBuff recharge, UnitEntityData[] inside,
            UnitEntityData target, UnitEntityData ally,
            List<Tuple<string, UnitEntityData, bool>> controls,
            bool turnBased, Action<string, bool, JObject, string> check)
        {
            VrockRequireLiveSubject37(vrock, screech, "volley");
            // The exemption cell needs the enemy, the ally and the devil to
            // all fail one Fortitude save, and a natural twenty always
            // succeeds, so without a bounded retry this cell fails a correct
            // creature about one run in seven. The released Sprint 36 burst
            // observer solved exactly this: retry only when every
            // deterministic fact was already exact and the sole shortfall was
            // a natural twenty, record every attempt, and force no roll.
            const int maximumAttempts = 3;
            var attempts = new JArray();
            bool volleyPassed = false;
            Action report = () => check("screech-identity-exemption",
                volleyPassed, new JObject {
                    ["owner"] = vrock.UniqueId,
                    ["attempts"] = attempts,
                    ["maximumAttempts"] = maximumAttempts,
                    ["selectedAttempt"] = volleyPassed ?
                        (int?)attempts.Count : null,
                    ["forcedRolls"] = 0,
                    ["retryRule"] =
                        "Retried only when the exemption pattern, the command and the hour marker were already exact and the sole shortfall was a natural twenty; no roll was forced and no assertion was relaxed."
                }, "The exemption follows registered demon identity: both released demons are never even asked to save, while a devil, an enemy and an ally of the summoner all are.");
            for (int attempt = 1; attempt <= maximumAttempts && !volleyPassed;
                attempt++)
            {
            if (attempt > 1)
            {
                // Undo only this owner's own leftovers before rerolling: every
                // stun it applied inside the ring, and its hour marker.
                foreach (UnitEntityData unit in inside)
                    foreach (Buff clear in unit.Buffs.Enumerable.Where(
                        value => ReferenceEquals(value.Blueprint, stunned))
                        .ToArray()) clear.Remove();
                foreach (int frame in VrockClearRecharge37(vrock, recharge,
                    target, stunned, turnBased)) yield return frame;
            }
            var observed = new Astral31SmiteEvents(vrock, screech, stunned,
                null, inside);
            EventBus.Subscribe(observed);
            UnitUseAbility command = null;
            bool naturalRetry = false;
            try
            {
                foreach (int frame in Sprint26Cast(vrock, screech,
                    new TargetWrapper(vrock), value => command = value))
                    yield return frame;
                Func<UnitEntityData, Buff> stun = unit => unit.Buffs.Enumerable
                    .FirstOrDefault(value => ReferenceEquals(value.Blueprint,
                        stunned) && ReferenceEquals(
                        value.Context?.MaybeCaster, vrock));
                bool commandExact = command != null && command.IsFinished &&
                    command.Result == UnitCommand.ResultType.Success &&
                    command.ExecutionProcess != null &&
                    command.ExecutionProcess.IsEnded;
                RuleSavingThrow targetSave = observed.Save(target);
                RuleSavingThrow allySave = observed.Save(ally);
                var controlRows = new JArray();
                bool controlsCoherent = true;
                bool controlNaturalTwenty = false;
                foreach (var control in controls)
                {
                    RuleSavingThrow save = observed.Save(control.Item2);
                    Buff effect = stun(control.Item2);
                    bool exempt = control.Item3;
                    controlNaturalTwenty |= !exempt && save != null &&
                        save.IsPassed && save.D20.Value == 20;
                    // An exempt creature is not merely unaffected: it is never
                    // asked to save, because the exemption is decided before
                    // the roll.
                    bool coherent = exempt
                        ? save == null && effect == null
                        : save != null && !save.IsPassed && effect != null;
                    controlsCoherent &= coherent;
                    controlRows.Add(new JObject {
                        ["key"] = control.Item1,
                        ["unit"] = control.Item2.UniqueId,
                        ["expectedExempt"] = exempt,
                        ["saveRolled"] = save != null,
                        ["saveD20"] = save?.D20.Value,
                        ["saveDc"] = save?.DifficultyClass,
                        ["savePassed"] = save?.IsPassed,
                        ["stunned"] = effect == null ? null :
                            InfernalSavedEffect34(effect),
                        ["coherent"] = coherent,
                        ["row"] = observed.Row(control.Item2)
                    });
                }
                bool enemyAndAlly = targetSave != null &&
                    !targetSave.IsPassed && stun(target) != null &&
                    allySave != null && !allySave.IsPassed &&
                    stun(ally) != null;
                // Everything the product controls, independent of the three
                // rolls: the command, the hour marker, the exemption pattern
                // itself - no exempt creature was asked to save and no
                // non-exempt creature was skipped - and that both the enemy
                // and the ally were actually asked.
                bool exemptionExact = commandExact && controlsCoherent &&
                    vrock.Buffs.GetBuff(recharge) != null &&
                    targetSave != null && allySave != null;
                volleyPassed = exemptionExact && enemyAndAlly;
                bool naturals = controlNaturalTwenty ||
                    (targetSave != null && targetSave.IsPassed &&
                        targetSave.D20.Value == 20) ||
                    (allySave != null && allySave.IsPassed &&
                        allySave.D20.Value == 20);
                naturalRetry = exemptionExact && !volleyPassed && naturals;
                attempts.Add(new JObject {
                    ["attempt"] = attempt,
                    ["command"] = command == null ? null :
                        Sprint27AiCommand(command),
                    ["enemy"] = observed.Row(target),
                    ["enemySaveD20"] = targetSave?.D20.Value,
                    ["enemySavePassed"] = targetSave?.IsPassed,
                    ["ally"] = observed.Row(ally),
                    ["allySaveD20"] = allySave?.D20.Value,
                    ["allySavePassed"] = allySave?.IsPassed,
                    ["allyIsSameFactionAsCaster"] = true,
                    ["controls"] = controlRows,
                    ["rechargeHeld"] = vrock.Buffs.GetBuff(recharge) != null,
                    ["exemptionFactsExact"] = exemptionExact,
                    ["naturalTwentyAmongRequiredSaves"] = naturals,
                    ["nativeNaturalRetry"] = naturalRetry,
                    ["passed"] = volleyPassed
                });
                if (!volleyPassed && naturalRetry) continue;
                report();
                if (!commandExact) throw new InvalidOperationException(
                    "Sprint37 screech volley did not complete.");
                if (!volleyPassed) throw new InvalidOperationException(
                    "Sprint37 screech exemption volley did not hold.");

                // Refusal while the marker stands, asked of the engine
                // rather than attempted: the caster checker is what refuses,
                // so the observation is its own answer plus availability. A
                // second cast through the throwing helper would report a
                // refusal and a setup failure identically.
                var availability = new AbilityData(
                    vrock.Descriptor.Abilities.GetAbility(screech));
                var checkers = new JArray();
                bool anyCheckerRefuses = false;
                foreach (IAbilityCasterChecker checker in
                    screech.CasterCheckers ??
                    Array.Empty<IAbilityCasterChecker>())
                {
                    bool correct = checker.CorrectCaster(vrock);
                    anyCheckerRefuses |= !correct;
                    checkers.Add(new JObject {
                        ["checker"] = checker.GetType().Name,
                        ["correctCaster"] = correct
                    });
                }
                check("screech-recharge-refusal",
                    !availability.IsAvailable && anyCheckerRefuses &&
                    vrock.Buffs.GetBuff(recharge) != null,
                    new JObject {
                        ["owner"] = vrock.UniqueId,
                        ["casterCheckers"] = checkers,
                        ["anyCheckerRefuses"] = anyCheckerRefuses,
                        ["available"] = availability.IsAvailable,
                        ["marker"] = InfernalSavedEffect34(
                            vrock.Buffs.GetBuff(recharge)),
                        ["rechargeHours"] =
                            VrockRules37Policy.ScreechRechargeHours
                    }, "A second screech is refused while this vrock's own hour marker stands, and no second effect or second spend occurs.");

                // The printed one-round expiry, waited out rather than
                // cancelled.
                Buff victim = stun(target);
                double start = Game.Instance.TimeController.GameTime
                    .TotalSeconds;
                DateTime deadline = DateTime.UtcNow.AddSeconds(60);
                while (!victim.IsDisposed && DateTime.UtcNow < deadline &&
                    Game.Instance.TimeController.GameTime.TotalSeconds -
                        start < GameConsts.RoundDuration * 3)
                {
                    Game.Instance.IsPaused = false;
                    // In turn-based play the clock only moves when turns do,
                    // so the printed one-round expiry is waited out by letting
                    // the native round turn over rather than by idling.
                    if (turnBased)
                        foreach (int frame in Sprint18AdvanceRound(vrock))
                            yield return frame;
                    else yield return 0;
                }
                double elapsed = Game.Instance.TimeController.GameTime
                    .TotalSeconds - start;
                check("screech-stun-expiry", victim.IsDisposed &&
                    elapsed <= GameConsts.RoundDuration * 2.5,
                    new JObject {
                        ["target"] = target.UniqueId,
                        ["disposed"] = victim.IsDisposed,
                        ["elapsedGameSeconds"] = elapsed,
                        ["roundDuration"] = GameConsts.RoundDuration,
                        ["printedRounds"] =
                            VrockRules37Policy.ScreechStunRounds,
                        ["removedByObserver"] = false
                    }, "A failed save stuns for its printed single round and then ends on its own.");
            }
            finally { EventBus.Unsubscribe(observed); }
            }
            if (!volleyPassed)
            {
                report();
                throw new InvalidOperationException(
                    "Sprint37 screech exemption volley exhausted its bounded natural-twenty retries.");
            }
        }

        /// <summary>
        /// The save DC is live Constitution, not a baked number: the observed
        /// difficulty class is compared with the production formula at the
        /// vrock's actual Constitution, and then moved by changing that stat.
        /// </summary>
        private IEnumerable<int> VrockScreechLiveDc37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData vrock,
            BlueprintAbility screech, BlueprintBuff stunned,
            BlueprintBuff recharge, UnitEntityData target, bool turnBased,
            Action<string, bool, JObject, string> check)
        {
            VrockRequireLiveSubject37(vrock, screech, "live-dc");
            int baseline = vrock.Stats.Constitution.BaseValue;
            var rows = new JArray();
            bool coherent = true;
            try
            {
                foreach (int delta in new[] { 0, 8 })
                {
                    vrock.Stats.Constitution.BaseValue = baseline + delta;
                    foreach (int frame in VrockClearRecharge37(vrock,
                        recharge, target, stunned, turnBased))
                        yield return frame;
                    int bonus = vrock.Stats.Constitution.Bonus;
                    int expected = VrockRules37Policy
                        .ScreechDifficultyClass(bonus);
                    var observed = new Astral31SmiteEvents(vrock, screech,
                        stunned, null, new[] { target });
                    EventBus.Subscribe(observed);
                    UnitUseAbility command = null;
                    try
                    {
                        foreach (int frame in Sprint26Cast(vrock, screech,
                            new TargetWrapper(vrock),
                            value => command = value)) yield return frame;
                        RuleSavingThrow save = observed.Save(target);
                        var granted = new AbilityData(
                            vrock.Descriptor.Abilities.GetAbility(screech));
                        var parameters = granted.CalculateParams();
                        bool row = save != null &&
                            save.DifficultyClass == expected &&
                            parameters.DC == expected &&
                            parameters.CasterLevel ==
                                VrockRules37Policy.VrockHitDice &&
                            parameters.SpellLevel == 0 &&
                            save.Type == SavingThrowType.Fortitude;
                        coherent &= row;
                        rows.Add(new JObject {
                            ["constitutionBase"] =
                                vrock.Stats.Constitution.BaseValue,
                            ["constitutionBonus"] = bonus,
                            ["expectedDc"] = expected,
                            ["observedSaveDc"] = save?.DifficultyClass,
                            ["abilityParamDc"] = parameters.DC,
                            ["abilityParamCasterLevel"] =
                                parameters.CasterLevel,
                            ["abilityParamSpellLevel"] =
                                parameters.SpellLevel,
                            ["saveType"] = save?.Type.ToString(),
                            ["command"] = command == null ? null :
                                Sprint27AiCommand(command),
                            ["coherent"] = row
                        });
                    }
                    finally { EventBus.Unsubscribe(observed); }
                }
                var distinct = rows.Select(row => (int?)row["observedSaveDc"])
                    .Distinct().ToArray();
                check("screech-live-constitution-dc",
                    coherent && distinct.Length == 2, new JObject {
                        ["owner"] = vrock.UniqueId,
                        ["hitDice"] = VrockRules37Policy.VrockHitDice,
                        ["observations"] = rows,
                        ["distinctDifficultyClasses"] = distinct.Length
                    }, "The observed Fortitude difficulty class is the production Constitution formula at this vrock's live score, and it actually moves when the score changes.");
            }
            finally
            {
                vrock.Stats.Constitution.BaseValue = baseline;
            }
        }

        /// <summary>
        /// The radius boundary, measured on the native collector: one unit
        /// just inside the printed thirty feet and one just outside.
        /// </summary>
        private IEnumerable<int> VrockScreechBoundary37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData vrock,
            BlueprintAbility screech, BlueprintBuff stunned,
            BlueprintBuff recharge, UnitEntityData target,
            UnitEntityData ally, bool turnBased,
            Action<string, bool, JObject, string> check)
        {
            VrockRequireLiveSubject37(vrock, screech, "radius-boundary");
            float radius = new Feet(
                VrockRules37Policy.ScreechRadiusFeet).Meters;
            foreach (int frame in VrockClearRecharge37(vrock, recharge,
                target, stunned, turnBased)) yield return frame;
            PlaceExpandedSummoningUnit(target, vrock.Position +
                Vector3.right * (radius - 0.6f));
            PlaceExpandedSummoningUnit(ally, vrock.Position -
                Vector3.right * (radius + 2.5f));
            var observed = new Astral31SmiteEvents(vrock, screech, stunned,
                null, new[] { target, ally });
            EventBus.Subscribe(observed);
            UnitUseAbility command = null;
            try
            {
                foreach (int frame in Sprint26Cast(vrock, screech,
                    new TargetWrapper(vrock), value => command = value))
                    yield return frame;
                RuleSavingThrow insideSave = observed.Save(target);
                RuleSavingThrow outsideSave = observed.Save(ally);
                check("screech-radius-boundary",
                    insideSave != null && outsideSave == null,
                    new JObject {
                        ["owner"] = vrock.UniqueId,
                        ["radiusFeet"] =
                            VrockRules37Policy.ScreechRadiusFeet,
                        ["radiusMeters"] = radius,
                        ["insideUnit"] = target.UniqueId,
                        ["insideCentreDistance"] = Vector3.Distance(
                            vrock.Position, target.Position),
                        ["insideEyeDistance"] = Vector3.Distance(
                            vrock.EyePosition, target.Position),
                        ["insideCorpulence"] = target.Corpulence,
                        ["insideSaveRolled"] = insideSave != null,
                        ["outsideUnit"] = ally.UniqueId,
                        ["outsideCentreDistance"] = Vector3.Distance(
                            vrock.Position, ally.Position),
                        ["outsideEyeDistance"] = Vector3.Distance(
                            vrock.EyePosition, ally.Position),
                        ["outsideCorpulence"] = ally.Corpulence,
                        ["outsideSaveRolled"] = outsideSave != null,
                        ["command"] = command == null ? null :
                            Sprint27AiCommand(command),
                        ["scope"] =
                            "Measured on these two live footprints at these positions; the native collector adds only the target's own corpulence to the radius."
                    }, "A creature just inside the printed thirty-foot spread is asked to save and one beyond it is not.");
            }
            finally { EventBus.Unsubscribe(observed); }
        }

        /// <summary>
        /// Two summoned vrocks: one screeching must not spend or refuse the
        /// other's, and each victim effect belongs to its own source.
        /// </summary>
        private IEnumerable<int> VrockScreechTwoOwners37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData first,
            BlueprintAbility screech, BlueprintBuff stunned,
            BlueprintBuff recharge, UnitEntityData target,
            List<UnitEntityData> owned, bool turnBased,
            Action<string, bool, JObject, string> check)
        {
            VrockRequireLiveSubject37(first, screech, "two-owner-first");
            UnitEntityData second = CastExpandedSummoningOwnTier(fixture,
                "vrock");
            owned.Add(second);
            VrockQuiesce37(second);
            // No appearance settling for the second vrock. This cell observes
            // that one owner's hour marker, its ability availability and its
            // victim effect belong to it alone; the second vrock never casts,
            // and its body is proven by the core phase on its own unit. The
            // first combined release run stopped here with bodyPresent true,
            // bodyEnabled false and fadedIn false after 267 frames - a
            // presentation dependency this observation does not rest on, in a
            // scene already crowded by five earlier reviews. Its view state is
            // recorded below for information rather than required.
            VrockRequireLiveSubject37(second, screech, "two-owner-second");
            string secondView = ExpandedSummoningPteranodonViewPatch
                .DescribeView(second.View);
            // In turn-based play a unit outside the native turn order is not a
            // fair witness to "neither spent nor refused": its availability
            // would be answered by its absence from combat rather than by the
            // hour rule. So the second vrock joins the same combat and the
            // same order, and never casts.
            var secondOrder = new JObject();
            if (turnBased)
                foreach (int frame in VrockJoinTurnOrder37(second, target,
                    secondOrder)) yield return frame;
            foreach (int frame in VrockClearRecharge37(first, recharge,
                target, stunned, turnBased)) yield return frame;
            PlaceExpandedSummoningUnit(second, first.Position +
                Vector3.forward * (first.Corpulence + second.Corpulence +
                    1f));
            PlaceExpandedSummoningUnit(target, first.Position +
                Vector3.right * (first.Corpulence + target.Corpulence + 1f));

            // A save-or-lose observation needs a bounded natural-20 retry or
            // it is flaky by construction: a natural twenty always succeeds,
            // so one unlucky roll would fail a correct creature. The released
            // Sprint 36 burst observer solved this the same way - retry only
            // when the single reason for the miss was a natural twenty, and
            // record every attempt so an RNG retry can never be mistaken for
            // a product pass.
            const int maximumAttempts = 3;
            var attempts = new JArray();
            bool passed = false;
            for (int attempt = 1; attempt <= maximumAttempts && !passed;
                attempt++)
            {
                if (attempt > 1)
                {
                    foreach (int frame in VrockClearRecharge37(first,
                        recharge, target, stunned, turnBased))
                        yield return frame;
                }
                var observed = new Astral31SmiteEvents(first, screech,
                    stunned, null, new[] { target });
                EventBus.Subscribe(observed);
                bool naturalRetry = false;
                try
                {
                    UnitUseAbility command = null;
                    foreach (int frame in Sprint26Cast(first, screech,
                        new TargetWrapper(first), value => command = value))
                        yield return frame;
                    var firstAvailability = new AbilityData(
                        first.Descriptor.Abilities.GetAbility(screech));
                    var secondAvailability = new AbilityData(
                        second.Descriptor.Abilities.GetAbility(screech));
                    RuleSavingThrow save = observed.Save(target);
                    Buff[] fromFirst = target.Buffs.Enumerable.Where(value =>
                        ReferenceEquals(value.Blueprint, stunned) &&
                        ReferenceEquals(value.Context?.MaybeCaster, first))
                        .ToArray();
                    Buff[] fromSecond = target.Buffs.Enumerable.Where(value =>
                        ReferenceEquals(value.Blueprint, stunned) &&
                        ReferenceEquals(value.Context?.MaybeCaster, second))
                        .ToArray();
                    // Ownership is the point of this cell and is checked on
                    // every attempt regardless of the roll.
                    bool ownership = command != null && command.IsFinished &&
                        first.Buffs.GetBuff(recharge) != null &&
                        second.Buffs.GetBuff(recharge) == null &&
                        !firstAvailability.IsAvailable &&
                        secondAvailability.IsAvailable &&
                        fromSecond.Length == 0;
                    passed = ownership && fromFirst.Length == 1;
                    bool natural20 = save != null && save.IsPassed &&
                        save.D20.Value == 20;
                    naturalRetry = ownership && fromFirst.Length == 0 &&
                        natural20;
                    attempts.Add(new JObject {
                        ["attempt"] = attempt,
                        ["command"] = command == null ? null :
                            Sprint27AiCommand(command),
                        ["saveD20"] = save?.D20.Value,
                        ["saveDc"] = save?.DifficultyClass,
                        ["savePassed"] = save?.IsPassed,
                        ["naturalTwenty"] = natural20,
                        ["firstMarker"] = first.Buffs.GetBuff(recharge) ==
                            null ? null : InfernalSavedEffect34(
                                first.Buffs.GetBuff(recharge)),
                        ["secondMarker"] = second.Buffs.GetBuff(recharge) ==
                            null ? null : InfernalSavedEffect34(
                                second.Buffs.GetBuff(recharge)),
                        ["firstAvailable"] = firstAvailability.IsAvailable,
                        ["secondAvailable"] = secondAvailability.IsAvailable,
                        ["victimEffectsFromFirst"] = fromFirst.Length,
                        ["victimEffectsFromSecond"] = fromSecond.Length,
                        ["ownershipExact"] = ownership,
                        ["nativeNaturalRetry"] = naturalRetry,
                        ["passed"] = passed
                    });
                }
                finally { EventBus.Unsubscribe(observed); }
                if (!passed && !naturalRetry) break;
            }
            check("screech-two-owner-isolation", passed, new JObject {
                ["first"] = first.UniqueId,
                ["second"] = second.UniqueId,
                ["attempts"] = attempts,
                ["maximumAttempts"] = maximumAttempts,
                ["selectedAttempt"] = passed ? (int?)attempts.Count : null,
                ["secondView"] = secondView,
                ["secondViewRequired"] = false,
                ["secondTurnOrder"] = turnBased ? secondOrder : null,
                ["forcedRolls"] = 0,
                ["retryRule"] =
                    "Retried only when ownership was already exact and the sole miss was a natural twenty; no roll was forced and no assertion was relaxed."
            }, "One vrock's hour marker and victim effect belong to it alone within at most three natural attempts; a second vrock standing beside it is neither spent nor refused.");
        }

        /// <summary>
        /// Clears the recharge marker and any prior victim effect between
        /// observations. The marker is an hour long by design, so a bounded
        /// phase removes this one owner's own marker rather than waiting an
        /// in-game hour; the refusal cell above is what observes that the
        /// marker genuinely refuses while it stands.
        /// </summary>
        private IEnumerable<int> VrockClearRecharge37(UnitEntityData vrock,
            BlueprintBuff recharge, UnitEntityData target,
            BlueprintBuff stunned, bool turnBased = false)
        {
            foreach (Buff buff in vrock.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, recharge)).ToArray())
                buff.Remove();
            foreach (Buff buff in target.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, stunned)).ToArray())
                buff.Remove();
            if (turnBased)
            {
                // A spent standard action returns at the owner's next turn,
                // not after a wall-clock wait, so turn the native round over
                // instead of idling until a timeout.
                int rounds = 0;
                while (rounds++ < 4 &&
                    vrock.CombatState.Cooldown.StandardAction > 0f)
                    foreach (int frame in Sprint18AdvanceRound(vrock))
                        yield return frame;
                yield break;
            }
            DateTime deadline = DateTime.UtcNow.AddSeconds(15);
            while (vrock.CombatState.Cooldown.StandardAction > 0f &&
                DateTime.UtcNow < deadline)
            {
                Game.Instance.IsPaused = false;
                yield return 0;
            }
        }
    }
}
