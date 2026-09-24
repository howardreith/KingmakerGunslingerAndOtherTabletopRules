using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds the Expanded Summoning Pteranodon bundle in Unity 2018.4.10f1.
///
/// The bundle ships exactly two assets: the mesh, and an ordered list of the
/// bone NAMES its vertex weights index. It deliberately ships no bone
/// transforms - the mesh's bind poses are normalised to identity here and
/// rebuilt at attach time from the live donor - which is both the
/// redistribution-safe choice and the only correct one.
///
/// Unity skins a vertex as
///     v_world = sum_i w_i * bones[i].localToWorldMatrix * bindposes[i] * v
/// so bind poses derived from the donor's pose at attach time would make the
/// mesh render as authored in whatever animation frame the unit happened to be
/// on. The donor's live pose differs from its bind pose by up to 3.954 units at
/// the wingtip - folded wings against spread ones - so that would misplace the
/// creature differently on every summon. Reusing the donor's own
/// sharedMesh.bindposes, which is the frame these vertices were authored in,
/// makes the binding deterministic.
///
/// There is deliberately no prefab. An earlier draft built a
/// SkinnedMeshRenderer prefab with an empty bones array and a placeholder
/// material, and nothing loaded it: the runtime reads the mesh and the name
/// list and builds its own renderer against the live donor. A prefab carrying a
/// bone-less skinned renderer is dead weight and a serialisation risk for no
/// benefit.
/// </summary>
public static class BuildPteranodonBundle
{
    private const string Bundle = "kingmakergunslinger.pteranodon";
    private const string SourceFbx = "Assets/Pteranodon/pteranodon.fbx";
    private const string MeshAssetPath = "Assets/Pteranodon/PteranodonMesh.asset";
    private const string BonesAssetPath = "Assets/Pteranodon/PteranodonBones.txt";

    /// <summary>
    /// The bones the mesh is allowed to bind to: six shared, and twenty per
    /// side - the wing chain, the leg, and eight toe bones.
    ///
    /// This is not redundant with the runtime's resolve step. That proves a name
    /// exists on the donor; this proves the generator has not started weighting
    /// geometry to a bone nobody reviewed. Absent on purpose: Tail_end, Tail_L,
    /// Tail_R and every *_end leaf. The donor's eagle tail fan carries no
    /// geometry, so a weight landing there would mean a fan had crept back in.
    /// </summary>
    private static readonly string[] AllowedBones =
    {
        "LowerTorso", "UpperTorso", "Neck", "Head", "Jaw", "Tail",
        "L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Feather_1", "L_Feather_2",
        "L_Feather_3", "L_Feather_4", "L_Feather_5", "L_Feather_6",
        "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0", "L_Finger_1_1",
        "L_Finger_1_2", "L_Finger_2_1", "L_Finger_2_2", "L_Finger_3_1",
        "L_Finger_3_2", "L_Finger_4_1", "L_Finger_4_2", "R_Arm_Upper",
        "R_Arm_Lower", "R_Palm", "R_Feather_1", "R_Feather_2", "R_Feather_3",
        "R_Feather_4", "R_Feather_5", "R_Feather_6", "R_Leg0_Upper",
        "R_Leg0_Lower", "R_Foot0", "R_Finger_1_1", "R_Finger_1_2",
        "R_Finger_2_1", "R_Finger_2_2", "R_Finger_3_1", "R_Finger_3_2",
        "R_Finger_4_1", "R_Finger_4_2"
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

        SkinnedMeshRenderer[] renderers =
            source.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (renderers.Length != 1) throw new InvalidOperationException(
            "The Pteranodon source must contain exactly one " +
            "SkinnedMeshRenderer; found " + renderers.Length + ".");

        string[] boneNames = BuildBoneNameList(renderers[0]);
        Mesh mesh = NormalisedMesh(renderers[0].sharedMesh, boneNames.Length);

        AssetDatabase.CreateAsset(mesh, MeshAssetPath);
        File.WriteAllText(Path.Combine(
            Path.GetDirectoryName(Application.dataPath), BonesAssetPath),
            string.Join("\n", boneNames));
        AssetDatabase.ImportAsset(BonesAssetPath);
        AssignBundle(MeshAssetPath);
        AssignBundle(BonesAssetPath);
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
            ";mesh=" + Path.GetFileNameWithoutExtension(MeshAssetPath) +
            ";bonesAsset=" + Path.GetFileNameWithoutExtension(BonesAssetPath) +
            ";boneCount=" + boneNames.Length +
            ";bones=" + string.Join(",", boneNames) +
            ";vertices=" + mesh.vertexCount +
            ";triangles=" + (mesh.triangles.Length / 3) +
            ";unity=" + Application.unityVersion);
    }

    private static void ConfigureImporter()
    {
        var importer = AssetImporter.GetAtPath(SourceFbx) as ModelImporter;
        if (importer == null) throw new FileNotFoundException(SourceFbx);
        // The runtime replaces bind poses on a copy of this mesh, which
        // requires it to be readable in a player build.
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
                "The mesh binds to bones outside the declared set: " +
                string.Join(", ", unexpected));
        if (names.Distinct(StringComparer.Ordinal).Count() != names.Count)
            throw new InvalidOperationException(
                "The Pteranodon bone list repeats a name.");
        return names.ToArray();
    }

    /// <summary>
    /// A copy of the source mesh with every bind pose replaced by the identity.
    /// Vertices, weights and bone order are untouched; only the donor's
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
                        "A vertex weight indexes a bone that does not exist.");
            }

            float total = values.Sum();
            if (Math.Abs(total - 1f) > 0.001f)
                throw new InvalidOperationException(
                    "A vertex weight does not sum to one: " + total);
        }

        Mesh mesh = UnityEngine.Object.Instantiate(source);
        mesh.name = "PteranodonMesh";
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

    private static void AssignBundle(string assetPath)
    {
        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null) throw new FileNotFoundException(assetPath);
        importer.assetBundleName = Bundle;
        importer.SaveAndReimport();
    }
}
