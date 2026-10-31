using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningCatalogTests
    {
        internal static void LillendPublicRoutesPreservePublishedSurface()
        {
            var lillend = ExpandedSummoningCatalog.All.Single(value => value.Key == "lillend-azata");
            Assertions.False(lillend.NaturesAllyTier.HasValue, "Lillend has no Nature's Ally placement.");
            Assertions.False(lillend.MonsterTemplated, "A celestial outsider does not gain animal templates.");
            var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == lillend.Key).OrderBy(value => value.ParentTier).ToArray();
            Assertions.True(routes.Select(value => value.ParentTier).SequenceEqual(new[] { 6, 7, 8, 9 }),
                "Lillend occupies exactly SM VI through IX.");
            Assertions.True(routes.Select(value => value.Multiplicity).SequenceEqual(new[] {
                SummonMultiplicity.One, SummonMultiplicity.OneD3,
                SummonMultiplicity.OneD4PlusOne, SummonMultiplicity.OneD4PlusOne }),
                "Lillend quantities propagate from its own tier.");
            Assertions.True(routes.All(SummonVisibilityCatalog.IsPublished),
                "The candidate exposes every legal Lillend route.");
            SummonVisibilityCatalog.Validate(); SummonChoicePresentationPolicy.Validate();
            Assertions.Equal("Lillend Azata", SummonChoicePresentationPolicy.For(lillend.Key).Label,
                "The creature retains its deliberate identity.");
            var identities = ExpandedSummoningIdentityCatalog.Build().ToDictionary(value => value.Symbol);
            foreach (var row in new[] {
                new { Suffix = "Spellbook", Type = "BlueprintSpellbook" },
                new { Suffix = "SpellList", Type = "BlueprintSpellList" },
                new { Suffix = "Courage", Type = "BlueprintActivatableAbility" },
                new { Suffix = "PerformanceResource", Type = "BlueprintAbilityResource" },
                new { Suffix = "SoundBurst", Type = "BlueprintAbility" },
                new { Suffix = "CourageAi", Type = "LillendCourageAi32" },
                new { Suffix = "HealEligibility", Type = "LillendHealingEligibility32" } })
                Assertions.Equal(row.Type, identities["KMG.Summoning.FeyCelestial.LillendAzata." + row.Suffix].PlannedType,
                    "Lillend's private native support dependency retains its declared type.");
        }

        internal static void SatyrPublicRoutesPreservePublishedSurface()
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
            Assertions.True(routes.All(SummonVisibilityCatalog.IsPublished),
                "The candidate exposes every legal Satyr route.");
            var prior = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(value => value.Creature.Key != "satyr" && value.Creature.Key != "lillend-azata" &&
                    value.Creature.Key != "bearded-devil" && value.Creature.Key != "kyton-chain-devil" &&
                    value.Creature.Key != "barbed-devil" && value.Creature.Key != "bone-devil").ToArray();
            Assertions.Equal(1150, prior.Length, "Accepted public route surface must be preserved.");
            Assertions.True(prior.All(SummonVisibilityCatalog.IsPublished), "Existing routes must remain published.");
            SummonVisibilityCatalog.Validate();
            SummonChoicePresentationPolicy.Validate();
            Assertions.Equal("Satyr", SummonChoicePresentationPolicy.For("satyr").Label,
                "The actor retains its deliberate menu identity.");
            var owned = ExpandedSummoningIdentityCatalog.Build().Where(value =>
                value.Symbol.StartsWith("KMG.Summoning.FeyCelestial.Satyr.", StringComparison.Ordinal)).ToArray();
            var graph = owned.ToDictionary(value => value.Symbol);
            foreach (var suffix in new[] { "Fear", "CharmPerson", "Sleep" })
            {
                Assertions.Equal("BlueprintAbility", graph["KMG.Summoning.FeyCelestial.Satyr." + suffix].PlannedType,
                    "Each supported control retains its own ability identity.");
                Assertions.Equal("SatyrControlEligibility", graph["KMG.Summoning.FeyCelestial.Satyr." + suffix + "Eligibility"].PlannedType,
                    "Each control has its owned eligibility mapping.");
            }
            foreach (var suffix in new[] { "Melee", "Ranged" })
                Assertions.Equal("BlueprintAiSwitchWeapon", graph["KMG.Summoning.FeyCelestial.Satyr." + suffix + "Ai"].PlannedType,
                    "Weapon choice uses native equipment commands.");
            foreach (var suffix in new[] { "Dagger", "Horns" })
                Assertions.Equal("BlueprintItemWeapon", graph["KMG.Summoning.FeyCelestial.Satyr." + suffix].PlannedType,
                    "Adult melee attacks retain separate owned item identities.");
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
