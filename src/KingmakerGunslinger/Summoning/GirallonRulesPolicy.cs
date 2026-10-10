using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Girallon's printed numbers, derived from its stat block rather than
    /// copied out of it.
    ///
    /// <para>These are the static baseline contract: what an unmodified
    /// Girallon must reproduce. They are not the runtime source of anything
    /// that can legitimately change. The rend is the clearest case - the
    /// printed line reads 1d4+6, and the implementation is the derivation that
    /// produces it, so a buffed, enlarged or weakened Girallon rends for what
    /// its live Strength supports.</para>
    ///
    /// <para>Nothing here is a framework. It is one creature's arithmetic,
    /// reachable by one creature, and the only thing it shares with Sprint 18
    /// is the rend state machine, which Sprint 19 gave a claw count rather
    /// than duplicating.</para>
    /// </summary>
    internal static class GirallonRulesPolicy
    {
        internal const string GirallonKey = "girallon";

        // The printed stat block, transcribed at intake from the Bestiary
        // entry rather than recalled. The recalled profile was wrong in four
        // places - Strength 22, 1d6 claws, a 1d8 bite and a two-claw rend -
        // and every one of those would have reached the player.
        internal const int HitDice = 7;
        internal const int Strength = 19;
        internal const int Dexterity = 17;
        internal const int Constitution = 18;
        internal const int Intelligence = 2;
        internal const int Wisdom = 12;
        internal const int Charisma = 7;
        internal const int SpeedFeet = 40;
        internal const int NaturalArmor = 6;
        internal const int BaseAttackBonus = 7;

        /// <summary>
        /// Printed racial dice only: 7d10 averages 38. Native Constitution,
        /// the Toughness feat and level dependencies all remain live, so the
        /// printed 73 is 38 racial plus 28 from Constitution plus 7 from
        /// Toughness and is never written down as a total.
        /// </summary>
        internal const int BaseRacialHitPoints = 38;
        internal const int PrintedHitPoints = 73;

        /// <summary>
        /// A magical beast's 7 hit dice give +5 on its two good saves and +2
        /// on its poor one. The generic donor class does not choose a
        /// creature-specific pair, so these are set on racial bases only.
        /// </summary>
        internal const int GoodSave = 5;
        internal const int PoorSave = 2;

        internal const int PrintedArmorClass = 18;
        internal const int PrintedTouchArmorClass = 12;
        internal const int PrintedFlatFootedArmorClass = 15;
        internal const int PrintedFortitude = 9;
        internal const int PrintedReflex = 8;
        internal const int PrintedWill = 5;
        internal const int PrintedCombatManeuverBonus = 12;
        internal const int PrintedCombatManeuverDefense = 25;

        /// <summary>
        /// One primary bite and four primary claws, every one at the same
        /// attack bonus and the whole Strength modifier. Five attacks, not a
        /// framework for five: they are five entries in this creature's own
        /// limb list, the same list the Dire Ape uses for two.
        /// </summary>
        internal const int ClawCount = 4;
        internal const int PrintedAttackBonus = 10;
        internal const int BiteDieSides = 6;
        internal const int ClawDieSides = 4;
        internal const int RendDiceCount = 1;
        internal const int RendDieSides = 4;

        // Seven skill ranks, which is what 7 hit dice at Intelligence 2 buys
        // a magical beast, and they allocate exactly: four on Perception,
        // three on Stealth, none on Climb.
        internal const int PerceptionRanks = 4;
        internal const int StealthRanks = 3;
        internal const int ClimbRanks = 0;
        internal const int PrintedPerceptionSkill = 11;
        internal const int PrintedStealthSkill = 5;

        /// <summary>
        /// The printed Climb total this project does not represent. Unlike the
        /// Sprint 18 apes, omitting it forfeits no printed rank: the +12 is
        /// Strength plus the racial climb-speed bonus and the Girallon spends
        /// no rank on it. Kept as a recorded number rather than a silence.
        /// </summary>
        internal const int PrintedClimbSkill = 12;
        internal const int ClimbSpeedFeet = 40;

        internal static int StrengthModifier { get { return (Strength - 10) / 2; } }

        /// <summary>
        /// One and a half times the Strength modifier, the tabletop bonus for
        /// a rending attack. Shared with the constrict derivation so the two
        /// cannot drift apart, and equal to the engine's own
        /// <c>(int)(Strength.Bonus * 1.5f)</c> on every reachable modifier.
        /// </summary>
        internal static int RendBonus
        {
            get
            {
                return ExpandedSummoningSpecialProfiles.ConstrictBonus(
                    StrengthModifier);
            }
        }

        internal static string RendDamage
        {
            get
            {
                return RendDiceCount + "d" + RendDieSides +
                    (RendBonus >= 0 ? "+" : "-") + Math.Abs(RendBonus);
            }
        }

        internal static void Validate()
        {
            // The printed line reads 1d4+6 at Strength 19. If this ever fails,
            // the derivation moved, not the stat block.
            if (RendDamage != "1d4+6" || RendBonus != 6 || StrengthModifier != 4)
                throw new InvalidOperationException(
                    "Girallon printed rend derivation changed.");
            // Every limb adds the plain Strength modifier, not one and a half
            // times it. The engine grants the larger figure to a natural
            // primary-hand weapon whenever the secondary hand is empty and
            // never looks at the additional limbs, which is why this creature
            // carries the Sprint 18 full-Strength correction.
            if (BaseAttackBonus + StrengthModifier - 1 != PrintedAttackBonus)
                throw new InvalidOperationException(
                    "Girallon printed attack bonus derivation changed: "
                    + "base attack plus Strength less one for Large size.");
            // 38 racial plus 28 from Constitution plus 7 from Toughness.
            if (BaseRacialHitPoints + HitDice * ((Constitution - 10) / 2) +
                    HitDice != PrintedHitPoints)
                throw new InvalidOperationException(
                    "Girallon printed hit point derivation changed.");
            if (GoodSave + (Constitution - 10) / 2 != PrintedFortitude ||
                GoodSave + (Dexterity - 10) / 2 != PrintedReflex ||
                PoorSave + (Wisdom - 10) / 2 + 2 != PrintedWill)
                throw new InvalidOperationException(
                    "Girallon printed save derivation changed.");
            // Armour class, touch and flat-footed from one set of parts, with
            // the Large size penalty applied once to each.
            if (10 + NaturalArmor + (Dexterity - 10) / 2 - 1 != PrintedArmorClass ||
                10 + (Dexterity - 10) / 2 - 1 != PrintedTouchArmorClass ||
                10 + NaturalArmor - 1 != PrintedFlatFootedArmorClass)
                throw new InvalidOperationException(
                    "Girallon printed armor class derivation changed.");
            if (BaseAttackBonus + StrengthModifier + 1 !=
                    PrintedCombatManeuverBonus ||
                10 + BaseAttackBonus + StrengthModifier +
                    (Dexterity - 10) / 2 + 1 != PrintedCombatManeuverDefense)
                throw new InvalidOperationException(
                    "Girallon printed combat maneuver derivation changed.");
            // Seven hit dice at Intelligence 2 buy a magical beast seven
            // ranks, and the stat block spends exactly seven.
            if (PerceptionRanks + StealthRanks + ClimbRanks != HitDice)
                throw new InvalidOperationException(
                    "Girallon printed skill ranks no longer total its hit dice.");
            // Perception is 4 ranks plus 3 class plus 1 Wisdom plus 3 Skill
            // Focus; Stealth is 3 ranks plus 3 class plus 3 Dexterity less 4
            // for Large size.
            if (PerceptionRanks + 3 + (Wisdom - 10) / 2 + 3 !=
                    PrintedPerceptionSkill ||
                StealthRanks + 3 + (Dexterity - 10) / 2 - 4 !=
                    PrintedStealthSkill)
                throw new InvalidOperationException(
                    "Girallon printed skill derivation changed.");
            // Climb costs no rank, so the omission forfeits nothing. The
            // printed +12 is Strength plus the racial climb-speed bonus.
            if (ClimbRanks != 0 || StrengthModifier + 8 != PrintedClimbSkill)
                throw new InvalidOperationException(
                    "Girallon printed Climb derivation changed; the omission "
                    + "is only free while it costs no rank.");
            if (ClawCount != 4)
                throw new InvalidOperationException(
                    "The Girallon prints four claws and its rend needs all four.");
            if (PrimateRulesPolicy.RendClawCount(GirallonKey) != ClawCount)
                throw new InvalidOperationException(
                    "The shared rend state machine disagrees with the printed "
                    + "Girallon claw count.");
        }
    }
}
