using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Publication-only exclusions. Sprints 9-13 are qualified and published,
    /// and so is Sprint 14's Fire Beetle.
    ///
    /// <para>The two Giant Ant castes are still here, and not because their
    /// mechanics failed. They qualified: printed Perception +5 exactly, the
    /// printed trip defence, a sting that is its own weapon type, a grab on the
    /// bite alone, an injury poison that a wound delivers and a zero-damage hit
    /// does not, and the vermin mind-affecting immunity proved by a buff the
    /// caster accepts and they refuse. What they cannot have is scent, because
    /// Kingmaker has no scent mechanic: the whole loaded blueprint library
    /// carries no component that could express it, and the engine's only sense
    /// plumbing is AddBlindsight with UnitPartBlindsense, which is a different
    /// rule. Substituting it would be that different rule wearing the right
    /// name. The Sprint 14 order's instruction for exactly this outcome is to
    /// stop publication of the affected creature with the engine evidence, so
    /// the castes stay suppressed pending one owner ruling, recorded as a
    /// blocker. No accepted-limitation label is created without him.</para>
    ///
    /// <para>Suppression is by name here rather than by omission from the
    /// catalog, so a held creature's identities are allocated once and are
    /// never reallocated when it publishes. The published surface is what every
    /// roster, census and player-path gate reconciles against.</para>
    /// </summary>
    internal static class SummonVisibilityCatalog
    {
        // Removing a key here is what publishes a creature, and that may only
        // happen once its own gates have passed on the head that publishes it.
        // The Fire Beetle's key came out on 2026-10-03 after the batched
        // Sprint 14 qualification; the two ant castes stay for the scent
        // barrier described above, which is an engine fact and not a defect in
        // them.
        private static readonly HashSet<string> SuppressedCreatureKeys =
            new HashSet<string>(new[] {
                "giant-ant-worker", "giant-ant-soldier"
            }, StringComparer.Ordinal);

        internal const int RegisteredLogicalPlacementCount = 952;
        // The worker's 16 placements and the soldier's 14. The Fire Beetle's 18
        // are published. A creature held for a proven engine barrier subtracts
        // exactly its own placements, which the Sprint 14 domain suite asserts
        // per creature so this arithmetic cannot drift.
        internal const int SuppressedLogicalPlacementCount = 30;
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
