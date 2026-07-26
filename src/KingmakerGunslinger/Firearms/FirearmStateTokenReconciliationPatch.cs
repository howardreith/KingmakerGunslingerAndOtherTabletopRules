using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Items;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Surrounds the exact zero-argument ItemEntity.ApplyEnchantments method. Only
    /// weapon items enter state-token inspection. Native Kingmaker removes dynamic
    /// enchantments with null ParentContext during this pass; the postfix restores the
    /// one known firearm-state token captured by the prefix.
    /// </summary>
    [HarmonyPatch]
    internal static class FirearmStateTokenReconciliationPatch
    {
        private static MethodBase _target;

        private static bool Prepare()
        {
            MethodInfo[] candidates = typeof(ItemEntity).GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly)
                .Where(method =>
                    string.Equals(
                        method.Name,
                        "ApplyEnchantments",
                        StringComparison.Ordinal) &&
                    !method.IsStatic &&
                    !method.IsGenericMethodDefinition &&
                    method.GetParameters().Length == 0 &&
                    method.ReturnType == typeof(void))
                .ToArray();
            if (candidates.Length != 1)
            {
                LogTargetFailure(candidates.Length);
                return false;
            }

            _target = candidates[0];
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Info(
                    "firearms",
                    "state-token.reconcile-patch-target",
                    "Resolved Kingmaker.Items.ItemEntity.ApplyEnchantments() for item-token durability repair.");
            }

            return true;
        }

        private static MethodBase TargetMethod()
        {
            return _target;
        }

        private static void Prefix(
            object __instance,
            out FirearmStateTokenReconciliationInvocation __state)
        {
            ItemEntityWeapon weapon = __instance as ItemEntityWeapon;
            __state = weapon == null
                ? FirearmStateTokenReconciliationInvocation.Empty
                : FirearmStateTokenReconciliationRuntime.Before(weapon);
        }

        private static void Postfix(
            object __instance,
            FirearmStateTokenReconciliationInvocation __state)
        {
            FirearmStateTokenReconciliationRuntime.After(__instance, __state);
        }

        private static void LogTargetFailure(int count)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Warning(
                    "firearms",
                    "state-token.reconcile-patch-skipped",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Expected one zero-argument ItemEntity.ApplyEnchantments method; found {0}. Loaded state may not survive native item refresh.",
                        count));
            }
        }
    }
}
