using System;
using Harmony12;
using Kingmaker.Controllers.Projectiles;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    /// <summary>Hides only the view renderers of the stable cloned firearm projectile.</summary>
    [HarmonyPatch(typeof(Projectile), "BeforeLaunch")]
    internal static class FirearmProjectileVisualPatch
    {
        private static void Postfix(Projectile __instance)
        {
            if (__instance == null || __instance.View == null ||
                !ReferenceEquals(__instance.Blueprint,
                    FirearmProjectileBlueprints.Projectile)) return;
            foreach (Renderer renderer in
                __instance.View.GetComponentsInChildren<Renderer>(true))
                renderer.enabled = false;
        }
    }
}
