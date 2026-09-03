using System.Collections.Generic;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalSpellAffinityNode
    {
        internal ElementalSpellAffinityNode(bool isOrdinarySpell,
            long descriptorMask)
        {
            IsOrdinarySpell = isOrdinarySpell;
            DescriptorMask = descriptorMask;
        }

        internal bool IsOrdinarySpell { get; private set; }
        internal long DescriptorMask { get; private set; }
    }

    internal static class ElementalSpellAffinityPolicy
    {
        internal static int CalculateDcBonus(bool hasAbilityData,
            bool hasEventSpellbook, bool spellbooksMatch,
            long requiredDescriptorMask,
            IEnumerable<ElementalSpellAffinityNode> abilityChain)
        {
            if (!hasAbilityData || !hasEventSpellbook || !spellbooksMatch ||
                requiredDescriptorMask == 0 || abilityChain == null)
                return 0;
            bool ordinarySpell = false;
            bool matchingDescriptor = false;
            foreach (ElementalSpellAffinityNode node in abilityChain)
            {
                if (node == null) continue;
                ordinarySpell |= node.IsOrdinarySpell;
                matchingDescriptor |= (node.DescriptorMask &
                    requiredDescriptorMask) != 0;
            }
            return ordinarySpell && matchingDescriptor ? 1 : 0;
        }
    }
}
