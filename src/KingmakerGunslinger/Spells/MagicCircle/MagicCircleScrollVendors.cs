using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Spells.MagicCircle
{
    internal sealed class MagicCircleScrollVendors
    {
        // Same established finite protective-scroll batch and supplier decision.
        internal const int Stock = 5;
        private readonly Dictionary<BlueprintSharedVendorTable, BlueprintComponent[]> _owned =
            new Dictionary<BlueprintSharedVendorTable, BlueprintComponent[]>();
        internal static MagicCircleScrollVendors Publish(LibraryScriptableObject library, MagicCircleBlueprintSet[] circles)
        {
            var result = new MagicCircleScrollVendors();
            var supplier = TeleportationScrollVendorPublication.DecideSupplier(library);
            try {
                foreach (var table in new[] { supplier.Arcane, supplier.Priest }.Where(value => value != null).Distinct()) {
                    var existing = table.ComponentsArray ?? Array.Empty<BlueprintComponent>();
                    var additions = new List<BlueprintComponent>();
                    foreach (var circle in circles) {
                        var found = existing.OfType<LootItemsPackFixed>().Where(row =>
                            CapitalVendorBlueprints.ReadItem(row)?.AssetGuid == circle.Scroll.AssetGuid).ToArray();
                        if (found.Length > 1 || found.Any(row => !ReferenceEquals(CapitalVendorBlueprints.ReadItem(row), circle.Scroll) ||
                            CapitalVendorBlueprints.ReadCount(row) != Stock))
                            throw new InvalidOperationException("Conflicting Magic Circle scroll stock: " + table.AssetGuid);
                        if (found.Length != 0) continue;
                        var addition = CapitalVendorBlueprints.CreateFixedEntry(circle.Scroll, Stock);
                        addition.name = "$KMG_MagicCircle_ScrollStock_" + circle.Alignment;
                        additions.Add(addition);
                    }
                    var publication = VendorCatalogPublication<BlueprintComponent>.Create(existing, additions.ToArray());
                    result._owned.Add(table, additions.ToArray());
                    table.ComponentsArray = publication.Published;
                }
                return result;
            }
            catch { result.Rollback(); throw; }
        }
        internal void Rollback()
        {
            // Remove only our exact inserted objects; later foreign rows survive.
            foreach (var entry in _owned) entry.Key.ComponentsArray = entry.Key.ComponentsArray.Where(row => !entry.Value.Contains(row)).ToArray();
            _owned.Clear();
        }
    }

    // Ordinary native stock creation misses merchants already materialized in a
    // saved campaign. Follow the existing one-time shared-table grant convention;
    // this separate small save payload has no dependency on Teleportation being ON.
    public sealed class UnitPartMagicCircleScrollGrants : UnitPart
    {
        [JsonProperty] private readonly List<string> _tables = new List<string>();
        internal bool Has(string table) { return _tables.Contains(table); }
        internal void Mark(string table) { if (!Has(table)) { _tables.Add(table); _tables.Sort(StringComparer.Ordinal); } }
    }

    internal static class MagicCircleScrollVendorMigration
    {
        private static readonly FieldInfo SharedInventory = typeof(UnitPartVendor).GetField("m_SharedInventory", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static bool Installed { get; private set; }
        internal static void Install(ModContext context)
        {
            if (!context.FeatureModules.Active.MagicCircleSpells || Installed) return;
            string owner = context.ModId + ".MagicCircle.ScrollVendors";
            var harmony = HarmonyInstance.Create(owner);
            try {
                var method = typeof(VendorLogic).GetMethod("BeginTrading", new[] { typeof(UnitEntityData) });
                if (method == null || SharedInventory == null) throw new InvalidOperationException("Native shared vendor contract changed.");
                harmony.Patch(method, null, new HarmonyMethod(typeof(MagicCircleScrollVendorMigration).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic)), null);
                Installed = true;
            }
            catch (Exception exception) {
                harmony.UnpatchAll(owner);
                context.Logger.Failure("magic-circle", "saved-scroll-stock.unavailable", "Saved vendor migration is unavailable; unrelated modules continue.", exception);
            }
        }
        private static void Postfix(UnitEntityData __0)
        {
            try { Migrate(__0); }
            catch (Exception exception) { ModContext context; if (ModContext.TryGet(out context))
                context.Logger.Failure("magic-circle", "scroll-stock.failed", "Existing vendor stock preserved; Magic Circle grant unavailable.", exception); }
        }
        internal static void Migrate(UnitEntityData vendor)
        {
            if (!Installed || BlueprintBootstrap.MagicCirclePublication == null || vendor == null) return;
            var player = Game.Instance?.Player;
            var owner = player?.MainCharacter.Value?.Descriptor;
            var part = vendor.Descriptor.Get<UnitPartVendor>();
            if (owner == null || part == null) return;
            var table = SharedInventory.GetValue(part) as BlueprintSharedVendorTable;
            var supplier = TeleportationScrollVendorPublication.DecideSupplier(BlueprintBootstrap.Library);
            if (table == null || !ReferenceEquals(table, supplier.Arcane) && !ReferenceEquals(table, supplier.Priest)) return;
            var ledger = owner.Ensure<UnitPartMagicCircleScrollGrants>();
            if (ledger.Has(table.AssetGuid)) return;
            var inventory = player.SharedVendorTables.GetTable(table);
            if (inventory == null) return;
            var scrolls = BlueprintBootstrap.MagicCircles.Select(circle => circle.Scroll).ToArray();
            // Never top up a partially depleted batch, and never refill bought-out
            // stock after the save-owned grant marker was recorded.
            if (!inventory.Any(item => scrolls.Contains(item.Blueprint)))
                foreach (var scroll in scrolls) inventory.Add(scroll, MagicCircleScrollVendors.Stock);
            ledger.Mark(table.AssetGuid);
        }
    }
}
