using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmFeatIconTests
    {
        internal static void OfficialSupportBoundaryIsExact()
        {
            FirearmKind[] expected = { FirearmKind.Pistol,
                FirearmKind.Musket, FirearmKind.Blunderbuss };
            FirearmKind[] official = OfficialFirearmSupport.Kinds;
            Assertions.True(official.SequenceEqual(expected),
                "Official firearm support is not exactly Pistol, Musket, and Blunderbuss.");
            Assertions.True(expected.All(OfficialFirearmSupport.IsOfficial),
                "An official firearm kind was rejected.");
            Assertions.False(OfficialFirearmSupport.IsOfficial(FirearmKind.Rifle) ||
                OfficialFirearmSupport.IsOfficial(FirearmKind.Revolver),
                "A legacy firearm returned to official support.");
            Assertions.True(OfficialFirearmSupport.IsLegacy(FirearmKind.Rifle) &&
                OfficialFirearmSupport.IsLegacy(FirearmKind.Revolver) &&
                OfficialFirearmSupport.IsRecognized(FirearmKind.Rifle) &&
                OfficialFirearmSupport.IsRecognized(FirearmKind.Revolver),
                "Legacy firearm recognition was not retained.");
            FirearmKind[] recognized = OfficialFirearmSupport.RecognizedKinds;
            Assertions.Equal(5, recognized.Length,
                "Stable firearm recognition identities changed.");
            official[0] = FirearmKind.Revolver;
            recognized[0] = FirearmKind.Unknown;
            Assertions.Equal(FirearmKind.Pistol,
                OfficialFirearmSupport.Kinds[0],
                "Official firearm storage was externally mutable.");
            Assertions.Equal(FirearmKind.Pistol,
                OfficialFirearmSupport.RecognizedKinds[0],
                "Recognized firearm storage was externally mutable.");

            string root = Environment.CurrentDirectory;
            string feat = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "FirearmFeatBlueprints.cs"));
            string training = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "GunTrainingBlueprints.cs"));
            string native = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Feats",
                "NativeFirearmFeatIntegration.cs"));
            foreach (string source in new[] { feat, training })
            {
                Assertions.True(source.Contains("OfficialFirearmSupport.Kinds") &&
                    source.Contains("OfficialFirearmSupport.RecognizedKinds"),
                    "A player-facing firearm selection lacks the centralized boundary.");
            }
            Assertions.True(native.Contains("_publishedKinds") &&
                native.Contains("_recognizedKinds"),
                "Native feat menus do not separate publication from legacy recognition.");
        }

        internal static void NativeStylePublicationIsExact()
        {
            string root = Environment.CurrentDirectory;
            string[] keys = {
                "firearm-monogram-pistol", "firearm-monogram-musket",
                "firearm-monogram-blunderbuss", "rapid-reload" };
            var hashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
            {
                string path = Path.Combine(root, "assets", "game", "icons",
                    key + ".png");
                Assertions.True(File.Exists(path), "Feat icon is missing: " + key);
                byte[] bytes = File.ReadAllBytes(path);
                Assertions.True(bytes.Length > 24 && bytes[0] == 0x89 &&
                    bytes[1] == 0x50 && bytes[2] == 0x4e && bytes[3] == 0x47 &&
                    ReadBigEndian(bytes, 16) == 64 && ReadBigEndian(bytes, 20) == 64,
                    "Feat icon is not an exact 64-by-64 PNG: " + key);
                using (SHA256 sha = SHA256.Create())
                    hashes.Add(BitConverter.ToString(sha.ComputeHash(bytes)));
            }
            Assertions.Equal(keys.Length, hashes.Count,
                "Firearm monograms and Rapid Reload are not distinct assets.");
            foreach (string retired in new[] { "firearm-monogram-rifle",
                "firearm-monogram-revolver" })
                Assertions.False(File.Exists(Path.Combine(root, "assets", "game",
                    "icons", retired + ".png")),
                    "A retired player-facing selector icon returned: " + retired);
            string spec = File.ReadAllText(Path.Combine(root, "assets-source",
                "original-icons", "firearm-feats", "icon-spec.json"));
            foreach (string token in new[] { "\"schemaVersion\": 3",
                "\"P\"", "\"M\"", "\"B\"", "full-square",
                "original System.Drawing vector paths", "#A6533F",
                "no blue corners", "retiredPlayerFacingAssets" })
                Assertions.True(spec.Contains(token),
                    "Editable icon specification lacks token: " + token);
            Assertions.False(spec.Contains("Segoe Script") ||
                spec.Contains("cornerBlue") || spec.Contains("\"Ri\"") ||
                spec.Contains("\"Rv\""),
                "Retired or rejected selector styling returned to the specification.");
            string wrapper = File.ReadAllText(Path.Combine(root, "tools",
                "New-FirearmFeatIcons.ps1"));
            Assertions.True(wrapper.Contains("icon-art/New-IconOverhaulAssets.ps1") &&
                wrapper.Contains("-Mode Feat"),
                "Compatibility generator does not delegate to the overhaul pipeline.");
            string generator = File.ReadAllText(Path.Combine(root, "tools",
                "icon-art", "New-IconOverhaulAssets.ps1"));
            foreach (string token in new[] { "Draw-SelectorField",
                "Draw-OriginalMonogram", "Draw-RapidReloadGlyph",
                "firearm-feat-icon-map.png", "HighQualityBicubic" })
                Assertions.True(generator.Contains(token),
                    "Deterministic icon-overhaul generator lacks: " + token);
            Assertions.False(generator.Contains("FillEllipse($badge") ||
                generator.Contains("cornerBlue") || generator.Contains("Segoe Script") ||
                generator.Contains("Draw-RapidReloadField"),
                "Rejected badge, blue-corner, font, or Rapid Reload card construction returned.");
            Assertions.True(spec.Contains(
                "transparent canvas matching neighboring vanilla feat glyphs"),
                "Rapid Reload does not declare the live-verified transparent feat-glyph treatment.");
            string publication = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints", "ProjectAssetIcons.cs"));
            Assertions.True(publication.Contains("ApplyFirearmFeatIcons(feats)") &&
                publication.Contains("feats.WeaponFocusChoices") &&
                publication.Contains("feats.RapidReloadChoices") &&
                publication.Contains("feats.DependentChoices") &&
                publication.Contains("FirearmFeatIconKeys") &&
                publication.Contains("KMG_Icon_rapid-reload"),
                "Exact firearm choice-icon publication is incomplete.");
            Assertions.False(publication.Contains("firearm-monogram-rifle") ||
                publication.Contains("firearm-monogram-revolver"),
                "Retired monograms remain in runtime icon publication.");
            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runtime.Contains("firearm-feat-icon-map") &&
                runtime.Contains("nativeTopLevelIconsPreserved") &&
                runtime.Contains("rapidChoiceIconsExact"),
                "Guarded runtime does not inspect native menus and Rapid Reload icons.");
            foreach (string validator in new[] { "validate-build-output.ps1",
                "validate-package.ps1" })
            {
                string source = File.ReadAllText(Path.Combine(root, "scripts", validator));
                foreach (string key in keys.Take(3))
                    Assertions.True(source.Contains("'" + key + "'"),
                        validator + " does not require icon " + key);
                int listStart = Math.Max(source.IndexOf("$requiredIcons",
                    StringComparison.Ordinal), source.IndexOf("$iconNames",
                    StringComparison.Ordinal));
                int retiredCheck = source.IndexOf(
                    "foreach ($name in @('firearm-monogram-rifle'",
                    StringComparison.Ordinal);
                Assertions.True(listStart >= 0 && retiredCheck > listStart &&
                    source.Contains("Retired player-facing selector exists"),
                    validator + " lacks the retired-selector package rejection.");
                string requiredRegion = source.Substring(listStart,
                    retiredCheck - listStart);
                Assertions.False(requiredRegion.Contains("firearm-monogram-rifle") ||
                    requiredRegion.Contains("firearm-monogram-revolver"),
                    validator + " still requires a retired selector icon.");
            }
        }

        internal static void RuntimeVisualEvidenceIsGuardedAndLive()
        {
            string root = Environment.CurrentDirectory;
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "IconOverhaulVisualEvidenceScenario.cs"));
            foreach (string token in new[] {
                "Width = 1920", "Height = 1200",
                "GetFullSelectionItems()", "RapidReloadChoices",
                "ProductionFirearms.Blunderbuss", "MagicFirearms.GenericEntries",
                "EasternWeapons.Named.Entries", "ElvenBranchedSpears.Named.Entries",
                "after-01-rapid-reload-feat-list.png",
                "after-02-rapid-reload-supported-choices.png",
                "after-03-weapon-focus-firearm-choices.png",
                "after-04-supported-firearm-items.png",
                "after-05-eastern-and-spear-items.png",
                "supporting visual evidence only", "Camera", "RenderTexture" })
                Assertions.True(scenario.Contains(token),
                    "Runtime icon evidence lacks: " + token);
            Assertions.False(scenario.Contains("ScreenCapture") ||
                scenario.Contains("Input.") || scenario.Contains("SendKeys") ||
                scenario.Contains("SaveGame") || scenario.Contains("LoadGame"),
                "Runtime visual evidence introduced UI input, screen navigation, or save access.");

            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs"));
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            string project = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "KingmakerGunslinger.csproj"));
            Assertions.True(catalog.Contains("IconOverhaulVisualEvidence") &&
                catalog.Contains("icon-overhaul-visual-evidence") &&
                runner.Contains("IconOverhaulVisualEvidenceScenario.Run(") &&
                automation.Contains("'icon-overhaul-visual-evidence'") &&
                automation.Contains("ReadinessBehavior = 'mod-load'") &&
                project.Contains("IconOverhaulVisualEvidenceScenario.cs"),
                "Runtime icon evidence is not registered in every guarded layer.");
        }

        private static int ReadBigEndian(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) |
                (bytes[offset + 2] << 8) | bytes[offset + 3];
        }
    }
}
