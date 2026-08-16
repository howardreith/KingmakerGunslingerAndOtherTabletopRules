using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class UrbanBarbarianInventoryContractTests
    {
        internal static void GuardedInventoryIsReadOnlyAndComplete()
        {
            string root = Environment.CurrentDirectory;
            string observer = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "UrbanBarbarianRageInventoryObserver.cs"));
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs"));
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            string focused = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "UrbanBarbarianFocusedScenario.cs"));
            string request = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRequest.cs"));
            string persistence = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "UrbanBarbarian",
                "UnitPartControlledRageSelection.cs"));
            string rageRuntime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "UrbanBarbarian",
                "ControlledRageRuntime.cs"));
            string blueprints = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "UrbanBarbarianBlueprints.cs"));
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(observer.Contains(
                    "f7d7eb166b3dd594fb330d085df41853") &&
                observer.Contains("4b1f3dd0f61946249a654941fc417a89") &&
                observer.Contains("CanContainRageContract(blueprint)") &&
                observer.Contains("records.Count <= 3000") &&
                !observer.Contains("ExpandReverse(") &&
                observer.Contains("barbarian.ClassSkills") &&
                observer.Contains("barbarian.Archetypes") &&
                observer.Contains("progression.LevelEntries") &&
                observer.Contains("component.GetType().FullName, \"barbarian\"") &&
                !observer.Contains("Contains(component.GetType().Assembly.GetName().Name, \"CallOfTheWild\")") &&
                observer.Contains("save-free-observer") &&
                !observer.Contains("BlueprintRegistry.Register") &&
                !observer.Contains("File.WriteAllBytes"),
                "Urban Rage inventory does not preserve its read-only exact-graph contract.");
            Assertions.True(catalog.Contains(
                    "observe-urban-barbarian-rage-inventory") &&
                runner.Contains("UrbanBarbarianRageInventoryObserver.Run") &&
                automation.Contains(
                    "'observe-urban-barbarian-rage-inventory' = [pscustomobject]@{") &&
                automation.Contains("RequiresManualInteraction = $false") &&
                automation.Contains("ReadinessBehavior = 'mod-load'"),
                "Guarded Urban Rage inventory is not consistently allowlisted and dispatched.");
            foreach (string token in new[] {
                "disposable-urban-barbarian-focused",
                "UrbanBarbarianFocusedScenario.Run",
                "urban-ordinary-rage",
                "urban-constitution-hp-cycle",
                "urban-greater-mighty-tiers",
                "urban-level-two-live-selector-boundary",
                "MechanicActionBarSlotAbility.GetConvertedAbilityData",
                "LivePanelVariants",
                "urban-native-rage-lifecycle",
                "urban-crowd-control-player-attack-pipeline",
                "UnitAttack.CreateAttackCommand",
                "TriggerAttackRule",
                "AttackBonusRule.BonusSources",
                "ACRule.BonusSources",
                "CrowdControlComponent.DescribeCandidate",
                "LastAttackObservation",
                "LastArmorClassObservation",
                "urban-controlled-trickery-and-spell-restriction",
                "StatType.SkillThievery",
                "spellAvailability.IsAvailable",
                "SpellCastingForbidden.Count",
                "urban-crowd-control-rule-events",
                "RuleCalculateAttackBonusWithoutTarget",
                "RuleCalculateAC",
                "new RuleCalculateAC(attacker, defender",
                "SpawnHostileTarget",
                "ApplyLevel(urban.Descriptor",
                "State.Units.All.Add(urban)",
                "ordinaryToggle.OnNewRound()",
                "ordinaryToggle.Stop(true)",
                "ControlledRageRuntime.TrySelect(owner, allocation)",
                "UnitPartControlledRageSelection>() != null",
                "!ordinaryFatiguedBefore" })
                Assertions.True(catalog.Contains(token) || runner.Contains(token) ||
                    focused.Contains(token) || automation.Contains(token),
                    "Focused Urban scenario contract is missing: " + token);
            Assertions.True(automation.Contains(
                    "'disposable-urban-barbarian-focused' = [pscustomobject]@{") &&
                focused.Contains("Same(unitsBefore, Snapshot(allUnits))") &&
                !focused.Contains("SaveGame") && !focused.Contains("LoadGame"),
                "Focused Urban scenario is not a save-free guarded fixture.");
            foreach (string token in new[] {
                "working-save-urban-barbarian-prepare",
                "working-save-urban-barbarian-off-verify-cleanup",
                "StartUrbanBarbarianPersistence",
                "CompleteUrbanBarbarianPersistence",
                "urban-persistence-features",
                "urban-persistence-selection",
                "urban-persistence-active-rage",
                "urban-persistence-module-off",
                "ArmExactWorkingSaveWrite" })
                Assertions.True(catalog.Contains(token) || runner.Contains(token) ||
                    automation.Contains(token),
                    "Urban persistence contract is missing: " + token);
            Assertions.True(automation.Contains(
                    "'working-save-urban-barbarian-prepare' = [pscustomobject]@{") &&
                automation.Contains(
                    "'working-save-urban-barbarian-off-verify-cleanup' = [pscustomobject]@{") &&
                automation.Contains("PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WorkingSaveUrbanBarbarianPrepare") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WorkingSaveUrbanBarbarianOffVerifyCleanup") &&
                runner.Contains("set.Count == UrbanBarbarianIdentityCatalog.IdentityCount") &&
                runner.Contains("archetypeReferences == (expectedActive ? 1 : 0)") &&
                runner.Contains("fixtureFeatures.Reverse()") &&
                runner.Contains("owner.Descriptor.RemoveFact(selector)") &&
                runner.Contains("set.TierSelectors") &&
                runner.Contains("set.LegacySelector") &&
                runner.Contains("cleanupDetail = \"features=\"") &&
                runner.Contains("Remove<UnitPartControlledRageSelection>()") &&
                persistence.Contains("[JsonProperty]") &&
                persistence.Contains("public override void PreSave()") &&
                persistence.Contains("public override void PostLoad()") &&
                persistence.Contains("TrySelectExact") &&
                rageRuntime.Contains("SynchronizeSelectionFacts") &&
                rageRuntime.Contains("SynchronizeSelector") &&
                rageRuntime.Contains("owner.Ensure<UnitPartControlledRageSelection>()") &&
                blueprints.Contains("ControlledRageSelectionController") &&
                blueprints.Contains("CreateLegacySelector") &&
                blueprints.Contains("SequenceEqual(new[] { 6, 10, 15 })") &&
                !blueprints.Contains("grant.Facts = new BlueprintUnitFact[] { selector }"),
                "Urban persistence is not guarded, module-aware, or cleanup-complete.");
            Assertions.Equal(2, Count(runner,
                "IsUrbanBarbarianPersistenceScenario() ||"),
                "Urban persistence must route through both the guarded working-save dispatch and its post-readiness exception path.");
        }

        private static int Count(string source, string value)
        {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(value, offset,
                StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }
            return count;
        }
    }
}
