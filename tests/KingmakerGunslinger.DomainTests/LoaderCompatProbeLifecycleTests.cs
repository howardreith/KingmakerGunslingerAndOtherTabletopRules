using System;
using System.Collections.Generic;
using KmgLoaderCompatHarness;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The loader-compat harness's probe ownership rules, exercised on the harness's own component with stand-in
    /// probes: who may poll a probe, how it ends, and when the next one may start.
    /// </summary>
    internal static class LoaderCompatProbeLifecycleTests
    {
        private sealed class StandInProbe : IOwnedProbe
        {
            private readonly string name;
            private readonly List<string> journal;
            private readonly Queue<string> verdicts;
            private readonly Func<CleanupReport> release;

            public StandInProbe(string name, List<string> journal, IEnumerable<string> verdicts, Func<CleanupReport> release = null)
            {
                this.name = name;
                this.journal = journal;
                this.verdicts = new Queue<string>(verdicts);
                this.release = release;
            }

            public int Polls { get; private set; }
            public int Releases { get; private set; }
            public string ReleasedWith { get; private set; }

            public object Result
            {
                get { return "recorded by " + name; }
            }

            public string Poll()
            {
                Polls++;
                journal.Add("poll:" + name);
                // The last verdict repeats: a probe that never finishes keeps answering null.
                return verdicts.Count > 1 ? verdicts.Dequeue() : verdicts.Peek();
            }

            public CleanupReport Release(string testFailure)
            {
                Releases++;
                ReleasedWith = testFailure;
                journal.Add("release:" + name);
                return release == null ? new CleanupReport() : release();
            }
        }

        private static CleanupReport Unrestored(params string[] failures)
        {
            var report = new CleanupReport();
            report.Failures.AddRange(failures);
            return report;
        }

        internal static void TimedOutProbeCannotSupplyTheNextProbesResult()
        {
            var journal = new List<string>();
            var slot = new ProbeSlot();
            var first = new StandInProbe("first", journal, new string[] { null });
            var second = new StandInProbe("second", journal, new[] { string.Empty });

            slot.Begin("0|first-step", first);
            Assertions.True(slot.Poll("0|first-step") == null && slot.Poll("0|first-step") == null, "The first probe is still running.");

            // While the first probe is active no other step can use the slot.
            Assertions.Throws<InvalidOperationException>(() => slot.Poll("1|second-step"), "Another step polled the first step's probe.");
            Assertions.Throws<InvalidOperationException>(() => slot.Begin("1|second-step", second), "A second probe started beside the first.");
            Assertions.True(slot.Terminate("1|second-step", "not mine") == null && slot.IsActive,
                "A step that owns no probe ended another step's probe.");

            // The first step times out.
            ProbeOutcome timedOut = slot.Terminate("0|first-step", "step timeout after 5s");
            Assertions.Equal("0|first-step", timedOut.Owner, "The timed-out probe's outcome left its step.");
            Assertions.Equal("recorded by first", (string)timedOut.Result, "The timed-out probe's record was lost.");
            Assertions.Equal("step timeout after 5s", timedOut.TestFailure, "The timeout was not kept as the probe's failure.");
            Assertions.Equal("step timeout after 5s", first.ReleasedWith, "The probe was not told why it ended.");
            Assertions.True(!timedOut.Passed && !slot.IsActive && first.Releases == 1, "The timed-out probe was not ended exactly once.");

            // Ending it again changes nothing and releases nothing twice.
            Assertions.True(ReferenceEquals(timedOut, slot.Terminate("0|first-step", "again")) && first.Releases == 1,
                "A second termination was not idempotent.");
            Assertions.True(slot.Terminate("1|second-step", "never started") == null, "A step that had no probe received another step's outcome.");

            // The next step gets its own probe and its own result.
            int firstPolls = first.Polls;
            slot.Begin("1|second-step", second);
            ProbeOutcome next = slot.Poll("1|second-step");
            Assertions.Equal("1|second-step", next.Owner, "The second step's outcome carries another identity.");
            Assertions.Equal("recorded by second", (string)next.Result, "The second step received the first probe's record.");
            Assertions.True(next.Passed && first.Polls == firstPolls && second.Polls == 1, "The timed-out probe was resumed by the next step.");
            // A late call for the first step is never answered with the second step's outcome, and releases nothing.
            Assertions.True(!ReferenceEquals(next, slot.Terminate("0|first-step", "late")) && first.Releases == 1 && second.Releases == 1,
                "The first step was handed the second step's outcome.");
        }

        internal static void ProbeThatThrowsIsEndedAndReleased()
        {
            var journal = new List<string>();
            var slot = new ProbeSlot();
            var probe = new ThrowingProbe(journal);
            slot.Begin("0|throws", probe);
            ProbeOutcome outcome = slot.Poll("0|throws");
            Assertions.True(outcome != null && !outcome.Passed && outcome.TestFailure.Contains("the probe threw") &&
                outcome.TestFailure.Contains("stand-in fault"), "An exception escaping the probe was not kept as its failure.");
            Assertions.True(!slot.IsActive && journal.Count == 2 && journal[1] == "release:throws", "A probe that threw was not released.");
        }

        private sealed class ThrowingProbe : IOwnedProbe
        {
            private readonly List<string> journal;
            public ThrowingProbe(List<string> journal) { this.journal = journal; }

            public object Result
            {
                get { return "recorded before the fault"; }
            }

            public string Poll()
            {
                journal.Add("poll:throws");
                throw new InvalidOperationException("stand-in fault");
            }

            public CleanupReport Release(string testFailure)
            {
                journal.Add("release:throws");
                return new CleanupReport();
            }
        }

        internal static void OneFailingCleanupDoesNotStopTheOthers()
        {
            var ran = new List<string>();
            var releases = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>("event subscription", () => ran.Add("event subscription")),
                new KeyValuePair<string, Action>("forced roll", () => { ran.Add("forced roll"); throw new InvalidOperationException("queue unavailable"); }),
                new KeyValuePair<string, Action>("unit 1", () => ran.Add("unit 1")),
                new KeyValuePair<string, Action>("unit 2", () => { ran.Add("unit 2"); throw new NullReferenceException("already disposed"); }),
                new KeyValuePair<string, Action>("blueprint clone 1", () => ran.Add("blueprint clone 1"))
            };

            CleanupReport report = ProbeCleanup.RunAll(releases);
            Assertions.Equal("event subscription,forced roll,unit 1,unit 2,blueprint clone 1", string.Join(",", ran.ToArray()),
                "A failing release stopped the releases after it.");
            Assertions.Equal(5, report.Attempted.Count, "Not every release was attempted.");
            Assertions.Equal(2, report.Failures.Count, "Not every failing release was recorded.");
            Assertions.True(report.Failures[0].StartsWith("forced roll: InvalidOperationException: queue unavailable", StringComparison.Ordinal) &&
                report.Failures[1].StartsWith("unit 2: NullReferenceException: already disposed", StringComparison.Ordinal),
                "A cleanup failure lost its resource or its cause.");
            Assertions.True(!report.Restored, "Failed releases were reported as restored isolation.");
            Assertions.True(ProbeCleanup.RunAll(new List<KeyValuePair<string, Action>>()).Restored, "Nothing to release is not a failure.");
        }

        internal static void CleanupFailureCannotPass()
        {
            var journal = new List<string>();

            // Every check held, but a unit could not be disposed.
            var slot = new ProbeSlot();
            slot.Begin("0|checks-held", new StandInProbe("checks-held", journal, new[] { string.Empty }, () => Unrestored("unit 2: disposal threw")));
            ProbeOutcome held = slot.Poll("0|checks-held");
            Assertions.True(string.IsNullOrEmpty(held.TestFailure) && !held.Passed, "A probe whose cleanup failed passed.");
            Assertions.Equal("cleanup failed: unit 2: disposal threw", held.Failure, "The cleanup failure is not the step's failure.");

            // A failed check keeps its own failure and gains every cleanup failure.
            slot = new ProbeSlot();
            slot.Begin("0|check-failed", new StandInProbe("check-failed", journal, new[] { "armor class was 15" },
                () => Unrestored("forced roll: still queued", "blueprint clone 1: destroy threw")));
            ProbeOutcome failed = slot.Poll("0|check-failed");
            Assertions.Equal("armor class was 15; cleanup failed: forced roll: still queued; cleanup failed: blueprint clone 1: destroy threw",
                failed.Failure, "The original failure or a cleanup failure was dropped.");
            Assertions.Equal("armor class was 15", failed.TestFailure, "The original test failure was rewritten.");

            // A release that throws outright is a cleanup failure too.
            slot = new ProbeSlot();
            slot.Begin("0|release-throws", new StandInProbe("release-throws", journal, new[] { string.Empty },
                () => { throw new InvalidOperationException("release fault"); }));
            ProbeOutcome thrown = slot.Poll("0|release-throws");
            Assertions.True(!thrown.Passed && !thrown.IsolationRestored && thrown.Failure.Contains("release fault"),
                "A release that threw was not a cleanup failure.");

            // And a clean probe still passes.
            slot = new ProbeSlot();
            slot.Begin("0|clean", new StandInProbe("clean", journal, new[] { string.Empty }));
            ProbeOutcome clean = slot.Poll("0|clean");
            Assertions.True(clean.Passed && clean.Failure == null && clean.IsolationRestored, "A clean probe did not pass.");
        }

        internal static void NextProbeStartsOnlyAfterIsolationIsRestored()
        {
            var journal = new List<string>();
            var slot = new ProbeSlot();
            var first = new StandInProbe("first", journal, new string[] { null, "first check failed" });
            var second = new StandInProbe("second", journal, new[] { string.Empty });

            // Restored: the failed step may be followed, and the next probe is polled only after the first is released.
            slot.Begin("0|first-step", first);
            Assertions.True(slot.Poll("0|first-step") == null, "The first probe is still running.");
            ProbeOutcome ended = slot.Poll("0|first-step");
            Assertions.True(ended != null && !ended.Passed && ended.IsolationRestored && slot.IsolationRestored, "A clean release was not restored isolation.");
            Assertions.Equal(StepContinuation.Advance, ProbeSlot.Decide(true, true, ended.IsolationRestored),
                "A failed step that allows continuation did not continue after its isolation was restored.");
            slot.Begin("1|second-step", second);
            slot.Poll("1|second-step");
            Assertions.Equal("poll:first,poll:first,release:first,poll:second,release:second", string.Join(",", journal.ToArray()),
                "The next probe ran before the previous probe was released.");

            // Not restored: nothing further may start, whatever the step says about continuing.
            journal.Clear();
            slot = new ProbeSlot();
            var leaking = new StandInProbe("leaking", journal, new[] { string.Empty }, () => Unrestored("unit 1: still in the area"));
            var after = new StandInProbe("after", journal, new[] { string.Empty });
            slot.Begin("0|leaking-step", leaking);
            ProbeOutcome leaked = slot.Poll("0|leaking-step");
            Assertions.True(!leaked.IsolationRestored && !slot.IsolationRestored, "A failed release was reported as restored isolation.");
            Assertions.Equal(StepContinuation.StopLaunch, ProbeSlot.Decide(false, true, leaked.IsolationRestored),
                "The launch continued although isolation was not restored.");
            Assertions.Equal(StepContinuation.StopLaunch, ProbeSlot.Decide(true, true, leaked.IsolationRestored),
                "Continue-on-failure overrode unrestored isolation.");
            InvalidOperationException refused = Assertions.Throws<InvalidOperationException>(() => slot.Begin("1|after-step", after),
                "A probe started although the previous probe's isolation was not restored.");
            Assertions.True(refused.Message.Contains("0|leaking-step") && after.Polls == 0, "The refusal does not name the step that left the game dirty.");

            // The ordinary rules are unchanged.
            Assertions.Equal(StepContinuation.Advance, ProbeSlot.Decide(false, false, true), "A passing step did not advance.");
            Assertions.Equal(StepContinuation.StopLaunch, ProbeSlot.Decide(true, false, true), "A failed step without continuation advanced.");

            // Abandoning at the end of a launch releases whatever is still active, under its own step.
            journal.Clear();
            slot = new ProbeSlot();
            var running = new StandInProbe("running", journal, new string[] { null });
            slot.Begin("3|running-step", running);
            slot.Poll("3|running-step");
            ProbeOutcome abandoned = slot.Abandon("the run ended");
            Assertions.True(abandoned.Owner == "3|running-step" && running.Releases == 1 && slot.Abandon("again") == null,
                "An active probe was not released when the launch ended.");
        }
    }
}
