using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Assets;
using UnityEditor;
using UnityEngine;

public static class BuildEasternWeaponsBundle
{
    private const string Bundle = "kingmakergunslinger.easternweapons";
    private const string CuttingEdgeMarker = "CuttingEdge";
    private const string StoredMountMarker = "StoredMount";
    private const float NodachiSupportStation = -0.169f;

    private static readonly string[] CommonSourceMarkers =
    {
        "KMG_Grip", "KMG_Tip", "KMG_Butt", "KMG_Forward",
        "KMG_BladeNormal", "KMG_Edge", "KMG_Stored"
    };

    private sealed class DonorFrame
    {
        internal DonorFrame(string name, Vector3 heldPosition,
            Vector3 heldEuler, Vector3 storedPosition, Vector3 storedEuler,
            Vector3 storedScale, Vector3 storedRendererCenter)
        {
            Name = name;
            HeldPosition = heldPosition;
            HeldEuler = heldEuler;
            StoredPosition = storedPosition;
            StoredEuler = storedEuler;
            StoredScale = storedScale;
            StoredRendererCenter = storedRendererCenter;
        }

        internal string Name;
        internal Vector3 HeldPosition;
        internal Vector3 HeldEuler;
        internal Vector3 StoredPosition;
        internal Vector3 StoredEuler;
        internal Vector3 StoredScale;
        internal Vector3 StoredRendererCenter;
    }

    private sealed class Weapon
    {
        internal Weapon(string key, string label, float minimum,
            float maximum, bool requiresSupport, DonorFrame donor)
        {
            Key = key;
            Label = label;
            Minimum = minimum;
            Maximum = maximum;
            RequiresSupport = requiresSupport;
            Donor = donor;
            SourcePath = "Assets/EasternWeapons/" + key + ".fbx";
            HeldPrefab = "Assets/EasternWeapons/" + label + ".prefab";
            StoredPrefab = "Assets/EasternWeapons/" + label +
                "Stored.prefab";
        }

        internal string Key;
        internal string Label;
        internal float Minimum;
        internal float Maximum;
        internal bool RequiresSupport;
        internal DonorFrame Donor;
        internal string SourcePath;
        internal string HeldPrefab;
        internal string StoredPrefab;
    }

    private sealed class SourceFrame
    {
        internal Vector3 Grip;
        internal Vector3 Tip;
        internal Vector3 Butt;
        internal Vector3 Forward;
        internal Vector3 BladeNormal;
        internal Vector3 CuttingEdge;
        internal Vector3 Stored;
        internal Vector3 Support;
        internal bool HasSupport;
        internal WeaponPresentationSemanticFrame Semantic;
    }

    private static readonly DonorFrame Scimitar = new DonorFrame(
        "native-Scimitar",
        new Vector3(-0.008432996f, -0.020028824f, 0.00105414f),
        new Vector3(353.967743f, 291.8186f, 184.377609f),
        new Vector3(-0.06271917f, -0.0416369066f, -0.118480414f),
        new Vector3(4.08384275f, 289.105225f, 289.6144f),
        new Vector3(1.05412471f, 1.05414915f, 1.05414057f),
        new Vector3(-0.000206507742f, 0.2642012f, -0.0341803059f));

    private static readonly DonorFrame BastardSword = new DonorFrame(
        "native-BastardSword", Vector3.zero,
        new Vector3(12.3281841f, 123.021339f, 178.818527f),
        new Vector3(0f, 0.00399958529f, -0.184981823f),
        new Vector3(356.3357f, 270f, 265.4645f),
        new Vector3(1.00008607f, 1.0000869f, 1.0000515f),
        new Vector3(0.0000006817281f, 0.462136239f,
            -0.000000476837158f));

    private static readonly DonorFrame Greatsword = new DonorFrame(
        "native-Greatsword", Vector3.zero,
        new Vector3(5.23646832f, 124.490906f, 179.792526f),
        new Vector3(0f, 0.0110000232f, -0.221000314f),
        new Vector3(353.2058f, 270.347778f, 267.0627f),
        new Vector3(1.00000072f, 1.00000215f, 1.00000143f),
        new Vector3(0.00370623916f, 0.5212074f, 0.004406467f));

