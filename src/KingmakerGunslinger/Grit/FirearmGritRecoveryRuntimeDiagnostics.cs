using System;
using System.Threading;

namespace KingmakerGunslinger.Grit
{
    internal static class FirearmGritRecoveryRuntimeDiagnostics
    {
        private static int _criticalApplied;
        private static int _killingBlowApplied;
        private static int _duplicates;
        private static int _ignored;
        private static int _faults;
        private static int _lastBefore;
        private static int _lastAfter;

        internal static int CriticalApplied { get { return Volatile.Read(ref _criticalApplied); } }
        internal static int KillingBlowApplied { get { return Volatile.Read(ref _killingBlowApplied); } }
        internal static int Duplicates { get { return Volatile.Read(ref _duplicates); } }
        internal static int Ignored { get { return Volatile.Read(ref _ignored); } }
        internal static int Faults { get { return Volatile.Read(ref _faults); } }
        internal static int LastBefore { get { return Volatile.Read(ref _lastBefore); } }
        internal static int LastAfter { get { return Volatile.Read(ref _lastAfter); } }

        internal static void RecordApplied(GritRecoveryEventKind kind, int before, int after)
        {
            Volatile.Write(ref _lastBefore, before);
            Volatile.Write(ref _lastAfter, after);
            if (kind == GritRecoveryEventKind.ConfirmedCritical)
                Interlocked.Increment(ref _criticalApplied);
            else
                Interlocked.Increment(ref _killingBlowApplied);
        }

        internal static void RecordDuplicate(GritRecoveryEventKind kind)
        {
            Interlocked.Increment(ref _duplicates);
        }

        internal static void RecordIgnored(GritRecoveryEventKind kind,
            GritRecoveryStatus status)
        {
            Interlocked.Increment(ref _ignored);
        }

        internal static void RecordFault(GritRecoveryEventKind kind,
            Exception exception)
        {
            Interlocked.Increment(ref _faults);
        }

        internal static void Reset()
        {
            Volatile.Write(ref _criticalApplied, 0);
            Volatile.Write(ref _killingBlowApplied, 0);
            Volatile.Write(ref _duplicates, 0);
            Volatile.Write(ref _ignored, 0);
            Volatile.Write(ref _faults, 0);
            Volatile.Write(ref _lastBefore, 0);
            Volatile.Write(ref _lastAfter, 0);
        }
    }
}
