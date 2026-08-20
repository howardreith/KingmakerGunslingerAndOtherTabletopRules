using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmFeatIconTests
    {
        internal static void NativeStylePublicationIsExact()
        {
            string root = Environment.CurrentDirectory;
            string[] keys = {
                "firearm-monogram-pistol", "firearm-monogram-musket",
                "firearm-monogram-blunderbuss", "firearm-monogram-rifle",
                "firearm-monogram-revolver", "rapid-reload" };
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
            string spec = File.ReadAllText(Path.Combine(root, "assets-source",
                "original-icons", "firearm-feats", "icon-spec.json"));
            foreach (string token in new[] { "\"P\"", "\"M\"", "\"B\"",
                "\"Ri\"", "\"Rv\"", "native-custom-weapon-selector-monogram",
                "CustomWeaponSelectorRuntime FeatureUIData null-icon plus monogram",
                "parchmentLight", "oxblood", "cornerBlue", "Segoe Script" })
                Assertions.True(spec.Contains(token),
                    "Editable icon specification lacks token: " + token);
            string generator = File.ReadAllText(Path.Combine(root, "tools",
                "New-FirearmFeatIcons.ps1"));
            foreach (string token in new[] { "Draw-NativeParameterField",
                "Draw-CalligraphicMonogram", "Draw-RapidReloadGlyph",
                "firearm-feat-icon-map.png", "DrawImage($image, $x + 16, 86, 32, 32)" })
                Assertions.True(generator.Contains(token),
                    "Deterministic native-style icon generator lacks: " + token);
            Assertions.False(generator.Contains("FillEllipse($badge") ||
                spec.Contains("\"template\":\"dark"),
                "Rejected dark circular badge construction returned.");
            string publication = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints", "ProjectAssetIcons.cs"));
            Assertions.True(publication.Contains("ApplyFirearmFeatIcons(feats)") &&
                publication.Contains("feats.WeaponFocusChoices") &&
                publication.Contains("feats.RapidReloadChoices") &&
                publication.Contains("feats.DependentChoices") &&
                publication.Contains("FirearmFeatIconKeys") &&
                publication.Contains("KMG_Icon_rapid-reload"),
                "Exact firearm choice-icon publication is incomplete.");
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
                foreach (string key in keys.Take(5))
                    Assertions.True(source.Contains("'" + key + "'"),
                        validator + " does not require icon " + key);
            }
        }

        private static int ReadBigEndian(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) |
                (bytes[offset + 2] << 8) | bytes[offset + 3];
        }
    }
}
