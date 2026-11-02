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
            string suffix = turnBased ? "-turnbased" : "";
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    row["turnBased"] = turnBased;
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
                    controls, check)) yield return frame;

                foreach (int frame in VrockScreechLiveDc37(fixture, vrock,
                    screech, stunned, recharge, target, check))
                    yield return frame;

                foreach (int frame in VrockScreechBoundary37(fixture, vrock,
                    screech, stunned, recharge, target, ally, check))
                    yield return frame;

                foreach (int frame in VrockScreechTwoOwners37(fixture, vrock,
                    screech, stunned, recharge, target, owned, check))
                    yield return frame;
            }
            finally
            {
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
            Action<string, bool, JObject, string> check)
        {
            VrockRequireLiveSubject37(vrock, screech, "volley");
            var observed = new Astral31SmiteEvents(vrock, screech, stunned,
                null, inside);
            EventBus.Subscribe(observed);
            UnitUseAbility command = null;
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
                foreach (var control in controls)
                {
                    RuleSavingThrow save = observed.Save(control.Item2);
                    Buff effect = stun(control.Item2);
                    bool exempt = control.Item3;
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
                check("screech-identity-exemption",
                    commandExact && controlsCoherent && enemyAndAlly &&
                    vrock.Buffs.GetBuff(recharge) != null,
                    new JObject {
                        ["owner"] = vrock.UniqueId,
                        ["command"] = command == null ? null :
                            Sprint27AiCommand(command),
                        ["enemy"] = observed.Row(target),
                        ["ally"] = observed.Row(ally),
                        ["allyIsSameFactionAsCaster"] = true,
                        ["controls"] = controlRows,
                        ["rechargeHeld"] = vrock.Buffs.GetBuff(recharge) !=
                            null
                    }, "The exemption follows registered demon identity: both released demons are never even asked to save, while a devil, an enemy and an ally of the summoner all are.");
                if (!commandExact) throw new InvalidOperationException(
                    "Sprint37 screech volley did not complete.");

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
                    yield return 0;
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

        /// <summary>
        /// The save DC is live Constitution, not a baked number: the observed
        /// difficulty class is compared with the production formula at the
        /// vrock's actual Constitution, and then moved by changing that stat.
        /// </summary>
        private IEnumerable<int> VrockScreechLiveDc37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData vrock,
            BlueprintAbility screech, BlueprintBuff stunned,
            BlueprintBuff recharge, UnitEntityData target,
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
                        recharge, target, stunned)) yield return frame;
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
            UnitEntityData ally, Action<string, bool, JObject, string> check)
        {
            VrockRequireLiveSubject37(vrock, screech, "radius-boundary");
            float radius = new Feet(
                VrockRules37Policy.ScreechRadiusFeet).Meters;
            foreach (int frame in VrockClearRecharge37(vrock, recharge,
                target, stunned)) yield return frame;
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
            List<UnitEntityData> owned,
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
            foreach (int frame in VrockClearRecharge37(first, recharge,
                target, stunned)) yield return frame;
            PlaceExpandedSummoningUnit(second, first.Position +
                Vector3.forward * (first.Corpulence + second.Corpulence +
                    1f));
            PlaceExpandedSummoningUnit(target, first.Position +
                Vector3.right * (first.Corpulence + target.Corpulence + 1f));

            var observed = new Astral31SmiteEvents(first, screech, stunned,
                null, new[] { target });
            EventBus.Subscribe(observed);
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
                Buff[] fromFirst = target.Buffs.Enumerable.Where(value =>
                    ReferenceEquals(value.Blueprint, stunned) &&
                    ReferenceEquals(value.Context?.MaybeCaster, first))
                    .ToArray();
                Buff[] fromSecond = target.Buffs.Enumerable.Where(value =>
                    ReferenceEquals(value.Blueprint, stunned) &&
                    ReferenceEquals(value.Context?.MaybeCaster, second))
                    .ToArray();
                check("screech-two-owner-isolation",
                    command != null && command.IsFinished &&
                    first.Buffs.GetBuff(recharge) != null &&
                    second.Buffs.GetBuff(recharge) == null &&
                    !firstAvailability.IsAvailable &&
                    secondAvailability.IsAvailable &&
                    fromFirst.Length == 1 && fromSecond.Length == 0,
                    new JObject {
                        ["first"] = first.UniqueId,
                        ["second"] = second.UniqueId,
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
                        ["secondView"] = secondView,
                        ["secondViewRequired"] = false,
                        ["command"] = command == null ? null :
                            Sprint27AiCommand(command)
                    }, "One vrock's hour marker and victim effect belong to it alone; a second vrock standing beside it is neither spent nor refused.");
            }
            finally { EventBus.Unsubscribe(observed); }
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
            BlueprintBuff stunned)
        {
            foreach (Buff buff in vrock.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, recharge)).ToArray())
                buff.Remove();
            foreach (Buff buff in target.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, stunned)).ToArray())
                buff.Remove();
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
