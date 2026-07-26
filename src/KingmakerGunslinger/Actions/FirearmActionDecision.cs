using System;

namespace KingmakerGunslinger.Actions
{
    internal sealed class FirearmActionDecision
    {
        internal FirearmActionDecision(
            FirearmActionKind action,
            bool isAvailable,
            string reason)
        {
            if (!Enum.IsDefined(typeof(FirearmActionKind), action) ||
                action == FirearmActionKind.Unknown)
            {
                throw new ArgumentOutOfRangeException("action");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("An action decision requires a reason.", "reason");
            }

            Action = action;
            IsAvailable = isAvailable;
            Reason = reason.Trim();
        }

        internal FirearmActionKind Action { get; private set; }

        internal bool IsAvailable { get; private set; }

        internal string Reason { get; private set; }
    }
}
