using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElvenBranchedSpearInvestigationTests
    {
        internal static void EvidenceReportAndObserverAreGuarded()
        {
            string root = Environment.CurrentDirectory;
            string report = File.ReadAllText(Path.Combine(root, "docs",
                "ELVEN-BRANCHED-SPEAR-IMPLEMENTATION-EVIDENCE.md"));
            string observer = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "ElvenBranchedSpearContractObserver.cs"));
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs"));
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));

            Assertions.True(report.Contains(
                    "6357b8cb27b92f6974ff61409c7aaffb7f2c3cdc") &&
                report.Contains("UnitCombatState.Disengage(target)") &&
                report.Contains("elven-branched-spears") &&
                report.Contains("No production identity or campaign target"),
                "Investigation report must record the base, exact movement boundary, module convention, and fail-closed identity gate.");
            Assertions.True(observer.Contains("\"longspear\"") &&
                observer.Contains("\"fauchard\"") &&
                observer.Contains("\"glaive\"") &&
                observer.Contains("\"bardiche\"") &&
                observer.Contains("\"elvencurvedblade\"") &&
                observer.Contains("\"damagegrace\"") &&
                observer.Contains("\"damagestatreplacement\"") &&
                observer.Contains("spear-native-cold-iron-weapons") &&
                observer.Contains("spear-native-weapon-feature-selections") &&
                observer.Contains("spear-native-race-grants") &&
                observer.Contains("spear-contract-observer-save-free"),
                "Read-only observer must inventory every required donor and Dexterity component family.");
            Assertions.False(observer.Contains(".AddFact(") ||
                observer.Contains(".InsertItem(") ||
                observer.Contains(".AddLoot(") ||
                observer.Contains("ComponentsArray =") ||
                observer.Contains("SaveGame") || observer.Contains("QuickSave") ||
                observer.Contains("LoadGame"),
                "Investigation observer may not mutate a unit, blueprint, inventory, loot table, or save.");
            const string scenario = "observe-elven-branched-spear-contracts";
            Assertions.True(catalog.Contains(scenario) &&
                runner.Contains("ElvenBranchedSpearContractObserver.Run(") &&
                automation.Contains("'" + scenario + "' = [pscustomobject]") &&
                automation.Contains("RequiresSaveName = $false") &&
                automation.Contains("RequiresManualInteraction = $false"),
                "Spear contract observer must be an allowed autonomous save-free runtime scenario.");
        }
    }
}
