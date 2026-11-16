using System;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The Dodge scenario's own judgement, on stand-in observations: what it
    /// accepts as a real activation, and that an interruption, a missing
    /// execution process, a missing prerequisite or a flat-footed target is
    /// reported as a failure and never as a pass.
    /// </summary>
    internal static class GunslingerDodgeScenarioVerdictTests
    {
        private static DodgeCommandObservation AcceptedUse()
        {
            return new DodgeCommandObservation
            {
                AvailableBeforeUse = true, CommandStarted = true,
                CommandFinished = true, CommandResult = "Success",
                HadExecutionProcess = true, DeliveryEnded = true,
                GritBefore = 5, GritAfter = 4, BuffsBefore = 0, BuffsAfter = 1,
                InstanceIsNew = true, AddedNotices = 1, ArmorClassBefore = 14,
                ArmorClassAfter = 16, MarkedNotDispelable = true,
                Permanent = false, TimeLeftSeconds = 5.5d
            };
        }

        private static DodgeCommandObservation RefusedAttempt()
        {
            return new DodgeCommandObservation
            {
                AvailableBeforeUse = false, CommandStarted = true,
                CommandFinished = true, CommandResult = "Fail",
                HadExecutionProcess = false, DeliveryEnded = false,
                GritBefore = 4, GritAfter = 4, BuffsBefore = 1, BuffsAfter = 1,
                InstanceIsNew = false, AddedNotices = 0, ArmorClassBefore = 16,
                ArmorClassAfter = 16
            };
        }

        private static void Rejected(string verdict, string fragment, string message)
        {
            Assertions.True(verdict != null && verdict.Contains(fragment),
                message + " Verdict: " + (verdict ?? "<accepted>"));
        }

        internal static void AcceptedActivation()
        {
            Assertions.True(GunslingerDodgeScenarioVerdict.Accepted(AcceptedUse(), 1) == null,
                "A complete activation was not accepted.");

            // The main-menu fixture's failure: the command ends before any
            // execution process exists.
            DodgeCommandObservation interrupted = AcceptedUse();
            interrupted.CommandResult = "Interrupt";
            interrupted.HadExecutionProcess = false;
            interrupted.DeliveryEnded = false;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(interrupted, 1),
                "before an execution process existed",
                "An interrupted command without an execution process passed.");

            DodgeCommandObservation timedOut = AcceptedUse();
            timedOut.TimedOut = true;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(timedOut, 1), "timed out",
                "A command that never settled passed.");

            DodgeCommandObservation undelivered = AcceptedUse();
            undelivered.DeliveryEnded = false;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(undelivered, 1),
                "delivery did not end", "An unfinished delivery passed.");

            DodgeCommandObservation unavailable = AcceptedUse();
            unavailable.AvailableBeforeUse = false;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(unavailable, 1),
                "unavailable", "An unavailable ability counted as an accepted activation.");
        }

        internal static void AcceptedActivationEffects()
        {
            DodgeCommandObservation free = AcceptedUse();
            free.GritAfter = free.GritBefore;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(free, 1), "expected 1 spent",
                "An activation that charged nothing passed at a cost of 1.");
            Assertions.True(GunslingerDodgeScenarioVerdict.Accepted(free, 0) == null,
                "A cost of 0 (selected True Grit) with nothing spent was not accepted.");

            DodgeCommandObservation doubleCharge = AcceptedUse();
            doubleCharge.GritAfter = 3;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(doubleCharge, 1),
                "expected 1 spent", "A double charge passed.");

            DodgeCommandObservation noBuff = AcceptedUse();
            noBuff.BuffsAfter = 0;
            noBuff.InstanceIsNew = false;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(noBuff, 1),
                "exactly one new buff", "An activation without its buff passed.");

            DodgeCommandObservation silent = AcceptedUse();
            silent.AddedNotices = 0;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(silent, 1),
                "buff-added notices", "A buff the game never announced passed.");

            DodgeCommandObservation cutShort = AcceptedUse();
            cutShort.MarkedNotDispelable = false;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(cutShort, 1),
                "did not receive the buff back",
                "A buff the native action never received back passed.");

            DodgeCommandObservation permanent = AcceptedUse();
            permanent.Permanent = true;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(permanent, 1),
                "not timed within one round", "A permanent buff passed.");

            DodgeCommandObservation wrongBonus = AcceptedUse();
            wrongBonus.ArmorClassAfter = 15;
            Rejected(GunslingerDodgeScenarioVerdict.Accepted(wrongBonus, 1),
                "expected +2", "A +1 armor class change passed.");

            Assertions.Throws<ArgumentNullException>(() =>
                GunslingerDodgeScenarioVerdict.Accepted(null, 1),
                "A missing observation was judged.");
        }

        internal static void RefusedAttemptChargesAndGrantsNothing()
        {
            Assertions.True(GunslingerDodgeScenarioVerdict.Refused(RefusedAttempt()) == null,
                "A refused attempt that changed nothing was not accepted.");

            DodgeCommandObservation executed = RefusedAttempt();
            executed.CommandResult = "Success";
            executed.HadExecutionProcess = true;
            executed.DeliveryEnded = true;
            Rejected(GunslingerDodgeScenarioVerdict.Refused(executed), "carried out",
                "An attempt the game carried out counted as refused.");

            DodgeCommandObservation charged = RefusedAttempt();
            charged.GritAfter = 3;
            Rejected(GunslingerDodgeScenarioVerdict.Refused(charged), "changed grit",
                "A refused attempt that charged grit passed.");

            DodgeCommandObservation regranted = RefusedAttempt();
            regranted.InstanceIsNew = true;
            regranted.AddedNotices = 1;
            Rejected(GunslingerDodgeScenarioVerdict.Refused(regranted),
                "changed the Dodge buffs", "A refused attempt that granted a buff passed.");

            DodgeCommandObservation shifted = RefusedAttempt();
            shifted.ArmorClassAfter = 18;
            Rejected(GunslingerDodgeScenarioVerdict.Refused(shifted),
                "changed armor class", "A refused attempt that changed armor class passed.");

            DodgeCommandObservation hung = RefusedAttempt();
            hung.TimedOut = true;
            Rejected(GunslingerDodgeScenarioVerdict.Refused(hung), "neither ran nor was refused",
                "An attempt that never settled counted as refused.");

            // A query and an attempt are different things: an attempt made
            // while the ability reads as available is not a refusal case.
            DodgeCommandObservation available = RefusedAttempt();
            available.AvailableBeforeUse = true;
            Rejected(GunslingerDodgeScenarioVerdict.Refused(available),
                GunslingerDodgeScenarioVerdict.NotJudged,
                "An attempt on an available ability was judged as a refusal.");
        }

        internal static void IncomingAttackReadsTheBonus()
        {
            var before = new DodgeIncomingAttack(true, 14, false);
            var during = new DodgeIncomingAttack(true, 16, false);
            var after = new DodgeIncomingAttack(true, 14, false);
            Assertions.True(GunslingerDodgeScenarioVerdict.IncomingAttacks(before, during,
                after) == null, "14, 16, 14 was not accepted.");
            Rejected(GunslingerDodgeScenarioVerdict.IncomingAttacks(before,
                new DodgeIncomingAttack(true, 14, false), after), "expected +2",
                "An attack that did not meet the bonus passed.");
            Rejected(GunslingerDodgeScenarioVerdict.IncomingAttacks(before, during,
                new DodgeIncomingAttack(true, 16, false)), "after Dodge",
                "A bonus that outlived the buff passed.");
            // A flat-footed target has no dodge bonus, so the roll proves nothing.
            Rejected(GunslingerDodgeScenarioVerdict.IncomingAttacks(before,
                new DodgeIncomingAttack(true, 16, true), after),
                GunslingerDodgeScenarioVerdict.NotJudged,
                "A flat-footed roll was judged.");
            Rejected(GunslingerDodgeScenarioVerdict.IncomingAttacks(before,
                new DodgeIncomingAttack(false, 0, false), after),
                GunslingerDodgeScenarioVerdict.NotJudged,
                "A missing attack roll was judged.");
            Rejected(GunslingerDodgeScenarioVerdict.IncomingAttacks(before, null, after),
                GunslingerDodgeScenarioVerdict.NotJudged, "A missing attack was judged.");
        }

        internal static void UnusedExpiry()
        {
            Assertions.True(GunslingerDodgeScenarioVerdict.Expiry(true, 6.02d, 1, 14, 14,
                1, 1, 0) == null, "A one-round end was not accepted.");
            Rejected(GunslingerDodgeScenarioVerdict.Expiry(false, -1d, 0, 14, 16, 1, 0, 1),
                "did not end by itself", "A buff that never ended passed.");
            Rejected(GunslingerDodgeScenarioVerdict.Expiry(true, 12d, 1, 14, 14, 1, 1, 0),
                "expected one round", "A two-round buff passed.");
            Rejected(GunslingerDodgeScenarioVerdict.Expiry(true, 6d, 2, 14, 14, 1, 1, 0),
                "buff-removed notices", "Two removals passed.");
            Rejected(GunslingerDodgeScenarioVerdict.Expiry(true, 6d, 1, 14, 16, 1, 1, 0),
                "after the end", "A bonus left behind passed.");
            Rejected(GunslingerDodgeScenarioVerdict.Expiry(true, 6d, 1, 14, 14, 2, 1, 1),
                "turned on", "A bonus applied twice passed.");
        }

        internal static void RecastNeedsItsPrerequisites()
        {
            Assertions.True(GunslingerDodgeScenarioVerdict.Recast(null, null, null, true, 16,
                true, 6.01d, 14, 14) == null, "A judged, isolated recast was not accepted.");

            // Each missing prerequisite alone makes the check "not judged",
            // even when everything observed afterwards looks right.
            foreach (string[] missing in new[]
            {
                new[] { "the command ended Interrupt", null, null },
                new[] { null, "the earlier instance is still on the unit", null },
                new[] { null, null, "the later activation did not produce a different instance" }
            })
            {
                string verdict = GunslingerDodgeScenarioVerdict.Recast(missing[0],
                    missing[1], missing[2], true, 16, true, 6d, 14, 14);
                Rejected(verdict, GunslingerDodgeScenarioVerdict.NotJudged,
                    "A recast without a prerequisite was judged.");
            }

            Rejected(GunslingerDodgeScenarioVerdict.Recast(null, null, null, false, 14, true,
                6d, 14, 14), "was gone when the earlier one's deadline passed",
                "A later activation ended by the earlier deadline passed.");
            Rejected(GunslingerDodgeScenarioVerdict.Recast(null, null, null, true, 16, true,
                3.5d, 14, 14), "one round of its own",
                "A later activation cut short passed.");
            Rejected(GunslingerDodgeScenarioVerdict.Recast(null, null, null, true, 16, false,
                -1d, 14, 16), "did not end by itself",
                "A later activation that never ended passed.");
            Rejected(GunslingerDodgeScenarioVerdict.Recast(null, null, null, true, 16, true,
                6d, 14, 16), "after the later activation ended",
                "A bonus left after the recast passed.");

            Assertions.True(GunslingerDodgeScenarioVerdict.EarlyRemoval(true, true, 1, 14,
                14) == null, "A verified early removal was not accepted.");
            Rejected(GunslingerDodgeScenarioVerdict.EarlyRemoval(false, true, 1, 14, 14),
                "not the earlier activation's instance",
                "Removal of some other instance passed.");
            Rejected(GunslingerDodgeScenarioVerdict.EarlyRemoval(true, false, 1, 14, 14),
                "still on the unit", "An instance that stayed passed.");
            Rejected(GunslingerDodgeScenarioVerdict.EarlyRemoval(true, true, 2, 14, 14),
                "buff-removed notices", "A removal counted twice passed.");
            Rejected(GunslingerDodgeScenarioVerdict.EarlyRemoval(true, true, 1, 14, 16),
                "after the removal", "A bonus left after the removal passed.");
        }
    }
}
