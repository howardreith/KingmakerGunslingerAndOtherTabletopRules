using System;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// What the Sprint 21 donor census is allowed to look at, and why it is
    /// two censuses rather than one.
    ///
    /// <para>This sprint authors two bodies that are nothing alike. The
    /// Bebelith is a Huge demonic spider-hunter with six hind walking legs and
    /// two elevated barbed foreclaws, a distinct jaw and a web-capable
    /// abdomen. The Giant Crab is a Medium broad shell on a lateral stance
    /// with eight walking legs and two large pincers, no abdomen and no
    /// sting. They share nothing but an exoskeleton, and the easiest wrong
    /// decision available here is to notice that both are arthropods, survey
    /// once, and author both against whichever rig the one survey liked.</para>
    ///
    /// <para>So each family gets its own anchors, its own discovery terms, its
    /// own priority list and its own prefab budget, and the written census
    /// reports them separately. A donor that happens to serve both has to earn
    /// that twice.</para>
    ///
    /// <para>What it measures is what the authoring actually needs: how many
    /// walking-leg chains the rig has and how long they are, whether there is
    /// an abdomen chain to hang a web-spinner on, whether there is a pedipalp
    /// or arm chain a foreclaw or a pincer can be authored onto, and whether
    /// the jaw is a real chain or a painted face. A body authored onto a
    /// chain nobody measured is a guess - which is the lesson Sprint 20 paid
    /// for when the Sprint 14 arachnid capture turned out not to have
    /// survived.</para>
    ///
    /// <para>It reports; it does not decide. There is no pass mark on a donor
    /// here, and no candidate is ranked by anything but the order its anchors
    /// are written in.</para>
    /// </summary>
    internal static class Sprint21DonorSurveyPolicy
    {
        /// <summary>
        /// The Bebelith's released donor. The creature already ships wearing
        /// this rig, so the census starts from the body a player can see
        /// today rather than from a candidate list that ignores it.
        /// </summary>
        internal const string DoomspiderDonorGuid =
            "51c66b0783a748c4b9538f0f0678c4d7";

        /// <summary>
        /// The compact many-legged arthropod rig every shipped Sprint 14, 15
        /// and 20 insect mesh is weighted to. An anchor for both families,
        /// because it is the measurement both authoring decisions are taken
        /// against - not because it is the answer to either.
        /// </summary>
        internal const string GiantSpiderDonorGuid =
            "9e120b5e0ad3c794491c049aa24b9fde";

        internal const string BebelithFamily = "bebelith";
        internal const string GiantCrabFamily = "giant-crab";

        /// <summary>
        /// One family's survey: its anchors, what it searches for, what it
        /// promotes into a prefab slot, and how many slots it may spend.
        /// </summary>
        internal sealed class Family
        {
            internal string Key;
            internal string Question;
            internal string[][] Anchors;
            internal string[] DiscoveryTerms;
            internal string[] PriorityTerms;
            internal int PrefabBudget;
        }

        internal static Family[] Families
        {
            get
            {
                return new[] { Bebelith(), GiantCrab() };
            }
        }

        private static Family Bebelith()
        {
            return new Family {
                Key = BebelithFamily,
                Question =
                    "A Huge demonic spider-hunter: six hind walking legs, two "
                    + "elevated barbed foreclaws, a distinct jaw and a "
                    + "web-capable abdomen. The released body is a Doomspider "
                    + "and the authored one must not be a repaint of it, so "
                    + "this measures what the rig can carry rather than what "
                    + "it currently wears.",
                Anchors = new[] {
                    new[] { "doomspider", DoomspiderDonorGuid },
                    new[] { "giant-spider", GiantSpiderDonorGuid },
                },
                // Demonic and arachnid both, because the creature is both and
                // the rig that carries an elevated foreclaw might be either.
                // Every term is four characters or longer: Sprint 20's first
                // census matched bare "ant" and spent eleven of twelve prefab
                // slots on adamantine golems and a manticore.
                DiscoveryTerms = new[] {
                    "spider", "arachn", "scorpion", "demon", "babau",
                    "vrock", "nabasu", "schir", "kalavakus", "vermin",
                    "centipede", "mantis",
                },
                PriorityTerms = new[] { "spider", "arachn", "demon" },
                PrefabBudget = 8,
            };
        }

        private static Family GiantCrab()
        {
            return new Family {
                Key = GiantCrabFamily,
                Question =
                    "A Medium broad shell on a lateral stance: eight walking "
                    + "legs where the rig permits, two large distinct "
                    + "pincers, no spider abdomen and no scorpion sting. The "
                    + "question is which rig gives eight real leg chains and "
                    + "two arm chains, not which arthropod the other family "
                    + "settled on.",
                Anchors = new[] {
                    new[] { "giant-spider", GiantSpiderDonorGuid },
                },
                DiscoveryTerms = new[] {
                    "crab", "crustac", "lobster", "shrimp", "spider",
                    "centipede", "beetle", "vermin", "arachn",
                },
                PriorityTerms = new[] { "crab", "crustac", "lobster" },
                PrefabBudget = 6,
            };
        }

        /// <summary>
        /// One launch, a fixed ceiling across both families. A census that
        /// could grow with the installation is not bounded.
        /// </summary>
        internal static int MaximumSurveyedPrefabs
        {
            get { return Families.Sum(value => value.PrefabBudget); }
        }

        internal static string AnchorKey(Family family, string guid)
        {
            string[] row = family.Anchors.FirstOrDefault(value =>
                string.Equals(value[1], guid, StringComparison.Ordinal));
            return row == null ? null : row[0];
        }

        internal static int AnchorRank(Family family, string guid)
        {
            for (int index = 0; index < family.Anchors.Length; index++)
                if (string.Equals(family.Anchors[index][1], guid,
                        StringComparison.Ordinal))
                    return index;
            return int.MaxValue;
        }

        /// <summary>
        /// The walking-leg chains, in the naming the shipped insect bodies
        /// already use. Four pairs: the fourth is a real leg on some rigs and
        /// empty on others, which is exactly the thing the crab needs to
        /// know and the thing the Bebelith needs to know it does NOT need.
        /// </summary>
        internal static string[] WalkingLegCandidates
        {
            get
            {
                return new[] {
                    "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0",
                    "L_Leg1_Upper", "L_Leg1_Lower", "L_Foot1",
                    "L_Leg2_Upper", "L_Leg2_Lower", "L_Foot2",
                    "L_Leg3_Upper", "L_Leg3_Lower", "L_Foot3",
                    "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0",
                    "R_Leg1_Upper", "R_Leg1_Lower", "R_Foot1",
                    "R_Leg2_Upper", "R_Leg2_Lower", "R_Foot2",
                    "R_Leg3_Upper", "R_Leg3_Lower", "R_Foot3",
                };
            }
        }

        /// <summary>
        /// The chain a Bebelith's barbed foreclaw or a crab's pincer is
        /// authored onto. Both creatures need an articulated limb forward of
        /// the body that is not a walking leg, and on this rig family that is
        /// the pedipalp chain.
        /// </summary>
        internal static string[] ForelimbCandidates
        {
            get
            {
                return new[] { "pedipalp1_L", "pedipalp2_L", "pedipalp3_L",
                    "pedipalp4_L", "pedipalp5_L", "pedipalp6_L", "pedipalp7_L",
                    "pedipalp1_R", "pedipalp2_R", "pedipalp3_R" };
            }
        }

        /// <summary>
        /// The abdomen chain. The Bebelith needs one - its web comes out of
        /// its abdomen - and the crab must not use one, because a crab with a
        /// spider's abdomen is the single most likely way this sprint's
        /// second body goes wrong.
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
        /// The jaw. The Bebelith's bite is a printed primary attack with its
        /// own damage die, so whether the rig has a real jaw chain or only a
        /// painted face changes how the bite is authored.
        ///
        /// <para>The first run reported no jaw on a rig that plainly has one.
        /// This skeleton spells its legs and feet capitalised and its
        /// pedipalps and cheliceræ lowercase, in one bone roster, and the
        /// candidate list had "Chelicera_L" with a capital. The raw roster
        /// the capture also writes is what exposed it, which is the reason
        /// the capture writes one. Chain matching is case-insensitive now,
        /// everywhere, because the same trap was available on every other
        /// chain.</para>
        /// </summary>
        internal static string[] JawCandidates
        {
            get
            {
                return new[] { "Jaw_M", "Jaw0_M", "Jaw1_M", "chelicera_L",
                    "chelicera_R", "Mandible_L", "Mandible_R", "Head_M" };
            }
        }

        /// <summary>
        /// Whether a bone roster contains a named candidate, ignoring case.
        /// One skeleton here mixes both conventions, so an exact comparison
        /// silently under-reports a chain that exists.
        /// </summary>
        internal static bool HasBone(System.Collections.Generic.ICollection<string> names,
            string candidate)
        {
            if (names == null || string.IsNullOrEmpty(candidate)) return false;
            foreach (string name in names)
                if (string.Equals(name, candidate,
                        StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        internal static bool Matches(string name, string[] terms)
        {
            if (string.IsNullOrEmpty(name) || terms == null) return false;
            string lowered = name.ToLowerInvariant();
            return terms.Any(term => lowered.Contains(term));
        }

        internal static bool IsPriority(Family family, string name)
        { return Matches(name, family.PriorityTerms); }

        /// <summary>
        /// Whether a blueprint is one this mod registered.
        ///
        /// <para>A census of the installation must not count what the mod
        /// being censused just added. Sprint 20's second run reported one
        /// installed scorpion and it was this project's own unit, registered
        /// minutes earlier, which for a while looked like it had overturned
        /// the Sprint 14 finding. This sprint registers a crab, so the same
        /// mistake is available again and is excluded the same way.</para>
        /// </summary>
        internal static bool IsOurs(string name)
        {
            return !string.IsNullOrEmpty(name) &&
                name.StartsWith("KMG_", StringComparison.Ordinal);
        }

        internal static void Validate()
        {
            Family[] families = Families;
            if (families.Length != 2)
                throw new InvalidOperationException(
                    "Sprint 21 authors two bodies that share nothing but an "
                    + "exoskeleton, so it surveys two families.");
            if (families[0].Key != BebelithFamily ||
                families[1].Key != GiantCrabFamily)
                throw new InvalidOperationException(
                    "The Sprint 21 census families changed.");
            foreach (Family family in families)
            {
                if (family.Anchors.Length < 1)
                    throw new InvalidOperationException(
                        "A family with no anchor measures nothing: "
                        + family.Key);
                if (family.PrefabBudget < family.Anchors.Length)
                    throw new InvalidOperationException(
                        "A family's prefab budget cannot exclude its own "
                        + "anchors: " + family.Key);
                if (string.IsNullOrEmpty(family.Question))
                    throw new InvalidOperationException(
                        "A census family states the question it answers: "
                        + family.Key);
                // The substring trap Sprint 20's first census fell into. Any
                // term this short matches words with nothing to do with the
                // creature.
                foreach (string term in family.DiscoveryTerms
                    .Concat(family.PriorityTerms))
                    if (term.Length < 4)
                        throw new InvalidOperationException(
                            "A discovery term shorter than four characters "
                            + "matches unrelated names: " + term);
                foreach (string term in family.PriorityTerms)
                    if (!family.DiscoveryTerms.Contains(term))
                        throw new InvalidOperationException(
                            "A priority term that is not a discovery term can "
                            + "never be promoted: " + term);
            }
            // The crab's survey must be able to find a crab, and the
            // Bebelith's must be able to find a demon. Without these two
            // terms each family is surveying the other's candidates.
            if (!families[1].DiscoveryTerms.Contains("crab"))
                throw new InvalidOperationException(
                    "A crab census that does not look for a crab reports "
                    + "nothing about this sprint's second body.");
            if (!families[0].DiscoveryTerms.Contains("demon"))
                throw new InvalidOperationException(
                    "A Bebelith census that looks only at arachnids has "
                    + "already decided the creature is a spider.");
            // Each family is surveyed separately so a shared anchor has to
            // earn its place in both. Collapsing them would be the exact
            // mistake this sprint is forbidden to make.
            if (families[0].Anchors.Any(value =>
                    value[1] == DoomspiderDonorGuid) &&
                families[1].Anchors.Any(value =>
                    value[1] == DoomspiderDonorGuid))
                throw new InvalidOperationException(
                    "The Doomspider is the Bebelith's released donor and is "
                    + "not a crab candidate.");
            if (MaximumSurveyedPrefabs < 2)
                throw new InvalidOperationException(
                    "The combined prefab cap must admit both families.");
            foreach (string[] chain in new[] { WalkingLegCandidates,
                ForelimbCandidates, AbdomenChainCandidates, JawCandidates })
                if (chain.Length < 2)
                    throw new InvalidOperationException(
                        "Each reported chain needs more than one candidate "
                        + "bone to be worth reporting.");
        }
    }
}
