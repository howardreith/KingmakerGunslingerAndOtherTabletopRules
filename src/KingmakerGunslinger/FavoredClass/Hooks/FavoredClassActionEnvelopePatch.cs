using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.ElementsSystem;
using KingmakerGunslinger.FavoredClass.Mechanics;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// The native per-action call of ActionList.Run (gameAction.RunAction(),
    /// already inside the native try/catch) is made through
    /// FavoredClassDemoralizeScope.RunAction, which makes the same call and
    /// restores the demoralize scope depth in a finally block, so a frame left
    /// by a demoralize that threw never reaches the next action. Only that one
    /// call site changes; the native exception handling, logging and order
    /// are unchanged. Unless the call is found exactly once the list is left
    /// untouched and the demoralize bonus is withheld.
    /// </summary>
    [HarmonyPatch(typeof(ActionList), "Run")]
    internal static class FavoredClassActionEnvelopePatch
    {
        private static readonly MethodInfo NativeRun = typeof(GameAction).GetMethod("RunAction",
            BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo ScopedRun = typeof(FavoredClassDemoralizeScope).GetMethod("RunAction",
            BindingFlags.Static | BindingFlags.NonPublic);

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> original = instructions.ToList();
            var values = new List<CodeInstruction>(original);
            bool scoped = FavoredClassCallScoping.ReplaceSingle(values, NativeRun, ScopedRun, false);
            FavoredClassDemoralizeScope.EnvelopeInstalled = scoped;
            return scoped ? values : original;
        }
    }
}
