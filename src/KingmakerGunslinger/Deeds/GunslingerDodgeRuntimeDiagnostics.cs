using System;
using System.Threading;

namespace KingmakerGunslinger.Deeds
{
    internal static class GunslingerDodgeRuntimeDiagnostics
    {
        private static long _applied, _rejected, _duplicates, _faults;
        internal static long Applied { get { return Interlocked.Read(ref _applied); } }
        internal static long Rejected { get { return Interlocked.Read(ref _rejected); } }
        internal static long Duplicates { get { return Interlocked.Read(ref _duplicates); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static void Record(GunslingerDodgeDecision decision)
        {
            if (decision != null && decision.ShouldApply) Interlocked.Increment(ref _applied);
            else Interlocked.Increment(ref _rejected);
        }
        internal static void RecordDuplicate() { Interlocked.Increment(ref _duplicates); }
        internal static void RecordFault(Exception ignored) { Interlocked.Increment(ref _faults); }
        internal static void Reset()
        {
            Interlocked.Exchange(ref _applied, 0); Interlocked.Exchange(ref _rejected, 0);
            Interlocked.Exchange(ref _duplicates, 0); Interlocked.Exchange(ref _faults, 0);
        }
    }
}
