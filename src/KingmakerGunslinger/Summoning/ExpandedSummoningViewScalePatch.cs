using System;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class ExpandedSummoningViewScalePatch
    {
        private sealed class AppliedMarker { }
        private static readonly ConditionalWeakTable<UnitEntityView,
            AppliedMarker> Applied = new ConditionalWeakTable<UnitEntityView,
                AppliedMarker>();

        private static void Postfix(UnitEntityView __instance)
        {
            if (__instance == null || __instance.EntityData == null ||
                __instance.EntityData.Blueprint == null) return;
            float multiplier;
            if (!SummonViewScaleCatalog.TryGetMultiplier(
                    __instance.EntityData.Blueprint.name, out multiplier)) return;
            lock (Applied)
            {
                AppliedMarker ignored;
                if (Applied.TryGetValue(__instance, out ignored)) return;
                Vector3 current = __instance.transform.localScale;
                if (!Finite(current) || current.x <= 0f || current.y <= 0f ||
                    current.z <= 0f) throw new InvalidOperationException(
                        "KMG summon view has an invalid pre-scale transform: " +
                        __instance.EntityData.Blueprint.name + ".");
                var spec = System.Linq.Enumerable.Single(
                    SummonViewScaleCatalog.All, value => value.BlueprintName ==
                        __instance.EntityData.Blueprint.name);
                if (spec.RetainNativeScale)
                {
                    // Exact 2.1.7b UnitEntityView.Update writes localScale =
                    // m_OriginalScale * m_Scale every frame. A one-time root
                    // transform edit is erased. Change only this live view's
                    // native baseline, once; prefab/blueprint size stay intact.
                    var baseline = AccessTools.Field(typeof(UnitEntityView), "m_OriginalScale");
                    if (baseline == null || baseline.FieldType != typeof(Vector3))
                        throw new InvalidOperationException("Native view-scale baseline is unavailable.");
                    Vector3 original = __instance.OriginalScale;
                    if (!Finite(original) || original.x <= 0 || original.y <= 0 || original.z <= 0)
                        throw new InvalidOperationException("Native view-scale baseline is invalid.");
                    baseline.SetValue(__instance, original * multiplier);
                }
                __instance.transform.localScale = current * multiplier;
                Applied.Add(__instance, new AppliedMarker());
            }
        }

        private static bool Finite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }
    }
}
