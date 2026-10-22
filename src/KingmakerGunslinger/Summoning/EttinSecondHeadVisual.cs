using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
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
        private sealed class Attachment
        {
            internal GameObject Root;
            internal Mesh Mesh;
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
                var owned = new Attachment { Outcome = "attach-started" };
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
                    owned.Root = new GameObject(ObjectPrefix + "_Root");
                    owned.Root.transform.SetParent(neck, false);
                    owned.Root.transform.localPosition = Vector3.zero;
                    owned.Root.transform.localRotation = Quaternion.identity;
                    owned.Root.transform.localScale = Vector3.one;
                    Transform skull = new GameObject(ObjectPrefix +
                        "_SkullControl").transform;
                    skull.SetParent(owned.Root.transform, false);
                    Vector3 offset = head.localRotation *
                        Vector3.Scale(head.localScale,
                            new Vector3(.52f, .02f, 0f));
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
                    Material skin = MakeMaterial(owned, "Skin", template, albedo, parameters);
                    Material shadow = MakeMaterial(owned, "Shadow", template, albedo, parameters);
                    Material hair = MakeMaterial(owned, "Hair", template, albedo, parameters);
                    Material eye = MakeMaterial(owned, "Eye", template, albedo, parameters);
                    var renderer = owned.Root.AddComponent<SkinnedMeshRenderer>();
                    renderer.sharedMesh = owned.Mesh;
                    renderer.bones = new[] { neck, skull };
                    renderer.rootBone = neck;
                    renderer.sharedMaterials = new[] { skin, shadow, hair, eye };
                    Bounds bounds = owned.Mesh.bounds;
                    bounds.Expand(.55f);
                    renderer.localBounds = bounds;
                    renderer.updateWhenOffscreen = true;
                    owned.Outcome = "attached;heads=2;original=1;project=1;" +
                        "authored=1;continuous-neck=1;orbit=0;" +
                        "shader=PF/StandardDynamic;original-paint=1;owned-textures=2";
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
            Material template, Texture2D albedo, Texture2D parameters)
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
            foreach (string slot in new[] { "_DissolveTex", "_PetrificationTex" })
                if (material.HasProperty(slot)) material.SetTexture(slot, null);
            material.SetColor("_TintColor", Color.white);
            material.SetFloat("_Roughness", 1f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Emission", 0f);
            return material;
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
