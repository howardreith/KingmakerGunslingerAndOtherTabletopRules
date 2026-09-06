using System;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalSummonInsightPolicyTests
    {
        internal static void RequiresTypedTemporaryFamilySpell()
        {
            foreach (ElementalAlternateTraitId trait in Enum.GetValues(
                typeof(ElementalAlternateTraitId)))
                for (int mask = 0; mask < 16; mask++)
                {
                    bool insight = trait == ElementalAlternateTraitId.FireInsight ||
                        trait == ElementalAlternateTraitId.EarthInsight ||
                        trait == ElementalAlternateTraitId.AirInsight;
                    Assertions.Equal(insight && mask == 15 ? 2 : 0,
                        ElementalSummonInsightPolicy.BonusRounds(trait,
                            (mask & 1) != 0, (mask & 2) != 0,
                            (mask & 4) != 0, (mask & 8) != 0),
                        trait + "/" + mask + ": exactly two rounds only for " +
                        "a matching temporary linked family spell summon.");
                }
            Assertions.Equal(18, ElementalSummonInsightPolicy.NativeParentGuids
                .Distinct(StringComparer.Ordinal).Count(),
                "Both native spell families must retain all nine ranks.");
            Assertions.Equal(3, new[] { ElementalAlternateTraitId.FireInsight,
                ElementalAlternateTraitId.EarthInsight,
                ElementalAlternateTraitId.AirInsight }.Select(
                    ElementalSummonInsightPolicy.NativeSubtypeGuid).Distinct(
                        StringComparer.Ordinal).Count(),
                "The three traits must not conflate their native subtype identities.");
        }
    }
}
