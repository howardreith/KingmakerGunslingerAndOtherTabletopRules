using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Cleanup2GiantPaletteTests
    {
        internal static void ExactBodySelectorsAndSummonScope()
        {
            foreach (string key in new[] { "stone-giant", "cloud-giant" })
            {
                GiantPaletteSelector selector = GiantSummonPalettePolicy.Selector(key);
                Assertions.Equal("hill_giant_1", selector.Renderer, "Hill-family treatment must target the body only.");
                Assertions.Equal("hill_giant_01_character_d", selector.Material, "Hill body material identity changed.");
                Assertions.Equal("635d6dee1c7831b41aa987bdbeff14bc", selector.PrefabId, "Hill prefab identity changed.");
                Assertions.False(selector.Renderer == "hill_giant01_outfit_02", "Separate outfit must remain excluded.");
            }
            foreach (string key in new[] { "fire-giant", "storm-giant" })
            {
                GiantPaletteSelector selector = GiantSummonPalettePolicy.Selector(key);
                Assertions.Equal("Frozen_body", selector.Renderer, "Frost-family treatment must target the body only.");
                Assertions.Equal("frost_giant_character_d", selector.Material, "Frost body material identity changed.");
                Assertions.Equal("df0f3fbceb3605d489c0dec8fa5c7d1e", selector.PrefabId, "Frost prefab identity changed.");
                Assertions.False(selector.Renderer == "Frozen_cape1", "A shared material must not include the separate cape.");
            }
            foreach (string key in new[] { "hill-giant", "frost-giant", "Cloud-Giant", "KMG.CloudGiant", "", null })
                Assertions.True(GiantSummonPalettePolicy.Selector(key) == null, "No inferred or native-control selector is permitted.");
            Assertions.True(GiantSummonPalettePolicy.MaterialNameMatches("frost_giant_character_d (Instance)",
                "frost_giant_character_d"), "One known native instance suffix must be recognized.");
            foreach (string name in new[] { "Frost_giant_character_d", "frost_giant_character_d foreign",
                "frost_giant_character_d (Instance) (Instance)", "KMG_SummonVisualVariant", null })
                Assertions.False(GiantSummonPalettePolicy.MaterialNameMatches(name, "frost_giant_character_d"),
                    "Names and arbitrary clone suffixes cannot imply ownership.");
            foreach (GiantSummonPresentationProfile profile in GiantSummonPresentationPolicy.All)
                Assertions.True(GiantSummonPresentationPolicy.Resolve(false, profile.UnitGuid, null) == null,
                    "Module-off scope must remain inert for every giant.");
            Assertions.True(GiantSummonPresentationPolicy.Resolve(true,
                GiantSummonPresentationPolicy.NativeFrostGuid, null) == null,
                "Ordinary native Frost cannot be authorized by material identity.");
            Assertions.True(GiantSummonPresentationPolicy.Resolve(true,
                "61dd71e874b16f742a0f783314105553", null) == null,
                "The ordinary Hill donor cannot be authorized by a matching body.");
        }

        internal static void ProofRequiresExactIdentityAndWholeBodyTreatment()
        {
            Assertions.Equal("missing-albedo-proof", GiantSummonPalettePolicy.ValidateProof(null),
                "No default palette proof may be inferred.");
            foreach (string key in new[] { "stone-giant", "fire-giant", "cloud-giant", "storm-giant" })
            {
                Assertions.Equal("proven-body-albedo", GiantSummonPalettePolicy.ValidateProof(Proof(key)),
                    "Complete synthetic proof must exercise the admission path.");
                GiantPaletteAlbedoProof production = GiantSummonPalettePolicy.ProductionProof(key);
                Assertions.Equal("proven-body-albedo", GiantSummonPalettePolicy.ValidateProof(production),
                    "Production body identity must pass the same live proof contract.");
                Assertions.Equal(GiantSummonPalettePolicy.Selector(key).PrefabId, production.PrefabId,
                    "Production proof must bind the selected native donor, never a guessed family name.");
            }
            foreach (string key in new[] { "hill-giant", "frost-giant", "Cloud-Giant", "foreign" })
            {
                Assertions.Equal("palette-treatment-not-reviewed", GiantSummonPalettePolicy.ValidateProof(Proof(key)),
                    "Only the four intended body treatments may register.");
                Assertions.True(GiantSummonPalettePolicy.ProductionProof(key) == null,
                    "Native controls and unknown families must have no production treatment.");
            }
            Assertions.Equal("prefab-identity-mismatch", GiantSummonPalettePolicy.ValidateProof(Proof("cloud-giant", prefab: "foreign")),
                "A valid-looking slot cannot authorize the wrong prefab.");
            foreach (GiantPaletteAlbedoProof proof in new[] {
                Proof("cloud-giant", path: ""), Proof("cloud-giant", path: "../foreign"),
                Proof("cloud-giant", slot: -1), Proof("cloud-giant", texture: ""),
                Proof("cloud-giant", width: 0), Proof("cloud-giant", height: 4097),
                Proof("cloud-giant", format: "") })
                Assertions.Equal("invalid-slot-or-texture-identity", GiantSummonPalettePolicy.ValidateProof(proof),
                    "Missing, escaping or unbounded texture/slot identities must refuse.");
            Assertions.Equal("missing-identity-evidence", GiantSummonPalettePolicy.ValidateProof(Proof("cloud-giant", identity: "")),
                "Fresh renderer/texture identity evidence is required.");
            foreach (string format in new[] { "RGBAFloat", "RGBAHalf", "Alpha8", "foreign" })
                Assertions.Equal("unsupported-albedo-channel-format", GiantSummonPalettePolicy.ValidateProof(Proof("cloud-giant", format: format)),
                    "Higher-precision alpha or an unreviewed channel format cannot be silently quantized.");
            Assertions.Equal("missing-whole-body-palette-evidence", GiantSummonPalettePolicy.ValidateProof(Proof("cloud-giant", treatment: "")),
                "Deliberate whole-body treatment must be explicit, not implied by a renderer name.");
        }

        internal static void WholeBodyTransformsLiftCloudAndRetainAlphaAndShading()
        {
            foreach (string key in new[] { "stone-giant", "fire-giant", "cloud-giant", "storm-giant" })
            {
                GiantPalettePixel dark = GiantSummonPalettePolicy.Transform(key, new GiantPalettePixel(0f, 0f, 0f, 0.37f));
                GiantPalettePixel middle = GiantSummonPalettePolicy.Transform(key, new GiantPalettePixel(0.5f, 0.5f, 0.5f, 0.37f));
                GiantPalettePixel light = GiantSummonPalettePolicy.Transform(key, new GiantPalettePixel(1f, 1f, 1f, 0.37f));
                Assertions.True(dark.Red < middle.Red && middle.Red < light.Red &&
                    dark.Green < middle.Green && middle.Green < light.Green &&
                    dark.Blue < middle.Blue && middle.Blue < light.Blue,
                    "Source luminance ordering must remain visible in each target channel.");
                foreach (GiantPalettePixel result in new[] { dark, middle, light })
                {
                    Assertions.Equal(0.37f, result.Alpha, "Transform cannot change opacity.");
                    Assertions.True(new[] { result.Red, result.Green, result.Blue }.All(value => value >= 0f && value <= 1f),
                        "No HDR/clamped output is needed for this albedo treatment.");
                }
            }
            GiantPalettePixel cloudBlack = GiantSummonPalettePolicy.Transform("cloud-giant", new GiantPalettePixel(0f, 0f, 0f, 0f));
            Near(0.462f, cloudBlack.Red, "Cloud must lift source value, not merely multiply a dark donor.");
            Assertions.True(cloudBlack.Blue > cloudBlack.Green && cloudBlack.Green > cloudBlack.Red,
                "Cloud proposal must retain the pale blue direction.");
            Assertions.Equal(0f, cloudBlack.Alpha, "Transparent source remains transparent.");
            GiantPalettePixel stormWhite = GiantSummonPalettePolicy.Transform("storm-giant", new GiantPalettePixel(1f, 1f, 1f, 1f));
            Near(0.90f, stormWhite.Red, "Storm red target changed.");
            Near(0.65f, stormWhite.Green, "Storm green target changed.");
            Near(0.40f, stormWhite.Blue, "Storm bronze target changed.");
            GiantPalettePixel stone = GiantSummonPalettePolicy.Transform("stone-giant", new GiantPalettePixel(.5f, .5f, .5f, 1f));
            GiantPalettePixel fire = GiantSummonPalettePolicy.Transform("fire-giant", new GiantPalettePixel(.5f, .5f, .5f, 1f));
            GiantPalettePixel cloud = GiantSummonPalettePolicy.Transform("cloud-giant", new GiantPalettePixel(.5f, .5f, .5f, 1f));
            GiantPalettePixel storm = GiantSummonPalettePolicy.Transform("storm-giant", new GiantPalettePixel(.5f, .5f, .5f, 1f));
            Assertions.True(Math.Abs(stone.Red - stone.Blue) < .03f,
                "Stone must read as neutral grey rather than inherit the brown donor hue.");
            Assertions.True(fire.Red > fire.Green && fire.Green > fire.Blue && fire.Red < storm.Red,
                "Fire's warm charcoal must remain darker and distinct from Storm bronze.");
            Assertions.True(cloud.Blue > stone.Blue && cloud.Red > storm.Red,
                "Cloud must remain pale and distinct from both grey Stone and bronze Storm.");
        }

        internal static void TransformRejectsUnboundedPixelsAndUnreviewedFamilies()
        {
            foreach (float bad in new[] { -0.01f, 1.01f, float.NaN, float.PositiveInfinity })
                foreach (int channel in Enumerable.Range(0, 4))
                {
                    var values = new[] { 0.5f, 0.5f, 0.5f, 1f };
                    values[channel] = bad;
                    Assertions.Throws<ArgumentOutOfRangeException>(() => GiantSummonPalettePolicy.Transform("cloud-giant",
                        new GiantPalettePixel(values[0], values[1], values[2], values[3])), "Malformed pixel must refuse.");
                }
            foreach (string key in new[] { "hill-giant", "frost-giant", "foreign", null })
                Assertions.Throws<InvalidOperationException>(() => GiantSummonPalettePolicy.Transform(key,
                    new GiantPalettePixel(0.5f, 0.5f, 0.5f, 1f)), "Unreviewed family cannot receive a palette proposal.");
        }

        private static GiantPaletteAlbedoProof Proof(string key, string prefab = null,
            string path = "synthetic_body[0]", int slot = 0, string texture = "synthetic-albedo",
            int width = 16, int height = 16, string format = "RGBA32",
            string identity = "synthetic-test-only-identity", string treatment = "synthetic-test-only-whole-body-treatment")
        {
            GiantPaletteSelector selector = GiantSummonPalettePolicy.Selector(key);
            return new GiantPaletteAlbedoProof(key, prefab ?? (selector == null ? "foreign" : selector.PrefabId),
                path, slot, texture, width, height, format, identity, treatment);
        }

        private static void Near(float expected, float actual, string message)
        { Assertions.True(Math.Abs(expected - actual) <= 0.000001f, message); }
    }
}
