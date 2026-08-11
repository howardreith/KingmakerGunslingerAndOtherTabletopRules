using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal static class ExpandedSummoningSpecialProfiles
    {
        private static readonly string[] ElementalKeys = BuildElementalKeys();
        private static readonly string[] MephitKeys = {
            "air-mephit", "earth-mephit", "fire-mephit", "water-mephit"
        };

        internal static IReadOnlyList<string> NativeElementalKeys
        { get { return Array.AsReadOnly(ElementalKeys); } }
        internal static IReadOnlyList<string> NativeMephitKeys
        { get { return Array.AsReadOnly(MephitKeys); } }

        internal const int LanternHitDice = 2;
        internal const int LanternStrength = 1;
        internal const int LanternDexterity = 11;
        internal const int LanternConstitution = 12;
        internal const int LanternIntelligence = 6;
        internal const int LanternWisdom = 11;
        internal const int LanternCharisma = 10;
        internal const int LanternSpeedFeet = 60;
        internal const int LanternRayRangeFeet = 30;
        internal const int LanternRayProjectiles = 2;
        internal const int LanternRayDiceCount = 1;
        internal const int LanternRayDieSides = 6;
        internal const int LanternDamageReduction = 10;
        internal const int LanternPoisonSaveBonus = 4;
        internal const int LanternEvilSaveAndAcBonus = 2;

        internal static void Validate()
        {
            if (ElementalKeys.Length != 24 || MephitKeys.Length != 4)
                throw new InvalidOperationException(
                    "Native elemental/mephit profile count changed.");
            string[] keys = ElementalKeys.Concat(MephitKeys).ToArray();
            if (keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
                throw new InvalidOperationException("Native reuse keys are duplicated.");
            foreach (string key in keys)
            {
                SummonCreatureSpec creature = ExpandedSummoningCatalog.All.SingleOrDefault(
                    value => value.Key == key);
                if (creature == null || !ExpandedSummoningDonorCatalog.For(key)
                    .DedicatedSummon)
                    throw new InvalidOperationException(
                        "Native reuse requires an exact dedicated donor: " + key + ".");
            }
        }

        private static string[] BuildElementalKeys()
        {
            string[] sizes = { "small", "medium", "large", "huge", "greater", "elder" };
            string[] elements = { "air", "earth", "fire", "water" };
            return sizes.SelectMany(size => elements.Select(element =>
                size + "-" + element + "-elemental")).ToArray();
        }
    }
}
