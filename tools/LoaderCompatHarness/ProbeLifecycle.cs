using System;
using System.Collections.Generic;

namespace KmgLoaderCompatHarness
{
    // Who owns a running probe, how it is ended and what its end amounts to. Nothing here touches the game, so the
    // rules can be exercised without it (tests: loader-compat-probe.* in the domain suite).

    /// <summary>What happened when a probe released what it owned. Every release is attempted; none hides another.</summary>
    internal sealed class CleanupReport
    {
        public readonly List<string> Attempted = new List<string>();
        public readonly List<string> Failures = new List<string>();

        public bool Restored
        {
            get { return Failures.Count == 0; }
        }
    }

    internal static class ProbeCleanup
    {
        /// <summary>Runs every release on its own. A release that throws is recorded and the rest still run.</summary>
        public static CleanupReport RunAll(IEnumerable<KeyValuePair<string, Action>> releases)
        {
            var report = new CleanupReport();
            if (releases == null) return report;
            foreach (KeyValuePair<string, Action> release in releases)
            {
                report.Attempted.Add(release.Key);
                try
                {
                    if (release.Value != null) release.Value();
                }
                catch (Exception exception)
                {
                    report.Failures.Add(release.Key + ": " + exception.GetType().Name + ": " + exception.Message);
                }
            }

            return report;
        }
    }

    /// <summary>A probe that owns things in the running game and can give them back.</summary>
    internal interface IOwnedProbe
    {
        /// <summary>What the probe recorded, whatever its end.</summary>
        object Result { get; }

        /// <summary>Null while running; otherwise the probe's own verdict, empty when all its checks held. Releases nothing.</summary>
        string Poll();

        /// <summary>Gives back everything the probe owns. The slot calls it exactly once, with the reason the probe ended.</summary>
        CleanupReport Release(string testFailure);
    }

    /// <summary>How one step's probe ended: its own verdict and whether everything it owned was given back.</summary>
    internal sealed class ProbeOutcome
    {
        public ProbeOutcome(string owner, object result, string testFailure, CleanupReport cleanup)
        {
            Owner = owner;
            Result = result;
            TestFailure = testFailure;
            Cleanup = cleanup ?? new CleanupReport();
        }

        /// <summary>The step that started the probe. Its result belongs under this identity and no other.</summary>
        public string Owner { get; private set; }

        public object Result { get; private set; }

        /// <summary>Null or empty when the probe's own checks all held.</summary>
        public string TestFailure { get; private set; }

        public CleanupReport Cleanup { get; private set; }

        public bool IsolationRestored
        {
            get { return Cleanup.Restored; }
        }

        /// <summary>A probe passes only when its checks held and everything it owned was given back.</summary>
        public bool Passed
        {
            get { return string.IsNullOrEmpty(TestFailure) && IsolationRestored; }
        }

        /// <summary>Null when the probe passed; otherwise the probe's own failure followed by every cleanup failure.</summary>
        public string Failure
        {
            get
            {
                if (Passed) return null;
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(TestFailure)) parts.Add(TestFailure);
                foreach (string failure in Cleanup.Failures) parts.Add("cleanup failed: " + failure);
                return string.Join("; ", parts.ToArray());
            }
        }
    }

    internal enum StepContinuation
    {
        Advance,
        StopLaunch
    }

    /// <summary>
    /// The one place a running probe is held. A step starts a probe under its own identity, and only that step may poll
    /// it. However the step ends, the probe is terminated before another step is considered, and its outcome stays with
    /// the step that started it. When a probe could not give back what it owned, no further probe may start.
    /// </summary>
    internal sealed class ProbeSlot
    {
        private IOwnedProbe probe;
        private string owner;
        private ProbeOutcome last;
        private string unrestoredOwner;

        public bool IsActive
        {
            get { return probe != null; }
        }

        public string Owner
        {
            get { return owner; }
        }

        public bool IsolationRestored
        {
            get { return unrestoredOwner == null; }
        }

        public void Begin(string stepIdentity, IOwnedProbe candidate)
        {
            if (string.IsNullOrEmpty(stepIdentity)) throw new ArgumentException("a probe needs the identity of the step that owns it", "stepIdentity");
            if (candidate == null) throw new ArgumentNullException("candidate");
            if (probe != null)
                throw new InvalidOperationException("step " + stepIdentity + " cannot start a probe while the probe of step " + owner + " is still active");
            if (unrestoredOwner != null)
                throw new InvalidOperationException("step " + stepIdentity + " cannot start a probe: isolation was not restored after step " + unrestoredOwner);
            probe = candidate;
            owner = stepIdentity;
        }

        /// <summary>
        /// Polls the probe for the step that owns it. Null while it runs. When the probe reports a verdict, or throws,
        /// it is terminated here and its outcome returned.
        /// </summary>
        public ProbeOutcome Poll(string stepIdentity)
        {
            if (probe == null) throw new InvalidOperationException("step " + stepIdentity + " has no active probe");
            if (!string.Equals(owner, stepIdentity, StringComparison.Ordinal))
                throw new InvalidOperationException("step " + stepIdentity + " asked for the probe of step " + owner);
            string verdict;
            try
            {
                verdict = probe.Poll();
            }
            catch (Exception exception)
            {
                return Terminate(stepIdentity, "the probe threw: " + exception);
            }

            return verdict == null ? null : Terminate(stepIdentity, verdict);
        }

        /// <summary>
        /// Ends the probe the step owns and releases what it holds. Safe to call again: a step whose probe has already
        /// ended gets the same outcome back and nothing is released twice. A step that never owned a probe gets null.
        /// </summary>
        public ProbeOutcome Terminate(string stepIdentity, string testFailure)
        {
            if (probe != null && string.Equals(owner, stepIdentity, StringComparison.Ordinal)) return End(testFailure);
            return last != null && string.Equals(last.Owner, stepIdentity, StringComparison.Ordinal) ? last : null;
        }

        /// <summary>Ends whatever probe is active, whoever owns it. Null when none is.</summary>
        public ProbeOutcome Abandon(string testFailure)
        {
            return probe == null ? null : End(testFailure);
        }

        private ProbeOutcome End(string testFailure)
        {
            IOwnedProbe ending = probe;
            string endingOwner = owner;
            probe = null;
            owner = null;
            CleanupReport report;
            try
            {
                report = ending.Release(testFailure) ?? new CleanupReport();
            }
            catch (Exception exception)
            {
                report = new CleanupReport();
                report.Attempted.Add("release");
                report.Failures.Add("release: " + exception.GetType().Name + ": " + exception.Message);
            }

            object recorded;
            try
            {
                recorded = ending.Result;
            }
            catch (Exception exception)
            {
                recorded = null;
                report.Failures.Add("result: " + exception.GetType().Name + ": " + exception.Message);
            }

            last = new ProbeOutcome(endingOwner, recorded, testFailure, report);
            if (!report.Restored) unrestoredOwner = endingOwner;
            return last;
        }

        /// <summary>
        /// Whether the launch may go on to its next step. A failed step goes on only when it says so, and no step goes
        /// on while something an earlier probe owned is still in the game.
        /// </summary>
        public static StepContinuation Decide(bool stepFailed, bool continueOnFailure, bool isolationRestored)
        {
            if (!isolationRestored) return StepContinuation.StopLaunch;
            return stepFailed && !continueOnFailure ? StepContinuation.StopLaunch : StepContinuation.Advance;
        }
    }
}
