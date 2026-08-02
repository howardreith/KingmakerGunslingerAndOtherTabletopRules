using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class MenacingShotTargetDecision
    {
        internal const double RadiusMeters = 30d * 0.3048d;
        internal MenacingShotTargetDecision(bool living, double distanceMeters)
        {
            if (double.IsNaN(distanceMeters) || double.IsInfinity(distanceMeters) ||
                distanceMeters < 0d) throw new ArgumentOutOfRangeException("distanceMeters");
            IsLiving = living; DistanceMeters = distanceMeters;
        }
        internal bool IsLiving { get; private set; }
        internal double DistanceMeters { get; private set; }
        internal bool IsAffected
        { get { return IsLiving && DistanceMeters <= RadiusMeters + 0.001d; } }
    }
}
