using System.Threading;

namespace KingmakerGunslinger.Deeds
{
    internal static class QuickClearRuntimeDiagnostics
    {
        private static int _applied, _rejected, _faults;
        internal static int Applied { get { return Volatile.Read(ref _applied); } }
        internal static int Rejected { get { return Volatile.Read(ref _rejected); } }
        internal static int Faults { get { return Volatile.Read(ref _faults); } }
        internal static void Record(QuickClearDecision value)
        {
            if (value != null && value.ShouldRepair) Interlocked.Increment(ref _applied);
            else Interlocked.Increment(ref _rejected);
        }
        internal static void RecordFault() { Interlocked.Increment(ref _faults); }
        internal static void Reset() { _applied = _rejected = _faults = 0; }
    }
}
