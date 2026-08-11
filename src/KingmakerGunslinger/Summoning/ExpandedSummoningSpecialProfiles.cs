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

        internal const int InvisibleStalkerHitDice = 7;
        internal const int InvisibleStalkerStrength = 18;
        internal const int InvisibleStalkerDexterity = 19;
        internal const int InvisibleStalkerConstitution = 22;
        internal const int InvisibleStalkerIntelligence = 14;
        internal const int InvisibleStalkerWisdom = 15;
        internal const int InvisibleStalkerCharisma = 11;
        internal const int InvisibleStalkerSpeedFeet = 30;

        internal const int ShadowDemonHitDice = 7;
        internal const int ShadowDemonStrength = 17;
        internal const int ShadowDemonDexterity = 20;
        internal const int ShadowDemonConstitution = 14;
        internal const int ShadowDemonIntelligence = 14;
        internal const int ShadowDemonWisdom = 13;
        internal const int ShadowDemonCharisma = 17;
        internal const int ShadowDemonSpeedFeet = 40;
        internal const int ShadowDemonDamageReduction = 10;
        internal const int ShadowDemonEnergyResistance = 10;
        internal const int ShadowDemonSpellResistance = 17;
        internal const int ShadowDemonColdDamageDice = 1;

        internal const int SalamanderHitDice = 8;
        internal const int SalamanderStrength = 16;
        internal const int SalamanderDexterity = 13;
        internal const int SalamanderConstitution = 18;
        internal const int SalamanderIntelligence = 14;
        internal const int SalamanderWisdom = 15;
        internal const int SalamanderCharisma = 13;
        internal const int SalamanderSpeedFeet = 20;
        internal const int SalamanderHeatDice = 1;
        internal const int SalamanderConstrictDice = 2;
        internal const int SalamanderConstrictBonus = 4;

        internal const int SuccubusHitDice = 8;
        internal const int SuccubusStrength = 13;
        internal const int SuccubusDexterity = 17;
        internal const int SuccubusConstitution = 14;
        internal const int SuccubusIntelligence = 18;
        internal const int SuccubusWisdom = 13;
        internal const int SuccubusCharisma = 27;
        internal const int SuccubusSpeedFeet = 30;
        internal const int SuccubusDamageReduction = 10;
        internal const int SuccubusEnergyResistance = 10;
        internal const int SuccubusSpellResistance = 18;
        internal const int SuccubusDominateRounds = 3;
        internal const int SuccubusEnergyDrainRounds = 1;

        internal const int BebelithHitDice = 12;
        internal const int BebelithStrength = 28;
        internal const int BebelithDexterity = 12;
        internal const int BebelithConstitution = 24;
        internal const int BebelithIntelligence = 11;
        internal const int BebelithWisdom = 13;
        internal const int BebelithCharisma = 13;
        internal const int BebelithSpeedFeet = 40;
        internal const int BebelithDamageReduction = 10;
        internal const int BebelithDismantleReflexDc = 25;
        internal const int BebelithDismantleAcPenalty = 2;
        internal const int BebelithDismantleRounds = 1;
        internal const int BebelithDemonHunterBonus = 2;

        internal const int PixieHitDice = 4;
        internal const int PixieStrength = 7;
        internal const int PixieDexterity = 21;
        internal const int PixieConstitution = 12;
        internal const int PixieIntelligence = 16;
        internal const int PixieWisdom = 15;
        internal const int PixieCharisma = 16;
        internal const int PixieSpeedFeet = 60;
        internal const int PixieDamageReduction = 10;
        internal const int PixieSpellResistance = 15;
        internal const int PixieSleepArrowUses = 16;
        internal const int PixieSleepArrowWillDc = 15;
        internal const int PixieSleepArrowRounds = 50;
        internal const int PixieDanceUses = 1;
        internal const int PixieDanceCasterLevel = 8;
        internal const int PixieDanceSpellLevel = 6;

        internal static bool ShouldAttemptBebelithDismantle(bool isClaw,
            bool isHit, bool targetHasArmor, int priorClawHits,
            bool alreadyAttempted)
        {
            return isClaw && isHit && targetHasArmor && priorClawHits == 1 &&
                !alreadyAttempted;
        }

        internal static bool IsBebelithDemonHuntingTarget(bool isOutsider,
            int alignment)
        { return isOutsider && alignment == 20; }

        internal static bool ShouldSpendPixieSleepArrow(bool isSleepBow,
            bool isHit, int remainingUses)
        { return isSleepBow && isHit && remainingUses > 0; }

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
            if (BebelithHitDice != 12 || BebelithDismantleReflexDc != 25 ||
                BebelithDismantleAcPenalty < 1 || BebelithDismantleRounds != 1 ||
                PixieHitDice != 4 || PixieSleepArrowUses != 16 ||
                PixieSleepArrowWillDc != 15 || PixieSleepArrowRounds != 50 ||
                PixieDanceUses != 1 || PixieDanceCasterLevel != 8)
                throw new InvalidOperationException(
                    "Bebelith/Pixie bounded special profile changed.");
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
