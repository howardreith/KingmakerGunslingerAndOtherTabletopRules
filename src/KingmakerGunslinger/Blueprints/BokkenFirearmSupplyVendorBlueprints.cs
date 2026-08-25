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
    internal static class BokkenFirearmSupplyVendorBlueprints
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
        internal const int RepairKitCount = 5;
        internal const int OverhaulKitCount = 2;
        internal const int GunsmithKitCount = 1;

        internal static BokkenVendorPublication Publish(
            LibraryScriptableObject library,
            BasicAmmunitionBlueprintSet ammunition, BlueprintItem repairKit,
            GunsmithingSupplyBlueprintSet supplies, bool publish,
            ModLogger logger)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (ammunition == null) throw new ArgumentNullException("ammunition");
            if (repairKit == null) throw new ArgumentNullException("repairKit");
            if (supplies == null) throw new ArgumentNullException("supplies");
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
                ammunition.PaperCartridge,
                repairKit,
                supplies.OverhaulKit,
                supplies.GunsmithKit
            };
            BlueprintItem[] items = publish ? owned :
                Array.Empty<BlueprintItem>();
            int[] counts = publish ? new[]
            {
                AmmunitionCount,
                AmmunitionCount,
                AmmunitionCount,
                RepairKitCount,
                OverhaulKitCount,
                GunsmithKitCount
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
            {
                var unchanged = BokkenVendorPublication.Unchanged(table,
                    existing, owned, items, counts);
                unchanged.Validate();
                return unchanged;
            }

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
                entry.name = "$KMG_BokkenFirearmSupply_" + item.name;
                return (BlueprintComponent)entry;
            }).ToArray();
            VendorCatalogPublication<BlueprintComponent> transaction =
                VendorCatalogPublication<BlueprintComponent>.Create(retained,
                    additions);
            table.ComponentsArray = transaction.Published;
            var publication = new BokkenVendorPublication(table, transaction,
                owned, items, counts, true, existing);
            try
            {
                publication.Validate();
                logger.Info("acquisition", "bokken-firearm-supplies.published",
                    string.Format(CultureInfo.InvariantCulture,
                        "Normalized {0} exact firearm-supply rows on {1} ({2}); enabled={3}; ammunition={4}; repair={5}; overhaul={6}; gunsmith={7}.",
                        items.Length, table.name, TableGuid, publish,
                        publish ? AmmunitionCount : 0,
                        publish ? RepairKitCount : 0,
                        publish ? OverhaulKitCount : 0,
                        publish ? GunsmithKitCount : 0));
                return publication;
            }
            catch
            {
                table.ComponentsArray = existing;
                throw;
            }
        }
    }

    internal sealed class BokkenVendorPublication
    {
        private readonly BlueprintUnitLoot _table;
        private readonly VendorCatalogPublication<BlueprintComponent> _transaction;
        private readonly BlueprintItem[] _owned;
        private readonly BlueprintItem[] _items;
        private readonly int[] _counts;
        private readonly BlueprintComponent[] _rollbackSnapshot;

        internal BokkenVendorPublication(BlueprintUnitLoot table,
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

        internal static BokkenVendorPublication Unchanged(BlueprintUnitLoot table,
            BlueprintComponent[] existing, BlueprintItem[] owned,
            BlueprintItem[] items, int[] counts)
        {
            return new BokkenVendorPublication(table,
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
                            "The disabled Bokken publication retained a project-owned row.");
                    continue;
                }
                if (matches.Length != 1 || CapitalVendorBlueprints.ReadCount(
                        matches[0]) != _counts[index])
                    throw new InvalidOperationException(
                        "The Bokken firearm-supply publication failed exact validation.");
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
