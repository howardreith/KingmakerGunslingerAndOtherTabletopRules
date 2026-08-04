using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class BeneathStolenLandsVendorPublication
    {
        private readonly List<CapitalVendorPublication> _tables;
        internal BeneathStolenLandsVendorPublication(List<CapitalVendorPublication> tables)
        { _tables = tables; }
        internal void Validate() { foreach (var table in _tables) table.Validate(); }
        internal void Rollback() { for (int i = _tables.Count - 1; i >= 0; i--) _tables[i].Rollback(); }
        internal int Count { get { return _tables.Count; } }
    }

    internal static class BeneathStolenLandsVendorBlueprints
    {
        internal const string StandaloneHonestGuyTableGuid = "a6bae621a7bd96b4fb3c1511cd2f9fac";
        internal const string StandaloneXellirenTableGuid = "08e090bb2038e3d47be56d8752d5dcaf";
        internal const string CampaignHonestGuyTableGuid = "45f027c06962df249b8c014a4b4e95e3";
        internal const string CampaignXellirenTableGuid = "420f1da6c2523f64eba810b9b484f60f";
        internal static readonly string[] TableGuids = {
            StandaloneHonestGuyTableGuid, StandaloneXellirenTableGuid,
            CampaignHonestGuyTableGuid, CampaignXellirenTableGuid };

        internal static BeneathStolenLandsVendorPublication Publish(
            LibraryScriptableObject library, ProductionFirearmBlueprintCatalog firearms,
            BasicAmmunitionBlueprintSet ammunition, BlueprintItem repairKit,
            GunsmithingSupplyBlueprintSet supplies, ModLogger logger)
        {
            BlueprintItem[] items = { firearms.Pistol.Item, firearms.Musket.Item,
                firearms.Blunderbuss.Item, firearms.AdvancedRifle.Item,
                firearms.AdvancedRevolver.Item, ammunition.BlackPowder,
                ammunition.LeadBall, repairKit, supplies.OverhaulKit,
                supplies.GunsmithKit };
            int[] counts = { 1, 1, 1, 1, 1, 200, 200, 10, 5, 1 };
            var publications = new List<CapitalVendorPublication>();
            try
            {
                foreach (string guid in TableGuids)
                {
                    BlueprintSharedVendorTable table = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(
                        library, guid, "exact Beneath the Stolen Lands vendor table");
                    BlueprintComponent[] existing = table.ComponentsArray ?? Array.Empty<BlueprintComponent>();
                    int[] matches = items.Select(item => existing.OfType<LootItemsPackFixed>()
                        .Count(component => ReferenceEquals(CapitalVendorBlueprints.ReadItem(component), item))).ToArray();
                    if (matches.Any(value => value > 1) ||
                        (matches.Any(value => value == 1) && matches.Any(value => value == 0)))
                        throw new InvalidOperationException("A BTSL vendor has a duplicate or partial Gunslinger publication: " + guid);
                    CapitalVendorPublication publication;
                    if (matches.All(value => value == 1))
                        publication = CapitalVendorPublication.Unchanged(table, existing, items, counts);
                    else
                    {
                        BlueprintComponent[] additions = items.Select((item, index) =>
                            (BlueprintComponent)CapitalVendorBlueprints.CreateFixedEntry(item, counts[index])).ToArray();
                        VendorCatalogPublication<BlueprintComponent> transaction =
                            VendorCatalogPublication<BlueprintComponent>.Create(existing, additions);
                        table.ComponentsArray = transaction.Published;
                        publication = new CapitalVendorPublication(table, transaction,
                            items, counts, true);
                    }
                    publication.Validate(); publications.Add(publication);
                }
                logger.Info("acquisition", "btsl-vendors.published",
                    "Published exact Gunslinger testing stock to four standalone/campaign BTSL vendor tables.");
                return new BeneathStolenLandsVendorPublication(publications);
            }
            catch
            {
                for (int i = publications.Count - 1; i >= 0; i--) publications[i].Rollback();
                throw;
            }
        }
    }
}
