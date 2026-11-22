namespace KingmakerGunslinger.Gunsmithing
{
    internal enum BatteredOriginInspection
    {
        /// <summary>The token blueprint exists; inspect the weapon's enchantments.</summary>
        Inspect = 0,

        /// <summary>
        /// Blueprint initialization is pending or failed and the token was never
        /// registered: no live item can carry it, so the weapon is not battered.
        /// </summary>
        Unavailable = 1
    }

    /// <summary>
    /// Decides whether a read-only battered-origin inspection may run. Only the
    /// explicit unavailable state (initialization not completed, or failed,
    /// with no token blueprint) is exempt; every other combination inspects and
    /// therefore keeps the strict blueprint requirement and its exception.
    /// </summary>
    internal static class BatteredFirearmOriginInspectionPolicy
    {
        internal static BatteredOriginInspection Decide(bool bootstrapInitialized,
            bool bootstrapFailed, bool tokenBlueprintAvailable)
        {
            if (tokenBlueprintAvailable) return BatteredOriginInspection.Inspect;
            // Initialized without the token is an invariant violation, not an
            // unavailable state; inspection proceeds and the requirement throws.
            if (bootstrapInitialized && !bootstrapFailed)
                return BatteredOriginInspection.Inspect;
            return BatteredOriginInspection.Unavailable;
        }
    }
}
