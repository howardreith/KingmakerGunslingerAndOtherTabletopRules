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

        // VIS-02: the human/tail Salamander shipped 16 triangle corners whose
        // own vertex normal faced against the triangle: a fold where the body
        // section turned all at once at the tail root (the waist pleat) and a
        // quarter-turn ring twist in the left middle finger. Normals are the
        // shipped Blender vertex normals, so agreement is judged per corner.
        internal static void Vis02SalamanderWaistAndFingersAreSound()
        {
            const string path = "assets/sprint17-serpents/salamander-human-mesh.json";
            DecodedMesh mesh = Decode(path);
            AssertSoundClosedExport(mesh, path);
            int opposing = 0;
            for (int f = 0; f < mesh.Triangles.Length; f += 3)
            {
                float[] a = mesh.Positions[mesh.Triangles[f]], b = mesh.Positions[mesh.Triangles[f + 1]], c = mesh.Positions[mesh.Triangles[f + 2]];
                double[] face = Cross(Sub(b, a), Sub(c, a));
                for (int corner = 0; corner < 3; corner++)
                {
                    float[] n = mesh.Normals[mesh.Triangles[f + corner]];
                    if (face[0] * n[0] + face[1] * n[1] + face[2] * n[2] < 0) opposing++;
                }
            }
            Assertions.Equal(0, opposing, "No triangle corner faces against its own shipped vertex normal.");

            // Every finger keeps a distal segment with real girth.
            foreach (string side in new[] { "L", "R" })
                for (int digit = 1; digit <= 3; digit++)
                {
                    string bone = side + "_Toe_" + digit + "_02";
                    int index = Array.IndexOf(mesh.Bones, bone);
                    Assertions.True(index >= 0, bone + " is exported.");
                    var ring = Enumerable.Range(0, mesh.Positions.Length)
                        .Where(v => mesh.BoneIndices[v][0] == index && mesh.BoneWeights[v][0] > .99f).ToList();
                    Assertions.True(ring.Count >= 8, bone + " keeps its distal finger rings.");
                    var centre = new float[3];
                    for (int k = 0; k < 3; k++) centre[k] = ring.Average(v => mesh.Positions[v][k]);
                    double spread = ring.Max(v => Length(Sub(mesh.Positions[v], centre)));
                    Assertions.True(spread > .01, bone + " is not collapsed.");
                }
            Assertions.Equal(36, mesh.Bones.Length, "The 26 human and 10 tail drivers are retained.");
            Assertions.Equal("eb0d5eaaf4bec8bf68506d42f299ded5d3d86cb676ff72e53f15d0cfbce9bfd2", (string)mesh.Payload["rigSha256"],
                "The export names the exact private human bind capture.");
            Assertions.True(mesh.Payload["originalTailSlap"] != null && mesh.Payload["originalTailRest"] != null,
                "The owned tail rest and slap are retained.");
        }

        // VIS-03: exactly the owned point-targeted DoubleMove movement actions
        // in the ledger, and nothing else, get the movement target row.
        internal static void Vis03MovementTargetIdentitiesAreExact()
        {
            JObject ledger = JObject.Parse(File.ReadAllText("blueprints/blueprints.json"));
            var owned = ledger["entries"].Where(entry =>
                    (string)entry["plannedType"] == "BlueprintAbility" &&
                    (((string)entry["symbol"]).StartsWith("KMG.Summoning.Special.", StringComparison.Ordinal) &&
                     (((string)entry["symbol"]).EndsWith(".Trample", StringComparison.Ordinal) ||
                      (string)entry["symbol"] == "KMG.Summoning.Special.Brachiosaurus.Overrun")))
                .Select(entry => (string)entry["guid"]).OrderBy(guid => guid, StringComparer.Ordinal).ToList();
            Assertions.Equal(10, owned.Count, "The ledger has nine owned Tramples and one owned Overrun.");
            Assertions.True(owned.SequenceEqual(Summoning.SummonActionTooltipPolicy.MovementTargetAbilityGuids
                    .OrderBy(guid => guid, StringComparer.Ordinal)),
                "The presented set is exactly the ledger's owned movement actions.");
            foreach (string guid in owned)
            {
                Assertions.True(Summoning.SummonActionTooltipPolicy.PresentsMovementTarget(guid, true, true),
                    guid + " presents its movement target.");
                Assertions.False(Summoning.SummonActionTooltipPolicy.PresentsMovementTarget(guid, false, true),
                    guid + " keeps native text if it stops using DoubleMove range.");
                Assertions.False(Summoning.SummonActionTooltipPolicy.PresentsMovementTarget(guid, true, false),
                    guid + " keeps native text if it stops targeting a point.");
            }
            // The native overrun the owned actions are cloned from and the two
            // owned overrun features keep their native presentation.
            foreach (string foreign in new[] {
                "1a3b471ecea51f7439a946b23577fd70",
                "c9f41b8d20e7456eae3d6c1f5a8b7204",
                "b45179f0d157465f899c0b9d096b67a6" })
                Assertions.False(Summoning.SummonActionTooltipPolicy.PresentsMovementTarget(foreign, true, true),
                    foreign + " keeps its native presentation.");
            Assertions.False(Summoning.SummonActionTooltipPolicy.PresentsMovementTarget(null, true, true),
                "A blueprint without an identity keeps its native presentation.");
        }

        // VIS-03: the target row keeps the native localized prefix and line
        // ending and states the native DoubleMove legality distance, with no
        // fictional number, range or school.
        internal static void Vis03MovementTargetTextIsTruthful()
        {
            string row = Summoning.SummonActionTooltipPolicy.MovementTarget("Target point",
                Summoning.SummonActionTooltipPolicy.MovementTargetRange);
            Assertions.Equal("Target point within twice this creature's speed.\n", row,
                "The row names the movement distance the native range check uses.");
            Assertions.False(row.Contains("ft") || row.Contains("feet") || row.Contains("None"),
                "No placeholder distance or school remains.");
            try
            {
                Summoning.SummonActionTooltipPolicy.MovementTarget("", "x");
                Assertions.True(false, "An empty native prefix is refused.");
            }
            catch (ArgumentException) { }
        }

        // VIS-04: the owned impact cue fires for exactly one resolved tail hit
        // and for nothing else; the later save plays no part in the decision.
        internal static void Vis04ImpactCueQualification()
        {
            int qualifying = 0;
            for (int mask = 0; mask < 32; mask++)
            {
                bool exactOwner = (mask & 1) != 0, exactTail = (mask & 2) != 0, hit = (mask & 4) != 0,
                    first = (mask & 8) != 0, victim = (mask & 16) != 0;
                bool cue = Summoning.AnkylosaurusTailImpactPolicy.QualifiesForCue(exactOwner, exactTail, hit, first, victim);
                if (cue) qualifying++;
                Assertions.Equal(exactOwner && exactTail && hit && first && victim, cue,
                    "Only the exact owner's resolved first tail hit on a creature plays the cue (mask " + mask + ").");
            }
            Assertions.Equal(1, qualifying, "A miss, a foreign owner or weapon, a repeated observation or no target never plays it.");
            Assertions.Equal("Sweetener_Low_06_Quake", Summoning.AnkylosaurusTailImpactPolicy.ImpactCue,
                "The provisional cue is the installed VFX-bank quake sweetener.");
            Assertions.Equal("VFX", Summoning.SummonPlaytestR6Policy.CueBank, "It plays from the bank the cue helper already loads.");
        }

        // VIS-05: only the exact owned Dust Mephit's launch of the shared cold
        // cone is presented as dust; the shared mapping is unchanged.
        internal static void Vis05DustBreathIdentityIsExact()
        {
            string cold = Summoning.MephitBreathPolicy.ColdConeProjectileGuid;
            string dust = Summoning.MephitBreathPolicy.DustMephitUnitGuid;
            Assertions.Equal(cold, Summoning.MephitBreathPolicy.ProjectileFor(Summoning.MephitBreathDamageKind.Slashing),
                "The Dust breath still delivers through the shared cold cone.");
            Assertions.Equal(cold, Summoning.MephitBreathPolicy.ProjectileFor(Summoning.MephitBreathDamageKind.Cold),
                "The shared cold-cone mapping for other mephits is unchanged.");
            Assertions.True(Summoning.MephitBreathPolicy.PresentsDustBreath(dust, cold), "The Dust Mephit's cone is presented as dust.");
            JObject ledger = JObject.Parse(File.ReadAllText("blueprints/blueprints.json"));
            Func<string, string> guid = symbol => (string)ledger["entries"].Single(entry => (string)entry["symbol"] == symbol)["guid"];
            Assertions.Equal(dust, guid("KMG.Summoning.Unit.DustMephit"), "The identity is the ledger's owned Dust Mephit unit.");
            foreach (string other in new[] { "KMG.Summoning.Unit.AirMephit", "KMG.Summoning.Unit.IceMephit", "KMG.Summoning.Unit.SaltMephit", "KMG.Summoning.Unit.EarthMephit" })
                Assertions.False(Summoning.MephitBreathPolicy.PresentsDustBreath(guid(other), cold), other + " keeps the native cold cone.");
            Assertions.False(Summoning.MephitBreathPolicy.PresentsDustBreath(dust, Summoning.MephitBreathPolicy.AcidConeProjectileGuid),
                "Another projectile from the Dust Mephit is not recoloured.");
            Assertions.False(Summoning.MephitBreathPolicy.PresentsDustBreath(null, cold), "A launcher without identity is not recoloured.");
        }

        // VIS-05: the recolour keeps each colour's alpha and brightness and
        // turns its hue from ice to warm dust.
        internal static void Vis05DustTintKeepsOpacityAndBrightness()
        {
            foreach (float[] ice in new[] { new[] { .55f, .78f, 1f, .5f }, new[] { .9f, .95f, 1f, .2f }, new[] { .1f, .2f, .35f, 1f } })
            {
                float[] dust = Summoning.MephitBreathPolicy.DustTint(ice[0], ice[1], ice[2], ice[3]);
                Func<float[], double> luminance = c => .2126 * c[0] + .7152 * c[1] + .0722 * c[2];
                Assertions.Equal(ice[3], dust[3], "Alpha is unchanged.");
                Assertions.True(Math.Abs(luminance(ice) - luminance(dust)) < 1e-4, "Brightness is unchanged.");
                Assertions.True(dust[0] > dust[1] && dust[1] > dust[2], "The hue is warm dust, red over green over blue.");
            }
            // A subtractive layer gets the complementary (cool) tone, so what
            // it removes from the scene leaves warm dust behind.
            float[] removed = Summoning.MephitBreathPolicy.DustTintSubtractive(.6f, .5f, .4f, .7f);
            Assertions.Equal(.7f, removed[3], "Subtractive alpha is unchanged.");
            Assertions.True(removed[2] > removed[1] && removed[1] > removed[0], "A subtractive layer removes cool light: blue over green over red.");
            Assertions.True(Summoning.MephitBreathPolicy.IsSubtractiveMaterial("Horizontal_Smoke00_128_Transparent_Hdr_Detail_Substract_Queue_02") &&
                !Summoning.MephitBreathPolicy.IsSubtractiveMaterial("Glow00_32_Add_Hdr_Offset_00"), "Subtractive layers are recognised by their native names.");
            float[] invalid = Summoning.MephitBreathPolicy.DustTint(float.NaN, 0, 0, .3f);
            Assertions.True(invalid.Take(3).All(value => !float.IsNaN(value)) && invalid[3] == .3f, "Non-finite input yields a finite dust tone.");
        }
    }
}
