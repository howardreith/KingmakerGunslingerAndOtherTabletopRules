using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.DirectSerialization;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Items;
using Kingmaker.View.MapObjects;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Acquisition
{
    /// <summary>
    /// Runs before native <c>LootComponent.OnDataSetted</c>, whose
    /// <c>LootPersistentData.UpdateLoot</c> would subtract a retired named
    /// weapon by blueprint from its exact former source. Only that exact
    /// authored source is touched. Untouched generated rewards (persisted
    /// <c>ItemEntity.Time</c> still zero) are removed exactly; any instance the
    /// party has held is retained. The retired history row is then set to zero,
    /// so the native diff does nothing for it and repeat loads are no-ops.
    /// Missing history is left to the engine, which skips reconciliation.
    /// </summary>
    [HarmonyPatch(typeof(LootComponent), "OnDataSetted")]
    internal static class RetiredLootReconciliationPatch
    {
        private const string GameMvid = "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7";
        private const BindingFlags Declared = BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private static readonly PropertyInfo DataProperty =
            typeof(LootComponent).GetProperty("Data", Declared);
        private static readonly FieldInfo KnownItemsField =
            typeof(LootComponent.LootPersistentData).GetField("m_KnownItems",
                BindingFlags.Instance | BindingFlags.NonPublic);
        internal static string LastOutcome = "not-used";

        private static bool Prepare()
        {
            // Installed 2.1.7b: OnDataSetted 0x06001A28 is 59 IL bytes and
            // LootPersistentData.UpdateLoot 0x0600A6C8 is 219 IL bytes.
            MethodInfo onDataSetted = typeof(LootComponent).GetMethod("OnDataSetted", Declared);
            MethodInfo updateLoot = typeof(LootComponent.LootPersistentData).GetMethod("UpdateLoot", Declared);
            return typeof(LootComponent).Module.ModuleVersionId.ToString() == GameMvid &&
                onDataSetted != null && onDataSetted.GetMethodBody().GetILAsByteArray().Length == 59 &&
                updateLoot != null && updateLoot.GetMethodBody().GetILAsByteArray().Length == 219 &&
                DataProperty != null && DataProperty.PropertyType == typeof(LootComponent.LootPersistentData) &&
                KnownItemsField != null &&
                KnownItemsField.FieldType == typeof(Dictionary<BlueprintItem, int>);
        }

        private static void Prefix(LootComponent __instance)
        {
            ElvenBranchedSpearCampaignBlueprints.RetiredLootSource[] sources;
            try
            {
                if (__instance == null) return;
                sources = ElvenBranchedSpearCampaignBlueprints.RetiredLootSources.Where(source =>
                    (__instance.LootTables ?? new BlueprintReference[0]).Any(reference =>
                    {
                        var loot = reference == null ? null : reference.Get() as BlueprintLoot;
                        return loot != null && string.Equals(loot.AssetGuid, source.Guid, StringComparison.Ordinal);
                    })).ToArray();
            }
            catch (Exception exception) { Fail("identity", exception); return; }
            foreach (var source in sources)
            {
                try { Reconcile(__instance, source); }
                catch (Exception exception)
                {
                    Fail(source.Kind.ToString(), exception);
                    Protect(__instance, source);
                }
            }
        }

        private static void Reconcile(LootComponent component,
            ElvenBranchedSpearCampaignBlueprints.RetiredLootSource source)
        {
            var data = DataProperty.GetValue(component, null) as LootComponent.LootPersistentData;
            var spears = BlueprintBootstrap.ElvenBranchedSpears;
            if (data == null || data.Loot == null || spears == null || spears.Named == null) return;
            BlueprintItem retired = spears.Named.Require(source.Kind).Item;
            // Never act if the current definition still publishes the item.
            if (component.MapObjectLoot.Any(entry => entry != null && ReferenceEquals(entry.Item, retired)))
            { LastOutcome = source.Kind + ":still-published-skip"; return; }
            var known = KnownItemsField.GetValue(data) as Dictionary<BlueprintItem, int>;
            int knownCount = 0;
            if (known != null) known.TryGetValue(retired, out knownCount);
            ItemEntity[] present = data.Loot.Items.Where(item => item != null &&
                ReferenceEquals(item.Blueprint, retired)).ToArray();
            RetiredLootPlan<ItemEntity> plan = RetiredLootReconciliationPolicy.Plan(
                known != null, knownCount, present,
                item => item.HoldingSlot == null &&
                    RetiredLootReconciliationPolicy.NeverHeldByParty(item.Time));
            foreach (ItemEntity item in plan.Remove)
            {
                data.Loot.Remove(item);
                if (data.Loot.Items.Any(value => ReferenceEquals(value, item)))
                    throw new InvalidOperationException("The exact untouched retired reward survived removal.");
            }
            if (plan.RetireHistory) known[retired] = 0;
            LastOutcome = source.Kind + ":" + plan.Decision + ";removed=" + plan.Remove.Length +
                ";retained=" + plan.Retain.Length + ";known=" + (known == null ? "none" : knownCount.ToString());
            if (plan.Decision == RetiredLootDecision.NotInHistory) return;
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("acquisition", "retired-loot.reconciled",
                    "source=" + source.Guid + ";entity=" + (component.MapObject == null ||
                        component.MapObject.Data == null ? "<none>" : component.MapObject.Data.UniqueId) +
                    ";" + LastOutcome);
        }

        // On any fault, prefer retention: retiring the history row stops the
        // native blueprint decrement, so an ambiguous copy cannot be deleted.
        private static void Protect(LootComponent component,
            ElvenBranchedSpearCampaignBlueprints.RetiredLootSource source)
        {
            try
            {
                var data = DataProperty.GetValue(component, null) as LootComponent.LootPersistentData;
                var known = data == null ? null : KnownItemsField.GetValue(data) as Dictionary<BlueprintItem, int>;
                var spears = BlueprintBootstrap.ElvenBranchedSpears;
                if (known == null || spears == null || spears.Named == null) return;
                BlueprintItem retired = spears.Named.Require(source.Kind).Item;
                if (known.ContainsKey(retired)) known[retired] = 0;
                LastOutcome = source.Kind + ":fault-retained";
            }
            catch (Exception exception) { Fail(source.Kind + ".protect", exception); }
        }

        private static void Fail(string scope, Exception exception)
        {
            LastOutcome = "fault:" + scope + ":" + exception.GetType().Name;
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Failure("acquisition", "retired-loot.failed",
                    "Retired campaign loot reconciliation failed for " + scope +
                    "; the ambiguous instance is retained where possible.", exception);
        }
    }
}
