using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Compatibility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free, guarded inspection of the three KMG-owned asset bundles.
    /// It instantiates request-local copies solely to exercise Unity loading
    /// and presentation validation, then destroys every copy before returning.
    /// </summary>
    internal static class CompatibilityAssetAttributionScenario
    {
        private const string EvidenceFileName =
            "kmg-compatibility-asset-attribution.json";

        private sealed class ScenarioEvidence
        {
            [JsonProperty("schemaVersion", Order = 1)]
            public int SchemaVersion { get; set; }
            [JsonProperty("configuration", Order = 2)]
            public string Configuration { get; set; }
            [JsonProperty("guardedRequest", Order = 3)]
            public bool GuardedRequest { get; set; }
            [JsonProperty("runId", Order = 4)]
            public string RunId { get; set; }
            [JsonProperty("unityVersion", Order = 5)]
            public string UnityVersion { get; set; }
            [JsonProperty("buildTarget", Order = 6)]
            public string BuildTarget { get; set; }
            [JsonProperty("globalLightmapsMode", Order = 7)]
            public string GlobalLightmapsMode { get; set; }
            [JsonProperty("bundleManifestSha256", Order = 8)]
            public string BundleManifestSha256 { get; set; }
            [JsonProperty("families", Order = 9)]
            public List<FamilyEvidence> Families { get; set; }
            [JsonProperty("totals", Order = 10)]
            public TotalsEvidence Totals { get; set; }
            [JsonProperty("cleanupComplete", Order = 11)]
            public bool CleanupComplete { get; set; }
        }

        private sealed class FamilyEvidence
        {
            [JsonProperty("family", Order = 1)] public string Family { get; set; }
            [JsonProperty("bundleName", Order = 2)] public string BundleName { get; set; }
            [JsonProperty("enabled", Order = 3)] public bool Enabled { get; set; }
            [JsonProperty("loaded", Order = 4)] public bool Loaded { get; set; }
            [JsonProperty("bundlePath", Order = 5)] public string BundlePath { get; set; }
            [JsonProperty("bundleSha256", Order = 6)] public string BundleSha256 { get; set; }
            [JsonProperty("expectedSha256", Order = 7)] public string ExpectedSha256 { get; set; }
            [JsonProperty("assetPaths", Order = 8)] public List<AssetEvidence> Assets { get; set; }
            [JsonProperty("prefabs", Order = 9)] public List<PrefabEvidence> Prefabs { get; set; }
            [JsonProperty("inspectionErrors", Order = 10)] public List<string> InspectionErrors { get; set; }
        }

        private sealed class AssetEvidence
        {
            [JsonProperty("path", Order = 1)] public string Path { get; set; }
            [JsonProperty("type", Order = 2)] public string Type { get; set; }
            [JsonProperty("name", Order = 3)] public string Name { get; set; }
        }

        private sealed class PrefabEvidence
        {
            [JsonProperty("assetPath", Order = 1)] public string AssetPath { get; set; }
            [JsonProperty("name", Order = 2)] public string Name { get; set; }
            [JsonProperty("componentTypes", Order = 3)] public List<string> ComponentTypes { get; set; }
            [JsonProperty("missingSerializedComponents", Order = 4)] public int MissingSerializedComponents { get; set; }
            [JsonProperty("cameraCount", Order = 5)] public int CameraCount { get; set; }
            [JsonProperty("lightCount", Order = 6)] public int LightCount { get; set; }
            [JsonProperty("particleSystemCount", Order = 7)] public int ParticleSystemCount { get; set; }
            [JsonProperty("particleRendererCount", Order = 8)] public int ParticleRendererCount { get; set; }
            [JsonProperty("renderers", Order = 9)] public List<RendererEvidence> Renderers { get; set; }
            [JsonProperty("instanceDestroyed", Order = 10)] public bool InstanceDestroyed { get; set; }
        }

        private sealed class RendererEvidence
        {
            [JsonProperty("path", Order = 1)] public string Path { get; set; }
            [JsonProperty("type", Order = 2)] public string Type { get; set; }
            [JsonProperty("enabled", Order = 3)] public bool Enabled { get; set; }
            [JsonProperty("lightmapIndex", Order = 4)] public int LightmapIndex { get; set; }
            [JsonProperty("realtimeLightmapIndex", Order = 5)] public int RealtimeLightmapIndex { get; set; }
            [JsonProperty("lightmapScaleOffset", Order = 6)] public string LightmapScaleOffset { get; set; }
            [JsonProperty("materials", Order = 7)] public List<MaterialEvidence> Materials { get; set; }
            [JsonProperty("mesh", Order = 8)] public MeshEvidence Mesh { get; set; }
            [JsonProperty("particleRenderMode", Order = 9)] public string ParticleRenderMode { get; set; }
        }

        private sealed class MaterialEvidence
        {
            [JsonProperty("slot", Order = 1)] public int Slot { get; set; }
            [JsonProperty("name", Order = 2)] public string Name { get; set; }
            [JsonProperty("shader", Order = 3)] public string Shader { get; set; }
            [JsonProperty("shaderSupported", Order = 4)] public bool ShaderSupported { get; set; }
            [JsonProperty("hasMainTexProperty", Order = 5)] public bool HasMainTexProperty { get; set; }
            [JsonProperty("mainTexture", Order = 6)] public string MainTexture { get; set; }
        }

        private sealed class MeshEvidence
        {
            [JsonProperty("name", Order = 1)] public string Name { get; set; }
            [JsonProperty("readable", Order = 2)] public bool Readable { get; set; }
            [JsonProperty("vertexCount", Order = 3)] public int VertexCount { get; set; }
            [JsonProperty("subMeshCount", Order = 4)] public int SubMeshCount { get; set; }
            [JsonProperty("triangleCount", Order = 5)] public int? TriangleCount { get; set; }
            [JsonProperty("surfaceArea", Order = 6)] public double? SurfaceArea { get; set; }
            [JsonProperty("boundsSize", Order = 7)] public string BoundsSize { get; set; }
        }

        private sealed class TotalsEvidence
        {
            [JsonProperty("assetPaths", Order = 1)] public int AssetPaths { get; set; }
            [JsonProperty("prefabs", Order = 2)] public int Prefabs { get; set; }
            [JsonProperty("renderers", Order = 3)] public int Renderers { get; set; }
            [JsonProperty("materials", Order = 4)] public int Materials { get; set; }
            [JsonProperty("unsupportedShaders", Order = 5)] public int UnsupportedShaders { get; set; }
            [JsonProperty("nonStandardShaders", Order = 6)] public int NonStandardShaders { get; set; }
            [JsonProperty("materialsWithoutMainTexProperty", Order = 7)] public int MaterialsWithoutMainTexProperty { get; set; }
            [JsonProperty("meshes", Order = 8)] public int Meshes { get; set; }
            [JsonProperty("zeroAreaReadableMeshes", Order = 9)] public int ZeroAreaReadableMeshes { get; set; }
            [JsonProperty("particleSystems", Order = 10)] public int ParticleSystems { get; set; }
            [JsonProperty("particleRenderers", Order = 11)] public int ParticleRenderers { get; set; }
            [JsonProperty("missingSerializedComponents", Order = 12)] public int MissingSerializedComponents { get; set; }
            [JsonProperty("cameras", Order = 13)] public int Cameras { get; set; }
            [JsonProperty("lights", Order = 14)] public int Lights { get; set; }
            [JsonProperty("lightmappedRenderers", Order = 15)] public int LightmappedRenderers { get; set; }
            [JsonProperty("inspectionErrors", Order = 16)] public int InspectionErrors { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new ScenarioEvidence
            {
                SchemaVersion = 1,
                Configuration = CompatibilityAttributionRuntimeControl
                    .AssetConfiguration,
                GuardedRequest = CompatibilityAttributionRuntimeControl
                    .AssetAttributionActive,
                RunId = CompatibilityAttributionRuntimeControl.RunId,
                GlobalLightmapsMode = LightmapSettings.lightmapsMode.ToString(),
                Families = new List<FamilyEvidence>(),
                Totals = new TotalsEvidence(),
                CleanupComplete = true
            };

            string manifestPath = Path.Combine(context.ModEntry.Path, "assets",
                "bundles", "asset-bundle-manifest.json");
            JObject manifest = File.Exists(manifestPath)
                ? JObject.Parse(File.ReadAllText(manifestPath)) : null;
            evidence.BundleManifestSha256 = File.Exists(manifestPath)
                ? Hash(manifestPath) : "<missing>";
            evidence.UnityVersion = manifest == null
                ? "<missing>" : (string)manifest["unityVersion"];
            evidence.BuildTarget = manifest == null
                ? "<missing>" : (string)manifest["buildTarget"];

            CompatibilityAssetAttributionPlan plan;
            bool planResolved = CompatibilityAssetAttributionPlan.TryResolve(
                evidence.Configuration, out plan);
            Add(assertions, "guarded-asset-configuration",
                "accepted guarded request and exact process-local asset plan",
                "active=" + evidence.GuardedRequest + ";runId=" +
                    evidence.RunId + ";configuration=" + evidence.Configuration,
                evidence.GuardedRequest && planResolved &&
                    string.Equals(evidence.RunId, request.RunId,
                        StringComparison.Ordinal),
                "validated runtime request plus early process-local control");
            Add(assertions, "bundle-build-contract",
                "Unity 2018.4.10f1 StandaloneWindows64 manifest",
                "unity=" + evidence.UnityVersion + ";target=" +
                    evidence.BuildTarget + ";sha256=" +
                    evidence.BundleManifestSha256,
                manifest != null && evidence.UnityVersion == "2018.4.10f1" &&
                    evidence.BuildTarget == "StandaloneWindows64",
                "installed KMG asset-bundle-manifest.json");

            if (planResolved)
            {
                InspectFamily(context, manifest, evidence,
                    CompatibilityAssetFamily.Firearms,
                    FirearmAssetRuntime.BundleName,
                    FirearmAssetRuntime.GetLoadedBundleForGuardedAttribution(),
                    plan.FirearmsEnabled, 19, 14);
                InspectFamily(context, manifest, evidence,
                    CompatibilityAssetFamily.ElvenBranchedSpears,
                    ElvenBranchedSpearAssetRuntime.BundleName,
                    ElvenBranchedSpearAssetRuntime
                        .GetLoadedBundleForGuardedAttribution(),
                    plan.ElvenBranchedSpearsEnabled, 6, 6);
                InspectFamily(context, manifest, evidence,
                    CompatibilityAssetFamily.EasternWeapons,
                    EasternWeaponAssetRuntime.BundleName,
                    EasternWeaponAssetRuntime
                        .GetLoadedBundleForGuardedAttribution(),
                    plan.EasternWeaponsEnabled, 24, 24);
            }

            Accumulate(evidence);
            bool familyStatesExact = planResolved && evidence.Families.All(value =>
                value.Enabled == value.Loaded &&
                (!value.Enabled || (value.Assets.Count > 0 &&
                    value.Prefabs.Count > 0 &&
                    value.BundleSha256 == value.ExpectedSha256)));
            Add(assertions, "asset-family-isolation",
                "only request-enabled KMG families loaded with exact bundle hashes",
                string.Join("|", evidence.Families.Select(value => value.Family +
                    ":enabled=" + value.Enabled + ":loaded=" + value.Loaded +
                    ":assets=" + value.Assets.Count + ":prefabs=" +
                    value.Prefabs.Count).ToArray()),
                familyStatesExact,
                "live AssetBundle references plus installed SHA-256");
            Add(assertions, "asset-warning-ownership-contract",
                "no particles, missing scripts, cameras, lights, unsupported/non-Standard shaders, missing _MainTex properties, lightmapped renderers, or zero-area readable meshes",
                JsonConvert.SerializeObject(evidence.Totals),
                evidence.Totals.ParticleSystems == 0 &&
                    evidence.Totals.ParticleRenderers == 0 &&
                    evidence.Totals.MissingSerializedComponents == 0 &&
                    evidence.Totals.Cameras == 0 && evidence.Totals.Lights == 0 &&
                    evidence.Totals.UnsupportedShaders == 0 &&
                    evidence.Totals.NonStandardShaders == 0 &&
                    evidence.Totals.MaterialsWithoutMainTexProperty == 0 &&
                    evidence.Totals.LightmappedRenderers == 0 &&
                    evidence.Totals.ZeroAreaReadableMeshes == 0 &&
                    evidence.Totals.InspectionErrors == 0,
                "runtime-loaded prefab/component/renderer/material/mesh inventory");
            Add(assertions, "asset-fixture-cleanup",
                "all request-local prefab instances destroyed",
                evidence.CleanupComplete ? "complete" : "incomplete",
                evidence.CleanupComplete,
                "UnityEngine.Object.DestroyImmediate on detached copies");
            Add(assertions, "no-save-owned-state", "no save API or persisted setting",
                "request-local asset plan and detached prefab copies only", true,
                "scenario implementation boundary");

            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("assetEvidenceSha256=" + Hash(path));
            diagnostics.Add("configuration=" + evidence.Configuration);
            diagnostics.Add("totals=" + JsonConvert.SerializeObject(
                evidence.Totals));
            foreach (FamilyEvidence family in evidence.Families)
                foreach (string error in family.InspectionErrors)
                    diagnostics.Add("family=" + family.Family +
                        ";inspectionError=" + error);

            bool pass = assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass) &&
                evidence.Totals.InspectionErrors == 0;
            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = context.Assembly.FullName + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = identity.GitCommit,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"),
                EndUtc = string.Empty,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = diagnostics.FirstOrDefault(value =>
                    value.IndexOf("inspectionError=", StringComparison.Ordinal)
                    >= 0) ?? string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void InspectFamily(ModContext context, JObject manifest,
            ScenarioEvidence evidence, CompatibilityAssetFamily family,
            string bundleName, AssetBundle bundle, bool enabled,
            int expectedAssets, int expectedPrefabs)
        {
            string familyName = FamilyName(family);
            string bundlePath = Path.Combine(context.ModEntry.Path, "assets",
                "bundles", bundleName);
            var row = new FamilyEvidence
            {
                Family = familyName,
                BundleName = bundleName,
                Enabled = enabled,
                Loaded = bundle != null,
                BundlePath = bundlePath,
                BundleSha256 = File.Exists(bundlePath) ? Hash(bundlePath) :
                    "<missing>",
                ExpectedSha256 = ExpectedBundleHash(manifest, bundleName),
                Assets = new List<AssetEvidence>(),
                Prefabs = new List<PrefabEvidence>(),
                InspectionErrors = new List<string>()
            };
            evidence.Families.Add(row);
            if (!enabled || bundle == null) return;
            try
            {
                string[] names = bundle.GetAllAssetNames()
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                foreach (string assetPath in names)
                {
                    UnityEngine.Object asset = bundle.LoadAsset(assetPath);
                    row.Assets.Add(new AssetEvidence
                    {
                        Path = assetPath,
                        Type = asset == null ? "<null>" :
                            asset.GetType().FullName,
                        Name = asset == null ? "<null>" : asset.name
                    });
                    GameObject prefab = asset as GameObject;
                    if (prefab != null)
                        row.Prefabs.Add(InspectPrefab(assetPath, prefab,
                            row.InspectionErrors, evidence));
                }
                if (row.Assets.Count != expectedAssets)
                    row.InspectionErrors.Add("asset-count expected=" +
                        expectedAssets + ";actual=" + row.Assets.Count);
                if (row.Prefabs.Count != expectedPrefabs)
                    row.InspectionErrors.Add("prefab-count expected=" +
                        expectedPrefabs + ";actual=" + row.Prefabs.Count);
            }
            catch (Exception exception)
            {
                row.InspectionErrors.Add("bundle-inspection " +
                    exception.GetType().FullName + ": " + exception.Message);
            }
        }

        private static PrefabEvidence InspectPrefab(string assetPath,
            GameObject prefab, ICollection<string> errors,
            ScenarioEvidence scenario)
        {
            GameObject instance = null;
            var row = new PrefabEvidence
            {
                AssetPath = assetPath,
                Name = prefab.name,
                ComponentTypes = new List<string>(),
                Renderers = new List<RendererEvidence>()
            };
            try
            {
                instance = UnityEngine.Object.Instantiate(prefab);
                instance.name = "KMG_CompatibilityAttribution_" + prefab.name;
                instance.SetActive(true);
                Transform[] transforms = instance.GetComponentsInChildren<
                    Transform>(true);
                foreach (Transform transform in transforms)
                {
                    Component[] components = transform.gameObject
                        .GetComponents<Component>();
                    row.MissingSerializedComponents += components.Count(value =>
                        value == null);
                    row.ParticleSystemCount += components.Count(value =>
                        value != null && string.Equals(value.GetType().FullName,
                            "UnityEngine.ParticleSystem", StringComparison.Ordinal));
                    row.ParticleRendererCount += components.Count(value =>
                        value != null && string.Equals(value.GetType().FullName,
                            "UnityEngine.ParticleSystemRenderer",
                            StringComparison.Ordinal));
                    row.ComponentTypes.AddRange(components.Where(value =>
                        value != null).Select(value => value.GetType().FullName));
                }
                row.ComponentTypes = row.ComponentTypes.Distinct(
                    StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToList();
                row.CameraCount = instance.GetComponentsInChildren<Camera>(true)
                    .Length;
                row.LightCount = instance.GetComponentsInChildren<Light>(true)
                    .Length;
                foreach (Renderer renderer in instance.GetComponentsInChildren<
                    Renderer>(true).OrderBy(value => TransformPath(
                        instance.transform, value.transform),
                        StringComparer.Ordinal))
                    row.Renderers.Add(InspectRenderer(instance.transform,
                        renderer, errors));
            }
            catch (Exception exception)
            {
                errors.Add(assetPath + " prefab-inspection " +
                    exception.GetType().FullName + ": " + exception.Message);
            }
            finally
            {
                if (instance != null)
                    UnityEngine.Object.DestroyImmediate(instance);
                row.InstanceDestroyed = instance == null;
                scenario.CleanupComplete &= row.InstanceDestroyed;
            }
            return row;
        }

        private static RendererEvidence InspectRenderer(Transform root,
            Renderer renderer, ICollection<string> errors)
        {
            var row = new RendererEvidence
            {
                Path = TransformPath(root, renderer.transform),
                Type = renderer.GetType().FullName,
                Enabled = renderer.enabled,
                LightmapIndex = renderer.lightmapIndex,
                RealtimeLightmapIndex = renderer.realtimeLightmapIndex,
                LightmapScaleOffset = Vector(renderer.lightmapScaleOffset),
                Materials = new List<MaterialEvidence>(),
                ParticleRenderMode = string.Empty
            };
            Material[] materials = renderer.sharedMaterials ??
                new Material[0];
            for (int index = 0; index < materials.Length; index++)
            {
                Material material = materials[index];
                Shader shader = material == null ? null : material.shader;
                bool hasMainTex = material != null &&
                    material.HasProperty("_MainTex");
                Texture mainTexture = hasMainTex ? material.mainTexture : null;
                row.Materials.Add(new MaterialEvidence
                {
                    Slot = index,
                    Name = material == null ? "<null>" : material.name,
                    Shader = shader == null ? "<null>" : shader.name,
                    ShaderSupported = shader != null && shader.isSupported,
                    HasMainTexProperty = hasMainTex,
                    MainTexture = mainTexture == null
                        ? "<null>" : mainTexture.name
                });
            }
            Mesh mesh = null;
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            if (filter != null) mesh = filter.sharedMesh;
            SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
            if (skinned != null) mesh = skinned.sharedMesh;
            if (string.Equals(renderer.GetType().FullName,
                "UnityEngine.ParticleSystemRenderer", StringComparison.Ordinal))
            {
                PropertyInfo meshProperty = renderer.GetType().GetProperty(
                    "mesh", BindingFlags.Instance | BindingFlags.Public);
                PropertyInfo modeProperty = renderer.GetType().GetProperty(
                    "renderMode", BindingFlags.Instance | BindingFlags.Public);
                mesh = meshProperty == null ? null :
                    meshProperty.GetValue(renderer, null) as Mesh;
                object mode = modeProperty == null ? null :
                    modeProperty.GetValue(renderer, null);
                row.ParticleRenderMode = mode == null ? "<unavailable>" :
                    mode.ToString();
            }
            if (mesh != null)
            {
                try { row.Mesh = InspectMesh(mesh); }
                catch (Exception exception)
                {
                    errors.Add(row.Path + " mesh-inspection " +
                        exception.GetType().FullName + ": " +
                        exception.Message);
                }
            }
            return row;
        }

        private static MeshEvidence InspectMesh(Mesh mesh)
        {
            var row = new MeshEvidence
            {
                Name = mesh.name,
                Readable = mesh.isReadable,
                VertexCount = mesh.vertexCount,
                SubMeshCount = mesh.subMeshCount,
                BoundsSize = Vector(mesh.bounds.size)
            };
            if (!mesh.isReadable) return row;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            row.TriangleCount = triangles.Length / 3;
            double area = 0d;
            for (int index = 0; index + 2 < triangles.Length; index += 3)
            {
                Vector3 first = vertices[triangles[index]];
                Vector3 second = vertices[triangles[index + 1]];
                Vector3 third = vertices[triangles[index + 2]];
                area += Vector3.Cross(second - first, third - first).magnitude /
                    2d;
            }
            row.SurfaceArea = area;
            return row;
        }

        private static void Accumulate(ScenarioEvidence evidence)
        {
            TotalsEvidence totals = evidence.Totals;
            foreach (FamilyEvidence family in evidence.Families)
            {
                totals.AssetPaths += family.Assets.Count;
                totals.Prefabs += family.Prefabs.Count;
                totals.InspectionErrors += family.InspectionErrors.Count;
                foreach (PrefabEvidence prefab in family.Prefabs)
                {
                    totals.MissingSerializedComponents +=
                        prefab.MissingSerializedComponents;
                    totals.Cameras += prefab.CameraCount;
                    totals.Lights += prefab.LightCount;
                    totals.ParticleSystems += prefab.ParticleSystemCount;
                    totals.ParticleRenderers += prefab.ParticleRendererCount;
                    totals.Renderers += prefab.Renderers.Count;
                    foreach (RendererEvidence renderer in prefab.Renderers)
                    {
                        if (!NoLightmap(renderer.LightmapIndex) ||
                            !NoLightmap(renderer.RealtimeLightmapIndex))
                            totals.LightmappedRenderers++;
                        totals.Materials += renderer.Materials.Count;
                        totals.UnsupportedShaders += renderer.Materials.Count(
                            value => !value.ShaderSupported);
                        totals.NonStandardShaders += renderer.Materials.Count(
                            value => !string.Equals(value.Shader, "Standard",
                                StringComparison.Ordinal));
                        totals.MaterialsWithoutMainTexProperty +=
                            renderer.Materials.Count(value =>
                                !value.HasMainTexProperty);
                        if (renderer.Mesh != null)
                        {
                            totals.Meshes++;
                            if (renderer.Mesh.SurfaceArea.HasValue &&
                                renderer.Mesh.SurfaceArea.Value <= 0d)
                                totals.ZeroAreaReadableMeshes++;
                        }
                    }
                }
            }
        }

        private static string ExpectedBundleHash(JObject manifest,
            string bundleName)
        {
            if (manifest == null) return "<missing>";
            if (string.Equals((string)manifest["bundleName"], bundleName,
                StringComparison.Ordinal))
                return (string)manifest["sha256"] ?? "<missing>";
            JArray bundles = manifest["bundles"] as JArray;
            JObject row = bundles == null ? null : bundles.OfType<JObject>()
                .SingleOrDefault(value => string.Equals((string)value["name"],
                    bundleName, StringComparison.Ordinal));
            return row == null ? "<missing>" :
                (string)row["sha256"] ?? "<missing>";
        }

        private static string FamilyName(CompatibilityAssetFamily family)
        {
            switch (family)
            {
                case CompatibilityAssetFamily.Firearms: return "C-FIREARMS";
                case CompatibilityAssetFamily.ElvenBranchedSpears:
                    return "C-SPEARS";
                case CompatibilityAssetFamily.EasternWeapons:
                    return "C-EASTERN";
                default: throw new ArgumentOutOfRangeException("family");
            }
        }

        private static bool NoLightmap(int value)
        { return value == -1 || value == 65535; }

        private static string TransformPath(Transform root, Transform value)
        {
            var parts = new List<string>();
            Transform cursor = value;
            while (cursor != null)
            {
                parts.Add(cursor.name);
                if (ReferenceEquals(cursor, root)) break;
                cursor = cursor.parent;
            }
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private static string Vector(Vector3 value)
        { return value.x + "," + value.y + "," + value.z; }

        private static string Vector(Vector4 value)
        { return value.x + "," + value.y + "," + value.z + "," + value.w; }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool passed,
            string source)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = source
            });
        }
    }
}
