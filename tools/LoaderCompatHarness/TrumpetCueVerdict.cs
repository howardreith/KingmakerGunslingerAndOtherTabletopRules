using System;
using System.Collections.Generic;
using System.Linq;

namespace KmgLoaderCompatHarness
{
    // Sprint 7 T7 / VIS-06: rules for the Trumpet cue reload probe that need no game (tests: sprint7-visual.trumpet-*).

    /// <summary>
    /// How the cue that was playing at the save ended. Only the product's own records count: the saved playing ID,
    /// its native end-of-event time, and for a retirement the product's stop reason and recorded stop result.
    /// </summary>
    internal static class TrumpetCueVerdict
    {
        internal const string Natural = "natural";

        /// <returns>"natural", "retired:&lt;reason&gt;", or "invalid:&lt;why&gt;".</returns>
        internal static string SavedCue(uint savedPlayingId, uint recordedPlayingId, DateTime postedUtc, DateTime? endUtc,
            string stopReason, string stopResult, DateTime? stopRequestedUtc, string productStopRequested)
        {
            if (savedPlayingId == 0 || recordedPlayingId != savedPlayingId) return "invalid:playing-id";
            if (!endUtc.HasValue) return "invalid:no-native-end";
            if (endUtc.Value < postedUtc) return "invalid:end-before-post";
            if (stopReason == null)
                return stopResult == null && !stopRequestedUtc.HasValue ? Natural : "invalid:stop-without-reason";
            if (string.IsNullOrEmpty(productStopRequested) || stopResult != productStopRequested) return "invalid:stop-not-requested";
            if (!stopRequestedUtc.HasValue || stopRequestedUtc.Value < postedUtc || stopRequestedUtc.Value > endUtc.Value)
                return "invalid:stop-order";
            return "retired:" + stopReason;
        }

