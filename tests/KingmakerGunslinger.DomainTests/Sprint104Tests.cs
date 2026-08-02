using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void TargetingArmsEligibleRider()
        {
            TargetingArmsRiderDecision decision =
                new TargetingArmsRiderService().Evaluate(true, false);
            Assertions.True(decision.ShouldDisableMainHand,
                "An eligible hit must disable the main-hand item.");
        }

        private static void TargetingArmsRiderGates()
        {
            var service = new TargetingArmsRiderService();
            Assertions.False(service.Evaluate(false, false).ShouldDisableMainHand,
                "A miss must not disable an item.");
            Assertions.False(service.Evaluate(true, true).ShouldDisableMainHand,
                "Sneak-attack immunity must suppress the arms rider.");
        }
    }
}
