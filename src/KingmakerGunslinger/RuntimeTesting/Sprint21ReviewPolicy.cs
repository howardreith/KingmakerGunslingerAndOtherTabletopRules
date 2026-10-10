using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// What the guarded Sprint 21 review expects to read back from a live
    /// Giant Crab.
    ///
    /// <para>Every number here is the printed one, and every one of them is
    /// derived in <see cref="GiantCrabRulesPolicy"/> rather than typed twice:
    /// this file names what the review measures, and that file owns the
    /// arithmetic.</para>
    ///
    /// <para>This creature is the simplest thing the series has reviewed and
    /// the review is written to say so rather than to look busy. Two claws,
    /// one rider, no poison, no second limb type - so the three cases Sprint
    /// 20 needed to separate grab from poison collapse into one, and what is
    /// left is the part that actually went wrong twice in Sprint 20: printed
    /// lines with a derivation and no carrier. Both of this creature's own
    /// carriers are of exactly that kind - a +12 anti-trip defence the shared
    /// native fact cannot deliver, and a racial +4 that is the whole of the
    /// printed Perception total - so they are measured first and from the
    /// engine rather than from the builder.</para>
    /// </summary>
    internal static class Sprint21ReviewPolicy
    {
        internal const string CrabKey = GiantCrabRulesPolicy.GiantCrabKey;

        internal static bool IsSprint21Creature(string key)
        { return string.Equals(key, CrabKey, StringComparison.Ordinal); }

        internal static string[] Keys { get { return new[] { CrabKey }; } }

        /// <summary>
        /// The review runs each routine in real time and in turn-based
        /// combat: a grab that only works in one mode is a defect, and so is
        /// a routine that loses a limb when the mode changes.
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
            /// One row per limb: rolls, die sides, damage bonus, attack bonus.
            /// Both are primary, at one bonus, and each adds the whole
            /// Strength modifier rather than half again.
            /// </summary>
            internal int[][] Limbs;
        }

        internal static LiveProfile Crab
        {
            get
            {
                return new LiveProfile
                {
                    Strength = GiantCrabRulesPolicy.Strength,
                    Dexterity = GiantCrabRulesPolicy.Dexterity,
                    Constitution = GiantCrabRulesPolicy.Constitution,
                    Intelligence =
                        GiantCrabRulesPolicy.SubstitutedIntelligence,
                    Wisdom = GiantCrabRulesPolicy.Wisdom,
                    Charisma = GiantCrabRulesPolicy.Charisma,
                    HitDice = GiantCrabRulesPolicy.HitDice,
                    HitPoints = GiantCrabRulesPolicy.PrintedHitPoints,
                    ArmorClass = GiantCrabRulesPolicy.PrintedArmorClass,
                    Touch = GiantCrabRulesPolicy.PrintedTouchArmorClass,
                    FlatFooted =
                        GiantCrabRulesPolicy.PrintedFlatFootedArmorClass,
                    Fortitude = GiantCrabRulesPolicy.PrintedFortitude,
                    Reflex = GiantCrabRulesPolicy.PrintedReflex,
                    Will = GiantCrabRulesPolicy.PrintedWill,
                    CombatManeuverDefense =
                        GiantCrabRulesPolicy.PrintedCombatManeuverDefense,
                    CombatManeuverDefenseVersusTrip = GiantCrabRulesPolicy
                        .PrintedCombatManeuverDefenseVersusTrip,
                    SpeedFeet = GiantCrabRulesPolicy.SpeedFeet,
                    // One printed skill reaches Kingmaker. Swim has no
                    // Kingmaker skill at all and is omitted under
                    // ORDINARY_MAP_LAND_USE_SCOPE, which costs this creature
                    // nothing because it has no ranks to spend.
                    Skills = new Dictionary<string, int>(StringComparer.Ordinal)
                    {
                        { "Perception",
                          GiantCrabRulesPolicy.PrintedPerceptionSkill },
                    },
                    // Claw, claw. One bonus, one die, one damage bonus: the
                    // plain Strength modifier on both.
                    Limbs = new[] { Limb(), Limb() },
                };
            }
        }

        private static int[] Limb()
        {
            return new[] { 1, GiantCrabRulesPolicy.ClawDieSides,
                GiantCrabRulesPolicy.StrengthModifier,
                GiantCrabRulesPolicy.PrintedAttackBonus };
        }

        /// <summary>
        /// Which limb carries which rider. Shorter than Sprint 20's because
        /// there is only one rider and only one kind of limb - but still a
        /// table, because "every limb grabs" is a claim about every limb and
        /// the review should be able to name the one that did not.
        /// </summary>
        internal static string[][] LimbRiders
        {
            get
            {
                return new[] {
                    new[] { "claw", "grab", "yes" },
                };
            }
        }

        /// <summary>
        /// The live rider cases. One, where Sprint 20 needed three: a crab has
        /// no second limb type for a rider to leak onto, so the case that
        /// matters is whether a claw that lands actually seizes.
        /// </summary>
        internal static string[][] RiderCases
        {
            get
            {
                return new[] {
                    new[] { "claw-seizes", "claw", "may seize" },
                };
            }
        }

        internal static void Validate()
        {
            GiantCrabRulesPolicy.Validate();
            if (CombatModes.Length != 2 || CombatModes[0] == CombatModes[1])
                throw new InvalidOperationException(
                    "Both combat modes must be reviewed.");
            if (Keys.Length != 1 || !IsSprint21Creature(Keys[0]))
                throw new InvalidOperationException(
                    "Sprint 21 reviews one creature live.");
            LiveProfile profile = Crab;
            if (profile.Limbs.Length != GiantCrabRulesPolicy.ClawCount)
                throw new InvalidOperationException(
                    "The review must measure every printed limb.");
            // Both limbs at one bonus and the plain Strength modifier. If the
            // engine's primary-hand rule reached this creature one row would
            // differ, and this is where that shows.
            if (profile.Limbs.Select(row => row[3]).Distinct().Count() != 1 ||
                profile.Limbs.Select(row => row[2]).Distinct().Count() != 1)
                throw new InvalidOperationException(
                    "Both Giant Crab claws share one attack and damage bonus.");
            if (profile.Limbs.Any(row =>
                    row[2] != GiantCrabRulesPolicy.StrengthModifier))
                throw new InvalidOperationException(
                    "A limb adding half again as much Strength is the defect "
                    + "the full-Strength carrier exists to prevent.");
            if (profile.CombatManeuverDefenseVersusTrip -
                    profile.CombatManeuverDefense !=
                    GiantCrabRulesPolicy.EightLegTripBonus)
                throw new InvalidOperationException(
                    "The printed anti-trip defence is the eight-legged bonus.");
            // Every limb this creature has carries the rider, and there is no
            // limb that does not. That is a different statement from Sprint
            // 20's and the table has to be able to make it.
            string[] grabs = LimbRiders.Where(row => row[1] == "grab" &&
                row[2] == "yes").Select(row => row[0]).ToArray();
            if (grabs.Length != 1 || grabs[0] != "claw")
                throw new InvalidOperationException(
                    "Grab is on the claws, which is every limb a crab has.");
            if (RiderCases.Length != 1 || RiderCases[0][1] != "claw")
                throw new InvalidOperationException(
                    "One rider, one kind of limb, one live case.");
            if (profile.Skills.Count != 1 ||
                !profile.Skills.ContainsKey("Perception"))
                throw new InvalidOperationException(
                    "One printed skill reaches Kingmaker; Swim has no "
                    + "Kingmaker skill and is recorded as omitted rather than "
                    + "substituted.");
        }
    }
}
