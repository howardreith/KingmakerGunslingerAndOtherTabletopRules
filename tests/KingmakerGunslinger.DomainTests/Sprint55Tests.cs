using System;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void SlingersLuckSavingThrowCostAndSecondResult()
        {
            SlingersLuckDecision value = Luck(15, 4, true,
                SlingersLuckRollKind.SavingThrow,
                SlingersLuckRollKind.SavingThrow, true, 19, 2);
            Assertions.True(value.Applied, "Saving reroll was rejected.");
            Assertions.Equal(2, value.GritCost, "Saving cost was reducible.");
            Assertions.Equal(2, value.Result, "Lower second result was not kept.");
        }
        private static void SlingersLuckSkillCheckCostAndSecondResult()
        {
            SlingersLuckDecision value = Luck(15, 1, true,
                SlingersLuckRollKind.SkillCheck,
                SlingersLuckRollKind.SkillCheck, true, 20, 1);
            Assertions.True(value.Applied, "Skill reroll was rejected.");
            Assertions.Equal(1, value.GritCost, "Skill cost mismatch.");
            Assertions.Equal(1, value.Result, "Mandatory second roll was lost.");
        }
        private static void SlingersLuckKindAndLevelGates()
        {
            Assertions.Equal(SlingersLuckStatus.WrongKind, Luck(15, 4, true,
                SlingersLuckRollKind.SavingThrow,
                SlingersLuckRollKind.SkillCheck, true, 5, 15).Status,
                "Wrong event kind consumed the marker.");
            Assertions.Equal(SlingersLuckStatus.LevelTooLow, Luck(14, 4, true,
                SlingersLuckRollKind.SkillCheck,
                SlingersLuckRollKind.SkillCheck, true, 5, 15).Status,
                "Level fourteen used the deed.");
        }
        private static void SlingersLuckGritGatesAreFixed()
        {
            Assertions.Equal(SlingersLuckStatus.InsufficientGrit,
                Luck(15, 1, true, SlingersLuckRollKind.SavingThrow,
                    SlingersLuckRollKind.SavingThrow, true, 5, 15).Status,
                "One grit paid the fixed saving cost.");
            Assertions.Equal(SlingersLuckStatus.InsufficientGrit,
                Luck(15, 0, true, SlingersLuckRollKind.SkillCheck,
                    SlingersLuckRollKind.SkillCheck, true, 5, 15).Status,
                "Zero grit paid the fixed skill cost.");
        }
        private static void SlingersLuckMarkerAndDuplicateGates()
        {
            Assertions.Equal(SlingersLuckStatus.NotArmed, Luck(15, 4, false,
                SlingersLuckRollKind.SkillCheck,
                SlingersLuckRollKind.SkillCheck, true, 5, 15).Status,
                "Unarmed roll was replaced.");
            Assertions.Equal(SlingersLuckStatus.Duplicate, Luck(15, 4, true,
                SlingersLuckRollKind.SkillCheck,
                SlingersLuckRollKind.SkillCheck, false, 5, 15).Status,
                "Duplicate callback rerolled again.");
        }
        private static void SlingersLuckInvalidInputRejected()
        {
            var service = new SlingersLuckService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new SlingersLuckRequest(15, 1, true,
                    (SlingersLuckRollKind)99, SlingersLuckRollKind.SkillCheck,
                    true, 1, 2), "Invalid kind was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new SlingersLuckRequest(15, 1, true,
                    SlingersLuckRollKind.SkillCheck,
                    SlingersLuckRollKind.SkillCheck, true, 0, 2),
                "Invalid d20 was accepted.");
        }
        private static SlingersLuckDecision Luck(int level, int grit, bool armed,
            SlingersLuckRollKind armedKind, SlingersLuckRollKind eventKind,
            bool first, int firstRoll, int secondRoll)
        { return new SlingersLuckService().Evaluate(new SlingersLuckRequest(level,
            grit, armed, armedKind, eventKind, first, firstRoll, secondRoll)); }
    }
}
