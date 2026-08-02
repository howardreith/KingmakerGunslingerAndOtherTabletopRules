using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeadShotDecision
    {
        internal DeadShotDecision(DeadShotStatus status, int[] attackBonuses,
            int gritCost, int chamberCost)
        {
            if (!Enum.IsDefined(typeof(DeadShotStatus), status))
                throw new ArgumentOutOfRangeException("status");
            if (attackBonuses == null) throw new ArgumentNullException("attackBonuses");
            Status = status;
            AttackBonuses = (int[])attackBonuses.Clone();
            GritCost = gritCost;
            ChamberCost = chamberCost;
        }

        internal DeadShotStatus Status { get; private set; }
        internal int[] AttackBonuses { get; private set; }
        internal int GritCost { get; private set; }
        internal int ChamberCost { get; private set; }
        internal bool ShouldAttack { get { return Status == DeadShotStatus.Eligible; } }
    }
}
