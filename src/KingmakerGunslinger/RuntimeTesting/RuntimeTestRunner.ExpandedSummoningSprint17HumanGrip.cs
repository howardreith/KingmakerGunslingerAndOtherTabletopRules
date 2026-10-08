using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Visual.Animation;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // CPU-only, request-local native/original surface comparison. Keep
        // borrowed references and weights privately; never render,
        // export, change, rebind or dispose a native mesh/transform.
        private sealed class Sprint17HumanGripSurface : IDisposable
        {
            private readonly int[][] _hands;
            private readonly Mesh _source;
            private readonly SkinnedMeshRenderer _anchor;
            private readonly JObject _metadata;
            private GameObject _control;
            private SkinnedMeshRenderer _baker;
            private Mesh _snapshot;

            internal Sprint17HumanGripSurface(SkinnedMeshRenderer anchor, List<UnityEngine.Object> resources, JObject metadata,
                bool nativeDeformers = false)
            {
                if (anchor == null || anchor.name != SalamanderHumanBindingPolicy.BodyName || resources == null || metadata == null)
                    throw new InvalidOperationException("Exact guarded human body required.");
                _anchor = anchor; _source = anchor.sharedMesh; _metadata = metadata;
                Transform[] bones = anchor.bones;
                if (_source == null || bones.Length == 0 || bones.Length > 4096 ||
                    _source.vertexCount < 8 || _source.vertexCount > 200000 || bones.Any(b => b == null))
                    throw new InvalidOperationException("Unbounded or missing exact hand surface.");
                // Imported combined meshes can reject vertices while exposing
                // weights/binds. Do not access native vertices or change its
                // import flags: BakeMesh fills only a new owned snapshot.
                var weights = _source.boneWeights;
                metadata["mesh"] = _source.name; metadata["vertices"] = _source.vertexCount;
                metadata["weights"] = weights.Length; metadata["bones"] = bones.Length;
                metadata["binds"] = _source.bindposes.Length; metadata["nativeCpuReadable"] = _source.isReadable;
                metadata["geometryExported"] = false; metadata["weightsChanged"] = false;
                metadata["available"] = false; metadata["ownedResources"] = 0;
                metadata["driverFamily"] = nativeDeformers ? "exact native hand ADJ children" : "original hand animation drivers";
                if (weights.Length != _source.vertexCount || bones.Length != _source.bindposes.Length)
                    throw new InvalidOperationException("Hand metadata mismatch: vertices=" + _source.vertexCount +
                        ";weights=" + weights.Length + ";bones=" + bones.Length +
                        ";binds=" + _source.bindposes.Length + ";readable=" + _source.isReadable);
                if (nativeDeformers)
                {
                    if (_source.name != "Bandit_FighterLeader_Renderer_Character_Diffuse_Cutout" ||
                        _source.vertexCount != 2268 || bones.Length != 1776 || bones.Distinct().Count() != 177)
                        throw new InvalidOperationException("Unreviewed native deforming control mesh/palette.");
                    var map = new JArray(); metadata["nativeHandDeformers"] = map;
                    Matrix4x4[] binds = _source.bindposes;
                    foreach (string driver in SalamanderHumanBindingPolicy.NativeNames.Where(name =>
                        SalamanderHumanBindingPolicy.IsGripDriver(name, "L") || SalamanderHumanBindingPolicy.IsGripDriver(name, "R")))
                    {
                        int[] slots = Enumerable.Range(0, bones.Length).Where(i => bones[i].name == driver + "_ADJ").ToArray();
                        Transform[] parents = bones.Where(b => b.name == driver).Distinct().ToArray();
                        bool valid = slots.Length > 0 && parents.Length == 1;
                        int first = slots.Length == 0 ? -1 : slots[0];
                        if (valid) valid = slots.All(i => ReferenceEquals(bones[i], bones[first]) &&
                            ReferenceEquals(bones[i].parent, parents[0]) &&
                            SalamanderHumanBindingPolicy.NativeGripDeformerDriver(bones[i].name, bones[i].parent.name) == driver &&
                            SalamanderTailAnimationPolicy.Finite(binds[i].determinant) && Math.Abs(binds[i].determinant) > .0000001f &&
                            SalamanderHumanBindingPolicy.BakeFrameMatches(
                                Enumerable.Range(0, 16).Select(k => binds[i][k]).ToArray(),
                                Enumerable.Range(0, 16).Select(k => binds[first][k]).ToArray()));
                        map.Add(new JObject { ["native"] = driver + "_ADJ", ["driver"] = driver,
                            ["slots"] = slots.Length, ["exactParentIdentityAndBind"] = valid });
                        if (!valid) throw new InvalidOperationException("Unreviewed native hand ADJ parent/identity/bind: " + driver);
                    }
                }
                var selections = new[] { new List<int>(), new List<int>() };
                var reasons = new[] { new Dictionary<string, int>(), new Dictionary<string, int>() };
                var usedBones = new Dictionary<string, int>();
                var positive = new int[2]; var maximum = new float[2];
                var driverVertices = SalamanderHumanBindingPolicy.NativeNames.Where(name =>
                    SalamanderHumanBindingPolicy.IsGripDriver(name, "L") ||
                    SalamanderHumanBindingPolicy.IsGripDriver(name, "R")).ToDictionary(name => name, name => 0);
                int finiteSums = 0, zeroSums = 0;
                float minimumSum = float.PositiveInfinity, maximumSum = float.NegativeInfinity;
                for (int v = 0; v < weights.Length; v++)
                {
                    BoneWeight w = weights[v];
                    int[] ids = { w.boneIndex0, w.boneIndex1, w.boneIndex2, w.boneIndex3 };
                    float[] values = { w.weight0, w.weight1, w.weight2, w.weight3 };
                    bool validIds = ids.All(id => id >= 0 && id < bones.Length);
                    string[] names = validIds ? ids.Select(id => nativeDeformers ?
                        SalamanderHumanBindingPolicy.NativeGripDeformerDriver(bones[id].name,
                            bones[id].parent == null ? null : bones[id].parent.name) : bones[id].name).ToArray() : null;
                    float sum = values.Sum();
                    if (SalamanderTailAnimationPolicy.Finite(sum))
                    {
                        finiteSums++; if (sum == 0) zeroSums++;
                        minimumSum = Mathf.Min(minimumSum, sum); maximumSum = Mathf.Max(maximumSum, sum);
                    }
                    if (validIds)
                    {
                        foreach (string raw in Enumerable.Range(0, 4).Where(i => values[i] > 0)
                            .Select(i => bones[ids[i]].name).Distinct())
                        { if (!usedBones.ContainsKey(raw)) usedBones[raw] = 0; usedBones[raw]++; }
                        foreach (string name in driverVertices.Keys.ToArray())
                            if (Enumerable.Range(0, 4).Any(i => names[i] == name && values[i] > 0)) driverVertices[name]++;
                    }
                    for (int side = 0; side < 2; side++)
                    {
                        string label = side == 0 ? "L" : "R";
                        string reason = validIds ? SalamanderHumanBindingPolicy.GripSurfaceDisposition(label, names, values) : "bone-index";
                        if (!reasons[side].ContainsKey(reason)) reasons[side][reason] = 0;
                        reasons[side][reason]++;
                        if (reason == "selected") selections[side].Add(v);
                        if (!validIds) continue;
                        float influence = Enumerable.Range(0, 4).Where(i =>
                            SalamanderHumanBindingPolicy.IsGripDriver(names[i], label)).Sum(i => values[i]);
                        if (SalamanderTailAnimationPolicy.Finite(influence))
                        { if (influence > 0) positive[side]++; maximum[side] = Mathf.Max(maximum[side], influence); }
                    }
                }
                _hands = selections.Select(list => list.ToArray()).ToArray();
                metadata["finiteWeightSums"] = finiteSums; metadata["zeroWeightSums"] = zeroSums;
                metadata["minimumWeightSum"] = finiteSums == 0 ? (float?)null : minimumSum;
                metadata["maximumWeightSum"] = finiteSums == 0 ? (float?)null : maximumSum;
                metadata["positiveWeightDriverVertices"] = Sprint17GripEvidence.Counters(driverVertices);
                metadata["positiveRawBoneVertices"] = Sprint17GripEvidence.Counters(usedBones);
                metadata["selection"] = new JArray(Enumerable.Range(0, 2).Select(side => new JObject {
                    ["side"] = side == 0 ? "L" : "R", ["selected"] = _hands[side].Length,
                    ["positiveInfluenceVertices"] = positive[side], ["maximumInfluence"] = maximum[side],
                    ["dispositions"] = Sprint17GripEvidence.Counters(reasons[side]) }));
                if (_hands.Any(hand => hand.Length < 8 || hand.Length > 8192))
                    throw new InvalidOperationException("Exact hand surface unavailable: L=" + _hands[0].Length + ";R=" + _hands[1].Length);
                try
                {
                    // Outside the live view so native renderer/material census
                    // cannot adopt it. No UnitEntityView, Animator or material.
                    // All transform writes below target only this owned control.
                    _control = new GameObject("KMG_Runtime_Sprint17_GripControl_" + _source.GetInstanceID());
                    resources.Add(_control); resources.Add(_control.transform);
                    _baker = _control.AddComponent<SkinnedMeshRenderer>(); resources.Add(_baker);
                    _baker.enabled = false;
                    _baker.sharedMesh = _source; _baker.bones = bones; _baker.rootBone = anchor.rootBone;
                    _baker.quality = anchor.quality; _baker.updateWhenOffscreen = false;
                    _snapshot = new Mesh { name = _control.name + "_Snapshot" }; resources.Add(_snapshot);
                    metadata["ownedResources"] = 4; metadata["available"] = true;
                }
                catch { Dispose(); throw; }
            }

            internal JObject Census()
            {
                var result = (JObject)_metadata.DeepClone();
                result.Merge(new JObject { ["mesh"] = _source.name, ["meshId"] = _source.GetInstanceID(),
                    ["nativeCpuReadable"] = _source.isReadable, ["vertices"] = _source.vertexCount,
                    ["leftVertices"] = _hands[0].Length, ["rightVertices"] = _hands[1].Length,
                    ["quality"] = _baker.quality.ToString(), ["controlEnabled"] = _baker.enabled,
                    ["controlOutsideLiveView"] = !_control.transform.IsChildOf(_anchor.transform),
                    ["ownedResources"] = 4, ["method"] = "owned disabled renderer CPU BakeMesh; live anchor frame",
                    ["geometryExported"] = false, ["nativePoseWritten"] = false });
                return result;
            }

            internal float[] Gaps(Vector3 start, Vector3 axis)
            {
                if (_source == null || _anchor == null || _baker == null || _snapshot == null ||
                    _baker.enabled || !ReferenceEquals(_baker.sharedMesh, _source) || axis.sqrMagnitude <= 0)
                    throw new InvalidOperationException("Exact non-rendering hand control unavailable.");
                Transform frame = _anchor.transform;
                _control.transform.position = frame.position;
                _control.transform.rotation = frame.rotation;
                _control.transform.localScale = frame.lossyScale;
                Matrix4x4 actual = _control.transform.localToWorldMatrix, expected = frame.localToWorldMatrix;
                if (!SalamanderHumanBindingPolicy.BakeFrameMatches(
                    Enumerable.Range(0, 16).Select(i => actual[i]).ToArray(),
                    Enumerable.Range(0, 16).Select(i => expected[i]).ToArray()))
                    throw new InvalidOperationException("Native hand frame has shear or changed scale; do not approximate.");
                _baker.BakeMesh(_snapshot);
                Vector3[] vertices = _snapshot.vertices;
                if (vertices.Length != _source.vertexCount)
                    throw new InvalidOperationException("Incomplete owned baked hand snapshot: " + vertices.Length +
                        "/" + _source.vertexCount);
                var result = new float[2];
                for (int side = 0; side < 2; side++)
                {
                    float gap = float.PositiveInfinity;
                    foreach (int index in _hands[side])
                    {
                        Vector3 world = actual.MultiplyPoint3x4(vertices[index]);
                        float distance = Vector3.Distance(world, start + axis *
                            Mathf.Clamp01(Vector3.Dot(world - start, axis) / axis.sqrMagnitude));
                        if (!SalamanderTailAnimationPolicy.Finite(distance))
                            throw new InvalidOperationException("Non-finite same-pose baked hand control.");
                        gap = Mathf.Min(gap, distance);
                    }
                    result[side] = gap;
                }
                return result;
            }

            public void Dispose()
            {
                if (_baker != null) { _baker.sharedMesh = null; _baker.bones = new Transform[0]; _baker.rootBone = null; }
                if (_snapshot != null) UnityEngine.Object.Destroy(_snapshot);
                if (_control != null) UnityEngine.Object.Destroy(_control);
                _baker = null; _snapshot = null; _control = null;
            }
        }

        private sealed class Sprint17HumanGripObservation : IDisposable
        {
            private readonly Sprint17HumanGripSurface _native, _original;
            private readonly MeshFilter _spear;
            private readonly UnitAttack _command;
            private readonly List<object> _handles = new List<object>();
            private readonly HashSet<Sprint17HumanGripSurface> _failedSurfaces = new HashSet<Sprint17HumanGripSurface>();
            private readonly JArray _events = new JArray(), _timeline = new JArray();
            internal readonly JObject Evidence;
            internal string Failure;
            internal Sprint17HumanGripObservation(Sprint17HumanGripSurface native,
                SkinnedMeshRenderer original, MeshFilter spear, UnitAttack command, List<UnityEngine.Object> resources,
                JObject nativeMetadata)
            {
                _native = native;
                _spear = spear; _command = command;
                Evidence = new JObject { ["scope"] = "same actor/bones/spear/frame; CPU-only borrowed native hand control; no native geometry export",
                    ["native"] = native == null ? nativeMetadata.DeepClone() : native.Census(),
                    ["nativeEvents"] = _events, ["timeline"] = _timeline, ["truncated"] = false };
                if (native == null) Fail("Native same-pose hand control unavailable; see selection census.");
                var originalMetadata = new JObject(); Evidence["original"] = originalMetadata;
                try
                {
                    _original = new Sprint17HumanGripSurface(original, resources, originalMetadata);
                    Evidence["original"] = _original.Census();
                }
                catch (Exception error) { originalMetadata["failure"] = error.ToString(); Fail(error.ToString()); }
            }

            internal JObject Read(string phase)
            {
                try { return ReadCore(phase); }
                catch (Exception error)
                {
                    Fail(error.ToString());
                    return new JObject { ["frame"] = Time.frameCount, ["phase"] = phase,
                        ["available"] = false, ["failure"] = error.ToString() };
                }
            }

            private void Fail(string reason)
            {
                if (Failure == null) Evidence["failure"] = Failure = reason;
            }

            private float[] ReadGaps(Sprint17HumanGripSurface surface, Vector3 start, Vector3 axis)
            {
                if (surface == null || _failedSurfaces.Contains(surface)) return null;
                try { return surface.Gaps(start, axis); }
                catch (Exception error) { _failedSurfaces.Add(surface); Fail(error.ToString()); return null; }
            }

            private JObject ReadCore(string phase)
            {
                var handle = _command.Animation;
                var active = handle == null ? null : handle.ActiveAnimation;
                var clip = active == null ? null : active.GetPlayableClip();
                if (handle == null || clip == null ||
                    handle.AttackWeaponStyle.ToString() != "PiercingTwoHanded") return null;
                int handleId = _handles.FindIndex(value => ReferenceEquals(value, handle));
                if (handleId < 0)
                {
                    if (_handles.Count >= 8) throw new InvalidOperationException("Unbounded spear handle census.");
                    handleId = _handles.Count; _handles.Add(handle);
                    try { _events.Add(NativeEventMetadata(active, handleId, clip.name)); }
                    catch (Exception error) { Fail(error.ToString()); _events.Add(new JObject {
                        ["handleId"] = handleId, ["clip"] = clip.name, ["failure"] = error.ToString() }); }
                }
                Bounds bounds = _spear.sharedMesh.bounds;
                Vector3 start = _spear.transform.TransformPoint(bounds.center - Vector3.up * bounds.extents.y);
                Vector3 end = _spear.transform.TransformPoint(bounds.center + Vector3.up * bounds.extents.y);
                float[] native = ReadGaps(_native, start, end - start), original = ReadGaps(_original, start, end - start);
                return new JObject { ["frame"] = Time.frameCount, ["phase"] = phase,
                    ["handleId"] = handleId, ["attackIndex"] = _command.GetAttackIndex(),
                    ["handleTime"] = handle.GetTime(), ["acted"] = handle.IsActed,
                    ["finished"] = handle.IsFinished, ["clip"] = clip.name,
                    ["clipTime"] = active.GetTime(), ["state"] = active.State.ToString(),
                    ["weight"] = active.GetWeight(), ["nativeLeft"] = native == null ? (float?)null : native[0],
                    ["nativeRight"] = native == null ? (float?)null : native[1],
                    ["originalLeft"] = original == null ? (float?)null : original[0],
                    ["originalRight"] = original == null ? (float?)null : original[1],
                    ["available"] = native != null && original != null && Failure == null };
            }

            internal void ObserveFrame(string phase)
            {
                if (_command.IsFinished) return;
                try
                {
                    if (_timeline.Count >= 512) { Evidence["truncated"] = true; return; }
                    JObject sample = Read(phase);
                    if (sample != null) _timeline.Add(sample);
                }
                catch (Exception error) { Fail(error.ToString()); }
            }

            public void Dispose() { if (_original != null) _original.Dispose(); }

            private static JObject NativeEventMetadata(AnimationBase active, int handleId, string clip)
            {
                // Read the existing per-playable cache only. Calling the
                // cache provider or invoking an event would change state.
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                if (active.GetType() != typeof(PlayableInfo))
                    throw new InvalidOperationException("Unreviewed native spear playback type.");
                var field = typeof(PlayableInfo).GetField("m_Events", flags);
                var events = field == null ? null : field.GetValue(active) as Array;
                if (events == null || events.Length > 64)
                    throw new InvalidOperationException("Exact native event cache unavailable.");
                var rows = new JArray();
                foreach (object entry in events)
                {
                    if (entry == null) throw new InvalidOperationException("Null native animation event.");
                    var type = entry.GetType();
                    var time = type.GetProperty("Time", flags);
                    var actionField = type.GetField("Event", flags);
                    var action = actionField == null ? null : actionField.GetValue(entry) as Delegate;
                    if (time == null || action == null)
                        throw new InvalidOperationException("Native event metadata shape changed.");
                    rows.Add(new JObject { ["time"] = (float)time.GetValue(entry, null),
                        ["method"] = action.Method.DeclaringType.FullName + "." + action.Method.Name });
                }
                return new JObject { ["handleId"] = handleId, ["clip"] = clip, ["events"] = rows,
                    ["eventInvoked"] = false, ["cacheProviderInvoked"] = false };
            }
        }
    }

    // Only present on this owned, guarded disposable actor. Neither callback
    // mutates a pose or executes an animation. No global hooks.
    internal sealed class Sprint17HumanGripFrameProbe : MonoBehaviour
    {
        internal Action<string> Observe;
        private void LateUpdate()
        {
            if (Observe == null) return;
            Observe("LateUpdate");
            StartCoroutine(AtEndOfFrame());
        }
        private IEnumerator AtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            if (Observe != null) Observe("EndOfFrame");
        }
    }
}
