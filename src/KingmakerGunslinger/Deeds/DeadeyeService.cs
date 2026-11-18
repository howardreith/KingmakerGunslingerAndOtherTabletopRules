using System;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Rules;

namespace KingmakerGunslinger.Deeds
{
    /// <summary>
    /// Pure Deadeye range policy for an armed shot. It authorizes only the
    /// touch-AC delta beyond the first range increment; native range
    /// penalties remain in Kingmaker's ordinary attack pipeline. It decides
    /// no cost: arming is paid once at activation (DeadeyeCostPolicy).
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
                    EffectiveFirearmRangePolicy.IncrementFeet(request.Definition,
                        request.RangeIncrementBonusFeet) *
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
            return new DeadeyeDecision(DeadeyeStatus.Eligible, increment);
        }

        private static DeadeyeDecision Reject(DeadeyeStatus status, int increment)
        {
            return new DeadeyeDecision(status, increment);
        }
    }
}
