using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Giant Scorpion's printed numbers, derived from its stat block
    /// rather than copied out of it.
    ///
    /// <para>This creature is unusually well behaved arithmetically: it has no
    /// Intelligence score, so it has no skill ranks and no feats, and every
    /// number in its block falls out of hit dice, ability scores, size and
    /// racial bonuses alone. That makes <see cref="Validate"/> a complete
    /// account of the stat block rather than a sample of it, and it is why
    /// the one number that did not close at intake - Stealth - is recorded in
    /// the frozen contract as a question against the book instead of being
    /// quietly rounded to fit.</para>
    ///
    /// <para>Nothing here is a framework. It is one creature's arithmetic,
    /// reachable by one creature. What it shares with earlier sprints it
    /// reuses rather than reimplements: the eight-legged trip defence the
    /// Giant Centipede and both Giant Ant castes already carry, the grapple
    /// lifecycle Sprint 6 built and Sprint 19 taught to hold several limbs,
    /// and the full-Strength limb carrier Sprint 18 shipped.</para>
    /// </summary>
    internal static class GiantScorpionRulesPolicy
    {
        internal const string GiantScorpionKey = "giant-scorpion";

        // The printed stat block. Transcribed at intake and then checked
        // against itself: eleven independent identities close on these
        // numbers, which is what pins Dexterity at 12 three separate ways.
        internal const int HitDice = 5;
        internal const int Strength = 19;
        internal const int Dexterity = 12;
        internal const int Constitution = 16;
        internal const int Wisdom = 10;
        internal const int Charisma = 2;
        internal const int SpeedFeet = 50;
        internal const int NaturalArmor = 6;
        internal const int BaseAttackBonus = 3;

        /// <summary>
        /// The printed Intelligence is an em dash: this creature is mindless.
        /// Kingmaker cannot represent an absent Intelligence score, so 1 is
        /// used, exactly as the Giant Spider, both Giant Ant castes and the
        /// Giant Stag Beetle already ship. The printed immunity to
        /// mind-affecting effects is carried as its own fact and is never
        /// inferred from this substituted score.
        /// </summary>
        internal const int SubstitutedIntelligence = 1;

        /// <summary>
        /// Printed racial dice only: 5d8 at the monster convention averages
        /// 22. Native Constitution stays live, so the printed 37 is 22 racial
        /// plus 15 from Constitution and is never written down as a total.
        /// </summary>
        internal const int BaseRacialHitPoints = 22;
        internal const int PrintedHitPoints = 37;

        /// <summary>
        /// Vermin take the animal-style good Fortitude with two poor saves.
        /// Five hit dice give +4 on the good one and +1 on each poor one.
        /// </summary>
        internal const int GoodSave = 4;
        internal const int PoorSave = 1;

        internal const int PrintedArmorClass = 16;
        internal const int PrintedTouchArmorClass = 10;
        internal const int PrintedFlatFootedArmorClass = 15;
        internal const int PrintedFortitude = 7;
        internal const int PrintedReflex = 2;
        internal const int PrintedWill = 1;
        internal const int PrintedCombatManeuverBonus = 8;
        internal const int PrintedGrappleBonus = 12;
        internal const int PrintedCombatManeuverDefense = 19;
        internal const int PrintedCombatManeuverDefenseVersusTrip = 31;

        /// <summary>
        /// Two primary claws and one primary sting, all three at one attack
        /// bonus and all three at the whole Strength modifier. Three attacks,
        /// not a framework for three: three entries in this creature's own
        /// limb list.
        /// </summary>
        internal const int ClawCount = 2;
        internal const int StingCount = 1;
        internal const int PrintedAttackBonus = 6;
        internal const int ClawDieSides = 6;
        internal const int StingDieSides = 6;

        /// <summary>
        /// The eight-legged stability bonus, which the project's existing
        /// TripDefenseEightLegs carrier produces. A scorpion has eight legs
        /// and the printed difference between its ordinary and anti-trip
        /// manoeuvre defence is exactly this.
        /// </summary>
        internal const int EightLegTripBonus = 12;

        /// <summary>
        /// Grab adds this to a grapple attempt, which is the whole of the
        /// printed difference between +8 and +12.
        /// </summary>
        internal const int GrabGrappleBonus = 4;

        // The printed poison graph. The difficulty class is NOT stored: it is
        // computed live from Constitution by the shared formula below, so a
        // buffed or weakened scorpion poisons for what its live Constitution
        // supports. Six rounds is what makes this graph new - every poison
        // carrier this project ships runs four.
        internal const int PoisonDiceCount = 1;
        internal const int PoisonDieSides = 2;
        internal const string PoisonDamagedStat = "Strength";
        internal const int PoisonRounds = 6;
        internal const int PoisonCureSaves = 1;
        internal const int PrintedPoisonDifficultyClass = 15;

        /// <summary>
        /// No ranks at all. A creature with no Intelligence score buys none,
        /// so all three printed skill totals are ability plus racial bonus.
        /// The builder must be told this explicitly or it grants its default
        /// Perception, Mobility and Stealth ranks and the creature reads
        /// better than its stat block.
        /// </summary>
        internal const int SkillRanks = 0;
        internal const int RacialSkillBonus = 4;
        internal const int PrintedPerceptionSkill = 4;
        internal const int PrintedStealthSkill = 1;

        /// <summary>
        /// The printed Climb total this project does not represent, because
        /// Kingmaker has no Climb skill. Omitting it forfeits no rank - this
        /// creature has none to spend - and neither Athletics nor Mobility is
        /// raised to stand in for it. Kept as a recorded number rather than a
        /// silence.
        /// </summary>
        internal const int PrintedClimbSkill = 8;

        /// <summary>
        /// Printed Space 10 feet, Reach 10 feet, with a parenthetical 5 feet
        /// for the claws. The creature keeps the 10, which is also the
        /// engine's own reach for a Large creature, and the per-limb exception
        /// is unrepresented: the claws threaten at 10 feet rather than 5. The
        /// project's reduced-reach carrier is deliberately not applied here -
        /// the Giant Stag Beetle uses it because that creature's whole printed
        /// Reach line is 5 feet, where this one's is 10.
        /// </summary>
        internal const int PrintedSpaceFeet = 10;
        internal const int PrintedReachFeet = 10;
        internal const int PrintedClawReachFeet = 5;

        internal static int StrengthModifier { get { return (Strength - 10) / 2; } }
        internal static int ConstitutionModifier { get { return (Constitution - 10) / 2; } }
        internal static int DexterityModifier { get { return (Dexterity - 10) / 2; } }

        /// <summary>
        /// 10 plus half the hit dice plus the Constitution modifier, which is
        /// the tabletop formula and the one the project's poison carriers
        /// already compute live. At the printed Constitution 16 and 5 hit dice
        /// this is DC 15.
        /// </summary>
        internal static int PoisonDifficultyClass(int hitDice, int constitutionBonus)
        { return 10 + hitDice / 2 + constitutionBonus; }

        internal static string PoisonDamage
        { get { return PoisonDiceCount + "d" + PoisonDieSides; } }

        internal static void Validate()
        {
            if (StrengthModifier != 4 || ConstitutionModifier != 3 ||
                DexterityModifier != 1)
                throw new InvalidOperationException(
                    "Giant Scorpion printed ability modifiers changed.");
            // 22 racial plus 15 from Constitution. No Toughness: vermin have
            // no feats, so there is no third term to go missing.
            if (BaseRacialHitPoints + HitDice * ConstitutionModifier !=
                    PrintedHitPoints)
                throw new InvalidOperationException(
                    "Giant Scorpion printed hit point derivation changed.");
            // Armour class, touch and flat-footed from one set of parts, with
            // the Large size penalty applied once to each. These three are the
            // first two of the three places Dexterity 12 is pinned.
            if (10 + NaturalArmor + DexterityModifier - 1 != PrintedArmorClass ||
                10 + DexterityModifier - 1 != PrintedTouchArmorClass ||
                10 + NaturalArmor - 1 != PrintedFlatFootedArmorClass)
                throw new InvalidOperationException(
                    "Giant Scorpion printed armor class derivation changed.");
            // One good save and two poor ones. Reflex is the third place
            // Dexterity 12 is pinned.
            if (GoodSave + ConstitutionModifier != PrintedFortitude ||
                PoorSave + DexterityModifier != PrintedReflex ||
                PoorSave + (Wisdom - 10) / 2 != PrintedWill)
                throw new InvalidOperationException(
                    "Giant Scorpion printed save derivation changed.");
            // Vermin use the 3/4 base attack progression.
            if (HitDice * 3 / 4 != BaseAttackBonus)
                throw new InvalidOperationException(
                    "Giant Scorpion printed base attack derivation changed.");
            // Every limb adds the plain Strength modifier, not one and a half
            // times it. The engine grants the larger figure to a natural
            // primary-hand weapon whenever the secondary hand is empty, which
            // is why this creature carries the Sprint 18 full-Strength
            // correction: without it one limb would read 1d6+6 against a
            // printed 1d6+4.
            if (BaseAttackBonus + StrengthModifier - 1 != PrintedAttackBonus)
                throw new InvalidOperationException(
                    "Giant Scorpion printed attack bonus derivation changed: "
                    + "base attack plus Strength less one for Large size.");
            if (ClawCount + StingCount != 3)
                throw new InvalidOperationException(
                    "The Giant Scorpion prints two claws and one sting.");
            if (BaseAttackBonus + StrengthModifier + 1 !=
                    PrintedCombatManeuverBonus ||
                PrintedCombatManeuverBonus + GrabGrappleBonus !=
                    PrintedGrappleBonus ||
                10 + BaseAttackBonus + StrengthModifier + DexterityModifier + 1
                    != PrintedCombatManeuverDefense ||
                PrintedCombatManeuverDefense + EightLegTripBonus !=
                    PrintedCombatManeuverDefenseVersusTrip)
                throw new InvalidOperationException(
                    "Giant Scorpion printed combat maneuver derivation changed.");
            // No Intelligence score, so no ranks, so every skill total is
            // ability plus the racial bonus and nothing else.
            if (SkillRanks != 0)
                throw new InvalidOperationException(
                    "A creature with no Intelligence score buys no skill ranks.");
            if ((Wisdom - 10) / 2 + RacialSkillBonus != PrintedPerceptionSkill ||
                DexterityModifier + RacialSkillBonus - 4 != PrintedStealthSkill ||
                StrengthModifier + RacialSkillBonus != PrintedClimbSkill)
                throw new InvalidOperationException(
                    "Giant Scorpion printed skill derivation changed.");
            // The poison difficulty class is derived, never stored. If this
            // fails the formula moved, not the stat block.
            if (PoisonDifficultyClass(HitDice, ConstitutionModifier) !=
                    PrintedPoisonDifficultyClass)
                throw new InvalidOperationException(
                    "Giant Scorpion printed poison difficulty class derivation "
                    + "changed.");
            if (PoisonDamage != "1d2" || PoisonRounds != 6 ||
                PoisonCureSaves != 1)
                throw new InvalidOperationException(
                    "Giant Scorpion printed poison graph changed.");
            // Six rounds is the whole reason this creature needs its own
            // poison carrier rather than sharing one, so the distinction is
            // asserted rather than left as a comment.
            if (PoisonRounds == 4)
                throw new InvalidOperationException(
                    "A six-round poison cannot share the project's four-round "
                    + "carriers.");
            // The creature's reach is the printed 10 feet and the claws' 5 is
            // the part the engine cannot express. If these ever become equal
            // the omission has stopped being an omission.
            if (PrintedReachFeet != PrintedSpaceFeet ||
                PrintedClawReachFeet >= PrintedReachFeet)
                throw new InvalidOperationException(
                    "Giant Scorpion printed reach derivation changed.");
        }
    }
}
