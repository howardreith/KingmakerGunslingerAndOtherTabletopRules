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

        internal string CoreStatus
        { get { return "Available  native Barbarian feature; Call of the Wild is optional"; } }

        internal string InteroperabilityStatus
        {
            get
            {
                if (Surface == UrbanCotwSurface.Absent)
                    return "Not applicable  Call of the Wild not detected";
                return InteroperabilityQualified ?
                    "Qualified  exact optional Rage interoperability detected" :
                    "Unqualified  optional bridge disabled; Urban core remains available";
            }
        }
    }

    internal static class UrbanCotwCompatibilityPolicy
    {
        internal static UrbanCotwCompatibilityDecision Evaluate(
            UrbanCotwSurface surface, bool exactNativeLifecycleRetained,
            bool markerRetained, bool duplicateBehavior,
            string failedStructuralCheck = "unspecified-structural-check")
        {
            if (surface == UrbanCotwSurface.Absent)
                return new UrbanCotwCompatibilityDecision(surface, true, false,
                    "Call of the Wild absent; interoperability not applicable.");
            bool qualified = surface == UrbanCotwSurface.Supported &&
                exactNativeLifecycleRetained && markerRetained && !duplicateBehavior;
            string diagnostic = qualified ?
                "Finalized native Rage lifecycle and marker are retained; no adapter required." :
                "Optional Call of the Wild Rage surface failed structural check '" +
                    failedStructuralCheck +
                    "'; optional interoperability is unqualified and Urban core remains available.";
            return new UrbanCotwCompatibilityDecision(surface, true, qualified,
                diagnostic);
        }
    }

    internal static class UrbanCotwCompatibilityStatusRegistry
    {
        private static readonly object Sync = new object();
        private static UrbanCotwCompatibilityDecision _current =
            UrbanCotwCompatibilityPolicy.Evaluate(UrbanCotwSurface.Absent,
                true, true, false);

        internal static UrbanCotwCompatibilityDecision Current
        { get { lock (Sync) return _current; } }

        internal static void Update(UrbanCotwCompatibilityDecision decision)
        {
            if (decision == null) return;
            lock (Sync) _current = decision;
        }
    }
}
