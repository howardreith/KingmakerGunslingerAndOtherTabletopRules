using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.ElvenBranchedSpear;

namespace KingmakerGunslinger.Development
{
    internal sealed partial class KingmakerDevelopmentBridge
    {
        internal DevelopmentActionResult DescribeRareFirearmCatalog()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            MagicFirearmBlueprintCatalog catalog = RequireRareCatalog();
            string report = string.Join(" | ", catalog.Entries.Select(entry =>
                entry.Spec.DisplayName + ":guid=" + entry.Item.AssetGuid +
                ":family=" + entry.Spec.Kind + ":type=" +
                entry.Family.WeaponType.AssetGuid + ":enchantments=" +
                DescribeEnchantments(entry.Item) + ":equivalent=+" +
                entry.Spec.EquivalentBonus.ToString(CultureInfo.InvariantCulture) +
                ":cost=" + entry.Spec.Cost.ToString(CultureInfo.InvariantCulture) +
                ":weight=" + entry.Item.Weight.ToString(CultureInfo.InvariantCulture) +
                ":inventory=" + CountMatchingInventoryItems(inventory, entry.Item)
                    .ToString(CultureInfo.InvariantCulture)).ToArray());
            return DevelopmentActionResult.Success("RARE FIREARM CATALOG: " + report);
        }

