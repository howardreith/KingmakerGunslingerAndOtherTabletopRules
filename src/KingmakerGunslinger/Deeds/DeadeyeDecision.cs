using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeadeyeDecision
    {
        internal DeadeyeDecision(DeadeyeStatus status, int rangeIncrement)
        {
            if (!Enum.IsDefined(typeof(DeadeyeStatus), status))
                throw new ArgumentOutOfRangeException(nameof(status));
            if (rangeIncrement < 0)
                throw new ArgumentOutOfRangeException(nameof(rangeIncrement));
            if (status == DeadeyeStatus.Eligible && rangeIncrement < 2)
                throw new ArgumentException("Eligible Deadeye requires a range increment beyond the first.");
            Status = status;
            RangeIncrement = rangeIncrement;
        }

        internal DeadeyeStatus Status { get; private set; }
        internal int RangeIncrement { get; private set; }
        internal bool UsesTouchArmorClass { get { return Status == DeadeyeStatus.Eligible; } }
    }
}
