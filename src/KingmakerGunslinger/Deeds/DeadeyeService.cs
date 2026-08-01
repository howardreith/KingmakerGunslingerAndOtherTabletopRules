using System;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Rules;

namespace KingmakerGunslinger.Deeds
{
    /// <summary>
    /// Pure Deadeye range/cost policy. It authorizes only the touch-AC delta;
    /// native range penalties remain in Kingmaker's ordinary attack pipeline.
    /// </summary>
    internal sealed class DeadeyeService
    {
        internal DeadeyeDecision Evaluate(DeadeyeRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!request.IsArmed)
                return Reject(DeadeyeStatus.NotArmed, 0);
            if (!request.IsExactFirearm || request.MarkerCount != 1)
                return Reject(DeadeyeStatus.NotExactFirearm, 0);
            if (request.Definition == null ||
                !request.Definition.HasFixedRangeIncrement ||
                double.IsNaN(request.DistanceMeters) ||
                double.IsInfinity(request.DistanceMeters) ||
                request.DistanceMeters < 0d)
                return Reject(DeadeyeStatus.UnsupportedRange, 0);

            int increment;
            try
            {
                double tolerantDistance = Math.Max(0d, request.DistanceMeters -
                    FirearmArmorClassService.RangeBoundaryToleranceMeters);
                increment = FirearmRangeMath.CalculateIncrement(tolerantDistance,
                    request.Definition.RangeIncrementFeet *
                        FirearmArmorClassService.MetersPerFoot);
            }
            catch (ArgumentOutOfRangeException)
            {
                return Reject(DeadeyeStatus.UnsupportedRange, 0);
            }
            catch (OverflowException)
            {
                return Reject(DeadeyeStatus.UnsupportedRange, 0);
            }

            if (increment <= 1)
                return Reject(DeadeyeStatus.WithinFirstIncrement, increment);
            int cost = checked(increment - 1);
            if (request.CurrentGrit < cost)
                return Reject(DeadeyeStatus.InsufficientGrit, increment);
            return new DeadeyeDecision(DeadeyeStatus.Eligible, increment, cost);
        }

        private static DeadeyeDecision Reject(DeadeyeStatus status, int increment)
        {
            return new DeadeyeDecision(status, increment, 0);
        }
    }
}
