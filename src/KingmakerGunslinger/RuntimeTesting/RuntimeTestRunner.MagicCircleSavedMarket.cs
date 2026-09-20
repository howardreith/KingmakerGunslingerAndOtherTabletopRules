using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using KingmakerGunslinger.Acquisition;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.MagicCircle;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Only the exact named disposable working-save actor owns this receipt.
    // Production aura ownership has no dependency on this test-only payload.
    public sealed class UnitPartMagicCirclePersistenceReceipt : UnitPart
    {
        [JsonProperty] public string TableId;
        [JsonProperty] public long OriginalMoney;
        [JsonProperty] public long PurchasedMoney;
        [JsonProperty] public string[] OriginalItems;
        [JsonProperty] public string[] PurchasedItems;
        [JsonProperty] public string[] OriginalMagicGrants;
        [JsonProperty] public bool OriginalMagicPart;
        [JsonProperty] public string[] OriginalTeleportGrants;
        [JsonProperty] public bool OriginalTeleportPart;
        // Native items have no persisted UniqueId. Initial exact absence of
        // these four blueprints, complete inventory snapshots and paused guarded
        // phases identify the four purchased stacks without inventing tokens or
        // serializing duplicate cross-file ItemEntity references.
        [JsonProperty] public string[] OwnedItems;
        [JsonProperty] public int[] StockCounts;
    }
    internal sealed partial class RuntimeTestRunner
    {
        private static FieldInfo CircleMarketField(Type type, string name)
        { return type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Native market field changed: " + name); }
        private static string CircleSavedItem(ItemEntity item)
        { return item.InventorySlotIndex + ":" + item.Blueprint.AssetGuid + ":" + item.Count + ":" + item.Charges; }
        private static string[] CircleSavedItems(ItemsCollection inventory)
        { return inventory.Select(CircleSavedItem).OrderBy(value => value, StringComparer.Ordinal).ToArray(); }
        private static Dictionary<BlueprintSharedVendorTable, ItemsCollection> CircleMarketTables()
        { return (Dictionary<BlueprintSharedVendorTable, ItemsCollection>)CircleMarketField(typeof(SharedVendorTables), "m_Dictionary").GetValue(Game.Instance.Player.SharedVendorTables); }
        private static System.Collections.IList CircleMarketSavedTables()
        { return (System.Collections.IList)CircleMarketField(typeof(SharedVendorTables), "m_PersistentTables").GetValue(Game.Instance.Player.SharedVendorTables); }
        private static BlueprintSharedVendorTable CircleMarketTable(object entry)
        { return (BlueprintSharedVendorTable)CircleMarketField(entry.GetType(), "Table").GetValue(entry); }
        private static List<string> CircleMagicGrants(UnitPartMagicCircleScrollGrants part)
        { return part == null ? null : (List<string>)CircleMarketField(typeof(UnitPartMagicCircleScrollGrants), "_tables").GetValue(part); }
        private static List<string> CircleTeleportGrants(UnitPartTeleportFamiliarity part)
        { return part == null ? null : (List<string>)CircleMarketField(typeof(UnitPartTeleportFamiliarity), "_scrollVendorGrants").GetValue(part); }

        private void PrepareCircleSavedMarket(UnitEntityData[] actors)
        {
            var player = Game.Instance.Player; var owner = player.MainCharacter.Value.Descriptor;
            var circles = BlueprintBootstrap.MagicCircles;
            var supplier = TeleportationScrollVendorPublication.DecideSupplier(BlueprintBootstrap.Library);
            var magic = owner.Get<UnitPartMagicCircleScrollGrants>(); var teleport = owner.Get<UnitPartTeleportFamiliarity>();
            var cached = CircleMarketTables(); var persisted = CircleMarketSavedTables();
            var table = new[] { supplier.Priest, supplier.Arcane }.Where(value => value != null).Distinct().FirstOrDefault(value =>
                !cached.ContainsKey(value) && !persisted.Cast<object>().Any(entry => ReferenceEquals(CircleMarketTable(entry), value)) &&
                !(CircleMagicGrants(magic)?.Contains(value.AssetGuid) ?? false) && !(CircleTeleportGrants(teleport)?.Contains("shared:" + value.AssetGuid) ?? false));
            if (table == null || circles.Any(circle => player.Inventory.Any(item => ReferenceEquals(item.Blueprint, circle.Scroll))))
                throw new InvalidOperationException("Saved market fixture requires one unvisited canonical supplier and no pre-existing Circle scrolls; never replace campaign stock.");
            var receipt = actors[1].Ensure<UnitPartMagicCirclePersistenceReceipt>();
            receipt.TableId = table.AssetGuid; receipt.OriginalMoney = player.Money; receipt.OriginalItems = CircleSavedItems(player.Inventory);
            receipt.OriginalMagicPart = magic != null; receipt.OriginalMagicGrants = CircleMagicGrants(magic)?.ToArray();
            receipt.OriginalTeleportPart = teleport != null; receipt.OriginalTeleportGrants = CircleTeleportGrants(teleport)?.ToArray();
            var items = player.Inventory.ToArray();
            typeof(Player).GetProperty("Money").GetSetMethod(true).Invoke(player, new object[] { Math.Max(player.Money, 20000L) });
            var vendor = actors[1].Ensure<UnitPartVendor>(); vendor.SetSharedInventory(table);
            var trade = new VendorLogic();
            try {
                trade.BeginTrading(actors[1]);
                for (int index = 0; index < circles.Length; index++) {
                    var item = trade.StoreItems.Single(value => ReferenceEquals(value.Blueprint, circles[index].Scroll));
                    if (item.Count != MagicCircleScrollVendors.Stock) throw new InvalidOperationException("Fresh native supplier stock is not five.");
                    trade.AddForBuy(item, index % 2 == 0 ? 2 : 5);
                }
                trade.Deal();
                receipt.StockCounts = circles.Select(circle => CircleItemCount(trade.StoreItems, circle.Scroll)).ToArray();
                receipt.PurchasedMoney = player.Money; receipt.PurchasedItems = CircleSavedItems(player.Inventory);
                receipt.OwnedItems = player.Inventory.Except(items).Select(CircleSavedItem).ToArray();
                CirclePersistenceCheck("native-market-purchase", receipt.StockCounts.SequenceEqual(new[] { 3, 0, 3, 0 }) &&
                    receipt.OwnedItems.Length == 4 && circles.All(circle => player.Inventory.Any(item => ReferenceEquals(item.Blueprint, circle.Scroll))) &&
                    owner.Get<UnitPartMagicCircleScrollGrants>().Has(table.AssetGuid),
                    "actual purchases create partial and bought-out native shared stock, four owned scroll stacks and the production saved grant marker");
            }
            finally { if (trade.IsTrading) trade.EndTraiding(); }
        }
        private JObject CaptureCircleSavedMarket(UnitEntityData[] actors)
        {
            var player = Game.Instance.Player; var receipt = actors[1].Get<UnitPartMagicCirclePersistenceReceipt>();
            if (receipt == null) throw new InvalidOperationException("Exact saved market receipt is missing.");
            var table = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(BlueprintBootstrap.Library, receipt.TableId, "saved fixture supplier");
            var vendor = actors[1].Get<UnitPartVendor>();
            if (vendor == null || !ReferenceEquals(CircleMarketField(typeof(UnitPartVendor), "m_SharedInventory").GetValue(vendor), table))
                throw new InvalidOperationException("Exact fixture merchant source did not hydrate.");
            var trade = new VendorLogic();
            try {
                trade.BeginTrading(actors[1]);
                var counts = BlueprintBootstrap.MagicCircles.Select(circle => CircleItemCount(trade.StoreItems, circle.Scroll)).ToArray();
                var inventory = CircleSavedItems(player.Inventory);
                var grants = CircleMagicGrants(player.MainCharacter.Value.Get<UnitPartMagicCircleScrollGrants>());
                CirclePersistenceCheck("saved-market-no-refill", counts.SequenceEqual(receipt.StockCounts) && grants != null && grants.Contains(receipt.TableId),
                    "native shared-table reconstruction and real merchant revisit preserve partial and bought-out stock with content ON or OFF");
                CirclePersistenceCheck("saved-scroll-item-hydration", inventory.SequenceEqual(receipt.PurchasedItems) && player.Money == receipt.PurchasedMoney &&
                    receipt.OwnedItems.All(owned => player.Inventory.Count(item => CircleSavedItem(item) == owned) == 1),
                    "exact four purchased scroll stacks, GUIDs, charges, counts and gold survive fresh load and scene reconstruction");
                return new JObject { ["table"] = receipt.TableId, ["stockCounts"] = new JArray(counts),
                    ["inventory"] = new JArray(inventory), ["gold"] = player.Money, ["grants"] = new JArray(grants),
                    ["originalInventory"] = new JArray(receipt.OriginalItems), ["originalGold"] = receipt.OriginalMoney };
            }
            finally { if (trade.IsTrading) trade.EndTraiding(); }
        }
        private void CaptureCircleSavedMarketAbsence()
        {
            var player = Game.Instance.Player;
            var circles = BlueprintBootstrap.MagicCircles;
            _circlePersistenceRecord["marketAbsence"] = new JObject {
                ["inventory"] = new JArray(CircleSavedItems(player.Inventory)), ["gold"] = player.Money,
                ["cachedTables"] = new JArray(CircleMarketTables().Keys.Select(value => value.AssetGuid)),
                ["savedTables"] = new JArray(CircleMarketSavedTables().Cast<object>().Select(value => CircleMarketTable(value).AssetGuid)),
                ["grants"] = new JArray(CircleMagicGrants(player.MainCharacter.Value.Get<UnitPartMagicCircleScrollGrants>()) ?? new List<string>()) };
            CirclePersistenceCheck("fresh-market-cleanup", !player.Inventory.Any(item => circles.Any(circle => ReferenceEquals(item.Blueprint, circle.Scroll))) &&
                !Game.Instance.State.Units.All.Any(unit => unit.Get<UnitPartMagicCirclePersistenceReceipt>() != null),
                "fresh load has no purchased fixture scroll or saved fixture receipt; wrapper compares original inventory/gold and exact unvisited supplier absence");
        }
        private void CleanupCircleSavedMarket(UnitEntityData[] actors)
        {
            var player = Game.Instance.Player; var owner = player.MainCharacter.Value.Descriptor;
            var receipt = actors[1].Get<UnitPartMagicCirclePersistenceReceipt>();
            if (receipt == null || !CircleSavedItems(player.Inventory).SequenceEqual(receipt.PurchasedItems) || player.Money != receipt.PurchasedMoney)
                throw new InvalidOperationException("Saved market ownership boundary changed; cleanup refused.");
            var table = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(BlueprintBootstrap.Library, receipt.TableId, "owned saved supplier");
            var purchased = receipt.OwnedItems.Select(owned => player.Inventory.Single(item => CircleSavedItem(item) == owned)).ToArray();
            if (purchased.Distinct().Count() != 4) throw new InvalidOperationException("Purchased stack ownership is ambiguous.");
            // Validate every ownership boundary before the first mutation. The
            // existing Teleportation migration deliberately namespaces its grant.
            var cached = CircleMarketTables(); var saved = CircleMarketSavedTables();
            var magic = owner.Get<UnitPartMagicCircleScrollGrants>(); var magicRows = CircleMagicGrants(magic);
            var teleport = owner.Get<UnitPartTeleportFamiliarity>(); var teleportRows = CircleTeleportGrants(teleport);
            string teleportKey = "shared:" + table.AssetGuid;
            if (!cached.ContainsKey(table) || magicRows == null || magicRows.Count(value => value == table.AssetGuid) != 1 ||
                !(receipt.OriginalMagicGrants ?? new string[0]).SequenceEqual(magicRows.Where(value => value != table.AssetGuid)))
                throw new InvalidOperationException("Foreign Magic Circle stock/grant state changed; cleanup refused before mutation.");
            if (!(receipt.OriginalTeleportGrants ?? new string[0]).SequenceEqual(
                (teleportRows ?? new List<string>()).Where(value => value != teleportKey)) ||
                (teleportRows?.Count(value => value == teleportKey) ?? 0) > 1 ||
                (receipt.OriginalTeleportGrants ?? new string[0]).Contains(teleportKey))
                throw new InvalidOperationException("Foreign Teleportation grant state changed; cleanup refused before mutation.");
            foreach (var owned in purchased) player.Inventory.Remove(owned);
            typeof(Player).GetProperty("Money").GetSetMethod(true).Invoke(player, new object[] { receipt.OriginalMoney });
            cached.Remove(table);
            foreach (var entry in saved.Cast<object>().Where(value => ReferenceEquals(CircleMarketTable(value), table)).ToArray()) saved.Remove(entry);
            magicRows.Remove(table.AssetGuid);
            if (!receipt.OriginalMagicPart) owner.Remove<UnitPartMagicCircleScrollGrants>();
            if (teleportRows != null) {
                teleportRows.Remove(teleportKey);
                if (receipt.OriginalTeleportPart && receipt.OriginalTeleportGrants == null)
                    CircleMarketField(typeof(UnitPartTeleportFamiliarity), "_scrollVendorGrants").SetValue(teleport, null);
            }
            if (!receipt.OriginalTeleportPart) owner.Remove<UnitPartTeleportFamiliarity>();
            CirclePersistenceCheck("exact-market-restoration", CircleSavedItems(player.Inventory).SequenceEqual(receipt.OriginalItems) &&
                player.Money == receipt.OriginalMoney && !cached.ContainsKey(table) && !saved.Cast<object>().Any(value => ReferenceEquals(CircleMarketTable(value), table)),
                "remove only purchased fixture stacks and previously absent canonical table; restore original gold and both grant ledgers before the one authorized cleanup save");
        }
    }
}
