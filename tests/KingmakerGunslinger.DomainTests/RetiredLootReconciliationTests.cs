using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Acquisition;

namespace KingmakerGunslinger.DomainTests
{
    // Thornstep's former Riverine Rise container across an upgrade. The model
    // reproduces the installed 2.1.7b LootPersistentData.UpdateLoot /
    // UpdateLootItem / ItemsCollection.Remove(blueprint, count) behavior
    // (decompiled 0x0600A6C8): a withdrawn row is subtracted by blueprint and
    // count, removing from the end of the collection. The production prefix
    // runs RetiredLootReconciliationPolicy first, then that native diff.
    internal static class RetiredLootReconciliationTests
    {
        private const string Thornstep = "thornstep";
        private sealed class Item
        {
            internal string Blueprint; internal int Count; internal TimeSpan Time;
            internal Item(string blueprint, int count, TimeSpan time)
            { Blueprint = blueprint; Count = count; Time = time; }
        }
        private sealed class Container
        {
            internal readonly List<Item> Loot = new List<Item>();
            internal Dictionary<string, int> Known;
            internal RetiredLootDecision? Decision;
        }

        // Native rows of the authored source after retirement (no Thornstep).
        private static readonly Dictionary<string, int> Published = new Dictionary<string, int> {
            { "lesser-restoration", 2 }, { "remove-blindness", 3 }, { "gold", 43 } };

        private static Container Generated()
        {
            // First area load under the six-placement publication: every row,
            // including Thornstep, is created in one pass with Time zero.
            var result = new Container { Known = new Dictionary<string, int>(Published) };
            result.Known[Thornstep] = 1;
            foreach (var row in result.Known) result.Loot.Add(new Item(row.Key, row.Value, TimeSpan.Zero));
            return result;
        }

        private static void NativeUpdateLoot(Container container)
        {
            if (container.Known == null) return;
            foreach (var row in Published)
                NativeUpdateItem(container, row.Key, container.Known.ContainsKey(row.Key) ? container.Known[row.Key] : 0, row.Value);
            foreach (string key in container.Known.Keys.Where(key => !Published.ContainsKey(key)).ToList())
                NativeUpdateItem(container, key, container.Known[key], 0);
        }

        private static void NativeUpdateItem(Container container, string key, int oldCount, int newCount)
        {
            if (oldCount == newCount) return;
            if (oldCount < newCount) container.Loot.Add(new Item(key, newCount - oldCount, TimeSpan.Zero));
            else
            {
                int count = oldCount - newCount;
                for (int index = container.Loot.Count - 1; index >= 0 && count > 0; index--)
                {
                    Item item = container.Loot[index];
                    if (item.Blueprint != key) continue;
                    int take = Math.Min(count, item.Count); count -= take; item.Count -= take;
                    if (item.Count == 0) container.Loot.RemoveAt(index);
                }
            }
            container.Known[key] = newCount;
        }

        // One area load after the upgrade: the production prefix, then native.
        private static void Load(Container container, bool protectedByPrefix = true)
        {
            if (protectedByPrefix)
            {
                int known = 0;
                if (container.Known != null) container.Known.TryGetValue(Thornstep, out known);
                var plan = RetiredLootReconciliationPolicy.Plan(container.Known != null, known,
                    container.Loot.Where(item => item.Blueprint == Thornstep).ToArray(),
                    item => RetiredLootReconciliationPolicy.NeverHeldByParty(item.Time));
                foreach (Item item in plan.Remove) container.Loot.Remove(item);
                if (plan.RetireHistory) container.Known[Thornstep] = 0;
                container.Decision = plan.Decision;
            }
            NativeUpdateLoot(container);
        }

        private static int Count(Container container, string key)
        { return container.Loot.Where(item => item.Blueprint == key).Sum(item => item.Count); }

        private static void AssertNativeRowsIntact(Container container, string scenario)
        {
            foreach (var row in Published)
                Assertions.Equal(row.Value, Count(container, row.Key),
                    scenario + ": unrelated native row " + row.Key + " changed.");
        }

        internal static void UntouchedGeneratedRewardIsRemovedOnce()
        {
            Container container = Generated();
            Load(container);
            Assertions.Equal(RetiredLootDecision.RemovedUntouched, container.Decision.Value, "Untouched reward decision.");
            Assertions.Equal(0, Count(container, Thornstep), "The untouched generated Thornstep was not removed.");
            Assertions.Equal(0, container.Known[Thornstep], "The retired history row was not retired.");
            AssertNativeRowsIntact(container, "untouched");
            Load(container);
            Assertions.Equal(RetiredLootDecision.NotInHistory, container.Decision.Value, "A repeat load acted again.");
            Assertions.Equal(0, Count(container, Thornstep), "A repeat load re-added Thornstep.");
            AssertNativeRowsIntact(container, "untouched repeat");
        }

