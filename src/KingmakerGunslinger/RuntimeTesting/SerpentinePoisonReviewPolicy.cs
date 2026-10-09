namespace KingmakerGunslinger.RuntimeTesting
{
    // Observation only, never poison behavior. Native OnFactActivate consumes
    // the injury action's failed save and deals exposure one without a second
    // save. OnNewRound supplies the remaining five saves/exposures.
    internal static class SerpentinePoisonReviewPolicy
    {
        internal static bool ExactExposureCounts(int exposures, int damageEvents,
            int injurySaves, int buffSaves)
        {
            return exposures >= 1 && exposures <= 6 && damageEvents == exposures &&
                injurySaves == 1 && buffSaves == exposures - 1;
        }

        internal static bool ExactExhaustion(int damageEvents, int injurySaves,
            int buffSaves, bool buffPresent, int expiryDamage, int duplicateDamage)
        {
            return ExactExposureCounts(6, damageEvents, injurySaves, buffSaves) &&
                !buffPresent && expiryDamage == 0 && duplicateDamage == 0;
        }
    }
}
