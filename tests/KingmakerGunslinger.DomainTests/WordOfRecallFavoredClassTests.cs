using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Spells.ShieldOther;

namespace KingmakerGunslinger.DomainTests
{
    // Regression coverage for the Favored Class Oracle Word of Recall
    // pick-gate repair: the per-level BlueprintParametrizedFeature lists the
    // reconciled spell through its shared class list, but the native pick
    // admits only items present in BlueprintParameterVariants (the
    // m_CachedItems source), so the canonical ability must be published
    // there too — additively, idempotently, and only for the exact
    // structurally validated per-level feature.
    internal static class WordOfRecallFavoredClassTests
    {
        private sealed class FakeVariant
        {
            internal FakeVariant(string guid) { Guid = guid; }
            internal string Guid { get; private set; }
        }

        internal static void VariantsMergePublishesOnceAndPreservesEntries()
        {
            var foreign = new FakeVariant("mystic-theurge-snapshot-spell");
            var recall = new FakeVariant("word-of-recall");
            var current = new List<FakeVariant> { foreign };
            List<FakeVariant> published = ShieldOtherSpellListMergePolicy.Merge(
                current, recall, value => value.Guid);
            Assertions.True(published.Count == 2 &&
                ReferenceEquals(published[0], foreign) &&
                ReferenceEquals(published[1], recall),
                "The Favored Class variants publication must preserve every foreign snapshot entry and append the canonical Word of Recall exactly once.");
            List<FakeVariant> second = ShieldOtherSpellListMergePolicy.Merge(
                published, recall, value => value.Guid);
            Assertions.True(ReferenceEquals(second, published) && second.Count == 2,
                "Repeated Favored Class variants reconciliation must be idempotent.");
            List<FakeVariant> already = new List<FakeVariant> { recall, foreign };
            List<FakeVariant> unchanged = ShieldOtherSpellListMergePolicy.Merge(
                already, recall, value => value.Guid);
            Assertions.True(ReferenceEquals(unchanged, already),
                "An already-published variants array must not be rebuilt or reordered.");
        }

        internal static void VariantsMergeFailsClosedOnNullEntries()
        {
            var recall = new FakeVariant("word-of-recall");
            Assertions.Throws<InvalidOperationException>(() =>
                ShieldOtherSpellListMergePolicy.Merge(
                    new List<FakeVariant> { null }, recall, value => value.Guid),
                "A malformed Favored Class variants array must fail closed instead of publishing.");
        }

