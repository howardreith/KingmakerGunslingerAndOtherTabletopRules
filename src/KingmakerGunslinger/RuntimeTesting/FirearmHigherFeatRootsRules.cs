using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Pure evaluation rules for the higher-firearm-feat-roots fixture. The
    /// runtime assertions and the focused domain tests use the same logic, so
    /// corrupted evidence cannot pass here and pass there.
    /// </summary>
    internal static class FirearmHigherFeatRootsRules
    {
        /// <summary>Cancellation proof: the visit genuinely held the exact
        /// root/parameter choice in the preview, was cancelled without apply,
        /// and the underlying progression is unchanged — the cancelled target
        /// neither appears nor gains rank, no level changed, and no other
        /// tracked fact changed. The intentional prerequisite facts committed
        /// before the snapshot are part of the compared state, not an
        /// unrelated final count.</summary>
        internal static bool EvaluateCancellationEvidence(
            Newtonsoft.Json.Linq.JObject before, Newtonsoft.Json.Linq.JObject after,
            bool previewHeld, string targetRootGuid, string targetParamGuid,
            IList<string> failures)
        {
            if (failures == null) throw new ArgumentNullException("failures");
            if (before == null || after == null)
            { failures.Add("snapshot-missing"); return false; }
            if (!previewHeld) failures.Add("preview-did-not-hold-target");
            if ((int?)before["classLevel"] != (int?)after["classLevel"])
                failures.Add("class-level-changed");
            if ((int?)before["characterLevel"] != (int?)after["characterLevel"])
                failures.Add("character-level-changed");
            var beforeFacts = before["facts"] as Newtonsoft.Json.Linq.JArray;
            var afterFacts = after["facts"] as Newtonsoft.Json.Linq.JArray;
            if (beforeFacts == null || afterFacts == null)
            { failures.Add("fact-snapshot-missing"); return false; }
            if (!Newtonsoft.Json.Linq.JToken.DeepEquals(beforeFacts, afterFacts))
            {
                failures.Add("progression-facts-changed");
                // Name the exact divergence for the record.
                foreach (var row in beforeFacts.OfType<Newtonsoft.Json.Linq.JObject>())
                {
                    var guid = (string)row["rootGuid"];
                    var param = (string)row["paramGuid"];
                    var rankBefore = (int?)row["rank"];
                    var matching = afterFacts.OfType<Newtonsoft.Json.Linq.JObject>()
                        .FirstOrDefault(value => (string)value["rootGuid"] == guid && (string)value["paramGuid"] == param);
                    if (matching == null) failures.Add("fact-vanished:" + guid + "/" + param);
                    else if ((int?)matching["rank"] != rankBefore)
                        failures.Add("rank-changed:" + guid + "/" + param + ":" + rankBefore + "->" + matching["rank"]);
                }
                foreach (var row in afterFacts.OfType<Newtonsoft.Json.Linq.JObject>())
                {
                    var guid = (string)row["rootGuid"];
                    var param = (string)row["paramGuid"];
                    var exists = beforeFacts.OfType<Newtonsoft.Json.Linq.JObject>().Any(value =>
                        (string)value["rootGuid"] == guid && (string)value["paramGuid"] == param);
                    if (!exists) failures.Add("fact-appeared:" + guid + "/" + param);
                }
            }
            // The cancelled target specifically must not be present.
            if (afterFacts.OfType<Newtonsoft.Json.Linq.JObject>().Any(value =>
                    (string)value["rootGuid"] == targetRootGuid &&
                    (string)value["paramGuid"] == targetParamGuid))
                failures.Add("cancelled-target-present");
            return failures.Count == 0;
        }

        /// <summary>Committed-parameter proof: the observed fact carries the
        /// expected native root and the shared Weapon Focus parameter of the
        /// kind at rank 1. The per-root wrapper GUID is explicitly supplied so
        /// substituting it for the parameter can never pass.</summary>
        internal static bool EvaluateCommittedParameter(
            string observedRootGuid, string observedParamGuid,
            string expectedRootGuid, string expectedParamGuid, string wrapperRootGuid,
            int rank, IList<string> failures)
        {
            if (failures == null) throw new ArgumentNullException("failures");
            if (!string.Equals(observedRootGuid, expectedRootGuid, StringComparison.Ordinal))
                failures.Add("root-mismatch");
            if (string.Equals(observedParamGuid, wrapperRootGuid, StringComparison.Ordinal))
                failures.Add("wrapper-substituted-as-parameter");
            if (!string.Equals(observedParamGuid, expectedParamGuid, StringComparison.Ordinal))
                failures.Add("parameter-mismatch");
            if (rank != 1) failures.Add("rank-not-one");
            return failures.Count == 0;
        }
    }
}
