using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint9Tests
    {
        internal static void DireBatPublishesOnlyItsPreservedPlacements()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            SummonVariantSpec[] bat = all.Where(value =>
                value.Creature.Key == "dire-bat").ToArray();
            Assertions.Equal(813, all.Length,
                "Sprint 9 must not create or remove logical placements.");
            Assertions.Equal(14, bat.Length,
                "Dire Bat retains seven placements in each summon family.");
            Assertions.True(all.All(SummonVisibilityCatalog.IsPublished),
                "Only the preserved Dire Bat placements may change visibility.");
            foreach (SummonFamily family in new[] {
                SummonFamily.Monster, SummonFamily.NaturesAlly })
            {
                SummonVariantSpec[] familyBat = bat.Where(value =>
                    value.Family == family).OrderBy(value => value.ParentTier)
                    .ToArray();
                Assertions.Equal("3|4|5|6|7|8|9",
                    string.Join("|", familyBat.Select(value => value.ParentTier)),
                    "Every published Bat tier must retain its old placement.");
                Assertions.Equal(SummonMultiplicity.One,
                    familyBat[0].Multiplicity, "Bat starts as one summon.");
                Assertions.Equal(SummonMultiplicity.OneD3,
                    familyBat[1].Multiplicity, "The next Bat tier uses 1d3.");
                Assertions.True(familyBat.Skip(2).All(value =>
                    value.Multiplicity == SummonMultiplicity.OneD4PlusOne),
                    "Later Bat tiers use 1d4+1.");
            }
        }

        internal static void EagleAndBatPersistWithTheirOwnViews()
        {
            string runner = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains(
                    "new[] { \"Monster\", \"eagle\", \"1\" }") &&
                runner.Contains(
                    "new[] { \"NaturesAlly\", \"dire-bat\", \"3\" }") &&
                runner.Contains("expanded-summoning-persistent-eagle-bat-visuals") &&
                runner.Contains("IsEagleAttached(") &&
                runner.Contains("IsDireBatAttached("),
                "The save/reload fixture must verify both original flying visuals.");
        }

        internal static void DireBatSenseHasASeparateBoundedIdentity()
        {
            var bat = ExpandedSummoningNaturalProfiles.For("dire-bat");
            Assertions.True(bat.Facts.Contains("DireBatBlindsense"),
                "Dire Bat needs its own imprecise blindsense fact.");
            Assertions.Equal(1, ExpandedSummoningIdentityCatalog.Build().Count(value =>
                value.Symbol == "KMG.Summoning.Natural.DireBat.Blindsense" &&
                value.PlannedType == "BlueprintFeature"),
                "The sense needs one append-only feature identity.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            Assertions.True(builder.Contains("nativeSense.Blindsight = false") &&
                builder.Contains("nativeSense.Range = new Feet(40)"),
                "Dire Bat senses creatures within 40 feet without precise blindsight.");
            string runner = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains("expanded-summoning-dire-bat-blindsense") &&
                runner.Contains("part.Reach(caster)") &&
                runner.Contains("{ \"DireBatBlindsense\", \"5dcc039bc9674208a51e4babcd8a30ee\" }") &&
                runner.Contains("\"KMG_Summoning_Natural_DireBat_Blindsense\""),
                "Guarded cast and inventory audits must recognize only the Bat-owned sense fact.");
        }

        internal static void DireBatOriginalMeshAndPaintingAreBound()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "flying-animals");
            string meshPath = Path.Combine(directory, "dire-bat-mesh.json");
            Assertions.True(File.Exists(meshPath),
                "The bat's original skinned mesh must ship beside its albedo.");
            JObject document = JObject.Parse(File.ReadAllText(meshPath));
            Assertions.Equal(2, (int)document["schemaVersion"],
                "The shared skinned-mesh runtime accepts schema 2.");
            Assertions.True(((string)document["space"]).Contains("donor renderer local"),
                "The bat is authored in the measured donor bind frame.");
            JArray bones = (JArray)document["bones"];
            Assertions.Equal(46, bones.Count, "Bat weights use the audited flying rig.");
            Assertions.Equal(46, bones.Select(value => (string)value)
                .Distinct(StringComparer.Ordinal).Count(), "No ambiguous bat bones.");
            int vertices = (int)document["vertexCount"];
            int triangles = (int)document["triangleCount"];
            Assertions.True(vertices > 1000 && triangles > 1000,
                "The bat has a skinned body, fingers and two-sided membranes.");
            byte[] bytes = Convert.FromBase64String((string)document["data"]);
            int expected = vertices * (12 + 12 + 8 + 4 * 8) + triangles * 3 * 4;
            Assertions.Equal(expected, bytes.Length,
                "The mesh payload exactly matches its declared vertex/triangle counts.");
            int weightOffset = vertices * (12 + 12 + 8) + triangles * 3 * 4;
            for (int vertex = 0; vertex < vertices; vertex++)
            {
                double total = 0;
                for (int slot = 0; slot < 4; slot++)
                {
                    int offset = weightOffset + vertex * 32 + slot * 8;
                    int bone = BitConverter.ToInt32(bytes, offset);
                    float weight = BitConverter.ToSingle(bytes, offset + 4);
                    Assertions.True(!float.IsNaN(weight) && weight >= 0f &&
                        (weight == 0f || bone >= 0 && bone < bones.Count),
                        "Bat vertex " + vertex + " has a valid bone weight.");
                    total += weight;
                }
                Assertions.True(Math.Abs(total - 1.0) < 0.001,
                    "Bat vertex " + vertex + " weights sum to one.");
            }
            JObject albedo = (JObject)document["albedo"];
            Assertions.Equal("dire-bat-albedo.png", (string)albedo["file"],
                "The mesh names only the adjacent original painting.");
            string pngPath = Path.Combine(directory, (string)albedo["file"]);
            Assertions.True(File.Exists(pngPath), "The bat painting must ship.");
            using (var sha = SHA256.Create())
            {
                string actual = string.Concat(sha.ComputeHash(File.ReadAllBytes(pngPath))
                    .Select(value => value.ToString("x2")));
                Assertions.Equal((string)albedo["sha256"], actual,
                    "The bat mesh is bound to the exact painted bytes.");
            }
            Assertions.True(File.Exists(Path.Combine(root, "assets-source",
                "original-models", "flying-animals", "generate_dire_bat.py")) &&
                File.Exists(Path.Combine(root, "assets-source", "original-models",
                    "flying-animals", "paint_dire_bat_albedo.py")),
                "Editable original mesh and painting generators are retained.");
        }

        internal static void EagleOriginalMeshAndPaintingAreBound()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "flying-animals");
            string meshPath = Path.Combine(directory, "eagle-mesh.json");
            Assertions.True(File.Exists(meshPath),
                "The Small Eagle must have its own feathered mesh.");
            JObject document = JObject.Parse(File.ReadAllText(meshPath));
            Assertions.Equal(2, (int)document["schemaVersion"],
                "Eagle mesh must use the audited skinned-mesh schema.");
            Assertions.Equal(46, ((JArray)document["bones"]).Count,
                "Eagle uses the same measured flying-animal binding.");
            int vertices = (int)document["vertexCount"];
            int triangles = (int)document["triangleCount"];
            Assertions.True(vertices > 1500 && triangles > 1500,
                "Eagle must contain feather vanes, fan and body, not a donor skin.");
            byte[] bytes = Convert.FromBase64String((string)document["data"]);
            Assertions.Equal(vertices * 64 + triangles * 12, bytes.Length,
                "Eagle payload must contain exact vertices, faces and weights.");
            JObject albedo = (JObject)document["albedo"];
            Assertions.Equal("eagle-albedo.png", (string)albedo["file"],
                "Eagle mesh names its own painting.");
            string pngPath = Path.Combine(directory, (string)albedo["file"]);
            Assertions.True(File.Exists(pngPath), "Eagle painting must ship.");
            using (var sha = SHA256.Create())
            {
                string actual = string.Concat(sha.ComputeHash(File.ReadAllBytes(pngPath))
                    .Select(value => value.ToString("x2")));
                Assertions.Equal((string)albedo["sha256"], actual,
                    "The Eagle mesh is bound to its exact painting.");
            }
            Assertions.True(File.Exists(Path.Combine(root, "assets-source",
                "original-models", "flying-animals", "generate_eagle.py")) &&
                File.Exists(Path.Combine(root, "assets-source", "original-models",
                    "flying-animals", "paint_eagle_albedo.py")),
                "Retain editable original Eagle geometry and paint generation.");
        }
    }
}
