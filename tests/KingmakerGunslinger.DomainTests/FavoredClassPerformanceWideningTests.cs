using System;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Review finding 4: the O01 cylinder is widened only together with its
    /// ring; injected failures restore both.
    /// </summary>
    internal static class FavoredClassPerformanceWideningTests
    {
        private const float Native = 15.24f;
        private const float Widened = 18.288f;

        /// <summary>A fake area instance: its radius and its ring's scale state.</summary>
        private sealed class Instance
        {
            internal float Radius = Native;
            internal bool RingScaled;
            internal int RestoreCalls;
            internal int RadiusWrites;

            internal void SetRadius(float value)
            {
                RadiusWrites++;
                Radius = value;
            }

            internal void RestoreRing()
            {
                RestoreCalls++;
                RingScaled = false;
            }
        }

        private static FavoredClassWideningOutcome Run(Instance instance, bool expected, bool present,
            Func<Instance, int> scale, Action<float> setRadius = null)
        {
            return FavoredClassPerformanceWidening.Apply(Native, Widened, expected, present,
                setRadius ?? instance.SetRadius, () => scale(instance), instance.RestoreRing);
        }

        internal static void SuccessWidensRingAndRadiusTogether()
        {
            var instance = new Instance();
            FavoredClassWideningOutcome outcome = Run(instance, true, true, value =>
            {
                Assertions.Equal(Native, value.Radius, "The ring is scaled before the cylinder is widened.");
                value.RingScaled = true;
                return 3;
            });
            Assertions.Equal(FavoredClassWideningOutcome.Widened, outcome, "Widened.");
            Assertions.True(instance.RingScaled && instance.Radius == Widened, "Ring and radius agree.");
        }

        internal static void InjectedFailuresRestoreRadiusAndRing()
        {
            // The scaler throws after changing some systems (it restores its own
            // partial scales and rethrows; the transaction restores again).
            var thrown = new Instance();
            FavoredClassWideningOutcome outcome = Run(thrown, true, true, value =>
            {
                value.RingScaled = true;
                throw new InvalidOperationException("injected ring failure");
            });
            Assertions.Equal(FavoredClassWideningOutcome.Failed, outcome, "A throwing scaler fails.");
            Assertions.True(thrown.Radius == Native && !thrown.RingScaled && thrown.RestoreCalls == 1,
                "A throwing scaler leaves the native radius and restores the ring.");
            // The scaler changes nothing (no particle system, or unavailable).
            var empty = new Instance();
            outcome = Run(empty, true, true, value => 0);
            Assertions.Equal(FavoredClassWideningOutcome.Failed, outcome, "An empty scale fails.");
            Assertions.True(empty.Radius == Native && empty.RestoreCalls == 1,
                "An empty scale never leaves the cylinder widened.");
            // Widening the cylinder throws after the ring was scaled.
            var radiusFault = new Instance();
            int attempts = 0;
            outcome = Run(radiusFault, true, true, value =>
            {
                value.RingScaled = true;
                return 2;
            }, radius =>
            {
                attempts++;
                if (radius == Widened)
                    throw new InvalidOperationException("injected radius failure");
                radiusFault.Radius = radius;
            });
            Assertions.Equal(FavoredClassWideningOutcome.Failed, outcome, "A radius failure fails.");
            Assertions.True(!radiusFault.RingScaled && radiusFault.Radius == Native && attempts == 2,
                "A radius failure restores the ring and writes the native radius back.");
            // A failing ring restore still ends with the native radius.
            var both = new Instance();
            outcome = FavoredClassPerformanceWidening.Apply(Native, Widened, true, true, both.SetRadius,
                () => { throw new InvalidOperationException("injected ring failure"); },
                () => { throw new InvalidOperationException("injected restore failure"); });
            Assertions.True(outcome == FavoredClassWideningOutcome.Failed && both.Radius == Native,
                "Even a failing restore leaves the native radius.");
        }

        internal static void RinglessAndDeferredTargetsAreDistinguished()
        {
            var ringless = new Instance();
            FavoredClassWideningOutcome outcome = Run(ringless, false, false, value =>
            {
                throw new InvalidOperationException("an intentionally ringless target never scales a ring");
            });
            Assertions.Equal(FavoredClassWideningOutcome.WidenedRingless, outcome, "Ringless target.");
            Assertions.True(ringless.Radius == Widened && ringless.RestoreCalls == 0,
                "An intentionally ringless target widens its cylinder alone.");
            var deferred = new Instance();
            outcome = Run(deferred, true, false, value =>
            {
                throw new InvalidOperationException("no ring yet");
            });
            Assertions.Equal(FavoredClassWideningOutcome.Deferred, outcome, "A ring that has not spawned yet.");
            Assertions.True(deferred.Radius == Native && deferred.RadiusWrites == 0,
                "A ring-expected instance stays native until its ring spawns.");
            // The late ring then widens both together.
            outcome = Run(deferred, true, true, value =>
            {
                value.RingScaled = true;
                return 1;
            });
            Assertions.True(outcome == FavoredClassWideningOutcome.Widened && deferred.Radius == Widened &&
                deferred.RingScaled, "The late ring widens both together.");
        }
    }
}
