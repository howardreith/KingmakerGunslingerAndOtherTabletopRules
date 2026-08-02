using Kingmaker.Controllers.Units;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Deeds
{
    public sealed class LightningReloadRoundMarker :
        OwnedGameLogicComponent<UnitDescriptor>, ITickEachRound
    {
        public void OnNewRound()
        {
            if (Owner != null && Owner.Buffs != null && Fact != null)
                Owner.Buffs.RemoveFact(Fact);
        }
    }
}
