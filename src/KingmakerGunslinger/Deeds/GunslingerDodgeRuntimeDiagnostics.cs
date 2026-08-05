using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Kingmaker.UnitLogic.Buffs;

namespace KingmakerGunslinger.Deeds
{
    internal static class GunslingerDodgeRuntimeDiagnostics
    {
        private static long _applied, _rejected, _duplicates, _faults;
        private static string _deliveryEvidence = "deliverEntered=false";
        internal static long Applied { get { return Interlocked.Read(ref _applied); } }
        internal static long Rejected { get { return Interlocked.Read(ref _rejected); } }
        internal static long Duplicates { get { return Interlocked.Read(ref _duplicates); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static string DeliveryEvidence { get { return _deliveryEvidence; } }
        internal static void Record(GunslingerDodgeDecision decision)
        {
            if (decision != null && decision.ShouldApply) Interlocked.Increment(ref _applied);
            else Interlocked.Increment(ref _rejected);
        }
        internal static void RecordDuplicate() { Interlocked.Increment(ref _duplicates); }
        internal static void RecordFault(Exception ignored) { Interlocked.Increment(ref _faults); }

        internal static void RecordDeliveryEntered(int acBefore)
        {
            _deliveryEvidence = "deliverEntered=true;buffCreated=false;" +
                "deliveryFault=false;acBeforeDeliver=" + acBefore;
        }

        internal static void RecordDeliveryApplied(Buff buff, int acBefore, int acAfter)
        {
            _deliveryEvidence = "deliverEntered=true;buffCreated=" + (buff != null) +
                ";deliveryFault=false;buffRuntimeIdentity=" +
                (buff == null ? "null" : RuntimeHelpers.GetHashCode(buff).ToString()) +
                ";buffStart=" + (buff == null ? TimeSpan.Zero :
                    buff.EndTime - buff.TimeLeft) +
                ";buffEnd=" + (buff == null ? TimeSpan.Zero : buff.EndTime) +
                ";buffRemaining=" + (buff == null ? TimeSpan.Zero : buff.TimeLeft) +
                ";acBeforeDeliver=" + acBefore + ";acAfterDeliver=" + acAfter;
        }

        internal static void RecordDeliveryFault(
            Exception exception, int acBefore, int acAfter)
        {
            Interlocked.Increment(ref _faults);
            _deliveryEvidence = "deliverEntered=true;buffCreated=false;" +
                "deliveryFault=true;exception=" +
                (exception == null ? "<null>" : exception.GetType().FullName) +
                ";message=" + (exception == null ? "<null>" : exception.Message) +
                ";acBeforeDeliver=" + acBefore + ";acAfterDeliver=" + acAfter;
        }

        internal static void Reset()
        {
            Interlocked.Exchange(ref _applied, 0); Interlocked.Exchange(ref _rejected, 0);
            Interlocked.Exchange(ref _duplicates, 0); Interlocked.Exchange(ref _faults, 0);
            _deliveryEvidence = "deliverEntered=false";
        }
    }
}
