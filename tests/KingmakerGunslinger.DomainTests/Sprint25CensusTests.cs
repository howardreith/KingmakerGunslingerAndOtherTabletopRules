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
            // The Mastodon prefab has never been recorded on this machine, so
            // the census records what the library exposes rather than
            // asserting an unmeasured value. The negative control stays pinned
            // because its prefab IS known, which is what keeps the discovery
            // path honest: only the unknown donor discovers.
            Assertions.Equal(Sprint25DonorCensusPolicy.DiscoverPrefab,
                donors[0][2],
                "The unmeasured Mastodon prefab is discovered, not asserted.");
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
