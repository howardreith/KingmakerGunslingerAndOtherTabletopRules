using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The Salamander's presentation-only tail gait (VIS-029): the waist and
    /// every height stay put, offsets are bounded and lie across the tail,
    /// and the clock and speed inputs fail safe.
    /// </summary>
    internal static class SalamanderTailGaitTests
    {
        // The shipped originalTailRest of salamander-human-mesh.json.
        private static readonly float[][] Rest = {
            new[] { 0f, .29f, 0f }, new[] { -.45f, .27f, -.4f }, new[] { -.78f, .25f, -.95f },
            new[] { -.5f, .23f, -1.52f }, new[] { .15f, .22f, -1.6f }, new[] { .72f, .21f, -1.27f },
            new[] { .82f, .19f, -.66f }, new[] { .46f, .17f, -.3f }, new[] { .2f, .155f, -.67f },
            new[] { .22f, .14f, -1.05f }, new[] { .52f, .13f, -1.12f } };

        internal static void TheWaveHoldsTheWaistAndHeights()
        {
            for (int step = 0; step < 16; step++)
            {
                float phase = (float)(2 * Math.PI * step / 16);
                float[][] points = SalamanderTailGait.Points(Rest, SalamanderTailGait.MoveAmplitude, phase);
                Assertions.Equal(SalamanderTailGait.PointCount, points.Length, "Eleven points come back.");
                Assertions.True(points[0].SequenceEqual(Rest[0]), "The waist point never moves.");
                for (int i = 0; i < points.Length; i++)
                {
                    Assertions.Equal(Rest[i][1], points[i][1], "Heights are unchanged, so floor support stands.");
                    float dx = points[i][0] - Rest[i][0], dz = points[i][2] - Rest[i][2];
                    Assertions.True(Math.Sqrt(dx * dx + dz * dz) <= SalamanderTailGait.MoveAmplitude + 1e-5,
                        "No point moves further than the amplitude.");
                    float[] a = Rest[Math.Max(i - 1, 0)], b = Rest[Math.Min(i + 1, Rest.Length - 1)];
                    float tx = b[0] - a[0], tz = b[2] - a[2];
                    Assertions.True(Math.Abs(dx * tx + dz * tz) <= 1e-4 * Math.Sqrt(tx * tx + tz * tz),
                        "Each point moves across the tail, along its in-plane normal.");
                }
            }
            float[][] still = SalamanderTailGait.Points(Rest, 0f, 1.3f);
            Assertions.True(still.Select((p, i) => p.SequenceEqual(Rest[i])).All(same => same),
                "Zero amplitude reproduces the authored rest exactly.");
            float[][] moving = SalamanderTailGait.Points(Rest, SalamanderTailGait.MoveAmplitude, 0f);
            Assertions.True(moving.Skip(4).Select((p, i) => Math.Sqrt(Math.Pow(p[0] - Rest[i + 4][0], 2) +
                    Math.Pow(p[2] - Rest[i + 4][2], 2))).Max() > .08,
                "At full amplitude the distal coil visibly moves.");
        }

        internal static void TheGaitInputsFailSafe()
        {
            Assertions.Equal(0f, SalamanderTailGait.SpeedFactor(0f), "Standing still is idle.");
            Assertions.Equal(0f, SalamanderTailGait.SpeedFactor(-1f), "Negative speed is idle.");
            Assertions.Equal(0f, SalamanderTailGait.SpeedFactor(float.NaN), "An invalid speed is idle.");
            Assertions.Equal(.5f, SalamanderTailGait.SpeedFactor(1f), "Half the full speed is half the gait.");
            Assertions.Equal(1f, SalamanderTailGait.SpeedFactor(9f), "Faster than full speed is clamped.");
            Assertions.Equal(SalamanderTailGait.IdleAmplitude, SalamanderTailGait.Amplitude(0f), "Idle sway.");
            Assertions.Equal(SalamanderTailGait.MoveAmplitude, SalamanderTailGait.Amplitude(1f), "Full gait.");
            Assertions.Equal(SalamanderTailGait.IdleAmplitude, SalamanderTailGait.Amplitude(float.NaN),
                "An invalid factor falls back to the idle sway.");
            Assertions.Equal(.25f, SalamanderTailGait.Follow(.25f, 1f, 0f), "No time, no change.");
            Assertions.Equal(.25f, SalamanderTailGait.Follow(.25f, float.NaN, .1f), "An invalid target is ignored.");
            float followed = SalamanderTailGait.Follow(0f, 1f, .1f);
            Assertions.True(followed > 0f && followed < 1f, "The speed factor eases toward the measured speed.");
            float phase = 0f;
            for (int i = 0; i < 400; i++)
            {
                float next = SalamanderTailGait.Advance(phase, 1f, .05f);
                Assertions.True(next >= 0f && next < 2 * Math.PI, "The phase stays in [0, 2 pi).");
                phase = next;
            }
            Assertions.Equal(1.5f, SalamanderTailGait.Advance(1.5f, 1f, 0f), "Paused or zero time holds the phase.");
            Assertions.Equal(1.5f, SalamanderTailGait.Advance(1.5f, 1f, float.NaN), "An invalid clock holds the phase.");
            Assertions.Equal(0f, SalamanderTailGait.Advance(float.NaN, 1f, .1f), "An invalid phase restarts at zero.");
            Assertions.True(SalamanderTailGait.Advance(0f, 1f, .1f) > SalamanderTailGait.Advance(0f, 0f, .1f),
                "Moving advances the wave faster than idling.");
        }

        internal static void MalformedRestPointsAreRefused()
        {
            Assertions.Throws<ArgumentException>(() => SalamanderTailGait.Points(null, .1f, 0f), "Null rest.");
            Assertions.Throws<ArgumentException>(() => SalamanderTailGait.Points(Rest.Take(10).ToArray(), .1f, 0f),
                "Ten points.");
            float[][] bad = Rest.Select(p => (float[])p.Clone()).ToArray();
            bad[3] = new[] { float.NaN, 0f, 0f };
            Assertions.Throws<ArgumentException>(() => SalamanderTailGait.Points(bad, .1f, 0f), "A non-finite point.");
            Assertions.Throws<ArgumentException>(() => SalamanderTailGait.Points(Rest, float.PositiveInfinity, 0f),
                "A non-finite amplitude.");
        }

        internal static void TheAttachmentDrivesOnlyItsOwnTailOutsideTheSlap()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning", "SalamanderHumanVisualAttachment.cs"));
            int start = source.IndexOf("private void LateUpdate()", StringComparison.Ordinal);
            Assertions.True(start > 0, "The gait runs in the attachment's LateUpdate.");
            string body = source.Substring(start, source.IndexOf("private static Vector3 Point(", start,
                StringComparison.Ordinal) - start);
            Assertions.True(body.Contains("if (TailAction.PlayingOwnedClip) { _gaitWeight = 0; return; }") &&
                body.Contains("Game.Instance.IsPaused") && body.Contains("!Live") &&
                body.Contains("_tail[bone].localPosition") && body.Contains("_tail[bone].localRotation") &&
                body.Contains("catch (Exception) { _tail = null; }"),
                "The gait stands aside for the slap and the pause, runs only while the body is live, " +
                "writes only the owned tail bones and stops itself on any failure.");
            Assertions.False(body.Contains("Body.bones") || body.Contains("_view.transform.position =") ||
                body.Contains("AnimationManager") || body.Contains("_player"),
                "It touches no native bone, unit position, animation manager or slap player.");
        }

        internal static void TheOwnedBodyStartsUndissolved()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning", "SalamanderHumanVisualAttachment.cs"));
            int clear = source.IndexOf("_material.DisableKeyword(\"DISSOLVE_ON\");", StringComparison.Ordinal);
            int zero = source.IndexOf("_material.SetFloat(\"_Dissolve\", 0f);", StringComparison.Ordinal);
            int adopt = source.IndexOf("AdoptByMaterialController(_view, Body, _material)", StringComparison.Ordinal);
            Assertions.True(clear > 0 && zero > clear && adopt > zero,
                "The owned clone drops the dissolve state before the material controller adopts its defaults.");
            Assertions.False(source.Contains("EnableKeyword(\"DISSOLVE_ON\")"),
                "Only the game's own dissolve animations turn the keyword back on.");
        }
    }
}
