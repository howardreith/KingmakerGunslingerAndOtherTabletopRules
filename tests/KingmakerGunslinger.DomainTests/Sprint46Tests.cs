using System;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void TargetingTorsoThreatRange()
        {
            var policy = new TargetingTorsoThreatService();
            Assertions.False(policy.Evaluate(true, 18, true, false).ShouldThreat,
                "Natural 18 was broadened by Targeting Torso.");
            Assertions.True(policy.Evaluate(true, 19, true, false).ShouldThreat,
                "Natural 19 did not threaten for Targeting Torso.");
            Assertions.True(policy.Evaluate(true, 20, true, false).ShouldThreat,
                "Natural 20 did not threaten for Targeting Torso.");
        }

        private static void TargetingTorsoThreatGates()
        {
            var policy = new TargetingTorsoThreatService();
            Assertions.False(policy.Evaluate(false, 19, true, false).ShouldThreat,
                "Unmarked attack received Targeting Torso threat range.");
            Assertions.False(policy.Evaluate(true, 19, false, false).ShouldThreat,
                "Miss received Targeting Torso threat.");
            Assertions.False(policy.Evaluate(true, 19, true, true).ShouldThreat,
                "Sneak-immune target received Targeting Torso threat.");
        }

        private static void TargetingTorsoThreatInvalid()
        {
            var policy = new TargetingTorsoThreatService();
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                policy.Evaluate(true, 0, true, false),
                "Natural zero was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                policy.Evaluate(true, 21, true, false),
                "Natural 21 was accepted.");
        }
    }
}
