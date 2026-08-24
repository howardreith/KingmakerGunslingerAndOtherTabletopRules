using System;

namespace KingmakerGunslinger.Compatibility
{
    internal enum CompatibilityAssetFamily
    {
        Firearms,
        ElvenBranchedSpears,
        EasternWeapons
    }

    /// <summary>
    /// Immutable, request-local asset-family plan for the compatibility audit.
    /// The plan has no default activation path; callers must first pass the
    /// guarded runtime-request parser and the exact scenario allowlist.
    /// </summary>
    internal sealed class CompatibilityAssetAttributionPlan
    {
        internal const string AllSuppressed = "all-suppressed";
        internal const string FirearmsOnly = "firearms-only";
        internal const string SpearsOnly = "spears-only";
        internal const string EasternOnly = "eastern-only";
        internal const string AllEnabled = "all-enabled";

        private CompatibilityAssetAttributionPlan(
            string configuration,
            bool firearms,
            bool spears,
            bool eastern)
        {
            Configuration = configuration;
            FirearmsEnabled = firearms;
            ElvenBranchedSpearsEnabled = spears;
            EasternWeaponsEnabled = eastern;
        }

        internal string Configuration { get; private set; }
        internal bool FirearmsEnabled { get; private set; }
        internal bool ElvenBranchedSpearsEnabled { get; private set; }
        internal bool EasternWeaponsEnabled { get; private set; }

        internal bool IsEnabled(CompatibilityAssetFamily family)
        {
            switch (family)
            {
                case CompatibilityAssetFamily.Firearms:
                    return FirearmsEnabled;
                case CompatibilityAssetFamily.ElvenBranchedSpears:
                    return ElvenBranchedSpearsEnabled;
                case CompatibilityAssetFamily.EasternWeapons:
                    return EasternWeaponsEnabled;
                default:
                    throw new ArgumentOutOfRangeException("family");
            }
        }

        internal static bool TryResolve(
            string configuration,
            out CompatibilityAssetAttributionPlan plan)
        {
            plan = null;
            if (string.Equals(configuration, AllSuppressed,
                StringComparison.Ordinal))
            {
                plan = new CompatibilityAssetAttributionPlan(
                    configuration, false, false, false);
            }
            else if (string.Equals(configuration, FirearmsOnly,
                StringComparison.Ordinal))
            {
                plan = new CompatibilityAssetAttributionPlan(
                    configuration, true, false, false);
            }
            else if (string.Equals(configuration, SpearsOnly,
                StringComparison.Ordinal))
            {
                plan = new CompatibilityAssetAttributionPlan(
                    configuration, false, true, false);
            }
            else if (string.Equals(configuration, EasternOnly,
                StringComparison.Ordinal))
            {
                plan = new CompatibilityAssetAttributionPlan(
                    configuration, false, false, true);
            }
            else if (string.Equals(configuration, AllEnabled,
                StringComparison.Ordinal))
            {
                plan = new CompatibilityAssetAttributionPlan(
                    configuration, true, true, true);
            }
            return plan != null;
        }
    }
}
