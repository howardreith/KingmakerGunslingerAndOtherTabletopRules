using System;
using Harmony12;
using Kingmaker.Designers.Mechanics.Facts;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// I06/S04 level-gate read point: after the native decision of an
    /// AddFeatureOnClassLevel gate, a gate that belongs to an owned, invested
    /// revelation's own graph decides at the effective oracle level, so the
    /// abilities and forms that revelation grants at later levels follow its
    /// earned steps (charter 8.10). Every other gate in the game returns the
    /// native decision unchanged, and the native component still adds or
    /// removes its own feature.
    /// </summary>
    [HarmonyPatch(typeof(AddFeatureOnClassLevel), "IsFeatureShouldBeApplied")]
    internal static class FavoredClassRevelationGatePatch
    {
        private static void Postfix(AddFeatureOnClassLevel __instance, ref bool __result)
        {
            try
            {
                FavoredClassRevelationScopes.GateResult(__instance, ref __result);
            }
            catch (Exception)
            {
                // Fail safe: the native decision stands.
            }
        }
    }
}
