using System.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Sprint25CensusTests
    {
        internal static void TheCensusSelectionIsClosedAndExact()
        {
            Sprint25DonorCensusPolicy.Validate();
            string[][] donors = Sprint25DonorCensusPolicy.Donors;
            Assertions.Equal(2, donors.Length,
                "Only the Mastodon rig and one already-measured negative control are surveyed.");
            Assertions.True(donors.Select(value => value[0]).SequenceEqual(
                    new[] { "mastodon",
                        "lizardfolk-free-hands-negative-control" }),
                "The closed donor order is stable and reviewable.");
            Assertions.Equal("028cc6f46e7998f46855a33ffde89567", donors[0][1],
                "The Mastodon blueprint identity is exact.");
            // The Mastodon prefab had never been recorded on this machine, so
            // the first census discovered it rather than asserting a value
            // nobody had measured. The 2026-10-16T13:35Z guarded run measured
            // it, so it is pinned now and discovery is spent: every donor in
            // this closed set is exact, and a changed install fails the census
            // instead of silently re-measuring.
            Assertions.Equal("fe9f25fe62e0f3b49a13910a996470b2",
                donors[0][2],
                "The measured Mastodon prefab is pinned exactly.");
            Assertions.True(donors.All(value =>
                    value[2] != Sprint25DonorCensusPolicy.DiscoverPrefab),
                "No donor discovers any more; the census is fully pinned.");
            Assertions.Equal("d6db6ba3b2c459a48ba0730beef0bac8", donors[1][2],
                "The already-measured control keeps its exact pinned prefab.");

            // Positive controls: a census that cannot find carriers this
            // project already maps is measuring nothing.
            Assertions.Equal(4, Sprint25DonorCensusPolicy.FactAnchors.Length,
                "The exact fact audit is finite.");
            Assertions.True(Sprint25DonorCensusPolicy.FactAnchors
                    .Any(value => value[0] == "Endurance" &&
                        value[1] == "54ee847996c25cd4ba8773d7b8555174"),
                "Endurance is anchored by the identity the released builder already maps.");
            Assertions.True(Sprint25DonorCensusPolicy.FactAnchors
                    .Any(value => value[0] == "Cleave"),
                "Cleave is anchored as the positive control for the Great Cleave question.");

            // Great Cleave and Greater Overrun have no GUID on record, so they
            // are discovered by term rather than anchored.
            Assertions.True(
                Sprint25DonorCensusPolicy.MatchesFeatCandidate("GreatCleave") &&
                Sprint25DonorCensusPolicy.MatchesFeatCandidate("GreaterOverrun") &&
                Sprint25DonorCensusPolicy.MatchesFeatCandidate("ImprovedOverrun"),
                "Both open feat questions are reachable by the discovery terms.");
            Assertions.False(
                Sprint25DonorCensusPolicy.MatchesFeatCandidate("PowerfulChargeFeature"),
                "Feat discovery stays narrow and deterministic.");
            Assertions.True(
                Sprint25DonorCensusPolicy.MaximumDiscoveredFeatBlueprints > 0,
                "Discovery is bounded.");
        }

        internal static void TheCensusIsWiredAsReadOnlyResearch()
        {
            Assertions.True(RuntimeTestScenarioCatalog.Names().Contains(
                    RuntimeTestScenarioCatalog
                        .ObserveExpandedSummoningSprint25DonorCensus),
                "The census scenario is registered in the catalog.");
            Assertions.Equal("observe-expanded-summoning-sprint25-donor-census",
                RuntimeTestScenarioCatalog
                    .ObserveExpandedSummoningSprint25DonorCensus,
                "The scenario name records that it only observes.");
            // An observe- scenario must not be a disposable save-backed run.
            Assertions.True(RuntimeTestScenarioCatalog
                    .ObserveExpandedSummoningSprint25DonorCensus
                    .StartsWith("observe-"),
                "Research observation is not a disposable gameplay scenario.");
        }
    }
}
