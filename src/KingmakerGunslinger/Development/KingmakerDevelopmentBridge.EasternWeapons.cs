using System;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;

namespace KingmakerGunslinger.Development
{
    internal sealed partial class KingmakerDevelopmentBridge
    {
        internal DevelopmentActionResult DescribeEasternWeaponCatalog()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            BlueprintItemWeapon[] items = RequireEasternWeapons();
            string report = string.Join(" | ", items.Select((item, index) =>
                index.ToString(CultureInfo.InvariantCulture) + ":" + item.Name +
                ":guid=" + item.AssetGuid + ":category=" + item.Type.Category +
                ":cost=" + item.Cost.ToString(CultureInfo.InvariantCulture) +
                ":weight=" + item.Weight.ToString(CultureInfo.InvariantCulture) +
                ":inventory=" + CountMatchingInventoryItems(inventory, item)
                    .ToString(CultureInfo.InvariantCulture)).ToArray());
            return DevelopmentActionResult.Success(
                "EASTERN WEAPONS CATALOG (DEVELOPMENT ONLY): " + report);
        }

        internal DevelopmentActionResult AddEasternWeaponSet()
        {
            return AddEasternWeapons(RequireEasternWeapons(),
                "all 30 Eastern Weapon variants");
        }

        internal DevelopmentActionResult AddWakizashiPath()
        {
            return AddEasternWeapons(RequireEasternWeaponPath(
                EasternWeaponFamily.Wakizashi),
                "the complete 10-item Wakizashi path");
        }

        internal DevelopmentActionResult AddKatanaPath()
        {
            return AddEasternWeapons(RequireEasternWeaponPath(
                EasternWeaponFamily.Katana),
                "the complete 10-item Katana path");
        }

        internal DevelopmentActionResult AddNodachiPath()
        {
            return AddEasternWeapons(RequireEasternWeaponPath(
                EasternWeaponFamily.Nodachi),
                "the complete 10-item Nodachi path");
        }

        internal DevelopmentActionResult AddEasternWeapon(int index)
        {
            BlueprintItemWeapon[] items = RequireEasternWeapons();
            if (index < 0 || index >= items.Length)
                throw new ArgumentOutOfRangeException("index");
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            BlueprintItemWeapon item = items[index];
            int before = CountMatchingInventoryItems(inventory, item);
            AddExact(inventory, item);
            int after = CountMatchingInventoryItems(inventory, item);
            if (after != before + 1)
                throw new InvalidOperationException(
                    "Exact Eastern Weapon inventory count did not increase by one.");
            return DevelopmentActionResult.Success(
                "Added exact development item [" + index + "] " + item.Name +
                ":" + item.AssetGuid + ";count=" + after +
                ". Use only KMG_AUTOMATION_WORKING or another disposable save.");
        }

        private DevelopmentActionResult AddEasternWeapons(
            BlueprintItemWeapon[] items, string label)
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            int[] before = items.Select(item =>
                CountMatchingInventoryItems(inventory, item)).ToArray();
            foreach (BlueprintItemWeapon item in items) AddExact(inventory, item);
            int[] after = items.Select(item =>
                CountMatchingInventoryItems(inventory, item)).ToArray();
            if (after.Where((value, index) => value != before[index] + 1).Any())
                throw new InvalidOperationException(
                    "One or more exact Eastern Weapon counts did not increase by one.");
            return DevelopmentActionResult.Success(
                "Added one exact copy of " + label +
                " to shared inventory. No proficiency, feat, class level, vendor, loot, campaign flag, or save API changed. Use only a disposable save.");
        }

        private static BlueprintItemWeapon[] RequireEasternWeapons()
        {
            EasternWeaponBlueprintSet set = BlueprintBootstrap.EasternWeapons;
            BlueprintItemWeapon[] items = set == null || set.Named == null ?
                null : set.Entries.Select(value => value.Item).Concat(
                    set.Named.Entries.Select(value => value.Item)).ToArray();
            if (items == null || items.Length != 30 ||
                items.Any(value => value == null) ||
                items.Distinct().Count() != 30)
                throw new InvalidOperationException(
                    "Eastern Weapon blueprint initialization has not completed.");
            return items;
        }

        private static BlueprintItemWeapon[] RequireEasternWeaponPath(
            EasternWeaponFamily family)
        {
            EasternWeaponBlueprintSet set = BlueprintBootstrap.EasternWeapons;
            BlueprintItemWeapon[] path = set == null || set.Named == null ? null :
                set.Entries.Where(value => value.Spec.Family == family).Select(
                    value => value.Item).Concat(set.Named.Entries.Where(value =>
                        value.Spec.Family == family).Select(value => value.Item))
                    .ToArray();
            if (path == null || path.Length != 10 ||
                path.Any(value => value == null) || path.Distinct().Count() != 10)
                throw new InvalidOperationException(
                    "The exact Eastern Weapon family path is unavailable: " +
                    family + ".");
            return path;
        }
    }
}
