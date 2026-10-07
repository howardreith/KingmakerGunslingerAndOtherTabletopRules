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
        // borrowed references and bone-local points privately; never render,
        // export, change, rebind or dispose a native mesh/transform.
        private sealed class Sprint17HumanGripSurface
        {
            private sealed class Point
            {
                internal Transform[] Bones;
                internal Vector3[] Local;
                internal float[] Weights;
            }
            private readonly Point[][] _hands;
            private readonly Mesh _source;
            internal Sprint17HumanGripSurface(Mesh mesh, Transform[] bones)
            {
                if (mesh == null || bones == null || bones.Length == 0 || bones.Length > 4096 ||
                    mesh.vertexCount < 8 || mesh.vertexCount > 200000 || bones.Any(b => b == null))
                    throw new InvalidOperationException("Unbounded or missing exact hand surface.");
                _source = mesh;
                var vertices = mesh.vertices; var weights = mesh.boneWeights; var bind = mesh.bindposes;
                if (vertices.Length != weights.Length || bones.Length != bind.Length)
                    throw new InvalidOperationException("Hand surface palette mismatch.");
                _hands = new Point[2][];
                for (int side = 0; side < 2; side++)
                {
                    var selected = new List<Point>();
                    for (int v = 0; v < vertices.Length; v++)
                    {
                        BoneWeight w = weights[v];
                        int[] ids = { w.boneIndex0, w.boneIndex1, w.boneIndex2, w.boneIndex3 };
                        float[] values = { w.weight0, w.weight1, w.weight2, w.weight3 };
                        if (ids.Any(id => id < 0 || id >= bones.Length))
                            throw new InvalidOperationException("Invalid hand surface bone index.");
                        if (!SalamanderHumanBindingPolicy.IsGripSurfaceVertex(side == 0 ? "L" : "R",
                            ids.Select(id => bones[id].name).ToArray(), values)) continue;
                        int[] used = Enumerable.Range(0, 4).Where(i => values[i] > 0).ToArray();
                        selected.Add(new Point { Bones = used.Select(i => bones[ids[i]]).ToArray(),
                            Local = used.Select(i => bind[ids[i]].MultiplyPoint3x4(vertices[v])).ToArray(),
                            Weights = used.Select(i => values[i]).ToArray() });
                    }
                    if (selected.Count < 8 || selected.Count > 8192)
                        throw new InvalidOperationException("Exact native/original hand surface unavailable.");
                    _hands[side] = selected.ToArray();
                }
            }

            internal JObject Census()
            {
                return new JObject { ["mesh"] = _source.name, ["meshId"] = _source.GetInstanceID(),
                    ["leftVertices"] = _hands[0].Length, ["rightVertices"] = _hands[1].Length,
                    ["geometryExported"] = false, ["poseWritten"] = false };
            }

            internal float[] Gaps(Vector3 start, Vector3 axis)
            {
                if (_source == null || axis.sqrMagnitude <= 0)
                    throw new InvalidOperationException("Borrowed hand or native spear unavailable.");
                var frames = _hands.SelectMany(hand => hand).SelectMany(point => point.Bones)
                    .Distinct().ToDictionary(bone => bone, bone => bone.localToWorldMatrix);
                var result = new float[2];
                for (int side = 0; side < 2; side++)
                {
                    float gap = float.PositiveInfinity;
                    foreach (var point in _hands[side])
                    {
                        Vector3 world = Vector3.zero;
                        for (int i = 0; i < point.Bones.Length; i++)
                            world += frames[point.Bones[i]].MultiplyPoint3x4(point.Local[i]) * point.Weights[i];
                        float distance = Vector3.Distance(world, start + axis *
                            Mathf.Clamp01(Vector3.Dot(world - start, axis) / axis.sqrMagnitude));
                        if (!SalamanderTailAnimationPolicy.Finite(distance))
                            throw new InvalidOperationException("Non-finite same-pose hand control.");
                        gap = Mathf.Min(gap, distance);
                    }
                    result[side] = gap;
                }
                return result;
            }
        }

        private sealed class Sprint17HumanGripObservation
        {
            private readonly Sprint17HumanGripSurface _native, _original;
            private readonly MeshFilter _spear;
            private readonly UnitAttack _command;
            private readonly List<object> _handles = new List<object>();
            private readonly JArray _events = new JArray(), _timeline = new JArray();
            internal readonly JObject Evidence;
            internal string Failure;
            internal Sprint17HumanGripObservation(Sprint17HumanGripSurface native,
                SkinnedMeshRenderer original, MeshFilter spear, UnitAttack command)
            {
                _native = native; _original = new Sprint17HumanGripSurface(original.sharedMesh, original.bones);
                _spear = spear; _command = command;
                Evidence = new JObject { ["scope"] = "same actor/bones/spear/frame; CPU-only borrowed native hand control; no native geometry export",
                    ["native"] = native.Census(), ["original"] = _original.Census(),
                    ["nativeEvents"] = _events, ["timeline"] = _timeline, ["truncated"] = false };
            }

            internal JObject Read(string phase)
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
                    _events.Add(NativeEventMetadata(active, handleId, clip.name));
                }
                Bounds bounds = _spear.sharedMesh.bounds;
                Vector3 start = _spear.transform.TransformPoint(bounds.center - Vector3.up * bounds.extents.y);
                Vector3 end = _spear.transform.TransformPoint(bounds.center + Vector3.up * bounds.extents.y);
                float[] native = _native.Gaps(start, end - start), original = _original.Gaps(start, end - start);
                return new JObject { ["frame"] = Time.frameCount, ["phase"] = phase,
                    ["handleId"] = handleId, ["attackIndex"] = _command.GetAttackIndex(),
                    ["handleTime"] = handle.GetTime(), ["acted"] = handle.IsActed,
                    ["finished"] = handle.IsFinished, ["clip"] = clip.name,
                    ["clipTime"] = active.GetTime(), ["state"] = active.State.ToString(),
                    ["weight"] = active.GetWeight(), ["nativeLeft"] = native[0], ["nativeRight"] = native[1],
                    ["originalLeft"] = original[0], ["originalRight"] = original[1],
                    ["available"] = true };
            }

            internal void ObserveFrame(string phase)
            {
                if (Failure != null || _command.IsFinished) return;
                try
                {
                    if (_timeline.Count >= 512) { Evidence["truncated"] = true; return; }
                    JObject sample = Read(phase);
                    if (sample != null) _timeline.Add(sample);
                }
                catch (Exception error) { Evidence["failure"] = Failure = error.ToString(); }
            }

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
