using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurModifierAdjustmentRequest
    {
        internal bool ExecutionCommitted { get; set; }
        internal BrownFurAbilityScore SelectedAbilityScore { get; set; }
        internal BrownFurAbilityScore ModifierAbilityScore { get; set; }
        internal int OriginalValue { get; set; }
        internal int Increase { get; set; }
        internal string OriginalDescriptor { get; set; }
        internal string CarrierFamily { get; set; }
        internal object SourceFact { get; set; }
        internal object ExpectedSourceFact { get; set; }
        internal object SourceContext { get; set; }
        internal object ExpectedSourceContext { get; set; }
    }

    internal sealed class BrownFurModifierAdjustmentDecision
    {
        internal BrownFurModifierAdjustmentDecision(bool eligible,
            string failure, int adjustedValue, string retainedDescriptor)
        {
            Eligible = eligible;
            Failure = failure ?? string.Empty;
            AdjustedValue = adjustedValue;
            RetainedDescriptor = retainedDescriptor ?? string.Empty;
        }

        internal bool Eligible { get; private set; }
        internal string Failure { get; private set; }
        internal int AdjustedValue { get; private set; }
        internal string RetainedDescriptor { get; private set; }

        internal static BrownFurModifierAdjustmentDecision Reject(
            string failure, BrownFurModifierAdjustmentRequest request)
        {
            return new BrownFurModifierAdjustmentDecision(false, failure,
                request == null ? 0 : request.OriginalValue,
                request == null ? string.Empty : request.OriginalDescriptor);
        }
    }

    internal static class BrownFurModifierAdjustmentPolicy
    {
        private static readonly HashSet<string> SupportedCarriers =
            new HashSet<string>(StringComparer.Ordinal) {
                "AddStatBonus", "AddContextStatBonus", "AddGenericStatBonus",
                "AddStatBonusAbilityValue", "Polymorph", "ChangeUnitSize"
            };

        internal static BrownFurModifierAdjustmentDecision Decide(
            BrownFurModifierAdjustmentRequest request)
        {
            if (request == null) return Reject("modifier-request-missing", null);
            if (!request.ExecutionCommitted)
                return Reject("modifier-execution-not-committed", request);
            if (request.SelectedAbilityScore == BrownFurAbilityScore.None)
                return Reject("modifier-stat-not-selected", request);
            if (request.ModifierAbilityScore != request.SelectedAbilityScore)
                return Reject("modifier-stat-not-selected-stat", request);
            if (request.OriginalValue <= 0)
                return Reject("modifier-not-positive-bonus", request);
            if (request.Increase != 2 && request.Increase != 4)
                return Reject("modifier-increase-invalid", request);
            if (string.IsNullOrWhiteSpace(request.OriginalDescriptor))
                return Reject("modifier-descriptor-missing", request);
            if (string.IsNullOrWhiteSpace(request.CarrierFamily) ||
                !SupportedCarriers.Contains(request.CarrierFamily))
                return Reject("modifier-carrier-unsupported", request);
            if (request.SourceFact == null || request.ExpectedSourceFact == null ||
                !ReferenceEquals(request.SourceFact, request.ExpectedSourceFact))
                return Reject("modifier-source-fact-mismatch", request);
            if (request.SourceContext == null ||
                request.ExpectedSourceContext == null ||
                !ReferenceEquals(request.SourceContext,
                    request.ExpectedSourceContext))
                return Reject("modifier-source-context-mismatch", request);
            int adjusted;
            try { adjusted = checked(request.OriginalValue + request.Increase); }
            catch (OverflowException)
            { return Reject("modifier-value-overflow", request); }
            return new BrownFurModifierAdjustmentDecision(true, string.Empty,
                adjusted, request.OriginalDescriptor);
        }

        internal static bool IsSupportedCarrier(string family)
        { return !string.IsNullOrWhiteSpace(family) &&
            SupportedCarriers.Contains(family); }

        private static BrownFurModifierAdjustmentDecision Reject(string failure,
            BrownFurModifierAdjustmentRequest request)
        { return BrownFurModifierAdjustmentDecision.Reject(failure, request); }
    }
}
