using System;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class SatyrGaitSample
    {
        internal double Time;
        internal float LeftForward, RightForward, LeftHeight, RightHeight;
        internal float LeftKneeForward, RightKneeForward, LeftThighDegrees, RightThighDegrees;
        internal bool LiveLocomotion;
        internal float[] Values => new[] { LeftForward, RightForward, LeftHeight, RightHeight,
            LeftKneeForward, RightKneeForward, LeftThighDegrees, RightThighDegrees };
    }

    // Evidence from the exact native joints during an ordinary native move.
    // Root/world translation is deliberately absent from this verdict.
    internal static class SatyrGaitPolicy
    {
        internal static bool Stepping(SatyrGaitSample[] samples)
        {
            if (samples == null || samples.Length < 12 || samples.Any(s => s == null || !s.LiveLocomotion ||
                double.IsNaN(s.Time) || double.IsInfinity(s.Time) ||
                s.Values.Any(v => float.IsNaN(v) || float.IsInfinity(v)))) return false;
            if (samples.Last().Time - samples[0].Time < .5 ||
                samples.Skip(1).Where((s, i) => s.Time <= samples[i].Time).Any()) return false;
            if (Range(samples.Select(s => s.LeftForward).ToArray()) < .08f ||
                Range(samples.Select(s => s.RightForward).ToArray()) < .08f ||
                Range(samples.Select(s => s.LeftHeight).ToArray()) < .02f ||
                Range(samples.Select(s => s.RightHeight).ToArray()) < .02f ||
                Range(samples.Select(s => s.LeftKneeForward).ToArray()) < .04f ||
                Range(samples.Select(s => s.RightKneeForward).ToArray()) < .04f ||
                Range(samples.Select(s => s.LeftThighDegrees).ToArray()) < 8 ||
                Range(samples.Select(s => s.RightThighDegrees).ToArray()) < 8) return false;
            var separation = samples.Select(s => s.LeftForward - s.RightForward).ToArray();
            float centre = (separation.Min() + separation.Max()) / 2;
            int changes = 0, previous = 0;
            foreach (float value in separation)
            {
                int phase = value > centre + .04f ? 1 : value < centre - .04f ? -1 : 0;
                if (phase == 0) continue;
                if (previous != 0 && previous != phase) changes++;
                previous = phase;
            }
            return changes >= 2;
        }

        private static float Range(float[] values) => values.Max() - values.Min();

        internal static bool RecentRetiredBow(bool observedPlayingClip, bool sameHandle, bool sameSelectedClip,
            bool finished, bool interrupted, bool acted, int playbackFrame, int releaseFrame) =>
            observedPlayingClip && sameHandle && sameSelectedClip && finished && !interrupted && acted &&
            playbackFrame >= 0 && releaseFrame >= playbackFrame && releaseFrame - playbackFrame <= 2;
    }
}
