using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>One explicit disposable giant-review Human construction lease.
    /// Character.BuildMesh discards its CPU positions with UploadMeshData(true).
    /// Observe that same mesh immediately before the unchanged native upload;
    /// never rebuild/modify an actor, keep a prefab mesh, or export geometry.</summary>
    internal sealed class Cleanup2CharacterCpuReference : IDisposable
    {
        private sealed class Reference
        {
            internal Character Character;
            internal UnitEntityView View;
            internal UnitEntityData Unit;
            internal Mesh Mesh;
            internal SkinnedMeshRenderer Renderer;
            internal Vector3[] Positions;
            internal BoneWeight[] Weights;
            internal Matrix4x4[] Binds;
            internal Transform[] Bones;
            internal string Digest;
        }

        private static Cleanup2CharacterCpuReference Active;
        private readonly BlueprintUnit _blueprint;
        private readonly string _runId;
        private readonly HarmonyInstance _hooks;
        private readonly List<Reference> _references = new List<Reference>();
        private readonly List<string> _failures = new List<string>();
        private UnitEntityData _unit;
        private bool _disposed;

        private static readonly MethodInfo Upload = typeof(Mesh).GetMethod(
            "UploadMeshData", new[] { typeof(bool) });
        private static readonly MethodInfo ObserveUpload = typeof(Cleanup2CharacterCpuReference).GetMethod(
            "ObserveThenUpload", BindingFlags.Static | BindingFlags.NonPublic);

        private Cleanup2CharacterCpuReference(RuntimeTestRequest request, BlueprintUnit blueprint)
        {
            _blueprint = blueprint; _runId = request.RunId;
            _hooks = HarmonyInstance.Create("kmg.cleanup2.character-cpu." + _runId);
        }

        // Pure admission is also used by the no-game behavior test. There is
        // no request/purpose override and no production automatic activation.
        internal static bool RequestAllowed(RuntimeTestRequest request)
        {
            string phase = RuntimeTestRequestParser.Cleanup2ReviewPhase(request);
            return request != null && request.ExitAfterCompletion && !string.IsNullOrEmpty(request.RunId) &&
                (phase == "giant-lineup" || phase == "scale-prototype");
        }

        internal static Cleanup2CharacterCpuReference Begin(RuntimeTestRequest request,
            BlueprintUnit requestOwnedHumanBlueprint)
        {
            if (!RequestAllowed(request))
                throw new InvalidOperationException("Exact automatic-exit disposable Cleanup2 giant request required.");
            if (Active != null) throw new InvalidOperationException("Another CPU-reference lease is active.");
            BlueprintUnit native = BlueprintRoot.Instance.DefaultPlayerCharacter;
            if (requestOwnedHumanBlueprint == null || native == null ||
                ReferenceEquals(requestOwnedHumanBlueprint, native) ||
                requestOwnedHumanBlueprint.AssetGuid != native.AssetGuid ||
                requestOwnedHumanBlueprint.Race == null ||
                requestOwnedHumanBlueprint.Race.AssetGuid != "0a5d473ead98b0646b94495af250fdc4")
                throw new InvalidOperationException("Exact request-owned DefaultPlayerCharacter clone with Human race required.");
            var lease = new Cleanup2CharacterCpuReference(request, requestOwnedHumanBlueprint);
            MethodInfo target = typeof(Character).GetMethod("BuildMesh", BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(Dictionary<BodyPart, EquipmentEntity>) }, null);
            if (target == null || target.ReturnType != typeof(void) || Upload == null || ObserveUpload == null ||
                target.Module.ModuleVersionId != new Guid("07fa1e4d-8618-41b3-9b8d-faa17d3b26f7"))
                throw new NotSupportedException("Exact installed 2.1.7b Character.BuildMesh/upload contract unavailable.");
            Active = lease;
            try
            {
                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
                lease._hooks.Patch(target, null,
                    new HarmonyMethod(typeof(Cleanup2CharacterCpuReference).GetMethod("BuildComplete", flags)),
                    new HarmonyMethod(typeof(Cleanup2CharacterCpuReference).GetMethod("RewriteUpload", flags)));
                return lease;
            }
            catch
            {
                lease._hooks.UnpatchAll(lease._hooks.Id); Active = null;
                throw;
            }
        }

        internal static IEnumerable<CodeInstruction> RewriteUpload(IEnumerable<CodeInstruction> instructions)
        {
            var source = instructions.ToList();
            int count = source.Count(item => item.opcode == OpCodes.Callvirt && Equals(item.operand, Upload));
            if (count != 1) throw new NotSupportedException("Expected one native Character mesh upload, observed " + count + ".");
            var result = new List<CodeInstruction>();
            for (int index = 0; index < source.Count; index++)
            {
                CodeInstruction item = source[index];
                if (item.opcode == OpCodes.Callvirt && Equals(item.operand, Upload))
                {
                    if (index == 0 || source[index - 1].opcode != OpCodes.Ldc_I4_1)
                        throw new NotSupportedException("Native Character upload must discard CPU data with literal true.");
                    // Existing stack: Mesh, bool. Add the actual Character;
                    // this wrapper always calls the original upload exactly once.
                    var owner = new CodeInstruction(OpCodes.Ldarg_0);
                    owner.labels.AddRange(item.labels); item.labels.Clear();
                    result.Add(owner); item.opcode = OpCodes.Call; item.operand = ObserveUpload;
                }
                result.Add(item);
            }
            return result;
        }

        private static void ObserveThenUpload(Mesh mesh, bool discard, Character character)
        {
            Cleanup2CharacterCpuReference lease = Active;
            if (lease != null && !lease._disposed)
            {
                try { lease.Capture(mesh, discard, character); }
                catch (Exception error) { lease.Fail("capture", error); }
            }
            // This is the original native call and original argument. No
            // catch suppresses a native error or changes readability lifetime.
            mesh.UploadMeshData(discard);
        }

        private UnitEntityView OwnedView(Character character)
        {
            var view = character == null ? null : character.GetComponentInParent<UnitEntityView>();
            var unit = view == null ? null : view.EntityData;
            if (unit == null || !ReferenceEquals(unit.Blueprint, _blueprint)) return null;
            if (!ReferenceEquals(view.CharacterAvatar, character) ||
                !ReferenceEquals(view.Data, unit) || (_unit != null && !ReferenceEquals(_unit, unit)))
                throw new InvalidOperationException("Request Human Character/view/unit ownership mismatch.");
            _unit = unit;
            return view;
        }

        private void Capture(Mesh mesh, bool discard, Character character)
        {
            UnitEntityView view = OwnedView(character);
            if (view == null) return; // foreign characters remain entirely native
            if (!discard || mesh == null || mesh.name != "Character" || !mesh.isReadable ||
                _references.Count >= 32 || _references.Any(item => ReferenceEquals(item.Mesh, mesh)))
                throw new InvalidOperationException("Current Human pre-upload mesh identity/readability/count refused.");
            Vector3[] positions = mesh.vertices;
            BoneWeight[] weights = mesh.boneWeights;
            Matrix4x4[] binds = mesh.bindposes;
            if (positions == null || positions.Length == 0 || positions.Length > 100000 ||
                weights == null || weights.Length != positions.Length || binds == null ||
                binds.Length == 0 || binds.Length > 8192 || positions.Any(point => !Finite(point)))
                throw new InvalidOperationException("Incomplete/invalid bounded pre-upload CPU records.");
            for (int i = 0; i < binds.Length; i++)
                for (int j = 0; j < 16; j++)
                    if (!Finite(binds[i][j])) throw new InvalidOperationException("Non-finite pre-upload bind.");
            _references.Add(new Reference { Character = character, View = view, Unit = view.EntityData,
                Mesh = mesh, Positions = positions, Weights = weights, Binds = binds,
                Digest = Digest(positions, weights, binds) });
        }

        private static void BuildComplete(Character __instance)
        {
            Cleanup2CharacterCpuReference lease = Active;
            if (lease == null || lease._disposed) return;
            try { lease.Complete(__instance); }
            catch (Exception error) { lease.Fail("complete", error); }
        }

        private void Complete(Character character)
        {
            UnitEntityView view = OwnedView(character);
            if (view == null) return;
            SkinnedMeshRenderer[] renderers = character.Renderers.ToArray();
            if (renderers.Length == 0 || renderers.Length > 16)
                throw new InvalidOperationException("Current Character renderer census outside bounds.");
            foreach (Reference reference in _references.Where(item => ReferenceEquals(item.Character, character) && item.Renderer == null))
            {
                var exact = renderers.Where(renderer => renderer != null &&
                    ReferenceEquals(renderer.sharedMesh, reference.Mesh)).ToArray();
                if (exact.Length != 1 || !exact[0].transform.IsChildOf(view.transform))
                    throw new InvalidOperationException("Pre-upload mesh not assigned to one exact owned renderer.");
                reference.Renderer = exact[0]; reference.Bones = exact[0].bones;
                if (reference.Bones == null || reference.Bones.Length != reference.Binds.Length)
                    throw new InvalidOperationException("Generated renderer palette differs from captured bind count.");
            }
        }

        internal static bool TryGetExactSource(UnitEntityView view, SkinnedMeshRenderer renderer,
            out Vector3[] positions, out JObject receipt)
        {
            positions = null;
            Cleanup2CharacterCpuReference lease = Active;
            receipt = new JObject { ["available"] = false, ["reason"] = "no-active-request-owned-reference" };
            if (lease == null || lease._disposed || view == null || renderer == null) return false;
            Reference[] matches = lease._references.Where(item => ReferenceEquals(item.View, view) &&
                ReferenceEquals(item.Renderer, renderer) && ReferenceEquals(item.Mesh, renderer.sharedMesh)).ToArray();
            receipt = lease.Describe();
            receipt["available"] = false;
            if (lease._failures.Count != 0 || matches.Length != 1 ||
                !ReferenceEquals(view.EntityData, lease._unit) || !ReferenceEquals(view.EntityData.Blueprint, lease._blueprint))
            { receipt["reason"] = "missing-ambiguous-failed-or-foreign-reference"; return false; }
            Reference reference = matches[0];
            Matrix4x4[] binds = renderer.sharedMesh.bindposes;
            BoneWeight[] weights = renderer.sharedMesh.boneWeights;
            bool exact = reference.Bones != null && renderer.bones.SequenceEqual(reference.Bones) &&
                EqualBinds(reference.Binds, binds) && EqualWeights(reference.Weights, weights) &&
                renderer.sharedMesh.vertexCount == reference.Positions.Length &&
                reference.Character.Renderers.Any(value => ReferenceEquals(value, renderer));
            receipt["meshInstance"] = renderer.sharedMesh.GetInstanceID();
            receipt["vertexCount"] = reference.Positions.Length;
            receipt["weightCount"] = reference.Weights.Length; receipt["bindCount"] = reference.Binds.Length;
            receipt["orderedCpuRecordsSha256"] = reference.Digest;
            receipt["sameAssignedMeshPaletteWeightsBinds"] = exact;
            receipt["available"] = exact; receipt["reason"] = exact ? "exact-pre-upload-current-source" : "current-source-mutated";
            if (!exact) return false;
            positions = (Vector3[])reference.Positions.Clone();
            return true;
        }

        internal JObject Describe()
        {
            return new JObject { ["runId"] = _runId, ["captures"] = _references.Count,
                ["assignedCaptures"] = _references.Count(item => item.Renderer != null),
                ["exactUnit"] = _unit == null ? null : _unit.UniqueId,
                ["failures"] = new JArray(_failures),
                ["scope"] = "Transient ordered current Character.BuildMesh records before unchanged UploadMeshData(true). No geometry export, prefab/actor rebuild, readability change, bounds or bake-convention substitution." };
        }

        private void Fail(string stage, Exception error)
        {
            if (_failures.Count < 16) _failures.Add(stage + ":" + error.GetType().Name + ":" +
                (error.Message.Length <= 384 ? error.Message : error.Message.Substring(0, 384)));
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        private static bool Finite(Vector3 value) { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
        private static bool EqualBinds(Matrix4x4[] first, Matrix4x4[] second)
        {
            if (first == null || second == null || first.Length != second.Length) return false;
            for (int i = 0; i < first.Length; i++) for (int j = 0; j < 16; j++)
                if (!SameFloat(first[i][j], second[i][j])) return false;
            return true;
        }
        private static bool EqualWeights(BoneWeight[] first, BoneWeight[] second)
        {
            if (first == null || second == null || first.Length != second.Length) return false;
            for (int i = 0; i < first.Length; i++)
            {
                BoneWeight a = first[i], b = second[i];
                if (a.boneIndex0 != b.boneIndex0 || a.boneIndex1 != b.boneIndex1 || a.boneIndex2 != b.boneIndex2 ||
                    a.boneIndex3 != b.boneIndex3 || !SameFloat(a.weight0, b.weight0) || !SameFloat(a.weight1, b.weight1) ||
                    !SameFloat(a.weight2, b.weight2) || !SameFloat(a.weight3, b.weight3)) return false;
            }
            return true;
        }
        private static bool SameFloat(float first, float second)
        { return BitConverter.ToInt32(BitConverter.GetBytes(first), 0) == BitConverter.ToInt32(BitConverter.GetBytes(second), 0); }
        private static string Digest(Vector3[] points, BoneWeight[] weights, Matrix4x4[] binds)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(points.Length); writer.Write(binds.Length);
                for (int i = 0; i < points.Length; i++)
                {
                    writer.Write(points[i].x); writer.Write(points[i].y); writer.Write(points[i].z);
                    BoneWeight w = weights[i];
                    writer.Write(w.boneIndex0); writer.Write(w.weight0); writer.Write(w.boneIndex1); writer.Write(w.weight1);
                    writer.Write(w.boneIndex2); writer.Write(w.weight2); writer.Write(w.boneIndex3); writer.Write(w.weight3);
                }
                foreach (Matrix4x4 bind in binds) for (int i = 0; i < 16; i++) writer.Write(bind[i]);
                writer.Flush();
                using (var sha = SHA256.Create())
                    return BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-", "").ToLowerInvariant();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (!ReferenceEquals(Active, this)) throw new InvalidOperationException("CPU-reference lease ownership lost.");
            _hooks.UnpatchAll(_hooks.Id);
            Active = null; _disposed = true;
            foreach (Reference item in _references)
            { item.Positions = null; item.Weights = null; item.Binds = null; item.Bones = null; }
            _references.Clear(); _unit = null;
        }
    }
}
