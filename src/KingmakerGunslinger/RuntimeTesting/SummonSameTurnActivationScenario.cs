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
        private const string NativeDogName =
            "KMG_Summoning_Native_SM_Tier1";
        private static readonly object ActiveGate = new object();
        private static Session _active;

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

        internal sealed class Session
        {
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
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
            private readonly List<string> _turns = new List<string>();
            private readonly HashSet<TurnController> _preparedSummonTurns =
                new HashSet<TurnController>();
            private readonly Dictionary<UnitEntityData, bool> _combatBefore =
                new Dictionary<UnitEntityData, bool>();
            private UnitEntityData _areaAnchor;
            private UnitEntityData _caster;
            private UnitEntityData _enemy;
            private BlueprintUnit _casterBlueprint;
            private BlueprintUnit _enemyBlueprint;
            private Spellbook _spellbook;
            private AbilityData _castAbility;
            private UnitUseAbility _castCommand;
            private RuleSummonUnit _summonRule;
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
            private int _firstSummonCommandRound;
            private int _sameRoundSummonCommands;
            private int _nextRoundSummonCommands;
            private int _firstSummonAttackRound;
            private int _sameRoundSummonAttacks;
            private int _nextRoundSummonAttacks;
            private bool _castCaptureActive;
            private TurnController _lastTurn;
            private TurnController _lastForcedTurn;
            private string _failureStage = "not-started";

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
                            CastQuickenedSummon();
                            _stage = 3;
                            break;
                        case 3:
                            PollQuickenedSummonResolution();
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
                AdvanceSpellcaster(_caster.Descriptor, wizard, 20,
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
                _castAbility = PrepareQuickenedSummon(_spellbook);

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

                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = true;
                Game.Instance.TurnBasedCombatController.Activate();
                _caster.JoinCombat();
                _enemy.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                _fixtureJoinedCombat = true;
                if (!Game.Instance.Player.IsInCombat)
                    throw new InvalidOperationException(
                        "The disposable player caster did not establish party combat.");
                Game.Instance.TurnBasedCombatController
                    .HandlePartyCombatStateChanged(true);
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
                _failureStage = "wait-caster-turn";
            }

            private void PollUntilCasterTurn()
            {
                CombatController controller =
                    Game.Instance.TurnBasedCombatController;
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

            private void CastQuickenedSummon()
            {
                _failureStage = "quickened-real-player-cast";
                CombatController controller =
                    Game.Instance.TurnBasedCombatController;
                TurnController current = controller.CurrentTurn;
                if (current == null || !ReferenceEquals(current.Unit, _caster))
                    throw new InvalidOperationException(
                        "The caster lost the current turn before the cast.");
                _castRound = controller.RoundNumber;
                _evidence.CastRound = _castRound;
                _evidence.CurrentActorBefore = Identity(current.Unit);
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
                _summonRule = null;
                _castCaptureActive = true;
                var target = new TargetWrapper(_caster.Position +
                    new Vector3(1.5f, 0f, 0f));
                _evidence.CanTarget = _castAbility.CanTarget(target);
                _castCommand = new UnitUseAbility(_castAbility, target);
                _evidence.CommandType = _castCommand.Type.ToString();
                _evidence.CanStart = _castCommand.CanStart;
                if (!_castAbility.IsAvailable || !_evidence.CanTarget ||
                    !_castCommand.CanStart)
                    throw new InvalidOperationException(
                        "The legitimate Quickened summon was unavailable: " +
                        "available=" + _castAbility.IsAvailable +
                        ";target=" + _evidence.CanTarget + ";canStart=" +
                        _castCommand.CanStart + ";reason=" +
                        _castAbility.GetUnavailableReason() + ".");
                _caster.Commands.Run(_castCommand);
                Game.Instance.IsPaused = false;
                _failureStage = "quickened-native-command-resolution";
            }

            private void PollQuickenedSummonResolution()
            {
                CombatController controller =
                    Game.Instance.TurnBasedCombatController;
                if (_castCommand == null)
                    throw new InvalidOperationException(
                        "The native Quickened command was not retained.");
                if (!_castCommand.IsStarted) return;
                if (_castCommand.Animation != null &&
                    !_castCommand.Animation.IsActed)
                    _castCommand.Animation.IsActed = true;
                if (!_castCommand.IsActed) return;
                if (_castCommand.ExecutionProcess == null)
                    throw new InvalidOperationException(
                        "The native Quickened command acted without creating " +
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
                        "The native Quickened command did not succeed; result=" +
                        _evidence.CommandResult + ".");
                _evidence.RuleSummonCount = _summons.Count;
                if (_summons.Count != 1 || _summonRule == null)
                    throw new InvalidOperationException(
                        "Expected exactly one real RuleSummonUnit result; rules=" +
                        _summons.Count + ".");
                UnitEntityData summon = _summons[0];
                _evidence.Summon = Identity(summon);
                _evidence.ContextAbilityReferenceExact = _summonRule.Context !=
                    null && _summonRule.Context.SourceAbilityContext != null &&
                    ReferenceEquals(_summonRule.Context.SourceAbilityContext
                        .Ability, _castAbility);
                _evidence.ExpectedLifecycleSeconds =
                    (_summonRule.Duration.Seconds +
                        _summonRule.BonusDuration.Seconds).TotalSeconds;
                SummonSameTurnActivationRequest activationRequest;
                SummonSameTurnActivationDecision activationDecision =
                    SummonSameTurnActivationRuntime.Inspect(_summonRule,
                        out activationRequest);
                _evidence.ActivationDisposition =
                    activationDecision.Disposition.ToString();
                _evidence.ActivationPolicy = DescribeActivationPolicy(
                    activationRequest, activationDecision);
                _diagnostics.Add("activation-policy=" +
                    _evidence.ActivationPolicy + ";abilityType=" +
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
                    SummonSameTurnActivationRuntime.TryRepair(_summonRule);
                bool duplicateAppearAfter = summon.Descriptor.Buffs.GetBuff(
                    BlueprintRoot.Instance.SystemMechanics
                        .SummonedUnitAppearBuff) != null;
                duplicateLifecycle = summon.Descriptor.Buffs.GetBuff(
                    BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
                double duplicateDurationAfter = duplicateLifecycle == null ?
                    -1d : duplicateLifecycle.TimeLeft.TotalSeconds;
                _evidence.DuplicateDisposition =
                    duplicateDecision.Disposition.ToString();
                _evidence.DuplicateNoOp = !duplicateDecision.ShouldRepair &&
                    duplicateAppearBefore == duplicateAppearAfter &&
                    Math.Abs(duplicateDurationBefore -
                        duplicateDurationAfter) <= 0.001d;
                CaptureSummonState("post-spawn", summon);
                CaptureCasterActions(false);
                _evidence.CurrentActorAfter = controller.CurrentTurn == null ?
                    "<none>" : Identity(controller.CurrentTurn.Unit);
                _evidence.CasterStillOwnsTurn = controller.CurrentTurn != null &&
                    ReferenceEquals(controller.CurrentTurn.Unit, _caster);
                _diagnostics.Add(DescribeEvidence());
                if (!_evidence.CasterStillOwnsTurn)
                    throw new InvalidOperationException(
                        "The cast changed current-turn ownership before the " +
                        "caster intentionally ended the turn.");
                Game.Instance.IsPaused = false;
                controller.CurrentTurn.ForceToEnd(true);
                _lastForcedTurn = controller.CurrentTurn;
                _forcedTurns++;
                _failureStage = "observe-first-summon-turn";
                _stage = 4;
            }

            private void PollSummonOpportunity()
            {
                CombatController controller =
                    Game.Instance.TurnBasedCombatController;
                TurnController turn = controller.CurrentTurn;
                if (turn == null) return;
                RecordTurn(turn);
                UnitEntityData summon = _summons.Single();
                bool nextRoundSettled = _nextRoundLawfulSummonTurns > 0 &&
                    (_sameRoundSummonAttacks > 0 || turn.IsEnding ||
                        !ReferenceEquals(turn.Unit, summon));
                if (nextRoundSettled ||
                    controller.RoundNumber > _castRound + 1)
                {
                    _evidence.FirstLawfulSummonTurnRound =
                        _firstLawfulSummonTurnRound;
                    _evidence.SameRoundSummonTurns = _sameRoundSummonTurns;
                    _evidence.SameRoundLawfulSummonTurns =
                        _sameRoundLawfulSummonTurns;
                    _evidence.NextRoundSummonTurns = _nextRoundSummonTurns;
                    _evidence.NextRoundLawfulSummonTurns =
                        _nextRoundLawfulSummonTurns;
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
                    CaptureSummonState("observation-end", summon);
                    AddAssertions();
                    _stage = 5;
                    return;
                }
                if (!ReferenceEquals(turn.Unit, summon))
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
                if (_evidence.FirstSummonTurnRound == 0)
                {
                    _evidence.FirstSummonTurnRound = round;
                    _evidence.SummonInCombatAtFirstTurn =
                        summon.CombatState.IsInCombat;
                    _evidence.SummonInTurnOrderAtFirstTurn = Game.Instance
                        .TurnBasedCombatController.SortedUnits.Contains(summon);
                }
                if (lawful && _firstLawfulSummonTurnRound == 0)
                    _firstLawfulSummonTurnRound = round;
                if (round == _castRound)
                {
                    _sameRoundSummonTurns++;
                    if (lawful) _sameRoundLawfulSummonTurns++;
                }
                else if (round == _castRound + 1)
                {
                    _nextRoundSummonTurns++;
                    if (lawful) _nextRoundLawfulSummonTurns++;
                }
            }

            internal void ObserveSummonCommand(UnitCommand command)
            {
                if (command == null || command.Executor == null ||
                    !_summons.Contains(command.Executor)) return;
                CombatController controller = Game.Instance == null ? null :
                    Game.Instance.TurnBasedCombatController;
                if (controller == null || controller.CurrentTurn == null ||
                    !ReferenceEquals(controller.CurrentTurn.Unit,
                        command.Executor)) return;
                int round = controller.RoundNumber;
                if (_firstSummonCommandRound == 0)
                    _firstSummonCommandRound = round;
                if (round == _castRound) _sameRoundSummonCommands++;
                else if (round == _castRound + 1)
                    _nextRoundSummonCommands++;
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
                if (controller == null || controller.CurrentTurn == null ||
                    !ReferenceEquals(controller.CurrentTurn.Unit,
                        attack.Initiator)) return;
                int round = controller.RoundNumber;
                if (_firstSummonAttackRound == 0)
                    _firstSummonAttackRound = round;
                if (round == _castRound) _sameRoundSummonAttacks++;
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
                }
                else
                {
                    _evidence.CasterSwiftAfter = cooldown.SwiftAction;
                    _evidence.CasterStandardAfter = cooldown.StandardAction;
                    _evidence.CasterMoveAfter = cooldown.MoveAction;
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
                        summon.CombatState.IsInCombat;
                    _evidence.SummonInTurnOrderAfterSpawn =
                        controller.SortedUnits.Contains(summon);
                    _evidence.AppearBuffAfterSpawn = appear != null;
                    _evidence.LifecycleSecondsAfterSpawn = lifecycle == null ?
                        -1d : lifecycle.TimeLeft.TotalSeconds;
                }
            }

            private void AddAssertions()
            {
                Add("quickened-real-player-path",
                    "Swift actual AbilityData through UnitUseAbility, RuleCastSpell, ContextActionSpawnMonster, and RuleSummonUnit",
                    "action=" + _evidence.ActionType + ";runtime=" +
                        _evidence.RuntimeActionType + ";metamagic=" +
                        _evidence.Metamagic + ";ruleCast=" +
                        _evidence.RuleCastCount + ";spawn=" +
                        _evidence.SpawnActionCount + ";summon=" +
                        _evidence.RuleSummonCount,
                    _evidence.ActionType == UnitCommand.CommandType.Swift
                        .ToString() &&
                    _evidence.RuntimeActionType == UnitCommand.CommandType
                        .Swift.ToString() &&
                    _evidence.Metamagic.IndexOf("Quicken",
                        StringComparison.Ordinal) >= 0 &&
                    _evidence.RuleCastCount == 1 &&
                    _evidence.SpawnActionCount == 1 &&
                    _evidence.RuleSummonCount == 1 &&
                    _evidence.ContextAbilityReferenceExact,
                    "exact installed runtime objects and reference identity");
                Add("quickened-caster-actions-preserved",
                    "Swift spent; Standard and Move unchanged; caster retains current turn after spawn",
                    "swift=" + _evidence.CasterSwiftBefore + "->" +
                        _evidence.CasterSwiftAfter + ";standard=" +
                        _evidence.CasterStandardBefore + "->" +
                        _evidence.CasterStandardAfter + ";move=" +
                        _evidence.CasterMoveBefore + "->" +
                        _evidence.CasterMoveAfter + ";current=" +
                        _evidence.CasterStillOwnsTurn,
                    _evidence.CasterStillOwnsTurn &&
                    _evidence.CasterSwiftAfter >= 5.999f &&
                    _evidence.CasterStandardAfter <= 0.001f &&
                    _evidence.CasterMoveAfter <= 0.001f,
                    "native UnitCombatState cooldowns and CurrentTurn");
                Add("accelerated-summon-enrollment",
                    "live genuine summon enters combat and turn order before its first scheduled turn",
                    "postSpawn=" + _evidence.SummonInCombatAfterSpawn + "/" +
                        _evidence.SummonInTurnOrderAfterSpawn +
                        ";firstTurn=" +
                        _evidence.SummonInCombatAtFirstTurn + "/" +
                        _evidence.SummonInTurnOrderAtFirstTurn,
                    _evidence.SummonInCombatAtFirstTurn &&
                    _evidence.SummonInTurnOrderAtFirstTurn,
                    "RuleSummonUnit result, UnitCombatState, and CombatController.SortedUnits");
                Add("accelerated-summon-current-round-opportunity",
                    "exactly one lawful summon turn occurs in cast round " +
                        _castRound,
                    "current=" + _sameRoundSummonTurns + ";lawful=" +
                        _sameRoundLawfulSummonTurns + ";firstLawful=" +
                        (_firstLawfulSummonTurnRound == 0 ? "none" :
                            _firstLawfulSummonTurnRound.ToString(
                                CultureInfo.InvariantCulture)),
                    _sameRoundSummonTurns == 1 &&
                    _sameRoundLawfulSummonTurns == 1 &&
                    _firstLawfulSummonTurnRound == _castRound,
                    "CurrentTurn status, CanActInCombat, appearance lock, and acted state");
                Add("accelerated-summon-single-opportunity",
                    "one current-round entry and no duplicate lawful activation",
                    "current=" + _sameRoundSummonTurns + ";lawful=" +
                        _sameRoundLawfulSummonTurns,
                    _sameRoundSummonTurns == 1 &&
                    _sameRoundLawfulSummonTurns == 1,
                    "spawned-unit identity plus round-correlated turns");
                Add("accelerated-summon-next-round-normal",
                    "exactly one lawful summon turn in the following round",
                    "current=" + _nextRoundSummonTurns + ";lawful=" +
                        _nextRoundLawfulSummonTurns,
                    _nextRoundSummonTurns == 1 &&
                    _nextRoundLawfulSummonTurns == 1,
                    "same spawned-unit identity in castRound+1");
                Add("accelerated-summon-first-command",
                    "native summon AI issues UnitAttack commands in both the cast-round and following-round lawful turns",
                    "firstRound=" + (_firstSummonCommandRound == 0 ?
                        "none" : _firstSummonCommandRound.ToString(
                            CultureInfo.InvariantCulture)) + ";current=" +
                        _sameRoundSummonCommands + ";next=" +
                        _nextRoundSummonCommands,
                    _firstSummonCommandRound == _castRound &&
                    _sameRoundSummonCommands > 0 &&
                    _nextRoundSummonCommands > 0 &&
                    _evidence.SummonCommands.All(value => value.IndexOf(
                        typeof(UnitAttack).FullName,
                        StringComparison.Ordinal) >= 0),
                    "UnitCommands.Run from the summoned unit during its native CurrentTurn");
                Add("accelerated-summon-single-action",
                    "the tier-one dog resolves exactly one RuleAttackWithWeapon during its one lawful cast-round opportunity",
                    "firstRound=" + (_firstSummonAttackRound == 0 ?
                        "none" : _firstSummonAttackRound.ToString(
                            CultureInfo.InvariantCulture)) + ";current=" +
                        _sameRoundSummonAttacks + ";next=" +
                        _nextRoundSummonAttacks,
                    _firstSummonAttackRound == _castRound &&
                    _sameRoundSummonAttacks == 1,
                    "RuleAttackWithWeapon.OnTrigger correlated to summoned-unit identity and CurrentTurn");
                Add("accelerated-summon-duration",
                    "lifecycle equals RuleSummonUnit Duration plus BonusDuration without full-round grace",
                    "expected=" + _evidence.ExpectedLifecycleSeconds
                        .ToString("0.###", CultureInfo.InvariantCulture) +
                        ";remaining=" + _evidence.LifecycleSecondsAfterSpawn
                        .ToString("0.###", CultureInfo.InvariantCulture),
                    Math.Abs(_evidence.LifecycleSecondsAfterSpawn -
                        _evidence.ExpectedLifecycleSeconds) <= 1d,
                    "canonical SummonedUnitBuff and exact RuleSummonUnit durations");
                Add("accelerated-summon-duplicate-callback",
                    "a repeated exact-unit callback is AlreadyEligible and changes neither canonical buff",
                    "decision=" + _evidence.DuplicateDisposition +
                        ";noOp=" + _evidence.DuplicateNoOp,
                    _evidence.DuplicateDisposition ==
                        SummonSameTurnActivationDisposition.AlreadyEligible
                            .ToString() && _evidence.DuplicateNoOp,
                    "stateless canonical-state normalization on the same RuleSummonUnit result");
                Add("runtime-cleanup-pending", "evaluated during finally",
                    "pending", true,
                    "request-local unit and game-state snapshots");
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
                _summonRule = rule;
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
                if (_fixtureJoinedCombat)
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
                BlueprintAbility parent = BlueprintLibraryLookup
                    .RequireExact<BlueprintAbility>(
                        BlueprintBootstrap.Library, SummonMonsterOneGuid,
                        "native Summon Monster I");
                BlueprintAbility selected = BlueprintBootstrap.Library
                    .GetAllBlueprints().OfType<BlueprintAbility>().Single(value =>
                        value.name == NativeDogName);
                if ((parent.AvailableMetamagic & Metamagic.Quicken) == 0)
                    throw new InvalidOperationException(
                        "Native Summon Monster I does not permit Quicken in " +
                        "the installed engine.");
                spellbook.AddKnown(1, parent, true);
                var metamagic = new MetamagicData();
                metamagic.Add(Metamagic.Quicken);
                metamagic.SpellLevelCost = Metamagic.Quicken.DefaultCost();
                var prepared = new AbilityData(parent, spellbook) {
                    MetamagicData = metamagic
                };
                if (!spellbook.Memorize(prepared, null))
                    throw new InvalidOperationException(
                        "The real Wizard spellbook rejected the legitimate " +
                        "Quickened Summon Monster I preparation.");
                SpellSlot slot = spellbook.GetMemorizedSpellSlots(5)
                    .SingleOrDefault(value => value != null &&
                        value.Spell != null &&
                        ReferenceEquals(value.Spell.Blueprint, parent) &&
                        value.Spell.HasMetamagic(Metamagic.Quicken));
                if (slot == null)
                    throw new InvalidOperationException(
                        "Quickened preparation produced no exact level-five slot.");
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
                Turns = new string[0];
            }
            public string Caster { get; set; }
            public string Enemy { get; set; }
            public string Spellbook { get; set; }
            public string Spell { get; set; }
            public int CastRound { get; set; }
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
            public string Summon { get; set; }
            public bool ContextAbilityReferenceExact { get; set; }
            public string ActivationDisposition { get; set; }
            public string ActivationPolicy { get; set; }
            public string DuplicateDisposition { get; set; }
            public bool DuplicateNoOp { get; set; }
            public float CasterSwiftBefore { get; set; }
            public float CasterStandardBefore { get; set; }
            public float CasterMoveBefore { get; set; }
            public float CasterSwiftAfter { get; set; }
            public float CasterStandardAfter { get; set; }
            public float CasterMoveAfter { get; set; }
            public bool CasterStillOwnsTurn { get; set; }
            public bool SummonInCombatAfterSpawn { get; set; }
            public bool SummonInTurnOrderAfterSpawn { get; set; }
            public bool SummonInCombatAtFirstTurn { get; set; }
            public bool SummonInTurnOrderAtFirstTurn { get; set; }
            public bool AppearBuffAfterSpawn { get; set; }
            public double ExpectedLifecycleSeconds { get; set; }
            public double LifecycleSecondsAfterSpawn { get; set; }
            public int FirstSummonTurnRound { get; set; }
            public int FirstLawfulSummonTurnRound { get; set; }
            public int SameRoundSummonTurns { get; set; }
            public int SameRoundLawfulSummonTurns { get; set; }
            public int NextRoundSummonTurns { get; set; }
            public int NextRoundLawfulSummonTurns { get; set; }
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
