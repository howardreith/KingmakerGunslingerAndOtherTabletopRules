using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// Scopes each pick the native controller replays (in priority order) to
    /// that controller, so an owned-target prerequisite can count a target
    /// chosen by a later-priority pick of the same level-up. Only the replay's
    /// two calls, action.Check and action.Apply, are routed through
    /// FavoredClassPendingPicks, which makes the same call inside a scope
    /// closed in a finally block: a pick that throws leaves nothing marked, a
    /// nested replay restores the outer one and a retry opens a fresh scope.
    /// The replay is otherwise unchanged; unless both calls are found exactly
    /// once it is left untouched and no pending pick is ever counted.
    /// </summary>
    [HarmonyPatch(typeof(LevelUpController), "ApplyLevelup")]
    internal static class FavoredClassLevelUpReplayPatch
    {
        private static readonly MethodInfo NativeCheck = typeof(ILevelUpAction).GetMethod("Check");
        private static readonly MethodInfo NativeApply = typeof(ILevelUpAction).GetMethod("Apply");
        private static readonly MethodInfo ScopedCheck = typeof(FavoredClassPendingPicks).GetMethod("Check",
            BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ScopedApply = typeof(FavoredClassPendingPicks).GetMethod("Apply",
            BindingFlags.Static | BindingFlags.NonPublic);

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> original = instructions.ToList();
            var values = new List<CodeInstruction>(original);
            bool scoped = FavoredClassCallScoping.ReplaceSingle(values, NativeCheck, ScopedCheck, true) &&
                FavoredClassCallScoping.ReplaceSingle(values, NativeApply, ScopedApply, true);
            FavoredClassPendingPicks.Installed = scoped;
            return scoped ? values : original;
        }
    }
}
