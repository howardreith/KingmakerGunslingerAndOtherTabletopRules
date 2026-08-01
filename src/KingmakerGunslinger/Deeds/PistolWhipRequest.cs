using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class PistolWhipRequest
    {
        internal PistolWhipRequest(bool exactEquippedFirearm, bool twoHanded,
            FirearmCondition condition, int currentGrit)
        {
            if (!Enum.IsDefined(typeof(FirearmCondition), condition))
                throw new ArgumentOutOfRangeException("condition");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            ExactEquippedFirearm = exactEquippedFirearm;
            TwoHanded = twoHanded;
            Condition = condition;
            CurrentGrit = currentGrit;
        }

        internal bool ExactEquippedFirearm { get; private set; }
        internal bool TwoHanded { get; private set; }
        internal FirearmCondition Condition { get; private set; }
        internal int CurrentGrit { get; private set; }
    }
}
