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
        internal static void OriginalBodyPayloadsAreCompleteAndRedistributionSafe()
        {
            string directory = Path.Combine(Environment.CurrentDirectory, "assets", "sprint17-serpents");
            string[] hashes = {
                "bd0d6f7ff16ac47886e621628f8f91391e684e8f299e128af1e88c7e75ce751f",
                "1c521c1bca191a73942a9886706de6415255f18caf787a7f411267283a14df02",
                "80de7840d183666c7e7553c41d96d5bc3f532f4ab2556f62147f867a26371fb6" };
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
                        "vertexCount", "triangleCount", "data", "visibleLegs", "jawSeparated" }),
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
                        Assertions.True(Math.Abs(sum - 1) < .00001 && positive >= 1 && positive <= 2,
                            "At most two normalized original influences.");
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

        internal static void OriginalBodyBindingRejectsUnreviewedDrivers()
        {
            foreach (string key in SerpentineVisualPolicy.Keys)
            {
                string[] bones = SerpentineVisualPolicy.Bones(key);
                Assertions.Equal(key == "salamander" ? 27 : 16, bones.Length, "Measured original driver count.");
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
