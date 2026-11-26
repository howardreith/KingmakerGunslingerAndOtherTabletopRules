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

        /// <summary>The round-7 entry point's language-bound identity, which new rows record (recovery C1, R2-F6).</summary>
        internal const string Round7Evaluator = nameof(Sprint7ObserverPolicy) + "." + nameof(CallbackLossRound7);

        /// <summary>The round-6 reader's language-bound identity, for current reinterpretations of retained round-6 rows.</summary>
        internal const string Round6Evaluator = nameof(Sprint7ObserverPolicy) + "." + nameof(CallbackLossRound6);

        /// <summary>The header of a new callback-loss row: what was synthetic, the declared windows and the evaluator the row's verdict comes from.</summary>
        internal static JObject NewCallbackLossRow(double windowSeconds, double disposalSeconds, double overlapSeconds)
        {
            return new JObject { ["synthetic"] = "the product's end-of-event callback for one exact survivor record is withheld from the product by the observer; native playback is untouched and read separately",
                ["declaredWindowSeconds"] = windowSeconds, ["declaredDisposalSeconds"] = disposalSeconds, ["declaredOverlapSeconds"] = overlapSeconds,
                ["evaluator"] = Round7Evaluator };
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

        // ---- The observation contract (recovery C1, amendment 1) -------------------------------------------------------
        //
        // One posted cue is identified by its immutable posting: a positive playing ID, a non-empty event, its posting
        // frame and its posting time (CueRow: playingId, event, frame, utc). Its lifecycle fields can only advance: endUtc
        // (the product received the completion callback) and one stop attempt (SummonCastCueLifecycle.Retire: stopAttempts
        // becomes 1 and stopReason is set first; on success stopRequestedUtc and stopResult "stop-requested" follow; on
        // failure stopResult "failed:..." with no request time; at most one attempt per playing ID). Once observed, an end
        // or stop never disappears or changes and stopAttempts never decreases. A cue record and the stop count the collector
        // reads beside it are read in consecutive statements with no yield between them, on the main thread where Retire
        // runs, so Retire cannot interleave and they agree exactly; samples and other reads that do yield between them are
        // not treated as one atomic snapshot. Native playback (playing / inactive / unknown) is a separate
        // query, true only at its own read. A playing ID of exactly 0 is the producer's record of a failed post: it owns
        // nothing.
        //
        // What the collector (S7CallbackLoss) observes, in recording order:
        //   S0 cuesBeforeSelection  every cue of each source, after both selected cues were confirmed playing and the
        //                           withholding target was armed on the survivor (Round6Second and Round7 only);
        //   S1 before               the selected survivor and other cue records and their playback;
        //   S2 afterOtherRetired    retirement time recorded first (otherRetiredUtc, Round7), the other source retired,
        //                           then the other cue and both stop counts, in the retirement frame;
        //   W  window[i]            per sample: the survivor's playback queried, then utc stamped, then survivorEndUtc,
        //                           tracked and the withheld count read; samples run while now < retirement + window;
        //   S3 otherRetirementSettled and survivorAtWindowEnd   after the loop exits: for Round7, whose otherRetiredUtc is
        //                           the loop's own start, not earlier than that + declaredWindowSeconds; for Round6, which
        //                           recorded only a later phase mark, not earlier than the later posting + window and the
        //                           last sample (the exact time is never recorded); the terminal withheld timestamps are
        //                           copied among them, before the survivor's disposal;
        //   S4 survivorAfterDisposal after the survivor's own source was then retired and observed destroyed.
        // Round6First (f3bfab1303) armed the target before both cues were confirmed playing: only the survivor's posting
        // bounds its withheld callback from below.
        //
        // A required observation that is absent is "missing:<path>"; present but undefined, of the wrong type, negative,
        // out of range, an empty event or a time without a recorded date and zone is "unreadable:<path>". Neither is
        // evidence of anything. A value that must be unset must be an explicit null (JSON null, or the collector's
        // String-typed JValue whose value is null).
        // ------------------------------------------------------------------------------------------------------------------

        private static string Settled(JObject row, CueCollector collector, JToken settledToken, string settledPath, JToken retiredToken, string retiredPath, string productStopRequested)
        {
            if (row == null) return "missing:row";
            JToken before = Child(row, "before"), windowEnd = Child(row, "survivorAtWindowEnd"), afterDisposal = Child(row, "survivorAfterDisposal");
            if (Text(before, "survivorPlayback") != PlaybackPlaying || Text(before, "otherPlayback") != PlaybackPlaying) return "not-both-playing";

            // S1: the selected postings.
            JToken survivorCue = Child(before, "survivor"), otherCue = Child(before, "other");
            string problem = Posting(survivorCue, "before.survivor") ?? Posting(otherCue, "before.other");
            if (problem != null) return problem;
            long survivorId = Id(survivorCue).Value, otherId = Id(otherCue).Value;
            if (survivorId == otherId) return "cue-identity-contradiction:survivor-is-other";

            // S0 (where emitted): each selected posting is exactly one record of its own source, unended and unstopped there.
            JToken survivorRecord = null, otherRecord = null;
            if (collector != CueCollector.Round6First)
            {
                JToken sources = Child(row, "cuesBeforeSelection");
                if (!(sources is JObject)) return sources == null ? "missing:cuesBeforeSelection" : "unreadable:cuesBeforeSelection";
                problem = SourceRecord(Child(sources, "survivorSource"), "cuesBeforeSelection.survivorSource", survivorId, out survivorRecord) ??
                    SourceRecord(Child(sources, "otherSource"), "cuesBeforeSelection.otherSource", otherId, out otherRecord);
                if (problem != null) return problem;
                JToken crossed;
                problem = SourceRecord(Child(sources, "otherSource"), "cuesBeforeSelection.otherSource", survivorId, out crossed);
                if (problem != null) return problem;
                if (crossed != null) return "cue-identity-contradiction:survivor-source";
                problem = SourceRecord(Child(sources, "survivorSource"), "cuesBeforeSelection.survivorSource", otherId, out crossed);
                if (problem != null) return problem;
                if (crossed != null) return "cue-identity-contradiction:other-source";
                if (survivorRecord == null) return "cue-identity-contradiction:survivor-source";
                if (otherRecord == null) return "cue-identity-contradiction:other-source";
                problem = SamePosting(survivorRecord, survivorCue, "survivor-source") ?? SamePosting(otherRecord, otherCue, "other-source");
                if (problem != null) return problem;
            }

            // S2: the retirement frame.
            JToken retiredFrame = Child(row, "afterOtherRetired");
            if (!(retiredFrame is JObject)) return retiredFrame == null ? "missing:afterOtherRetired" : "unreadable:afterOtherRetired";
            JToken otherAtRetirement = Child(retiredFrame, "other");
            problem = Posting(otherAtRetirement, "afterOtherRetired.other");
            if (problem != null) return problem;
            problem = SamePosting(otherCue, otherAtRetirement, "other-retirement-frame");
            if (problem != null) return problem;

            // S3: the other source's settled record and the retirement time.
            if (settledToken == null) return "missing:" + settledPath;
            if (!(settledToken is JObject)) return "unreadable:" + settledPath;
            JToken otherSettled = Child(settledToken, "other");
            problem = Posting(otherSettled, settledPath + ".other");
            if (problem != null) return problem;
            problem = SamePosting(otherCue, otherSettled, "other-settled");
            if (problem != null) return problem;
            if (retiredToken == null) return "missing:" + retiredPath;
            DateTime? retiredUtc = Utc(retiredToken);
            if (!retiredUtc.HasValue) return "unreadable:" + retiredPath;
            DateTime survivorPosted = Utc(Child(survivorCue, "utc")).Value, otherPosted = Utc(Child(otherCue, "utc")).Value;
            if (retiredUtc.Value < survivorPosted || retiredUtc.Value < otherPosted) return "retirement-before-posting";
            double? declaredWindow = Seconds(Child(row, "declaredWindowSeconds"));
            if (!declaredWindow.HasValue) return Problem(row, "declaredWindowSeconds", "row");
            // The zero-stop terminal reads came after the loop. Round7 recorded the loop's own start (otherRetiredUtc), so
            // they came at or after that start + window, the loop deadline. Round6 recorded only the later phase mark, so
            // its deadline is bounded below only by what the row establishes: the retirement followed both postings.
            DateTime? loopDeadline = collector == CueCollector.Round7 ? retiredUtc.Value.AddSeconds(declaredWindow.Value) : (DateTime?)null;
            DateTime terminalNotBefore = loopDeadline ?? (survivorPosted >= otherPosted ? survivorPosted : otherPosted).AddSeconds(declaredWindow.Value);

            // The other cue's lifecycle, S0 -> S1 -> S2 -> S3: nothing observed may disappear or change; attempts never fall.
            problem = (otherRecord == null ? null : Advances(otherRecord, otherCue, "other")) ?? Advances(otherCue, otherAtRetirement, "other") ?? Advances(otherAtRetirement, otherSettled, "other");
            if (problem != null) return problem;
            int? otherFrameStops = Count(retiredFrame, "otherStopAttempts");
            if (!otherFrameStops.HasValue) return Problem(retiredFrame, "otherStopAttempts", "afterOtherRetired");
            int? otherStops = Count(otherSettled, "stopAttempts");
            if (!otherStops.HasValue) return Problem(otherSettled, "stopAttempts", settledPath + ".other");
            if (otherStops.Value < otherFrameStops.Value) return "lifecycle-contradiction:other.stopAttempts-decreased";
            if (otherFrameStops.Value > 1 || otherStops.Value > 1) return "stop-attempts-impossible:other";
            // R4-F1: Retire skips a posting whose end is already recorded, so an end seen with zero attempts keeps it at zero.
            if (otherFrameStops.Value == 0 && Unset(otherAtRetirement, "endUtc", "afterOtherRetired.other", true) == "set" && otherStops.Value > 0)
                return "lifecycle-contradiction:other.stop-after-completion";
            // R3-F2: each stop observation agrees with the attempt count read with it.
            problem = StopAgrees(otherRecord, null, "cuesBeforeSelection.otherSource[selected]") ?? StopAgrees(otherCue, null, "before.other") ??
                StopAgrees(otherAtRetirement, otherFrameStops, "afterOtherRetired.other") ?? StopAgrees(otherSettled, otherStops, settledPath + ".other");
            if (problem != null) return problem;
            // An end or stop already recorded before the retirement cannot be the retirement's own.
            foreach (var earlier in new[] { new { Cue = otherCue, Path = "before.other" } })
            {
                string end = Unset(earlier.Cue, "endUtc", earlier.Path, true);
                if (end != null) return end == "set" ? "other-ended-before-retirement" : end;
                string stop = Unset(earlier.Cue, "stopRequestedUtc", earlier.Path, true);
                if (stop != null) return stop == "set" ? "other-stopped-before-retirement" : stop;
            }
            // R4-F6: the settled completion is an explicit null (no completion recorded), a readable time, or unavailable.
            string settledEnd = Unset(otherSettled, "endUtc", settledPath + ".other", true);
            if (settledEnd != null && settledEnd != "set") return settledEnd;
            // R4-F7: a successful request is credited to the retirement only with the production retirement cause.
            if (Text(otherSettled, "stopResult") == SummonCastCueStopRequested && !RetirementReason(Text(otherSettled, "stopReason")))
                return "other-stop-cause-not-retirement";
            if (otherStops.Value != 1 || !RetiredByOwnId(otherStops.Value, Text(otherSettled, "stopResult"), productStopRequested,
                    Utc(Child(otherSettled, "stopRequestedUtc")), Utc(Child(otherSettled, "endUtc")), retiredUtc.Value))
                return "other-not-stopped-by-own-id";

            // The survivor through the window: no stop in any pre-disposal snapshot, counts cumulative and readable.
            int? frameStops = Count(retiredFrame, "survivorStopAttempts");
            if (!frameStops.HasValue) return Problem(retiredFrame, "survivorStopAttempts", "afterOtherRetired");
            if (frameStops.Value > 1) return "stop-attempts-impossible:afterOtherRetired.survivorStopAttempts";
            if (frameStops.Value > 0) return "survivor-stopped-by-other-retirement";
            int windowStops = frameStops.Value;
            if (collector == CueCollector.Round7)
            {
                int? through = Count(settledToken, "survivorStopAttemptsThroughWindow");
                if (!through.HasValue) return Problem(settledToken, "survivorStopAttemptsThroughWindow", settledPath);
                if (through.Value > 1) return "stop-attempts-impossible:" + settledPath + ".survivorStopAttemptsThroughWindow";
                if (through.Value > 0) return "survivor-stopped-by-other-retirement";
                windowStops = through.Value;
            }
            JToken kept = Child(windowEnd, "cue");
            problem = Posting(kept, "survivorAtWindowEnd.cue");
            if (problem != null) return problem;
            problem = SamePosting(survivorCue, kept, "survivor-window-end");
            if (problem != null) return problem;
            foreach (var snapshot in new[] { new { Cue = survivorRecord, Path = "cuesBeforeSelection.survivorSource[selected]" }, new { Cue = survivorCue, Path = "before.survivor" },
                new { Cue = kept, Path = "survivorAtWindowEnd.cue" } })
            {
                if (snapshot.Cue == null) continue;
                string stop = Unset(snapshot.Cue, "stopReason", snapshot.Path, false) ?? Unset(snapshot.Cue, "stopResult", snapshot.Path, false) ??
                    Unset(snapshot.Cue, "stopRequestedUtc", snapshot.Path, true);
                if (stop != null) return stop == "set" ? "survivor-stopped-by-other-retirement" : stop;
                string end = Unset(snapshot.Cue, "endUtc", snapshot.Path, true);
                if (end != null) return end == "set" ? "callback-not-withheld" : end;
            }
            // The arming lower bound for the withheld callback: Round6First armed on the survivor alone; later
            // collectors armed only once both selected cues were confirmed playing, so after both were posted.
            DateTime armedNotBefore = collector == CueCollector.Round6First || survivorPosted >= otherPosted ? survivorPosted : otherPosted;

            // The withheld callback, at the times the collector observed it. Each sample reads its time before its count,
            // so a callback timed before a sample's time must be in that sample's count, and a sample's count cannot
            // include a callback first timed after the next sample's time; the terminal list is read after the last sample.
            JToken withheldToken = Child(windowEnd, "callbacksWithheld");
            if (withheldToken == null) return "missing:survivorAtWindowEnd.callbacksWithheld";
            if (!(withheldToken is JArray)) return "unreadable:survivorAtWindowEnd.callbacksWithheld";
            var withheld = new List<DateTime>();
            foreach (JToken value in (JArray)withheldToken)
            {
                DateTime? at = Utc(value);
                if (!at.HasValue) return "unreadable:survivorAtWindowEnd.callbacksWithheld";
                if (at.Value < survivorPosted) return "callback-predates-cue";
                if (at.Value < armedNotBefore) return "callback-predates-arming";
                if (withheld.Count > 0 && at.Value < withheld[withheld.Count - 1]) return "unreadable:survivorAtWindowEnd.callbacksWithheld";
                withheld.Add(at.Value);
            }
            if (withheld.Count < 1) return "callback-not-withheld";
            JToken windowToken = Child(row, "window");
            if (windowToken == null) return "missing:window";
            if (!(windowToken is JArray) || ((JArray)windowToken).Count == 0) return "unreadable:window";
            var window = ((JArray)windowToken).ToList();
            var samples = new List<string>();
            var times = new List<DateTime>();
            var counts = new List<int>();
            for (int index = 0; index < window.Count; index++)
            {
                JToken sample = window[index];
                string path = "window[" + index + "]";
                if (!(sample is JObject)) return "unreadable:" + path;
                DateTime? at = Utc(Child(sample, "utc"));
                if (!at.HasValue) return Problem(sample, "utc", path);
                if (times.Count > 0 && at.Value < times[times.Count - 1]) return "unreadable:" + path + ".utc";
                if (at.Value < retiredUtc.Value) return "window-before-retirement";
                string playback = Text(sample, "survivorPlayback");
                if (playback == null) return Problem(sample, "survivorPlayback", path);
                if (playback != PlaybackPlaying && playback != PlaybackInactive && playback != PlaybackUnknown) return "unreadable:" + path + ".survivorPlayback";
                string end = Unset(sample, "survivorEndUtc", path, true);
                if (end != null) return end == "set" ? "callback-not-withheld" : end;
                int? count = Count(sample, "callbacksWithheld");
                if (!count.HasValue) return Problem(sample, "callbacksWithheld", path);
                if (counts.Count > 0 && count.Value < counts[counts.Count - 1]) return "callback-count-decreased";
                samples.Add(playback); times.Add(at.Value); counts.Add(count.Value);
            }
            if (counts[counts.Count - 1] > withheld.Count) return "callback-count-exceeds-terminal";
            // R4-F2 (Round7): a sample stamped at or past the loop deadline ends the loop; no later sample can follow it.
            if (loopDeadline.HasValue)
                for (int index = 0; index + 1 < times.Count; index++)
                    if (times[index] >= loopDeadline.Value) return "window-sample-after-deadline:window[" + (index + 1) + "]";
            // R4-F1: the retirement frame was read before the first sample; a time it recorded cannot be later than that sample.
            foreach (string field in new[] { "stopRequestedUtc", "endUtc" })
            {
                DateTime? recorded = Unset(otherAtRetirement, field, "afterOtherRetired.other", true) == "set" ? Utc(Child(otherAtRetirement, field)) : null;
                if (recorded.HasValue && recorded.Value > times[0]) return "retirement-frame-after-first-sample:" + field;
            }
            for (int index = 0; index < times.Count; index++)
            {
                int before_ = withheld.Count(at => at < times[index]);
                int notAfterNext = index + 1 < times.Count ? withheld.Count(at => at <= times[index + 1]) : withheld.Count;
                if (counts[index] < before_ || counts[index] > notAfterNext) return "callback-count-contradiction:window[" + index + "]";
            }
            int firstInactive = samples.IndexOf(PlaybackInactive);
            if (firstInactive < 0) return samples.Contains(PlaybackUnknown) ? "survivor-playback-unknown" : "survivor-playback-persisted";
            if (samples.Skip(firstInactive).Contains(PlaybackPlaying)) return "survivor-playback-contradictory";
            string terminal = Terminal(Text(windowEnd, "playback"), "window-end") ?? Terminal(Text(afterDisposal, "playback"), "after-disposal");
            if (terminal != null) return terminal;
            bool? tracked = Flag(windowEnd, "tracked");
            if (!tracked.HasValue) return Problem(windowEnd, "tracked", "survivorAtWindowEnd");
            if (!tracked.Value) return "survivor-tracking-not-source-local";

            // S4: the survivor's own source disposal. Its stop is its own only when later than the zero-stop interval, which
            // reaches at least the last window sample (the terminal reads that still found no stop came after it).
            if (!(afterDisposal is JObject)) return afterDisposal == null ? "missing:survivorAfterDisposal" : "unreadable:survivorAfterDisposal";
            bool? viewDestroyed = Flag(afterDisposal, "viewDestroyed"), componentDestroyed = Flag(afterDisposal, "playbackComponentDestroyed");
            if (!viewDestroyed.HasValue) return Problem(afterDisposal, "viewDestroyed", "survivorAfterDisposal");
            if (!componentDestroyed.HasValue) return Problem(afterDisposal, "playbackComponentDestroyed", "survivorAfterDisposal");
            if (!viewDestroyed.Value || !componentDestroyed.Value) return "source-disposal-not-observed";
            JToken disposedCue = Child(afterDisposal, "cue");
            problem = Posting(disposedCue, "survivorAfterDisposal.cue");
            if (problem != null) return problem;
            problem = SamePosting(survivorCue, disposedCue, "survivor-after-disposal") ?? Advances(kept, disposedCue, "survivor");
            if (problem != null) return problem;
            string manufactured = Unset(disposedCue, "endUtc", "survivorAfterDisposal.cue", true);
            if (manufactured != null) return manufactured == "set" ? "end-manufactured" : manufactured;
            int? disposedStops = Count(afterDisposal, "stopAttempts");
            if (!disposedStops.HasValue) return Problem(afterDisposal, "stopAttempts", "survivorAfterDisposal");
            if (disposedStops.Value < windowStops) return "lifecycle-contradiction:survivor.stopAttempts-decreased";
            problem = StopAgrees(disposedCue, disposedStops, "survivorAfterDisposal.cue");
            if (problem != null) return problem;
            string ownStop = Unset(disposedCue, "stopRequestedUtc", "survivorAfterDisposal.cue", true);
            if (ownStop != null && ownStop != "set") return ownStop;
            if (ownStop == "set")
            {
                DateTime stoppedAt = Utc(Child(disposedCue, "stopRequestedUtc")).Value;
                // R2-F3 / R4-F3: the through-window reads that still found no stop came after the last sample and at or after
                // the established deadline bound; the disposal stop came after them. A shared clock tick is not an inversion (R4-F4).
                if (stoppedAt < terminalNotBefore || stoppedAt < times[times.Count - 1]) return "survivor-stop-inside-zero-stop-interval";
                // R3-F1: the terminal withheld list was copied before the survivor's disposal began; only a strictly later callback inverts that.
                if (withheld.Any(at => at > stoppedAt)) return "callback-after-terminal-capture";
                if (!RetirementReason(Text(disposedCue, "stopReason"))) return "survivor-stop-cause-not-retirement";
            }
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

        /// <summary>A readable posting: an object with a valid playing ID, an event, a non-negative posting frame and a posting time.</summary>
        private static string Posting(JToken cue, string path)
        {
            if (cue == null) return "missing:" + path;
            if (!(cue is JObject)) return "unreadable:" + path;
            if (!Id(cue).HasValue) return Problem(cue, "playingId", path);
            if (string.IsNullOrEmpty(Text(cue, "event"))) return Problem(cue, "event", path);
            if (!Count(cue, "frame").HasValue) return Problem(cue, "frame", path);
            if (!Utc(Child(cue, "utc")).HasValue) return Problem(cue, "utc", path);
            return null;
        }

        /// <summary>Two readable snapshots of the same posting: same playing ID, event, posting frame and posting time.</summary>
        private static string SamePosting(JToken a, JToken b, string which)
        {
            return Id(a) == Id(b) && Text(a, "event") == Text(b, "event") && Count(a, "frame") == Count(b, "frame") && Utc(Child(a, "utc")) == Utc(Child(b, "utc"))
                ? null : "cue-identity-contradiction:" + which;
        }

        /// <summary>
        /// The one record of a source inventory with the given playing ID. Records whose playing ID is exactly 0 are the
        /// producer's failed posts and own nothing; any other unreadable record makes the inventory unreadable; two records
        /// with the same ID are ambiguous. A found record must be a readable posting.
        /// </summary>
        private static string SourceRecord(JToken inventory, string path, long id, out JToken found)
        {
            found = null;
            var array = inventory as JArray;
            if (array == null) return inventory == null ? "missing:" + path : "unreadable:" + path;
            int index = 0;
            foreach (JToken record in array)
            {
                string at = path + "[" + index++ + "]";
                decimal? raw = Integer(Child(record, "playingId"));
                if (raw.HasValue && raw.Value == 0m) continue;
                // Every positive record is inventory evidence: it must be a readable posting, selected or not.
                string posting = Posting(record, at);
                if (posting != null) return posting;
                if (Id(record).Value != id) continue;
                if (found != null) return "cue-identity-ambiguous:" + path;
                found = record;
            }
            return null;
        }

        /// <summary>
        /// The lifecycle of one posting from an earlier to a later snapshot: an end or a stop observed earlier is still the
        /// same later (it cannot disappear or change); fields still unset earlier may be set later.
        /// </summary>
        private static string Advances(JToken earlier, JToken later, string which)
        {
            foreach (var field in new[] { new { Name = "endUtc", Time = true }, new { Name = "stopRequestedUtc", Time = true }, new { Name = "stopReason", Time = false }, new { Name = "stopResult", Time = false } })
            {
                string first = Unset(earlier, field.Name, which, field.Time);
                if (first == null) continue;
                if (first != "set") return first;
                string second = Unset(later, field.Name, which, field.Time);
                bool same = second == "set" && (field.Time ? Utc(Child(earlier, field.Name)) == Utc(Child(later, field.Name)) : Text(earlier, field.Name) == Text(later, field.Name));
                if (!same) return "lifecycle-contradiction:" + which + "." + field.Name;
            }
            return null;
        }

        /// <summary>
        /// R3-F2: one snapshot's stop fields against the attempt count read with it (null when the collector read no count
        /// with this snapshot). Without an attempt every stop field is unset; with the one attempt the reason is set and
        /// either the request succeeded (time and "stop-requested") or failed ("failed:..." without a time). The fields
        /// also agree among themselves.
        /// </summary>
        private static string StopAgrees(JToken cue, int? attempts, string path)
        {
            if (cue == null) return null;
            string reason = Unset(cue, "stopReason", path, false), time = Unset(cue, "stopRequestedUtc", path, true), result = Unset(cue, "stopResult", path, false);
            foreach (string state in new[] { reason, time, result }) if (state != null && state != "set") return state;
            string text = Text(cue, "stopResult");
            bool any = reason != null || time != null || result != null;
            bool succeeded = reason != null && result != null && text == SummonCastCueStopRequested && time != null;
            bool failed = reason != null && result != null && text.StartsWith("failed:", StringComparison.Ordinal) && time == null;
            if (any && !succeeded && !failed) return "stop-fields-inconsistent:" + path;
            if (attempts.HasValue)
            {
                if (attempts.Value > 1) return "stop-attempts-impossible:" + path;
                if (attempts.Value == 0 && any) return "stop-without-attempt:" + path;
                if (attempts.Value == 1 && !any) return "attempt-without-stop:" + path;
            }
            return null;
        }

        /// <summary>The production retirement causes (SummonCastCuePlayback: "entity-retired" in LateUpdate, "view-destroyed" in OnDestroy).</summary>
        private static bool RetirementReason(string reason) { return reason == "entity-retired" || reason == "view-destroyed"; }

        // The producer's successful stop result (SummonCastCueLifecycle.StopRequested), kept here so the evaluator stays pure.
        private const string SummonCastCueStopRequested = "stop-requested";

        /// <summary>A declared duration: a finite positive number of seconds, otherwise unreadable (null).</summary>
        private static double? Seconds(JToken token)
        {
            var value = token as JValue;
            if (value == null || (value.Type != JTokenType.Float && value.Type != JTokenType.Integer) || value.Value == null) return null;
            double seconds;
            try { seconds = Convert.ToDouble(value.Value, System.Globalization.CultureInfo.InvariantCulture); }
            catch (Exception) { return null; }
            return seconds > 0 && !double.IsInfinity(seconds) && !double.IsNaN(seconds) ? (double?)seconds : null;
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

        /// <summary>A count or frame: a non-negative integer within Int32; negative, malformed or oversized is unreadable (null), never an observation.</summary>
        private static int? Count(JToken parent, string name)
        {
            decimal? value = Integer(Child(parent, name));
            return value.HasValue && value.Value >= 0 && value.Value <= int.MaxValue ? (int?)(int)value.Value : null;
        }

        /// <summary>A cue record's playing ID: a positive integer within the UInt32 range, otherwise unknown.</summary>
        private static long? Id(JToken cue)
        {
            decimal? value = Integer(Child(cue, "playingId"));
            return value.HasValue && value.Value > 0 && value.Value <= uint.MaxValue ? (long?)(long)value.Value : null;
        }

        /// <summary>
        /// A recorded instant: the collector's round-trip string (ToString("o"): full date, time and zone) or the date token
        /// a parser makes of it. A value without a recorded date and zone is unreadable (null): the replay machine's date or
        /// offset is never supplied (R3-F3).
        /// </summary>
        internal static DateTime? Utc(JToken token)
        {
            if (token == null || ExplicitNull(token)) return null;
            if (token.Type == JTokenType.Date)
            {
                object raw = ((JValue)token).Value;
                if (raw is DateTimeOffset) return ((DateTimeOffset)raw).UtcDateTime;
                if (!(raw is DateTime) || ((DateTime)raw).Kind == DateTimeKind.Unspecified) return null;
                return ((DateTime)raw).ToUniversalTime();
            }
            if (token.Type != JTokenType.String) return null;
            DateTime parsed;
            if (!DateTime.TryParseExact((string)((JValue)token).Value, "o", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out parsed) ||
                parsed.Kind == DateTimeKind.Unspecified) return null;
            return parsed.ToUniversalTime();
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

    /// <summary>
    /// Recovery C3 (R7-F4): requests an observer makes directly (for example retiring a cue source) and their results, kept
    /// apart. A request is recorded before it is made, so it is never issued twice; a request that throws is a named failure
    /// and is rethrown to its caller (the original error). Publication sends every failure to the enclosing isolation as a
    /// sticky failure and every requested item not read settled as a residual the admission guard re-reads.
    /// </summary>
    internal sealed class Sprint7OwnedRequests<T> where T : class
    {
        private readonly List<KeyValuePair<T, string>> requested = new List<KeyValuePair<T, string>>();
        internal readonly List<string> Failures = new List<string>();

        internal bool Requested(T item) { return item != null && requested.Any(entry => ReferenceEquals(entry.Key, item)); }

        internal int Count { get { return requested.Count; } }

        /// <summary>Makes the request once; returns false when it was already made. A throwing request is recorded and rethrown.</summary>
        internal bool Request(T item, string label, Action<T> request)
        {
            if (item == null || Requested(item)) return false;
            requested.Add(new KeyValuePair<T, string>(item, label));
            try { request(item); }
            catch (Exception error)
            {
                Failures.Add(label + ": " + error.GetType().Name + ": " + error.Message);
                throw;
            }
            return true;
        }

        /// <summary>As <see cref="Request"/>, for an unwinding release: the failure is recorded, not rethrown.</summary>
        internal bool RequestRecorded(T item, string label, Action<T> request)
        {
            try { return Request(item, label, request); }
            catch (Exception) { return true; }
        }

        /// <summary>Failures become sticky isolation failures; each request whose result is not read settled stays a residual.</summary>
        internal void Publish(Sprint7CellIsolation isolation, string owner, Func<T, bool?> settled)
        {
            isolation.Fail(owner, Failures);
            foreach (KeyValuePair<T, string> entry in requested)
            {
                T item = entry.Key;
                bool? state;
                try { state = settled(item); } catch (Exception) { state = null; }
                if (state != true) isolation.Residual(owner, entry.Value + " settled", () => settled(item));
            }
        }
    }

    /// <summary>
    /// Recovery C3 (R7-F2): the commands a cell issued and the exact execution processes they started. A process is retained
    /// when first seen and before any interruption can clear the command's field; each command is released on its own, so an
    /// interruption that throws never skips another command; a retained process not read ended (running, or unreadable) is a
    /// live execution the cell publishes as a residual.
    /// </summary>
    internal sealed class Sprint7OwnedCommands<TCommand, TProcess> where TCommand : class where TProcess : class
    {
        private readonly List<TCommand> commands = new List<TCommand>();
        private readonly List<TProcess> processes = new List<TProcess>();
        private readonly Func<TCommand, TProcess> processOf;
        private readonly Func<TProcess, bool?> ended;

        internal Sprint7OwnedCommands(Func<TCommand, TProcess> processOf, Func<TProcess, bool?> ended)
        {
            this.processOf = processOf;
            this.ended = ended;
        }

        internal List<TCommand> Commands { get { return commands.ToList(); } }
        internal List<TProcess> Processes { get { return processes.ToList(); } }

        internal void Own(TCommand command) { if (command != null && !commands.Any(value => ReferenceEquals(value, command))) commands.Add(command); }

        /// <summary>Retains the current process of an owned command (of every owned command when null).</summary>
        internal void Capture(TCommand command = null)
        {
            foreach (TCommand owned in command == null ? commands.ToList() : new List<TCommand> { command })
            {
                TProcess process;
                try { process = processOf(owned); } catch (Exception) { process = null; }
                if (process != null && !processes.Any(value => ReferenceEquals(value, process))) processes.Add(process);
            }
        }

        internal bool Retains(TProcess process) { return process != null && processes.Any(value => ReferenceEquals(value, process)); }

        /// <summary>
        /// Releases each given owned command (every owned command when null) on its own: its process is captured first, then
        /// the interruption runs when <paramref name="unfinished"/> says the command still runs. Returns the named failures.
        /// </summary>
        internal List<string> ReleaseEach(Func<TCommand, bool> unfinished, Action<TCommand> interrupt, Func<TCommand, string> label, IEnumerable<TCommand> only = null)
        {
            var failures = new List<string>();
            foreach (TCommand command in (only ?? commands).ToList())
            {
                Capture(command);
                try { if (unfinished(command)) interrupt(command); }
                catch (Exception error) { failures.Add(label(command) + ": " + error.GetType().Name + ": " + error.Message); }
            }
            return failures;
        }

        /// <summary>Retained processes not read ended: running, or unreadable.</summary>
        internal List<TProcess> Live()
        {
            Capture();
            return processes.Where(process => Ended(process) != true).ToList();
        }

        internal bool? Ended(TProcess process)
        {
            try { return process == null ? (bool?)null : ended(process); } catch (Exception) { return null; }
        }
    }

    /// <summary>
    /// Recovery C3 (R7-F3): one attempt to install an observer's hooks. The attempt is registered before the constructor can
    /// partly succeed and throw; neither a constructor failure nor a null result implies zero acquisition. Publication reads
    /// the owned hooks back explicitly (by the observer's own identity, not through the returned object) and sends the setup
    /// failure, an unreadable readback and any remaining hook to the enclosing isolation as sticky failures.
    /// </summary>
    internal sealed class Sprint7HookAttempt<T> where T : class
    {
        private readonly string name;
        private readonly Func<int> readback;
        internal bool Attempted { get; private set; }
        internal T Acquired { get; private set; }
        internal string SetupFailure { get; private set; }

        internal Sprint7HookAttempt(string name, Func<int> readback)
        {
            this.name = name;
            this.readback = readback;
        }

        /// <summary>Registers the attempt, then acquires; a throwing acquisition is recorded and rethrown (the original error).</summary>
        internal T Acquire(Func<T> acquire)
        {
            Attempted = true;
            try { Acquired = acquire(); }
            catch (Exception error)
            {
                SetupFailure = name + " setup failed: " + error.GetType().Name + ": " + error.Message;
                throw;
            }
            if (Acquired == null) SetupFailure = name + " setup returned no observer";
            return Acquired;
        }

        /// <summary>The owned hooks read now; null when the read throws.</summary>
        internal int? Remaining()
        {
            try { return readback(); } catch (Exception) { return null; }
        }

        /// <summary>Reads the hooks back once and publishes; returns the readback (0 when never attempted).</summary>
        internal int? Publish(Sprint7CellIsolation isolation, string owner)
        {
            if (!Attempted) return 0;
            int? remaining = Remaining();
            if (SetupFailure != null) isolation.Fail(owner, SetupFailure);
            if (remaining == null) isolation.Fail(owner, name + " hook readback unreadable");
            else if (remaining.Value != 0) isolation.Fail(owner, name + " hooks remaining: " + remaining.Value);
            return remaining;
        }
    }

    /// <summary>
    /// Recovery C3 (R4-F1, R7-F5): the one abandonment driver every effects driver uses. It runs the phase's own cell sequence
    /// over a child isolation (the inner cell, then a next cell). In the abandonment mode it disposes that sequence once the
    /// inner cell is ready, with its injected fault set at that moment; in the run-to-completion mode the fault is set before
    /// the inner cell starts and the sequence runs to its end. The same admission guard then re-reads the child's residuals and
    /// refuses the next cell, and the driver repairs its own intervention. The child's complete outcome decides: every child
    /// failure is either one of the declared expected failures (the injected fault and its known residual) or unexpected, and
    /// an unexpected failure reaches the enclosing phase by name and is never exempted by the repair. The repair is attempted
    /// once on every exit, including the driver's own disposal during its recovery wait; a failed repair is kept. The driver
    /// reports the fixture safe only when every expected failure was seen, nothing unexpected failed, no residual remains, the
    /// next cell was refused and never started, the repair succeeded and the driver's explicit safety readback is clean.
    /// </summary>
    internal sealed class Sprint7Abandonment
    {
        internal string Owner;
        internal Sprint7CellIsolation Phase;
        internal readonly Sprint7CellIsolation Child = new Sprint7CellIsolation();
        internal Func<bool> Ready;               // null: run to completion
        internal Action Inject, Clear;
        internal Func<List<string>> Repair;
        internal Func<IEnumerable<int>> AfterRepair;   // a declared settle wait between the repair and the safety readback
        internal Func<List<string>> Unsafe;
        internal string[] Expected = new string[0];
        internal readonly JObject Row = new JObject();
        internal bool Proven { get; private set; }
        internal bool Repaired { get; private set; }
        internal readonly List<string> RepairFailures = new List<string>();

        internal List<string> Unexpected { get { return Child.Failures.Where(failure => !Expected.Any(marker => failure.Contains(marker))).ToList(); } }
        internal List<string> ExpectedMissing { get { return Expected.Where(marker => !Child.Failures.Any(failure => failure.Contains(marker))).ToList(); } }

        private void RepairOnce(string when)
        {
            if (Repaired) return;
            Repaired = true;
            Row["repairedWhen"] = when;
            try { RepairFailures.AddRange(Repair == null ? new List<string>() : (Repair() ?? new List<string>())); }
            catch (Exception error) { RepairFailures.Add("repair: " + error.GetType().Name + ": " + error.Message); }
        }

        private void ClearInjection()
        {
            try { if (Clear != null) Clear(); }
            catch (Exception error) { RepairFailures.Add("clear injection: " + error.GetType().Name + ": " + error.Message); }
        }

        internal IEnumerable<int> Run(KeyValuePair<string, Func<IEnumerable<int>>> inner, KeyValuePair<string, Func<IEnumerable<int>>> next,
            Func<string, Func<IEnumerable<int>>, IEnumerable<int>> run, TimeSpan settle, TimeSpan readyLimit)
        {
            Phase.Opened(Owner);
            Row["isolationRestored"] = false;
            Row["mode"] = Ready == null ? "run-to-completion" : "abandon-when-ready";
            IEnumerator<int> sequence = Sprint7ObserverPolicy.Sequence(Child, new[] { inner, next }, run, settle).GetEnumerator();
            try
            {
                bool ready;
                DateTime limit = DateTime.UtcNow + readyLimit;
                if (Ready == null)
                {
                    if (Inject != null) Inject();
                    bool more = true;
                    while (more && DateTime.UtcNow < limit) { more = sequence.MoveNext(); if (more) yield return sequence.Current; }
                    ready = !more;
                    ClearInjection();
                    sequence.Dispose();
                }
                else
                {
                    while (DateTime.UtcNow < limit && !Ready() && sequence.MoveNext()) yield return sequence.Current;
                    ready = Ready();
                    if (Inject != null) Inject();
                    try { sequence.Dispose(); } finally { ClearInjection(); }
                }
                Row["readyBeforeAbandon"] = ready;
                Row["childAfterDisposal"] = Child.Describe();
                // The same admission guard over the next cell, on the same child result (the recovery wait).
                foreach (int step in Sprint7ObserverPolicy.Sequence(Child, new[] { next }, run, settle)) yield return step;
                Row["childAfterResume"] = Child.Describe();
                RepairOnce("after the guard");
                if (AfterRepair != null) foreach (int step in AfterRepair()) yield return step;
                List<string> unsafeReasons;
                try { unsafeReasons = Unsafe == null ? new List<string>() : (Unsafe() ?? new List<string>()); }
                catch (Exception error) { unsafeReasons = new List<string> { "safety readback: " + error.GetType().Name + ": " + error.Message }; }
                Row["unsafe"] = new JArray(unsafeReasons.ToArray());
                Proven = ready && ExpectedMissing.Count == 0 && Unexpected.Count == 0 && Child.Pending.Count == 0 && Child.Refused == next.Key &&
                    !Child.Admitted.Contains(next.Key) && RepairFailures.Count == 0 && unsafeReasons.Count == 0;
            }
            finally
            {
                ClearInjection();
                sequence.Dispose();
                RepairOnce("on unwind");
                Row["expectedMissing"] = new JArray(ExpectedMissing.ToArray());
                Row["unexpected"] = new JArray(Unexpected.ToArray());
                Row["repairFailures"] = new JArray(RepairFailures.ToArray());
                Row["child"] = Child.Describe();
                Row["isolationRestored"] = Proven;
                foreach (string failure in Unexpected) Phase.Fail(Owner, "unexpected child failure: " + failure);
                foreach (string failure in RepairFailures) Phase.Fail(Owner, "repair failed: " + failure);
                if (!Proven) Phase.Fail(Owner, "the fixture was not proven safe after the driver's repair");
                Phase.Reported(Owner);
            }
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

        /// <summary>
        /// Recovery C3 (R7-F3): the effects phase's cell runner. A cell's setup, execution and disposal exceptions become sticky
        /// failures of the isolation it reports to (a cell that later loses its visible resource is still failed), and the
        /// cell's own finally always runs. <paramref name="completed"/> receives the cell's name and its exception (null when it
        /// ran to its end); it is not called when the runner itself is disposed mid-cell.
        /// </summary>
        internal static IEnumerable<int> IsolatedCell(Sprint7CellIsolation isolation, string name, Func<IEnumerable<int>> body, Action<string, Exception> completed)
        {
            IEnumerator<int> steps = null; Exception failure = null;
            try
            {
                try { steps = body().GetEnumerator(); } catch (Exception error) { failure = error; }
                while (failure == null)
                {
                    bool more = false;
                    try { more = steps.MoveNext(); } catch (Exception error) { failure = error; }
                    if (!more) break;
                    yield return steps.Current;
                }
            }
            finally
            {
                if (steps != null)
                    try { steps.Dispose(); }
                    catch (Exception error) { isolation.Fail(name, "cell disposal: " + error.GetType().Name + ": " + error.Message); }
            }
            if (failure != null) isolation.Fail(name, "cell exception: " + failure.GetType().Name + ": " + failure.Message);
            if (completed != null) completed(name, failure);
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
