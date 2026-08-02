namespace KingmakerGunslinger.Deeds
{
    internal sealed class MenacingShotDecision
    {
        internal MenacingShotDecision(MenacingShotStatus status, int dc,
            int frightenedRounds, int gritCost, int roundsConsumed)
        {
            Status = status; DifficultyClass = dc;
            FrightenedRounds = frightenedRounds; GritCost = gritCost;
            RoundsConsumed = roundsConsumed;
        }
        internal MenacingShotStatus Status { get; private set; }
        internal int DifficultyClass { get; private set; }
        internal int FrightenedRounds { get; private set; }
        internal int GritCost { get; private set; }
        internal int RoundsConsumed { get; private set; }
        internal bool ShouldApply { get { return Status == MenacingShotStatus.Eligible; } }
    }
}
