using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Sprint 38 Glabrezu contract. Printed values live here once. Nothing
    /// that the engine can derive from live stats is hard-coded as a result:
    /// the spell-like difficulty classes come from the live Charisma, the
    /// rend bonus comes from the live Strength through the engine's own
    /// <c>RendFeature</c>, and the printed totals below exist only so an
    /// observer can compare like with like.
    /// </summary>
    internal static class GlabrezuRules38Policy
    {
        internal const int GlabrezuHitDice = 12;

        // Spell-like caster level 14 is deliberately NOT the hit dice. The
        // engine's RuleCalculateAbilityParams derives a level from the unit
        // and will report 12; the project's ContextCalculateAbilityParams
        // puts 14 on the applied effect's own context. Both are recorded in
        // receipts and neither is a defect.
        internal const int SpellLikeCasterLevel = 14;

        // Printed ability scores.
        internal const int Strength = 31;
        internal const int Dexterity = 11;
        internal const int Constitution = 31;
        internal const int Intelligence = 16;
        internal const int Wisdom = 16;
        internal const int Charisma = 20;
        internal const int CharismaBonus = 5;

        internal const int NaturalArmor = 20;
        internal const int SpellResistance = 24;
        internal const int HitPoints = 186;
        internal const int SpeedFeet = 40;

        // Huge: a 15-foot space and a 15-foot reach. Both printed.
        internal const int ReachFeet = 15;

        // Printed save totals, retained only for comparison. The blueprint
        // carries the base values; genuine native difficulty modifiers stay a
        // separate finding and are never compensated for here.
        internal const int PrintedFortitude = 18;
        internal const int PrintedReflex = 4;
        internal const int PrintedWill = 11;
        internal const int BaseFortitude = 8;
        internal const int BaseWill = 8;

        // The printed Reflex +4 does not follow from the chassis. A 12-HD
        // outsider has three good saves, which gives a +8 base, and Dexterity
        // 11 adds nothing - so the derivation gives +8 while Fortitude and
        // Will both match their printed totals exactly. The printed value is
        // carried as printed, because the source entry is this project's
        // authority and silently "fixing" a published stat block is not a
        // Sprint 38 decision to take unilaterally. Recorded in
        // planning/SPRINT38-RULES.md rather than hidden here.
        internal const int BaseReflex = 4;
        internal const int DerivedGoodSaveBase = 8;

        internal const int PrintedPerception = 26;
        internal const int PrintedStealth = 7;
        internal const int SkillRanks = 12;
        internal const int RacialPerceptionBonus = 8;
        internal const int RacialBluffBonus = 8;

        // Three exact natural weapons in five limbs. The pincer's printed
        // 19-20 threat range is the weapon's own critical roll edge: the
        // engine's BlueprintWeaponType defaults the edge to 20 and the sheet
        // renders the width as 21 - edge, so 19 is exactly a 19-20 range.
        // This is how printed Improved Critical (pincer) is delivered, because
        // the installed game has no pincer weapon category and therefore no
        // native Improved Critical (pincer) feature could exist.
        internal const int PincerDiceCount = 2;
        internal const int PincerDieSides = 8;
        internal const int PincerCriticalRollEdge = 19;
        internal const int ClawDiceCount = 1;
        internal const int ClawDieSides = 6;
        internal const int BiteDiceCount = 1;
        internal const int BiteDieSides = 8;

        // Printed rend: 2 pincers, 2d8+15. The +15 is one and a half times
        // the live Strength bonus and is computed by the engine's own
        // RendFeature, so only the dice are pinned.
        internal const int RendDiceCount = 2;
        internal const int RendDieSides = 8;
        internal const int RendPincerCount = 2;

        // Power Word Stun. Printed: enchantment (compulsion) [mind-affecting],
        // standard action, close range, one creature with 150 hp or less, no
        // saving throw, spell resistance yes, 1/day.
        internal const int PowerWordStunSpellLevel = 8;
        internal const int PowerWordStunUses = 1;
        internal const int PowerWordStunHitPointCeiling = 150;
        internal const int PowerWordStunLowThreshold = 50;
        internal const int PowerWordStunMidThreshold = 100;
        internal const int PowerWordStunLowDice = 4;
        internal const int PowerWordStunMidDice = 2;
        internal const int PowerWordStunHighDice = 1;
        internal const int PowerWordStunDieSides = 4;

        // At-will spell-like levels, used only to derive their difficulty
        // classes and to label the cloned graphs.
        internal const int ChaosHammerSpellLevel = 4;
        internal const int UnholyBlightSpellLevel = 4;
        internal const int ConfusionSpellLevel = 4;
        internal const int DispelMagicSpellLevel = 3;
        internal const int MirrorImageSpellLevel = 2;

        // Printed difficulty classes, for comparison against the live values.
        internal const int PrintedChaosHammerDc = 19;
        internal const int PrintedConfusionDc = 19;

        /// <summary>
        /// A spell-like ability's difficulty class: ten plus the spell level
        /// plus the caster's Charisma bonus. This single formula reproduces
        /// every difficulty class the printed entry states - chaos hammer and
        /// confusion at spell level 4 give 19, and reverse gravity at spell
        /// level 7 gave the printed 22 before that ability was omitted - so no
        /// difficulty class is pinned as a constant anywhere in the
        /// implementation.
        /// </summary>
        internal static int SpellDifficultyClass(int spellLevel,
            int charismaBonus)
        {
            return 10 + spellLevel + charismaBonus;
        }

        /// <summary>
        /// How many rounds Power Word Stun lasts against a recipient at a
        /// given <paramref name="currentHitPoints"/>, as a count of d4, or
        /// zero when the recipient is unaffected.
        ///
        /// <para>The printed spell reads the recipient's <b>current</b> hit
        /// points, not its maximum, and it is resolved at that moment: 50 or
        /// fewer gives 4d4 rounds, 51 to 100 gives 2d4, 101 to 150 gives 1d4,
        /// and 151 or more is unaffected entirely. There is no saving throw at
        /// any threshold, so a creature only escapes by being above the
        /// ceiling, by native immunity, or through spell resistance.</para>
        /// </summary>
        internal static int PowerWordStunDiceCount(int currentHitPoints)
        {
            if (currentHitPoints > PowerWordStunHitPointCeiling) return 0;
            if (currentHitPoints <= PowerWordStunLowThreshold)
                return PowerWordStunLowDice;
            if (currentHitPoints <= PowerWordStunMidThreshold)
                return PowerWordStunMidDice;
            return PowerWordStunHighDice;
        }

        /// <summary>
        /// Whether Power Word Stun reaches one recipient. The printed ability
        /// has no saving throw, so the only gates are the exact source, a live
        /// recipient, the printed hit-point ceiling, and the two native
        /// defences the engine owns: condition immunity and spell resistance.
        /// Neither of those is pre-filtered away from its own consumer here;
        /// this predicate is the decision in testable form.
        /// </summary>
        internal static bool PowerWordStunReaches(bool exactSource,
            bool liveTarget, int currentHitPoints, bool immune,
            bool resisted)
        {
            return exactSource && liveTarget && !immune && !resisted &&
                PowerWordStunDiceCount(currentHitPoints) > 0;
        }

        /// <summary>
        /// Whether two pincer hits in one sequence make a rend. Claws and the
        /// bite carry a different weapon identity and can never substitute,
        /// and a sequence rends at most once - the universal once-per-round
        /// limit, which the released gate enforces by resetting on the round
        /// boundary as well as on the command boundary.
        /// </summary>
        internal static bool RendTriggers(bool exactSource, int pincerHits,
            bool sameVictim, bool alreadyRentThisRound)
        {
            return exactSource && sameVictim && !alreadyRentThisRound &&
                pincerHits >= RendPincerCount;
        }

        internal static void Validate()
        {
            if (SpellDifficultyClass(ChaosHammerSpellLevel, CharismaBonus)
                    != PrintedChaosHammerDc ||
                SpellDifficultyClass(ConfusionSpellLevel, CharismaBonus)
                    != PrintedConfusionDc ||
                // Reverse gravity is omitted by owner decision, but the same
                // formula reproduced its printed DC 22 at spell level 7. The
                // check is retained because it is what establishes that the
                // formula - and not a table of constants - is the source of
                // every printed difficulty class.
                SpellDifficultyClass(7, CharismaBonus) != 22 ||
                (Charisma - 10) / 2 != CharismaBonus)
                throw new InvalidOperationException(
                    "Sprint 38 Glabrezu spell-like difficulty derivation changed.");

            // Power Word Stun thresholds, at and either side of every printed
            // boundary. 150 is the last affected value and 151 the first
            // unaffected one.
            if (PowerWordStunDiceCount(1) != PowerWordStunLowDice ||
                PowerWordStunDiceCount(50) != PowerWordStunLowDice ||
                PowerWordStunDiceCount(51) != PowerWordStunMidDice ||
                PowerWordStunDiceCount(100) != PowerWordStunMidDice ||
                PowerWordStunDiceCount(101) != PowerWordStunHighDice ||
                PowerWordStunDiceCount(150) != PowerWordStunHighDice ||
                PowerWordStunDiceCount(151) != 0 ||
                PowerWordStunDiceCount(999) != 0 ||
                PowerWordStunDieSides != 4 || PowerWordStunUses != 1 ||
                PowerWordStunSpellLevel != 8)
                throw new InvalidOperationException(
                    "Sprint 38 Power Word Stun thresholds changed.");

            if (!PowerWordStunReaches(true, true, 50, false, false) ||
                PowerWordStunReaches(false, true, 50, false, false) ||
                PowerWordStunReaches(true, false, 50, false, false) ||
                PowerWordStunReaches(true, true, 50, true, false) ||
                PowerWordStunReaches(true, true, 50, false, true) ||
                PowerWordStunReaches(true, true, 151, false, false))
                throw new InvalidOperationException(
                    "Sprint 38 Power Word Stun eligibility changed.");

            if (!RendTriggers(true, 2, true, false) ||
                RendTriggers(true, 1, true, false) ||
                RendTriggers(true, 2, false, false) ||
                RendTriggers(true, 2, true, true) ||
                RendTriggers(false, 2, true, false) ||
                RendDiceCount != 2 || RendDieSides != 8 ||
                RendPincerCount != 2)
                throw new InvalidOperationException(
                    "Sprint 38 Glabrezu rend contract changed.");

            // Printed save totals. Fortitude and Will derive exactly; Reflex
            // does not, and the mismatch against the derived good-save base is
            // asserted so the anomaly cannot be quietly removed later.
            if (BaseFortitude + (Constitution - 10) / 2 != PrintedFortitude ||
                BaseWill + (Wisdom - 10) / 2 != PrintedWill ||
                BaseReflex + (Dexterity - 10) / 2 != PrintedReflex ||
                BaseFortitude != DerivedGoodSaveBase ||
                BaseWill != DerivedGoodSaveBase ||
                BaseReflex == DerivedGoodSaveBase)
                throw new InvalidOperationException(
                    "Sprint 38 Glabrezu save contract changed.");

            // Perception: ranks + Wisdom + class skill + racial.
            // Stealth: ranks + class skill + Dexterity - the Huge size penalty.
            if (SkillRanks + (Wisdom - 10) / 2 + 3 + RacialPerceptionBonus
                    != PrintedPerception ||
                SkillRanks + 3 + (Dexterity - 10) / 2 - 8 != PrintedStealth ||
                SkillRanks != GlabrezuHitDice)
                throw new InvalidOperationException(
                    "Sprint 38 Glabrezu skill derivation changed.");

            if (NaturalArmor != 20 || SpellResistance != 24 ||
                HitPoints != 186 || GlabrezuHitDice != 12 ||
                SpeedFeet != 40 || ReachFeet != 15 ||
                SpellLikeCasterLevel != 14 ||
                SpellLikeCasterLevel == GlabrezuHitDice ||
                PincerDiceCount != 2 || PincerDieSides != 8 ||
                PincerCriticalRollEdge != 19 ||
                ClawDiceCount != 1 || ClawDieSides != 6 ||
                BiteDiceCount != 1 || BiteDieSides != 8 ||
                RacialBluffBonus != 8)
                throw new InvalidOperationException(
                    "Sprint 38 Glabrezu rules contract changed.");

            // 12d10 averaging 66 plus 10 per die from Constitution 31.
            if (GlabrezuHitDice * 10 / 2 + GlabrezuHitDice / 2 +
                GlabrezuHitDice * ((Constitution - 10) / 2) != HitPoints)
                throw new InvalidOperationException(
                    "Sprint 38 Glabrezu hit points no longer follow 12d10+120.");
        }
    }
}
