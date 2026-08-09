using Harmony12;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Acadamae
{
    internal static class AcadamaeCastingRuntime
    {
        private static readonly AcadamaeInvocationTracker<UnitUseAbility, AbilityData> Invocations =
            new AcadamaeInvocationTracker<UnitUseAbility, AbilityData>();
        [System.ThreadStatic] private static bool _inspectPreAcadamae;

        internal static bool IsEligible(AbilityData ability, bool longerThanStandard)
        {
            if (ability == null || ability.Caster == null ||
                ability.Caster.Progression == null || ability.Blueprint == null ||
                ability.SpellLevel < 0 || ability.SpellLevel > 10)
                return false;
            var spellbook = ability.Spellbook;
            bool preparedInvocation = spellbook != null &&
                !spellbook.Blueprint.Spontaneous && spellbook.CanSpend(ability, false);
            AcadamaeCastDecision decision = AcadamaeCastingPolicy.Decide(
                new AcadamaeCastRequest {
                    HasFeat = BlueprintBootstrap.AcadamaeGraduate != null &&
                        ability.Caster.Progression.Features.GetRank(
                            BlueprintBootstrap.AcadamaeGraduate) > 0,
                    IsRealSpell = ability.Blueprint.IsSpell,
                    HasSpellbook = spellbook != null,
                    IsPreparedInvocation = preparedInvocation,
                    IsArcane = spellbook != null && spellbook.Blueprint.IsArcane,
                    IsConjuration = ability.Blueprint.School == SpellSchool.Conjuration,
                    IsSummoning = (ability.Blueprint.SpellDescriptor &
                        SpellDescriptor.Summoning) != 0,
                    EffectiveCastingTime = longerThanStandard ?
                        AcadamaeCastingTime.FullRound : AcadamaeCastingTime.Standard,
                    EffectiveRounds = 1,
                    SpellLevel = ability.SpellLevel
                });
            return decision.Eligible;
        }

        internal static bool InspectPreAcadamae(AbilityData ability)
        {
            try { _inspectPreAcadamae = true; return ability.RequireFullRoundAction; }
            finally { _inspectPreAcadamae = false; }
        }

        internal static bool IsInspecting { get { return _inspectPreAcadamae; } }
        internal static void Arm(UnitUseAbility command, AbilityData ability)
        { if (IsEligible(ability, InspectPreAcadamae(ability))) Invocations.Arm(command, ability); }
        internal static void Begin(UnitUseAbility command) { Invocations.Begin(command); }
        internal static void End(UnitUseAbility command) { Invocations.EndAction(command); }
        internal static void Cancel(UnitUseAbility command) { Invocations.Cancel(command); }
        internal static bool Complete(RuleCastSpell rule)
        { return rule != null && rule.Success && Invocations.ConsumeSuccessful(rule.Spell); }
    }

    [HarmonyPatch(typeof(AbilityData), "get_RequireFullRoundAction")]
    [HarmonyAfter("CallOfTheWild")]
    internal static class AcadamaeRequireFullRoundActionPatch
    {
        private static void Postfix(AbilityData __instance, ref bool __result)
        {
            if (!AcadamaeCastingRuntime.IsInspecting && __result &&
                AcadamaeCastingRuntime.IsEligible(__instance, true)) __result = false;
        }
    }

    [HarmonyPatch(typeof(UnitUseAbility), MethodType.Constructor,
        typeof(UnitCommand.CommandType), typeof(AbilityData), typeof(TargetWrapper))]
    internal static class AcadamaeCommandConstructorPatch
    {
        private static void Postfix(UnitUseAbility __instance, AbilityData __1)
        { AcadamaeCastingRuntime.Arm(__instance, __1); }
    }

    [HarmonyPatch(typeof(UnitUseAbility), "OnAction")]
    internal static class AcadamaeCommandActionPatch
    {
        private static void Prefix(UnitUseAbility __instance)
        { AcadamaeCastingRuntime.Begin(__instance); }
        private static void Postfix(UnitUseAbility __instance)
        { AcadamaeCastingRuntime.End(__instance); }
    }

    [HarmonyPatch(typeof(RuleCastSpell), "OnTrigger")]
    [HarmonyAfter("CallOfTheWild")]
    internal static class AcadamaeSuccessfulCastPatch
    {
        private static void Postfix(RuleCastSpell __instance)
        { AcadamaeCastingRuntime.Complete(__instance); }
    }

    [HarmonyPatch(typeof(UnitUseAbility), "OnEnded")]
    internal static class AcadamaeCommandEndedPatch
    {
        private static void Postfix(UnitUseAbility __instance)
        { AcadamaeCastingRuntime.Cancel(__instance); }
    }
}
