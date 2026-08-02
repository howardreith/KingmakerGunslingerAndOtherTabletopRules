using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class CheatDeathDecision
    {
        internal CheatDeathDecision(CheatDeathStatus status, int gritCost,
            int finalHitPoints)
        {
            if (gritCost < 0) throw new ArgumentOutOfRangeException("gritCost");
            if (status == CheatDeathStatus.Applied &&
                (gritCost < 1 || finalHitPoints != 1))
                throw new ArgumentException("Applied Cheat Death must spend grit and leave 1 HP.");
            if (status != CheatDeathStatus.Applied && gritCost != 0)
                throw new ArgumentException("Rejected Cheat Death cannot spend grit.");
            Status = status;
            GritCost = gritCost;
            FinalHitPoints = finalHitPoints;
        }

        internal CheatDeathStatus Status { get; private set; }
        internal int GritCost { get; private set; }
        internal int FinalHitPoints { get; private set; }
        internal bool Applied { get { return Status == CheatDeathStatus.Applied; } }
    }
}
