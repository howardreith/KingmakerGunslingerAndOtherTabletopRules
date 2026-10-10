using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Xill's printed numbers, derived from its stat block rather than
    /// copied out of it.
    ///
    /// <para>These are the static baseline contract: what an unmodified Xill
    /// must reproduce. The paralysis difficulty class is the clearest case -
    /// the printed line reads DC 16, and the implementation is the derivation
    /// that produces it, so the live creature saves against what its own hit
    /// dice and Constitution support.</para>
    ///
    /// <para>Three printed abilities are deliberately absent, and are named
    /// here so the absence is a recorded decision rather than a silence: the
    /// armed multiweapon routine, implant and planewalk. Nothing stands in for
    /// any of them.</para>
    /// </summary>
    internal static class XillRulesPolicy
    {
        internal const string XillKey = "xill";

        internal const int HitDice = 9;
        internal const int Strength = 17;
        internal const int Dexterity = 18;
        internal const int Constitution = 14;
        internal const int Intelligence = 15;
        internal const int Wisdom = 12;
        internal const int Charisma = 11;
        internal const int SpeedFeet = 40;
        internal const int BaseAttackBonus = 9;

        /// <summary>
        /// Printed racial dice only: 9d10 averages 49. The printed 67 is 49
        /// racial plus 18 from Constitution, and is never written as a total.
        /// </summary>
        internal const int BaseRacialHitPoints = 49;
        internal const int PrintedHitPoints = 67;

        /// <summary>
        /// An outsider's 9 hit dice give +6 on its two good saves and +3 on
        /// its poor one. The generic donor class does not choose a
        /// creature-specific pair, so these are set on racial bases only.
        /// </summary>
        internal const int GoodSave = 6;
        internal const int PoorSave = 3;

        /// <summary>
        /// Natural armour and the shield bonus are separate parts because the
        /// stat block separates them, and because they behave differently: a
        /// shield bonus does not apply against a touch attack and natural
        /// armour does not either, but something that suppresses one does not
        /// suppress the other.
        /// </summary>
        internal const int NaturalArmor = 5;
        internal const int ShieldBonus = 2;

        internal const int PrintedArmorClass = 21;
        internal const int PrintedTouchArmorClass = 14;
        internal const int PrintedFlatFootedArmorClass = 17;
        internal const int PrintedFortitude = 8;
        internal const int PrintedReflex = 10;
        internal const int PrintedWill = 6;
        internal const int PrintedSpellResistance = 17;
        internal const int PrintedCombatManeuverBonus = 12;
        internal const int PrintedCombatManeuverDefense = 26;
        internal const int PrintedGrappleBonus = 16;

        /// <summary>
        /// The natural routine: four primary claws and a primary bite. The
        /// claws carry Weapon Focus and the bite does not, which is the whole
        /// reason the printed bonuses differ by one.
        /// </summary>
        internal const int ClawCount = 4;
        internal const int PrintedClawAttackBonus = 13;
        internal const int PrintedBiteAttackBonus = 12;
        internal const int WeaponFocusBonus = 1;
        internal const int ClawDieSides = 4;
        internal const int BiteDieSides = 3;

        /// <summary>
        /// Grab adds four to a grapple check, which is the whole difference
        /// between the printed combat manoeuvre bonus and its grapple figure.
        /// </summary>
        internal const int GrabManeuverBonus = 4;

        internal const int PrintedParalysisDifficultyClass = 16;

        // Skill ranks, derived from the printed totals rather than guessed.
        // An outsider gets 6 + Intelligence modifier ranks per hit die, which
        // at Intelligence 15 is eight a die and seventy-two in total; the
        // printed totals account for seventy of them, every printed skill
        // being a class skill at +3.
        //
        // Eight printed skills map onto five Kingmaker skills, because
        // Kingmaker merges three pairs. Where two printed skills land on one
        // Kingmaker skill the ranks are NOT added: taking the larger and
        // recording the merge keeps the creature at its printed competence,
        // whereas summing would make it better than its own stat block.
        internal const int MobilityRanks = 9;          // Acrobatics +16
        internal const int StealthRanks = 7;           // Stealth +14
        internal const int PerceptionRanks = 9;        // Perception +13
        internal const int PersuasionRanks = 9;        // Bluff +12
        internal const int KnowledgeArcanaRanks = 9;   // Knowledge (arcana) +14

        internal const int PrintedAcrobaticsSkill = 16;
        internal const int PrintedStealthSkill = 14;
        internal const int PrintedPerceptionSkill = 13;
        internal const int PrintedBluffSkill = 12;
        internal const int PrintedKnowledgeArcanaSkill = 14;

        // The three printed skills that have no Kingmaker skill of their own.
        // Each one lands on a Kingmaker skill another printed skill already
        // holds, so each is recorded as merged rather than represented.
        internal const int PrintedIntimidateSkill = 12;     // -> Persuasion
        internal const int PrintedSenseMotiveSkill = 13;    // -> Perception
        internal const int PrintedKnowledgePlanesSkill = 14; // -> Knowledge (arcana)

        /// <summary>
        /// Ranks the printed stat block spends that this project can place on
        /// a Kingmaker skill of their own. The remainder is merged, not lost
        /// and not silently added on top of a skill that already has ranks.
        /// </summary>
        internal const int PlacedSkillRanks = MobilityRanks + StealthRanks +
            PerceptionRanks + PersuasionRanks + KnowledgeArcanaRanks;
        internal const int PrintedSkillRanks = 70;

        internal static int StrengthModifier { get { return (Strength - 10) / 2; } }
        internal static int DexterityModifier { get { return (Dexterity - 10) / 2; } }
        internal static int ConstitutionModifier
        { get { return (Constitution - 10) / 2; } }

        /// <summary>
        /// The printed paralysis save: ten plus half the hit dice plus the
        /// Constitution modifier, which is DC 16 for an unmodified Xill and
        /// follows the live creature rather than being pinned to 16.
        /// </summary>
        internal static int ParalysisDifficultyClass(int constitutionBonus)
        {
            return ParalysisDifficultyClass(HitDice, constitutionBonus);
        }

        internal static int ParalysisDifficultyClass(int hitDice,
            int constitutionBonus)
        {
            if (hitDice < 1)
                throw new ArgumentOutOfRangeException("hitDice");
            return 10 + hitDice / 2 + constitutionBonus;
        }

        internal static void Validate()
        {
            // 49 racial plus 18 from Constitution.
            if (BaseRacialHitPoints + HitDice * ConstitutionModifier !=
                    PrintedHitPoints)
                throw new InvalidOperationException(
                    "Xill printed hit point derivation changed.");
            if (GoodSave + ConstitutionModifier != PrintedFortitude ||
                GoodSave + DexterityModifier != PrintedReflex ||
                PoorSave + (Wisdom - 10) / 2 + 2 != PrintedWill)
                throw new InvalidOperationException(
                    "Xill printed save derivation changed.");
            // Medium, so no size term: armour class takes natural armour,
            // Dexterity and the shield bonus; touch drops both armour terms;
            // flat-footed drops Dexterity.
            if (10 + NaturalArmor + DexterityModifier + ShieldBonus !=
                    PrintedArmorClass ||
                10 + DexterityModifier != PrintedTouchArmorClass ||
                10 + NaturalArmor + ShieldBonus !=
                    PrintedFlatFootedArmorClass)
                throw new InvalidOperationException(
                    "Xill printed armor class derivation changed.");
            // The shield bonus is the whole reason armour class and
            // flat-footed exceed what natural armour alone would give. If it
            // were dropped the creature would read 19 and 15 against a stat
            // block that says 21 and 17.
            if (PrintedArmorClass - ShieldBonus != 19 ||
                PrintedFlatFootedArmorClass - ShieldBonus != 15)
                throw new InvalidOperationException(
                    "Xill shield bonus accounting changed.");
            // Claws carry Weapon Focus; the bite does not.
            if (BaseAttackBonus + StrengthModifier + WeaponFocusBonus !=
                    PrintedClawAttackBonus ||
                BaseAttackBonus + StrengthModifier != PrintedBiteAttackBonus ||
                PrintedClawAttackBonus - PrintedBiteAttackBonus !=
                    WeaponFocusBonus)
                throw new InvalidOperationException(
                    "Xill printed attack bonus derivation changed.");
            if (BaseAttackBonus + StrengthModifier !=
                    PrintedCombatManeuverBonus ||
                10 + BaseAttackBonus + StrengthModifier + DexterityModifier !=
                    PrintedCombatManeuverDefense ||
                PrintedCombatManeuverBonus + GrabManeuverBonus !=
                    PrintedGrappleBonus)
                throw new InvalidOperationException(
                    "Xill printed combat maneuver derivation changed.");
            if (ParalysisDifficultyClass(ConstitutionModifier) !=
                    PrintedParalysisDifficultyClass)
                throw new InvalidOperationException(
                    "Xill printed paralysis difficulty class derivation changed.");
            if (ClawCount != 4)
                throw new InvalidOperationException(
                    "The Xill prints four claws in its natural routine.");
            // Every limb adds the plain Strength modifier. The printed armed
            // routine bites for 1d3+1 because the bite is secondary there; the
            // natural routine bites for 1d3+3 because it is not, and the
            // natural routine is the one implemented.
            if (PrintedBiteAttackBonus - BaseAttackBonus != StrengthModifier)
                throw new InvalidOperationException(
                    "Xill limbs must add the ordinary Strength modifier.");
            // Every printed skill is a class skill at +3 for an outsider, so
            // each total is ranks plus three plus its attribute modifier.
            if (MobilityRanks + 3 + DexterityModifier != PrintedAcrobaticsSkill ||
                StealthRanks + 3 + DexterityModifier != PrintedStealthSkill ||
                PerceptionRanks + 3 + (Wisdom - 10) / 2 != PrintedPerceptionSkill ||
                PersuasionRanks + 3 + (Charisma - 10) / 2 != PrintedBluffSkill ||
                KnowledgeArcanaRanks + 3 + (Intelligence - 10) / 2 !=
                    PrintedKnowledgeArcanaSkill)
                throw new InvalidOperationException(
                    "Xill printed skill derivation changed.");
            // The three merged skills derive from the same parts, which is
            // what makes the merge honest: Sense Motive and Perception are the
            // same number, Intimidate and Bluff are the same number, and the
            // two Knowledges are the same number. Merging them costs the
            // creature nothing it would otherwise show, and adding them would
            // give it competence its stat block does not print.
            if (PerceptionRanks + 3 + (Wisdom - 10) / 2 !=
                    PrintedSenseMotiveSkill ||
                PersuasionRanks + 3 + (Charisma - 10) / 2 !=
                    PrintedIntimidateSkill ||
                KnowledgeArcanaRanks + 3 + (Intelligence - 10) / 2 !=
                    PrintedKnowledgePlanesSkill)
                throw new InvalidOperationException(
                    "Xill merged skill accounting changed; a merge is only "
                    + "honest while both printed skills derive the same total.");
            if (PlacedSkillRanks != 43 || PrintedSkillRanks != 70 ||
                PlacedSkillRanks + MobilityRanks + PerceptionRanks +
                    PersuasionRanks != PrintedSkillRanks)
                throw new InvalidOperationException(
                    "Xill skill rank accounting changed: forty-three ranks are "
                    + "placed and twenty-seven are merged into skills that "
                    + "already hold a printed skill.");
        }
    }
}
