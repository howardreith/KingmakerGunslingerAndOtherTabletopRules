using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Giant Crab's printed numbers, derived from its stat block rather
    /// than copied out of it.
    ///
    /// <para>Like the Sprint 20 Giant Scorpion this creature is arithmetically
    /// complete: it has no Intelligence score, so it has no skill ranks and no
    /// feats, and every number in its block falls out of hit dice, ability
    /// scores, size and racial bonuses alone. Sixteen independent identities
    /// close on the transcription, which is what makes <see cref="Validate"/>
    /// a full account of the stat block rather than a sample of it - and,
    /// unlike Sprint 20's, every one of them closed at intake. Nothing here is
    /// recorded as a question against the book.</para>
    ///
    /// <para>What it reuses it reuses rather than reimplements: the grapple
    /// lifecycle Sprint 6 built and Sprint 19 taught to hold several limbs,
    /// the full-Strength limb carrier Sprint 18 shipped, and the shared native
    /// Medium 1d4 claw. What it owns it owns because the alternative would be
    /// wrong rather than merely inconvenient: its anti-trip defence is +12
    /// where the shared native eight-leg fact delivers +8, and the Sprint 20
    /// carrier that does deliver +12 is named and described for a scorpion.
    /// </para>
    /// </summary>
    internal static class GiantCrabRulesPolicy
    {
        internal const string GiantCrabKey = "giant-crab";

        // The printed stat block, transcribed at intake.
        internal const int HitDice = 3;
        internal const int Strength = 15;
        internal const int Dexterity = 13;
        internal const int Constitution = 14;
        internal const int Wisdom = 10;
        internal const int Charisma = 2;
        internal const int SpeedFeet = 30;
        internal const int NaturalArmor = 5;
        internal const int BaseAttackBonus = 2;

        /// <summary>
        /// The printed Intelligence is an em dash: this creature is mindless.
        /// Kingmaker cannot represent an absent Intelligence score, so 1 is
        /// used, exactly as every mindless vermin this project already ships.
        /// The printed immunity to mind-affecting effects is carried as its own
        /// fact and is never inferred from this substituted score, because 1 is
        /// not mindless as far as the engine is concerned.
        /// </summary>
        internal const int SubstitutedIntelligence = 1;

        /// <summary>
        /// Printed racial dice only: 3d8 at the monster convention averages 13.
        /// Native Constitution stays live, so the printed 19 is 13 racial plus
        /// 6 from Constitution and is never written down as a total.
        /// </summary>
        internal const int BaseRacialHitPoints = 13;
        internal const int PrintedHitPoints = 19;

        /// <summary>
        /// Vermin take the animal-style good Fortitude with two poor saves.
        /// Three hit dice give +3 on the good one and +1 on each poor one.
        /// </summary>
        internal const int GoodSave = 3;
        internal const int PoorSave = 1;

        internal const int PrintedArmorClass = 16;
        internal const int PrintedTouchArmorClass = 11;
        internal const int PrintedFlatFootedArmorClass = 15;
        internal const int PrintedFortitude = 5;
        internal const int PrintedReflex = 2;
        internal const int PrintedWill = 1;
        internal const int PrintedCombatManeuverBonus = 4;
        internal const int PrintedGrappleBonus = 8;
        internal const int PrintedCombatManeuverDefense = 15;
        internal const int PrintedCombatManeuverDefenseVersusTrip = 27;

        /// <summary>
        /// Two primary claws and nothing else. The whole printed routine, and
        /// the shortest one this programme has shipped for a creature with a
        /// rider: both claws grab.
        /// </summary>
        internal const int ClawCount = 2;
        internal const int PrintedAttackBonus = 4;
        internal const int ClawDieSides = 4;

        /// <summary>
        /// Constrict, which the printed block gives at the claw's own damage.
        ///
        /// <para>1d4 plus the whole Strength modifier, computed from live
        /// Strength rather than stored, and applied only to the exact target
        /// held by the maintaining claw. Each claw holds its own foe under the
        /// multi-limb lifecycle Sprint 19 built, so "the target it is holding"
        /// is a per-limb question here rather than a per-creature one - which
        /// is the whole reason this creature is worth reviewing live.</para>
        /// </summary>
        internal const int ConstrictDiceCount = 1;
        internal const int ConstrictDieSides = 4;

        /// <summary>
        /// The printed stability bonus against trip: the difference between
        /// CMD 15 and CMD 27.
        ///
        /// <para>A crab walks on eight legs, and the stat-block convention is
        /// four per pair of legs beyond the first - three extra pairs, +12. The
        /// Sprint 20 Giant Scorpion prints the same figure for the same reason,
        /// which is why this creature's carrier matches that one's value and
        /// not the shared native eight-leg fact's
        /// <see cref="SharedNativeTripBonus"/>. It carries its own rather than
        /// the scorpion's because that one is named and described for a
        /// scorpion, and a crab wearing it would read as the wrong creature's
        /// feature.</para>
        /// </summary>
        internal const int EightLegTripBonus = 12;

        /// <summary>
        /// What the shared native carrier delivers, measured on a live creature
        /// in Sprint 20 rather than inferred from its name. Recorded so the
        /// reason this creature owns a carrier is a number rather than a
        /// comment.
        /// </summary>
        internal const int SharedNativeTripBonus = 8;

        /// <summary>
        /// Grab adds this to a grapple attempt, which is the whole of the
        /// printed difference between +4 and +8.
        /// </summary>
        internal const int GrabGrappleBonus = 4;

        /// <summary>
        /// No ranks at all. A creature with no Intelligence score buys none, so
        /// both printed skill totals are ability plus racial bonus. The builder
        /// must be told this explicitly or it grants its default Perception,
        /// Mobility and Stealth ranks and the creature reads better than its
        /// stat block.
        /// </summary>
        internal const int SkillRanks = 0;
        internal const int RacialPerceptionBonus = 4;
        internal const int PrintedPerceptionSkill = 4;

        /// <summary>
        /// The printed swim speed, recorded and unrepresented. The creature
        /// ships at its printed 30-foot ground speed; this number is not folded
        /// into that one, no skill is raised to stand in for Swim, and no
        /// aquatic movement subsystem is built - which is one of this mission's
        /// hard boundaries as well as the honest answer.
        ///
        /// <para>Water dependency goes the same way. It has no practical
        /// trigger inside an ordinary summon duration and no drowning consumer
        /// inside ordinary-map scope, so it is recorded here rather than
        /// implemented.</para>
        /// </summary>
        internal const int PrintedSwimSpeedFeet = 20;

        internal const int PrintedSpaceFeet = 5;
        internal const int PrintedReachFeet = 5;

        internal static int StrengthModifier { get { return (Strength - 10) / 2; } }
        internal static int ConstitutionModifier { get { return (Constitution - 10) / 2; } }
        internal static int DexterityModifier { get { return (Dexterity - 10) / 2; } }
        internal static int WisdomModifier { get { return (Wisdom - 10) / 2; } }

        internal static void Validate()
        {
            if (StrengthModifier != 2 || ConstitutionModifier != 2 ||
                DexterityModifier != 1 || WisdomModifier != 0)
                throw new InvalidOperationException(
                    "Giant Crab printed ability modifiers changed.");
            // 13 racial plus 6 from Constitution. No Toughness: vermin have no
            // feats, so there is no third term to go missing.
            if (BaseRacialHitPoints + HitDice * ConstitutionModifier !=
                    PrintedHitPoints)
                throw new InvalidOperationException(
                    "Giant Crab printed hit point derivation changed.");
            // Armour class, touch and flat-footed from one set of parts, with
            // no size term because the creature is Medium. These are two of the
            // three places Dexterity 12 is pinned.
            if (10 + NaturalArmor + DexterityModifier != PrintedArmorClass ||
                10 + DexterityModifier != PrintedTouchArmorClass ||
                10 + NaturalArmor != PrintedFlatFootedArmorClass)
                throw new InvalidOperationException(
                    "Giant Crab printed armor class derivation changed.");
            // One good save and two poor ones. Reflex is the third place
            // Dexterity 12 is pinned.
            if (GoodSave + ConstitutionModifier != PrintedFortitude ||
                PoorSave + DexterityModifier != PrintedReflex ||
                PoorSave + WisdomModifier != PrintedWill)
                throw new InvalidOperationException(
                    "Giant Crab printed save derivation changed.");
            // Vermin use the 3/4 base attack progression.
            if (HitDice * 3 / 4 != BaseAttackBonus)
                throw new InvalidOperationException(
                    "Giant Crab printed base attack derivation changed.");
            // Both claws add the plain Strength modifier, not one and a half
            // times it. The engine grants the larger figure to a natural
            // primary-hand weapon whenever the secondary hand is empty, which
            // is why this creature carries the released Sprint 18
            // full-Strength correction: without it one claw would read 1d4+3
            // against a printed 1d4+2.
            if (BaseAttackBonus + StrengthModifier != PrintedAttackBonus)
                throw new InvalidOperationException(
                    "Giant Crab printed attack bonus derivation changed: base "
                    + "attack plus Strength, with no size term.");
            if (ClawCount != 2)
                throw new InvalidOperationException(
                    "The Giant Crab prints two claws and nothing else.");
            if (BaseAttackBonus + StrengthModifier !=
                    PrintedCombatManeuverBonus ||
                PrintedCombatManeuverBonus + GrabGrappleBonus !=
                    PrintedGrappleBonus ||
                10 + BaseAttackBonus + StrengthModifier + DexterityModifier
                    != PrintedCombatManeuverDefense ||
                PrintedCombatManeuverDefense + EightLegTripBonus !=
                    PrintedCombatManeuverDefenseVersusTrip)
                throw new InvalidOperationException(
                    "Giant Crab printed combat maneuver derivation changed.");
            // The whole reason this creature owns an anti-trip carrier. If
            // these ever become equal the shared native fact would do, and this
            // creature should take it rather than keep its own.
            if (EightLegTripBonus == SharedNativeTripBonus)
                throw new InvalidOperationException(
                    "A creature whose printed stability bonus equals the shared "
                    + "native carrier's must use the shared one.");
            // No Intelligence score, so no ranks, so both skill totals are
            // ability plus the racial bonus and nothing else.
            if (SkillRanks != 0)
                throw new InvalidOperationException(
                    "A creature with no Intelligence score buys no skill ranks.");
            if (WisdomModifier + RacialPerceptionBonus != PrintedPerceptionSkill)
                throw new InvalidOperationException(
                    "Giant Crab printed skill derivation changed.");
            // Constrict is the claw's own damage, so it adds the whole
            // Strength modifier and nothing else. A constrict that differed
            // from the claw would mean one of the two had drifted.
            if (ConstrictDiceCount != 1 || ConstrictDieSides != ClawDieSides)
                throw new InvalidOperationException(
                    "Giant Crab printed constrict derivation changed: it is "
                    + "the claw's own die plus the whole Strength modifier.");
            // Space and reach are the engine's own for a Medium creature, so
            // there is nothing to represent and nothing to omit. Asserted
            // rather than assumed, because every earlier creature in this
            // programme that got reach wrong got it wrong by inheriting.
            if (PrintedSpaceFeet != 5 || PrintedReachFeet != 5)
                throw new InvalidOperationException(
                    "Giant Crab printed space and reach changed.");
            // The printed swim speed is recorded and unrepresented, and it is
            // NOT the ground speed. If these ever became equal the omission
            // would have quietly turned into a substitution.
            if (PrintedSwimSpeedFeet == SpeedFeet)
                throw new InvalidOperationException(
                    "The printed swim speed must stay distinct from the ground "
                    + "speed it is not substituted into.");
        }
    }
}
