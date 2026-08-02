using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class LightningReloadRequest
    {
        public LightningReloadRequest(bool exactFirearm, FirearmCondition condition,
            int loadedRounds, int capacity, int currentGrit,
            bool hasBasicAmmunition, bool usedThisRound)
        {
            if (!Enum.IsDefined(typeof(FirearmCondition), condition))
                throw new ArgumentOutOfRangeException("condition");
            if (loadedRounds < 0) throw new ArgumentOutOfRangeException("loadedRounds");
            if (capacity < 1) throw new ArgumentOutOfRangeException("capacity");
            if (loadedRounds > capacity) throw new ArgumentOutOfRangeException("loadedRounds");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            ExactFirearm = exactFirearm; Condition = condition;
            LoadedRounds = loadedRounds; Capacity = capacity;
            CurrentGrit = currentGrit; HasBasicAmmunition = hasBasicAmmunition;
            UsedThisRound = usedThisRound;
        }

        public bool ExactFirearm { get; private set; }
        public FirearmCondition Condition { get; private set; }
        public int LoadedRounds { get; private set; }
        public int Capacity { get; private set; }
        public int CurrentGrit { get; private set; }
        public bool HasBasicAmmunition { get; private set; }
        public bool UsedThisRound { get; private set; }
    }
}
