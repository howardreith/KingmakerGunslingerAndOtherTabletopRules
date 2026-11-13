using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // No save or merchant UI is used. Model D normalization operates only on
        // detached arrays of real native components. The regional withdrawal
        // check alone injects stale rows into the registered Dire Narlmarches
        // and Pitax tables in memory and restores the exact array in finally.
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
            // Owner acquisition decision (2026-11-12): neither regional trader
            // stocks an authored Eastern weapon or branched spear. An empty
            // desired set asserts the absence of every owned generic identity.
            var pitax = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(library, "e5ab1fccf37c55f41a20a80c6ba6a460", "Model D Pitax");
            ObserveModelDStock(checks, "regional-eastern-retired", regional, "DireNarlmarchesVillageVendorTable",
                eastern, Array.Empty<BlueprintItem>(), Array.Empty<int>());
            ObserveModelDStock(checks, "regional-spear-retired", regional, "DireNarlmarchesVillageVendorTable",
                spears, Array.Empty<BlueprintItem>(), Array.Empty<int>());
            ObserveModelDStock(checks, "pitax-eastern-retired", pitax, "PitaxTownVendorTable",
                eastern, Array.Empty<BlueprintItem>(), Array.Empty<int>());
            ObserveModelDStock(checks, "pitax-spear-retired", pitax, "PitaxTownVendorTable",
                spears, Array.Empty<BlueprintItem>(), Array.Empty<int>());
            // Oleg keeps ordinary/masterwork Eastern weapons and, since the
            // 2026-11-11 cold iron removal, only the ordinary and masterwork spears.
            var eastEarly = BlueprintBootstrap.EasternWeapons.Entries.Where(e =>
                e.Spec.Kind == KingmakerGunslinger.EasternWeapons.EasternWeaponGenericKind.Mundane ||
                e.Spec.Kind == KingmakerGunslinger.EasternWeapons.EasternWeaponGenericKind.Masterwork).Select(e => (BlueprintItem)e.Item).ToArray();
            var spearEarly = BlueprintBootstrap.ElvenBranchedSpears.Entries.Where(e =>
                e.Spec.Kind == KingmakerGunslinger.ElvenBranchedSpear.ElvenBranchedSpearItemKind.Mundane ||
                e.Spec.Kind == KingmakerGunslinger.ElvenBranchedSpear.ElvenBranchedSpearItemKind.Masterwork).Select(e => (BlueprintItem)e.Item).ToArray();
            ObserveModelDStock(checks, "oleg-eastern-preserved", oleg, oleg.name, eastern, eastEarly, Enumerable.Repeat(1, 6).ToArray());
            ObserveModelDStock(checks, "oleg-spear-preserved", oleg, oleg.name, spears, spearEarly, Enumerable.Repeat(1, 2).ToArray());
            if (checks.Any(c => c.Status != "PASS")) return CreateResult("FAIL", checks, null);
            ObserveRegionalWithdrawal(checks, regional, eastern, spears);
            ObserveRegionalWithdrawal(checks, pitax, eastern, spears);
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

        // Regional withdrawal (2026-11-12), observed on the registered native
        // table without a save, player, merchant UI or inventory:
        // 1. The real campaign publishers, called again, leave the registered
        //    array untouched. Stale owned rows injected in memory are removed
        //    while the native rows and a foreign row keep identity, order and
        //    count. The original array reference is restored in finally.
        // 2. The native 2.1.7b SharedVendorTables reconciliation statics
        //    (GetFixedItems, GetLootDifference, UpdateLootItems) run on detached
        //    persisted-entry lists shaped like older saves. The GetTable wiring
        //    around them (once per loaded game, then KnownItems = current fixed
        //    rows) is source-derived, not exercised here.
        private void ObserveRegionalWithdrawal(List<RuntimeTestAssertion> checks,
            BlueprintSharedVendorTable table, BlueprintItem[] eastern, BlueprintItem[] spears)
        {
            string key = "model-d-" + (table.name == "PitaxTownVendorTable" ? "pitax" : "regional");
            var library = BlueprintBootstrap.Library;
            BlueprintItem[] retired = eastern.Concat(spears).ToArray();
            BlueprintComponent[] registered = table.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            Action republish = () =>
            {
                ElvenBranchedSpearCampaignBlueprints.Publish(library, BlueprintBootstrap.ElvenBranchedSpears, _context.Logger);
                EasternWeaponCampaignBlueprints.Publish(library, BlueprintBootstrap.EasternWeapons, _context.Logger);
            };
            republish();
            checks.Add(Assertion(key + "-repeat-publication", "same registered array reference",
                "same=" + ReferenceEquals(registered, table.ComponentsArray),
                ReferenceEquals(registered, table.ComponentsArray),
                "real Eastern and spear campaign publishers called again after bootstrap"));

            var foreign = ScriptableObject.CreateInstance<BlueprintItemWeapon>();
            foreign.name = "KMG_RequestLocal_RegionalWithdrawal_ForeignControl";
            var foreignRow = CapitalVendorBlueprints.CreateFixedEntry(foreign, 37);
            BlueprintComponent[] expected = registered.Concat(new BlueprintComponent[] { foreignRow }).ToArray();
            try
            {
                table.ComponentsArray = expected.Concat(retired.Select(item =>
                    (BlueprintComponent)CapitalVendorBlueprints.CreateFixedEntry(item, 1))).ToArray();
                republish();
                BlueprintComponent[] cleaned = table.ComponentsArray ?? Array.Empty<BlueprintComponent>();
                bool exact = cleaned.Length == expected.Length &&
                    !cleaned.Where((row, index) => !ReferenceEquals(row, expected[index])).Any() &&
                    CapitalVendorBlueprints.ReadCount(foreignRow) == 37;
                checks.Add(Assertion(key + "-stale-rows-removed",
                    "18 stale owned rows removed; " + registered.Length + " native rows and foreign*37 kept in order",
                    "before=" + (expected.Length + retired.Length) + ";after=" + cleaned.Length, exact,
                    "real publishers on the registered table; exact component references"));
                republish();
                checks.Add(Assertion(key + "-cleanup-idempotent", "second publication leaves the cleaned array",
                    "same=" + ReferenceEquals(cleaned, table.ComponentsArray), ReferenceEquals(cleaned, table.ComponentsArray),
                    "no row is re-added or removed again"));
            }
            finally
            {
                table.ComponentsArray = registered;
                UnityEngine.Object.DestroyImmediate(foreign);
            }
            checks.Add(Assertion(key + "-fixture-restored", "original registered array reference",
                "same=" + ReferenceEquals(registered, table.ComponentsArray), ReferenceEquals(registered, table.ComponentsArray),
                "in-memory blueprint only; no save or inventory"));

            Dictionary<BlueprintItem, int> current = SharedVendorTables.GetFixedItems(
                new List<BlueprintUnitLoot> { table });
            var older = new Dictionary<BlueprintItem, int>(current);
            foreach (BlueprintItem item in retired)
            {
                int known;
                older[item] = (older.TryGetValue(item, out known) ? known : 0) + 1;
            }
            Dictionary<BlueprintItem, int> difference = SharedVendorTables.GetLootDifference(older, current);
            checks.Add(Assertion(key + "-native-known-item-difference", "exactly the 18 retired identities at -1",
                "entries=" + difference.Count + ";retired=" + retired.Count(item => difference.ContainsKey(item) && difference[item] == -1),
                difference.Count == retired.Length && retired.All(item => difference.ContainsKey(item) && difference[item] == -1),
                "native GetFixedItems/GetLootDifference on the registered table; native rows unaffected"));

            BlueprintItem probe = retired[0];
            BlueprintItem nativeItem = current.Keys.FirstOrDefault(item => !retired.Contains(item));
            Func<Dictionary<BlueprintItem, int>, int?, int[]> reconcile = (known, probeCount) =>
            {
                var entries = new List<LootEntry>();
                if (nativeItem != null) entries.Add(new LootEntry { Item = nativeItem, Count = 5 });
                if (probeCount.HasValue) entries.Add(new LootEntry { Item = probe, Count = probeCount.Value });
                foreach (KeyValuePair<BlueprintItem, int> pair in SharedVendorTables.GetLootDifference(known, current))
                    SharedVendorTables.UpdateLootItems(entries, pair.Key, pair.Value);
                return new[] {
                    entries.Where(entry => entry.Item == probe).Sum(entry => entry.Count),
                    entries.Count(entry => entry.Item == probe),
                    entries.Where(entry => nativeItem != null && entry.Item == nativeItem).Sum(entry => entry.Count) };
            };
            int nativeExpected = nativeItem == null ? 0 : 5;
            int[] untouched = reconcile(older, 1);
            int[] purchased = reconcile(older, null);
            int[] soldBackOnce = reconcile(older, 1);
            int[] soldBackExtra = reconcile(older, 3);
            int[] missingHistory = reconcile(null, 1);
            int[] repeated = reconcile(current, 1);
            checks.Add(Assertion(key + "-native-reconcile-untouched", "unsold stocked copy 1 -> 0; native 5 kept",
                "probe=" + untouched[0] + ";native=" + untouched[2], untouched[0] == 0 && untouched[2] == nativeExpected,
                "native UpdateLootItems; probe=" + probe.name + ";native=" + (nativeItem == null ? "<none>" : nativeItem.name)));
            checks.Add(Assertion(key + "-native-reconcile-purchased", "no entry added; nothing below zero",
                "probe=" + purchased[0] + ";rows=" + purchased[1], purchased[0] == 0 && purchased[1] == 0 && purchased[2] == nativeExpected,
                "purchased copy has no persisted vendor entry; player inventory is not part of this path"));
            checks.Add(Assertion(key + "-native-reconcile-sold-back", "known limitation: bought-then-sold copy 1 -> 0; extra copies 3 -> 2",
                "once=" + soldBackOnce[0] + ";extra=" + soldBackExtra[0], soldBackOnce[0] == 0 && soldBackExtra[0] == 2,
                "persisted entries keep blueprint/count only; a sold-back copy is indistinguishable from the stocked one"));
            checks.Add(Assertion(key + "-native-reconcile-missing-history", "null KnownItems: no difference; copy kept",
                "probe=" + missingHistory[0], missingHistory[0] == 1, "native GetLootDifference skips null history"));
            checks.Add(Assertion(key + "-native-reconcile-repeat", "KnownItems already current: no further subtraction",
                "probe=" + repeated[0], repeated[0] == 1, "GetTable stores the current fixed rows as KnownItems after reconciling"));
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
