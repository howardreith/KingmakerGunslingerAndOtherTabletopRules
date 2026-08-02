namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeathsShotDecision
    {
        internal DeathsShotDecision(bool consume, bool shouldSave, int cost, int dc)
        { ConsumeMarker = consume; ShouldSave = shouldSave; GritCost = cost;
            DifficultyClass = dc; }
        internal bool ConsumeMarker { get; private set; }
        internal bool ShouldSave { get; private set; }
        internal int GritCost { get; private set; }
        internal int DifficultyClass { get; private set; }
    }
}
