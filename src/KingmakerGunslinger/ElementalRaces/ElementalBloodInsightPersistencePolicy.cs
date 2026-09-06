using System;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>Incremental six-trait save matrix, distinct from the historical
    /// retain-base matrix. No unimplemented trait is used as a fixture.</summary>
    internal static class ElementalBloodInsightPersistencePolicy
    {
        internal const string MatrixId = "release-c-blood-insight-six-traits-v1";
        internal const string EfreetiMatrixId = "release-c-efreeti-blood-insight-seven-traits-v2";
        internal const string CrystallineMatrixId = "release-c-crystalline-efreeti-blood-insight-eight-traits-v3";

        // Keep the earlier matrices executable and unchanged. This incremental
        // matrix adds both sexes of Ironsoul without losing any earlier trait.
        internal static ElementalAlternateTraitId[] CrystallineTraits(ElementalHeritageRace race,
            int genderIndex, int heritageIndex)
        {
            ElementalAlternateTraitId[] previous = Traits(race, genderIndex, heritageIndex);
            return race == ElementalHeritageRace.Oread && heritageIndex == 2
                ? new[] { ElementalAlternateTraitId.CrystallineForm } : previous;
        }

        // Retain the original six-trait policy as a separately testable
        // historical matrix. The next matrix adds a legal, disjoint SLA
        // replacement to every Ifrit fixture without losing blood/Insight rows.
        internal static ElementalAlternateTraitId[] Traits(ElementalHeritageRace race,
            int genderIndex, int heritageIndex)
        {
            ElementalAlternateTraitId? affinity = Trait(race, genderIndex, heritageIndex);
            if (race == ElementalHeritageRace.Ifrit)
                return new[] { affinity.Value, ElementalAlternateTraitId.EfreetiMagic };
            return affinity.HasValue ? new[] { affinity.Value } : new ElementalAlternateTraitId[0];
        }

        internal static int EfreetiVariantIndex(int genderIndex, int heritageIndex)
        {
            Trait(ElementalHeritageRace.Ifrit, genderIndex, heritageIndex);
            return (genderIndex + heritageIndex) % 2;
        }

        internal static ElementalAlternateTraitId? Trait(ElementalHeritageRace race,
            int genderIndex, int heritageIndex)
        {
            if (genderIndex < 0 || genderIndex > 1)
                throw new ArgumentOutOfRangeException("genderIndex");
            if (heritageIndex < 0 || heritageIndex >= ElementalHeritagePolicy.ChoicesPerRace)
                throw new ArgumentOutOfRangeException("heritageIndex");
            bool blood = (genderIndex + heritageIndex) % 2 == 0;
            switch (race)
            {
                case ElementalHeritageRace.Ifrit:
                    return blood ? ElementalAlternateTraitId.FireInTheBlood : ElementalAlternateTraitId.FireInsight;
                case ElementalHeritageRace.Oread:
                    return blood ? ElementalAlternateTraitId.StoneInTheBlood : ElementalAlternateTraitId.EarthInsight;
                case ElementalHeritageRace.Sylph:
                    return blood ? ElementalAlternateTraitId.StormInTheBlood : ElementalAlternateTraitId.AirInsight;
                case ElementalHeritageRace.Undine: return null;
                default: throw new ArgumentOutOfRangeException("race");
            }
        }
    }
}
