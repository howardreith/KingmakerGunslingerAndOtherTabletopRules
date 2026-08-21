using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Assets;
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

        internal static void GeneratedPistolVariantsAreExactAndReproducible()
        {
            JObject report = JObject.Parse(Read("assets-source",
                "original-models", "firearm-pistol-variants",
                "firearm-pistol-variants-build-report.json"));
            JToken[] variants = report["variants"].ToArray();
            Assertions.Equal(2, variants.Length,
                "The project-owned Pistol vocabulary must add exactly two named variants.");
            Assertions.True(new[] { "PistolDuelist", "PistolLastWord" }
                .SequenceEqual(variants.Select(value => (string)value["name"])),
                "Pistol source variant ordering changed.");
            Assertions.Equal("0.339", report["semanticLengthMeters"].ToString(),
                "The fixed Pistol semantic length changed.");
            var expected = new System.Collections.Generic.Dictionary<string,
                string>(StringComparer.Ordinal)
            {
                { "PistolDuelist", "D39F645A949CC8F42386FE852C632A360B50F7E19C13BEDDEDA9714F01B8BBE3" },
                { "PistolLastWord", "BB8CCB51034D2EE66C293E7E5D7BEEC3F0F17340CF7660DD21CB220091AFFDEB" }
            };
            foreach (JToken variant in variants)
            {
                string name = (string)variant["name"];
                string root = Path.Combine(Environment.CurrentDirectory,
                    "assets-source", "original-models", "firearm-pistol-variants");
                string fbx = Path.Combine(root, (string)variant["fbx"]);
                string render = Path.Combine(root, ((string)variant["render"])
                    .Replace('/', Path.DirectorySeparatorChar));
                Assertions.Equal(expected[name], Hash(fbx),
                    name + " FBX identity changed.");
                Assertions.Equal((string)variant["fbxSha256"], Hash(fbx),
                    name + " build report does not match its FBX.");
                Assertions.Equal((string)variant["renderSha256"], Hash(render),
                    name + " build report does not match its render.");
                Assertions.Equal(4, variant["markersMeters"].Count(),
                    name + " does not author exactly four semantic markers.");
                Assertions.True((int)variant["meshCount"] >= 8 &&
                    (int)variant["triangleCount"] >= 200 &&
                    (int)variant["materialCount"] == 3,
                    name + " lacks deliberate low-poly silhouette geometry.");
            }
            string generator = Read("assets-source", "original-models",
                "firearm-pistol-variants",
                "generate_firearm_pistol_variants.py");
            Assertions.True(generator.Contains("PYTHONHASHSEED") &&
                generator.Contains("install_deterministic_fbx_contract") &&
                generator.Contains("object_types={\"EMPTY\", \"MESH\"}") &&
                generator.Contains("Pistol.Duelist") &&
                generator.Contains("Pistol.LastWord"),
                "The Pistol source is not a deterministic exact-variant generator.");
        }

        internal static void PistolItemVariantRuntimeContractIsExact()
        {
            KeyValuePair<string, string>[] firearms =
                WeaponVisualVariantCatalog.Snapshot().Where(value =>
                    value.Key.StartsWith("KMG.Firearms.",
                        StringComparison.Ordinal) ||
                    value.Key == "KMG.Test.TestMusketItem").ToArray();
            Assertions.Equal(14, firearms.Length,
                "Every equipped firearm item must have one exact runtime mapping.");
            Assertions.Equal(7, firearms.Select(value => value.Value)
                .Distinct(StringComparer.Ordinal).Count(),
                "The pre-review firearm vocabulary must be seven bounded variants.");
            Assertions.Equal(3, firearms.Where(value => value.Value.StartsWith(
                    "Pistol.", StringComparison.Ordinal)).Select(value => value.Value)
                .Distinct(StringComparer.Ordinal).Count(),
                "Pistol must expose Service, Duelist, and LastWord variants.");
            Assertions.Equal(WeaponVisualVariantCatalog.PistolDuelist,
                WeaponVisualVariantCatalog.Require(
                    "KMG.Firearms.DuelistsRebuttalItem"),
                "Duelist's Rebuttal mapping changed.");
            Assertions.Equal(WeaponVisualVariantCatalog.PistolLastWord,
                WeaponVisualVariantCatalog.Require(
                    "KMG.Firearms.TheLastWordItem"),
                "The Last Word mapping changed.");

            string builder = Read("tools", "unity",
                "BuildFirearmBundles.cs");
            string runtime = Read("src", "KingmakerGunslinger", "Assets",
                "FirearmAssetRuntime.cs");
            string presentation = Read("src", "KingmakerGunslinger",
                "Blueprints", "FirearmWeaponPresentation.cs");
            string magic = Read("src", "KingmakerGunslinger", "Blueprints",
                "MagicFirearmBlueprints.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string staging = Read("scripts", "Prepare-UnityAssets.ps1");
            foreach (string token in new[] { "PistolDuelist",
                "PistolLastWord", "pistol-duelist.fbx",
                "pistol-last-word.fbx" })
                Assertions.True(builder.Contains(token) || staging.Contains(token),
                    "Pistol Unity pipeline omitted " + token + ".");
            foreach (string token in new[] { "ItemVariantPrefabs",
                "TryLoadItemVariantPrefab", "PublishServiceVariants",
                "HasValidatedItemVariant", "InstantiateItemVariantPrefab" })
                Assertions.True(runtime.Contains(token),
                    "Firearm variant runtime omitted " + token + ".");
            Assertions.True(presentation.Contains("ApplyItemVariant") &&
                presentation.Contains("HasExactItemVariant") &&
                presentation.Contains("maps across its qualified firearm family boundary") &&
                magic.Contains("ApplyItemVariant(clone") &&
                runner.Contains("firearm-all-14-item-visual-identities") &&
                runner.Contains("pistol-three-variant-separation") &&
                runner.Contains("firearm-item-variant-cleanup"),
                "Exact item binding or fail-closed runtime observation is incomplete.");
        }

        internal static void MarkerImporterFailsClosed()
        {
            string builder = Read("tools", "unity", "BuildFirearmBundles.cs");
            foreach (string marker in new[] { "KMG_Grip", "KMG_Support",
                "KMG_Butt", "KMG_Muzzle", "KMG_Back", "KMG_WeaponUp",
                "KMG_WeaponForward" })
                Assertions.True(builder.Contains(marker),
                    "Unity marker importer omitted " + marker + ".");
            foreach (string failure in new[] {
                "partial-or-duplicate-marker-set=true",
                "requires its complete source-authored KMG marker set",
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
            string runtime = Read("src", "KingmakerGunslinger", "Assets",
                "FirearmAssetRuntime.cs");
            string presentation = Read("src", "KingmakerGunslinger", "Blueprints",
                "FirearmWeaponPresentation.cs");
            string generator = Read("assets-source", "third-party", "models",
                "firearm-long-gun-derivatives", "generate_long_gun_derivatives.py");
            string staging = Read("scripts", "Prepare-UnityAssets.ps1");
            JObject report = JObject.Parse(Read("assets-source", "third-party",
                "models", "firearm-long-gun-derivatives", "generation-report.json"));
            Assertions.Equal(2, (int)report["schemaVersion"],
                "The canonical long-gun report schema changed.");
            Assertions.Equal(3, report["outputs"].Count(),
                "The canonical long-gun build must publish exactly three derivatives.");
            Assertions.Equal("+X physical butt-to-muzzle",
                (string)report["sourceForwardAxis"],
                "The measured physical source polarity changed.");
            Assertions.Equal("+Z physical stock/receiver up",
                (string)report["sourceUpAxis"],
                "The measured physical source roll axis changed.");
            foreach (JToken output in report["outputs"])
            {
                string path = Path.Combine(Environment.CurrentDirectory,
                    "assets-source", "third-party", "models",
                    "firearm-long-gun-derivatives", (string)output["output"]);
                Assertions.True(File.Exists(path), "Normalized long-gun FBX is missing.");
                Assertions.Equal((string)output["outputSha256"], Hash(path),
                    "Normalized long-gun FBX differs from its generation report.");
                Assertions.Equal(7, output["markers"].Count(),
                    "A canonical long-gun derivative lacks its exact seven-marker frame contract.");
                Assertions.True(output["markers"].Any(value =>
                        (string)value == "KMG_WeaponUp") &&
                    output["markers"].Any(value =>
                        (string)value == "KMG_WeaponForward") &&
                    output["sourceFrame"] != null &&
                    output["canonicalFrame"] != null &&
                    (double)output["canonicalFrame"][
                        "nearestGripGeometryMeters"] < 0.12,
                    "A canonical derivative lacks its measured source basis or physical grip evidence.");
            }
            JToken musket = report["outputs"].Single(value =>
                (string)value["name"] == "musket-normalized");
            Assertions.True(Math.Abs((double)musket["blenderMarkerMeters"]
                    ["KMG_Support"][2] - 0.374) < 0.000001 &&
                Math.Abs((double)musket["expectedUnityMarkerMeters"]
                    ["KMG_Support"][2] - 0.374) < 0.000001,
                "The Musket support marker drifted from the measured native Heavy Crossbow IK station.");
            Assertions.True(builder.Contains("musket-normalized.fbx") &&
                builder.Contains("blunderbuss-normalized.fbx") &&
                builder.Contains("rifle-normalized.fbx") &&
                builder.Contains("RifleBelt") &&
                builder.Contains("CanonicalLongGun") &&
                builder.Contains("StoredLongGun") &&
                builder.Contains("NativeHeavyCrossbowHeldEuler") &&
                builder.Contains("NativeHeavyCrossbowStoredEuler") &&
                builder.Contains("NativeHeavyCrossbowStoredRendererCenter") &&
                builder.Contains("TargetAnchorPosition") &&
                builder.Contains("KMG_WeaponUp") &&
                builder.Contains("KMG_WeaponForward") &&
                builder.Contains("buttProjection") &&
                builder.Contains("supportProjection") &&
                builder.Contains("muzzleProjection") &&
                builder.Contains("semantic butt-to-muzzle length is outside") &&
                builder.Contains("BackMount") && builder.Contains("KMG_Back") &&
                builder.Contains("diagnostic-pass-through; not-production-bound") &&
                builder.Contains("diagnostic-minimal-control; not-production-bound") &&
                builder.Contains("diagnostic-clearance-stock; not-production-bound"),
                "Normalized production binding or diagnostic isolation changed.");
            Assertions.True(!Identities.Any(profile.Contains) &&
                profile.Contains("FirearmKind.Musket") &&
                profile.Contains("FirearmKind.Blunderbuss") &&
                profile.Contains("FirearmKind.Rifle") &&
                profile.Split(new[] { "FirearmHolsterPolicy.Custom" },
                    StringSplitOptions.None).Length - 1 >= 3,
                "A diagnostic candidate leaked or custom back publication was removed.");
            Assertions.True(runtime.Contains("TryLoadBackPrefab") &&
                runtime.Contains("FirearmKind.Rifle, \"riflebelt\"") &&
                runtime.Contains("identity-root+Visual+BackMount+renderer") &&
                presentation.Contains("m_WeaponSheathModel\", null") &&
                generator.Contains("KMG_Support") &&
                generator.Contains("KMG_Muzzle") &&
                generator.Contains("KMG_WeaponUp") &&
                generator.Contains("KMG_WeaponForward") &&
                staging.Contains("three canonical long-gun derivatives"),
                "The normalized rig/back fail-closed runtime contract is incomplete.");
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
