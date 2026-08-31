using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class GunslingerOutfitAuditTests
    {
        internal static void GuardedBoundaryIsExact()
        {
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            const string scenario = "gunslinger-outfit-audit";
            Assertions.True(catalog.Contains(
                    "internal const string GunslingerOutfitAudit") &&
                catalog.Contains(scenario) &&
                runner.Contains(
                    "GunslingerOutfitAuditScenario.Run(") &&
                automation.Contains("'" + scenario +
                    "' = [pscustomobject]") &&
                project.Contains(
                    "RuntimeTesting\\GunslingerOutfitAuditScenario.cs"),
                "Outfit audit is not wired through the exact guarded catalog, runner, automation, and build surfaces.");
            string metadata = automation.Substring(
                automation.IndexOf("'" + scenario +
                    "' = [pscustomobject]", StringComparison.Ordinal), 450);
            Assertions.True(metadata.Contains(
                    "RequiresSaveName = $false") &&
                metadata.Contains("RequiresManualInteraction = $false") &&
                metadata.Contains("ReadinessBehavior = 'mod-load'"),
                "Outfit audit must remain autonomous and save-free.");
        }

        internal static void InventoryIsDeterministicAndReadOnly()
        {
            string audit = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "GunslingerOutfitAuditScenario.cs");
            foreach (string token in new[] {
                "BlueprintRoot.Instance",
                "Progression.CharacterRaces",
                "Progression.CharacterClasses",
                "GetClothesLinks(", "GetLinks(gender, race)",
                "ResourcesLibrary.GetBlueprints<BlueprintItemEquipment>()",
                "ResourcesLibrary.LibraryObject.ResourceNamesByAssetId",
                "ResourcesLibrary.TryGetResource<EquipmentEntity>",
                "lower.StartsWith(", "StringComparison.Ordinal",
                "link.Load(false)", "class-clothing", "item-linked",
                "raw-resource", "CandidateSetId(",
                "OrderBy(value => value.AssetId",
                "RuntimeTestResultWriter.WriteAtomic",
                "no-save-owned-state" })
                Assertions.True(audit.Contains(token),
                    "Outfit audit lacks deterministic inventory token: " +
                    token);
            foreach (string forbidden in new[] {
                "SaveGame", "QuickSave", "PlayerPrefs",
                "AddEquipmentEntity(", "RemoveEquipmentEntity(",
                "RebuildOutfit(", "Game.Instance.Player.Inventory",
                "ScreenCapture", "Input.", "Mouse" })
                Assertions.False(audit.Contains(forbidden),
                    "Catalog-only audit contains forbidden mutation/UI token: " +
                    forbidden);
        }

        internal static void EvidenceManifestPreservesCatalog()
        {
            string result = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestResult.cs");
            Assertions.True(result.Contains(
                    "List<string> scenarioEvidence") &&
                result.Contains("File.Exists(value)") &&
                result.Contains("result.EvidenceFiles.Add(path)"),
                "Runtime result writer drops scenario-owned evidence files.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
