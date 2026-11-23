using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Acquisition;

namespace KingmakerGunslinger.DomainTests
{
    // LIM-01 (Sprint 6 D1). The model reproduces the installed 2.1.7b
    // SharedVendorTables.GetTable first-access path (decompiled, 398 IL bytes):
    // a table with no persisted data is generated from the current fixed rows;
    // otherwise GetLootDifference(KnownItems, fixed) (empty when KnownItems is
    // null) drives UpdateLootItems on the blueprint/count Entries, and KnownItems
    // is then replaced by the fixed rows. The production prefix runs
    // RetiredVendorHistoryPolicy on the exact retired pairs first.
    internal static class RetiredVendorHistoryTests
    {
        private const string ColdIron = "spear-cold-iron";
        private const string Mundane = "spear-mundane";
        private const string Native = "native-longsword";

        private sealed class Table
        {
            internal List<KeyValuePair<string, int>> Entries = new List<KeyValuePair<string, int>>();
            internal Dictionary<string, int> Known;
            internal int Count(string item) { return Entries.Where(e => e.Key == item).Sum(e => e.Value); }
        }

        // The current publication: the retired cold iron row is gone.
        private static readonly Dictionary<string, int> Published = new Dictionary<string, int> {
            { Mundane, 1 }, { Native, 2 } };

        private static Table Legacy(Dictionary<string, int> known, params KeyValuePair<string, int>[] entries)
        { return new Table { Known = known, Entries = entries.ToList() }; }
        private static KeyValuePair<string, int> E(string item, int count) { return new KeyValuePair<string, int>(item, count); }
        private static Dictionary<string, int> OldKnown() { return new Dictionary<string, int> { { ColdIron, 1 }, { Mundane, 1 }, { Native, 2 } }; }

        // Native first access (one reconciliation), optionally preceded by the LIM-01 policy for the exact retired items.
        private static Table Access(Table table, string[] retired)
        {
            if (table == null)
                return new Table { Known = new Dictionary<string, int>(Published), Entries = Published.Select(r => E(r.Key, r.Value)).ToList() };
            if (retired != null)
            {
                var plan = RetiredVendorHistoryPolicy.Plan(table.Known, Published, retired);
                RetiredVendorHistoryPolicy.Apply(table.Known, plan);
            }
            if (table.Known != null)
                foreach (string item in table.Known.Keys.Union(Published.Keys).ToList())
                {
                    int oldCount = table.Known.ContainsKey(item) ? table.Known[item] : 0;
                    int newCount = Published.ContainsKey(item) ? Published[item] : 0;
                    if (oldCount != newCount) NativeUpdate(table.Entries, item, newCount - oldCount);
                }
            table.Known = new Dictionary<string, int>(Published);
            return table;
        }

        private static void NativeUpdate(List<KeyValuePair<string, int>> entries, string item, int difference)
        {
            for (int index = 0; index < entries.Count && difference != 0; index++)
            {
                if (entries[index].Key != item) continue;
                int count = entries[index].Value;
                if (count + difference >= 0) { entries[index] = E(item, count + difference); difference = 0; }
                else { difference += count; entries[index] = E(item, 0); }
            }
            if (difference > 0) entries.Add(E(item, difference));
        }

        private static Table SaveLoad(Table table)
        {
            // PreSave rewrites Entries from the live collection (positive counts only); KnownItems persists as is.
            return new Table { Known = table.Known == null ? null : new Dictionary<string, int>(table.Known),
                Entries = table.Entries.Where(e => e.Value > 0).ToList() };
        }

        private static readonly string[] Retired = { ColdIron };

        private static void Expect(bool condition, string message) { if (!condition) throw new Exception(message); }

        // Case 1: a fresh table is generated from the current rows; the retired row never appears.
        internal static void FreshTablePublishesNoRetiredStock()
        {
            Table fresh = Access(null, Retired);
            Expect(fresh.Count(ColdIron) == 0, "fresh table generated retired stock");
            Expect(fresh.Count(Mundane) == 1 && fresh.Count(Native) == 2, "fresh table lost active stock");
        }

