using System;

namespace KingmakerGunslinger.Summoning
{
    internal static class InfernalRules35
    {
        internal const string BarbedUnitSymbol =
            "KMG.Summoning.Unit.BarbedDevil";
        internal const string BoneUnitSymbol =
            "KMG.Summoning.Unit.BoneDevil";
        internal const string BarbedPrivateIdentityPrefix =
            "KMG.Summoning.Infernal.BarbedDevil.";
        internal const string BonePrivateIdentityPrefix =
            "KMG.Summoning.Infernal.BoneDevil.";
        internal const int AppendedCreatureIdentityCount = 2;
        internal const int AppendedRootIdentityCount = 5;
        internal const int AppendedPrivateIdentityCount = 47;
        internal const int AppendedCatalogIdentityCount =
            AppendedCreatureIdentityCount + AppendedRootIdentityCount +
            AppendedPrivateIdentityCount;
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
        internal const int SpellLikeCasterLevel = 12;
        internal const int UnholyBlightSpellLevel = 4;
        internal const int UnholyBlightRadiusFeet = 20;
        internal const int UnholyBlightUses = 1;
        internal const int UnholyBlightOrdinaryDice = 5;
        internal const int UnholyBlightGoodOutsiderDice = 10;

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

        internal static bool ShouldClearReleasedImpaleHold(bool exactOwner,
            bool exactInitiatorTarget, bool reciprocalTarget)
        {
            return exactOwner && exactInitiatorTarget && !reciprocalTarget;
        }

        internal static bool PoisonQualifies(bool exactSting, bool hit,
            int finalDamage)
        {
            return SummonInjuryPoisonPolicy.ShouldDeliver(exactSting, hit,
                finalDamage);
        }

        internal static bool ShouldBreakBoneInvisibility(bool exactOwner,
            bool factActive, bool ownerInGame, bool targetInGame, bool enemy)
        {
            return exactOwner && factActive && ownerInGame && targetInGame &&
                enemy;
        }

        internal static bool SpellAiCanAct(bool exactOwner, bool inCombat,
            bool hasActiveEffect, bool hasGrantedAbility,
            bool grantedAbilityAvailable)
        {
            return exactOwner && inCombat && !hasActiveEffect &&
                hasGrantedAbility && grantedAbilityAvailable;
        }

        internal static int AbilityModifier(int score)
        { return (int)Math.Floor((score - 10) / 2.0); }

        internal static int UnholyBlightWillDc(int liveCharismaScore)
        {
            return 10 + UnholyBlightSpellLevel +
                AbilityModifier(liveCharismaScore);
        }

        internal static int UnholyBlightDiceCount(bool goodOutsider)
        {
            return goodOutsider ? UnholyBlightGoodOutsiderDice :
                UnholyBlightOrdinaryDice;
        }

        internal static int UnholyBlightDieSides(bool goodOutsider)
        { return goodOutsider ? 6 : 8; }

        internal static int UnholyBlightDamage(int rolledDamage, bool good,
            bool evil, bool saved)
        {
            if (rolledDamage < 0)
                throw new ArgumentOutOfRangeException("rolledDamage");
            if (good && evil)
                throw new ArgumentException(
                    "A creature cannot be both good and evil.");
            if (evil) return 0;
            int divisor = good ? (saved ? 2 : 1) : (saved ? 4 : 2);
            return rolledDamage / divisor;
        }

        internal static bool UnholyBlightSickens(bool good, bool evil,
            bool saved)
        {
            if (good && evil)
                throw new ArgumentException(
                    "A creature cannot be both good and evil.");
            return good && !saved;
        }

        internal static string PrivateUnitDonorSymbol(string unitSymbol)
        {
            if (unitSymbol == null)
                throw new ArgumentNullException("unitSymbol");
            return String.Equals(unitSymbol, WallSectionUnitSymbol,
                StringComparison.Ordinal) ? BoneUnitSymbol : null;
        }

