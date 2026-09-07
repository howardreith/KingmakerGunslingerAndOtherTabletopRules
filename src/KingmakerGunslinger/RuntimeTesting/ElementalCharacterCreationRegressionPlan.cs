using System;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-local coverage data only; these combinations never alter production catalogs.
    internal static class ElementalCharacterCreationRegressionPlan
    {
        internal static bool IsAllowedCase(string race, string characterClass, string allocation)
        {
            return new[] { "Ifrit", "Oread", "Sylph", "Undine" }.Contains(race) &&
                new[] { "Fighter", "Gunslinger" }.Contains(characterClass) &&
                new[] { "point-buy", "roll" }.Contains(allocation);
        }

        internal static int[] Route(int character)
        {
            switch (character)
            {
                case 0: return new[] { 0, 1, 0 };
                case 1: return new[] { 1, 2 };
                case 2: return new[] { 2, 0, 1 };
                default: throw new ArgumentOutOfRangeException("character");
            }
        }

        internal static ElementalAlternateTraitId[] Traits(ElementalHeritageRace race, int choice)
        {
            if (choice < 0 || choice > 2) throw new ArgumentOutOfRangeException("choice");
            if (choice == 0) return new ElementalAlternateTraitId[0];
            switch (race)
            {
                case ElementalHeritageRace.Ifrit:
                    return choice == 1 ? new[] { ElementalAlternateTraitId.BrazenFlame, ElementalAlternateTraitId.FireInTheBlood }
                        : new[] { ElementalAlternateTraitId.WildfireHeart, ElementalAlternateTraitId.FireInsight, ElementalAlternateTraitId.ForgeHardened };
                case ElementalHeritageRace.Oread:
                    return choice == 1 ? new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.CrystallineForm }
                        : new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.EarthInsight };
                case ElementalHeritageRace.Sylph:
                    return choice == 1 ? new[] { ElementalAlternateTraitId.Secretive, ElementalAlternateTraitId.BreezeKissed }
                        : new[] { ElementalAlternateTraitId.LikeTheWind, ElementalAlternateTraitId.AirInsight, ElementalAlternateTraitId.WhisperingWind };
                case ElementalHeritageRace.Undine:
                    return new[] { choice == 1 ? ElementalAlternateTraitId.AcidBreath : ElementalAlternateTraitId.OozeBreath };
                default: throw new ArgumentOutOfRangeException("race");
            }
        }
    }
}
