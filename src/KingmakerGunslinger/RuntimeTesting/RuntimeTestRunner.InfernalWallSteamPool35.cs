using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.Particles;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Wall steam draws the native kineticist steam cloud with its flame
        // renderers switched off. Installed FxHelper.SpawnFxOnGameObject
        // claims that cloud from GameObjectsPool, and native area teardown
        // (AreaEffectView.OnDestroy -> FxHelper.Destroy) releases it back;
        // installed PooledFx.OnClaim/OnRelease reset particle, sorting, snap
        // and deactivatable-component state but not Renderer.enabled. This
        // probe captures the live Wall steam areas' clouds, waits for their
        // actual release after the caller's native end route, then claims the same prefab
        // exactly as a native consumer does and compares every renderer's
        // enabled flag with the prefab's own. The release route is the
        // native one the caller runs between capture and claim (the source
        // Bone Devil's retirement ends its surviving steam area).
        private static string InfernalWallRelativePath35(Transform root, Transform node)
        {
            var parts = new List<string>();
            for (Transform current = node; current != null && current != root; current = current.parent)
                parts.Add(current.name);
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private static List<GameObject> InfernalWallSteamCapture35(InfernalWallRun35 run, JObject row)
        {
            var areas = Game.Instance.State.AreaEffects.All.Where(area => area != null && !area.IsEnded &&
                ReferenceEquals(area.Blueprint, run.Fog) && ReferenceEquals(area.Context?.SourceAbility, run.Wall)).ToArray();
            var clouds = new List<GameObject>();
            var areaRows = new JArray();
            foreach (var area in areas)
            {
                var cloud = ReadExpandedSummoningOptionalMember(area.View, "m_SpawnedFx") as GameObject;
                areaRows.Add(new JObject { ["area"] = area.UniqueId, ["cloud"] = cloud == null ? (int?)null : cloud.GetInstanceID(),
                    ["cloudName"] = cloud == null ? null : cloud.name,
                    ["flameRenderersOff"] = cloud == null ? 0 : cloud.GetComponentsInChildren<Renderer>(true).Count(value =>
                        value != null && InfernalRules35.WallSteamHidesLayer(value.name) && !value.enabled) });
                if (cloud != null) clouds.Add(cloud);
            }
            row["liveWallSteamAreas"] = areaRows;
            return clouds;
        }

        private IEnumerable<int> InfernalWallSteamPool35(InfernalWallRun35 run, List<GameObject> clouds, JObject row,
            string endRoute, Action<JObject> result)
        {
            row["scope"] = "Native pool reuse of the Wall steam cloud prefab by an ordinary native claim; no pool state written by the observer.";
            row["endRoute"] = endRoute;
            GameObject prefab = new PrefabLink { AssetId = InfernalRules35.WallSteamFxAssetId }.Load(true);
            row["prefab"] = prefab == null ? null : prefab.name;
            if (prefab == null) { row["probeRan"] = false; result(row); yield break; }
            PooledGameObject pooled = prefab.GetComponent<PooledGameObject>();
            row["prefabPoolComponent"] = pooled == null ? null : pooled.GetType().FullName;
            row["prefabPoolComponentEnabled"] = pooled != null && pooled.enabled;
            row["poolDisabled"] = GameObjectsPool.Disabled;
            row["prefabFadeOut"] = prefab.GetComponent<FxFadeOut>() == null ? (float?)null : prefab.GetComponent<FxFadeOut>().Duration;
            // Renderers are matched by hierarchy path and type. A duplicate
            // key on either side would let a comparison skip a renderer, so
            // duplicates, missing and extra keys are counted, not ignored.
            var authored = new Dictionary<string, bool>();
            int prefabDuplicates = 0;
            foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                string key = InfernalWallRelativePath35(prefab.transform, renderer.transform) + "#" + renderer.GetType().Name;
                if (authored.ContainsKey(key)) prefabDuplicates++;
                else authored.Add(key, renderer.enabled);
            }
            row["prefabRenderers"] = authored.Count;
            row["prefabDuplicateRendererKeys"] = prefabDuplicates;
            row["prefabRenderersDisabled"] = new JArray(authored.Where(value => !value.Value).Select(value => value.Key));

            DateTime releaseDeadline = DateTime.UtcNow.AddSeconds(12);
            while (DateTime.UtcNow < releaseDeadline && clouds.Any(cloud => cloud != null && !GameObjectsPool.IsInPool(cloud)))
            { Game.Instance.IsPaused = false; yield return 0; }
            row["releasedClouds"] = new JArray(clouds.Select(cloud => new JObject {
                ["cloud"] = ReferenceEquals(cloud, null) ? (int?)null : cloud.GetInstanceID(),
                ["destroyed"] = cloud == null, ["inPool"] = cloud != null && GameObjectsPool.IsInPool(cloud),
                ["activeSelf"] = cloud != null && cloud.activeSelf,
                ["flameRenderersOffInPool"] = cloud == null ? 0 : cloud.GetComponentsInChildren<Renderer>(true).Count(value =>
                    value != null && InfernalRules35.WallSteamHidesLayer(value.name) && !value.enabled) }));

            var anchor = new GameObject("KMG_S35_SteamPoolProbe");
            anchor.transform.position = run.Fixture.Caster.Position + Vector3.up * 40f;
            GameObject claimed = null;
            try
            {
                claimed = FxHelper.SpawnFxOnGameObject(prefab, anchor);
                row["claimed"] = claimed == null ? (int?)null : claimed.GetInstanceID();
                row["claimedReusedWallCloud"] = claimed != null && clouds.Any(cloud => cloud != null && ReferenceEquals(cloud, claimed));
                var differing = new JArray();
                var seen = new HashSet<string>();
                int compared = 0, claimedDuplicates = 0, extra = 0;
                if (claimed != null)
                    foreach (Renderer renderer in claimed.GetComponentsInChildren<Renderer>(true))
                    {
                        if (renderer == null) continue;
                        string key = InfernalWallRelativePath35(claimed.transform, renderer.transform) + "#" + renderer.GetType().Name;
                        if (!seen.Add(key)) { claimedDuplicates++; continue; }
                        bool expected;
                        if (!authored.TryGetValue(key, out expected)) { extra++; continue; }
                        compared++;
                        if (renderer.enabled != expected)
                            differing.Add(new JObject { ["renderer"] = key, ["enabled"] = renderer.enabled, ["prefab"] = expected });
                    }
                row["claimedRenderersCompared"] = compared;
                row["claimedDuplicateRendererKeys"] = claimedDuplicates;
                row["claimedExtraRendererKeys"] = extra;
                row["claimedMissingRendererKeys"] = authored.Keys.Count(key => !seen.Contains(key));
                row["claimedRenderersDifferingFromPrefab"] = differing;
                row["probeRan"] = claimed != null;
            }
            finally
            {
                if (claimed != null) FxHelper.Destroy(claimed);
                UnityEngine.Object.Destroy(anchor);
            }
            result(row);
        }
    }
}
