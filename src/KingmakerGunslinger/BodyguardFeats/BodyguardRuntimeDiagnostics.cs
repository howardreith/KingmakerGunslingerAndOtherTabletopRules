using System.Collections.Generic;
using System.Threading;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal static class BodyguardRuntimeDiagnostics
    {
        private static long _frames;
        private static long _attempts;
        private static long _successfulAttempts;
        private static long _interceptions;
        private static long _faults;
        private static long _duplicateCallbacks;
        private static long _completed;
        private static string _lastObservation;
        private static readonly object ObservationGate = new object();
        private static readonly List<string> Observations = new List<string>();
        private const int ObservationLimit = 512;

        internal static long Frames { get { return Interlocked.Read(ref _frames); } }
        internal static long Attempts { get { return Interlocked.Read(ref _attempts); } }
        internal static long SuccessfulAttempts
        { get { return Interlocked.Read(ref _successfulAttempts); } }
        internal static long Interceptions
        { get { return Interlocked.Read(ref _interceptions); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static long DuplicateCallbacks
        { get { return Interlocked.Read(ref _duplicateCallbacks); } }
        internal static long Completed { get { return Interlocked.Read(ref _completed); } }
        internal static string LastObservation { get { return _lastObservation; } }
        internal static string[] SnapshotObservations()
        { lock (ObservationGate) return Observations.ToArray(); }

        internal static void Frame(string observation)
        { Record("frame", observation); Interlocked.Increment(ref _frames); }
        internal static void Attempt(bool success, string observation)
        {
            Record(success ? "attempt-success" : "attempt-failure", observation);
            Interlocked.Increment(ref _attempts);
            if (success) Interlocked.Increment(ref _successfulAttempts);
        }
        internal static void Intercept(string observation)
        { Record("intercept", observation); Interlocked.Increment(ref _interceptions); }
        internal static void Fault(string observation)
        { Record("fault", observation); Interlocked.Increment(ref _faults); }
        internal static void Duplicate(string observation)
        { Record("duplicate", observation); Interlocked.Increment(ref _duplicateCallbacks); }
        internal static void Complete(string observation)
        { Record("complete", observation); Interlocked.Increment(ref _completed); }
        internal static void Observation(string observation)
        { Record("observation", observation); }

        internal static void Reset()
        {
            Interlocked.Exchange(ref _frames, 0);
            Interlocked.Exchange(ref _attempts, 0);
            Interlocked.Exchange(ref _successfulAttempts, 0);
            Interlocked.Exchange(ref _interceptions, 0);
            Interlocked.Exchange(ref _faults, 0);
            Interlocked.Exchange(ref _duplicateCallbacks, 0);
            Interlocked.Exchange(ref _completed, 0);
            _lastObservation = null;
            lock (ObservationGate) Observations.Clear();
        }

        private static void Record(string kind, string observation)
        {
            string value = "kind=" + kind + ";" +
                (observation ?? string.Empty);
            _lastObservation = value;
            lock (ObservationGate)
            {
                if (Observations.Count == ObservationLimit)
                    Observations.RemoveAt(0);
                Observations.Add(value);
            }
        }
    }
}
