using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class TeleportationScrollVendorPublication
    {
        // Verified native merchant tables (observe-teleportation-native-contracts).
        // Both are BlueprintSharedVendorTable assets: Zarcie's AddVendorItems
        // carries her Arcane I shared table (the single tier she sells, which
        // natively stocks seventh-level scrolls), and Arsinoe plus every Jhod
        // unit reference C11_JhodVendorTable. One table grant covers each whole
        // aliased family and can never double-grant.
        internal const string ArcaneTableId = "5450d563aab78134196ee9a932e88671";
        internal const string PriestTableId = "afa2c7f292b8e1c4d9c835f0e8047dd3";
        internal const string ArcaneTableName = "ArcaneScrollsVendorTableI";
        internal const string PriestTableName = "C11_JhodVendorTable";
        internal const int TeleportStock = 5;
        internal const int GreaterTeleportStock = 3;
        internal const int WordOfRecallStock = 5;

        private readonly Dictionary<BlueprintSharedVendorTable, BlueprintComponent[]> _before =
            new Dictionary<BlueprintSharedVendorTable, BlueprintComponent[]>();
        internal int ChangedTableCount { get { return _before.Count; } }

        internal static TeleportationScrollVendorPublication Publish(LibraryScriptableObject library,
            TeleportationScrollBlueprintSet scrolls, bool publish, ModLogger logger)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (scrolls == null) throw new ArgumentNullException("scrolls");
            if (logger == null) throw new ArgumentNullException("logger");
            var arcane = RequireTable(library, ArcaneTableId, ArcaneTableName);
            var priest = RequireTable(library, PriestTableId, PriestTableName);
            var result = new TeleportationScrollVendorPublication();
            // Owned covers every project scroll so module OFF normalizes all rows
            // away; stocked is the finite batch the mission approved.
            var owned = new BlueprintItem[] { scrolls.Teleport, scrolls.GreaterTeleport, scrolls.WordOfRecall };
            var batches = new[]
            {
                new { table = arcane, items = publish ? new BlueprintItem[] { scrolls.Teleport, scrolls.GreaterTeleport } : new BlueprintItem[0],
                    counts = publish ? new[] { TeleportStock, GreaterTeleportStock } : new int[0] },
                new { table = priest, items = publish ? new BlueprintItem[] { scrolls.WordOfRecall } : new BlueprintItem[0],
                    counts = publish ? new[] { WordOfRecallStock } : new int[0] }
            };
            foreach (var batch in batches) result.Apply(batch.table, batch.items, batch.counts, owned);
            if (result.ChangedTableCount == 0) return result;
            try
            {
                result.Validate(arcane, priest, scrolls, publish);
                logger.Info("teleportation-spells", "scroll-vendors.published", string.Format(CultureInfo.InvariantCulture,
                    "Normalized finite scroll stock on {0} arcane and {1} priest tables; enabled={2}.",
                    publish ? 2 : 0, publish ? 1 : 0, publish));
                return result;
            }
            catch
            {
                result.Rollback();
                throw;
            }
        }

        private void Apply(BlueprintSharedVendorTable table, BlueprintItem[] items, int[] counts, BlueprintItem[] owned)
        {
            var existing = table.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            bool exact = items.Select((item, index) => existing.OfType<LootItemsPackFixed>()
                .Where(component => ReferenceEquals(CapitalVendorBlueprints.ReadItem(component), item)).ToArray())
                .Select((found, index) => found.Length == 1 && CapitalVendorBlueprints.ReadCount(found[0]) == counts[index])
                .All(value => value);
            bool obsolete = existing.OfType<LootItemsPackFixed>().Any(component =>
                owned.Contains(CapitalVendorBlueprints.ReadItem(component)) && !items.Contains(CapitalVendorBlueprints.ReadItem(component)));
            if (exact && !obsolete) return;
            var retained = existing.Where(component =>
            {
                var fixedEntry = component as LootItemsPackFixed;
                return fixedEntry == null || !owned.Contains(CapitalVendorBlueprints.ReadItem(fixedEntry));
            }).ToArray();
            var additions = items.Select((item, index) =>
            {
                var entry = CapitalVendorBlueprints.CreateFixedEntry(item, counts[index]);
                entry.name = "$KMG_TeleportationScrollStock_" + item.name;
                return (BlueprintComponent)entry;
            }).ToArray();
            var transaction = VendorCatalogPublication<BlueprintComponent>.Create(retained, additions);
            _before.Add(table, existing);
            table.ComponentsArray = transaction.Published;
        }

        private void Validate(BlueprintSharedVendorTable arcane, BlueprintSharedVendorTable priest,
            TeleportationScrollBlueprintSet scrolls, bool publish)
        {
            if (publish)
            {
                RequireExact(arcane, scrolls.Teleport, TeleportStock);
                RequireExact(arcane, scrolls.GreaterTeleport, GreaterTeleportStock);
                RequireExact(priest, scrolls.WordOfRecall, WordOfRecallStock);
            }
            else
            {
                foreach (var table in new[] { arcane, priest })
                    foreach (var scroll in new BlueprintItem[] { scrolls.Teleport, scrolls.GreaterTeleport, scrolls.WordOfRecall })
                        if (table.ComponentsArray.OfType<LootItemsPackFixed>().Any(component =>
                            ReferenceEquals(CapitalVendorBlueprints.ReadItem(component), scroll)))
                            throw new InvalidOperationException("Disabled scroll stock remains on " + table.name);
            }
        }

        private static void RequireExact(BlueprintSharedVendorTable table, BlueprintItem item, int count)
        {
            var found = table.ComponentsArray.OfType<LootItemsPackFixed>()
                .Where(component => ReferenceEquals(CapitalVendorBlueprints.ReadItem(component), item)).ToArray();
            if (found.Length != 1 || CapitalVendorBlueprints.ReadCount(found[0]) != count)
                throw new InvalidOperationException("Scroll stock normalization differs on " + table.name);
        }

        private static BlueprintSharedVendorTable RequireTable(LibraryScriptableObject library, string guid, string expectedName)
        {
            var table = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(library, guid, "native teleportation scroll vendor table");
            if (!string.Equals(table.name, expectedName, StringComparison.Ordinal))
                throw new InvalidOperationException("Vendor table GUID/name mismatch: " + table.name + ":" + guid);
            return table;
        }

        internal void Rollback()
        {
            foreach (var entry in _before) entry.Key.ComponentsArray = entry.Value;
            _before.Clear();
        }
    }
}
