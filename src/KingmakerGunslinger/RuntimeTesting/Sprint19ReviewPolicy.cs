using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// What the Sprint 19 guarded review expects to read off a live Girallon
    /// or Xill.
    ///
    /// <para>These are the printed stat blocks, not preferences. The runtime
    /// review reads the live creature and compares it to this; the offline
    /// suite checks that this still agrees with what the rules policies
    /// derive. A number that drifts has to break both.</para>
    ///
    /// <para>Unlike Sprint 18, a limb carries its own attack bonus here. The
    /// Xill's claws are +13 and its bite +12 because Weapon Focus (claw)
    /// reaches one and not the other, and a single shared bonus could not say
    /// so.</para>
    /// </summary>
    internal sealed class Sprint19LiveProfile
    {
        internal string Key;
        internal int Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma;
        internal int HitDice, HitPoints, ArmorClass, Touch, FlatFooted;
        internal int Fortitude, Reflex, Will, CombatManeuverDefense;

        /// <summary>
        /// Limbs in body order - the primary hand first, then the additional
        /// limbs - as (rolls, die sides, damage bonus, attack bonus).
        /// </summary>
        internal int[][] Limbs;

        /// <summary>Printed skill totals, by Kingmaker skill name.</summary>
        internal Dictionary<string, int> Skills;

        internal bool HasRend;
        internal int RendRolls, RendDieSides, RendBonus, RendClawCount;

        /// <summary>Zero when the creature prints no spell resistance.</summary>
        internal int SpellResistance;

        /// <summary>Zero when the creature prints no grab.</summary>
        internal int GrappleBonus;

        /// <summary>Zero when the creature prints no paralysis.</summary>
        internal int ParalysisDifficultyClass;
    }

    internal static class Sprint19ReviewPolicy
    {
        internal static string[] Keys
        {
            get
            {
                return new[] { GirallonRulesPolicy.GirallonKey,
                    XillRulesPolicy.XillKey };
            }
        }

        /// <summary>
        /// Whether this creature is one of Sprint 19's two. Named so the
        /// earlier sprints' publication guards can exclude it the same way
        /// they already exclude Sprint 17's snakes and Sprint 18's apes -
        /// by name, rather than by being relaxed.
        /// </summary>
        internal static bool IsSprint19Creature(string key)
        { return Keys.Contains(key, StringComparer.Ordinal); }

        /// <summary>
        /// Every way a Girallon attack sequence can end, and whether the rend
        /// is owed. The review drives each one and proves the emission.
        ///
        /// <para>The three-claw case is the one that matters most: the printed
        /// line needs all four, and a gate written for two claws would rend
        /// here. Sprint 18's two-claw table could not express it.</para>
        /// </summary>
        internal static string[][] RendCases
        {
            get
            {
                return new[] {
                    new[] { "four-claws-one-target", "qualifies" },
                    new[] { "three-claws-one-target", "no-rend" },
                    new[] { "two-claws-one-target", "no-rend" },
                    new[] { "one-claw-only", "no-rend" },
                    new[] { "four-claws-split-targets", "no-rend" },
                    new[] { "bite-and-three-claws", "no-rend" },
                    new[] { "claws-across-two-commands", "no-rend" },
                    new[] { "claws-across-two-turns", "no-rend" },
                    new[] { "second-sequence-after-a-rend", "qualifies-once-more" },
                };
            }
        }

        /// <summary>
        /// The review runs each routine in real time and in turn-based
        /// combat: a rend that only works in one mode is a defect, and so is a
        /// routine that loses a limb when the mode changes.
        /// </summary>
        internal static bool[] CombatModes { get { return new[] { false, true }; } }

        /// <summary>
        /// Whether a rend case can be driven in this combat mode.
        ///
        /// <para>Exactly one cannot. claws-across-two-turns needs its two
        /// sequences to fall either side of a round boundary, and in real
        /// time the engine merges a second attack on one target into the live
        /// command, so they are one command and the case cannot differ from
        /// claws-across-two-commands. Forcing them apart needs a whole
        /// six-second idle round, through which the engine ends combat and
        /// despawns the target's view. The behaviour is proved in turn-based
        /// combat, where a round boundary is a real observable thing, and the
        /// real-time row records that rather than going quiet.</para>
        /// </summary>
        internal static bool RendCaseRunsInThisMode(string name, bool turnBased)
        { return turnBased || name != "claws-across-two-turns"; }

        internal static Sprint19LiveProfile For(string key)
        {
            if (key == GirallonRulesPolicy.GirallonKey)
                return new Sprint19LiveProfile {
                    Key = key,
                    Strength = 19, Dexterity = 17, Constitution = 18,
                    Intelligence = 2, Wisdom = 12, Charisma = 7,
                    HitDice = 7, HitPoints = 73, ArmorClass = 18, Touch = 12,
                    FlatFooted = 15, Fortitude = 9, Reflex = 8, Will = 5,
                    CombatManeuverDefense = 25,
                    // One primary 1d6 bite, then four primary 1d4 claws, all
                    // five at +10 and all five at the whole Strength modifier.
                    Limbs = new[] {
                        new[] { 1, 6, 4, 10 },
                        new[] { 1, 4, 4, 10 }, new[] { 1, 4, 4, 10 },
                        new[] { 1, 4, 4, 10 }, new[] { 1, 4, 4, 10 } },
                    Skills = new Dictionary<string, int>(StringComparer.Ordinal) {
                        { "Perception", 11 }, { "Stealth", 5 } },
                    HasRend = true, RendRolls = 1, RendDieSides = 4,
                    RendBonus = 6, RendClawCount = 4,
                };
            if (key == XillRulesPolicy.XillKey)
                return new Sprint19LiveProfile {
                    Key = key,
                    Strength = 17, Dexterity = 18, Constitution = 14,
                    Intelligence = 15, Wisdom = 12, Charisma = 11,
                    HitDice = 9, HitPoints = 67, ArmorClass = 21, Touch = 14,
                    FlatFooted = 17, Fortitude = 8, Reflex = 10, Will = 6,
                    CombatManeuverDefense = 26,
                    // One primary 1d3 bite at +12, then four primary 1d4 claws
                    // at +13. The one-point gap is Weapon Focus (claw), which
                    // is exactly why it is granted as the feat rather than
                    // folded into a flat bonus that would reach the bite too.
                    Limbs = new[] {
                        new[] { 1, 3, 3, 12 },
                        new[] { 1, 4, 3, 13 }, new[] { 1, 4, 3, 13 },
                        new[] { 1, 4, 3, 13 }, new[] { 1, 4, 3, 13 } },
                    Skills = new Dictionary<string, int>(StringComparer.Ordinal) {
                        { "Mobility", 16 }, { "Stealth", 14 },
                        { "Perception", 13 }, { "Persuasion", 12 },
                        { "KnowledgeArcana", 14 } },
                    HasRend = false,
                    SpellResistance = 17,
                    GrappleBonus = 16,
                    ParalysisDifficultyClass = 16,
                };
            throw new ArgumentOutOfRangeException("key", key,
                "Sprint 19 reviews exactly the Girallon and the Xill.");
        }

        /// <summary>
        /// The live routine, as a count of attacks in one full attack. Both
        /// creatures have five: a bite and four claws.
        /// </summary>
        internal static int ExpectedAttacksInAFullAttack(string key)
        { return For(key).Limbs.Length; }

        internal static void Validate()
        {
            foreach (string key in Keys)
            {
                Sprint19LiveProfile live = For(key);
                if (live.Key != key)
                    throw new InvalidOperationException(
                        "A review expectation is filed under the wrong key: " + key);
                // Five limbs on both creatures, every one well formed, and
                // every one carrying the whole Strength modifier - which is
                // what both stat blocks print and what the engine does not do
                // on its own.
                int strengthModifier = (live.Strength - 10) / 2;
                if (live.Limbs.Length != 5 || live.Limbs.Any(limb =>
                        limb.Length != 4 || limb[0] <= 0 || limb[1] <= 0 ||
                        limb[2] != strengthModifier || limb[3] <= 0))
                    throw new InvalidOperationException(
                        "A limb is malformed or does not carry full Strength: " + key);
                // Four of the five limbs are claws, which is what both printed
                // routines say and what the Girallon's rend counts.
                if (live.Limbs.Skip(1).Count() != 4)
                    throw new InvalidOperationException(
                        "Both Sprint 19 creatures print four claws: " + key);
                if (live.Skills.Count == 0 || live.Skills.Values.Any(value => value <= 0))
                    throw new InvalidOperationException(
                        "A printed skill total is missing or not positive: " + key);
                ValidateGirallon(key, live, strengthModifier);
                ValidateXill(key, live, strengthModifier);
            }
            string[] names = RendCases.Select(row => row[0]).ToArray();
            if (names.Distinct(StringComparer.Ordinal).Count() != names.Length ||
                    RendCases.Count(row => row[1] == "no-rend") != 7 ||
                    RendCases.Count(row => row[1].StartsWith("qualifies",
                        StringComparison.Ordinal)) != 2)
                throw new InvalidOperationException(
                    "The rend case table must keep its seven non-qualifying cases.");
            // The case that a two-claw gate would get wrong. Sprint 19 exists
            // partly to prove this one, so it is not allowed to go missing.
            if (!RendCases.Any(row => row[0] == "three-claws-one-target" &&
                    row[1] == "no-rend"))
                throw new InvalidOperationException(
                    "Three of four claws must be proved not to rend.");
            if (CombatModes.Length != 2 || CombatModes[0] == CombatModes[1])
                throw new InvalidOperationException(
                    "Both combat modes must be reviewed.");
            // Exactly one case is mode-limited, and only the one the engine's
            // command merging makes meaningless in real time.
            string[] limited = RendCases.Select(row => row[0])
                .Where(name => !RendCaseRunsInThisMode(name, false)).ToArray();
            if (limited.Length != 1 || limited[0] != "claws-across-two-turns")
                throw new InvalidOperationException(
                    "Only the round-boundary case may be turn-based only.");
            if (RendCases.Any(row => !RendCaseRunsInThisMode(row[0], true)))
                throw new InvalidOperationException(
                    "Every rend case must run in turn-based combat.");
        }

        private static void ValidateGirallon(string key,
            Sprint19LiveProfile live, int strengthModifier)
        {
            if (key != GirallonRulesPolicy.GirallonKey) return;
            if (live.Strength != GirallonRulesPolicy.Strength ||
                live.HitDice != GirallonRulesPolicy.HitDice ||
                live.HitPoints != GirallonRulesPolicy.PrintedHitPoints ||
                live.ArmorClass != GirallonRulesPolicy.PrintedArmorClass ||
                live.Touch != GirallonRulesPolicy.PrintedTouchArmorClass ||
                live.FlatFooted !=
                    GirallonRulesPolicy.PrintedFlatFootedArmorClass ||
                live.Fortitude != GirallonRulesPolicy.PrintedFortitude ||
                live.Reflex != GirallonRulesPolicy.PrintedReflex ||
                live.Will != GirallonRulesPolicy.PrintedWill ||
                live.CombatManeuverDefense !=
                    GirallonRulesPolicy.PrintedCombatManeuverDefense)
                throw new InvalidOperationException(
                    "The review expectation and the Girallon rules policy disagree.");
            // A magical beast has full base attack, so the bonus is hit dice
            // plus Strength less one for Large size - not the three-quarters
            // an animal gets, which is what Sprint 18's apes used.
            if (live.Limbs.Any(limb => limb[3] !=
                    live.HitDice + strengthModifier - 1))
                throw new InvalidOperationException(
                    "The Girallon attack bonus is not full base attack plus "
                    + "Strength less one for Large size.");
            if (!live.HasRend || live.RendClawCount != GirallonRulesPolicy.ClawCount ||
                live.RendRolls != GirallonRulesPolicy.RendDiceCount ||
                live.RendDieSides != GirallonRulesPolicy.RendDieSides ||
                live.RendBonus != GirallonRulesPolicy.RendBonus)
                throw new InvalidOperationException(
                    "The expected Girallon rend is not the printed one.");
            if (live.Skills["Perception"] !=
                    GirallonRulesPolicy.PrintedPerceptionSkill ||
                live.Skills["Stealth"] != GirallonRulesPolicy.PrintedStealthSkill)
                throw new InvalidOperationException(
                    "The expected Girallon skills are not the printed ones.");
            // No Climb is expected, because none is represented. A review that
            // expected one would pass on a creature that had quietly gained a
            // substitute.
            if (live.Skills.ContainsKey("Athletics") ||
                live.Skills.ContainsKey("Climb") ||
                live.Skills.ContainsKey("Mobility"))
                throw new InvalidOperationException(
                    "The Girallon prints no Climb this project represents and "
                    + "nothing is substituted for it.");
            if (live.SpellResistance != 0 || live.GrappleBonus != 0 ||
                live.ParalysisDifficultyClass != 0)
                throw new InvalidOperationException(
                    "The Girallon prints no spell resistance, grab or paralysis.");
        }

        private static void ValidateXill(string key,
            Sprint19LiveProfile live, int strengthModifier)
        {
            if (key != XillRulesPolicy.XillKey) return;
            if (live.Strength != XillRulesPolicy.Strength ||
                live.HitDice != XillRulesPolicy.HitDice ||
                live.HitPoints != XillRulesPolicy.PrintedHitPoints ||
                live.ArmorClass != XillRulesPolicy.PrintedArmorClass ||
                live.Touch != XillRulesPolicy.PrintedTouchArmorClass ||
                live.FlatFooted != XillRulesPolicy.PrintedFlatFootedArmorClass ||
                live.Fortitude != XillRulesPolicy.PrintedFortitude ||
                live.Reflex != XillRulesPolicy.PrintedReflex ||
                live.Will != XillRulesPolicy.PrintedWill ||
                live.CombatManeuverDefense !=
                    XillRulesPolicy.PrintedCombatManeuverDefense ||
                live.SpellResistance != XillRulesPolicy.PrintedSpellResistance ||
                live.GrappleBonus != XillRulesPolicy.PrintedGrappleBonus ||
                live.ParalysisDifficultyClass !=
                    XillRulesPolicy.PrintedParalysisDifficultyClass)
                throw new InvalidOperationException(
                    "The review expectation and the Xill rules policy disagree.");
            // An outsider has full base attack, and Medium costs nothing. The
            // bite is base attack plus Strength; each claw is one more,
            // because Weapon Focus (claw) reaches the claws alone.
            if (live.Limbs[0][3] != live.HitDice + strengthModifier)
                throw new InvalidOperationException(
                    "The Xill bite is not full base attack plus Strength.");
            if (live.Limbs.Skip(1).Any(limb => limb[3] !=
                    live.HitDice + strengthModifier +
                    XillRulesPolicy.WeaponFocusBonus))
                throw new InvalidOperationException(
                    "The Xill claws do not carry Weapon Focus.");
            if (live.Limbs[0][3] + XillRulesPolicy.WeaponFocusBonus !=
                    live.Limbs[1][3])
                throw new InvalidOperationException(
                    "The Xill claw and bite must differ by exactly the "
                    + "Weapon Focus bonus.");
            if (live.HasRend || live.RendClawCount != 0)
                throw new InvalidOperationException(
                    "The Xill prints no rend.");
            if (live.Skills["Mobility"] != XillRulesPolicy.PrintedAcrobaticsSkill ||
                live.Skills["Stealth"] != XillRulesPolicy.PrintedStealthSkill ||
                live.Skills["Perception"] != XillRulesPolicy.PrintedPerceptionSkill ||
                live.Skills["Persuasion"] != XillRulesPolicy.PrintedBluffSkill ||
                live.Skills["KnowledgeArcana"] !=
                    XillRulesPolicy.PrintedKnowledgeArcanaSkill)
                throw new InvalidOperationException(
                    "The expected Xill skills are not the printed ones.");
            // Five Kingmaker skills for eight printed ones. The review expects
            // five and no more: a sixth would mean a merged skill had been
            // given a home of its own, and a stacked total would mean two
            // printed skills had been added together.
            if (live.Skills.Count != 5)
                throw new InvalidOperationException(
                    "The Xill maps eight printed skills onto exactly five "
                    + "Kingmaker skills, with three recorded as merged.");
        }
    }
}
