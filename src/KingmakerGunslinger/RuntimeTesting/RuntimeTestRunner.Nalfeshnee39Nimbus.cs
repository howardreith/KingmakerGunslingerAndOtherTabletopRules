using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
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
        /// Unholy Nimbus on live units, driven the way a player drives it.
        ///
        /// <para>Every decisive observation here goes through a production
        /// entry point. The fixture summons units, moves them, reads state
        /// and lets game time pass; it never applies the windup, never
        /// removes it, never calls the component's cancellation marker and
        /// never resolves a burst itself. If any of those were done from
        /// here the phase would be testing the fixture.</para>
        ///
        /// <para>The one-pending rule and the delayed resolution are the two
        /// risks the charter named, and they are observed as three separate
        /// facts rather than one: the engine's own availability answer, the
        /// resource that is NOT debited, and the original deadline that is
        /// neither replaced nor extended. An implementation that refused
        /// inside the effect action after spending would pass a naive "a
        /// second cast did nothing" check and fail all three of these.</para>
        /// </summary>
        private IEnumerable<int> ReviewNalfeshneeNimbus39(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            // Each group retires its own units as it finishes - that is what
            // keeps the next group and the other combat mode starting from a
            // clean scene. This list is only the safety sweep for a group
            // that threw before reaching its own disposal.
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = turnBased
                    ? ExpandedSummoningNativeProfilePolicy
                        .Sprint39NalfeshneeTurnsPhase
                    : ExpandedSummoningNativeProfilePolicy
                        .Sprint39NalfeshneeNimbusPhase,
                ["turnBased"] = turnBased,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData victim = fixture.Hostile;
            UnitEntityData ally = fixture.Caster;
            int oldVictimWill = victim.Stats.SaveWill.BaseValue;
            int oldVictimHp = victim.Stats.HitPoints.BaseValue;
            int oldAllyWill = ally.Stats.SaveWill.BaseValue;
            int oldAllyHp = ally.Stats.HitPoints.BaseValue;
            Vector3 oldVictimPosition = victim.Position;
            Vector3 oldAllyPosition = ally.Position;
            bool oldMode = Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                .EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    // The engine's own answer, on every cell, so a
                    // "-turnbased" name can never stand for a mode that was
                    // never actually entered.
                    row["nativeTurnBasedMode"] = TurnBased.Controllers
                        .CombatController.IsInTurnBasedCombat();
                    cells.Add(row);
                    Sprint25Check("sprint39-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint39 nalfeshnee nimbus cell failed: " + id);
                };
            try
            {
                BlueprintAbility nimbus = NimbusAbility39();
                BlueprintBuff windup = NimbusWindup39();
                BlueprintBuff daze = NimbusDaze39();
                BlueprintAbilityResource resource = NimbusResource39();

                // The recipients are set up to fail honestly rather than
                // being forced after the save is rolled.
                victim.Stats.SaveWill.BaseValue = -100;
                victim.Stats.HitPoints.BaseValue = 100000;
                ally.Stats.SaveWill.BaseValue = -100;
                ally.Stats.HitPoints.BaseValue = 100000;

                // ONE MODE PER PASS, and every group disposes its own units
                // before the next begins. The released turns reviews are one
                // reviewer called twice for exactly this reason: a turn-based
                // pass that inherits a scene full of still-living summons
                // from earlier groups puts the installed turn-based combat
                // mod into a state no released phase creates, and it threw
                // inside its own StartTurn from the harness clock when this
                // phase first tried to carry both modes and ten owned units
                // through one transaction.
                if (turnBased)
                {
                    foreach (int frame in NimbusCycle39(fixture, owned,
                        nimbus, windup, daze, resource, victim, ally, true,
                        check)) yield return frame;
                }
                else
                {
                    foreach (int frame in NimbusInterface39(fixture, owned,
                        nimbus, windup, resource, check)) yield return frame;
                    foreach (int frame in NimbusCycle39(fixture, owned,
                        nimbus, windup, daze, resource, victim, ally, false,
                        check)) yield return frame;
                    foreach (int frame in NimbusExhaustion39(fixture, owned,
                        nimbus, windup, daze, resource, victim, check))
                        yield return frame;
                    foreach (int frame in NimbusTwoOwners39(fixture, owned,
                        nimbus, windup, daze, resource, victim, check))
                        yield return frame;
                    foreach (int frame in NimbusDestroyedSource39(fixture,
                        nimbus, windup, daze, victim, check))
                        yield return frame;
                }
            }
            finally
            {
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                    .EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
                receipt["nativeModeRestored"] = TurnBased.Controllers
                    .CombatController.IsInTurnBasedCombat() == oldMode;
                foreach (Buff buff in new[] { victim, ally }
                    .SelectMany(unit => unit.Buffs.Enumerable).ToArray())
                    if (buff.Blueprint != null && buff.Blueprint.name
                        .StartsWith("KMG_", StringComparison.Ordinal))
                        buff.Remove();
                victim.Stats.SaveWill.BaseValue = oldVictimWill;
                victim.Stats.HitPoints.BaseValue = oldVictimHp;
                ally.Stats.SaveWill.BaseValue = oldAllyWill;
                ally.Stats.HitPoints.BaseValue = oldAllyHp;
                PlaceExpandedSummoningUnit(victim, oldVictimPosition);
                PlaceExpandedSummoningUnit(ally, oldAllyPosition);
                // Normally a no-op: every group retired its own units, so
                // this only catches a group that threw before it could.
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.Where(unit => unit != null && !unit.Destroyed)
                        .ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory, turnBased
                        ? "sprint39-nalfeshnee-turns.json"
                        : "sprint39-nalfeshnee-nimbus.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        /// <summary>
        /// The ability as the action bar sees it, before anything is spent.
        ///
        /// <para>A claim that the player can reach this is only honest if the
        /// ability is actually granted, active, visible and not excluded from
        /// auto-fill. The classification is checked here too: supernatural
        /// rather than spell-like is not cosmetic, because the two take their
        /// difficulty from different places and only one of them is subject
        /// to spell resistance.</para>
        /// </summary>
        private IEnumerable<int> NimbusInterface39(
            ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> owned, BlueprintAbility nimbus,
            BlueprintBuff windup, BlueprintAbilityResource resource,
            Action<string, bool, JObject, string> check)
        {
            var mine = new List<UnitEntityData>();
            UnitEntityData owner = CastExpandedSummoningOwnTier(
                fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
            owned.Add(owner);
            mine.Add(owner);
            VrockQuiesce37(owner);
            foreach (int frame in Outsider30NativeAppearance(owner,
                owner.Blueprint.name)) yield return frame;

            Ability granted = owner.Descriptor.Abilities.GetAbility(nimbus);
            AbilityData manual = granted == null ? null :
                new AbilityData(granted);
            int uses = owner.Descriptor.Resources.GetResourceAmount(resource);
            AbilityParams parameters = manual == null ? null :
                manual.CalculateParams();
            int expectedDc = NalfeshneeRules39Policy
                .SupernaturalDifficultyClass(
                    NalfeshneeRules39Policy.NalfeshneeHitDice,
                    owner.Descriptor.Stats.Charisma.Bonus);
            check("nalfeshnee-nimbus-interface",
                granted != null && granted.Active && !nimbus.Hidden &&
                    !nimbus.ActionBarAutoFillIgnored &&
                    nimbus.Type == AbilityType.Supernatural &&
                    nimbus.ActionType == UnitCommand.CommandType.Free &&
                    !nimbus.IsFullRoundAction &&
                    nimbus.Range == AbilityRange.Personal &&
                    nimbus.CanTargetSelf && !nimbus.CanTargetEnemies &&
                    manual != null && manual.IsAvailable &&
                    uses == NalfeshneeRules39Policy.NimbusUsesPerDay &&
                    parameters != null && parameters.DC == expectedDc &&
                    owner.Descriptor.Buffs.GetBuff(windup) == null,
                new JObject {
                    ["owner"] = owner.UniqueId,
                    ["granted"] = granted != null,
                    ["factActive"] = granted != null && granted.Active,
                    ["hiddenFromUi"] = nimbus.Hidden,
                    ["actionBarAutoFillIgnored"] =
                        nimbus.ActionBarAutoFillIgnored,
                    ["abilityType"] = nimbus.Type.ToString(),
                    ["actionType"] = nimbus.ActionType.ToString(),
                    ["fullRoundAction"] = nimbus.IsFullRoundAction,
                    ["range"] = nimbus.Range.ToString(),
                    ["canTargetSelf"] = nimbus.CanTargetSelf,
                    ["canTargetEnemies"] = nimbus.CanTargetEnemies,
                    ["availableBeforeUse"] = manual == null ? null :
                        (bool?)manual.IsAvailable,
                    ["usesAvailable"] = uses,
                    ["printedUsesPerDay"] =
                        NalfeshneeRules39Policy.NimbusUsesPerDay,
                    ["charismaBonus"] = owner.Descriptor.Stats.Charisma.Bonus,
                    ["expectedSupernaturalDc"] = expectedDc,
                    ["printedDc"] = NalfeshneeRules39Policy.PrintedNimbusDc,
                    ["observedParamDc"] = parameters == null ? null :
                        (int?)parameters.DC,
                    // Recorded, NOT asserted, and the distinction is the
                    // engine's. ContextCalculateAbilityParams.Calculate takes
                    // a MechanicsContext and triggers its own
                    // RuleCalculateAbilityParams with the replacement caster
                    // level, so it can only run once a cast has a context.
                    // AbilityData.CalculateParams is the PRE-CAST estimate on
                    // a different path and reports the creature's own level -
                    // fourteen here - however the ability declares its
                    // override. The printed twelve is therefore observed
                    // in-context on the pending windup below, where it is
                    // actually the number the rules use.
                    ["preCastParamCasterLevel"] = parameters == null ? null :
                        (int?)parameters.CasterLevel,
                    ["preCastCasterLevelIsTheCreaturesOwn"] = true,
                    ["printedSpellLikeCasterLevel"] =
                        NalfeshneeRules39Policy.SpellLikeCasterLevel,
                    ["pendingBeforeUse"] =
                        owner.Descriptor.Buffs.GetBuff(windup) != null,
                    ["scope"] = "The cycle cell below issues the same UnitUseAbility command the action bar issues; this cell establishes only that the bar can reach it."
                },
                "Unholy Nimbus is granted, active and reachable by the player's own free action, is declared supernatural rather than spell-like, carries its three printed daily uses and takes its difficulty from half the hit dice plus live Charisma.");
            // Retired here rather than at the end of the phase, so the
            // next group and the other combat mode see a clean scene.
            DisposeExpandedSummoningUnits(fixture.Created, mine.ToArray());
        }

        /// <summary>
        /// One complete cycle in one mode: a real activation, the refusal
        /// while it is pending, and the burst a round later with real saves,
        /// real dice and the printed demon exemption - plus the recipient
        /// boundary, which is decided at the burst and not at activation.
        /// </summary>
        private IEnumerable<int> NimbusCycle39(
            ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> owned, BlueprintAbility nimbus,
            BlueprintBuff windup, BlueprintBuff daze,
            BlueprintAbilityResource resource, UnitEntityData victim,
            UnitEntityData ally, bool turnBased,
            Action<string, bool, JObject, string> check)
        {
            string suffix = turnBased ? "-turnbased" : "";
            var mine = new List<UnitEntityData>();
            UnitEntityData owner = CastExpandedSummoningOwnTier(
                fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
            owned.Add(owner);
            mine.Add(owner);
            VrockQuiesce37(owner);
            foreach (int frame in Outsider30NativeAppearance(owner,
                owner.Blueprint.name)) yield return frame;
            foreach (int frame in NimbusWaitDazeOut39(victim, daze))
                yield return frame;
            foreach (int frame in NimbusWaitDazeOut39(ally, daze))
                yield return frame;

            // Real controls, each a registered creature rather than a flag:
            // one released demon that the printed exemption must spare, one
            // released devil that it must NOT spare, and a late arrival that
            // is outside the radius when the windup starts.
            UnitEntityData exemptDemon = CastExpandedSummoningOwnTier(
                fixture, "succubus");
            UnitEntityData devil = CastExpandedSummoningOwnTier(
                fixture, "bone-devil");
            UnitEntityData latecomer = CastExpandedSummoningOwnTier(
                fixture, "bone-devil");
            foreach (UnitEntityData control in
                new[] { exemptDemon, devil, latecomer })
            {
                owned.Add(control);
                mine.Add(control);
                VrockQuiesce37(control);
                control.Stats.SaveWill.BaseValue = -100;
                foreach (int frame in WaitInfernalSupport34(control))
                    yield return frame;
            }

            if (turnBased)
            {
                var admission = new JObject();
                foreach (int frame in VrockEnterTurnBased37(owner, victim,
                    admission)) yield return frame;
                check("nalfeshnee-nimbus-turn-admission" + suffix,
                    (bool)admission["admitted"] &&
                        (bool)admission["ownerActing"] &&
                        (bool)admission["nativeTurnBasedMode"], admission,
                    "The nalfeshnee is enrolled in native turn-based combat through the engine's own enrollment and turn-order toggle, and holds its own turn before any nimbus is ordered.");
                // Deliberately nothing else is enrolled. Only the OWNER has
                // to act here, and the released turns phase enrols exactly
                // one extra unit - a second caster that needs its own turn.
                // Recipients do not: a save and a condition are unit-agnostic
                // and need no initiative, and the burst comes from the
                // owner's own buff expiry rather than from anyone's turn.
                //
                // The first turn-based attempt did enrol all four recipients,
                // the party caster among them, and the installed turn-based
                // combat mod then threw inside its own StartTurn from the
                // harness clock. That is outside the shape every released
                // turns phase uses, and enrolling a party member in combat
                // with a hostile summon is a state change this observation
                // never needed, so the fix is to stop asking for it rather
                // than to work around another mod.
            }

            // Inside the printed radius at activation: the enemy, the ALLY -
            // collateral is retained rather than faction-filtered - the
            // exempt demon and the devil. The latecomer is well outside.
            VrockRing37(owner, new[] { victim, ally, exemptDemon, devil },
                NalfeshneeRules39Policy.NimbusRadiusFeet - 15f);
            VrockRing37(owner, new[] { latecomer },
                NalfeshneeRules39Policy.NimbusRadiusFeet * 3f);

            var watcher = new NalfeshneeNimbusWatcher39(owner, nimbus, daze);
            EventBus.Subscribe(watcher);
            try
            {
                int before = owner.Descriptor.Resources
                    .GetResourceAmount(resource);
                JObject readiness = VrockCastReadiness37(owner, nimbus,
                    new TargetWrapper(owner));
                UnitUseAbility command = null;
                double startTime = Game.Instance.TimeController.GameTime
                    .TotalSeconds;
                foreach (int frame in Sprint26Cast(owner, nimbus,
                    new TargetWrapper(owner), value => command = value))
                    yield return frame;
                Buff pending = owner.Descriptor.Buffs.GetBuff(windup);
                int afterFirst = owner.Descriptor.Resources
                    .GetResourceAmount(resource);
                TimeSpan deadline = pending == null ? TimeSpan.Zero :
                    pending.EndTime;
                double remaining = pending == null ? 0 :
                    pending.TimeLeft.TotalSeconds;
                int pendingCount = owner.Descriptor.Buffs.Enumerable
                    .Count(value => ReferenceEquals(value.Blueprint, windup));
                check("nalfeshnee-nimbus-activation" + suffix,
                    command != null && command.IsFinished &&
                        command.Result == UnitCommand.ResultType.Success &&
                        pending != null && pendingCount == 1 &&
                        afterFirst == before - 1 && remaining > 0 &&
                        remaining <= GameConsts.RoundDuration * 1.5 &&
                        pending.Context != null &&
                        pending.Context.Params.CasterLevel ==
                            NalfeshneeRules39Policy.SpellLikeCasterLevel &&
                        watcher.Saves.Count == 0 &&
                        watcher.DazeApplications.Count == 0,
                    new JObject {
                        ["owner"] = owner.UniqueId,
                        ["readiness"] = readiness,
                        ["commandResult"] = command == null ? null :
                            command.Result.ToString(),
                        ["commandFinished"] = command != null &&
                            command.IsFinished,
                        ["pendingCreated"] = pending != null,
                        ["pendingCount"] = pendingCount,
                        ["usesBefore"] = before,
                        ["usesAfter"] = afterFirst,
                        ["deadlineGameSeconds"] = deadline.TotalSeconds,
                        ["remainingGameSeconds"] = remaining,
                        ["printedDelayRounds"] =
                            NalfeshneeRules39Policy.NimbusDelayRounds,
                        ["roundDuration"] = GameConsts.RoundDuration,
                        ["savesAtActivation"] = watcher.Saves.Count,
                        ["dazesAtActivation"] = watcher.DazeApplications.Count,
                        ["activationGameTime"] = startTime,
                        // The in-context caster level, which is the number the
                        // printed ranges and the greater dispel cap read. The
                        // pre-cast estimate in the interface cell is a
                        // different quantity on a different path.
                        ["inContextCasterLevel"] = pending == null ||
                            pending.Context == null ? null :
                            (int?)pending.Context.Params.CasterLevel,
                        ["printedSpellLikeCasterLevel"] =
                            NalfeshneeRules39Policy.SpellLikeCasterLevel
                    },
                    "One legal free action succeeds, spends exactly one use, creates exactly one pending windup carrying an absolute deadline about one round out, and resolves nothing yet - no save is rolled and nobody is dazed at activation.");

                // THE REFUSAL, asked of the engine rather than attempted. The
                // caster checker is what refuses, so what is recorded is its
                // own answer plus availability plus the untouched resource; a
                // second cast through the throwing helper would report a
                // correct refusal and a setup failure identically.
                var availability = new AbilityData(
                    owner.Descriptor.Abilities.GetAbility(nimbus));
                var checkers = new JArray();
                bool anyRefuses = false;
                foreach (IAbilityCasterChecker checker in
                    nimbus.CasterCheckers ??
                    Array.Empty<IAbilityCasterChecker>())
                {
                    bool correct = checker.CorrectCaster(owner);
                    anyRefuses |= !correct;
                    checkers.Add(new JObject {
                        ["checker"] = checker.GetType().Name,
                        ["correctCaster"] = correct });
                }
                Buff stillPending = owner.Descriptor.Buffs.GetBuff(windup);
                int afterRefusal = owner.Descriptor.Resources
                    .GetResourceAmount(resource);
                check("nalfeshnee-nimbus-one-pending" + suffix,
                    !availability.IsAvailable && anyRefuses &&
                        afterRefusal == afterFirst && stillPending != null &&
                        ReferenceEquals(stillPending, pending) &&
                        stillPending.EndTime == deadline &&
                        owner.Descriptor.Buffs.Enumerable.Count(value =>
                            ReferenceEquals(value.Blueprint, windup)) == 1,
                    new JObject {
                        ["owner"] = owner.UniqueId,
                        ["casterCheckers"] = checkers,
                        ["anyCheckerRefuses"] = anyRefuses,
                        ["availableWhilePending"] = availability.IsAvailable,
                        ["maxPendingPerOwner"] = NalfeshneeRules39Policy
                            .NimbusMaxPendingPerOwner,
                        ["usesBeforeSecondAttempt"] = afterFirst,
                        ["usesAfterSecondAttempt"] = afterRefusal,
                        ["deadlineBefore"] = deadline.TotalSeconds,
                        ["deadlineAfter"] = stillPending == null ? 0 :
                            stillPending.EndTime.TotalSeconds,
                        ["sameWindupInstance"] =
                            ReferenceEquals(stillPending, pending),
                        ["pendingCount"] = owner.Descriptor.Buffs.Enumerable
                            .Count(value => ReferenceEquals(value.Blueprint,
                                windup))
                    },
                    "While one windup is pending the ability is unavailable in the engine's own availability check and its caster checker is what refuses, so a second activation spends nothing and the original deadline is neither replaced, extended nor duplicated.");

                // The recipient boundary is decided at the burst, not at
                // activation: the devil walks out and the latecomer walks in
                // while the windup is still pending.
                VrockRing37(owner, new[] { devil },
                    NalfeshneeRules39Policy.NimbusRadiusFeet * 3f);
                VrockRing37(owner, new[] { latecomer },
                    NalfeshneeRules39Policy.NimbusRadiusFeet - 15f);

                // The deadline is waited out rather than cancelled. In
                // turn-based play the clock only moves when turns do.
                DateTime wall = DateTime.UtcNow.AddSeconds(150);
                while (owner.Descriptor.Buffs.GetBuff(windup) != null &&
                    DateTime.UtcNow < wall &&
                    Game.Instance.TimeController.GameTime.TotalSeconds -
                        startTime < GameConsts.RoundDuration * 4)
                {
                    Game.Instance.IsPaused = false;
                    if (turnBased)
                        foreach (int frame in Sprint18AdvanceRound(owner))
                            yield return frame;
                    else yield return 0;
                }
                double elapsed = Game.Instance.TimeController.GameTime
                    .TotalSeconds - startTime;

                RuleSavingThrow victimSave = watcher.Save(victim);
                RuleSavingThrow allySave = watcher.Save(ally);
                RuleSavingThrow latecomerSave = watcher.Save(latecomer);
                Buff victimDaze = victim.Descriptor.Buffs.GetBuff(daze);
                Buff allyDaze = ally.Descriptor.Buffs.GetBuff(daze);
                Buff latecomerDaze = latecomer.Descriptor.Buffs.GetBuff(daze);
                // WAS CAUGHT, which is a different fact from IS STILL DAZED.
                // The printed duration is a 1d10 roll, so a creature that
                // rolled one round can legitimately be clear again by the
                // time the burst's own round advance returns - and in
                // turn-based play the advance IS how the deadline is reached,
                // so the two readings can never be made simultaneous. The
                // first turn-based run failed exactly there: the latecomer's
                // save was rolled and its daze applied, and it had expired on
                // its own clock a round later.
                //
                // So what the burst DID is read from the applications the
                // burst itself made, and the live condition is kept beside it
                // as supporting evidence. For the spared creatures the
                // recorded absence is the stronger claim of the two: nothing
                // was ever applied to them, not merely nothing remains.
                Func<UnitEntityData, bool> caught = unit =>
                    watcher.DazeApplications.Any(value =>
                        ReferenceEquals(value.Initiator, unit) &&
                        value.AppliedBuff != null);
                // THE PRINTED RULE IS "SAVE OR BE DAZED", and that is what
                // these cells assert: every non-demon in the radius had a
                // real Will save rolled at the right difficulty, and every
                // one that FAILED was caught. Demanding that a particular
                // recipient be caught additionally requires the die to
                // cooperate, and a natural twenty is an automatic success on
                // a saving throw however far a bonus is driven down - the
                // first grouped release run lost its feeblemind cell to
                // exactly that. With three recipients a burst would flake
                // about one run in seven.
                //
                // The implication alone could be satisfied vacuously by three
                // automatic successes, so at least one recipient must have
                // actually failed and been dazed. That is one chance in eight
                // thousand of needing a retry rather than one in seven.
                Func<UnitEntityData, bool> savedAndConsistent = unit => {
                    RuleSavingThrow roll = watcher.Save(unit);
                    return roll != null && roll.Type == SavingThrowType.Will &&
                        roll.DifficultyClass == NalfeshneeRules39Policy
                            .SupernaturalDifficultyClass(
                                NalfeshneeRules39Policy.NalfeshneeHitDice,
                                owner.Descriptor.Stats.Charisma.Bonus) &&
                        (roll.IsPassed || caught(unit));
                };
                int expectedDc = NalfeshneeRules39Policy
                    .SupernaturalDifficultyClass(
                        NalfeshneeRules39Policy.NalfeshneeHitDice,
                        owner.Descriptor.Stats.Charisma.Bonus);
                check("nalfeshnee-nimbus-delayed-burst" + suffix,
                    owner.Descriptor.Buffs.GetBuff(windup) == null &&
                        elapsed >= GameConsts.RoundDuration * 0.5 &&
                        savedAndConsistent(victim) &&
                        savedAndConsistent(ally) &&
                        (caught(victim) || caught(ally)),
                    new JObject {
                        ["owner"] = owner.UniqueId,
                        ["pendingAfter"] =
                            owner.Descriptor.Buffs.GetBuff(windup) != null,
                        ["elapsedGameSeconds"] = elapsed,
                        ["roundDuration"] = GameConsts.RoundDuration,
                        ["expectedDc"] = expectedDc,
                        ["victim"] = watcher.Row(victim, daze),
                        ["ally"] = watcher.Row(ally, daze),
                        ["radiusFeet"] =
                            NalfeshneeRules39Policy.NimbusRadiusFeet,
                        // The printed rule catches any non-demon in the area,
                        // and that is kept as collateral rather than quietly
                        // made faction-safe - which would be a different
                        // ability.
                        ["allySaveRolled"] = allySave != null,
                        ["victimSaveD20"] = victimSave == null ? null :
                            (int?)victimSave.D20.Value,
                        ["allySaveD20"] = allySave == null ? null :
                            (int?)allySave.D20.Value,
                        ["victimCaughtByTheBurst"] = caught(victim),
                        ["allyCaughtByTheBurst"] = caught(ally),
                        ["atLeastOneRecipientActuallyDazed"] =
                            caught(victim) || caught(ally),
                        ["naturalTwentyIsAnAutomaticSave"] = true,
                        ["victimStillDazedWhenRead"] = victimDaze != null,
                        ["allyStillDazedWhenRead"] = allyDaze != null,
                        ["collateralRetained"] = allySave != null &&
                            caught(ally),
                        ["removedByObserver"] = false
                    },
                    "The windup resolves on its own deadline about a round after activation: real Will saves at the live Charisma-derived difficulty are rolled for BOTH the enemy and the summoner's own ally in the radius, every one of them that fails is dazed, and at least one actually is - so the printed save-or-be-dazed rule holds and the collateral is retained rather than filtered to enemies.");

                // Whichever recipient the burst actually caught, for the same
                // reason the cells above assert the implication: the victim
                // may legitimately have rolled an automatic save while the
                // ally failed, and the printed duration is a property of the
                // daze that landed rather than of any one recipient.
                RuleApplyBuff applied = watcher.DazeApplications
                    .FirstOrDefault(value => value.AppliedBuff != null);
                UnitEntityData dazed = applied == null ? victim :
                    applied.Initiator;
                RuleRollDice rolled = watcher.Roll(dazed);
                double expectedSeconds = rolled == null ? -1 :
                    GameConsts.RoundDuration * rolled.Result;
                check("nalfeshnee-nimbus-rolled-duration" + suffix,
                    rolled != null && applied != null &&
                        (int)rolled.DiceFormula.Dice ==
                            NalfeshneeRules39Policy.NimbusDazeDieSides &&
                        rolled.DiceFormula.Rolls ==
                            NalfeshneeRules39Policy.NimbusDazeDiceCount &&
                        rolled.Result >= 1 &&
                        rolled.Result <= NalfeshneeRules39Policy
                            .NimbusDazeDieSides &&
                        applied.Duration.HasValue &&
                        Math.Abs(applied.Duration.Value.TotalSeconds -
                            expectedSeconds) < 0.001,
                    new JObject {
                        ["diceCount"] = rolled == null ? null :
                            (int?)rolled.DiceFormula.Rolls,
                        ["dieSides"] = rolled == null ? null :
                            (int?)(int)rolled.DiceFormula.Dice,
                        ["printedDice"] =
                            NalfeshneeRules39Policy.NimbusDazeDiceCount + "d" +
                            NalfeshneeRules39Policy.NimbusDazeDieSides,
                        ["rolledRounds"] = rolled == null ? null :
                            (int?)rolled.Result,
                        ["appliedSeconds"] = applied == null ||
                            !applied.Duration.HasValue ? null :
                            (double?)applied.Duration.Value.TotalSeconds,
                        ["expectedSeconds"] = expectedSeconds,
                        ["measuredOnRecipient"] = dazed.UniqueId,
                        ["measuredOnTheRecipientTheBurstCaught"] =
                            applied != null
                    },
                    "The daze lasts a natively rolled one-to-ten rounds taken from the engine's own dice rather than a fixed number, and the duration actually applied is exactly that roll in rounds.");

                check("nalfeshnee-nimbus-recipients-at-burst" + suffix,
                    savedAndConsistent(latecomer) &&
                        watcher.Save(devil) == null && !caught(devil) &&
                        devil.Descriptor.Buffs.GetBuff(daze) == null &&
                        watcher.Save(exemptDemon) == null &&
                        !caught(exemptDemon) &&
                        exemptDemon.Descriptor.Buffs.GetBuff(daze) == null &&
                        SummonDemonIdentityCatalog.IsDemonKey("succubus") &&
                        !SummonDemonIdentityCatalog.IsDemonKey("bone-devil"),
                    new JObject {
                        ["latecomerKey"] = "bone-devil",
                        ["latecomerOutsideAtActivation"] = true,
                        ["latecomerInsideAtBurst"] = true,
                        ["latecomerSaveRolled"] = latecomerSave != null,
                        ["latecomerSaveD20"] = latecomerSave == null ? null :
                            (int?)latecomerSave.D20.Value,
                        ["latecomerCaughtByTheBurst"] = caught(latecomer),
                        ["latecomerStillDazedWhenRead"] =
                            latecomerDaze != null,
                        ["departedKey"] = "bone-devil",
                        ["departedInsideAtActivation"] = true,
                        ["departedOutsideAtBurst"] = true,
                        ["departedSaveRolled"] = watcher.Save(devil) != null,
                        ["departedCaughtByTheBurst"] = caught(devil),
                        ["departedDazed"] = devil.Descriptor.Buffs
                            .GetBuff(daze) != null,
                        ["exemptDemonKey"] = "succubus",
                        ["exemptDemonInsideAtBurst"] = true,
                        ["exemptDemonSaveRolled"] =
                            watcher.Save(exemptDemon) != null,
                        ["exemptDemonCaughtByTheBurst"] =
                            caught(exemptDemon),
                        ["exemptDemonDazed"] = exemptDemon.Descriptor.Buffs
                            .GetBuff(daze) != null,
                        ["succubusIsDemon"] =
                            SummonDemonIdentityCatalog.IsDemonKey("succubus"),
                        ["boneDevilIsDemon"] = SummonDemonIdentityCatalog
                            .IsDemonKey("bone-devil"),
                        ["authoritativeDemonCount"] =
                            SummonDemonIdentityCatalog.UnitSymbols().Count()
                    },
                    "Recipients are chosen at the burst from the world as it then is: a creature that walked in during the windup has a real save rolled for it and is dazed unless it saves, and one that walked out has none rolled at all, while a released demon standing inside is spared and a released devil inside is not - so the exemption is a subtype identity rather than an outsider, alignment or faction rule.");
            }
            finally
            {
                EventBus.Unsubscribe(watcher);
            // Retired here rather than at the end of the phase, so the
            // next group and the other combat mode see a clean scene.
            DisposeExpandedSummoningUnits(fixture.Created, mine.ToArray());
            }
        }

        /// <summary>
        /// Limited uses, spent one real activation at a time. Each cycle must
        /// resolve before the next may begin, which is the one-pending rule
        /// doing its work rather than a cooldown - and the decisive detail is
        /// WHY the fourth attempt is refused: the resource is empty, while
        /// the pending-buff caster checker is satisfied. A build that blocked
        /// here for the wrong reason would look identical on availability
        /// alone.
        /// </summary>
        private IEnumerable<int> NimbusExhaustion39(
            ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> owned, BlueprintAbility nimbus,
            BlueprintBuff windup, BlueprintBuff daze,
            BlueprintAbilityResource resource, UnitEntityData victim,
            Action<string, bool, JObject, string> check)
        {
            var mine = new List<UnitEntityData>();
            UnitEntityData owner = CastExpandedSummoningOwnTier(
                fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
            owned.Add(owner);
            mine.Add(owner);
            VrockQuiesce37(owner);
            foreach (int frame in Outsider30NativeAppearance(owner,
                owner.Blueprint.name)) yield return frame;
            foreach (int frame in NimbusWaitDazeOut39(victim, daze))
                yield return frame;
            VrockRing37(owner, new[] { victim },
                NalfeshneeRules39Policy.NimbusRadiusFeet - 15f);

            var rows = new JArray();
            bool coherent = true;
            for (int use = 1; use <= NalfeshneeRules39Policy.NimbusUsesPerDay;
                use++)
            {
                int before = owner.Descriptor.Resources
                    .GetResourceAmount(resource);
                bool availableBefore = new AbilityData(owner.Descriptor
                    .Abilities.GetAbility(nimbus)).IsAvailable;
                UnitUseAbility command = null;
                double start = Game.Instance.TimeController.GameTime
                    .TotalSeconds;
                foreach (int frame in Sprint26Cast(owner, nimbus,
                    new TargetWrapper(owner), value => command = value))
                    yield return frame;
                int after = owner.Descriptor.Resources
                    .GetResourceAmount(resource);
                bool pending = owner.Descriptor.Buffs.GetBuff(windup) != null;
                DateTime wall = DateTime.UtcNow.AddSeconds(120);
                while (owner.Descriptor.Buffs.GetBuff(windup) != null &&
                    DateTime.UtcNow < wall &&
                    Game.Instance.TimeController.GameTime.TotalSeconds -
                        start < GameConsts.RoundDuration * 4)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                bool resolved = owner.Descriptor.Buffs.GetBuff(windup) == null;
                bool row = availableBefore &&
                    before == NalfeshneeRules39Policy.NimbusUsesPerDay -
                        (use - 1) &&
                    after == before - 1 && pending && resolved &&
                    command != null &&
                    command.Result == UnitCommand.ResultType.Success;
                coherent &= row;
                rows.Add(new JObject {
                    ["use"] = use,
                    ["availableBefore"] = availableBefore,
                    ["usesBefore"] = before,
                    ["usesAfter"] = after,
                    ["pendingCreated"] = pending,
                    ["resolvedBeforeNext"] = resolved,
                    ["commandResult"] = command == null ? null :
                        command.Result.ToString(),
                    ["coherent"] = row });
                // Deliberately no daze cleanup between uses. This cell reads
                // only the owner's resource, availability and pending state,
                // and a dazed creature still rolls its own saves - so
                // stripping or waiting out the witness's condition here would
                // buy nothing and cost three more expiries.
            }
            int remaining = owner.Descriptor.Resources
                .GetResourceAmount(resource);
            var exhausted = new AbilityData(
                owner.Descriptor.Abilities.GetAbility(nimbus));
            var checkers = new JArray();
            bool anyCheckerRefuses = false;
            foreach (IAbilityCasterChecker checker in nimbus.CasterCheckers ??
                Array.Empty<IAbilityCasterChecker>())
            {
                bool correct = checker.CorrectCaster(owner);
                anyCheckerRefuses |= !correct;
                checkers.Add(new JObject {
                    ["checker"] = checker.GetType().Name,
                    ["correctCaster"] = correct });
            }
            check("nalfeshnee-nimbus-uses-exhausted",
                coherent && remaining == 0 && !exhausted.IsAvailable &&
                    !anyCheckerRefuses &&
                    owner.Descriptor.Buffs.GetBuff(windup) == null,
                new JObject {
                    ["owner"] = owner.UniqueId,
                    ["uses"] = rows,
                    ["printedUsesPerDay"] =
                        NalfeshneeRules39Policy.NimbusUsesPerDay,
                    ["remaining"] = remaining,
                    ["availableAtZero"] = exhausted.IsAvailable,
                    ["casterCheckers"] = checkers,
                    ["anyCheckerRefuses"] = anyCheckerRefuses,
                    ["pendingAtZero"] =
                        owner.Descriptor.Buffs.GetBuff(windup) != null
                },
                "All three printed daily uses are spent by three real activations, each resolving before the next may begin, and the fourth is refused by the empty resource rather than by the pending-buff restriction, which is satisfied at that moment.");
            // Retired here rather than at the end of the phase, so the
            // next group and the other combat mode see a clean scene.
            DisposeExpandedSummoningUnits(fixture.Created, mine.ToArray());
        }

        /// <summary>
        /// Two owners, each holding its own windup. One resolving must leave
        /// the other's deadline, resource and pending state exactly as they
        /// were, and the other must still burst on its own later deadline.
        ///
        /// <para>Per-fact component state is what makes that possible - each
        /// fact carries its own instantiated copy of the component - and this
        /// is where a shared static would show, as one burst resolving both
        /// or cancelling the second.</para>
        /// </summary>
        private IEnumerable<int> NimbusTwoOwners39(
            ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> owned, BlueprintAbility nimbus,
            BlueprintBuff windup, BlueprintBuff daze,
            BlueprintAbilityResource resource, UnitEntityData victim,
            Action<string, bool, JObject, string> check)
        {
            UnitEntityData first = CastExpandedSummoningOwnTier(
                fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
            UnitEntityData second = CastExpandedSummoningOwnTier(
                fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
            var mine = new List<UnitEntityData> { first, second };
            owned.Add(first);
            owned.Add(second);
            foreach (UnitEntityData unit in new[] { first, second })
            {
                VrockQuiesce37(unit);
                foreach (int frame in Outsider30NativeAppearance(unit,
                    unit.Blueprint.name)) yield return frame;
            }
            foreach (int frame in NimbusWaitDazeOut39(victim, daze))
                yield return frame;
            // EACH OWNER GETS ITS OWN WITNESS, and the two are far enough
            // apart that neither is in the other's radius. That is what makes
            // the two bursts distinguishable: a shared recipient would leave
            // "the second one resolved" indistinguishable from "the first one
            // caught everybody".
            //
            // The first version placed the second owner far away and gave it
            // no witness at all, and it passed only because earlier groups
            // left their summons standing nearby. Retiring each group's units
            // promptly removed that accident and the cell failed honestly:
            // the burst resolved and caught nobody, because nobody was there.
            UnitEntityData secondWitness = CastExpandedSummoningOwnTier(
                fixture, "bone-devil");
            owned.Add(secondWitness);
            mine.Add(secondWitness);
            VrockQuiesce37(secondWitness);
            secondWitness.Stats.SaveWill.BaseValue = -100;
            foreach (int frame in WaitInfernalSupport34(secondWitness))
                yield return frame;
            VrockRing37(first, new[] { victim },
                NalfeshneeRules39Policy.NimbusRadiusFeet - 15f);
            PlaceExpandedSummoningUnit(second, first.Position +
                Vector3.right * new Feet(
                    NalfeshneeRules39Policy.NimbusRadiusFeet * 4f).Meters);
            VrockRing37(second, new[] { secondWitness },
                NalfeshneeRules39Policy.NimbusRadiusFeet - 15f);

            var watcher = new NalfeshneeNimbusWatcher39(second, nimbus, daze);
            EventBus.Subscribe(watcher);
            try
            {
                UnitUseAbility ordered = null;
                foreach (int frame in Sprint26Cast(first, nimbus,
                    new TargetWrapper(first), value => ordered = value))
                    yield return frame;
                // A deliberate gap, so the two deadlines are genuinely
                // different instants and the first resolves while the second
                // is still pending.
                double gapStart = Game.Instance.TimeController.GameTime
                    .TotalSeconds;
                while (Game.Instance.TimeController.GameTime.TotalSeconds -
                    gapStart < GameConsts.RoundDuration * 0.4)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                foreach (int frame in Sprint26Cast(second, nimbus,
                    new TargetWrapper(second), value => ordered = value))
                    yield return frame;

                Buff firstPending = first.Descriptor.Buffs.GetBuff(windup);
                Buff secondPending = second.Descriptor.Buffs.GetBuff(windup);
                TimeSpan secondDeadline = secondPending == null ?
                    TimeSpan.Zero : secondPending.EndTime;
                int secondUses = second.Descriptor.Resources
                    .GetResourceAmount(resource);
                bool bothPending = firstPending != null &&
                    secondPending != null &&
                    firstPending.EndTime != secondPending.EndTime;

                DateTime wall = DateTime.UtcNow.AddSeconds(120);
                while (first.Descriptor.Buffs.GetBuff(windup) != null &&
                    DateTime.UtcNow < wall)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                Buff secondAfterFirst = second.Descriptor.Buffs
                    .GetBuff(windup);
                check("nalfeshnee-nimbus-owners-independent",
                    bothPending &&
                        first.Descriptor.Buffs.GetBuff(windup) == null &&
                        secondAfterFirst != null &&
                        ReferenceEquals(secondAfterFirst, secondPending) &&
                        secondAfterFirst.EndTime == secondDeadline &&
                        second.Descriptor.Resources
                            .GetResourceAmount(resource) == secondUses &&
                        watcher.Saves.Count == 0,
                    new JObject {
                        ["firstOwner"] = first.UniqueId,
                        ["secondOwner"] = second.UniqueId,
                        ["bothPendingAtDistinctDeadlines"] = bothPending,
                        ["firstDeadline"] = firstPending == null ? 0 :
                            firstPending.EndTime.TotalSeconds,
                        ["firstResolved"] =
                            first.Descriptor.Buffs.GetBuff(windup) == null,
                        ["secondStillPending"] = secondAfterFirst != null,
                        ["secondSameInstance"] =
                            ReferenceEquals(secondAfterFirst, secondPending),
                        ["secondDeadlineBefore"] =
                            secondDeadline.TotalSeconds,
                        ["secondDeadlineAfter"] = secondAfterFirst == null ?
                            0 : secondAfterFirst.EndTime.TotalSeconds,
                        ["secondUsesBefore"] = secondUses,
                        ["secondUsesAfter"] = second.Descriptor.Resources
                            .GetResourceAmount(resource),
                        ["secondBurstsSoFar"] = watcher.Saves.Count
                    },
                    "One nalfeshnee's windup resolving leaves a second's pending windup, its exact deadline and its remaining uses untouched and does not detonate it early: the pending limit and the resource are per creature, not shared.");

                wall = DateTime.UtcNow.AddSeconds(120);
                while (second.Descriptor.Buffs.GetBuff(windup) != null &&
                    DateTime.UtcNow < wall)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                check("nalfeshnee-nimbus-second-owner-resolves",
                    second.Descriptor.Buffs.GetBuff(windup) == null &&
                        watcher.Save(secondWitness) != null &&
                        watcher.Saves.All(value =>
                            value.Type == SavingThrowType.Will) &&
                        // Its own area, not the first's: the far witness is
                        // caught and the near one is untouched by THIS owner.
                        watcher.Save(victim) == null,
                    new JObject {
                        ["secondOwner"] = second.UniqueId,
                        ["secondResolved"] =
                            second.Descriptor.Buffs.GetBuff(windup) == null,
                        ["secondWitness"] = secondWitness.UniqueId,
                        ["secondWitnessSaveRolled"] =
                            watcher.Save(secondWitness) != null,
                        ["secondSaveCount"] = watcher.Saves.Count,
                        ["secondSaveTypes"] = new JArray(watcher.Saves
                            .Select(value => value.Type.ToString())
                            .Distinct()),
                        ["secondRecipients"] = new JArray(watcher.Saves
                            .Select(value => value.Initiator.UniqueId)
                            .Distinct()),
                        ["firstOwnersWitnessSparedBySecond"] =
                            watcher.Save(victim) == null,
                        ["ownerSeparationFeet"] =
                            NalfeshneeRules39Policy.NimbusRadiusFeet * 4
                    },
                    "The second nalfeshnee's own windup then bursts on its own later deadline, rolling a real Will save for the witness inside ITS radius and none for the first owner's witness far outside it - so each burst resolves against its own area and neither owner's pending resolution was consumed by the other's.");
            }
            finally
            {
                EventBus.Unsubscribe(watcher);
            // Retired here rather than at the end of the phase, so the
            // next group and the other combat mode see a clean scene.
            DisposeExpandedSummoningUnits(fixture.Created, mine.ToArray());
            }
        }

        /// <summary>
        /// A destroyed source bursts nothing.
        ///
        /// <para>The destruction goes through the same production disposal
        /// path every other review uses, so what is observed is the component
        /// marking itself on the engine's own lifetime event together with
        /// the owner-state clause in its resolution test. The fixture does not
        /// call the cancellation marker; if it did, this cell would prove only
        /// that the fixture can set a flag.</para>
        /// </summary>
        private IEnumerable<int> NimbusDestroyedSource39(
            ExpandedSummoningCorrectionFixture fixture,
            BlueprintAbility nimbus, BlueprintBuff windup,
            BlueprintBuff daze, UnitEntityData victim,
            Action<string, bool, JObject, string> check)
        {
            UnitEntityData doomed = CastExpandedSummoningOwnTier(
                fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
            VrockQuiesce37(doomed);
            foreach (int frame in Outsider30NativeAppearance(doomed,
                doomed.Blueprint.name)) yield return frame;
            foreach (int frame in NimbusWaitDazeOut39(victim, daze))
                yield return frame;
            VrockRing37(doomed, new[] { victim },
                NalfeshneeRules39Policy.NimbusRadiusFeet - 15f);

            var watcher = new NalfeshneeNimbusWatcher39(doomed, nimbus, daze);
            EventBus.Subscribe(watcher);
            try
            {
                UnitUseAbility command = null;
                double start = Game.Instance.TimeController.GameTime
                    .TotalSeconds;
                foreach (int frame in Sprint26Cast(doomed, nimbus,
                    new TargetWrapper(doomed), value => command = value))
                    yield return frame;
                Buff pending = doomed.Descriptor.Buffs.GetBuff(windup);
                bool pendingBefore = pending != null;
                double deadline = pending == null ? 0 :
                    pending.EndTime.TotalSeconds;
                // Strictly before the deadline, which is what makes this a
                // cancellation rather than a race with an expiry.
                bool beforeDeadline = pending != null &&
                    pending.TimeLeft > TimeSpan.Zero;

                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { doomed });

                // Then well past the deadline in GAME time, so a surviving
                // scheduled burst would have had every chance to fire.
                DateTime wall = DateTime.UtcNow.AddSeconds(90);
                while (DateTime.UtcNow < wall &&
                    Game.Instance.TimeController.GameTime.TotalSeconds -
                        start < GameConsts.RoundDuration * 4)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                double elapsed = Game.Instance.TimeController.GameTime
                    .TotalSeconds - start;
                check("nalfeshnee-nimbus-destroyed-source",
                    pendingBefore && beforeDeadline && doomed.Destroyed &&
                        elapsed > GameConsts.RoundDuration * 2 &&
                        watcher.Saves.Count == 0 &&
                        watcher.DazeApplications.Count == 0 &&
                        victim.Descriptor.Buffs.GetBuff(daze) == null,
                    new JObject {
                        ["owner"] = doomed.UniqueId,
                        ["commandResult"] = command == null ? null :
                            command.Result.ToString(),
                        ["pendingAtDestruction"] = pendingBefore,
                        ["destroyedBeforeDeadline"] = beforeDeadline,
                        ["deadlineGameSeconds"] = deadline,
                        ["sourceDestroyed"] = doomed.Destroyed,
                        ["elapsedGameSeconds"] = elapsed,
                        ["roundDuration"] = GameConsts.RoundDuration,
                        ["savesAfterDestruction"] = watcher.Saves.Count,
                        ["dazesAfterDestruction"] =
                            watcher.DazeApplications.Count,
                        ["witnessInsideRadius"] = true,
                        ["witnessDazed"] =
                            victim.Descriptor.Buffs.GetBuff(daze) != null,
                        ["cancellationCalledByFixture"] = false,
                        ["destroyedThroughProductionDisposal"] = true
                    },
                    "A windup pending when its source is destroyed before its deadline resolves nothing at all: no save is rolled, no daze is applied, and the witness standing well inside the radius is untouched more than two rounds past the deadline.");
            }
            finally { EventBus.Unsubscribe(watcher); }
        }

        /// <summary>
        /// Waits a daze out on its own clock rather than stripping it, so a
        /// later cycle measures a clean recipient without the fixture
        /// removing an effect this phase exists to measure.
        /// </summary>
        private IEnumerable<int> NimbusWaitDazeOut39(UnitEntityData unit,
            BlueprintBuff daze)
        {
            DateTime wall = DateTime.UtcNow.AddSeconds(180);
            while (unit.Descriptor.Buffs.GetBuff(daze) != null &&
                DateTime.UtcNow < wall)
            {
                Game.Instance.IsPaused = false;
                if (TurnBased.Controllers.CombatController
                    .IsInTurnBasedCombat())
                    foreach (int frame in Sprint18AdvanceRound(unit))
                        yield return frame;
                else yield return 0;
            }
            if (unit.Descriptor.Buffs.GetBuff(daze) != null)
                throw new InvalidOperationException(
                    "A nimbus daze outlasted its bounded natural expiry wait; " +
                    "the next cycle would have measured a stale condition.");
        }

        /// <summary>
        /// The burst's own rules, heard on the global rulebook.
        ///
        /// <para>The windup resolves from a buff's deactivation rather than
        /// from an ability execution, so the burst's context carries the DAZE
        /// as its associated blueprint and no source ability at all. That is
        /// why ownership is identified by the caster together with the daze
        /// blueprint: matching on a source ability - as the released smite
        /// watcher does - would silently record nothing here, and every
        /// assertion built on it would be vacuously true.</para>
        /// </summary>
        private sealed class NalfeshneeNimbusWatcher39 :
            IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleRollDice>,
            IGlobalRulebookHandler<RuleApplyBuff>
        {
            private readonly UnitEntityData _owner;
            private readonly BlueprintAbility _nimbus;
            private readonly BlueprintBuff _daze;
            internal readonly List<RuleSavingThrow> Saves =
                new List<RuleSavingThrow>();
            internal readonly List<RuleRollDice> Rolls =
                new List<RuleRollDice>();
            internal readonly List<RuleApplyBuff> DazeApplications =
                new List<RuleApplyBuff>();

            internal NalfeshneeNimbusWatcher39(UnitEntityData owner,
                BlueprintAbility nimbus, BlueprintBuff daze)
            { _owner = owner; _nimbus = nimbus; _daze = daze; }

            private bool Mine(MechanicsContext context)
            {
                return context != null &&
                    ReferenceEquals(context.MaybeCaster, _owner) &&
                    ReferenceEquals(context.AssociatedBlueprint, _daze);
            }

            internal RuleSavingThrow Save(UnitEntityData target)
            {
                return Saves.LastOrDefault(value =>
                    ReferenceEquals(value.Initiator, target));
            }

            internal RuleRollDice Roll(UnitEntityData target)
            {
                return Rolls.LastOrDefault(value => value.Reason != null &&
                    value.Reason.Context != null &&
                    ReferenceEquals(value.Reason.Context.MainTarget.Unit,
                        target));
            }

            internal JObject Row(UnitEntityData target, BlueprintBuff daze)
            {
                RuleSavingThrow save = Save(target);
                RuleRollDice roll = Roll(target);
                Buff condition = target.Descriptor.Buffs.GetBuff(daze);
                return new JObject {
                    ["target"] = target.UniqueId,
                    ["saveRolled"] = save != null,
                    ["saveType"] = save == null ? null : save.Type.ToString(),
                    ["saveD20"] = save == null ? null : (int?)save.D20.Value,
                    ["saveStat"] = save == null ? null : (int?)save.StatValue,
                    ["saveDc"] = save == null ? null :
                        (int?)save.DifficultyClass,
                    ["savePassed"] = save == null ? null :
                        (bool?)save.IsPassed,
                    ["rolledRounds"] = roll == null ? null : (int?)roll.Result,
                    ["dazed"] = condition != null,
                    ["dazeSecondsLeft"] = condition == null ? null :
                        (double?)condition.TimeLeft.TotalSeconds,
                    ["nimbus"] = _nimbus.AssetGuid
                };
            }

            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            // A saving throw carries no mechanics context of its own, so the
            // owner is identified from the reason's own source context.
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (evt.Reason != null && Mine(evt.Reason.Context))
                    Saves.Add(evt);
            }

            public void OnEventAboutToTrigger(RuleRollDice evt) { }
            public void OnEventDidTrigger(RuleRollDice evt)
            {
                if (ReferenceEquals(evt.Initiator, _owner) &&
                    evt.Reason != null && Mine(evt.Reason.Context))
                    Rolls.Add(evt);
            }

            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (ReferenceEquals(evt.Blueprint, _daze) &&
                    Mine(evt.Context) &&
                    !DazeApplications.Any(value => ReferenceEquals(
                        value.AppliedBuff, evt.AppliedBuff)))
                    DazeApplications.Add(evt);
            }
        }

        private static BlueprintAbility NimbusAbility39()
        {
            return (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                .BySymbol[ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix +
                    "UnholyNimbus"];
        }

        private static BlueprintBuff NimbusWindup39()
        {
            return (BlueprintBuff)ExpandedSummoningBlueprints.Registered
                .BySymbol[ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix +
                    "UnholyNimbusWindup"];
        }

        private static BlueprintBuff NimbusDaze39()
        {
            return (BlueprintBuff)ExpandedSummoningBlueprints.Registered
                .BySymbol[ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix +
                    "UnholyNimbusDaze"];
        }

        private static BlueprintAbilityResource NimbusResource39()
        {
            return (BlueprintAbilityResource)ExpandedSummoningBlueprints
                .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                    .Nalfeshnee39Prefix + "UnholyNimbusResource"];
        }
    }
}
