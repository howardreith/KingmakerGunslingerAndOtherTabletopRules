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

        internal const int NativeRespecVisits = 8;
        internal static bool IsAllowedRespecCase(string race, string characterClass, string allocation)
        {
            return IsAllowedCase(race, characterClass, allocation) && characterClass == "Fighter" && allocation == "point-buy";
        }
        internal static int NativeRespecChoice(int visit)
        {
            if (visit < 0 || visit >= NativeRespecVisits) throw new ArgumentOutOfRangeException("visit");
            return new[] { 0, 0, 1, 2, 0, 2, 1, 0 }[visit];
        }

        internal static ElementalAlternateTraitId[] NativeRespecTraits(ElementalHeritageRace race, int choice)
        {
            var traits = Traits(race, choice);
            if (choice != 2) return traits;
            return traits.Select(id => id == ElementalAlternateTraitId.EarthInsight ? ElementalAlternateTraitId.StoneInTheBlood :
                id == ElementalAlternateTraitId.AirInsight ? ElementalAlternateTraitId.StormInTheBlood : id).ToArray();
        }

        internal const int NereidRespecVisitsPerSex = 10;
        internal static bool IsAllowedNereidRespecSex(string sex) => sex == "Male" || sex == "Female";
        internal static bool NereidRespecCanceled(int visit)
        {
            if (visit < 0 || visit >= 2 * NereidRespecVisitsPerSex) throw new ArgumentOutOfRangeException("visit");
            return visit % NereidRespecVisitsPerSex == 5 || visit % NereidRespecVisitsPerSex == 9;
        }
        internal static int NereidRespecChoice(int visit)
        {
            if (visit < 0 || visit >= 2 * NereidRespecVisitsPerSex) throw new ArgumentOutOfRangeException("visit");
            return new[] { 0, 0, 1, 2, 0, 2, 2, 1, 0, 2 }[visit % NereidRespecVisitsPerSex];
        }

        internal static int[] NereidRoute(int character)
        {
            if (character < 0 || character >= 6) throw new ArgumentOutOfRangeException("character");
            int heritage = character % 3;
            return new[] { heritage, (heritage + 1) % 3, heritage, heritage, heritage };
        }
        internal static ElementalAlternateTraitId[] NereidTraits(int visit, int revision, bool nativeRespec)
        {
            if (visit < 0 || visit >= (nativeRespec ? 2 * NereidRespecVisitsPerSex : 6) || revision < 0 ||
                revision >= (nativeRespec ? 1 : 5)) throw new ArgumentOutOfRangeException("visit");
            int local = visit % NereidRespecVisitsPerSex;
            bool retain = nativeRespec ? local == 2 || local == 4 || local == 9 : revision == 0 || revision == 3;
            return retain ? new ElementalAlternateTraitId[0] : new[] { ElementalAlternateTraitId.NereidFascination };
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
