using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints.Root;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class ExpandedSummoningSalamanderViewPatch
    {
        internal sealed class Attempt { internal string Outcome = "human-tail:waiting-for-native-settlement"; }
        private static readonly ConditionalWeakTable<UnitEntityView, Attempt> Attempts =
            new ConditionalWeakTable<UnitEntityView, Attempt>();

        internal static string DescribeView(UnitEntityView view)
        {
            Attempt attempt;
            return view != null && Attempts.TryGetValue(view, out attempt) ? attempt.Outcome : "not-attempted";
        }

        internal static bool Permits(UnitEntityView view, ModContext context)
        {
            var unit = view == null ? null : view.EntityData;
            var blueprint = unit == null ? null : unit.Blueprint;
            var spear = blueprint == null || blueprint.Body == null ? null :
                blueprint.Body.PrimaryHand as Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon;
            return blueprint != null && context != null && context.FeatureModules != null && context.FeatureModules.Active != null &&
                SalamanderProductionViewPolicy.Permits(context.FeatureModules.Active.ExpandedSummoning,
                    blueprint.AssetGuid, blueprint.name, blueprint.Prefab == null ? null : blueprint.Prefab.AssetId,
                    spear == null ? null : spear.AssetGuid, unit.Body != null && unit.Body.IsPolymorphed);
        }

        internal static void Postfix(UnitEntityView __instance)
        {
            ModContext context;
            if (!ModContext.TryGet(out context) || !Permits(__instance, context)) return;
            lock (Attempts)
            {
                Attempt previous;
                if (Attempts.TryGetValue(__instance, out previous)) return;
                var attempt = new Attempt();
                Attempts.Add(__instance, attempt);
                try
                {
                    __instance.gameObject.AddComponent<SalamanderProductionViewBinding>().Initialize(__instance, context, attempt);
                }
                catch (Exception error)
                {
                    attempt.Outcome = "human-tail:binding-start-failed:" + error.GetType().Name;
                    Debug.LogException(error);
                }
            }
        }
    }

    // No native visibility, appearance lock, rig, outfit or Animator writes.
    // Only a ready exact human view can enter the previously proved swap.
    internal sealed class SalamanderProductionViewBinding : MonoBehaviour
    {
        private UnitEntityView _view;
        private ModContext _context;
        private ExpandedSummoningSalamanderViewPatch.Attempt _attempt;
        private readonly SalamanderViewSettlement _settlement = new SalamanderViewSettlement();

        internal void Initialize(UnitEntityView view, ModContext context, ExpandedSummoningSalamanderViewPatch.Attempt attempt)
        { _view = view; _context = context; _attempt = attempt; }

        private void LateUpdate()
        {
            if (_attempt == null) return;
            try
            {
                if (!ExpandedSummoningSalamanderViewPatch.Permits(_view, _context) || _view.EntityData.Destroyed)
                { Finish("human-tail:owner-no-longer-eligible"); return; }
                var skins = _view.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(s => s.sharedMesh != null).ToArray();
                var body = skins.SingleOrDefault(s => s.name == SalamanderHumanBindingPolicy.BodyName);
                var unitBody = _view.EntityData.Body;
                var spear = unitBody == null || unitBody.PrimaryHand == null ? null : unitBody.PrimaryHand.MaybeWeapon;
                bool ready = spear != null && spear.Blueprint.AssetGuid == SalamanderTailAnimationPolicy.Spear &&
                    body != null && body.rootBone != null && body.rootBone.name == "Pelvis" &&
                    body.sharedMesh.vertexCount == 2268 && body.bones.Length == 1776 && body.sharedMesh.bindposes.Length == 1776 &&
                    body.bones.All(b => b != null) && skins.Length == 2 && skins.Where(s => s != body).All(s =>
                        SalamanderHumanBindingPolicy.IsReviewedAuxiliary(s.name, s.sharedMesh.name, s.bones.Length)) &&
                    body.sharedMaterial != null && body.sharedMaterial.HasProperty("_MainTex") &&
                    _view.AnimationManager != null && ReferenceEquals(_view.AnimationManager.AnimationSet, BlueprintRoot.Instance.HumanAnimationSet);
                var decision = _settlement.Observe(Time.frameCount, body == null ? null : body.sharedMesh, ready);
                if (decision == SalamanderViewSettlementResult.Waiting) return;
                if (decision != SalamanderViewSettlementResult.Ready)
                { Finish("human-tail:native-settlement-expired;donor-fallback"); return; }
                string outcome;
                SalamanderHumanVisualAttachment.TryAttach(_view, _context, out outcome);
                Finish(outcome);
            }
            catch (Exception error)
            {
                Finish("human-tail:binding-exception:" + error.GetType().Name + ";donor-fallback");
                Debug.LogException(error);
            }
        }

        private void Finish(string outcome)
        {
            _attempt.Outcome = outcome;
            enabled = false; // Our observer only; never a native renderer/controller.
            Destroy(this);
        }

        private void OnDestroy()
        {
            if (_attempt != null && _attempt.Outcome == "human-tail:waiting-for-native-settlement")
                _attempt.Outcome = "human-tail:view-destroyed-before-settlement";
        }
    }
}
