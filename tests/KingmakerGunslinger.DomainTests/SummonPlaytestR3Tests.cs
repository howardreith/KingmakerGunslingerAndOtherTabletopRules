using System;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
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

        /// <summary>
        /// The defect: owned trample/overrun inherited the native overrun's
        /// Duration "Trample" and its maneuver description as Saving Throw.
        /// </summary>
        internal static void TrampleAndOverrunRowsStateTheImplementedContract()
        {
            Assertions.Equal("", SummonActionTooltipPolicy.InstantaneousDuration, "Instantaneous movement omits the Duration row (empty text), never 'Trample' or <null>.");
            Assertions.Equal("", SummonActionTooltipPolicy.OverrunSavingThrow, "An overrun's combat maneuver check is not a saving throw.");
            string plain = SummonActionTooltipPolicy.TrampleSavingThrow(17, false);
            Assertions.True(plain.StartsWith("Reflex DC 17 half", StringComparison.Ordinal), "Trample states its Reflex save and DC.");
            Assertions.True(plain.Contains("attack of opportunity") && !plain.Contains("DC 19"), "The attack-of-opportunity alternative is kept; no stampede DC without Stampede.");
            string stampede = SummonActionTooltipPolicy.TrampleSavingThrow(17, true);
            Assertions.True(stampede.Contains("DC 19 during Stampede") && stampede.Contains("attack of opportunity"), "Stampede's +2 applies only while active.");
            foreach (string text in new[] { plain, stampede })
                Assertions.False(text == "Trample" || text.Contains("<null>") || text.Contains("run over your target"), "No inherited native overrun text.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => SummonActionTooltipPolicy.TrampleSavingThrow(0, false), "A missing DC is refused.");
        }

        internal static void UnrelatedModalRefusesEveryR3Surface()
        {
            Assertions.True(ExpandedSummoningNativeProfilePolicy.SummonPlaytestR3SurfaceOwned(true, true, false, true),
                "Exact native Default surface without narrative or an unrelated modal is owned.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SummonPlaytestR3SurfaceOwned(true, true, true, true),
                "An unrelated native message box refuses an otherwise exact surface.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SummonPlaytestR3SurfaceOwned(false, true, false, true), "Lost native Default refuses.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SummonPlaytestR3SurfaceOwned(true, false, false, true), "Narrative overlay refuses.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SummonPlaytestR3SurfaceOwned(true, true, false, false), "A different surface refuses.");
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
