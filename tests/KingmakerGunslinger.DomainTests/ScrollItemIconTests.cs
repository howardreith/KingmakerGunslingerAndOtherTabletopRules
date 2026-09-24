using System;
using System.IO;
using System.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ScrollItemIconTests
    {
        // The strategic scroll items follow the owner-directed native scroll
        // convention: the item icon is a composed parchment scroll that
        // integrates the approved spell painting inside, while the bare
        // approved painting remains the spell ability identity.
        internal static void ComposedScrollAssignmentIsExact()
        {
            string root = Environment.CurrentDirectory;
            string assignment = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints", "OwnedIconAssignments.cs"));
            string[] spellKeys = { "teleport", "greater-teleport", "word-of-recall",
                "magic-circle-against-evil", "magic-circle-against-good", "magic-circle-against-chaos", "magic-circle-against-law" };
            foreach (string spell in spellKeys)
            {
                Assertions.True(assignment.Contains(
                    "new Binding(\"KMG.Spells." + PascalSpell(spell) +
                    ".Scroll\", \"scroll-of-" + spell + "\", typeof(BlueprintItemEquipmentUsable))"),
                    "Scroll item lost its composed scroll icon binding: " + spell);
                Assertions.True(assignment.Contains(
                    "new Binding(\"KMG.Spells." + PascalSpell(spell) +
                    ".Ability\", \"" + spell + "\", typeof(BlueprintAbility))"),
                    "Spell ability lost its approved painting binding: " + spell);
            }
            // The composed item binding must not collapse back onto the bare
            // spell painting (the exact regression this change fixes).
            foreach (string spell in spellKeys)
                Assertions.False(assignment.Contains(
                    "new Binding(\"KMG.Spells." + PascalSpell(spell) +
                    ".Scroll\", \"" + spell + "\","),
                    "Scroll item binds the bare spell painting again: " + spell);
            Assertions.True(assignment.Contains(
                "composite the exact native scroll shell") && assignment.Contains(
                "approved spell painting as the inner emblem"),
                "The native-shell compositing rationale is not recorded at the binding site.");
        }

        internal static void ComposedScrollAssetsAreRegistered()
        {
            string root = Environment.CurrentDirectory;
            string[] keys = { "scroll-of-teleport",
                "scroll-of-greater-teleport", "scroll-of-word-of-recall", "scroll-of-magic-circle-against-evil",
                "scroll-of-magic-circle-against-good", "scroll-of-magic-circle-against-chaos", "scroll-of-magic-circle-against-law" };
            foreach (string key in keys)
            {
                string asset = Path.Combine(root, "assets", "game", "icons",
                    key + ".png");
                Assertions.True(File.Exists(asset),
                    "Composed scroll icon is missing from game assets: " + key);
                byte[] header = new byte[24];
                using (FileStream stream = File.OpenRead(asset))
                    Assertions.Equal(24, stream.Read(header, 0, 24),
                        "Composed scroll icon is truncated: " + key);
                Assertions.True(header[0] == 0x89 && header[1] == 0x50 &&
                    header[2] == 0x4e && header[3] == 0x47,
                    "Composed scroll icon is not a PNG: " + key);
                Assertions.True(ReadBigEndian(header, 16) == 128 &&
                    ReadBigEndian(header, 20) == 128,
                    "Composed scroll icon is not an exact 128-by-128 PNG: " + key);
            }
            // The three composed icons must be distinct assets.
            var hashes = new System.Collections.Generic.HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
            foreach (string key in keys)
                using (var sha = System.Security.Cryptography.SHA256.Create())
                    hashes.Add(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(
                        Path.Combine(root, "assets", "game", "icons", key + ".png")))));
            Assertions.Equal(keys.Length, hashes.Count,
                "Composed scroll icons are not distinct assets.");
        }

        internal static void CatalogAndRuntimeContractStayAligned()
        {
            string root = Environment.CurrentDirectory;
            string catalog = File.ReadAllText(Path.Combine(root,
                "assets-source", "original-icons", "icon-catalog.json"));
            string[] symbols = { "KMG.Spells.Teleport.Scroll",
                "KMG.Spells.GreaterTeleport.Scroll", "KMG.Spells.WordOfRecall.Scroll" };
            foreach (string symbol in symbols)
            {
                int at = catalog.IndexOf("\"symbol\": \"" + symbol + "\"",
                    StringComparison.Ordinal);
                Assertions.True(at >= 0, "Scroll consumer vanished from catalog: " + symbol);
                string region = catalog.Substring(at, Math.Min(900, catalog.Length - at));
                Assertions.True(region.Contains("\"currentArt\": \"owned-v3-scroll-composition\""),
                    "Scroll consumer does not use the composed art source: " + symbol);
                Assertions.True(region.Contains("\"disposition\": \"original-required\""),
                    "Scroll consumer is not dedicated original art: " + symbol);
            }
            foreach (string token in new[] {
                "\"paintedConceptCount\": 100", "\"paintedConsumerCount\": 160",
                "scroll-of-teleport", "scroll-of-greater-teleport",
                "scroll-of-word-of-recall" })
                Assertions.True(catalog.Contains(token),
                    "Catalog lacks scroll composition contract token: " + token);
            // The corrected icons are asset composites on the exact native
            // scroll shell, not newly painted scroll artwork.
            foreach (string token in new[] {
                "asset compositing: exact native scroll shell",
                "cross-donor median of five same-design native scroll sprites" })
                Assertions.True(catalog.Contains(token),
                    "Catalog lacks the native-shell compositing method token: " + token);
            string guide = File.ReadAllText(Path.Combine(root, "docs",
                "ICON-ART-GUIDE.md"));
            foreach (string token in new[] {
                "ASSET COMPOSITES on the exact native scroll shell",
                "cross-donor\nmedian", "no procedural" })
                Assertions.True(guide.Contains(token),
                    "Guide lacks the corrected native-shell contract token: " + token);
            // Package count carries the three composed icons.
            string package = File.ReadAllText(Path.Combine(root, "scripts", "package.ps1"));
            Assertions.True(package.Contains("{ 237 } else { 235 }"),
                "Package file count does not include the three composed scroll icons.");
            // The runtime identity check must verify the composed item icon,
            // not the retired spell-matches-item equality.
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.TeleportationScrollIcons.cs"));
            string merchant = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.TeleportationScrollMerchantIcons.cs"));
            foreach (string source in new[] { runner, merchant })
            {
                Assertions.True(source.Contains("scrollIconExact") &&
                    source.Contains("spellIconDistinctFromItem"),
                    "Guarded runtime lost the composed scroll identity assertion.");
                Assertions.False(source.Contains("spellIconMatchesItem"),
                    "The retired spell-matches-item icon assertion returned.");
            }
            Assertions.True(runner.Contains("ExpectedScrollIconKey") &&
                merchant.Contains("DescribeNativeScrollSlot"),
                "Guarded runtime lost the shared composed scroll identity helper.");
            foreach (string key in new[] { "scroll-of-teleport",
                "scroll-of-greater-teleport", "scroll-of-word-of-recall" })
                Assertions.True(runner.Contains("\"" + key + "\""),
                    "Guarded runtime does not expect the composed key: " + key);
        }

        internal static void NativeScrollReferenceDumpStaysReadonly()
        {
            string root = Environment.CurrentDirectory;
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "IconOverhaulVisualEvidenceScenario.cs"));
            foreach (string token in new[] {
                "DumpNativeScrollReferences", "after-12-native-scroll-references.png",
                "REFERENCE ONLY - NOT PROJECT ART", "nativeScrollReferences" })
                Assertions.True(scenario.Contains(token),
                    "Native scroll reference dump lacks: " + token);
            Assertions.False(scenario.Contains("ScreenCapture") ||
                scenario.Contains("Input.") || scenario.Contains("SendKeys"),
                "Native scroll reference dump gained UI input or screen capture.");
        }

        private static string PascalSpell(string key)
        {
            if (key.StartsWith("magic-circle-against-", StringComparison.Ordinal)) {
                string alignment = key.Substring("magic-circle-against-".Length);
                return "MagicCircle." + char.ToUpperInvariant(alignment[0]) + alignment.Substring(1);
            }
            return string.Concat(key.Split('-').Select(
                part => char.ToUpperInvariant(part[0]) + part.Substring(1)));
        }

        private static int ReadBigEndian(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) |
                (bytes[offset + 2] << 8) | (bytes[offset + 3]);
        }
    }
}
