using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeadShotRollObservation
    {
        internal DeadShotRollObservation(bool hit, bool misfire, bool criticalThreat)
        {
            if (misfire && hit) throw new ArgumentException("A misfire cannot hit.");
            if (criticalThreat && !hit)
                throw new ArgumentException("A critical threat must hit.");
            Hit = hit;
            Misfire = misfire;
            CriticalThreat = criticalThreat;
        }

        internal bool Hit { get; private set; }
        internal bool Misfire { get; private set; }
        internal bool CriticalThreat { get; private set; }
    }
}
