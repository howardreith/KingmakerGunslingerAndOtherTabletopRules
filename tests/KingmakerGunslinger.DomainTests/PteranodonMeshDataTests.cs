using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Validates the shipped Pteranodon mesh data and albedo as data.
    ///
    /// The runtime validates them too, before it touches a donor renderer, but
    /// that check only runs when the game does. This one runs on every build, so
    /// a regenerated asset that is malformed, over-influenced, bound to a bone
    /// nobody reviewed, or paired with a texture other than the one it names is
    /// caught at the point it is committed rather than in a fallback status
    /// string hours later.
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
        private const string AssetDirectory = "assets/pteranodon";
        private const int AlbedoSize = 1024;

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

        /// <summary>
        /// The atlas the generator maps to and the painter paints: five regions
        /// that tile the unit square exactly. The wings take the top half; the
        /// body, crest, beak and limbs take the four quarters below it.
        /// </summary>
        private static readonly string[] AtlasRegions =
        { "membrane", "body", "crest", "beak", "limbs" };

        private static string Resolve(string relative)
        {
            return Path.Combine(Environment.CurrentDirectory,
                relative.Replace('/', Path.DirectorySeparatorChar));
        }

        private static JObject Document()
        {
            string path = Resolve(MeshDataPath);
            Assertions.True(File.Exists(path),
                "The shipped Pteranodon mesh data is missing: " + MeshDataPath);
            return JObject.Parse(File.ReadAllText(path));
        }

        internal static void ShippedMeshDataIsWellFormed()
        {
            JObject document = Document();
            Assertions.True((int?)document["schemaVersion"] == 2,
                "The mesh data schema version is not 2.");
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

            // The atlas: every named region inside the unit square, none
            // degenerate, and together covering it exactly once, so no texel
            // is unaccounted for and no two parts paint over each other.
            var atlas = document["uvAtlas"] as JObject;
            Assertions.True(atlas != null, "The mesh data records no uv atlas.");
            double covered = 0.0;
            var boxes = new List<double[]>();
            foreach (string name in AtlasRegions)
            {
                var region = atlas[name] as JArray;
                Assertions.True(region != null && region.Count == 4,
                    "The uv atlas has no four-value region '" + name + "'.");
                double[] box = region.Select(value => (double)value).ToArray();
                Assertions.True(box.All(value => value >= 0.0 && value <= 1.0) &&
                    box[0] < box[2] && box[1] < box[3],
                    "The uv atlas region '" + name + "' is not a box inside the " +
                    "unit square.");
                foreach (double[] other in boxes)
                {
                    bool overlaps = box[0] < other[2] && other[0] < box[2] &&
                        box[1] < other[3] && other[1] < box[3];
                    Assertions.False(overlaps,
                        "The uv atlas region '" + name + "' overlaps another.");
                }

                boxes.Add(box);
                covered += (box[2] - box[0]) * (box[3] - box[1]);
            }

            Assertions.True(Math.Abs(covered - 1.0) < 1e-9,
                "The uv atlas regions cover " + covered + " of the texture, not all of it.");
            Assertions.True(atlas.Properties().Count() == AtlasRegions.Length,
                "The uv atlas declares a region nothing here reviews.");
        }

        /// <summary>
        /// The payload is positions, then normals, then texture coordinates,
        /// then triangle indices, then four (bone index, weight) pairs per
        /// vertex. Every index has to be in range, every coordinate inside the
        /// atlas, and every vertex's weights have to sum to one, because a mesh
        /// that fails any of these renders as a spike to the origin or an edge
        /// texel rather than failing loudly.
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
            int expected = vertexCount * 12 + vertexCount * 12 + vertexCount * 8 +
                triangleCount * 3 * 4 + vertexCount * 4 * 8;
            Assertions.True(blob.Length == expected,
                "The payload is " + blob.Length + " bytes; " + expected +
                " were expected for " + vertexCount + " vertices and " +
                triangleCount + " triangles.");

            int offset = vertexCount * 24;
            for (int index = 0; index < vertexCount; index++)
            {
                float u = BitConverter.ToSingle(blob, offset);
                float v = BitConverter.ToSingle(blob, offset + 4);
                offset += 8;
                Assertions.True(!float.IsNaN(u) && !float.IsNaN(v) &&
                    u >= 0f && u <= 1f && v >= 0f && v <= 1f,
                    "Vertex " + index + " has texture coordinate (" + u + ", " +
                    v + ") outside the atlas.");
            }

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

        /// <summary>
        /// The mesh names its painting by bare file name, exact SHA-256 and
        /// header dimensions, and the runtime refuses the visual if the file
        /// beside it is anything else. So the file has to exist, hash to the
        /// recorded value, and be the 8-bit RGB or RGBA PNG of the recorded
        /// size - the same checks the loader makes, run here on every build.
        /// </summary>
        internal static void ShippedAlbedoMatchesItsManifest()
        {
            JObject document = Document();
            var albedo = document["albedo"] as JObject;
            Assertions.True(albedo != null, "The mesh data names no albedo.");

            string file = (string)albedo["file"] ?? string.Empty;
            Assertions.True(file == "pteranodon-albedo.png",
                "The albedo is not the reviewed file name: '" + file + "'.");
            string sha256 = (string)albedo["sha256"] ?? string.Empty;
            Assertions.True(sha256.Length == 64 && sha256.All(value =>
                    (value >= '0' && value <= '9') || (value >= 'a' && value <= 'f')),
                "The albedo hash is not a lowercase SHA-256.");
            Assertions.True((int?)albedo["width"] == AlbedoSize &&
                (int?)albedo["height"] == AlbedoSize,
                "The albedo is not declared as " + AlbedoSize + "x" + AlbedoSize + ".");
            Assertions.True((int?)albedo["bitDepth"] == 8,
                "The albedo is not declared as 8 bits per channel.");
            int colorType = (int?)albedo["colorType"] ?? -1;
            Assertions.True(colorType == 2 || colorType == 6,
                "The albedo is not declared as RGB or RGBA.");

            string path = Resolve(AssetDirectory + "/" + file);
            Assertions.True(File.Exists(path),
                "The shipped Pteranodon albedo is missing: " + path);
            byte[] bytes = File.ReadAllBytes(path);
            string actual;
            using (SHA256 sha = SHA256.Create())
                actual = string.Concat(sha.ComputeHash(bytes)
                    .Select(value => value.ToString("x2")));
            Assertions.True(actual == sha256,
                "The albedo on disk hashes to " + actual + ", not the " +
                sha256 + " the mesh data names; the runtime would refuse it.");

            Assertions.True(bytes.Length > 33 && bytes[0] == 0x89 &&
                bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                bytes[12] == (byte)'I' && bytes[13] == (byte)'H' &&
                bytes[14] == (byte)'D' && bytes[15] == (byte)'R',
                "The albedo is not a PNG.");
            int width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
            int height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
            Assertions.True(width == AlbedoSize && height == AlbedoSize,
                "The albedo header says " + width + "x" + height + ".");
            Assertions.True(bytes[24] == 8 && bytes[25] == colorType,
                "The albedo header's depth or colour type differs from the manifest.");
        }
    }
}
