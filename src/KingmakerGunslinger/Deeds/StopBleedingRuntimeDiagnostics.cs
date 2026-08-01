using System.Threading;

namespace KingmakerGunslinger.Deeds
{
    internal static class StopBleedingRuntimeDiagnostics
    {
        private static long _applied;
        private static long _rejected;
        private static long _faults;

        internal static long Applied { get { return Interlocked.Read(ref _applied); } }
        internal static long Rejected { get { return Interlocked.Read(ref _rejected); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static void RecordApplied() { Interlocked.Increment(ref _applied); }
        internal static void RecordRejected() { Interlocked.Increment(ref _rejected); }
        internal static void RecordFault() { Interlocked.Increment(ref _faults); }
        internal static void Reset()
        {
            Interlocked.Exchange(ref _applied, 0);
            Interlocked.Exchange(ref _rejected, 0);
            Interlocked.Exchange(ref _faults, 0);
        }
    }
}
