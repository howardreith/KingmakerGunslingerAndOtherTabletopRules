using System;
using System.Collections.Generic;

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

        // Spell-like caster level 14 is deliberately NOT the hit dice, and
        // the installed engine applies it in two different places with two
        // different answers. Traced rather than assumed:
        //
        //  * RuleCalculateAbilityParams..ctor does
        //    GetComponent<ContextSetAbilityParams>() - the DERIVED type, not
        //    the ContextAbilityParamsCalculator base - so it does not see
        //    this project's ContextCalculateAbilityParams at all and the
        //    pre-cast estimate reports the creature's 12 hit dice. That is
        //    the preview value only.
        //  * MechanicsContext.RecalculateAbilityParams does
        //    GetComponent<ContextAbilityParamsCalculator>() - the base type -
        //    so it does honour it, and AbilityExecutionProcess.PrepareCast
        //    calls context.Recalculate() before the effect is applied. So the
        //    execution context carries 14, and everything downstream of it
        //    does too: the applied effect, and the spell-resistance check,
        //    whose SpellPenetration is Context.Params.CasterLevel.
        //
        // Consequence actually followed through: penetration uses 14, which
        // is what the printed caster level requires, and only the preview
        // shows 12.
        //
        // Range is a third, separate fact. The installed AbilityRange.Close
        // CATEGORY is a flat 30 feet that never reads a caster level, so the
        // printed 25 ft. + 5 ft./2 levels scaling does not exist in that
        // category. That is a reason not to use the category, not a reason to
        // halve this creature's printed reach: the ability ships with
        // AbilityRange.Custom and an explicit CustomRange derived from its own
        // spell-like caster level. That is the mechanism the released scatter,
        // web, ray, Infernal34 and Breeze-Kissed abilities already use, and
        // BlueprintAbility.GetRange returns CustomRange verbatim, so player
        // targeting, approach, the cast and the AI all share one distance. No
        // other consumer of Close is touched.
        internal const int SpellLikeCasterLevel = 14;
        internal const int PreCastEstimatedCasterLevel = GlabrezuHitDice;
        // What the installed Close CATEGORY would have given, recorded only to
        // explain why this ability does not use it.
        internal const int InstalledCloseCategoryFeet = 30;
        // The printed close range at this creature's spell-like caster level,
        // and the range the ability actually ships with.
        internal const int SelectedCloseRangeFeet = 60;

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

        // Printed save totals, and the base values the blueprint carries.
        // Genuine native difficulty modifiers stay a separate finding and are
        // never compensated for here.
        internal const int PrintedFortitude = 18;
        internal const int PrintedReflex = 4;
        internal const int PrintedWill = 11;
        internal const int BaseFortitude = 8;
        internal const int BaseReflex = 4;
        internal const int BaseWill = 8;

        // All three printed totals derive exactly, and nothing about them is
        // anomalous. The outsider type gives a creature TWO good saves, not
        // three, and which two is per-creature; this entry's are Fortitude
        // and Will, with Reflex poor. On the standard progressions at 12 hit
        // dice that is a good base of 2 + 12/2 = 8 and a poor base of
        // 12/3 = 4, so:
        //   Fortitude 8 + Constitution 10 = 18
        //   Reflex    4 + Dexterity     0 =  4
        //   Will      8 + Wisdom        3 = 11
        // An earlier revision of this file claimed outsiders have three good
        // saves and therefore recorded the printed Reflex as a published
        // stat-block anomaly. That premise was wrong; the value was always
        // right. The exact racial-profile mapping below expresses this
        // creature's own good/poor selection and is not a balance adaptation.
        internal const int GoodSaveBase = 8;
        internal const int PoorSaveBase = 4;

        internal const int PrintedPerception = 26;
        internal const int PrintedStealth = 7;
        internal const int SkillRanks = 12;
        internal const int RacialPerceptionBonus = 8;
        /// <summary>
        /// The installed native Persuasive feat's own Perception bonus,
        /// measured on the live unit rather than assumed. In this game that
        /// feat adds +2 to PERCEPTION as well as to Persuasion, and this
        /// creature's printed feat list includes Persuasive, so the live
        /// Perception reads two above the printed +26. The printed line was
        /// computed without that implementation; nothing is subtracted to
        /// match it, and the difference is disclosed instead.
        /// </summary>
        internal const int NativePersuasivePerceptionBonus = 2;
        internal const string NativePersuasiveFeatGuid =
            "86d93a5891d299d4983bdc6ef3987afd";
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
        // Unholy blight is also a fourth-level spell, so the same derivation
        // reaches the same printed 19. It is named separately rather than
        // shared with chaos hammer's constant: if either printed spell level
        // were ever read differently, one shared number would hide it.
        internal const int PrintedUnholyBlightDc = 19;

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
        /// The printed close range at a given caster level: 25 feet plus 5
        /// feet per two caster levels. Derived here so the ability's
        /// configured distance follows its own selected spell-like caster
        /// level rather than being written down a second time.
        /// </summary>
        internal static int CloseRangeFeet(int casterLevel)
        {
            return 25 + 5 * (casterLevel / 2);
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
        /// Whether this pincer attack is the rend.
        ///
        /// <para>The printed universal rule scopes both halves to the
        /// <b>round</b>: two of the named attacks hitting the same opponent in
        /// the same round, and usable only once per round. Neither half is
        /// scoped to one attack command, so this predicate is not either.
        /// That matters in both directions. A rend is allowed when the two
        /// qualifying pincer hits land in different commands inside one round
        /// - an attack of opportunity and then a full attack, say - and it is
        /// refused for the rest of that round once it has actually
        /// happened.</para>
        ///
        /// <para>The round allowance is deliberately a separate operand from
        /// the attack-pair bookkeeping, because the released shared tracker
        /// conflates them: its Follow() resets the whole tracker - emitted
        /// flag included - whenever the attack command changes, so a second
        /// qualifying pair in the same round would rend again. Keeping
        /// <paramref name="spentThisRound"/> outside that bookkeeping is what
        /// makes the printed limit real rather than incidental.</para>
        /// </summary>
        internal static bool RendQualifies(bool exactSource,
            bool hasLiveAttackCommand,
            bool everyOtherPincerHitThisVictimThisRound,
            bool alreadyArmed, bool spentThisRound)
        {
            return exactSource && hasLiveAttackCommand &&
                everyOtherPincerHitThisVictimThisRound && !alreadyArmed &&
                !spentThisRound;
        }

        /// <summary>
        /// The compact form, for a reader comparing the printed line against
        /// the implementation: this many qualifying pincer hits on one victim
        /// in one round, from the exact owner, once.
        /// </summary>
        internal static bool RendTriggers(bool exactSource, int pincerHits,
            bool sameVictim, bool alreadyRentThisRound)
        {
            return RendQualifies(exactSource, true,
                sameVictim && pincerHits >= RendPincerCount, false,
                alreadyRentThisRound);
        }

        /// <summary>
        /// Whether a completed pincer attack spends the round's one rend. Only
        /// an armed attack that actually hit does: a miss disarms and leaves
        /// the allowance intact, and an attack that was never armed cannot
        /// spend it, so repeating one event grants nothing.
        /// </summary>
        internal static bool RendSpends(bool wasArmed, bool hit)
        { return wasArmed && hit; }

        internal static void Validate()
        {
            if (SpellDifficultyClass(ChaosHammerSpellLevel, CharismaBonus)
                    != PrintedChaosHammerDc ||
                SpellDifficultyClass(ConfusionSpellLevel, CharismaBonus)
                    != PrintedConfusionDc ||
                SpellDifficultyClass(UnholyBlightSpellLevel, CharismaBonus)
                    != PrintedUnholyBlightDc ||
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

            // The round allowance is its own operand, so a command change -
            // which is not one of these operands at all - cannot refill it.
            if (!RendQualifies(true, true, true, false, false) ||
                RendQualifies(false, true, true, false, false) ||
                RendQualifies(true, false, true, false, false) ||
                RendQualifies(true, true, false, false, false) ||
                RendQualifies(true, true, true, true, false) ||
                RendQualifies(true, true, true, false, true) ||
                !RendSpends(true, true) || RendSpends(true, false) ||
                RendSpends(false, true) || RendSpends(false, false))
                throw new InvalidOperationException(
                    "Sprint 38 Glabrezu round-level rend allowance changed.");

            // Every printed save total derives from the two-good-save outsider
            // contract: good Fortitude and Will, poor Reflex, on the standard
            // progressions at this creature's hit dice.
            if (BaseFortitude + (Constitution - 10) / 2 != PrintedFortitude ||
                BaseWill + (Wisdom - 10) / 2 != PrintedWill ||
                BaseReflex + (Dexterity - 10) / 2 != PrintedReflex ||
                GoodSaveBase != 2 + GlabrezuHitDice / 2 ||
                PoorSaveBase != GlabrezuHitDice / 3 ||
                BaseFortitude != GoodSaveBase ||
                BaseWill != GoodSaveBase ||
                BaseReflex != PoorSaveBase ||
                // Exactly two good saves, so the three base values are not
                // all equal and the poor one is the Reflex.
                GoodSaveBase == PoorSaveBase)
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
                PreCastEstimatedCasterLevel != GlabrezuHitDice ||
                PreCastEstimatedCasterLevel == SpellLikeCasterLevel ||
                InstalledCloseCategoryFeet != 30 ||
                SelectedCloseRangeFeet !=
                    CloseRangeFeet(SpellLikeCasterLevel) ||
                CloseRangeFeet(14) != 60 || CloseRangeFeet(12) != 55 ||
                CloseRangeFeet(1) != 25 ||
                // The ability ships at the printed range, so the shorter
                // category distance must NOT be what it uses.
                SelectedCloseRangeFeet == InstalledCloseCategoryFeet ||
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

    /// <summary>
    /// One Glabrezu's rend state for the current round.
    ///
    /// <para>The victim slots and the round allowance are separate on purpose.
    /// The released <see cref="ClawRendTracker"/> conflates them: its
    /// <c>Follow()</c> calls <c>Reset()</c> whenever the attack command
    /// changes, clearing the emitted flag along with the claw bookkeeping, so
    /// its limit is genuinely per command and a second qualifying pair in the
    /// same round rends again. Here an attack command is not an input at all,
    /// so nothing a command does can refill the allowance; only
    /// <see cref="BeginRound"/> does, and only the installed round boundary
    /// calls it.</para>
    ///
    /// <para>Deliberately dependency-free, like the rest of this file, so the
    /// domain tests exercise this exact production type rather than a copy of
    /// its rules. The live component owns one of these per unit.</para>
    /// </summary>
    internal sealed class GlabrezuRendRound38
    {
        /// <summary>
        /// Which of this creature's named attacks have hit one victim so far
        /// this round.
        /// </summary>
        private sealed class VictimHits
        {
            internal VictimHits(object victim, int pincerCount)
            {
                Victim = victim;
                Limbs = new bool[pincerCount];
            }

            internal readonly object Victim;
            internal readonly bool[] Limbs;
        }

        private readonly List<VictimHits> _victims = new List<VictimHits>();
        private readonly int _pincerCount;
        private long _stamp = long.MinValue;

        internal GlabrezuRendRound38(int pincerCount)
        {
            if (pincerCount < 2)
                throw new ArgumentOutOfRangeException("pincerCount",
                    "A printed rend needs at least two named attacks.");
            _pincerCount = pincerCount;
        }

        internal int PincerCount { get { return _pincerCount; } }
        internal bool Armed { get; private set; }
        internal bool SpentThisRound { get; private set; }

        /// <summary>
        /// The round this state belongs to, or <see cref="long.MinValue"/>
        /// before one has been stamped.
        /// </summary>
        internal long RoundStamp { get { return _stamp; } }

        /// <summary>
        /// Begins a new round when, and only when, the caller's round stamp
        /// differs from the one this state carries. Returns whether it reset.
        ///
        /// <para>This is what makes the printed once-per-round limit real
        /// rather than dependent on a callback that does not arrive. The
        /// engine dispatches <c>ITickEachRound</c> through the unit's LOGIC
        /// fact collection - both the base <c>UnitTicksController</c> and the
        /// turn-based <c>TurnController</c> call
        /// <c>unit.Logic.CallFactComponents</c> - so a component carried by a
        /// FEATURE never receives it. A guarded run proved the consequence
        /// exactly: the owner sat in real combat for 975 unpaused frames with
        /// its rend spent and the allowance never came back.</para>
        ///
        /// <para>The stamp itself is supplied by the caller, so this class
        /// stays dependency-free and the domain tests drive the real
        /// production type. The gate derives it from game time, which advances
        /// in both real-time and turn-based play.</para>
        /// </summary>
        internal bool BeginRoundIfChanged(long stamp)
        {
            if (_stamp == stamp) return false;
            BeginRound();
            _stamp = stamp;
            return true;
        }

        /// <summary>A new round returns the allowance and forgets every pair.</summary>
        internal void BeginRound()
        {
            _victims.Clear();
            Armed = false;
            SpentThisRound = false;
        }

        /// <summary>
        /// Whether every pincer but this one has already hit this exact victim
        /// in this round.
        ///
        /// <para>The hits are remembered <b>per victim</b>, not per limb. An
        /// earlier shape kept one slot per pincer holding its most recent
        /// victim, which silently lost a qualifying hit whenever a pincer
        /// moved on: pincer one hits A, pincer one then hits B, pincer two
        /// hits A - the printed rule says two pincers hit the same opponent in
        /// the same round, and they did, but A had already been overwritten by
        /// B and the rend was refused. The printed rule is about the victim,
        /// so the bookkeeping is too.</para>
        /// </summary>
        internal bool EveryOtherPincerHit(int pincer, object victim)
        {
            VictimHits hits = Find(victim);
            if (hits == null) return false;
            for (int index = 0; index < _pincerCount; index++)
            {
                if (index == pincer - 1) continue;
                if (!hits.Limbs[index]) return false;
            }
            return true;
        }

        internal bool TryArm(bool exactSource, bool hasLiveAttackCommand,
            int pincer, object victim)
        {
            if (victim == null || !Holds(pincer)) return false;
            bool qualifies = GlabrezuRules38Policy.RendQualifies(exactSource,
                hasLiveAttackCommand, EveryOtherPincerHit(pincer, victim),
                Armed, SpentThisRound);
            if (qualifies) Armed = true;
            return qualifies;
        }

        /// <summary>
        /// What the pincer attack actually did. A miss disarms and leaves the
        /// round's allowance intact; an armed hit is the one rend this round
        /// gets and spends it. An attack that was never armed cannot spend it,
        /// and recording the same limb against the same victim twice sets a
        /// flag that is already set, so a replayed callback is not a second
        /// successful attack.
        /// </summary>
        internal void RecordOutcome(int pincer, object victim, bool hit)
        {
            if (!Holds(pincer)) return;
            bool wasArmed = Armed;
            Armed = false;
            if (GlabrezuRules38Policy.RendSpends(wasArmed, hit))
            {
                SpentThisRound = true;
                return;
            }
            if (wasArmed || !hit || victim == null) return;
            Remember(victim).Limbs[pincer - 1] = true;
        }

        /// <summary>For the guarded fixture: which limbs have hit this victim.</summary>
        internal bool HasHit(int pincer, object victim)
        {
            VictimHits hits = Holds(pincer) ? Find(victim) : null;
            return hits != null && hits.Limbs[pincer - 1];
        }

        /// <summary>For the guarded fixture: how many victims this round touched.</summary>
        internal int TrackedVictimCount { get { return _victims.Count; } }

        private VictimHits Find(object victim)
        {
            if (victim == null) return null;
            for (int index = 0; index < _victims.Count; index++)
                if (ReferenceEquals(_victims[index].Victim, victim))
                    return _victims[index];
            return null;
        }

        private VictimHits Remember(object victim)
        {
            VictimHits hits = Find(victim);
            if (hits != null) return hits;
            hits = new VictimHits(victim, _pincerCount);
            _victims.Add(hits);
            return hits;
        }

        private bool Holds(int pincer)
        { return pincer >= 1 && pincer <= _pincerCount; }
    }
}
