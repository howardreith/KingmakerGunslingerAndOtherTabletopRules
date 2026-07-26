using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Captures known item-owned state tokens immediately before Kingmaker's native
    /// ApplyEnchantments pass and restores only the exact token that native code removed.
    /// </summary>
    internal static class FirearmStateTokenReconciliationRuntime
    {
        private static readonly FirearmStateTokenReconciliationService Service =
            new FirearmStateTokenReconciliationService();

        internal static FirearmStateTokenReconciliationInvocation Before(
            object itemInstance)
        {
            try
            {
                if (!FirearmRuntimeState.IsConfigured || itemInstance == null)
                {
                    FirearmStateTokenReconciliationDiagnostics.RecordCall(false);
                    return FirearmStateTokenReconciliationInvocation.Empty;
                }

                IReadOnlyList<string> before =
                    FirearmRuntimeState.ReadStateTokenIds(itemInstance);
                FirearmStateTokenReconciliationDiagnostics.RecordCall(
                    before.Count != 0);
                return new FirearmStateTokenReconciliationInvocation(before);
            }
            catch (Exception exception)
            {
                FirearmStateTokenReconciliationDiagnostics.RecordFault(
                    exception,
                    "before");
                LogFault(
                    "state-token.reconcile-before-failed",
                    "Failed to inspect state tokens before native item-enchantment reconciliation. The native method was allowed to continue.",
                    exception);
                return FirearmStateTokenReconciliationInvocation.Empty;
            }
        }

        internal static void After(
            object itemInstance,
            FirearmStateTokenReconciliationInvocation invocation)
        {
            if (invocation == null || !invocation.HasObservation ||
                itemInstance == null || !FirearmRuntimeState.IsConfigured)
            {
                return;
            }

            try
            {
                IReadOnlyList<string> after =
                    FirearmRuntimeState.ReadStateTokenIds(itemInstance);
                FirearmStateTokenReconciliationDecision decision = Service.Evaluate(
                    invocation.BeforeTokenIds,
                    after);

                if (decision.Action == FirearmStateTokenReconciliationAction.RestoreMissing)
                {
                    FirearmRuntimeState.RestoreMissingStateToken(
                        itemInstance,
                        decision.TokenToRestore);
                    IReadOnlyList<string> verified =
                        FirearmRuntimeState.ReadStateTokenIds(itemInstance);
                    if (verified.Count != 1 ||
                        !string.Equals(
                            verified[0],
                            decision.TokenToRestore,
                            StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "The state token removed by native reconciliation did not verify after restoration.");
                    }
                }

                FirearmStateTokenReconciliationDiagnostics.RecordDecision(
                    decision,
                    DescribeItem(itemInstance));

                if (decision.Action == FirearmStateTokenReconciliationAction.Conflict)
                {
                    LogWarning(
                        "state-token.reconcile-conflict",
                        decision + "; item=" + DescribeItem(itemInstance));
                }
                else if (decision.Action == FirearmStateTokenReconciliationAction.RestoreMissing)
                {
                    LogInfo(
                        "state-token.restored-after-native-removal",
                        decision + "; item=" + DescribeItem(itemInstance));
                }
            }
            catch (Exception exception)
            {
                FirearmStateTokenReconciliationDiagnostics.RecordFault(
                    exception,
                    "after");
                LogFault(
                    "state-token.reconcile-after-failed",
                    "Failed to verify or restore a firearm-state token after native item-enchantment reconciliation.",
                    exception);
            }
        }

        private static string DescribeItem(object itemInstance)
        {
            if (itemInstance == null)
            {
                return "<null>";
            }

            object value;
            string ignored;
            if (KingmakerGunslinger.Development.ReflectionAccess.TryGetFirstNonNullMember(
                itemInstance,
                new[] { "Name", "name" },
                out value,
                out ignored))
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            return itemInstance.GetType().FullName;
        }

        private static void LogInfo(string eventName, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Info("firearms", eventName, message);
            }
        }

        private static void LogWarning(string eventName, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Warning("firearms", eventName, message);
            }
        }

        private static void LogFault(
            string eventName,
            string message,
            Exception exception)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Failure(
                    "firearms",
                    eventName,
                    message,
                    exception);
            }
        }
    }

    /// <summary>
    /// Per-invocation Harmony state. It retains only strict token ID strings and no
    /// Kingmaker item, blueprint, unit, or context object.
    /// </summary>
    internal sealed class FirearmStateTokenReconciliationInvocation
    {
        private readonly string[] _before;

        internal static readonly FirearmStateTokenReconciliationInvocation Empty =
            new FirearmStateTokenReconciliationInvocation(Array.Empty<string>(), false);

        internal FirearmStateTokenReconciliationInvocation(
            IEnumerable<string> beforeTokenIds)
            : this(beforeTokenIds, true)
        {
        }

        private FirearmStateTokenReconciliationInvocation(
            IEnumerable<string> beforeTokenIds,
            bool hasObservation)
        {
            if (beforeTokenIds == null)
            {
                throw new ArgumentNullException("beforeTokenIds");
            }

            _before = beforeTokenIds.ToArray();
            HasObservation = hasObservation;
        }

        internal bool HasObservation { get; private set; }

        internal IReadOnlyList<string> BeforeTokenIds
        {
            get { return _before.ToArray(); }
        }
    }
}
