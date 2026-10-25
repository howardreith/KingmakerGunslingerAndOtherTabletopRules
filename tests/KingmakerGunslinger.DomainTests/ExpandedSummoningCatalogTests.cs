using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningCatalogTests
    {
        internal static void SatyrHiddenRoutesPreservePublishedSurface()
        {
            var satyr = ExpandedSummoningCatalog.All.Single(value => value.Key == "satyr");
            Assertions.False(satyr.MonsterTier.HasValue, "Satyr has no Summon Monster placement.");
            Assertions.False(satyr.MonsterTemplated, "Satyr has no alignment template children.");
            var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                .Where(value => value.Creature.Key == "satyr").OrderBy(value => value.ParentTier).ToArray();
            Assertions.True(routes.Select(value => value.ParentTier).SequenceEqual(new[] { 4, 5, 6, 7, 8, 9 }),
                "Satyr must occupy exactly SNA IV through IX.");
            Assertions.True(routes.Select(value => value.Multiplicity).SequenceEqual(new[] {
                SummonMultiplicity.One, SummonMultiplicity.OneD3, SummonMultiplicity.OneD4PlusOne,
                SummonMultiplicity.OneD4PlusOne, SummonMultiplicity.OneD4PlusOne, SummonMultiplicity.OneD4PlusOne }),
                "Satyr quantities must propagate from its own tier.");
            Assertions.True(routes.All(value => !SummonVisibilityCatalog.IsPublished(value)),
                "Unqualified Satyr routes must remain hidden.");
            var prior = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(value => value.Creature.Key != "satyr").ToArray();
            Assertions.Equal(1150, prior.Length, "Accepted public route surface must be preserved.");
            Assertions.True(prior.All(SummonVisibilityCatalog.IsPublished), "Existing routes must remain published.");
            SummonVisibilityCatalog.Validate();
        }

        internal static void FrozenRosterAndPlacementCounts()
        {
            ExpandedSummoningCatalog.Validate();
            // Sprint 18 adds the two apes at Summon Monster and Nature's Ally
            // III and IV, which is seven and six placements per family.
            Assertions.Equal(SummoningCurrentSurface.RegisteredCreatureCount,
                ExpandedSummoningCatalog.All.Count, "Unique creature count changed.");
            Assertions.Equal(SummoningCurrentSurface.RegisteredMonsterCreatureCount,
                ExpandedSummoningCatalog.All.Count(v => v.MonsterTier.HasValue),
                "SM roster count changed.");
            Assertions.Equal(SummoningCurrentSurface.RegisteredNatureAllyCreatureCount,
                ExpandedSummoningCatalog.All.Count(v => v.NaturesAllyTier.HasValue),
                "SNA roster count changed.");
            Assertions.Equal(SummoningCurrentSurface.RegisteredMonsterPlacements,
                ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Count,
                "SM placement count changed.");
            Assertions.Equal(SummoningCurrentSurface.RegisteredNatureAllyPlacements,
                ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Count,
                "SNA placement count changed.");
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
