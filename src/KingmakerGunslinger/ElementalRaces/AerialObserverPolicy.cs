using System;

namespace KingmakerGunslinger.ElementalRaces
{
    // No state, blueprint, publication or altitude/presentation inference.
    internal static class AerialObserverPolicy
    {
        internal static bool ExactFlight(object canonicalCarrier, object activeCarrier,
            bool active, bool suppressed, bool exactContract)
        {
            return exactContract && canonicalCarrier != null &&
                ReferenceEquals(canonicalCarrier, activeCarrier) && active && !suppressed;
        }

        internal static int Bonus(bool foundationPresent, bool exactFlight,
            bool perception, bool exactContract)
        {
            return foundationPresent && exactFlight && perception && exactContract ? 2 : 0;
        }
    }
}
