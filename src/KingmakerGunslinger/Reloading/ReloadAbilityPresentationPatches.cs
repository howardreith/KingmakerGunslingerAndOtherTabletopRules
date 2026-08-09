using Harmony12;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Reloading
{
    internal static class ReloadAbilityPresentation
    {
        internal static bool TryAction(AbilityData ability,
            out EffectiveReloadAction action)
        {
            action = EffectiveReloadAction.Unknown;
            if (ability == null || ability.Caster == null ||
                !ReferenceEquals(ability.Blueprint,
                    BlueprintBootstrap.ReloadTestMusketAbility)) return false;
            ReloadTestMusketAvailability availability =
                ReloadTestMusketRuntime.Evaluate(ability.Caster,
                    BlueprintBootstrap.ProductionFirearms.Musket.Item,
                    BlueprintBootstrap.BasicAmmunition.BlackPowder,
                    BlueprintBootstrap.BasicAmmunition.LeadBall);
            if (!availability.IsAvailable || availability.Plan == null) return false;
            action = availability.Plan.Action;
            return action != EffectiveReloadAction.Unknown;
        }

        internal static UnitCommand.CommandType Command(EffectiveReloadAction action)
        {
            return action == EffectiveReloadAction.Free ? UnitCommand.CommandType.Free :
                action == EffectiveReloadAction.Move ? UnitCommand.CommandType.Move :
                UnitCommand.CommandType.Standard;
        }
    }

    // The two-argument convenience constructor chains into this authoritative
    // constructor. Preserve the granted AbilityData and alter only the action
    // argument consumed by UnitUseAbility.
    [HarmonyPatch(typeof(UnitUseAbility), MethodType.Constructor,
        typeof(UnitCommand.CommandType), typeof(AbilityData), typeof(TargetWrapper))]
    internal static class ReloadAbilityCommandTypePatch
    {
        private static void Prefix(ref UnitCommand.CommandType __0, AbilityData __1)
        {
            EffectiveReloadAction action;
            if (ReloadAbilityPresentation.TryAction(__1, out action))
                __0 = ReloadAbilityPresentation.Command(action);
        }
    }

    [HarmonyPatch(typeof(AbilityData), "get_ActionType")]
    internal static class ReloadAbilityActionTypePatch
    {
        private static void Postfix(AbilityData __instance,
            ref UnitCommand.CommandType __result)
        {
            EffectiveReloadAction action;
            if (ReloadAbilityPresentation.TryAction(__instance, out action))
                __result = ReloadAbilityPresentation.Command(action);
        }
    }

    [HarmonyPatch(typeof(AbilityData), "get_RuntimeActionType")]
    internal static class ReloadAbilityRuntimeActionTypePatch
    {
        private static void Postfix(AbilityData __instance,
            ref UnitCommand.CommandType __result)
        {
            EffectiveReloadAction action;
            if (ReloadAbilityPresentation.TryAction(__instance, out action))
                __result = ReloadAbilityPresentation.Command(action);
        }
    }

    [HarmonyPatch(typeof(AbilityData), "get_RequireFullRoundAction")]
    internal static class ReloadAbilityFullRoundPatch
    {
        private static void Postfix(AbilityData __instance, ref bool __result)
        {
            EffectiveReloadAction action;
            if (ReloadAbilityPresentation.TryAction(__instance, out action))
                __result = action == EffectiveReloadAction.FullRound;
        }
    }
}
