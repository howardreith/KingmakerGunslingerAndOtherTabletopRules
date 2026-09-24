using System;
using System.Reflection;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using KingmakerGunslinger.FavoredClass.Mechanics;
using UnityEngine;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// O01 read point: after the native view of a published performance area
    /// creates its per-instance cylinder and spawns its ring effect (on spawn
    /// and again when a save is loaded), widens that one instance's cylinder
    /// to the casting bard's own range and scales that one spawned ring by
    /// the same ratio, so the actual range and the visible boundary agree.
    /// The shared area blueprint, other performers of the same area and every
    /// other performance keep their native size.
    /// </summary>
    [HarmonyPatch(typeof(AreaEffectView), "InitAtRuntime")]
    internal static class FavoredClassPerformanceRangePatch
    {
        private static readonly FieldInfo SpawnedFx = typeof(AreaEffectView).GetField("m_SpawnedFx",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static void Postfix(AreaEffectView __instance, MechanicsContext context,
            BlueprintAbilityAreaEffect blueprint)
        {
            if (__instance == null || context == null || blueprint == null ||
                blueprint.Shape != AreaEffectShape.Cylinder)
                return;
            try
            {
                string key = FavoredClassPerformanceManifest.KeyForArea(blueprint.AssetGuid);
                UnitEntityData caster = key == null ? null : context.MaybeCaster;
                if (caster == null)
                    return;
                int steps = FavoredClassEarnedSteps.For(caster.Descriptor, FavoredClassCatalog.EffectPerformanceRange,
                    key);
                var cylinder = __instance.Shape as ScriptZoneCylinder;
                if (steps <= 0 || cylinder == null)
                    return;
                float native = blueprint.Size.Meters;
                float widened = FavoredClassMechanicsPolicy.PerformanceRadiusMeters(native, steps);
                cylinder.Radius = widened;
                var ring = SpawnedFx == null ? null : SpawnedFx.GetValue(__instance) as GameObject;
                if (ring != null && native > 0f)
                    FavoredClassPerformanceRing.Scale(ring, widened / native);
            }
            catch (Exception)
            {
                // Fail safe: the area keeps its native radius and is never broken.
            }
        }
    }

    /// <summary>
    /// Pooled effects are reused without a reset: a ring scaled for one
    /// owner is restored exactly before it returns to the pool.
    /// </summary>
    [HarmonyPatch(typeof(Kingmaker.Visual.Particles.GameObjectsPool), "Release")]
    internal static class FavoredClassPerformanceRingReleasePatch
    {
        private static void Prefix(GameObject instance)
        {
            try
            {
                FavoredClassPerformanceRing.Restore(instance);
            }
            catch (Exception)
            {
                // Never interfere with the native release.
            }
        }
    }
}
