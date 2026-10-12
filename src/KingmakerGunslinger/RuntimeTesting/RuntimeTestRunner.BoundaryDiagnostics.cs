using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static string BoundaryRow(BlueprintItem item, int count)
        { return item.AssetGuid + ":" + count.ToString(CultureInfo.InvariantCulture); }

        private static VendorBoundaryRows BoundaryVendor(BlueprintSharedVendorTable table,
            BlueprintItem[] owned, BlueprintItem[] expected, int[] counts,
            BlueprintItem[] historical, int[] historicalCounts, string group)
        {
            return new VendorBoundaryRows(table.AssetGuid,
                expected.Select((item, index) => BoundaryRow(item, counts[index])),
                (table.ComponentsArray ?? Array.Empty<BlueprintComponent>()).OfType<LootItemsPackFixed>()
                    .Where(row => owned.Contains(CapitalVendorBlueprints.ReadItem(row)))
                    .Select(row => BoundaryRow(CapitalVendorBlueprints.ReadItem(row), CapitalVendorBlueprints.ReadCount(row))),
                historical.Select((item, index) => BoundaryRow(item, historicalCounts[index])), group);
        }

        private VendorBoundaryRows[] ObserveBoundaryVendorRows(bool gunslinger, bool eastern, bool spear)
        {
            var rows = new List<VendorBoundaryRows>();
            var library = BlueprintBootstrap.Library;
            var smith = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(library, CapitalVendorBlueprints.TableGuid, "capital boundary stock");
            BlueprintItem[] capital = {
                BlueprintBootstrap.MagicFirearms.Require(MagicFirearmBlueprints.PistolPlus1Symbol).Item,
                BlueprintBootstrap.MagicFirearms.Require(MagicFirearmBlueprints.MusketPlus1Symbol).Item,
                BlueprintBootstrap.MagicFirearms.Require(MagicFirearmBlueprints.BlunderbussPlus1Symbol).Item,
                BlueprintBootstrap.BasicAmmunition.BlackPowder, BlueprintBootstrap.BasicAmmunition.LeadBall,
                BlueprintBootstrap.BasicAmmunition.PaperCartridge, BlueprintBootstrap.GunsmithingSupplies.GunsmithKit
            };
            var counts = new[] { 1, 1, 1, 200, 200, 200, 1 };
            var historical = new[] { (BlueprintItem)BlueprintBootstrap.ProductionFirearms.Pistol.Item,
                BlueprintBootstrap.ProductionFirearms.Musket.Item, BlueprintBootstrap.ProductionFirearms.Blunderbuss.Item }.Concat(capital).ToArray();
            rows.Add(BoundaryVendor(smith, ModuleGunslingerStockItems(), gunslinger ? capital : new BlueprintItem[0],
                gunslinger ? counts : new int[0], gunslinger ? historical : new BlueprintItem[0],
                gunslinger ? new[] { 1, 1, 1 }.Concat(counts).ToArray() : new int[0], "gunslinger"));
            var easternSet = BlueprintBootstrap.EasternWeapons;
            var easternOwned = easternSet.Entries.Select(value => (BlueprintItem)value.Item).Concat(easternSet.Named.Entries.Select(value => (BlueprintItem)value.Item)).ToArray();
            foreach (var spec in EasternWeaponCampaignBlueprints.VendorSpecs) {
                var table = library.GetAllBlueprints().OfType<BlueprintSharedVendorTable>().SingleOrDefault(value => value.AssetGuid == spec.Guid);
                if (table == null && spec.Optional) continue;
                if (table == null) throw new InvalidOperationException("Required Eastern boundary table absent: " + spec.Guid);
                var desired = eastern ? spec.GenericKinds.SelectMany(kind => easternSet.Entries.Where(value => value.Spec.Kind == kind).Select(value => (BlueprintItem)value.Item))
                    .Concat(spec.NamedKinds.Select(kind => (BlueprintItem)easternSet.Named.Require(kind).Item)).ToArray() : new BlueprintItem[0];
                var old = eastern && spec.Guid == CapitalVendorBlueprints.TableGuid ? easternSet.Entries.Select(value => (BlueprintItem)value.Item).ToArray() : desired;
                rows.Add(BoundaryVendor(table, easternOwned, desired, desired.Select(value => 1).ToArray(), old, old.Select(value => 1).ToArray(), "eastern"));
            }
            var spearSet = BlueprintBootstrap.ElvenBranchedSpears;
            var spearOwned = spearSet.Entries.Select(value => (BlueprintItem)value.Item).Concat(spearSet.Named.Entries.Select(value => (BlueprintItem)value.Item)).ToArray();
            foreach (var spec in ElvenBranchedSpearCampaignBlueprints.VendorSpecs) {
                var table = library.GetAllBlueprints().OfType<BlueprintSharedVendorTable>().SingleOrDefault(value => value.AssetGuid == spec.Guid);
                if (table == null && spec.Optional) continue;
                if (table == null) throw new InvalidOperationException("Required spear boundary table absent: " + spec.Guid);
                var desired = spear ? spec.FoundationKinds.Select(kind => (BlueprintItem)spearSet.Require(kind).Item)
                    .Concat(spec.NamedKind.HasValue ? new[] { (BlueprintItem)spearSet.Named.Require(spec.NamedKind.Value).Item } : new BlueprintItem[0]).ToArray() : new BlueprintItem[0];
                var old = spear && spec.Guid == CapitalVendorBlueprints.TableGuid ? spearSet.Entries.Select(value => (BlueprintItem)value.Item).ToArray() : desired;
                rows.Add(BoundaryVendor(table, spearOwned, desired, desired.Select(value => 1).ToArray(), old, old.Select(value => 1).ToArray(), "spear"));
            }
            var manager = _context.ModEntry.GetType().DeclaringType;
            var entries = ((IEnumerable)manager.GetField("modEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null))
                .Cast<UnityModManagerNet.UnityModManager.ModEntry>().Where(value => value != null && value.Info != null).ToArray();
            var dlc = Kingmaker.Blueprints.Root.BlueprintRoot.Instance.DlcSettings;
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "boundary-vendor-rows.json"),
                BoundaryObservationPolicy.Serialize(new { schemaVersion = 1, runId = _request.RunId,
                    phase = "guarded-unloaded-startup", modules = new { gunslinger, eastern, spear },
                    gameVersion = GameVersion.Cached,
                    mods = entries.Select(value => new { id = value.Info.Id, version = value.Info.Version, value.Loaded, value.Active, value.ErrorOnLoading,
                        assemblyMvid = value.Assembly == null ? null : value.Assembly.ManifestModule.ModuleVersionId.ToString() }).ToArray(),
                    cotwAssemblyPresent = AppDomain.CurrentDomain.GetAssemblies().Any(value => value.GetName().Name == "CallOfTheWild"),
                    dlc = new { endless = new { dlc.Endless.SteamDLC, dlc.Endless.Enabled },
                        tieflings = new { dlc.Tieflings.SteamDLC, dlc.Tieflings.Enabled },
                        varnhold = new { dlc.Varnhold.SteamDLC, dlc.Varnhold.Enabled } },
                    optionalTables = BeneathStolenLandsVendorBlueprints.TableGuids.Select(id => new { id, loaded = library.BlueprintsByAssetId.ContainsKey(id) }).ToArray(),
                    rows = rows.ToArray() }));
            return rows.ToArray();
        }
    }
}
