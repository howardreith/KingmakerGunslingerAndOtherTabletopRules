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
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(observer.Contains(
                    "f7d7eb166b3dd594fb330d085df41853") &&
                observer.Contains("ExpandForward(selected, 4)") &&
                observer.Contains("ExpandReverse(all, selected, 3)") &&
                observer.Contains("barbarian.ClassSkills") &&
                observer.Contains("barbarian.Archetypes") &&
                observer.Contains("progression.LevelEntries") &&
                observer.Contains("component.GetType().Assembly.GetName().Name") &&
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
        }
    }
}
