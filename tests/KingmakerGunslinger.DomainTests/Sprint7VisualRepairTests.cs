using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    // Sprint 7 (S7 T1-T7 / REP-04, VIS-01 to VIS-06) regressions.
    internal static class Sprint7VisualRepairTests
    {
        // A decoded schema-2 creature export: positions, normals, UVs,
        // triangles and the four-slot weight rows, in the loader's byte order.
        internal sealed class DecodedMesh
        {
            internal JObject Payload;
            internal float[][] Positions;
            internal float[][] Normals;
            internal int[] Triangles;
            internal int[][] BoneIndices;
            internal float[][] BoneWeights;
            internal string[] Bones;
        }

        internal static DecodedMesh Decode(string relative)
        {
            JObject payload = JObject.Parse(File.ReadAllText(relative));
            byte[] blob = Convert.FromBase64String((string)payload["data"]);
            int n = (int)payload["vertexCount"], t = (int)payload["triangleCount"];
            Assertions.Equal(n * 32 + t * 12 + n * 32, blob.Length, relative + " blob length matches its counts.");
            var mesh = new DecodedMesh
            {
                Payload = payload,
                Positions = new float[n][],
                Normals = new float[n][],
                Triangles = new int[t * 3],
                BoneIndices = new int[n][],
                BoneWeights = new float[n][],
                Bones = payload["bones"].Values<string>().ToArray()
            };
            for (int i = 0; i < n; i++)
            {
                mesh.Positions[i] = Floats(blob, i * 12, 3);
                mesh.Normals[i] = Floats(blob, n * 12 + i * 12, 3);
            }
            Buffer.BlockCopy(blob, n * 32, mesh.Triangles, 0, t * 12);
            int weights = n * 32 + t * 12;
            for (int i = 0; i < n; i++)
            {
                mesh.BoneIndices[i] = new int[4];
                mesh.BoneWeights[i] = new float[4];
                for (int slot = 0; slot < 4; slot++)
                {
                    mesh.BoneIndices[i][slot] = BitConverter.ToInt32(blob, weights + i * 32 + slot * 8);
                    mesh.BoneWeights[i][slot] = BitConverter.ToSingle(blob, weights + i * 32 + slot * 8 + 4);
                }
            }
            return mesh;
        }

        private static float[] Floats(byte[] blob, int offset, int count)
        {
            var values = new float[count];
            for (int k = 0; k < count; k++) values[k] = BitConverter.ToSingle(blob, offset + k * 4);
            return values;
        }

        private static double[] Sub(float[] a, float[] b)
        {
            return new double[] { a[0] - b[0], a[1] - b[1], a[2] - b[2] };
        }

        private static double[] Cross(double[] a, double[] b)
        {
            return new[] { a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0] };
        }

        private static double Length(double[] a)
        {
            return Math.Sqrt(a[0] * a[0] + a[1] * a[1] + a[2] * a[2]);
        }

        // UV seams duplicate a position; topology is judged on welded points.
        private static int[] Weld(DecodedMesh mesh)
        {
            var ids = new Dictionary<Tuple<float, float, float>, int>();
            return mesh.Positions.Select(p =>
            {
                var key = Tuple.Create(p[0], p[1], p[2]);
                int id;
                if (!ids.TryGetValue(key, out id)) { id = ids.Count; ids[key] = id; }
                return id;
            }).ToArray();
        }

        // The decoded-export contract VIS-01 adds: no zero-area triangle, a
        // closed consistently wound surface on welded positions (each directed
        // edge once, always with its reverse), every closed part outward,
        // every vertex used, finite unit normals and valid weight rows.
        // Returns the welded components for creature-specific checks.
        internal static List<List<int>> AssertSoundClosedExport(DecodedMesh mesh, string name)
        {
            int n = mesh.Positions.Length;
            var used = new bool[n];
            foreach (int index in mesh.Triangles)
            {
                Assertions.True(index >= 0 && index < n, name + " triangle indices are in range.");
                used[index] = true;
            }
            Assertions.Equal(0, used.Count(value => !value), name + " has no unused vertex.");
            for (int i = 0; i < n; i++)
            {
                Assertions.True(mesh.Positions[i].Concat(mesh.Normals[i]).All(v => !float.IsNaN(v) && !float.IsInfinity(v)),
                    name + " positions and normals are finite.");
                double length = Math.Sqrt(mesh.Normals[i].Sum(v => (double)v * v));
                Assertions.True(Math.Abs(length - 1) < 1e-3, name + " normals are unit length.");
                double total = 0;
                for (int slot = 0; slot < 4; slot++)
                {
                    float weight = mesh.BoneWeights[i][slot];
                    Assertions.True(weight >= 0, name + " weights are nonnegative.");
                    if (weight > 0)
                        Assertions.True(mesh.BoneIndices[i][slot] >= 0 && mesh.BoneIndices[i][slot] < mesh.Bones.Length,
                            name + " weighted bone indices name an exported bone.");
                    total += weight;
                }
                Assertions.True(Math.Abs(total - 1) < 1e-3, name + " weight rows sum to one.");
            }

            int[] weld = Weld(mesh);
            int zeroArea = 0;
            var directed = new Dictionary<long, int>();
            int weldCount = weld.Max() + 1;
            Func<int, int, long> edge = (u, v) => (long)u * weldCount + v;
            var parent = Enumerable.Range(0, weldCount).ToArray();
            Func<int, int> find = null;
            find = x => parent[x] == x ? x : (parent[x] = find(parent[x]));
            for (int f = 0; f < mesh.Triangles.Length; f += 3)
            {
                int a = mesh.Triangles[f], b = mesh.Triangles[f + 1], c = mesh.Triangles[f + 2];
                double area = Length(Cross(Sub(mesh.Positions[b], mesh.Positions[a]), Sub(mesh.Positions[c], mesh.Positions[a]))) / 2;
                if (area < 1e-10) zeroArea++;
                int[] w = { weld[a], weld[b], weld[c] };
                for (int k = 0; k < 3; k++)
                {
                    long key = edge(w[k], w[(k + 1) % 3]);
                    int count;
                    directed.TryGetValue(key, out count);
                    directed[key] = count + 1;
                }
                parent[find(w[0])] = find(w[1]);
                parent[find(w[1])] = find(w[2]);
            }
            Assertions.Equal(0, zeroArea, name + " has no zero-area triangle.");
            Assertions.Equal(0, directed.Count(e => e.Value > 1), name + " uses no directed edge twice on welded positions.");
            Assertions.Equal(0, directed.Count(e => e.Key / weldCount == e.Key % weldCount),
                name + " has no triangle collapsed onto one welded point.");
            Assertions.Equal(0, directed.Keys.Count(key => !directed.ContainsKey(edge((int)(key % weldCount), (int)(key / weldCount)))),
                name + " is closed: every welded directed edge has its reverse.");

            var components = new Dictionary<int, List<int>>();
            for (int f = 0; f < mesh.Triangles.Length; f += 3)
            {
                int root = find(weld[mesh.Triangles[f]]);
                List<int> faces;
                if (!components.TryGetValue(root, out faces)) components[root] = faces = new List<int>();
                faces.Add(f);
            }
            foreach (List<int> faces in components.Values)
            {
                double volume = 0;
                foreach (int f in faces)
                {
                    float[] a = mesh.Positions[mesh.Triangles[f]], b = mesh.Positions[mesh.Triangles[f + 1]], c = mesh.Positions[mesh.Triangles[f + 2]];
                    var bc = Cross(new double[] { b[0], b[1], b[2] }, new double[] { c[0], c[1], c[2] });
                    volume += (a[0] * bc[0] + a[1] * bc[1] + a[2] * bc[2]) / 6;
                }
                Assertions.True(volume > 0, name + " every closed part faces outward (positive signed volume).");
            }
            return components.Values.ToList();
        }

        // VIS-01: every wing membrane strip ran from the wing wrist, so its
        // first grid row collapsed there: 48 zero-area triangles at the wing
        // roots, and the three strips of each wing welded into one inside-out
        // part. The repaired export is checked with its two R7 siblings,
        // which already met the same contract, as controls for the check.
        internal static void Vis01NalfeshneeWingRootsAreSound()
        {
            foreach (string path in new[] {
                "assets/sprint38-glabrezu/glabrezu-mesh.json",
                "assets/sprint40-ice-devil/ice-devil-mesh.json",
                "assets/sprint39-nalfeshnee/nalfeshnee-mesh.json" })
                AssertSoundClosedExport(Decode(path), path);

            DecodedMesh nalfeshnee = Decode("assets/sprint39-nalfeshnee/nalfeshnee-mesh.json");
            List<List<int>> parts = AssertSoundClosedExport(nalfeshnee, "nalfeshnee");
            int torso = Array.IndexOf(nalfeshnee.Bones, "Torso_Upper");
            Assertions.True(torso >= 0, "The Nalfeshnee exports Torso_Upper.");
            // Membrane parts: wholly chest-weighted parts reaching past the
            // shoulder to the wing tips behind the body.
            int membranes = 0;
            foreach (List<int> faces in parts)
            {
                var vertices = faces.SelectMany(f => new[] { nalfeshnee.Triangles[f], nalfeshnee.Triangles[f + 1], nalfeshnee.Triangles[f + 2] }).Distinct().ToList();
                bool chestOnly = vertices.All(v => nalfeshnee.BoneIndices[v].Where((bone, slot) => nalfeshnee.BoneWeights[v][slot] > 0).All(bone => bone == torso));
                float reach = vertices.Max(v => Math.Abs(nalfeshnee.Positions[v][0]));
                bool behind = vertices.All(v => nalfeshnee.Positions[v][1] < -.15f);
                if (chestOnly && behind && reach > .60f) membranes++;
            }
            Assertions.Equal(6, membranes, "Each wing keeps its three membrane panels as separate closed chest-weighted shells.");
            Assertions.Equal(false, (bool)nalfeshnee.Payload["articulatedWings"], "The wings still follow the chest.");
            Assertions.Equal(2, (int)nalfeshnee.Payload["visibleWings"], "Two wings are retained.");
        }
    }
}
