using System;

namespace KingmakerGunslinger.Summoning
{
    internal struct GiantPalettePixel
    {
        internal GiantPalettePixel(float red, float green, float blue, float alpha)
        { Red = red; Green = green; Blue = blue; Alpha = alpha; }
        internal readonly float Red, Green, Blue, Alpha;
    }

    internal sealed class GiantPaletteSelector
    {
        internal GiantPaletteSelector(string key, string prefabId, string renderer,
            string material)
        { Key = key; PrefabId = prefabId; Renderer = renderer; Material = material; }
        internal string Key { get; private set; }
        internal string PrefabId { get; private set; }
        internal string Renderer { get; private set; }
        internal string Material { get; private set; }
    }

    /// <summary>
    /// Exact native body slot and texture, plus the deliberate whole-body
    /// palette treatment. Hair/skirt regions
    /// on that albedo receive the same transform; separate outfit/cape/armor
    /// renderers remain protected. No guessed mask or external config is used.
    /// </summary>
    internal sealed class GiantPaletteAlbedoProof
    {
        internal GiantPaletteAlbedoProof(string key, string prefabId, string rendererPath,
            int materialSlot, string textureName, int width, int height,
            string textureFormat, string identityEvidence, string bodyPaletteEvidence)
        {
            Key = key; PrefabId = prefabId; RendererPath = rendererPath;
            MaterialSlot = materialSlot; TextureName = textureName; Width = width;
            Height = height; TextureFormat = textureFormat;
            IdentityEvidence = identityEvidence; BodyPaletteEvidence = bodyPaletteEvidence;
        }
        internal string Key { get; private set; }
        internal string PrefabId { get; private set; }
        internal string RendererPath { get; private set; }
        internal int MaterialSlot { get; private set; }
        internal string TextureName { get; private set; }
        internal int Width { get; private set; }
        internal int Height { get; private set; }
        internal string TextureFormat { get; private set; }
        internal string IdentityEvidence { get; private set; }
        internal string BodyPaletteEvidence { get; private set; }
    }

    internal static class GiantSummonPalettePolicy
    {
        internal const string ShaderName = "PF/StandardDynamic";
        internal const string AlbedoSlot = "_MainTex";
        internal const string TintSlot = "_TintColor";
        private const string HillPrefab = "635d6dee1c7831b41aa987bdbeff14bc";
        private const string FrostPrefab = "df0f3fbceb3605d489c0dec8fa5c7d1e";

        // Native identities observed in the detached donor census and again in
        // the save-free Cloud/Storm survey. Every live instance must still pass
        // the exact prefab/path/slot/shader/texture checks before allocation.
        // This is body-atlas recoloring, not a claimed skin/hair UV mask.
        internal static GiantPaletteAlbedoProof ProductionProof(string key)
        {
            GiantPaletteSelector selector = Selector(key);
            if (selector == null) return null;
            bool hill = selector.PrefabId == HillPrefab;
            return new GiantPaletteAlbedoProof(key, selector.PrefabId,
                hill ? "Hill01_body_RIG[2]/hill_giant_1[1]" : "Frozen_body_RIG[3]/Frozen_body[0]",
                0, hill ? "hill_giant_01_d" : "frost_giant_d", 1024, 1024, "DXT1",
                "20261022T0021352089530Z-fca727ae39c6443b99a759a9b19d8656:cleanup2-pre-survey-actor-" +
                    (hill ? "cloud-giant" : "storm-giant"),
                "cleanup2-production-body-palette-v1:" + key +
                    ";all body atlas regions; separate outfit/cape/equipment unchanged");
        }

        // Census-proven body identities. Hill outfit and Frost cape are not
        // selectors, including the cape that shares the Frost body material.
        internal static GiantPaletteSelector Selector(string key)
        {
            switch (key)
            {
                case "stone-giant": case "cloud-giant":
                    return new GiantPaletteSelector(key, HillPrefab, "hill_giant_1",
                        "hill_giant_01_character_d");
                case "fire-giant": case "storm-giant":
                    return new GiantPaletteSelector(key, FrostPrefab, "Frozen_body",
                        "frost_giant_character_d");
                default: return null;
            }
        }

        internal static string ValidateProof(GiantPaletteAlbedoProof proof)
        {
            if (proof == null) return "missing-albedo-proof";
            if (Selector(proof.Key) == null)
                return "palette-treatment-not-reviewed";
            GiantPaletteSelector selector = Selector(proof.Key);
            if (selector == null || proof.PrefabId != selector.PrefabId)
                return "prefab-identity-mismatch";
            if (string.IsNullOrWhiteSpace(proof.RendererPath) ||
                proof.RendererPath.StartsWith("/", StringComparison.Ordinal) ||
                proof.RendererPath.Contains("..") || proof.MaterialSlot < 0 ||
                string.IsNullOrWhiteSpace(proof.TextureName) ||
                string.IsNullOrWhiteSpace(proof.TextureFormat) || proof.Width < 1 ||
                proof.Height < 1 || proof.Width > 4096 || proof.Height > 4096)
                return "invalid-slot-or-texture-identity";
            switch (proof.TextureFormat)
            {
                case "RGB24": case "RGBA32": case "ARGB32": case "BGRA32":
                case "DXT1": case "DXT5": break;
                default: return "unsupported-albedo-channel-format";
            }
            if (string.IsNullOrWhiteSpace(proof.IdentityEvidence)) return "missing-identity-evidence";
            if (string.IsNullOrWhiteSpace(proof.BodyPaletteEvidence))
                return "missing-whole-body-palette-evidence";
            return "proven-body-albedo";
        }

        internal static bool MaterialNameMatches(string actual, string expected)
        { return actual == expected || actual == expected + " (Instance)"; }

        /// <summary>
        /// Appearance scalars admitted only for an exact proven body albedo.
        /// Every region of that body albedo receives the same
        /// transform. Luminance retains shading; alpha is unchanged. Input/output
        /// are normalized encoded albedo channels, not lighting or a skin mask.
        /// </summary>
        internal static GiantPalettePixel Transform(string key, GiantPalettePixel source)
        {
            if (!Channel(source.Red) || !Channel(source.Green) || !Channel(source.Blue) ||
                !Channel(source.Alpha)) throw new ArgumentOutOfRangeException("source");
            float luminance = 0.2126f * source.Red + 0.7152f * source.Green + 0.0722f * source.Blue;
            switch (key)
            {
                case "stone-giant":
                    float stone = 0.18f + 0.82f * luminance;
                    return new GiantPalettePixel(0.69f * stone, 0.70f * stone,
                        0.72f * stone, source.Alpha);
                case "fire-giant":
                    // Warm charcoal/iron, deliberately distinct from both the
                    // native ice-blue body and Storm's brighter bronze.
                    float iron = 0.16f + 0.84f * luminance;
                    return new GiantPalettePixel(0.56f * iron, 0.34f * iron,
                        0.23f * iron, source.Alpha);
                case "cloud-giant":
                    float pale = 0.55f + 0.45f * luminance;
                    return new GiantPalettePixel(0.84f * pale, 0.91f * pale,
                        0.98f * pale, source.Alpha);
                case "storm-giant":
                    float bronze = 0.25f + 0.75f * luminance;
                    return new GiantPalettePixel(0.90f * bronze, 0.65f * bronze,
                        0.40f * bronze, source.Alpha);
                default: throw new InvalidOperationException("No reviewed giant palette proposal: " + key);
            }
        }

        private static bool Channel(float value)
        { return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f && value <= 1f; }
    }
}
