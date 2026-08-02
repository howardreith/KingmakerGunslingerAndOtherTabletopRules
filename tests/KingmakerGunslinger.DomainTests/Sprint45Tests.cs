using System;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void TargetingHeadEligible()
        {
            TargetingHeadDecision result = TargetingHead(true,
                FirearmCondition.Normal, 1, 1, true);
            Assertions.Equal(TargetingHeadStatus.Accepted, result.Status,
                "Eligible Targeting Head was rejected.");
            Assertions.True(result.ShouldAttack, "Eligible Head attack omitted.");
            Assertions.Equal(1, result.GritCost, "Head grit cost changed.");
        }

        private static void TargetingHeadPreconditions()
        {
            AssertTargetingHead(false, FirearmCondition.Normal, 1, 1, true,
                TargetingHeadStatus.NoExactFirearm);
            AssertTargetingHead(true, FirearmCondition.Normal, 0, 1, true,
                TargetingHeadStatus.Empty);
            AssertTargetingHead(true, FirearmCondition.Wrecked, 1, 1, true,
                TargetingHeadStatus.Wrecked);
            AssertTargetingHead(true, FirearmCondition.Broken, 1, 0, true,
                TargetingHeadStatus.InsufficientGrit);
            AssertTargetingHead(true, FirearmCondition.Normal, 1, 1, false,
                TargetingHeadStatus.InvalidTarget);
        }

        private static void TargetingHeadHitRider()
        {
            TargetingHeadRiderDecision result = new TargetingHeadService()
                .EvaluateRider(true, false);
            Assertions.True(result.ShouldConfuse, "Eligible Head rider omitted.");
            Assertions.Equal(1, result.DurationRounds, "Head duration changed.");
        }

        private static void TargetingHeadRiderGates()
        {
            Assertions.False(new TargetingHeadService().EvaluateRider(false, false)
                .ShouldConfuse, "Miss applied Head rider.");
            Assertions.False(new TargetingHeadService().EvaluateRider(true, true)
                .ShouldConfuse, "Sneak-immune target received Head rider.");
        }

        private static void TargetingHeadInvalid()
        {
            Assertions.Throws<ArgumentNullException>(() =>
                new TargetingHeadService().Evaluate(null), "Null request accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new TargetingHeadRequest(true, FirearmCondition.Unknown, 1, 1, true),
                "Unknown condition accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new TargetingHeadRequest(true, FirearmCondition.Normal, -1, 1, true),
                "Negative rounds accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new TargetingHeadRequest(true, FirearmCondition.Normal, 1, -1, true),
                "Negative grit accepted.");
        }

        private static TargetingHeadDecision TargetingHead(bool exact,
            FirearmCondition condition, int rounds, int grit, bool target)
        {
            return new TargetingHeadService().Evaluate(new TargetingHeadRequest(
                exact, condition, rounds, grit, target));
        }

        private static void AssertTargetingHead(bool exact,
            FirearmCondition condition, int rounds, int grit, bool target,
            TargetingHeadStatus expected)
        {
            TargetingHeadDecision result = TargetingHead(exact, condition,
                rounds, grit, target);
            Assertions.Equal(expected, result.Status, "Head rejection changed.");
            Assertions.False(result.ShouldAttack, "Rejected Head attack exposed.");
            Assertions.Equal(0, result.GritCost, "Rejected Head exposed grit cost.");
        }
    }
}
