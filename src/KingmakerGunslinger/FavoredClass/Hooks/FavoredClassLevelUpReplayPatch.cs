using Harmony12;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// Marks the level-up whose picks the native controller is replaying (in
    /// priority order), so an owned-target prerequisite can count a target
    /// chosen by a later-priority pick of the same level-up. The replay and
    /// every native check are otherwise unchanged; the mark is keyed by the
    /// replayed state, so a mark left by an interrupted replay matches nothing.
    /// </summary>
    [HarmonyPatch(typeof(LevelUpController), "ApplyLevelup")]
    internal static class FavoredClassLevelUpReplayPatch
    {
        private static void Prefix(LevelUpController __instance)
        {
            FavoredClassPendingPicks.Begin(__instance);
        }

        private static void Postfix()
        {
            FavoredClassPendingPicks.End();
        }
    }
}
