using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class StunningShotRequest
    {
        internal StunningShotRequest(int level, int wisdomModifier,
            int currentGrit, bool exactFirearm, bool ownedAttack, bool hit,
            bool immuneToCriticalHit, bool firstEvaluation)
        {
            if (level < 0) throw new ArgumentOutOfRangeException("level");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            Level = level; WisdomModifier = wisdomModifier;
            CurrentGrit = currentGrit; ExactFirearm = exactFirearm;
            OwnedAttack = ownedAttack; Hit = hit;
            ImmuneToCriticalHit = immuneToCriticalHit;
            FirstEvaluation = firstEvaluation;
        }
        internal int Level { get; private set; }
        internal int WisdomModifier { get; private set; }
        internal int CurrentGrit { get; private set; }
        internal bool ExactFirearm { get; private set; }
        internal bool OwnedAttack { get; private set; }
        internal bool Hit { get; private set; }
        internal bool ImmuneToCriticalHit { get; private set; }
        internal bool FirstEvaluation { get; private set; }
    }
}
