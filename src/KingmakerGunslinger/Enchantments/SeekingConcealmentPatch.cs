using System;
using System.Reflection;
using Harmony12;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.Enchantments
{
    internal static class SeekingConcealmentRuntime
    {
        [ThreadStatic] private static ItemEntityWeapon _forcedWeapon;
        [ThreadStatic] private static int _forcedRoll;

        internal static void QueueForcedRoll(ItemEntityWeapon weapon, int roll)
        {
            if (weapon == null) throw new ArgumentNullException("weapon");
            if (roll < 1 || roll > 100) throw new ArgumentOutOfRangeException("roll");
            _forcedWeapon = weapon;
            _forcedRoll = roll;
        }

        internal static void CancelForcedRoll()
        {
            _forcedWeapon = null;
            _forcedRoll = 0;
        }

        internal static void AfterCheck(RuleConcealmentCheck check)
        {
            ItemEntityWeapon expected = _forcedWeapon;
            if (check == null || expected == null) return;
            RulebookEventContext context = Rulebook.CurrentContext;
            RuleAttackRoll attack = context == null
                ? null : context.LastEvent<RuleAttackRoll>();
            if (attack == null || !ReferenceEquals(attack.Weapon, expected) ||
                attack.Initiator == null || attack.Target == null)
                return;
            try
            {
                PropertyInfo roll = typeof(RuleConcealmentCheck).GetProperty(
                    "Roll", BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
                if (roll == null) return;
                roll.SetValue(check,
                    (RulebookEvent.RollEntry)_forcedRoll, null);
                CancelForcedRoll();
            }
            catch
            {
                // A diagnostic force must never alter native behavior on failure.
            }
        }
    }

    [HarmonyPatch(typeof(RuleConcealmentCheck), "OnTrigger")]
    internal static class SeekingConcealmentForcedRollPatch
    {
        private static void Postfix(RuleConcealmentCheck __instance)
        {
            SeekingConcealmentRuntime.AfterCheck(__instance);
        }
    }

    /// <summary>
    /// Changes only the final success read of the exact concealment check stored by
    /// the current parent attack. Native concealment classification and rolls remain.
    /// </summary>
    [HarmonyPatch(typeof(RuleConcealmentCheck), "get_Success")]
    internal static class SeekingConcealmentSuccessPatch
    {
        private static void Postfix(RuleConcealmentCheck __instance, ref bool __result)
        {
            try
            {
                if (__instance == null) return;
                RulebookEventContext context = Rulebook.CurrentContext;
                RuleAttackRoll attack = context == null
                    ? null : context.LastEvent<RuleAttackRoll>();
                if (!SeekingConcealmentPolicy.ShouldBypass(
                    __result,
                    attack != null,
                    attack != null && ReferenceEquals(
                        attack.ConcealmentCheck, __instance),
                    attack != null && attack.Initiator != null &&
                        attack.Target != null,
                    attack != null &&
                        SeekingExactItemResolver.IsAuthorized(attack.Weapon)))
                {
                    return;
                }

                __result = true;
            }
            catch (Exception)
            {
                // Fail closed to the native concealment result.
            }
        }
    }
}
