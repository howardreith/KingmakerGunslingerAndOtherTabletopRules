using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UI.Tooltip;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Additional native consumers inside the existing local-area spellbook
        // fixture. Only three newly created item entities are touched. No use,
        // equip, copy, sale, world action or save operation is performed.
        private IEnumerable<int> CaptureNativeStrategicScrollItems()
        {
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationSpellbookUi ||
                !_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete ||
                _workingSaveSmoke.WriteObserved || !game.IsPaused || ui.ServiceWindow.WindowTabs.IsShow ||
                ui.DescriptionController.DescWindow.gameObject.activeInHierarchy || BlueprintBootstrap.TeleportationScrolls == null)
                throw new InvalidOperationException("Scroll icon capture requires the owned paused working-save spellbook fixture and idle native UI.");
            var inventory = player.Inventory;
            var before = inventory.Items.ToArray();
            var counts = before.Select(item => item.Count).ToArray();
            var indices = before.Select(item => item.InventorySlotIndex).ToArray();
            var identified = before.Select(item => item.IsIdentified).ToArray();
            var charges = before.Select(item => item.Charges).ToArray();
            var icons = before.Select(item => item.Icon).ToArray();
            long money = player.Money;
            var window = ui.ServiceWindow.WindowTabs.SubWindowsList.Select(pair => pair.SubWindow).OfType<Inventory>().Single();
            if (window.IsShow || window.Filter == null || window.Stash == null || window.Sheet == null)
                throw new InvalidOperationException("Native inventory is unavailable or already owned.");
            var filter = window.Filter;
            var originalFilter = filter.CurrentFilter; var originalSorter = filter.CurrentSorter;
            bool originalSave = filter.Save;
            var savedFilter = player.UISettings.InventoryFilter; var savedSorter = player.UISettings.InventorySorter;
            var originalGroup = GroupController.Instance.GetCurrentCharacter();
            var sheetCharacter = typeof(CharacterScreenController).GetField("m_CurrentCharacter", BindingFlags.Instance | BindingFlags.NonPublic);
            var sheetSection = typeof(CharacterScreenController).GetField("m_CurrentSection", BindingFlags.Instance | BindingFlags.NonPublic);
            var oldCharacter = sheetCharacter.GetValue(window.Sheet); var oldSection = sheetSection.GetValue(window.Sheet);
            var set = BlueprintBootstrap.TeleportationScrolls;
            var blueprints = new[] { set.Teleport, set.GreaterTeleport, set.WordOfRecall };
            if (before.Any(item => blueprints.Contains(item.Blueprint)))
                throw new InvalidOperationException("Owned scroll capture requires initial absence to avoid merging with existing stacks.");
            var owned = new List<ItemEntity>();
            ScrollRectExtended scroll = null;
            Vector2 originalScroll = Vector2.zero, originalVelocity = Vector2.zero;
            bool opened = false, filterOwned = false;
            var evidence = new JObject { ["surface"] = "native-scroll-inventory", ["status"] = "pending" };
            Func<bool> inventoryExact = () => ReferenceEquals(player.Inventory, inventory) && player.Money == money &&
                before.All(item => inventory.Items.Any(value => ReferenceEquals(value, item))) &&
                before.Select((item, index) => item.Count == counts[index] && item.Charges == charges[index] &&
                    item.IsIdentified == identified[index] && ReferenceEquals(item.Icon, icons[index])).All(value => value) &&
                inventory.Items.Count == before.Length + owned.Count && owned.All(item =>
                    ReferenceEquals(item.Collection, inventory) && inventory.Items.Contains(item) && item.Count == 1 && item.Charges == 1);
            Func<bool> ownsUi = () => opened && window.IsShow && window.gameObject.activeInHierarchy &&
                ui.ServiceWindow.WindowTabs.IsShow && ReferenceEquals(window.Stash.Collection, inventory) &&
                game.IsPaused && !_workingSaveSmoke.WriteObserved && inventoryExact();
            try
            {
                foreach (var blueprint in blueprints)
                {
                    var item = inventory.Add(blueprint);
                    if (item == null || before.Contains(item)) throw new InvalidOperationException("Native scroll insertion did not produce an owned entity.");
                    owned.Add(item);
                    item.Identify();
                }
                if (!inventoryExact()) throw new InvalidOperationException("Native scroll setup changed unrelated inventory state.");
                // Use normal filter APIs while retaining the saved preference.
                // NotSorted prevents this fixture from rearranging the stash.
                filterOwned = true; filter.Save = false;
                filter.SetSorter(ItemsFilter.SorterType.NotSorted);
                filter.ChangeFilter(ItemsFilter.FilterType.NoFilter, false, true);
                opened = true;
                ui.ServiceWindow.HandleOpenInventory();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (!ownsUi()) throw new InvalidOperationException("The real native inventory did not open with the exact owned items.");
                scroll = window.Stash.GetComponentsInChildren<ScrollRectExtended>(true).Single(value =>
                    value.isActiveAndEnabled && value.vertical && value.content != null && value.viewport != null);
                originalScroll = scroll.normalizedPosition; originalVelocity = scroll.velocity;
                for (int index = 0; index < owned.Count; index++)
                {
                    var item = owned[index]; var blueprint = blueprints[index];
                    var virtualSlot = window.Stash.VirtualSlots.Single(value => ReferenceEquals(value.Item, item));
                    Canvas.ForceUpdateCanvases();
                    // Scroll using the native virtual row's measured position;
                    // never relocate a pooled slot or construct a substitute row.
                    float range = scroll.content.rect.height - scroll.viewport.rect.height;
                    scroll.StopMovement();
                    if (range > 0) scroll.verticalNormalizedPosition = Mathf.Clamp01(1 -
                        (virtualSlot.Position.y + virtualSlot.Size.y / 2 - scroll.viewport.rect.height / 2) / range);
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    var slot = window.Stash.GetComponentsInChildren<ItemSlot>(true).Single(value =>
                        value.gameObject.activeInHierarchy && ReferenceEquals(value.Item, item));
                    Func<bool> ownsSlot = () => ownsUi() && slot != null && slot.gameObject.activeInHierarchy &&
                        ReferenceEquals(slot.Item, item) && ReferenceEquals(virtualSlot.Item, item);
                    Func<JObject> describe = () => DescribeNativeScrollSlot(slot, item, blueprint, before, ownsSlot());
                    foreach (int frame in _teleportationNativeIconScreens.CaptureRow("native-scroll-row:" + blueprint.AssetGuid,
                        (RectTransform)slot.transform, describe, ownsSlot)) yield return frame;
                    var state = describe();
                    var target = (JObject)state["targetRow"];
                    bool exact = (bool)target["renderedIconExact"] && (bool)target["spellIconMatchesItem"] &&
                        (bool)target["itemReferenceRetained"] && (int)target["otherItemRows"] > 0 && (bool)target["otherItemIconsExact"];
                    TeleportSpellbookUiAssert("inventory-icon-" + blueprint.AssetGuid,
                        "real native item slot uses its matching strategic spell identity and preserves existing controls", state.ToString(), exact);
                    if (!exact) throw new InvalidOperationException("Native scroll inventory icon identity differs.");
                    var tooltip = slot.Tooltip;
                    var tipObject = typeof(TooltipTrigger).GetField("m_Obj", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (tooltip == null || tooltip.gameObject != slot.gameObject)
                        throw new InvalidOperationException("The native scroll slot has no owned tooltip trigger.");
                    // OpenDescriptionWindow collects its object from the native
                    // slot on demand. An unopened trigger may have no m_Obj yet.
                    tooltip.OpenDescriptionWindow();
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    bool tooltipItemExact = ReferenceEquals(tipObject.GetValue(tooltip), item);
                    bool tooltipDataItemExact = tooltip.Data != null && ReferenceEquals(tooltip.Data.Item, item);
                    if (!tooltipItemExact || !tooltipDataItemExact)
                        throw new InvalidOperationException("The native description did not collect the exact scroll item from its slot.");
                    var description = ui.DescriptionController.DescWindow;
                    var labels = description.GetComponentsInChildren<TextMeshProUGUI>(true).Where(value => value.isActiveAndEnabled).ToArray();
                    int descriptionIcons = description.GetComponentsInChildren<Image>(true).Count(value =>
                        value.isActiveAndEnabled && ReferenceEquals(value.sprite, blueprint.Icon));
                    bool named = labels.Any(value => !value.isTextTruncated &&
                        string.Equals(value.GetParsedText(), item.Name, StringComparison.OrdinalIgnoreCase));
                    var descriptionState = describe();
                    descriptionState["surface"] = "native-scroll-description";
                    descriptionState["description"] = new JObject { ["nameExact"] = named,
                        ["matchingIcons"] = descriptionIcons, ["tooltipItemExact"] = tooltipItemExact,
                        ["tooltipDataItemExact"] = tooltipDataItemExact,
                        ["shown"] = description.gameObject.activeInHierarchy };
                    foreach (int frame in _teleportationNativeIconScreens.Capture("native-scroll-description:" + blueprint.AssetGuid,
                        descriptionState, () => ownsSlot() && description.gameObject.activeInHierarchy &&
                            ReferenceEquals(tipObject.GetValue(tooltip), item) && ReferenceEquals(tooltip.Data?.Item, item))) yield return frame;
                    bool descriptionExact = named && descriptionIcons > 0 && description.gameObject.activeInHierarchy;
                    TeleportSpellbookUiAssert("inventory-description-" + blueprint.AssetGuid,
                        "actual native item description displays the exact scroll name and matching icon", descriptionState.ToString(), descriptionExact);
                    if (!descriptionExact) throw new InvalidOperationException("Native scroll description identity differs.");
                    tooltip.CloseDescriptionWindow();
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                }
                evidence["status"] = "three-native-scroll-slots-and-descriptions-captured";
                ui.ServiceWindow.HandleOpenInventory();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (window.IsShow || ui.ServiceWindow.WindowTabs.IsShow)
                    throw new InvalidOperationException("Native inventory did not finish its normal close.");
                opened = false;
                foreach (int frame in CaptureNativeStrategicScrollMerchant(blueprints, before, inventoryExact)) yield return frame;
            }
            finally
            {
                ui.DescriptionController.HandleCloseDescriptionWindow(ui.DescriptionController.DescWindow);
                if (scroll != null && scroll.isActiveAndEnabled)
                {
                    scroll.verticalNormalizedPosition = originalScroll.y;
                    if (scroll.horizontal) scroll.horizontalNormalizedPosition = originalScroll.x;
                    scroll.velocity = originalVelocity;
                }
                if (opened && window.IsShow && ui.ServiceWindow.WindowTabs.IsShow) ui.ServiceWindow.HandleOpenInventory();
                if (filterOwned)
                {
                    filter.SetSorter(originalSorter);
                    filter.ChangeFilter(originalFilter, false, true);
                    filter.Save = originalSave;
                }
                foreach (var item in owned)
                {
                    if (!ReferenceEquals(item.Collection, inventory) || !inventory.Items.Contains(item) || item.Count != 1)
                        throw new InvalidOperationException("Owned scroll changed before exact removal.");
                    inventory.Remove(item, 1).Dispose();
                }
                // The native UI assigns display indices. Restore only those
                // original scalar indices after closing its rows and removing
                // the exact owned additions, without sorting original items.
                var slotIndex = typeof(ItemEntity).GetField("m_InventorySlotIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                for (int index = 0; index < before.Length; index++) slotIndex.SetValue(before[index], indices[index]);
                sheetCharacter.SetValue(window.Sheet, oldCharacter); sheetSection.SetValue(window.Sheet, oldSection);
                var cleanup = new JObject { ["itemReferencesAndOrder"] = inventory.Items.SequenceEqual(before),
                    ["itemScalarsAndIcons"] = before.Select((item, index) =>
                    item.Count == counts[index] && item.InventorySlotIndex == indices[index] && item.IsIdentified == identified[index] &&
                    item.Charges == charges[index] && ReferenceEquals(item.Icon, icons[index])).All(value => value),
                    ["money"] = player.Money == money, ["tabsClosed"] = !ui.ServiceWindow.WindowTabs.IsShow,
                    ["inventoryClosed"] = !window.IsShow, ["descriptionClosed"] = !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy,
                    ["filter"] = filter.CurrentFilter == originalFilter, ["sorter"] = filter.CurrentSorter == originalSorter,
                    ["filterSave"] = filter.Save == originalSave, ["savedFilter"] = player.UISettings.InventoryFilter == savedFilter,
                    ["savedSorter"] = player.UISettings.InventorySorter == savedSorter,
                    ["sheetCharacter"] = ReferenceEquals(sheetCharacter.GetValue(window.Sheet), oldCharacter),
                    ["sheetSection"] = Equals(sheetSection.GetValue(window.Sheet), oldSection),
                    ["group"] = ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), originalGroup),
                    ["zeroSaveWrites"] = !_workingSaveSmoke.WriteObserved };
                bool restored = cleanup.Properties().All(value => (bool)value.Value);
                evidence["checks"] = cleanup;
                evidence["restored"] = restored; evidence["originalItemCount"] = before.Length; evidence["temporaryItemCount"] = owned.Count;
                CaptureTeleportSpellbookUi("native-scroll-inventory-cleanup", evidence);
                TeleportSpellbookUiAssert("inventory-icon-cleanup", "exact original item references/order/counts/indices/identification/charges/icons, gold, filters, group and closed windows; zero writes",
                    evidence.ToString(), restored);
                // The assertion makes the run fail; do not mask a prior capture
                // exception while this iterator is unwinding.
            }
        }

        private static JObject DescribeNativeScrollSlot(ItemSlot slot, ItemEntity item,
            BlueprintItemEquipmentUsable blueprint, ItemEntity[] controls, bool retained)
        {
            var otherRows = slot.ParentGroup.GetComponentsInChildren<ItemSlot>(true).Where(value =>
                value.gameObject.activeInHierarchy && value.Item != null && controls.Contains(value.Item)).ToArray();
            return new JObject { ["surface"] = "native-scroll-inventory", ["targetRow"] = new JObject {
                ["name"] = item.Name, ["itemGuid"] = blueprint.AssetGuid, ["spellGuid"] = blueprint.Ability.AssetGuid,
                ["iconName"] = blueprint.Icon?.name, ["renderedIconExact"] = ReferenceEquals(slot.ItemImage.sprite, blueprint.Icon),
                ["spellIconMatchesItem"] = ReferenceEquals(blueprint.Icon, blueprint.Ability.Icon),
                ["itemReferenceRetained"] = retained, ["itemCount"] = item.Count, ["charges"] = item.Charges,
                ["identified"] = item.IsIdentified, ["otherItemRows"] = otherRows.Length,
                ["otherItemIconsExact"] = otherRows.All(value => ReferenceEquals(value.ItemImage.sprite,
                    value.Item.Icon ?? Game.Instance.BlueprintRoot.UIRoot.UIIcons.DefaultItemIcon)) } };
        }
    }
}
