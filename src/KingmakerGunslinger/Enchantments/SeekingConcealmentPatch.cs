using System;
using Harmony12;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.Enchantments
{
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
