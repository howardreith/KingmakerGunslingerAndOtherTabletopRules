using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Visual-only timing for one Eagle natural-attack swing.</summary>
    internal static class EagleAttackLungePolicy
    {
        internal const float MaximumMeters = 0.95f;
        internal const float ApproachSeconds = 0.16f;
        internal const float PreImpactLimitSeconds = 1.0f;
        internal const float ImpactHoldSeconds = 0.10f;
        internal const float ReturnSeconds = 0.22f;

        internal static float Weight(float elapsed, float sinceImpact)
        {
            if (!Finite(elapsed) || elapsed < 0f) return 0f;
            if (Finite(sinceImpact) && sinceImpact >= 0f)
            {
                if (sinceImpact <= ImpactHoldSeconds) return 1f;
                float returning = (sinceImpact - ImpactHoldSeconds) /
                    ReturnSeconds;
                return Math.Max(0f, 1f - returning);
            }
            if (elapsed < ApproachSeconds)
                return elapsed / ApproachSeconds;
            if (elapsed <= PreImpactLimitSeconds) return 1f;
            return Math.Max(0f, 1f -
                (elapsed - PreImpactLimitSeconds) / ReturnSeconds);
        }

        private static bool Finite(float value)
        { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
