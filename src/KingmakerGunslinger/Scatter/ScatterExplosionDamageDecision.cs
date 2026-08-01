using System;

namespace KingmakerGunslinger.Scatter
{
    internal sealed class ScatterExplosionDamageDecision
    {
        internal ScatterExplosionDamageDecision(bool shouldApply, int baseDamageMultiplier)
        {
            if (baseDamageMultiplier != 0 && baseDamageMultiplier != 1 &&
                baseDamageMultiplier != 3)
                throw new ArgumentOutOfRangeException("baseDamageMultiplier");
            if (shouldApply != (baseDamageMultiplier > 0))
                throw new ArgumentException("Explosion application and multiplier disagree.");
            ShouldApply = shouldApply;
            BaseDamageMultiplier = baseDamageMultiplier;
        }

        internal bool ShouldApply { get; private set; }
        internal int BaseDamageMultiplier { get; private set; }
    }
}
