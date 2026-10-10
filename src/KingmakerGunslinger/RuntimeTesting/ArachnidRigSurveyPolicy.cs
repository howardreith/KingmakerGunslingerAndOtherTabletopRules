using System;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// What the Sprint 20 arachnid donor census is allowed to look at.
    ///
    /// <para>It was written expecting a narrower question than Sprint 18's.
    /// Sprint 18 had to <em>choose</em> a rig from a field of candidates, and
    /// Sprint 20 believed its donor was already settled: the Sprint 14 census
    /// recorded that Kingmaker has no scorpion and that the Giant Spider is
    /// the only compact many-legged arthropod the game has.</para>
    ///
    /// <para>The first run disproved that. One scorpion blueprint exists in
    /// the live library, which is exactly the kind of thing a census is for
    /// and exactly what re-reading Sprint 14's notes would never have found.
    /// So this is a choosing census after all, and the measurements it was
    /// written for - the full bone roster, the bind frame, and the length of
    /// the abdomen chain a metasoma is authored onto - are now taken for
    /// whatever candidates it finds rather than for one assumed donor.</para>
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
        ///
        /// <para>The first census run matched on bare "ant" and spent eleven
        /// of its twelve prefab slots on adamANTine golems, a treANT, a
        /// mANTicore and a peasANT, so the arthropods it was meant to find
        /// never reached the survey. A term this short is a substring trap;
        /// the ants this project already ships are reached through the Giant
        /// Spider anchor anyway, so the term is simply gone.</para>
        /// </summary>
        internal static string[] DiscoveryTerms
        {
            get
            {
                return new[] {
                    "spider", "scorpion", "centipede", "beetle", "arachn",
                    "crab", "mantis", "wasp", "vermin", "swarm"
                };
            }
        }

        /// <summary>
        /// Names that make a blueprint worth a prefab slot ahead of the rest.
        /// A scorpion rig, if one exists, changes this sprint's whole visual
        /// decision, so it is measured rather than merely counted - the first
        /// run reported that one exists and could not say what it was.
        /// </summary>
        internal static string[] PriorityTerms
        {
            get { return new[] { "scorpion", "spider", "arachn" }; }
        }

        internal static bool IsPriority(string name)
        { return Matches(name, PriorityTerms); }

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
            if (!DiscoveryTerms.Contains("scorpion") ||
                !PriorityTerms.Contains("scorpion"))
                throw new InvalidOperationException(
                    "A census that does not look for a scorpion, and give it a "
                    + "prefab slot, cannot report what the game has.");
            // The substring trap the first run fell into. Any term this short
            // matches words that have nothing to do with arthropods.
            foreach (string term in DiscoveryTerms.Concat(PriorityTerms))
                if (term.Length < 4)
                    throw new InvalidOperationException(
                        "A discovery term shorter than four characters matches "
                        + "unrelated names: " + term);
            if (AbdomenChainCandidates.Length < 2 ||
                ChelaChainCandidates.Length < 2 ||
                FourthLegCandidates.Length < 2)
                throw new InvalidOperationException(
                    "Each reported chain needs more than one candidate bone to "
                    + "be worth reporting.");
        }
    }
}
