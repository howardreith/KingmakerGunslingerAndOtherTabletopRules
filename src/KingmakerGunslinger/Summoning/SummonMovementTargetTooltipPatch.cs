using System;
using Harmony12;
using Kingmaker.Blueprints.Root;
using Kingmaker.Localization;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// VIS-03: presentation of the owned Trample and Overrun actions only.
    /// Desktop and controller windows both build their rows through
    /// <c>BlueprintAbility.GetTarget</c> and
    /// <c>DescriptionTemplatesAbility.AbilityHeader</c>. Range, legality,
    /// path rules, type, action cost and every mechanic are untouched, and
    /// every other ability keeps its native text.
    /// </summary>
    internal static class SummonMovementTargetPresentation
    {
        // Created once with the owned blueprints; null keeps the native text.
        internal static LocalizedString Range;

        internal static bool Owns(BlueprintAbility ability)
        {
            return ability != null && SummonActionTooltipPolicy.PresentsMovementTarget(
                ability.AssetGuid, ability.Range == AbilityRange.DoubleMove,
                ability.CanTargetPoint);
        }
    }

    [HarmonyPatch(typeof(BlueprintAbility), "GetTarget")]
    internal static class SummonMovementTargetTextPatch
    {
        private static void Postfix(BlueprintAbility __instance, ref string __result)
        {
            try
            {
                LocalizedString range = SummonMovementTargetPresentation.Range;
                if (range == null || !SummonMovementTargetPresentation.Owns(__instance))
                    return;
                string targetPoint = LocalizedTexts.Instance.AbilityTargets.TargetPoint;
                string text = range;
                if (!string.IsNullOrEmpty(targetPoint) && !string.IsNullOrEmpty(text))
                    __result = SummonActionTooltipPolicy.MovementTarget(targetPoint, text);
            }
            catch (Exception)
            {
                // Keep the native text.
            }
        }
    }

    /// <summary>
    /// The fact header prints the school slot unless it is given a blueprint
    /// whose school is None; the native AbilityData header always blanks it.
    /// For an owned movement action the header is given its own blueprint,
    /// so it leaves the school slot empty instead of printing "None".
    /// </summary>
    [HarmonyPatch(typeof(DescriptionTemplatesAbility), "AbilityHeader")]
    internal static class SummonMovementTargetHeaderPatch
    {
        private static void Prefix(Ability ability, ref BlueprintAbility blueprintAbility)
        {
            try
            {
                if (blueprintAbility == null && ability != null &&
                    SummonMovementTargetPresentation.Range != null &&
                    SummonMovementTargetPresentation.Owns(ability.Blueprint))
                    blueprintAbility = ability.Blueprint;
            }
            catch (Exception)
            {
                // Keep the native header.
            }
        }
    }
}
