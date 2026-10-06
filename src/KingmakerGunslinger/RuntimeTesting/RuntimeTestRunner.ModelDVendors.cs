using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // No save or merchant UI is used. Fixture normalization operates only on
        // detached arrays of real native components, never registered tables.
        private RuntimeTestResult RunModelDVendorObservation()
        {
            var checks = new List<RuntimeTestAssertion>();
            var library = BlueprintBootstrap.Library;
            var firearms = BlueprintBootstrap.ProductionFirearms;
            var magic = BlueprintBootstrap.MagicFirearms;
            var ammo = BlueprintBootstrap.BasicAmmunition;
            var supplies = BlueprintBootstrap.GunsmithingSupplies;
            BlueprintItem[] suppliesOwned = OlegFirearmVendorBlueprints.Owned(ammo,
                BlueprintBootstrap.FirearmRepairKit, supplies);
            BlueprintItem[] gunOwned = firearms.Entries.Select(e => (BlueprintItem)e.Item)
                .Concat(magic.Entries.Select(e => (BlueprintItem)e.Item)).Concat(suppliesOwned).ToArray();
            BlueprintItem[] progression = BlueprintBootstrap.ProgressionWeapons.Entries.Select(e => (BlueprintItem)e.Item).ToArray();
            BlueprintItem[] eastern = BlueprintBootstrap.EasternWeapons.Entries.Select(e => (BlueprintItem)e.Item).ToArray();
            BlueprintItem[] spears = BlueprintBootstrap.ElvenBranchedSpears.Entries.Select(e => (BlueprintItem)e.Item).ToArray();
            BlueprintItem[] olegItems = { firearms.Pistol.Item, firearms.Musket.Item, firearms.Blunderbuss.Item, ammo.BlackPowder, ammo.LeadBall };
            BlueprintItem[] capitalItems = { magic.Require(MagicFirearmBlueprints.PistolPlus1Symbol).Item,
                magic.Require(MagicFirearmBlueprints.MusketPlus1Symbol).Item,
                magic.Require(MagicFirearmBlueprints.BlunderbussPlus1Symbol).Item,
                ammo.BlackPowder, ammo.LeadBall, ammo.PaperCartridge, supplies.GunsmithKit };
            BlueprintItem[] bokkenItems = { ammo.BlackPowder, ammo.LeadBall, ammo.PaperCartridge };
            var oleg = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(library, OlegFirearmVendorBlueprints.TableGuid, "Model D Oleg");
            var capital = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(library, CapitalVendorBlueprints.TableGuid, "Model D capital");
            var bokken = BlueprintLibraryLookup.RequireExact<BlueprintUnitLoot>(library, BokkenFirearmSupplyVendorBlueprints.TableGuid, "Model D Bokken");
            var regional = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(library, "f072a8f6889b5f345b7f4e7c74cb3e4c", "Model D Dire Narlmarches");
            ObserveModelDStock(checks, "oleg", oleg, OlegFirearmVendorBlueprints.ExpectedTableName,
                gunOwned.Concat(progression).ToArray(), olegItems, new[] { 1, 1, 1, 50, 50 });
            ObserveModelDStock(checks, "capital", capital, CapitalVendorBlueprints.ExpectedTableName,
                gunOwned.Concat(progression).Concat(eastern).Concat(spears).ToArray(), capitalItems, new[] { 1, 1, 1, 200, 200, 200, 1 });
            ObserveModelDStock(checks, "bokken", bokken, BokkenFirearmSupplyVendorBlueprints.ExpectedTableName,
                gunOwned.Concat(progression).ToArray(), bokkenItems, new[] { 100, 100, 100 });
            ObserveModelDStock(checks, "regional-eastern", regional, "DireNarlmarchesVillageVendorTable",
                eastern, eastern, Enumerable.Repeat(1, 12).ToArray());
            ObserveModelDStock(checks, "regional-spear", regional, "DireNarlmarchesVillageVendorTable",
                spears, spears, Enumerable.Repeat(1, 6).ToArray());
            // Verify both retained early thematic rows and the other regional path.
            var eastEarly = BlueprintBootstrap.EasternWeapons.Entries.Where(e =>
                e.Spec.Kind == KingmakerGunslinger.EasternWeapons.EasternWeaponGenericKind.Mundane ||
                e.Spec.Kind == KingmakerGunslinger.EasternWeapons.EasternWeaponGenericKind.Masterwork).Select(e => (BlueprintItem)e.Item).ToArray();
            var spearEarly = BlueprintBootstrap.ElvenBranchedSpears.Entries.Where(e =>
                e.Spec.Kind == KingmakerGunslinger.ElvenBranchedSpear.ElvenBranchedSpearItemKind.Mundane ||
                e.Spec.Kind == KingmakerGunslinger.ElvenBranchedSpear.ElvenBranchedSpearItemKind.Masterwork ||
                e.Spec.Kind == KingmakerGunslinger.ElvenBranchedSpear.ElvenBranchedSpearItemKind.ColdIron ||
                e.Spec.Kind == KingmakerGunslinger.ElvenBranchedSpear.ElvenBranchedSpearItemKind.MasterworkColdIron).Select(e => (BlueprintItem)e.Item).ToArray();
            ObserveModelDStock(checks, "oleg-eastern-preserved", oleg, oleg.name, eastern, eastEarly, Enumerable.Repeat(1, 6).ToArray());
            ObserveModelDStock(checks, "oleg-spear-preserved", oleg, oleg.name, spears, spearEarly, Enumerable.Repeat(1, 4).ToArray());
            var pitax = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(library, "e5ab1fccf37c55f41a20a80c6ba6a460", "Model D Pitax");
            ObserveModelDStock(checks, "pitax-eastern-preserved", pitax, "PitaxTownVendorTable", eastern, eastern, Enumerable.Repeat(1, 12).ToArray());
            ObserveModelDStock(checks, "pitax-spear-preserved", pitax, "PitaxTownVendorTable", spears, spears, Enumerable.Repeat(1, 6).ToArray());
            if (checks.Any(c => c.Status != "PASS")) return CreateResult("FAIL", checks, null);

            BlueprintComponent[] olegBefore = oleg.ComponentsArray, capitalBefore = capital.ComponentsArray, bokkenBefore = bokken.ComponentsArray;
            var olegAgain = OlegFirearmVendorBlueprints.Publish(library, firearms, magic, ammo,
                BlueprintBootstrap.FirearmRepairKit, supplies, true, _context.Logger);
            var capitalAgain = CapitalVendorBlueprints.Publish(library, firearms, magic, ammo,
                BlueprintBootstrap.FirearmRepairKit, supplies, true, BlueprintBootstrap.EasternWeapons,
                BlueprintBootstrap.ElvenBranchedSpears, BlueprintBootstrap.CordOfStubbornResolve, _context.Logger);
            var bokkenAgain = BokkenFirearmSupplyVendorBlueprints.Publish(library, ammo,
                BlueprintBootstrap.FirearmRepairKit, supplies, true, _context.Logger);
            checks.Add(Assertion("model-d-repeat-initialization", "no changes; same registered arrays",
                "oleg=" + olegAgain.Changed + ";capital=" + capitalAgain.Changed + ";bokken=" + bokkenAgain.Changed,
                !olegAgain.Changed && !capitalAgain.Changed && !bokkenAgain.Changed &&
                ReferenceEquals(olegBefore, oleg.ComponentsArray) && ReferenceEquals(capitalBefore, capital.ComponentsArray) &&
                ReferenceEquals(bokkenBefore, bokken.ComponentsArray), "real publishers called twice; exact registered table references"));

            var foreign = ScriptableObject.CreateInstance<BlueprintItemWeapon>();
            foreign.name = "KMG_RequestLocal_ModelD_ForeignControl";
            try
            {
                BlueprintComponent native = capitalBefore.First(c => !(c is LootItemsPackFixed) ||
                    !gunOwned.Concat(eastern).Concat(spears).Contains(CapitalVendorBlueprints.ReadItem(c as LootItemsPackFixed)));
                ObserveModelDMigration(checks, "oleg-migration", gunOwned, suppliesOwned, olegItems, new[] { 1, 1, 1, 50, 50 }, native, foreign);
                ObserveModelDMigration(checks, "capital-migration", gunOwned.Concat(eastern).Concat(spears).ToArray(),
                    capitalItems.Concat(new BlueprintItem[] { firearms.Pistol.Item, firearms.Musket.Item, firearms.Blunderbuss.Item }).Concat(eastern).Concat(spears).ToArray(),
                    capitalItems, new[] { 1, 1, 1, 200, 200, 200, 1 }, native, foreign);
                ObserveModelDMigration(checks, "bokken-migration", suppliesOwned,
                    bokkenItems.Concat(new[] { supplies.GunsmithKit }).ToArray(), bokkenItems, new[] { 100, 100, 100 }, native, foreign);
            }
            finally { UnityEngine.Object.DestroyImmediate(foreign); }
            checks.Add(Assertion("model-d-detached-fixture-cleanup", "registered tables unchanged; fixture destroyed",
                "no registered-table mutation, inventory access or save load",
                foreign == null && ReferenceEquals(olegBefore, oleg.ComponentsArray) &&
                ReferenceEquals(capitalBefore, capital.ComponentsArray) && ReferenceEquals(bokkenBefore, bokken.ComponentsArray),
                "only detached native row arrays and one unregistered item; no player acquisition path"));
            var result = CreateResult(checks.All(c => c.Status == "PASS") ? "PASS" : "FAIL", checks, null);
            result.Diagnostics.Add("Model D fixed stock can deplete; native table regeneration is not guaranteed. No save, UI input, merchant navigation, inventory mutation or renewable supply is involved.");
            return result;
        }

        private static void ObserveModelDStock(List<RuntimeTestAssertion> checks, string key,
            BlueprintScriptableObject table, string name, BlueprintItem[] owned, BlueprintItem[] desired, int[] counts)
        {
            var rows = (table.ComponentsArray ?? Array.Empty<BlueprintComponent>()).OfType<LootItemsPackFixed>()
                .Where(row => owned.Contains(CapitalVendorBlueprints.ReadItem(row))).ToArray();
            bool exact = table.name == name && ModelDRowsExact(rows, desired, counts);
            string observed = "table=" + table.AssetGuid + ":" + table.name + ";rows=" + string.Join("|", rows.Select(row =>
                CapitalVendorBlueprints.ReadItem(row).AssetGuid + ":" + CapitalVendorBlueprints.ReadItem(row).name + "*" + CapitalVendorBlueprints.ReadCount(row)).ToArray());
            checks.Add(Assertion("model-d-" + key, "exact " + desired.Length + " rows; counts=" + string.Join(",", counts), observed, exact,
                "registered native table; exact blueprint references, including absence of every excluded owned item"));
        }

        private static bool ModelDRowsExact(LootItemsPackFixed[] rows, BlueprintItem[] items, int[] counts)
        {
            return items.Length == counts.Length && rows.Length == items.Length && items.Select((item, index) =>
                rows.Count(row => ReferenceEquals(CapitalVendorBlueprints.ReadItem(row), item) &&
                    CapitalVendorBlueprints.ReadCount(row) == counts[index]) == 1).All(value => value);
        }

        private static void ObserveModelDMigration(List<RuntimeTestAssertion> checks, string key, BlueprintItem[] owned,
            BlueprintItem[] previous, BlueprintItem[] desired, int[] counts, BlueprintComponent native, BlueprintItem foreign)
        {
            var foreignRow = CapitalVendorBlueprints.CreateFixedEntry(foreign, 37);
            var before = new[] { native, (BlueprintComponent)foreignRow }.Concat(previous.Select(item =>
                (BlueprintComponent)CapitalVendorBlueprints.CreateFixedEntry(item, 99))).ToArray();
            Func<BlueprintComponent[], BlueprintItem[], int[], VendorCatalogPublication<BlueprintComponent>> normalize = (rows, items, quantities) =>
                VendorCatalogPublication<BlueprintComponent>.NormalizeOwned(rows, owned, items, quantities,
                    row => CapitalVendorBlueprints.ReadItem(row as LootItemsPackFixed),
                    row => CapitalVendorBlueprints.ReadCount((LootItemsPackFixed)row),
                    (item, count) => CapitalVendorBlueprints.CreateFixedEntry(item, count));
            var tx = normalize(before, desired, counts);
            var after = tx.Published;
            var ownedRows = after.OfType<LootItemsPackFixed>().Where(row => owned.Contains(CapitalVendorBlueprints.ReadItem(row))).ToArray();
            checks.Add(Assertion(key + "-old-shape", "old rows normalized to exact Model D stock", "before=" + before.Length + ";after=" + after.Length,
                ModelDRowsExact(ownedRows, desired, counts), "real native LootItemsPackFixed/LootItem round-trip; production normalizer"));
            checks.Add(Assertion(key + "-preserves-native-foreign", "same two unrelated rows in original order; foreign count 37", "retained=" + (after.Length - ownedRows.Length),
                ReferenceEquals(native, after[0]) && ReferenceEquals(foreignRow, after[1]) && CapitalVendorBlueprints.ReadCount(foreignRow) == 37,
                "unknown unregistered foreign blueprint and native registered component"));
            checks.Add(Assertion(key + "-idempotent", "second normalization unchanged", "changed=" + normalize(after, desired, counts).Changed,
                !normalize(after, desired, counts).Changed, "same real native rows; no duplicated publication path"));
            var off = normalize(after, Array.Empty<BlueprintItem>(), Array.Empty<int>());
            checks.Add(Assertion(key + "-module-off", "only native and foreign rows remain", "rows=" + off.Published.Length,
                off.Published.Length == 2 && ReferenceEquals(native, off.Published[0]) && ReferenceEquals(foreignRow, off.Published[1]),
                "disabled desired set cleans exact owned identities only"));
            var restored = tx.Rollback();
            checks.Add(Assertion(key + "-rollback", "exact original references restored", "rows=" + restored.Length,
                restored.SequenceEqual(before), "transaction rollback of detached arrays; no save or inventory mutation"));
        }
    }
}
