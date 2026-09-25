using System;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>The result of widening one performance area instance (O01).</summary>
    internal enum FavoredClassWideningOutcome
    {
        /// <summary>The ring was scaled and then the cylinder widened: range and boundary agree.</summary>
        Widened,

        /// <summary>An intentionally ringless target: only the cylinder is widened.</summary>
        WidenedRingless,

        /// <summary>A ring is expected but has not spawned yet: nothing changes until it does.</summary>
        Deferred,

        /// <summary>Scaling or widening failed: the native radius and ring are restored.</summary>
        Failed
    }

    /// <summary>
    /// O01 transaction for one area instance: the cylinder is widened only
    /// together with its ring. The ring is scaled first; the widened radius is
    /// applied only when the scaler changed at least one ring system; any
    /// exception or an empty scale restores both the ring transforms and the
    /// native radius, so a larger range is never hidden behind an unchanged
    /// ring. An intentionally ringless target (its effect link spawns nothing
    /// for any bard) widens the cylinder alone, and a ring that has not
    /// spawned yet leaves the instance native until its own spawn widens both.
    /// </summary>
    internal static class FavoredClassPerformanceWidening
    {
        internal static FavoredClassWideningOutcome Apply(float nativeRadius, float widenedRadius, bool ringExpected,
            bool ringPresent, Action<float> setRadius, Func<int> scaleRing, Action restoreRing)
        {
            if (setRadius == null) throw new ArgumentNullException("setRadius");
            if (scaleRing == null) throw new ArgumentNullException("scaleRing");
            if (restoreRing == null) throw new ArgumentNullException("restoreRing");
            if (!ringExpected)
            {
                try
                {
                    setRadius(widenedRadius);
                    return FavoredClassWideningOutcome.WidenedRingless;
                }
                catch (Exception)
                {
                    Restore(nativeRadius, setRadius, null);
                    return FavoredClassWideningOutcome.Failed;
                }
            }
            if (!ringPresent)
                return FavoredClassWideningOutcome.Deferred;
            int scaled;
            try
            {
                scaled = scaleRing();
            }
            catch (Exception)
            {
                Restore(nativeRadius, setRadius, restoreRing);
                return FavoredClassWideningOutcome.Failed;
            }
            if (scaled <= 0)
            {
                Restore(nativeRadius, setRadius, restoreRing);
                return FavoredClassWideningOutcome.Failed;
            }
            try
            {
                setRadius(widenedRadius);
                return FavoredClassWideningOutcome.Widened;
            }
            catch (Exception)
            {
                Restore(nativeRadius, setRadius, restoreRing);
                return FavoredClassWideningOutcome.Failed;
            }
        }

        private static void Restore(float nativeRadius, Action<float> setRadius, Action restoreRing)
        {
            if (restoreRing != null)
                try { restoreRing(); }
                catch (Exception) { }
            try { setRadius(nativeRadius); }
            catch (Exception) { }
        }
    }
}
