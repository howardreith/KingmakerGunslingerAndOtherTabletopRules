using System;
using System.Collections.Generic;
using System.IO;
using KingmakerGunslinger.Assets;
using UnityEditor;
using UnityEngine;

public static class BuildFirearmBundles
{
    private const string Bundle = "kingmakergunslinger.firearms";
    private static readonly Vector3 NativeHeavyCrossbowHeldEuler =
        new Vector3(81.58254f, 6.878487f, 255.457428f);
    private static readonly Vector3 NativeHeavyCrossbowStoredPosition =
        new Vector3(-0.227002054f, -0.0360002033f, 0.111000687f);
    private static readonly Vector3 NativeHeavyCrossbowStoredEuler =
        new Vector3(29.35143f, 112.346809f, 16.69746f);
    private static readonly Vector3 NativeHeavyCrossbowStoredRendererCenter =
        new Vector3(-0.000450193882f, 0.008564681f, 0.328089476f);

    internal sealed class FirearmPrefabSpec
    {
        internal string Name;
        internal string Family;
        internal string SourceModelFile;
        internal bool IsBeltOrBackModel;
        internal bool RequiresTwoHandRig;
        internal Vector3 VisualPosition;
        internal Vector3 VisualEuler;
        internal float VisualScale;
        internal Vector3 MuzzlePosition;
        internal Vector3 MuzzleEuler;
        internal Vector3 GripPosition;
        internal Vector3 ButtPosition;
        internal Vector3 SourceUpAxis;
        internal Vector3 SourceForwardAxis;
        internal Vector3 TargetForwardAxis;
        internal Vector3 TargetUpAxis;
        internal bool UseSemanticBasis;
        internal Vector3 SupportHandPosition;
        internal Vector3 SupportHandEuler;
        internal bool HasSemanticAnchors;
        internal bool RequireSourceMarkers;
        internal bool RequireSourceFrameMarkers;
        internal bool DiagnosticOnly;
        internal Vector3 SourceGripPoint;
        internal Vector3 SourceSupportPoint;
        internal Vector3 SourceButtPoint;
        internal Vector3 SourceMuzzlePoint;
        internal Vector3 SourceBackPoint;
        internal Vector3 TargetAnchorPosition;
        internal bool HasBackAnchor;
        internal float ExpectedLengthMeters;
        internal float MinimumLengthMeters;
        internal float MaximumLengthMeters;
        internal string CandidateAnimation;
        internal string CalibrationStatus;
    }

