using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Frozen adult Pathfinder First Edition arithmetic and identity names for
    /// Sprint 24. Runtime components still read live Strength, size, attacks
    /// and native rule outcomes; this class owns no combat state.
    /// </summary>
    internal static class TheropodRulesPolicy
    {
        internal const string DeinonychusKey = "deinonychus";
        internal const string TyrannosaurusKey = "tyrannosaurus";

        internal const string DeinonychusTalonSymbol =
            "KMG.Summoning.Natural.Deinonychus.Talon1d8";
        internal const string DeinonychusUnitTypeSymbol =
            "KMG.Summoning.Natural.Deinonychus.UnitType";
        internal const string DeinonychusChassisSymbol =
            "KMG.Summoning.Natural.Deinonychus.Chassis";
        internal const string TyrannosaurusBiteSymbol =
            "KMG.Summoning.Natural.Tyrannosaurus.Bite4d6";
        internal const string TyrannosaurusUnitTypeSymbol =
            "KMG.Summoning.Natural.Tyrannosaurus.UnitType";
        internal const string TyrannosaurusChassisSymbol =
            "KMG.Summoning.Natural.Tyrannosaurus.Chassis";
        internal const string TyrannosaurusPowerfulBiteSymbol =
            "KMG.Summoning.Special.Tyrannosaurus.PowerfulBite";
        internal const string TyrannosaurusCombatTraitsSymbol =
            "KMG.Summoning.Special.Tyrannosaurus.CombatTraits";
        internal const string TyrannosaurusSwallowedSymbol =
            "KMG.Summoning.Special.Tyrannosaurus.Swallowed";
        internal const string TyrannosaurusBleedingCriticalSymbol =
            "KMG.Summoning.Special.Tyrannosaurus.BleedingCritical";

        // One current authority for the append. Historical sprint tests use
        // these deltas when proving their own immutable prefix; they do not
        // acquire another copy of the current global totals.
        internal const int AppendedIdentityCount = 48;
        internal const int AppendedCreatureCount = 2;
        internal const int AppendedAbilityIdentityCount = 36;
        internal const int AppendedBuffIdentityCount = 2;
        internal const int AppendedWeaponIdentityCount = 2;
        internal const int AppendedUnitTypeIdentityCount = 2;
        internal const int AppendedFeatureIdentityCount = 4;
        internal const int AppendedPrivateIdentityCount =
            AppendedBuffIdentityCount + AppendedWeaponIdentityCount +
            AppendedUnitTypeIdentityCount + AppendedFeatureIdentityCount;
        internal const int AppendedMonsterPlacements = 9;
        internal const int AppendedNatureAllyPlacements = 9;
        internal const int AppendedTemplateExecutionsPerAlignment = 9;
        internal const int RegisteredCreatureCount = 107;
        internal const int RegisteredMonsterCreatureCount = 95;
        internal const int RegisteredNatureAllyCreatureCount = 95;
        internal const int RegisteredMonsterPlacements = 543;
        internal const int RegisteredNatureAllyPlacements = 550;
        internal const int RegisteredLogicalPlacements = 1093;
        internal const int HiddenLogicalPlacements = 18;
        internal const int HiddenPublishedLogicalPlacements = 1075;
        internal const int FinalPublishedLogicalPlacements = 1093;
        internal const int RetainedNativeWrappers = 29;
        internal const int HiddenVisibleChoices = 1104;
        internal const int PublicVisibleChoices = 1122;

        internal const int DeinonychusBaseRacialHitPoints = 18;
        internal const int DeinonychusMobilityRanks = 0;
        internal const int DeinonychusPerceptionRanks = 2;
        internal const int DeinonychusStealthRanks = 2;
        internal const int DeinonychusRacialSkillBonus = 8;

        internal const int TyrannosaurusBaseRacialHitPoints = 81;
        internal const int TyrannosaurusMobilityRanks = 0;
        internal const int TyrannosaurusPerceptionRanks = 18;
        internal const int TyrannosaurusStealthRanks = 0;
        internal const int TyrannosaurusRacialPerceptionBonus = 8;
        internal const int TyrannosaurusSkillFocusCompletionBonus = 3;

        internal const int TyrannosaurusGrabMaximumTargetSizeDelta = 0;
        internal const int TyrannosaurusSwallowMaximumTargetSizeDelta = -1;
        internal const int TyrannosaurusGrappleBonus = 4;
        internal const int TyrannosaurusSwallowDiceCount = 2;
        internal const int TyrannosaurusSwallowFlatDamage = 11;
        internal const int TyrannosaurusInteriorArmorClass = 17;
        internal const int TyrannosaurusInteriorHitPoints = 15;

        // A reconstructed Gargantuan body exposes fifteen feet before its
        // creature-owned five-foot correction. Native proof must confirm the
        // final twenty-foot uniform reach before publication.
        internal const int TyrannosaurusReachBonusFeet = 5;

        internal static bool UsesExactRanks(string key)
        {
            return IsTheropod(key);
        }

        internal static bool IsTheropod(string key)
        {
            return key == DeinonychusKey || key == TyrannosaurusKey;
        }

        internal static int NativeOrdinaryBiteStrengthContribution(
            int strengthModifier)
        {
            return strengthModifier > 0
                ? checked(strengthModifier + strengthModifier / 2)
                : strengthModifier;
        }

        internal static int PowerfulBiteCorrection(int strengthModifier)
        {
            return checked(strengthModifier * 2 -
                NativeOrdinaryBiteStrengthContribution(strengthModifier));
        }

        internal static int MultipleNaturalPrimaryExcessStrength(
            int strengthModifier)
        {
            return checked(NativeOrdinaryBiteStrengthContribution(
                strengthModifier) - strengthModifier);
        }

        internal static bool CanGrab(int ownerSizeOrder, int targetSizeOrder)
        {
            return ownerSizeOrder >= 0 && targetSizeOrder >= 0 &&
                targetSizeOrder <= ownerSizeOrder +
                    TyrannosaurusGrabMaximumTargetSizeDelta;
        }

        internal static bool CanSwallow(int ownerSizeOrder, int targetSizeOrder)
        {
            return ownerSizeOrder >= 0 && targetSizeOrder >= 0 &&
                targetSizeOrder <= ownerSizeOrder +
                    TyrannosaurusSwallowMaximumTargetSizeDelta;
        }

        internal static bool ShouldApplyBleedingCritical(bool exactOwner,
            bool exactBite, bool hit, bool criticalConfirmed,
            bool immuneToCritical, bool targetAvailable, bool firstEvent)
        {
            return exactOwner && exactBite && hit && criticalConfirmed &&
                !immuneToCritical && targetAvailable && firstEvent;
        }

        internal static void Validate()
        {
            if (PowerfulBiteCorrection(9) != 5 ||
                PowerfulBiteCorrection(10) != 5 ||
                PowerfulBiteCorrection(11) != 6 ||
                PowerfulBiteCorrection(12) != 6 ||
                PowerfulBiteCorrection(0) != 0 ||
                PowerfulBiteCorrection(-1) != -1 ||
                PowerfulBiteCorrection(-5) != -5 ||
                MultipleNaturalPrimaryExcessStrength(2) != 1 ||
                MultipleNaturalPrimaryExcessStrength(3) != 1 ||
                MultipleNaturalPrimaryExcessStrength(0) != 0 ||
                MultipleNaturalPrimaryExcessStrength(-1) != 0 ||
                DeinonychusBaseRacialHitPoints + 4 * 4 != 34 ||
                TyrannosaurusBaseRacialHitPoints + 18 * 4 != 153 ||
                DeinonychusPerceptionRanks + 3 + 1 +
                    DeinonychusRacialSkillBonus != 14 ||
                DeinonychusStealthRanks + 3 + 2 +
                    DeinonychusRacialSkillBonus != 15 ||
                TyrannosaurusPerceptionRanks + 3 + 2 +
                    TyrannosaurusRacialPerceptionBonus + 3 +
                    TyrannosaurusSkillFocusCompletionBonus != 37 ||
                !CanGrab(3, 3) || CanGrab(3, 4) ||
                CanSwallow(3, 3) || !CanSwallow(3, 2) ||
                !ShouldApplyBleedingCritical(true, true, true, true,
                    false, true, true) ||
                ShouldApplyBleedingCritical(true, true, true, false,
                    false, true, true) ||
                ShouldApplyBleedingCritical(true, true, true, true,
                    true, true, true) ||
                AppendedAbilityIdentityCount +
                    AppendedPrivateIdentityCount +
                    AppendedCreatureCount != AppendedIdentityCount ||
                RegisteredMonsterPlacements +
                    RegisteredNatureAllyPlacements !=
                    RegisteredLogicalPlacements ||
                AppendedMonsterPlacements +
                    AppendedNatureAllyPlacements !=
                    HiddenLogicalPlacements ||
                HiddenLogicalPlacements != 18 ||
                RegisteredLogicalPlacements - HiddenLogicalPlacements !=
                    HiddenPublishedLogicalPlacements ||
                HiddenVisibleChoices != HiddenPublishedLogicalPlacements +
                    RetainedNativeWrappers ||
                PublicVisibleChoices != FinalPublishedLogicalPlacements +
                    RetainedNativeWrappers)
                throw new InvalidOperationException(
                    "Sprint 24 theropod arithmetic changed.");
        }
    }
}
