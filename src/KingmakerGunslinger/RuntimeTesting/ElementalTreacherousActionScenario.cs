using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class ElementalTreacherousScenario
    {
        private static void Actions(RuntimeTestRequest request, ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics, ICollection<string> files)
        {
            var rows = new JArray();
            var world = Game.Instance.State.Units.All.ToArray();
            var areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
            var random = UnityEngine.Random.state;
            var hands = Game.Instance.HandsEquipmentController;
            var setter = typeof(Game).GetProperty("HandsEquipmentController").GetSetMethod(true);
            var queueField = typeof(Kingmaker.Controllers.Units.UnitHandEquipmentController).GetField(
                "m_UnitsToUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            var queue = hands == null ? null : (List<UnitEntityData>)queueField.GetValue(hands);
            if (world.Length != 0 || (queue != null && queue.Count != 0))
                throw new InvalidOperationException("Native Treacherous actions require an empty fixture boundary.");
            var ownedHands = hands == null ? new Kingmaker.Controllers.Units.UnitHandEquipmentController() : null;
            if (ownedHands != null) setter.Invoke(Game.Instance, new object[] { ownedHands });
            var tutorial = Kingmaker.Blueprints.Root.BlueprintRoot.Instance.UITutorials.TBMStarted.Lock;
            if (tutorial == null) throw new InvalidOperationException("Native turn tutorial flag absent.");
            bool tutorialLocked = tutorial.IsLocked;
            var flagsBefore = Game.Instance.Player.UnlockableFlags.UnlockedFlags.ToArray();
            var race = BlueprintBootstrap.ElementalRaces.Oread;
            var trait = race.AlternateTraits.Require(ElementalAlternateTraitId.TreacherousEarth);
            var main = trait.Mechanics().OfType<BlueprintAbility>().Single(value => value.Type == AbilityType.Supernatural);
            var resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
            var areaBlueprint = trait.Mechanics().OfType<BlueprintAbilityAreaEffect>().Single();
            var originalComponents = main.ComponentsArray;
            var ground = main.GetComponent<ElementalTreacherousGroundTarget>();
            if (originalComponents.Count(value => value is ElementalTreacherousGroundTarget) != 1)
                throw new InvalidOperationException("The exact production ground checker is absent.");
            try
            {
                foreach (bool turnBased in new[] { false, true })
                {
                    string label = turnBased ? "turn-based-" : "rtwp-";
                    var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
                    var provider = CoreProvider(trait);
                    BlueprintFaction hostile = null;
                    ElementalNativeTurnScope turns = null;
                    ElementalNereidScenario.NativeNereidMovement movement = null;
                    AreaEffectEntityData createdArea = null;
                    var clock = Game.Instance.Player.GameTime;
                    try
                    {
                        var caster = fixture.Caster;
                        caster.Descriptor.AddFact(trait.Marker);
                        movement = new ElementalNereidScenario.NativeNereidMovement(rows, assertions, physicalGround: true);
                        hostile = UnityEngine.Object.Instantiate(caster.Blueprint.Faction);
                        hostile.name = "KMG_Runtime_Treacherous_Action_Hostile";
                        hostile.Peaceful = hostile.AlwaysEnemy = hostile.Neutral = hostile.IsDirectlyControllable = false;
                        hostile.Dummy = null; hostile.AttackFactions = new[] { caster.Blueprint.Faction };
                        var enemy = fixture.SpawnFixtureUnit(null, hostile, new Vector3(0, 0, .8f), "TreacherousActionEnemy");
                        enemy.Stats.SaveWill.BaseValue = 100;
                        caster.Memory.Add(enemy); enemy.Memory.Add(caster);
                        ElementalSpellAffinityScenario.Advance(caster.Descriptor,
                            BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                                "48ac8db94d5de7645906c7d0ad3bcfbd", "terrain command level fixture"), 2);
                        caster.Descriptor.AddFact(provider);
                        caster.Descriptor.Resources.Restore(resource, 1);
                        // The fixture borrows the save-free main-menu Player.
                        // Temporarily mark this one tutorial as seen; the exact
                        // flag dictionary is verified after restoration below.
                        if (turnBased) {
                            tutorial.Unlock();
                            turns = new ElementalNativeTurnScope(caster, enemy, rows, label);
                        }
                        else { caster.CombatState.JoinCombat(); caster.CombatState.OnNewRound(); }
                        var controller = new Kingmaker.Controllers.Units.UnitActionController();
                        var data = new AbilityData(caster.Descriptor.Abilities.GetAbility(main));
                        var near = caster.Position + new Vector3(0, 0, 1);
                        var far = caster.Position + new Vector3(0, 0, 8);
                        var target = new TargetWrapper(near);
                        var farTarget = new TargetWrapper(far);
                        if (!data.CanTarget(target) || data.CanTarget(farTarget))
                            throw new InvalidOperationException("Production ground targeting failed its native touch boundary.");
                        // Native Init treats an already-rejected target as an
                        // invalid caller operation and invokes its QA reporter.
                        // The target consumer must reject it before queuing.
                        Check(assertions, rows, label + "native-touch-placement",
                            data.CanTarget(target) && !data.CanTarget(farTarget) &&
                            !data.CanTarget(new TargetWrapper(near + Vector3.up)) &&
                            Vector3.Distance(caster.Position, far) > data.GetApproachDistance() &&
                            caster.Descriptor.Resources.GetResourceAmount(resource) == 1,
                            "production targeting rejects the distant/floating points before queuing; accepted and canceled native commands are tested below");
                        caster.Commands.InterruptAll(true); caster.Commands.RemoveFinishedAndUpdateQueue();
                        var before = ActionCosts(caster);
                        var canceled = new Kingmaker.UnitLogic.Commands.UnitUseAbility(data, target);
                        caster.Commands.Run(canceled);
                        bool queued = caster.Commands.Contains(canceled);
                        caster.Commands.InterruptAll(true); caster.Commands.RemoveFinishedAndUpdateQueue();
                        Check(assertions, rows, label + "cancel-before-commit",
                            queued && canceled.IsUnitEnoughClose && !canceled.IsStarted && !canceled.IsActed && !canceled.Cutscene &&
                            caster.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                            ActionCosts(caster).SequenceEqual(before),
                            "native queued cancellation preserves all action channels and daily use");
                        UnityEngine.Random.InitState(7419);
                        var command = new Kingmaker.UnitLogic.Commands.UnitUseAbility(data, target);
                        caster.Commands.Run(command);
                        try
                        {
                            for (int tick = 0; !command.IsActed && !command.IsFinished && tick < 16; tick++)
                            {
                                if (turns != null) turns.Drive(command);
                                else {
                                    if (command.Animation != null) command.Animation.IsActed = true;
                                    ElementalBreathScenario.TickCommand(controller, command);
                                }
                            }
                            var committed = ActionCosts(caster);
                            float expected = before[0] + (turnBased ? 6 : Math.Max(0, 6 - command.TimeSinceStart));
                            Check(assertions, rows, label + "native-standard-commit",
                                command.IsStarted && command.IsActed && !command.Cutscene && !command.IsIgnoreCooldown &&
                                Math.Abs(committed[0] - expected) < .001f &&
                                committed[1] == before[1] && committed[2] == before[2] &&
                                caster.Descriptor.Resources.GetResourceAmount(resource) == 0,
                                "before=" + string.Join(",", before) + ";committed=" + string.Join(",", committed) +
                                ";expectedStandard=" + expected + ";started=" + command.IsStarted + ";acted=" + command.IsActed);
                            for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 120; tick++)
                            {
                                if (turns == null) command.ExecutionProcess.Tick();
                                else turns.Execute(command.ExecutionProcess.Tick);
                            }
                            Game.Instance.EntityCreator.Tick();
                            createdArea = Game.Instance.State.AreaEffects.All.SingleOrDefault(value =>
                                ReferenceEquals(value.Blueprint, areaBlueprint) && !value.IsEnded &&
                                ReferenceEquals(value.Context.MaybeCaster, caster));
                            if (createdArea == null) throw new InvalidOperationException("Native accepted ground command left no owned area.");
                            Tick(createdArea);
                            Check(assertions, rows, label + "native-fixed-area-commit",
                                command.ExecutionProcess != null && command.ExecutionProcess.IsEnded &&
                                ActionCosts(caster).SequenceEqual(committed) &&
                                caster.Descriptor.Resources.GetResourceAmount(resource) == 0 && !data.IsAvailable &&
                                Vector3.Distance(createdArea.Position, near) < .01f &&
                                createdArea.Context.Params.CasterLevel == 2 &&
                                enemy.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain),
                                "single accepted native command creates exactly one owned fixed patch, charges one use and preserves action charges");
                            createdArea.ForceEnd(); Tick(createdArea);
                            if (turns != null) turns.Dispose();
                            caster.LeaveCombat(); enemy.LeaveCombat();
                            Kingmaker.Controllers.Rest.RestController.ApplyRest(caster.Descriptor);
                            Check(assertions, rows, label + "ordinary-rest",
                                !caster.IsInCombat && caster.Descriptor.Resources.GetResourceAmount(resource) == 1 && data.IsAvailable,
                                "native ordinary rest restores exactly one daily use after the accepted command");
                        }
                        finally
                        {
                            if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Detach();
                            caster.Commands.InterruptAll(true); caster.Commands.RemoveFinishedAndUpdateQueue();
                        }
                    }
                    finally
                    {
                        if (turns != null) turns.Dispose();
                        if (createdArea != null) { createdArea.ForceEnd(); Tick(createdArea); }
                        if (movement != null) movement.Dispose();
                        Game.Instance.Player.GameTime = clock;
                        fixture.Dispose();
                        UnityEngine.Object.Destroy(provider);
                        if (hostile != null) UnityEngine.Object.Destroy(hostile);
                        Check(assertions, rows, label + "lifetime",
                            (turns == null || turns.Restored) && fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                            fixture.NativeObservationReleased && fixture.NativeTeardownObserved &&
                            fixture.AreaContextRestored && fixture.PlayerContextRestored,
                            "nativeErrors=" + fixture.NativeErrors + ";nativeExceptions=" + fixture.NativeExceptions);
                    }
                }
            }
            finally
            {
                Check(assertions, rows, "production-ground-checker-unchanged",
                    ReferenceEquals(main.ComponentsArray, originalComponents) &&
                    ReferenceEquals(main.GetComponent<ElementalTreacherousGroundTarget>(), ground) &&
                    ReferenceEquals(ground.Ability, main) && trait.Definition.IsPublished,
                    "native commands use the unmodified published production checker");
                UnityEngine.Random.state = random;
                if (tutorialLocked) tutorial.Lock();
                Check(assertions, rows, "tutorial-flag-restored",
                    flagsBefore.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal).SequenceEqual(
                        Game.Instance.Player.UnlockableFlags.UnlockedFlags.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal)),
                    "exact native main-menu Player flag keys and values restored; no save was loaded");
                if (ownedHands != null) {
                    if (!ReferenceEquals(Game.Instance.HandsEquipmentController, ownedHands))
                        throw new InvalidOperationException("Native hand-controller ownership changed.");
                    setter.Invoke(Game.Instance, new object[] { hands });
                }
                if (queue != null && queue.Count != 0) {
                    if (queue.Any(unit => unit != null && !unit.ShouldBeDestroyed && unit.View != null))
                        throw new InvalidOperationException("Live foreign hand work entered the fixture.");
                    bool paused = Game.Instance.IsPaused;
                    try { Game.Instance.IsPaused = false; hands.Tick(); }
                    finally { Game.Instance.IsPaused = paused; }
                }
                Check(assertions, rows, "action-fixture-cleanup",
                    world.SequenceEqual(Game.Instance.State.Units.All) && areasBefore.SequenceEqual(Game.Instance.State.AreaEffects.All) &&
                    (queue == null || queue.Count == 0) &&
                    ReferenceEquals(hands, Game.Instance.HandsEquipmentController),
                    "exact world and native controller references restored");
                string path = Path.Combine(request.EvidenceDirectory, "elemental-treacherous-actions.json");
                File.WriteAllText(path, new JObject { { "saveStateTouched", false },
                    { "qualificationBoundary", "native RTWP/TB commands using production ground targeting, touch range, cancellation, fixed area and ordinary rest; authored areas, creator/respec and persistence excluded" },
                    { "observations", rows } }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }
        private static float[] ActionCosts(UnitEntityData unit)
        { return new[] { unit.CombatState.Cooldown.StandardAction, unit.CombatState.Cooldown.MoveAction,
            unit.CombatState.Cooldown.SwiftAction }; }

        private static BlueprintFeature CoreProvider(ElementalAlternateTraitBlueprints trait)
        {
            var provider = ScriptableObject.CreateInstance<BlueprintFeature>();
            provider.name = "KMG_Runtime_Treacherous_Core_Provider"; provider.Ranks = 1;
            provider.ComponentsArray = trait.Provider.ComponentsArray.Where(value =>
                !(value is ElementalAlternateTraitProviderController)).Select(value =>
                    UnityEngine.Object.Instantiate(value)).ToArray();
            return provider;
        }

    }
}
