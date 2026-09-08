using System;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal interface ITeleportMishapDamageTarget
    {
        bool Living { get; }
        void SynchronizeNativeLifeState();
        void ApplyCanonicalDamage(int amount);
    }
    internal static class TeleportMishapDamagePolicy
    {
        internal static bool Apply(ITeleportMishapDamageTarget target, int amount)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (amount < 1 || amount > 10) throw new ArgumentOutOfRangeException("amount");
            if (!target.Living) return false;
            // A preceding native damage event can affect another traveler. Let the
            // native controller settle that state before selecting this packet.
            target.SynchronizeNativeLifeState();
            if (!target.Living) return false;
            target.ApplyCanonicalDamage(amount);
            // Global-map travelers are sleeping units. Normal AwakeUnits ticking
            // cannot settle a death before this synchronous spell's next reroll.
            target.SynchronizeNativeLifeState();
            return true;
        }
    }
}
