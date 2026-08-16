using Harmony12;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Parts;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurActivatableGroupRuntime
    {
        // Blueprint enum values are serialized as Int32. This project-owned
        // value is deliberately outside Kingmaker's and CotW's known ranges;
        // the exact GetGroupSize patch below supplies its one-slot capacity
        // without indexing the engine's fixed native-group array.
        internal const int PowerfulChangeGroupValue = 81082;
        internal static readonly ActivatableAbilityGroup PowerfulChangeGroup =
            (ActivatableAbilityGroup)PowerfulChangeGroupValue;
    }

    [HarmonyPatch(typeof(UnitPartActivatableAbility), "GetGroupSize", new[] {
        typeof(ActivatableAbilityGroup) })]
    internal static class BrownFurActivatableGroupSizePatch
    {
        private static bool Prefix(ActivatableAbilityGroup __0,
            ref int __result)
        {
            if ((int)__0 != BrownFurActivatableGroupRuntime
                    .PowerfulChangeGroupValue)
                return true;
            __result = 1;
            return false;
        }
    }

    /// <summary>
    /// Kingmaker normally delays removal of an activatable's applied buff when
    /// IsOn becomes false. Brown-Fur intent markers must disappear at the same
    /// instant as the native selected overlay. Force-removal is restricted to
    /// the seven Brown-Fur toggles and does not alter activation or spending.
    /// </summary>
    [HarmonyPatch(typeof(ActivatableAbility), "set_IsOn")]
    internal static class BrownFurActivatableImmediateOffPatch
    {
        private static void Postfix(ActivatableAbility __instance, bool __0)
        {
            try
            {
                if (!__0 && __instance != null && __instance.IsRunning &&
                    BrownFurPlayerIntentRuntime.IsBrownFurToggle(__instance))
                    __instance.Stop(true);
            }
            catch (System.Exception exception)
            {
                BrownFurCastExecutionRuntime.RecordPatchFailure(
                    "activatable-immediate-off", exception);
            }
        }
    }
}
