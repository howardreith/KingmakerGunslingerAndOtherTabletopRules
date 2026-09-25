using System;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>What one pet-armor projection sync does (O07/O08).</summary>
    internal sealed class FavoredClassPetSyncPlan
    {
        /// <summary>Remove the projection from the previously tracked pet.</summary>
        internal bool Unproject;

        /// <summary>Add the projection to the desired pet (it has none).</summary>
        internal bool Project;

        /// <summary>The desired pet already carries it: refresh its value (never a second copy).</summary>
        internal bool Refresh;
    }

    /// <summary>
    /// O07/O08 projection decisions around one qualified desired pet: the
    /// master's current pet when it qualifies for the counter's pet class,
    /// otherwise none. A tracked pet that is no longer the desired one (it
    /// was unlinked, dismissed, transferred, replaced or stopped qualifying)
    /// loses the projection, unless another master with the same counter now
    /// holds it; a destroyed pet is simply forgotten. The desired pet carries
    /// exactly one projection.
    /// </summary>
    internal static class FavoredClassPetSync
    {
        internal static FavoredClassPetSyncPlan Plan<T>(T tracked, T desired, Func<T, bool> hasProjection,
            Func<T, bool> claimedByAnotherMaster) where T : class
        {
            if (hasProjection == null) throw new ArgumentNullException("hasProjection");
            if (claimedByAnotherMaster == null) throw new ArgumentNullException("claimedByAnotherMaster");
            var plan = new FavoredClassPetSyncPlan();
            plan.Unproject = tracked != null && !ReferenceEquals(tracked, desired) && hasProjection(tracked) &&
                !claimedByAnotherMaster(tracked);
            if (desired != null)
            {
                if (hasProjection(desired))
                    plan.Refresh = true;
                else
                    plan.Project = true;
            }
            return plan;
        }
    }
}
