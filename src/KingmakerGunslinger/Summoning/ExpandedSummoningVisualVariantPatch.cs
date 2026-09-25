using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.MaterialEffects.RimLighting;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// A bounded visual variant for a KMG summon that shares a native rig:
    /// the view's renderer materials are cloned and either tinted, with an
    /// optional rim light colour (the lion on the leopard rig; the six
    /// mephit variants, whose translucent bodies are seen through the
    /// shader's HDR rim glow - rounds 8-11 showed the game's material
    /// controller rewriting the mephit rig's tint slot and a project main
    /// texture changing nothing on it), or given a procedural coat on the
    /// main texture slot (the tiger and the cheetah). The colours come from
    /// the plain profiles the domain tests compile; only this file knows
    /// UnityEngine.
    /// </summary>
    internal sealed class SummonVisualVariant
    {
        internal SummonVisualVariant(string blueprintName,
            SummonVisualTintProfile tint)
        {
            if (string.IsNullOrEmpty(blueprintName))
                throw new ArgumentException("A blueprint name is required.",
                    "blueprintName");
            if (tint == null) throw new ArgumentNullException("tint");
            BlueprintName = blueprintName;
            Key = tint.Key;
            Tint = new Color(tint.TintRed, tint.TintGreen, tint.TintBlue, 1f);
            Rim = tint.HasRim ? new Color(tint.RimRed, tint.RimGreen, tint.RimBlue, 1f)
                : (Color?)null;
        }

        /// <summary>
        /// Sprint 8: a procedural coat. The rig's mesh is rasterized into a
        /// private texture with the profile's colours; the tint stays white.
        /// </summary>
        internal SummonVisualVariant(string blueprintName, SummonCoatProfile coat)
        {
            if (string.IsNullOrEmpty(blueprintName))
                throw new ArgumentException("A blueprint name is required.",
                    "blueprintName");
            if (coat == null) throw new ArgumentNullException("coat");
            BlueprintName = blueprintName;
            Key = coat.Key;
            Tint = Color.white;
            Rim = null;
            Coat = coat;
        }

        internal string BlueprintName { get; private set; }
        internal string Key { get; private set; }
        internal Color Tint { get; private set; }
        /// <summary>The shader's rim light colour (HDR), or none.</summary>
        internal Color? Rim { get; private set; }
        internal SummonCoatProfile Coat { get; private set; }
    }

    /// <summary>
    /// Sprint 8: rasterizes a rig's own mesh into a coat texture. Each
    /// triangle is drawn in the mesh's texture space with a colour derived
    /// from where its vertices sit on the body: stripes run across the body's
    /// long axis, spots tile its surface, and the underside pales toward the
    /// belly colour. The game's own texture is never read; only geometry is.
    /// </summary>
    internal static class SummonCoatRasterizer
    {
        internal static Texture2D Rasterize(Mesh mesh, SummonCoatProfile coat, int size,
            out string outcome)
        {
            Vector3[] vertices = mesh == null ? null : mesh.vertices;
            Vector2[] uvs = mesh == null ? null : mesh.uv;
            int[] triangles = mesh == null ? null : mesh.triangles;
            if (vertices == null || uvs == null || triangles == null ||
                vertices.Length == 0 || uvs.Length != vertices.Length ||
                triangles.Length < 3)
            {
                // A mesh the CPU cannot read (no Read/Write flag - the mephit
                // rigs) gives empty arrays; the pattern is then drawn over the
                // whole texture instead of along the body.
                return RasterizeTextureSpace(coat, size, out outcome);
            }
            Bounds bounds = mesh.bounds;
            Vector3 extent = bounds.size;
            // The body's long axis is its largest extent; the vertical axis
            // is the one closest to the mesh's up.
            int longAxis = extent.x >= extent.y && extent.x >= extent.z ? 0 :
                extent.y >= extent.z ? 1 : 2;
            int upAxis = longAxis == 1 ? (extent.x >= extent.z ? 0 : 2) : 1;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = ExpandedSummoningVisualVariantPatch.VariantMaterialName + "_" +
                coat.Key;
            var pixels = new Color32[size * size];
            Color32 baseColor = ToColor32(coat.BaseRed, coat.BaseGreen, coat.BaseBlue);
            for (int index = 0; index < pixels.Length; index++) pixels[index] = baseColor;
            int drawn = 0;
            for (int index = 0; index + 2 < triangles.Length; index += 3)
            {
                int a = triangles[index], b = triangles[index + 1], c = triangles[index + 2];
                if (a >= vertices.Length || b >= vertices.Length || c >= vertices.Length)
                    continue;
                Color32 colorA = Shade(vertices[a], bounds, longAxis, upAxis, coat);
                Color32 colorB = Shade(vertices[b], bounds, longAxis, upAxis, coat);
                Color32 colorC = Shade(vertices[c], bounds, longAxis, upAxis, coat);
                DrawTriangle(pixels, size, uvs[a], uvs[b], uvs[c], colorA, colorB, colorC);
                drawn++;
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            outcome = "coat=" + coat.Pattern + ";mode=mesh;triangles=" + drawn + ";size=" + size;
            return texture;
        }

        /// <summary>
        /// The coat drawn over the whole texture when the rig's mesh cannot
        /// be read: bands or cellular spots at the profile's frequency across
        /// the texture, over a low-frequency mottle between the base and the
        /// belly colour, so it reads as a surface pattern wherever the atlas
        /// puts it.
        /// </summary>
        private static Texture2D RasterizeTextureSpace(SummonCoatProfile coat, int size,
            out string outcome)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = ExpandedSummoningVisualVariantPatch.VariantMaterialName + "_" +
                coat.Key;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                float v = (y + 0.5f) / size;
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size;
                    float marking;
                    if (coat.Pattern == SummonCoatPattern.Stripes)
                    {
                        float phase = u * coat.Frequency * Mathf.PI * 2f +
                            Mathf.Sin(v * 11f) * 0.9f + Mathf.Sin(v * 29f + u * 7f) * 0.35f;
                        marking = Mathf.Clamp01((Mathf.Sin(phase) - 0.35f) * 3f);
                    }
                    else
                    {
                        float cu = u * coat.Frequency, cv = v * coat.Frequency;
                        float cellU = cu - Mathf.Floor(cu) - 0.5f, cellV = cv - Mathf.Floor(cv) - 0.5f;
                        float jitter = Mathf.Sin(Mathf.Floor(cu) * 12.9898f +
                            Mathf.Floor(cv) * 78.233f) * 0.25f;
                        float distance = Mathf.Sqrt((cellU + jitter) * (cellU + jitter) +
                            (cellV - jitter) * (cellV - jitter));
                        marking = Mathf.Clamp01((0.28f - distance) * 8f);
                    }
                    float mottle = 0.5f + 0.5f * Mathf.Sin(u * 9.7f + 1.3f) * Mathf.Sin(v * 7.3f);
                    float red = Mathf.Lerp(Mathf.Lerp(coat.BaseRed, coat.BellyRed, mottle * 0.6f),
                        coat.MarkRed, marking);
                    float green = Mathf.Lerp(Mathf.Lerp(coat.BaseGreen, coat.BellyGreen, mottle * 0.6f),
                        coat.MarkGreen, marking);
                    float blue = Mathf.Lerp(Mathf.Lerp(coat.BaseBlue, coat.BellyBlue, mottle * 0.6f),
                        coat.MarkBlue, marking);
                    pixels[y * size + x] = ToColor32(red, green, blue);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            outcome = "coat=" + coat.Pattern + ";mode=texture-space;size=" + size;
            return texture;
        }

        private static Color32 ToColor32(float red, float green, float blue)
        {
            return new Color32((byte)Mathf.RoundToInt(Mathf.Clamp01(red) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(green) * 255f),
                (byte)Mathf.RoundToInt(Mathf.Clamp01(blue) * 255f), 255);
        }

        private static float Axis(Vector3 value, int axis)
        { return axis == 0 ? value.x : axis == 1 ? value.y : value.z; }

        private static Color32 Shade(Vector3 vertex, Bounds bounds, int longAxis, int upAxis,
            SummonCoatProfile coat)
        {
            float along = Mathf.InverseLerp(Axis(bounds.min, longAxis),
                Axis(bounds.max, longAxis), Axis(vertex, longAxis));
            float up = Mathf.InverseLerp(Axis(bounds.min, upAxis),
                Axis(bounds.max, upAxis), Axis(vertex, upAxis));
            int sideAxis = 3 - longAxis - upAxis;
            float side = Mathf.InverseLerp(Axis(bounds.min, sideAxis),
                Axis(bounds.max, sideAxis), Axis(vertex, sideAxis));
            float marking;
            if (coat.Pattern == SummonCoatPattern.Stripes)
            {
                // Vertical stripes across the long axis, wavering with height.
                float phase = along * coat.Frequency * Mathf.PI * 2f +
                    Mathf.Sin(up * 9f) * 0.8f + Mathf.Sin(side * 5f) * 0.4f;
                marking = Mathf.Clamp01((Mathf.Sin(phase) - 0.35f) * 3f);
            }
            else
            {
                // Spots: a cellular pattern over the surface.
                float u = along * coat.Frequency, v = (up + side) * coat.Frequency * 0.5f;
                float cellU = u - Mathf.Floor(u) - 0.5f, cellV = v - Mathf.Floor(v) - 0.5f;
                float jitter = Mathf.Sin(Mathf.Floor(u) * 12.9898f + Mathf.Floor(v) * 78.233f)
                    * 0.25f;
                float distance = Mathf.Sqrt((cellU + jitter) * (cellU + jitter) +
                    (cellV - jitter) * (cellV - jitter));
                marking = Mathf.Clamp01((0.28f - distance) * 8f);
            }
            // Belly: the lower third pales; markings fade there too.
            float belly = Mathf.Clamp01((0.34f - up) * 4f);
            marking *= 1f - belly;
            float red = Mathf.Lerp(Mathf.Lerp(coat.BaseRed, coat.BellyRed, belly),
                coat.MarkRed, marking);
            float green = Mathf.Lerp(Mathf.Lerp(coat.BaseGreen, coat.BellyGreen, belly),
                coat.MarkGreen, marking);
            float blue = Mathf.Lerp(Mathf.Lerp(coat.BaseBlue, coat.BellyBlue, belly),
                coat.MarkBlue, marking);
            return ToColor32(red, green, blue);
        }

        private static void DrawTriangle(Color32[] pixels, int size, Vector2 uvA,
            Vector2 uvB, Vector2 uvC, Color32 colorA, Color32 colorB, Color32 colorC)
        {
            Vector2 a = Wrap(uvA) * (size - 1), b = Wrap(uvB) * (size - 1),
                c = Wrap(uvC) * (size - 1);
            int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))) - 1);
            int maxX = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))) + 1);
            int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))) - 1);
            int maxY = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))) + 1);
            if (maxX - minX > size / 2 || maxY - minY > size / 2) return; // a wrapped seam
            float area = (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
            if (Mathf.Abs(area) < 0.0001f) return;
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float wA = ((b.x - px) * (c.y - py) - (c.x - px) * (b.y - py)) / area;
                    float wB = ((c.x - px) * (a.y - py) - (a.x - px) * (c.y - py)) / area;
                    float wC = 1f - wA - wB;
                    const float slack = -0.02f;
                    if (wA < slack || wB < slack || wC < slack) continue;
                    pixels[y * size + x] = new Color32(
                        (byte)Mathf.Clamp(colorA.r * wA + colorB.r * wB + colorC.r * wC, 0, 255),
                        (byte)Mathf.Clamp(colorA.g * wA + colorB.g * wB + colorC.g * wC, 0, 255),
                        (byte)Mathf.Clamp(colorA.b * wA + colorB.b * wB + colorC.b * wC, 0, 255),
                        255);
                }
        }

        private static Vector2 Wrap(Vector2 uv)
        { return new Vector2(uv.x - Mathf.Floor(uv.x), uv.y - Mathf.Floor(uv.y)); }
    }

    /// <summary>
    /// The rim light on the mephit rigs is a looping rim animation that
    /// reaches the view's material controller after the view attaches (with
    /// the unit's spawned effects), so an attach-time pass cannot see it
    /// (round 13). This prefix on the controller's Update looks up, once
    /// per controller, whether its view is a registered variant with a rim
    /// colour, and thereafter recolours every new looping animation in the
    /// controller's rim list before the controller evaluates it. Transient
    /// animations (hit flashes) keep the game's colour. Nothing shared is
    /// touched: the settings objects belong to this view's effects.
    /// </summary>
    [HarmonyPatch(typeof(StandardMaterialController), "Update")]
    internal static class ExpandedSummoningRimAnimationPatch
    {
        private sealed class State
        {
            internal Color? Rim;
            internal bool Resolved;
            internal int Recoloured;
            internal readonly HashSet<RimLightingAnimationSettings> Seen =
                new HashSet<RimLightingAnimationSettings>();
        }

        private static readonly ConditionalWeakTable<StandardMaterialController, State> States =
            new ConditionalWeakTable<StandardMaterialController, State>();
        private static readonly FieldInfo RimControllerField = typeof(StandardMaterialController)
            .GetField("m_RimController", BindingFlags.Instance | BindingFlags.NonPublic |
                BindingFlags.Public);

        /// <summary>The controller's rim animation list owner (a private field of the game's controller).</summary>
        internal static RimLightingAnimationController RimControllerOf(
            StandardMaterialController controller)
        {
            return controller == null || RimControllerField == null ? null :
                RimControllerField.GetValue(controller) as RimLightingAnimationController;
        }

        /// <summary>"rim=R/G/B;recoloured=N" for a view's controller, or "&lt;none&gt;".</summary>
        internal static string Describe(UnitEntityView view)
        {
            StandardMaterialController controller = view == null ? null :
                view.GetComponentInChildren<StandardMaterialController>(true);
            State state;
            if (controller == null || !States.TryGetValue(controller, out state)) return "<none>";
            return "resolved=" + state.Resolved + ";rim=" + (state.Rim.HasValue ?
                state.Rim.Value.r.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "/" +
                state.Rim.Value.g.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "/" +
                state.Rim.Value.b.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) : "<none>") +
                ";recoloured=" + state.Recoloured;
        }

        private static void Prefix(StandardMaterialController __instance)
        {
            try
            {
                if (__instance == null) return;
                State state;
                if (!States.TryGetValue(__instance, out state))
                {
                    state = new State();
                    States.Add(__instance, state);
                }
                if (!state.Resolved)
                {
                    UnitEntityView view = __instance.GetComponentInParent<UnitEntityView>();
                    if (view == null || view.EntityData == null ||
                        view.EntityData.Blueprint == null) return;
                    state.Rim = ExpandedSummoningVisualVariantPatch.RimFor(view);
                    state.Resolved = true;
                }
                if (!state.Rim.HasValue) return;
                RimLightingAnimationController rims = RimControllerOf(__instance);
                if (rims == null || rims.Animations == null) return;
                bool arrived = false;
                foreach (RimLightingAnimationSettings settings in rims.Animations)
                {
                    if (settings == null || !settings.LoopAnimation || state.Seen.Contains(settings))
                        continue;
                    state.Seen.Add(settings);
                    state.Recoloured++;
                    arrived = true;
                }
                if (!arrived) return;
                // The controller adds its looping animations together: each
                // recoloured one carries an equal share of the target, so the
                // combined glow lands on the profile's brightness.
                foreach (RimLightingAnimationSettings settings in state.Seen)
                    ExpandedSummoningVisualVariantPatch.RecolourRimAnimation(settings,
                        state.Rim.Value, state.Seen.Count);
            }
            catch (Exception)
            {
                // A visual variant never interrupts the game's own material update.
            }
        }
    }

    /// <summary>
    /// Applies a registered visual variant when a KMG summon's view attaches
    /// (Sprint 5). Everything is validated before a renderer is touched: no
    /// registered variant, no renderer, or a material without a colour slot
    /// leaves the native look untouched and records why. The cloned material
    /// is private to the view, so the donor's shared material - and every
    /// native creature using it - is never changed. Outcomes are kept for the
    /// runtime fixture and the creature review.
    /// </summary>
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class ExpandedSummoningVisualVariantPatch
    {
        internal const string VariantMaterialName = "KMG_SummonVisualVariant";
        private static readonly string[] ColorSlots = { "_Color", "_TintColor",
            "_BaseColor", "_MainColor" };
        private const string RimSlot = "_RimColor";
        private const string MainTextureSlot = "_MainTex";
        private const string DissolveSlot = "_Dissolve";
        internal const int CoatTextureSize = 512;

        private static readonly Dictionary<string, SummonVisualVariant> Variants =
            new Dictionary<string, SummonVisualVariant>(StringComparer.Ordinal);
        private static readonly ConditionalWeakTable<UnitEntityView, string>
            Applied = new ConditionalWeakTable<UnitEntityView, string>();
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        internal static void Register(SummonVisualVariant variant)
        {
            if (variant == null) throw new ArgumentNullException("variant");
            lock (Sync) { Variants[variant.BlueprintName] = variant; }
        }

        internal static IReadOnlyList<string> RegisteredBlueprintNames
        { get { lock (Sync) { return Variants.Keys.OrderBy(value => value,
            StringComparer.Ordinal).ToArray(); } } }

        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) { return Outcomes.ToArray(); } } }

        /// <summary>
        /// "variant:applied;materials=2;slot=_Color;rim=0" and the like,
        /// or the reason nothing was applied; "&lt;none&gt;" for a view this
        /// patch never saw.
        /// </summary>
        internal static string DescribeView(UnitEntityView view)
        {
            string outcome;
            if (view == null || !Applied.TryGetValue(view, out outcome)) return "<none>";
            return outcome;
        }

        private static void Postfix(UnitEntityView __instance)
        {
            try
            {
                if (__instance == null || __instance.EntityData == null ||
                    __instance.EntityData.Blueprint == null) return;
                SummonVisualVariant variant;
                lock (Sync)
                {
                    if (!Variants.TryGetValue(__instance.EntityData.Blueprint.name,
                            out variant)) return;
                }
                string existing;
                if (Applied.TryGetValue(__instance, out existing)) return;
                string outcome;
                try { outcome = Apply(__instance, variant); }
                catch (Exception exception)
                { outcome = "variant:exception:" + exception.GetType().Name; }
                Applied.Add(__instance, outcome);
                lock (Sync)
                {
                    if (Outcomes.Count >= 256) Outcomes.RemoveAt(0);
                    Outcomes.Add(__instance.EntityData.Blueprint.name + "=" + outcome);
                }
            }
            catch (Exception)
            {
                // A visual variant never interrupts the game's own view attach.
            }
        }

        /// <summary>
        /// The rim light on the mephit rigs is a looping rim animation: a
        /// RimLightingAnimationSetup on the view registers its own settings
        /// object with the material controller, which rewrites _RimColor
        /// every frame from the settings' colour gradient and intensity
        /// curve (times IntensityScale). The variant's rim colour goes into
        /// that per-view object: the gradient's colour keys become the
        /// variant colour (alpha keys and times kept) and IntensityScale is
        /// set so the intensity curve's peak lands on the variant's
        /// brightness - the pulse stays, in the variant's colour. Nothing
        /// shared is touched: the settings object belongs to this view's
        /// component instance.
        /// </summary>
        private static int RecolourRimAnimations(UnitEntityView view, Color rim)
        {
            int recoloured = 0;
            foreach (RimLightingAnimationSetup setup in
                view.GetComponentsInChildren<RimLightingAnimationSetup>(true))
            {
                RimLightingAnimationSettings settings = setup == null ? null : setup.Settings;
                if (settings == null) continue;
                RecolourRimAnimation(settings, rim, 1);
                recoloured++;
            }
            return recoloured;
        }

        /// <summary>
        /// One rim animation in the variant's colour: the gradient's colour
        /// keys become the colour (normalized; alpha keys and times kept)
        /// and IntensityScale is set so the intensity curve's peak lands on
        /// the colour's brightest channel divided by <paramref name="share"/>
        /// - the controller adds its looping animations together, so each
        /// of N carries a 1/N share of the target.
        /// </summary>
        internal static void RecolourRimAnimation(RimLightingAnimationSettings settings,
            Color rim, int share)
        {
            float peakTarget = Mathf.Max(rim.r, Mathf.Max(rim.g, rim.b));
            if (settings == null || peakTarget <= 0f) return;
            if (share < 1) share = 1;
            var normalized = new Color(rim.r / peakTarget, rim.g / peakTarget,
                rim.b / peakTarget, 1f);
            Gradient source = settings.ColorOverLifetime;
            GradientColorKey[] colorKeys = source == null || source.colorKeys == null ||
                source.colorKeys.Length == 0
                ? new[] { new GradientColorKey(normalized, 0f), new GradientColorKey(normalized, 1f) }
                : source.colorKeys.Select(key => new GradientColorKey(normalized, key.time))
                    .ToArray();
            GradientAlphaKey[] alphaKeys = source == null || source.alphaKeys == null ||
                source.alphaKeys.Length == 0
                ? new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
                : source.alphaKeys;
            var gradient = new Gradient();
            gradient.SetKeys(colorKeys, alphaKeys);
            if (source != null) gradient.mode = source.mode;
            settings.ColorOverLifetime = gradient;
            float peakCurve = 1f;
            AnimationCurve intensity = settings.IntensityOverLifetime;
            if (intensity != null && intensity.keys != null && intensity.keys.Length != 0)
                peakCurve = intensity.keys.Max(key => key.value);
            if (peakCurve <= 0f) peakCurve = 1f;
            settings.IntensityScale = peakTarget / peakCurve / share;
            settings.CurrentColor = normalized;
        }

        /// <summary>The registered rim colour for a view, or none.</summary>
        internal static Color? RimFor(UnitEntityView view)
        {
            if (view == null || view.EntityData == null || view.EntityData.Blueprint == null)
                return null;
            SummonVisualVariant variant;
            lock (Sync)
            {
                if (!Variants.TryGetValue(view.EntityData.Blueprint.name, out variant))
                    return null;
            }
            return variant.Rim;
        }

        internal static string Apply(UnitEntityView view, SummonVisualVariant variant)
        {
            Renderer[] renderers = view.GetComponentsInChildren<Renderer>(true)
                .Where(value => value != null && value.sharedMaterials != null &&
                    value.sharedMaterials.Length != 0)
                .ToArray();
            if (renderers.Length == 0) return "variant:no-renderer";
            int tinted = 0, glowing = 0, coated = 0;
            string slotUsed = null;
            string coatOutcome = null;
            foreach (Renderer renderer in renderers)
            {
                Material[] originals = renderer.sharedMaterials;
                var replacements = new Material[originals.Length];
                bool changed = false;
                Texture2D coat = null;
                if (variant.Coat != null)
                {
                    // Only the rig's skinned body takes a coat; a particle or
                    // line renderer on the view is left as it is.
                    SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
                    if (skinned == null) { coatOutcome = coatOutcome ?? "no-skinned-mesh"; continue; }
                    coat = SummonCoatRasterizer.Rasterize(skinned.sharedMesh, variant.Coat,
                        CoatTextureSize, out coatOutcome);
                }
                for (int index = 0; index < originals.Length; index++)
                {
                    Material original = originals[index];
                    replacements[index] = original;
                    if (original == null || original.name == VariantMaterialName)
                        continue;
                    string slot = ColorSlots.FirstOrDefault(original.HasProperty);
                    if (slot == null) continue;
                    if (coat != null && !original.HasProperty(MainTextureSlot)) continue;
                    var material = new Material(original);
                    material.name = VariantMaterialName;
                    if (coat != null && material.HasProperty(MainTextureSlot))
                    {
                        // The coat replaces the albedo; the colour slot is
                        // reset so the rig's own tint does not stain it.
                        material.SetTexture(MainTextureSlot, coat);
                        material.SetTextureScale(MainTextureSlot, Vector2.one);
                        material.SetTextureOffset(MainTextureSlot, Vector2.zero);
                        material.SetColor(slot, Color.white);
                        coated++;
                    }
                    else
                        material.SetColor(slot, original.GetColor(slot) * variant.Tint);
                    if (variant.Rim.HasValue && material.HasProperty(RimSlot))
                    {
                        material.SetColor(RimSlot, variant.Rim.Value);
                        glowing++;
                    }
                    // The clone starts intact: at attach the summon is still
                    // materialising, and a clone taken mid-dissolve would stay
                    // invisible once the controller stops driving the
                    // material it was taken from.
                    if (material.HasProperty(DissolveSlot))
                        material.SetFloat(DissolveSlot, 0f);
                    replacements[index] = material;
                    slotUsed = slotUsed ?? slot;
                    tinted++;
                    changed = true;
                }
                if (changed) renderer.sharedMaterials = replacements;
            }
            if (tinted == 0) return "variant:no-colour-slot";
            if (variant.Coat != null && coated == 0)
                return "variant:coat-not-applied;reason=" + (coatOutcome ?? "no-main-texture");
            // The game's material controller cached the renderer's materials
            // before the swap and keeps driving those - and re-instantiates
            // what it drives, which is how the round-8 review found the native
            // look back on every tinted mephit. It re-reads the renderers
            // here, so what it drives from now on are instances of the
            // clones: the Pteranodon's treatment, shared.
            string controller = ExpandedSummoningPteranodonViewPatch
                .ReinitializeMaterialController(view);
            // The rim light on these rigs is driven every frame by the
            // controller's looping rim animation (round 12); the variant's
            // colour goes into that per-view animation too.
            int rimAnimations = variant.Rim.HasValue
                ? RecolourRimAnimations(view, variant.Rim.Value) : 0;
            Material driven = renderers[0].sharedMaterial;
            return "variant:applied;key=" + variant.Key + ";materials=" + tinted +
                ";slot=" + slotUsed + ";rim=" + glowing + ";rimAnimations=" + rimAnimations +
                (variant.Coat != null ? ";coat=" + coated + ";" + coatOutcome : "") +
                ";controller=" + controller + ";driven=" + (driven == null ? "<none>" :
                    driven.name.Replace(';', ',').Replace('|', '/'));
        }
    }
}
