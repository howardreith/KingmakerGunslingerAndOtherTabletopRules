using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class ElementalTraitNativeTurnScenario
    {
        internal static RuntimeTestResult Run(ModContext context, RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            string exceptionSummary = string.Empty;
            try
            {
                if (!context.FeatureModules.Active.ElementalRaces)
                    throw new InvalidOperationException("The native turn scenario requires Elemental Races enabled.");
                Exercise(request, assertions, evidenceFiles);
            }
            catch (Exception exception)
            {
                exceptionSummary = exception.ToString();
                diagnostics.Add(exceptionSummary);
            }
            bool pass = string.IsNullOrEmpty(exceptionSummary) &&
                assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"),
                EndUtc = string.Empty,
                DurationMilliseconds = (long)(DateTime.UtcNow - started)
                    .TotalMilliseconds,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = exceptionSummary,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .OfType<AssemblyMetadataAttribute>().SingleOrDefault(
                    item => string.Equals(item.Key, key,
                        StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }

        private static string Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        internal static void Exercise(RuntimeTestRequest request, ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> files)
        {
            var rows = new JArray(); var diagnostics = new List<string>();
            var world = Game.Instance.State.Units.All.ToArray();
            var random = UnityEngine.Random.state;
            var clock = Game.Instance.Player.GameTime;
            var hands = Game.Instance.HandsEquipmentController;
            var setter = typeof(Game).GetProperty("HandsEquipmentController").GetSetMethod(true);
            if (world.Length != 0 || Game.Instance.ProjectileController.Projectiles.Any() ||
                Game.Instance.Player.IsInCombat || CombatController.IsInTurnBasedCombat() || setter == null)
                throw new InvalidOperationException("Native turn-cost fixture requires an empty, idle main-menu world.");
            var queueField = typeof(UnitHandEquipmentController).GetField("m_UnitsToUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var handsQueue = hands == null ? null : queueField == null ? null :
                queueField.GetValue(hands) as List<UnitEntityData>;
            if (hands != null && (handsQueue == null || handsQueue.Count != 0))
                throw new InvalidOperationException("Native turn fixture cannot borrow a hand controller with pending foreign work.");
            var ownedHands = hands == null ? new UnitHandEquipmentController() : null;
            if (ownedHands != null) setter.Invoke(Game.Instance, new object[] { ownedHands });
            try
            {
                foreach (var race in new[] { BlueprintBootstrap.ElementalRaces.Sylph, BlueprintBootstrap.ElementalRaces.Undine })
                    foreach (var heritage in race.Heritages.Choices())
                        foreach (int variant in new[] { 0, 1 })
                            Run(race, heritage, variant, rows, diagnostics, assertions);
            }
            finally
            {
                if (ownedHands != null)
                {
                    if (!ReferenceEquals(Game.Instance.HandsEquipmentController, ownedHands))
                        throw new InvalidOperationException("Native turn-cost hand-controller ownership changed.");
                    setter.Invoke(Game.Instance, new object[] { hands });
                }
                if (handsQueue != null)
                {
                    if (handsQueue.Any(unit => unit != null && !unit.ShouldBeDestroyed && unit.View != null))
                        throw new InvalidOperationException("The fixture left live work on the original hand controller.");
                    bool paused = Game.Instance.IsPaused;
                    try { Game.Instance.IsPaused = false; hands.Tick(); }
                    finally { Game.Instance.IsPaused = paused; }
                }
                UnityEngine.Random.state = random; Game.Instance.Player.GameTime = clock;
                bool clean = (handsQueue == null || handsQueue.Count == 0) && CharacterCreationObservationIdentity.SameOrderedReferences(world, Game.Instance.State.Units.All.ToArray()) &&
                    !Game.Instance.ProjectileController.Projectiles.Any() && ReferenceEquals(hands, Game.Instance.HandsEquipmentController);
                Check(assertions, rows, "exact-fixture-cleanup", clean, "original world, hands, projectiles, clock and random restored");
                string path = Path.Combine(request.EvidenceDirectory, "elemental-trait-native-turn-costs.json");
                File.WriteAllText(path, new JObject { ["schemaVersion"] = 1, ["saveStateTouched"] = false,
                    ["cleanupExact"] = clean, ["observations"] = rows, ["diagnostics"] = new JArray(diagnostics),
                    ["boundary"] = "native current turn, ordinary UnitUseAbility queue, native action controller and execution process; owned animation/projectile completion cues" }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }

        private static void Run(ElementalRaceBlueprints race, ElementalHeritageBlueprints heritage, int variant,
            JArray rows, ICollection<string> diagnostics, ICollection<RuntimeTestAssertion> assertions)
        {
            var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
            ElementalNativeTurnScope turns = null;
            BlueprintFaction hostile = null;
            try
            {
                var caster = fixture.Caster;
                hostile = UnityEngine.Object.Instantiate(caster.Blueprint.Faction);
                hostile.name = "KMG_Runtime_ElementalTurn_Hostile";
                hostile.Peaceful = hostile.AlwaysEnemy = hostile.Neutral = hostile.IsDirectlyControllable = false;
                hostile.Dummy = null; hostile.AttackFactions = new[] { caster.Blueprint.Faction };
                var enemy = fixture.SpawnFixtureUnit(race.Race, hostile, new Vector3(0, 0, 0.8f), "ElementalTurnTarget");
                caster.Memory.Add(enemy); enemy.Memory.Add(caster);
                bool breeze = ReferenceEquals(race, BlueprintBootstrap.ElementalRaces.Sylph);
                var trait = race.AlternateTraits.Require(breeze ? ElementalAlternateTraitId.BreezeKissed :
                    variant == 0 ? ElementalAlternateTraitId.AcidBreath : ElementalAlternateTraitId.OozeBreath);
                caster.Descriptor.AddFact(heritage.Marker);
                caster.Descriptor.AddFact(trait.Marker);
                ElementalSpellAffinityScenario.Advance(caster.Descriptor, BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                    BlueprintBootstrap.Library, "48ac8db94d5de7645906c7d0ad3bcfbd", "native turn-cost Fighter"), 2);
                caster.Stats.HitPoints.BaseValue = enemy.Stats.HitPoints.BaseValue = 10000;
                caster.Damage = enemy.Damage = 0;
                caster.Position = Vector3.zero; caster.Orientation = 0;
                var resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
                var prefix = heritage.Definition.Id + "-" + trait.Definition.Id + "-" + variant + "-";
                turns = new ElementalNativeTurnScope(caster, enemy, rows, prefix);
                if (breeze)
                {
                    var abilities = trait.Mechanics().OfType<BlueprintAbility>().ToArray();
                    var calm = abilities.Single(value => value.name.EndsWith("_CalmWinds", StringComparison.Ordinal));
                    var renew = abilities.Single(value => value.name.EndsWith("_RenewWinds", StringComparison.Ordinal));
                    var gust = abilities.Single(value => value.GetComponent<AbilityVariants>() != null);
                    var calmed = trait.Mechanics().OfType<BlueprintBuff>().Single();
                    Cast(turns, caster, calm, new TargetWrapper(caster), resource, 1, true, rows, assertions, prefix + "calm");
                    Check(assertions, rows, prefix + "calm-fact", caster.Descriptor.HasFact(calmed), "native Calm applied its exact buff");
                    var blocked = new UnitUseAbility(Data(caster.Descriptor, renew), new TargetWrapper(caster));
                    caster.Commands.Run(blocked); turns.Drive(blocked);
                    Check(assertions, rows, prefix + "swift-already-spent", !blocked.IsStarted && blocked.ExecutionProcess == null &&
                        caster.Descriptor.HasFact(calmed) &&
                        caster.Descriptor.Resources.GetResourceAmount(resource) == 1 && Costs(caster).SequenceEqual(new[] { 0f, 0f, 6f }),
                        "same-turn Renew cannot execute or remove Calm;started=" + blocked.IsStarted +
                        ";actedFlag=" + blocked.IsActed + ";result=" + blocked.Result + ";process=" +
                        (blocked.ExecutionProcess != null) + ";costs=" + string.Join(",", Costs(caster)));
                    caster.Commands.InterruptAll(true); caster.Commands.RemoveFinishedAndUpdateQueue();
                    turns.EndCurrentTurn(); turns.ReachCasterTurn();
                    Cast(turns, caster, renew, new TargetWrapper(caster), resource, 1, true, rows, assertions, prefix + "renew");
                    Check(assertions, rows, prefix + "renew-fact", !caster.Descriptor.HasFact(calmed), "native Renew removed the exact calm buff");
                    // A standard action remains available after this same turn's swift action.
                    Cast(turns, caster, gust.GetComponent<AbilityVariants>().Variants[variant], new TargetWrapper(enemy),
                        resource, 0, false, rows, assertions, prefix + "gust");
                }
                else
                    Cast(turns, caster, trait.Mechanics().OfType<BlueprintAbility>().Single(),
                        new TargetWrapper(new Vector3(0, 0, 1.4f)), resource, 0, false, rows, assertions, prefix + "breath");
            }
            finally
            {
                try { if (turns != null) turns.Dispose(); }
                finally
                {
                    fixture.Dispose();
                    if (hostile != null) UnityEngine.Object.DestroyImmediate(hostile);
                    Check(assertions, rows, heritage.Definition.Id + "-" + variant + "-native-lifetime",
                        (turns == null || turns.Restored) && fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                        fixture.NativeInitializationObserved && fixture.NativeTeardownObserved && fixture.NativeObservationReleased &&
                        fixture.AreaContextRestored && fixture.PlayerContextRestored,
                        "nativeErrors=" + fixture.NativeErrors + ";nativeExceptions=" + fixture.NativeExceptions);
                }
            }
        }

        private static void Cast(ElementalNativeTurnScope turns, UnitEntityData caster, BlueprintAbility ability,
            TargetWrapper target, BlueprintAbilityResource resource, int remaining, bool swift, JArray rows,
            ICollection<RuntimeTestAssertion> assertions, string label)
        {
            var data = Data(caster.Descriptor, ability);
            float[] before = Costs(caster);
            int amount = caster.Descriptor.Resources.GetResourceAmount(resource);
            var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
            if (turn == null || !ReferenceEquals(turn.Unit, caster) || !data.IsAvailable || !data.CanTarget(target))
                throw new InvalidOperationException("Owned turn ability is unavailable: " + label);
            var canceled = new UnitUseAbility(data, target); caster.Commands.Run(canceled);
            caster.Commands.InterruptAll(true); caster.Commands.RemoveFinishedAndUpdateQueue();
            Check(assertions, rows, label + "-cancel", !canceled.IsStarted && !canceled.IsActed &&
                Costs(caster).SequenceEqual(before) && caster.Descriptor.Resources.GetResourceAmount(resource) == amount,
                "ordinary queued cancellation preserves action channels and daily use");
            var command = new UnitUseAbility(data, target);
            var priorProjectiles = Game.Instance.ProjectileController.Projectiles.ToArray();
            var created = new List<Projectile>();
            caster.Commands.Run(command);
            try
            {
                for (int tick = 0; !command.IsActed && !command.IsFinished && tick < 16; ++tick) turns.Drive(command);
                float[] committed = Costs(caster);
                float[] expected = (float[])before.Clone(); expected[swift ? 2 : 0] += 6;
                Check(assertions, rows, label + "-native-commit", command.IsStarted && command.IsActed &&
                    !command.Cutscene && !command.IsIgnoreCooldown && committed.SequenceEqual(expected) &&
                    ReferenceEquals(turn, Game.Instance.TurnBasedCombatController.CurrentTurn) && CombatController.IsInTurnBasedCombat(),
                    "before=" + string.Join(",", before) + ";after=" + string.Join(",", committed) + ";elapsed=" + command.TimeSinceStart + ";started=" + command.IsStarted + ";acted=" + command.IsActed +
                    ";result=" + command.Result + ";turn=" + turn.Status + ";close=" + command.IsUnitEnoughClose +
                    ";combatReady=" + caster.CombatState.CanActInCombat + ";waitingUI=" + Game.Instance.TurnBasedCombatController.WaitingForUI.Value);
                for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 120; ++tick)
                {
                    turns.Execute(command.ExecutionProcess.Tick);
                    foreach (var projectile in Game.Instance.ProjectileController.Projectiles.Where(value =>
                        !priorProjectiles.Contains(value) && !created.Contains(value) && ReferenceEquals(value.Launcher, caster)).ToArray())
                    {
                        created.Add(projectile);
                        typeof(Projectile).GetProperty("IsHit").GetSetMethod(true).Invoke(projectile, new object[] { true });
                        projectile.OnHit();
                    }
                }
                if (!command.IsFinished) turns.Drive(command);
                Check(assertions, rows, label + "-exactly-once", command.ExecutionProcess != null && command.ExecutionProcess.IsEnded &&
                    Costs(caster).SequenceEqual(committed) && caster.Descriptor.Resources.GetResourceAmount(resource) == remaining,
                    "completed=" + (command.ExecutionProcess != null && command.ExecutionProcess.IsEnded) + ";daily=" +
                    caster.Descriptor.Resources.GetResourceAmount(resource) + ";projectiles=" + created.Count);
            }
            finally
            {
                if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Detach();
                caster.Commands.InterruptAll(true); caster.Commands.RemoveFinishedAndUpdateQueue();
                foreach (var projectile in created) projectile.Cleared = true;
                Game.Instance.ProjectileController.Tick();
                Game.Instance.ProjectileController.Tick();
                if (!CharacterCreationObservationIdentity.SameOrderedReferences(priorProjectiles, Game.Instance.ProjectileController.Projectiles.ToArray()))
                    throw new InvalidOperationException("Native turn projectile cleanup did not restore exact references.");
            }
        }

        private static AbilityData Data(UnitDescriptor owner, BlueprintAbility ability)
        {
            var data = owner.Abilities.GetAbility(ability);
            if (data != null) return data.Data;
            if (ability.Parent != null && owner.Abilities.GetAbility(ability.Parent) != null)
                return new AbilityData(new AbilityData(owner.Abilities.GetAbility(ability.Parent)), ability);
            throw new InvalidOperationException("Native turn fixture does not own the exact ability.");
        }
        private static float[] Costs(UnitEntityData owner) => new[] { owner.CombatState.Cooldown.StandardAction,
            owner.CombatState.Cooldown.MoveAction, owner.CombatState.Cooldown.SwiftAction };
        private static void Check(ICollection<RuntimeTestAssertion> assertions, JArray rows, string name, bool pass, string observed)
        {
            rows.Add(new JObject { ["name"] = name, ["pass"] = pass, ["observed"] = observed });
            assertions.Add(new RuntimeTestAssertion { Name = "elemental-native-turn-" + name,
                Expected = "exact native turn action and daily resource contract", Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "native current actor, ordinary command, UnitActionController, execution process and cooldown state" });
            if (!pass) throw new InvalidOperationException("Native turn-cost assertion failed: " + name + "; " + observed);
        }
    }
}