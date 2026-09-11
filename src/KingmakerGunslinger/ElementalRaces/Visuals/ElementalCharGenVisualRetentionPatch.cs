using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony12;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    [HarmonyPatch(typeof(CharGenDollRoom), "DollStateUpdated", new[] { typeof(DollState) })]
    internal static class ElementalCharGenVisualRetentionPatch
    {
        private static readonly FieldInfo InitialIds = typeof(CharGenDollRoom).GetField(
            "m_InitiallyLoadedEquipmentEntityIds", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo InitialAssets = typeof(CharGenDollRoom).GetField(
            "m_InitiallyLoadedEquipmentEntityInnerAssets", BindingFlags.Instance | BindingFlags.NonPublic);

        private static void Prefix(CharGenDollRoom __instance)
        {
            var visuals = BlueprintBootstrap.ElementalRaces?.Visuals;
            if (__instance == null || visuals == null) return;
            if (InitialIds == null || InitialAssets == null)
                throw new InvalidOperationException("Native character creator visual-retention fields are unavailable.");
            var ids = InitialIds.GetValue(__instance) as HashSet<string>;
            var assets = InitialAssets.GetValue(__instance) as List<UnityEngine.Object>;
            // CreateDolls owns initialization. Its initial snapshot may precede
            // KMG registration; extend that snapshot before native unload planning.
            if (ids == null || assets == null) return;
            // A creator's own removal passes can destroy the shared inner assets
            // of the exact registered proxies (foreign bundle unloads are
            // forceful). Recover the exact resources first, then extend the
            // native retention plan. An unrecoverable elemental state is
            // isolated here so ordinary native character creation still runs.
            try
            {
                visuals.EnsureCreatorResourcesRetained(ids, assets,
                    visuals.Registry.Logger);
            }
            catch (Exception exception)
            {
                ElementalVisualResourceRecovery.ReportIsolatedCreatorFailure(
                    visuals.Registry.Logger, exception);
            }
        }
    }

    [HarmonyPatch(typeof(Kingmaker.Blueprints.ResourcesLibrary), "CleanupLoadedCache")]
    internal static class ElementalVisualResourceCacheCleanupPatch
    {
        // Every native area load runs this cleanup: entries whose request
        // counters decayed to zero are evicted and destroyed, then survivors'
        // counters reset. Re-arm exactly the registered identities and
        // reconstruct anything this pass destroyed, so committed elemental
        // characters keep a live body across area transitions.
        private static void Postfix()
        {
            var visuals = BlueprintBootstrap.ElementalRaces?.Visuals;
            if (visuals == null) return;
            if (Kingmaker.Blueprints.ResourcesLibrary.Preloading) return;
            try
            {
                ElementalVisualResourceRecovery.Heal(visuals,
                    visuals.Registry.Logger, "loaded-cache-cleanup");
                visuals.Registry.ArmRetentionCounters();
            }
            catch (Exception exception)
            {
                if (ElementalVisualResourceRecovery.ReportSuppressed(DateTime.UtcNow))
                    return;
                visuals.Registry.Logger.Warning("elemental-races",
                    "visual-resource.cache-cleanup-isolated",
                    "Loaded-cache cleanup recovery was isolated: " + exception.Message);
            }
        }
    }

    [HarmonyPatch(typeof(CharGenDollRoom), "OnDisable")]
    internal static class ElementalCreatorCloseRecoveryPatch
    {
        // The last doll update's removal pass runs after the final
        // DollStateUpdated, so its damage is invisible to the retention prefix.
        // Heal once as the creator closes so the committed world unit never
        // spawns against destroyed proxies.
        private static void Postfix(CharGenDollRoom __instance)
        {
            if (__instance == null) return;
            var visuals = BlueprintBootstrap.ElementalRaces?.Visuals;
            if (visuals == null) return;
            try
            {
                ElementalVisualResourceRecovery.Heal(visuals,
                    visuals.Registry.Logger, "creator-closed");
            }
            catch (Exception exception)
            {
                if (ElementalVisualResourceRecovery.ReportSuppressed(DateTime.UtcNow))
                    return;
                visuals.Registry.Logger.Warning("elemental-races",
                    "visual-resource.creator-close-isolated",
                    "Creator-close recovery was isolated: " + exception.Message);
            }
        }
    }
}
