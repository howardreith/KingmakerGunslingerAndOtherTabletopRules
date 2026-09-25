using System.Collections.Generic;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>One live area instance of an owner's performance: its widening outcome and range.</summary>
    internal struct FavoredClassLiveRange
    {
        internal FavoredClassLiveRange(FavoredClassWideningOutcome outcome, int feet)
        {
            Outcome = outcome;
            Feet = feet;
        }

        internal FavoredClassWideningOutcome Outcome { get; private set; }

        /// <summary>The range the instance was widened to, in feet (meaningful once widened).</summary>
        internal int Feet { get; private set; }
    }

    /// <summary>
    /// O01 displayed range: an owner's performance descriptions follow the
    /// actual widening outcome of that owner's own live areas of the target,
    /// never the earned steps alone. A live area that failed to widen or is
    /// still waiting for its ring keeps every description native; widened
    /// live areas show the range they actually have. With no live area the
    /// owner's configured range is shown, unless the owner's last completed
    /// widening of that target failed: then the descriptions stay native
    /// until a later widening succeeds.
    /// </summary>
    internal static class FavoredClassRangePresentation
    {
        /// <summary>The range the owner's descriptions show, in feet, or null for the native text.</summary>
        internal static int? Feet(IList<FavoredClassLiveRange> live, FavoredClassWideningOutcome? lastCompleted,
            int configuredFeet)
        {
            if (live != null && live.Count > 0)
            {
                int feet = configuredFeet;
                foreach (FavoredClassLiveRange instance in live)
                {
                    if (!IsWidened(instance.Outcome))
                        return null;
                    // The most recent live instance decides the range shown.
                    feet = instance.Feet;
                }
                return feet;
            }
            return lastCompleted == FavoredClassWideningOutcome.Failed ? (int?)null : configuredFeet;
        }

        internal static bool IsWidened(FavoredClassWideningOutcome outcome)
        {
            return outcome == FavoredClassWideningOutcome.Widened ||
                outcome == FavoredClassWideningOutcome.WidenedRingless;
        }

        /// <summary>A deferral is not a completed widening: it never changes the last completed outcome.</summary>
        internal static FavoredClassWideningOutcome? NextLastCompleted(FavoredClassWideningOutcome? previous,
            FavoredClassWideningOutcome outcome)
        {
            return outcome == FavoredClassWideningOutcome.Deferred ? previous : outcome;
        }
    }
}
