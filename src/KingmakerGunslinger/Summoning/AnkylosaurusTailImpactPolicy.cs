using System;

namespace KingmakerGunslinger.Summoning
{
    internal static class AnkylosaurusTailImpactPolicy
    {
        internal const float DurationSeconds = .24f;
        internal const float SurfaceClearanceMeters = .02f;
        // The adult's independently measured 15-foot tail reach permits a
        // cosmetic skeleton approach bounded by half its 20-foot footprint.
        internal const float MaximumApproachMeters = 3.048f;

        internal static float Weight(float elapsed)
        {
            if (float.IsNaN(elapsed) || float.IsInfinity(elapsed) ||
                elapsed < 0f || elapsed >= DurationSeconds) return 0f;
            return (float)Math.Sin(Math.PI * elapsed / DurationSeconds);
        }

        internal static float Approach(float gap, float weight)
        {
            if (float.IsNaN(gap) || float.IsInfinity(gap) || gap <=
                    SurfaceClearanceMeters || float.IsNaN(weight) ||
                float.IsInfinity(weight) || weight <= 0f) return 0f;
            return Math.Min(gap - SurfaceClearanceMeters,
                MaximumApproachMeters * Math.Min(weight, 1f));
        }
    }
}
