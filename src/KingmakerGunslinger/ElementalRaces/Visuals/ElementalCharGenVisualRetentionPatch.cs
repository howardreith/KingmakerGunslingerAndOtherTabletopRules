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
            visuals.RetainCharacterCreatorResources(ids, assets);
        }
    }
}
