using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Immutable, game-object-free snapshot presented to the correlation engine.
    /// Missing runtime data is represented explicitly by the caller rather than guessed.
    /// </summary>
    internal sealed class CombatTraceObservation
    {
        private readonly IReadOnlyDictionary<string, string> _fields;

        internal CombatTraceObservation(
            CombatTraceStage stage,
            CombatTracePhase phase,
            int eventIdentity,
            int? parentEventIdentity,
            bool isExactFirearm,
            int markerCount,
            IDictionary<string, string> fields)
        {
            if (eventIdentity == 0)
            {
                throw new ArgumentOutOfRangeException("eventIdentity", "An event identity must be nonzero.");
            }

            if (markerCount < -1)
            {
                throw new ArgumentOutOfRangeException("markerCount", "Marker count must be -1 or greater.");
            }

            if (isExactFirearm && markerCount != 1)
            {
                throw new ArgumentException(
                    "An exact firearm observation must report exactly one marker.",
                    "markerCount");
            }

            Stage = stage;
            Phase = phase;
            EventIdentity = eventIdentity;
            ParentEventIdentity = parentEventIdentity;
            IsExactFirearm = isExactFirearm;
            MarkerCount = markerCount;

            var copy = new SortedDictionary<string, string>(StringComparer.Ordinal);
            if (fields != null)
            {
                foreach (KeyValuePair<string, string> pair in fields)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                    {
                        throw new ArgumentException("Trace field keys must be nonempty.", "fields");
                    }

                    copy[pair.Key] = pair.Value ?? "<null>";
                }
            }

            _fields = new ReadOnlyDictionary<string, string>(copy);
        }

        internal CombatTraceStage Stage { get; private set; }

        internal CombatTracePhase Phase { get; private set; }

        internal int EventIdentity { get; private set; }

        internal int? ParentEventIdentity { get; private set; }

        internal bool IsExactFirearm { get; private set; }

        internal int MarkerCount { get; private set; }

        internal IReadOnlyDictionary<string, string> Fields
        {
            get { return _fields; }
        }
    }
}
