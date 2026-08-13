using System;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class SummonVariantSpec
    {
        internal SummonVariantSpec(SummonFamily family, int parentTier,
            SummonCreatureSpec creature, int sourceTier, SummonMultiplicity multiplicity)
        {
            if (creature == null) throw new ArgumentNullException("creature");
            if (parentTier < 1 || parentTier > 9) throw new ArgumentOutOfRangeException("parentTier");
            if (sourceTier < 1 || sourceTier > parentTier) throw new ArgumentOutOfRangeException("sourceTier");
            Family = family; ParentTier = parentTier; Creature = creature;
            SourceTier = sourceTier; Multiplicity = multiplicity;
        }
        internal SummonFamily Family { get; private set; }
        internal int ParentTier { get; private set; }
        internal SummonCreatureSpec Creature { get; private set; }
        internal int SourceTier { get; private set; }
        internal SummonMultiplicity Multiplicity { get; private set; }
        internal string StableKey { get { return Family + "." + ParentTier + "." + Creature.Key + "." + Multiplicity; } }
    }
}
