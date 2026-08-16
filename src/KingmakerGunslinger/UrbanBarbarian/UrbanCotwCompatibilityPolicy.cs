namespace KingmakerGunslinger.UrbanBarbarian
{
    internal enum UrbanCotwSurface
    {
        Absent,
        Supported,
        Unknown,
        Ambiguous
    }

    internal sealed class UrbanCotwCompatibilityDecision
    {
        internal UrbanCotwCompatibilityDecision(UrbanCotwSurface surface,
            bool coreAvailable, bool interoperabilityQualified, string diagnostic)
        {
            Surface = surface;
            CoreAvailable = coreAvailable;
            InteroperabilityQualified = interoperabilityQualified;
            Diagnostic = diagnostic;
        }

        internal UrbanCotwSurface Surface { get; private set; }
        internal bool CoreAvailable { get; private set; }
        internal bool InteroperabilityQualified { get; private set; }
        internal string Diagnostic { get; private set; }
    }

    internal static class UrbanCotwCompatibilityPolicy
    {
        internal static UrbanCotwCompatibilityDecision Evaluate(
            UrbanCotwSurface surface, bool exactNativeLifecycleRetained,
            bool markerRetained, bool duplicateBehavior)
        {
            if (surface == UrbanCotwSurface.Absent)
                return new UrbanCotwCompatibilityDecision(surface, true, false,
                    "Call of the Wild absent; interoperability not applicable.");
            bool qualified = surface == UrbanCotwSurface.Supported &&
                exactNativeLifecycleRetained && markerRetained && !duplicateBehavior;
            string diagnostic = qualified ?
                "Finalized native Rage lifecycle and marker are retained; no adapter required." :
                "Optional Call of the Wild Rage surface is unqualified; Urban core remains available.";
            return new UrbanCotwCompatibilityDecision(surface, true, qualified,
                diagnostic);
        }
    }
}
