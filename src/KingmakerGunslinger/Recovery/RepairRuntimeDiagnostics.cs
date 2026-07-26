using System;
using System.Globalization;
using System.Threading;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Process-local counters for the player-facing ordinary repair ability. These
    /// diagnostics never participate in item persistence or gameplay decisions.
    /// </summary>
    internal static class RepairRuntimeDiagnostics
    {
        private static readonly object Gate = new object();
        private static long _attempts;
        private static long _completed;
        private static long _rejected;
        private static long _faults;
        private static string _lastResult =
            "No Repair Test Musket delivery has completed in this process.";

        internal static long Attempts { get { return Interlocked.Read(ref _attempts); } }

        internal static long Completed { get { return Interlocked.Read(ref _completed); } }

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

        internal static void Record(FirearmRepairRuntimeResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            Interlocked.Increment(ref _attempts);
            if (result.Succeeded)
            {
                Interlocked.Increment(ref _completed);
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
                "attempts={0}; completed={1}; rejected={2}; faults={3}; last={4}",
                Attempts,
                Completed,
                Rejected,
                Faults,
                LastResult);
        }
    }
}
