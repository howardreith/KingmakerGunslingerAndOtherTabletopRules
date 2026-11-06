using System;
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
    /// LateUpdate follows the area and native OnDestroy removes it.
    /// </summary>
    [HarmonyPatch(typeof(AreaEffectView), "SpawnFxs")]
    internal static class InfernalWallSteamPresentation35
    {
        private static readonly FieldInfo SpawnedFx = typeof(AreaEffectView)
            .GetField("m_SpawnedFx", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PrefabLink Steam = new PrefabLink {
            AssetId = InfernalRules35.WallSteamFxAssetId };

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
                SpawnedFx.SetValue(__instance,
                    FxHelper.SpawnFxOnGameObject(prefab, __instance.gameObject));
            }
            catch (Exception)
            {
                // Presentation only: the native area and its mechanics are
                // unaffected if the effect cannot be attached.
            }
        }
    }
}
