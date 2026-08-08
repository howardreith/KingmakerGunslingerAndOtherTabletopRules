using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

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
