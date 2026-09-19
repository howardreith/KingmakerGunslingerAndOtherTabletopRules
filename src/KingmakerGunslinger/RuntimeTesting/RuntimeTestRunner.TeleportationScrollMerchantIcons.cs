using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.Group;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.Vendor;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Actual VendorUI, with a detached request-owned merchant and private
        // stock. No shared vendor table, selection for trade, purchase or sale.
        private IEnumerable<int> CaptureNativeStrategicScrollMerchant(
            BlueprintItemEquipmentUsable[] blueprints, ItemEntity[] playerControls, Func<bool> inventoryExact)
        {
            var game = Game.Instance; var ui = game.UI; var vendor = game.Vendor;
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationSpellbookUi ||
                !_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete ||
                _workingSaveSmoke.WriteObserved || !game.IsPaused || !inventoryExact() || vendor == null ||
                vendor.IsTrading || vendor.IsChanged || vendor.ItemsForBuy.Items.Count != 0 || vendor.ItemsForSell.Items.Count != 0 ||
                vendor.DealPrice != 0 || vendor.DealWeight != 0 || ui.ServiceWindow.WindowTabs.IsShow ||
                ui.DescriptionController.DescWindow.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Native merchant icon capture requires the owned paused fixture and empty idle trade state.");
            var shop = Resources.FindObjectsOfTypeAll<VendorUI>().Where(value =>
                value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded).Single();
            var group = GroupController.Instance;
            var groupAction = typeof(GroupController).GetField("<SelectCharacterAction>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (shop.IsShow || groupAction.GetValue(group) != null)
                throw new InvalidOperationException("Native shop or group selection is already owned.");
            var originalGroup = group.GetCurrentCharacter();
            var beforeUnits = game.State.Units.All.ToArray();
            var buy = vendor.ItemsForBuy; var sell = vendor.ItemsForSell;
            var vendorData = typeof(VendorUI).GetField("m_VendorData", BindingFlags.Instance | BindingFlags.NonPublic);
            var prices = typeof(VendorLogic).GetField("m_VendorPrices", BindingFlags.Instance | BindingFlags.NonPublic);
            var oldVendorData = vendorData.GetValue(shop); var oldPrices = prices.GetValue(vendor);
            var groups = new[] { shop.Store, shop.PlayerStash, shop.Buy, shop.Sell };
            var oldCollections = groups.Select(value => value.Collection).ToArray();
            if (oldCollections.Any(value => value != null))
                throw new InvalidOperationException("Idle native merchant retained unexpected item collections.");
            var filters = groups.Select(value => value.Filter).Where(value => value != null).Distinct().ToArray();
            var filterState = filters.Select(value => new { Filter = value, Kind = value.CurrentFilter,
                Sorter = value.CurrentSorter, Save = value.Save }).ToArray();
            var controlBlueprint = playerControls.First(value => value.Icon != null &&
                !value.Blueprint.name.StartsWith("KMG_", StringComparison.Ordinal)).Blueprint;
            var controlIcon = controlBlueprint.Icon;
            BlueprintUnit blueprint = null; UnitEntityData merchant = null; ItemsCollection stock = null;
            var ownedItems = new List<ItemEntity>();
            bool opened = false;
            Func<bool> idleTransfers = () => ReferenceEquals(game.Vendor, vendor) &&
                ReferenceEquals(vendor.ItemsForBuy, buy) && ReferenceEquals(vendor.ItemsForSell, sell) &&
                buy.Items.Count == 0 && sell.Items.Count == 0 && !vendor.IsChanged && vendor.DealPrice == 0 && vendor.DealWeight == 0;
            Func<bool> ownsShop = () => opened && shop.IsShow && shop.gameObject.activeInHierarchy &&
                vendor.IsTrading && ReferenceEquals(vendor.VendorUnit, merchant) &&
                ReferenceEquals(shop.Store.Collection, stock) && ReferenceEquals(vendor.StoreItems, stock) &&
                stock.Items.Count == 4 && ownedItems.All(item => ReferenceEquals(item.Collection, stock) && item.Count == 1) &&
                !merchant.Get<UnitPartVendor>().AutoIdentifyPlayersInventory && idleTransfers() &&
                inventoryExact() && game.IsPaused && !_workingSaveSmoke.WriteObserved &&
                !game.State.Units.All.Contains(merchant) && ReferenceEquals(controlBlueprint.Icon, controlIcon);
            try
            {
                blueprint = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
                blueprint.name = "KMG_Runtime_IconScrollMerchant";
                blueprint.Brain = null; blueprint.IsCheater = false;
                merchant = new ChargenUnit(blueprint).Unit;
                merchant.Descriptor.CustomName = "KMG Icon Scroll Review";
                if (game.State.Units.All.Contains(merchant) || merchant.Get<UnitPartVendor>() != null)
                    throw new InvalidOperationException("Native merchant fixture is registered or already has vendor stock.");
                var part = merchant.Ensure<UnitPartVendor>();
                stock = part.Inventory;
                if (part.AutoIdentifyPlayersInventory || stock.Items.Count != 0 || ReferenceEquals(stock, game.Player.Inventory))
                    throw new InvalidOperationException("Native merchant fixture did not receive private empty stock.");
                foreach (var itemBlueprint in blueprints) ownedItems.Add(stock.Add(itemBlueprint));
                var control = stock.Add(controlBlueprint); ownedItems.Add(control);
                if (ownedItems.Any(value => value == null) || !inventoryExact())
                    throw new InvalidOperationException("Native merchant fixture setup changed player inventory.");
                foreach (var filter in filters) filter.Save = false;
                opened = true;
                shop.HandleTradeStarted(merchant);
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (!ownsShop()) throw new InvalidOperationException("Actual native VendorUI did not bind to the private merchant and stock.");
                for (int index = 0; index < blueprints.Length; index++)
                {
                    var item = ownedItems[index]; var itemBlueprint = blueprints[index];
                    var slot = shop.Store.GetComponentsInChildren<ItemSlot>(true).Single(value =>
                        value.gameObject.activeInHierarchy && ReferenceEquals(value.Item, item));
                    Func<bool> retained = () => ownsShop() && slot != null && ReferenceEquals(slot.Item, item);
                    Func<JObject> describe = () => {
                        var state = DescribeNativeScrollSlot(slot, item, itemBlueprint, new[] { control }, retained());
                        state["surface"] = "native-scroll-merchant";
                        state["merchant"] = new JObject { ["nativeVendorBound"] = ReferenceEquals(vendor.VendorUnit, merchant),
                            ["nativeStoreBound"] = ReferenceEquals(shop.Store.Collection, stock),
                            ["privateStock"] = !merchant.Get<UnitPartVendor>().AutoIdentifyPlayersInventory,
                            ["stockCount"] = stock.Items.Count, ["transferCollectionsEmpty"] = idleTransfers(),
                            ["playerInventoryUnchanged"] = inventoryExact(), ["unregisteredVendor"] = !game.State.Units.All.Contains(merchant),
                            ["controlBlueprintRetained"] = ReferenceEquals(controlBlueprint.Icon, controlIcon) };
                        return state;
                    };
                    foreach (int frame in _teleportationNativeIconScreens.CaptureRow("native-scroll-merchant-row:" + itemBlueprint.AssetGuid,
                        (RectTransform)slot.transform, describe, retained)) yield return frame;
                    var stateAfter = describe(); var target = (JObject)stateAfter["targetRow"];
                    bool exact = retained() && (bool)target["renderedIconExact"] && (bool)target["scrollIconExact"] &&
                        (bool)target["spellIconDistinctFromItem"] &&
                        (bool)target["identified"] && (int)target["otherItemRows"] > 0 && (bool)target["otherItemIconsExact"];
                    TeleportSpellbookUiAssert("merchant-icon-" + itemBlueprint.AssetGuid,
                        "actual native merchant row retains the composed scroll identity with the approved spell symbol inside, private stock, ordinary control and empty trade collections", stateAfter.ToString(), exact);
                    if (!exact) throw new InvalidOperationException("Native merchant scroll icon identity differs.");
                }
                if (!ownsShop()) throw new InvalidOperationException("Native merchant ownership changed before normal close.");
                shop.HandleTradeExit();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (shop.IsShow || vendor.IsTrading || !idleTransfers())
                    throw new InvalidOperationException("Native merchant did not finish its unchanged-trade close.");
                opened = false;
            }
            finally
            {
                if (opened && ReferenceEquals(game.Vendor, vendor) && ReferenceEquals(vendor.VendorUnit, merchant) && idleTransfers())
                    shop.HandleTradeExit();
                bool tradeClosed = ReferenceEquals(game.Vendor, vendor) && !vendor.IsTrading && !shop.IsShow && idleTransfers();
                var ownedCollections = new[] { stock, game.Player.Inventory, buy, sell };
                bool collectionsRestorable = groups.Select((value, index) => value.VirtualSlots.Count == 0 &&
                    (ReferenceEquals(value.Collection, ownedCollections[index]) || ReferenceEquals(value.Collection, oldCollections[index]))).All(value => value);
                if (tradeClosed && collectionsRestorable)
                {
                    foreach (var snapshot in filterState)
                    {
                        snapshot.Filter.Save = false;
                        snapshot.Filter.SetSorter(snapshot.Sorter);
                        snapshot.Filter.ChangeFilter(snapshot.Kind, false, true);
                        snapshot.Filter.Save = snapshot.Save;
                    }
                    // Native Cleanup empties its virtual lists but retains this
                    // auto-property. Release only the exact owned bindings after
                    // normal close, before disposing the temporary stock.
                    var collectionSetter = typeof(SlotsGroup).GetProperty("Collection").GetSetMethod(true);
                    for (int index = 0; index < groups.Length; index++)
                        collectionSetter.Invoke(groups[index], new object[] { oldCollections[index] });
                    vendorData.SetValue(shop, oldVendorData); prices.SetValue(vendor, oldPrices);
                    if (!ReferenceEquals(group.GetCurrentCharacter(), originalGroup)) group.SelectUnit(originalGroup);
                }
                bool stockExact = stock != null && stock.Items.Count == ownedItems.Count && ownedItems.All(item =>
                    item != null && ReferenceEquals(item.Collection, stock) && stock.Items.Contains(item) && item.Count == 1);
                if (tradeClosed && collectionsRestorable && stockExact)
                {
                    foreach (var item in ownedItems) stock.Remove(item, 1).Dispose();
                    merchant.Dispose();
                    UnityEngine.Object.Destroy(blueprint);
                }
                var cleanup = new JObject { ["tradeClosed"] = tradeClosed, ["ownedStockRemoved"] = stockExact && stock.Items.Count == 0,
                    ["closedCollectionsOwned"] = collectionsRestorable,
                    ["playerInventory"] = inventoryExact(), ["ordinaryBlueprint"] = ReferenceEquals(controlBlueprint.Icon, controlIcon),
                    ["originalUnitRegistry"] = game.State.Units.All.Count == beforeUnits.Length && beforeUnits.All(game.State.Units.All.Contains),
                    ["vendorData"] = ReferenceEquals(vendorData.GetValue(shop), oldVendorData),
                    ["vendorPrices"] = ReferenceEquals(prices.GetValue(vendor), oldPrices),
                    ["filterValues"] = filterState.All(value => value.Filter.CurrentFilter == value.Kind && value.Filter.CurrentSorter == value.Sorter && value.Filter.Save == value.Save),
                    ["nativeCollections"] = groups.Select(value => value.Collection).SequenceEqual(oldCollections),
                    ["group"] = ReferenceEquals(group.GetCurrentCharacter(), originalGroup) && groupAction.GetValue(group) == null,
                    ["zeroSaveWrites"] = !_workingSaveSmoke.WriteObserved };
                bool restored = cleanup.Properties().All(value => (bool)value.Value);
                CaptureTeleportSpellbookUi("native-scroll-merchant-cleanup", new JObject { ["restored"] = restored, ["checks"] = cleanup });
                TeleportSpellbookUiAssert("merchant-icon-cleanup", "only private merchant stock disposed; player inventory, trade state, filters, group and registry retained; zero writes", cleanup.ToString(), restored);
            }
        }
    }
}
