using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeathsShotService
    {
        internal DeathsShotDecision Evaluate(int level, int dexterityModifier,
            int currentGrit, bool owned, bool exact, bool hit,
            bool criticalConfirmed, bool criticalImmune, bool first)
        {
            if (level < 0 || currentGrit < 0)
                throw new ArgumentOutOfRangeException("level");
            if (!owned || !exact || !first)
                return new DeathsShotDecision(false, false, 0, 0);
            if (level < 19 || !hit || !criticalConfirmed || criticalImmune ||
                currentGrit < 1)
                return new DeathsShotDecision(true, false, 0, 0);
            return new DeathsShotDecision(true, true, 1,
                10 + level / 2 + dexterityModifier);
        }
    }
}
