using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // These counters are already scalar evidence, not game objects. Never
    // send them through the active game's process-global save converters.
    internal static class Sprint17GripEvidence
    {
        internal static JObject Counters(IDictionary<string, int> counters)
        {
            if (counters == null) throw new ArgumentNullException("counters");
            return new JObject(counters.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new JProperty(pair.Key, pair.Value)));
        }

        private const double GripMeters = .08;
        private const string ActMethod = "Kingmaker.Visual.Animation.AnimationClipEventsCache+<>c.<CreateEvent>b__3_12";

        private static double ActTime(string clip)
        {
            return clip == "Human_2H_spear_attack_01" ? .620427966 :
                clip == "Human_2H_spear_attack_02" ? .734528542 : -1;
        }

        private static bool Number(JObject sample, string key, out double value)
        {
            value = double.NaN;
            JToken token = sample == null ? null : sample[key];
            if (token == null || (token.Type != JTokenType.Float && token.Type != JTokenType.Integer)) return false;
            value = (double)token;
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
        }

        private static string SampleRejection(JObject sample, string clip)
        {
            if (sample == null || (bool?)sample["available"] != true || (string)sample["clip"] != clip)
                return "missing-same-pose-control";
            foreach (string key in new[] { "nativeLeft", "nativeRight", "originalLeft", "originalRight", "clipTime", "handleTime" })
            { double value; if (!Number(sample, key, out value)) return "non-finite-or-negative-sample"; }
            double nativeLeft = (double)sample["nativeLeft"], originalLeft = (double)sample["originalLeft"];
            if ((double)sample["nativeRight"] > GripMeters || (double)sample["originalRight"] > GripMeters)
                return "weapon-hand-lost-grip";
            if (Math.Abs(originalLeft - nativeLeft) > GripMeters) return "lead-hand-diverges-from-native";
            if (nativeLeft <= GripMeters && originalLeft > GripMeters) return "lead-hand-lost-native-grip";
            return null;
        }

        // Native02's lead hand is released by its actual IsActed/rule frames
        // during transition-out. Requiring both hands on the shaft at that rule
        // frame rejects the unmodified native control too. Compare EVERY
        // observed frame, not a chosen earlier pose: retain the weapon hand,
        // retain both grips whenever native does, and bound native-following
        // lead-hand separation by the SAME 8cm tolerance. Never move a bone,
        // choose an attack variant, shift a rule event or infer missing frames.
        internal static string SpearGripRejection(JArray timeline, JArray nativeEvents, JObject[] contacts)
        {
            try
            {
                if (timeline == null || timeline.Count < 8 || timeline.Count > 512 ||
                    nativeEvents == null || nativeEvents.Count != 2 || contacts == null || contacts.Length != 2 ||
                    timeline.Any(t => !(t is JObject) || ((int?)t["handleId"] != 0 && (int?)t["handleId"] != 1)))
                    return "incomplete-grip-review";
                for (int handle = 0; handle < 2; handle++)
                {
                    JObject contact = contacts[handle];
                    JObject rule = contact == null ? null : contact["samePoseGripControl"] as JObject;
                    JObject end = contact == null ? null : contact["endOfFrame"] as JObject;
                    JObject rendered = end == null ? null : end["samePoseGripControl"] as JObject;
                    JObject events = nativeEvents[handle] as JObject;
                    string clip = rule == null ? null : (string)rule["clip"];
                    double actTime = ActTime(clip);
                    if (actTime < 0 || events == null || (int?)events["handleId"] != handle ||
                        (string)events["clip"] != clip || (bool?)events["eventInvoked"] != false ||
                        (bool?)events["cacheProviderInvoked"] != false) return "unreviewed-native-event";
                    var eventList = events["events"] as JArray;
                    var acts = eventList == null ? new JObject[0] : eventList.OfType<JObject>()
                        .Where(e => (string)e["method"] == ActMethod).ToArray();
                    double cachedTime;
                    if (acts.Length != 1 || !Number(acts[0], "time", out cachedTime) ||
                        Math.Abs(cachedTime - actTime) > .000001) return "changed-native-act-event";
                    string rejection = SampleRejection(rule, clip) ?? SampleRejection(rendered, clip);
                    if (rejection != null) return rejection;
                    if ((int?)rule["handleId"] != handle || (int?)rendered["handleId"] != handle ||
                        (string)rule["phase"] != "rule-event" || (string)rendered["phase"] != "end-of-rule-frame" ||
                        (bool?)rule["acted"] != true || (bool?)rendered["acted"] != true ||
                        (int?)rule["frame"] != (int?)rendered["frame"] || (bool?)end["sameFrameAndHandle"] != true ||
                        (double)rule["clipTime"] < actTime) return "uncorrelated-rule-frame";
                    if ((double)rule["nativeLeft"] > GripMeters &&
                        (clip != "Human_2H_spear_attack_02" || (string)rule["state"] != "TransitioningOut"))
                        return "unreviewed-rule-frame-release";
                    JObject[] samples = timeline.OfType<JObject>().Where(s => (int?)s["handleId"] == handle).ToArray();
                    if (samples.Length < 8) return "missing-handle-timeline";
                    bool hasActed = false; double previousTime = -1;
                    foreach (JObject sample in samples)
                    {
                        rejection = SampleRejection(sample, clip); if (rejection != null) return rejection;
                        bool? acted = (bool?)sample["acted"];
                        if (!acted.HasValue || (hasActed && !acted.Value) || (double)sample["clipTime"] < previousTime)
                            return "missing-or-reversed-act-playback";
                        hasActed |= acted.Value; previousTime = (double)sample["clipTime"];
                    }
                    var frames = samples.GroupBy(s => (int)s["frame"]).ToArray();
                    for (int index = 0; index < frames.Length; index++)
                        if (frames[index].Key < 0 || (index > 0 && frames[index].Key != frames[index - 1].Key + 1) ||
                            !frames[index].Select(s => (string)s["phase"]).SequenceEqual(new[] { "LateUpdate", "EndOfFrame" }))
                            return "missing-reordered-or-duplicate-frame";
                    if (!frames.Any(f => f.Key == (int)rule["frame"]) ||
                        samples.Where(s => (bool?)s["acted"] == false && (double)s["nativeLeft"] <= GripMeters)
                            .Select(s => (int)s["frame"]).Distinct().Count() < 2 ||
                        samples.Where(s => (bool?)s["acted"] == true).Select(s => (int)s["frame"]).Distinct().Count() < 2)
                        return "missing-grip-or-act-transition";
                }
                return null;
            }
            catch (Exception error) { return "malformed-grip-evidence:" + error.GetType().Name; }
        }
    }
}
