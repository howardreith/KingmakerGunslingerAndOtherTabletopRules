using System;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Attaches the original ape bodies, and only to the two hidden KMG apes.
    ///
    /// <para>The gate is immutable identity, blueprint name AND donor prefab
    /// together, so no native troll and no other borrowed-rig creature can be
    /// reached by it. Both apes are Large and so is the donor, so unlike the
    /// Sprint 17 snakes nothing here rescales the view or the collision
    /// footprint: the geometry is authored at the size the rig already
    /// stands.</para>
    /// </summary>
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class ExpandedSummoningPrimateViewPatch
    {
        private sealed class Attempt { internal string Outcome; }
        private static readonly ConditionalWeakTable<UnitEntityView, Attempt> Applied =
            new ConditionalWeakTable<UnitEntityView, Attempt>();

        internal static string DescribeView(UnitEntityView view)
        {
            Attempt attempt;
            return view != null && Applied.TryGetValue(view, out attempt)
                ? attempt.Outcome : "not-attempted";
        }

        internal static void Postfix(UnitEntityView __instance)
        {
            ModContext context;
            if (__instance == null || __instance.EntityData == null ||
                __instance.EntityData.Blueprint == null || !ModContext.TryGet(out context) ||
                context.FeatureModules == null || context.FeatureModules.Active == null)
                return;
            var unit = __instance.EntityData.Blueprint;
            string key;
            if (!PrimateVisualPolicy.TryProductionPrimate(
                    context.FeatureModules.Active.ExpandedSummoning, unit.AssetGuid,
                    unit.name, unit.Prefab == null ? null : unit.Prefab.AssetId, out key))
                return;
            lock (Applied)
            {
                Attempt existing;
                if (Applied.TryGetValue(__instance, out existing)) return;
                var attempt = new Attempt { Outcome = "donor-visual:attachment-started" };
                Applied.Add(__instance, attempt);
                try
                {
                    string outcome;
                    PrimateVisualAttachment.TryAttach(__instance, key, context, out outcome);
                    attempt.Outcome = outcome;
                }
                catch (Exception error)
                {
                    // TryAttach owns rollback and cleanup; never retry on a
                    // second native callback for the same view.
                    attempt.Outcome = "donor-visual:attachment-exception:" +
                        error.GetType().Name + ":" + error.Message;
                    Debug.LogException(error);
                }
            }
        }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class PrimateVisualTeardownPatch
    {
        private static void Prefix(UnitEntityView __instance)
        {
            var owned = __instance == null ? null :
                __instance.GetComponent<PrimateVisualAttachment>();
            if (owned == null) return;
            try { owned.Release(); }
            catch (Exception) { /* Never interrupt native destruction. */ }
        }
    }
}
