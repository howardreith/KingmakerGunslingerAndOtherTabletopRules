using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Two bounded transitions over the existing natural performance cases.
        // The existing observer/economy instances remain the only subscribers.
        private IEnumerable<int> ReviewPerformanceCombat32(ExpandedSummoningCorrectionFixture fixture,
            Cleanup2NativeDefaultScope mode, UnitEntityData owner, UnitEntityData sibling,
            ActivatableAbility noncombat, BlueprintBuff recipient, Lillend32CommandStarts starts,
            FirearmReviewCosts economy, JArray costs, bool turnBased)
        {
            var game = Game.Instance; var target = fixture.Hostile;
            var anchor = game.Player.MainCharacter.Value;
            string id = turnBased ? "fascinate-combat-turn" : "competence-combat-rtwp";
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldEngagement = SettingsRoot.Instance.PauseOnEngagement.CurrentValue;
            var targetPosition = target.Position;
            BlueprintFeature charmImmunity = null; bool maneuverAdded = false;
            var idle = new[] { target, sibling, fixture.Caster, anchor };
            var passedTurns = new JArray();
            var priorCommands = new HashSet<UnitCommand>(starts.Commands);
            var dispatcher = LillendNoncombat32.Find(owner);
            if (dispatcher == null) throw new InvalidOperationException("Exact dispatcher is missing.");
            var performanceAi = owner.Blueprint.Brain.Actions.OfType<LillendCourageAi32>().Single();
            var courage = owner.Descriptor.ActivatableAbilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, performanceAi.Courage));
            var resource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == "KMG_Summoning_FeyCelestial_LillendAzata_PerformanceResource");
            int poolBefore = owner.Descriptor.Resources.GetResourceAmount(resource);
            int siblingPool = sibling.Descriptor.Resources.GetResourceAmount(resource);
            var assistance = fixture.Blueprints.OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_FeyCelestial_LillendAzata_FascinateAssistance");
            var facts = game.State.Units.All.SelectMany(unit => unit.Buffs.Enumerable).Where(value =>
                (ReferenceEquals(value.Blueprint, recipient) || turnBased && ReferenceEquals(value.Blueprint, assistance)) &&
                ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray();
            if (!mode.Current || !noncombat.IsRunning || facts.Length == 0 || poolBefore < 3 ||
                owner.IsInCombat || game.Player.IsInCombat || !owner.IsEnemy(target) || !target.IsEnemy(owner) ||
                idle.Any(unit => unit == null || unit.Destroyed || unit.IsBrainActive || !unit.Commands.Empty || unit.Commands.Queue.Count != 0))
                throw new InvalidOperationException("Exact live noncombat performance/idle owned transition precondition failed.");
            try
            {
                if (target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers))
                    throw new InvalidOperationException("Combat trigger has unexpected maneuver immunity.");
                var charm = ScriptableObject.CreateInstance<BuffDescriptorImmunity>(); charm.Descriptor = SpellDescriptor.Charm;
                charmImmunity = ScriptableObject.CreateInstance<BlueprintFeature>(); charmImmunity.Ranks = 1;
                charmImmunity.name = "KMG_Runtime_S32_PerformanceTrigger_CharmImmunity";
                charmImmunity.ComponentsArray = new BlueprintComponent[] { charm };
                target.Descriptor.AddFact(charmImmunity);
                target.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers); maneuverAdded = true;
                // Competence's owned hostile was outside Fascinate range during
                // selection. Move only that idle hostile onto a surveyed point,
                // keeping performer and its live recipient in place. Fascinate
                // already established actual geometry and needs no movement.
                if (!turnBased)
                {
                    bool placed = false;
                    foreach (var direction in new[] { Vector3.forward, Vector3.right, Vector3.back, Vector3.left })
                    {
                        var requested = owner.Position + direction * 8f;
                        var point = AstarPath.active.GetNearest(requested);
                        if (point.node == null || !point.node.Walkable || Vector3.Distance(point.clampedPosition, requested) > .6f) continue;
                        PlaceExpandedSummoningUnit(target, point.clampedPosition);
                        if (owner.HasLOS(target) && LillendPerformanceSight32.Sees(owner, target, 90)) { placed = true; break; }
                    }
                    if (!placed) throw new InvalidOperationException("Combat trigger has no visible walkable target point.");
                }
                target.Wake(0f); if (!game.State.AwakeUnits.Contains(target)) game.State.AwakeUnits.Add(target);
                if ((!turnBased && !owner.HasLOS(target)) || !LillendPerformanceSight32.Sees(owner, target, 90) ||
                    !noncombat.IsRunning || owner.IsInCombat || game.Player.IsInCombat)
                    throw new InvalidOperationException("Live performance or native combat target visibility changed before entry.");
                SettingsRoot.Instance.PauseOnEngagement.CurrentValue = false;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = turnBased;
                game.TurnBasedCombatController.Activate();
                // Ordinary native combat entry is a disclosed fixture trigger.
                // StopOwned, carrier retirement and Courage remain production work.
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.JoinCombat(); target.JoinCombat();
                // The native join controller owns the false-to-true party edge.
                // Player.UpdateIsInCombat only assigns the flag; calling it here
                // consumes that edge before the controller can emit the event
                // that initializes turn order and the native combat clock.
                double entered = game.Player.GameTime.TotalSeconds;
                double previousClock = entered, maximumClockRewind = 0;
                int enteredRound = turnBased ? game.TurnBasedCombatController.RoundNumber : -1;
                Sprint28SetBrainActivePreservingActions(owner, true);
                var deadline = DateTime.UtcNow.AddSeconds(60);
                UnitActivateAbility activation = null;
                while (DateTime.UtcNow < deadline)
                {
                    if (!mode.Current) throw new InvalidOperationException("Combat transition lost native Default ownership.");
                    double clock = game.Player.GameTime.TotalSeconds;
                    maximumClockRewind = Math.Max(maximumClockRewind, previousClock - clock);
                    previousClock = clock;
                    if (turnBased)
                    {
                        var turn = game.TurnBasedCombatController.CurrentTurn;
                        if (turn != null && !ReferenceEquals(turn.Unit, owner))
                        {
                            if (!idle.Contains(turn.Unit) || turn.Unit.IsBrainActive || !turn.Unit.Commands.Empty || turn.Unit.Commands.Queue.Count != 0)
                                throw new InvalidOperationException("Unknown or active nonparticipant in performance transition turn order.");
                            bool acting = turn.Status == TurnBased.Controllers.TurnController.TurnStatus.Preparing ||
                                turn.Status == TurnBased.Controllers.TurnController.TurnStatus.Acting;
                            if (acting && turn.CanEndTurnAndNoActing())
                            {
                                passedTurns.Add(new JObject { ["unit"] = turn.Unit.UniqueId, ["round"] = game.TurnBasedCombatController.RoundNumber });
                                EndSprint17SnakeTurn(turn);
                            }
                        }
                    }
                    activation = starts.Commands.OfType<UnitActivateAbility>().SingleOrDefault(value =>
                        !priorCommands.Contains(value) && ReferenceEquals(value.Ability, courage));
                    bool retired = !noncombat.IsOn && !noncombat.IsRunning &&
                        !owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, noncombat.Blueprint.Buff)) &&
                        facts.All(value => value.IsDisposed || !value.Owner.Buffs.Enumerable.Contains(value));
                    if (retired && activation != null && activation.IsFinished && courage.IsRunning &&
                        game.Player.GameTime.TotalSeconds >= entered + 7 &&
                        (!turnBased || game.TurnBasedCombatController.RoundNumber > enteredRound)) break;
                    game.IsPaused = false; yield return 0;
                }
                if (!mode.Current) throw new InvalidOperationException("Combat transition lost ownership at verdict.");
                var noncombatCommands = starts.Commands.OfType<UnitActivateAbility>().Where(value => !priorCommands.Contains(value) &&
                    (ReferenceEquals(value.Ability.Blueprint, dispatcher.Competence) ||
                     ReferenceEquals(value.Ability.Blueprint, dispatcher.Fascinate))).ToArray();
                bool carrierPresent = owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, noncombat.Blueprint.Buff));
                var remainingFacts = facts.Where(value => !value.IsDisposed && value.Owner.Buffs.Enumerable.Contains(value)).ToArray();
                bool clean = !noncombat.IsOn && !noncombat.IsRunning && !carrierPresent && remainingFacts.Length == 0 &&
                    LillendNoncombat32.Find(owner)?.HasPendingSelection == false && noncombatCommands.Length == 0;
                bool paid = activation != null && activation.IsActed && activation.IsFinished &&
                    activation.Result == UnitCommand.ResultType.Success && activation.Type == UnitCommand.CommandType.Move &&
                    !activation.Cutscene && !activation.IsIgnoreCooldown && economy.Count(activation) == 1 && economy.Exact(activation, turnBased);
                bool exact = clean && paid && courage.IsRunning && owner.IsInCombat && target.IsInCombat &&
                    game.Player.GameTime.TotalSeconds >= entered + 7 && owner.Descriptor.Resources.GetResourceAmount(resource) < poolBefore &&
                    (!turnBased || game.TurnBasedCombatController.RoundNumber > enteredRound) &&
                    sibling.Descriptor.Resources.GetResourceAmount(resource) == siblingPool;
                Sprint25Check(id, exact, new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                    ["noncombatOn"] = noncombat.IsOn, ["noncombatRunning"] = noncombat.IsRunning,
                    ["capturedRecipientFacts"] = facts.Length, ["retired"] = clean, ["paidCourage"] = paid,
                    ["carrierPresent"] = carrierPresent,
                    ["remainingCapturedFacts"] = new JArray(remainingFacts.Select(value => new JObject {
                        ["blueprint"] = value.Blueprint.AssetGuid, ["recipient"] = value.Owner.Unit.UniqueId,
                        ["source"] = value.Context?.MaybeCaster?.UniqueId, ["area"] = value.SourceAreaEffectId,
                        ["active"] = value.Active })),
                    ["courageRunning"] = courage.IsRunning, ["commands"] = new JArray(starts.Commands.Select(Sprint27AiCommand)),
                    ["costs"] = costs.DeepClone(), ["noncombatRestarts"] = noncombatCommands.Length,
                    ["enteredGameSeconds"] = entered, ["observedGameSeconds"] = game.Player.GameTime.TotalSeconds,
                    ["maximumObservedClockRewindSeconds"] = maximumClockRewind,
                    ["enteredRound"] = enteredRound, ["observedRound"] = turnBased ? game.TurnBasedCombatController.RoundNumber : -1,
                    ["nativeTurnState"] = turnBased ? Sprint22TurnState(owner, target) : null,
                    ["nativeControllerInitialized"] = game.TurnBasedCombatController.Initialized,
                    ["fixtureCharmOnlyImmunity"] = true, ["fixtureManeuverImmunity"] = maneuverAdded,
                    ["pendingSelectionRemainedAbsent"] = !dispatcher.HasPendingSelection,
                    ["poolBefore"] = poolBefore, ["poolAfter"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                    ["siblingPoolBefore"] = siblingPool, ["siblingPoolAfter"] = sibling.Descriptor.Resources.GetResourceAmount(resource),
                    ["idleFixtureTurnsPassed"] = passedTurns, ["fixturePerformanceCommands"] = 0, ["manualPerformanceTicks"] = 0 },
                    "Native combat entry retires the live noncombat toggle/carrier/owned recipient and assistance facts; pending selection remains absent. Natural AI pays one Move for Courage, with no noncombat restart across a later native round and independent sibling resources. Owned Charm-only/maneuver immunity isolates the combat trigger.");
                if (!exact) throw new InvalidOperationException("Native noncombat-to-combat performance transition failed.");
            }
            finally
            {
                // Independent restorations still run when a previous one fails.
                try
                {
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    foreach (var unit in idle.Concat(new[] { owner }).Where(unit => unit != null && !unit.Destroyed))
                    { if (!ReferenceEquals(unit, anchor)) unit.Commands.InterruptAll(true); if (unit.IsInCombat) unit.LeaveCombat(); }
                    game.Player.UpdateIsInCombat();
                }
                finally
                {
                    try
                    {
                        if (target != null && !target.Destroyed)
                        {
                            if (maneuverAdded) target.Descriptor.State.RemoveCondition(UnitCondition.ImmuneToCombatManeuvers);
                            if (charmImmunity != null) target.Descriptor.RemoveFact(charmImmunity);
                            PlaceExpandedSummoningUnit(target, targetPosition);
                        }
                    }
                    finally
                    {
                        if (charmImmunity != null)
                        { foreach (var component in charmImmunity.ComponentsArray) UnityEngine.Object.DestroyImmediate(component); UnityEngine.Object.DestroyImmediate(charmImmunity); }
                        SettingsRoot.Instance.PauseOnEngagement.CurrentValue = oldEngagement;
                        if (mode.Current)
                        { SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; game.TurnBasedCombatController.Activate(); }
                    }
                }
            }
        }
    }
}
