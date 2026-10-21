using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum StandaloneElementalConsumerRoute
    {
        Unknown, OrdinaryClassLearning, OrdinarySpellbookDiscovery,
        ExistingKnownSpell, PreparedSlot, PinnedHotbar, DomainOrSpecialGrant,
        Item, NpcOrAi
    }

    internal sealed class StandaloneElementalAbilitySpec
    {
        internal StandaloneElementalAbilitySpec(string guid, string name, int tier,
            string parentGuid, string[] creatureKeys, string monsterParent, string allyParent,
            bool uniqueNpc = false)
        {
            Guid = guid; InternalName = name; CanonicalTier = tier; ParentGuid = parentGuid;
            CreatureKeys = Array.AsReadOnly(creatureKeys); MonsterParentGuid = monsterParent;
            NaturesAllyParentGuid = allyParent; UniqueNpc = uniqueNpc;
        }
        internal string Guid { get; private set; }
        internal string InternalName { get; private set; }
        internal int CanonicalTier { get; private set; }
        internal string ParentGuid { get; private set; }
        internal bool IsParent { get { return ParentGuid == null && !UniqueNpc; } }
        internal bool UniqueNpc { get; private set; }
        internal IReadOnlyList<string> CreatureKeys { get; private set; }
        internal string MonsterParentGuid { get; private set; }
        internal string NaturesAllyParentGuid { get; private set; }
    }

    /// <summary>
    /// Results of actual consumer/access and invocation-contract verification.
    /// All conditions default to unproved. The scoped learning helper supplies
    /// these only from this invocation's exact list, book and live blueprints.
    /// </summary>
    internal sealed class StandaloneElementalEquivalenceEvidence
    {
        internal bool ConsumerInventoryComplete;
        internal string SourceSpellListGuid;
        internal string CanonicalSpellListGuid;
        internal string SourceSpellbookGuid;
        internal string CanonicalSpellbookGuid;
        internal int SourceSpellLevel;
        internal int CanonicalSpellLevel;
        internal string CanonicalParentGuid;
        internal string[] PublishedCanonicalCreatureKeys;
        internal bool InvocationContractEqual;
        internal bool ExistingSerializedReferencesIntact;
        internal bool OriginalSlotAndResourcesRetained;
        internal bool LegacyInvocationStillReachable;
        internal bool LearningSelectionBudgetVerified;
    }

    internal static class StandaloneElementalPresentationPolicy
    {
        internal const string WrigglingManAbilityGuid = "03b02f96bf14a7d4290eb2605bd2c15e";
        // Native inventory identity, not a PASS receipt. The ten complete
        // ordinary lists below authorize only their scoped learning route;
        // incomplete other owners receive no suppression authorization.
        internal const string InitialInventorySha256 =
            "1a29acebde261915b3ccd46e46489005b3af756f951d57f7d9d333a6bd8bef3c";

        private static readonly StandaloneElementalAbilitySpec[] Values = Build();
        private static readonly HashSet<string> OrdinaryClassLists = new HashSet<string>(
            new[] {
                "25a5013493bdcf74bb2424532214d0c8", // Bard
                "8443ce803d2d31347897a3d85cc32f53", // Cleric
                "bad8638d40639d04fa2f80a1cac67d6b", // Druid
                "ba0401fdeb4062f40a7aa95b6f07fe89", // Wizard
                "280dd5167ccafe449a33fbe93c7a875e", // Thassilonian Abjuration
                "5b154578f228c174bac546b6c29886ce", // Thassilonian Conjuration
                "ac551db78c1baa34eb8edca088be13cb", // Thassilonian Enchantment
                "5c08349132cb6b04181797f58ccf38ae", // Thassilonian Necromancy
                "f3a8f76b1d030a64084355ba3eea369a", // Thassilonian Transmutation
                "69a6eba12bc77ea4191f573d63c9df12"  // Wizard Conjuration
            }, StringComparer.Ordinal);

        internal static IReadOnlyList<StandaloneElementalAbilitySpec> All
        { get { return Array.AsReadOnly(Values); } }
        internal static StandaloneElementalAbilitySpec Find(string guid)
        { return Values.SingleOrDefault(value => string.Equals(value.Guid, guid, StringComparison.Ordinal)); }

        internal static bool IsObservedOrdinaryList(string guid)
        { return OrdinaryClassLists.Contains(guid ?? ""); }

        /// <summary>
        /// The exact copy/filter decision shared by both native learning UI
        /// helpers. Selected and already-known legacy rows remain visible.
        /// Counting distinct unselected, unknown rows prevents a UI filter
        /// from making a mandatory learning allocation impossible. Extra slots
        /// are conservatively charged to this level by the caller.
        /// </summary>
        internal static List<T> FilterLearningRows<T>(IEnumerable<T> original,
            Func<T, string> identity, ISet<string> eligible, ISet<string> selected,
            ISet<string> known, ISet<string> unavailable, int freeLevelSlots, int freeExtraSlots,
            bool contextVerified, out string disposition)
        {
            if (original == null) throw new ArgumentNullException("original");
            if (identity == null) throw new ArgumentNullException("identity");
            var copy = original.ToList();
            if (!contextVerified || eligible == null || selected == null || known == null || unavailable == null ||
                freeLevelSlots < 0 || freeExtraSlots < 0)
            { disposition = "preserve-selection-context-unproved"; return copy; }
            var filtered = copy.Where(value => !eligible.Contains(identity(value)) ||
                selected.Contains(identity(value)) || known.Contains(identity(value))).ToList();
            if (filtered.Count == copy.Count)
            { disposition = "preserve-no-eligible-unselected-row"; return copy; }
            int available = filtered.Select(identity).Where(value => !string.IsNullOrEmpty(value) &&
                !selected.Contains(value) && !known.Contains(value) && !unavailable.Contains(value))
                .Distinct(StringComparer.Ordinal).Count();
            if ((long)available < (long)freeLevelSlots + freeExtraSlots)
            { disposition = "preserve-learning-budget-insufficient"; return copy; }
            disposition = "filtered-redundant-learning-rows";
            return filtered;
        }

        /// <summary>
        /// A disposition for this exact consumer route. An eligible discovery
        /// row is only a pure policy result, never permission to change saved
        /// abilities, shared spell-list membership, domains, items or NPC facts.
        /// </summary>
        internal static string Disposition(bool moduleActive, string abilityGuid,
            StandaloneElementalConsumerRoute route, StandaloneElementalEquivalenceEvidence proof)
        {
            if (!moduleActive) return "preserve-module-off";
            StandaloneElementalAbilitySpec spec = Find(abilityGuid);
            if (spec == null) return "preserve-unknown-identity";
            if (spec.UniqueNpc) return "preserve-unique-wriggling-man";
            switch (route)
            {
                case StandaloneElementalConsumerRoute.ExistingKnownSpell:
                case StandaloneElementalConsumerRoute.PreparedSlot:
                case StandaloneElementalConsumerRoute.PinnedHotbar:
                    return "compatibility-presentation-required-preserve-invocation";
                case StandaloneElementalConsumerRoute.DomainOrSpecialGrant:
                    return "preserve-special-access";
                case StandaloneElementalConsumerRoute.Item:
                    return "preserve-item-contract";
                case StandaloneElementalConsumerRoute.NpcOrAi:
                    return "preserve-npc-or-ai";
                case StandaloneElementalConsumerRoute.OrdinaryClassLearning:
                case StandaloneElementalConsumerRoute.OrdinarySpellbookDiscovery:
                    break;
                default: return "preserve-unclassified-consumer";
            }
            if (proof == null || !proof.ConsumerInventoryComplete)
                return "preserve-consumer-inventory-incomplete";
            if (!OrdinaryClassLists.Contains(proof.SourceSpellListGuid ?? "") ||
                !SameNonempty(proof.SourceSpellListGuid, proof.CanonicalSpellListGuid) ||
                !SameNonempty(proof.SourceSpellbookGuid, proof.CanonicalSpellbookGuid) ||
                proof.SourceSpellLevel != spec.CanonicalTier ||
                proof.CanonicalSpellLevel != spec.CanonicalTier ||
                (proof.CanonicalParentGuid != spec.MonsterParentGuid &&
                 proof.CanonicalParentGuid != spec.NaturesAllyParentGuid))
                return "preserve-access-or-level-not-equivalent";
            string[] published = proof.PublishedCanonicalCreatureKeys ?? new string[0];
            if (spec.CreatureKeys.Any(key => published.Count(value => value == key) != 1))
                return "preserve-canonical-choice-coverage-unproved";
            if (!proof.InvocationContractEqual || !proof.ExistingSerializedReferencesIntact ||
                !proof.OriginalSlotAndResourcesRetained || !proof.LegacyInvocationStillReachable)
                return "preserve-invocation-compatibility-unproved";
            if (route == StandaloneElementalConsumerRoute.OrdinaryClassLearning &&
                !proof.LearningSelectionBudgetVerified)
                return "preserve-learning-budget-unproved";
            return "eligible-redundant-discovery-only";
        }

        private static bool SameNonempty(string left, string right)
        { return !string.IsNullOrWhiteSpace(left) && string.Equals(left, right, StringComparison.Ordinal); }

        private static StandaloneElementalAbilitySpec[] Build()
        {
            var result = new List<StandaloneElementalAbilitySpec>();
            AddRank(result, "small", "Small", 2, "970c6db48ff0c6f43afc9dbb48780d03",
                "1724061e89c667045a6891179ee2e8e7", "298148133cdc3fd42889b99c82711986",
                "9cc6b61eba880b944a8f489c44640b5c", "69b36426bb910e341a943f101daed594",
                "d8f88028204bc2041be9d9d51f58e6a5", "107788f47c4481f4db6da06498b28270");
            AddRank(result, "medium", "Medium", 4, "e42b1dbff4262c6469a9ff0a6ce730e3",
                "7ed74a3ec8c458d4fb50b192fd7be6ef", "c83db50513abdf74ca103651931fac4b",
                "cce8eca4409e1be4794ce94597034f38", "4ed414d36753459499578c9c3eb38b17",
                "7b30211b83d55194db872b6c9c0d9cc1", "2c86e646a17f6064994ba7e8da4dd77c");
            AddRank(result, "large", "Large", 5, "89404dd71edc1aa42962824b44156fe5",
                "630c8b85d9f07a64f917d79cb5905741", "8f98a22f35ca6684a983363d32e51bfe",
                "16fff43f034133a4a86e914a523e021f", "202a024bc7c224d429a9cfb14fa9c3d8",
                "b3a203742191449458d2544b3f442194", "9374c81817be79f4e92b48a4aa5ded6e");
            AddRank(result, "huge", "Huge", 6, "766ec978fa993034f86a372c8eb1fc10",
                "e740afbab0147944dab35d83faa0ae1c", "55bbce9b3e76d4a4a8c8e0698d29002c",
                "2c26e78ca33e83843b1aa3f6783877bd", "989d3ed13d27d054ea2d26ab4956d075",
                "4814f8645d1d77447a70479c0be51c72", "4695c4674e62eb3479fa03f0b47cc6a8");
            AddRank(result, "greater", "Greater", 7, "8eb769e3b583f594faabe1cfdb0bb696",
                "ab167fd8203c1314bac6568932f1752f", "051b979e7d7f8ec41b9fa35d04746b33",
                "a4849d6bd536ade48a3839a4ad960a8b", "3ecd589cf1a55df42a3b66940ee93ea4",
                "35c6b4497a5bfd24aabe0cf28507abef", "cfa3ff71a42ab8a4c8a8a4b5db3f04d4");
            AddRank(result, "elder", "Elder", 8, "8a7f8c1223bda1541b42fd0320cdbe2b",
                "d3ac756a229830243a72e84f3ab050d0", "ea78c04f0bd13d049a1cce5daf8d83e0",
                "333efbf776ab61c4da53e9622751d95f", "65254c7a2cf18944287207e1de3e44e8",
                "e4926aa766a1cc048835237b3a97597d", "1e54cc4ef5b261248adb30916ba49e72");
            result.Add(new StandaloneElementalAbilitySpec(WrigglingManAbilityGuid,
                "WrigglingManSummonElementalElderAir", 0, null, new string[0], null, null, true));
            return result.ToArray();
        }

        private static void AddRank(ICollection<StandaloneElementalAbilitySpec> result,
            string rankKey, string rankName, int tier, string parent, string monster, string ally,
            string air, string earth, string fire, string water)
        {
            string[] elements = { "air", "earth", "fire", "water" };
            string[] names = { "Air", "Earth", "Fire", "Water" };
            string[] guids = { air, earth, fire, water };
            string[] keys = elements.Select(value => rankKey + "-" + value + "-elemental").ToArray();
            result.Add(new StandaloneElementalAbilitySpec(parent, "SummonElemental" + rankName + "Base",
                tier, null, keys, monster, ally));
            for (int index = 0; index < elements.Length; index++)
                result.Add(new StandaloneElementalAbilitySpec(guids[index],
                    "SummonElemental" + rankName + names[index], tier, parent,
                    new[] { keys[index] }, monster, ally));
        }
    }
}
