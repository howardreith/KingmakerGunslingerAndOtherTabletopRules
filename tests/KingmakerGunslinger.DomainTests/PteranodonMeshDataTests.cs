using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Validates the shipped Pteranodon mesh data as data.
    ///
    /// The runtime validates it too, before it touches a donor renderer, but
    /// that check only runs when the game does. This one runs on every build, so
    /// a regenerated asset that is malformed, over-influenced, or bound to a
    /// bone nobody reviewed is caught at the point it is committed rather than
    /// in a fallback status string hours later.
    ///
    /// The runtime's own <c>BuildMesh</c> cannot be exercised here because it
    /// constructs a <c>UnityEngine.Mesh</c>, which needs the engine and not just
    /// the assembly. What is shared is the contract, not the code: these
    /// assertions and the runtime's are written from the same rules and the
    /// payload arithmetic is spelled out in both.
    /// </summary>
    internal static class PteranodonMeshDataTests
    {
        private const string MeshDataPath = "assets/pteranodon/pteranodon-mesh.json";

        /// <summary>
        /// Six shared bones and twenty per side. Tail_end, Tail_L, Tail_R and
        /// every *_end leaf are excluded on purpose: the donor's eagle tail fan
        /// carries no geometry, so a weight landing there would mean a feather
        /// fan had crept back into a pterosaur.
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

        private static JObject Document()
        {
            string path = Path.Combine(Environment.CurrentDirectory,
                MeshDataPath.Replace('/', Path.DirectorySeparatorChar));
            Assertions.True(File.Exists(path),
                "The shipped Pteranodon mesh data is missing: " + MeshDataPath);
            return JObject.Parse(File.ReadAllText(path));
        }

        internal static void ShippedMeshDataIsWellFormed()
        {
            JObject document = Document();
            Assertions.True((int?)document["schemaVersion"] == 1,
                "The mesh data schema version is not 1.");
            Assertions.True(((string)document["space"] ?? string.Empty)
                    .IndexOf("donor renderer local", StringComparison.Ordinal) >= 0,
                "The mesh data does not record the space it is authored in, " +
                "which is the whole reason the bind-pose work exists.");
            Assertions.True(((string)document["rigSha256"] ?? string.Empty).Length == 64,
                "The mesh data does not record the rig it was generated from.");

            string[] bones = ((JArray)document["bones"] ?? new JArray())
                .Select(value => (string)value).ToArray();
            Assertions.True(bones.Length == 46,
                "Expected 46 bones - six shared and twenty per side - got " +
                bones.Length + ".");
            Assertions.True(
                bones.Distinct(StringComparer.Ordinal).Count() == bones.Length,
                "The bone list repeats a name.");
            string[] unexpected = bones.Where(value =>
                !AllowedBones.Contains(value, StringComparer.Ordinal)).ToArray();
            Assertions.True(unexpected.Length == 0,
                "The mesh binds to bones outside the declared set: " +
                string.Join(", ", unexpected));
            foreach (string absent in new[] { "Tail_end", "Tail_L", "Tail_R" })
            {
                Assertions.False(bones.Contains(absent, StringComparer.Ordinal),
                    "The mesh weights geometry to " + absent + ", which is part " +
                    "of the donor's eagle tail fan and must carry none.");
            }
        }

        /// <summary>
        /// The payload is positions, then normals, then triangle indices, then
        /// four (bone index, weight) pairs per vertex. Every index has to be in
        /// range and every vertex's weights have to sum to one, because a mesh
        /// that fails either renders as a spike to the origin rather than
        /// failing loudly.
        /// </summary>
        internal static void ShippedMeshDataPayloadIsConsistent()
        {
            JObject document = Document();
            int vertexCount = (int?)document["vertexCount"] ?? 0;
            int triangleCount = (int?)document["triangleCount"] ?? 0;
            int boneCount = ((JArray)document["bones"] ?? new JArray()).Count;
            Assertions.True(vertexCount > 0 && triangleCount > 0,
                "The mesh declares " + vertexCount + " vertices and " +
                triangleCount + " triangles.");

            byte[] blob = Convert.FromBase64String(
                (string)document["data"] ?? string.Empty);
            int expected = vertexCount * 12 + vertexCount * 12 +
                triangleCount * 3 * 4 + vertexCount * 4 * 8;
            Assertions.True(blob.Length == expected,
                "The payload is " + blob.Length + " bytes; " + expected +
                " were expected for " + vertexCount + " vertices and " +
                triangleCount + " triangles.");

            int offset = vertexCount * 24;
            for (int index = 0; index < triangleCount * 3; index++)
            {
                int vertex = BitConverter.ToInt32(blob, offset);
                offset += 4;
                Assertions.True(vertex >= 0 && vertex < vertexCount,
                    "Triangle corner " + index + " indexes vertex " + vertex +
                    " of " + vertexCount + ".");
            }

            var used = new HashSet<int>();
            int overInfluenced = 0;
            for (int index = 0; index < vertexCount; index++)
            {
                float total = 0f;
                int influences = 0;
                for (int slot = 0; slot < 4; slot++)
                {
                    int bone = BitConverter.ToInt32(blob, offset);
                    float weight = BitConverter.ToSingle(blob, offset + 4);
                    offset += 8;
                    total += weight;
                    if (weight <= 0f) continue;
                    influences++;
                    used.Add(bone);
                    Assertions.True(bone >= 0 && bone < boneCount,
                        "Vertex " + index + " is weighted to bone " + bone +
                        " of " + boneCount + ".");
                }

                if (influences > 4) overInfluenced++;
                Assertions.True(Math.Abs(total - 1f) <= 0.001f,
                    "Vertex " + index + " weights sum to " + total + ".");
            }

            Assertions.True(overInfluenced == 0,
                overInfluenced + " vertices carry more than four influences, " +
                "which Unity cannot represent.");
            // Every declared bone should actually carry geometry; a bone in the
            // list that nothing is weighted to means the generator declared a
            // binding it did not use, and the loader would resolve a transform
            // for no reason.
            Assertions.True(used.Count == boneCount,
                "The mesh declares " + boneCount + " bones but only weights " +
                "geometry to " + used.Count + " of them.");
        }
    }
}
