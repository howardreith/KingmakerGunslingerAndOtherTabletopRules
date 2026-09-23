using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds the Expanded Summoning Pteranodon bundle in Unity 2018.4.10f1.
///
/// The bundle ships one skinned prefab plus an ordered list of the bone NAMES
/// its vertex weights index. It deliberately ships no bone transforms: the mesh
/// bind poses are normalised to identity here and rebuilt at attach time from
/// the live donor, which is both the redistribution-safe choice and the only
/// correct one.
///
/// Unity skins a vertex as
///     v_world = sum_i w_i * bones[i].localToWorldMatrix * bindposes[i] * v
/// so bind poses derived from the donor's pose at attach time would make the
/// mesh render as authored in whatever animation frame the unit happened to be
/// on. The donor's live pose differs from its bind pose by up to 3.954 units at
/// the wingtip - folded wings against spread ones - so that would misplace the
/// membrane differently on every summon. Reusing the donor's own
/// sharedMesh.bindposes, which is the frame these vertices were authored in,
/// makes the binding deterministic.
/// </summary>
public static class BuildPteranodonBundle
{
    private const string Bundle = "kingmakergunslinger.pteranodon";
    private const string SourceFbx = "Assets/Pteranodon/pteranodon-membrane.fbx";
    private const string PrefabPath = "Assets/Pteranodon/PteranodonMembrane.prefab";
    private const string BonesAssetPath =
        "Assets/Pteranodon/PteranodonMembraneBones.txt";
    private const string PrefabName = "PteranodonMembrane";
    private const string BonesAssetName = "PteranodonMembraneBones";

    /// <summary>
    /// Bones the membrane is allowed to bind to. A weight landing anywhere else
    /// means the generator drifted, and the loader would then have to resolve a
    /// name whose role nobody checked.
    /// </summary>
    private static readonly string[] AllowedBones =
    {
        "L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Foot0",
        "L_Feather_1", "L_Feather_2", "L_Feather_3",
        "L_Feather_4", "L_Feather_5", "L_Feather_6",
        "R_Arm_Upper", "R_Arm_Lower", "R_Palm", "R_Foot0",
        "R_Feather_1", "R_Feather_2", "R_Feather_3",
        "R_Feather_4", "R_Feather_5", "R_Feather_6"
    };

    public static void BuildBatch()
    {
        if (!Application.unityVersion.Equals("2018.4.10f1",
            StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Exact Unity 2018.4.10f1 is required; observed " +
                Application.unityVersion);

        ConfigureImporter();
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(SourceFbx);
        if (source == null) throw new FileNotFoundException(SourceFbx);

        SkinnedMeshRenderer sourceRenderer = source
            .GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .SingleOrDefault();
        if (sourceRenderer == null) throw new InvalidOperationException(
            "The Pteranodon source must contain exactly one SkinnedMeshRenderer.");

        string[] boneNames = BuildBoneNameList(sourceRenderer);
        Mesh mesh = NormalisedMesh(sourceRenderer.sharedMesh, boneNames.Length);
        AssetDatabase.CreateAsset(mesh, "Assets/Pteranodon/PteranodonMembrane.asset");
        File.WriteAllText(Path.Combine(
            Path.GetDirectoryName(Application.dataPath), BonesAssetPath),
            string.Join("\n", boneNames));
        AssetDatabase.ImportAsset(BonesAssetPath);

        BuildPrefab(mesh, boneNames.Length);
        AssignBundle(PrefabPath);
        AssignBundle(BonesAssetPath);
        AssignBundle("Assets/Pteranodon/PteranodonMembrane.asset");

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
                "The Pteranodon bundle was not produced.");

        Debug.Log("KMG_PTERANODON_BUNDLE path=" + bundle +
            ";bytes=" + new FileInfo(bundle).Length +
            ";prefab=" + PrefabName +
            ";bonesAsset=" + BonesAssetName +
            ";bones=" + string.Join(",", boneNames) +
            ";vertices=" + mesh.vertexCount +
            ";triangles=" + (mesh.triangles.Length / 3) +
            ";unity=" + Application.unityVersion);
    }

    private static void ConfigureImporter()
    {
        var importer = AssetImporter.GetAtPath(SourceFbx) as ModelImporter;
        if (importer == null) throw new FileNotFoundException(SourceFbx);
        // The loader replaces bind poses on a copy of this mesh, which requires
        // it to be readable at runtime.
        importer.isReadable = true;
        importer.animationType = ModelImporterAnimationType.None;
        importer.importAnimation = false;
        importer.importCameras = false;
        importer.importLights = false;
        // Optimising the hierarchy discards bone names, which are the entire
        // binding contract with the donor.
        importer.optimizeGameObjects = false;
        importer.importNormals = ModelImporterNormals.Import;
        importer.SaveAndReimport();
    }

