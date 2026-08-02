using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingHeadRequest
    {
        internal TargetingHeadRequest(bool exactFirearm,
            FirearmCondition condition, int loadedRounds, int currentGrit,
            bool validTarget)
        {
            if (!Enum.IsDefined(typeof(FirearmCondition), condition) ||
                condition == FirearmCondition.Unknown)
                throw new ArgumentOutOfRangeException("condition");
            if (loadedRounds < 0) throw new ArgumentOutOfRangeException("loadedRounds");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            ExactFirearm = exactFirearm; Condition = condition;
            LoadedRounds = loadedRounds; CurrentGrit = currentGrit;
            ValidTarget = validTarget;
        }
        internal bool ExactFirearm { get; private set; }
        internal FirearmCondition Condition { get; private set; }
        internal int LoadedRounds { get; private set; }
        internal int CurrentGrit { get; private set; }
        internal bool ValidTarget { get; private set; }
    }
}
