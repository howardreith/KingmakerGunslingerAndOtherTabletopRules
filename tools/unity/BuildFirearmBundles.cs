using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildFirearmBundles
{
    private const string Bundle = "kingmakergunslinger.firearms";

    internal sealed class FirearmPrefabSpec
    {
        internal string Name;
        internal string Family;
        internal bool IsBeltOrBackModel;
        internal bool RequiresTwoHandRig;
        internal Vector3 VisualPosition;
        internal Vector3 VisualEuler;
        internal float VisualScale;
        internal Vector3 MuzzlePosition;
        internal Vector3 MuzzleEuler;
        internal Vector3 SupportHandPosition;
        internal Vector3 SupportHandEuler;
        internal float ExpectedLengthMeters;
        internal float MinimumLengthMeters;
        internal float MaximumLengthMeters;
        internal string CandidateAnimation;
        internal string CalibrationStatus;
    }

    private static readonly FirearmPrefabSpec[] Specs =
    {
        Spec("Pistol", "Pistol", false, false,
            new Vector3(0f, 0f, 0.1632f), new Vector3(0f, 180f, 180f), 0.24f,
            new Vector3(0f, 0f, 0.264f), 0.264f, 0.15f, 0.45f,
            "Crossbow", "autonomous-candidate; equipped-visual-roll-corrected"),
        Spec("PistolBelt", "Pistol", true, false,
            Vector3.zero, new Vector3(0f, 90f, 90f), 0.24f,
            Vector3.zero, 0.264f, 0.15f, 0.45f,
            "None", "disabled; independent-holster-calibration-pending"),
        Spec("Musket", "Musket", false, true,
            new Vector3(0f, 0f, 0.35f), new Vector3(0f, 90f, 0f), 2.0f,
            new Vector3(0f, 0f, 0.8525f), 0.8525f, 0.55f, 1.60f,
            "Crossbow", "autonomous-calibration-candidate"),
        Spec("MusketBelt", "Musket", true, false,
            Vector3.zero, Vector3.zero, 2.0f, Vector3.zero,
            0.8525f, 0.55f, 1.60f, "None",
            "disabled; independent-back-calibration-pending"),
        Spec("Blunderbuss", "Blunderbuss", false, true,
            new Vector3(0f, 0f, 0.25f), new Vector3(0f, 90f, 0f), 0.5f,
            new Vector3(0f, 0f, 0.6875f), 0.6875f, 0.40f, 1.30f,
            "Crossbow", "autonomous-calibration-candidate"),
        Spec("BlunderbussBelt", "Blunderbuss", true, false,
            Vector3.zero, Vector3.zero, 0.5f, Vector3.zero,
            0.6875f, 0.40f, 1.30f, "None",
            "disabled; independent-back-calibration-pending"),
        Spec("Revolver", "Revolver", false, false,
            new Vector3(-0.0460553f, -0.1052241f, 0.1857974f),
            new Vector3(0f, 90f, 0f), 0.01719849f,
            new Vector3(0f, 0f, 0.264f), 0.264f, 0.15f, 0.45f,
            "Crossbow", "source-unit-normalization-required"),
        Spec("Rifle", "Rifle", false, true,
            new Vector3(0f, 0f, -0.651f), new Vector3(0f, 90f, 0f),
            1.5401387f, new Vector3(0f, 0f, 0.8525f),
            0.8525f, 0.55f, 1.60f, "Crossbow",
            "autonomous-calibration-candidate")
    };

    public static void BuildBatch()
    {
        if (!Application.unityVersion.Equals("2018.4.10f1", StringComparison.Ordinal))
            throw new InvalidOperationException("Exact Unity 2018.4.10f1 is required; observed " + Application.unityVersion);
        foreach (FirearmPrefabSpec spec in Specs) CreatePrefab(spec);
        foreach (FirearmPrefabSpec spec in Specs)
        {
            string path = "Assets/ApprovedModels/" + spec.Name + ".prefab";
            AssetImporter importer = AssetImporter.GetAtPath(path);
            if (importer == null) throw new FileNotFoundException(path);
            importer.assetBundleName = Bundle;
        }
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/ApprovedAudio" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter audio = (AudioImporter)AssetImporter.GetAtPath(path);
            AudioImporterSampleSettings settings = audio.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            audio.defaultSampleSettings = settings;
            audio.forceToMono = true;
            audio.preloadAudioData = true;
            audio.assetBundleName = Bundle;
            audio.SaveAndReimport();
        }
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.SaveAssets();
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/Windows"));
        Directory.CreateDirectory(output);
        BuildPipeline.BuildAssetBundles(output,
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle,
            BuildTarget.StandaloneWindows64);
    }

    private static FirearmPrefabSpec Spec(string name, string family,
        bool isBeltOrBackModel, bool requiresTwoHandRig, Vector3 visualPosition,
        Vector3 visualEuler, float visualScale, Vector3 muzzlePosition,
        float expectedLengthMeters, float minimumLengthMeters,
        float maximumLengthMeters, string candidateAnimation,
        string calibrationStatus)
    {
        return new FirearmPrefabSpec
        {
            Name = name,
            Family = family,
            IsBeltOrBackModel = isBeltOrBackModel,
            RequiresTwoHandRig = requiresTwoHandRig,
            VisualPosition = visualPosition,
            VisualEuler = visualEuler,
            VisualScale = visualScale,
            MuzzlePosition = muzzlePosition,
            MuzzleEuler = Vector3.zero,
            SupportHandPosition = requiresTwoHandRig
                ? new Vector3(0f, 0f, muzzlePosition.z * 0.55f)
                : Vector3.zero,
            SupportHandEuler = Vector3.zero,
            ExpectedLengthMeters = expectedLengthMeters,
            MinimumLengthMeters = minimumLengthMeters,
            MaximumLengthMeters = maximumLengthMeters,
            CandidateAnimation = candidateAnimation,
            CalibrationStatus = calibrationStatus
        };
    }

    private static void CreatePrefab(FirearmPrefabSpec spec)
    {
        ValidateSpec(spec);
        string folder = "Assets/ApprovedModels/" + spec.Family;
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { folder });
        if (modelGuids.Length != 1)
            throw new InvalidOperationException(spec.Family +
                " requires exactly one model; observed " + modelGuids.Length);
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(modelGuids[0]));
        GameObject root = new GameObject(spec.Name);
        GameObject visual = UnityEngine.Object.Instantiate(source, root.transform);
        visual.name = "Visual";
        foreach (Camera value in visual.GetComponentsInChildren<Camera>(true))
            UnityEngine.Object.DestroyImmediate(value.gameObject);
        foreach (Light value in visual.GetComponentsInChildren<Light>(true))
            UnityEngine.Object.DestroyImmediate(value.gameObject);
        if (!spec.IsBeltOrBackModel &&
            (spec.Family == "Musket" || spec.Family == "Blunderbuss"))
            RetainHighestDetailRenderers(visual, spec);
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            throw new InvalidOperationException(spec.Name + " has no renderer.");
        ApplyMaterials(spec, renderers);
        ValidateVisibleScales(visual, spec);
        LogRendererDiagnostics(visual, spec, renderers);
        visual.transform.localPosition = spec.VisualPosition;
        visual.transform.localRotation = Quaternion.Euler(spec.VisualEuler);
        visual.transform.localScale = Vector3.one * spec.VisualScale;
        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(root.transform, false);
        muzzle.transform.localPosition = spec.MuzzlePosition;
        muzzle.transform.localRotation = Quaternion.Euler(spec.MuzzleEuler);
        if (spec.RequiresTwoHandRig)
        {
            GameObject support = new GameObject("SupportHandTarget");
            support.transform.SetParent(root.transform, false);
            support.transform.localPosition = spec.SupportHandPosition;
            support.transform.localRotation = Quaternion.Euler(
                spec.SupportHandEuler);
        }
        if (spec.Name == "Pistol" &&
            (spec.VisualEuler.x != 0f || spec.VisualEuler.y != 180f ||
             spec.VisualEuler.z != 180f))
            throw new InvalidOperationException(
                "Pistol equipped Visual must carry the isolated 180-degree roll correction.");
        ValidateHierarchy(root, spec, renderers);
        PrefabUtility.CreatePrefab("Assets/ApprovedModels/" + spec.Name +
            ".prefab", root,
            ReplacePrefabOptions.ReplaceNameBased);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static void RetainHighestDetailRenderers(GameObject visual,
        FirearmPrefabSpec spec)
    {
        LODGroup[] groups = visual.GetComponentsInChildren<LODGroup>(true);
        int removedRenderers = 0;
        foreach (LODGroup group in groups)
        {
            LOD[] lods = group.GetLODs();
            var retained = new HashSet<Renderer>();
            if (lods.Length > 0 && lods[0].renderers != null)
                foreach (Renderer renderer in lods[0].renderers)
                    if (renderer != null) retained.Add(renderer);
            for (int index = 1; index < lods.Length; index++)
                if (lods[index].renderers != null)
                    foreach (Renderer renderer in lods[index].renderers)
                        if (renderer != null && !retained.Contains(renderer))
                        {
                            UnityEngine.Object.DestroyImmediate(renderer);
                            removedRenderers++;
                        }
            UnityEngine.Object.DestroyImmediate(group);
        }
        if (visual.GetComponentsInChildren<LODGroup>(true).Length != 0)
            throw new InvalidOperationException(spec.Name +
                " retains an LODGroup after highest-detail normalization.");
        Debug.Log("KMG_RIG_LOD name=" + spec.Name + ";groups=" +
            groups.Length + ";removedLowerDetailRenderers=" + removedRenderers +
            ";policy=retain-lod0-and-remove-lodgroup");
    }

    private static void ValidateVisibleScales(GameObject visual,
        FirearmPrefabSpec spec)
    {
        foreach (Transform child in visual.GetComponentsInChildren<Transform>(true))
        {
            Vector3 scale = child.localScale;
            if (!Finite(scale) || scale.x <= 0f || scale.y <= 0f || scale.z <= 0f)
                throw new InvalidOperationException(spec.Name +
                    " visible hierarchy contains a zero, negative, mirrored, or non-finite scale at " +
                    TransformPath(child, visual.transform) + ": " + scale.ToString("R"));
        }
    }

    private static void LogRendererDiagnostics(GameObject visual,
        FirearmPrefabSpec spec, Renderer[] renderers)
    {
        foreach (Renderer renderer in renderers)
        {
            Mesh mesh = null;
            SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
            if (skinned != null) mesh = skinned.sharedMesh;
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            if (mesh == null && filter != null) mesh = filter.sharedMesh;
            string[] materials = new string[renderer.sharedMaterials.Length];
            for (int index = 0; index < renderer.sharedMaterials.Length; index++)
            {
                Material material = renderer.sharedMaterials[index];
                materials[index] = material == null ? "<null>" :
                    material.name + "@" + (material.shader == null ?
                        "<null-shader>" : material.shader.name) +
                    "#cullPolicy=" + (material.shader != null &&
                        material.shader.name == "KingmakerGunslinger/DoubleSidedDiffuse"
                        ? "off" : "shader-default");
            }
            Debug.Log("KMG_RIG_RENDERER name=" + spec.Name + ";path=" +
                TransformPath(renderer.transform, visual.transform) +
                ";type=" + renderer.GetType().Name +
                ";enabled=" + renderer.enabled +
                ";active=" + renderer.gameObject.activeInHierarchy +
                ";mesh=" + (mesh == null ? "<null>" : mesh.name) +
                ";vertices=" + (mesh == null ? -1 : mesh.vertexCount) +
                ";normals=" + (mesh == null || mesh.normals == null ? -1 : mesh.normals.Length) +
                ";materials=" + string.Join("|", materials) +
                ";localScale=" + renderer.transform.localScale.ToString("R"));
        }
    }

    private static string TransformPath(Transform value, Transform root)
    {
        string path = value.name;
        while (value.parent != null && value != root)
        {
            value = value.parent;
            if (value != root) path = value.name + "/" + path;
        }
        return path;
    }

    private static void ValidateSpec(FirearmPrefabSpec spec)
    {
        if (spec == null || string.IsNullOrEmpty(spec.Name) ||
            string.IsNullOrEmpty(spec.Family))
            throw new InvalidOperationException("Every firearm rig needs identity.");
        if (!Finite(spec.VisualPosition) || !Finite(spec.VisualEuler) ||
            !Finite(spec.MuzzlePosition) || !Finite(spec.MuzzleEuler) ||
            !Finite(spec.SupportHandPosition) || !Finite(spec.SupportHandEuler) ||
            !Finite(spec.VisualScale) || spec.VisualScale <= 0f)
            throw new InvalidOperationException(spec.Name +
                " contains a non-finite or non-positive transform.");
        if (spec.MinimumLengthMeters <= 0f ||
            spec.ExpectedLengthMeters < spec.MinimumLengthMeters ||
            spec.ExpectedLengthMeters > spec.MaximumLengthMeters)
            throw new InvalidOperationException(spec.Name +
                " has an invalid expected-length contract.");
        if (!spec.IsBeltOrBackModel && spec.MuzzlePosition.z <= 0f)
            throw new InvalidOperationException(spec.Name +
                " muzzle must be forward of the grip on declared +Z.");
        if (spec.RequiresTwoHandRig &&
            (spec.SupportHandPosition.z <= 0f ||
             spec.SupportHandPosition.z >= spec.MuzzlePosition.z))
            throw new InvalidOperationException(spec.Name +
                " support target must lie between grip and muzzle.");
    }

    private static void ValidateHierarchy(GameObject root,
        FirearmPrefabSpec spec, Renderer[] renderers)
    {
        if (root.transform.localPosition != Vector3.zero ||
            root.transform.localRotation != Quaternion.identity ||
            root.transform.localScale != Vector3.one)
            throw new InvalidOperationException(spec.Name +
                " root is not an identity dominant-hand grip frame.");
        if (root.transform.Find("Visual") == null ||
            root.transform.Find("Muzzle") == null || renderers.Length == 0)
            throw new InvalidOperationException(spec.Name +
                " lacks its required visual or muzzle hierarchy.");
        bool hasSupport = root.transform.Find("SupportHandTarget") != null;
        if (hasSupport != spec.RequiresTwoHandRig)
            throw new InvalidOperationException(spec.Name +
                " support-target requirement does not match its rig family.");
        if (root.GetComponentsInChildren<Camera>(true).Length != 0 ||
            root.GetComponentsInChildren<Light>(true).Length != 0)
            throw new InvalidOperationException(spec.Name +
                " contains an unapproved camera or light.");
        if (root.GetComponentsInChildren<LODGroup>(true).Length != 0)
            throw new InvalidOperationException(spec.Name +
                " contains runtime LODGroup behavior.");
        foreach (Renderer renderer in renderers)
            foreach (Material material in renderer.sharedMaterials)
                if (material == null || material.shader == null)
                    throw new InvalidOperationException(spec.Name +
                        " contains a null material or shader.");
    }

    private static bool Finite(Vector3 value)
    {
        return Finite(value.x) && Finite(value.y) && Finite(value.z);
    }

    private static bool Finite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static void ApplyMaterials(FirearmPrefabSpec spec,
        Renderer[] renderers)
    {
        string family = spec.Family;
        bool doubleSidedHeldLongGun = !spec.IsBeltOrBackModel &&
            (family == "Musket" || family == "Blunderbuss");
        string materialFolder = "Assets/ApprovedModels/GeneratedMaterials";
        if (!AssetDatabase.IsValidFolder(materialFolder))
            AssetDatabase.CreateFolder("Assets/ApprovedModels", "GeneratedMaterials");
        foreach (Renderer renderer in renderers)
        {
            Material[] source = renderer.sharedMaterials;
            Material[] assigned = new Material[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                string key = source[index] == null ? "material" + index : source[index].name;
                string path = materialFolder + "/" + family +
                    (doubleSidedHeldLongGun ? "_Held_" : "_") +
                    Sanitize(key) + ".mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(Shader.Find("Standard"));
                    material.name = family + "_" + key;
                    Texture2D albedo = FindAlbedo(family, key);
                    if (albedo != null) material.SetTexture("_MainTex", albedo);
                    material.SetFloat("_Glossiness", 0.25f);
                    AssetDatabase.CreateAsset(material, path);
                }
                if (doubleSidedHeldLongGun)
                {
                    Shader doubleSided = Shader.Find(
                        "KingmakerGunslinger/DoubleSidedDiffuse");
                    if (doubleSided == null)
                        throw new InvalidOperationException(
                            "Bundled double-sided held-weapon shader is unavailable.");
                    material.shader = doubleSided;
                    EditorUtility.SetDirty(material);
                }
                assigned[index] = material;
            }
            renderer.sharedMaterials = assigned;
        }
    }

    private static Texture2D FindAlbedo(string family, string material)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] {
            "Assets/ApprovedModels/" + family });
        string needle = family == "Pistol" ? "albedo" :
            family == "Blunderbuss" ? "color" :
            family == "Musket" ? (material.Contains("10") ? "10_TXTR" : "07_TXTR") :
            family == "Rifle" ? "BaseColor" :
            (material.Contains("2") ? "Set2_BaseColor" : material.Contains("3") ?
                "Set3_BaseColor" : "Set1_BaseColor");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        return null;
    }

    private static string Sanitize(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars())
            value = value.Replace(invalid, '_');
        return value.Replace(' ', '_');
    }

    private static Bounds CalculateBounds(Renderer[] renderers)
    {
        Bounds result = renderers[0].bounds;
        foreach (Renderer renderer in renderers) result.Encapsulate(renderer.bounds);
        return result;
    }
}
