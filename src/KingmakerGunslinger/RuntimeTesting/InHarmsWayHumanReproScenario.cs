using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.BodyguardFeats;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded, request-local replay against the byte-preserved human save.
    /// The scenario mutates only the in-memory disposable load and uses the
    /// save's real facts, modes, units, positions, spear, and native rule path.
    /// </summary>
    internal static class InHarmsWayHumanReproScenario
    {
        private const string ProtectorId =
            "5b6aa62a-e6fb-42c3-ba78-9cd3549505c1";
        private const string VictimId =
            "533a5084-8aa1-4aa0-a8f6-b8eac959368f";
        private const string AttackerId =
            "007a489e-d797-4555-ab6c-0c27cd6431ee";
        private const string EvidenceFileName =
            "in-harms-way-human-repro.json";

        private sealed class StateEvidence
        {
            [JsonProperty("protector", Order = 1)]
            public string Protector { get; set; }
            [JsonProperty("victim", Order = 2)]
            public string Victim { get; set; }
            [JsonProperty("attacker", Order = 3)]
            public string Attacker { get; set; }
            [JsonProperty("bodyguardFeat", Order = 4)]
            public bool BodyguardFeat { get; set; }
            [JsonProperty("inHarmsWayFeat", Order = 5)]
            public bool InHarmsWayFeat { get; set; }
            [JsonProperty("bodyguardActivatable", Order = 6)]
            public bool BodyguardActivatable { get; set; }
            [JsonProperty("inHarmsWayActivatable", Order = 7)]
            public bool InHarmsWayActivatable { get; set; }
            [JsonProperty("bodyguardIsOn", Order = 8)]
            public bool BodyguardIsOn { get; set; }
            [JsonProperty("inHarmsWayIsOn", Order = 9)]
            public bool InHarmsWayIsOn { get; set; }
            [JsonProperty("bodyguardIsRunning", Order = 10)]
            public bool BodyguardIsRunning { get; set; }
            [JsonProperty("inHarmsWayIsRunning", Order = 11)]
            public bool InHarmsWayIsRunning { get; set; }
            [JsonProperty("bodyguardMarker", Order = 12)]
            public bool BodyguardMarker { get; set; }
            [JsonProperty("inHarmsWayMarker", Order = 13)]
            public bool InHarmsWayMarker { get; set; }
            [JsonProperty("alive", Order = 14)] public bool Alive { get; set; }
            [JsonProperty("conscious", Order = 15)]
            public bool Conscious { get; set; }
            [JsonProperty("canAct", Order = 16)] public bool CanAct { get; set; }
            [JsonProperty("hasSwiftAction", Order = 17)]
            public bool HasSwiftAction { get; set; }
            [JsonProperty("swiftCooldown", Order = 18)]
            public float SwiftCooldown { get; set; }
            [JsonProperty("aooRemaining", Order = 19)]
            public int AooRemaining { get; set; }
            [JsonProperty("weaponGuid", Order = 20)]
            public string WeaponGuid { get; set; }
            [JsonProperty("weaponName", Order = 21)]
            public string WeaponName { get; set; }
        }

        private sealed class AttackEvidence
        {
            [JsonProperty("name", Order = 1)] public string Name { get; set; }
            [JsonProperty("attackIdentity", Order = 2)]
            public int AttackIdentity { get; set; }
            [JsonProperty("attackD20", Order = 3)]
            public int AttackD20 { get; set; }
            [JsonProperty("attackTotal", Order = 4)]
            public int AttackTotal { get; set; }
            [JsonProperty("targetAc", Order = 5)]
            public int TargetAc { get; set; }
            [JsonProperty("hit", Order = 6)] public bool Hit { get; set; }
            [JsonProperty("criticalThreat", Order = 7)]
            public bool CriticalThreat { get; set; }
            [JsonProperty("confirmationD20", Order = 8)]
            public int ConfirmationD20 { get; set; }
            [JsonProperty("confirmationTotal", Order = 9)]
            public int ConfirmationTotal { get; set; }
            [JsonProperty("criticalConfirmed", Order = 10)]
            public bool CriticalConfirmed { get; set; }
            [JsonProperty("bodyguardContribution", Order = 11)]
            public int BodyguardContribution { get; set; }
            [JsonProperty("aooBefore", Order = 12)]
            public int AooBefore { get; set; }
            [JsonProperty("aooAfter", Order = 13)]
            public int AooAfter { get; set; }
            [JsonProperty("swiftBefore", Order = 14)]
            public float SwiftBefore { get; set; }
            [JsonProperty("swiftAfter", Order = 15)]
            public float SwiftAfter { get; set; }
            [JsonProperty("victimHpLoss", Order = 16)]
            public int VictimHpLoss { get; set; }
            [JsonProperty("protectorHpLoss", Order = 17)]
            public int ProtectorHpLoss { get; set; }
            [JsonProperty("rollTargetRestored", Order = 18)]
            public bool RollTargetRestored { get; set; }
            [JsonProperty("weaponTargetRestored", Order = 19)]
            public bool WeaponTargetRestored { get; set; }
            [JsonProperty("control", Order = 20)]
            public string Control { get; set; }
            [JsonProperty("counters", Order = 21)]
            public string Counters { get; set; }
            [JsonProperty("observations", Order = 22)]
            public string[] Observations { get; set; }
            [JsonProperty("combatLogLast", Order = 23)]
            public string CombatLogLast { get; set; }
        }

        private sealed class ReproEvidence
        {
            [JsonProperty("initialState", Order = 1)]
            public StateEvidence InitialState { get; set; }
            [JsonProperty("attacks", Order = 2)]
            public List<AttackEvidence> Attacks { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var files = new List<string>();
            var evidence = new ReproEvidence {
                Attacks = new List<AttackEvidence>() };
            try
            {
                UnitEntityData protector = RequireUnit(ProtectorId,
                    "HelpfulDefenderTest");
                UnitEntityData victim = RequireUnit(VictimId, "VictimTest");
                UnitEntityData attacker = RequireUnit(AttackerId, "Kobold");
                ItemEntityWeapon spear = attacker.Body.PrimaryHand.MaybeWeapon;
                if (spear == null || spear.Blueprint == null ||
                    !spear.Blueprint.IsMelee)
                    throw new InvalidOperationException(
                        "The exact human attacker no longer has its melee spear.");

                evidence.InitialState = CaptureState(protector, victim,
                    attacker, spear);
                StateEvidence state = evidence.InitialState;
                Add(assertions, "human-save-exact-feature-and-mode-state",
                    "exact feats, activatables, IsOn state, and marker buffs are synchronized",
                    JsonConvert.SerializeObject(state),
                    state.BodyguardFeat && state.InHarmsWayFeat &&
                        state.BodyguardActivatable &&
                        state.InHarmsWayActivatable && state.BodyguardIsOn &&
                        state.InHarmsWayIsOn && state.BodyguardIsRunning &&
                        state.InHarmsWayIsRunning && state.BodyguardMarker &&
                        state.InHarmsWayMarker,
                    "byte-preserved human save after guarded native load");

                evidence.Attacks.Add(Attack("saved-state-critical", attacker,
                    victim, protector, spear, 20, 17, null));
                AttackEvidence saved = evidence.Attacks.Last();
                bool savedConsistent = Has(saved, "decision=eligible")
                    ? saved.VictimHpLoss == 0 && saved.ProtectorHpLoss > 0
                    : Has(saved, "decision=swift-cooldown-active") ||
                      Has(saved, "decision=has-swift-action-false") ||
                      Has(saved, "decision=protector-unable-to-act");
                Add(assertions, "human-save-temporal-gate-characterized",
                    "saved temporal action state either intercepts or records one exact native rejection",
                    Describe(saved), savedConsistent,
                    "real saved units and native spear attack without action reset");

                evidence.Attacks.Add(Attack("available-normal-hit", attacker,
                    victim, protector, spear, 19, null, 0f));
                AttackEvidence normal = evidence.Attacks.Last();
                Add(assertions, "human-save-normal-hit-intercepts",
                    "available native immediate action redirects one +4 Bodyguard hit",
                    Describe(normal), SuccessfulInterception(normal, false),
                    "actual HP recipient and RuleDealDamage target observation");

                evidence.Attacks.Add(Attack("available-confirmed-critical",
                    attacker, victim, protector, spear, 20, 17, 0f));
                AttackEvidence critical = evidence.Attacks.Last();
                Add(assertions, "human-save-critical-intercepts",
                    "natural 20 and confirmation 17 remain confirmed while complete delivery moves once",
                    Describe(critical), SuccessfulInterception(critical, true),
                    "actual critical HP recipient and target restoration");

                evidence.Attacks.Add(Attack("immediate-unavailable", attacker,
                    victim, protector, spear, 19, null, 6f));
                AttackEvidence unavailable = evidence.Attacks.Last();
                Add(assertions, "human-save-no-immediate-negative-control",
                    "active mode with native swift cooldown leaves original recipient and explains why",
                    Describe(unavailable), unavailable.BodyguardContribution == 4 &&
                        unavailable.Hit && unavailable.VictimHpLoss > 0 &&
                        unavailable.ProtectorHpLoss == 0 &&
                        Has(unavailable, "decision=swift-cooldown-active") &&
                        unavailable.CombatLogLast.Contains(
                            "no immediate action is available"),
                    "native shared swift/immediate cooldown negative control");
            }
            catch (Exception exception)
            {
                diagnostics.Add("exception=" + exception);
            }
            finally
            {
                BodyguardQualificationControl.Clear();
                BodyguardRuntime.ClearAll("human-repro-finally");
            }

            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            files.Add(path);
            diagnostics.Add("evidence=" + path);
            foreach (AttackEvidence attack in evidence.Attacks)
                diagnostics.Add(Describe(attack));
            bool passed = diagnostics.All(value => !value.StartsWith(
                    "exception=", StringComparison.Ordinal)) &&
                assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass);
            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            return new RuntimeTestResult {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = context.Assembly.FullName + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = identity.GitCommit,
                GameVersion = UnityEngine.Application.version ?? string.Empty,
                StartUtc = started.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary =
                    diagnostics.FirstOrDefault(value => value.StartsWith(
                        "exception=", StringComparison.Ordinal)) ?? string.Empty,
                EvidenceFiles = files,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static AttackEvidence Attack(string name,
            UnitEntityData attacker, UnitEntityData victim,
            UnitEntityData protector, ItemEntityWeapon weapon,
            int incoming, int? confirmation, float? swift)
        {
            BodyguardRuntime.ClearAll("human-repro-case-start");
            BodyguardRuntimeDiagnostics.Reset();
            victim.Descriptor.Damage = 0;
            protector.Descriptor.Damage = 0;
            protector.CombatState.AttackOfOpportunityCount = 3;
            if (swift.HasValue)
                protector.CombatState.Cooldown.SwiftAction = swift.Value;
            int aooBefore = protector.CombatState.AttackOfOpportunityCount;
            float swiftBefore = protector.CombatState.Cooldown.SwiftAction;
            int victimBefore = victim.HPLeft;
            int protectorBefore = protector.HPLeft;
            long logBefore = BodyguardCombatLog.Attempts;
            RuleAttackWithWeapon attack = null;
            string control;
            if (confirmation.HasValue)
                BodyguardQualificationControl.ArmCritical(incoming,
                    confirmation.Value, 20);
            else
                BodyguardQualificationControl.Arm(incoming, 20);
            try
            {
                attack = new RuleAttackWithWeapon(attacker, victim, weapon, 0)
                    { Maximized = true };
                Rulebook.Trigger(attack);
            }
            finally
            {
                control = BodyguardQualificationControl.DescribeAndClear();
            }
            if (attack == null || attack.AttackRoll == null)
                throw new InvalidOperationException(
                    "The native human-repro attack did not expose its roll.");
            RuleAttackRoll roll = attack.AttackRoll;
            string[] observations = BodyguardRuntimeDiagnostics
                .SnapshotObservations();
            return new AttackEvidence {
                Name = name,
                AttackIdentity = RuntimeHelpers.GetHashCode(roll),
                AttackD20 = roll.Roll,
                AttackTotal = roll.Roll + roll.AttackBonus,
                TargetAc = roll.TargetAC,
                Hit = roll.IsHit,
                CriticalThreat = roll.IsCriticalRoll,
                ConfirmationD20 = roll.IsCriticalRoll ?
                    (int)roll.CriticalConfirmationRoll : 0,
                ConfirmationTotal = roll.IsCriticalRoll ?
                    (int)roll.CriticalConfirmationRoll + roll.AttackBonus +
                    roll.CriticalConfirmationBonus : 0,
                CriticalConfirmed = roll.IsCriticalConfirmed,
                BodyguardContribution = roll.ACRule == null ||
                    roll.ACRule.BonusSources == null ? 0 :
                    roll.ACRule.BonusSources.Where(value => value.Source !=
                        null && value.Source.Blueprint != null &&
                        string.Equals(value.Source.Blueprint.AssetGuid,
                            BlueprintBootstrap.BodyguardFeats.Bodyguard
                                .AssetGuid, StringComparison.Ordinal))
                        .Sum(value => value.Bonus),
                AooBefore = aooBefore,
                AooAfter = protector.CombatState.AttackOfOpportunityCount,
                SwiftBefore = swiftBefore,
                SwiftAfter = protector.CombatState.Cooldown.SwiftAction,
                VictimHpLoss = victimBefore - victim.HPLeft,
                ProtectorHpLoss = protectorBefore - protector.HPLeft,
                RollTargetRestored = ReferenceEquals(roll.Target, victim),
                WeaponTargetRestored = ReferenceEquals(attack.Target, victim),
                Control = control,
                Counters = "frames=" + BodyguardRuntimeDiagnostics.Frames +
                    ";attempts=" + BodyguardRuntimeDiagnostics.Attempts +
                    ";interceptions=" +
                    BodyguardRuntimeDiagnostics.Interceptions +
                    ";faults=" + BodyguardRuntimeDiagnostics.Faults +
                    ";duplicates=" +
                    BodyguardRuntimeDiagnostics.DuplicateCallbacks +
                    ";completed=" + BodyguardRuntimeDiagnostics.Completed,
                Observations = observations,
                CombatLogLast = BodyguardCombatLog.Attempts == logBefore ?
                    string.Empty : BodyguardCombatLog.LastMessage ?? string.Empty
            };
        }

        private static StateEvidence CaptureState(UnitEntityData protector,
            UnitEntityData victim, UnitEntityData attacker,
            ItemEntityWeapon weapon)
        {
            BodyguardFeatBlueprintSet set = BlueprintBootstrap.BodyguardFeats;
            if (set == null) throw new InvalidOperationException(
                "Bodyguard blueprint set is unavailable after save load.");
            ActivatableAbility bodyguard = FindMode(protector,
                set.Modes.BodyguardAbility);
            ActivatableAbility inHarmsWay = FindMode(protector,
                set.Modes.InHarmsWayAbility);
            BodyguardImmediateActionSnapshot action =
                BodyguardActionEconomyAccess.ObserveImmediateAction(protector);
            return new StateEvidence {
                Protector = Identity(protector), Victim = Identity(victim),
                Attacker = Identity(attacker),
                BodyguardFeat = ExactFact(protector, set.Bodyguard),
                InHarmsWayFeat = ExactFact(protector, set.InHarmsWay),
                BodyguardActivatable = bodyguard != null,
                InHarmsWayActivatable = inHarmsWay != null,
                BodyguardIsOn = bodyguard != null && bodyguard.IsOn,
                InHarmsWayIsOn = inHarmsWay != null && inHarmsWay.IsOn,
                BodyguardIsRunning = bodyguard != null && bodyguard.IsRunning,
                InHarmsWayIsRunning = inHarmsWay != null &&
                    inHarmsWay.IsRunning,
                BodyguardMarker = protector.Descriptor.Buffs.GetBuff(
                    set.Modes.BodyguardMarker) != null,
                InHarmsWayMarker = protector.Descriptor.Buffs.GetBuff(
                    set.Modes.InHarmsWayMarker) != null,
                Alive = action.Alive, Conscious = action.Conscious,
                CanAct = action.CanAct, HasSwiftAction = action.HasSwiftAction,
                SwiftCooldown = action.SwiftCooldown,
                AooRemaining = protector.CombatState.AttackOfOpportunityCount,
                WeaponGuid = weapon.Blueprint.AssetGuid,
                WeaponName = weapon.Blueprint.name ?? string.Empty
            };
        }

        private static UnitEntityData RequireUnit(string id, string name)
        {
            UnitEntityData[] matches = Game.Instance.State.Units.All
                .Where(value => value != null && string.Equals(value.UniqueId,
                    id, StringComparison.Ordinal) && string.Equals(
                    value.CharacterName, name, StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException("Expected one exact " +
                    name + " but found " + matches.Length + ".");
            return matches[0];
        }

        private static ActivatableAbility FindMode(UnitEntityData unit,
            BlueprintActivatableAbility blueprint)
        {
            return unit.Descriptor.ActivatableAbilities.Enumerable
                .SingleOrDefault(value => value != null && ReferenceEquals(
                    value.Blueprint, blueprint));
        }

        private static bool ExactFact(UnitEntityData unit,
            BlueprintFeature feature)
        {
            Fact fact = unit.Descriptor.GetFact(feature);
            return fact != null && ReferenceEquals(fact.Blueprint, feature);
        }

        private static bool SuccessfulInterception(AttackEvidence value,
            bool critical)
        {
            return value.BodyguardContribution == 4 && value.Hit &&
                value.CriticalConfirmed == critical &&
                (!critical || value.AttackD20 == 20 &&
                    value.ConfirmationD20 == 17) &&
                value.AooAfter == value.AooBefore - 1 &&
                Math.Abs(value.SwiftAfter - (value.SwiftBefore + 6f)) <
                    0.0001f && value.VictimHpLoss == 0 &&
                value.ProtectorHpLoss > 0 &&
                value.RollTargetRestored && value.WeaponTargetRestored &&
                Has(value, "decision=eligible") &&
                Has(value, "stage=rule-deal-damage-prefix") &&
                Has(value, "deliveryRecipient=" + ProtectorId) &&
                value.Control.Contains(critical ? "confirmationConsumed=1" :
                    "confirmationConsumed=0");
        }

        private static bool Has(AttackEvidence value, string token)
        {
            return value != null && value.Observations != null &&
                value.Observations.Any(item => item != null && item.IndexOf(
                    token, StringComparison.Ordinal) >= 0);
        }

        private static string Describe(AttackEvidence value)
        {
            return "case=" + value.Name + ";d20=" + value.AttackD20 +
                ";total=" + value.AttackTotal + ";ac=" + value.TargetAc +
                ";hit=" + value.Hit + ";criticalThreat=" +
                value.CriticalThreat + ";confirmation=" +
                value.ConfirmationD20 + "/" + value.ConfirmationTotal +
                ";critical=" + value.CriticalConfirmed +
                ";bodyguard=" + value.BodyguardContribution + ";aoo=" +
                value.AooBefore + "->" + value.AooAfter + ";swift=" +
                value.SwiftBefore.ToString("R") + "->" +
                value.SwiftAfter.ToString("R") + ";hpLoss=" +
                value.VictimHpLoss + "/" + value.ProtectorHpLoss + ";" +
                value.Counters + ";control=" + value.Control;
        }

        private static string Identity(UnitEntityData unit)
        {
            return unit == null ? "<null>" : unit.UniqueId + "/" +
                unit.CharacterName;
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool passed,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = name,
                Expected = expected, Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail, Evidence = evidence });
        }
    }
}
