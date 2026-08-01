using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class QuickClearRequest
    {
        internal QuickClearRequest(QuickClearMode mode, bool exactEquippedFirearm,
            FirearmCondition condition, bool brokenByMisfire, int currentGrit)
        {
            if (!Enum.IsDefined(typeof(QuickClearMode), mode) || mode == QuickClearMode.Unknown)
                throw new ArgumentOutOfRangeException("mode");
            if (!Enum.IsDefined(typeof(FirearmCondition), condition))
                throw new ArgumentOutOfRangeException("condition");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            Mode = mode; ExactEquippedFirearm = exactEquippedFirearm;
            Condition = condition; BrokenByMisfire = brokenByMisfire;
            CurrentGrit = currentGrit;
        }

        internal QuickClearMode Mode { get; private set; }
        internal bool ExactEquippedFirearm { get; private set; }
        internal FirearmCondition Condition { get; private set; }
        internal bool BrokenByMisfire { get; private set; }
        internal int CurrentGrit { get; private set; }
    }
}
