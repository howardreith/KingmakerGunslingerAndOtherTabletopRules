using System;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void DeadeyeSecondIncrementCostsOne()
        {
            DeadeyeDecision result = Deadeye((20d * 0.3048d) + 0.001d, 1);
            Assertions.Equal(DeadeyeStatus.Eligible, result.Status,
                "Second-increment Deadeye was rejected.");
            Assertions.Equal(2, result.RangeIncrement, "Deadeye increment changed.");
            Assertions.Equal(1, result.GritCost, "Second increment did not cost one grit.");
            Assertions.True(result.UsesTouchArmorClass, "Deadeye did not authorize touch AC.");
        }

        private static void DeadeyeCostScalesBeyondFirst()
        {
            DeadeyeDecision result = Deadeye((60d * 0.3048d) + 0.001d, 3);
            Assertions.Equal(4, result.RangeIncrement, "Fourth increment changed.");
            Assertions.Equal(3, result.GritCost,
                "Deadeye cost was not one per increment beyond first.");
        }

        private static void DeadeyeFirstIncrementDoesNotSpend()
        {
            DeadeyeDecision result = Deadeye(20d * 0.3048d, 5);
            Assertions.Equal(DeadeyeStatus.WithinFirstIncrement, result.Status,
                "First-increment shot incorrectly activated Deadeye.");
            Assertions.Equal(0, result.GritCost, "Rejected first increment spent grit.");
        }

        private static void DeadeyeInsufficientGritFailsAtomic()
        {
            DeadeyeDecision result = Deadeye((60d * 0.3048d) + 0.001d, 2);
            Assertions.Equal(DeadeyeStatus.InsufficientGrit, result.Status,
                "Insufficient grit was accepted.");
            Assertions.Equal(0, result.GritCost,
                "Insufficient Deadeye decision exposed a partial cost.");
        }

        private static void DeadeyeContextFailsClosed()
        {
            FirearmDefinition pistol = ProductionFirearmCatalog.CreatePistol().Definition;
            var service = new DeadeyeService();
            Assertions.Equal(DeadeyeStatus.NotArmed,
                service.Evaluate(new DeadeyeRequest(false, true, 1, pistol,
                    30d * 0.3048d, 2)).Status, "Unarmed Deadeye activated.");
            Assertions.Equal(DeadeyeStatus.NotExactFirearm,
                service.Evaluate(new DeadeyeRequest(true, false, 0, pistol,
                    30d * 0.3048d, 2)).Status, "Non-firearm Deadeye activated.");
        }

        private static void DeadeyeSpecialAndInvalidRangeFailClosed()
        {
            var service = new DeadeyeService();
            Assertions.Equal(DeadeyeStatus.UnsupportedRange,
                service.Evaluate(new DeadeyeRequest(true, true, 1,
                    ProductionFirearmCatalog.CreateBlunderbuss().Definition, 10d, 5)).Status,
                "Special-range firearm guessed a Deadeye cost.");
            Assertions.Equal(DeadeyeStatus.UnsupportedRange,
                service.Evaluate(new DeadeyeRequest(true, true, 1,
                    ProductionFirearmCatalog.CreatePistol().Definition, double.NaN, 5)).Status,
                "Invalid Deadeye distance was accepted.");
        }

        private static void DeadeyeInvalidInputRejected()
        {
            var service = new DeadeyeService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null Deadeye request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new DeadeyeRequest(true, true, -1,
                    ProductionFirearmCatalog.CreatePistol().Definition, 1d, 1),
                "Negative marker count was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new DeadeyeRequest(true, true, 1,
                    ProductionFirearmCatalog.CreatePistol().Definition, 1d, -1),
                "Negative grit was accepted.");
        }

        private static DeadeyeDecision Deadeye(double distanceMeters, int grit)
        {
            return new DeadeyeService().Evaluate(new DeadeyeRequest(true, true, 1,
                ProductionFirearmCatalog.CreatePistol().Definition, distanceMeters, grit));
        }
    }
}
