using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.CustomWeapons;
using KingmakerGunslinger.EasternWeapons;
using Newtonsoft.Json.Linq;
using System.IO;

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
            Assertions.Equal(19, eastern.Length,
                "Eastern foundation identity ledger count changed.");
            Assertions.Equal(19, eastern.Select(value =>
                (string)value["guid"]).Distinct(StringComparer.Ordinal).Count(),
                "Eastern foundation GUIDs are not unique.");
            Assertions.Equal(3, eastern.Count(value =>
                (string)value["plannedType"] == "BlueprintWeaponType"),
                "Eastern weapon-type identity count changed.");
            Assertions.Equal(12, eastern.Count(value =>
                (string)value["plannedType"] == "BlueprintItemWeapon"),
                "Eastern generic item identity count changed.");
            Assertions.True(eastern.All(value =>
                (string)value["status"] == "active" &&
                (string)value["milestone"] == "Eastern Weapons"),
                "Eastern generic identities must all be active and owned.");
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
                "wakizashiEwp", "PublishMartial", "Concat(new[] { nodachi })",
                "EasternWeaponProficiencyRuntime.Configure",
                "feature.ComponentsArray = next", "Rollback()" })
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
    }
}
