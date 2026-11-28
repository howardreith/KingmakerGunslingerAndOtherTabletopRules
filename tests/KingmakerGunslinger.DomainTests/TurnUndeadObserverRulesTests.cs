using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.TurnUndead;
using KmgLoaderCompatHarness;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The judgments the Turn Undead native probes apply (tools/LoaderCompatHarness/TurnUndeadObserverRules.cs,
    /// linked source), tested against constructed cases and against a retained native trace.
    /// </summary>
    internal static class TurnUndeadObserverRulesTests
    {
        private static TurnSaveRecord Save(bool passed, int natural, int dc = 15) =>
            new TurnSaveRecord("Will", dc, passed, natural);

        internal static void InitialAndRepeatSavesFollowTheRoll()
        {
            string outcome;
            // Eligible failure: the effect is required.
            Assertions.Equal(0, TurnUndeadObserverRules.JudgeInitial("skeleton", true, new[] { Save(false, 9) }, true, 15, out outcome).Count,
                "A failed save with the effect is correct.");
            Assertions.Equal("failed", outcome, "The outcome is the rolled failure.");
            Assertions.Equal(1, TurnUndeadObserverRules.JudgeInitial("skeleton", true, new[] { Save(false, 9) }, false, 15, out outcome).Count,
                "A failed save without the effect is a defect.");
            // Eligible success, including a natural 20 against a lowered total: no effect is correct.
            Assertions.Equal(0, TurnUndeadObserverRules.JudgeInitial("skeleton", true, new[] { Save(true, 20) }, false, 15, out outcome).Count,
                "A natural 20 that saves with no effect is correct, not a failure.");
            Assertions.Equal("passed", outcome, "The outcome is the rolled success.");
            List<string> effectAfterSuccess = TurnUndeadObserverRules.JudgeInitial("skeleton", true, new[] { Save(true, 20) }, true, 15, out outcome);
            Assertions.True(effectAfterSuccess.Count == 1 && effectAfterSuccess[0].Contains("after a successful save"),
                "An effect after a successful save is rejected.");
            Assertions.True(TurnUndeadObserverRules.CoverageGap("strong zombie lord", "passed", "failed")
                    .StartsWith(TurnUndeadObserverRules.CoveragePrefix, StringComparison.Ordinal) &&
                TurnUndeadObserverRules.CoverageGap("skeleton", "failed", "failed") == null,
                "A wanted outcome the dice did not produce is reported as missing coverage.");
            // Selective exclusion: an excluded unit rolls nothing and gains nothing.
            Assertions.Equal(0, TurnUndeadObserverRules.JudgeInitial("ally", false, new TurnSaveRecord[0], false, 15, out outcome).Count,
                "An excluded ally without a save is correct.");
            Assertions.Equal("excluded", outcome, "The outcome is exclusion.");
            Assertions.Equal(2, TurnUndeadObserverRules.JudgeInitial("ally", false, new[] { Save(false, 3) }, true, 15, out outcome).Count,
                "An excluded unit that rolled and was affected yields both problems.");
            // Wrong DC or a second roll.
            Assertions.Equal(1, TurnUndeadObserverRules.JudgeInitial("skeleton", true, new[] { Save(false, 9, 16) }, true, 15, out outcome).Count,
                "A wrong DC is a defect.");
            Assertions.Equal(1, TurnUndeadObserverRules.JudgeInitial("skeleton", true, new[] { Save(false, 9), Save(false, 4) }, true, 15, out outcome).Count,
                "Two initial saves are a defect.");

            bool recovered;
            Assertions.True(TurnUndeadObserverRules.JudgeRepeats("lord", true, new[] { Save(false, 9), Save(true, 20) }, false, out recovered).Count == 0 && recovered,
                "An early successful repeat save is recovery.");
            Assertions.Equal(1, TurnUndeadObserverRules.JudgeRepeats("lord", true, new[] { Save(false, 9), Save(true, 20) }, true, out recovered).Count,
                "An effect kept after a successful repeat save is a defect.");
            Assertions.True(TurnUndeadObserverRules.JudgeRepeats("lord", true, new[] { Save(false, 9), Save(false, 2) }, true, out recovered).Count == 0 && !recovered,
                "Failed repeat saves keep the effect.");
            Assertions.Equal(1, TurnUndeadObserverRules.JudgeRepeats("lord", true, new[] { Save(false, 9) }, false, out recovered).Count,
                "Losing the effect without a successful save is a defect.");
            Assertions.Equal(1, TurnUndeadObserverRules.JudgeRepeats("lord", true, new[] { Save(true, 18), Save(false, 2) }, false, out recovered).Count,
                "Saves after recovery are a defect.");
            Assertions.Equal(0, TurnUndeadObserverRules.JudgeRepeats("skeleton", false, new TurnSaveRecord[0], true, out recovered).Count,
                "A mindless unit keeps the effect without repeat saves.");
            Assertions.Equal(1, TurnUndeadObserverRules.JudgeRepeats("skeleton", false, new[] { Save(false, 4) }, true, out recovered).Count,
                "A mindless repeat save is a defect.");
        }

        internal static void MembershipIsExactPerIdentity()
        {
            var expected = new[] { "a", "b", "c" };
            Assertions.Equal(0, TurnUndeadObserverRules.MembershipProblems("list", expected, new[] { "c", "a", "b" }).Count,
                "Every identity once, in any order, passes.");
            List<string> duplicate = TurnUndeadObserverRules.MembershipProblems("list", expected, new[] { "a", "a", "c" });
            Assertions.True(duplicate.Count == 2 && duplicate.Any(p => p.Contains(" a ") && p.Contains("2 time")) &&
                duplicate.Any(p => p.Contains(" b ") && p.Contains("0 time")),
                "A duplicate standing in for a missing identity fails although the total count matches.");
            Assertions.Equal(1, TurnUndeadObserverRules.MembershipProblems("list", expected, new[] { "a", "b", "c", "d" }).Count,
                "An unexpected identity fails.");
            Assertions.Equal(3, TurnUndeadObserverRules.MembershipProblems("list", expected, new string[0]).Count,
                "An empty list fails for every identity.");
            Assertions.True(TurnUndeadObserverRules.OnlyInjected(new[] { "nested: injected by abandon-interrupt" }, "injected by abandon-interrupt") &&
                !TurnUndeadObserverRules.OnlyInjected(new[] { "nested: injected by abandon-interrupt", "pause flag: InvalidOperationException" }, "injected by abandon-interrupt") &&
                !TurnUndeadObserverRules.OnlyInjected(new string[0], "injected by abandon-interrupt"),
                "Only the harness's own injected failure is separable from a real cleanup failure.");
        }

        /// <summary>
        /// Run 20261128T115155Z failed under the old reader because the zombie lord's second repeat save rolled a natural
        /// 20 and correctly ended its effect. Its retained verdict stays FAIL; the current reader judges the same trace as
        /// valid early recovery.
        /// </summary>
        internal static void RetainedEarlyRecoveryTraceIsRecovery()
        {
            JObject document = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "docs", "evidence", "turn-undead", "native-runs.json")));
            JToken run = document["runs"].Single(r => ((string)r["sessionId"]).StartsWith("20261128T115155Z", StringComparison.Ordinal));
            JToken launch = run["launches"][0];
            Assertions.True((string)run["status"] == "FAIL" && (string)launch["verdict"] == "FAIL",
                "The retained verdict of 115155Z stays FAIL.");
            JToken rounds = launch["probes"]["first-turn"]["rounds"];
            List<TurnSaveRecord> repeats = rounds["lordRepeatSaves"].Select(s =>
                new TurnSaveRecord((string)s["type"], (int)s["dc"], (bool)s["passed"], (int?)s["natural"])).ToList();
            bool recovered;
            List<string> problems = TurnUndeadObserverRules.JudgeRepeats("zombie lord", true, repeats, (bool)rounds["lordFleeingAtEnd"], out recovered);
            Assertions.True(problems.Count == 0 && recovered && repeats.Count == 2 && repeats[1].Passed && repeats[1].Natural == 20,
                "The 115155Z repeat saves (a failure, then a natural 20 that ended the effect) are recovery: " + string.Join("; ", problems.ToArray()));
        }

        internal static void ResolvedIdentitiesClassifyAsTheirCreature()
        {
            var animated = TurnUndeadClassificationPolicy.Classify(new TurnUndeadUnitRecord("4050be4512d245f40bf9461074b672f4",
                TurnUndeadClassificationPolicy.SkeletonType, new string[0]));
            Assertions.True(animated.Mind == TurnUndeadMind.Mindless && !animated.OwnerReview,
                "The Animate Dead skeleton is mindless and no longer an owner-review case.");
            var priest = TurnUndeadClassificationPolicy.Classify(new TurnUndeadUnitRecord("fe662d20a0272bb4ea66bef675b4b52d",
                "e2fc4028eca4b40448e0339964d93b87", new string[0]));
            Assertions.True(priest.Mind == TurnUndeadMind.Mindless && !priest.OwnerReview, "A dread zombie cyclops priest is a zombie.");
            var champion = TurnUndeadClassificationPolicy.Classify(new TurnUndeadUnitRecord("209f78dd464984c40b1eb43fabdab5a0",
                TurnUndeadClassificationPolicy.SkeletonType, new string[0]));
            Assertions.Equal(TurnUndeadMind.Intelligent, champion.Mind, "The summoned greater skeletal champion stays intelligent.");
            var mage = TurnUndeadClassificationPolicy.Classify(new TurnUndeadUnitRecord("976d4937eb2620c4baf465bb6c7a00a7",
                TurnUndeadClassificationPolicy.SkeletonType, new string[0]));
            Assertions.True(mage.OwnerReview, "A skeleton without a tabletop identity remains an owner-review default.");
        }
    }
}
