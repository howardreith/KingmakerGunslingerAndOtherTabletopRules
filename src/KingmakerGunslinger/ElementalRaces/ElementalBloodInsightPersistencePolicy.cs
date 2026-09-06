using System;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>Incremental six-trait save matrix, distinct from the historical
    /// retain-base matrix. No unimplemented trait is used as a fixture.</summary>
    internal static class ElementalBloodInsightPersistencePolicy
    {
        internal const string MatrixId = "release-c-blood-insight-six-traits-v1";

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
