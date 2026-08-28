using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.CustomWeapons;
using KingmakerGunslinger.EasternWeapons;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Cryptography;

namespace KingmakerGunslinger.DomainTests
{
    internal static class EasternWeaponFoundationTests
    {
        internal static void LockedCategoryProfilesAreExact()
        {
            CustomWeaponCategoryDefinition[] categories =
                EasternWeaponCatalog.AllCategories;
            Assertions.Equal(3, categories.Length,
                "Eastern Weapons must contain exactly three categories.");
            Assertions.True(categories.Select(value => value.CategoryValue)
                .SequenceEqual(new[] { 0x004b4d48, 0x004b4d49, 0x004b4d4a }),
                "Stable category values changed.");
            Assertions.True(categories.Select(value => value.Presentation.Acronym)
                .SequenceEqual(new[] { "WK", "KA", "NO" }),
                "Category acronyms changed.");

            CustomWeaponCategoryDefinition wakizashi =
                EasternWeaponCatalog.RequireCategory(EasternWeaponFamily.Wakizashi);
            Assertions.True(wakizashi.BaseCost == 35 && wakizashi.WeightPounds == 2 &&
                wakizashi.DamageDiceCount == 1 && wakizashi.DamageDieSides == 6 &&
                wakizashi.CriticalThreatMinimum == 18 &&
                wakizashi.CriticalMultiplier == 2 && wakizashi.Finessable &&
                wakizashi.Handedness == CustomWeaponHandedness.Light &&
                wakizashi.Proficiency == CustomWeaponProficiencyPolicy.Exotic &&
                wakizashi.FighterGroups ==
                    CustomWeaponFighterGroupPolicy.LightBlades &&
                wakizashi.DamageForms == (CustomWeaponDamageForm.Piercing |
                    CustomWeaponDamageForm.Slashing),
                "Wakizashi profile changed.");

            CustomWeaponCategoryDefinition katana =
                EasternWeaponCatalog.RequireCategory(EasternWeaponFamily.Katana);
            Assertions.True(katana.BaseCost == 50 && katana.WeightPounds == 6 &&
                katana.DamageDiceCount == 1 && katana.DamageDieSides == 8 &&
                katana.CriticalThreatMinimum == 18 &&
                katana.CriticalMultiplier == 2 && !katana.Finessable &&
                katana.Handedness == CustomWeaponHandedness.OneHandedVersatile &&
                katana.Proficiency ==
                    CustomWeaponProficiencyPolicy.KatanaGripDependent &&
                katana.FighterGroups ==
                    CustomWeaponFighterGroupPolicy.HeavyBlades &&
                katana.DamageForms == CustomWeaponDamageForm.Slashing,
                "Katana profile changed.");

            CustomWeaponCategoryDefinition nodachi =
                EasternWeaponCatalog.RequireCategory(EasternWeaponFamily.Nodachi);
            Assertions.True(nodachi.BaseCost == 60 && nodachi.WeightPounds == 8 &&
                nodachi.DamageDiceCount == 1 && nodachi.DamageDieSides == 10 &&
                nodachi.CriticalThreatMinimum == 18 &&
                nodachi.CriticalMultiplier == 2 && !nodachi.Finessable &&
                nodachi.Handedness == CustomWeaponHandedness.TwoHanded &&
                nodachi.Proficiency == CustomWeaponProficiencyPolicy.Martial &&
                nodachi.FighterGroups ==
                    (CustomWeaponFighterGroupPolicy.HeavyBlades |
                        CustomWeaponFighterGroupPolicy.Polearms),
                "Nodachi profile changed.");
            Assertions.True(categories.All(value => !value.Reach && !value.Thrown),
                "Eastern weapons must not be reach or thrown weapons.");
        }

        internal static void GenericCatalogIsExact()
        {
            EasternWeaponGenericSpec[] items = EasternWeaponCatalog.AllGenericItems;
            Assertions.Equal(12, items.Length,
                "Eastern generic catalog must contain twelve items.");
            foreach (EasternWeaponFamily family in Enum.GetValues(
                typeof(EasternWeaponFamily)).Cast<EasternWeaponFamily>())
            {
                EasternWeaponGenericSpec[] path = items.Where(value =>
                    value.Family == family).OrderBy(value => value.Kind).ToArray();
                CustomWeaponCategoryDefinition category =
                    EasternWeaponCatalog.RequireCategory(family);
                Assertions.Equal(4, path.Length,
                    "Each Eastern family must contain four generic items.");
                Assertions.True(path.Select(value => value.Cost).SequenceEqual(
                    new[] { category.BaseCost, category.BaseCost + 300,
                        category.BaseCost * 2, category.BaseCost + 2300 }),
                    "Generic tabletop pricing changed for " + family + ".");
                Assertions.True(!path[0].Masterwork && !path[0].ColdIron &&
                    path[0].Enhancement == 0 && path[1].Masterwork &&
                    !path[1].ColdIron && path[1].Enhancement == 0 &&
                    !path[2].Masterwork && path[2].ColdIron &&
                    path[2].Enhancement == 0 && path[3].Masterwork &&
                    !path[3].ColdIron && path[3].Enhancement == 1,
                    "Generic quality construction changed for " + family + ".");
            }
            Assertions.Equal(12, items.Select(value => value.Symbol).Distinct().Count(),
                "Generic symbols are not unique.");
        }

        internal static void RegistryFailsClosedOnCollisions()
        {
            var registry = new CustomWeaponCategoryRegistry();
            foreach (CustomWeaponCategoryDefinition definition in
                EasternWeaponCatalog.AllCategories) registry.Add(definition);
            Assertions.Equal(3, registry.All.Length,
                "Registry lost an Eastern category.");
            Assertions.Throws<InvalidOperationException>(() => registry.Add(
                EasternWeaponCatalog.RequireCategory(EasternWeaponFamily.Wakizashi)),
                "Registry accepted a duplicate category definition.");
            Assertions.Throws<InvalidOperationException>(() =>
                registry.ValidateLoadedValues(new[] {
                    new KeyValuePair<int, string>(0x004b4d49, "ForeignKatana") }),
                "Registry accepted a loaded category collision.");
            registry.ValidateLoadedValues(new[] {
                new KeyValuePair<int, string>(74, "NativeCategory") });
        }

