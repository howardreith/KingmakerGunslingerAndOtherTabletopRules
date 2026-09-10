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
        internal const string FallbackArcaneTableId = "8c17a31b6a9a6eb4cbb668902e9edcb1";
        internal const string ArcaneTableName = "ArcaneScrollsVendorTableI";
        internal const string PriestTableName = "C11_JhodVendorTable";
        internal const string FallbackArcaneTableName = "FirstVendorTable";
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
            // Arcane supplier: Zarcie's verified tier when her content is loaded;
            // the approved Hassuf fallback when it genuinely is not. Priest supply
            // resolves independently so one unavailable supplier never blocks the
            // other.
            var arcane = FindTable(library, ArcaneTableId, ArcaneTableName);
            var arcaneFallback = false;
            if (arcane == null)
            {
                arcane = FindTable(library, FallbackArcaneTableId, FallbackArcaneTableName);
                arcaneFallback = arcane != null;
            }
            var priest = FindTable(library, PriestTableId, PriestTableName);
            // Owned covers every project scroll so module OFF normalizes all rows
            // away; stocked is the finite batch the mission approved.
            var owned = new BlueprintItem[] { scrolls.Teleport, scrolls.GreaterTeleport, scrolls.WordOfRecall };
            var result = new TeleportationScrollVendorPublication();
            var batches = new List<KeyValuePair<BlueprintSharedVendorTable, KeyValuePair<BlueprintItem[], int[]>>>();
            if (arcane != null)
                batches.Add(new KeyValuePair<BlueprintSharedVendorTable, KeyValuePair<BlueprintItem[], int[]>>(
                    arcane, new KeyValuePair<BlueprintItem[], int[]>(
                        publish ? new BlueprintItem[] { scrolls.Teleport, scrolls.GreaterTeleport } : new BlueprintItem[0],
                        publish ? new[] { TeleportStock, GreaterTeleportStock } : new int[0])));
            if (priest != null)
                batches.Add(new KeyValuePair<BlueprintSharedVendorTable, KeyValuePair<BlueprintItem[], int[]>>(
                    priest, new KeyValuePair<BlueprintItem[], int[]>(
                        publish ? new BlueprintItem[] { scrolls.WordOfRecall } : new BlueprintItem[0],
                        publish ? new[] { WordOfRecallStock } : new int[0])));
            // Every mutation and every validation lives inside the rollback
            // boundary: a failure on any supplier restores ALL changed tables.
            try
            {
                foreach (var batch in batches)
                {
                    if (FaultInjection != null)
                    {
                        var injected = FaultInjection(batch.Key.name);
                        if (injected != null) throw injected;
                    }
                    result.Apply(batch.Key, batch.Value.Key, batch.Value.Value, owned);
                }
                if (result.ChangedTableCount == 0)
                {
                    LogPublished(logger, publish, arcaneFallback, arcane, priest, result, unchanged: true);
                    return result;
                }
                result.Validate(batches, scrolls, publish);
                LogPublished(logger, publish, arcaneFallback, arcane, priest, result, unchanged: false);
                return result;
            }
            catch
            {
                result.Rollback();
                throw;
            }
        }

        // Request-local fault injection for the guarded atomicity regression; a
        // null value (always, in normal play) leaves the production path intact.
        private static Func<string, Exception> _faultInjection;
        internal static Func<string, Exception> FaultInjection
        { get { return _faultInjection; } set { _faultInjection = value; } }

        private static void LogPublished(ModLogger logger, bool publish, bool fallback,
            BlueprintSharedVendorTable arcane, BlueprintSharedVendorTable priest,
            TeleportationScrollVendorPublication result, bool unchanged)
        {
            logger.Info("teleportation-spells", "scroll-vendors.published", string.Format(CultureInfo.InvariantCulture,
                "Arcane supplier: {0}{1}; priest supplier: {2}; changed tables: {3}; enabled={4}; unchanged={5}.",
                arcane == null ? "unavailable" : arcane.name, fallback ? " (Hassuf fallback)" : string.Empty,
                priest == null ? "unavailable" : priest.name,
                result.ChangedTableCount, publish, unchanged));
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

        private void Validate(List<KeyValuePair<BlueprintSharedVendorTable, KeyValuePair<BlueprintItem[], int[]>>> batches,
            TeleportationScrollBlueprintSet scrolls, bool publish)
        {
            if (publish)
            {
                foreach (var batch in batches)
                {
                    var items = batch.Value.Key;
                    var counts = batch.Value.Value;
                    for (int index = 0; index < items.Length; index++)
                        RequireExact(batch.Key, items[index], counts[index]);
                }
            }
            else
            {
                foreach (var batch in batches)
                    foreach (var scroll in new BlueprintItem[] { scrolls.Teleport, scrolls.GreaterTeleport, scrolls.WordOfRecall })
                        if (batch.Key.ComponentsArray.OfType<LootItemsPackFixed>().Any(component =>
                            ReferenceEquals(CapitalVendorBlueprints.ReadItem(component), scroll)))
                            throw new InvalidOperationException("Disabled scroll stock remains on " + batch.Key.name);
            }
        }

        private static void RequireExact(BlueprintSharedVendorTable table, BlueprintItem item, int count)
        {
            var found = table.ComponentsArray.OfType<LootItemsPackFixed>()
                .Where(component => ReferenceEquals(CapitalVendorBlueprints.ReadItem(component), item)).ToArray();
            if (found.Length != 1 || CapitalVendorBlueprints.ReadCount(found[0]) != count)
                throw new InvalidOperationException("Scroll stock normalization differs on " + table.name);
        }

        private static BlueprintSharedVendorTable FindTable(LibraryScriptableObject library, string guid, string expectedName)
        {
            // An absent table is a genuinely unavailable optional supplier (its
            // area content is not loaded), never a publication failure for the
            // other supplier.
            var table = library.BlueprintsByAssetId.TryGetValue(guid, out var found) &&
                found is BlueprintSharedVendorTable candidate && string.Equals(candidate.name, expectedName, StringComparison.Ordinal) ?
                candidate : null;
            if (table == null && found != null)
                throw new InvalidOperationException("Vendor table GUID/name mismatch: " + found.name + ":" + guid);
            return table;
        }

        internal void Rollback()
        {
            foreach (var entry in _before) entry.Key.ComponentsArray = entry.Value;
            _before.Clear();
        }
    }
}
