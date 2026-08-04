using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Blueprints
{
    internal static class CapitalVendorBlueprints
    {
        internal const string TableGuid = "afa2c7f292b8e1c4d9c835f0e8047dd3";
        internal const int WeaponCount = 1;
        internal const int ConsumableCount = 99;
        internal const int AmmunitionCount = 200;
        private const BindingFlags Flags = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        internal static CapitalVendorPublication Publish(
            LibraryScriptableObject library,
            ProductionFirearmBlueprintCatalog firearms,
            BasicAmmunitionBlueprintSet ammunition,
            BlueprintItem repairKit,
            GunsmithingSupplyBlueprintSet gunsmithingSupplies,
            ModLogger logger)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (firearms == null) throw new ArgumentNullException("firearms");
            if (ammunition == null) throw new ArgumentNullException("ammunition");
            if (repairKit == null) throw new ArgumentNullException("repairKit");
            if (gunsmithingSupplies == null) throw new ArgumentNullException("gunsmithingSupplies");
            if (logger == null) throw new ArgumentNullException("logger");

            BlueprintSharedVendorTable table =
                BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(
                    library, TableGuid, "native capital Jhod vendor table");
            BlueprintItem[] items =
            {
                firearms.Pistol.Item,
                firearms.Musket.Item,
                firearms.Blunderbuss.Item,
                firearms.AdvancedRifle.Item,
                firearms.AdvancedRevolver.Item,
                ammunition.BlackPowder,
                ammunition.LeadBall,
                repairKit,
                gunsmithingSupplies.OverhaulKit,
                gunsmithingSupplies.GunsmithKit
            };
            int[] counts =
            {
                WeaponCount, WeaponCount, WeaponCount, WeaponCount, WeaponCount,
                AmmunitionCount, AmmunitionCount, 10, 5, WeaponCount
            };
            BlueprintComponent[] existing = table.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            int[] matches = items.Select(item => existing.OfType<LootItemsPackFixed>()
                .Count(component => ReferenceEquals(ReadItem(component), item))).ToArray();
            if (matches.Any(value => value > 1) ||
                (matches.Any(value => value == 1) && matches.Any(value => value == 0)))
                throw new InvalidOperationException(
                    "The capital vendor contains a duplicate or partial Gunslinger publication.");
            if (matches.All(value => value == 1))
                return CapitalVendorPublication.Unchanged(table, existing, items, counts);

            BlueprintComponent[] additions = items.Select((item, index) =>
                CreateFixedEntry(item, counts[index])).Cast<BlueprintComponent>().ToArray();
            VendorCatalogPublication<BlueprintComponent> transaction =
                VendorCatalogPublication<BlueprintComponent>.Create(existing, additions);
            table.ComponentsArray = transaction.Published;
            var publication = new CapitalVendorPublication(
                table, transaction, items, counts, true);
            publication.Validate();
            logger.Info("acquisition", "capital-vendor.published",
                string.Format(CultureInfo.InvariantCulture,
                    "Appended {0} Gunslinger entries to {1} ({2}); weapons={3}, consumables={4}; Blunderbuss included.",
                    items.Length, table.name, TableGuid, WeaponCount, ConsumableCount));
            return publication;
        }

        internal static BlueprintItem ReadItem(LootItemsPackFixed component)
        {
            if (component == null) return null;
            FieldInfo field = typeof(LootItemsPackFixed).GetField("m_Item", Flags);
            object lootItem = field == null ? null : field.GetValue(component);
            PropertyInfo property = lootItem == null ? null :
                lootItem.GetType().GetProperty("Item", Flags);
            return property == null ? null :
                property.GetValue(lootItem, null) as BlueprintItem;
        }

        internal static int ReadCount(LootItemsPackFixed component)
        {
            FieldInfo field = typeof(LootItemsPackFixed).GetField("m_Count", Flags);
            if (field == null) throw new MissingFieldException(
                typeof(LootItemsPackFixed).FullName, "m_Count");
            return (int)field.GetValue(component);
        }

        internal static LootItemsPackFixed CreateFixedEntry(BlueprintItem item, int count)
        {
            if (item == null) throw new ArgumentNullException("item");
            FieldInfo wrapperItem = typeof(LootItem).GetField("m_Item", Flags);
            FieldInfo fixedItem = typeof(LootItemsPackFixed).GetField("m_Item", Flags);
            FieldInfo fixedCount = typeof(LootItemsPackFixed).GetField("m_Count", Flags);
            if (wrapperItem == null || fixedItem == null || fixedCount == null)
                throw new InvalidOperationException(
                    "The installed fixed vendor-entry field contract is unavailable.");
            var wrapper = new LootItem();
            wrapperItem.SetValue(wrapper, item);
            var component = new LootItemsPackFixed();
            component.name = "$KMG_CapitalVendor_" + item.name;
            fixedItem.SetValue(component, wrapper);
            fixedCount.SetValue(component, count);
            if (!ReferenceEquals(ReadItem(component), item) || ReadCount(component) != count)
                throw new InvalidOperationException(
                    "A detached fixed vendor entry did not round-trip exactly.");
            return component;
        }
    }

    internal sealed class CapitalVendorPublication
    {
        private readonly BlueprintSharedVendorTable _table;
        private readonly VendorCatalogPublication<BlueprintComponent> _transaction;
        private readonly BlueprintItem[] _items;
        private readonly int[] _counts;

        internal CapitalVendorPublication(BlueprintSharedVendorTable table,
            VendorCatalogPublication<BlueprintComponent> transaction,
            BlueprintItem[] items, int[] counts, bool changed)
        {
            _table = table ?? throw new ArgumentNullException("table");
            _transaction = transaction ?? throw new ArgumentNullException("transaction");
            _items = items ?? throw new ArgumentNullException("items");
            _counts = counts ?? throw new ArgumentNullException("counts");
            Changed = changed;
        }

        internal bool Changed { get; private set; }

        internal static CapitalVendorPublication Unchanged(
            BlueprintSharedVendorTable table, BlueprintComponent[] existing,
            BlueprintItem[] items, int[] counts)
        {
            return new CapitalVendorPublication(table,
                VendorCatalogPublication<BlueprintComponent>.Create(existing,
                    Array.Empty<BlueprintComponent>()), items, counts, false);
        }

        internal void Validate()
        {
            BlueprintComponent[] components = _table.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            for (int index = 0; index < _items.Length; index++)
            {
                LootItemsPackFixed[] matches = components.OfType<LootItemsPackFixed>()
                    .Where(value => ReferenceEquals(
                        CapitalVendorBlueprints.ReadItem(value), _items[index])).ToArray();
                if (matches.Length != 1 ||
                    CapitalVendorBlueprints.ReadCount(matches[0]) != _counts[index])
                    throw new InvalidOperationException(
                        "The capital vendor publication failed exact validation.");
            }
        }

        internal void Rollback()
        {
            if (!Changed) return;
            BlueprintComponent[] published = _transaction.Published;
            BlueprintComponent[] current = _table.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            if (current.Length != published.Length || current.Where((value, index) =>
                !ReferenceEquals(value, published[index])).Any())
                throw new InvalidOperationException(
                    "Capital vendor rollback refused because the table changed after publication.");
            _table.ComponentsArray = _transaction.Rollback();
            Changed = false;
        }
    }
}
