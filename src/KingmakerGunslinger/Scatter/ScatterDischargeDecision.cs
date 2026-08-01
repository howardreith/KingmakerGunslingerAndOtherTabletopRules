using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>One-state-transition decision for a complete scatter action.</summary>
    internal sealed class ScatterDischargeDecision
    {
        internal ScatterDischargeDecision(
            ScatterDischargeStatus status,
            FirearmState before,
            FirearmState after,
            int targetCount,
            int roundsConsumed,
            bool shouldForceMiss)
        {
            if (status != ScatterDischargeStatus.RejectedBeforeDelivery &&
                status != ScatterDischargeStatus.Fired &&
                status != ScatterDischargeStatus.Empty &&
                status != ScatterDischargeStatus.Wrecked)
                throw new ArgumentOutOfRangeException("status");
            Before = before ?? throw new ArgumentNullException("before");
            After = after ?? throw new ArgumentNullException("after");
            if (targetCount < 0) throw new ArgumentOutOfRangeException("targetCount");
            if (roundsConsumed < 0 || roundsConsumed > 1)
                throw new ArgumentOutOfRangeException("roundsConsumed");

            Status = status;
            TargetCount = targetCount;
            RoundsConsumed = roundsConsumed;
            ShouldForceMiss = shouldForceMiss;
            Validate();
        }

        internal ScatterDischargeStatus Status { get; private set; }
        internal FirearmState Before { get; private set; }
        internal FirearmState After { get; private set; }
        internal int TargetCount { get; private set; }
        internal int RoundsConsumed { get; private set; }
        internal bool ShouldForceMiss { get; private set; }

        private void Validate()
        {
            if (Status == ScatterDischargeStatus.Fired)
            {
                if (RoundsConsumed != 1 || ShouldForceMiss || Before.IsEmpty ||
                    After != FirearmStateMachine.Fire(Before))
                    throw new ArgumentException("A fired scatter action must perform exactly one canonical Fire transition.");
                return;
            }
            if (After != Before || RoundsConsumed != 0)
                throw new ArgumentException("A non-fired scatter action must preserve state and consume nothing.");
            if (Status == ScatterDischargeStatus.RejectedBeforeDelivery && ShouldForceMiss)
                throw new ArgumentException("Pre-delivery rejection is not an attempted attack roll.");
            if ((Status == ScatterDischargeStatus.Empty || Status == ScatterDischargeStatus.Wrecked) &&
                !ShouldForceMiss)
                throw new ArgumentException("Empty or Wrecked scatter attempts must force a miss.");
        }
    }
}
