using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Ettin-only sculpt on the exact readable Hill mesh. No native geometry or
    /// pixels are distributed: the installed mesh supplies detailed face topology,
    /// UVs and weights. The original upper body is replaced by two branches of ONE
    /// continuous surface. Lower body, rig, equipment and colliders are untouched.
    /// </summary>
    internal static class EttinBodyMesh
    {
        internal const string MeshName = "KMG_Ettin_AuthoredBody";
        private const float Cut = 2.9f, MidX = .000255333f;
        internal sealed class Data
        {
            internal Vector3[] Vertices, Normals;
            internal Vector2[] UV;
            internal BoneWeight[] Weights;
            internal int[] Triangles;
        }
        private sealed class Row
        {
            internal Vector3 P;
            internal Vector2 UV;
            internal float SourceY;
            internal Dictionary<int, float> W;
        }

        internal static Vector3 BranchOffset(int side)
        { return new Vector3(side * .34f, -.05f, 0f); }

        internal static Mesh Build(Mesh source)
        {
            if (source == null || source.name != "hill_giant_1" || !source.isReadable ||
                source.vertexCount != 2886 || source.subMeshCount != 1 || source.triangles.Length != 12744)
                throw new NotSupportedException("Exact readable Hill body mesh contract changed.");
            var data = BuildData(source.vertices, source.uv, source.boneWeights,
                source.triangles, source.bindposes.Length);
            var mesh = new Mesh { name = MeshName };
            try
            {
                mesh.vertices = data.Vertices; mesh.uv = data.UV;
                mesh.boneWeights = data.Weights; mesh.triangles = data.Triangles;
                mesh.normals = data.Normals;
                Matrix4x4[] native = source.bindposes;
                var bind = native.ToList();
                foreach (int side in new[] { -1, 1 })
                    for (int bone = 3; bone <= 6; bone++)
                        bind.Add(native[bone] * Matrix4x4.Translate(-BranchOffset(side)));
                mesh.bindposes = bind.ToArray();
                mesh.RecalculateBounds();
                return mesh;
            }
            catch { UnityEngine.Object.DestroyImmediate(mesh); throw; }
        }

        // Pure array construction is also used for the private offline preview.
        // It does not read a game, create Unity objects, or change source arrays.
        internal static Data BuildData(Vector3[] positions, Vector2[] uv,
            BoneWeight[] skin, int[] indices, int boneCount)
        {
            if (positions.Length != uv.Length || positions.Length != skin.Length || boneCount < 7)
                throw new ArgumentException("Ettin source channels are incomplete.");
            var source = positions.Select((p, i) => new Row { P = p, UV = uv[i],
                SourceY = p.y, W = Unpack(skin[i]) }).ToArray();
            var lower = new List<Row>(); var upper = new List<Row>();
            var lowerTriangles = new List<int>(); var upperTriangles = new List<int>();
            var cutPoints = new List<Row>();
            for (int t = 0; t < indices.Length; t += 3)
            {
                Row a = source[indices[t]], b = source[indices[t + 1]], c = source[indices[t + 2]];
                Row ab = Mix(a, b, .5f), bc = Mix(b, c, .5f), ca = Mix(c, a, .5f);
                foreach (Row[] triangle in new[] { new[] { a, ab, ca }, new[] { ab, b, bc },
                    new[] { ca, bc, c }, new[] { ab, bc, ca } })
                {
                    Clip(triangle, false, lower, lowerTriangles, cutPoints);
                    Clip(triangle, true, upper, upperTriangles, null);
                }
            }
            if (cutPoints.Count == 0) throw new InvalidOperationException("Ettin shoulder section unavailable.");
            Row front = cutPoints.OrderBy(r => r.P.z).First();
            Row back = cutPoints.OrderByDescending(r => r.P.z).First();
            var rows = new List<Row>(lower); var triangles = new List<int>(lowerTriangles);
            foreach (int side in new[] { -1, 1 })
            {
                int start = rows.Count;
                rows.AddRange(upper.Select(row => Sculpt(row, side, boneCount, front, back)));
                triangles.AddRange(upperTriangles.Select(index => start + index));
            }
            var nondegenerate = new List<int>();
            for (int i = 0; i < triangles.Count; i += 3)
            {
                int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                if (Key(rows[a].P) == Key(rows[b].P) || Key(rows[b].P) == Key(rows[c].P) ||
                    Key(rows[c].P) == Key(rows[a].P)) continue;
                nondegenerate.Add(a); nondegenerate.Add(b); nondegenerate.Add(c);
            }
            triangles = nondegenerate;
            CloseBranchEndpoints(rows, triangles);
            RelaxJunction(rows, triangles);
            return Export(rows, triangles, boneCount + 8);
        }

        private static Row Sculpt(Row row, int side, int boneCount, Row front, Row back)
        {
            float x = row.P.x, y = row.P.y, z = row.P.z;
            float head = Smooth(row.W.Where(p => p.Key >= 4 && p.Key <= 6).Sum(p => p.Value));
            float width = .46f + ((side < 0 ? .94f : .90f) - .46f) * head;
            x = x * width + side * .34f;
            z = .17f + (z - .17f) * (.65f + .35f * head);
            float jaw = Smooth((y - 3.1f) / .13f) * (1f - Smooth((y - 3.38f) / .1f));
            float centre = side * .34f;
            x = centre + (x - centre) * (1f + (side < 0 ? .12f : -.065f) * jaw);
            float face = Smooth((.10f - z) / .2f) * Smooth((y - 3.28f) / .1f) *
                (1f - Smooth((y - 3.5f) / .1f));
            if (side > 0) x += .023f * face;
            else z -= .025f * jaw * Smooth((.10f - z) / .2f);
            y += .04f + (side > 0 ? .025f : -.012f) * head - .09f * head;
            float blend = Smooth((row.SourceY - Cut) / .27f);
            bool exterior = side * (row.P.x - MidX) >= -.00001f;
            Row endpoint = row.P.z < (front.P.z + back.P.z) * .5f ? front : back;
            Vector3 bottom = exterior ? row.P : new Vector3(endpoint.P.x, row.P.y, endpoint.P.z);
            var weights = row.W.ToDictionary(p => p.Key >= 3 && p.Key <= 6 ?
                boneCount + (side < 0 ? 0 : 4) + p.Key - 3 : p.Key, p => p.Value);
            if (blend < 1f) weights = BlendWeights(exterior ? row.W : endpoint.W, weights, blend);
            return new Row { P = Vector3.LerpUnclamped(bottom, new Vector3(x, y, z), blend),
                UV = row.UV, W = weights, SourceY = row.SourceY };
        }

        private static void Clip(Row[] triangle, bool above, List<Row> rows,
            List<int> triangles, List<Row> cuts)
        {
            var polygon = new List<Row>();
            for (int i = 0; i < 3; i++)
            {
                Row a = triangle[i], b = triangle[(i + 1) % 3];
                bool first = (a.P.y >= Cut) == above, second = (b.P.y >= Cut) == above;
                if (first) polygon.Add(a);
                if (first != second)
                {
                    Row point = Mix(a, b, (Cut - a.P.y) / (b.P.y - a.P.y));
                    point.P.y = Cut; polygon.Add(point); if (cuts != null) cuts.Add(point);
                }
            }
            int start = rows.Count; rows.AddRange(polygon);
            for (int i = 1; i < polygon.Count - 1; i++)
            { triangles.Add(start); triangles.Add(start + i); triangles.Add(start + i + 1); }
        }

        private static void CloseBranchEndpoints(List<Row> rows, List<int> triangles)
        {
            var edges = new Dictionary<string, List<int[]>>();
            for (int i = 0; i < triangles.Count; i += 3)
                for (int j = 0; j < 3; j++)
                {
                    int a = triangles[i + j], b = triangles[i + (j + 1) % 3];
                    string first = Key(rows[a].P), second = Key(rows[b].P);
                    string key = string.CompareOrdinal(first, second) < 0 ? first + "/" + second : second + "/" + first;
                    List<int[]> list; if (!edges.TryGetValue(key, out list)) edges[key] = list = new List<int[]>();
                    list.Add(new[] { a, b });
                }
            var open = edges.Values.Where(list => list.Count == 1).Select(list => list[0]).ToList();
            while (open.Count != 0)
            {
                int[] first = open[0];
                int[] second = open.SingleOrDefault(e => Key(rows[e[0]].P) == Key(rows[first[1]].P));
                int[] third = second == null ? null : open.SingleOrDefault(e =>
                    Key(rows[e[0]].P) == Key(rows[second[1]].P) && Key(rows[e[1]].P) == Key(rows[first[0]].P));
                if (second == null || third == null || new[] { first[0], first[1], second[1] }
                    .Any(i => Math.Abs(rows[i].P.y - Cut) >= .04f))
                    throw new InvalidOperationException("Unexpected open edge outside Ettin shoulder endpoints.");
                triangles.Add(second[1]); triangles.Add(first[1]); triangles.Add(first[0]);
                open.Remove(first); open.Remove(second); open.Remove(third);
            }
        }

        private static void RelaxJunction(List<Row> rows, List<int> triangles)
        {
            var groups = rows.Select((row, index) => new { row, index }).GroupBy(p => Key(p.row.P)).ToArray();
            int[] map = new int[rows.Count];
            for (int i = 0; i < groups.Length; i++) foreach (var item in groups[i]) map[item.index] = i;
            var adjacent = Enumerable.Range(0, groups.Length).Select(i => new HashSet<int>()).ToArray();
            for (int i = 0; i < triangles.Count; i += 3)
                for (int j = 0; j < 3; j++)
                {
                    int a = map[triangles[i + j]], b = map[triangles[i + (j + 1) % 3]];
                    if (a != b) { adjacent[a].Add(b); adjacent[b].Add(a); }
                }
            Vector3[] positions = groups.Select(g => g.First().row.P).ToArray();
            float[] y = groups.Select(g => g.First().row.SourceY).ToArray();
            for (int pass = 0; pass < 59; pass++)
            {
                var next = new Vector3[positions.Length];
                for (int i = 0; i < positions.Length; i++)
                {
                    float amount = pass < 52 ? (pass % 2 == 0 ? .48f : -.49f) *
                        Smooth((y[i] - 2.65f) / .22f) * (1f - Smooth((y[i] - 3.04f) / .17f)) :
                        .38f * Smooth((y[i] - 2.72f) / .18f) * (1f - Smooth((y[i] - 3f) / .12f));
                    Vector3 mean = Vector3.zero;
                    foreach (int neighbour in adjacent[i]) mean += positions[neighbour];
                    if (adjacent[i].Count == 0) next[i] = positions[i];
                    else next[i] = Vector3.LerpUnclamped(positions[i], mean / adjacent[i].Count, amount);
                }
                positions = next;
            }
            for (int i = 0; i < rows.Count; i++) rows[i].P = positions[map[i]];
        }

        private static Data Export(List<Row> rows, List<int> triangles, int boneCount)
        {
            var normals = new Dictionary<string, Vector3>();
            for (int i = 0; i < triangles.Count; i += 3)
            {
                Vector3 a = rows[triangles[i]].P, b = rows[triangles[i + 1]].P, c = rows[triangles[i + 2]].P;
                Vector3 n = Vector3.Cross(b - a, c - a);
                for (int j = 0; j < 3; j++)
                {
                    string key = Key(rows[triangles[i + j]].P); Vector3 value;
                    normals.TryGetValue(key, out value); normals[key] = value + n;
                }
            }
            // Preserve UV splits, but share the exact position, normal and skin
            // at the connected shoulder seam. Unreferenced clipped rows vanish.
            var unique = new Dictionary<string, int>(); var output = new List<Row>(); var outputSkin = new List<BoneWeight>();
            var outputIndices = new List<int>();
            foreach (int index in triangles)
            {
                Row row = rows[index]; BoneWeight weight = Pack(row.W);
                string key = Key(row.P) + "/" + Q(row.UV.x) + "," + Q(row.UV.y) + "/" +
                    weight.boneIndex0 + ":" + Q(weight.weight0) + "," + weight.boneIndex1 + ":" + Q(weight.weight1) + "," +
                    weight.boneIndex2 + ":" + Q(weight.weight2) + "," + weight.boneIndex3 + ":" + Q(weight.weight3);
                int mapped;
                if (!unique.TryGetValue(key, out mapped))
                { unique[key] = mapped = output.Count; output.Add(row); outputSkin.Add(weight); }
                outputIndices.Add(mapped);
            }
            if (output.Count >= 65535 || outputSkin.Any(w => Unpack(w).Any(p => p.Key < 0 || p.Key >= boneCount)))
                throw new InvalidOperationException("Ettin private mesh exceeds audited index/skin bounds.");
            return new Data { Vertices = output.Select(r => r.P).ToArray(), UV = output.Select(r => r.UV).ToArray(),
                Normals = output.Select(r => NormalizeSurfaceNormal(normals[Key(r.P)])).ToArray(),
                Weights = outputSkin.ToArray(), Triangles = outputIndices.ToArray() };
        }

        private static Vector3 NormalizeSurfaceNormal(Vector3 value)
        {
            // Unity's Vector3.normalized zeros vectors below 1e-5. Area-weighted
            // normals on the donor's small facial triangles can be smaller while
            // still having a well-defined direction. Normalize without that
            // positional epsilon; genuinely degenerate surfaces still refuse.
            double length = Math.Sqrt((double)value.x * value.x + (double)value.y * value.y + (double)value.z * value.z);
            if (double.IsNaN(length) || double.IsInfinity(length) || length <= 1e-20)
                throw new InvalidOperationException("Ettin surface has an undefined normal.");
            return new Vector3((float)(value.x / length), (float)(value.y / length), (float)(value.z / length));
        }

        private static Row Mix(Row a, Row b, float t)
        { return new Row { P = Vector3.LerpUnclamped(a.P, b.P, t), UV = Vector2.LerpUnclamped(a.UV, b.UV, t),
            SourceY = a.SourceY + (b.SourceY - a.SourceY) * t, W = Unpack(Pack(BlendWeights(a.W, b.W, t))) }; }
        private static Dictionary<int, float> BlendWeights(Dictionary<int, float> a, Dictionary<int, float> b, float t)
        { return a.Keys.Concat(b.Keys).Distinct().ToDictionary(i => i, i =>
            (a.ContainsKey(i) ? a[i] : 0f) * (1f - t) + (b.ContainsKey(i) ? b[i] : 0f) * t); }
        private static Dictionary<int, float> Unpack(BoneWeight w)
        {
            var result = new Dictionary<int, float>();
            int[] ids = { w.boneIndex0, w.boneIndex1, w.boneIndex2, w.boneIndex3 };
            float[] values = { w.weight0, w.weight1, w.weight2, w.weight3 };
            for (int i = 0; i < 4; i++) if (values[i] > 0f)
            { float value; result.TryGetValue(ids[i], out value); result[ids[i]] = value + values[i]; }
            return result;
        }
        private static BoneWeight Pack(Dictionary<int, float> input)
        {
            var sorted = input.Where(p => p.Value > 0f).OrderByDescending(p => p.Value).ThenBy(p => p.Key).Take(4).ToArray();
            float total = sorted.Sum(p => p.Value);
            if (!(total > 0f)) throw new InvalidOperationException("Ettin vertex has no valid skin.");
            int[] ids = new int[4]; float[] values = new float[4];
            for (int i = 0; i < sorted.Length; i++) { ids[i] = sorted[i].Key; values[i] = sorted[i].Value / total; }
            return new BoneWeight { boneIndex0 = ids[0], boneIndex1 = ids[1], boneIndex2 = ids[2], boneIndex3 = ids[3],
                weight0 = values[0], weight1 = values[1], weight2 = values[2], weight3 = values[3] };
        }
        private static float Smooth(float v) { v = Mathf.Clamp01(v); return v * v * (3f - 2f * v); }
        private static string Q(float v) { return Math.Round(v, 5).ToString("F5", CultureInfo.InvariantCulture); }
        private static string Key(Vector3 v) { return Q(v.x) + "," + Q(v.y) + "," + Q(v.z); }
    }
}
