using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElvenBranchedSpear;

namespace KingmakerGunslinger.Acquisition
{
    /// <summary>
    /// LIM-01 (Sprint 6 D1). Runs before native <c>SharedVendorTables.GetTable</c>.
    /// On the first access to a table per loaded game the engine diffs the
    /// persisted <c>KnownItems</c> with the current fixed rows and subtracts each
    /// lowered row from the blueprint/count <c>Entries</c>, then replaces
    /// <c>KnownItems</c>. For the exact LIM-01 retired table/item pairs this
    /// prefix first lowers the history row to the current published count, so
    /// the native diff makes no decrement for them. Ambiguous materialized
    /// copies (an unsold original and a sold-back copy look the same) are
    /// therefore preserved; fresh tables never generate retired rows because
    /// publication removed them. Other tables, other items and absent history
    /// are left to the engine. Cached tables (already reconciled this load) are
    /// not touched.
    /// </summary>
    [HarmonyPatch(typeof(SharedVendorTables), "GetTable")]
    internal static class RetiredVendorHistoryPatch
    {
        private const string GameMvid = "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7";
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly FieldInfo CacheField = typeof(SharedVendorTables).GetField("m_Dictionary", Instance);
        private static readonly FieldInfo TablesField = typeof(SharedVendorTables).GetField("m_PersistentTables", Instance);
        private static readonly Type TableDataType = typeof(SharedVendorTables).GetNestedType("TableData", BindingFlags.NonPublic);
        private static readonly FieldInfo TableField = TableDataType == null ? null : TableDataType.GetField("Table", Instance);
        private static readonly FieldInfo LootField = TableDataType == null ? null : TableDataType.GetField("Loot", Instance);
        private static readonly FieldInfo EntriesField = TableDataType == null ? null : TableDataType.GetField("Entries", Instance);
        private static readonly FieldInfo KnownField = TableDataType == null ? null : TableDataType.GetField("KnownItems", Instance);
        internal static string LastOutcome = "not-used";

        // Exact LIM-01 tables. Oleg retired only his two cold iron branched
        // spear rows (v0.0.183); the Dire Narlmarches village and Pitax traders
        // retired every authored branched spear and Eastern weapon row (v0.0.185).
        internal const string OlegTable = "f720440559fc00949900bfa1575196ac";
        internal const string DireNarlmarchesTable = "f072a8f6889b5f345b7f4e7c74cb3e4c";
        internal const string PitaxTable = "e5ab1fccf37c55f41a20a80c6ba6a460";

        private static bool Prepare()
        {
            // Installed 2.1.7b: GetTable is 398 IL bytes; TableData carries
            // Table, Entries, Loot and KnownItems.
            MethodInfo getTable = typeof(SharedVendorTables).GetMethod("GetTable", Instance);
            return typeof(SharedVendorTables).Module.ModuleVersionId.ToString() == GameMvid &&
                getTable != null && getTable.GetMethodBody().GetILAsByteArray().Length == 398 &&
                CacheField != null && CacheField.FieldType == typeof(Dictionary<BlueprintSharedVendorTable, ItemsCollection>) &&
                TablesField != null && TableDataType != null &&
                TableField != null && TableField.FieldType == typeof(BlueprintSharedVendorTable) &&
                LootField != null && LootField.FieldType == typeof(List<BlueprintUnitLoot>) &&
                EntriesField != null && EntriesField.FieldType == typeof(List<LootEntry>) &&
                KnownField != null && KnownField.FieldType == typeof(Dictionary<BlueprintItem, int>);
        }

        /// <summary>The exact retired items for a LIM-01 table, or null for every other table.</summary>
        internal static BlueprintItem[] RetiredItems(string tableGuid)
        {
            var spears = BlueprintBootstrap.ElvenBranchedSpears;
            var eastern = BlueprintBootstrap.EasternWeapons;
            if (string.Equals(tableGuid, OlegTable, StringComparison.Ordinal))
            {
                if (spears == null) return null;
                return new[] {
                    (BlueprintItem)spears.Require(ElvenBranchedSpearItemKind.ColdIron).Item,
                    (BlueprintItem)spears.Require(ElvenBranchedSpearItemKind.MasterworkColdIron).Item };
            }
            if (string.Equals(tableGuid, DireNarlmarchesTable, StringComparison.Ordinal) ||
                string.Equals(tableGuid, PitaxTable, StringComparison.Ordinal))
            {
                if (spears == null || spears.Named == null || eastern == null || eastern.Named == null) return null;
                return spears.Entries.Select(value => (BlueprintItem)value.Item)
                    .Concat(spears.Named.Entries.Select(value => (BlueprintItem)value.Item))
                    .Concat(eastern.Entries.Select(value => (BlueprintItem)value.Item))
                    .Concat(eastern.Named.Entries.Select(value => (BlueprintItem)value.Item))
                    .Where(item => item != null).Distinct().ToArray();
            }
            return null;
        }

