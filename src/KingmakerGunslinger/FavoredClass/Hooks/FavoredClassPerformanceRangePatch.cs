using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using KingmakerGunslinger.FavoredClass.Mechanics;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// O01 read point: after the native view of a manifest performance area
    /// creates its per-instance cylinder (on spawn and again when a save is
    /// loaded), widens that one instance by the casting bard's own earned
    /// steps for that performance. The shared area blueprint, other
    /// performers of the same area, the effect buffs and the visual ring are
    /// never changed.
    /// </summary>
    [HarmonyPatch(typeof(AreaEffectView), "InitAtRuntime")]
    internal static class FavoredClassPerformanceRangePatch
    {
        private static void Postfix(AreaEffectView __instance, MechanicsContext context,
            BlueprintAbilityAreaEffect blueprint)
        {
            if (__instance == null || context == null || blueprint == null ||
                blueprint.Shape != AreaEffectShape.Cylinder)
                return;
            string key = FavoredClassPerformanceManifest.KeyForArea(blueprint.AssetGuid);
            UnitEntityData caster = key == null ? null : context.MaybeCaster;
            if (caster == null)
                return;
            int steps = FavoredClassEarnedSteps.For(caster.Descriptor, FavoredClassCatalog.EffectPerformanceRange, key);
            var cylinder = __instance.Shape as ScriptZoneCylinder;
            if (steps <= 0 || cylinder == null)
                return;
            cylinder.Radius = FavoredClassMechanicsPolicy.PerformanceRadiusMeters(blueprint.Size.Meters, steps);
        }
    }
}
