using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.RuleSystem;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Resolves the exact one-argument rule-event OnTrigger callback from the
    /// installed Assembly-CSharp. Missing or ambiguous contracts disable the affected
    /// patch instead of binding a plausible but unverified overload.
    /// </summary>
    internal static class RuleEventPatchTarget
    {
        internal static bool TryResolve(
            string typeName,
            string purpose,
            out MethodBase target)
        {
            target = null;
            purpose = string.IsNullOrWhiteSpace(purpose)
                ? "rule-event integration"
                : purpose.Trim();

            try
            {
                Type type = typeof(LibraryScriptableObject).Assembly.GetType(typeName, false);
                if (type == null)
                {
                    LogUnavailable(typeName, purpose, "type not found");
                    return false;
                }

                MethodInfo[] candidates = type.GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly)
                    .Where(method => RuleEventPatchContract.IsCompatibleOnTrigger(
                        method,
                        typeof(RulebookEventContext)))
                    .ToArray();

                if (candidates.Length != 1)
                {
                    LogUnavailable(
                        typeName,
                        purpose,
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "expected one void instance OnTrigger(RulebookEventContext) method; found {0}",
                            candidates.Length));
                    return false;
                }

                target = candidates[0];
                ModContext context;
                if (ModContext.TryGet(out context))
                {
                    context.Logger.Info(
                        "combat",
                        "patch.target",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Resolved {0} target {1}.{2}(RulebookEventContext).",
                            purpose,
                            type.FullName,
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
                        "combat",
                        "patch.target-failed",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Failed to resolve optional {0} target {1}.",
                            purpose,
                            typeName),
                        exception);
                }

                return false;
            }
        }

        private static void LogUnavailable(
            string typeName,
            string purpose,
            string reason)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Warning(
                    "combat",
                    "patch.skipped",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Skipped optional {0} patch for {1}: {2}.",
                        purpose,
                        typeName,
                        reason));
            }
        }
    }
}
