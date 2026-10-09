using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Only the two hidden KMG snakes. Research proved this native
    /// binding at .2 view scale; production lifecycle/contacts still require
    /// the exact guarded candidate. Neither native Worm nor Salamander matches.
    /// Reuses the instance-owned swap/rollback, not the other rig families.</summary>
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class ExpandedSummoningSerpentineViewPatch
    {
        private sealed class Attempt { internal string Outcome; }
        private static readonly ConditionalWeakTable<UnitEntityView, Attempt> Applied =
            new ConditionalWeakTable<UnitEntityView, Attempt>();
        private static readonly FieldInfo BaseCorpulence = typeof(UnitEntityView).GetField(
            "m_Corpulence", BindingFlags.Instance | BindingFlags.NonPublic);

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
                context.FeatureModules == null || context.FeatureModules.Active == null) return;
            var unit = __instance.EntityData.Blueprint;
            string key;
            if (!SerpentineVisualPolicy.TryProductionSnake(context.FeatureModules.Active.ExpandedSummoning,
                unit.AssetGuid, unit.name, unit.Prefab == null ? null : unit.Prefab.AssetId, out key)) return;
            lock (Applied)
            {
                Attempt existing;
                if (Applied.TryGetValue(__instance, out existing)) return;
                var attempt = new Attempt { Outcome = "donor-visual:attachment-started" };
                Applied.Add(__instance, attempt);
                try
                {
                    // Native Corpulence ignores transform scale: it uses its
                    // serialized base radius and live rules-size multiplier.
                    // Scale only this exact snake instance's base radius with
                    // its body, once. No getter/global movement/weapon rewrite.
                    Vector3 scale = __instance.transform.localScale;
                    if (!PositiveFinite(scale.x) || !PositiveFinite(scale.y) || !PositiveFinite(scale.z))
                        throw new InvalidOperationException("Invalid original snake view scale.");
                    if (BaseCorpulence == null || BaseCorpulence.FieldType != typeof(float))
                        throw new MissingFieldException("UnitEntityView.m_Corpulence");
                    float radius = SerpentineVisualPolicy.ScaleSnakeBaseCorpulence(
                        (float)BaseCorpulence.GetValue(__instance));
                    BaseCorpulence.SetValue(__instance, radius);
                    __instance.transform.localScale = scale * SerpentineVisualPolicy.SnakeViewMultiplier;
                    string outcome;
                    SerpentineVisualAttachment.TryAttach(__instance, key, context, out outcome);
                    attempt.Outcome = outcome;
                }
                catch (Exception error)
                {
                    // TryAttach owns rollback and cleanup; never retry or
                    // multiply the scale again on a second native callback.
                    attempt.Outcome = "donor-visual:attachment-exception:" + error.GetType().Name + ":" + error.Message;
                    Debug.LogException(error);
                }
            }
        }

        private static bool PositiveFinite(float value)
        { return value > 0 && !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    // The native special-attack selector consumes world distance without
    // view-scale compensation. Normalize only an exact owned snake's Bite
    // getter result to its donor clip space; never mutate the stored handle,
    // shared action/ranges/clip, rig, command, weapon or actor position.
    [HarmonyPatch(typeof(UnitAnimationActionHandle), "get_AttackTargetDistance")]
    internal static class SerpentineBiteAnimationDistancePatch
    {
        private static void Postfix(UnitAnimationActionHandle __instance, ref float __result)
        {
            var action = __instance == null ? null : __instance.Action as UnitAnimationActionSpecialAttack;
            if (action == null || action.AttackType != UnitAnimationSpecialAttackType.Bite ||
                __instance.Manager == null) return;
            var view = __instance.Manager.GetComponentInParent<UnitEntityView>();
            if (view == null || view.EntityData == null || view.EntityData.Blueprint == null) return;
            ModContext context;
            if (!ModContext.TryGet(out context) || context.FeatureModules == null ||
                context.FeatureModules.Active == null) return;
            var unit = view.EntityData.Blueprint;
            var attachment = view.GetComponent<SerpentineVisualAttachment>();
            float corrected;
            if (SerpentineVisualPolicy.TrySnakeBiteAnimationDistance(
                context.FeatureModules.Active.ExpandedSummoning, unit.AssetGuid, unit.name,
                unit.Prefab == null ? null : unit.Prefab.AssetId,
                attachment != null && attachment.OriginalBodyLive, true, __result, out corrected) &&
                ReferenceEquals(view.AnimationManager, __instance.Manager) &&
                ReferenceEquals(view.AnimationManager.GetAction(UnitAnimationSpecialAttackType.Bite), action) &&
                SerpentineVisualPolicy.IsNativeSnakeBiteAction(action.name, action.Clips == null ? null :
                    action.Clips.Select(clip => clip == null ? null : clip.name)))
                __result = corrected;
        }
    }
}
