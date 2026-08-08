using System;
using System.Linq;
using Harmony12;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Deeds
{
    internal static class LightningReloadPresentation
    {
        internal static bool TryAction(AbilityData ability,
            out LightningReloadAction action)
        {
            action = LightningReloadAction.Unknown;
            if (ability == null || ability.Caster == null ||
                BlueprintBootstrap.GunslingerClass == null ||
                !ReferenceEquals(ability.Blueprint,
                    BlueprintBootstrap.GunslingerClass.LightningReload.Ability))
                return false;
            LightningReloadAbilityLogic logic = ability.Blueprint.ComponentsArray
                .OfType<LightningReloadAbilityLogic>().SingleOrDefault();
            if (logic == null) return false;
            LightningReloadAvailability availability = LightningReloadRuntime.Evaluate(
                ability.Caster, logic.BlackPowder, logic.LeadBall, logic.UsedMarker);
            if (!availability.Decision.IsAvailable) return false;
            action = availability.Decision.Action;
            return action != LightningReloadAction.Unknown;
        }

        internal static UnitCommand.CommandType Command(LightningReloadAction action)
        {
            return action == LightningReloadAction.Free
                ? UnitCommand.CommandType.Free : UnitCommand.CommandType.Swift;
        }
    }

    [HarmonyPatch(typeof(UnitUseAbility), MethodType.Constructor,
        typeof(UnitCommand.CommandType), typeof(AbilityData), typeof(TargetWrapper))]
    internal static class LightningReloadCommandTypePatch
    {
        private static void Prefix(ref UnitCommand.CommandType __0, AbilityData __1)
        {
            LightningReloadAction action;
            if (LightningReloadPresentation.TryAction(__1, out action))
                __0 = LightningReloadPresentation.Command(action);
        }
    }

    [HarmonyPatch(typeof(AbilityData), "get_ActionType")]
    internal static class LightningReloadActionTypePatch
    {
        private static void Postfix(AbilityData __instance,
            ref UnitCommand.CommandType __result)
        {
            LightningReloadAction action;
            if (LightningReloadPresentation.TryAction(__instance, out action))
                __result = LightningReloadPresentation.Command(action);
        }
    }

    [HarmonyPatch(typeof(AbilityData), "get_RuntimeActionType")]
    internal static class LightningReloadRuntimeActionTypePatch
    {
        private static void Postfix(AbilityData __instance,
            ref UnitCommand.CommandType __result)
        {
            LightningReloadAction action;
            if (LightningReloadPresentation.TryAction(__instance, out action))
                __result = LightningReloadPresentation.Command(action);
        }
    }
}
