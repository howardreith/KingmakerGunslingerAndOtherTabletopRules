using System;
using System.Collections.Generic;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Repair sprint R2 LIFE-02: the shared Bone/Ice Devil fear aura is called
    /// for every unit inside on every tick, so the exact source's memory on
    /// the victim is the only thing that turns presence into one evaluation.
    /// </summary>
    internal static class SummonPlaytestR2Tests
    {
        // Drives the real admission and memory-duration policy through the
        // native callback cadence: one call per tick for a stationary unit.
        // memoryAccepted models whether the victim's buff collection keeps
        // the marker (descriptor immunity refused the old fear-context one).
        private static int Evaluations(int ticks, double secondsPerTick,
            bool memoryAccepted, bool alwaysPasses)
        {
            double rememberedUntil = double.NegativeInfinity;
            int evaluations = 0;
            for (int tick = 0; tick < ticks; tick++)
            {
                double now = tick * secondsPerTick;
                FearAuraExposure admission = InfernalRules35.FearAuraAdmission(
                    true, false, true, true, now < rememberedUntil);
                if (admission != FearAuraExposure.Evaluate) continue;
                evaluations++;
                double seconds = InfernalRules35.FearAuraMemorySeconds(
                    alwaysPasses, alwaysPasses ? 0 : 3);
                if (memoryAccepted) rememberedUntil = now + seconds;
            }
            return evaluations;
        }

        internal static void StationaryPairEvaluatesOnceWhenMemoryIsKept()
        {
            // Five seconds of 60 Hz ticks next to one aura source.
            Assertions.Equal(300, Evaluations(300, 1d / 60d, false, true),
                "A refused marker leaves the gate open: every tick saves again (the reported spam).");
            Assertions.Equal(1, Evaluations(300, 1d / 60d, true, true),
                "A kept marker turns a stationary successful save into one evaluation.");
            Assertions.Equal(1, Evaluations(300, 1d / 60d, true, false),
                "A failed save is not re-rolled while its rolled fear duration lasts.");
            // Three-round failure memory, then legitimate re-exposure.
            Assertions.Equal(2, Evaluations(1200, 1d / 60d, true, false),
                "After the failed-save memory ends, a unit still inside is legitimately exposed again.");
        }

        internal static void AdmissionIsSourceSpecificAndKeepsNativeEligibility()
        {
            Assertions.Equal(FearAuraExposure.Evaluate,
                InfernalRules35.FearAuraAdmission(true, false, true, true, false),
                "A live unit inside, ally or enemy, is evaluated: no enemy-only rule.");
            Assertions.Equal(FearAuraExposure.Remembered,
                InfernalRules35.FearAuraAdmission(true, false, true, true, true),
                "This source's memory stops another evaluation.");
            Assertions.Equal(FearAuraExposure.NotOwner,
                InfernalRules35.FearAuraAdmission(false, false, true, true, false),
                "Only the configured creature's own aura evaluates.");
            Assertions.Equal(FearAuraExposure.Self,
                InfernalRules35.FearAuraAdmission(true, true, true, true, false),
                "The source never saves against its own aura.");
            Assertions.Equal(FearAuraExposure.Ineligible,
                InfernalRules35.FearAuraAdmission(true, false, false, true, false),
                "Dead or destroyed units are not evaluated.");
            Assertions.Equal(FearAuraExposure.Ineligible,
                InfernalRules35.FearAuraAdmission(true, false, true, false, false),
                "An ended area evaluates nobody.");

            // Memory is keyed by source: one source remembering a victim does
            // not stop a second, distinct source.
            var remembered = new HashSet<string> { "ice-devil-1|nalfeshnee" };
            Func<string, string, FearAuraExposure> admit = (source, victim) =>
                InfernalRules35.FearAuraAdmission(true, false, true, true,
                    remembered.Contains(source + "|" + victim));
            Assertions.Equal(FearAuraExposure.Remembered, admit("ice-devil-1", "nalfeshnee"),
                "The remembering source is gated.");
            Assertions.Equal(FearAuraExposure.Evaluate, admit("ice-devil-2", "nalfeshnee"),
                "A second Ice Devil stays independent.");
            Assertions.Equal(FearAuraExposure.Evaluate, admit("bone-devil-1", "nalfeshnee"),
                "A Bone Devil stays independent.");
        }

        // LIFE-01 observer: execution is the native act boundary, never the
        // queue round or an IsActed value read afterwards.
        internal static void ActionTimingRejectsQueuedButLaterOrNeverActed()
        {
            const int cast = 2;
            var queuedNowActedNext = new SummonCommandTiming { Sequence = 1, QueuedRound = cast,
                StartedRound = cast + 1, ActedRound = cast + 1, ActedGameSeconds = 18, ResultAtAct = "Success" };
            Assertions.Equal(SummonFirstActionDisposition.ActedLater,
                SummonActionTimingPolicy.Classify(new[] { queuedNowActedNext }, cast),
                "A command queued in the cast round that acts in the next round is not a cast-round action.");
            var neverActed = new SummonCommandTiming { Sequence = 1, QueuedRound = cast, StartedRound = cast };
            Assertions.Equal(SummonFirstActionDisposition.QueuedNeverActed,
                SummonActionTimingPolicy.Classify(new[] { neverActed }, cast),
                "A queued or started command that never reaches its act boundary is no action.");
            var interrupted = new SummonCommandTiming { Sequence = 1, QueuedRound = cast, StartedRound = cast,
                EndedRound = cast, ResultAtEnd = "Interrupt" };
            Assertions.Equal(SummonFirstActionDisposition.QueuedNeverActed,
                SummonActionTimingPolicy.Classify(new[] { interrupted }, cast),
                "An interrupted command that never acted is no action.");
            Assertions.Equal(SummonFirstActionDisposition.NoCommand,
                SummonActionTimingPolicy.Classify(new SummonCommandTiming[0], cast),
                "No command at all is reported as such.");
        }

        internal static void ActionTimingAcceptsTheFirstCastRoundAction()
        {
            const int cast = 2;
            // A move that acts, then an attack that misses: the attack's command
            // result is still Success, and the first act boundary decides.
            var move = new SummonCommandTiming { Sequence = 1, QueuedRound = cast, StartedRound = cast,
                ActedRound = cast, ActedGameSeconds = 12.1, ResultAtAct = "Success" };
            var missedAttack = new SummonCommandTiming { Sequence = 2, QueuedRound = cast, StartedRound = cast,
                ActedRound = cast, ActedGameSeconds = 13.4, ResultAtAct = "Success" };
            Assertions.Equal(SummonFirstActionDisposition.ActedInCastRound,
                SummonActionTimingPolicy.Classify(new[] { missedAttack, move }, cast),
                "A cast-round act boundary with a completed result is a legal action, hit or miss.");
            Assertions.True(ReferenceEquals(move, SummonActionTimingPolicy.FirstAction(new[] { missedAttack, move })),
                "The first legal action is the earliest act boundary, not the queue order.");
            var failed = new SummonCommandTiming { Sequence = 3, QueuedRound = cast, StartedRound = cast,
                ActedRound = cast, ActedGameSeconds = 11, ResultAtAct = "Fail" };
            Assertions.False(SummonActionTimingPolicy.IsLegalAction(failed),
                "An act boundary whose result is Fail is recorded but is not counted as a legal action.");
        }

        internal static void MemoryDurationsKeepTheEstablishedContract()
        {
            Assertions.Equal(86400d, InfernalRules35.FearAuraMemorySeconds(true, 0),
                "A successful save keeps the established 24-hour source memory (unchanged by R2).");
            Assertions.Equal(6d, InfernalRules35.FearAuraMemorySeconds(false, 1),
                "A one-round fear remembers one round.");
            Assertions.Equal(36d, InfernalRules35.FearAuraMemorySeconds(false, 6),
                "A six-round fear remembers six rounds.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                InfernalRules35.FearAuraMemorySeconds(false, 0),
                "A failed save always has a rolled duration.");
        }
    }
}
