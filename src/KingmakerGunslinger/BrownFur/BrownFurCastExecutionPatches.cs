using System;
using Harmony12;
using Kingmaker.Controllers;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;

namespace KingmakerGunslinger.BrownFur
{
    [HarmonyPatch(typeof(RuleCastSpell), MethodType.Constructor,
        typeof(AbilityData), typeof(Kingmaker.Utility.TargetWrapper))]
    internal static class BrownFurRuleConstructorPatch
    {
        private static void Postfix(RuleCastSpell __instance)
        {
            try { BrownFurCastExecutionRuntime.AttachRule(__instance); }
            catch (Exception exception)
            { BrownFurCastExecutionRuntime.RecordPatchFailure(
                "rule-constructor", exception); }
        }
    }

    [HarmonyPatch(typeof(RuleCastSpell), "OnTrigger", new[] {
        typeof(RulebookEventContext) })]
    [HarmonyAfter("CallOfTheWild")]
    internal static class BrownFurRuleCommitPatch
    {
        private static bool Prefix(RuleCastSpell __instance)
        {
            try
            {
                bool proceed;
                return !BrownFurCastExecutionRuntime.TryCommit(__instance,
                    out proceed) || proceed;
            }
            catch (Exception exception)
            {
                BrownFurCastExecutionRuntime.RecordPatchFailure(
                    "rule-commit", exception);
                return true;
            }
        }

        private static void Postfix(RuleCastSpell __instance)
        {
            try { BrownFurCastExecutionRuntime.AttachProcess(__instance); }
            catch (Exception exception)
            { BrownFurCastExecutionRuntime.RecordPatchFailure(
                "rule-process", exception); }
        }

        private static Exception Finalizer(RuleCastSpell __instance,
            Exception __exception)
        {
            try
            {
                if (__exception != null)
                    BrownFurCastExecutionRuntime.RuleFailed(__instance);
            }
            catch (Exception exception)
            { BrownFurCastExecutionRuntime.RecordPatchFailure(
                "rule-finalizer", exception); }
            return __exception;
        }
    }

    [HarmonyPatch(typeof(AbilityData), "Spend", new Type[0])]
    internal static class BrownFurRejectedSpendPatch
    {
        private static bool Prefix(AbilityData __instance)
        { return !BrownFurCastExecutionRuntime.ConsumeSpendSuppression(__instance); }
    }

    [HarmonyPatch(typeof(UnitUseAbility), "OnEnded", new[] { typeof(bool) })]
    internal static class BrownFurCommandEndPatch
    {
        private static void Postfix(UnitUseAbility __instance)
        { BrownFurCastExecutionRuntime.EndCommand(__instance); }
    }

    [HarmonyPatch(typeof(AbilityExecutionProcess), "Tick", new Type[0])]
    internal static class BrownFurProcessTerminalPatch
    {
        private static void Postfix(AbilityExecutionProcess __instance)
        { BrownFurCastExecutionRuntime.ProcessTick(__instance); }
    }
}
