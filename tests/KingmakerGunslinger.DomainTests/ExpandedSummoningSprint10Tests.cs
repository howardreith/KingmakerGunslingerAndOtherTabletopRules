using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint10Tests
    {
        internal static void GiantWaspOriginalVisualUsesAuditedInstanceBinding()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "flying-animals");
            string path = Path.Combine(directory, "giant-wasp-mesh.json");
            JObject mesh = JObject.Parse(File.ReadAllText(path));
            Assertions.Equal(2, (int)mesh["schemaVersion"],
                "Wasp must use the shared skinned-mesh schema.");
            Assertions.True(((string)mesh["space"]).Contains("donor renderer local"),
                "Wasp geometry must bind in the donor's measured local frame.");
            int vertices = (int)mesh["vertexCount"];
            int triangles = (int)mesh["triangleCount"];
            Assertions.True(vertices >= 500 && triangles >= 500,
                "Wasp must carry a body, stinger, six legs and four wings.");
            byte[] payload = Convert.FromBase64String((string)mesh["data"]);
            Assertions.Equal(vertices * 64 + triangles * 12, payload.Length,
                "Wasp vertex, triangle and bone-weight payload must be complete.");
            string[] bones = ((JArray)mesh["bones"])
                .Select(value => (string)value).ToArray();
            Assertions.Equal(bones.Length,
                bones.Distinct(StringComparer.Ordinal).Count(),
                "Every Wasp weight must have one unambiguous bone name.");
            Assertions.True(bones.Contains("L_Arm_Upper") &&
                bones.Contains("R_Arm_Upper") && bones.Contains("Tail"),
                "Wing and stinger geometry must follow animated donor bones.");
            JObject albedo = (JObject)mesh["albedo"];
            Assertions.Equal("giant-wasp-albedo.png", (string)albedo["file"],
                "Wasp mesh must name its own adjacent painting.");
            using (var sha = SHA256.Create())
            {
                string actual = string.Concat(sha.ComputeHash(File.ReadAllBytes(
                    Path.Combine(directory, (string)albedo["file"])))
                    .Select(value => value.ToString("x2")));
                Assertions.Equal((string)albedo["sha256"], actual,
                    "Wasp painting must match the exact audited mesh manifest.");
            }
            Assertions.True(File.Exists(Path.Combine(root, "assets-source",
                "original-models", "flying-animals", "generate_giant_wasp.py")) &&
                File.Exists(Path.Combine(root, "assets-source",
                    "original-models", "flying-animals",
                    "paint_giant_wasp_albedo.py")),
                "Wasp's original editable geometry and painting must remain available.");
            string view = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            Assertions.True(view.Contains("TryGetGiantWaspVisual") &&
                view.Contains("GiantWaspVisualName") &&
                view.Contains("TryResolveDonorBinding(donor, boneNames") &&
                view.Contains("donor.sharedMesh = mesh") &&
                view.Contains("donor.sharedMaterials = new[] { material }") &&
                view.Contains("Revert(attachment)"),
                "Wasp must share the validated per-instance swap and rollback.");
            string loader = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Assets", "PteranodonAssetRuntime.cs"));
            Assertions.True(loader.Contains(
                "assets/flying-animals/giant-wasp-mesh.json") &&
                loader.Contains("ConfigureGiantWasp(context)") &&
                loader.Contains("BuildMesh(File.ReadAllText(path)") &&
                loader.Contains("LoadAlbedo(Path.GetDirectoryName(path)"),
                "Wasp must load through the package's audited mesh and painting parser.");
            string audit = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            Assertions.True(audit.Contains("giant-wasp-original-asset-loader") &&
                audit.Contains("PteranodonAssetRuntime.GiantWaspStatus == \"visual:published\""),
                "The guarded audit must fail if the packaged Wasp mesh or painting is rejected in game.");
        }

        internal static void NativeFlyingVerminSurveyStaysMetadataOnly()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            Assertions.True(source.Contains("\"wasp\", \"stirge\"") &&
                source.Contains("\"mosquito\"") &&
                source.Contains("\"beetle\"") &&
                source.Contains("\"mantis\"") &&
                source.Contains("56ec8788092b6314e8f3c1c502e8433f") &&
                source.Contains("\"blood\"") &&
                source.Contains("\"attach\"") &&
                source.Contains("\"drain\"") &&
                source.Contains("GetAllBlueprints().Where(value => value != null)") &&
                source.Contains("units.Add(DescribeNativeUnit(unit))") &&
                source.Contains("native-donor-audit.json") &&
                !source.Contains("AssetBundle.LoadFromFile") &&
                !source.Contains("Texture2D.EncodeToPNG"),
                "Sprint 10 intake must inventory installed native metadata for both flying vermin and their signature seams without exporting game art.");
        }
    }
}
