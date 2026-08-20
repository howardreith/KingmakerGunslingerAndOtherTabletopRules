using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Rules
{
    internal static class FirearmPenetrationRangePolicy
    {
        internal const int EarlyPenetrationIncrements = 1;
        internal const int AdvancedPenetrationIncrements = 5;

        internal static int PenetrationIncrements(FirearmDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            return definition.Era == FirearmEra.Advanced
                ? AdvancedPenetrationIncrements
                : EarlyPenetrationIncrements;
        }

        internal static double EffectivePenetrationRangeFeet(
            FirearmDefinition definition, int rangeIncrementBonusFeet)
        {
            return checked(EffectiveFirearmRangePolicy.IncrementFeet(
                definition, rangeIncrementBonusFeet) *
                PenetrationIncrements(definition));
        }

        internal static bool UsesTouchArmorClass(FirearmDefinition definition,
            int rangeIncrement, bool deadeyeAuthorized)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (rangeIncrement < 1) throw new ArgumentOutOfRangeException(
                "rangeIncrement");
            return deadeyeAuthorized ||
                rangeIncrement <= PenetrationIncrements(definition);
        }
    }
}
