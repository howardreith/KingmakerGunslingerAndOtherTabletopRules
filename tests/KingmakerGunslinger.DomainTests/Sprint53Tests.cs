using System;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void EvasivePositiveGritAtLevelFifteen()
        {
            EvasiveDecision value = Evasive(15, 1, false);
            Assertions.True(value.ShouldBeActive, "Eligible Evasive was inactive.");
            Assertions.True(value.StateChanges, "Activation transition was lost.");
            Assertions.Equal(3, value.NativeBenefitCount, "Benefit count mismatch.");
        }
        private static void EvasiveZeroGritRemovesBenefits()
        {
            EvasiveDecision value = Evasive(15, 0, true);
            Assertions.False(value.ShouldBeActive, "Zero grit retained Evasive.");
            Assertions.True(value.StateChanges, "Removal transition was lost.");
        }
        private static void EvasiveLevelGateAndStableState()
        {
            Assertions.False(Evasive(14, 3, false).ShouldBeActive,
                "Level fourteen received Evasive.");
            Assertions.False(Evasive(15, 3, true).StateChanges,
                "Stable active state requested duplicate grants.");
        }
        private static void EvasiveUnitStateIsIndependent()
        {
            Assertions.True(Evasive(15, 1, false).ShouldBeActive,
                "Positive-grit unit was inactive.");
            Assertions.False(Evasive(15, 0, false).ShouldBeActive,
                "Another unit's positive grit leaked into this request.");
        }
        private static void EvasiveInvalidInputRejected()
        {
            var service = new EvasiveService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new EvasiveRequest(21, 1, false), "Invalid level was accepted.");
        }
        private static EvasiveDecision Evasive(int level, int grit, bool active)
        { return new EvasiveService().Evaluate(new EvasiveRequest(level, grit, active)); }
    }
}
