namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Result of presenting one immutable observation to the correlation engine.
    /// </summary>
    internal sealed class CombatTraceDecision
    {
        private CombatTraceDecision(
            bool accepted,
            bool created,
            long traceId,
            int callbackOrdinal,
            CombatTraceRecord record,
            CombatTraceSnapshot completedTrace)
        {
            Accepted = accepted;
            Created = created;
            TraceId = traceId;
            CallbackOrdinal = callbackOrdinal;
            Record = record;
            CompletedTrace = completedTrace;
        }

        internal bool Accepted { get; private set; }

        internal bool Created { get; private set; }

        internal long TraceId { get; private set; }

        internal int CallbackOrdinal { get; private set; }

        internal CombatTraceRecord Record { get; private set; }

        internal CombatTraceSnapshot CompletedTrace { get; private set; }

        internal bool Completed
        {
            get { return CompletedTrace != null; }
        }

        internal static CombatTraceDecision Ignored()
        {
            return new CombatTraceDecision(false, false, 0L, 0, null, null);
        }

        internal static CombatTraceDecision AcceptedRecord(
            bool created,
            long traceId,
            CombatTraceRecord record,
            CombatTraceSnapshot completedTrace)
        {
            return new CombatTraceDecision(
                true,
                created,
                traceId,
                record == null ? 0 : record.CallbackOrdinal,
                record,
                completedTrace);
        }
    }
}
