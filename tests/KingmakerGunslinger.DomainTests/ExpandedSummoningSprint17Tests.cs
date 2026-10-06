using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint17Tests
    {
        internal static void NativeSpearBoundsRetainConservativeUncertainty()
        {
            Assertions.Equal((float?).25f, SerpentineRigSurveyPolicy.ConservativeSpearEndGap(.125f, .125f),
                "The complete transverse uncertainty is added, never subtracted to pass contact.");
            Assertions.Equal((float?)0, SerpentineRigSurveyPolicy.ConservativeSpearEndGap(0, 0),
                "An exact zero is not confused with an unknown measurement.");
            Assertions.Equal((float?)4.5f, SerpentineRigSurveyPolicy.ConservativeSpearEndGap(4, .5f),
                "Research retains a large miss for diagnosis; it does not clamp to the acceptance limit.");
            foreach (float invalid in new[] { -.01f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                Assertions.False(SerpentineRigSurveyPolicy.ConservativeSpearEndGap(invalid, .1f).HasValue,
                    "Missing, negative or nonfinite end-centre evidence is rejected.");
                Assertions.False(SerpentineRigSurveyPolicy.ConservativeSpearEndGap(.1f, invalid).HasValue,
                    "Missing, negative or nonfinite uncertainty is rejected.");
            }
            Assertions.False(SerpentineRigSurveyPolicy.ConservativeSpearEndGap(float.MaxValue, float.MaxValue).HasValue,
                "Overflow cannot become apparently valid geometry.");
        }

        internal static void ContactResearchRequiresIssuedOwnedMeasuredEvents()
        {
            foreach (float gap in new[] { 0f, .25f, 8f })
                Assertions.True(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, true, false, true, 7, gap),
                    "Research retains a measured miss instead of pretending only good contact exists.");
            Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(false, true, false, true, 7, 0),
                "An unrelated actor or target cannot satisfy the requested command.");
            Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, false, false, true, 7, 0),
                "A queued, rejected or incidental event is not an executing issued attack.");
            Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, true, true, true, 7, 0),
                "An opportunity attack cannot satisfy the manual command.");
            Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, true, false, false, 7, 0),
                "The native animation contact boundary must be observed.");
            foreach (int count in new[] { -1, 0 })
                Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, true, false, true, count, 0),
                    "An unmeasured donor attack is not a required contact.");
            foreach (float gap in new[] { -.1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assertions.False(SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(true, true, false, true, 7, gap),
                    "Unknown or invalid geometry cannot satisfy research.");
        }

        internal static void NativeSpearResearchRejectsEveryChangedIdentity()
        {
            string[] exact = { "salamander", SerpentineVisualPolicy.TwoHandPrefab,
                SerpentineVisualPolicy.ProjectSpear, "Spear", "TH_SpearArmy", "WP_SpearArmy", "WeaponPivot", "R_Palm" };
            Func<string[], bool> permits = row => SerpentineVisualPolicy.PermitsNativeSpearResearch(
                row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7]);
            Assertions.True(permits(exact), "Only the measured project spear and exact two-hand palm seam may attach.");
            for (int index = 0; index < exact.Length; index++)
                foreach (string replacement in new[] { null, "", "foreign", exact[index].ToUpperInvariant(),
                    SerpentineVisualPolicy.ClubShieldPrefab, "Greatclub", "ShieldPivot", "L_Palm", "viper" })
                {
                    if (replacement == exact[index]) continue;
                    string[] changed = (string[])exact.Clone(); changed[index] = replacement;
                    Assertions.False(permits(changed), "A single substituted identity fails closed; no other weapon, view or anchor.");
                }
        }

        internal static void OriginalTriangleDiagnosticIsExactReversibleAndInputPreserving()
        {
            int[] source = { 0, 1, 2, 0, 2, 3 };
            int[] result = SerpentineRigSurveyPolicy.ReverseOriginalTriangleOrder(source, 4);
            Assertions.True(source.SequenceEqual(new[] { 0, 1, 2, 0, 2, 3 }), "Caller-owned original indices never mutate.");
            Assertions.True(result.SequenceEqual(new[] { 0, 2, 1, 0, 3, 2 }), "Exactly the second/third slots swap per triangle.");
            Assertions.True(SerpentineRigSurveyPolicy.ReverseOriginalTriangleOrder(result, 4).SequenceEqual(source),
                "Two swaps restore original order exactly, not an approximate mesh rewrite.");
            foreach (int[] invalid in new[] { null, new int[0], new[] { 0, 1 }, new[] { -1, 1, 2 },
                new[] { 0, 1, 4 }, new[] { 0, 0, 1 }, new[] { 0, 1, 0 }, new[] { 0, 1, 1 } })
            {
                bool rejected = false;
                try { SerpentineRigSurveyPolicy.ReverseOriginalTriangleOrder(invalid, 4); }
                catch (ArgumentException) { rejected = true; }
                Assertions.True(rejected, "Incomplete, out-of-range and degenerate lists fail closed.");
            }
            foreach (int count in new[] { -1, 0, 1, 2 })
            {
                bool rejected = false;
                try { SerpentineRigSurveyPolicy.ReverseOriginalTriangleOrder(new[] { 0, 1, 2 }, count); }
                catch (ArgumentException) { rejected = true; }
                Assertions.True(rejected, "A body must have at least three addressable vertices.");
            }
        }

        internal static void GroundResearchRequiresMeasuredSurfaceAndRetainsPenetration()
        {
            Assertions.Equal((float?)(-.125f), SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                -6.125f, true, -6f, 1f, false), "Penetration is not clamped or compared to an actor/nav origin.");
            Assertions.Equal((float?).375f, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                17.375f, true, 17f, 1f, false), "A positive gap remains a gap, not automatic ground contact.");
            Assertions.Equal((float?)(-.125f), SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                13.875f, true, 14f, .2f, false), "World translation does not change measured clearance.");
            Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                0, false, 0, 1, false), "A missed ray is unknown, never zero clearance.");
            Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                0, true, 0, 1, true), "The owned actor's collider cannot stand in for terrain.");
            foreach (float normal in new[] { -.2f, 0, .199f, 1.1f, float.NaN, float.PositiveInfinity })
                Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                    0, true, 0, normal, false), "Only a finite upward-facing measured surface is usable.");
            foreach (float bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                    bad, true, 0, 1, false), "Nonfinite original geometry cannot produce ground evidence.");
                Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                    0, true, bad, 1, false), "Nonfinite hit geometry cannot produce ground evidence.");
            }
            Assertions.Equal((float?)null, SerpentineRigSurveyPolicy.MeasuredGroundClearance(
                float.MaxValue, true, -float.MaxValue, 1, false), "Overflow is unknown, not a usable measurement.");
        }

        internal static void BodyResearchIsClosedAndUsesWorkingSaveGuard()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSerpentineBodies;
            Assertions.Equal("disposable-expanded-summoning-serpentine-bodies", scenario,
                "One closed research request, not a general runtime asset loader.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario) &&
                RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(scenario),
                "Body research traverses the existing exact working-save guard.");
            foreach (string value in new[] { null, "", scenario.ToUpperInvariant(), scenario + "-arbitrary",
                "working-save-expanded-summoning-serpentine-bodies" })
                Assertions.False(RuntimeTestScenarioCatalog.IsAllowed(value) ||
                    RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(value),
                    "Unknown variants cannot widen the source, save or publication scope.");
            Assertions.True(SerpentineVisualPolicy.Keys.SequenceEqual(new[] { "viper", "constrictor-snake", "salamander" }),
                "Only the three original bodies run; caller parameters cannot substitute arbitrary keys.");
        }

        internal static void OriginalBodyPayloadsAreCompleteAndRedistributionSafe()
        {
            string directory = Path.Combine(Environment.CurrentDirectory, "assets", "sprint17-serpents");
            string[] hashes = {
                "754d82bbdd4a030b0bddb150202bc6ec9bf2d149754741e9d2900a1286463d8e",
                "f7811906bbc1e257536371d2d8192e04d412e26bd2864aa55ae56e68c7e0356f",
                "eb189795bc717e71fc874136f386286bc2fb20fa00c24f83a50d2d89557f9a29" };
            int keyIndex = 0;
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                byte[] bytes = File.ReadAllBytes(Path.Combine(directory, key + "-mesh.json"));
                using (SHA256 hash = SHA256.Create())
                    Assertions.Equal(hashes[keyIndex++], BitConverter.ToString(hash.ComputeHash(bytes))
                        .Replace("-", "").ToLowerInvariant(), "Byte-reproduced original body, not native geometry.");
                JObject mesh = JObject.Parse(System.Text.Encoding.UTF8.GetString(bytes));
                Assertions.True(SerpentineVisualPolicy.ExactSet(mesh.Properties().Select(value => value.Name),
                    new[] { "schemaVersion", "space", "rigSha256", "bones", "uvAtlas", "albedo",
                        "vertexCount", "triangleCount", "data", "visibleLegs", "jawSeparated", "triangleWinding" }),
                    "Only original mesh/paint and bone names; no bind transforms, animation curves or native assets.");
                string[] bones = mesh["bones"].Values<string>().ToArray();
                Assertions.True(SerpentineVisualPolicy.PermitsBones(key, bones) &&
                    (int)mesh["schemaVersion"] == 2 && (int)mesh["visibleLegs"] == 0 && (bool)mesh["jawSeparated"],
                    "The actual loader policy accepts this complete original body.");
                int count = (int)mesh["vertexCount"], triangles = (int)mesh["triangleCount"];
                byte[] data = Convert.FromBase64String((string)mesh["data"]);
                Assertions.True(count > 2000 && triangles > 4000 && data.Length == count * 64 + triangles * 12,
                    "Full original geometry payload, not an asset reference.");
                using (var reader = new BinaryReader(new MemoryStream(data)))
                {
                    for (int i = 0; i < count * 6; i++)
                    {
                        float value = reader.ReadSingle();
                        Assertions.True(!float.IsNaN(value) && !float.IsInfinity(value), "Finite positions/normals.");
                    }
                    for (int i = 0; i < count * 2; i++)
                    {
                        float value = reader.ReadSingle();
                        Assertions.True(value > 0 && value < 1, "Original inset atlas coordinates.");
                    }
                    for (int i = 0; i < triangles * 3; i++)
                    {
                        int index = reader.ReadInt32();
                        Assertions.True(index >= 0 && index < count, "Triangle indices stay inside original geometry.");
                    }
                    var totals = new double[bones.Length];
                    for (int i = 0; i < count; i++)
                    {
                        float sum = 0; int positive = 0;
                        for (int slot = 0; slot < 4; slot++)
                        {
                            int bone = reader.ReadInt32(); float weight = reader.ReadSingle();
                            Assertions.True(bone >= 0 && bone < bones.Length && weight >= 0 && weight <= 1,
                                "Only approved normalized drivers, never extra native branches.");
                            sum += weight; totals[bone] += weight;
                            if (weight > 0) positive++;
                        }
                        Assertions.True(Math.Abs(sum - 1) < .00001 && positive >= 1 && positive <= (key == "salamander" ? 3 : 2),
                            "At most two native influences plus the hybrid-only original support.");
                    }
                    Assertions.True(totals.All(value => value > 0), "Each declared driver carries real geometry.");
                }
                JObject albedo = (JObject)mesh["albedo"];
                Assertions.Equal(key + "-albedo.png", (string)albedo["file"], "Only this creature's painting.");
                using (SHA256 hash = SHA256.Create())
                    Assertions.Equal((string)albedo["sha256"], BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(
                        Path.Combine(directory, key + "-albedo.png")))).Replace("-", "").ToLowerInvariant(),
                        "Exact deterministic original paint pairing.");
            }
        }

        internal static void OriginalBodyWindingHasOutwardGeometryAndFailsClosedOnOldAssets()
        {
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                JObject mesh = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                    "assets", "sprint17-serpents", key + "-mesh.json")));
                Assertions.True(SerpentineVisualPolicy.PermitsOriginalWinding(key, (string)mesh["triangleWinding"]),
                    "Only the measured original-body export convention is accepted.");
                foreach (string bad in new[] { null, "", "clockwise", "foreign", SerpentineVisualPolicy.OutwardWinding.ToUpperInvariant() })
                    Assertions.False(SerpentineVisualPolicy.PermitsOriginalWinding(key, bad),
                        "Missing, historic or unreviewed face conventions fail before attachment.");
                int count = (int)mesh["vertexCount"], triangles = (int)mesh["triangleCount"];
                byte[] data = Convert.FromBase64String((string)mesh["data"]);
                var vertices = new double[count][]; var normals = new double[count][];
                for (int i = 0; i < count; i++)
                {
                    vertices[i] = Enumerable.Range(0, 3).Select(axis => (double)BitConverter.ToSingle(data, i * 12 + axis * 4)).ToArray();
                    normals[i] = Enumerable.Range(0, 3).Select(axis => (double)BitConverter.ToSingle(data, count * 12 + i * 12 + axis * 4)).ToArray();
                }
                double orientation = 0, volume = 0;
                for (int i = 0; i < triangles; i++)
                {
                    int offset = count * 32 + i * 12;
                    int a = BitConverter.ToInt32(data, offset), b = BitConverter.ToInt32(data, offset + 4),
                        c = BitConverter.ToInt32(data, offset + 8);
                    double[] u = Enumerable.Range(0, 3).Select(axis => vertices[b][axis] - vertices[a][axis]).ToArray();
                    double[] v = Enumerable.Range(0, 3).Select(axis => vertices[c][axis] - vertices[a][axis]).ToArray();
                    double[] cross = { u[1] * v[2] - u[2] * v[1], u[2] * v[0] - u[0] * v[2], u[0] * v[1] - u[1] * v[0] };
                    for (int axis = 0; axis < 3; axis++)
                    {
                        orientation += cross[axis] * (normals[a][axis] + normals[b][axis] + normals[c][axis]);
                        volume += vertices[a][axis] * cross[axis] / 6;
                    }
                }
                Assertions.True(orientation > 0 && volume > 0,
                    "Whole-body signed volume and area-weighted normals point outward; historic inverted indices fail both.");
            }
            foreach (string key in new[] { null, "", "purple-worm", "crocodile", "Viper" })
                Assertions.False(SerpentineVisualPolicy.PermitsOriginalWinding(key, SerpentineVisualPolicy.OutwardWinding),
                    "The winding correction cannot opt a different family into attachment.");
        }

        internal static void OriginalBodyBindingRejectsUnreviewedDrivers()
        {
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                string[] bones = SerpentineVisualPolicy.Bones(key);
                Assertions.Equal(key == "salamander" ? 28 : 16, bones.Length, "Measured native drivers plus the hybrid-only support.");
                Assertions.True(SerpentineVisualPolicy.PermitsBones(key, bones.Reverse()),
                    "Native name mapping is independent of export order.");
                foreach (string bone in bones)
                    Assertions.False(SerpentineVisualPolicy.PermitsBones(key, bones.Where(value => value != bone)),
                        "A missing body, jaw, hand or tail driver is rejected.");
                foreach (string foreign in new[] { "Jaw_Left", "Jaw_Right", "Jaw_Up", "Horn", "Stone01",
                    "L_Leg_Upper", "R_Leg_Upper", "WeaponPivot", "ShieldPivot", null, "" })
                    Assertions.False(SerpentineVisualPolicy.PermitsBones(key, bones.Concat(new[] { foreign })),
                        "No worm petal/stone, hybrid leg/equipment or arbitrary branch may carry geometry.");
                Assertions.False(SerpentineVisualPolicy.PermitsBones(key, bones.Concat(new[] { bones[0] })),
                    "Duplicate influence names cannot map ambiguously.");
                bones[0] = "mutated";
                Assertions.True(SerpentineVisualPolicy.PermitsBones(key, SerpentineVisualPolicy.Bones(key)),
                    "Consumers cannot mutate the binding authority.");
            }
            foreach (string unknown in new[] { null, "", "Viper", "purple-worm", "medium-water-elemental", "foreign" })
                Assertions.False(SerpentineVisualPolicy.PermitsBones(unknown, SerpentineVisualPolicy.Bones("viper")),
                    "Only new original identities are allowed, never native negative controls.");
        }

        internal static void MultiRendererBodySwapRequiresExactDonorSet()
        {
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                bool snake = SerpentineVisualPolicy.IsSnake(key);
                string prefab = snake ? SerpentineVisualPolicy.WormPrefab : SerpentineVisualPolicy.ClubShieldPrefab;
                string[] skins = { SerpentineVisualPolicy.BodyRenderer(key), SerpentineVisualPolicy.AuxiliaryRenderer(key) };
                string[] equipment = snake ? new string[0] : new[] { "lizardman_club", "WP_ShieldLightDamaged" };
                int bodyCount = snake ? 40 : 39, extraCount = snake ? 40 : 19;
                string root = snake ? "Hips_Joints" : "Torso_Lower";
                Func<string, string[], int, int, string, string[], bool> permits = (view, names, body, extra, anchor, statics) =>
                    SerpentineVisualPolicy.PermitsDonor(key, view, names, body, extra, anchor, statics);
                Assertions.True(permits(prefab, skins.Reverse().ToArray(), bodyCount, extraCount, root, equipment),
                    "Exact two-skin donor and known static equipment are accepted by BODY policy only.");
                Assertions.False(permits("foreign", skins, bodyCount, extraCount, root, equipment), "No unknown view.");
                Assertions.False(permits(prefab, skins.Take(1).ToArray(), bodyCount, extraCount, root, equipment), "No partial rig.");
                Assertions.False(permits(prefab, skins.Concat(new[] { "extra-skin" }).ToArray(), bodyCount, extraCount, root, equipment),
                    "An unreviewed third skin is not silently left visible.");
                Assertions.False(permits(prefab, new[] { skins[0], skins[0] }, bodyCount, extraCount, root, equipment), "No duplicate skin names.");
                Assertions.False(permits(prefab, skins, bodyCount - 1, extraCount, root, equipment), "Body bind count pinned.");
                Assertions.False(permits(prefab, skins, bodyCount, extraCount - 1, root, equipment), "Auxiliary bind count pinned.");
                Assertions.False(permits(prefab, skins, bodyCount, extraCount, "foreign", equipment), "Root pinned.");
                Assertions.False(permits(prefab, skins, bodyCount, extraCount, root, equipment.Concat(new[] { "unexpected-weapon" }).ToArray()),
                    "Never suppress an unreviewed weapon or leave it silently visible.");
            }
            string[] hybridSkins = { "_lizardman001", "_ammunition04" };
            Assertions.True(SerpentineVisualPolicy.PermitsDonor("salamander", SerpentineVisualPolicy.TwoHandPrefab,
                hybridSkins, 39, 19, "Torso_Lower", new[] { "lizardman_club" }),
                "Measured identical two-hand body frame is accepted without inferring grip/contact.");
            Assertions.False(SerpentineVisualPolicy.PermitsDonor("salamander", SerpentineVisualPolicy.TwoHandPrefab,
                hybridSkins, 39, 19, "Torso_Lower", new[] { "lizardman_club", "WP_ShieldLightDamaged" }),
                "A shield must not silently appear on the two-hand donor.");
        }

        internal static void HybridSupportMappingPreservesEveryNativeSlotAndRejectsUnknowns()
        {
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                string[] original = SerpentineVisualPolicy.Bones(key).Reverse().ToArray();
                string[] required = original.Where(name => name != SerpentineVisualPolicy.HybridSupport).ToArray();
                string[] native = required.Concat(Enumerable.Range(0, (key == "salamander" ? 39 : 40) - required.Length)
                    .Select(index => "UnusedNative" + index)).Reverse().ToArray();
                int[] slots;
                Assertions.True(SerpentineVisualPolicy.TryResolveDriverSlots(key, original, native, out slots),
                    "Complete original driver mapping works independently of native/export order.");
                Assertions.Equal(original.Length, slots.Length, "No missing or appended native mapping.");
                for (int i = 0; i < original.Length; i++)
                    if (original[i] == SerpentineVisualPolicy.HybridSupport)
                        Assertions.True(key == "salamander" && slots[i] == -1, "Only named hybrid support selects renderer frame.");
                    else Assertions.Equal(original[i], native[slots[i]], "Every real driver retains its exact native bindpose slot.");
                foreach (string[] bad in new[] { null, native.Take(native.Length - 1).ToArray(),
                    native.Concat(new[] { "foreign" }).ToArray(), native.Select(name => name == required[0] ? "missing" : name).ToArray(),
                    native.Select(name => name == required[0] ? native[0] : name).ToArray(),
                    native.Select(name => name == required[0] ? SerpentineVisualPolicy.HybridSupport : name).ToArray() })
                {
                    Assertions.False(SerpentineVisualPolicy.TryResolveDriverSlots(key, original, bad, out slots),
                        "Missing, duplicate, foreign-count and project-named native mappings never fall back.");
                    Assertions.Equal((int[])null, slots, "No partially usable mapping on rejection.");
                }
                Assertions.False(SerpentineVisualPolicy.TryResolveDriverSlots(key,
                    original.Concat(new[] { SerpentineVisualPolicy.HybridSupport }).ToArray(), native, out slots),
                    "A support cannot be added to snakes or duplicated on the hybrid.");
            }
        }

        internal static void RigSurveyUsesOnlyExactNativeSources()
        {
            Assertions.True(SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(
                SerpentineRigSurveyPolicy.HybridWeaponBlueprint, SerpentineRigSurveyPolicy.HybridWeaponPrefab,
                SerpentineRigSurveyPolicy.HybridPrimaryWeapon, false),
                "Only the archived native greatclub/no-offhand pair is a detached comparison.");
            foreach (string invalid in new[] { null, "", "foreign",
                SerpentineRigSurveyPolicy.NativeBlueprint("salamander") })
            {
                Assertions.False(SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(invalid,
                    SerpentineRigSurveyPolicy.HybridWeaponPrefab, SerpentineRigSurveyPolicy.HybridPrimaryWeapon, false),
                    "No different native or project blueprint is accepted.");
                Assertions.False(SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(
                    SerpentineRigSurveyPolicy.HybridWeaponBlueprint, invalid,
                    SerpentineRigSurveyPolicy.HybridPrimaryWeapon, false), "No arbitrary prefab is loaded.");
                Assertions.False(SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(
                    SerpentineRigSurveyPolicy.HybridWeaponBlueprint, SerpentineRigSurveyPolicy.HybridWeaponPrefab,
                    invalid, false), "The observed manufactured weapon is pinned too.");
            }
            Assertions.False(SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(
                SerpentineRigSurveyPolicy.HybridWeaponBlueprint, SerpentineRigSurveyPolicy.HybridWeaponPrefab,
                SerpentineRigSurveyPolicy.HybridPrimaryWeapon, true), "A shield/offhand donor cannot substitute.");
            Assertions.False(SerpentineRigSurveyPolicy.CountPresentClips(null).HasValue,
                "A native action without a clip enumeration stays unknown, not an exception or invented zero.");
            Assertions.Equal((int?)0, SerpentineRigSurveyPolicy.CountPresentClips(new bool[0]),
                "An exposed empty clip collection is an explicit zero.");
            Assertions.Equal((int?)0, SerpentineRigSurveyPolicy.CountPresentClips(new[] { false, false }),
                "Null entries do not count as clips.");
            Assertions.Equal((int?)2, SerpentineRigSurveyPolicy.CountPresentClips(new[] { true, false, true }),
                "Only live native clip entries are counted.");
            Assertions.True(SerpentineRigSurveyPolicy.Keys.SequenceEqual(
                new[] { "medium-water-elemental", "salamander", "purple-worm" }),
                "Only the three fixed pre-existing rig carriers are in the research scope; no new publication.");
            string[] copy = SerpentineRigSurveyPolicy.Keys;
            copy[0] = "foreign";
            Assertions.Equal("medium-water-elemental", SerpentineRigSurveyPolicy.Keys[0],
                "Consumers cannot change the shared research target set.");
            foreach (string key in SerpentineRigSurveyPolicy.Keys)
            {
                string native = SerpentineRigSurveyPolicy.NativeBlueprint(key);
                string prefab = SerpentineRigSurveyPolicy.Prefab(key);
                Assertions.True(SerpentineRigSurveyPolicy.MatchesNativeSource(key, native, prefab),
                    "The recorded native blueprint/view pair is accepted.");
                foreach (string bad in new[] { "", null, "foreign", native.ToUpperInvariant(),
                    "6f5db07e89834a01aa9b7e7aed6cd407" })
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key, bad, prefab),
                        "A missing, changed or optional Eidolon source is never substituted.");
                foreach (string bad in new[] { "", null, "foreign", prefab.ToUpperInvariant() })
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key, native, bad),
                        "View resolution is exact, not a permissive arbitrary-asset loader.");
                foreach (string other in SerpentineRigSurveyPolicy.Keys.Where(value => value != key))
                {
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key,
                        SerpentineRigSurveyPolicy.NativeBlueprint(other), prefab),
                        "A different allowed donor cannot substitute for the selected source.");
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key,
                        native, SerpentineRigSurveyPolicy.Prefab(other)),
                        "A different allowed view cannot substitute for the selected source.");
                }
            }
            foreach (string key in new[] { null, "", "viper", "constrictor-snake", "Salamander", "foreign" })
                Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key, null, null),
                    "Unknown, new or differently cased keys fail closed.");
        }
    }
}
