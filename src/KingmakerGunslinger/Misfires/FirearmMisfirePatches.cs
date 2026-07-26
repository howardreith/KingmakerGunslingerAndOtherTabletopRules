using System.Reflection;
using Harmony12;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// Development-only forced-roll hook. It patches the exact private Roll setter,
    /// not the global dice subsystem, so unrelated d20 rolls remain untouched.
    /// </summary>
    [HarmonyPatch]
    internal static class RuleAttackRollNaturalRollSetterPatch
    {
        private static MethodBase _target;

        private static bool Prepare()
        {
            return FirearmMisfirePatchTarget.TryResolveRollSetter(out _target);
        }

        private static MethodBase TargetMethod()
        {
            return _target;
        }

        private static void Prefix(
            RuleAttackRoll __instance,
            ref RulebookEvent.RollEntry value)
        {
            FirearmMisfireRuntime.BeforeSetRoll(__instance, ref value);
        }
    }

    /// <summary>
    /// Final natural-roll decision hook. Kingmaker first computes its ordinary
    /// success result; the postfix can turn a configured misfire into a miss and
    /// apply the bounded condition transition to the exact discharged firearm.
    /// </summary>
    [HarmonyPatch]
    internal static class RuleAttackRollMisfireDecisionPatch
    {
        private static MethodBase _target;

        private static bool Prepare()
        {
            return FirearmMisfirePatchTarget.TryResolveSuccessRoll(out _target);
        }

        private static MethodBase TargetMethod()
        {
            return _target;
        }

        private static void Postfix(
            RuleAttackRoll __instance,
            int d20,
            ref bool __result)
        {
            FirearmMisfireRuntime.AfterIsSuccessRoll(
                __instance,
                d20,
                ref __result);
        }
    }
}