    private static readonly FirearmPrefabSpec[] Specs =
    {
        BasisCalibrated(Anchored(Spec("Pistol", "Pistol", "model.dae", false,
            false, new Vector3(0f, 0f, 0.1632f),
            new Vector3(0f, 180f, 0f), 0.24f,
            new Vector3(0f, 0f, 0.4032f), 0.480f, 0.45f, 0.51f,
            "Crossbow", "basis-calibrated; renderer-endpoint-verified"),
            new Vector3(0f, 0f, 0.68f), Vector3.zero,
            new Vector3(0f, 0f, 1.00f),
            new Vector3(0f, 0f, -1.00f))),
        RootFramed(Spec("PistolBelt", "Pistol", "model.dae", true, false,
            Vector3.zero, new Vector3(0f, 90f, 90f), 0.24f,
            new Vector3(0f, 0f, 0.4032f), 0.480f, 0.45f, 0.51f,
            "None", "disabled; independent-holster-calibration-pending"),
            new Vector3(0.1632f, 0f, 0f),
            new Vector3(-0.2400f, 0f, 0f),
            new Vector3(0.2400f, 0f, 0f)),
        CanonicalLongGun(FrameAuthored(Anchored(Spec("Musket", "Musket",
            "musket-normalized.fbx", false, true,
            Vector3.zero, Vector3.zero, 1f,
            new Vector3(-0.002413f, 0.051845f, 1.045200f),
            1.342269f, 1.25f, 1.45f,
            "Crossbow", "canonical-source-frame; trigger-wrist-grip"),
            Vector3.zero, new Vector3(-0.031f, -0.051f, 0.374f),
            new Vector3(-0.002425f, -0.026154f, -0.294800f),
            new Vector3(-0.002413f, 0.051845f, 1.045200f)), false)),
        MarkerAuthored(Anchored(Spec("MusketPassThrough", "Musket",
            "musket-pass-through.fbx", false, true,
            Vector3.zero, new Vector3(0f, 90f, 0f), 4.186f,
            new Vector3(0f, 0f, 1.180452f), 1.349985f, 1.25f, 1.45f,
            "Crossbow", "diagnostic-pass-through; not-production-bound"),
            new Vector3(0.0400f, 0f, 0f),
            new Vector3(-0.1000f, -0.0122f, -0.0074f),
            new Vector3(0.0805f, 0f, 0f),
            new Vector3(-0.2420f, 0f, 0f)), true),
        MarkerAuthored(Anchored(Spec("MusketMinimalControl", "Musket",
            "musket-minimal-control.fbx", false, true,
            Vector3.zero, Vector3.zero, 1f,
            new Vector3(0f, 0f, 1.180452f), 1.349985f, 1.25f, 1.45f,
            "Crossbow", "diagnostic-minimal-control; not-production-bound"),
            Vector3.zero,
            new Vector3(-0.030976f, -0.051069f, 0.586040f),
            new Vector3(0f, 0f, -0.169533f),
            new Vector3(0f, 0f, 1.180452f)), true),
        MarkerAuthored(Anchored(Spec("MusketClearanceStock", "Musket",
            "musket-clearance-stock.fbx", false, true,
            Vector3.zero, Vector3.zero, 1f,
            new Vector3(0f, 0f, 1.180452f), 1.349985f, 1.25f, 1.45f,
            "Crossbow", "diagnostic-clearance-stock; not-production-bound"),
            Vector3.zero,
            new Vector3(-0.030976f, -0.051069f, 0.586040f),
            new Vector3(0f, 0f, -0.169533f),
            new Vector3(0f, 0f, 1.180452f)), true),
        BasisCalibrated(MarkerAuthored(Anchored(Spec("PistolDuelist", "Pistol",
            "pistol-duelist.fbx", false, false,
            Vector3.zero, Vector3.zero, 1f,
            new Vector3(0f, 0f, 0.264f), 0.339f, 0.30f, 0.40f,
            "PiercingOneHanded", "production-item-variant; exact-symbol-bound"),
            Vector3.zero,
            new Vector3(0f, -0.020f, 0.145f),
            new Vector3(0f, 0f, -0.075f),
            new Vector3(0f, 0f, 0.264f)), false)),
        BasisCalibrated(MarkerAuthored(Anchored(Spec("PistolLastWord", "Pistol",
            "pistol-last-word.fbx", false, false,
            Vector3.zero, Vector3.zero, 1f,
            new Vector3(0f, 0f, 0.264f), 0.339f, 0.30f, 0.40f,
            "PiercingOneHanded", "production-item-variant; exact-symbol-bound"),
            Vector3.zero,
            new Vector3(0f, -0.020f, 0.145f),
            new Vector3(0f, 0f, -0.075f),
            new Vector3(0f, 0f, 0.264f)), false)),
        StoredLongGun(BackAuthored(FrameAuthored(Anchored(Spec("MusketBelt", "Musket",
            "musket-normalized.fbx", true, false,
            Vector3.zero, NativeHeavyCrossbowStoredEuler, 1f,
            new Vector3(-0.002413f, 0.051845f, 1.045200f),
            1.342269f, 1.25f, 1.45f, "None",
            "native-heavy-crossbow-stored-basis; independent-back-anchor"),
            Vector3.zero, new Vector3(-0.031f, -0.051f, 0.374f),
            new Vector3(-0.002425f, -0.026154f, -0.294800f),
            new Vector3(-0.002413f, 0.051845f, 1.045200f)), false))),
        CanonicalLongGun(FrameAuthored(Anchored(Spec("Blunderbuss", "Blunderbuss",
            "blunderbuss-normalized.fbx", false, true,
            Vector3.zero, Vector3.zero, 1f,
            new Vector3(-0.002044f, 0.024475f, 0.627800f),
            0.862674f, 0.78f, 1.05f,
            "Crossbow", "canonical-source-frame; trigger-wrist-grip"),
            Vector3.zero, new Vector3(-0.031f, -0.051f, 0.36f),
            new Vector3(-0.002043f, -0.043402f, -0.232200f),
            new Vector3(-0.002044f, 0.024475f, 0.627800f)), false)),
        StoredLongGun(BackAuthored(FrameAuthored(Anchored(Spec("BlunderbussBelt", "Blunderbuss",
            "blunderbuss-normalized.fbx", true, false,
            Vector3.zero, NativeHeavyCrossbowStoredEuler, 1f,
            new Vector3(-0.002044f, 0.024475f, 0.627800f),
            0.862674f, 0.78f, 1.05f, "None",
            "native-heavy-crossbow-stored-basis; independent-back-anchor"),
            Vector3.zero, new Vector3(-0.031f, -0.051f, 0.36f),
            new Vector3(-0.002043f, -0.043402f, -0.232200f),
            new Vector3(-0.002044f, 0.024475f, 0.627800f)), false))),
        BasisCalibrated(Anchored(Spec("Revolver", "Revolver",
            "Final2 Sketchfab.fbx", false, false, Vector3.zero,
            new Vector3(0f, -90f, 0f), 0.01719849f,
            new Vector3(0f, 0.0593f, 0.2796f), 0.321f, 0.30f, 0.34f,
            "Crossbow", "basis-calibrated; component-bounds-derived"),
            new Vector3(-8.889382f, 7.50916529f, 2.68602586f),
            Vector3.zero,
            new Vector3(-10.9627813f, 7.50916529f, 2.68602586f),
            new Vector3(7.3696742f, 10.9590158f, 2.68599129f)),
            Vector3.right),
        CanonicalLongGun(FrameAuthored(Anchored(Spec("Rifle", "Rifle",
            "rifle-normalized.fbx", false, true,
            Vector3.zero, Vector3.zero, 1f,
            new Vector3(0f, 0.058063f, 0.679220f),
            1.011401f, 0.95f, 1.10f, "Crossbow",
            "canonical-source-frame; trigger-lever-wrist-grip"),
            Vector3.zero, new Vector3(-0.031f, -0.051f, 0.374f),
            new Vector3(0f, 0.004849f, -0.330780f),
            new Vector3(0f, 0.058063f, 0.679220f)), false)),
        StoredLongGun(BackAuthored(FrameAuthored(Anchored(Spec("RifleBelt",
            "Rifle", "rifle-normalized.fbx", true, false,
            Vector3.zero, NativeHeavyCrossbowStoredEuler, 1f,
            new Vector3(0f, 0.058063f, 0.679220f),
            1.011401f, 0.95f, 1.10f, "None",
            "native-heavy-crossbow-stored-basis; independent-back-anchor"),
            Vector3.zero, new Vector3(-0.031f, -0.051f, 0.374f),
            new Vector3(0f, 0.004849f, -0.330780f),
            new Vector3(0f, 0.058063f, 0.679220f)), false)))
    };

