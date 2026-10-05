namespace KingmakerGunslinger.RuntimeTesting
{
    // Completion of the request-local drill, not production command behavior.
    internal static class CrocodilianCommandReviewPolicy
    {
        internal static bool CanFinish(bool manual, bool rejectedCastFinished,
            bool issuedAttackObserved, bool requiredCombatObserved, int framesAfterAttack)
        {
            return requiredCombatObserved && framesAfterAttack >= 90 &&
                (!manual || rejectedCastFinished && issuedAttackObserved);
        }
    }
}
