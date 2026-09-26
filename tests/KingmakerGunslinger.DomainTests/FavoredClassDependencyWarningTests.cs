using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>L06: the missing-dependency warning's classification, text and wiring.</summary>
    internal static class FavoredClassDependencyWarningTests
    {
        private const string Gunslinger = "abca4797366d4df0831a418eee39069a";
        private const string Alchemist = "0937bec61c0dabc468428f496580c721";

        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src", "KingmakerGunslinger" }
                .Concat(parts).ToArray())).Replace("\r\n", "\n");
        }

        // Only what KMG can prove is attributed: the host's favored class
        // choice and its per-class identities, the Call of the Wild content
        // the manifests name, KMG's own identities; the rest is unknown.
        internal static void MissingReferencesAreClassifiedByProof()
        {
            string revelation = FavoredClassRevelationManifest.All.First().FeatureGuids.First();
            string owned = FavoredClassIdentityCatalog.All.First().Guid;
            var missing = new[]
            {
                FavoredClassDependencyPolicy.FavoredClassChoiceGuid,
                FavoredClassHostContract.ExpectedProgressionGuid(Gunslinger),
                FavoredClassHostContract.ExpectedBonusSelectionGuid(Alchemist),
                FavoredClassRevelationManifest.OracleClassGuid,
                revelation,
                owned,
                "ffffffffffffffffffffffffffffffff",
                FavoredClassDependencyPolicy.FavoredClassChoiceGuid
            };
            FavoredClassDependencyReport report = FavoredClassDependencyPolicy.Classify(missing,
                FavoredClassIntegrationAvailability.HostDisabled, new[] { Gunslinger, Alchemist });
            Assertions.Equal(7, report.Total, "Distinct missing references.");
            Assertions.Equal(3, report.Host, "The choice and two per-class host identities.");
            Assertions.Equal(2, report.CallOfTheWild, "The Oracle class and one revelation.");
            Assertions.Equal(1, report.Owned, "One KMG identity.");
            Assertions.Equal(1, report.Unknown, "One unknown reference.");
            Assertions.True(report.Relevant, "Host content makes the warning relevant.");
            Assertions.True(report.HostExamples.Contains("favored-class-choice"), "The choice is named.");
            FavoredClassDependencyReport unrelated = FavoredClassDependencyPolicy.Classify(
                new[] { "ffffffffffffffffffffffffffffffff" }, FavoredClassIntegrationAvailability.Published,
                new[] { Gunslinger });
            Assertions.False(unrelated.Relevant, "An unrelated missing reference is not KMG's warning.");
        }

        internal static void WarningTextIsPreciseAndSafe()
        {
            FavoredClassDependencyReport report = FavoredClassDependencyPolicy.Classify(
                new[]
                {
                    FavoredClassDependencyPolicy.FavoredClassChoiceGuid,
                    FavoredClassRevelationManifest.OracleClassGuid,
                    "ffffffffffffffffffffffffffffffff"
                },
                FavoredClassIntegrationAvailability.HostDisabled, new[] { Gunslinger });
            string text = FavoredClassDependencyPolicy.Compose(report);
            foreach (string token in new[]
            {
                "3 of its blueprint references are missing",
                "1 from Favored Class (ZFavoredClass 1.3.1, which is disabled in Unity Mod Manager)",
                "1 from Call of the Wild 1.14.4c",
                "1 from another mod or version",
                "Nothing was loaded and the save file was not changed.",
                "install and enable ZFavoredClass 1.3.1 with Call of the Wild 1.14.4c in Unity Mod Manager",
                "do not save over it"
            })
                Assertions.True(text.Contains(token), "Warning text: " + token);
            Assertions.Equal("not installed or not loaded",
                FavoredClassDependencyPolicy.HostState(FavoredClassIntegrationAvailability.HostAbsent), "Absent.");
            Assertions.Equal("installed but not the supported build",
                FavoredClassDependencyPolicy.HostState(FavoredClassIntegrationAvailability.UnsupportedBinary),
                "Unsupported.");
        }

        // The hooks only observe: the converter still throws natively, the
        // menu message keeps the native text after KMG's, nothing writes.
        internal static void HooksOnlyObserve()
        {
            string hooks = Source("FavoredClass", "Hooks", "FavoredClassDependencyWarningPatches.cs");
            foreach (string token in new[]
            {
                "private static void ConverterPrefix(JsonReader reader)",
                "harmony.Patch(read, new HarmonyMethod(prefix), null, null);",
                "[HarmonyPatch(typeof(Game), \"LoadGame\")]",
                "[HarmonyPatch(typeof(Game), \"ResetToMainMenu\")]",
                "private static void Prefix(ref string message)",
                "message = FavoredClassDependencyWarning.Augment(message);",
                "FavoredClassDependencyWarning.BeginLoad();"
            })
                Assertions.True(hooks.Contains(token), "Hook token: " + token);
            foreach (string forbidden in new[] { "return false", "SaveGame(", "SaveJson(", "__result", "Postfix" })
                Assertions.False(hooks.Contains(forbidden), "The hooks never suppress or write: " + forbidden);
            string warning = Source("FavoredClass", "FavoredClassDependencyWarning.cs");
            Assertions.True(warning.Contains("return FavoredClassDependencyPolicy.Compose(report) + \"\\n\\n\" + nativeMessage;") &&
                warning.Contains("if (nativeMessage == null || missing.Length == 0)") &&
                warning.Contains("if (!report.Relevant)"),
                "The native message is kept, after KMG's warning, and only for a relevant failure.");
            string context = Source("Bootstrap", "ModContext.cs");
            Assertions.True(context.Contains("FavoredClass.Hooks.FavoredClassDependencyWarningPatches.Install(harmony);"),
                "The converter observation is installed with the other hand-made patches.");
        }
    }
}
