using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.View;
using Kingmaker.Visual.OccludedObjectHighlighting;
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

            internal bool MatchesOwned(Cloth target, CapsuleCollider[] capsules, ProposedClothDistances distances)
            {
                if (target == null || !target.enabled || !target.gameObject.activeInHierarchy ||
                    !Same(target.capsuleColliders, capsules) || target.sphereColliders.Length != 0 ||
                    _scalars.Any(pair => pair.Key.Name != "enabled" && !Equals(pair.Value, pair.Key.GetValue(target, null)))) return false;
                var expected = Coefficients.ToArray();
                for (int i = 0; i < expected.Length; i++)
                {
                    expected[i].maxDistance = GiantSummonPresentationPolicy.ScaleClothConstraint(expected[i].maxDistance, distances.MaxDistanceMultiplier);
                    expected[i].collisionSphereDistance = GiantSummonPresentationPolicy.ScaleClothConstraint(expected[i].collisionSphereDistance, distances.CollisionSphereDistanceMultiplier);
                }
                if (!SameCoefficients(target.coefficients, expected)) return false;
                var indices = new List<uint>(); var weights = new List<Vector3>(); var self = new List<uint>();
                target.GetVirtualParticleIndices(indices); target.GetVirtualParticleWeights(weights); target.GetSelfAndInterCollisionIndices(self);
                return indices.SequenceEqual(_virtualIndices) && weights.SequenceEqual(_virtualWeights) && self.SequenceEqual(_selfIndices);
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
        private List<Renderer> _nativeRendererCache;
        private Renderer[] _nativeRendererCacheBefore, _registeredRenderers = new Renderer[0];
        private object _nativeColliderCache;
        private Collider[] _nativeColliderCacheBefore;
        private OccludedObjectHighlighter _occludedHighlighter;
        private Kingmaker.Visual.UnitMultiHighlight _ordinaryHighlighter;
        private HighlightingSystem.Highlighter _ordinaryReceiverOwner;
        private bool _rendererCacheRegistered, _nativeVisibilityObserved, _nativeVisible;
        private int _nativeVisibilityWrites, _highlightRefreshes, _retainedRendererCacheReferences;
        private bool _sourceStateRestored;
        private bool _highlightRetirementReady;
        private GiantSummonPresentationProfile _profile;
        private static readonly FieldInfo ColliderCacheField = typeof(EntityViewBase).GetField("m_Colliders", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo OccludedRenderersField = typeof(OccludedObjectHighlighter).GetField("m_Renderers", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo FindOccludedRenderers = typeof(OccludedObjectHighlighter).GetMethod("FindRenderers", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        private static readonly FieldInfo OrdinaryReceiversField = typeof(HighlightingSystem.Highlighter).GetField("highlightableRenderers", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo UpdateOrdinaryRenderers = typeof(HighlightingSystem.Highlighter).GetMethod("UpdateRenderers", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);
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
        internal bool IsProductionOwner { get { return _production; } }
        // Closed until the actual authored disabled-capsule contact control
        // and native copied-cape packet both qualify. No configuration bypass.
        internal static bool ProductionCopiedClothEnabled { get { return false; } }

        internal static string ProductionClothAdmission(UnitEntityData actor, GiantSummonPresentationProfile profile)
        {
            if (actor?.View == null || profile == null) return "missing-unit-profile";
            Cloth[] cloths = actor.View.GetComponentsInChildren<Cloth>(true);
            if (cloths.Length == 0) return "no-cloth";
            if (!ProductionCopiedClothEnabled) return "await-native-copied-cloth-contact-proof";
            if (profile.NativePrefabId != "df0f3fbceb3605d489c0dec8fa5c7d1e" ||
                (profile.Key != "frost-giant" && profile.Key != "fire-giant" && profile.Key != "storm-giant") || cloths.Length != 1)
                return "unqualified-cloth-profile";
            Cloth cloth = cloths[0]; var skin = cloth.GetComponent<SkinnedMeshRenderer>();
            if (!cloth.enabled || !cloth.gameObject.activeInHierarchy || skin == null || !skin.enabled ||
                skin.name != "Frozen_cape1" || skin.sharedMesh == null || skin.sharedMesh.name != "Frozen_cape1" || skin.sharedMesh.vertexCount != 354 ||
                RendererPath(actor.View.transform, skin.transform) != "Frozen_body_RIG[3]/Frozen_cape1[1]" ||
                cloth.capsuleColliders.Length != 5 || cloth.sphereColliders.Length != 0)
                return "native-frozen-cape-signature-mismatch";
            ClothSkinningCoefficient[] values = cloth.coefficients;
            ulong hash = 14695981039346656037UL;
            unchecked { foreach (var value in values) foreach (float number in new[] { value.maxDistance, value.collisionSphereDistance })
                foreach (byte part in BitConverter.GetBytes(number)) hash = (hash ^ part) * 1099511628211UL; }
            return values.Length == 292 && values.Count(value => value.maxDistance == 0f) == 92 &&
                hash.ToString("x16", System.Globalization.CultureInfo.InvariantCulture) == "1caf9c99f5b5755f"
                ? "qualified-frozen-cape" : "native-frozen-cape-constraints-mismatch";
        }

        private static string RendererPath(Transform root, Transform value)
        {
            var parts = new List<string>();
            for (Transform node = value; node != null && node != root; node = node.parent)
                parts.Add(node.name + "[" + node.GetSiblingIndex() + "]");
            parts.Reverse(); return string.Join("/", parts.ToArray());
        }

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
            private readonly JArray _initialization = new JArray();
            private readonly JObject[] _frameEvidence = new JObject[2];
            private JObject _firstFrameFailure;
            private readonly Vector3[] _vertices;
            private readonly Material _borrowed;
            private readonly System.Diagnostics.Stopwatch _elapsed = System.Diagnostics.Stopwatch.StartNew();
            private readonly float _startedTime, _startedUnscaledTime;
            private float _deltaSum, _minimumDelta = float.MaxValue, _maximumDelta;
            private readonly Vector3[] _freeMinimum = new Vector3[2], _freeMaximum = new Vector3[2];
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
                _startedTime = Time.time; _startedUnscaledTime = Time.unscaledTime;
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
                        skin.enabled = false;
                        var capsuleNode = new GameObject("DisabledCapsule");
                        capsuleNode.transform.SetParent(node.transform, false);
                        _capsules[i] = capsuleNode.AddComponent<CapsuleCollider>();
                        _capsules[i].enabled = false;
                        _capsules[i].direction = 0; _capsules[i].center = new Vector3(0f, .9f, -.35f);
                        _capsules[i].radius = .35f; _capsules[i].height = 1.4f;
                        _cloths[i] = node.AddComponent<Cloth>();
                        _cloths[i].enabled = false;
                    }
                    _root.SetActive(true);
                    // As established for the native cape, an inactive solver
                    // can discard coefficient writes. Initialize both hidden
                    // authored controls on their active owned hierarchy, then
                    // prove all pin constraints before observing any motion.
                    for (int i = 0; i < 2; i++)
                    {
                        Cloth cloth = _cloths[i];
                        cloth.enabled = true;
                        cloth.useGravity = true; cloth.damping = .15f;
                        cloth.bendingStiffness = .2f; cloth.stretchingStiffness = .8f;
                        cloth.externalAcceleration = new Vector3(0f, 0f, -12f);
                        cloth.worldVelocityScale = 0f; cloth.worldAccelerationScale = 0f;
                        var expected = Enumerable.Range(0, _vertices.Length).Select(index =>
                            // Zero is an enabled skinned-surface backstop,
                            // opposing the -Z stimulus behind +Z normals.
                            // Unity2018.4 ClothInspector uses exact MaxValue
                            // to disable it, as does the observed native cape.
                            new ClothSkinningCoefficient { maxDistance = index < Side ? 0f : 3f, collisionSphereDistance = float.MaxValue }).ToArray();
                        CapsuleCollider[] capsules = i == 0 ? new CapsuleCollider[0] : new[] { _capsules[i] };
                        cloth.coefficients = expected;
                        cloth.capsuleColliders = capsules;
                        cloth.sphereColliders = new ClothSphereColliderPair[0];
                        ClothSkinningCoefficient[] actual = cloth.coefficients;
                        bool exact = cloth.gameObject.activeInHierarchy && cloth.enabled && !_capsules[i].enabled &&
                            SameCoefficients(actual, expected) && Same(cloth.capsuleColliders, capsules) && cloth.sphereColliders.Length == 0 &&
                            cloth.useGravity && cloth.damping.Equals(.15f) && cloth.bendingStiffness.Equals(.2f) &&
                            cloth.stretchingStiffness.Equals(.8f) && cloth.externalAcceleration.Equals(new Vector3(0f, 0f, -12f)) &&
                            cloth.worldVelocityScale.Equals(0f) && cloth.worldAccelerationScale.Equals(0f);
                        var receipt = new JObject { ["grid"] = i, ["activeInHierarchy"] = cloth.gameObject.activeInHierarchy,
                            ["enabled"] = cloth.enabled, ["rendererHiddenDuringInitialization"] = !cloth.GetComponent<SkinnedMeshRenderer>().enabled,
                            ["expectedCoefficientCount"] = expected.Length, ["readbackCoefficientCount"] = actual.Length,
                            ["readbackPinnedCount"] = actual.Count(value => value.maxDistance == 0f),
                            ["readbackDisabledBackstopCount"] = actual.Count(value => value.collisionSphereDistance == float.MaxValue),
                            ["coefficientsExact"] = SameCoefficients(actual, expected), ["capsuleReferencesExact"] = Same(cloth.capsuleColliders, capsules),
                            ["ownedCapsuleDisabled"] = !_capsules[i].enabled, ["configurationExact"] = exact,
                            ["scalarReadback"] = new JObject { ["useGravity"] = cloth.useGravity, ["damping"] = cloth.damping,
                                ["bendingStiffness"] = cloth.bendingStiffness, ["stretchingStiffness"] = cloth.stretchingStiffness,
                                ["externalAcceleration"] = Vector(cloth.externalAcceleration), ["worldVelocityScale"] = cloth.worldVelocityScale,
                                ["worldAccelerationScale"] = cloth.worldAccelerationScale, ["capsuleCount"] = cloth.capsuleColliders.Length,
                                ["spherePairCount"] = cloth.sphereColliders.Length } };
                        _initialization.Add(receipt);
                        if (!exact) throw new NotSupportedException("Authored Cloth control configuration did not round-trip exactly: " + receipt.ToString(Formatting.None));
                    }
                    foreach (Cloth cloth in _cloths) cloth.GetComponent<SkinnedMeshRenderer>().enabled = true;
                }
                catch { Dispose(); throw; }
            }

            internal void Observe()
            {
                if (_disposed || _lastFrame == Time.frameCount) return;
                _lastFrame = Time.frameCount;
                _samples++;
                _deltaSum += Time.deltaTime; _minimumDelta = Mathf.Min(_minimumDelta, Time.deltaTime); _maximumDelta = Mathf.Max(_maximumDelta, Time.deltaTime);
                var local = new Vector3[2][];
                for (int i = 0; i < 2; i++)
                {
                    _disabled &= _capsules[i] != null && !_capsules[i].enabled;
                    Vector3[] particles = _cloths[i].vertices;
                    if (particles.Length != _vertices.Length || particles.Any(value => !Finite(value)))
                    { _reason = "Missing/non-finite particles or authored no-seam grid count mismatch"; if (_valid == 0) { _samples--; _warmup++; } return; }
                    float localError = Enumerable.Range(0, Side).Max(index => Vector3.Distance(particles[index], _vertices[index]));
                    float worldError = Enumerable.Range(0, Side).Max(index => Vector3.Distance(particles[index], _frames[i].TransformPoint(_vertices[index])));
                    int worst = Enumerable.Range(0, Side).OrderByDescending(index => Vector3.Distance(particles[index], _vertices[index])).First();
                    _frameEvidence[i] = new JObject { ["grid"] = i, ["sample"] = _samples,
                        ["maximumPinnedRendererLocalError"] = localError, ["maximumPinnedWorldError"] = worldError,
                        ["worstRendererLocalPinnedIndex"] = worst, ["authoredPinnedLocal"] = Vector(_vertices[worst]),
                        ["returnedAuthoredPinnedParticle"] = Vector(particles[worst]),
                        ["authoredRendererWorldMatrix"] = Matrix(_frames[i].localToWorldMatrix),
                        ["readbackCoefficientCount"] = _cloths[i].coefficients.Length,
                        ["readbackPinnedCount"] = _cloths[i].coefficients.Count(value => value.maxDistance == 0f) };
                    string space = localError < .03f && worldError >= .03f ? "renderer-local" :
                        worldError < .03f && localError >= .03f ? "world" : null;
                    if (space == null || (_spaces[i] != null && _spaces[i] != space))
                    { _reason = "Pinned authored vertices do not prove one stable particle frame";
                        if (_firstFrameFailure == null) _firstFrameFailure = (JObject)_frameEvidence[i].DeepClone();
                        if (_valid == 0) { _samples--; _warmup++; } return; }
                    _spaces[i] = space;
                    local[i] = space == "renderer-local" ? particles : particles.Select(_frames[i].InverseTransformPoint).ToArray();
                    _freeMinimum[i] = local[i][Side]; _freeMaximum[i] = local[i][Side];
                    for (int index = Side + 1; index < local[i].Length; index++)
                    { _freeMinimum[i] = Vector3.Min(_freeMinimum[i], local[i][index]); _freeMaximum[i] = Vector3.Max(_freeMaximum[i], local[i][index]); }
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
                bool positive = ObservationWindowComplete && _samples >= 36 && _valid == _samples && _disabled && _motion > .03f &&
                    _difference > .03f && _controlPenetration > .08f && _contactPenetration <= .08f;
                return new JObject { ["passed"] = positive, ["samples"] = _samples, ["validSamples"] = _valid,
                    ["initializationFramesWithoutAProvenParticleFrame"] = _warmup,
                    ["particlesPerAuthoredGrid"] = _vertices.Length, ["pinnedParticlesPerGrid"] = Side,
                    ["controlParticleFrame"] = _spaces[0], ["contactParticleFrame"] = _spaces[1],
                    ["exactActiveSolverInitialization"] = _initialization,
                    ["latestAuthoredPinnedFrameEvidence"] = new JArray(_frameEvidence.Where(value => value != null)),
                    ["firstAuthoredPinnedFrameFailure"] = _firstFrameFailure,
                    ["unitySimulationElapsedSeconds"] = Time.time - _startedTime,
                    ["unscaledElapsedSeconds"] = Time.unscaledTime - _startedUnscaledTime, ["wallElapsedSeconds"] = _elapsed.Elapsed.TotalSeconds,
                    ["sampledDeltaTimeSum"] = _deltaSum, ["minimumSampleDeltaTime"] = _samples == 0 ? 0f : _minimumDelta,
                    ["maximumSampleDeltaTime"] = _maximumDelta,
                    ["latestFreeParticleRangesAuthoredLocal"] = new JArray(Enumerable.Range(0, 2).Select(index => new JObject {
                        ["grid"] = index, ["minimum"] = Vector(_freeMinimum[index]), ["maximum"] = Vector(_freeMaximum[index]) })),
                    ["allOwnedCapsulesAlwaysDisabled"] = _disabled, ["positiveParticleMotion"] = _motion,
                    ["maximumPairedParticleDifference"] = _difference, ["noCollisionControlMaximumCapsulePenetration"] = _controlPenetration,
                    ["disabledCapsuleMaximumPenetration"] = _contactPenetration, ["reason"] = _reason,
                    ["limit"] = "Paired project-authored 9x9 no-seam grids test only disabled-capsule consumption under stated gravity/external acceleration. This does not accept native cape distance/inertia calibration or art.",
                    ["disposed"] = _disposed, ["allOwnedResourcesReleased"] = _disposed && _root == null && _meshes.All(value => value == null),
                    ["borrowedNativeMaterialAlive"] = _borrowed != null };
            }

            internal bool ObservationWindowComplete { get { return Time.time - _startedTime >= 2f && _valid >= 36; } }
            internal double WallElapsedSeconds { get { return _elapsed.Elapsed.TotalSeconds; } }

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
            string clothAdmission = ProductionClothAdmission(actor, profile);
            if (clothAdmission != "no-cloth" && clothAdmission != "qualified-frozen-cape")
                throw new NotSupportedException("Production copied-Cloth refused: " + clothAdmission);
            if (actor.View.GetComponentInChildren<Cleanup2GiantRenderScalePrototype>(true) != null)
                throw new InvalidOperationException("An existing visual scale owner cannot be replaced.");
            return TakeOwned(actor, profile.RelativeVisualFactor,
                new ProposedClothDistances(profile.RelativeVisualFactor, profile.RelativeVisualFactor,
                    clothAdmission == "no-cloth" ? "not-used-production-has-no-cloth" : "qualified-frozen-cape-relative-distance-v1"),
                "production:" + actor.UniqueId, true, NativeBodySole(actor, profile), source, profile);
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
            ProposedClothDistances proposedDistances, string identity, bool production, float nativeSoleWorldY, string productionSource = null,
            GiantSummonPresentationProfile profile = null)
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
                owner._profile = profile;
                if (production)
                {
                    owner._nativeRendererCache = actor.View.Renderers;
                    owner._nativeRendererCacheBefore = owner._nativeRendererCache?.ToArray();
                    owner._nativeColliderCache = ColliderCacheField?.GetValue(actor.View);
                    owner._nativeColliderCacheBefore = (owner._nativeColliderCache as System.Collections.IEnumerable)?.Cast<Collider>().ToArray();
                    owner._occludedHighlighter = actor.View.OccludedHighlighter;
                    owner._ordinaryHighlighter = actor.View.GetComponent<Kingmaker.Visual.UnitMultiHighlight>();
                    owner._ordinaryReceiverOwner = owner._ordinaryHighlighter == null ? null : owner._ordinaryHighlighter.Highlighter;
                    if (owner._nativeRendererCacheBefore == null || owner._nativeRendererCacheBefore.Length == 0 ||
                        owner._nativeRendererCacheBefore.Any(value => value == null || !plan.Renderers.Contains(value)) ||
                        plan.Renderers.Any(value => !owner._nativeRendererCacheBefore.Contains(value)) ||
                        owner._nativeColliderCacheBefore == null || owner._occludedHighlighter == null || owner._ordinaryHighlighter == null ||
                        owner._ordinaryReceiverOwner == null || OrdinaryReceiversField == null || UpdateOrdinaryRenderers == null ||
                        OccludedRenderersField == null || FindOccludedRenderers == null)
                        throw new NotSupportedException("Exact native renderer/collider/highlight cache intake required.");
                }
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
                if (production) owner.RegisterNativeRenderers();
                owner.RevealOwnedRenderers();
                owner._status = production ? (owner._ownedCloths.Count == 0 ? "PRODUCTION_ACTIVE_NO_CLOTH" : "PRODUCTION_ACTIVE_QUALIFIED_FROZEN_CAPE") : "EXPERIMENT_ACTIVE_COLLISION_AND_DISTANCE_CALIBRATION_UNKNOWN";
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

        private void RegisterNativeRenderers()
        {
            if (!NativeRendererCacheExact() || !NativeColliderCacheExact())
                throw new InvalidOperationException("Native cached visual/collider intake changed before owned registration.");
            _registeredRenderers = _skins.Where(value => value.Proxy != null).Select(value => (Renderer)value.Proxy)
                .Concat(_statics.Select(value => (Renderer)value.Proxy)).ToArray();
            if (_registeredRenderers.Any(value => value == null || _nativeRendererCache.Any(source => ReferenceEquals(source, value))))
                throw new InvalidOperationException("Owned renderer registration is missing or already present.");
            _nativeRendererCache.AddRange(_registeredRenderers);
            _rendererCacheRegistered = true;
            RefreshNativeHighlighters();
            if (!NativeRendererCacheExact() || !NativeColliderCacheExact() || !OccludedReceiversExact() || !OrdinaryReceiversExact())
                throw new InvalidOperationException("Owned renderer registration did not preserve exact native cache membership.");
        }

        private bool NativeColliderCacheExact()
        {
            if (!_production) return true;
            object current = _view == null || ColliderCacheField == null ? null : ColliderCacheField.GetValue(_view);
            var values = current as System.Collections.IEnumerable;
            return ReferenceEquals(current, _nativeColliderCache) && values != null &&
                Same(values.Cast<Collider>().ToArray(), _nativeColliderCacheBefore);
        }

        private bool NativeRendererCacheExact()
        {
            if (!_production) return true;
            Renderer[] expected = _rendererCacheRegistered ? _nativeRendererCacheBefore.Concat(_registeredRenderers).ToArray() : _nativeRendererCacheBefore;
            return _view != null && ReferenceEquals(_view.Renderers, _nativeRendererCache) &&
                Same(_nativeRendererCache.ToArray(), expected);
        }

        private bool OccludedReceiversExact()
        {
            if (!_production || !_rendererCacheRegistered) return true;
            var receivers = _occludedHighlighter == null ? null : OccludedRenderersField.GetValue(_occludedHighlighter) as Renderer[];
            return _view != null && ReferenceEquals(_view.OccludedHighlighter, _occludedHighlighter) && receivers != null &&
                _registeredRenderers.All(renderer => receivers.Count(value => ReferenceEquals(value, renderer)) == 1);
        }

        private void RefreshNativeHighlighters()
        {
            if (_view == null || !ReferenceEquals(_view.GetComponent<Kingmaker.Visual.UnitMultiHighlight>(), _ordinaryHighlighter) ||
                !ReferenceEquals(_ordinaryHighlighter.Highlighter, _ordinaryReceiverOwner) ||
                !ReferenceEquals(_view.OccludedHighlighter, _occludedHighlighter))
                throw new InvalidOperationException("Native highlight owner was replaced; no foreign refresh.");
            // These exact native seams only invalidate/build visual receiver
            // caches. Never call UpdateCachedRenderersAndColliders: that would
            // add the disabled visual capsules to the gameplay collider cache.
            _view.MarkRenderersAndCollidersAreUpdated();
            // Installed Highlighter.UpdateRenderers consumes its dirty flag,
            // rebuilds highlight-only receivers/materials and reads original
            // sharedMaterials. It never assigns a native PF material array.
            UpdateOrdinaryRenderers.Invoke(_ordinaryReceiverOwner, null);
            FindOccludedRenderers.Invoke(_occludedHighlighter, null);
            _highlightRefreshes++;
        }

        private bool ExpectedNativeEnabled(Renderer renderer, bool intake)
        { return _production && _nativeVisibilityObserved && _nativeRendererCacheBefore.Contains(renderer) ? _nativeVisible : intake; }

        internal static void NativeVisibilityChanged(EntityFader fader, bool visible)
        {
            UnitEntityView view = fader == null ? null : fader.GetComponent<UnitEntityView>();
            if (view == null || !ReferenceEquals(view.GetComponent<EntityFader>(), fader)) return;
            foreach (var owner in view.GetComponentsInChildren<Cleanup2GiantRenderScalePrototype>(true).Where(value =>
                value != null && value._production && !value._disposed && value._committed && value._rendererCacheRegistered && ReferenceEquals(value._view, view)))
            {
                try
                {
                    // The installed fader has just written this exact cached
                    // list. Validate every non-visibility field before taking
                    // responsibility for its demonstrated enabled transition.
                    owner.RequireSourceIdentity(true, visible);
                    owner._nativeVisibilityObserved = true; owner._nativeVisible = visible; owner._nativeVisibilityWrites++;
                    foreach (Skin skin in owner._skins.Where(value => value.Cloth != null)) skin.Source.enabled = false;
                    foreach (StaticMesh mesh in owner._statics) mesh.Source.enabled = false;
                    owner.RequireSourceIdentity(true);
                }
                catch (Exception error)
                {
                    owner._status = "REFUSED:native-visibility:" + error.GetType().Name + ":" + error.Message;
                    owner.Dispose();
                }
            }
        }

        private void UnregisterNativeRenderers()
        {
            if (!_production || !_rendererCacheRegistered) return;
            if (_view != null && !ReferenceEquals(_view.Renderers, _nativeRendererCache))
                _restoreIssues.Add("foreign-native-renderer-cache-instance-preserved");
            _nativeRendererCache.RemoveAll(renderer => _registeredRenderers.Any(value => ReferenceEquals(value, renderer)));
            _rendererCacheRegistered = false;
            if (!Same(_nativeRendererCache.ToArray(), _nativeRendererCacheBefore))
                _restoreIssues.Add("foreign-native-renderer-cache-contents-preserved");
            if (_occludedHighlighter != null)
            {
                var current = OccludedRenderersField.GetValue(_occludedHighlighter) as Renderer[];
                if (current != null) OccludedRenderersField.SetValue(_occludedHighlighter,
                    current.Where(renderer => !_registeredRenderers.Any(value => ReferenceEquals(value, renderer))).ToArray());
            }
            if (!NativeColliderCacheExact()) _restoreIssues.Add("foreign-native-collider-cache-preserved");
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

        private void RequireSourceIdentity(bool committed, bool? authenticNativeVisibility = null)
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
            if (_production && (!NativeRendererCacheExact() || !NativeColliderCacheExact() || !OccludedReceiversExact() || !OrdinaryReceiversExact()))
                throw new InvalidOperationException("Native visual/collider receiver cache ownership changed.");
            foreach (Skin skin in _skins)
            {
                Transform[] bones = committed && skin.Cloth == null ? skin.AppliedBones : skin.Bones;
                Transform root = committed && skin.Cloth == null ? skin.AppliedRoot : skin.RootBone;
                bool expectedEnabled = ExpectedNativeEnabled(skin.Source, skin.Enabled);
                if (authenticNativeVisibility.HasValue && _nativeRendererCacheBefore.Contains(skin.Source)) expectedEnabled = authenticNativeVisibility.Value;
                else if (committed && skin.Cloth != null) expectedEnabled = false;
                if (skin.Source == null || skin.Source.sharedMesh != skin.Mesh || !Same(skin.Source.sharedMaterials, skin.Materials) ||
                    !Same(skin.Source.bones, bones) || skin.Source.rootBone != root || skin.Source.enabled != expectedEnabled ||
                    (skin.Cloth != null && !skin.Cloth.Unchanged()))
                    throw new InvalidOperationException("Foreign native skin/material/palette/Cloth edit; prototype refuses continuation.");
            }
            foreach (StaticMesh mesh in _statics)
                if (mesh.Source == null || mesh.Filter == null || mesh.Filter.sharedMesh != mesh.Mesh ||
                    !Same(mesh.Source.sharedMaterials, mesh.Materials) || mesh.Source.enabled !=
                    (authenticNativeVisibility.HasValue ? authenticNativeVisibility.Value : committed ? false : ExpectedNativeEnabled(mesh.Source, mesh.Enabled)))
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
            if (_production && committed && _rendererCacheRegistered)
            {
                foreach (Skin skin in _skins.Where(value => value.Proxy != null))
                    if (skin.Proxy.enabled != (authenticNativeVisibility ?? ExpectedNativeEnabled(skin.Source, skin.Enabled)))
                        throw new InvalidOperationException("Owned cape renderer visibility changed outside the native fader.");
                foreach (StaticMesh mesh in _statics)
                    if (mesh.Proxy.enabled != (authenticNativeVisibility ?? ExpectedNativeEnabled(mesh.Source, mesh.Enabled)))
                        throw new InvalidOperationException("Owned static renderer visibility changed outside the native fader.");
            }
            bool initializedSolver = committed || _clothInitializedBeforeSourceCommit;
            if (initializedSolver)
                foreach (Skin skin in _skins.Where(value => value.Cloth != null))
                    if (!skin.Cloth.MatchesOwned(skin.Proxy.GetComponent<Cloth>(), skin.Cloth.Capsules.Select(value => _capsules[value]).ToArray(), _distances))
                        throw new InvalidOperationException("Owned solver exact cloned configuration changed after initialization.");
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
                result["retainedForeignRendererCacheReferences"] = _retainedRendererCacheReferences;
                result["ownedRootReleased"] = this == null || gameObject == null;
                return result;
            }
            return Report();
        }

        internal JObject ObserveProduction()
        {
            JObject receipt = Describe();
            receipt["ownerPresent"] = true;
            receipt["observerCalledApplyOrSynchronize"] = false;
            bool exact = false; string reason = null;
            if (_production && !_disposed)
                try { RequireSourceIdentity(true); exact = true; }
                catch (Exception error) { reason = error.GetType().Name + ":" + error.Message; }
            receipt["currentOwnershipExact"] = exact;
            receipt["ordinaryOwnedReceiversExact"] = OrdinaryReceiversExact();
            receipt["ownershipFailure"] = reason;
            receipt["ownedRootInstance"] = this == null || gameObject == null ? 0 : gameObject.GetInstanceID();
            receipt["privateAllocationInstances"] = new JArray(_mirrors.Values.Where(value => value != null)
                .Select(value => value.gameObject.GetInstanceID()).Concat(_ownedCloths.Where(value => value != null).Select(value => value.GetInstanceID()))
                .Concat(_registeredRenderers.Where(value => value != null).Select(value => value.GetInstanceID()))
                .Concat(_capsules.Values.Where(value => value != null).Select(value => value.GetInstanceID())).Distinct());
            // Sole is a current-pose observation of the exact admitted body,
            // never a fitted absolute human/feet calibration or contact PASS.
            if (exact && _profile != null)
                try { receipt["currentBodySoleWorldY"] = NativeBodySole(_actor, _profile); }
                catch (Exception error) { receipt["currentSoleObservationFailure"] = error.GetType().Name + ":" + error.Message; }
            return receipt;
        }

        private bool OrdinaryReceiversExact()
        {
            if (!_production || !_rendererCacheRegistered) return true;
            var values = _ordinaryReceiverOwner == null ? null : OrdinaryReceiversField.GetValue(_ordinaryReceiverOwner) as System.Collections.IEnumerable;
            if (values == null) return false;
            Component[] receivers = values.Cast<Component>().Where(value => value != null).ToArray();
            Renderer[] visibleSurfaces = _registeredRenderers.Concat(_skins.Where(value => value.Cloth == null).Select(value => (Renderer)value.Source)).ToArray();
            return visibleSurfaces.All(renderer => receivers.Count(value => ReferenceEquals(value.GetComponent<Renderer>(), renderer)) == 1);
        }

        private void TryRetireOwnedRoot()
        {
            if (_production && !_highlightRetirementReady)
            {
                if (_view == null || _registeredRenderers.Length == 0) _highlightRetirementReady = true;
                else try
                {
                    RefreshNativeHighlighters();
                    _highlightRetirementReady = true;
                }
                catch (Exception) { /* Keep the exact owned allocations alive. */ }
            }
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
            // Foreign caches may legitimately retain exact borrowed renderer
            // references. Never destroy those allocations out from under a
            // receiver outside this owner's original native cache.
            _retainedRendererCacheReferences = Resources.FindObjectsOfTypeAll<UnitEntityView>().Count(view => view != null && view.Renderers != null &&
                view.Renderers.Any(renderer => _registeredRenderers.Any(owned => ReferenceEquals(owned, renderer))));
            _retainedRendererCacheReferences += Resources.FindObjectsOfTypeAll<OccludedObjectHighlighter>().Count(highlighter => {
                var receivers = OccludedRenderersField == null ? null : OccludedRenderersField.GetValue(highlighter) as Renderer[];
                return receivers != null && receivers.Any(renderer => _registeredRenderers.Any(owned => ReferenceEquals(owned, renderer))); });
            _retainedRendererCacheReferences += Resources.FindObjectsOfTypeAll<HighlightingSystem.Highlighter>().Count(highlighter => {
                var receivers = OrdinaryReceiversField == null ? null : OrdinaryReceiversField.GetValue(highlighter) as System.Collections.IEnumerable;
                return receivers != null && receivers.Cast<Component>().Any(component => component != null &&
                    _registeredRenderers.Any(owned => ReferenceEquals(owned, component.GetComponent<Renderer>()))); });
            _retirementPending = _retainedSkinReferences + _retainedClothReferences + _retainedAnchorReferences + _retainedRendererCacheReferences > 0 ||
                (_production && !_highlightRetirementReady);
            if (_retirementPending)
            {
                // Only our root moves. Keep its world pose and exact resources
                // alive even if the former native view is subsequently freed.
                if (transform.parent != null) transform.SetParent(null, true);
                if (!_restoreIssues.Contains("owned-mirrors-retained-for-foreign-references"))
                    _restoreIssues.Add("owned-mirrors-retained-for-foreign-references");
                return;
            }
            // No receiver retains these private allocations. Drop only their
            // exact borrowed material bindings before deferred destruction so
            // an independent palette owner can release in this same frame.
            foreach (Skin skin in _skins.Where(value => value.Proxy != null))
                if (Same(skin.Proxy.sharedMaterials, skin.Materials)) skin.Proxy.sharedMaterials = new Material[0];
            foreach (StaticMesh mesh in _statics.Where(value => value.Proxy != null))
                if (Same(mesh.Proxy.sharedMaterials, mesh.Materials)) mesh.Proxy.sharedMaterials = new Material[0];
            if (gameObject != null) { gameObject.SetActive(false); UnityEngine.Object.Destroy(gameObject); }
        }

        private JObject Report()
        {
            return new JObject { ["runId"] = _runId, ["actor"] = _actor == null ? null : _actor.UniqueId,
                ["status"] = _status, ["uniformVisualFactor"] = _factor, ["syncCount"] = _syncCount,
                ["productionOwner"] = _production, ["productionCopiedClothEnabled"] = _production && _ownedCloths.Count != 0,
                ["copiedClothProductionGateOpen"] = ProductionCopiedClothEnabled,
                ["nativeRendererCacheRegistered"] = _rendererCacheRegistered,
                ["nativeRendererCacheExact"] = NativeRendererCacheExact(), ["nativeColliderCacheExact"] = NativeColliderCacheExact(),
                ["occludedOwnedReceiversExact"] = OccludedReceiversExact(),
                ["ordinaryOwnedReceiversExact"] = OrdinaryReceiversExact(),
                ["registeredOwnedRenderers"] = new JArray(_registeredRenderers.Select(value => value == null ? 0 : value.GetInstanceID())),
                ["nativeVisibilityTransitionObserved"] = _nativeVisibilityObserved, ["nativeVisibilityWrites"] = _nativeVisibilityWrites,
                ["lastNativeVisibility"] = _nativeVisibilityObserved ? (JToken)new JValue(_nativeVisible) : JValue.CreateNull(),
                ["nativeHighlightRefreshes"] = _highlightRefreshes, ["sourceStateRestored"] = _sourceStateRestored,
                ["borrowedMaterialReferencesExact"] = _skins.All(value => value.Source != null && Same(value.Source.sharedMaterials, value.Materials)) &&
                    _statics.All(value => value.Source != null && Same(value.Source.sharedMaterials, value.Materials)),
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
                    ["coefficientCount"] = value == null ? 0 : value.coefficients.Length,
                    ["pinnedCount"] = value == null ? 0 : value.coefficients.Count(coefficient => coefficient.maxDistance == 0f),
                    ["capsulesAllDisabled"] = value != null && value.capsuleColliders.All(c => c != null && !c.enabled) })),
                ["ordinarySkinRetargetCount"] = _skins.Count(value => value.Cloth == null), ["staticProxyCount"] = _statics.Count,
                ["maxDistanceMultiplier"] = _distances.MaxDistanceMultiplier,
                ["collisionSphereDistanceMultiplier"] = _distances.CollisionSphereDistanceMultiplier,
                ["distanceProposal"] = _distances.Proposal,
                ["disabledCapsuleCollisionResponse"] = ProductionCopiedClothEnabled ? "QUALIFIED_EXACT_AUTHORED_NATIVE_CONTACT_CONTROL" : "UNKNOWN_REQUIRES_NATIVE_CONTACT_PROOF",
                ["clothDistanceCalibration"] = _production ? (_ownedCloths.Count == 0 ? "NO_CLOTH" : "EXACT_QUALIFIED_FROZEN_PROFILE") : "EXPERIMENTAL_NOT_ACCEPTED",
                ["renderAndHighlightEquivalence"] = _production ? "NATIVE_RECEIVER_REGISTRATION_REQUIRES_VISIBLE_REVIEW" : "UNVERIFIED_NATIVE_CAPTURE_REQUIRED",
                ["restoreIssues"] = new JArray(_restoreIssues),
                ["scope"] = "Instance-owned relative visual scale. Original Cloth, collider, skeleton/root transforms, mesh assets, materials and animation are preserved. Ordinary skin palette/bounds, displaced renderer visibility and exact native visual receiver membership are conditionally restored." };
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
                            skin.Source.enabled = ExpectedNativeEnabled(skin.Source, skin.Enabled);
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
                        mesh.Source.enabled = ExpectedNativeEnabled(mesh.Source, mesh.Enabled);
                    else _restoreIssues.Add("foreign-static-renderer:" + mesh.Source.GetInstanceID());
                }
                catch (Exception error) { _restoreIssues.Add("static:" + error.GetType().Name + ":" + error.Message); }
            // Disable only owned solver/renderers before deferred destruction.
            foreach (Cloth cloth in _ownedCloths) if (cloth != null) cloth.enabled = false;
            foreach (Skin skin in _skins) if (skin.Proxy != null) { skin.Proxy.enabled = false; skin.Proxy.gameObject.SetActive(false); }
            foreach (StaticMesh mesh in _statics) if (mesh.Proxy != null) { mesh.Proxy.enabled = false; mesh.Proxy.gameObject.SetActive(false); }
            // Native Highlighter.GrabRenderers recursively visits even
            // inactive children. Detach only our now-hidden root before its
            // refresh, so the receiver census genuinely excludes allocations
            // awaiting retirement without changing a native transform.
            if (_production && transform.parent != null) transform.SetParent(null, true);
            try
            {
                UnregisterNativeRenderers();
                _sourceStateRestored = _skins.All(skin => skin.Source != null && skin.Source.sharedMesh == skin.Mesh &&
                    Same(skin.Source.bones, skin.Bones) && skin.Source.rootBone == skin.RootBone && (skin.Cloth != null || skin.Source.localBounds.Equals(skin.Bounds)) &&
                    skin.Source.enabled == ExpectedNativeEnabled(skin.Source, skin.Enabled) && (skin.Cloth == null || skin.Cloth.Unchanged())) &&
                    _statics.All(mesh => mesh.Source != null && mesh.Filter != null && mesh.Filter.sharedMesh == mesh.Mesh &&
                        mesh.Source.enabled == ExpectedNativeEnabled(mesh.Source, mesh.Enabled)) && NativeColliderCacheExact() && NativeRendererCacheExact();
                if (_production)
                {
                    // The detached root update owner remains alive through
                    // Unity's deferred private allocation retirement.
                    RefreshNativeHighlighters();
                }
                _highlightRetirementReady = true;
            }
            catch (Exception error) { _restoreIssues.Add("native-visual-cache-release:" + error.GetType().Name + ":" + error.Message); }
            try { _lastReport = Report(); _lastReport["disposed"] = true; }
            catch (Exception error) { _lastReport = new JObject { ["status"] = _status, ["disposed"] = true, ["reportError"] = error.Message, ["restoreIssues"] = new JArray(_restoreIssues) }; }
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
