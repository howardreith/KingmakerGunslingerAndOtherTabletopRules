using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Rules
{
    internal static class EffectiveFirearmRangePolicy
    {
        internal static double IncrementFeet(FirearmDefinition definition,
            int perAttackBonusFeet)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (!definition.HasFixedRangeIncrement ||
                definition.RangeIncrementFeet <= 0)
                throw new ArgumentException(
                    "The firearm has no fixed positive range increment.",
                    "definition");
            if (perAttackBonusFeet < 0)
                throw new ArgumentOutOfRangeException("perAttackBonusFeet");
            return checked(definition.RangeIncrementFeet + perAttackBonusFeet);
        }
    }
}
