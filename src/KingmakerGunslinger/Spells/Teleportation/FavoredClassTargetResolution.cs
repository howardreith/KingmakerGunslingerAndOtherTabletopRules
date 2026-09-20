using Kingmaker.Blueprints.Classes.Selection;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Outcome of resolving the optional Favored Class per-level feature
    // target. The reconciler treats these distinctions mechanically:
    // absent is a safe no-op for the optional integration; malformed and
    // ambiguous fail closed without any mutation and carry a diagnostic.
    internal sealed class FavoredClassTargetResolution
    {
        internal FavoredClassTargetResolution(string outcome,
            BlueprintParametrizedFeature target, string detail)
        { Outcome = outcome; Target = target; Detail = detail; }
        // "resolved", "absent", "malformed" or "ambiguous".
        internal string Outcome { get; private set; }
        // Non-null only for "resolved".
        internal BlueprintParametrizedFeature Target { get; private set; }
        internal string Detail { get; private set; }

        internal static FavoredClassTargetResolution Resolved(
            BlueprintParametrizedFeature target, string detail)
        { return new FavoredClassTargetResolution("resolved", target, detail); }
        internal static FavoredClassTargetResolution Absent(string detail)
        { return new FavoredClassTargetResolution("absent", null, detail); }
        internal static FavoredClassTargetResolution Malformed(string detail)
        { return new FavoredClassTargetResolution("malformed", null, detail); }
        internal static FavoredClassTargetResolution Ambiguous(string detail)
        { return new FavoredClassTargetResolution("ambiguous", null, detail); }
    }
}
