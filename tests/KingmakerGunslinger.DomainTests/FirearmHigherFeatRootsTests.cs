using System;
using System.IO;
using System.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmHigherFeatRootsTests
    {
        internal static readonly string[] RootGuids = {
            "09c9e82965fb4334b984a1e9df3bd088",
            "31470b17e8446ae4ea0dacd6c5817d86",
            "7cf5edc65e785a24f9cf93af987d66b3",
            "f4201c85a991369408740c6888362e20" };
        internal static readonly string[] RootNames = { "GreaterWeaponFocus",
            "WeaponSpecialization", "GreaterWeaponSpecialization", "ImprovedCritical" };
        internal static readonly string[] Kinds = { "Pistol", "Musket", "Blunderbuss" };

        internal static void ExactTwelveCombinationPlanIsFixed()
        {
            Assertions.Equal(4, RootGuids.Length, "Exactly four higher roots.");
            Assertions.Equal(3, Kinds.Length, "Exactly three official firearm kinds.");
            var combos = RootNames.SelectMany(root => Kinds, (root, kind) => root + ":" + kind).ToArray();
            Assertions.Equal(12, combos.Length, "Exactly twelve root/weapon combinations derive from the fixed plan.");
            Assertions.Equal(12, combos.Distinct().Count(), "All twelve combinations are distinct.");
            Assertions.True(combos.Contains("GreaterWeaponSpecialization:Blunderbuss") &&
                combos.Contains("ImprovedCritical:Pistol"), "The plan includes boundary combinations.");
        }

        internal static void RootIdentitiesAreExact()
        {
            Assertions.True(RootGuids.All(value => value.Length == 32 &&
                    value.All(ch => (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f'))),
                "Every higher-feat root GUID is a full 32-character lowercase hex identity.");
            Assertions.Equal(4, RootGuids.Distinct().Count(), "Root GUIDs are distinct.");
        }

        internal static void ScenarioWiringIsComplete()
        {
            string root = Environment.CurrentDirectory;
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestScenarioCatalog.cs"));
            Assertions.True(catalog.Contains("disposable-firearm-higher-feat-roots"),
                "The scenario is registered in the production allowlist catalog.");
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains("RunFirearmHigherFeatRoots("),
                "The runner dispatches the higher-feat-roots scenario.");
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.FirearmHigherFeatRoots.cs"));
            foreach (string token in new[] {
                "StartWithoutAssigningStaticInstance", "SelectClass(fighter, false)",
                "ApplyClassMechanics", "ApplyLevelup", "controller.Cancel()",
                "TryCommitRoot(controller, descriptor, slot", "AddFact(BlueprintBootstrap.FirearmProficiency)",
                "ExtractSelectionItems", "pending.Count == 0", "verified == 12"
            })
                Assertions.True(scenario.Contains(token),
                    "The higher-feat-roots scenario lost a required real-selection step: " + token);
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(automation.Contains("'disposable-firearm-higher-feat-roots'"),
                "The PowerShell metadata table knows the scenario.");
        }
    }
}
