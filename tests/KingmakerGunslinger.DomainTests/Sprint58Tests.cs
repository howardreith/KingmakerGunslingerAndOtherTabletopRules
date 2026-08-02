using System;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void StunningShotEligibleHit()
        {
            StunningShotDecision value = Stun(19, 4, 3, true, true, true, false, true);
            Assertions.Equal(StunningShotStatus.Applied, value.Status, "Eligible hit rejected.");
            Assertions.Equal(2, value.GritCost, "Stunning Shot cost changed.");
            Assertions.Equal(23, value.DifficultyClass, "Stunning Shot DC changed.");
            Assertions.True(value.ConsumeMarker, "Eligible marker was not consumed.");
        }

        private static void StunningShotMissAndImmunity()
        {
            Assertions.Equal(StunningShotStatus.Miss,
                Stun(19, 4, 3, true, true, false, false, true).Status,
                "Miss applied Stunning Shot.");
            Assertions.Equal(StunningShotStatus.CriticalImmune,
                Stun(19, 4, 3, true, true, true, true, true).Status,
                "Critical immunity was bypassed.");
        }

        private static void StunningShotResourceAndLevelGates()
        {
            Assertions.Equal(StunningShotStatus.LevelTooLow,
                Stun(18, 4, 3, true, true, true, false, true).Status,
                "Level 18 accepted.");
            Assertions.Equal(StunningShotStatus.InsufficientGrit,
                Stun(19, 4, 1, true, true, true, false, true).Status,
                "One grit accepted.");
        }

        private static void StunningShotWeaponAndOwnerIsolation()
        {
            StunningShotDecision weapon = Stun(19, 4, 3, false, true, true, false, true);
            Assertions.Equal(StunningShotStatus.WrongWeapon, weapon.Status,
                "Non-firearm accepted.");
            Assertions.True(!weapon.ConsumeMarker, "Non-firearm consumed marker.");
            Assertions.Equal(StunningShotStatus.WrongOwner,
                Stun(19, 4, 3, true, false, true, false, true).Status,
                "Other owner accepted.");
        }

        private static void StunningShotDuplicateGate()
        {
            StunningShotDecision value = Stun(19, 4, 3, true, true, true, false, false);
            Assertions.Equal(StunningShotStatus.Duplicate, value.Status,
                "Duplicate accepted.");
            Assertions.True(!value.ConsumeMarker, "Duplicate consumed marker.");
        }

        private static void StunningShotInvalidInput()
        {
            var service = new StunningShotService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null request accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new StunningShotRequest(-1, 0, 0, true, true, true, false, true),
                "Negative level accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new StunningShotRequest(19, 0, -1, true, true, true, false, true),
                "Negative grit accepted.");
        }

        private static StunningShotDecision Stun(int level, int wisdom, int grit,
            bool exact, bool owned, bool hit, bool immune, bool first)
        { return new StunningShotService().Evaluate(new StunningShotRequest(level,
            wisdom, grit, exact, owned, hit, immune, first)); }
    }
}
