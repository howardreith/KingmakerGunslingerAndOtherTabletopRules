using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// Resolves only the exact Kingmaker 2.1.7b natural-roll contracts. Missing or
    /// ambiguous methods disable the affected optional patch rather than guessing.
    /// </summary>
    internal static class FirearmMisfirePatchTarget
    {
        internal static bool TryResolveRollSetter(out MethodBase target)
        {
            return TryResolve(
                "natural-roll assignment",
                method => FirearmMisfirePatchContract.IsCompatibleRollSetter(
                    method,
                    typeof(RuleAttackRoll),
                    typeof(RulebookEvent.RollEntry)),
                "one private void set_Roll(RulebookEvent.RollEntry) method",
                out target);
        }

        internal static bool TryResolveSuccessRoll(out MethodBase target)
        {
            return TryResolve(
                "natural-roll misfire decision",
                method => FirearmMisfirePatchContract.IsCompatibleSuccessRoll(
                    method,
                    typeof(RuleAttackRoll)),
                "one public bool IsSuccessRoll(Int32) method",
                out target);
        }

        private static bool TryResolve(
            string purpose,
            Func<MethodInfo, bool> predicate,
            string expected,
            out MethodBase target)
        {
            target = null;
            try
            {
                MethodInfo[] candidates = typeof(RuleAttackRoll)
                    .GetMethods(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly)
                    .Where(predicate)
                    .ToArray();

                if (candidates.Length != 1)
                {
                    LogUnavailable(
                        purpose,
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "expected {0}; found {1}",
                            expected,
                            candidates.Length));
                    return false;
                }

                target = candidates[0];
                ModContext context;
                if (ModContext.TryGet(out context))
                {
                    context.Logger.Info(
                        "misfire",
                        "patch.target",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Resolved {0} target {1}.{2}.",
                            purpose,
                            typeof(RuleAttackRoll).FullName,
                            candidates[0].Name));
                }

                return true;
            }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context))
                {
                    context.Logger.Failure(
                        "misfire",
                        "patch.target-failed",
                        "Failed to resolve the optional " + purpose + " target.",
                        exception);
                }

                return false;
            }
        }

        private static void LogUnavailable(string purpose, string reason)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Warning(
                    "misfire",
                    "patch.skipped",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Skipped optional {0} patch: {1}.",
                        purpose,
                        reason));
            }
        }
    }
}
