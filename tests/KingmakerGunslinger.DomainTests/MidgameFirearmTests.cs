using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class MidgameFirearmTests
    {
        internal static void DesignsAndPricesFillTheMissingTier()
        {
            MidgameFirearmSpec[] items = MidgameFirearmCatalog.Entries;
            Assertions.Equal(2, items.Length, "Exactly two merchant designs.");
            Assertions.Equal(3, MidgameFirearmCatalog.ActualEnhancement, "Actual enhancement.");
            Assertions.Equal(4, MidgameFirearmCatalog.EquivalentBonus, "Pricing equivalence.");
            Assertions.True(items.Select(x => x.Symbol).Distinct().Count() == 2 &&
                items.Select(x => x.DisplayName).Distinct().Count() == 2, "Unique identities/names.");
            Assertions.True(items[0].Kind == FirearmKind.Musket && items[0].Cost == 33800 &&
                items[0].DisplayName == "Roadwarden" && items[0].Reliable && !items[0].Seeking,
                "Roadwarden is exactly a +3 Reliable musket.");
            Assertions.True(items[1].Kind == FirearmKind.Pistol && items[1].Cost == 33300 &&
                items[1].DisplayName == "Dead Reckoning" && !items[1].Reliable && items[1].Seeking,
                "Dead Reckoning trades Reliable for Seeking.");
            foreach (MidgameFirearmSpec item in items)
            {
                int familyBase = item.Kind == FirearmKind.Musket ? 1500 : 1000;
                Assertions.Equal(familyBase + 300 + 2000 * 4 * 4, item.Cost,
                    "Family base + masterwork + equivalent +4 enchantment price.");
                Assertions.True(item.Cost > 19300 && item.Cost < 51800,
                    "Missing tier lies between existing +2 and +4 named rewards.");
            }
            items[0] = null;
            Assertions.True(MidgameFirearmCatalog.Entries[0] != null,
                "Consumers cannot replace the authored catalog entries.");
        }

        internal static void PropertyTextKeepsTheTradeoffs()
        {
            string reliable = MidgameFirearmCatalog.Roadwarden.Description;
            string seeking = MidgameFirearmCatalog.DeadReckoning.Description;
            Assertions.True(reliable.Contains("misfire value by 1 after other increases") &&
                reliable.Contains("minimum of 0") && reliable.Contains("natural 1 still misses"),
                "Reliable text retains its actual limit.");
            Assertions.True(seeking.Contains("ignores concealment miss chances") &&
                seeking.Contains("does not reveal unseen creatures") &&
                seeking.Contains("could not otherwise target") && seeking.Contains("other defenses"),
                "Seeking text limits both revelation and targeting.");
            Assertions.False(seeking.Contains("Reliable"), "No free Reliable on the pistol.");
            foreach (MidgameFirearmSpec item in MidgameFirearmCatalog.Entries)
                Assertions.True(!string.IsNullOrWhiteSpace(item.Flavor) &&
                    !item.Description.Contains("never misses") &&
                    !item.Description.Contains("never misfires"), "Truthful equipment text.");
        }

        internal static void StableIdentitiesAreNewAndExact()
        {
            DirectoryInfo root = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (root != null && !File.Exists(Path.Combine(root.FullName, "Info.json"))) root = root.Parent;
            Assertions.True(root != null, "Repository fixture root.");
            JToken[] entries = JObject.Parse(File.ReadAllText(Path.Combine(root.FullName,
                "blueprints", "blueprints.json")))["entries"].ToArray();
            string[] ids = { "66d2f8c4d6aa43e0be72ac18ed9fcd81", "b8db89aba5364c27b1626896664a1913" };
            for (int i = 0; i < ids.Length; i++)
            {
                JToken entry = entries.Single(x => (string)x["symbol"] == MidgameFirearmCatalog.Entries[i].Symbol);
                Assertions.True((string)entry["guid"] == ids[i] &&
                    (string)entry["plannedType"] == "BlueprintItemWeapon" &&
                    (string)entry["status"] == "active", "Stable active weapon identity.");
                Assertions.Equal(1, entries.Count(x => (string)x["guid"] == ids[i]), "GUID not reused.");
            }
        }

        internal static void MerchantScopeIncludesBothNativeVariants()
        {
            Assertions.Equal("b8b362de19b0a8340ad050586f1162d1", SkeletalSalesmanStockCatalog.UnitGuid,
                "Exact native Skeletal Salesman unit.");
            Assertions.Equal("RE_Trader", SkeletalSalesmanStockCatalog.UnitName, "Native unit name.");
            Assertions.Equal(491, SkeletalSalesmanStockCatalog.FirstKingdomDay, "C3 native first day.");
            Assertions.Equal(1, SkeletalSalesmanStockCatalog.CopiesPerStock, "One copy per generated stock.");
            string[] names = { "C3_VendorTableLarge", "C3_VendorTableSmall", "C4_VendorTableLarge", "C4_VendorTableSmall" };
            string[] ids = { "b3bc1bb9f4a59f3438edc505e0f3b407", "9126c670f0743b647b4e9ba850214d8d",
                "fc01b45fee3606749a21d9612c5629a6", "4b1bb03a5d19a534bad2aa5cd766af92" };
            Assertions.True(SkeletalSalesmanStockCatalog.Targets.Select(x => x.Name).SequenceEqual(names) &&
                SkeletalSalesmanStockCatalog.Targets.Select(x => x.Guid).SequenceEqual(ids),
                "Only both middle/later variants, no C2 or other merchants.");
        }

        internal static void InsertionPreservesOtherStockAndRepeats()
        {
            var axe = new StockRow("Axe", 0); var sword = new StockRow("Longsword", 0);
            var spear = new StockRow("Spear", 0); var armor = new StockRow("Armor", 1);
            var road = new StockRow("Roadwarden", 0); var dead = new StockRow("Dead Reckoning", 0);
            StockRow[] before = { axe, sword, spear, armor };
            StockRow[] additions = { road, dead };
            var publication = VendorCatalogPublication<StockRow>.CreateIntegrated(before, additions, x => x.Key);
            Assertions.True(publication.Published.SequenceEqual(new[] { axe, dead, sword, road, spear, armor }),
                "Additions use visible names among weapons.");
            Assertions.True(publication.Published.Except(additions).SequenceEqual(before),
                "Unrelated stock order/identity retained.");
            var repeated = VendorCatalogPublication<StockRow>.CreateIntegrated(publication.Published, additions, x => x.Key);
            Assertions.True(!repeated.Changed && repeated.Published.SequenceEqual(publication.Published),
                "Repeat publication does not duplicate or move stock.");
            Assertions.True(publication.Rollback().SequenceEqual(before), "Exact rollback.");
        }

        private sealed class StockRow
        {
            internal StockRow(string name, int type) { Key = type.ToString("D4") + ":" + name; }
            internal string Key { get; private set; }
        }
    }
}
