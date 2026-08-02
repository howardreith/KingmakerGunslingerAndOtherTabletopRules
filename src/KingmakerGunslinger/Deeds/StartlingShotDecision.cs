using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class StartlingShotDecision
    {
        internal StartlingShotDecision(StartlingShotStatus status,
            int chamberCost, int gritCost, int durationRounds)
        {
            if (!Enum.IsDefined(typeof(StartlingShotStatus), status))
                throw new ArgumentOutOfRangeException("status");
            if (chamberCost < 0) throw new ArgumentOutOfRangeException("chamberCost");
            if (gritCost < 0) throw new ArgumentOutOfRangeException("gritCost");
            if (durationRounds < 0)
                throw new ArgumentOutOfRangeException("durationRounds");
            Status = status;
            ChamberCost = chamberCost;
            GritCost = gritCost;
            DurationRounds = durationRounds;
        }

        internal StartlingShotStatus Status { get; private set; }
        internal int ChamberCost { get; private set; }
        internal int GritCost { get; private set; }
        internal int DurationRounds { get; private set; }
        internal bool ShouldApply { get { return Status == StartlingShotStatus.Eligible; } }
    }
}
