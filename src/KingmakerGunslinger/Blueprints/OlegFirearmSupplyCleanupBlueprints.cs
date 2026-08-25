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
    internal static class OlegFirearmSupplyCleanupBlueprints
    {
        internal const string TableGuid = "f720440559fc00949900bfa1575196ac";
        internal const string ExpectedTableName = "C11_OlegVendorTable";
        internal const string OlegOwnerGuid = "5db389e0409ef534d81358555e6ab99d";
        internal const string OlegOwnerName = "OTP_Oleg";
        internal const string FirstVisitOwnerGuid =
            "67db4b8bacc69e643880f0a4ed6dff6f";
        internal const string FirstVisitOwnerName = "OTP_Oleg_FirstVisit";

        internal static OlegVendorCleanupPublication Normalize(
            LibraryScriptableObject library,
            BasicAmmunitionBlueprintSet ammunition, BlueprintItem repairKit,
            GunsmithingSupplyBlueprintSet supplies, bool moduleEnabled,
            ModLogger logger)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (ammunition == null) throw new ArgumentNullException("ammunition");
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

            BlueprintItem[] owned = Owned(ammunition, repairKit, supplies);
            BlueprintComponent[] existing = table.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            BlueprintComponent[] retained = existing.Where(component =>
            {
                var fixedEntry = component as LootItemsPackFixed;
                return fixedEntry == null || !owned.Contains(
                    CapitalVendorBlueprints.ReadItem(fixedEntry));
            }).ToArray();
            if (retained.Length == existing.Length)
            {
                var unchanged = OlegVendorCleanupPublication.Unchanged(table,
                    existing, owned);
                unchanged.Validate();
                return unchanged;
            }

            VendorCatalogPublication<BlueprintComponent> transaction =
                VendorCatalogPublication<BlueprintComponent>.Create(retained,
                    Array.Empty<BlueprintComponent>());
            table.ComponentsArray = transaction.Published;
            var publication = new OlegVendorCleanupPublication(table,
                transaction, owned, true, existing);
            try
            {
                publication.Validate();
                logger.Info("acquisition", "oleg-firearm-supplies.removed",
                    string.Format(CultureInfo.InvariantCulture,
                        "Removed {0} exact project-owned firearm-supply row(s) from {1} ({2}); moduleEnabled={3}.",
                        existing.Length - retained.Length, table.name, TableGuid,
                        moduleEnabled));
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
            return new[]
            {
                ammunition.BlackPowder,
                ammunition.LeadBall,
                ammunition.PaperCartridge,
                repairKit,
                supplies.OverhaulKit,
                supplies.GunsmithKit
            };
        }
    }

    internal sealed class OlegVendorCleanupPublication
    {
        private readonly BlueprintSharedVendorTable _table;
        private readonly VendorCatalogPublication<BlueprintComponent> _transaction;
        private readonly BlueprintItem[] _owned;
        private readonly BlueprintComponent[] _rollbackSnapshot;

        internal OlegVendorCleanupPublication(BlueprintSharedVendorTable table,
            VendorCatalogPublication<BlueprintComponent> transaction,
            BlueprintItem[] owned, bool changed,
            BlueprintComponent[] rollbackSnapshot = null)
        {
            _table = table ?? throw new ArgumentNullException("table");
            _transaction = transaction ?? throw new ArgumentNullException(
                "transaction");
            _owned = owned ?? throw new ArgumentNullException("owned");
            _rollbackSnapshot = rollbackSnapshot ?? transaction.Rollback();
            Changed = changed;
        }

        internal bool Changed { get; private set; }

        internal static OlegVendorCleanupPublication Unchanged(
            BlueprintSharedVendorTable table, BlueprintComponent[] existing,
            BlueprintItem[] owned)
        {
            return new OlegVendorCleanupPublication(table,
                VendorCatalogPublication<BlueprintComponent>.Create(existing,
                    Array.Empty<BlueprintComponent>()), owned, false);
        }

        internal void Validate()
        {
            BlueprintComponent[] components = _table.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            foreach (BlueprintItem item in _owned)
                if (components.OfType<LootItemsPackFixed>().Any(value =>
                        ReferenceEquals(CapitalVendorBlueprints.ReadItem(value),
                            item)))
                    throw new InvalidOperationException(
                        "The Oleg vendor table retained a project-owned firearm-supply row.");
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
                    "Oleg vendor rollback refused because the table changed after cleanup.");
            _table.ComponentsArray = _rollbackSnapshot;
            Changed = false;
        }
    }
}
