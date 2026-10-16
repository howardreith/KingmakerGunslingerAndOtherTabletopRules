using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Frozen adult Pathfinder First Edition arithmetic and identity names for
    /// Sprint 25, Giant Bodies. Runtime components still read live Strength,
    /// size, attacks and native rule outcomes; this class owns no combat state.
    ///
    /// <para>Brachiosaurus is the new creature and is Summon Nature's Ally VII
    /// only: three roots at parents VII, VIII and IX, no Summon Monster row and
    /// no celestial or fiendish execution children. The owner settled that on
    /// 2026-10-15 against a recorded source discrepancy - the printed Summon
    /// Monster VII list and this project's own ideal-roster catalog both place
    /// Brachiosaurus on the Monster table as well - and chose the reversible
    /// direction, because identities are allocated once and append-only.
    /// Nothing here is authority to add those Monster roots.</para>
    ///
    /// <para>Elephant is an existing released Summon Monster and Nature's Ally
    /// VI creature. Sprint 25 completes it; it does not re-register it. Its
    /// identity, its eight existing roots, their execution children and their
    /// order are preserved exactly, and actual Mastodon units and assets are
    /// left alone.</para>
    /// </summary>
    internal static class GiantBodyRulesPolicy
    {
        internal const string BrachiosaurusKey = "brachiosaurus";
        internal const string ElephantKey = "elephant";

        internal const string BrachiosaurusTailSymbol =
            "KMG.Summoning.Natural.Brachiosaurus.Tail4d6";
        internal const string BrachiosaurusTailTypeSymbol =
            "KMG.Summoning.Natural.Brachiosaurus.TailType";
        internal const string BrachiosaurusUnitTypeSymbol =
            "KMG.Summoning.Natural.Brachiosaurus.UnitType";
        internal const string BrachiosaurusChassisSymbol =
            "KMG.Summoning.Natural.Brachiosaurus.Chassis";
        internal const string BrachiosaurusTrampleSymbol =
            "KMG.Summoning.Special.Brachiosaurus.Trample";
        internal const string ElephantChassisSymbol =
            "KMG.Summoning.Natural.Elephant.Chassis";
        internal const string ElephantUnitTypeSymbol =
            "KMG.Summoning.Natural.Elephant.UnitType";
        internal const string ElephantTrampleSymbol =
            "KMG.Summoning.Special.Elephant.Trample";

        // One current authority for the append. Historical sprint tests use
        // these deltas when proving their own immutable prefix; they do not
        // acquire another copy of the current global totals.
        //
        // Three logical Nature's Ally abilities, one new creature, and five
        // private blueprint identities, all five belonging to the new
        // Brachiosaurus: its own unit type so it never reads as a donor, its
        // own 4d6 tail and tail type because no native blueprint carries that
        // routine at twenty-foot reach, a chassis carrier for the size and
        // reach corrections, and its own trample ability.
        //
        // The three Elephant symbols above are reserved names, not registered
        // identities. Elephant is an existing released creature, and the order
        // is to check its actual production carriers against the frozen entry
        // before adding anything. Whatever the bounded native census proves
        // missing is appended then, with this count moved in the same commit.
        internal const int AppendedIdentityCount = 9;
        internal const int AppendedCreatureCount = 1;
        internal const int AppendedAbilityIdentityCount = 3;
        internal const int AppendedWeaponIdentityCount = 1;
        internal const int AppendedWeaponTypeIdentityCount = 1;
        internal const int AppendedUnitTypeIdentityCount = 1;
        internal const int AppendedFeatureIdentityCount = 1;
        internal const int AppendedTrampleAbilityIdentityCount = 1;
        internal const int AppendedPrivateIdentityCount =
            AppendedWeaponIdentityCount + AppendedWeaponTypeIdentityCount +
            AppendedUnitTypeIdentityCount + AppendedFeatureIdentityCount +
            AppendedTrampleAbilityIdentityCount;

        /// <summary>
        /// Every appended identity whose planned type is BlueprintAbility:
        /// the three logical roots plus the trample, which is counted as a
        /// private carrier but is still an ability.
        /// </summary>
        internal const int AppendedTotalAbilityIdentityCount =
            AppendedAbilityIdentityCount + AppendedTrampleAbilityIdentityCount;

        /// <summary>
        /// Elephant carriers named but deliberately not yet registered,
        /// pending the bounded native census of its real production carriers.
        /// </summary>
        internal const int ReservedElephantIdentityCount = 3;

        /// <summary>Brachiosaurus has no Monster row, so nothing is appended
        /// to the Monster surface and no template children are created.</summary>
        internal const int AppendedMonsterPlacements = 0;
        internal const int AppendedNatureAllyPlacements = 3;
        internal const int AppendedTemplateExecutionsPerAlignment = 0;
        internal const int AppendedTemplatedPlacements = 0;

        internal const int RegisteredCreatureCount = 108;
        internal const int RegisteredMonsterCreatureCount = 95;
        internal const int RegisteredNatureAllyCreatureCount = 96;
        internal const int RegisteredMonsterPlacements = 543;
        internal const int RegisteredNatureAllyPlacements = 553;
        internal const int RegisteredLogicalPlacements = 1096;
        internal const int RegisteredTemplatedPlacements = 319;
        internal const int HiddenLogicalPlacements = 3;
        internal const int HiddenPublishedLogicalPlacements = 1093;
        internal const int FinalPublishedLogicalPlacements = 1096;
        internal const int RetainedNativeWrappers = 29;
        internal const int HiddenVisibleChoices = 1122;
        internal const int PublicVisibleChoices = 1125;
        internal const int PublicSuppressedLogicalPlacements = 0;

        // Brachiosaurus, frozen adult entry. Both sources agree on every field
        // below. hp 171 (18d8+90) with Con 21, so the racial base is
        // 171 - 18 * 5.
        internal const int BrachiosaurusHitDice = 18;
        internal const int BrachiosaurusBaseRacialHitPoints = 81;
        internal const int BrachiosaurusStrength = 37;
        internal const int BrachiosaurusDexterity = 10;
        internal const int BrachiosaurusConstitution = 21;
        internal const int BrachiosaurusWisdom = 13;
        internal const int BrachiosaurusBaseAttackBonus = 13;
        internal const int BrachiosaurusSizeAttackModifier = -4;
        internal const int BrachiosaurusSizeManeuverModifier = 4;
        internal const int BrachiosaurusNaturalArmor = 12;
        internal const int BrachiosaurusSpeedFeet = 30;
        internal const int BrachiosaurusSpaceFeet = 20;
        internal const int BrachiosaurusReachFeet = 20;
        internal const int BrachiosaurusTailDiceCount = 4;
        internal const int BrachiosaurusTailDieSize = 6;
        internal const int BrachiosaurusTrampleDiceCount = 2;
        internal const int BrachiosaurusTrampleDieSize = 6;
        internal const int BrachiosaurusPerceptionRanks = 18;
        internal const int BrachiosaurusSkillFocusBonus = 3;
        internal const int BrachiosaurusSkillFocusCompletionBonus = 3;
        internal const int BrachiosaurusQuadrupedAntiTripBonus = 4;
        internal const int BrachiosaurusFeatCount = 9;

        /// <summary>
        /// The printed tail bonus differs between sources: the current AoN
        /// entry reads +22 and the legacy PRD reads +23. The selected contract
        /// is +23, because it is what the inputs both sources print derive -
        /// BAB +13, Strength 37 for +13, Gargantuan -4 and Weapon Focus (tail)
        /// +1 - and those same inputs independently derive the CMB +30 and
        /// CMD 40 that both also print. A +22 would require an unexplained -1
        /// that no printed input supplies. No hidden modifier is manufactured,
        /// Weapon Focus is not removed, and the sources are not claimed to
        /// agree. Owner instruction, 2026-10-15.
        /// </summary>
        internal const int BrachiosaurusPrintedTailAttackLegacy = 23;
        internal const int BrachiosaurusPrintedTailAttackCurrentAon = 22;
        internal const int BrachiosaurusSelectedTailAttack =
            BrachiosaurusPrintedTailAttackLegacy;
        internal const int BrachiosaurusWeaponFocusTailBonus = 1;

        // A reconstructed Gargantuan body exposes fifteen feet before its
        // creature-owned correction, exactly as Sprint 24's Tyrannosaurus did.
        // Native proof must confirm the final twenty-foot uniform reach before
        // publication rather than trusting this constant.
        internal const int BrachiosaurusReachBonusFeet = 5;

        // Elephant, frozen adult entry. Both sources agree exactly and there
        // is no discrepancy to resolve. hp 93 (11d8+44) with Con 19, so the
        // racial base is 93 - 11 * 4.
        internal const int ElephantHitDice = 11;
        internal const int ElephantBaseRacialHitPoints = 49;
        internal const int ElephantStrength = 30;
        internal const int ElephantDexterity = 10;
        internal const int ElephantConstitution = 19;
        internal const int ElephantWisdom = 13;
        internal const int ElephantBaseAttackBonus = 8;
        internal const int ElephantSizeAttackModifier = -2;
        internal const int ElephantSizeManeuverModifier = 2;
        internal const int ElephantNaturalArmor = 9;
        internal const int ElephantSpeedFeet = 40;
        internal const int ElephantSpaceFeet = 15;

        /// <summary>
        /// Space is fifteen feet but printed reach is ten. A Huge body is not
        /// evidence of fifteen-foot reach, and this creature must not be used
        /// to generalise one.
        /// </summary>
        internal const int ElephantReachFeet = 10;
        internal const int ElephantGoreDiceCount = 2;
        internal const int ElephantGoreDieSize = 8;
        internal const int ElephantSlamDiceCount = 2;
        internal const int ElephantSlamDieSize = 6;
        internal const int ElephantTrampleDiceCount = 2;
        internal const int ElephantTrampleDieSize = 8;
        internal const int ElephantPerceptionRanks = 11;
        internal const int ElephantSkillFocusBonus = 3;
        internal const int ElephantSkillFocusCompletionBonus = 3;
        internal const int ElephantQuadrupedAntiTripBonus = 4;
        internal const int ElephantFeatCount = 6;
        internal const int ElephantExistingMonsterRoots = 4;
        internal const int ElephantExistingNatureAllyRoots = 4;
        internal const int ElephantExistingRoots =
            ElephantExistingMonsterRoots + ElephantExistingNatureAllyRoots;

        internal static bool IsGiantBody(string key)
        {
            return key == BrachiosaurusKey || key == ElephantKey;
        }

        /// <summary>
        /// Only the new creature is withheld. Elephant is already released, so
        /// this sprint must never suppress it, and "a giant body" is not the
        /// same predicate as "withheld by this sprint".
        /// </summary>
        internal static bool IsWithheldThisSprint(string key)
        {
            return key == BrachiosaurusKey;
        }

        /// <summary>
        /// Elephant is completed rather than registered, so a Sprint 25 route
        /// census must find no new Elephant placement at all.
        /// </summary>
        internal static bool IsCompletedExistingCreature(string key)
        {
            return key == ElephantKey;
        }

        /// <summary>
        /// Brachiosaurus carries exactly one natural weapon, so a positive
        /// Strength modifier takes the native one-and-a-half multiplier while
        /// zero and penalties stay at face value. Elephant carries two primary
        /// natural attacks and therefore does not use this routine.
        /// </summary>
        internal static bool UsesLoneNaturalWeaponStrength(string key)
        {
            return key == BrachiosaurusKey;
        }

        internal static int LoneNaturalWeaponStrengthContribution(
            int strengthModifier)
        {
            return strengthModifier > 0
                ? checked(strengthModifier + strengthModifier / 2)
                : strengthModifier;
        }

        /// <summary>
        /// Each of Elephant's two primary natural attacks adds its Strength
        /// modifier once. Zero and penalties stay at face value.
        /// </summary>
        internal static int MultipleNaturalPrimaryStrengthContribution(
            int strengthModifier)
        {
            return strengthModifier;
        }

        /// <summary>
        /// Trample damage is the one-and-a-half Strength routine for both
        /// creatures, which is what reproduces the printed 2d6+19 and 2d8+15.
        /// </summary>
        internal static int TrampleStrengthContribution(int strengthModifier)
        {
            return LoneNaturalWeaponStrengthContribution(strengthModifier);
        }

        /// <summary>
        /// Trample's save DC derives live: 10 + half the creature's hit dice,
        /// rounded down, + its Strength modifier. That reproduces the printed
        /// DC 32 for Brachiosaurus and DC 25 for Elephant without hard-coding
        /// either number.
        /// </summary>
        internal static int TrampleSaveDifficulty(int hitDice,
            int strengthModifier)
        {
            if (hitDice <= 0) throw new ArgumentOutOfRangeException("hitDice");
            return checked(10 + hitDice / 2 + strengthModifier);
        }

        internal static int AttackBonus(int baseAttackBonus,
            int strengthModifier, int sizeAttackModifier, int weaponFocusBonus)
        {
            return checked(baseAttackBonus + strengthModifier +
                sizeAttackModifier + weaponFocusBonus);
        }

        internal static int ManeuverBonus(int baseAttackBonus,
            int strengthModifier, int sizeManeuverModifier)
        {
            return checked(baseAttackBonus + strengthModifier +
                sizeManeuverModifier);
        }

        internal static int ManeuverDefense(int baseAttackBonus,
            int strengthModifier, int dexterityModifier,
            int sizeManeuverModifier)
        {
            return checked(10 + baseAttackBonus + strengthModifier +
                dexterityModifier + sizeManeuverModifier);
        }

        internal static int AbilityModifier(int score)
        {
            return (int)Math.Floor((score - 10) / 2.0);
        }

        internal static void Validate()
        {
            int brachStrength = AbilityModifier(BrachiosaurusStrength);
            int brachDexterity = AbilityModifier(BrachiosaurusDexterity);
            int brachConstitution = AbilityModifier(BrachiosaurusConstitution);
            int brachWisdom = AbilityModifier(BrachiosaurusWisdom);
            int elephantStrength = AbilityModifier(ElephantStrength);
            int elephantDexterity = AbilityModifier(ElephantDexterity);
            int elephantConstitution = AbilityModifier(ElephantConstitution);
            int elephantWisdom = AbilityModifier(ElephantWisdom);

            if (brachStrength != 13 || brachDexterity != 0 ||
                brachConstitution != 5 || brachWisdom != 1 ||
                elephantStrength != 10 || elephantDexterity != 0 ||
                elephantConstitution != 4 || elephantWisdom != 1 ||
                // Printed hit points.
                BrachiosaurusBaseRacialHitPoints +
                    BrachiosaurusHitDice * brachConstitution != 171 ||
                ElephantBaseRacialHitPoints +
                    ElephantHitDice * elephantConstitution != 93 ||
                // The selected tail bonus is the one the shared inputs derive,
                // and the rejected reading is not reachable from them.
                AttackBonus(BrachiosaurusBaseAttackBonus, brachStrength,
                        BrachiosaurusSizeAttackModifier,
                        BrachiosaurusWeaponFocusTailBonus) !=
                    BrachiosaurusSelectedTailAttack ||
                BrachiosaurusSelectedTailAttack !=
                    BrachiosaurusPrintedTailAttackLegacy ||
                BrachiosaurusPrintedTailAttackCurrentAon !=
                    BrachiosaurusSelectedTailAttack - 1 ||
                // Elephant's two primaries carry no weapon focus.
                AttackBonus(ElephantBaseAttackBonus, elephantStrength,
                    ElephantSizeAttackModifier, 0) != 16 ||
                // Both printed maneuver values derive from the same inputs,
                // which is why the ability scores and BAB are not in doubt.
                ManeuverBonus(BrachiosaurusBaseAttackBonus, brachStrength,
                    BrachiosaurusSizeManeuverModifier) != 30 ||
                ManeuverDefense(BrachiosaurusBaseAttackBonus, brachStrength,
                    brachDexterity, BrachiosaurusSizeManeuverModifier) != 40 ||
                ManeuverDefense(BrachiosaurusBaseAttackBonus, brachStrength,
                    brachDexterity, BrachiosaurusSizeManeuverModifier) +
                    BrachiosaurusQuadrupedAntiTripBonus != 44 ||
                ManeuverBonus(ElephantBaseAttackBonus, elephantStrength,
                    ElephantSizeManeuverModifier) != 20 ||
                ManeuverDefense(ElephantBaseAttackBonus, elephantStrength,
                    elephantDexterity, ElephantSizeManeuverModifier) != 30 ||
                ManeuverDefense(ElephantBaseAttackBonus, elephantStrength,
                    elephantDexterity, ElephantSizeManeuverModifier) +
                    ElephantQuadrupedAntiTripBonus != 34 ||
                // Armor class.
                10 + BrachiosaurusNaturalArmor + brachDexterity +
                    BrachiosaurusSizeAttackModifier != 18 ||
                10 + ElephantNaturalArmor + elephantDexterity +
                    ElephantSizeAttackModifier != 17 ||
                // The lone-tail routine reproduces the printed 4d6+19 and the
                // trample routines reproduce 2d6+19 and 2d8+15.
                LoneNaturalWeaponStrengthContribution(brachStrength) != 19 ||
                TrampleStrengthContribution(brachStrength) != 19 ||
                TrampleStrengthContribution(elephantStrength) != 15 ||
                // Elephant's primaries add Strength once, not one and a half.
                MultipleNaturalPrimaryStrengthContribution(
                    elephantStrength) != 10 ||
                // Odd and even positive Strength, and zero or penalty, stay
                // exact on the lone-weapon routine.
                LoneNaturalWeaponStrengthContribution(9) != 13 ||
                LoneNaturalWeaponStrengthContribution(10) != 15 ||
                LoneNaturalWeaponStrengthContribution(11) != 16 ||
                LoneNaturalWeaponStrengthContribution(12) != 18 ||
                LoneNaturalWeaponStrengthContribution(0) != 0 ||
                LoneNaturalWeaponStrengthContribution(-1) != -1 ||
                LoneNaturalWeaponStrengthContribution(-5) != -5 ||
                MultipleNaturalPrimaryStrengthContribution(0) != 0 ||
                MultipleNaturalPrimaryStrengthContribution(-3) != -3 ||
                // Printed trample DCs, derived rather than stated.
                TrampleSaveDifficulty(BrachiosaurusHitDice,
                    brachStrength) != 32 ||
                TrampleSaveDifficulty(ElephantHitDice,
                    elephantStrength) != 25 ||
                // Printed Perception, including the Skill Focus completion the
                // Sprint 24 review proved the engine needs stated separately.
                BrachiosaurusPerceptionRanks + 3 + brachWisdom +
                    BrachiosaurusSkillFocusBonus +
                    BrachiosaurusSkillFocusCompletionBonus != 28 ||
                ElephantPerceptionRanks + 3 + elephantWisdom +
                    ElephantSkillFocusBonus +
                    ElephantSkillFocusCompletionBonus != 21 ||
                // A creature gets one feat at each odd hit die.
                BrachiosaurusFeatCount != (BrachiosaurusHitDice + 1) / 2 ||
                ElephantFeatCount != (ElephantHitDice + 1) / 2 ||
                // Space and reach, including the ten-foot reach that must not
                // become a general Huge rule.
                BrachiosaurusSpaceFeet != 20 ||
                BrachiosaurusReachFeet != 20 ||
                ElephantSpaceFeet != 15 ||
                ElephantReachFeet != 10 ||
                // Elephant's reach is strictly shorter than its space. If the
                // two are ever conflated, this fails rather than quietly
                // granting every Huge creature fifteen-foot reach.
                ElephantReachFeet >= ElephantSpaceFeet ||
                // Brachiosaurus is the creature whose reach does equal its
                // space, so the rule above is not a general one either.
                BrachiosaurusReachFeet != BrachiosaurusSpaceFeet ||
                // Only Brachiosaurus uses the lone-weapon routine.
                !UsesLoneNaturalWeaponStrength(BrachiosaurusKey) ||
                UsesLoneNaturalWeaponStrength(ElephantKey) ||
                !IsGiantBody(BrachiosaurusKey) ||
                !IsGiantBody(ElephantKey) ||
                IsGiantBody(TheropodRulesPolicy.TyrannosaurusKey) ||
                // Withheld is the new creature only. The released Elephant is
                // never suppressed, and a giant body is not the same thing as
                // a withheld one.
                !IsWithheldThisSprint(BrachiosaurusKey) ||
                IsWithheldThisSprint(ElephantKey) ||
                IsWithheldThisSprint(TheropodRulesPolicy.TyrannosaurusKey) ||
                !IsCompletedExistingCreature(ElephantKey) ||
                IsCompletedExistingCreature(BrachiosaurusKey) ||
                // The append is one creature, three logical roots and eight
                // private identities, and nothing templated.
                AppendedAbilityIdentityCount +
                    AppendedPrivateIdentityCount +
                    AppendedCreatureCount != AppendedIdentityCount ||
                AppendedMonsterPlacements != 0 ||
                AppendedTemplateExecutionsPerAlignment != 0 ||
                AppendedTemplatedPlacements != 0 ||
                AppendedNatureAllyPlacements != 3 ||
                AppendedCreatureCount != 1 ||
                // The Monster surface does not move this sprint.
                RegisteredMonsterCreatureCount !=
                    TheropodRulesPolicy.RegisteredMonsterCreatureCount ||
                RegisteredMonsterPlacements !=
                    TheropodRulesPolicy.RegisteredMonsterPlacements ||
                RegisteredTemplatedPlacements != 319 ||
                // Totals reconcile against the released Sprint 24 surface.
                RegisteredCreatureCount !=
                    TheropodRulesPolicy.RegisteredCreatureCount +
                    AppendedCreatureCount ||
                RegisteredNatureAllyCreatureCount !=
                    TheropodRulesPolicy.RegisteredNatureAllyCreatureCount +
                    AppendedCreatureCount ||
                RegisteredNatureAllyPlacements !=
                    TheropodRulesPolicy.RegisteredNatureAllyPlacements +
                    AppendedNatureAllyPlacements ||
                RegisteredMonsterPlacements +
                    RegisteredNatureAllyPlacements !=
                    RegisteredLogicalPlacements ||
                RegisteredLogicalPlacements !=
                    TheropodRulesPolicy.FinalPublishedLogicalPlacements +
                    AppendedNatureAllyPlacements ||
                // Brachiosaurus is withheld until its own qualification, and
                // three suppressed roots hold back no execution children.
                AppendedMonsterPlacements + AppendedNatureAllyPlacements !=
                    HiddenLogicalPlacements ||
                HiddenLogicalPlacements != 3 ||
                RegisteredLogicalPlacements - HiddenLogicalPlacements !=
                    HiddenPublishedLogicalPlacements ||
                HiddenPublishedLogicalPlacements !=
                    TheropodRulesPolicy.FinalPublishedLogicalPlacements ||
                RetainedNativeWrappers !=
                    TheropodRulesPolicy.RetainedNativeWrappers ||
                HiddenVisibleChoices != HiddenPublishedLogicalPlacements +
                    RetainedNativeWrappers ||
                HiddenVisibleChoices !=
                    TheropodRulesPolicy.PublicVisibleChoices ||
                PublicVisibleChoices != FinalPublishedLogicalPlacements +
                    RetainedNativeWrappers ||
                PublicSuppressedLogicalPlacements != 0 ||
                RegisteredLogicalPlacements -
                    PublicSuppressedLogicalPlacements !=
                    FinalPublishedLogicalPlacements)
                throw new InvalidOperationException(
                    "Sprint 25 giant-body arithmetic changed.");
        }
    }
}
