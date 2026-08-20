using System;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Blueprints
{
    internal static class OlegMaintenanceVendorBlueprints
    {
        internal const string TableGuid = "f720440559fc00949900bfa1575196ac";
        internal const string ExpectedTableName = "C11_OlegVendorTable";
        internal const string OlegOwnerGuid = "5db389e0409ef534d81358555e6ab99d";
        internal const string OlegOwnerName = "OTP_Oleg";
        internal const string FirstVisitOwnerGuid =
            "67db4b8bacc69e643880f0a4ed6dff6f";
        internal const string FirstVisitOwnerName = "OTP_Oleg_FirstVisit";
        internal const int RepairKitCount = 5;
        internal const int OverhaulKitCount = 2;

        internal static CapitalVendorPublication Publish(
            LibraryScriptableObject library, BlueprintItem repairKit,
            GunsmithingSupplyBlueprintSet supplies, bool publish,
            ModLogger logger)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (repairKit == null) throw new ArgumentNullException("repairKit");
            if (supplies == null) throw new ArgumentNullException("supplies");
            if (logger == null) throw new ArgumentNullException("logger");

            BlueprintSharedVendorTable table =
                BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(
                    library, TableGuid, "native Oleg vendor table");
            if (!string.Equals(table.name, ExpectedTableName,
                    StringComparison.Ordinal))
                throw new InvalidOperationException("Oleg merchant GUID/name mismatch: " +
                    table.name + ":" + TableGuid);

            BlueprintItem[] owned = { repairKit, supplies.OverhaulKit };
            BlueprintItem[] items = publish ? owned : Array.Empty<BlueprintItem>();
            int[] counts = publish ? new[] { RepairKitCount, OverhaulKitCount } :
                Array.Empty<int>();
            BlueprintComponent[] existing = table.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            bool obsolete = existing.OfType<LootItemsPackFixed>().Any(component =>
                owned.Contains(CapitalVendorBlueprints.ReadItem(component)) &&
                !items.Contains(CapitalVendorBlueprints.ReadItem(component)));
            bool exactCounts = items.Select((item, index) => existing
                .OfType<LootItemsPackFixed>().Where(component => ReferenceEquals(
                    CapitalVendorBlueprints.ReadItem(component), item)).ToArray())
                .Select((found, index) => found.Length == 1 &&
                    CapitalVendorBlueprints.ReadCount(found[0]) == counts[index])
                .All(value => value);
            if (!obsolete && exactCounts)
                return CapitalVendorPublication.Unchanged(table, existing, items,
                    counts);

            BlueprintComponent[] retained = existing.Where(component =>
            {
                var fixedEntry = component as LootItemsPackFixed;
                return fixedEntry == null || !owned.Contains(
                    CapitalVendorBlueprints.ReadItem(fixedEntry));
            }).ToArray();
            BlueprintComponent[] additions = items.Select((item, index) =>
            {
                LootItemsPackFixed entry = CapitalVendorBlueprints.CreateFixedEntry(
                    item, counts[index]);
                entry.name = "$KMG_OlegMaintenance_" + item.name;
                return (BlueprintComponent)entry;
            }).ToArray();
            VendorCatalogPublication<BlueprintComponent> transaction =
                VendorCatalogPublication<BlueprintComponent>.Create(retained,
                    additions);
            table.ComponentsArray = transaction.Published;
            var publication = new CapitalVendorPublication(table, transaction,
                items, counts, true, existing);
            publication.Validate();
            logger.Info("acquisition", "oleg-maintenance.published",
                string.Format(CultureInfo.InvariantCulture,
                    "Normalized {0} exact maintenance rows on {1} ({2}); enabled={3}; Repair Kits={4}; Overhaul Kits={5}.",
                    items.Length, table.name, TableGuid, publish,
                    publish ? RepairKitCount : 0,
                    publish ? OverhaulKitCount : 0));
            return publication;
        }
    }
}
