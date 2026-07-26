using System;

namespace KingmakerGunslinger.Explosions
{
    /// <summary>
    /// Pure reference-identity target record used after Kingmaker's exact spatial
    /// query has established radius, line-of-sight, living-state, and targetability.
    /// It carries no scene or rulebook behavior and is dependency-free for tests.
    /// </summary>
    internal sealed class FirearmExplosionTargetCandidate
    {
        internal FirearmExplosionTargetCandidate(
            object unit,
            string stableIdentity,
            string displayName,
            float distanceMeters,
            bool isExactWielder)
        {
            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            if (unit.GetType().IsValueType)
            {
                throw new ArgumentException(
                    "An explosion target must have reference identity.",
                    "unit");
            }

            if (string.IsNullOrWhiteSpace(stableIdentity))
            {
                throw new ArgumentException(
                    "A stable target identity is required.",
                    "stableIdentity");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(
                    "A target display name is required.",
                    "displayName");
            }

            if (float.IsNaN(distanceMeters) ||
                float.IsInfinity(distanceMeters) ||
                distanceMeters < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    "distanceMeters",
                    distanceMeters,
                    "Target distance must be a finite nonnegative value.");
            }

            Unit = unit;
            StableIdentity = stableIdentity.Trim();
            DisplayName = displayName.Trim();
            DistanceMeters = distanceMeters;
            IsExactWielder = isExactWielder;
        }

        internal object Unit { get; private set; }

        internal string StableIdentity { get; private set; }

        internal string DisplayName { get; private set; }

        internal float DistanceMeters { get; private set; }

        internal bool IsExactWielder { get; private set; }
    }
}
