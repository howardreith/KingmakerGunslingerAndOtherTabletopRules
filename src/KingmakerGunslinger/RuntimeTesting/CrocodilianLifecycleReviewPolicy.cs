namespace KingmakerGunslinger.RuntimeTesting
{
    // Only the closed request-local death drill may resume its paused clock.
    // This is not an auto-pause setting or a production lifecycle adaptation.
    internal static class CrocodilianLifecycleReviewPolicy
    {
        internal static bool CanResumeAfterRequestedDeath(bool ownedPair,
            string boundary, bool sourceDead, bool targetDead)
        {
            return ownedPair && (boundary == "source-death" && sourceDead ||
                boundary == "target-death" && targetDead);
        }
    }
}
