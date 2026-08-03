using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildFirearmBundles
{
    private const string Bundle = "kingmakergunslinger.firearms";

    public static void BuildBatch()
    {
        if (!Application.unityVersion.Equals("2018.4.10f1", StringComparison.Ordinal))
            throw new InvalidOperationException("Exact Unity 2018.4.10f1 is required; observed " + Application.unityVersion);
        CreatePrefab("Pistol", 0.75f);
        CreatePrefab("Musket", 1.55f);
        CreatePrefab("Blunderbuss", 1.25f);
        CreatePrefab("Revolver", 0.80f);
        CreatePrefab("Rifle", 1.55f);
        string[] approved = { "Assets/ApprovedModels/Pistol.prefab",
            "Assets/ApprovedModels/Musket.prefab", "Assets/ApprovedModels/Blunderbuss.prefab",
            "Assets/ApprovedModels/Revolver.prefab", "Assets/ApprovedModels/Rifle.prefab" };
        foreach (string path in approved)
        {
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

    private static void CreatePrefab(string name, float targetLength)
    {
        string folder = "Assets/ApprovedModels/" + name;
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { folder });
        if (modelGuids.Length != 1)
            throw new InvalidOperationException(name + " requires exactly one model; observed " + modelGuids.Length);
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
            AssetDatabase.GUIDToAssetPath(modelGuids[0]));
        GameObject root = new GameObject(name);
        GameObject visual = UnityEngine.Object.Instantiate(source, root.transform);
        visual.name = "Visual";
        foreach (Camera value in visual.GetComponentsInChildren<Camera>(true))
            UnityEngine.Object.DestroyImmediate(value.gameObject);
        foreach (Light value in visual.GetComponentsInChildren<Light>(true))
            UnityEngine.Object.DestroyImmediate(value.gameObject);
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) throw new InvalidOperationException(name + " has no renderer.");
        ApplyMaterials(name, renderers);
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
        Vector3 size = bounds.size;
        if (size.x >= size.y && size.x >= size.z)
            visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        else if (size.y >= size.x && size.y >= size.z)
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        bounds = CalculateBounds(renderers);
        float length = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        visual.transform.localScale = Vector3.one * (targetLength / length);
        bounds = CalculateBounds(renderers);
        visual.transform.position -= bounds.center;
        visual.transform.position += Vector3.forward * (bounds.extents.z * 0.45f);
        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(root.transform, false);
        muzzle.transform.localPosition = new Vector3(0f, 0f, targetLength * 0.55f);
        PrefabUtility.CreatePrefab("Assets/ApprovedModels/" + name + ".prefab", root,
            ReplacePrefabOptions.ReplaceNameBased);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static void ApplyMaterials(string family, Renderer[] renderers)
    {
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
                string path = materialFolder + "/" + family + "_" + Sanitize(key) + ".mat";
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
