using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint28Tests
    {
        internal static void ElevenRoutesAreSnaOnlyAndPublic()
        {
            string[] keys = { GiantHumanoidRulesPolicy.Hill,
                GiantHumanoidRulesPolicy.Stone, GiantHumanoidRulesPolicy.Fire };
            var giants = ExpandedSummoningCatalog.All.Where(value =>
                keys.Contains(value.Key)).ToArray();
            Assertions.Equal(3, giants.Length, "Exactly three adult giant entries.");
            Assertions.True(giants.All(value => !value.MonsterTier.HasValue &&
                !value.MonsterTemplated), "No Monster or template giant entries.");
            var monster = ExpandedSummoningCatalog.GenerateVariants(
                SummonFamily.Monster);
            Assertions.True(!monster.Any(value => keys.Contains(
                value.Creature.Key)), "No Monster giant routes.");
            var ally = ExpandedSummoningCatalog.GenerateVariants(
                SummonFamily.NaturesAlly).Where(value => keys.Contains(
                    value.Creature.Key)).ToArray();
            Assertions.Equal(GiantHumanoidRulesPolicy.AppendedRootCount,
                ally.Length, "Exactly eleven SNA roots.");
            foreach (string key in keys)
            {
                var routes = ally.Where(value => value.Creature.Key == key)
                    .OrderBy(value => value.ParentTier).ToArray();
                int tier = key == GiantHumanoidRulesPolicy.Fire ? 7 : 6;
                Assertions.True(routes.Select(value => value.ParentTier)
                    .SequenceEqual(Enumerable.Range(tier, 10 - tier)),
                    key + " parent tiers.");
                Assertions.Equal(SummonMultiplicity.One,
                    routes[0].Multiplicity, key + " single.");
                Assertions.Equal(SummonMultiplicity.OneD3,
                    routes[1].Multiplicity, key + " one d3.");
                Assertions.True(routes.Skip(2).All(value =>
                    value.Multiplicity == SummonMultiplicity.OneD4PlusOne),
                    key + " one d4 plus one.");
                Assertions.True(routes.All(value =>
                    SummonVisibilityCatalog.IsPublished(value)),
                    key + " has only its authorized public routes.");
                Assertions.True(SummonIconCatalog.All.Any(value =>
                    value.Key == key), key + " has its own icon concept.");
            }
            Assertions.Equal(1119, SummonVisibilityCatalog
                .PublishedLogicalPlacementCount,
                "Only the eleven registered giant roots become public.");
        }

        internal static void IdentitiesAppendAndDonorsRemainExact()
        {
            var ids = ExpandedSummoningIdentityCatalog.Build().Where(value =>
                GiantHumanoidRulesPolicy.IsNewIdentity(value.Symbol)).ToArray();
            Assertions.Equal(GiantHumanoidRulesPolicy.AppendedCatalogIdentityCount,
                ids.Length, "Exactly 35 new giant identities.");
            Assertions.Equal(3, ids.Count(value =>
                value.PlannedType == "BlueprintUnit"),
                "Three project-owned unit identities.");
            Assertions.Equal(11, ids.Count(value =>
                value.PlannedType == "BlueprintAbility" &&
                value.Symbol.StartsWith("KMG.Summoning.Ability.", StringComparison.Ordinal)),
                "Eleven project-owned SNA roots.");
            Assertions.Equal(GiantHumanoidRulesPolicy.AppendedPrivateIdentityCount,
                ids.Count(value => value.PlannedType != "BlueprintUnit" &&
                    !value.Symbol.StartsWith("KMG.Summoning.Ability.", StringComparison.Ordinal)),
                "Twenty-one private giant identities.");
            Assertions.Equal("61dd71e874b16f742a0f783314105553",
                ExpandedSummoningDonorCatalog.For("hill-giant").Guid,
                "Audited adult Hill donor.");
            Assertions.Equal("61dd71e874b16f742a0f783314105553",
                ExpandedSummoningDonorCatalog.For("stone-giant").Guid,
                "Stone borrows only the Hill visual shell.");
            Assertions.Equal("590cd3d5e76fdc649a5f97bc984cd3c4",
                ExpandedSummoningDonorCatalog.For("fire-giant").Guid,
                "Fire borrows retained armored Frost visual shell.");
        }

        internal static void StoneRockyStealthRequiresSelectedArea()
        {
            Assertions.Equal(8,
                GiantHumanoidRulesPolicy.StoneRockyStealthBonus,
                "Printed conditional racial Stealth increment.");
            Assertions.True(GiantHumanoidRulesPolicy.IsStoneRockyTerrain("Hills"),
                "Selected Hills area.");
            Assertions.True(GiantHumanoidRulesPolicy.IsStoneRockyTerrain("NaturalCaves"),
                "Selected natural cave area.");
            foreach (string setting in new[] { null, "", "Plains", "Forest",
                "Swamp", "FirstWorld", "Caves", "hills" })
                Assertions.False(GiantHumanoidRulesPolicy.IsStoneRockyTerrain(setting),
                    "Other or missing area does not grant a rocky bonus: " + setting);
        }

        internal static void RockArithmeticAndCatchAreBounded()
        {
            Assertions.Equal(120,
                GiantHumanoidRulesPolicy.RockRangeIncrementFeet("hill-giant"),
                "Hill increment.");
            Assertions.Equal(180,
                GiantHumanoidRulesPolicy.RockRangeIncrementFeet("stone-giant"),
                "Stone increment.");
            Assertions.Equal(120,
                GiantHumanoidRulesPolicy.RockRangeIncrementFeet("fire-giant"),
                "Fire increment.");
            Assertions.Equal(600,
                GiantHumanoidRulesPolicy.RockMaximumRangeFeet("hill-giant"),
                "Five Hill increments are command-reachable.");
            Assertions.Equal(900,
                GiantHumanoidRulesPolicy.RockMaximumRangeFeet("stone-giant"),
                "Five Stone increments are command-reachable.");
            foreach (var row in new[] {
                new { EdgeFeet = 0f, Expected = 0 },
                new { EdgeFeet = 120f, Expected = 0 },
                new { EdgeFeet = 121f, Expected = -2 },
                new { EdgeFeet = 240f, Expected = -2 },
                new { EdgeFeet = 241f, Expected = -4 },
                new { EdgeFeet = 600f, Expected = -8 } })
                Assertions.Equal(row.Expected,
                    GiantHumanoidRulesPolicy.RockRangePenalty(120,
                        row.EdgeFeet * 0.3048f),
                    "Live range penalty at " + row.EdgeFeet + " feet.");
            Assertions.Equal(-2,
                GiantHumanoidRulesPolicy.RockRangePenalty(180,
                    181f * 0.3048f),
                "Stone uses its own printed increment.");
            Assertions.Equal(1,
                GiantHumanoidRulesPolicy.RockFullAttackCount("hill-giant"),
                "Hill prepared throw.");
            Assertions.Equal(2,
                GiantHumanoidRulesPolicy.RockFullAttackCount("stone-giant"),
                "Stone prepared full attack.");
            Assertions.Equal(1,
                GiantHumanoidRulesPolicy.RockFullAttackCount("fire-giant"),
                "Fire prepared throw.");
            foreach (var row in new[] {
                new { Strength = 7, Expected = 10 },
                new { Strength = 8, Expected = 12 },
                new { Strength = 10, Expected = 15 },
                new { Strength = 0, Expected = 0 },
                new { Strength = -1, Expected = -1 } })
                Assertions.Equal(row.Expected,
                    GiantHumanoidRulesPolicy.OneAndHalfStrength(row.Strength),
                    "Live Strength boundary " + row.Strength);
            Assertions.Equal(15,
                GiantHumanoidRulesPolicy.CatchDifficultyClass("Small", 0),
                "Small mundane rock.");
            Assertions.Equal(22,
                GiantHumanoidRulesPolicy.CatchDifficultyClass("Medium", 2),
                "Medium magical rock.");
            Assertions.Equal(25,
                GiantHumanoidRulesPolicy.CatchDifficultyClass("Large", 0),
                "Large mundane rock.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                GiantHumanoidRulesPolicy.CatchDifficultyClass("Arrow", 0),
                "Arrow is not a rock catch.");
        }
    }
}
