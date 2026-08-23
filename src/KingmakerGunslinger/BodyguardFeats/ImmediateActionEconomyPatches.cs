using System;
using Harmony12;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using TurnBased.Controllers;

namespace KingmakerGunslinger.BodyguardFeats
{
    [HarmonyPatch(typeof(TurnController), "Prepare", new Type[0])]
    internal static class ImmediateActionTurnPreparePatch
    {
        private static void Prefix(TurnController __instance)
        { ImmediateActionEconomyRuntime.BeginTurnPrepare(__instance); }

        private static void Postfix(TurnController __instance)
        { ImmediateActionEconomyRuntime.EndTurnPrepare(__instance); }
    }

    [HarmonyPatch(typeof(UnitCombatState.Cooldowns), "Clear", new Type[0])]
    internal static class ImmediateActionCooldownClearPatch
    {
        private static void Postfix(UnitCombatState.Cooldowns __instance)
        { ImmediateActionEconomyRuntime.OnCooldownCleared(__instance); }
    }

    [HarmonyPatch(typeof(TurnController), "Dispose", new Type[0])]
    internal static class ImmediateActionTurnDisposePatch
    {
        private static void Prefix(TurnController __instance)
        { ImmediateActionEconomyRuntime.OnTurnDisposed(__instance); }
    }

    [HarmonyPatch(typeof(UnitEntityData), "HasSwiftAction", new Type[0])]
    internal static class ImmediateActionSwiftDebtPatch
    {
        private static void Postfix(UnitEntityData __instance,
            ref bool __result)
        {
            if (__result && ImmediateActionEconomyRuntime
                    .HasChargedTurnDebt(__instance))
                __result = false;
        }
    }

    [HarmonyPatch(typeof(UnitEntityData), "PostLoad", new Type[0])]
    internal static class ImmediateActionPostLoadPatch
    {
        private static void Postfix(UnitEntityData __instance)
        { ImmediateActionEconomyRuntime.RestoreAfterLoad(__instance); }
    }

    [HarmonyPatch(typeof(CombatController),
        "HandlePartyCombatStateChanged", new Type[] { typeof(bool) })]
    internal static class ImmediateActionCombatEndPatch
    {
        private static void Prefix(bool inCombat)
        {
            if (!inCombat)
                ImmediateActionEconomyRuntime.ClearAll("party-combat-ended");
        }
    }
}
