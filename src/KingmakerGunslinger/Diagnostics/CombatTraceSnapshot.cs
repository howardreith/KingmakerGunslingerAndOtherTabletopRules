using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Immutable completed trace suitable for formatting after all runtime objects have gone out of scope.
    /// </summary>
    internal sealed class CombatTraceSnapshot
    {
        private readonly IReadOnlyList<CombatTraceRecord> _records;

        internal CombatTraceSnapshot(
            long traceId,
            int rootEventIdentity,
            CombatTraceStage rootStage,
            IEnumerable<CombatTraceRecord> records)
        {
            if (traceId < 1L)
            {
                throw new ArgumentOutOfRangeException("traceId", "Trace ID must be positive.");
            }

            if (rootEventIdentity == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "rootEventIdentity",
                    "Root event identity must be nonzero.");
            }

            if (records == null)
            {
                throw new ArgumentNullException("records");
            }

            List<CombatTraceRecord> copy = records.ToList();
            TraceId = traceId;
            RootEventIdentity = rootEventIdentity;
            RootStage = rootStage;
            _records = new ReadOnlyCollection<CombatTraceRecord>(copy);
            DuplicateCallbackCount = copy.Count(record => record.IsDuplicate);
        }

        internal long TraceId { get; private set; }

        internal int RootEventIdentity { get; private set; }

        internal CombatTraceStage RootStage { get; private set; }

        internal IReadOnlyList<CombatTraceRecord> Records
        {
            get { return _records; }
        }

        internal int DuplicateCallbackCount { get; private set; }
    }
}
