using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Salamander's presentation-only tail gait (visual refinement,
    /// VIS-029). The ten original tail bones move only during the authored
    /// tail slap, so the coil used to glide rigidly behind the walking upper
    /// body. While the unit moves, and faintly while it idles, a horizontal
    /// wave now travels from the waist to the tip: each original tail point
    /// moves along its own in-plane normal by
    /// <c>amplitude * ramp(arc / length) * sin(2 pi arc / Wavelength - phase)</c>,
    /// where the ramp holds the waist still.
    ///
    /// <para>Heights never change, so the authored floor support stands. On the
    /// shipped mesh the offline check (lowest tail vertex 0.0361 against
    /// 0.0367 at rest; non-adjacent coils at least 6.3 cm apart against
    /// 12 cm at rest) fixed the wavelength and the moving amplitude; a shorter
    /// wavelength brought the coils within 1 cm. Commands, targets, reach,
    /// contact, timing and the slap itself are untouched. Plain arithmetic
    /// only; the domain tests compile it.</para>
    /// </summary>
    internal static class SalamanderTailGait
    {
        internal const int PointCount = 11;
        internal const float Wavelength = 2.6f;
        internal const float IdleAmplitude = 0.04f;
        internal const float MoveAmplitude = 0.12f;
        /// <summary>The share of the tail, from the waist, over which the wave fades in.</summary>
        internal const float WaistRamp = 0.3f;
        internal const float IdleHertz = 0.35f;
        /// <summary>Planar speed (m/s) at which the full moving amplitude applies.</summary>
        internal const float FullSpeed = 2f;
        /// <summary>Per-frame planar travel above which the view is taken to have been placed, not moved.</summary>
        internal const float PlacementMeters = 1.5f;
        /// <summary>Rate (1/s) at which the speed factor follows the measured speed.</summary>
        internal const float Response = 4f;
        /// <summary>Seconds over which the gait returns after the slap.</summary>
        internal const float BlendSeconds = 0.35f;

        internal static float SpeedFactor(float speed)
        {
            if (!Finite(speed) || speed <= 0) return 0;
            return Math.Min(1f, speed / FullSpeed);
        }

        internal static float Follow(float current, float target, float deltaTime)
        {
            if (!Finite(deltaTime) || deltaTime <= 0 || !Finite(target)) return current;
            return current + (target - current) * (1f - (float)Math.Exp(-Response * deltaTime));
        }

        internal static float Amplitude(float factor)
        {
            float f = Finite(factor) ? Math.Max(0f, Math.Min(1f, factor)) : 0f;
            return IdleAmplitude * (1f - f) + MoveAmplitude * f;
        }

        /// <summary>Advances the wave phase, kept in [0, 2 pi).</summary>
        internal static float Advance(float phase, float factor, float deltaTime)
        {
            if (!Finite(deltaTime) || deltaTime <= 0 || !Finite(phase)) return Finite(phase) ? phase : 0f;
            float f = Finite(factor) ? Math.Max(0f, Math.Min(1f, factor)) : 0f;
            double turn = 2 * Math.PI;
            double next = phase + turn * (IdleHertz + f * FullSpeed / Wavelength) * deltaTime;
            next %= turn;
            return (float)(next < 0 ? next + turn : next);
        }

        internal static float Ramp(float share)
        {
            float u = Math.Max(0f, Math.Min(1f, share / WaistRamp));
            return u * u * (3 - 2 * u);
        }

        /// <summary>
        /// The gait points for the eleven original rest points (x, y, z in
        /// the renderer's +Y-up space). The waist point and every height are
        /// returned unchanged; a point whose neighbours are vertically stacked
        /// is left at rest.
        /// </summary>
        internal static float[][] Points(float[][] rest, float amplitude, float phase)
        {
            if (rest == null || rest.Length != PointCount)
                throw new ArgumentException("Eleven original tail points are required.", "rest");
            foreach (float[] point in rest)
                if (point == null || point.Length != 3 || !Finite(point[0]) || !Finite(point[1]) || !Finite(point[2]))
                    throw new ArgumentException("Original tail points must be finite (x, y, z).", "rest");
            if (!Finite(amplitude) || !Finite(phase))
                throw new ArgumentException("The gait amplitude and phase must be finite.");
            var arc = new double[PointCount];
            for (int i = 1; i < PointCount; i++)
            {
                double dx = rest[i][0] - rest[i - 1][0], dy = rest[i][1] - rest[i - 1][1],
                    dz = rest[i][2] - rest[i - 1][2];
                arc[i] = arc[i - 1] + Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
            double length = arc[PointCount - 1];
            if (length <= 1e-6) throw new ArgumentException("The original tail has no length.", "rest");
            var result = new float[PointCount][];
            for (int i = 0; i < PointCount; i++)
            {
                float[] a = rest[Math.Max(i - 1, 0)], b = rest[Math.Min(i + 1, PointCount - 1)];
                double tx = b[0] - a[0], tz = b[2] - a[2];
                double m = Math.Sqrt(tx * tx + tz * tz);
                double offset = m < 1e-6 ? 0 : amplitude * Ramp((float)(arc[i] / length)) *
                    Math.Sin(2 * Math.PI * arc[i] / Wavelength - phase);
                result[i] = m < 1e-6 ? new[] { rest[i][0], rest[i][1], rest[i][2] } : new[] {
                    (float)(rest[i][0] + tz / m * offset), rest[i][1], (float)(rest[i][2] - tx / m * offset) };
            }
            return result;
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