    private static readonly Weapon[] Weapons =
    {
        W("wakizashi", "Wakizashi", 0.55f, 0.95f, false, Scimitar),
        W("wakizashi-petal", "WakizashiPetal", 0.55f, 0.95f, false,
            Scimitar),
        W("wakizashi-moon", "WakizashiMoon", 0.55f, 0.95f, false,
            Scimitar),
        W("wakizashi-capstone", "WakizashiCapstone", 0.55f, 0.95f,
            false, Scimitar),
        W("katana", "Katana", 0.85f, 1.25f, false, BastardSword),
        W("katana-reed", "KatanaReed", 0.85f, 1.25f, false,
            BastardSword),
        W("katana-regal", "KatanaRegal", 0.85f, 1.25f, false,
            BastardSword),
        W("katana-capstone", "KatanaCapstone", 0.85f, 1.25f, false,
            BastardSword),
        W("nodachi", "Nodachi", 1.30f, 1.90f, true, Greatsword),
        W("nodachi-cleaver", "NodachiCleaver", 1.30f, 1.90f, true,
            Greatsword),
        W("nodachi-titan", "NodachiTitan", 1.30f, 1.90f, true,
            Greatsword),
        W("nodachi-capstone", "NodachiCapstone", 1.30f, 1.90f, true,
            Greatsword)
    };

    private static Weapon W(string key, string label, float minimum,
        float maximum, bool requiresSupport, DonorFrame donor)
    {
        return new Weapon(key, label, minimum, maximum, requiresSupport,
            donor);
    }

