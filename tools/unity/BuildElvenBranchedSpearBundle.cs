using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildElvenBranchedSpearBundle
{
    private const string Bundle = "kingmakergunslinger.elvenbranchedspear";

    private sealed class Variant
    {
        internal Variant(string source, string root)
        {
            Source = "Assets/ElvenBranchedSpear/" + source;
            Root = root;
            HeldPrefab = "Assets/ElvenBranchedSpear/" + root + ".prefab";
            BackPrefab = "Assets/ElvenBranchedSpear/" + root + "Back.prefab";
        }
        internal string Source;
        internal string Root;
        internal string HeldPrefab;
        internal string BackPrefab;
    }

    private static readonly Variant[] Variants =
    {
        new Variant("elven-branched-spear.fbx", "ElvenBranchedSpear"),
        new Variant("elven-branched-spear-thorn.fbx",
            "ElvenBranchedSpearThorn"),
        new Variant("elven-branched-spear-crown.fbx",
            "ElvenBranchedSpearCrown")
    };

    public static void BuildBatch()
    {
        if (!Application.unityVersion.Equals("2018.4.10f1",
            StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Exact Unity 2018.4.10f1 is required; observed " +
                Application.unityVersion);
        foreach (Variant variant in Variants)
        {
            BuildPrefab(variant, false);
            BuildPrefab(variant, true);
        }
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.SaveAssets();
        string output = Path.GetFullPath(Path.Combine(Application.dataPath,
            "../Builds/Windows"));
        Directory.CreateDirectory(output);
        BuildPipeline.BuildAssetBundles(output,
            BuildAssetBundleOptions.ChunkBasedCompression |
            BuildAssetBundleOptions.DeterministicAssetBundle |
            BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildTarget.StandaloneWindows64);
        string bundle = Path.Combine(output, Bundle);
        if (!File.Exists(bundle) || new FileInfo(bundle).Length == 0)
            throw new InvalidOperationException(
                "Dedicated spear bundle was not produced.");
        Debug.Log("KMG_ELVEN_BRANCHED_SPEAR_BUNDLE path=" + bundle +
            ";prefabs=" + string.Join(",", Variants.Select(value =>
                value.HeldPrefab + "|" + value.BackPrefab).ToArray()) +
            ";unity=" +
            Application.unityVersion);
    }

    private static void BuildPrefab(Variant variant, bool back)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
            variant.Source);
        if (source == null) throw new FileNotFoundException(variant.Source);
        string rootName = variant.Root + (back ? "Back" : string.Empty);
        string prefabPath = back ? variant.BackPrefab : variant.HeldPrefab;
        GameObject root = new GameObject(rootName);
        try
        {
            GameObject visual = UnityEngine.Object.Instantiate(source,
                root.transform);
            visual.name = "Visual";
            Vector3 visualPosition = back
                ? new Vector3(0f, -0.18f, 0.06f) : Vector3.zero;
            Quaternion visualRotation = back
                ? Quaternion.AngleAxis(35f, Vector3.forward) *
                    Quaternion.Euler(-90f, 0f, 0f)
                : Quaternion.Euler(90f, 0f, 0f);
            visual.transform.localPosition = visualPosition;
            visual.transform.localRotation = visualRotation;
            visual.transform.localScale = Vector3.one;
            foreach (Camera value in visual.GetComponentsInChildren<Camera>(true))
                UnityEngine.Object.DestroyImmediate(value.gameObject);
            foreach (Light value in visual.GetComponentsInChildren<Light>(true))
                UnityEngine.Object.DestroyImmediate(value.gameObject);
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new InvalidOperationException(
                    variant.Root + " source has no renderer.");
            Shader standard = Shader.Find("Standard");
            if (standard == null) throw new InvalidOperationException(
                "Unity Standard shader is unavailable.");
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                for (int index = 0; index < materials.Length; index++)
                {
                    if (materials[index] == null)
                        throw new InvalidOperationException(
                            rootName + " source contains a null material.");
                    materials[index].shader = standard;
                    materials[index].SetFloat("_Mode", 0f);
                    EditorUtility.SetDirty(materials[index]);
                }
                renderer.sharedMaterials = materials;
            }
            AddAnchor(root, "Grip", TransformSourcePoint(visualPosition,
                visualRotation, 0f));
            AddAnchor(root, "SupportHandTarget", TransformSourcePoint(
                visualPosition, visualRotation, 0.37f));
            AddAnchor(root, "Tip", TransformSourcePoint(visualPosition,
                visualRotation, 1.14f));
            AddAnchor(root, "Butt", TransformSourcePoint(visualPosition,
                visualRotation, -1.14f));
            if (back) AddAnchor(root, "BackMount", Vector3.zero);
            Bounds bounds = CombinedBounds(renderers);
            bool heldBounds = !back && bounds.size.y >= 2.20f &&
                bounds.size.y <= 2.35f && bounds.size.y > bounds.size.x &&
                bounds.size.y > bounds.size.z;
            bool backBounds = back && bounds.size.x >= 1.20f &&
                bounds.size.y >= 1.75f && bounds.size.y > bounds.size.x &&
                bounds.size.z < 0.35f;
            if (!Finite(bounds.min) || !Finite(bounds.max) ||
                (!heldBounds && !backBounds))
                throw new InvalidOperationException(rootName +
                    " source bounds do not match the native Longspear -Y/2.28m forward or diagonal-back contract: " + bounds);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
        AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
        if (importer == null) throw new FileNotFoundException(prefabPath);
        importer.assetBundleName = Bundle;
    }

    private static Vector3 TransformSourcePoint(Vector3 position,
        Quaternion rotation, float sourceZ)
    {
        return position + rotation * new Vector3(0f, 0f, sourceZ);
    }

    private static void AddAnchor(GameObject root, string name, Vector3 position)
    {
        GameObject value = new GameObject(name);
        value.transform.SetParent(root.transform, false);
        value.transform.localPosition = position;
        value.transform.localRotation = Quaternion.identity;
        value.transform.localScale = Vector3.one;
    }

    private static Bounds CombinedBounds(Renderer[] renderers)
    {
        Bounds value = renderers[0].bounds;
        foreach (Renderer renderer in renderers.Skip(1))
            value.Encapsulate(renderer.bounds);
        return value;
    }

    private static bool Finite(Vector3 value)
    { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
    private static bool Finite(float value)
    { return !float.IsNaN(value) && !float.IsInfinity(value); }
}
