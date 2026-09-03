using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalSpellAffinityPolicyTests
    {
        private const long Fire = 1L << 2;
        private const long Cold = 1L << 3;

        internal static void OrdinarySpellsRequireMatchingDescriptor()
        {
            Assertions.Equal(1, Bonus(true, true, true, Fire,
                Spell(Fire)), "A matching spellbook spell should gain +1 DC.");
            Assertions.Equal(0, Bonus(true, true, true, Fire,
                Spell(Cold)), "A nonmatching spellbook spell should gain +0 DC.");
        }

        internal static void VariantsAndParentsProduceAtMostOneBonus()
        {
            Assertions.Equal(1, Bonus(true, true, true, Fire,
                Spell(0), Spell(Fire)),
                "A matching parent should qualify its ordinary spell variant.");
            Assertions.Equal(1, Bonus(true, true, true, Fire,
                Spell(Fire), Spell(Fire)),
                "Matching child and parent descriptors must still produce +1 total.");
        }

        internal static void NonspellInvocationSourcesAreRejected()
        {
            Assertions.Equal(0, Bonus(true, false, false, Fire,
                Nonspell(Fire)),
                "A racial spell-like ability must not gain affinity DC.");
            Assertions.Equal(0, Bonus(true, true, true, Fire,
                Nonspell(Fire)),
                "A descriptor-bearing nonspell remains ineligible even with an artificial spellbook.");
            Assertions.Equal(0, Bonus(true, false, false, Fire,
                Spell(Fire)),
                "An item-cast spell blueprint without spellbook context must be ineligible.");
            Assertions.Equal(0, Bonus(true, false, false, Fire,
                Nonspell(Fire)),
                "Kinetic, supernatural, weapon, and arbitrary descriptor abilities must be ineligible.");
        }

        internal static void InvocationBoundaryFailsClosed()
        {
            Assertions.Equal(0, Bonus(false, true, true, Fire,
                Spell(Fire)), "An event without AbilityData must be ineligible.");
            Assertions.Equal(0, Bonus(true, true, false, Fire,
                Spell(Fire)), "Mismatched event and invocation spellbooks must be ineligible.");
            Assertions.Equal(0, Bonus(true, true, true, 0,
                Spell(Fire)), "An empty affinity descriptor must be ineligible.");
            Assertions.Equal(0,
                ElementalSpellAffinityPolicy.CalculateDcBonus(true, true,
                    true, Fire, null),
                "A missing effective blueprint chain must be ineligible.");
        }

        private static int Bonus(bool hasAbilityData,
            bool hasEventSpellbook, bool spellbooksMatch,
            long requiredDescriptorMask,
            params ElementalSpellAffinityNode[] chain)
        {
            return ElementalSpellAffinityPolicy.CalculateDcBonus(
                hasAbilityData, hasEventSpellbook, spellbooksMatch,
                requiredDescriptorMask, chain);
        }

        private static ElementalSpellAffinityNode Spell(long descriptor)
        {
            return new ElementalSpellAffinityNode(true, descriptor);
        }

        private static ElementalSpellAffinityNode Nonspell(long descriptor)
        {
            return new ElementalSpellAffinityNode(false, descriptor);
        }
    }
}
