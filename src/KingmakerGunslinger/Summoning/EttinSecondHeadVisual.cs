using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One original second head on the retained humanoid giant rig.
    /// Original sculpted geometry bridges the native neck and an owned skull
    /// control. The skull copies head rotation at a fixed neck-relative offset;
    /// turning the first head no longer orbits the second head around it.
    /// Native animation and the donor's first head remain intact.</summary>
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class EttinSecondHeadVisual
    {
        internal const string ObjectPrefix = "KMG_Ettin_SecondHead";
        internal const string DonorPrefabId =
            "635d6dee1c7831b41aa987bdbeff14bc";
        // Append-only project unit identity; the donor prefab remains unchanged.
        internal const string EttinUnitGuid = "2481bc73abd64ef0ae581b48438b5404";
        private static readonly FieldInfo CachedMaterialSettings = typeof(StandardMaterialController).GetField(
            "m_MaterialSettings", BindingFlags.Instance | BindingFlags.NonPublic);
        private sealed class FadeDefaults
        {
            internal bool IsDissolveEnabled, DissolveCutoutEnabled, DissolveEmissionEnabled;
            internal Texture2D DissolveTexture;
            internal Vector2 DissolveTextureScale, DissolveTextureOffset;
            internal float Dissolve, DissolveWidth, DissolveColorScale;
            internal Color DissolveColor;
            internal int RenderQueue;
            internal string RenderTypeTagValue;
        }
        private sealed class Attachment
        {
            internal GameObject Root;
            internal Mesh Mesh;
            internal UnitEntityView View;
            internal StandardMaterialController Controller;
            internal Material SourceMaterial;
            internal bool ControllerAdoptionStarted;
            internal FadeDefaults Baseline;
            internal JObject AttachedSourceFadeDefaults;
            internal JArray AttachedMaterialFadeDefaults;
            internal float SourceDefaultDissolve;
            internal string ControllerCleanup;
            internal readonly List<Material> Materials = new List<Material>();
            internal readonly List<Texture2D> Textures = new List<Texture2D>();
            internal string Outcome;
        }
        private static readonly ConditionalWeakTable<UnitEntityView,
            Attachment> Applied = new ConditionalWeakTable<UnitEntityView,
                Attachment>();

        internal static string Describe(UnitEntityView view)
        {
            Attachment value;
            return view != null && Applied.TryGetValue(view, out value) ?
                value.Outcome : "not-attempted";
        }

        private static void Postfix(UnitEntityView __instance)
        {
            ModContext context;
            if (string.IsNullOrEmpty(EttinUnitGuid) || __instance == null ||
                __instance.EntityData == null ||
                __instance.EntityData.Blueprint == null ||
                !ModContext.TryGet(out context) ||
                context.FeatureModules == null ||
                context.FeatureModules.Active == null ||
                !context.FeatureModules.Active.ExpandedSummoning)
                return;
            var blueprint = __instance.EntityData.Blueprint;
            if (blueprint.name != "KMG_Summoning_Unit_Ettin" ||
                blueprint.AssetGuid != EttinUnitGuid ||
                blueprint.Prefab == null ||
                blueprint.Prefab.AssetId != DonorPrefabId)
                return;
            lock (Applied)
            {
                Attachment ignored;
                if (Applied.TryGetValue(__instance, out ignored))
                {
                    if (ignored.Outcome != "released") return;
                    Applied.Remove(__instance);
                }
                var owned = new Attachment { View = __instance, Outcome = "attach-started" };
                Applied.Add(__instance, owned);
                try
                {
                    Transform[] heads = __instance
                        .GetComponentsInChildren<Transform>(true)
                        .Where(value => value != null &&
                            value.name == "Head").Distinct().ToArray();
                    if (heads.Length != 1)
                        throw new InvalidOperationException(
                            "The audited Hill Giant rig no longer has one head bone: " +
                            heads.Length);
                    Transform head = heads[0];
                    Transform neck = head.parent;
                    if (neck == null)
                        throw new InvalidOperationException(
                            "The audited Hill Giant head has no neck parent.");
                    Material template = RequireHillBodyMaterial(__instance);
                    StandardMaterialController[] controllers = __instance
                        .GetComponentsInChildren<StandardMaterialController>(true);
                    FadeDefaults defaults;
                    if (controllers.Length != 1 || ControllerMaterials(controllers[0]) == null ||
                        !ControllerMaterials(controllers[0]).Contains(template) ||
                        !TryGetFadeDefaults(controllers[0], template, out defaults))
                        throw new InvalidOperationException("Exact Hill source controller/cached fade defaults unavailable.");
                    if (float.IsNaN(defaults.Dissolve) || float.IsInfinity(defaults.Dissolve))
                        throw new InvalidOperationException("Non-finite native cached dissolve default.");
                    owned.Controller = controllers[0]; owned.SourceMaterial = template;
                    owned.SourceDefaultDissolve = defaults.Dissolve;
                    owned.Baseline = defaults;
                    owned.AttachedSourceFadeDefaults = DescribeFadeDefaults(defaults);
                    owned.Root = new GameObject(ObjectPrefix + "_Root");
                    owned.Root.transform.SetParent(neck, false);
                    owned.Root.transform.localPosition = Vector3.zero;
                    owned.Root.transform.localRotation = Quaternion.identity;
                    owned.Root.transform.localScale = Vector3.one;
                    if (!owned.Root.transform.IsChildOf(owned.Controller.transform))
                        throw new InvalidOperationException("Owned head is outside exact source material-controller hierarchy.");
                    Transform skull = new GameObject(ObjectPrefix +
                        "_SkullControl").transform;
                    skull.SetParent(owned.Root.transform, false);
                    Vector3 offset = head.localRotation *
                        Vector3.Scale(head.localScale,
                            new Vector3(.44f, -.035f, -.02f));
                    var driver = owned.Root.AddComponent<EttinHeadPoseDriver>();
                    driver.Configure(head, skull, offset);
                    owned.Mesh = EttinAuthoredHeadMesh.Build(
                        owned.Root.transform.worldToLocalMatrix *
                            head.localToWorldMatrix);
                    owned.Mesh.bindposes = new[] {
                        neck.worldToLocalMatrix *
                            owned.Root.transform.localToWorldMatrix,
                        skull.worldToLocalMatrix *
                            owned.Root.transform.localToWorldMatrix
                    };
                    Texture2D albedo = EttinAuthoredHeadMesh.BuildAlbedo();
                    owned.Textures.Add(albedo);
                    // Exact installed PF property label: R=Roughness,
                    // G=Emission, B=Metallic. Original matte, nonemissive,
                    // nonmetal skin/hair/eyes; not a guessed Standard map.
                    var parameters = new Texture2D(2, 2, TextureFormat.RGBA32,
                        false, true) { name = ObjectPrefix + "_OriginalParameters",
                        wrapMode = TextureWrapMode.Clamp };
                    owned.Textures.Add(parameters);
                    parameters.SetPixels32(new[] { new Color32(230, 0, 0, 255),
                        new Color32(230, 0, 0, 255), new Color32(230, 0, 0, 255),
                        new Color32(230, 0, 0, 255) });
                    parameters.Apply(false, false);
                    Material skin = MakeMaterial(owned, "Skin", template, albedo, parameters, defaults);
                    Material shadow = MakeMaterial(owned, "Shadow", template, albedo, parameters, defaults);
                    Material hair = MakeMaterial(owned, "Hair", template, albedo, parameters, defaults);
                    Material eye = MakeMaterial(owned, "Eye", template, albedo, parameters, defaults);
                    var renderer = owned.Root.AddComponent<SkinnedMeshRenderer>();
                    renderer.sharedMesh = owned.Mesh;
                    renderer.bones = new[] { neck, skull };
                    renderer.rootBone = neck;
                    renderer.sharedMaterials = new[] { skin, shadow, hair, eye };
                    Bounds bounds = owned.Mesh.bounds;
                    bounds.Expand(.55f);
                    renderer.localBounds = bounds;
                    renderer.updateWhenOffscreen = true;
                    // Reinit restores existing cached native defaults and then
                    // discovers the new renderer. The four new materials must
                    // cache the SOURCE BASELINE, never its transient live fade.
                    owned.ControllerAdoptionStarted = true;
                    string reinit = ExpandedSummoningPteranodonViewPatch.ReinitializeMaterialController(__instance);
                    Material[] assigned = renderer.sharedMaterials;
                    IList<Material> driven = ControllerMaterials(owned.Controller);
                    if (reinit != "reinitialized" || assigned.Length != 4 ||
                        driven == null || assigned.Any(material => !AdoptedWithBaseline(owned, material, driven)))
                        throw new InvalidOperationException("Owned head was not adopted with exact native baseline fade defaults.");
                    // Scalar receipt only: preserve the cache accepted here so
                    // later native recensus/instancing is distinguishable from
                    // an initialization mismatch. No material state is changed.
                    owned.AttachedMaterialFadeDefaults = new JArray(assigned.Select(material => {
                        FadeDefaults cached;
                        TryGetFadeDefaults(owned.Controller, material, out cached);
                        return new JObject { ["material"] = material.GetInstanceID(),
                            ["name"] = material.name, ["cached"] = DescribeFadeDefaults(cached) };
                    }));
                    owned.Outcome = "attached;heads=2;original=1;project=1;" +
                        "authored=1;continuous-neck=1;orbit=0;" +
                        "shader=PF/StandardDynamic;original-paint=1;owned-textures=2;controller-adopted=1;fade-default=source-cache";
                }
                catch (Exception error)
                {
                    Release(owned);
                    owned.Outcome = "failed:" + error.GetType().Name +
                        ":" + error.Message;
                    Debug.LogException(error);
                }
            }
        }

        private static Material RequireHillBodyMaterial(UnitEntityView view)
        {
            SkinnedMeshRenderer[] bodies = view.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.name == "hill_giant_1" &&
                    value.sharedMesh != null && value.sharedMesh.name == "hill_giant_1").ToArray();
            Material[] materials = bodies.Length == 1 ? bodies[0].sharedMaterials : null;
            Material source = materials != null && materials.Length == 1 ? materials[0] : null;
            if (source == null || !ExactInstanceName(source.name, "hill_giant_01_character_d") ||
                source.shader == null || source.shader.name != "PF/StandardDynamic" ||
                !source.HasProperty("_MainTex") || !source.HasProperty("_Parameters") ||
                !source.HasProperty("_BumpMap") || !source.HasProperty("_TintColor") ||
                source.GetTexture("_MainTex") == null || source.GetTexture("_MainTex").name != "hill_giant_01_d" ||
                source.GetTexture("_Parameters") == null || source.GetTexture("_Parameters").name != "hill_giant_01_m" ||
                source.GetTexture("_BumpMap") != null)
                throw new InvalidOperationException(
                    "Exact audited Hill body PF material/texture/flat-normal contract unavailable.");
            return source;
        }

        private static Material MakeMaterial(Attachment owned, string name,
            Material template, Texture2D albedo, Texture2D parameters,
            FadeDefaults defaults)
        {
            // Clone shader state/keywords only; every sampled authored surface
            // map is replaced or cleared. Never sample donor body UV maps.
            var material = new Material(template) { name = ObjectPrefix + "_" + name };
            owned.Materials.Add(material);
            material.SetTexture("_MainTex", albedo);
            material.SetTextureScale("_MainTex", Vector2.one);
            material.SetTextureOffset("_MainTex", Vector2.zero);
            material.SetTexture("_Parameters", parameters);
            material.SetTextureScale("_Parameters", Vector2.one);
            material.SetTextureOffset("_Parameters", Vector2.zero);
            // The exact shader declares built-in 'bump' as its default, and
            // the audited Hill body has no map. Use our vertex normals plus
            // that flat default, without inventing a packed normal format.
            material.SetTexture("_BumpMap", null);
            // Exact native CharacterAtlas normal-channel contract uses this
            // keyword/flag pair. This new surface has no normal atlas input.
            material.DisableKeyword("NORMAL_MAP_ON");
            material.SetFloat("_UseNormalMapAtlas", 0f);
            material.SetTexture("_BumpMap2", null);
            if (material.HasProperty("_PetrificationTex")) material.SetTexture("_PetrificationTex", null);
            material.SetColor("_TintColor", Color.white);
            material.SetFloat("_Roughness", 1f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Emission", 0f);
            // Exact native DissolveAnimationController.RevertToDefaults
            // contract. Borrow native FX references only; never own/destroy
            // them or guess neutral fade values from the current frame.
            if (defaults.IsDissolveEnabled) material.EnableKeyword("DISSOLVE_ON");
            else material.DisableKeyword("DISSOLVE_ON");
            material.SetTexture("_DissolveTex", defaults.DissolveTexture);
            material.SetTextureScale("_DissolveTex", defaults.DissolveTextureScale);
            material.SetTextureOffset("_DissolveTex", defaults.DissolveTextureOffset);
            material.SetFloat("_Dissolve", defaults.Dissolve);
            material.SetFloat("_DissolveWidth", defaults.DissolveWidth);
            material.SetColor("_DissolveColor", defaults.DissolveColor);
            material.SetFloat("_DissolveColorScale", defaults.DissolveColorScale);
            material.SetFloat("_DissolveCutout", defaults.DissolveCutoutEnabled ? 1f : 0f);
            material.SetFloat("_DissolveEmission", defaults.DissolveEmissionEnabled ? 1f : 0f);
            material.SetOverrideTag("RenderType", defaults.RenderTypeTagValue);
            material.renderQueue = defaults.RenderQueue;
            return material;
        }

        private static IList<Material> ControllerMaterials(StandardMaterialController controller)
        { return ExpandedSummoningPteranodonViewPatch.ControllerMaterials(controller); }

        private static T CachedField<T>(object settings, string name)
        {
            FieldInfo field = settings.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field == null || field.FieldType != typeof(T))
                throw new NotSupportedException("Native cached fade field contract changed: " + name);
            return (T)field.GetValue(settings);
        }

        private static bool TryGetFadeDefaults(StandardMaterialController controller,
            Material material, out FadeDefaults defaults)
        {
            defaults = null;
            var dictionary = controller == null || CachedMaterialSettings == null ? null :
                CachedMaterialSettings.GetValue(controller) as IDictionary;
            if (material == null || dictionary == null || !dictionary.Contains(material)) return false;
            object settings = dictionary[material];
            if (settings == null || settings.GetType().FullName !=
                "Kingmaker.Visual.MaterialEffects.StandardMaterialController+MaterialSettings")
                throw new NotSupportedException("Exact native cached material settings unavailable.");
            defaults = new FadeDefaults {
                IsDissolveEnabled = CachedField<bool>(settings, "IsDissolveEnabled"),
                DissolveTexture = CachedField<Texture2D>(settings, "DissolveTexture"),
                DissolveTextureScale = CachedField<Vector2>(settings, "DissolveTextureScale"),
                DissolveTextureOffset = CachedField<Vector2>(settings, "DissolveTextureOffset"),
                Dissolve = CachedField<float>(settings, "Dissolve"),
                DissolveWidth = CachedField<float>(settings, "DissolveWidth"),
                DissolveColor = CachedField<Color>(settings, "DissolveColor"),
                DissolveColorScale = CachedField<float>(settings, "DissolveColorScale"),
                DissolveCutoutEnabled = CachedField<bool>(settings, "DissolveCutoutEnabled"),
                DissolveEmissionEnabled = CachedField<bool>(settings, "DissolveEmissionEnabled"),
                RenderQueue = CachedField<int>(settings, "RenderQueue"),
                RenderTypeTagValue = CachedField<string>(settings, "RenderTypeTagValue") };
            return true;
        }

        private static bool AdoptedWithBaseline(Attachment owned, Material material, IList<Material> driven)
        {
            FadeDefaults defaults;
            return material != null && driven.Contains(material) &&
                TryGetFadeDefaults(owned.Controller, material, out defaults) &&
                SameFadeBaseline(defaults, owned.Baseline);
        }

        private static bool SameFadeBaseline(FadeDefaults first, FadeDefaults second)
        {
            return first != null && second != null && first.IsDissolveEnabled == second.IsDissolveEnabled &&
                ReferenceEquals(first.DissolveTexture, second.DissolveTexture) &&
                first.DissolveTextureScale.x == second.DissolveTextureScale.x &&
                first.DissolveTextureScale.y == second.DissolveTextureScale.y &&
                first.DissolveTextureOffset.x == second.DissolveTextureOffset.x &&
                first.DissolveTextureOffset.y == second.DissolveTextureOffset.y &&
                first.Dissolve == second.Dissolve && first.DissolveWidth == second.DissolveWidth &&
                first.DissolveColor.r == second.DissolveColor.r && first.DissolveColor.g == second.DissolveColor.g &&
                first.DissolveColor.b == second.DissolveColor.b && first.DissolveColor.a == second.DissolveColor.a &&
                first.DissolveColorScale == second.DissolveColorScale &&
                first.DissolveCutoutEnabled == second.DissolveCutoutEnabled &&
                first.DissolveEmissionEnabled == second.DissolveEmissionEnabled &&
                first.RenderQueue == second.RenderQueue && first.RenderTypeTagValue == second.RenderTypeTagValue;
        }

        private static JObject DescribeFadeDefaults(FadeDefaults value)
        {
            if (value == null) return null;
            Texture2D texture = value.DissolveTexture;
            return new JObject {
                ["IsDissolveEnabled"] = value.IsDissolveEnabled,
                ["DissolveTexture"] = new JObject {
                    ["managedNull"] = ReferenceEquals(texture, null),
                    ["nativeAlive"] = texture != null,
                    ["nativeInstanceId"] = ReferenceEquals(texture, null) ? 0 : texture.GetInstanceID(),
                    ["managedIdentity"] = ReferenceEquals(texture, null) ? 0 : RuntimeHelpers.GetHashCode(texture),
                    ["name"] = texture == null ? null : texture.name },
                ["DissolveTextureScale"] = new JArray(value.DissolveTextureScale.x, value.DissolveTextureScale.y),
                ["DissolveTextureOffset"] = new JArray(value.DissolveTextureOffset.x, value.DissolveTextureOffset.y),
                ["Dissolve"] = value.Dissolve, ["DissolveWidth"] = value.DissolveWidth,
                ["DissolveColor"] = new JArray(value.DissolveColor.r, value.DissolveColor.g,
                    value.DissolveColor.b, value.DissolveColor.a),
                ["DissolveColorScale"] = value.DissolveColorScale,
                ["DissolveCutoutEnabled"] = value.DissolveCutoutEnabled,
                ["DissolveEmissionEnabled"] = value.DissolveEmissionEnabled,
                ["RenderQueue"] = value.RenderQueue, ["RenderTypeTagValue"] = value.RenderTypeTagValue };
        }

        // These comparisons intentionally mirror SameFadeBaseline, including
        // managed texture identity and exact scalar comparisons; diagnostics
        // do not replace or relax the existing admission/assertion predicate.
        private static JArray FadeBaselineMismatches(FadeDefaults current, FadeDefaults attached)
        {
            var result = new JArray();
            if (current == null || attached == null) { result.Add("cacheUnavailable"); return result; }
            if (current.IsDissolveEnabled != attached.IsDissolveEnabled) result.Add("IsDissolveEnabled");
            if (!ReferenceEquals(current.DissolveTexture, attached.DissolveTexture)) result.Add("DissolveTexture.managedReference");
            if (current.DissolveTextureScale.x != attached.DissolveTextureScale.x) result.Add("DissolveTextureScale.x");
            if (current.DissolveTextureScale.y != attached.DissolveTextureScale.y) result.Add("DissolveTextureScale.y");
            if (current.DissolveTextureOffset.x != attached.DissolveTextureOffset.x) result.Add("DissolveTextureOffset.x");
            if (current.DissolveTextureOffset.y != attached.DissolveTextureOffset.y) result.Add("DissolveTextureOffset.y");
            if (current.Dissolve != attached.Dissolve) result.Add("Dissolve");
            if (current.DissolveWidth != attached.DissolveWidth) result.Add("DissolveWidth");
            if (current.DissolveColor.r != attached.DissolveColor.r) result.Add("DissolveColor.r");
            if (current.DissolveColor.g != attached.DissolveColor.g) result.Add("DissolveColor.g");
            if (current.DissolveColor.b != attached.DissolveColor.b) result.Add("DissolveColor.b");
            if (current.DissolveColor.a != attached.DissolveColor.a) result.Add("DissolveColor.a");
            if (current.DissolveColorScale != attached.DissolveColorScale) result.Add("DissolveColorScale");
            if (current.DissolveCutoutEnabled != attached.DissolveCutoutEnabled) result.Add("DissolveCutoutEnabled");
            if (current.DissolveEmissionEnabled != attached.DissolveEmissionEnabled) result.Add("DissolveEmissionEnabled");
            if (current.RenderQueue != attached.RenderQueue) result.Add("RenderQueue");
            if (current.RenderTypeTagValue != attached.RenderTypeTagValue) result.Add("RenderTypeTagValue");
            return result;
        }

        private static bool ExactInstanceName(string actual, string expected)
        {
            return actual != null && actual.StartsWith(expected, StringComparison.Ordinal) &&
                actual.Substring(expected.Length).Replace(" (Instance)", "") == "";
        }

        private static void Release(Attachment owned)
        {
            if (owned == null) return;
            // Native renderer setup can instantiate materials for this skin
            // after our four shared originals have been assigned. Every
            // renderer below this exact project-created root belongs to this
            // head. Collect its named clones only while one original owned
            // map still proves provenance; never destroy a foreign material.
            var materials = OwnedMaterials(owned);
            if (owned.Root != null)
                UnityEngine.Object.DestroyImmediate(owned.Root);
            owned.Root = null;
            if (owned.ControllerAdoptionStarted && owned.View != null && owned.Controller != null)
            {
                try { owned.ControllerCleanup = ExpandedSummoningPteranodonViewPatch.ReinitializeMaterialController(owned.View); }
                catch (Exception error) { owned.ControllerCleanup = "failed:" + error.GetType().Name + ":" + error.Message; }
                // Always release our private resources even if native controller
                // refresh fails. The explicit receipt retains that failure.
            }
            if (owned.Mesh != null)
                UnityEngine.Object.DestroyImmediate(owned.Mesh);
            owned.Mesh = null;
            foreach (Material material in materials)
                if (material != null)
                    UnityEngine.Object.DestroyImmediate(material);
            owned.Materials.Clear();
            foreach (Texture2D texture in owned.Textures)
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            owned.Textures.Clear();
        }

        private static Material[] OwnedMaterials(Attachment owned)
        {
            if (owned == null) return new Material[0];
            var assigned = owned.Root == null ? Enumerable.Empty<Material>() :
                owned.Root.GetComponentsInChildren<Renderer>(true)
                    .SelectMany(renderer => renderer.sharedMaterials)
                    .Where(material => material != null &&
                        new[] { "Skin", "Shadow", "Hair", "Eye" }.Any(name =>
                            ExactInstanceName(material.name, ObjectPrefix + "_" + name)) &&
                        material.shader != null && material.shader.name == "PF/StandardDynamic" &&
                        material.HasProperty("_MainTex") && material.HasProperty("_Parameters") &&
                        owned.Textures.Any(texture => ReferenceEquals(material.GetTexture("_MainTex"), texture) ||
                            ReferenceEquals(material.GetTexture("_Parameters"), texture)));
            return owned.Materials.Concat(assigned).Where(material =>
                    material != null)
                .GroupBy(material => material.GetInstanceID())
                .Select(group => group.First()).ToArray();
        }

        internal static UnityEngine.Object[] CaptureOwnedResources(
            UnitEntityView view)
        {
            Attachment owned;
            if (view == null || !Applied.TryGetValue(view, out owned))
                return new UnityEngine.Object[0];
            return (owned.Root == null ? Enumerable.Empty<UnityEngine.Object>() :
                    new UnityEngine.Object[] { owned.Root })
                .Concat(OwnedMaterials(owned).Cast<UnityEngine.Object>())
                .Concat(owned.Textures.Where(value => value != null).Cast<UnityEngine.Object>())
                .Concat(owned.Mesh == null ? Enumerable.Empty<UnityEngine.Object>() :
                    new UnityEngine.Object[] { owned.Mesh })
                .ToArray();
        }

        internal static JObject ObserveMaterialControl(UnitEntityView view)
        {
            Attachment owned;
            // A retained managed view key can still expose its final cleanup
            // receipt after Unity has destroyed the native view object.
            if (ReferenceEquals(view, null) || !Applied.TryGetValue(view, out owned))
                return new JObject { ["attachmentPresent"] = false };
            Material[] assigned = owned.Root == null ? new Material[0] : owned.Root
                .GetComponentsInChildren<Renderer>(true).SelectMany(renderer => renderer.sharedMaterials).ToArray();
            FadeDefaults sourceCached = null;
            Material currentSource = null;
            string sourceObservationError = null;
            if (view != null && owned.Root != null)
            {
                try { currentSource = RequireHillBodyMaterial(view); }
                catch (Exception error) { sourceObservationError = error.GetType().Name + ":" + error.Message; }
            }
            bool sourceHasCache = currentSource != null && owned.Controller != null &&
                TryGetFadeDefaults(owned.Controller, currentSource, out sourceCached);
            return new JObject { ["attachmentPresent"] = true, ["outcome"] = owned.Outcome,
                ["controller"] = owned.Controller == null ? 0 : owned.Controller.GetInstanceID(),
                ["sourceMaterial"] = owned.SourceMaterial == null ? 0 : owned.SourceMaterial.GetInstanceID(),
                ["sourceCachedDissolveBaseline"] = owned.SourceDefaultDissolve,
                ["attachSourceCachedFadeDefaults"] = owned.AttachedSourceFadeDefaults?.DeepClone(),
                ["attachMaterialCachedFadeDefaults"] = owned.AttachedMaterialFadeDefaults?.DeepClone(),
                ["currentSourceMaterial"] = currentSource == null ? 0 : currentSource.GetInstanceID(),
                ["currentSourceMatchesAttachedMaterial"] = ReferenceEquals(currentSource, owned.SourceMaterial),
                ["currentSourceObservationError"] = sourceObservationError,
                ["currentSourceNativeDefaultCached"] = sourceHasCache,
                ["currentSourceCachedFadeDefaults"] = DescribeFadeDefaults(sourceCached),
                ["currentSourceMismatchesFromAttach"] = FadeBaselineMismatches(sourceCached, owned.Baseline),
                ["controllerCleanup"] = owned.ControllerCleanup,
                ["headMaterials"] = new JArray(assigned.Take(4).Select(material => {
                    FadeDefaults cached = null;
                    IList<Material> driven = ControllerMaterials(owned.Controller);
                    bool member = material != null && driven != null && driven.Contains(material);
                    bool hasCache = material != null && owned.Controller != null &&
                        TryGetFadeDefaults(owned.Controller, material, out cached);
                    return new JObject { ["material"] = material == null ? 0 : material.GetInstanceID(),
                        ["name"] = material?.name, ["controllerMember"] = member,
                        ["nativeDefaultCached"] = hasCache,
                        ["fullCachedFadeBaselineMatchesSource"] = hasCache && SameFadeBaseline(cached, owned.Baseline),
                        ["cachedFadeDefaults"] = DescribeFadeDefaults(cached),
                        ["cachedFadeMismatchesFromAttach"] = FadeBaselineMismatches(cached, owned.Baseline),
                        ["cachedFadeMismatchesFromCurrentSource"] = FadeBaselineMismatches(cached, sourceCached),
                        ["fadeTextureReferenceMatchesAttach"] = hasCache && owned.Baseline != null &&
                            ReferenceEquals(cached.DissolveTexture, owned.Baseline.DissolveTexture),
                        ["fadeTextureNativeEqualsAttach"] = hasCache && owned.Baseline != null &&
                            cached.DissolveTexture == owned.Baseline.DissolveTexture,
                        ["liveDissolve"] = material != null && material.HasProperty("_Dissolve") ?
                            (JToken)new JValue(material.GetFloat("_Dissolve")) : JValue.CreateNull(),
                        ["cachedDissolve"] = hasCache ? (JToken)new JValue(cached.Dissolve) : JValue.CreateNull() };
                })), ["assignedCount"] = assigned.Length,
                ["observationOnly"] = true, ["fadeAmountForced"] = false };
        }

        internal static void ReleaseView(UnitEntityView view)
        {
            Attachment owned;
            if (view == null || !Applied.TryGetValue(view, out owned)) return;
            Release(owned);
            owned.Outcome = "released";
        }
    }

    /// <summary>Two heads share expression motion, while the second skull has
    /// its own neck-relative pivot. Owned by the attachment root.</summary>
    internal sealed class EttinHeadPoseDriver : MonoBehaviour
    {
        private Transform _source;
        private Transform _skull;
        private Vector3 _offset;

        internal void Configure(Transform source, Transform skull,
            Vector3 offset)
        {
            _source = source;
            _skull = skull;
            _offset = offset;
            Follow();
        }

        private void LateUpdate() { Follow(); }

        private void Follow()
        {
            if (_source == null || _skull == null) return;
            _skull.localPosition = _source.localPosition + _offset;
            _skull.localRotation = _source.localRotation;
            _skull.localScale = _source.localScale;
        }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class EttinSecondHeadTeardown
    {
        private static void Prefix(UnitEntityView __instance)
        {
            try { EttinSecondHeadVisual.ReleaseView(__instance); }
            catch (Exception) { /* Native view destruction must complete. */ }
        }
    }
}
