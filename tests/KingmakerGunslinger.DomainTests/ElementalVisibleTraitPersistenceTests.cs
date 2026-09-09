using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalVisibleTraitPersistenceTests
    {
        internal static void PublishedInventoryDistinguishesHistoricalAndWaivedPersistenceCoverage()
        {
            var rows = new List<ElementalAlternateTraitId[]>();
            foreach (ElementalHeritageRace race in Enum.GetValues(typeof(ElementalHeritageRace)))
                for (int gender = 0; gender < 2; ++gender)
                    for (int index = 0; index < 3; ++index)
                    {
                        var traits = ElementalVisibleTraitPersistencePolicy.Traits(race, gender, index);
                        rows.Add(traits);
                        var heritage = ElementalHeritagePolicy.Ordered().Where(value => value.ParentRace == race).ElementAt(index);
                        var state = ElementalAlternateTraitPolicy.Resolve(race, heritage.Id, traits);
                        Assertions.True(traits.Length > 0 && traits.All(ElementalAlternateTraitPolicy.IsPublished), "Persistence accepted an empty or deferred fixture.");
                        Assertions.Equal(traits.Length, state.TraitProviderSymbols().Length, "A conflicting slot silently discarded a fixture trait.");
                        Assertions.Equal(state.Fingerprint, ElementalAlternateTraitPolicy.Resolve(race, heritage.Id, traits.Reverse()).Fingerprint,
                            "Persisted provider reconstruction depends on marker order.");
                        Assertions.Equal(state.Fingerprint, ElementalAlternateTraitPolicy.ResolveMarkers(race, heritage.Id, state.MarkerSymbols()).Fingerprint,
                            "Serialized exact markers no longer reconstruct the same selected provider graph.");
                        foreach (ElementalHeritageStat stat in Enum.GetValues(typeof(ElementalHeritageStat)))
                            Assertions.Equal(heritage.ModifierFor(stat), state.ModifierFor(stat), "Combined visible traits changed heritage stats.");
                    }
            var published = Enum.GetValues(typeof(ElementalAlternateTraitId)).Cast<ElementalAlternateTraitId>()
                .Where(ElementalAlternateTraitPolicy.IsPublished).OrderBy(value => value).ToArray();
            Assertions.Equal(24, rows.Count, "All fixed race/gender/heritage fixture identities must remain.");
            Assertions.Equal(21, published.Length, "Visible content inventory changed without an explicit qualification matrix update.");
            var releasedCoverage = rows.SelectMany(value => value).Distinct().ToArray();
            Assertions.Equal(19, releasedCoverage.Length, "The original nineteen-trait fixture must remain intact.");
            var nereidCoverage = new List<ElementalAlternateTraitId>();
            for (int gender = 0; gender < 2; ++gender)
                for (int index = 0; index < 3; ++index)
                {
                    var traits = ElementalVisibleTraitPersistencePolicy.Traits(ElementalHeritageRace.Undine, gender, index, true);
                    Assertions.True(traits.SequenceEqual(new[] { ElementalAlternateTraitId.NereidFascination }),
                        "The separately qualified Nereid fixture must cover every heritage and sex.");
                    nereidCoverage.AddRange(traits);
                }
            Assertions.Equal(6, nereidCoverage.Count, "All six committed Nereid persistence rows must remain represented.");
            Assertions.True(releasedCoverage.Concat(nereidCoverage).Concat(new[] { ElementalAlternateTraitId.TreacherousEarth })
                .Distinct().OrderBy(value => value).SequenceEqual(published),
                "Published inventory must equal historical coverage plus Treacherous, whose full Player/save qualification was explicitly waived.");
            Assertions.Equal(17, rows.Count(value => value.Length > 1), "Combined-slot fixtures were lost.");
            var blood = new[] { ElementalAlternateTraitId.FireInTheBlood, ElementalAlternateTraitId.StoneInTheBlood, ElementalAlternateTraitId.StormInTheBlood };
            Assertions.Equal(7, rows.Count(value => value.Any(blood.Contains)), "Partial blood-expenditure fixtures changed.");
            Assertions.Equal(2, rows.Count(value => value.Contains(ElementalAlternateTraitId.BreezeKissed)), "Both spent and voluntarily calmed Breeze states must persist.");
        }

        internal static void PassiveSlaReplacementsAreExplicitlyRepresented()
        {
            var passive = new[] { ElementalAlternateTraitId.BrazenFlame, ElementalAlternateTraitId.ForgeHardened,
                ElementalAlternateTraitId.Secretive, ElementalAlternateTraitId.WhisperingWind };
            int absent = 0;
            foreach (ElementalHeritageRace race in Enum.GetValues(typeof(ElementalHeritageRace)))
                for (int gender = 0; gender < 2; ++gender)
                    for (int index = 0; index < 3; ++index)
                    {
                        var traits = ElementalVisibleTraitPersistencePolicy.Traits(race, gender, index);
                        var heritage = ElementalHeritagePolicy.Ordered().Where(value => value.ParentRace == race).ElementAt(index);
                        var state = ElementalAlternateTraitPolicy.Resolve(race, heritage.Id, traits);
                        if (!traits.Any(passive.Contains)) continue;
                        ++absent;
                        Assertions.True(state.RacialSlaFeatureSymbol == null && state.RacialSlaAbilitySymbol == null && state.RacialSlaResourceSymbol == null,
                            "An intentional passive SLA replacement retained an inactive heritage ability or resource.");
                        Assertions.Equal(traits.Length, state.TraitProviderSymbols().Length, "An intentional absent SLA discarded its mechanical provider.");
                    }
            Assertions.Equal(10, absent, "Every passive SLA replacement must be represented without inventing a successful cast.");
        }

        internal static void MatrixRequestsAreIndependentAndFailClosed()
        {
            var before = ElementalVisibleTraitPersistencePolicy.Traits(ElementalHeritageRace.Sylph, 0, 1);
            var expected = before.ToArray(); before[0] = ElementalAlternateTraitId.NereidFascination;
            Assertions.True(ElementalVisibleTraitPersistencePolicy.Traits(ElementalHeritageRace.Sylph, 0, 1).SequenceEqual(expected),
                "A caller mutated another fixture's trait inventory.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => ElementalVisibleTraitPersistencePolicy.Traits((ElementalHeritageRace)99, 0, 0), "Unknown race accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => ElementalVisibleTraitPersistencePolicy.Traits(ElementalHeritageRace.Ifrit, -1, 0), "Unknown gender accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => ElementalVisibleTraitPersistencePolicy.Traits(ElementalHeritageRace.Ifrit, 0, 3), "Unknown heritage accepted.");
            Assertions.Equal("release-c-visible-nineteen-traits-v5", ElementalVisibleTraitPersistencePolicy.MatrixId, "Matrix evidence lost its distinct identity.");
        }
    }
}
