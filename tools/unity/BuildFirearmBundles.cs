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
        internal string SourceModelFile;
        internal bool IsBeltOrBackModel;
        internal bool RequiresTwoHandRig;
        internal Vector3 VisualPosition;
        internal Vector3 VisualEuler;
        internal float VisualScale;
        internal Vector3 MuzzlePosition;
        internal Vector3 MuzzleEuler;
        internal Vector3 SupportHandPosition;
        internal Vector3 SupportHandEuler;
        internal bool HasSemanticAnchors;
        internal Vector3 SourceGripPoint;
        internal Vector3 SourceSupportPoint;
        internal Vector3 SourceButtPoint;
        internal Vector3 SourceMuzzlePoint;
        internal float ExpectedLengthMeters;
        internal float MinimumLengthMeters;
        internal float MaximumLengthMeters;
        internal string CandidateAnimation;
        internal string CalibrationStatus;
    }

    private static readonly FirearmPrefabSpec[] Specs =
    {
        Spec("Pistol", "Pistol", "model.dae", false, false,
            new Vector3(0f, 0f, 0.1632f), new Vector3(0f, 180f, 180f), 0.24f,
            new Vector3(0f, 0f, 0.264f), 0.264f, 0.15f, 0.45f,
            "Crossbow", "autonomous-candidate; equipped-visual-roll-corrected"),
        Spec("PistolBelt", "Pistol", "model.dae", true, false,
            Vector3.zero, new Vector3(0f, 90f, 90f), 0.24f,
            Vector3.zero, 0.264f, 0.15f, 0.45f,
            "None", "disabled; independent-holster-calibration-pending"),
        Anchored(Spec("Musket", "Musket", "Musket 01.fbx", false, true,
            Vector3.zero, new Vector3(0f, 90f, 0f), 4.186f,
            new Vector3(0f, 0f, 0.8525f), 0.8525f, 0.55f, 1.60f,
            "Crossbow", "semantic-anchor-candidate"),
            new Vector3(0.0400f, 0f, 0f),
            new Vector3(-0.1000f, -0.0122f, -0.0074f),
            new Vector3(0.0805f, 0f, 0f),
            new Vector3(-0.2420f, 0f, 0f)),
        Spec("MusketBelt", "Musket", "Musket 01.fbx", true, false,
            Vector3.zero, Vector3.zero, 2.0f, Vector3.zero,
            0.8525f, 0.55f, 1.60f, "None",
            "disabled; independent-back-calibration-pending"),
        Anchored(Spec("Blunderbuss", "Blunderbuss", "Blunderbuss_Low_Poly.fbx", false, true,
            Vector3.zero, new Vector3(0f, 90f, 0f), 20f,
            new Vector3(0f, 0f, 0.6875f), 0.6875f, 0.40f, 1.30f,
            "Crossbow", "semantic-anchor-candidate"),
            new Vector3(0.0100f, 0f, -0.00316f),
            new Vector3(-0.0125f, -0.00255f, -0.00471f),
            new Vector3(0.01565f, 0f, -0.00316f),
            new Vector3(-0.02675f, 0f, -0.00316f)),
        Spec("BlunderbussBelt", "Blunderbuss", "Blunderbuss_Low_Poly.fbx", true, false,
            Vector3.zero, Vector3.zero, 0.5f, Vector3.zero,
            0.6875f, 0.40f, 1.30f, "None",
            "disabled; independent-back-calibration-pending"),
        Spec("Revolver", "Revolver", "Final2 Sketchfab.fbx", false, false,
            new Vector3(-0.0460553f, -0.1052241f, 0.1857974f),
            new Vector3(0f, 90f, 0f), 0.01719849f,
            new Vector3(0f, 0f, 0.264f), 0.264f, 0.15f, 0.45f,
            "Crossbow", "source-unit-normalization-required"),
        Anchored(Spec("Rifle", "Rifle", "fusilALevier.fbx", false, true,
            Vector3.zero, new Vector3(0f, 90f, 0f),
            1.5401387f, new Vector3(0f, 0f, 0.8525f),
            0.8525f, 0.55f, 1.60f, "Crossbow",
            "semantic-anchor-candidate"),
            new Vector3(0.1300f, 0f, 0f),
            new Vector3(-0.1946f, -0.0331f, -0.0201f),
            new Vector3(0.5030f, 0f, 0f),
            new Vector3(-0.5030f, 0f, 0f))
    };

    private static FirearmPrefabSpec Anchored(FirearmPrefabSpec spec,
        Vector3 grip, Vector3 support, Vector3 butt, Vector3 muzzle)
    {
        spec.HasSemanticAnchors = true;
        spec.SourceGripPoint = grip;
        spec.SourceSupportPoint = support;
        spec.SourceButtPoint = butt;
        spec.SourceMuzzlePoint = muzzle;
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
        Quaternion visualRotation = Quaternion.Euler(spec.VisualEuler);
        visual.transform.localPosition = spec.HasSemanticAnchors
            ? -TransformSourcePoint(spec, spec.SourceGripPoint)
            : spec.VisualPosition;
        visual.transform.localRotation = visualRotation;
        visual.transform.localScale = Vector3.one * spec.VisualScale;
        LogHierarchyDiagnostics(visual, spec, renderers);
        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(root.transform, false);
        muzzle.transform.localPosition = spec.HasSemanticAnchors
            ? AnchorRelativeToGrip(spec, spec.SourceMuzzlePoint)
            : spec.MuzzlePosition;
        muzzle.transform.localRotation = Quaternion.Euler(spec.MuzzleEuler);
        if (spec.RequiresTwoHandRig)
        {
            GameObject support = new GameObject("SupportHandTarget");
            support.transform.SetParent(root.transform, false);
            support.transform.localPosition = spec.HasSemanticAnchors
                ? AnchorRelativeToGrip(spec, spec.SourceSupportPoint)
                : spec.SupportHandPosition;
            support.transform.localRotation = Quaternion.Euler(
                spec.SupportHandEuler);
            GameObject butt = new GameObject("Butt");
            butt.transform.SetParent(root.transform, false);
            butt.transform.localPosition = AnchorRelativeToGrip(spec,
                spec.SourceButtPoint);
            GameObject markers = new GameObject("DevelopmentMarkers_RedGrip_GreenSupport_BlueMuzzle_YellowButt");
            markers.transform.SetParent(root.transform, false);
            markers.SetActive(false);
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

    private static Vector3 TransformSourcePoint(FirearmPrefabSpec spec,
        Vector3 point)
    {
        return Quaternion.Euler(spec.VisualEuler) * (point * spec.VisualScale);
    }

    private static Vector3 AnchorRelativeToGrip(FirearmPrefabSpec spec,
        Vector3 point)
    {
        return TransformSourcePoint(spec, point) -
            TransformSourcePoint(spec, spec.SourceGripPoint);
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
        Debug.Log("KMG_RIG_BOUNDS name=" + spec.Name + ";center=" +
            bounds.center.ToString("R") + ";size=" + bounds.size.ToString("R") +
            ";magnitude=" + bounds.size.magnitude.ToString("R") +
            ";renderers=" + renderers.Length);
        if (!spec.IsBeltOrBackModel && spec.RequiresTwoHandRig &&
            (bounds.size.magnitude < spec.MinimumLengthMeters ||
             bounds.size.magnitude > spec.MaximumLengthMeters))
            throw new InvalidOperationException(spec.Name +
                " rendered bounds magnitude is outside its long-gun contract: " +
                bounds.size.magnitude.ToString("R") + " not in [" +
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
        if (spec.RequiresTwoHandRig && !spec.HasSemanticAnchors)
            throw new InvalidOperationException(spec.Name +
                " requires explicit source-space Grip/Support/Butt/Muzzle anchors.");
        if (spec.HasSemanticAnchors &&
            (!Finite(spec.SourceGripPoint) || !Finite(spec.SourceSupportPoint) ||
             !Finite(spec.SourceButtPoint) || !Finite(spec.SourceMuzzlePoint) ||
             spec.SourceButtPoint == spec.SourceMuzzlePoint))
            throw new InvalidOperationException(spec.Name +
                " semantic source anchors are invalid or collapsed.");
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
        if (spec.RequiresTwoHandRig && root.transform.Find("Butt") == null)
            throw new InvalidOperationException(spec.Name +
                " lacks its semantic Butt anchor.");
        if (spec.RequiresTwoHandRig)
        {
            Vector3 muzzle = root.transform.Find("Muzzle").localPosition;
            Vector3 butt = root.transform.Find("Butt").localPosition;
            Vector3 support = root.transform.Find("SupportHandTarget").localPosition;
            float length = Vector3.Distance(butt, muzzle);
            if (length < spec.MinimumLengthMeters || length > spec.MaximumLengthMeters ||
                support.z <= butt.z || support.z >= muzzle.z)
                throw new InvalidOperationException(spec.Name +
                    " semantic anchor ordering/length is invalid: butt=" + butt.ToString("R") +
                    ";support=" + support.ToString("R") + ";muzzle=" +
                    muzzle.ToString("R") + ";length=" + length.ToString("R"));
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
