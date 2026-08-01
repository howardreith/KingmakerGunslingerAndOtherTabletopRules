using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class StopBleedingService
    {
        private const double FiveFeetMeters = 5d * 0.3048d;
        private const double DistanceTolerance = 0.001d;

        internal StopBleedingDecision Evaluate(StopBleedingRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (!request.ExactEquippedFirearm)
                return Reject(StopBleedingStatus.NotExactEquippedFirearm);
            if (request.Condition == FirearmCondition.Wrecked)
                return Reject(StopBleedingStatus.Wrecked);
            if (request.LoadedRounds < 1)
                return Reject(StopBleedingStatus.Empty);
            if (request.CurrentGrit < 1)
                return Reject(StopBleedingStatus.InsufficientGrit);
            if (request.DistanceMeters > FiveFeetMeters + DistanceTolerance)
                return Reject(StopBleedingStatus.OutOfRange);
            if (request.BleedCount < 1)
                return Reject(StopBleedingStatus.NoBleed);
            return new StopBleedingDecision(StopBleedingStatus.Eligible, 1);
        }

        private static StopBleedingDecision Reject(StopBleedingStatus status)
        {
            return new StopBleedingDecision(status, 0);
        }
    }
}
