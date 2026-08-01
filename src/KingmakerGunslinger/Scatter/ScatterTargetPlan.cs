using System;
using System.Collections.Generic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>Immutable, exact-reference scatter target plan.</summary>
    internal sealed class ScatterTargetPlan
    {
        private readonly ScatterTargetCandidate[] _targets;

        internal ScatterTargetPlan(
            object exactWielder,
            IEnumerable<ScatterTargetCandidate> targets,
            int observedCandidates,
            int outsideCandidates,
            int duplicateCandidates,
            int wielderCandidates)
        {
            if (exactWielder == null) throw new ArgumentNullException("exactWielder");
            if (targets == null) throw new ArgumentNullException("targets");
            if (observedCandidates < 0) throw new ArgumentOutOfRangeException("observedCandidates");
            if (outsideCandidates < 0 || outsideCandidates > observedCandidates)
                throw new ArgumentOutOfRangeException("outsideCandidates");
            if (duplicateCandidates < 0 || duplicateCandidates > observedCandidates)
                throw new ArgumentOutOfRangeException("duplicateCandidates");
            if (wielderCandidates < 0 || wielderCandidates > observedCandidates)
                throw new ArgumentOutOfRangeException("wielderCandidates");

            _targets = new List<ScatterTargetCandidate>(targets).ToArray();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            foreach (ScatterTargetCandidate target in _targets)
            {
                if (target == null)
                    throw new ArgumentException("Scatter target plans cannot contain null targets.", "targets");
                if (target.Geometry != ScatterGeometryDisposition.Inside)
                    throw new ArgumentException("Scatter target plans may contain only inside-cone targets.", "targets");
                if (ReferenceEquals(target.Unit, exactWielder))
                    throw new ArgumentException("Scatter target plans cannot include the exact wielder.", "targets");
                if (!seen.Add(target.Unit))
                    throw new ArgumentException("Scatter target plans cannot contain duplicate unit references.", "targets");
            }

            ObservedCandidates = observedCandidates;
            OutsideCandidates = outsideCandidates;
            DuplicateCandidates = duplicateCandidates;
            WielderCandidates = wielderCandidates;
        }

        internal IReadOnlyList<ScatterTargetCandidate> Targets { get { return _targets; } }
        internal int TargetCount { get { return _targets.Length; } }
        internal int ObservedCandidates { get; private set; }
        internal int OutsideCandidates { get; private set; }
        internal int DuplicateCandidates { get; private set; }
        internal int WielderCandidates { get; private set; }
    }
}
