namespace KingmakerGunslinger.Deeds
{
    internal sealed class StopBleedingDecision
    {
        internal StopBleedingDecision(StopBleedingStatus status,
            int roundsConsumed)
        {
            Status = status;
            RoundsConsumed = roundsConsumed;
        }

        internal StopBleedingStatus Status { get; private set; }
        internal int RoundsConsumed { get; private set; }
        internal bool ShouldApply { get { return Status == StopBleedingStatus.Eligible; } }
    }
}
