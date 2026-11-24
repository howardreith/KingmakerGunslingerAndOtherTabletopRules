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
    /// the others still run.
    /// </summary>
    internal sealed class OwnedProbeResources
    {
        private readonly List<KeyValuePair<string, Action>> held = new List<KeyValuePair<string, Action>>();

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
            try { if (entry.Value != null) entry.Value(); }
            catch (Exception exception) { failures.Add(entry.Key + ": " + exception.GetType().Name + ": " + exception.Message); }
        }

        internal CleanupReport ReleaseAll()
        {
            var releases = new List<KeyValuePair<string, Action>>(held);
            releases.Reverse();
            held.Clear();
            return ProbeCleanup.RunAll(releases);
        }
    }
}
