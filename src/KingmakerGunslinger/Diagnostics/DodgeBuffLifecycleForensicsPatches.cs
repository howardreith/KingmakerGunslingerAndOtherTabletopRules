using System;
using System.Diagnostics;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.EntitySystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.Diagnostics
{
    [HarmonyPatch(typeof(BuffCollection), "TriggerRuleApplyBuff", new[] { typeof(BlueprintBuff), typeof(MechanicsContext), typeof(TimeSpan?) })]
    internal static class DodgeForensicsTriggerRuleApplyBuffPatch
    {
        private static void Prefix(BlueprintBuff blueprint, MechanicsContext context, TimeSpan? duration)
        { DodgeBuffLifecycleForensics.RecordCreation("trigger-rule-apply-buff-prefix", blueprint, context, duration, null, null, null); }
        private static void Postfix(BlueprintBuff blueprint, MechanicsContext context, TimeSpan? duration, Buff __result)
        { DodgeBuffLifecycleForensics.RecordCreation("trigger-rule-apply-buff-postfix", blueprint, context, duration, __result, __result != null, null); }
        private static Exception Finalizer(BlueprintBuff blueprint, MechanicsContext context, TimeSpan? duration, Exception __exception)
        { if (__exception != null) DodgeBuffLifecycleForensics.RecordCreation("trigger-rule-apply-buff-exception", blueprint, context, duration, null, false, __exception); return __exception; }
    }

    [HarmonyPatch(typeof(BuffCollection), "AddBuffInternal", new[] { typeof(BlueprintBuff), typeof(MechanicsContext), typeof(TimeSpan?) })]
    internal static class DodgeForensicsAddBuffInternalPatch
    {
        private static void Prefix(BlueprintBuff blueprint, MechanicsContext context, TimeSpan? duration)
        { DodgeBuffLifecycleForensics.RecordCreation("add-buff-internal-prefix", blueprint, context, duration, null, true, null); }
        private static void Postfix(BlueprintBuff blueprint, MechanicsContext context, TimeSpan? duration, Buff __result)
        { DodgeBuffLifecycleForensics.RecordCreation("add-buff-internal-postfix", blueprint, context, duration, __result, true, null); }
    }

    [HarmonyPatch(typeof(BuffCollection), "OnFactCreated", new[] { typeof(Fact) })]
    internal static class DodgeForensicsOnFactCreatedPatch
    { private static void Postfix(Fact fact, BuffCollection __instance) { DodgeBuffLifecycleForensics.Record("on-fact-created", fact as Buff, __instance, null, null, null); } }
    [HarmonyPatch(typeof(BuffCollection), "OnFactAdded", new[] { typeof(Fact) })]
    internal static class DodgeForensicsOnFactAddedPatch
    { private static void Postfix(Fact fact, BuffCollection __instance) { DodgeBuffLifecycleForensics.Record("on-fact-added", fact as Buff, __instance, null, null, null); } }
    [HarmonyPatch(typeof(BuffCollection), "UpdateNextEvent")]
    internal static class DodgeForensicsUpdateNextEventPatch
    {
        private static void Prefix(BuffCollection __instance) { DodgeBuffLifecycleForensics.Record("update-next-event-prefix", null, __instance, null, null, null); }
        private static void Postfix(BuffCollection __instance) { DodgeBuffLifecycleForensics.Record("update-next-event-postfix", null, __instance, null, null, null); }
    }
    [HarmonyPatch(typeof(BuffCollection), "Tick")]
    internal static class DodgeForensicsTickPatch
    {
        private static void Prefix(BuffCollection __instance) { DodgeBuffLifecycleForensics.Record("tick-prefix", null, __instance, null, null, null); }
        private static void Postfix(BuffCollection __instance) { DodgeBuffLifecycleForensics.Record("tick-postfix", null, __instance, null, null, null); }
    }
    [HarmonyPatch(typeof(FactCollection), "RemoveFact", new[] { typeof(Fact) })]
    internal static class DodgeForensicsRemoveFactPatch
    {
        private static void Prefix(Fact fact) { var b = fact as Buff; DodgeBuffLifecycleForensics.Record("remove-fact-prefix", b, b == null || b.Owner == null ? null : b.Owner.Buffs, null, null, null); }
        private static void Postfix(Fact fact) { var b = fact as Buff; DodgeBuffLifecycleForensics.Record("remove-fact-postfix", b, b == null || b.Owner == null ? null : b.Owner.Buffs, null, null, null); }
    }
    [HarmonyPatch(typeof(BuffCollection), "OnFactRemoved", new[] { typeof(Fact) })]
    internal static class DodgeForensicsOnFactRemovedPatch
    { private static void Prefix(Fact fact, BuffCollection __instance) { DodgeBuffLifecycleForensics.Record("on-fact-removed", fact as Buff, __instance, null, null, null); } }
    [HarmonyPatch(typeof(Buff), "OnRemove")]
    internal static class DodgeForensicsBuffOnRemovePatch
    { private static void Prefix(Buff __instance) { DodgeBuffLifecycleForensics.Record("buff-on-remove-prefix", __instance, null, null, null, null); } private static void Postfix(Buff __instance) { DodgeBuffLifecycleForensics.Record("buff-on-remove-postfix", __instance, null, null, null, null); } }
    [HarmonyPatch(typeof(Buff), "Dispose")]
    internal static class DodgeForensicsBuffDisposePatch
    { private static void Prefix(Buff __instance) { DodgeBuffLifecycleForensics.Record("buff-dispose-prefix", __instance, null, null, null, null); } private static void Postfix(Buff __instance) { DodgeBuffLifecycleForensics.Record("buff-dispose-postfix", __instance, null, null, null, null); } }
}
