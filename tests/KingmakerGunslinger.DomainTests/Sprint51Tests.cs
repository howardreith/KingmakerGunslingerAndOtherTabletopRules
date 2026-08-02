using System;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void ExpertLoadingSuppressesBrokenMisfire()
        {
            ExpertLoadingDecision value = Evaluate(true, true, true, true,
                true, 1);
            Assertions.True(value.ConsumeMarker, "Exact attack did not consume marker.");
            Assertions.True(value.SuppressExplosion, "Broken misfire was not suppressed.");
            Assertions.Equal(1, value.GritCost, "Suppression cost mismatch.");
        }

        private static void ExpertLoadingInsufficientGritFailsClosed()
        {
            ExpertLoadingDecision value = Evaluate(true, true, true, true,
                true, 0);
            Assertions.True(value.ConsumeMarker, "Exact attack did not consume marker.");
            Assertions.False(value.SuppressExplosion, "No-grit event was suppressed.");
            Assertions.Equal(0, value.GritCost, "Rejected suppression spent grit.");
        }

        private static void ExpertLoadingGatesAreExact()
        {
            Assertions.False(Evaluate(false, true, true, true,
                true, 1).ConsumeMarker,
                "Non-firearm action consumed marker.");
            Assertions.False(Evaluate(true, false, true, true,
                true, 1).ConsumeMarker,
                "Ineligible attack consumed marker.");
            Assertions.False(Evaluate(true, true, false, true,
                true, 1).ConsumeMarker,
                "Duplicate evaluation consumed marker.");
            Assertions.False(Evaluate(true, true, true, false,
                true, 1).SuppressExplosion,
                "Ordinary roll was suppressed.");
            Assertions.False(Evaluate(true, true, true, true,
                false, 1).SuppressExplosion,
                "Normal firearm damage was suppressed.");
        }

        private static void ExpertLoadingInvalidInputFailsClosed()
        {
            var service = new ExpertLoadingService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new ExpertLoadingRequest(true, true, true, true,
                    false, -1), "Negative grit was accepted.");
        }

        private static ExpertLoadingDecision Evaluate(bool exact, bool eligible,
            bool first, bool misfire, bool wouldExplode, int grit)
        {
            return new ExpertLoadingService().Evaluate(new ExpertLoadingRequest(
                exact, eligible, first, misfire, wouldExplode, grit));
        }
    }
}
