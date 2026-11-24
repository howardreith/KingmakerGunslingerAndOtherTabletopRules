using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KmgLoaderCompatHarness
{
    // Sprint 7 T1 / REP-04: verdicts of the Ghaele harness probe that need no game (tests: sprint7-visual.ghaele-*).

    /// <summary>
    /// Review R3-F6: which in-life controller snapshots attribute the Ghaele body's _Dissolve to the native donor's own
    /// loop. The donor contributes two sets: every animation it configures (an animation outside that set contradicts)
    /// and the qualifying loops it was actually observed running: active (started, not delayed), looping, non-cutout,
    /// with a readable driven material equal to the animation's current value. A summon snapshot is positive only when
    /// one of its active looping non-cutout animations has a qualifying donor signature and drives a readable body
    /// material to that value. Current snapshots carry the "bodyMaterials" property: an explicit null (body unreadable)
    /// cannot count positively, but its readable controller evidence is still examined first, so a contradiction on such
    /// a row refuses attribution (review R5-F2); without one it is inconclusive. Legacy rows without the property are
    /// judged without body identity and labelled so. Any
    /// cutout animation, any animation the donor does not configure, or an active animation whose readable driven
    /// material disagrees contradicts. Attribution needs at least <see cref="MinimumPositive"/> positive snapshots and
    /// no contradiction.
    /// </summary>
    internal static class GhaeleAttribution
    {
        internal const int MinimumPositive = 5;
        internal const string Positive = "positive";
        internal const string Contradicting = "contradicting";
        internal const string Inconclusive = "inconclusive";

        internal static string Signature(JToken animation)
        {
            return string.Format(CultureInfo.InvariantCulture, "delay={0:F2} lifetime={1:F2} loop={2} cutout={3} backwards={4}",
                (double)animation["Delay"], (double)animation["Lifetime"], (bool)animation["LoopAnimation"], (bool)animation["DissolveCutout"], (bool)animation["PlayBackwards"]);
        }

        private static bool Active(JToken animation)
        {
            return (bool)animation["IsStarted"] && !(bool)animation["IsDelayed"];
        }

        /// <summary>Every animation signature the donor configures, in any snapshot.</summary>
        internal static HashSet<string> Configured(IEnumerable<JToken> donorSnapshots)
        {
            return new HashSet<string>((donorSnapshots ?? Enumerable.Empty<JToken>()).SelectMany(row => row["controllers"] ?? new JArray())
                .SelectMany(controller => controller["animations"] ?? new JArray()).Select(Signature));
        }

        /// <summary>Loops the donor was observed running: active, looping, non-cutout, driving a readable material to their own value.</summary>
        internal static HashSet<string> Qualifying(IEnumerable<JToken> donorSnapshots)
        {
            var result = new HashSet<string>();
            foreach (JToken controller in (donorSnapshots ?? Enumerable.Empty<JToken>()).SelectMany(row => row["controllers"] ?? new JArray()))
                foreach (JToken animation in controller["animations"] ?? new JArray())
                {
                    if (!Active(animation) || !(bool)animation["LoopAnimation"] || (bool)animation["DissolveCutout"]) continue;
                    double current = (double)animation["CurrentDissolve"];
                    if ((controller["driven"] ?? new JArray()).Any(material => (double)material["dissolve"] >= 0 && Math.Abs((double)material["dissolve"] - current) <= .001))
                        result.Add(Signature(animation));
                }
            return result;
        }

        /// <returns>"positive", "contradicting" or "inconclusive:&lt;why&gt;".</returns>
        internal static string Classify(JToken snapshot, ICollection<string> configured, ICollection<string> qualifying)
        {
            if (configured == null || configured.Count == 0) return Inconclusive + ":no-donor-signature";
            JToken controllers = snapshot == null ? null : snapshot["controllers"];
            if (controllers == null || !controllers.Any()) return Inconclusive + ":no-controller";
            var row = snapshot as JObject;
            bool current = row != null && row.Property("bodyMaterials") != null;
            JToken bodyMaterials = snapshot["bodyMaterials"];
            // R5-F2: an unreadable body withholds positive credit only; contradictions are evaluated first.
            bool unreadableBody = current && (bodyMaterials == null || bodyMaterials.Type != JTokenType.Array);
            HashSet<long> body = current && !unreadableBody ? new HashSet<long>(bodyMaterials.Select(value => (long)value)) : null;
            bool anyAnimation = false, anyActive = false, anyReadable = false, loopMatched = false, qualifyingLoop = false, positive = false;
            foreach (JToken controller in controllers)
            {
                JToken driven = controller["driven"] ?? new JArray();
                foreach (JToken animation in controller["animations"] ?? new JArray())
                {
                    anyAnimation = true;
                    if (!configured.Contains(Signature(animation)) || (bool)animation["DissolveCutout"]) return Contradicting;
                    if (!Active(animation)) continue;
                    anyActive = true;
                    double value = (double)animation["CurrentDissolve"];
                    foreach (JToken material in driven)
                    {
                        double read = (double)material["dissolve"];
                        if (read < 0) continue;
                        anyReadable = true;
                        if (Math.Abs(read - value) > .001) return Contradicting;
                        if (!(bool)animation["LoopAnimation"]) continue;
                        loopMatched = true;
                        if (qualifying == null || !qualifying.Contains(Signature(animation))) continue;
                        qualifyingLoop = true;
                        if (!unreadableBody && (body == null || body.Contains((long)material["material"]))) positive = true;
                    }
                }
            }
            if (unreadableBody) return Inconclusive + ":no-readable-body";
            if (positive) return Positive;
            if (!anyAnimation) return Inconclusive + ":no-animation";
            if (!anyActive) return Inconclusive + ":no-active-animation";
            if (!anyReadable) return Inconclusive + ":no-readable-driven-value";
            if (!loopMatched) return Inconclusive + ":no-active-loop";
            if (!qualifyingLoop) return Inconclusive + ":donor-loop-not-observed-running";
            return Inconclusive + ":no-driven-body-material";
        }

        internal static JObject Evaluate(IEnumerable<JToken> inLifeSnapshots, IEnumerable<JToken> donorSnapshots)
        {
            var donors = (donorSnapshots ?? Enumerable.Empty<JToken>()).ToList();
            HashSet<string> configured = Configured(donors), qualifying = Qualifying(donors);
            var rows = (inLifeSnapshots ?? Enumerable.Empty<JToken>()).ToList();
            var classes = rows.Select(row => Classify(row, configured, qualifying)).ToList();
            int positive = classes.Count(value => value == Positive), contradicting = classes.Count(value => value == Contradicting);
            bool bodyRecorded = rows.Count > 0 && rows.All(row => row is JObject && ((JObject)row).Property("bodyMaterials") != null);
            return new JObject
            {
                ["snapshots"] = rows.Count, ["positive"] = positive, ["contradicting"] = contradicting,
                ["inconclusive"] = classes.Count - positive - contradicting,
                ["classes"] = new JArray(classes.Cast<object>().ToArray()),
                ["donorConfigured"] = new JArray(configured.OrderBy(value => value, StringComparer.Ordinal).Cast<object>().ToArray()),
                ["donorQualifying"] = new JArray(qualifying.OrderBy(value => value, StringComparer.Ordinal).Cast<object>().ToArray()),
                ["bodyMaterialIdentity"] = bodyRecorded ? "recorded" : "not-recorded",
                ["attributed"] = qualifying.Count > 0 && contradicting == 0 && positive >= MinimumPositive
            };
        }
    }

    /// <summary>
    /// Review R4-F6: a lighting observation counts only with a readable body whose cutout is clear and a usable party
    /// capture of that body: an image written, the unit in frame, a lit screen with the fade cleared, the capture's
    /// renderer enabled, the unit's own fader reporting it visible (review R5-F3) and the renderer carrying the body's own
    /// mesh; and an interpolated light-probe reading.
    /// </summary>
    internal static class GhaeleLighting
    {
        internal static bool Valid(JToken row, out string reason)
        {
            reason = null;
            if (row == null) { reason = "no-row"; return false; }
            if (row["body"] == null || row["body"].Type == JTokenType.Null) { reason = "no-body"; return false; }
            if (row["bodyDissolveCutout"] == null || (double)row["bodyDissolveCutout"] < 0 || (double)row["bodyDissolveCutout"] > .02) { reason = "cutout"; return false; }
            if (row["probeDcLuminance"] == null || row["probeError"] != null) { reason = "no-light-probe"; return false; }
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string part in ((string)row["partyCapture"] ?? string.Empty).Split(';'))
            {
                int equals = part.IndexOf('=');
                if (equals > 0 && !fields.ContainsKey(part.Substring(0, equals))) fields[part.Substring(0, equals)] = part.Substring(equals + 1);
            }
            Func<string, string> field = name => fields.ContainsKey(name) ? fields[name] : null;
            double fade;
            if (field("png") == null || field("png") == "<none>") reason = "no-image";
            else if (field("inFrame") != "true") reason = "out-of-frame";
            else if (field("screenLit") != "true") reason = "unlit";
            else if (!double.TryParse(field("screenFade"), NumberStyles.Float, CultureInfo.InvariantCulture, out fade) || fade > .05) reason = "screen-fade";
            else if (field("rendererEnabled") != "true") reason = "renderer-disabled";
            else if (field("faderVisible") == null) reason = "fader-unavailable";
            else if (field("faderVisible") != "true") reason = "fader-hidden";
            else if (field("mesh") == null || field("mesh") != (string)row["bodyMesh"]) reason = "wrong-body";
            // The helper's "intact" flag reads the raw _Dissolve, which on the Ghaele is the native non-cutout loop
            // attributed above; the body's own cutout readback (checked first) is what removes surface.
            return reason == null;
        }
    }

    /// <summary>
    /// Review R5-F1: the cast abandoned with its command pending. Immediately before disposal (after every control was set
    /// up) the exact command must still be queued without an execution and the exact ability loan still held; after the
    /// observation window the command and grant are settled, the known foreign summon survives, the independent
    /// execution-correlated witness saw no owned summon and no unattributed same-caster Ghaele, and its subscription
    /// ended.
    /// </summary>
    internal static class GhaelePending
    {
        internal static string Verdict(bool commandPendingBeforeDispose, bool loanHeldBeforeDispose, bool commandRemains, bool grantRemains,
            bool foreignSurvived, int witnessOwnedSummons, int witnessUnattributedSummons, bool witnessSubscribedAfter)
        {
            if (!commandPendingBeforeDispose) return "command-not-pending-at-disposal";
            if (!loanHeldBeforeDispose) return "loan-not-held-at-disposal";
            if (commandRemains) return "command-remains";
            if (grantRemains) return "grant-remains";
            if (!foreignSurvived) return "foreign-summon-lost";
            if (witnessOwnedSummons != 0) return "late-owned-summon";
            if (witnessUnattributedSummons != 0) return "unattributed-same-caster-summon";
            if (witnessSubscribedAfter) return "witness-still-subscribed";
            return null;
        }
    }

    /// <summary>
    /// Review R4-F2: the combat teardown releases only the probe's own attacks and target. A participant still fighting
    /// afterwards is left alone; it is a reported conflict unless the engagement holding it belongs to the independent
    /// control the observation set up (then it must still be held), and every participant that was idle at intake must
    /// leave combat natively once that control is released.
    /// </summary>
    internal static class GhaeleCombat
    {
        internal static string Verdict(int ownedAttacksLeft, bool targetDestroyRequested, bool controlPreserved, bool controlEngagementHeld,
            int unexplainedInCombat, bool witnessSurvived, bool witnessSettled, bool nativeLeaveObserved)
        {
            if (ownedAttacksLeft != 0) return "owned-attack-left";
            if (!targetDestroyRequested) return "target-not-released";
            if (!controlPreserved) return "independent-control-not-preserved";
            if (!controlEngagementHeld) return "independent-engagement-not-held";
            if (unexplainedInCombat != 0) return "conflict:unowned-engagement";
            if (!witnessSurvived) return "witness-interrupted";
            if (!witnessSettled) return "witness-not-settled";
            if (!nativeLeaveObserved) return "native-leave-not-observed";
            return null;
        }
    }

    /// <summary>
    /// Review R4-F3: the cast's ability loan judged against the exact state the cast held. A borrowed ability must be the
    /// same instance carrying the same parameter object as before the cast; a temporary grant must no longer be the
    /// unit's ability (absent, or a distinct replacement).
    /// </summary>
    internal static class GhaeleLoan
    {
        internal static bool Released(bool borrowed, object current, object existing, object granted, object currentParams, object priorParams, out string reason)
        {
            if (borrowed)
                reason = current == null ? "borrowed-missing" : !ReferenceEquals(current, existing) ? "borrowed-replaced" :
                    !ReferenceEquals(currentParams, priorParams) ? "parameters-not-restored" : null;
            else
                reason = granted == null ? "no-grant" : ReferenceEquals(current, granted) ? "grant-retained" : null;
            return reason == null;
        }
    }
}
