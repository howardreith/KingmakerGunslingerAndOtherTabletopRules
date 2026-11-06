using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Wall of Ice steam uses the native Obscuring Mist area for its
    /// mechanics. That area's own effect link resolves to no prefab in the
    /// installed game, so the native view spawns nothing and the steam is
    /// invisible. For the Wall's own steam only, after the native spawn
    /// attempt (on creation and after load), the view adopts the native
    /// 20-foot kineticist steam cloud as its spawned effect, so native
    /// LateUpdate follows the area and native OnDestroy removes it. The
    /// cloud's burning-ground and glow renderers are switched off: Wall steam
    /// deals no fire, so it must not look like it burns. The cloud is a
    /// pooled native effect: installed FxHelper claims it from
    /// GameObjectsPool, native area teardown releases it back, and installed
    /// PooledFx resets particle, sorting, snap and component state on
    /// release but not Renderer.enabled. The renderers switched off here are
    /// recorded for this owned instance and switched back on at the native
    /// release boundary, so a later native consumer of the same prefab gets
    /// the prefab's own flame layers.
    /// </summary>
    [HarmonyPatch(typeof(AreaEffectView), "SpawnFxs")]
    internal static class InfernalWallSteamPresentation35
    {
        private static readonly FieldInfo SpawnedFx = typeof(AreaEffectView)
            .GetField("m_SpawnedFx", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PrefabLink Steam = new PrefabLink {
            AssetId = InfernalRules35.WallSteamFxAssetId };
        // Owned cloud instance id -> the renderers this patch switched off
        // (each was enabled before). Cleared at release.
        private static readonly Dictionary<int, List<Renderer>> Owned =
            new Dictionary<int, List<Renderer>>();

        internal static int OwnedCloudCount { get { return Owned.Count; } }

        internal static void RestoreOwned(GameObject instance)
        {
            if (instance == null) return;
            List<Renderer> switchedOff;
            int id = instance.GetInstanceID();
            if (!Owned.TryGetValue(id, out switchedOff)) return;
            Owned.Remove(id);
            foreach (Renderer layer in switchedOff)
                if (layer != null) layer.enabled = true;
        }

        private static void Postfix(AreaEffectView __instance)
        {
            try
            {
                if (__instance == null || SpawnedFx == null) return;
                var existing = SpawnedFx.GetValue(__instance) as GameObject;
                var data = __instance.Data as AreaEffectEntityData;
                var context = data?.Context ?? __instance.Context;
                if (!InfernalRules35.WallSteamNeedsPresentation(existing != null,
                        data?.Blueprint?.AssetGuid, context?.SourceAbility?.name))
                    return;
                GameObject prefab = Steam.Load(true);
                if (prefab == null) return;
                GameObject cloud = FxHelper.SpawnFxOnGameObject(prefab, __instance.gameObject);
                // Only the flame layers' own renderers are switched off. Their
                // GameObjects stay active: the vapour systems are nested under
                // them, and deactivating a flame object stopped the whole
                // cloud natively (0 playing systems, 0 particles).
                if (cloud != null)
                {
                    // Instances destroyed outside the pool leave no renderers.
                    foreach (int stale in Owned.Where(entry =>
                            entry.Value.All(layer => layer == null))
                            .Select(entry => entry.Key).ToArray())
                        Owned.Remove(stale);
                    var switchedOff = new List<Renderer>();
                    foreach (Renderer layer in cloud.GetComponentsInChildren<Renderer>(true))
                        if (layer != null && layer.enabled &&
                                InfernalRules35.WallSteamHidesLayer(layer.name))
                        {
                            layer.enabled = false;
                            switchedOff.Add(layer);
                        }
                    if (switchedOff.Count > 0)
                        Owned[cloud.GetInstanceID()] = switchedOff;
                }
                SpawnedFx.SetValue(__instance, cloud);
            }
            catch (Exception)
            {
                // Presentation only: the native area and its mechanics are
                // unaffected if the effect cannot be attached.
            }
        }
    }

    /// <summary>
    /// The native release boundary for every pooled effect, also reached
    /// after a native fade-out. A Wall steam cloud gets back the flame
    /// renderers it had before it was claimed for the Wall.
    /// </summary>
    [HarmonyPatch(typeof(GameObjectsPool), "Release")]
    internal static class InfernalWallSteamPoolReleasePatch
    {
        private static void Prefix(GameObject instance)
        {
            try
            {
                InfernalWallSteamPresentation35.RestoreOwned(instance);
            }
            catch (Exception)
            {
                // Never interfere with the native release.
            }
        }
    }
}
