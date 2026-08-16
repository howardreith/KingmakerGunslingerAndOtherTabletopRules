using System;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal static class ControlledRageHitPointPolicy
    {
        internal static int ReconcileCurrentHitPoints(int currentHitPoints,
            int oldMaximumHitPoints, int newMaximumHitPoints)
        {
            if (oldMaximumHitPoints < 1)
                throw new ArgumentOutOfRangeException("oldMaximumHitPoints");
            if (newMaximumHitPoints < 1)
                throw new ArgumentOutOfRangeException("newMaximumHitPoints");
            if (currentHitPoints > oldMaximumHitPoints)
                throw new ArgumentOutOfRangeException("currentHitPoints");
            long reconciled = (long)currentHitPoints + newMaximumHitPoints -
                oldMaximumHitPoints;
            if (reconciled > newMaximumHitPoints) reconciled = newMaximumHitPoints;
            if (reconciled < int.MinValue) return int.MinValue;
            return (int)reconciled;
        }
    }
}
