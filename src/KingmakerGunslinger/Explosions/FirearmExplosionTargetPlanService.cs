using System;
using System.Collections.Generic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Explosions
{
    /// <summary>
    /// Pure deterministic reference-identity planner for a spatial query that has
    /// already applied Kingmaker's native radius, line-of-sight, dead-unit, and
    /// untargetable-unit filters.
    /// </summary>
    internal sealed class FirearmExplosionTargetPlanService
    {
        internal FirearmExplosionTargetPlan Build(
            FirearmExplosionTargetCandidate exactWielder,
            IEnumerable<FirearmExplosionTargetCandidate> nearbyCandidates)
        {
            if (exactWielder == null)
            {
                throw new ArgumentNullException("exactWielder");
            }

            if (!exactWielder.IsExactWielder)
            {
                throw new ArgumentException(
                    "The exact-wielder target must be marked as the exact wielder.",
                    "exactWielder");
            }

            if (nearbyCandidates == null)
            {
                throw new ArgumentNullException("nearbyCandidates");
            }

            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            seen.Add(exactWielder.Unit);

            var nearby = new List<FirearmExplosionTargetCandidate>();
            int observed = 0;
            int duplicates = 0;
            foreach (FirearmExplosionTargetCandidate candidate in nearbyCandidates)
            {
                observed++;
                if (candidate == null)
                {
                    throw new ArgumentException(
                        "The spatial query produced a null target candidate.",
                        "nearbyCandidates");
                }

                if (candidate.IsExactWielder)
                {
                    throw new ArgumentException(
                        "Nearby target candidates cannot claim exact-wielder status.",
                        "nearbyCandidates");
                }

                if (!seen.Add(candidate.Unit))
                {
                    duplicates++;
                    continue;
                }

                nearby.Add(candidate);
            }

            nearby.Sort(CompareNearby);
            nearby.Add(exactWielder);
            return new FirearmExplosionTargetPlan(
                nearby,
                observed,
                duplicates);
        }

        private static int CompareNearby(
            FirearmExplosionTargetCandidate left,
            FirearmExplosionTargetCandidate right)
        {
            int distance = left.DistanceMeters.CompareTo(right.DistanceMeters);
            if (distance != 0)
            {
                return distance;
            }

            int identity = string.Compare(
                left.StableIdentity,
                right.StableIdentity,
                StringComparison.Ordinal);
            if (identity != 0)
            {
                return identity;
            }

            return string.Compare(
                left.DisplayName,
                right.DisplayName,
                StringComparison.Ordinal);
        }
    }
}
