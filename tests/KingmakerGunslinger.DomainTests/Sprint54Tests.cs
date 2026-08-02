using System;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void MenacingShotEligibleExactValues()
        {
            MenacingShotDecision value = Menacing(15, 4, true,
                FirearmCondition.Normal, 1, 1);
            Assertions.True(value.ShouldApply, "Eligible Menacing Shot rejected.");
            Assertions.Equal(21, value.DifficultyClass, "Wisdom DC mismatch.");
            Assertions.Equal(15, value.FrightenedRounds, "Fear rank mismatch.");
            Assertions.Equal(1, value.GritCost, "Grit cost mismatch.");
            Assertions.Equal(1, value.RoundsConsumed, "Discharge mismatch.");
        }
        private static void MenacingShotLevelAndWisdomDc()
        {
            Assertions.Equal(MenacingShotStatus.BelowRequiredLevel,
                Menacing(14, 20, true, FirearmCondition.Normal, 1, 4).Status,
                "Level fourteen bypassed the deed gate.");
            Assertions.Equal(25, Menacing(20, 5, true,
                FirearmCondition.Broken, 2, 2).DifficultyClass,
                "Level twenty DC mismatch.");
        }
        private static void MenacingShotFirearmAndGritGates()
        {
            Assertions.Equal(MenacingShotStatus.NotExactEquippedFirearm,
                Menacing(15, 0, false, FirearmCondition.Normal, 1, 1).Status,
                "Ambiguous firearm was accepted.");
            Assertions.Equal(MenacingShotStatus.Wrecked,
                Menacing(15, 0, true, FirearmCondition.Wrecked, 1, 1).Status,
                "Wrecked firearm was accepted.");
            Assertions.Equal(MenacingShotStatus.Empty,
                Menacing(15, 0, true, FirearmCondition.Normal, 0, 1).Status,
                "Empty firearm was accepted.");
            Assertions.Equal(MenacingShotStatus.InsufficientGrit,
                Menacing(15, 0, true, FirearmCondition.Normal, 1, 0).Status,
                "Zero grit was accepted.");
        }
        private static void MenacingShotLivingRadiusBoundary()
        {
            Assertions.True(new MenacingShotTargetDecision(true,
                MenacingShotTargetDecision.RadiusMeters).IsAffected,
                "Exact 30-foot living target was excluded.");
            Assertions.False(new MenacingShotTargetDecision(true,
                MenacingShotTargetDecision.RadiusMeters + 0.01d).IsAffected,
                "Outside target was included.");
            Assertions.False(new MenacingShotTargetDecision(false, 0d).IsAffected,
                "Nonliving target was included.");
        }
        private static void MenacingShotRejectionsAreAtomic()
        {
            MenacingShotDecision value = Menacing(15, 3, true,
                FirearmCondition.Normal, 0, 2);
            Assertions.Equal(0, value.GritCost, "Rejected deed spent grit.");
            Assertions.Equal(0, value.RoundsConsumed,
                "Rejected deed consumed a chamber.");
        }
        private static void MenacingShotInvalidInputRejected()
        {
            var service = new MenacingShotService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new MenacingShotRequest(21, 0, true, FirearmCondition.Normal, 1, 1),
                "Invalid level was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new MenacingShotTargetDecision(true, double.NaN),
                "Invalid distance was accepted.");
        }
        private static MenacingShotDecision Menacing(int level, int wisdom,
            bool exact, FirearmCondition condition, int rounds, int grit)
        { return new MenacingShotService().Evaluate(new MenacingShotRequest(
            level, wisdom, exact, condition, rounds, grit)); }
    }
}
