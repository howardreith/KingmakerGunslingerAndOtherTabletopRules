using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Publication-only exclusions. Sprint 9 publishes the preserved Dire Bat
    /// placements after its own visual, icon, sense and live cast qualification.
    /// </summary>
    internal static class SummonVisibilityCatalog
    {
        private static readonly HashSet<string> SuppressedCreatureKeys =
            new HashSet<string>(StringComparer.Ordinal) { "giant-wasp" };

        internal const int RegisteredLogicalPlacementCount = 825;
        internal const int SuppressedLogicalPlacementCount = 12;
        internal const int PublishedLogicalPlacementCount =
            RegisteredLogicalPlacementCount - SuppressedLogicalPlacementCount;

        internal static bool IsPublished(SummonVariantSpec variant)
        {
            if (variant == null) throw new ArgumentNullException("variant");
            return !SuppressedCreatureKeys.Contains(variant.Creature.Key);
        }

        internal static void Validate()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            SummonVariantSpec[] suppressed = all.Where(value =>
                !IsPublished(value)).ToArray();
            if (all.Length != RegisteredLogicalPlacementCount ||
                suppressed.Length != SuppressedLogicalPlacementCount ||
                suppressed.Any(value => value.Creature.Key != "giant-wasp") ||
                all.Count(IsPublished) != PublishedLogicalPlacementCount)
                throw new InvalidOperationException(
                    "Frozen summon publication visibility catalog changed.");
        }
    }
}
