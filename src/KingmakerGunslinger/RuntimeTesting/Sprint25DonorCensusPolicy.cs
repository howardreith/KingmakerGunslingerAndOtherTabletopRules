using System;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Closed, source-controlled inputs for the one Sprint 25 quadruped donor
    /// census. Like the Sprint 24 theropod census this is intentionally not a
    /// name-driven donor search: these are the exact recorded candidates whose
    /// rest, bind and action metadata is needed before a sauropod or an
    /// elephant body can be authored.
    ///
    /// <para>This research observation proves donor structure only. It does
    /// not qualify either Sprint 25 creature, and it is never published.</para>
    /// </summary>
    internal static class Sprint25DonorCensusPolicy
    {
        /// <summary>
        /// The Mastodon's prefab identity has never been recorded on this
        /// machine, so the census records what the installed library actually
        /// exposes instead of asserting a value nobody has measured. A donor
        /// whose prefab IS already known stays pinned, and the discovered
        /// value is pinned in turn once this census has run.
        /// </summary>
        internal const string DiscoverPrefab = "discover";

        internal static readonly string[][] Donors = {
            // The quadruped under test. The released Elephant, Triceratops,
            // Rhinoceros and Woolly Rhinoceros already ride this rig, so it is
            // the existing precedent rather than a new choice - but whether its
            // trunk and neck chains can carry a credible long-necked sauropod
            // is exactly what this census has to answer rather than assume.
            new[] {
                "mastodon",
                "028cc6f46e7998f46855a33ffde89567",
                DiscoverPrefab
            },
            // Negative control: the already-measured Sprint 24 biped. Its
            // hierarchy is known, so if the capture cannot tell a two-legged
            // donor from a four-legged one the difference shows here rather
            // than in authored geometry.
            new[] {
                "lizardfolk-free-hands-negative-control",
                "a8cbd4cdb0feaf64eab1452257c3c971",
                "d6db6ba3b2c459a48ba0730beef0bac8"
            }
        };

        /// <summary>
        /// Exact installed-engine identities. Endurance and Cleave are
        /// positive controls: both are already mapped by the released natural
        /// builder, so a census that cannot find them is measuring nothing.
        /// </summary>
        internal static readonly string[][] FactAnchors = {
            new[] { "Endurance", "54ee847996c25cd4ba8773d7b8555174" },
            new[] { "Cleave", "d809b6c4ff2aaff4fa70d712a70f7d7b" },
            new[] { "PowerAttack", "9972f33f977fc724c838e59641b2fca5" },
            new[] { "ImprovedBullRush", "b3614622866fe7046b787a548bbd7f59" }
        };

        /// <summary>
        /// Great Cleave and Greater Overrun have no project alias and no GUID
        /// on record, so they cannot be anchored. The census enumerates what
        /// the installed library actually exposes under these terms, with each
        /// candidate's component graph, which is what separates "this project
        /// never mapped it" from "the engine does not implement it".
        /// </summary>
        internal static readonly string[] FeatDiscoveryTerms = {
            "Cleave", "Overrun"
        };

        internal const int MaximumDiscoveredFeatBlueprints = 64;

        internal static bool MatchesFeatCandidate(string name)
        {
            return !string.IsNullOrEmpty(name) && FeatDiscoveryTerms.Any(term =>
                name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        internal static void Validate()
        {
            if (Donors.Length != 2 || Donors.Any(value => value.Length != 3) ||
                Donors.Select(value => value[1]).Any(value =>
                    value.Length != 32 || value.Any(c => !Uri.IsHexDigit(c))) ||
                Donors.Select(value => value[1]).Distinct().Count() != Donors.Length ||
                Donors.Any(value => value[2] != DiscoverPrefab &&
                    (value[2].Length != 32 || value[2].Any(c => !Uri.IsHexDigit(c)))))
                throw new InvalidOperationException(
                    "Sprint 25 closed quadruped donor selection is invalid.");

            if (FactAnchors.Length != 4 ||
                FactAnchors.Any(value => value.Length != 2 ||
                    value[1].Length != 32 || value[1].Any(c => !Uri.IsHexDigit(c))) ||
                FactAnchors.Select(value => value[0]).Distinct().Count() !=
                    FactAnchors.Length)
                throw new InvalidOperationException(
                    "Sprint 25 census fact anchors are invalid.");

            if (FeatDiscoveryTerms.Length != 2 ||
                FeatDiscoveryTerms.Any(string.IsNullOrWhiteSpace) ||
                MaximumDiscoveredFeatBlueprints <= 0)
                throw new InvalidOperationException(
                    "Sprint 25 census feat discovery terms are invalid.");
        }
    }
}
