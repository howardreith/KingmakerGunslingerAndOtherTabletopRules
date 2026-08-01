using System;
using KingmakerGunslinger.Grit;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void GritMaximumWisdomMinimum()
        {
            var service = new GritPoolService();
            Assertions.Equal(1, service.CalculateMaximum(-3, 0), "Negative Wisdom changed.");
            Assertions.Equal(1, service.CalculateMaximum(0, 0), "Zero Wisdom changed.");
            Assertions.Equal(4, service.CalculateMaximum(4, 0), "Positive Wisdom changed.");
            Assertions.Equal(6, service.CalculateMaximum(4, 2), "Bonus maximum changed.");
        }

        private static void GritDailyResetExact()
        {
            Assertions.Equal(new GritPoolState(3, 3),
                new GritPoolService().ResetDaily(3, 0), "Daily reset changed.");
        }

        private static void GritStateRejectsInvalidBounds()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() => new GritPoolState(0, 0),
                "Zero maximum was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => new GritPoolState(-1, 1),
                "Negative current was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => new GritPoolState(2, 1),
                "Over-maximum current was accepted.");
        }

        private static void GritReconcileClampsWithoutRefill()
        {
            var service = new GritPoolService();
            Assertions.Equal(new GritPoolState(2, 5),
                service.ReconcileMaximum(new GritPoolState(2, 3), 5, 0),
                "Increasing maximum refilled grit.");
            Assertions.Equal(new GritPoolState(2, 2),
                service.ReconcileMaximum(new GritPoolState(3, 3), 2, 0),
                "Decreasing maximum did not clamp grit.");
        }

        private static void GritSpendApplied()
        {
            var result = new GritPoolService().Spend(new GritPoolState(3, 3), 2,
                "deed-1", new GritOperationGate());
            Assertions.Equal(GritTransactionStatus.Applied, result.Status,
                "Valid spend was rejected.");
            Assertions.Equal(new GritPoolState(1, 3), result.After,
                "Valid spend delta changed.");
        }

        private static void GritSpendInsufficientAtomic()
        {
            var state = new GritPoolState(1, 3);
            var result = new GritPoolService().Spend(state, 2, "deed-2",
                new GritOperationGate());
            Assertions.Equal(GritTransactionStatus.Insufficient, result.Status,
                "Insufficient spend status changed.");
            Assertions.True(ReferenceEquals(state, result.After),
                "Insufficient spend mutated state.");
        }

        private static void GritRestoreAppliedAndCapped()
        {
            var result = new GritPoolService().Restore(new GritPoolState(1, 3), 5,
                "critical-1", new GritOperationGate());
            Assertions.Equal(GritTransactionStatus.Applied, result.Status,
                "Restore was rejected.");
            Assertions.Equal(new GritPoolState(3, 3), result.After,
                "Restore did not clamp at maximum.");
            var huge = new GritPoolService().Restore(new GritPoolState(0, 3),
                int.MaxValue, "critical-huge", new GritOperationGate());
            Assertions.Equal(new GritPoolState(3, 3), huge.After,
                "Large restore overflowed instead of clamping.");
        }

        private static void GritRestoreAtMaximumAtomic()
        {
            var state = new GritPoolState(3, 3);
            var result = new GritPoolService().Restore(state, 1, "critical-2",
                new GritOperationGate());
            Assertions.Equal(GritTransactionStatus.AtMaximum, result.Status,
                "At-maximum restore status changed.");
            Assertions.True(ReferenceEquals(state, result.After),
                "At-maximum restore mutated state.");
        }

        private static void GritDuplicateSpendRejected()
        {
            var gate = new GritOperationGate();
            var service = new GritPoolService();
            var first = service.Spend(new GritPoolState(3, 3), 1, "deed-3", gate);
            var second = service.Spend(first.After, 1, "deed-3", gate);
            Assertions.Equal(GritTransactionStatus.Duplicate, second.Status,
                "Duplicate spend was applied.");
            Assertions.Equal(first.After, second.After, "Duplicate spend changed state.");
        }

        private static void GritDuplicateRestoreRejected()
        {
            var gate = new GritOperationGate();
            var service = new GritPoolService();
            var first = service.Restore(new GritPoolState(0, 3), 1, "kill-1", gate);
            var second = service.Restore(first.After, 1, "kill-1", gate);
            Assertions.Equal(GritTransactionStatus.Duplicate, second.Status,
                "Duplicate restore was applied.");
            Assertions.Equal(first.After, second.After, "Duplicate restore changed state.");
        }

        private static void GritUnitGatesAreIsolated()
        {
            var service = new GritPoolService();
            var left = service.Spend(new GritPoolState(2, 2), 1, "same-event",
                new GritOperationGate());
            var right = service.Spend(new GritPoolState(2, 2), 1, "same-event",
                new GritOperationGate());
            Assertions.Equal(GritTransactionStatus.Applied, left.Status,
                "Left unit spend failed.");
            Assertions.Equal(GritTransactionStatus.Applied, right.Status,
                "Operation identity leaked between units.");
        }

        private static void GritInvalidTransactionsRejected()
        {
            var service = new GritPoolService();
            var state = new GritPoolState(1, 1);
            var gate = new GritOperationGate();
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                service.Spend(state, 0, "bad", gate), "Zero spend was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                service.Restore(state, -1, "bad", gate), "Negative restore was accepted.");
            Assertions.Throws<ArgumentException>(() =>
                service.Spend(state, 1, " ", gate), "Blank operation identity was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                service.CalculateMaximum(1, -1), "Negative maximum bonus was accepted.");
        }
    }
}
