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
    internal static class BokkenAmmunitionVendorBlueprints
    {
        internal const string TableGuid = "4778ecb5df5d48742b9be5a204ed4657";
        internal const string ExpectedTableName = "C11_BokkenVendorTable";
        internal const string BokkenOwnerGuid =
            "4f5acdb403f6ef642959f6bedc051ac7";
        internal const string BokkenOwnerName = "OTP_Bokken";
        internal const string ZeroStateOwnerGuid =
            "57f84fdde3cc2994284fb3acc4a3cb97";
        internal const string ZeroStateOwnerName = "OTP_Bokken_ZeroState";
        internal const int AmmunitionCount = 100;

        internal static BokkenVendorPublication Publish(
            LibraryScriptableObject library,
            BasicAmmunitionBlueprintSet ammunition, bool publish,
            ModLogger logger)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (ammunition == null) throw new ArgumentNullException("ammunition");
            if (logger == null) throw new ArgumentNullException("logger");

            BlueprintUnitLoot table =
                BlueprintLibraryLookup.RequireExact<BlueprintUnitLoot>(library,
                    TableGuid, "native Bokken vendor loot table");
            if (!string.Equals(table.name, ExpectedTableName,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Bokken merchant GUID/name mismatch: " + table.name + ":" +
                    TableGuid);

            BlueprintItem[] owned =
            {
                ammunition.BlackPowder,
                ammunition.LeadBall,
                ammunition.PaperCartridge
            };
            BlueprintItem[] items = publish ? owned : Array.Empty<BlueprintItem>();
            int[] counts = publish ? new[]
            {
                AmmunitionCount, AmmunitionCount, AmmunitionCount
            } : Array.Empty<int>();
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
                return BokkenVendorPublication.Unchanged(table, existing, items,
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
                entry.name = "$KMG_BokkenAmmunition_" + item.name;
                return (BlueprintComponent)entry;
            }).ToArray();
            VendorCatalogPublication<BlueprintComponent> transaction =
                VendorCatalogPublication<BlueprintComponent>.Create(retained,
                    additions);
            table.ComponentsArray = transaction.Published;
            var publication = new BokkenVendorPublication(table, transaction,
                items, counts, true, existing);
            publication.Validate();
            logger.Info("acquisition", "bokken-ammunition.published",
                string.Format(CultureInfo.InvariantCulture,
                    "Normalized {0} exact ammunition rows on {1} ({2}); enabled={3}; each count={4}.",
                    items.Length, table.name, TableGuid, publish,
                    publish ? AmmunitionCount : 0));
            return publication;
        }
    }

    internal sealed class BokkenVendorPublication
    {
        private readonly BlueprintUnitLoot _table;
        private readonly VendorCatalogPublication<BlueprintComponent> _transaction;
        private readonly BlueprintItem[] _items;
        private readonly int[] _counts;
        private readonly BlueprintComponent[] _rollbackSnapshot;

        internal BokkenVendorPublication(BlueprintUnitLoot table,
            VendorCatalogPublication<BlueprintComponent> transaction,
            BlueprintItem[] items, int[] counts, bool changed,
            BlueprintComponent[] rollbackSnapshot = null)
        {
            _table = table ?? throw new ArgumentNullException("table");
            _transaction = transaction ?? throw new ArgumentNullException(
                "transaction");
            _items = items ?? throw new ArgumentNullException("items");
            _counts = counts ?? throw new ArgumentNullException("counts");
            _rollbackSnapshot = rollbackSnapshot ?? transaction.Rollback();
            Changed = changed;
        }

        internal bool Changed { get; private set; }

        internal static BokkenVendorPublication Unchanged(BlueprintUnitLoot table,
            BlueprintComponent[] existing, BlueprintItem[] items, int[] counts)
        {
            return new BokkenVendorPublication(table,
                VendorCatalogPublication<BlueprintComponent>.Create(existing,
                    Array.Empty<BlueprintComponent>()), items, counts, false);
        }

        internal void Validate()
        {
            BlueprintComponent[] components = _table.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            for (int index = 0; index < _items.Length; index++)
            {
                LootItemsPackFixed[] matches = components
                    .OfType<LootItemsPackFixed>().Where(value => ReferenceEquals(
                        CapitalVendorBlueprints.ReadItem(value), _items[index]))
                    .ToArray();
                if (matches.Length != 1 || CapitalVendorBlueprints.ReadCount(
                        matches[0]) != _counts[index])
                    throw new InvalidOperationException(
                        "The Bokken vendor publication failed exact validation.");
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
                    "Bokken vendor rollback refused because the table changed after publication.");
            _table.ComponentsArray = _rollbackSnapshot;
            Changed = false;
        }
    }
}
