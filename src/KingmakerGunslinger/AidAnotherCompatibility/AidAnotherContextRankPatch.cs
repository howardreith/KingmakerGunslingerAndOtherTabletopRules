using Harmony12;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace KingmakerGunslinger.AidAnotherCompatibility
{
    /// <summary>
    /// Exact-reference-gated correction for CotW's one shared Aid Another rank
    /// configuration. Unrelated ContextRankConfig instances are never changed.
    /// </summary>
    [HarmonyPatch(typeof(ContextRankConfig), "GetValue",
        new[] { typeof(MechanicsContext) })]
    [HarmonyAfter(CotwAidAnotherResolver.ModId)]
    internal static class AidAnotherContextRankPatch
    {
        private static void Postfix(ContextRankConfig __instance,
            MechanicsContext __0, ref int __result)
        {
            AidAnotherGrantRuntime.TryOverrideCanonical(__instance, __0,
                ref __result);
        }
    }
}
