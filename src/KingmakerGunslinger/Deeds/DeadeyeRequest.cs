using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeadeyeRequest
    {
        internal DeadeyeRequest(bool isArmed, bool isExactFirearm,
            int markerCount, FirearmDefinition definition, double distanceMeters)
            : this(isArmed, isExactFirearm, markerCount, definition,
                distanceMeters, 0)
        {
        }

        internal DeadeyeRequest(bool isArmed, bool isExactFirearm,
            int markerCount, FirearmDefinition definition, double distanceMeters,
            int rangeIncrementBonusFeet)
        {
            if (markerCount < 0) throw new ArgumentOutOfRangeException(nameof(markerCount));
            IsArmed = isArmed;
            IsExactFirearm = isExactFirearm;
            MarkerCount = markerCount;
            Definition = definition;
            DistanceMeters = distanceMeters;
            RangeIncrementBonusFeet = rangeIncrementBonusFeet;
        }

        internal bool IsArmed { get; private set; }
        internal bool IsExactFirearm { get; private set; }
        internal int MarkerCount { get; private set; }
        internal FirearmDefinition Definition { get; private set; }
        internal double DistanceMeters { get; private set; }
        internal int RangeIncrementBonusFeet { get; private set; }
    }
}
