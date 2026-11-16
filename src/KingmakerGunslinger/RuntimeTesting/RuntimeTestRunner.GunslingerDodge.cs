using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Gunslinger's Dodge through its registered ability, on the guarded
    /// read-only working save. A disposable gunslinger levelled by the
    /// level-up controller stands in the loaded area with a real view; the
    /// ability is used through the game's ability command with time running,
    /// and the scenario waits across frames for the command and its native
    /// delivery. An incoming attack is the game's own weapon attack rule from
    /// a disposable hostile unit. Nothing adds the buff directly, and nothing
    /// reads the retired delivery or reaction diagnostics. No save is written.
    /// Turn-based combat is not exercised.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _dodgeSteps;
        private Stopwatch _dodgeWatch;
        private string _dodgeStage = "not-started";
        private readonly List<RuntimeTestAssertion> _dodgeAssertions =
            new List<RuntimeTestAssertion>();
        private readonly List<string> _dodgeDiagnostics = new List<string>();

        internal const string DodgeExecution =
            "registered ability through UnitUseAbility with time running in the loaded working save; " +
            "command and native delivery awaited across frames; incoming attack through RuleAttackWithWeapon";

        private sealed class DodgeNotices : IUnitBuffHandler
        {
            internal BlueprintBuff Blueprint;
            internal UnitEntityData Unit;
            internal int Added;
            internal int Removed;
            internal double LastAddedAt = -1d;
            internal double LastRemovedAt = -1d;

            private bool Ours(Buff buff)
            {
                return buff != null && ReferenceEquals(buff.Blueprint, Blueprint) &&
                    buff.Owner != null && ReferenceEquals(buff.Owner.Unit, Unit);
            }

            public void HandleBuffDidAdded(Buff buff)
            {
                if (!Ours(buff)) return;
                Added++;
                LastAddedAt = DodgeNow();
            }

            public void HandleBuffDidRemoved(Buff buff)
            {
                if (!Ours(buff)) return;
                Removed++;
                LastRemovedAt = DodgeNow();
            }
        }

        private static double DodgeNow()
        {
            return Game.Instance.TimeController.GameTime.TotalSeconds;
        }

        private void PollGunslingerDodge()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerDodge ||
                !_request.ExitAfterCompletion || _workingSaveSmoke == null ||
                !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException(
                    "Gunslinger's Dodge requires the exact guarded read-only working save and mandatory exit.");
            if (_dodgeWatch == null) _dodgeWatch = Stopwatch.StartNew();
            bool timedOut = _dodgeWatch.Elapsed.TotalSeconds >
                _request.CompletionTimeoutSeconds;
            Exception failure = null;
            if (!timedOut)
            {
                if (LoadingProcess.Instance.IsLoadingInProcess ||
                    LoadingProcess.Instance.IsLoadingScreenActive) return;
                if (_dodgeSteps == null)
                    _dodgeSteps = RunGunslingerDodge().GetEnumerator();
                try { if (_dodgeSteps.MoveNext()) return; }
                catch (Exception error) { failure = error; }
            }

            // Disposing the steps runs the scenario's cleanup, whether it
            // finished, threw or ran out of time.
            if (_dodgeSteps != null)
            {
                try { _dodgeSteps.Dispose(); }
                catch (Exception error)
                {
                    failure = failure == null ? error :
                        new AggregateException(failure, error);
                }
                _dodgeSteps = null;
            }

            if (timedOut)
                _dodgeAssertions.Add(Assertion("dodge-scenario-completed",
                    "the scenario finishes inside its completion timeout",
                    "timed out after " + _request.CompletionTimeoutSeconds +
                        " s at stage " + _dodgeStage, false, DodgeExecution));
            WorkingSaveSmokeEvidence evidence = _workingSaveSmoke.Stop();
            _dodgeAssertions.Add(Assertion("exact-working-save",
                "stable exact working-save load", evidence.StableFingerprint,
                evidence.CompletionCallbackObserved &&
                    evidence.DescriptorReferenceCorrelated &&
                    !string.IsNullOrWhiteSpace(evidence.StableFingerprint),
                "qualified receiver-bound working-save path"));
            _dodgeAssertions.Add(Assertion("loaded-mod-version",
                _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            string status = failure != null ? RuntimeTestStatuses.Error :
                timedOut ? RuntimeTestStatuses.Timeout :
                _dodgeAssertions.All(value => value.Status == "PASS")
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail;
            RuntimeTestResult result = CreateResult(status, _dodgeAssertions,
                failure == null ? null : "stage " + _dodgeStage + ": " + failure);
            result.WorkingSaveSmoke = evidence;
            result.Diagnostics.AddRange(_dodgeDiagnostics);
            Complete(result);
        }

        private void StopGunslingerDodge(RuntimeTestResult result)
        {
            if (_dodgeSteps == null) return;
            IEnumerator<int> steps = _dodgeSteps;
            _dodgeSteps = null;
            string error = "Runner completed before the Gunslinger's Dodge scenario finished (stage " +
                _dodgeStage + ").";
            try { steps.Dispose(); }
            catch (Exception failure)
            {
                error += " " + failure;
                result.Status = RuntimeTestStatuses.Error;
            }
            result.Diagnostics.Add(error);
        }

        private void DodgeAssert(string name, string expected, string observed,
            string verdict, string evidence)
        {
            _dodgeAssertions.Add(Assertion(name, expected,
                verdict == null ? observed : verdict + " | " + observed,
                verdict == null, evidence + " | " + DodgeExecution));
        }

        private static int DodgeBuffCount(UnitEntityData unit, BlueprintBuff blueprint)
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(value =>
                ReferenceEquals(value.Blueprint, blueprint));
        }

        private static Buff DodgeBuffOn(UnitEntityData unit, BlueprintBuff blueprint)
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(
                value => ReferenceEquals(value.Blueprint, blueprint));
        }

        /// <summary>Frames pass, with time running, until the condition holds or the real-time limit is reached.</summary>
        private static IEnumerable<int> DodgeUntil(Func<bool> condition,
            double realSeconds)
        {
            Stopwatch watch = Stopwatch.StartNew();
            while (!condition() && watch.Elapsed.TotalSeconds < realSeconds)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                yield return 0;
            }
        }

        private static IEnumerable<int> DodgeGameSeconds(double seconds)
        {
            double end = DodgeNow() + seconds;
            return DodgeUntil(() => DodgeNow() >= end, seconds * 4d + 30d);
        }

        /// <summary>
        /// Both units are in the fight, know of each other and can act, so
        /// that the attack does not catch its target flat-footed.
        /// </summary>
        private static void DodgeEngage(UnitEntityData first, UnitEntityData second)
        {
            first.CombatState.JoinCombat();
            second.CombatState.JoinCombat();
            first.CombatState.Cooldown.Initiative = 0f;
            second.CombatState.Cooldown.Initiative = 0f;
            if (!first.Memory.Contains(second)) first.Memory.Add(second);
            if (!second.Memory.Contains(first)) second.Memory.Add(first);
        }

        private static DodgeIncomingAttack DodgeIncoming(UnitEntityData attacker,
            UnitEntityData defender, ItemEntityWeapon weapon)
        {
            DodgeEngage(defender, attacker);
            RuleAttackWithWeapon attack = Rulebook.Trigger(
                new RuleAttackWithWeapon(attacker, defender, weapon, 0));
            RuleAttackRoll roll = attack.AttackRoll;
            return roll == null || roll.ACRule == null
                ? new DodgeIncomingAttack(false, 0, false)
                : new DodgeIncomingAttack(true, roll.TargetAC,
                    roll.IsTargetFlatFooted);
        }

        /// <summary>
        /// One use of the ability through the game's ability command. The
        /// command is issued whether or not the ability reads as available,
        /// and the observation records what the game then did.
        /// </summary>
        private IEnumerable<int> DodgeUse(UnitEntityData unit, Ability ability,
            BlueprintBuff buffBlueprint, BlueprintAbilityResource grit,
            DodgeNotices notices, DodgeCommandObservation use)
        {
            Buff[] before = unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(
                value => ReferenceEquals(value.Blueprint, buffBlueprint)).ToArray();
            int addedBefore = notices.Added;
            use.GritBefore = unit.Descriptor.Resources.GetResourceAmount(grit);
            use.BuffsBefore = before.Length;
            use.ArmorClassBefore = unit.Descriptor.Stats.AC.ModifiedValue;
            var data = new AbilityData(ability);
            use.AvailableBeforeUse = data.IsAvailable;
            unit.CombatState.Cooldown.SwiftAction = 0f;
            unit.Commands.InterruptAll(true);
            var command = new UnitUseAbility(data, new TargetWrapper(unit));
            command.IgnoreCooldown(null);
            unit.Commands.Run(command);
            Func<bool> settled = () => command.IsFinished &&
                (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded);
            foreach (int frame in DodgeUntil(settled, 30d)) yield return frame;
            use.TimedOut = !settled();
            use.CommandStarted = command.IsStarted;
            use.CommandFinished = command.IsFinished;
            use.CommandResult = command.Result.ToString();
            use.HadExecutionProcess = command.ExecutionProcess != null;
            use.DeliveryEnded = command.ExecutionProcess != null &&
                command.ExecutionProcess.IsEnded;
            if (use.TimedOut) unit.Commands.InterruptAll(true);
            Buff after = DodgeBuffOn(unit, buffBlueprint);
            use.GritAfter = unit.Descriptor.Resources.GetResourceAmount(grit);
            use.BuffsAfter = DodgeBuffCount(unit, buffBlueprint);
            use.InstanceIsNew = after != null && !before.Any(value =>
                ReferenceEquals(value, after));
            use.AddedNotices = notices.Added - addedBefore;
            use.ArmorClassAfter = unit.Descriptor.Stats.AC.ModifiedValue;
            use.MarkedNotDispelable = after != null && after.IsNotDispelable;
            use.Permanent = after != null && after.IsPermanent;
            use.TimeLeftSeconds = after == null || after.IsPermanent ? 0d :
                after.TimeLeft.TotalSeconds;
        }

        /// <summary>A disposable copy of the default character under the first faction the game treats as the defender's enemy.</summary>
        private static UnitEntityData DodgeSpawnHostile(UnitEntityData enemyOf,
            Vector3 position, List<BlueprintUnit> prototypes)
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            var seen = new HashSet<BlueprintFaction>();
            foreach (BlueprintUnit candidate in BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintUnit>()
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal))
            {
                BlueprintFaction faction = candidate.Faction;
                if (faction == null || ReferenceEquals(faction, source.Faction) ||
                    !seen.Add(faction)) continue;
                BlueprintUnit prototype = UnityEngine.Object.Instantiate(source);
                prototype.name = "KMG_Runtime_Dodge_Attacker";
                prototype.IsCheater = true;
                prototype.Faction = faction;
                UnitEntityData unit = null;
                try
                {
                    unit = Game.Instance.EntityCreator.SpawnUnit(prototype, position,
                        Quaternion.identity, enemyOf.HoldingState);
                    Game.Instance.EntityCreator.Tick();
                    if (unit != null && unit.IsInState && unit.View != null &&
                        unit.IsEnemy(enemyOf))
                    {
                        prototypes.Add(prototype);
                        return unit;
                    }
                }
                catch (Exception)
                {
                    // The next faction is tried.
                }
                if (unit != null && !unit.Destroyed) unit.Destroy();
                Game.Instance.EntityDestroyer.Tick();
                UnityEngine.Object.Destroy(prototype);
            }
            return null;
        }

        private IEnumerable<int> RunGunslingerDodge()
        {
            Game game = Game.Instance;
            Player player = game.Player;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            _dodgeStage = "fixture-admission";
            UnitEntityData anchor = player.Party.FirstOrDefault(value =>
                value.HoldingState != null && value.IsInGame);
            bool turnBased = TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
            string fixture = "mode=" + game.CurrentMode + ";turnBased=" + turnBased +
                ";partyInCombat=" + player.Party.Any(value => value.IsInCombat) +
                ";anchor=" + (anchor != null) + ";gunslinger=" + (gunslinger != null);
            bool admitted = game.CurrentMode == GameModeType.Default && !turnBased &&
                !player.Party.Any(value => value.IsInCombat) && anchor != null &&
                gunslinger != null && gunslinger.Dodge != null;
            DodgeAssert("dodge-fixture-admitted",
                "a loaded area in the default real-time mode, party out of combat, an in-area party anchor",
                fixture, admitted ? null : "the loaded fixture is not admitted for this scenario",
                "Game.CurrentMode; CombatController; Player.Party");
            if (!admitted) yield break;

            BlueprintAbility abilityBlueprint = gunslinger.Dodge.ProneAbility;
            BlueprintBuff buffBlueprint = gunslinger.Dodge.ArmorClassBuff;
            BlueprintAbilityResource grit = gunslinger.Grit.Resource;
            UnitEntityData[] units = game.State.Units.All.ToArray();
            UnitEntityData[] party = player.Party.ToArray();
            bool paused = game.IsPaused;
            var prototypes = new List<BlueprintUnit>();
            var notices = new DodgeNotices { Blueprint = buffBlueprint };
            UnitEntityData defender = null, attacker = null;
            object levelController = null;
            bool subscribed = false;
            try
            {
                _dodgeStage = "spawn-and-level";
                game.IsPaused = true;
                BlueprintUnit prototype = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                prototype.name = "KMG_Runtime_Dodge_Gunslinger";
                prototype.IsCheater = false;
                prototypes.Add(prototype);
                defender = game.EntityCreator.SpawnUnit(prototype,
                    anchor.Position + Vector3.right * 2f, Quaternion.identity,
                    anchor.HoldingState);
                game.EntityCreator.Tick();
                if (defender == null || !defender.IsInState || defender.View == null)
                    throw new InvalidOperationException(
                        "The disposable gunslinger did not enter the area with a view.");
                SetExpandedSummoningBrainActive(defender, false);
                defender.Descriptor.State.Immortality.Retain();
                defender.Stats.HitPoints.BaseValue = 10000;
                defender.Descriptor.Stats.Wisdom.BaseValue = 20;
                AdvanceDisposableGunslinger(defender.Descriptor, gunslinger, 1,
                    ref levelController);
                defender.Descriptor.Resources.Restore(grit);
                notices.Unit = defender;
                EventBus.Subscribe(notices);
                subscribed = true;

                Ability ability = defender.Descriptor.Abilities.GetAbility(abilityBlueprint);
                string[] components = abilityBlueprint.ComponentsArray.Select(value =>
                    value == null ? "<null>" : value.GetType().Name).ToArray();
                bool liveComponents = components.SequenceEqual(new[]
                {
                    typeof(AbilityResourceLogic).Name, typeof(DodgeGritCostCalculator).Name,
                    typeof(AbilityCasterHasNoFacts).Name, typeof(AbilityEffectRunAction).Name
                });
                DodgeAssert("dodge-registered-ability",
                    "granted by the level-up path; resource logic, cost calculator, not-already-active check and run-action effect",
                    "granted=" + (ability != null) + ";components=" +
                        string.Join(",", components) + ";grit=" +
                        defender.Descriptor.Resources.GetResourceAmount(grit),
                    ability == null ? "the level-up path did not grant the ability" :
                        liveComponents ? null : "the registered ability's components changed",
                    "UnitDescriptor.Abilities; BlueprintAbility.ComponentsArray");
                if (ability == null) yield break;

                attacker = DodgeSpawnHostile(defender,
                    defender.Position + Vector3.right * 1.5f, prototypes);
                if (attacker == null) throw new InvalidOperationException(
                    "No loaded faction produced a hostile disposable attacker.");
                SetExpandedSummoningBrainActive(attacker, false);
                attacker.Descriptor.State.Immortality.Retain();
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 5;
                ItemEntityWeapon weapon = attacker.Body.PrimaryHand.MaybeWeapon ??
                    attacker.Body.EmptyHandWeapon;
                if (weapon == null) throw new InvalidOperationException(
                    "The disposable attacker has no weapon to attack with.");
                foreach (int frame in DodgeGameSeconds(0.5d)) yield return frame;

                // 1. An accepted activation, seen by an incoming attack.
                _dodgeStage = "accepted-activation";
                int armorClassBase = defender.Descriptor.Stats.AC.ModifiedValue;
                DodgeIncomingAttack before = DodgeIncoming(attacker, defender, weapon);
                long guardBefore = GunslingerDodgeExpirationPatch.ExpiredRemovals;
                long onBefore = GunslingerDodgeArmorClassBonus.TurnedOn;
                long offBefore = GunslingerDodgeArmorClassBonus.TurnedOff;
                int removedBefore = notices.Removed;
                var accepted = new DodgeCommandObservation();
                foreach (int frame in DodgeUse(defender, ability, buffBlueprint,
                    grit, notices, accepted)) yield return frame;
                string acceptedVerdict = GunslingerDodgeScenarioVerdict.Accepted(accepted, 1);
                double acceptedAt = notices.LastAddedAt;
                Buff first = DodgeBuffOn(defender, buffBlueprint);
                DodgeAssert("dodge-accepted-activation",
                    "the command and its native delivery complete; 1 grit; one new timed buff; armor class +2",
                    accepted.Describe(), acceptedVerdict,
                    "UnitUseAbility; AbilityExecutionProcess; shared grit; IUnitBuffHandler");
                DodgeIncomingAttack during = DodgeIncoming(attacker, defender, weapon);

                // 2. While it is active: the availability reading, then an
                //    actual command, which the game must refuse before it
                //    spends or applies anything.
                _dodgeStage = "refused-while-active";
                var whileActive = new DodgeCommandObservation();
                foreach (int frame in DodgeUse(defender, ability, buffBlueprint,
                    grit, notices, whileActive)) yield return frame;
                string whileActiveVerdict = acceptedVerdict != null
                    ? GunslingerDodgeScenarioVerdict.NotJudged + "no activation was active"
                    : GunslingerDodgeScenarioVerdict.Refused(whileActive) ??
                        (ReferenceEquals(DodgeBuffOn(defender, buffBlueprint), first)
                            ? null : "the active buff instance was replaced");
                DodgeAssert("dodge-refused-while-active",
                    "reads unavailable; an attempted command delivers nothing, spends no grit and leaves the one active buff",
                    whileActive.Describe(), whileActiveVerdict,
                    "AbilityData.IsAvailable (queried); UnitUseAbility (attempted)");

                // 3. Left alone, it ends after one round and takes its bonus away once.
                _dodgeStage = "unused-expiry";
                foreach (int frame in DodgeUntil(() => DodgeBuffOn(defender,
                    buffBlueprint) == null, 60d)) yield return frame;
                bool ended = DodgeBuffOn(defender, buffBlueprint) == null;
                double lasted = ended && acceptedAt >= 0d
                    ? notices.LastRemovedAt - acceptedAt : -1d;
                int armorClassAfterEnd = defender.Descriptor.Stats.AC.ModifiedValue;
                DodgeAssert("dodge-unused-expiry",
                    "ends by itself after one round; one removal; armor class restored; bonus on once and off once",
                    string.Format(CultureInfo.InvariantCulture,
                        "ended={0};lastedGameSeconds={1:0.##};removedNotices={2};ac={3}->{4};" +
                        "bonusOn={5};bonusOff={6};activeModifiers={7};expiryGuardRemovals={8}",
                        ended, lasted, notices.Removed - removedBefore, armorClassBase,
                        armorClassAfterEnd, GunslingerDodgeArmorClassBonus.TurnedOn - onBefore,
                        GunslingerDodgeArmorClassBonus.TurnedOff - offBefore,
                        GunslingerDodgeArmorClassBonus.ActiveModifiers,
                        GunslingerDodgeExpirationPatch.ExpiredRemovals - guardBefore),
                    acceptedVerdict != null
                        ? GunslingerDodgeScenarioVerdict.NotJudged + "the activation did not complete"
                        : GunslingerDodgeScenarioVerdict.Expiry(ended, lasted,
                            notices.Removed - removedBefore, armorClassBase,
                            armorClassAfterEnd,
                            GunslingerDodgeArmorClassBonus.TurnedOn - onBefore,
                            GunslingerDodgeArmorClassBonus.TurnedOff - offBefore,
                            GunslingerDodgeArmorClassBonus.ActiveModifiers),
                    "IUnitBuffHandler game-time notices; GunslingerDodgeArmorClassBonus");
                DodgeIncomingAttack after = DodgeIncoming(attacker, defender, weapon);
                DodgeAssert("dodge-incoming-attack",
                    "the same incoming attack is resolved against +2 while Dodge is active and the base value before and after",
                    "before=" + before.Describe() + ";during=" + during.Describe() +
                        ";after=" + after.Describe() + ";armorClassBase=" + armorClassBase,
                    acceptedVerdict != null
                        ? GunslingerDodgeScenarioVerdict.NotJudged + "the activation did not complete"
                        : GunslingerDodgeScenarioVerdict.IncomingAttacks(before, during, after),
                    "RuleAttackWithWeapon; RuleAttackRoll.TargetAC and IsTargetFlatFooted");

                // 4. Early removal, then a new activation before the earlier
                //    one's deadline. Each prerequisite is established first.
                _dodgeStage = "recast";
                guardBefore = GunslingerDodgeExpirationPatch.ExpiredRemovals;
                var earlier = new DodgeCommandObservation();
                foreach (int frame in DodgeUse(defender, ability, buffBlueprint,
                    grit, notices, earlier)) yield return frame;
                string earlierVerdict = GunslingerDodgeScenarioVerdict.Accepted(earlier, 1);
                double earlierAt = notices.LastAddedAt;
                Buff earlierBuff = DodgeBuffOn(defender, buffBlueprint);
                foreach (int frame in DodgeGameSeconds(2d)) yield return frame;
                Buff onUnit = DodgeBuffOn(defender, buffBlueprint);
                bool sameInstance = earlierBuff != null && ReferenceEquals(onUnit, earlierBuff);
                removedBefore = notices.Removed;
                if (onUnit != null) defender.Descriptor.Buffs.RemoveFact(onUnit);
                bool earlierGone = earlierBuff != null &&
                    !defender.Descriptor.Buffs.RawFacts.Contains(earlierBuff) &&
                    DodgeBuffOn(defender, buffBlueprint) == null;
                int removalNotices = notices.Removed - removedBefore;
                int armorClassAfterRemoval = defender.Descriptor.Stats.AC.ModifiedValue;
                double removedAfter = DodgeNow() - earlierAt;
                string removalVerdict = GunslingerDodgeScenarioVerdict.EarlyRemoval(
                    sameInstance, earlierGone, removalNotices, armorClassBase,
                    armorClassAfterRemoval);
                foreach (int frame in DodgeGameSeconds(0.5d)) yield return frame;
                var later = new DodgeCommandObservation();
                foreach (int frame in DodgeUse(defender, ability, buffBlueprint,
                    grit, notices, later)) yield return frame;
                Buff laterBuff = DodgeBuffOn(defender, buffBlueprint);
                string laterVerdict = GunslingerDodgeScenarioVerdict.Accepted(later, 1) ??
                    (laterBuff != null && !ReferenceEquals(laterBuff, earlierBuff)
                        ? null : "the later activation did not produce a different instance");
                double laterAt = notices.LastAddedAt;
                bool prerequisites = earlierVerdict == null && removalVerdict == null &&
                    laterVerdict == null;
                bool laterAtDeadline = false, laterEnded = false;
                int armorClassAtDeadline = -1, armorClassAfterLater = -1;
                double atDeadline = -1d, laterLasted = -1d;
                if (prerequisites)
                {
                    foreach (int frame in DodgeUntil(() => DodgeNow() >= earlierAt + 6.6d,
                        60d)) yield return frame;
                    atDeadline = DodgeNow() - earlierAt;
                    laterAtDeadline = ReferenceEquals(DodgeBuffOn(defender, buffBlueprint),
                        laterBuff);
                    armorClassAtDeadline = defender.Descriptor.Stats.AC.ModifiedValue;
                }
                removedBefore = notices.Removed;
                foreach (int frame in DodgeUntil(() => DodgeBuffOn(defender,
                    buffBlueprint) == null, 60d)) yield return frame;
                laterEnded = DodgeBuffOn(defender, buffBlueprint) == null &&
                    notices.Removed - removedBefore == 1;
                if (laterEnded) laterLasted = notices.LastRemovedAt - laterAt;
                armorClassAfterLater = defender.Descriptor.Stats.AC.ModifiedValue;
                DodgeAssert("dodge-recast-isolation",
                    "after the earlier instance is removed early, a later activation survives the earlier deadline and ends after a round of its own",
                    string.Format(CultureInfo.InvariantCulture,
                        "earlier[{0}];removal[afterGameSeconds={1:0.##};sameInstance={2};gone={3};" +
                        "removedNotices={4};ac={5}];later[{6}];atEarlierDeadline[gameSeconds={7:0.##};" +
                        "laterOnUnit={8};ac={9}];laterEnded={10};laterLastedGameSeconds={11:0.##};" +
                        "acAfter={12};expiryGuardRemovals={13}",
                        earlier.Describe(), removedAfter, sameInstance, earlierGone,
                        removalNotices, armorClassAfterRemoval, later.Describe(), atDeadline,
                        laterAtDeadline, armorClassAtDeadline, laterEnded, laterLasted,
                        armorClassAfterLater,
                        GunslingerDodgeExpirationPatch.ExpiredRemovals - guardBefore),
                    GunslingerDodgeScenarioVerdict.Recast(earlierVerdict, removalVerdict,
                        laterVerdict, laterAtDeadline, armorClassAtDeadline, laterEnded,
                        laterLasted, armorClassBase, armorClassAfterLater),
                    "exact Buff instances; IUnitBuffHandler game-time notices");

                // 5. With no grit: the availability reading, then an actual command.
                _dodgeStage = "refused-without-grit";
                int remaining = defender.Descriptor.Resources.GetResourceAmount(grit);
                if (remaining > 0) defender.Descriptor.Resources.Spend(grit, remaining);
                var withoutGrit = new DodgeCommandObservation();
                foreach (int frame in DodgeUse(defender, ability, buffBlueprint,
                    grit, notices, withoutGrit)) yield return frame;
                DodgeAssert("dodge-refused-without-grit",
                    "reads unavailable at 0 grit; an attempted command delivers nothing, applies no buff and leaves grit at 0",
                    withoutGrit.Describe(),
                    withoutGrit.GritBefore != 0
                        ? GunslingerDodgeScenarioVerdict.NotJudged + "grit was not 0"
                        : GunslingerDodgeScenarioVerdict.Refused(withoutGrit),
                    "AbilityData.IsAvailable (queried); UnitUseAbility (attempted)");
                _dodgeStage = "cleanup";
            }
            finally
            {
                // Each owned thing is released on its own; every failure is kept.
                var failures = new List<string>();
                Action<string, Action> release = (name, action) =>
                {
                    try { action(); }
                    catch (Exception error) { failures.Add(name + ": " + error.Message); }
                };
                release("pause", () => game.IsPaused = true);
                release("buff notices", () =>
                {
                    if (subscribed) EventBus.Unsubscribe(notices);
                    subscribed = false;
                });
                release("level-up controller", () =>
                {
                    if (levelController != null)
                        levelController.GetType().GetMethod("Cancel").Invoke(levelController, null);
                });
                foreach (UnitEntityData unit in new[] { attacker, defender })
                {
                    UnitEntityData owned = unit;
                    if (owned == null) continue;
                    release("unit " + owned.Blueprint.name, () =>
                    {
                        if (owned.Destroyed) return;
                        owned.Commands.InterruptAll(true);
                        owned.Destroy();
                    });
                }
                release("entity destroyer", () =>
                {
                    game.EntityDestroyer.Tick();
                    game.EntityDestroyer.Tick();
                });
                foreach (BlueprintUnit owned in prototypes)
                {
                    BlueprintUnit prototype = owned;
                    release("prototype " + prototype.name, () =>
                        UnityEngine.Object.Destroy(prototype));
                }
                release("restore pause", () => game.IsPaused = paused);
                UnitEntityData[] extra = game.State.Units.All.Except(units).ToArray();
                UnitEntityData[] missing = units.Except(game.State.Units.All).ToArray();
                bool sameParty = player.Party.SequenceEqual(party);
                bool clean = failures.Count == 0 && extra.Length == 0 &&
                    missing.Length == 0 && sameParty && !_workingSaveSmoke.WriteObserved &&
                    GunslingerDodgeArmorClassBonus.ActiveModifiers == 0;
                _dodgeAssertions.Add(Assertion("dodge-cleanup",
                    "every owned unit, prototype and subscription released; original units and party unchanged; no Dodge modifier left; nothing saved",
                    "releaseFailures=" + (failures.Count == 0 ? "none" :
                        string.Join(" || ", failures.ToArray())) + ";extraUnits=" +
                        extra.Length + ";missingUnits=" + missing.Length + ";party=" +
                        sameParty + ";activeModifiers=" +
                        GunslingerDodgeArmorClassBonus.ActiveModifiers + ";saveWriteObserved=" +
                        _workingSaveSmoke.WriteObserved + ";endedAtStage=" + _dodgeStage,
                    clean, "Game.State.Units census; Player.Party; working-save write guard | " +
                        DodgeExecution));
            }
        }
    }
}
