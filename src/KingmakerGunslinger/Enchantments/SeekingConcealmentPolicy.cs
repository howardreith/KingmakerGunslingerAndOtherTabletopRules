namespace KingmakerGunslinger.Enchantments
{
    internal static class SeekingConcealmentPolicy
    {
        internal static bool ShouldBypass(
            bool nativeSuccess,
            bool exactParentAttack,
            bool exactStoredCheck,
            bool participantsAvailable,
            bool exactItemAuthorized)
        {
            return !nativeSuccess && exactParentAttack && exactStoredCheck &&
                participantsAvailable && exactItemAuthorized;
        }
    }
}
