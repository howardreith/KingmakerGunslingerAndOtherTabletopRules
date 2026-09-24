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
                var cylinder = __instance.Shape as ScriptZoneCylinder;
                float native, widened;
                if (cylinder == null || !OwnerRadius(context, blueprint, out native, out widened))
                    return;
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

        /// <summary>The casting bard's own radius for a published performance area with earned steps.</summary>
        internal static bool OwnerRadius(MechanicsContext context, BlueprintAbilityAreaEffect blueprint,
            out float native, out float widened)
        {
            native = widened = 0f;
            string key = FavoredClassPerformanceManifest.KeyForArea(blueprint.AssetGuid);
            UnitEntityData caster = key == null ? null : context.MaybeCaster;
            if (caster == null)
                return false;
            int steps = FavoredClassEarnedSteps.For(caster.Descriptor, FavoredClassCatalog.EffectPerformanceRange,
                key);
            if (steps <= 0)
                return false;
            native = blueprint.Size.Meters;
            widened = FavoredClassMechanicsPolicy.PerformanceRadiusMeters(native, steps);
            return true;
        }

        /// <summary>
        /// A ring spawned after the initialization (a save load whose owner
        /// view was not ready yet) receives the scale of the cylinder the
        /// initialization widened for this owner, exactly once.
        /// </summary>
        internal static void ScaleLateRing(AreaEffectView view)
        {
            var data = view == null ? null : view.Data as AreaEffectEntityData;
            BlueprintAbilityAreaEffect blueprint = data == null ? null : data.Blueprint;
            var cylinder = view == null ? null : view.Shape as ScriptZoneCylinder;
            var ring = view == null || SpawnedFx == null ? null : SpawnedFx.GetValue(view) as GameObject;
            if (blueprint == null || cylinder == null || ring == null || view.Context == null ||
                blueprint.Shape != AreaEffectShape.Cylinder || FavoredClassPerformanceRing.IsScaled(ring))
                return;
            float native, widened;
            if (OwnerRadius(view.Context, blueprint, out native, out widened) && native > 0f &&
                Math.Abs(cylinder.Radius - widened) < 0.0001f)
                FavoredClassPerformanceRing.Scale(ring, widened / native);
        }
    }

    /// <summary>
    /// On a save load the native attach spawns a ring the initialization
    /// could not (its owner's view was not ready yet); that late ring takes
    /// its owner's scale. A ring spawned during the initialization is
    /// scaled by the initialization postfix, never twice.
    /// </summary>
    [HarmonyPatch(typeof(AreaEffectView), "SpawnFxs")]
    internal static class FavoredClassPerformanceLateRingPatch
    {
        private static void Postfix(AreaEffectView __instance)
        {
            try
            {
                FavoredClassPerformanceRangePatch.ScaleLateRing(__instance);
            }
            catch (Exception)
            {
                // Fail safe: the ring keeps its native size; the area is unaffected.
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
