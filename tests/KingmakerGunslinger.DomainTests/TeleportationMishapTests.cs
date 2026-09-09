using System;
using System.Collections.Generic;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationMishapTests
    {
        private sealed class Target : ITeleportMishapDamageTarget
        {
            internal readonly List<string> Calls = new List<string>();
            internal bool Alive = true, PendingDeath, DieFromPacket, Unconscious, ThrowOnDamage;
            public bool Living { get { return Alive; } }
            public void SynchronizeNativeLifeState() { Calls.Add("life"); if (PendingDeath) Alive = false; }
            public void ApplyCanonicalDamage(int amount)
            { Calls.Add("damage:" + amount); if (ThrowOnDamage) throw new InvalidOperationException("native damage"); PendingDeath = DieFromPacket; }
        }
        internal static void NativeStateSurroundsEveryExactDamagePacket()
        {
            for (int amount = 1; amount <= 10; amount++)
            {
                var target = new Target();
                Assertions.True(TeleportMishapDamagePolicy.Apply(target, amount), "Living target takes the exact canonical packet.");
                Assertions.Equal("life,damage:" + amount + ",life", string.Join(",", target.Calls), "Native state is settled before eligibility and before another mishap can begin.");
            }
        }
        internal static void NewNativeDeathExcludesSubsequentMishaps()
        {
            var target = new Target { DieFromPacket = true };
            TeleportMishapDamagePolicy.Apply(target, 2);
            Assertions.False(target.Living, "The native transition takes effect synchronously.");
            Assertions.False(TeleportMishapDamagePolicy.Apply(target, 1), "A later mishap does not damage a dead unit.");
            Assertions.Equal("life,damage:2,life", string.Join(",", target.Calls), "No extra damage or life transition for the dead unit.");
        }
        internal static void PrecedingIndirectDamageCanInvalidateTheNextTarget()
        {
            var target = new Target { PendingDeath = true };
            Assertions.False(TeleportMishapDamagePolicy.Apply(target, 3), "A preceding native effect can make the current target dead.");
            Assertions.Equal("life", string.Join(",", target.Calls), "Revalidation never adds a packet to that newly dead target.");
        }
        internal static void UnconsciousLivingUnitsStillTakeEachMishap()
        {
            var target = new Target { Unconscious = true };
            Assertions.True(target.Unconscious && TeleportMishapDamagePolicy.Apply(target, 2) && TeleportMishapDamagePolicy.Apply(target, 1),
                "Native unconsciousness does not imply death or exclude a living passenger.");
            Assertions.Equal("life,damage:2,life,life,damage:1,life", string.Join(",", target.Calls), "Both packets retain native updates.");
        }
        internal static void InvalidRollsAndNativeErrorsAreNotSilentlyReplaced()
        {
            var target = new Target();
            Assertions.Throws<ArgumentOutOfRangeException>(() => TeleportMishapDamagePolicy.Apply(target, 0), "No out-of-range packet.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => TeleportMishapDamagePolicy.Apply(target, 11), "No clamping to a different roll.");
            Assertions.Equal(0, target.Calls.Count, "Invalid input causes no native operation.");
            target.ThrowOnDamage = true;
            Assertions.Throws<InvalidOperationException>(() => TeleportMishapDamagePolicy.Apply(target, 1), "Native error reaches the request's material-effect transaction guard.");
            Assertions.Equal("life,damage:1", string.Join(",", target.Calls), "No invented recovery operation follows a native failure.");
        }
    }
}
