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
            // Each missing prerequisite alone makes the check "not judged",
            // even when every time and everything observed afterwards is right.
            foreach (string[] missing in new[]
            {
                new[] { "the command ended Interrupt", null, null },
                new[] { null, "the earlier instance is still on the unit", null },
                new[] { null, null, "the later activation did not produce a different instance" }
            })
            {
                string verdict = GunslingerDodgeScenarioVerdict.Recast(missing[0],
                    missing[1], missing[2], TimelyRecast(), 14);
                Rejected(verdict, GunslingerDodgeScenarioVerdict.NotJudged,
                    "A recast without a prerequisite was judged.");
            }
            Assertions.Throws<ArgumentNullException>(() =>
                GunslingerDodgeScenarioVerdict.Recast(null, null, null, null, 14),
                "A recast without its measured times was judged.");

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

        // ---- review corrections: recast timing, waits, finalization, admission ----

        private static DodgeWait Wait(string name, bool reached)
        {
            var wait = new DodgeWait(name, 60d);
            // Through the production poll: either the condition holds, or
            // the real-time limit runs out with the condition still false.
            wait.Poll(reached, reached ? 1.5d : 60d);
            return wait;
        }

        /// <summary>
        /// A timely recast: cast at 100, deadline 106, removed at 102, later
        /// cast at 102.5, unit read at 106.6, later ended at 108.52.
        /// </summary>
        private static DodgeRecastObservation TimelyRecast()
        {
            return new DodgeRecastObservation
            {
                EarlierDeadlineCaptured = true, EarlierAddedAt = 100d,
                EarlierDeadline = 106d, EarlierRemovedAt = 102d,
                LaterAddedAt = 102.5d, DeadlineWait = Wait("deadline", true),
                SurvivalObservedAt = 106.6d, LaterInstanceOnUnit = true,
                ArmorClassAtObservation = 16, EndWait = Wait("end", true),
                LaterEnded = true, LaterEndedAt = 108.52d, ArmorClassAfterEnd = 14
            };
        }

        private static string Recast(DodgeRecastObservation timing)
        {
            return GunslingerDodgeScenarioVerdict.Recast(null, null, null, timing, 14);
        }

        internal static void RecastTimelyOverlapPasses()
        {
            Assertions.True(Recast(TimelyRecast()) == null,
                "A timely recast read after the earlier deadline was not accepted: " +
                Recast(TimelyRecast()));
            string evidence = TimelyRecast().Describe();
            foreach (string part in new[] { "earlierDeadline=106", "earlierRemovedAt=102",
                "laterAddedAt=102.5", "survivalObservedAt=106.6", "deadline=reached" })
                Assertions.True(evidence.Contains(part),
                    "The recast evidence does not carry " + part + ": " + evidence);
        }

        internal static void RecastAfterTheOldDeadlineFails()
        {
            // Every activation succeeded and the later buff behaves, but it
            // was applied after the earlier deadline: nothing overlapped.
            DodgeRecastObservation late = TimelyRecast();
            late.LaterAddedAt = 106.4d;
            late.SurvivalObservedAt = 107d;
            late.LaterEndedAt = 112.41d;
            Rejected(Recast(late), GunslingerDodgeScenarioVerdict.NotJudged,
                "A recast applied after the earlier deadline was judged.");
            Rejected(Recast(late), "so nothing overlapped",
                "The late recast's reason does not say that nothing overlapped.");

            DodgeRecastObservation removedLate = TimelyRecast();
            removedLate.EarlierRemovedAt = 106.2d;
            removedLate.LaterAddedAt = 106.3d;
            Rejected(Recast(removedLate), "was not removed early",
                "A removal after the earlier deadline counted as early.");

            DodgeRecastObservation unknown = TimelyRecast();
            unknown.EarlierDeadlineCaptured = false;
            Rejected(Recast(unknown), "deadline was not captured",
                "A recast without the earlier deadline was judged.");
        }

        internal static void RecastDeadlineWaitMustReachItsDeadline()
        {
            // The wait ran out its real-time limit while game time was still
            // short of the deadline. The later buff then expired normally,
            // which must not rescue the check.
            DodgeRecastObservation stalled = TimelyRecast();
            stalled.DeadlineWait = Wait("recast-earlier-deadline", false);
            stalled.SurvivalObservedAt = 104.1d;
            Assertions.True(stalled.LaterEnded && stalled.EndWait.Reached,
                "The stand-in no longer has a normally ending later buff.");
            Rejected(Recast(stalled), GunslingerDodgeScenarioVerdict.NotJudged,
                "A deadline wait that ran out was treated as having reached the deadline.");
            Rejected(Recast(stalled), "did not reach it",
                "The stalled wait's reason does not name the wait.");

            // The wait reports reached but the unit was read too soon after
            // the deadline for the scheduling margin.
            DodgeRecastObservation early = TimelyRecast();
            early.SurvivalObservedAt = 106.2d;
            Rejected(Recast(early), GunslingerDodgeScenarioVerdict.NotJudged,
                "A reading inside the scheduling margin was judged.");

            DodgeRecastObservation notStarted = TimelyRecast();
            notStarted.DeadlineWait = null;
            Rejected(Recast(notStarted), "not started",
                "A recast with no deadline wait was judged.");

            // With the overlap established, the outcome itself still decides.
            DodgeRecastObservation gone = TimelyRecast();
            gone.LaterInstanceOnUnit = false;
            gone.ArmorClassAtObservation = 14;
            Rejected(Recast(gone), "was not on the unit once the earlier one's deadline had passed",
                "A later instance ended by the earlier deadline passed.");
            DodgeRecastObservation cutShort = TimelyRecast();
            cutShort.LaterEndedAt = 106.9d;
            Rejected(Recast(cutShort), "one round of its own",
                "A later activation cut short passed.");
            DodgeRecastObservation neverEnded = TimelyRecast();
            neverEnded.EndWait = Wait("end", false);
            neverEnded.LaterEnded = false;
            Rejected(Recast(neverEnded), "did not end by itself",
                "A later activation that never ended passed.");
            DodgeRecastObservation leftBehind = TimelyRecast();
            leftBehind.ArmorClassAfterEnd = 16;
            Rejected(Recast(leftBehind), "after the later activation ended",
                "A bonus left after the recast passed.");
        }

        internal static void WaitsRecordWhyTheyEnded()
        {
            var reached = new DodgeWait("buff-gone", 30d);
            Assertions.False(reached.Poll(false, 1d), "A wait ended with time left and no condition.");
            Assertions.True(reached.Poll(true, 2d), "A wait did not end on its condition.");
            Assertions.True(reached.Reached && reached.Ended, "A met condition was not recorded.");

            var ranOut = new DodgeWait("deadline", 30d);
            Assertions.False(ranOut.Poll(false, 29.9d), "A wait ended before its limit.");
            Assertions.True(ranOut.Poll(false, 30d), "A wait did not end at its limit.");
            Assertions.True(ranOut.Ended && !ranOut.Reached,
                "A wait that ran out its limit counts as reached.");
            // The answer is final: a condition that holds later changes nothing.
            Assertions.True(ranOut.Poll(true, 31d) && !ranOut.Reached,
                "A wait that had run out was reached afterwards.");
            Assertions.True(ranOut.Describe().Contains("ran-out"),
                "The evidence does not say the wait ran out: " + ranOut.Describe());

            Assertions.True(GunslingerDodgeScenarioVerdict.Waits(new[] { reached }) == null,
                "Reached waits were rejected.");
            Rejected(GunslingerDodgeScenarioVerdict.Waits(new[] { reached, ranOut }),
                "deadline", "A wait that ran out did not fail the scenario.");
            Rejected(GunslingerDodgeScenarioVerdict.Waits(new DodgeWait[] { null }),
                "<missing>", "A missing wait passed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => new DodgeWait("x", 0d),
                "A wait with no limit was accepted.");
        }

        private sealed class Row
        {
            internal string Name;
            internal bool Passed;
            internal string Observed;
        }

        private static DodgeScenarioSession<Row> Session()
        {
            return new DodgeScenarioSession<Row>((name, expected, observed) =>
                new Row { Name = name, Passed = false, Observed = observed });
        }

        /// <summary>
        /// Stand-in for the scenario's steps, with the same shape: work that
        /// records assertions as it goes, and a finally block that releases
        /// what it owns and records the cleanup result.
        /// </summary>
        private static System.Collections.Generic.IEnumerator<int> Steps(
            DodgeScenarioSession<Row> session, int[] disposals, bool failingRelease,
            bool throwingCleanup)
        {
            try
            {
                session.Stage = "accepted-activation";
                session.Assertions.Add(new Row { Name = "dodge-accepted-activation", Passed = true });
                yield return 0;
                session.Stage = "recast";
                session.Assertions.Add(new Row { Name = "dodge-unused-expiry", Passed = true });
                yield return 0;
                session.Stage = "cleanup";
            }
            finally
            {
                disposals[0]++;
                var releases = new DodgeReleases();
                releases.Run("pause", () => { });
                releases.Run("unit attacker", () =>
                {
                    if (failingRelease) throw new InvalidOperationException("the unit would not go");
                });
                releases.Run("unit defender", () => { });
                session.Assertions.Add(new Row
                {
                    Name = "dodge-cleanup", Passed = releases.Failures.Count == 0,
                    Observed = releases.Describe()
                });
                if (throwingCleanup) throw new InvalidOperationException("cleanup itself broke");
            }
        }

        internal static void OuterAbortKeepsScenarioAndCleanupEvidence()
        {
            DodgeScenarioSession<Row> session = Session();
            var disposals = new int[1];
            var steps = Steps(session, disposals, false, false);
            session.Begin(steps);
            steps.MoveNext();
            steps.MoveNext();

            // The runner completes with a result of its own.
            var outer = new System.Collections.Generic.List<Row>
            {
                new Row { Name = "no-save-writing-api", Passed = false, Observed = "a save write was observed" }
            };
            DodgeFinalization finalization = session.Finish(outer);
            Assertions.True(finalization.Aborted && finalization.DisposalFailure == null,
                "A scenario cut off by the runner was not recorded as aborted.");
            string names = string.Join(",", outer.ConvertAll(row => row.Name).ToArray());
            Assertions.Equal(
                "no-save-writing-api,dodge-accepted-activation,dodge-unused-expiry,dodge-scenario-aborted,dodge-cleanup",
                names, "The outer result does not hold its own failure, the partial observations and the cleanup result.");
            Assertions.Equal("a save write was observed", outer[0].Observed,
                "The outer failure's reason changed.");
            Assertions.True(outer[3].Observed.Contains("stage recast"),
                "The abort does not name the stage the scenario was at: " + outer[3].Observed);
            Assertions.True(outer[4].Passed, "A clean cleanup was recorded as failed.");
            Assertions.Equal(4, finalization.Attached, "The attached count is wrong.");
            Assertions.Equal(1, disposals[0], "The steps were not disposed exactly once.");
            Assertions.False(session.Active || session.HasUnattached,
                "The session still has something to finish.");

            // Finalization again, with the same result and with another one.
            DodgeFinalization again = session.Finish(outer);
            Assertions.True(!again.Aborted && again.Attached == 0 && outer.Count == 5,
                "A second finalization changed the result.");
            var other = new System.Collections.Generic.List<Row>();
            session.Finish(other);
            Assertions.Equal(0, other.Count, "A later result received assertions already published.");
            Assertions.Equal(1, disposals[0], "A second finalization disposed the steps again.");
            Assertions.Equal(1, session.Disposals, "The session counted more than one disposal.");
        }

        internal static void CleanupFailureAppearsBesideTheAbortReason()
        {
            DodgeScenarioSession<Row> session = Session();
            var disposals = new int[1];
            var steps = Steps(session, disposals, true, true);
            session.Begin(steps);
            steps.MoveNext();
            var outer = new System.Collections.Generic.List<Row>
            {
                new Row { Name = "working-save-guard", Passed = false, Observed = "baseline was loaded" }
            };
            DodgeFinalization finalization = session.Finish(outer);
            Assertions.True(finalization.Aborted && finalization.DisposalFailure != null,
                "The exception raised while ending the steps was lost.");
            Assertions.Equal("working-save-guard", outer[0].Name, "The outer failure moved.");
            Assertions.Equal("baseline was loaded", outer[0].Observed, "The outer reason changed.");
            Row cleanup = outer.Find(row => row.Name == "dodge-cleanup");
            Assertions.True(cleanup != null && !cleanup.Passed &&
                cleanup.Observed.Contains("unit attacker: the unit would not go") &&
                cleanup.Observed.Contains("attempted=pause,unit attacker,unit defender"),
                "The failing release, or the releases attempted after it, are not in the result: " +
                (cleanup == null ? "<no cleanup row>" : cleanup.Observed));
            Row disposal = outer.Find(row => row.Name == "dodge-cleanup-disposal");
            Assertions.True(disposal != null && !disposal.Passed &&
                disposal.Observed.Contains("cleanup itself broke"),
                "The disposal exception is not in the result.");
            Assertions.True(outer.Exists(row => row.Name == "dodge-scenario-aborted"),
                "The abort is not in the result.");
            Assertions.Equal(1, disposals[0], "The steps were not disposed exactly once.");
            session.Finish(outer);
            Assertions.Equal(1, disposals[0], "A second finalization disposed the steps again.");

            // One release that throws does not stop the others.
            var releases = new DodgeReleases();
            int ran = 0;
            releases.Run("first", () => ran++);
            releases.Run("second", () => { throw new InvalidOperationException("no"); });
            releases.Run("third", () => ran++);
            Assertions.True(ran == 2 && releases.Attempted.Count == 3 &&
                releases.Failures.Count == 1 && releases.Failures[0] == "second: no",
                "A failing release stopped the releases after it: " + releases.Describe());
        }

        internal static void NormalCompletionDoesNotDuplicate()
        {
            // The scenario finishes by itself and its result is built on its
            // own list; the runner's finalization then has nothing to add.
            DodgeScenarioSession<Row> session = Session();
            var disposals = new int[1];
            var steps = Steps(session, disposals, false, false);
            session.Begin(steps);
            while (steps.MoveNext()) { }
            Assertions.True(session.End() == null, "Ending finished steps raised.");
            session.Assertions.Add(new Row { Name = "loaded-mod-version", Passed = true });
            session.MarkPublished();
            int count = session.Assertions.Count;
            DodgeFinalization same = session.Finish(session.Assertions);
            Assertions.True(!same.Aborted && same.Attached == 0 &&
                session.Assertions.Count == count,
                "Finalizing a result built on the scenario's own list added to it.");
            var copy = new System.Collections.Generic.List<Row>(session.Assertions);
            DodgeFinalization copied = session.Finish(copy);
            Assertions.True(copied.Attached == 0 && copy.Count == count,
                "Finalizing a published result duplicated its assertions.");
            Assertions.Equal(4, count, "The finished scenario's assertions are not the expected four.");
            Assertions.Equal(1, disposals[0], "The finished steps were not disposed exactly once.");
            Assertions.True(session.End() == null && session.Disposals == 1,
                "Ending the steps a second time disposed them again.");

            // A session that never ran leaves any result alone.
            var untouched = new System.Collections.Generic.List<Row> { new Row { Name = "other" } };
            DodgeFinalization idle = Session().Finish(untouched);
            Assertions.True(!idle.Aborted && idle.Attached == 0 && untouched.Count == 1,
                "A scenario that never ran changed another scenario's result.");
            Assertions.Throws<InvalidOperationException>(() =>
            {
                DodgeScenarioSession<Row> twice = Session();
                twice.Begin(Steps(twice, new int[1], false, false));
                twice.Begin(Steps(twice, new int[1], false, false));
            }, "Two sets of steps were allowed to run at once.");
        }

        internal static void AdmissionRequiresAutomaticExit()
        {
            Assertions.Equal(RuntimeTestScenarioCatalog.DisposableGunslingerDodge,
                GunslingerDodgeScenarioAdmission.Scenario,
                "The admission rule names a different scenario from the catalog.");
            Assertions.Equal(GunslingerDodgeScenarioAdmission.ExitRequired,
                GunslingerDodgeScenarioAdmission.Rejection(
                    RuntimeTestScenarioCatalog.DisposableGunslingerDodge, false),
                "A Dodge request that does not exit was admitted.");
            Assertions.True(GunslingerDodgeScenarioAdmission.Rejection(
                RuntimeTestScenarioCatalog.DisposableGunslingerDodge, true) == null,
                "A Dodge request with automatic exit was refused.");
            // The rule is Dodge's own.
            foreach (string other in new[] { RuntimeTestScenarioCatalog.WorkingSaveSmoke,
                RuntimeTestScenarioCatalog.DisposableGunslingerDeadeye, "", null })
                Assertions.True(GunslingerDodgeScenarioAdmission.Rejection(other, false) == null,
                    "The Dodge exit rule refused another scenario: " + other);
        }
    }
}
