using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Explicit experimental fixture entry only. No production attach hook,
    // Harmony registration, automatic calibration or accepted height is here.
    // Native WeaponSnap has default execution order; this matches the existing
    // qualified visual attachment order after its LateUpdate.
    [DefaultExecutionOrder(10010)]
    internal sealed class Cleanup2GiantRenderScalePrototype : MonoBehaviour, IDisposable
    {
        internal sealed class ProposedClothDistances
        {
            internal readonly float MaxDistanceMultiplier;
            internal readonly float CollisionSphereDistanceMultiplier;
            internal readonly string Proposal;

            internal ProposedClothDistances(float maximum, float collisionSphere, string proposal)
            {
                if (!Finite(maximum) || maximum <= 0f || !Finite(collisionSphere) || collisionSphere <= 0f ||
                    string.IsNullOrWhiteSpace(proposal))
                    throw new ArgumentException("Explicit finite positive experimental distance multipliers and proposal identity required.");
                MaxDistanceMultiplier = maximum;
                CollisionSphereDistanceMultiplier = collisionSphere;
                Proposal = proposal;
            }
        }

        private sealed class Skin
        {
            internal SkinnedMeshRenderer Source, Proxy;
            internal Mesh Mesh;
            internal Material[] Materials;
            internal Transform[] Bones, AppliedBones;
            internal Transform RootBone, AppliedRoot;
            internal Bounds Bounds, AppliedBounds;
            internal bool Enabled, Changed;
            internal ClothConfiguration Cloth;
        }

        private sealed class StaticMesh
        {
            internal MeshRenderer Source, Proxy;
            internal MeshFilter Filter;
            internal Mesh Mesh;
            internal Material[] Materials;
            internal bool Enabled, Changed;
        }

        private sealed class NativeCollider
        {
            internal Collider Source;
            internal string Geometry;
            internal Matrix4x4 IntakeWorld, BeforeLastWrite, AfterLastWrite;
        }

        // These are the exact non-obsolete writable fields on the installed
        // UnityEngine.ClothModule surface. The deprecated selfCollision and
        // solverFrequency aliases are not independent supported settings.
        private sealed class ClothConfiguration
        {
            private static readonly string[] ScalarNames = {
                "sleepThreshold", "bendingStiffness", "stretchingStiffness", "damping",
                "externalAcceleration", "randomAcceleration", "useGravity", "enabled", "friction",
                "collisionMassScale", "enableContinuousCollision", "useVirtualParticles",
                "worldVelocityScale", "worldAccelerationScale", "clothSolverFrequency", "useTethers",
                "stiffnessFrequency", "selfCollisionDistance", "selfCollisionStiffness"
            };
            internal readonly Cloth Source;
            private readonly Dictionary<PropertyInfo, object> _scalars = new Dictionary<PropertyInfo, object>();
            internal readonly ClothSkinningCoefficient[] Coefficients;
            internal readonly CapsuleCollider[] Capsules;
            private readonly ClothSphereColliderPair[] _spheres;
            private readonly List<uint> _virtualIndices = new List<uint>();
            private readonly List<Vector3> _virtualWeights = new List<Vector3>();
            private readonly List<uint> _selfIndices = new List<uint>();

            internal ClothConfiguration(Cloth source)
            {
                Source = source;
                foreach (string name in ScalarNames)
                {
                    PropertyInfo property = typeof(Cloth).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                    if (property == null || !property.CanRead || !property.CanWrite)
                        throw new NotSupportedException("Installed Cloth field unavailable: " + name);
                    object value = property.GetValue(source, null);
                    if ((value is float && !Finite((float)value)) || (value is Vector3 && !Finite((Vector3)value)))
                        throw new InvalidOperationException("Non-finite native Cloth configuration: " + name);
                    _scalars.Add(property, value);
                }
                Coefficients = source.coefficients.ToArray();
                Capsules = source.capsuleColliders.ToArray();
                _spheres = source.sphereColliders.ToArray();
                if (!source.enabled || Coefficients.Length == 0 || Coefficients.Length > 100000 || Capsules.Length > 32)
                    throw new NotSupportedException("Enabled bounded native Cloth input required.");
                if (_spheres.Length != 0)
                    throw new NotSupportedException("Sphere-pair proxy collision is outside this capsule-only experiment.");
                foreach (var value in Coefficients)
                    if (!Finite(value.maxDistance) || !Finite(value.collisionSphereDistance) || value.maxDistance < 0f || value.collisionSphereDistance < 0f)
                        throw new NotSupportedException("Negative, sentinel or non-finite Cloth coefficient requires separate review.");
                source.GetVirtualParticleIndices(_virtualIndices);
                source.GetVirtualParticleWeights(_virtualWeights);
                source.GetSelfAndInterCollisionIndices(_selfIndices);
                if (_virtualIndices.Count > 300000 || _virtualWeights.Count > 100000 || _selfIndices.Count > 100000 ||
                    _virtualWeights.Any(value => !Finite(value)))
                    throw new NotSupportedException("Unbounded or non-finite native Cloth particle configuration.");
            }

            internal bool Unchanged()
            {
                if (Source == null || _scalars.Any(pair => !Equals(pair.Value, pair.Key.GetValue(Source, null))) ||
                    !SameCoefficients(Source.coefficients, Coefficients) || !Same(Source.capsuleColliders, Capsules) ||
                    Source.sphereColliders.Length != _spheres.Length) return false;
                var indices = new List<uint>(); var weights = new List<Vector3>(); var self = new List<uint>();
                Source.GetVirtualParticleIndices(indices); Source.GetVirtualParticleWeights(weights);
                Source.GetSelfAndInterCollisionIndices(self);
                return indices.SequenceEqual(_virtualIndices) && weights.SequenceEqual(_virtualWeights) && self.SequenceEqual(_selfIndices);
            }

            internal void CopyTo(Cloth target, CapsuleCollider[] capsules, ProposedClothDistances distances)
            {
                foreach (var pair in _scalars)
                    if (pair.Key.Name != "enabled") pair.Key.SetValue(target, pair.Value, null);
                var coefficients = Coefficients.ToArray();
                for (int i = 0; i < coefficients.Length; i++)
                {
                    coefficients[i].maxDistance *= distances.MaxDistanceMultiplier;
                    coefficients[i].collisionSphereDistance *= distances.CollisionSphereDistanceMultiplier;
                    if (!Finite(coefficients[i].maxDistance) || !Finite(coefficients[i].collisionSphereDistance))
                        throw new InvalidOperationException("Experimental Cloth distance overflow.");
                }
                target.coefficients = coefficients;
                target.capsuleColliders = capsules;
                target.sphereColliders = new ClothSphereColliderPair[0];
                target.SetVirtualParticleIndices(new List<uint>(_virtualIndices));
                target.SetVirtualParticleWeights(new List<Vector3>(_virtualWeights));
                target.SetSelfAndInterCollisionIndices(new List<uint>(_selfIndices));
                target.enabled = true; // Owned solver only, still under an inactive owned root.
                if (!SameCoefficients(target.coefficients, coefficients) || !Same(target.capsuleColliders, capsules) ||
                    target.sphereColliders.Length != 0 || _scalars.Any(pair => pair.Key.Name != "enabled" &&
                    !Equals(pair.Value, pair.Key.GetValue(target, null))))
                    throw new NotSupportedException("Owned Cloth configuration did not round-trip exactly.");
                var indices = new List<uint>(); var weights = new List<Vector3>(); var self = new List<uint>();
                target.GetVirtualParticleIndices(indices); target.GetVirtualParticleWeights(weights);
                target.GetSelfAndInterCollisionIndices(self);
                if (!indices.SequenceEqual(_virtualIndices) || !weights.SequenceEqual(_virtualWeights) || !self.SequenceEqual(_selfIndices))
                    throw new NotSupportedException("Owned Cloth particle configuration did not round-trip exactly.");
            }

            internal JObject Describe()
            {
                var values = new JObject();
                foreach (var pair in _scalars)
                    values[pair.Key.Name] = pair.Value is Vector3 ? Vector((Vector3)pair.Value) : JToken.FromObject(pair.Value);
                return new JObject { ["source"] = Source == null ? 0 : Source.GetInstanceID(), ["fields"] = values,
                    ["coefficientCount"] = Coefficients.Length, ["pinnedCount"] = Coefficients.Count(value => value.maxDistance == 0f),
                    ["virtualIndexCount"] = _virtualIndices.Count, ["virtualWeightCount"] = _virtualWeights.Count,
                    ["selfInterCollisionIndexCount"] = _selfIndices.Count, ["capsuleCount"] = Capsules.Length,
                    ["spherePairCount"] = _spheres.Length };
            }
        }

        private UnitEntityData _actor;
        private UnitEntityView _view;
        private Transform _sourceRoot;
        private Transform[] _sourceTransforms;
        private Transform[] _sourceParents;
        private Renderer[] _sourceRenderers;
        private readonly List<Skin> _skins = new List<Skin>();
        private readonly List<StaticMesh> _statics = new List<StaticMesh>();
        private readonly List<NativeCollider> _nativeColliders = new List<NativeCollider>();
        private readonly Dictionary<Transform, Transform> _mirrors = new Dictionary<Transform, Transform>();
        private readonly Dictionary<CapsuleCollider, CapsuleCollider> _capsules = new Dictionary<CapsuleCollider, CapsuleCollider>();
        private readonly List<Cloth> _ownedCloths = new List<Cloth>();
        private ProposedClothDistances _distances;
        private float _factor;
        private string _runId, _status;
        private bool _committed, _disposed;
        private int _syncCount, _lastSyncFrame;
        private bool _sourceTransformsUnchanged = true, _colliderGeometryUnchanged = true, _colliderMatricesUnchanged = true;
        private JObject _lastReport;
        private readonly List<string> _restoreIssues = new List<string>();

        internal IEnumerable<Cloth> OwnedCloths { get { return _ownedCloths.ToArray(); } }

        internal static Cleanup2GiantRenderScalePrototype Take(RuntimeTestRequest request, UnitEntityData actor,
            UnitEntityData requestCaster, IList<UnitEntityData> exactPositiveOwnedActors, float uniformFactor,
            ProposedClothDistances proposedDistances)
        {
            if (request == null || !request.Enabled || !request.ExitAfterCompletion ||
                request.Scenario != "disposable-expanded-summoning-cleanup2-native-review" ||
                request.Parameters == null || request.Parameters.Count != 1 ||
                (string)request.Parameters["reviewPhase"] != "scale-prototype" || string.IsNullOrWhiteSpace(request.RunId))
                throw new InvalidOperationException("Exact automatic-exit Cleanup 2 scale-prototype request required.");
            if (actor == null || requestCaster == null || ReferenceEquals(actor, requestCaster) || actor.Destroyed ||
                actor.View == null || exactPositiveOwnedActors == null ||
                exactPositiveOwnedActors.Count(value => ReferenceEquals(value, actor)) != 1)
                throw new InvalidOperationException("Exactly one positive request-owned live summon reference required.");
            var provenance = new List<Buff>();
            foreach (Buff buff in actor.Descriptor.Buffs)
                if (buff.Blueprint == BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) provenance.Add(buff);
            if (provenance.Count != 1 || provenance[0].MaybeContext == null ||
                !ReferenceEquals(provenance[0].MaybeContext.MaybeCaster, requestCaster) || provenance[0].MaybeContext.SourceAbility == null)
                throw new InvalidOperationException("Exact native summoned-buff provenance from the request caster required.");
            if (!Finite(uniformFactor) || uniformFactor < .1f || uniformFactor > 10f || proposedDistances == null)
                throw new ArgumentException("Finite bounded uniform visual factor and explicit experimental distance proposal required.");
            if (actor.View.GetComponentInChildren<Cleanup2GiantRenderScalePrototype>(true) != null)
                throw new InvalidOperationException("A scale prototype already owns this actor view.");

            // Capture the entire native view before creating or changing anything.
            // The temporary managed plan owns no Unity object or native state.
            var plan = new Plan(actor, uniformFactor, proposedDistances);
            GameObject root = null;
            Cleanup2GiantRenderScalePrototype owner = null;
            try
            {
                root = new GameObject("KMG_C2_RenderScale_" + actor.UniqueId);
                root.SetActive(false);
                root.transform.SetParent(actor.View.transform, false);
                root.transform.localScale = Vector3.one * uniformFactor;
                owner = root.AddComponent<Cleanup2GiantRenderScalePrototype>();
                owner._actor = actor; owner._view = actor.View; owner._sourceRoot = actor.View.transform;
                owner._runId = request.RunId; owner._factor = uniformFactor; owner._distances = proposedDistances;
                owner._sourceTransforms = plan.Transforms; owner._sourceParents = plan.Parents;
                owner._sourceRenderers = plan.Renderers;
                owner._skins.AddRange(plan.Skins); owner._statics.AddRange(plan.Statics);
                owner._nativeColliders.AddRange(plan.Colliders);
                owner._mirrors.Add(owner._sourceRoot, root.transform);
                foreach (Transform source in plan.Transforms) owner.Mirror(source);
                owner.CopyMirrorPose();
                owner.CreateOwnedRenderers();
                owner.RequireSourceIdentity(false);
                owner.Commit();
                root.SetActive(true);
                owner._status = "EXPERIMENT_ACTIVE_COLLISION_AND_DISTANCE_CALIBRATION_UNKNOWN";
                owner.Synchronize();
                return owner;
            }
            catch
            {
                if (owner != null) owner.Dispose();
                else if (root != null) UnityEngine.Object.Destroy(root);
                throw;
            }
        }

        private sealed class Plan
        {
            internal readonly Transform[] Transforms, Parents;
            internal readonly Renderer[] Renderers;
            internal readonly List<Skin> Skins = new List<Skin>();
            internal readonly List<StaticMesh> Statics = new List<StaticMesh>();
            internal readonly List<NativeCollider> Colliders = new List<NativeCollider>();

            internal Plan(UnitEntityData actor, float factor, ProposedClothDistances distances)
            {
                Transform root = actor.View.transform;
                Transforms = root.GetComponentsInChildren<Transform>(true);
                Parents = Transforms.Select(value => value.parent).ToArray();
                Renderers = root.GetComponentsInChildren<Renderer>(true);
                if (Transforms.Length == 0 || Transforms.Length > 2048 || Renderers.Length == 0 || Renderers.Length > 128)
                    throw new NotSupportedException("Empty or unbounded native view.");
                foreach (Transform value in Transforms)
                    if (!Finite(value.localPosition) || !Finite(value.localScale) || !Finite(value.localRotation) ||
                        !Finite(value.localToWorldMatrix) || value.localScale.x <= 0f || value.localScale.y <= 0f || value.localScale.z <= 0f)
                        throw new NotSupportedException("Non-finite, negative or zero source transform scale.");
                foreach (Renderer renderer in Renderers)
                {
                    if (renderer.sharedMaterials.Length > 32) throw new NotSupportedException("Unbounded renderer materials.");
                    var skin = renderer as SkinnedMeshRenderer;
                    var meshRenderer = renderer as MeshRenderer;
                    if (skin != null)
                    {
                        Transform[] bones = skin.bones;
                        if (skin.sharedMesh == null || bones == null || bones.Length == 0 || bones.Length > 512 || skin.rootBone == null ||
                            bones.Any(value => value == null || !Transforms.Contains(value)) || !Transforms.Contains(skin.rootBone))
                            throw new NotSupportedException("Missing or foreign skinned mesh/palette/root.");
                        var cloth = skin.GetComponent<Cloth>();
                        var state = new Skin { Source = skin, Mesh = skin.sharedMesh, Materials = skin.sharedMaterials,
                            Bones = bones.ToArray(), RootBone = skin.rootBone, Bounds = skin.localBounds, Enabled = skin.enabled,
                            Cloth = cloth == null ? null : new ClothConfiguration(cloth) };
                        if (cloth != null && !skin.enabled) throw new NotSupportedException("Suppressed native Cloth is not an established live input.");
                        if (state.Cloth != null)
                            foreach (CapsuleCollider capsule in state.Cloth.Capsules)
                                if (capsule == null || !Transforms.Contains(capsule.transform))
                                    throw new NotSupportedException("Missing or foreign native Cloth capsule.");
                        // Check proposed distance arithmetic before any Unity mutation.
                        if (state.Cloth != null && state.Cloth.Coefficients.Any(value =>
                            !Finite(value.maxDistance * distances.MaxDistanceMultiplier) ||
                            !Finite(value.collisionSphereDistance * distances.CollisionSphereDistanceMultiplier)))
                            throw new ArgumentException("Experimental Cloth distance overflow.");
                        Vector3 scaled = root.InverseTransformPoint(skin.transform.TransformPoint(state.Bounds.center)) * factor;
                        if (!Finite(scaled) || !Finite(state.Bounds.size * factor)) throw new ArgumentException("Visual bounds overflow.");
                        Skins.Add(state);
                    }
                    else if (meshRenderer != null)
                    {
                        var filter = renderer.GetComponent<MeshFilter>();
                        if (filter == null || filter.sharedMesh == null) throw new NotSupportedException("Static renderer lacks an exact mesh filter.");
                        Statics.Add(new StaticMesh { Source = meshRenderer, Filter = filter, Mesh = filter.sharedMesh,
                            Materials = renderer.sharedMaterials, Enabled = renderer.enabled });
                    }
                    else if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                        throw new NotSupportedException("Active unsupported renderer: " + renderer.GetType().FullName);
                }
                Cloth[] cloths = root.GetComponentsInChildren<Cloth>(true);
                if (cloths.Length != Skins.Count(value => value.Cloth != null))
                    throw new NotSupportedException("Every native Cloth must have one accounted skinned renderer.");
                Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
                if (colliders.Length > 128) throw new NotSupportedException("Unbounded native collider census.");
                foreach (Collider collider in colliders)
                    Colliders.Add(new NativeCollider { Source = collider, Geometry = ColliderGeometry(collider),
                        IntakeWorld = collider.transform.localToWorldMatrix });
            }
        }

        private Transform Mirror(Transform source)
        {
            Transform found;
            if (_mirrors.TryGetValue(source, out found)) return found;
            if (source == null || source.parent == null || !_sourceTransforms.Contains(source))
                throw new InvalidOperationException("Mirror source escaped the exact preflight hierarchy.");
            var node = new GameObject("KMG_C2_Mirror_" + Array.IndexOf(_sourceTransforms, source));
            node.SetActive(false);
            node.transform.SetParent(Mirror(source.parent), false);
            _mirrors.Add(source, node.transform);
            // These nodes contain Transform only unless this helper adds its
            // own disabled capsule or an owned renderer below them.
            node.SetActive(source.gameObject.activeSelf);
            return node.transform;
        }

        private void CopyMirrorPose()
        {
            foreach (Transform source in _sourceTransforms)
            {
                if (source == _sourceRoot) continue;
                Transform mirror = _mirrors[source];
                mirror.localPosition = source.localPosition;
                mirror.localRotation = source.localRotation;
                mirror.localScale = source.localScale;
                if (mirror.gameObject.activeSelf != source.gameObject.activeSelf)
                    mirror.gameObject.SetActive(source.gameObject.activeSelf);
            }
        }

        private CapsuleCollider CapsuleProxy(CapsuleCollider source)
        {
            CapsuleCollider existing;
            if (_capsules.TryGetValue(source, out existing)) return existing;
            var node = new GameObject("KMG_C2_DisabledClothCapsule_" + _capsules.Count);
            node.SetActive(false);
            node.transform.SetParent(Mirror(source.transform), false);
            var capsule = node.AddComponent<CapsuleCollider>();
            capsule.enabled = false; // Never activated as a gameplay physics shape.
            capsule.center = source.center; capsule.radius = source.radius;
            capsule.height = source.height; capsule.direction = source.direction;
            capsule.isTrigger = source.isTrigger;
            _capsules.Add(source, capsule);
            node.SetActive(true); // Parent root remains inactive until the complete commit.
            return capsule;
        }

        private void CreateOwnedRenderers()
        {
            foreach (Skin skin in _skins)
            {
                skin.AppliedBones = skin.Bones.Select(Mirror).ToArray();
                skin.AppliedRoot = Mirror(skin.RootBone);
                if (skin.Cloth == null) continue;
                var node = new GameObject("KMG_C2_ClothRenderer_" + _ownedCloths.Count);
                node.transform.SetParent(Mirror(skin.Source.transform), false);
                node.layer = skin.Source.gameObject.layer;
                var proxy = node.AddComponent<SkinnedMeshRenderer>();
                skin.Proxy = proxy;
                proxy.sharedMesh = skin.Mesh; proxy.sharedMaterials = skin.Materials;
                proxy.bones = skin.AppliedBones; proxy.rootBone = skin.AppliedRoot;
                proxy.localBounds = skin.Bounds;
                proxy.quality = skin.Source.quality; proxy.updateWhenOffscreen = skin.Source.updateWhenOffscreen;
                proxy.skinnedMotionVectors = skin.Source.skinnedMotionVectors;
                CopyRendererSettings(skin.Source, proxy);
                var cloth = node.AddComponent<Cloth>();
                _ownedCloths.Add(cloth);
                skin.Cloth.CopyTo(cloth, skin.Cloth.Capsules.Select(CapsuleProxy).ToArray(), _distances);
            }
            foreach (StaticMesh mesh in _statics)
            {
                var node = new GameObject("KMG_C2_StaticRenderer_" + mesh.Source.GetInstanceID());
                node.transform.SetParent(Mirror(mesh.Source.transform), false);
                node.layer = mesh.Source.gameObject.layer;
                node.AddComponent<MeshFilter>().sharedMesh = mesh.Mesh;
                mesh.Proxy = node.AddComponent<MeshRenderer>();
                mesh.Proxy.sharedMaterials = mesh.Materials;
                CopyRendererSettings(mesh.Source, mesh.Proxy);
            }
        }

        private static void CopyRendererSettings(Renderer source, Renderer target)
        {
            target.enabled = source.enabled;
            target.shadowCastingMode = source.shadowCastingMode; target.receiveShadows = source.receiveShadows;
            target.lightProbeUsage = source.lightProbeUsage; target.reflectionProbeUsage = source.reflectionProbeUsage;
            target.probeAnchor = source.probeAnchor; target.sortingLayerID = source.sortingLayerID;
            target.sortingOrder = source.sortingOrder; target.motionVectorGenerationMode = source.motionVectorGenerationMode;
            target.allowOcclusionWhenDynamic = source.allowOcclusionWhenDynamic;
            CopyPropertyBlocks(source, target);
        }

        private static void CopyPropertyBlocks(Renderer source, Renderer target)
        {
            var block = new MaterialPropertyBlock();
            source.GetPropertyBlock(block); target.SetPropertyBlock(block);
            for (int i = 0; i < source.sharedMaterials.Length; i++)
            {
                block.Clear(); source.GetPropertyBlock(block, i); target.SetPropertyBlock(block, i);
            }
        }

        private void Commit()
        {
            Matrix4x4[] matrices = _nativeColliders.Select(value => value.Source.transform.localToWorldMatrix).ToArray();
            Matrix4x4[] transforms = _sourceTransforms.Select(value => value.localToWorldMatrix).ToArray();
            foreach (Skin skin in _skins)
            {
                skin.Changed = true;
                if (skin.Cloth != null) skin.Source.enabled = false;
                else
                {
                    skin.Source.bones = skin.AppliedBones; skin.Source.rootBone = skin.AppliedRoot;
                    Vector3 anchor = skin.Source.transform.InverseTransformPoint(_sourceRoot.position);
                    skin.AppliedBounds = new Bounds(anchor + (skin.Bounds.center - anchor) * _factor, skin.Bounds.size * _factor);
                    skin.Source.localBounds = skin.AppliedBounds;
                }
            }
            foreach (StaticMesh mesh in _statics) { mesh.Changed = true; mesh.Source.enabled = false; }
            _committed = true;
            VerifyNoNativeWrite(transforms, matrices);
        }

        private void RequireSourceIdentity(bool committed)
        {
            if (_actor == null || _actor.Destroyed || _view == null || !ReferenceEquals(_actor.View, _view) || _sourceRoot == null)
                throw new InvalidOperationException("Actor/view reconstruction refuses continued prototype mutation.");
            if (transform.parent != _sourceRoot || !transform.localPosition.Equals(Vector3.zero) ||
                !transform.localRotation.Equals(Quaternion.identity) || !transform.localScale.Equals(Vector3.one * _factor))
                throw new InvalidOperationException("Owned proxy root pose/scale changed; no foreign correction is overwritten.");
            Transform[] transforms = _sourceRoot.GetComponentsInChildren<Transform>(true).Where(value => !Owned(value)).ToArray();
            Renderer[] renderers = _sourceRoot.GetComponentsInChildren<Renderer>(true).Where(value => !Owned(value.transform)).ToArray();
            if (!Same(transforms, _sourceTransforms) || !Same(renderers, _sourceRenderers) ||
                _sourceTransforms.Where((value, i) => value.parent != _sourceParents[i]).Any())
                throw new InvalidOperationException("Native hierarchy/model refreshed; no replacement model is claimed.");
            foreach (Skin skin in _skins)
            {
                Transform[] bones = committed && skin.Cloth == null ? skin.AppliedBones : skin.Bones;
                Transform root = committed && skin.Cloth == null ? skin.AppliedRoot : skin.RootBone;
                bool expectedEnabled = committed && skin.Cloth != null ? false : skin.Enabled;
                if (skin.Source == null || skin.Source.sharedMesh != skin.Mesh || !Same(skin.Source.sharedMaterials, skin.Materials) ||
                    !Same(skin.Source.bones, bones) || skin.Source.rootBone != root || skin.Source.enabled != expectedEnabled ||
                    (skin.Cloth != null && !skin.Cloth.Unchanged()))
                    throw new InvalidOperationException("Foreign native skin/material/palette/Cloth edit; prototype refuses continuation.");
            }
            foreach (StaticMesh mesh in _statics)
                if (mesh.Source == null || mesh.Filter == null || mesh.Filter.sharedMesh != mesh.Mesh ||
                    !Same(mesh.Source.sharedMaterials, mesh.Materials) || mesh.Source.enabled != (committed ? false : mesh.Enabled))
                    throw new InvalidOperationException("Foreign native static renderer edit; prototype refuses continuation.");
            Collider[] colliders = _sourceRoot.GetComponentsInChildren<Collider>(true).Where(value => !Owned(value.transform)).ToArray();
            if (!Same(colliders, _nativeColliders.Select(value => value.Source).ToArray()))
                throw new InvalidOperationException("Native collider census changed.");
            foreach (NativeCollider collider in _nativeColliders)
                if (ColliderGeometry(collider.Source) != collider.Geometry)
                { _colliderGeometryUnchanged = false; throw new InvalidOperationException("Native collider geometry/configuration changed."); }
            if (_capsules.Any(pair => pair.Value == null || pair.Value.enabled ||
                !pair.Value.center.Equals(pair.Key.center) || !pair.Value.radius.Equals(pair.Key.radius) ||
                !pair.Value.height.Equals(pair.Key.height) || pair.Value.direction != pair.Key.direction))
                throw new InvalidOperationException("Owned visual capsule was enabled, replaced or edited; no enabled fallback is permitted.");
            foreach (Skin skin in _skins.Where(value => value.Cloth != null))
                if (skin.Proxy == null || skin.Proxy.sharedMesh != skin.Mesh || !Same(skin.Proxy.sharedMaterials, skin.Materials) ||
                    !Same(skin.Proxy.bones, skin.AppliedBones) || skin.Proxy.rootBone != skin.AppliedRoot)
                    throw new InvalidOperationException("Owned cloth renderer palette/mesh/material changed.");
            foreach (Cloth cloth in _ownedCloths)
                if (cloth == null || !cloth.enabled || cloth.vertices.Any(value => !Finite(value)))
                    throw new InvalidOperationException("Owned solver is missing, disabled or non-finite.");
            foreach (StaticMesh mesh in _statics)
                if (mesh.Proxy == null || !Same(mesh.Proxy.sharedMaterials, mesh.Materials) ||
                    mesh.Proxy.GetComponent<MeshFilter>().sharedMesh != mesh.Mesh)
                    throw new InvalidOperationException("Owned static renderer mesh/material changed.");
            foreach (Renderer renderer in _sourceRenderers)
                if (!(renderer is SkinnedMeshRenderer) && !(renderer is MeshRenderer) && renderer.enabled && renderer.gameObject.activeInHierarchy)
                    throw new NotSupportedException("Previously inactive unsupported renderer became active.");
        }

        private bool Owned(Transform value)
        { return value == transform || value.IsChildOf(transform); }

        // Public-to-this-assembly for a bounded observer to request same-frame
        // evidence after explicitly advancing the qualified native clock.
        internal void Synchronize()
        {
            if (_disposed || !_committed) return;
            RequireSourceIdentity(true);
            Matrix4x4[] before = _sourceTransforms.Select(value => value.localToWorldMatrix).ToArray();
            Matrix4x4[] colliders = _nativeColliders.Select(value => value.Source.transform.localToWorldMatrix).ToArray();
            CopyMirrorPose();
            foreach (Skin skin in _skins.Where(value => value.Proxy != null)) CopyPropertyBlocks(skin.Source, skin.Proxy);
            foreach (StaticMesh mesh in _statics) CopyPropertyBlocks(mesh.Source, mesh.Proxy);
            VerifyNoNativeWrite(before, colliders);
            _lastSyncFrame = Time.frameCount; _syncCount++;
        }

        private void VerifyNoNativeWrite(Matrix4x4[] source, Matrix4x4[] colliders)
        {
            for (int i = 0; i < _nativeColliders.Count; i++)
            {
                _nativeColliders[i].BeforeLastWrite = colliders[i];
                _nativeColliders[i].AfterLastWrite = _nativeColliders[i].Source.transform.localToWorldMatrix;
            }
            _sourceTransformsUnchanged &= source.Select((value, i) => SameMatrix(value, _sourceTransforms[i].localToWorldMatrix)).All(value => value);
            _colliderMatricesUnchanged &= colliders.Select((value, i) => SameMatrix(value, _nativeColliders[i].Source.transform.localToWorldMatrix)).All(value => value);
            _colliderGeometryUnchanged &= _nativeColliders.All(value => ColliderGeometry(value.Source) == value.Geometry);
            if (!_sourceTransformsUnchanged || !_colliderMatricesUnchanged || !_colliderGeometryUnchanged)
                throw new InvalidOperationException("Same-frame native transform/collider invariance failed.");
        }

        private void LateUpdate()
        {
            if (_disposed || !_committed) return;
            try { Synchronize(); }
            catch (Exception error)
            {
                _status = "REFUSED:" + error.GetType().Name + ":" + error.Message;
                Dispose();
            }
        }

        internal JObject Describe()
        {
            if (_disposed) return _lastReport == null ? new JObject { ["status"] = _status } : (JObject)_lastReport.DeepClone();
            return Report();
        }

        private JObject Report()
        {
            return new JObject { ["runId"] = _runId, ["actor"] = _actor == null ? null : _actor.UniqueId,
                ["status"] = _status, ["uniformVisualFactor"] = _factor, ["syncCount"] = _syncCount,
                ["lastSyncFrame"] = _lastSyncFrame, ["executionOrder"] = 10010,
                ["nativeSourceTransformsSameFrameExact"] = _sourceTransformsUnchanged,
                ["nativeColliderMatricesSameFrameExact"] = _colliderMatricesUnchanged,
                ["nativeColliderGeometryExact"] = _colliderGeometryUnchanged,
                ["nativeColliders"] = new JArray(_nativeColliders.Select(value => new JObject {
                    ["instance"] = value.Source == null ? 0 : value.Source.GetInstanceID(), ["geometryAtIntake"] = value.Geometry,
                    ["currentGeometry"] = value.Source == null ? null : ColliderGeometry(value.Source),
                    ["intakeWorldMatrix"] = Matrix(value.IntakeWorld),
                    ["beforeLastPrototypeWriteWorldMatrix"] = Matrix(value.BeforeLastWrite),
                    ["afterLastPrototypeWriteWorldMatrix"] = Matrix(value.AfterLastWrite),
                    ["worldMatrix"] = value.Source == null ? null : Matrix(value.Source.transform.localToWorldMatrix) })),
                ["ownedDisabledCapsules"] = new JArray(_capsules.Select(pair => new JObject {
                    ["source"] = pair.Key == null ? 0 : pair.Key.GetInstanceID(),
                    ["owned"] = pair.Value == null ? 0 : pair.Value.GetInstanceID(),
                    ["enabled"] = pair.Value != null && pair.Value.enabled,
                    ["geometry"] = pair.Value == null ? null : ColliderGeometry(pair.Value),
                    ["worldMatrix"] = pair.Value == null ? null : Matrix(pair.Value.transform.localToWorldMatrix) })),
                ["nativeClothConfigurations"] = new JArray(_skins.Where(value => value.Cloth != null).Select(value => value.Cloth.Describe())),
                ["ownedCloths"] = new JArray(_ownedCloths.Select(value => new JObject {
                    ["instance"] = value == null ? 0 : value.GetInstanceID(), ["enabled"] = value != null && value.enabled,
                    ["particles"] = value == null ? 0 : value.vertices.Length,
                    ["allParticleCoordinatesFinite"] = value != null && value.vertices.All(Finite),
                    ["capsulesAllDisabled"] = value != null && value.capsuleColliders.All(c => c != null && !c.enabled) })),
                ["ordinarySkinRetargetCount"] = _skins.Count(value => value.Cloth == null), ["staticProxyCount"] = _statics.Count,
                ["maxDistanceMultiplier"] = _distances.MaxDistanceMultiplier,
                ["collisionSphereDistanceMultiplier"] = _distances.CollisionSphereDistanceMultiplier,
                ["distanceProposal"] = _distances.Proposal,
                ["disabledCapsuleCollisionResponse"] = "UNKNOWN_REQUIRES_NATIVE_CONTACT_PROOF",
                ["clothDistanceCalibration"] = "EXPERIMENTAL_NOT_ACCEPTED",
                ["renderAndHighlightEquivalence"] = "UNVERIFIED_NATIVE_CAPTURE_REQUIRED",
                ["restoreIssues"] = new JArray(_restoreIssues),
                ["scope"] = "Request-owned visual experiment only. No original Cloth/collider/skeleton/root fields, mesh assets, materials, animation or unit components are modified or cloned. The ordinary skin palette/bounds and original cloth/static render flags are conditionally restored." };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (Skin skin in _skins.Where(value => value.Changed))
            {
                try
                {
                    if (skin.Source == null) continue;
                    if (skin.Source.sharedMesh != skin.Mesh || !Same(skin.Source.sharedMaterials, skin.Materials))
                    { _restoreIssues.Add("foreign-skin-mesh-material:" + skin.Source.GetInstanceID()); continue; }
                    if (skin.Cloth != null)
                    {
                        if (!skin.Source.enabled && Same(skin.Source.bones, skin.Bones) && skin.Source.rootBone == skin.RootBone)
                            skin.Source.enabled = skin.Enabled;
                        else _restoreIssues.Add("foreign-cloth-renderer-state:" + skin.Source.GetInstanceID());
                    }
                    else
                    {
                        // Restore independent owned fields even when commit
                        // stopped between setters. Never require a complete
                        // commit in order to undo its successful first write.
                        if (Same(skin.Source.bones, skin.AppliedBones)) skin.Source.bones = skin.Bones;
                        else if (!Same(skin.Source.bones, skin.Bones)) _restoreIssues.Add("foreign-skin-palette:" + skin.Source.GetInstanceID());
                        if (skin.Source.rootBone == skin.AppliedRoot) skin.Source.rootBone = skin.RootBone;
                        else if (skin.Source.rootBone != skin.RootBone) _restoreIssues.Add("foreign-skin-root:" + skin.Source.GetInstanceID());
                        if (skin.Source.localBounds.Equals(skin.AppliedBounds)) skin.Source.localBounds = skin.Bounds;
                        else if (!skin.Source.localBounds.Equals(skin.Bounds)) _restoreIssues.Add("foreign-skin-bounds:" + skin.Source.GetInstanceID());
                    }
                }
                catch (Exception error) { _restoreIssues.Add("skin:" + error.GetType().Name + ":" + error.Message); }
            }
            foreach (StaticMesh mesh in _statics.Where(value => value.Changed))
                try
                {
                    if (mesh.Source == null) continue;
                    if (mesh.Filter != null && mesh.Filter.sharedMesh == mesh.Mesh && Same(mesh.Source.sharedMaterials, mesh.Materials) && !mesh.Source.enabled)
                        mesh.Source.enabled = mesh.Enabled;
                    else _restoreIssues.Add("foreign-static-renderer:" + mesh.Source.GetInstanceID());
                }
                catch (Exception error) { _restoreIssues.Add("static:" + error.GetType().Name + ":" + error.Message); }
            try { _lastReport = Report(); _lastReport["disposed"] = true; }
            catch (Exception error) { _lastReport = new JObject { ["status"] = _status, ["disposed"] = true, ["reportError"] = error.Message, ["restoreIssues"] = new JArray(_restoreIssues) }; }
            // Disable only owned solver/renderers before deferred destruction.
            foreach (Cloth cloth in _ownedCloths) if (cloth != null) cloth.enabled = false;
            if (gameObject != null) { gameObject.SetActive(false); UnityEngine.Object.Destroy(gameObject); }
        }

        private void OnDestroy() { if (!_disposed) Dispose(); }

        private static string ColliderGeometry(Collider collider)
        {
            if (collider == null) throw new InvalidOperationException("Native collider disappeared.");
            var row = new JObject { ["type"] = collider.GetType().FullName, ["enabled"] = collider.enabled,
                ["isTrigger"] = collider.isTrigger, ["contactOffset"] = collider.contactOffset,
                ["sharedMaterial"] = collider.sharedMaterial == null ? 0 : collider.sharedMaterial.GetInstanceID() };
            var capsule = collider as CapsuleCollider; var sphere = collider as SphereCollider;
            var box = collider as BoxCollider; var mesh = collider as MeshCollider;
            if (capsule != null)
            {
                if (!Finite(capsule.center) || !Finite(capsule.radius) || !Finite(capsule.height) ||
                    capsule.radius <= 0f || capsule.height <= 0f || capsule.direction < 0 || capsule.direction > 2)
                    throw new NotSupportedException("Invalid native capsule geometry.");
                row["center"] = Vector(capsule.center); row["radius"] = capsule.radius; row["height"] = capsule.height; row["direction"] = capsule.direction;
            }
            else if (sphere != null)
            {
                if (!Finite(sphere.center) || !Finite(sphere.radius) || sphere.radius <= 0f) throw new NotSupportedException("Invalid native sphere geometry.");
                row["center"] = Vector(sphere.center); row["radius"] = sphere.radius;
            }
            else if (box != null)
            {
                if (!Finite(box.center) || !Finite(box.size) || box.size.x <= 0f || box.size.y <= 0f || box.size.z <= 0f)
                    throw new NotSupportedException("Invalid native box geometry.");
                row["center"] = Vector(box.center); row["size"] = Vector(box.size);
            }
            else if (mesh != null) { row["sharedMesh"] = mesh.sharedMesh == null ? 0 : mesh.sharedMesh.GetInstanceID(); row["convex"] = mesh.convex; }
            else throw new NotSupportedException("Unreviewed collider geometry: " + collider.GetType().FullName);
            return row.ToString(Formatting.None);
        }

        private static bool Same<T>(T[] first, T[] second) where T : UnityEngine.Object
        { return first != null && second != null && first.Length == second.Length && first.Where((value, i) => !ReferenceEquals(value, second[i])).Count() == 0; }
        private static bool SameCoefficients(ClothSkinningCoefficient[] first, ClothSkinningCoefficient[] second)
        { return first != null && second != null && first.Length == second.Length && first.Where((value, i) => !value.maxDistance.Equals(second[i].maxDistance) || !value.collisionSphereDistance.Equals(second[i].collisionSphereDistance)).Count() == 0; }
        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        private static bool Finite(Vector3 value) { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
        private static bool Finite(Quaternion value) { return Finite(value.x) && Finite(value.y) && Finite(value.z) && Finite(value.w); }
        private static bool Finite(Matrix4x4 value) { for (int i = 0; i < 16; i++) if (!Finite(value[i])) return false; return true; }
        private static bool SameMatrix(Matrix4x4 first, Matrix4x4 second) { for (int i = 0; i < 16; i++) if (!first[i].Equals(second[i])) return false; return true; }
        private static JArray Vector(Vector3 value) { return new JArray(value.x, value.y, value.z); }
        private static JArray Matrix(Matrix4x4 value) { return new JArray(Enumerable.Range(0, 16).Select(i => value[i])); }
    }
}
