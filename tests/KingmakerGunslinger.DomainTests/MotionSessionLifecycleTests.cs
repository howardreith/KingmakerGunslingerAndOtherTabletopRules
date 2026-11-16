using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The motion session's lifecycle, on the class the session itself runs:
    /// the pause it releases while an attack is sampled, its owned cleanup,
    /// and what happens when the runner completes with another result
    /// first. The game here is a pause flag; the calls are the ones the
    /// session makes, in the order it makes them.
    /// </summary>
    internal static class MotionSessionLifecycleTests
    {
        private sealed class Row
        {
            internal string Name;
            internal string Expected;
            internal string Observed;
            internal bool Passed;
        }

        private sealed class FakeGame
        {
            internal bool Paused;
            internal int Sets;
            internal bool SetterFails;
        }

        private static MotionSessionLifecycle<Row> Lifecycle(FakeGame game)
        {
            return new MotionSessionLifecycle<Row>(
                () => game.Paused,
                value =>
                {
                    if (game.SetterFails)
                        throw new InvalidOperationException("the pause would not change");
                    game.Paused = value;
                    game.Sets++;
                },
                (name, expected, observed, passed) => new Row
                {
                    Name = name, Expected = expected, Observed = observed, Passed = passed
                });
        }

        // The session's releases: each records that it ran and what the
        // pause was when it did.
        private static List<KeyValuePair<string, Action>> Releases(FakeGame game,
            List<string> ran, string failing = null)
        {
            var releases = new List<KeyValuePair<string, Action>>();
            foreach (string name in new[] { "actor-commands", "actor-dispose", "actor-blueprint" })
            {
                string current = name;
                releases.Add(new KeyValuePair<string, Action>(current, () =>
                {
                    ran.Add(current + "@paused=" + game.Paused);
                    if (current == failing)
                        throw new InvalidOperationException("the unit would not go");
                }));
            }
            return releases;
        }

        private static List<Row> OuterAssertions()
        {
            return new List<Row>
            {
                new Row
                {
                    Name = "no-save-writing-api", Expected = "none",
                    Observed = "a save write was observed", Passed = false
                }
            };
        }

        private const string OuterReason =
            "A request-scoped observation hook failed; original game behavior was preserved.";

        private static string Names(IEnumerable<Row> rows)
        {
            return string.Join(",", rows.Select(row => row.Name).ToArray());
        }

        private static Row Named(IEnumerable<Row> rows, string name)
        {
            return rows.Single(row => row.Name == name);
        }

        // Equip and settle with the pause held, then sample an attack.
        private static void SampleAnAttack(MotionSessionLifecycle<Row> lifecycle,
            int updates)
        {
            lifecycle.HoldOutsideSampling();
            lifecycle.HoldOutsideSampling();
            for (int update = 0; update < updates; update++)
                lifecycle.ReleaseForSampling();
        }

        internal static void PausedSamplingOuterAbortRestoresThePause()
        {
            var game = new FakeGame { Paused = true };
            MotionSessionLifecycle<Row> lifecycle = Lifecycle(game);

            // Before the first attack the session owns nothing.
            lifecycle.HoldOutsideSampling();
            Assertions.True(game.Paused && game.Sets == 0 && !lifecycle.PauseOwned,
                "The session touched the pause before sampling.");

            Assertions.True(lifecycle.ReleaseForSampling(),
                "Taking ownership of the pause was not reported.");
            Assertions.True(!game.Paused && lifecycle.PauseOwned && lifecycle.Sampling,
                "Sampling did not release the pause.");
            for (int update = 0; update < 6; update++)
                Assertions.False(lifecycle.ReleaseForSampling(),
                    "Ownership was reported a second time.");
            // The game pauses itself mid-sequence; the next sample lets time run again.
            game.Paused = true;
            Assertions.False(lifecycle.ReleaseForSampling(),
                "Ownership was reported a second time.");
            Assertions.True(!game.Paused, "A later sample left the game paused.");
            Assertions.Equal(8, lifecycle.SamplingUpdates, "The sampled updates were not counted.");
            Assertions.True(MotionOuterAbortDrill.Due(true, lifecycle.Ended,
                lifecycle.Sampling, lifecycle.PauseOwned, game.Paused,
                lifecycle.SamplingUpdates), "The drill's point was not reached.");

            // The runner completes with a result of its own. No further
            // update reaches the session.
            var ran = new List<string>();
            List<Row> outer = OuterAssertions();
            var diagnostics = new List<string> { OuterReason };
            var partial = new List<Row>
            {
                new Row { Name = "weapon-presentation-motion-evidence-exception", Passed = false }
            };
            MotionTermination termination = lifecycle.Terminate(
                "sample-native-attack-Pistol.Service", Releases(game, ran),
                () => null, () => new[] { "phase=3;case=1/6:Pistol.Service" },
                partial, outer, diagnostics);

            Assertions.True(termination.Performed, "The session was not ended.");
            Assertions.True(game.Paused, "The pause the session released was not restored.");
            Assertions.True(termination.PauseOwned && termination.Sampling &&
                termination.PausedAtTermination == false &&
                termination.PausedAfterwards == true && termination.PauseRestored,
                "The pause's state before and after ending is not recorded.");
            Assertions.Equal(1, lifecycle.PauseRestorations,
                "The pause was not restored exactly once.");
            Assertions.Equal("restore-pause,actor-commands,actor-dispose,actor-blueprint",
                string.Join(",", termination.Attempted.ToArray()),
                "The releases were not attempted pause first, then in order.");
            Assertions.Equal(
                "actor-commands@paused=True,actor-dispose@paused=True,actor-blueprint@paused=True",
                string.Join(",", ran.ToArray()),
                "The pause was not back in force before the other releases ran.");
            Assertions.True(termination.CleanupClean && termination.Failures.Count == 0,
                "A clean cleanup recorded a failure.");

            // The outer result: its own entry first and unchanged, then the
            // session's, then what ending it did.
            Assertions.Equal("no-save-writing-api," +
                "weapon-presentation-motion-evidence-exception," +
                MotionSessionLifecycle<Row>.TerminatedName + "," +
                MotionSessionLifecycle<Row>.PauseName + "," +
                MotionSessionLifecycle<Row>.CleanupName + "," +
                MotionSessionLifecycle<Row>.ResidueName, Names(outer),
                "The outer result does not hold its own failure, the partial observations and the termination record, in that order.");
            Assertions.Equal("a save write was observed", outer[0].Observed,
                "The outer failure's reason changed.");
            Assertions.Equal(5, termination.Attached, "The attached count is wrong.");
            Row ended = Named(outer, MotionSessionLifecycle<Row>.TerminatedName);
            Assertions.True(!ended.Passed && ended.Observed.Contains(
                "stage sample-native-attack-Pistol.Service;sampling=True;samplingUpdates=8"),
                "The termination does not name where the session was: " + ended.Observed);
            Row pause = Named(outer, MotionSessionLifecycle<Row>.PauseName);
            Assertions.True(pause.Passed && pause.Observed.StartsWith(
                "owned=True;pausedAtTermination=False;pausedAfterwards=True;",
                StringComparison.Ordinal),
                "The pause restoration is not its own passing record: " + pause.Observed);
            Assertions.True(Named(outer, MotionSessionLifecycle<Row>.CleanupName).Passed &&
                Named(outer, MotionSessionLifecycle<Row>.ResidueName).Passed,
                "A clean cleanup or an empty residue was recorded as failed.");
            Assertions.Equal(OuterReason, diagnostics[0], "The outer reason is no longer first.");
            Assertions.True(diagnostics.Count == 3 &&
                diagnostics[1].StartsWith("motionTermination=", StringComparison.Ordinal) &&
                diagnostics[2] == "motionObservation:phase=3;case=1/6:Pistol.Service",
                "The termination and the partial observations were not added after the outer reason.");

            // The status is the outer result's: only a pass is not kept.
            foreach (string status in new[] { "FAIL", "ERROR", "TIMEOUT", "AMBIGUOUS" })
                Assertions.Equal(status, MotionTermination.StatusAfter(status, "PASS", "FAIL"),
                    "A failing outer status was replaced.");
            Assertions.Equal("FAIL", MotionTermination.StatusAfter("PASS", "PASS", "FAIL"),
                "An ended session left a pass standing.");

            // An ended session does not release the pause again.
            Assertions.True(lifecycle.Ended && lifecycle.Terminated, "The session is not ended.");
            int sets = game.Sets;
            Assertions.False(lifecycle.ReleaseForSampling(), "An ended session sampled.");
            lifecycle.HoldOutsideSampling();
            Assertions.True(game.Paused && game.Sets == sets,
                "An ended session changed the pause.");
        }

        internal static void NormalCompletionAndRepeatedFinalizationDoNothingTwice()
        {
            // The session runs to its own end.
            var game = new FakeGame { Paused = true };
            MotionSessionLifecycle<Row> lifecycle = Lifecycle(game);
            SampleAnAttack(lifecycle, 96);
            Assertions.False(game.Paused, "Sampling did not release the pause.");
            lifecycle.HoldOutsideSampling();
            Assertions.True(game.Paused, "The pause was not held between attack sequences.");
            SampleAnAttack(lifecycle, 96);
            lifecycle.HoldOutsideSampling();

            var ran = new List<string>();
            Assertions.True(lifecycle.RunCleanup(Releases(game, ran)), "The cleanup did not run.");
            Assertions.True(game.Paused && ran.Count == 3 &&
                lifecycle.CleanupFailures.Count == 0,
                "The session's own cleanup did not leave the pause in force and release everything.");
            Assertions.False(lifecycle.RunCleanup(Releases(game, ran)),
                "The cleanup ran a second time.");
            Assertions.Equal(3, ran.Count, "A release ran twice.");
            lifecycle.MarkCompleted();

            // The runner then completes with the session's own result.
            List<Row> own = OuterAssertions();
            var diagnostics = new List<string> { "attackSampling=game was paused" };
            int sets = game.Sets;
            for (int pass = 0; pass < 2; pass++)
            {
                MotionTermination none = lifecycle.Terminate("motion-request-cleanup",
                    Releases(game, ran), () => "should not be read",
                    () => new[] { "should not be read" }, new List<Row>(), own, diagnostics);
                Assertions.False(none.Performed, "A completed session was ended again.");
                Assertions.Equal(0, none.Attached, "Something was attached twice.");
            }
            Assertions.True(own.Count == 1 && diagnostics.Count == 1 && ran.Count == 3 &&
                game.Sets == sets && !lifecycle.Terminated,
                "Finalizing a completed session added evidence or released something again.");
            Assertions.Equal("restore-pause,actor-commands,actor-dispose,actor-blueprint",
                string.Join(",", lifecycle.CleanupAttempted.ToArray()),
                "The release record grew after completion.");

            // An aborted session finalized twice.
            var second = new FakeGame { Paused = true };
            MotionSessionLifecycle<Row> aborted = Lifecycle(second);
            SampleAnAttack(aborted, 8);
            var secondRan = new List<string>();
            List<Row> outer = OuterAssertions();
            var outerDiagnostics = new List<string> { OuterReason };
            aborted.Terminate("sample-native-attack-Pistol.Duelist",
                Releases(second, secondRan), () => null, () => new string[0],
                new List<Row>(), outer, outerDiagnostics);
            int rows = outer.Count, lines = outerDiagnostics.Count;
            MotionTermination again = aborted.Terminate("sample-native-attack-Pistol.Duelist",
                Releases(second, secondRan), () => null, () => new string[0],
                new List<Row>(), outer, outerDiagnostics);
            Assertions.False(again.Performed, "An ended session was ended a second time.");
            Assertions.True(outer.Count == rows && outerDiagnostics.Count == lines &&
                secondRan.Count == 3 && aborted.PauseRestorations == 1 && second.Paused,
                "A second finalization released or recorded something again.");

            // The session's cleanup has run and its result is still
            // settling when the runner completes: nothing is released
            // again, and the pause is looked at once more.
            var third = new FakeGame { Paused = true };
            MotionSessionLifecycle<Row> settling = Lifecycle(third);
            SampleAnAttack(settling, 8);
            var thirdRan = new List<string>();
            settling.RunCleanup(Releases(third, thirdRan));
            third.Paused = false;
            List<Row> late = OuterAssertions();
            MotionTermination during = settling.Terminate("motion-request-cleanup",
                Releases(third, thirdRan), () => null, () => new string[0],
                new List<Row>(), late, new List<string>());
            Assertions.True(during.Performed && during.CleanupAlreadyRun &&
                thirdRan.Count == 3, "A cleanup that had run was run again.");
            Assertions.True(third.Paused && during.PauseRestored,
                "The pause was not looked at again after the cleanup had run.");
            Assertions.Equal(
                "restore-pause,actor-commands,actor-dispose,actor-blueprint,restore-pause",
                string.Join(",", during.Attempted.ToArray()),
                "The second look at the pause is not recorded.");
        }

        internal static void InitiallyRunningSessionOwnsNoPause()
        {
            // The session runs to its own end in a game that was never paused.
            var game = new FakeGame { Paused = false };
            MotionSessionLifecycle<Row> lifecycle = Lifecycle(game);
            lifecycle.HoldOutsideSampling();
            for (int update = 0; update < 96; update++)
                Assertions.False(lifecycle.ReleaseForSampling(),
                    "A running game was reported as a released pause.");
            lifecycle.HoldOutsideSampling();
            var ran = new List<string>();
            lifecycle.RunCleanup(Releases(game, ran));
            Assertions.True(!lifecycle.PauseOwned && !game.Paused && game.Sets == 0,
                "A session that found the game running changed the pause.");
            Assertions.Equal(3, ran.Count, "The releases did not run.");
            Assertions.Equal("actor-commands@paused=False", ran[0],
                "The session paused a game it had found running.");

            // The same, ended by the runner while sampling.
            var second = new FakeGame { Paused = false };
            MotionSessionLifecycle<Row> aborted = Lifecycle(second);
            SampleAnAttack(aborted, 24);
            Assertions.False(MotionOuterAbortDrill.Due(true, aborted.Ended, aborted.Sampling,
                aborted.PauseOwned, second.Paused, aborted.SamplingUpdates),
                "The drill fired without a released pause.");
            List<Row> outer = OuterAssertions();
            MotionTermination termination = aborted.Terminate(
                "sample-native-attack-Pistol.Service", Releases(second, new List<string>()),
                () => null, () => new string[0], new List<Row>(), outer, new List<string>());
            Assertions.True(termination.Performed && !termination.PauseOwned &&
                termination.PausedAtTermination == false &&
                termination.PausedAfterwards == false && termination.PauseRestored,
                "A game found running was not left running.");
            Assertions.True(!second.Paused && second.Sets == 0,
                "Ending a session that owned no pause changed the pause.");
            Row pause = Named(outer, MotionSessionLifecycle<Row>.PauseName);
            Assertions.True(pause.Passed && pause.Observed.StartsWith(
                "owned=False;pausedAtTermination=False;pausedAfterwards=False;",
                StringComparison.Ordinal),
                "The unowned pause is not reported as left alone: " + pause.Observed);

            // A game someone else paused, which the session never sampled
            // under, is not the session's to change either.
            var third = new FakeGame { Paused = false };
            MotionSessionLifecycle<Row> bystander = Lifecycle(third);
            SampleAnAttack(bystander, 8);
            bystander.HoldOutsideSampling();
            third.Paused = true;
            bystander.HoldOutsideSampling();
            bystander.RunCleanup(Releases(third, new List<string>()));
            Assertions.True(third.Paused && third.Sets == 0 && !bystander.PauseOwned,
                "The session changed a pause it did not own.");
        }

        internal static void CleanupFailureIsReportedAndTheAbortReasonKept()
        {
            var game = new FakeGame { Paused = true };
            MotionSessionLifecycle<Row> lifecycle = Lifecycle(game);
            SampleAnAttack(lifecycle, 12);
            var ran = new List<string>();
            List<Row> outer = OuterAssertions();
            var diagnostics = new List<string> { OuterReason };
            MotionTermination termination = lifecycle.Terminate(
                "sample-native-attack-Pistol.LastWord",
                Releases(game, ran, "actor-dispose"),
                () => "the disposable actor is still listed",
                () => { throw new InvalidOperationException("the records were gone"); },
                new List<Row>(), outer, diagnostics);

            // One release raised; the one after it was still attempted, and
            // the pause had already been restored.
            Assertions.Equal(3, ran.Count, "A release after the failing one was not attempted.");
            Assertions.Equal("actor-blueprint@paused=True", ran[2],
                "The release after the failing one did not run with the pause restored.");
            Assertions.True(game.Paused && termination.PauseRestored,
                "A failing release kept the pause from being restored.");
            Assertions.True(!termination.CleanupClean && termination.Failures.Count == 1 &&
                termination.Failures[0] == "actor-dispose: the unit would not go",
                "The failing release is not named with its reason.");

            Row cleanup = Named(outer, MotionSessionLifecycle<Row>.CleanupName);
            Assertions.True(!cleanup.Passed && cleanup.Observed ==
                "attempted=restore-pause,actor-commands,actor-dispose,actor-blueprint;" +
                "releaseFailures=actor-dispose: the unit would not go",
                "The cleanup failure is not reported: " + cleanup.Observed);
            Row residue = Named(outer, MotionSessionLifecycle<Row>.ResidueName);
            Assertions.True(!residue.Passed &&
                residue.Observed == "the disposable actor is still listed",
                "What remained is not reported: " + residue.Observed);
            Assertions.True(Named(outer, MotionSessionLifecycle<Row>.PauseName).Passed,
                "The pause's own record was failed by an unrelated release.");

            // The original cause is still the result's first entry, unchanged.
            Assertions.True(outer[0].Name == "no-save-writing-api" && !outer[0].Passed &&
                outer[0].Observed == "a save write was observed",
                "The outer failure was replaced or changed.");
            Assertions.Equal(OuterReason, diagnostics[0], "The outer reason was replaced.");
            Assertions.True(diagnostics.Any(line => line ==
                "motionObservation:unreadable=the records were gone"),
                "Unreadable partial observations were not reported as such.");
            Assertions.Equal("ERROR", MotionTermination.StatusAfter("ERROR", "PASS", "FAIL"),
                "A cleanup failure replaced the outer status.");

            // A pause that will not go back is a failure of its own and
            // does not stop the other releases.
            var stuck = new FakeGame { Paused = true };
            MotionSessionLifecycle<Row> second = Lifecycle(stuck);
            SampleAnAttack(second, 8);
            stuck.SetterFails = true;
            var stuckRan = new List<string>();
            List<Row> stuckOuter = OuterAssertions();
            MotionTermination unrestored = second.Terminate(
                "sample-native-attack-Revolver.Service", Releases(stuck, stuckRan),
                () => null, () => new string[0], new List<Row>(), stuckOuter,
                new List<string>());
            Assertions.Equal(3, stuckRan.Count,
                "A pause that would not restore kept the releases from being attempted.");
            Assertions.True(!unrestored.PauseRestored && unrestored.PausedAfterwards == false &&
                unrestored.Failures.Count == 1 && unrestored.Failures[0] ==
                    "restore-pause: the pause would not change",
                "An unrestored pause was reported as restored.");
            Assertions.False(Named(stuckOuter, MotionSessionLifecycle<Row>.PauseName).Passed,
                "An unrestored pause passed its own record.");
            Assertions.False(Named(stuckOuter, MotionSessionLifecycle<Row>.CleanupName).Passed,
                "A failed pause restoration passed the cleanup record.");
        }

        internal static void OuterAbortDrillIsItsOwnRequestAndFiresOnlyUnderAReleasedPause()
        {
            const string drill = MotionOuterAbortDrill.AttackSampling;
            Assertions.Equal(null, MotionOuterAbortDrill.Rejection(true, false, null, 0, false),
                "An ordinary motion request was refused.");
            Assertions.Equal(null, MotionOuterAbortDrill.Rejection(false, false, null, 3, false),
                "A request that does not ask for the drill was refused.");
            Assertions.Equal(null, MotionOuterAbortDrill.Rejection(true, true, drill, 1, true),
                "The drill request was refused.");
            Assertions.Equal(MotionOuterAbortDrill.Rejected,
                MotionOuterAbortDrill.Rejection(true, true, drill, 1, false),
                "A drill that would leave the game running was admitted.");
            Assertions.Equal(MotionOuterAbortDrill.Rejected,
                MotionOuterAbortDrill.Rejection(true, true, "attack", 1, true),
                "An unknown drill point was admitted.");
            Assertions.Equal(MotionOuterAbortDrill.Rejected,
                MotionOuterAbortDrill.Rejection(true, true, null, 1, true),
                "A drill parameter that is not a string was admitted.");
            Assertions.Equal(MotionOuterAbortDrill.Rejected,
                MotionOuterAbortDrill.Rejection(true, true, drill, 2, true),
                "The drill was admitted beside another parameter.");
            Assertions.Equal(MotionOuterAbortDrill.Rejected,
                MotionOuterAbortDrill.Rejection(false, true, drill, 1, true),
                "The drill was admitted for a scenario with no motion session.");

            int enough = MotionOuterAbortDrill.SamplingUpdatesBeforeAbort;
            Assertions.True(MotionOuterAbortDrill.Due(true, false, true, true, false, enough),
                "The drill did not fire at its point.");
            Assertions.False(MotionOuterAbortDrill.Due(false, false, true, true, false, enough),
                "The drill fired without being requested.");
            Assertions.False(MotionOuterAbortDrill.Due(true, true, true, true, false, enough),
                "The drill fired after the session had ended.");
            Assertions.False(MotionOuterAbortDrill.Due(true, false, false, true, false, enough),
                "The drill fired outside an attack sequence.");
            Assertions.False(MotionOuterAbortDrill.Due(true, false, true, false, false, enough),
                "The drill fired without an owned pause.");
            Assertions.False(MotionOuterAbortDrill.Due(true, false, true, true, true, enough),
                "The drill fired while the game was paused.");
            Assertions.False(MotionOuterAbortDrill.Due(true, false, true, true, false, enough - 1),
                "The drill fired before the attack had been sampled.");
        }
    }
}