        internal DevelopmentActionResult AddRareFirearm(int index)
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            MagicFirearmBlueprintCatalog catalog = RequireRareCatalog();
            if (index < 0 || index >= catalog.Entries.Length)
                throw new ArgumentOutOfRangeException("index");
            MagicFirearmBlueprintEntry entry = catalog.Entries[index];
            object inventory = RequireInventory(runtime.Player);
            int before = CountMatchingInventoryItems(inventory, entry.Item);
            AddExact(inventory, entry.Item);
            int after = CountMatchingInventoryItems(inventory, entry.Item);
            if (after != before + 1)
                throw new InvalidOperationException("Exact rare-firearm inventory count did not increase by one.");
            return DevelopmentActionResult.Success("Added exact development item " +
                entry.Spec.DisplayName + ":" + entry.Item.AssetGuid +
                ";count=" + after + ". Use only a disposable save; this does not prove campaign placement.");
        }

        internal DevelopmentActionResult AddRareFirearmSet()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: false);
            object inventory = RequireInventory(runtime.Player);
            MagicFirearmBlueprintCatalog catalog = RequireRareCatalog();
            foreach (MagicFirearmBlueprintEntry entry in catalog.Entries)
                AddExact(inventory, entry.Item);
            return DevelopmentActionResult.Success("Added one exact copy of all eight rare-firearm test items to shared inventory. No proficiency, ammunition, class level, or placement state changed. Use a disposable save.");
        }

        internal DevelopmentActionResult DescribeRareFirearmAcquisition()
        {
            ResolveRuntime(requireUnit: false);
            MagicFirearmBlueprintCatalog catalog = RequireRareCatalog();
            object currentArea = null; string areaMember;
            ReflectionAccess.TryGetFirstNonNullMember(Game.Instance,
                new[] { "CurrentlyLoadedArea" }, out currentArea, out areaMember);
            string currentGuid = ReadString(currentArea, "AssetGuid");
            string currentName = ReadString(currentArea, "name");
            RareFirearmCampaignLootBlueprints.TargetSpec[] targets =
                RareFirearmCampaignLootBlueprints.TargetSpecs;
            string report = string.Join(" | ", targets.Select(target =>
            {
                MagicFirearmBlueprintEntry item = catalog.Require(target.ItemSymbol);
                var loot = BlueprintBootstrap.Library.GetAllBlueprints()
                    .OfType<Kingmaker.Blueprints.Loot.BlueprintLoot>()
                    .Single(value => value.AssetGuid == target.Guid);
                bool published = (loot.Items ?? new Kingmaker.Blueprints.Loot.LootEntry[0])
                    .Count(value => value != null && ReferenceEquals(value.Item,
                        item.Item) && value.Count == 1) == 1;
                bool areaMatch = loot.Area != null && (ReferenceEquals(loot.Area,
                    currentArea) || loot.Area.AssetGuid == currentGuid);
                return item.Spec.DisplayName + ":item=" + item.Item.AssetGuid +
                    ":target=" + loot.name + ":" + loot.AssetGuid +
                    ":type=" + loot.GetType().FullName + ":area=" +
                    loot.Area.name + ":" + loot.Area.AssetGuid +
                    ":originalContentsPreserved=observer-qualified" +
                    ":knownReferences=0:unique-area-owned" +
                    ":published=" + published + ":currentAreaMatch=" + areaMatch +
                    ":liveEntity=unresolved-read-only:coordinates=unavailable:distance=unavailable";
            }).ToArray());
            return DevelopmentActionResult.Success("RARE FIREARM ACQUISITION AUDIT; currentArea=" +
                (string.IsNullOrEmpty(currentName) ? "<unresolved>" : currentName) +
                ":" + (string.IsNullOrEmpty(currentGuid) ? "<unresolved>" : currentGuid) +
                "; " + report + ". Locator fallback is identity/current-area reporting only; it never opens, moves, highlights, teleports, or mutates loot.");
        }

        internal DevelopmentActionResult DescribeProjectMagicItemAcquisition()
        {
            ResolveRuntime(requireUnit: false);
            object currentArea = null; string areaMember;
            ReflectionAccess.TryGetFirstNonNullMember(Game.Instance,
                new[] { "CurrentlyLoadedArea" }, out currentArea, out areaMember);
            string currentGuid = ReadString(currentArea, "AssetGuid");
            string currentName = ReadString(currentArea, "name");
            var records = new System.Collections.Generic.List<string>();
            MagicFirearmBlueprintCatalog firearms = RequireRareCatalog();
            foreach (RareFirearmCampaignLootBlueprints.TargetSpec spec in
                RareFirearmCampaignLootBlueprints.TargetSpecs)
                records.Add(DescribeProjectPlacement(
                    firearms.Require(spec.ItemSymbol).Item, spec.Guid,
                    currentArea, currentGuid));
            EasternWeaponBlueprintSet eastern = BlueprintBootstrap.EasternWeapons;
            foreach (EasternLootSpec spec in
                EasternWeaponCampaignBlueprints.LootSpecs)
                records.Add(DescribeProjectPlacement((BlueprintItem)eastern.Named
                    .Require(spec.NamedKinds.Single()).Item, spec.Guid,
                    currentArea, currentGuid));
            ElvenBranchedSpearBlueprintSet spears =
                BlueprintBootstrap.ElvenBranchedSpears;
            foreach (ElvenBranchedSpearCampaignBlueprints.LootSpec spec in
                ElvenBranchedSpearCampaignBlueprints.LootSpecs)
                records.Add(DescribeProjectPlacement((BlueprintItem)spears.Named
                    .Require(spec.NamedKind).Item, spec.Guid, currentArea,
                    currentGuid));
            records.Add(DescribeProjectPlacement(
                BlueprintBootstrap.CordOfStubbornResolve,
                CordOfStubbornResolveBlueprints.AcquisitionGuid, currentArea,
                currentGuid));
            return DevelopmentActionResult.Success(
                "PROJECT MAGIC ITEM ACQUISITION AUDIT (READ ONLY); placements=" +
                records.Count + ";currentArea=" +
                (string.IsNullOrEmpty(currentName) ? "<unresolved>" : currentName) +
                ":" + (string.IsNullOrEmpty(currentGuid) ? "<unresolved>" :
                    currentGuid) + "; " + string.Join(" | ", records.ToArray()) +
                ". This audit does not open, move, grant, select, teleport, or save anything.");
        }

        private static string DescribeProjectPlacement(BlueprintItem item,
            string targetGuid, object currentArea, string currentAreaGuid)
        {
            BlueprintLoot target = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintLoot>().Single(value => string.Equals(
                    value.AssetGuid, targetGuid, StringComparison.Ordinal));
            LootEntry[] matches = (target.Items ?? new LootEntry[0]).Where(value =>
                value != null && ReferenceEquals(value.Item, item)).ToArray();
            bool currentAreaMatch = target.Area != null &&
                (ReferenceEquals(target.Area, currentArea) || string.Equals(
                    target.Area.AssetGuid, currentAreaGuid,
                    StringComparison.Ordinal));
            return item.Name + ":item=" + item.AssetGuid + ":target=" +
                target.name + ":" + target.AssetGuid + ":area=" +
                (target.Area == null ? "<none>" : target.Area.name + ":" +
                    target.Area.AssetGuid) + ":countOneMatches=" +
                matches.Count(value => value.Count == 1) +
                ":currentAreaMatch=" + currentAreaMatch;
        }

        private static MagicFirearmBlueprintCatalog RequireRareCatalog()
        {
            MagicFirearmBlueprintCatalog value = BlueprintBootstrap.MagicFirearms;
            if (value == null || value.Entries == null || value.Entries.Length != 8)
                throw new InvalidOperationException("Rare firearm blueprint initialization has not completed.");
            return value;
        }

        private static void AddExact(object inventory, BlueprintItemWeapon item)
        {
            object result; string method;
            object[][] argumentSets = { new object[] { item, 1, false },
                new object[] { item, 1 }, new object[] { item } };
            if (!ReflectionAccess.TryInvokeAny(inventory,
                new[] { "Add", "AddItem", "AddItemSilent" }, argumentSets,
                out result, out method))
                throw new MissingMethodException("No compatible shared-inventory add method was resolved.");
        }

        private static string DescribeEnchantments(BlueprintItemWeapon item)
        {
            FieldInfo field = typeof(BlueprintItemWeapon).GetField("m_Enchantments",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            BlueprintWeaponEnchantment[] values = field == null ? null :
                field.GetValue(item) as BlueprintWeaponEnchantment[];
            return values == null ? "<unresolved>" : string.Join(",",
                values.Select(value => value.Name + ":" + value.AssetGuid).ToArray());
        }

        private static string ReadString(object source, string member)
        {
            object value;
            return ReflectionAccess.TryGetMember(source, member, out value) &&
                value != null ? Convert.ToString(value, CultureInfo.InvariantCulture) : null;
        }
    }
}
