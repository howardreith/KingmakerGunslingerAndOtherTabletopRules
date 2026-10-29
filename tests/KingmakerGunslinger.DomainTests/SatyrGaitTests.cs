using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SatyrGaitTests
    {
        private static SatyrGaitSample[] Stride() => Enumerable.Range(0, 41).Select(i => {
            float wave = (float)Math.Sin(i * Math.PI / 10);
            return new SatyrGaitSample { Time = i * .05, LiveLocomotion = true,
                LeftForward = wave * .18f, RightForward = -wave * .18f,
                LeftHeight = Math.Max(0, wave) * .08f, RightHeight = Math.Max(0, -wave) * .08f,
                LeftKneeForward = wave * .10f, RightKneeForward = -wave * .10f,
                LeftThighDegrees = Math.Abs(wave) * 30, RightThighDegrees = Math.Abs(wave) * 25 };
        }).ToArray();

        internal static void RequiresBilateralArticulatedStride()
        {
            Assertions.True(SatyrGaitPolicy.Stepping(Stride()), "Articulated alternating strides admitted.");
            var rigid = Stride();
            foreach (var s in rigid) { s.LeftForward = s.RightForward = 0; }
            Assertions.False(SatyrGaitPolicy.Stepping(rigid), "Translating planted feet cannot pass.");
            var hopping = Stride();
            foreach (var s in hopping) s.RightForward = s.LeftForward;
            Assertions.False(SatyrGaitPolicy.Stepping(hopping), "Synchronous foot motion is not alternating gait.");
            var oneLeg = Stride();
            foreach (var s in oneLeg) s.RightKneeForward = 0;
            Assertions.False(SatyrGaitPolicy.Stepping(oneLeg), "Both knees must articulate.");
            var noThigh = Stride();
            foreach (var s in noThigh) s.LeftThighDegrees = 0;
            Assertions.False(SatyrGaitPolicy.Stepping(noThigh), "Hoof motion without an articulated thigh rejected.");
        }

        internal static void RejectsInvalidAndRetiredEvidence()
        {
            Assertions.False(SatyrGaitPolicy.Stepping(null), "Missing evidence.");
            Assertions.False(SatyrGaitPolicy.Stepping(Stride().Take(4).ToArray()), "Sparse samples.");
            var retired = Stride(); retired[20].LiveLocomotion = false;
            Assertions.False(SatyrGaitPolicy.Stepping(retired), "Retired native handle cannot qualify travel.");
            var bad = Stride(); bad[20].LeftHeight = float.NaN;
            Assertions.False(SatyrGaitPolicy.Stepping(bad), "Nonfinite joint rejected.");
            var repeated = Stride(); repeated[20].Time = repeated[19].Time;
            Assertions.False(SatyrGaitPolicy.Stepping(repeated), "Duplicate frame time rejected.");
        }
    }
}
