using System;
using System.Linq;
using System.Threading;
using Harmony12;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// The native activatable ability is the sole source of truth. The hidden
    /// marker buff remains engine-owned compatibility state and is never used
    /// to select ammunition or action economy.
    /// </summary>
    internal static class PaperCartridgeModeRuntime
    {
        private static int _revision;

        internal static int Revision
        { get { return Interlocked.CompareExchange(ref _revision, 0, 0); } }

        internal static bool IsActive(UnitDescriptor unit, BlueprintBuff marker)
        {
            PaperCartridgeModeBlueprintSet mode =
                BlueprintBootstrap.PaperCartridgeMode;
            return mode != null && IsActive(unit, mode.Ability,
                marker ?? mode.Marker);
        }

        internal static bool IsActive(UnitDescriptor unit,
            BlueprintActivatableAbility ability, BlueprintBuff marker)
        {
            try
            {
                ActivatableAbility toggle = Find(unit, ability);
                return toggle != null && toggle.IsOn;
            }
            catch (Exception)
            {
                // A partially hydrated unit cannot safely select a reload
                // profile. Fail closed without changing serialized state.
                return false;
            }
        }

        internal static bool IsPaperToggle(ActivatableAbility ability)
        {
            PaperCartridgeModeBlueprintSet mode =
                BlueprintBootstrap.PaperCartridgeMode;
            return ability != null && mode != null && ReferenceEquals(
                ability.Blueprint, mode.Ability);
        }

        internal static void OnToggleChanged(ActivatableAbility ability)
        {
            if (!IsPaperToggle(ability)) return;
            Interlocked.Increment(ref _revision);
            ReloadAbilityPresentation.InvalidatePaperMode();
        }

        private static ActivatableAbility Find(UnitDescriptor unit,
            BlueprintActivatableAbility ability)
        {
            if (unit == null || ability == null ||
                unit.ActivatableAbilities == null) return null;
            ActivatableAbility[] matches = unit.ActivatableAbilities.Enumerable
                .Where(value => value != null && ReferenceEquals(
                    value.Blueprint, ability)).ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }
    }

    [HarmonyPatch(typeof(ActivatableAbility), "set_IsOn")]
    internal static class PaperCartridgeModeImmediateStatePatch
    {
        private static void Postfix(ActivatableAbility __instance)
        {
            try { PaperCartridgeModeRuntime.OnToggleChanged(__instance); }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context)) context.Logger.Failure(
                    "reload", "paper-cartridge-mode.cache-invalidation-failed",
                    "Paper cartridge mode presentation invalidation failed.",
                    exception);
            }
        }
    }

}
