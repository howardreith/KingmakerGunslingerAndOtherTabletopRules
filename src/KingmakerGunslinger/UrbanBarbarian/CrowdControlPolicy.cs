using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal sealed class CrowdControlCandidate
    {
        internal bool IsInGame { get; set; }
        internal bool IsDestroyed { get; set; }
        internal bool IsDetached { get; set; }
        internal bool IsTurnedOn { get; set; }
        internal bool IsConscious { get; set; }
        internal bool IsHostile { get; set; }
        internal bool IsUntargetable { get; set; }
        internal bool IsSummoned { get; set; }
        internal double EdgeDistanceFeet { get; set; }
    }

    internal static class CrowdControlPolicy
    {
        internal const double AdjacentFeet = 5.0;
        internal const double DistanceToleranceFeet = 0.001;

        internal static int CountAdjacentActiveEnemies(
            IEnumerable<CrowdControlCandidate> candidates)
        {
            if (candidates == null) throw new ArgumentNullException("candidates");
            return candidates.Count(IsAdjacentActiveEnemy);
        }

        internal static bool Applies(IEnumerable<CrowdControlCandidate> candidates)
        {
            return CountAdjacentActiveEnemies(candidates) >= 2;
        }

        internal static bool IsAdjacentActiveEnemy(CrowdControlCandidate candidate)
        {
            return candidate != null && candidate.IsInGame &&
                !candidate.IsDestroyed && !candidate.IsDetached &&
                candidate.IsTurnedOn && candidate.IsConscious &&
                candidate.IsHostile && !double.IsNaN(candidate.EdgeDistanceFeet) &&
                !double.IsInfinity(candidate.EdgeDistanceFeet) &&
                candidate.EdgeDistanceFeet >= 0 &&
                candidate.EdgeDistanceFeet <= AdjacentFeet + DistanceToleranceFeet;
        }
    }
}
