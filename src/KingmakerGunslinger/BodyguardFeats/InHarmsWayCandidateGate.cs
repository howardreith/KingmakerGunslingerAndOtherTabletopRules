using System;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal enum InHarmsWayCandidateRejection
    {
        None = 0,
        ModuleDisabled,
        AttackMissed,
        BodyguardNotAttempted,
        BodyguardFailed,
        MissingInHarmsWayFeat,
        InHarmsWayModeOff,
        MarkerMissing,
        ActivatableMarkerDivergence,
        ProtectorUnableToAct,
        SwiftCooldownActive,
        HasSwiftActionFalse,
        AlreadyIntercepted,
        DeliveryContractUnavailable,
        TargetRedirectionRejected,
        PolicyRejected
    }

    /// <summary>
    /// Complete, immutable-at-evaluation input for one successful Bodyguard
    /// user's In Harm's Way gate. Runtime facts are flattened here so gate
    /// ordering and rejection diagnostics remain independently testable.
    /// </summary>
    internal sealed class InHarmsWayCandidateGateInput
    {
        internal string PersistentId { get; set; }
        internal int PartyOrder { get; set; }
        internal bool ModuleEnabled { get; set; }
        internal bool AttackHit { get; set; }
        internal bool BodyguardAttempted { get; set; }
        internal bool BodyguardSucceeded { get; set; }
        internal int BodyguardContribution { get; set; }
        internal bool HasBodyguardFeat { get; set; }
        internal bool HasInHarmsWayFeat { get; set; }
        internal bool HasBodyguardActivatable { get; set; }
        internal bool HasInHarmsWayActivatable { get; set; }
        internal bool? BodyguardActivatableIsOn { get; set; }
        internal bool? InHarmsWayActivatableIsOn { get; set; }
        internal bool BodyguardMarkerPresent { get; set; }
        internal bool InHarmsWayMarkerPresent { get; set; }
        internal bool Alive { get; set; }
        internal bool Conscious { get; set; }
        internal bool CanAct { get; set; }
        internal bool HasSwiftAction { get; set; }
        internal float SwiftCooldown { get; set; }
        internal bool AlreadyIntercepted { get; set; }
        internal bool DeliveryContractAvailable { get; set; }
    }

    internal sealed class InHarmsWayCandidateGateDecision
    {
        internal InHarmsWayCandidateGateDecision(
            InHarmsWayCandidateRejection rejection)
        { Rejection = rejection; }

        internal InHarmsWayCandidateRejection Rejection { get; private set; }
        internal bool Eligible
        { get { return Rejection == InHarmsWayCandidateRejection.None; } }
        internal string Reason
        { get { return InHarmsWayCandidateGate.Reason(Rejection); } }
    }

    internal static class InHarmsWayCandidateGate
    {
        internal static InHarmsWayCandidateGateDecision Evaluate(
            InHarmsWayCandidateGateInput input)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (string.IsNullOrWhiteSpace(input.PersistentId))
                throw new ArgumentException(
                    "A persistent protector identity is required.", "input");
            if (!input.ModuleEnabled)
                return Reject(InHarmsWayCandidateRejection.ModuleDisabled);
            if (!input.AttackHit)
                return Reject(InHarmsWayCandidateRejection.AttackMissed);
            if (!input.BodyguardAttempted)
                return Reject(InHarmsWayCandidateRejection.BodyguardNotAttempted);
            if (!input.BodyguardSucceeded)
                return Reject(InHarmsWayCandidateRejection.BodyguardFailed);
            if (!input.HasInHarmsWayFeat)
                return Reject(InHarmsWayCandidateRejection
                    .MissingInHarmsWayFeat);

            if (!input.HasInHarmsWayActivatable)
            {
                if (input.InHarmsWayMarkerPresent)
                    return Reject(InHarmsWayCandidateRejection
                        .ActivatableMarkerDivergence);
                return Reject(InHarmsWayCandidateRejection
                    .InHarmsWayModeOff);
            }
            if (!input.InHarmsWayActivatableIsOn.HasValue)
            {
                if (!input.InHarmsWayMarkerPresent)
                    return Reject(InHarmsWayCandidateRejection.MarkerMissing);
            }
            else if (input.InHarmsWayActivatableIsOn.Value !=
                input.InHarmsWayMarkerPresent)
                return Reject(InHarmsWayCandidateRejection
                    .ActivatableMarkerDivergence);
            else if (!input.InHarmsWayActivatableIsOn.Value)
                return Reject(InHarmsWayCandidateRejection
                    .InHarmsWayModeOff);
            if (!input.InHarmsWayMarkerPresent)
                return Reject(InHarmsWayCandidateRejection.MarkerMissing);

            if (!input.Alive || !input.Conscious || !input.CanAct)
                return Reject(InHarmsWayCandidateRejection
                    .ProtectorUnableToAct);
            if (input.AlreadyIntercepted)
                return Reject(InHarmsWayCandidateRejection.AlreadyIntercepted);
            if (!input.DeliveryContractAvailable)
                return Reject(InHarmsWayCandidateRejection
                    .DeliveryContractUnavailable);
            if (float.IsNaN(input.SwiftCooldown) ||
                float.IsInfinity(input.SwiftCooldown))
                return Reject(InHarmsWayCandidateRejection
                    .HasSwiftActionFalse);
            if (input.SwiftCooldown > 0f)
                return Reject(InHarmsWayCandidateRejection
                    .SwiftCooldownActive);
            if (!input.HasSwiftAction)
                return Reject(InHarmsWayCandidateRejection
                    .HasSwiftActionFalse);
            return Reject(InHarmsWayCandidateRejection.None);
        }

        internal static string Reason(InHarmsWayCandidateRejection rejection)
        {
            switch (rejection)
            {
                case InHarmsWayCandidateRejection.None:
                    return "eligible";
                case InHarmsWayCandidateRejection.ModuleDisabled:
                    return "module-disabled";
                case InHarmsWayCandidateRejection.AttackMissed:
                    return "attack-missed";
                case InHarmsWayCandidateRejection.BodyguardNotAttempted:
                    return "bodyguard-not-attempted";
                case InHarmsWayCandidateRejection.BodyguardFailed:
                    return "bodyguard-failed";
                case InHarmsWayCandidateRejection.MissingInHarmsWayFeat:
                    return "missing-in-harms-way-feat";
                case InHarmsWayCandidateRejection.InHarmsWayModeOff:
                    return "in-harms-way-mode-off";
                case InHarmsWayCandidateRejection.MarkerMissing:
                    return "marker-missing";
                case InHarmsWayCandidateRejection
                    .ActivatableMarkerDivergence:
                    return "activatable-marker-divergence";
                case InHarmsWayCandidateRejection.ProtectorUnableToAct:
                    return "protector-unable-to-act";
                case InHarmsWayCandidateRejection.SwiftCooldownActive:
                    return "swift-cooldown-active";
                case InHarmsWayCandidateRejection.HasSwiftActionFalse:
                    return "has-swift-action-false";
                case InHarmsWayCandidateRejection.AlreadyIntercepted:
                    return "already-intercepted";
                case InHarmsWayCandidateRejection
                    .DeliveryContractUnavailable:
                    return "delivery-contract-unavailable";
                case InHarmsWayCandidateRejection.TargetRedirectionRejected:
                    return "target-redirection-rejected";
                case InHarmsWayCandidateRejection.PolicyRejected:
                    return "policy-rejected";
                default:
                    return "unknown";
            }
        }

        private static InHarmsWayCandidateGateDecision Reject(
            InHarmsWayCandidateRejection rejection)
        { return new InHarmsWayCandidateGateDecision(rejection); }
    }
}