        internal static void ReconcilerFavoredClassContract()
        {
            string root = Environment.CurrentDirectory;
            string source = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Spells", "Teleportation",
                "TeleportationFinalLiveReconciler.cs"));
            foreach (string token in new[] {
                "9ba3858327354e2093613efb9de198d7",
                "ResolveFavoredClassLevelFeature",
                "BlueprintParameterVariants",
                "m_CachedItems",
                "FeatureParameterType.LearnSpell",
                "FavoredClassTargetPolicy.Resolve",
                "HasValidSelectionContract",
                "HasValidGrantConfiguration",
                "level.SpecificSpellLevel || level.SpellLevelPenalty != 0",
                "PrerequisiteClassSpellLevel",
                "prerequisite.RequiredSpellLevel == level.SpellLevel + 1",
                "if (resolution.Target == null)",
                "ReadItemCache",
                "WriteItemCache",
                "FavoredClassVariantsTransaction<BlueprintScriptableObject>",
                "Favored Class Oracle level-6 variants are not singular",
                "favored-class.absent",
                "favored-class.malformed",
                "favored-class.ambiguous",
                "favored-class.published",
                "favored-class.unchanged",
                "favored-class.failed" })
                Assertions.True(source.Contains(token),
                    "Favored Class reconciler contract is missing: " + token);
            Assertions.True(source.Split(new[] { "ReconcileFavoredClass(library, wordOfRecall, context);" },
                StringSplitOptions.None).Length == 3,
                "The Favored Class variants reconciliation must run an idempotent second pass.");
            Assertions.True(source.Contains("FavoredClassTargetResolution.Absent(") &&
                source.Contains("FavoredClassTargetResolution.Malformed(") &&
                source.Contains("FavoredClassTargetResolution.Ambiguous("),
                "Optional absence, malformed presence and ambiguity must be distinct outcomes.");
        }

        internal static void FavoredClassRouteScenarioContract()
        {
            string root = Environment.CurrentDirectory;
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.WordOfRecallFavoredClass.cs"));
            foreach (string token in new[] {
                "27947ef789544982a437200c3189c59a",
                "9ba3858327354e2093613efb9de198d7",
                "7249760f01784ea997afaa9c433c2e68",
                "b19759026d6508b9022f1edb4ec4b31f",
                "b7f02ba92b363064fb873963bec275ee",
                "0a5d473ead98b0646b94495af250fdc4",
                "ExtractSelectionItems",
                "fcb-level6-candidates-",
                "fcb-incomplete-award-control",
                "fcb-below-prerequisite-control",
                "fcb-duplicate-control",
                "fcb-persistence-roundtrip",
                "fcb-cancel",
                "fcb-aasimar-committed",
                "fcb-human-committed",
                "fcb-cleanup" })
                Assertions.True(scenario.Contains(token),
                    "Favored Class acceptance scenario lacks evidence token: " + token);
            Assertions.False(scenario.Contains(".AddKnown("),
                "The acceptance fixture must never inject the spell through AddKnown.");
            Assertions.True(scenario.Contains("!ReferenceEquals(value, recall)"),
                "The ordinary filler picks must never consume the canonical Word of Recall.");
        }

        internal static void PersistenceDriverCleanupContract()
        {
            string root = Environment.CurrentDirectory;
            string driver = File.ReadAllText(Path.Combine(root, "scripts",
                "Invoke-WordOfRecallFavoredClassPersistence.ps1"));
            string selfTest = File.ReadAllText(Path.Combine(root, "scripts",
                "Test-WordOfRecallFavoredClassPersistence.ps1"));
            string common = File.ReadAllText(Path.Combine(root, "scripts",
                "fcb-persistence-regressions", "Common.ps1"));
            foreach (string token in new[] {
                "function Remove-PersistenceOwnedSave",
                "function Invoke-FinalizationStages",
                "completedSha256",
                "createdSha256",
                "changed or replaced output",
                "preserved = $true",
                "cleanupFailed",
                "preservedOwnedSaves",
                "stageOutcomes",
                "catalog-assertion",
                "process-exit",
                "prohibited while the game process is alive",
                "Close-KmgProtectedSaveCatalog" })
                Assertions.True(driver.Contains(token),
                    "Persistence driver cleanup contract is missing: " + token);
            foreach (string token in new[] {
                "fcb-persistence-regressions",
                "Scenario1ChangedOutput.ps1",
                "Scenario2CatalogAssertion.ps1",
                "Scenario3SidecarFailure.ps1",
                "Scenario4ProcessExit.ps1",
                "Scenario5Success.ps1",
                "checks=$script:checks" })
                Assertions.True(selfTest.Contains(token),
                    "Persistence finalization regression runner lacks: " + token);
            foreach (string token in new[] {
                "Assert-KmgProtectedSaveCatalog",
                "Close-KmgProtectedSaveCatalog",
                "Invoke-FinalizationStages",
                "Remove-PersistenceOwnedSave" })
                Assertions.True(common.Contains(token),
                    "Persistence finalization regression prologue lacks: " + token);
            // Cleanup failures must be recorded before any throw and cannot
            // skip catalog disposal.
            int cleanupIndex = driver.IndexOf("foreach ($save in $Owned)", StringComparison.Ordinal);
            int catalogIndex = driver.IndexOf("Close-KmgProtectedSaveCatalog", StringComparison.Ordinal);
            int stageWrite = driver.IndexOf("'mods-after.json'", StringComparison.Ordinal);
            Assertions.True(cleanupIndex >= 0 && catalogIndex > cleanupIndex && stageWrite > catalogIndex,
                "Cleanup, catalog disposal and the inventory evidence must run in that order.");
            // Every finalization stage runs independently through its own
            // try/catch; the record write precedes both failure throws.
            int recordWrite = driver.IndexOf("'transaction-result.json'", StringComparison.Ordinal);
            int reportFailure = driver.IndexOf(
                "Persistence finalization failed before its record", StringComparison.Ordinal);
            int finalFailure = driver.IndexOf(
                "Persistence finalization failed: ", StringComparison.Ordinal);
            int stageTry = driver.IndexOf("try {", driver.IndexOf(
                "Assert-KmgProtectedSaveCatalog", StringComparison.Ordinal));
            int stageCatch = driver.IndexOf("catch", stageTry);
            Assertions.True(recordWrite >= 0 && reportFailure > recordWrite && finalFailure > recordWrite,
                "The final record write must precede the propagated aggregate failures.");
            Assertions.True(stageTry >= 0 && stageCatch > stageTry,
                "Each finalization stage must own an independent catch.");
        }

        internal static void FavoredClassObserverContract()
        {
            string root = Environment.CurrentDirectory;
            string observer = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "WordOfRecallFavoredClassObserver.cs"));
            foreach (string token in new[] {
                "observe-word-of-recall-favored-class",
                "9ba3858327354e2093613efb9de198d7",
                "7249760f01784ea997afaa9c433c2e68",
                "fcb-level6-full-items-recall",
                "fcb-level6-list-identity",
                "fcb-level6-learn-component",
                "fcb-level6-prerequisite",
                "oracle-level6-filtered-cache",
                "fcb-level6-cached-items",
                "oracle-level6-recall-not-locked" })
                Assertions.True(observer.Contains(token),
                    "Favored Class diagnostic observer lacks evidence token: " + token);
        }
    }
}