        internal static void ClaimedAndReturnedCopyIsRetained()
        {
            Container control = Generated();
            control.Loot.Single(item => item.Blueprint == Thornstep).Time = TimeSpan.FromHours(31.5);
            Load(control, protectedByPrefix: false);
            Assertions.Equal(0, Count(control, Thornstep),
                "Control: the unprotected native decrement should delete the returned copy (the reviewed defect).");

            Container container = Generated();
            // Taken into the party inventory (Time stamped), later put back.
            container.Loot.Single(item => item.Blueprint == Thornstep).Time = TimeSpan.FromHours(31.5);
            Load(container);
            Assertions.Equal(RetiredLootDecision.RetainedAmbiguous, container.Decision.Value, "Returned copy decision.");
            Assertions.Equal(1, Count(container, Thornstep), "A previously claimed Thornstep was deleted.");
            Assertions.Equal(0, container.Known[Thornstep], "The history row must be retired so native never decrements.");
            AssertNativeRowsIntact(container, "returned");
            Load(container);
            Assertions.Equal(1, Count(container, Thornstep), "A repeat load deleted the retained copy.");
        }

        internal static void MissingKnownHistoryRetainsEveryCopy()
        {
            foreach (TimeSpan time in new[] { TimeSpan.Zero, TimeSpan.FromHours(4) })
            {
                Container container = Generated();
                container.Known = null;
                container.Loot.Single(item => item.Blueprint == Thornstep).Time = time;
                Load(container);
                Assertions.Equal(RetiredLootDecision.NoHistoryRetained, container.Decision.Value, "Missing history decision.");
                Assertions.Equal(1, Count(container, Thornstep), "A Thornstep without known history was removed.");
                Assertions.True(container.Known == null, "Missing history was fabricated.");
                AssertNativeRowsIntact(container, "missing history");
            }
        }

        internal static void UnrelatedItemsInTheContainerArePreserved()
        {
            Container container = Generated();
            // A partially looted native row and a deposited unrelated item.
            container.Loot.Single(item => item.Blueprint == "remove-blindness").Count = 1;
            container.Known["remove-blindness"] = 3;
            container.Loot.Add(new Item("players-dagger", 1, TimeSpan.FromHours(12)));
            Load(container);
            Assertions.Equal(0, Count(container, Thornstep), "The untouched reward survived.");
            Assertions.Equal(1, Count(container, "remove-blindness"), "A partially looted native row was refilled or reduced.");
            Assertions.Equal(2, Count(container, "lesser-restoration"), "An untouched native row changed.");
            Assertions.Equal(43, Count(container, "gold"), "Native gold changed.");
            Assertions.Equal(1, Count(container, "players-dagger"), "A deposited unrelated item changed.");
        }

        internal static void MixedCopiesRemoveOnlyTheExactUntouchedReward()
        {
            Container control = Generated();
            control.Loot.Add(new Item(Thornstep, 1, TimeSpan.FromHours(9)));
            Load(control, protectedByPrefix: false);
            Assertions.True(control.Loot.Single(item => item.Blueprint == Thornstep).Time == TimeSpan.Zero,
                "Control: native removal by blueprint should take the deposited (last) copy.");

            Container container = Generated();
            Item deposited = new Item(Thornstep, 1, TimeSpan.FromHours(9));
            container.Loot.Add(deposited);
            Load(container);
            Assertions.Equal(RetiredLootDecision.RemovedUntouchedRetainedAmbiguous, container.Decision.Value, "Mixed decision.");
            Assertions.True(container.Loot.Count(item => item.Blueprint == Thornstep) == 1 &&
                container.Loot.Contains(deposited), "The deposited copy was not the one retained.");
            AssertNativeRowsIntact(container, "mixed");
        }

        internal static void ClaimedFreshAndProvenanceBoundaries()
        {
            Container claimed = Generated();
            claimed.Loot.RemoveAll(item => item.Blueprint == Thornstep);
            Load(claimed);
            Assertions.Equal(RetiredLootDecision.AlreadyClaimed, claimed.Decision.Value, "Claimed decision.");
            Assertions.Equal(0, claimed.Known[Thornstep], "Claimed history row not retired.");
            AssertNativeRowsIntact(claimed, "claimed");

            var fresh = new Container { Known = new Dictionary<string, int>(Published) };
            foreach (var row in Published) fresh.Loot.Add(new Item(row.Key, row.Value, TimeSpan.Zero));
            Load(fresh);
            Assertions.Equal(RetiredLootDecision.NotInHistory, fresh.Decision.Value, "Fresh decision.");
            Assertions.Equal(0, Count(fresh, Thornstep), "A fresh source generated Thornstep.");
            AssertNativeRowsIntact(fresh, "fresh");

            Assertions.True(RetiredLootReconciliationPolicy.NeverHeldByParty(TimeSpan.Zero) &&
                !RetiredLootReconciliationPolicy.NeverHeldByParty(TimeSpan.FromTicks(1)),
                "Only the unstamped zero time is untouched provenance.");
            var two = RetiredLootReconciliationPolicy.Plan(true, 1,
                new[] { "first", "second" }, value => true);
            Assertions.True(two.Remove.Length == 1 && two.Remove[0] == "first" && two.Retain.Single() == "second",
                "More untouched copies than the recorded history were removed.");
        }
    }
}
