using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmFitAssetTests
    {
        private static readonly string[] Identities = {
            "MusketPassThrough", "MusketMinimalControl",
            "MusketClearanceStock"
        };

        internal static void GeneratedCandidatesAreExactAndReproducible()
        {
            JObject report = JObject.Parse(Read("assets-source", "original-models",
                "firearm-fit-experiments",
                "musket-fit-candidates-build-report.json"));
            JToken[] candidates = report["candidates"].ToArray();
            Assertions.Equal(3, candidates.Length,
                "The bounded Musket geometry experiment must contain three candidates.");
            Assertions.True(Identities.SequenceEqual(candidates.Select(value =>
                (string)value["name"])), "Musket diagnostic ordering changed.");
            Assertions.Equal("1.349985", report["semanticLengthMeters"].ToString(),
                "The fixed Musket semantic length changed.");
            foreach (JToken candidate in candidates)
            {
                string fbx = Path.Combine(Environment.CurrentDirectory,
                    "assets-source", "original-models", "firearm-fit-experiments",
                    (string)candidate["fbx"]);
                string render = Path.Combine(Environment.CurrentDirectory,
                    "assets-source", "original-models", "firearm-fit-experiments",
                    ((string)candidate["render"]).Replace('/', Path.DirectorySeparatorChar));
                Assertions.True(File.Exists(fbx) && File.Exists(render),
                    (string)candidate["name"] + " generated artifacts are missing.");
                Assertions.Equal((string)candidate["fbxSha256"], Hash(fbx),
                    (string)candidate["name"] + " FBX differs from its build report.");
                Assertions.Equal((string)candidate["renderSha256"], Hash(render),
                    (string)candidate["name"] + " render differs from its build report.");
                Assertions.True((int)candidate["meshCount"] > 0 &&
                    (int)candidate["triangleCount"] > 0 &&
                    (int)candidate["materialCount"] <= 3,
                    (string)candidate["name"] +
                    " violates the visible low-material structural contract.");
                Assertions.Equal(4, candidate["markersMeters"].Count(),
                    (string)candidate["name"] +
                    " does not author exactly four semantic markers.");
            }
            Assertions.Equal("BD3AFC3372453FAFF4742220B5E49FC7E021F10D9596E5C7000D2555FE486E18",
                (string)report["sourceFbxSha256"],
                "The preserved licensed pass-through input changed.");
            string generator = Read("assets-source", "original-models",
                "firearm-fit-experiments", "generate_musket_fit_candidates.py");
            Assertions.True(generator.Contains("PYTHONHASHSEED") &&
                generator.Contains("install_deterministic_fbx_contract") &&
                generator.Contains("object_types={\"EMPTY\", \"MESH\"}") &&
                generator.IndexOf("export(root", StringComparison.Ordinal) <
                generator.IndexOf("render(root", StringComparison.Ordinal),
                "The generator must use stable FBX identities and export production trees before adding render-only objects.");
        }

        internal static void MarkerImporterFailsClosed()
        {
            string builder = Read("tools", "unity", "BuildFirearmBundles.cs");
            foreach (string marker in new[] { "KMG_Grip", "KMG_Support",
                "KMG_Butt", "KMG_Muzzle" })
                Assertions.True(builder.Contains(marker),
                    "Unity marker importer omitted " + marker + ".");
            foreach (string failure in new[] {
                "partial-or-duplicate-marker-set=true",
                "requires source-authored KMG_Grip/KMG_Support/KMG_Butt/KMG_Muzzle markers",
                "non-finite source-authored semantic marker",
                "marker-authored +Z muzzle axis is invalid",
                "marker-authored support point is outside the plausible weapon envelope",
                "marker-authored scale/length is implausible",
                "has no renderer"
            })
                Assertions.True(builder.Contains(failure),
                    "Unity importer lacks fail-closed evidence for " + failure + ".");
            Assertions.True(builder.Contains("source=legacy-fallback") &&
                builder.Contains("source=authored") &&
                builder.Contains("ResolveSemanticMarkers(source, spec)") &&
                builder.Contains("spec.RequireSourceMarkers"),
                "Source-authored markers and the explicit legacy fallback are not distinguishable.");
            string staging = Read("scripts", "Prepare-UnityAssets.ps1");
            foreach (string fbx in new[] { "musket-pass-through.fbx",
                "musket-minimal-control.fbx", "musket-clearance-stock.fbx" })
                Assertions.True(staging.Contains(fbx),
                    "Unity staging omitted " + fbx + ".");
        }

        internal static void DiagnosticRuntimeBoundaryIsExact()
        {
            string runtime = Read("src", "KingmakerGunslinger", "Assets",
                "FirearmAssetRuntime.cs");
            string runner = Read("src", "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs");
            string calibration = Read("src", "KingmakerGunslinger", "Development",
                "FirearmVisualCalibration.cs");
            string calibrationUi = Read("src", "KingmakerGunslinger", "Development",
                "FirearmVisualCalibrationUi.cs");
            foreach (string identity in Identities)
                Assertions.True(runtime.Contains(identity) && runner.Contains(identity),
                    identity + " lacks a runtime load/observer contract.");
            Assertions.True(runtime.Contains("DiagnosticPrefabs") &&
                runtime.Contains("DiagnosticCapabilities") &&
                runtime.Contains("productionBinding=false") &&
                runtime.Contains("InstantiateDiagnosticPrefab") &&
                runtime.Contains("HasValidatedDiagnosticPrefab"),
                "Diagnostic prefabs are not isolated from the production FirearmKind cache.");
            Assertions.True(runner.Contains("new Vector3(0f, 0f, 1.180452f)") &&
                runner.Contains("new Vector3(-0.030976f, -0.051069f, 0.586040f)") &&
                runner.Contains("new Vector3(0f, 0f, -0.169533f)") &&
                runner.Contains("exactMarkerSet") &&
                runner.Contains("ReferenceEquals(offsets.IkTargetLeftHand") &&
                runner.Contains("finally-owned transient diagnostic GameObject"),
                "Runtime observation does not prove the exact fixed frame, markers, IK, and cleanup.");
            Assertions.True(calibration.Contains("ShowSelectedMusketDiagnostic") &&
                calibration.Contains("firearm.Definition.Kind != FirearmKind.Musket") &&
                calibration.Contains("GetDiagnosticPrefab(identity)") &&
                calibration.Contains("HandsEquipment.UpdateAll()") &&
                calibration.Contains("close/reopen inventory for a clean doll rebuild") &&
                calibrationUi.Contains("Show pass-through Musket") &&
                calibrationUi.Contains("Show minimal-control Musket") &&
                calibrationUi.Contains("Show clearance-stock Musket") &&
                calibrationUi.Contains("Restore production Musket"),
                "The human comparison lab cannot select and safely restore all three diagnostic Musket candidates.");
        }

        internal static void ProductionBindingRemainsFrozen()
        {
            string builder = Read("tools", "unity", "BuildFirearmBundles.cs");
            string profile = Read("src", "KingmakerGunslinger", "Assets",
                "FirearmPresentationProfile.cs");
            Assertions.True(builder.Contains(
                "Spec(\"Musket\", \"Musket\", \"Musket 01.fbx\", false, true") &&
                builder.Contains("new Vector3(0.0400f, 0f, 0f)") &&
                builder.Contains("new Vector3(-0.1000f, -0.0122f, -0.0074f)") &&
                builder.Contains("diagnostic-pass-through; not-production-bound") &&
                builder.Contains("diagnostic-minimal-control; not-production-bound") &&
                builder.Contains("diagnostic-clearance-stock; not-production-bound"),
                "The production Musket fallback or diagnostic-only status changed.");
            Assertions.True(!Identities.Any(profile.Contains) &&
                profile.Contains("FirearmKind.Musket"),
                "A diagnostic candidate leaked into production item presentation before human selection.");
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
