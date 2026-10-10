using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Bebelith's printed numbers, derived from its stat block rather than
    /// copied out of it.
    ///
    /// <para>This creature is already released at Summon Monster VII, and
    /// Sprint 21 keeps its public identity exactly - the same unit, the same
    /// three roots, the same ordering and the same save compatibility - while
    /// replacing what the released build got wrong or left out. The released
    /// chassis was already right about size, hit dice, ability scores, speed,
    /// natural armour and damage reduction; this policy is the authority for
    /// the rest, which is most of the creature's character: a bite-only rot, a
    /// web, penetrating strike, the printed anti-trip defence, the printed
    /// racial Stealth, and a dismantle that goes through the engine's own
    /// sunder manoeuvre instead of an invented difficulty class.</para>
    ///
    /// <para>Sixteen independent identities close on the transcription, which
    /// is what makes <see cref="Validate"/> a full account of the stat block
    /// rather than a sample of it. Two of them say something the released build
    /// did not know: the printed saves are a good Fortitude, a good Reflex and
    /// a POOR Will, and the printed Will of +7 needs Iron Will on top of that
    /// poor save. A three-good-save outsider chassis reads Will 9 or 11 and is
    /// therefore wrong for this creature, which is a thing only a live review
    /// can settle.</para>
    /// </summary>
    internal static class BebelithRulesPolicy
    {
        internal const string BebelithKey = "bebelith";

        // The printed stat block, transcribed at intake.
        internal const int HitDice = 12;
        internal const int Strength = 28;
        internal const int Dexterity = 12;
        internal const int Constitution = 24;
        internal const int Intelligence = 11;
        internal const int Wisdom = 13;
        internal const int Charisma = 13;
        internal const int SpeedFeet = 40;
        internal const int NaturalArmor = 13;
        internal const int BaseAttackBonus = 12;

        /// <summary>
        /// Huge: a -2 size term on armour class and attack rolls, and a +2 one
        /// on combat manoeuvres. Printed Space and Reach are both 15 feet,
        /// which is the engine's own for a Huge creature.
        /// </summary>
        internal const int SizePenalty = -2;
        internal const int SizeManeuverBonus = 2;
        internal const int PrintedSpaceFeet = 15;
        internal const int PrintedReachFeet = 15;

        /// <summary>
        /// Printed racial dice only: 12d10 at the monster convention averages
        /// 66. Native Constitution stays live, so the printed 150 is 66 racial
        /// plus 84 from Constitution and is never written down as a total.
        /// </summary>
        internal const int BaseRacialHitPoints = 66;
        internal const int PrintedHitPoints = 150;

        /// <summary>
        /// Twelve hit dice give +8 on a good save and +4 on a poor one. This
        /// creature's printed saves are good/good/poor with two feats on top,
        /// which is the one place the released chassis may differ from the
        /// book.
        /// </summary>
        internal const int GoodSave = 8;
        internal const int PoorSave = 4;
        internal const int LightningReflexesBonus = 2;
        internal const int IronWillBonus = 2;

        internal const int PrintedArmorClass = 22;
        internal const int PrintedTouchArmorClass = 9;
        internal const int PrintedFlatFootedArmorClass = 21;
        internal const int PrintedFortitude = 15;
        internal const int PrintedReflex = 11;
        internal const int PrintedWill = 7;
        internal const int PrintedCombatManeuverBonus = 23;
        internal const int PrintedCombatManeuverDefense = 34;
        internal const int PrintedCombatManeuverDefenseVersusTrip = 46;
        internal const int PrintedDamageReduction = 10;

        /// <summary>
        /// One bite and two claws, all three primary and all three at one
        /// attack bonus and the whole Strength modifier. The bite rolls 2d6
        /// and each claw 2d4, and the claws threaten a critical on 19 or 20 -
        /// the only printed critical range in this programme so far.
        /// </summary>
        internal const int ClawCount = 2;
        internal const int BiteCount = 1;
        internal const int PrintedAttackBonus = 19;
        internal const int BiteDiceCount = 2;
        internal const int BiteDieSides = 6;
        internal const int ClawDiceCount = 2;
        internal const int ClawDieSides = 4;
        internal const int ClawCriticalThreshold = 19;

        /// <summary>
        /// The printed stability bonus against trip: the difference between
        /// CMD 34 and CMD 46.
        ///
        /// <para>A bebelith walks on eight legs, and the stat-block convention
        /// is four per pair of legs beyond the first - three extra pairs, +12.
        /// That makes this the third creature in the series to print the figure
        /// and the third to need a carrier of its own, because the shared
        /// native eight-leg fact delivers
        /// <see cref="SharedNativeTripBonus"/>, which Sprint 20 measured on a
        /// live creature rather than inferring from its name.</para>
        /// </summary>
        internal const int EightLegTripBonus = 12;
        internal const int SharedNativeTripBonus = 8;

        /// <summary>
        /// Rot: a bite-only injury effect. Two Constitution on each failed
        /// save, five exposures - the bite round and four after it - and two
        /// consecutive successes to cure. The difficulty class is NOT stored:
        /// it is computed live from Constitution by the shared formula, so a
        /// buffed or weakened bebelith rots for what its live Constitution
        /// supports.
        /// </summary>
        internal const int RotConstitutionDamage = 2;
        internal const int RotExposures = 5;
        internal const int RotCureSaves = 2;
        internal const int PrintedRotDifficultyClass = 23;
        internal const string RotDamagedStat = "Constitution";

        /// <summary>
        /// Web: a ranged attack at +11 with the same Constitution-derived
        /// difficulty class as rot, and twelve hit points as an object.
        /// </summary>
        internal const int PrintedWebRangedAttack = 11;
        internal const int PrintedWebDifficultyClass = 23;
        internal const int PrintedWebHitPoints = 12;

        /// <summary>
        /// The printed racial bonus. One skill, and Kingmaker has it - unlike
        /// the Climb this creature also prints, which it does not.
        /// </summary>
        internal const int RacialStealthBonus = 8;

        /// <summary>
        /// The printed climb speed, recorded and unrepresented. Kingmaker has
        /// no climbing movement and no Climb skill, and this number is not
        /// folded into the 40-foot ground speed.
        /// </summary>
        internal const int PrintedClimbSpeedFeet = 20;

        internal static int StrengthModifier { get { return (Strength - 10) / 2; } }
        internal static int DexterityModifier { get { return (Dexterity - 10) / 2; } }
        internal static int ConstitutionModifier { get { return (Constitution - 10) / 2; } }
        internal static int WisdomModifier { get { return (Wisdom - 10) / 2; } }

        /// <summary>
        /// Ten plus half the hit dice plus the Constitution modifier, which is
        /// the tabletop formula the project's existing carriers already
        /// compute live. At the printed Constitution 24 and twelve hit dice
        /// this is 23 - and it is the difficulty class of both rot and the web,
        /// which is why neither is stored.
        /// </summary>
        internal static int DifficultyClass(int hitDice, int constitutionBonus)
        { return 10 + hitDice / 2 + constitutionBonus; }

        internal static void Validate()
        {
            if (StrengthModifier != 9 || DexterityModifier != 1 ||
                ConstitutionModifier != 7 || WisdomModifier != 1)
                throw new InvalidOperationException(
                    "Bebelith printed ability modifiers changed.");
            // 66 racial plus 84 from Constitution across twelve dice.
            if (BaseRacialHitPoints + HitDice * ConstitutionModifier !=
                    PrintedHitPoints)
                throw new InvalidOperationException(
                    "Bebelith printed hit point derivation changed.");
            // Armour class, touch and flat-footed from one set of parts, with
            // the Huge size penalty applied once to each. These three pin
            // Dexterity 12 and the natural armour at +13.
            if (10 + NaturalArmor + DexterityModifier + SizePenalty !=
                    PrintedArmorClass ||
                10 + DexterityModifier + SizePenalty !=
                    PrintedTouchArmorClass ||
                10 + NaturalArmor + SizePenalty !=
                    PrintedFlatFootedArmorClass)
                throw new InvalidOperationException(
                    "Bebelith printed armor class derivation changed.");
            // Good Fortitude, good Reflex, POOR Will, with Lightning Reflexes
            // and Iron Will on top. The Will clause is the one that says a
            // three-good-save chassis is wrong for this creature.
            if (GoodSave + ConstitutionModifier != PrintedFortitude ||
                GoodSave + DexterityModifier + LightningReflexesBonus !=
                    PrintedReflex ||
                PoorSave + WisdomModifier + IronWillBonus != PrintedWill)
                throw new InvalidOperationException(
                    "Bebelith printed save derivation changed.");
            // A good Will would read nine before Iron Will and eleven after,
            // and the stat block prints seven. Asserted rather than commented,
            // because the released chassis takes a native outsider class whose
            // save progression this project does not choose.
            if (GoodSave + WisdomModifier == PrintedWill ||
                GoodSave + WisdomModifier + IronWillBonus == PrintedWill)
                throw new InvalidOperationException(
                    "The printed Bebelith Will save is a poor save; a "
                    + "three-good-save chassis cannot produce it.");
            // One bonus across all three limbs, each adding the whole Strength
            // modifier rather than one and a half times it.
            if (BaseAttackBonus + StrengthModifier + SizePenalty !=
                    PrintedAttackBonus)
                throw new InvalidOperationException(
                    "Bebelith printed attack bonus derivation changed: base "
                    + "attack plus Strength less two for Huge size.");
            if (BiteCount + ClawCount != 3)
                throw new InvalidOperationException(
                    "The Bebelith prints one bite and two claws.");
            if (ClawCriticalThreshold != 19)
                throw new InvalidOperationException(
                    "The Bebelith's printed claw critical range is 19-20.");
            // Manoeuvre bonus and defence, with the Huge size term the other
            // way round from armour class.
            if (BaseAttackBonus + StrengthModifier + SizeManeuverBonus !=
                    PrintedCombatManeuverBonus ||
                10 + BaseAttackBonus + StrengthModifier + DexterityModifier +
                    SizeManeuverBonus != PrintedCombatManeuverDefense ||
                PrintedCombatManeuverDefense + EightLegTripBonus !=
                    PrintedCombatManeuverDefenseVersusTrip)
                throw new InvalidOperationException(
                    "Bebelith printed combat maneuver derivation changed.");
            // The whole reason this creature owns an anti-trip carrier, the
            // third time this programme has met it.
            if (EightLegTripBonus == SharedNativeTripBonus)
                throw new InvalidOperationException(
                    "A creature whose printed stability bonus equals the "
                    + "shared native carrier's must use the shared one.");
            // Rot and the web share one derived difficulty class, so neither
            // is stored. If this fails the formula moved, not the stat block.
            if (DifficultyClass(HitDice, ConstitutionModifier) !=
                    PrintedRotDifficultyClass ||
                DifficultyClass(HitDice, ConstitutionModifier) !=
                    PrintedWebDifficultyClass)
                throw new InvalidOperationException(
                    "Bebelith printed difficulty class derivation changed.");
            // The printed rot graph. Five exposures is the bite round plus
            // four, and two consecutive successes cure - both longer than
            // anything this project's poison carriers ship, which is why rot
            // needs its own.
            if (RotConstitutionDamage != 2 || RotExposures != 5 ||
                RotCureSaves != 2)
                throw new InvalidOperationException(
                    "Bebelith printed rot graph changed.");
            if (RotCureSaves == 1)
                throw new InvalidOperationException(
                    "A two-save cure cannot share the project's one-save "
                    + "poison carriers.");
            // The web's ranged attack takes Dexterity and the size penalty,
            // not Strength: it is thrown rather than swung.
            if (BaseAttackBonus + DexterityModifier + SizePenalty !=
                    PrintedWebRangedAttack)
                throw new InvalidOperationException(
                    "Bebelith printed web ranged attack derivation changed.");
            if (PrintedWebHitPoints != 12)
                throw new InvalidOperationException(
                    "Bebelith printed web hit points changed.");
            if (RacialStealthBonus != 8)
                throw new InvalidOperationException(
                    "Bebelith printed racial Stealth bonus changed.");
            // The climb speed is recorded and unrepresented, and it is NOT the
            // ground speed. If these ever became equal the omission would have
            // quietly turned into a substitution.
            if (PrintedClimbSpeedFeet == SpeedFeet)
                throw new InvalidOperationException(
                    "The printed climb speed must stay distinct from the "
                    + "ground speed it is not substituted into.");
            if (PrintedSpaceFeet != PrintedReachFeet)
                throw new InvalidOperationException(
                    "Bebelith printed space and reach changed.");
        }
    }
}
