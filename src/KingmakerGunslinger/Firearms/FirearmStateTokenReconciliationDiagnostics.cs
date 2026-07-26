using System;
using System.Globalization;
using System.Threading;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Process-local evidence for Kingmaker's native ItemEntity.ApplyEnchantments pass.
    /// These counters do not own or reconstruct firearm state.
    /// </summary>
    internal static class FirearmStateTokenReconciliationDiagnostics
    {
        private static readonly object Gate = new object();
        private static long _calls;
        private static long _tokensObserved;
        private static long _preserved;
        private static long _restored;
        private static long _conflicts;
        private static long _faults;
        private static string _last = "No native item-enchantment reconciliation has observed a firearm-state token in this process.";

        internal static long Calls { get { return Interlocked.Read(ref _calls); } }
        internal static long TokensObserved { get { return Interlocked.Read(ref _tokensObserved); } }
        internal static long Preserved { get { return Interlocked.Read(ref _preserved); } }
        internal static long Restored { get { return Interlocked.Read(ref _restored); } }
        internal static long Conflicts { get { return Interlocked.Read(ref _conflicts); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }

        internal static void RecordCall(bool tokenObserved)
        {
            Interlocked.Increment(ref _calls);
            if (tokenObserved)
            {
                Interlocked.Increment(ref _tokensObserved);
            }
        }

        internal static void RecordDecision(
            FirearmStateTokenReconciliationDecision decision,
            string itemDescription)
        {
            if (decision == null)
            {
                throw new ArgumentNullException("decision");
            }

            switch (decision.Action)
            {
                case FirearmStateTokenReconciliationAction.NoToken:
                    return;
                case FirearmStateTokenReconciliationAction.Preserved:
                    Interlocked.Increment(ref _preserved);
                    break;
                case FirearmStateTokenReconciliationAction.RestoreMissing:
                    Interlocked.Increment(ref _restored);
                    break;
                case FirearmStateTokenReconciliationAction.Conflict:
                    Interlocked.Increment(ref _conflicts);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("decision");
            }

            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "{0}; item={1}",
                decision,
                Normalize(itemDescription)));
        }

        internal static void RecordFault(Exception exception, string phase)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Interlocked.Increment(ref _faults);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "FAULT phase={0}; {1}: {2}",
                Normalize(phase),
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
                "calls={0}; tokensObserved={1}; preserved={2}; restoredAfterNativeRemoval={3}; conflicts={4}; faults={5}; last={6}",
                Calls,
                TokensObserved,
                Preserved,
                Restored,
                Conflicts,
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
