using System;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// What the Sprint 20 arachnid donor census is allowed to look at.
    ///
    /// <para>This census asks a narrower question than Sprint 18's. Sprint 18
    /// had to <em>choose</em> a rig from a field of candidates; Sprint 20
    /// already knows its donor, because the Sprint 14 census established that
    /// Kingmaker has no scorpion and that the Giant Spider is the only compact
    /// many-legged arthropod the game has. What Sprint 20 needs is the rig's
    /// measurements - the full bone roster, the bind-pose transforms, and how
    /// many segments the abdomen chain has, because a scorpion's metasoma is
    /// authored onto that chain.</para>
    ///
    /// <para>Sprint 19 skipped a census because Sprint 18's capture survived
    /// under runtime-evidence and could be re-decoded offline. The Sprint 14
    /// arachnid capture did not survive: its provenance records that the
    /// request-local capture stays outside Git and the package. The shipped
    /// insect meshes give the bone names those creatures used and the rig
    /// fingerprint, but not the roster, the bind frame or the chain length,
    /// and a tail authored onto a chain nobody measured is a guess.</para>
    ///
    /// <para>So the survey is deliberately one anchor wide. A handful of
    /// discovery terms accompany it only so the conclusion is drawn against
    /// what the installation actually has rather than against a memory of the
    /// Sprint 14 finding - if some other arthropod rig has appeared, this
    /// census will say so.</para>
    /// </summary>
    internal static class ArachnidRigSurveyPolicy
    {
        /// <summary>
        /// The one donor this sprint authors against. Pinned by asset id, not
        /// by name: it is the exact renderer every shipped Sprint 14 and 15
        /// insect mesh is weighted to.
        /// </summary>
        internal const string GiantSpiderDonorGuid =
            "9e120b5e0ad3c794491c049aa24b9fde";

        /// <summary>
        /// The rig fingerprint the shipped insect meshes record. The census
        /// cannot verify this directly - it is a hash of a capture, not of a
        /// prefab - but it is carried here so the written census names the
        /// rig the offline bodies already claim.
        /// </summary>
        internal const string RecordedInsectRigSha256 =
            "60074cc2edb939cd4ab3e49961eec470866f94096d40e97f490dedae60faa19b";

        internal static string[][] AnchorDonors
        {
            get
            {
                return new[] {
                    new[] { "giant-spider", GiantSpiderDonorGuid }
                };
            }
        }

        internal static string AnchorKey(string guid)
        {
            string[] row = AnchorDonors.FirstOrDefault(value =>
                string.Equals(value[1], guid, StringComparison.Ordinal));
            return row == null ? null : row[0];
        }

        internal static int AnchorRank(string guid)
        {
            for (int index = 0; index < AnchorDonors.Length; index++)
                if (string.Equals(AnchorDonors[index][1], guid,
                        StringComparison.Ordinal))
                    return index;
            return int.MaxValue;
        }

        /// <summary>
        /// Every arthropod term the installation might answer to. Not a
        /// shortlist of candidates - the donor is already decided - but a
        /// check that the Sprint 14 finding still holds.
        /// </summary>
        internal static string[] DiscoveryTerms
        {
            get
            {
                return new[] {
                    "spider", "scorpion", "centipede", "vermin", "insect",
                    "beetle", "ant", "crab", "arachn", "wasp", "mantis",
                    "swarm"
                };
            }
        }

        /// <summary>
        /// One launch, a fixed ceiling. A census that could grow with the
        /// installation is not bounded.
        /// </summary>
        internal const int MaximumSurveyedPrefabs = 12;

        /// <summary>
        /// The abdomen chain a scorpion's metasoma is authored onto. The
        /// census reports how many of these the rig actually has; this is the
        /// set it looks for, in the project's standard tail naming.
        /// </summary>
        internal static string[] AbdomenChainCandidates
        {
            get
            {
                return new[] { "Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M",
                    "Tail4_M", "Tail5_M" };
            }
        }

        /// <summary>
        /// The pedipalp chain a scorpion's chelae are authored onto, and the
        /// fourth leg pair that is a real leg here and is empty on the ants.
        /// Reported, never required: the census states what the rig has.
        /// </summary>
        internal static string[] ChelaChainCandidates
        {
            get
            {
                return new[] { "pedipalp1_L", "pedipalp2_L", "pedipalp3_L",
                    "pedipalp4_L", "pedipalp5_L", "pedipalp6_L", "pedipalp7_L" };
            }
        }

        internal static string[] FourthLegCandidates
        {
            get
            {
                return new[] { "L_Leg3_Upper", "L_Leg3_Lower", "L_Foot3",
                    "R_Leg3_Upper", "R_Leg3_Lower", "R_Foot3" };
            }
        }

        internal static bool Matches(string name, string[] terms)
        {
            if (string.IsNullOrEmpty(name) || terms == null) return false;
            string lowered = name.ToLowerInvariant();
            return terms.Any(term => lowered.Contains(term));
        }

        internal static void Validate()
        {
            if (AnchorDonors.Length != 1)
                throw new InvalidOperationException(
                    "The Sprint 20 census surveys one known donor; it does not "
                    + "choose between candidates.");
            if (AnchorDonors[0][1] != GiantSpiderDonorGuid)
                throw new InvalidOperationException(
                    "The Sprint 20 census anchor is the Giant Spider renderer "
                    + "every shipped insect mesh is weighted to.");
            if (MaximumSurveyedPrefabs < AnchorDonors.Length)
                throw new InvalidOperationException(
                    "The prefab cap cannot exclude the anchor.");
            if (!DiscoveryTerms.Contains("scorpion"))
                throw new InvalidOperationException(
                    "A census that does not look for a scorpion cannot report "
                    + "that the game has none.");
            if (AbdomenChainCandidates.Length < 2 ||
                ChelaChainCandidates.Length < 2 ||
                FourthLegCandidates.Length < 2)
                throw new InvalidOperationException(
                    "Each reported chain needs more than one candidate bone to "
                    + "be worth reporting.");
        }
    }
}
