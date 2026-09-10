using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Loot;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Native AddVendorItems/AddSharedVendor components only generate stock when a
    // vendor entity is CREATED, so merchants already materialized in an existing
    // campaign never see the published scroll rows. This idempotent sweep runs
    // when trading begins and adds the finite batch exactly once per target,
    // tracked by a save-owned grant marker:
    // - one marker per shared table (the whole priest family receives one batch);
    // - one marker per own-inventory vendor entity (Zarcie's clone).
    // A target that already stocked the batch natively only records its marker.
    // Bought-out stock is never replenished: the marker, not the current count,
    // decides. The sweep runs only with the module ON and touches nothing else.
    internal static class TeleportationScrollVendorMigration
    {
        internal static bool Installed { get; private set; }

        internal static void Install(ModContext context)
        {
            if (!context.FeatureModules.Active.TeleportationSpells || Installed) return;
            MethodInfo beginTrading = typeof(VendorLogic).GetMethod("BeginTrading", new[] { typeof(UnitEntityData) });
            try
            {
                if (beginTrading == null || beginTrading.ReturnType != typeof(void))
                    throw new InvalidOperationException("Native vendor trading boundary differs.");
                context.Harmony.Patch(beginTrading, null, new HarmonyMethod(typeof(TeleportationScrollVendorMigration)
                    .GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic)), null);
                Installed = true;
                context.Logger.Info("teleportation-spells", "scroll-vendor-migration-installed",
                    "boundary=VendorLogic.BeginTrading;oncePerTarget=grant-marker;neverRefills=true");
            }
            catch (Exception exception)
            {
                Installed = false;
                try { if (beginTrading != null) context.Harmony.Unpatch(beginTrading, HarmonyPatchType.Postfix, context.ModId); }
                catch (Exception cleanup) { context.Logger.Failure("teleportation-spells", "scroll-vendor-migration-cleanup-failed", "Callback remains inert.", cleanup); }
                context.Logger.Failure("teleportation-spells", "scroll-vendor-migration-unavailable",
                    "Saved-inventory scroll migration is unavailable; fresh vendor stock still works.", exception);
            }
        }

        private static void Postfix(UnitEntityData __0)
        {
            try { Migrate(__0); }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("teleportation-spells", "scroll-vendor-migration-failed",
                        "Trading continues with the unmodified vendor inventory.", exception);
            }
        }

        // Test/observer entry point: applies the migration rules to one vendor.
        internal static void Migrate(UnitEntityData vendorUnit)
        {
            if (vendorUnit == null || BlueprintBootstrap.TeleportationScrolls == null) return;
            var player = Game.Instance == null ? null : Game.Instance.Player;
            if (player == null) return;
            var ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
            var part = vendorUnit.Descriptor == null ? null : vendorUnit.Descriptor.Get<UnitPartVendor>();
            if (ledger == null || part == null) return;
            if (IsSharedPriest(part))
            {
                // One grant identity for the shared table: the first family member
                // to open trading materializes the batch for the whole table.
                var table = player.SharedVendorTables.GetTable(SharedPriestTable());
                if (table != null) EnsureBatch(ledger, "shared:" + TeleportationScrollVendorPublication.PriestTableId,
                    table, BatchFor(vendorUnit, divine: true));
            }
            else if (UsesArcaneTable(vendorUnit))
            {
                var inventory = part.Inventory;
                if (inventory != null) EnsureBatch(ledger, "own:" + vendorUnit.UniqueId,
                    inventory, BatchFor(vendorUnit, divine: false));
            }
        }

        private static IEnumerable<KeyValuePair<BlueprintItemEquipmentUsable, int>> BatchFor(
            UnitEntityData vendorUnit, bool divine)
        {
            var scrolls = BlueprintBootstrap.TeleportationScrolls;
            if (divine)
            {
                yield return new KeyValuePair<BlueprintItemEquipmentUsable, int>(
                    scrolls.WordOfRecall, TeleportationScrollVendorPublication.WordOfRecallStock);
            }
            else
            {
                yield return new KeyValuePair<BlueprintItemEquipmentUsable, int>(
                    scrolls.Teleport, TeleportationScrollVendorPublication.TeleportStock);
                yield return new KeyValuePair<BlueprintItemEquipmentUsable, int>(
                    scrolls.GreaterTeleport, TeleportationScrollVendorPublication.GreaterTeleportStock);
            }
        }

        private static void EnsureBatch(UnitPartTeleportFamiliarity ledger, string target,
            ItemsCollection inventory, IEnumerable<KeyValuePair<BlueprintItemEquipmentUsable, int>> batch)
        {
            if (ledger.HasScrollVendorGrant(target)) return;
            bool complete = true, absent = true;
            foreach (var entry in batch)
            {
                int count = Count(inventory, entry.Key);
                if (count < entry.Value) complete = false;
                if (count > 0) absent = false;
            }
            // A natively stocked target only records its marker; a partially
            // depleted target without a marker cannot be attributed to this grant
            // and is never topped up. Only a fully absent batch is added.
            if (absent)
                foreach (var entry in batch) inventory.Add(entry.Key, entry.Value);
            else if (!complete)
                ReportPartial(target);
            ledger.RecordScrollVendorGrant(target);
        }

        private static void ReportPartial(string target)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("teleportation-spells", "scroll-vendor-grant-partial",
                    "A partially stocked target was only marked; no refill. target=" + target);
        }

        private static int Count(ItemsCollection inventory, BlueprintItem item)
        {
            if (inventory == null || item == null) return 0;
            int total = 0;
            foreach (var entity in inventory)
                if (entity != null && ReferenceEquals(entity.Blueprint, item)) total += entity.Count;
            return total;
        }

        private static bool IsSharedPriest(UnitPartVendor part)
        {
            var table = SharedInventoryTable(part);
            return table != null && string.Equals(table.AssetGuid,
                TeleportationScrollVendorPublication.PriestTableId, StringComparison.Ordinal);
        }

        private static bool UsesArcaneTable(UnitEntityData vendorUnit)
        {
            var blueprint = vendorUnit == null ? null : vendorUnit.Blueprint as BlueprintUnit;
            if (blueprint == null) return false;
            var loot = VendorItemsLoot(blueprint);
            return loot != null && string.Equals(loot.AssetGuid,
                TeleportationScrollVendorPublication.ArcaneTableId, StringComparison.Ordinal);
        }

        private static readonly FieldInfo VendorItemsLootField = typeof(Kingmaker.UnitLogic.FactLogic.AddVendorItems)
            .GetField("m_Loot", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static BlueprintUnitLoot VendorItemsLoot(BlueprintUnit unit)
        {
            if (unit == null || VendorItemsLootField == null) return null;
            foreach (var component in unit.ComponentsArray)
            {
                var items = component as Kingmaker.UnitLogic.FactLogic.AddVendorItems;
                if (items == null) continue;
                var loot = VendorItemsLootField.GetValue(items) as BlueprintUnitLoot;
                if (loot != null) return loot;
            }
            return null;
        }

        private static readonly FieldInfo SharedInventoryField = typeof(UnitPartVendor)
            .GetField("m_SharedInventory", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static BlueprintSharedVendorTable SharedInventoryTable(UnitPartVendor part)
        { return SharedInventoryField == null ? null : SharedInventoryField.GetValue(part) as BlueprintSharedVendorTable; }

        private static BlueprintSharedVendorTable SharedPriestTable()
        {
            return ResourcesLibrary.TryGetBlueprint<BlueprintSharedVendorTable>(
                TeleportationScrollVendorPublication.PriestTableId);
        }
    }
}