        internal static void GenericBlueprintSourceContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string source = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "EasternWeaponBlueprints.cs"));
            foreach (string token in new[] {
                "ValidateCategoryCollisions(library)",
                "EasternWeaponCatalog.AllCategories",
                "EasternWeaponCatalog.AllGenericItems",
                "NativeMasterworkGuid",
                "NativeEnhancementOneGuid",
                "spec.Enhancement == 1 ? new[] { plusOne }",
                "spec.Masterwork ? new[] { masterwork }",
                "PhysicalDamageMaterial.ColdIron",
                "item.Description.IndexOf(\"Brace\"",
                "type.AttackRange.Value != 2",
                "IsOneHandedWhichCanBeUsedWithTwoHands",
                "Registered three stable categories, twelve generic items" })
                Assertions.True(source.Contains(token),
                    "Eastern generic blueprint source contract is missing: " + token);
            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "EasternWeapons",
                "EasternWeaponCategoryRuntime.cs"));
            foreach (string token in new[] {
                "StatsStrings", "HasSubCategory", "GetSubCategories",
                "get_IsOneHandedWhichCanBeUsedWithTwoHands",
                "EasternWeaponFamily.Katana", "WeaponSubCategory.Finessable",
                "WeaponSubCategory.Martial", "WeaponSubCategory.Exotic" })
                Assertions.True(runtime.Contains(token),
                    "Eastern category runtime contract is missing: " + token);

            JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(root,
                "blueprints", "blueprints.json")));
            JArray entries = (JArray)manifest["entries"];
            JObject[] eastern = entries.Cast<JObject>().Where(value =>
                ((string)value["symbol"]).StartsWith("KMG.EasternWeapons.",
                    StringComparison.Ordinal)).ToArray();
            Assertions.Equal(46, eastern.Length,
                "Eastern foundation identity ledger count changed.");
            Assertions.Equal(46, eastern.Select(value =>
                (string)value["guid"]).Distinct(StringComparer.Ordinal).Count(),
                "Eastern foundation GUIDs are not unique.");
            Assertions.Equal(3, eastern.Count(value =>
                (string)value["plannedType"] == "BlueprintWeaponType"),
                "Eastern weapon-type identity count changed.");
            Assertions.Equal(30, eastern.Count(value =>
                (string)value["plannedType"] == "BlueprintItemWeapon"),
                "Eastern total item identity count changed.");
            Assertions.True(eastern.All(value =>
                (string)value["status"] == "active" &&
                (string)value["milestone"] == "Eastern Weapons"),
                "Eastern identities must all be active and owned.");
        }

        internal static void NamedNativeCatalogIsExact()
        {
            string root = Environment.CurrentDirectory;
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "EasternWeapons",
                "EasternWeaponNamedCatalog.cs"));
            string[] names = {
                "Paper Lantern", "Quiet Current", "Falling Petal",
                "Foxfire Whisper", "Empty Sleeve", "Night Without Moon",
                "Wayfarer's Oath", "Winter Reed", "Drawn Horizon",
                "Thunder at the Gate", "Moonlit Crossing", "Heaven's Measure",
                "Border Sentinel", "Cloud-Cleaver", "Storm Over Stone",
                "Mountain-Sunder", "Unfixed Form", "World-Tree Severer" };
            foreach (string name in names)
                Assertions.True(catalog.Contains("\"" + name + "\""),
                    "Eastern named catalog is missing: " + name);
            foreach (string token in new[] {
                "Items.Length != 18", "NativeEffectiveBonus > 10",
                "EasternWeaponNativeProperty.BrilliantEnergy",
                "EasternWeaponNativeProperty.Speed", "bespokePremium" })
                Assertions.True(catalog.Contains(token),
                    "Eastern named catalog guard is missing: " + token);

            string blueprint = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "EasternWeaponNamedBlueprints.cs"));
            foreach (string token in new[] {
                "30f90becaaac51f41bf56641966c4121",
                "421e54078b7719d40915ce0672511d0b",
                "a36ad92c51789b44fa8a1c5c116a1328",
                "102a9c8c9b7a75e4fb5844e79deaf4c0",
                "47857e1a5a3ec1a46adf6491b1423b4f",
                "7bda5277d36ad114f9f9fd21d0dab658",
                "690e762f7704e1f4aa1ac69ef0ce6a96",
                "28a9964d81fedae44bae3ca45710c140",
                "66e9e299c9002ea4bb65b6f300e43770",
                "f1c0c50108025d546b2554674ea1c006",
                "result.Length != 18", "ConfigureNamed", "ValidateNamed" })
                Assertions.True(blueprint.Contains(token),
                    "Eastern named blueprint contract is missing: " + token);

            JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(root,
                "blueprints", "blueprints.json")));
            Assertions.Equal(18, EasternNamedItemEntries(manifest).Length,
                "Eastern named identity count changed.");
        }

        private static JObject[] EasternNamedItemEntries(JObject manifest)
        {
            string[] genericSuffixes = { ".BaseItem", ".MasterworkItem",
                ".ColdIronItem", ".Plus1Item" };
            return ((JArray)manifest["entries"]).Cast<JObject>().Where(value =>
            {
                string symbol = (string)value["symbol"];
                return symbol.StartsWith("KMG.EasternWeapons.",
                    StringComparison.Ordinal) &&
                    (string)value["plannedType"] == "BlueprintItemWeapon" &&
                    !genericSuffixes.Any(symbol.EndsWith);
            }).ToArray();
        }

        internal static void ProficiencySelectorAndGroupContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string blueprint = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "EasternWeaponBlueprints.cs"));
            foreach (string token in new[] {
                "Weapon Proficiency (\" + display + \")",
                "Finesse Training (Wakizashi)",
                "0fca9259e370cd049a1dd50bede687f7",
                "04f3b956e5a5cf649bce83774e0bfe4a",
                "NativeMartialWeaponProficiencyGuid",
                "PrerequisiteNotProficient",
                "AddStartingEquipment",
                "WeaponTypeDamageStatReplacement",
                "EasternWeaponProficiencyPenaltyComponent",
                "ParameterSelectorGuids" })
                Assertions.True(blueprint.Contains(token),
                    "Eastern proficiency blueprint contract is missing: " + token);

            string policy = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "EasternWeapons",
                "EasternWeaponProficiencyRuntime.cs"));
            foreach (string token in new[] {
                "weapon.HoldInTwoHands && HasBroadMartial(unit)",
                "unit.Proficiencies.Contains(category)",
                "evt.SetAttackBonusPenalty(evt.AttackBonusPenalty + 4)",
                "UnitPartWeaponTraining", "WeaponFighterGroup.BladesHeavy",
                "WeaponFighterGroup.Polearms",
                "Math.Max(__result, fact.GetRank())" })
                Assertions.True(policy.Contains(token),
                    "Eastern proficiency/group runtime is missing: " + token);
            Assertions.False(policy.Contains("GetType().Name") ||
                policy.Contains("animation") || policy.Contains("race ==") ||
                policy.Contains("class =="),
                "Eastern proficiency uses a forbidden heuristic.");

            string publication = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "EasternWeapons",
                "EasternWeaponSelectorPublication.cs"));
            foreach (string token in new[] {
                "CustomWeaponSelectorRuntime.Configure",
                "InsertOrderedAfter", "spearAnchor", "katanaEwp",
                "wakizashiEwp", "Rollback()",
                "spearCount == 1 ? 2 : 1",
                "Count(_ewpSelection.AllFeatures, spearAnchor) == 0" })
                Assertions.True(publication.Contains(token),
                    "Eastern selector transaction is missing: " + token);

            string shared = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "CustomWeapons",
                "CustomWeaponSelectorRuntime.cs"));
            Assertions.Equal(1, shared.Split(new[] {
                "\"GetFullSelectionItems\"" }, StringSplitOptions.None).Length - 1,
                "Custom weapons must use one full-selector Harmony patch.");
            Assertions.True(shared.Contains("SelectMany(value => value.Options)") &&
                shared.Contains("GroupBy(value => value.Category)") &&
                shared.Contains("OrderBy(value => value.Name"),
                "Custom weapon categories are not merged and sorted once.");
        }

        internal static void NamedBespokeEffectContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string effects = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "EasternWeapons",
                "EasternWeaponNamedEffects.cs"));
            foreach (string token in new[] {
                "evt.AttackRoll.IsCriticalConfirmed",
                "active.HoldInTwoHands",
                "evt.Weapon.HoldInTwoHands",
                "evt.AddBonusDamage(2)",
                "ability.IsRunning",
                "new ForceDamage(",
                "new DiceFormula(1, DiceType.D6)",
                "HasBuff(evt.Initiator, RoundMarker)",
                "descriptor.State.Size != descriptor.OriginalSize",
                "descriptor.Body.IsPolymorphed",
                "evt.IncreaseWeaponSize()",
                "MightyCleavingEnumerator",
                "_successfulTargets == 2",
                "<isGreater>5__3",
                "HarmonyPatch(typeof(AbilityCustomCleave), \"Deliver\"" })
                Assertions.True(effects.Contains(token),
                    "Eastern bespoke effect contract is missing: " + token);
            Assertions.False(effects.Contains("DamageEnergyType") ||
                effects.Contains("IncreaseWeaponSize();\n            evt.IncreaseWeaponSize"),
                "Eastern bespoke mechanics use an approximate or compounded path.");

            string blueprints = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "EasternWeaponNamedBlueprints.cs"));
            foreach (string token in new[] {
                "AddFactToEquipmentWielder",
                "StatType.Initiative",
                "ModifierDescriptor.Competence",
                "ModifierDescriptor.Dodge",
                "PowerAttackFeatureGuid",
                "PowerAttackToggleGuid",
                "ValidatePowerAttackAuthority",
                "OfType<PowerAttackWatcher>()",
                "MightyCleavingRuntime.Configure",
                "RegisterBuffs(registry)",
                "RegisterEnchantments(" })
                Assertions.True(blueprints.Contains(token),
                    "Eastern bespoke blueprint contract is missing: " + token);

            JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(root,
                "blueprints", "blueprints.json")));
            JObject[] effectsEntries = ((JArray)manifest["entries"])
                .Cast<JObject>().Where(value =>
                    ((string)value["symbol"]).StartsWith(
                        "KMG.EasternWeapons.", StringComparison.Ordinal) &&
                    (string)value["plannedType"] != "BlueprintItemWeapon" &&
                    (string)value["plannedType"] != "BlueprintWeaponType" &&
                    !((string)value["symbol"]).EndsWith(
                        "ExoticWeaponProficiency", StringComparison.Ordinal) &&
                    !((string)value["symbol"]).EndsWith(
                        "FinesseTraining", StringComparison.Ordinal) &&
                    !((string)value["symbol"]).EndsWith(
                        "ProficiencyPolicyEnchantment", StringComparison.Ordinal))
                .ToArray();
            Assertions.Equal(9, effectsEntries.Length,
                "Eastern bespoke identity count changed.");
            Assertions.Equal(4, effectsEntries.Count(value =>
                (string)value["plannedType"] == "BlueprintBuff"),
                "Eastern bespoke buff identity count changed.");
            Assertions.Equal(3, effectsEntries.Count(value =>
                (string)value["plannedType"] ==
                    "BlueprintWeaponEnchantment"),
                "Eastern bespoke enchantment identity count changed.");
            Assertions.Equal(2, effectsEntries.Count(value =>
                (string)value["plannedType"] == "BlueprintFeature"),
                "Eastern equipment-feature identity count changed.");
        }

        internal static void CampaignPublicationIsExactAndTransactional()
        {
            string root = Environment.CurrentDirectory;
            string campaign = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "EasternWeaponCampaignBlueprints.cs"));
            foreach (string token in new[] {
                "C11_OlegVendorTable",
                "CapitalVendorBlueprints.ExpectedTableName",
                "DireNarlmarchesVillageVendorTable",
                "PitaxTownVendorTable",
                "StandaloneHonestGuyTableGuid",
                "StandaloneXellirenTableGuid",
                "CampaignHonestGuyTableGuid",
                "CampaignXellirenTableGuid",
                "BorderSentinel",
                "Forest_BarrikadedChest1",
                "Forest_LootBoxGood2",
                "Forest_cache",
                "RichHuman_Loot_1",
                "RichHuman_ST_BackpackBard_U_Any",
                "Forest_Good_GuardedChest",
                "RichHuman_GoodLoot_BarrelJewelry",
                "FirstWorld_GoodLoot_Trapped_1",
                "FirstWorld_2ndFloorGoodHiddenLockedLoot08",
                "CleanupLoot",
                "IsHonestGuyTable",
                "placed.Length != 18",
                "placed.Distinct().Count() != 18",
                "CreateIntegrated",
                "rollback refused after foreign mutation",
                "weapons.AttachCampaign(result)" })
                Assertions.True(campaign.Contains(token),
                    "Eastern campaign publication contract is missing: " +
                    token);
            Assertions.False(campaign.Contains("NamedKinds = new[]") &&
                campaign.Contains("ordinary BTSL"),
                "Named Eastern weapons must not enter ordinary BTSL stock.");
            Assertions.Equal(39, campaign.Split(new[] {
                "new EasternLootSpec(" }, StringSplitOptions.None).Length - 1,
                "Distinct Eastern placement plus cleanup target count changed.");
            foreach (string token in new[] {
                "020246502ff864f4aab19e2fc00e63ee", "TrollLair_Exterior",
                "6abcbbc0a161aa54380808655de92197", "TrollLair_SecondLevel",
                "27b9b282c32996842bde77e360b72107", "ShrineOfLamashtu",
                "2bffac36ed3499f4f9a1e6456e96a0f6", "CandlemereTower",
                "5b8346d4fc947624e9f8728fe7a12535", "SilverstepGrotto_Cave",
                "2d95232e6fc0b594bb6e13e3d3ea0dc3", "Varnhold",
                "399410bf927fb3349bad940394fd9abe", "ArmagsTomb",
                "1946bfd560469984788d4523e0d2786a", "ArmagsTomb_Level2",
                "3160ffda16f855747ac22738f55a5c67", "RushlightFestivalCamp",
                "b4183a776ad4c0b44acbc04837630a2e", "Brineheart" })
                Assertions.True(campaign.Contains(token),
                    "Rebalanced Eastern acquisition target is missing: " + token);

            string rareCampaign = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "RareFirearmCampaignLootBlueprints.cs"));
            foreach (string token in new[] {
                "1f0bef6b8e540d644962171dc8810459", "VarnholdStockade",
                "aeba7802ade083841935daf88d4652d3", "IrovettiPalaceFW",
                "3bc451b100283774a9e23699dd869f1a", "FirstWorld_GoodLoot_Locked_2",
                "CleanupTargets", "RareFirearmLootCleanupMutation" })
                Assertions.True(rareCampaign.Contains(token),
                    "Rebalanced rare-firearm acquisition contract is missing: " + token);

            string bootstrap = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Bootstrap", "BlueprintBootstrap.cs"));
            Assertions.True(bootstrap.Contains(
                    "publicationPlan.EasternWeaponCommerce") &&
                bootstrap.Contains("EasternWeaponCampaignBlueprints") &&
                bootstrap.Contains("easternCampaignPublication.Rollback()"),
                "Eastern commerce is not module-gated and rollback-owned.");

            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            Assertions.True(runtime.Contains(
                    "RareFirearmCampaignLootBlueprints.TargetSpecs") &&
                runtime.Contains("BlueprintBootstrap.MagicFirearms") &&
                runtime.Contains("spec.AreaName"),
                "Live rare-firearm qualification must consume the authoritative target specs.");
            foreach (string token in new[] { "easternVendorRows",
                "easternNamedVendorRows", "easternBtslRows",
                "installedEasternHonestGuyTables * 12", "easternLootRows",
                "easternSet.Campaign != null",
                "eastern-vendor-publication",
                "eastern-btsl-vendor-publication",
                "eastern-named-campaign-publication",
                "expectedEasternCommerce",
                "42 + easternBtslHonestGuyTables * 12 : 0",
                "expectedEasternCommerce ? 24 : 0",
                "easternNamedBtslRows == 0",
                "expectedEasternCommerce ? 18 : 0",
                "easternPlacedKinds.Distinct().Count() ==",
                "expectedEasternCommerce ? 18 : 0" })
                Assertions.True(runtime.Contains(token),
                    "Eastern runtime commerce assertion is missing: " + token);
            Assertions.True(runtime.Contains(
                    "project-magic-item-distribution") &&
                runtime.Contains(
                    "ProjectMagicItemDiscoverabilityPolicy.Audit(observations)") &&
                runtime.Contains("exact = audit.IsAcceptable") &&
                runtime.Contains("targetAreas[entry.Key]") &&
                runtime.Contains("vendorRows == 0"),
                "Cross-system unique-item distribution is not live-qualified.");
            foreach (string areaTerm in new[] { "troll", "womb", "varnhold",
                "barbarian", "rushlight", "brineheart", "blakemoor" })
                Assertions.True(runtime.Contains("\"" + areaTerm + "\""),
                    "The read-only acquisition candidate observer omits underused campaign arc: " +
                    areaTerm);

            string development = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Development",
                "KingmakerDevelopmentBridge.RareFirearms.cs"));
            Assertions.True(development.Contains(
                    "DescribeProjectMagicItemAcquisition") &&
                development.Contains("placements=") &&
                development.Contains("countOneMatches=") &&
                development.Contains("currentAreaMatch="),
                "The read-only all-item location audit is incomplete.");
        }

        internal static void BorderSentinelPlacementIsLaterAndSingular()
        {
            string root = Environment.CurrentDirectory;
            string campaign = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "EasternWeaponCampaignBlueprints.cs"));
            int olegStart = campaign.IndexOf("C11_OlegVendorTable",
                StringComparison.Ordinal);
            int capitalStart = campaign.IndexOf(
                "CapitalVendorBlueprints.TableGuid", olegStart,
                StringComparison.Ordinal);
            Assertions.True(olegStart >= 0 && capitalStart > olegStart,
                "The exact Oleg vendor specification is unavailable.");
            string olegSpec = campaign.Substring(olegStart,
                capitalStart - olegStart);
            Assertions.False(olegSpec.Contains("BorderSentinel"),
                "Border Sentinel returned to Oleg's desired inventory.");
            Assertions.Equal(1, campaign.Split(new[] {
                "EasternWeaponNamedKind.BorderSentinel" },
                StringSplitOptions.None).Length - 1,
                "Border Sentinel must have exactly one campaign placement spec.");
            foreach (string token in new[] {
                "c8b8159fb695be64883b609a7e77e75d",
                "PoorHuman_treasure_chest_03", "StagLordFort",
                "PublicationLootTargetCount",
                "_loot.Count != expectedLoot", "LootRowCount != 18",
                "Distinct().Count() !=", "expectedLoot" })
                Assertions.True(campaign.Contains(token),
                    "Border Sentinel fixed-loot contract is missing: " + token);

            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            foreach (string token in new[] {
                "border-sentinel-later-placement",
                "borderSentinelOlegRows == 0",
                "borderSentinelVendorRows == 0",
                "borderSentinelLootRows == (expectedEasternCommerce ? 1 : 0)",
                "exact item reference across every registered shared vendor" })
                Assertions.True(runtime.Contains(token),
                    "Live Border Sentinel assertion is missing: " + token);
        }

        internal static void DevelopmentControlsAreExactAndInventoryOnly()
        {
            string root = Environment.CurrentDirectory;
            string bridge = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Development",
                "KingmakerDevelopmentBridge.EasternWeapons.cs"));
            foreach (string token in new[] {
                "DescribeEasternWeaponCatalog",
                "DescribeBorderSentinelAcquisition", "AddEasternWeaponSet",
                "AddWakizashiPath", "AddKatanaPath", "AddNodachiPath",
                "AddEasternWeapon(int index)", "items.Length != 30",
                "path.Length != 10", "after != before + 1",
                "This audit does not open, move, grant, teleport, or save anything",
                "No proficiency, feat, class level, vendor, loot, campaign flag, or save API changed" })
                Assertions.True(bridge.Contains(token),
                    "Eastern development bridge lacks: " + token);
            foreach (string forbidden in new[] { "SaveGame", "AddFact(",
                "RemoveFact(", "Publish(", "LootItemsPackFixed" })
                Assertions.False(bridge.Contains(forbidden),
                    "Eastern inventory-only development bridge contains forbidden mutation: " + forbidden);

            string controls = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Development",
                "DevelopmentControls.cs"));
            string ui = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Development", "DevelopmentUi.cs"));
            foreach (string token in new[] {
                "Print complete Eastern Weapons catalog audit",
                "Add all 30 Eastern Weapon variants",
                "Add complete Wakizashi path (10)",
                "Add complete Katana path (10)",
                "Add complete Nodachi path (10)",
                "DevelopmentControls.AddEasternWeapon" })
                Assertions.True(ui.Contains(token),
                    "Eastern development UI lacks: " + token);
            Assertions.True(controls.Contains("eastern-weapons-add-all") &&
                controls.Contains("eastern-weapons-add-wakizashi-path") &&
                controls.Contains("eastern-weapons-add-katana-path") &&
                controls.Contains("eastern-weapons-add-nodachi-path"),
                "Eastern development controls are not routed through the exception-contained bridge.");
        }

        internal static void WorkingSavePersistenceContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs"));
            string common = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            string sequence = File.ReadAllText(Path.Combine(root, "scripts",
                "Test-EasternWeaponsWorkingSavePersistence.ps1"));
            foreach (string scenario in new[] {
                "working-save-eastern-weapons-prepare",
                "working-save-eastern-weapons-verify-cleanup",
                "working-save-eastern-weapons-verify-absent" })
            {
                Assertions.True(catalog.Contains(scenario) &&
                    common.Contains("'" + scenario + "'") &&
                    sequence.Contains("'" + scenario + "'"),
                    "Eastern persistence scenario is not guarded everywhere: " +
                    scenario);
            }
            foreach (string token in new[] {
                "IsEasternWeaponsPersistenceScenario()",
                "DevelopmentControls.AddEasternWeaponSet()",
                "items.Length != 30", "before.Any(value => value != 0)",
                "set.WakizashiProficiency", "set.KatanaProficiency",
                "Eastern verify/cleanup requires the module-disabled fresh process",
                "set.Campaign == null",
                "!EasternWeaponCategoryRuntime.PresentationEnabled",
                "Game.Instance.Player.Inventory.Remove(item, 1)",
                "_workingSaveSmoke.ArmExactWorkingSaveWrite()",
                "evidence.ExpectedWorkingSaveRoutineCount == 1",
                "evidence.ExpectedWorkingSaveRoutineCount == 0" })
                Assertions.True(runner.Contains(token),
                    "Eastern persistence runner lacks: " + token);
            Assertions.True(sequence.Contains(
                    "$SaveName -cne 'KMG_AUTOMATION_WORKING'") &&
                sequence.Contains("Restore-OriginalFeatureState") &&
                sequence.Contains("Wait-ForGuardedKingmakerExit") &&
                sequence.Contains("[Convert]::ToBase64String($restored)"),
                "Eastern persistence transaction does not fail closed and restore settings exactly.");
            Assertions.False(sequence.Contains("KMG_AUTOMATION_BASELINE"),
                "Eastern persistence transaction may not name the protected baseline.");
        }

        internal static void OriginalAssetPipelineIsExactAndFailSafe()
        {
            string root = Environment.CurrentDirectory;
            string sourceRoot = Path.Combine(root, "assets-source",
                "original-models", "eastern-weapons");
            string generator = File.ReadAllText(Path.Combine(sourceRoot,
                "generate_eastern_weapons.py"));
            string report = File.ReadAllText(Path.Combine(sourceRoot,
                "eastern-weapons-build-report.json"));
            foreach (string token in new[] { "bpy.ops.export_scene.fbx",
                "bpy.ops.wm.save_as_mainfile", "film_transparent = True",
                "Original project-owned assets", "\"triangles\": 12252",
                "PYTHONHASHSEED", "ICON_RENDER_ANGLE_DEGREES = 42.0",
                "\"targetAngleDegrees\": 42.0",
                "\"tipDirection\": \"upper-right\"",
                "\"buttDirection\": \"lower-left\"",
                "\"runtimeDimensions\": [",
                "curved asymmetric single edge at local -X; blunt spine at local +X",
                "\"schemaVersion\": 3",
                "KMG_Grip", "KMG_Tip", "KMG_Butt", "KMG_Forward",
                "KMG_BladeNormal", "KMG_Edge", "KMG_Stored",
                "mesh-grounded tip/pommel",
                "ownsNegativeXExtreme", "isNegativeXOfBladeMean",
                "gripInsidePhysicalHandle", "positiveIdentityMeshScales",
                "\"overallLengthMeters\": 0.76",
                "\"overallLengthMeters\": 1.05",
                "\"overallLengthMeters\": 1.5799999999999998" })
                Assertions.True(generator.Contains(token) || report.Contains(token),
                    "Eastern Blender source/report lacks: " + token);
            var expectedFbx = new Dictionary<string, string>
            {
                { "wakizashi.fbx", "A121C0BD1010B4083A29644D49DDD61020829AFAB5687E6D0249AC1EC80543D0" },
                { "wakizashi-petal.fbx", "5A7C77D2C382ACC71C1AD0DCF9D48B7E5A5C271933B163D0BC8EF4C83CC9979C" },
                { "wakizashi-moon.fbx", "4EDB462134FA8BDE867A0AAD42E4109746317F9BBA27C0C4AF2347755A5DF2FA" },
                { "wakizashi-capstone.fbx", "5031D238E65D6D57A859E4FBF2DEEB5AA800D0F58D0376E6EA1C31935DB98233" },
                { "katana.fbx", "CD9047823503CBE1367FD6667FC0835BF6A578376210E8B9CD48C1ECD02234B8" },
                { "katana-reed.fbx", "2579AFE8BB2B18EDB274B3CBF323C815368F77A0A1543756CE72667CCF48F3BF" },
                { "katana-regal.fbx", "D3B7D032F170CE97447A6AC815D79D5608CC910E148ED2185D5AD663027EB753" },
                { "katana-capstone.fbx", "4E58F7AB12FE45787BA0F3C4860E7F3E8EB759F4519A481AB2987FA6DBB0664E" },
                { "nodachi.fbx", "87BDFBBE364865F3C9A824C9F74FD0E5823045328C096FAE2EDF5A105F211A3E" },
                { "nodachi-cleaver.fbx", "7A689899A799A4AB8060FCEB9691586259AAA80713D1697651058C1A3F35CAD0" },
                { "nodachi-titan.fbx", "AC28942CED2F76060EC75323353AF7BA11DC98958F5CC0E62173BDBAA87C918F" },
                { "nodachi-capstone.fbx", "ACFA2048E8A339AA3670A7CDF240A2E8A3F8E9187A9E3828EDA9ED0E8D3CFB84" }
            };
            foreach (KeyValuePair<string, string> pair in expectedFbx)
                Assertions.Equal(pair.Value,
                    Sha256(Path.Combine(sourceRoot, pair.Key)),
                    "Generated Eastern FBX hash changed: " + pair.Key);
            Assertions.Equal(
                "AE311993F683295D3DD996285D28385A20F593DF16903D909818EB4F25A0096B",
                Sha256(Path.Combine(root, "assets", "bundles",
                    "kingmakergunslinger.easternweapons")),
                "Dedicated Eastern Weapons bundle hash changed.");
            JObject bundleManifest = JObject.Parse(File.ReadAllText(Path.Combine(
                root, "assets", "bundles", "asset-bundle-manifest.json")));
            JObject easternBundle = ((JArray)bundleManifest["bundles"])
                .Cast<JObject>().Single(value => (string)value["name"] ==
                    "kingmakergunslinger.easternweapons");
            Assertions.Equal(
                "AE311993F683295D3DD996285D28385A20F593DF16903D909818EB4F25A0096B",
                (string)easternBundle["sha256"],
                "Eastern bundle manifest hash is stale.");
            string[] expectedPrefabs = {
                "Wakizashi", "WakizashiStored",
                "WakizashiPetal", "WakizashiPetalStored",
                "WakizashiMoon", "WakizashiMoonStored",
                "WakizashiCapstone", "WakizashiCapstoneStored",
                "Katana", "KatanaStored", "KatanaReed", "KatanaReedStored",
                "KatanaRegal", "KatanaRegalStored",
                "KatanaCapstone", "KatanaCapstoneStored",
                "Nodachi", "NodachiStored",
                "NodachiCleaver", "NodachiCleaverStored",
                "NodachiTitan", "NodachiTitanStored",
                "NodachiCapstone", "NodachiCapstoneStored" };
            Assertions.True(((JArray)easternBundle["prefabs"])
                .Select(value => (string)value).SequenceEqual(expectedPrefabs),
                "Eastern bundle manifest must enumerate each independent held/stored pair.");
            JObject reportVariants = (JObject)JObject.Parse(report)["variants"];
            JObject sourceHashes = (JObject)easternBundle["sourceFbxSha256"];
            foreach (KeyValuePair<string, string> pair in expectedFbx)
            {
                string prefab = (string)reportVariants.Properties().Single(value =>
                        (string)value.Value["fbx"] == pair.Key).Value["prefab"];
                Assertions.Equal(pair.Value, (string)sourceHashes[prefab],
                    "Eastern bundle manifest source hash changed: " + prefab);
            }

            string[] iconNames = { "wakizashi", "katana", "nodachi",
                "night-without-moon", "heavens-measure",
                "world-tree-severer" };
            string[] iconHashes = iconNames.Select(name =>
            {
                string path = Path.Combine(root, "assets", "game", "icons",
                    name + ".png");
                AssertRgbaPng128(path);
                return Sha256(path);
            }).ToArray();
            Assertions.Equal(6, iconHashes.Distinct().Count(),
                "Eastern category and capstone icons must all be distinct.");

            string blueprints = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "EasternWeaponBlueprints.cs"));
            string combat = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "EasternWeaponCombatScenario.cs"));
            string contractObserver = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "EasternWeaponContractObserver.cs"));
            foreach (string token in new[] {
                "WakizashiVisualDonorGuid", "d9fbec4637d71bd4ebc977628de3daf3",
                "KatanaVisualDonorGuid", "d2fe2c5516b56f04da1d5ea51ae3ddfe",
                "NodachiVisualDonorGuid", "5f824fbb0766a3543bbd6ae50248688f",
                "HasApprovedVisualOrNativeFallback(item, spec.Symbol)",
                "eastern-all-30-visual-identities", "CuttingEdge",
                "itemOverrideFieldExists", "exact-item-visual",
                "VisualContractMatches", "QualifyAllItemVisuals(eastern" })
                Assertions.True(blueprints.Contains(token) ||
                    combat.Contains(token) || contractObserver.Contains(token),
                    "Eastern family visual normalization lacks: " + token);

            string builder = File.ReadAllText(Path.Combine(root, "tools",
                "unity", "BuildEasternWeaponsBundle.cs"));
            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Assets",
                "EasternWeaponAssetRuntime.cs"));
            foreach (string token in new[] { "2018.4.10f1",
                "kingmakergunslinger.easternweapons", "\"Wakizashi\"",
                "\"Katana\"", "\"Nodachi\"", "prefabPath", "Grip",
                "WeaponPresentationFrameContract.SupportMarker", "Tip", "Butt", "Standard",
                "DeterministicAssetBundle", "ForceRebuildAssetBundle" })
                Assertions.True(builder.Contains(token) || runtime.Contains(token),
                    "Eastern Unity pipeline lacks: " + token);
            foreach (string token in new[] { "AssetBundle.LoadFromFile",
                "prefabs.Length != Contracts.Length * 2", "candidate.Unload(false)",
                "native-fallback:bundle-missing",
                "native-fallback:bundle-rejected", "ApplyTo",
                "ExactModels(weaponType.VisualParameters, prefab",
                "native-fallback:model-assignment-rejected",
                "HasApprovedVisualOrNativeFallback",
                "GetComponentsInChildren<Camera>",
                "GetComponentsInChildren<Light>", "InstantiatePrefab",
                "InstantiateStoredPrefab", "m_WeaponBeltModel",
                "m_WeaponSheathModel", "visual.SheathModel == null",
                "PreservesUnreplacedDonorFields", "StoredMount",
                "HasCalibratedDonorFrame",
                "offsets.m_SlotOffsets = new EquipmentOffsets.Offsets[0]",
                "held support-hand offset initialization failed" })
                Assertions.True(runtime.Contains(token),
                    "Eastern fail-safe asset runtime lacks: " + token);

            string runtimeObserver = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            foreach (string token in new[] { "easternAssetInstances",
                "InstantiatePrefab(family)", "DestroyImmediate(instance)",
                "easternAssetCleanup" })
                Assertions.True(runtimeObserver.Contains(token),
                    "Eastern runtime asset cleanup assertion lacks: " + token);

            string icons = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints", "ProjectAssetIcons.cs"));
            foreach (string token in iconNames)
                Assertions.True(icons.Contains("\"" + token + "\""),
                    "Eastern icon publication lacks: " + token);
        }

        internal static void CombatScenarioUsesLiveRulesAndCleansUp()
        {
            string root = Environment.CurrentDirectory;
            string path = Path.Combine(root, "src", "KingmakerGunslinger",
                "RuntimeTesting", "EasternWeaponCombatScenario.cs");
            string source = File.ReadAllText(path);
            foreach (string token in new[] {
                "disposable SceneEntitiesState", "SpawnHostileTarget",
                "new ItemEntityWeapon", "WeaponAttack(attacker",
                "RuleCalculateWeaponStats", "HoldInTwoHands",
                "FindNativeD20Seed(19)", "IsCriticalConfirmed",
                "powerAttack.IsOn = true", "LastMountainSunderDamage",
                "ordinary.WeaponSize + 1", "SameReferences",
                "eastern-combat-fixture-cleanup",
                "WeaponTrainingLightBladesGuid", "GetWeaponRank(equipped)",
                "eastern-selector-publication",
                "eastern-all-named-native-properties",
                "CreateFullAttack", "HasteBuffGuid", "UndeadTypeGuid" })
                Assertions.True(source.Contains(token),
                    "Eastern live combat scenario lacks: " + token);
            foreach (string token in new[] {
                "FindUnconfirmedThreat", "afterUnconfirmed",
                "mountainMiss", "afterSwitch",
                "SetPolymorphed(attacker, true)",
                "simultaneousApplications" })
                Assertions.True(source.Contains(token),
                    "Eastern live negative-control coverage lacks: " + token);
            string observer = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            Assertions.True(observer.Contains("curveEwpIndex") &&
                observer.Contains("!expectedElvenBranchedSpears"),
                "Eastern module observer lacks the spear-disabled ordering contract.");
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs"));
            string common = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(catalog.Contains(
                "disposable-eastern-weapons-combat") && common.Contains(
                "'disposable-eastern-weapons-combat'") && common.Contains(
                "RequiresSaveName = $false"),
                "Eastern combat scenario is not in both guarded catalogs as save-free.");
        }

        internal static void ArmsArmorGripBridgeIsExactAndOptional()
        {
            string root = Environment.CurrentDirectory;
            string path = Path.Combine(root, "src", "KingmakerGunslinger",
                "Compatibility", "EasternWeaponArmsArmorCompatibility.cs");
            string source = File.ReadAllText(path);
            foreach (string token in new[] {
                "AssemblyName = \"ArmsArmor\"",
                "HelperTypeName = \"ArmsArmor.Helpers\"",
                "MethodName = \"IsExoticTwoHandedMartialWeapon\"",
                "new[] { typeof(BlueprintItemWeapon) }",
                "GripTypeName = \"ArmsArmor.ItemEntityWeaponPatch\"",
                "GripMethodName = \"IsTwoHanded\"",
                "new[] { typeof(ItemEntityWeapon), typeof(UnitDescriptor) }",
                "classification.ReturnType != typeof(bool)",
                "ReferenceEquals(weapon.Type,",
                "EasternWeaponFamily.Katana",
                "ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon, weapon)",
                "owner.Body.SecondaryHand.MaybeItem == null",
                "harmony.Patch(classification, null,",
                "harmony.Patch(grip, null, new HarmonyMethod(gripPostfix), null)" })
                Assertions.True(source.Contains(token),
                    "Eastern Arms and Armor grip bridge lacks: " + token);
            Assertions.False(source.Contains("CallOfTheWild") ||
                source.Contains("WeaponCategory.BastardSword"),
                "Eastern Arms and Armor bridge acquired an unrelated optional-mod or native-category dependency.");
            string main = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Main.cs"));
            Assertions.True(main.Contains(
                "EasternWeaponArmsArmorCompatibility.Install(context.Harmony)"),
                "Eastern Arms and Armor bridge is not installed at bootstrap.");
        }

        internal static void CallOfTheWildFocusedWeaponIsExactAndOptional()
        {
            string root = Environment.CurrentDirectory;
            string path = Path.Combine(root, "src", "KingmakerGunslinger",
                "Compatibility", "CustomWeaponFocusedWeaponCompatibility.cs");
            string source = File.ReadAllText(path);
            string bootstrapSource = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Bootstrap", "BlueprintBootstrap.cs"));
            string observerSource = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "EasternWeaponContractObserver.cs"));
            foreach (string token in new[] {
                "786bde5345a548408fade70b60a70482",
                "FocusedWeaponAdvancedWeaponTrainingFeatureSelection",
                "PrerequisiteParametrizedFeature",
                "1e1f627d26ad36f43bbd26cc2bf8ac7e",
                "ContextWeaponDamageDiceReplacementForSpecificCategory",
                "29a6081e7f4d41fdb9e5da830dd32522",
                "a13bcc2d98e4426cb017d4edfa05818c",
                "70ecd8ffc4e64cce99eccaa2b509bf3d",
                "266e9d03ef6e4da6aa56b599f9a6aebc",
                "c062c6d16aecddc4ab67d9c783b2ad46",
                "dice.Length != 5", "selection == null",
                "value.ComponentsArray = Array.Empty<BlueprintComponent>()",
                "selection.AllFeatures = next",
                "eastern-cotw-focused-weapon-contract",
                "DescribeFocusedWeapon", "IsExactFocusedWeapon",
                "publicationPlan.ElvenBranchedSpearSelectors",
                "publicationPlan.EasternWeaponSelectors" })
                Assertions.True(source.Contains(token) ||
                    bootstrapSource.Contains(token) ||
                    observerSource.Contains(token),
                    "Focused Weapon compatibility lacks: " + token);
            Assertions.False(source.Contains("GetFullSelectionItems") ||
                source.Contains("ExtractSelectionItems") ||
                source.Contains("HarmonyPatch"),
                "Focused Weapon must use its native feature-selection prerequisite path.");

            JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(
                root, "blueprints", "blueprints.json")));
            JArray entries = (JArray)manifest["entries"];
            string[] symbols = {
                "KMG.CustomWeapons.FocusedWeapon.ElvenBranchedSpear",
                "KMG.CustomWeapons.FocusedWeapon.Wakizashi",
                "KMG.CustomWeapons.FocusedWeapon.Katana",
                "KMG.CustomWeapons.FocusedWeapon.Nodachi" };
            Assertions.Equal(4, entries.Count(entry => symbols.Contains(
                (string)entry["symbol"]) &&
                (string)entry["plannedType"] == "BlueprintFeature" &&
                (string)entry["status"] == "active"),
                "Focused Weapon persistent identity count changed.");
        }

        private static void AssertRgbaPng128(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Assertions.True(bytes.Length > 33 && bytes[0] == 0x89 &&
                bytes[1] == 0x50 && bytes[2] == 0x4e && bytes[3] == 0x47,
                "Eastern icon is not a PNG: " + path);
            int width = (bytes[16] << 24) | (bytes[17] << 16) |
                (bytes[18] << 8) | bytes[19];
            int height = (bytes[20] << 24) | (bytes[21] << 16) |
                (bytes[22] << 8) | bytes[23];
            Assertions.True(width == 128 && height == 128 && bytes[25] == 6,
                "Eastern icon is not exact 128x128 RGBA: " + path);
        }

        private static string Sha256(string path)
        {
            using (SHA256 value = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(value.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }
    }
}
