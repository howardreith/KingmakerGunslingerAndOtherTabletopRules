using System;
using System.Reflection;
using Harmony12;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.ContextData;

namespace KingmakerGunslinger.BodyguardFeats
{
    [HarmonyPatch]
    internal static class BodyguardRuleEventCompletionPatch
    {
        private static bool Prepare()
        { return InHarmsWayDeliveryAccess.ContractAvailable; }

        private static MethodBase TargetMethod()
        { return InHarmsWayDeliveryAccess.EventCompletionTarget; }

        // Rulebook.TriggerEventInternal catches failures from OnTrigger,
        // OnEventDidTrigger, and OnDidTrigger before PopEvent is called. A
        // Prefix here is therefore Harmony-1.2's exception-safe completion
        // boundary for every concrete rule event.
        private static void Prefix(RulebookEvent __0)
        {
            try { BodyguardRuntime.RuleEventCompleted(__0); }
            catch { }
        }
    }

    [HarmonyPatch]
    internal static class BodyguardAbilityDeliveryTargetPatch
    {
        private static bool Prepare()
        { return InHarmsWayDeliveryAccess.ContractAvailable; }

        private static MethodBase TargetMethod()
        { return InHarmsWayDeliveryAccess.AbilitySetterTarget; }

        private static void Postfix(AbilityDeliveryTarget __instance,
            RuleAttackRoll __0)
        {
            try
            { BodyguardRuntime.AbilityDeliveryTargetAssigned(__instance, __0); }
            catch { }
        }
    }

    [HarmonyPatch]
    internal static class BodyguardAbilityApplyEffectPatch
    {
        private static bool Prepare()
        { return InHarmsWayDeliveryAccess.ContractAvailable; }

        private static MethodBase TargetMethod()
        { return InHarmsWayDeliveryAccess.AbilityApplyEffectTarget; }

        private static void Postfix(AbilityDeliveryTarget __1)
        {
            try { BodyguardRuntime.AbilityEffectCompleted(__1,
                "apply-effect-postfix"); }
            catch { }
        }
    }

    [HarmonyPatch]
    internal static class BodyguardAbilityContextDisposePatch
    {
        private static bool Prepare()
        { return InHarmsWayDeliveryAccess.ContractAvailable; }

        private static MethodBase TargetMethod()
        { return InHarmsWayDeliveryAccess.ContextDataDisposeTarget; }

        // AbilityExecutionProcess.ApplyEffect disposes ContextAttackData from
        // an existing IL finally block. This prefix therefore runs for both
        // normal and exceptional effect delivery on Harmony 1.2.
        private static void Prefix(ElementsContextData __instance)
        {
            var attack = __instance as ContextAttackData;
            if (attack == null) return;
            try { BodyguardRuntime.AbilityAttackContextDisposed(attack); }
            catch { }
        }
    }

    [HarmonyPatch(typeof(SceneEntitiesState), "Dispose", new Type[0])]
    internal static class BodyguardSceneCleanupPatch
    {
        private static void Prefix()
        { BodyguardRuntime.ClearAll("scene-dispose"); }
    }

}
