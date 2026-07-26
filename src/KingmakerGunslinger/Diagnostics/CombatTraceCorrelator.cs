using System;
using System.Collections.Generic;
using System.Globalization;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Game-independent event correlator. It stores only integer event identities and
    /// immutable snapshots; no rule, unit, item, or blueprint object can escape a callback.
    /// </summary>
    internal sealed class CombatTraceCorrelator
    {
        private readonly Func<long> _nextTraceId;
        private readonly Dictionary<int, TraceState> _eventToTrace =
            new Dictionary<int, TraceState>();
        private readonly Dictionary<long, TraceState> _traces =
            new Dictionary<long, TraceState>();

        internal CombatTraceCorrelator(Func<long> nextTraceId)
        {
            _nextTraceId = nextTraceId ?? throw new ArgumentNullException("nextTraceId");
        }

        internal int ActiveTraceCount
        {
            get { return _traces.Count; }
        }

        internal CombatTraceDecision Observe(
            CombatTraceObservation observation,
            bool closesRootEvent)
        {
            if (observation == null)
            {
                throw new ArgumentNullException("observation");
            }

            TraceState trace = FindTrace(observation);
            bool created = false;
            if (trace == null && CanStartTrace(observation))
            {
                long traceId = _nextTraceId();
                if (traceId < 1L || _traces.ContainsKey(traceId))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The trace ID source produced an invalid or duplicate ID: {0}.",
                            traceId));
                }

                trace = new TraceState(
                    traceId,
                    observation.EventIdentity,
                    observation.Stage);
                _traces.Add(traceId, trace);
                created = true;
            }

            if (trace == null)
            {
                return CombatTraceDecision.Ignored();
            }

            MapEvent(trace, observation.EventIdentity);
            int callbackOrdinal = trace.NextCallbackOrdinal(observation);
            var record = new CombatTraceRecord(observation, callbackOrdinal);
            trace.Records.Add(record);

            CombatTraceSnapshot completed = null;
            if (closesRootEvent &&
                observation.Phase == CombatTracePhase.After &&
                observation.EventIdentity == trace.RootEventIdentity)
            {
                completed = Complete(trace);
            }

            return CombatTraceDecision.AcceptedRecord(
                created,
                trace.TraceId,
                record,
                completed);
        }

        internal int Reset()
        {
            int count = _traces.Count;
            _eventToTrace.Clear();
            _traces.Clear();
            return count;
        }

        private static bool CanStartTrace(CombatTraceObservation observation)
        {
            if (!observation.IsExactFirearm || observation.MarkerCount != 1)
            {
                return false;
            }

            return observation.Stage == CombatTraceStage.WeaponAttack ||
                observation.Stage == CombatTraceStage.AttackRoll;
        }

        private TraceState FindTrace(CombatTraceObservation observation)
        {
            TraceState trace;
            if (_eventToTrace.TryGetValue(observation.EventIdentity, out trace))
            {
                return trace;
            }

            if (observation.ParentEventIdentity.HasValue &&
                _eventToTrace.TryGetValue(observation.ParentEventIdentity.Value, out trace))
            {
                return trace;
            }

            return null;
        }

        private void MapEvent(TraceState trace, int eventIdentity)
        {
            TraceState existing;
            if (_eventToTrace.TryGetValue(eventIdentity, out existing) &&
                !ReferenceEquals(existing, trace))
            {
                throw new InvalidOperationException(
                    "One runtime event identity was associated with two active combat traces.");
            }

            _eventToTrace[eventIdentity] = trace;
            trace.EventIdentities.Add(eventIdentity);
        }

        private CombatTraceSnapshot Complete(TraceState trace)
        {
            foreach (int eventIdentity in trace.EventIdentities)
            {
                TraceState mapped;
                if (_eventToTrace.TryGetValue(eventIdentity, out mapped) &&
                    ReferenceEquals(mapped, trace))
                {
                    _eventToTrace.Remove(eventIdentity);
                }
            }

            _traces.Remove(trace.TraceId);
            return new CombatTraceSnapshot(
                trace.TraceId,
                trace.RootEventIdentity,
                trace.RootStage,
                trace.Records);
        }

        private sealed class TraceState
        {
            private readonly Dictionary<string, int> _callbackCounts =
                new Dictionary<string, int>(StringComparer.Ordinal);

            internal TraceState(
                long traceId,
                int rootEventIdentity,
                CombatTraceStage rootStage)
            {
                TraceId = traceId;
                RootEventIdentity = rootEventIdentity;
                RootStage = rootStage;
                Records = new List<CombatTraceRecord>();
                EventIdentities = new HashSet<int>();
            }

            internal long TraceId { get; private set; }

            internal int RootEventIdentity { get; private set; }

            internal CombatTraceStage RootStage { get; private set; }

            internal List<CombatTraceRecord> Records { get; private set; }

            internal HashSet<int> EventIdentities { get; private set; }

            internal int NextCallbackOrdinal(CombatTraceObservation observation)
            {
                string key = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}:{1}:{2}",
                    observation.EventIdentity,
                    observation.Stage,
                    observation.Phase);
                int count;
                _callbackCounts.TryGetValue(key, out count);
                count++;
                _callbackCounts[key] = count;
                return count;
            }
        }
    }
}
