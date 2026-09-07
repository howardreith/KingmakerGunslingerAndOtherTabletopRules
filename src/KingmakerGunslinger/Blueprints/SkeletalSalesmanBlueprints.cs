using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Blueprints
{
    internal static class SkeletalSalesmanBlueprints
    {
        internal static SkeletalSalesmanPublication Publish(LibraryScriptableObject library,
            MagicFirearmBlueprintCatalog catalog, ModLogger logger)
        {
            if (library == null || catalog == null || logger == null)
                throw new ArgumentNullException("Skeletal Salesman publication inputs are incomplete.");
            BlueprintUnit merchant = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(
                library, SkeletalSalesmanStockCatalog.UnitGuid, "Skeletal Salesman unit");
            if (!string.Equals(merchant.name, SkeletalSalesmanStockCatalog.UnitName,
                StringComparison.Ordinal))
                throw new InvalidOperationException("Skeletal Salesman unit identity mismatch.");
            BlueprintItem[] items = MidgameFirearmCatalog.Entries.Select(spec =>
                (BlueprintItem)catalog.Require(spec.Symbol).Item).ToArray();
            int[] counts = items.Select(item => SkeletalSalesmanStockCatalog.CopiesPerStock).ToArray();
            var publications = new List<CapitalVendorPublication>();
            try
            {
                foreach (SkeletalSalesmanStockTarget target in SkeletalSalesmanStockCatalog.Targets)
                {
                    BlueprintSharedVendorTable table = BlueprintLibraryLookup
                        .RequireExact<BlueprintSharedVendorTable>(library, target.Guid,
                            "Skeletal Salesman stock " + target.Name);
                    if (!string.Equals(table.name, target.Name, StringComparison.Ordinal))
                        throw new InvalidOperationException("Skeletal Salesman stock identity mismatch: " + target.Guid);
                    BlueprintComponent[] existing = table.ComponentsArray ?? Array.Empty<BlueprintComponent>();
                    var additions = new List<BlueprintComponent>();
                    foreach (BlueprintItem item in items)
                    {
                        LootItemsPackFixed[] matches = existing.OfType<LootItemsPackFixed>()
                            .Where(entry => ReferenceEquals(CapitalVendorBlueprints.ReadItem(entry), item)).ToArray();
                        if (matches.Length > 1 || (matches.Length == 1 &&
                            CapitalVendorBlueprints.ReadCount(matches[0]) != SkeletalSalesmanStockCatalog.CopiesPerStock))
                            throw new InvalidOperationException("Ambiguous named firearm stock: " + target.Name + ";" + item.name);
                        if (matches.Length == 0)
                        {
                            LootItemsPackFixed addition = CapitalVendorBlueprints.CreateFixedEntry(
                                item, SkeletalSalesmanStockCatalog.CopiesPerStock);
                            addition.name = "$KMG_SkeletalSalesman_" + item.name;
                            additions.Add(addition);
                        }
                    }
                    VendorCatalogPublication<BlueprintComponent> transaction =
                        VendorCatalogPublication<BlueprintComponent>.CreateIntegrated(
                            existing, additions.ToArray(), ReadVisibleSortKey);
                    var publication = new CapitalVendorPublication(table, transaction,
                        items, counts, transaction.Changed, existing);
                    if (transaction.Changed) table.ComponentsArray = transaction.Published;
                    publications.Add(publication);
                    publication.Validate();
                }
                logger.Info("acquisition", "skeletal-salesman.published",
                    "Published one Roadwarden and one Dead Reckoning in each C3/C4 stock variant. Native UnitPartVendor known-item reconciliation retains purchases on load.");
                return new SkeletalSalesmanPublication(publications);
            }
            catch
            {
                for (int i = publications.Count - 1; i >= 0; i--) publications[i].Rollback();
                throw;
            }
        }

        // Scoped insertion only: keep all existing rows in their original relative order.
        // Desktop VendorUI independently defaults to native item type + localized name;
        // its price/name sorts and weapon filter remain entirely native.
        internal static string ReadVisibleSortKey(BlueprintComponent component)
        {
            BlueprintItem item = CapitalVendorBlueprints.ReadItem(component as LootItemsPackFixed);
            return item == null ? null : ((int)item.ItemType).ToString("D4",
                CultureInfo.InvariantCulture) + ":" + item.Name;
        }
    }

    internal sealed class SkeletalSalesmanPublication
    {
        private readonly List<CapitalVendorPublication> _tables;
        internal SkeletalSalesmanPublication(List<CapitalVendorPublication> tables)
        { _tables = tables; }
        internal int Count { get { return _tables.Count; } }
        internal void Validate() { foreach (var table in _tables) table.Validate(); }
        internal void Rollback() { for (int i = _tables.Count - 1; i >= 0; i--) _tables[i].Rollback(); }
    }
}
