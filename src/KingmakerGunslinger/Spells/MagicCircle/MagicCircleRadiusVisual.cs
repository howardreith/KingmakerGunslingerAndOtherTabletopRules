using System;
using System.Linq;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker;
using Kingmaker.UI.AbilityTarget;
using Kingmaker.Visual.Decals;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Spells.MagicCircle
{
    // Use the game's depth-projected AoE boundary: collision meshes can be
    // ramps beneath visibly stepped meshes. Native decals follow the rendered
    // surface and its occlusion, without vertex raycasts or a custom update loop.
    internal sealed class MagicCircleRadiusVisual : ScreenSpaceDecal
    {
        // The extra vertical reach keeps the native middle-opacity gradient
        // readable across the measured staircase. This is an invisible volume,
        // not a dome; X/Z remain exactly the gameplay area's diameter.
        internal const float ProjectionHeight = 12f;
        internal ScreenSpaceDecal Boundary => this;
        internal float Radius { get; private set; }
        internal string Alignment { get; private set; }
        internal string AreaId { get; private set; }
        public override DecalType Type => DecalType.GUI;

        internal static Color ColorFor(string alignment)
        {
            // Native Protection identity: cyan-blue sigil, gold sunburst, red
            // fractured stone, and the geometric star's violet perimeter.
            switch (alignment) {
                case "Evil": return new Color(0.16f, 0.62f, 1f, 0.88f);
                case "Good": return new Color(1f, 0.78f, 0.18f, 0.88f);
                case "Chaos": return new Color(1f, 0.18f, 0.12f, 0.88f);
                case "Law": return new Color(0.77f, 0.49f, 1f, 0.88f);
                default: throw new ArgumentOutOfRangeException(nameof(alignment));
            }
        }

        internal static void Attach(AreaEffectEntityData area)
        {
            if (area == null || area.IsEnded || area.View == null || BlueprintBootstrap.MagicCircles == null) return;
            var circle = BlueprintBootstrap.MagicCircles.FirstOrDefault(value => ReferenceEquals(value.Area, area.Blueprint));
            if (circle == null) return;
            var existing = area.View.GetComponentInChildren<MagicCircleRadiusVisual>(true);
            if (existing != null) { existing.Boundary.enabled = true; return; }
            var root = new GameObject("KMG_MagicCircle_Radius_" + circle.Alignment);
            root.SetActive(false);
            root.transform.SetParent(area.View.transform, false);
            var visual = root.AddComponent<MagicCircleRadiusVisual>();
            visual.AreaId = area.UniqueId;
            try { visual.Initialize(circle.Alignment, area.Blueprint.Size.Meters); root.SetActive(true); }
            catch { UnityEngine.Object.Destroy(root); throw; }
        }

        private void Initialize(string alignment, float radius)
        {
            // This exact native reference was inspected and compared on the
            // authored slope/stairs. No global search or native asset mutation.
            var range = Game.Instance?.UI?.AbilityTargetSelection?.GetComponent<AbilityAoERange>();
            var template = range?.Range?.GetComponent<GUIDecal>();
            var source = template?.SharedMaterial;
            if (source == null || source.shader.name != "PF/Decals/GUIDecal" ||
                !source.shader.isSupported || source.mainTexture?.name != "Sector" || !source.IsKeywordEnabled("CIRCLE_ON"))
                throw new InvalidOperationException("Native circular AoE decal material unavailable.");
            Alignment = alignment; Radius = radius;
            m_Material = new Material(source) { name = "KMG_MagicCircle_Radius_Material_" + alignment };
            Layer = template.Layer;
            MaterialProperties.SetColor("_Color", ColorFor(alignment));
            SetValidateHeight(false); // Native depth projection; no collision-ramp height correction.
            transform.localScale = new Vector3(radius * 2, ProjectionHeight, radius * 2);
            // Native OnEnable/OnDisable own registration and depth/culling
            // state. Native Update refreshes bounds only after transform changes.
        }

        internal static void Hide(AreaEffectEntityData area)
        {
            if (MagicCircleAreaLifetime.ForOwnedArea(area) == null) return;
            var visual = area?.View == null ? null : area.View.GetComponentInChildren<MagicCircleRadiusVisual>(true);
            if (visual?.Boundary != null) visual.Boundary.enabled = false;
        }

        private void OnDestroy()
        {
            if (m_Material != null) UnityEngine.Object.Destroy(m_Material);
        }
    }

    [HarmonyPatch(typeof(AreaEffectEntityData), "OnViewAttached")]
    internal static class MagicCircleRadiusAttachPatch
    {
        private static bool _reported;
        private static void Postfix(AreaEffectEntityData __instance)
        {
            try { MagicCircleRadiusVisual.Attach(__instance); }
            catch (Exception exception) {
                if (_reported) return;
                _reported = true;
                ModContext context;
                if (ModContext.TryGet(out context)) context.Logger.Failure("magic-circle", "radius.failed",
                    "The circle's gameplay area remains active; its visual boundary could not be created.", exception);
            }
        }
    }

    [HarmonyPatch(typeof(AreaEffectEntityData), "ForceEnd")]
    internal static class MagicCircleRadiusEndPatch
    {
        private static void Postfix(AreaEffectEntityData __instance) { MagicCircleRadiusVisual.Hide(__instance); }
    }

    [HarmonyPatch(typeof(AreaEffectEntityData), "OnAreaUnload")]
    internal static class MagicCircleRadiusUnloadPatch
    {
        private static void Postfix(AreaEffectEntityData __instance) { MagicCircleRadiusVisual.Hide(__instance); }
    }
}
