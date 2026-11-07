using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Repair sprint R3 UI-01/UI-03: catalog-level portrait ownership and the
    /// Ghaele light-ray identities.
    /// </summary>
    internal static class SummonPlaytestR3Tests
    {
        internal static void EveryRegisteredSummonOwnsItsPortraitArt()
        {
            SummonCreatureSpec[] creatures = SummonPortraitPolicy.PortraitCreatures().ToArray();
            Assertions.Equal(ExpandedSummoningIdentityCatalog.UnitCount, creatures.Length,
                "Every registered KMG summon unit gets a portrait.");
            Assertions.True(creatures.All(value => SummonPortraitPolicy.ArtKey(value) == value.Key),
                "Each portrait uses its own creature's art key, never a donor's or a family share.");
            Assertions.True(creatures.All(value => SummonIconCatalog.All.Any(icon => icon.Key == value.Key)),
                "Every portrait art key is a loaded, approved summon icon concept.");
            Assertions.Equal(creatures.Length, creatures.Select(value => SummonPortraitPolicy.PortraitName(value.Key))
                .Distinct(StringComparer.Ordinal).Count(), "Portrait names are unique per creature.");
            Assertions.True(ExpandedSummoningCatalog.All.Where(SummonIconCatalog.IsPublishedSomewhere)
                .All(value => creatures.Contains(value)), "Every published creature is covered.");
            foreach (string key in new[] { "dire-rat", "lantern-archon", "nalfeshnee", "glabrezu", "ice-devil", "ghaele-azata" })
                Assertions.True(creatures.Any(value => value.Key == key), "The reported creature " + key + " is covered.");
        }

        internal static void PortraitCropKeepsTheNativePortraitAspect()
        {
            int[] crop = SummonPortraitPolicy.CropRect(SummonPortraitPolicy.IconSize);
            Assertions.Equal(0, crop[1], "Full height.");
            Assertions.Equal(SummonPortraitPolicy.IconSize, crop[3], "Full height.");
            Assertions.True(crop[0] > 0 && crop[0] * 2 + crop[2] == SummonPortraitPolicy.IconSize,
                "The crop is centred inside the square art.");
            double aspect = (double)crop[2] / crop[3];
            Assertions.True(Math.Abs(aspect - SummonPortraitPolicy.NativeSmallPortraitAspect) < 0.01,
                "The crop matches the native small-portrait aspect, so the frame does not stretch it.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => SummonPortraitPolicy.CropRect(4), "Degenerate sizes are refused.");
        }

        internal static void GhaeleLightRayIdentitiesAreRegistered()
        {
            var identities = ExpandedSummoningIdentityCatalog.Build();
            foreach (var expected in new[] {
                new { Symbol = "KMG.Summoning.Special.GhaeleAzata.LightRay", Type = "BlueprintAbility" },
                new { Symbol = "KMG.Summoning.Special.GhaeleAzata.LightRayAi", Type = "BlueprintAiCastSpell" },
                new { Symbol = "KMG.Summoning.Special.GhaeleAzata.Brain", Type = "BlueprintBrain" } })
                Assertions.Equal(1, identities.Count(value => value.Symbol == expected.Symbol && value.PlannedType == expected.Type),
                    "Exactly one registered " + expected.Type + " " + expected.Symbol + ".");
        }
    }
}
