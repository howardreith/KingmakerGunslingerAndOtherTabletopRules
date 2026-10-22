using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // Native UnitAttack owns its HasAnimation=false immediate Tick branch,
    // unchanged stored ScheduleAct(1), rule, cost
    // and lifetime. Only this exact project skin receives original pose art.
    [DefaultExecutionOrder(20000)]
    internal sealed class SummonAttackGestureVisual : MonoBehaviour
    {
        internal sealed class Schedule
        {
            internal int Serial, Index, Limb, Samples;
            internal UnitAttack Command;
            internal AttackHandInfo Info;
            internal ItemEntityWeapon Weapon;
            internal float Start, Deadline, Delay, ActTime = -1f, MaximumAngle;
            internal float NativeActTickDelta;
            internal int EligibleTicks, ScheduleFrame, ActFrame, ActTickSerial;
            internal bool HasAnimationAtSchedule, HasAnimationAtAct, HandledAtSchedule, HandledAtAct, OneFrameAtAct;
            internal double ActGame = -1d;
            internal bool Suppressed, Cancelled;
        }
        private sealed class Mirror
        { internal Transform Source, Copy, SourceParent, CopyParent; }
        private sealed class Pivot
        { internal int Index, Joint, Limb; internal Transform Source, Turn, Recenter, Parent; internal Vector3 Offset; }
        private sealed class Suppression : IDisposable
        {
            internal SummonAttackGestureVisual Owner;
            public void Dispose()
            {
                if (Owner == null) return;
                if (ReferenceEquals(Owner._suppression, this)) { Owner._suppression = null; Owner._suppressed = false; }
                Owner = null;
            }
        }
        private static readonly FieldInfo PretendAct = typeof(UnitCommand).GetField("m_PretendActTime", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo HasAnimation = typeof(UnitCommand).GetField("HasAnimation", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActHandled = typeof(UnitCommand).GetField("m_IsAnimationActHandled", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo SerializedSet = typeof(AnimationManager).GetField("m_AnimationSet", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly List<Mirror> _mirrors = new List<Mirror>();
        private readonly List<Pivot> _pivots = new List<Pivot>();
        private readonly Dictionary<Transform, Transform> _copies = new Dictionary<Transform, Transform>();
        private readonly Dictionary<Transform, Transform> _childParents = new Dictionary<Transform, Transform>();
        private readonly List<Schedule> _schedules = new List<Schedule>();
        private UnitEntityView _view;
        private UnitEntityData _actor, _caster;
        private SkinnedMeshRenderer _renderer;
        private Mesh _mesh;
        private Transform _carrier, _rootBone;
        private Transform[] _nativeBones, _ownedBones;
        private Matrix4x4[] _nativeBind, _ownedBind;
        private Bounds _originalBounds, _ownedBounds;
        private Func<bool> _receipt;
        private UnitAnimationManager _manager, _prefabManager;
        private AnimationSet _nativeSet;
        private List<AnimationActionBase> _nativeActionList;
        private AnimationActionBase[] _nativeActions;
        private string _key, _refusal;
        private bool _configured, _released, _suppressed;
        private Suppression _suppression;
        private int _serial, _proofSamples;
        private int _sourceInvariantSamples, _sourceInvariantFailures;
        private float _maximumZeroError;
        private UnitAttack _tickCommand;
        private int _tickSerial, _tickFrame;
        private float _tickBefore, _tickDelta;
        internal string Key { get { return _key; } }
        internal bool Ready { get { return _configured && !_released && ExactPalette() && _receipt != null && _receipt(); } }
        internal IList<Schedule> Schedules { get { return _schedules.AsReadOnly(); } }

        internal void Configure(string key, UnitEntityView view, SkinnedMeshRenderer renderer, Func<bool> receipt)
        {
            _key = key; _view = view; _actor = view == null ? null : view.EntityData; _renderer = renderer; _receipt = receipt;
            try
            {
                if (!SummonAttackGesturePolicy.Handles(key) || view == null || _actor == null || renderer == null ||
                    receipt == null || !receipt() || renderer.GetComponent<Cloth>() != null || PretendAct == null || HasAnimation == null || ActHandled == null ||
                    renderer.sharedMesh == null || !renderer.transform.IsChildOf(view.transform))
                    throw new InvalidOperationException("Exact ordinary original skin receipt unavailable.");
                _mesh = renderer.sharedMesh; _nativeBones = renderer.bones; _rootBone = renderer.rootBone;
                _nativeBind = _mesh.bindposes; _ownedBind = _nativeBind.ToArray(); _ownedBones = _nativeBones.ToArray();
                _originalBounds = renderer.localBounds;
                if (_nativeBones.Length != _nativeBind.Length || _nativeBones.Length == 0 || _nativeBones.Length > 128 ||
                    _nativeBones.Any(value => value == null || !value.IsChildOf(view.transform)))
                    throw new InvalidOperationException("Unsupported project palette or foreign skin bone.");
                var selected = key == SummonAttackGesturePolicy.Crab ? new[] { "L", "R" }.SelectMany(side => Enumerable.Range(1, 7).Select(i => "pedipalp" + i + "_" + side)).ToArray() :
                    new[] { "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M" };
                if (selected.Any(name => _nativeBones.Count(value => value.name == name) != 1))
                    throw new InvalidOperationException("Exact authored gesture joints absent or ambiguous.");
                if (key == SummonAttackGesturePolicy.Brachiosaurus && (_mesh.vertexCount != 8302 || _nativeBones.Length != 29))
                    throw new InvalidOperationException("Unreviewed Brachiosaurus body revision.");
                if (key == SummonAttackGesturePolicy.Brachiosaurus) ValidateTailWeightPayload();
                _carrier = new GameObject("KMG_OriginalAttackPalette_" + key).transform;
                _carrier.SetParent(view.transform, false);
                Vector3[] authored = { new Vector3(0, 3.25f, 2.9f), new Vector3(0, 2.9f, 3.8f), new Vector3(0, 2.62f, 4.8f), new Vector3(0, 2.45f, 5.75f) };
                // Establish ancestor pivots before cloning their descendants.
                foreach (string name in selected.OrderBy(name => Depth(_nativeBones.Single(value => value.name == name))))
                {
                    int index = Array.FindIndex(_nativeBones, value => value.name == name), ordinal = Array.IndexOf(selected, name);
                    Transform source = _nativeBones[index], copy = MirrorPath(source);
                    Vector3 offset = key == SummonAttackGesturePolicy.Brachiosaurus ? _nativeBind[index].MultiplyPoint3x4(authored[ordinal]) : Vector3.zero;
                    if (!Finite(offset)) throw new InvalidOperationException("Non-finite live bind-derived pivot.");
                    Transform turn = new GameObject("KMG_OriginalPose_" + name).transform; turn.SetParent(copy, false); turn.localPosition = offset;
                    Transform recenter = new GameObject("KMG_PivotRecenter_" + name).transform; recenter.SetParent(turn, false); recenter.localPosition = -offset;
                    _childParents[source] = recenter;
                    _pivots.Add(new Pivot { Index = index, Source = source, Turn = turn, Recenter = recenter, Parent = copy, Offset = offset,
                        Limb = key == SummonAttackGesturePolicy.Crab ? ordinal / 7 : 0, Joint = key == SummonAttackGesturePolicy.Crab ? ordinal % 7 : ordinal });
                    _ownedBones[index] = turn;
                    _ownedBind[index] = Matrix4x4.TRS(-offset, Quaternion.identity, Vector3.one) * _nativeBind[index];
                }
                CopyCurrentPose();
                renderer.bones = _ownedBones; _mesh.bindposes = _ownedBind;
                // Culling envelope only. This is never an anatomy/contact or
                // range measurement, and no native collider is changed.
                _ownedBounds = _originalBounds; _ownedBounds.Expand(key == SummonAttackGesturePolicy.Brachiosaurus ? 12f : 5f);
                renderer.localBounds = _ownedBounds;
                _configured = true;
                if (!ProveCurrentPose()) throw new InvalidOperationException("Actual current-pose zero-offset skin equality failed.");
            }
            catch (Exception error)
            {
                _refusal = error.GetType().Name + ":" + error.Message;
                Release(); // Body attachment remains intact, gesture refuses.
            }
        }

        private static int Depth(Transform value) { int result = 0; while (value != null && result < 256) { result++; value = value.parent; } return result; }
        private void ValidateTailWeightPayload()
        {
            BoneWeight[] weights = _mesh.boneWeights; int[] triangles = _mesh.triangles;
            var tail = new HashSet<int>(Enumerable.Range(0, 5).Select(j => Array.FindIndex(_nativeBones, bone => bone.name == "Tail" + j + "_M")));
            int tail0 = Array.FindIndex(_nativeBones, bone => bone.name == "Tail0_M");
            if (tail.Contains(-1) || weights.Length != 8302 || triangles.Length != 12836 * 3) throw new InvalidOperationException("Unreviewed original tail payload.");
            bool[] owned = weights.Select(w => w.weight0 > 0 && tail.Contains(w.boneIndex0) || w.weight1 > 0 && tail.Contains(w.boneIndex1) ||
                w.weight2 > 0 && tail.Contains(w.boneIndex2) || w.weight3 > 0 && tail.Contains(w.boneIndex3)).ToArray();
            var boundary = new HashSet<int>(); int junction = 0;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int count = (owned[triangles[i]] ? 1 : 0) + (owned[triangles[i + 1]] ? 1 : 0) + (owned[triangles[i + 2]] ? 1 : 0);
                if (count == 0 || count == 3) continue;
                junction++; for (int j = 0; j < 3; j++) if (owned[triangles[i + j]]) boundary.Add(triangles[i + j]);
            }
            if (owned.Count(value => value) != 605 || junction != 70 || boundary.Count != 40 || boundary.Any(index => {
                BoneWeight w = weights[index]; return (w.weight0 > 0 && w.boneIndex0 != tail0) || (w.weight1 > 0 && w.boneIndex1 != tail0) ||
                    (w.weight2 > 0 && w.boneIndex2 != tail0) || (w.weight3 > 0 && w.boneIndex3 != tail0); }))
                throw new InvalidOperationException("Original v4 pelvis-junction weights are not source-pinned.");
        }
        private Transform MirrorPath(Transform source)
        {
            if (source == _view.transform) return _carrier;
            Transform found;
            if (_copies.TryGetValue(source, out found)) return found;
            if (source == null || !source.IsChildOf(_view.transform) || _mirrors.Count >= 256)
                throw new InvalidOperationException("Unbounded/foreign native skin ancestry.");
            Transform parent;
            if (!_childParents.TryGetValue(source.parent, out parent)) parent = MirrorPath(source.parent);
            var copy = new GameObject("KMG_AttackMirror_" + _mirrors.Count).transform; copy.SetParent(parent, false);
            _copies.Add(source, copy); _mirrors.Add(new Mirror { Source = source, Copy = copy, SourceParent = source.parent, CopyParent = parent });
            return copy;
        }
        private void CopyCurrentPose()
        {
            foreach (Mirror m in _mirrors)
            {
                if (m.Source == null || m.Copy == null) throw new InvalidOperationException("Native or owned pose path destroyed.");
                m.Copy.localPosition = m.Source.localPosition; m.Copy.localRotation = m.Source.localRotation; m.Copy.localScale = m.Source.localScale;
            }
        }
        private bool ExactPalette()
        {
            if (_renderer == null || _mesh == null || !ReferenceEquals(_renderer.sharedMesh, _mesh) ||
                !ReferenceEquals(_renderer.rootBone, _rootBone) || _ownedBones == null || _ownedBind == null) return false;
            Transform[] bones = _renderer.bones; Matrix4x4[] bind = _mesh.bindposes;
            return bones.Length == _ownedBones.Length && bind.Length == _ownedBind.Length &&
                bones.Where((value, i) => !ReferenceEquals(value, _ownedBones[i])).Any() == false &&
                bind.Where((value, i) => !value.Equals(_ownedBind[i])).Any() == false &&
                _carrier != null && ReferenceEquals(_carrier.parent, _view.transform) && _carrier.localPosition == Vector3.zero &&
                _carrier.localRotation == Quaternion.identity && _carrier.localScale == Vector3.one &&
                _mirrors.All(value => value.Source != null && value.Copy != null && ReferenceEquals(value.Source.parent, value.SourceParent) && ReferenceEquals(value.Copy.parent, value.CopyParent)) &&
                _pivots.All(value => value.Turn != null && value.Recenter != null && value.Source != null &&
                    ReferenceEquals(value.Turn.parent, value.Parent) && ReferenceEquals(value.Recenter.parent, value.Turn) &&
                    value.Turn.localPosition == value.Offset && value.Turn.localScale == Vector3.one &&
                    value.Recenter.localPosition == -value.Offset && value.Recenter.localRotation == Quaternion.identity && value.Recenter.localScale == Vector3.one);
        }

        // Synchronous pair in the SAME current native pose, not old bind data.
        // Only aggregate error escapes; temporary project-skin arrays die here.
        internal bool ProveCurrentPose()
        {
            if (!_configured || _released || !ExactPalette() || _receipt == null || !_receipt()) return false;
            var before = new Mesh(); var after = new Mesh();
            Quaternion[] rotations = _pivots.Select(value => value.Turn.localRotation).ToArray();
            try
            {
                CopyCurrentPose(); foreach (Pivot p in _pivots) p.Turn.localRotation = Quaternion.identity;
                _renderer.bones = _nativeBones; _mesh.bindposes = _nativeBind; _renderer.BakeMesh(before);
                _renderer.bones = _ownedBones; _mesh.bindposes = _ownedBind; _renderer.BakeMesh(after);
                Vector3[] a = before.vertices, b = after.vertices;
                if (a.Length != _mesh.vertexCount || b.Length != a.Length || a.Length == 0) return false;
                float maximum = 0f;
                for (int i = 0; i < a.Length; i++)
                { if (!Finite(a[i]) || !Finite(b[i])) return false; maximum = Mathf.Max(maximum, Vector3.Distance(a[i], b[i])); }
                _proofSamples++; _maximumZeroError = Mathf.Max(_maximumZeroError, maximum);
                return maximum <= .0002f;
            }
            finally
            {
                // These assignments occur without yielding, external callback
                // or native skeleton writes; no foreign frame is overwritten.
                if (_renderer != null && ReferenceEquals(_renderer.sharedMesh, _mesh)) { _renderer.bones = _ownedBones; _mesh.bindposes = _ownedBind; }
                for (int i = 0; i < rotations.Length; i++) if (_pivots[i].Turn != null) _pivots[i].Turn.localRotation = rotations[i];
                UnityEngine.Object.DestroyImmediate(before); UnityEngine.Object.DestroyImmediate(after);
            }
        }

        internal bool ProveSameFrameSourceState()
        {
            if (!Ready) return false;
            Transform[] sources = _mirrors.Select(value => value.Source).Concat(new[] { _view.transform }).Distinct().ToArray();
            Matrix4x4[] matrices = sources.Select(value => value.localToWorldMatrix).ToArray();
            Transform[] parents = sources.Select(value => value.parent).ToArray();
            Collider[] colliders = _view.GetComponentsInChildren<Collider>(true);
            if (colliders.Any(value => !(value is CapsuleCollider) && !(value is SphereCollider) && !(value is BoxCollider) &&
                !(value is MeshCollider) && !(value is CharacterController))) throw new NotSupportedException("Unmeasured native collider geometry type.");
            string[] geometry = colliders.Select(ColliderGeometry).ToArray();
            Matrix4x4[] colliderMatrices = colliders.Select(value => value.transform.localToWorldMatrix).ToArray();
            ApplyCurrentPose();
            bool same = sources.Where((value, index) => value == null || !ReferenceEquals(value.parent, parents[index]) || !value.localToWorldMatrix.Equals(matrices[index])).Any() == false &&
                colliders.SequenceEqual(_view.GetComponentsInChildren<Collider>(true)) && colliders.Where((value, index) =>
                    value == null || ColliderGeometry(value) != geometry[index] || !value.transform.localToWorldMatrix.Equals(colliderMatrices[index])).Any() == false;
            _sourceInvariantSamples++; if (!same) _sourceInvariantFailures++;
            return same;
        }
        internal JObject MeasureGestureDisplacement()
        {
            if (!Ready) throw new InvalidOperationException("Exact private skin receipt lost before current-pose measurement.");
            var posed = new Mesh(); var native = new Mesh();
            try
            {
                ApplyCurrentPose(); _renderer.BakeMesh(posed);
                _renderer.bones = _nativeBones; _mesh.bindposes = _nativeBind; _renderer.BakeMesh(native);
                Vector3[] a = posed.vertices, b = native.vertices; BoneWeight[] weights = _mesh.boneWeights;
                if (a.Length != _mesh.vertexCount || a.Length != b.Length || a.Length != weights.Length)
                    throw new InvalidOperationException("Current-pose comparison vertex counts differ.");
                var selected = new HashSet<int>(_pivots.Select(value => value.Index));
                float maximum = 0f, protectedMaximum = 0f; int protectedCount = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    if (!Finite(a[i]) || !Finite(b[i])) throw new InvalidOperationException("Non-finite current skin position.");
                    float distance = Vector3.Distance(a[i], b[i]); maximum = Mathf.Max(maximum, distance); BoneWeight w = weights[i];
                    bool affected = w.weight0 > 0 && selected.Contains(w.boneIndex0) || w.weight1 > 0 && selected.Contains(w.boneIndex1) ||
                        w.weight2 > 0 && selected.Contains(w.boneIndex2) || w.weight3 > 0 && selected.Contains(w.boneIndex3);
                    if (!affected) { protectedCount++; protectedMaximum = Mathf.Max(protectedMaximum, distance); }
                }
                return new JObject { ["vertices"] = a.Length, ["finite"] = true, ["maximumGestureRendererLocalDisplacement"] = maximum,
                    ["unaffectedWeightRows"] = protectedCount, ["maximumUnaffectedRendererLocalDisplacement"] = protectedMaximum,
                    ["basis"] = "Synchronous actual-current-pose BakeMesh pair in renderer local space, own palette pose vs untouched native palette; no vertices exported." };
            }
            finally
            {
                if (_renderer != null && ReferenceEquals(_renderer.sharedMesh, _mesh)) { _renderer.bones = _ownedBones; _mesh.bindposes = _ownedBind; }
                UnityEngine.Object.DestroyImmediate(posed); UnityEngine.Object.DestroyImmediate(native);
            }
        }
        internal Bounds CurrentFramingBounds()
        {
            if (!Ready) throw new InvalidOperationException("Private current skin unavailable for camera framing.");
            var mesh = new Mesh();
            try
            {
                ApplyCurrentPose(); _renderer.BakeMesh(mesh); Vector3[] vertices = mesh.vertices;
                if (vertices.Length != _mesh.vertexCount || vertices.Length == 0) throw new InvalidOperationException("Framing vertex count changed.");
                var bounds = new Bounds(_renderer.transform.TransformPoint(vertices[0]), Vector3.zero);
                foreach (Vector3 vertex in vertices)
                { Vector3 world = _renderer.transform.TransformPoint(vertex); if (!Finite(world)) throw new InvalidOperationException("Non-finite camera framing point."); bounds.Encapsulate(world); }
                return bounds; // Camera/culling aggregate only, not anatomy or reach.
            }
            finally { UnityEngine.Object.DestroyImmediate(mesh); }
        }
        private static string ColliderGeometry(Collider value)
        {
            var result = new JObject { ["instance"] = value.GetInstanceID(), ["type"] = value.GetType().FullName,
                ["enabled"] = value.enabled, ["trigger"] = value.isTrigger, ["active"] = value.gameObject.activeSelf,
                ["material"] = value.sharedMaterial == null ? 0 : value.sharedMaterial.GetInstanceID() };
            var capsule = value as CapsuleCollider; var sphere = value as SphereCollider; var box = value as BoxCollider; var mesh = value as MeshCollider; var controller = value as CharacterController;
            if (capsule != null) { result["center"] = Vector(capsule.center); result["radius"] = capsule.radius; result["height"] = capsule.height; result["direction"] = capsule.direction; }
            else if (sphere != null) { result["center"] = Vector(sphere.center); result["radius"] = sphere.radius; }
            else if (box != null) { result["center"] = Vector(box.center); result["size"] = Vector(box.size); }
            else if (mesh != null) { result["mesh"] = mesh.sharedMesh == null ? 0 : mesh.sharedMesh.GetInstanceID(); result["convex"] = mesh.convex; }
            else if (controller != null) { result["center"] = Vector(controller.center); result["radius"] = controller.radius; result["height"] = controller.height;
                result["slopeLimit"] = controller.slopeLimit; result["stepOffset"] = controller.stepOffset; result["skinWidth"] = controller.skinWidth; result["minimumMove"] = controller.minMoveDistance;
                result["detectCollisions"] = controller.detectCollisions; result["enableOverlapRecovery"] = controller.enableOverlapRecovery; }
            return result.ToString(Newtonsoft.Json.Formatting.None);
        }

        internal static SummonAttackGestureVisual For(UnitEntityData actor)
        {
            if (actor == null || actor.Destroyed || actor.View == null) return null;
            var visual = actor.View.GetComponent<SummonAttackGestureVisual>();
            return visual != null && visual.isActiveAndEnabled ? visual : null;
        }
        private bool PositiveOwner(out string sourceGuid)
        {
            sourceGuid = null;
            if (_actor == null || _actor.Destroyed || !_actor.IsInState || _actor.Descriptor.State.IsDead ||
                _view == null || !ReferenceEquals(_actor.View, _view) || _actor.Blueprint == null ||
                !ExpandedSummoningVisualVariantPatch.IsModuleActive || !Ready) return false;
            var markers = _actor.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => value.Blueprint == BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff).ToArray();
            if (markers.Length != 1 || !(markers[0].TimeLeft.TotalSeconds > 0d)) return false;
            var source = markers[0].MaybeContext?.SourceAbility; var caster = markers[0].MaybeContext?.MaybeCaster;
            if (source == null || caster == null || caster.Destroyed || !caster.IsInState || ReferenceEquals(caster, _actor) || (_caster != null && !ReferenceEquals(caster, _caster)) ||
                _view.AnimationManager == null || _view.AnimationManager.GetAction(Special(_key)) != null || !NativeActionsUnchanged()) return false;
            if (_key == SummonAttackGesturePolicy.Crab ? _actor.Blueprint.AssetGuid != CrabSummonAnimationBindingPolicy.UnitGuid ||
                _actor.Blueprint.name != CrabSummonAnimationBindingPolicy.UnitName || _actor.Blueprint.Prefab?.AssetId != CrabSummonAnimationBindingPolicy.PrefabGuid ||
                !CrabSummonAnimationBindingPolicy.SourceGuids.Contains(source.AssetGuid) :
                _actor.Blueprint.AssetGuid != SummonAttackGesturePolicy.BrachiosaurusUnit || _actor.Blueprint.name != GiantBodyVisualPolicy.BrachiosaurusBlueprintName ||
                _actor.Blueprint.Prefab?.AssetId != GiantBodyVisualPolicy.DonorPrefabGuid || !SummonAttackGesturePolicy.BrachiosaurusSources.Contains(source.AssetGuid)) return false;
            if (!ReferenceEquals(BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library, _actor.Blueprint.AssetGuid, "original gesture unit"), _actor.Blueprint) ||
                !ReferenceEquals(BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, source.AssetGuid, "original gesture source"), source)) return false;
            _caster = caster; sourceGuid = source.AssetGuid; return true;
        }
        private bool NativeActionsUnchanged()
        {
            if (SerializedSet == null || SerializedSet.FieldType != typeof(AnimationSet)) return false;
            if (_manager == null)
            {
                var donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                    _key == SummonAttackGesturePolicy.Crab ? CrabSummonAnimationBindingPolicy.SpiderGuid : GiantBodyVisualPolicy.DonorGuid,
                    "unchanged original attack donor");
                if (donor.Prefab == null || donor.Prefab.AssetId != _actor.Blueprint.Prefab?.AssetId) return false;
                var prefab = donor.Prefab.Load(false);
                var candidates = prefab == null ? new UnitAnimationManager[0] : prefab.GetComponentsInChildren<UnitAnimationManager>(true);
                if (candidates.Length != 1) return false;
                var set = SerializedSet.GetValue(candidates[0]) as AnimationSet;
                if (set == null || !ReferenceEquals(set, _view.AnimationManager.AnimationSet) || set.Actions == null || set.Actions.Count > 64) return false;
                _manager = _view.AnimationManager; _prefabManager = candidates[0]; _nativeSet = set;
                _nativeActionList = set.Actions; _nativeActions = set.Actions.ToArray();
            }
            return _manager != null && _prefabManager != null && ReferenceEquals(_view.AnimationManager, _manager) &&
                ReferenceEquals(_manager.AnimationSet, _nativeSet) && ReferenceEquals(SerializedSet.GetValue(_prefabManager), _nativeSet) &&
                ReferenceEquals(_nativeSet.Actions, _nativeActionList) && _nativeSet.Actions.SequenceEqual(_nativeActions);
        }
        private static UnitAnimationSpecialAttackType Special(string key)
        { return key == SummonAttackGesturePolicy.Crab ? UnitAnimationSpecialAttackType.Claw : UnitAnimationSpecialAttackType.Tail; }

        internal void ObserveSchedule(UnitAttack command, float delay)
        {
            try
            {
                string source;
                if (!PositiveOwner(out source) || command.GetType() != typeof(UnitAttack) || !ReferenceEquals(command.Executor, _actor) || !command.IsStarted || command.IsFinished ||
                    command.OverrideAction != null || command.Cutscene || command.IsIgnoreCooldown || command.IsCharge ||
                    command.Animation != null || command.PlannedAttack == null || command.PlannedAttack.AnimationHandle != null ||
                    _view.AnimationManager == null || _view.AnimationManager.GetAction(Special(_key)) != null) return;
                AttackHandInfo info = command.PlannedAttack; int index = command.GetAttackIndex();
                if (index < 0 || index >= command.AllAttacks.Count || !ReferenceEquals(command.AllAttacks[index], info) || info.Weapon == null) return;
                var limbs = new[] { _actor.Body.PrimaryHand.MaybeWeapon }.Concat(_actor.Body.AdditionalLimbs.Select(value => value.MaybeWeapon)).ToArray();
                int limb = Array.FindIndex(limbs, value => ReferenceEquals(value, info.Weapon));
                bool exactWeapons = _key == SummonAttackGesturePolicy.Crab ? limbs.Length == 2 && limbs.Distinct().Count() == 2 &&
                    limbs.All(value => value != null && value.Blueprint.AssetGuid == CrabSummonAnimationBindingPolicy.ClawGuid) :
                    limbs.Length == 1 && limb == 0 && limbs[0] != null && limbs[0].Blueprint.AssetGuid == SummonAttackGesturePolicy.TailWeapon;
                float deadline = (float)PretendAct.GetValue(command);
                var scope = new SummonAttackGestureScope { Key = _key, UnitGuid = _actor.Blueprint.AssetGuid, UnitName = _actor.Blueprint.name,
                    PrefabGuid = _actor.Blueprint.Prefab?.AssetId, SourceGuid = source, WeaponGuid = info.Weapon.Blueprint.AssetGuid,
                    ModuleActive = true, RegisteredUnit = true, RegisteredSource = true, PositiveCaster = true, LiveSummon = true,
                    ExactViewReceipt = Ready, ExactNativePalette = exactWeapons && limb >= 0, MissingNativeHandle = true,
                    NativeSchedule = !(bool)HasAnimation.GetValue(command) && !(bool)ActHandled.GetValue(command) && !command.IsOneFrameCommand &&
                        Mathf.Abs(delay - 1f) <= .00001f && Mathf.Abs(deadline - command.TimeSinceStart - delay) <= .0001f,
                    CurrentPoseEquality = ProveCurrentPose() };
                if (!SummonAttackGesturePolicy.Permits(scope)) { _refusal = "native-schedule-or-exact-owner-policy-refused"; return; }
                if (_schedules.Any(value => ReferenceEquals(value.Command, command) && value.Index == index))
                { foreach (Schedule previous in _schedules.Where(value => ReferenceEquals(value.Command, command))) previous.Cancelled = true; _refusal = "duplicate-native-schedule"; return; }
                if (_schedules.Count == 32) _schedules.RemoveAt(0);
                _schedules.Add(new Schedule { Serial = ++_serial, Index = index, Limb = limb, Command = command, Info = info,
                    Weapon = info.Weapon, Start = command.TimeSinceStart, Deadline = deadline, Delay = delay, Suppressed = _suppressed,
                    HasAnimationAtSchedule = (bool)HasAnimation.GetValue(command), HandledAtSchedule = (bool)ActHandled.GetValue(command),
                    ScheduleFrame = Time.frameCount, EligibleTicks = ReferenceEquals(_tickCommand, command) ? 1 : 0 });
            }
            catch (Exception error) { _refusal = "schedule-observer:" + error.GetType().Name + ":" + error.Message; }
        }
        internal void ObserveNativeAct(UnitAttack command)
        {
            try
            {
                var record = _schedules.LastOrDefault(value => ReferenceEquals(value.Command, command) && value.Index == command.GetAttackIndex());
                if (record == null) return;
                record.HasAnimationAtAct = (bool)HasAnimation.GetValue(command); record.HandledAtAct = (bool)ActHandled.GetValue(command);
                record.OneFrameAtAct = command.IsOneFrameCommand; record.NativeActTickDelta = Game.Instance.TimeController.DeltaTime;
                record.ActFrame = Time.frameCount; record.ActTickSerial = _tickSerial;
                if (record.ActTime >= 0 || !ReferenceEquals(record.Info, command.PlannedAttack) || command.Animation != null ||
                    !ReferenceEquals(record.Info.Weapon, record.Weapon) || !Ready ||
                    !ReferenceEquals(_tickCommand, command) || _tickFrame != Time.frameCount || _tickDelta != record.NativeActTickDelta ||
                    Mathf.Abs(command.TimeSinceStart - _tickBefore - _tickDelta) > .0002f ||
                    Mathf.Abs((float)PretendAct.GetValue(command) - record.Deadline) > .0001f ||
                    !SummonAttackGesturePolicy.ImmediateNativeAct(record.HasAnimationAtSchedule, record.HasAnimationAtAct, record.OneFrameAtAct,
                        record.HandledAtSchedule, record.HandledAtAct, record.Start, command.TimeSinceStart, record.NativeActTickDelta, record.EligibleTicks))
                { record.Cancelled = true; _refusal = "native-act-chain-mismatch"; return; }
                record.ActTime = command.TimeSinceStart; record.ActGame = Game.Instance.Player.GameTime.TotalSeconds;
                ApplyCurrentPose(); // Original art at the real native boundary; never calls OnAction/Act itself.
            }
            catch (Exception error) { _refusal = "act-observer:" + error.GetType().Name + ":" + error.Message; }
        }
        internal void ObserveTickBegin(UnitAttack command)
        {
            if (!Ready || !ReferenceEquals(command.Executor, _actor)) return;
            if (_tickCommand != null) { _refusal = "nested-native-tick"; foreach (Schedule s in _schedules) s.Cancelled = true; return; }
            _tickCommand = command; _tickSerial++; _tickFrame = Time.frameCount;
            _tickBefore = command.TimeSinceStart; _tickDelta = Game.Instance.TimeController.DeltaTime;
            foreach (Schedule s in _schedules.Where(value => ReferenceEquals(value.Command, command) && value.ActTime < 0 && !value.Cancelled &&
                value.Index == command.GetAttackIndex() && ReferenceEquals(value.Info, command.PlannedAttack))) s.EligibleTicks++;
        }
        internal void ObserveTickEnd(UnitAttack command)
        { if (ReferenceEquals(_tickCommand, command)) _tickCommand = null; }
        private float Elapsed(Schedule value)
        {
            if (value.ActTime >= 0) return SummonAttackGesturePolicy.ActualActCurveTime(true, Mathf.Max(0f, (float)(Game.Instance.Player.GameTime.TotalSeconds - value.ActGame)));
            if (value.Command.IsFinished) { value.Cancelled = true; return -1f; }
            return 0f; // Native HasAnimation=false may act on its first tick. No invented one-second windup.
        }
        private void ApplyCurrentPose()
        {
            if (!Ready) return;
            CopyCurrentPose();
            foreach (Pivot p in _pivots)
            {
                p.Turn.localRotation = Quaternion.identity;
                var record = _schedules.LastOrDefault(value => value.Limb == p.Limb && !value.Cancelled);
                if (record == null || record.Suppressed || _suppressed) continue;
                float elapsed = Elapsed(record); if (elapsed < 0 || elapsed >= 1.5f) continue;
                float yaw = _key == SummonAttackGesturePolicy.Brachiosaurus ? SummonAttackGesturePolicy.TailYaw(p.Joint, elapsed) : SummonAttackGesturePolicy.ChelaYaw(p.Joint, p.Limb == 0, elapsed);
                float pitch = _key == SummonAttackGesturePolicy.Brachiosaurus ? SummonAttackGesturePolicy.TailPitch(p.Joint, elapsed) : SummonAttackGesturePolicy.ChelaPitch(p.Joint, elapsed);
                Vector3 up = p.Turn.parent.InverseTransformDirection(_key == SummonAttackGesturePolicy.Crab ? _renderer.transform.forward : _renderer.transform.up).normalized;
                Vector3 right = p.Turn.parent.InverseTransformDirection(_renderer.transform.right).normalized;
                p.Turn.localRotation = Quaternion.AngleAxis(yaw, up) * Quaternion.AngleAxis(pitch, right);
                record.MaximumAngle = Mathf.Max(record.MaximumAngle, Quaternion.Angle(Quaternion.identity, p.Turn.localRotation));
            }
            foreach (Schedule record in _schedules.Where(value => !value.Cancelled && Elapsed(value) >= 0 && Elapsed(value) < 1.5f)) record.Samples++;
        }
        private void LateUpdate()
        {
            if (!_configured || _released) return;
            try
            {
                string source;
                if (!Ready) { _refusal = "foreign-renderer-material-palette-refresh"; Release(); return; }
                if (_schedules.Count > 0 && !PositiveOwner(out source))
                { foreach (Schedule value in _schedules) value.Cancelled = true; }
                ApplyCurrentPose();
            }
            catch (Exception error) { _refusal = "pose:" + error.GetType().Name + ":" + error.Message; Release(); }
        }
        internal IDisposable SuppressForReview()
        {
            if (!Ready || _suppression != null || _schedules.Any(value => !value.Cancelled && Elapsed(value) >= 0 && Elapsed(value) < 1.5f))
                throw new InvalidOperationException("An idle exact gesture receipt is required for baseline-only render suppression.");
            _suppressed = true; return _suppression = new Suppression { Owner = this };
        }
        internal JObject Describe()
        {
            return new JObject { ["key"] = _key, ["ready"] = Ready, ["released"] = _released, ["refusal"] = _refusal,
                ["currentPoseZeroProofs"] = _proofSamples, ["maximumZeroPoseRendererLocalError"] = _maximumZeroError,
                ["nativeActionReceiptCaptured"] = _manager != null, ["nativeActionListUnchanged"] = _manager != null && NativeActionsUnchanged(),
                ["sameFrameSourceAndColliderProofs"] = _sourceInvariantSamples, ["sourceOrColliderMutationFailures"] = _sourceInvariantFailures,
                ["nativeRootBoneUnchanged"] = _renderer != null && ReferenceEquals(_renderer.rootBone, _rootBone),
                ["releasedNativePaletteRestored"] = _released && _renderer != null && _nativeBones != null && _renderer.bones.SequenceEqual(_nativeBones),
                ["releasedPrivateBindRestored"] = _released && _mesh != null && _nativeBind != null && _mesh.bindposes.SequenceEqual(_nativeBind),
                ["releasedCullingBoundsRestored"] = _released && _renderer != null && _renderer.localBounds.Equals(_originalBounds),
                ["ownedTransformCount"] = _mirrors.Count(value => value.Copy != null) + _pivots.Sum(value => (value.Turn == null ? 0 : 1) + (value.Recenter == null ? 0 : 1)) + (_carrier == null ? 0 : 1),
                ["cullingOnlyBoundsExpanded"] = _configured && !_released,
                ["palettePoints"] = new JArray(_pivots.Select(value => new JObject { ["source"] = value.Source?.name,
                    ["sourceInstance"] = value.Source == null ? 0 : value.Source.GetInstanceID(), ["paletteIndex"] = value.Index,
                    ["ownedInstance"] = value.Turn == null ? 0 : value.Turn.GetInstanceID(), ["limb"] = value.Limb,
                    ["viewPoint"] = value.Turn == null || _view == null ? null : Vector(_view.transform.InverseTransformPoint(value.Turn.position)),
                    ["ownedAngle"] = value.Turn == null ? 0f : Quaternion.Angle(Quaternion.identity, value.Turn.localRotation) })),
                ["schedules"] = new JArray(_schedules.Select(value => new JObject { ["serial"] = value.Serial, ["index"] = value.Index,
                    ["commandIdentity"] = RuntimeHelpers.GetHashCode(value.Command), ["infoIdentity"] = RuntimeHelpers.GetHashCode(value.Info),
                    ["weaponIdentity"] = RuntimeHelpers.GetHashCode(value.Weapon), ["weapon"] = value.Weapon.Blueprint.AssetGuid,
                    ["start"] = value.Start, ["deadline"] = value.Deadline, ["nativeDelay"] = value.Delay, ["actualActCommandTime"] = value.ActTime,
                    ["hasAnimationAtSchedule"] = value.HasAnimationAtSchedule, ["hasAnimationAtAct"] = value.HasAnimationAtAct,
                    ["nativeHandledAtSchedule"] = value.HandledAtSchedule, ["nativeHandledAtAct"] = value.HandledAtAct,
                    ["nativeOneFrameAtAct"] = value.OneFrameAtAct, ["nativeActTickDelta"] = value.NativeActTickDelta,
                    ["eligibleNativeTicks"] = value.EligibleTicks, ["scheduleUnityFrame"] = value.ScheduleFrame,
                    ["actUnityFrame"] = value.ActFrame, ["actNativeTickSerial"] = value.ActTickSerial,
                    ["nativeBranch"] = "HasAnimation=false immediate native Tick; stored ScheduleAct(1) is not a minimum delay; no pre-act windup claimed",
                    ["actualActGameTime"] = value.ActGame, ["suppressed"] = value.Suppressed, ["cancelled"] = value.Cancelled,
                    ["samples"] = value.Samples, ["maximumOwnedAngle"] = value.MaximumAngle })) };
        }
        private static JObject Vector(Vector3 v) { return new JObject { ["x"] = v.x, ["y"] = v.y, ["z"] = v.z }; }
        private static bool Finite(Vector3 v) { return !(float.IsNaN(v.x) || float.IsInfinity(v.x) || float.IsNaN(v.y) || float.IsInfinity(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.z)); }

        internal void Release()
        {
            if (_released) return;
            _released = true; _configured = false;
            if (_suppression != null) _suppression.Dispose();
            // Restore only our exact still-installed slots. Foreign palette or
            // bind edits survive; never restore by name or distance heuristic.
            if (_renderer != null && _ownedBones != null)
            {
                Transform[] bones = _renderer.bones;
                for (int index = 0; index < bones.Length; index++)
                {
                    Pivot p = _pivots.FirstOrDefault(value => ReferenceEquals(bones[index], value.Turn));
                    if (p != null) bones[index] = p.Source;
                }
                _renderer.bones = bones;
                if (_mesh != null && ReferenceEquals(_renderer.sharedMesh, _mesh))
                {
                    Matrix4x4[] bind = _mesh.bindposes;
                    foreach (Pivot p in _pivots)
                        if (p.Index < bind.Length && bind[p.Index].Equals(_ownedBind[p.Index])) bind[p.Index] = _nativeBind[p.Index];
                    _mesh.bindposes = bind;
                }
                if (_renderer.localBounds.Equals(_ownedBounds)) _renderer.localBounds = _originalBounds;
            }
            if (_carrier != null) UnityEngine.Object.DestroyImmediate(_carrier.gameObject);
            _carrier = null;
        }
        private void OnDisable()
        {
            foreach (Schedule value in _schedules) value.Cancelled = true;
            if (!_released && Ready)
            { CopyCurrentPose(); foreach (Pivot p in _pivots) p.Turn.localRotation = Quaternion.identity; }
        }
        private void OnDestroy() { Release(); }
    }

    // The postfix has no ref parameters/result and never suppresses the
    // original. The native method already stored its own deadline.
    [HarmonyPatch(typeof(UnitCommand), "ScheduleAct", new[] { typeof(float) })]
    internal static class SummonAttackGestureSchedulePatch
    {
        private static void Postfix(UnitCommand __instance, float __0)
        {
            try
            {
                var attack = __instance as UnitAttack;
                var visual = attack == null ? null : SummonAttackGestureVisual.For(attack.Executor);
                if (visual != null) visual.ObserveSchedule(attack, __0);
            }
            catch (Exception) { /* Presentation admission cannot break a native command. */ }
        }
    }
    [HarmonyPatch(typeof(UnitAttack), "OnAction")]
    internal static class SummonAttackGestureActPatch
    {
        private static void Prefix(UnitAttack __instance)
        {
            try { var visual = SummonAttackGestureVisual.For(__instance.Executor); if (visual != null) visual.ObserveNativeAct(__instance); }
            catch (Exception) { /* Native OnAction always runs unchanged. */ }
        }
    }
    [HarmonyPatch(typeof(UnitCommand), "Tick")]
    internal static class SummonAttackGestureTickPatch
    {
        private static void Prefix(UnitCommand __instance)
        { try { var attack = __instance as UnitAttack; if (attack != null) SummonAttackGestureVisual.For(attack.Executor)?.ObserveTickBegin(attack); } catch (Exception) { } }
        private static void Postfix(UnitCommand __instance)
        { try { var attack = __instance as UnitAttack; if (attack != null) SummonAttackGestureVisual.For(attack.Executor)?.ObserveTickEnd(attack); } catch (Exception) { } }
    }
}
