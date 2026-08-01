using Kingmaker.UnitLogic.Buffs;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class StopBleedingResult
    {
        internal StopBleedingResult(StopBleedingDecision decision, Buff removed)
        {
            Decision = decision;
            Removed = removed;
        }

        internal StopBleedingDecision Decision { get; private set; }
        internal Buff Removed { get; private set; }
        internal bool Applied { get { return Removed != null; } }
    }
}