    private static FirearmPrefabSpec Anchored(FirearmPrefabSpec spec,
        Vector3 grip, Vector3 support, Vector3 butt, Vector3 muzzle)
    {
        spec.HasSemanticAnchors = true;
        spec.SourceGripPoint = grip;
        spec.SourceSupportPoint = support;
        spec.SourceButtPoint = butt;
        spec.SourceMuzzlePoint = muzzle;
        spec.SourceForwardAxis = (muzzle - grip).normalized;
        return spec;
    }

    private static FirearmPrefabSpec BasisCalibrated(
        FirearmPrefabSpec spec)
    {
        spec.UseSemanticBasis = true;
        return spec;
    }

    private static FirearmPrefabSpec BasisCalibrated(
        FirearmPrefabSpec spec, Vector3 sourceForwardAxis)
    {
        spec.SourceForwardAxis = sourceForwardAxis.normalized;
        return BasisCalibrated(spec);
    }

    private static FirearmPrefabSpec MarkerAuthored(FirearmPrefabSpec spec,
        bool diagnosticOnly)
    {
        spec.RequireSourceMarkers = true;
        spec.DiagnosticOnly = diagnosticOnly;
        return spec;
    }

    private static FirearmPrefabSpec FrameAuthored(FirearmPrefabSpec spec,
        bool diagnosticOnly)
    {
        MarkerAuthored(spec, diagnosticOnly);
        spec.RequireSourceFrameMarkers = true;
        return spec;
    }

    private static FirearmPrefabSpec CanonicalLongGun(
        FirearmPrefabSpec spec)
    {
        Quaternion donorRotation = Quaternion.Euler(
            NativeHeavyCrossbowHeldEuler);
        spec.VisualEuler = NativeHeavyCrossbowHeldEuler;
        spec.SourceForwardAxis = Vector3.forward;
        spec.SourceUpAxis = Vector3.up;
        spec.TargetForwardAxis = donorRotation * Vector3.forward;
        spec.TargetUpAxis = donorRotation * Vector3.up;
        return BasisCalibrated(spec);
    }

    private static FirearmPrefabSpec StoredLongGun(
        FirearmPrefabSpec spec)
    {
        Quaternion donorRotation = Quaternion.Euler(
            NativeHeavyCrossbowStoredEuler);
        spec.SourceForwardAxis = Vector3.forward;
        spec.SourceUpAxis = Vector3.up;
        spec.TargetForwardAxis = donorRotation * Vector3.forward;
        spec.TargetUpAxis = donorRotation * Vector3.up;
        spec.TargetAnchorPosition = NativeHeavyCrossbowStoredPosition +
            donorRotation * NativeHeavyCrossbowStoredRendererCenter;
        return BasisCalibrated(spec);
    }

    private static FirearmPrefabSpec BackAuthored(FirearmPrefabSpec spec)
    {
        spec.HasBackAnchor = true;
        return spec;
    }

