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
        // Existing released identity; no new Elephant blueprint is allocated.
        internal const string ElephantUnitGuid = "9dd8544097234f05bcd35d400d91b510";

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

        /// <summary>
        /// The bounded ordinary Overrun command, selected by the owner over a
        /// waiver. It is a DIFFERENT action from Trample and shares none of
        /// its machinery: an overrun raises a real combat maneuver, where a
        /// trample is automatic and raises none.
        /// </summary>
        internal const string BrachiosaurusOverrunSymbol =
            "KMG.Summoning.Special.Brachiosaurus.Overrun";

        /// <summary>The cloned Greater Overrun carrier, this creature
        /// only.</summary>
        internal const string BrachiosaurusGreaterOverrunSymbol =
            "KMG.Summoning.Special.Brachiosaurus.GreaterOverrun";
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
        // Elephant's trample and high-rank Skill Focus completion were each
        // appended only after native evidence proved them missing. Its unit
        // type remains a reserved name; no released unit/weapon/root moves.
        internal const int AppendedAdapterIdentityCount = 8;

        /// <summary>
        /// Thirteen Sprint 25 catalog identities plus the eight caller adapters the
        /// alternate summoning release projects for any creature added to
        /// the central catalog. Those eight are not chosen here: each GUID
        /// is uuid5 of its own symbol, so the manifest reproduces and
        /// verifies them rather than trusting an allocation.
        /// </summary>
        internal const int AppendedIdentityCount = 21;

        /// <summary>
        /// What Sprint 25 adds to the C# identity catalog. The eight caller
        /// adapters are deliberately excluded: they are a projection the
        /// alternate summoning release derives from the roster and allocates
        /// in the blueprint ledger, and they never become catalog rows.
        /// </summary>
        internal const int AppendedCatalogIdentityCount =
            AppendedIdentityCount - AppendedAdapterIdentityCount;
        internal const int AppendedCreatureCount = 1;
        internal const int AppendedAbilityIdentityCount = 3;
        internal const int AppendedWeaponIdentityCount = 1;
        internal const int AppendedWeaponTypeIdentityCount = 1;
        internal const int AppendedUnitTypeIdentityCount = 1;
        internal const int AppendedFeatureIdentityCount = 2;
        internal const int AppendedTrampleAbilityIdentityCount = 2;

        /// <summary>
        /// One Brachiosaurus-owned ordinary Overrun command. It is counted
        /// apart from the tramples because it is a different action: it raises
        /// a real combat maneuver, where a trample never does.
        /// </summary>
        internal const int AppendedOverrunAbilityIdentityCount = 1;

        /// <summary>
        /// One cloned Greater Overrun carrier, owned by this creature.
        /// The native feat's provocation fires on success alone; the
        /// printed rider follows a knockdown the overrun caused, so the
        /// clone drops that component and carries the corrected one. It
        /// is a BlueprintFeature, so it is a private carrier and not an
        /// ability.
        /// </summary>
        internal const int AppendedGreaterOverrunIdentityCount = 1;
        internal const int AppendedPrivateIdentityCount =
            AppendedWeaponIdentityCount + AppendedWeaponTypeIdentityCount +
            AppendedUnitTypeIdentityCount + AppendedFeatureIdentityCount +
            AppendedTrampleAbilityIdentityCount +
            AppendedOverrunAbilityIdentityCount +
            AppendedGreaterOverrunIdentityCount;

        /// <summary>
        /// Every appended identity in the C# catalog whose planned type is
        /// BlueprintAbility: the three logical roots, the two tramples and
        /// the ordinary Overrun command. All three of the latter are counted
        /// as private carriers but are still abilities. The eight caller
        /// adapters are also abilities but never enter the C# catalog, so
        /// they are not here.
        /// </summary>
        internal const int AppendedTotalAbilityIdentityCount =
            AppendedAbilityIdentityCount + AppendedTrampleAbilityIdentityCount +
            AppendedOverrunAbilityIdentityCount;

        /// <summary>
        /// Elephant's unneeded species type remains reserved. Its own trample
        /// and native Skill Focus completion have separately demonstrated
        /// missing behavior and now own append-only private identities.
        /// </summary>
        // The live e8b00499 profile proved that eleven ranks plus the native
        // Skill Focus still gave only +3. Its private high-rank completion is
        // now justified; only the unnecessary species type remains reserved.
        internal const int ReservedElephantIdentityCount = 1;

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
        internal const int BrachiosaurusPowerAttackAdditionalDamage =
            1 + BrachiosaurusBaseAttackBonus / 4;

        // A reconstructed Gargantuan body exposes fifteen feet before its
        // creature-owned correction, exactly as Sprint 24's Tyrannosaurus did.
        // Native proof must confirm the final twenty-foot uniform reach before
        // publication rather than trusting this constant.
        internal const int BrachiosaurusReachBonusFeet = 5;

        // BlueprintWeaponType subtracts four feet from melee values at or
        // below ten, so nine stores as five - the same arithmetic the
        // released Stegosaurus tail uses, and the reason that value is
        // nine here rather than twenty. The weapon type carries the five
        // feet a tail contributes before body reach; the Gargantuan body
        // contributes fifteen and BrachiosaurusReachBonusFeet the final
        // five. Native proof must confirm the twenty-foot uniform reach
        // this composes before publication rather than trusting it here.
        internal const int BrachiosaurusTailTypeStoredRangeFeet = 9;

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
        /// Brachiosaurus allocates its printed Perception-only ranks
        /// exactly. Elephant keeps its observed eleven Perception ranks but
        /// also needs the creation-only racial HP path: the native class
        /// average produced 94 rather than its printed 93. Other skills and
        /// released identities remain unchanged.
        /// </summary>
        internal static bool UsesExactRanks(string key)
        {
            return IsGiantBody(key);
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
            // One implementation, on the shared carrier the trample
            // damage path reads, so this record helper and the runtime
            // cannot drift apart again.
            return UngulateRulesPolicy.OneAndAHalfStrength(
                strengthModifier);
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
                // The append is one creature, three logical roots, six
                // private identities and eight caller adapters, and nothing
                // templated.
                // The adapters are part of the append too: the alternate
                // summoning release projects one caller identity for every
                // creature its family and tiers admit, so a new creature
                // brings its own with it.
                AppendedAbilityIdentityCount +
                    AppendedPrivateIdentityCount +
                    AppendedAdapterIdentityCount +
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
