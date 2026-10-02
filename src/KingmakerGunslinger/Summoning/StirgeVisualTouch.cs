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
    /// Poses only one Stirge's private skeleton for its native touch strike.
    /// The owner entity follows its attached prey at a bounded offset; only
    /// this Stirge's private skeleton is posed for the touch. The prey's
    /// entity, renderer, navigation, selection and collision are untouched.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    internal sealed class StirgeVisualTouch : MonoBehaviour
    {
        private const float MaximumApproachMeters = 2.5f;
        private const float SurfaceClearanceMeters = 0.05f;
        private const float ReturnSeconds = 0.22f;
        private UnitEntityView _view;
        private SkinnedMeshRenderer _renderer;
        private Transform _root;
        private Transform _head;
        private UnitEntityData _target;
        private Vector3 _tipLocal;
        private int _tipIndex = -1;
        private Mesh _baked;
        private float _startedAt = -1f;
        private float _impactAt = -1f;
        private float _releasedAt = -1f;
        private bool _wasAttached;
        private bool _poseApplied;
        private Vector3 _nativeRootPosition;
        private Vector3 _appliedRootPosition;
        private Quaternion _nativeRootRotation;
        private Quaternion _appliedRootRotation;
        private Quaternion _nativeHeadRotation;
        private Quaternion _appliedHeadRotation;
        private float _bakedGap = -1f;
        private float _modelForwardDot = -1f;
        private bool _tipInside;
        private int _begins;
        private int _impacts;

        internal void Configure(UnitEntityView view,
            SkinnedMeshRenderer renderer)
        {
            if (view == null || renderer == null || renderer.rootBone == null ||
                renderer.sharedMesh == null || renderer.sharedMesh.name !=
                    ExpandedSummoningPteranodonViewPatch.StirgeVisualName)
                throw new InvalidOperationException(
                    "Stirge touch pose requires an attached original renderer.");
            Transform[] bones = renderer.bones;
            int headIndex = Array.FindIndex(bones, value => value != null &&
                value.name == "Head");
            BoneWeight[] weights = renderer.sharedMesh.boneWeights;
            if (headIndex < 0 || weights == null || weights.Length == 0)
                throw new InvalidOperationException(
                    "Stirge touch pose requires a weighted Head bone.");
            Mesh baked = new Mesh();
            baked.name = "KMG_StirgeTouchProbe";
            try
            {
                renderer.BakeMesh(baked);
                Vector3[] vertices = baked.vertices;
                if (vertices == null || vertices.Length != weights.Length)
                    throw new InvalidOperationException(
                        "Stirge touch pose has no matching baked vertices.");
                Transform head = bones[headIndex];
                int tipIndex = -1;
                float longest = -1f;
                for (int index = 0; index < vertices.Length; index++)
                {
                    BoneWeight weight = weights[index];
                    bool headOwned =
                        weight.boneIndex0 == headIndex && weight.weight0 >= 0.75f ||
                        weight.boneIndex1 == headIndex && weight.weight1 >= 0.75f ||
                        weight.boneIndex2 == headIndex && weight.weight2 >= 0.75f ||
                        weight.boneIndex3 == headIndex && weight.weight3 >= 0.75f;
                    if (!headOwned) continue;
                    Vector3 world = World(renderer, vertices[index]);
                    float length = (world - head.position).sqrMagnitude;
                    if (length <= longest) continue;
                    longest = length;
                    tipIndex = index;
                }
                if (tipIndex < 0)
                    throw new InvalidOperationException(
                        "Stirge touch pose found no Head-owned proboscis tip.");
                _view = view;
                _renderer = renderer;
                _root = renderer.rootBone;
                _head = head;
                _tipIndex = tipIndex;
                _tipLocal = head.InverseTransformPoint(
                    World(renderer, vertices[tipIndex]));
                _baked = baked;
                baked = null;
            }
            finally
            {
                if (baked != null) UnityEngine.Object.Destroy(baked);
            }
        }

        internal static StirgeVisualTouch For(UnitAttack command)
        { return For(command == null ? null : command.Executor); }

        internal static StirgeVisualTouch For(UnitEntityData owner)
        {
            if (owner == null || owner.Blueprint == null || owner.View == null ||
                owner.Blueprint.name != ExpandedSummoningPteranodonViewPatch
                    .StirgeBlueprintName) return null;
            StirgeVisualTouch pose = owner.View.GetComponent<StirgeVisualTouch>();
            return pose != null && pose.isActiveAndEnabled &&
                pose._renderer != null && pose._renderer.sharedMesh != null &&
                pose._renderer.sharedMesh.name ==
                    ExpandedSummoningPteranodonViewPatch.StirgeVisualName
                ? pose : null;
        }

        internal void Begin(UnitEntityData target)
        {
            if (_view == null || _head == null || target == null ||
                target.View == null) return;
            _target = target;
            _startedAt = Time.unscaledTime;
            _impactAt = -1f;
            _releasedAt = -1f;
            _wasAttached = false;
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
                ";tip=" + _tipIndex + ";bakedGap=" +
                _bakedGap.ToString("0.###", CultureInfo.InvariantCulture) +
                ";inside=" + _tipInside + ";offset=" +
                Vector3.Distance(_nativeRootPosition, _appliedRootPosition)
                    .ToString("0.###", CultureInfo.InvariantCulture) +
                ";modelForwardDot=" + _modelForwardDot.ToString("0.###",
                    CultureInfo.InvariantCulture);
        }

        internal float BakedGap { get { return _bakedGap; } }
        internal bool TipInside { get { return _tipInside; } }
        internal float ModelForwardDot { get { return _modelForwardDot; } }

        private void LateUpdate()
        {
            if (_view != null) StirgeHoldComponent.FollowAttached(_view.EntityData);
            ApplyAtCurrentTime();
        }

        private float Weight()
        {
            if (_target == null) return 0f;
            bool attached = _view != null && _view.EntityData != null &&
                ReferenceEquals(StirgeHoldComponent.AttachedTarget(
                    _view.EntityData), _target);
            if (attached)
            {
                _wasAttached = true;
                _releasedAt = -1f;
                return 1f;
            }
            if (_wasAttached)
            {
                if (_releasedAt < 0f) _releasedAt = Time.unscaledTime;
                return Mathf.Max(0f, 1f -
                    (Time.unscaledTime - _releasedAt) / ReturnSeconds);
            }
            return EagleAttackLungePolicy.Weight(
                Time.unscaledTime - _startedAt,
                _impactAt < 0f ? -1f : Time.unscaledTime - _impactAt);
        }

        private void ApplyAtCurrentTime()
        {
            if (_root == null || _head == null || _renderer == null ||
                _baked == null) return;
            RestoreNative();
            if (_target == null || _target.View == null || _startedAt < 0f)
                return;
            float weight = Weight();
            if (weight <= 0f)
            {
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
            _nativeRootPosition = _root.position;
            _nativeRootRotation = _root.rotation;
            _nativeHeadRotation = _head.rotation;
            Vector3 toward = targetBounds.center - _root.position;
            toward.y = 0f;
            // The original source is authored with -Z forward in the donor
            // renderer's local frame. +Z is the back of its head.
            Vector3 forward = -_root.forward;
            forward.y = 0f;
            if (toward.sqrMagnitude > 0.001f &&
                forward.sqrMagnitude > 0.001f)
                _root.rotation = Quaternion.Slerp(_nativeRootRotation,
                    Quaternion.FromToRotation(forward, toward) *
                        _nativeRootRotation, weight);
            _renderer.BakeMesh(_baked);
            _tipLocal = _head.InverseTransformPoint(
                World(_renderer, _baked.vertices[_tipIndex]));
            Vector3 tip = _head.TransformPoint(_tipLocal);
            Vector3 from = tip - _head.position;
            Vector3 to = targetBounds.ClosestPoint(_head.position) -
                _head.position;
            if (from.sqrMagnitude > 0.001f && to.sqrMagnitude > 0.001f)
                _head.rotation = Quaternion.Slerp(_nativeHeadRotation,
                    Quaternion.FromToRotation(from, to) *
                        _nativeHeadRotation, weight);
            tip = _head.TransformPoint(_tipLocal);
            Vector3 approach = targetBounds.ClosestPoint(tip) - tip;
            float distance = Mathf.Min(Mathf.Max(0f,
                approach.magnitude - SurfaceClearanceMeters),
                MaximumApproachMeters * weight);
            if (distance > 0.001f)
                _root.position += approach.normalized * distance;
            _appliedRootPosition = _root.position;
            _appliedRootRotation = _root.rotation;
            _appliedHeadRotation = _head.rotation;
            _poseApplied = true;
            _renderer.BakeMesh(_baked);
            Vector3 bakedTip = World(_renderer,
                _baked.vertices[_tipIndex]);
            _bakedGap = Vector3.Distance(bakedTip,
                targetBounds.ClosestPoint(bakedTip));
            _tipInside = targetBounds.Contains(bakedTip);
            Vector3 modelForward = -_root.forward;
            modelForward.y = 0f;
            Vector3 targetDirection = targetBounds.center - _root.position;
            targetDirection.y = 0f;
            _modelForwardDot = modelForward.sqrMagnitude < 0.001f ||
                targetDirection.sqrMagnitude < 0.001f ? -1f :
                Vector3.Dot(modelForward.normalized,
                    targetDirection.normalized);
        }

        private static Vector3 World(SkinnedMeshRenderer renderer,
            Vector3 bakedVertex)
        {
            // BakeMesh includes the view's 0.25 scale; TransformPoint would
            // apply that scale a second time.
            return renderer.transform.position +
                renderer.transform.rotation * bakedVertex;
        }

        private void RestoreNative()
        {
            if (!_poseApplied) return;
            if (_head != null && Quaternion.Angle(_head.rotation,
                    _appliedHeadRotation) <= Quaternion.Angle(_head.rotation,
                    _nativeHeadRotation))
                _head.rotation = _nativeHeadRotation;
            if (_root != null)
            {
                if (Vector3.Distance(_root.position,
                        _appliedRootPosition) <= Vector3.Distance(
                        _root.position, _nativeRootPosition))
                    _root.position = _nativeRootPosition;
                if (Quaternion.Angle(_root.rotation,
                        _appliedRootRotation) <= Quaternion.Angle(
                        _root.rotation, _nativeRootRotation))
                    _root.rotation = _nativeRootRotation;
            }
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
    internal static class StirgeVisualTouchStartPatch
    {
        private static void Postfix(UnitAttack __instance, bool __result)
        {
            if (!__result) return;
            StirgeVisualTouch pose = StirgeVisualTouch.For(__instance);
            if (pose != null) pose.Begin(__instance.Target);
        }
    }

    [HarmonyPatch(typeof(RuleAttackWithWeapon), "OnTrigger",
        new[] { typeof(RulebookEventContext) })]
    internal static class StirgeVisualTouchImpactPatch
    {
        private static void Prefix(RuleAttackWithWeapon __instance)
        {
            if (__instance == null || __instance.Weapon == null ||
                __instance.Weapon.Blueprint == null ||
                __instance.Weapon.Blueprint.name !=
                    "KMG_Summoning_Natural_StirgeTouch") return;
            StirgeVisualTouch pose = StirgeVisualTouch.For(
                __instance.Initiator);
            if (pose == null) return;
            try { pose.Impact(__instance.Target); }
            catch (Exception error)
            {
                // A view pose must never cancel the authoritative weapon rule.
                pose.enabled = false;
                Debug.LogException(error);
            }
        }
    }
}
