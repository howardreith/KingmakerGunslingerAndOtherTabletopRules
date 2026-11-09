using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonPlaytestR6Tests
    {
        private static string Source(string relative)
        {
            return File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "src", "KingmakerGunslinger", relative));
        }

        private static int Count(string text, string value)
        {
            return text.Split(new[] { value }, StringSplitOptions.None).Length - 1;
        }

        public static void AidCastStyleSurvivesTargetRewriting()
        {
            Assertions.Equal("Omni", SummonPlaytestR6Policy.ChooseCastStyle("Touch", new[] { "Touch", "Omni", "Self" }), "Native Touch is rewritten for a self target, so a stable style replaces it.");
            Assertions.Equal("Omni", SummonPlaytestR6Policy.ChooseCastStyle("Omni", new[] { "Omni", "Point" }), "A stable, playable current style is kept.");
            Assertions.Equal("Self", SummonPlaytestR6Policy.ChooseCastStyle("Touch", new[] { "Self", "Touch" }), "The next stable playable style is used.");
            Assertions.Equal(null, SummonPlaytestR6Policy.ChooseCastStyle("Touch", new[] { "Touch", "SelfTouch" }), "Only target-rewritten styles give no choice.");
            Assertions.Equal(null, SummonPlaytestR6Policy.ChooseCastStyle("Immediate", new[] { "Immediate" }), "Immediate skips the animation.");
        }

        public static void RedcapBindingRequiresKmgSummonContext()
        {
            string redcap = SummonPlaytestR6Policy.RedcapSummonedUnitGuid;
            Assertions.True(SummonPlaytestR6Policy.RedcapBindingAllowed(redcap, "KMG_Summoning_NativeOption_SM_Tier5_Redcap_One", true), "A KMG ability that spawns this unit binds.");
            Assertions.False(SummonPlaytestR6Policy.RedcapBindingAllowed(redcap, "SummonMonsterVSingle", true), "The base game's own summon does not bind.");
            Assertions.False(SummonPlaytestR6Policy.RedcapBindingAllowed(redcap, null, true), "A summon with no recoverable context keeps native behavior.");
            Assertions.False(SummonPlaytestR6Policy.RedcapBindingAllowed(redcap, "KMG_Summoning_NativeOption_Other", false), "A KMG ability that does not spawn this unit does not bind.");
            Assertions.False(SummonPlaytestR6Policy.RedcapBindingAllowed("00000000000000000000000000000000", "KMG_Summoning_X", true), "Another unit does not bind.");
            string binding = Source(Path.Combine("Summoning", "RedcapScytheAnimationR6.cs"));
            Assertions.True(binding.Contains("UnityEngine.Object.Instantiate(hand)") && binding.Contains("UnityEngine.Object.Instantiate(_native)"), "The action and set are private copies.");
            Assertions.False(binding.Contains("VisualParameters") || binding.Contains("m_VisualParameters"), "The shared scythe's presentation is not modified.");
            Assertions.True(binding.Contains("private void OnDestroy()") && binding.Contains("manager.AnimationSet = _native"), "The native set is restored and owned copies destroyed.");
        }

        public static void ObserverRejectsMissingCastGesture()
        {
            var valid = new[] { new R6CommandFrame { Started = true, Acted = false, Action = "Cast", Clip = "Omni_casting" } };
            string[] clips = { "Omni_casting" };
            Assertions.True(SummonPlaytestR6Policy.CastGestureObserved(valid, "Cast", clips, 1, false), "The clip during the casting interval passes.");
            var idle = new[] { new R6CommandFrame { Started = true, Acted = false, Action = "LocoMotion", Clip = null } };
            Assertions.False(SummonPlaytestR6Policy.CastGestureObserved(idle, "Cast", clips, 1, false), "An idle-only trace fails.");
            var afterAct = new[] { new R6CommandFrame { Started = true, Acted = true, Action = "Cast", Clip = "Omni_casting" } };
            Assertions.False(SummonPlaytestR6Policy.CastGestureObserved(afterAct, "Cast", clips, 1, false), "A clip only after the act fails.");
            Assertions.False(SummonPlaytestR6Policy.CastGestureObserved(valid, "Cast", clips, 0, false), "Missing execution fails.");
            Assertions.False(SummonPlaytestR6Policy.CastGestureObserved(valid, "Cast", clips, 2, false), "A duplicate application fails.");
            Assertions.False(SummonPlaytestR6Policy.CastGestureObserved(valid, "Cast", clips, 1, true), "Cancellation fails.");
        }

        public static void ObserverRequiresScytheClipAtTheRealRoll()
        {
            var frames = new[] { new R6CommandFrame { Clip = "Attack_with_scythe_01", GameFrame = 100 }, new R6CommandFrame { Clip = "LegAttack_01", GameFrame = 200 } };
            Assertions.True(SummonPlaytestR6Policy.ScytheSwingObserved(frames, new[] { 101 }, 2), "The clip at the roll passes.");
            Assertions.False(SummonPlaytestR6Policy.ScytheSwingObserved(frames, new[] { 150 }, 2), "A clip elsewhere in the capture fails.");
            Assertions.False(SummonPlaytestR6Policy.ScytheSwingObserved(frames, new int[0], 2), "No real scythe roll fails.");
            Assertions.False(SummonPlaytestR6Policy.ScytheSwingObserved(new[] { frames[1] }, new[] { 200 }, 2), "Only a kick clip fails.");
        }

        public static void ArrivalRimsEndWithoutTouchingOtherEffects()
        {
            Assertions.True(SummonGlowPolicy.IsArrivalBuff("50d51854cf6a3434d96a87d050e1d09a"), "Monster IV-VI arrival is covered.");
            Assertions.True(SummonGlowPolicy.IsArrivalBuff("2c3b98e56c1b4e9479f692d7ef95b2d2"), "Ally I-III arrival is covered.");
            Assertions.False(SummonGlowPolicy.IsArrivalBuff("706c182e86d9be848b59ddccca73d13e"), "The persistent visual buff keeps its own handling.");
            Assertions.True(SummonGlowPolicy.OutlastsArrival(true, .5f) && SummonGlowPolicy.OutlastsArrival(false, 30f), "Looping or long rims end after the arrival.");
            Assertions.False(SummonGlowPolicy.OutlastsArrival(false, 2f), "A brief rim is left to finish itself.");
            Assertions.Equal(1, SummonGlowPolicy.ActiveGenericRims("SummonedCreatureVisual:1/2/0/0|SummonedCreatureSpawnMonsterIV-VI:1/0/1/0|MephitFireVisualBuff:2/1/2/1"), "Only generic rims count; intrinsic ones do not.");
            Assertions.Equal(0, SummonGlowPolicy.ActiveGenericRims("NaturalInvisibilityBuff:1/0/1/0"), "An invisibility effect is not generic.");
            string patch = Source(Path.Combine("Summoning", "ExpandedSummoningSummonGlowPatch.cs"));
            Assertions.False(patch.Contains("RemoveFact") || patch.Contains("Renderer") , "The buff stays and no renderer is stripped.");
        }

        // The hook is the risk: a per-target action or precast hook would
        // repeat the cue or sound on a cancelled cast.
        public static void RetirementStopsOnlyUnfinishedOwnedPlayback()
        {
            var now = new DateTime(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);
            var unfinished = new SummonCastCueRecord { EventName = "InspireCourage", Ability = "blast", CasterId = "caster", Utc = now, Frame = 1, PlayingId = 861 };
            var completed = new SummonCastCueRecord { PlayingId = 700, EndUtc = now };
            var failing = new SummonCastCueRecord { PlayingId = 5 };
            var later = new SummonCastCueRecord { PlayingId = 6 };
            var stopped = new System.Collections.Generic.List<uint>();
            Action<uint> engine = id => { if (id == 5) throw new InvalidOperationException("no engine"); stopped.Add(id); };
            int requested = SummonCastCueLifecycle.Retire(new[] { unfinished, completed, failing, later }, "view-destroyed", () => now, engine);
            Assertions.Equal(2, requested, "Only stops that returned count as requested.");
            Assertions.True(stopped.SequenceEqual(new uint[] { 861, 6 }), "Only unfinished owned playback is stopped; one failure does not skip the next, and unrelated IDs are never named.");
            Assertions.Equal(SummonCastCueLifecycle.StopRequested, unfinished.StopResult, "A returned void call is recorded as a request.");
            Assertions.True(failing.StopResult.StartsWith("failed:", StringComparison.Ordinal) && !failing.StopRequestedUtc.HasValue, "A failed stop stays visible.");
            Assertions.Equal(0, completed.StopAttempts, "A completed playback is left alone.");
            Assertions.False(unfinished.EndUtc.HasValue, "A stop request does not manufacture completion.");
            int repeat = SummonCastCueLifecycle.Retire(new[] { unfinished, completed, failing, later }, "entity-retired", () => now, engine);
            Assertions.Equal(0, repeat, "Repeated retirement does nothing.");
            Assertions.Equal(2, stopped.Count, "No playback is stopped twice.");
            Assertions.Equal("view-destroyed", unfinished.StopReason, "The first retirement reason is kept.");
            string cue = Source(Path.Combine("Summoning", "SummonAbilityCastCue.cs"));
            Assertions.True(cue.Contains("AkSoundEngine.StopPlayingID(id)") && cue.Contains("SummonCastCueLifecycle.Retire"), "Production stops by playing ID through the lifecycle.");
            Assertions.False(cue.Contains("ExecuteActionOnEvent") || cue.Contains("StopAll") || cue.Contains("AddComponent<AkGameObj>"), "No event-wide or global stop and no duplicate sound registration.");
            Assertions.False(cue.Contains("EndUtc = DateTime") && !cue.Contains("OnEnded"), "Only the end-of-event callback completes a record.");
        }

        public static void ObserverRequiresARealStopAndEnd()
        {
            var retired = new DateTime(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);
            Func<SummonCastCueRecord> good = () => new SummonCastCueRecord { PlayingId = 861, StopResult = SummonCastCueLifecycle.StopRequested,
                StopRequestedUtc = retired.AddMilliseconds(5), EndUtc = retired.AddMilliseconds(120) };
            Assertions.True(SummonPlaytestR6Policy.CueRetirementObserved(good(), retired, true, true, 861, 1d), "Stop request and end after retirement pass.");
            Assertions.False(SummonPlaytestR6Policy.CueRetirementObserved(good(), retired, false, true, 861, 1d), "The real command must have finished.");
            Assertions.False(SummonPlaytestR6Policy.CueRetirementObserved(good(), retired, true, false, 861, 1d), "The cue must be active when retirement begins.");
            Assertions.False(SummonPlaytestR6Policy.CueRetirementObserved(good(), retired, true, true, 900, 1d), "It must be the same owned playing ID.");
            var ended = good(); ended.EndUtc = retired.AddSeconds(-1);
            Assertions.False(SummonPlaytestR6Policy.CueRetirementObserved(ended, retired, true, true, 861, 1d), "A negative retirement-to-end interval fails.");
            var slow = good(); slow.EndUtc = retired.AddSeconds(4.6);
            Assertions.False(SummonPlaytestR6Policy.CueRetirementObserved(slow, retired, true, true, 861, 1d), "A cue that plays on fails.");
            var noStop = good(); noStop.StopResult = null; noStop.StopRequestedUtc = null;
            Assertions.False(SummonPlaytestR6Policy.CueRetirementObserved(noStop, retired, true, true, 861, 1d), "A short duration without a stop request fails.");
            var noEnd = good(); noEnd.EndUtc = null;
            Assertions.False(SummonPlaytestR6Policy.CueRetirementObserved(noEnd, retired, true, true, 861, 1d), "A request without a native end fails.");
        }

        public static void ObserverRequiresRealLanternExecution()
        {
            Assertions.True(SummonPlaytestR6Policy.AbilityExecutionObserved(true, true, false, 1, "Success"), "A completed execution passes, hit or miss.");
            Assertions.False(SummonPlaytestR6Policy.AbilityExecutionObserved(true, true, true, 1, "Success"), "Cancellation fails.");
            Assertions.False(SummonPlaytestR6Policy.AbilityExecutionObserved(true, true, false, 1, "Interrupt"), "An interrupted finish fails.");
            Assertions.False(SummonPlaytestR6Policy.AbilityExecutionObserved(false, false, false, -1, "None"), "A timeout fails.");
            Assertions.False(SummonPlaytestR6Policy.AbilityExecutionObserved(true, false, false, 1, "Success"), "Missing execution end fails.");
            Assertions.False(SummonPlaytestR6Policy.AbilityExecutionObserved(true, true, false, -1, "Success"), "A command that never acted fails.");
        }

        public static void CastCuesFireOncePerExecution()
        {
            string cue = Source(Path.Combine("Summoning", "SummonAbilityCastCue.cs"));
            Assertions.True(cue.Contains("IAbilityOnCastLogic"), "The cue uses the native once-per-execution cast hook.");
            Assertions.True(cue.Contains("Contexts.TryGetValue(context, out seen)"), "A repeated hook for one execution posts nothing.");
            Assertions.True(cue.Contains("AK_EndOfEvent"), "Each cue records its end-of-event callback.");
            Assertions.False(cue.Contains("releases it with that view"), "The unproven release claim is gone.");
            string vrock = Source(Path.Combine("Blueprints", "ExpandedSummoningSpecialBuilder.Vrock37.cs"));
            string trumpet = Source(Path.Combine("Blueprints", "ExpandedSummoningSpecialBuilder.Trumpet31Blast.cs"));
            Assertions.Equal(1, Count(vrock, "CreateInstance<SummonAbilityCastCue>"), "Only the Vrock screech carries a cue.");
            Assertions.Equal(1, Count(trumpet, "CreateInstance<SummonAbilityCastCue>"), "Only the Trumpet blast carries a cue.");
        }

        public static void LanternDonorChangeIsPerViewAndReversible()
        {
            string orb = Source(Path.Combine("Summoning", "LanternArchonOrbView.cs"));
            Assertions.True(orb.Contains("_donorBodies[index].sharedMesh = hidden ? _emptyMeshes[index] : _donorMeshes[index]"), "The swap is on this view's renderer and can be undone.");
            int release = orb.IndexOf("internal void Release()", StringComparison.Ordinal);
            Assertions.True(release > 0 && orb.IndexOf("SetDonorBodiesHidden(false)", release, StringComparison.Ordinal) > release, "Release restores the donor mesh.");
            Assertions.True(orb.IndexOf("DestroyImmediate(empty)", release, StringComparison.Ordinal) > release, "Release destroys the private empty meshes.");
        }

        public static void R6ObserverOnlyObservesAbilityProgress()
        {
            string observer = Source(Path.Combine("RuntimeTesting", "RuntimeTestRunner.SummonPlaytestR6Presentation.cs"));
            Assertions.False(observer.Contains("ExecutionProcess.Tick()") || observer.Contains("R5RunData("), "The R6 observer never advances an ability itself.");
            Assertions.True(observer.Contains("R6SelectedCells = { \"config\", \"redcap-scythe\", \"audio\" }"), "This checkpoint's native itinerary is explicit.");
            Assertions.True(observer.Contains("AkSoundEngine") == false, "Cues are exercised through the product component, not a direct post.");
        }
    }
}
