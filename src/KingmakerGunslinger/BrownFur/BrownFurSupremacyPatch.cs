using Harmony12;
using Kingmaker.UnitLogic.Abilities;
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
}
