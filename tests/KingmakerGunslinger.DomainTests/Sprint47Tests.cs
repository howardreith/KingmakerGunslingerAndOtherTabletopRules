using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void TargetingLegsEligibleRider()
        {
            TargetingLegsRiderDecision result =
                new TargetingLegsRiderService().Evaluate(true, false, false);
            Assertions.True(result.ShouldTrip,
                "Eligible Targeting Legs rider was omitted.");
        }

        private static void TargetingLegsRiderGates()
        {
            var policy = new TargetingLegsRiderService();
            Assertions.False(policy.Evaluate(false, false, false).ShouldTrip,
                "Miss dispatched Targeting Legs Trip.");
            Assertions.False(policy.Evaluate(true, true, false).ShouldTrip,
                "Sneak-immune target received Targeting Legs Trip.");
            Assertions.False(policy.Evaluate(true, false, true).ShouldTrip,
                "Trip-immune target received Targeting Legs Trip.");
        }

        private static void TargetingLegsRiderObservations()
        {
            TargetingLegsRiderDecision result =
                new TargetingLegsRiderService().Evaluate(true, true, true);
            Assertions.True(result.Hit, "Hit observation changed.");
            Assertions.True(result.ImmuneToSneakAttack,
                "Sneak immunity observation changed.");
            Assertions.True(result.ImmuneToTrip,
                "Trip immunity observation changed.");
        }
    }
}