    private static FirearmPrefabSpec RootFramed(FirearmPrefabSpec spec,
        Vector3 grip, Vector3 muzzle, Vector3 butt)
    {
        spec.GripPosition = grip;
        spec.MuzzlePosition = muzzle;
        spec.ButtPosition = butt;
        spec.SourceForwardAxis = Quaternion.Inverse(
            Quaternion.Euler(spec.VisualEuler)) * (muzzle - grip).normalized;
        return spec;
    }

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
            BuildAssetBundleOptions.ChunkBasedCompression |
            BuildAssetBundleOptions.DeterministicAssetBundle |
            BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildTarget.StandaloneWindows64);
    }

    private static FirearmPrefabSpec Spec(string name, string family,
        string sourceModelFile,
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
            SourceModelFile = sourceModelFile,
            IsBeltOrBackModel = isBeltOrBackModel,
            RequiresTwoHandRig = requiresTwoHandRig,
            VisualPosition = visualPosition,
            VisualEuler = visualEuler,
            VisualScale = visualScale,
            MuzzlePosition = muzzlePosition,
            MuzzleEuler = Vector3.zero,
            GripPosition = Vector3.zero,
            ButtPosition = new Vector3(0f, 0f,
                muzzlePosition.z - Mathf.Max(expectedLengthMeters, 0.339f)),
            SourceUpAxis = Vector3.up,
            SourceForwardAxis = Vector3.forward,
            TargetForwardAxis = Vector3.forward,
            TargetUpAxis = Vector3.up,
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
        string[] modelPaths = Array.ConvertAll(modelGuids,
            AssetDatabase.GUIDToAssetPath);
        string[] matches = Array.FindAll(modelPaths, path =>
            Path.GetFileName(path).Equals(spec.SourceModelFile,
                StringComparison.OrdinalIgnoreCase));
        if (matches.Length != 1)
            throw new InvalidOperationException(spec.Family +
                " requires exact source model " + spec.SourceModelFile +
                "; observed matches=" + matches.Length + ";models=" +
                string.Join("|", modelPaths));
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
            matches[0]);
        ResolveSemanticMarkers(source, spec);
        Debug.Log("KMG_RIG_BINDING name=" + spec.Name + ";family=" +
            spec.Family + ";source=" + matches[0] + ";sourceGuid=" +
            AssetDatabase.AssetPathToGUID(matches[0]) +
            ";prefab=Assets/ApprovedModels/" + spec.Name + ".prefab" +
            ";bundle=" + Bundle);
        GameObject root = new GameObject(spec.Name);
        GameObject visual = UnityEngine.Object.Instantiate(source, root.transform);
        visual.name = "Visual";
        foreach (Camera value in visual.GetComponentsInChildren<Camera>(true))
            UnityEngine.Object.DestroyImmediate(value.gameObject);
        foreach (Light value in visual.GetComponentsInChildren<Light>(true))
            UnityEngine.Object.DestroyImmediate(value.gameObject);
        if (spec.Family == "Revolver")
            RemoveDuplicatePreviewGeometry(visual, spec);
        if (!spec.IsBeltOrBackModel && spec.RequiresTwoHandRig)
        {
            RetainHighestDetailRenderers(visual, spec);
            MakeHeldLongGunMeshesTwoSided(visual, spec);
        }
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            throw new InvalidOperationException(spec.Name + " has no renderer.");
        ApplyMaterials(spec, renderers);
        ValidateVisibleScales(visual, spec);
        LogRendererDiagnostics(visual, spec, renderers);
        Quaternion visualRotation = ResolveVisualRotation(spec);
        visual.transform.localPosition = spec.HasSemanticAnchors
            ? spec.TargetAnchorPosition - TransformSourcePoint(spec,
                SourceAnchorPoint(spec))
            : spec.VisualPosition;
        visual.transform.localRotation = visualRotation;
        visual.transform.localScale = Vector3.one * spec.VisualScale;
        LogHierarchyDiagnostics(visual, spec, renderers);
        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(root.transform, false);
        muzzle.transform.localPosition = spec.HasSemanticAnchors
            ? TransformAnchoredPoint(spec, spec.SourceMuzzlePoint)
            : spec.MuzzlePosition;
        muzzle.transform.localRotation = Quaternion.Euler(spec.MuzzleEuler);
        GameObject grip = new GameObject(
            WeaponPresentationFrameContract.GripMarker);
        grip.transform.SetParent(root.transform, false);
        grip.transform.localPosition = spec.HasSemanticAnchors
            ? TransformAnchoredPoint(spec, spec.SourceGripPoint)
            : spec.GripPosition;
        GameObject weaponUp = new GameObject(
            WeaponPresentationFrameContract.WeaponUpMarker);
        weaponUp.transform.SetParent(root.transform, false);
        weaponUp.transform.localPosition = grip.transform.localPosition +
            visualRotation * spec.SourceUpAxis.normalized * 0.10f;
        GameObject weaponForward = new GameObject(
            WeaponPresentationFrameContract.WeaponForwardMarker);
        weaponForward.transform.SetParent(root.transform, false);
        weaponForward.transform.localPosition = grip.transform.localPosition +
            visualRotation * spec.SourceForwardAxis.normalized * 0.10f;
        GameObject butt = new GameObject(
            WeaponPresentationFrameContract.ButtMarker);
        butt.transform.SetParent(root.transform, false);
        butt.transform.localPosition = spec.HasSemanticAnchors
            ? TransformAnchoredPoint(spec, spec.SourceButtPoint)
            : spec.ButtPosition;
        if (spec.HasBackAnchor)
        {
            GameObject back = new GameObject("BackMount");
            back.transform.SetParent(root.transform, false);
            back.transform.localPosition = TransformAnchoredPoint(spec,
                spec.SourceBackPoint);
        }
        if (spec.RequiresTwoHandRig)
        {
            GameObject support = new GameObject("SupportHandTarget");
            support.transform.SetParent(root.transform, false);
            support.transform.localPosition = spec.HasSemanticAnchors
                ? TransformAnchoredPoint(spec, spec.SourceSupportPoint)
                : spec.SupportHandPosition;
            support.transform.localRotation = Quaternion.Euler(
                spec.SupportHandEuler);
            GameObject markers = new GameObject("DevelopmentMarkers_RedGrip_GreenSupport_BlueMuzzle_YellowButt");
            markers.transform.SetParent(root.transform, false);
            markers.SetActive(false);
        }
        if (spec.Name == "Pistol" && !spec.UseSemanticBasis)
            throw new InvalidOperationException(
                "Pistol equipped Visual must be derived from its semantic basis.");
        WeaponPresentationSemanticFrame frame =
            WeaponPresentationFrameContract.RequireWithForwardMarker(
                root.transform,
                spec.Name, "Muzzle",
                WeaponPresentationFrameContract.WeaponUpMarker,
                WeaponPresentationFrameContract.WeaponForwardMarker,
                spec.RequiresTwoHandRig, spec.MinimumLengthMeters,
                spec.MaximumLengthMeters);
        WeaponPresentationProjection projection =
            WeaponPresentationFrameContract.ValidateRendererEndpoints(
                root.transform, visual.transform, frame, spec.Name,
                spec.HasSemanticAnchors ? 0.12f : 0.25f);
        Debug.Log("KMG_FIREARM_SEMANTIC_FRAME name=" + spec.Name +
            ";forward=" + frame.Forward.ToString("R") + ";up=" +
            frame.Up.ToString("R") + ";right=" +
            frame.Right.ToString("R") + ";rendererProjection=" +
            projection.Minimum.ToString("R") + ".." +
            projection.Maximum.ToString("R") + ";sources=" +
            projection.SourceCount);
        ValidateHierarchy(root, spec, renderers);
        PrefabUtility.CreatePrefab("Assets/ApprovedModels/" + spec.Name +
            ".prefab", root,
            ReplacePrefabOptions.ReplaceNameBased);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static Vector3 TransformSourcePoint(FirearmPrefabSpec spec,
        Vector3 point)
    {
        return ResolveVisualRotation(spec) * (point * spec.VisualScale);
    }

    private static Vector3 SourceAnchorPoint(FirearmPrefabSpec spec)
    {
        return spec.IsBeltOrBackModel && spec.HasBackAnchor
            ? spec.SourceBackPoint : spec.SourceGripPoint;
    }

    private static Vector3 TransformAnchoredPoint(FirearmPrefabSpec spec,
        Vector3 point)
    {
        return spec.TargetAnchorPosition + TransformSourcePoint(spec,
            point - SourceAnchorPoint(spec));
    }

    private static Quaternion ResolveVisualRotation(FirearmPrefabSpec spec)
    {
        Quaternion declared = Quaternion.Euler(spec.VisualEuler);
        if (!spec.UseSemanticBasis) return declared;
        WeaponPresentationSemanticFrame source =
            new WeaponPresentationSemanticFrame(spec.SourceGripPoint,
                spec.SourceMuzzlePoint, spec.SourceButtPoint,
                spec.SourceGripPoint + spec.SourceUpAxis, false,
                Vector3.zero, spec.SourceForwardAxis);
        Quaternion solved = WeaponPresentationFrameContract.SolveRotation(
            source, spec.TargetForwardAxis, spec.TargetUpAxis);
        if (Quaternion.Angle(solved, declared) > 0.05f)
            throw new InvalidOperationException(spec.Name +
                " serialized Euler rotation differs from its solved semantic basis: declared=" +
                spec.VisualEuler.ToString("R") + ";solved=" +
                solved.eulerAngles.ToString("R"));
        return solved;
    }

    private static Vector3 CanonicalRelativeToGrip(FirearmPrefabSpec spec,
        Vector3 point)
    {
        WeaponPresentationSemanticFrame source =
            new WeaponPresentationSemanticFrame(spec.SourceGripPoint,
                spec.SourceMuzzlePoint, spec.SourceButtPoint,
                spec.SourceGripPoint + spec.SourceUpAxis, false,
                Vector3.zero, spec.SourceForwardAxis);
        Quaternion rotation = WeaponPresentationFrameContract.SolveRotation(
            source, Vector3.forward, Vector3.up);
        return rotation * ((point - spec.SourceGripPoint) * spec.VisualScale);
    }

    private static void ResolveSemanticMarkers(GameObject source,
        FirearmPrefabSpec spec)
    {
        if (!spec.RequiresTwoHandRig && !spec.RequireSourceMarkers) return;
        var required = new List<string> {
            "KMG_Grip", "KMG_Support", "KMG_Butt", "KMG_Muzzle"
        };
        if (spec.HasBackAnchor) required.Add("KMG_Back");
        if (spec.RequireSourceFrameMarkers)
        {
            required.Add("KMG_WeaponUp");
            required.Add("KMG_WeaponForward");
        }
        var matches = new Dictionary<string, List<Transform>>();
        foreach (string marker in required)
            matches[marker] = new List<Transform>();
        foreach (Transform child in source.GetComponentsInChildren<Transform>(true))
            if (matches.ContainsKey(child.name)) matches[child.name].Add(child);
        int found = 0;
        foreach (string marker in required) found += matches[marker].Count;
        if (found == 0)
        {
            if (spec.RequireSourceMarkers)
                throw new InvalidOperationException(spec.Name +
                    " requires its complete source-authored KMG marker set.");
            Debug.Log("KMG_RIG_MARKERS name=" + spec.Name +
                ";source=legacy-fallback;required=false");
            ValidateResolvedSemanticPoints(spec);
            return;
        }
        foreach (string marker in required)
            if (matches[marker].Count != 1)
                throw new InvalidOperationException(spec.Name +
                    " marker contract requires exactly one " + marker +
                    ";observed=" + matches[marker].Count +
                    ";partial-or-duplicate-marker-set=true");
        spec.SourceGripPoint = SourceLocalPoint(source, matches["KMG_Grip"][0]);
        spec.SourceSupportPoint = SourceLocalPoint(source, matches["KMG_Support"][0]);
        spec.SourceButtPoint = SourceLocalPoint(source, matches["KMG_Butt"][0]);
        spec.SourceMuzzlePoint = SourceLocalPoint(source, matches["KMG_Muzzle"][0]);
        if (spec.HasBackAnchor)
            spec.SourceBackPoint = SourceLocalPoint(source, matches["KMG_Back"][0]);
        if (spec.RequireSourceFrameMarkers)
        {
            Vector3 sourceUpPoint = SourceLocalPoint(source,
                matches["KMG_WeaponUp"][0]);
            Vector3 sourceForwardPoint = SourceLocalPoint(source,
                matches["KMG_WeaponForward"][0]);
            spec.SourceUpAxis = sourceUpPoint - spec.SourceGripPoint;
            spec.SourceForwardAxis = sourceForwardPoint -
                spec.SourceGripPoint;
        }
        if (!Finite(spec.SourceGripPoint) || !Finite(spec.SourceSupportPoint) ||
            !Finite(spec.SourceButtPoint) || !Finite(spec.SourceMuzzlePoint) ||
            (spec.HasBackAnchor && !Finite(spec.SourceBackPoint)) ||
            !Finite(spec.SourceUpAxis) || !Finite(spec.SourceForwardAxis) ||
            spec.SourceUpAxis.sqrMagnitude <= 0.000001f ||
            spec.SourceForwardAxis.sqrMagnitude <= 0.000001f ||
            Mathf.Abs(Vector3.Dot(spec.SourceUpAxis.normalized,
                spec.SourceForwardAxis.normalized)) >= 0.98f)
            throw new InvalidOperationException(spec.Name +
                " contains a non-finite source-authored semantic marker or a degenerate/collinear source frame marker.");
        ValidateResolvedSemanticPoints(spec);
        Debug.Log("KMG_RIG_MARKERS name=" + spec.Name +
            ";source=authored;grip=" + spec.SourceGripPoint.ToString("R") +
            ";support=" + spec.SourceSupportPoint.ToString("R") +
            ";butt=" + spec.SourceButtPoint.ToString("R") +
            ";muzzle=" + spec.SourceMuzzlePoint.ToString("R") +
            ";sourceForward=" + spec.SourceForwardAxis.ToString("R") +
            ";sourceUp=" + spec.SourceUpAxis.ToString("R") +
            ";diagnosticOnly=" + spec.DiagnosticOnly);
    }

    private static Vector3 SourceLocalPoint(GameObject source, Transform marker)
    {
        return source.transform.InverseTransformPoint(marker.position);
    }

    private static void ValidateResolvedSemanticPoints(FirearmPrefabSpec spec)
    {
        Vector3 grip = Vector3.zero;
        Vector3 support = CanonicalRelativeToGrip(spec,
            spec.SourceSupportPoint);
        Vector3 butt = CanonicalRelativeToGrip(spec, spec.SourceButtPoint);
        Vector3 muzzle = CanonicalRelativeToGrip(spec,
            spec.SourceMuzzlePoint);
        float length = Vector3.Distance(butt, muzzle);
        float maximumLateral = Mathf.Max(0.20f, length * 0.25f);
        if (!Finite(support) || !Finite(butt) || !Finite(muzzle) ||
            length < spec.MinimumLengthMeters || length > spec.MaximumLengthMeters)
            throw new InvalidOperationException(spec.Name +
                " marker-authored scale/length is implausible: length=" +
                length.ToString("R"));
        if (muzzle.z <= 0f || muzzle.z <= Mathf.Abs(muzzle.x) * 4f ||
            muzzle.z <= Mathf.Abs(muzzle.y) * 4f)
            throw new InvalidOperationException(spec.Name +
                " marker-authored +Z muzzle axis is invalid: " + muzzle.ToString("R"));
        if (butt.z >= grip.z || support.z <= grip.z || support.z >= muzzle.z)
            throw new InvalidOperationException(spec.Name +
                " marker-authored grip/support/butt/muzzle ordering is invalid.");
        if (Mathf.Sqrt(support.x * support.x + support.y * support.y) >
            maximumLateral)
            throw new InvalidOperationException(spec.Name +
                " marker-authored support point is outside the plausible weapon envelope: " +
                support.ToString("R"));
    }

    private static void RemoveDuplicatePreviewGeometry(GameObject visual,
        FirearmPrefabSpec spec)
    {
        int removed = 0;
        Transform[] children = visual.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == visual.transform) continue;
            string name = child.name;
            int dot = name.LastIndexOf('.');
            int suffix;
            if (dot > 0 && int.TryParse(name.Substring(dot + 1), out suffix))
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
                removed++;
            }
        }
        Debug.Log("KMG_RIG_CLEANUP name=" + spec.Name +
            ";removedDuplicatePreviewObjects=" + removed +
            ";policy=retain-unsuffixed-low-poly-assembly");
        if (removed == 0)
            throw new InvalidOperationException(
                "Revolver duplicate preview cleanup found no suffixed objects.");
    }

    private static void MakeHeldLongGunMeshesTwoSided(GameObject visual,
        FirearmPrefabSpec spec)
    {
        string folder = "Assets/ApprovedModels/GeneratedMeshes";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/ApprovedModels", "GeneratedMeshes");
        int converted = 0;
        foreach (MeshFilter filter in visual.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh source = filter.sharedMesh;
            if (source == null) continue;
            Mesh mesh = UnityEngine.Object.Instantiate(source);
            mesh.name = spec.Name + "_" + Sanitize(TransformPath(
                filter.transform, visual.transform)) + "_TwoSided";
            for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                int[] front = mesh.GetTriangles(submesh);
                int[] both = new int[front.Length * 2];
                Array.Copy(front, both, front.Length);
                for (int index = 0; index < front.Length; index += 3)
                {
                    both[front.Length + index] = front[index];
                    both[front.Length + index + 1] = front[index + 2];
                    both[front.Length + index + 2] = front[index + 1];
                }
                mesh.SetTriangles(both, submesh);
            }
            mesh.RecalculateBounds();
            string path = folder + "/" + Sanitize(mesh.name) + ".asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            filter.sharedMesh = mesh;
            converted++;
        }
        if (converted == 0)
            throw new InvalidOperationException(spec.Name +
                " has no MeshFilter eligible for two-sided held geometry.");
        Debug.Log("KMG_RIG_CULLING name=" + spec.Name +
            ";convertedMeshes=" + converted +
            ";policy=opaque-standard-with-reversed-backfaces");
    }

    private static void LogHierarchyDiagnostics(GameObject visual,
        FirearmPrefabSpec spec, Renderer[] renderers)
    {
        foreach (Transform child in visual.GetComponentsInChildren<Transform>(true))
        {
            string components = string.Join("|", Array.ConvertAll(
                child.GetComponents<Component>(), value => value == null ?
                    "<null>" : value.GetType().Name));
            Debug.Log("KMG_RIG_TRANSFORM name=" + spec.Name + ";path=" +
                TransformPath(child, visual.transform) + ";position=" +
                child.localPosition.ToString("R") + ";rotation=" +
                child.localEulerAngles.ToString("R") + ";scale=" +
                child.localScale.ToString("R") + ";components=" + components);
        }
        Bounds bounds = CalculateBounds(renderers);
        float semanticLength = Vector3.Distance(spec.SourceButtPoint,
            spec.SourceMuzzlePoint) * spec.VisualScale;
        Debug.Log("KMG_RIG_BOUNDS name=" + spec.Name + ";center=" +
            bounds.center.ToString("R") + ";size=" + bounds.size.ToString("R") +
            ";magnitude=" + bounds.size.magnitude.ToString("R") +
            ";semanticLength=" + semanticLength.ToString("R") +
            ";renderers=" + renderers.Length);
        if (!spec.IsBeltOrBackModel && spec.RequiresTwoHandRig &&
            (semanticLength < spec.MinimumLengthMeters ||
             semanticLength > spec.MaximumLengthMeters))
            throw new InvalidOperationException(spec.Name +
                " semantic butt-to-muzzle length is outside its long-gun contract: " +
                semanticLength.ToString("R") + " not in [" +
                spec.MinimumLengthMeters.ToString("R") + "," +
                spec.MaximumLengthMeters.ToString("R") + "]");
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
                ";localScale=" + renderer.transform.localScale.ToString("R") +
                ";boundsCenter=" + renderer.bounds.center.ToString("R") +
                ";boundsSize=" + renderer.bounds.size.ToString("R"));
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
            !Finite(spec.GripPosition) || !Finite(spec.ButtPosition) ||
            !Finite(spec.SourceUpAxis) || !Finite(spec.SourceForwardAxis) ||
            !Finite(spec.TargetForwardAxis) || !Finite(spec.TargetUpAxis) ||
            !Finite(spec.SupportHandPosition) || !Finite(spec.SupportHandEuler) ||
            !Finite(spec.VisualScale) || spec.VisualScale <= 0f)
            throw new InvalidOperationException(spec.Name +
                " contains a non-finite or non-positive transform.");
        if (spec.SourceUpAxis.sqrMagnitude <= 0.000001f)
            throw new InvalidOperationException(spec.Name +
                " has a degenerate source WeaponUp axis.");
        if (spec.SourceForwardAxis.sqrMagnitude <= 0.000001f ||
            spec.TargetForwardAxis.sqrMagnitude <= 0.000001f ||
            spec.TargetUpAxis.sqrMagnitude <= 0.000001f)
            throw new InvalidOperationException(spec.Name +
                " has a degenerate source or target basis axis.");
        if (spec.UseSemanticBasis && !spec.HasSemanticAnchors)
            throw new InvalidOperationException(spec.Name +
                " cannot solve a basis without source semantic points.");
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
        if (spec.RequiresTwoHandRig && !spec.HasSemanticAnchors)
            throw new InvalidOperationException(spec.Name +
                " requires explicit source-space Grip/Support/Butt/Muzzle anchors.");
        if (spec.HasSemanticAnchors &&
            (!Finite(spec.SourceGripPoint) || !Finite(spec.SourceSupportPoint) ||
             !Finite(spec.SourceButtPoint) || !Finite(spec.SourceMuzzlePoint) ||
             spec.SourceButtPoint == spec.SourceMuzzlePoint))
            throw new InvalidOperationException(spec.Name +
                " semantic source anchors are invalid or collapsed.");
        if (spec.RequireSourceMarkers && !spec.HasSemanticAnchors)
            throw new InvalidOperationException(spec.Name +
                " cannot require source markers without semantic-anchor behavior.");
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
        if (root.transform.Find("Grip") == null ||
            root.transform.Find("Butt") == null ||
            root.transform.Find("WeaponUp") == null ||
            root.transform.Find("WeaponForward") == null)
            throw new InvalidOperationException(spec.Name +
                " lacks its complete Grip/Butt/WeaponForward/WeaponUp frame.");
        if (spec.HasBackAnchor && root.transform.Find("BackMount") == null)
            throw new InvalidOperationException(spec.Name +
                " lacks its independent BackMount anchor.");
        if (spec.RequiresTwoHandRig)
        {
            Vector3 grip = root.transform.Find("Grip").localPosition;
            Vector3 muzzle = root.transform.Find("Muzzle").localPosition;
            Vector3 butt = root.transform.Find("Butt").localPosition;
            Vector3 support = root.transform.Find("SupportHandTarget").localPosition;
            Vector3 forwardMarker = root.transform.Find(
                "WeaponForward").localPosition;
            Vector3 forward = (forwardMarker - grip).normalized;
            float buttProjection = Vector3.Dot(butt - grip, forward);
            float supportProjection = Vector3.Dot(support - grip, forward);
            float muzzleProjection = Vector3.Dot(muzzle - grip, forward);
            float length = Vector3.Distance(butt, muzzle);
            if (length < spec.MinimumLengthMeters ||
                length > spec.MaximumLengthMeters ||
                buttProjection >= 0f || supportProjection <= 0f ||
                supportProjection >= muzzleProjection)
                throw new InvalidOperationException(spec.Name +
                    " semantic anchor ordering/length is invalid: butt=" + butt.ToString("R") +
                    ";support=" + support.ToString("R") + ";muzzle=" +
                    muzzle.ToString("R") + ";forward=" +
                    forward.ToString("R") + ";projections=" +
                    buttProjection.ToString("R") + "/" +
                    supportProjection.ToString("R") + "/" +
                    muzzleProjection.ToString("R") + ";length=" +
                    length.ToString("R"));
            Debug.Log("KMG_RIG_ANCHORS name=" + spec.Name + ";sourceGrip=" +
                spec.SourceGripPoint.ToString("R") + ";sourceSupport=" +
                spec.SourceSupportPoint.ToString("R") + ";sourceButt=" +
                spec.SourceButtPoint.ToString("R") + ";sourceMuzzle=" +
                spec.SourceMuzzlePoint.ToString("R") + ";visualPosition=" +
                root.transform.Find("Visual").localPosition.ToString("R") +
                ";support=" + support.ToString("R") + ";butt=" +
                butt.ToString("R") + ";muzzle=" + muzzle.ToString("R") +
                ";length=" + length.ToString("R"));
        }
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
                string path = materialFolder + "/" + family + "_" +
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
                material.shader = Shader.Find("Standard");
                material.SetFloat("_Mode", 0f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                EditorUtility.SetDirty(material);
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
