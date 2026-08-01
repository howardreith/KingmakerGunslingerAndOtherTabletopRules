using System;

namespace KingmakerGunslinger.Scatter
{
    internal sealed class ScatterConeDistanceDecision
    {
        internal ScatterConeDistanceDecision(int distanceFeet, float distanceMeters)
        {
            if (distanceFeet <= 0 || distanceFeet % 5 != 0)
                throw new ArgumentOutOfRangeException("distanceFeet");
            if (float.IsNaN(distanceMeters) || float.IsInfinity(distanceMeters) || distanceMeters <= 0f)
                throw new ArgumentOutOfRangeException("distanceMeters");
            DistanceFeet = distanceFeet;
            DistanceMeters = distanceMeters;
        }

        internal int DistanceFeet { get; private set; }
        internal float DistanceMeters { get; private set; }
    }
}
