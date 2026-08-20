using System;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Rules
{
    /// <summary>
    /// Pure selector for the Sprint 9 early-firearm penetration rule.
    /// It preserves contextual AC adjustments by adding the difference between
    /// touch AC and ordinary AC to the rule event's already-calculated TargetAC.
    /// </summary>
    internal static class FirearmArmorClassService
    {
        internal const double MetersPerFoot = 0.3048d;
        internal const double RangeBoundaryToleranceMeters = 0.0001d;

        internal static FirearmArmorClassDecision Select(
            FirearmArmorClassRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!request.IsExactFirearm || request.MarkerCount != 1)
            {
                return Ordinary(request, 0, "not-exact-firearm");
            }

            if (request.Definition == null)
            {
                return Ordinary(request, 0, "missing-firearm-definition");
            }

            if (request.AlreadyApplied)
            {
                return Ordinary(request, 0, "already-applied");
            }

            if (!request.Definition.HasFixedRangeIncrement)
            {
                return Ordinary(request, 0, "special-range-not-implemented");
            }

            if (double.IsNaN(request.DistanceMeters) ||
                double.IsInfinity(request.DistanceMeters) ||
                request.DistanceMeters < 0d)
            {
                return Ordinary(request, 0, "invalid-range-input");
            }

            int rangeIncrement;
            double penetrationRangeFeet;
            try
            {
                double incrementFeet = EffectiveFirearmRangePolicy.IncrementFeet(
                    request.Definition, request.RangeIncrementBonusFeet);
                double incrementMeters = incrementFeet * MetersPerFoot;
                penetrationRangeFeet = FirearmPenetrationRangePolicy
                    .EffectivePenetrationRangeFeet(request.Definition,
                        request.RangeIncrementBonusFeet);
                double tolerantDistance = Math.Max(
                    0d,
                    request.DistanceMeters - RangeBoundaryToleranceMeters);
                rangeIncrement = FirearmRangeMath.CalculateIncrement(
                    tolerantDistance,
                    incrementMeters);
            }
            catch (ArgumentOutOfRangeException)
            {
                return Ordinary(request, 0, "invalid-range-input");
            }
            catch (OverflowException)
            {
                return Ordinary(request, 0, "range-overflow");
            }

            if (!FirearmPenetrationRangePolicy.UsesTouchArmorClass(
                    request.Definition, rangeIncrement,
                    request.DeadeyeAuthorized))
            {
                return Ordinary(request, rangeIncrement,
                    "outside-firearm-penetration-range", penetrationRangeFeet);
            }

            long adjustment = (long)request.TouchArmorClass - request.OrdinaryArmorClass;
            long selected = (long)request.CurrentTargetArmorClass + adjustment;
            if (adjustment < int.MinValue || adjustment > int.MaxValue ||
                selected < int.MinValue || selected > int.MaxValue)
            {
                return Ordinary(request, rangeIncrement, "armor-class-overflow");
            }

            int selectedArmorClass = (int)selected;
            int integerAdjustment = (int)adjustment;
            return new FirearmArmorClassDecision(
                true,
                selectedArmorClass != request.CurrentTargetArmorClass,
                selectedArmorClass,
                integerAdjustment,
                rangeIncrement,
                penetrationRangeFeet,
                selectedArmorClass == request.CurrentTargetArmorClass
                    ? "touch-ac-selected-no-numeric-delta"
                    : request.DeadeyeAuthorized && rangeIncrement > 1
                        ? "touch-ac-deadeye"
                        : request.Definition.Era == FirearmEra.Advanced
                            ? "touch-ac-advanced-penetration"
                            : "touch-ac-first-range-increment");
        }

        private static FirearmArmorClassDecision Ordinary(
            FirearmArmorClassRequest request,
            int rangeIncrement,
            string reason,
            double effectivePenetrationRangeFeet = 0d)
        {
            return new FirearmArmorClassDecision(
                false,
                false,
                request.CurrentTargetArmorClass,
                0,
                rangeIncrement,
                effectivePenetrationRangeFeet,
                reason);
        }
    }
}
