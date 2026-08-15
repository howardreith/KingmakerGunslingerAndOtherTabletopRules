using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildEasternWeaponsBundle
{
    private const string Bundle = "kingmakergunslinger.easternweapons";

    private sealed class Weapon
    {
        internal Weapon(string key, string label, float support, float tip,
            float butt, float minimum, float maximum)
        { Key = key; Label = label; Support = support; Tip = tip; Butt = butt;
          Minimum = minimum; Maximum = maximum; }
        internal string Key, Label;
        internal float Support, Tip, Butt, Minimum, Maximum;
    }

    private static readonly Weapon[] Weapons =
    {
        new Weapon("wakizashi", "Wakizashi", 0.07f, 0.56f, -0.20f, 0.55f, 0.95f),
        new Weapon("wakizashi-petal", "WakizashiPetal", 0.07f, 0.56f, -0.20f, 0.55f, 0.95f),
        new Weapon("wakizashi-moon", "WakizashiMoon", 0.07f, 0.56f, -0.20f, 0.55f, 0.95f),
        new Weapon("wakizashi-capstone", "WakizashiCapstone", 0.07f, 0.56f, -0.20f, 0.55f, 0.95f),
        new Weapon("katana", "Katana", 0.10f, 0.76f, -0.29f, 0.85f, 1.25f),
        new Weapon("katana-reed", "KatanaReed", 0.10f, 0.76f, -0.29f, 0.85f, 1.25f),
        new Weapon("katana-regal", "KatanaRegal", 0.10f, 0.76f, -0.29f, 0.85f, 1.25f),
        new Weapon("katana-capstone", "KatanaCapstone", 0.10f, 0.76f, -0.29f, 0.85f, 1.25f),
        new Weapon("nodachi", "Nodachi", 0.13f, 1.16f, -0.42f, 1.30f, 1.90f),
        new Weapon("nodachi-cleaver", "NodachiCleaver", 0.13f, 1.16f, -0.42f, 1.30f, 1.90f),
        new Weapon("nodachi-titan", "NodachiTitan", 0.13f, 1.16f, -0.42f, 1.30f, 1.90f),
        new Weapon("nodachi-capstone", "NodachiCapstone", 0.13f, 1.16f, -0.42f, 1.30f, 1.90f),
    };

    public static void BuildBatch()
    {
        if (!Application.unityVersion.Equals("2018.4.10f1",
            StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Exact Unity 2018.4.10f1 is required; observed " +
                Application.unityVersion);
        var prefabPaths = new List<string>();
        foreach (Weapon weapon in Weapons) prefabPaths.Add(BuildPrefab(weapon));
        if (prefabPaths.Distinct(StringComparer.Ordinal).Count() != 12)
            throw new InvalidOperationException("Eastern prefab identities collided.");
        foreach (string path in prefabPaths)
        {
            AssetImporter importer = AssetImporter.GetAtPath(path);
            if (importer == null) throw new FileNotFoundException(path);
            importer.assetBundleName = Bundle;
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
            throw new InvalidOperationException("Eastern Weapons bundle was not produced.");
        Debug.Log("KMG_EASTERN_WEAPONS_BUNDLE path=" + bundle +
            ";prefabs=" + string.Join("|", prefabPaths.ToArray()) +
            ";unity=" + Application.unityVersion);
    }

    private static string BuildPrefab(Weapon weapon)
    {
        string sourcePath = "Assets/EasternWeapons/" + weapon.Key + ".fbx";
        string prefabPath = "Assets/EasternWeapons/" + weapon.Label + ".prefab";
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
        if (source == null) throw new FileNotFoundException(sourcePath);
        GameObject root = new GameObject(weapon.Label);
        try
        {
            GameObject visual = UnityEngine.Object.Instantiate(source, root.transform);
            visual.name = "Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            foreach (Camera value in visual.GetComponentsInChildren<Camera>(true))
                UnityEngine.Object.DestroyImmediate(value.gameObject);
            foreach (Light value in visual.GetComponentsInChildren<Light>(true))
                UnityEngine.Object.DestroyImmediate(value.gameObject);
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new InvalidOperationException(weapon.Label + " has no renderer.");
            Shader standard = Shader.Find("Standard");
            if (standard == null) throw new InvalidOperationException(
                "Unity Standard shader is unavailable.");
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                    throw new InvalidOperationException(weapon.Label +
                        " renderer has no material.");
                for (int index = 0; index < materials.Length; index++)
                {
                    if (materials[index] == null)
                        throw new InvalidOperationException(weapon.Label +
                            " contains a null material.");
                    materials[index].shader = standard;
                    materials[index].SetFloat("_Mode", 0f);
                    EditorUtility.SetDirty(materials[index]);
                }
                renderer.sharedMaterials = materials;
            }
            AddAnchor(root, "Grip", Vector3.zero);
            AddAnchor(root, "SupportHandTarget", new Vector3(0f, 0f, weapon.Support));
            AddAnchor(root, "Tip", new Vector3(0f, 0f, weapon.Tip));
            AddAnchor(root, "Butt", new Vector3(0f, 0f, weapon.Butt));
            Bounds bounds = CombinedBounds(renderers);
            if (!Finite(bounds.min) || !Finite(bounds.max) ||
                bounds.size.magnitude < weapon.Minimum ||
                bounds.size.magnitude > weapon.Maximum)
                throw new InvalidOperationException(weapon.Label +
                    " bounds are nonfinite or implausible: " + bounds);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
        return prefabPath;
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
        foreach (Renderer renderer in renderers.Skip(1)) value.Encapsulate(renderer.bounds);
        return value;
    }
    private static bool Finite(Vector3 value)
    { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
    private static bool Finite(float value)
    { return !float.IsNaN(value) && !float.IsInfinity(value); }
}
