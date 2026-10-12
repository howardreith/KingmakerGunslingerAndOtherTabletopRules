using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// What the guarded Sprint 21 review expects to read back from a live
    /// Bebelith.
    ///
    /// <para>Every number here is the printed one, derived in <see
    /// cref="BebelithRulesPolicy"/> rather than typed twice: this file names
    /// what the review measures, and that file owns the arithmetic.</para>
    ///
    /// <para>This is a different kind of review from the Giant Crab's, and
    /// the difference is the whole reason it is its own file. The crab is a
    /// new creature and the question is whether it arrives correct. The
    /// Bebelith is RELEASED, and the question is whether an overhaul that
    /// keeps its identity has actually changed its mechanics - so every case
    /// here is a before-and-after against a build a player already has, and
    /// four of them are negative controls rather than measurements.</para>
    ///
    /// <para>The negative controls are the point. Penetrating strike grants
    /// cold iron and good against demons only, and granting them universally
    /// is invisible against a demon and wrong against everything else; the
    /// only way to see it is to hit something that is not a demon and read
    /// the damage bundle. Dismantle Armor must do nothing at all to an
    /// unarmoured target and nothing to a natural-armoured one, and both of
    /// those are absences that no positive case can prove.</para>
    /// </summary>
    internal static class Sprint21BebelithReviewPolicy
    {
        internal const string BebelithKey = BebelithRulesPolicy.BebelithKey;

        /// <summary>
        /// The creature's own tier. The review casts the single-Bebelith root
        /// rather than one of the quantity roots, because three of them in a
        /// bounded fixture is three times the state to clean up for no extra
        /// evidence.
        /// </summary>
        internal const int OwnTier = 7;

        internal sealed class LiveProfile
        {
            internal int Strength, Dexterity, Constitution, Intelligence;
            internal int Wisdom, Charisma, HitDice, HitPoints;
            internal int ArmorClass, Touch, FlatFooted;
            internal int Fortitude, Reflex, Will;
            internal int CombatManeuverDefense, CombatManeuverDefenseVersusTrip;
            internal int SpeedFeet, DamageReduction;
            internal Dictionary<string, int> Skills;
            /// <summary>
            /// One row per limb: rolls, die sides, damage bonus, attack bonus.
            /// The bite first, because it is the primary limb and the one rot
            /// rides; then two claws.
            /// </summary>
            internal int[][] Limbs;
        }

        internal static LiveProfile Bebelith
        {
            get
            {
                return new LiveProfile
                {
                    Strength = BebelithRulesPolicy.Strength,
                    Dexterity = BebelithRulesPolicy.Dexterity,
                    Constitution = BebelithRulesPolicy.Constitution,
                    Intelligence = BebelithRulesPolicy.Intelligence,
                    Wisdom = BebelithRulesPolicy.Wisdom,
                    Charisma = BebelithRulesPolicy.Charisma,
                    HitDice = BebelithRulesPolicy.HitDice,
                    HitPoints = BebelithRulesPolicy.PrintedHitPoints,
                    ArmorClass = BebelithRulesPolicy.PrintedArmorClass,
                    Touch = BebelithRulesPolicy.PrintedTouchArmorClass,
                    FlatFooted =
                        BebelithRulesPolicy.PrintedFlatFootedArmorClass,
                    Fortitude = BebelithRulesPolicy.PrintedFortitude,
                    Reflex = BebelithRulesPolicy.PrintedReflex,
                    Will = BebelithRulesPolicy.PrintedWill,
                    CombatManeuverDefense =
                        BebelithRulesPolicy.PrintedCombatManeuverDefense,
                    CombatManeuverDefenseVersusTrip = BebelithRulesPolicy
                        .PrintedCombatManeuverDefenseVersusTrip,
                    SpeedFeet = BebelithRulesPolicy.SpeedFeet,
                    DamageReduction =
                        BebelithRulesPolicy.PrintedDamageReduction,
                    // One printed racial skill reaches Kingmaker. Climb has
                    // no Kingmaker skill at all and is omitted under
                    // ORDINARY_MAP_LAND_USE_SCOPE.
                    Skills = new Dictionary<string, int>(StringComparer.Ordinal)
                    {
                        { "Stealth", BebelithRulesPolicy.RacialStealthBonus },
                    },
                    Limbs = new[] { Bite(), Claw(), Claw() },
                };
            }
        }

        private static int[] Bite()
        {
            return new[] { BebelithRulesPolicy.BiteDiceCount,
                BebelithRulesPolicy.BiteDieSides,
                BebelithRulesPolicy.StrengthModifier,
                BebelithRulesPolicy.PrintedAttackBonus };
        }

        private static int[] Claw()
        {
            return new[] { BebelithRulesPolicy.ClawDiceCount,
                BebelithRulesPolicy.ClawDieSides,
                BebelithRulesPolicy.StrengthModifier,
                BebelithRulesPolicy.PrintedAttackBonus };
        }

        /// <summary>
        /// Which limb carries which rider, and - as importantly - which does
        /// not. Rot is bite-only; the claws carry the dismantle sequence and
        /// no injury effect at all.
        /// </summary>
        internal static string[][] LimbRiders
        {
            get
            {
                return new[] {
                    new[] { "bite", "rot", "yes" },
                    new[] { "claw", "rot", "no" },
                    new[] { "claw", "dismantle", "yes" },
                    new[] { "bite", "dismantle", "no" },
                };
            }
        }

        /// <summary>
        /// The live cases, in the order they are run. Four of the seven are
        /// negative controls, which is the right proportion for a creature
        /// whose two hardest mechanics are both defined by what they must NOT
        /// touch.
        /// </summary>
        internal static string[][] Cases
        {
            get
            {
                return new[] {
                    new[] { "rot-rides-the-bite", "measurement",
                        "a bite that wounds forces a Fortitude save at the "
                        + "printed difficulty class" },
                    new[] { "rot-spares-the-claw", "negative-control",
                        "a claw that wounds forces nothing" },
                    new[] { "penetrating-strike-general", "measurement",
                        "every natural attack counts as chaotic and magic" },
                    new[] { "penetrating-strike-spares-non-demons",
                        "negative-control",
                        "a non-demon target sees no cold iron and no good" },
                    new[] { "dismantle-needs-both-claws", "measurement",
                        "one claw landing attempts nothing; the second "
                        + "attempts the engine's own sunder manoeuvre" },
                    new[] { "dismantle-spares-the-unarmoured",
                        "negative-control",
                        "a target with nothing equipped sees no manoeuvre, "
                        + "no save and no state" },
                    new[] { "dismantle-touches-no-inventory",
                        "negative-control",
                        "the target's equipment is byte-identical before and "
                        + "after a successful dismantle" },
                };
            }
        }

        /// <summary>
        /// The alignment value the engine uses for chaotic evil, which is
        /// what <see cref="ExpandedSummoningSpecialProfiles"/> reads to decide
        /// whether a target is a demon. Named here so the review can arrange
        /// a demon control without the arrangement being a magic number.
        /// </summary>
        internal const int ChaoticEvilAlignment = 20;

        internal static void Validate()
        {
            BebelithRulesPolicy.Validate();
            LiveProfile profile = Bebelith;
            if (profile.Limbs.Length !=
                    BebelithRulesPolicy.BiteCount + BebelithRulesPolicy.ClawCount)
                throw new InvalidOperationException(
                    "The review must measure every printed limb: one bite "
                    + "and two claws.");
            // Every limb at one attack bonus, which the printed block says
            // and which the released build's limb order did not produce.
            if (profile.Limbs.Select(row => row[3]).Distinct().Count() != 1)
                throw new InvalidOperationException(
                    "Every printed Bebelith limb attacks at +19.");
            if (profile.Limbs.Any(row =>
                    row[2] != BebelithRulesPolicy.StrengthModifier))
                throw new InvalidOperationException(
                    "A limb adding half again as much Strength is the defect "
                    + "the full-Strength carrier exists to prevent.");
            // The bite is first and is the only limb with its own die, which
            // is how the review tells them apart in a damage bundle.
            if (profile.Limbs[0][1] == profile.Limbs[1][1])
                throw new InvalidOperationException(
                    "The bite and the claws must print different dice, or a "
                    + "measured bundle cannot say which limb produced it.");
            if (profile.CombatManeuverDefenseVersusTrip -
                    profile.CombatManeuverDefense !=
                    BebelithRulesPolicy.EightLegTripBonus)
                throw new InvalidOperationException(
                    "The printed anti-trip defence is the eight-legged bonus, "
                    + "which the shared native fact cannot deliver.");
            // Rot on the bite and nowhere else; the dismantle on the claws
            // and nowhere else. Both halves are stated, because "bite only"
            // is a claim about the claws.
            foreach (string[] required in new[] {
                new[] { "bite", "rot", "yes" },
                new[] { "claw", "rot", "no" },
                new[] { "claw", "dismantle", "yes" },
                new[] { "bite", "dismantle", "no" } })
                if (!LimbRiders.Any(row => row.SequenceEqual(required)))
                    throw new InvalidOperationException(
                        "The rider table must state both halves of "
                        + required[1] + " on the " + required[0]);
            // More than half the live cases are negative controls. That is
            // not padding: this creature's two hardest mechanics are both
            // defined by what they must not touch, and an absence cannot be
            // proved by a positive case.
            int controls = Cases.Count(row => row[1] == "negative-control");
            if (controls < Cases.Length - controls)
                throw new InvalidOperationException(
                    "An overhaul of a released creature is mostly negative "
                    + "controls, or it is not measuring what changed.");
            if (!Cases.Any(row => row[0].Contains("non-demon")))
                throw new InvalidOperationException(
                    "Penetrating strike's demon half is invisible against a "
                    + "demon; the control is the only case that can see it.");
            if (!Cases.Any(row => row[0].Contains("unarmoured")) ||
                !Cases.Any(row => row[0].Contains("inventory")))
                throw new InvalidOperationException(
                    "Dismantle Armor's mandatory safety is an absence, and "
                    + "these two cases are the only ones that can see it.");
            if (profile.Skills.Count != 1 ||
                !profile.Skills.ContainsKey("Stealth"))
                throw new InvalidOperationException(
                    "One printed racial skill reaches Kingmaker; Climb has no "
                    + "Kingmaker skill and is recorded as omitted rather than "
                    + "substituted.");
            if (ChaoticEvilAlignment != 20)
                throw new InvalidOperationException(
                    "The demon discrimination reads the engine's chaotic "
                    + "evil alignment value.");
        }
    }
}
