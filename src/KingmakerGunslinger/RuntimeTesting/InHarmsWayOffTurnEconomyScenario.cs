using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using TurnBased.Controllers;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Multi-update, disposable-save proof of the exact human action-economy
    /// path. Dice are request-locally fixed for repeatability, but turn owner,
    /// raw swift state, debt transitions, attacks, damage, and delivery all use
    /// native runtime objects and callbacks.
    /// </summary>
    internal static class InHarmsWayOffTurnEconomyScenario
    {
        private const string ProtectorId =
            "5b6aa62a-e6fb-42c3-ba78-9cd3549505c1";
        private const string VictimId =
            "533a5084-8aa1-4aa0-a8f6-b8eac959368f";
        private const string AttackerId =
            "007a489e-d797-4555-ab6c-0c27cd6431ee";
        private const string EvidenceFileName =
            "in-harms-way-off-turn-economy.json";

        internal static Session Begin(ModContext context,
            RuntimeTestRequest request)
        { return new Session(context, request); }

        private sealed class ActionEvidence
        {
            [JsonProperty("stage", Order = 1)]
            public string Stage { get; set; }
            [JsonProperty("turnBased", Order = 2)]
            public bool TurnBased { get; set; }
            [JsonProperty("round", Order = 3)]
            public int Round { get; set; }
            [JsonProperty("currentTurn", Order = 4)]
            public string CurrentTurn { get; set; }
            [JsonProperty("protectorIsCurrentTurn", Order = 5)]
            public bool ProtectorIsCurrentTurn { get; set; }
            [JsonProperty("turnStatus", Order = 6)]
            public string TurnStatus { get; set; }
            [JsonProperty("hasSwiftAction", Order = 7)]
            public bool HasSwiftAction { get; set; }
            [JsonProperty("swiftCooldown", Order = 8)]
            public float SwiftCooldown { get; set; }
            [JsonProperty("standardCooldown", Order = 9)]
            public float StandardCooldown { get; set; }
            [JsonProperty("moveCooldown", Order = 10)]
            public float MoveCooldown { get; set; }
            [JsonProperty("flatFooted", Order = 11)]
            public bool FlatFooted { get; set; }
            [JsonProperty("debt", Order = 12)]
            public string Debt { get; set; }
            [JsonProperty("available", Order = 13)]
            public bool Available { get; set; }
            [JsonProperty("reason", Order = 14)]
            public string Reason { get; set; }
        }

        private sealed class AttackEvidence
        {
            [JsonProperty("name", Order = 1)]
            public string Name { get; set; }
            [JsonProperty("attackIdentity", Order = 2)]
            public int AttackIdentity { get; set; }
            [JsonProperty("before", Order = 3)]
            public ActionEvidence Before { get; set; }
            [JsonProperty("after", Order = 4)]
            public ActionEvidence After { get; set; }
            [JsonProperty("aooBefore", Order = 5)]
            public int AooBefore { get; set; }
            [JsonProperty("aooAfter", Order = 6)]
            public int AooAfter { get; set; }
            [JsonProperty("aidControl", Order = 7)]
            public string AidControl { get; set; }
            [JsonProperty("nativeAc", Order = 8)]
            public int NativeAc { get; set; }
            [JsonProperty("bodyguardContribution", Order = 9)]
            public int BodyguardContribution { get; set; }
            [JsonProperty("bodyguardSourceCount", Order = 10)]
            public int BodyguardSourceCount { get; set; }
            [JsonProperty("attackD20", Order = 11)]
            public int AttackD20 { get; set; }
            [JsonProperty("attackBonus", Order = 12)]
            public int AttackBonus { get; set; }
            [JsonProperty("attackTotal", Order = 13)]
            public int AttackTotal { get; set; }
            [JsonProperty("targetAc", Order = 14)]
            public int TargetAc { get; set; }
            [JsonProperty("hit", Order = 15)]
            public bool Hit { get; set; }
            [JsonProperty("criticalThreat", Order = 16)]
            public bool CriticalThreat { get; set; }
            [JsonProperty("confirmationD20", Order = 17)]
            public int ConfirmationD20 { get; set; }
            [JsonProperty("confirmationTotal", Order = 18)]
            public int ConfirmationTotal { get; set; }
            [JsonProperty("criticalConfirmed", Order = 19)]
            public bool CriticalConfirmed { get; set; }
            [JsonProperty("victimHpBefore", Order = 20)]
            public int VictimHpBefore { get; set; }
            [JsonProperty("victimHpAfter", Order = 21)]
            public int VictimHpAfter { get; set; }
            [JsonProperty("protectorHpBefore", Order = 22)]
            public int ProtectorHpBefore { get; set; }
            [JsonProperty("protectorHpAfter", Order = 23)]
            public int ProtectorHpAfter { get; set; }
            [JsonProperty("rollTargetRestored", Order = 24)]
            public bool RollTargetRestored { get; set; }
            [JsonProperty("weaponTargetRestored", Order = 25)]
            public bool WeaponTargetRestored { get; set; }
            [JsonProperty("counters", Order = 26)]
            public string Counters { get; set; }
            [JsonProperty("combatLog", Order = 27)]
            public string CombatLog { get; set; }
            [JsonProperty("observations", Order = 28)]
            public string[] Observations { get; set; }
        }

        private sealed class Evidence
        {
            [JsonProperty("initial", Order = 1)]
            public ActionEvidence Initial { get; set; }
            [JsonProperty("chargedTurn", Order = 2)]
            public ActionEvidence ChargedTurn { get; set; }
            [JsonProperty("afterChargedTurn", Order = 3)]
            public ActionEvidence AfterChargedTurn { get; set; }
            [JsonProperty("attacks", Order = 4)]
            public List<AttackEvidence> Attacks { get; set; }
            [JsonProperty("forcedTurnCount", Order = 5)]
            public int ForcedTurnCount { get; set; }
        }

        internal sealed class Session
        {
            private const int MaximumForcedTurns = 40;
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly Stopwatch _elapsed = Stopwatch.StartNew();
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics = new List<string>();
            private readonly List<string> _warnings = new List<string>();
            private readonly List<string> _files = new List<string>();
            private readonly Evidence _evidence = new Evidence {
                Attacks = new List<AttackEvidence>() };
            private UnitEntityData _protector;
            private UnitEntityData _victim;
            private UnitEntityData _attacker;
            private ItemEntityWeapon _spear;
            private TurnController _lastForced;
            private int _stage;
            private int _forcedTurns;
            private bool _initialPause;
            private bool _pauseCaptured;

            internal Session(ModContext context, RuntimeTestRequest request)
            {
                if (context == null) throw new ArgumentNullException("context");
                if (request == null) throw new ArgumentNullException("request");
                _context = context;
                _request = request;
            }

            internal bool Complete { get; private set; }
            internal RuntimeTestResult Result { get; private set; }

            internal void Poll()
            {
                if (Complete) return;
                try
                {
                    if (_elapsed.Elapsed.TotalSeconds > Math.Min(180,
                            _request.CompletionTimeoutSeconds))
                        throw new TimeoutException("Off-turn economy stage " +
                            _stage + " did not complete in time.");
                    switch (_stage)
                    {
                        case 0:
                            Initialize();
                            RunInitialAttacks();
                            BeginAdvance();
                            _stage = 1;
                            break;
                        case 1:
                            PollUntilProtectorTurn(2);
                            break;
                        case 2:
                            ObserveChargedTurnAndCompleteIt();
                            _stage = 3;
                            break;
                        case 3:
                            PollUntilAttackerTurn(4);
                            break;
                        case 4:
                            RunRefreshAttack();
                            BeginAdvance();
                            _stage = 5;
                            break;
                        case 5:
                            PollUntilProtectorTurn(6);
                            break;
                        case 6:
                            CompleteSecondChargedTurn();
                            _stage = 7;
                            break;
                        case 7:
                            PollUntilAttackerTurn(8);
                            break;
                        case 8:
                            RunCriticalAttack();
                            Finish();
                            break;
                        default:
                            throw new InvalidOperationException(
                                "Unknown off-turn economy stage " + _stage);
                    }
                }
                catch (Exception exception)
                {
                    Add("off-turn-economy-exception", "no exception",
                        "stage=" + _stage + ";" + exception, false,
                        "guarded disposable human-save session");
                    _diagnostics.Add("exception=" + exception);
                    Finish();
                }
            }

            private void Initialize()
            {
                _protector = RequireUnit(ProtectorId,
                    "HelpfulDefenderTest");
                _victim = RequireUnit(VictimId, "VictimTest");
                _attacker = RequireUnit(AttackerId, "Kobold");
                _spear = _attacker.Body.PrimaryHand.MaybeWeapon;
                if (_spear == null || _spear.Blueprint == null ||
                    !_spear.Blueprint.IsMelee)
                    throw new InvalidOperationException(
                        "The exact Kobold no longer carries its native melee spear.");
                RequireModes();
                if (!CombatController.IsInTurnBasedCombat())
                    throw new InvalidOperationException(
                        "The human-repro save is not in turn-based combat.");
                TurnController turn = CurrentTurn();
                if (turn == null || !ReferenceEquals(turn.Unit, _attacker))
                    throw new InvalidOperationException(
                        "The exact initial current turn is not the Kobold.");
                _initialPause = Game.Instance.IsPaused;
                _pauseCaptured = true;
                Game.Instance.IsPaused = true;
                _evidence.Initial = CaptureAction("initial-enemy-turn");
                ActionEvidence initial = _evidence.Initial;
                Add("off-turn-human-state",
                    "Kobold current, protector off-turn, no debt, exact native action observation",
                    Describe(initial), initial.TurnBased &&
                        !initial.ProtectorIsCurrentTurn &&
                        initial.CurrentTurn.StartsWith(AttackerId,
                            StringComparison.Ordinal) &&
                        initial.Debt == ImmediateActionDebtState.None.ToString() &&
                        initial.Available && !initial.FlatFooted,
                    "live turn controller and RuleCheckTargetFlatFooted");
            }

            private void RunInitialAttacks()
            {
                AttackEvidence first = Attack("off-turn-idle-intercepts",
                    ExactHitRoll(), null);
                _evidence.Attacks.Add(first);
                Add("off-turn-idle-intercepts",
                    "real +4 Bodyguard hit redirects complete native damage and creates next-turn debt",
                    Describe(first), Intercepted(first, false) &&
                        first.AttackTotal == first.TargetAc &&
                        first.After.Debt == ImmediateActionDebtState
                            .PendingNextTurn.ToString(),
                    "native RuleAttackWithWeapon, RuleDealDamage, HP, and fact state");

                PrepareHealth();
                AttackEvidence second = Attack(
                    "second-before-charged-turn-denied", ExactHitRoll(), null);
                _evidence.Attacks.Add(second);
                Add("second-immediate-denied",
                    "a second hit before the charged turn damages only the victim and preserves pending debt",
                    Describe(second), Rejected(second,
                        "immediate-debt-pending-next-turn") &&
                        second.After.Debt == ImmediateActionDebtState
                            .PendingNextTurn.ToString(),
                    "native delivery plus persistent exact-reason combat log");
            }

            private void ObserveChargedTurnAndCompleteIt()
            {
                Game.Instance.IsPaused = true;
                _evidence.ChargedTurn = CaptureAction("charged-own-turn");
                ActionEvidence charged = _evidence.ChargedTurn;
                Add("next-actual-turn-swift-debt",
                    "pending debt becomes charged; swift is unavailable while standard and move remain fresh",
                    Describe(charged), charged.ProtectorIsCurrentTurn &&
                        charged.Debt == ImmediateActionDebtState.ChargedTurn
                            .ToString() && !charged.HasSwiftAction &&
                        charged.SwiftCooldown >= 5.999f &&
                        charged.StandardCooldown <= 0.001f &&
                        charged.MoveCooldown <= 0.001f,
                    "TurnController.Prepare/Cooldowns.Clear and native HasSwiftAction");
                TurnController turn = CurrentTurn();
                if (turn == null || !ReferenceEquals(turn.Unit, _protector))
                    throw new InvalidOperationException(
                        "The charged protector turn disappeared before completion.");
                Game.Instance.IsPaused = false;
                turn.ForceToEnd(true);
                _lastForced = turn;
                _forcedTurns++;
            }

            private void RunRefreshAttack()
            {
                Game.Instance.IsPaused = true;
                _evidence.AfterChargedTurn = CaptureAction(
                    "enemy-turn-after-charged-turn");
                ActionEvidence refreshed = _evidence.AfterChargedTurn;
                Add("actual-turn-completion-refreshes-immediate",
                    "debt clears only after the charged actual turn completes",
                    Describe(refreshed), !refreshed.ProtectorIsCurrentTurn &&
                        refreshed.Debt == ImmediateActionDebtState.None
                            .ToString() && refreshed.Available,
                    "TurnController.Dispose transition");
                PrepareHealth();
                AttackEvidence attack = Attack("post-turn-refresh-intercepts",
                    ExactHitRoll(), null);
                _evidence.Attacks.Add(attack);
                Add("post-turn-refresh-intercepts",
                    "a later enemy-turn hit can be intercepted again exactly once",
                    Describe(attack), Intercepted(attack, false) &&
                        attack.After.Debt == ImmediateActionDebtState
                            .PendingNextTurn.ToString(),
                    "native HP recipient and refreshed debt transaction");
            }

            private void CompleteSecondChargedTurn()
            {
                Game.Instance.IsPaused = true;
                ActionEvidence charged = CaptureAction(
                    "second-charged-own-turn");
                Add("second-charged-turn-swift-denied",
                    "the refreshed interception charges the next actual turn once",
                    Describe(charged), charged.ProtectorIsCurrentTurn &&
                        charged.Debt == ImmediateActionDebtState.ChargedTurn
                            .ToString() && !charged.HasSwiftAction,
                    "native turn and KMG debt fact");
                TurnController turn = CurrentTurn();
                Game.Instance.IsPaused = false;
                turn.ForceToEnd(true);
                _lastForced = turn;
                _forcedTurns++;
            }

            private void RunCriticalAttack()
            {
                Game.Instance.IsPaused = true;
                ActionEvidence before = CaptureAction(
                    "critical-enemy-turn-before");
                if (before.Debt != ImmediateActionDebtState.None.ToString() ||
                    !before.Available)
                    throw new InvalidOperationException(
                        "Immediate action did not refresh before critical case: " +
                        Describe(before));
                PrepareHealth();
                AttackEvidence attack = Attack(
                    "off-turn-confirmed-critical-intercepts", 20, 20);
                _evidence.Attacks.Add(attack);
                Add("off-turn-confirmed-critical-intercepts",
                    "natural 20 and confirmation remain confirmed; victim loses zero and protector receives one complete critical delivery",
                    Describe(attack), Intercepted(attack, true) &&
                        attack.AttackD20 == 20 &&
                        attack.ConfirmationD20 == 20,
                    "native critical state, RuleDealDamage target, HP, and target restoration");
            }

            private void PollUntilProtectorTurn(int nextStage)
            {
                TurnController turn = CurrentTurn();
                if (turn == null) return;
                if (ReferenceEquals(turn.Unit, _protector))
                {
                    if (ImmediateActionEconomyRuntime.ObserveDebt(_protector) ==
                        ImmediateActionDebtState.ChargedTurn)
                    {
                        _lastForced = null;
                        _stage = nextStage;
                    }
                    return;
                }
                ForceCurrentOnce(turn);
            }

            private void PollUntilAttackerTurn(int nextStage)
            {
                TurnController turn = CurrentTurn();
                if (turn == null) return;
                if (ReferenceEquals(turn.Unit, _attacker))
                {
                    _lastForced = null;
                    _stage = nextStage;
                    return;
                }
                ForceCurrentOnce(turn);
            }

            private void ForceCurrentOnce(TurnController turn)
            {
                if (ReferenceEquals(turn, _lastForced)) return;
                if (_forcedTurns >= MaximumForcedTurns)
                    throw new InvalidOperationException(
                        "The native initiative loop did not reach the expected unit.");
                Game.Instance.IsPaused = false;
                turn.ForceToEnd(true);
                _lastForced = turn;
                _forcedTurns++;
            }

            private void BeginAdvance()
            {
                Game.Instance.IsPaused = false;
                _lastForced = null;
            }

            private AttackEvidence Attack(string name, int incoming,
                int? confirmation)
            {
                BodyguardRuntimeDiagnostics.Reset();
                BodyguardQualificationControl.Clear();
                ActionEvidence before = CaptureAction(name + "-before");
                int aooBefore = _protector.CombatState
                    .AttackOfOpportunityCount;
                int victimBefore = _victim.HPLeft;
                int protectorBefore = _protector.HPLeft;
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
                    attack = new RuleAttackWithWeapon(_attacker, _victim,
                        _spear, 0) { Maximized = true };
                    Rulebook.Trigger(attack);
                }
                finally
                { control = BodyguardQualificationControl.DescribeAndClear(); }
                if (attack == null || attack.AttackRoll == null)
                    throw new InvalidOperationException(
                        "The native spear attack exposed no RuleAttackRoll.");
                RuleAttackRoll roll = attack.AttackRoll;
                int[] sources = BodyguardSources(roll);
                string[] observations = BodyguardRuntimeDiagnostics
                    .SnapshotObservations();
                return new AttackEvidence {
                    Name = name,
                    AttackIdentity = RuntimeHelpers.GetHashCode(roll),
                    Before = before,
                    After = CaptureAction(name + "-after"),
                    AooBefore = aooBefore,
                    AooAfter = _protector.CombatState
                        .AttackOfOpportunityCount,
                    AidControl = control,
                    NativeAc = roll.TargetAC - sources.Sum(),
                    BodyguardContribution = sources.Sum(),
                    BodyguardSourceCount = sources.Length,
                    AttackD20 = roll.Roll,
                    AttackBonus = roll.AttackBonus,
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
                    VictimHpBefore = victimBefore,
                    VictimHpAfter = _victim.HPLeft,
                    ProtectorHpBefore = protectorBefore,
                    ProtectorHpAfter = _protector.HPLeft,
                    RollTargetRestored = ReferenceEquals(roll.Target, _victim),
                    WeaponTargetRestored = ReferenceEquals(attack.Target,
                        _victim),
                    Counters = "frames=" +
                        BodyguardRuntimeDiagnostics.Frames + ";attempts=" +
                        BodyguardRuntimeDiagnostics.Attempts +
                        ";interceptions=" +
                        BodyguardRuntimeDiagnostics.Interceptions +
                        ";completed=" +
                        BodyguardRuntimeDiagnostics.Completed +
                        ";faults=" + BodyguardRuntimeDiagnostics.Faults +
                        ";duplicates=" +
                        BodyguardRuntimeDiagnostics.DuplicateCallbacks,
                    CombatLog = BodyguardCombatLog.Attempts == logBefore ?
                        string.Empty : BodyguardCombatLog.LastMessage ??
                        string.Empty,
                    Observations = observations
                };
            }

            private int ExactHitRoll()
            {
                int nativeAc = Rulebook.Trigger(new RuleCalculateAC(_attacker,
                    _victim, AttackType.Melee)).TargetAC;
                var attackBonus = new RuleCalculateAttackBonus(_attacker,
                    _victim, _spear, 0);
                Rulebook.Trigger(attackBonus);
                int roll = checked(nativeAc + 4 - attackBonus.Result);
                if (roll <= 1 || roll >= 20)
                    throw new InvalidOperationException(
                        "The exact total-equals-AC roll is not a noncritical d20: " +
                        roll + ";nativeAc=" + nativeAc + ";attackBonus=" +
                        attackBonus.Result + ".");
                return roll;
            }

            private ActionEvidence CaptureAction(string stage)
            {
                BodyguardImmediateActionSnapshot snapshot =
                    BodyguardActionEconomyAccess.ObserveImmediateAction(
                        _protector, _attacker);
                TurnController turn = CurrentTurn();
                var controller = Game.Instance.TurnBasedCombatController;
                return new ActionEvidence {
                    Stage = stage,
                    TurnBased = snapshot.TurnBased,
                    Round = controller == null ? -1 : controller.RoundNumber,
                    CurrentTurn = Identity(turn == null ? null : turn.Unit),
                    ProtectorIsCurrentTurn = snapshot.ProtectorIsCurrentTurn,
                    TurnStatus = turn == null ? "<null>" :
                        turn.Status.ToString(),
                    HasSwiftAction = snapshot.HasSwiftAction,
                    SwiftCooldown = snapshot.SwiftCooldown,
                    StandardCooldown = snapshot.StandardCooldown,
                    MoveCooldown = snapshot.MoveCooldown,
                    FlatFooted = snapshot.FlatFooted,
                    Debt = snapshot.DebtState.ToString(),
                    Available = snapshot.Available,
                    Reason = snapshot.Reason
                };
            }

            private void RequireModes()
            {
                BodyguardFeatBlueprintSet set = BlueprintBootstrap
                    .BodyguardFeats;
                if (set == null || !ExactFact(_protector, set.Bodyguard) ||
                    !ExactFact(_protector, set.InHarmsWay))
                    throw new InvalidOperationException(
                        "The exact protector lacks Bodyguard or In Harm's Way.");
                ActivatableAbility bodyguard = FindMode(_protector,
                    set.Modes.BodyguardAbility);
                ActivatableAbility inHarmsWay = FindMode(_protector,
                    set.Modes.InHarmsWayAbility);
                bool markers = _protector.Descriptor.Buffs.GetBuff(
                        set.Modes.BodyguardMarker) != null &&
                    _protector.Descriptor.Buffs.GetBuff(
                        set.Modes.InHarmsWayMarker) != null;
                if (bodyguard == null || inHarmsWay == null ||
                    !bodyguard.IsOn || !bodyguard.IsRunning ||
                    !inHarmsWay.IsOn || !inHarmsWay.IsRunning || !markers)
                    throw new InvalidOperationException(
                        "The exact real activatables and marker buffs are not synchronized and active.");
            }

            private void PrepareHealth()
            {
                if (_victim.HPLeft <= 10 || _protector.HPLeft <= 10)
                    throw new InvalidOperationException(
                        "The disposable units lack enough remaining HP for native delivery evidence.");
            }

            private void Finish()
            {
                if (Complete) return;
                try
                {
                    _evidence.ForcedTurnCount = _forcedTurns;
                    string path = Path.Combine(_request.EvidenceDirectory,
                        EvidenceFileName);
                    File.WriteAllText(path, JsonConvert.SerializeObject(
                        _evidence, Formatting.Indented));
                    _files.Add(path);
                    _diagnostics.Add("evidence=" + path);
                    foreach (AttackEvidence value in _evidence.Attacks)
                        _diagnostics.Add(Describe(value));
                }
                catch (Exception exception)
                {
                    _diagnostics.Add("evidence-write-exception=" + exception);
                    Add("off-turn-evidence-write", "evidence written",
                        exception.ToString(), false,
                        "request-local evidence directory");
                }
                finally
                {
                    BodyguardQualificationControl.Clear();
                    BodyguardRuntime.ClearAll(
                        "off-turn-economy-scenario-finally");
                    if (_pauseCaptured && Game.Instance != null)
                        Game.Instance.IsPaused = _initialPause;
                }
                bool pass = _assertions.Count > 0 && _assertions.All(value =>
                    value.Status == RuntimeTestStatuses.Pass) &&
                    _diagnostics.All(value => !value.StartsWith("exception=",
                        StringComparison.Ordinal) && !value.StartsWith(
                        "evidence-write-exception=", StringComparison.Ordinal));
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                Result = new RuntimeTestResult {
                    SchemaVersion = 1, RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    Status = pass ? RuntimeTestStatuses.Pass :
                        RuntimeTestStatuses.Fail,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = _context.Assembly.FullName + ";pid=" +
                        Process.GetCurrentProcess().Id,
                    GitCommit = identity.GitCommit,
                    GameVersion = UnityEngine.Application.version ??
                        string.Empty,
                    StartUtc = _started.ToString("o"), EndUtc = string.Empty,
                    Assertions = _assertions, Diagnostics = _diagnostics,
                    Warnings = _warnings,
                    ExceptionSummary = _diagnostics.FirstOrDefault(value =>
                        value.StartsWith("exception=", StringComparison.Ordinal))
                        ?? string.Empty,
                    EvidenceFiles = _files,
                    AutomaticExitRequested = _request.ExitAfterCompletion,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
                Complete = true;
            }

            private bool Intercepted(AttackEvidence value, bool critical)
            {
                return value.BodyguardContribution == 4 &&
                    value.BodyguardSourceCount == 1 && value.Hit &&
                    value.CriticalConfirmed == critical &&
                    value.AooAfter == value.AooBefore - 1 &&
                    value.VictimHpAfter == value.VictimHpBefore &&
                    value.ProtectorHpAfter < value.ProtectorHpBefore &&
                    value.RollTargetRestored && value.WeaponTargetRestored &&
                    Has(value, "decision=eligible") &&
                    Has(value, "stage=rule-deal-damage-prefix") &&
                    Has(value, "deliveryRecipient=" + ProtectorId) &&
                    value.Counters.Contains("interceptions=1") &&
                    value.Counters.Contains("faults=0") &&
                    value.Counters.Contains("duplicates=0");
            }

            private bool Rejected(AttackEvidence value, string reason)
            {
                return value.BodyguardContribution == 4 && value.Hit &&
                    value.AooAfter == value.AooBefore - 1 &&
                    value.VictimHpAfter < value.VictimHpBefore &&
                    value.ProtectorHpAfter == value.ProtectorHpBefore &&
                    value.RollTargetRestored && value.WeaponTargetRestored &&
                    Has(value, "decision=" + reason) &&
                    value.CombatLog.IndexOf("cannot use In Harm's Way",
                        StringComparison.Ordinal) >= 0 &&
                    value.Counters.Contains("interceptions=0") &&
                    value.Counters.Contains("faults=0");
            }

            private static int[] BodyguardSources(RuleAttackRoll roll)
            {
                BodyguardFeatBlueprintSet set = BlueprintBootstrap
                    .BodyguardFeats;
                if (roll == null || roll.ACRule == null ||
                    roll.ACRule.BonusSources == null || set == null)
                    return new int[0];
                return roll.ACRule.BonusSources.Where(value => value.Source !=
                        null && value.Source.Blueprint != null &&
                        string.Equals(value.Source.Blueprint.AssetGuid,
                            set.Bodyguard.AssetGuid, StringComparison.Ordinal))
                    .Select(value => value.Bonus).ToArray();
            }

            private static bool Has(AttackEvidence value, string token)
            {
                return value != null && value.Observations != null &&
                    value.Observations.Any(item => item != null &&
                        item.IndexOf(token, StringComparison.Ordinal) >= 0);
            }

            private static UnitEntityData RequireUnit(string id, string name)
            {
                UnitEntityData[] matches = Game.Instance.State.Units.All
                    .Where(value => value != null && string.Equals(
                        value.UniqueId, id, StringComparison.Ordinal) &&
                        string.Equals(value.CharacterName, name,
                            StringComparison.Ordinal)).ToArray();
                if (matches.Length != 1)
                    throw new InvalidOperationException("Expected one exact " +
                        name + " but found " + matches.Length + ".");
                return matches[0];
            }

            private static ActivatableAbility FindMode(UnitEntityData unit,
                Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility blueprint)
            {
                return unit.Descriptor.ActivatableAbilities.Enumerable
                    .SingleOrDefault(value => value != null &&
                        ReferenceEquals(value.Blueprint, blueprint));
            }

            private static bool ExactFact(UnitEntityData unit,
                BlueprintFeature feature)
            {
                Fact fact = unit.Descriptor.GetFact(feature);
                return fact != null && ReferenceEquals(fact.Blueprint,
                    feature);
            }

            private static TurnController CurrentTurn()
            {
                return Game.Instance == null ||
                    Game.Instance.TurnBasedCombatController == null ? null :
                    Game.Instance.TurnBasedCombatController.CurrentTurn;
            }

            private static string Identity(UnitEntityData unit)
            {
                return unit == null ? "<null>" : (unit.UniqueId ??
                    "<no-id>") + "/" + (unit.CharacterName ?? "<unnamed>");
            }

            private static string Describe(ActionEvidence value)
            {
                if (value == null) return "<null>";
                return "stage=" + value.Stage + ";round=" + value.Round +
                    ";currentTurn=" + value.CurrentTurn +
                    ";protectorIsCurrentTurn=" +
                    value.ProtectorIsCurrentTurn + ";turnStatus=" +
                    value.TurnStatus + ";hasSwiftAction=" +
                    value.HasSwiftAction + ";swift=" +
                    value.SwiftCooldown.ToString("R",
                        CultureInfo.InvariantCulture) + ";standard=" +
                    value.StandardCooldown.ToString("R",
                        CultureInfo.InvariantCulture) + ";move=" +
                    value.MoveCooldown.ToString("R",
                        CultureInfo.InvariantCulture) + ";flatFooted=" +
                    value.FlatFooted + ";debt=" + value.Debt +
                    ";available=" + value.Available + ";reason=" +
                    value.Reason;
            }

            private static string Describe(AttackEvidence value)
            {
                if (value == null) return "<null>";
                return "case=" + value.Name + ";attack=" +
                    value.AttackIdentity + ";d20=" + value.AttackD20 +
                    ";bonus=" + value.AttackBonus + ";total=" +
                    value.AttackTotal + ";ac=" + value.TargetAc +
                    ";bodyguard=" + value.BodyguardContribution +
                    ";hit=" + value.Hit + ";critical=" +
                    value.CriticalConfirmed + ";aoo=" + value.AooBefore +
                    "->" + value.AooAfter + ";victimHp=" +
                    value.VictimHpBefore + "->" + value.VictimHpAfter +
                    ";protectorHp=" + value.ProtectorHpBefore + "->" +
                    value.ProtectorHpAfter + ";debt=" + value.Before.Debt +
                    "->" + value.After.Debt + ";" + value.Counters;
            }

            private void Add(string name, string expected, string observed,
                bool passed, string evidence)
            {
                _assertions.Add(new RuntimeTestAssertion { Name = name,
                    Expected = expected, Observed = observed,
                    Status = passed ? RuntimeTestStatuses.Pass :
                        RuntimeTestStatuses.Fail, Evidence = evidence });
            }
        }
    }
}
