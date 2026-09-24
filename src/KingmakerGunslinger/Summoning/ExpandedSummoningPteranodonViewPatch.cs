using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.View;
using KingmakerGunslinger.Assets;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Attaches the original Pteranodon visual to one live summoned unit.
    ///
    /// The donor prefab is <c>GiantEagle</c>, shared by eagle, dire bat,
    /// pteranodon and roc. Every change here is therefore instance-local: the
    /// shared prefab, its mesh, its materials and its animator are never
    /// touched, and eagle, dire bat and roc are the negative controls that prove
    /// it.
    ///
    /// The visual rides the donor's own <see cref="SkinnedMeshRenderer"/>
    /// component, on this one instance: its mesh, bone array and material are
    /// swapped for the Pteranodon's, and nothing else about the view changes.
    /// That is deliberate. The game drives a unit's renderers by reference -
    /// <c>EntityFader</c> hides and fades them in on summon, the FX visibility
    /// manager and the occlusion highlighter cache them, hit flashes and the
    /// death dissolve write to their materials - and a renderer added beside
    /// the donor's would sit outside every one of those, visible through fog,
    /// opaque during the fade, untouched by a hit. The first live isolation
    /// check found exactly that: freshly summoned units had their donor
    /// renderer disabled by the fader while a sibling would have stayed on.
    ///
    /// The invariant is that a unit is always either custom-visual-attached or
    /// donor-visual-intact. It is never invisible, never double-bodied and never
    /// half-initialised. Any failure before the swap simply leaves the donor
    /// alone; any failure after it puts the original mesh, bones and materials
    /// back on the same component and destroys what was made.
    /// </summary>
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class ExpandedSummoningPteranodonViewPatch
    {
        internal const string PteranodonBlueprintName =
            "KMG_Summoning_Unit_Pteranodon";
        /// <summary>
        /// The name carried by the private mesh and material the swap installs;
        /// observers recognise the attached state by it.
        /// </summary>
        internal const string CustomVisualName = "KMG_PteranodonMembrane";
        private const string MainTexture = "_MainTex";

        /// <summary>
        /// Fault injection for the guarded fallback drill. When set, it runs at
        /// the one point where a failure is most expensive - after the swap has
        /// been made - so the rollback path is exercised on a live unit. Only
        /// the runtime-testing fixture sets it, and it clears it again in the
        /// same cast.
        /// </summary>
        internal static Action PostSuppressionFaultForTest;

        /// <summary>
        /// Texture slots the donor's material may carry that the painting does
        /// not: the eagle's normal, specular, occlusion, emission, mask and
        /// detail maps are indexed by the eagle's texture coordinates, which
        /// mean nothing on this mesh. Each one present is cleared on the
        /// private copy, and the shader falls back to its flat default for that
        /// slot. Unity 2018 cannot enumerate a shader's properties at runtime,
        /// so this is a probe list - the Standard shader's names plus the
        /// spellings Owlcat's PF shaders use - and the outcome records which of
        /// them the donor's shader actually declares.
        /// </summary>
        private static readonly string[] SuppressedMaps =
        {
            "_BumpMap", "_NormalMap", "_NormalTex", "_SpecGlossMap", "_SpecularMap",
            "_SpecTex", "_MetallicGlossMap", "_OcclusionMap", "_EmissionMap",
            "_EmissiveMap", "_EmissionTex", "_MaskMap", "_MaskTex", "_Mask",
            "_DetailAlbedoMap", "_DetailNormalMap", "_DetailMask", "_ParallaxMap",
            "_RampTex", "_ColorMask", "_TintMask", "_GlossMap", "_DecalTex",
            "_DetailTex", "_DirtTex", "_SecondaryTex"
        };

        private sealed class Attachment
        {
            internal string Outcome;
            internal SkinnedMeshRenderer Donor;
            internal Mesh OriginalMesh;
            internal Transform[] OriginalBones;
            internal Material[] OriginalMaterials;
            internal Material Material;
            internal Mesh Mesh;
        }

        private static readonly ConditionalWeakTable<UnitEntityView, Attachment>
            Applied = new ConditionalWeakTable<UnitEntityView, Attachment>();
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        /// <summary>
        /// Outcome strings in attachment order, for guarded runtime observation.
        /// Bounded so a long session cannot grow it without limit.
        /// </summary>
        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) return Outcomes.ToArray(); } }

        internal static string DescribeView(UnitEntityView view)
        {
            Attachment attachment;
            if (view == null || !Applied.TryGetValue(view, out attachment))
                return "not-attempted";
            return attachment.Outcome;
        }

        /// <summary>
        /// The donor's own rig as this view's renderer held it before the
        /// swap - the 72 bones and bind poses the Pteranodon was authored
        /// against. False when nothing was swapped on the view, in which case
        /// the renderer itself still carries that rig.
        /// </summary>
        internal static bool TryGetDonorRig(UnitEntityView view,
            out Transform[] bones, out Matrix4x4[] bindposes)
        {
            bones = null;
            bindposes = null;
            Attachment attachment;
            if (view == null || !Applied.TryGetValue(view, out attachment) ||
                attachment.Mesh == null || attachment.OriginalMesh == null ||
                attachment.OriginalBones == null)
                return false;
            bones = attachment.OriginalBones;
            bindposes = attachment.OriginalMesh.bindposes;
            return true;
        }

        private static void Record(string outcome)
        {
            lock (Sync)
            {
                if (Outcomes.Count >= 256) Outcomes.RemoveAt(0);
                Outcomes.Add(outcome);
            }
        }

        private static void Postfix(UnitEntityView __instance)
        {
            if (__instance == null || __instance.EntityData == null ||
                __instance.EntityData.Blueprint == null) return;
            if (!string.Equals(__instance.EntityData.Blueprint.name,
                PteranodonBlueprintName, StringComparison.Ordinal)) return;

            lock (Applied)
            {
                Attachment existing;
                if (Applied.TryGetValue(__instance, out existing)) return;
                Attachment attachment = new Attachment();
                Applied.Add(__instance, attachment);
                attachment.Outcome = Attach(__instance, attachment);
                Record(attachment.Outcome);
            }
        }

        /// <summary>
        /// Everything is validated before the donor renderer is touched, so
        /// the common failure is a no-op rather than a rollback.
        /// </summary>
        private static string Attach(UnitEntityView view, Attachment attachment)
        {
            Mesh source;
            string[] boneNames;
            if (!PteranodonAssetRuntime.TryGetMembrane(out source, out boneNames))
                return Fallback(PteranodonAssetRuntime.Status);
            Texture2D albedo;
            if (!PteranodonAssetRuntime.TryGetAlbedo(out albedo))
                return Fallback(PteranodonAssetRuntime.Status);

            SkinnedMeshRenderer[] donors = view
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null)
                .ToArray();
            if (donors.Length != 1)
                return "donor-visual:donor-renderer-count=" + donors.Length;
            SkinnedMeshRenderer donor = donors[0];

            Transform[] bones;
            Matrix4x4[] bindposes;
            string reason;
            if (!PteranodonAssetRuntime.TryResolveDonorBinding(donor, boneNames,
                out bones, out bindposes, out reason))
                return "donor-visual:" + reason;

            if (donor.rootBone == null) return "donor-visual:donor-root-bone-missing";
            Material donorMaterial = donor.sharedMaterial;
            if (donorMaterial == null)
                return "donor-visual:donor-material-missing";
            // The painting can only be shown through a main texture slot. A
            // shader without one is not something this patch knows how to
            // paint, so the donor stays; nothing has been changed yet.
            if (!donorMaterial.HasProperty(MainTexture))
                return "donor-visual:donor-material-has-no-main-texture";

            // What the rollback puts back: the references this instance's
            // component holds right now. The shared assets behind them are
            // never modified, so restoring the references restores the donor.
            attachment.Donor = donor;
            attachment.OriginalMesh = donor.sharedMesh;
            attachment.OriginalBones = donor.bones;
            attachment.OriginalMaterials = donor.sharedMaterials;

            Material material = null;
            Mesh mesh = null;
            bool swapped = false;
            try
            {
                // A copy, so the cached asset keeps its identity bind poses and
                // a second unit binds from the same clean source.
                mesh = UnityEngine.Object.Instantiate(source);
                mesh.name = CustomVisualName;
                mesh.bindposes = bindposes;

                // Cloned from the donor's material so the creature is shaded by
                // the game's own pipeline rather than a bundled stand-in; then
                // the eagle's textures are replaced by the painting.
                material = new Material(donorMaterial);
                material.name = CustomVisualName;
                string dressing = DressMaterial(material, albedo);

                // The swap, on this one instance's renderer component: the
                // Pteranodon's mesh, the 46 bones its weights index, and its
                // material. Root bone, bounds, shadow modes, quality and the
                // component's enabled state stay whatever the game set.
                swapped = true;
                donor.sharedMesh = mesh;
                donor.bones = bones;
                donor.sharedMaterials = new[] { material };

                attachment.Material = material;
                attachment.Mesh = mesh;

                Action fault = PostSuppressionFaultForTest;
                if (fault != null) fault();
                return "visual:attached;bones=" + bones.Length +
                    ";vertices=" + mesh.vertexCount + ";albedo=" +
                    albedo.width + "x" + albedo.height + ";rendererEnabled=" +
                    (donor.enabled ? "true" : "false") + ";" + dressing;
            }
            catch (Exception error)
            {
                if (swapped) Revert(attachment);
                if (material != null) UnityEngine.Object.Destroy(material);
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                attachment.Material = null;
                attachment.Mesh = null;
                return "donor-visual:attach-failed:" + error.GetType().Name;
            }
        }

        /// <summary>
        /// Puts the painting on the private material copy and takes the
        /// eagle's own maps off it. Returns what was done, for the evidence
        /// record: which slots the shader declares, which were cleared, and
        /// the tint the donor material carried, which is reset to white so the
        /// albedo renders as painted.
        /// </summary>
        private static string DressMaterial(Material material, Texture2D albedo)
        {
            material.SetTexture(MainTexture, albedo);
            material.SetTextureScale(MainTexture, Vector2.one);
            material.SetTextureOffset(MainTexture, Vector2.zero);

            var declared = new List<string>();
            var cleared = new List<string>();
            foreach (string name in SuppressedMaps)
            {
                if (!material.HasProperty(name)) continue;
                declared.Add(name);
                if (material.GetTexture(name) == null) continue;
                material.SetTexture(name, null);
                cleared.Add(name);
            }

            string tint = "<none>";
            if (material.HasProperty("_Color"))
            {
                Color original = material.GetColor("_Color");
                tint = Describe(original);
                material.SetColor("_Color", Color.white);
            }

            string emission = "<none>";
            if (material.HasProperty("_EmissionColor"))
            {
                emission = Describe(material.GetColor("_EmissionColor"));
                material.SetColor("_EmissionColor", Color.black);
            }

            return "shader=" + (material.shader == null ? "<null>" :
                material.shader.name) + ";declared=" + (declared.Count == 0 ?
                "<none>" : string.Join(",", declared.ToArray())) +
                ";cleared=" + (cleared.Count == 0 ?
                "<none>" : string.Join(",", cleared.ToArray())) +
                ";donorTint=" + tint + ";donorEmission=" + emission;
        }

        private static string Describe(Color value)
        {
            return string.Join("/", new[]
            {
                value.r.ToString("0.###", CultureInfo.InvariantCulture),
                value.g.ToString("0.###", CultureInfo.InvariantCulture),
                value.b.ToString("0.###", CultureInfo.InvariantCulture),
                value.a.ToString("0.###", CultureInfo.InvariantCulture)
            });
        }

        /// <summary>The loader's own statuses already carry the prefix.</summary>
        private static string Fallback(string status)
        {
            return status != null &&
                status.StartsWith("donor-visual:", StringComparison.Ordinal)
                ? status : "donor-visual:" + status;
        }

        /// <summary>
        /// Puts the donor back exactly as it was: the same component, the
        /// references it held before the swap.
        /// </summary>
        private static void Revert(Attachment attachment)
        {
            SkinnedMeshRenderer donor = attachment.Donor;
            if (donor == null) return;
            donor.sharedMesh = attachment.OriginalMesh;
            donor.bones = attachment.OriginalBones;
            donor.sharedMaterials = attachment.OriginalMaterials;
        }
    }
}
