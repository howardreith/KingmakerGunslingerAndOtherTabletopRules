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

        internal static bool CanRetryHeldAttack(bool manualHeldCell, bool ready,
            int attempts, bool issuedAttackObserved, bool targetAlive,
            bool hasRelationship, bool riderObserved, bool commandPending)
        {
            // A missed bite emits no grapple check. The prerequisite is the
            // actual issued attack, not a check that only a hit can produce.
            return manualHeldCell && ready && attempts > 0 && attempts < 4 &&
                issuedAttackObserved && targetAlive && !hasRelationship &&
                !riderObserved && !commandPending;
        }

        internal static bool ShouldAdvanceHeldTurn(bool attackIssuedThisTurn,
            bool hasRelationship, bool riderObserved, bool commandPending, bool canEnd)
        {
            // Advance a spent attack turn or an established hold's next round.
            // Do not skip a fresh turn before its retry can be queued.
            return (attackIssuedThisTurn || hasRelationship) && !riderObserved &&
                !commandPending && canEnd;
        }

        internal static bool InjectFirstBiteMiss(string creature, string driver,
            int attempts, bool injected, bool exactOwnedPair, bool bite, bool issuedCommandActive)
        {
            // Deterministic negative control only. Never force a positive hit,
            // maneuver or rider, and never alter an AI or unrelated actor.
            return creature == "crocodile" && driver == "manual-hold" &&
                attempts == 1 && !injected && exactOwnedPair && bite && issuedCommandActive;
        }
    }
}
