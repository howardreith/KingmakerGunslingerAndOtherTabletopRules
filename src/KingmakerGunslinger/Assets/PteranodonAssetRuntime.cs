using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    /// <summary>
    /// Loads and validates the Pteranodon replacement visual once per process.
    ///
    /// The bundle carries original geometry, its vertex weights, and the ordered
    /// list of bone NAMES those weights index. It deliberately carries no bone
    /// transforms: the mesh's bind poses are normalised to identity when the
    /// bundle is built and rebuilt from the live donor at attach time.
    ///
    /// That is not only a redistribution precaution. Unity skins a vertex as
    ///
    ///     v_world = sum_i w_i * bones[i].localToWorldMatrix * bindposes[i] * v
    ///
    /// so bind poses derived from the donor's pose at attach time would make the
    /// mesh render as authored in whatever animation frame the unit happened to
    /// be on. The donor's live pose differs from its bind pose by up to 3.954
    /// units at the wingtip - folded wings against spread ones - which would
    /// misplace the membrane differently on every summon. Reusing the donor's
    /// own sharedMesh.bindposes, the frame the vertices were authored in, makes
    /// the binding deterministic.
    ///
    /// Every failure path here leaves the donor visual intact, which is the
    /// approved fallback: a Pteranodon that looks like a giant eagle is a
    /// cosmetic shortfall, an invisible or half-bound one is a defect.
    /// </summary>
    internal static class PteranodonAssetRuntime
    {
        internal const string BundleName = "kingmakergunslinger.pteranodon";
        internal const string MeshAssetName = "pteranodonmesh";
        internal const string BonesAssetName = "pteranodonbones";

        /// <summary>
        /// Bones the membrane may bind to. The bundle is rejected if it names
        /// anything else, so a drifted asset cannot silently ask the loader to
        /// resolve a bone whose role nobody checked.
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
        private static AssetBundle _bundle;
        private static Mesh _mesh;
        private static string[] _boneNames;
        private static string _status = "donor-visual:not-configured";

        internal static string Status { get { lock (Sync) return _status; } }

        internal static bool HasValidatedMesh
        { get { lock (Sync) return _mesh != null && _boneNames != null; } }

        internal static AssetBundle GetLoadedBundleForGuardedAttribution()
        { lock (Sync) return _bundle; }

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
                context.Logger.Info("pteranodon", "bundle.skipped",
                    "Expanded Summoning is disabled; the donor visual remains active.");
                return;
            }

            lock (Sync)
            {
                if (_bundle != null && _mesh != null && _boneNames != null)
                {
                    context.Logger.Info("pteranodon", "bundle.reused",
                        "The validated Pteranodon membrane is already published.");
                    return;
                }
            }

            string path = Path.Combine(context.ModEntry.Path, "assets",
                "bundles", BundleName);
            if (!File.Exists(path))
            {
                lock (Sync) _status = "donor-visual:bundle-missing";
                context.Logger.Warning("pteranodon", "bundle.missing",
                    "The Pteranodon bundle is unavailable; the donor visual remains active: " + path);
                return;
            }

            AssetBundle candidate = null;
            try
            {
                candidate = AssetBundle.LoadFromFile(path);
                if (candidate == null)
                    throw new InvalidDataException(
                        "AssetBundle.LoadFromFile returned null for " + path);

                Mesh mesh = candidate.LoadAsset<Mesh>(MeshAssetName);
                TextAsset bones = candidate.LoadAsset<TextAsset>(BonesAssetName);
                string[] names = ValidateBundle(mesh, bones);

                lock (Sync)
                {
                    _bundle = candidate;
                    _mesh = mesh;
                    _boneNames = names;
                    _status = "membrane:published";
                }
                candidate = null;
                context.Logger.Info("pteranodon", "bundle.published",
                    "Validated the Pteranodon membrane: vertices=" +
                    mesh.vertexCount + ";bones=" + names.Length);
            }
            catch (Exception error)
            {
                lock (Sync)
                {
                    _bundle = null;
                    _mesh = null;
                    _boneNames = null;
                    _status = "donor-visual:invalid-bundle";
                }
                context.Logger.Warning("pteranodon", "bundle.rejected",
                    "The Pteranodon bundle was rejected; the donor visual remains active: " +
                    error.Message);
            }
            finally
            {
                if (candidate != null) candidate.Unload(true);
            }
        }

        /// <summary>
        /// Everything that must hold before a single donor renderer is touched.
        /// </summary>
        private static string[] ValidateBundle(Mesh mesh, TextAsset bones)
        {
            if (mesh == null) throw new InvalidDataException(
                "The bundle has no mesh named " + MeshAssetName + ".");
            if (bones == null) throw new InvalidDataException(
                "The bundle has no bone list named " + BonesAssetName + ".");
            if (!mesh.isReadable) throw new InvalidDataException(
                "The membrane mesh is not readable, so its bind poses cannot be rebuilt.");
            if (mesh.vertexCount == 0) throw new InvalidDataException(
                "The membrane mesh has no vertices.");
            if (mesh.triangles == null || mesh.triangles.Length == 0)
                throw new InvalidDataException(
                    "The membrane mesh has no triangles.");

            BoneWeight[] weights = mesh.boneWeights;
            if (weights == null || weights.Length != mesh.vertexCount)
                throw new InvalidDataException(
                    "The membrane mesh is not fully weighted: " +
                    (weights == null ? 0 : weights.Length) + " of " +
                    mesh.vertexCount + ".");

            string[] names = bones.text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => value.Length != 0).ToArray();
            if (names.Length == 0) throw new InvalidDataException(
                "The membrane bone list is empty.");
            if (names.Distinct(StringComparer.Ordinal).Count() != names.Length)
                throw new InvalidDataException(
                    "The membrane bone list repeats a name.");

            string[] unexpected = names.Where(value =>
                !AllowedBones.Contains(value, StringComparer.Ordinal)).ToArray();
            if (unexpected.Length != 0) throw new InvalidDataException(
                "The membrane binds to bones outside the declared set: " +
                string.Join(", ", unexpected));

            Matrix4x4[] bindposes = mesh.bindposes;
            if (bindposes == null || bindposes.Length != names.Length)
                throw new InvalidDataException(
                    "The membrane has " + (bindposes == null ? 0 :
                    bindposes.Length) + " bind poses for " + names.Length +
                    " bones.");
            // Donor transforms must not ship. A bundle that carries them is
            // rejected rather than silently used, because a stale baked pose is
            // exactly the failure the runtime rebind exists to remove.
            for (int index = 0; index < bindposes.Length; index++)
            {
                if (bindposes[index] != Matrix4x4.identity)
                    throw new InvalidDataException(
                        "The membrane ships a non-identity bind pose at index " +
                        index + "; bind poses are rebuilt from the live donor.");
            }

            foreach (BoneWeight weight in weights)
            {
                int[] indexes = { weight.boneIndex0, weight.boneIndex1,
                    weight.boneIndex2, weight.boneIndex3 };
                float[] values = { weight.weight0, weight.weight1,
                    weight.weight2, weight.weight3 };
                float total = 0f;
                for (int slot = 0; slot < 4; slot++)
                {
                    total += values[slot];
                    if (values[slot] <= 0f) continue;
                    if (indexes[slot] < 0 || indexes[slot] >= names.Length)
                        throw new InvalidDataException(
                            "A membrane vertex weight indexes bone " +
                            indexes[slot] + " of " + names.Length + ".");
                }

                if (Math.Abs(total - 1f) > 0.001f)
                    throw new InvalidDataException(
                        "A membrane vertex weight sums to " + total + ".");
            }

            return names;
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
                { reason = "donor-bone-name-ambiguous:" + donorBones[slot].name;
                  return false; }
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
