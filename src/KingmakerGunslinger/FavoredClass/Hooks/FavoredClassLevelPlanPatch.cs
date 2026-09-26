using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// E15: a stored level plan (auto-level, a pregen, an imported companion)
    /// is applied by the controller itself, outside the scoped replay, and in
    /// priority order it lists the host's reward before the bloodline, the
    /// revelation or the power chosen at the same level. Only the plan's one
    /// AddAction(action, ignoreOrder) call is routed through
    /// FavoredClassPendingPicks.AddPlanned, which makes the same call inside
    /// a scope of that controller and plan closed in a finally block, so the
    /// plan's own later picks count as chosen, exactly as a replay's do.
    /// Unless the call is found exactly once the plan is left untouched and
    /// its picks are checked natively.
    /// </summary>
    [HarmonyPatch(typeof(LevelUpController), "ApplyLevelUpPlan")]
    internal static class FavoredClassLevelPlanPatch
    {
        private static readonly MethodInfo NativeAddAction = typeof(LevelUpController).GetMethod("AddAction",
            BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(ILevelUpAction), typeof(bool) },
            null);
        private static readonly MethodInfo ScopedAddAction = typeof(FavoredClassPendingPicks).GetMethod(
            "AddPlanned", BindingFlags.Static | BindingFlags.NonPublic);

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> original = instructions.ToList();
            var values = new List<CodeInstruction>(original);
            bool scoped = FavoredClassPendingPicks.BindNativeAddAction(NativeAddAction) &&
                FavoredClassCallScoping.ReplaceSingle(values, NativeAddAction, ScopedAddAction, false);
            FavoredClassPendingPicks.PlanInstalled = scoped;
            return scoped ? values : original;
        }
    }
}
