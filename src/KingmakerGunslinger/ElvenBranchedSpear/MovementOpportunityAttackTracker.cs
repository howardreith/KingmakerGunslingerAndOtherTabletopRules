using System;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    /// <summary>
    /// Correlates only commands constructed by UnitCombatState.Disengage.
    /// Kingmaker's attack rule records AoO status but not provocation reason.
    /// </summary>
    internal static class MovementOpportunityAttackTracker
    {
        private sealed class Marker { }

        private static readonly ConditionalWeakTable<UnitAttackOfOpportunity, Marker>
            MovementCommands = new ConditionalWeakTable<UnitAttackOfOpportunity, Marker>();
        [ThreadStatic]
        private static UnitAttackOfOpportunity ActiveMovementAttack;
        [ThreadStatic]
        private static int DisengageDepth;

        internal static bool IsRunning(UnitEntityData attacker)
        {
            return attacker != null && ActiveMovementAttack != null &&
                ReferenceEquals(ActiveMovementAttack.Executor, attacker);
        }

        internal static bool EnterOpportunityAction(
            UnitAttackOfOpportunity opportunity)
        {
            Marker marker;
            bool movement = opportunity != null && MovementCommands.TryGetValue(
                opportunity, out marker);
            if (movement) ActiveMovementAttack = opportunity;
            return movement;
        }

        internal static void ExitOpportunityAction(bool entered)
        {
            if (entered) ActiveMovementAttack = null;
        }

        internal static void EnterDisengage()
        {
            DisengageDepth++;
        }

        internal static void ExitDisengage()
        {
            if (DisengageDepth > 0) DisengageDepth--;
        }

        internal static void Mark(UnitAttackOfOpportunity command)
        {
            if (command == null || DisengageDepth <= 0) return;
            MovementCommands.Remove(command);
            MovementCommands.Add(command, new Marker());
        }
    }

    [HarmonyPatch(typeof(UnitCombatState), "Disengage",
        new[] { typeof(UnitEntityData) })]
    internal static class MovementOpportunityDisengageBoundaryPatch
    {
        private static void Prefix()
        {
            MovementOpportunityAttackTracker.EnterDisengage();
        }

        private static void Postfix()
        {
            MovementOpportunityAttackTracker.ExitDisengage();
        }
    }

    [HarmonyPatch(typeof(UnitAttackOfOpportunity), MethodType.Constructor,
        new[] { typeof(UnitEntityData) })]
    internal static class MovementOpportunityCommandConstructionPatch
    {
        private static void Postfix(UnitAttackOfOpportunity __instance)
        {
            MovementOpportunityAttackTracker.Mark(__instance);
        }
    }

    [HarmonyPatch(typeof(UnitAttackOfOpportunity), "OnAction")]
    internal static class MovementOpportunityActionBoundaryPatch
    {
        private static void Prefix(UnitAttackOfOpportunity __instance,
            out bool __state)
        {
            __state = MovementOpportunityAttackTracker.EnterOpportunityAction(
                __instance);
        }

        private static void Postfix(bool __state)
        {
            MovementOpportunityAttackTracker.ExitOpportunityAction(__state);
        }
    }

}
