using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// What the guarded Sprint 20 review expects to read back from a live
    /// Giant Scorpion.
    ///
    /// <para>Every number here is the printed one, and every one of them is
    /// derived in <see cref="GiantScorpionRulesPolicy"/> rather than typed
    /// twice: this file names what the review measures, and that file owns
    /// the arithmetic. A number that drifts breaks the derivation there
    /// before it reaches a guarded run.</para>
    ///
    /// <para>Two things this review checks that Sprint 19's could not. Grab
    /// and poison ride different limbs on one creature, so a claw that
    /// poisons or a sting that grabs is a defect the review has to be able
    /// to see. And the printed manoeuvre defence against trip is twelve
    /// higher than the ordinary one, which is the eight-legged carrier doing
    /// its job and nothing else doing it twice.</para>
    /// </summary>
    internal static class Sprint20ReviewPolicy
    {
        internal const string ScorpionKey =
            GiantScorpionRulesPolicy.GiantScorpionKey;

        internal static bool IsSprint20Creature(string key)
        { return string.Equals(key, ScorpionKey, StringComparison.Ordinal); }

        internal static string[] Keys { get { return new[] { ScorpionKey }; } }

        /// <summary>
        /// The review runs each routine in real time and in turn-based
        /// combat: a poison that only works in one mode is a defect, and so
        /// is a routine that loses a limb when the mode changes.
        /// </summary>
        internal static bool[] CombatModes { get { return new[] { false, true }; } }

        internal sealed class LiveProfile
        {
            internal int Strength, Dexterity, Constitution, Intelligence;
            internal int Wisdom, Charisma, HitDice, HitPoints;
            internal int ArmorClass, Touch, FlatFooted;
            internal int Fortitude, Reflex, Will;
            internal int CombatManeuverDefense, CombatManeuverDefenseVersusTrip;
            internal int SpeedFeet;
            internal Dictionary<string, int> Skills;
            /// <summary>
            /// One row per limb: rolls, die sides, damage bonus, attack
            /// bonus. All three are primary, at one bonus, and each adds the
            /// whole Strength modifier rather than half again.
            /// </summary>
            internal int[][] Limbs;
        }

        internal static LiveProfile Scorpion
        {
            get
            {
                var policy = typeof(GiantScorpionRulesPolicy);
                return new LiveProfile
                {
                    Strength = GiantScorpionRulesPolicy.Strength,
                    Dexterity = GiantScorpionRulesPolicy.Dexterity,
                    Constitution = GiantScorpionRulesPolicy.Constitution,
                    Intelligence =
                        GiantScorpionRulesPolicy.SubstitutedIntelligence,
                    Wisdom = GiantScorpionRulesPolicy.Wisdom,
                    Charisma = GiantScorpionRulesPolicy.Charisma,
                    HitDice = GiantScorpionRulesPolicy.HitDice,
                    HitPoints = GiantScorpionRulesPolicy.PrintedHitPoints,
                    ArmorClass = GiantScorpionRulesPolicy.PrintedArmorClass,
                    Touch = GiantScorpionRulesPolicy.PrintedTouchArmorClass,
                    FlatFooted =
                        GiantScorpionRulesPolicy.PrintedFlatFootedArmorClass,
                    Fortitude = GiantScorpionRulesPolicy.PrintedFortitude,
                    Reflex = GiantScorpionRulesPolicy.PrintedReflex,
                    Will = GiantScorpionRulesPolicy.PrintedWill,
                    CombatManeuverDefense =
                        GiantScorpionRulesPolicy.PrintedCombatManeuverDefense,
                    CombatManeuverDefenseVersusTrip = GiantScorpionRulesPolicy
                        .PrintedCombatManeuverDefenseVersusTrip,
                    SpeedFeet = GiantScorpionRulesPolicy.SpeedFeet,
                    // Two printed skills reach Kingmaker. Climb has no
                    // Kingmaker skill at all and is omitted under
                    // ORDINARY_MAP_LAND_USE_SCOPE, which costs this creature
                    // nothing because it has no ranks to spend.
                    Skills = new Dictionary<string, int>(StringComparer.Ordinal)
                    {
                        { "Perception",
                          GiantScorpionRulesPolicy.PrintedPerceptionSkill },
                        { "Stealth",
                          GiantScorpionRulesPolicy.PrintedStealthSkill },
                    },
                    // Claw, claw, sting. One bonus, one die, one damage
                    // bonus: the plain Strength modifier on every limb.
                    Limbs = new[] {
                        Limb(GiantScorpionRulesPolicy.ClawDieSides),
                        Limb(GiantScorpionRulesPolicy.ClawDieSides),
                        Limb(GiantScorpionRulesPolicy.StingDieSides),
                    },
                };
            }
        }

        private static int[] Limb(int dieSides)
        {
            return new[] { 1, dieSides,
                GiantScorpionRulesPolicy.StrengthModifier,
                GiantScorpionRulesPolicy.PrintedAttackBonus };
        }

        /// <summary>
        /// Which limb carries which rider. The whole point of the pair: a
        /// claw that poisons or a sting that grabs is the defect this names.
        /// </summary>
        internal static string[][] LimbRiders
        {
            get
            {
                return new[] {
                    new[] { "claw", "grab", "yes" },
                    new[] { "claw", "poison", "no" },
                    new[] { "sting", "grab", "no" },
                    new[] { "sting", "poison", "yes" },
                };
            }
        }

        /// <summary>
        /// The live rider cases: which limb lands, which side of the printed
        /// difficulty class the disposable target's save falls on, and what
        /// must happen.
        ///
        /// <para>Three cases rather than two, because a sting whose save is
        /// made and a sting whose save is failed are different measurements of
        /// the same gate: the first proves the save is real and the second
        /// proves the poison is. A claw case with no save at all is the third,
        /// and it is the one that would catch a poison attached to the wrong
        /// weapon.</para>
        /// </summary>
        internal static string[][] RiderCases
        {
            get
            {
                return new[] {
                    new[] { "claw-seizes", "claw", "save-irrelevant",
                            "may seize, never poisons" },
                    new[] { "sting-poisons", "sting", "save-failed",
                            "poisons, never seizes" },
                    new[] { "sting-save-made", "sting", "save-made",
                            "forces the save, poisons nothing, seizes nothing" },
                };
            }
        }

        internal static void Validate()
        {
            GiantScorpionRulesPolicy.Validate();
            if (CombatModes.Length != 2 || CombatModes[0] == CombatModes[1])
                throw new InvalidOperationException(
                    "Both combat modes must be reviewed.");
            if (Keys.Length != 1 || !IsSprint20Creature(Keys[0]))
                throw new InvalidOperationException(
                    "Sprint 20 reviews one creature.");
            LiveProfile profile = Scorpion;
            if (profile.Limbs.Length !=
                    GiantScorpionRulesPolicy.ClawCount +
                    GiantScorpionRulesPolicy.StingCount)
                throw new InvalidOperationException(
                    "The review must measure every printed limb.");
            // Every limb at one bonus and the plain Strength modifier. If
            // the engine's primary-hand rule reached this creature one row
            // would differ, and this is where that shows.
            if (profile.Limbs.Select(row => row[3]).Distinct().Count() != 1 ||
                profile.Limbs.Select(row => row[2]).Distinct().Count() != 1)
                throw new InvalidOperationException(
                    "Every Giant Scorpion limb shares one attack and damage "
                    + "bonus.");
            if (profile.Limbs.Any(row =>
                    row[2] != GiantScorpionRulesPolicy.StrengthModifier))
                throw new InvalidOperationException(
                    "A limb adding half again as much Strength is the defect "
                    + "the full-Strength carrier exists to prevent.");
            if (profile.CombatManeuverDefenseVersusTrip -
                    profile.CombatManeuverDefense !=
                    GiantScorpionRulesPolicy.EightLegTripBonus)
                throw new InvalidOperationException(
                    "The printed anti-trip defence is the eight-legged bonus.");
            // Exactly one limb grabs and exactly one poisons, and they are
            // not the same limb.
            string[] grabs = LimbRiders.Where(row => row[1] == "grab" &&
                row[2] == "yes").Select(row => row[0]).ToArray();
            string[] poisons = LimbRiders.Where(row => row[1] == "poison" &&
                row[2] == "yes").Select(row => row[0]).ToArray();
            if (grabs.Length != 1 || poisons.Length != 1 ||
                grabs[0] == poisons[0])
                throw new InvalidOperationException(
                    "Grab is on the claws and poison on the sting, and neither "
                    + "reaches the other.");
            // Every rider the table declares has a live case, and every live
            // case names a limb the rider table knows. A case for a limb that
            // carries neither rider would prove nothing, and a declared rider
            // with no case is the gap this is here to refuse.
            string[] cased = RiderCases.Select(row => row[1]).Distinct().ToArray();
            if (cased.Length != 2 || !cased.Contains("claw") ||
                !cased.Contains("sting"))
                throw new InvalidOperationException(
                    "Both limbs that carry a rider must be exercised live.");
            if (RiderCases.Any(row => !LimbRiders.Any(rider =>
                    rider[0] == row[1] && rider[2] == "yes")))
                throw new InvalidOperationException(
                    "A live rider case must name a limb that carries a rider.");
            if (RiderCases.Count(row => row[1] == "sting") != 2)
                throw new InvalidOperationException(
                    "The sting is measured on both sides of its save.");
            if (profile.Skills.Count != 2)
                throw new InvalidOperationException(
                    "Two printed skills reach Kingmaker; Climb has no Kingmaker "
                    + "skill and is recorded as omitted rather than substituted.");
        }
    }
}
