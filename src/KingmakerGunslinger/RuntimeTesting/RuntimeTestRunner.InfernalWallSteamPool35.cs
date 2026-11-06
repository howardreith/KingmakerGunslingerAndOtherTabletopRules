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
        // probe ends the live Wall steam areas by the native area end route,
        // waits for their clouds' actual release, then claims the same prefab
        // exactly as a native consumer does and compares every renderer's
        // enabled flag with the prefab's own.
        private static string InfernalWallRelativePath35(Transform root, Transform node)
        {
            var parts = new List<string>();
            for (Transform current = node; current != null && current != root; current = current.parent)
                parts.Add(current.name);
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private IEnumerable<int> InfernalWallSteamPool35(InfernalWallRun35 run, Action<JObject> result)
        {
            var row = new JObject { ["scope"] = "Native pool reuse of the Wall steam cloud prefab by an ordinary native claim; no pool state written by the observer." };
            GameObject prefab = new PrefabLink { AssetId = InfernalRules35.WallSteamFxAssetId }.Load(true);
            row["prefab"] = prefab == null ? null : prefab.name;
            if (prefab == null) { row["probeRan"] = false; result(row); yield break; }
            PooledGameObject pooled = prefab.GetComponent<PooledGameObject>();
            row["prefabPoolComponent"] = pooled == null ? null : pooled.GetType().FullName;
            row["prefabPoolComponentEnabled"] = pooled != null && pooled.enabled;
            row["poolDisabled"] = GameObjectsPool.Disabled;
            row["prefabFadeOut"] = prefab.GetComponent<FxFadeOut>() == null ? (float?)null : prefab.GetComponent<FxFadeOut>().Duration;
            var authored = new Dictionary<string, bool>();
            foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                string key = InfernalWallRelativePath35(prefab.transform, renderer.transform) + "#" + renderer.GetType().Name;
                if (!authored.ContainsKey(key)) authored.Add(key, renderer.enabled);
            }
            row["prefabRenderers"] = authored.Count;
            row["prefabRenderersDisabled"] = new JArray(authored.Where(value => !value.Value).Select(value => value.Key));

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
                area.ForceEnd();
            }
            row["liveWallSteamAreas"] = areaRows;
            DateTime releaseDeadline = DateTime.UtcNow.AddSeconds(12);
            while (DateTime.UtcNow < releaseDeadline && clouds.Any(cloud => cloud != null && !GameObjectsPool.IsInPool(cloud)))
            { Game.Instance.IsPaused = false; yield return 0; }
            row["releasedClouds"] = new JArray(clouds.Select(cloud => new JObject {
                ["destroyed"] = cloud == null, ["inPool"] = cloud != null && GameObjectsPool.IsInPool(cloud),
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
                int compared = 0;
                if (claimed != null)
                    foreach (Renderer renderer in claimed.GetComponentsInChildren<Renderer>(true))
                    {
                        if (renderer == null) continue;
                        string key = InfernalWallRelativePath35(claimed.transform, renderer.transform) + "#" + renderer.GetType().Name;
                        bool expected;
                        if (!authored.TryGetValue(key, out expected)) continue;
                        compared++;
                        if (renderer.enabled != expected)
                            differing.Add(new JObject { ["renderer"] = key, ["enabled"] = renderer.enabled, ["prefab"] = expected });
                    }
                row["claimedRenderersCompared"] = compared;
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
