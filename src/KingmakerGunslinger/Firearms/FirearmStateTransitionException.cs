using System;

namespace KingmakerGunslinger.Firearms
{
    internal enum FirearmStateTransitionError
    {
        Unknown = 0,
        Empty = 1,
        Wrecked = 2,
        CapacityExceeded = 3,
        IncompatibleAmmunition = 4,
        MixedAmmunition = 5,
        NotBroken = 6,
        NotWrecked = 7
    }

    /// <summary>
    /// Typed rejection for a valid request shape that is not legal from the
    /// supplied immutable state.
    /// </summary>
    internal sealed class FirearmStateTransitionException : InvalidOperationException
    {
        internal FirearmStateTransitionException(
            FirearmStateTransitionError error,
            string message)
            : base(message)
        {
            if (!Enum.IsDefined(typeof(FirearmStateTransitionError), error) ||
                error == FirearmStateTransitionError.Unknown)
            {
                throw new ArgumentOutOfRangeException("error", error, "A defined transition error is required.");
            }

            Error = error;
        }

        internal FirearmStateTransitionError Error { get; private set; }
    }
}
