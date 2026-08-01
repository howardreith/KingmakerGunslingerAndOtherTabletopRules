using System;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>
    /// Dependency-free unit candidate after native enumeration and an explicit
    /// geometry decision. Unit identity is always reference identity.
    /// </summary>
    internal sealed class ScatterTargetCandidate
    {
        internal ScatterTargetCandidate(
            object unit,
            string stableIdentity,
            string displayName,
            float distanceMeters,
            ScatterGeometryDisposition geometry)
        {
            if (unit == null) throw new ArgumentNullException("unit");
            if (unit.GetType().IsValueType)
            {
                throw new ArgumentException(
                    "A scatter target must have reference identity.", "unit");
            }
            if (string.IsNullOrWhiteSpace(stableIdentity))
                throw new ArgumentException("A stable target identity is required.", "stableIdentity");
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("A target display name is required.", "displayName");
            if (float.IsNaN(distanceMeters) || float.IsInfinity(distanceMeters) ||
                distanceMeters < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    "distanceMeters", distanceMeters,
                    "Scatter target distance must be finite and nonnegative.");
            }
            if (geometry != ScatterGeometryDisposition.Inside &&
                geometry != ScatterGeometryDisposition.Outside &&
                geometry != ScatterGeometryDisposition.Unknown)
            {
                throw new ArgumentOutOfRangeException("geometry");
            }

            Unit = unit;
            StableIdentity = stableIdentity.Trim();
            DisplayName = displayName.Trim();
            DistanceMeters = distanceMeters;
            Geometry = geometry;
        }

        internal object Unit { get; private set; }
        internal string StableIdentity { get; private set; }
        internal string DisplayName { get; private set; }
        internal float DistanceMeters { get; private set; }
        internal ScatterGeometryDisposition Geometry { get; private set; }
    }
}
