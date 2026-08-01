using System;
using KingmakerGunslinger.Explosions;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>
    /// Selects the tabletop triple base-damage multiplier only for an exploding
    /// scatter volley whose every independent roll misfired.
    /// </summary>
    internal sealed class ScatterExplosionDamageService
    {
        internal ScatterExplosionDamageDecision Evaluate(
            FirearmDefinition definition,
            FirearmExplosionDecision explosion,
            ScatterAttackVolleyDecision volley)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (explosion == null) throw new ArgumentNullException("explosion");

            if (!explosion.RequiresBurstDamage)
                return new ScatterExplosionDamageDecision(false, 0);

            if (!definition.IsScatter)
            {
                if (volley != null)
                    throw new ArgumentException("A non-scatter explosion cannot carry scatter volley evidence.", "volley");
                return new ScatterExplosionDamageDecision(true, 1);
            }

            if (volley == null)
                throw new ArgumentNullException("volley", "A scatter explosion requires complete volley evidence.");
            if (!volley.AllRollsMisfire)
                throw new InvalidOperationException(
                    "A scatter firearm cannot explode unless every independent attack roll misfired.");
            return new ScatterExplosionDamageDecision(true, 3);
        }
    }
}
