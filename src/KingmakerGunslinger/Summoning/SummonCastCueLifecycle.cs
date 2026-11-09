using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One accepted playback of a cast cue, and what happened to it.</summary>
    internal sealed class SummonCastCueRecord
    {
        internal string EventName;
        internal string Ability;
        internal string CasterId;
        internal DateTime Utc;
        internal int Frame;
        internal uint PlayingId;

        /// <summary>Set only by the sound engine's end-of-event callback.</summary>
        internal DateTime? EndUtc;
        internal int EndFrame = -1;

        internal string StopReason;
        internal DateTime? StopRequestedUtc;
        internal string StopResult;
        internal int StopAttempts;
    }

    /// <summary>
    /// Retirement of this feature's own unfinished playbacks, by playing ID.
    /// The installed stop call returns nothing, so a call that returns is a
    /// request, not proof of a stop; only the end-of-event callback is
    /// completion. Each playback gets one attempt, so repeated retirement is
    /// harmless, and one failure cannot skip the others.
    /// </summary>
    internal static class SummonCastCueLifecycle
    {
        internal const string StopRequested = "stop-requested";

        internal static int Retire(IEnumerable<SummonCastCueRecord> owned, string reason,
            Func<DateTime> now, Action<uint> stop)
        {
            int requested = 0;
            foreach (SummonCastCueRecord record in owned ?? new SummonCastCueRecord[0])
            {
                if (record == null || record.PlayingId == 0 || record.EndUtc.HasValue || record.StopAttempts > 0) continue;
                record.StopAttempts++;
                record.StopReason = reason;
                try
                {
                    stop(record.PlayingId);
                    record.StopRequestedUtc = now();
                    record.StopResult = StopRequested;
                    requested++;
                }
                catch (Exception error)
                {
                    record.StopResult = "failed:" + error.GetType().Name + ":" + error.Message;
                }
            }
            return requested;
        }
    }
}
