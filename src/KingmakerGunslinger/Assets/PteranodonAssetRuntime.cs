using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    /// <summary>
    /// Loads and validates the Pteranodon replacement visual once per process.
    ///
    /// The asset is mesh data - vertices, normals, triangles, vertex weights,
    /// and the ordered list of bone NAMES those weights index - not an
    /// AssetBundle. Three reasons, in order of weight:
    ///
    /// - It carries strictly less. A bundle would embed bind poses, a material
    ///   and import settings; this carries our own geometry plus the donor's
    ///   bone names, which are the binding contract and are already recorded in
    ///   the native audit. No donor transform ships.
    /// - It depends on no Unity editor licence and no specific editor version.
    ///   That is not hypothetical: the 2018.4.10f1 install that built every
    ///   previous bundle in this repository stopped accepting its licence
    ///   between 2026-08-21 and 2026-09-23 and now demands account credentials
    ///   to re-activate.
    /// - It is less code on both sides than the bundle path it replaces.
    ///
    /// Bind poses are supplied at attach time from the live donor, which is the
    /// only correct binding as well as the redistribution-safe one. Unity skins
    /// a vertex as
    ///
    ///     v_world = sum_i w_i * bones[i].localToWorldMatrix * bindposes[i] * v
    ///
    /// so bind poses derived from the donor's pose at attach time would make the
    /// mesh render as authored in whatever animation frame the unit happened to
    /// be on. The donor's live pose differs from its bind pose by up to 3.954
    /// units at the wingtip - folded wings against spread ones - which would
    /// misplace the creature differently on every summon. Reusing the donor's
    /// own sharedMesh.bindposes, the frame these vertices were authored in,
    /// makes the binding deterministic.
    ///
    /// Every failure path here leaves the donor visual intact, which is the
    /// approved fallback: a Pteranodon that looks like a giant eagle is a
    /// cosmetic shortfall, an invisible or half-bound one is a defect.
    /// </summary>
    internal static class PteranodonAssetRuntime
    {
        internal const string MeshDataRelativePath =
            "assets/pteranodon/pteranodon-mesh.json";
        internal const int SupportedSchemaVersion = 1;

        /// <summary>
        /// The bones the mesh may bind to: six shared, and twenty per side - the
        /// wing chain, the leg, and eight toe bones.
        ///
        /// This is not redundant with resolving them against the donor. That
        /// proves a name exists; this proves the generator has not started
        /// weighting geometry to a bone nobody reviewed. Absent on purpose:
        /// Tail_end, Tail_L, Tail_R and every *_end leaf. The donor's eagle tail
        /// fan carries no geometry, so a weight landing there would mean a fan
        /// had crept back in.
        /// </summary>
        private static readonly string[] AllowedBones =
        {
            "LowerTorso", "UpperTorso", "Neck", "Head", "Jaw", "Tail",
            "L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Feather_1", "L_Feather_2",
            "L_Feather_3", "L_Feather_4", "L_Feather_5", "L_Feather_6",
            "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0", "L_Finger_1_1",
            "L_Finger_1_2", "L_Finger_2_1", "L_Finger_2_2", "L_Finger_3_1",
            "L_Finger_3_2", "L_Finger_4_1", "L_Finger_4_2", "R_Arm_Upper",
            "R_Arm_Lower", "R_Palm", "R_Feather_1", "R_Feather_2", "R_Feather_3",
            "R_Feather_4", "R_Feather_5", "R_Feather_6", "R_Leg0_Upper",
            "R_Leg0_Lower", "R_Foot0", "R_Finger_1_1", "R_Finger_1_2",
            "R_Finger_2_1", "R_Finger_2_2", "R_Finger_3_1", "R_Finger_3_2",
            "R_Finger_4_1", "R_Finger_4_2"
        };

        private static readonly object Sync = new object();
        private static Mesh _mesh;
        private static string[] _boneNames;
        private static string _status = "donor-visual:not-configured";

        internal static string Status { get { lock (Sync) return _status; } }

        internal static bool HasValidatedMesh
        { get { lock (Sync) return _mesh != null && _boneNames != null; } }

        /// <summary>The validated mesh and the bone names its weights index.</summary>
        internal static bool TryGetMembrane(out Mesh mesh, out string[] boneNames)
        {
            lock (Sync)
            {
                mesh = _mesh;
                boneNames = _boneNames == null ? null :
                    (string[])_boneNames.Clone();
                return mesh != null && boneNames != null;
            }
        }

        internal static void Configure(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) _status = "donor-visual:module-disabled";
                context.Logger.Info("pteranodon", "mesh.skipped",
                    "Expanded Summoning is disabled; the donor visual remains active.");
                return;
            }

            lock (Sync)
            {
                if (_mesh != null && _boneNames != null)
                {
                    context.Logger.Info("pteranodon", "mesh.reused",
                        "The validated Pteranodon mesh is already published.");
                    return;
                }
            }

            string path = Path.Combine(context.ModEntry.Path,
                MeshDataRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                lock (Sync) _status = "donor-visual:mesh-data-missing";
                context.Logger.Warning("pteranodon", "mesh.missing",
                    "The Pteranodon mesh data is unavailable; the donor visual remains active: " + path);
                return;
            }

            try
            {
                string[] names;
                Mesh mesh = BuildMesh(File.ReadAllText(path), out names);
                lock (Sync)
                {
                    _mesh = mesh;
                    _boneNames = names;
                    _status = "mesh:published";
                }

                context.Logger.Info("pteranodon", "mesh.published",
                    "Validated the Pteranodon mesh: vertices=" +
                    mesh.vertexCount + ";triangles=" +
                    (mesh.triangles.Length / 3) + ";bones=" + names.Length);
            }
            catch (Exception error)
            {
                lock (Sync)
                {
                    _mesh = null;
                    _boneNames = null;
                    _status = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning("pteranodon", "mesh.rejected",
                    "The Pteranodon mesh data was rejected; the donor visual remains active: " +
                    error.Message);
            }
        }

        /// <summary>
        /// Builds the mesh, validating everything before a donor renderer could
        /// ever be touched. Bind poses are left at identity: the attach path
        /// supplies the donor's own.
        /// </summary>
        internal static Mesh BuildMesh(string json, out string[] boneNames)
        {
            JObject document = JObject.Parse(json);
            int schema = (int?)document["schemaVersion"] ?? 0;
            if (schema != SupportedSchemaVersion)
                throw new InvalidDataException(
                    "Unsupported Pteranodon mesh schema " + schema + "; expected " +
                    SupportedSchemaVersion + ".");

            var names = ((JArray)document["bones"] ?? new JArray())
                .Select(value => (string)value).ToArray();
            if (names.Length == 0)
                throw new InvalidDataException("The bone list is empty.");
            if (names.Any(string.IsNullOrWhiteSpace))
                throw new InvalidDataException("The bone list has a blank name.");
            if (names.Distinct(StringComparer.Ordinal).Count() != names.Length)
                throw new InvalidDataException("The bone list repeats a name.");
            string[] unexpected = names.Where(value =>
                !AllowedBones.Contains(value, StringComparer.Ordinal)).ToArray();
            if (unexpected.Length != 0)
                throw new InvalidDataException(
                    "The mesh binds to bones outside the declared set: " +
                    string.Join(", ", unexpected));

            int vertexCount = (int?)document["vertexCount"] ?? 0;
            int triangleCount = (int?)document["triangleCount"] ?? 0;
            if (vertexCount <= 0 || triangleCount <= 0)
                throw new InvalidDataException(
                    "The mesh declares " + vertexCount + " vertices and " +
                    triangleCount + " triangles.");

            byte[] blob = Convert.FromBase64String((string)document["data"] ?? string.Empty);
            int expected = vertexCount * 12 + vertexCount * 12 +
                triangleCount * 3 * 4 + vertexCount * 4 * 8;
            if (blob.Length != expected)
                throw new InvalidDataException(
                    "The mesh payload is " + blob.Length + " bytes; " +
                    expected + " were expected for " + vertexCount +
                    " vertices and " + triangleCount + " triangles.");

            int offset = 0;
            var vertices = new Vector3[vertexCount];
            for (int index = 0; index < vertexCount; index++)
                vertices[index] = ReadVector(blob, ref offset);
            var normals = new Vector3[vertexCount];
            for (int index = 0; index < vertexCount; index++)
                normals[index] = ReadVector(blob, ref offset);
            var triangles = new int[triangleCount * 3];
            for (int index = 0; index < triangles.Length; index++)
            {
                triangles[index] = BitConverter.ToInt32(blob, offset);
                offset += 4;
                if (triangles[index] < 0 || triangles[index] >= vertexCount)
                    throw new InvalidDataException(
                        "A triangle indexes vertex " + triangles[index] +
                        " of " + vertexCount + ".");
            }

            var weights = new BoneWeight[vertexCount];
            for (int index = 0; index < vertexCount; index++)
            {
                var slots = new int[4];
                var values = new float[4];
                float total = 0f;
                for (int slot = 0; slot < 4; slot++)
                {
                    slots[slot] = BitConverter.ToInt32(blob, offset);
                    offset += 4;
                    values[slot] = BitConverter.ToSingle(blob, offset);
                    offset += 4;
                    total += values[slot];
                    if (values[slot] <= 0f) continue;
                    if (slots[slot] < 0 || slots[slot] >= names.Length)
                        throw new InvalidDataException(
                            "A vertex weight indexes bone " + slots[slot] +
                            " of " + names.Length + ".");
                }

                if (Math.Abs(total - 1f) > 0.001f)
                    throw new InvalidDataException(
                        "A vertex weight sums to " + total + ".");
                weights[index] = new BoneWeight
                {
                    boneIndex0 = slots[0], weight0 = values[0],
                    boneIndex1 = slots[1], weight1 = values[1],
                    boneIndex2 = slots[2], weight2 = values[2],
                    boneIndex3 = slots[3], weight3 = values[3]
                };
            }

            var mesh = new Mesh { name = "KMG_Pteranodon" };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.triangles = triangles;
            mesh.boneWeights = weights;
            // Identity bind poses on purpose: the attach path replaces them with
            // the donor's own, and shipping any other value would be shipping a
            // pose that is wrong by construction.
            var identity = new Matrix4x4[names.Length];
            for (int index = 0; index < names.Length; index++)
                identity[index] = Matrix4x4.identity;
            mesh.bindposes = identity;
            mesh.RecalculateBounds();

            boneNames = names;
            return mesh;
        }

        private static Vector3 ReadVector(byte[] blob, ref int offset)
        {
            float x = BitConverter.ToSingle(blob, offset);
            float y = BitConverter.ToSingle(blob, offset + 4);
            float z = BitConverter.ToSingle(blob, offset + 8);
            offset += 12;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Resolves the bone transforms and bind poses for one donor renderer.
        ///
        /// Returns false and changes nothing if any declared bone is absent from
        /// the donor's skinned bone array, which is the only place a bind pose
        /// for it could come from.
        /// </summary>
        internal static bool TryResolveDonorBinding(
            SkinnedMeshRenderer donor, string[] boneNames,
            out Transform[] bones, out Matrix4x4[] bindposes, out string reason)
        {
            bones = null;
            bindposes = null;
            reason = null;
            if (donor == null) { reason = "donor-renderer-missing"; return false; }
            if (boneNames == null || boneNames.Length == 0)
            { reason = "no-bone-names"; return false; }
            Transform[] donorBones = donor.bones;
            Mesh donorMesh = donor.sharedMesh;
            if (donorBones == null || donorBones.Length == 0)
            { reason = "donor-has-no-bones"; return false; }
            if (donorMesh == null)
            { reason = "donor-has-no-mesh"; return false; }
            Matrix4x4[] donorBind = donorMesh.bindposes;
            if (donorBind == null || donorBind.Length != donorBones.Length)
            { reason = "donor-bind-pose-count-mismatch"; return false; }

            var index = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int slot = 0; slot < donorBones.Length; slot++)
            {
                if (donorBones[slot] == null) continue;
                // A duplicated bone name would make the mapping ambiguous.
                if (index.ContainsKey(donorBones[slot].name))
                {
                    reason = "donor-bone-name-ambiguous:" + donorBones[slot].name;
                    return false;
                }

                index[donorBones[slot].name] = slot;
            }

            var resolvedBones = new Transform[boneNames.Length];
            var resolvedBind = new Matrix4x4[boneNames.Length];
            for (int slot = 0; slot < boneNames.Length; slot++)
            {
                int donorSlot;
                if (!index.TryGetValue(boneNames[slot], out donorSlot))
                { reason = "donor-bone-missing:" + boneNames[slot]; return false; }
                resolvedBones[slot] = donorBones[donorSlot];
                resolvedBind[slot] = donorBind[donorSlot];
            }

            bones = resolvedBones;
            bindposes = resolvedBind;
            return true;
        }
    }
}
