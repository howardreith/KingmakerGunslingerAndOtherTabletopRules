using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Assets;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class WeaponVisualMappingAuditTests
    {
        internal static void CoversEveryActiveCustomWeaponIdentity()
        {
            JObject manifest = Parse("blueprints", "blueprints.json");
            JObject audit = Parse("docs", "weapon-visual-mapping-audit.json");
            JToken[] expected = manifest["entries"].Where(value =>
                (string)value["plannedType"] == "BlueprintItemWeapon" &&
                (string)value["status"] == "active").ToArray();
            JToken[] actual = audit["items"].ToArray();
            Assertions.Equal(68, expected.Length,
                "The active custom-weapon baseline changed without an audit update.");
            Assertions.Equal(expected.Length, actual.Length,
                "The visual audit does not cover every active custom weapon.");
            string[] expectedIdentities = expected.Select(value =>
                    (string)value["symbol"] + "|" + (string)value["guid"])
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] actualIdentities = actual.Select(value =>
                    (string)value["symbolicIdentity"] + "|" +
                    (string)value["assetGuid"])
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Assertions.True(expectedIdentities.SequenceEqual(actualIdentities),
                "The machine-readable audit changed or omitted a saved blueprint identity.");

            string markdown = Read("docs", "WEAPON-VISUAL-MAPPING-AUDIT.md");
            foreach (string symbol in expected.Select(value =>
                (string)value["symbol"]))
                Assertions.True(markdown.Contains(symbol),
                    "The narrative audit omitted " + symbol + ".");
        }

        internal static void RecordsEveryRequiredVisualContract()
        {
            JObject audit = Parse("docs", "weapon-visual-mapping-audit.json");
            string[] required = {
                "symbolicIdentity", "assetGuid", "displayedName",
                "familyOrFirearmKind", "weaponType", "weaponTypeAssetGuid",
                "currentItemLevelVisual", "currentTypeLevelVisual",
                "effectiveEquippedPrefab", "sourceFbx", "sourceBlend",
                "deterministicGenerator", "animationDonorStyle",
                "gripHandednessContract", "currentMaterial", "currentBundle",
                "sourceLicenseProvenance", "currentManyToOneVisualGroup",
                "proposedVisualVariant", "clippingOrientationConcerns", "tier",
                "mappingScope"
            };
            foreach (JToken item in audit["items"])
                foreach (string field in required)
                    Assertions.True(item[field] != null &&
                        !string.IsNullOrWhiteSpace((string)item[field]),
                        (string)item["symbolicIdentity"] +
                        " lacks required audit field " + field + ".");

            Assertions.Equal(56, audit["items"].Count(value =>
                (string)value["mappingScope"] == "equipped project weapon"),
                "Equipped custom-weapon audit scope changed.");
            Assertions.Equal(2, audit["items"].Count(value =>
                (string)value["mappingScope"] == "mechanics-only exclusion"),
                "Pistol-Whip preserve-only scope changed.");
            Assertions.Equal(10, audit["items"].Count(value =>
                (string)value["mappingScope"] == "summoning-only exclusion"),
                "Expanded Summoning weapon scope changed.");
            Assertions.True(audit["items"].Where(value =>
                    (string)value["mappingScope"] == "equipped project weapon")
                .All(value => (string)value["proposedVisualVariant"] !=
                    "not applicable"),
                "An equipped project weapon lacks an explicit proposed variant.");
        }

        internal static void VariantVocabularyIsBoundedAndFamilySafe()
        {
            JObject audit = Parse("docs", "weapon-visual-mapping-audit.json");
            JToken[] equipped = audit["items"].Where(value =>
                (string)value["mappingScope"] == "equipped project weapon").ToArray();
            var expectedCounts = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                { "Pistol", 4 }, { "Musket", 5 }, { "Blunderbuss", 3 },
                { "Rifle", 1 }, { "Revolver", 1 },
                { "Elven Branched Spear", 12 }, { "Wakizashi", 10 },
                { "Katana", 10 }, { "Nodachi", 10 }
            };
            foreach (KeyValuePair<string, int> pair in expectedCounts)
                Assertions.Equal(pair.Value, equipped.Count(value =>
                    (string)value["familyOrFirearmKind"] == pair.Key),
                    "Visual audit family count changed for " + pair.Key + ".");

            foreach (IGrouping<string, JToken> family in equipped.GroupBy(value =>
                (string)value["familyOrFirearmKind"]))
            {
                int variants = family.Select(value =>
                    (string)value["proposedVisualVariant"]).Distinct().Count();
                Assertions.True(variants <= 5,
                    family.Key + " exceeds the bounded five-variant vocabulary.");
                bool isHumanGatedLongGun = family.Key == "Musket" ||
                    family.Key == "Blunderbuss";
                if (family.Count() >= 3 && !isHumanGatedLongGun)
                    Assertions.True(variants >= 2,
                        family.Key + " lacks meaningful proposed variety.");
                if (isHumanGatedLongGun)
                    Assertions.Equal(1, variants, family.Key +
                        " variants must remain on the accepted Service model until the fit candidate passes human review.");
                Assertions.True(family.All(value =>
                    ((string)value["proposedVisualVariant"]).StartsWith(
                        family.Key.Replace(" ", string.Empty),
                        StringComparison.Ordinal) ||
                    family.Key == "Elven Branched Spear" &&
                    ((string)value["proposedVisualVariant"]).StartsWith(
                        "ElvenBranchedSpear", StringComparison.Ordinal)),
                    family.Key + " contains a cross-family proposed variant.");
            }
            Assertions.Equal("exact deterministic blueprint identity; no runtime randomness or transient state",
                (string)audit["mappingPolicy"],
                "Deterministic variant authority changed.");
        }

        internal static void RuntimeCatalogMatchesApprovedSpearVariants()
        {
            JObject audit = Parse("docs", "weapon-visual-mapping-audit.json");
            JToken[] spears = audit["items"].Where(value =>
                (string)value["familyOrFirearmKind"] ==
                    "Elven Branched Spear").ToArray();
            Assertions.Equal(12, spears.Length,
                "The approved spear item mapping count changed.");
            foreach (JToken spear in spears)
            {
                string symbol = (string)spear["symbolicIdentity"];
                string variant;
                Assertions.True(WeaponVisualVariantCatalog.TryGet(symbol,
                    out variant), symbol + " lacks a runtime visual mapping.");
                Assertions.Equal((string)spear["proposedVisualVariant"],
                    variant, symbol + " audit/runtime visual mapping diverged.");
            }
            Assertions.Equal(3, spears.Select(value =>
                (string)value["proposedVisualVariant"]).Distinct().Count(),
                "The spear vocabulary must remain bounded to three variants.");
        }

        internal static void RuntimeCatalogMatchesApprovedEasternVariants()
        {
            JObject audit = Parse("docs", "weapon-visual-mapping-audit.json");
            string[] families = { "Wakizashi", "Katana", "Nodachi" };
            JToken[] items = audit["items"].Where(value => families.Contains(
                (string)value["familyOrFirearmKind"])).ToArray();
            Assertions.Equal(30, items.Length,
                "The approved Eastern item mapping count changed.");
            foreach (JToken item in items)
            {
                string symbol = (string)item["symbolicIdentity"];
                string variant;
                Assertions.True(WeaponVisualVariantCatalog.TryGet(symbol,
                    out variant), symbol + " lacks a runtime visual mapping.");
                Assertions.Equal((string)item["proposedVisualVariant"],
                    variant, symbol + " audit/runtime visual mapping diverged.");
                Assertions.True(variant.StartsWith(
                    (string)item["familyOrFirearmKind"] + ".",
                    StringComparison.Ordinal),
                    symbol + " crosses its qualified family boundary.");
            }
            foreach (string family in families)
                Assertions.Equal(4, items.Where(value =>
                    (string)value["familyOrFirearmKind"] == family).Select(
                    value => (string)value["proposedVisualVariant"])
                    .Distinct().Count(), family +
                    " must use exactly four reusable variants.");
        }

        internal static void RuntimeCatalogMatchesApprovedFirearmVariants()
        {
            JObject audit = Parse("docs", "weapon-visual-mapping-audit.json");
            string[] families = { "Pistol", "Musket", "Blunderbuss", "Rifle",
                "Revolver" };
            JToken[] items = audit["items"].Where(value => families.Contains(
                (string)value["familyOrFirearmKind"])).ToArray();
            Assertions.Equal(14, items.Length,
                "The approved equipped firearm item count changed.");
            foreach (JToken item in items)
            {
                string symbol = (string)item["symbolicIdentity"];
                string variant;
                Assertions.True(WeaponVisualVariantCatalog.TryGet(symbol,
                    out variant), symbol + " lacks a runtime firearm mapping.");
                Assertions.Equal((string)item["proposedVisualVariant"], variant,
                    symbol + " audit/runtime firearm mapping diverged.");
                Assertions.True(variant.StartsWith(
                    (string)item["familyOrFirearmKind"] + ".",
                    StringComparison.Ordinal),
                    symbol + " crosses its firearm family boundary.");
            }
            Assertions.Equal(3, items.Where(value =>
                    (string)value["familyOrFirearmKind"] == "Pistol")
                .Select(value => (string)value["proposedVisualVariant"])
                .Distinct().Count(),
                "Pistol must use exactly three approved reusable variants.");
            Assertions.True(items.Where(value =>
                    (string)value["familyOrFirearmKind"] == "Musket" ||
                    (string)value["familyOrFirearmKind"] == "Blunderbuss")
                .All(value => ((string)value["proposedVisualVariant"])
                    .EndsWith(".Service", StringComparison.Ordinal)),
                "Long-gun named variants must remain gated on fit acceptance.");
        }

        private static JObject Parse(params string[] parts)
        { return JObject.Parse(Read(parts)); }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            for (int index = 0; index < parts.Length; index++)
                path = Path.Combine(path, parts[index]);
            return File.ReadAllText(path);
        }
    }
}
