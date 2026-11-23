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

        /// <summary>
        /// VIS-04: the owned impact cue, an installed one-shot in the VFX bank
        /// the summon cue helper already loads (Wwise ID 4293635115, sources
        /// Sweetener_Low_06a/b/c_Quake): a low randomized thud layered over the
        /// native weapon hit to give the club its weight. Provisional until the
        /// owner listens; the event name is the only thing to change.
        /// </summary>
        internal const string ImpactCue = "Sweetener_Low_06_Quake";
        internal const string ImpactCueLabel = "AnkylosaurusTailImpact";

        /// <summary>
        /// A qualifying impact is the exact owner's exact tail weapon, a
        /// resolved native hit, its first observation and a struck creature.
        /// The later Fortitude save, its result and the target's survival do
        /// not change whether the club landed.
        /// </summary>
        internal static bool QualifiesForCue(bool exactOwner, bool exactTail, bool hit,
            bool firstObservation, bool victimPresent)
        {
            return exactOwner && exactTail && hit && firstObservation && victimPresent;
        }

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
