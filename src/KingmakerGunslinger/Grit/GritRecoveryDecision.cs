using System;

namespace KingmakerGunslinger.Grit
{
    internal sealed class GritRecoveryDecision
    {
        internal GritRecoveryDecision(GritRecoveryEventKind eventKind,
            GritRecoveryStatus status)
        {
            if (!Enum.IsDefined(typeof(GritRecoveryEventKind), eventKind))
                throw new ArgumentOutOfRangeException(nameof(eventKind));
            if (!Enum.IsDefined(typeof(GritRecoveryStatus), status))
                throw new ArgumentOutOfRangeException(nameof(status));
            EventKind = eventKind;
            Status = status;
        }

        internal GritRecoveryEventKind EventKind { get; private set; }
        internal GritRecoveryStatus Status { get; private set; }
        internal bool ShouldRestore { get { return Status == GritRecoveryStatus.Eligible; } }
        internal int RestoreAmount { get { return ShouldRestore ? 1 : 0; } }
    }
}
