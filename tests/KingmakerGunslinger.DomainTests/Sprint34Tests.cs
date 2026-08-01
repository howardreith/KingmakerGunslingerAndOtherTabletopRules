using System;
using KingmakerGunslinger.Classes;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void ClassChassisConstants()
        {
            Assertions.Equal(20, GunslingerClassChassis.MaximumLevel,
                "Maximum class level changed.");
            Assertions.Equal(10, GunslingerClassChassis.HitDie,
                "Gunslinger hit die changed.");
            Assertions.Equal(4, GunslingerClassChassis.SkillRanksPerLevel,
                "Skill ranks per level changed.");
        }

        private static void ClassChassisExactRows()
        {
            var chassis = new GunslingerClassChassis();
            AssertClassRow(chassis, 1, 1, 2, 2, 0);
            AssertClassRow(chassis, 5, 5, 4, 4, 1);
            AssertClassRow(chassis, 10, 10, 7, 7, 3);
            AssertClassRow(chassis, 15, 15, 9, 9, 5);
            AssertClassRow(chassis, 20, 20, 12, 12, 6);
        }

        private static void ClassChassisCompleteMonotonic()
        {
            var chassis = new GunslingerClassChassis();
            Assertions.Equal(20, chassis.Levels.Count,
                "The chassis does not contain exactly twenty rows.");
            for (int level = 1; level <= 20; level++)
            {
                GunslingerClassLevel row = chassis.Levels[level - 1];
                Assertions.Equal(level, row.Level, "Level order changed.");
                Assertions.Equal(level, row.BaseAttackBonus, "Full BAB changed.");
                if (level > 1)
                {
                    GunslingerClassLevel previous = chassis.Levels[level - 2];
                    Assertions.True(row.Fortitude >= previous.Fortitude &&
                        row.Reflex >= previous.Reflex && row.Will >= previous.Will,
                        "Base saves regressed between levels.");
                }
            }
        }

        private static void ClassChassisSaveFormulas()
        {
            for (int level = 1; level <= 20; level++)
            {
                Assertions.Equal(2 + level / 2,
                    GunslingerClassChassis.GoodSave(level), "Good-save formula changed.");
                Assertions.Equal(level / 3,
                    GunslingerClassChassis.PoorSave(level), "Poor-save formula changed.");
            }
        }

        private static void ClassChassisInvalidLevel()
        {
            var chassis = new GunslingerClassChassis();
            Assertions.Throws<ArgumentOutOfRangeException>(() => chassis.RequireLevel(0),
                "Level zero did not fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => chassis.RequireLevel(21),
                "Level twenty-one did not fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                GunslingerClassChassis.GoodSave(0), "Invalid good-save level accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                GunslingerClassChassis.PoorSave(21), "Invalid poor-save level accepted.");
        }

        private static void ClassChassisLevelValueSemantics()
        {
            GunslingerClassLevel first = new GunslingerClassChassis().RequireLevel(7);
            GunslingerClassLevel second = new GunslingerClassChassis().RequireLevel(7);
            Assertions.True(first.Equals(second), "Equivalent level rows differ.");
            Assertions.Equal(first.GetHashCode(), second.GetHashCode(),
                "Equivalent level-row hashes differ.");
            Assertions.Equal("level=7;bab=7;fort=5;ref=5;will=2", first.ToString(),
                "Deterministic level formatting changed.");
        }

        private static void AssertClassRow(GunslingerClassChassis chassis, int level,
            int bab, int fortitude, int reflex, int will)
        {
            GunslingerClassLevel row = chassis.RequireLevel(level);
            Assertions.Equal(bab, row.BaseAttackBonus, "BAB row changed.");
            Assertions.Equal(fortitude, row.Fortitude, "Fortitude row changed.");
            Assertions.Equal(reflex, row.Reflex, "Reflex row changed.");
            Assertions.Equal(will, row.Will, "Will row changed.");
        }
    }
}
