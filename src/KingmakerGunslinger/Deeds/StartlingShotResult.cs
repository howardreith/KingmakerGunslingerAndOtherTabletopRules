using Kingmaker.UnitLogic.Buffs;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class StartlingShotResult
    {
        internal StartlingShotResult(StartlingShotDecision decision, Buff buff)
        {
            Decision = decision;
            Buff = buff;
        }

        internal StartlingShotDecision Decision { get; private set; }
        internal Buff Buff { get; private set; }
    }
}
