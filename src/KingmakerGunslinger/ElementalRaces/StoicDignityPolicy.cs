using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    // Tokens are canonical blueprint OBJECT identities. Names, descriptors and
    // caster identity are deliberately not inputs to effect correlation.
    internal sealed class StoicEffectIdentity
    {
        internal readonly object Effect;
        internal readonly object Ability;
        internal readonly object[] AbilityParents;
        internal readonly bool Ambiguous;
        internal StoicEffectIdentity(object effect, object ability,
            IEnumerable<object> parents = null, bool ambiguous = false)
        { Effect = effect; Ability = ability; AbilityParents = (parents ?? Enumerable.Empty<object>()).ToArray(); Ambiguous = ambiguous; }
    }

    internal static class StoicDignityPolicy
    {
        // Match the exact native Feet.Meters float boundary before promotion.
        internal const float RadiusMetres = 3.048f;
        internal static bool SameEffect(StoicEffectIdentity incoming, StoicEffectIdentity existing)
        {
            if (incoming == null || existing == null || incoming.Ambiguous || existing.Ambiguous) return false;
            // Two known distinct effect facts are stronger evidence than a
            // shared granting ability. Do not suppress a different buff from
            // a multi-effect ability merely because its ability matches.
            if (incoming.Effect != null && existing.Effect != null)
                return ReferenceEquals(incoming.Effect, existing.Effect);
            if (incoming.Ability == null || existing.Ability == null) return false;
            return ReferenceEquals(incoming.Ability, existing.Ability) ||
                incoming.AbilityParents.Any(p => ReferenceEquals(p, existing.Ability)) ||
                existing.AbilityParents.Any(p => ReferenceEquals(p, incoming.Ability));
            // Siblings sharing only a generic ability parent do NOT match.
        }
        internal static bool Eligible(bool present, bool conscious, bool dead,
            bool mindAffecting, bool allyRecipient, bool self, bool ally,
            double distanceMetres, bool provenSameEffect, bool exactContract)
        {
            if (!present || !exactContract || !conscious || dead || !mindAffecting || provenSameEffect) return false;
            if (!allyRecipient) return self;
            return !self && ally && !double.IsNaN(distanceMetres) && !double.IsInfinity(distanceMetres) &&
                distanceMetres >= 0 && distanceMetres <= RadiusMetres;
        }
    }
}
