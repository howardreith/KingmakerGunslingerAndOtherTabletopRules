using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 7 review observers act only on what they acquired: a description
    /// window opens only when no foreign window, hover or modal is showing; the
    /// window is the observer's only while it shows the exact data object the
    /// observer built; a fact, buff or effect is released only as the exact
    /// instance the observer itself created, and only while it is still that
    /// instance; every release is attempted and a failed one is reported
    /// beside the original failure, never in its place.
    /// </summary>
    internal static partial class Sprint7ObserverPolicy
    {
        internal static bool CanOpenWindow(bool windowOpen, bool hoverOpen, bool modalOpen)
        {
            return !windowOpen && !hoverOpen && !modalOpen;
        }

        internal static bool OwnsWindow(object shown, object opened)
        {
            return opened != null && ReferenceEquals(shown, opened);
        }

        /// <summary>Grant only when the unit does not already hold the fact.</summary>
        internal static bool ShouldGrant(bool presentBefore)
        {
            return !presentBefore;
        }

        /// <summary>
        /// Release a grant only if the observer made it and the unit still holds
        /// that exact instance (a replacement or an original is left alone).
        /// </summary>
        internal static bool ShouldRelease(object granted, object current)
        {
            return granted != null && ReferenceEquals(granted, current);
        }

        /// <summary>The present instances that are exactly ones the observer acquired.</summary>
        internal static List<T> Acquired<T>(IEnumerable<T> present, IEnumerable<T> acquired) where T : class
        {
            var mine = new List<T>(acquired ?? Enumerable.Empty<T>());
            return (present ?? Enumerable.Empty<T>()).Where(value => value != null && mine.Any(own => ReferenceEquals(own, value))).ToList();
        }

        /// <summary>
        /// A held observer resource is released only when every release ran
        /// without failure and none of the observer's own hooks remains.
        /// </summary>
        internal static bool Released(ICollection<string> failures, int remainingOwned)
        {
            return (failures == null || failures.Count == 0) && remainingOwned == 0;
        }

        /// <summary>
        /// R1-F3: a gameplay-distance party capture counts only when the shared
        /// helper wrote an image (png named, not &lt;none&gt;), the unit was in frame,
        /// the screen was lit with the native fade cleared, the helper's renderer
        /// is enabled and carries the expected body mesh, and the body is intact.
        /// </summary>
        internal static bool PartyCaptureValid(string readback, string expectedMesh, out string reason)
        {
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string part in (readback ?? string.Empty).Split(';'))
            {
                int equals = part.IndexOf('=');
                if (equals > 0 && !fields.ContainsKey(part.Substring(0, equals))) fields[part.Substring(0, equals)] = part.Substring(equals + 1);
            }
            Func<string, string> field = name => fields.ContainsKey(name) ? fields[name] : null;
            double fade;
            if (field("png") == null || field("png") == "<none>") reason = "no-image" + (field("reason") == null ? string.Empty : ":" + field("reason"));
            else if (field("inFrame") != "true") reason = "out-of-frame";
            else if (field("screenLit") != "true") reason = "unlit";
            else if (!double.TryParse(field("screenFade"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fade) || fade > .05) reason = "screen-fade";
            else if (field("rendererEnabled") != "true") reason = "renderer-disabled";
            else if (string.IsNullOrEmpty(expectedMesh) || field("mesh") != expectedMesh) reason = "wrong-body";
            else if (field("intact") != "true") reason = "dissolved";
            else reason = null;
            return reason == null;
        }

        /// <summary>Runs every release; a release that throws is recorded and the rest still run.</summary>
        internal static List<string> RunReleases(IEnumerable<KeyValuePair<string, Action>> releases)
        {
            var failures = new List<string>();
            foreach (KeyValuePair<string, Action> release in releases ?? Enumerable.Empty<KeyValuePair<string, Action>>())
            {
                try { if (release.Value != null) release.Value(); }
                catch (Exception error) { failures.Add(release.Key + ": " + error.GetType().Name + ": " + error.Message); }
            }
            return failures;
        }

        /// <summary>The failure a partly built observer reports: the original cause, with any incomplete rollback.</summary>
        internal static InvalidOperationException SetupFailure(string observer, Exception original, IList<string> rollbackFailures, int remainingOwned)
        {
            string rollback = (rollbackFailures == null || rollbackFailures.Count == 0) && remainingOwned == 0
                ? "rollback complete"
                : "rollback INCOMPLETE: " + remainingOwned + " owned item(s) remain; " + string.Join("; ", (rollbackFailures ?? new List<string>()).ToArray());
            return new InvalidOperationException(observer + " setup failed (" + rollback + "): " +
                (original == null ? "unknown" : original.GetType().Name + ": " + original.Message), original);
        }
    }
    /// <summary>
    /// R4-F5: native playback of one cue's playing ID through the installed query (GetEventIDFromPlayingID: the event of
    /// a playing ID, 0 when it is not playing). Only an initialized engine that answered is evidence; an engine that is
    /// not initialized, a query that threw or a missing playing ID is unknown, never completion. A queried inactivity is
    /// recorded apart from the product's end-of-event callback.
    /// </summary>
    internal static partial class Sprint7ObserverPolicy
    {
        internal const string PlaybackPlaying = "playing";
        internal const string PlaybackInactive = "inactive";
        internal const string PlaybackUnknown = "unknown";

        internal static string PlaybackState(bool initialized, uint playingId, uint eventId, bool threw)
        {
            if (!initialized || threw || playingId == 0) return PlaybackUnknown;
            return eventId != 0 ? PlaybackPlaying : PlaybackInactive;
        }

        /// <summary>R4-F5, normal-callback control: the per-ID trace around a concurrent retirement, as a verdict.</summary>
        internal static string ConcurrentRetirementTrace(string retiringBefore, string survivorBefore, int survivorStopAttempts, bool survivorCallback,
            string survivorSettled, bool survivorTrackedSettled, string retiringSettled)
        {
            if (retiringBefore != PlaybackPlaying || survivorBefore != PlaybackPlaying) return "not-both-playing";
            if (survivorStopAttempts != 0) return "survivor-stopped-by-other-retirement";
            if (!survivorCallback) return "survivor-callback-missing";
            if (survivorSettled != PlaybackInactive) return "survivor-playback-not-confirmed-inactive:" + survivorSettled;
            if (survivorTrackedSettled) return "survivor-still-tracked";
            if (retiringSettled != PlaybackInactive) return "retired-playback-not-confirmed-inactive:" + retiringSettled;
            return null;
        }

        /// <summary>
        /// R4-F5: a retired source's cue was stopped by its own playing ID: one stop attempt, the product's recorded
        /// stop-requested result no earlier than the retirement and within a second of it, and the native end-of-event
        /// callback after the request and within a second of it. Read once settled: the product requests the stop in the
        /// retired view's own destroy callback, after the retirement call returns.
        /// </summary>
        internal static bool RetiredByOwnId(int stopAttempts, string stopResult, string productStopRequested, DateTime? stopRequestedUtc, DateTime? endUtc, DateTime retiredUtc)
        {
            return stopAttempts == 1 && !string.IsNullOrEmpty(productStopRequested) && stopResult == productStopRequested && stopRequestedUtc.HasValue &&
                stopRequestedUtc.Value >= retiredUtc && stopRequestedUtc.Value <= retiredUtc.AddSeconds(1) && endUtc.HasValue &&
                endUtc.Value >= stopRequestedUtc.Value && endUtc.Value <= stopRequestedUtc.Value.AddSeconds(1);
        }

        /// <summary>The callback-loss collector that produced a row: what it emitted decides what the evaluator may require.</summary>
        internal enum CueCollector
        {
            /// <summary>Round-6 collectors (f3bfab1303, 740d94a995): no explicit survivor stop count was emitted.</summary>
            Round6,
            /// <summary>From candidate 3 (9c39b1ff7f) on: emits otherRetirementSettled with survivorStopAttemptsThroughWindow.</summary>
            Round7
        }

        /// <summary>
        /// One cue record as the collector writes it (the runner's S7Cue delegates here). Absent values are written as
        /// explicit nulls; in memory they are String-typed JValues whose value is null, after a file round trip JSON nulls.
        /// </summary>
        internal static JObject CueRow(string eventName, uint playingId, int frame, DateTime utc, DateTime? endUtc, string stopReason, DateTime? stopRequestedUtc, string stopResult)
        {
            return new JObject { ["event"] = eventName, ["playingId"] = playingId, ["frame"] = frame, ["utc"] = utc.ToString("o"),
                ["endUtc"] = endUtc.HasValue ? endUtc.Value.ToString("o") : null, ["stopReason"] = stopReason,
                ["stopRequestedUtc"] = stopRequestedUtc.HasValue ? stopRequestedUtc.Value.ToString("o") : null,
                ["stopResult"] = stopResult, ["seconds"] = endUtc.HasValue ? (endUtc.Value - utc).TotalSeconds : (double?)null };
        }

        /// <summary>
        /// R4-F5 synthetic callback loss, the one settled evaluator (owner amendment 3 §4; recovery C1: R7-F1, R7-F6). It reads
        /// the callback-loss row exactly as the collector emitted it, in memory or read back from its file, with the other
        /// source's settled cue record and the retirement time; it is the in-game verdict and the reader of retained rows.
        /// A required property must be present: an absent or unreadable one is "missing:&lt;path&gt;", never evidence. A value
        /// required to be unset must be an explicit null in either representation. The selected survivor and other playing
        /// IDs must be valid and distinct, and the settled other record and both terminal survivor records must carry those
        /// same IDs. A Round7 row must carry the explicit survivor stop count through the window (absent is unknown, not
        /// zero); a Round6 row, whose collector emitted no count, is judged on the window-end cue's own stop fields. Null only
        /// when: both IDs were confirmed playing; the other source's retirement stopped its own ID; the survivor received no
        /// stop through the window; its callback was withheld and no end was recorded; the native query found it inactive in
        /// the window, never playing again, and inactive at both terminal reads (window end, after its own source's
        /// disposal); it was still tracked by its own source at the window end; that source's view and playback component
        /// were observed destroyed; no end was manufactured; no other record changed; and the observer's hook is gone. Stop
        /// requests, completion callbacks and native playback queries stay separate inputs.
        /// </summary>
        internal static string CallbackLossSettled(JObject row, JObject otherSettled, DateTime retiredUtc, string productStopRequested, CueCollector collector)
        {
            if (row == null) return "missing:row";
            JToken before = Child(row, "before"), windowEnd = Child(row, "survivorAtWindowEnd"), afterDisposal = Child(row, "survivorAfterDisposal");
            if (Text(before, "survivorPlayback") != PlaybackPlaying || Text(before, "otherPlayback") != PlaybackPlaying) return "not-both-playing";
            // R7-F6: the selected identities, bound to every later record of the same cue.
            long? survivorId = Id(Child(before, "survivor")), otherId = Id(Child(before, "other"));
            if (!survivorId.HasValue || !otherId.HasValue) return "missing:before.ids";
            if (survivorId.Value == otherId.Value) return "cue-identity-contradiction:survivor-is-other";
            if (otherSettled == null) return "missing:otherSettled";
            if (Id(otherSettled) != otherId) return "cue-identity-contradiction:other-settled";
            int? otherStops = Number(otherSettled, "stopAttempts");
            if (!otherStops.HasValue) return "missing:otherSettled.stopAttempts";
            if (otherStops != 1 || !RetiredByOwnId(otherStops.Value, Text(otherSettled, "stopResult"), productStopRequested,
                    Utc(Child(otherSettled, "stopRequestedUtc")), Utc(Child(otherSettled, "endUtc")), retiredUtc))
                return "other-not-stopped-by-own-id";
            JToken kept = Child(windowEnd, "cue");
            if (kept == null || kept.Type != JTokenType.Object) return "missing:survivorAtWindowEnd.cue";
            if (Id(kept) != survivorId) return "cue-identity-contradiction:survivor-window-end";
            // R7-F6: the explicit count where the collector emitted one; otherwise (Round6) the window-end stop fields.
            if (collector == CueCollector.Round7)
            {
                JToken settled = Child(row, "otherRetirementSettled");
                if (settled == null || settled.Type != JTokenType.Object) return "missing:otherRetirementSettled";
                if (Id(Child(settled, "other")) != otherId) return "cue-identity-contradiction:other-retirement-settled";
                int? survivorStops = Number(settled, "survivorStopAttemptsThroughWindow");
                if (!survivorStops.HasValue) return "missing:otherRetirementSettled.survivorStopAttemptsThroughWindow";
                if (survivorStops.Value != 0) return "survivor-stopped-by-other-retirement";
            }
            string unset = Unset(kept, "stopResult", "survivorAtWindowEnd.cue") ?? Unset(kept, "stopRequestedUtc", "survivorAtWindowEnd.cue");
            if (unset != null) return unset == "set" ? "survivor-stopped-by-other-retirement" : unset;
            JToken withheldToken = Child(windowEnd, "callbacksWithheld");
            if (withheldToken == null || withheldToken.Type != JTokenType.Array) return "missing:survivorAtWindowEnd.callbacksWithheld";
            if (((JArray)withheldToken).Count < 1) return "callback-not-withheld";
            unset = Unset(kept, "endUtc", "survivorAtWindowEnd.cue");
            if (unset != null) return unset == "set" ? "callback-not-withheld" : unset;
            JToken windowToken = Child(row, "window");
            if (windowToken == null || windowToken.Type != JTokenType.Array) return "missing:window";
            var samples = ((JArray)windowToken).Select(sample => Text(sample, "survivorPlayback")).ToList();
            int firstInactive = samples.IndexOf(PlaybackInactive);
            if (firstInactive < 0) return samples.Contains(PlaybackUnknown) ? "survivor-playback-unknown" : "survivor-playback-persisted";
            if (samples.Skip(firstInactive).Contains(PlaybackPlaying)) return "survivor-playback-contradictory";
            string terminal = Terminal(Text(windowEnd, "playback"), "window-end") ?? Terminal(Text(afterDisposal, "playback"), "after-disposal");
            if (terminal != null) return terminal;
            bool? tracked = Flag(windowEnd, "tracked");
            if (!tracked.HasValue) return "missing:survivorAtWindowEnd.tracked";
            if (!tracked.Value) return "survivor-tracking-not-source-local";
            bool? viewDestroyed = Flag(afterDisposal, "viewDestroyed"), componentDestroyed = Flag(afterDisposal, "playbackComponentDestroyed");
            if (!viewDestroyed.HasValue || !componentDestroyed.HasValue) return "missing:survivorAfterDisposal.destroyed";
            if (!viewDestroyed.Value || !componentDestroyed.Value) return "source-disposal-not-observed";
            JToken disposedCue = Child(afterDisposal, "cue");
            if (disposedCue == null || disposedCue.Type != JTokenType.Object) return "missing:survivorAfterDisposal.cue";
            if (Id(disposedCue) != survivorId) return "cue-identity-contradiction:survivor-after-disposal";
            unset = Unset(disposedCue, "endUtc", "survivorAfterDisposal.cue");
            if (unset != null) return unset == "set" ? "end-manufactured" : unset;
            int? unrelated = Number(afterDisposal, "unrelatedRecordsChanged");
            if (!unrelated.HasValue) return "missing:survivorAfterDisposal.unrelatedRecordsChanged";
            if (unrelated.Value != 0) return "unrelated-record-affected";
            int? hooks = Number(row, "interventionHooksAfter");
            JToken hookFailures = Child(row, "interventionDisposeFailures");
            if (!hooks.HasValue || hookFailures == null || hookFailures.Type != JTokenType.Array) return "intervention-not-restored";
            if (hooks.Value != 0 || ((JArray)hookFailures).Count != 0) return "intervention-not-restored";
            return null;
        }

        // A required terminal read: only a confirmed inactive ID passes; playing is persistence, anything else unknown.
        private static string Terminal(string state, string which)
        {
            if (state == PlaybackInactive) return null;
            return state == PlaybackPlaying ? "survivor-playback-persisted:" + which : "survivor-playback-unknown:" + which;
        }

        /// <summary>
        /// An explicit null in either representation: a JSON null as read back from a file, or the String-typed JValue
        /// (any JValue) whose value is null that the collector builds in memory. An absent token is not explicit null.
        /// </summary>
        internal static bool ExplicitNull(JToken token)
        {
            if (token == null) return false;
            if (token.Type == JTokenType.Null || token.Type == JTokenType.Undefined) return true;
            var value = token as JValue;
            return value != null && value.Value == null;
        }

        /// <summary>A property that must be recorded as unset: null when explicitly null, "set" when it has a value, "missing:&lt;path&gt;" when absent.</summary>
        private static string Unset(JToken parent, string name, string path)
        {
            var owner = parent as JObject;
            if (owner == null || owner.Property(name) == null) return "missing:" + path + "." + name;
            return ExplicitNull(owner[name]) ? null : "set";
        }

        private static JToken Child(JToken parent, string name) { var owner = parent as JObject; return owner == null ? null : owner[name]; }
        private static string Text(JToken parent, string name) { JToken token = Child(parent, name); return token == null || ExplicitNull(token) || token.Type != JTokenType.String ? null : (string)token; }
        private static bool? Flag(JToken parent, string name) { JToken token = Child(parent, name); return token == null || token.Type != JTokenType.Boolean ? (bool?)null : (bool)token; }
        private static int? Number(JToken parent, string name) { JToken token = Child(parent, name); return token == null || token.Type != JTokenType.Integer ? (int?)null : (int)token; }

        /// <summary>A cue record's playing ID: a positive integer, otherwise unknown.</summary>
        private static long? Id(JToken cue)
        {
            JToken token = Child(cue, "playingId");
            if (token == null || token.Type != JTokenType.Integer) return null;
            long id = (long)token;
            return id > 0 && id <= uint.MaxValue ? id : (long?)null;
        }

        /// <summary>A recorded time: an ISO-8601 string as the collector writes it, or a date token as a parser reads it; explicit null or unreadable is null.</summary>
        internal static DateTime? Utc(JToken token)
        {
            if (token == null || ExplicitNull(token)) return null;
            if (token.Type == JTokenType.Date) return ((DateTime)token).ToUniversalTime();
            if (token.Type != JTokenType.String) return null;
            DateTime parsed;
            return DateTime.TryParse((string)token, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out parsed)
                ? parsed.ToUniversalTime() : (DateTime?)null;
        }
    }

    /// <summary>
    /// Owner amendment 3 §3 (R4-F1, R6-F2): one cumulative cleanup result for an effects phase, or for a child run of its
    /// cells. A cell registers before its first acquisition (<see cref="Opened"/>) and publishes its cleanup from its own
    /// finally, so iterator disposal cannot skip it: named failures, which stay (a later repair never erases one), and
    /// residuals whose exact owned object is re-read until it settles. A cell that never reported, a residual still live
    /// and a residual that cannot be read all count as not restored. Recoveries (residuals observed settled after
    /// publication) are kept apart from the failures.
    /// </summary>
    internal sealed class Sprint7CellIsolation
    {
        private readonly List<string> failures = new List<string>();
        private readonly List<string> unreported = new List<string>();
        private readonly List<KeyValuePair<string, Func<bool?>>> pending = new List<KeyValuePair<string, Func<bool?>>>();
        private readonly List<string> unreadable = new List<string>();
        internal readonly List<string> Recoveries = new List<string>();
        internal readonly List<string> Admitted = new List<string>();
        internal string Refused;

        internal List<string> Failures { get { return failures.ToList(); } }
        internal List<string> Pending { get { return pending.Select(item => item.Key).ToList(); } }
        internal List<string> Unreported { get { return unreported.ToList(); } }

        internal bool Restored { get { return failures.Count == 0 && unreported.Count == 0 && pending.Count == 0; } }

        internal void Opened(string owner) { if (!unreported.Contains(owner)) unreported.Add(owner); }
        internal void Reported(string owner) { unreported.Remove(owner); }

        internal void Fail(string owner, string reason) { failures.Add(owner + ": " + reason); }

        internal void Fail(string owner, IEnumerable<string> reasons)
        {
            foreach (string reason in reasons ?? Enumerable.Empty<string>()) Fail(owner, reason);
        }

        /// <summary>A residual the owner still holds: settled() is true once settled, false while live, null when unreadable.</summary>
        internal void Residual(string owner, string what, Func<bool?> settled)
        {
            pending.Add(new KeyValuePair<string, Func<bool?>>(owner + ": " + what, settled));
        }

        /// <summary>Re-reads every residual once; a settled one leaves with a recovery observation. True when none remains.</summary>
        internal bool Settle()
        {
            unreadable.Clear();
            foreach (KeyValuePair<string, Func<bool?>> item in pending.ToList())
            {
                bool? state;
                try { state = item.Value == null ? null : item.Value(); }
                catch (Exception) { state = null; }
                if (state == true) { pending.Remove(item); Recoveries.Add(item.Key + " observed settled"); }
                else if (state == null) unreadable.Add(item.Key);
            }
            return pending.Count == 0;
        }

        internal JObject Describe()
        {
            return new JObject { ["restored"] = Restored, ["failures"] = new JArray(failures.ToArray()), ["unreported"] = new JArray(unreported.ToArray()),
                ["pending"] = new JArray(Pending.ToArray()), ["unreadable"] = new JArray(unreadable.ToArray()), ["recoveries"] = new JArray(Recoveries.ToArray()),
                ["admitted"] = new JArray(Admitted.ToArray()), ["refused"] = Refused };
        }
    }

    internal static partial class Sprint7ObserverPolicy
    {
        /// <summary>
        /// Owner amendment 3 §3: the effects phase's cell sequence. Before each cell the residuals are re-read for at most
        /// <paramref name="settleLimit"/>; the cell is admitted only when the isolation is restored, otherwise it is
        /// recorded as refused and nothing later runs. Each admitted cell runs through <paramref name="run"/>; disposing
        /// the sequence disposes the running cell, whose finally publishes its cleanup.
        /// </summary>
        internal static IEnumerable<int> Sequence(Sprint7CellIsolation isolation, IEnumerable<KeyValuePair<string, Func<IEnumerable<int>>>> cells,
            Func<string, Func<IEnumerable<int>>, IEnumerable<int>> run, TimeSpan settleLimit)
        {
            foreach (KeyValuePair<string, Func<IEnumerable<int>>> cell in cells)
            {
                foreach (int step in Settle(isolation, settleLimit)) yield return step;
                if (!isolation.Restored) { isolation.Refused = cell.Key; yield break; }
                isolation.Admitted.Add(cell.Key);
                foreach (int step in run(cell.Key, cell.Value)) yield return step;
            }
        }

        /// <summary>Re-reads the residuals every step until none remains or the limit passes.</summary>
        internal static IEnumerable<int> Settle(Sprint7CellIsolation isolation, TimeSpan limit)
        {
            DateTime end = DateTime.UtcNow + limit;
            while (!isolation.Settle() && DateTime.UtcNow < end) yield return 0;
        }

        internal static KeyValuePair<string, Func<IEnumerable<int>>> Cell(string name, Func<IEnumerable<int>> body)
        {
            return new KeyValuePair<string, Func<IEnumerable<int>>>(name, body);
        }
    }

    /// <summary>
    /// R3-F3: the conditions a cell's own executions introduced. Instances
    /// already present when the cell began are never acquired; an instance is
    /// acquired when it is seen with an owned origin (for Dust: a Dust breath
    /// execution by one of the cell's own Dust units), however its cast ended,
    /// and release removes exactly the acquired instances still present.
    /// </summary>
    internal sealed class AcquisitionLedger<T> where T : class
    {
        private readonly List<T> _preexisting;
        internal readonly List<T> Acquired = new List<T>();

        /// <summary>How many acquired instances the last release removed.</summary>
        internal int LastRemoved { get; private set; }

        internal AcquisitionLedger(IEnumerable<T> present)
        {
            _preexisting = new List<T>(present ?? Enumerable.Empty<T>());
        }

        private bool Contains(List<T> list, T value) { return list.Any(item => ReferenceEquals(item, value)); }

        /// <summary>Acquires every present instance with an owned origin that was not there at the start; returns how many were new.</summary>
        internal int Observe(IEnumerable<T> present, Func<T, bool> ownedOrigin)
        {
            int added = 0;
            foreach (T value in (present ?? Enumerable.Empty<T>()).ToList())
                if (value != null && !Contains(_preexisting, value) && !Contains(Acquired, value) && ownedOrigin(value)) { Acquired.Add(value); added++; }
            return added;
        }

        /// <summary>Observes once more, then removes each acquired instance still present; every removal is attempted.</summary>
        internal List<string> Release(IEnumerable<T> present, Func<T, bool> ownedOrigin, Action<T> remove)
        {
            List<T> now = (present ?? Enumerable.Empty<T>()).ToList();
            Observe(now, ownedOrigin);
            var failures = new List<string>();
            LastRemoved = 0;
            foreach (T value in now.Where(item => item != null && Contains(Acquired, item)))
            {
                try { remove(value); LastRemoved++; }
                catch (Exception error) { failures.Add(error.GetType().Name + ": " + error.Message); }
            }
            return failures;
        }

        /// <summary>Present instances with an owned origin that were not there at the start.</summary>
        internal int Remaining(IEnumerable<T> present, Func<T, bool> ownedOrigin)
        {
            return (present ?? Enumerable.Empty<T>()).Count(value => value != null && !Contains(_preexisting, value) && ownedOrigin(value));
        }
    }

}
