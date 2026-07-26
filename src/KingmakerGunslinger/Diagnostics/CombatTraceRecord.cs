using System;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// One correlated observation plus its callback ordinal.
    /// </summary>
    internal sealed class CombatTraceRecord
    {
        internal CombatTraceRecord(CombatTraceObservation observation, int callbackOrdinal)
        {
            Observation = observation ?? throw new ArgumentNullException("observation");
            if (callbackOrdinal < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "callbackOrdinal",
                    "Callback ordinal must be positive.");
            }

            CallbackOrdinal = callbackOrdinal;
        }

        internal CombatTraceObservation Observation { get; private set; }

        internal int CallbackOrdinal { get; private set; }

        internal bool IsDuplicate
        {
            get { return CallbackOrdinal > 1; }
        }
    }
}
