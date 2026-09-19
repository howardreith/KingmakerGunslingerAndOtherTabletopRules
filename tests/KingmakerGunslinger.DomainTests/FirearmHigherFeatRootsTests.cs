using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmHigherFeatRootsTests
    {
        internal static readonly string[] RootGuids = {
            "09c9e82965fb4334b984a1e9df3bd088",
            "31470b17e8446ae4ea0dacd6c5817d86",
            "7cf5edc65e785a24f9cf93af987d66b3",
            "f4201c85a991369408740c6888362e20" };
        internal static readonly string[] RootNames = { "GreaterWeaponFocus",
            "WeaponSpecialization", "GreaterWeaponSpecialization", "ImprovedCritical" };
        internal static readonly string[] Kinds = { "Pistol", "Musket", "Blunderbuss" };

        internal static void ExactTwelveCombinationPlanIsFixed()
        {
            Assertions.Equal(4, RootGuids.Length, "Exactly four higher roots.");
            Assertions.Equal(3, Kinds.Length, "Exactly three official firearm kinds.");
            var combos = RootNames.SelectMany(root => Kinds, (root, kind) => root + ":" + kind).ToArray();
            Assertions.Equal(12, combos.Length, "Exactly twelve root/weapon combinations derive from the fixed plan.");
            Assertions.Equal(12, combos.Distinct().Count(), "All twelve combinations are distinct.");
            Assertions.True(combos.Contains("GreaterWeaponSpecialization:Blunderbuss") &&
                combos.Contains("ImprovedCritical:Pistol"), "The plan includes boundary combinations.");
        }

        internal static void RootIdentitiesAreExact()
        {
            Assertions.True(RootGuids.All(value => value.Length == 32 &&
                    value.All(ch => (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f'))),
                "Every higher-feat root GUID is a full 32-character lowercase hex identity.");
            Assertions.Equal(4, RootGuids.Distinct().Count(), "Root GUIDs are distinct.");
        }

        internal static void ScenarioWiringIsComplete()
        {
            string root = Environment.CurrentDirectory;
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestScenarioCatalog.cs"));
            Assertions.True(catalog.Contains("disposable-firearm-higher-feat-roots"),
                "The scenario is registered in the production allowlist catalog.");
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains("RunFirearmHigherFeatRoots("),
                "The runner dispatches the higher-feat-roots scenario.");
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.FirearmHigherFeatRoots.cs"));
            foreach (string token in new[] {
                "StartWithoutAssigningStaticInstance", "SelectClass(fighter, false)",
                "ApplyClassMechanics", "ApplyLevelup", "controller.Cancel()",
                "TryCommitRoot(controller, descriptor, slot", "AddFact(BlueprintBootstrap.FirearmProficiency)",
                "ExtractSelectionItems", "pending.Count == 0", "verified == 12"
            })
                Assertions.True(scenario.Contains(token),
                    "The higher-feat-roots scenario lost a required real-selection step: " + token);
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(automation.Contains("'disposable-firearm-higher-feat-roots'"),
                "The PowerShell metadata table knows the scenario.");
        }

        private static Newtonsoft.Json.Linq.JObject Snapshot(int classLevel, params string[] factKeys)
        {
            var facts = new Newtonsoft.Json.Linq.JArray();
            foreach (string key in factKeys)
                facts.Add(new Newtonsoft.Json.Linq.JObject {
                    ["rootGuid"] = key.Split('/')[0], ["paramGuid"] = key.Split('/')[1], ["rank"] = 1 });
            return new Newtonsoft.Json.Linq.JObject {
                ["classLevel"] = classLevel, ["characterLevel"] = classLevel, ["facts"] = facts };
        }

        internal static void CancellationEvidenceEvaluationIsExact()
        {
            const string root = "09c9e82965fb4334b984a1e9df3bd088";
            const string param = "1e1f627d26ad36f43bbd26cc2bf8ac7e";
            var failures = new List<string>();
            // Clean cancellation: identical snapshots, target held, absent after.
            Assertions.True(FirearmHigherFeatRootsRules.EvaluateCancellationEvidence(
                Snapshot(8, "wf/" + param), Snapshot(8, "wf/" + param), true, root, param, failures),
                "Identical snapshots with a held preview prove cancellation. failures=" + string.Join("|", failures));

            failures.Clear();
            Assertions.False(FirearmHigherFeatRootsRules.EvaluateCancellationEvidence(
                Snapshot(8, "wf/" + param), Snapshot(8, "wf/" + param, root + "/" + param), true, root, param, failures),
                "A post-cancel target appearance must fail.");
            Assertions.True(failures.Contains("fact-appeared:" + root + "/" + param) &&
                failures.Contains("cancelled-target-present"),
                "The appeared target must be named. failures=" + string.Join("|", failures));

            failures.Clear();
            Assertions.False(FirearmHigherFeatRootsRules.EvaluateCancellationEvidence(
                Snapshot(8, "wf/" + param), Snapshot(8, "wf/" + param), false, root, param, failures),
                "A visit that never held the target must not masquerade as a passing cancellation.");
            Assertions.True(failures.Contains("preview-did-not-hold-target"),
                "The missing preview hold must be named.");

            failures.Clear();
            Assertions.False(FirearmHigherFeatRootsRules.EvaluateCancellationEvidence(
                Snapshot(8, "wf/" + param), Snapshot(9, "wf/" + param), true, root, param, failures),
                "A changed class level must fail the cancellation proof.");
            Assertions.True(failures.Contains("class-level-changed"), "The level change must be named.");

            var rankBefore = Snapshot(8, "ws/" + param);
            rankBefore["facts"][0]["rank"] = 1;
            var rankAfter = Snapshot(8, "ws/" + param);
            rankAfter["facts"][0]["rank"] = 2;
            failures.Clear();
            Assertions.False(FirearmHigherFeatRootsRules.EvaluateCancellationEvidence(
                rankBefore, rankAfter, true, root, param, failures),
                "A rank gain must fail the cancellation proof.");
            Assertions.True(failures.Contains("rank-changed:ws/" + param + ":1->2"),
                "The rank change must be named exactly.");

            failures.Clear();
            Assertions.False(FirearmHigherFeatRootsRules.EvaluateCancellationEvidence(
                null, Snapshot(8), true, root, param, failures),
                "A missing snapshot must fail closed.");
            Assertions.True(failures.Contains("snapshot-missing"), "The missing snapshot must be named.");
        }

        internal static void CommittedParameterEvaluationRejectsWrapperSubstitution()
        {
            const string root = "31470b17e8446ae4ea0dacd6c5817d86";
            const string param = "1e1f627d26ad36f43bbd26cc2bf8ac7e";
            const string wrapper = "e115e1e0a17a4aceb0010000000000aa";
            var failures = new List<string>();
            Assertions.True(FirearmHigherFeatRootsRules.EvaluateCommittedParameter(
                root, param, root, param, wrapper, 1, failures),
                "The exact observed pair at rank 1 passes. failures=" + string.Join("|", failures));

            failures.Clear();
            Assertions.False(FirearmHigherFeatRootsRules.EvaluateCommittedParameter(
                root, wrapper, root, param, wrapper, 1, failures),
                "Substituting the per-root wrapper as the parameter must fail.");
            Assertions.True(failures.Contains("wrapper-substituted-as-parameter") &&
                failures.Contains("parameter-mismatch"),
                "The wrapper substitution must be named. failures=" + string.Join("|", failures));

            failures.Clear();
            Assertions.False(FirearmHigherFeatRootsRules.EvaluateCommittedParameter(
                wrapper, param, root, param, wrapper, 1, failures),
                "A wrong observed root must fail.");
            Assertions.True(failures.Contains("root-mismatch"), "The root mismatch must be named.");

            failures.Clear();
            Assertions.False(FirearmHigherFeatRootsRules.EvaluateCommittedParameter(
                root, param, root, param, wrapper, 2, failures),
                "A rank other than one must fail.");
            Assertions.True(failures.Contains("rank-not-one"), "The rank failure must be named.");
        }
    }
}
