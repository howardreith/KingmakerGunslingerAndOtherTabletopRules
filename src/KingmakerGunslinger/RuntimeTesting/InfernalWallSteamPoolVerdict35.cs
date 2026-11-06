using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Wall steam pool-reuse acceptance over the recorded probe receipt.
    /// Reuse is proven only when a cloud the Wall modified (flame renderers
    /// switched off) was captured from a live Wall steam area, reached the
    /// native pool, and the ordinary native claim returned that same
    /// instance; the renderer comparison must cover the complete, nonzero,
    /// duplicate-free set of prefab renderers with nothing missing or extra,
    /// no renderer may differ from the prefab, and the Wall must hold no
    /// cloud ownership after release. A different (pristine) instance, a
    /// missing reuse identity, or an empty or incomplete comparison is not
    /// reuse.
    /// </summary>
    internal static class InfernalWallSteamPoolVerdict35
    {
        internal static string Failure(JObject receipt)
        {
            if (receipt == null) return "no-receipt";
            if ((bool?)receipt["probeRan"] != true) return "probe-not-run";
            var modified = new HashSet<long>();
            foreach (var area in (receipt["liveWallSteamAreas"] as JArray ?? new JArray()).OfType<JObject>())
            {
                long? cloud = (long?)area["cloud"];
                if (cloud.HasValue && ((int?)area["flameRenderersOff"] ?? 0) > 0) modified.Add(cloud.Value);
            }
            if (modified.Count == 0) return "no-captured-wall-modified-cloud";
            var released = new HashSet<long>();
            foreach (var row in (receipt["releasedClouds"] as JArray ?? new JArray()).OfType<JObject>())
            {
                long? cloud = (long?)row["cloud"];
                if (cloud.HasValue && modified.Contains(cloud.Value) && (bool?)row["inPool"] == true)
                    released.Add(cloud.Value);
            }
            if (released.Count == 0) return "captured-cloud-not-released-to-native-pool";
            long? claimed = (long?)receipt["claimed"];
            if (!claimed.HasValue) return "no-claimed-instance";
            if (!released.Contains(claimed.Value)) return "claim-did-not-reuse-a-captured-cloud";
            int expected = (int?)receipt["prefabRenderers"] ?? 0;
            if (expected <= 0) return "empty-expected-renderer-set";
            if (((int?)receipt["prefabDuplicateRendererKeys"] ?? -1) != 0) return "expected-renderer-keys-not-unique";
            if (((int?)receipt["claimedDuplicateRendererKeys"] ?? -1) != 0) return "claimed-renderer-keys-not-unique";
            if (((int?)receipt["claimedMissingRendererKeys"] ?? -1) != 0) return "claimed-renderers-missing";
            if (((int?)receipt["claimedExtraRendererKeys"] ?? -1) != 0) return "claimed-renderers-extra";
            if (((int?)receipt["claimedRenderersCompared"] ?? 0) != expected) return "incomplete-renderer-comparison";
            var differing = receipt["claimedRenderersDifferingFromPrefab"] as JArray;
            if (differing == null) return "no-difference-record";
            if (differing.Count != 0) return "renderer-state-differs";
            if (((int?)receipt["ownedCloudsAfterRelease"] ?? -1) != 0) return "wall-ownership-not-cleared";
            return null;
        }

        internal static bool Reused(JObject receipt) { return Failure(receipt) == null; }
    }
}
