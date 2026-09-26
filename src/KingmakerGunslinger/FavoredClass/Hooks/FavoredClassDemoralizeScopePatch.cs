using Harmony12;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.FavoredClass.Mechanics;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// Marks the native Demoralize action while it resolves so the I07
    /// demoralize bonus can recognize its Intimidate check. The prefix runs
    /// first (before Call of the Wild's replacing prefix) and only opens a
    /// frame with the action's own context; the postfix restores the depth
    /// the prefix opened at, so an outer demoralize keeps its frame. A
    /// demoralize that throws before its postfix is closed by the per-action
    /// envelope (FavoredClassActionEnvelopePatch). It never changes the
    /// action, its check or its result.
    /// </summary>
    [HarmonyPatch(typeof(Demoralize), "RunAction")]
    internal static class FavoredClassDemoralizeScopePatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(out int __state)
        {
            __state = FavoredClassDemoralizeScope.Enter();
        }

        private static void Postfix(int __state)
        {
            FavoredClassDemoralizeScope.Exit(__state);
        }
    }
}
