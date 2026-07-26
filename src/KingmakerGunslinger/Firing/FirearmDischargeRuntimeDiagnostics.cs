using System;
using System.Globalization;
using System.Threading;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Process-local observations for loaded-round attack enforcement. Counters are
    /// diagnostic only and are never a source of firearm state.
    /// </summary>
    internal static class FirearmDischargeRuntimeDiagnostics
    {
        private static readonly object Gate = new object();
        private static long _observed;
        private static long _ignored;
        private static long _fired;
        private static long _emptyRejected;
        private static long _wreckedRejected;
        private static long _duplicates;
        private static long _faults;
        private static string _last = "No firearm attack roll has been enforced in this process.";

        internal static long Observed { get { return Interlocked.Read(ref _observed); } }
        internal static long Ignored { get { return Interlocked.Read(ref _ignored); } }
        internal static long Fired { get { return Interlocked.Read(ref _fired); } }
        internal static long EmptyRejected { get { return Interlocked.Read(ref _emptyRejected); } }
        internal static long WreckedRejected { get { return Interlocked.Read(ref _wreckedRejected); } }
        internal static long Duplicates { get { return Interlocked.Read(ref _duplicates); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }

        internal static void RecordIgnored(string reason)
        {
            Interlocked.Increment(ref _observed);
            Interlocked.Increment(ref _ignored);
            SetLast("IGNORED: " + Normalize(reason));
        }

        internal static void RecordDuplicate()
        {
            Interlocked.Increment(ref _duplicates);
            SetLast("DUPLICATE: an already-observed attack-roll event was not allowed to consume another round.");
        }

        internal static void Record(FirearmDischargeResult result, string firearm)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            Interlocked.Increment(ref _observed);
            switch (result.Status)
            {
                case FirearmDischargeStatus.Fired:
                    Interlocked.Increment(ref _fired);
                    break;
                case FirearmDischargeStatus.Empty:
                    Interlocked.Increment(ref _emptyRejected);
                    break;
                case FirearmDischargeStatus.Wrecked:
                    Interlocked.Increment(ref _wreckedRejected);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("result");
            }

            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "{0}; firearm={1}",
                result,
                Normalize(firearm)));
        }

        internal static void RecordFault(Exception exception, bool firearmWasRecognized)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Interlocked.Increment(ref _observed);
            Interlocked.Increment(ref _faults);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "FAULT recognizedFirearm={0}; {1}: {2}",
                firearmWasRecognized,
                exception.GetType().Name,
                exception.Message));
        }

        internal static string Describe()
        {
            string last;
            lock (Gate)
            {
                last = _last;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "observed={0}; fired={1}; emptyRejected={2}; wreckedRejected={3}; ignored={4}; duplicateEvents={5}; faults={6}; last={7}",
                Observed,
                Fired,
                EmptyRejected,
                WreckedRejected,
                Ignored,
                Duplicates,
                Faults,
                last);
        }

        private static void SetLast(string value)
        {
            lock (Gate)
            {
                _last = value;
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<unavailable>" : value.Trim();
        }
    }
}
