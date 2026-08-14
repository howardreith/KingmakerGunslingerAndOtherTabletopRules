using System;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Development
{
    internal sealed partial class KingmakerDevelopmentBridge
    {
        internal DevelopmentActionResult DescribeElvenBranchedSpearCatalog()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            BlueprintItemWeapon[] items = RequireElvenBranchedSpears();
            string report = string.Join(" | ", items.Select((item, index) =>
                index.ToString(CultureInfo.InvariantCulture) + ":" + item.Name +
                ":guid=" + item.AssetGuid + ":cost=" +
                item.Cost.ToString(CultureInfo.InvariantCulture) +
                ":weight=" + item.Weight.ToString(CultureInfo.InvariantCulture) +
                ":inventory=" + CountMatchingInventoryItems(inventory, item)
                    .ToString(CultureInfo.InvariantCulture)).ToArray());
            return DevelopmentActionResult.Success(
                "ELVEN BRANCHED SPEAR CATALOG: " + report);
        }

        internal DevelopmentActionResult AddElvenBranchedSpear(int index)
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            BlueprintItemWeapon[] items = RequireElvenBranchedSpears();
            if (index < 0 || index >= items.Length)
                throw new ArgumentOutOfRangeException("index");
            BlueprintItemWeapon item = items[index];
            object inventory = RequireInventory(runtime.Player);
            int before = CountMatchingInventoryItems(inventory, item);
            AddExact(inventory, item);
            int after = CountMatchingInventoryItems(inventory, item);
            if (after != before + 1)
                throw new InvalidOperationException(
                    "Exact Elven Branched Spear inventory count did not increase by one.");
            return DevelopmentActionResult.Success(
                "Added exact development item " + item.Name + ":" +
                item.AssetGuid + ";count=" + after +
                ". Use only KMG_AUTOMATION_WORKING or another disposable save.");
        }

        internal DevelopmentActionResult AddElvenBranchedSpearSet()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            BlueprintItemWeapon[] items = RequireElvenBranchedSpears();
            foreach (BlueprintItemWeapon item in items) AddExact(inventory, item);
            bool exact = items.All(item =>
                CountMatchingInventoryItems(inventory, item) >= 1);
            if (!exact) throw new InvalidOperationException(
                "One or more exact Elven Branched Spear variants could not be verified after the development grant.");
            return DevelopmentActionResult.Success(
                "Added one exact copy of all 12 Elven Branched Spear variants to shared inventory. No proficiency, feat, class level, vendor, loot, or campaign state changed. Use only a disposable save.");
        }

        private static BlueprintItemWeapon[] RequireElvenBranchedSpears()
        {
            ElvenBranchedSpearBlueprintSet set =
                BlueprintBootstrap.ElvenBranchedSpears;
            BlueprintItemWeapon[] items = set == null || set.Named == null
                ? null : set.Entries.Select(value => value.Item).Concat(
                    set.Named.Entries.Select(value => value.Item)).ToArray();
            if (items == null || items.Length != 12 ||
                items.Any(value => value == null) ||
                items.Distinct().Count() != 12)
                throw new InvalidOperationException(
                    "Elven Branched Spear blueprint initialization has not completed.");
            return items;
        }
    }
}
