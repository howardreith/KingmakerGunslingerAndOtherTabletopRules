using System;
using System.Threading;

namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// Process-local single-slot diagnostic queue. Zero is reserved as the empty
    /// sentinel, so only valid d20 values can ever be published or consumed.
    /// Native weapons and rejected firearm attacks never touch this queue.
    /// </summary>
    internal sealed class ForcedNaturalRollQueue
    {
        private int _pending;

        internal int? Pending
        {
            get
            {
                int value = Interlocked.CompareExchange(ref _pending, 0, 0);
                return value == 0 ? (int?)null : value;
            }
        }

        internal int? Set(int naturalRoll)
        {
            Validate(naturalRoll);
            int previous = Interlocked.Exchange(ref _pending, naturalRoll);
            return previous == 0 ? (int?)null : previous;
        }

        internal bool TryConsume(out int naturalRoll)
        {
            naturalRoll = Interlocked.Exchange(ref _pending, 0);
            return naturalRoll != 0;
        }

        internal int? Cancel()
        {
            int previous = Interlocked.Exchange(ref _pending, 0);
            return previous == 0 ? (int?)null : previous;
        }

        private static void Validate(int naturalRoll)
        {
            if (naturalRoll < 1 || naturalRoll > 20)
            {
                throw new ArgumentOutOfRangeException(
                    "naturalRoll",
                    naturalRoll,
                    "A forced natural d20 must be in the range 1..20.");
            }
        }
    }
}
