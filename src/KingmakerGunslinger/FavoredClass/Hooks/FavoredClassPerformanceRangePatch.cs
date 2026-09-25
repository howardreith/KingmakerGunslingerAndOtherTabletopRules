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
    /// to the casting bard's own range together with that one spawned ring,
    /// in one transaction (FavoredClassPerformanceWidening): the ring is
    /// scaled first and the cylinder is widened only when the ring changed; a
    /// failure restores both. An intentionally ringless target widens its
    /// cylinder alone, and a ring that spawns later widens both then. An
    /// owner's performance widens only as a whole: while another live area of
    /// it is native, a new or late-ring area stays native (Held). Every
    /// outcome is recorded for that owner and performance
    /// (FavoredClassPerformanceInstances), which the owner's descriptions
    /// follow. The shared area blueprint, other performers of the same area
    /// and every other performance keep their native size.
    /// </summary>
    [HarmonyPatch(typeof(AreaEffectView), "InitAtRuntime")]
    internal static class FavoredClassPerformanceRangePatch
    {
        private static readonly FieldInfo SpawnedFx = typeof(AreaEffectView).GetField("m_SpawnedFx",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Guarded runtime qualification only: replaces the ring scaler so a
        /// failure can be injected into the real transaction; null in play.
        /// </summary>
        internal static Func<GameObject, float, int> RingScalerOverride;

        /// <summary>
        /// Guarded runtime qualification only: every ring counts as not spawned
        /// yet, so the deferred path (a save load whose owner view was not
        /// ready) can be observed on a real instance; false in play.
        /// </summary>
        internal static bool DeferRingsForQualification;

        private static void Postfix(AreaEffectView __instance, MechanicsContext context,
            BlueprintAbilityAreaEffect blueprint)
        {
            if (__instance == null || context == null || blueprint == null ||
                blueprint.Shape != AreaEffectShape.Cylinder)
                return;
            var cylinder = __instance.Shape as ScriptZoneCylinder;
            float native, widened;
            int feet;
            try
            {
                if (cylinder == null || !OwnerRadius(context, blueprint, out native, out widened, out feet))
                    return;
            }
            catch (Exception)
            {
                return;
            }
            var ring = DeferRingsForQualification || SpawnedFx == null ? null :
                SpawnedFx.GetValue(__instance) as GameObject;
            // A ring the native attach already handled for this owner is never scaled twice.
            if (ring != null && FavoredClassPerformanceRing.IsScaled(ring))
                return;
            FavoredClassWideningOutcome outcome = MayWiden(__instance, context, blueprint)
                ? Widen(cylinder, ring, native, widened, RingExpected(blueprint))
                : FavoredClassWideningOutcome.Held;
            Record(__instance, context, blueprint, outcome, feet, native);
        }

        /// <summary>
        /// Whether this instance may attempt widening: every other live area of
        /// the same owner's performance is widened (a failure here keeps it native).
        /// </summary>
        private static bool MayWiden(AreaEffectView view, MechanicsContext context, BlueprintAbilityAreaEffect blueprint)
        {
            try
            {
                return FavoredClassPerformanceInstances.MayWiden(view, context.MaybeCaster,
                    FavoredClassPerformanceManifest.KeyForArea(blueprint.AssetGuid));
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Records the instance's outcome for its owner and target (the owner's descriptions follow it).</summary>
        private static void Record(AreaEffectView view, MechanicsContext context, BlueprintAbilityAreaEffect blueprint,
            FavoredClassWideningOutcome outcome, int feet, float native)
        {
            try
            {
                FavoredClassPerformanceInstances.Record(view, context.MaybeCaster,
                    FavoredClassPerformanceManifest.KeyForArea(blueprint.AssetGuid), outcome, feet, native);
            }
            catch (Exception)
            {
                // The owner's descriptions then keep the configured range.
            }
        }

        /// <summary>Whether the area's published target spawns a ring (Scandal's link spawns none).</summary>
        internal static bool RingExpected(BlueprintAbilityAreaEffect blueprint)
        {
            string key = blueprint == null ? null : FavoredClassPerformanceManifest.KeyForArea(blueprint.AssetGuid);
            return key == null || FavoredClassPerformanceManifest.For(key).RingSpawns;
        }

        /// <summary>The one widening transaction for one area instance and its ring.</summary>
        internal static FavoredClassWideningOutcome Widen(ScriptZoneCylinder cylinder, GameObject ring, float native,
            float widened, bool ringExpected)
        {
            if (cylinder == null || native <= 0f)
                return FavoredClassWideningOutcome.Failed;
            try
            {
                return FavoredClassPerformanceWidening.Apply(native, widened, ringExpected, ring != null,
                    radius => cylinder.Radius = radius,
                    () => ScaleRing(ring, widened / native),
                    () => FavoredClassPerformanceRing.Restore(ring));
            }
            catch (Exception)
            {
                // Fail safe: native radius and ring.
                try { FavoredClassPerformanceRing.Restore(ring); } catch (Exception) { }
                try { cylinder.Radius = native; } catch (Exception) { }
                return FavoredClassWideningOutcome.Failed;
            }
        }

        private static int ScaleRing(GameObject ring, float factor)
        {
            Func<GameObject, float, int> scaler = RingScalerOverride;
            return scaler != null ? scaler(ring, factor) : FavoredClassPerformanceRing.Scale(ring, factor);
        }

        /// <summary>
        /// The casting bard's own radius (and range in feet) for a published
        /// performance area with earned steps; a target withheld in this
        /// process stays native.
        /// </summary>
        internal static bool OwnerRadius(MechanicsContext context, BlueprintAbilityAreaEffect blueprint,
            out float native, out float widened, out int feet)
        {
            native = widened = 0f;
            feet = 0;
            string key = FavoredClassPerformanceManifest.KeyForArea(blueprint.AssetGuid);
            UnitEntityData caster = key == null ? null : context.MaybeCaster;
            if (caster == null ||
                FavoredClassRuntime.IsEffectUnavailable(FavoredClassCatalog.EffectPerformanceRange) ||
                FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectPerformanceRange, key))
                return false;
            int steps = FavoredClassEarnedSteps.For(caster.Descriptor, FavoredClassCatalog.EffectPerformanceRange,
                key);
            if (steps <= 0)
                return false;
            native = blueprint.Size.Meters;
            widened = FavoredClassMechanicsPolicy.PerformanceRadiusMeters(native, steps);
            feet = FavoredClassPerformanceManifest.OwnerFeet(FavoredClassPerformanceManifest.For(key), steps);
            return true;
        }

        /// <summary>
        /// A ring spawned after the initialization (a save load whose owner
        /// view was not ready yet) arrives while the instance is still at its
        /// native radius (the initialization deferred the widening): the ring
        /// and the cylinder are widened together now, exactly once, or both
        /// stay native on failure.
        /// </summary>
        internal static void ScaleLateRing(AreaEffectView view)
        {
            if (DeferRingsForQualification)
                return;
            var data = view == null ? null : view.Data as AreaEffectEntityData;
            BlueprintAbilityAreaEffect blueprint = data == null ? null : data.Blueprint;
            var cylinder = view == null ? null : view.Shape as ScriptZoneCylinder;
            var ring = view == null || SpawnedFx == null ? null : SpawnedFx.GetValue(view) as GameObject;
            if (blueprint == null || cylinder == null || ring == null || view.Context == null ||
                blueprint.Shape != AreaEffectShape.Cylinder || FavoredClassPerformanceRing.IsScaled(ring))
                return;
            float native, widened;
            int feet;
            if (OwnerRadius(view.Context, blueprint, out native, out widened, out feet) && native > 0f &&
                Math.Abs(cylinder.Radius - native) < 0.0001f)
                Record(view, view.Context, blueprint, MayWiden(view, view.Context, blueprint)
                    ? Widen(cylinder, ring, native, widened, true)
                    : FavoredClassWideningOutcome.Held, feet, native);
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
