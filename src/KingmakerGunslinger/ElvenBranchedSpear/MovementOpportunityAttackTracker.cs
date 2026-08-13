using System;
using System.Diagnostics;
using System.Reflection;
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

        internal static bool IsRunning(UnitEntityData attacker)
        {
            if (attacker == null || attacker.Commands == null) return false;
            foreach (Kingmaker.UnitLogic.Commands.Base.UnitCommand command in
                attacker.GetAllCommands())
            {
                UnitAttackOfOpportunity opportunity =
                    command as UnitAttackOfOpportunity;
                Marker marker;
                if (opportunity != null && opportunity.IsRunning &&
                    MovementCommands.TryGetValue(opportunity, out marker))
                    return true;
            }
            return false;
        }

        internal static bool IsMovementConstructionBoundary(StackTrace trace)
        {
            if (trace == null) return false;
            bool sawOpportunityFactory = false;
            StackFrame[] frames = trace.GetFrames() ?? Array.Empty<StackFrame>();
            foreach (StackFrame frame in frames)
            {
                MethodBase method = frame == null ? null : frame.GetMethod();
                if (method == null || method.DeclaringType != typeof(UnitCombatState))
                    continue;
                if (string.Equals(method.Name, "AttackOfOpportunity",
                    StringComparison.Ordinal))
                {
                    sawOpportunityFactory = true;
                    continue;
                }
                if (sawOpportunityFactory && string.Equals(method.Name, "Disengage",
                    StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        internal static void Mark(UnitAttackOfOpportunity command)
        {
            if (command == null || !IsMovementConstructionBoundary(
                new StackTrace(1, false))) return;
            MovementCommands.Remove(command);
            MovementCommands.Add(command, new Marker());
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
}
