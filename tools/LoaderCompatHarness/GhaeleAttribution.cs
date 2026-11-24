using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KmgLoaderCompatHarness
{
    // Sprint 7 T1 / REP-04 (review R3-F6): which in-life controller snapshots attribute the Ghaele body's _Dissolve to
    // the native donor's own loop. Nothing here touches the game (tests: sprint7-visual.ghaele-attribution).

    /// <summary>
    /// A snapshot is positive only when it shows an active (started, not delayed), looping, non-cutout animation whose
    /// signature the native donor also runs, and at least one readable driven material equal to that animation's
    /// current value; when the snapshot records the body renderer's materials, that driven material must be one of
    /// them. Any animation the donor does not run, any cutout animation, or an active animation whose readable driven
    /// material disagrees contradicts. Everything else (no controller, no animation, only inactive or delayed
    /// animations, no readable driven value, a driven material that is not the body's) is inconclusive and never
    /// confirms. Attribution needs at least <see cref="MinimumPositive"/> positive snapshots and no contradiction.
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

        /// <returns>"positive", "contradicting" or "inconclusive:&lt;why&gt;".</returns>
        internal static string Classify(JToken snapshot, ICollection<string> donorSignatures)
        {
            if (donorSignatures == null || donorSignatures.Count == 0) return Inconclusive + ":no-donor-signature";
            JToken controllers = snapshot == null ? null : snapshot["controllers"];
            if (controllers == null || !controllers.Any()) return Inconclusive + ":no-controller";
            JToken bodyMaterials = snapshot["bodyMaterials"];
            var body = bodyMaterials == null || bodyMaterials.Type != JTokenType.Array ? null : new HashSet<long>(bodyMaterials.Select(value => (long)value));
            bool anyAnimation = false, anyActive = false, anyReadable = false, loopMatched = false, positive = false;
            foreach (JToken controller in controllers)
            {
                JToken driven = controller["driven"] ?? new JArray();
                foreach (JToken animation in controller["animations"] ?? new JArray())
                {
                    anyAnimation = true;
                    if (!donorSignatures.Contains(Signature(animation)) || (bool)animation["DissolveCutout"]) return Contradicting;
                    bool active = (bool)animation["IsStarted"] && !(bool)animation["IsDelayed"];
                    if (!active) continue;
                    anyActive = true;
                    double current = (double)animation["CurrentDissolve"];
                    foreach (JToken material in driven)
                    {
                        double value = (double)material["dissolve"];
                        if (value < 0) continue;
                        anyReadable = true;
                        if (Math.Abs(value - current) > .001) return Contradicting;
                        bool onBody = body == null || body.Contains((long)material["material"]);
                        if (!(bool)animation["LoopAnimation"]) continue;
                        loopMatched = true;
                        if (onBody) positive = true;
                    }
                }
            }
            if (positive) return Positive;
            if (!anyAnimation) return Inconclusive + ":no-animation";
            if (!anyActive) return Inconclusive + ":no-active-animation";
            if (!anyReadable) return Inconclusive + ":no-readable-driven-value";
            return Inconclusive + (loopMatched ? ":no-driven-body-material" : ":no-active-loop");
        }

        internal static JObject Evaluate(IEnumerable<JToken> inLifeSnapshots, ICollection<string> donorSignatures)
        {
            var rows = (inLifeSnapshots ?? Enumerable.Empty<JToken>()).ToList();
            var classes = rows.Select(row => Classify(row, donorSignatures)).ToList();
            int positive = classes.Count(value => value == Positive), contradicting = classes.Count(value => value == Contradicting);
            bool bodyRecorded = rows.Count > 0 && rows.All(row => row["bodyMaterials"] != null && row["bodyMaterials"].Type == JTokenType.Array);
            return new JObject
            {
                ["snapshots"] = rows.Count, ["positive"] = positive, ["contradicting"] = contradicting,
                ["inconclusive"] = classes.Count - positive - contradicting,
                ["classes"] = new JArray(classes.Cast<object>().ToArray()),
                ["bodyMaterialIdentity"] = bodyRecorded ? "recorded" : "not-recorded",
                ["attributed"] = donorSignatures != null && donorSignatures.Count > 0 && contradicting == 0 && positive >= MinimumPositive
            };
        }
    }
}
