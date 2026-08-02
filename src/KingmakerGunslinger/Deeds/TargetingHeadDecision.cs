namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingHeadDecision
    {
        internal TargetingHeadDecision(TargetingHeadStatus status) { Status = status; }
        internal TargetingHeadStatus Status { get; private set; }
        internal bool ShouldAttack { get { return Status == TargetingHeadStatus.Accepted; } }
        internal int GritCost { get { return ShouldAttack ? 1 : 0; } }
    }
}
