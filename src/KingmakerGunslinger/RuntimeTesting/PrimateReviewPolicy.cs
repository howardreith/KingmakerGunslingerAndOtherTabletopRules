using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// What the Sprint 18 guarded review expects to read off a live ape.
    ///
    /// <para>These are the printed stat blocks, not preferences. The runtime
    /// review reads the live creature and compares it to this; the offline
    /// suite checks that this still agrees with what the rules policy
    /// derives. A number that drifts has to break both.</para>
    /// </summary>
    internal sealed class PrimateLiveProfile
    {
        internal string Key;
        internal int Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma;
        internal int HitDice, HitPoints, ArmorClass, Touch, FlatFooted;
        internal int Fortitude, Reflex, Will, CombatManeuverDefense;
        internal int AttackBonus;
        /// <summary>Printed skill totals. Stealth is unprinted on the Ape.</summary>
        internal int Mobility, Perception;
        internal int? Stealth;
        /// <summary>Limb damage as (rolls, die sides, bonus), in limb order.</summary>
        internal int[][] Limbs;
        internal bool HasRend;
        internal int RendRolls, RendDieSides, RendBonus;
    }

    internal static class PrimateReviewPolicy
    {
        /// <summary>
        /// Every way a Dire Ape attack sequence can end, and whether the rend
        /// is owed. The review drives each one and proves the emission.
        /// </summary>
        internal static string[][] RendCases
        {
            get
            {
                return new[] {
                    new[] { "both-claws-one-target", "qualifies" },
                    new[] { "one-claw-only", "no-rend" },
                    new[] { "claws-on-two-targets", "no-rend" },
                    new[] { "bite-and-one-claw", "no-rend" },
                    new[] { "claws-across-two-commands", "no-rend" },
                    new[] { "claws-across-two-turns", "no-rend" },
                    new[] { "second-sequence-after-a-rend", "qualifies-once-more" },
                };
            }
        }

        internal static PrimateLiveProfile For(string key)
        {
            if (key == PrimateRulesPolicy.ApeKey)
                return new PrimateLiveProfile {
                    Key = key,
                    Strength = 15, Dexterity = 15, Constitution = 14,
                    Intelligence = 2, Wisdom = 12, Charisma = 7,
                    HitDice = 3, HitPoints = 19, ArmorClass = 14, Touch = 11,
                    FlatFooted = 12, Fortitude = 7, Reflex = 5, Will = 2,
                    CombatManeuverDefense = 17, AttackBonus = 3,
                    Mobility = 6, Perception = 8, Stealth = null,
                    Limbs = new[] { new[] { 1, 6, 2 }, new[] { 1, 6, 2 } },
                    HasRend = false,
                };
            if (key == PrimateRulesPolicy.DireApeKey)
                return new PrimateLiveProfile {
                    Key = key,
                    Strength = 19, Dexterity = 15, Constitution = 16,
                    Intelligence = 2, Wisdom = 12, Charisma = 7,
                    HitDice = 4, HitPoints = 30, ArmorClass = 15, Touch = 11,
                    FlatFooted = 13, Fortitude = 7, Reflex = 6, Will = 4,
                    CombatManeuverDefense = 20, AttackBonus = 6,
                    Mobility = 6, Perception = 8, Stealth = 2,
                    Limbs = new[] { new[] { 1, 6, 4 }, new[] { 1, 4, 4 },
                        new[] { 1, 4, 4 } },
                    HasRend = true, RendRolls = 1, RendDieSides = 4, RendBonus = 6,
                };
            throw new ArgumentOutOfRangeException("key", key,
                "Sprint 18 reviews exactly the two apes.");
        }

        /// <summary>
        /// The live routine, as a count of attacks in one full attack. The Ape
        /// has two slams and nothing else; the Dire Ape has one bite and two
        /// claws, and all three are primary.
        /// </summary>
        internal static int ExpectedAttacksInAFullAttack(string key)
        { return For(key).Limbs.Length; }

        /// <summary>
        /// The review runs each routine in real time and in turn-based
        /// combat: a rend that only works in one mode is a defect, and so is a
        /// routine that loses a limb when the mode changes.
        /// </summary>
        internal static bool[] CombatModes { get { return new[] { false, true }; } }

        internal static void Validate()
        {
            foreach (string key in new[] { PrimateRulesPolicy.ApeKey,
                PrimateRulesPolicy.DireApeKey })
            {
                PrimateLiveProfile live = For(key);
                PrimateRulesProfile printed = PrimateRulesPolicy.For(key);
                if (live.Key != key || live.Strength != printed.Strength ||
                        live.HitDice != printed.HitDice)
                    throw new InvalidOperationException(
                        "The review expectation and the rules policy disagree: " + key);
                // Printed hit points are the racial dice plus Constitution per
                // die, which is exactly what the builder gives the live unit.
                if (live.HitPoints != printed.BaseHitPoints +
                        printed.HitDice * ((live.Constitution - 10) / 2))
                    throw new InvalidOperationException(
                        "The expected live hit points are not the printed roll plus " +
                        "Constitution per hit die: " + key);
                // Every limb is a primary natural attack in both routines, so
                // every limb carries the whole Strength modifier.
                if (live.Limbs.Length < 2 ||
                        live.Limbs.Any(limb => limb.Length != 3 || limb[0] <= 0 ||
                            limb[1] <= 0 || limb[2] != printed.StrengthModifier))
                    throw new InvalidOperationException(
                        "A limb is malformed or does not carry full Strength: " + key);
                if (live.AttackBonus != printed.HitDice * 3 / 4 +
                        printed.StrengthModifier - 1)
                    throw new InvalidOperationException(
                        "The expected attack bonus is not animal BAB plus Strength " +
                        "minus Large size: " + key);
                // Stealth is printed on the Dire Ape and unprinted on the Ape,
                // and the review must not invent a rank for the Ape.
                if ((live.Stealth.HasValue ? 1 : 0) != printed.StealthRanks)
                    throw new InvalidOperationException(
                        "The expected Stealth does not match the printed ranks: " + key);
                if (printed.MobilityRanks != 1 || printed.PerceptionRanks != 1)
                    throw new InvalidOperationException(
                        "Both apes print exactly one Mobility and one Perception rank.");
                if (live.HasRend != printed.HasRend)
                    throw new InvalidOperationException(
                        "Rend presence disagrees with the rules policy: " + key);
                if (live.HasRend && (live.RendRolls != printed.RendDiceCount ||
                        live.RendDieSides != printed.RendDieSides ||
                        live.RendBonus != printed.RendBonus))
                    throw new InvalidOperationException(
                        "The expected rend is not the printed one: " + key);
                if (!live.HasRend && (live.RendRolls != 0 || live.RendBonus != 0))
                    throw new InvalidOperationException(
                        "A creature with no printed rend must expect none: " + key);
                if (ExpectedAttacksInAFullAttack(key) != live.Limbs.Length)
                    throw new InvalidOperationException("Routine length disagrees: " + key);
            }
            string[] names = RendCases.Select(row => row[0]).ToArray();
            if (names.Distinct(StringComparer.Ordinal).Count() != names.Length ||
                    RendCases.Count(row => row[1] == "no-rend") != 5 ||
                    RendCases.Count(row => row[1].StartsWith("qualifies",
                        StringComparison.Ordinal)) != 2)
                throw new InvalidOperationException(
                    "The rend case table must keep its five non-qualifying cases.");
            if (CombatModes.Length != 2 || CombatModes[0] == CombatModes[1])
                throw new InvalidOperationException(
                    "Both combat modes must be reviewed.");
        }
    }
}
