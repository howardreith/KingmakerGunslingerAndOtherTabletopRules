using System;

namespace KingmakerGunslinger.Summoning
{
    internal static class IceDevilRules40Policy
    {
        internal const int HitDice = 14;
        internal const int SpellLikeCasterLevel = 13;
        internal const int Strength = 23, Dexterity = 21, Constitution = 22;
        internal const int Intelligence = 25, Wisdom = 22, Charisma = 20;
        internal const int HitPoints = 161, NaturalArmor = 18;
        internal const int FortitudeBase = 9, ReflexBase = 9, WillBase = 4;
        internal const int SpellResistance = 24, DamageReduction = 10;
        internal const int FlySpeedFeet = 60, OmittedLandSpeedFeet = 40;
        internal const int ReachFeet = 10, FearRadiusFeet = 10;
        internal const int Regeneration = 5;
        internal const int ConeSpellLevel = 5, StormSpellLevel = 4, WallSpellLevel = 4;

        internal static int SlowDc(int constitutionBonus)
        { return 10 + HitDice / 2 + constitutionBonus; }

        // This is deliberately a hit predicate, not the wounding predicate
        // used by Bone Devil poison. Final physical/frost damage is irrelevant.
        internal static bool QualifiesForSlow(bool exactOwner,
            bool exactInitiator, bool spear, bool tail, bool hit)
        { return exactOwner && exactInitiator && (spear || tail) && hit; }

        // Preserve the printed bite's full positive Strength contribution
        // while retaining its native secondary attack role and attack penalty.
        // The tail keeps the native half contribution. Already-correct native
        // or installed consumers must not receive a second correction.
        internal static int BiteStrengthCorrection(int strengthBonus,
            float nativeMultiplier, bool secondary)
        {
            return secondary && strengthBonus > 0 &&
                Math.Abs(nativeMultiplier - .5f) < .001f ?
                strengthBonus - (int)(strengthBonus * nativeMultiplier) : 0;
        }

        internal static int WallHitPoints(int casterLevel) { return 3 * casterLevel; }
        internal static int WallDurationRounds(int casterLevel) { return 10 * casterLevel; }
    }
}
