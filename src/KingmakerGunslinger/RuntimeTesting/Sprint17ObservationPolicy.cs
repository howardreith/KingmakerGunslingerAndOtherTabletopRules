using System;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Interpretation of native evidence only. No production-rule changes.
    internal static class Sprint17ObservationPolicy
    {
        internal static bool NativeReach(float body, float spearType, float tailType,
            float minimum, float spear, float tail)
        {
            return body == 5 && spearType == 5 && tailType == 10 &&
                !float.IsNaN(minimum) && !float.IsInfinity(minimum) && minimum > 0 &&
                spear == Math.Max(minimum, spearType - 4) &&
                tail == Math.Max(minimum, tailType - 4);
        }

        internal static bool ResolvedStrike(bool expectedMiss, bool hit,
            bool resolvedResults, int damageEvents)
        {
            return expectedMiss ? !hit && !resolvedResults && damageEvents == 0 :
                hit && resolvedResults && damageEvents == 1;
        }

        internal static bool FireImmunity(bool packetImmune, int deliveredDamage)
        { return packetImmune && deliveredDamage == 0; }

        internal static bool MagicReduction(int expectedEnhancement, bool exactWeapon,
            int enhancement, int rolled, int reduction, int final)
        {
            return (expectedEnhancement == 0 || expectedEnhancement == 1) &&
                exactWeapon && enhancement == expectedEnhancement && rolled >= 15 &&
                reduction == (expectedEnhancement == 0 ? 10 : 0) &&
                final == rolled - reduction;
        }

        internal static bool NativeExpiry(bool exactControl, bool effectAvailable,
            bool nativeFade, bool originalFade, bool despawned, bool resourcesGone)
        {
            // Absence is acceptable only with a matched native control and
            // a recorded missing donor effect, never on an unexplained failure.
            return exactControl && despawned && resourcesGone &&
                (effectAvailable ? nativeFade && originalFade : nativeFade == originalFade);
        }
    }
}
