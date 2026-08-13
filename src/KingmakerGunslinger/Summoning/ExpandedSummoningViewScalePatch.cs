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
