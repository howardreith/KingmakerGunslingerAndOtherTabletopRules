using System;

namespace KingmakerGunslinger.Summoning
{
    internal static class InfernalRules35
    {
        internal const string BoneUnitSymbol =
            "KMG.Summoning.Unit.BoneDevil";
        internal const string WallSectionUnitSymbol =
            "KMG.Summoning.Infernal.BoneDevil.WallSectionUnit";
        internal const string WallSectionUnitGuid =
            "cc2c011c696e4d3e846815c60245465a";
        internal const string WallSectionUnitName =
            "KMG_Summoning_Infernal_BoneDevil_WallSectionUnit";
        internal const int BarbedHitDice = 12;
        internal const int BarbedBaseCriticalThreatMinimum = 20;
        internal const int BarbedEffectiveCriticalThreatMinimum = 19;
        internal const int BarbedFearRacialDcBonus = 0;
        internal const int BoneHitDice = 10;
        internal const int BoneFearRacialDcBonus = 0;
        internal const int BonePoisonDc = 20;

        internal static bool QualifiesForBarbedDefense(bool hit, bool melee,
            bool manufactured, float rawWeaponReachFeet,
            float ordinaryReachFeet)
        {
            return hit && melee && !(manufactured &&
                rawWeaponReachFeet > ordinaryReachFeet);
        }

        internal static bool QualifiesForClawFear(bool exactClaw, bool hit,
            int finalDamage)
        {
            if (finalDamage < 0)
                throw new ArgumentOutOfRangeException("finalDamage");
            return exactClaw && hit && finalDamage > 0;
        }

        internal static int FearDc(int hitDice, int abilityBonus,
            int racialBonus)
        {
            return 10 + hitDice / 2 + abilityBonus + racialBonus;
        }

        internal static int ImpaleStrengthBonus(int strengthBonus)
        {
            return strengthBonus > 0 ? strengthBonus + strengthBonus / 2 :
                strengthBonus;
        }

        internal static bool PoisonQualifies(bool exactSting, bool hit,
            int finalDamage)
        {
            return SummonInjuryPoisonPolicy.ShouldDeliver(exactSting, hit,
                finalDamage);
        }

        internal static string PrivateUnitDonorSymbol(string unitSymbol)
        {
            if (unitSymbol == null)
                throw new ArgumentNullException("unitSymbol");
            return String.Equals(unitSymbol, WallSectionUnitSymbol,
                StringComparison.Ordinal) ? BoneUnitSymbol : null;
        }

        internal static void Validate()
        {
            if (FearDc(BarbedHitDice, 4, BarbedFearRacialDcBonus) != 20 ||
                BarbedBaseCriticalThreatMinimum != 20 ||
                BarbedEffectiveCriticalThreatMinimum != 19 ||
                FearDc(BoneHitDice, 4, BoneFearRacialDcBonus) != 19 ||
                ImpaleStrengthBonus(6) != 9 ||
                ImpaleStrengthBonus(-2) != -2 ||
                PrivateUnitDonorSymbol(WallSectionUnitSymbol) !=
                    BoneUnitSymbol ||
                QualifiesForBarbedDefense(true, true, true, 10, 5) ||
                !QualifiesForBarbedDefense(true, true, false, 10, 5) ||
                QualifiesForClawFear(true, true, 0))
                throw new InvalidOperationException(
                    "Sprint 35 infernal rule contract changed.");
        }
    }
}
