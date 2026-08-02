using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class MenacingShotRequest
    {
        internal MenacingShotRequest(int gunslingerLevel, int wisdomModifier,
            bool exactEquippedFirearm, FirearmCondition condition,
            int loadedRounds, int currentGrit)
        {
            if (gunslingerLevel < 0 || gunslingerLevel > 20)
                throw new ArgumentOutOfRangeException("gunslingerLevel");
            if (wisdomModifier < -20 || wisdomModifier > 20)
                throw new ArgumentOutOfRangeException("wisdomModifier");
            if (!Enum.IsDefined(typeof(FirearmCondition), condition) ||
                condition == FirearmCondition.Unknown)
                throw new ArgumentOutOfRangeException("condition");
            if (loadedRounds < 0) throw new ArgumentOutOfRangeException("loadedRounds");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            GunslingerLevel = gunslingerLevel;
            WisdomModifier = wisdomModifier;
            ExactEquippedFirearm = exactEquippedFirearm;
            Condition = condition;
            LoadedRounds = loadedRounds;
            CurrentGrit = currentGrit;
        }

        internal int GunslingerLevel { get; private set; }
        internal int WisdomModifier { get; private set; }
        internal bool ExactEquippedFirearm { get; private set; }
        internal FirearmCondition Condition { get; private set; }
        internal int LoadedRounds { get; private set; }
        internal int CurrentGrit { get; private set; }
    }
}
