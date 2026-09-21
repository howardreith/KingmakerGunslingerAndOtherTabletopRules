using System;
using System.Linq;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingmakerGunslinger.Spells.MagicCircle
{
    // Presentation owned by the native moving area view, never by the caster or
    // a saved registry. No Update loop, particles, shared material mutation, or
    // gameplay changes. A recreated native view gets exactly one fresh ring.
    internal sealed class MagicCircleRadiusVisual : MonoBehaviour
    {
        internal const int Segments = 96;
        internal const float GroundOffset = 0.055f;
        internal LineRenderer Boundary { get; private set; }
        internal float Radius { get; private set; }
        internal string Alignment { get; private set; }
        internal string AreaId { get; private set; }
        private Material _material;

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
            root.transform.SetParent(area.View.transform, false);
            var visual = root.AddComponent<MagicCircleRadiusVisual>();
            visual.AreaId = area.UniqueId;
            try { visual.Initialize(circle.Alignment, area.Blueprint.Size.Meters); }
            catch { UnityEngine.Object.Destroy(root); throw; }
        }

        private void Initialize(string alignment, float radius)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader == null || !shader.isSupported) throw new InvalidOperationException("Magic Circle boundary shader unavailable.");
            Alignment = alignment; Radius = radius;
            _material = new Material(shader) { name = "KMG_MagicCircle_Radius_Material_" + alignment };
            Boundary = gameObject.AddComponent<LineRenderer>();
            Boundary.sharedMaterial = _material;
            Boundary.useWorldSpace = false;
            Boundary.loop = true;
            Boundary.positionCount = Segments;
            Boundary.startWidth = Boundary.endWidth = 0.07f;
            Boundary.startColor = Boundary.endColor = ColorFor(alignment);
            Boundary.shadowCastingMode = ShadowCastingMode.Off;
            Boundary.receiveShadows = false;
            var points = new Vector3[Segments];
            for (int i = 0; i < points.Length; i++) {
                float angle = i * Mathf.PI * 2f / Segments;
                points[i] = new Vector3(Mathf.Cos(angle) * radius, GroundOffset, Mathf.Sin(angle) * radius);
            }
            Boundary.SetPositions(points);
        }

        internal static void Hide(AreaEffectEntityData area)
        {
            if (MagicCircleAreaLifetime.ForOwnedArea(area) == null) return;
            var visual = area?.View == null ? null : area.View.GetComponentInChildren<MagicCircleRadiusVisual>(true);
            if (visual?.Boundary != null) visual.Boundary.enabled = false;
        }

        private void OnDestroy()
        {
            if (_material != null) UnityEngine.Object.Destroy(_material);
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
