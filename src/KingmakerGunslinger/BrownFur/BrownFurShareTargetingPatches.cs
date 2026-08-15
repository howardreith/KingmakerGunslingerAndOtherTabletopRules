using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;

namespace KingmakerGunslinger.BrownFur
{
    [HarmonyPatch(typeof(AbilityData), "get_TargetAnchor")]
    [HarmonyAfter("CallOfTheWild")]
    internal static class BrownFurShareTargetAnchorPatch
    {
        private static void Postfix(AbilityData __instance,
            ref AbilityTargetAnchor __result)
        {
            try
            {
                AbilityTargetAnchor value;
                if (BrownFurShareTargetingRuntime.TryOverrideAnchor(__instance,
                    out value)) __result = value;
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(AbilityData), "CanTarget", new[] {
        typeof(TargetWrapper) })]
    [HarmonyAfter("CallOfTheWild")]
    internal static class BrownFurShareCanTargetPatch
    {
        private static void Postfix(AbilityData __instance, TargetWrapper __0,
            ref bool __result)
        {
            try
            {
                bool value;
                if (BrownFurShareTargetingRuntime.TryOverrideTarget(__instance,
                    __0, out value)) __result = value;
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(AbilityData), "GetApproachDistance", new[] {
        typeof(UnitEntityData) })]
    [HarmonyAfter("CallOfTheWild")]
    internal static class BrownFurShareApproachDistancePatch
    {
        private static void Postfix(AbilityData __instance, UnitEntityData __0,
            ref float __result)
        {
            try
            {
                float value;
                if (BrownFurShareTargetingRuntime.TryOverrideApproachDistance(
                    __instance, __0, __result, out value)) __result = value;
            }
            catch { }
        }
    }
}
