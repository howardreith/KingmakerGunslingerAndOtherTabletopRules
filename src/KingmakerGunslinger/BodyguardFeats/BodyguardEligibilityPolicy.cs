using System;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal enum BodyguardEligibilityFailure
    {
        None = 0,
        ModuleDisabled,
        NotHostileAttack,
        NotAlly,
        AttackerNotHostile,
        ProtectorIsAttacker,
        ProtectorIsTarget,
        FeatureAbsent,
        ModeOff,
        Dead,
        Unconscious,
        UnableToAct,
        NativeAooDenied,
        NoAooRemaining,
        NotAdjacent,
        AttackerNotThreatened
    }

    internal sealed class BodyguardEligibilityRequest
    {
        internal bool ModuleEnabled { get; set; }
        internal bool HostileAttackRoll { get; set; }
        internal bool TargetIsAlly { get; set; }
        internal bool AttackerIsHostile { get; set; }
        internal bool ProtectorIsAttacker { get; set; }
        internal bool ProtectorIsTarget { get; set; }
        internal bool HasBodyguard { get; set; }
        internal bool BodyguardModeActive { get; set; }
        internal bool Alive { get; set; }
        internal bool Conscious { get; set; }
        internal bool AbleToAct { get; set; }
        internal bool NativeAooAllowed { get; set; }
        internal int AooRemaining { get; set; }
        internal double ProtectorTargetEdgeDistanceFeet { get; set; }
        internal double AdjacencyFeet { get; set; }
        internal double DistanceToleranceFeet { get; set; }
        internal bool ThreatensAttacker { get; set; }
    }

    internal sealed class BodyguardEligibilityDecision
    {
        internal BodyguardEligibilityDecision(BodyguardEligibilityFailure failure)
        { Failure = failure; }
        internal BodyguardEligibilityFailure Failure { get; private set; }
        internal bool Eligible { get { return Failure == BodyguardEligibilityFailure.None; } }
    }

    internal static class BodyguardEligibilityPolicy
    {
        internal static BodyguardEligibilityDecision Evaluate(
            BodyguardEligibilityRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (!request.ModuleEnabled) return Deny(
                BodyguardEligibilityFailure.ModuleDisabled);
            if (!request.HostileAttackRoll) return Deny(
                BodyguardEligibilityFailure.NotHostileAttack);
            if (!request.TargetIsAlly) return Deny(
                BodyguardEligibilityFailure.NotAlly);
            if (!request.AttackerIsHostile) return Deny(
                BodyguardEligibilityFailure.AttackerNotHostile);
            if (request.ProtectorIsAttacker) return Deny(
                BodyguardEligibilityFailure.ProtectorIsAttacker);
            if (request.ProtectorIsTarget) return Deny(
                BodyguardEligibilityFailure.ProtectorIsTarget);
            if (!request.HasBodyguard) return Deny(
                BodyguardEligibilityFailure.FeatureAbsent);
            if (!request.BodyguardModeActive) return Deny(
                BodyguardEligibilityFailure.ModeOff);
            if (!request.Alive) return Deny(BodyguardEligibilityFailure.Dead);
            if (!request.Conscious) return Deny(
                BodyguardEligibilityFailure.Unconscious);
            if (!request.AbleToAct) return Deny(
                BodyguardEligibilityFailure.UnableToAct);
            if (!request.NativeAooAllowed) return Deny(
                BodyguardEligibilityFailure.NativeAooDenied);
            if (request.AooRemaining <= 0) return Deny(
                BodyguardEligibilityFailure.NoAooRemaining);
            if (double.IsNaN(request.ProtectorTargetEdgeDistanceFeet) ||
                double.IsInfinity(request.ProtectorTargetEdgeDistanceFeet) ||
                double.IsNaN(request.AdjacencyFeet) ||
                double.IsInfinity(request.AdjacencyFeet) ||
                double.IsNaN(request.DistanceToleranceFeet) ||
                double.IsInfinity(request.DistanceToleranceFeet) ||
                request.AdjacencyFeet < 0d || request.DistanceToleranceFeet < 0d ||
                request.ProtectorTargetEdgeDistanceFeet > request.AdjacencyFeet +
                    request.DistanceToleranceFeet)
                return Deny(BodyguardEligibilityFailure.NotAdjacent);
            if (!request.ThreatensAttacker) return Deny(
                BodyguardEligibilityFailure.AttackerNotThreatened);
            return new BodyguardEligibilityDecision(BodyguardEligibilityFailure.None);
        }

        private static BodyguardEligibilityDecision Deny(
            BodyguardEligibilityFailure failure)
        { return new BodyguardEligibilityDecision(failure); }
    }
}
