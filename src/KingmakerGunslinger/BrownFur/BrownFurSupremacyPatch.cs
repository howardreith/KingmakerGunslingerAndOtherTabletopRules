using Harmony12;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace KingmakerGunslinger.BrownFur
{
    [HarmonyPatch(typeof(AbilityData), "CreateExecutionContext", new[] {
        typeof(TargetWrapper) })]
    [HarmonyAfter("CallOfTheWild")]
    internal static class BrownFurSupremacyPatch
    {
        private static void Postfix(AbilityData __instance,
            AbilityExecutionContext __result)
        {
            try
            { BrownFurSupremacyRuntime.TryApply(__instance, __result); }
            catch
            {
                // Optional Brown-Fur context adjustment must never prevent
                // native or CotW execution-context construction.
            }
        }
    }

    [HarmonyPatch(typeof(ContextDurationValue), "Calculate", new[] {
        typeof(MechanicsContext) })]
    [HarmonyAfter("CallOfTheWild")]
    internal static class BrownFurSupremacyNonstandardDurationPatch
    {
        private static void Postfix(ContextDurationValue __instance,
            MechanicsContext context, ref Rounds __result)
        {
            try
            {
                BrownFurSupremacyRuntime.TryDoubleNonstandardDuration(
                    __instance, context, ref __result);
            }
            catch
            {
                // Optional Brown-Fur duration adaptation must never prevent
                // native or CotW duration calculation.
            }
        }
    }
}
