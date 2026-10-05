using System;
using System.Globalization;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// An instance-local contact pose for the two original crocodilians. The
    /// native attack owns its timing, target and result. Only their fixed tail
    /// chain and visual root move; unit position, reach and collision do not.
    /// As with the qualified Wasp pose, restore only our still-applied offsets.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    internal sealed class CrocodilianAttackVisualPose : MonoBehaviour
    {
        private UnitEntityView _view;
        private string _key;
        private BlueprintUnit _owner;
        private BlueprintItemWeapon _bite;
        private BlueprintItemWeapon _tailWeapon;
        private SkinnedMeshRenderer _renderer;
        private Mesh _mesh;
        private Transform[] _bones;
        private Matrix4x4[] _bindposes;
        private Matrix4x4[] _skin;
        private Vector3[] _vertices;
        private BoneWeight[] _weights;
        private int[] _jawVertices;
        private int[] _tailVertices;
        private int _tailTip;
        private Transform _tail;
        private Transform _root;
        private UnitEntityData _target;
        private bool _tailAttack;
        private float _startedAt = -1f;
        private float _impactAt = -1f;
        private Quaternion _nativeTail;
        private Quaternion _appliedTail;
        private Vector3 _nativeRoot;
        private Vector3 _appliedRoot;
        private bool _applied;
        private int _impacts;
        private float _gapBefore = -1f;
        private float _gapAfter = -1f;

        internal void Configure(string key, UnitEntityView view,
            SkinnedMeshRenderer renderer)
        {
            var blueprint = view == null || view.EntityData == null ? null : view.EntityData.Blueprint;
            // The weapon rules belong to the owned CombatTraits fact, not to
            // the unit's direct components. Resolve that exact attached graph
            // once and cache its owner/weapon references for attack callbacks.
            var stats = blueprint == null ? null :
                (blueprint.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                    .Where(fact => fact != null)
                    .SelectMany(fact => fact.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<SummonCrocodilianWeaponStats>()
                    .SingleOrDefault(value => ReferenceEquals(value.OwningBlueprint, blueprint));
            if (stats == null || !ReferenceEquals(stats.OwningBlueprint,
                    view.EntityData.Blueprint) || stats.Bite == null || stats.Tail == null ||
                renderer == null || renderer.sharedMesh == null || renderer.rootBone == null ||
                renderer.sharedMesh.name != "KMG_" + key + "_Original" ||
                !CrocodilianVisualPolicy.IsPermitted(key,
                    renderer.bones.Select(bone => bone == null ? null : bone.name)))
                throw new InvalidOperationException(
                    "Crocodilian contact requires its exact owner and original skin.");
            _mesh = renderer.sharedMesh;
            _vertices = _mesh.vertices;
            _weights = _mesh.boneWeights;
            _bones = renderer.bones;
            _bindposes = _mesh.bindposes;
            if (_vertices.Length != _weights.Length || _bindposes.Length != _bones.Length)
                throw new InvalidOperationException("Crocodilian skin arrays disagree.");
            _skin = new Matrix4x4[_bones.Length];
            _jawVertices = Enumerable.Range(0, _vertices.Length).Where(index =>
                HasDriver(_weights[index], false)).ToArray();
            _tailVertices = Enumerable.Range(0, _vertices.Length).Where(index =>
                HasDriver(_weights[index], true)).ToArray();
            _tail = _bones.Single(bone => bone.name == "cent_tail1_jnt");
            _root = renderer.rootBone;
            if (_jawVertices.Length == 0 || _tailVertices.Length == 0)
                throw new InvalidOperationException("Crocodilian contact drivers are absent.");
            RefreshSkin();
            _tailTip = _tailVertices.OrderByDescending(index =>
                (WorldVertex(index) - _tail.position).sqrMagnitude).First();
            _owner = stats.OwningBlueprint;
            _key = key;
            _bite = stats.Bite;
            _tailWeapon = stats.Tail;
            _view = view;
            _renderer = renderer;
        }

        private bool HasDriver(BoneWeight weight, bool tail)
        {
            return Driver(weight.boneIndex0, weight.weight0, tail) ||
                Driver(weight.boneIndex1, weight.weight1, tail) ||
                Driver(weight.boneIndex2, weight.weight2, tail) ||
                Driver(weight.boneIndex3, weight.weight3, tail);
        }

        private bool Driver(int index, float weight, bool tail)
        {
            if (weight < 0.25f || index < 0 || index >= _bones.Length) return false;
            string name = _bones[index].name;
            return tail ? name.StartsWith("cent_tail", StringComparison.Ordinal) :
                name == "cent_head1_jnt" || name == "cent_jaw1_jnt";
        }

        internal static CrocodilianAttackVisualPose For(UnitEntityData owner)
        {
            if (owner == null || owner.View == null) return null;
            var pose = owner.View.GetComponent<CrocodilianAttackVisualPose>();
            return pose != null && pose.isActiveAndEnabled &&
                ReferenceEquals(pose._owner, owner.Blueprint) &&
                ReferenceEquals(pose._view, owner.View) && pose._renderer != null &&
                ReferenceEquals(pose._mesh, pose._renderer.sharedMesh) ? pose : null;
        }

        internal void Begin(UnitEntityData target, BlueprintItemWeapon weapon)
        {
            if (_view == null || target == null || target.View == null ||
                !ReferenceEquals(weapon, _bite) && !ReferenceEquals(weapon, _tailWeapon)) return;
            RestoreNative();
            _tailAttack = ReferenceEquals(weapon, _tailWeapon);
            _target = target;
            _startedAt = Time.unscaledTime;
            _impactAt = -1f;
        }

        internal void Impact(UnitEntityData target, BlueprintItemWeapon weapon)
        {
            if (!ReferenceEquals(weapon, _bite) && !ReferenceEquals(weapon, _tailWeapon)) return;
            if (_startedAt < 0f || !ReferenceEquals(_target, target) ||
                _tailAttack != ReferenceEquals(weapon, _tailWeapon)) Begin(target, weapon);
            _impactAt = Time.unscaledTime;
            _impacts++;
            ApplyAtCurrentTime();
        }

        internal string Describe()
        {
            return "impacts=" + _impacts + ";kind=" + (_tailAttack ? "tail" : "bite") +
                ";approachCap=" + CrocodilianVisualPolicy.ContactApproach(_key, float.MaxValue, 1f)
                    .ToString("0.###", CultureInfo.InvariantCulture) +
                ";gap=" + _gapBefore.ToString("0.###", CultureInfo.InvariantCulture) +
                "->" + _gapAfter.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private void LateUpdate()
        {
            try { ApplyAtCurrentTime(); }
            catch (Exception error) { enabled = false; Debug.LogException(error); }
        }

        private void RefreshSkin()
        {
            for (int index = 0; index < _bones.Length; index++)
                _skin[index] = _bones[index].localToWorldMatrix * _bindposes[index];
        }

        private Vector3 WorldVertex(int index)
        {
            BoneWeight w = _weights[index];
            Vector3 v = _vertices[index];
            return _skin[w.boneIndex0].MultiplyPoint3x4(v) * w.weight0 +
                _skin[w.boneIndex1].MultiplyPoint3x4(v) * w.weight1 +
                _skin[w.boneIndex2].MultiplyPoint3x4(v) * w.weight2 +
                _skin[w.boneIndex3].MultiplyPoint3x4(v) * w.weight3;
        }

        private Vector3 ClosestSurface(Bounds bounds, out float gap)
        {
            Vector3 closest = Vector3.zero;
            gap = float.MaxValue;
            foreach (int index in _tailAttack ? _tailVertices : _jawVertices)
            {
                Vector3 point = WorldVertex(index);
                float distance = Vector3.Distance(point, bounds.ClosestPoint(point));
                if (distance < gap) { gap = distance; closest = point; }
            }
            return closest;
        }

        private void ApplyAtCurrentTime()
        {
            RestoreNative();
            if (_view == null || _renderer == null || _target == null ||
                _target.View == null || _startedAt < 0f) return;
            float elapsed = Time.unscaledTime - _startedAt;
            float sinceImpact = _impactAt < 0f ? -1f : Time.unscaledTime - _impactAt;
            float weight = EagleAttackLungePolicy.Weight(elapsed, sinceImpact);
            if (weight <= 0f) { if (_impactAt >= 0f || elapsed > 1.22f) _target = null; return; }
            var targetRenderer = _target.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.enabled && value.sharedMesh != null &&
                    value.sharedMesh.vertexCount >= 100 && value.bones.Length >= 8)
                .OrderByDescending(value => value.bones.Length)
                .ThenByDescending(value => value.bounds.size.sqrMagnitude).FirstOrDefault();
            if (targetRenderer == null) return;
            Bounds bounds = targetRenderer.bounds;
            _nativeTail = _tail.rotation;
            _nativeRoot = _root.position;
            RefreshSkin();
            ClosestSurface(bounds, out _gapBefore);
            if (_tailAttack)
            {
                Vector3 current = WorldVertex(_tailTip) - _tail.position;
                Vector3 desired = bounds.ClosestPoint(_tail.position) - _tail.position;
                if (current.sqrMagnitude > 0.001f && desired.sqrMagnitude > 0.001f)
                    _tail.rotation = Quaternion.Slerp(_nativeTail,
                        Quaternion.FromToRotation(current, desired) * _nativeTail, weight);
                RefreshSkin();
            }
            float gap;
            Vector3 surface = ClosestSurface(bounds, out gap);
            Vector3 approach = bounds.ClosestPoint(surface) - surface;
            float distance = CrocodilianVisualPolicy.ContactApproach(_key, approach.magnitude, weight);
            if (distance > 0.001f) _root.position += approach.normalized * distance;
            _appliedTail = _tail.rotation;
            _appliedRoot = _root.position;
            _applied = true;
            RefreshSkin();
            ClosestSurface(bounds, out _gapAfter);
        }

        internal void RestoreNative()
        {
            if (!_applied) return;
            if (_tail != null && Quaternion.Angle(_tail.rotation, _appliedTail) <=
                    Quaternion.Angle(_tail.rotation, _nativeTail)) _tail.rotation = _nativeTail;
            if (_root != null && Vector3.Distance(_root.position, _appliedRoot) <=
                    Vector3.Distance(_root.position, _nativeRoot)) _root.position = _nativeRoot;
            _applied = false;
        }
        private void OnDisable() { RestoreNative(); }
        private void OnDestroy() { RestoreNative(); }
    }

    [HarmonyPatch(typeof(UnitAttack), "TryStartNextAttack", new[] { typeof(bool) })]
    internal static class CrocodilianAttackVisualStartPatch
    {
        private static void Postfix(UnitAttack __instance, bool __result)
        {
            if (!__result || __instance == null || __instance.PlannedAttack == null ||
                __instance.PlannedAttack.Hand == null || __instance.PlannedAttack.Hand.Weapon == null) return;
            var pose = CrocodilianAttackVisualPose.For(__instance.Executor);
            if (pose == null) return;
            try { pose.Begin(__instance.Target, __instance.PlannedAttack.Hand.Weapon.Blueprint); }
            catch (Exception error) { pose.enabled = false; Debug.LogException(error); }
        }
    }

    [HarmonyPatch(typeof(RuleAttackWithWeapon), "OnTrigger", new[] { typeof(RulebookEventContext) })]
    internal static class CrocodilianAttackVisualImpactPatch
    {
        private static void Prefix(RuleAttackWithWeapon __instance)
        {
            if (__instance == null || __instance.Weapon == null) return;
            var pose = CrocodilianAttackVisualPose.For(__instance.Initiator);
            if (pose == null) return;
            try { pose.Impact(__instance.Target, __instance.Weapon.Blueprint); }
            catch (Exception error) { pose.enabled = false; Debug.LogException(error); }
        }
    }
}
