using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Publication-only exclusions. Sprints 9-14 are qualified and published.
    ///
    /// <para>The two Giant Ant castes published on 2026-10-03 under
    /// <c>OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED</c>.
    /// They had been withheld for their printed scent and nothing else - their
    /// mechanics qualified 174/174 - and the owner's ruling covers Scent,
    /// Darkvision and Low-light Vision wherever Kingmaker has no faithful
    /// native carrier, directing that a creature is not to be kept hidden for
    /// one of those three alone. Nothing was implemented to earn that: no
    /// substitute sense, no vision-range override, and no record anywhere
    /// claims the omitted traits work.</para>
    ///
    /// <para>What remains here is Sprint 15's pair, registered ahead of their
    /// own qualification the way every sprint before them did. The Drone
    /// carries the same unmodelled senses and is no longer held for them; it
    /// waits only on its own gates.</para>
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
        // Sprint 14 qualification, and the two Giant Ant castes' keys came out
        // the same day once the owner accepted the passive-sense limitation
        // that was the only thing holding them.
        //
        // What is left is Sprint 15's pair, waiting on their own gates and on
        // nothing else.
        // Empty, and kept rather than deleted: registering a creature
        // ahead of its own qualification and withholding it until the guarded
        // review passes is how every sprint in this phase has worked, and the
        // next one will need it again.
        private static readonly HashSet<string> SuppressedCreatureKeys =
            new HashSet<string>(Array.Empty<string>(),
                StringComparer.Ordinal);

        internal const int RegisteredLogicalPlacementCount = 970;
        // Nothing is withheld. Sprint 15's Giant Ant Drone and Giant Stag
        // Beetle published on their guarded review, which was the last hidden
        // pair in the tranche, so for the first time since Sprint 9 the
        // registered and published surfaces are the same. A creature held for
        // any reason subtracts exactly its own placements, which the Sprint 14
        // and 15 domain suites assert per creature so this arithmetic cannot
        // drift.
        internal const int SuppressedLogicalPlacementCount = 0;
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
