using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Sprint 6 D1 (LIM-01): retired shared-vendor rows and ambiguous materialized copies, through the native
    /// SharedVendorTables persistence and first-access reconciliation.
    ///
    /// The fixture is CONSTRUCTED legacy-shaped vendor data, not a recovered organic save and not UI trading: the
    /// "seed" launch writes TableData (blueprint/count Entries and KnownItems history) for the exact LIM-01 tables and
    /// one unrelated control table through the native fields, adds retired items to party inventory and stash, and
    /// saves natively. A fresh process then loads that save, so the engine's own first GetTable access reconciles each
    /// table (the native m_Dictionary cache starts empty). "reconcile:native" expects the native decrement (baseline on
    /// the accepted product); "reconcile:protected" expects the LIM-01 protection; both then save. "reload:protected"
    /// loads that save in another fresh process and expects no further change.
    ///
    /// Representations are deliberately ambiguous: one stocked copy with history of one is both "untouched original"
    /// and "bought, then one matching sale"; the probe cannot and does not tell them apart, exactly like product code.
    /// </summary>
    internal sealed class VendorRetirementProbe : IOwnedProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal const string Oleg = "f720440559fc00949900bfa1575196ac";
        internal const string Dire = "f072a8f6889b5f345b7f4e7c74cb3e4c";
        internal const string Pitax = "e5ab1fccf37c55f41a20a80c6ba6a460";

        private readonly string mode;
        private readonly JObject result = new JObject();
        private readonly List<string> problems = new List<string>();
        private readonly IEnumerator<object> script;
        private string verdict;
        private Assembly product;
        private Type tableDataType;
        private FieldInfo cacheField, tablesField, tableField, lootField, entriesField, knownField;

        public VendorRetirementProbe(string mode) { this.mode = mode; result["mode"] = mode; script = Run().GetEnumerator(); }
        public object Result => result;

        public string Poll()
        {
            if (verdict != null) return verdict;
            bool more;
            try { more = script.MoveNext(); }
            catch (Exception exception) { problems.Add("probe exception: " + exception); more = false; }
            if (more) return null;
            result["problems"] = new JArray(problems.Cast<object>().ToArray());
            verdict = problems.Count == 0 ? string.Empty : string.Join(" | ", problems.ToArray());
            return verdict;
        }

        public CleanupReport Release(string testFailure) => ProbeCleanup.RunAll(new KeyValuePair<string, Action>[0]);

        // ------------------------------------------------------------ native access

        private SharedVendorTables Vendors => Game.Instance.Player.SharedVendorTables;
        private Dictionary<BlueprintSharedVendorTable, ItemsCollection> Cache => (Dictionary<BlueprintSharedVendorTable, ItemsCollection>)cacheField.GetValue(Vendors);
        private IList Persistent => (IList)tablesField.GetValue(Vendors);
        private object DataOf(BlueprintSharedVendorTable table) => Persistent.Cast<object>().FirstOrDefault(d => ReferenceEquals(tableField.GetValue(d), table));
        private List<LootEntry> EntriesOf(object data) => (List<LootEntry>)entriesField.GetValue(data);
        private Dictionary<BlueprintItem, int> KnownOf(object data) => (Dictionary<BlueprintItem, int>)knownField.GetValue(data);

        private static BlueprintSharedVendorTable Table(string guid) => ResourcesLibrary.TryGetBlueprint<BlueprintSharedVendorTable>(guid);

        private static object Member(object owner, string name)
        {
            Type t = owner.GetType();
            PropertyInfo p = t.GetProperty(name, Any);
            if (p != null) return p.GetValue(owner, null);
            FieldInfo f = t.GetField(name, Any);
            return f == null ? null : f.GetValue(owner);
        }

        private BlueprintItem[] FamilyItems(string setProperty)
        {
            object set = product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty(setProperty, Any).GetValue(null, null);
            IEnumerable<object> entries = ((IEnumerable)Member(set, "Entries")).Cast<object>();
            IEnumerable<object> named = ((IEnumerable)Member(Member(set, "Named"), "Entries")).Cast<object>();
            return entries.Concat(named).Select(e => (BlueprintItem)Member(e, "Item")).Where(i => i != null).ToArray();
        }

        private BlueprintItem[] Retired(string guid) =>
            product.GetType("KingmakerGunslinger.Acquisition.RetiredVendorHistoryPatch", false) is Type patch ?
                (BlueprintItem[])patch.GetMethod("RetiredItems", Any).Invoke(null, new object[] { guid }) : null;

        private BlueprintItem SpearKind(string kind)
        {
            object set = product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("ElvenBranchedSpears", Any).GetValue(null, null);
            Type kindType = product.GetType("KingmakerGunslinger.ElvenBranchedSpear.ElvenBranchedSpearItemKind", true);
            object entry = set.GetType().GetMethod("Require", Any, null, new[] { kindType }, null).Invoke(set, new[] { Enum.Parse(kindType, kind) });
            return (BlueprintItem)Member(entry, "Item");
        }

        private static JObject Counts(IEnumerable<LootEntry> entries) =>
            new JObject(entries.Where(e => e != null && e.Item != null).GroupBy(e => e.Item.name).OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => new JProperty(g.Key, g.Sum(e => e.Count))));
        private static JObject Counts(ItemsCollection collection) =>
            new JObject(collection.Items.Where(i => i != null).GroupBy(i => i.Blueprint.name).OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => new JProperty(g.Key, g.Sum(i => i.Count))));
        private static JObject Known(Dictionary<BlueprintItem, int> known) => known == null ? null :
            new JObject(known.Where(k => k.Key != null).OrderBy(k => k.Key.name, StringComparer.Ordinal).Select(k => new JProperty(k.Key.name, k.Value)));
        private static int Count(JObject counts, BlueprintItem item) => counts == null || item == null || counts[item.name] == null ? 0 : (int)counts[item.name];

        private IEnumerable<object> Run()
        {
            product = ProductProbes.Entry().Assembly;
            tableDataType = typeof(SharedVendorTables).GetNestedType("TableData", BindingFlags.NonPublic);
            cacheField = typeof(SharedVendorTables).GetField("m_Dictionary", Any);
            tablesField = typeof(SharedVendorTables).GetField("m_PersistentTables", Any);
            tableField = tableDataType.GetField("Table", Any); lootField = tableDataType.GetField("Loot", Any);
            entriesField = tableDataType.GetField("Entries", Any); knownField = tableDataType.GetField("KnownItems", Any);
            result["area"] = Game.Instance.CurrentlyLoadedArea == null ? null : Game.Instance.CurrentlyLoadedArea.name;
            result["scenario"] = "native table access on constructed legacy-shaped vendor data (not UI trading, not an organic legacy save)";
            yield return null;
            IEnumerable<object> body = mode == "seed" ? Seed() : mode.StartsWith("reconcile", StringComparison.Ordinal) ? Reconcile(mode.EndsWith(":protected", StringComparison.Ordinal), true) :
                mode == "reload:protected" ? Reconcile(true, false) : null;
            if (body == null) { problems.Add("unknown mode " + mode); yield break; }
            foreach (object step in body) yield return step;
        }

        // ------------------------------------------------------------ fixture selection

        private sealed class Plan
        {
            internal BlueprintSharedVendorTable Oleg, Dire, Pitax, Control;
            internal BlueprintItem ColdIron, MwColdIron, SpearA, SpearB, SpearNamed, EastA, EastB, EastNamed, NativeAtPitax;
        }

        private Plan Select()
        {
            var plan = new Plan { Oleg = Table(Oleg), Dire = Table(Dire), Pitax = Table(Pitax) };
            BlueprintItem[] spears = FamilyItems("ElvenBranchedSpears"), eastern = FamilyItems("EasternWeapons");
            var owned = new HashSet<BlueprintItem>(spears.Concat(eastern));
            plan.ColdIron = SpearKind("ColdIron"); plan.MwColdIron = SpearKind("MasterworkColdIron");
            plan.SpearA = SpearKind("Mundane"); plan.SpearB = SpearKind("Masterwork");
            object spearSet = product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("ElvenBranchedSpears", Any).GetValue(null, null);
            object easternSet = product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("EasternWeapons", Any).GetValue(null, null);
            plan.SpearNamed = ((IEnumerable)Member(Member(spearSet, "Named"), "Entries")).Cast<object>().Select(e => (BlueprintItem)Member(e, "Item")).First();
            BlueprintItem[] easternGeneric = ((IEnumerable)Member(easternSet, "Entries")).Cast<object>().Select(e => (BlueprintItem)Member(e, "Item")).ToArray();
            plan.EastA = easternGeneric[0]; plan.EastB = easternGeneric[1];
            plan.EastNamed = ((IEnumerable)Member(Member(easternSet, "Named"), "Entries")).Cast<object>().Select(e => (BlueprintItem)Member(e, "Item")).First();
            plan.NativeAtPitax = plan.Pitax.GetComponents<LootItemsPackFixed>().Select(c => c.Item.Item).FirstOrDefault(i => i != null && !owned.Contains(i));
            // Unrelated control table: a native shared vendor table with fixed rows and no project-owned item, outside LIM-01.
            plan.Control = ResourcesLibrary.LibraryObject.GetAllBlueprints().OfType<BlueprintSharedVendorTable>()
                .Where(t => t.AssetGuid != Oleg && t.AssetGuid != Dire && t.AssetGuid != Pitax && !t.name.StartsWith("KMG_", StringComparison.Ordinal) &&
                    t.GetComponents<LootItemsPackFixed>().Any() && !t.GetComponents<LootItemsPackFixed>().Any(c => c.Item.Item != null && owned.Contains(c.Item.Item)))
                .OrderBy(t => t.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
            result["fixture"] = new JObject
            {
                ["tables"] = new JObject { ["oleg"] = plan.Oleg?.name, ["direNarlmarches"] = plan.Dire?.name, ["pitax"] = plan.Pitax?.name,
                    ["control"] = plan.Control == null ? null : plan.Control.name + ":" + plan.Control.AssetGuid },
                ["items"] = new JObject { ["coldIron"] = plan.ColdIron?.name, ["mwColdIron"] = plan.MwColdIron?.name, ["spearA"] = plan.SpearA?.name,
                    ["spearB"] = plan.SpearB?.name, ["spearNamed"] = plan.SpearNamed?.name, ["eastA"] = plan.EastA?.name, ["eastB"] = plan.EastB?.name,
                    ["eastNamed"] = plan.EastNamed?.name, ["nativeAtPitax"] = plan.NativeAtPitax?.name },
                ["familyCounts"] = new JObject { ["spear"] = spears.Length, ["eastern"] = eastern.Length }
            };
            if (new object[] { plan.Oleg, plan.Dire, plan.Pitax, plan.Control, plan.ColdIron, plan.MwColdIron, plan.SpearA, plan.SpearB, plan.SpearNamed,
                plan.EastA, plan.EastB, plan.EastNamed, plan.NativeAtPitax }.Any(x => x == null))
                problems.Add("fixture: an identity is missing: " + result["fixture"].ToString(Formatting.None));
            return plan;
        }

        /// <summary>Exact identity coverage of the product's retired sets (null on a product without the protection).</summary>
        private void RetiredIdentity(Plan plan, bool protectedProduct)
        {
            BlueprintItem[] spears = FamilyItems("ElvenBranchedSpears"), eastern = FamilyItems("EasternWeapons");
            BlueprintItem[] oleg = Retired(Oleg), dire = Retired(Dire), pitax = Retired(Pitax), control = Retired(plan.Control.AssetGuid);
            var row = new JObject
            {
                ["patchPresent"] = product.GetType("KingmakerGunslinger.Acquisition.RetiredVendorHistoryPatch", false) != null,
                ["oleg"] = oleg == null ? null : new JArray(oleg.Select(i => (object)i.name).ToArray()),
                ["direCount"] = dire?.Length, ["pitaxCount"] = pitax?.Length, ["controlProtected"] = control != null
            };
            result["retiredIdentity"] = row;
            if (!protectedProduct) return;
            var expectRegional = new HashSet<BlueprintItem>(spears.Concat(eastern));
            if (oleg == null || oleg.Length != 2 || !oleg.Contains(plan.ColdIron) || !oleg.Contains(plan.MwColdIron)) problems.Add("identity: Oleg's retired set is not exactly the two cold iron spears");
            if (dire == null || !expectRegional.SetEquals(dire)) problems.Add("identity: Dire Narlmarches' retired set is not every spear and Eastern item");
            if (pitax == null || !expectRegional.SetEquals(pitax)) problems.Add("identity: Pitax's retired set is not every spear and Eastern item");
            if (control != null) problems.Add("identity: the unrelated control table is protected");
        }

        // ------------------------------------------------------------ seed

        private object EnsureData(BlueprintSharedVendorTable table)
        {
            object data = DataOf(table);
            if (data != null) return data;
            data = Activator.CreateInstance(tableDataType, true);
            tableField.SetValue(data, table);
            ((List<BlueprintUnitLoot>)lootField.GetValue(data)).Add(table);
            Persistent.Add(data);
            return data;
        }

        private void SeedTable(BlueprintSharedVendorTable table, bool history, Dictionary<BlueprintItem, int> extraKnown, Dictionary<BlueprintItem, int> extraEntries, JObject row)
        {
            bool existed = DataOf(table) != null;
            object data = EnsureData(table);
            row["fixtureHadPersistentData"] = existed;
            if (existed) row["fixtureBefore"] = new JObject { ["entries"] = Counts(EntriesOf(data)), ["known"] = Known(KnownOf(data)) };
            List<LootEntry> entries = EntriesOf(data);
            entries.Clear();
            entries.AddRange(table.GenerateItems()); // native generator for the active rows
            foreach (var e in extraEntries) if (e.Value > 0) entries.Add(new LootEntry { Item = e.Key, Count = e.Value });
            Dictionary<BlueprintItem, int> known = null;
            if (history)
            {
                known = SharedVendorTables.GetFixedItems((List<BlueprintUnitLoot>)lootField.GetValue(data));
                foreach (var k in extraKnown) known[k.Key] = (known.ContainsKey(k.Key) ? known[k.Key] : 0) + k.Value;
            }
            knownField.SetValue(data, known);
            row["seeded"] = new JObject { ["entries"] = Counts(entries), ["known"] = Known(known),
                ["publishedRows"] = Known(SharedVendorTables.GetFixedItems((List<BlueprintUnitLoot>)lootField.GetValue(data))) };
        }

        private JObject PartyState(Plan plan)
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            Func<ItemsCollection, JObject> retiredIn = c => new JObject(new[] { plan.ColdIron, plan.EastNamed }.Select(i =>
                new JProperty(i.name, c.Items.Where(x => x != null && ReferenceEquals(x.Blueprint, i)).Sum(x => x.Count))));
            return new JObject
            {
                ["inventory"] = retiredIn(Game.Instance.Player.Inventory), ["inventoryTotal"] = Game.Instance.Player.Inventory.Items.Sum(x => x.Count),
                ["stash"] = retiredIn(Game.Instance.Player.SharedStash), ["stashTotal"] = Game.Instance.Player.SharedStash.Items.Sum(x => x.Count),
                ["mainEquipment"] = new JArray(main.Body.AllSlots.Where(s => s.HasItem).Select(s => (object)s.Item.Blueprint.name).OrderBy(n => (string)n, StringComparer.Ordinal).ToArray())
            };
        }

        private IEnumerable<object> Seed()
        {
            Plan plan = Select();
            if (problems.Count > 0) yield break;
            RetiredIdentity(plan, false);
            foreach (var t in new[] { plan.Oleg, plan.Dire, plan.Pitax, plan.Control })
                if (Cache.ContainsKey(t)) { problems.Add("seed: " + t.name + " was already accessed in this process; seeding would not reach reconciliation"); yield break; }
            // Fresh publication (native generator used by a table with no persisted data): retired rows absent.
            var fresh = new JObject();
            result["freshPublication"] = fresh;
            foreach (var t in new[] { plan.Oleg, plan.Dire, plan.Pitax })
            {
                BlueprintItem[] fixedRows = t.GetComponents<LootItemsPackFixed>().Select(c => c.Item.Item).Where(i => i != null).ToArray();
                List<LootEntry> generated = t.GenerateItems();
                BlueprintItem[] retiredHere = t == plan.Oleg ? new[] { plan.ColdIron, plan.MwColdIron } : FamilyItems("ElvenBranchedSpears").Concat(FamilyItems("EasternWeapons")).ToArray();
                fresh[t.name] = new JObject { ["fixedRows"] = fixedRows.Length, ["generated"] = Counts(generated),
                    ["retiredInFixedRows"] = fixedRows.Count(retiredHere.Contains), ["retiredGenerated"] = generated.Count(e => retiredHere.Contains(e.Item)) };
                if (fixedRows.Any(retiredHere.Contains) || generated.Any(e => retiredHere.Contains(e.Item))) problems.Add("fresh: " + t.name + " still publishes a retired row");
            }
            if (!plan.Oleg.GetComponents<LootItemsPackFixed>().Any(c => ReferenceEquals(c.Item.Item, plan.SpearA)) ||
                !plan.Oleg.GetComponents<LootItemsPackFixed>().Any(c => ReferenceEquals(c.Item.Item, plan.SpearB)))
                problems.Add("fresh: Oleg's active mundane/masterwork branched spear rows are not published");

            var seeds = new JObject();
            result["seed"] = seeds;
            // Oleg (history): cold iron 1 stocked (ambiguous: untouched or bought-then-one-sale); masterwork cold iron bought (none stocked).
            SeedTable(plan.Oleg, true, new Dictionary<BlueprintItem, int> { { plan.ColdIron, 1 }, { plan.MwColdIron, 1 } },
                new Dictionary<BlueprintItem, int> { { plan.ColdIron, 1 } }, (JObject)(seeds["oleg"] = new JObject()));
            // Pitax (history, both families): single ambiguous, multiple matching, mixed, bought-out; plus a native row lowered by one (control).
            var pitaxKnown = new Dictionary<BlueprintItem, int> { { plan.SpearA, 1 }, { plan.SpearB, 1 }, { plan.SpearNamed, 1 }, { plan.EastA, 1 }, { plan.EastB, 1 }, { plan.EastNamed, 1 }, { plan.NativeAtPitax, 1 } };
            var pitaxEntries = new Dictionary<BlueprintItem, int> { { plan.SpearA, 3 }, { plan.SpearB, 2 }, { plan.SpearNamed, 1 }, { plan.EastA, 1 }, { plan.EastNamed, 2 }, { plan.NativeAtPitax, 1 } };
            SeedTable(plan.Pitax, true, pitaxKnown, pitaxEntries, (JObject)(seeds["pitax"] = new JObject()));
            // Dire Narlmarches (missing history): both families stocked.
            SeedTable(plan.Dire, false, new Dictionary<BlueprintItem, int>(), new Dictionary<BlueprintItem, int> { { plan.SpearA, 2 }, { plan.EastA, 1 } },
                (JObject)(seeds["direNarlmarches"] = new JObject()));
            // Unrelated control table: the same cold iron spear with a lowered history row, outside LIM-01.
            SeedTable(plan.Control, true, new Dictionary<BlueprintItem, int> { { plan.ColdIron, 1 } }, new Dictionary<BlueprintItem, int> { { plan.ColdIron, 1 } },
                (JObject)(seeds["control"] = new JObject()));
            // Party controls: retired items held by the party and in the stash.
            Game.Instance.Player.Inventory.Add(plan.ColdIron, 1);
            Game.Instance.Player.Inventory.Add(plan.EastNamed, 1);
            Game.Instance.Player.SharedStash.Add(plan.ColdIron, 1);
            result["partyAtSeed"] = PartyState(plan);
            yield return null;
            foreach (var t in new[] { plan.Oleg, plan.Dire, plan.Pitax, plan.Control })
                if (Cache.ContainsKey(t)) { problems.Add("seed: " + t.name + " was accessed before the save"); yield break; }
            bool allowed = Game.Instance.SaveManager.IsSaveAllowed();
            result["saveAllowed"] = allowed;
            if (!allowed) { problems.Add("seed: native save admission refused"); yield break; }
            foreach (object step in SaveAs("d1seed")) yield return step;
        }

        private IEnumerable<object> SaveAs(string label)
        {
            bool saved = false;
            Game.Instance.SaveGame(Game.Instance.SaveManager.CreateNewSave(label), () => saved = true);
            float until = Time.realtimeSinceStartup + 120f;
            while ((!saved || Game.Instance.SaveManager.CommitInProgress || LoadingProcess.Instance.IsLoadingInProcess) && Time.realtimeSinceStartup < until)
                yield return null;
            bool finished = saved && !Game.Instance.SaveManager.CommitInProgress && !LoadingProcess.Instance.IsLoadingInProcess;
            result["save." + label] = new JObject { ["callbackReceived"] = saved, ["finished"] = finished };
            if (!finished) problems.Add("save " + label + " did not finish");
        }

        // ------------------------------------------------------------ reconcile / reload

        /// <summary>The native first-access diff on loaded data, as decompiled (oracle for every non-case row).</summary>
        private static Dictionary<string, int> NativeExpectation(JObject entries, Dictionary<BlueprintItem, int> known, Dictionary<BlueprintItem, int> fixedRows, HashSet<BlueprintItem> protectedItems)
        {
            var counts = entries.Properties().ToDictionary(p => p.Name, p => (int)p.Value);
            if (known == null) return counts;
            foreach (BlueprintItem item in known.Keys.Union(fixedRows.Keys))
            {
                int oldCount = known.ContainsKey(item) ? known[item] : 0, newCount = fixedRows.ContainsKey(item) ? fixedRows[item] : 0;
                if (protectedItems.Contains(item) && oldCount > newCount) continue;
                int diff = newCount - oldCount;
                if (diff == 0) continue;
                int have = counts.ContainsKey(item.name) ? counts[item.name] : 0;
                counts[item.name] = Math.Max(0, have + diff);
            }
            return counts;
        }

        private IEnumerable<object> Reconcile(bool protectedExpected, bool saveAfter)
        {
            Plan plan = Select();
            if (problems.Count > 0) yield break;
            RetiredIdentity(plan, protectedExpected);
            var tables = new[] { new { Key = "oleg", T = plan.Oleg }, new { Key = "pitax", T = plan.Pitax }, new { Key = "direNarlmarches", T = plan.Dire }, new { Key = "control", T = plan.Control } };
            JObject party0 = PartyState(plan);
            var rows = new JObject();
            result["tables"] = rows;
            var after = new Dictionary<string, JObject>();
            foreach (var x in tables)
            {
                var row = new JObject();
                rows[x.Key] = row;
                row["cachedBeforeAccess"] = Cache.ContainsKey(x.T);
                if (Cache.ContainsKey(x.T)) { problems.Add(x.Key + ": already cached before the first access; reconciliation would be skipped"); continue; }
                object data = DataOf(x.T);
                if (data == null) { problems.Add(x.Key + ": no persisted table data after load"); continue; }
                JObject loaded = Counts(EntriesOf(data));
                Dictionary<BlueprintItem, int> knownLoaded = KnownOf(data) == null ? null : new Dictionary<BlueprintItem, int>(KnownOf(data));
                Dictionary<BlueprintItem, int> fixedRows = SharedVendorTables.GetFixedItems((List<BlueprintUnitLoot>)lootField.GetValue(data));
                row["loaded"] = new JObject { ["entries"] = loaded, ["known"] = Known(knownLoaded) };
                var protectedItems = new HashSet<BlueprintItem>(protectedExpected ? (Retired(x.T.AssetGuid) ?? new BlueprintItem[0]) : new BlueprintItem[0]);
                Dictionary<string, int> expected = NativeExpectation(loaded, knownLoaded, fixedRows, protectedItems);
                ItemsCollection first = Vendors.GetTable(x.T);            // native first access: reconciliation
                JObject firstCounts = Counts(first);
                ItemsCollection second = Vendors.GetTable(x.T);           // reopen: cached, no reconciliation
                row["afterFirstAccess"] = firstCounts;
                row["knownAfter"] = Known(KnownOf(data));
                row["reopenSameCollection"] = ReferenceEquals(first, second);
                row["reopenCounts"] = Counts(second);
                row["productOutcome"] = product.GetType("KingmakerGunslinger.Acquisition.RetiredVendorHistoryPatch", false)?.GetField("LastOutcome", Any)?.GetValue(null) as string;
                var keys = new HashSet<string>(expected.Where(e => e.Value > 0).Select(e => e.Key).Concat(firstCounts.Properties().Select(p => p.Name)));
                string[] mismatch = keys.OrderBy(k => k, StringComparer.Ordinal).Where(k =>
                        (expected.ContainsKey(k) ? Math.Max(0, expected[k]) : 0) != (firstCounts[k] == null ? 0 : (int)firstCounts[k]))
                    .Select(k => k + ": expected " + (expected.ContainsKey(k) ? Math.Max(0, expected[k]) : 0) + ", got " + (firstCounts[k] == null ? 0 : (int)firstCounts[k])).ToArray();
                row["oracleMismatches"] = new JArray(mismatch.Cast<object>().ToArray());
                if (mismatch.Length > 0) problems.Add(x.Key + ": counts differ from the " + (protectedExpected ? "protected" : "native") + " expectation: " + string.Join("; ", mismatch));
                if (!JToken.DeepEquals(firstCounts, Counts(second))) problems.Add(x.Key + ": the reopened table changed");
                if (KnownOf(data) != null && KnownOf(data).Keys.Any(k => !fixedRows.ContainsKey(k))) problems.Add(x.Key + ": history keeps a row the publication no longer has");
                after[x.Key] = firstCounts;
            }
            // Case table (exact seeded rows).
            Func<string, BlueprintItem, int> got = (k, i) => after.ContainsKey(k) ? Count(after[k], i) : -1;
            Func<string, BlueprintItem, int> was = (k, i) => Count((JObject)rows[k]?["loaded"]?["entries"], i);
            var cases = new JObject
            {
                ["2/4 Oleg cold iron, one stocked (untouched or bought+one sale)"] = Case(was("oleg", plan.ColdIron), got("oleg", plan.ColdIron)),
                ["3 Oleg masterwork cold iron, bought out"] = Case(was("oleg", plan.MwColdIron), got("oleg", plan.MwColdIron)),
                ["5 Pitax spear A, three stocked (multiple matching)"] = Case(was("pitax", plan.SpearA), got("pitax", plan.SpearA)),
                ["5 Pitax spear B, two stocked (mixed)"] = Case(was("pitax", plan.SpearB), got("pitax", plan.SpearB)),
                ["2/4 Pitax named spear, one stocked"] = Case(was("pitax", plan.SpearNamed), got("pitax", plan.SpearNamed)),
                ["2/4 Pitax Eastern A, one stocked"] = Case(was("pitax", plan.EastA), got("pitax", plan.EastA)),
                ["3 Pitax Eastern B, bought out"] = Case(was("pitax", plan.EastB), got("pitax", plan.EastB)),
                ["5 Pitax Eastern named, two stocked"] = Case(was("pitax", plan.EastNamed), got("pitax", plan.EastNamed)),
                ["6 Dire spear A, missing history"] = Case(was("direNarlmarches", plan.SpearA), got("direNarlmarches", plan.SpearA)),
                ["6 Dire Eastern A, missing history"] = Case(was("direNarlmarches", plan.EastA), got("direNarlmarches", plan.EastA)),
                ["control: native row lowered by one at Pitax"] = Case(was("pitax", plan.NativeAtPitax), got("pitax", plan.NativeAtPitax)),
                ["control: same cold iron spear at an unrelated table"] = Case(was("control", plan.ColdIron), got("control", plan.ColdIron))
            };
            result["cases"] = cases;
            // Party controls: unchanged by any access.
            JObject party1 = PartyState(plan);
            result["party"] = new JObject { ["before"] = party0, ["after"] = party1 };
            if (!JToken.DeepEquals(party0, party1)) problems.Add("party inventory, stash or equipment changed during vendor access");
            if (saveAfter)
            {
                yield return null;
                foreach (object step in SaveAs("d1recon")) yield return step;
            }
        }

        private static JObject Case(int before, int after) => new JObject { ["before"] = before, ["after"] = after };
    }
}
