using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Publication-only exclusions. Sprints 9-15 are qualified and published.
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
    /// <para>Sprint 16's complete hidden candidate e3aeae63 passed all six
    /// guarded stages and exact restoration. The publication candidate removes
    /// only Dire Crocodile; its six public routes and the fourteen preserved
    /// Crocodile routes must pass the exact publication artifact gate.</para>
    ///
    /// <para>Suppression is by name here rather than by omission from the
    /// catalog, so a held creature's identities are allocated once and are
    /// never reallocated when it publishes. The published surface is what every
    /// roster, census and player-path gate reconciles against.</para>
    /// </summary>
    internal static class SummonVisibilityCatalog
    {
        // Removing a key creates a publication candidate after hidden gates
        // pass. Completion additionally requires the exact public-route gate.
        // The Fire Beetle's key came out on 2026-10-03 after the batched
        // Sprint 14 qualification, and the two Giant Ant castes' keys came out
        // the same day once the owner accepted the passive-sense limitation
        // that was the only thing holding them.
        //
        // The exact 47e8c121 hidden batch qualifies both new snakes independently.
        // This is their publication candidate, not a full Sprint 17 PASS:
        // Salamander's retained reach-observation failure still blocks closure.
        // Existing Salamander identities and five published roots do not move.
        //
        // The two Sprint 18 ape keys came out on 2026-10-09 after the complete
        // hidden candidate passed: 29 of 29 on the batched review and 12 of 12
        // on the party-camera art review at 1d4+1, on one candidate, with the
        // owner installation restored exactly after every run. Nothing moved
        // but these two keys: every identity was allocated at registration.
        private static readonly HashSet<string> SuppressedCreatureKeys =
            new HashSet<string>(new string[0], StringComparer.Ordinal);

        internal const int RegisteredLogicalPlacementCount = 1034;
        // Sprint 18 registered the Ape's fourteen roots and the Dire Ape's
        // twelve and held all twenty-six until the complete hidden candidate
        // passed. It has, so nothing is withheld: every registered placement
        // is published, and publication moved nothing but two names.
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
