using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalBloodInsightPersistencePolicyTests
    {
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
