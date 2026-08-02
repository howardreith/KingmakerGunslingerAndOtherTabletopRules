using System;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void CheatDeathLethalApplies()
        {
            CheatDeathDecision value = Cheat(19, 3, 0, true, true);
            Assertions.Equal(CheatDeathStatus.Applied, value.Status,
                "Lethal damage did not trigger Cheat Death.");
            Assertions.Equal(3, value.GritCost, "Cheat Death did not spend all grit.");
            Assertions.Equal(1, value.FinalHitPoints, "Cheat Death did not leave 1 HP.");
        }

        private static void CheatDeathAllGritCosts()
        {
            Assertions.Equal(1, Cheat(19, 1, -20, true, true).GritCost,
                "Minimum nonzero grit cost changed.");
            Assertions.Equal(7, Cheat(20, 7, 0, true, true).GritCost,
                "Full current grit cost changed.");
        }

        private static void CheatDeathPositiveHitPointsRejected()
        { Assertions.Equal(CheatDeathStatus.NotLethal,
            Cheat(19, 4, 1, true, true).Status,
            "Positive final HP triggered Cheat Death."); }

        private static void CheatDeathResourceAndLevelGates()
        {
            Assertions.Equal(CheatDeathStatus.InsufficientGrit,
                Cheat(19, 0, 0, true, true).Status, "Zero grit was accepted.");
            Assertions.Equal(CheatDeathStatus.LevelTooLow,
                Cheat(18, 3, 0, true, true).Status, "Level 18 was accepted.");
        }

        private static void CheatDeathTargetAndDuplicateGates()
        {
            Assertions.Equal(CheatDeathStatus.WrongTarget,
                Cheat(19, 3, 0, false, true).Status, "Other target was accepted.");
            Assertions.Equal(CheatDeathStatus.Duplicate,
                Cheat(19, 3, 0, true, false).Status, "Duplicate was accepted.");
        }

        private static void CheatDeathInvalidInputRejected()
        {
            var service = new CheatDeathService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new CheatDeathRequest(-1, 1, 0, 1, true, true),
                "Negative level was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new CheatDeathRequest(19, -1, 0, 1, true, true),
                "Negative grit was accepted.");
        }

        private static CheatDeathDecision Cheat(int level, int grit, int hp,
            bool ownsTarget, bool first)
        { return new CheatDeathService().Evaluate(new CheatDeathRequest(level,
            grit, hp, 1, ownsTarget, first)); }
    }
}
