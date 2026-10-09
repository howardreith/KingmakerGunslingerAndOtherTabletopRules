using System;

namespace KingmakerGunslinger.Acquisition
{
    internal static class CampaignWeaponRecoveryPolicy
    {
        internal const string HistoricalUncertainty = "Historical ownership is unknown: the weapon may have been sold, dropped, or left in an unloaded area. The ledger prevents another recovery grant in this saved state; it does not prove no historical copy exists elsewhere.";

        internal static bool IsOwnedIdentity(string canonicalGuid, string observedGuid)
        {
            return !string.IsNullOrWhiteSpace(canonicalGuid) && !string.IsNullOrWhiteSpace(observedGuid) &&
                (string.Equals(canonicalGuid, observedGuid, StringComparison.Ordinal) ||
                observedGuid.StartsWith(canonicalGuid + "#CraftMagicItems", StringComparison.Ordinal));
        }

        internal static string Refusal(bool relocated, bool moduleEnabled,
            bool inspectionComplete, bool locationVisited, bool recorded,
            string ownedLocation, string availableContainer)
        {
            if (!relocated) return "This weapon has no supported relocation recovery.";
            if (!string.IsNullOrEmpty(ownedLocation)) return "Owned canonical or supported upgraded copy found: " + ownedLocation;
            if (recorded) return "A recovery attempt is already recorded for this weapon in this save.";
            if (!moduleEnabled) return "The owning content module is disabled.";
            if (!inspectionComplete) return "Ownership inspection is incomplete; recovery refused.";
            if (!locationVisited) return "Required campaign progress is missing: visit the destination area normally first.";
            if (!string.IsNullOrEmpty(availableContainer)) return "The weapon remains available in a known container: " + availableContainer;
            return null;
        }
    }
}
