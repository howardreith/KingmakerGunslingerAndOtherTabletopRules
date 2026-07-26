using System.Reflection;
using Harmony12;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Rules;
using KingmakerGunslinger.Misfires;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Shared rule-event patch surface. Weapon-attack observations remain read-only.
    /// Attack-roll callbacks maintain the short-lived firearm context, and the AC
    /// postfix applies the Sprint 9 range-limited touch-AC selection before the
    /// optional trace captures the final TargetAC value.
    /// </summary>
    [HarmonyPatch]
    internal static class RuleAttackWithWeaponTracePatch
    {
        private const string TargetTypeName =
            "Kingmaker.RuleSystem.Rules.RuleAttackWithWeapon";
        private static MethodBase _target;

        private static bool Prepare()
        {
            return RuleEventPatchTarget.TryResolve(
                TargetTypeName,
                "weapon-attack tracing",
                out _target);
        }

        private static MethodBase TargetMethod()
        {
            return _target;
        }

        private static void Prefix(object __instance)
        {
            CombatTraceRuntime.Before(CombatTraceStage.WeaponAttack, __instance);
        }

        private static void Postfix(object __instance)
        {
            CombatTraceRuntime.After(CombatTraceStage.WeaponAttack, __instance);
        }
    }

    [HarmonyPatch]
    internal static class RuleAttackRollFirearmPatch
    {
        private const string TargetTypeName =
            "Kingmaker.RuleSystem.Rules.RuleAttackRoll";
        private static MethodBase _target;

        private static bool Prepare()
        {
            return RuleEventPatchTarget.TryResolve(
                TargetTypeName,
                "firearm attack context and tracing",
                out _target);
        }

        private static MethodBase TargetMethod()
        {
            return _target;
        }

        private static void Prefix(object __instance)
        {
            FirearmDischargeRuntime.BeforeAttackRoll(__instance);
            FirearmArmorClassRuntime.BeforeAttackRoll(__instance);
            CombatTraceRuntime.Before(CombatTraceStage.AttackRoll, __instance);
        }

        private static void Postfix(object __instance)
        {
            try
            {
                CombatTraceRuntime.After(CombatTraceStage.AttackRoll, __instance);
            }
            finally
            {
                try
                {
                    FirearmArmorClassRuntime.AfterAttackRoll(__instance);
                }
                finally
                {
                    FirearmMisfireRuntime.FinishAttack(
                        __instance as Kingmaker.RuleSystem.Rules.RuleAttackRoll);
                }
            }
        }
    }

    [HarmonyPatch]
    internal static class RuleCalculateAcFirearmPatch
    {
        private const string TargetTypeName =
            "Kingmaker.RuleSystem.Rules.RuleCalculateAC";
        private static MethodBase _target;

        private static bool Prepare()
        {
            return RuleEventPatchTarget.TryResolve(
                TargetTypeName,
                "range-limited firearm touch AC and tracing",
                out _target);
        }

        private static MethodBase TargetMethod()
        {
            return _target;
        }

        private static void Prefix(object __instance)
        {
            CombatTraceRuntime.Before(CombatTraceStage.ArmorClass, __instance);
        }

        private static void Postfix(object __instance)
        {
            FirearmArmorClassRuntime.AfterCalculateArmorClass(__instance);
            CombatTraceRuntime.After(CombatTraceStage.ArmorClass, __instance);
        }
    }
}
