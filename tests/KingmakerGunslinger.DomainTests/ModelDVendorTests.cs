using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.FeatureModules;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ModelDVendorTests
    {
        private sealed class Item { internal readonly string Name; internal Item(string name) { Name = name; } }
        private sealed class Row
        {
            internal readonly Item Item; internal readonly int Count;
            internal Row(Item item, int count) { Item = item; Count = count; }
        }
        private static VendorCatalogPublication<Row> Normalize(Row[] rows, Item[] owned,
            Item[] wanted, int[] counts, Func<Item, int, Row> factory = null)
        {
            return VendorCatalogPublication<Row>.NormalizeOwned(rows, owned, wanted, counts,
                row => row.Item, row => row.Count, factory ?? ((item, count) => new Row(item, count)));
        }
        internal static void PreviousStockShape()
        {
            var old = new Item("mundane"); var wanted = new Item("plus1");
            var native = new Row(new Item("native"), 8);
            var rows = new[] { native, new Row(old, 1), new Row(wanted, 17) };
            var tx = Normalize(rows, new[] { old, wanted }, new[] { wanted }, new[] { 1 });
            Assertions.Equal(2, tx.Published.Length, "Retired stock removed.");
            Assertions.True(ReferenceEquals(native, tx.Published[0]), "Native identity/order.");
            Assertions.Equal(1, tx.Published[1].Count, "Requested stock count.");
            Assertions.Equal(3, rows.Length, "Input snapshot unchanged.");
        }
        internal static void ExactIdentityPreservesForeignNames()
        {
            var owned = new Item("Pistol"); var foreign = new Row(new Item("Pistol"), 41);
            var tx = Normalize(new[] { foreign }, new[] { owned }, new[] { owned }, new[] { 1 });
            Assertions.True(ReferenceEquals(foreign, tx.Published[0]), "Display-name collision is foreign.");
            Assertions.Equal(41, foreign.Count, "Foreign quantity.");
        }
        internal static void NativeNonItemComponentsSurvive()
        {
            var owned = new Item("ammo"); var native = new Row(null, 0);
            var tx = Normalize(new[] { native }, new[] { owned }, new[] { owned }, new[] { 50 });
            Assertions.True(ReferenceEquals(native, tx.Published[0]), "Native metadata row survives.");
        }
        internal static void DuplicateOwnedRowsCollapse()
        {
            var item = new Item("ammo");
            var tx = Normalize(new[] { new Row(item, 50), new Row(item, 50) },
                new[] { item }, new[] { item }, new[] { 50 });
            Assertions.Equal(1, tx.Published.Length, "Duplicate paths normalize to one row.");
        }
        internal static void IncorrectQuantityIsRepaired()
        {
            var item = new Item("ammo");
            var tx = Normalize(new[] { new Row(item, 1) }, new[] { item }, new[] { item }, new[] { 200 });
            Assertions.Equal(200, tx.Published.Single().Count, "Capital quantity repaired.");
        }
        internal static void ModuleDisabledRemovesOnlyOwned()
        {
            var item = new Item("ammo"); var foreign = new Row(new Item("foreign"), 23);
            var tx = Normalize(new[] { new Row(item, 50), foreign }, new[] { item }, new Item[0], new int[0]);
            Assertions.True(tx.Published.Length == 1 && ReferenceEquals(foreign, tx.Published[0]), "Disabled cleanup.");
        }
        internal static void EnableDisableEnable()
        {
            var item = new Item("ammo"); var native = new Row(new Item("native"), 3);
            var first = Normalize(new[] { native }, new[] { item }, new[] { item }, new[] { 50 });
            var off = Normalize(first.Published, new[] { item }, new Item[0], new int[0]);
            var next = Normalize(off.Published, new[] { item }, new[] { item }, new[] { 50 });
            Assertions.Equal(2, next.Published.Length, "No duplicate after reenable.");
            Assertions.True(ReferenceEquals(native, next.Published[0]), "Native survives every phase.");
        }
        internal static void RepeatedInitializationPreservesReferences()
        {
            var item = new Item("ammo"); var row = new Row(item, 50);
            var tx = Normalize(new[] { row }, new[] { item }, new[] { item }, new[] { 50 });
            Assertions.False(tx.Changed, "Exact table is not rewritten.");
            Assertions.True(ReferenceEquals(row, tx.Published[0]), "Exact row reference retained.");
        }
        internal static void RollbackRestoresPreviousShape()
        {
            var item = new Item("ammo"); var row = new Row(item, 9);
            var tx = Normalize(new[] { row }, new[] { item }, new[] { item }, new[] { 50 });
            Assertions.True(ReferenceEquals(row, tx.Rollback().Single()), "Original wrong-count row restored exactly.");
            Assertions.Throws<InvalidOperationException>(() => tx.Rollback(), "Rollback cannot be replayed.");
        }
        internal static void PublicationArraysAreDefensive()
        {
            var item = new Item("ammo"); var tx = Normalize(new Row[0], new[] { item }, new[] { item }, new[] { 50 });
            tx.Published[0] = null;
            Assertions.True(tx.Published[0] != null, "Caller cannot corrupt stored transaction.");
        }
        internal static void NullCatalogFailsClosed()
        {
            var item = new Item("ammo");
            Assertions.Throws<InvalidOperationException>(() => Normalize(new Row[] { null }, new[] { item }, new[] { item }, new[] { 50 }), "Null row conflict.");
        }
        internal static void DuplicateReferenceFailsClosed()
        {
            var item = new Item("ammo"); var row = new Row(item, 50);
            Assertions.Throws<InvalidOperationException>(() => Normalize(new[] { row, row }, new[] { item }, new[] { item }, new[] { 50 }), "Aliased component conflict.");
        }
        internal static void DesiredIdentityMustBeUniqueAndOwned()
        {
            var item = new Item("ammo"); var other = new Item("foreign");
            Assertions.Throws<InvalidOperationException>(() => Normalize(new Row[0], new[] { item }, new[] { item, item }, new[] { 1, 1 }), "Duplicate desired item.");
            Assertions.Throws<InvalidOperationException>(() => Normalize(new Row[0], new[] { item }, new[] { other }, new[] { 1 }), "Foreign desired item.");
        }
        internal static void InvalidCountsFailClosed()
        {
            var item = new Item("ammo");
            foreach (int count in new[] { 0, -1 })
                Assertions.Throws<InvalidOperationException>(() => Normalize(new Row[0], new[] { item }, new[] { item }, new[] { count }), "Nonpositive count.");
            Assertions.Throws<InvalidOperationException>(() => Normalize(new Row[0], new[] { item }, new[] { item }, new int[0]), "Count length mismatch.");
        }
        internal static void FactoryRoundtripFailsBeforePublication()
        {
            var item = new Item("ammo"); var row = new Row(new Item("native"), 5); var rows = new[] { row };
            Assertions.Throws<InvalidOperationException>(() => Normalize(rows, new[] { item }, new[] { item }, new[] { 50 }, (i, c) => new Row(i, 1)), "Bad native factory.");
            Assertions.True(rows.Length == 1 && ReferenceEquals(row, rows[0]), "No partial mutation.");
        }
        internal static void FactoryExceptionLeavesInputUntouched()
        {
            var item = new Item("ammo"); var row = new Row(new Item("native"), 5); var rows = new[] { row };
            Assertions.Throws<InvalidOperationException>(() => Normalize(rows, new[] { item }, new[] { item }, new[] { 50 }, (i, c) => { throw new InvalidOperationException("injected"); }), "Factory fault.");
            Assertions.True(ReferenceEquals(row, rows.Single()), "No mutation on factory fault.");
        }
        internal static void OlegExactFiveContract()
        {
            string source = Source("Blueprints/OlegFirearmVendorBlueprints.cs");
            string stock = Slice(source, "BlueprintItem[] stocked =", "BlueprintItem[] items =");
            foreach (string item in new[] { "firearms.Pistol.Item", "firearms.Musket.Item", "firearms.Blunderbuss.Item", "ammunition.BlackPowder", "ammunition.LeadBall" })
                Assertions.Equal(1, stock.Split(new[] { item }, StringSplitOptions.None).Length - 1, "Singular Oleg row: " + item);
            foreach (string excluded in new[] { "PaperCartridge", "GunsmithKit", "magicFirearms", "Plus1", "Progression", "Named" })
                Assertions.False(stock.Contains(excluded), "Oleg excluded stock: " + excluded);
            Assertions.True(source.Contains("1, 1, 1, AmmunitionCount, AmmunitionCount") && source.Contains("AmmunitionCount = 50"), "Oleg quantities.");
        }
        internal static void CapitalExactSevenAndRegionalRetirement()
        {
            string source = Source("Blueprints/CapitalVendorBlueprints.cs");
            string stock = Slice(source, "BlueprintItem[] gunslingerItems =", "int[] gunslingerCounts =");
            Assertions.Equal(3, stock.Split(new[] { "Plus1Symbol" }, StringSplitOptions.None).Length - 1, "Three +1 firearms.");
            foreach (string excluded in new[] { "firearms.Pistol", "firearms.Musket", "firearms.Blunderbuss", "easternWeapons", "elvenBranchedSpears" })
                Assertions.False(stock.Contains(excluded), "Capital excluded stock: " + excluded);
            foreach (string owned in new[] { ".Concat(easternWeapons.Entries", ".Concat(elvenBranchedSpears.Entries" })
                Assertions.True(source.Contains(owned), "Retired generic identities cleaned even with module off.");
            Assertions.True(source.Contains("AmmunitionCount = 200"), "Capital ammo count.");
        }
        internal static void BokkenExactThreeNoKit()
        {
            string source = Source("Blueprints/BokkenFirearmSupplyVendorBlueprints.cs");
            string stock = Slice(source, "BlueprintItem[] stocked =", "BlueprintItem[] items =");
            Assertions.Equal(3, stock.Split(new[] { "ammunition." }, StringSplitOptions.None).Length - 1, "Three ammunition rows.");
            Assertions.False(stock.Contains("Kit") || stock.Contains("firearms"), "No kit/firearms.");
            Assertions.True(source.Contains("AmmunitionCount = 100") && Slice(source, "BlueprintItem[] owned =", "BlueprintItem[] stocked =").Contains("supplies.GunsmithKit"), "Retired kit remains owned.");
        }
        internal static void ModuleCombinationsAndIndependentOleg()
        {
            for (int flags = 0; flags < 8; flags++)
            {
                bool guns = (flags & 1) != 0, east = (flags & 2) != 0, spear = (flags & 4) != 0;
                var config = new FeatureModuleConfiguration(guns, false, false, false, spear, east, false, false, false, false, false, false, false);
                var plan = new FeatureModulePublicationPlan(config);
                Assertions.Equal(guns, plan.CapitalGunslingerStock, "Independent firearm gate.");
                Assertions.Equal(east, plan.EasternWeaponCommerce, "Eastern gate.");
                Assertions.Equal(spear, plan.ElvenBranchedSpearCommerce, "Spear gate.");
            }
            string boot = Source("Bootstrap/BlueprintBootstrap.cs");
            string oleg = Slice(boot, "OlegFirearmVendorBlueprints.Publish(", "bokkenSupplyPublication =");
            Assertions.True(oleg.Contains("publicationPlan.CapitalGunslingerStock"), "Oleg uses independent Gunslinger gate.");
            Assertions.False(oleg.Contains("EasternWeaponCommerce") || oleg.Contains("ElvenBranchedSpearCommerce"), "No foreign module dependency.");
        }
        // Byte-normalized baseline contracts supplement the native table runtime
        // observations; they are not presented as gameplay or tooltip evidence.
        internal static void RegionalEasternAndNamedLootUnchanged()
        {
            AssertHash(Source("Blueprints/EasternWeaponCampaignBlueprints.cs").Replace("CapitalVendorBlueprints.ExpectedTableName, new EasternWeaponGenericKind[0],", "CapitalVendorBlueprints.ExpectedTableName, AllGenericKinds(),").Replace("Model D capital retired generic stock", "capital recurring generic stock"), "cc21b32bfce787f22d58311aad04a78df6e91b1d2cc82ee8e131c393ade96509", "Blueprints/EasternWeaponCampaignBlueprints.cs");
        }
        internal static void RegionalSpearsAndNamedLootUnchanged()
        {
            AssertHash(Source("Blueprints/ElvenBranchedSpearCampaignBlueprints.cs").Replace("CapitalVendorBlueprints.ExpectedTableName,\n                new ElvenBranchedSpearItemKind[0], null)", "CapitalVendorBlueprints.ExpectedTableName,\n                AllFoundationKinds(), null)"), "b8588b4d52b1674bfe4fe69d7f124fa8ddf69fede1ebfca2ac39e5f63555528d", "Blueprints/ElvenBranchedSpearCampaignBlueprints.cs");
        }
        internal static void BtslUnchanged()
        {
            AssertHash(Source("Blueprints/BeneathStolenLandsVendorBlueprints.cs"), "9cc28999f8b2426930c86897db554c1cae4e93d830be13e43bf1416f26c4d063", "Blueprints/BeneathStolenLandsVendorBlueprints.cs");
        }
        internal static void BetterVendorsCatalogScheduleAndLedgerUnchanged()
        {
            AssertHash(Source("Acquisition/BetterVendors/BetterVendorsCompatibilityCoordinator.cs"), "8c8c04a6af738f82ecc959be9aecc971c1133236c73fd1927684b5e5aee08ab3", "Acquisition/BetterVendors/BetterVendorsCompatibilityCoordinator.cs");
            AssertHash(Source("Acquisition/BetterVendors/BetterVendorsContract.cs"), "073400a9fe1a7e66d9abac00fface3c1dddb603cb7c33257aa255c32d8510277", "Acquisition/BetterVendors/BetterVendorsContract.cs");
            AssertHash(Source("Acquisition/BetterVendors/BetterVendorsGrantPlanner.cs"), "31b065130f25db61efd1a4fbd8980628ba142ebcfab8646ae0052024a743d3e4", "Acquisition/BetterVendors/BetterVendorsGrantPlanner.cs");
            AssertHash(Source("Acquisition/BetterVendors/BetterVendorsIntegrationStatus.cs"), "01f2b569d2cf762a4aa6fa680f0ad23a830e21e92dd21be2e67709f8aa1355e6", "Acquisition/BetterVendors/BetterVendorsIntegrationStatus.cs");
            AssertHash(Source("Acquisition/BetterVendors/BetterVendorsProgressionSchedule.cs"), "448c2276f793fc2a354c56247ee5cf1b7019875ba62107ebcfe30ba6e73375cb", "Acquisition/BetterVendors/BetterVendorsProgressionSchedule.cs");
            AssertHash(Source("Acquisition/BetterVendors/BetterVendorsStockPass.cs"), "3b55cf34db699fb0d20596420a62d4cc0d53d2df701e526bae25cf8a23daef2b", "Acquisition/BetterVendors/BetterVendorsStockPass.cs");
            AssertHash(Source("Acquisition/BetterVendors/BetterVendorsStockRuntime.cs"), "7b6459fc023d53f7c794f8aecd99962fde9eb940a60008b546240c1325046bbb", "Acquisition/BetterVendors/BetterVendorsStockRuntime.cs");
            AssertHash(Source("Acquisition/BetterVendors/UnitPartBetterVendorsProgressionGrants.cs"), "6c2936212b2d760adc1a79be23d32b1479d808b667436fb4e8803cd5f81dff17", "Acquisition/BetterVendors/UnitPartBetterVendorsProgressionGrants.cs");
        }
        internal static void SalesmanNamedLootAndStarterUnchanged()
        {
            AssertHash(Source("Blueprints/SkeletalSalesmanBlueprints.cs"), "6af87ceb9f6b3b7300311a66d177b1aec69157ac291910d2545a2d2102169cdd", "Blueprints/SkeletalSalesmanBlueprints.cs");
            AssertHash(Source("Blueprints/RareFirearmCampaignLootBlueprints.cs"), "b73eb014668d1b0b5282f0d2d884c86ccd8abfd24fafd8294c1074eade27eea7", "Blueprints/RareFirearmCampaignLootBlueprints.cs");
            AssertHash(Source("Gunsmithing/GunslingerStartingFirearmGrantTransaction.cs"), "791a0b2d0862a05a8e26e753992aa18de07fc2ab8f2a2010e722cb38d04b7f90", "Gunsmithing/GunslingerStartingFirearmGrantTransaction.cs");
        }
        internal static void ItemIdentityEconomicsEnchantmentsAndCraftingUnchanged()
        {
            AssertHash(Source("Blueprints/ProductionFirearmBlueprints.cs"), "9e530097a64a0f9f7eb430336465f91c6e660bcf52b4d92b150ce2b049dc3ffd", "Blueprints/ProductionFirearmBlueprints.cs");
            AssertHash(Source("Blueprints/MagicFirearmBlueprints.cs"), "ab7ddafea87e3959999221de716fcca61b5cd2ddc8e93c6699efcc3bc78a5478", "Blueprints/MagicFirearmBlueprints.cs");
            AssertHash(Source("Blueprints/BasicAmmunitionBlueprints.cs"), "e636c43de97830372af4f99defbd30fd5770f1effeadaf64f2bd83c1c59fc49e", "Blueprints/BasicAmmunitionBlueprints.cs");
            AssertHash(Source("Blueprints/GunsmithingSupplyBlueprints.cs"), "9cb1f57472ae9929034f7d6c65b06c1a77b78ea6c26fc1794c9edb9087f9104d", "Blueprints/GunsmithingSupplyBlueprints.cs");
            AssertHash(Source("Blueprints/GunsmithingCraftingBlueprints.cs"), "8bf13115aca5c9fb064dc79be6861b57348e42b43b2ef87508f9f5ea5278c650", "Blueprints/GunsmithingCraftingBlueprints.cs");
            AssertHash(Source("Blueprints/EasternWeaponBlueprints.cs"), "44365a93be563a4541f18abda4e7889ea4a68e305cee332a58aab75ca24ea50e", "Blueprints/EasternWeaponBlueprints.cs");
            AssertHash(Source("Blueprints/ElvenBranchedSpearBlueprints.cs"), "bc2199650c429230a14d270ada62aa2e10f26a9c934dc42cd763af99497a19d9", "Blueprints/ElvenBranchedSpearBlueprints.cs");
        }
        private static void AssertHash(string source, string expected, string path)
        {
            using (var sha = SHA256.Create())
                Assertions.Equal(expected, BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(source.Replace("\r\n", "\n")))).Replace("-", "").ToLowerInvariant(), "Protected baseline contract: " + path);
        }
        private static string Source(string path)
        { return File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "src", "KingmakerGunslinger", path)).Replace("\r\n", "\n"); }
        private static string Slice(string source, string start, string end)
        { int a = source.IndexOf(start, StringComparison.Ordinal); int b = source.IndexOf(end, a, StringComparison.Ordinal); return source.Substring(a, b - a); }
    }
}
