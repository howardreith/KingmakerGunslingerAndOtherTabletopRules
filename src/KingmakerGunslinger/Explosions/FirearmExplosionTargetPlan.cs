using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Explosions
{
    /// <summary>
    /// Immutable deterministic target plan. Nearby targets are ordered by distance
    /// and stable identity; the exact wielder is deliberately last so a lethal
    /// self-damage result cannot prevent already-qualified nearby targets from
    /// receiving their simultaneous burst consequence.
    /// </summary>
    internal sealed class FirearmExplosionTargetPlan
    {
        private readonly FirearmExplosionTargetCandidate[] _targets;

        internal FirearmExplosionTargetPlan(
            IEnumerable<FirearmExplosionTargetCandidate> targets,
            int observedCandidates,
            int duplicateCandidates)
        {
            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (observedCandidates < 0)
            {
                throw new ArgumentOutOfRangeException("observedCandidates");
            }

            if (duplicateCandidates < 0 || duplicateCandidates > observedCandidates)
            {
                throw new ArgumentOutOfRangeException("duplicateCandidates");
            }

            _targets = new List<FirearmExplosionTargetCandidate>(targets).ToArray();
            if (_targets.Length == 0)
            {
                throw new ArgumentException(
                    "An explosion target plan must include the exact wielder.",
                    "targets");
            }

            int wielderCount = 0;
            foreach (FirearmExplosionTargetCandidate target in _targets)
            {
                if (target == null)
                {
                    throw new ArgumentException(
                        "Explosion target plans cannot contain null targets.",
                        "targets");
                }

                if (target.IsExactWielder)
                {
                    wielderCount++;
                }
            }

            if (wielderCount != 1 || !_targets[_targets.Length - 1].IsExactWielder)
            {
                throw new ArgumentException(
                    "An explosion target plan must contain the exact wielder exactly once and last.",
                    "targets");
            }

            ObservedCandidates = observedCandidates;
            DuplicateCandidates = duplicateCandidates;
        }

        internal IReadOnlyList<FirearmExplosionTargetCandidate> Targets
        {
            get { return _targets; }
        }

        internal int TargetCount
        {
            get { return _targets.Length; }
        }

        internal int ObservedCandidates { get; private set; }

        internal int DuplicateCandidates { get; private set; }
    }
}
