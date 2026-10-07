using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Pure tabletop-to-runtime gates shared by the Sprint 12 bite
    /// disease components and their detached domain tests.</summary>
    internal static class SummonInjuryDiseasePolicy
    {
        internal const int DireRatFortitudeDc = 11;
        internal const int GoblinDogFortitudeDc = 12;
        internal const int GoblinDogAllergyDurationSeconds = 24 * 60 * 60;
        internal const int GoblinDogAllergyAbilityPenalty = -2;

        internal static bool ShouldResolve(bool exactBite, bool hit,
            int actualDamage, bool targetAvailable, bool excludedUnitType)
        {
            if (actualDamage < 0)
                throw new ArgumentOutOfRangeException("actualDamage");
            return exactBite && hit && actualDamage > 0 && targetAvailable &&
                !excludedUnitType;
        }

        internal static bool IsMagicalAbility(bool spell, bool spellLike,
            bool supernatural)
        {
            return spell || spellLike || supernatural;
        }

        internal static bool ShouldRemoveAllergicReaction(int actualHealing,
            bool spell, bool spellLike, bool supernatural)
        {
            if (actualHealing < 0)
                throw new ArgumentOutOfRangeException("actualHealing");
            return actualHealing > 0 &&
                IsMagicalAbility(spell, spellLike, supernatural);
        }
    }
}
