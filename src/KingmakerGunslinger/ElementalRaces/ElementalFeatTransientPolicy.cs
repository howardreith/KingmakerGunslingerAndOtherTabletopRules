using System;

namespace KingmakerGunslinger.ElementalRaces
{
    internal enum ElementalFeatTransientRestoreDecision
    {
        Clear = 0,
        WaitForOwnedItems = 1,
        Restore = 2
    }

    internal static class ElementalFeatTransientPolicy
    {
        internal const int CurrentSchemaVersion = 1;

        internal static ElementalFeatTransientRestoreDecision Decide(
            bool prerequisitePresent, long endTimeTicks, long nowTicks,
            int expectedItemCount, int resolvedItemCount)
        {
            if (!prerequisitePresent || endTimeTicks <= nowTicks ||
                endTimeTicks <= 0L || expectedItemCount < 0 ||
                expectedItemCount > 2 || resolvedItemCount < 0 ||
                resolvedItemCount > expectedItemCount)
                return ElementalFeatTransientRestoreDecision.Clear;
            if (resolvedItemCount != expectedItemCount)
                return ElementalFeatTransientRestoreDecision
                    .WaitForOwnedItems;
            return ElementalFeatTransientRestoreDecision.Restore;
        }

        internal static TimeSpan Remaining(long endTimeTicks, long nowTicks)
        {
            if (endTimeTicks <= nowTicks || endTimeTicks <= 0L)
                return TimeSpan.Zero;
            try { return TimeSpan.FromTicks(endTimeTicks - nowTicks); }
            catch (OverflowException) { return TimeSpan.Zero; }
        }

        internal static bool PreserveDuringBuffTeardown(
            bool ownerIsExplicitlyDead, long persistedEndTimeTicks,
            long nowTicks)
        {
            // Kingmaker deactivates feature facts before transient buffs while
            // serializing a living unit, so prerequisite presence is not a
            // reliable teardown discriminator. Only confirmed death or exact
            // expiry owns destructive cleanup here; post-load reconciliation
            // rechecks every live prerequisite before restoring mechanics.
            return !ownerIsExplicitlyDead &&
                Remaining(persistedEndTimeTicks, nowTicks) > TimeSpan.Zero;
        }
    }
}