        internal static bool Accepted(string verdict)
        {
            return verdict == Natural || (verdict != null && verdict.StartsWith("retired:", StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// What a probe changed in the running game, each with its own restore. Released once, newest first, on every exit
    /// (normal end, failure, timeout or abandonment while the probe is yielded); a restore that throws is recorded and
    /// the others still run. Every release's outcome, whichever drain ran it (an early release, the iterator's finally
    /// or the slot's Release), is kept in <see cref="Outcome"/>, so a failure seen by one drain is never lost to a
    /// later, empty one (review R3-F2).
    /// </summary>
    internal sealed class OwnedProbeResources
    {
        private readonly List<KeyValuePair<string, Action>> held = new List<KeyValuePair<string, Action>>();

        /// <summary>Every release attempted so far and every failure among them.</summary>
        internal readonly CleanupReport Outcome = new CleanupReport();

        internal int Count { get { return held.Count; } }

        internal void Hold(string name, Action restore)
        {
            held.Add(new KeyValuePair<string, Action>(name, restore));
        }

        /// <summary>Releases one resource early (for example the seeding after its blast), once.</summary>
        internal void ReleaseNow(string name, List<string> failures)
        {
            int index = held.FindLastIndex(candidate => candidate.Key == name);
            if (index < 0) return;
            KeyValuePair<string, Action> entry = held[index];
            held.RemoveAt(index);
            CleanupReport report = ProbeCleanup.RunAll(new[] { entry });
            Record(report);
            if (failures != null) failures.AddRange(report.Failures);
        }

        internal CleanupReport ReleaseAll()
        {
            var releases = new List<KeyValuePair<string, Action>>(held);
            releases.Reverse();
            held.Clear();
            CleanupReport report = ProbeCleanup.RunAll(releases);
            Record(report);
            return report;
        }

        /// <summary>Runs releases the probe never held (its final releases), each once, keeping their outcome too.</summary>
        internal CleanupReport Run(IEnumerable<KeyValuePair<string, Action>> releases)
        {
            CleanupReport report = ProbeCleanup.RunAll(releases);
            Record(report);
            return report;
        }

        private void Record(CleanupReport report)
        {
            Outcome.Attempted.AddRange(report.Attempted);
            Outcome.Failures.AddRange(report.Failures);
        }
    }

    /// <summary>
    /// The Poll/Release protocol both yielded harness probes (Trumpet, Ghaele) share (review R3-F2). Release disposes the
    /// iterator (its finally blocks may drain the held state), drains whatever is still held, then builds and runs the
    /// probe's own final releases from what it still owns at that point (review R5-F5: nothing released by the
    /// iterator's cleanup is released again). The report it returns is the probe's one cumulative outcome, read last:
    /// every held release from whichever drain ran it, every direct destruction request and witness settlement made
    /// earlier (owner amendment 3, R6-F1) and the final releases, so a failure anywhere reaches ProbeSlot
    /// (IsolationRestored=false) and the launch stops. Each release runs once.
    /// </summary>
    internal sealed class YieldedProbeLifecycle
    {
        private readonly IEnumerator<object> script;
        private string verdict;

        internal readonly OwnedProbeResources Owned;
        internal readonly List<string> Problems = new List<string>();

        internal YieldedProbeLifecycle(OwnedProbeResources owned, Func<YieldedProbeLifecycle, IEnumerable<object>> body)
        {
            Owned = owned;
            script = body(this).GetEnumerator();
        }

        internal bool Ended { get { return verdict != null; } }

        /// <summary>Null while the iterator runs; then empty, or the problems joined.</summary>
        internal string Poll()
        {
            if (verdict != null) return verdict;
            bool more;
            try { more = script.MoveNext(); }
            catch (Exception exception) { Problems.Add("probe exception: " + exception); more = false; }
            if (more) return null;
            verdict = Problems.Count == 0 ? string.Empty : string.Join(" | ", Problems.ToArray());
            return verdict;
        }

        internal CleanupReport Release(Func<IEnumerable<KeyValuePair<string, Action>>> finalReleases)
        {
            var report = new CleanupReport();
            report.Attempted.Add("probe iterator");
            try { script.Dispose(); }
            catch (Exception exception) { report.Failures.Add("probe iterator: " + exception.GetType().Name + ": " + exception.Message); }
            Owned.ReleaseAll();
            try { Owned.Run(finalReleases == null ? Enumerable.Empty<KeyValuePair<string, Action>>() : finalReleases().ToList()); }
            catch (Exception exception)
            {
                Owned.Outcome.Attempted.Add("final releases");
                Owned.Outcome.Failures.Add("final releases: " + exception.GetType().Name + ": " + exception.Message);
            }
            report.Attempted.AddRange(Owned.Outcome.Attempted);
            report.Failures.AddRange(Owned.Outcome.Failures);
            foreach (string failure in report.Failures)
                if (!Problems.Contains("cleanup failed: " + failure)) Problems.Add("cleanup failed: " + failure);
            return report;
        }
    }

    /// <summary>
    /// Review R2-F2: which saving throws the Trumpet probe treats as the observed blast's own. A blast save is one the
    /// blast's own action triggers: no other saving throw is below it on the rule stack. A save dispatched from inside
    /// another save's callbacks inherits the blast's rule context but is not the blast's. The probe marks only such a
    /// save as passed through the save's own AutoPass field; it never touches a d20 rule or the shared generator.
    /// </summary>
    internal static class TrumpetSaveOwnership
    {
        internal static bool TopLevelSave(IEnumerable<object> ruleStack, object save, Func<object, bool> isSave)
        {
            return save != null && !(ruleStack ?? Enumerable.Empty<object>()).Any(rule => rule != null && !ReferenceEquals(rule, save) && isSave(rule));
        }

        /// <summary>
        /// The decisive R2-F2 interval: a foreign d20 dispatched directly inside the owned save before its native roll.
        /// Each value is compared with an untreated generator copy drawn in the same order: the foreign die, the owned
        /// save's own die (rolled natively, not forced) and the next unrelated draw; the owned save passed through its
        /// AutoPass alone and the foreign die was never pre-rolled.
        /// </summary>
        internal static string NestedForeignVerdict(bool insideOwnedSave, int foreignD20, int controlForeignD20, bool foreignPreRolled,
            int ownedSaveD20, int controlOwnedD20, bool ownedAutoPass, bool ownedPassed, bool followingMatches)
        {
            if (!insideOwnedSave) return "not-inside-owned-save";
            if (foreignPreRolled || foreignD20 != controlForeignD20) return "foreign-d20-changed";
            if (!ownedAutoPass || !ownedPassed) return "owned-save-not-passed";
            if (ownedSaveD20 != controlOwnedD20) return "owned-save-die-not-natural";
            if (!followingMatches) return "following-draw-changed";
            return null;
        }
    }

    /// <summary>
    /// Review R5-F5: the disposable units a probe created, each with one destruction-request owner. A request is
    /// recorded before it is made, so no later drain requests the same unit again, whether or not native destruction
    /// (which completes after the call) has finished; final releases are built only for units never requested. Owner
    /// amendment 3 (R6-F1): every request, direct or from a release, is recorded in the probe's one cumulative outcome
    /// under the unit's identity, and a request that throws is kept there as a named failure (and rethrown to its
    /// caller) even though the unit stays marked requested.
    /// </summary>
    internal sealed class OwnedDisposables<T> where T : class
    {
        private readonly List<T> items = new List<T>();
        private readonly List<T> requested = new List<T>();
        private readonly string name;
        private readonly Func<T, string> identity;
        private readonly Action<T> destroy;
        private readonly CleanupReport outcome;

        internal OwnedDisposables(string name, Func<T, string> identity, Action<T> destroy, CleanupReport outcome)
        {
            this.name = name;
            this.identity = identity;
            this.destroy = destroy;
            this.outcome = outcome ?? new CleanupReport();
        }

        private static bool Has(List<T> list, T item) { return list.Any(value => ReferenceEquals(value, item)); }

        internal void Add(T item) { if (item != null && !Has(items, item)) items.Add(item); }

        internal bool WasRequested(T item) { return item != null && Has(requested, item); }

        /// <summary>Requests destruction once; returns false (and does nothing) when it was already requested.</summary>
        internal bool Destroy(T item)
        {
            if (item == null || Has(requested, item)) return false;
            requested.Add(item);
            string label = name + " " + identity(item);
            outcome.Attempted.Add(label);
            try { destroy(item); }
            catch (Exception exception)
            {
                outcome.Failures.Add(label + ": " + exception.GetType().Name + ": " + exception.Message);
                throw;
            }
            return true;
        }

        /// <summary>Owned units never requested.</summary>
        internal List<T> Remaining { get { return items.Where(item => !Has(requested, item)).ToList(); } }

        /// <summary>
        /// The final release: one request per owned unit still unrequested, made when it runs. A failed request is
        /// already kept in the outcome under its identity; the others still run.
        /// </summary>
        internal void ReleaseRemaining()
        {
            foreach (T item in Remaining)
                try { Destroy(item); }
                catch (Exception) { }
        }
    }

    /// <summary>
    /// Review R4-F7: one release per owned item, so a failure on one item is reported under its own identity and every
    /// other item still gets its attempt.
    /// </summary>
    internal static class ProbeReleases
    {
        internal static List<KeyValuePair<string, Action>> PerItem<T>(string name, IEnumerable<T> items, Func<T, string> identity, Action<T> release)
        {
            var list = new List<KeyValuePair<string, Action>>();
            foreach (T item in (items ?? Enumerable.Empty<T>()).ToList())
            {
                T owned = item;
                list.Add(new KeyValuePair<string, Action>(name + " " + identity(owned), () => release(owned)));
            }
            return list;
        }
    }
}
