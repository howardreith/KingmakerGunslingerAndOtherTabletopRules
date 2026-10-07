using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SalamanderTailAnimationTests
    {
        internal static void OnlyTheClosedOwnedPrototypeMayUseTheAction()
        {
            string[] exact = { SalamanderTailAnimationPolicy.UnitGuid,
                SalamanderTailAnimationPolicy.PrototypeName, SalamanderTailAnimationPolicy.Prefab,
                SalamanderTailAnimationPolicy.Spear };
            Func<string[], bool, bool, bool, bool> permits = (row, enabled, player, bones) =>
                SalamanderTailAnimationPolicy.PermitsPrototype(enabled, row[0], row[1], row[2], row[3], player, bones);
            Assertions.True(permits(exact, true, true, true), "Exact request-local prototype only.");
            for (int mask = 0; mask < 7; mask++)
                Assertions.False(permits(exact, (mask & 1) != 0, (mask & 2) != 0, (mask & 4) != 0),
                    "All module/player/bone ownership operands are required.");
            for (int index = 0; index < exact.Length; index++)
            foreach (string other in new[] { null, "", "foreign", "KMG_Summoning_Unit_Salamander", exact[index].ToUpperInvariant() })
            {
                string[] changed = (string[])exact.Clone();
                changed[index] = other;
                Assertions.False(permits(changed, true, true, true), "Native, published and cross-donor identities rejected.");
            }
        }

        internal static void ExactlyTenOriginalTailDriversAreAllowed()
        {
            string[] names = SalamanderTailAnimationPolicy.TailNames;
            Assertions.Equal(10, names.Length, "Bounded original tail, not arbitrary limbs.");
            Assertions.True(SalamanderTailAnimationPolicy.ExactTailNames(names), "Closed ordered set.");
            foreach (string[] changed in new[] { null, new string[0], names.Reverse().ToArray(),
                names.Take(9).ToArray(), names.Concat(new[] { "Head" }).ToArray(),
                names.Select((name, i) => i == 1 ? names[0] : name).ToArray() })
                Assertions.False(SalamanderTailAnimationPolicy.ExactTailNames(changed), "Missing/foreign/duplicate/reordered drivers rejected.");
            names[0] = "native";
            Assertions.True(SalamanderTailAnimationPolicy.ExactTailNames(SalamanderTailAnimationPolicy.TailNames),
                "Callers cannot mutate the closed list.");
        }

        internal static void NativeClockAndAcknowledgedClipProduceOneAct()
        {
            object handle = new object();
            var player = new SalamanderTailPlayback(handle);
            float clip;
            foreach (float time in new[] { 0f, .1f, .3f, .3f, .59f })
            {
                Assertions.True(player.Prepare(handle, time, false, true, out clip), "Monotonic native clock; repeats model pause.");
                Assertions.False(player.Sampled(handle, clip, true), "No event before authored act time.");
            }
            Assertions.True(player.Prepare(handle, .6f, false, true, out clip), "The native clock crosses the act marker.");
            Assertions.False(player.Acted, "Preparing time is not evaluating the clip.");
            Assertions.True(player.Sampled(handle, clip, true), "The exact evaluated state permits one event.");
            Assertions.False(player.Sampled(handle, clip, true), "Duplicate acknowledgement cannot replay.");
            foreach (float time in new[] { .6f, .8f, 1.4f, 2f })
            {
                Assertions.True(player.Prepare(handle, time, false, true, out clip), "Clock can run at native accelerated speed.");
                Assertions.False(player.Sampled(handle, clip, true), "No repeated act.");
                Assertions.True(clip <= SalamanderTailAnimationPolicy.Duration, "Clip time clamps only at its own end.");
            }
            Assertions.True(player.Acted && player.Complete, "Native action can release after recovery.");
        }

        internal static void FailedClipEvaluationCannotBecomeAnAct()
        {
            object handle = new object();
            foreach (bool ready in new[] { false, true })
            {
                var player = new SalamanderTailPlayback(handle);
                float clip;
                bool prepared = player.Prepare(handle, .7f, false, ready, out clip);
                Assertions.Equal(ready, prepared, "Clip must actually be ready.");
                Assertions.False(player.Sampled(handle, clip, false), "Missing evaluation never fires a synthetic action.");
                Assertions.True(player.Closed && !player.Acted, "Failure closes this playback.");
            }
            var mismatched = new SalamanderTailPlayback(handle);
            float pending;
            Assertions.True(mismatched.Prepare(handle, .6f, false, true, out pending), "Pending exact sample.");
            Assertions.False(mismatched.Sampled(handle, .7f, true), "A different sample cannot acknowledge it.");
            Assertions.True(mismatched.Closed && !mismatched.Acted, "Mismatched state fails closed.");
        }

        internal static void InvalidOrRewoundNativeTimeFailsClosed()
        {
            object handle = new object();
            foreach (float bad in new[] { -.1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                var player = new SalamanderTailPlayback(handle);
                float sample;
                Assertions.False(player.Prepare(handle, bad, false, true, out sample), "Invalid native clock.");
                Assertions.True(player.Closed && !player.Acted, "No fallback event.");
            }
            var rewind = new SalamanderTailPlayback(handle);
            float clip;
            rewind.Prepare(handle, .3f, false, true, out clip);
            rewind.Sampled(handle, clip, true);
            Assertions.False(rewind.Prepare(handle, .2f, false, true, out clip), "One handle cannot rewind.");
            Assertions.True(rewind.Closed && !rewind.Acted, "Replay must use a new native handle.");
        }

        internal static void InterruptionAndForeignHandlesDoNotLeakEvents()
        {
            object handle = new object(), other = new object();
            var player = new SalamanderTailPlayback(handle);
            float clip;
            Assertions.False(player.Prepare(other, .6f, false, true, out clip), "Foreign handle cannot use this player.");
            player.Close(other);
            Assertions.False(player.Closed, "Foreign stop cannot cancel this owner.");
            player.Prepare(handle, .59f, false, true, out clip);
            player.Sampled(handle, clip, true);
            Assertions.False(player.Prepare(handle, .6f, true, true, out clip), "Native interruption stops before the event.");
            Assertions.True(player.Closed && !player.Acted, "No damage trigger after interruption.");
            Assertions.False(player.Prepare(handle, .7f, false, true, out clip), "Closed playback cannot revive.");
            var next = new SalamanderTailPlayback(other);
            next.Prepare(other, .6f, false, true, out clip);
            Assertions.True(next.Sampled(other, clip, true), "A distinct later native command owns a new event.");
        }
    }
}

