using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonPlaytestR7Tests
    {
        private sealed class Body
        {
            internal JObject Payload;
            internal float[][] Positions;
            internal int[] Triangles;
        }

        private static Body Load(string relative)
        {
            JObject payload = JObject.Parse(File.ReadAllText(relative));
            byte[] blob = Convert.FromBase64String((string)payload["data"]);
            int vertices = (int)payload["vertexCount"], triangles = (int)payload["triangleCount"];
            var positions = new float[vertices][];
            for (int index = 0; index < vertices; index++)
                positions[index] = new[] { BitConverter.ToSingle(blob, index * 12), BitConverter.ToSingle(blob, index * 12 + 4), BitConverter.ToSingle(blob, index * 12 + 8) };
            var indices = new int[triangles * 3];
            Buffer.BlockCopy(blob, vertices * 32, indices, 0, indices.Length * 4);
            return new Body { Payload = payload, Positions = positions, Triangles = indices };
        }

        // UV seams duplicate a position, so connectivity is measured on welded points.
        private static int[] WeldedComponents(Body body)
        {
            var parent = new Dictionary<string, string>();
            Func<int, string> key = index => string.Join(",", body.Positions[index].Select(value => Math.Round(value, 4)));
            Func<string, string> find = null;
            find = value =>
            {
                string root = value;
                while (parent[root] != root) root = parent[root];
                parent[value] = root;
                return root;
            };
            for (int index = 0; index < body.Positions.Length; index++)
                if (!parent.ContainsKey(key(index))) parent[key(index)] = key(index);
            for (int index = 0; index < body.Triangles.Length; index += 3)
                for (int corner = 1; corner < 3; corner++)
                    parent[find(key(body.Triangles[index]))] = find(key(body.Triangles[index + corner]));
            var ids = new Dictionary<string, int>();
            return Enumerable.Range(0, body.Positions.Length).Select(index =>
            {
                string root = find(key(index));
                int id;
                if (!ids.TryGetValue(root, out id)) { id = ids.Count; ids[root] = id; }
                return id;
            }).ToArray();
        }

        private static int Nearest(Body body, float x, float y, float z)
        {
            return Enumerable.Range(0, body.Positions.Length).OrderBy(index =>
                Math.Pow(body.Positions[index][0] - x, 2) + Math.Pow(body.Positions[index][1] - y, 2) +
                Math.Pow(body.Positions[index][2] - z, 2)).First();
        }

        // ART-01: the defect was small-arm roots floating off the chest.
        internal static void GlabrezuSmallArmsAreOneSkinWithTheChest()
        {
            Body body = Load("assets/sprint38-glabrezu/glabrezu-mesh.json");
            int[] components = WeldedComponents(body);
            int chest = Nearest(body, 0f, .25f, 1.40f);
            foreach (float side in new[] { -1f, 1f })
            {
                int forearm = Nearest(body, side * .47f, .40f, 1.05f);
                Assertions.True(Math.Abs(body.Positions[forearm][0] - side * .47f) < .12f,
                    "A small forearm exists at the authored position.");
                Assertions.Equal(components[chest], components[forearm],
                    "The small arm is the same continuous skin as the chest.");
            }
            Assertions.Equal(4, (int)body.Payload["visibleArms"], "The four-arm identity is retained.");
            Assertions.Equal(2, (int)body.Payload["visiblePincers"], "The pincers are retained.");
            Assertions.Equal(false, (bool)body.Payload["articulatedSmallArms"], "The chest-driven limit stays recorded.");
        }

        internal static void RevisedExportsSatisfyTheirLoaderContracts()
        {
            foreach (var row in new[] {
                new { Path = "assets/sprint38-glabrezu/glabrezu-mesh.json", Creature = "glabrezu", Silhouette = "four-armed-pincered-demon" },
                new { Path = "assets/sprint39-nalfeshnee/nalfeshnee-mesh.json", Creature = "nalfeshnee", Silhouette = "corpulent-boar-headed-winged-demon" },
                new { Path = "assets/sprint40-ice-devil/ice-devil-mesh.json", Creature = "ice-devil", Silhouette = "insectile-compound-eyed-segmented-gelugon" } })
            {
                Body body = Load(row.Path);
                Assertions.Equal(row.Creature, (string)body.Payload["creature"], row.Creature + " names itself.");
                Assertions.Equal(row.Silhouette, (string)body.Payload["distinctSilhouette"], row.Creature + " keeps its identity.");
                Assertions.True(InfernalVisualPolicy36.IsReviewedRig((string)body.Payload["rigSha256"]), row.Creature + " names a reviewed rig.");
                Assertions.True(body.Payload["bones"].Values<string>().SequenceEqual(InfernalVisualPolicy36.Bones), row.Creature + " keeps the exact bone set.");
                Assertions.True((int)body.Payload["vertexCount"] < 20000 && (int)body.Payload["triangleCount"] < 20000, row.Creature + " stays a modest skinned mesh.");
                Assertions.True(File.Exists(Path.Combine(Path.GetDirectoryName(row.Path), (string)body.Payload["albedo"]["file"])), row.Creature + " ships its albedo.");
            }
            JObject nalfeshnee = Load("assets/sprint39-nalfeshnee/nalfeshnee-mesh.json").Payload;
            Assertions.Equal(4, (int)nalfeshnee["visibleTusks"], "Four tusks are retained.");
            Assertions.Equal(2, (int)nalfeshnee["visibleWings"], "Two wings are retained.");
            Assertions.Equal(false, (bool)nalfeshnee["articulatedWings"], "The wings' rig limit stays recorded.");
            JObject ice = Load("assets/sprint40-ice-devil/ice-devil-mesh.json").Payload;
            Assertions.Equal(7, (int)ice["tail"]["segments"], "The seven-segment tail is retained.");
            Assertions.Equal(4, (int)ice["mandibles"], "Four mandibles are retained.");
            Assertions.Equal(false, (bool)ice["spearBakedIntoBody"], "The spear remains the equipped native weapon.");
        }
    }
}
