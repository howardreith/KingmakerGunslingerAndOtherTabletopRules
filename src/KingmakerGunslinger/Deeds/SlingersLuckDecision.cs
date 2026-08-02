namespace KingmakerGunslinger.Deeds
{
    internal sealed class SlingersLuckDecision
    {
        internal SlingersLuckDecision(SlingersLuckStatus status, int gritCost,
            int result)
        { Status = status; GritCost = gritCost; Result = result; }
        internal SlingersLuckStatus Status { get; private set; }
        internal int GritCost { get; private set; }
        internal int Result { get; private set; }
        internal bool Applied { get { return Status == SlingersLuckStatus.Applied; } }
        internal bool ConsumeMarker { get { return Applied; } }
    }
}
