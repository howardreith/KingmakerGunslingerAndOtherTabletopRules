using System;
using Harmony12;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// I06/S04 rank read point: adds the caster's earned steps to the class
    /// level base value of exactly the Oracle engine rank configs scoped to a
    /// chosen revelation, keyed by the config and the context's own
    /// blueprint (copied blueprints share config instances). It runs after
    /// Call of the Wild's postfix, which assigns that base value, and before
    /// the rank's own progression and limits apply, so the value is the
    /// native value at the effective oracle level. Unscoped ranks return at
    /// once and are never changed.
    /// </summary>
    [HarmonyPatch(typeof(ContextRankConfig), "GetBaseValue")]
    internal static class FavoredClassRevelationRankPatch
    {
        [HarmonyAfter("CallOfTheWild")]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(ContextRankConfig __instance, MechanicsContext context, ref int __result)
        {
            int bonus;
            try
            {
                bonus = FavoredClassRevelationScopes.RankBonus(__instance, context);
            }
            catch (Exception)
            {
                // Fail safe: an unexpected state adds nothing and never breaks
                // the native rank evaluation.
                return;
            }
            if (bonus > 0)
                __result += bonus;
        }
    }
}