    private static string[] BuildBoneNameList(SkinnedMeshRenderer renderer)
    {
        Transform[] bones = renderer.bones;
        if (bones == null || bones.Length == 0)
            throw new InvalidOperationException(
                "The Pteranodon source mesh has no bones.");
        var names = new List<string>();
        foreach (Transform bone in bones)
        {
            if (bone == null) throw new InvalidOperationException(
                "The Pteranodon source mesh has a null bone.");
            names.Add(bone.name);
        }

        string[] unexpected = names.Where(value =>
            !AllowedBones.Contains(value, StringComparer.Ordinal)).ToArray();
        if (unexpected.Length != 0)
            throw new InvalidOperationException(
                "The membrane binds to bones outside the declared set: " +
                string.Join(", ", unexpected));
        if (names.Distinct(StringComparer.Ordinal).Count() != names.Count)
            throw new InvalidOperationException(
                "The Pteranodon bone list repeats a name.");
        return names.ToArray();
    }

    /// <summary>
    /// A copy of the source mesh with every bind pose replaced by the identity.
    ///
    /// The vertices, weights and bone order are untouched; only the donor's
    /// transforms are dropped, because the loader supplies the real matrices.
    /// </summary>
    private static Mesh NormalisedMesh(Mesh source, int boneCount)
    {
        if (source == null) throw new InvalidOperationException(
            "The Pteranodon source renderer has no mesh.");
        if (source.vertexCount == 0 || source.triangles.Length == 0)
            throw new InvalidOperationException(
                "The Pteranodon source mesh is empty.");
        BoneWeight[] weights = source.boneWeights;
        if (weights == null || weights.Length != source.vertexCount)
            throw new InvalidOperationException(
                "The Pteranodon source mesh is not fully weighted.");
        foreach (BoneWeight weight in weights)
        {
            int[] indexes = { weight.boneIndex0, weight.boneIndex1,
                weight.boneIndex2, weight.boneIndex3 };
            float[] values = { weight.weight0, weight.weight1,
                weight.weight2, weight.weight3 };
            for (int slot = 0; slot < 4; slot++)
            {
                if (values[slot] <= 0f) continue;
                if (indexes[slot] < 0 || indexes[slot] >= boneCount)
                    throw new InvalidOperationException(
                        "A Pteranodon vertex weight indexes a bone that does " +
                        "not exist.");
            }

            float total = values.Sum();
            if (Math.Abs(total - 1f) > 0.001f)
                throw new InvalidOperationException(
                    "A Pteranodon vertex weight does not sum to one: " + total);
        }

        Mesh mesh = UnityEngine.Object.Instantiate(source);
        mesh.name = PrefabName;
        var identity = new Matrix4x4[boneCount];
        for (int index = 0; index < boneCount; index++)
            identity[index] = Matrix4x4.identity;
        mesh.bindposes = identity;
        if (mesh.bindposes.Any(value => value != Matrix4x4.identity))
            throw new InvalidOperationException(
                "Donor bind poses survived normalisation; the bundle must not " +
                "carry them.");
        return mesh;
    }

    private static void BuildPrefab(Mesh mesh, int boneCount)
    {
        GameObject root = new GameObject(PrefabName);
        try
        {
            SkinnedMeshRenderer renderer =
                root.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = mesh;
            // No bones and no root bone: the loader resolves both against the
            // live donor rig. Shipping transforms here would be donor rig data.
            renderer.bones = new Transform[0];
            renderer.rootBone = null;
            renderer.updateWhenOffscreen = false;
            renderer.localBounds = mesh.bounds;

            Shader standard = Shader.Find("Standard");
            if (standard == null) throw new InvalidOperationException(
                "Unity Standard shader is unavailable.");
            Material material = new Material(standard);
            material.name = PrefabName + "Placeholder";
            // The runtime clones the donor's own material so the membrane is
            // shaded by the game's pipeline; this one only keeps the prefab
            // valid inside the bundle.
            AssetDatabase.CreateAsset(material,
                "Assets/Pteranodon/PteranodonMembranePlaceholder.mat");
            renderer.sharedMaterial = material;

            PrefabUtility.CreatePrefab(PrefabPath, root);
            AssignBundle("Assets/Pteranodon/PteranodonMembranePlaceholder.mat");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void AssignBundle(string assetPath)
    {
        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null)
            throw new FileNotFoundException(assetPath);
        importer.assetBundleName = Bundle;
        importer.SaveAndReimport();
    }
}
