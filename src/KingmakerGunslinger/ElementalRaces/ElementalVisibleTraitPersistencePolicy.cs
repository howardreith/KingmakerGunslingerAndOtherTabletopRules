using System;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalVisibleTraitPersistencePolicy
    {
        internal const string MatrixId = "release-c-visible-nineteen-traits-v5";
        internal static ElementalAlternateTraitId[] Traits(ElementalHeritageRace race, int genderIndex, int heritageIndex, bool nereidQualification = false)
        {
            if (genderIndex < 0 || genderIndex > 1) throw new ArgumentOutOfRangeException("genderIndex");
            if (heritageIndex < 0 || heritageIndex > 2) throw new ArgumentOutOfRangeException("heritageIndex");
            if (nereidQualification && race == ElementalHeritageRace.Undine)
                return new[] { ElementalAlternateTraitId.NereidFascination };
            int row = genderIndex * 3 + heritageIndex;
            switch (race)
            {
                case ElementalHeritageRace.Ifrit:
                    switch (row)
                    {
                        case 0: return new[] { ElementalAlternateTraitId.WildfireHeart, ElementalAlternateTraitId.FireInTheBlood, ElementalAlternateTraitId.EfreetiMagic };
                        case 1: return new[] { ElementalAlternateTraitId.BrazenFlame, ElementalAlternateTraitId.FireInsight };
                        case 2: return new[] { ElementalAlternateTraitId.WildfireHeart, ElementalAlternateTraitId.FireInTheBlood, ElementalAlternateTraitId.ForgeHardened };
                        case 3: return new[] { ElementalAlternateTraitId.BrazenFlame, ElementalAlternateTraitId.FireInTheBlood };
                        case 4: return new[] { ElementalAlternateTraitId.WildfireHeart, ElementalAlternateTraitId.FireInsight, ElementalAlternateTraitId.EfreetiMagic };
                        case 5: return new[] { ElementalAlternateTraitId.FireInsight, ElementalAlternateTraitId.ForgeHardened };
                    }
                    break;
                case ElementalHeritageRace.Oread:
                    switch (row)
                    {
                        case 0: return new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.StoneInTheBlood };
                        case 1: return new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.EarthInsight };
                        case 2: return new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.CrystallineForm };
                        case 3: return new[] { ElementalAlternateTraitId.StoneInTheBlood };
                        case 4: return new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.EarthInsight };
                        case 5: return new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.CrystallineForm };
                    }
                    break;
                case ElementalHeritageRace.Sylph:
                    switch (row)
                    {
                        case 0: return new[] { ElementalAlternateTraitId.LikeTheWind, ElementalAlternateTraitId.StormInTheBlood, ElementalAlternateTraitId.WhisperingWind };
                        case 1: return new[] { ElementalAlternateTraitId.ThunderousResilience, ElementalAlternateTraitId.BreezeKissed, ElementalAlternateTraitId.WhisperingWind };
                        case 2: return new[] { ElementalAlternateTraitId.Secretive, ElementalAlternateTraitId.AirInsight };
                        case 3: return new[] { ElementalAlternateTraitId.ThunderousResilience, ElementalAlternateTraitId.StormInTheBlood, ElementalAlternateTraitId.WhisperingWind };
                        case 4: return new[] { ElementalAlternateTraitId.LikeTheWind, ElementalAlternateTraitId.BreezeKissed, ElementalAlternateTraitId.WhisperingWind };
                        case 5: return new[] { ElementalAlternateTraitId.Secretive, ElementalAlternateTraitId.AirInsight };
                    }
                    break;
                case ElementalHeritageRace.Undine:
                    switch (row)
                    {
                        case 0: return new[] { ElementalAlternateTraitId.AcidBreath };
                        case 1: return new[] { ElementalAlternateTraitId.OozeBreath };
                        case 2: return new[] { ElementalAlternateTraitId.AcidBreath };
                        case 3: return new[] { ElementalAlternateTraitId.OozeBreath };
                        case 4: return new[] { ElementalAlternateTraitId.AcidBreath };
                        case 5: return new[] { ElementalAlternateTraitId.OozeBreath };
                    }
                    break;
            }
            throw new ArgumentOutOfRangeException("race");
        }
    }
}
