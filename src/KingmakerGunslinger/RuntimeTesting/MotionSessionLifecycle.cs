using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The deliberate outer abort a motion request may ask for: the runner
    /// ends the request with a failure of its own while the motion session
    /// is sampling an attack with the pause released. It exists to exercise
    /// the runner's finalization of the session; a request that asks for it
    /// can never pass.
    /// </summary>
    internal static class MotionOuterAbortDrill
    {
        internal const string Parameter = "outerAbortDrill";
        internal const string AttackSampling = "attack-sampling";
        internal const string Rejected = "motion-outer-abort-drill-invalid";
        /// <summary>The attack has been sampled for this many updates before the abort.</summary>
        internal const int SamplingUpdatesBeforeAbort = 8;
        internal const string Reason =
            "Deliberate outer-abort drill: the runner ends this request while the motion " +
            "session is sampling an attack with the pause released. This failure is intended.";
        internal const string NotReachedName = "weapon-presentation-motion-outer-abort-drill";

        /// <summary>The drill and the working save's name, which every motion request carries.</summary>
        internal const int RequestParameterCount = 2;

        /// <summary>
        /// The reason a request is refused, or null. The drill is its own
        /// request: it stands beside the working save's name and nothing
        /// else, it names the one supported point, it belongs to a motion
        /// scenario, and the process must exit when it completes. The save
        /// name itself is checked where it always is.
        /// </summary>
        internal static string Rejection(bool motionScenario, bool parameterPresent,
            string value, int parameterCount, bool exitAfterCompletion)
        {
            if (!parameterPresent) return null;
            return motionScenario && parameterCount == RequestParameterCount &&
                string.Equals(value, AttackSampling, StringComparison.Ordinal) &&
                exitAfterCompletion ? null : Rejected;
        }

        /// <summary>The runner aborts now.</summary>
        internal static bool Due(bool requested, bool sessionEnded, bool sampling,
            bool pauseOwned, bool pausedNow, int samplingUpdates)
        {
            return requested && !sessionEnded && sampling && pauseOwned &&
                !pausedNow && samplingUpdates >= SamplingUpdatesBeforeAbort;
        }
    }

    /// <summary>What ending a motion session from outside did.</summary>
    internal sealed class MotionTermination
    {
        /// <summary>False when the session had already ended; nothing was done or added.</summary>
        internal bool Performed;
        internal string Stage;
        /// <summary>An attack sequence was being sampled when the session was ended.</summary>
        internal bool Sampling;
        internal int SamplingUpdates;
        /// <summary>The session had found the game paused and released it.</summary>
        internal bool PauseOwned;
        internal bool? PausedAtTermination;
        internal bool? PausedAfterwards;
        internal string PauseFault;
        /// <summary>The session's own cleanup had already run; only the pause was looked at again.</summary>
        internal bool CleanupAlreadyRun;
        internal IList<string> Attempted = new List<string>();
        internal IList<string> Failures = new List<string>();
        /// <summary>What the session created that is still there, or null.</summary>
        internal string Residue;
        /// <summary>How many assertions the outer result received.</summary>
        internal int Attached;
        internal bool PauseRestored;
        internal bool CleanupClean;

        /// <summary>The status the outer result keeps: its own, unless that was a pass.</summary>
        internal static string StatusAfter(string status, string pass, string fail)
        {
            return string.Equals(status, pass, StringComparison.Ordinal) ? fail : status;
        }
    }

    /// <summary>
    /// What a motion session owns besides its evidence, and the one place
    /// that gives it back: the pause it releases while an attack is sampled,
    /// and the releases of the units, items and blueprints it created.
    ///
    /// The attack animation advances on scaled time, which stands still
    /// while the game is paused. A session that finds the game paused at an
    /// attack sample takes ownership of the pause: it releases it for each
    /// attack sequence, holds it between them, and puts it back when it
    /// ends. A session that finds the game running owns nothing and changes
    /// nothing.
    ///
    /// The session ends in one of two ways. It runs to its own result
    /// (<see cref="RunCleanup"/>, then <see cref="MarkCompleted"/>), or the
    /// runner completes with a result of its own while the session is still
    /// running (<see cref="Terminate"/>). Either way each release is
    /// attempted on its own, the pause first, and nothing is done twice.
    /// </summary>
    internal sealed class MotionSessionLifecycle<TAssertion>
    {
        internal const string TerminatedName = "weapon-presentation-motion-terminated";
        internal const string PauseName = "weapon-presentation-motion-pause-restored";
        internal const string CleanupName = "weapon-presentation-motion-termination-cleanup";
        internal const string ResidueName = "weapon-presentation-motion-termination-residue";
        internal const string RestorePauseRelease = "restore-pause";

        private readonly Func<bool> _isPaused;
        private readonly Action<bool> _setPaused;
        private readonly Func<string, string, string, bool, TAssertion> _assertion;
        // The per-release isolation the Dodge scenario's cleanup established.
        private readonly DodgeReleases _releases = new DodgeReleases();

        /// <param name="isPaused">Reads the game's pause.</param>
        /// <param name="setPaused">Sets the game's pause.</param>
        /// <param name="assertion">Builds an assertion from a name, what was expected, what was observed and whether it holds.</param>
        internal MotionSessionLifecycle(Func<bool> isPaused, Action<bool> setPaused,
            Func<string, string, string, bool, TAssertion> assertion)
        {
            if (isPaused == null) throw new ArgumentNullException("isPaused");
            if (setPaused == null) throw new ArgumentNullException("setPaused");
            if (assertion == null) throw new ArgumentNullException("assertion");
            _isPaused = isPaused;
            _setPaused = setPaused;
            _assertion = assertion;
        }

        internal bool Sampling { get; private set; }
        /// <summary>Updates of the attack sequence being sampled.</summary>
        internal int SamplingUpdates { get; private set; }
        internal bool PauseOwned { get; private set; }
        internal int PauseReleases { get; private set; }
        internal int PauseRestorations { get; private set; }
        internal bool CleanupRun { get; private set; }
        /// <summary>The session has its own result, or was ended from outside.</summary>
        internal bool Ended { get; private set; }
        internal bool Terminated { get; private set; }
        internal IList<string> CleanupAttempted { get { return _releases.Attempted; } }
        internal IList<string> CleanupFailures { get { return _releases.Failures; } }

        internal string DescribeCleanup()
        {
            return _releases.Describe();
        }

        /// <summary>
        /// Called on every update that samples a running attack. Lets time
        /// run if the game is paused. Returns true the one time the session
        /// takes ownership of the pause.
        /// </summary>
        internal bool ReleaseForSampling()
        {
            if (Ended || CleanupRun) return false;
            Sampling = true;
            SamplingUpdates++;
            if (!_isPaused()) return false;
            bool first = !PauseOwned;
            PauseOwned = true;
            _setPaused(false);
            PauseReleases++;
            return first;
        }

        /// <summary>
        /// Called on every update outside an attack sequence. Holds a pause
        /// the session owns; left running, the game acts on the newly
        /// equipped weapon itself.
        /// </summary>
        internal void HoldOutsideSampling()
        {
            Sampling = false;
            SamplingUpdates = 0;
            if (Ended || !PauseOwned) return;
            if (!_isPaused()) _setPaused(true);
        }

        /// <summary>
        /// The session's owned cleanup, once: the pause first, then each
        /// release on its own. Returns false when it had already run.
        /// </summary>
        internal bool RunCleanup(IEnumerable<KeyValuePair<string, Action>> releases)
        {
            if (CleanupRun) return false;
            CleanupRun = true;
            Sampling = false;
            _releases.Run(RestorePauseRelease, RestorePause);
            if (releases == null) return true;
            foreach (KeyValuePair<string, Action> release in releases)
                if (release.Value != null) _releases.Run(release.Key, release.Value);
            return true;
        }

        /// <summary>The session produced its own result; ending it again does nothing.</summary>
        internal void MarkCompleted()
        {
            Ended = true;
        }

        /// <summary>
        /// The runner is completing with a result that is not the session's.
        /// Restores a pause the session released, runs the cleanup if it has
        /// not run, reads what remains, and adds to the outer result what
        /// the session had asserted, what ending it did, and what it had
        /// observed. The outer result's own entries stay first and are not
        /// changed; its status and reason are not this method's to touch.
        /// Does nothing when the session has already ended.
        /// </summary>
        /// <param name="stage">Where the session was.</param>
        /// <param name="releases">The session's owned releases.</param>
        /// <param name="residue">Returns what the session created that is still there, or null.</param>
        /// <param name="observations">What the session had observed so far.</param>
        /// <param name="sessionAssertions">What the session had asserted so far.</param>
        /// <param name="resultAssertions">The outer result's assertions.</param>
        /// <param name="resultDiagnostics">The outer result's diagnostics.</param>
        internal MotionTermination Terminate(string stage,
            IEnumerable<KeyValuePair<string, Action>> releases, Func<string> residue,
            Func<IEnumerable<string>> observations, IList<TAssertion> sessionAssertions,
            IList<TAssertion> resultAssertions, IList<string> resultDiagnostics)
        {
            var termination = new MotionTermination
            {
                Stage = string.IsNullOrEmpty(stage) ? "<unknown>" : stage,
                Sampling = Sampling, SamplingUpdates = SamplingUpdates,
                PauseOwned = PauseOwned, CleanupAlreadyRun = CleanupRun
            };
            if (Ended) return termination;
            Ended = true;
            Terminated = true;
            termination.Performed = true;

            string faultBefore, faultAfter;
            termination.PausedAtTermination = ReadPause(out faultBefore);
            // The pause comes first and does not depend on anything else.
            if (CleanupRun) _releases.Run(RestorePauseRelease, RestorePause);
            else RunCleanup(releases);
            termination.PausedAfterwards = ReadPause(out faultAfter);
            termination.PauseFault = faultBefore ?? faultAfter;
            foreach (string name in _releases.Attempted) termination.Attempted.Add(name);
            foreach (string failure in _releases.Failures) termination.Failures.Add(failure);

            string residueFault = null;
            if (residue != null)
                try { termination.Residue = residue(); }
                catch (Exception error) { residueFault = error.Message; }

            var checks = new List<TAssertion>();
            checks.Add(_assertion(TerminatedName, "the session runs to its own end",
                "the runner completed with another result while the session was at stage " +
                    termination.Stage + ";sampling=" + termination.Sampling +
                    ";samplingUpdates=" + termination.SamplingUpdates +
                    ";cleanupAlreadyRun=" + termination.CleanupAlreadyRun, false));

            termination.PauseRestored = termination.PauseFault == null &&
                (termination.PauseOwned
                    ? termination.PausedAfterwards == true
                    : termination.PausedAfterwards == termination.PausedAtTermination);
            checks.Add(_assertion(PauseName,
                "a pause the session released is in force again; a game the session found running is left as it was",
                "owned=" + termination.PauseOwned + ";pausedAtTermination=" +
                    Describe(termination.PausedAtTermination) + ";pausedAfterwards=" +
                    Describe(termination.PausedAfterwards) + ";releases=" + PauseReleases +
                    ";restorations=" + PauseRestorations +
                    (termination.PauseFault == null ? string.Empty
                        : ";unreadable=" + termination.PauseFault),
                termination.PauseRestored));

            termination.CleanupClean = termination.Failures.Count == 0;
            checks.Add(_assertion(CleanupName,
                "every release the session owns is attempted and none raises",
                _releases.Describe(), termination.CleanupClean));

            checks.Add(_assertion(ResidueName,
                "nothing the session created remains, read once at termination",
                residueFault != null ? "unreadable=" + residueFault :
                    termination.Residue == null ? "none" : termination.Residue,
                residueFault == null && termination.Residue == null));

            if (resultAssertions != null)
            {
                if (sessionAssertions != null &&
                    !ReferenceEquals(sessionAssertions, resultAssertions))
                    foreach (TAssertion assertion in sessionAssertions)
                    {
                        resultAssertions.Add(assertion);
                        termination.Attached++;
                    }
                foreach (TAssertion check in checks)
                {
                    resultAssertions.Add(check);
                    termination.Attached++;
                }
            }
            if (resultDiagnostics != null)
            {
                resultDiagnostics.Add("motionTermination=the runner completed before the " +
                    "motion session finished (stage " + termination.Stage +
                    "); what the session had observed and what ending it did were added " +
                    "to this result, after the result's own entries");
                if (observations != null)
                    try
                    {
                        foreach (string observation in observations() ?? new string[0])
                            resultDiagnostics.Add("motionObservation:" + observation);
                    }
                    catch (Exception error)
                    {
                        resultDiagnostics.Add("motionObservation:unreadable=" +
                            error.Message);
                    }
            }
            return termination;
        }

        private void RestorePause()
        {
            if (!PauseOwned || _isPaused()) return;
            _setPaused(true);
            PauseRestorations++;
        }

        private bool? ReadPause(out string fault)
        {
            fault = null;
            try { return _isPaused(); }
            catch (Exception error)
            {
                fault = error.Message;
                return null;
            }
        }

        private static string Describe(bool? value)
        {
            return value.HasValue ? value.Value.ToString() : "<unreadable>";
        }
    }
}