        // Cases 2 and 4: an untouched original and a bought-then-sold-back copy have the same representation;
        // the native baseline subtracts it, the protected path preserves it.
        internal static void AmbiguousSingleCopyIsPreserved()
        {
            Table baseline = Access(Legacy(OldKnown(), E(ColdIron, 1), E(Mundane, 1), E(Native, 2)), null);
            Expect(baseline.Count(ColdIron) == 0, "the model must reproduce the native baseline decrement");
            Table untouched = Access(Legacy(OldKnown(), E(ColdIron, 1), E(Mundane, 1), E(Native, 2)), Retired);
            Table soldBack = Access(Legacy(OldKnown(), E(ColdIron, 1), E(Mundane, 1), E(Native, 2)), Retired);
            Expect(untouched.Count(ColdIron) == 1 && soldBack.Count(ColdIron) == 1, "an ambiguous copy was subtracted");
            Expect(untouched.Count(Mundane) == 1 && untouched.Count(Native) == 2, "active or unrelated stock changed");
            Expect(!untouched.Known.ContainsKey(ColdIron), "history kept the retired row");
        }

        // Case 3: a purchased original with no matching stock stays empty and is never replenished.
        internal static void PurchasedOriginalIsNotReplenished()
        {
            Table bought = Access(Legacy(OldKnown(), E(Mundane, 1), E(Native, 2)), Retired);
            Expect(bought.Count(ColdIron) == 0, "a purchased retired row was replenished");
            bought = Access(SaveLoad(bought), Retired);
            Expect(bought.Count(ColdIron) == 0, "a purchased retired row came back after reload");
        }

        // Case 5: multiple matching sales and mixed counts are preserved without repeated decrement.
        internal static void MultipleMatchingCopiesArePreservedAcrossRepeats()
        {
            Table mixed = Access(Legacy(OldKnown(), E(ColdIron, 3), E(Mundane, 1), E(Native, 2)), Retired);
            Expect(mixed.Count(ColdIron) == 3, "mixed retired copies were subtracted");
            for (int repeat = 0; repeat < 3; repeat++) mixed = Access(SaveLoad(mixed), Retired);
            Expect(mixed.Count(ColdIron) == 3, "a repeat reconciliation or reload changed the retired count");
        }

        // Case 6: absent history is preserved; the engine skips the diff and no provenance is invented.
        internal static void MissingHistoryIsPreserved()
        {
            Table table = Access(Legacy(null, E(ColdIron, 2), E(Mundane, 1), E(Native, 2)), Retired);
            Expect(table.Count(ColdIron) == 2, "missing history lost a copy");
            Expect(table.Known != null && !table.Known.ContainsKey(ColdIron), "history was not replaced by the current rows");
            Expect(RetiredVendorHistoryPolicy.Plan<string>(null, Published, Retired).Decision == RetiredVendorHistoryDecision.NoHistory,
                "absent history must not be planned");
        }

        // Controls: an unrelated item at the affected table and the same item at an unrelated table keep native reconciliation.
        internal static void NativeReconciliationOutsideRetiredPairsIsIntact()
        {
            var known = OldKnown(); known[Native] = 3; // the native row was lowered from 3 to 2
            Table affected = Access(Legacy(known, E(ColdIron, 1), E(Mundane, 1), E(Native, 3)), Retired);
            Expect(affected.Count(Native) == 2, "the unrelated native decrement at the affected table was suppressed");
            Expect(affected.Count(ColdIron) == 1, "the retired copy at the affected table was subtracted");
            Table unrelated = Access(Legacy(OldKnown(), E(ColdIron, 1), E(Mundane, 1), E(Native, 2)), new string[0]);
            Expect(unrelated.Count(ColdIron) == 0, "the same item at an unrelated table must keep the native decrement");
        }

        // A retired item still partly published keeps exactly the published share; only the excess history is lowered.
        internal static void PartialPublicationLowersOnlyTheExcess()
        {
            var plan = RetiredVendorHistoryPolicy.Plan(new Dictionary<string, int> { { Mundane, 3 } }, Published, new[] { Mundane });
            Expect(plan.Decision == RetiredVendorHistoryDecision.Neutralized && plan.Neutralize.Single().Value == 1, "excess history was not lowered to the published count");
            var none = RetiredVendorHistoryPolicy.Plan(new Dictionary<string, int> { { Mundane, 1 } }, Published, new[] { Mundane });
            Expect(none.Decision == RetiredVendorHistoryDecision.NoRetiredRows, "an unchanged row was planned");
        }
    }
}
