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
        // Sprint 19 registered the Girallon and the Xill and withheld both
        // through eleven guarded in-game reviews, publishing only once the
        // complete hidden candidate passed: 39 of 39 on the batched mechanics
        // review and 12 of 12 on the party-camera art review at 1d4+1, on one
        // candidate, with the owner installation restored exactly after every
        // run. Nothing moved but these two keys: every identity was allocated
        // at registration.
        // Sprint 20 registered the Giant Scorpion withheld and published it
        // on 2026-10-10, once the complete hidden candidate passed: 20 of 20
        // on the batched mechanics review in both combat modes and 4 of 4 on
        // the party-camera art review, on one candidate, with the owner
        // installation restored exactly after every run. It took three
        // guarded reviews to get there and all three disagreements were
        // measured rather than argued - a printed grab with no carrier, an
        // anti-trip defence eight points where the stat block prints twelve,
        // and a racial +4 that existed only in the arithmetic.
        //
        // It is the first creature in this programme that was templated while
        // withheld, so its twelve placements also held back twenty-four
        // celestial and fiendish execution children, allocated and inert, and
        // publishing the twelve publishes those too. Nothing else moves:
        // removing this key is the whole of publication, every identity having
        // been allocated at registration. The visible surface goes from the
        // 1073 choices v0.0.148 shows to 1085.
        // Sprint 21 registers the Giant Crab and withholds all seven of its
        // roots. Unlike the Sprint 20 scorpion it is not templated and is not
        // on the Summon Monster table at all, so seven suppressed placements
        // hold back seven roots and no execution children. Nothing published
        // before this branch moves: the visible surface stays at the 1085
        // choices v0.0.149 shows.
        // Empty again. Sprint 21's Giant Crab published once its own
        // guarded review passed, which is what every creature in this series
        // has had to do, and publication was the removal of this one key and
        // nothing else.
        // Sprint 23 registered both armored dinosaurs once and withheld all
        // twelve roots through the complete hidden candidate. Publication
        // removes only those two keys: no identity, route or template child
        // is reallocated.
        // Sprint 24 registers both theropods once and withholds exactly their
        // eighteen logical roots. Removing both keys together is the later
        // publication operation; a half-published pair is invalid.
        // Sprint 24 publication, v0.0.155: both theropod keys were removed
        // together after their hidden qualification completed. No identity,
        // route, template child or ordering changed with them.
        // Historical hidden stage: Sprint 25 registered Brachiosaurus once and withheld its
        // three Nature's Ally roots. Like the Sprint 21 Giant Crab and the
        // Sprint 23 Stegosaurus it is untemplated and has no Monster row, so
        // three suppressed placements hold back three roots and no execution
        // children. Nothing published before this branch moves: the visible
        // surface stays at the 1122 choices v0.0.155 shows until this key
        // comes out.
        // Sprint 25 public candidate removes only that key after hidden native
        // mechanics/body and targeted ON/OFF cold-load qualification. The exact
        // public inventory/icon/eleven-root gate still qualifies this artifact.
        // The set is a literal here on purpose. The static validator and the
        // manifest generator both read this set out of the source text, so a
        // symbol reference could make the offline and native surfaces disagree.
        // Validate below retains the exact three SNA-only Brachiosaurus roots.
        // Sprint34 exposes ten routes after hidden native mechanics qualification.
        // Sprint35 registers Barbed Devil and Bone Devil with exactly five
        // Monster routes withheld. Registration still runs so dependency and
        // native-consumer proof can use the executable candidate without a
        // temporary public publication.
        private static readonly HashSet<string> SuppressedCreatureKeys =
            new HashSet<string>(new[] { "barbed-devil", "bone-devil" },
                StringComparer.Ordinal);

        internal const int RegisteredLogicalPlacementCount = 1175;
        internal const int SuppressedLogicalPlacementCount = 5;
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
            string[] sprint29Keys = { "ettin", "cloud-giant", "storm-giant" };
            SummonVariantSpec[] sprint29 = all.Where(value =>
                sprint29Keys.Contains(value.Creature.Key)).ToArray();
            bool sprint29Routes =
                sprint29.Length == 8 &&
                sprint29.All(value => value.Family == SummonFamily.NaturesAlly &&
                    !value.Creature.MonsterTemplated && IsPublished(value)) &&
                sprint29Keys.All(key => sprint29.Where(value =>
                    value.Creature.Key == key).Select(value => value.ParentTier)
                    .OrderBy(value => value).SequenceEqual(key == "ettin" ?
                        new[] { 5, 6, 7, 8, 9 } : key == "cloud-giant" ?
                        new[] { 8, 9 } : new[] { 9 }));
            SummonVariantSpec[] brachiosaurus = all.Where(value =>
                value.Creature.Key == GiantBodyRulesPolicy.BrachiosaurusKey).ToArray();
            SummonVariantSpec[] giants = all.Where(value =>
                value.Creature.Key == "hill-giant" ||
                value.Creature.Key == "stone-giant" ||
                value.Creature.Key == "fire-giant").ToArray();
            bool giantRoutes = new[] { "hill-giant", "stone-giant" }
                .All(key => giants.Where(value => value.Creature.Key == key)
                    .Select(value => value.ParentTier).OrderBy(value => value)
                    .SequenceEqual(new[] { 6, 7, 8, 9 })) &&
                giants.Where(value => value.Creature.Key == "fire-giant")
                    .Select(value => value.ParentTier).OrderBy(value => value)
                    .SequenceEqual(new[] { 7, 8, 9 }) &&
                giants.All(value => value.Family == SummonFamily.NaturesAlly &&
                    !value.Creature.MonsterTemplated);
            if (all.Length != RegisteredLogicalPlacementCount ||
                suppressed.Length != SuppressedLogicalPlacementCount ||
                all.Count(IsPublished) != PublishedLogicalPlacementCount ||
                SuppressedCreatureKeys.Count != 2 ||
                !sprint29Routes || !giantRoutes ||
                !giants.All(IsPublished) ||
                !giants.Select(value => value.Creature.Key).Distinct()
                    .OrderBy(value => value).SequenceEqual(
                        new[] { "fire-giant", "hill-giant", "stone-giant" }) ||
                brachiosaurus.Length != 3 ||
                // Publication adds no Monster row or template child.
                brachiosaurus.Any(value =>
                    value.Family != SummonFamily.NaturesAlly) ||
                brachiosaurus.Any(value => value.Creature.MonsterTemplated) ||
                brachiosaurus.Select(value => value.ParentTier)
                    .OrderBy(value => value)
                    .SequenceEqual(new[] { 7, 8, 9 }) == false)
                throw new InvalidOperationException(
                    "Frozen summon publication visibility catalog changed.");
        }
    }
}
