using System;
using System.Linq;
using System.Threading;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// The activatable ability is the sole source of truth. The hidden buff is
    /// retained only as the engine's implementation marker and is reconciled
    /// immediately when it diverges from the selected toggle state.
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
            ActivatableAbility toggle = Find(unit, ability);
            bool active = toggle != null && toggle.IsOn;
            Reconcile(unit, ability, marker, active);
            return active;
        }

        internal static void Reconcile(UnitDescriptor unit)
        {
            PaperCartridgeModeBlueprintSet mode =
                BlueprintBootstrap.PaperCartridgeMode;
            if (mode != null) Reconcile(unit, mode.Ability, mode.Marker);
        }

        internal static void Reconcile(UnitDescriptor unit,
            BlueprintActivatableAbility ability, BlueprintBuff marker)
        {
            ActivatableAbility[] toggles = unit == null || ability == null ||
                unit.ActivatableAbilities == null ?
                new ActivatableAbility[0] : unit.ActivatableAbilities.Enumerable
                    .Where(value => value != null && ReferenceEquals(
                        value.Blueprint, ability)).ToArray();
            Reconcile(unit, ability, marker, toggles.Length == 1 &&
                toggles[0].IsOn);
        }

        internal static bool IsPaperToggle(ActivatableAbility ability)
        {
            PaperCartridgeModeBlueprintSet mode =
                BlueprintBootstrap.PaperCartridgeMode;
            return ability != null && mode != null && ReferenceEquals(
                ability.Blueprint, mode.Ability);
        }

        internal static void OnToggleChanged(ActivatableAbility ability,
            bool isOn)
        {
            if (!IsPaperToggle(ability)) return;
            if (!isOn && ability.IsRunning) ability.Stop(true);
            Reconcile(ability.Owner);
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

        private static void Reconcile(UnitDescriptor unit,
            BlueprintActivatableAbility ability, BlueprintBuff marker,
            bool active)
        {
            if (unit == null || marker == null || unit.Buffs == null) return;
            Buff[] markers = unit.Buffs.RawFacts.OfType<Buff>().Where(value =>
                value != null && ReferenceEquals(value.Blueprint, marker))
                .ToArray();
            if (!active)
            {
                foreach (Buff value in markers) unit.Buffs.RemoveFact(value);
                return;
            }
            for (int index = 1; index < markers.Length; index++)
                unit.Buffs.RemoveFact(markers[index]);
        }
    }

    [HarmonyPatch(typeof(ActivatableAbility), "set_IsOn")]
    internal static class PaperCartridgeModeImmediateStatePatch
    {
        private static void Postfix(ActivatableAbility __instance, bool __0)
        {
            try { PaperCartridgeModeRuntime.OnToggleChanged(__instance, __0); }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context)) context.Logger.Failure(
                    "reload", "paper-cartridge-mode.reconcile-failed",
                    "Paper cartridge mode was reconciled fail-closed.",
                    exception);
            }
        }
    }

    [HarmonyPatch(typeof(UnitEntityData), "PostLoad", new Type[0])]
    internal static class PaperCartridgeModePostLoadPatch
    {
        private static void Postfix(UnitEntityData __instance)
        {
            try { PaperCartridgeModeRuntime.Reconcile(__instance == null ?
                null : __instance.Descriptor); }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context)) context.Logger.Failure(
                    "reload", "paper-cartridge-mode.post-load-failed",
                    "Paper cartridge mode reconciliation failed after load.",
                    exception);
            }
        }
    }
}
