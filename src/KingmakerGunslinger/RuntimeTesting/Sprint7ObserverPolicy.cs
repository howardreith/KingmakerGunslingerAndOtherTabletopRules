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
            /// <summary>Candidate 1 (f3bfab1303): no per-source cue inventory, no settled other record, no explicit stop count.</summary>
            Round6First,
            /// <summary>Candidate 2 (740d94a995): adds the per-source cue inventory (cuesBeforeSelection).</summary>
            Round6Second,
            /// <summary>From candidate 3 (9c39b1ff7f) on: adds otherRetiredUtc and otherRetirementSettled with the explicit survivor stop count.</summary>
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
        /// The round-7 entry point, for the in-game verdict and for every reader of a round-7 row alike: the retirement time
        /// (otherRetiredUtc) and the other source's settled record (otherRetirementSettled.other) are read from the row itself.
        /// </summary>
        internal static string CallbackLossRound7(JObject row, string productStopRequested)
        {
            return Guarded(() => Settled(row, CueCollector.Round7, Child(row, "otherRetirementSettled"), "otherRetirementSettled",
                Child(row, "otherRetiredUtc"), "otherRetiredUtc", productStopRequested));
        }

        /// <summary>
        /// The round-6 entry point: those collectors emitted no settled other record or retirement time in the row, so the
        /// reader supplies the phase's end-of-phase cue record of the other ID and the phase timeline's retirement mark.
        /// </summary>
        internal static string CallbackLossRound6(JObject row, CueCollector collector, JToken phaseOtherCue, JToken retirementMarkUtc, string productStopRequested)
        {
            if (collector == CueCollector.Round7) return "wrong-entry:round7-row";
            return Guarded(() => Settled(row, collector, phaseOtherCue == null ? null : new JObject { ["other"] = phaseOtherCue.DeepClone() }, "phase.cuesAtEnd",
                retirementMarkUtc, "timeline.anky-loss-retire-other", productStopRequested));
        }

        // A malformed observation is never a verdict-free crash: it is an unreadable result.
        private static string Guarded(Func<string> evaluate)
        {
            try { return evaluate(); }
            catch (Exception error) { return "unreadable:" + error.GetType().Name; }
        }

        /// <summary>
        /// R4-F5 synthetic callback loss, the one settled evaluator (owner amendment 3 §4; recovery C1: R7-F1, R7-F6). It reads
        /// the callback-loss row exactly as its collector emitted it, in memory or read back from its file. A required
        /// property must be present: absent is "missing:&lt;path&gt;"; present but undefined, of the wrong type or out of range
        /// is "unreadable:&lt;path&gt;"; neither is ever evidence. A value required to be unset must be an explicit null (a
        /// JSON null, or the collector's String-typed JValue whose value is null). Every snapshot the collector emitted is
        /// reconciled: the selected survivor and other IDs must be valid and distinct, match the retirement-frame other
        /// record, the settled other record, both terminal survivor records and (where emitted) the per-source inventory;
        /// the survivor must show no stop request, stop reason, stop result or end in any pre-disposal snapshot, a zero stop
        /// count in the retirement frame and (round 7) through the window, no recorded end in any window sample, and
        /// readable withheld-callback timestamps matching the window's withheld count. Only the survivor's own disposal may
        /// then request its stop. Stop requests, completion callbacks and native playback queries stay separate inputs.
        /// </summary>
        private static string Settled(JObject row, CueCollector collector, JToken settledToken, string settledPath, JToken retiredToken, string retiredPath, string productStopRequested)
        {
            if (row == null) return "missing:row";
            JToken before = Child(row, "before"), windowEnd = Child(row, "survivorAtWindowEnd"), afterDisposal = Child(row, "survivorAfterDisposal");
            if (Text(before, "survivorPlayback") != PlaybackPlaying || Text(before, "otherPlayback") != PlaybackPlaying) return "not-both-playing";

            // Identities, bound to every snapshot of the same cue the collector emitted.
            JToken survivorCue = Child(before, "survivor"), otherCue = Child(before, "other");
            long? survivorId = Id(survivorCue), otherId = Id(otherCue);
            if (!survivorId.HasValue || !otherId.HasValue) return IdProblem(survivorCue, "before.survivor") ?? IdProblem(otherCue, "before.other") ?? "unreadable:before.ids";
            if (survivorId.Value == otherId.Value) return "cue-identity-contradiction:survivor-is-other";
            JToken retiredFrame = Child(row, "afterOtherRetired");
            if (!(retiredFrame is JObject)) return retiredFrame == null ? "missing:afterOtherRetired" : "unreadable:afterOtherRetired";
            if (Id(Child(retiredFrame, "other")) != otherId) return "cue-identity-contradiction:other-retirement-frame";
            if (collector != CueCollector.Round6First)
            {
                JToken sources = Child(row, "cuesBeforeSelection");
                if (!(sources is JObject)) return sources == null ? "missing:cuesBeforeSelection" : "unreadable:cuesBeforeSelection";
                List<long?> survivorSource = Ids(Child(sources, "survivorSource")), otherSource = Ids(Child(sources, "otherSource"));
                if (survivorSource == null || otherSource == null) return "unreadable:cuesBeforeSelection";
                if (!survivorSource.Contains(survivorId) || otherSource.Contains(survivorId)) return "cue-identity-contradiction:survivor-source";
                if (!otherSource.Contains(otherId) || survivorSource.Contains(otherId)) return "cue-identity-contradiction:other-source";
            }

            // The other source's retirement, read once settled.
            if (settledToken == null) return "missing:" + settledPath;
            if (!(settledToken is JObject)) return "unreadable:" + settledPath;
            JToken otherSettled = Child(settledToken, "other");
            if (otherSettled == null) return "missing:" + settledPath + ".other";
            if (!(otherSettled is JObject)) return "unreadable:" + settledPath + ".other";
            if (Id(otherSettled) != otherId) return "cue-identity-contradiction:other-settled";
            if (retiredToken == null) return "missing:" + retiredPath;
            DateTime? retiredUtc = Utc(retiredToken);
            if (!retiredUtc.HasValue) return "unreadable:" + retiredPath;
            int? otherStops = Count(otherSettled, "stopAttempts");
            if (!otherStops.HasValue) return Problem(otherSettled, "stopAttempts", settledPath + ".other");
            if (otherStops != 1 || !RetiredByOwnId(otherStops.Value, Text(otherSettled, "stopResult"), productStopRequested,
                    Utc(Child(otherSettled, "stopRequestedUtc")), Utc(Child(otherSettled, "endUtc")), retiredUtc.Value))
                return "other-not-stopped-by-own-id";

            // No stop for the survivor before its own disposal, in any snapshot.
            int? frameStops = Count(retiredFrame, "survivorStopAttempts");
            if (!frameStops.HasValue) return Problem(retiredFrame, "survivorStopAttempts", "afterOtherRetired");
            if (frameStops.Value != 0) return "survivor-stopped-by-other-retirement";
            if (collector == CueCollector.Round7)
            {
                int? windowStops = Count(settledToken, "survivorStopAttemptsThroughWindow");
                if (!windowStops.HasValue) return Problem(settledToken, "survivorStopAttemptsThroughWindow", settledPath);
                if (windowStops.Value != 0) return "survivor-stopped-by-other-retirement";
            }
            JToken kept = Child(windowEnd, "cue");
            if (!(kept is JObject)) return kept == null ? "missing:survivorAtWindowEnd.cue" : "unreadable:survivorAtWindowEnd.cue";
            if (Id(kept) != survivorId) return "cue-identity-contradiction:survivor-window-end";
            foreach (var snapshot in new[] { new { Cue = survivorCue, Path = "before.survivor" }, new { Cue = kept, Path = "survivorAtWindowEnd.cue" } })
            {
                string stop = Unset(snapshot.Cue, "stopReason", snapshot.Path, false) ?? Unset(snapshot.Cue, "stopResult", snapshot.Path, false) ??
                    Unset(snapshot.Cue, "stopRequestedUtc", snapshot.Path, true);
                if (stop != null) return stop == "set" ? "survivor-stopped-by-other-retirement" : stop;
                string end = Unset(snapshot.Cue, "endUtc", snapshot.Path, true);
                if (end != null) return end == "set" ? "callback-not-withheld" : end;
            }

            // The withheld callback: readable timestamps, matching the window's own count.
            JToken withheldToken = Child(windowEnd, "callbacksWithheld");
            if (withheldToken == null) return "missing:survivorAtWindowEnd.callbacksWithheld";
            if (!(withheldToken is JArray)) return "unreadable:survivorAtWindowEnd.callbacksWithheld";
            var withheld = (JArray)withheldToken;
            if (withheld.Count < 1) return "callback-not-withheld";
            if (withheld.Any(value => !Utc(value).HasValue)) return "unreadable:survivorAtWindowEnd.callbacksWithheld";

            // Every window sample: a readable playback state, no recorded end, a non-decreasing withheld count.
            JToken windowToken = Child(row, "window");
            if (windowToken == null) return "missing:window";
            if (!(windowToken is JArray) || ((JArray)windowToken).Count == 0) return "unreadable:window";
            var samples = new List<string>();
            int previousWithheld = 0, index = 0;
            foreach (JToken sample in (JArray)windowToken)
            {
                string path = "window[" + index++ + "]";
                if (!(sample is JObject)) return "unreadable:" + path;
                string playback = Text(sample, "survivorPlayback");
                if (playback == null) return Problem(sample, "survivorPlayback", path);
                if (playback != PlaybackPlaying && playback != PlaybackInactive && playback != PlaybackUnknown) return "unreadable:" + path + ".survivorPlayback";
                samples.Add(playback);
                string end = Unset(sample, "survivorEndUtc", path, true);
                if (end != null) return end == "set" ? "callback-not-withheld" : end;
                int? count = Count(sample, "callbacksWithheld");
                if (!count.HasValue) return Problem(sample, "callbacksWithheld", path);
                if (count.Value < previousWithheld) return "unreadable:" + path + ".callbacksWithheld";
                previousWithheld = count.Value;
            }
            if (previousWithheld != withheld.Count) return "callback-withheld-count-mismatch";
            int firstInactive = samples.IndexOf(PlaybackInactive);
            if (firstInactive < 0) return samples.Contains(PlaybackUnknown) ? "survivor-playback-unknown" : "survivor-playback-persisted";
            if (samples.Skip(firstInactive).Contains(PlaybackPlaying)) return "survivor-playback-contradictory";
            string terminal = Terminal(Text(windowEnd, "playback"), "window-end") ?? Terminal(Text(afterDisposal, "playback"), "after-disposal");
            if (terminal != null) return terminal;
            bool? tracked = Flag(windowEnd, "tracked");
            if (!tracked.HasValue) return Problem(windowEnd, "tracked", "survivorAtWindowEnd");
            if (!tracked.Value) return "survivor-tracking-not-source-local";

            // The survivor's own source disposal: observed, its stop is its own, no end manufactured.
            if (!(afterDisposal is JObject)) return afterDisposal == null ? "missing:survivorAfterDisposal" : "unreadable:survivorAfterDisposal";
            bool? viewDestroyed = Flag(afterDisposal, "viewDestroyed"), componentDestroyed = Flag(afterDisposal, "playbackComponentDestroyed");
            if (!viewDestroyed.HasValue) return Problem(afterDisposal, "viewDestroyed", "survivorAfterDisposal");
            if (!componentDestroyed.HasValue) return Problem(afterDisposal, "playbackComponentDestroyed", "survivorAfterDisposal");
            if (!viewDestroyed.Value || !componentDestroyed.Value) return "source-disposal-not-observed";
            JToken disposedCue = Child(afterDisposal, "cue");
            if (!(disposedCue is JObject)) return disposedCue == null ? "missing:survivorAfterDisposal.cue" : "unreadable:survivorAfterDisposal.cue";
            if (Id(disposedCue) != survivorId) return "cue-identity-contradiction:survivor-after-disposal";
            string manufactured = Unset(disposedCue, "endUtc", "survivorAfterDisposal.cue", true);
            if (manufactured != null) return manufactured == "set" ? "end-manufactured" : manufactured;
            int? unrelated = Count(afterDisposal, "unrelatedRecordsChanged");
            if (!unrelated.HasValue) return Problem(afterDisposal, "unrelatedRecordsChanged", "survivorAfterDisposal");
            if (unrelated.Value != 0) return "unrelated-record-affected";
            int? hooks = Count(row, "interventionHooksAfter");
            JToken hookFailures = Child(row, "interventionDisposeFailures");
            if (!hooks.HasValue || !(hookFailures is JArray)) return "intervention-not-restored";
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
        /// whose value is null that a null C# string becomes in the collector's memory. Absent, undefined and any other
        /// type are not explicit null.
        /// </summary>
        internal static bool ExplicitNull(JToken token)
        {
            if (token == null) return false;
            if (token.Type == JTokenType.Null) return true;
            var value = token as JValue;
            return value != null && value.Type == JTokenType.String && value.Value == null;
        }

        /// <summary>
        /// A field that must be recorded as unset: null when explicitly null; "set" when it carries a readable value (a
        /// timestamp for a time field, a string otherwise); "missing:&lt;path&gt;" when absent; "unreadable:&lt;path&gt;" when
        /// undefined or of a type or form that cannot be read (never mistaken for a stop or an end).
        /// </summary>
        private static string Unset(JToken parent, string name, string path, bool time)
        {
            var owner = parent as JObject;
            if (owner == null || owner.Property(name) == null) return "missing:" + path + "." + name;
            JToken token = owner[name];
            if (ExplicitNull(token)) return null;
            bool readable = time ? Utc(token).HasValue : token.Type == JTokenType.String && ((JValue)token).Value != null;
            return readable ? "set" : "unreadable:" + path + "." + name;
        }

        private static JToken Child(JToken parent, string name) { var owner = parent as JObject; return owner == null ? null : owner[name]; }

        private static string Problem(JToken parent, string name, string path)
        {
            var owner = parent as JObject;
            return owner == null || owner.Property(name) == null ? "missing:" + path + "." + name : "unreadable:" + path + "." + name;
        }

        private static string Text(JToken parent, string name)
        {
            JToken token = Child(parent, name);
            return token == null || token.Type != JTokenType.String || ExplicitNull(token) ? null : (string)((JValue)token).Value;
        }

        private static bool? Flag(JToken parent, string name) { JToken token = Child(parent, name); return token == null || token.Type != JTokenType.Boolean ? (bool?)null : (bool)((JValue)token).Value; }

        // An integer observation narrowed only when representable (a value beyond the target range is unreadable, never an exception).
        private static decimal? Integer(JToken token)
        {
            var value = token as JValue;
            if (value == null || value.Type != JTokenType.Integer || value.Value == null) return null;
            try { return Convert.ToDecimal(value.Value, System.Globalization.CultureInfo.InvariantCulture); }
            catch (Exception) { return null; }
        }

        private static int? Count(JToken parent, string name)
        {
            decimal? value = Integer(Child(parent, name));
            return value.HasValue && value.Value >= int.MinValue && value.Value <= int.MaxValue ? (int?)(int)value.Value : null;
        }

        /// <summary>A cue record's playing ID: a positive integer within the UInt32 range, otherwise unknown.</summary>
        private static long? Id(JToken cue)
        {
            decimal? value = Integer(Child(cue, "playingId"));
            return value.HasValue && value.Value > 0 && value.Value <= uint.MaxValue ? (long?)(long)value.Value : null;
        }

        private static string IdProblem(JToken cue, string path)
        {
            if (cue == null) return "missing:" + path;
            if (!(cue is JObject)) return "unreadable:" + path;
            return Id(cue).HasValue ? null : Problem(cue, "playingId", path);
        }

        private static List<long?> Ids(JToken cues)
        {
            var array = cues as JArray;
            if (array == null) return null;
            var ids = array.Select(Id).ToList();
            return ids.Any(id => !id.HasValue) ? null : ids;
        }

        /// <summary>A recorded time: an ISO-8601 string as the collector writes it, or a date token as a parser reads it; anything else is unreadable (null).</summary>
        internal static DateTime? Utc(JToken token)
        {
            if (token == null || ExplicitNull(token)) return null;
            if (token.Type == JTokenType.Date) return ((DateTime)token).ToUniversalTime();
            if (token.Type != JTokenType.String) return null;
            DateTime parsed;
            return DateTime.TryParse((string)((JValue)token).Value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out parsed)
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
