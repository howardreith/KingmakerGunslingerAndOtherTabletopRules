using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// What the Sprint 18 primate donor census is allowed to look at.
    ///
    /// <para>The installed library carries no primate unit type at all - the
    /// October 2 census and the Sprint 18 re-run both enumerated every type and
    /// found none - so neither ape can clone a native primate and both bodies
    /// must be original geometry on a non-primate rig. The census therefore
    /// asks a narrower question than "find the ape": which installed rig can
    /// carry a Large, upright, long-armed body with two forelimb contacts and a
    /// bite, without a human or bear gait.</para>
    ///
    /// <para>Targets are fixed here, never supplied by a request. The anchors
    /// are exact blueprints this project already validated; the discovery terms
    /// widen the census to the families the offline pre-census ranked, and the
    /// cap keeps one launch bounded. Everything is read-only metadata from a
    /// detached prefab: no campaign actor is spawned, no asset is modified and
    /// no geometry, texture or animation curve leaves the game.</para>
    /// </summary>
    internal static class PrimateRigSurveyPolicy
    {
        /// <summary>
        /// Exact donors this project has already proved it can load and clone.
        /// The Owlbear is the provisional Sprint 18 donor and the one the apes
        /// currently ride, so its rig is the census baseline rather than a
        /// discovery.
        /// </summary>
        internal static string[][] AnchorDonors
        {
            get
            {
                return new[] {
                    new[] { "owlbear", "d6e0acbdbdb56114898922063ae2cba0" },
                    new[] { "grizzly-bear", "0b214d8e81a563549ba0be37cd1c16d0" },
                    new[] { "dire-bear", "260da5b557e3fb04bb4960a36a5d1dc4" },
                    new[] { "cyclops", "124f1c45ef24d654e9cd420fe84f7f36" }
                };
            }
        }

        internal static string AnchorKey(string guid)
        {
            string[] row = AnchorDonors.FirstOrDefault(value =>
                string.Equals(value[1], guid, StringComparison.Ordinal));
            return row == null ? null : row[0];
        }

        /// <summary>
        /// The families the offline pre-census ranked as credible carriers for
        /// an upright long-armed body. Trolls lead on silhouette and contacts;
        /// the rest are enumerated so the decision is made against what the
        /// installation actually has rather than against a memory of it.
        /// </summary>
        internal static string[] DiscoveryTerms
        {
            get
            {
                return new[] {
                    "troll", "athach", "wildhunt", "wild_hunt", "gargoyle",
                    "souleater", "soul_eater", "ogre", "hillgiant", "stonegiant",
                    "frostgiant", "cloudgiant", "owlbear", "bear", "cyclop",
                    "treant", "golem", "zombie", "spriggan", "boggard"
                };
            }
        }

        /// <summary>
        /// One launch, a bounded census. The cap is on distinct view prefabs,
        /// not blueprints, because fifty troll blueprints share a handful of
        /// rigs and the rig is what the census is about.
        /// </summary>
        internal const int MaximumSurveyedPrefabs = 28;

        /// <summary>
        /// A rig cannot carry an ape unless it is at least this tall in its own
        /// bind frame. Recorded as a measurement, never as a pass mark: the
        /// census reports, and the body is authored afterwards by a person
        /// reading the report.
        /// </summary>
        internal const float MinimumCredibleBindHeight = 0.9f;

        internal static bool Matches(string name, IEnumerable<string> terms)
        {
            if (string.IsNullOrEmpty(name)) return false;
            string folded = name.Replace("_", string.Empty).Replace("-", string.Empty)
                .Replace(" ", string.Empty).ToLowerInvariant();
            return terms.Any(term => folded.IndexOf(
                term.Replace("_", string.Empty).Replace("-", string.Empty),
                StringComparison.Ordinal) >= 0);
        }

        /// <summary>
        /// Deterministic order, so two censuses of one installation produce the
        /// same file. Anchors first in their declared order, then everything
        /// discovered, ordered by blueprint name.
        /// </summary>
        internal static int AnchorRank(string guid)
        {
            string[][] anchors = AnchorDonors;
            for (int index = 0; index < anchors.Length; index++)
                if (string.Equals(anchors[index][1], guid, StringComparison.Ordinal))
                    return index;
            return int.MaxValue;
        }

        internal static void Validate()
        {
            string[][] anchors = AnchorDonors;
            if (anchors.Length != 4 ||
                anchors.Select(value => value[1]).Distinct(StringComparer.Ordinal)
                    .Count() != anchors.Length ||
                anchors.Any(value => value[1].Length != 32))
                throw new InvalidOperationException(
                    "The Sprint 18 census anchors must be four distinct exact donors.");
            if (DiscoveryTerms.Length == 0 || MaximumSurveyedPrefabs < anchors.Length)
                throw new InvalidOperationException(
                    "The census must survey at least its own anchors.");
            if (!Matches("CR9_TrollStandard", DiscoveryTerms) ||
                !Matches("Wild_Hunt_Scout", DiscoveryTerms) ||
                Matches("SummonedViperStandard", DiscoveryTerms))
                throw new InvalidOperationException(
                    "The census discovery terms no longer select the ranked families.");
        }
    }
}
