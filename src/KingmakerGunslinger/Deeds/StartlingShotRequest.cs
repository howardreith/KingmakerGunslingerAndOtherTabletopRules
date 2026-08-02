using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class StartlingShotRequest
    {
        internal StartlingShotRequest(bool exactEquippedFirearm,
            FirearmCondition condition, int loadedRounds, int currentGrit,
            bool validEnemyTarget)
        {
            if (!Enum.IsDefined(typeof(FirearmCondition), condition) ||
                condition == FirearmCondition.Unknown)
                throw new ArgumentOutOfRangeException("condition");
            if (loadedRounds < 0) throw new ArgumentOutOfRangeException("loadedRounds");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            ExactEquippedFirearm = exactEquippedFirearm;
            Condition = condition;
            LoadedRounds = loadedRounds;
            CurrentGrit = currentGrit;
            ValidEnemyTarget = validEnemyTarget;
        }

        internal bool ExactEquippedFirearm { get; private set; }
        internal FirearmCondition Condition { get; private set; }
        internal int LoadedRounds { get; private set; }
        internal int CurrentGrit { get; private set; }
        internal bool ValidEnemyTarget { get; private set; }
    }
}
