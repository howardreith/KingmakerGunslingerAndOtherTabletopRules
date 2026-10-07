using System;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Interpretation of native evidence only. No production-rule changes.
    internal static class Sprint17ObservationPolicy
    {
        internal static bool NativeReach(float body, float rawSpearType, float rawTailType,
            float minimum, float computedSpearType, float computedTailType, float spear, float tail)
        {
            // BlueprintWeaponType.AttackRange already applies the native
            // four-foot allowance/floor. Only m_AttackRange is a raw input.
            return body == 5 && rawSpearType == 5 && rawTailType == 10 &&
                !float.IsNaN(minimum) && !float.IsInfinity(minimum) && minimum > 0 &&
                computedSpearType == Math.Max(minimum, rawSpearType - 4) &&
                computedTailType == Math.Max(minimum, rawTailType - 4) &&
                spear == computedSpearType && tail == computedTailType;
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
