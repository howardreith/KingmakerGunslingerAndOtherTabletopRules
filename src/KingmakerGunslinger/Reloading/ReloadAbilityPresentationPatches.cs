using Harmony12;
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
            if (!availability.IsAvailable || availability.Firearm == null) return false;
            action = ReloadActionEconomy.Evaluate(availability.Firearm.Definition,
                RapidReloadRuntime.HasMatchingChoice(ability.Caster,
                    availability.Firearm.Definition.Kind));
            return action != EffectiveReloadAction.Unknown;
        }

        internal static UnitCommand.CommandType Command(EffectiveReloadAction action)
        {
            return action == EffectiveReloadAction.Free ? UnitCommand.CommandType.Free :
                action == EffectiveReloadAction.Move ? UnitCommand.CommandType.Move :
                UnitCommand.CommandType.Standard;
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