        private static bool IsTarget(string guid) =>
            string.Equals(guid, OlegTable, StringComparison.Ordinal) ||
            string.Equals(guid, DireNarlmarchesTable, StringComparison.Ordinal) ||
            string.Equals(guid, PitaxTable, StringComparison.Ordinal);

        private static void Prefix(SharedVendorTables __instance, BlueprintSharedVendorTable blueprint)
        {
            if (__instance == null || blueprint == null || !IsTarget(blueprint.AssetGuid)) return;
            Dictionary<BlueprintItem, int> known = null;
            BlueprintItem[] retired = null;
            try
            {
                var cache = CacheField.GetValue(__instance) as Dictionary<BlueprintSharedVendorTable, ItemsCollection>;
                if (cache != null && cache.ContainsKey(blueprint)) return; // already reconciled this load
                object data = ((IEnumerable)TablesField.GetValue(__instance)).Cast<object>()
                    .FirstOrDefault(value => ReferenceEquals(TableField.GetValue(value), blueprint));
                if (data == null) { LastOutcome = blueprint.AssetGuid + ":fresh"; return; } // native generation from current rows
                known = KnownField.GetValue(data) as Dictionary<BlueprintItem, int>;
                retired = RetiredItems(blueprint.AssetGuid);
                if (retired == null) throw new InvalidOperationException("LIM-01 retired items are unavailable.");
                Dictionary<BlueprintItem, int> published = SharedVendorTables.GetFixedItems(
                    (List<BlueprintUnitLoot>)LootField.GetValue(data));
                RetiredVendorHistoryPlan<BlueprintItem> plan = RetiredVendorHistoryPolicy.Plan(known, published, retired);
                RetiredVendorHistoryPolicy.Apply(known, plan);
                var entries = (List<LootEntry>)EntriesField.GetValue(data);
                LastOutcome = blueprint.AssetGuid + ":" + plan.Decision + ";rows=" + string.Join(",", plan.Neutralize.Select(row =>
                    (row.Key == null ? "<null>" : row.Key.name) + "->" + row.Value + "/entries=" +
                    entries.Where(entry => entry != null && ReferenceEquals(entry.Item, row.Key)).Sum(entry => entry.Count)).ToArray());
                if (plan.Decision != RetiredVendorHistoryDecision.Neutralized) return;
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Info("acquisition", "retired-vendor.history",
                        "table=" + blueprint.name + ";" + LastOutcome);
            }
            catch (Exception exception) { Fail(blueprint, known, retired, exception); }
        }

        // On a fault, best effort when the history and item identities are
        // available: removing the retired history rows stops the native
        // blueprint/count decrement for this product's retired publication.
        // Not qualified when another publisher adds a positive row for a
        // protected item (the native diff could then add stock); unsupported
        // engine builds are excluded by Prepare.
        private static void Fail(BlueprintSharedVendorTable blueprint, Dictionary<BlueprintItem, int> known,
            BlueprintItem[] retired, Exception exception)
        {
            string retainedNote = "history-not-reachable";
            try
            {
                if (known != null && retired != null)
                {
                    foreach (BlueprintItem item in retired) known.Remove(item);
                    retainedNote = "retired-history-removed";
                }
            }
            catch (Exception inner) { retainedNote = "protect-failed:" + inner.GetType().Name; }
            LastOutcome = "fault:" + (blueprint == null ? "<null>" : blueprint.AssetGuid) + ":" + exception.GetType().Name + ":" + retainedNote;
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Failure("acquisition", "retired-vendor.failed",
                    "LIM-01 retired vendor history protection failed; " + retainedNote + ".", exception);
        }
    }
}
