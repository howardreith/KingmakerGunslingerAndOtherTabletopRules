using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Regression cover for a real defect: the first ideal-roster manifest
    /// recorded coverage as literal data and consulted only
    /// ExpandedSummoningCatalog to fill it. The eleven creatures that ship
    /// solely as retained native wrappers were therefore marked as though no
    /// identity existed for them, and the reuse tests read the same single
    /// catalog, so they encoded the omission instead of catching it.
    ///
    /// These tests fail against that implementation. They assert on the union
    /// of both shipped catalogs and name every retained wrapper creature.
    /// </summary>
    internal static class ExpandedSummoningCoverageTests
    {
        /// <summary>Every creature that ships only as a retained native wrapper.</summary>
        private static readonly string[] NativeWrapperOnlyCreatures = {
            "axiomite", "bogeyman", "frost-giant", "hamadryad", "manticore",
            "mite", "movanic-deva", "nereid", "redcap", "soul-eater",
            "thanadaemon"
        };

        internal static void RetainedNativeCreaturesAreRepresented()
        {
            ExpandedSummoningCoveragePolicy.Validate();

            foreach (string key in NativeWrapperOnlyCreatures)
            {
                // The defect: these resolved to None because only the
                // project-owned catalog was consulted.
                Assertions.Equal(SummonUnitProvenance.NativeWrapper,
                    ExpandedSummoningCoveragePolicy.Provenance(key),
                    "A retained native creature must be represented: " + key);
                Assertions.True(
                    ExpandedSummoningIdealRosterCatalog.Find(key) != null,
                    "A retained native creature must exist in the ideal roster: " + key);
            }

            Assertions.Equal(11,
                ExpandedSummoningCoveragePolicy.NativeWrapperCreatures.Count,
                "The retained native wrapper creature count changed.");

            // Negative control, kept permanently so this test cannot quietly
            // stop discriminating. The reviewed implementation decided coverage
            // by asking only the project-owned catalog. Reproduce that question
            // here and prove it gives the wrong answer for all eleven: if this
            // ever agrees with the derived policy, the policy has regressed to
            // the single-catalog view that caused the defect.
            foreach (string key in NativeWrapperOnlyCreatures)
            {
                bool projectOwnedOnlyView = ExpandedSummoningCatalog.All.Any(
                    value => string.Equals(value.Key, key, StringComparison.Ordinal));
                Assertions.False(projectOwnedOnlyView,
                    "Test setup error: " + key + " is project-owned, so it cannot " +
                    "demonstrate the single-catalog defect.");
                Assertions.True(
                    ExpandedSummoningCoveragePolicy.Provenance(key) !=
                        SummonUnitProvenance.None,
                    "The derived policy regressed to the single-catalog view for: " + key);
            }
        }

        /// <summary>
        /// Coverage counts come from the union of both catalogs, never one.
        /// </summary>
        internal static void RepresentationIsTheUnionOfBothCatalogs()
        {
            Assertions.Equal(74, ExpandedSummoningCatalog.All.Count,
                "Project-owned identities must be preserved.");
            Assertions.Equal(85,
                ExpandedSummoningCoveragePolicy.RepresentedCreatures.Count,
                "Represented creatures must be 74 project-owned plus 11 native wrappers.");
            Assertions.Equal(84,
                ExpandedSummoningCoveragePolicy.PublishedSomewhere.Count,
                "84 creatures are published somewhere; Dire Bat is registered only.");
            Assertions.False(
                ExpandedSummoningCoveragePolicy.PublishedSomewhere.Contains("dire-bat"),
                "Dire Bat must not count as published.");

            int notRepresented = ExpandedSummoningIdealRosterCatalog.All.Count(
                value => ExpandedSummoningCoveragePolicy.Provenance(value.Key) ==
                    SummonUnitProvenance.None);
            Assertions.Equal(60, notRepresented,
                "60 ideal-roster creatures are not represented in the summon roster yet.");
            Assertions.Equal(145,
                ExpandedSummoningCoveragePolicy.RepresentedCreatures.Count + notRepresented,
                "Represented plus unrepresented must account for the whole roster.");
        }

        /// <summary>
        /// Frost Giant is the required regression case. Its unit exists only
        /// as a retained native wrapper; it was published at Summon Monster
        /// VIII by Phase 0 and at Summon Nature's Ally VII by Phase 1 Sprint 3,
        /// through wrappers that reuse the same native unit. Neither family
        /// may be collapsed into the other and no second identity may appear.
        /// </summary>
        internal static void FrostGiantSplitsAcrossFamilies()
        {
            IdealRosterEntry frostGiant =
                ExpandedSummoningIdealRosterCatalog.Find("frost-giant");
            Assertions.True(frostGiant != null, "Frost Giant must be in the roster.");
            Assertions.Equal(8, frostGiant.MonsterTier, "Frost Giant is Summon Monster VIII.");
            Assertions.Equal(7, frostGiant.NaturesAllyTier,
                "Frost Giant is Summon Nature's Ally VII.");

            // Its unit exists - it must never be classified as nonexistent,
            // and it must never become project-owned as well.
            Assertions.Equal(SummonUnitProvenance.NativeWrapper,
                ExpandedSummoningCoveragePolicy.Provenance("frost-giant"),
                "Frost Giant's unit exists as a retained native wrapper.");
            Assertions.False(ExpandedSummoningCatalog.All.Any(value =>
                    value.Key == "frost-giant"),
                "Frost Giant must not gain a project-owned duplicate identity.");

            // Published in both families, each through its own wrappers.
            Assertions.Equal(SummonFamilyCoverage.Published,
                ExpandedSummoningCoveragePolicy.Coverage("frost-giant",
                    SummonFamily.Monster, frostGiant.MonsterTier),
                "Frost Giant is selectable in Summon Monster.");
            Assertions.Equal(SummonFamilyCoverage.Published,
                ExpandedSummoningCoveragePolicy.Coverage("frost-giant",
                    SummonFamily.NaturesAlly, frostGiant.NaturesAllyTier),
                "Frost Giant is selectable in Summon Nature's Ally since Sprint 3.");
            Assertions.True(ExpandedSummoningCoveragePolicy.HasNativeWrapper(
                    "frost-giant", SummonFamily.NaturesAlly),
                "The Nature's Ally Frost Giant wrappers exist.");

            // One native unit behind every wrapper in both families.
            string[] units = SummonNativeExpansionCatalog.All
                .Where(value => value.CreatureKey == "FrostGiant")
                .Select(value => value.UnitGuid).Distinct().ToArray();
            Assertions.Equal(1, units.Length,
                "Every Frost Giant wrapper must reuse the one retained native unit.");
            Assertions.Equal("590cd3d5e76fdc649a5f97bc984cd3c4", units[0],
                "Frost Giant's retained native unit changed.");
        }

        /// <summary>
        /// Per-family coverage must agree with what the catalogs actually offer,
        /// for every creature and both families.
        /// </summary>
        internal static void PerFamilyCoverageAgreesWithTheCatalogs()
        {
            foreach (IdealRosterEntry entry in ExpandedSummoningIdealRosterCatalog.All)
            {
                foreach (SummonFamily family in new[] {
                    SummonFamily.Monster, SummonFamily.NaturesAlly })
                {
                    int? tier = entry.Tier(family);
                    SummonFamilyCoverage coverage =
                        ExpandedSummoningCoveragePolicy.Coverage(entry.Key, family, tier);

                    if (!tier.HasValue)
                    {
                        Assertions.Equal(SummonFamilyCoverage.NotOffered, coverage,
                            "A creature outside a family must not report coverage: " + entry.Key);
                        continue;
                    }

                    Assertions.True(coverage != SummonFamilyCoverage.NotOffered,
                        "A creature the roster places must report real coverage: " + entry.Key);

                    // Anything published must be backed by a real live option.
                    if (coverage == SummonFamilyCoverage.Published)
                    {
                        bool owned = ExpandedSummoningCatalog.All.Any(value =>
                            string.Equals(value.Key, entry.Key, StringComparison.Ordinal) &&
                            (family == SummonFamily.Monster
                                ? value.MonsterTier.HasValue
                                : value.NaturesAllyTier.HasValue));
                        bool wrapped = ExpandedSummoningCoveragePolicy
                            .HasNativeWrapper(entry.Key, family);
                        Assertions.True(owned || wrapped,
                            "Published coverage must name a live option: " + entry.Key);
                    }

                    // Nothing planned may already be selectable.
                    if (coverage == SummonFamilyCoverage.Planned)
                    {
                        Assertions.False(ExpandedSummoningCoveragePolicy
                            .HasNativeWrapper(entry.Key, family),
                            "A planned placement cannot already have a wrapper: " + entry.Key);
                    }
                }
            }
        }

        /// <summary>
        /// The correction must not invent creatures. Every wrapper keeps its
        /// exact native unit GUID and every project-owned identity survives.
        /// </summary>
        internal static void ExistingIdentitiesAreUntouched()
        {
            ExpandedSummoningCatalog.Validate();
            SummonNativeExpansionCatalog.Validate();
            SummonVisibilityCatalog.Validate();

            Assertions.Equal(29, SummonNativeExpansionCatalog.All.Count,
                "Native wrapper placements must be preserved exactly.");
            foreach (SummonNativeExpansionSpec wrapper in SummonNativeExpansionCatalog.All)
            {
                Assertions.Equal(32, wrapper.UnitGuid.Length,
                    "A wrapper lost its exact native unit GUID: " + wrapper.CreatureKey);
                Assertions.Equal(32, wrapper.SourceAbilityGuid.Length,
                    "A wrapper lost its exact source ability GUID: " + wrapper.CreatureKey);
            }

            // The visible surface is unchanged by a bookkeeping correction.
            Assertions.Equal(750,
                ExpandedSummoningBaselineInventory.VisibleChoices(SummonFamily.Monster) +
                ExpandedSummoningBaselineInventory.VisibleChoices(SummonFamily.NaturesAlly),
                "Coverage derivation must not change the shipped visible surface.");
        }
    }
}
