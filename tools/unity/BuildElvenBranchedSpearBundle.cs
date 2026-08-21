using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Assets;
using UnityEditor;
using UnityEngine;

public static class BuildElvenBranchedSpearBundle
{
    private const string Bundle = "kingmakergunslinger.elvenbranchedspear";
    private const float ExpectedLength = 2.28f;
    private const float NativeSupportStationFromGrip = 0.593016f;
    private static readonly Vector3 NativeLongspearHeldEuler =
        new Vector3(9.712032f, 123.546196f, 178.825317f);
    private static readonly Vector3 NativeLongspearStoredPosition =
        new Vector3(-0.004000134f, -0.00700025726f, 0.213005632f);
    private static readonly Vector3 NativeLongspearStoredEuler =
        new Vector3(359.074829f, 290.676361f, 267.541138f);
    private static readonly Vector3 NativeLongspearStoredRendererCenter =
        new Vector3(0.000167660415f, -0.0002424717f,
            -0.0000419598073f);
    private static readonly string[] SourceMarkerNames =
    {
        "KMG_Grip", "KMG_Support", "KMG_Tip", "KMG_Butt",
        "KMG_HeadUp", "KMG_Back"
    };

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

    private sealed class SourceFrame
    {
        internal Vector3 Grip;
        internal Vector3 Support;
        internal Vector3 Tip;
        internal Vector3 Butt;
        internal Vector3 HeadUp;
        internal Vector3 Back;
        internal WeaponPresentationSemanticFrame Semantic;
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
            ";unity=" + Application.unityVersion);
    }

    private static void BuildPrefab(Variant variant, bool back)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
            variant.Source);
        if (source == null) throw new FileNotFoundException(variant.Source);
        SourceFrame sourceFrame = ResolveSourceFrame(source, variant);
        string rootName = variant.Root + (back ? "Back" : string.Empty);
        string prefabPath = back ? variant.BackPrefab : variant.HeldPrefab;
        GameObject root = new GameObject(rootName);
        try
        {
            Quaternion donorRotation = Quaternion.Euler(back ?
                NativeLongspearStoredEuler : NativeLongspearHeldEuler);
            Vector3 targetForward = donorRotation * Vector3.up;
            Vector3 targetHeadNormal = donorRotation * Vector3.right;
            Quaternion visualRotation =
                WeaponPresentationFrameContract.SolveRotation(
                    sourceFrame.Semantic, targetForward,
                    targetHeadNormal);
            Vector3 sourceAnchor = back ? sourceFrame.Back :
                sourceFrame.Grip;
            Vector3 targetAnchor = back ? StoredRendererAnchor() :
                Vector3.zero;
            Vector3 visualPosition =
                WeaponPresentationFrameContract.SolveTranslation(
                    visualRotation, 1f, sourceAnchor, targetAnchor);

            GameObject visual = UnityEngine.Object.Instantiate(source,
                root.transform);
            visual.name = "Visual";
            RemoveBuildMarkers(visual, rootName);
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
            ApplyMaterials(rootName, renderers);

            AddAnchor(root, WeaponPresentationFrameContract.GripMarker,
                TransformSourcePoint(visualPosition, visualRotation,
                    sourceFrame.Grip));
            AddAnchor(root, WeaponPresentationFrameContract.SupportMarker,
                TransformSourcePoint(visualPosition, visualRotation,
                    sourceFrame.Support));
            AddAnchor(root, "Tip", TransformSourcePoint(visualPosition,
                visualRotation, sourceFrame.Tip));
            AddAnchor(root, WeaponPresentationFrameContract.ButtMarker,
                TransformSourcePoint(visualPosition, visualRotation,
                    sourceFrame.Butt));
            AddAnchor(root, WeaponPresentationFrameContract.HeadUpMarker,
                TransformSourcePoint(visualPosition, visualRotation,
                    sourceFrame.HeadUp));
            if (back) AddAnchor(root, "BackMount", targetAnchor);

            WeaponPresentationSemanticFrame frame =
                WeaponPresentationFrameContract.Require(root.transform,
                    rootName, "Tip",
                    WeaponPresentationFrameContract.HeadUpMarker, true,
                    2.25f, 2.32f);
            if (Vector3.Dot(frame.Forward, targetForward.normalized) <
                    0.99999f ||
                Vector3.Dot(frame.Up, targetHeadNormal.normalized) <
                    0.99999f)
                throw new InvalidOperationException(rootName +
                    " does not match the measured native Longspear " +
                    (back ? "stored" : "held") + " semantic basis.");
            float supportStation = Vector3.Dot(frame.Support - frame.Grip,
                frame.Forward);
            if (Mathf.Abs(supportStation -
                    NativeSupportStationFromGrip) > 0.001f)
                throw new InvalidOperationException(rootName +
                    " support station does not match the measured native " +
                    "Longspear grip-to-IK interval: " +
                    supportStation.ToString("R"));
            WeaponPresentationProjection projection =
                WeaponPresentationFrameContract.ValidateRendererEndpoints(
                    root.transform, visual.transform, frame, rootName, 0.08f);
            WeaponPresentationFrameContract.ValidateSecondaryAsPlaneNormal(
                root.transform, visual.transform, frame, rootName, 0.20f);
            Bounds bounds = CombinedBounds(renderers);
            if (!Finite(bounds.min) || !Finite(bounds.max) ||
                projection.Span < 2.25f || projection.Span > 2.32f)
                throw new InvalidOperationException(rootName +
                    " source bounds do not match the renderer-grounded " +
                    "2.28m spear contract: " + bounds);
            Debug.Log("KMG_SPEAR_SEMANTIC_FRAME name=" + rootName +
                ";source=authored-KMG-markers;donor=native-Longspear-" +
                (back ? "stored" : "held") + ";forward=" +
                frame.Forward.ToString("R") + ";headNormal=" +
                frame.Up.ToString("R") + ";right=" +
                frame.Right.ToString("R") + ";grip=" +
                frame.Grip.ToString("R") + ";supportStation=" +
                supportStation.ToString("R") + ";targetAnchor=" +
                targetAnchor.ToString("R") + ";visualPosition=" +
                visualPosition.ToString("R") + ";visualEuler=" +
                visualRotation.eulerAngles.ToString("R") +
                ";rendererProjection=" + projection.Minimum.ToString("R") +
                ".." + projection.Maximum.ToString("R") + ";sources=" +
                projection.SourceCount);
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

    private static SourceFrame ResolveSourceFrame(GameObject source,
        Variant variant)
    {
        var markers = new Dictionary<string, Transform>(
            StringComparer.Ordinal);
        foreach (string name in SourceMarkerNames)
        {
            Transform[] matches = source.GetComponentsInChildren<Transform>(
                true).Where(value => value != null && value.name == name)
                .ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException(variant.Root +
                    " requires exactly one source-authored " + name +
                    "; observed=" + matches.Length + ".");
            markers.Add(name, matches[0]);
        }
        var frame = new SourceFrame
        {
            Grip = SourceLocalPoint(source, markers["KMG_Grip"]),
            Support = SourceLocalPoint(source, markers["KMG_Support"]),
            Tip = SourceLocalPoint(source, markers["KMG_Tip"]),
            Butt = SourceLocalPoint(source, markers["KMG_Butt"]),
            HeadUp = SourceLocalPoint(source, markers["KMG_HeadUp"]),
            Back = SourceLocalPoint(source, markers["KMG_Back"])
        };
        frame.Semantic = new WeaponPresentationSemanticFrame(frame.Grip,
            frame.Tip, frame.Butt, frame.HeadUp, true, frame.Support,
            frame.Tip - frame.Grip);
        float length = Vector3.Distance(frame.Tip, frame.Butt);
        float supportStation = Vector3.Dot(frame.Support - frame.Grip,
            frame.Semantic.Forward);
        if (!Finite(frame.Grip) || !Finite(frame.Support) ||
            !Finite(frame.Tip) || !Finite(frame.Butt) ||
            !Finite(frame.HeadUp) || !Finite(frame.Back) ||
            Vector3.Dot(frame.Semantic.Forward, Vector3.forward) < 0.99999f ||
            Vector3.Dot(frame.Semantic.Up, Vector3.up) < 0.99999f ||
            Mathf.Abs(length - ExpectedLength) > 0.001f ||
            Mathf.Abs(supportStation - NativeSupportStationFromGrip) >
                0.001f)
            throw new InvalidOperationException(variant.Root +
                " source-authored frame is not +Z physical-tip/+Y " +
                "head-normal with the measured support station.");
        Debug.Log("KMG_SPEAR_SOURCE_FRAME name=" + variant.Root +
            ";grip=" + frame.Grip.ToString("R") + ";support=" +
            frame.Support.ToString("R") + ";tip=" +
            frame.Tip.ToString("R") + ";butt=" +
            frame.Butt.ToString("R") + ";headUp=" +
            frame.HeadUp.ToString("R") + ";back=" +
            frame.Back.ToString("R") + ";length=" +
            length.ToString("R"));
        return frame;
    }

    private static Vector3 SourceLocalPoint(GameObject source,
        Transform marker)
    {
        return source.transform.InverseTransformPoint(marker.position);
    }

    private static void RemoveBuildMarkers(GameObject visual, string label)
    {
        Transform[] markers = visual.GetComponentsInChildren<Transform>(true)
            .Where(value => value != null &&
                SourceMarkerNames.Contains(value.name)).ToArray();
        if (markers.Length != SourceMarkerNames.Length)
            throw new InvalidOperationException(label +
                " instantiated source marker set is incomplete.");
        foreach (Transform marker in markers)
            UnityEngine.Object.DestroyImmediate(marker.gameObject);
    }

    private static void ApplyMaterials(string label, Renderer[] renderers)
    {
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
                        label + " source contains a null material.");
                materials[index].shader = standard;
                materials[index].SetFloat("_Mode", 0f);
                EditorUtility.SetDirty(materials[index]);
            }
            renderer.sharedMaterials = materials;
        }
    }

    private static Vector3 StoredRendererAnchor()
    {
        Quaternion rotation = Quaternion.Euler(NativeLongspearStoredEuler);
        return NativeLongspearStoredPosition +
            rotation * NativeLongspearStoredRendererCenter;
    }

    private static Vector3 TransformSourcePoint(Vector3 position,
        Quaternion rotation, Vector3 sourcePoint)
    {
        return position + rotation * sourcePoint;
    }

    private static void AddAnchor(GameObject root, string name,
        Vector3 position)
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
