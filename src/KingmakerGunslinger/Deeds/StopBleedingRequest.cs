using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class StopBleedingRequest
    {
        internal StopBleedingRequest(bool exactEquippedFirearm,
            FirearmCondition condition, int loadedRounds, int currentGrit,
            double distanceMeters, int bleedCount)
        {
            if (!Enum.IsDefined(typeof(FirearmCondition), condition) ||
                condition == FirearmCondition.Unknown)
                throw new ArgumentOutOfRangeException("condition");
            if (loadedRounds < 0) throw new ArgumentOutOfRangeException("loadedRounds");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            if (double.IsNaN(distanceMeters) || double.IsInfinity(distanceMeters) ||
                distanceMeters < 0d) throw new ArgumentOutOfRangeException("distanceMeters");
            if (bleedCount < 0) throw new ArgumentOutOfRangeException("bleedCount");
            ExactEquippedFirearm = exactEquippedFirearm;
            Condition = condition;
            LoadedRounds = loadedRounds;
            CurrentGrit = currentGrit;
            DistanceMeters = distanceMeters;
            BleedCount = bleedCount;
        }

        internal bool ExactEquippedFirearm { get; private set; }
        internal FirearmCondition Condition { get; private set; }
        internal int LoadedRounds { get; private set; }
        internal int CurrentGrit { get; private set; }
        internal double DistanceMeters { get; private set; }
        internal int BleedCount { get; private set; }
    }
}
