using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class StunningShotDecision
    {
        internal StunningShotDecision(StunningShotStatus status,
            bool consumeMarker, int gritCost, int difficultyClass)
        {
            if (gritCost < 0) throw new ArgumentOutOfRangeException("gritCost");
            if (status == StunningShotStatus.Applied &&
                (!consumeMarker || gritCost != 2 || difficultyClass < 1))
                throw new ArgumentException("Applied Stunning Shot is incomplete.");
            if (status != StunningShotStatus.Applied &&
                (gritCost != 0 || difficultyClass != 0))
                throw new ArgumentException("Rejected Stunning Shot cannot spend or save.");
            Status = status; ConsumeMarker = consumeMarker;
            GritCost = gritCost; DifficultyClass = difficultyClass;
        }
        internal StunningShotStatus Status { get; private set; }
        internal bool ConsumeMarker { get; private set; }
        internal int GritCost { get; private set; }
        internal int DifficultyClass { get; private set; }
        internal bool ShouldSave { get { return Status == StunningShotStatus.Applied; } }
    }
}
