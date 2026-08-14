using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class EasternWeaponsInvestigationTests
    {
        internal static void EvidenceAndObserverRemainInvestigationOnly()
        {
            string root = Environment.CurrentDirectory;
            string report = File.ReadAllText(Path.Combine(root, "docs",
                "EASTERN-WEAPONS-IMPLEMENTATION-EVIDENCE.md"));
            string observer = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "EasternWeaponContractObserver.cs"));
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs"));
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));

            Assertions.True(report.Contains(
                    "4ffd15b09992bd9cee9d330eee0a650ad2c94661") &&
                report.Contains("Weapon Proficiency (Elven Branched Spear)") &&
                report.Contains("No production category value") &&
                report.Contains("observe-eastern-weapon-contracts"),
                "Investigation evidence must record the exact base, corrected proficiency contract, scenario, and fail-closed production gate.");
            foreach (string donor in new[] { "\"kukri\"", "\"shortsword\"",
                "\"rapier\"", "\"scimitar\"", "\"longsword\"",
                "\"bastardsword\"", "\"falchion\"", "\"greatsword\"" })
                Assertions.True(observer.Contains(donor),
                    "Observer must inventory donor term " + donor + ".");
            foreach (string contract in new[] { "\"brilliantenergy\"",
                "\"mightycleaving\"", "\"impact\"", "\"coupdegrace\"",
                "\"originalsize\"", "\"criticalconfirm\"",
                "eastern-campaign-contract-inventory",
                "eastern-contract-observer-save-free" })
                Assertions.True(observer.Contains(contract),
                    "Observer must inventory contract " + contract + ".");
            Assertions.False(observer.Contains(".AddFact(") ||
                observer.Contains(".InsertItem(") || observer.Contains(".AddLoot(") ||
                observer.Contains("ComponentsArray =") ||
                observer.Contains("SaveGame") || observer.Contains("QuickSave") ||
                observer.Contains("LoadGame"),
                "Investigation observer may not mutate a unit, blueprint, inventory, loot table, or save.");
            const string scenario = "observe-eastern-weapon-contracts";
            Assertions.True(catalog.Contains(scenario) &&
                runner.Contains("EasternWeaponContractObserver.Run(") &&
                automation.Contains("'" + scenario + "' = [pscustomobject]") &&
                automation.Contains("RequiresSaveName = $false") &&
                automation.Contains("RequiresManualInteraction = $false"),
                "Eastern contract observer must be an allowed autonomous save-free runtime scenario.");
        }
    }
}
