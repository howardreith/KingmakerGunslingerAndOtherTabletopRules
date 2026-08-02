using System;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void StartlingShotEligible()
        {
            StartlingShotDecision result = Startling(true, FirearmCondition.Normal,
                1, 1, true);
            Assertions.Equal(StartlingShotStatus.Eligible, result.Status,
                "Eligible Startling Shot was rejected.");
            Assertions.Equal(1, result.ChamberCost,
                "Startling Shot did not consume exactly one chamber.");
            Assertions.Equal(0, result.GritCost,
                "Startling Shot incorrectly spent grit.");
            Assertions.Equal(1, result.DurationRounds,
                "Startling Shot duration changed.");
            Assertions.True(result.ShouldApply,
                "Eligible Startling Shot did not expose delivery.");
        }

        private static void StartlingShotPreconditionsAtomic()
        {
            AssertStartlingRejected(false, FirearmCondition.Normal, 1, 1, true,
                StartlingShotStatus.NotExactEquippedFirearm);
            AssertStartlingRejected(true, FirearmCondition.Wrecked, 1, 1, true,
                StartlingShotStatus.Wrecked);
            AssertStartlingRejected(true, FirearmCondition.Normal, 0, 1, true,
                StartlingShotStatus.Empty);
            AssertStartlingRejected(true, FirearmCondition.Normal, 1, 0, true,
                StartlingShotStatus.InsufficientGrit);
            AssertStartlingRejected(true, FirearmCondition.Normal, 1, 1, false,
                StartlingShotStatus.InvalidTarget);
        }

        private static void StartlingShotInvalidInputs()
        {
            var service = new StartlingShotService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null Startling Shot request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new StartlingShotRequest(true, FirearmCondition.Unknown, 1, 1, true),
                "Unknown condition was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new StartlingShotRequest(true, FirearmCondition.Normal, -1, 1, true),
                "Negative chamber count was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new StartlingShotRequest(true, FirearmCondition.Normal, 1, -1, true),
                "Negative grit was accepted.");
        }

        private static StartlingShotDecision Startling(bool exact,
            FirearmCondition condition, int loaded, int grit, bool target)
        {
            return new StartlingShotService().Evaluate(new StartlingShotRequest(
                exact, condition, loaded, grit, target));
        }

        private static void AssertStartlingRejected(bool exact,
            FirearmCondition condition, int loaded, int grit, bool target,
            StartlingShotStatus expected)
        {
            StartlingShotDecision result = Startling(exact, condition, loaded,
                grit, target);
            Assertions.Equal(expected, result.Status,
                "Startling Shot rejection changed.");
            Assertions.Equal(0, result.ChamberCost,
                "Rejected Startling Shot exposed chamber cost.");
            Assertions.Equal(0, result.GritCost,
                "Rejected Startling Shot exposed grit cost.");
            Assertions.Equal(0, result.DurationRounds,
                "Rejected Startling Shot exposed a duration.");
            Assertions.False(result.ShouldApply,
                "Rejected Startling Shot exposed delivery.");
        }
    }
}
