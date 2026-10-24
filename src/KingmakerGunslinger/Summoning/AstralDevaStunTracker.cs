using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Round-local successful hammer hits, kept separately for each
    /// opponent. The second hit arms one save; later hits on the same opponent
    /// cannot manufacture another save until the next round.</summary>
    internal sealed class AstralDevaStunTracker
    {
        private readonly Dictionary<string, int> _hits =
            new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> _attempted =
            new HashSet<string>(StringComparer.Ordinal);

        internal bool RecordHit(string targetId)
        {
            if (string.IsNullOrEmpty(targetId) || _attempted.Contains(targetId))
                return false;
            int count;
            _hits.TryGetValue(targetId, out count);
            count++;
            _hits[targetId] = count;
            if (count < 2) return false;
            _attempted.Add(targetId);
            return true;
        }

        internal int HitCount(string targetId)
        {
            int count;
            return !string.IsNullOrEmpty(targetId) &&
                _hits.TryGetValue(targetId, out count) ? count : 0;
        }

        internal bool WasAttempted(string targetId)
        { return !string.IsNullOrEmpty(targetId) && _attempted.Contains(targetId); }

        internal void Reset()
        {
            _hits.Clear();
            _attempted.Clear();
        }
    }
}
