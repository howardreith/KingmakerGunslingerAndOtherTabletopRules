using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>Fail-closed boundary for the native cone API's caller-supplied distance.</summary>
    internal sealed class ScatterConeDistanceService
    {
        internal const int BlunderbussConeDistanceFeet = 15;
        private const float MetersPerFoot = 0.3048f;

        internal ScatterConeDistanceDecision ResolveBlunderbuss(FirearmDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (definition.Kind != FirearmKind.Blunderbuss || !definition.IsScatter)
                throw new ArgumentException(
                    "The tabletop 15-foot cone authority applies only to a scatter Blunderbuss.",
                    "definition");
            return Resolve(definition, BlunderbussConeDistanceFeet);
        }

        internal ScatterConeDistanceDecision Resolve(FirearmDefinition definition, int? authorizedDistanceFeet)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (!definition.IsScatter)
                throw new ArgumentException("Cone distance applies only to scatter firearms.", "definition");
            if (!authorizedDistanceFeet.HasValue)
                throw new InvalidOperationException("Scatter cone distance has no project authority; native delivery is unavailable.");

            int feet = authorizedDistanceFeet.Value;
            if (feet < FirearmDefinition.MinimumRangeIncrementFeet || feet > FirearmDefinition.MaximumRangeIncrementFeet)
                throw new ArgumentOutOfRangeException("authorizedDistanceFeet");
            if (feet % 5 != 0)
                throw new ArgumentException("Authorized scatter distance must use a five-foot step.", "authorizedDistanceFeet");
            return new ScatterConeDistanceDecision(feet, feet * MetersPerFoot);
        }
    }
}
