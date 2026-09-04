using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalFeatNativeAuditTests
    {
        internal static void GuardedAuditIsReadOnlyAndExact()
        {
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalFeatNativeAuditScenario.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string compatibility = Read("scripts", "compatibility",
                "Invoke-KingmakerCompatibilityProfile.ps1");
            foreach (string token in new[]
            {
                "SaveStateTouched = false",
                "library.GetAllBlueprints()",
                "Enum.GetNames(",
                "DirtyTrickBlind",
                "ContextActionSpawnMonster",
                "WeaponEnergyDamageDice",
                "AddConcealment",
                "ExactBlueprintContracts",
                "FormatContract(",
                "70cffb448c132fa409e49156d013b175",
                "08ae1c01155a2184db869e9ebedc758d",
                "25699a90ed3299e438b6fd5548930809",
                "61b312b8f91cc48418768b77cd6dcc02",
                "30f90becaaac51f41bf56641966c4121",
                "107788f47c4481f4db6da06498b28270",
                "04944455200bc224d955a8e9bbd64f3f",
                "56372b0a2749c224392a5ee74105c534"
            })
                Assertions.True(scenario.Contains(token),
                    "Release B native audit is missing exact evidence token " +
                    token + ".");
            foreach (string source in new[]
            {
                catalog, automation, preflight, compatibility
            })
                Assertions.True(source.Contains(
                        "observe-elemental-feat-native-contracts"),
                    "Release B native audit is outside a guarded allowlist or dispatch surface.");
            Assertions.True(runner.Contains(
                    ".ObserveElementalFeatNativeContracts") &&
                runner.Contains("ElementalFeatNativeAuditScenario.Run("),
                "Release B native audit is outside the central constant-based dispatch surface.");
            foreach (string forbidden in new[]
            {
                "SaveManager", "SaveGame", "AddFact(", "CreateUnit(",
                "UnitSpawner", "Spend(", "Restore("
            })
                Assertions.False(scenario.Contains(forbidden),
                    "Read-only Release B audit contains mutating surface " +
                    forbidden + ".");
        }

        private static string Read(params string[] path)
        {
            return File.ReadAllText(Path.Combine(FindRoot(),
                Path.Combine(path)));
        }

        private static string FindRoot()
        {
            DirectoryInfo current = new DirectoryInfo(
                AppDomain.CurrentDomain.BaseDirectory);
            while (current != null && !File.Exists(Path.Combine(
                current.FullName, "KingmakerGunslinger.sln")))
                current = current.Parent;
            if (current == null)
                throw new DirectoryNotFoundException(
                    "Could not locate the repository root.");
            return current.FullName;
        }
    }
}
