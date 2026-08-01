namespace KingmakerGunslinger.Deeds
{
    internal sealed class QuickClearDecision
    {
        internal QuickClearDecision(QuickClearStatus status, QuickClearMode mode,
            int gritCost)
        {
            Status = status; Mode = mode; GritCost = gritCost;
        }

        internal QuickClearStatus Status { get; private set; }
        internal QuickClearMode Mode { get; private set; }
        internal int GritCost { get; private set; }
        internal bool ShouldRepair { get { return Status == QuickClearStatus.Eligible; } }
    }
}
