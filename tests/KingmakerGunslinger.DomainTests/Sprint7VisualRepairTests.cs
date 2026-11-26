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
                ExpandedSummoningNativeProfilePolicy.Sprint39S7UiPhase, ExpandedSummoningNativeProfilePolicy.Sprint39S7EffectsPhase,
                ExpandedSummoningNativeProfilePolicy.Sprint39S7EffectsMechanicsPhase };
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
                return Life.Release(() => new[] { new KeyValuePair<string, Action>("disposable units", () => Units++) });
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

        // R2-F2 (round 6): the Trumpet probe forces no die. Only the blast's own
        // top-level save is marked passed (its AutoPass); a save nested inside
        // another save is not the blast's. The decisive interval's verdict: a
        // foreign d20 dispatched directly inside the owned save before its
        // native roll, the owned save's own die and the following draw all keep
        // their untreated values, and the owned save passed through AutoPass.
        internal static void TrumpetOwnedSaveBoundary()
        {
            Func<object, bool> isSave = rule => rule is string && ((string)rule).StartsWith("save", StringComparison.Ordinal);
            object blastSave = "save-blast", nestedSave = "save-nested", ability = "ability-rule", d20 = "d20-foreign";
            Assertions.True(KmgLoaderCompatHarness.TrumpetSaveOwnership.TopLevelSave(new[] { ability, blastSave }, blastSave, isSave), "The blast's own save is top level.");
            Assertions.False(KmgLoaderCompatHarness.TrumpetSaveOwnership.TopLevelSave(new[] { ability, blastSave, nestedSave }, nestedSave, isSave), "A save nested inside the blast save is not the blast's.");
            Assertions.True(KmgLoaderCompatHarness.TrumpetSaveOwnership.TopLevelSave(new[] { ability, blastSave, d20 }, blastSave, isSave), "A foreign d20 open above the owned save does not change which save is owned.");
            Assertions.False(KmgLoaderCompatHarness.TrumpetSaveOwnership.TopLevelSave(new[] { ability }, null, isSave), "No save, no ownership.");
            Func<bool, int, int, bool, int, int, bool, bool, bool, string> verdict = KmgLoaderCompatHarness.TrumpetSaveOwnership.NestedForeignVerdict;
            Assertions.Equal(null, verdict(true, 7, 7, false, 13, 13, true, true, true), "Untreated foreign die, natural owned die, AutoPass pass and untouched following draw pass.");
            Assertions.Equal("not-inside-owned-save", verdict(false, 7, 7, false, 13, 13, true, true, true), "A foreign die outside the owned save is not the decisive case.");
            Assertions.Equal("foreign-d20-changed", verdict(true, 20, 7, false, 13, 13, true, true, true), "A foreign die given 20 (the round-5 claim defect) fails.");
            Assertions.Equal("foreign-d20-changed", verdict(true, 7, 7, true, 13, 13, true, true, true), "A pre-rolled foreign die fails even with a matching value.");
            Assertions.Equal("owned-save-not-passed", verdict(true, 7, 7, false, 13, 13, false, true, true), "An owned save not passed through AutoPass fails.");
            Assertions.Equal("owned-save-die-not-natural", verdict(true, 7, 7, false, 20, 13, true, true, true), "A forced owned die fails.");
            Assertions.Equal("following-draw-changed", verdict(true, 7, 7, false, 13, 13, true, true, false), "A shifted following draw fails.");
            string probe = File.ReadAllText("tools/LoaderCompatHarness/TrumpetCueProbes.cs");
            Assertions.True(probe.Contains("evt.AutoPass = true;") && !probe.Contains("PreRolled.SetValue") && !probe.Contains("IGlobalRulebookHandler<RuleRollD20>") &&
                probe.Contains("Rulebook.Trigger(foreign)"), "The probe marks only the owned save, never sets a die, and dispatches the foreign d20 directly.");
        }

        // R5-F5: the real ProbeSlot -> Release -> iterator finally -> remaining
        // resources sequence with deferred destruction. The combat cleanup in the
        // iterator's finally requests the target's destruction; destruction
        // completes later (Destroyed stays false); the final releases are built
        // afterwards and request no unit twice. A failing independent release is
        // retained and the other releases are still attempted.
        private sealed class FakeUnit
        {
            internal readonly string Id; internal int DestroyRequests;
            // Destruction is deferred past the whole test: it never completes here.
            internal bool Destroyed { get { return false; } }
            internal FakeUnit(string id) { Id = id; }
        }

        private sealed class DisposablesProbe : KmgLoaderCompatHarness.IOwnedProbe
        {
            internal readonly KmgLoaderCompatHarness.OwnedProbeResources Owned = new KmgLoaderCompatHarness.OwnedProbeResources();
            internal readonly KmgLoaderCompatHarness.YieldedProbeLifecycle Life;
            internal readonly KmgLoaderCompatHarness.OwnedDisposables<FakeUnit> Units;
            internal readonly FakeUnit Target = new FakeUnit("hostile"), Ghaele = new FakeUnit("ghaele"), Donor = new FakeUnit("donor");
            internal int WitnessSettled, Pause;
            internal DisposablesProbe()
            {
                // Native destruction completes after the call: the request is counted, Destroyed stays false.
                Units = new KmgLoaderCompatHarness.OwnedDisposables<FakeUnit>("disposable unit", u => u.Id, u => { if (!u.Destroyed) u.DestroyRequests++; if (u.Id == "donor") throw new InvalidOperationException("cannot destroy donor"); }, Owned.Outcome);
                Life = new KmgLoaderCompatHarness.YieldedProbeLifecycle(Owned, lifecycle => Body());
            }
            private IEnumerable<object> Body()
            {
                Owned.Hold("pause", () => Pause++);
                Units.Add(Ghaele); Units.Add(Donor); Units.Add(Target);
                Owned.Hold("witness move", () => WitnessSettled++);
                Owned.Hold("combat state", () => Units.Destroy(Target));
                try { while (true) yield return null; }
                finally { Owned.ReleaseAll(); }
            }
            public object Result { get { return "recorded"; } }
            public string Poll() { return Life.Poll(); }
            public KmgLoaderCompatHarness.CleanupReport Release(string testFailure) { return Life.Release(() => new[] { new KeyValuePair<string, Action>("disposable units", Units.ReleaseRemaining) }); }
        }

        internal static void ProbeSlotDestroysEachUnitOnce()
        {
            const string owner = "4|kmgGhaeleLifetime_timeline";
            var slot = new KmgLoaderCompatHarness.ProbeSlot();
            var probe = new DisposablesProbe();
            slot.Begin(owner, probe);
            Assertions.True(slot.Poll(owner) == null, "The probe is yielded with combat active.");
            KmgLoaderCompatHarness.ProbeOutcome outcome = slot.Abandon("step timeout after 360s");
            Assertions.True(probe.Target.DestroyRequests == 1 && !probe.Target.Destroyed, "The target held by both the combat cleanup and the final collection gets exactly one request, with destruction still pending.");
            Assertions.True(probe.Ghaele.DestroyRequests == 1 && probe.Donor.DestroyRequests == 1, "Every other owned unit gets exactly one request, the one after the failing release included.");
            Assertions.True(probe.WitnessSettled == 1 && probe.Pause == 1, "The witness move and the pause are released once.");
            Assertions.True(!outcome.IsolationRestored && outcome.Failure.Contains("disposable unit donor") && outcome.Failure.Contains("cannot destroy donor") &&
                outcome.TestFailure == "step timeout after 360s", "The failed release is retained under its identity with the original error.");
            Assertions.True(outcome.Cleanup.Attempted.Count(name => name == "disposable unit ghaele") == 1 && outcome.Cleanup.Attempted.Count(name => name == "disposable unit donor") == 1 &&
                outcome.Cleanup.Attempted.Count(name => name == "disposable unit hostile") == 1, "Every request is in the one cumulative outcome, each unit once; the final release requests only units no earlier release requested.");
            Assertions.True(probe.Units.Remaining.Count == 0 && probe.Units.WasRequested(probe.Target), "Nothing owned is left unrequested.");
            // Control: a final release attempted before the iterator's cleanup still requests the target once.
            var eager = new DisposablesProbe();
            Assertions.True(eager.Life.Poll() == null, "The control probe is yielded.");
            eager.Units.ReleaseRemaining();
            eager.Owned.ReleaseAll();
            Assertions.True(eager.Target.DestroyRequests == 1 && eager.Ghaele.DestroyRequests == 1, "Even a final release made before the iterator's cleanup requests the target once: the request record, not Destroyed, decides.");
        }

        // Owner amendment 3 (R6-F1): failed early releases reach ProbeSlot. A probe built as the Ghaele probe is
        // (YieldedProbeLifecycle, OwnedDisposables over the probe's cumulative outcome, a held witness settlement).
        private sealed class EarlyReleaseProbe : KmgLoaderCompatHarness.IOwnedProbe
        {
            internal readonly KmgLoaderCompatHarness.OwnedProbeResources Owned = new KmgLoaderCompatHarness.OwnedProbeResources();
            internal readonly KmgLoaderCompatHarness.YieldedProbeLifecycle Life;
            internal readonly KmgLoaderCompatHarness.OwnedDisposables<FakeUnit> Units;
            internal readonly FakeUnit First = new FakeUnit("first"), Second = new FakeUnit("second");
            internal int Pause, Interrupts;
            private readonly string mode;
            internal EarlyReleaseProbe(string mode)
            {
                this.mode = mode;
                Units = new KmgLoaderCompatHarness.OwnedDisposables<FakeUnit>("disposable unit", u => u.Id, u => { u.DestroyRequests++; if (u.Id == "first" && mode == "destroy-throws") throw new InvalidOperationException("cannot destroy first"); }, Owned.Outcome);
                Life = new KmgLoaderCompatHarness.YieldedProbeLifecycle(Owned, lifecycle => Body());
            }
            private IEnumerable<object> Body()
            {
                Owned.Hold("pause", () => Pause++);
                Units.Add(First); Units.Add(Second);
                try
                {
                    yield return null;
                    if (mode == "destroy-throws")
                    {
                        Life.Problems.Add("original test error: the lifecycle case did not hold");
                        Units.Destroy(First); // the direct, early destruction request (as in Lifecycle) throws
                    }
                    else
                    {
                        // The witness move, settled right after its readback; interruption leaves it held and unfinished
                        // unless the mode lets it finish.
                        bool finished = false;
                        Owned.Hold("witness move", () => KmgLoaderCompatHarness.WitnessSettlement.Settle(() => { Interrupts++; finished = mode == "witness-settles"; }, () => finished, () => !finished));
                        var failures = new List<string>();
                        Owned.ReleaseNow("witness move", failures);
                        if (mode == "witness-held") Life.Problems.Add("original test error: the combat verdict did not hold");
                    }
                    while (true) yield return null;
                }
                finally { Owned.ReleaseAll(); }
            }
            public object Result { get { return "recorded"; } }
            public string Poll() { return Life.Poll(); }
            public KmgLoaderCompatHarness.CleanupReport Release(string testFailure) { return Life.Release(() => new[] { new KeyValuePair<string, Action>("disposable units", Units.ReleaseRemaining) }); }
        }

        internal static void GhaeleEarlyReleaseFailuresReachSlot()
        {
            const string owner = "3|kmgGhaeleLifetime_lifecycle";
            Func<KmgLoaderCompatHarness.ProbeSlot, bool> refusesNext = slot =>
            {
                try { slot.Begin("4|kmgGhaeleLifetime_visibility", new EarlyReleaseProbe("witness-settles")); return false; }
                catch (InvalidOperationException error) { return error.Message.Contains("isolation was not restored"); }
            };
            // An early direct destruction throws.
            var slot1 = new KmgLoaderCompatHarness.ProbeSlot();
            var throwing = new EarlyReleaseProbe("destroy-throws");
            slot1.Begin(owner, throwing);
            Assertions.True(slot1.Poll(owner) == null, "Yielded before the early release.");
            KmgLoaderCompatHarness.ProbeOutcome thrown = slot1.Poll(owner);
            Assertions.True(thrown != null && thrown.TestFailure.Contains("original test error: the lifecycle case did not hold") && thrown.TestFailure.Contains("cannot destroy first"),
                "The original test error is kept beside the exception that ended the probe.");
            Assertions.True(!thrown.IsolationRestored && thrown.Cleanup.Failures.Any(value => value == "disposable unit first: InvalidOperationException: cannot destroy first"),
                "The failed direct request is a named failure in the cumulative outcome ProbeSlot reads.");
            Assertions.True(throwing.First.DestroyRequests == 1 && throwing.Second.DestroyRequests == 1 && throwing.Pause == 1,
                "The failed unit received exactly one request (it stays marked requested); the other unit and the pause were still released.");
            Assertions.True(!slot1.IsolationRestored && refusesNext(slot1) &&
                KmgLoaderCompatHarness.ProbeSlot.Decide(true, true, slot1.IsolationRestored) == KmgLoaderCompatHarness.StepContinuation.StopLaunch,
                "Isolation fails and the next probe is refused, even for a step that continues on failure.");

            // A witness command remains held and unfinished after its settlement.
            var slot2 = new KmgLoaderCompatHarness.ProbeSlot();
            var held = new EarlyReleaseProbe("witness-held");
            slot2.Begin(owner, held);
            Assertions.True(slot2.Poll(owner) == null && slot2.Poll(owner) == null, "Yielded after the witness settlement.");
            KmgLoaderCompatHarness.ProbeOutcome abandoned = slot2.Abandon("step timeout after 240s");
            Assertions.True(!abandoned.IsolationRestored && abandoned.Cleanup.Failures.Any(value => value == "witness move: InvalidOperationException: " + KmgLoaderCompatHarness.WitnessSettlement.StillHeld),
                "The unsettled witness fails cleanup with its named error.");
            Assertions.Equal("step timeout after 240s", abandoned.TestFailure, "The slot's original error is kept.");
            Assertions.True(held.Interrupts == 1 && held.Pause == 1 && held.First.DestroyRequests == 1 && held.Second.DestroyRequests == 1, "The witness is interrupted once and every independent release still runs.");
            Assertions.True(!slot2.IsolationRestored && refusesNext(slot2), "Isolation fails and the next probe is refused.");

            // Control: a witness that settles leaves isolation restored.
            var slot3 = new KmgLoaderCompatHarness.ProbeSlot();
            var settles = new EarlyReleaseProbe("witness-settles");
            slot3.Begin(owner, settles);
            slot3.Poll(owner); slot3.Poll(owner);
            KmgLoaderCompatHarness.ProbeOutcome clean = slot3.Abandon("step timeout after 240s");
            Assertions.True(clean.IsolationRestored && slot3.IsolationRestored && settles.Interrupts == 1, "A settled witness and clean releases restore isolation.");
        }

        // Recovery C2 (R5-F1) and C2 amendment 1: late Ghaele units through the real ProbeSlot -> Poll -> Release / Abandon
        // path, using the production caller and finalizer GhaeleLifetimeProbe uses: WitnessCaller (subscription and
        // reconciliation released independently; ReconcileLatest), LateResultObservation (unknown by reference),
        // LateResultReconciliation and the shared LateUnitFinalRelease sequence (no fixture-built final action array).
        // Only native boundaries are replaced: destroy, the terminal read, the unsubscribe and delivery (active while the
        // witness is subscribed, as EventBus delivery is).
        private sealed class LateUnit
        {
            internal readonly string Id; internal int DestroyRequests, TerminalReads; internal bool Destroyed, Registered = true, DestroyCompletes = true, DestroyThrows;
            // Read-dependent terminal state: when set, the first read returns it and later reads return true (gone).
            internal bool? FirstReadOnly;
            internal LateUnit(string id) { Id = id; }
            // The engine's terminal read as GhaeleLifetimeProbe.Terminal makes it: destroyed, present, or unresolved.
            internal bool? Terminal()
            {
                TerminalReads++;
                if (FirstReadOnly.HasValue) return TerminalReads == 1 ? FirstReadOnly : true;
                return Destroyed ? true : Registered ? false : (bool?)null;
            }
        }

        private sealed class CreationQueue
        {
            internal readonly List<LateUnit> Queued = new List<LateUnit>();
            internal int Ticks;
            internal void Tick() { Ticks++; foreach (LateUnit unit in Queued) unit.Registered = true; Queued.Clear(); }
        }

        private sealed class LateUnitProbe : KmgLoaderCompatHarness.IOwnedProbe
        {
            internal readonly KmgLoaderCompatHarness.OwnedProbeResources Owned = new KmgLoaderCompatHarness.OwnedProbeResources();
            internal readonly KmgLoaderCompatHarness.YieldedProbeLifecycle Life;
            internal readonly KmgLoaderCompatHarness.OwnedDisposables<LateUnit> Units;
            internal readonly KmgLoaderCompatHarness.LateResultObservation<LateUnit> Observed = new KmgLoaderCompatHarness.LateResultObservation<LateUnit>();
            internal readonly List<KmgLoaderCompatHarness.WitnessCaller<LateUnit>> Callers = new List<KmgLoaderCompatHarness.WitnessCaller<LateUnit>>();
            internal readonly LateUnit Late = new LateUnit("late"), Extra = new LateUnit("extra"), Foreign = new LateUnit("foreign"), Stranger = new LateUnit("stranger"),
                Shifty = new LateUnit("shifty"), Later = new LateUnit("later"), LaterStranger = new LateUnit("later-stranger");
            internal readonly KmgLoaderCompatHarness.LateResultAttribution<LateUnit> Attribution = new KmgLoaderCompatHarness.LateResultAttribution<LateUnit>();
            // C2 amendment 2 (R3-F1): one owned cast held through the production OwnedCastRelease, and the engine's creation
            // queue as a controlled boundary: a tick registers every queued entry (here a foreign queued unit, and the cast's
            // own summon when it is still pending creation). Cleanup must never tick it.
            internal readonly CreationQueue Creation = new CreationQueue();
            internal readonly LateUnit ForeignQueued = new LateUnit("foreign-queued") { Registered = false };
            internal LateUnit CastSummoned, CastRegistered;
            internal int CastRequests;
            internal bool HoldCast, TickBeforeSummonRead;
            // The cast boundary the shared held release reads: the retained summon reference. The control mode models the
            // removed creator tick at exactly this point (before the read), so an advance there registers the queued foreign unit.
            internal LateUnit ReadCastSummon() { if (TickBeforeSummonRead) Creation.Tick(); return CastSummoned; }
            internal readonly JObject Row = new JObject();
            internal int Pause, Unsubscribes;
            internal bool Subscribed, FailUnsubscribe, ExtraArrives, StrangerArrives, StrangerRegisteredForTeardown, ShiftyArrives, DeliverAfterRelease, AbandonBeforeSettle,
                ForeignAlsoUnattributed = true;
            internal LateUnitProbe()
            {
                Units = new KmgLoaderCompatHarness.OwnedDisposables<LateUnit>("disposable unit", u => u.Id,
                    u => { u.DestroyRequests++; if (u.DestroyThrows) throw new InvalidOperationException("injected destroy failure"); if (u.DestroyCompletes) u.Destroyed = true; },
                    Owned.Outcome, u => u.Terminal());
                Life = new KmgLoaderCompatHarness.YieldedProbeLifecycle(Owned, lifecycle => Body());
            }
            // Native delivery to the witness: only while it is subscribed.
            private void Correlated(LateUnit unit) { if (Subscribed) Observed.Owned.Add(unit); }
            private void Unattributed(LateUnit unit) { if (Subscribed) Observed.Unattributed.Add(unit); }
            private IEnumerable<object> Body()
            {
                Owned.Hold("pause", () => Pause++);
                Units.Add(Late);                                   // registered with its owner before delivery
                Observed.Known.Add(Foreign);                        // the foreign same-caster control the caller knows
                if (HoldCast)
                {
                    Creation.Queued.Add(ForeignQueued);
                    KmgLoaderCompatHarness.OwnedCastRelease.Hold(Owned, "cast: unregistered summons", Units, ReadCastSummon, () => CastRegistered, u => u.Destroyed, () => CastRequests++);
                }
                Subscribed = true;
                Callers.Add(new KmgLoaderCompatHarness.WitnessCaller<LateUnit>(Owned, Units, Observed, Attribution, "late-result witness",
                    () => { Unsubscribes++; if (FailUnsubscribe) throw new InvalidOperationException("injected unsubscribe failure"); Subscribed = false; }));
                try
                {
                    yield return null;
                    Correlated(Late);                               // the delivered correlated result
                    if (ForeignAlsoUnattributed) Unattributed(Foreign); // seen before it was known, as in the spawn case
                    if (ExtraArrives) Correlated(Extra);           // a further correlated unit after the cast owner's disposal
                    if (StrangerArrives) { Unattributed(Stranger); if (StrangerRegisteredForTeardown) Units.Add(Stranger); }
                    if (ShiftyArrives) { Unattributed(Shifty); Correlated(Shifty); } // the same reference, first unattributed, then matched
                    yield return null;
                    if (AbandonBeforeSettle) while (true) yield return null;
                    Callers[0].ReleaseNow(null);                     // the early release, as GhaeleLifetimeProbe.ReleaseWitness
                    Units.Destroy(Late);                             // the caller's own request: not repeated after reconciliation
                    KmgLoaderCompatHarness.LateResultReconciliation.Settle(Units, Row, "unsettledAfterWitness", u => u.Id);
                    Row["foreignAlive"] = !Foreign.Destroyed; Row["foreignRequested"] = Units.WasRequested(Foreign);
                    Units.ReleaseRemaining();                       // the end of the run: remaining owned units, then settlement
                    KmgLoaderCompatHarness.LateResultReconciliation.Settle(Units, Row, "unsettledAtEnd", u => u.Id);
                    yield return null;
                    if (DeliverAfterRelease) { Correlated(Later); Unattributed(LaterStranger); } // still delivered if the unsubscribe failed
                    yield return null;
                }
                finally { Owned.ReleaseAll(); }
            }
            public object Result { get { return Row; } }
            public string Poll() { return Life.Poll(); }
            // The production final sequence, as GhaeleLifetimeProbe.Release calls it.
            public KmgLoaderCompatHarness.CleanupReport Release(string testFailure)
            {
                return KmgLoaderCompatHarness.LateUnitFinalRelease.Release(Life, Units, Callers, Row, "unsettledAtFinalRelease", u => u.Id, null);
            }
        }

        internal static void GhaeleLateUnitsReachSlot()
        {
            const string owner = "3|kmgGhaeleLifetime_lifecycle";
            Func<KmgLoaderCompatHarness.ProbeSlot, bool> refusesNext = slot =>
            {
                try { slot.Begin("4|kmgGhaeleLifetime_lifecycle", new LateUnitProbe()); return false; }
                catch (InvalidOperationException error) { return error.Message.Contains("isolation was not restored"); }
            };
            Func<LateUnitProbe, KmgLoaderCompatHarness.ProbeSlot, KmgLoaderCompatHarness.ProbeOutcome> runToEnd = (probe, slot) =>
            {
                slot.Begin(owner, probe);
                KmgLoaderCompatHarness.ProbeOutcome outcome = null;
                for (int poll = 0; poll < 10 && outcome == null; poll++) outcome = slot.Poll(owner);
                return outcome;
            };
            Func<LateUnitProbe, bool> foreignUntouched = probe => !probe.Foreign.Destroyed && probe.Foreign.DestroyRequests == 0;
            // R3-F3: a settlement field must be recorded as an array; a missing or misshapen field is not clean evidence.
            Func<JObject, string, string[]> rowList = (row, field) =>
            {
                var recorded = row[field] as JArray;
                if (recorded == null) throw new InvalidOperationException("settlement field " + field + " is missing or not an array");
                return recorded.Values<string>().ToArray();
            };
            Func<Action, bool> throws = action => { try { action(); return false; } catch (InvalidOperationException) { return true; } };
            Assertions.True(rowList(new JObject { ["unsettled"] = new JArray() }, "unsettled").Length == 0 && throws(() => rowList(new JObject(), "unsettled")) &&
                throws(() => rowList(new JObject { ["unsettled"] = "none" }, "unsettled")), "R3-F3: an explicit empty array is clean; a missing or non-array settlement field is refused.");

            // Control: the late unit is destroyed after its one request; the known foreign control (also seen unattributed before
            // it was known) is untouched; isolation restored; the final release found nothing new and nothing unsettled.
            var clean = new LateUnitProbe();
            KmgLoaderCompatHarness.ProbeOutcome cleanOutcome = runToEnd(clean, new KmgLoaderCompatHarness.ProbeSlot());
            Assertions.True(cleanOutcome.IsolationRestored && clean.Late.DestroyRequests == 1 && clean.Late.Destroyed && foreignUntouched(clean) && (bool)clean.Row["foreignAlive"] &&
                !(bool)clean.Row["foreignRequested"] && rowList(clean.Row, "unsettledAfterWitness").Length == 0 && rowList(clean.Row, "unsettledAtFinalRelease").Length == 0 &&
                cleanOutcome.Cleanup.Attempted.Contains("late-result witness final reconciliation"), "A settled late unit and an untouched known foreign control leave isolation restored through the shared final release.");

            // R1-F1: destruction requested but the unit's terminal state cannot be concluded (registration pending, not destroyed).
            var pending = new LateUnitProbe(); pending.ExtraArrives = true; pending.Extra.Registered = false; pending.Extra.DestroyCompletes = false;
            var slot1 = new KmgLoaderCompatHarness.ProbeSlot();
            KmgLoaderCompatHarness.ProbeOutcome pendingOutcome = runToEnd(pending, slot1);
            Assertions.True(!pendingOutcome.IsolationRestored && pendingOutcome.Cleanup.Failures.Count(value => value == "disposable unit extra: destruction requested, terminal state unresolved") == 1 &&
                pending.Extra.DestroyRequests == 1 && refusesNext(slot1), "R1-F1: a correlated unit with destruction pending and registration pending is unresolved by name; one request; next probe refused.");

            // Destruction returns but the unit stays present: named, one request, next probe refused.
            var stays = new LateUnitProbe(); stays.Late.DestroyCompletes = false;
            var slot2 = new KmgLoaderCompatHarness.ProbeSlot();
            KmgLoaderCompatHarness.ProbeOutcome staysOutcome = runToEnd(stays, slot2);
            Assertions.True(!staysOutcome.IsolationRestored && staysOutcome.Cleanup.Failures.Count(value => value == "disposable unit late: destruction requested, unit still present") == 1 &&
                stays.Late.DestroyRequests == 1 && stays.Pause == 1 && foreignUntouched(stays) && refusesNext(slot2), "A returned request does not establish disappearance; independent cleanup continues; foreign control untouched.");

            // R2-F2 (amendment case B): the same reference first unattributed, then positively correlated, before release.
            var shifty = new LateUnitProbe(); shifty.ShiftyArrives = true;
            KmgLoaderCompatHarness.ProbeOutcome shiftyOutcome = runToEnd(shifty, new KmgLoaderCompatHarness.ProbeSlot());
            Assertions.True(shifty.Observed.Unattributed.Contains(shifty.Shifty) && shifty.Observed.Owned.Contains(shifty.Shifty) && shifty.Observed.Unknown.Count == 0 &&
                shifty.Shifty.DestroyRequests == 1 && shifty.Shifty.Destroyed && shifty.Units.Owns(shifty.Shifty) && foreignUntouched(shifty) &&
                !shiftyOutcome.Cleanup.Failures.Any(value => value.Contains("ownership unknown")) && shiftyOutcome.IsolationRestored,
                "R2-F2: positive attribution resolves the same reference's earlier uncertainty: one request, settled, no residual unknown, clean admission.");

            // R1-F2 retained: an identity still unknown at release stays a named failure even when registered for fixture teardown.
            var stranger = new LateUnitProbe(); stranger.StrangerArrives = true; stranger.StrangerRegisteredForTeardown = true;
            var slot3 = new KmgLoaderCompatHarness.ProbeSlot();
            KmgLoaderCompatHarness.ProbeOutcome strangerOutcome = runToEnd(stranger, slot3);
            Assertions.True(!strangerOutcome.IsolationRestored && strangerOutcome.Cleanup.Failures.Count(value => value == "witness late-result witness: unit stranger: ownership unknown, left in place") == 1 &&
                stranger.Stranger.DestroyRequests == 1 && foreignUntouched(stranger) && refusesNext(slot3),
                "A registered-but-unattributed unit is not attributed by its teardown registration: named once, one (teardown) request, foreign untouched, next probe refused.");
            var unregistered = new LateUnitProbe(); unregistered.StrangerArrives = true;
            KmgLoaderCompatHarness.ProbeOutcome unregisteredOutcome = runToEnd(unregistered, new KmgLoaderCompatHarness.ProbeSlot());
            Assertions.True(!unregisteredOutcome.IsolationRestored && unregistered.Stranger.DestroyRequests == 0, "Unknown cast ownership never authorizes destruction, and still prevents clean isolation.");

            // R2-F1 (amendment case A): the early unsubscribe throws before detaching, after the early reconciliation ran; a further
            // correlated unit and an unknown unit are delivered later, after the run's own end settlement, before slot termination.
            var failing = new LateUnitProbe(); failing.FailUnsubscribe = true; failing.DeliverAfterRelease = true;
            var slot4 = new KmgLoaderCompatHarness.ProbeSlot();
            KmgLoaderCompatHarness.ProbeOutcome failingOutcome = runToEnd(failing, slot4);
            Assertions.True(failing.Subscribed && failing.Unsubscribes == 1 && failing.Observed.Owned.Contains(failing.Later) && failing.Observed.Unattributed.Contains(failing.LaterStranger),
                "Case A setup: the witness stayed subscribed and received both later results.");
            Assertions.True(!failingOutcome.IsolationRestored && failingOutcome.Cleanup.Failures.Contains("late-result witness subscription: InvalidOperationException: injected unsubscribe failure") &&
                failing.Later.DestroyRequests == 1 && failing.Later.Destroyed && failing.Units.Owns(failing.Later) && !rowList(failing.Row, "unsettledAtFinalRelease").Any() &&
                failingOutcome.Cleanup.Failures.Count(value => value == "witness late-result witness: unit later-stranger: ownership unknown, left in place") == 1 &&
                failing.LaterStranger.DestroyRequests == 0 && failing.Late.DestroyRequests == 1 && foreignUntouched(failing) && failing.Pause == 1 && refusesNext(slot4),
                "R2-F1: the final release reconciles the later arrivals: the correlated unit adopted, requested once and destroyed; the unknown named and left alone; the unsubscribe error kept; foreign untouched; next probe refused.");
            // The same, with the later unit's destruction still pending at the final snapshot: a named residual in row and outcome.
            var failingPending = new LateUnitProbe(); failingPending.FailUnsubscribe = true; failingPending.DeliverAfterRelease = true; failingPending.Later.DestroyCompletes = false;
            var slot5 = new KmgLoaderCompatHarness.ProbeSlot();
            KmgLoaderCompatHarness.ProbeOutcome failingPendingOutcome = runToEnd(failingPending, slot5);
            Assertions.True(!failingPendingOutcome.IsolationRestored && failingPending.Later.DestroyRequests == 1 &&
                failingPendingOutcome.Cleanup.Failures.Count(value => value == "disposable unit later: destruction requested, unit still present") == 1 &&
                rowList(failingPending.Row, "unsettledAtFinalRelease").SequenceEqual(new[] { "later: destruction requested, unit still present" }) && refusesNext(slot5),
                "R2-F1: a later correlated unit still present at the final snapshot is one request and a named residual in the final row and the slot outcome.");

            // R1-F7 (amendment case C): abandonment through the real ProbeSlot before ordinary settlement. The extra unit's terminal
            // reader changes after its first read (present, then gone), so the final row and the outcome agree only when both
            // consume one snapshot; the late unit's destruction request throws, and independent cleanup continues.
            var abandoned = new LateUnitProbe(); abandoned.ExtraArrives = true; abandoned.AbandonBeforeSettle = true; abandoned.Extra.FirstReadOnly = false; abandoned.Late.DestroyThrows = true;
            var unrelatedPending = new LateUnit("unrelated-pending") { DestroyCompletes = false };
            var slot6 = new KmgLoaderCompatHarness.ProbeSlot();
            slot6.Begin(owner, abandoned);
            Assertions.True(slot6.Poll(owner) == null && slot6.Poll(owner) == null && slot6.Poll(owner) == null, "Yielded after the late results arrived.");
            KmgLoaderCompatHarness.ProbeOutcome abandonedOutcome = slot6.Abandon("step timeout after 240s");
            const string residual = "extra: destruction requested, unit still present";
            Assertions.True(abandonedOutcome.TestFailure == "step timeout after 240s" && !abandonedOutcome.IsolationRestored && ReferenceEquals(abandonedOutcome.Result, abandoned.Row) &&
                rowList(abandoned.Row, "unsettledAtFinalRelease").Contains(residual) && abandonedOutcome.Cleanup.Failures.Count(value => value == "disposable unit " + residual) == 1 &&
                abandoned.Extra.TerminalReads == 1, "Case C: the final row and the slot outcome carry the same unresolved unit from one read; a second read would have found it gone.");
            Assertions.True(abandonedOutcome.Cleanup.Failures.Contains("disposable unit late: InvalidOperationException: injected destroy failure") &&
                abandoned.Late.DestroyRequests == 1 && abandoned.Extra.DestroyRequests == 1 && abandoned.Unsubscribes == 1 && abandoned.Pause == 1 && foreignUntouched(abandoned) &&
                abandonedOutcome.Cleanup.Attempted.Contains("late-result witness final reconciliation") && abandonedOutcome.Cleanup.Attempted.Contains("disposable units settled") &&
                unrelatedPending.DestroyRequests == 0 && !unrelatedPending.Destroyed && refusesNext(slot6),
                "Case C: the original timeout kept; one request each despite a failing request; the held and final releases all ran; unrelated pending work untouched; next probe refused.");

            // R3-F1 (C2 amendment 2): the production held cast release reached through ProbeSlot.Abandon, with a foreign unit queued
            // for creation behind a controlled queue that would register it if ticked.
            Func<Action<LateUnitProbe>, KeyValuePair<LateUnitProbe, KmgLoaderCompatHarness.ProbeOutcome>> abandonCast = arrange =>
            {
                var probe = new LateUnitProbe(); probe.HoldCast = true; probe.AbandonBeforeSettle = true; arrange(probe);
                var slot = new KmgLoaderCompatHarness.ProbeSlot();
                slot.Begin(owner, probe);
                slot.Poll(owner); slot.Poll(owner); slot.Poll(owner);
                KmgLoaderCompatHarness.ProbeOutcome outcome = slot.Abandon("step timeout after 240s");
                probe.Row["nextRefused"] = refusesNext(slot);
                return new KeyValuePair<LateUnitProbe, KmgLoaderCompatHarness.ProbeOutcome>(probe, outcome);
            };
            Func<LateUnitProbe, bool> queueUntouched = probe => probe.Creation.Ticks == 0 && !probe.ForeignQueued.Registered && probe.ForeignQueued.DestroyRequests == 0 &&
                probe.Creation.Queued.Contains(probe.ForeignQueued);
            // No owned summon: cleanup requests nothing and does not run the creation queue to look for one.
            // Detector control: the former tick modelled at the retained-summon read through the same ProbeSlot.Abandon path registers
            // the queued foreign unit, so a tick reintroduced at that boundary is visible to the assertions below.
            KeyValuePair<LateUnitProbe, KmgLoaderCompatHarness.ProbeOutcome> ticking = abandonCast(probe => probe.TickBeforeSummonRead = true);
            Assertions.True(ticking.Key.Creation.Ticks == 1 && ticking.Key.ForeignQueued.Registered && !ticking.Key.Creation.Queued.Contains(ticking.Key.ForeignQueued) &&
                !queueUntouched(ticking.Key), "R3-F1 detector: an advance at the cast-release boundary registers the queued foreign unit and fails the untouched check.");
            KeyValuePair<LateUnitProbe, KmgLoaderCompatHarness.ProbeOutcome> none = abandonCast(probe => { });
            Assertions.True(queueUntouched(none.Key) && none.Key.CastRequests == 0 && none.Value.TestFailure == "step timeout after 240s" && none.Value.IsolationRestored &&
                none.Value.Cleanup.Attempted.Contains("cast: unregistered summons"), "R3-F1: with no owned summon the held cast release requests nothing and leaves the foreign creation queued.");
            // Conclusive destruction: the exact registered-in-state summon gets one request and is destroyed; the queue is untouched.
            KeyValuePair<LateUnitProbe, KmgLoaderCompatHarness.ProbeOutcome> conclusive = abandonCast(probe => probe.CastSummoned = new LateUnit("cast-summon"));
            Assertions.True(queueUntouched(conclusive.Key) && conclusive.Key.CastRequests == 1 && conclusive.Key.CastSummoned.DestroyRequests == 1 && conclusive.Key.CastSummoned.Destroyed &&
                conclusive.Value.IsolationRestored && conclusive.Value.TestFailure == "step timeout after 240s" && !rowList(conclusive.Key.Row, "unsettledAtFinalRelease").Any(),
                "R3-F1: the exact owned summon is requested once and observed destroyed; the foreign creation stays queued and unrequested.");
            // Pending creation: the owned summon itself still waits in the queue; it is requested once and stays a named residual.
            KeyValuePair<LateUnitProbe, KmgLoaderCompatHarness.ProbeOutcome> queued = abandonCast(probe =>
            {
                probe.CastSummoned = new LateUnit("cast-summon") { Registered = false, DestroyCompletes = false };
                probe.Creation.Queued.Add(probe.CastSummoned);
            });
            Assertions.True(queueUntouched(queued.Key) && !queued.Key.CastSummoned.Registered && queued.Key.CastSummoned.DestroyRequests == 1 && queued.Key.CastRequests == 1 &&
                rowList(queued.Key.Row, "unsettledAtFinalRelease").SequenceEqual(new[] { "cast-summon: destruction requested, terminal state unresolved" }) &&
                queued.Value.Cleanup.Failures.Count(value => value == "disposable unit cast-summon: destruction requested, terminal state unresolved") == 1 &&
                queued.Value.TestFailure == "step timeout after 240s" && queued.Key.Pause == 1 && queued.Key.Unsubscribes == 1 && !queued.Value.IsolationRestored && (bool)queued.Key.Row["nextRefused"],
                "R3-F1: a summon still pending creation is requested once and named unresolved in row and outcome; neither queued entry is registered; independent releases ran; next probe refused.");
            // The registered-reference exclusion: a summon the probe registered is released with the owned units, not by the cast.
            KeyValuePair<LateUnitProbe, KmgLoaderCompatHarness.ProbeOutcome> registered = abandonCast(probe =>
            {
                probe.CastSummoned = probe.CastRegistered = new LateUnit("cast-summon");
                probe.Units.Add(probe.CastSummoned);
            });
            Assertions.True(registered.Key.CastRequests == 0 && registered.Key.CastSummoned.DestroyRequests == 1 && registered.Key.CastSummoned.Destroyed && queueUntouched(registered.Key),
                "R3-F1: a registered summon is not requested by the cast release; its one request is the owned-unit release.");

            // R3-F2 (C2 amendment 2): two witnesses share the probe's attribution view.
            foreach (bool bFirst in new[] { false, true })
                foreach (bool abandon in new[] { false, true })
                    foreach (bool foreignTornDown in new[] { false, true })
                    {
                        var shared = new SharedWitnessProbe { BFirst = bFirst, Abandon = abandon, ForeignRegisteredForTeardown = foreignTornDown };
                        var slot = new KmgLoaderCompatHarness.ProbeSlot();
                        slot.Begin(owner, shared);
                        KmgLoaderCompatHarness.ProbeOutcome outcome = null;
                        for (int poll = 0; poll < 10 && outcome == null; poll++) outcome = slot.Poll(owner);
                        if (abandon) { Assertions.True(outcome == null, "The shared-witness probe was still running."); outcome = slot.Abandon("step timeout after 240s"); }
                        string label = " (" + (bFirst ? "B created first" : "A created first") + ", " + (abandon ? "held unwind on abandonment" : "early release, then final") +
                            (foreignTornDown ? ", foreign also registered for fixture teardown" : "") + ")";
                        Assertions.True(shared.Summon.DestroyRequests == 1 && shared.Summon.Destroyed && shared.ForeignRequestsDuringReconciliation == 0 &&
                            shared.Foreign.DestroyRequests == (foreignTornDown ? 1 : 0) && shared.Unknown.DestroyRequests == 0 &&
                            (foreignTornDown
                                ? shared.Requests.SequenceEqual(new[] { "summon", "teardown-sentinel", "foreign" })
                                : shared.Requests.SequenceEqual(new[] { "summon" })),
                            "R3-F2 / R4-F1: one request for the summon B matched; the foreign control B identifies is requested only by fixture teardown, after the teardown-sentinel registered ahead of it (so no early, held or final witness reconciliation requested it)" + label);
                        Assertions.True(!outcome.Cleanup.Failures.Any(value => value.Contains("unit summon: ownership unknown") || value.Contains("unit foreign: ownership unknown")) &&
                            outcome.Cleanup.Failures.Count(value => value == "witness witness A: unit unknown: ownership unknown, left in place") == 1 &&
                            outcome.Cleanup.Failures.Count(value => value == "witness A subscription: InvalidOperationException: injected unsubscribe failure") == 1 &&
                            !outcome.IsolationRestored && refusesNext(slot) && (abandon ? outcome.TestFailure == "step timeout after 240s" : true) &&
                            rowList(shared.Row, "unsettledAtFinalRelease").Length == 0,
                            "R3-F2: no false unknown for either identified reference; the genuine unknown named once; the unsubscribe error kept; next probe refused" + label);
                    }
        }

        // R3-F2: two production witness callers over one probe-local attribution view, through ProbeSlot and the shared
        // finalizer. Witness A's unsubscribe fails (early, or on held unwind); later a summon arrives unattributed to A and
        // positively matched by B, a foreign control seen by A is explicitly identified by B, and a separate reference stays
        // genuinely unknown to both.
        private sealed class SharedWitnessProbe : KmgLoaderCompatHarness.IOwnedProbe
        {
            internal readonly KmgLoaderCompatHarness.OwnedProbeResources Owned = new KmgLoaderCompatHarness.OwnedProbeResources();
            internal readonly KmgLoaderCompatHarness.YieldedProbeLifecycle Life;
            internal readonly KmgLoaderCompatHarness.OwnedDisposables<LateUnit> Units;
            internal readonly KmgLoaderCompatHarness.LateResultAttribution<LateUnit> Attribution = new KmgLoaderCompatHarness.LateResultAttribution<LateUnit>();
            internal readonly KmgLoaderCompatHarness.LateResultObservation<LateUnit> A = new KmgLoaderCompatHarness.LateResultObservation<LateUnit>(),
                B = new KmgLoaderCompatHarness.LateResultObservation<LateUnit>();
            internal readonly List<KmgLoaderCompatHarness.WitnessCaller<LateUnit>> Callers = new List<KmgLoaderCompatHarness.WitnessCaller<LateUnit>>();
            internal readonly LateUnit Summon = new LateUnit("summon"), Foreign = new LateUnit("foreign"), Unknown = new LateUnit("unknown"), Sentinel = new LateUnit("teardown-sentinel");
            // Every destruction request in order (R4-F1): the sentinel is registered for fixture teardown ahead of the foreign
            // control, so a final witness request for the foreign control (made before the teardown step) would precede it.
            internal readonly List<string> Requests = new List<string>();
            internal readonly JObject Row = new JObject();
            internal bool BFirst, Abandon, ForeignRegisteredForTeardown, ASubscribed, BSubscribed, Reconciling;
            internal int ForeignRequestsDuringReconciliation;
            internal SharedWitnessProbe()
            {
                Units = new KmgLoaderCompatHarness.OwnedDisposables<LateUnit>("disposable unit", u => u.Id,
                    u => { u.DestroyRequests++; Requests.Add(u.Id); if (ReferenceEquals(u, Foreign) && Reconciling) ForeignRequestsDuringReconciliation++; if (u.DestroyCompletes) u.Destroyed = true; },
                    Owned.Outcome, u => u.Terminal());
                Life = new KmgLoaderCompatHarness.YieldedProbeLifecycle(Owned, lifecycle => Body());
            }
            private void Watch(string name)
            {
                bool isA = name == "witness A";
                // The reconciliation boundary is marked so a request for the foreign control made by reconciliation is seen.
                Callers.Add(new KmgLoaderCompatHarness.WitnessCaller<LateUnit>(Owned, Units, isA ? A : B, Attribution, name,
                    () => { if (isA) throw new InvalidOperationException("injected unsubscribe failure"); BSubscribed = false; }));
                if (isA) ASubscribed = true; else { BSubscribed = true; B.Known.Add(Foreign); }
            }
            private IEnumerable<object> Body()
            {
                if (ForeignRegisteredForTeardown) { Units.Add(Sentinel); Units.Add(Foreign); }   // independently owned by the fixture: teardown may remove it
                if (BFirst) { Watch("witness B"); Watch("witness A"); } else { Watch("witness A"); Watch("witness B"); }
                try
                {
                    yield return null;
                    if (ASubscribed) A.Unattributed.Add(Foreign);       // seen by A, which does not know it
                    if (!Abandon) { Reconciling = true; Callers.First(c => c.Name == "witness A").ReleaseNow(null); Reconciling = false; } // early release; the unsubscribe fails
                    yield return null;
                    // Later results: A is still subscribed (its unsubscribe failed or has not run yet).
                    if (ASubscribed) { A.Unattributed.Add(Summon); A.Unattributed.Add(Unknown); }
                    if (BSubscribed) B.Owned.Add(Summon);
                    yield return null;
                    if (Abandon) while (true) yield return null;
                }
                finally { Reconciling = true; Owned.ReleaseAll(); Reconciling = false; }   // held unwind: subscriptions and reconciliations
            }
            public object Result { get { return Row; } }
            public string Poll() { return Life.Poll(); }
            public KmgLoaderCompatHarness.CleanupReport Release(string testFailure)
            {
                // The production final sequence. The reconciliation mark covers the early and held boundaries; on the final
                // boundary the variants without fixture teardown show the foreign control is never requested at all.
                return KmgLoaderCompatHarness.LateUnitFinalRelease.Release(Life, Units, Callers, Row, "unsettledAtFinalRelease", u => u.Id, null);
            }
        }

        // Owner amendment 3 (R4-F1, R6-F2): the effects phase's cell sequence over one cumulative isolation, driven
        // the way the phase drives it: cells publish their cleanup from their own finally; disposal of the sequence
        // (the enclosing runner) unwinds the running cell; the admission guard refuses the next cell; a later repair
        // never erases a named failure; residual executions are re-read until they settle.
        private sealed class FakeExecution
        {
            internal bool? Ended = false; internal bool Throws; internal int ThrowAfter = -1; private int reads;
            internal bool? Read() { if (Throws || (ThrowAfter >= 0 && reads++ >= ThrowAfter)) throw new InvalidOperationException("unreadable"); return Ended; }
        }

        private static IEnumerable<int> FakeCell(Sprint7CellIsolation isolation, string owner, Func<List<string>> releases, Action<Sprint7CellIsolation> residuals, Action started, int yields, bool throwBeforeReport = false)
        {
            isolation.Opened(owner);
            started();
            try { for (int index = 0; index < yields; index++) yield return index; }
            finally
            {
                List<string> failures = releases();
                if (throwBeforeReport) throw new InvalidOperationException("cleanup threw before reporting");
                isolation.Fail(owner, failures);
                residuals(isolation);
                isolation.Reported(owner);
            }
        }

        internal static void EffectsCellSequenceIsolation()
        {
            Func<string, Func<IEnumerable<int>>, IEnumerable<int>> run = (name, body) => body();
            var limit = TimeSpan.Zero;

            // R4-F1: the Dust cell's early-exit cast fails its owned condition removal while the enclosing runner is
            // disposed; its execution is still live; the other releases run; the next cell never starts.
            var isolation = new Sprint7CellIsolation();
            var execution = new FakeExecution();
            int independent = 0, nextStarts = 0;
            const string named = "injected failure of an owned Sickened removal";
            Func<List<string>> dustReleases = () => Sprint7ObserverPolicy.RunReleases(new[] {
                new KeyValuePair<string, Action>("owned conditions", () => { throw new InvalidOperationException(named); }),
                new KeyValuePair<string, Action>("projectile trace", () => independent++),
                new KeyValuePair<string, Action>("condition ledger subscription", () => independent++) });
            IEnumerator<int> phase = Sprint7ObserverPolicy.Sequence(isolation, new[] {
                Sprint7ObserverPolicy.Cell("s7-dust", () => FakeCell(isolation, "s7-dust", dustReleases, value => value.Residual("s7-dust", "owned execution breath#1", execution.Read), () => { }, 100)),
                Sprint7ObserverPolicy.Cell("s7-next", () => FakeCell(isolation, "s7-next", () => new List<string>(), value => { }, () => nextStarts++, 1)) }, run, limit).GetEnumerator();
            Assertions.True(phase.MoveNext() && phase.MoveNext(), "The Dust cell is yielded mid-cast.");
            Assertions.True(isolation.Unreported.Contains("s7-dust") && !isolation.Restored, "A cell registered before its first acquisition is unrestored until it reports.");
            phase.Dispose();
            Assertions.True(isolation.Failures.Contains("s7-dust: owned conditions: InvalidOperationException: " + named), "Disposing the enclosing runner publishes the named failure from the cell's own finally.");
            Assertions.True(independent == 2 && isolation.Pending.Contains("s7-dust: owned execution breath#1") && isolation.Unreported.Count == 0,
                "The independent releases still ran, and the live execution is a pending residual.");
            Assertions.True(nextStarts == 0 && isolation.Admitted.SequenceEqual(new[] { "s7-dust" }), "Disposal admitted nothing further.");
            // The same admission guard, on the same result: the execution is still live, so the next cell is refused.
            foreach (int step in Sprint7ObserverPolicy.Sequence(isolation, new[] { Sprint7ObserverPolicy.Cell("s7-next", () => FakeCell(isolation, "s7-next", () => new List<string>(), value => { }, () => nextStarts++, 1)) }, run, limit)) { }
            Assertions.True(isolation.Refused == "s7-next" && nextStarts == 0, "A still-live execution and a failed release refuse the next cell.");
            // The exact execution ends (recovery observation); the named failure stays, so the next cell is still refused.
            execution.Ended = true;
            foreach (int step in Sprint7ObserverPolicy.Sequence(isolation, new[] { Sprint7ObserverPolicy.Cell("s7-next", () => FakeCell(isolation, "s7-next", () => new List<string>(), value => { }, () => nextStarts++, 1)) }, run, limit)) { }
            Assertions.True(isolation.Recoveries.Contains("s7-dust: owned execution breath#1 observed settled") && isolation.Pending.Count == 0, "The exact execution is observed settled, as a recovery kept apart from the failures.");
            Assertions.True(!isolation.Restored && isolation.Refused == "s7-next" && nextStarts == 0, "A repair or a later recovery never erases the named failure: advancement stays refused.");

            // Residuals alone: live refuses, unreadable refuses, settled admits (with the recovery recorded).
            var residual = new Sprint7CellIsolation();
            var live = new FakeExecution();
            int laterStarts = 0;
            foreach (int step in Sprint7ObserverPolicy.Sequence(residual, new[] {
                Sprint7ObserverPolicy.Cell("s7-dust", () => FakeCell(residual, "s7-dust", () => new List<string>(), value => value.Residual("s7-dust", "owned execution breath#2", live.Read), () => { }, 2)),
                Sprint7ObserverPolicy.Cell("s7-later", () => FakeCell(residual, "s7-later", () => new List<string>(), value => { }, () => laterStarts++, 1)) }, run, limit)) { }
            Assertions.True(residual.Refused == "s7-later" && laterStarts == 0 && residual.Failures.Count == 0, "A still-live execution alone prevents the next cell.");
            live.Throws = true;
            residual.Settle();
            Assertions.True(residual.Describe()["unreadable"].Values<string>().Contains("s7-dust: owned execution breath#2") && !residual.Restored, "An unreadable execution is never restored.");
            live.Throws = false; live.Ended = true;
            foreach (int step in Sprint7ObserverPolicy.Sequence(residual, new[] { Sprint7ObserverPolicy.Cell("s7-later", () => FakeCell(residual, "s7-later", () => new List<string>(), value => { }, () => laterStarts++, 1)) }, run, limit)) { }
            Assertions.True(residual.Restored && laterStarts == 1 && residual.Recoveries.Count == 1, "Once the exact execution is observed ended, the next cell is admitted.");

            // A cell whose cleanup throws before it reports leaves the isolation unrestored.
            var silent = new Sprint7CellIsolation();
            int afterSilent = 0;
            try
            {
                foreach (int step in Sprint7ObserverPolicy.Sequence(silent, new[] {
                    Sprint7ObserverPolicy.Cell("s7-dust", () => FakeCell(silent, "s7-dust", () => new List<string>(), value => { }, () => { }, 1, true)),
                    Sprint7ObserverPolicy.Cell("s7-later", () => FakeCell(silent, "s7-later", () => new List<string>(), value => { }, () => afterSilent++, 1)) }, run, limit)) { }
            }
            catch (InvalidOperationException) { }
            Assertions.True(!silent.Restored && silent.Unreported.Contains("s7-dust") && afterSilent == 0, "A cleanup that never reported is not restored.");

            // R6-F2: the callback hook is acquired, the runner is disposed and the unpatch fails; the sources are still
            // retired; the next cell is refused; the outer driver removes its own hook (zero remain) without erasing
            // the failure. Control: an ordinary unpatch leaves zero hooks and admits the next cell.
            foreach (bool inject in new[] { true, false })
            {
                var hookPhase = new Sprint7CellIsolation();
                int hooks = 0, retired = 0, starts = 0;
                Func<List<string>> hookReleases = () =>
                {
                    List<string> failures = Sprint7ObserverPolicy.RunReleases(new[] {
                        new KeyValuePair<string, Action>("cue callback hook", () => { if (inject) throw new InvalidOperationException("injected failure of the cue callback unpatch"); hooks--; }),
                        new KeyValuePair<string, Action>("survivor source", () => retired++), new KeyValuePair<string, Action>("other source", () => retired++) });
                    if (hooks != 0) failures.Add("hooks remaining: " + hooks);
                    return failures;
                };
                IEnumerator<int> hookRun = Sprint7ObserverPolicy.Sequence(hookPhase, new[] {
                    Sprint7ObserverPolicy.Cell("s7-callback", () => FakeCell(hookPhase, "callback-withhold", hookReleases, value => { }, () => hooks++, inject ? 100 : 2)),
                    Sprint7ObserverPolicy.Cell("s7-next", () => FakeCell(hookPhase, "s7-next", () => new List<string>(), value => { }, () => starts++, 1)) }, run, limit).GetEnumerator();
                if (inject)
                {
                    Assertions.True(hookRun.MoveNext() && hooks == 1, "The hook is acquired and the observer is yielded.");
                    hookRun.Dispose();
                    foreach (int step in Sprint7ObserverPolicy.Sequence(hookPhase, new[] { Sprint7ObserverPolicy.Cell("s7-next", () => FakeCell(hookPhase, "s7-next", () => new List<string>(), value => { }, () => starts++, 1)) }, run, limit)) { }
                    Assertions.True(hookPhase.Failures.Contains("callback-withhold: cue callback hook: InvalidOperationException: injected failure of the cue callback unpatch") &&
                        hookPhase.Failures.Contains("callback-withhold: hooks remaining: 1"), "The named unpatch failure and the remaining hook reach the phase.");
                    Assertions.True(retired == 2 && starts == 0 && hookPhase.Refused == "s7-next", "The sources are still retired and the next cell is refused.");
                    hooks = 0; // the outer driver's own repair
                    Assertions.True(hooks == 0 && !hookPhase.Restored, "The driver's repair leaves zero hooks; the child failure stays recorded.");
                }
                else
                {
                    while (hookRun.MoveNext()) { }
                    Assertions.True(hooks == 0 && retired == 2 && starts == 1 && hookPhase.Restored, "Ordinary control: zero hooks, the next cell admitted, isolation restored.");
                }
            }
        }

        // Recovery C3 (C3 review R1-F11): the effects phase's ownership contract through the production orchestration it runs
        // on (Sequence, IsolatedCell, Sprint7Abandonment, ReleaseOwnedEffects, Sprint7OwnedCommands, AcquisitionLedger,
        // Sprint7OwnedRequests, Sprint7HookAttempt, Sprint7SetupScope, RollbackSetup, RepairExact, RequiredCellsComplete). Only
        // the native boundary operations are controlled (a command's process and interruption, a condition's presence and
        // removal, a unit's retirement and settlement, a hook count); every failure starts at that boundary.
        private sealed class FakeCommand
        {
            internal readonly string Id; internal FakeExecution Process; internal bool Finished, InterruptThrows, ProcessUnreadable; internal int Interrupts;
            internal FakeCommand(string id) { Id = id; }
        }

        private sealed class FakeCondition
        {
            internal readonly string Id; internal bool Owned = true, RemoveThrows; internal int Removes; internal FakeExecution Context;
            internal FakeCondition(string id) { Id = id; }
        }

        // A cell's effects as the Dust cell holds them: owned commands, a condition ledger over the present conditions, a foreign
        // control, and a subscription.
        private sealed class FakeEffects
        {
            internal readonly List<FakeCondition> Present = new List<FakeCondition>();
            internal readonly AcquisitionLedger<FakeCondition> Ledger;
            internal readonly Sprint7OwnedCommands<FakeCommand, FakeExecution> Commands;
            internal readonly FakeCondition Foreign = new FakeCondition("foreign") { Owned = false };
            internal bool Subscribed = true, ForeignKeptAtSettlement, UnsubscribeThrows; internal int Unsubscribes;
            internal readonly List<object> Observed = new List<object>();   // observed deliveries, as S7DustConditions keeps them
            internal FakeExecution PendingTarget;                           // an outside-target retirement published at settlement
            private readonly Sprint7CellIsolation isolation; private readonly string owner;
            // Ownership as production attributes it: an Owned fixture flag, or the production AttributedTo over observed
            // deliveries and retained process contexts.
            internal bool IsOwned(FakeCondition condition) { return condition.Owned || Sprint7ObserverPolicy.AttributedTo(condition.Context, Observed, Commands, process => process); }
            internal FakeEffects(Sprint7CellIsolation isolation, string owner)
            {
                this.isolation = isolation; this.owner = owner;
                Present.Add(Foreign);
                Ledger = new AcquisitionLedger<FakeCondition>(Present);
                Commands = new Sprint7OwnedCommands<FakeCommand, FakeExecution>(command => { if (command.ProcessUnreadable) throw new InvalidOperationException("process unreadable"); return command.Process; },
                    process => process.Read(), failure => isolation.Fail(owner, failure));
            }
            internal List<string> Release()
            {
                return Ledger.Release(Present.ToList(), IsOwned, condition =>
                {
                    condition.Removes++;
                    if (condition.RemoveThrows) throw new InvalidOperationException("injected failure of an owned Sickened removal");
                    Present.Remove(condition);
                }, condition => condition.Id);
            }
            internal bool RemainingUnreadable;
            internal List<string> RemainingIds()
            {
                if (RemainingUnreadable) throw new InvalidOperationException("remaining read failed");
                return Ledger.RemainingItems(Present.ToList(), IsOwned).Select(condition => condition.Id).ToList();
            }
            internal JObject ReleaseAll(Sprint7CellIsolation isolation, string owner)
            {
                return Sprint7ObserverPolicy.ReleaseOwnedEffects(isolation, owner, Commands, command => !command.Finished, command =>
                    {
                        command.Interrupts++;
                        command.Process = null; // the interruption clears the command's field
                        if (command.InterruptThrows) throw new InvalidOperationException("injected failure of an owned command interruption: " + command.Id);
                        command.Finished = true;
                    }, command => "owned command " + command.Id, process => "breath#" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(process),
                    Release, RemainingIds, () => { Unsubscribes++; if (UnsubscribeThrows) throw new InvalidOperationException("unsubscribe failed"); Subscribed = false; }, () =>
                    {
                        ForeignKeptAtSettlement = Present.Contains(Foreign);
                        if (PendingTarget != null) this.isolation.Residual(this.owner, "outside target settled", PendingTarget.Read);
                        return Sprint7ObserverPolicy.ControlsKept(new[] { new KeyValuePair<string, Func<bool?>>("foreign", () => Present.Contains(Foreign)) });
                    });
            }
        }

        // The minimal shape of an effects cell: registered before acquisition, its effects released through the production
        // release from its own finally, whichever way it unwinds.
        private static IEnumerable<int> EffectCell(Sprint7CellIsolation isolation, string owner, FakeEffects effects, Action ready, int steps, Action afterRelease = null)
        {
            isolation.Opened(owner);
            try
            {
                ready();
                for (int step = 0; step < steps; step++) yield return 0;
            }
            finally
            {
                effects.ReleaseAll(isolation, owner);
                if (afterRelease != null) afterRelease();
                isolation.Reported(owner);
            }
        }

        private static IEnumerable<int> Counting(Action start) { start(); yield return 0; }

        private sealed class FaultyCell : IEnumerable<int>, IEnumerator<int>
        {
            internal bool ThrowOnDispose, ThrowOnCurrent; private int steps;
            public IEnumerator<int> GetEnumerator() { return this; }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this; }
            public int Current { get { if (ThrowOnCurrent) throw new InvalidOperationException("current failed"); return steps; } }
            object System.Collections.IEnumerator.Current { get { return Current; } }
            public bool MoveNext() { return ++steps <= 1; }
            public void Reset() { }
            public void Dispose() { if (ThrowOnDispose) throw new InvalidOperationException("disposal failed"); }
        }

        internal static void EffectsOwnershipRecovery()
        {
            var limit = TimeSpan.Zero;
            var none = TimeSpan.FromSeconds(5);
            Func<Sprint7CellIsolation, Func<string, Func<IEnumerable<int>>, IEnumerable<int>>> cells = isolation =>
                (name, body) => Sprint7ObserverPolicy.IsolatedCell(isolation, name, body, null);
            Func<string, string> removal = id => "removal of " + id + ": InvalidOperationException: injected failure of an owned Sickened removal";

            // R4-F1, R1-F6, R1-F11: the abandonment driver over the production effects release. The injected fault is a real
            // removal failure of the exact leftover L; variants add, at their own boundaries, an independent command
            // interruption failure, a different residual count, an independent failure whose text contains the expected phrase,
            // and an owner that never reports. Only the exact expected failures are exempt; each variant refuses the next phase cell.
            foreach (string variant in new[] { "control", "command-release", "residual-count", "phrase", "unreported" })
            {
                var phase = new Sprint7CellIsolation();
                bool ready = false; int childNext = 0, phaseNext = 0;
                Sprint7Abandonment driver = null;
                var leftover = new FakeCondition("L");
                FakeEffects effects = null;
                driver = new Sprint7Abandonment
                {
                    Owner = "dust driver", Phase = phase, Ready = () => ready,
                    Inject = () => leftover.RemoveThrows = true, Clear = () => leftover.RemoveThrows = false,
                    Expected = () => new[] { "s7-dust: release failure: " + removal("L"), "s7-dust: owned conditions remaining: 1 (L)" },
                    Repair = () => Sprint7ObserverPolicy.RepairExact(new[] { leftover }, new[] { "L" }, condition => effects.Present.Contains(condition), condition => { condition.Removes++; effects.Present.Remove(condition); }),
                };
                effects = new FakeEffects(driver.Child, "s7-dust");
                var command = new FakeCommand("C") { Process = new FakeExecution { Ended = true }, Finished = true };
                effects.Commands.Own(command);
                Func<IEnumerable<int>> inner = () => EffectCell(driver.Child, "s7-dust", effects, () =>
                {
                    effects.Present.Add(leftover);
                    if (variant == "command-release") { command.Finished = false; command.Process = new FakeExecution { Ended = true }; command.InterruptThrows = true; }
                    if (variant == "residual-count") effects.Present.Add(new FakeCondition("L2") { RemoveThrows = true });
                    if (variant == "phrase") effects.Present.Add(new FakeCondition("L extra") { RemoveThrows = true });
                    if (variant == "unreported") driver.Child.Opened("trace-owner");
                    ready = true;
                }, 100);
                foreach (int step in Sprint7ObserverPolicy.Sequence(phase, new[] {
                    Sprint7ObserverPolicy.Cell("s7-dust-abandoned", () => driver.Run(Sprint7ObserverPolicy.Cell("inner", inner),
                        Sprint7ObserverPolicy.Cell("next", () => Counting(() => childNext++)), cells(driver.Child), limit, none)),
                    Sprint7ObserverPolicy.Cell("s7-later", () => Counting(() => phaseNext++)) }, cells(phase), limit)) { }
                Assertions.True(childNext == 0 && driver.Child.Refused == "next" && leftover.Removes == 2 && !effects.Present.Contains(leftover) && effects.Present.Contains(effects.Foreign),
                    variant + ": the leftover's removal failed once at the boundary (not retried by later releases), the driver's repair removed it, the foreign control is untouched and the child's next cell was refused.");
                // With no execution live at cleanup the settlement is immediate and does not release a second time.
                Assertions.True(effects.Unsubscribes == 1 && !effects.Subscribed, variant + ": with nothing live the ledger unsubscribed at cleanup, once.");
                if (variant == "control")
                    Assertions.True(driver.Proven && phase.Restored && phaseNext == 1 && driver.Unexpected.Count == 0, "control: only the exact expected failures: the driver proves the fixture safe and the next phase cell runs.");
                else
                    Assertions.True(!driver.Proven && !phase.Restored && phase.Refused == "s7-later" && phaseNext == 0 &&
                        (variant == "unreported" ? phase.Failures.Contains("dust driver: child owner never reported: trace-owner") : driver.Unexpected.Count >= 1) &&
                        phase.Failures.Contains("dust driver: the fixture was not proven safe after the driver's repair"),
                        variant + ": the additional failure (or unreported owner) is not exempted by the injected fault's repair; the phase fails and the next phase cell never starts.");
                if (variant == "command-release")
                    Assertions.True(phase.Failures.Contains("dust driver: unexpected child failure: s7-dust: owned command C: InvalidOperationException: injected failure of an owned command interruption: C"),
                        "command-release: the independent interruption failure reaches the phase by name.");
                if (variant == "phrase")
                    Assertions.True(driver.Unexpected.Contains("s7-dust: release failure: " + removal("L extra")), "phrase: text containing the expected phrase is not the expected failure.");
                if (variant == "residual-count")
                    Assertions.True(driver.Unexpected.Contains("s7-dust: owned conditions remaining: 2 (L, L2)") && driver.ExpectedMissing.Contains("s7-dust: owned conditions remaining: 1 (L)"),
                        "residual-count: a different remaining count is neither the expected residual nor exempt.");
            }

            // R7-F5, R1-F7: the driver is disposed during its recovery wait; its repair of the exact leftover returns without
            // removing it. The result is published (still present), the foreign control is untouched, the child failure is kept.
            {
                var phase = new Sprint7CellIsolation();
                bool ready = false;
                var leftover = new FakeCondition("L");
                FakeEffects effects = null;
                var driver = new Sprint7Abandonment
                {
                    Owner = "dust driver", Phase = phase, Ready = () => ready, Inject = () => leftover.RemoveThrows = true, Clear = () => leftover.RemoveThrows = false,
                    Expected = () => new[] { "s7-dust: release failure: " + removal("L"), "s7-dust: owned conditions remaining: 1 (L)" },
                    Repair = () => Sprint7ObserverPolicy.RepairExact(new[] { leftover }, new[] { "L" }, condition => effects.Present.Contains(condition), condition => condition.Removes++),
                };
                effects = new FakeEffects(driver.Child, "s7-dust");
                var live = new FakeExecution();
                var command = new FakeCommand("C") { Process = live };
                effects.Commands.Own(command);
                IEnumerator<int> run = driver.Run(Sprint7ObserverPolicy.Cell("inner", () => EffectCell(driver.Child, "s7-dust", effects, () => { effects.Present.Add(leftover); ready = true; }, 100)),
                    Sprint7ObserverPolicy.Cell("next", () => Counting(() => { })), cells(driver.Child), TimeSpan.FromSeconds(5), none).GetEnumerator();
                for (int step = 0; step < 6 && run.MoveNext(); step++) { }
                Assertions.True(driver.Child.Pending.Any(value => value.StartsWith("s7-dust: owned execution ")) && !driver.Repaired, "R7-F5 setup: in the recovery wait with the execution live and nothing repaired yet.");
                run.Dispose();
                Assertions.True(leftover.Removes == 2 && effects.Present.Contains(leftover) && effects.Present.Contains(effects.Foreign) && (string)driver.Row["repairedWhen"] == "on unwind" &&
                    driver.Child.Failures.Contains("s7-dust: release failure: " + removal("L")) &&
                    phase.Failures.Contains("dust driver: repair failed: L: still present after the repair returned") &&
                    phase.Failures.Contains("dust driver: the fixture was not proven safe after the driver's repair") && phase.Unreported.Count == 0,
                    "R7-F5 / R1-F7: disposal in the recovery wait attempts the exact repair once; a removal that returns with the leftover present is a published failed repair; the foreign control and the child failure are kept.");
                // R2-F2: the child's pending reconciliation moved to the phase; a late condition then arrives and the execution ends;
                // only the phase's own settlement runs it: the exact late condition is released, the ledger unsubscribes once, the
                // foreign control is kept, and the original failure stays.
                Assertions.True(phase.Pending.Contains("dust driver child s7-dust: owned conditions reconciled after settlement") && effects.Subscribed,
                    "R2-F2: the pending reconciliation was handed to the phase; the ledger is still subscribed.");
                var late = new FakeCondition("late");
                effects.Present.Add(late);
                live.Ended = true;
                phase.Settle();
                Assertions.True(late.Removes == 1 && !effects.Present.Contains(late) && effects.Unsubscribes == 1 && !effects.Subscribed && effects.ForeignKeptAtSettlement &&
                    !phase.Restored && phase.Failures.Contains("dust driver: the fixture was not proven safe after the driver's repair"),
                    "R2-F2: the phase's settlement releases the late condition, unsubscribes once and keeps the foreign control; the original failed verdict stays.");
            }

            // R2-F2: a pending reconciliation whose execution never settles is terminated by the phase's teardown: owned conditions
            // released, the ledger unsubscribed, the controls read, and the unestablished settlement named.
            {
                var phase = new Sprint7CellIsolation();
                FakeEffects effects = new FakeEffects(phase, "s7-dust");
                effects.Commands.Own(new FakeCommand("C") { Process = new FakeExecution() });
                foreach (int step in EffectCell(phase, "s7-dust", effects, () => { }, 1)) { }
                var late = new FakeCondition("late");
                effects.Present.Add(late);
                phase.TerminatePending();
                Assertions.True(late.Removes == 1 && effects.Unsubscribes == 1 && phase.Pending.Count == 0 &&
                    phase.Failures.Contains("s7-dust: owned conditions reconciled after settlement: not settled at teardown") &&
                    phase.Failures.Contains("s7-dust: owned conditions reconciled after settlement: reconciled at teardown before every owned execution settled") && !phase.Restored,
                    "R2-F2: terminal teardown runs the bounded reconciliation once and records the uncertainty.");
            }

            // R2-F3: the disposal happens in the same step that establishes readiness; a delivery scheduled for the next step has
            // not happened at the disposal-time readback. A delivery before readiness is seen by that readback and the control fails.
            foreach (bool deliveredFirst in new[] { false, true })
            {
                var phase = new Sprint7CellIsolation();
                bool ready = false, delivered = false; int stepsSeen = 0;
                JObject atDisposal = null;
                var driver = new Sprint7Abandonment
                {
                    Owner = "pre-delivery driver", Phase = phase, ExpectAdmitted = true, Ready = () => ready,
                    AtDisposal = () => atDisposal = new JObject { ["delivered"] = delivered, ["steps"] = stepsSeen },
                    Unsafe = () => atDisposal == null || (bool)atDisposal["delivered"] ? new List<string> { "delivered before disposal" } : new List<string>(),
                };
                FakeEffects effects = new FakeEffects(driver.Child, "s7-dust");
                foreach (int step in driver.Run(Sprint7ObserverPolicy.Cell("inner", () => PreDeliveryCell(driver.Child, effects, deliveredFirst, () => ready = true, () => delivered = true, () => stepsSeen++)),
                    Sprint7ObserverPolicy.Cell("next", () => Counting(() => { })), cells(driver.Child), TimeSpan.FromSeconds(1), none)) { }
                Assertions.True(atDisposal != null && (bool)atDisposal["delivered"] == deliveredFirst && driver.Proven == !deliveredFirst,
                    "R2-F3 (" + (deliveredFirst ? "delivered before readiness: rejected" : "delivery scheduled after readiness: not yet delivered at disposal") + ").");
            }

            // R2-F4: a release that wrongly removes the foreign control fails the result consumed by admission.
            {
                var phase = new Sprint7CellIsolation();
                int later = 0;
                FakeEffects effects = new FakeEffects(phase, "s7-dust");
                // A faulty cleanup removes the foreign control during the cell (the ledger itself never acquires a preexisting instance).
                foreach (int step in Sprint7ObserverPolicy.Sequence(phase, new[] {
                    Sprint7ObserverPolicy.Cell("s7-dust", () => EffectCell(phase, "s7-dust", effects, () => effects.Present.Remove(effects.Foreign), 1)),
                    Sprint7ObserverPolicy.Cell("s7-later", () => Counting(() => later++)) }, cells(phase), limit)) { }
                Assertions.True(!effects.Present.Contains(effects.Foreign) && phase.Failures.Contains("s7-dust: control missing after the owned release: foreign") &&
                    phase.Refused == "s7-later" && later == 0, "R2-F4: removing the foreign control in the owned release is a named failure and the next cell is refused.");
            }

            // R2-F6: the command-to-process association, and delivery attribution isolated from an unreadable command.
            {
                var flaky = new FakeCommand("flaky") { ProcessUnreadable = true };
                var owned = new Sprint7OwnedCommands<FakeCommand, FakeExecution>(command => { if (command.ProcessUnreadable) throw new InvalidOperationException("unreadable"); return command.Process; },
                    process => process.Read());
                owned.Own(flaky);
                Assertions.True(owned.EndedOf(flaky) == null && owned.Unknown.Contains(flaky), "R2-F6: an unreadable command is unknown.");
                var recovered = new FakeExecution();
                flaky.ProcessUnreadable = false; flaky.Process = recovered;
                Assertions.True(owned.EndedOf(flaky) == false && !owned.Unknown.Contains(flaky) && ReferenceEquals(owned.ProcessFor(flaky), recovered), "R2-F6: once readable its process is bound.");
                flaky.Process = null; recovered.Ended = true;
                Assertions.True(owned.EndedOf(flaky) == true, "R2-F6: after its field clears, the bound process still answers and settles.");
                var owned2 = new Sprint7OwnedCommands<FakeCommand, FakeExecution>(command => command.Process, process => process.Read());
                var broken = new FakeExecution(); var known = new FakeExecution();
                owned2.Own(new FakeCommand("broken") { Process = broken }); owned2.Own(new FakeCommand("known") { Process = known }); owned2.Capture();
                bool brokenRead = false;
                Assertions.True(owned2.RetainsContext(known, value => { if (ReferenceEquals(value, broken)) { brokenRead = true; throw new InvalidOperationException("context unreadable"); } return value; }) && brokenRead,
                    "R2-F6: the throwing context getter ran first, and the independently known execution was still attributed.");
            }

            // R2-F7: per-actor teardown: one failure does not skip the others; an actor not listed is untouched.
            {
                var released = new List<string>();
                List<string> failures = Sprint7ObserverPolicy.ReleaseEach(new[] { "a", "b", "c" }, actor => "combat " + actor, actor =>
                {
                    if (actor == "a") throw new InvalidOperationException("interruption failed");
                    released.Add(actor);
                });
                Assertions.True(failures.SequenceEqual(new[] { "combat a: InvalidOperationException: interruption failed" }) && released.SequenceEqual(new[] { "b", "c" }) && !released.Contains("foreign"),
                    "R2-F7: the failing actor is named, the later owned actors are still released, and the unlisted foreign actor is untouched.");
            }

            // R2-F8: an early condition-release failure enters the result at the boundary; a later cleanup read then throws; both
            // remain, and the next cell is refused.
            {
                var phase = new Sprint7CellIsolation();
                int later = 0;
                FakeEffects effects = new FakeEffects(phase, "s7-dust");
                var stuck = new FakeCondition("stuck") { RemoveThrows = true };
                effects.Present.Add(stuck);
                foreach (int step in Sprint7ObserverPolicy.Sequence(phase, new[] {
                    Sprint7ObserverPolicy.Cell("s7-dust", () => EarlyClearThenFailingRead(phase, effects)),
                    Sprint7ObserverPolicy.Cell("s7-later", () => Counting(() => later++)) }, cells(phase), limit)) { }
                Assertions.True(phase.Failures.Contains("s7-dust: release failure: " + removal("stuck")) &&
                    phase.Failures.Contains("s7-dust: owned conditions unread") && phase.Refused == "s7-later" && later == 0 && stuck.Removes == 1,
                    "R2-F8: the early removal failure was published where it happened and survives the later unreadable read; the next cell is refused.");
            }

            // R1-F2: an ordinary cell disposed after its execution started and before delivery; a condition is delivered late and
            // the execution then ends. The ledger stays subscribed until settlement, the late owned condition is released there,
            // the foreign control is kept through it, and the next cell is admitted. Variant: the late removal fails.
            foreach (bool lateFails in new[] { false, true })
            {
                var phase = new Sprint7CellIsolation();
                bool ready = false; int childNext = 0;
                FakeEffects effects = null;
                bool waited = false;
                var driver = new Sprint7Abandonment { Owner = "pre-delivery driver", Phase = phase, Ready = () => ready, ExpectAdmitted = !lateFails,
                    AfterRepair = () => Counting(() => waited = true) };
                effects = new FakeEffects(driver.Child, "s7-dust");
                // The driver's safety readback: the foreign control kept through settlement and the ledger unsubscribed after it.
                driver.Unsafe = () => !waited || !effects.ForeignKeptAtSettlement || effects.Subscribed ? new List<string> { "not settled safely" } : new List<string>();
                var process = new FakeExecution();
                effects.Commands.Own(new FakeCommand("C") { Process = process });
                IEnumerator<int> run = driver.Run(Sprint7ObserverPolicy.Cell("inner", () => EffectCell(driver.Child, "s7-dust", effects, () => ready = true, 100)),
                    Sprint7ObserverPolicy.Cell("next", () => Counting(() => childNext++)), cells(driver.Child), TimeSpan.FromSeconds(1), none).GetEnumerator();
                for (int step = 0; step < 4 && run.MoveNext(); step++) { }
                Assertions.True(effects.Subscribed && driver.Child.Pending.Contains("s7-dust: owned conditions reconciled after settlement") && childNext == 0,
                    "R1-F2: after disposal the ledger stays subscribed and the reconciliation is pending while the execution runs.");
                var late = new FakeCondition("late") { RemoveThrows = lateFails };
                effects.Present.Add(late);  // delivered by the retained execution after the cell was disposed
                process.Ended = true;
                while (run.MoveNext()) { }
                if (!lateFails)
                    Assertions.True(late.Removes == 1 && !effects.Present.Contains(late) && effects.ForeignKeptAtSettlement && !effects.Subscribed && effects.Unsubscribes == 1 &&
                        childNext == 1 && driver.Child.Restored && driver.Proven && phase.Restored,
                        "R1-F2: at settlement the late owned condition is released, the foreign control is kept, the ledger unsubscribes, and the next cell is admitted.");
                else
                    Assertions.True(childNext == 0 && driver.Child.Failures.Contains("s7-dust: late release failure: " + removal("late")) &&
                        driver.Child.Failures.Contains("s7-dust: late owned condition remaining after settlement: late") && !phase.Restored,
                        "R1-F2: a late condition that cannot be released is a named failure and the next cell is refused.");
            }

            // R1-F3: an early (pre-delivery) release failure enters the cumulative result at once; the command then finishes and
            // the final release succeeds, yet the failure stays and the next phase cell is refused.
            {
                var phase = new Sprint7CellIsolation();
                int later = 0;
                FakeEffects effects = new FakeEffects(phase, "s7-dust");
                var command = new FakeCommand("early") { Process = new FakeExecution { Ended = true }, InterruptThrows = true };
                effects.Commands.Own(command);
                foreach (int step in Sprint7ObserverPolicy.Sequence(phase, new[] {
                    Sprint7ObserverPolicy.Cell("s7-dust", () => EarlyRelease(phase, effects, command)),
                    Sprint7ObserverPolicy.Cell("s7-later", () => Counting(() => later++)) }, cells(phase), limit)) { }
                Assertions.True(phase.Failures.Contains("s7-dust: owned command early: InvalidOperationException: injected failure of an owned command interruption: early") &&
                    effects.Commands.Failures.Count == 1 && command.Finished && phase.Refused == "s7-later" && later == 0 &&
                    !Sprint7ObserverPolicy.RequiredCellsComplete(phase, new[] { "s7-dust", "s7-later" }),
                    "R1-F3: the early failure is published at once and survives the later successful release; the next cell is refused and the qualification is incomplete.");
            }

            // R1-F4: a command whose process cannot be read is a named unknown residual (not a known null); a command whose
            // process appears during the yielded frame is captured before its interruption clears the field.
            {
                var phase = new Sprint7CellIsolation();
                int later = 0;
                FakeEffects effects = new FakeEffects(phase, "s7-dust");
                var unreadable = new FakeCommand("unreadable") { ProcessUnreadable = true };
                var starting = new FakeCommand("starting");
                effects.Commands.Own(unreadable); effects.Commands.Own(starting);
                var started = new FakeExecution { Ended = true };
                foreach (int step in Sprint7ObserverPolicy.Sequence(phase, new[] {
                    Sprint7ObserverPolicy.Cell("s7-dust", () => EffectCell(phase, "s7-dust", effects, () => { }, 1, null)),
                    Sprint7ObserverPolicy.Cell("s7-later", () => Counting(() => later++)) }, (name, body) =>
                    {
                        if (name == "s7-dust") starting.Process = started; // execution starts during the yielded frame
                        return Sprint7ObserverPolicy.IsolatedCell(phase, name, body, null);
                    }, limit)) { }
                Assertions.True(phase.Pending.Contains("s7-dust: unreadable execution of owned command unreadable") && unreadable.Interrupts == 1 && starting.Interrupts == 1 &&
                    effects.Commands.Retains(started) && starting.Process == null && phase.Refused == "s7-later" && later == 0,
                    "R1-F4: the unreadable execution is a named residual that refuses the next cell; the started process was retained before the interruption cleared it; both commands were interrupted.");
            }

            // R1-F8: a residual outlives the admission deadline, so the required next cell is refused; the residual then settles
            // in final restoration. Physical restoration holds, the declared qualification does not.
            {
                var phase = new Sprint7CellIsolation();
                var residual = new FakeExecution();
                int later = 0;
                foreach (int step in Sprint7ObserverPolicy.Sequence(phase, new[] {
                    Sprint7ObserverPolicy.Cell("s7-a", () => ResidualCell(phase, residual)),
                    Sprint7ObserverPolicy.Cell("s7-b", () => Counting(() => later++)) }, cells(phase), limit)) { }
                residual.Ended = true;
                phase.Settle();
                Assertions.True(phase.Restored && phase.Refused == "s7-b" && later == 0 && !Sprint7ObserverPolicy.RequiredCellsComplete(phase, new[] { "s7-a", "s7-b" }) &&
                    Sprint7ObserverPolicy.RequiredCellsComplete(new Sprint7CellIsolation(), new string[0]),
                    "R1-F8: eventual restoration does not turn a refused required cell into a completed qualification.");
            }

            // R1-F10: disposal-only failure, a throwing Current and a throwing completion publication reach consistent outcomes.
            foreach (string mode in new[] { "dispose", "current", "completion" })
            {
                var isolation = new Sprint7CellIsolation();
                Exception reported = null; bool called = false;
                var faulty = new FaultyCell { ThrowOnDispose = mode == "dispose", ThrowOnCurrent = mode == "current" };
                foreach (int step in Sprint7ObserverPolicy.IsolatedCell(isolation, "cell", () => faulty, (name, error) =>
                {
                    called = true; reported = error;
                    if (mode == "completion") throw new InvalidOperationException("completion receipt failed");
                })) { }
                if (mode == "dispose")
                    Assertions.True(called && reported != null && !isolation.Completed.Contains("cell") && isolation.Failures.Contains("cell: cell disposal: InvalidOperationException: disposal failed"),
                        "R1-F10: a disposal-only failure is reported to completion as a failure and the cell is not completed.");
                if (mode == "current")
                    Assertions.True(called && reported != null && isolation.Failures.Contains("cell: cell exception: InvalidOperationException: current failed") && !isolation.Completed.Contains("cell"),
                        "R1-F10: a throwing Current is the cell's exception.");
                if (mode == "completion")
                    Assertions.True(called && reported == null && !isolation.Completed.Contains("cell") && isolation.Failures.Contains("cell: completion publication: InvalidOperationException: completion receipt failed") &&
                        !isolation.Restored && !Sprint7ObserverPolicy.RequiredCellsComplete(isolation, new[] { "cell" }),
                        "R1-F10: a failed completion publication is a sticky isolation failure and the cell is not a successful completion.");
            }

            // R1-F1: one retirement record across the cell's direct retirement, its unwinding release and the phase teardown, for
            // each failure position. The failed source is attempted once, stays accounted for by identity, the other source and
            // an unrelated phase unit are still released, the failure is published once, and isolation fails.
            foreach (string failing in new[] { "other", "keeper" })
            {
                var phase = new Sprint7CellIsolation();
                var sources = new Sprint7OwnedRequests<FakeExecution>();
                var other = new FakeExecution(); var keeper = new FakeExecution(); var extra = new FakeExecution();
                var attempts = new Dictionary<FakeExecution, int> { { other, 0 }, { keeper, 0 }, { extra, 0 } };
                FakeExecution bad = failing == "other" ? other : keeper;
                var fixtureAttempted = new List<FakeExecution>();
                // The production retirement operation: the attempt is recorded in the fixture's collection before the disposal.
                Action<FakeExecution> retire = unit => Sprint7ObserverPolicy.RetireRecorded(fixtureAttempted, unit,
                    value => { attempts[value]++; if (ReferenceEquals(value, bad)) throw new InvalidOperationException("injected retirement failure"); value.Ended = true; });
                int later = 0;
                foreach (int step in Sprint7ObserverPolicy.Sequence(phase, new[] {
                    Sprint7ObserverPolicy.Cell("s7-callback", () => RetiringObserver(phase, sources, other, keeper, retire)),
                    Sprint7ObserverPolicy.Cell("s7-later", () => Counting(() => later++)) }, cells(phase), limit)) { }
                // The phase teardown: every phase unit not yet requested, each on its own, then settlement by identity.
                int teardown = sources.ReleaseOutstanding(new[] { other, keeper, extra }, unit => "phase unit", retire);
                sources.Publish(phase, "phase teardown", unit => unit.Read());
                // The fixture's outer teardown: the production decision over the actual collection the retirement operation wrote.
                foreach (FakeExecution unit in Sprint7ObserverPolicy.FixtureTeardownCandidates(new[] { other, keeper, extra }, fixtureAttempted)) retire(unit);
                Assertions.True(fixtureAttempted.Count == 3 && fixtureAttempted.Contains(bad), "R1-F11: the production retirement operation recorded every attempt, the failing one included.");
                string label = failing == "other" ? "other source" : "survivor source";
                Assertions.True(attempts[bad] == 1 && attempts[failing == "other" ? keeper : other] == 1 && attempts[extra] == 1 && teardown == 1 &&
                    phase.Failures.Count(value => value.EndsWith(label + ": InvalidOperationException: injected retirement failure")) == 1 &&
                    phase.Failures.Contains("s7-callback: cell exception: InvalidOperationException: injected retirement failure") &&
                    phase.Pending.Count(value => value == "callback-withhold: " + label + " settled") == 1 && !phase.Restored && phase.Refused == "s7-later" && later == 0,
                    "R1-F1 (" + failing + " fails): one attempt per source across the cell and the phase teardown; the failure is published once and the unsettled source stays a residual by identity; independent releases continue; the next cell is refused.");
            }

            // R1-F9: a set-up's second acquisition subscribes and then throws; both releases are attempted, newest first; the
            // original error is the cell's; the next cell is refused.
            {
                var phase = new Sprint7CellIsolation();
                var order = new List<string>();
                int later = 0;
                var bus = new List<FakeHandler>();
                foreach (int step in Sprint7ObserverPolicy.Sequence(phase, new[] {
                    Sprint7ObserverPolicy.Cell("s7-dust", () => SetupThenFail(phase, order, bus)),
                    Sprint7ObserverPolicy.Cell("s7-later", () => Counting(() => later++)) }, cells(phase), limit)) { }
                Assertions.True(order.SequenceEqual(new[] { "recorder subscribed", "saves subscribed", "saves unsubscribed", "recorder unsubscribed" }) && bus.Count == 0 &&
                    phase.Failures.Contains("s7-dust: cell exception: InvalidOperationException: saves subscription failed after registering") && phase.Refused == "s7-later" && later == 0,
                    "R1-F9: the exact handler (constructed and listed before its subscription) and the earlier recorder are both unsubscribed, newest first; the original error stays; the next cell is refused.");
            }

            // R1-F5: the shared rollback runs the unhook whatever the counts read; unreadable counts are unknown, never zero; the
            // original set-up error survives in the set-up failure; an unreadable readback refuses clean isolation.
            {
                int unhooks = 0;
                int? installed, remaining;
                // The unhook is the production per-release runner: one failing unpatch beside an independent release.
                bool independentReleased = false;
                List<string> failures = Sprint7ObserverPolicy.RollbackSetup(() => { throw new InvalidOperationException("count failed"); }, () =>
                    {
                        unhooks++;
                        return Sprint7ObserverPolicy.RunReleases(new[] {
                            new KeyValuePair<string, Action>("hook", () => { throw new InvalidOperationException("rollback unpatch failed"); }),
                            new KeyValuePair<string, Action>("second hook", () => independentReleased = true) });
                    }, out installed, out remaining);
                Assertions.True(independentReleased && failures.Contains("hook: InvalidOperationException: rollback unpatch failed") &&
                    failures.Contains("count before rollback: InvalidOperationException: count failed") && failures.Contains("count after rollback: InvalidOperationException: count failed"),
                    "R1-F5: the independent unpatch still ran; the failing unpatch and both read errors are kept individually.");
                int reads = 0; int? firstInstalled, laterRemaining;
                List<string> recovered = Sprint7ObserverPolicy.RollbackSetup(() => { if (reads++ == 0) throw new InvalidOperationException("first read failed"); return 0; }, () => new List<string>(),
                    out firstInstalled, out laterRemaining);
                Assertions.True(firstInstalled == null && laterRemaining == 0 && recovered.SequenceEqual(new[] { "count before rollback: InvalidOperationException: first read failed" }) &&
                    Sprint7ObserverPolicy.SetupFailure("Observer", new InvalidOperationException("original"), recovered, laterRemaining).Message.Contains("rollback INCOMPLETE"),
                    "R1-F5: a first read that failed is not hidden by a later zero: the rollback is not reported complete.");
                string message = Sprint7ObserverPolicy.SetupFailure("Observer", new InvalidOperationException("original set-up failure"), failures, remaining).Message;
                Assertions.True(unhooks == 1 && installed == null && remaining == null && message.Contains("owned items unknown (readback failed)") && message.EndsWith("InvalidOperationException: original set-up failure") &&
                    message.Contains("rollback unpatch failed"), "R1-F5: with both counts unreadable the rollback still ran; the result is unknown and the original error and rollback error are kept.");
                var isolation = new Sprint7CellIsolation();
                var attempt = new Sprint7HookAttempt<object>("trace", () => { throw new InvalidOperationException("count failed"); });
                try { attempt.Acquire(() => { throw new InvalidOperationException("original set-up failure"); }); } catch (InvalidOperationException) { }
                attempt.Publish(isolation, "trace-setup");
                Assertions.True(isolation.Failures.SequenceEqual(new[] { "trace-setup: trace setup failed: InvalidOperationException: original set-up failure", "trace-setup: trace hook readback unreadable: InvalidOperationException: count failed" }) && !isolation.Restored,
                    "R1-F5: the set-up error and the unknown readback are both kept, and isolation is not restored.");
            }

            // R3-F1: after the driver hands its pending reconciliation to the phase, the reconciliation's later failure (a failing
            // unsubscribe) and a residual it creates (an outside-target retirement) reach the phase, beside the original failure.
            {
                var phase = new Sprint7CellIsolation();
                bool ready = false;
                FakeEffects effects = null;
                var driver = new Sprint7Abandonment { Owner = "dust driver", Phase = phase, Ready = () => ready };
                effects = new FakeEffects(driver.Child, "s7-dust");
                var live = new FakeExecution();
                effects.Commands.Own(new FakeCommand("C") { Process = live });
                IEnumerator<int> run = driver.Run(Sprint7ObserverPolicy.Cell("inner", () => EffectCell(driver.Child, "s7-dust", effects, () => ready = true, 100)),
                    Sprint7ObserverPolicy.Cell("next", () => Counting(() => { })), cells(driver.Child), TimeSpan.FromSeconds(5), none).GetEnumerator();
                for (int step = 0; step < 4 && run.MoveNext(); step++) { }
                run.Dispose();
                effects.UnsubscribeThrows = true;
                effects.PendingTarget = new FakeExecution();
                live.Ended = true;
                phase.Settle();
                Assertions.True(phase.Failures.Contains("dust driver child s7-dust: condition ledger unsubscribe: InvalidOperationException: unsubscribe failed") &&
                    phase.Pending.Contains("dust driver child s7-dust: outside target settled") &&
                    phase.Failures.Contains("dust driver: the fixture was not proven safe after the driver's repair") && driver.Child.Failures.Any(value => value.Contains("unsubscribe failed")),
                    "R3-F1: the late unsubscribe failure and the new outside-target residual reach the phase (the child keeps its history) beside the original failure.");
                effects.PendingTarget.Ended = true;
                phase.Settle();
                Assertions.True(!phase.Pending.Any(value => value.EndsWith("outside target settled")), "R3-F1: the forwarded residual is consumed by the phase's own settlement.");
            }

            // R3-F4: the census reads the execution ended, then its end-state read throws; the reconciliation stays owned and terminal
            // teardown runs it once, with the uncertainty named.
            {
                var phase = new Sprint7CellIsolation();
                FakeEffects effects = new FakeEffects(phase, "s7-dust");
                var process = new FakeExecution { Ended = true, ThrowAfter = 2 };   // reads: census Live(), census loop, then the settlement read throws
                effects.Commands.Own(new FakeCommand("C") { Process = process, Finished = true });
                foreach (int step in EffectCell(phase, "s7-dust", effects, () => { }, 1)) { }
                Assertions.True(phase.Pending.Contains("s7-dust: owned conditions reconciled after settlement") && effects.Subscribed && effects.Unsubscribes == 0,
                    "R3-F4: an unreadable final read keeps the reconciliation owned and the ledger subscribed.");
                phase.TerminatePending();
                Assertions.True(effects.Unsubscribes == 1 && !effects.Subscribed && phase.Failures.Contains("s7-dust: owned conditions reconciled after settlement: not settled at teardown"),
                    "R3-F4: terminal teardown runs the reconciliation exactly once and names the unestablished settlement.");
            }

            // R3-F2: a condition delivered while its command's process was unreadable is reconciled once the process is recovered
            // and ended; the foreign control survives; no observed delivery is fabricated; the next cell is then admitted.
            {
                var phase = new Sprint7CellIsolation();
                FakeEffects effects = new FakeEffects(phase, "s7-dust");
                var hidden = new FakeExecution();
                var command = new FakeCommand("C") { ProcessUnreadable = true, Finished = true };
                effects.Commands.Own(command);
                var missed = new FakeCondition("missed") { Owned = false, Context = hidden };
                foreach (int step in EffectCell(phase, "s7-dust", effects, () => effects.Present.Add(missed), 1)) { }
                Assertions.True(effects.Present.Contains(missed) && phase.Pending.Contains("s7-dust: unreadable execution of owned command C") && !phase.Restored,
                    "R3-F2: while the process is unreadable the missed condition is not attributable and the unknown keeps isolation unrestored.");
                command.ProcessUnreadable = false; command.Process = hidden; hidden.Ended = true;
                phase.Settle();
                Assertions.True(!effects.Present.Contains(missed) && effects.Present.Contains(effects.Foreign) && effects.Observed.Count == 0 && phase.Pending.Count == 0 && phase.Restored,
                    "R3-F2: once the process is recovered and ended, settlement releases the condition attributed by its context; the foreign control stays; no delivery was fabricated.");
            }

            // R2-F7: exact issued commands by reference; one interruption fails; the later command is interrupted; another owner's
            // command on the same actor is untouched; the failing actor's queue maintenance and combat exit still run.
            {
                var interrupted = new List<string>(); var maintained = new List<string>(); var left = new List<string>();
                var issued = new[] { new KeyValuePair<string, string>("A", "a1"), new KeyValuePair<string, string>("B", "b1") };
                List<string> failures = Sprint7ObserverPolicy.ReleaseIssuedCommands(issued, new[] { "A", "B" }, unit => true, command => false,
                    (unit, command) => { if (command == "a1") throw new InvalidOperationException("interruption failed"); interrupted.Add(command); },
                    unit => maintained.Add(unit), unit => left.Add(unit), unit => unit, command => command);
                Assertions.True(failures.SequenceEqual(new[] { "interrupt a1 on A: InvalidOperationException: interruption failed" }) && interrupted.SequenceEqual(new[] { "b1" }) &&
                    !interrupted.Contains("b2") && maintained.SequenceEqual(new[] { "A", "B" }) && left.SequenceEqual(new[] { "A", "B" }),
                    "R2-F7: the failure is named; b1 is interrupted and the foreign b2 (not issued) untouched; queue and combat cleanup still run for both actors.");
            }

            // R3-F3: required controls are declared per path; a required same-caster control with no handle, and an unreadable
            // required read, are named failures; a control not yet due is reported as such.
            {
                var row = new JObject();
                List<string> failures = Sprint7ObserverPolicy.ControlResults(new[] {
                    new KeyValuePair<string, KeyValuePair<bool, bool?>>("caster-behind control", new KeyValuePair<bool, bool?>(true, null)),
                    new KeyValuePair<string, KeyValuePair<bool, bool?>>("same-caster unowned control (no handle)", new KeyValuePair<bool, bool?>(true, false)),
                    new KeyValuePair<string, KeyValuePair<bool, bool?>>("later control", new KeyValuePair<bool, bool?>(false, false)) }, row);
                Assertions.True(failures.SequenceEqual(new[] { "required control unreadable after the owned release: caster-behind control",
                    "required control missing after the owned release: same-caster unowned control (no handle)" }) && (string)row["controls"]["later control"] == "not due on this path",
                    "R3-F3: unreadable and missing required controls are named; a control not due on the path is not.");
            }

            // R1-F9: a phase set-up placement throws after both subscriptions; every release (the placement's own position restore
            // included) runs newest first; the original error is the phase's.
            {
                var order = new List<string>();
                var setup = new Sprint7SetupScope();
                var isolation = new Sprint7CellIsolation();
                try
                {
                    setup.Acquire("recorder subscription", () => order.Add("recorder subscribed"), () => order.Add("recorder unsubscribed"));
                    setup.Acquire("saves subscription", () => order.Add("saves subscribed"), () => order.Add("saves unsubscribed"));
                    try { setup.Acquire("baseline hostile placement", () => { throw new InvalidOperationException("ground lookup failed"); }, () => order.Add("position restored")); }
                    catch (InvalidOperationException error) { isolation.Fail("phase setup", "baseline hostile placement: " + error.GetType().Name + ": " + error.Message); throw; }
                }
                catch (InvalidOperationException) { }
                isolation.Fail("phase", setup.ReleaseAll());
                Assertions.True(order.SequenceEqual(new[] { "recorder subscribed", "saves subscribed", "position restored", "saves unsubscribed", "recorder unsubscribed" }) &&
                    isolation.Failures.SequenceEqual(new[] { "phase setup: baseline hostile placement: InvalidOperationException: ground lookup failed" }) &&
                    !Sprint7ObserverPolicy.RequiredCellsComplete(isolation, new[] { "s7-ankylosaurus" }),
                    "R1-F9: the placement failure stays in the phase result while both subscriptions and the position are released.");
            }

            // R2-F8: an ordinary removal exception without an ID is bound to its condition at the boundary and survives a later read failure.
            {
                var phase = new Sprint7CellIsolation();
                int later = 0;
                FakeEffects effects = new FakeEffects(phase, "s7-dust");
                var plain = new FakeCondition("plain-instance");
                effects.Present.Add(plain);
                var ledger = effects.Ledger;
                foreach (int step in Sprint7ObserverPolicy.Sequence(phase, new[] {
                    Sprint7ObserverPolicy.Cell("s7-dust", () => PlainRemovalThenFailingRead(phase, effects, plain)),
                    Sprint7ObserverPolicy.Cell("s7-later", () => Counting(() => later++)) }, cells(phase), limit)) { }
                Assertions.True(phase.Failures.Contains("s7-dust: release failure: removal of plain-instance: InvalidOperationException: engine refused") &&
                    phase.Failures.Contains("s7-dust: owned conditions unread") && phase.Refused == "s7-later" && later == 0,
                    "R2-F8: the removal is bound to its instance at the boundary and both causes remain; the next cell is refused.");
            }

            // R1-F8, R1-F12, R2-F9: the selections' required cells and the casts expectation the runner emits.
            Assertions.True(!Sprint7ObserverPolicy.RequiredEffectsCells(true).Contains("s7-samples") && Sprint7ObserverPolicy.RequiredEffectsCells(false).Contains("s7-samples") &&
                Sprint7ObserverPolicy.RequiredEffectsCells(true).Contains("s7-dust-pre-delivery-disposed") && Sprint7ObserverPolicy.RequiredEffectsCells(true).Length == 7,
                "The mechanics-only selection requires the seven mechanical cells and not the samples cell; the presentation selection adds the samples.");
            Assertions.True(!Sprint7ObserverPolicy.DustCastsExpectation(true).Contains("framed on an unobstructed bearing") &&
                Sprint7ObserverPolicy.DustCastsExpectation(false).Contains("framed on an unobstructed bearing"),
                "R2-F9: the mechanics-only Dust casts check asserts no framing; the presentation profile keeps its framing clause.");
        }

        // The Dust cell's pre-delivery mode: readiness established in one step, a delivery possible in the next.
        private static IEnumerable<int> PreDeliveryCell(Sprint7CellIsolation isolation, FakeEffects effects, bool deliveredFirst, Action ready, Action deliver, Action step)
        {
            isolation.Opened("s7-dust");
            try
            {
                step();
                if (deliveredFirst) deliver();
                ready();
                yield return 0;
                step();
                deliver(); // would happen in the next frame
                yield return 0;
            }
            finally { effects.ReleaseAll(isolation, "s7-dust"); isolation.Reported("s7-dust"); }
        }

        private static IEnumerable<int> PlainRemovalThenFailingRead(Sprint7CellIsolation isolation, FakeEffects effects, FakeCondition plain)
        {
            isolation.Opened("s7-dust");
            try
            {
                // An engine removal error whose message carries no identity.
                foreach (string failure in effects.Ledger.Release(effects.Present.ToList(), effects.IsOwned, condition => { throw new InvalidOperationException("engine refused"); }, condition => condition.Id))
                    isolation.Fail("s7-dust", "release failure: " + failure);
                yield return 0;
            }
            finally
            {
                effects.RemainingUnreadable = true;
                effects.ReleaseAll(isolation, "s7-dust");
                isolation.Reported("s7-dust");
            }
        }

        // An early between-casts clear whose failure is published where it happens (as the Dust cell's clearSickened does), then
        // a final release whose remaining read throws.
        private static IEnumerable<int> EarlyClearThenFailingRead(Sprint7CellIsolation isolation, FakeEffects effects)
        {
            isolation.Opened("s7-dust");
            try
            {
                foreach (string failure in effects.Release()) isolation.Fail("s7-dust", "release failure: " + failure);
                yield return 0;
            }
            finally
            {
                effects.RemainingUnreadable = true;
                effects.ReleaseAll(isolation, "s7-dust");
                isolation.Reported("s7-dust");
            }
        }

        private static IEnumerable<int> EarlyRelease(Sprint7CellIsolation isolation, FakeEffects effects, FakeCommand command)
        {
            isolation.Opened("s7-dust");
            try
            {
                effects.Commands.ReleaseEach(value => !value.Finished, value => { value.Interrupts++; throw new InvalidOperationException("injected failure of an owned command interruption: " + value.Id); },
                    value => "owned command " + value.Id, new[] { command });
                yield return 0;
                command.InterruptThrows = false; command.Finished = true; // the command finishes later on its own
            }
            finally { effects.ReleaseAll(isolation, "s7-dust"); isolation.Reported("s7-dust"); }
        }

        private static IEnumerable<int> ResidualCell(Sprint7CellIsolation isolation, FakeExecution residual)
        {
            isolation.Opened("s7-a");
            try { yield return 0; }
            finally { isolation.Residual("s7-a", "owned execution breath#1", residual.Read); isolation.Reported("s7-a"); }
        }

        // The callback observer's retirement shape over the shared record: direct requests in order, then the unwinding release
        // of anything not yet requested, then publication.
        private static IEnumerable<int> RetiringObserver(Sprint7CellIsolation isolation, Sprint7OwnedRequests<FakeExecution> sources, FakeExecution other, FakeExecution keeper,
            Action<FakeExecution> retire)
        {
            isolation.Opened("callback-withhold");
            try
            {
                yield return 0;
                sources.Request(other, "other source", retire);
                sources.Request(keeper, "survivor source", retire);
            }
            finally
            {
                // The observer's unwinding release, through the shared production operation.
                Sprint7ObserverPolicy.ReleaseObserverSources(sources, new[] { new KeyValuePair<FakeExecution, string>(keeper, "survivor source"),
                    new KeyValuePair<FakeExecution, string>(other, "other source") }, retire, isolation, "callback-withhold", unit => unit.Read());
                isolation.Reported("callback-withhold");
            }
        }

        // A rule handler as the effects callers now hold one: constructed (and listed) first; its subscription registers it on the
        // bus and may then throw; its disposal removes exactly it.
        private sealed class FakeHandler
        {
            private readonly List<FakeHandler> bus; private readonly List<string> order; private readonly string name; internal bool ThrowAfterRegistering;
            internal FakeHandler(List<FakeHandler> bus, List<string> order, string name) { this.bus = bus; this.order = order; this.name = name; }
            internal void Subscribe()
            {
                bus.Add(this); order.Add(name + " subscribed");
                if (ThrowAfterRegistering) throw new InvalidOperationException(name + " subscription failed after registering");
            }
            internal void Dispose() { bus.Remove(this); order.Add(name + " unsubscribed"); }
        }

        private static IEnumerable<int> SetupThenFail(Sprint7CellIsolation isolation, List<string> order, List<FakeHandler> bus)
        {
            isolation.Opened("s7-dust");
            var setup = new Sprint7SetupScope();
            try
            {
                var recorder = new FakeHandler(bus, order, "recorder");
                setup.Acquire("recorder subscription", recorder.Subscribe, recorder.Dispose);
                var saves = new FakeHandler(bus, order, "saves") { ThrowAfterRegistering = true };
                setup.Acquire("saves subscription", saves.Subscribe, saves.Dispose);
                yield return 0;
            }
            finally
            {
                isolation.Fail("s7-dust", setup.ReleaseAll());
                isolation.Reported("s7-dust");
            }
        }

        // Recovery C3: the combined native qualification's callback-loss row (session 20261126T054228Z, candidate de63d6f4),
        // persisted as the in-game collector wrote it. C1's revised evaluator judged it live in that run (its recorded
        // verdict); the persisted row, and its collector-shaped live counterpart, must receive the same verdict here.
        internal static void CombinedRunVerdictsAgree()
        {
            // The earlier combined run (product 6abd692fa7; C1's same-run criterion) and recovery candidate 3's run (66b0ff3f14).
            foreach (string file in new[] { "20261126T054228Z-callback-loss-row.json", "20261126T134425Z-callback-loss-row.json" })
                RunVerdictsAgree(file);
        }

        private static void RunVerdictsAgree(string file)
        {
            JObject record = JObject.Parse(File.ReadAllText("docs/evidence/sprint7/recovery-c3/" + file));
            var persisted = (JObject)record["row"];
            string live = (string)record["liveVerdict"];
            Assertions.True(record["liveVerdict"].Type == JTokenType.Null && persisted["verdict"].Type == JTokenType.Null && live == null,
                "The live verdict recorded in the same run is null (characterized).");
            Assertions.True((string)persisted["evaluator"] == "Sprint7ObserverPolicy.CallbackLossRound7" && ((JArray)persisted["overlapSchedule"]).Count >= 1 &&
                (bool)persisted["overlapSchedule"][0]["sameFrame"] && (bool)persisted["overlapSchedule"].Last["bothPlaying"],
                "The row names C1's round-7 entry point and records the declared same-frame overlap delivery.");
            const string stop = "stop-requested";
            Assertions.Equal(live, Sprint7ObserverPolicy.CallbackLossRound7((JObject)persisted.DeepClone(), stop), "The persisted row receives the live verdict of the same run.");
            Assertions.Equal(live, Sprint7ObserverPolicy.CallbackLossRound7((JObject)LiveCollectorShape(persisted.DeepClone()), stop),
                "The collector-shaped live counterpart of the same row receives the same verdict.");
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
            // R5-F2: an unreadable body withholds positive credit, never a contradiction.
            Func<JObject, JObject> nullBody = row => { row["bodyMaterials"] = JValue.CreateNull(); return row; };
            Assertions.Equal("contradicting", classify(nullBody(Snapshot(new JArray(activeLoop(), Anim(true, false, 4.75, 5, true, true, .4)), driven(), null))), "A null-body row with a cutout animation contradicts.");
            Assertions.Equal("contradicting", classify(nullBody(Snapshot(new JArray(Anim(true, false, 2, 5, true, false, .4)), driven(), null))), "A null-body row with an animation the donor does not configure contradicts.");
            Assertions.Equal("contradicting", classify(nullBody(Snapshot(new JArray(activeLoop()), new JArray(new JObject { ["material"] = 11, ["dissolve"] = .9 }), null))), "A null-body row whose readable driven value disagrees contradicts.");
            foreach (JObject contradiction in new[] { nullBody(Snapshot(new JArray(Anim(true, false, 4.75, 5, true, true, .4)), driven(), null)), nullBody(Snapshot(new JArray(Anim(true, false, 2, 5, true, false, .4)), driven(), null)) })
            {
                JObject five = KmgLoaderCompatHarness.GhaeleAttribution.Evaluate(Enumerable.Repeat(Snapshot(new JArray(activeLoop()), driven(), new JArray(11)), 5).Concat(new[] { contradiction }), donors);
                Assertions.True(!(bool)five["attributed"] && (int)five["contradicting"] == 1, "Five positive rows plus a null-body row with a cutout or unknown animation do not attribute.");
            }
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
                new { Folder = "r4", Name = "20261124T163235Z-owner-0.33.0-s7-ghaele-01-ghaele.json", Body = "recorded" },
                // Round 6: the complete retained round-5 datasets (released v0.0.195 and candidate) under the corrected readers.
                new { Folder = "r5", Name = "20261124T182757Z-owner-0.33.0-s7-ghaele-01-ghaele.json", Body = "recorded" },
                new { Folder = "r5", Name = "20261124T183206Z-owner-0.33.0-s7-ghaele-01-ghaele.json", Body = "recorded" } })
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
                if (retained.Folder != "r3")
                    foreach (JObject light in probe["lighting"].OfType<JObject>())
                    {
                        // Owner amendment 3 (R5-F3): no retained row recorded the subject's component presence (its
                        // faderVisible=true came from the helper's fallback), so the corrected reader cannot establish the
                        // fader; every other readback still qualifies. The recorded in-run value is kept.
                        string why;
                        bool valid = KmgLoaderCompatHarness.GhaeleLighting.Valid(light, out why);
                        var withSubject = (JObject)light.DeepClone(); withSubject["subject"] = KmgLoaderCompatHarness.GhaeleLighting.Subject("retained", 1, 1, true);
                        string otherwise;
                        bool rest = KmgLoaderCompatHarness.GhaeleLighting.Valid(withSubject, out otherwise);
                        Console.WriteLine("GHAELE-LIGHTING-REEVALUATION " + retained.Folder + "/" + retained.Name + " " + light["label"] + " recorded=" + light["valid"] + " corrected=" + (why ?? "valid") + " otherReadbacks=" + (otherwise ?? "valid"));
                        Assertions.True(!valid && why == "fader-presence-unrecorded" && rest, retained.Name + " " + light["label"] + ": only the fader's presence is unestablished (" + why + ", " + otherwise + ").");
                        if (light["valid"] != null) Assertions.True((bool)light["valid"], retained.Name + " " + light["label"] + ": the in-run verdict is kept as recorded.");
                    }
            }
        }

        // R4-F6: a lighting observation counts only with a readable body, a clear
        // cutout, a light-probe reading and a usable capture of that body.
        internal static void GhaeleLightingAndLoanVerdicts()
        {
            Func<JObject> good = () => new JObject { ["label"] = "idle", ["body"] = "GhaeleAzata_R", ["bodyMesh"] = "GhaeleAzata_Mesh", ["bodyDissolveCutout"] = 0.0,
                ["subject"] = KmgLoaderCompatHarness.GhaeleLighting.Subject("ghaele-1", 4401, 4417, true),
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
            // Owner amendment 3 (R5-F3): the subject readback, from the raw component lookup through the producer to the
            // verdict. Missing, present-hidden and present-visible stay distinct; the capture helper's own faderVisible
            // (true when no fader exists) is not used.
            var missing = good(); missing["subject"] = KmgLoaderCompatHarness.GhaeleLighting.Subject("ghaele-1", 4401, null, null);
            Assertions.False(KmgLoaderCompatHarness.GhaeleLighting.Valid(missing, out why) || why != "fader-unavailable", "A live view without an EntityFader is unavailable, although the helper still says faderVisible=true.");
            Assertions.True((bool)missing["subject"]["faderPresent"] == false && missing["subject"]["faderVisible"].Type == JTokenType.Null, "The producer records a missing component as absent, never as visible.");
            var presentHidden = good(); presentHidden["subject"] = KmgLoaderCompatHarness.GhaeleLighting.Subject("ghaele-1", 4401, 4417, false);
            Assertions.False(KmgLoaderCompatHarness.GhaeleLighting.Valid(presentHidden, out why) || why != "fader-hidden", "A present fader reporting hidden fails as hidden.");
            var noView = good(); noView["subject"] = KmgLoaderCompatHarness.GhaeleLighting.Subject("ghaele-1", null, null, null);
            Assertions.False(KmgLoaderCompatHarness.GhaeleLighting.Valid(noView, out why) || why != "fader-unavailable", "No view: unavailable.");
            var unrecorded = good(); unrecorded.Remove("subject");
            Assertions.False(KmgLoaderCompatHarness.GhaeleLighting.Valid(unrecorded, out why) || why != "fader-presence-unrecorded", "A row without the subject readback cannot establish the component.");
            Assertions.True(KmgLoaderCompatHarness.GhaeleLighting.Valid(good(), out why) && why == null, "A present, visible fader with every other readback qualifies.");
            Assertions.True(KmgLoaderCompatHarness.GhaeleLighting.Subject("ghaele-1", 4401, null, true)["faderVisible"].Type == JTokenType.Null, "A visible flag without a component is not carried.");
            string probeText = File.ReadAllText("tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs");
            Assertions.True(probeText.Contains("row[\"subject\"] = GhaeleLighting.Subject(") && probeText.Contains("unit.View.GetComponent<EntityFader>()"),
                "The probe's lighting producer reads the component itself at the observation boundary.");

            // Owner amendment 3 (R5-F1): the witness's own correlation, independent of the cast owner and the command's fields.
            object data = new object(), execution = new object(), otherData = new object(), otherExecution = new object();
            Assertions.Equal("execution", KmgLoaderCompatHarness.GhaeleWitness.Correlated(data, new[] { execution }, execution, null, otherData, null), "A retained execution context correlates.");
            Assertions.Equal("execution", KmgLoaderCompatHarness.GhaeleWitness.Correlated(data, new[] { execution }, otherExecution, execution, otherData, null), "A derived context whose source is retained correlates.");
            Assertions.Equal("ability-data", KmgLoaderCompatHarness.GhaeleWitness.Correlated(data, new object[0], otherExecution, null, data, null), "The exact AbilityData taken from the command correlates after disposal.");
            Assertions.Equal(null, KmgLoaderCompatHarness.GhaeleWitness.Correlated(data, new[] { execution }, otherExecution, null, otherData, otherData), "A foreign execution with another AbilityData does not.");
            Assertions.Equal(null, KmgLoaderCompatHarness.GhaeleWitness.Correlated(null, new object[0], otherExecution, null, null, null), "Nothing watched, nothing owned.");
            Assertions.True(probeText.Contains("lateWitness.OnEventDidTrigger(lateRule)") && probeText.Contains("mark(\"cast-owner-disposed\")") &&
                probeText.IndexOf("lateRun.Dispose()", StringComparison.Ordinal) < probeText.IndexOf("lateWitness.OnEventDidTrigger(lateRule)", StringComparison.Ordinal) &&
                !probeText.Contains("GhaelePending.Verdict(true, true"), "The negative control delivers after the cast owner's disposal and passes no hardcoded verdict input.");

            // R5-F1: the pending abandonment verdict, with the controlled late-owned-result negative case.
            Assertions.Equal(null, KmgLoaderCompatHarness.GhaelePending.Verdict(true, true, false, false, true, 0, 0, false), "Pending at disposal, settled after, foreign kept, nothing late: settled.");
            Assertions.Equal("late-owned-summon", KmgLoaderCompatHarness.GhaelePending.Verdict(true, true, false, false, true, 1, 0, false), "A late owned summon seen by the witness fails.");
            Assertions.Equal("unattributed-same-caster-summon", KmgLoaderCompatHarness.GhaelePending.Verdict(true, true, false, false, true, 0, 1, false), "An unattributed same-caster Ghaele fails.");
            Assertions.Equal("command-not-pending-at-disposal", KmgLoaderCompatHarness.GhaelePending.Verdict(false, true, false, false, true, 0, 0, false), "A command no longer pending at disposal is not the case.");
            Assertions.Equal("loan-not-held-at-disposal", KmgLoaderCompatHarness.GhaelePending.Verdict(true, false, false, false, true, 0, 0, false), "A loan disturbed by the control setup is not the case.");
            Assertions.Equal("witness-still-subscribed", KmgLoaderCompatHarness.GhaelePending.Verdict(true, true, false, false, true, 0, 0, true), "The witness's own subscription must end.");
            Assertions.Equal("foreign-summon-lost", KmgLoaderCompatHarness.GhaelePending.Verdict(true, true, false, false, false, 0, 0, false), "The known foreign summon must survive.");

            // R4-F2 / R5-F4: the combat teardown verdict.
            Assertions.Equal(null, KmgLoaderCompatHarness.GhaeleCombat.Verdict(0, true, true, true, 0, true, true, true), "Own activity released, control preserved, witness survived then settled, native leave.");
            Assertions.Equal("conflict:unowned-engagement", KmgLoaderCompatHarness.GhaeleCombat.Verdict(0, true, true, true, 1, true, true, true), "An engagement the probe does not own is reported, not cleared.");
            Assertions.Equal("independent-control-not-preserved", KmgLoaderCompatHarness.GhaeleCombat.Verdict(0, true, false, false, 0, true, true, true), "The independent engagement must survive the teardown.");
            Assertions.Equal("witness-not-settled", KmgLoaderCompatHarness.GhaeleCombat.Verdict(0, true, true, true, 0, true, false, true), "A witness left running is not a clean teardown.");
            Assertions.Equal("witness-interrupted", KmgLoaderCompatHarness.GhaeleCombat.Verdict(0, true, true, true, 0, false, true, true), "An interrupted witness fails.");
            Assertions.Equal("native-leave-not-observed", KmgLoaderCompatHarness.GhaeleCombat.Verdict(0, true, true, true, 0, true, true, false), "Participants must leave combat natively.");
            string probeSource = File.ReadAllText("tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs");
            Assertions.False(probeSource.Contains("LeaveCombat("), "The Ghaele probe never calls unit-wide LeaveCombat.");

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
                return Life.Release(() => KmgLoaderCompatHarness.ProbeReleases.PerItem("disposable unit", new[] { "unit-1", "unit-2" }, id => id, id =>
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

            // A release whose removal fails (the early-exit cast's, round 6, now in the cell's own ledger).
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

        // R4-F5: native playback query states and the two impact-cue verdicts.
        internal static void ImpactCuePlaybackVerdicts()
        {
            const string playing = Sprint7ObserverPolicy.PlaybackPlaying, inactive = Sprint7ObserverPolicy.PlaybackInactive, unknown = Sprint7ObserverPolicy.PlaybackUnknown;
            Assertions.Equal(playing, Sprint7ObserverPolicy.PlaybackState(true, 497, 3054481153u, false), "An event for the playing ID is playing.");
            Assertions.Equal(inactive, Sprint7ObserverPolicy.PlaybackState(true, 497, 0, false), "An initialized engine answering 0 is confirmed inactive.");
            Assertions.Equal(unknown, Sprint7ObserverPolicy.PlaybackState(false, 497, 0, false), "An uninitialized engine is unknown, never completion.");
            Assertions.Equal(unknown, Sprint7ObserverPolicy.PlaybackState(true, 497, 0, true), "A query that threw is unknown.");
            Assertions.Equal(unknown, Sprint7ObserverPolicy.PlaybackState(true, 0, 0, false), "No playing ID is unknown.");
            Assertions.Equal(null, Sprint7ObserverPolicy.ConcurrentRetirementTrace(playing, playing, 0, true, inactive, false, inactive), "The normal-callback control passes on the per-ID trace.");
            Assertions.Equal("survivor-callback-missing", Sprint7ObserverPolicy.ConcurrentRetirementTrace(playing, playing, 0, false, unknown, true, inactive), "The 164749Z shape (no survivor callback, playback unmeasured) fails.");
            Assertions.Equal("survivor-still-tracked", Sprint7ObserverPolicy.ConcurrentRetirementTrace(playing, playing, 0, true, inactive, true, inactive), "Tracking left after the callback fails.");
            Assertions.Equal("survivor-stopped-by-other-retirement", Sprint7ObserverPolicy.ConcurrentRetirementTrace(playing, playing, 1, true, inactive, false, inactive), "A cross-source stop fails.");
            Assertions.Equal("not-both-playing", Sprint7ObserverPolicy.ConcurrentRetirementTrace(playing, inactive, 0, true, inactive, false, inactive), "Both IDs must be confirmed playing first.");

            // The settled retirement predicate: the product's stop is requested in the retired view's destroy callback.
            DateTime retire = new DateTime(2026, 11, 24, 20, 29, 2, 326, DateTimeKind.Utc);
            Assertions.True(Sprint7ObserverPolicy.RetiredByOwnId(1, "stop-requested", "stop-requested", retire.AddMilliseconds(4), retire.AddMilliseconds(53), retire), "A stop requested just after the call, then the native end, is its own retirement.");
            Assertions.False(Sprint7ObserverPolicy.RetiredByOwnId(0, null, "stop-requested", null, null, retire), "Read in the retirement frame (nothing yet) it is not established.");
            Assertions.False(Sprint7ObserverPolicy.RetiredByOwnId(1, "stop-requested", "stop-requested", retire.AddMilliseconds(4), null, retire), "Without the native end it is a request only.");
            Assertions.False(Sprint7ObserverPolicy.RetiredByOwnId(1, "stop-requested", "stop-requested", retire.AddSeconds(-1), retire.AddMilliseconds(53), retire), "A stop before the retirement is not caused by it.");
            Assertions.False(Sprint7ObserverPolicy.RetiredByOwnId(1, "failed:X", "stop-requested", retire.AddMilliseconds(4), retire.AddMilliseconds(53), retire), "A failed stop is not a retirement.");

            // Recovery C1 (R7-F1, R7-F6): the callback-loss evaluator has two entry points, one per collector generation. A
            // round-7 row is judged by CallbackLossRound7 alone, which reads the retirement time and the settled other record
            // from the row itself; the in-game collector and every reader use it. The round-6 collectors emitted neither, so
            // CallbackLossRound6 takes the phase's end-of-phase record of the other ID and the timeline's retirement mark:
            // a reader over fields those collectors (f3bfab1303 for 200320Z, 740d94a995 for 202648Z) already emitted; it
            // executed no later live-sampling path. Every in-run verdict stays as recorded (history).
            const string stop = "stop-requested";
            Func<string, JObject> phaseOf = name => (JObject)JObject.Parse(File.ReadAllText(Path.Combine("docs/evidence/sprint7/r6", name)))["records"]["summon-s7-effects.json"];
            Func<JObject, JToken> otherOf = phaseRecord => phaseRecord["cuesAtEnd"].OfType<JObject>().First(cue => (uint)cue["playingId"] == (uint)phaseRecord["ankylosaurus"]["callbackLoss"]["before"]["other"]["playingId"]);
            Func<JObject, JToken> markOf = phaseRecord => phaseRecord["timeline"].First(mark => (string)mark["mark"] == "anky-loss-retire-other")["utc"];
            foreach (var retained in new[] {
                new { Name = "20261124T202648Z-owner-0.33.0-s7-fx-01-fx.json", Collector = Sprint7ObserverPolicy.CueCollector.Round6Second, Recorded = "other-not-stopped-by-own-id", Settled = (string)null },
                new { Name = "20261124T200320Z-owner-0.33.0-s7-fx-01-fx.json", Collector = Sprint7ObserverPolicy.CueCollector.Round6First, Recorded = "not-both-playing", Settled = "not-both-playing" } })
            {
                JObject phase = phaseOf(retained.Name);
                JObject row = (JObject)phase["ankylosaurus"]["callbackLoss"];
                Assertions.Equal(retained.Recorded, (string)row["verdict"], retained.Name + ": the in-run verdict is retained as recorded.");
                string settled = Sprint7ObserverPolicy.CallbackLossRound6(row, retained.Collector, otherOf(phase), markOf(phase), stop);
                Console.WriteLine("ANKY-CALLBACK-LOSS-REEVALUATION run=" + retained.Name + " historicalVerdict=" + row["verdict"] + " historicalEvaluator=" + ((string)row["evaluator"] ?? "<not emitted by this collector>") +
                    " currentReader=" + Sprint7ObserverPolicy.Round6Evaluator + "(" + retained.Collector + ") currentResult=" + (settled ?? "characterized"));
                Assertions.Equal(retained.Settled, settled, retained.Name + ": the round-6 reader over the retained row.");
            }
            JObject basePhase = phaseOf("20261124T202648Z-owner-0.33.0-s7-fx-01-fx.json");
            JObject baseRow = (JObject)basePhase["ankylosaurus"]["callbackLoss"];
            Func<Action<JObject, JObject>, string> mutated = change =>
            {
                var row = (JObject)baseRow.DeepClone(); var other = (JObject)otherOf(basePhase).DeepClone();
                change(row, other);
                return Sprint7ObserverPolicy.CallbackLossRound6(row, Sprint7ObserverPolicy.CueCollector.Round6Second, other, markOf(basePhase), stop);
            };
            Assertions.Equal(null, mutated((row, other) => { }), "The unaltered 202648Z copy is characterized.");
            foreach (string terminal in new[] { "survivorAtWindowEnd", "survivorAfterDisposal" })
            {
                string which = terminal == "survivorAtWindowEnd" ? "window-end" : "after-disposal";
                Assertions.Equal("survivor-playback-persisted:" + which, mutated((row, other) => row[terminal]["playback"] = playing), terminal + " playing is persistence.");
                Assertions.Equal("survivor-playback-unknown:" + which, mutated((row, other) => row[terminal]["playback"] = unknown), terminal + " unknown is not completion.");
                Assertions.Equal("survivor-playback-unknown:" + which, mutated((row, other) => ((JObject)row[terminal]).Remove("playback")), terminal + " missing is not completion.");
            }
            Assertions.Equal("survivor-stopped-by-other-retirement", mutated((row, other) => row["afterOtherRetired"]["survivorStopAttempts"] = 1), "R1-F1: a positive retirement-frame survivor count refuses (round 6).");
            Assertions.Equal("not-both-playing", mutated((row, other) => row["before"]["otherPlayback"] = inactive), "Both IDs must be confirmed playing first.");
            Assertions.Equal("stop-without-attempt:phase.cuesAtEnd.other", mutated((row, other) => other["stopAttempts"] = 0), "Stop fields recorded with zero attempts disagree (R3-F2).");
            Assertions.Equal("other-not-stopped-by-own-id", mutated((row, other) => { other["stopAttempts"] = 0; other["stopReason"] = null; other["stopResult"] = null; other["stopRequestedUtc"] = null; }),
                "A consistently never-stopped other source was not retired by its own ID.");
            Assertions.Equal("other-not-stopped-by-own-id", mutated((row, other) => other["endUtc"] = null), "A stop request without the native end is not its own retirement.");
            Assertions.Equal("survivor-stopped-by-other-retirement", mutated((row, other) => row["survivorAtWindowEnd"]["cue"]["stopResult"] = "stop-requested"), "A stop for the survivor during the window fails.");
            Assertions.Equal("callback-not-withheld", mutated((row, other) => row["survivorAtWindowEnd"]["callbacksWithheld"] = new JArray()), "Without a withheld callback the loss was not exercised.");
            Assertions.Equal("callback-not-withheld", mutated((row, other) => row["survivorAtWindowEnd"]["cue"]["endUtc"] = "2026-11-24T20:29:03.0000000Z"), "A product that recorded the end did not lose the callback.");
            Assertions.Equal("survivor-playback-persisted", mutated((row, other) => { foreach (JObject sample in row["window"]) sample["survivorPlayback"] = playing; }), "No inactive sample in the window is persistence.");
            Assertions.Equal("survivor-playback-unknown", mutated((row, other) => { foreach (JObject sample in row["window"]) sample["survivorPlayback"] = unknown; }), "Only unknown samples are not completion.");
            Assertions.Equal("survivor-playback-contradictory", mutated((row, other) => { var again = (JObject)row["window"].Last.DeepClone(); again["survivorPlayback"] = playing; ((JArray)row["window"]).Add(again); }), "Playing again after inactivity contradicts.");
            Assertions.Equal("survivor-tracking-not-source-local", mutated((row, other) => row["survivorAtWindowEnd"]["tracked"] = false), "The residual must be the survivor's own source tracking.");
            Assertions.Equal("source-disposal-not-observed", mutated((row, other) => row["survivorAfterDisposal"]["viewDestroyed"] = false), "A destroy request with disposal pending is insufficient.");
            Assertions.Equal("end-manufactured", mutated((row, other) => row["survivorAfterDisposal"]["cue"]["endUtc"] = "2026-11-24T20:29:15.0000000Z"), "An end the callback never delivered must not appear.");
            Assertions.Equal("unrelated-record-affected", mutated((row, other) => row["survivorAfterDisposal"]["unrelatedRecordsChanged"] = 1), "Another record touched by the survivor's retirement fails.");
            Assertions.Equal("intervention-not-restored", mutated((row, other) => row["interventionHooksAfter"] = 1), "The observer's hook must be gone.");
            Assertions.Equal("intervention-not-restored", mutated((row, other) => row.Remove("interventionHooksAfter")), "An unread hook count is not restoration.");
            Assertions.Equal("intervention-not-restored", mutated((row, other) => row["interventionDisposeFailures"] = new JArray("cue callback hook: InvalidOperationException: x")), "A failed unpatch is not restoration.");
            Assertions.Equal("cue-identity-contradiction:other-retirement-frame", mutated((row, other) => row["afterOtherRetired"]["other"]["playingId"] = (long)row["before"]["survivor"]["playingId"]), "Round 6: the retirement-frame other record is bound too.");
            Assertions.Equal("wrong-entry:round7-row", Sprint7ObserverPolicy.CallbackLossRound6(baseRow, Sprint7ObserverPolicy.CueCollector.Round7, otherOf(basePhase), markOf(basePhase), stop), "A round-7 row has one entry point.");
            // R4-F3: the Round6 phase mark is not the loop's start. With the local start at 20:29:02.3250 and the mark read after it
            // at 20:29:02.3261873, a disposal stop at 20:29:14.3258 follows the real deadline (14.3250) although it precedes
            // mark + window (14.3261873); the reader bounds the deadline only by the later posting + window (14.3142622).
            Func<string, string> between = stopAt =>
            {
                var row = (JObject)baseRow.DeepClone();
                row["survivorAfterDisposal"]["cue"]["stopRequestedUtc"] = stopAt;
                return Sprint7ObserverPolicy.CallbackLossRound6(row, Sprint7ObserverPolicy.CueCollector.Round6Second, otherOf(basePhase), new JValue("2026-11-24T20:29:02.3261873Z"), stop);
            };
            Assertions.Equal(null, between("2026-11-24T20:29:14.3258000Z"), "R4-F3: a Round6 disposal stop between the real deadline and mark + window is accepted.");
            Assertions.Equal("survivor-stop-inside-zero-stop-interval", between("2026-11-24T20:29:14.3000000Z"), "A Round6 stop before the later posting + window still refuses.");

            // Round 7, the final collector (candidate 9c39b1ff7f, package 49f8b88c…) in game: 223809Z found no overlap (the
            // in-run "not-both-playing" stands); 224949Z reached overlap and ran the whole live path. Its in-run verdict,
            // "survivor-stopped-by-other-retirement", is history and stays recorded: candidate 3's evaluator misread the
            // collector's in-memory String-typed nulls (R7-F1). The recovery evaluator reads both representations alike.
            Func<string, JObject> r7 = name => (JObject)JObject.Parse(File.ReadAllText(Path.Combine("docs/evidence/sprint7/r7", name)))["records"]["summon-s7-effects.json"];
            Func<JObject, string> judge7 = row => Sprint7ObserverPolicy.CallbackLossRound7(row, stop);
            foreach (var run in new[] {
                new { Name = "20261124T223809Z-owner-0.33.0-s7-fx-01-fx.json", Recorded = "not-both-playing", Settled = "not-both-playing" },
                new { Name = "20261124T224949Z-owner-0.33.0-s7-fx-01-fx.json", Recorded = "survivor-stopped-by-other-retirement", Settled = (string)null } })
            {
                JObject row = (JObject)r7(run.Name)["ankylosaurus"]["callbackLoss"];
                Assertions.Equal(run.Recorded, (string)row["verdict"], run.Name + ": the in-run verdict is retained as recorded (history).");
                string settled = judge7(row);
                Console.WriteLine("ANKY-CALLBACK-LOSS-REEVALUATION run=" + run.Name + " historicalVerdict=" + row["verdict"] + " historicalEvaluator=" + ((string)row["evaluator"] ?? "<not emitted by this collector>") +
                    " currentReader=" + Sprint7ObserverPolicy.Round7Evaluator + " currentResult=" + (settled ?? "characterized"));
                Assertions.Equal(run.Settled, settled, run.Name + ": the round-7 entry point over the collector's recorded row.");
            }

            // The immutable 224949Z row and its collector-shaped live counterpart, both through CallbackLossRound7, which the
            // in-game collector calls with its in-memory row. The live counterpart is built as the collector builds it in
            // memory: every cue record through the collector's own producer (CueRow, which S7Cue delegates to), every other
            // unset value as the String-typed null JValue a null C# string becomes, times as the strings it writes. It is
            // never serialized. Both must characterize; every negative must refuse, identically, in both.
            JObject persisted = (JObject)r7("20261124T224949Z-owner-0.33.0-s7-fx-01-fx.json")["ankylosaurus"]["callbackLoss"];
            Func<JObject> persistedCopy = () => (JObject)persisted.DeepClone();
            Func<JObject> liveCopy = () => (JObject)LiveCollectorShape(persisted.DeepClone());
            JObject live = liveCopy();
            Assertions.True(live["survivorAtWindowEnd"]["cue"]["stopResult"].Type == JTokenType.String && ((JValue)live["survivorAtWindowEnd"]["cue"]["stopResult"]).Value == null &&
                live["otherRetiredUtc"].Type == JTokenType.String && persisted["survivorAtWindowEnd"]["cue"]["stopResult"].Type == JTokenType.Null && persisted["otherRetiredUtc"].Type == JTokenType.Date,
                "The live counterpart carries the collector's String-typed nulls and string times; the persisted row carries JSON nulls and parsed dates.");
            Assertions.True(Sprint7ObserverPolicy.ExplicitNull(live["survivorAtWindowEnd"]["cue"]["stopResult"]) && Sprint7ObserverPolicy.ExplicitNull(JValue.CreateNull()) &&
                !Sprint7ObserverPolicy.ExplicitNull(null) && !Sprint7ObserverPolicy.ExplicitNull(JValue.CreateUndefined()) && !Sprint7ObserverPolicy.ExplicitNull(new JValue("stop-requested")) &&
                !Sprint7ObserverPolicy.ExplicitNull(new JObject()), "Explicit null: JSON null or a null-valued String JValue; absent, undefined, a value or an object are not.");
            Assertions.Equal(null, judge7(persistedCopy()), "The persisted 224949Z row characterizes.");
            Assertions.Equal(null, judge7(live), "Its collector-shaped live counterpart characterizes identically.");
            long survivorPlaying = (long)persisted["before"]["survivor"]["playingId"], otherPlaying = (long)persisted["before"]["other"]["playingId"];
            long otherSourceOnly = persisted["cuesBeforeSelection"]["otherSource"].Select(cue => (long)cue["playingId"]).First(id => id != otherPlaying);
            foreach (var shape in new[] { new { Name = "persisted", Make = persistedCopy, Null = (Func<JToken>)(() => JValue.CreateNull()) },
                new { Name = "live", Make = liveCopy, Null = (Func<JToken>)(() => new JValue((string)null)) } })
            {
                var current = shape;
                Func<Action<JObject>, string> with = change => { JObject row = current.Make(); change(row); return judge7(row); };
                string n = shape.Name + ": ";
                // Missing required properties are unknown, never an unset value or zero.
                Assertions.Equal("missing:survivorAtWindowEnd.cue.endUtc", with(row => ((JObject)row["survivorAtWindowEnd"]["cue"]).Remove("endUtc")), n + "an omitted endUtc is not an unset callback.");
                Assertions.Equal("missing:survivorAtWindowEnd.cue.stopResult", with(row => ((JObject)row["survivorAtWindowEnd"]["cue"]).Remove("stopResult")), n + "an omitted stop result is not an absent stop.");
                Assertions.Equal("missing:survivorAtWindowEnd.cue.stopRequestedUtc", with(row => ((JObject)row["survivorAtWindowEnd"]["cue"]).Remove("stopRequestedUtc")), n + "an omitted stop request time is unknown.");
                Assertions.Equal("missing:survivorAfterDisposal.cue.endUtc", with(row => ((JObject)row["survivorAfterDisposal"]["cue"]).Remove("endUtc")), n + "an omitted end after disposal is unknown.");
                Assertions.Equal("missing:otherRetirementSettled.survivorStopAttemptsThroughWindow", with(row => ((JObject)row["otherRetirementSettled"]).Remove("survivorStopAttemptsThroughWindow")), n + "an omitted survivor stop count is unknown, not zero.");
                Assertions.Equal("missing:afterOtherRetired.survivorStopAttempts", with(row => ((JObject)row["afterOtherRetired"]).Remove("survivorStopAttempts")), n + "an omitted retirement-frame count is unknown.");
                Assertions.Equal("missing:otherRetirementSettled.other", with(row => ((JObject)row["otherRetirementSettled"]).Remove("other")), n + "no settled other record, no verdict.");
                Assertions.Equal("missing:before.survivor.playingId", with(row => ((JObject)row["before"]["survivor"]).Remove("playingId")), n + "a selected cue without its playing ID is unknown.");
                Assertions.Equal("missing:window[3].survivorPlayback", with(row => ((JObject)row["window"][3]).Remove("survivorPlayback")), n + "R1-F2: an omitted sample playback is unknown, not skipped.");
                Assertions.Equal("missing:window[3].survivorEndUtc", with(row => ((JObject)row["window"][3]).Remove("survivorEndUtc")), n + "an omitted sample end is unknown.");
                // R1-F4: the retirement time and the settled record come from the row, alike for both shapes.
                Assertions.Equal("missing:otherRetiredUtc", with(row => row.Remove("otherRetiredUtc")), n + "R1-F4: an omitted retirement time refuses in either shape.");
                Assertions.Equal("unreadable:otherRetiredUtc", with(row => row["otherRetiredUtc"] = "not a time"), n + "an unreadable retirement time refuses.");
                Assertions.Equal("unreadable:otherRetirementSettled", with(row => row["otherRetirementSettled"] = current.Null()), n + "a null settled record refuses without throwing.");
                Assertions.Equal("unreadable:otherRetirementSettled", with(row => row["otherRetirementSettled"] = 5), n + "a scalar settled record refuses without throwing.");
                // R1-F1: actual stop observations in any pre-disposal snapshot prevent characterization.
                Assertions.Equal("survivor-stopped-by-other-retirement", with(row => row["afterOtherRetired"]["survivorStopAttempts"] = 1), n + "R1-F1: a positive retirement-frame count refuses although the window count is zero.");
                Assertions.Equal("survivor-stopped-by-other-retirement", with(row => row["otherRetirementSettled"]["survivorStopAttemptsThroughWindow"] = 1), n + "a nonzero through-window count refuses.");
                Assertions.Equal("survivor-stopped-by-other-retirement", with(row => row["survivorAtWindowEnd"]["cue"]["stopReason"] = "view-destroyed"), n + "R1-F1: a recorded stop reason before disposal refuses.");
                Assertions.Equal("survivor-stopped-by-other-retirement", with(row => row["before"]["survivor"]["stopReason"] = "view-destroyed"), n + "a stop reason already on the selected cue refuses.");
                Assertions.Equal("survivor-stopped-by-other-retirement", with(row => { row["survivorAtWindowEnd"]["cue"]["stopResult"] = "stop-requested"; row["survivorAtWindowEnd"]["cue"]["stopRequestedUtc"] = "2026-11-24T22:51:57.0000000Z"; }), n + "a recorded stop request refuses.");
                Assertions.Equal("survivor-stopped-by-other-retirement", with(row => row["survivorAtWindowEnd"]["cue"]["stopRequestedUtc"] = "2026-11-24T22:51:57.0000000Z"), n + "a recorded stop request time alone refuses.");
                // R1-F2: callback and end observations in the window.
                Assertions.Equal("callback-not-withheld", with(row => row["window"][1]["survivorEndUtc"] = "2026-11-24T22:51:56.9000000Z"), n + "R1-F2: an end recorded in a window sample refuses although both terminal ends are null.");
                Assertions.Equal("unreadable:survivorAtWindowEnd.callbacksWithheld", with(row => { row["survivorAtWindowEnd"]["callbacksWithheld"] = new JArray(current.Null()); foreach (JObject sample in row["window"]) sample["callbacksWithheld"] = 0; }), n + "R1-F2: a placeholder withheld timestamp refuses.");
                Assertions.True(with(row => { foreach (JObject sample in row["window"]) sample["callbacksWithheld"] = 0; }).StartsWith("callback-count-contradiction:window[", StringComparison.Ordinal), n + "a callback timed before later samples whose counts stay zero refuses.");
                Assertions.Equal("callback-not-withheld", with(row => row["survivorAtWindowEnd"]["cue"]["endUtc"] = "2026-11-24T22:51:56.7000000Z"), n + "a received completion callback (end recorded) refuses.");
                Assertions.Equal("end-manufactured", with(row => row["survivorAfterDisposal"]["cue"]["endUtc"] = "2026-11-24T22:52:08.5000000Z"), n + "an end after disposal refuses.");
                // R1-F3 / R7-F6: substituted or contradictory cue identities.
                Assertions.Equal("cue-identity-contradiction:other-retirement-frame", with(row => row["afterOtherRetired"]["other"]["playingId"] = survivorPlaying), n + "R1-F3: the retirement-frame other cue substituted with the survivor refuses.");
                Assertions.Equal("cue-identity-contradiction:survivor-source", with(row => { row["before"]["survivor"]["playingId"] = otherSourceOnly; row["survivorAtWindowEnd"]["cue"]["playingId"] = otherSourceOnly; row["survivorAfterDisposal"]["cue"]["playingId"] = otherSourceOnly; }),
                    n + "R1-F3: a consistent survivor substitution that the recorded source inventory contradicts refuses.");
                Assertions.Equal("cue-identity-contradiction:survivor-window-end", with(row => row["survivorAtWindowEnd"]["cue"]["playingId"] = 999), n + "a substituted window-end cue refuses.");
                Assertions.Equal("cue-identity-contradiction:survivor-after-disposal", with(row => row["survivorAfterDisposal"]["cue"]["playingId"] = 999), n + "a substituted disposal cue refuses.");
                Assertions.Equal("cue-identity-contradiction:other-settled", with(row => row["otherRetirementSettled"]["other"]["playingId"] = 999), n + "a substituted settled other record refuses.");
                Assertions.Equal("cue-identity-contradiction:survivor-is-other", with(row => row["before"]["other"]["playingId"] = survivorPlaying), n + "the same ID selected twice refuses.");
                Assertions.Equal("unreadable:before.other.playingId", with(row => row["before"]["other"]["playingId"] = 0), n + "an invalid playing ID is not an identity.");
                // R1-F5: undefined and wrong-type stop or end fields are unreadable, never an unset value or an observation.
                Assertions.Equal("unreadable:survivorAfterDisposal.cue.endUtc", with(row => row["survivorAfterDisposal"]["cue"]["endUtc"] = JValue.CreateUndefined()), n + "R1-F5: an undefined end is unreadable.");
                Assertions.Equal("unreadable:survivorAtWindowEnd.cue.endUtc", with(row => row["survivorAtWindowEnd"]["cue"]["endUtc"] = new JObject()), n + "an object in an end field is unreadable, not a recorded end.");
                Assertions.Equal("unreadable:survivorAtWindowEnd.cue.endUtc", with(row => row["survivorAtWindowEnd"]["cue"]["endUtc"] = "not a time"), n + "an unparsable end is unreadable.");
                Assertions.Equal("unreadable:survivorAtWindowEnd.cue.stopResult", with(row => row["survivorAtWindowEnd"]["cue"]["stopResult"] = new JArray()), n + "an array in a stop field is unreadable.");
                Assertions.Equal("unreadable:survivorAtWindowEnd.cue.stopReason", with(row => row["survivorAtWindowEnd"]["cue"]["stopReason"] = JValue.CreateUndefined()), n + "an undefined stop reason is unreadable.");
                // R1-F6: out-of-range integers are unreadable, never an exception.
                Assertions.Equal("unreadable:otherRetirementSettled.survivorStopAttemptsThroughWindow", with(row => row["otherRetirementSettled"]["survivorStopAttemptsThroughWindow"] = 2147483648L), n + "R1-F6: a count above Int32.MaxValue is unreadable.");
                Assertions.Equal("unreadable:before.survivor.playingId", with(row => row["before"]["survivor"]["playingId"] = new JValue(ulong.MaxValue)), n + "R1-F6: a playing ID above Int64.MaxValue is unreadable.");
                Assertions.Equal("unreadable:survivorAtWindowEnd.cue.playingId", with(row => row["survivorAtWindowEnd"]["cue"]["playingId"] = new JValue(ulong.MaxValue)), n + "an oversized terminal ID is unreadable, without throwing.");
                // C1 amendment 1 (R2-F1): selection is bound to the posting, not to a bare ID.
                Assertions.Equal("cue-identity-contradiction:survivor-source", with(row => { row["before"]["survivor"]["playingId"] = 513; row["survivorAtWindowEnd"]["cue"]["playingId"] = 513; row["survivorAfterDisposal"]["cue"]["playingId"] = 513; }),
                    n + "R2-F1: survivor 586 replaced by the already-ended same-source cue 513 (keeping 586's posting) refuses.");
                Assertions.Equal("cue-identity-contradiction:other-source", with(row => { row["before"]["other"]["playingId"] = 539; row["afterOtherRetired"]["other"]["playingId"] = 539; row["otherRetirementSettled"]["other"]["playingId"] = 539; }),
                    n + "R2-F1: other 622 replaced by the earlier same-source cue 539 (keeping 622's posting) refuses.");
                Assertions.Equal("cue-identity-contradiction:survivor-window-end", with(row => { JToken earlier = row["cuesBeforeSelection"]["survivorSource"].First(cue => (long)cue["playingId"] == 513).DeepClone(); row["before"]["survivor"] = earlier;
                    row["survivorAtWindowEnd"]["cue"]["playingId"] = 513; row["survivorAfterDisposal"]["cue"]["playingId"] = 513; }), n + "the whole earlier record selected instead does not match the later snapshots' posting.");
                Assertions.Equal("cue-identity-ambiguous:cuesBeforeSelection.survivorSource", with(row => ((JArray)row["cuesBeforeSelection"]["survivorSource"]).Add(row["cuesBeforeSelection"]["survivorSource"].First(cue => (long)cue["playingId"] == survivorPlaying).DeepClone())),
                    n + "two inventory records with the selected ID are ambiguous.");
                // R2-F2: withheld callbacks at the times the collector observed them.
                Assertions.Equal(null, with(row => { foreach (JObject sample in row["window"]) sample["callbacksWithheld"] = 0; row["survivorAtWindowEnd"]["callbacksWithheld"] = new JArray("2026-11-24T22:52:08.3500000Z"); }),
                    n + "R2-F2: a callback after the last periodic sample, first seen in the terminal list, is accepted.");
                Assertions.Equal("callback-predates-cue", with(row => row["survivorAtWindowEnd"]["callbacksWithheld"] = new JArray("2026-11-24T22:51:54.0000000Z")), n + "R2-F2: a callback dated before its selected cue was posted refuses.");
                Assertions.Equal("callback-count-contradiction:window[0]", with(row => { row["survivorAtWindowEnd"]["callbacksWithheld"] = new JArray("2026-11-24T22:51:58.0000000Z"); foreach (JObject sample in row["window"]) sample["callbacksWithheld"] = 1; }),
                    n + "a sample counting a callback first timed after the next sample's time refuses.");
                Assertions.Equal("callback-count-exceeds-terminal", with(row => ((JObject)row["window"].Last)["callbacksWithheld"] = 2), n + "a periodic count above the later terminal total refuses.");
                Assertions.Equal("callback-count-decreased", with(row => ((JObject)row["window"][10])["callbacksWithheld"] = 0), n + "a cumulative count that falls refuses.");
                Assertions.Equal("unreadable:window[2].callbacksWithheld", with(row => ((JObject)row["window"][2])["callbacksWithheld"] = -1), n + "a negative sample count is unreadable.");
                // R2-F3: earlier and later lifecycle observations of the same posting.
                Assertions.Equal("lifecycle-contradiction:other.endUtc", with(row => row["before"]["other"]["endUtc"] = "2026-11-24T22:51:56.4200000Z"),
                    n + "R2-F3: an earlier recorded completion of the other cue cannot be erased by the favorable later records.");
                Assertions.True(with(row => { foreach (JToken cue in new[] { row["before"]["other"], row["afterOtherRetired"]["other"], row["otherRetirementSettled"]["other"] }) cue["endUtc"] = "2026-11-24T22:51:56.4200000Z"; }) != null,
                    n + "a completion recorded before the retirement cannot be credited to the retirement (refused; the label is diagnostic only under C1 amendment 2).");
                Assertions.Equal("lifecycle-contradiction:other.stopAttempts-decreased", with(row => row["afterOtherRetired"]["otherStopAttempts"] = 2), n + "R2-F3: a retirement-frame stop count above the settled count refuses.");
                Assertions.Equal("survivor-stop-inside-zero-stop-interval", with(row => row["survivorAfterDisposal"]["cue"]["stopRequestedUtc"] = "2026-11-24T22:51:56.4333170Z"),
                    n + "R2-F3: a survivor stop timed inside the interval reported to have zero stops refuses.");
                Assertions.Equal(null, with(row => { }), n + "the actual disposal stop (22:52:08.4634895Z, after the last sample) stays valid.");
                // R2-F4: the producer's failed post (playing ID 0) owns nothing and spoils nothing.
                Assertions.Equal(null, with(row => ((JArray)row["cuesBeforeSelection"]["survivorSource"]).Insert(0, current.Name == "live"
                    ? Sprint7ObserverPolicy.CueRow("Sweetener_Low_06_Quake", 0, 5500, new DateTime(2026, 11, 24, 22, 51, 48, DateTimeKind.Utc), null, null, null, null)
                    : JObject.Parse(Sprint7ObserverPolicy.CueRow("Sweetener_Low_06_Quake", 0, 5500, new DateTime(2026, 11, 24, 22, 51, 48, DateTimeKind.Utc), null, null, null, null).ToString()))),
                    n + "R2-F4: an unselected failed post with playing ID 0 does not invalidate the selected pair.");
                Assertions.Equal("unreadable:before.survivor.playingId", with(row => row["before"]["survivor"]["playingId"] = 0), n + "selecting playing ID 0 refuses.");
                Assertions.Equal("unreadable:cuesBeforeSelection.survivorSource[0].playingId", with(row => row["cuesBeforeSelection"]["survivorSource"][0]["playingId"] = -3), n + "any other malformed inventory record still makes the inventory unreadable.");
                // R2-F5: counts are non-negative and in range; unknown is never an observed stop.
                foreach (string field in new[] { "afterOtherRetired.survivorStopAttempts", "otherRetirementSettled.survivorStopAttemptsThroughWindow" })
                {
                    string[] at = field.Split('.');
                    Assertions.Equal(null, with(row => row[at[0]][at[1]] = 0), n + field + " zero: no stop.");
                    Assertions.Equal("survivor-stopped-by-other-retirement", with(row => row[at[0]][at[1]] = 1), n + field + " positive: an observed stop.");
                    Assertions.Equal("unreadable:" + field, with(row => row[at[0]][at[1]] = -1), n + field + " negative: unreadable, not a stop.");
                    Assertions.Equal("unreadable:" + field, with(row => row[at[0]][at[1]] = 2147483648L), n + field + " oversized: unreadable.");
                }
                // C1 round 3 (R2-F3): the zero-stop interval reaches at least retirement + declared window (22:52:08.4293166Z).
                Assertions.Equal("survivor-stop-inside-zero-stop-interval", with(row => row["survivorAfterDisposal"]["cue"]["stopRequestedUtc"] = "2026-11-24T22:52:08.3500000Z"),
                    n + "R2-F3: a disposal stop after the last sample but inside the declared window refuses.");
                Assertions.Equal(null, with(row => { foreach (JObject sample in row["window"]) sample["callbacksWithheld"] = 0; row["survivorAtWindowEnd"]["callbacksWithheld"] = new JArray("2026-11-24T22:52:08.3500000Z"); }),
                    n + "a withheld callback at 22:52:08.35 with zero periodic counts is still accepted.");
                // R3-F1: callback attribution bounded by arming and by the terminal capture; retirement after both postings.
                Assertions.Equal("callback-after-terminal-capture", with(row => { foreach (JObject sample in row["window"]) sample["callbacksWithheld"] = 0; row["survivorAtWindowEnd"]["callbacksWithheld"] = new JArray("2026-11-24T22:52:08.5000000Z"); }),
                    n + "R3-F1: a callback after the recorded disposal stop (so after the terminal list was copied) refuses.");
                Assertions.Equal("callback-predates-arming", with(row => { foreach (JObject sample in row["window"]) sample["callbacksWithheld"] = 1; row["survivorAtWindowEnd"]["callbacksWithheld"] = new JArray("2026-11-24T22:51:55.0000000Z"); }),
                    n + "R3-F1: a callback before the other cue 622 was posted (before arming) refuses.");
                Assertions.Equal("retirement-before-posting", with(row => row["otherRetiredUtc"] = "2026-11-24T22:51:56.0000000Z"), n + "R3-F1: a retirement time before cue 622 was posted refuses.");
                Assertions.Equal("window-before-retirement", with(row => row["window"][0]["utc"] = "2026-11-24T22:51:56.0000000Z"), n + "a window sample before the retirement refuses.");
                // R3-F2: stop fields agree with the attempt count read with them.
                Assertions.Equal("stop-without-attempt:survivorAfterDisposal.cue", with(row => row["survivorAfterDisposal"]["stopAttempts"] = 0), n + "R3-F2: a successful disposal request with zero attempts refuses.");
                Assertions.Equal("stop-without-attempt:afterOtherRetired.other", with(row => { row["afterOtherRetired"]["other"]["stopReason"] = "view-destroyed"; row["afterOtherRetired"]["other"]["stopResult"] = "stop-requested";
                    row["afterOtherRetired"]["other"]["stopRequestedUtc"] = row["otherRetirementSettled"]["other"]["stopRequestedUtc"].DeepClone(); }), n + "R3-F2: a retirement-frame stop already recorded with zero attempts refuses.");
                Assertions.Equal("stop-fields-inconsistent:afterOtherRetired.other", with(row => row["afterOtherRetired"]["other"]["stopResult"] = "stop-requested"), n + "a stop result without its reason and time refuses.");
                Assertions.Equal("stop-fields-inconsistent:survivorAfterDisposal.cue", with(row => row["survivorAfterDisposal"]["cue"]["stopResult"] = "failed:InvalidOperationException:x"), n + "a failed result beside a request time refuses.");
                Assertions.Equal("stop-attempts-impossible:survivorAfterDisposal.cue", with(row => row["survivorAfterDisposal"]["stopAttempts"] = 2), n + "more than the producer's one attempt refuses.");
                Assertions.Equal("other-not-stopped-by-own-id", with(row => { row["otherRetirementSettled"]["other"]["stopResult"] = "failed:InvalidOperationException:x"; row["otherRetirementSettled"]["other"]["stopRequestedUtc"] = current.Null(); }),
                    n + "the producer's failed stop (attempt and failure, no request time) is consistent but not a retirement by its own ID.");
                // R3-F3: a time without a recorded date or zone is unreadable, never the replay machine's.
                Assertions.Equal("unreadable:survivorAfterDisposal.cue.stopRequestedUtc", with(row => row["survivorAfterDisposal"]["cue"]["stopRequestedUtc"] = "23:59:00"), n + "R3-F3: a time-only stop is unreadable.");
                Assertions.Equal("unreadable:survivorAfterDisposal.cue.stopRequestedUtc", with(row => row["survivorAfterDisposal"]["cue"]["stopRequestedUtc"] = "2026-11-24T22:52:08.4634895"), n + "a stop without a zone is unreadable.");
                // R3-F4: positive inventory records must be readable postings; the selected event must have a name.
                Assertions.Equal("missing:cuesBeforeSelection.survivorSource[0].event", with(row => row["cuesBeforeSelection"]["survivorSource"][0] = new JObject { ["playingId"] = 513 }), n + "R3-F4: an unselected positive record without a posting refuses.");
                Assertions.Equal("unreadable:before.survivor.event", with(row => { row["before"]["survivor"]["event"] = ""; row["survivorAtWindowEnd"]["cue"]["event"] = ""; row["survivorAfterDisposal"]["cue"]["event"] = "";
                    row["cuesBeforeSelection"]["survivorSource"].First(cue => (long)cue["playingId"] == survivorPlaying)["event"] = ""; }), n + "R3-F4: an empty event name is not a posting.");
                // C1 amendment 2 (round-4 findings).
                // R4-F1: a completion seen with zero attempts locks the count (Retire skips a completed posting); a retirement-frame
                // time strictly after the first sample is impossible; a stop already attempted and then completed stays valid.
                Assertions.Equal("lifecycle-contradiction:other.stop-after-completion", with(row => row["afterOtherRetired"]["other"]["endUtc"] = row["otherRetirementSettled"]["other"]["endUtc"].DeepClone()),
                    n + "R4-F1: an end recorded at zero attempts followed by a settled first stop refuses.");
                Assertions.Equal("retirement-frame-after-first-sample:stopRequestedUtc", with(row => { var frame = (JObject)row["afterOtherRetired"]; var settled = (JObject)row["otherRetirementSettled"]["other"];
                    foreach (string field in new[] { "stopReason", "stopResult", "stopRequestedUtc" }) frame["other"][field] = settled[field].DeepClone(); frame["otherStopAttempts"] = 1; }),
                    n + "R4-F1: a complete retirement-frame stop timed after the first sample (22:51:56.4303174Z) refuses.");
                Assertions.Equal(null, with(row => { var frame = (JObject)row["afterOtherRetired"]; var settled = (JObject)row["otherRetirementSettled"]["other"]; JToken tick = row["window"][0]["utc"].DeepClone();
                    settled["stopRequestedUtc"] = tick.DeepClone(); foreach (string field in new[] { "stopReason", "stopResult", "stopRequestedUtc" }) frame["other"][field] = settled[field].DeepClone(); frame["otherStopAttempts"] = 1; }),
                    n + "a stop already attempted in the retirement frame at the first sample's own tick, completed later, stays valid.");
                // R4-F2: no further sample after one stamped at or past the recorded deadline (22:52:08.4293166Z).
                Assertions.Equal("window-sample-after-deadline:window[" + (((JArray)current.Make()["window"]).Count - 1) + "]", with(row => { var window = (JArray)row["window"];
                    window[window.Count - 2]["utc"] = "2026-11-24T22:52:08.4300000Z"; window[window.Count - 1]["utc"] = "2026-11-24T22:52:08.4500000Z"; }),
                    n + "R4-F2: two samples past the deadline (another iteration after an expired one) refuse.");
                Assertions.Equal(null, with(row => { var window = (JArray)row["window"]; window[window.Count - 1]["utc"] = "2026-11-24T22:52:08.4500000Z"; }), n + "a final sample straddling its own guard stays valid.");
                // R4-F4: equal readings in the known order are not inversions.
                Assertions.Equal(null, with(row => { foreach (JObject sample in row["window"]) sample["callbacksWithheld"] = 0; row["survivorAtWindowEnd"]["callbacksWithheld"] = new JArray("2026-11-24T22:52:08.4634895Z"); }),
                    n + "R4-F4: a terminal callback sharing the disposal stop's clock tick is accepted.");
                Assertions.Equal(null, with(row => row["survivorAfterDisposal"]["cue"]["stopRequestedUtc"] = "2026-11-24T22:52:08.4293166Z"), n + "a disposal stop on the deadline's own tick is accepted.");
                // R4-F5: more than the one attempt per posting is impossible wherever it is read (fixed, not rebutted).
                Assertions.Equal("stop-attempts-impossible:afterOtherRetired.survivorStopAttempts", with(row => row["afterOtherRetired"]["survivorStopAttempts"] = 2), n + "R4-F5: a retirement-frame count of two is impossible.");
                Assertions.Equal("stop-attempts-impossible:otherRetirementSettled.survivorStopAttemptsThroughWindow", with(row => row["otherRetirementSettled"]["survivorStopAttemptsThroughWindow"] = 2), n + "R4-F5: a through-window count of two is impossible.");
                // R4-F6: an omitted or malformed settled end is unavailable, distinct from a recorded null (fixed, not rebutted).
                Assertions.Equal("missing:otherRetirementSettled.other.endUtc", with(row => ((JObject)row["otherRetirementSettled"]["other"]).Remove("endUtc")), n + "R4-F6: an omitted settled end is missing.");
                Assertions.Equal("unreadable:otherRetirementSettled.other.endUtc", with(row => row["otherRetirementSettled"]["other"]["endUtc"] = "not a date"), n + "R4-F6: a malformed settled end is unreadable.");
                Assertions.Equal("other-not-stopped-by-own-id", with(row => row["otherRetirementSettled"]["other"]["endUtc"] = current.Null()), n + "R4-F6: a recorded null means no completion was recorded.");
                // R4-F7: a successful request is credited to the retirement only with the production cause.
                Assertions.Equal("other-stop-cause-not-retirement", with(row => row["otherRetirementSettled"]["other"]["stopReason"] = ""), n + "R4-F7: an empty cause refuses.");
                Assertions.Equal("other-stop-cause-not-retirement", with(row => row["otherRetirementSettled"]["other"]["stopReason"] = "manual-preview-stop"), n + "R4-F7: an unrelated cause refuses.");
                Assertions.Equal(null, with(row => row["otherRetirementSettled"]["other"]["stopReason"] = "entity-retired"), n + "the other production retirement cause (entity-retired) is valid.");
                Assertions.Equal("survivor-stop-cause-not-retirement", with(row => row["survivorAfterDisposal"]["cue"]["stopReason"] = "manual-preview-stop"), n + "the survivor's post-window stop needs its own retirement cause too.");
                // The accepted terminal playback rules stay.
                Assertions.Equal("survivor-playback-persisted:window-end", with(row => row["survivorAtWindowEnd"]["playback"] = Sprint7ObserverPolicy.PlaybackPlaying), n + "terminal playing is persistence.");
                Assertions.Equal("survivor-playback-unknown:after-disposal", with(row => ((JObject)row["survivorAfterDisposal"]).Remove("playback")), n + "a missing terminal read is unknown.");
                // The survivor's own disposal may request its stop (preserved).
                Assertions.Equal(null, with(row => { row["survivorAfterDisposal"]["cue"]["stopReason"] = "view-destroyed"; row["survivorAfterDisposal"]["cue"]["stopResult"] = "stop-requested"; }), n + "the stop requested by the survivor's own disposal is legitimate.");
                // A stop request is not completion: the other source's native end is required separately.
                Assertions.Equal("other-not-stopped-by-own-id", with(row => row["otherRetirementSettled"]["other"]["endUtc"] = current.Null()), n + "the other source's stop request without its native end is not its retirement.");
            }
            // R3-F3: complete collector timestamps and their reparsed date values are readable; no date or no zone is not.
            Assertions.True(Sprint7ObserverPolicy.Utc(new JValue("2026-11-24T22:52:08.4634895Z")) == new DateTime(2026, 11, 24, 22, 52, 8, DateTimeKind.Utc).AddTicks(4634895) &&
                Sprint7ObserverPolicy.Utc(JToken.Parse("\"2026-11-24T22:52:08.4634895Z\"")) == new DateTime(2026, 11, 24, 22, 52, 8, DateTimeKind.Utc).AddTicks(4634895) &&
                Sprint7ObserverPolicy.Utc(new JValue("23:59:00")) == null && Sprint7ObserverPolicy.Utc(new JValue("2026-11-24T22:52:08.4634895")) == null &&
                Sprint7ObserverPolicy.Utc(new JValue(new DateTime(2026, 11, 24, 22, 52, 8, DateTimeKind.Unspecified))) == null, "Only complete instants are readable.");
            // R2-F6: a newly produced row names the entry point it actually invokes; the retained row keeps its original name.
            Assertions.Equal("Sprint7ObserverPolicy.CallbackLossRound7", (string)Sprint7ObserverPolicy.NewCallbackLossRow(12, 6, 16)["evaluator"], "A new callback-loss row identifies CallbackLossRound7.");
            Assertions.Equal(Sprint7ObserverPolicy.Round7Evaluator, (string)Sprint7ObserverPolicy.NewCallbackLossRow(12, 6, 16)["evaluator"], "The identity is language-bound (nameof).");
            Assertions.Equal("Sprint7ObserverPolicy.CallbackLossSettled", (string)persisted["evaluator"], "The retained 224949Z row keeps its original evaluator identity (history).");
            Assertions.Equal("not-both-playing", Sprint7ObserverPolicy.CallbackLossRound7(new JObject(), stop), "An empty row is not characterized and does not throw.");
            Assertions.Equal("missing:row", Sprint7ObserverPolicy.CallbackLossRound7(null, stop), "No row, no verdict.");
            string collector = File.ReadAllText("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Sprint7Presentation.cs");
            Assertions.True(collector.Contains("Sprint7ObserverPolicy.CallbackLossRound7(row, SummonCastCueLifecycle.StopRequested)") && collector.Contains("Sprint7ObserverPolicy.CueRow(") &&
                collector.Contains("JObject row = Sprint7ObserverPolicy.NewCallbackLossRow(WindowSeconds, DisposalSeconds, OverlapSeconds);") && !collector.Contains("\"Sprint7ObserverPolicy.CallbackLossSettled\"") &&
                !collector.Contains("CallbackLossSettled(") && !collector.Contains("CallbackLossVerdict("),
                "The in-game collector judges its own in-memory row through the same round-7 entry point the readers use.");
        }

        // Recovery C1: a collector-shaped in-memory counterpart of a recorded callback-loss row. Each cue record is rebuilt
        // through the collector's own producer (CueRow); its extra fields are copied; every other JSON null becomes the
        // String-typed null JValue a null C# string becomes in memory. Nothing is serialized.
        private static JToken LiveCollectorShape(JToken token)
        {
            var obj = token as JObject;
            if (obj != null)
            {
                if (obj.Property("playingId") != null && obj.Property("event") != null && obj.Property("utc") != null)
                {
                    JObject cue = Sprint7ObserverPolicy.CueRow((string)obj["event"], (uint)obj["playingId"], (int)obj["frame"], Sprint7ObserverPolicy.Utc(obj["utc"]).Value,
                        Sprint7ObserverPolicy.Utc(obj["endUtc"]), Sprint7ObserverPolicy.ExplicitNull(obj["stopReason"]) ? null : (string)obj["stopReason"],
                        Sprint7ObserverPolicy.Utc(obj["stopRequestedUtc"]), Sprint7ObserverPolicy.ExplicitNull(obj["stopResult"]) ? null : (string)obj["stopResult"]);
                    foreach (JProperty extra in obj.Properties().Where(property => cue.Property(property.Name) == null).ToList()) cue[extra.Name] = LiveCollectorShape(extra.Value);
                    return cue;
                }
                var copy = new JObject();
                foreach (JProperty property in obj.Properties()) copy[property.Name] = LiveCollectorShape(property.Value);
                return copy;
            }
            var array = token as JArray;
            if (array != null) return new JArray(array.Select(LiveCollectorShape));
            if (token != null && token.Type == JTokenType.Null) return new JValue((string)null);
            if (token != null && token.Type == JTokenType.Date) return new JValue(((DateTime)token).ToUniversalTime().ToString("o"));
            return token == null ? null : token.DeepClone();
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
