using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningPresentationTests
    {
        internal static void OriginalIconManifestMatchesFiles()
        {
            string root = Environment.CurrentDirectory;
            string manifestPath = Path.Combine(root, "assets-source",
                "original-icons", "expanded-summoning", "icon-manifest.json");
            JObject manifest = JObject.Parse(File.ReadAllText(manifestPath));
            JArray rows = (JArray)manifest["icons"];
            Assertions.Equal(1, (int)manifest["schemaVersion"],
                "Icon manifest schema changed.");
            Assertions.Equal(77, (int)manifest["count"],
                "Icon manifest count changed.");
            Assertions.Equal(77, rows.Count,
                "Icon manifest row count changed.");
            string[] catalogKeys = SummonIconCatalog.All.Select(value =>
                value.Key).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] manifestKeys = rows.Select(value => (string)value["key"])
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Assertions.Equal(string.Join("|", catalogKeys),
                string.Join("|", manifestKeys),
                "Icon manifest has missing or stale concepts.");

            var hashes = new HashSet<string>(StringComparer.Ordinal);
            foreach (JToken row in rows)
            {
                string source = Path.Combine(root, ((string)row["sourceFile"])
                    .Replace('/', Path.DirectorySeparatorChar));
                string output = Path.Combine(root, ((string)row["productionFile"])
                    .Replace('/', Path.DirectorySeparatorChar));
                Assertions.True(File.Exists(source), "Icon source is missing: " + source);
                Assertions.True(File.Exists(output), "Production icon is missing: " + output);
                Assertions.Equal((string)row["sourceSha256"], Hash(source),
                    "Icon source hash changed.");
                string outputHash = Hash(output);
                Assertions.Equal((string)row["outputSha256"], outputHash,
                    "Production icon hash changed.");
                Assertions.True(hashes.Add(outputHash),
                    "Unrelated icon concepts share production pixels.");
                AssertImage(output, 128, 128);
            }

            string outputDirectory = Path.Combine(root, "assets", "game",
                "icons", "expanded-summoning");
            string[] actual = Directory.GetFiles(outputDirectory, "*.png")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Assertions.Equal(string.Join("|", manifestKeys), string.Join("|", actual),
                "Production icon directory has missing or unmanifested PNGs.");
        }

        internal static void OriginalIconSourceContractIsExclusive()
        {
            string root = Environment.CurrentDirectory;
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning", "SummonIconCatalog.cs"));
            string generated = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningAbilityBuilder.cs"));
            string split = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNativeOptionBuilder.cs"));
            string assignment = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningIconBuilder.cs"));
            Assertions.False(catalog.Contains("SummonIconSourceKind") ||
                catalog.Contains("SourceGuid"),
                "Production summon icon catalog may not reference game GUID sources.");
            Assertions.False(generated.Contains("IconFor(") ||
                generated.Contains(".Portrait"),
                "Generated summon abilities may not inherit unit/category icons.");
            Assertions.False(split.Contains("source.Icon"),
                "Split-native wrappers may not inherit Owlcat icons.");
            Assertions.True(assignment.Contains(
                    "ExpandedSummoningProjectIcons.Require("),
                "Final icon assignment must use the project icon cache.");
        }

        internal static void RuntimeIconCacheAndPackagePathsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string loader = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningProjectIcons.cs"));
            string project = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "KingmakerGunslinger.csproj"));
            string package = File.ReadAllText(Path.Combine(root, "scripts",
                "package.ps1"));
            Assertions.True(loader.Contains("context.ModEntry.Path") &&
                loader.Contains("Dictionary<string, Sprite>") &&
                loader.Contains("DontUnloadUnusedAsset") &&
                loader.Contains("FallbackCount { get { return 0; }") &&
                loader.Contains("Icons.Add(spec.Key, sprite)"),
                "Runtime icon cache must be mod-rooted, persistent, one-per-key, and fallback-free.");
            Assertions.True(project.Contains(
                    "assets\\game\\icons\\expanded-summoning\\*.png") &&
                project.Contains("expanded-summoning\\icon-manifest.json"),
                "Runtime project must include every manifest-backed icon path.");
            Assertions.True(package.Contains("expanded-summoning") &&
                package.Contains("summonIconDestination") &&
                package.Contains("expectedPackageFileCount = if ($hasFirearmSoundBank) { 123 } else { 121 }"),
                "Standalone package must stage the exact runtime icon tree.");
        }

        internal static void SnaWrappersNamingAndScaleAreExact()
        {
            SummonNativeExpansionSpec[] allies = SummonNativeExpansionCatalog.All
                .Where(value => value.Family == SummonFamily.NaturesAlly).ToArray();
            Assertions.Equal(9, allies.Length,
                "Creature-named SNA preservation wrapper count changed.");
            Assertions.True(allies.Any(value => value.Tier == 1 &&
                    value.DisplayName == "Mite" && value.IconKey == "mite" &&
                    value.Multiplicity == SummonMultiplicity.One),
                "SNA I generic preservation child must be replaced by the Mite wrapper.");
            Assertions.True(allies.All(value => value.DisplayName !=
                    "Summon Nature's Ally I"),
                "No SNA wrapper may retain a generic parent name.");
            Assertions.True(ExpandedSummoningCatalog.All.Single(value =>
                    value.Key == "dire-tiger").DisplayName == "Smilodon",
                "Stable dire-tiger identity must display as Smilodon.");
            Assertions.False(ExpandedSummoningCatalog.All.Any(value =>
                    value.DisplayName == "Dire Tiger / Smilodon"),
                "Legacy Smilodon display name remains player-facing.");
            float eagle;
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_Eagle", out eagle) && eagle == 0.30f,
                "Eagle view-only scale changed.");
            Assertions.True(SummonViewScaleCatalog.All.All(value =>
                    value.Multiplier >= 0.20f && value.Multiplier <= 1.25f),
                "Accepted view-only scale bounds changed.");
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runtime.Contains(
                    "expanded-summoning-eagle-medium-humanoid-scale") &&
                runtime.Contains("eagleHeight < mediumHumanoidHeight") &&
                runtime.Contains("Size == Size.Small") &&
                runtime.Contains("eagle-medium-humanoid-live-comparison.png") &&
                runtime.Contains("WriteExpandedSummoningEagleComparison") &&
                runtime.Contains("Camera.main") &&
                runtime.Contains("camera.CopyFrom(liveCamera)") &&
                runtime.Contains("detached camera frame") &&
                runtime.Contains("meaningfulPixels") &&
                runtime.Contains("framebuffer was blank"),
                "Visual runtime gate must compare Eagle live bounds to a Medium humanoid while retaining mechanical Small size.");
            string contactSheets = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "tools",
                "New-ExpandedSummoningMenuContactSheets.ps1"));
            Assertions.True(runtime.Contains(
                    "expanded-summoning-menu-contact-sheet-index.json") &&
                runtime.Contains("expanded-summoning-menu-contact-sheets") &&
                contactSheets.Contains("exact final-live order") &&
                contactSheets.Contains("$index.sheets).Count -ne 9") &&
                contactSheets.Contains("texture is not 128x128") &&
                contactSheets.Contains("lacks a project-owned sprite key"),
                "Runtime menu evidence must index and render nine exact live sprite sheets.");
        }

        internal static void PlayerPathHarnessUsesFamilyParentOffsets()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(source.Contains("SummonFamily.Monster ? 0 : 9") &&
                source.Contains("nativeExpansionCases.Count == 26") &&
                source.Contains("26/26 visible creature-named"),
                "Player-path coverage must route all 17 SM and nine SNA wrappers through their actual family parents.");
            Assertions.False(source.Contains(
                    "spellbook, parents[nativeSpec.Tier - 1], distinct"),
                "SNA wrappers may not be tested through Summon Monster parents.");
        }

        private static void AssertImage(string path, int width, int height)
        {
            using (var bitmap = new Bitmap(path))
            {
                Assertions.Equal(width, bitmap.Width, "Icon width changed.");
                Assertions.Equal(height, bitmap.Height, "Icon height changed.");
                Assertions.True(bitmap.RawFormat.Guid == ImageFormat.Png.Guid,
                    "Production icon is not PNG.");
                int visible = 0, white = 0;
                var colors = new HashSet<int>();
                double sum = 0, squares = 0;
                for (int y = 0; y < bitmap.Height; y += 2)
                    for (int x = 0; x < bitmap.Width; x += 2)
                    {
                        Color pixel = bitmap.GetPixel(x, y);
                        colors.Add(pixel.ToArgb());
                        if (pixel.A > 8) visible++;
                        if (pixel.A > 248 && pixel.R > 248 && pixel.G > 248 &&
                            pixel.B > 248) white++;
                        double luma = 0.2126 * pixel.R + 0.7152 * pixel.G +
                            0.0722 * pixel.B;
                        sum += luma; squares += luma * luma;
                    }
                const int samples = 4096;
                double variance = squares / samples -
                    Math.Pow(sum / samples, 2);
                Assertions.True(visible >= samples / 4 &&
                    white < samples * 0.95 && colors.Count >= 128 &&
                    variance >= 80,
                    "Icon is transparent, white, uniform, or visually blank.");
            }
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return string.Concat(sha.ComputeHash(stream).Select(value =>
                    value.ToString("x2")));
        }
    }
}
