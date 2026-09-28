using System;
using System.Globalization;
using System.Linq;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// A visual-only Tail pose on one attached Wasp. The native attack, unit
    /// position, selection and collision remain authoritative. Every frame
    /// starts from the animator's current pose, and teardown restores only
    /// offsets that still belong to this component.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    internal sealed class GiantWaspVisualSting : MonoBehaviour
    {
        private const float MaximumApproachMeters = 0.25f;
        private UnitEntityView _view;
        private SkinnedMeshRenderer _renderer;
        private Transform _tail;
        private Transform _root;
        private UnitEntityData _target;
        private Vector3 _tipLocal;
        private int _tipIndex = -1;
        private Mesh _baked;
        private float _startedAt = -1f;
        private float _impactAt = -1f;
        private Quaternion _nativeTail;
        private Quaternion _appliedTail;
        private Vector3 _nativeRoot;
        private Vector3 _appliedRoot;
        private bool _poseApplied;
        private float _gapBefore = -1f;
        private float _gapAfter = -1f;
        private float _bakedGapAfter = -1f;
        private int _begins;
        private int _impacts;

        internal void Configure(UnitEntityView view, SkinnedMeshRenderer renderer)
        {
            if (view == null || renderer == null || renderer.sharedMesh == null ||
                renderer.sharedMesh.name != ExpandedSummoningPteranodonViewPatch
                    .GiantWaspVisualName || renderer.rootBone == null)
                throw new InvalidOperationException(
                    "Wasp sting pose requires one attached original renderer.");
            Transform[] bones = renderer.bones;
            int tailIndex = Array.FindIndex(bones, value => value != null &&
                value.name == "Tail");
            BoneWeight[] weights = renderer.sharedMesh.boneWeights;
            if (tailIndex < 0 || weights == null || weights.Length == 0)
                throw new InvalidOperationException(
                    "Wasp sting pose requires a weighted Tail bone.");
            Mesh baked = new Mesh();
            baked.name = "KMG_GiantWaspStingProbe";
            try
            {
                renderer.BakeMesh(baked);
                Vector3[] vertices = baked.vertices;
                if (vertices == null || vertices.Length != weights.Length)
                    throw new InvalidOperationException(
                        "Wasp sting pose has no matching baked vertices.");
                Transform tail = bones[tailIndex];
                int tipIndex = -1;
                float longest = -1f;
                for (int index = 0; index < vertices.Length; index++)
                {
                    BoneWeight weight = weights[index];
                    bool tailOwned =
                        weight.boneIndex0 == tailIndex && weight.weight0 >= 0.75f ||
                        weight.boneIndex1 == tailIndex && weight.weight1 >= 0.75f ||
                        weight.boneIndex2 == tailIndex && weight.weight2 >= 0.75f ||
                        weight.boneIndex3 == tailIndex && weight.weight3 >= 0.75f;
                    if (!tailOwned) continue;
                    Vector3 world = renderer.transform.TransformPoint(vertices[index]);
                    float length = (world - tail.position).sqrMagnitude;
                    if (length <= longest) continue;
                    longest = length;
                    tipIndex = index;
                }
                if (tipIndex < 0)
                    throw new InvalidOperationException(
                        "Wasp sting pose found no Tail-owned stinger tip.");
                _tipLocal = tail.InverseTransformPoint(renderer.transform
                    .TransformPoint(vertices[tipIndex]));
                _tipIndex = tipIndex;
                _baked = baked;
                baked = null;
                _tail = tail;
                _root = renderer.rootBone;
                _view = view;
                _renderer = renderer;
            }
            finally
            {
                if (baked != null) UnityEngine.Object.Destroy(baked);
            }
        }

        internal static GiantWaspVisualSting For(UnitAttack command)
        {
            return For(command == null ? null : command.Executor);
        }

        internal static GiantWaspVisualSting For(UnitEntityData owner)
        {
            if (owner == null || owner.Blueprint == null || owner.View == null ||
                owner.Blueprint.name != ExpandedSummoningPteranodonViewPatch
                    .GiantWaspBlueprintName) return null;
            GiantWaspVisualSting pose = owner.View
                .GetComponent<GiantWaspVisualSting>();
            return pose != null && pose.isActiveAndEnabled &&
                pose._renderer != null && pose._renderer.sharedMesh != null &&
                pose._renderer.sharedMesh.name ==
                    ExpandedSummoningPteranodonViewPatch.GiantWaspVisualName
                ? pose : null;
        }

        internal void Begin(UnitEntityData target)
        {
            if (_view == null || _tail == null || target == null ||
                target.View == null) return;
            _target = target;
            _startedAt = Time.unscaledTime;
            _impactAt = -1f;
            _begins++;
        }

        internal void Impact(UnitEntityData target)
        {
            if (target == null || target.View == null) return;
            if (_startedAt < 0f || !ReferenceEquals(_target, target))
                Begin(target);
            _impactAt = Time.unscaledTime;
            _impacts++;
            ApplyAtCurrentTime();
        }

        internal string Describe()
        {
            return "begins=" + _begins + ";impacts=" + _impacts +
                ";tip=" + _tipIndex +
                ";gap=" + _gapBefore.ToString("0.###",
                    CultureInfo.InvariantCulture) + "->" +
                _gapAfter.ToString("0.###", CultureInfo.InvariantCulture) +
                ";bakedGap=" + _bakedGapAfter.ToString("0.###",
                    CultureInfo.InvariantCulture);
        }

        internal float BakedGapAfter { get { return _bakedGapAfter; } }

        private void LateUpdate()
        { ApplyAtCurrentTime(); }

        private void ApplyAtCurrentTime()
        {
            if (_tail == null || _root == null) return;
            RestoreNative();
            if (_view == null || _target == null || _target.View == null ||
                _startedAt < 0f) return;
            float elapsed = Time.unscaledTime - _startedAt;
            float sinceImpact = _impactAt < 0f ? -1f :
                Time.unscaledTime - _impactAt;
            float weight = EagleAttackLungePolicy.Weight(elapsed, sinceImpact);
            if (weight <= 0f)
            {
                if (elapsed > EagleAttackLungePolicy.PreImpactLimitSeconds +
                    EagleAttackLungePolicy.ReturnSeconds || _impactAt >= 0f)
                    _target = null;
                return;
            }
            SkinnedMeshRenderer targetRenderer = _target.View
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.enabled &&
                    value.sharedMesh != null && value.sharedMesh.vertexCount >= 100 &&
                    value.bones != null && value.bones.Length >= 8)
                .OrderByDescending(value => value.bones.Length)
                .ThenByDescending(value => value.bounds.size.sqrMagnitude)
                .FirstOrDefault();
            if (targetRenderer == null) return;
            Bounds targetBounds = targetRenderer.bounds;
            _nativeTail = _tail.rotation;
            _nativeRoot = _root.position;
            _renderer.BakeMesh(_baked);
            _tipLocal = _tail.InverseTransformPoint(_renderer.transform
                .TransformPoint(_baked.vertices[_tipIndex]));
            Vector3 tip = _tail.TransformPoint(_tipLocal);
            Vector3 current = tip - _tail.position;
            Vector3 desired = targetBounds.ClosestPoint(_tail.position) -
                _tail.position;
            if (current.sqrMagnitude < 0.001f || desired.sqrMagnitude < 0.001f)
                return;
            _gapBefore = Vector3.Distance(tip, targetBounds.ClosestPoint(tip));
            Quaternion aimed = Quaternion.FromToRotation(current, desired) *
                _nativeTail;
            _tail.rotation = Quaternion.Slerp(_nativeTail, aimed, weight);
            tip = _tail.TransformPoint(_tipLocal);
            Vector3 approach = targetBounds.ClosestPoint(tip) - tip;
            float distance = Mathf.Min(approach.magnitude,
                MaximumApproachMeters * weight);
            if (distance > 0.001f)
                _root.position += approach.normalized * distance;
            _appliedTail = _tail.rotation;
            _appliedRoot = _root.position;
            _poseApplied = true;
            tip = _tail.TransformPoint(_tipLocal);
            _gapAfter = Vector3.Distance(tip, targetBounds.ClosestPoint(tip));
            _renderer.BakeMesh(_baked);
            Vector3 bakedTip = _renderer.transform.TransformPoint(
                _baked.vertices[_tipIndex]);
            _bakedGapAfter = Vector3.Distance(bakedTip,
                targetBounds.ClosestPoint(bakedTip));
        }

        private void RestoreNative()
        {
            if (!_poseApplied) return;
            if (_tail != null && Quaternion.Angle(_tail.rotation, _appliedTail) <=
                Quaternion.Angle(_tail.rotation, _nativeTail))
                _tail.rotation = _nativeTail;
            if (_root != null && Vector3.Distance(_root.position, _appliedRoot) <=
                Vector3.Distance(_root.position, _nativeRoot))
                _root.position = _nativeRoot;
            _poseApplied = false;
        }

        private void OnDisable()
        { RestoreNative(); }

        private void OnDestroy()
        {
            RestoreNative();
            if (_baked != null) UnityEngine.Object.Destroy(_baked);
        }
    }

    [HarmonyPatch(typeof(UnitAttack), "TryStartNextAttack")]
    internal static class GiantWaspVisualStingStartPatch
    {
        private static void Postfix(UnitAttack __instance, bool __result)
        {
            if (!__result) return;
            GiantWaspVisualSting pose = GiantWaspVisualSting.For(__instance);
            if (pose != null) pose.Begin(__instance.Target);
        }
    }

    [HarmonyPatch(typeof(RuleAttackWithWeapon), "OnTrigger",
        new[] { typeof(RulebookEventContext) })]
    internal static class GiantWaspVisualStingImpactPatch
    {
        private static void Prefix(RuleAttackWithWeapon __instance)
        {
            if (__instance == null || __instance.Weapon == null ||
                __instance.Weapon.Blueprint == null ||
                __instance.Weapon.Blueprint.name !=
                    "KMG_Summoning_Natural_WaspSting1d8") return;
            GiantWaspVisualSting pose = GiantWaspVisualSting.For(
                __instance.Initiator);
            if (pose == null) return;
            try { pose.Impact(__instance.Target); }
            catch (Exception error)
            {
                // A visual pose must never cancel the game's weapon rule.
                pose.enabled = false;
                Debug.LogException(error);
            }
        }
    }
}
