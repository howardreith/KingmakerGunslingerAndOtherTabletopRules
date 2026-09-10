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
    // Native stock components only generate inventory when a vendor entity is
    // CREATED, so merchants already materialized in an existing campaign never
    // see the published scroll rows. This idempotent sweep runs when trading
    // begins and adds the finite batch exactly once per shared vendor table,
    // tracked by a save-owned grant marker — one grant identity per shared
    // table, so the whole aliased family receives exactly one batch.
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
        { Migrate(vendorUnit, BlueprintBootstrap.Library == null ? null : TeleportationScrollVendorPublication.DecideSupplier(BlueprintBootstrap.Library)); }
        // The bounded fixture seam: the SAME shared decision governs migration;
        // a guarded fixture may establish the genuine-absence condition.
        internal static void Migrate(UnitEntityData vendorUnit, TeleportationScrollVendorPublication.SupplierDecision decision)
        {
            if (vendorUnit == null || BlueprintBootstrap.TeleportationScrolls == null) return;
            var player = Game.Instance == null ? null : Game.Instance.Player;
            if (player == null) return;
            var ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
            var part = vendorUnit.Descriptor == null ? null : vendorUnit.Descriptor.Get<UnitPartVendor>();
            if (ledger == null || part == null) return;
            // Both mission families use shared vendor tables (Zarcie's
            // AddVendorItems carries the Arcane I shared table; Arsinoe and the
            // Jhod units reference C11_JhodVendorTable). The supplier decision is
            // the SAME one publication used: when the primary arcane table is
            // genuinely unavailable, the approved Hassuf fallback table is the
            // arcane supplier and receives the batch exactly once under its own
            // grant identity. One grant identity per shared table: the first
            // family member to open trading materializes the batch for the whole
            // aliased family.
            var sharedTable = SharedInventoryTable(part);
            if (sharedTable == null) return;
            var supplier = decision ?? TeleportationScrollVendorPublication.DecideSupplier(BlueprintBootstrap.Library);
            bool priest = supplier.Priest != null && string.Equals(sharedTable.AssetGuid,
                supplier.Priest.AssetGuid, StringComparison.Ordinal);
            bool arcane = supplier.Arcane != null && string.Equals(sharedTable.AssetGuid,
                supplier.Arcane.AssetGuid, StringComparison.Ordinal);
            if (!priest && !arcane) return;
            var table = player.SharedVendorTables.GetTable(sharedTable);
            if (table != null) EnsureBatch(ledger, "shared:" + sharedTable.AssetGuid,
                table, BatchFor(vendorUnit, priest));
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

        private static readonly FieldInfo SharedInventoryField = typeof(UnitPartVendor)
            .GetField("m_SharedInventory", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static BlueprintSharedVendorTable SharedInventoryTable(UnitPartVendor part)
        { return SharedInventoryField == null ? null : SharedInventoryField.GetValue(part) as BlueprintSharedVendorTable; }
    }
}
