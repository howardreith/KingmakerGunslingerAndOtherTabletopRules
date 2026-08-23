using System.Reflection;
using Harmony12;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Rules;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Grit;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.BodyguardFeats;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules;
using System;

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
            BodyguardRuntime.BeforeAttackRoll(__instance as RuleAttackRoll);
            TargetingTorsoRuntime.ConfigureAttackRoll(
                __instance as RuleAttackRoll);
            DeadShotRuntime.ConfigureDelivery(
                __instance as RuleAttackRoll);
            FirearmDischargeRuntime.BeforeAttackRoll(__instance);
            DeadeyeRuntime.BeforeAttackRoll(
                __instance as RuleAttackRoll);
            FirearmArmorClassRuntime.BeforeAttackRoll(__instance);
            CombatTraceRuntime.Before(CombatTraceStage.AttackRoll, __instance);
        }

        private static void Postfix(object __instance)
        {
            try
            {
                BodyguardRuntime.AfterAttackRoll(__instance as RuleAttackRoll);
                BleedingWoundRuntime.AfterAttack(
                    __instance as RuleAttackRoll);
                FirearmGritRecoveryRuntime.AfterAttackRoll(
                    __instance as RuleAttackRoll);
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
                        __instance as RuleAttackRoll);
                }
            }
        }

    }

    [HarmonyPatch]
    internal static class RuleDealDamageGritRecoveryPatch
    {
        private const string TargetTypeName =
            "Kingmaker.RuleSystem.Rules.Damage.RuleDealDamage";
        private static MethodBase _target;

        private static bool Prepare()
        {
            return RuleEventPatchTarget.TryResolve(
                TargetTypeName,
                "firearm killing-blow grit recovery",
                out _target);
        }

        private static MethodBase TargetMethod()
        {
            return _target;
        }

        private static void Prefix(object __instance)
        {
            RuleDealDamage damage = __instance as RuleDealDamage;
            if (damage != null)
                BodyguardRuntime.ObserveNativeDelivery(damage.AttackRoll,
                    damage.Target, "rule-deal-damage-prefix");
            Scatter.ScatterVolleyRuntime.SuppressPrecisionDamage(damage);
        }

        private static void Postfix(object __instance)
        {
            RuleDealDamage damage = __instance as RuleDealDamage;
            if (damage != null)
                BodyguardRuntime.ObserveNativeDelivery(damage.AttackRoll,
                    damage.Target, "rule-deal-damage-postfix");
        }
    }

    [HarmonyPatch(typeof(Kingmaker.RuleSystem.Rules.RuleAttackWithWeaponResolve),
        "OnTrigger")]
    internal static class RuleAttackWithWeaponResolveGritRecoveryPatch
    {
        private static void Prefix(
            Kingmaker.RuleSystem.Rules.RuleAttackWithWeaponResolve __instance)
        {
            RuleAttackWithWeapon attack = __instance == null ? null :
                __instance.AttackWithWeapon;
            BodyguardRuntime.ObserveNativeDelivery(attack == null ? null :
                attack.AttackRoll, __instance == null ? null :
                __instance.Target, "weapon-resolve-prefix");
        }

        private static void Postfix(
            Kingmaker.RuleSystem.Rules.RuleAttackWithWeaponResolve __instance)
        {
            RuleAttackWithWeapon attack = __instance == null ? null :
                __instance.AttackWithWeapon;
            BodyguardRuntime.ObserveNativeDelivery(attack == null ? null :
                attack.AttackRoll, __instance == null ? null :
                __instance.Target, "weapon-resolve-postfix");
            FirearmGritRecoveryRuntime.AfterWeaponResolve(__instance);
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
            BodyguardRuntime.AfterCalculateArmorClass(
                __instance as RuleCalculateAC);
            CombatTraceRuntime.After(CombatTraceStage.ArmorClass, __instance);
        }
    }
}
