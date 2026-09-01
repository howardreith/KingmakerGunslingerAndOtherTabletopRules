using System;
using System.Globalization;
using System.Threading;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Process-local counters for the first real reload ability. These values are
    /// diagnostic only and never participate in firearm persistence or gameplay rules.
    /// </summary>
    internal static class ReloadRuntimeDiagnostics
    {
        private static readonly object Gate = new object();
        private static long _attempts;
        private static long _loaded;
        private static long _rejected;
        private static long _faults;
        private static string _lastResult = "No reload delivery has completed in this process.";

        internal static long Attempts { get { return Interlocked.Read(ref _attempts); } }
        internal static long Loaded { get { return Interlocked.Read(ref _loaded); } }
        internal static long Rejected { get { return Interlocked.Read(ref _rejected); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }

        internal static string LastResult
        {
            get
            {
                lock (Gate)
                {
                    return _lastResult;
                }
            }
        }

        internal static void Record(FirearmReloadResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            Interlocked.Increment(ref _attempts);
            if (result.Succeeded)
            {
                Interlocked.Increment(ref _loaded);
            }
            else
            {
                Interlocked.Increment(ref _rejected);
            }

            lock (Gate)
            {
                _lastResult = result.ToString();
            }
        }

        internal static void RecordRejected(string technicalReason)
        {
            if (string.IsNullOrWhiteSpace(technicalReason))
                throw new ArgumentException("A technical reload reason is required.",
                    "technicalReason");
            Interlocked.Increment(ref _attempts);
            Interlocked.Increment(ref _rejected);
            lock (Gate)
            {
                _lastResult = "REJECTED " + technicalReason;
            }
        }

        internal static void RecordFault(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Interlocked.Increment(ref _attempts);
            Interlocked.Increment(ref _faults);
            lock (Gate)
            {
                _lastResult = string.Format(
                    CultureInfo.InvariantCulture,
                    "FAULT {0}: {1}",
                    exception.GetType().Name,
                    exception.Message);
            }
        }

        internal static string Describe()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "attempts={0}; loaded={1}; rejected={2}; faults={3}; last={4}",
                Attempts,
                Loaded,
                Rejected,
                Faults,
                LastResult);
        }
    }
}