        internal static bool WallNodeIntersects(float alongMeters,
            float acrossMeters, float halfLengthMeters,
            float halfThicknessMeters, float nodeSizeMeters)
        {
            if (halfLengthMeters <= 0 || halfThicknessMeters <= 0 ||
                nodeSizeMeters <= 0 || float.IsNaN(alongMeters) ||
                float.IsNaN(acrossMeters) ||
                float.IsNaN(halfLengthMeters) ||
                float.IsNaN(halfThicknessMeters) ||
                float.IsNaN(nodeSizeMeters) ||
                float.IsInfinity(alongMeters) ||
                float.IsInfinity(acrossMeters) ||
                float.IsInfinity(halfLengthMeters) ||
                float.IsInfinity(halfThicknessMeters) ||
                float.IsInfinity(nodeSizeMeters))
                throw new ArgumentOutOfRangeException("Wall footprint");
            // A square grid cell can project by at most half its diagonal on
            // either arbitrarily rotated wall axis. Include intersecting cells,
            // not only cells whose centres happen to lie inside the thin wall.
            float cellProjection = nodeSizeMeters * .70710678f;
            return Math.Abs(alongMeters) <=
                    halfLengthMeters + cellProjection &&
                Math.Abs(acrossMeters) <=
                    halfThicknessMeters + cellProjection;
        }

        internal static void Validate()
        {
            if (AppendedCatalogIdentityCount != 54 ||
                FearDc(BarbedHitDice, 4, BarbedFearRacialDcBonus) != 20 ||
                BarbedBaseCriticalThreatMinimum != 20 ||
                BarbedEffectiveCriticalThreatMinimum != 19 ||
                FearDc(BoneHitDice, 4, BoneFearRacialDcBonus) != 19 ||
                ImpaleStrengthBonus(6) != 9 ||
                ImpaleStrengthBonus(-2) != -2 ||
                !ShouldClearReleasedImpaleHold(true, true, false) ||
                ShouldClearReleasedImpaleHold(false, true, false) ||
                ShouldClearReleasedImpaleHold(true, false, false) ||
                ShouldClearReleasedImpaleHold(true, true, true) ||
                PrivateUnitDonorSymbol(WallSectionUnitSymbol) !=
                    BoneUnitSymbol ||
                !WallNodeIntersects(1.52f, 0f, 1.525f, .375f, .5f) ||
                !WallNodeIntersects(0f, .6f, 1.525f, .375f, .5f) ||
                WallNodeIntersects(2f, 0f, 1.525f, .375f, .5f) ||
                QualifiesForBarbedDefense(true, true, true, 10, 5) ||
                !QualifiesForBarbedDefense(true, true, false, 10, 5) ||
                QualifiesForClawFear(true, true, 0) ||
                !ShouldBreakBoneInvisibility(true, true, true, true, true) ||
                ShouldBreakBoneInvisibility(false, true, true, true, true) ||
                ShouldBreakBoneInvisibility(true, false, true, true, true) ||
                ShouldBreakBoneInvisibility(true, true, false, true, true) ||
                ShouldBreakBoneInvisibility(true, true, true, false, true) ||
                ShouldBreakBoneInvisibility(true, true, true, true, false) ||
                !SpellAiCanAct(true, true, false, true, true) ||
                SpellAiCanAct(false, true, false, true, true) ||
                SpellAiCanAct(true, false, false, true, true) ||
                SpellAiCanAct(true, true, true, true, true) ||
                SpellAiCanAct(true, true, false, false, true) ||
                SpellAiCanAct(true, true, false, true, false) ||
                SpellLikeCasterLevel != 12 ||
                UnholyBlightSpellLevel != 4 ||
                UnholyBlightRadiusFeet != 20 || UnholyBlightUses != 1 ||
                UnholyBlightWillDc(18) != 18 ||
                UnholyBlightDiceCount(false) != 5 ||
                UnholyBlightDieSides(false) != 8 ||
                UnholyBlightDiceCount(true) != 10 ||
                UnholyBlightDieSides(true) != 6 ||
                UnholyBlightDamage(40, true, false, false) != 40 ||
                UnholyBlightDamage(40, true, false, true) != 20 ||
                UnholyBlightDamage(40, false, false, false) != 20 ||
                UnholyBlightDamage(40, false, false, true) != 10 ||
                UnholyBlightDamage(40, false, true, false) != 0 ||
                !UnholyBlightSickens(true, false, false) ||
                UnholyBlightSickens(true, false, true) ||
                UnholyBlightSickens(false, false, false))
                throw new InvalidOperationException(
                    "Sprint 35 infernal rule contract changed.");
        }
    }
}
