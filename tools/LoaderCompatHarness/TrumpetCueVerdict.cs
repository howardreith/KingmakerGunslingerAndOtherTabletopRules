using System;
using System.Collections.Generic;

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

        private void Record(CleanupReport report)
        {
            Outcome.Attempted.AddRange(report.Attempted);
            Outcome.Failures.AddRange(report.Failures);
        }
    }

    /// <summary>
    /// The Poll/Release protocol both yielded harness probes (Trumpet, Ghaele) share (review R3-F2). Release disposes the
    /// iterator (its finally blocks may drain the held state), drains whatever is still held, then runs the probe's own
    /// final releases; the report it returns carries every held release's outcome from whichever drain ran it, so a
    /// failed restore reaches ProbeSlot (IsolationRestored=false) and the launch stops. Each release runs once.
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

        internal CleanupReport Release(IEnumerable<KeyValuePair<string, Action>> finalReleases)
        {
            var report = new CleanupReport();
            report.Attempted.Add("probe iterator");
            try { script.Dispose(); }
            catch (Exception exception) { report.Failures.Add("probe iterator: " + exception.GetType().Name + ": " + exception.Message); }
            Owned.ReleaseAll();
            report.Attempted.AddRange(Owned.Outcome.Attempted);
            report.Failures.AddRange(Owned.Outcome.Failures);
            CleanupReport rest = ProbeCleanup.RunAll(finalReleases);
            report.Attempted.AddRange(rest.Attempted);
            report.Failures.AddRange(rest.Failures);
            foreach (string failure in report.Failures)
                if (!Problems.Contains("cleanup failed: " + failure)) Problems.Add("cleanup failed: " + failure);
            return report;
        }
    }

    /// <summary>
    /// A shared-generator intervention confined to one owned rule (review R2-F2). Begin, at the owned rule's
    /// about-to-trigger, saves the generator and seeds it; End, at that same rule's did-trigger, puts the saved state back.
    /// Draws made outside the owned rule (before it, after it, or by other work while a command yields) are never seeded
    /// and never rewound; only the owned rule's own draws are replaced. Abandon ends an open intervention.
    /// </summary>
    internal sealed class OwnedRollScope<TState>
    {
        private readonly Func<TState> read;
        private readonly Action<TState> write;
        private readonly Action seed;
        private object rule;
        private TState saved;

        internal OwnedRollScope(Func<TState> read, Action<TState> write, Action seed)
        {
            this.read = read; this.write = write; this.seed = seed;
        }

        internal int Seeded { get; private set; }

        internal bool Active { get { return rule != null; } }

        internal bool Begin(object ownedRule)
        {
            if (ownedRule == null || rule != null) return false;
            saved = read();
            rule = ownedRule;
            seed();
            Seeded++;
            return true;
        }

        internal bool End(object ownedRule)
        {
            if (rule == null || !ReferenceEquals(rule, ownedRule)) return false;
            rule = null;
            write(saved);
            return true;
        }

        internal void Abandon()
        {
            if (rule == null) return;
            rule = null;
            write(saved);
        }
    }
}
