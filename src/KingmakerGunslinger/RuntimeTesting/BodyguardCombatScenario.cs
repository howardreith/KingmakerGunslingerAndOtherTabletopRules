using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.BodyguardFeats;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BodyguardCombatScenario
    {
        private const string EvidenceFileName =
            "bodyguard-combat-evidence.json";

        private sealed class ScenarioEvidence
        {
            [JsonProperty("moduleActive", Order = 1)]
            public bool ModuleActive { get; set; }
            [JsonProperty("bodyguardPublication", Order = 2)]
            public string BodyguardPublication { get; set; }
            [JsonProperty("cases", Order = 3)]
            public List<BodyguardCombatCaseEvidence> Cases { get; set; }
            [JsonProperty("cleanup", Order = 4)]
            public string Cleanup { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new ScenarioEvidence {
                ModuleActive = context.FeatureModules.Active.BodyguardFeats,
                Cases = new List<BodyguardCombatCaseEvidence>()
            };
            bool expectedEnabled = request.Scenario ==
                RuntimeTestScenarioCatalog.DisposableBodyguardFeats;
            bool cleaned = false;
            string stage = "fixture";
            BodyguardCombatFixture fixture = null;
            try
            {
                Add(assertions, "bodyguard-module-state",
                    expectedEnabled ? "bodyguard-feats active" :
                        "bodyguard-feats inactive",
                    "active=" + evidence.ModuleActive,
                    evidence.ModuleActive == expectedEnabled,
                    "restart-gated FeatureModuleSettingsState.Active");
                fixture = new BodyguardCombatFixture();
                evidence.BodyguardPublication = fixture.PublicationCounts();
                if (expectedEnabled)
                    RunEnabled(fixture, evidence, assertions, ref stage);
                else
                    RunDisabled(context, fixture, evidence, assertions,
                        ref stage);
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" + exception);
            }
            finally
            {
                BodyguardQualificationControl.Clear();
                BodyguardQualificationRiderComponent.Reset(false);
                BodyguardQualificationDamageProbe.Reset(false);
                BodyguardRuntime.ClearAll("qualification-finally");
                if (fixture != null)
                {
                    try { fixture.Dispose(); cleaned = fixture.Cleaned; }
                    catch (Exception exception)
                    { diagnostics.Add("stage=cleanup;exception=" + exception); }
                }
            }
            evidence.Cleanup = "globalUnitsRestored=" + cleaned;
            Add(assertions, "bodyguard-disposable-cleanup",
                "global unit snapshot restored and request-local state removed",
                evidence.Cleanup, cleaned,
                "disposable scene, units, facts, blueprints, weapons, and thread-local frames");
            Add(assertions, "loaded-mod-version", request.ExpectedModVersion,
                context.ModEntry.Info.Version,
                string.Equals(request.ExpectedModVersion,
                    context.ModEntry.Info.Version, StringComparison.Ordinal),
                "Unity Mod Manager ModEntry.Info.Version");

            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("combatEvidenceSha256=" + Hash(path));
            foreach (BodyguardCombatCaseEvidence item in evidence.Cases)
                diagnostics.Add("case=" + item.Name + ";" +
                    item.RuntimeCounters + ";" + item.Rider);
            bool pass = diagnostics.All(value => value.IndexOf("exception=",
                    StringComparison.Ordinal) < 0) &&
                assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass);
            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            return new RuntimeTestResult {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = context.Assembly.FullName + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = identity.GitCommit,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary =
                    diagnostics.FirstOrDefault(value => value.IndexOf(
                        "exception=", StringComparison.Ordinal) >= 0) ??
                        string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void RunEnabled(BodyguardCombatFixture fixture,
            ScenarioEvidence evidence,
            ICollection<RuntimeTestAssertion> assertions, ref string stage)
        {
            stage = "enabled-publication";
            Add(assertions, "bodyguard-enabled-publication",
                "both feats singular in all four native selection arrays",
                evidence.BodyguardPublication,
                evidence.BodyguardPublication ==
                    "basic.Features=2;basic.AllFeatures=2;fighter.Features=2;fighter.AllFeatures=2",
                "exact object/GUID counts for both project feats");

            stage = "baseline";
            fixture.ClearModes();
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence baseline = fixture.Attack("baseline",
                10, 0, false, false);
            evidence.Cases.Add(baseline);
            int baseMargin = baseline.Roll + baseline.AttackBonus -
                baseline.TargetAc;
            Add(assertions, "bodyguard-baseline-inert",
                "ordinary hostile attack; no AoO/swift/AC/target mutation",
                Describe(baseline), baseline.AooAfter.SequenceEqual(
                    baseline.AooBefore) && Same(baseline.SwiftBefore,
                    baseline.SwiftAfter) && Counter(baseline, "attempts") == 0 &&
                    Counter(baseline, "interceptions") == 0 &&
                    baseline.CombatLogCount == 0 &&
                    baseline.RollTargetRestored &&
                    baseline.WeaponTargetRestored,
                "native RuleAttackWithWeapon plus runtime counters");

            stage = "bodyguard-turns-hit-to-miss";
            fixture.SetModes(fixture.ProtectorOne, true, false);
            fixture.ResetEconomy(3, 0f, 3, 0f);
            int barelyHitPenalty = baseMargin - 1;
            BodyguardCombatCaseEvidence turnsMiss = fixture.Attack(
                "bodyguard-hit-to-miss", 10, barelyHitPenalty, false, false,
                20);
            evidence.Cases.Add(turnsMiss);
            Add(assertions, "bodyguard-success-turns-hit-to-miss",
                "one preauthorized AoO, successful Aid +2, miss, no swift spend",
                Describe(turnsMiss), turnsMiss.AooAfter[0] ==
                    turnsMiss.AooBefore[0] - 1 &&
                    Counter(turnsMiss, "attempts") == 1 &&
                    Counter(turnsMiss, "successful") == 1 &&
                    Contribution(turnsMiss) == 2 && !turnsMiss.Hit &&
                    turnsMiss.HpLoss.All(value => value == 0) &&
                    turnsMiss.CombatLogCount == 1 &&
                    turnsMiss.CombatLogLastMessage.Contains("+2 AC") &&
                    Same(turnsMiss.SwiftBefore, turnsMiss.SwiftAfter),
                "forced Aid natural 20 and incoming natural 10 through native rules");
            Add(assertions, "bodyguard-ac-breakdown-one",
                "one successful protector exposes one named +2 Bodyguard source without changing the final total",
                Describe(turnsMiss),
                turnsMiss.NativeAcBeforeBodyguard + 2 == turnsMiss.TargetAc &&
                    turnsMiss.BodyguardContribution == 2 &&
                    HasTruthfulBodyguardSources(turnsMiss, 1, 2),
                "RuleCalculateAC.BonusSources consumed by native AttackLogMessage.AppendArmorClassBreakdown");

            stage = "bodyguard-failure";
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence failure = fixture.Attack(
                "bodyguard-failure", 10, barelyHitPenalty, false, false, 1);
            evidence.Cases.Add(failure);
            Add(assertions, "bodyguard-failure-spends",
                "failed Aid spends one AoO, contributes +0, attack resolves normally",
                Describe(failure), failure.AooAfter[0] ==
                    failure.AooBefore[0] - 1 &&
                    Counter(failure, "attempts") == 1 &&
                    Counter(failure, "successful") == 0 &&
                    Contribution(failure) == 0 && failure.Hit &&
                    failure.HpLoss[0] > 0 &&
                    failure.CombatLogCount == 1 &&
                    failure.CombatLogLastMessage.Contains("failure") &&
                    Same(failure.SwiftBefore, failure.SwiftAfter),
                "native AoO count and attack damage recipient");
            Add(assertions, "bodyguard-ac-breakdown-failure",
                "failed Aid exposes no Bodyguard AC source and leaves native AC unchanged",
                Describe(failure),
                failure.NativeAcBeforeBodyguard == failure.TargetAc &&
                    failure.BodyguardContribution == 0 &&
                    HasTruthfulBodyguardSources(failure, 0, 0),
                "native RuleCalculateAC source collection negative control");

            stage = "preauthorized-already-miss";
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence alreadyMiss = fixture.Attack(
                "preauthorized-already-miss", 10, baseMargin + 5, false,
                false, 20);
            evidence.Cases.Add(alreadyMiss);
            Add(assertions, "bodyguard-preauthorized-already-miss",
                "AoO and Aid occur even when base attack misses by five",
                Describe(alreadyMiss), alreadyMiss.AooAfter[0] ==
                    alreadyMiss.AooBefore[0] - 1 &&
                    Counter(alreadyMiss, "attempts") == 1 &&
                    Counter(alreadyMiss, "successful") == 1 &&
                    !alreadyMiss.Hit,
                "resource/roll observations precede attack result");

            stage = "preauthorized-overwhelming-hit";
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence overwhelming = fixture.Attack(
                "preauthorized-overwhelming-hit", 10, baseMargin - 7,
                false, false, 20);
            evidence.Cases.Add(overwhelming);
            Add(assertions, "bodyguard-preauthorized-overwhelming-hit",
                "AoO and Aid occur even when +2 cannot prevent the hit",
                Describe(overwhelming), overwhelming.AooAfter[0] ==
                    overwhelming.AooBefore[0] - 1 &&
                    Counter(overwhelming, "attempts") == 1 &&
                    Counter(overwhelming, "successful") == 1 &&
                    Contribution(overwhelming) == 2 && overwhelming.Hit &&
                    overwhelming.HpLoss[0] > 0,
                "resource/roll observations are independent of final hit");

            QualifyThreatAndRanged(fixture, evidence, assertions, baseMargin,
                ref stage);
            QualifyInterception(fixture, evidence, assertions, baseMargin,
                ref stage);
            QualifyMultipleAndSequential(fixture, evidence, assertions,
                baseMargin, ref stage);
            QualifyZeroDamageAndShieldOther(fixture, evidence, assertions,
                baseMargin, ref stage);
        }

        private static void QualifyThreatAndRanged(
            BodyguardCombatFixture fixture, ScenarioEvidence evidence,
            ICollection<RuntimeTestAssertion> assertions, int baseMargin,
            ref string stage)
        {
            stage = "threat-required";
            fixture.SetAttackerPosition(new Vector3(8f, 0f, 0f));
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence outside = fixture.Attack(
                "outside-threat", 10, baseMargin - 7, false, false);
            evidence.Cases.Add(outside);
            fixture.SetAttackerPosition(
                BodyguardCombatFixture.DefaultAttackerPosition);
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence inside = fixture.Attack(
                "inside-threat", 10, baseMargin - 7, false, false, 20);
            evidence.Cases.Add(inside);
            Add(assertions, "bodyguard-threat-required",
                "ally adjacency alone is inert; attacker in native reach enables Bodyguard",
                "outside={" + Describe(outside) + "};inside={" +
                    Describe(inside) + "}",
                outside.AooAfter[0] == outside.AooBefore[0] &&
                    Counter(outside, "attempts") == 0 &&
                    inside.AooAfter[0] == inside.AooBefore[0] - 1 &&
                    Counter(inside, "attempts") == 1,
                "native IsReach with live corpulence/distance");

            stage = "ranged-attacker";
            fixture.UseSynchronousRangedAttacker(true);
            fixture.SetAttackerPosition(new Vector3(8f, 0f, 0f));
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence far = fixture.Attack(
                "ranged-outside-threat", 10, 0, false, false);
            evidence.Cases.Add(far);
            fixture.SetAttackerPosition(
                BodyguardCombatFixture.DefaultAttackerPosition);
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence near = fixture.Attack(
                "ranged-inside-threat", 10, 0, false, false, 20);
            evidence.Cases.Add(near);
            fixture.UseSynchronousRangedAttacker(false);
            Add(assertions, "bodyguard-ranged-attacker-threat",
                "distant ranged attacker ineligible; same ranged attack inside threat attempts",
                "far={" + Describe(far) + "};near={" + Describe(near) + "}",
                far.AttackFamily == "ranged-weapon" &&
                    far.AooAfter[0] == far.AooBefore[0] &&
                    Counter(far, "attempts") == 0 &&
                    near.AttackFamily == "ranged-weapon" &&
                    near.AooAfter[0] == near.AooBefore[0] - 1 &&
                    Counter(near, "attempts") == 1,
                "projectile-free request-local clone preserves native ranged roll/damage path");
        }

        private static void QualifyInterception(BodyguardCombatFixture fixture,
            ScenarioEvidence evidence,
            ICollection<RuntimeTestAssertion> assertions, int baseMargin,
            ref string stage)
        {
            stage = "in-harms-way-off";
            fixture.SetModes(fixture.ProtectorOne, true, false);
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence off = fixture.Attack(
                "in-harms-way-mode-off", 10, baseMargin - 7, false, true, 20);
            evidence.Cases.Add(off);
            Add(assertions, "in-harms-way-mode-off",
                "successful Bodyguard does not intercept or spend swift while mode off",
                Describe(off), off.Hit && Counter(off, "successful") == 1 &&
                    Counter(off, "interceptions") == 0 &&
                    off.HpLoss[0] > 0 && off.HpLoss[1] == 0 &&
                    Same(off.SwiftBefore, off.SwiftAfter),
                "live target HP and shared swift cooldown");

            stage = "in-harms-way-full-delivery";
            fixture.SetModes(fixture.ProtectorOne, true, true);
            fixture.AddFlaming();
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence intercepted = fixture.Attack(
                "in-harms-way-full-delivery", 10, baseMargin - 7, true,
                true, 20);
            evidence.Cases.Add(intercepted);
            fixture.RemoveFlaming();
            Add(assertions, "in-harms-way-full-delivery",
                "preserved critical redirects physical, fire, save, condition, and HP exactly once",
                Describe(intercepted), intercepted.Hit &&
                    intercepted.Critical &&
                    Counter(intercepted, "interceptions") == 1 &&
                    intercepted.AooAfter[0] == intercepted.AooBefore[0] - 1 &&
                    Near(intercepted.SwiftAfter[0],
                        intercepted.SwiftBefore[0] + 6f) &&
                    intercepted.HpLoss[0] == 0 &&
                    intercepted.HpLoss[1] > 0 &&
                    intercepted.DamageKinds.Any(value => value.IndexOf(
                        "PhysicalDamage", StringComparison.Ordinal) >= 0) &&
                    intercepted.DamageKinds.Any(value => value.IndexOf(
                        "EnergyDamage", StringComparison.Ordinal) >= 0) &&
                    intercepted.CombatLogCount == 2 &&
                    intercepted.CombatLogLastMessage.Contains(
                        "In Harm's Way") &&
                    intercepted.CombatLogLastMessage.Contains(
                        "complete delivery") &&
                    HasRider(intercepted, fixture.ProtectorOne) &&
                    fixture.HasRider(fixture.ProtectorOne) &&
                    !fixture.HasRider(fixture.Target) &&
                    intercepted.RollTargetRestored &&
                    intercepted.WeaponTargetRestored,
                "native attack/damage plus attack-roll Did rider and RuleSavingThrow");

            stage = "in-harms-way-no-immediate";
            fixture.RemoveRiders();
            fixture.ResetEconomy(3, 6f, 3, 0f);
            BodyguardCombatCaseEvidence unavailable = fixture.Attack(
                "no-immediate-action", 10, baseMargin - 7, false, true, 20);
            evidence.Cases.Add(unavailable);
            Add(assertions, "in-harms-way-no-immediate",
                "unavailable native swift leaves original target unchanged",
                Describe(unavailable), Counter(unavailable,
                    "interceptions") == 0 &&
                    Near(unavailable.SwiftAfter[0], 6f) &&
                    unavailable.HpLoss[0] > 0 &&
                    unavailable.HpLoss[1] == 0 &&
                    unavailable.RollTargetRestored &&
                    unavailable.WeaponTargetRestored,
                "native UnitCombatState.Cooldown.SwiftAction");
        }

        private static void QualifyMultipleAndSequential(
            BodyguardCombatFixture fixture, ScenarioEvidence evidence,
            ICollection<RuntimeTestAssertion> assertions, int baseMargin,
            ref string stage)
        {
            stage = "multiple-protectors";
            fixture.RemoveRiders();
            fixture.SetModes(fixture.ProtectorTwo, true, true);
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence multiple = fixture.Attack(
                "multiple-protectors", 10, baseMargin - 9, false, true,
                20, 20);
            evidence.Cases.Add(multiple);
            int selected = string.CompareOrdinal(fixture.ProtectorOne.UniqueId,
                fixture.ProtectorTwo.UniqueId) <= 0 ? 0 : 1;
            int unselected = selected == 0 ? 1 : 0;
            Add(assertions, "bodyguard-multiple-stack-and-arbitration",
                "two AoOs, +4 AC, identity-first sole interceptor, one swift",
                Describe(multiple) + ";expectedSelected=" +
                    fixture.Protectors[selected].UniqueId,
                multiple.AooAfter[0] == multiple.AooBefore[0] - 1 &&
                    multiple.AooAfter[1] == multiple.AooBefore[1] - 1 &&
                    Counter(multiple, "successful") == 2 &&
                    Contribution(multiple) == 4 &&
                    Counter(multiple, "interceptions") == 1 &&
                    multiple.CombatLogCount == 3 &&
                    Near(multiple.SwiftAfter[selected],
                        multiple.SwiftBefore[selected] + 6f) &&
                    Near(multiple.SwiftAfter[unselected],
                        multiple.SwiftBefore[unselected]) &&
                    multiple.HpLoss[selected + 1] > 0 &&
                    multiple.HpLoss[unselected + 1] == 0,
                "party-order fallback then ordinal persistent UnitEntityData.UniqueId");
            Add(assertions, "bodyguard-ac-breakdown-two",
                "two successful protectors expose two named +2 sources totaling +4 exactly once",
                Describe(multiple),
                multiple.NativeAcBeforeBodyguard + 4 == multiple.TargetAc &&
                    multiple.BodyguardContribution == 4 &&
                    HasTruthfulBodyguardSources(multiple, 2, 4) &&
                    multiple.BodyguardSources.Select(value =>
                        value.SourceFactIdentity).Distinct().Count() == 2,
                "two protector-owned Bodyguard facts in native RuleCalculateAC.BonusSources");

            stage = "multiple-attacks";
            fixture.SetModes(fixture.ProtectorTwo, false, false);
            fixture.SetModes(fixture.ProtectorOne, true, false);
            fixture.ResetEconomy(2, 0f, 3, 0f);
            BodyguardCombatCaseEvidence first = fixture.Attack(
                "sequential-first", 10, baseMargin + 5, false, false, 20);
            BodyguardCombatCaseEvidence second = fixture.Attack(
                "sequential-second", 10, baseMargin + 5, false, false, 20);
            evidence.Cases.Add(first);
            evidence.Cases.Add(second);
            Add(assertions, "bodyguard-sequential-frames",
                "two independent attacks spend once each with no state leakage",
                "first={" + Describe(first) + "};second={" +
                    Describe(second) + "}",
                first.AooBefore[0] == 2 && first.AooAfter[0] == 1 &&
                    second.AooBefore[0] == 1 && second.AooAfter[0] == 0 &&
                    Contribution(first) == 2 && Contribution(second) == 2 &&
                    first.AttackIdentity != second.AttackIdentity &&
                    first.RollTargetRestored && second.RollTargetRestored &&
                    Counter(first, "faults") == 0 &&
                    Counter(second, "faults") == 0 &&
                    HasTruthfulBodyguardSources(first, 1, 2) &&
                    HasTruthfulBodyguardSources(second, 1, 2),
                "separate RuleAttackRoll identities and native carried AoO count");
        }

        private static void QualifyZeroDamageAndShieldOther(
            BodyguardCombatFixture fixture, ScenarioEvidence evidence,
            ICollection<RuntimeTestAssertion> assertions, int baseMargin,
            ref string stage)
        {
            stage = "zero-damage-rider";
            fixture.SetModes(fixture.ProtectorOne, true, true);
            fixture.AddCompleteDefense(fixture.ProtectorOne);
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence zero = fixture.Attack(
                "zero-damage-rider", 10, baseMargin - 7, false, true, 20);
            evidence.Cases.Add(zero);
            fixture.RemoveCompleteDefense(fixture.ProtectorOne);
            Add(assertions, "in-harms-way-zero-damage-rider",
                "zero HP damage still redirects one attack-linked save and condition",
                Describe(zero), Counter(zero, "interceptions") == 1 &&
                    zero.HpLoss.All(value => value == 0) &&
                    HasRider(zero, fixture.ProtectorOne) &&
                    fixture.HasRider(fixture.ProtectorOne) &&
                    !fixture.HasRider(fixture.Target),
                "native DR plus independent RuleAttackRoll Did delivery");

            stage = "shield-other-on-interceptor";
            fixture.RemoveRiders();
            fixture.RemoveShieldOther();
            fixture.ApplyShieldOther(fixture.ProtectorOne,
                fixture.ProtectorTwo);
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence shieldInterceptor = fixture.Attack(
                "shield-other-on-interceptor", 10, baseMargin - 7, false,
                false, 20);
            evidence.Cases.Add(shieldInterceptor);
            Add(assertions, "in-harms-way-shield-other-interceptor",
                "redirect to interceptor precedes Shield Other finalized split",
                Describe(shieldInterceptor), Counter(shieldInterceptor,
                    "interceptions") == 1 &&
                    shieldInterceptor.HpLoss[0] == 0 &&
                    shieldInterceptor.HpLoss[1] > 0 &&
                    shieldInterceptor.HpLoss[2] > 0,
                "natural In Harm's Way then Shield Other RuleDealDamage ordering");

            stage = "shield-other-on-original";
            fixture.RemoveShieldOther();
            fixture.ApplyShieldOther(fixture.Target, fixture.ProtectorTwo);
            fixture.ResetEconomy(3, 0f, 3, 0f);
            BodyguardCombatCaseEvidence shieldOriginal = fixture.Attack(
                "shield-other-on-original", 10, baseMargin - 7, false,
                false, 20);
            evidence.Cases.Add(shieldOriginal);
            fixture.RemoveShieldOther();
            Add(assertions, "in-harms-way-shield-other-original",
                "original ally has no intercepted damage for Shield Other to split",
                Describe(shieldOriginal), Counter(shieldOriginal,
                    "interceptions") == 1 &&
                    shieldOriginal.HpLoss[0] == 0 &&
                    shieldOriginal.HpLoss[1] > 0 &&
                    shieldOriginal.HpLoss[2] == 0,
                "natural redirected RuleDealDamage recipient ordering");
        }

        private static void RunDisabled(ModContext context,
            BodyguardCombatFixture fixture, ScenarioEvidence evidence,
            ICollection<RuntimeTestAssertion> assertions, ref string stage)
        {
            stage = "disabled-publication";
            Add(assertions, "bodyguard-disabled-publication",
                "neither feat advertised in any native selection array",
                evidence.BodyguardPublication,
                evidence.BodyguardPublication ==
                    "basic.Features=0;basic.AllFeatures=0;fighter.Features=0;fighter.AllFeatures=0",
                "identities retained but publication restart-gated off");
            fixture.SetModes(fixture.ProtectorOne, true, true);
            fixture.ResetEconomy(3, 0f, 3, 0f);
            stage = "disabled-runtime";
            BodyguardCombatCaseEvidence disabled = fixture.Attack(
                "module-disabled", 10, -30, false, true, 20);
            evidence.Cases.Add(disabled);
            Add(assertions, "bodyguard-disabled-runtime-inert",
                "manually present facts/markers cannot mutate combat while disabled",
                Describe(disabled), disabled.AooAfter.SequenceEqual(
                    disabled.AooBefore) && Same(disabled.SwiftBefore,
                    disabled.SwiftAfter) && Counter(disabled, "attempts") == 0 &&
                    Counter(disabled, "interceptions") == 0 &&
                    disabled.CombatLogCount == 0 &&
                    disabled.HpLoss[0] > 0 && disabled.HpLoss[1] == 0 &&
                    disabled.BodyguardContribution == 0 &&
                    HasTruthfulBodyguardSources(disabled, 0, 0) &&
                    disabled.RollTargetRestored &&
                    disabled.WeaponTargetRestored,
                "live module-disabled RuleAttackWithWeapon control");
            Add(assertions, "bodyguard-disabled-other-module-isolation",
                "Shield Other remains independently active",
                "shieldOther=" + context.FeatureModules.Active.ShieldOther,
                context.FeatureModules.Active.ShieldOther &&
                    BlueprintBootstrap.ShieldOther != null,
                "independent active module snapshot and blueprint family");
        }

        private static bool HasRider(BodyguardCombatCaseEvidence value,
            UnitEntityData target)
        {
            return value.Rider.IndexOf("invocations=1",
                    StringComparison.Ordinal) >= 0 &&
                value.Rider.IndexOf("target=" + target.UniqueId,
                    StringComparison.Ordinal) >= 0 &&
                value.Rider.IndexOf("saveTarget=" + target.UniqueId,
                    StringComparison.Ordinal) >= 0;
        }

        private static bool HasTruthfulBodyguardSources(
            BodyguardCombatCaseEvidence value, int expectedCount,
            int expectedTotal)
        {
            if (value == null || value.BodyguardSources == null ||
                value.BodyguardSources.Length != expectedCount ||
                value.BodyguardSources.Sum(item => item.Bonus) != expectedTotal)
                return false;
            string guid = BlueprintBootstrap.BodyguardFeats == null ? null :
                BlueprintBootstrap.BodyguardFeats.Bodyguard.AssetGuid;
            return value.BodyguardSources.All(item => item.Bonus == 2 &&
                string.Equals(item.SourceName, "Bodyguard",
                    StringComparison.Ordinal) &&
                string.Equals(item.SourceBlueprintGuid, guid,
                    StringComparison.Ordinal) &&
                string.Equals(item.SourceBlueprintName,
                    "KMG_Bodyguard_Feature", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(item.SourceFactType));
        }

        private static string Describe(BodyguardCombatCaseEvidence value)
        {
            return "roll=" + value.Roll + ";bonus=" + value.AttackBonus +
                ";ac=" + value.TargetAc + ";hit=" + value.Hit +
                ";nativeAc=" + value.NativeAcBeforeBodyguard +
                ";bodyguardContribution=" + value.BodyguardContribution +
                ";bodyguardSources=" + string.Join(",",
                    (value.BodyguardSources ??
                        new BodyguardArmorClassSourceEvidence[0]).Select(item =>
                            item.Bonus + "/" + item.SourceName + "/" +
                            item.SourceBlueprintGuid + "/" +
                            item.SourceFactIdentity).ToArray()) +
                ";critical=" + value.Critical + ";aoo=" +
                string.Join("/", value.AooBefore) + "->" +
                string.Join("/", value.AooAfter) + ";swift=" +
                string.Join("/", value.SwiftBefore.Select(item =>
                    item.ToString("R")).ToArray()) + "->" +
                string.Join("/", value.SwiftAfter.Select(item =>
                    item.ToString("R")).ToArray()) + ";hpLoss=" +
                string.Join("/", value.HpLoss) + ";" +
                value.RuntimeCounters + ";combatLogs=" +
                value.CombatLogCount + ";lastCombatLog=" +
                value.CombatLogLastMessage + ";" + value.AidControl + ";" +
                value.Rider;
        }

        private static int Counter(BodyguardCombatCaseEvidence value,
            string name)
        {
            string prefix = name + "=";
            string part = value.RuntimeCounters.Split(';').Single(item =>
                item.StartsWith(prefix, StringComparison.Ordinal));
            return int.Parse(part.Substring(prefix.Length));
        }

        private static int Contribution(BodyguardCombatCaseEvidence value)
        {
            const string marker = ";acContribution=";
            string observation = value.RuntimeObservations.LastOrDefault(item =>
                item.IndexOf("stage=armor-class", StringComparison.Ordinal) >= 0);
            if (observation == null) return 0;
            int start = observation.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0) return 0;
            start += marker.Length;
            int end = observation.IndexOf(';', start);
            return int.Parse(end < 0 ? observation.Substring(start) :
                observation.Substring(start, end - start));
        }

        private static bool Same(float[] left, float[] right)
        {
            return left.Length == right.Length && left.Zip(right, Near)
                .All(value => value);
        }

        private static bool Near(float left, float right)
        { return Math.Abs(left - right) < 0.0001f; }

        private static string Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", "").ToLowerInvariant();
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string id, string expected, string observed, bool passed,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = id,
                Expected = expected, Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail, Evidence = evidence });
        }
    }
}
