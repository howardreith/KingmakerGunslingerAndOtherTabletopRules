using System;

namespace KingmakerGunslinger.Summoning
{
    internal enum AnkylosaurusControl { None, Dazed, Stunned }

    internal static class AnkylosaurusRulesPolicy
    {
        internal const string Key = "ankylosaurus";
        internal const int HitDice = 10;
        internal const int BaseRacialHitPoints = 45;
        internal const int PerceptionRanks = 10;
        internal const int ReachBonusFeet = 3;
        // BlueprintWeaponType subtracts four feet from melee values at or
        // below ten.  Nine therefore contributes five feet; the corrected
        // 15-foot body contributes another ten through ReachRange.
        internal const int TailTypeStoredRangeFeet = 9;
        internal const int PowerAttackAdditionalDamage = 2;
        internal const string UnitTypeSymbol = "KMG.Summoning.Natural.Ankylosaurus.UnitType";
        internal const string ChassisFeatureSymbol = "KMG.Summoning.Natural.Ankylosaurus.Chassis";
        internal const string TailTypeSymbol = "KMG.Summoning.Natural.Ankylosaurus.TailType";
        internal const string StunFeatureSymbol = "KMG.Summoning.Special.Ankylosaurus.Stun";
        internal const string DazedSymbol = "KMG.Summoning.Special.Ankylosaurus.Dazed";
        internal const string StunnedSymbol = "KMG.Summoning.Special.Ankylosaurus.Stunned";

        internal static int DifficultyClass(int racialHitDice, int currentStrengthBonus)
        {
            if (racialHitDice < 1) throw new ArgumentOutOfRangeException("racialHitDice");
            return 10 + racialHitDice / 2 + currentStrengthBonus;
        }

        internal static AnkylosaurusControl Decide(bool exactOwner, bool exactTail,
            bool hit, bool targetAvailable, bool firstEvent, bool criticalConfirmed,
            bool criticalImmune)
        {
            if (!exactOwner || !exactTail || !hit || !targetAvailable || !firstEvent)
                return AnkylosaurusControl.None;
            return criticalConfirmed && !criticalImmune ? AnkylosaurusControl.Stunned : AnkylosaurusControl.Dazed;
        }

        internal static int Rounds(AnkylosaurusControl control, int nativeD4)
        {
            if (control == AnkylosaurusControl.None) return 0;
            if (control == AnkylosaurusControl.Dazed) return 1;
            if (nativeD4 < 1 || nativeD4 > 4) throw new ArgumentOutOfRangeException("nativeD4");
            return nativeD4;
        }
    }
}
