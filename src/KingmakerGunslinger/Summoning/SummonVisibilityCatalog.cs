using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Publication-only exclusions. Sprints 9-13 are qualified and published.
    /// The three Sprint 14 insects are registered ahead of their own
    /// qualification, the way the Shadow Mastiff was in Sprint 13, so their
    /// identities are allocated once and never move; their placements are
    /// withheld until their mechanics, visual identity and lifecycle qualify.
    /// The published surface is unchanged while they are suppressed, and it is
    /// the published count rather than the registered one that every roster,
    /// census and player-path gate reconciles against.
    /// </summary>
    internal static class SummonVisibilityCatalog
    {
        // Sprint 14's three insects. A creature is suppressed by being named
        // here rather than by being left out of the catalog, so its identities
        // are allocated once and are never reallocated when it publishes.
        // Removing a key here is what publishes a creature, and that may only
        // happen once its own gates have passed on the head that publishes it.
        private static readonly HashSet<string> SuppressedCreatureKeys =
            new HashSet<string>(new[] {
                "fire-beetle", "giant-ant-worker", "giant-ant-soldier"
            }, StringComparer.Ordinal);

        internal const int RegisteredLogicalPlacementCount = 952;
        internal const int SuppressedLogicalPlacementCount = 48;
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
