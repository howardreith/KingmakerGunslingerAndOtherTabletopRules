using System;
using Harmony12;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;

namespace KingmakerGunslinger.Gunsmithing
{
    [HarmonyPatch(typeof(LevelUpHelper), "AddStartingItems")]
    internal static class GunslingerNativeStartingFirearmPatch
    {
        private static bool Prefix(UnitDescriptor unit,
            ref NativeStartingFirearmObservation __state)
        {
            __state = GunslingerStartingFirearmGrantTransaction
                .BeginNativeGrant(unit);
            return __state == null || !__state.SuppressNative;
        }

        private static void Postfix(NativeStartingFirearmObservation __state)
        {
            GunslingerStartingFirearmGrantTransaction.CompleteNativeGrant(
                __state);
        }
    }

    [HarmonyPatch(typeof(LevelUpController), "Commit")]
    internal static class GunslingerFirstLevelCommitPatch
    {
        private static void Prefix(LevelUpController __instance,
            ref GunslingerLevelTransitionSnapshot __state)
        {
            __state = __instance == null ? null :
                GunslingerStartingFirearmGrantTransaction.CaptureTransition(
                    __instance.Unit);
            GunslingerStartingFirearmGrantTransaction.BeginCommit(__state,
                __instance != null && __instance.State != null &&
                StartingFirearmGrantPolicy.IsCommittedCharacterCreation(
                    __instance.State.IsFirstLevel,
                    __instance.State.Mode == LevelUpState.CharBuildMode.CharGen));
        }

        private static void Postfix(LevelUpController __instance,
            GunslingerLevelTransitionSnapshot __state)
        {
            try
            {
                GunslingerStartingFirearmGrantTransaction.CompleteTransition(
                    __state, __instance == null ? null : __instance.Unit);
            }
            finally
            {
                GunslingerStartingFirearmGrantTransaction.EndCommit(__state);
            }
        }

        private static Exception Finalizer(
            GunslingerLevelTransitionSnapshot __state, Exception __exception)
        {
            GunslingerStartingFirearmGrantTransaction.EndCommit(__state);
            return __exception;
        }
    }

    [HarmonyPatch(typeof(Player), "RespecCompanion",
        new[] { typeof(UnitEntityData), typeof(Action) })]
    internal static class GunslingerFirstLevelRespecSuccessPatch
    {
        private static void Prefix(UnitEntityData unit,
            ref Action successCallback)
        {
            GunslingerLevelTransitionSnapshot transition = unit == null ? null :
                GunslingerStartingFirearmGrantTransaction.CaptureTransition(
                    unit.Descriptor);
            if (transition == null) return;
            Action continuation = successCallback;
            successCallback = () =>
            {
                // Kingmaker invokes this only after the detached respec model
                // has been copied back into the original player unit.
                GunslingerStartingFirearmGrantTransaction.CompleteTransition(
                    transition, unit.Descriptor);
                if (continuation != null) continuation();
            };
        }
    }
}
