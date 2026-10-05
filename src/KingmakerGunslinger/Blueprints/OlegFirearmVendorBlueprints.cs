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
    internal static class OlegFirearmVendorBlueprints
    {
        internal const string TableGuid = "f720440559fc00949900bfa1575196ac";
        internal const string ExpectedTableName = "C11_OlegVendorTable";
        internal const string OlegOwnerGuid =
            "5db389e0409ef534d81358555e6ab99d";
        internal const string OlegOwnerName = "OTP_Oleg";
        internal const string FirstVisitOwnerGuid =
            "67db4b8bacc69e643880f0a4ed6dff6f";
        internal const string FirstVisitOwnerName = "OTP_Oleg_FirstVisit";
        internal const int AmmunitionCount = 50;

        internal static OlegVendorPublication Publish(
            LibraryScriptableObject library,
            ProductionFirearmBlueprintCatalog firearms,
            MagicFirearmBlueprintCatalog magicFirearms,
            BasicAmmunitionBlueprintSet ammunition, BlueprintItem repairKit,
            GunsmithingSupplyBlueprintSet supplies, bool publish,
            ModLogger logger)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (firearms == null || magicFirearms == null) throw new ArgumentNullException("firearms");
            if (ammunition == null) throw new ArgumentNullException("ammunition");
            if (repairKit == null) throw new ArgumentNullException("repairKit");
            if (supplies == null) throw new ArgumentNullException("supplies");
            if (logger == null) throw new ArgumentNullException("logger");

            BlueprintSharedVendorTable table =
                BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(library,
                    TableGuid, "native Oleg vendor loot table");
            if (!string.Equals(table.name, ExpectedTableName,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Oleg merchant GUID/name mismatch: " + table.name + ":" +
                    TableGuid);

            // The obsolete consumable Firearm Repair Kit and Firearm Overhaul Kit stay
            // in the owned set so previously injected rows are cleaned up, but they are
            // never offered again.
            BlueprintItem[] owned = firearms.Entries.Select(value => (BlueprintItem)value.Item)
                .Concat(magicFirearms.Entries.Select(value => (BlueprintItem)value.Item))
                .Concat(Owned(ammunition, repairKit, supplies)).Distinct().ToArray();
            BlueprintItem[] stocked =
            {
                firearms.Pistol.Item,
                firearms.Musket.Item,
                firearms.Blunderbuss.Item,
                ammunition.BlackPowder,
                ammunition.LeadBall
            };
            BlueprintItem[] items = publish ? stocked :
                Array.Empty<BlueprintItem>();
            int[] counts = publish ? new[]
            {
                1, 1, 1, AmmunitionCount, AmmunitionCount
            } : Array.Empty<int>();
            BlueprintComponent[] existing = table.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            VendorCatalogPublication<BlueprintComponent> transaction =
                VendorCatalogPublication<BlueprintComponent>.NormalizeOwned(existing,
                    owned, items, counts,
                    row => CapitalVendorBlueprints.ReadItem(row as LootItemsPackFixed),
                    row => CapitalVendorBlueprints.ReadCount((LootItemsPackFixed)row),
                    (item, count) =>
                    {
                        var entry = CapitalVendorBlueprints.CreateFixedEntry(item, count);
                        entry.name = "$KMG_OlegFirearmSupply_" + item.name;
                        return entry;
                    });
            if (!transaction.Changed)
            {
                var unchanged = OlegVendorPublication.Unchanged(table,
                    existing, owned, items, counts);
                unchanged.Validate();
                return unchanged;
            }
            table.ComponentsArray = transaction.Published;
            var publication = new OlegVendorPublication(table, transaction,
                owned, items, counts, true, existing);
            try
            {
                publication.Validate();
                logger.Info("acquisition", "oleg-firearm-supplies.published",
                    string.Format(CultureInfo.InvariantCulture,
                        "Normalized {0} exact firearm-supply rows on {1} ({2}); enabled={3}; ammunition={4}.",
                        items.Length, table.name, TableGuid, publish,
                        publish ? AmmunitionCount : 0));
                return publication;
            }
            catch
            {
                table.ComponentsArray = existing;
                throw;
            }
        }
        internal static BlueprintItem[] Owned(
            BasicAmmunitionBlueprintSet ammunition, BlueprintItem repairKit,
            GunsmithingSupplyBlueprintSet supplies)
        {
            return new[] { ammunition.BlackPowder, ammunition.LeadBall,
                ammunition.PaperCartridge, repairKit, supplies.OverhaulKit,
                supplies.GunsmithKit };
        }

    }

    internal sealed class OlegVendorPublication
    {
        private readonly BlueprintSharedVendorTable _table;
        private readonly VendorCatalogPublication<BlueprintComponent> _transaction;
        private readonly BlueprintItem[] _owned;
        private readonly BlueprintItem[] _items;
        private readonly int[] _counts;
        private readonly BlueprintComponent[] _rollbackSnapshot;

        internal OlegVendorPublication(BlueprintSharedVendorTable table,
            VendorCatalogPublication<BlueprintComponent> transaction,
            BlueprintItem[] owned, BlueprintItem[] items, int[] counts,
            bool changed, BlueprintComponent[] rollbackSnapshot = null)
        {
            _table = table ?? throw new ArgumentNullException("table");
            _transaction = transaction ?? throw new ArgumentNullException(
                "transaction");
            _owned = owned ?? throw new ArgumentNullException("owned");
            _items = items ?? throw new ArgumentNullException("items");
            _counts = counts ?? throw new ArgumentNullException("counts");
            _rollbackSnapshot = rollbackSnapshot ?? transaction.Rollback();
            Changed = changed;
        }

        internal bool Changed { get; private set; }

        internal static OlegVendorPublication Unchanged(BlueprintSharedVendorTable table,
            BlueprintComponent[] existing, BlueprintItem[] owned,
            BlueprintItem[] items, int[] counts)
        {
            return new OlegVendorPublication(table,
                VendorCatalogPublication<BlueprintComponent>.Create(existing,
                    Array.Empty<BlueprintComponent>()), owned, items, counts,
                    false);
        }

        internal void Validate()
        {
            BlueprintComponent[] components = _table.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            foreach (BlueprintItem owned in _owned)
            {
                int index = Array.FindIndex(_items, value =>
                    ReferenceEquals(value, owned));
                LootItemsPackFixed[] matches = components
                    .OfType<LootItemsPackFixed>().Where(value => ReferenceEquals(
                        CapitalVendorBlueprints.ReadItem(value), owned))
                    .ToArray();
                if (index < 0)
                {
                    if (matches.Length != 0)
                        throw new InvalidOperationException(
                            "The disabled Oleg publication retained a project-owned row.");
                    continue;
                }
                if (matches.Length != 1 || CapitalVendorBlueprints.ReadCount(
                        matches[0]) != _counts[index])
                    throw new InvalidOperationException(
                        "The Oleg firearm-supply publication failed exact validation.");
            }
        }

        internal void Rollback()
        {
            if (!Changed) return;
            BlueprintComponent[] published = _transaction.Published;
            BlueprintComponent[] current = _table.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            if (current.Length != published.Length || current.Where((value,
                    index) => !ReferenceEquals(value, published[index])).Any())
                throw new InvalidOperationException(
                    "Oleg vendor rollback refused because the table changed after publication.");
            _table.ComponentsArray = _rollbackSnapshot;
            Changed = false;
        }
    }
}
