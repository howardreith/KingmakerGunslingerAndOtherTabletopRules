using System;
using System.Collections.Generic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>
    /// Deterministically filters a geometry-evaluated candidate sequence. Any
    /// unresolved geometry fails the complete action closed.
    /// </summary>
    internal sealed class ScatterTargetPlanService
    {
        internal ScatterTargetPlan Build(
            object exactWielder,
            IEnumerable<ScatterTargetCandidate> candidates)
        {
            if (exactWielder == null) throw new ArgumentNullException("exactWielder");
            if (exactWielder.GetType().IsValueType)
                throw new ArgumentException("The exact wielder must have reference identity.", "exactWielder");
            if (candidates == null) throw new ArgumentNullException("candidates");

            var accepted = new List<ScatterTargetCandidate>();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            int observed = 0;
            int outside = 0;
            int duplicates = 0;
            int wielders = 0;
            foreach (ScatterTargetCandidate candidate in candidates)
            {
                observed++;
                if (candidate == null)
                    throw new ArgumentException("Native enumeration produced a null scatter candidate.", "candidates");
                if (candidate.Geometry == ScatterGeometryDisposition.Unknown)
                {
                    throw new InvalidOperationException(
                        "Scatter geometry is unresolved; no partial target plan is permitted.");
                }
                if (ReferenceEquals(candidate.Unit, exactWielder))
                {
                    wielders++;
                    continue;
                }
                if (candidate.Geometry == ScatterGeometryDisposition.Outside)
                {
                    outside++;
                    continue;
                }
                if (!seen.Add(candidate.Unit))
                {
                    duplicates++;
                    continue;
                }
                accepted.Add(candidate);
            }

            accepted.Sort(Compare);
            return new ScatterTargetPlan(
                exactWielder, accepted, observed, outside, duplicates, wielders);
        }

        private static int Compare(ScatterTargetCandidate left, ScatterTargetCandidate right)
        {
            int distance = left.DistanceMeters.CompareTo(right.DistanceMeters);
            if (distance != 0) return distance;
            int identity = string.Compare(
                left.StableIdentity, right.StableIdentity, StringComparison.Ordinal);
            if (identity != 0) return identity;
            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal);
        }
    }
}
