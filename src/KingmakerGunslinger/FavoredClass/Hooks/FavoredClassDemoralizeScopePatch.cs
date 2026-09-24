using Harmony12;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.FavoredClass.Mechanics;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// Marks the native Demoralize action while it resolves so the I07
    /// demoralize bonus can recognize its Intimidate check. The prefix runs
    /// first (before Call of the Wild's replacing prefix) and only records
    /// the action's own context; the postfix clears it. It never changes the
    /// action, its check or its result.
    /// </summary>
    [HarmonyPatch(typeof(Demoralize), "RunAction")]
    internal static class FavoredClassDemoralizeScopePatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix()
        {
            FavoredClassDemoralizeScope.Enter();
        }

        private static void Postfix()
        {
            FavoredClassDemoralizeScope.Exit();
        }
    }
}
