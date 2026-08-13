using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningCatalogTests
    {
        internal static void FrozenRosterAndPlacementCounts()
        {
            ExpandedSummoningCatalog.Validate();
            Assertions.Equal(67, ExpandedSummoningCatalog.All.Count, "Unique creature count changed.");
            Assertions.Equal(66, ExpandedSummoningCatalog.All.Count(v => v.MonsterTier.HasValue), "SM roster count changed.");
            Assertions.Equal(57, ExpandedSummoningCatalog.All.Count(v => v.NaturesAllyTier.HasValue), "SNA roster count changed.");
            Assertions.Equal(361, ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Count, "SM placement count changed.");
            Assertions.Equal(320, ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Count, "SNA placement count changed.");
        }
        internal static void QuantityRulesAreExactAndSameKind()
        {
            foreach (SummonFamily family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog.GenerateVariants(family)) {
                SummonMultiplicity expected = variant.SourceTier == variant.ParentTier ? SummonMultiplicity.One : variant.SourceTier == variant.ParentTier - 1 ? SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne;
                Assertions.Equal(expected, variant.Multiplicity, "Quantity mapping changed for " + variant.StableKey);
                Assertions.True(variant.StableKey.Contains(variant.Creature.Key), "A variant lost its same-kind identity.");
            }
        }
        internal static void AlignmentPoliciesAreFamilyScoped()
        {
            foreach (SummonCreatureSpec creature in ExpandedSummoningCatalog.All) {
                if (creature.MonsterTier.HasValue) Assertions.Equal(creature.MonsterTemplated ? SummonTemplatePolicy.CelestialOrFiendish : SummonTemplatePolicy.None, creature.TemplatePolicy(SummonFamily.Monster), "SM policy mismatch for " + creature.Key);
                if (creature.NaturesAllyTier.HasValue) Assertions.Equal(SummonTemplatePolicy.CasterAlignment, creature.TemplatePolicy(SummonFamily.NaturesAlly), "SNA policy mismatch for " + creature.Key);
            }
        }
        internal static void CatalogGuardsInvalidSpecs()
        {
            Assertions.Throws<ArgumentException>(() => new SummonCreatureSpec("", "Bad", 1, false, null, null), "Blank keys must fail.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => new SummonCreatureSpec("bad", "Bad", 10, false, null, null), "Invalid tiers must fail.");
            Assertions.Throws<ArgumentException>(() => new SummonCreatureSpec("bad", "Bad", null, true, 1, null), "SNA-only entries cannot receive SM templates.");
        }
    }
}
