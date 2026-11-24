using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
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

        // VIS-04: the owned impact cue fires once per resolved tail roll on the
        // roll's own in-game target; the cue's claim is taken only when every
        // other condition holds, so a stale, removed or mismatched wrapper never
        // consumes it. The later save plays no part in the decision.
        internal static void Vis04ImpactCueQualification()
        {
            int qualifying = 0;
            for (int mask = 0; mask < 32; mask++)
            {
                bool exactOwner = (mask & 1) != 0, exactTail = (mask & 2) != 0, hit = (mask & 4) != 0,
                    inGame = (mask & 8) != 0, rollTarget = (mask & 16) != 0;
                int claims = 0;
                bool cue = Summoning.AnkylosaurusTailImpactPolicy.QualifiesForCue(exactOwner, exactTail, hit, inGame, rollTarget, () => { claims++; return true; });
                bool valid = exactOwner && exactTail && hit && inGame && rollTarget;
                if (cue) qualifying++;
                Assertions.Equal(valid, cue, "Only the exact owner's resolved tail hit on the roll's own in-game target plays the cue (mask " + mask + ").");
                Assertions.Equal(valid ? 1 : 0, claims, "The cue claim is taken only for a valid strike (mask " + mask + ").");
            }
            Assertions.Equal(1, qualifying, "A miss, a foreign owner or weapon, a removed creature or another creature than the roll's target never plays it.");
            Assertions.False(Summoning.AnkylosaurusTailImpactPolicy.QualifiesForCue(true, true, true, true, true, () => false),
                "A roll whose cue was already claimed does not play again.");

            // The production gate: a mismatched wrapper first, then the valid
            // wrapper of the same roll, then a duplicate of it, then a later roll.
            var gate = new Firing.ReferenceEventGate();
            object roll = new object(), later = new object();
            Func<bool, bool, object, bool> observe = (inGame, rollTarget, r) =>
                Summoning.AnkylosaurusTailImpactPolicy.QualifiesForCue(true, true, true, inGame, rollTarget, () => gate.TryMark(r));
            Assertions.False(observe(true, false, roll), "A wrapper naming another creature than the roll's target posts nothing.");
            Assertions.False(observe(false, true, roll), "A removed target posts nothing.");
            Assertions.True(observe(true, true, roll), "The valid wrapper of the same roll still posts its one cue.");
            Assertions.False(observe(true, true, roll), "A duplicate observation of that roll posts nothing.");
            Assertions.True(observe(true, true, later), "A later roll posts its own cue.");
            string control = File.ReadAllText("src/KingmakerGunslinger/Summoning/AnkylosaurusTailControl.cs");
            Assertions.True(control.Contains("victim != null && !victim.Destroyed && victim.IsInGame") && control.Contains("() => CueEvents.TryMark(attack)"),
                "The production handler maps the event to the in-game target, the roll target and the cue's own gate.");
            Assertions.Equal("Sweetener_Low_06_Quake", Summoning.AnkylosaurusTailImpactPolicy.ImpactCue,
                "The provisional cue is the installed VFX-bank quake sweetener.");
            Assertions.Equal("VFX", Summoning.SummonPlaytestR6Policy.CueBank, "It plays from the bank the cue helper already loads.");
        }

        // VIS-05: only the owned Dust breath executed by the exact owned Dust
        // Mephit presents the shared cold cone as dust; the mapping is unchanged.
        internal static void Vis05DustBreathIdentityIsExact()
        {
            string cold = Summoning.MephitBreathPolicy.ColdConeProjectileGuid;
            string dust = Summoning.MephitBreathPolicy.DustMephitUnitGuid;
            string breath = Summoning.MephitBreathPolicy.DustMephitBreathGuid;
            Assertions.Equal(cold, Summoning.MephitBreathPolicy.ProjectileFor(Summoning.MephitBreathDamageKind.Slashing),
                "The Dust breath still delivers through the shared cold cone.");
            Assertions.Equal(cold, Summoning.MephitBreathPolicy.ProjectileFor(Summoning.MephitBreathDamageKind.Cold),
                "The shared cold-cone mapping for other mephits is unchanged.");
            Assertions.True(Summoning.MephitBreathPolicy.PresentsDustBreath(dust, breath, cold), "The Dust Mephit's own breath is presented as dust.");
            JObject ledger = JObject.Parse(File.ReadAllText("blueprints/blueprints.json"));
            Func<string, string> guid = symbol => (string)ledger["entries"].Single(entry => (string)entry["symbol"] == symbol)["guid"];
            Assertions.Equal(dust, guid("KMG.Summoning.Unit.DustMephit"), "The unit is the ledger's owned Dust Mephit.");
            Assertions.Equal(breath, guid("KMG.Summoning.Special.DustMephit.Breath"), "The ability is the ledger's owned Dust breath.");
            foreach (string other in new[] { "KMG.Summoning.Unit.AirMephit", "KMG.Summoning.Unit.IceMephit", "KMG.Summoning.Unit.SaltMephit", "KMG.Summoning.Unit.EarthMephit" })
                Assertions.False(Summoning.MephitBreathPolicy.PresentsDustBreath(guid(other), breath, cold), other + " keeps the native cold cone.");
            Assertions.False(Summoning.MephitBreathPolicy.PresentsDustBreath(dust, guid("KMG.Summoning.Special.IceMephit.Breath"), cold),
                "Another ability executed by the Dust Mephit keeps the native cold cone.");
            Assertions.False(Summoning.MephitBreathPolicy.PresentsDustBreath(dust, null, cold), "A launch outside an ability execution keeps the native cone.");
            Assertions.False(Summoning.MephitBreathPolicy.PresentsDustBreath(dust, breath, Summoning.MephitBreathPolicy.AcidConeProjectileGuid),
                "Another projectile is not recoloured.");
            Assertions.False(Summoning.MephitBreathPolicy.PresentsDustBreath(null, breath, cold), "A launcher without identity is not recoloured.");
        }

        // R1-F9: a stand-in native creation whose one cast-effect spawn is made
        // through the production call-site adoption (CastEffectAdoption.Spawn),
        // as the transpiled CreateView makes it; it can be interrupted before or
        // after that spawn and can run nested work first.
        private sealed class StandInFx
        {
            internal readonly string Prefab; internal readonly float Point; internal int Tints;
            internal StandInFx(string prefab, float point) { Prefab = prefab; Point = point; }
        }

        private sealed class StandInWorld
        {
            internal readonly List<StandInFx> Spawned = new List<StandInFx>();
            internal int Adoptions;

            // Any caller's native spawn (no product code).
            internal StandInFx NativeSpawn(string prefab, float point)
            {
                var fx = new StandInFx(prefab, point); Spawned.Add(fx); return fx;
            }

            internal void CreateView(bool owned, string interrupt, Action nested)
            {
                if (nested != null) nested();
                if (interrupt == "before-spawn") throw new NullReferenceException("no target");
                Summoning.CastEffectAdoption.Spawn(() => NativeSpawn("cold-cone-cast", 1f), () => owned,
                    fx => { fx.Tints++; Adoptions++; }, error => { throw new InvalidOperationException("unexpected adoption failure", error); });
                if (interrupt == "after-spawn") throw new InvalidOperationException("after spawn");
            }
        }

        // R1-F9: ownership is bound to the one spawn invocation of the creation
        // itself: an interrupted creation lends nothing, a foreign spawn of the
        // same prefab at the same point inside the enclosing creation stays
        // native, and the subsequent valid owned spawn is tinted exactly once.
        internal static void Vis05CallSiteAdoptionOwnership()
        {
            // The reviewer's interval: an enclosing owned creation is active; an
            // inner owned creation is interrupted; before the enclosing call
            // returns, another caller spawns the same prefab at the same point.
            var world = new StandInWorld();
            StandInFx foreign = null;
            world.CreateView(true, null, () =>
            {
                try { world.CreateView(true, "before-spawn", null); Assertions.True(false, "The inner creation is interrupted."); }
                catch (NullReferenceException) { }
                foreign = world.NativeSpawn("cold-cone-cast", 1f);
                Assertions.Equal(0, foreign.Tints, "The foreign same-prefab, same-point spawn inside the enclosing creation stays native.");
                Assertions.Equal(0, world.Adoptions, "The interrupted creation tinted nothing and lent nothing.");
            });
            Assertions.Equal(2, world.Spawned.Count, "Two instances exist: the foreign one and the enclosing creation's own.");
            Assertions.Equal(1, world.Adoptions, "The subsequent valid owned spawn (the enclosing creation's) is tinted once.");
            Assertions.True(world.Spawned[1].Tints == 1 && foreign.Tints == 0, "Exactly the owned creation's own instance carries the tint.");

            // The same interval inside a foreign enclosing creation, then a later owned creation.
            var second = new StandInWorld();
            StandInFx stray = null;
            second.CreateView(false, null, () =>
            {
                try { second.CreateView(true, "before-spawn", null); } catch (NullReferenceException) { }
                stray = second.NativeSpawn("cold-cone-cast", 1f);
            });
            Assertions.True(stray.Tints == 0 && second.Spawned.All(fx => fx.Tints == 0), "Neither the foreign spawn nor the foreign creation's own spawn is tinted.");
            second.CreateView(true, null, null);
            Assertions.True(second.Adoptions == 1 && second.Spawned.Last().Tints == 1, "A later valid owned creation still receives its own tint.");

            // Interrupted after its own spawn: that instance is the owned cast's
            // (tinted once); a later foreign spawn is not.
            var third = new StandInWorld();
            try { third.CreateView(true, "after-spawn", null); } catch (InvalidOperationException) { }
            StandInFx after = third.NativeSpawn("cold-cone-cast", 1f);
            Assertions.True(third.Adoptions == 1 && third.Spawned[0].Tints == 1 && after.Tints == 0, "A creation interrupted after its spawn keeps only its own tint.");

            // A foreign creation nested in an owned one is not tinted; the owned one is.
            var fourth = new StandInWorld();
            fourth.CreateView(true, null, () => fourth.CreateView(false, null, null));
            Assertions.True(fourth.Spawned[0].Tints == 0 && fourth.Spawned[1].Tints == 1, "Nested foreign creation native, enclosing owned creation tinted.");

            // Native failures propagate unchanged; no native instance, nothing adopted;
            // an adoption failure is reported and the native instance returned.
            var boom = new InvalidOperationException("native spawn failed");
            try { Summoning.CastEffectAdoption.Spawn<StandInFx>(() => { throw boom; }, () => true, fx => fx.Tints++, null); Assertions.True(false, "Native failure propagates."); }
            catch (InvalidOperationException error) { Assertions.True(ReferenceEquals(error, boom), "The native exception is the one propagated."); }
            bool asked = false;
            Assertions.True(Summoning.CastEffectAdoption.Spawn<StandInFx>(() => null, () => { asked = true; return true; }, fx => fx.Tints++, null) == null && !asked,
                "A spawn that produced nothing adopts nothing.");
            Exception reported = null;
            var kept = new StandInFx("cold-cone-cast", 1f);
            Assertions.True(ReferenceEquals(kept, Summoning.CastEffectAdoption.Spawn(() => kept, () => true, fx => { throw new InvalidOperationException("tint failed"); }, error => reported = error)) &&
                reported != null && kept.Tints == 0, "An adoption failure is reported and the native instance is returned untouched.");
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

        // F13: the six Sprint 7 review phases are exact fresh, save-free phases
        // of the Sprint 32 review scenario only; outdoor ones use the native
        // Default fixture, interior ones the static interior; none grants
        // player input, Wall preparation or host restriction; only the UI phase
        // may capture screens (enforced in NativeIconScreenEvidence.Supports).
        internal static void Sprint7AdmissionBoundaries()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint32NativeReview;
            string[] outdoor = { ExpandedSummoningNativeProfilePolicy.Sprint39S7GhaelePhase, ExpandedSummoningNativeProfilePolicy.Sprint39S7ModelsPhase,
                ExpandedSummoningNativeProfilePolicy.Sprint39S7UiPhase, ExpandedSummoningNativeProfilePolicy.Sprint39S7EffectsPhase };
            string[] interior = { ExpandedSummoningNativeProfilePolicy.Sprint39S7GhaeleInteriorPhase, ExpandedSummoningNativeProfilePolicy.Sprint39S7ModelsInteriorPhase };
            foreach (string phase in outdoor.Concat(interior))
            {
                Assertions.True(ExpandedSummoningNativeProfilePolicy.Sprint7Phase(phase), phase + " is a Sprint 7 phase.");
                Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 0, 0, false, false, false, false, true), phase + " admitted fresh and save-free.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false, scenario, 1, phase, 0, 0, 0, false, false, false, false, true), phase + " requires automatic exit.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 1, 0, 0, false, false, false, false, true), phase + " cannot replace a live world.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 1, 0, false, false, false, false, true), phase + " cannot start with a party.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 0, 0, true, false, false, false, true), phase + " cannot start in a loaded area.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 0, 0, false, false, true, false, true), phase + " cannot import a save.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 0, 0, false, false, false, false, false), phase + " requires the idle menu.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(scenario, 2, phase), phase + " refuses an extra parameter.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(scenario, 0, phase), phase + " refuses a missing parameter.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(scenario, 1, phase + " "), phase + " refuses a malformed name.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(scenario, 1, phase.ToUpperInvariant()), phase + " refuses a case variant.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, RuntimeTestScenarioCatalog.DisposableExpandedSummoningCleanup2NativeReview, 1, phase, 0, 0, 0, false, false, false, false, true),
                    phase + " is refused under another scenario.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32NativeDefaultOutdoor(RuntimeTestScenarioCatalog.DisposableExpandedSummoningCleanup2NativeReview, phase), phase + " lends no outdoor admission to another scenario.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32NativeInputPhase(phase), phase + " grants no player input.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.WallFixturePhase(phase), phase + " never acquires Wall preparation.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint40Phase(phase), phase + " is not host-restricted.");
            }
            foreach (string phase in outdoor)
                Assertions.True(ExpandedSummoningNativeProfilePolicy.Sprint32NativeDefaultOutdoor(scenario, phase) && ExpandedSummoningNativeProfilePolicy.Sprint7OutdoorPhase(phase), phase + " uses the outdoor native Default fixture.");
            foreach (string phase in interior)
                Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32NativeDefaultOutdoor(scenario, phase) || ExpandedSummoningNativeProfilePolicy.Sprint7OutdoorPhase(phase), phase + " stays in the static interior.");
            foreach (string near in new[] { "summon-s7", "summon-s7-", "summon-s7-fx", "summon-s7-ghaele-dim", "summon-s7-trumpet" })
                Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint7Phase(near) || ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(scenario, 1, near), near + " is not admitted.");
            // Screen capture: the production decision NativeIconScreenEvidence.Supports consumes.
            Assertions.True(ExpandedSummoningNativeProfilePolicy.Sprint32ScreenCaptureAdmitted(scenario, 1, ExpandedSummoningNativeProfilePolicy.Sprint39S7UiPhase),
                "The Sprint 7 UI phase is admitted to screen capture.");
            foreach (string phase in outdoor.Concat(interior).Where(value => value != ExpandedSummoningNativeProfilePolicy.Sprint39S7UiPhase))
                Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32ScreenCaptureAdmitted(scenario, 1, phase), phase + " is refused screen capture.");
            foreach (int count in new[] { 0, 2 })
                Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32ScreenCaptureAdmitted(scenario, count, ExpandedSummoningNativeProfilePolicy.Sprint39S7UiPhase),
                    "Screen capture refuses " + count + " parameters.");
            foreach (string malformed in new[] { "summon-s7-ui ", "SUMMON-S7-UI", "summon-s7-ui-hud", "", null })
                Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32ScreenCaptureAdmitted(scenario, 1, malformed), "Malformed phase '" + malformed + "' is refused screen capture.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32ScreenCaptureAdmitted(RuntimeTestScenarioCatalog.DisposableExpandedSummoningCleanup2NativeReview, 1,
                ExpandedSummoningNativeProfilePolicy.Sprint39S7UiPhase), "Another scenario cannot borrow the Sprint 7 capture admission.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.Sprint32ScreenCaptureAdmitted(scenario, 1, ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestUiPhase),
                "The existing R3 UI capture admission is unchanged.");
        }

        // F4, R2-F3, R2-F4, F14: observers act only on what they acquired and
        // report an incomplete rollback beside the original failure.
        internal static void Sprint7ObserverOwnsOnlyItsAcquisitions()
        {
            Assertions.True(Sprint7ObserverPolicy.CanOpenWindow(false, false, false), "A clear screen admits the observer's window.");
            Assertions.False(Sprint7ObserverPolicy.CanOpenWindow(true, false, false), "An initially open description window refuses the phase.");
            Assertions.False(Sprint7ObserverPolicy.CanOpenWindow(false, true, false), "An open hover tooltip refuses the phase.");
            Assertions.False(Sprint7ObserverPolicy.CanOpenWindow(false, false, true), "An unrelated modal refuses the phase.");
            object opened = new object(), foreign = new object();
            Assertions.True(Sprint7ObserverPolicy.OwnsWindow(opened, opened), "The window showing the observer's own data is its own.");
            Assertions.False(Sprint7ObserverPolicy.OwnsWindow(foreign, opened), "A replaced or foreign window is never closed by the observer.");
            Assertions.False(Sprint7ObserverPolicy.OwnsWindow(null, null) || Sprint7ObserverPolicy.OwnsWindow(foreign, null), "With nothing opened the observer owns nothing.");

            // Control facts and borrowed abilities: grant only when absent;
            // release only the observer's own instance while it is still held.
            Assertions.False(Sprint7ObserverPolicy.ShouldGrant(true), "An existing control is used as it is, never granted again.");
            Assertions.True(Sprint7ObserverPolicy.ShouldGrant(false), "A missing control is granted.");
            object original = new object(), granted = new object(), replacement = new object();
            Assertions.False(Sprint7ObserverPolicy.ShouldRelease(null, original), "An existing (not granted) control is never released.");
            Assertions.True(Sprint7ObserverPolicy.ShouldRelease(granted, granted), "The observer's own grant is released.");
            Assertions.False(Sprint7ObserverPolicy.ShouldRelease(granted, replacement), "A replacement made during a yielded window survives.");
            Assertions.False(Sprint7ObserverPolicy.ShouldRelease(granted, null), "A grant already gone is not released twice.");
            string observer = File.ReadAllText("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs");
            Assertions.True(observer.Contains("used.OverrideParams = priorOverride") && observer.Contains("Abilities.RemoveFact(granted)") &&
                observer.Contains("Abilities.RemoveFact(entry.Value)"), "Borrowed overrides are restored and grants are released by instance.");

            // Conditions: only the breath-owned instances are removed.
            object breathOwned = new object(), otherContext = new object(), laterBreath = new object();
            List<object> removed = Sprint7ObserverPolicy.Acquired(new[] { otherContext, breathOwned, laterBreath }, new[] { breathOwned, laterBreath });
            Assertions.Equal(2, removed.Count, "Both breath-owned instances are selected.");
            Assertions.False(removed.Contains(otherContext), "A Sickened from another context survives the reset and the cleanup.");

            // Rollback: every release runs; failures are reported with the original cause.
            var order = new List<string>();
            List<string> failures = Sprint7ObserverPolicy.RunReleases(new[] {
                new KeyValuePair<string, Action>("first", () => order.Add("first")),
                new KeyValuePair<string, Action>("broken", () => { throw new InvalidOperationException("cannot unpatch"); }),
                new KeyValuePair<string, Action>("last", () => order.Add("last")) });
            Assertions.Equal("first,last", string.Join(",", order.ToArray()), "One failing release does not skip the later ones.");
            Assertions.Equal(1, failures.Count, "The failing release is recorded.");
            var cause = new InvalidOperationException("injected failure after the first patch");
            InvalidOperationException reported = Sprint7ObserverPolicy.SetupFailure("S7ProjectileTrace", cause, failures, 2);
            Assertions.True(ReferenceEquals(reported.InnerException, cause), "The original setup failure is preserved as the cause.");
            Assertions.True(reported.Message.Contains("rollback INCOMPLETE: 2 owned item(s) remain") && reported.Message.Contains("cannot unpatch"),
                "An incomplete rollback is reported, not hidden.");
            Assertions.True(Sprint7ObserverPolicy.SetupFailure("S7BodySwap", cause, new List<string>(), 0).Message.Contains("rollback complete"),
                "A complete rollback says so.");
            Assertions.True(observer.Contains("Inject == \"after-first-patch\"") && observer.Contains("Inject == \"after-comparison-mesh\"") && observer.Contains("s7-observer-rollback"),
                "Both observers carry the bounded native failure checks after partial acquisition.");
        }

        // R2-F1: the reload probe judges the saved cue by the product's own
        // records: playing ID, native end, and for a retirement the recorded
        // stop result and ordering.
        internal static void TrumpetReloadVerdict()
        {
            const string requested = "stop-requested";
            DateTime post = new DateTime(2026, 11, 24, 2, 26, 2, DateTimeKind.Utc);
            DateTime end = post.AddSeconds(3.9), stop = post.AddSeconds(2);
            Assertions.Equal("natural", KmgLoaderCompatHarness.TrumpetCueVerdict.SavedCue(159, 159, post, end, null, null, null, requested), "A natural end is accepted.");
            Assertions.Equal("retired:view-destroyed", KmgLoaderCompatHarness.TrumpetCueVerdict.SavedCue(159, 159, post, end, "view-destroyed", requested, stop, requested),
                "A retirement with the product's recorded stop-requested result, before the native end, is accepted.");
            Assertions.Equal("invalid:stop-not-requested", KmgLoaderCompatHarness.TrumpetCueVerdict.SavedCue(159, 159, post, end, "view-destroyed", "stop-failed", stop, requested),
                "A failed stop is rejected.");
            Assertions.Equal("invalid:stop-not-requested", KmgLoaderCompatHarness.TrumpetCueVerdict.SavedCue(159, 159, post, end, "view-destroyed", "StopRequested", stop, requested),
                "Only the product's exact recorded value counts.");
            Assertions.Equal("invalid:no-native-end", KmgLoaderCompatHarness.TrumpetCueVerdict.SavedCue(159, 159, post, null, "view-destroyed", requested, stop, requested),
                "A missing end-of-event callback is rejected, even after a stop request.");
            Assertions.Equal("invalid:playing-id", KmgLoaderCompatHarness.TrumpetCueVerdict.SavedCue(159, 160, post, end, null, null, null, requested), "Another playing ID is rejected.");
            Assertions.Equal("invalid:end-before-post", KmgLoaderCompatHarness.TrumpetCueVerdict.SavedCue(159, 159, post, post.AddSeconds(-1), null, null, null, requested), "An end before the post is rejected.");
            Assertions.Equal("invalid:stop-order", KmgLoaderCompatHarness.TrumpetCueVerdict.SavedCue(159, 159, post, end, "view-destroyed", requested, end.AddSeconds(1), requested),
                "A stop requested after the end is rejected.");
            Assertions.True(KmgLoaderCompatHarness.TrumpetCueVerdict.Accepted("natural") && KmgLoaderCompatHarness.TrumpetCueVerdict.Accepted("retired:entity-retired") &&
                !KmgLoaderCompatHarness.TrumpetCueVerdict.Accepted("invalid:no-native-end"), "Only natural ends and valid retirements are accepted.");
            string probe = File.ReadAllText("tools/LoaderCompatHarness/TrumpetCueProbes.cs");
            Assertions.True(probe.Contains("GetField(\"StopRequested\", Any)") && probe.Contains("TrumpetCueVerdict.SavedCue("),
                "The probe reads the product's own stop-result constant and applies this verdict.");
        }

        // R2-F2: abandoned while yielded, a probe restores pause, its event
        // subscription and the shared generator exactly once, newest first;
        // a failing restore is reported and the others still run.
        internal static void TrumpetProbeAbandonmentRestores()
        {
            bool paused = true, subscribed = false; int rng = 5;
            var order = new List<string>();
            var owned = new KmgLoaderCompatHarness.OwnedProbeResources();
            Func<IEnumerable<object>> body = () => Body(owned, () => paused = false, () => subscribed = true, () => rng = 9,
                () => { paused = true; order.Add("pause"); }, () => { subscribed = false; order.Add("subscription"); }, () => { rng = 5; order.Add("rng"); });
            IEnumerator<object> script = body().GetEnumerator();
            Assertions.True(script.MoveNext() && !paused && subscribed && rng == 9, "While yielded the probe has unpaused, subscribed and seeded.");
            script.Dispose(); // the slot abandons the probe (timeout) while it is yielded
            Assertions.True(paused && !subscribed && rng == 5, "Disposing the yielded iterator restores pause, subscription and the generator exactly.");
            Assertions.Equal("rng,subscription,pause", string.Join(",", order.ToArray()), "Restores run newest first.");
            KmgLoaderCompatHarness.CleanupReport again = owned.ReleaseAll();
            Assertions.True(again.Restored && again.Attempted.Count == 0 && order.Count == 3, "A later release finds nothing left and changes nothing.");

            var failing = new KmgLoaderCompatHarness.OwnedProbeResources();
            bool later = false;
            failing.Hold("first", () => later = true);
            failing.Hold("broken", () => { throw new InvalidOperationException("cannot unsubscribe"); });
            KmgLoaderCompatHarness.CleanupReport report = failing.ReleaseAll();
            Assertions.True(later && !report.Restored && report.Failures.Count == 1, "A failing restore is reported and the others still run.");
            var early = new KmgLoaderCompatHarness.OwnedProbeResources();
            int seeded = 1; var failures = new List<string>();
            early.Hold("blast seeding", () => seeded = 0);
            early.ReleaseNow("blast seeding", failures);
            Assertions.True(seeded == 0 && early.Count == 0 && failures.Count == 0, "An early release runs right away, once.");
            Assertions.True(early.Outcome.Attempted.Count == 1 && early.Outcome.Restored, "Its outcome is kept for the slot's report.");
        }

        // R3-F2: a yielded probe built exactly as the Trumpet and Ghaele probes
        // are (YieldedProbeLifecycle, held restores, an iterator finally that
        // drains them), with an injectable failing restore.
        private sealed class LifecycleProbe : KmgLoaderCompatHarness.IOwnedProbe
        {
            internal readonly KmgLoaderCompatHarness.OwnedProbeResources Owned = new KmgLoaderCompatHarness.OwnedProbeResources();
            internal readonly KmgLoaderCompatHarness.YieldedProbeLifecycle Life;
            internal int Pause, Unsubscribe, Rng, Units;
            private readonly bool failUnsubscribe;
            private readonly int yields;

            internal LifecycleProbe(bool failUnsubscribe, int yields)
            {
                this.failUnsubscribe = failUnsubscribe; this.yields = yields;
                Life = new KmgLoaderCompatHarness.YieldedProbeLifecycle(Owned, lifecycle => Body());
            }

            private IEnumerable<object> Body()
            {
                Owned.Hold("pause", () => Pause++);
                Owned.Hold("save seeding subscription", () => { Unsubscribe++; if (failUnsubscribe) throw new InvalidOperationException("cannot unsubscribe"); });
                Owned.Hold("rng", () => Rng++);
                try { for (int index = 0; index < yields; index++) yield return null; }
                finally
                {
                    KmgLoaderCompatHarness.CleanupReport held = Owned.ReleaseAll();
                    if (!held.Restored) Life.Problems.Add("restore failures: " + string.Join("; ", held.Failures.ToArray()));
                }
            }

            public object Result { get { return "recorded"; } }
            public string Poll() { return Life.Poll(); }
            public KmgLoaderCompatHarness.CleanupReport Release(string testFailure)
            {
                return Life.Release(new[] { new KeyValuePair<string, Action>("disposable units", () => Units++) });
            }
        }

        // R3-F2: a restore that fails while the iterator is disposed reaches the
        // slot: the named failure, every other release attempted once, the
        // original test error kept, IsolationRestored=false, and no further probe.
        internal static void ProbeCleanupReachesSlot()
        {
            const string owner = "3|kmgTrumpetCue_before";
            var slot = new KmgLoaderCompatHarness.ProbeSlot();
            var probe = new LifecycleProbe(true, 5);
            slot.Begin(owner, probe);
            Assertions.True(slot.Poll(owner) == null, "The probe is yielded.");
            KmgLoaderCompatHarness.ProbeOutcome outcome = slot.Abandon("step timeout after 180s");
            Assertions.False(outcome.IsolationRestored, "The failed unsubscribe reaches the slot as unrestored isolation.");
            Assertions.Equal("step timeout after 180s", outcome.TestFailure, "The original test error is kept.");
            Assertions.True(outcome.Failure.Contains("save seeding subscription") && outcome.Failure.Contains("cannot unsubscribe"), "The failure is named.");
            Assertions.True(probe.Pause == 1 && probe.Unsubscribe == 1 && probe.Rng == 1 && probe.Units == 1, "Every release is attempted exactly once.");
            Assertions.True(outcome.Cleanup.Attempted.Contains("pause") && outcome.Cleanup.Attempted.Contains("rng") && outcome.Cleanup.Attempted.Contains("disposable units"),
                "The remaining releases are attempted after the failure.");
            Assertions.False(slot.IsolationRestored, "The slot records the unrestored isolation.");
            bool refused = false;
            try { slot.Begin("4|kmgTrumpetCue_after", new LifecycleProbe(false, 1)); }
            catch (InvalidOperationException error) { refused = error.Message.Contains("isolation was not restored"); }
            Assertions.True(refused, "No other probe may start.");
            Assertions.True(KmgLoaderCompatHarness.ProbeSlot.Decide(false, true, slot.IsolationRestored) == KmgLoaderCompatHarness.StepContinuation.StopLaunch,
                "The launch stops.");

            // The same failure at an ordinary end: the verdict and the slot both carry it.
            var ending = new KmgLoaderCompatHarness.ProbeSlot();
            var finished = new LifecycleProbe(true, 1);
            ending.Begin(owner, finished);
            Assertions.True(ending.Poll(owner) == null, "First poll yields.");
            KmgLoaderCompatHarness.ProbeOutcome ended = ending.Poll(owner);
            Assertions.True(ended != null && !ended.IsolationRestored && ended.TestFailure.Contains("cannot unsubscribe"), "An ordinary end with a failed restore fails isolation and the verdict.");
            Assertions.True(finished.Pause == 1 && finished.Unsubscribe == 1 && finished.Rng == 1 && finished.Units == 1, "Each release still runs once.");

            // Control: nothing fails, isolation restored, the probe passes.
            var clean = new KmgLoaderCompatHarness.ProbeSlot();
            var fine = new LifecycleProbe(false, 2);
            clean.Begin(owner, fine);
            clean.Poll(owner);
            KmgLoaderCompatHarness.ProbeOutcome abandoned = clean.Abandon("step timeout after 180s");
            Assertions.True(abandoned.IsolationRestored && clean.IsolationRestored && fine.Unsubscribe == 1 && fine.Units == 1, "A clean abandonment restores isolation.");
        }

        // R2-F2 (round 4): no shared-generator intervention. Only the d20 rule whose
        // parent on the rule stack is an open owned save is claimed (pre-rolled),
        // once per save; a foreign save dispatched inside the owned save's
        // callbacks, its d20 and any foreign draw are untouched; abandonment
        // clears everything.
        internal static void TrumpetOwnedD20Claim()
        {
            var claim = new KmgLoaderCompatHarness.OwnedD20Claim();
            object ownedSave = new object(), foreignSave = new object(), unrelatedSave = new object();
            Assertions.False(claim.Claim(ownedSave), "Nothing is claimed before the owned save opens.");
            claim.Begin(ownedSave);
            Assertions.True(claim.Open, "The owned save is open between its callbacks.");
            Assertions.False(claim.Claim(foreignSave), "A foreign save dispatched inside the owned save is not claimed (its d20's parent is the foreign save).");
            Assertions.False(claim.Claim(null), "A d20 with no parent rule is not claimed.");
            Assertions.True(claim.Claim(ownedSave), "The owned save's own d20 is claimed.");
            Assertions.False(claim.Claim(ownedSave), "It is claimed once.");
            claim.End(ownedSave);
            Assertions.True(!claim.Open && !claim.Claim(ownedSave), "After the owned save ends nothing more is claimed.");
            Assertions.False(claim.Claim(unrelatedSave), "An unrelated save is never claimed.");
            Assertions.Equal(1, claim.Claims, "Exactly one d20 was pre-rolled.");
            claim.Begin(ownedSave);
            claim.Abandon();
            Assertions.True(!claim.Open && !claim.Claim(ownedSave), "Abandonment closes the intervention.");
            // A save dispatched inside another save's callbacks inherits the blast's rule context but is not the blast's.
            Func<object, bool> isSave = rule => rule is string && ((string)rule).StartsWith("save", StringComparison.Ordinal);
            object blastSave = "save-blast", nestedSave = "save-nested", ability = "ability-rule";
            Assertions.True(KmgLoaderCompatHarness.OwnedD20Claim.TopLevelSave(new[] { ability, blastSave }, blastSave, isSave), "The blast's own save is top level.");
            Assertions.False(KmgLoaderCompatHarness.OwnedD20Claim.TopLevelSave(new[] { ability, blastSave, nestedSave }, nestedSave, isSave), "A save nested inside the blast save is not the blast's.");
        }

        private static JObject Anim(bool started, bool delayed, double delay, double lifetime, bool loop, bool cutout, double current)
        {
            return new JObject { ["IsStarted"] = started, ["IsDelayed"] = delayed, ["Delay"] = delay, ["Lifetime"] = lifetime, ["LoopAnimation"] = loop,
                ["DissolveCutout"] = cutout, ["PlayBackwards"] = false, ["CurrentDissolve"] = current };
        }

        private static JObject Snapshot(JArray animations, JArray driven, JArray body)
        {
            var snapshot = new JObject { ["t"] = 10, ["controllers"] = animations == null ? new JArray() :
                new JArray(new JObject { ["controller"] = 1, ["animations"] = animations, ["driven"] = driven ?? new JArray() }) };
            if (body != null) snapshot["bodyMaterials"] = body;
            return snapshot;
        }

        private static JObject Donor(params JObject[] animations)
        {
            return new JObject { ["t"] = 10, ["controllers"] = new JArray(new JObject { ["controller"] = 2, ["animations"] = new JArray(animations),
                ["driven"] = new JArray(new JObject { ["material"] = 21, ["dissolve"] = .4 }) }) };
        }

        // R3-F6 (round 4): only positive samples attribute. Current snapshots
        // need a readable body (explicit null is inconclusive); qualifying donor
        // loops must have been observed running with a readable matching value;
        // the retained round-3 (legacy) and round-4 (current) rows are re-evaluated.
        internal static void GhaeleAttributionRequiresPositiveSamples()
        {
            JObject activeLoop() { return Anim(true, false, 4.75, 5, true, false, .4); }
            JObject appearance() { return Anim(false, false, 0, 3, false, false, 0); }
            var donors = new[] { Donor(activeLoop(), appearance()) };
            var configured = KmgLoaderCompatHarness.GhaeleAttribution.Configured(donors);
            var qualifying = KmgLoaderCompatHarness.GhaeleAttribution.Qualifying(donors);
            Assertions.True(qualifying.Count == 1 && configured.Count == 2, "Only the donor's observed running loop qualifies; both animations are configured.");
            Func<JArray> driven = () => new JArray(new JObject { ["material"] = 11, ["dissolve"] = .4 });
            Func<JObject, string> classify = row => KmgLoaderCompatHarness.GhaeleAttribution.Classify(row, configured, qualifying);
            Assertions.Equal("positive", classify(Snapshot(new JArray(activeLoop()), driven(), new JArray(11))), "An active qualifying loop driving a body material confirms.");
            Assertions.Equal("positive", classify(Snapshot(new JArray(activeLoop()), driven(), null)), "A legacy row without the body property is judged without body identity.");
            var explicitNull = Snapshot(new JArray(activeLoop()), driven(), null); explicitNull["bodyMaterials"] = JValue.CreateNull();
            Assertions.Equal("inconclusive:no-readable-body", classify(explicitNull), "A current row whose body could not be read does not confirm.");
            Assertions.Equal("inconclusive:no-controller", classify(Snapshot(null, null, new JArray(11))), "An empty controller set does not confirm.");
            Assertions.Equal("inconclusive:no-animation", classify(Snapshot(new JArray(), driven(), new JArray(11))), "A controller without animations does not confirm.");
            Assertions.Equal("inconclusive:no-active-animation", classify(Snapshot(new JArray(Anim(false, false, 4.75, 5, true, false, 0)), driven(), new JArray(11))), "An inactive animation does not confirm.");
            Assertions.Equal("inconclusive:no-active-animation", classify(Snapshot(new JArray(Anim(true, true, 4.75, 5, true, false, 0)), driven(), new JArray(11))), "A delayed-only animation does not confirm.");
            Assertions.Equal("inconclusive:no-readable-driven-value", classify(Snapshot(new JArray(activeLoop()), new JArray(new JObject { ["material"] = 11, ["dissolve"] = -1 }), new JArray(11))), "An unreadable driven value does not confirm.");
            Assertions.Equal("inconclusive:no-driven-body-material", classify(Snapshot(new JArray(activeLoop()), driven(), new JArray(99))), "A driven material that is not the body's does not confirm.");
            Assertions.Equal("inconclusive:no-active-loop", classify(Snapshot(new JArray(Anim(true, false, 0, 3, false, false, .4)), driven(), new JArray(11))), "Only the appearance animation does not confirm a loop.");
            Assertions.Equal("contradicting", classify(Snapshot(new JArray(Anim(true, false, 4.75, 5, true, true, .4)), driven(), new JArray(11))), "A cutout dissolve contradicts.");
            Assertions.Equal("contradicting", classify(Snapshot(new JArray(Anim(true, false, 2, 5, true, false, .4)), driven(), new JArray(11))), "An animation the donor does not configure contradicts.");
            Assertions.Equal("contradicting", classify(Snapshot(new JArray(activeLoop()), new JArray(new JObject { ["material"] = 11, ["dissolve"] = .9 }), new JArray(11))), "A disagreeing driven value contradicts.");
            // A donor that only configures, or only delays, the loop never qualifies it.
            foreach (JObject idle in new[] { Anim(false, false, 4.75, 5, true, false, 0), Anim(true, true, 4.75, 5, true, false, 0) })
            {
                var lazy = new[] { Donor(idle, appearance()) };
                JObject verdict = KmgLoaderCompatHarness.GhaeleAttribution.Evaluate(Enumerable.Repeat(Snapshot(new JArray(activeLoop()), driven(), new JArray(11)), 6), lazy);
                Assertions.True(!(bool)verdict["attributed"] && (int)verdict["positive"] == 0, "A donor loop never observed running does not attribute.");
            }
            Func<int, int, int, JObject> evaluate = (positive, inconclusive, contradicting) => KmgLoaderCompatHarness.GhaeleAttribution.Evaluate(
                Enumerable.Repeat(Snapshot(new JArray(activeLoop()), driven(), new JArray(11)), positive)
                    .Concat(Enumerable.Repeat(Snapshot(null, null, new JArray(11)), inconclusive))
                    .Concat(Enumerable.Repeat(Snapshot(new JArray(Anim(true, false, 4.75, 5, true, true, .4)), driven(), new JArray(11)), contradicting)), donors);
            Assertions.False((bool)evaluate(0, 6, 0)["attributed"], "Only inconclusive samples never attribute.");
            Assertions.False((bool)evaluate(4, 4, 0)["attributed"], "Fewer than five positive samples do not attribute.");
            Assertions.False((bool)evaluate(6, 0, 1)["attributed"], "Any contradiction refuses attribution.");
            Assertions.True((bool)evaluate(5, 3, 0)["attributed"], "Five positive samples and no contradiction attribute.");

            // Re-evaluation of the retained released (v0.0.195) and candidate rows, as recorded.
            foreach (var retained in new[] {
                new { Folder = "r3", Name = "20261124T052619Z-owner-0.33.0-s7-ghaele-01-ghaele.json", Body = "not-recorded" },
                new { Folder = "r3", Name = "20261124T052954Z-owner-0.33.0-s7-ghaele-01-ghaele.json", Body = "not-recorded" },
                new { Folder = "r4", Name = "20261124T162819Z-owner-0.33.0-s7-ghaele-01-ghaele.json", Body = "recorded" },
                new { Folder = "r4", Name = "20261124T163235Z-owner-0.33.0-s7-ghaele-01-ghaele.json", Body = "recorded" } })
            {
                JObject probe = (JObject)JObject.Parse(File.ReadAllText(Path.Combine("docs/evidence/sprint7/" + retained.Folder, retained.Name)))["harness"]["kmgGhaeleLifetime_timeline"];
                double appeared = (double)probe["timeline"]["appearanceEndedAt"], ends = (double)probe["timeline"]["lifetimeEndsAt"];
                var inLife = probe["controllers"].Where(row => (double)row["t"] >= appeared + 1 && (ends < 0 || (double)row["t"] < ends - .5)).ToList();
                JObject result = KmgLoaderCompatHarness.GhaeleAttribution.Evaluate(inLife, probe["donorControllers"]);
                result.Remove("classes");
                Console.WriteLine("GHAELE-REEVALUATION " + retained.Folder + "/" + retained.Name + " productVersion=" + probe["productVersion"] + " " + result.ToString(Newtonsoft.Json.Formatting.None));
                Assertions.True((bool)result["attributed"] && (int)result["contradicting"] == 0 && (int)result["positive"] >= KmgLoaderCompatHarness.GhaeleAttribution.MinimumPositive,
                    retained.Name + ": the retained rows still attribute through positive samples and observed donor loops.");
                Assertions.Equal(retained.Body, (string)result["bodyMaterialIdentity"], retained.Name + ": body identity as recorded.");
                if (retained.Folder == "r4")
                    foreach (JObject light in probe["lighting"].OfType<JObject>())
                    {
                        string why;
                        Assertions.True(KmgLoaderCompatHarness.GhaeleLighting.Valid(light, out why), retained.Name + " " + light["label"] + ": the retained lighting row qualifies (" + why + ").");
                    }
            }
        }

        // R4-F6: a lighting observation counts only with a readable body, a clear
        // cutout, a light-probe reading and a usable capture of that body.
        internal static void GhaeleLightingAndLoanVerdicts()
        {
            Func<JObject> good = () => new JObject { ["label"] = "idle", ["body"] = "GhaeleAzata_R", ["bodyMesh"] = "GhaeleAzata_Mesh", ["bodyDissolveCutout"] = 0.0,
                ["probeDcLuminance"] = .24, ["partyCapture"] = "png=s7-ghaele-idle.png;bytes=1;viewport=0.5,0.5;inFrame=true;screenFade=0;luma=36;screenLit=true;rendererEnabled=true;faderVisible=true;mesh=GhaeleAzata_Mesh;dissolve=0;intact=true" };
            string why;
            Assertions.True(KmgLoaderCompatHarness.GhaeleLighting.Valid(good(), out why), "A complete lighting row qualifies.");
            var noBody = good(); noBody["body"] = JValue.CreateNull();
            Assertions.False(KmgLoaderCompatHarness.GhaeleLighting.Valid(noBody, out why) || why != "no-body", "A missing body fails.");
            var empty = good(); empty["partyCapture"] = "png=<none>;reason=empty-render;inFrame=false";
            Assertions.False(KmgLoaderCompatHarness.GhaeleLighting.Valid(empty, out why) || why != "no-image", "An empty capture fails.");
            var noCapture = good(); noCapture.Remove("partyCapture");
            Assertions.False(KmgLoaderCompatHarness.GhaeleLighting.Valid(noCapture, out why), "No capture fails.");
            var probeError = good(); probeError["probeError"] = "ArgumentException";
            Assertions.False(KmgLoaderCompatHarness.GhaeleLighting.Valid(probeError, out why) || why != "no-light-probe", "A failed light-probe reading fails.");
            var cut = good(); cut["bodyDissolveCutout"] = .5;
            Assertions.False(KmgLoaderCompatHarness.GhaeleLighting.Valid(cut, out why) || why != "cutout", "A cut-out body fails.");
            var wrong = good(); wrong["partyCapture"] = ((string)wrong["partyCapture"]).Replace("mesh=GhaeleAzata_Mesh", "mesh=Wing_Mesh");
            Assertions.False(KmgLoaderCompatHarness.GhaeleLighting.Valid(wrong, out why) || why != "wrong-body", "A capture of another mesh fails.");

            // R4-F3: the loan verdict against the exact prior state.
            object existing = new object(), prior = new object(), granted = new object(), replacement = new object(), other = new object();
            Assertions.True(KmgLoaderCompatHarness.GhaeleLoan.Released(true, existing, existing, null, prior, prior, out why), "A borrowed ability with its own (even CL20) parameter object restored passes.");
            Assertions.False(KmgLoaderCompatHarness.GhaeleLoan.Released(true, null, existing, null, null, prior, out why) || why != "borrowed-missing", "A vanished borrowed ability fails.");
            Assertions.False(KmgLoaderCompatHarness.GhaeleLoan.Released(true, existing, existing, null, other, prior, out why) || why != "parameters-not-restored", "Incorrectly restored parameters fail.");
            Assertions.False(KmgLoaderCompatHarness.GhaeleLoan.Released(true, replacement, existing, null, prior, prior, out why) || why != "borrowed-replaced", "A different instance in place of the borrowed one fails.");
            Assertions.True(KmgLoaderCompatHarness.GhaeleLoan.Released(false, null, null, granted, null, null, out why), "A removed temporary grant passes.");
            Assertions.True(KmgLoaderCompatHarness.GhaeleLoan.Released(false, replacement, null, granted, null, null, out why), "A distinct replacement passes.");
            Assertions.False(KmgLoaderCompatHarness.GhaeleLoan.Released(false, granted, null, granted, null, null, out why) || why != "grant-retained", "A retained grant fails.");
        }

        // R4-F7: a final release over several owned items attempts each item once;
        // a failure on the first is reported under its identity, the second still
        // runs, and the slot keeps the original error and failed isolation.
        private sealed class ItemsProbe : KmgLoaderCompatHarness.IOwnedProbe
        {
            internal readonly KmgLoaderCompatHarness.OwnedProbeResources Owned = new KmgLoaderCompatHarness.OwnedProbeResources();
            internal readonly KmgLoaderCompatHarness.YieldedProbeLifecycle Life;
            internal readonly Dictionary<string, int> Releases = new Dictionary<string, int> { ["unit-1"] = 0, ["unit-2"] = 0 };
            internal ItemsProbe() { Life = new KmgLoaderCompatHarness.YieldedProbeLifecycle(Owned, lifecycle => Body()); }
            private IEnumerable<object> Body() { yield return null; yield return null; }
            public object Result { get { return "recorded"; } }
            public string Poll() { return Life.Poll(); }
            public KmgLoaderCompatHarness.CleanupReport Release(string testFailure)
            {
                return Life.Release(KmgLoaderCompatHarness.ProbeReleases.PerItem("disposable unit", new[] { "unit-1", "unit-2" }, id => id, id =>
                {
                    Releases[id]++;
                    if (id == "unit-1") throw new InvalidOperationException("cannot destroy unit-1");
                }));
            }
        }

        internal static void ProbeFinalReleasesPerItem()
        {
            var slot = new KmgLoaderCompatHarness.ProbeSlot();
            var probe = new ItemsProbe();
            slot.Begin("2|kmgGhaeleLifetime_timeline", probe);
            Assertions.True(slot.Poll("2|kmgGhaeleLifetime_timeline") == null, "Yielded.");
            KmgLoaderCompatHarness.ProbeOutcome outcome = slot.Abandon("step timeout after 360s");
            Assertions.True(probe.Releases["unit-1"] == 1 && probe.Releases["unit-2"] == 1, "Each item's release is attempted exactly once, the second after the first failed.");
            Assertions.True(outcome.Failure.Contains("disposable unit unit-1") && outcome.Failure.Contains("cannot destroy unit-1") && !outcome.Failure.Contains("disposable unit unit-2:"),
                "The failure names the item.");
            Assertions.True(!outcome.IsolationRestored && outcome.TestFailure == "step timeout after 360s" && !slot.IsolationRestored, "Failed isolation and the original error reach the slot.");
            Assertions.True(outcome.Cleanup.Attempted.Contains("disposable unit unit-2"), "The second item was attempted.");
        }

        // R3-F3 / R3-F4 / R1-F3: the cell's condition ledger, the trace release
        // verdict and the party-capture verdict.
        internal static void Sprint7CellOwnershipAndCaptureVerdicts()
        {
            object foreign = new object(), preexisting = new object();
            var owned = new HashSet<object>();
            Func<object, bool> fromCellBreath = value => owned.Contains(value);
            var present = new List<object> { foreign, preexisting };
            owned.Add(preexisting); // a Dust-origin instance already present when the cell began is never the cell's
            var ledger = new AcquisitionLedger<object>(present);
            // Three mechanics casts, each observed after its readback.
            for (int cast = 0; cast < 3; cast++) { var applied = new object(); owned.Add(applied); present.Add(applied); Assertions.Equal(1, ledger.Observe(present, fromCellBreath), "Each mechanics cast's instance is acquired."); }
            // Later concurrent, paired, nested and final casts, and an interrupted command, never observed before cleanup.
            for (int cast = 0; cast < 5; cast++) { var later = new object(); owned.Add(later); present.Add(later); }
            var removed = new List<object>();
            List<string> failures = ledger.Release(present, fromCellBreath, value => { removed.Add(value); present.Remove(value); });
            Assertions.True(failures.Count == 0 && removed.Count == 8 && ledger.LastRemoved == 8, "Cleanup acquires and removes every instance the cell's executions introduced, observed or not.");
            Assertions.True(present.Contains(foreign) && present.Contains(preexisting), "The foreign control and an instance present before the cell survive.");
            Assertions.Equal(0, ledger.Remaining(present, fromCellBreath), "No cell-owned condition remains.");

            // Early exit right after application: its own ledger, released by its finally.
            var early = new AcquisitionLedger<object>(present);
            var applying = new object(); owned.Add(applying); present.Add(applying);
            Assertions.Equal(1, early.Observe(present, fromCellBreath), "The early exit sees its application.");
            failures = early.Release(present, value => fromCellBreath(value), value => { if (ReferenceEquals(value, applying)) throw new InvalidOperationException("cannot remove"); present.Remove(value); });
            Assertions.True(failures.Count == 1 && early.Remaining(present, fromCellBreath) == 1, "A failed removal is reported and its instance is still counted as remaining.");

            Assertions.True(Sprint7ObserverPolicy.Released(new List<string>(), 0), "No failure and no hook: released.");
            Assertions.False(Sprint7ObserverPolicy.Released(new List<string> { "CreateView hooks: injected failure" }, 2), "A failed unpatch with hooks left fails the verdict.");
            Assertions.False(Sprint7ObserverPolicy.Released(new List<string>(), 1), "A remaining owned hook fails the verdict even without a reported failure.");

            const string good = "png=s7-nalfeshnee-bright-after-party.png;bytes=812345;viewport=0.5,0.48;inFrame=true;screenFade=0;luma=61.2;screenLit=true;rendererEnabled=true;faderVisible=true;mesh=KMG_Nalfeshnee_Body;dissolve=0;intact=true;material=x;visual=y";
            string reason;
            Assertions.True(Sprint7ObserverPolicy.PartyCaptureValid(good, "KMG_Nalfeshnee_Body", out reason) && reason == null, "A lit, in-frame image of the enabled body passes.");
            Assertions.False(Sprint7ObserverPolicy.PartyCaptureValid("png=<none>;reason=empty-render;inFrame=false", "KMG_Nalfeshnee_Body", out reason), "An absent image fails.");
            Assertions.Equal("no-image:empty-render", reason, "The absent image is named.");
            Assertions.False(Sprint7ObserverPolicy.PartyCaptureValid(good.Replace("mesh=KMG_Nalfeshnee_Body", "mesh=_ammunition04"), "KMG_Nalfeshnee_Body", out reason) || reason != "wrong-body", "A capture naming an accessory fails.");
            Assertions.False(Sprint7ObserverPolicy.PartyCaptureValid(good.Replace("rendererEnabled=true", "rendererEnabled=false"), "KMG_Nalfeshnee_Body", out reason) || reason != "renderer-disabled", "A disabled renderer fails.");
            Assertions.False(Sprint7ObserverPolicy.PartyCaptureValid(good.Replace("screenLit=true", "screenLit=false"), "KMG_Nalfeshnee_Body", out reason) || reason != "unlit", "An unlit frame fails.");
            Assertions.False(Sprint7ObserverPolicy.PartyCaptureValid(good.Replace("inFrame=true", "inFrame=false"), "KMG_Nalfeshnee_Body", out reason) || reason != "out-of-frame", "An out-of-frame unit fails.");
            Assertions.False(Sprint7ObserverPolicy.PartyCaptureValid(good.Replace("screenFade=0", "screenFade=0.6"), "KMG_Nalfeshnee_Body", out reason) || reason != "screen-fade", "A frame under the native fade fails.");
            Assertions.False(Sprint7ObserverPolicy.PartyCaptureValid(good.Replace("intact=true", "intact=false"), "KMG_Nalfeshnee_Body", out reason) || reason != "dissolved", "A dissolving body fails.");
            Assertions.False(Sprint7ObserverPolicy.PartyCaptureValid(good, null, out reason), "Without an expected body nothing passes.");
        }

        private static IEnumerable<object> Body(KmgLoaderCompatHarness.OwnedProbeResources owned, Action unpause, Action subscribe, Action seed,
            Action restorePause, Action unsubscribe, Action restoreRng)
        {
            owned.Hold("pause", restorePause); unpause();
            owned.Hold("subscription", unsubscribe); subscribe();
            owned.Hold("rng", restoreRng); seed();
            try { yield return null; yield return null; }
            finally { owned.ReleaseAll(); }
        }
    }
}
