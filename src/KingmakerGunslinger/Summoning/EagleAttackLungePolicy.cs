using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Summoning
{
    // Only the three original flight attachments demonstrated by the release143
    // ownership receipt. No global resource scan or name-based destruction.
    internal static class OriginalFlightCleanupPolicy
    {
        internal static bool Handles(string key)
        { return key == "eagle" || key == "dire-bat" || key == "pteranodon"; }

        internal static T[] PrivateMaterials<T>(T created, IEnumerable<T> installed,
            IEnumerable<T> borrowed) where T : class
        {
            var originals = new List<T>(borrowed ?? new T[0]);
            var result = new List<T>();
            Action<T> add = value => {
                if (value == null || originals.Exists(o => ReferenceEquals(o, value)) ||
                    result.Exists(o => ReferenceEquals(o, value))) return;
                result.Add(value);
            };
            add(created);
            foreach (T value in installed ?? new T[0]) add(value);
            return result.ToArray();
        }
    }

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
