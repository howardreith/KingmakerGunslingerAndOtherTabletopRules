using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeadeyeDecision
    {
        internal DeadeyeDecision(DeadeyeStatus status, int rangeIncrement,
            int gritCost)
        {
            if (!Enum.IsDefined(typeof(DeadeyeStatus), status))
                throw new ArgumentOutOfRangeException(nameof(status));
            if (rangeIncrement < 0)
                throw new ArgumentOutOfRangeException(nameof(rangeIncrement));
            if (gritCost < 0)
                throw new ArgumentOutOfRangeException(nameof(gritCost));
            if (status == DeadeyeStatus.Eligible &&
                (rangeIncrement < 2 || gritCost != rangeIncrement - 1))
                throw new ArgumentException("Eligible Deadeye cost must equal range increment minus one.");
            if (status != DeadeyeStatus.Eligible && gritCost != 0)
                throw new ArgumentException("Rejected Deadeye decisions cannot spend grit.");
            Status = status;
            RangeIncrement = rangeIncrement;
            GritCost = gritCost;
        }

        internal DeadeyeStatus Status { get; private set; }
        internal int RangeIncrement { get; private set; }
        internal int GritCost { get; private set; }
        internal bool UsesTouchArmorClass { get { return Status == DeadeyeStatus.Eligible; } }
    }
}
