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
    /// Geometry, colors and ownership are project-created at runtime. The
    /// donor's first head, native animations, meshes and materials stay intact.
    /// The second head follows the existing head bone; independent facial
    /// animation is deliberately outside this sprint's visual standard.</summary>
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
            internal readonly List<Material> Materials = new List<Material>();
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
                if (Applied.TryGetValue(__instance, out ignored)) return;
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
                    owned.Root = new GameObject(ObjectPrefix + "_Root");
                    owned.Root.transform.SetParent(heads[0], false);
                    owned.Root.transform.localPosition =
                        new Vector3(.63f, .04f, .02f);
                    owned.Root.transform.localRotation = Quaternion.identity;
                    Material skin = MakeMaterial(owned, "Skin",
                        new Color(.55f, .43f, .39f, 1f));
                    Material shadow = MakeMaterial(owned, "Shadow",
                        new Color(.31f, .25f, .23f, 1f));
                    Material hair = MakeMaterial(owned, "Hair",
                        new Color(.19f, .17f, .16f, 1f));
                    Material eye = MakeMaterial(owned, "Eye",
                        new Color(.89f, .83f, .68f, 1f));
                    Part(owned, "Skull", skin, new Vector3(0f, .12f, 0f),
                        new Vector3(.48f, .59f, .43f));
                    Part(owned, "Jaw", shadow,
                        new Vector3(0f, -.18f, .15f),
                        new Vector3(.34f, .18f, .31f));
                    Part(owned, "Nose", skin,
                        new Vector3(0f, .08f, .38f),
                        new Vector3(.16f, .22f, .22f));
                    Part(owned, "BrowLeft", hair,
                        new Vector3(-.20f, .23f, .34f),
                        new Vector3(.20f, .08f, .10f));
                    Part(owned, "BrowRight", hair,
                        new Vector3(.20f, .23f, .34f),
                        new Vector3(.20f, .08f, .10f));
                    Part(owned, "EyeLeft", eye,
                        new Vector3(-.20f, .15f, .39f),
                        new Vector3(.09f, .07f, .05f));
                    Part(owned, "EyeRight", eye,
                        new Vector3(.20f, .15f, .39f),
                        new Vector3(.09f, .07f, .05f));
                    Part(owned, "Crown", hair,
                        new Vector3(0f, .39f, -.04f),
                        new Vector3(.47f, .19f, .42f));
                    owned.Outcome = "attached;heads=2;original=1;project=1;parts=8";
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

        private static Material MakeMaterial(Attachment owned, string name,
            Color color)
        {
            Shader shader = Shader.Find("Standard") ??
                Shader.Find("Legacy Shaders/Diffuse");
            if (shader == null)
                throw new InvalidOperationException(
                    "No visible shader for original Ettin head.");
            var material = new Material(shader) {
                name = ObjectPrefix + "_" + name,
                color = color
            };
            owned.Materials.Add(material);
            return material;
        }

        private static void Part(Attachment owned, string name,
            Material material, Vector3 position, Vector3 scale)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            part.name = ObjectPrefix + "_" + name;
            part.transform.SetParent(owned.Root.transform, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void Release(Attachment owned)
        {
            if (owned == null) return;
            // Native renderer setup can instantiate one material per primitive
            // after our four shared originals have been assigned. Every
            // renderer below this exact project-created root belongs to this
            // head, so capture those instances before destroying the root.
            var materials = OwnedMaterials(owned);
            if (owned.Root != null)
                UnityEngine.Object.DestroyImmediate(owned.Root);
            owned.Root = null;
            foreach (Material material in materials)
                if (material != null)
                    UnityEngine.Object.DestroyImmediate(material);
            owned.Materials.Clear();
        }

        private static Material[] OwnedMaterials(Attachment owned)
        {
            if (owned == null) return new Material[0];
            var assigned = owned.Root == null ? Enumerable.Empty<Material>() :
                owned.Root.GetComponentsInChildren<Renderer>(true)
                    .SelectMany(renderer => renderer.sharedMaterials);
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
