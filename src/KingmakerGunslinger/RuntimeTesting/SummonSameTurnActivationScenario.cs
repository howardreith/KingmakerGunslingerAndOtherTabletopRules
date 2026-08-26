using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Acadamae;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded save-backed real-spellbook reproduction for the accelerated
    /// summon timing defect. The fixture owns only disposable units and never
    /// invokes a direct spawn action; every observed summon must traverse the
    /// native UnitUseAbility -> RuleCastSpell -> ContextActionSpawnMonster ->
    /// RuleSummonUnit chain.
    /// </summary>
    internal static class SummonSameTurnActivationScenario
    {
        private const string EvidenceFileName =
            "summon-same-turn-activation.json";
        private const string WizardGuid =
            "ba34257984f4c41408ce1dc2004e342e";
        private const string SummonMonsterOneGuid =
            "8fd74eddd9b6c224693d9ab241f25e84";
        private const string SummonMonsterThreeGuid =
            "5d61dde0020bbf54ba1521f7ca0229dc";
        private const string NativeDogName =
            "KMG_Summoning_Native_SM_Tier1";
        private const string ExpandedEagleMultipleName =
            "KMG_Summoning_Ability_SM_Tier3_Eagle_OneD4PlusOne";
        private static readonly object ActiveGate = new object();
        private static Session _active;

        private enum ScenarioKind
        {
            Quickened,
            Acadamae,
            Multiple,
            NativeControl,
            RtwpControl
        }

        internal static Session Begin(ModContext context,
            RuntimeTestRequest request)
        {
            lock (ActiveGate)
            {
                if (_active != null)
                    throw new InvalidOperationException(
                        "A summon activation scenario is already active.");
                _active = new Session(context, request);
                return _active;
            }
        }

        private static Session Active
        {
            get { lock (ActiveGate) return _active; }
        }

        private static void Release(Session session)
        {
            lock (ActiveGate)
                if (ReferenceEquals(_active, session)) _active = null;
        }

        private static ScenarioKind ResolveKind(string scenario)
        {
            if (scenario == RuntimeTestScenarioCatalog.SummonSameTurnActivation)
                return ScenarioKind.Quickened;
            if (scenario == RuntimeTestScenarioCatalog.SummonSameTurnAcadamae)
                return ScenarioKind.Acadamae;
            if (scenario == RuntimeTestScenarioCatalog.SummonSameTurnMultiple)
                return ScenarioKind.Multiple;
            if (scenario == RuntimeTestScenarioCatalog
                .SummonSameTurnNativeControl)
                return ScenarioKind.NativeControl;
            if (scenario == RuntimeTestScenarioCatalog
                .SummonSameTurnRtwpControl)
                return ScenarioKind.RtwpControl;
            throw new ArgumentException("Unsupported summon activation scenario: " +
                scenario, "scenario");
        }

        internal sealed class Session
        {
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly ScenarioKind _kind;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly Stopwatch _elapsed = Stopwatch.StartNew();
            private readonly List<RuntimeTestAssertion> _assertions =
                new List<RuntimeTestAssertion>();
            private readonly List<string> _diagnostics =
                new List<string>();
            private readonly List<string> _warnings = new List<string>();
            private readonly List<string> _files = new List<string>();
            private readonly Evidence _evidence = new Evidence();
            private readonly List<UnitEntityData> _summons =
                new List<UnitEntityData>();
            private readonly Dictionary<UnitEntityData, RuleSummonUnit>
                _summonRules = new Dictionary<UnitEntityData, RuleSummonUnit>();
            private readonly List<string> _turns = new List<string>();
            private readonly HashSet<TurnController> _preparedSummonTurns =
                new HashSet<TurnController>();
            private readonly Dictionary<UnitEntityData, bool> _combatBefore =
                new Dictionary<UnitEntityData, bool>();
            private readonly Dictionary<UnitEntityData, int> _sameTurnsByUnit =
                new Dictionary<UnitEntityData, int>();
            private readonly Dictionary<UnitEntityData, int> _sameLawfulByUnit =
                new Dictionary<UnitEntityData, int>();
            private readonly Dictionary<UnitEntityData, int> _nextTurnsByUnit =
                new Dictionary<UnitEntityData, int>();
            private readonly Dictionary<UnitEntityData, int> _nextLawfulByUnit =
                new Dictionary<UnitEntityData, int>();
            private readonly Dictionary<UnitEntityData, int>
                _followingTurnsByUnit =
                    new Dictionary<UnitEntityData, int>();
            private readonly Dictionary<UnitEntityData, int>
                _followingLawfulByUnit =
                    new Dictionary<UnitEntityData, int>();
            private readonly Dictionary<UnitEntityData, int> _sameCommandsByUnit =
                new Dictionary<UnitEntityData, int>();
            private readonly Dictionary<UnitEntityData, int> _nextCommandsByUnit =
                new Dictionary<UnitEntityData, int>();
            private readonly Dictionary<UnitEntityData, int> _sameAttacksByUnit =
                new Dictionary<UnitEntityData, int>();
            private UnitEntityData _areaAnchor;
            private UnitEntityData _caster;
            private UnitEntityData _enemy;
            private BlueprintUnit _casterBlueprint;
            private BlueprintUnit _enemyBlueprint;
            private Spellbook _spellbook;
            private AbilityData _castAbility;
            private UnitUseAbility _castCommand;
            private RuleSummonUnit _summonRule;
            private SpellSlot _castSlot;
            private ActivatableAbility _acadamaeMode;
            private BlueprintUnit _nonSummonBlueprint;
            private UnitEntityData _nonSummonControlUnit;
            private object _levelController;
            private object[] _unitsBefore;
            private object[] _partyBefore;
            private bool _initialPause;
            private bool _initialTurnBasedSetting;
            private bool _stateCaptured;
            private bool _fixtureJoinedCombat;
            private int _stage;
            private int _forcedTurns;
            private int _castRound;
            private int _firstLawfulSummonTurnRound;
            private int _sameRoundSummonTurns;
            private int _sameRoundLawfulSummonTurns;
            private int _nextRoundSummonTurns;
            private int _nextRoundLawfulSummonTurns;
            private int _followingRoundSummonTurns;
            private int _followingRoundLawfulSummonTurns;
            private int _firstSummonCommandRound;
            private int _sameRoundSummonCommands;
            private int _nextRoundSummonCommands;
            private int _firstSummonAttackRound;
            private int _sameRoundSummonAttacks;
            private int _nextRoundSummonAttacks;
            private bool _castCaptureActive;
            private int _acadamaeCompletedBefore;
            private long _acadamaePublicationBefore;
            private TurnController _lastTurn;
            private TurnController _lastForcedTurn;
            private string _failureStage = "not-started";

            internal Session(ModContext context, RuntimeTestRequest request)
            {
                if (context == null) throw new ArgumentNullException("context");
                if (request == null) throw new ArgumentNullException("request");
                _context = context;
                _request = request;
                _kind = ResolveKind(request.Scenario);
                _evidence.Case = _kind.ToString();
            }

            internal bool Complete { get; private set; }
            internal RuntimeTestResult Result { get; private set; }

            internal void Poll()
            {
                if (Complete) return;
                try
                {
                    if (_elapsed.Elapsed.TotalSeconds > Math.Min(240,
                            _request.CompletionTimeoutSeconds))
                        throw new TimeoutException("Summon activation stage " +
                            _stage + " did not complete in time.");
                    switch (_stage)
                    {
                        case 0:
                            Initialize();
                            _stage = 1;
                            break;
                        case 1:
                            PollUntilCasterTurn();
                            break;
                        case 2:
                            CastSummon();
                            _stage = 3;
                            break;
                        case 3:
                            PollSummonResolution();
                            break;
                        case 4:
                            PollSummonOpportunity();
                            break;
                        case 5:
                            Finish();
                            break;
                        default:
                            throw new InvalidOperationException(
                                "Unknown summon activation stage " + _stage + ".");
                    }
                }
                catch (Exception exception)
                {
                    _diagnostics.Add("exception=" + exception);
                    _diagnostics.Add("failureStage=" + _failureStage);
                    Finish();
                }
            }

            private void Initialize()
            {
                _failureStage = "fixture-initialize";
                SummonAcceleratedInvocationRuntime.ResetDiagnostics();
                if (Game.Instance == null || Game.Instance.Player == null ||
                    Game.Instance.State == null)
                    throw new InvalidOperationException(
                        "No loaded Kingmaker game state is available.");
                _initialPause = Game.Instance.IsPaused;
                _initialTurnBasedSetting = SettingsRoot.Instance
                    .EnableTurnBasedMode.CurrentValue;
                _stateCaptured = true;
                if (Game.Instance.Player.IsInCombat)
                    throw new InvalidOperationException(
                        "The guarded working save unexpectedly began in combat.");
                _unitsBefore = Game.Instance.State.Units.All.Cast<object>()
                    .ToArray();
                _partyBefore = Game.Instance.Player.Party.Cast<object>()
                    .ToArray();
                foreach (UnitEntityData unit in _unitsBefore
                    .OfType<UnitEntityData>())
                    _combatBefore[unit] = unit.CombatState != null &&
                        unit.CombatState.IsInCombat;
                _areaAnchor = Game.Instance.Player.Party.FirstOrDefault(value =>
                    value != null && value.HoldingState != null &&
                    value.IsInState);
                if (_areaAnchor == null)
                    throw new InvalidOperationException(
                        "The guarded working save has no live party area anchor.");

                _casterBlueprint = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                _casterBlueprint.name =
                    "KMG_Runtime_SummonSameTurn_Caster";
                _casterBlueprint.IsCheater = false;
                _caster = Game.Instance.EntityCreator.SpawnUnit(
                    _casterBlueprint, _areaAnchor.Position,
                    Quaternion.identity, _areaAnchor.HoldingState);
                Game.Instance.EntityCreator.Tick();
                if (_caster == null || !_caster.IsInState ||
                    _caster.View == null || _caster.View.Data != _caster)
                    throw new InvalidOperationException(
                        "The disposable caster did not enter the live area.");
                if (!Game.Instance.Player.Party.Contains(_caster))
                    Game.Instance.Player.Party.Add(_caster);
                _caster.Descriptor.Stats.HitPoints.BaseValue = 10000;
                _caster.Descriptor.Stats.Intelligence.BaseValue = 30;
                _caster.Descriptor.State.Immortality.Retain();

                BlueprintCharacterClass wizard = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, WizardGuid,
                        "native Wizard summon activation spellbook");
                int casterLevels = _kind == ScenarioKind.Acadamae ||
                    _kind == ScenarioKind.NativeControl ? 2 : 20;
                AdvanceSpellcaster(_caster.Descriptor, wizard, casterLevels,
                    ref _levelController);
                _spellbook = _caster.Descriptor.GetSpellbook(wizard);
                if (_spellbook == null)
                    throw new InvalidOperationException(
                        "The disposable caster has no Wizard spellbook.");
                int wizardLevel = _caster.Descriptor.Progression
                    .GetClassLevel(wizard);
                while (_spellbook.CasterLevel < wizardLevel)
                    _spellbook.AddCasterLevel();
                _spellbook.UpdateAllSlotsSize(false);
                _spellbook.Rest();
                _castAbility = PrepareCaseAbility();

                if (_kind == ScenarioKind.Acadamae ||
                    _kind == ScenarioKind.NativeControl)
                    ConfigureAcadamaeMode(_kind == ScenarioKind.Acadamae);

                _enemy = ElvenBranchedSpearCombatScenario
                    .SpawnHostileTarget(_caster, _casterBlueprint,
                        _caster.Position + new Vector3(3f, 0f, 0f),
                        _caster.HoldingState, out _enemyBlueprint);
                Game.Instance.EntityCreator.Tick();
                if (_enemy == null || !_enemy.IsInState ||
                    !_enemy.IsEnemy(_caster))
                    throw new InvalidOperationException(
                        "The disposable hostile did not enter the live area.");
                _enemy.Descriptor.Stats.HitPoints.BaseValue = 10000;
                _enemy.Descriptor.State.Immortality.Retain();

                bool turnBased = _kind != ScenarioKind.RtwpControl;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                    turnBased;
                Game.Instance.TurnBasedCombatController.Activate();
                _caster.JoinCombat();
                _enemy.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                _fixtureJoinedCombat = true;
                if (!Game.Instance.Player.IsInCombat)
                    throw new InvalidOperationException(
                        "The disposable player caster did not establish party combat.");
                if (turnBased)
                    Game.Instance.TurnBasedCombatController
                        .HandlePartyCombatStateChanged(true);
                if (CombatController.IsInTurnBasedCombat() != turnBased)
                    throw new InvalidOperationException(
                        "The exact combat mode did not match the requested " +
                        _kind + " control.");
                if (_kind == ScenarioKind.NativeControl)
                    RunNativeNegativeControls();
                Game.Instance.IsPaused = false;
                _evidence.Caster = Identity(_caster);
                _evidence.Enemy = Identity(_enemy);
                _evidence.Spellbook = _spellbook.Blueprint.name + ":" +
                    _spellbook.Blueprint.AssetGuid;
                _evidence.Spell = _castAbility.Blueprint.name + ":" +
                    _castAbility.Blueprint.AssetGuid;
                _diagnostics.Add("fixture=caster=" + _evidence.Caster +
                    ";enemy=" + _evidence.Enemy + ";spellbook=" +
                    _evidence.Spellbook + ";spell=" + _evidence.Spell);
                _failureStage = turnBased ? "wait-caster-turn" :
                    "rtwp-cast-ready";
            }

            private void PollUntilCasterTurn()
            {
                CombatController controller =
                    Game.Instance.TurnBasedCombatController;
                if (_kind == ScenarioKind.RtwpControl)
                {
                    if (CombatController.IsInTurnBasedCombat())
                        throw new InvalidOperationException(
                            "The RTwP control unexpectedly entered turn-based combat.");
                    Game.Instance.IsPaused = true;
                    _stage = 2;
                    return;
                }
                if (!CombatController.IsInTurnBasedCombat() ||
                    !controller.Initialized) return;
                TurnController turn = controller.CurrentTurn;
                if (turn == null) return;
                RecordTurn(turn);
                if (ReferenceEquals(turn.Unit, _caster))
                {
                    _lastForcedTurn = null;
                    Game.Instance.IsPaused = true;
                    _stage = 2;
                    return;
                }
                ForceTurnOnce(turn);
            }

            private void CastSummon()
            {
                _failureStage = _kind.ToString().ToLowerInvariant() +
                    "-real-player-cast";
                CombatController controller =
                    Game.Instance.TurnBasedCombatController;
                TurnController current = controller.CurrentTurn;
                if (_kind != ScenarioKind.RtwpControl &&
                    (current == null || !ReferenceEquals(current.Unit, _caster)))
                    throw new InvalidOperationException(
                        "The caster lost the current turn before the cast.");
                _castRound = _kind == ScenarioKind.RtwpControl ? 0 :
                    controller.RoundNumber;
                _evidence.CastRound = _castRound;
                _evidence.TurnBasedAtCast =
                    CombatController.IsInTurnBasedCombat();
                _evidence.CurrentActorBefore = current == null ? "<rtwp>" :
                    Identity(current.Unit);
                _evidence.ActionType = _castAbility.ActionType.ToString();
                _evidence.RuntimeActionType =
                    _castAbility.RuntimeActionType.ToString();
                _evidence.RequireFullRoundAction =
                    _castAbility.RequireFullRoundAction;
                _evidence.BlueprintFullRound =
                    _castAbility.Blueprint.IsFullRoundAction;
                _evidence.Metamagic = _castAbility.MetamagicData == null ?
                    "<none>" : _castAbility.MetamagicData.MetamagicMask
                        .ToString();
                CaptureCasterActions(true);
                _summons.Clear();
                _summonRules.Clear();
                _summonRule = null;
                _castCaptureActive = true;
                var target = new TargetWrapper(_caster.Position +
                    new Vector3(1.5f, 0f, 0f));
                _evidence.CanTarget = _castAbility.CanTarget(target);
                _castCommand = new UnitUseAbility(_castAbility, target);
                _evidence.CommandType = _castCommand.Type.ToString();
                _evidence.CanStart = _castCommand.CanStart;
                if (_kind == ScenarioKind.Acadamae)
                {
                    _acadamaeCompletedBefore = AcadamaeCastingRuntime
                        .CompletedCount;
                    _acadamaePublicationBefore = AcadamaeCastingRuntime
                        .ResolutionPublicationAttemptCount;
                    AcadamaeSavingThrowTestControl.Queue(20);
                }
                if (!_castAbility.IsAvailable || !_evidence.CanTarget ||
                    !_castCommand.CanStart)
                    throw new InvalidOperationException(
                        "The real summon invocation was unavailable: " +
                        "available=" + _castAbility.IsAvailable +
                        ";target=" + _evidence.CanTarget + ";canStart=" +
                        _castCommand.CanStart + ";reason=" +
                        _castAbility.GetUnavailableReason() + ".");
                _caster.Commands.Run(_castCommand);
                Game.Instance.IsPaused = false;
                _failureStage = _kind.ToString().ToLowerInvariant() +
                    "-native-command-resolution";
            }

            private void PollSummonResolution()
            {
                CombatController controller =
                    Game.Instance.TurnBasedCombatController;
                if (_castCommand == null)
                    throw new InvalidOperationException(
                        "The native summon command was not retained.");
                if (!_castCommand.IsStarted) return;
                if (_castCommand.Animation != null &&
                    !_castCommand.Animation.IsActed)
                    _castCommand.Animation.IsActed = true;
                if (!_castCommand.IsActed) return;
                if (_castCommand.ExecutionProcess == null)
                    throw new InvalidOperationException(
                        "The native summon command acted without creating " +
                        "an ability execution process.");
                if (!_castCommand.ExecutionProcess.IsEnded) return;
                if (_castCommand.Animation != null &&
                    !_castCommand.Animation.IsFinished)
                {
                    FinishAnimation(_castCommand.Animation);
                    return;
                }
                if (!_castCommand.IsFinished) return;
                Game.Instance.EntityCreator.Tick();
                _castCaptureActive = false;
                _evidence.CommandResult = _castCommand.Result.ToString();
                if (!string.Equals(_evidence.CommandResult, "Success",
                        StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "The native summon command did not succeed; result=" +
                        _evidence.CommandResult + ".");
                _evidence.RuleSummonCount = _summons.Count;
                int expectedMinimum = _kind == ScenarioKind.Multiple ? 2 : 1;
                int expectedMaximum = _kind == ScenarioKind.Multiple ? 5 : 1;
                _evidence.ExpectedSummonMinimum = expectedMinimum;
                _evidence.ExpectedSummonMaximum = expectedMaximum;
                if (_summons.Count < expectedMinimum ||
                    _summons.Count > expectedMaximum || _summonRule == null ||
                    _summonRules.Count != _summons.Count)
                    throw new InvalidOperationException(
                        "Unexpected real RuleSummonUnit result count; expected=" +
                        expectedMinimum + ".." + expectedMaximum + ";rules=" +
                        _summons.Count + ";correlated=" +
                        _summonRules.Count + ".");
                _evidence.Summon = string.Join("|", _summons.Select(Identity)
                    .ToArray());
                _evidence.ContextAbilityReferenceExact = _summons.All(value =>
                {
                    RuleSummonUnit rule = _summonRules[value];
                    return rule.Context != null &&
                        rule.Context.SourceAbilityContext != null &&
                        ReferenceEquals(rule.Context.SourceAbilityContext
                            .Ability, _castAbility);
                });
                _evidence.ExpectedLifecycleSeconds =
                    (_summonRule.Duration.Seconds +
                        _summonRule.BonusDuration.Seconds).TotalSeconds;
                var dispositions = new List<string>();
                var policies = new List<string>();
                var duplicateDispositions = new List<string>();
                bool duplicateNoOp = true;
                _evidence.SummonInCombatAfterSpawn = true;
                _evidence.SummonInTurnOrderAfterSpawn = true;
                _evidence.SummonInCombatAtFirstTurn = true;
                _evidence.SummonInTurnOrderAtFirstTurn = true;
                _evidence.AppearBuffAfterSpawn = true;
                _evidence.LifecycleSecondsAfterSpawn = double.NaN;
                foreach (UnitEntityData summon in _summons)
                {
                    RuleSummonUnit rule = _summonRules[summon];
                    double expected = (rule.Duration.Seconds +
                        rule.BonusDuration.Seconds).TotalSeconds;
                    if (Math.Abs(expected -
                            _evidence.ExpectedLifecycleSeconds) > 0.001d)
                        throw new InvalidOperationException(
                            "Multi-summon rule durations diverged.");
                    SummonSameTurnActivationRequest activationRequest;
                    SummonSameTurnActivationDecision activationDecision =
                        SummonSameTurnActivationRuntime.Inspect(rule,
                            out activationRequest);
                    dispositions.Add(activationDecision.Disposition.ToString());
                    string policy = DescribeActivationPolicy(
                        activationRequest, activationDecision);
                    policies.Add(Identity(summon) + "=" + policy);
                    _diagnostics.Add("activation-policy=unit=" +
                        Identity(summon) + ";" + policy + ";abilityType=" +
                        _castAbility.Blueprint.Type + ";descriptor=" +
                        _castAbility.Blueprint.SpellDescriptor + ";spellbook=" +
                        (_castAbility.Spellbook != null));
                    bool duplicateAppearBefore = summon.Descriptor.Buffs.GetBuff(
                        BlueprintRoot.Instance.SystemMechanics
                            .SummonedUnitAppearBuff) != null;
                    Buff duplicateLifecycle = summon.Descriptor.Buffs.GetBuff(
                        BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
                    double duplicateDurationBefore = duplicateLifecycle == null ?
                        -1d : duplicateLifecycle.TimeLeft.TotalSeconds;
                    SummonSameTurnActivationDecision duplicateDecision =
                        SummonSameTurnActivationRuntime.TryRepair(rule);
                    bool duplicateAppearAfter = summon.Descriptor.Buffs.GetBuff(
                        BlueprintRoot.Instance.SystemMechanics
                            .SummonedUnitAppearBuff) != null;
                    duplicateLifecycle = summon.Descriptor.Buffs.GetBuff(
                        BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
                    double duplicateDurationAfter = duplicateLifecycle == null ?
                        -1d : duplicateLifecycle.TimeLeft.TotalSeconds;
                    duplicateDispositions.Add(duplicateDecision.Disposition
                        .ToString());
                    duplicateNoOp = duplicateNoOp &&
                        !duplicateDecision.ShouldRepair &&
                        duplicateAppearBefore == duplicateAppearAfter &&
                        Math.Abs(duplicateDurationBefore -
                            duplicateDurationAfter) <= 0.001d;
                    CaptureSummonState("post-spawn", summon);
                }
                _evidence.ActivationDisposition = string.Join(",",
                    dispositions.ToArray());
                _evidence.ActivationPolicy = string.Join("|",
                    policies.ToArray());
                _evidence.DuplicateDisposition = string.Join(",",
                    duplicateDispositions.ToArray());
                _evidence.DuplicateNoOp = duplicateNoOp;
                _evidence.ExactSummonKind = _kind != ScenarioKind.Multiple ||
                    _summons.All(value => value.Blueprint != null &&
                        value.Blueprint.name == "KMG_Summoning_Unit_Eagle");
                _evidence.AccelerationCorrelationTrace =
                    SummonAcceleratedInvocationRuntime.DiagnosticTrace;
                _diagnostics.Add("acceleration-correlation=" +
                    _evidence.AccelerationCorrelationTrace);
                CaptureAcadamaeEvidence();
                CaptureCasterActions(false);
                _evidence.CurrentActorAfter = controller.CurrentTurn == null ?
                    (_kind == ScenarioKind.RtwpControl ? "<rtwp>" : "<none>") :
                    Identity(controller.CurrentTurn.Unit);
                _evidence.CasterStillOwnsTurn = _kind !=
                    ScenarioKind.RtwpControl && controller.CurrentTurn != null &&
                    ReferenceEquals(controller.CurrentTurn.Unit, _caster);
                _diagnostics.Add(DescribeEvidence());
                if (_kind != ScenarioKind.RtwpControl &&
                    !_evidence.CasterStillOwnsTurn)
                    throw new InvalidOperationException(
                        "The cast changed current-turn ownership before the " +
                        "caster intentionally ended the turn.");
                Game.Instance.IsPaused = false;
                if (_kind != ScenarioKind.RtwpControl)
                {
                    controller.CurrentTurn.ForceToEnd(true);
                    _lastForcedTurn = controller.CurrentTurn;
                    _forcedTurns++;
                }
                _failureStage = _kind == ScenarioKind.RtwpControl ?
                    "observe-rtwp-native-activation" :
                    "observe-first-summon-turn";
                _stage = 4;
            }

            private void PollSummonOpportunity()
            {
                CombatController controller =
                    Game.Instance.TurnBasedCombatController;
                if (_kind == ScenarioKind.RtwpControl)
                {
                    Game.Instance.EntityCreator.Tick();
                    bool live = _summons.All(value => value != null &&
                        !value.Destroyed && value.IsInState &&
                        value.CombatState.IsInCombat);
                    bool active = _summons.All(value =>
                        Count(_sameCommandsByUnit, value) > 0 ||
                        Count(_sameAttacksByUnit, value) > 0);
                    bool appearanceCleared = _summons.All(value =>
                        value.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance
                            .SystemMechanics.SummonedUnitAppearBuff) == null);
                    if (!live || !active || !appearanceCleared) return;
                    _evidence.RtwpNativeActive = true;
                    _evidence.RtwpNativeAppearanceCleared = true;
                    _evidence.RtwpCurrentTurnAbsent =
                        controller.CurrentTurn == null;
                    _evidence.RtwpTurnOrderAbsent = _summons.All(value =>
                        !controller.SortedUnits.Contains(value));
                    FinalizeOpportunityObservations();
                    return;
                }
                TurnController turn = controller.CurrentTurn;
                if (turn == null) return;
                RecordTurn(turn);
                bool allSameLawful = AllUnitsHave(_sameLawfulByUnit, 1);
                bool allNextLawful = AllUnitsHave(_nextLawfulByUnit, 1);
                bool allFollowingLawful = AllUnitsHave(
                    _followingLawfulByUnit, 1);
                bool summonOwnsTurn = _summons.Contains(turn.Unit);
                if (_kind == ScenarioKind.Acadamae && allSameLawful &&
                    allNextLawful)
                {
                    Game.Instance.EntityCreator.Tick();
                    Game.Instance.EntityDestroyer.Tick();
                    _evidence.ExpiredAtExpectedBoundary =
                        controller.RoundNumber >= _castRound + 2 &&
                        _summons.All(IsExpired);
                    if (_evidence.ExpiredAtExpectedBoundary)
                    {
                        FinalizeOpportunityObservations();
                        return;
                    }
                    if (controller.RoundNumber > _castRound + 2)
                    {
                        FinalizeOpportunityObservations();
                        return;
                    }
                }
                else if (_kind == ScenarioKind.NativeControl)
                {
                    if (allFollowingLawful && !summonOwnsTurn)
                    {
                        FinalizeOpportunityObservations();
                        return;
                    }
                    if (controller.RoundNumber > _castRound + 2)
                    {
                        FinalizeOpportunityObservations();
                        return;
                    }
                }
                else if (allNextLawful && !summonOwnsTurn)
                {
                    FinalizeOpportunityObservations();
                    return;
                }
                else if (controller.RoundNumber > _castRound + 1 &&
                    _kind != ScenarioKind.Acadamae)
                {
                    FinalizeOpportunityObservations();
                    return;
                }
                if (!summonOwnsTurn)
                    ForceTurnOnce(turn);
            }

            internal void ObserveTurnPrepared(TurnController turn)
            {
                if (turn == null || turn.Unit == null ||
                    !_summons.Contains(turn.Unit) ||
                    !_preparedSummonTurns.Add(turn)) return;
                UnitEntityData summon = turn.Unit;
                int round = Game.Instance.TurnBasedCombatController
                    .RoundNumber;
                Buff appear = summon.Descriptor.Buffs.GetBuff(BlueprintRoot
                    .Instance.SystemMechanics.SummonedUnitAppearBuff);
                bool lawful = summon.CombatState.CanActInCombat &&
                    appear == null && !turn.IsEnding && !turn.IsActed();
                string observation = "round=" + round + ";status=" +
                    turn.Status + ";acted=" + turn.IsActed() +
                    ";canAct=" + summon.CombatState.CanActInCombat +
                    ";able=" + summon.IsAbleToAct() + ";appear=" +
                    (appear != null) + ";standard=" +
                    summon.HasStandardAction() + ";move=" +
                    summon.HasMoveAction() + ";swift=" +
                    summon.HasSwiftAction() + ";lawful=" + lawful;
                _evidence.SummonTurnObservations.Add(observation);
                _diagnostics.Add("summon-turn-prepared=" + observation);
                bool firstForUnit = Count(_sameTurnsByUnit, summon) == 0 &&
                    Count(_nextTurnsByUnit, summon) == 0;
                if (_evidence.FirstSummonTurnRound == 0)
                    _evidence.FirstSummonTurnRound = round;
                if (firstForUnit)
                {
                    _evidence.SummonInCombatAtFirstTurn =
                        _evidence.SummonInCombatAtFirstTurn &&
                        summon.CombatState.IsInCombat;
                    _evidence.SummonInTurnOrderAtFirstTurn =
                        _evidence.SummonInTurnOrderAtFirstTurn && Game.Instance
                            .TurnBasedCombatController.SortedUnits.Contains(summon);
                }
                if (lawful && _firstLawfulSummonTurnRound == 0)
                    _firstLawfulSummonTurnRound = round;
                if (round == _castRound)
                {
                    _sameRoundSummonTurns++;
                    Increment(_sameTurnsByUnit, summon);
                    if (lawful)
                    {
                        _sameRoundLawfulSummonTurns++;
                        Increment(_sameLawfulByUnit, summon);
                    }
                }
                else if (round == _castRound + 1)
                {
                    _nextRoundSummonTurns++;
                    Increment(_nextTurnsByUnit, summon);
                    if (lawful)
                    {
                        _nextRoundLawfulSummonTurns++;
                        Increment(_nextLawfulByUnit, summon);
                    }
                }
                else if (_kind == ScenarioKind.NativeControl &&
                    round == _castRound + 2)
                {
                    _followingRoundSummonTurns++;
                    Increment(_followingTurnsByUnit, summon);
                    if (lawful)
                    {
                        _followingRoundLawfulSummonTurns++;
                        Increment(_followingLawfulByUnit, summon);
                    }
                }
            }

            internal void ObserveSummonCommand(UnitCommand command)
            {
                if (command == null || command.Executor == null ||
                    !_summons.Contains(command.Executor)) return;
                CombatController controller = Game.Instance == null ? null :
                    Game.Instance.TurnBasedCombatController;
                bool rtwp = _kind == ScenarioKind.RtwpControl;
                if (controller == null || !rtwp &&
                    (controller.CurrentTurn == null ||
                     !ReferenceEquals(controller.CurrentTurn.Unit,
                        command.Executor))) return;
                int round = rtwp ? 0 : controller.RoundNumber;
                if (_firstSummonCommandRound == 0)
                    _firstSummonCommandRound = rtwp ? -1 : round;
                if (round == _castRound)
                {
                    _sameRoundSummonCommands++;
                    Increment(_sameCommandsByUnit, command.Executor);
                }
                else if (round == _castRound + 1)
                {
                    _nextRoundSummonCommands++;
                    Increment(_nextCommandsByUnit, command.Executor);
                }
                string observation = "round=" + round + ";type=" +
                    command.Type + ";command=" + command.GetType().FullName;
                _evidence.SummonCommands.Add(observation);
                _diagnostics.Add("summon-command=" + observation);
            }

            internal void ObserveSummonAttack(RuleAttackWithWeapon attack)
            {
                if (attack == null || attack.Initiator == null ||
                    !_summons.Contains(attack.Initiator)) return;
                CombatController controller = Game.Instance == null ? null :
                    Game.Instance.TurnBasedCombatController;
                bool rtwp = _kind == ScenarioKind.RtwpControl;
                if (controller == null || !rtwp &&
                    (controller.CurrentTurn == null ||
                     !ReferenceEquals(controller.CurrentTurn.Unit,
                        attack.Initiator))) return;
                int round = rtwp ? 0 : controller.RoundNumber;
                if (_firstSummonAttackRound == 0)
                    _firstSummonAttackRound = rtwp ? -1 : round;
                if (round == _castRound)
                {
                    _sameRoundSummonAttacks++;
                    Increment(_sameAttacksByUnit, attack.Initiator);
                }
                else if (round == _castRound + 1)
                    _nextRoundSummonAttacks++;
                _diagnostics.Add("summon-attack=round=" + round +
                    ";target=" + Identity(attack.Target) +
                    ";weapon=" + (attack.Weapon == null ? "<none>" :
                        attack.Weapon.Blueprint.name));
            }

            private void ForceTurnOnce(TurnController turn)
            {
                if (ReferenceEquals(turn, _lastForcedTurn)) return;
                if (_forcedTurns >= 48)
                    throw new InvalidOperationException(
                        "The native turn loop did not reach the expected " +
                        "summon opportunity within 48 turns.");
                Game.Instance.IsPaused = false;
                turn.ForceToEnd(true);
                _lastForcedTurn = turn;
                _forcedTurns++;
            }

            private void RecordTurn(TurnController turn)
            {
                if (ReferenceEquals(turn, _lastTurn)) return;
                _lastTurn = turn;
                string value = "round=" + Game.Instance
                    .TurnBasedCombatController.RoundNumber + ";unit=" +
                    Identity(turn == null ? null : turn.Unit) + ";status=" +
                    (turn == null ? "<none>" : turn.Status.ToString()) +
                    ";acted=" + (turn != null && turn.IsActed());
                _turns.Add(value);
                _diagnostics.Add("turn=" + value);
            }

            private void CaptureCasterActions(bool before)
            {
                var cooldown = _caster.CombatState.Cooldown;
                if (before)
                {
                    _evidence.CasterSwiftBefore = cooldown.SwiftAction;
                    _evidence.CasterStandardBefore = cooldown.StandardAction;
                    _evidence.CasterMoveBefore = cooldown.MoveAction;
                    _evidence.CasterHasSwiftBefore = _caster.HasSwiftAction();
                    _evidence.CasterHasStandardBefore =
                        _caster.HasStandardAction();
                    _evidence.CasterHasMoveBefore = _caster.HasMoveAction();
                }
                else
                {
                    _evidence.CasterSwiftAfter = cooldown.SwiftAction;
                    _evidence.CasterStandardAfter = cooldown.StandardAction;
                    _evidence.CasterMoveAfter = cooldown.MoveAction;
                    _evidence.CasterHasSwiftAfter = _caster.HasSwiftAction();
                    _evidence.CasterHasStandardAfter =
                        _caster.HasStandardAction();
                    _evidence.CasterHasMoveAfter = _caster.HasMoveAction();
                }
            }

            private void CaptureSummonState(string stage,
                UnitEntityData summon)
            {
                Buff appear = summon.Descriptor.Buffs.GetBuff(BlueprintRoot
                    .Instance.SystemMechanics.SummonedUnitAppearBuff);
                Buff lifecycle = summon.Descriptor.Buffs.GetBuff(BlueprintRoot
                    .Instance.SystemMechanics.SummonedUnitBuff);
                CombatController controller = Game.Instance
                    .TurnBasedCombatController;
                string row = "stage=" + stage + ";unit=" + Identity(summon) +
                    ";live=" + (!summon.Destroyed && summon.IsInState) +
                    ";inCombat=" + summon.CombatState.IsInCombat +
                    ";initiative=" + summon.CombatState.Initiative +
                    ";initiativeCooldown=" + summon.CombatState.Cooldown
                        .Initiative.ToString("R", CultureInfo.InvariantCulture) +
                    ";canAct=" + summon.CombatState.CanActInCombat +
                    ";appear=" + (appear != null) + ";appearTime=" +
                    (appear == null ? "<none>" : appear.TimeLeft.ToString()) +
                    ";lifecycle=" + (lifecycle == null ? "<none>" :
                        lifecycle.TimeLeft.ToString()) + ";turnOrder=" +
                    controller.SortedUnits.Contains(summon) + ";round=" +
                    controller.RoundNumber + ";current=" +
                    (controller.CurrentTurn == null ? "<none>" :
                        Identity(controller.CurrentTurn.Unit));
                _evidence.SummonStates.Add(row);
                _diagnostics.Add("summon-state=" + row);
                if (stage == "post-spawn")
                {
                    _evidence.SummonInCombatAfterSpawn =
                        _evidence.SummonInCombatAfterSpawn &&
                        summon.CombatState.IsInCombat;
                    _evidence.SummonInTurnOrderAfterSpawn =
                        _evidence.SummonInTurnOrderAfterSpawn &&
                        controller.SortedUnits.Contains(summon);
                    _evidence.AppearBuffAfterSpawn =
                        _evidence.AppearBuffAfterSpawn && appear != null;
                    double seconds = lifecycle == null ? -1d :
                        lifecycle.TimeLeft.TotalSeconds;
                    _evidence.LifecycleSecondsBySummon.Add(Identity(summon) +
                        "=" + seconds.ToString("R",
                            CultureInfo.InvariantCulture));
                    _evidence.LifecycleSecondsAfterSpawn = double.IsNaN(
                        _evidence.LifecycleSecondsAfterSpawn) ? seconds :
                        Math.Min(_evidence.LifecycleSecondsAfterSpawn, seconds);
                }
            }

            private void CaptureAcadamaeEvidence()
            {
                _evidence.SlotAvailableAfter = _castSlot != null &&
                    _castSlot.Available;
                if (_kind != ScenarioKind.Acadamae) return;
                _evidence.AcadamaeModeOn = _acadamaeMode != null &&
                    _acadamaeMode.IsOn;
                _evidence.AcadamaeCompletedDelta =
                    AcadamaeCastingRuntime.CompletedCount -
                    _acadamaeCompletedBefore;
                _evidence.AcadamaePublicationDelta =
                    AcadamaeCastingRuntime.ResolutionPublicationAttemptCount -
                    _acadamaePublicationBefore;
                _evidence.AcadamaeSavePassed =
                    AcadamaeCastingRuntime.LastSavePassed;
                _evidence.AcadamaeNaturalRoll =
                    AcadamaeCastingRuntime.LastNaturalRoll;
                _evidence.AcadamaeDifficultyClass =
                    AcadamaeCastingRuntime.LastDifficultyClass;
                _evidence.AcadamaeFatigueDisposition =
                    AcadamaeCastingRuntime.LastFatigueDisposition;
                _evidence.CasterFatigued = _caster.Descriptor.State
                    .HasCondition(UnitCondition.Fatigued);
                _evidence.CasterExhausted = _caster.Descriptor.State
                    .HasCondition(UnitCondition.Exhausted);
            }

            private void FinalizeOpportunityObservations()
            {
                _evidence.FirstLawfulSummonTurnRound =
                    _firstLawfulSummonTurnRound;
                _evidence.SameRoundSummonTurns = _sameRoundSummonTurns;
                _evidence.SameRoundLawfulSummonTurns =
                    _sameRoundLawfulSummonTurns;
                _evidence.NextRoundSummonTurns = _nextRoundSummonTurns;
                _evidence.NextRoundLawfulSummonTurns =
                    _nextRoundLawfulSummonTurns;
                _evidence.FollowingRoundSummonTurns =
                    _followingRoundSummonTurns;
                _evidence.FollowingRoundLawfulSummonTurns =
                    _followingRoundLawfulSummonTurns;
                _evidence.FirstSummonCommandRound =
                    _firstSummonCommandRound;
                _evidence.SameRoundSummonCommands =
                    _sameRoundSummonCommands;
                _evidence.NextRoundSummonCommands =
                    _nextRoundSummonCommands;
                _evidence.FirstSummonAttackRound =
                    _firstSummonAttackRound;
                _evidence.SameRoundSummonAttacks =
                    _sameRoundSummonAttacks;
                _evidence.NextRoundSummonAttacks =
                    _nextRoundSummonAttacks;
                _evidence.Turns = _turns.ToArray();
                foreach (UnitEntityData summon in _summons)
                {
                    _evidence.UnitOpportunities.Add(Identity(summon) +
                        ";same=" + Count(_sameTurnsByUnit, summon) + "/" +
                        Count(_sameLawfulByUnit, summon) + ";next=" +
                        Count(_nextTurnsByUnit, summon) + "/" +
                        Count(_nextLawfulByUnit, summon) + ";following=" +
                        Count(_followingTurnsByUnit, summon) + "/" +
                        Count(_followingLawfulByUnit, summon) +
                        ";commands=" +
                        Count(_sameCommandsByUnit, summon) + "/" +
                        Count(_nextCommandsByUnit, summon) + ";attacks=" +
                        Count(_sameAttacksByUnit, summon));
                    if (!IsExpired(summon))
                        CaptureSummonState("observation-end", summon);
                    else
                    {
                        string row = "stage=observation-end;unit=" +
                            Identity(summon) + ";expired=true;destroyed=" +
                            summon.Destroyed + ";inState=" + summon.IsInState;
                        _evidence.SummonStates.Add(row);
                        _diagnostics.Add("summon-state=" + row);
                    }
                }
                AddAssertions();
                _stage = 5;
            }

            private bool AllUnitsHave(
                IDictionary<UnitEntityData, int> values, int expected)
            {
                return _summons.Count > 0 && _summons.All(value =>
                    Count(values, value) == expected);
            }

            private static int Count(
                IDictionary<UnitEntityData, int> values,
                UnitEntityData unit)
            {
                int result;
                return values.TryGetValue(unit, out result) ? result : 0;
            }

            private static void Increment(
                IDictionary<UnitEntityData, int> values,
                UnitEntityData unit)
            {
                values[unit] = Count(values, unit) + 1;
            }

            private static bool IsExpired(UnitEntityData unit)
            {
                return unit == null || unit.Destroyed || !unit.IsInState ||
                    Game.Instance == null || Game.Instance.State == null ||
                    !Game.Instance.State.Units.All.Contains(unit);
            }

            private void AddAssertions()
            {
                string path = "action=" + _evidence.ActionType +
                    ";runtime=" + _evidence.RuntimeActionType +
                    ";fullRound=" + _evidence.RequireFullRoundAction +
                    ";blueprintFullRound=" + _evidence.BlueprintFullRound +
                    ";metamagic=" + _evidence.Metamagic + ";ruleCast=" +
                    _evidence.RuleCastCount + ";spawn=" +
                    _evidence.SpawnActionCount + ";summon=" +
                    _evidence.RuleSummonCount + ";slot=" +
                    _evidence.SlotAvailableBefore + "->" +
                    _evidence.SlotAvailableAfter;
                bool realPath = _evidence.CommandResult == "Success" &&
                    _evidence.RuleCastCount == 1 &&
                    _evidence.SpawnActionCount == 1 &&
                    _evidence.RuleSummonCount >=
                        _evidence.ExpectedSummonMinimum &&
                    _evidence.RuleSummonCount <=
                        _evidence.ExpectedSummonMaximum &&
                    _evidence.ContextAbilityReferenceExact &&
                    _evidence.SlotAvailableBefore &&
                    !_evidence.SlotAvailableAfter;
                Add(_kind == ScenarioKind.Quickened ?
                        "quickened-real-player-path" :
                        "summon-real-player-path",
                    "actual prepared spellbook invocation through UnitUseAbility, RuleCastSpell, ContextActionSpawnMonster, and RuleSummonUnit",
                    path, realPath,
                    "exact installed runtime objects and reference identity");

                if (_kind == ScenarioKind.RtwpControl)
                {
                    Add("rtwp-native-summon-activation",
                        "RTwP remains native: its appearance completes, no TB state is created, and native summon AI activates",
                        "turnBased=" + _evidence.TurnBasedAtCast +
                            ";decision=" + _evidence.ActivationDisposition +
                            ";appear=" + _evidence.AppearBuffAfterSpawn +
                            "->" +
                            _evidence.RtwpNativeAppearanceCleared +
                            ";currentTurnAbsent=" +
                            _evidence.RtwpCurrentTurnAbsent +
                            ";turnOrderAbsent=" +
                            _evidence.RtwpTurnOrderAbsent +
                            ";active=" + _evidence.RtwpNativeActive +
                            ";commands=" + _sameRoundSummonCommands,
                        !_evidence.TurnBasedAtCast &&
                        AllDispositions(_evidence.ActivationDisposition,
                            SummonSameTurnActivationDisposition
                                .RealTimeWithPause) &&
                        _evidence.AppearBuffAfterSpawn &&
                        _evidence.RtwpNativeAppearanceCleared &&
                        _evidence.RtwpCurrentTurnAbsent &&
                        _evidence.RtwpTurnOrderAbsent &&
                        _evidence.RtwpNativeActive &&
                        _sameRoundSummonCommands > 0 && _forcedTurns == 0,
                        "native combat state and UnitCommands.Run; no artificial CurrentTurn");
                    Add("rtwp-duration-native",
                        "RTwP lifecycle remains exact native duration",
                        DurationObserved(), AcceleratedDurationExact(),
                        "canonical SummonedUnitBuff");
                    Add("rtwp-duplicate-observation",
                        "repeat observation changes no canonical buff",
                        _evidence.DuplicateDisposition + ";noOp=" +
                            _evidence.DuplicateNoOp,
                        AllDispositions(_evidence.DuplicateDisposition,
                            SummonSameTurnActivationDisposition
                                .RealTimeWithPause) &&
                        _evidence.DuplicateNoOp,
                        "production RTwP fail-closed policy");
                }
                else if (_kind == ScenarioKind.NativeControl)
                {
                    Add("ordinary-native-and-acadamae-off",
                        "mode OFF remains native Full-Round and receives no Acadamae consequence",
                        path + ";mode=" + _evidence.AcadamaeModeOn +
                            ";acadamaeDelta=" +
                            _evidence.AcadamaeCompletedDelta,
                        _evidence.RequireFullRoundAction &&
                        _evidence.BlueprintFullRound &&
                        !_evidence.AcadamaeModeOn &&
                        _evidence.AcadamaeCompletedDelta == 0 &&
                        AllDispositions(_evidence.ActivationDisposition,
                            SummonSameTurnActivationDisposition
                                .NativeFullRoundInvocation),
                        "actual Acadamae feat/toggle OFF plus native AbilityData");
                    Add("ordinary-native-scheduling",
                        "native appearance lock blocks any cast-round entry and the next-round entry; the first lawful opportunity is castRound+2",
                        OpportunitiesObserved(),
                        _summons.All(value =>
                            Count(_sameTurnsByUnit, value) <= 1) &&
                        AllUnitsHave(_sameLawfulByUnit, 0) &&
                        AllUnitsHave(_nextTurnsByUnit, 1) &&
                        AllUnitsHave(_nextLawfulByUnit, 0) &&
                        AllUnitsHave(_followingTurnsByUnit, 1) &&
                        AllUnitsHave(_followingLawfulByUnit, 1) &&
                        _evidence.FirstLawfulSummonTurnRound ==
                            _castRound + 2,
                        "unmodified native appearance lock and TurnController");
                    Add("ordinary-native-duration",
                        "native Full-Round grace remains exactly six seconds",
                        DurationObserved(),
                        _evidence.AppearBuffAfterSpawn &&
                        Math.Abs(_evidence.LifecycleSecondsAfterSpawn -
                            (_evidence.ExpectedLifecycleSeconds +
                             SummonSameTurnActivationPolicy
                                .NativeGraceSeconds)) <= 1d,
                        "canonical native appearance and lifecycle buffs");
                    Add("cancelled-summon-no-activation",
                        "real prepared command rejected before range creates no rule, unit, or slot spend",
                        _evidence.CancelledControlDetails,
                        _evidence.CancelledControlPassed,
                        "actual AbilityData and UnitUseAbility pre-run cancellation boundary");
                    Add("non-summon-spawn-unaffected",
                        "ordinary EntityCreator unit receives no summon lifecycle or activation handling",
                        _evidence.NonSummonControlDetails,
                        _evidence.NonSummonControlPassed,
                        "live non-summon BlueprintUnit control");
                }
                else
                {
                    UnitCommand.CommandType expectedAction =
                        _kind == ScenarioKind.Acadamae ?
                            UnitCommand.CommandType.Standard :
                            UnitCommand.CommandType.Swift;
                    Add(_kind == ScenarioKind.Acadamae ?
                            "acadamae-caster-actions-preserved" :
                            "quickened-caster-actions-preserved",
                        expectedAction + " spent; all lawful remaining caster actions and current-turn ownership preserved",
                        CasterActionsObserved(),
                        _evidence.CasterStillOwnsTurn &&
                        (_kind == ScenarioKind.Acadamae ?
                            _evidence.CasterStandardAfter >= 5.999f &&
                            _evidence.CasterMoveAfter >= 2.999f &&
                            _evidence.CasterMoveAfter <= 3.001f &&
                            !_evidence.CasterHasStandardAfter &&
                            _evidence.CasterHasSwiftAfter &&
                            !_evidence.CasterHasMoveAfter :
                            _evidence.CasterSwiftAfter >= 5.999f &&
                            !_evidence.CasterHasSwiftAfter &&
                            _evidence.CasterHasStandardAfter &&
                            _evidence.CasterHasMoveAfter) &&
                        _evidence.ActionType == expectedAction.ToString() &&
                        _evidence.RuntimeActionType ==
                            expectedAction.ToString() &&
                        !_evidence.RequireFullRoundAction &&
                        _evidence.BlueprintFullRound,
                        "native UnitCombatState cooldowns and CurrentTurn");
                    Add("accelerated-summon-enrollment",
                        "every genuine summon enters combat and turn order before its first scheduled turn",
                        "postSpawn=" + _evidence.SummonInCombatAfterSpawn +
                            "/" + _evidence.SummonInTurnOrderAfterSpawn +
                            ";firstTurn=" +
                            _evidence.SummonInCombatAtFirstTurn + "/" +
                            _evidence.SummonInTurnOrderAtFirstTurn,
                        _evidence.SummonInCombatAtFirstTurn &&
                        _evidence.SummonInTurnOrderAtFirstTurn,
                        "RuleSummonUnit, UnitCombatState, and SortedUnits");
                    Add("accelerated-summon-current-round-opportunity",
                        "every spawned unit receives exactly one lawful cast-round opportunity",
                        OpportunitiesObserved(),
                        AllUnitsHave(_sameTurnsByUnit, 1) &&
                        AllUnitsHave(_sameLawfulByUnit, 1) &&
                        _firstLawfulSummonTurnRound == _castRound,
                        "spawned-unit identity plus exact round-correlated turns");
                    Add("accelerated-summon-next-round-normal",
                        "every surviving spawned unit receives exactly one normal following-round opportunity",
                        OpportunitiesObserved(),
                        AllUnitsHave(_nextTurnsByUnit, 1) &&
                        AllUnitsHave(_nextLawfulByUnit, 1),
                        "same unit identities in castRound+1");
                    Add("accelerated-summon-native-ai",
                        "every summon issues native commands in current and following lawful turns",
                        string.Join(" | ", _evidence.UnitOpportunities
                            .ToArray()),
                        AllUnitsAtLeast(_sameCommandsByUnit, 1) &&
                        AllUnitsAtLeast(_nextCommandsByUnit, 1),
                        "UnitCommands.Run correlated to exact summon and CurrentTurn");
                    if (_kind == ScenarioKind.Quickened)
                        Add("accelerated-summon-single-action",
                            "tier-one dog resolves exactly one weapon rule in its one cast-round opportunity",
                            "first=" + _firstSummonAttackRound +
                                ";current=" + _sameRoundSummonAttacks,
                            _firstSummonAttackRound == _castRound &&
                            _sameRoundSummonAttacks == 1,
                            "RuleAttackWithWeapon correlated to summon CurrentTurn");
                    Add("accelerated-summon-duration",
                        "lifecycle equals RuleSummonUnit duration without Full-Round grace",
                        DurationObserved(), AcceleratedDurationExact(),
                        "canonical SummonedUnitBuff and exact rule durations");
                    Add("accelerated-summon-duplicate-callback",
                        "every repeated exact-unit callback is AlreadyEligible and changes no canonical buff",
                        _evidence.DuplicateDisposition + ";noOp=" +
                            _evidence.DuplicateNoOp,
                        AllDispositions(_evidence.DuplicateDisposition,
                            SummonSameTurnActivationDisposition
                                .AlreadyEligible) &&
                        _evidence.DuplicateNoOp,
                        "stateless per-unit canonical-state normalization");
                    if (_kind == ScenarioKind.Acadamae)
                    {
                        Add("acadamae-standard-save-and-slot",
                            "ON cast is Standard, spends one slot, resolves exactly one successful Fortitude save, and applies no fatigue",
                            "mode=" + _evidence.AcadamaeModeOn +
                                ";completed=" +
                                _evidence.AcadamaeCompletedDelta +
                                ";published=" +
                                _evidence.AcadamaePublicationDelta +
                                ";save=" + _evidence.AcadamaeSavePassed +
                                ";roll=" + _evidence.AcadamaeNaturalRoll +
                                ";dc=" + _evidence.AcadamaeDifficultyClass +
                                ";fatigue=" +
                                _evidence.AcadamaeFatigueDisposition,
                            _evidence.AcadamaeModeOn &&
                            _evidence.AcadamaeCompletedDelta == 1 &&
                            _evidence.AcadamaePublicationDelta == 1 &&
                            _evidence.AcadamaeSavePassed &&
                            _evidence.AcadamaeNaturalRoll == 20 &&
                            !_evidence.CasterFatigued &&
                            !_evidence.CasterExhausted,
                            "existing Acadamae command/rule correlation and native save");
                        Add("accelerated-summon-expiration",
                            "two-round caster-level duration grants current and next opportunities then expires at castRound+2",
                            "duration=" + DurationObserved() +
                                ";expired=" +
                                _evidence.ExpiredAtExpectedBoundary,
                            Math.Abs(_evidence.ExpectedLifecycleSeconds - 12d)
                                <= 0.001d &&
                            _evidence.ExpiredAtExpectedBoundary,
                            "canonical SummonedUnitBuff expiration and entity removal");
                    }
                    if (_kind == ScenarioKind.Multiple)
                        Add("multiple-kmg-summon-opportunities",
                            "actual KMG Eagle 1d4+1 creates 2..5 exact units and every unit receives one opportunity exactly once",
                            "count=" + _evidence.RuleSummonCount +
                                ";exact=" + _evidence.ExactSummonKind +
                                ";units=" + string.Join(" | ",
                                    _evidence.UnitOpportunities.ToArray()),
                            _evidence.RuleSummonCount >= 2 &&
                            _evidence.RuleSummonCount <= 5 &&
                            _evidence.ExactSummonKind &&
                            AllUnitsHave(_sameTurnsByUnit, 1) &&
                            AllUnitsHave(_sameLawfulByUnit, 1) &&
                            AllUnitsHave(_nextTurnsByUnit, 1) &&
                            AllUnitsHave(_nextLawfulByUnit, 1),
                            "KMG logical root and per-spawn RuleSummonUnit identity");
                }
                Add("runtime-cleanup-pending", "evaluated during finally",
                    "pending", true,
                    "request-local unit and game-state snapshots");
            }

            private bool AllUnitsAtLeast(
                IDictionary<UnitEntityData, int> values, int minimum)
            {
                return _summons.Count > 0 && _summons.All(value =>
                    Count(values, value) >= minimum);
            }

            private static bool AllDispositions(string value,
                SummonSameTurnActivationDisposition expected)
            {
                if (string.IsNullOrWhiteSpace(value)) return false;
                string expectedValue = expected.ToString();
                string[] values = value.Split(',');
                return values.Length > 0 && values.All(candidate =>
                    string.Equals(candidate, expectedValue,
                        StringComparison.Ordinal));
            }

            private bool AcceleratedDurationExact()
            {
                return _evidence.LifecycleSecondsBySummon.Count ==
                    _summons.Count && Math.Abs(
                        _evidence.LifecycleSecondsAfterSpawn -
                        _evidence.ExpectedLifecycleSeconds) <= 1d;
            }

            private string DurationObserved()
            {
                return "expected=" + _evidence.ExpectedLifecycleSeconds
                    .ToString("0.###", CultureInfo.InvariantCulture) +
                    ";minimumRemaining=" +
                    _evidence.LifecycleSecondsAfterSpawn.ToString("0.###",
                        CultureInfo.InvariantCulture) + ";units=" +
                    string.Join("|", _evidence.LifecycleSecondsBySummon
                        .ToArray());
            }

            private string OpportunitiesObserved()
            {
                return "current=" + _sameRoundSummonTurns + "/" +
                    _sameRoundLawfulSummonTurns + ";next=" +
                    _nextRoundSummonTurns + "/" +
                    _nextRoundLawfulSummonTurns + ";units=" +
                    string.Join(" | ", _evidence.UnitOpportunities.ToArray());
            }

            private string CasterActionsObserved()
            {
                return "swift=" + _evidence.CasterSwiftBefore + "->" +
                    _evidence.CasterSwiftAfter + ";standard=" +
                    _evidence.CasterStandardBefore + "->" +
                    _evidence.CasterStandardAfter + ";move=" +
                    _evidence.CasterMoveBefore + "->" +
                    _evidence.CasterMoveAfter + ";has=" +
                    _evidence.CasterHasSwiftBefore + "/" +
                    _evidence.CasterHasStandardBefore + "/" +
                    _evidence.CasterHasMoveBefore + "->" +
                    _evidence.CasterHasSwiftAfter + "/" +
                    _evidence.CasterHasStandardAfter + "/" +
                    _evidence.CasterHasMoveAfter + ";current=" +
                    _evidence.CasterStillOwnsTurn;
            }

            internal void ObserveRuleCast(RuleCastSpell rule)
            {
                if (rule == null || _caster == null ||
                    !_castCaptureActive ||
                    !ReferenceEquals(rule.Initiator, _caster)) return;
                _evidence.RuleCastCount++;
                _evidence.RuleCastSuccess = rule.Success;
            }

            internal void ObserveSpawnAction(ContextActionSpawnMonster action)
            {
                if (action != null && _caster != null && _castCaptureActive)
                    _evidence.SpawnActionCount++;
            }

            internal void ObserveSummonRule(RuleSummonUnit rule)
            {
                if (rule == null || _caster == null ||
                    !_castCaptureActive ||
                    !ReferenceEquals(rule.Initiator, _caster) ||
                    rule.SummonedUnit == null) return;
                if (_summonRule == null) _summonRule = rule;
                _summonRules[rule.SummonedUnit] = rule;
                if (!_summons.Contains(rule.SummonedUnit))
                    _summons.Add(rule.SummonedUnit);
            }

            internal void ObserveFailure(string boundary, Exception exception)
            {
                _diagnostics.Add("observer-exception=" + boundary + ":" +
                    exception);
            }

            private void Finish()
            {
                if (Complete) return;
                bool cleaned = false;
                try
                {
                    Cleanup();
                    object[] unitsAfter = Game.Instance.State.Units.All
                        .Cast<object>().ToArray();
                    object[] partyAfter = Game.Instance.Player.Party
                        .Cast<object>().ToArray();
                    _evidence.CleanupDetails = "units=" +
                        DescribeReferenceDifference(_unitsBefore, unitsAfter) +
                        ";party=" + DescribeReferenceDifference(_partyBefore,
                            partyAfter) + ";playerCombat=" +
                        Game.Instance.Player.IsInCombat;
                    _diagnostics.Add("cleanup-state=" +
                        _evidence.CleanupDetails);
                    cleaned = SameReferences(_unitsBefore, unitsAfter) &&
                        SameReferences(_partyBefore, partyAfter) &&
                        !Game.Instance.Player.IsInCombat;
                    RuntimeTestAssertion cleanup = _assertions
                        .FirstOrDefault(value => value.Name ==
                            "runtime-cleanup-pending");
                    if (cleanup != null)
                    {
                        cleanup.Name = "summon-activation-cleanup";
                        cleanup.Expected =
                            "unit and party snapshots restored; no combat";
                        cleanup.Observed = "restored=" + cleaned;
                        cleanup.Status = cleaned ? RuntimeTestStatuses.Pass :
                            RuntimeTestStatuses.Fail;
                    }
                    else Add("summon-activation-cleanup",
                        "unit and party snapshots restored; no combat",
                        "restored=" + cleaned, cleaned,
                        "request-local unit and game-state snapshots");
                }
                catch (Exception exception)
                {
                    _diagnostics.Add("cleanup-exception=" + exception);
                    Add("summon-activation-cleanup",
                        "unit and party snapshots restored; no combat",
                        exception.ToString(), false,
                        "request-local unit and game-state snapshots");
                }
                try
                {
                    _evidence.ForcedTurnCount = _forcedTurns;
                    _evidence.Cleaned = cleaned;
                    string path = Path.Combine(_request.EvidenceDirectory,
                        EvidenceFileName);
                    var settings = new JsonSerializerSettings {
                        Formatting = Formatting.Indented,
                        PreserveReferencesHandling =
                            PreserveReferencesHandling.None,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new Newtonsoft.Json.Serialization
                            .DefaultContractResolver()
                    };
                    File.WriteAllText(path, JsonConvert.SerializeObject(
                        _evidence, settings));
                    _files.Add(path);
                    _diagnostics.Add("evidence=" + path);
                }
                catch (Exception exception)
                {
                    _diagnostics.Add("evidence-write-exception=" + exception);
                }
                finally
                {
                    Release(this);
                }
                bool pass = _assertions.Count > 0 && _assertions.All(value =>
                    value.Status == RuntimeTestStatuses.Pass) &&
                    !_diagnostics.Any(value => value.StartsWith("exception=",
                        StringComparison.Ordinal) || value.StartsWith(
                        "cleanup-exception=", StringComparison.Ordinal) ||
                        value.StartsWith("observer-exception=",
                            StringComparison.Ordinal) || value.StartsWith(
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
                    GameVersion = Application.version ?? string.Empty,
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

            private AbilityData PrepareCaseAbility()
            {
                AbilityData result;
                if (_kind == ScenarioKind.Multiple)
                    result = PrepareQuickenedSummon(_spellbook,
                        SummonMonsterThreeGuid, ExpandedEagleMultipleName,
                        3, 7, out _castSlot);
                else if (_kind == ScenarioKind.Acadamae)
                    result = PrepareAcadamaeSummon(_spellbook,
                        out _castSlot);
                else if (_kind == ScenarioKind.NativeControl)
                    result = PreparePreparedSummon(_spellbook,
                        SummonMonsterOneGuid, NativeDogName, 1,
                        out _castSlot);
                else
                    result = PrepareQuickenedSummon(_spellbook,
                        SummonMonsterOneGuid, NativeDogName, 1, 5,
                        out _castSlot);
                _evidence.SpellLevel = _kind == ScenarioKind.Multiple ? 7 :
                    _kind == ScenarioKind.Quickened ||
                    _kind == ScenarioKind.RtwpControl ? 5 : 1;
                _evidence.SlotAvailableBefore = _castSlot != null &&
                    _castSlot.Available;
                return result;
            }

            private void ConfigureAcadamaeMode(bool enabled)
            {
                AcadamaeCastingRuntime.ResetDiagnostics();
                _caster.Descriptor.AddFact(
                    BlueprintBootstrap.AcadamaeGraduate);
                _acadamaeMode = _caster.Descriptor.ActivatableAbilities
                    .Enumerable.Single(value => ReferenceEquals(
                        value.Blueprint,
                        BlueprintBootstrap.AcadamaeGraduateMode.Ability));
                if (enabled)
                {
                    _acadamaeMode.IsOn = true;
                    if (_caster.Descriptor.Buffs.GetBuff(
                            BlueprintBootstrap.AcadamaeGraduateMode.Marker) ==
                        null)
                        throw new InvalidOperationException(
                            "Acadamae ON did not create the exact marker.");
                }
                else
                {
                    _acadamaeMode.IsOn = false;
                    _acadamaeMode.Stop(true);
                    if (_caster.Descriptor.Buffs.GetBuff(
                            BlueprintBootstrap.AcadamaeGraduateMode.Marker) !=
                        null)
                        throw new InvalidOperationException(
                            "Acadamae OFF retained its activation marker.");
                }
                _evidence.AcadamaeModeOn = _acadamaeMode.IsOn;
            }

            private void RunNativeNegativeControls()
            {
                _failureStage = "native-negative-controls";
                int summonRulesBefore = _summonRules.Count;
                bool slotBefore = _castSlot != null && _castSlot.Available;
                var farTarget = new TargetWrapper(_caster.Position +
                    new Vector3(1000f, 0f, 0f));
                var rejected = new UnitUseAbility(_castAbility, farTarget);
                rejected.Init(_caster);
                bool canTarget = _castAbility.CanTarget(farTarget);
                bool canStart = _castAbility.IsAvailable && rejected.CanStart;
                bool withinRange = rejected.IsUnitEnoughClose;
                bool slotAfter = _castSlot != null && _castSlot.Available;
                _evidence.CancelledControlPassed = canTarget && canStart &&
                    !withinRange && slotBefore && slotAfter &&
                    _summonRules.Count == summonRulesBefore &&
                    _summons.Count == 0;
                _evidence.CancelledControlDetails = "canTarget=" + canTarget +
                    ";canStart=" + canStart + ";withinRange=" + withinRange +
                    ";slot=" + slotBefore + "->" + slotAfter +
                    ";summonRules=" + summonRulesBefore + "->" +
                    _summonRules.Count;

                _nonSummonBlueprint = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                _nonSummonBlueprint.name =
                    "KMG_Runtime_SummonSameTurn_NonSummonControl";
                _nonSummonBlueprint.IsCheater = false;
                _nonSummonControlUnit = Game.Instance.EntityCreator.SpawnUnit(
                    _nonSummonBlueprint,
                    _caster.Position + new Vector3(5f, 0f, 0f),
                    Quaternion.identity, _caster.HoldingState);
                Game.Instance.EntityCreator.Tick();
                _nonSummonControlUnit.JoinCombat();
                Buff lifecycle = _nonSummonControlUnit.Descriptor.Buffs.GetBuff(
                    BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
                Buff appearance = _nonSummonControlUnit.Descriptor.Buffs
                    .GetBuff(BlueprintRoot.Instance.SystemMechanics
                        .SummonedUnitAppearBuff);
                _evidence.NonSummonControlPassed =
                    _nonSummonControlUnit.IsInState &&
                    _nonSummonControlUnit.CombatState.IsInCombat &&
                    lifecycle == null && appearance == null &&
                    _summonRules.Count == summonRulesBefore &&
                    _summons.Count == 0;
                _evidence.NonSummonControlDetails = "unit=" +
                    Identity(_nonSummonControlUnit) + ";live=" +
                    _nonSummonControlUnit.IsInState + ";combat=" +
                    _nonSummonControlUnit.CombatState.IsInCombat +
                    ";lifecycle=" + (lifecycle != null) + ";appearance=" +
                    (appearance != null) + ";summonRules=" +
                    _summonRules.Count;
                // Keep this live control registered until the authoritative
                // combat reset. Removing a unit that the turn controller can
                // still enumerate leaves native cleanup with a stale entry.
                _failureStage = "wait-caster-turn";
            }

            private void Cleanup()
            {
                _failureStage = "cleanup";
                _castCaptureActive = false;
                Game.Instance.IsPaused = true;
                if (_levelController != null)
                {
                    MethodInfo cancel = _levelController.GetType().GetMethod(
                        "Cancel", BindingFlags.Instance | BindingFlags.Public |
                            BindingFlags.NonPublic);
                    if (cancel != null) cancel.Invoke(_levelController, null);
                    _levelController = null;
                }
                if (_acadamaeMode != null)
                {
                    if (_acadamaeMode.IsOn) _acadamaeMode.IsOn = false;
                    _acadamaeMode.Stop(true);
                }
                AcadamaeSavingThrowTestControl.Cancel();
                AcadamaeCastingRuntime.ResetDiagnostics();
                if (_fixtureJoinedCombat &&
                    _kind != ScenarioKind.RtwpControl)
                    Game.Instance.TurnBasedCombatController
                        .HandlePartyCombatStateChanged(false);
                foreach (KeyValuePair<UnitEntityData, bool> pair in
                    _combatBefore)
                {
                    if (!pair.Value && pair.Key != null &&
                        pair.Key.CombatState != null &&
                        pair.Key.CombatState.IsInCombat)
                        pair.Key.LeaveCombat();
                }
                foreach (UnitEntityData summon in _summons.Where(value =>
                    value != null).ToArray())
                    DisposeUnit(summon);
                if (_nonSummonControlUnit != null)
                    DisposeUnit(_nonSummonControlUnit);
                if (_enemy != null) DisposeUnit(_enemy);
                if (_caster != null)
                {
                    Game.Instance.Player.Party.Remove(_caster);
                    DisposeUnit(_caster);
                }
                Game.Instance.EntityCreator.Tick();
                Game.Instance.EntityDestroyer.Tick();
                Game.Instance.EntityDestroyer.Tick();
                if (_fixtureJoinedCombat)
                    Game.Instance.Player.UpdateIsInCombat();
                if (_enemyBlueprint != null)
                    UnityEngine.Object.DestroyImmediate(_enemyBlueprint);
                if (_nonSummonBlueprint != null)
                    UnityEngine.Object.DestroyImmediate(_nonSummonBlueprint);
                if (_casterBlueprint != null)
                    UnityEngine.Object.DestroyImmediate(_casterBlueprint);
                if (_stateCaptured)
                {
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                        _initialTurnBasedSetting;
                    Game.Instance.TurnBasedCombatController.Activate();
                    Game.Instance.IsPaused = _initialPause;
                }
            }

            private static void DisposeUnit(UnitEntityData unit)
            {
                if (unit == null) return;
                try
                {
                    if (unit.CombatState != null &&
                        unit.CombatState.IsInCombat) unit.LeaveCombat();
                }
                catch { }
                try { Game.Instance.State.Units.All.Remove(unit); }
                catch { }
                try { unit.Descriptor.State.Immortality.ReleaseAll(); }
                catch { }
                try { unit.Dispose(); }
                catch { }
            }

            private static AbilityData PrepareQuickenedSummon(
                Spellbook spellbook)
            {
                SpellSlot ignored;
                return PrepareQuickenedSummon(spellbook,
                    SummonMonsterOneGuid, NativeDogName, 1, 5,
                    out ignored);
            }

            private static AbilityData PrepareQuickenedSummon(
                Spellbook spellbook, string parentGuid, string selectedName,
                int nativeSpellLevel, int preparedSpellLevel,
                out SpellSlot slot)
            {
                BlueprintAbility parent = BlueprintLibraryLookup
                    .RequireExact<BlueprintAbility>(
                        BlueprintBootstrap.Library, parentGuid,
                        "native summon parent for Quickened fixture");
                BlueprintAbility selected = BlueprintBootstrap.Library
                    .GetAllBlueprints().OfType<BlueprintAbility>().Single(value =>
                        value.name == selectedName);
                if ((parent.AvailableMetamagic & Metamagic.Quicken) == 0)
                    throw new InvalidOperationException(
                        "The native summon parent does not permit Quicken in " +
                        "the installed engine.");
                spellbook.AddKnown(nativeSpellLevel, parent, true);
                var metamagic = new MetamagicData();
                metamagic.Add(Metamagic.Quicken);
                metamagic.SpellLevelCost = Metamagic.Quicken.DefaultCost();
                var prepared = new AbilityData(parent, spellbook) {
                    MetamagicData = metamagic
                };
                if (!spellbook.Memorize(prepared, null))
                    throw new InvalidOperationException(
                        "The real Wizard spellbook rejected the legitimate " +
                        "Quickened summon preparation.");
                slot = spellbook.GetMemorizedSpellSlots(preparedSpellLevel)
                    .SingleOrDefault(value => value != null &&
                        value.Spell != null &&
                        ReferenceEquals(value.Spell.Blueprint, parent) &&
                        value.Spell.HasMetamagic(Metamagic.Quicken));
                if (slot == null)
                    throw new InvalidOperationException(
                        "Quickened preparation produced no exact level-" +
                        preparedSpellLevel + " slot.");
                slot.Available = true;
                slot.Spell.ParamSpellSlot = slot;
                var result = new AbilityData(slot.Spell, selected) {
                    ParamSpellSlot = slot
                };
                if (!result.HasMetamagic(Metamagic.Quicken) ||
                    result.ActionType != UnitCommand.CommandType.Swift ||
                    result.RequireFullRoundAction)
                    throw new InvalidOperationException(
                        "The installed AbilityData did not preserve legitimate " +
                        "Quicken action semantics.");
                return result;
            }

            private static AbilityData PreparePreparedSummon(
                Spellbook spellbook, string parentGuid, string selectedName,
                int spellLevel, out SpellSlot slot)
            {
                BlueprintAbility parent = BlueprintLibraryLookup
                    .RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                        parentGuid, "native prepared summon parent");
                BlueprintAbility selected = BlueprintBootstrap.Library
                    .GetAllBlueprints().OfType<BlueprintAbility>().Single(value =>
                        value.name == selectedName);
                spellbook.AddKnown(spellLevel, parent, true);
                if (!spellbook.Memorize(new AbilityData(parent, spellbook),
                        null))
                    throw new InvalidOperationException(
                        "The Wizard spellbook rejected native summon preparation.");
                slot = spellbook.GetMemorizedSpellSlots(spellLevel)
                    .SingleOrDefault(value => value != null &&
                        value.Spell != null &&
                        ReferenceEquals(value.Spell.Blueprint, parent));
                if (slot == null || slot.Spell == null)
                    throw new InvalidOperationException(
                        "Native summon preparation produced no exact slot.");
                slot.Available = true;
                slot.Spell.ParamSpellSlot = slot;
                return new AbilityData(slot.Spell, selected) {
                    ParamSpellSlot = slot
                };
            }

            private static AbilityData PrepareAcadamaeSummon(
                Spellbook spellbook, out SpellSlot slot)
            {
                AbilityData invocation = PreparePreparedSummon(spellbook,
                    SummonMonsterOneGuid, NativeDogName, 1, out slot);
                if (invocation.ConvertedFrom == null) return invocation;
                BlueprintAbility selected = invocation.Blueprint;
                BlueprintAbility canonical =
                    invocation.ConvertedFrom.Blueprint;
                var detachedCanonical = new AbilityData(canonical, spellbook) {
                    ParamSpellSlot = null
                };
                return new AbilityData(detachedCanonical, selected) {
                    ParamSpellSlot = null
                };
            }

            private static void AdvanceSpellcaster(UnitDescriptor owner,
                BlueprintCharacterClass characterClass, int levels,
                ref object activeController)
            {
                Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp
                    .LevelUpController);
                MethodInfo start = type.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo select = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) },
                    null);
                MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo apply = type.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance);
                MethodInfo cancel = type.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                object charGen = Enum.Parse(start.GetParameters()[4]
                    .ParameterType, "CharGen", false);
                for (int index = 0; index < levels; index++)
                {
                    activeController = start.Invoke(null,
                        new object[] { owner, false, null, null, charGen });
                    if (!(bool)select.Invoke(activeController,
                            new object[] { characterClass, false }))
                        throw new InvalidOperationException(
                            "Disposable Wizard selection failed at level " +
                            (index + 1) + ".");
                    mechanics.Invoke(activeController, null);
                    apply.Invoke(activeController, new object[] { owner });
                    cancel.Invoke(activeController, null);
                    activeController = null;
                }
            }

            private static void FinishAnimation(object handle)
            {
                PropertyInfo property = null;
                for (Type type = handle == null ? null : handle.GetType();
                    type != null && property == null; type = type.BaseType)
                    property = type.GetProperty("IsFinished",
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.DeclaredOnly);
                MethodInfo setter = property == null ? null :
                    property.GetSetMethod(true);
                if (setter == null) throw new MissingMethodException(
                    handle == null ? "<null>" : handle.GetType().FullName,
                    "set_IsFinished");
                setter.Invoke(handle, new object[] { true });
            }

            private static string Identity(UnitEntityData unit)
            {
                return unit == null ? "<null>" :
                    (unit.UniqueId ?? "<no-id>") + "/" +
                    (unit.CharacterName ?? "<unnamed>") + "#" +
                    RuntimeHelpers.GetHashCode(unit);
            }

            private string DescribeEvidence()
            {
                return "castRound=" + _evidence.CastRound +
                    ";currentBefore=" + _evidence.CurrentActorBefore +
                    ";currentAfter=" + _evidence.CurrentActorAfter +
                    ";action=" + _evidence.ActionType + "/" +
                    _evidence.RuntimeActionType + ";fullRound=actual:" +
                    _evidence.RequireFullRoundAction + ",blueprint:" +
                    _evidence.BlueprintFullRound + ";metamagic=" +
                    _evidence.Metamagic + ";rules=" +
                    _evidence.RuleCastCount + "/" +
                    _evidence.SpawnActionCount + "/" +
                    _evidence.RuleSummonCount + ";appear=" +
                    _evidence.AppearBuffAfterSpawn + ";duration=" +
                    _evidence.LifecycleSecondsAfterSpawn.ToString("R",
                        CultureInfo.InvariantCulture) + ";casterCooldown=" +
                    _evidence.CasterSwiftAfter.ToString("R",
                        CultureInfo.InvariantCulture) + "/" +
                    _evidence.CasterStandardAfter.ToString("R",
                        CultureInfo.InvariantCulture) + "/" +
                    _evidence.CasterMoveAfter.ToString("R",
                        CultureInfo.InvariantCulture);
            }

            private static string DescribeActivationPolicy(
                SummonSameTurnActivationRequest request,
                SummonSameTurnActivationDecision decision)
            {
                return "decision=" + decision.Disposition +
                    ";inCombat=" + request.InCombat +
                    ";turnBased=" + request.TurnBased +
                    ";genuine=" + request.GenuineSummonRule +
                    ";summoningSpell=" + request.SummoningSpell +
                    ";live=" + request.HasLiveSummon +
                    ";casterMatch=" + request.CasterMatchesInvocation +
                    ";casterTurn=" + request.CasterOwnsCurrentTurn +
                    ";acceleratedCommand=" +
                    request.AcceleratedCommandCorrelated +
                    ";actualFullRound=" +
                    request.ActualRequiresFullRound +
                    ";blueprintFullRound=" +
                    request.BlueprintRequiresFullRound +
                    ";acted=" + request.SummonAlreadyActed +
                    ";lifecycle=" + request.HasLifecycle + "/" +
                    request.LifecycleContextMatches +
                    ";appearance=" + request.HasAppearanceLock + "/" +
                    request.AppearanceContextMatches +
                    ";duration=" + request.ObservedLifecycleSeconds
                        .ToString("R", CultureInfo.InvariantCulture) + "/" +
                    request.ExpectedLifecycleSeconds.ToString("R",
                        CultureInfo.InvariantCulture);
            }

            private void Add(string name, string expected, string observed,
                bool passed, string evidence)
            {
                _assertions.Add(new RuntimeTestAssertion { Name = name,
                    Expected = expected, Observed = observed,
                    Status = passed ? RuntimeTestStatuses.Pass :
                        RuntimeTestStatuses.Fail, Evidence = evidence });
            }

            private static bool SameReferences(object[] expected,
                object[] actual)
            {
                if (expected == null || actual == null ||
                    expected.Length != actual.Length) return false;
                var remaining = new List<object>(actual);
                foreach (object value in expected)
                {
                    int index = remaining.FindIndex(candidate =>
                        ReferenceEquals(candidate, value));
                    if (index < 0) return false;
                    remaining.RemoveAt(index);
                }
                return remaining.Count == 0;
            }

            private static string DescribeReferenceDifference(
                object[] expected, object[] actual)
            {
                if (expected == null || actual == null) return "<null>";
                object[] added = actual.Where(value => !expected.Any(
                    candidate => ReferenceEquals(candidate, value))).ToArray();
                object[] missing = expected.Where(value => !actual.Any(
                    candidate => ReferenceEquals(candidate, value))).ToArray();
                return "expected=" + expected.Length + ",actual=" +
                    actual.Length + ",added=[" + string.Join(",",
                        added.Select(DescribeObject).ToArray()) +
                    "],missing=[" + string.Join(",",
                        missing.Select(DescribeObject).ToArray()) + "]";
            }

            private static string DescribeObject(object value)
            {
                var unit = value as UnitEntityData;
                return unit == null ? (value == null ? "<null>" :
                    value.GetType().FullName + "#" +
                    RuntimeHelpers.GetHashCode(value)) : Identity(unit);
            }
        }

        [JsonObject(MemberSerialization.OptOut)]
        public sealed class Evidence
        {
            public Evidence()
            {
                SummonStates = new List<string>();
                SummonTurnObservations = new List<string>();
                SummonCommands = new List<string>();
                LifecycleSecondsBySummon = new List<string>();
                UnitOpportunities = new List<string>();
                Turns = new string[0];
            }
            public string Case { get; set; }
            public string Caster { get; set; }
            public string Enemy { get; set; }
            public string Spellbook { get; set; }
            public string Spell { get; set; }
            public int CastRound { get; set; }
            public int SpellLevel { get; set; }
            public bool TurnBasedAtCast { get; set; }
            public string CurrentActorBefore { get; set; }
            public string CurrentActorAfter { get; set; }
            public string ActionType { get; set; }
            public string RuntimeActionType { get; set; }
            public string CommandType { get; set; }
            public string CommandResult { get; set; }
            public string Metamagic { get; set; }
            public bool RequireFullRoundAction { get; set; }
            public bool BlueprintFullRound { get; set; }
            public bool CanTarget { get; set; }
            public bool CanStart { get; set; }
            public int RuleCastCount { get; set; }
            public bool RuleCastSuccess { get; set; }
            public int SpawnActionCount { get; set; }
            public int RuleSummonCount { get; set; }
            public int ExpectedSummonMinimum { get; set; }
            public int ExpectedSummonMaximum { get; set; }
            public bool ExactSummonKind { get; set; }
            public string Summon { get; set; }
            public bool ContextAbilityReferenceExact { get; set; }
            public string ActivationDisposition { get; set; }
            public string ActivationPolicy { get; set; }
            public string AccelerationCorrelationTrace { get; set; }
            public string DuplicateDisposition { get; set; }
            public bool DuplicateNoOp { get; set; }
            public bool SlotAvailableBefore { get; set; }
            public bool SlotAvailableAfter { get; set; }
            public bool AcadamaeModeOn { get; set; }
            public int AcadamaeCompletedDelta { get; set; }
            public long AcadamaePublicationDelta { get; set; }
            public bool AcadamaeSavePassed { get; set; }
            public int AcadamaeNaturalRoll { get; set; }
            public int AcadamaeDifficultyClass { get; set; }
            public string AcadamaeFatigueDisposition { get; set; }
            public bool CasterFatigued { get; set; }
            public bool CasterExhausted { get; set; }
            public bool CancelledControlPassed { get; set; }
            public string CancelledControlDetails { get; set; }
            public bool NonSummonControlPassed { get; set; }
            public string NonSummonControlDetails { get; set; }
            public bool RtwpNativeActive { get; set; }
            public bool RtwpNativeAppearanceCleared { get; set; }
            public bool RtwpCurrentTurnAbsent { get; set; }
            public bool RtwpTurnOrderAbsent { get; set; }
            public bool ExpiredAtExpectedBoundary { get; set; }
            public float CasterSwiftBefore { get; set; }
            public float CasterStandardBefore { get; set; }
            public float CasterMoveBefore { get; set; }
            public float CasterSwiftAfter { get; set; }
            public float CasterStandardAfter { get; set; }
            public float CasterMoveAfter { get; set; }
            public bool CasterHasSwiftBefore { get; set; }
            public bool CasterHasStandardBefore { get; set; }
            public bool CasterHasMoveBefore { get; set; }
            public bool CasterHasSwiftAfter { get; set; }
            public bool CasterHasStandardAfter { get; set; }
            public bool CasterHasMoveAfter { get; set; }
            public bool CasterStillOwnsTurn { get; set; }
            public bool SummonInCombatAfterSpawn { get; set; }
            public bool SummonInTurnOrderAfterSpawn { get; set; }
            public bool SummonInCombatAtFirstTurn { get; set; }
            public bool SummonInTurnOrderAtFirstTurn { get; set; }
            public bool AppearBuffAfterSpawn { get; set; }
            public double ExpectedLifecycleSeconds { get; set; }
            public double LifecycleSecondsAfterSpawn { get; set; }
            public List<string> LifecycleSecondsBySummon { get; private set; }
            public int FirstSummonTurnRound { get; set; }
            public int FirstLawfulSummonTurnRound { get; set; }
            public int SameRoundSummonTurns { get; set; }
            public int SameRoundLawfulSummonTurns { get; set; }
            public int NextRoundSummonTurns { get; set; }
            public int NextRoundLawfulSummonTurns { get; set; }
            public int FollowingRoundSummonTurns { get; set; }
            public int FollowingRoundLawfulSummonTurns { get; set; }
            public int FirstSummonCommandRound { get; set; }
            public int SameRoundSummonCommands { get; set; }
            public int NextRoundSummonCommands { get; set; }
            public int FirstSummonAttackRound { get; set; }
            public int SameRoundSummonAttacks { get; set; }
            public int NextRoundSummonAttacks { get; set; }
            public int ForcedTurnCount { get; set; }
            public string[] Turns { get; set; }
            public List<string> SummonStates { get; private set; }
            public List<string> SummonTurnObservations { get; private set; }
            public List<string> SummonCommands { get; private set; }
            public List<string> UnitOpportunities { get; private set; }
            public string CleanupDetails { get; set; }
            public bool Cleaned { get; set; }
        }

        [HarmonyPatch(typeof(RuleCastSpell), "OnTrigger",
            new[] { typeof(RulebookEventContext) })]
        private static class RuleCastObserverPatch
        {
            private static void Postfix(RuleCastSpell __instance)
            {
                Session session = Active;
                if (session == null) return;
                try { session.ObserveRuleCast(__instance); }
                catch (Exception exception)
                { session.ObserveFailure("RuleCastSpell", exception); }
            }
        }

        [HarmonyPatch(typeof(ContextActionSpawnMonster), "RunAction")]
        private static class SpawnActionObserverPatch
        {
            private static void Prefix(ContextActionSpawnMonster __instance)
            {
                Session session = Active;
                if (session == null) return;
                try { session.ObserveSpawnAction(__instance); }
                catch (Exception exception)
                { session.ObserveFailure("ContextActionSpawnMonster", exception); }
            }
        }

        [HarmonyPatch(typeof(RuleSummonUnit), "OnTrigger",
            new[] { typeof(RulebookEventContext) })]
        private static class SummonRuleObserverPatch
        {
            private static void Postfix(RuleSummonUnit __instance)
            {
                Session session = Active;
                if (session == null) return;
                try { session.ObserveSummonRule(__instance); }
                catch (Exception exception)
                { session.ObserveFailure("RuleSummonUnit", exception); }
            }
        }

        [HarmonyPatch(typeof(TurnController), "Prepare", new Type[0])]
        private static class TurnPrepareObserverPatch
        {
            private static void Postfix(TurnController __instance)
            {
                Session session = Active;
                if (session == null) return;
                try { session.ObserveTurnPrepared(__instance); }
                catch (Exception exception)
                { session.ObserveFailure("TurnController.Prepare", exception); }
            }
        }

        [HarmonyPatch(typeof(UnitCommands), "Run",
            new[] { typeof(UnitCommand) })]
        private static class SummonCommandObserverPatch
        {
            private static void Postfix(UnitCommand cmd)
            {
                Session session = Active;
                if (session == null) return;
                try { session.ObserveSummonCommand(cmd); }
                catch (Exception exception)
                { session.ObserveFailure("UnitCommands.Run", exception); }
            }
        }

        [HarmonyPatch(typeof(RuleAttackWithWeapon), "OnTrigger",
            new[] { typeof(RulebookEventContext) })]
        private static class SummonAttackObserverPatch
        {
            private static void Postfix(RuleAttackWithWeapon __instance)
            {
                Session session = Active;
                if (session == null) return;
                try { session.ObserveSummonAttack(__instance); }
                catch (Exception exception)
                { session.ObserveFailure("RuleAttackWithWeapon", exception); }
            }
        }
    }
}
