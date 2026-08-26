using System;
using System.Collections.Generic;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    [HarmonyPatch(typeof(ActionBarGroupSlot), "OnToggleGroupClick")]
    internal static class ExpandedSummoningVariantMenuSourceSlotPatch
    {
        private static void Prefix(ActionBarGroupSlot __instance,
            ActionBarSpellsGroup ___SubGroup)
        {
            if (__instance != null && ___SubGroup != null)
                ExpandedSummoningVariantMenuRuntime.CaptureSourceSlot(
                    ___SubGroup, __instance);
        }
    }

    [HarmonyPatch(typeof(ActionBarSpellsGroup), "Toggle", new[] {
        typeof(UnitEntityData), typeof(IEnumerable<AbilityData>),
        typeof(AbilityData) })]
    internal static class ExpandedSummoningVariantMenuTogglePatch
    {
        private static void Prefix(ActionBarSpellsGroup __instance,
            bool ___m_IsActive, out ActionBarGroupSlot __state)
        {
            __state = ExpandedSummoningVariantMenuRuntime.ConsumeSourceSlot(
                __instance);
            if (__instance != null && !___m_IsActive)
                ExpandedSummoningVariantMenuRuntime.PrepareForNativeFill(
                    __instance);
        }

        private static void Postfix(ActionBarSpellsGroup __instance,
            AbilityData sourceSpell, bool ___m_IsActive,
            List<ActionBarSpontaneousConvertedSlot> ___m_Slots,
            ActionBarGroupSlot __state)
        {
            if (__instance == null || !___m_IsActive || sourceSpell == null ||
                !ExpandedSummoningPublisher.IsPublishedExpandedParent(
                    sourceSpell.Blueprint))
            {
                return;
            }

            try
            {
                ExpandedSummoningVariantMenuRuntime.Apply(__instance,
                    ___m_Slots, sourceSpell, __state);
            }
            catch (Exception exception)
            {
                ExpandedSummoningVariantMenuRuntime.RestoreNative(__instance);
                ExpandedSummoningVariantMenuRuntime.RecordFailure(exception);
            }
        }
    }

    [HarmonyPatch(typeof(ActionBarSpellsGroup), "Hide", new[] { typeof(bool) })]
    internal static class ExpandedSummoningVariantMenuHidePatch
    {
        private static void Postfix(ActionBarSpellsGroup __instance)
        {
            if (__instance != null)
                ExpandedSummoningVariantMenuRuntime.RestoreNative(__instance);
        }
    }
}
