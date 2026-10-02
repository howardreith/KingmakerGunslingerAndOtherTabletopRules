using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Publication-only exclusions. Sprints 9-12 are qualified and published.
    /// The Sprint 13 Shadow Mastiff is registered ahead of its own
    /// qualification so its identities are allocated once and never move, and
    /// its four placements are withheld until its mechanics, visual identity
    /// and lifecycle qualify. The published surface is unchanged while it is
    /// suppressed.
    /// </summary>
    internal static class SummonVisibilityCatalog
    {
        private static readonly HashSet<string> SuppressedCreatureKeys =
            new HashSet<string>(new[] { "shadow-mastiff" },
                StringComparer.Ordinal);

        internal const int RegisteredLogicalPlacementCount = 904;
        internal const int SuppressedLogicalPlacementCount = 4;
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
                all.Count(IsPublished) != PublishedLogicalPlacementCount)
                throw new InvalidOperationException(
                    "Frozen summon publication visibility catalog changed.");
        }
    }
}
