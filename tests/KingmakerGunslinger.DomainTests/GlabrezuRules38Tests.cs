using System;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class GlabrezuRules38Tests
    {
        internal static void PrintedContractDerivesRatherThanPins()
        {
            GlabrezuRules38Policy.Validate();

            // Every printed spell-like difficulty class comes out of one
            // formula, so none of them is a constant in the implementation.
            Assertions.Equal(GlabrezuRules38Policy.PrintedChaosHammerDc,
                GlabrezuRules38Policy.SpellDifficultyClass(
                    GlabrezuRules38Policy.ChaosHammerSpellLevel,
                    GlabrezuRules38Policy.CharismaBonus),
                "Chaos hammer's printed DC derives from Charisma.");
            Assertions.Equal(22, GlabrezuRules38Policy.SpellDifficultyClass(7,
                    GlabrezuRules38Policy.CharismaBonus),
                "The same formula reproduced reverse gravity's printed DC before that ability was omitted.");

            // The outsider type gives TWO good saves, not three, and which two
            // is per-creature. This entry's are Fortitude and Will, with a
            // poor Reflex, and all three printed totals derive exactly - so
            // there is no published anomaly to carry.
            Assertions.Equal(2 + GlabrezuRules38Policy.GlabrezuHitDice / 2,
                GlabrezuRules38Policy.GoodSaveBase,
                "A good save base is 2 + half the hit dice.");
            Assertions.Equal(GlabrezuRules38Policy.GlabrezuHitDice / 3,
                GlabrezuRules38Policy.PoorSaveBase,
                "A poor save base is a third of the hit dice.");
            Assertions.Equal(GlabrezuRules38Policy.PrintedFortitude,
                GlabrezuRules38Policy.GoodSaveBase +
                    (GlabrezuRules38Policy.Constitution - 10) / 2,
                "Printed Fortitude is a good save plus Constitution.");
            Assertions.Equal(GlabrezuRules38Policy.PrintedWill,
                GlabrezuRules38Policy.GoodSaveBase +
                    (GlabrezuRules38Policy.Wisdom - 10) / 2,
                "Printed Will is a good save plus Wisdom.");
            Assertions.Equal(GlabrezuRules38Policy.PrintedReflex,
                GlabrezuRules38Policy.PoorSaveBase +
                    (GlabrezuRules38Policy.Dexterity - 10) / 2,
                "Printed Reflex is a POOR save plus Dexterity, which is why +4 is correct and not an anomaly.");
            Assertions.Equal(GlabrezuRules38Policy.PoorSaveBase,
                GlabrezuRules38Policy.BaseReflex,
                "The blueprint carries the poor Reflex base.");

            // The two caster levels the installed engine actually reports are
            // different quantities, and the range is a third, separate fact.
            Assertions.Equal(GlabrezuRules38Policy.GlabrezuHitDice,
                GlabrezuRules38Policy.PreCastEstimatedCasterLevel,
                "The pre-cast estimate is the creature's hit dice, because RuleCalculateAbilityParams reads only ContextSetAbilityParams.");
            Assertions.Equal(14, GlabrezuRules38Policy.SpellLikeCasterLevel,
                "The execution context and everything downstream of it carry the printed caster level.");
            // The ability does not use the installed Close category, which
            // is a flat 30 that reads no caster level; it carries an explicit
            // custom range derived from its own caster level, so the printed
            // reach survives rather than being halved by that limitation.
            Assertions.Equal(60,
                GlabrezuRules38Policy.CloseRangeFeet(
                    GlabrezuRules38Policy.SpellLikeCasterLevel),
                "Printed close range at caster level 14.");
            Assertions.Equal(GlabrezuRules38Policy.SelectedCloseRangeFeet,
                GlabrezuRules38Policy.CloseRangeFeet(
                    GlabrezuRules38Policy.SpellLikeCasterLevel),
                "The shipped range is derived from the caster level, not written down twice.");
            Assertions.True(GlabrezuRules38Policy.SelectedCloseRangeFeet >
                    GlabrezuRules38Policy.InstalledCloseCategoryFeet,
                "The shipped range is the printed one, not the shorter installed category.");
        }

        internal static void PowerWordStunBucketsFollowCurrentHitPoints()
        {
            foreach (int[] row in new[] {
                new[] { 1, 4 }, new[] { 50, 4 },
                new[] { 51, 2 }, new[] { 100, 2 },
                new[] { 101, 1 }, new[] { 150, 1 },
                new[] { 151, 0 }, new[] { 999, 0 } })
                Assertions.Equal(row[1],
                    GlabrezuRules38Policy.PowerWordStunDiceCount(row[0]),
                    "Printed duration bucket at " + row[0] + " current hit points.");

            // No saving throw at any threshold: a recipient escapes only by
            // being above the ceiling, by native immunity, or through spell
            // resistance - and each of those is a separate operand.
            Assertions.True(GlabrezuRules38Policy.PowerWordStunReaches(
                    true, true, 50, false, false),
                "A live susceptible recipient below the ceiling is affected.");
            Assertions.False(GlabrezuRules38Policy.PowerWordStunReaches(
                    true, true, 151, false, false),
                "At or above 151 current hit points nothing is applied at all.");
            Assertions.False(GlabrezuRules38Policy.PowerWordStunReaches(
                    true, true, 50, true, false),
                "Native immunity refuses it.");
            Assertions.False(GlabrezuRules38Policy.PowerWordStunReaches(
                    true, true, 50, false, true),
                "Spell resistance refuses it.");
            Assertions.False(GlabrezuRules38Policy.PowerWordStunReaches(
                    false, true, 50, false, false),
                "Another creature's effect does not resolve here.");
        }

        /// <summary>
        /// The round contract, on the exact production tracker the live
        /// component owns. The thing this is really guarding is that an attack
        /// command is not an input: the released shared tracker takes a
        /// sequence and resets its emitted flag whenever that sequence
        /// changes, which is why it cannot express a once-per-round limit.
        /// </summary>
        internal static void RendIsOncePerRoundAndNotPerCommand()
        {
            object victim = new object();
            object other = new object();

            var round = new GlabrezuRendRound38(
                GlabrezuRules38Policy.RendPincerCount);
            Assertions.Equal(2, round.PincerCount,
                "The printed rend names two pincers.");

            // One pincer alone is not a rend.
            Assertions.False(round.TryArm(true, true, 1, victim),
                "The first pincer cannot rend with nothing before it.");
            round.RecordOutcome(1, victim, true);
            Assertions.False(round.SpentThisRound,
                "A single pincer hit spends nothing.");

            // The same pincer twice is not two pincers.
            Assertions.False(round.TryArm(true, true, 1, victim),
                "One pincer hitting twice is not a rend.");

            // The second pincer on the same victim rends, once.
            Assertions.True(round.TryArm(true, true, 2, victim),
                "Both pincers on one creature in one round must rend.");
            Assertions.True(round.Armed, "The qualifying pincer is armed.");
            round.RecordOutcome(2, victim, true);
            Assertions.True(round.SpentThisRound,
                "The resolved rend spends the round's one use.");
            Assertions.False(round.Armed,
                "A resolved rend leaves nothing pending.");

            // THE DEFECT THIS EXISTS FOR. A new attack command in the same
            // round must not refill the allowance. The production tracker has
            // no command operand at all, so there is nothing a command change
            // can reset - which is exactly the difference from the released
            // shared tracker.
            round.RecordOutcome(1, victim, true);
            Assertions.False(round.TryArm(true, true, 2, victim),
                "A second qualifying pair in the same round does not rend again.");
            round.RecordOutcome(2, victim, true);
            Assertions.False(round.Armed,
                "Nothing is armed by a refused pair.");

            // The next legitimate round makes it available again.
            round.BeginRound();
            Assertions.False(round.SpentThisRound,
                "A new round returns the allowance.");
            round.RecordOutcome(1, victim, true);
            Assertions.True(round.TryArm(true, true, 2, victim),
                "The next round's qualifying pair rends.");
            round.RecordOutcome(2, victim, true);
            Assertions.True(round.SpentThisRound,
                "And spends that round's use.");

            // A new round also forgets pairs, so a hit cannot pair across the
            // round boundary.
            round.BeginRound();
            Assertions.False(round.TryArm(true, true, 2, victim),
                "A pincer hit in the previous round is not a pair in this one.");

            // A pincer that moves between victims inside one round must not
            // erase the hit it already landed. Pincer one hits A, pincer one
            // then hits B, pincer two hits A: the printed rule says two
            // pincers hit the same opponent in the same round, and they did,
            // so this rends. Storing only the last victim per limb would have
            // overwritten A with B and silently refused it.
            var moved = new GlabrezuRendRound38(2);
            moved.RecordOutcome(1, victim, true);
            moved.RecordOutcome(1, other, true);
            Assertions.True(moved.TryArm(true, true, 2, victim),
                "A pincer that already hit this victim earlier in the round still pairs after it strikes someone else.");
            moved.RecordOutcome(2, victim, true);
            Assertions.True(moved.SpentThisRound,
                "And that rend spends the round's one use.");
            // The other victim is still only half-hit, and the allowance is
            // gone for the round anyway.
            Assertions.False(moved.TryArm(true, true, 2, other),
                "The round's rend is spent, so the second victim does not also rend.");

            // Pincers on different creatures never rend.
            var split = new GlabrezuRendRound38(2);
            split.RecordOutcome(1, victim, true);
            Assertions.False(split.TryArm(true, true, 2, other),
                "Pincers that hit different creatures do not rend.");
            Assertions.False(split.SpentThisRound, "And spend nothing.");
            Assertions.True(split.TryArm(true, true, 2, victim),
                "A pincer that does reach the first one's victim still rends.");

            // Wrong limbs and missing operands.
            var limbs = new GlabrezuRendRound38(2);
            limbs.RecordOutcome(1, victim, true);
            Assertions.False(limbs.TryArm(true, true, 3, victim),
                "A limb beyond the printed pincer count never rends - this is where a claw would land.");
            Assertions.False(limbs.TryArm(true, true, 0, victim),
                "A limb index below the first pincer never rends.");
            Assertions.False(limbs.TryArm(true, true, 2, null),
                "No victim, no rend.");
            Assertions.False(limbs.TryArm(true, false, 2, victim),
                "An attack with no live attack command never rends.");
            Assertions.False(limbs.TryArm(false, true, 2, victim),
                "Another creature's attack never rends here.");

            // A miss must not spend the round's allowance.
            var missed = new GlabrezuRendRound38(2);
            missed.RecordOutcome(1, victim, true);
            Assertions.True(missed.TryArm(true, true, 2, victim),
                "The second pincer qualifies before its roll is known.");
            missed.RecordOutcome(2, victim, false);
            Assertions.False(missed.SpentThisRound,
                "A missed rend spends nothing.");
            Assertions.False(missed.Armed, "A miss leaves nothing armed.");
            Assertions.True(missed.TryArm(true, true, 2, victim),
                "So the round's rend is still available to a later qualifying hit.");

            // A pincer that misses records no hit, so it cannot pair later.
            var onlyMisses = new GlabrezuRendRound38(2);
            onlyMisses.RecordOutcome(1, victim, false);
            Assertions.False(onlyMisses.TryArm(true, true, 2, victim),
                "A missed pincer is not a hit to pair with.");

            // Repeating one resolved event grants nothing.
            var replayed = new GlabrezuRendRound38(2);
            replayed.RecordOutcome(1, victim, true);
            Assertions.True(replayed.TryArm(true, true, 2, victim), "Armed.");
            replayed.RecordOutcome(2, victim, true);
            replayed.RecordOutcome(2, victim, true);
            Assertions.True(replayed.SpentThisRound,
                "A duplicated outcome leaves the use spent, not refunded.");
            Assertions.False(replayed.Armed,
                "And re-arms nothing.");

            // Independent owners keep their own allowance.
            var first = new GlabrezuRendRound38(2);
            var second = new GlabrezuRendRound38(2);
            first.RecordOutcome(1, victim, true);
            Assertions.True(first.TryArm(true, true, 2, victim), "Armed.");
            first.RecordOutcome(2, victim, true);
            Assertions.True(first.SpentThisRound, "The first glabrezu rent.");
            Assertions.False(second.SpentThisRound,
                "One glabrezu's rend does not consume another's.");
            second.RecordOutcome(1, victim, true);
            Assertions.True(second.TryArm(true, true, 2, victim),
                "A second independent glabrezu still has its own rend.");

            // An invalid pincer count is refused rather than silently reduced.
            bool refused = false;
            try { new GlabrezuRendRound38(1); }
            catch (ArgumentOutOfRangeException) { refused = true; }
            Assertions.True(refused,
                "A printed rend needs at least two named attacks.");
        }

        /// <summary>
        /// Why the Glabrezu could not reuse the released gate, asserted
        /// against both production types rather than described in a comment.
        /// The released tracker is deliberately left exactly as it is: the
        /// Dire Ape and Girallon are qualified and published on it.
        /// </summary>
        internal static void ReleasedTrackerReopensOnACommandChangeAndIsUnchanged()
        {
            object victim = new object();
            object firstCommand = new object();
            object secondCommand = new object();

            var released = new ClawRendTracker(2);
            released.RecordOutcome(firstCommand, 1, victim, true);
            Assertions.True(released.TryArm(firstCommand, 2, victim),
                "The released tracker rends on its qualifying pair.");
            released.RecordOutcome(firstCommand, 2, victim, true);
            Assertions.True(released.HasEmitted,
                "And records that it emitted one.");

            // The command change resets the whole tracker, emitted flag
            // included, so a second qualifying pair rends again inside what
            // may well be the same round. This is the behaviour the Glabrezu
            // needed to not inherit; it is asserted, not assumed.
            released.RecordOutcome(secondCommand, 1, victim, true);
            Assertions.False(released.HasEmitted,
                "A new attack command clears the released tracker's emitted flag.");
            Assertions.True(released.TryArm(secondCommand, 2, victim),
                "So the released tracker permits a second rend after a command change.");

            // The Glabrezu's own tracker refuses exactly that case.
            var glabrezu = new GlabrezuRendRound38(2);
            glabrezu.RecordOutcome(1, victim, true);
            Assertions.True(glabrezu.TryArm(true, true, 2, victim), "Armed.");
            glabrezu.RecordOutcome(2, victim, true);
            glabrezu.RecordOutcome(1, victim, true);
            Assertions.False(glabrezu.TryArm(true, true, 2, victim),
                "The Glabrezu's round allowance survives any number of command changes.");
        }
    }
}
