using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.View;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // One owner implementation is shared by the bounded experiment and the
    // strictly scoped production entry. Copied Cloth remains experimental.
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
                        throw new NotSupportedException("Negative or non-finite Cloth coefficient requires separate review.");
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
                var renderer = target.GetComponent<SkinnedMeshRenderer>();
                if (!target.gameObject.activeInHierarchy || renderer == null || renderer.enabled ||
                    capsules.Any(value => value == null || value.enabled))
                    throw new NotSupportedException("Owned Cloth initialization requires an active owned hierarchy, hidden renderer and disabled capsule proxies.");
                foreach (var pair in _scalars)
                    if (pair.Key.Name != "enabled") pair.Key.SetValue(target, pair.Value, null);
                // Native25 returned zero coefficients while the hierarchy was
                // inactive. Initialize only the hidden owned solver before
                // assigning its exact source constraints; no native renderer
                // has been suppressed or palette redirected at this point.
                target.enabled = true;
                var coefficients = Coefficients.ToArray();
                for (int i = 0; i < coefficients.Length; i++)
                {
                    coefficients[i].maxDistance = GiantSummonPresentationPolicy.ScaleClothConstraint(
                        coefficients[i].maxDistance, distances.MaxDistanceMultiplier);
                    coefficients[i].collisionSphereDistance = GiantSummonPresentationPolicy.ScaleClothConstraint(
                        coefficients[i].collisionSphereDistance, distances.CollisionSphereDistanceMultiplier);
                }
                target.coefficients = coefficients;
                target.capsuleColliders = capsules;
                target.sphereColliders = new ClothSphereColliderPair[0];
                target.SetVirtualParticleIndices(new List<uint>(_virtualIndices));
                target.SetVirtualParticleWeights(new List<Vector3>(_virtualWeights));
                target.SetSelfAndInterCollisionIndices(new List<uint>(_selfIndices));
                if (!SameCoefficients(target.coefficients, coefficients) || !Same(target.capsuleColliders, capsules) ||
                    target.sphereColliders.Length != 0 || _scalars.Any(pair => pair.Key.Name != "enabled" &&
                    !Equals(pair.Value, pair.Key.GetValue(target, null))))
                    throw new NotSupportedException("Owned Cloth configuration did not round-trip exactly; " +
                        RoundTripDifferences(target, coefficients, capsules).ToString(Formatting.None));
                var indices = new List<uint>(); var weights = new List<Vector3>(); var self = new List<uint>();
                target.GetVirtualParticleIndices(indices); target.GetVirtualParticleWeights(weights);
                target.GetSelfAndInterCollisionIndices(self);
                if (!indices.SequenceEqual(_virtualIndices) || !weights.SequenceEqual(_virtualWeights) || !self.SequenceEqual(_selfIndices))
                    throw new NotSupportedException("Owned Cloth particle configuration did not round-trip exactly; " +
                        RoundTripDifferences(target, coefficients, capsules).ToString(Formatting.None));
            }

            // Failure-only bounded scalars/references. Never export donor
            // vertices/texture pixels or change a failed comparison to PASS.
            private JObject RoundTripDifferences(Cloth target, ClothSkinningCoefficient[] expected, CapsuleCollider[] capsules)
            {
                var scalars = new JArray();
                foreach (var pair in _scalars.Where(value => value.Key.Name != "enabled"))
                {
                    try
                    {
                        object actual = pair.Key.GetValue(target, null);
                        if (!Equals(pair.Value, actual)) scalars.Add(new JObject { ["field"] = pair.Key.Name,
                            ["expected"] = ScalarEvidence(pair.Value), ["readback"] = ScalarEvidence(actual) });
                    }
                    catch (Exception error) { scalars.Add(new JObject { ["field"] = pair.Key.Name,
                        ["expected"] = ScalarEvidence(pair.Value), ["readbackError"] = error.GetType().Name + ":" + error.Message }); }
                }
                ClothSkinningCoefficient[] coefficients = target.coefficients;
                var differences = new JArray(); int mismatchCount = 0;
                for (int index = 0; index < Math.Min(expected.Length, coefficients.Length); index++)
                {
                    var left = expected[index]; var right = coefficients[index];
                    bool max = !left.maxDistance.Equals(right.maxDistance), sphere = !left.collisionSphereDistance.Equals(right.collisionSphereDistance);
                    if (!max && !sphere) continue;
                    mismatchCount++;
                    if (differences.Count < 16) differences.Add(new JObject { ["coefficientIndex"] = index,
                        ["maxDistanceExpected"] = max ? ScalarEvidence(left.maxDistance) : null,
                        ["maxDistanceReadback"] = max ? ScalarEvidence(right.maxDistance) : null,
                        ["collisionSphereDistanceExpected"] = sphere ? ScalarEvidence(left.collisionSphereDistance) : null,
                        ["collisionSphereDistanceReadback"] = sphere ? ScalarEvidence(right.collisionSphereDistance) : null });
                }
                var indices = new List<uint>(); var weights = new List<Vector3>(); var self = new List<uint>();
                var particleDifferences = new JArray();
                try { target.GetVirtualParticleIndices(indices); target.GetVirtualParticleWeights(weights); target.GetSelfAndInterCollisionIndices(self); }
                catch (Exception error) { particleDifferences.Add(new JObject { ["field"] = "particleReadback",
                    ["readbackError"] = error.GetType().Name + ":" + error.Message }); }
                Action<string, IList<uint>, IList<uint>> describeIndices = (field, left, right) => {
                    if (left.SequenceEqual(right)) return;
                    particleDifferences.Add(new JObject { ["field"] = field, ["expectedCount"] = left.Count, ["readbackCount"] = right.Count,
                        ["firstDifferences"] = new JArray(Enumerable.Range(0, Math.Min(left.Count, right.Count)).Where(index => left[index] != right[index])
                            .Take(16).Select(index => new JObject { ["index"] = index, ["expected"] = left[index], ["readback"] = right[index] })) });
                };
                describeIndices("virtualParticleIndices", _virtualIndices, indices);
                describeIndices("selfAndInterCollisionIndices", _selfIndices, self);
                if (!weights.SequenceEqual(_virtualWeights)) particleDifferences.Add(new JObject {
                    ["field"] = "virtualParticleWeights", ["expectedCount"] = _virtualWeights.Count, ["readbackCount"] = weights.Count,
                    ["firstDifferences"] = new JArray(Enumerable.Range(0, Math.Min(weights.Count, _virtualWeights.Count)).Where(index => !weights[index].Equals(_virtualWeights[index]))
                        .Take(16).Select(index => new JObject { ["index"] = index, ["expected"] = ScalarEvidence(_virtualWeights[index]), ["readback"] = ScalarEvidence(weights[index]) })) });
                var skin = target.GetComponent<SkinnedMeshRenderer>();
                return new JObject { ["sourceCloth"] = Source == null ? 0 : Source.GetInstanceID(), ["targetCloth"] = target.GetInstanceID(),
                    ["targetEnabled"] = target.enabled, ["targetActiveSelf"] = target.gameObject.activeSelf, ["targetActiveInHierarchy"] = target.gameObject.activeInHierarchy,
                    ["targetMesh"] = skin?.sharedMesh == null ? null : skin.sharedMesh.name,
                    ["targetMeshVertexCount"] = skin?.sharedMesh == null ? 0 : skin.sharedMesh.vertexCount,
                    ["scalarDifferences"] = scalars, ["expectedCoefficientCount"] = expected.Length, ["readbackCoefficientCount"] = coefficients.Length,
                    ["coefficientMismatchCountInCommonRange"] = mismatchCount, ["firstCoefficientDifferences"] = differences,
                    ["capsuleReferencesExact"] = Same(target.capsuleColliders, capsules),
                    ["expectedCapsules"] = new JArray(capsules.Select(value => value == null ? 0 : value.GetInstanceID())),
                    ["readbackCapsules"] = new JArray(target.capsuleColliders.Select(value => value == null ? 0 : value.GetInstanceID())),
                    ["expectedSpherePairCount"] = 0, ["readbackSpherePairCount"] = target.sphereColliders.Length,
                    ["particleDifferences"] = particleDifferences, ["comparisonWeakened"] = false,
                    ["scope"] = "Failure-only exact scalar/reference/coefficient-index diagnostics; no geometry/pixels or acceptance claim." };
            }

            private static JToken ScalarEvidence(object value)
            {
                if (value is float)
                {
                    float number = (float)value;
                    return new JObject { ["type"] = "System.Single", ["roundTrip"] = number.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                        ["bits"] = "0x" + BitConverter.ToInt32(BitConverter.GetBytes(number), 0).ToString("x8", System.Globalization.CultureInfo.InvariantCulture) };
                }
                if (value is Vector3)
                {
                    var vector = (Vector3)value;
                    return new JObject { ["type"] = "UnityEngine.Vector3", ["x"] = ScalarEvidence(vector.x), ["y"] = ScalarEvidence(vector.y), ["z"] = ScalarEvidence(vector.z) };
                }
                return new JObject { ["type"] = value == null ? null : value.GetType().FullName,
                    ["value"] = value == null ? JValue.CreateNull() : JToken.FromObject(value) };
            }

            internal JObject Describe()
            {
                var values = new JObject();
                foreach (var pair in _scalars)
                    values[pair.Key.Name] = pair.Value is Vector3 ? Vector((Vector3)pair.Value) : JToken.FromObject(pair.Value);
                return new JObject { ["source"] = Source == null ? 0 : Source.GetInstanceID(), ["fields"] = values,
                    ["coefficientCount"] = Coefficients.Length, ["pinnedCount"] = Coefficients.Count(value => value.maxDistance == 0f),
                    ["disabledMaxDistanceCount"] = Coefficients.Count(value => value.maxDistance == float.MaxValue),
                    ["disabledCollisionSphereDistanceCount"] = Coefficients.Count(value => value.collisionSphereDistance == float.MaxValue),
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
        private bool _production;
        private bool _clothInitializedBeforeSourceCommit;
        private bool _retirementPending;
        private int _retainedSkinReferences, _retainedClothReferences, _retainedAnchorReferences;
        private string _productionSourceGuid;
        private Vector3 _visualPivot, _ownedOffset;
        private int _syncCount, _lastSyncFrame;
        private bool _sourceTransformsUnchanged = true, _colliderGeometryUnchanged = true, _colliderMatricesUnchanged = true;
        private JObject _lastReport;
        private readonly List<string> _restoreIssues = new List<string>();

        internal IEnumerable<Cloth> OwnedCloths { get { return _ownedCloths.ToArray(); } }
        internal bool IsDisposed { get { return _disposed; } }
        internal string Status { get { return _status; } }

        // A small authored cloth control, never a replacement giant asset.
        // The paired inputs differ only by an explicit DISABLED capsule array.
        // Pinned vertices establish the returned-particle coordinate frame
        // before contact is evaluated; no native cape particle index map is
        // assumed and no enabled physics collider is ever created.
        internal sealed class DisabledCapsuleContactProbe : IDisposable
        {
            private const int Side = 9;
            private readonly GameObject _root;
            private readonly Cloth[] _cloths = new Cloth[2];
            private readonly Mesh[] _meshes = new Mesh[2];
            private readonly Transform[] _frames = new Transform[2];
            private readonly CapsuleCollider[] _capsules = new CapsuleCollider[2];
            private readonly Vector3[][] _first = new Vector3[2][];
            private readonly string[] _spaces = new string[2];
            private readonly Vector3[] _vertices;
            private readonly Material _borrowed;
            private int _samples, _valid, _warmup, _lastFrame = -1;
            private bool _disposed, _disabled = true;
            private float _difference, _controlPenetration, _contactPenetration, _motion;
            private string _reason;

            internal static DisabledCapsuleContactProbe Take(Cleanup2GiantRenderScalePrototype owner)
            {
                if (owner == null || owner._production || owner._disposed || !owner._committed ||
                    owner._actor.Blueprint.AssetGuid != GiantSummonPresentationPolicy.NativeFrostGuid ||
                    owner._ownedCloths.Count != 1)
                    throw new InvalidOperationException("Exact request-owned Frost copied-solver owner required for the contact control.");
                return new DisabledCapsuleContactProbe(owner);
            }

            private DisabledCapsuleContactProbe(Cleanup2GiantRenderScalePrototype owner)
            {
                _borrowed = owner._ownedCloths[0].GetComponent<SkinnedMeshRenderer>().sharedMaterials.FirstOrDefault(value => value != null);
                if (_borrowed == null) throw new InvalidOperationException("Contact control requires one borrowed live native material.");
                _root = new GameObject("KMG_C2_DisabledCapsuleContactControl");
                _root.SetActive(false);
                _root.transform.SetParent(owner.transform, false);
                _root.transform.SetPositionAndRotation(owner._actor.Position + Vector3.up * 12f, Quaternion.identity);
                Vector3 scale = owner.transform.lossyScale;
                if (!Finite(scale) || scale.x <= 0f || scale.y <= 0f || scale.z <= 0f)
                { UnityEngine.Object.Destroy(_root); throw new InvalidOperationException("Finite positive contact-parent scale required."); }
                _root.transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
                _vertices = Enumerable.Range(0, Side * Side).Select(index =>
                    new Vector3((index % Side - 4) * .125f, 2f - index / Side * .25f, 0f)).ToArray();
                try
                {
                    var triangles = new List<int>();
                    for (int y = 0; y < Side - 1; y++) for (int x = 0; x < Side - 1; x++)
                    {
                        int a = y * Side + x;
                        triangles.AddRange(new[] { a, a + Side, a + 1, a + 1, a + Side, a + Side + 1 });
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        var node = new GameObject(i == 0 ? "NoCollisionControl" : "DisabledCapsuleContact");
                        node.transform.SetParent(_root.transform, false); node.transform.localPosition = Vector3.right * i * 3f;
                        _frames[i] = node.transform;
                        _meshes[i] = new Mesh { name = "KMG_C2_OriginalContactGrid_" + i };
                        _meshes[i].vertices = _vertices; _meshes[i].triangles = triangles.ToArray();
                        _meshes[i].normals = Enumerable.Repeat(Vector3.forward, _vertices.Length).ToArray();
                        _meshes[i].boneWeights = Enumerable.Range(0, _vertices.Length).Select(index =>
                            new BoneWeight { boneIndex0 = 0, weight0 = 1f }).ToArray();
                        _meshes[i].bindposes = new[] { Matrix4x4.identity }; _meshes[i].RecalculateBounds();
                        var skin = node.AddComponent<SkinnedMeshRenderer>();
                        skin.sharedMesh = _meshes[i]; skin.sharedMaterial = _borrowed;
                        skin.bones = new[] { node.transform }; skin.rootBone = node.transform;
                        skin.localBounds = _meshes[i].bounds;
                        skin.updateWhenOffscreen = true; skin.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                        var capsuleNode = new GameObject("DisabledCapsule");
                        capsuleNode.transform.SetParent(node.transform, false);
                        _capsules[i] = capsuleNode.AddComponent<CapsuleCollider>();
                        _capsules[i].enabled = false;
                        _capsules[i].direction = 0; _capsules[i].center = new Vector3(0f, .9f, -.35f);
                        _capsules[i].radius = .35f; _capsules[i].height = 1.4f;
                        _cloths[i] = node.AddComponent<Cloth>();
                        _cloths[i].useGravity = true; _cloths[i].damping = .15f;
                        _cloths[i].bendingStiffness = .2f; _cloths[i].stretchingStiffness = .8f;
                        _cloths[i].externalAcceleration = new Vector3(0f, 0f, -12f);
                        _cloths[i].worldVelocityScale = 0f; _cloths[i].worldAccelerationScale = 0f;
                        _cloths[i].coefficients = Enumerable.Range(0, _vertices.Length).Select(index =>
                            new ClothSkinningCoefficient { maxDistance = index < Side ? 0f : 3f, collisionSphereDistance = 0f }).ToArray();
                        _cloths[i].capsuleColliders = i == 0 ? new CapsuleCollider[0] : new[] { _capsules[i] };
                        _cloths[i].sphereColliders = new ClothSphereColliderPair[0];
                    }
                    _root.SetActive(true);
                }
                catch { Dispose(); throw; }
            }

            internal void Observe()
            {
                if (_disposed || _lastFrame == Time.frameCount) return;
                _lastFrame = Time.frameCount;
                _samples++;
                var local = new Vector3[2][];
                for (int i = 0; i < 2; i++)
                {
                    _disabled &= _capsules[i] != null && !_capsules[i].enabled;
                    Vector3[] particles = _cloths[i].vertices;
                    if (particles.Length != _vertices.Length || particles.Any(value => !Finite(value)))
                    { _reason = "Missing/non-finite particles or authored no-seam grid count mismatch"; if (_valid == 0) { _samples--; _warmup++; } return; }
                    float localError = Enumerable.Range(0, Side).Max(index => Vector3.Distance(particles[index], _vertices[index]));
                    float worldError = Enumerable.Range(0, Side).Max(index => Vector3.Distance(particles[index], _frames[i].TransformPoint(_vertices[index])));
                    string space = localError < .03f && worldError >= .03f ? "renderer-local" :
                        worldError < .03f && localError >= .03f ? "world" : null;
                    if (space == null || (_spaces[i] != null && _spaces[i] != space))
                    { _reason = "Pinned authored vertices do not prove one stable particle frame"; if (_valid == 0) { _samples--; _warmup++; } return; }
                    _spaces[i] = space;
                    local[i] = space == "renderer-local" ? particles : particles.Select(_frames[i].InverseTransformPoint).ToArray();
                    if (_first[i] == null) _first[i] = local[i];
                    for (int index = Side; index < local[i].Length; index++)
                    {
                        _motion = Mathf.Max(_motion, Vector3.Distance(local[i][index], _first[i][index]));
                        Vector3 p = local[i][index] - new Vector3(0f, .9f, -.35f);
                        p.x -= Mathf.Clamp(p.x, -.35f, .35f);
                        float penetration = Mathf.Max(0f, .35f - p.magnitude);
                        if (i == 0) _controlPenetration = Mathf.Max(_controlPenetration, penetration);
                        else _contactPenetration = Mathf.Max(_contactPenetration, penetration);
                    }
                }
                _valid++; _reason = null;
                for (int index = Side; index < _vertices.Length; index++)
                    _difference = Mathf.Max(_difference, Vector3.Distance(local[0][index], local[1][index]));
            }

            internal JObject Describe()
            {
                bool positive = _samples >= 36 && _valid == _samples && _disabled && _motion > .03f &&
                    _difference > .03f && _controlPenetration > .08f && _contactPenetration <= .08f;
                return new JObject { ["passed"] = positive, ["samples"] = _samples, ["validSamples"] = _valid,
                    ["initializationFramesWithoutAProvenParticleFrame"] = _warmup,
                    ["particlesPerAuthoredGrid"] = _vertices.Length, ["pinnedParticlesPerGrid"] = Side,
                    ["controlParticleFrame"] = _spaces[0], ["contactParticleFrame"] = _spaces[1],
                    ["allOwnedCapsulesAlwaysDisabled"] = _disabled, ["positiveParticleMotion"] = _motion,
                    ["maximumPairedParticleDifference"] = _difference, ["noCollisionControlMaximumCapsulePenetration"] = _controlPenetration,
                    ["disabledCapsuleMaximumPenetration"] = _contactPenetration, ["reason"] = _reason,
                    ["limit"] = "Paired project-authored 9x9 no-seam grids test only disabled-capsule consumption under stated gravity/external acceleration. This does not accept native cape distance/inertia calibration or art.",
                    ["disposed"] = _disposed, ["allOwnedResourcesReleased"] = _disposed && _root == null && _meshes.All(value => value == null),
                    ["borrowedNativeMaterialAlive"] = _borrowed != null };
            }

            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                foreach (Cloth cloth in _cloths) if (cloth != null) cloth.enabled = false;
                if (_root != null) { _root.SetActive(false); UnityEngine.Object.Destroy(_root); }
                foreach (Mesh mesh in _meshes) if (mesh != null) UnityEngine.Object.Destroy(mesh);
            }
        }

        internal static Cleanup2GiantRenderScalePrototype Take(RuntimeTestRequest request, UnitEntityData actor,
            UnitEntityData requestCaster, IList<UnitEntityData> exactPositiveOwnedActors, float uniformFactor,
            ProposedClothDistances proposedDistances, float verifiedNativeSoleWorldY)
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

            return TakeOwned(actor, uniformFactor, proposedDistances, request.RunId, false, verifiedNativeSoleWorldY);
        }

        internal static Cleanup2GiantRenderScalePrototype TakeProduction(UnitEntityData actor,
            GiantSummonPresentationProfile profile)
        {
            if (!ExpandedSummoningVisualVariantPatch.IsModuleActive || actor == null || actor.Destroyed ||
                actor.View == null || actor.Blueprint == null || profile == null ||
                actor.Blueprint.Prefab == null || actor.Blueprint.Prefab.AssetId != profile.NativePrefabId)
                throw new InvalidOperationException("Exact active giant unit/view/native-prefab scope required.");
            Buff[] markers = actor.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                value.Blueprint == BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff).ToArray();
            string source = markers.Length == 1 ? markers[0].MaybeContext?.SourceAbility?.AssetGuid : null;
            if (source == null || markers[0].MaybeContext.MaybeCaster == null ||
                !ReferenceEquals(GiantSummonPresentationPolicy.Resolve(true, actor.Blueprint.AssetGuid, source), profile))
                throw new InvalidOperationException("Exact live giant summon provenance required.");
            // There is no source-only assertion that a copied solver consumes
            // disabled capsules or preserves cape contact at another scale.
            if (actor.View.GetComponentsInChildren<Cloth>(true).Length != 0)
                throw new NotSupportedException("Production copied-Cloth activation awaits positive native solver/contact qualification.");
            if (actor.View.GetComponentInChildren<Cleanup2GiantRenderScalePrototype>(true) != null)
                throw new InvalidOperationException("An existing visual scale owner cannot be replaced.");
            return TakeOwned(actor, profile.RelativeVisualFactor,
                new ProposedClothDistances(1f, 1f, "not-used-production-has-no-cloth"), "production:" + actor.UniqueId, true,
                NativeBodySole(actor, profile), source);
        }

        private static float NativeBodySole(UnitEntityData actor, GiantSummonPresentationProfile profile)
        {
            string bodyName = profile.NativePrefabId == "635d6dee1c7831b41aa987bdbeff14bc" ? "hill_giant_1" : "Frozen_body";
            SkinnedMeshRenderer[] matches = actor.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value.name == bodyName && value.enabled && value.gameObject.activeInHierarchy).ToArray();
            if (matches.Length != 1 || matches[0].sharedMesh == null || !matches[0].sharedMesh.isReadable)
                throw new NotSupportedException("One exact CPU-readable native body is required for a stable visual sole pivot.");
            var skin = matches[0];
            Vector3[] vertices = skin.sharedMesh.vertices;
            BoneWeight[] weights = skin.sharedMesh.boneWeights;
            Matrix4x4[] binds = skin.sharedMesh.bindposes;
            Transform[] bones = skin.bones;
            if (vertices.Length == 0 || vertices.Length > 100000 || weights.Length != vertices.Length)
                throw new NotSupportedException("Incomplete/bounded native sole input required.");
            var matrices = new Dictionary<int, Matrix4x4>();
            float sole = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                BoneWeight w = weights[i];
                int[] indices = { w.boneIndex0, w.boneIndex1, w.boneIndex2, w.boneIndex3 };
                float[] influences = { w.weight0, w.weight1, w.weight2, w.weight3 };
                Vector3 world = Vector3.zero; float sum = 0f;
                if (!Finite(vertices[i])) throw new NotSupportedException("Non-finite native body point.");
                for (int influence = 0; influence < 4; influence++)
                {
                    float weight = influences[influence]; sum += weight;
                    if (!Finite(weight) || weight < 0f) throw new NotSupportedException("Invalid native sole weight.");
                    if (weight == 0f) continue;
                    int index = indices[influence];
                    if (index < 0 || index >= bones.Length || index >= binds.Length || bones[index] == null ||
                        !bones[index].IsChildOf(actor.View.transform)) throw new NotSupportedException("Foreign/incomplete native sole palette.");
                    Matrix4x4 matrix;
                    if (!matrices.TryGetValue(index, out matrix))
                    {
                        matrix = bones[index].localToWorldMatrix * binds[index];
                        if (!Finite(matrix)) throw new NotSupportedException("Non-finite native sole matrix.");
                        matrices.Add(index, matrix);
                    }
                    world += matrix.MultiplyPoint3x4(vertices[i]) * weight;
                }
                if (!Finite(world) || !Finite(sum) || Mathf.Abs(sum - 1f) > .005f)
                    throw new NotSupportedException("Native sole normalization/position invalid.");
                sole = Mathf.Min(sole, world.y);
            }
            return sole;
        }

        private static Cleanup2GiantRenderScalePrototype TakeOwned(UnitEntityData actor, float uniformFactor,
            ProposedClothDistances proposedDistances, string identity, bool production, float nativeSoleWorldY, string productionSource = null)
        {
            Transform nativeRoot = actor.View.transform;
            if (!Finite(nativeSoleWorldY) || Vector3.Dot(nativeRoot.up, Vector3.up) < .9999f)
                throw new NotSupportedException("Finite native body sole and upright source root required.");
            float localSole = nativeRoot.InverseTransformPoint(new Vector3(nativeRoot.position.x, nativeSoleWorldY, nativeRoot.position.z)).y;
            Vector3 pivot = new Vector3(0f, localSole, 0f);
            Vector3 offset = new Vector3(0f, GiantSummonPresentationPolicy.SolePivotOffset(localSole, uniformFactor), 0f);
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
                root.transform.localPosition = offset;
                owner = root.AddComponent<Cleanup2GiantRenderScalePrototype>();
                owner._actor = actor; owner._view = actor.View; owner._sourceRoot = actor.View.transform;
                owner._runId = identity; owner._factor = uniformFactor; owner._distances = proposedDistances; owner._production = production;
                owner._productionSourceGuid = productionSource;
                owner._visualPivot = pivot; owner._ownedOffset = offset;
                owner._sourceTransforms = plan.Transforms; owner._sourceParents = plan.Parents;
                owner._sourceRenderers = plan.Renderers;
                owner._skins.AddRange(plan.Skins); owner._statics.AddRange(plan.Statics);
                owner._nativeColliders.AddRange(plan.Colliders);
                owner._mirrors.Add(owner._sourceRoot, root.transform);
                foreach (Transform source in plan.Transforms) owner.Mirror(source);
                owner.CopyMirrorPose();
                owner.CreateOwnedRenderers();
                owner.RequireSourceIdentity(false);
                // All owned renderers and capsule physics shapes remain
                // disabled. Only this private hierarchy becomes active so
                // Cloth can initialize before any native source mutation.
                Matrix4x4[] beforeInitialization = owner._sourceTransforms.Select(value => value.localToWorldMatrix).ToArray();
                Matrix4x4[] colliderInitialization = owner._nativeColliders.Select(value => value.Source.transform.localToWorldMatrix).ToArray();
                root.SetActive(true);
                owner.InitializeOwnedCloth();
                owner.VerifyNoNativeWrite(beforeInitialization, colliderInitialization);
                owner.RequireSourceIdentity(false);
                owner.Commit();
                owner.RevealOwnedRenderers();
                owner._status = production ? "PRODUCTION_ACTIVE_NO_CLOTH" : "EXPERIMENT_ACTIVE_COLLISION_AND_DISTANCE_CALIBRATION_UNKNOWN";
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
                        if (state.Cloth != null)
                            foreach (var value in state.Cloth.Coefficients)
                            {
                                GiantSummonPresentationPolicy.ScaleClothConstraint(value.maxDistance, distances.MaxDistanceMultiplier);
                                GiantSummonPresentationPolicy.ScaleClothConstraint(value.collisionSphereDistance, distances.CollisionSphereDistanceMultiplier);
                            }
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
            var node = new GameObject("KMG_C2_Mirror_" + Array.IndexOf(_sourceTransforms, source) + "_" + source.name);
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
                proxy.enabled = false;
                var cloth = node.AddComponent<Cloth>();
                cloth.enabled = false;
                _ownedCloths.Add(cloth);
                foreach (CapsuleCollider capsule in skin.Cloth.Capsules) CapsuleProxy(capsule);
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
                mesh.Proxy.enabled = false;
            }
        }

        private void InitializeOwnedCloth()
        {
            if (_committed || !gameObject.activeInHierarchy ||
                _skins.Any(value => value.Proxy != null && value.Proxy.enabled) ||
                _statics.Any(value => value.Proxy != null && value.Proxy.enabled))
                throw new InvalidOperationException("Owned Cloth must initialize hidden before the native source commit.");
            foreach (Skin skin in _skins.Where(value => value.Cloth != null))
                skin.Cloth.CopyTo(skin.Proxy.GetComponent<Cloth>(),
                    skin.Cloth.Capsules.Select(value => _capsules[value]).ToArray(), _distances);
            _clothInitializedBeforeSourceCommit = _ownedCloths.Count != 0;
        }

        private void RevealOwnedRenderers()
        {
            foreach (Skin skin in _skins.Where(value => value.Proxy != null)) skin.Proxy.enabled = skin.Enabled;
            foreach (StaticMesh mesh in _statics) mesh.Proxy.enabled = mesh.Enabled;
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
            if (_ownedCloths.Count != 0 && !_clothInitializedBeforeSourceCommit)
                throw new InvalidOperationException("Exact hidden owned Cloth initialization must precede native source commit.");
            Matrix4x4[] matrices = _nativeColliders.Select(value => value.Source.transform.localToWorldMatrix).ToArray();
            Matrix4x4[] transforms = _sourceTransforms.Select(value => value.localToWorldMatrix).ToArray();
            foreach (Skin skin in _skins)
            {
                skin.Changed = true;
                if (skin.Cloth != null) skin.Source.enabled = false;
                else
                {
                    skin.Source.bones = skin.AppliedBones; skin.Source.rootBone = skin.AppliedRoot;
                    Vector3 anchor = skin.Source.transform.InverseTransformPoint(_sourceRoot.TransformPoint(_visualPivot));
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
            if (transform.parent != _sourceRoot || !transform.localPosition.Equals(_ownedOffset) ||
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
            bool initializedSolver = committed || _clothInitializedBeforeSourceCommit;
            foreach (Cloth cloth in _ownedCloths)
                if (cloth == null || cloth.enabled != initializedSolver ||
                    (!initializedSolver && gameObject.activeInHierarchy) ||
                    (initializedSolver && (!cloth.gameObject.activeInHierarchy || cloth.vertices.Any(value => !Finite(value)))))
                    throw new InvalidOperationException("Owned solver phase contract failed: expected " +
                        (initializedSolver ? "initialized-active-enabled-finite" : "prepared-inactive-disabled") + ".");
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
            if (_production && !ExpandedSummoningVisualVariantPatch.IsModuleActive)
                throw new InvalidOperationException("Module-off releases the owned giant presentation.");
            if (_production)
            {
                int markers = 0;
                foreach (Buff buff in _actor.Descriptor.Buffs)
                    if (buff.Blueprint == BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)
                    {
                        markers++;
                        if (buff.MaybeContext?.SourceAbility?.AssetGuid != _productionSourceGuid || buff.MaybeContext.MaybeCaster == null)
                            throw new InvalidOperationException("Owned production summon provenance changed.");
                    }
                if (markers != 1) throw new InvalidOperationException("Owned production summon marker ended or became ambiguous.");
            }
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
            if (_disposed)
            {
                if (_retirementPending) TryRetireOwnedRoot();
                return;
            }
            if (!_committed) return;
            try { Synchronize(); }
            catch (Exception error)
            {
                _status = "REFUSED:" + error.GetType().Name + ":" + error.Message;
                Dispose();
            }
        }

        internal JObject Describe()
        {
            if (_disposed)
            {
                JObject result = _lastReport == null ? new JObject { ["status"] = _status } : (JObject)_lastReport.DeepClone();
                result["ownedRootRetirementPending"] = _retirementPending;
                result["retainedForeignSkinReferences"] = _retainedSkinReferences;
                result["retainedForeignClothReferences"] = _retainedClothReferences;
                result["retainedForeignProbeAnchorReferences"] = _retainedAnchorReferences;
                result["ownedRootReleased"] = this == null || gameObject == null;
                return result;
            }
            return Report();
        }

        private void TryRetireOwnedRoot()
        {
            // Refusal must not turn a foreign/mixed palette into dangling
            // bones. Include inactive loaded receivers: a hidden cached view
            // can still reference these exact allocated transforms. This is a
            // reference-only census; no donor mesh/pixel/component is changed.
            _retainedSkinReferences = Resources.FindObjectsOfTypeAll<SkinnedMeshRenderer>().Count(skin =>
                skin != null && !Owned(skin.transform) &&
                ((skin.rootBone != null && Owned(skin.rootBone)) || skin.bones.Any(bone => bone != null && Owned(bone))));
            _retainedClothReferences = Resources.FindObjectsOfTypeAll<Cloth>().Count(cloth =>
                cloth != null && !Owned(cloth.transform) && cloth.capsuleColliders.Any(capsule => capsule != null && Owned(capsule.transform)));
            _retainedAnchorReferences = Resources.FindObjectsOfTypeAll<Renderer>().Count(renderer =>
                renderer != null && !Owned(renderer.transform) && renderer.probeAnchor != null && Owned(renderer.probeAnchor));
            _retirementPending = _retainedSkinReferences + _retainedClothReferences + _retainedAnchorReferences > 0;
            if (_retirementPending)
            {
                // Only our root moves. Keep its world pose and exact resources
                // alive even if the former native view is subsequently freed.
                if (transform.parent != null) transform.SetParent(null, true);
                if (!_restoreIssues.Contains("owned-mirrors-retained-for-foreign-references"))
                    _restoreIssues.Add("owned-mirrors-retained-for-foreign-references");
                return;
            }
            if (gameObject != null) { gameObject.SetActive(false); UnityEngine.Object.Destroy(gameObject); }
        }

        private JObject Report()
        {
            return new JObject { ["runId"] = _runId, ["actor"] = _actor == null ? null : _actor.UniqueId,
                ["status"] = _status, ["uniformVisualFactor"] = _factor, ["syncCount"] = _syncCount,
                ["productionOwner"] = _production, ["productionCopiedClothEnabled"] = false,
                ["ownedClothInitializedHiddenBeforeSourceCommit"] = _clothInitializedBeforeSourceCommit,
                ["stableNativeSolePivotInView"] = Vector(_visualPivot), ["ownedVisualRootOffset"] = Vector(_ownedOffset),
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
                    if (skin.Source.sharedMesh != skin.Mesh)
                    { _restoreIssues.Add("foreign-skin-mesh:" + skin.Source.GetInstanceID()); continue; }
                    // A separately owned palette can release its material
                    // first. That does not transfer ownership of our exact
                    // bone/root/bounds fields on the unchanged native mesh.
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
            foreach (Skin skin in _skins) if (skin.Proxy != null) skin.Proxy.enabled = false;
            foreach (StaticMesh mesh in _statics) if (mesh.Proxy != null) mesh.Proxy.enabled = false;
            TryRetireOwnedRoot();
            if (_lastReport != null) _lastReport["restoreIssues"] = new JArray(_restoreIssues);
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
