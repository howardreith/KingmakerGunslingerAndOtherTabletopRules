using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingTorsoThreatService
    {
        internal TargetingTorsoThreatDecision Evaluate(bool marked,
            int naturalRoll, bool hit, bool immuneToSneakAttack)
        {
            if (naturalRoll < 1 || naturalRoll > 20)
                throw new ArgumentOutOfRangeException("naturalRoll");
            return new TargetingTorsoThreatDecision(marked, naturalRoll, hit,
                immuneToSneakAttack);
        }
    }
}
