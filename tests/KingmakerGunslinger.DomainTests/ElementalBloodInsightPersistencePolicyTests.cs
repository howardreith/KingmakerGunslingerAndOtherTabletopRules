using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalBloodInsightPersistencePolicyTests
    {
        internal static void BreathFixtureCoverageIsExact()
        {
            var covered = new List<Tuple<ElementalHeritageRace, int, int, ElementalAlternateTraitId[]>>();
            foreach (ElementalHeritageRace race in Enum.GetValues(typeof(ElementalHeritageRace)))
                for (int gender = 0; gender < 2; gender++)
                    for (int index = 0; index < 3; index++)
                    {
                        var traits = ElementalBloodInsightPersistencePolicy.BreathTraits(race, gender, index);
                        covered.Add(Tuple.Create(race, gender, index, traits));
                        var heritage = ElementalHeritagePolicy.Ordered().Where(value => value.ParentRace == race).ElementAt(index);
                        var state = ElementalAlternateTraitPolicy.Resolve(race, heritage.Id, traits);
                        Assertions.Equal(state.Fingerprint,
                            ElementalAlternateTraitPolicy.Resolve(race, heritage.Id, traits.Reverse()).Fingerprint,
                            "Every ten-trait fixture is independent of fact order.");
                        Assertions.Equal(state.Fingerprint,
                            ElementalAlternateTraitPolicy.ResolveMarkers(race, heritage.Id, state.MarkerSymbols()).Fingerprint,
                            "Native marker identities reconstruct the same desired providers.");
                        foreach (ElementalHeritageStat stat in Enum.GetValues(typeof(ElementalHeritageStat)))
                            Assertions.Equal(heritage.ModifierFor(stat), state.ModifierFor(stat), "No racial stat drift.");
                        Assertions.True(state.EnergyResistanceProviderSymbol != null, "Every fixture retains resistance.");
                        if (race == ElementalHeritageRace.Undine)
                            Assertions.True(state.ElementalAffinityProviderSymbol != null &&
                                state.RacialSlaFeatureSymbol == null && state.RacialSlaAbilitySymbol == null &&
                                state.RacialSlaResourceSymbol == null,
                                "Only the heritage SLA slot is consumed; no inactive Hydraulic Push/Blur/Chill Touch provider remains.");
                        else
                            Assertions.True(traits.SequenceEqual(ElementalBloodInsightPersistencePolicy.CrystallineTraits(race, gender, index)),
                                "All previous non-Undine fixture choices remain exact.");
                    }
            Assertions.Equal(24, covered.Count, "All existing fixture identities remain.");
            Assertions.Equal(24, covered.Count(value => value.Item4.Length != 0), "Every fixture is now trait-bearing.");
            Assertions.Equal(6, covered.Count(value => value.Item4.Length == 2), "All legal combined Ifrit fixtures remain.");
            Assertions.Equal(10, covered.SelectMany(value => value.Item4).Distinct().Count(), "Exactly ten implemented traits.");
            foreach (var id in new[] { ElementalAlternateTraitId.AcidBreath, ElementalAlternateTraitId.OozeBreath })
            {
                var rows = covered.Where(value => value.Item4.Contains(id)).ToArray();
                Assertions.Equal(3, rows.Length, "Each breath persists three times.");
                Assertions.Equal(2, rows.Select(value => value.Item2).Distinct().Count(), "Each breath covers both sexes.");
                Assertions.Equal(3, rows.Select(value => value.Item3).Distinct().Count(), "Each breath covers every Undine heritage.");
            }
            var mutable = ElementalBloodInsightPersistencePolicy.BreathTraits(ElementalHeritageRace.Undine, 0, 0);
            mutable[0] = ElementalAlternateTraitId.NereidFascination;
            Assertions.Equal(ElementalAlternateTraitId.AcidBreath,
                ElementalBloodInsightPersistencePolicy.BreathTraits(ElementalHeritageRace.Undine, 0, 0)[0],
                "Callers cannot mutate subsequent choices.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => ElementalBloodInsightPersistencePolicy.BreathTraits(
                ElementalHeritageRace.Undine, -1, 0), "Unknown gender fails closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => ElementalBloodInsightPersistencePolicy.BreathTraits(
                ElementalHeritageRace.Undine, 0, 3), "Unknown heritage fails closed.");
            CrystallineFixtureCoverageIsExact();
        }

        internal static void CrystallineFixtureCoverageIsExact()
        {
            var covered = new List<Tuple<ElementalHeritageRace, int, int, ElementalAlternateTraitId[]>>();
            foreach (ElementalHeritageRace race in Enum.GetValues(typeof(ElementalHeritageRace)))
                for (int gender = 0; gender < 2; gender++)
                    for (int index = 0; index < 3; index++)
                    {
                        ElementalAlternateTraitId[] traits = ElementalBloodInsightPersistencePolicy.CrystallineTraits(race, gender, index);
                        covered.Add(Tuple.Create(race, gender, index, traits));
                        ElementalHeritageDefinition heritage = ElementalHeritagePolicy.Ordered().Where(value => value.ParentRace == race).ElementAt(index);
                        ElementalAlternateTraitState state = ElementalAlternateTraitPolicy.Resolve(race, heritage.Id, traits);
                        Assertions.Equal(state.Fingerprint,
                            ElementalAlternateTraitPolicy.Resolve(race, heritage.Id, traits.Reverse()).Fingerprint,
                            "Every eight-trait fixture resolves independently of fact order.");
                        Assertions.Equal(state.Fingerprint,
                            ElementalAlternateTraitPolicy.ResolveMarkers(race, heritage.Id, state.MarkerSymbols()).Fingerprint,
                            "Serialized markers reconstruct the same exact providers.");
                        foreach (ElementalHeritageStat stat in Enum.GetValues(typeof(ElementalHeritageStat)))
                            Assertions.Equal(heritage.ModifierFor(stat), state.ModifierFor(stat), "Racial stats are unchanged.");
                        if (traits.Contains(ElementalAlternateTraitId.CrystallineForm))
                        {
                            Assertions.Equal(ElementalHeritageRace.Oread, race, "Only Oreads receive the trait.");
                            Assertions.Equal(ElementalHeritageId.Ironsoul, heritage.Id, "Incremental fixtures explicitly use Ironsoul.");
                            Assertions.True(state.ElementalAffinityProviderSymbol == null &&
                                state.EnergyResistanceProviderSymbol != null && state.RacialSlaResourceSymbol != null,
                                "Crystalline consumes affinity only; its resource never replaces the heritage SLA resource.");
                        }
                    }
            Assertions.Equal(24, covered.Count, "All race, sex and heritage fixtures remain.");
            Assertions.Equal(18, covered.Count(value => value.Item4.Length != 0), "Eighteen native-selected trait fixtures.");
            Assertions.Equal(6, covered.Count(value => value.Item4.Length == 2), "Six legal combined Ifrit fixtures remain.");
            Assertions.Equal(8, covered.SelectMany(value => value.Item4).Distinct().Count(), "All seven prior traits plus Crystalline remain represented.");
            var crystalline = covered.Where(value => value.Item4.Contains(ElementalAlternateTraitId.CrystallineForm)).ToArray();
            Assertions.Equal(2, crystalline.Length, "One spent and one armed native fixture.");
            Assertions.Equal(2, crystalline.Select(value => value.Item2).Distinct().Count(), "Both sexes covered.");
            ElementalAlternateTraitId[] blood = { ElementalAlternateTraitId.FireInTheBlood,
                ElementalAlternateTraitId.StoneInTheBlood, ElementalAlternateTraitId.StormInTheBlood };
            Assertions.Equal(8, covered.Count(value => value.Item4.Any(blood.Contains)), "Eight partially spent blood fixtures remain.");
            EfreetiCombinedFixtureCoverageIsExact();
            NativeFixtureCoverageIsExact();
        }

        internal static void EfreetiCombinedFixtureCoverageIsExact()
        {
            var covered = new List<Tuple<ElementalHeritageRace, int, int, ElementalAlternateTraitId[]>>();
            foreach (ElementalHeritageRace race in Enum.GetValues(typeof(ElementalHeritageRace)))
            {
                ElementalHeritageDefinition[] heritages = ElementalHeritagePolicy.Ordered()
                    .Where(value => value.ParentRace == race).ToArray();
                for (int gender = 0; gender < 2; gender++)
                    for (int index = 0; index < heritages.Length; index++)
                    {
                        ElementalAlternateTraitId[] traits = ElementalBloodInsightPersistencePolicy.Traits(race, gender, index);
                        covered.Add(Tuple.Create(race, gender, index, traits));
                        ElementalAlternateTraitState state = ElementalAlternateTraitPolicy.Resolve(race, heritages[index].Id, traits);
                        ElementalAlternateTraitState reversed = ElementalAlternateTraitPolicy.Resolve(race, heritages[index].Id, traits.Reverse());
                        ElementalAlternateTraitState reconstructed = ElementalAlternateTraitPolicy.ResolveMarkers(race,
                            heritages[index].Id, state.MarkerSymbols().ToArray());
                        Assertions.Equal(state.Fingerprint, reversed.Fingerprint, "Disjoint slot state is order independent.");
                        Assertions.Equal(state.Fingerprint, reconstructed.Fingerprint, "Serialized marker identities reconstruct exact providers.");
                        Assertions.True(state.EnergyResistanceProviderSymbol != null, "Every fixture retains resistance for zero-damage blood triggers.");
                        Assertions.Equal(race == ElementalHeritageRace.Undine, state.ElementalAffinityProviderSymbol != null,
                            "Exactly the eighteen existing blood/Insight fixtures replace affinity.");
                        Assertions.Equal(race != ElementalHeritageRace.Ifrit, state.RacialSlaFeatureSymbol != null,
                            "Only Efreeti Magic consumes the heritage SLA provider.");
                        Assertions.Equal(race != ElementalHeritageRace.Ifrit, state.RacialSlaResourceSymbol != null,
                            "No inactive Ifrit heritage resource is expected.");
                        Assertions.Equal(race != ElementalHeritageRace.Ifrit, state.RacialSlaAbilitySymbol != null,
                            "No inactive Ifrit heritage ability is expected.");
                        foreach (ElementalHeritageStat stat in Enum.GetValues(typeof(ElementalHeritageStat)))
                            Assertions.Equal(heritages[index].ModifierFor(stat), state.ModifierFor(stat),
                                "Combining traits never duplicates or changes heritage racial modifiers.");
                    }
            }
            Assertions.Equal(24, covered.Count, "All existing race/sex/heritage fixtures remain.");
            Assertions.Equal(18, covered.Count(value => value.Item4.Length != 0), "Eighteen trait-bearing fixtures remain.");
            Assertions.Equal(6, covered.Count(value => value.Item4.Length == 2), "All six Ifrit fixtures carry a legal two-trait combination.");
            Assertions.Equal(7, covered.SelectMany(value => value.Item4).Distinct().Count(), "Seven actual mechanics, no placeholder traits.");
            foreach (int variant in new[] { 0, 1 })
            {
                var rows = covered.Where(value => value.Item1 == ElementalHeritageRace.Ifrit &&
                    ElementalBloodInsightPersistencePolicy.EfreetiVariantIndex(value.Item2, value.Item3) == variant).ToArray();
                Assertions.Equal(3, rows.Length, "Each native size option persists three times.");
                Assertions.Equal(2, rows.Select(value => value.Item2).Distinct().Count(), "Each size option covers both sexes.");
                Assertions.Equal(3, rows.Select(value => value.Item3).Distinct().Count(), "Each size option covers every Ifrit heritage.");
            }
            ElementalAlternateTraitId[] mutable = ElementalBloodInsightPersistencePolicy.Traits(ElementalHeritageRace.Ifrit, 0, 0);
            mutable[1] = ElementalAlternateTraitId.BrazenFlame;
            Assertions.Equal(ElementalAlternateTraitId.EfreetiMagic,
                ElementalBloodInsightPersistencePolicy.Traits(ElementalHeritageRace.Ifrit, 0, 0)[1],
                "Caller changes cannot mutate the next fixture's identity list.");
        }

        internal static void NativeFixtureCoverageIsExact()
        {
            var rows = new List<Tuple<ElementalHeritageRace, int, int, ElementalAlternateTraitId?>>();
            foreach (ElementalHeritageRace race in Enum.GetValues(typeof(ElementalHeritageRace)))
                for (int gender = 0; gender < 2; gender++)
                    for (int heritage = 0; heritage < 3; heritage++)
                    {
                        ElementalAlternateTraitId? trait = ElementalBloodInsightPersistencePolicy.Trait(race, gender, heritage);
                        rows.Add(Tuple.Create(race, gender, heritage, trait));
                        Assertions.Equal(race != ElementalHeritageRace.Undine, trait.HasValue,
                            "Only the six implemented affinity-replacement traits enter this incremental matrix.");
                        if (trait.HasValue)
                        {
                            ElementalAlternateTraitDefinition definition = ElementalAlternateTraitPolicy.ForRace(race)
                                .Single(value => value.Id == trait.Value);
                            Assertions.Equal(ElementalRacialTraitSlot.ElementalAffinity, definition.ReplacedSlots,
                                "Resistance, racial stats, and SLA/resource expectations are unchanged in this slice.");
                        }
                    }
            Assertions.Equal(24, rows.Count, "All race/sex/heritage fixtures remain present.");
            Assertions.Equal(18, rows.Count(value => value.Item4.HasValue), "Eighteen fixtures carry actual traits.");
            ElementalAlternateTraitId[] expected = { ElementalAlternateTraitId.FireInTheBlood,
                ElementalAlternateTraitId.FireInsight, ElementalAlternateTraitId.StoneInTheBlood,
                ElementalAlternateTraitId.EarthInsight, ElementalAlternateTraitId.StormInTheBlood,
                ElementalAlternateTraitId.AirInsight };
            Assertions.True(rows.Where(value => value.Item4.HasValue).Select(value => value.Item4.Value)
                .Distinct().OrderBy(value => value).SequenceEqual(expected.OrderBy(value => value)),
                "Exactly the three Insights and three blood traits are covered; no placeholder counts as implemented.");
            foreach (ElementalAlternateTraitId trait in expected)
            {
                var covered = rows.Where(value => value.Item4 == trait).ToArray();
                Assertions.Equal(3, covered.Length, "Each trait receives three independent save fixtures.");
                Assertions.Equal(2, covered.Select(value => value.Item2).Distinct().Count(), "Each trait covers both sexes.");
                Assertions.Equal(3, covered.Select(value => value.Item3).Distinct().Count(), "Each trait covers every parent heritage.");
            }
            Assertions.Throws<ArgumentOutOfRangeException>(() => ElementalBloodInsightPersistencePolicy.Trait(
                (ElementalHeritageRace)99, 0, 0), "Unknown races fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => ElementalBloodInsightPersistencePolicy.Trait(
                ElementalHeritageRace.Ifrit, -1, 0), "Unknown genders fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => ElementalBloodInsightPersistencePolicy.Trait(
                ElementalHeritageRace.Ifrit, 0, 3), "Unknown heritages fail closed.");
        }
    }
}
