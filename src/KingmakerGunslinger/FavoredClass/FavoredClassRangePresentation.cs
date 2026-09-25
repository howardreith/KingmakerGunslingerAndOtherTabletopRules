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
    /// live areas of that owner's performance (FavoredClassRangeGroup keeps
    /// them all widened or all native). While any live area is native the
    /// descriptions are native; while every live area is widened they show
    /// the range those areas actually have; with no live area they show the
    /// owner's configured range. No outcome is remembered after its area
    /// ends, so nothing survives a save load, a new game or a respec.
    /// </summary>
    internal static class FavoredClassRangePresentation
    {
        /// <summary>The range the owner's descriptions show, in feet, or null for the native text.</summary>
        internal static int? Feet(IList<FavoredClassLiveRange> live, int? configuredFeet)
        {
            if (live == null || live.Count == 0)
                return configuredFeet;
            int? feet = null;
            foreach (FavoredClassLiveRange instance in live)
            {
                if (!IsWidened(instance.Outcome))
                    return null;
                // The most recent live instance decides the range shown.
                feet = instance.Feet;
            }
            return feet;
        }

        internal static bool IsWidened(FavoredClassWideningOutcome outcome)
        {
            return outcome == FavoredClassWideningOutcome.Widened ||
                outcome == FavoredClassWideningOutcome.WidenedRingless;
        }
    }
}
