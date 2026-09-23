using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Acquisition.BetterVendors;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.CraftMagicItemsCompatibility;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Reloading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BetterVendorsProgressionTests
    {
        private const string ManifestPrefixSha256 =
            "43909eb0094d38862dfe26c61345554b5d364d523a2c1ce1b91b996760a52fd2";
        private const int PreservedManifestEntries = 1913;
        private const string CatalogManifestPath =
            "docs/better-vendors-progression-catalog.json";

        // ---------------------------------------------------------------
        // A. Catalog and blueprint identity contracts
        // ---------------------------------------------------------------

        internal static void CatalogHasExactlyTheFiftyAuthorizedEntries()
        {
            ProgressionWeaponSpec[] all = ProgressionWeaponCatalog.All;
            Assertions.Equal(50, all.Length, "Progression entry count changed.");
            Assertions.Equal(30, all.Count(value => value.IsFirearm),
                "Firearm entry count changed.");
            Assertions.Equal(15, all.Count(value => value.IsFirearm && !value.Reliable),
                "Ordinary firearm entry count changed.");
            Assertions.Equal(15, all.Count(value => value.Reliable),
                "Reliable firearm entry count changed.");
            Assertions.Equal(20, all.Count(value => !value.IsFirearm),
                "Melee entry count changed.");
            Assertions.Equal(7, all.Count(value => value.ReusesCanonicalItem),
                "Reused canonical +1 count changed.");
            Assertions.Equal(43, all.Count(value => !value.ReusesCanonicalItem),
                "New blueprint count changed.");
            Assertions.Equal(ProgressionWeaponCatalog.NewBlueprintCount,
                all.Count(value => !value.ReusesCanonicalItem),
                "New blueprint constant diverged from the catalog.");
            foreach (Func<ProgressionWeaponSpec, string> key in new
                Func<ProgressionWeaponSpec, string>[] { value => value.Symbol,
                    value => value.Guid, value => value.InternalName,
                    value => value.DisplayName, value => value.Key })
                Assertions.Equal(50, all.Select(key).Distinct(
                    StringComparer.Ordinal).Count(),
                    "Progression identities collide.");
            foreach (ProgressionWeaponFamily family in Enum.GetValues(
                typeof(ProgressionWeaponFamily)))
            {
                int[] ordinary = all.Where(value => value.Family == family &&
                        !value.Reliable).Select(value => value.ActualEnhancement)
                    .OrderBy(value => value).ToArray();
                Assertions.True(ordinary.SequenceEqual(new[] { 1, 2, 3, 4, 5 }),
                    family + " ordinary coverage must be exactly +1..+5.");
                int[] reliable = all.Where(value => value.Family == family &&
                        value.Reliable).Select(value => value.ActualEnhancement)
                    .OrderBy(value => value).ToArray();
                Assertions.True(ProgressionWeaponCatalog.IsFirearm(family)
                    ? reliable.SequenceEqual(new[] { 1, 2, 3, 4, 5 })
                    : reliable.Length == 0,
                    family + " Reliable coverage is wrong.");
                Assertions.Equal(1, all.Count(value => value.Family == family &&
                    value.ReusesCanonicalItem), family + " must reuse exactly its +1.");
                Assertions.True(all.Single(value => value.Family == family &&
                    value.ReusesCanonicalItem).ActualEnhancement == 1 &&
                    !all.Single(value => value.Family == family &&
                        value.ReusesCanonicalItem).Reliable,
                    family + " may reuse only its ordinary +1.");
            }
            Assertions.True(all.All(value => value.ProgressionEligible &&
                    value.UnlockTier == value.ActualEnhancement),
                "Every entry must be explicitly eligible and unlock at its actual enhancement.");
        }

        internal static void ReusedCanonicalIdentitiesAreUnchanged()
        {
            var expected = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "KMG.Firearms.PistolPlus1Item", "d0145d0410a34df08d68a67367c1dfc9" },
                { "KMG.Firearms.MusketPlus1Item", "3402fe01de1648b187c192500e370f01" },
                { "KMG.Firearms.BlunderbussPlus1Item", "1dc7efe0792040f187a18adfdc54c6e0" },
                { "KMG.ElvenBranchedSpear.Plus1Item", "66111becd22690a2a19444a5c6bd0c7b" },
                { "KMG.EasternWeapons.Wakizashi.Plus1Item", "83a507873a518b54793d0da632def246" },
                { "KMG.EasternWeapons.Katana.Plus1Item", "87b3d851726a4a9abd0baec6beca957c" },
                { "KMG.EasternWeapons.Nodachi.Plus1Item", "38e31ba5cdbdc668f8dcd8985070c0b7" }
            };
            ProgressionWeaponSpec[] reused = ProgressionWeaponCatalog.All.Where(
                value => value.ReusesCanonicalItem).ToArray();
            Assertions.Equal(expected.Count, reused.Length, "Reused set changed.");
            foreach (ProgressionWeaponSpec spec in reused)
                Assertions.Equal(expected[spec.Symbol], spec.Guid,
                    "Reused canonical GUID changed: " + spec.Symbol);
            Assertions.Equal(ElvenBranchedSpearCatalog.Require(
                    ElvenBranchedSpearItemKind.PlusOne).Symbol,
                ProgressionWeaponCatalog.All.Single(value => value.Family ==
                    ProgressionWeaponFamily.ElvenBranchedSpear &&
                    value.ReusesCanonicalItem).Symbol,
                "Spear +1 must be the canonical catalog identity.");
            foreach (EasternWeaponFamily family in Enum.GetValues(
                typeof(EasternWeaponFamily)))
                Assertions.Equal(EasternWeaponCatalog.RequireGeneric(family,
                        EasternWeaponGenericKind.PlusOne).Symbol,
                    ProgressionWeaponCatalog.All.Single(value =>
                        value.ReusesCanonicalItem && !value.IsFirearm &&
                        value.Family.ToString() == family.ToString()).Symbol,
                    family + " +1 must be the canonical catalog identity.");
            string magic = Source("Blueprints", "MagicFirearmBlueprints.cs");
            foreach (string token in new[] {
                "PistolPlus1Symbol = \"KMG.Firearms.PistolPlus1Item\"",
                "MusketPlus1Symbol = \"KMG.Firearms.MusketPlus1Item\"",
                "BlunderbussPlus1Symbol = \"KMG.Firearms.BlunderbussPlus1Item\"" })
                Assertions.True(magic.Contains(token),
                    "Canonical firearm +1 symbol drifted: " + token);
        }

        internal static void ManifestAppendsExactlyTheNewIdentities()
        {
            JToken[] entries = JObject.Parse(Read("blueprints", "blueprints.json"))
                ["entries"].ToArray();
            Assertions.Equal(PreservedManifestEntries + 43, entries.Length,
                "Manifest must be the preserved ledger plus 43 progression identities.");
            string prefix = string.Concat(entries.Take(PreservedManifestEntries)
                .Select(value => string.Join("|", new[] {
                    (string)value["symbol"], (string)value["guid"],
                    (string)value["plannedType"], (string)value["status"],
                    (string)value["milestone"], (string)value["notes"] }) + "\n"));
            Assertions.Equal(ManifestPrefixSha256, Sha256(prefix),
                "The preserved 1913-entry manifest prefix changed.");
            Assertions.Equal(entries.Length, entries.Select(value =>
                (string)value["guid"]).Distinct(StringComparer.Ordinal).Count(),
                "Manifest GUID collision.");
            Assertions.Equal(entries.Length, entries.Select(value =>
                (string)value["symbol"]).Distinct(StringComparer.Ordinal).Count(),
                "Manifest symbol collision.");
            JToken[] appended = entries.Skip(PreservedManifestEntries).ToArray();
            string[] expectedNew = ProgressionWeaponCatalog.All.Where(value =>
                    !value.ReusesCanonicalItem).Select(value => value.Symbol + "|" +
                    value.Guid).OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] actualNew = appended.Select(value => (string)value["symbol"] +
                    "|" + (string)value["guid"]).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            Assertions.True(expectedNew.SequenceEqual(actualNew),
                "Appended identities are not exactly the new progression variants.");
            Assertions.True(appended.All(value =>
                    (string)value["plannedType"] == "BlueprintItemWeapon" &&
                    (string)value["status"] == "active" &&
                    (string)value["milestone"] == "Better Vendors progression"),
                "Appended identities must be active weapon items of this change.");
            foreach (ProgressionWeaponSpec spec in ProgressionWeaponCatalog.All)
            {
                JToken[] matches = entries.Where(value => (string)value["symbol"] ==
                    spec.Symbol).ToArray();
                Assertions.Equal(1, matches.Length, "Manifest lacks " + spec.Symbol);
                Assertions.Equal(spec.Guid, (string)matches[0]["guid"],
                    "Manifest GUID mismatch for " + spec.Symbol);
                Assertions.Equal("BlueprintItemWeapon",
                    (string)matches[0]["plannedType"], spec.Symbol + " type");
            }
        }

        internal static void PricingFollowsTheCompleteEnchantmentPackage()
        {
            foreach (ProgressionWeaponSpec spec in ProgressionWeaponCatalog.All)
            {
                int equivalent = spec.ActualEnhancement + (spec.Reliable ? 1 : 0);
                Assertions.Equal(equivalent, spec.EquivalentBonus,
                    spec.Symbol + " equivalent bonus");
                Assertions.Equal(spec.MundaneBaseCost + 300 +
                    2000 * equivalent * equivalent, spec.Cost, spec.Symbol + " price");
                Assertions.True(spec.ActualEnhancement >= 1 &&
                    spec.ActualEnhancement <= 5,
                    spec.Symbol + " actual enhancement exceeds +5.");
            }
            ProgressionWeaponSpec reliableFive = ProgressionWeaponCatalog.RequireSymbol(
                "KMG.Firearms.ReliablePistolPlus5Item");
            Assertions.Equal(5, reliableFive.ActualEnhancement,
                "A +5 Reliable pistol must remain a +5 weapon.");
            Assertions.Equal(73300, reliableFive.Cost,
                "A +5 Reliable pistol is priced as a +6 package.");
            Assertions.Equal(5, reliableFive.UnlockTier,
                "Reliable unlocks at its actual enhancement milestone.");

            // Existing canonical items already follow the same rule.
            Assertions.Equal(3300, Cost("KMG.Firearms.PistolPlus1Item"), "Pistol +1");
            Assertions.Equal(3800, Cost("KMG.Firearms.MusketPlus1Item"), "Musket +1");
            Assertions.Equal(4300, Cost("KMG.Firearms.BlunderbussPlus1Item"),
                "Blunderbuss +1");
            Assertions.Equal(ElvenBranchedSpearCatalog.Require(
                    ElvenBranchedSpearItemKind.PlusOne).Cost,
                Cost("KMG.ElvenBranchedSpear.Plus1Item"), "spear +1");
            foreach (EasternWeaponFamily family in Enum.GetValues(
                typeof(EasternWeaponFamily)))
            {
                EasternWeaponGenericSpec plusOne = EasternWeaponCatalog
                    .RequireGeneric(family, EasternWeaponGenericKind.PlusOne);
                Assertions.Equal(plusOne.Cost, Cost(plusOne.Symbol),
                    family + " +1");
            }
            // Authored Reliable packages: the same complete-package rule.
            Assertions.Equal(19300, Cost("KMG.Firearms.ReliablePistolPlus2Item"),
                "Reliable Pistol +2 matches Duelist's Rebuttal's package.");
            Assertions.Equal(MidgameFirearmCatalog.Roadwarden.Cost,
                Cost("KMG.Firearms.ReliableMusketPlus3Item"),
                "Reliable Musket +3 matches Roadwarden's package.");
            Assertions.Equal(51800, Cost("KMG.Firearms.ReliableMusketPlus4Item"),
                "Reliable Musket +4 matches The River King's Measure.");
            Assertions.Equal(52300, Cost("KMG.Firearms.ReliableBlunderbussPlus4Item"),
                "Reliable Blunderbuss +4 matches Irovetti's Ovation.");
            string magic = Source("Blueprints", "MagicFirearmBlueprints.cs");
            foreach (string token in new[] {
                "\"Duelist's Rebuttal\", FirearmKind.Pistol, 19300, 3, true",
                "\"The River King's Measure\", FirearmKind.Musket, 51800, 5, true",
                "\"Irovetti's Ovation\", FirearmKind.Blunderbuss, 52300, 5, true",
                "\"Pistol +1\", FirearmKind.Pistol, 3300, 1, false",
                "\"Musket +1\", FirearmKind.Musket, 3800, 1, false",
                "\"Blunderbuss +1\", FirearmKind.Blunderbuss, 4300, 1, false" })
                Assertions.True(magic.Contains(token),
                    "Authored reference price changed: " + token);

            Assertions.Equal(EasternWeaponCatalog.MasterworkPremium,
                MagicWeaponPricing.MasterworkPremium, "masterwork premium");
            Assertions.Equal(EasternWeaponCatalog.PlusOneMagicPremium,
                MagicWeaponPricing.EnhancementPriceFactor, "enhancement factor");
            Assertions.Equal(CraftMagicItemsCompatibilityPolicy.ReliableEquivalentBonus,
                ProgressionWeaponCatalog.ReliableEquivalentBonus,
                "Reliable equivalent bonus diverged from the canonical policy.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                MagicWeaponPricing.Cost(-1, 1), "negative base");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                MagicWeaponPricing.Cost(100, 0), "zero bonus");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                MagicWeaponPricing.Cost(100, 11), "bonus above +10");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                MagicWeaponPricing.EquivalentBonus(6, 0), "actual above +5");
            Assertions.Equal(ProductionFirearmCatalog.CreatePistol().CostGold,
                ProgressionWeaponCatalog.RequireSymbol(
                    "KMG.Firearms.PistolPlus3Item").MundaneBaseCost,
                "The mundane base must come from the canonical family.");
        }

        internal static void ExcludedVariantsAndFamiliesAreAbsent()
        {
            string[] forbidden = { "Rifle", "Revolver", "ColdIron", "Battered",
                "Test", "Duelist", "RiverKing", "Irovetti", "LastWord", "WatchAt",
                "Roadwarden", "DeadReckoning", "Corrosive", "Flaming", "Frost",
                "Shock", "Seeking", "Greater", "Masterwork", "Diagnostic" };
            foreach (ProgressionWeaponSpec spec in ProgressionWeaponCatalog.All)
            {
                Assertions.False(forbidden.Any(token => spec.Symbol.IndexOf(token,
                        StringComparison.OrdinalIgnoreCase) >= 0),
                    "Excluded variant entered the catalog: " + spec.Symbol);
                Assertions.True(spec.Symbol.EndsWith("Plus" +
                        spec.ActualEnhancement + "Item", StringComparison.Ordinal),
                    "Only generic enhancement variants are authorized: " + spec.Symbol);
                Assertions.True(!spec.Reliable || spec.IsFirearm,
                    "Reliable must stay firearm-only: " + spec.Symbol);
            }
            string[] namedEastern = { "PaperLantern", "QuietCurrent",
                "FallingPetal", "FoxfireWhisper", "EmptySleeve", "NightWithoutMoon",
                "WayfarersOath", "WinterReed", "DrawnHorizon", "ThunderAtTheGate",
                "MoonlitCrossing", "HeavensMeasure", "BorderSentinel",
                "CloudCleaver", "StormOverStone", "MountainSunder", "UnfixedForm",
                "WorldTreeSeverer", "Boughkeeper", "Thornstep", "MoonlitFork",
                "VipersReach", "BriarCrownedSpear", "SpearOfTheFirstBranch" };
            Assertions.False(ProgressionWeaponCatalog.All.Any(spec =>
                    namedEastern.Any(token => spec.Symbol.Contains(token))),
                "A named weapon entered the generic progression catalog.");
            Assertions.Throws<ArgumentException>(() => new ProgressionWeaponSpec(
                "KMG.EasternWeapons.Katana.ReliablePlus1Item",
                "0123456789abcdef0123456789abcdef", "internal", "display",
                ProgressionWeaponFamily.Katana, 1, true, 50, false),
                "Reliable melee weapons must be rejected.");
        }

        internal static void ModuleOwnershipIsExplicit()
        {
            foreach (ProgressionWeaponSpec spec in ProgressionWeaponCatalog.All)
            {
                ProgressionContentModule expected = spec.IsFirearm
                    ? ProgressionContentModule.Gunslinger
                    : spec.Family == ProgressionWeaponFamily.ElvenBranchedSpear
                    ? ProgressionContentModule.ElvenBranchedSpears
                    : ProgressionContentModule.EasternWeapons;
                Assertions.Equal(expected, spec.Module, spec.Symbol + " module");
            }
            Assertions.Equal(30, ProgressionWeaponCatalog.All.Count(value =>
                value.Module == ProgressionContentModule.Gunslinger), "Gunslinger");
            Assertions.Equal(15, ProgressionWeaponCatalog.All.Count(value =>
                value.Module == ProgressionContentModule.EasternWeapons), "Eastern");
            Assertions.Equal(5, ProgressionWeaponCatalog.All.Count(value =>
                value.Module == ProgressionContentModule.ElvenBranchedSpears), "Spear");
        }

        internal static void VisualMappingsShareTheCanonicalFamilyVariant()
        {
            foreach (ProgressionWeaponSpec spec in ProgressionWeaponCatalog.All)
            {
                ProgressionWeaponSpec plusOne = ProgressionWeaponCatalog.All.Single(
                    value => value.Family == spec.Family && value.ReusesCanonicalItem);
                string variant;
                Assertions.True(WeaponVisualVariantCatalog.TryGet(spec.Symbol,
                    out variant), spec.Symbol + " lacks a visual mapping.");
                Assertions.Equal(WeaponVisualVariantCatalog.Require(plusOne.Symbol),
                    variant, spec.Symbol + " must share its +1 family visual.");
            }
        }

        internal static void MachineReadableCatalogMatchesTheCode()
        {
            string expected = WriteCatalogManifestJson();
            string actual = Read(CatalogManifestPath.Split('/'));
            Assertions.Equal(expected.Replace("\r\n", "\n"),
                actual.Replace("\r\n", "\n"),
                "docs/better-vendors-progression-catalog.json is stale; regenerate it with --write-better-vendors-catalog.");
            JObject document = JObject.Parse(actual);
            Assertions.Equal(50, document["entries"].Count(), "manifest entries");
            Assertions.Equal(7, document["entries"].Count(value =>
                (string)value["status"] == "reused"), "manifest reused");
            Assertions.Equal(43, document["entries"].Count(value =>
                (string)value["status"] == "new"), "manifest new");
        }

        internal static string WriteCatalogManifestJson()
        {
            var document = new JObject
            {
                ["schemaVersion"] = 1,
                ["description"] = "Authorized Better Vendors progression catalog; generated from ProgressionWeaponCatalog.",
                ["betterVendors"] = new JObject
                {
                    ["modId"] = BetterVendorsContract.ModId,
                    ["verifiedModVersion"] = BetterVendorsContract.VerifiedModVersion,
                    ["verifiedAssemblyVersion"] = BetterVendorsContract.VerifiedAssemblyVersion,
                    ["verifiedModuleVersionId"] = BetterVendorsContract.VerifiedModuleVersionId,
                    ["verifiedFileSha256"] = BetterVendorsContract.VerifiedFileSha256,
                    ["militaryDestinationTableGuid"] = BetterVendorsContract.MilitaryDestinationTableGuid
                },
                ["schedule"] = new JArray(BetterVendorsProgressionSchedule.All.Select(
                    tier => new JObject
                    {
                        ["tier"] = tier.Tier,
                        ["militaryRank"] = tier.MilitaryRank,
                        ["quantityAddedPerStockEvent"] = tier.Quantity
                    })),
                ["entries"] = new JArray(ProgressionWeaponCatalog.All.Select(spec =>
                {
                    BetterVendorsProgressionTier tier = BetterVendorsProgressionSchedule
                        .RequireTier(spec.UnlockTier);
                    return new JObject
                    {
                        ["key"] = spec.Key,
                        ["symbol"] = spec.Symbol,
                        ["guid"] = spec.Guid,
                        ["status"] = spec.ReusesCanonicalItem ? "reused" : "new",
                        ["displayName"] = spec.DisplayName,
                        ["internalName"] = spec.InternalName,
                        ["family"] = spec.Family.ToString(),
                        ["module"] = spec.Module.ToString(),
                        ["actualEnhancement"] = spec.ActualEnhancement,
                        ["reliable"] = spec.Reliable,
                        ["equivalentBonus"] = spec.EquivalentBonus,
                        ["progressionEligible"] = spec.ProgressionEligible,
                        ["mundaneBaseCost"] = spec.MundaneBaseCost,
                        ["baseItemCost"] = spec.Cost,
                        ["unlockMilitaryRank"] = tier.MilitaryRank,
                        ["quantityAddedPerStockEvent"] = tier.Quantity,
                        ["visualVariant"] = WeaponVisualVariantCatalog.Require(spec.Symbol)
                    };
                }))
            };
            return document.ToString(Formatting.Indented).Replace("\r\n", "\n") + "\n";
        }

        // ---------------------------------------------------------------
        // C. Verified progression schedule
        // ---------------------------------------------------------------

        internal static void ScheduleMatchesTheVerifiedMilitaryRanks()
        {
            var expected = new Dictionary<int, int[]>
            {
                { 1, new[] { 1, 5 } }, { 3, new[] { 2, 5 } }, { 5, new[] { 3, 5 } },
                { 7, new[] { 4, 2 } }, { 9, new[] { 5, 2 } }
            };
            for (int rank = -1; rank <= 11; rank++)
            {
                BetterVendorsProgressionTier tier;
                bool found = BetterVendorsProgressionSchedule
                    .TryGetTierForMilitaryStockRank(rank, out tier);
                Assertions.Equal(expected.ContainsKey(rank), found,
                    "Military rank " + rank + " tier presence");
                if (!found) continue;
                Assertions.Equal(expected[rank][0], tier.Tier, "rank " + rank + " tier");
                Assertions.Equal(expected[rank][1], tier.Quantity,
                    "rank " + rank + " quantity");
            }
            var unlocked = new Dictionary<int, int[]>
            {
                { -1, new int[0] }, { 0, new int[0] }, { 1, new[] { 1 } },
                { 2, new[] { 1 } }, { 3, new[] { 1, 2 } }, { 4, new[] { 1, 2 } },
                { 5, new[] { 1, 2, 3 } }, { 6, new[] { 1, 2, 3 } },
                { 7, new[] { 1, 2, 3, 4 } }, { 8, new[] { 1, 2, 3, 4 } },
                { 9, new[] { 1, 2, 3, 4, 5 } }, { 10, new[] { 1, 2, 3, 4, 5 } }
            };
            foreach (KeyValuePair<int, int[]> pair in unlocked)
                Assertions.True(BetterVendorsProgressionSchedule.UnlockedTiers(pair.Key)
                        .Select(value => value.Tier).SequenceEqual(pair.Value),
                    "Unlocked tiers at Military rank " + pair.Key);
            for (int tier = 1; tier <= 5; tier++)
                Assertions.Equal(BetterVendorsContract.EnhancementGuid(tier),
                    new[] { "d42fc23b92c640846ac137dc26e000d4",
                        "eb2faccc4c9487d43b3575d7e77ff3f5",
                        "80bb8a737579e35498177e1e3c75899b",
                        "783d7d496da6ac44f9511011fc5f1979",
                        "bdba267e951851449af552aa9f9e3992" }[tier - 1],
                    "native enhancement GUID +" + tier);
        }

        // ---------------------------------------------------------------
        // D. Pass scoping, classification and narrow observation
        // ---------------------------------------------------------------

        internal static void CatchUpPassClassifiesEveryCallAsCatchUp()
        {
            var owner = new object();
            BetterVendorsStockPass.EndPass();
            var kinds = new List<BetterVendorsStockCallKind>();
            for (int rank = 0; rank <= 5; rank++)
            {
                BetterVendorsStockCall call = BetterVendorsStockPass.BeginMilitaryCall(
                    owner, rank, 5);
                kinds.Add(call.Kind);
                Assertions.True(BetterVendorsStockPass.CompleteMilitaryCall(call),
                    "complete");
            }
            BetterVendorsStockPass.EndPass();
            Assertions.True(kinds.All(value => value == BetterVendorsStockCallKind.CatchUp),
                "Every call of a first-time Better Vendors catch-up pass must be ledger-gated.");
        }

        internal static void SingleCurrentRankCallIsBetterVendorsReplenishment()
        {
            BetterVendorsStockPass.EndPass();
            BetterVendorsStockCall call = BetterVendorsStockPass.BeginMilitaryCall(
                new object(), 5, 5);
            Assertions.Equal(BetterVendorsStockCallKind.CurrentTier, call.Kind,
                "The first current-rank call is Better Vendors' own current-tier event.");
            Assertions.Equal(3, call.Tier.Tier, "rank 5 stocks tier 3");
            BetterVendorsStockPass.EndPass();
            BetterVendorsStockCall even = BetterVendorsStockPass.BeginMilitaryCall(
                new object(), 6, 6);
            Assertions.True(even.Tier == null,
                "Military VI is elemental-only and never supplemented.");
            BetterVendorsStockPass.EndPass();
        }

        internal static void UnsupportedCallsNeverStock()
        {
            Assertions.Equal(BetterVendorsStockCallKind.Unsupported,
                BetterVendorsStockPass.Classify(6, 5, 0), "future rank");
            Assertions.Equal(BetterVendorsStockCallKind.Unsupported,
                BetterVendorsStockPass.Classify(-1, 5, 0), "negative rank");
            Assertions.Equal(BetterVendorsStockCallKind.Unsupported,
                BetterVendorsStockPass.Classify(1, -1, 0), "no kingdom");
            Assertions.Equal(BetterVendorsStockCallKind.Unsupported,
                BetterVendorsStockPass.Classify(11, 11, 0), "rank above ten");
            var call = new BetterVendorsStockCall(7, 5, 0,
                BetterVendorsStockCallKind.Unsupported);
            Assertions.True(call.Tier == null, "Unsupported calls carry no tier.");
            Assertions.True(BetterVendorsGrantPlanner.PlanStockCall(
                    ProgressionWeaponCatalog.All, AllModules(), value => false, call)
                .IsEmpty, "Unsupported calls must never stock.");
        }

        internal static void PassScopeUnwindsAfterAFailedCall()
        {
            var owner = new object();
            BetterVendorsStockPass.EndPass();
            BetterVendorsStockCall failed = BetterVendorsStockPass.BeginMilitaryCall(
                owner, 0, 5);
            // The original threw: no postfix, the call stays open until the
            // AddStock postfix (which the verified AddStock always reaches).
            Assertions.True(ReferenceEquals(failed,
                BetterVendorsStockPass.OpenCall), "open call");
            BetterVendorsStockPass.EndPass();
            Assertions.True(BetterVendorsStockPass.OpenCall == null &&
                !BetterVendorsStockPass.HasOpenPass,
                "The AddStock postfix must unwind the pass and its open call.");
            Assertions.False(BetterVendorsStockPass.CompleteMilitaryCall(failed),
                "A stale call can never be completed later.");
            BetterVendorsStockCall next = BetterVendorsStockPass.BeginMilitaryCall(
                owner, 5, 5);
            Assertions.Equal(0, next.Sequence, "A later pass starts fresh.");
            Assertions.Equal(BetterVendorsStockCallKind.CurrentTier, next.Kind,
                "A later replenishment is not misclassified by a failed pass.");
            BetterVendorsStockPass.EndPass();
        }

        internal static void PassIsBoundToOneCampaignAndThread()
        {
            BetterVendorsStockPass.EndPass();
            var first = new object();
            var second = new object();
            BetterVendorsStockCall a = BetterVendorsStockPass.BeginMilitaryCall(
                first, 0, 3);
            BetterVendorsStockCall b = BetterVendorsStockPass.BeginMilitaryCall(
                second, 3, 3);
            Assertions.Equal(0, a.Sequence, "first campaign sequence");
            Assertions.Equal(0, b.Sequence,
                "A different campaign owner never inherits another pass.");
            Assertions.Equal(BetterVendorsStockCallKind.CurrentTier, b.Kind,
                "The new owner's first current-rank call is current-tier.");
            BetterVendorsStockCall observedElsewhere = null;
            var thread = new Thread(() =>
            {
                observedElsewhere = BetterVendorsStockPass.OpenCall;
            });
            thread.Start();
            thread.Join();
            Assertions.True(observedElsewhere == null,
                "Pass scope must be thread-local.");
            BetterVendorsStockPass.EndPass();
        }

        internal static void OnlyTheOrdinaryTierQueryIsObserved()
        {
            var call = new BetterVendorsStockCall(5, 5, 0,
                BetterVendorsStockCallKind.CurrentTier);
            string plus3 = BetterVendorsContract.EnhancementGuid(3);
            var generic = new List<string> { "c3209eb058d471548928a200d70765e0" };
            var corrosive = new List<string> { "633b38ff1d11de64a91d490c683ab1c8" };
            Assertions.True(BetterVendorsStockPass.IsOrdinaryTierQuery(call,
                new List<string> { plus3 }, null, null, true, true), "ordinary +3");
            Assertions.False(BetterVendorsStockPass.IsOrdinaryTierQuery(call,
                new List<string> { plus3 }, generic, null, true, true),
                "composite/thrown/oversized query");
            Assertions.False(BetterVendorsStockPass.IsOrdinaryTierQuery(call,
                new List<string> { plus3 }, corrosive, null, true, true),
                "elemental query");
            Assertions.False(BetterVendorsStockPass.IsOrdinaryTierQuery(call,
                new List<string> { plus3 }, corrosive, generic, false, true),
                "bow-specific query");
            Assertions.False(BetterVendorsStockPass.IsOrdinaryTierQuery(call,
                new List<string> { plus3 }, null, null, false, true),
                "category-specific query");
            Assertions.False(BetterVendorsStockPass.IsOrdinaryTierQuery(call,
                new List<string> { plus3 }, null, null, true, false),
                "unique-by-name disabled query");
            Assertions.False(BetterVendorsStockPass.IsOrdinaryTierQuery(call,
                new List<string> { BetterVendorsContract.EnhancementGuid(2) },
                null, null, true, true), "another tier's query");
            Assertions.False(BetterVendorsStockPass.IsOrdinaryTierQuery(call,
                new List<string> { plus3, plus3 }, null, null, true, true),
                "multi-GUID query");
            Assertions.False(BetterVendorsStockPass.IsOrdinaryTierQuery(null,
                new List<string> { plus3 }, null, null, true, true),
                "manual query outside a stock call");
            Assertions.False(BetterVendorsStockPass.IsOrdinaryTierQuery(
                new BetterVendorsStockCall(6, 6, 0,
                    BetterVendorsStockCallKind.CurrentTier),
                new List<string> { BetterVendorsContract.EnhancementGuid(1) },
                corrosive, null, true, true), "Military VI corrosive query");
        }

        // ---------------------------------------------------------------
        // B/E. Grant planning, modules, ledger and duplication
        // ---------------------------------------------------------------

        internal static void CurrentTierEventAddsAndReplenishesItsTier()
        {
            var call = new BetterVendorsStockCall(5, 5, 0,
                BetterVendorsStockCallKind.CurrentTier);
            BetterVendorsGrantPlan first = BetterVendorsGrantPlanner.PlanStockCall(
                ProgressionWeaponCatalog.All, AllModules(), value => false, call);
            Assertions.Equal(10, first.Grants.Length,
                "Tier +3: three ordinary firearms, three Reliable firearms, four melee.");
            Assertions.True(first.Grants.All(value => value.Quantity == 5 &&
                    value.Spec.ActualEnhancement == 3 &&
                    value.Reason == BetterVendorsGrantReason.InitialGrant),
                "Military V adds five of each +3 entry.");
            Assertions.Equal(3, first.Grants.Count(value => value.Spec.Reliable),
                "Reliable +3 firearms unlock with the ordinary +3 tier.");
            BetterVendorsGrantPlan again = BetterVendorsGrantPlanner.PlanStockCall(
                ProgressionWeaponCatalog.All, AllModules(), value => true, call);
            Assertions.Equal(10, again.Grants.Length,
                "A later Better Vendors current-tier event replenishes like Better Vendors.");
            Assertions.True(again.Grants.All(value =>
                    value.Reason == BetterVendorsGrantReason.Replenishment),
                "Already granted entries are replenishment, not new grants.");
        }

        internal static void CatchUpCallsNeverRegrantRecordedEntries()
        {
            ProgressionWeaponSpec[] tierTwo = ProgressionWeaponCatalog.ForTier(2);
            var granted = new HashSet<string>(tierTwo.Take(4).Select(value =>
                value.Guid), StringComparer.Ordinal);
            var call = new BetterVendorsStockCall(3, 5, 3,
                BetterVendorsStockCallKind.CatchUp);
            BetterVendorsGrantPlan plan = BetterVendorsGrantPlanner.PlanStockCall(
                ProgressionWeaponCatalog.All, AllModules(), granted.Contains, call);
            Assertions.Equal(tierTwo.Length - 4, plan.Grants.Length,
                "Catch-up calls grant only missing entries.");
            Assertions.False(plan.Grants.Any(value => granted.Contains(
                value.Spec.Guid)), "A recorded entry was regranted.");
            Assertions.True(plan.Grants.All(value => value.Quantity == 5 &&
                value.Spec.ActualEnhancement == 2), "Tier +2 quantity.");
        }

        internal static void BetterVendorsOwnSelectionIsNeverDuplicated()
        {
            ProgressionWeaponSpec katana = ProgressionWeaponCatalog.RequireSymbol(
                "KMG.EasternWeapons.Katana.Plus2Item");
            var call = new BetterVendorsStockCall(3, 3, 0,
                BetterVendorsStockCallKind.CurrentTier);
            call.ObserveOrdinaryQuery(new[] { katana.Guid, "not-ours" });
            BetterVendorsGrantPlan plan = BetterVendorsGrantPlanner.PlanStockCall(
                ProgressionWeaponCatalog.All, AllModules(), value => false, call);
            Assertions.True(plan.NativelyDelivered.Single() == katana,
                "An entry Better Vendors already selected is recorded, not added.");
            Assertions.False(plan.Grants.Any(value => value.Spec == katana),
                "An entry Better Vendors already selected was added twice.");
            Assertions.Equal(ProgressionWeaponCatalog.ForTier(2).Length - 1,
                plan.Grants.Length, "Other entries still receive their grant.");
            Assertions.True(call.OrdinaryQueryObserved, "observation recorded");
        }

        internal static void EachContentModuleIsIndependent()
        {
            var call = new BetterVendorsStockCall(1, 1, 0,
                BetterVendorsStockCallKind.CurrentTier);
            foreach (ProgressionContentModule disabled in Enum.GetValues(
                typeof(ProgressionContentModule)))
            {
                var modules = new ProgressionModuleState(
                    disabled != ProgressionContentModule.Gunslinger,
                    disabled != ProgressionContentModule.EasternWeapons,
                    disabled != ProgressionContentModule.ElvenBranchedSpears);
                BetterVendorsGrantPlan plan = BetterVendorsGrantPlanner.PlanStockCall(
                    ProgressionWeaponCatalog.All, modules, value => false, call);
                Assertions.False(plan.Grants.Any(value => value.Spec.Module ==
                    disabled), disabled + " gained stock while disabled.");
                Assertions.True(plan.ModuleSuppressed.All(value => value.Module ==
                        disabled) && plan.ModuleSuppressed.Length > 0,
                    disabled + " suppression must be exact.");
                Assertions.Equal(ProgressionWeaponCatalog.ForTier(1).Count(value =>
                        value.Module != disabled), plan.Grants.Length,
                    disabled + " disabled must not affect other modules.");
                BetterVendorsGrantPlan catchUp = BetterVendorsGrantPlanner.PlanCatchUp(
                    ProgressionWeaponCatalog.All, modules, value => false, true, 9);
                Assertions.False(catchUp.Grants.Any(value => value.Spec.Module ==
                    disabled), disabled + " caught up while disabled.");
            }
            Assertions.True(BetterVendorsGrantPlanner.PlanCatchUp(
                    ProgressionWeaponCatalog.All,
                    new ProgressionModuleState(false, false, false),
                    value => false, true, 10).Grants.Length == 0,
                "All modules off grants nothing.");
        }

        internal static void CatchUpGrantsOnlyReachedMilestones()
        {
            var expectedByRank = new Dictionary<int, int>
            {
                { 0, 0 }, { 1, 10 }, { 2, 10 }, { 3, 20 }, { 4, 20 }, { 5, 30 },
                { 6, 30 }, { 7, 40 }, { 8, 40 }, { 9, 50 }, { 10, 50 }
            };
            foreach (KeyValuePair<int, int> pair in expectedByRank)
            {
                BetterVendorsGrantPlan plan = BetterVendorsGrantPlanner.PlanCatchUp(
                    ProgressionWeaponCatalog.All, AllModules(), value => false,
                    true, pair.Key);
                Assertions.Equal(pair.Value, plan.Grants.Length,
                    "Catch-up at Military " + pair.Key);
                Assertions.True(plan.Grants.All(value => value.Tier.MilitaryRank <=
                        pair.Key && value.Quantity == value.Tier.Quantity),
                    "Catch-up granted a future tier at Military " + pair.Key);
            }
            BetterVendorsGrantPlan five = BetterVendorsGrantPlanner.PlanCatchUp(
                ProgressionWeaponCatalog.All, AllModules(), value => false, true, 5);
            Assertions.True(five.Grants.Select(value => value.Spec.ActualEnhancement)
                    .Distinct().OrderBy(value => value).SequenceEqual(new[] { 1, 2, 3 }),
                "Military V catches up +1, +2 and +3 only.");
            BetterVendorsGrantPlan seven = BetterVendorsGrantPlanner.PlanCatchUp(
                ProgressionWeaponCatalog.All, AllModules(), value => false, true, 7);
            Assertions.True(seven.Grants.Where(value =>
                    value.Spec.ActualEnhancement == 4).All(value => value.Quantity == 2),
                "Military VII makes +4 eligible with its two-copy quantity.");
            Assertions.True(BetterVendorsGrantPlanner.PlanCatchUp(
                    ProgressionWeaponCatalog.All, AllModules(), value => false,
                    false, 10).IsEmpty, "No kingdom means no progression grant.");
            Assertions.True(BetterVendorsGrantPlanner.PlanCatchUp(
                    ProgressionWeaponCatalog.All, AllModules(), value => true,
                    true, 10).IsEmpty,
                "A fully recorded ledger grants nothing even if every copy was bought.");
        }

        internal static void CatalogExpansionAndModuleReenablePreserveBookkeeping()
        {
            // An earlier catalog version granted everything except the Katana
            // family and was run with Eastern Weapons disabled.
            var backing = new List<string>();
            var ledger = new ProgressionGrantLedger(backing);
            foreach (ProgressionWeaponSpec spec in ProgressionWeaponCatalog.All.Where(
                value => value.Family != ProgressionWeaponFamily.Katana &&
                    value.Module != ProgressionContentModule.EasternWeapons &&
                    value.UnlockTier <= 3))
                ledger.Record(spec.Guid);
            backing.Add("ffffffffffffffffffffffffffffffff");
            BetterVendorsGrantPlan plan = BetterVendorsGrantPlanner.PlanCatchUp(
                ProgressionWeaponCatalog.All, AllModules(), ledger.Has, true, 5);
            Assertions.True(plan.Grants.All(value => value.Spec.Module ==
                    ProgressionContentModule.EasternWeapons),
                "Re-enabling Eastern Weapons grants only its missing entries.");
            Assertions.Equal(9, plan.Grants.Length,
                "Three Eastern families times three reached tiers.");
            Assertions.True(ledger.Snapshot().Contains(
                    "ffffffffffffffffffffffffffffffff"),
                "Unknown identities recorded by another version are preserved.");
        }

        internal static void LedgerIsSortedIdempotentAndSaveLocal()
        {
            ProgressionWeaponSpec[] specs = ProgressionWeaponCatalog.All;
            var a = new ProgressionGrantLedger(new List<string>());
            var b = new ProgressionGrantLedger(new List<string>());
            Assertions.True(a.Record(specs[9].Guid), "first record");
            Assertions.False(a.Record(specs[9].Guid), "idempotent record");
            Assertions.True(a.Record(specs[0].Guid), "second entry");
            string[] snapshot = a.Snapshot();
            Assertions.True(snapshot.SequenceEqual(snapshot.OrderBy(value => value,
                StringComparer.Ordinal)), "ledger must stay sorted");
            Assertions.Equal(2, a.Count, "ledger count");
            Assertions.Equal(0, b.Count, "Another campaign's ledger is independent.");
            Assertions.False(b.Has(specs[9].Guid),
                "A second campaign must not inherit the first campaign's grants.");
            Assertions.Throws<ArgumentException>(() => a.Record(
                "0123456789abcdef0123456789abcdef"),
                "Only authorized identities can be recorded.");
            string part = Source("Acquisition", "BetterVendors",
                "UnitPartBetterVendorsProgressionGrants.cs");
            Assertions.True(part.Contains(
                    "public sealed class UnitPartBetterVendorsProgressionGrants : UnitPart") &&
                part.Contains("private int m_SchemaVersion = CurrentSchemaVersion;") &&
                part.Contains("private readonly List<string> m_GrantedEntries") &&
                part.Contains("[JsonProperty]"),
                "The persisted ledger contract changed.");
            Assertions.False(part.Contains("stockUpToDate") ||
                part.Contains("FreeformData"),
                "The ledger must never read or reuse Better Vendors' flag.");
        }

        internal static void ApplierRecordsOnlyObservedMutations()
        {
            ProgressionWeaponSpec[] tier = ProgressionWeaponCatalog.ForTier(1);
            BetterVendorsGrant[] grants = tier.Select(spec => new BetterVendorsGrant(
                spec, BetterVendorsProgressionSchedule.RequireTier(1),
                BetterVendorsGrantReason.InitialGrant)).ToArray();
            var shop = new Dictionary<string, int>(StringComparer.Ordinal);
            var recorded = new HashSet<string>(StringComparer.Ordinal);
            string failing = tier[2].Guid;
            string partial = tier[4].Guid;
            BetterVendorsGrantOutcome outcome = BetterVendorsGrantApplier.Apply(
                grants, spec => Count(shop, spec.Guid), (spec, quantity) =>
                {
                    if (spec.Guid == failing)
                        throw new InvalidOperationException("no mutation");
                    if (spec.Guid == partial)
                    {
                        shop[spec.Guid] = Count(shop, spec.Guid) + 2;
                        throw new InvalidOperationException("partial mutation");
                    }
                    shop[spec.Guid] = Count(shop, spec.Guid) + quantity;
                }, recorded.Add);
            Assertions.False(recorded.Contains(failing),
                "An addition that changed nothing must stay unrecorded for retry.");
            Assertions.True(recorded.Contains(partial),
                "A partially applied addition must be recorded, never topped up.");
            Assertions.Equal(1, outcome.Failed, "failed count");
            Assertions.Equal(1, outcome.Partial, "partial count");
            Assertions.Equal(2, outcome.Errors.Count, "errors retained for logging");
            Assertions.Equal(tier.Length - 1, recorded.Count, "recorded count");

            BetterVendorsGrantPlan retry = BetterVendorsGrantPlanner.PlanCatchUp(
                ProgressionWeaponCatalog.All, AllModules(), recorded.Contains,
                true, 1);
            Assertions.Equal(failing, retry.Grants.Single().Spec.Guid,
                "A retry repeats only the failed, unapplied grant.");
            BetterVendorsGrantApplier.Apply(retry.Grants, spec => Count(shop,
                spec.Guid), (spec, quantity) => shop[spec.Guid] = Count(shop,
                    spec.Guid) + quantity, recorded.Add);
            Assertions.Equal(5, Count(shop, failing), "retried grant quantity");
            Assertions.Equal(2, Count(shop, partial), "partial grant was not topped up");
        }

        // ---------------------------------------------------------------
        // E. Lifecycle simulation of the verified Better Vendors contract
        // ---------------------------------------------------------------

        internal static void FreshCampaignFollowsBetterVendorsEvents()
        {
            var campaign = new SimulatedCampaign();
            campaign.Load();
            Assertions.Equal(0, campaign.TotalCopies, "No kingdom, no stock.");
            campaign.FoundKingdom();
            campaign.OpenBlacksmith();
            Assertions.Equal(0, campaign.TotalCopies, "Military 0 grants nothing.");
            campaign.ImproveMilitary();
            Assertions.Equal(10 * 5, campaign.TotalCopies, "Military I adds +1 x5.");
            campaign.Load();
            campaign.OpenBlacksmith();
            campaign.OpenBlacksmith();
            Assertions.Equal(10 * 5, campaign.TotalCopies,
                "Reloading and reopening trade never restock.");
            campaign.ImproveMilitary();
            Assertions.Equal(10 * 5, campaign.TotalCopies, "Military II adds nothing.");
            campaign.ImproveMilitary();
            Assertions.Equal(10 * 5 + 10 * 5, campaign.TotalCopies,
                "Military III adds +2 x5.");
            campaign.ImproveArcane();
            Assertions.Equal(10 * 5 + 10 * 5 * 2, campaign.TotalCopies,
                "Better Vendors replenishes the current Military tier on any stat rank-up.");
            Assertions.Equal(20, campaign.LedgerCount, "Ledger records each entry once.");
        }

        internal static void ExistingCampaignCatchesUpExactlyOnce()
        {
            var campaign = SimulatedCampaign.InitializedBeforeIntegration(5);
            campaign.Load();
            Assertions.Equal(0, campaign.TotalCopies,
                "Better Vendors does not rerun its own catch-up on load.");
            campaign.OpenBlacksmith();
            Assertions.Equal(30 * 5, campaign.TotalCopies,
                "Military V catches up +1, +2 and +3 five each.");
            Assertions.Equal(0, campaign.CopiesAtTier(4), "No future +4 tier.");
            campaign.BuyEverything();
            campaign.OpenBlacksmith();
            campaign.Load();
            campaign.OpenBlacksmith();
            Assertions.Equal(0, campaign.TotalCopies,
                "Bought-out stock is never refilled by the adapter.");
            campaign.ImproveMilitary();
            campaign.ImproveMilitary();
            Assertions.Equal(10 * 2, campaign.TotalCopies,
                "Military VII adds +4 two each.");
        }

        internal static void StockEventThenCatchUpAndCatchUpThenStockEvent()
        {
            var stockFirst = SimulatedCampaign.InitializedBeforeIntegration(6);
            stockFirst.ImproveMilitary();
            Assertions.Equal(10 * 2, stockFirst.TotalCopies,
                "Military VII grants +4 immediately.");
            stockFirst.OpenBlacksmith();
            Assertions.Equal(10 * 2 + 30 * 5, stockFirst.TotalCopies,
                "Catch-up adds only the missing lower tiers.");
            Assertions.Equal(10 * 2, stockFirst.CopiesAtTier(4),
                "The +4 grant was not repeated by catch-up.");

            var catchUpFirst = SimulatedCampaign.InitializedBeforeIntegration(5);
            catchUpFirst.OpenBlacksmith();
            catchUpFirst.ImproveDivine();
            Assertions.Equal(30 * 5 + 10 * 5, catchUpFirst.TotalCopies,
                "A later Better Vendors event replenishes only the current tier.");
        }

        internal static void FirstTimeBetterVendorsCatchUpNeverDuplicatesMigration()
        {
            var campaign = new SimulatedCampaign();
            campaign.FoundKingdom(5);
            campaign.SetBetterVendorsProgression(false);
            campaign.Load();
            campaign.SetBetterVendorsProgression(true);
            campaign.OpenBlacksmith();
            int migrated = campaign.TotalCopies;
            Assertions.Equal(30 * 5, migrated, "migration grants reached tiers");
            campaign.Load();
            Assertions.Equal(migrated, campaign.TotalCopies,
                "Better Vendors' own first catch-up pass must not duplicate recorded grants.");
            Assertions.True(campaign.BetterVendorsCaughtUp,
                "Better Vendors still performs its own catch-up.");
        }

        internal static void DisablingAndSwitchingPreserveBookkeeping()
        {
            var first = SimulatedCampaign.InitializedBeforeIntegration(3);
            var second = SimulatedCampaign.InitializedBeforeIntegration(9);
            first.OpenBlacksmith();
            second.SetBetterVendorsProgression(false);
            second.OpenBlacksmith();
            Assertions.Equal(0, second.TotalCopies,
                "Progression off: no additions.");
            first.SetBetterVendorsProgression(false);
            first.ImproveMilitary();
            first.ImproveMilitary();
            Assertions.Equal(20 * 5, first.TotalCopies,
                "Disabled progression adds nothing and removes nothing.");
            Assertions.Equal(20, first.LedgerCount, "Disabling keeps the ledger.");
            first.SetBetterVendorsProgression(true);
            first.OpenBlacksmith();
            Assertions.Equal(20 * 5 + 10 * 5, first.TotalCopies,
                "Re-enabling catches up only the missing +3 tier.");
            second.SetBetterVendorsProgression(true);
            second.OpenBlacksmith();
            Assertions.Equal(30 * 5 + 20 * 2, second.TotalCopies,
                "A second campaign keeps its own independent ledger.");
            Assertions.Equal(30, first.LedgerCount, "first ledger");
            Assertions.Equal(50, second.LedgerCount, "second ledger");
        }

        // ---------------------------------------------------------------
        // B. Contract gate and status
        // ---------------------------------------------------------------

        internal static void VerifiedContractIsAccepted()
        {
            BetterVendorsContractObservation observed = VerifiedObservation();
            Assertions.True(BetterVendorsContract.Evaluate(observed).IsCompatible,
                "The verified 2.0.8 observation must be compatible.");
            observed.ModVersion = "2.0.9";
            observed.ModuleVersionId = Guid.Empty.ToString();
            observed.FileSha256 = new string('0', 64);
            Assertions.True(BetterVendorsContract.Evaluate(observed).IsCompatible,
                "Identity labels are diagnostics; behavior fingerprints are the gate.");
        }

        internal static void UnverifiedContractsFailClosed()
        {
            var mutations = new Dictionary<string, Action<BetterVendorsContractObservation>>
            {
                { "assembly-name", value => value.AssemblyName = "BetterVendorsFork" },
                { "required-member", value => value.MissingMember = "ProgressionLogic.AddMilitaryStock(int)" },
                { "military-destination", value => value.MilitaryDestinationGuid = "afa2c7f292b8e1c4d9c835f0e8047dd3" },
                { "enhancement-levels", value => value.EnhancementLevelGuids = null },
                { "load-trigger", value => value.LoadPatchTarget = "Kingmaker.Game.LoadGame" },
                { "rank-trigger", value => value.ImproveStatPatchTarget = null }
            };
            foreach (string key in BetterVendorsContract.FingerprintKeys)
            {
                string captured = key;
                mutations.Add("unverified-behavior:" + key, value =>
                    value.MethodIlSha256[captured] = new string('a', 64));
            }
            mutations.Add("unverified-behavior:" + BetterVendorsContract.AddMilitaryStockKey + ":absent",
                value => value.MethodIlSha256.Remove(BetterVendorsContract.AddMilitaryStockKey));
            foreach (KeyValuePair<string, Action<BetterVendorsContractObservation>> mutation
                in mutations)
            {
                BetterVendorsContractObservation observed = VerifiedObservation();
                mutation.Value(observed);
                BetterVendorsContractDecision decision =
                    BetterVendorsContract.Evaluate(observed);
                Assertions.False(decision.IsCompatible,
                    "Mutation must fail closed: " + mutation.Key);
                Assertions.True(mutation.Key.StartsWith(decision.FailedCheck,
                        StringComparison.Ordinal),
                    "Wrong failed check for " + mutation.Key + ": " +
                    decision.FailedCheck);
            }
            Assertions.False(BetterVendorsContract.Evaluate(null).IsCompatible,
                "A missing observation fails closed.");
        }

        internal static void StatusChangesAreReportedOnce()
        {
            BetterVendorsIntegrationStatusRegistry.ResetForTests();
            var ready = new BetterVendorsIntegrationStatus(
                BetterVendorsIntegrationAvailability.Ready, "verified");
            Assertions.True(BetterVendorsIntegrationStatusRegistry.Update(ready),
                "first transition");
            Assertions.False(BetterVendorsIntegrationStatusRegistry.Update(
                new BetterVendorsIntegrationStatus(
                    BetterVendorsIntegrationAvailability.Ready, "verified")),
                "A repeated identical status must not log again.");
            Assertions.True(BetterVendorsIntegrationStatusRegistry.Update(
                new BetterVendorsIntegrationStatus(
                    BetterVendorsIntegrationAvailability.Incompatible, "changed")),
                "A real change is reported.");
            BetterVendorsIntegrationStatusRegistry.ResetForTests();
        }

        // ---------------------------------------------------------------
        // F/G. Source contracts: narrow hooks and unchanged acquisition
        // ---------------------------------------------------------------

        internal static void AdapterHooksAreNarrowAndReadOnly()
        {
            string coordinator = Source("Acquisition", "BetterVendors",
                "BetterVendorsCompatibilityCoordinator.cs");
            string runtime = Source("Acquisition", "BetterVendors",
                "BetterVendorsStockRuntime.cs");
            string all = coordinator + runtime + Source("Acquisition",
                "BetterVendors", "BetterVendorsStockPass.cs") + Source(
                "Acquisition", "BetterVendors", "BetterVendorsGrantPlanner.cs");
            Assertions.Equal(4, CountOccurrences(coordinator, "harmony.Patch("),
                "Exactly four hooks may be installed.");
            foreach (string token in new[] { "contract.AddStock, null",
                "contract.AddMilitaryStock,", "contract.GetFilterWeapons, null",
                "harmony.Patch(beginTrading, null" })
                Assertions.True(coordinator.Contains(token), "Missing hook " + token);
            Assertions.True(coordinator.Contains("BetterVendorsContract.Evaluate") &&
                    coordinator.IndexOf("BetterVendorsContract.Evaluate",
                        StringComparison.Ordinal) < coordinator.IndexOf(
                        "InstallContractPatches(contract);", StringComparison.Ordinal),
                "Hooks must be installed only after the contract is verified.");
            Assertions.True(coordinator.Contains("harmony.UnpatchAll(HarmonyOwner)") &&
                coordinator.Contains("harmony.UnpatchAll(TradingHarmonyOwner)"),
                "Failed hook installation must be rolled back.");
            foreach (string forbidden in new[] { "stockUpToDate", "FreeformData",
                "__result.Add", "__result.Remove", "__result.Clear",
                "ref List<BlueprintItemWeapon>", ".Remove(", "RemoveAll(",
                "Invoke(null, new object[] { ", "AddStock.Invoke",
                "AddMilitaryStock.Invoke", "ComponentsArray =" })
                Assertions.False(all.Contains(forbidden),
                    "Adapter touched forbidden state or mutated Better Vendors: " +
                    forbidden);
            Assertions.False(System.Text.RegularExpressions.Regex.IsMatch(all,
                    @"__result\s*=[^=]"),
                "The Better Vendors selection result must never be reassigned.");
            foreach (string callback in new[] { "AddMilitaryStockPrefix",
                "AddMilitaryStockPostfix", "GetFilterWeaponsPostfix",
                "BeginTradingPostfix" })
            {
                int start = runtime.IndexOf("internal static void " + callback,
                    StringComparison.Ordinal);
                int end = runtime.IndexOf("internal static", start + 10,
                    StringComparison.Ordinal);
                string body = runtime.Substring(start, end - start);
                Assertions.True(body.Contains("catch (Exception exception)") &&
                        body.Contains("ReportFailure("),
                    callback + " must be fail-soft.");
            }
            Assertions.True(runtime.Contains("player.SharedVendorTables.GetTable(table)") &&
                runtime.Contains("CapitalVendorBlueprints.ExpectedTableName"),
                "Stock must go only to the verified shared blacksmith table.");
            Assertions.True(runtime.Contains("IsMilitaryDestinationVendor(vendorUnit)"),
                "Catch-up must be restricted to the Military destination vendor.");
        }

        internal static void OrdinaryAcquisitionPathsExcludeProgressionVariants()
        {
            string[] paths =
            {
                "CapitalVendorBlueprints.cs", "BeneathStolenLandsVendorBlueprints.cs",
                "EasternWeaponCampaignBlueprints.cs",
                "ElvenBranchedSpearCampaignBlueprints.cs",
                "RareFirearmCampaignLootBlueprints.cs", "SkeletalSalesmanBlueprints.cs",
                "BokkenFirearmSupplyVendorBlueprints.cs",
                "OlegFirearmSupplyCleanupBlueprints.cs"
            };
            foreach (string path in paths)
            {
                string source = Source("Blueprints", path);
                Assertions.False(source.Contains("ProgressionWeapon") ||
                        source.Contains("BetterVendors"),
                    path + " must not publish progression variants.");
            }
            foreach (string path in new[] {
                Path.Combine("Acquisition", "RetiredKitVendorStockCleanup.cs"),
                Path.Combine("CraftMagicItemsCompatibility",
                    "CraftMagicItemsRegistrationCatalog.cs") })
                Assertions.False(Source(path.Split(Path.DirectorySeparatorChar))
                        .Contains("ProgressionWeapon"),
                    path + " must not enumerate progression variants.");
            string registration = Source("Blueprints", "ProgressionWeaponBlueprints.cs");
            foreach (string forbidden in new[] { "LootItemsPackFixed",
                "ComponentsArray =", "BlueprintLoot", "SharedVendorTables",
                "CreateFixedEntry" })
                Assertions.False(registration.Contains(forbidden),
                    "Registration must never publish stock: " + forbidden);
            Assertions.Equal(12, EasternWeaponCatalog.AllGenericItems.Length,
                "Eastern ordinary generic stock must stay the four existing kinds.");
            Assertions.Equal(4, Enum.GetValues(typeof(EasternWeaponGenericKind)).Length,
                "Eastern campaign stock publishes every EasternWeaponGenericKind value.");
            Assertions.Equal(6, ElvenBranchedSpearCatalog.All.Length,
                "Spear ordinary stock must stay the six existing items.");
            string capital = Source("Blueprints", "CapitalVendorBlueprints.cs");
            Assertions.True(capital.Contains("magicFirearms.Require(MagicFirearmBlueprints.PistolPlus1Symbol).Item") &&
                    capital.Contains("magicFirearms.Require(MagicFirearmBlueprints.MusketPlus1Symbol).Item") &&
                    capital.Contains("magicFirearms.Require(MagicFirearmBlueprints.BlunderbussPlus1Symbol).Item"),
                "The baseline capital +1 stock must be preserved.");
            string magic = Source("Blueprints", "MagicFirearmBlueprints.cs");
            Assertions.True(magic.Contains("catalog.Entries.Length != 10"),
                "The authored magic firearm catalog must keep its ten entries.");
        }

        internal static void BootstrapRegistersUnconditionallyAfterIcons()
        {
            string bootstrap = Source("Bootstrap", "BlueprintBootstrap.cs");
            int icons = bootstrap.IndexOf("ProjectAssetIcons.Apply(gunslingerClassBlueprints",
                StringComparison.Ordinal);
            int register = bootstrap.IndexOf("ProgressionWeaponBlueprints.Register(",
                StringComparison.Ordinal);
            int presentation = bootstrap.IndexOf(
                "PlayerFacingPresentation.ApplyArchetypes(", StringComparison.Ordinal);
            Assertions.True(icons > 0 && register > icons && presentation > register,
                "Progression variants must register after the family icons exist.");
            string between = bootstrap.Substring(icons, register - icons);
            Assertions.False(between.Contains("publicationPlan") ||
                    between.Contains("if ("),
                "Registration must not depend on a publication or module switch.");
            Assertions.True(bootstrap.Contains(
                    "ElementalRaceIdentityCatalog.IdentityCount +\n            Acquisition.ProgressionWeaponCatalog.NewBlueprintCount;"),
                "The expected registration count must include exactly the new variants.");
        }

        internal static void ReliableProgressionVariantsUseCanonicalMechanics()
        {
            string registration = Source("Blueprints", "ProgressionWeaponBlueprints.cs");
            Assertions.True(registration.Contains(
                    "BlueprintWeaponEnchantment reliable = magicFirearms.Reliable;") &&
                registration.Contains("Enchantments.ReliableBlueprints.Validate(reliable);") &&
                registration.Contains("new[] { enhancement, reliable }"),
                "Reliable variants must reuse the canonical Reliable enchantment.");
            Assertions.False(registration.Contains("FirearmMisfireReductionComponent"),
                "No vendor-only Reliable implementation may exist.");
            string reliable = Source("Enchantments", "ReliableBlueprints.cs");
            Assertions.True(reliable.Contains("FirearmMisfireReductionComponent.Create(1)") &&
                reliable.Contains("Set(value, \"m_EnchantmentCost\", 1);"),
                "Canonical Reliable must reduce by one and cost +1 equivalent.");
            AmmunitionId loose = ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition;
            int pistolBase = FirearmDefinitions.CreateEarlyPistol().MisfireValue;
            int reduced = EffectiveFirearmMisfireValuePolicy.Evaluate(pistolBase,
                FirearmCondition.Normal, false, loose, 1);
            Assertions.Equal(Math.Max(0, pistolBase - 1), reduced,
                "Reliable lowers the misfire value by one to a minimum of zero.");
            var service = new FirearmMisfireService();
            FirearmMisfireDecision naturalOne = service.Evaluate(1, reduced, false);
            Assertions.False(naturalOne.FinalSuccess,
                "A natural 1 still misses with Reliable.");
            Assertions.False(naturalOne.IsMisfire,
                "At misfire value 0 a natural 1 is a miss, not a misfire.");
            Assertions.True(service.Evaluate(1, pistolBase, true).IsMisfire,
                "Without Reliable the same natural 1 misfires.");
            Assertions.Equal(6, EffectiveFirearmMisfireValuePolicy.Evaluate(2,
                    FirearmCondition.Broken, false,
                    ReloadAmmunitionProfileCatalog.PaperCartridge.LoadedAmmunition, 1),
                "Reliable applies after every other misfire increase.");
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static BetterVendorsContractObservation VerifiedObservation()
        {
            var observed = new BetterVendorsContractObservation
            {
                ModVersion = BetterVendorsContract.VerifiedModVersion,
                AssemblyName = BetterVendorsContract.AssemblyName,
                AssemblyVersion = BetterVendorsContract.VerifiedAssemblyVersion,
                ModuleVersionId = BetterVendorsContract.VerifiedModuleVersionId,
                FileSha256 = BetterVendorsContract.VerifiedFileSha256,
                MilitaryDestinationGuid = BetterVendorsContract.MilitaryDestinationTableGuid,
                EnhancementLevelGuids = Enumerable.Range(1, 5).Select(
                    BetterVendorsContract.EnhancementGuid).ToArray(),
                LoadPatchTarget = BetterVendorsContract.LoadPatchTarget,
                ImproveStatPatchTarget = BetterVendorsContract.ImproveStatPatchTarget
            };
            foreach (string key in BetterVendorsContract.FingerprintKeys)
                observed.MethodIlSha256[key] =
                    BetterVendorsContract.VerifiedIlSha256(key);
            return observed;
        }

        private static ProgressionModuleState AllModules()
        {
            return new ProgressionModuleState(true, true, true);
        }

        private static int Cost(string symbol)
        {
            return ProgressionWeaponCatalog.RequireSymbol(symbol).Cost;
        }

        private static int Count(Dictionary<string, int> shop, string guid)
        {
            int value;
            return shop.TryGetValue(guid, out value) ? value : 0;
        }

        private static int CountOccurrences(string text, string token)
        {
            int count = 0;
            for (int index = text.IndexOf(token, StringComparison.Ordinal);
                index >= 0; index = text.IndexOf(token, index + token.Length,
                    StringComparison.Ordinal))
                count++;
            return count;
        }

        private static string Sha256(string value)
        {
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(
                        Encoding.UTF8.GetBytes(value))).Replace("-", string.Empty)
                    .ToLowerInvariant();
        }

        private static string Source(params string[] parts)
        {
            return Read(new[] { "src", "KingmakerGunslinger" }.Concat(parts)
                .ToArray()).Replace("\r\n", "\n");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }

        /// <summary>
        /// A faithful model of the verified Better Vendors 2.0.8 lifecycle
        /// (Player.PostLoad catch-up gated by its own flag, AddStock on every
        /// Arcane/Divine/Military/Stability rank-up, lower-rank catch-up loop
        /// then the current rank) driving the production pass scope, planner,
        /// applier and trade-open catch-up with a simulated shared table.
        /// </summary>
        private sealed class SimulatedCampaign
        {
            private readonly object _player = new object();
            private readonly Dictionary<string, int> _shop =
                new Dictionary<string, int>(StringComparer.Ordinal);
            private readonly ProgressionGrantLedger _ledger =
                new ProgressionGrantLedger(new List<string>());
            private bool _kingdom;
            private int _military;
            private bool _progression = true;

            internal bool BetterVendorsCaughtUp { get; private set; }

            internal int TotalCopies { get { return _shop.Values.Sum(); } }
            internal int LedgerCount { get { return _ledger.Count; } }

            internal static SimulatedCampaign InitializedBeforeIntegration(int rank)
            {
                var campaign = new SimulatedCampaign();
                campaign._kingdom = true;
                campaign._military = rank;
                // Better Vendors caught this save up before the integration
                // existed: its native items were stocked, ours never were.
                campaign.BetterVendorsCaughtUp = true;
                return campaign;
            }

            internal void FoundKingdom(int rank = 0)
            {
                _kingdom = true;
                _military = rank;
            }

            internal void SetBetterVendorsProgression(bool enabled)
            {
                _progression = enabled;
            }

            internal int CopiesAtTier(int tier)
            {
                return ProgressionWeaponCatalog.ForTier(tier).Sum(spec =>
                    Count(_shop, spec.Guid));
            }

            internal void BuyEverything() { _shop.Clear(); }

            internal void Load()
            {
                // Better Vendors' Player.PostLoad postfix.
                if (!BetterVendorsCaughtUp) AddStock();
            }

            internal void ImproveMilitary()
            {
                if (_military < 10) _military++;
                AddStock();
            }

            internal void ImproveArcane() { AddStock(); }
            internal void ImproveDivine() { AddStock(); }

            internal void OpenBlacksmith()
            {
                if (!_progression || !_kingdom) return;
                BetterVendorsGrantPlan plan = BetterVendorsGrantPlanner.PlanCatchUp(
                    ProgressionWeaponCatalog.All, AllModules(), _ledger.Has,
                    _kingdom, _military);
                Apply(plan.Grants);
            }

            private void AddStock()
            {
                if (!_kingdom || !_progression) return;
                try
                {
                    if (!BetterVendorsCaughtUp)
                    {
                        for (int rank = 0; rank < _military; rank++)
                            MilitaryStock(rank);
                        BetterVendorsCaughtUp = true;
                    }
                    MilitaryStock(_military);
                }
                finally
                {
                    // The production AddStock postfix.
                    BetterVendorsStockPass.EndPass();
                }
            }

            private void MilitaryStock(int rank)
            {
                BetterVendorsStockCall call = BetterVendorsStockPass.BeginMilitaryCall(
                    _player, rank, _military);
                if (!BetterVendorsStockPass.CompleteMilitaryCall(call))
                    throw new InvalidOperationException("call scope lost");
                BetterVendorsGrantPlan plan = BetterVendorsGrantPlanner.PlanStockCall(
                    ProgressionWeaponCatalog.All, AllModules(), _ledger.Has, call);
                Apply(plan.Grants);
            }

            private void Apply(IEnumerable<BetterVendorsGrant> grants)
            {
                BetterVendorsGrantApplier.Apply(grants, spec => Count(_shop,
                    spec.Guid), (spec, quantity) => _shop[spec.Guid] =
                        Count(_shop, spec.Guid) + quantity, _ledger.Record);
            }
        }
    }
}
