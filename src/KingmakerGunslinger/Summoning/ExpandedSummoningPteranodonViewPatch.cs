using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Bootstrap;
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
        internal const string DireBatBlueprintName =
            "KMG_Summoning_Unit_DireBat";
        internal const string EagleBlueprintName =
            "KMG_Summoning_Unit_Eagle";
        internal const string GiantWaspBlueprintName =
            "KMG_Summoning_Unit_GiantWasp";
        internal const string StirgeBlueprintName =
            "KMG_Summoning_Unit_Stirge";
        internal const string AurochsBlueprintName =
            "KMG_Summoning_Unit_Aurochs";
        internal const string BisonBlueprintName =
            "KMG_Summoning_Unit_Bison";
        internal const string RhinocerosBlueprintName =
            "KMG_Summoning_Unit_Rhinoceros";
        internal const string WoollyRhinocerosBlueprintName =
            "KMG_Summoning_Unit_WoollyRhinoceros";
        internal const string DireRatBlueprintName =
            "KMG_Summoning_Unit_DireRat";
        internal const string HyenaBlueprintName =
            "KMG_Summoning_Unit_Hyena";
        internal const string GoblinDogBlueprintName =
            "KMG_Summoning_Unit_GoblinDog";
        internal const string WolverineBlueprintName =
            "KMG_Summoning_Unit_Wolverine";
        internal const string ShadowMastiffBlueprintName =
            "KMG_Summoning_Unit_ShadowMastiff";
        internal const string PoisonousFrogBlueprintName =
            "KMG_Summoning_Unit_PoisonousFrog";
        internal const string FireBeetleBlueprintName =
            "KMG_Summoning_Unit_FireBeetle";
        internal const string GiantAntWorkerBlueprintName =
            "KMG_Summoning_Unit_GiantAntWorker";
        internal const string GiantAntSoldierBlueprintName =
            "KMG_Summoning_Unit_GiantAntSoldier";
        internal const string GiantAntDroneBlueprintName =
            "KMG_Summoning_Unit_GiantAntDrone";
        internal const string GiantStagBeetleBlueprintName =
            "KMG_Summoning_Unit_GiantStagBeetle";
        internal const string GiantScorpionBlueprintName =
            "KMG_Summoning_Unit_GiantScorpion";
        internal const string GiantCrabBlueprintName =
            "KMG_Summoning_Unit_GiantCrab";
        internal const string BebelithBlueprintName =
            "KMG_Summoning_Unit_Bebelith";
        /// <summary>
        /// The name carried by the private mesh and material the swap installs;
        /// observers recognise the attached state by it.
        /// </summary>
        internal const string CustomVisualName = "KMG_PteranodonMembrane";
        internal const string DireBatVisualName = "KMG_DireBatMembrane";
        internal const string EagleVisualName = "KMG_EagleFeathers";
        internal const string GiantWaspVisualName = "KMG_GiantWaspMembrane";
        internal const string StirgeVisualName = "KMG_StirgeMembrane";
        private static readonly Dictionary<string, string> VisualKeys =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { PteranodonBlueprintName, "pteranodon" },
                { DireBatBlueprintName, "dire-bat" },
                { EagleBlueprintName, "eagle" },
                { GiantWaspBlueprintName, "giant-wasp" },
                { StirgeBlueprintName, "stirge" },
                { AurochsBlueprintName, "aurochs" },
                { BisonBlueprintName, "bison" },
                { RhinocerosBlueprintName, "rhinoceros" },
                { WoollyRhinocerosBlueprintName, "woolly-rhinoceros" },
                { DireRatBlueprintName, "dire-rat" },
                { HyenaBlueprintName, "hyena" },
                { GoblinDogBlueprintName, "goblin-dog" },
                { WolverineBlueprintName, "wolverine" },
                { ShadowMastiffBlueprintName, "shadow-mastiff" },
                { PoisonousFrogBlueprintName, "poisonous-frog" },
                { FireBeetleBlueprintName, "fire-beetle" },
                { GiantAntWorkerBlueprintName, "giant-ant-worker" },
                { GiantAntSoldierBlueprintName, "giant-ant-soldier" },
                { GiantAntDroneBlueprintName, "giant-ant-drone" },
                { GiantStagBeetleBlueprintName, "giant-stag-beetle" },
                { GiantScorpionBlueprintName, Sprint14BonePolicy.ScorpionKey },
                { GiantCrabBlueprintName, Sprint14BonePolicy.CrabKey },
                // The only released creature in this table. Sprint 20 learned
                // the cost of the opposite mistake the expensive way: its
                // scorpion was missing from here and would have spawned
                // wearing a Giant Spider.
                { BebelithBlueprintName, Sprint14BonePolicy.BebelithKey },
                { "KMG_Summoning_Unit_Crocodile", "crocodile" },
                { "KMG_Summoning_Unit_Gorgon", "gorgon" },
                { "KMG_Summoning_Unit_DireCrocodile", "dire-crocodile" },
                { AnkylosaurusVisualPolicy.BlueprintName, AnkylosaurusRulesPolicy.Key },
                { GiantBodyVisualPolicy.BrachiosaurusBlueprintName,
                    GiantBodyRulesPolicy.BrachiosaurusKey },
                { GiantBodyVisualPolicy.ElephantBlueprintName,
                    GiantBodyRulesPolicy.ElephantKey },
                { ArmoredDinosaurVisualPolicy.TriceratopsBlueprintName,
                    ArmoredDinosaurRulesPolicy.TriceratopsKey },
                { ArmoredDinosaurVisualPolicy.StegosaurusBlueprintName,
                    ArmoredDinosaurRulesPolicy.StegosaurusKey },
                { TheropodVisualPolicy.DeinonychusBlueprintName,
                    TheropodRulesPolicy.DeinonychusKey },
                { TheropodVisualPolicy.TyrannosaurusBlueprintName,
                    TheropodRulesPolicy.TyrannosaurusKey }
            };
        private static readonly HashSet<string> UngulateKeys =
            new HashSet<string>(StringComparer.Ordinal)
            { "aurochs", "bison", "rhinoceros", "woolly-rhinoceros", "gorgon" };
        private static readonly HashSet<string> Sprint12QuadrupedKeys =
            new HashSet<string>(StringComparer.Ordinal)
            { "dire-rat", "hyena", "goblin-dog" };
        private static readonly HashSet<string> Sprint13CreatureKeys =
            new HashSet<string>(StringComparer.Ordinal)
            { "wolverine", "shadow-mastiff", "poisonous-frog" };
        // The whole insect family on the Giant Spider rig, Sprints 14 and 15
        // together: they share a donor, a bone policy and an asset pipeline, so
        // every seam that asks "is this one of the insects" wants all five.
        // Sprint 20's Giant Scorpion joins them: same donor, same asset
        // pipeline, same loader. Its bone policy differs - it is the one
        // creature here that walks on the fourth leg chain - but every seam
        // that asks "does this creature ride the Giant Spider rig" wants it.
        private static readonly HashSet<string> Sprint14InsectKeys =
            new HashSet<string>(StringComparer.Ordinal)
            { "fire-beetle", "giant-ant-worker", "giant-ant-soldier",
              "giant-ant-drone", "giant-stag-beetle",
              Sprint14BonePolicy.ScorpionKey, Sprint14BonePolicy.CrabKey,
              Sprint14BonePolicy.BebelithKey };
        private const string MainTexture = "_MainTex";
        private static bool IsCrocodilian(string key)
        { return CrocodilianVisualPolicy.Keys.Contains(key, StringComparer.Ordinal); }

        internal static bool HandlesBlueprintName(string blueprintName)
        {
            return blueprintName != null && VisualKeys.ContainsKey(blueprintName);
        }

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
            internal string VisualKey;
            internal SkinnedMeshRenderer Donor;
            internal Mesh OriginalMesh;
            internal Transform[] OriginalBones;
            internal Material[] OriginalMaterials;
            internal Material Material;
            internal Mesh Mesh;
            internal SkinnedMeshRenderer[] SuppressedRenderers;
            internal bool[] OriginalSuppressedRendererStates;
            internal TheropodAccessorySuppression TheropodSuppression;
            internal EagleAttackVisualLunge EagleLunge;
            internal GiantWaspVisualSting WaspSting;
            internal CrocodilianAttackVisualPose CrocodilianPose;
            internal AnkylosaurusTailImpactVisual AnkylosaurusImpact;
            internal StirgeVisualTouch StirgeTouch;
            internal FireBeetleVisualGlow BeetleGlow;
        }

        private static readonly ConditionalWeakTable<UnitEntityView, Attachment>
            Applied = new ConditionalWeakTable<UnitEntityView, Attachment>();
        private static readonly FieldInfo BaseCorpulence = typeof(UnitEntityView)
            .GetField("m_Corpulence", BindingFlags.Instance |
                BindingFlags.NonPublic);
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
            Mesh ignored;
            return TryGetDonorRig(view, out bones, out bindposes, out ignored);
        }

        internal static bool TryGetDonorRig(UnitEntityView view,
            out Transform[] bones, out Matrix4x4[] bindposes,
            out Mesh originalMesh)
        {
            bones = null;
            bindposes = null;
            originalMesh = null;
            Attachment attachment;
            if (view == null || !Applied.TryGetValue(view, out attachment) ||
                attachment.Mesh == null || attachment.OriginalMesh == null ||
                attachment.OriginalBones == null)
                return false;
            bones = attachment.OriginalBones;
            bindposes = attachment.OriginalMesh.bindposes;
            originalMesh = attachment.OriginalMesh;
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
            string blueprintName = __instance.EntityData.Blueprint.name;
            string visualKey;
            if (!VisualKeys.TryGetValue(blueprintName, out visualKey)) return;
            if (TheropodVisualPolicy.Handles(visualKey))
            {
                ModContext context;
                string exactKey;
                var blueprint = __instance.EntityData.Blueprint;
                bool moduleOn = ModContext.TryGet(out context) &&
                    context.FeatureModules != null &&
                    context.FeatureModules.Active != null &&
                    context.FeatureModules.Active.ExpandedSummoning;
                // The exact-identity gate only has meaning while the module is
                // on, because TryProductionTheropod refuses outright when it is
                // off. Returning here with the module off would skip the
                // attachment record altogether and report "not-attempted",
                // which deliberately does not prove this registered
                // save-compatible identity reached the asset loader's
                // module-disabled refusal. So fall through instead and let
                // Attach record that refusal, exactly as the armored dinosaurs
                // and every other family do. The loader still performs no file
                // read and no Unity allocation while the module is off.
                if (moduleOn &&
                    (!TheropodVisualPolicy.TryProductionTheropod(true,
                        blueprint.AssetGuid, blueprint.name,
                        blueprint.Prefab == null ? null :
                            blueprint.Prefab.AssetId, out exactKey) ||
                    exactKey != visualKey))
                    return;
            }

            lock (Applied)
            {
                Attachment existing;
                if (Applied.TryGetValue(__instance, out existing)) return;
                Attachment attachment = new Attachment();
                attachment.VisualKey = visualKey;
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
            string spatial = string.Empty;
            if (TheropodVisualPolicy.Handles(attachment.VisualKey))
            {
                string failure;
                if (!TryConfigureTheropodSpatial(view, attachment.VisualKey,
                        out spatial, out failure))
                    return "donor-visual:theropod-spatial-failed:" + failure;
            }
            Mesh source;
            string[] boneNames;
            Texture2D albedo;
            if (attachment.VisualKey == "dire-bat")
            {
                if (!PteranodonAssetRuntime.TryGetDireBatVisual(out source,
                    out boneNames, out albedo))
                    return Fallback(PteranodonAssetRuntime.DireBatStatus);
            }
            else if (attachment.VisualKey == "eagle")
            {
                if (!PteranodonAssetRuntime.TryGetEagleVisual(out source,
                    out boneNames, out albedo))
                    return Fallback(PteranodonAssetRuntime.EagleStatus);
            }
            else if (attachment.VisualKey == "giant-wasp")
            {
                if (!PteranodonAssetRuntime.TryGetGiantWaspVisual(out source,
                    out boneNames, out albedo))
                    return Fallback(PteranodonAssetRuntime.GiantWaspStatus);
            }
            else if (attachment.VisualKey == "stirge")
            {
                if (!PteranodonAssetRuntime.TryGetStirgeVisual(out source,
                    out boneNames, out albedo))
                    return Fallback(PteranodonAssetRuntime.StirgeStatus);
            }
            else if (attachment.VisualKey == AnkylosaurusRulesPolicy.Key)
            {
                string status;
                if (!PteranodonAssetRuntime.TryGetAnkylosaurusVisual(out source, out boneNames, out albedo, out status))
                    return Fallback(status);
            }
            else if (ArmoredDinosaurVisualPolicy.Handles(
                attachment.VisualKey))
            {
                string status;
                if (!PteranodonAssetRuntime.TryGetArmoredDinosaurVisual(
                    attachment.VisualKey, out source, out boneNames,
                    out albedo, out status))
                    return Fallback(status);
            }
            else if (TheropodVisualPolicy.Handles(attachment.VisualKey))
            {
                string status;
                if (!PteranodonAssetRuntime.TryGetTheropodVisual(
                    attachment.VisualKey, out source, out boneNames,
                    out albedo, out status))
                    return Fallback(status);
            }
            else if (GiantBodyVisualPolicy.Handles(attachment.VisualKey))
            {
                string status;
                if (!PteranodonAssetRuntime.TryGetGiantBodyVisual(
                    attachment.VisualKey, out source, out boneNames,
                    out albedo, out status))
                    return Fallback(status);
            }
            else if (UngulateKeys.Contains(attachment.VisualKey))
            {
                string status;
                if (!PteranodonAssetRuntime.TryGetUngulateVisual(
                    attachment.VisualKey, out source, out boneNames,
                    out albedo, out status))
                    return Fallback(status);
            }
            else if (Sprint12QuadrupedKeys.Contains(attachment.VisualKey))
            {
                string status;
                if (!PteranodonAssetRuntime.TryGetSprint12QuadrupedVisual(
                    attachment.VisualKey, out source, out boneNames,
                    out albedo, out status))
                    return Fallback(status);
            }
            else if (Sprint13CreatureKeys.Contains(attachment.VisualKey))
            {
                string status;
                if (!PteranodonAssetRuntime.TryGetSprint13CreatureVisual(
                    attachment.VisualKey, out source, out boneNames,
                    out albedo, out status))
                    return Fallback(status);
            }
            else if (Sprint14InsectKeys.Contains(attachment.VisualKey))
            {
                string status;
                if (!PteranodonAssetRuntime.TryGetSprint14InsectVisual(
                    attachment.VisualKey, out source, out boneNames,
                    out albedo, out status))
                    return Fallback(status);
            }
            else if (IsCrocodilian(attachment.VisualKey))
            {
                string status;
                if (!PteranodonAssetRuntime.TryGetCrocodilianVisual(
                    attachment.VisualKey, out source, out boneNames, out albedo, out status))
                    return Fallback(status);
            }
            else
            {
                if (!PteranodonAssetRuntime.TryGetMembrane(out source,
                    out boneNames))
                    return Fallback(PteranodonAssetRuntime.Status);
                if (!PteranodonAssetRuntime.TryGetAlbedo(out albedo))
                    return Fallback(PteranodonAssetRuntime.Status);
            }

            SkinnedMeshRenderer[] donors = view
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null)
                .ToArray();
            SkinnedMeshRenderer donor;
            if (TheropodVisualPolicy.Handles(attachment.VisualKey))
            {
                if (!TheropodVisualPolicy.IsExactRendererSet(
                        donors.Select(value => value.name)))
                    return "donor-visual:theropod-renderer-set=" +
                        string.Join(",", donors.Select(value => value.name)
                            .OrderBy(value => value,
                                StringComparer.Ordinal).ToArray());
                donor = donors.Single(value => value.name ==
                    TheropodVisualPolicy.BodyRendererName);
                SkinnedMeshRenderer accessory = donors.Single(value =>
                    value.name == TheropodVisualPolicy.AccessoryRendererName);
                attachment.SuppressedRenderers = new[] { accessory };
                attachment.OriginalSuppressedRendererStates = new[] {
                    accessory.enabled };
            }
            else if (GiantBodyVisualPolicy.Handles(attachment.VisualKey))
            {
                // The measured donor exposes exactly one skinned renderer. A
                // view presenting anything else is not that donor, and the
                // original body stays uninstalled rather than being pushed
                // onto a creature it was not authored against.
                if (!GiantBodyVisualPolicy.IsExactRendererSet(
                        donors.Select(value => value.name)))
                    return "donor-visual:giant-body-renderer-set=" +
                        string.Join(",", donors.Select(value => value.name)
                            .OrderBy(value => value,
                                StringComparer.Ordinal).ToArray());
                donor = donors[0];
            }
            else
            {
                if (donors.Length != 1)
                    return "donor-visual:donor-renderer-count=" +
                        donors.Length;
                donor = donors[0];
            }

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
                string visualName = attachment.VisualKey == "dire-bat"
                    ? DireBatVisualName : attachment.VisualKey == "eagle"
                        ? EagleVisualName : attachment.VisualKey == "giant-wasp"
                            ? GiantWaspVisualName : attachment.VisualKey == "stirge"
                                ? StirgeVisualName : (UngulateKeys.Contains(
                                    attachment.VisualKey) ||
                                    Sprint12QuadrupedKeys.Contains(
                                        attachment.VisualKey) ||
                                    Sprint13CreatureKeys.Contains(
                                        attachment.VisualKey) ||
                                    Sprint14InsectKeys.Contains(
                                        attachment.VisualKey) ||
                                    IsCrocodilian(attachment.VisualKey) ||
                                    attachment.VisualKey == AnkylosaurusRulesPolicy.Key ||
                                    ArmoredDinosaurVisualPolicy.Handles(
                                        attachment.VisualKey) ||
                                    TheropodVisualPolicy.Handles(
                                        attachment.VisualKey) ||
                                    GiantBodyVisualPolicy.Handles(attachment.VisualKey))
                                    ? "KMG_" + attachment.VisualKey + "_Original"
                                    : CustomVisualName;
                mesh.name = visualName;
                mesh.bindposes = bindposes;

                // Cloned from the donor's material so the creature is shaded by
                // the game's own pipeline rather than a bundled stand-in; then
                // the eagle's textures are replaced by the painting.
                material = new Material(donorMaterial);
                material.name = visualName;
                string dressing = DressMaterial(material, albedo);

                // The swap, on this one instance's renderer component: the
                // Pteranodon's mesh, the 46 bones its weights index, and its
                // material. Root bone, bounds, shadow modes, quality and the
                // component's enabled state stay whatever the game set.
                swapped = true;
                donor.sharedMesh = mesh;
                donor.bones = bones;
                donor.sharedMaterials = new[] { material };
                if (attachment.SuppressedRenderers != null)
                    foreach (SkinnedMeshRenderer renderer in
                        attachment.SuppressedRenderers)
                        renderer.enabled = false;

                attachment.Material = material;
                attachment.Mesh = mesh;

                // The game's material controller cached the donor's materials
                // before the swap and keeps driving those: the summon's
                // dissolve-in, hit tint, death dissolve and fade-out would all
                // pass this material by, and a clone taken while the donor was
                // fully dissolved stays invisible. So the clone starts intact
                // and the controller re-reads the renderer's materials.
                string controller = AdoptByMaterialController(view, donor, material);

                if (TheropodVisualPolicy.Handles(attachment.VisualKey))
                {
                    attachment.TheropodSuppression = view.gameObject
                        .AddComponent<TheropodAccessorySuppression>();
                    attachment.TheropodSuppression.Configure(
                        attachment.SuppressedRenderers.Single());
                }

                Action fault = PostSuppressionFaultForTest;
                if (fault != null) fault();
                if (attachment.VisualKey == AnkylosaurusRulesPolicy.Key)
                {
                    attachment.AnkylosaurusImpact=view.gameObject.AddComponent<AnkylosaurusTailImpactVisual>();
                    attachment.AnkylosaurusImpact.Configure(view,donor);
                }
                if (attachment.VisualKey == "eagle")
                {
                    attachment.EagleLunge = view.gameObject
                        .AddComponent<EagleAttackVisualLunge>();
                    attachment.EagleLunge.Configure(view, donor);
                }
                if (attachment.VisualKey == "giant-wasp")
                {
                    attachment.WaspSting = view.gameObject
                        .AddComponent<GiantWaspVisualSting>();
                    attachment.WaspSting.Configure(view, donor);
                }
                if (IsCrocodilian(attachment.VisualKey))
                {
                    attachment.CrocodilianPose = view.gameObject
                        .AddComponent<CrocodilianAttackVisualPose>();
                    attachment.CrocodilianPose.Configure(attachment.VisualKey, view, donor);
                }
                if (attachment.VisualKey == "stirge")
                {
                    attachment.StirgeTouch = view.gameObject
                        .AddComponent<StirgeVisualTouch>();
                    attachment.StirgeTouch.Configure(view, donor);
                }
                if (attachment.VisualKey == "fire-beetle")
                {
                    // A light this view owns, matching the creature's painted
                    // glands. It carries no rule: Kingmaker has no
                    // mechanics-layer illumination model and this does not add
                    // one.
                    attachment.BeetleGlow = view.gameObject
                        .AddComponent<FireBeetleVisualGlow>();
                    attachment.BeetleGlow.Configure(view, donor);
                }
                return "visual:attached;bones=" + bones.Length +
                    ";vertices=" + mesh.vertexCount + ";albedo=" +
                    albedo.width + "x" + albedo.height + ";rendererEnabled=" +
                    (donor.enabled ? "true" : "false") +
                    ";accessorySuppressed=" +
                    (attachment.SuppressedRenderers == null ? 0 :
                        attachment.SuppressedRenderers.Length) + ";" + dressing +
                    ";" + controller + (spatial.Length == 0 ? string.Empty :
                        ";" + spatial);
            }
            catch (Exception error)
            {
                if (attachment.EagleLunge != null)
                {
                    UnityEngine.Object.Destroy(attachment.EagleLunge);
                    attachment.EagleLunge = null;
                }
                if (attachment.WaspSting != null)
                {
                    UnityEngine.Object.Destroy(attachment.WaspSting);
                    attachment.WaspSting = null;
                }
                if (attachment.CrocodilianPose != null)
                {
                    attachment.CrocodilianPose.enabled = false;
                    UnityEngine.Object.Destroy(attachment.CrocodilianPose);
                    attachment.CrocodilianPose = null;
                }
                if (attachment.StirgeTouch != null)
                {
                    UnityEngine.Object.Destroy(attachment.StirgeTouch);
                    attachment.StirgeTouch = null;
                }
                if (attachment.BeetleGlow != null)
                {
                    // A rollback after the glow attached must not leave its
                    // carrier behind while the swap is reverted.
                    attachment.BeetleGlow.Release(true);
                    UnityEngine.Object.Destroy(attachment.BeetleGlow);
                    attachment.BeetleGlow = null;
                }
                // ReinitMaterials instantiates a controller-driven clone. On a
                // post-swap fault, capture that exact owned family before
                // Revert drops its references. Donor/cache materials survive.
                var rollbackMaterials = new HashSet<Material>();
                if (material != null) rollbackMaterials.Add(material);
                if (swapped && (attachment.VisualKey == "gorgon" || IsCrocodilian(attachment.VisualKey) ||
                    TheropodVisualPolicy.Handles(attachment.VisualKey) ||
                    GiantBodyVisualPolicy.Handles(attachment.VisualKey)))
                {
                    string ownedName = "KMG_" + attachment.VisualKey + "_Original";
                    foreach (Material candidate in donor.sharedMaterials)
                        if (candidate != null && candidate.name.StartsWith(ownedName,
                                StringComparison.Ordinal)) rollbackMaterials.Add(candidate);
                    IList<Material> driven = ControllerMaterials(
                        donor.GetComponentInParent<StandardMaterialController>());
                    if (driven != null)
                        foreach (Material candidate in driven)
                            if (candidate != null && candidate.name.StartsWith(ownedName,
                                    StringComparison.Ordinal)) rollbackMaterials.Add(candidate);
                }
                if (swapped) Revert(attachment);
                foreach (Material candidate in rollbackMaterials)
                    if (candidate != null) UnityEngine.Object.Destroy(candidate);
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                attachment.Material = null;
                attachment.Mesh = null;
                return "donor-visual:attach-failed:" + error.GetType().Name;
            }
        }

        private static bool TryConfigureTheropodSpatial(UnitEntityView view,
            string key, out string description, out string failure)
        {
            description = string.Empty;
            failure = string.Empty;
            float radius;
            if (view == null ||
                !TheropodVisualPolicy.TryBaseCorpulence(key, out radius))
            {
                failure = "invalid-owner-or-key";
                return false;
            }
            if (BaseCorpulence == null ||
                BaseCorpulence.FieldType != typeof(float))
            {
                failure = "missing-UnitEntityView.m_Corpulence";
                return false;
            }
            float original = (float)BaseCorpulence.GetValue(view);
            try
            {
                BaseCorpulence.SetValue(view, radius);
                // This is public on the pinned Kingmaker 2.1.7b contract.
                // Calling it directly makes reference drift a compile failure
                // instead of silently rejecting every live theropod view.
                view.SetupSelectionColliders(false);
                CapsuleCollider soft = view.GetComponentsInChildren<
                    CapsuleCollider>(true).SingleOrDefault(value =>
                        value.name == "[soft collider]");
                if (soft == null || soft.radius <= 0f ||
                    Math.Abs((float)BaseCorpulence.GetValue(view) - radius) >
                        .00001f)
                    throw new InvalidOperationException(
                        "theropod selection collider did not update");
                description = "baseCorpulence=" + radius.ToString("R",
                    CultureInfo.InvariantCulture) + ";liveCorpulence=" +
                    view.Corpulence.ToString("R", CultureInfo.InvariantCulture) +
                    ";softColliderRadius=" + soft.radius.ToString("R",
                        CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception error)
            {
                try
                {
                    BaseCorpulence.SetValue(view, original);
                    view.SetupSelectionColliders(false);
                }
                catch (Exception)
                {
                    // Preserve the first exact failing contract in the
                    // outcome; native view teardown must remain available.
                }
                failure = error.GetType().Name;
                return false;
            }
        }

        /// <summary>
        /// Puts the painting on the private material copy and takes the
        /// eagle's own maps off it. Returns what was done, for the evidence
        /// record: which slots the shader declares, which were cleared, and
        /// the tint the donor material carried, which is reset to white so the
        /// albedo renders as painted.
        /// </summary>
        /// <summary>
        /// Kingmaker's dynamic shader has two fog-of-war treatments. With
        /// <c>FOG_OF_WAR_DISSOLVE_ON</c> a fogged creature dissolves and keeps
        /// its painting; without it the same creature is drawn as a flat
        /// untextured silhouette. Which one a donor material carries is the
        /// donor's own business, and the Worg carries the flat one while the
        /// Dog and Wolf carry the dissolve. A cloned material inherits that,
        /// so an original mesh borrowing the Worg rig rendered as a solid blue
        /// shape the moment it stepped outside the party's vision, while the
        /// same code on the other two donors looked right. Evidence: guarded
        /// creature review `20261001T1812593825736Z`, where the Goblin Dog was
        /// flat blue in all four live party-camera frames and was the only one
        /// of the three whose material lacked the keyword.
        ///
        /// The clone is project-owned and instance-local, so it is given the
        /// dissolve treatment regardless of donor. The donor material is never
        /// touched, and <c>_Dissolve</c> already starts at 0, so this changes
        /// how the project's own mesh is shaded in fog and nothing else.
        /// </summary>
        private const string FogOfWarAffectedKeyword = "FOG_OF_WAR_AFFECTED";
        private const string FogOfWarDissolveKeyword = "FOG_OF_WAR_DISSOLVE_ON";

        internal static string DressMaterial(Material material, Texture2D albedo)
        {
            material.SetTexture(MainTexture, albedo);
            material.SetTextureScale(MainTexture, Vector2.one);
            material.SetTextureOffset(MainTexture, Vector2.zero);

            string donorKeywords = material.shaderKeywords == null ||
                material.shaderKeywords.Length == 0 ? "<none>" :
                string.Join("|", material.shaderKeywords);
            string fogTreatment = "donor";
            if (material.IsKeywordEnabled(FogOfWarAffectedKeyword) &&
                !material.IsKeywordEnabled(FogOfWarDissolveKeyword))
            {
                material.EnableKeyword(FogOfWarDissolveKeyword);
                fogTreatment = "dissolve-enabled";
            }

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
                ";donorTint=" + tint + ";donorEmission=" + emission +
                ";donorKeywords=" + donorKeywords + ";fogTreatment=" +
                fogTreatment;
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
            if (attachment.TheropodSuppression != null)
            {
                attachment.TheropodSuppression.Release();
                UnityEngine.Object.DestroyImmediate(
                    attachment.TheropodSuppression);
                attachment.TheropodSuppression = null;
            }
            if (attachment.AnkylosaurusImpact != null)
            {
                attachment.AnkylosaurusImpact.enabled=false;
                UnityEngine.Object.DestroyImmediate(attachment.AnkylosaurusImpact);
                attachment.AnkylosaurusImpact=null;
            }
            if (attachment.CrocodilianPose != null)
            {
                attachment.CrocodilianPose.enabled = false;
                attachment.CrocodilianPose.RestoreNative();
                UnityEngine.Object.DestroyImmediate(attachment.CrocodilianPose);
                attachment.CrocodilianPose = null;
            }
            if (attachment.SuppressedRenderers != null &&
                attachment.OriginalSuppressedRendererStates != null)
                for (int index = 0; index <
                    attachment.SuppressedRenderers.Length && index <
                    attachment.OriginalSuppressedRendererStates.Length; index++)
                    if (attachment.SuppressedRenderers[index] != null)
                        attachment.SuppressedRenderers[index].enabled =
                            attachment.OriginalSuppressedRendererStates[index];
            SkinnedMeshRenderer donor = attachment.Donor;
            if (donor == null) return;
            donor.sharedMesh = attachment.OriginalMesh;
            donor.bones = attachment.OriginalBones;
            donor.sharedMaterials = attachment.OriginalMaterials;
            StandardMaterialController controller = donor
                .GetComponentInParent<StandardMaterialController>();
            if (controller != null) ReinitMaterials(controller);
        }

        internal static UnityEngine.Object[] CaptureOriginalFlightOwnedResources(UnitEntityView view)
        {
            Attachment attachment;
            if (view == null || !Applied.TryGetValue(view, out attachment) ||
                (!OriginalFlightCleanupPolicy.Handles(attachment.VisualKey) && attachment.VisualKey != "ankylosaurus")) return new UnityEngine.Object[0];
            // The exact swapped renderer belongs to this attachment. Its native
            // controller instance is private too; original donor references are
            // excluded by reference, never by a KMG_ prefix or a global search.
            var installed = attachment.Donor != null && attachment.Mesh != null &&
                ReferenceEquals(attachment.Donor.sharedMesh, attachment.Mesh)
                    ? attachment.Donor.sharedMaterials : new Material[0];
            return OriginalFlightCleanupPolicy.PrivateMaterials(attachment.Material,
                installed, attachment.OriginalMaterials).Cast<UnityEngine.Object>()
                .Concat(new UnityEngine.Object[] { attachment.Mesh, attachment.AnkylosaurusImpact }).Where(o => o != null).ToArray();
        }

        /// <summary>
        /// Exact per-view resource receipt for the two armored-dinosaur bodies.
        /// Donor mesh/material references and immutable source caches are
        /// excluded by identity; only the private installed mesh and material
        /// family returned to <see cref="ReleasePhase2View"/> are reported.
        /// </summary>
        internal static UnityEngine.Object[] CaptureArmoredDinosaurOwnedResources(
            UnitEntityView view)
        {
            Attachment attachment;
            if (view == null || !Applied.TryGetValue(view, out attachment) ||
                !ArmoredDinosaurVisualPolicy.Handles(attachment.VisualKey))
                return new UnityEngine.Object[0];
            Material[] installed = attachment.Donor == null
                ? new Material[0] : attachment.Donor.sharedMaterials;
            IList<Material> driven = attachment.Donor == null ? null :
                ControllerMaterials(attachment.Donor.GetComponentInParent<
                    StandardMaterialController>());
            IEnumerable<Material> privateMaterials =
                OriginalFlightCleanupPolicy.PrivateMaterials(
                    attachment.Material,
                    installed.Concat(driven ?? new Material[0]),
                    attachment.OriginalMaterials);
            return privateMaterials.Cast<UnityEngine.Object>()
                .Concat(new UnityEngine.Object[] { attachment.Mesh })
                .Where(value => value != null).Distinct().ToArray();
        }

        /// <summary>Exact per-view owned resources for the two Sprint 25
        /// bodies. Both bind to one donor renderer, so the receipt is the
        /// armoured-dinosaur shape: this view's private material instances
        /// and this view's mesh instance, never a donor asset.</summary>
        internal static UnityEngine.Object[] CaptureGorgonOwnedResources(UnitEntityView view)
        {
            Attachment attachment;
            if(view==null || !Applied.TryGetValue(view,out attachment) || attachment.VisualKey!="gorgon")
                return new UnityEngine.Object[0];
            Material[] installed=attachment.Donor==null?new Material[0]:attachment.Donor.sharedMaterials;
            IList<Material> driven=attachment.Donor==null?null:ControllerMaterials(attachment.Donor.GetComponentInParent<StandardMaterialController>());
            return installed.Concat(driven??new Material[0]).Concat(new[]{attachment.Material})
                .Where(v=>v!=null && v.name.StartsWith("KMG_gorgon_Original",StringComparison.Ordinal))
                .Cast<UnityEngine.Object>().Concat(new UnityEngine.Object[]{attachment.Mesh})
                .Where(v=>v!=null).Distinct().ToArray();
        }

        internal static UnityEngine.Object[] CaptureGiantBodyOwnedResources(
            UnitEntityView view)
        {
            Attachment attachment;
            if (view == null || !Applied.TryGetValue(view, out attachment) ||
                !GiantBodyVisualPolicy.Handles(attachment.VisualKey))
                return new UnityEngine.Object[0];
            Material[] installed = attachment.Donor == null
                ? new Material[0] : attachment.Donor.sharedMaterials;
            IList<Material> driven = attachment.Donor == null ? null :
                ControllerMaterials(attachment.Donor.GetComponentInParent<
                    StandardMaterialController>());
            string ownedName = "KMG_" + attachment.VisualKey + "_Original";
            IEnumerable<Material> privateMaterials = installed.Concat(driven ?? new Material[0])
                .Concat(new[] { attachment.Material })
                .Where(value => value != null && value.name.StartsWith(ownedName, StringComparison.Ordinal))
                .Distinct();
            return privateMaterials.Cast<UnityEngine.Object>()
                .Concat(new UnityEngine.Object[] { attachment.Mesh })
                .Where(value => value != null).Distinct().ToArray();
        }

        /// <summary>Exact per-view owned resources for the two theropod bodies.
        /// The disabled accessory and both donor renderers remain native and
        /// therefore never appear in this receipt.</summary>
        internal static UnityEngine.Object[] CaptureTheropodOwnedResources(
            UnitEntityView view)
        {
            Attachment attachment;
            if (view == null || !Applied.TryGetValue(view, out attachment) ||
                !TheropodVisualPolicy.Handles(attachment.VisualKey))
                return new UnityEngine.Object[0];
            Material[] installed = attachment.Donor == null
                ? new Material[0] : attachment.Donor.sharedMaterials;
            IList<Material> driven = attachment.Donor == null ? null :
                ControllerMaterials(attachment.Donor.GetComponentInParent<
                    StandardMaterialController>());
            // ReinitMaterials also creates native per-view instances for the
            // restored donor materials. They are not theropod-owned and the
            // release path intentionally does not destroy them. Match that
            // path's exact KMG material family instead of treating every
            // instance unequal to the prefab material as project-owned.
            string ownedMaterialName = "KMG_" + attachment.VisualKey +
                "_Original";
            IEnumerable<Material> privateMaterials = installed
                .Concat(driven ?? new Material[0])
                .Concat(new[] { attachment.Material })
                .Where(value => value != null && value.name.StartsWith(
                    ownedMaterialName, StringComparison.Ordinal))
                .Distinct();
            return privateMaterials.Cast<UnityEngine.Object>()
                .Concat(new UnityEngine.Object[] { attachment.Mesh,
                    attachment.TheropodSuppression })
                .Where(value => value != null).Distinct().ToArray();
        }

        internal static void ReleaseOriginalFlightView(UnitEntityView view)
        {
            Attachment attachment;
            if (view == null || !Applied.TryGetValue(view, out attachment) ||
                (!OriginalFlightCleanupPolicy.Handles(attachment.VisualKey) && attachment.VisualKey != "ankylosaurus")) return;
            var owned = CaptureOriginalFlightOwnedResources(view);
            try
            {
                if (attachment.EagleLunge != null)
                {
                    attachment.EagleLunge.enabled = false; // native pose restored by OnDisable
                    UnityEngine.Object.DestroyImmediate(attachment.EagleLunge);
                    attachment.EagleLunge = null;
                }
                Revert(attachment);
            }
            finally
            {
                foreach (var value in owned) if (value != null) UnityEngine.Object.DestroyImmediate(value);
                attachment.Material = null; attachment.Mesh = null;
                Applied.Remove(view);
            }
        }

        /// <summary>Release Phase 2 creatures' per-view clones on death.
        /// The cached source mesh/painting and the native donor stay owned by
        /// their existing systems; no accepted Phase 1 view is changed here.</summary>
        internal static void ReleasePhase2View(UnitEntityView view)
        {
            Attachment attachment;
            if (view == null || !Applied.TryGetValue(view, out attachment) ||
                (attachment.VisualKey != "giant-wasp" &&
                 attachment.VisualKey != "stirge" &&
                 !UngulateKeys.Contains(attachment.VisualKey) &&
                 !Sprint12QuadrupedKeys.Contains(attachment.VisualKey) &&
                 !Sprint13CreatureKeys.Contains(attachment.VisualKey) &&
                 !Sprint14InsectKeys.Contains(
                    attachment.VisualKey) &&
                 !IsCrocodilian(attachment.VisualKey) &&
                 !ArmoredDinosaurVisualPolicy.Handles(
                    attachment.VisualKey) &&
                 !TheropodVisualPolicy.Handles(
                    attachment.VisualKey) &&
                 !GiantBodyVisualPolicy.Handles(attachment.VisualKey))) return;
            string visualName = attachment.VisualKey == "stirge"
                ? StirgeVisualName : attachment.VisualKey == "giant-wasp"
                    ? GiantWaspVisualName
                    : "KMG_" + attachment.VisualKey + "_Original";
            if (attachment.WaspSting != null)
                attachment.WaspSting.enabled = false;
            if (attachment.StirgeTouch != null)
            {
                attachment.StirgeTouch.enabled = false;
                UnityEngine.Object.DestroyImmediate(attachment.StirgeTouch);
                attachment.StirgeTouch = null;
            }
            if (attachment.BeetleGlow != null)
            {
                // The component's own release destroys the carrier and the
                // light in this frame; destroying the component alone would
                // have queued the carrier for the end of it, which is the
                // artefact a crowd of expiring beetles would show.
                attachment.BeetleGlow.Release(true);
                attachment.BeetleGlow.enabled = false;
                UnityEngine.Object.DestroyImmediate(attachment.BeetleGlow);
                attachment.BeetleGlow = null;
            }
            var materials = new HashSet<Material>();
            if (attachment.Material != null)
                materials.Add(attachment.Material);
            if (attachment.Donor != null)
            {
                foreach (Material material in attachment.Donor.sharedMaterials)
                    if (material != null && material.name.StartsWith(
                        visualName, StringComparison.Ordinal))
                        materials.Add(material);
                StandardMaterialController controller = attachment.Donor
                    .GetComponentInParent<StandardMaterialController>();
                IList<Material> driven = ControllerMaterials(controller);
                if (driven != null)
                    foreach (Material material in driven)
                        if (material != null && material.name.StartsWith(
                            visualName, StringComparison.Ordinal))
                            materials.Add(material);
                Revert(attachment);
            }
            foreach (Material material in materials)
                if (material != null) UnityEngine.Object.DestroyImmediate(material);
            if (attachment.Mesh != null)
                UnityEngine.Object.DestroyImmediate(attachment.Mesh);
            attachment.Material = null;
            attachment.Mesh = null;
            Applied.Remove(view);
        }

        private const string DissolveProperty = "_Dissolve";

        /// <summary>
        /// The dissolve amount the clone was taken with, then the clone reset
        /// to intact and the view's material controller re-reading its
        /// renderers, so the game's own fades and tints include it.
        /// </summary>
        internal static string AdoptByMaterialController(UnitEntityView view,
            SkinnedMeshRenderer donor, Material material)
        {
            string dissolve = "<none>";
            if (material.HasProperty(DissolveProperty))
            {
                dissolve = material.GetFloat(DissolveProperty).ToString("0.###",
                    CultureInfo.InvariantCulture);
                material.SetFloat(DissolveProperty, 0f);
            }
            StandardMaterialController controller =
                view.GetComponentInChildren<StandardMaterialController>(true);
            if (controller == null)
                return "clonedDissolve=" + dissolve + ";materialController=absent";
            bool reinitialized = ReinitMaterials(controller);
            // The controller instantiates what it drives, so the renderer's
            // material after the reinit is an instance of the clone; that is
            // the one the game's fades reach, and the one to test for.
            IList<Material> materials = ControllerMaterials(controller);
            Material driven = donor.sharedMaterial;
            int count = materials == null ? -1 : materials.Count;
            bool adopted = materials != null && driven != null &&
                materials.Contains(driven) && driven.name.StartsWith(
                    material.name, StringComparison.Ordinal);
            return "clonedDissolve=" + dissolve + ";materialController=" +
                (reinitialized ? "reinitialized" : "reinit-unavailable") +
                ";controllerMaterials=" + count + ";adopted=" +
                (adopted ? "true" : "false") + ";driven=" +
                (driven == null ? "<none>" : driven.name);
        }

        /// <summary>
        /// Shared with every visual variant (Sprint 5 onward): the view's
        /// material controller re-reads its renderers, so the clones a patch
        /// has just put on them are what the game's fades and tints drive
        /// from now on. Returns what happened, for the outcome record.
        /// </summary>
        internal static string ReinitializeMaterialController(UnitEntityView view)
        {
            StandardMaterialController controller = view == null ? null :
                view.GetComponentInChildren<StandardMaterialController>(true);
            if (controller == null) return "absent";
            return ReinitMaterials(controller) ? "reinitialized" : "reinit-unavailable";
        }

        /// <summary>The materials the controller currently drives (its private list).</summary>
        internal static IList<Material> ControllerMaterials(
            StandardMaterialController controller)
        {
            if (controller == null) return null;
            FieldInfo field = typeof(StandardMaterialController).GetField(
                "m_Materials", BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(controller) as IList<Material>;
        }

        private static bool ReinitMaterials(StandardMaterialController controller)
        {
            MethodInfo method = typeof(StandardMaterialController).GetMethod(
                "ReinitMaterials", BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method == null) return false;
            method.Invoke(controller, null);
            return true;
        }
    }

    /// <summary>
    /// Native fade/visibility controllers revisit every renderer after the
    /// attachment has completed. The theropod donor's ammunition renderer is
    /// not part of the authored body, so this per-view owner suppresses it
    /// after those updates without touching the shared prefab. Revert restores
    /// the exact state captured before attachment.
    /// </summary>
    internal sealed class TheropodAccessorySuppression : MonoBehaviour
    {
        private SkinnedMeshRenderer _renderer;

        internal void Configure(SkinnedMeshRenderer renderer)
        {
            _renderer = renderer;
            Suppress();
        }

        private void LateUpdate()
        {
            Suppress();
        }

        private void Suppress()
        {
            if (_renderer != null && _renderer.enabled)
                _renderer.enabled = false;
        }

        internal void Release()
        {
            enabled = false;
            _renderer = null;
        }
    }


    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class ExpandedSummoningWaspVisualTeardownPatch
    {
        private static void Prefix(UnitEntityView __instance)
        {
            try
            {
                ExpandedSummoningPteranodonViewPatch.ReleaseOriginalFlightView(__instance);
                ExpandedSummoningPteranodonViewPatch.ReleasePhase2View(__instance);
            }
            catch (Exception)
            {
                // Resource release must never interrupt native view teardown.
            }
        }
    }
}