    public static void BuildBatch()
    {
        if (!Application.unityVersion.Equals("2018.4.10f1",
            StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Exact Unity 2018.4.10f1 is required; observed " +
                Application.unityVersion);
        var prefabPaths = new List<string>();
        foreach (Weapon weapon in Weapons)
        {
            prefabPaths.Add(BuildPrefab(weapon, false));
            prefabPaths.Add(BuildPrefab(weapon, true));
        }
        if (prefabPaths.Count != 24 ||
            prefabPaths.Distinct(StringComparer.Ordinal).Count() != 24)
            throw new InvalidOperationException(
                "Eastern held/stored prefab identities collided.");
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
            throw new InvalidOperationException(
                "Eastern Weapons bundle was not produced.");
        Debug.Log("KMG_EASTERN_WEAPONS_BUNDLE path=" + bundle +
            ";prefabs=" + string.Join("|", prefabPaths.ToArray()) +
            ";unity=" + Application.unityVersion);
    }

    private static string BuildPrefab(Weapon weapon, bool stored)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
            weapon.SourcePath);
        if (source == null) throw new FileNotFoundException(weapon.SourcePath);
        SourceFrame sourceFrame = ResolveSourceFrame(source, weapon);
        string rootName = weapon.Label + (stored ? "Stored" : string.Empty);
        string prefabPath = stored ? weapon.StoredPrefab : weapon.HeldPrefab;
        GameObject root = new GameObject(rootName);
        try
        {
            Quaternion donorRotation = Quaternion.Euler(stored ?
                weapon.Donor.StoredEuler : weapon.Donor.HeldEuler);
            Vector3 targetForward = donorRotation * Vector3.up;
            Vector3 targetBladeNormal = donorRotation * Vector3.right;
            Vector3 targetCuttingEdge = donorRotation * Vector3.back;
            Quaternion visualRotation =
                WeaponPresentationFrameContract.SolveRotation(
                    sourceFrame.Semantic, targetForward,
                    targetBladeNormal);
            Vector3 sourceAnchor = stored ? sourceFrame.Stored :
                sourceFrame.Grip;
            Vector3 targetAnchor = stored ? StoredRendererAnchor(
                weapon.Donor) : weapon.Donor.HeldPosition;
            Vector3 visualPosition =
                WeaponPresentationFrameContract.SolveTranslation(
                    visualRotation, 1f, sourceAnchor, targetAnchor);

            GameObject visual = UnityEngine.Object.Instantiate(source,
                root.transform);
            visual.name = "Visual";
            RemoveBuildMarkers(visual, weapon, rootName);
            visual.transform.localPosition = visualPosition;
            visual.transform.localRotation = visualRotation;
            visual.transform.localScale = Vector3.one;
            foreach (Camera value in visual.GetComponentsInChildren<Camera>(true))
                UnityEngine.Object.DestroyImmediate(value.gameObject);
            foreach (Light value in visual.GetComponentsInChildren<Light>(true))
                UnityEngine.Object.DestroyImmediate(value.gameObject);
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new InvalidOperationException(rootName +
                    " source has no renderer.");
            ApplyMaterials(rootName, renderers);

            AddAnchor(root, WeaponPresentationFrameContract.GripMarker,
                TransformSourcePoint(visualPosition, visualRotation,
                    sourceFrame.Grip));
            AddAnchor(root, "Tip", TransformSourcePoint(visualPosition,
                visualRotation, sourceFrame.Tip));
            AddAnchor(root, WeaponPresentationFrameContract.ButtMarker,
                TransformSourcePoint(visualPosition, visualRotation,
                    sourceFrame.Butt));
            AddAnchor(root, WeaponPresentationFrameContract.WeaponForwardMarker,
                TransformSourcePoint(visualPosition, visualRotation,
                    sourceFrame.Forward));
            AddAnchor(root, WeaponPresentationFrameContract.BladeNormalMarker,
                TransformSourcePoint(visualPosition, visualRotation,
                    sourceFrame.BladeNormal));
            AddAnchor(root, CuttingEdgeMarker,
                TransformSourcePoint(visualPosition, visualRotation,
                    sourceFrame.CuttingEdge));
            if (!stored && sourceFrame.HasSupport)
                AddAnchor(root, WeaponPresentationFrameContract.SupportMarker,
                    TransformSourcePoint(visualPosition, visualRotation,
                        sourceFrame.Support));
            if (stored) AddAnchor(root, StoredMountMarker, targetAnchor);

            WeaponPresentationSemanticFrame frame = stored ||
                !weapon.RequiresSupport
                ? WeaponPresentationFrameContract.RequireWithForwardMarker(
                    root.transform, rootName, "Tip",
                    WeaponPresentationFrameContract.BladeNormalMarker,
                    WeaponPresentationFrameContract.WeaponForwardMarker,
                    false, weapon.Minimum, weapon.Maximum)
                : WeaponPresentationFrameContract.
                    RequireWithForwardMarkerAndButtSupport(root.transform,
                        rootName, "Tip",
                        WeaponPresentationFrameContract.BladeNormalMarker,
                        WeaponPresentationFrameContract.WeaponForwardMarker,
                        weapon.Minimum, weapon.Maximum);
            Transform edge = root.transform.Find(CuttingEdgeMarker);
            Vector3 edgeDirection = (edge.localPosition -
                frame.Grip).normalized;
            if (Vector3.Dot(frame.Forward, targetForward.normalized) <
                    0.99999f ||
                Vector3.Dot(frame.Up, targetBladeNormal.normalized) <
                    0.99999f ||
                Vector3.Dot(edgeDirection,
                    targetCuttingEdge.normalized) < 0.99999f ||
                Vector3.Dot(edgeDirection, -frame.Right) < 0.99999f)
                throw new InvalidOperationException(rootName +
                    " does not match the measured " + weapon.Donor.Name +
                    " " + (stored ? "stored" : "held") +
                    " blade basis.");
            if (!stored && weapon.RequiresSupport)
            {
                float supportStation = Vector3.Dot(frame.Support -
                    frame.Grip, frame.Forward);
                if (Mathf.Abs(supportStation - NodachiSupportStation) >
                    0.001f)
                    throw new InvalidOperationException(rootName +
                        " support station does not match the measured native Greatsword handle interval: " +
                        supportStation.ToString("R"));
            }
            WeaponPresentationProjection projection =
                WeaponPresentationFrameContract.ValidateRendererEndpoints(
                    root.transform, visual.transform, frame, rootName,
                    0.08f);
            WeaponPresentationFrameContract.ValidateSecondaryAsPlaneNormal(
                root.transform, visual.transform, frame, rootName, 0.20f);
            Bounds bounds = CombinedBounds(renderers);
            if (!Finite(bounds.min) || !Finite(bounds.max) ||
                projection.Span < weapon.Minimum ||
                projection.Span > weapon.Maximum)
                throw new InvalidOperationException(rootName +
                    " renderer bounds are nonfinite or implausible: " +
                    bounds);
            if (stored && Vector3.Distance(bounds.center, targetAnchor) >
                    0.015f)
                throw new InvalidOperationException(rootName +
                    " authored renderer center does not land on the measured native stored anchor: renderer=" +
                    bounds.center.ToString("R") + ";target=" +
                    targetAnchor.ToString("R"));
            if (!Approximately(root.transform.localPosition, Vector3.zero) ||
                !Approximately(root.transform.localRotation,
                    Quaternion.identity) ||
                !Approximately(root.transform.localScale, Vector3.one))
                throw new InvalidOperationException(rootName +
                    " equipment root is not identity-transformed.");

            Debug.Log("KMG_EASTERN_SEMANTIC_FRAME name=" + rootName +
                ";source=authored-KMG-markers;donor=" + weapon.Donor.Name +
                "-" + (stored ? "stored" : "held") + ";forward=" +
                frame.Forward.ToString("R") + ";bladeNormal=" +
                frame.Up.ToString("R") + ";cuttingEdge=" +
                edgeDirection.ToString("R") + ";grip=" +
                frame.Grip.ToString("R") + ";targetAnchor=" +
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
        return prefabPath;
    }

    private static SourceFrame ResolveSourceFrame(GameObject source,
        Weapon weapon)
    {
        var markers = new Dictionary<string, Transform>(
            StringComparer.Ordinal);
        foreach (string name in CommonSourceMarkers)
            markers.Add(name, RequireSourceMarker(source, weapon.Label,
                name));
        Transform support = FindSourceMarker(source, "KMG_Support",
            weapon.Label);
        if (weapon.RequiresSupport != (support != null))
            throw new InvalidOperationException(weapon.Label +
                " source support-marker requirement is inconsistent.");
        Vector3 grip = SourceLocalPoint(source, markers["KMG_Grip"]);
        Vector3 importedBladeNormal = SourceLocalPoint(source,
            markers["KMG_BladeNormal"]);
        var frame = new SourceFrame
        {
            Grip = grip,
            Tip = SourceLocalPoint(source, markers["KMG_Tip"]),
            Butt = SourceLocalPoint(source, markers["KMG_Butt"]),
            Forward = SourceLocalPoint(source, markers["KMG_Forward"]),
            // Blender's FBX export/import path reflects X while leaving the
            // +Z longitudinal and +Y marker coordinates numerically intact.
            // Reverse the oriented plane normal to restore a right-handed
            // semantic basis whose physical +X imported edge is -Right.
            BladeNormal = grip - (importedBladeNormal - grip),
            CuttingEdge = SourceLocalPoint(source, markers["KMG_Edge"]),
            Stored = SourceLocalPoint(source, markers["KMG_Stored"]),
            HasSupport = support != null,
            Support = support == null ? Vector3.zero :
                SourceLocalPoint(source, support)
        };
        frame.Semantic = new WeaponPresentationSemanticFrame(frame.Grip,
            frame.Tip, frame.Butt, frame.BladeNormal, frame.HasSupport,
            frame.Support, frame.Forward - frame.Grip);
        Vector3 edgeDirection = (frame.CuttingEdge - frame.Grip).normalized;
        float supportStation = frame.HasSupport ? Vector3.Dot(
            frame.Support - frame.Grip, frame.Semantic.Forward) : 0f;
        float length = Vector3.Distance(frame.Tip, frame.Butt);
        if (!Finite(frame.Grip) || !Finite(frame.Tip) ||
            !Finite(frame.Butt) || !Finite(frame.Forward) ||
            !Finite(frame.BladeNormal) || !Finite(frame.CuttingEdge) ||
            !Finite(frame.Stored) ||
            Vector3.Dot(frame.Semantic.Forward, Vector3.forward) <
                0.99999f ||
            Vector3.Dot(frame.Semantic.Up, Vector3.down) < 0.99999f ||
            Vector3.Dot(edgeDirection, Vector3.right) < 0.99999f ||
            Vector3.Dot(edgeDirection, -frame.Semantic.Right) < 0.99999f ||
            length < weapon.Minimum || length > weapon.Maximum ||
            (frame.HasSupport && Mathf.Abs(supportStation -
                NodachiSupportStation) > 0.001f))
            throw new InvalidOperationException(weapon.Label +
                " imported source frame is not +Z longitudinal/-Y oriented blade-normal/+X physical cutting-edge after the explicit Blender-to-Unity handedness conversion: grip=" +
                frame.Grip.ToString("R") + ";tip=" +
                frame.Tip.ToString("R") + ";butt=" +
                frame.Butt.ToString("R") + ";forwardMarker=" +
                frame.Forward.ToString("R") + ";bladeNormalMarker=" +
                frame.BladeNormal.ToString("R") + ";edgeMarker=" +
                frame.CuttingEdge.ToString("R") + ";stored=" +
                frame.Stored.ToString("R") + ";resolvedForward=" +
                frame.Semantic.Forward.ToString("R") +
                ";resolvedBladeNormal=" + frame.Semantic.Up.ToString("R") +
                ";importedBladeNormalMarker=" +
                importedBladeNormal.ToString("R") +
                ";resolvedEdge=" + edgeDirection.ToString("R") +
                ";length=" + length.ToString("R") +
                ";supportStation=" + supportStation.ToString("R") + ".");
        Debug.Log("KMG_EASTERN_SOURCE_FRAME name=" + weapon.Label +
            ";contract=Blender+Z/+Y/-X reflected by FBX import to Unity+Z/-Y-oriented-normal/+X-edge;grip=" +
            frame.Grip.ToString("R") + ";tip=" +
            frame.Tip.ToString("R") + ";butt=" +
            frame.Butt.ToString("R") + ";forward=" +
            frame.Semantic.Forward.ToString("R") + ";bladeNormal=" +
            frame.Semantic.Up.ToString("R") +
            ";importedBladeNormalMarker=" +
            importedBladeNormal.ToString("R") + ";cuttingEdge=" +
            edgeDirection.ToString("R") + ";stored=" +
            frame.Stored.ToString("R") + ";supportStation=" +
            supportStation.ToString("R") + ";length=" +
            length.ToString("R"));
        return frame;
    }

    private static Transform RequireSourceMarker(GameObject source,
        string label, string name)
    {
        Transform marker = FindSourceMarker(source, name, label);
        if (marker == null)
            throw new InvalidOperationException(label +
                " requires exactly one source-authored " + name +
                "; observed=0.");
        return marker;
    }

    private static Transform FindSourceMarker(GameObject source,
        string name, string label)
    {
        Transform[] matches = source.GetComponentsInChildren<Transform>(true)
            .Where(value => value != null && value.name == name).ToArray();
        if (matches.Length > 1)
            throw new InvalidOperationException(label +
                " requires at most one source-authored " + name +
                "; observed=" + matches.Length + ".");
        return matches.Length == 0 ? null : matches[0];
    }

    private static void RemoveBuildMarkers(GameObject visual, Weapon weapon,
        string label)
    {
        var names = new HashSet<string>(CommonSourceMarkers,
            StringComparer.Ordinal);
        names.Add("KMG_Support");
        Transform[] markers = visual.GetComponentsInChildren<Transform>(true)
            .Where(value => value != null && names.Contains(value.name))
            .ToArray();
        int expected = CommonSourceMarkers.Length +
            (weapon.RequiresSupport ? 1 : 0);
        if (markers.Length != expected)
            throw new InvalidOperationException(label +
                " instantiated source marker set is incomplete: expected=" +
                expected + ";observed=" + markers.Length + ".");
        foreach (Transform marker in markers)
            UnityEngine.Object.DestroyImmediate(marker.gameObject);
    }

    private static Vector3 StoredRendererAnchor(DonorFrame donor)
    {
        Quaternion rotation = Quaternion.Euler(donor.StoredEuler);
        return donor.StoredPosition + rotation * Vector3.Scale(
            donor.StoredRendererCenter, donor.StoredScale);
    }

    private static Vector3 SourceLocalPoint(GameObject source,
        Transform marker)
    {
        return source.transform.InverseTransformPoint(marker.position);
    }

    private static Vector3 TransformSourcePoint(Vector3 position,
        Quaternion rotation, Vector3 sourcePoint)
    {
        return position + rotation * sourcePoint;
    }

    private static void ApplyMaterials(string label, Renderer[] renderers)
    {
        Shader standard = Shader.Find("Standard");
        if (standard == null) throw new InvalidOperationException(
            "Unity Standard shader is unavailable.");
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
                throw new InvalidOperationException(label +
                    " renderer has no material.");
            for (int index = 0; index < materials.Length; index++)
            {
                if (materials[index] == null)
                    throw new InvalidOperationException(label +
                        " contains a null material.");
                materials[index].shader = standard;
                materials[index].SetFloat("_Mode", 0f);
                EditorUtility.SetDirty(materials[index]);
            }
            renderer.sharedMaterials = materials;
        }
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

    private static bool Approximately(Vector3 left, Vector3 right)
    {
        return (left - right).sqrMagnitude <= 0.000001f;
    }

    private static bool Approximately(Quaternion left, Quaternion right)
    {
        return Mathf.Abs(Quaternion.Dot(left, right)) >= 0.999999f;
    }

    private static bool Finite(Vector3 value)
    {
        return Finite(value.x) && Finite(value.y) && Finite(value.z);
    }

    private static bool Finite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
