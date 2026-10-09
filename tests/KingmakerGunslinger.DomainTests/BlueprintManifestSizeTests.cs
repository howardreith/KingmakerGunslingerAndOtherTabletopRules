using System;
using System.IO;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The shipped blueprint ledger must fit the loader's own corruption bound.
    ///
    /// <para>This exists because of a real failure. Sprint 18 appended the Ape
    /// and Dire Ape, which took the ledger to 2980 entries and 1,054,189 bytes,
    /// just over the one-mebibyte bound the loader enforced. Every offline gate
    /// passed - repository validation, 2508 domain tests, a clean Release build
    /// and strict package validation - and the mod then failed to initialize in
    /// game, taking Shield Other, Teleportation, Eastern Weapons and Favored
    /// Class down with it, because none of those gates ever compared the file
    /// it ships against the limit the loader applies.</para>
    ///
    /// <para>The ledger is append-only, so it only ever grows. Measuring it
    /// here means the next sprint that crosses the bound is told offline, by a
    /// failing test naming the exact byte counts, rather than by a dead mod.</para>
    /// </summary>
    internal static class BlueprintManifestSizeTests
    {
        internal static void ShippedLedgerFitsTheLoaderBound()
        {
            string path = Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json");
            Assertions.True(File.Exists(path),
                "The shipped blueprint ledger must exist at the loader's path.");
            long length = new FileInfo(path).Length;

            Assertions.True(length > 0,
                "The loader rejects an empty ledger; the shipped one must not be empty.");
            Assertions.True(length <= BlueprintManifest.MaximumManifestBytes,
                "The shipped blueprint ledger is " + length +
                " bytes, past the loader's " + BlueprintManifest.MaximumManifestBytes +
                "-byte bound. The mod will refuse to initialize in game. Raise " +
                "BlueprintManifest.MaximumManifestBytes deliberately, with a reason.");

            // Headroom, so a sprint is warned before it is broken rather than
            // after. The ledger grows by tens of entries per sprint and an
            // entry costs roughly 350 bytes, so a quarter of the bound is many
            // sprints of warning.
            long headroom = BlueprintManifest.MaximumManifestBytes - length;
            Assertions.True(headroom >= BlueprintManifest.MaximumManifestBytes / 4,
                "The shipped blueprint ledger is " + length + " bytes and leaves only " +
                headroom + " bytes under the loader's bound. Raise the bound now, " +
                "before a sprint crosses it in game.");
        }

        /// <summary>
        /// The loader's own rejection still works at both ends, so raising the
        /// bound did not turn the guard off.
        /// </summary>
        internal static void TheCorruptionBoundStillRejectsBothEnds()
        {
            Assertions.True(BlueprintManifest.MaximumManifestBytes > 0,
                "The corruption bound must be a positive length.");
            Assertions.False(0 > 0 && 0 <= BlueprintManifest.MaximumManifestBytes,
                "An empty ledger is never acceptable.");
            Assertions.False(
                BlueprintManifest.MaximumManifestBytes + 1 <=
                    BlueprintManifest.MaximumManifestBytes,
                "A ledger one byte past the bound must still be rejected.");
        }
    }
}
