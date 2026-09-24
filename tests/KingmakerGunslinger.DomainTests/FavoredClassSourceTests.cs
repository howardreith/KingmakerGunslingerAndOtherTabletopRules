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
            "disposable-favored-class-grit",
            "disposable-favored-class-gunslinger-menus",
            "disposable-favored-class-gunslinger-mechanics",
            "disposable-favored-class-initiative-timing",
            "observe-favored-class-host-state",
            "disposable-favored-class-elemental-core",
            "disposable-favored-class-mostly-human",
            "disposable-favored-class-elemental-advanced",
            "disposable-favored-class-oracle-revelations",
            "disposable-favored-class-performance-range",
            "observe-favored-class-performance-visuals",
            "disposable-favored-class-multiclass",
            "disposable-favored-class-respec",
            "working-save-favored-class-lifecycle",
            "working-save-favored-class-visual-census"
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
                catalog.Contains("                DisposableFavoredClassGrit,") &&
                catalog.Contains("                DisposableFavoredClassGunslingerMenus,") &&
                catalog.Contains("                DisposableFavoredClassGunslingerMechanics,"),
                "Every favored-class scenario is allowlisted.");
            Assertions.True(runner.Contains("Complete(RunFavoredClassContract());") &&
                runner.Contains("Complete(RunFavoredClassGrit());") &&
                runner.Contains("Complete(RunFavoredClassGunslingerMenus());") &&
                runner.Contains("Complete(RunFavoredClassGunslingerMechanics());"),
                "Every favored-class scenario dispatches.");
        }

        // The integration never executes host code, never repeats the host's
        // class scan, patches native game types from FavoredClass/Hooks plus
        // exactly one scoped postfix on the host's PrerequisiteRace.Check
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
                // Hooks may patch native game code only (never host or CotW
                // code), from the dedicated Hooks folder.
                bool hook = Path.GetFileName(Path.GetDirectoryName(file)) == "Hooks";
                Assertions.False(!hook && (text.Contains("[HarmonyPatch") || text.Contains("HarmonyPatch(")),
                    name + " must not install Harmony patches outside FavoredClass/Hooks.");
                if (hook && name == "FavoredClassHostRaceBridge.cs")
                    Assertions.True(text.Contains("host.PrerequisiteRaceType.GetMethod(\"Check\"") &&
                        text.Contains("harmony.Patch(check, null, new HarmonyMethod(postfix), null);") &&
                        !text.Contains("[HarmonyPatch") && !text.Contains("ZFavoredClass") &&
                        !text.Contains("CallOfTheWild"),
                        name + " must patch only the adapter-resolved host race prerequisite.");
                else if (hook && name == "FavoredClassRevelationRankPatch.cs")
                    // Names the provider only to order its postfix after the
                    // provider's own assignment of the same native value.
                    Assertions.True(text.Contains("[HarmonyPatch(typeof(ContextRankConfig), \"GetBaseValue\")]") &&
                        !text.Contains("ZFavoredClass") &&
                        !text.Replace("[HarmonyAfter(\"CallOfTheWild\")]", string.Empty).Contains("CallOfTheWild"),
                        name + " must patch only the native rank base value, ordered after the provider.");
                else if (hook)
                    Assertions.True(text.Contains("[HarmonyPatch(typeof(") && !text.Contains("ZFavoredClass") &&
                        !text.Contains("CallOfTheWild"), name + " must patch a native game type only.");
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
