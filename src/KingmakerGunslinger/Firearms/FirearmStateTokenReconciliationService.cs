using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Pure comparison service used around ItemEntity.ApplyEnchantments. It restores
    /// only the unambiguous case where one known token existed before and none exists
    /// afterward. Every other changed or malformed set fails closed as a conflict.
    /// </summary>
    internal sealed class FirearmStateTokenReconciliationService
    {
        internal FirearmStateTokenReconciliationDecision Evaluate(
            IEnumerable<string> before,
            IEnumerable<string> after)
        {
            string[] beforeIds = Materialize(before, "before");
            string[] afterIds = Materialize(after, "after");

            if (beforeIds.Length == 0)
            {
                if (afterIds.Length == 0)
                {
                    return new FirearmStateTokenReconciliationDecision(
                        FirearmStateTokenReconciliationAction.NoToken,
                        beforeIds,
                        afterIds,
                        null,
                        "No firearm-state token existed before native reconciliation.");
                }

                return Conflict(
                    beforeIds,
                    afterIds,
                    "A firearm-state token appeared during native reconciliation without an observed before token.");
            }

            if (beforeIds.Length != 1)
            {
                return Conflict(
                    beforeIds,
                    afterIds,
                    "The before state contained multiple firearm-state tokens and cannot be reconciled implicitly.");
            }

            if (afterIds.Length == 0)
            {
                return new FirearmStateTokenReconciliationDecision(
                    FirearmStateTokenReconciliationAction.RestoreMissing,
                    beforeIds,
                    afterIds,
                    beforeIds[0],
                    "Kingmaker removed the one observed item-owned state token during native reconciliation.");
            }

            if (afterIds.Length == 1 &&
                string.Equals(beforeIds[0], afterIds[0], StringComparison.Ordinal))
            {
                return new FirearmStateTokenReconciliationDecision(
                    FirearmStateTokenReconciliationAction.Preserved,
                    beforeIds,
                    afterIds,
                    null,
                    "The item-owned state token survived native reconciliation unchanged.");
            }

            return Conflict(
                beforeIds,
                afterIds,
                "The firearm-state token set changed to a different or ambiguous value during native reconciliation.");
        }

        private static FirearmStateTokenReconciliationDecision Conflict(
            IEnumerable<string> before,
            IEnumerable<string> after,
            string reason)
        {
            return new FirearmStateTokenReconciliationDecision(
                FirearmStateTokenReconciliationAction.Conflict,
                before,
                after,
                null,
                reason);
        }

        private static string[] Materialize(
            IEnumerable<string> values,
            string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            string[] result = values.ToArray();
            if (result.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException(
                    "A reconciliation token collection cannot contain null or empty IDs.",
                    parameterName);
            }

            return result.Select(value => value.Trim()).ToArray();
        }
    }
}
