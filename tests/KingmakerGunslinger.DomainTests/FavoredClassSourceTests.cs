using System;
using System.IO;
using System.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FavoredClassSourceTests
    {
        private static readonly string[] Scenarios =
        {
            "observe-favored-class-contract",
            "disposable-favored-class-grit"
        };

        private static string Read(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory }
                .Concat(parts).ToArray()));
        }

        internal static void RuntimeScenariosAreRegisteredEverywhere()
        {
            string catalog = Read("src", "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs");
            string automation = Read("scripts", "RuntimeAutomation.Common.ps1");
            string preflight = Read("scripts", "Test-RuntimeScenarioPreflight.ps1");
            foreach (string scenario in Scenarios)
            {
                Assertions.True(catalog.Contains("\"" + scenario + "\""), scenario + " catalog constant.");
                Assertions.True(automation.Contains("'" + scenario + "' = [pscustomobject]@{"),
                    scenario + " PowerShell metadata.");
                Assertions.True(preflight.Contains("'" + scenario + "',"), scenario + " preflight list.");
            }
            Assertions.True(catalog.Contains("                ObserveFavoredClassContract,") &&
                catalog.Contains("                DisposableFavoredClassGrit,"),
                "Both scenarios are allowlisted.");
            Assertions.True(runner.Contains("Complete(RunFavoredClassContract());") &&
                runner.Contains("Complete(RunFavoredClassGrit());"), "Both scenarios dispatch.");
        }

        // The integration never executes host code, never repeats the host's
        // class scan, installs no Harmony patch of its own in this phase
        // (therefore no second automatic hit-point patch), reads the host
        // library before any Core static, and registers its leaves in a
        // contained registry whose failure cannot fail the core bootstrap.
        internal static void IntegrationSourceInvariants()
        {
            string directory = Path.Combine(Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "FavoredClass");
            foreach (string file in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                string text = File.ReadAllText(file);
                string name = Path.GetFileName(file);
                Assertions.False(text.Contains("[HarmonyPatch") || text.Contains("HarmonyPatch("),
                    name + " must not install Harmony patches.");
                Assertions.False(text.Contains(".Invoke("), name + " must not invoke host methods.");
                Assertions.False(text.Contains("using ZFavoredClass") || text.Contains("using CallOfTheWild"),
                    name + " must not reference host namespaces.");
            }
            string adapter = Read("src", "KingmakerGunslinger", "FavoredClass", "FavoredClassHostAdapter.cs");
            int library = adapter.IndexOf("main.GetField(FavoredClassHostContract.LibraryField",
                StringComparison.Ordinal);
            int firstCoreStatic = adapter.IndexOf("ReadStatic(core,", StringComparison.Ordinal);
            Assertions.True(library > 0 && firstCoreStatic > library,
                "Main.library must be read before any Core static.");
            Assertions.True(adapter.Contains("if (readiness.LibraryAssigned)"),
                "Core statics are read only after the library is observed.");
            string bootstrap = Read("src", "KingmakerGunslinger", "Bootstrap", "BlueprintBootstrap.cs");
            Assertions.True(bootstrap.Contains("var favoredClassRegistry = new BlueprintRegistry(") &&
                bootstrap.Contains("favoredClassRegistry.RollbackAll();") &&
                bootstrap.Contains("KingmakerGunslinger.FavoredClass.FavoredClassBlueprints.Register(\n" +
                    "                        favoredClassRegistry,"),
                "Leaves register in a contained registry with exact rollback.");
            string publication = Read("src", "KingmakerGunslinger", "FavoredClass", "FavoredClassPublication.cs");
            Assertions.True(publication.Contains("feature => feature.AssetGuid, true);"),
                "Publication appends by stable identity and tolerates foreign multiplicity.");
            Assertions.True(publication.Contains("return \"class-not-scanned\";") &&
                publication.Contains("return \"profile-disabled\";"),
                "Missing classes and disabled profiles publish nothing (never an unrestricted fallback).");
        }
    }
}
