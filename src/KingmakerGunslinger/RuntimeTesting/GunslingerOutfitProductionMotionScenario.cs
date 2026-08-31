using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Presentation;
using KingmakerGunslinger.Reloading;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded dynamic qualification of the exact production class outfit via
    /// native movement, turn, UnitAttack, and Reload Firearm commands.
    /// </summary>
    internal static partial class GunslingerOutfitRenderScenario
    {
        private const string OutfitMotionShortswordItemGuid =
            "57c8994d1f1becf49ac4f642e5d8ca9d";

        private static readonly ProductionMotionSpec[] ProductionMotionSpecs =
        {
            new ProductionMotionSpec("unarmed-idle", "idle", "none"),
            new ProductionMotionSpec("musket-slow-walk", "movement-slow", "musket"),
            new ProductionMotionSpec("musket-normal-run", "movement-run", "musket"),
            new ProductionMotionSpec("musket-turn-right", "turn", "musket"),
            new ProductionMotionSpec("pistol-native-attack", "attack", "pistol"),
            new ProductionMotionSpec("musket-native-attack", "attack", "musket"),
            new ProductionMotionSpec("musket-production-reload", "reload", "musket"),
            new ProductionMotionSpec("shortsword-native-melee", "attack", "shortsword")
        };

        internal static ProductionCompatibilitySession BeginProductionMotion(
            ModContext context, RuntimeTestRequest request)
        {
            return new ProductionCompatibilitySession(context, request);
        }

        private sealed class ProductionMotionSpec
        {
            internal ProductionMotionSpec(string label, string kind, string weapon)
            {
                Label = label;
                Kind = kind;
                Weapon = weapon;
            }

            internal readonly string Label;
            internal readonly string Kind;
            internal readonly string Weapon;
        }

        internal sealed partial class ProductionCompatibilitySession
        {
            private static readonly int[] ProductionMotionAttackUpdates =
            {
                1, 12, 36
            };

            private static readonly int[] ProductionMotionReloadUpdates =
            {
                1, 12, 36, 96, 160, 240
            };

            private const int MotionReadyUpdates = 30;
            private const int MotionMaximumUpdates = 360;
            private readonly JArray _motionFixtureRecords = new JArray();
            private readonly JArray _motionRecords = new JArray();
            private readonly JArray _motionMovementOutcomes = new JArray();
            private readonly JArray _motionTurnOutcomes = new JArray();
            private readonly JArray _motionAttackOutcomes = new JArray();
            private readonly JArray _motionReloadOutcomes = new JArray();
            private readonly JArray _motionRestorationRecords = new JArray();
            private readonly JArray _motionCombatBoundaryRecords = new JArray();
            private Player _motionPlayer;
            private TurnBased.Controllers.CombatController
                _motionTurnBasedController;
            private UnitCombatLeaveController _motionCombatLeaveController;
            private UnitCombatJoinController _motionCombatJoinController;
            private AreaPersistentState _motionAreaState;
            private SceneEntitiesState _motionLoadedSceneState;
            private SceneEntitiesState _motionSceneState;
            private bool _motionSceneDisposed;
            private object[] _motionControllableBefore = new object[0];
            private object[] _motionCrossSceneBefore = new object[0];
            private bool _motionPlayerCombatBefore;
            private bool _motionTurnBasedCombatBefore;
            private int _motionPartyCombatantsBefore;
            private bool _motionTurnBasedHasEnemyBefore;
            private bool _motionTurnBasedHadEnemyBefore;
            private int _motionTurnBasedUnitsBefore;
            private BlueprintItem _motionPowder;
            private BlueprintItem _motionBall;
            private int _motionPowderBefore;
            private int _motionBallBefore;
            private int _motionAmmunitionSeed;
            private bool _motionInventoryCaptured;
            private bool _motionInventoryRestored;
            private BlueprintAbility _motionReloadBlueprint;
            private Ability _motionReloadAbility;
            private AbilityData _motionAbilityData;
            private UnitUseAbility _motionReloadCommand;
            private EffectiveReloadAction _motionExpectedReloadAction;
            private int _motionPlannedRounds;
            private UnitEntityData _motionTarget;
            private BlueprintUnit _motionHostileBlueprint;
            private BlueprintFaction _motionActorFaction;
            private BlueprintFaction _motionTargetFaction;
            private Renderer[] _motionBodyRenderers = new Renderer[0];
            private ProductionMotionSpec _motionSpec;
            private ItemEntityWeapon _motionWeapon;
            private bool _motionFirearmStateSet;
            private Transform _motionRemovedPresentation;
            private UnitMoveTo _motionMoveCommand;
            private UnitAttack _motionAttackCommand;
            private float? _motionOriginalMaxSpeedOverride;
            private UnitAnimationActionLocoMotion.WalkSpeedType
                _motionOriginalWalkSpeedType;
            private float _motionOriginalAnimationSpeed;
            private Vector3 _motionMovementStart;
            private Vector3 _motionMovementDestination;
            private uint _motionMovementStartArea;
            private uint _motionMovementDestinationArea;
            private uint _motionMovementGraphIndex;
            private float _motionMovementRequestedSpeed;
            private float _motionMovementDistance;
            private float _motionMovementMaximumVelocity;
            private float _motionWalkMaximumVelocity;
            private float _motionRunMaximumVelocity;
            private bool _motionMovementCommandAccepted;
            private bool _motionMovementObserved;
            private bool _motionVelocityObserved;

            private Vector3 _motionTurnStartForward;
            private float _motionTurnDegrees;
            private bool _motionAttackTargetPrepared;
            private bool _motionAttackProbeDetached;
            private bool _motionCommandInstalled;
            private bool _motionCommandCanStart;
            private bool _motionCommandCloseEnough;
            private bool _motionCommandTargetInState;
            private bool _motionCommandStarted;
            private bool _motionCommandRunningObserved;
            private bool _motionAttackRetirementReady;
            private bool _motionAnimationObserved;
            private bool _motionAnimationActedObserved;
            private bool _motionActedCaptureTaken;
            private bool _motionExecutionProcessObserved;
            private bool _motionExecutionProcessEndedObserved;
            private bool _motionAbilityAvailable;
            private bool _motionAbilityTargetable;
            private UnitCommand.CommandType _motionRuntimeActionType;
            private bool _motionRequireFullRoundAction;
            private int _motionExplicitCommandTicks;
            private int _motionExplicitProcessTicks;
            private int _motionActionUpdates;
            private int _motionCaptureScheduleIndex;
            private int _motionTargetAttempts;
            private string _motionTargetPlacement = "<not-prepared>";
            private float _motionTargetDistance;
            private float _motionApproachRadius;
            private bool _motionNeedLineOfSight;
            private long _motionFiredBefore;
            private long _motionDischargeFaultsBefore;
            private long _motionReloadAttemptsBefore;
            private long _motionReloadLoadedBefore;
            private long _motionReloadRejectedBefore;
            private long _motionReloadFaultsBefore;
            private int _motionPowderBeforeCommand;
            private int _motionBallBeforeCommand;
            private int _motionStep;
            private int _motionPhase;
            private int _motionCaptured;
            private int _motionViewCount;
            private int _motionRestorations;
            private bool _motionIndexWritten;
            private bool _motionPreviousActionCleared;

            private bool IsProductionMotion
            {
                get
                {
                    return string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .GunslingerOutfitProductionMotion,
                        StringComparison.Ordinal);
                }
            }

            private void PrepareProductionMotionActorBlueprint(
                BlueprintUnit blueprint)
            {
                if (blueprint == null || blueprint.Faction == null)
                    throw new InvalidOperationException(
                        "Production motion requires an exact source faction.");
                if (_motionActorFaction != null ||
                    _motionTargetFaction != null)
                    throw new InvalidOperationException(
                        "Production motion faction clones were not retired.");

                BlueprintFaction source = blueprint.Faction;
                _motionActorFaction = UnityEngine.Object.Instantiate(source);
                _motionTargetFaction = UnityEngine.Object.Instantiate(source);
                _motionActorFaction.name =
                    "KMG_Runtime_Gunslinger_Outfit_Motion_Actor_Faction";
                _motionTargetFaction.name =
                    "KMG_Runtime_Gunslinger_Outfit_Motion_Target_Faction";
                ConfigureProductionMotionFaction(_motionActorFaction,
                    _motionTargetFaction);
                ConfigureProductionMotionFaction(_motionTargetFaction,
                    _motionActorFaction);
                blueprint.Faction = _motionActorFaction;

                if (ReferenceEquals(source, _motionActorFaction) ||
                    ReferenceEquals(source, _motionTargetFaction) ||
                    ReferenceEquals(_motionActorFaction,
                        _motionTargetFaction))
                    throw new InvalidOperationException(
                        "Production motion faction isolation reused a native object.");
            }

            private static void ConfigureProductionMotionFaction(
                BlueprintFaction faction, BlueprintFaction enemy)
            {
                if (faction == null || enemy == null)
                    throw new ArgumentNullException("faction");
                faction.Peaceful = false;
                faction.AlwaysEnemy = false;
                faction.Neutral = false;
                faction.IsDirectlyControllable = false;
                faction.Dummy = null;
                faction.AttackFactions = new[] { enemy };
            }

            private SceneEntitiesState ProductionMotionHoldingState()
            {
                if (_motionAreaState == null ||
                    _motionLoadedSceneState == null ||
                    _motionSceneState == null || _motionSceneDisposed ||
                    !ReferenceEquals(_motionAreaState,
                        Game.Instance.State.LoadedAreaState) ||
                    !ReferenceEquals(_motionAreaState,
                        Game.Instance.CurrentScene) ||
                    !ReferenceEquals(_motionLoadedSceneState,
                        _motionAreaState.MainState) ||
                    !_motionLoadedSceneState.IsSceneLoaded ||
                    !_motionSceneState.IsSceneLoaded ||
                    ReferenceEquals(_motionSceneState,
                        _motionLoadedSceneState) ||
                    ReferenceEquals(_motionSceneState,
                        _motionPlayer.CrossSceneState) ||
                    !string.Equals(_motionSceneState.SceneName,
                        _motionLoadedSceneState.SceneName,
                        StringComparison.Ordinal) ||
                    !_motionSceneState.SkipSerialize)
                    throw new InvalidOperationException(
                        "Production motion lost its request-local loaded-scene holding state.");
                return _motionSceneState;
            }

            private void RefreshProductionMotionPlayerLists()
            {
                _motionPlayer.InvalidateCharacterLists();
                _motionPlayer.UpdateCharacterLists();
            }

            private bool ProductionMotionPlayerListsExact()
            {
                return _motionPlayer != null &&
                    _motionPlayer.CrossSceneState != null &&
                    SameReferences(_motionControllableBefore,
                        Snapshot(_motionPlayer.ControllableCharacters)) &&
                    SameReferences(_motionCrossSceneBefore,
                        Snapshot(_motionPlayer.CrossSceneState.AllEntityData));
            }

            private void PollProductionMotion()
            {
                if (_phase == 0)
                {
                    Initialize();
                    InitializeProductionMotion();
                    _phase = 1;
                    return;
                }
                if (_phase == 1)
                {
                    SpawnFixture();
                    _phase = 2;
                    _settleUpdates = 0;
                    return;
                }
                if (_phase == 2)
                {
                    PollFixtureReadiness();
                    return;
                }
                PollProductionMotionAction();
            }

            private void InitializeProductionMotion()
            {
                _stage = "initialize-production-motion";
                if (ProductionMotionSpecs.Length != 8 ||
                    ProductionMotionSpecs.Select(value => value.Label)
                        .Distinct(StringComparer.Ordinal).Count() != 8 ||
                    ProductionMotionAttackUpdates.Length != 3 ||
                    ProductionMotionReloadUpdates.Length != 6)
                    throw new InvalidOperationException(
                        "The production motion catalog or frame schedule changed.");
                _motionPlayer = Game.Instance.Player;
                _motionTurnBasedController = Game.Instance
                    .TurnBasedCombatController;
                _motionCombatLeaveController = Game.Instance
                    .GetController<UnitCombatLeaveController>(true);
                _motionCombatJoinController = Game.Instance
                    .GetController<UnitCombatJoinController>(true);
                _motionAreaState = Game.Instance.State.LoadedAreaState;
                _motionLoadedSceneState = _motionAreaState == null ? null :
                    _motionAreaState.MainState;
                _motionPowder = BlueprintBootstrap.BasicAmmunition.BlackPowder;
                _motionBall = BlueprintBootstrap.BasicAmmunition.LeadBall;
                _motionReloadBlueprint = BlueprintBootstrap
                    .ReloadTestMusketAbility;
                if (_motionPlayer == null ||
                    _motionTurnBasedController == null ||
                    _motionCombatLeaveController == null ||
                    _motionCombatJoinController == null ||
                    _motionAreaState == null ||
                    _motionLoadedSceneState == null ||
                    _motionPlayer.CrossSceneState == null ||
                    !ReferenceEquals(_motionAreaState,
                        Game.Instance.CurrentScene) ||
                    !ReferenceEquals(_motionLoadedSceneState,
                        _motionAreaState.MainState) ||
                    !_motionLoadedSceneState.IsSceneLoaded ||
                    ReferenceEquals(_motionLoadedSceneState,
                        _motionPlayer.CrossSceneState) ||
                    _motionPlayer.Inventory == null ||
                    _motionPowder == null || _motionBall == null ||
                    _motionReloadBlueprint == null ||
                    BlueprintBootstrap.ProductionFirearms == null ||
                    BlueprintBootstrap.ProductionFirearms.Pistol == null ||
                    BlueprintBootstrap.ProductionFirearms.Musket == null ||
                    BlueprintBootstrap.ProductionFirearms.Pistol.Item == null ||
                    BlueprintBootstrap.ProductionFirearms.Musket.Item == null)
                    throw new InvalidOperationException(
                        "Production motion dependencies are unavailable.");
                RefreshProductionMotionPlayerLists();
                _motionControllableBefore = Snapshot(
                    _motionPlayer.ControllableCharacters);
                _motionCrossSceneBefore = Snapshot(
                    _motionPlayer.CrossSceneState.AllEntityData);
                _motionPlayerCombatBefore = _motionPlayer.IsInCombat;
                _motionTurnBasedCombatBefore =
                    TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat();
                _motionPartyCombatantsBefore = _motionPlayer.Party.Count(
                    unit => unit != null && unit.CombatState != null &&
                        unit.CombatState.IsInCombat);
                _motionTurnBasedHasEnemyBefore =
                    _motionTurnBasedController.HasEnemyInCombat;
                _motionTurnBasedHadEnemyBefore =
                    _motionTurnBasedController.HadEnemyAtSomePoint;
                _motionTurnBasedUnitsBefore = _motionTurnBasedController
                    .SortedUnits.Count();
                if (_motionPlayerCombatBefore ||
                    _motionTurnBasedCombatBefore ||
                    _motionPartyCombatantsBefore != 0 ||
                    _motionTurnBasedHasEnemyBefore ||
                    _motionTurnBasedHadEnemyBefore ||
                    _motionTurnBasedUnitsBefore != 0)
                    throw new InvalidOperationException(
                        "Production motion requires a clean non-combat working save.");
                _motionSceneState = new SceneEntitiesState(
                    _motionLoadedSceneState.SceneName)
                {
                    SkipSerialize = true
                };
                _motionSceneDisposed = false;
                if (ReferenceEquals(_motionSceneState,
                        _motionLoadedSceneState) ||
                    ReferenceEquals(_motionSceneState,
                        _motionPlayer.CrossSceneState) ||
                    !_motionSceneState.IsSceneLoaded ||
                    _motionSceneState.AllEntityData.Count != 0)
                    throw new InvalidOperationException(
                        "Production motion did not create an empty request-local loaded-scene state.");
                _diagnostics.Add(
                    "productionMotionHoldingState=requestLocalLoadedScene:" +
                    _motionSceneState.GetType().FullName +
                    ";sceneName=" + _motionSceneState.SceneName +
                    ";loadedState=" +
                    _motionLoadedSceneState.GetType().FullName +
                    ";skipSerialize=" + _motionSceneState.SkipSerialize +
                    ";anchorCrossScene=" + ReferenceEquals(
                        _anchor.HoldingState,
                        _motionPlayer.CrossSceneState) +
                    ";controllableCount=" +
                    _motionControllableBefore.Length +
                    ";crossSceneCount=" + _motionCrossSceneBefore.Length);
                BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(
                    BlueprintBootstrap.Library,
                    OutfitMotionShortswordItemGuid,
                    "gunslinger-outfit-motion-native-shortsword");
                _motionPowderBefore = _motionPlayer.Inventory.Count(
                    _motionPowder);
                _motionBallBefore = _motionPlayer.Inventory.Count(_motionBall);
                _motionInventoryCaptured = true;
                _motionAmmunitionSeed = 4;
                _motionPlayer.Inventory.Add(_motionPowder,
                    _motionAmmunitionSeed);
                _motionPlayer.Inventory.Add(_motionBall,
                    _motionAmmunitionSeed);
                if (_motionPlayer.Inventory.Count(_motionPowder) !=
                        _motionPowderBefore + _motionAmmunitionSeed ||
                    _motionPlayer.Inventory.Count(_motionBall) !=
                        _motionBallBefore + _motionAmmunitionSeed)
                    throw new InvalidOperationException(
                        "Request-local motion ammunition was not seeded exactly.");
                _diagnostics.Add("productionMotionInventory=powder:" +
                    _motionPowderBefore + "+" + _motionAmmunitionSeed +
                    ";ball:" + _motionBallBefore + "+" +
                    _motionAmmunitionSeed);
                WriteProgress("motion-initialized");
            }

            private void PollProductionMotionAction()
            {
                switch (_motionPhase)
                {
                    case 0:
                        InitializeProductionMotionFixture();
                        _motionPhase = 1;
                        return;
                    case 1:
                        PrepareProductionMotionAction();
                        _motionPhase = 2;
                        _settleUpdates = 0;
                        return;
                    case 2:
                        PollProductionMotionReady();
                        return;
                    case 3:
                        PollProductionMotionExecution();
                        return;
                    case 4:
                        BeginProductionMotionRemoval();
                        _motionPhase = 5;
                        _settleUpdates = 0;
                        return;
                    default:
                        PollProductionMotionRemoval();
                        return;
                }
            }

            private void InitializeProductionMotionFixture()
            {
                ProductionCompatibilityFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "initialize-motion-" + fixture.Label;
                if (_actor == null || _actor.View == null ||
                    _actor.View.MovementAgent == null ||
                    _actor.View.AnimationManager == null ||
                    _avatar == null || !ProductionMotionOutfitExact())
                    throw new InvalidOperationException(fixture.Label +
                        " did not expose the exact production outfit and native motion contracts.");
                _motionBodyRenderers = ActiveRenderers(_actor);
                _motionOriginalMaxSpeedOverride = _actor.View.MovementAgent
                    .MaxSpeedOverride;
                _motionOriginalWalkSpeedType = _actor.View.AnimationManager
                    .WalkSpeedType;
                _motionOriginalAnimationSpeed = _actor.View.AnimationManager
                    .Speed;
                _actor.Descriptor.AddFact(_motionReloadBlueprint);
                _motionReloadAbility = _actor.Descriptor.Abilities.GetAbility(
                    _motionReloadBlueprint);
                if (_motionReloadAbility == null)
                    throw new InvalidOperationException(fixture.Label +
                        " did not retain the production Reload Firearm ability.");

                RefreshProductionMotionPlayerLists();
                if (_motionActorFaction == null ||
                    _motionTargetFaction == null || _actor.IsPlayerFaction ||
                    ReferenceEquals(_actor.Group, _motionPlayer.Group) ||
                    !ReferenceEquals(_actor.HoldingState,
                        ProductionMotionHoldingState()) ||
                    ContainsReference(_motionPlayer.ControllableCharacters,
                        _actor) || !ProductionMotionPlayerListsExact())
                    throw new InvalidOperationException(fixture.Label +
                        " did not isolate its request-local actor and area state.");
                _actor.Commands.InterruptAll(true);
                if (_actor.CombatState.IsInCombat)
                    _actor.LeaveCombat();
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(false);
                _motionFixtureRecords.Add(new JObject
                {
                    { "fixture", fixture.Label },
                    { "gender", fixture.Gender.ToString() },
                    { "raceId", fixture.Race.RaceId.ToString() },
                    { "productionAssetIds", new JArray(
                        CurrentProductionAssetIds()) },
                    { "selectedHairAssetId", _hairAssetId },
                    { "productionOutfitExact",
                        ProductionMotionOutfitExact() },
                    { "humanoidRigExact",
                        HasExactHumanoidRig(_actor.View.transform) },
                    { "actorIsPlayerFaction", _actor.IsPlayerFaction },
                    { "actorSharesPlayerGroup",
                        ReferenceEquals(_actor.Group, _motionPlayer.Group) },
                    { "actorHoldingStateIsRequestLocalLoadedScene",
                        ReferenceEquals(_actor.HoldingState,
                            _motionSceneState) },
                    { "requestLocalSceneMatchesLoadedScene",
                        _motionLoadedSceneState != null &&
                        string.Equals(_motionSceneState.SceneName,
                            _motionLoadedSceneState.SceneName,
                            StringComparison.Ordinal) &&
                        _motionSceneState.IsSceneLoaded },
                    { "actorInControllableCharacters",
                        ContainsReference(
                            _motionPlayer.ControllableCharacters, _actor) },
                    { "playerCharacterListsExact",
                        ProductionMotionPlayerListsExact() },
                    { "actorGroupId", _actor.GroupId },
                    { "movementAgentType",
                        _actor.View.MovementAgent.GetType().FullName },
                    { "animationManagerType",
                        _actor.View.AnimationManager.GetType().FullName },
                    { "locomotionClipCount", MotionAnimationClipCount(
                        UnitAnimationType.LocoMotion) },
                    { "mainHandAttackClipCount", MotionAnimationClipCount(
                        UnitAnimationType.MainHandAttack) },
                    { "baseMaxSpeed",
                        _actor.View.MovementAgent.MaxSpeed },
                    { "originalMaxSpeedOverride",
                        _motionOriginalMaxSpeedOverride.HasValue
                            ? (JToken)_motionOriginalMaxSpeedOverride.Value
                            : JValue.CreateNull() },
                    { "originalWalkSpeedType",
                        _motionOriginalWalkSpeedType.ToString() },
                    { "originalAnimationSpeed",
                        _motionOriginalAnimationSpeed }
                });
                _motionWalkMaximumVelocity = 0f;
                _motionRunMaximumVelocity = 0f;
                _motionStep = 0;
                ResetProductionMotionActionState();
                WriteProgress("motion-fixture-ready");
            }

            private void CreateProductionMotionTarget()
            {
                if (_motionTarget != null ||
                    _motionHostileBlueprint != null)
                    throw new InvalidOperationException(
                        "Production motion retained a prior target.");
                if (_actor == null || _actorBlueprint == null ||
                    _motionTargetFaction == null || _anchor == null)
                    throw new InvalidOperationException(
                        "Production motion target dependencies are unavailable.");

                Vector3 targetPosition = NearestNavigable(_actor.Position +
                    Vector3.forward * 6f);
                _motionHostileBlueprint = UnityEngine.Object.Instantiate(
                    _actorBlueprint);
                _motionHostileBlueprint.name =
                    "KMG_Runtime_Gunslinger_Outfit_Motion_Target";
                _motionHostileBlueprint.Faction = _motionTargetFaction;
                _motionHostileBlueprint.IsCheater = true;
                _motionHostileBlueprint.StartingInventory =
                    Array.Empty<BlueprintItem>();
                _motionTarget = Game.Instance.EntityCreator.SpawnUnit(
                    _motionHostileBlueprint, targetPosition,
                    Quaternion.identity, ProductionMotionHoldingState());
                Game.Instance.EntityCreator.Tick();
                RefreshProductionMotionPlayerLists();
                if (_motionTarget == null || _motionTarget.View == null ||
                    _motionTarget.Descriptor == null ||
                    _motionTarget.IsPlayerFaction ||
                    !ReferenceEquals(_motionTarget.HoldingState,
                        _motionSceneState) ||
                    ContainsReference(_motionPlayer.ControllableCharacters,
                        _actor) ||
                    ContainsReference(_motionPlayer.ControllableCharacters,
                        _motionTarget) ||
                    !ProductionMotionPlayerListsExact() ||
                    ReferenceEquals(_motionTarget.Group,
                        _motionPlayer.Group) ||
                    !_actor.IsEnemy(_motionTarget) ||
                    !_motionTarget.IsEnemy(_actor) ||
                    _actor.IsEnemy(_anchor) || _anchor.IsEnemy(_actor) ||
                    _motionTarget.IsEnemy(_anchor) ||
                    _anchor.IsEnemy(_motionTarget))
                    throw new InvalidOperationException(
                        "Production motion target violated isolated bilateral hostility.");

                _motionTarget.Descriptor.State.Immortality.Retain();
                _motionTarget.Descriptor.Stats.HitPoints.BaseValue = 10000;
                _motionTarget.Descriptor.Damage = 0;
                _motionTarget.Commands.InterruptAll(true);
                if (_motionTarget.CombatState.IsInCombat)
                    _motionTarget.LeaveCombat();
                _diagnostics.Add("productionMotionIsolation=" +
                    _fixtures[_fixtureIndex].Label + ";actorGroup=" +
                    _actor.GroupId + ";targetGroup=" +
                    _motionTarget.GroupId + ";playerGroup=" +
                    _motionPlayer.Group.Id + ";bilateralEnemy=True;" +
                    "playerHostility=False;requestLocalLoadedScene=True;" +
                    "playerListsExact=True");
            }

            private int MotionAnimationClipCount(UnitAnimationType type)
            {
                var action = _actor == null || _actor.View == null ||
                    _actor.View.AnimationManager == null ? null :
                    _actor.View.AnimationManager.GetAction(type);
                return action == null ? 0 : action.Clips.Count(value =>
                    value != null);
            }

            private void PrepareProductionMotionAction()
            {
                _motionSpec = ProductionMotionSpecs[_motionStep];
                _stage = "prepare-motion-" +
                    _fixtures[_fixtureIndex].Label + "-" +
                    _motionSpec.Label;
                _motionPreviousActionCleared =
                    ProductionMotionTransientStateCleared();
                if (!_motionPreviousActionCleared)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " began before the previous action was cleared; " +
                        ProductionMotionPlayerBoundaryDescription() + ".");
                ResetProductionMotionActionState();
                _motionSpec = ProductionMotionSpecs[_motionStep];
                _motionPreviousActionCleared = true;
                if (string.Equals(_motionSpec.Kind, "attack",
                        StringComparison.Ordinal))
                    CreateProductionMotionTarget();
                else if (_motionTarget != null ||
                    _motionHostileBlueprint != null)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " retained a target outside an attack action.");
                if (!string.Equals(_motionSpec.Kind, "idle",
                        StringComparison.Ordinal))
                    EquipProductionMotionWeapon();
                WriteProgress("motion-action-prepared");
            }

            private void EquipProductionMotionWeapon()
            {
                BlueprintItemWeapon blueprint;
                switch (_motionSpec.Weapon)
                {
                    case "pistol":
                        blueprint = BlueprintBootstrap.ProductionFirearms
                            .Pistol.Item;
                        break;
                    case "musket":
                        blueprint = BlueprintBootstrap.ProductionFirearms
                            .Musket.Item;
                        break;
                    case "shortsword":
                        blueprint = BlueprintLibraryLookup.RequireExact<
                            BlueprintItemWeapon>(BlueprintBootstrap.Library,
                                OutfitMotionShortswordItemGuid,
                                "gunslinger-outfit-motion-native-shortsword");
                        break;
                    default:
                        throw new InvalidOperationException(
                            "Unknown production motion weapon " +
                            _motionSpec.Weapon + ".");
                }
                _motionWeapon = new ItemEntityWeapon(blueprint);
                _actor.Body.PrimaryHand.InsertItem(_motionWeapon);
                if (!ReferenceEquals(_actor.Body.PrimaryHand.MaybeWeapon,
                        _motionWeapon))
                    throw new InvalidOperationException(_motionSpec.Label +
                        " did not retain its exact primary-hand item.");
                if (!string.Equals(_motionSpec.Weapon, "shortsword",
                        StringComparison.Ordinal))
                {
                    bool reload = string.Equals(_motionSpec.Kind, "reload",
                        StringComparison.Ordinal);
                    FirearmRuntimeState.Service.Set(_motionWeapon,
                        new FirearmState(
                            FirearmState.CurrentSchemaVersion,
                            reload ? 0 : 1,
                            reload ? null : FirearmStateTokenCatalog
                                .DiagnosticLeadBall,
                            FirearmCondition.Normal));
                    _motionFirearmStateSet = true;
                }
                _actor.View.HandsEquipment.UpdateAll();
                _actor.View.HandsEquipment.ForceSwitch(true);
                if (string.Equals(_motionSpec.Kind, "attack",
                        StringComparison.Ordinal))
                {
                    if (!_actor.CombatState.IsInCombat)
                        _actor.CombatState.JoinCombat();
                    if (!_motionTarget.CombatState.IsInCombat)
                        _motionTarget.CombatState.JoinCombat();
                    _actor.CombatState.Engage(_motionTarget);
                }
            }

            private void PollProductionMotionExecution()
            {
                if (string.Equals(_motionSpec.Kind, "movement-slow",
                        StringComparison.Ordinal) ||
                    string.Equals(_motionSpec.Kind, "movement-run",
                        StringComparison.Ordinal))
                    PollProductionMotionMovement();
                else if (string.Equals(_motionSpec.Kind, "turn",
                        StringComparison.Ordinal))
                    PollProductionMotionTurn();
                else if (string.Equals(_motionSpec.Kind, "attack",
                        StringComparison.Ordinal))
                    PollProductionMotionAttack();
                else if (string.Equals(_motionSpec.Kind, "reload",
                        StringComparison.Ordinal))
                    PollProductionMotionReload();
                else
                    throw new InvalidOperationException(
                        "Unexpected production motion execution kind " +
                        _motionSpec.Kind + ".");
            }

            private void PollProductionMotionReady()
            {
                _stage = "settle-motion-" +
                    _fixtures[_fixtureIndex].Label + "-" +
                    _motionSpec.Label;
                TickProductionMotionRuntime();
                _settleUpdates++;
                if (!ProductionMotionOutfitExact())
                    throw new InvalidOperationException(_motionSpec.Label +
                        " changed the exact production outfit while settling.");
                if (string.Equals(_motionSpec.Kind, "idle",
                        StringComparison.Ordinal))
                {
                    if (_settleUpdates < MotionReadyUpdates) return;
                    CaptureProductionMotionRecord("unarmed-idle", 0,
                        "stable native unarmed idle on the exact production class outfit",
                        null, "none", true);
                    AdvanceProductionMotionAction();
                    return;
                }

                WeaponVisualParameters visual = _motionWeapon.Blueprint
                    .VisualParameters;
                string role;
                Transform model = WeaponPresentationEvidenceScenario
                    .ResolveActivePresentation(_actor, visual,
                        string.Equals(_motionSpec.Kind, "reload",
                            StringComparison.Ordinal) ? "reload-ready" :
                        string.Equals(_motionSpec.Kind, "attack",
                            StringComparison.Ordinal) ? "combat-ready" :
                            "held-idle", out role);
                bool ready = Renderable(model) &&
                    _actor.View.HandsEquipment.InCombat;
                if (!ready || _settleUpdates < MotionReadyUpdates)
                {
                    if (_settleUpdates < MotionMaximumUpdates) return;
                    throw new InvalidOperationException(_motionSpec.Label +
                        " did not settle to a renderable held state; role=" +
                        role + ";renderable=" + Renderable(model) +
                        ";handsInCombat=" +
                        _actor.View.HandsEquipment.InCombat + ".");
                }

                if (string.Equals(_motionSpec.Kind, "movement-slow",
                        StringComparison.Ordinal) ||
                    string.Equals(_motionSpec.Kind, "movement-run",
                        StringComparison.Ordinal))
                {
                    StartProductionMotionMovement();
                    _motionPhase = 3;
                    return;
                }
                if (string.Equals(_motionSpec.Kind, "turn",
                        StringComparison.Ordinal))
                {
                    StartProductionMotionTurn();
                    _motionPhase = 3;
                    return;
                }
                if (string.Equals(_motionSpec.Kind, "attack",
                        StringComparison.Ordinal))
                {
                    if (!_motionAttackTargetPrepared)
                    {
                        PrepareProductionMotionAttackTarget();
                        _settleUpdates = 0;
                        return;
                    }
                    CaptureProductionMotionRecord(_motionSpec.Label +
                        "-ready", 0,
                        "target-aligned combat-ready frame before native UnitAttack",
                        model, role, false);
                    StartProductionMotionAttack();
                    _motionPhase = 3;
                    return;
                }
                if (string.Equals(_motionSpec.Kind, "reload",
                        StringComparison.Ordinal))
                {
                    CaptureProductionMotionRecord(_motionSpec.Label +
                        "-ready", 0,
                        "live held frame before production Reload Firearm UnitUseAbility",
                        model, role, false);
                    StartProductionMotionReload();
                    _motionPhase = 3;
                    return;
                }
                throw new InvalidOperationException(
                    "Unknown production motion kind " +
                    _motionSpec.Kind + ".");
            }

            private void TickProductionMotionRuntime()
            {
                Game.Instance.EntityCreator.Tick();
                if (_actor != null && _actor.View != null &&
                    _actor.View.AnimationManager != null)
                    _actor.View.AnimationManager.Tick();
                if (_actor != null && _actor.View != null &&
                    _actor.View.HandsEquipment != null)
                    _actor.View.HandsEquipment.UpdateAll();
                if (!ProductionMotionPlayerBoundaryExact())
                    throw new InvalidOperationException(_stage +
                        " changed the working-save combat boundary; " +
                        ProductionMotionPlayerBoundaryDescription() + ".");
            }

            private static void SetProductionMotionUnitPosition(
                UnitEntityData unit, Vector3 position)
            {
                if (unit == null) throw new ArgumentNullException("unit");
                unit.Position = position;
                if (unit.View != null)
                    unit.View.transform.position = position;
            }

            private void StartProductionMotionMovement()
            {
                _stage = "start-" + _motionSpec.Label;
                if (AstarPath.active == null ||
                    _actor.CombatState.IsInCombat ||
                    Game.Instance.Player.IsInCombat ||
                    TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat())
                    throw new InvalidOperationException(_motionSpec.Label +
                        " cannot start without a live navmesh and clean combat state;" +
                        "navmesh=" + (AstarPath.active != null) +
                        ";actorInCombat=" +
                        _actor.CombatState.IsInCombat +
                        ";playerInCombat=" +
                        Game.Instance.Player.IsInCombat +
                        ";turnBasedCombat=" +
                        TurnBased.Controllers.CombatController
                            .IsInTurnBasedCombat() + ".");
                Pathfinding.NNInfo start = AstarPath.active.GetNearest(
                    _actor.Position);
                if (start.node == null || !start.node.Walkable)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " has no walkable start node.");
                _motionMovementStart = start.clampedPosition;
                SetProductionMotionUnitPosition(_actor,
                    _motionMovementStart);
                _motionMovementStartArea = start.node.Area;
                _motionMovementGraphIndex = start.node.GraphIndex;
                Vector3[] offsets =
                {
                    new Vector3(5f, 0f, 5f),
                    new Vector3(-5f, 0f, 5f),
                    new Vector3(5f, 0f, -5f),
                    new Vector3(-5f, 0f, -5f)
                };
                Pathfinding.NNInfo[] candidates = offsets.Select(offset =>
                        AstarPath.active.GetNearest(
                            _motionMovementStart + offset))
                    .Where(value => value.node != null &&
                        value.node.Walkable &&
                        value.node.Area == _motionMovementStartArea &&
                        value.node.GraphIndex == _motionMovementGraphIndex)
                    .OrderByDescending(value => Vector3.Distance(
                        value.clampedPosition, _motionMovementStart))
                    .ToArray();
                if (candidates.Length == 0)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " has no same-area movement destination.");
                _motionMovementDestination = candidates[0].clampedPosition;
                _motionMovementDestinationArea = candidates[0].node.Area;
                if (Vector3.Distance(_motionMovementStart,
                        _motionMovementDestination) < 2f)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " has an insufficient native movement span.");

                _actor.View.MovementAgent.MaxSpeedOverride =
                    _motionOriginalMaxSpeedOverride;
                float baseSpeed = _actor.View.MovementAgent.MaxSpeed;
                if (baseSpeed <= 0f)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " reported a non-positive native maximum speed.");
                bool slow = string.Equals(_motionSpec.Kind,
                    "movement-slow", StringComparison.Ordinal);
                _motionMovementRequestedSpeed = slow
                    ? Mathf.Max(0.5f, baseSpeed * 0.4f) : baseSpeed;
                _actor.View.MovementAgent.MaxSpeedOverride =
                    _motionMovementRequestedSpeed;
                _actor.View.AnimationManager.WalkSpeedType = slow
                    ? UnitAnimationActionLocoMotion.WalkSpeedType.Slow
                    : UnitAnimationActionLocoMotion.WalkSpeedType.Normal;
                _motionMoveCommand = new UnitMoveTo(
                    _motionMovementDestination);
                _actor.Commands.Run(_motionMoveCommand);
                _motionMovementCommandAccepted =
                    _actor.Commands.Contains(_motionMoveCommand) &&
                    ReferenceEquals(_motionMoveCommand.Executor, _actor);
                if (!_motionMovementCommandAccepted)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " native UnitMoveTo was not accepted.");
                var path = new Pathfinding.ForcedPath(new List<Vector3>
                {
                    _motionMovementStart,
                    _motionMovementDestination
                });
                path.UserTag = "KMG production outfit motion " +
                    _motionSpec.Label;
                _actor.View.AgentASP.ForcePath(path, 0.1f);
                if (!_actor.View.MovementAgent.WantsToMove)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " native movement agent rejected its path.");
                _settleUpdates = 0;
            }

            private bool IsProductionMotionActorTurn()
            {
                TurnBased.Controllers.CombatController controller =
                    Game.Instance.TurnBasedCombatController;
                return controller != null && controller.CurrentTurn != null &&
                    ReferenceEquals(controller.CurrentTurn.Unit, _actor);
            }

            private void PollProductionMotionMovement()
            {
                _stage = "execute-" + _motionSpec.Label;
                TickProductionMotionRuntime();
                if (!TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat() || IsProductionMotionActorTurn())
                    _actor.View.MovementAgent.TickMovement(
                        Game.Instance.TimeController.DeltaTime);
                _settleUpdates++;
                float velocity = _actor.View.MovementAgent.Velocity.magnitude;
                _motionMovementMaximumVelocity = Mathf.Max(
                    _motionMovementMaximumVelocity, velocity);
                _motionMovementObserved |= _actor.View.IsMoving() ||
                    _actor.View.MovementAgent.IsReallyMoving ||
                    _actor.View.MovementAgent.WantsToMove;
                _motionVelocityObserved |= velocity > 0.01f;
                _motionMovementDistance = Vector3.Distance(
                    _motionMovementStart, _actor.Position);
                bool run = string.Equals(_motionSpec.Kind, "movement-run",
                    StringComparison.Ordinal);
                bool speedDistinct = !run || _motionWalkMaximumVelocity <= 0f ||
                    _motionMovementMaximumVelocity >
                        _motionWalkMaximumVelocity * 1.2f;
                string role;
                Transform model = WeaponPresentationEvidenceScenario
                    .ResolveActivePresentation(_actor,
                        _motionWeapon.Blueprint.VisualParameters,
                        "held-idle", out role);
                if (_motionMovementObserved && _motionVelocityObserved &&
                    _motionMovementDistance >= 0.75f && speedDistinct &&
                    Renderable(model) && ProductionMotionOutfitExact())
                {
                    CaptureProductionMotionRecord(_motionSpec.Label,
                        _settleUpdates,
                        "live UnitMoveTo plus ForcedPath frame with nonzero velocity and measurable displacement",
                        model, role, false);
                    if (run)
                        _motionRunMaximumVelocity =
                            _motionMovementMaximumVelocity;
                    else
                        _motionWalkMaximumVelocity =
                            _motionMovementMaximumVelocity;
                    _motionMovementOutcomes.Add(new JObject
                    {
                        { "fixture", _fixtures[_fixtureIndex].Label },
                        { "action", _motionSpec.Label },
                        { "walkSpeedType",
                            _actor.View.AnimationManager.WalkSpeedType
                                .ToString() },
                        { "requestedMaxSpeed",
                            _motionMovementRequestedSpeed },
                        { "maximumObservedVelocity",
                            _motionMovementMaximumVelocity },
                        { "distanceMeters", _motionMovementDistance },
                        { "commandAccepted",
                            _motionMovementCommandAccepted },
                        { "movingObserved", _motionMovementObserved },
                        { "velocityObserved", _motionVelocityObserved },
                        { "startArea", _motionMovementStartArea },
                        { "destinationArea",
                            _motionMovementDestinationArea },
                        { "graphIndex", _motionMovementGraphIndex }
                    });
                    _actor.View.StopMoving();
                    _actor.Commands.InterruptAll(true);
                    _motionPhase = 4;
                    return;
                }
                if (_settleUpdates < MotionMaximumUpdates) return;
                throw new InvalidOperationException(_motionSpec.Label +
                    " did not expose a distinct live movement frame; distance=" +
                    _motionMovementDistance.ToString("R") +
                    ";maxVelocity=" +
                    _motionMovementMaximumVelocity.ToString("R") +
                    ";walkMax=" +
                    _motionWalkMaximumVelocity.ToString("R") +
                    ";requested=" +
                    _motionMovementRequestedSpeed.ToString("R") + ".");
            }

            private void StartProductionMotionTurn()
            {
                _motionTurnStartForward = _actor.OrientationDirection;
                _motionTurnStartForward.y = 0f;
                if (_motionTurnStartForward.sqrMagnitude < 0.5f)
                    _motionTurnStartForward = Vector3.forward;
                _motionTurnStartForward.Normalize();
                Vector3 right = new Vector3(_motionTurnStartForward.z, 0f,
                    -_motionTurnStartForward.x).normalized;
                _actor.ForceLookAt(_actor.Position + right * 5f);
                _settleUpdates = 0;
            }

            private void PollProductionMotionTurn()
            {
                _stage = "execute-" + _motionSpec.Label;
                TickProductionMotionRuntime();
                _settleUpdates++;
                Vector3 current = _actor.OrientationDirection;
                current.y = 0f;
                if (current.sqrMagnitude > 0.01f) current.Normalize();
                _motionTurnDegrees = Vector3.Angle(
                    _motionTurnStartForward, current);
                string role;
                Transform model = WeaponPresentationEvidenceScenario
                    .ResolveActivePresentation(_actor,
                        _motionWeapon.Blueprint.VisualParameters,
                        "held-idle", out role);
                if (_motionTurnDegrees >= 60f && _settleUpdates >= 4 &&
                    Renderable(model) && ProductionMotionOutfitExact())
                {
                    CaptureProductionMotionRecord(_motionSpec.Label,
                        _settleUpdates,
                        "native ForceLookAt endpoint after a body-relative right turn",
                        model, role, false);
                    _motionTurnOutcomes.Add(new JObject
                    {
                        { "fixture", _fixtures[_fixtureIndex].Label },
                        { "action", _motionSpec.Label },
                        { "turnDegrees", _motionTurnDegrees },
                        { "startForward",
                            _motionTurnStartForward.ToString("R") },
                        { "endForward", current.ToString("R") }
                    });
                    _motionPhase = 4;
                    return;
                }
                if (_settleUpdates < MotionMaximumUpdates) return;
                throw new InvalidOperationException(_motionSpec.Label +
                    " did not reach the requested right turn; degrees=" +
                    _motionTurnDegrees.ToString("R") + ".");
            }

            private void PrepareProductionMotionAttackTarget()
            {
                _stage = "align-target-" + _motionSpec.Label;
                UnitCommand issued = UnitAttack.CreateAttackCommand(_actor,
                    _motionTarget);
                _motionAttackCommand = issued as UnitAttack;
                if (_motionAttackCommand == null)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " did not create a native UnitAttack probe.");
                _motionAttackCommand.IsSingleAttack = true;
                // Init is the native planning boundary used by UnitCommands.Run,
                // but it does not register or advance the command. A live probe
                // can act while the visual rig settles and consume the firearm
                // round reserved for the separately constructed evidence command.
                _motionAttackCommand.Init(_actor);
                _motionAttackProbeDetached =
                    !_actor.Commands.Contains(_motionAttackCommand);
                if (!_motionAttackProbeDetached)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " registered its readiness-only UnitAttack probe.");
                Vector3 forward = _actor.OrientationDirection;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.5f) forward = Vector3.forward;
                forward.Normalize();
                Vector3 right = new Vector3(forward.z, 0f, -forward.x);
                Vector3[] directions =
                {
                    forward, right, -right, -forward,
                    (forward + right).normalized,
                    (forward - right).normalized,
                    (-forward + right).normalized,
                    (-forward - right).normalized
                };
                float[] distances = { 6f, 4f, 2f, 1f, 0.5f };
                var attempts = new List<string>();
                _motionTargetAttempts = 0;
                foreach (float distance in distances)
                    for (int directionIndex = 0;
                        directionIndex < directions.Length; directionIndex++)
                    {
                        Vector3 candidate = NearestNavigable(
                            _actor.Position +
                            directions[directionIndex] * distance);
                        SetProductionMotionUnitPosition(_motionTarget,
                            candidate);
                        _actor.ForceLookAt(candidate);
                        _motionTarget.ForceLookAt(_actor.Position);
                        Game.Instance.EntityCreator.Tick();
                        _motionTargetAttempts++;
                        bool targetInState = _motionTarget.IsInState;
                        bool canStart = _motionAttackCommand.CanStart;
                        bool closeEnough =
                            _motionAttackCommand.IsUnitEnoughClose;
                        float actualDistance = Vector3.Distance(
                            _actor.Position, _motionTarget.Position);
                        string placement = "distance-" +
                            distance.ToString("R") + "-direction-" +
                            directionIndex;
                        attempts.Add(placement + ":state=" +
                            targetInState + ":start=" + canStart +
                            ":close=" + closeEnough);
                        if (!targetInState || !canStart || !closeEnough)
                            continue;
                        _motionCommandTargetInState = targetInState;
                        _motionCommandCanStart = canStart;
                        _motionCommandCloseEnough = closeEnough;
                        _motionNeedLineOfSight =
                            _motionAttackCommand.NeedLoS;
                        _motionApproachRadius =
                            _motionAttackCommand.ApproachRadius;
                        _motionTargetDistance = actualDistance;
                        _motionTargetPlacement = placement;
                        _motionAttackTargetPrepared = true;
                        _diagnostics.Add(_motionSpec.Label +
                            ":target=" + placement + ";distance=" +
                            actualDistance.ToString("R") +
                            ";attempts=" + _motionTargetAttempts +
                            ";probeDetached=True");
                        if (_actor.Commands.Contains(_motionAttackCommand))
                            throw new InvalidOperationException(
                                _motionSpec.Label +
                                " readiness probe became live while positioning its target.");
                        _actor.View.HandsEquipment
                            .OnCombatStateChanged(true);
                        _actor.View.HandsEquipment
                            .MatchWithCurrentCombatState();
                        _motionAttackCommand = null;
                        return;
                    }
                throw new InvalidOperationException(_motionSpec.Label +
                    " had no navmesh-backed native UnitAttack start position: " +
                    string.Join("|", attempts.ToArray()) + ".");
            }

            private void StartProductionMotionAttack()
            {
                _stage = "start-native-attack-" + _motionSpec.Label;
                if (!_motionAttackProbeDetached)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " did not retain a detached readiness probe boundary.");
                _motionFiredBefore =
                    FirearmDischargeRuntimeDiagnostics.Fired;
                _motionDischargeFaultsBefore =
                    FirearmDischargeRuntimeDiagnostics.Faults;
                UnitCommand issued = UnitAttack.CreateAttackCommand(_actor,
                    _motionTarget);
                _motionAttackCommand = issued as UnitAttack;
                if (_motionAttackCommand == null)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " did not reproduce a native UnitAttack.");
                _motionAttackCommand.IsSingleAttack = true;
                _actor.Commands.Run(_motionAttackCommand);
                _motionCommandInstalled =
                    _actor.Commands.Contains(_motionAttackCommand);
                _motionCommandCanStart = _motionAttackCommand.CanStart;
                _motionCommandCloseEnough =
                    _motionAttackCommand.IsUnitEnoughClose;
                if (!_motionCommandInstalled || !_motionCommandCanStart ||
                    !_motionCommandCloseEnough)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " lost native UnitAttack readiness; installed=" +
                        _motionCommandInstalled + ";canStart=" +
                        _motionCommandCanStart + ";close=" +
                        _motionCommandCloseEnough + ".");
                _motionAttackCommand.Start();
                ObserveProductionMotionAttack();
                if (!_motionCommandStarted ||
                    !_motionCommandRunningObserved)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " native UnitAttack did not enter running state.");
                _motionActionUpdates = 0;
                _motionCaptureScheduleIndex = 0;
            }

            private void ObserveProductionMotionAttack()
            {
                if (_motionAttackCommand == null) return;
                _motionCommandInstalled |=
                    _actor.Commands.Contains(_motionAttackCommand);
                _motionCommandStarted |= _motionAttackCommand.IsStarted;
                _motionCommandRunningObserved |=
                    _motionAttackCommand.IsRunning;
                _motionAnimationObserved |=
                    _motionAttackCommand.Animation != null;
                _motionAnimationActedObserved |=
                    _motionAttackCommand.Animation != null &&
                    _motionAttackCommand.Animation.IsActed;
            }

            private void PollProductionMotionAttack()
            {
                _stage = "sample-native-attack-" + _motionSpec.Label;
                TickProductionMotionRuntime();
                _motionTarget.Commands.InterruptAll(true);
                ObserveProductionMotionAttack();
                if (_motionAttackCommand.IsRunning &&
                    _motionAttackCommand.Animation != null &&
                    _motionAttackCommand.Animation.IsActed &&
                    _motionAttackCommand.Result ==
                        UnitCommand.ResultType.None)
                {
                    _motionAttackCommand.Tick();
                    _motionExplicitCommandTicks++;
                }
                ObserveProductionMotionAttack();
                _motionActionUpdates++;

                if (_motionAnimationActedObserved &&
                    !_motionActedCaptureTaken)
                {
                    string actedRole;
                    Transform actedModel = WeaponPresentationEvidenceScenario
                        .ResolveActivePresentation(_actor,
                            _motionWeapon.Blueprint.VisualParameters,
                            "attack", out actedRole);
                    if (!Renderable(actedModel))
                        throw new InvalidOperationException(
                            _motionSpec.Label +
                            " lost its held model at the acted frame.");
                    CaptureProductionMotionRecord(_motionSpec.Label +
                        "-acted-update-" +
                        _motionActionUpdates.ToString("000"),
                        _motionActionUpdates,
                        "event-aligned acted frame from the live native UnitAttack",
                        actedModel, actedRole, false);
                    _motionActedCaptureTaken = true;
                }

                if (_motionCaptureScheduleIndex <
                        ProductionMotionAttackUpdates.Length &&
                    _motionActionUpdates >=
                        ProductionMotionAttackUpdates[
                            _motionCaptureScheduleIndex])
                {
                    string role;
                    Transform model = WeaponPresentationEvidenceScenario
                        .ResolveActivePresentation(_actor,
                            _motionWeapon.Blueprint.VisualParameters,
                            "attack", out role);
                    if (!Renderable(model))
                        throw new InvalidOperationException(
                            _motionSpec.Label +
                            " lost its held model during update " +
                            _motionActionUpdates + ".");
                    CaptureProductionMotionRecord(_motionSpec.Label +
                        "-update-" +
                        _motionActionUpdates.ToString("000"),
                        _motionActionUpdates,
                        "fixed live native UnitAttack animation sample; counters establish firearm discharge",
                        model, role, false);
                    _motionCaptureScheduleIndex++;
                }

                bool firearm = !string.Equals(_motionSpec.Weapon,
                    "shortsword", StringComparison.Ordinal);
                bool attackObserved = firearm
                    ? FirearmDischargeRuntimeDiagnostics.Fired -
                        _motionFiredBefore >= 1
                    : _motionCommandRunningObserved &&
                        _motionAnimationObserved;
                bool evidenceComplete = _motionCaptureScheduleIndex ==
                        ProductionMotionAttackUpdates.Length &&
                    _motionActedCaptureTaken && attackObserved;
                bool commandRunning = _motionAttackCommand != null &&
                    _motionAttackCommand.IsRunning;
                bool commandInterruptible = _motionAttackCommand == null ||
                    _motionAttackCommand.IsInterruptible;
                _motionAttackRetirementReady = !commandRunning ||
                    commandInterruptible;
                bool complete = evidenceComplete &&
                    _motionAttackRetirementReady;
                if (!complete)
                {
                    if (_motionActionUpdates < MotionMaximumUpdates) return;
                    throw new InvalidOperationException(_motionSpec.Label +
                        " did not complete exact native attack evidence; " +
                        "captures=" + _motionCaptureScheduleIndex +
                        ";acted=" + _motionActedCaptureTaken +
                        ";observed=" + attackObserved +
                        ";retirementReady=" +
                        _motionAttackRetirementReady +
                        ";commandRunning=" + commandRunning +
                        ";commandInterruptible=" +
                        commandInterruptible +
                        ";updates=" + _motionActionUpdates + ".");
                }

                FirearmState state = firearm && _motionWeapon != null
                    ? FirearmRuntimeState.Service
                        .GetOrCreate(_motionWeapon).Repository.State
                    : null;
                _motionAttackOutcomes.Add(new JObject
                {
                    { "fixture", _fixtures[_fixtureIndex].Label },
                    { "action", _motionSpec.Label },
                    { "firearm", firearm },
                    { "readinessProbeDetached",
                        _motionAttackProbeDetached },
                    { "commandInstalled", _motionCommandInstalled },
                    { "commandCanStart", _motionCommandCanStart },
                    { "commandCloseEnough", _motionCommandCloseEnough },
                    { "commandTargetInState",
                        _motionCommandTargetInState },
                    { "commandStarted", _motionCommandStarted },
                    { "commandRunningObserved",
                        _motionCommandRunningObserved },
                    { "retirementReady",
                        _motionAttackRetirementReady },
                    { "commandRunningAtRetirement", commandRunning },
                    { "commandInterruptibleAtRetirement",
                        commandInterruptible },
                    { "slotEvicted", false },
                    { "residentCommandTypesAtEvidenceCompletion",
                        new JArray(
                            ProductionMotionResidentCommandTypes()) },
                    { "queuedCommandTypesAtEvidenceCompletion",
                        new JArray(
                            ProductionMotionQueuedCommandTypes()) },
                    { "actionUpdates", _motionActionUpdates },
                    { "animationObserved", _motionAnimationObserved },
                    { "animationActedObserved",
                        _motionAnimationActedObserved },
                    { "actedCaptureTaken", _motionActedCaptureTaken },
                    { "targetPlacement", _motionTargetPlacement },
                    { "targetAttempts", _motionTargetAttempts },
                    { "targetDistance", _motionTargetDistance },
                    { "approachRadius", _motionApproachRadius },
                    { "needLineOfSight", _motionNeedLineOfSight },
                    { "explicitCommandTicks",
                        _motionExplicitCommandTicks },
                    { "firedDelta",
                        FirearmDischargeRuntimeDiagnostics.Fired -
                            _motionFiredBefore },
                    { "faultDelta",
                        FirearmDischargeRuntimeDiagnostics.Faults -
                            _motionDischargeFaultsBefore },
                    { "loadedRoundsAfter",
                        state == null ? -1 : state.LoadedRounds }
                });
                string removalRole;
                _motionRemovedPresentation =
                    WeaponPresentationEvidenceScenario
                        .ResolveActivePresentation(_actor,
                            _motionWeapon.Blueprint.VisualParameters,
                            "attack", out removalRole);
                _motionPhase = 4;
            }

            private void StartProductionMotionReload()
            {
                _stage = "start-production-reload-" + _motionSpec.Label;
                ReloadTestMusketAvailability availability =
                    ReloadTestMusketRuntime.Evaluate(_actor.Descriptor,
                        BlueprintBootstrap.ProductionFirearms.Musket.Item,
                        _motionPowder, _motionBall);
                if (!availability.IsAvailable || availability.Plan == null ||
                    !ReferenceEquals(availability.Weapon, _motionWeapon))
                    throw new InvalidOperationException(_motionSpec.Label +
                        " production reload did not resolve the equipped musket: " +
                        availability + ".");
                _motionExpectedReloadAction = availability.Plan.Action;
                _motionPlannedRounds = availability.Plan.RoundsLoadable;
                _motionAbilityData = new AbilityData(_motionReloadAbility);
                var target = new TargetWrapper(_actor);
                _motionAbilityAvailable = _motionAbilityData.IsAvailable;
                _motionAbilityTargetable =
                    _motionAbilityData.CanTarget(target);
                _motionRuntimeActionType =
                    _motionAbilityData.RuntimeActionType;
                _motionRequireFullRoundAction =
                    _motionAbilityData.RequireFullRoundAction;
                _motionReloadCommand = new UnitUseAbility(
                    _motionAbilityData, target);
                _motionCommandCanStart = _motionReloadCommand.CanStart;
                if (!_motionAbilityAvailable ||
                    !_motionAbilityTargetable ||
                    !_motionCommandCanStart)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " production reload was not ready; available=" +
                        _motionAbilityAvailable + ";targetable=" +
                        _motionAbilityTargetable + ";canStart=" +
                        _motionCommandCanStart + ".");

                _motionReloadAttemptsBefore =
                    ReloadRuntimeDiagnostics.Attempts;
                _motionReloadLoadedBefore = ReloadRuntimeDiagnostics.Loaded;
                _motionReloadRejectedBefore =
                    ReloadRuntimeDiagnostics.Rejected;
                _motionReloadFaultsBefore = ReloadRuntimeDiagnostics.Faults;
                _motionFiredBefore =
                    FirearmDischargeRuntimeDiagnostics.Fired;
                _motionDischargeFaultsBefore =
                    FirearmDischargeRuntimeDiagnostics.Faults;
                _motionPowderBeforeCommand = _motionPlayer.Inventory.Count(
                    _motionPowder);
                _motionBallBeforeCommand = _motionPlayer.Inventory.Count(
                    _motionBall);
                _motionReloadCommand.IgnoreCooldown(TimeSpan.Zero);
                _actor.Commands.Run(_motionReloadCommand);
                _motionReloadCommand.Start();
                ObserveProductionMotionReload();
                if (!_motionCommandStarted ||
                    !_motionCommandRunningObserved)
                    throw new InvalidOperationException(_motionSpec.Label +
                        " native UnitUseAbility did not enter running state.");
                _motionActionUpdates = 0;
                _motionCaptureScheduleIndex = 0;
            }

            private void ObserveProductionMotionReload()
            {
                if (_motionReloadCommand == null) return;
                _motionCommandInstalled |=
                    _actor.Commands.Contains(_motionReloadCommand);
                _motionCommandStarted |= _motionReloadCommand.IsStarted;
                _motionCommandRunningObserved |=
                    _motionReloadCommand.IsRunning;
                _motionAnimationObserved |=
                    _motionReloadCommand.Animation != null;
                _motionAnimationActedObserved |=
                    _motionReloadCommand.Animation != null &&
                    _motionReloadCommand.Animation.IsActed;
                _motionExecutionProcessObserved |=
                    _motionReloadCommand.ExecutionProcess != null;
                _motionExecutionProcessEndedObserved |=
                    _motionReloadCommand.ExecutionProcess != null &&
                    _motionReloadCommand.ExecutionProcess.IsEnded;
            }

            private Transform ResolveProductionMotionReloadModel(
                out string role)
            {
                Transform model = WeaponPresentationEvidenceScenario
                    .ResolveActivePresentation(_actor,
                        _motionWeapon.Blueprint.VisualParameters,
                        "reload", out role);
                if (Renderable(model)) return model;
                return WeaponPresentationEvidenceScenario
                    .ResolveActivePresentation(_actor,
                        _motionWeapon.Blueprint.VisualParameters,
                        "stored", out role);
            }

            private void PollProductionMotionReload()
            {
                _stage = "sample-production-reload-" + _motionSpec.Label;
                TickProductionMotionRuntime();
                ObserveProductionMotionReload();
                if (_motionReloadCommand.IsRunning &&
                    _motionReloadCommand.Animation != null &&
                    _motionReloadCommand.Animation.IsActed &&
                    _motionReloadCommand.Result ==
                        UnitCommand.ResultType.None)
                {
                    _motionReloadCommand.Tick();
                    _motionExplicitCommandTicks++;
                }
                if (_motionReloadCommand.ExecutionProcess != null &&
                    !_motionReloadCommand.ExecutionProcess.IsEnded)
                {
                    _motionReloadCommand.ExecutionProcess.Tick();
                    _motionExplicitProcessTicks++;
                }
                ObserveProductionMotionReload();
                _motionActionUpdates++;

                if (_motionAnimationActedObserved &&
                    !_motionActedCaptureTaken)
                {
                    string actedRole;
                    Transform actedModel =
                        ResolveProductionMotionReloadModel(out actedRole);
                    if (!Renderable(actedModel))
                        throw new InvalidOperationException(
                            _motionSpec.Label +
                            " lost its musket at the acted reload frame.");
                    CaptureProductionMotionRecord(_motionSpec.Label +
                        "-acted-update-" +
                        _motionActionUpdates.ToString("000"),
                        _motionActionUpdates,
                        "event-aligned acted frame from production Reload Firearm",
                        actedModel, actedRole, false);
                    _motionActedCaptureTaken = true;
                }

                if (_motionCaptureScheduleIndex <
                        ProductionMotionReloadUpdates.Length &&
                    _motionActionUpdates >=
                        ProductionMotionReloadUpdates[
                            _motionCaptureScheduleIndex])
                {
                    string role;
                    Transform model =
                        ResolveProductionMotionReloadModel(out role);
                    if (!Renderable(model))
                        throw new InvalidOperationException(
                            _motionSpec.Label +
                            " lost its musket during reload update " +
                            _motionActionUpdates + ".");
                    CaptureProductionMotionRecord(_motionSpec.Label +
                        "-update-" +
                        _motionActionUpdates.ToString("000"),
                        _motionActionUpdates,
                        "fixed live production UnitUseAbility reload sample",
                        model, role, false);
                    _motionCaptureScheduleIndex++;
                }

                bool deliveryObserved = ReloadRuntimeDiagnostics.Attempts -
                    _motionReloadAttemptsBefore >= 1;
                bool complete = _motionCaptureScheduleIndex ==
                        ProductionMotionReloadUpdates.Length &&
                    _motionActedCaptureTaken && deliveryObserved;
                if (!complete)
                {
                    if (_motionActionUpdates < MotionMaximumUpdates) return;
                    throw new InvalidOperationException(_motionSpec.Label +
                        " did not complete production reload evidence; captures=" +
                        _motionCaptureScheduleIndex + ";acted=" +
                        _motionActedCaptureTaken + ";delivery=" +
                        deliveryObserved + ".");
                }

                FirearmState state = FirearmRuntimeState.Service
                    .GetOrCreate(_motionWeapon).Repository.State;
                int powderConsumed = _motionPowderBeforeCommand -
                    _motionPlayer.Inventory.Count(_motionPowder);
                int ballConsumed = _motionBallBeforeCommand -
                    _motionPlayer.Inventory.Count(_motionBall);
                _motionReloadOutcomes.Add(new JObject
                {
                    { "fixture", _fixtures[_fixtureIndex].Label },
                    { "action", _motionSpec.Label },
                    { "expectedReloadAction",
                        _motionExpectedReloadAction.ToString() },
                    { "plannedRounds", _motionPlannedRounds },
                    { "abilityAvailable", _motionAbilityAvailable },
                    { "abilityTargetable", _motionAbilityTargetable },
                    { "runtimeActionType",
                        _motionRuntimeActionType.ToString() },
                    { "requireFullRoundAction",
                        _motionRequireFullRoundAction },
                    { "commandCanStart", _motionCommandCanStart },
                    { "commandInstalled", _motionCommandInstalled },
                    { "commandStarted", _motionCommandStarted },
                    { "commandRunningObserved",
                        _motionCommandRunningObserved },
                    { "animationObserved", _motionAnimationObserved },
                    { "animationActedObserved",
                        _motionAnimationActedObserved },
                    { "actedCaptureTaken", _motionActedCaptureTaken },
                    { "executionProcessObserved",
                        _motionExecutionProcessObserved },
                    { "executionProcessEndedObserved",
                        _motionExecutionProcessEndedObserved },
                    { "explicitCommandTicks",
                        _motionExplicitCommandTicks },
                    { "explicitProcessTicks",
                        _motionExplicitProcessTicks },
                    { "attemptsDelta", ReloadRuntimeDiagnostics.Attempts -
                        _motionReloadAttemptsBefore },
                    { "loadedDelta", ReloadRuntimeDiagnostics.Loaded -
                        _motionReloadLoadedBefore },
                    { "rejectedDelta", ReloadRuntimeDiagnostics.Rejected -
                        _motionReloadRejectedBefore },
                    { "faultDelta", ReloadRuntimeDiagnostics.Faults -
                        _motionReloadFaultsBefore },
                    { "loadedRoundsAfter", state.LoadedRounds },
                    { "powderConsumed", powderConsumed },
                    { "ballsConsumed", ballConsumed },
                    { "firedDelta",
                        FirearmDischargeRuntimeDiagnostics.Fired -
                            _motionFiredBefore },
                    { "dischargeFaultDelta",
                        FirearmDischargeRuntimeDiagnostics.Faults -
                            _motionDischargeFaultsBefore },
                    { "commandResult",
                        _motionReloadCommand.Result.ToString() }
                });
                string removalRole;
                _motionRemovedPresentation =
                    ResolveProductionMotionReloadModel(out removalRole);
                _motionPhase = 4;
            }

            private void CaptureProductionMotionRecord(string state,
                int update, string claim, Transform model, string role,
                bool allowBodyOnly)
            {
                _stage = "capture-" +
                    _fixtures[_fixtureIndex].Label + "-" + state;
                if (!ProductionMotionOutfitExact())
                    throw new InvalidOperationException(state +
                        " cannot be captured because the production outfit contract changed.");
                if (!ProductionMotionPlayerBoundaryExact())
                    throw new InvalidOperationException(state +
                        " cannot be captured because the working-save combat boundary changed; " +
                        ProductionMotionPlayerBoundaryDescription() + ".");
                string stem = SafeFileName("production-motion-" +
                    _fixtures[_fixtureIndex].Label + "-" + state);
                string pngPath = Path.Combine(_request.EvidenceDirectory,
                    stem + ".png");
                string jsonPath = Path.Combine(_request.EvidenceDirectory,
                    stem + ".json");
                WeaponPresentationEvidenceScenario.CaptureSummary capture =
                    WeaponPresentationEvidenceScenario.CaptureContactSheet(
                        _actor, model, _motionBodyRenderers, pngPath,
                        allowBodyOnly);
                FirearmState firearmState = _motionFirearmStateSet &&
                    _motionWeapon != null
                    ? FirearmRuntimeState.Service.GetOrCreate(_motionWeapon)
                        .Repository.State
                    : null;
                float? maxSpeedOverride = _actor.View.MovementAgent == null
                    ? (float?)null
                    : _actor.View.MovementAgent.MaxSpeedOverride;
                var record = new JObject
                {
                    { "schemaVersion", 1 },
                    { "scenario", _request.Scenario },
                    { "fixture", _fixtures[_fixtureIndex].Label },
                    { "gender",
                        _fixtures[_fixtureIndex].Gender.ToString() },
                    { "raceId",
                        _fixtures[_fixtureIndex].Race.RaceId.ToString() },
                    { "action", _motionSpec.Label },
                    { "kind", _motionSpec.Kind },
                    { "state", state },
                    { "update", update },
                    { "claimBoundary", claim },
                    { "previousActionCleared",
                        _motionPreviousActionCleared },
                    { "attackReadinessProbeDetached",
                        !string.Equals(_motionSpec.Kind, "attack",
                            StringComparison.Ordinal) ||
                        _motionAttackProbeDetached },
                    { "productionClassGuid", _gunslingerClass.AssetGuid },
                    { "productionAssetIds", new JArray(
                        CurrentProductionAssetIds()) },
                    { "productionEntitiesPresent",
                        ProductionEntitiesPresent() },
                    { "productionRamps", ProductionRampEvidence() },
                    { "selectedHairAssetId", _hairAssetId },
                    { "hairEntityPreserved",
                        _avatar.EquipmentEntities.Any(value =>
                            ReferenceEquals(value, _hairEntity)) },
                    { "savedLinksUnchanged",
                        _baseSavedLinks.SequenceEqual(
                            ProductionSavedLinks(_avatar),
                            StringComparer.Ordinal) },
                    { "humanoidRigExact",
                        HasExactHumanoidRig(_actor.View.transform) },
                    { "activeRendererCount",
                        ActiveRenderers(_actor).Length },
                    { "actorIsPlayerFaction", _actor.IsPlayerFaction },
                    { "actorSharesPlayerGroup",
                        ReferenceEquals(_actor.Group, _motionPlayer.Group) },
                    { "actorHoldingStateIsRequestLocalLoadedScene",
                        ReferenceEquals(
                        _actor.HoldingState, _motionSceneState) },
                    { "requestLocalSceneMatchesLoadedScene",
                        _motionLoadedSceneState != null &&
                        string.Equals(_motionSceneState.SceneName,
                            _motionLoadedSceneState.SceneName,
                            StringComparison.Ordinal) &&
                        _motionSceneState.IsSceneLoaded },
                    { "actorInControllableCharacters", ContainsReference(
                        _motionPlayer.ControllableCharacters, _actor) },
                    { "targetPresent", _motionTarget != null },
                    { "targetIsPlayerFaction", _motionTarget != null &&
                        _motionTarget.IsPlayerFaction },
                    { "targetSharesPlayerGroup", _motionTarget != null &&
                        ReferenceEquals(_motionTarget.Group,
                            _motionPlayer.Group) },
                    { "targetHoldingStateIsRequestLocalLoadedScene",
                        _motionTarget != null && ReferenceEquals(
                            _motionTarget.HoldingState, _motionSceneState) },
                    { "targetInControllableCharacters",
                        _motionTarget != null && ContainsReference(
                            _motionPlayer.ControllableCharacters,
                            _motionTarget) },
                    { "actorTargetBilateralEnemy", _motionTarget != null &&
                        _actor.IsEnemy(_motionTarget) &&
                        _motionTarget.IsEnemy(_actor) },
                    { "actorPlayerHostility", _actor.IsEnemy(_anchor) ||
                        _anchor.IsEnemy(_actor) },
                    { "targetPlayerHostility", _motionTarget != null &&
                        (_motionTarget.IsEnemy(_anchor) ||
                            _anchor.IsEnemy(_motionTarget)) },
                    { "playerBoundaryExact",
                        ProductionMotionPlayerBoundaryExact() },
                    { "playerCharacterListsExact",
                        ProductionMotionPlayerListsExact() },
                    { "weaponKind", _motionSpec.Weapon },
                    { "itemGuid", _motionWeapon == null ||
                        _motionWeapon.Blueprint == null ? "<none>" :
                        _motionWeapon.Blueprint.AssetGuid },
                    { "itemName", _motionWeapon == null ||
                        _motionWeapon.Blueprint == null ? "<none>" :
                        _motionWeapon.Blueprint.name },
                    { "primaryHandExact", _motionWeapon == null
                        ? _actor.Body.PrimaryHand.MaybeItem == null
                        : ReferenceEquals(
                            _actor.Body.PrimaryHand.MaybeWeapon,
                            _motionWeapon) },
                    { "handsInCombat",
                        _actor.View.HandsEquipment.InCombat },
                    { "weaponPresentationRole", role },
                    { "weaponModelRenderable", Renderable(model) },
                    { "weaponModelPath",
                        ProductionMotionTransformPath(model,
                            _actor.View.transform) },
                    { "loadedRounds", firearmState == null ? -1 :
                        firearmState.LoadedRounds },
                    { "loadedAmmunition", firearmState == null ||
                        firearmState.LoadedAmmunition == null
                            ? JValue.CreateNull()
                            : (JToken)firearmState.LoadedAmmunition
                                .ToString() },
                    { "unitPosition", _actor.Position.ToString("R") },
                    { "unitOrientation",
                        _actor.OrientationDirection.ToString("R") },
                    { "targetPosition", _motionTarget == null ? "<none>" :
                        _motionTarget.Position.ToString("R") },
                    { "targetDistance", _motionTarget == null ? -1f :
                        Vector3.Distance(_actor.Position,
                            _motionTarget.Position) }
                };

                record["movementStart"] =
                    _motionMovementStart.ToString("R");
                record["movementDestination"] =
                    _motionMovementDestination.ToString("R");
                record["movementDistanceMeters"] =
                    _motionMovementDistance;
                record["movementRequestedMaxSpeed"] =
                    _motionMovementRequestedSpeed;
                record["movementMaximumVelocity"] =
                    _motionMovementMaximumVelocity;
                record["movementVelocity"] =
                    _actor.View.MovementAgent == null ? "<none>" :
                    _actor.View.MovementAgent.Velocity.ToString("R");
                record["movementWantsToMove"] =
                    _actor.View.MovementAgent != null &&
                    _actor.View.MovementAgent.WantsToMove;
                record["movementIsReallyMoving"] =
                    _actor.View.MovementAgent != null &&
                    _actor.View.MovementAgent.IsReallyMoving;
                record["maxSpeedOverride"] = maxSpeedOverride.HasValue
                    ? (JToken)maxSpeedOverride.Value : JValue.CreateNull();
                record["walkSpeedType"] =
                    _actor.View.AnimationManager.WalkSpeedType.ToString();
                record["turnDegrees"] = _motionTurnDegrees;
                record["commandType"] = _motionAttackCommand != null
                    ? _motionAttackCommand.GetType().FullName
                    : _motionReloadCommand != null
                        ? _motionReloadCommand.GetType().FullName
                        : _motionMoveCommand != null
                            ? _motionMoveCommand.GetType().FullName
                            : "<none>";
                record["commandInstalled"] = _motionCommandInstalled;
                record["commandCanStart"] = _motionCommandCanStart;
                record["commandCloseEnough"] = _motionCommandCloseEnough;
                record["commandTargetInState"] =
                    _motionCommandTargetInState;
                record["commandStarted"] = _motionCommandStarted;
                record["commandRunningObserved"] =
                    _motionCommandRunningObserved;
                record["attackRetirementReady"] =
                    _motionAttackRetirementReady;
                record["commandCurrentlyRunning"] =
                    _motionAttackCommand != null &&
                    _motionAttackCommand.IsRunning;
                record["commandCurrentlyInterruptible"] =
                    _motionAttackCommand != null &&
                    _motionAttackCommand.IsInterruptible;
                record["animationObserved"] = _motionAnimationObserved;
                record["animationActedObserved"] =
                    _motionAnimationActedObserved;
                record["commandAnimationActed"] =
                    (_motionAttackCommand != null &&
                        _motionAttackCommand.Animation != null &&
                        _motionAttackCommand.Animation.IsActed) ||
                    (_motionReloadCommand != null &&
                        _motionReloadCommand.Animation != null &&
                        _motionReloadCommand.Animation.IsActed);
                record["targetPlacement"] = _motionTargetPlacement;
                record["targetAttempts"] = _motionTargetAttempts;
                record["approachRadius"] = _motionApproachRadius;
                record["needLineOfSight"] = _motionNeedLineOfSight;
                record["explicitCommandTicks"] =
                    _motionExplicitCommandTicks;
                record["explicitProcessTicks"] =
                    _motionExplicitProcessTicks;
                record["abilityAvailable"] = _motionAbilityAvailable;
                record["abilityTargetable"] = _motionAbilityTargetable;
                record["abilityRuntimeActionType"] =
                    _motionRuntimeActionType.ToString();
                record["abilityRequireFullRoundAction"] =
                    _motionRequireFullRoundAction;
                record["executionProcessObserved"] =
                    _motionExecutionProcessObserved;
                record["executionProcessEndedObserved"] =
                    _motionExecutionProcessEndedObserved;
                record["reloadAttempts"] =
                    ReloadRuntimeDiagnostics.Attempts;
                record["reloadLoaded"] = ReloadRuntimeDiagnostics.Loaded;
                record["reloadRejected"] =
                    ReloadRuntimeDiagnostics.Rejected;
                record["reloadFaults"] = ReloadRuntimeDiagnostics.Faults;
                record["firearmDischargeFired"] =
                    FirearmDischargeRuntimeDiagnostics.Fired;
                record["firearmDischargeFaults"] =
                    FirearmDischargeRuntimeDiagnostics.Faults;
                record["powderCount"] = _motionPlayer == null ? -1 :
                    _motionPlayer.Inventory.Count(_motionPowder);
                record["ballCount"] = _motionPlayer == null ? -1 :
                    _motionPlayer.Inventory.Count(_motionBall);
                record["activeCommandTypes"] = new JArray(
                    ProductionMotionResidentCommandTypes());
                record["runningCommandTypes"] = new JArray(
                    ProductionMotionRunningCommandTypes());
                record["queuedCommandTypes"] = new JArray(
                    ProductionMotionQueuedCommandTypes());
                record["productionBlueprintUnchanged"] =
                    ProductionBlueprintUnchanged();
                record["productionBlueprintMutated"] = false;
                record["saveApiCalled"] = false;
                record["preview"] = new JObject
                {
                    { "file", Path.GetFileName(capture.PngPath) },
                    { "bytes", capture.Bytes },
                    { "sha256", capture.Sha256 },
                    { "meaningfulPixels", capture.MeaningfulPixels },
                    { "framing", capture.Framing },
                    { "lowPixelDensity", capture.LowPixelDensity }
                };
                WriteJsonAtomic(jsonPath, record);
                _motionRecords.Add(record);
                _evidenceFiles.Add(capture.PngPath);
                _evidenceFiles.Add(jsonPath);
                _motionCaptured++;
                _motionViewCount += 4;
                _diagnostics.Add(_fixtures[_fixtureIndex].Label + ":" +
                    state + ":png=" + Path.GetFileName(capture.PngPath) +
                    ";sha256=" + capture.Sha256 + ";pixels=" +
                    capture.MeaningfulPixels);
                if (capture.LowPixelDensity)
                    _warnings.Add(_fixtures[_fixtureIndex].Label + "/" +
                        state + " has low foreground pixel density; retain " +
                        "it as an explicit framing diagnostic.");
            }

            private static string ProductionMotionTransformPath(
                Transform value, Transform root)
            {
                if (value == null) return "<none>";
                var names = new List<string>();
                for (Transform current = value; current != null;
                    current = current.parent)
                {
                    names.Add(current.name);
                    if (ReferenceEquals(current, root)) break;
                }
                names.Reverse();
                return string.Join("/", names.ToArray());
            }

            private string[] ProductionMotionRunningCommandTypes()
            {
                return _actor == null ? new string[0] :
                    _actor.Commands.Raw
                        .Where(value => value != null && value.IsRunning)
                        .Select(value => value.GetType().FullName).ToArray();
            }

            private string[] ProductionMotionResidentCommandTypes()
            {
                return _actor == null ? new string[0] :
                    _actor.Commands.Raw.Where(value => value != null)
                        .Select(value => value.GetType().FullName).ToArray();
            }

            private string[] ProductionMotionQueuedCommandTypes()
            {
                return _actor == null ? new string[0] :
                    _actor.Commands.Queue.Where(value => value != null)
                        .Select(value => value.GetType().FullName).ToArray();
            }

            private void BeginProductionMotionRemoval()
            {
                _stage = "remove-motion-" + _motionSpec.Label;
                if (_actor != null)
                {
                    string[] queuedBefore =
                        ProductionMotionQueuedCommandTypes();
                    if (queuedBefore.Length != 0)
                        throw new InvalidOperationException(
                            "Production motion refused teardown with queued native commands: " +
                            string.Join("|", queuedBefore) + ".");
                    _actor.Commands.InterruptAll(true);
                    _actor.Commands.RemoveFinishedAndUpdateQueue();
                    string[] runningCommands =
                        ProductionMotionRunningCommandTypes();
                    string[] residentCommands =
                        ProductionMotionResidentCommandTypes();
                    string[] queuedCommands =
                        ProductionMotionQueuedCommandTypes();
                    bool slotEvicted = _motionAttackCommand == null ||
                        !_actor.Commands.Contains(_motionAttackCommand);
                    if (_motionSpec != null && string.Equals(
                            _motionSpec.Kind, "attack",
                            StringComparison.Ordinal) &&
                        _motionAttackOutcomes.Count > 0)
                    {
                        JObject attackOutcome = _motionAttackOutcomes[
                            _motionAttackOutcomes.Count - 1] as JObject;
                        if (attackOutcome != null)
                        {
                            attackOutcome["slotEvicted"] = slotEvicted;
                            attackOutcome[
                                "residentCommandTypesAfterRetirement"] =
                                new JArray(residentCommands);
                            attackOutcome[
                                "queuedCommandTypesAfterRetirement"] =
                                new JArray(queuedCommands);
                            attackOutcome[
                                "runningCommandTypesAfterRetirement"] =
                                new JArray(runningCommands);
                        }
                    }
                    if (!slotEvicted || !_actor.Commands.Empty ||
                        runningCommands.Length != 0 ||
                        residentCommands.Length != 0 ||
                        queuedCommands.Length != 0)
                        throw new InvalidOperationException(
                            "Production motion refused teardown until its native command slot was empty; " +
                            "slotEvicted=" + slotEvicted + ";empty=" +
                            _actor.Commands.Empty + ";running=" +
                            string.Join("|", runningCommands) +
                            ";resident=" +
                            string.Join("|", residentCommands) +
                            ";queued=" + string.Join("|", queuedCommands) +
                            ".");
                    if (_actor.View != null)
                    {
                        _actor.View.StopMoving();
                        if (_actor.View.MovementAgent != null)
                            _actor.View.MovementAgent.MaxSpeedOverride =
                                _motionOriginalMaxSpeedOverride;
                        if (_actor.View.AnimationManager != null)
                        {
                            _actor.View.AnimationManager.WalkSpeedType =
                                _motionOriginalWalkSpeedType;
                            _actor.View.AnimationManager.Speed =
                                _motionOriginalAnimationSpeed;
                        }
                    }
                    if (_actor.CombatState.IsInCombat)
                        _actor.LeaveCombat();
                }
                if (_motionTarget != null)
                {
                    _motionTarget.Commands.InterruptAll(true);
                    if (_motionTarget.CombatState.IsInCombat)
                        _motionTarget.LeaveCombat();
                }
                if (_motionRemovedPresentation == null &&
                    _motionWeapon != null && _actor != null)
                {
                    string ignoredRole;
                    _motionRemovedPresentation =
                        WeaponPresentationEvidenceScenario
                            .ResolveActivePresentation(_actor,
                                _motionWeapon.Blueprint.VisualParameters,
                                "stored", out ignoredRole);
                }
                RemoveProductionMotionWeapon();
                if (_actor != null && _actor.View != null &&
                    _actor.View.HandsEquipment != null)
                {
                    _actor.View.HandsEquipment.UpdateAll();
                    _actor.View.HandsEquipment.ForceSwitch(false);
                }
                if (_motionSpec != null && string.Equals(
                        _motionSpec.Kind, "attack", StringComparison.Ordinal))
                    RetireProductionMotionTarget();
            }

            private void RemoveProductionMotionWeapon()
            {
                if (_actor != null && _actor.Body != null &&
                    _actor.Body.PrimaryHand.MaybeItem != null)
                    _actor.Body.PrimaryHand.RemoveItem(false);
                if (_motionWeapon != null)
                {
                    if (_motionFirearmStateSet)
                        FirearmRuntimeState.Service.Forget(_motionWeapon);
                    _motionWeapon.Dispose();
                }
                _motionWeapon = null;
                _motionFirearmStateSet = false;
            }

            private void PollProductionMotionRemoval()
            {
                _stage = "settle-motion-removal-" +
                    (_motionSpec == null ? "unknown" : _motionSpec.Label);
                TickProductionMotionRuntime();
                if (_actor != null && _actor.View != null &&
                    _actor.View.HandsEquipment != null)
                    _actor.View.HandsEquipment.ForceSwitch(false);
                _settleUpdates++;
                bool removedPresentation =
                    _motionRemovedPresentation == null ||
                    !_motionRemovedPresentation.gameObject.activeInHierarchy ||
                    !_motionRemovedPresentation.IsChildOf(
                        _actor.View.transform);
                bool cleared = ProductionMotionTransientStateCleared() &&
                    removedPresentation;
                if (!cleared)
                {
                    if (_settleUpdates < MotionMaximumUpdates) return;
                    throw new InvalidOperationException(
                        "Production motion transient state did not clear after " +
                        _settleUpdates + " updates.");
                }
                _motionRemovedPresentation = null;
                AdvanceProductionMotionAction();
            }

            private bool ProductionMotionTransientStateCleared()
            {
                if (_actor == null || _actor.View == null ||
                    _actor.View.HandsEquipment == null ||
                    _actor.View.MovementAgent == null ||
                    _actor.View.AnimationManager == null)
                    return false;
                bool targetClean = _motionTarget == null ||
                    !_motionTarget.CombatState.IsInCombat;
                return _motionWeapon == null &&
                    ProductionMotionRunningCommandTypes().Length == 0 &&
                    ProductionMotionResidentCommandTypes().Length == 0 &&
                    ProductionMotionQueuedCommandTypes().Length == 0 &&
                    _actor.Commands.Empty &&
                    _actor.Body.PrimaryHand.MaybeItem == null &&
                    _actor.Body.SecondaryHand.MaybeItem == null &&
                    _actor.View.HandsEquipment.GetWeaponModel(false) == null &&
                    _actor.View.HandsEquipment.GetWeaponModel(true) == null &&
                    !_actor.View.HandsEquipment.InCombat &&
                    !_actor.CombatState.IsInCombat && targetClean &&
                    _actor.View.MovementAgent.MaxSpeedOverride ==
                        _motionOriginalMaxSpeedOverride &&
                    _actor.View.AnimationManager.WalkSpeedType ==
                        _motionOriginalWalkSpeedType &&
                    Math.Abs(_actor.View.AnimationManager.Speed -
                        _motionOriginalAnimationSpeed) < 0.0001f &&
                    ProductionMotionPlayerBoundaryExact();
            }

            private bool ProductionMotionPlayerBoundaryExact()
            {
                return _motionPlayer != null &&
                    _motionTurnBasedController != null &&
                    ProductionMotionPlayerListsExact() &&
                    (_actor == null ||
                        ReferenceEquals(_actor.HoldingState,
                            _motionSceneState) &&
                        !ContainsReference(
                            _motionPlayer.ControllableCharacters, _actor)) &&
                    (_motionTarget == null ||
                        ReferenceEquals(_motionTarget.HoldingState,
                            _motionSceneState) &&
                        !ContainsReference(
                            _motionPlayer.ControllableCharacters,
                            _motionTarget)) &&
                    _motionPlayer.IsInCombat ==
                        _motionPlayerCombatBefore &&
                    _motionPlayer.Party.Count(unit => unit != null &&
                        unit.CombatState != null &&
                        unit.CombatState.IsInCombat) ==
                            _motionPartyCombatantsBefore &&
                    TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat() ==
                            _motionTurnBasedCombatBefore &&
                    _motionTurnBasedController.HasEnemyInCombat ==
                        _motionTurnBasedHasEnemyBefore &&
                    _motionTurnBasedController.HadEnemyAtSomePoint ==
                        _motionTurnBasedHadEnemyBefore &&
                    _motionTurnBasedController.SortedUnits.Count() ==
                        _motionTurnBasedUnitsBefore;
            }

            private string ProductionMotionPlayerBoundaryDescription()
            {
                if (_motionPlayer == null ||
                    _motionTurnBasedController == null)
                    return "player-boundary=unavailable";
                return "player=" + _motionPlayer.IsInCombat +
                    "/" + _motionPlayerCombatBefore + ";party=" +
                    _motionPlayer.Party.Count(unit => unit != null &&
                        unit.CombatState != null &&
                        unit.CombatState.IsInCombat) + "/" +
                    _motionPartyCombatantsBefore + ";turnBased=" +
                    TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat() + "/" +
                    _motionTurnBasedCombatBefore + ";hasEnemy=" +
                    _motionTurnBasedController.HasEnemyInCombat + "/" +
                    _motionTurnBasedHasEnemyBefore + ";hadEnemy=" +
                    _motionTurnBasedController.HadEnemyAtSomePoint + "/" +
                    _motionTurnBasedHadEnemyBefore + ";units=" +
                    _motionTurnBasedController.SortedUnits.Count() + "/" +
                    _motionTurnBasedUnitsBefore + ";playerLists=" +
                    ProductionMotionPlayerListsExact();
            }

            private bool ProductionMotionOutfitExact()
            {
                return _actor != null && _actor.View != null &&
                    _avatar != null && ProductionEntitiesPresent() &&
                    _avatar.EquipmentEntities.Any(value =>
                        ReferenceEquals(value, _hairEntity)) &&
                    _baseSavedLinks.SequenceEqual(
                        ProductionSavedLinks(_avatar),
                        StringComparer.Ordinal) &&
                    ProductionRampsExact(false) &&
                    HasExactHumanoidRig(_actor.View.transform) &&
                    ActiveRenderers(_actor).Length > 0;
            }

            private void ResetProductionMotionActionState()
            {
                _motionSpec = null;
                _motionMoveCommand = null;
                _motionAttackCommand = null;
                _motionAbilityData = null;
                _motionReloadCommand = null;
                _motionExpectedReloadAction = EffectiveReloadAction.Unknown;
                _motionPlannedRounds = 0;
                _motionRemovedPresentation = null;
                _motionMovementStart = Vector3.zero;
                _motionMovementDestination = Vector3.zero;
                _motionMovementStartArea = 0;
                _motionMovementDestinationArea = 0;
                _motionMovementGraphIndex = 0;
                _motionMovementRequestedSpeed = 0f;
                _motionMovementDistance = 0f;
                _motionMovementMaximumVelocity = 0f;
                _motionMovementCommandAccepted = false;
                _motionMovementObserved = false;
                _motionVelocityObserved = false;
                _motionTurnStartForward = Vector3.zero;
                _motionTurnDegrees = 0f;
                _motionAttackTargetPrepared = false;
                _motionAttackProbeDetached = false;
                _motionCommandInstalled = false;
                _motionCommandCanStart = false;
                _motionCommandCloseEnough = false;
                _motionCommandTargetInState = false;
                _motionCommandStarted = false;
                _motionCommandRunningObserved = false;
                _motionAttackRetirementReady = false;
                _motionAnimationObserved = false;
                _motionAnimationActedObserved = false;
                _motionActedCaptureTaken = false;
                _motionExecutionProcessObserved = false;
                _motionExecutionProcessEndedObserved = false;
                _motionAbilityAvailable = false;
                _motionAbilityTargetable = false;
                _motionRuntimeActionType = UnitCommand.CommandType.Free;
                _motionRequireFullRoundAction = false;
                _motionExplicitCommandTicks = 0;
                _motionExplicitProcessTicks = 0;
                _motionActionUpdates = 0;
                _motionCaptureScheduleIndex = 0;
                _motionTargetAttempts = 0;
                _motionTargetPlacement = "<not-prepared>";
                _motionTargetDistance = 0f;
                _motionApproachRadius = 0f;
                _motionNeedLineOfSight = false;
                _motionFiredBefore = 0;
                _motionDischargeFaultsBefore = 0;
                _motionReloadAttemptsBefore = 0;
                _motionReloadLoadedBefore = 0;
                _motionReloadRejectedBefore = 0;
                _motionReloadFaultsBefore = 0;
                _motionPowderBeforeCommand = 0;
                _motionBallBeforeCommand = 0;
                _motionPreviousActionCleared = false;
            }

            private void AdvanceProductionMotionAction()
            {
                ResetProductionMotionActionState();
                _motionStep++;
                _settleUpdates = 0;
                if (_motionStep < ProductionMotionSpecs.Length)
                {
                    _motionPhase = 1;
                    WriteProgress("motion-action-advanced");
                    return;
                }
                FinishProductionMotionFixture();
            }

            private void FinishProductionMotionFixture()
            {
                ProductionCompatibilityFixture fixture =
                    _fixtures[_fixtureIndex];
                _stage = "restore-motion-" + fixture.Label;
                bool outfitBeforeRestore = ProductionMotionOutfitExact();
                bool originalRestored = outfitBeforeRestore &&
                    RestoreProductionSnapshot(_avatarBefore,
                        _savedLinksBefore);
                _motionRestorationRecords.Add(new JObject
                {
                    { "fixture", fixture.Label },
                    { "productionOutfitExactBeforeRestore",
                        outfitBeforeRestore },
                    { "originalAvatarRestored", originalRestored },
                    { "walkMaximumVelocity",
                        _motionWalkMaximumVelocity },
                    { "runMaximumVelocity",
                        _motionRunMaximumVelocity },
                    { "runDistinctFromWalk",
                        _motionRunMaximumVelocity >
                            _motionWalkMaximumVelocity * 1.2f },
                    { "savedLinksUnchanged", _avatar != null &&
                        _savedLinksBefore.SequenceEqual(
                            ProductionSavedLinks(_avatar),
                            StringComparer.Ordinal) }
                });
                if (!originalRestored)
                    throw new InvalidOperationException(fixture.Label +
                        " did not restore its exact original avatar after motion.");
                _motionRestorations++;
                RetireProductionMotionTarget();
                RetireProductionActor();
                RetireProductionMotionFactions();
                if (_motionSceneState == null ||
                    _motionSceneState.AllEntityData.Count != 0)
                    throw new InvalidOperationException(fixture.Label +
                        " did not empty its request-local loaded-scene state.");
                ReconcileProductionMotionCombatBoundary(fixture.Label);
                _fixtureIndex++;
                _motionStep = 0;
                _motionPhase = 0;
                _settleUpdates = 0;
                if (_fixtureIndex < _fixtures.Length)
                {
                    _phase = 1;
                    return;
                }
                WriteProductionMotionIndex();
                _motionIndexWritten = true;
                BeginCleanup();
            }

            private void RetireProductionMotionTarget()
            {
                if (_motionTarget != null)
                {
                    UnitEntityData retiredTarget = _motionTarget;
                    if (_actor != null && _actor.Group != null)
                        _actor.Group.Memory.Remove(retiredTarget);
                    if (retiredTarget.Group != null && _actor != null)
                        retiredTarget.Group.Memory.Remove(_actor);
                    UnitEntityData dependent =
                        retiredTarget.Descriptor == null ? null :
                        retiredTarget.Descriptor.Pet;
                    if (dependent != null &&
                        !_unitsBefore.Any(value => ReferenceEquals(
                            value, dependent)))
                    {
                        dependent.Commands.InterruptAll(true);
                        if (dependent.CombatState.IsInCombat)
                            dependent.LeaveCombat();
                        if (ContainsReference(_party, dependent))
                            Game.Instance.Player.Party.Remove(dependent);
                        if (ContainsReference(_allUnits, dependent))
                            Game.Instance.State.Units.All.Remove(dependent);
                        DisposeProductionMotionEntity(dependent);
                    }
                    retiredTarget.Commands.InterruptAll(true);
                    if (retiredTarget.CombatState.IsInCombat)
                        retiredTarget.LeaveCombat();
                    if (retiredTarget.Descriptor != null)
                        retiredTarget.Descriptor.State.Immortality.ReleaseAll();
                    if (ContainsReference(_party, retiredTarget))
                        Game.Instance.Player.Party.Remove(retiredTarget);
                    if (ContainsReference(_allUnits, retiredTarget))
                        Game.Instance.State.Units.All.Remove(retiredTarget);
                    DisposeProductionMotionEntity(retiredTarget);
                }
                if (_motionHostileBlueprint != null)
                    UnityEngine.Object.DestroyImmediate(
                        _motionHostileBlueprint);
                _motionTarget = null;
                _motionHostileBlueprint = null;
                RefreshProductionMotionPlayerLists();
                if (!ProductionMotionPlayerListsExact())
                    throw new InvalidOperationException(
                        "Production motion target retirement changed player character lists.");
            }

            private void RetireProductionMotionFactions()
            {
                if (_motionTarget != null ||
                    _motionHostileBlueprint != null)
                    RetireProductionMotionTarget();
                if (_motionActorFaction != null)
                    UnityEngine.Object.DestroyImmediate(_motionActorFaction);
                if (_motionTargetFaction != null)
                    UnityEngine.Object.DestroyImmediate(_motionTargetFaction);
                _motionActorFaction = null;
                _motionTargetFaction = null;
            }

            private void DisposeProductionMotionEntity(
                UnitEntityData entity)
            {
                if (entity == null) return;
                if (_motionSceneState != null &&
                    ReferenceEquals(entity.HoldingState,
                        _motionSceneState) &&
                    _motionSceneState.AllEntityData.Any(value =>
                        ReferenceEquals(value, entity)))
                {
                    _motionSceneState.RemoveEntityData(entity);
                    return;
                }
                entity.Dispose();
            }

            private void RetireProductionMotionScene()
            {
                if (_motionSceneState == null || _motionSceneDisposed)
                    return;
                foreach (EntityDataBase entity in _motionSceneState
                    .AllEntityData.ToArray())
                {
                    UnitEntityData unit = entity as UnitEntityData;
                    if (unit != null)
                    {
                        if (unit.Commands != null)
                            unit.Commands.InterruptAll(true);
                        if (unit.CombatState != null &&
                            unit.CombatState.IsInCombat)
                            unit.LeaveCombat();
                        if (unit.Descriptor != null)
                            unit.Descriptor.State.Immortality.ReleaseAll();
                        if (ContainsReference(_party, unit))
                            Game.Instance.Player.Party.Remove(unit);
                        if (ContainsReference(_allUnits, unit))
                            Game.Instance.State.Units.All.Remove(unit);
                    }
                    _motionSceneState.RemoveEntityData(entity);
                }
                _motionSceneState.Dispose();
                _motionSceneDisposed = true;
                if (_motionSceneState.AllEntityData.Count != 0)
                    throw new InvalidOperationException(
                        "Production motion request-local scene did not dispose exactly.");
            }

            private void ReconcileProductionMotionCombatBoundary(
                string fixture)
            {
                if (_motionPlayer == null)
                    throw new InvalidOperationException(
                        "Production motion lost its exact player reference.");
                RefreshProductionMotionPlayerLists();
                bool playerListsBefore =
                    ProductionMotionPlayerListsExact();
                bool playerBefore = _motionPlayer.IsInCombat;
                bool turnBasedBefore =
                    TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat();
                int partyBefore = _motionPlayer.Party.Count(unit =>
                    unit != null && unit.CombatState != null &&
                        unit.CombatState.IsInCombat);
                bool hasEnemyBefore = _motionTurnBasedController
                    .HasEnemyInCombat;
                bool hadEnemyBefore = _motionTurnBasedController
                    .HadEnemyAtSomePoint;
                int turnBasedUnitsBefore = _motionTurnBasedController
                    .SortedUnits.Count();
                _motionTurnBasedController.Tick();
                bool hasEnemyAfterTurnTick = _motionTurnBasedController
                    .HasEnemyInCombat;
                _motionCombatLeaveController.Tick();
                _motionCombatJoinController.Tick();
                RefreshProductionMotionPlayerLists();
                bool playerListsAfter =
                    ProductionMotionPlayerListsExact();
                bool playerAfter = _motionPlayer.IsInCombat;
                bool turnBasedAfter =
                    TurnBased.Controllers.CombatController
                        .IsInTurnBasedCombat();
                int partyAfter = _motionPlayer.Party.Count(unit =>
                    unit != null && unit.CombatState != null &&
                        unit.CombatState.IsInCombat);
                bool hasEnemyAfter = _motionTurnBasedController
                    .HasEnemyInCombat;
                bool hadEnemyAfter = _motionTurnBasedController
                    .HadEnemyAtSomePoint;
                int turnBasedUnitsAfter = _motionTurnBasedController
                    .SortedUnits.Count();
                _motionCombatBoundaryRecords.Add(new JObject
                {
                    { "fixture", fixture },
                    { "playerInCombatBeforeReconcile", playerBefore },
                    { "playerInCombatAfterReconcile", playerAfter },
                    { "partyCombatantsBeforeReconcile", partyBefore },
                    { "partyCombatantsAfterReconcile", partyAfter },
                    { "turnBasedCombatBeforeReconcile", turnBasedBefore },
                    { "turnBasedCombatAfterReconcile", turnBasedAfter },
                    { "turnBasedHasEnemyBeforeReconcile", hasEnemyBefore },
                    { "turnBasedHasEnemyAfterTurnTick", hasEnemyAfterTurnTick },
                    { "turnBasedHasEnemyAfterReconcile", hasEnemyAfter },
                    { "turnBasedHadEnemyBeforeReconcile", hadEnemyBefore },
                    { "turnBasedHadEnemyAfterReconcile", hadEnemyAfter },
                    { "turnBasedUnitsBeforeReconcile", turnBasedUnitsBefore },
                    { "turnBasedUnitsAfterReconcile", turnBasedUnitsAfter },
                    { "playerCharacterListsBeforeReconcile",
                        playerListsBefore },
                    { "playerCharacterListsAfterReconcile",
                        playerListsAfter },
                    { "expectedPlayerInCombat",
                        _motionPlayerCombatBefore },
                    { "expectedPartyCombatants",
                        _motionPartyCombatantsBefore },
                    { "expectedTurnBasedCombat",
                        _motionTurnBasedCombatBefore },
                    { "expectedTurnBasedHasEnemy",
                        _motionTurnBasedHasEnemyBefore },
                    { "expectedTurnBasedHadEnemy",
                        _motionTurnBasedHadEnemyBefore },
                    { "expectedTurnBasedUnits",
                        _motionTurnBasedUnitsBefore },
                    { "nativeReconciliation",
                        "TurnBased.Controllers.CombatController.Tick->" +
                        "Kingmaker.Controllers.Combat.UnitCombatLeaveController.Tick->" +
                        "Kingmaker.Controllers.Combat.UnitCombatJoinController.Tick" }
                });
                _diagnostics.Add("productionMotionCombatBoundary=" +
                    fixture + ";player=" + playerBefore + "->" +
                    playerAfter + ";party=" + partyBefore + "->" +
                    partyAfter + ";turnBased=" + turnBasedBefore +
                    "->" + turnBasedAfter + ";hasEnemy=" +
                    hasEnemyBefore + "->" + hasEnemyAfterTurnTick +
                    "->" + hasEnemyAfter + ";hadEnemy=" +
                    hadEnemyBefore + "->" + hadEnemyAfter +
                    ";turnBasedUnits=" + turnBasedUnitsBefore + "->" +
                    turnBasedUnitsAfter + ";playerLists=" +
                    playerListsBefore + "->" + playerListsAfter);
                if (!playerListsBefore || !playerListsAfter ||
                    playerAfter != _motionPlayerCombatBefore ||
                    partyAfter != _motionPartyCombatantsBefore ||
                    turnBasedAfter != _motionTurnBasedCombatBefore ||
                    hasEnemyAfter != _motionTurnBasedHasEnemyBefore ||
                    hadEnemyAfter != _motionTurnBasedHadEnemyBefore ||
                    turnBasedUnitsAfter != _motionTurnBasedUnitsBefore)
                    throw new InvalidOperationException(fixture +
                        " did not restore the exact native combat boundary.");
            }

            private void PrepareProductionMotionCleanup()
            {
                try
                {
                    if (_actor != null)
                    {
                        _actor.Commands.InterruptAll(true);
                        if (_actor.View != null)
                        {
                            _actor.View.StopMoving();
                            if (_actor.View.MovementAgent != null)
                                _actor.View.MovementAgent.MaxSpeedOverride =
                                    _motionOriginalMaxSpeedOverride;
                            if (_actor.View.AnimationManager != null)
                            {
                                _actor.View.AnimationManager.WalkSpeedType =
                                    _motionOriginalWalkSpeedType;
                                _actor.View.AnimationManager.Speed =
                                    _motionOriginalAnimationSpeed;
                            }
                        }
                        if (_actor.CombatState.IsInCombat)
                            _actor.LeaveCombat();
                    }
                    RemoveProductionMotionWeapon();
                    RetireProductionMotionTarget();
                    if (_motionTurnBasedController != null)
                        _motionTurnBasedController.Tick();
                    if (_motionCombatLeaveController != null)
                        _motionCombatLeaveController.Tick();
                    if (_motionCombatJoinController != null)
                        _motionCombatJoinController.Tick();
                }
                finally
                {
                    RestoreProductionMotionInventory();
                }
            }

            private void RestoreProductionMotionInventory()
            {
                if (!_motionInventoryCaptured)
                {
                    _motionInventoryRestored = true;
                    return;
                }
                if (_motionPlayer == null ||
                    _motionPlayer.Inventory == null ||
                    _motionPowder == null || _motionBall == null)
                {
                    _motionInventoryRestored = false;
                    return;
                }
                RestoreProductionMotionInventoryCount(_motionPowder,
                    _motionPowderBefore);
                RestoreProductionMotionInventoryCount(_motionBall,
                    _motionBallBefore);
                _motionInventoryRestored =
                    _motionPlayer.Inventory.Count(_motionPowder) ==
                        _motionPowderBefore &&
                    _motionPlayer.Inventory.Count(_motionBall) ==
                        _motionBallBefore;
            }

            private void RestoreProductionMotionInventoryCount(
                BlueprintItem item, int expected)
            {
                int current = _motionPlayer.Inventory.Count(item);
                if (current > expected)
                    _motionPlayer.Inventory.Remove(item,
                        current - expected);
                else if (current < expected)
                    _motionPlayer.Inventory.Add(item,
                        expected - current);
            }

            private void WriteProductionMotionIndex()
            {
                _stage = "write-production-motion-index";
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                var index = new JObject
                {
                    { "schemaVersion", 1 },
                    { "scenario", _request.Scenario },
                    { "fixture",
                        "exact production male/female Human Gunslinger DollData actors" },
                    { "loadedModVersion",
                        _context.ModEntry.Info.Version },
                    { "gitCommit", identity.GitCommit },
                    { "runtimeIdentity", identity.RuntimeIdentity },
                    { "gameAssemblySha256", _gameAssemblySha256 },
                    { "gameAssemblyMvid", _gameAssemblyMvid },
                    { "productionClassGuid", _gunslingerClass.AssetGuid },
                    { "maleAssetIds", new JArray(
                        GunslingerClassAppearanceCatalog.MaleAssetIds()) },
                    { "femaleAssetIds", new JArray(
                        GunslingerClassAppearanceCatalog.FemaleAssetIds()) },
                    { "defaultPrimaryColor",
                        GunslingerClassAppearanceCatalog
                            .DefaultPrimaryColor },
                    { "defaultSecondaryColor",
                        GunslingerClassAppearanceCatalog
                            .DefaultSecondaryColor },
                    { "actions", new JArray(ProductionMotionSpecs.Select(
                        value => value.Label).ToArray()) },
                    { "attackCaptureUpdates",
                        new JArray(ProductionMotionAttackUpdates) },
                    { "reloadCaptureUpdates",
                        new JArray(ProductionMotionReloadUpdates) },
                    { "views", new JArray("front", "right-side", "rear",
                        "front-right-three-quarter") },
                    { "fixtures", _motionFixtureRecords },
                    { "movementOutcomes", _motionMovementOutcomes },
                    { "turnOutcomes", _motionTurnOutcomes },
                    { "attackOutcomes", _motionAttackOutcomes },
                    { "reloadOutcomes", _motionReloadOutcomes },
                    { "records", _motionRecords },
                    { "restorations", _motionRestorationRecords },
                    { "combatBoundaries", _motionCombatBoundaryRecords },
                    { "playerInCombatBefore", _motionPlayerCombatBefore },
                    { "partyCombatantsBefore",
                        _motionPartyCombatantsBefore },
                    { "turnBasedCombatBefore",
                        _motionTurnBasedCombatBefore },
                    { "turnBasedHasEnemyBefore",
                        _motionTurnBasedHasEnemyBefore },
                    { "turnBasedHadEnemyBefore",
                        _motionTurnBasedHadEnemyBefore },
                    { "turnBasedUnitsBefore",
                        _motionTurnBasedUnitsBefore },
                    { "ammunitionSeed", _motionAmmunitionSeed },
                    { "powderCountBefore", _motionPowderBefore },
                    { "ballCountBefore", _motionBallBefore },
                    { "productionBlueprintUnchanged",
                        ProductionBlueprintUnchanged() },
                    { "productionBlueprintMutated", false },
                    { "saveApiCalled", false }
                };
                string path = Path.Combine(_request.EvidenceDirectory,
                    "gunslinger-outfit-production-motion-index.json");
                WriteJsonAtomic(path, index);
                _evidenceFiles.Add(path);
            }

            private void FinishProductionMotion(bool cleaned)
            {
                const int expectedFixtures = 2;
                const int expectedRecords = 54;
                JObject[] records = _motionRecords.OfType<JObject>()
                    .ToArray();
                JObject[] movements = _motionMovementOutcomes
                    .OfType<JObject>().ToArray();
                JObject[] attacks = _motionAttackOutcomes
                    .OfType<JObject>().ToArray();
                JObject[] reloads = _motionReloadOutcomes
                    .OfType<JObject>().ToArray();
                JObject[] combatBoundaries =
                    _motionCombatBoundaryRecords.OfType<JObject>()
                        .ToArray();
                var expectedActionCounts = new Dictionary<string, int>(
                    StringComparer.Ordinal)
                {
                    { "unarmed-idle", 2 },
                    { "musket-slow-walk", 2 },
                    { "musket-normal-run", 2 },
                    { "musket-turn-right", 2 },
                    { "pistol-native-attack", 10 },
                    { "musket-native-attack", 10 },
                    { "musket-production-reload", 16 },
                    { "shortsword-native-melee", 10 }
                };
                bool exactActionCounts = expectedActionCounts.All(pair =>
                    records.Count(value => string.Equals(
                        (string)value["action"], pair.Key,
                        StringComparison.Ordinal)) == pair.Value);
                bool recordContracts = records.All(value =>
                    (bool)value["previousActionCleared"] &&
                    (bool)value["attackReadinessProbeDetached"] &&
                    (bool)value["productionEntitiesPresent"] &&
                    (bool)value["hairEntityPreserved"] &&
                    (bool)value["savedLinksUnchanged"] &&
                    (bool)value["humanoidRigExact"] &&
                    (bool)value["productionBlueprintUnchanged"] &&
                    !(bool)value["productionBlueprintMutated"] &&
                    !(bool)value["saveApiCalled"] &&
                    !(bool)value["actorIsPlayerFaction"] &&
                    !(bool)value["actorSharesPlayerGroup"] &&
                    (bool)value[
                        "actorHoldingStateIsRequestLocalLoadedScene"] &&
                    (bool)value[
                        "requestLocalSceneMatchesLoadedScene"] &&
                    !(bool)value["actorInControllableCharacters"] &&
                    !(bool)value["actorPlayerHostility"] &&
                    !(bool)value["targetPlayerHostility"] &&
                    (bool)value["playerBoundaryExact"] &&
                    (bool)value["playerCharacterListsExact"] &&
                    (string.Equals((string)value["kind"], "attack",
                            StringComparison.Ordinal)
                        ? (bool)value["targetPresent"] &&
                            !(bool)value["targetIsPlayerFaction"] &&
                            !(bool)value["targetSharesPlayerGroup"] &&
                            (bool)value[
                                "targetHoldingStateIsRequestLocalLoadedScene"] &&
                            !(bool)value[
                                "targetInControllableCharacters"] &&
                            (bool)value["actorTargetBilateralEnemy"]
                        : !(bool)value["targetPresent"]) &&
                    (int)value["activeRendererCount"] > 0 &&
                    (int)value["preview"]["meaningfulPixels"] > 0);
                bool movementContracts = movements.Length == 4 &&
                    movements.All(value =>
                        (bool)value["commandAccepted"] &&
                        (bool)value["movingObserved"] &&
                        (bool)value["velocityObserved"] &&
                        (float)value["distanceMeters"] >= 0.75f &&
                        (uint)value["startArea"] ==
                            (uint)value["destinationArea"]) &&
                    _motionRestorationRecords.OfType<JObject>().All(value =>
                        (bool)value["runDistinctFromWalk"]);
                bool turnContracts = _motionTurnOutcomes.Count == 2 &&
                    _motionTurnOutcomes.OfType<JObject>().All(value =>
                        (float)value["turnDegrees"] >= 60f);
                bool attackContracts = attacks.Length == 6 &&
                    attacks.All(value =>
                        (bool)value["readinessProbeDetached"] &&
                        (bool)value["commandInstalled"] &&
                        (bool)value["commandCanStart"] &&
                        (bool)value["commandCloseEnough"] &&
                        (bool)value["commandTargetInState"] &&
                        (bool)value["commandStarted"] &&
                        (bool)value["commandRunningObserved"] &&
                        (bool)value["retirementReady"] &&
                        (bool)value["slotEvicted"] &&
                        ((JArray)value[
                            "residentCommandTypesAfterRetirement"]).Count ==
                            0 &&
                        ((JArray)value[
                            "queuedCommandTypesAfterRetirement"]).Count == 0 &&
                        ((JArray)value[
                            "runningCommandTypesAfterRetirement"]).Count ==
                            0 &&
                        (bool)value["animationObserved"] &&
                        (bool)value["animationActedObserved"] &&
                        (bool)value["actedCaptureTaken"] &&
                        (!(bool)value["firearm"] ||
                            ((long)value["firedDelta"] >= 1 &&
                            (long)value["faultDelta"] == 0)));
                bool reloadContracts = reloads.Length == 2 &&
                    reloads.All(value =>
                        (bool)value["abilityAvailable"] &&
                        (bool)value["abilityTargetable"] &&
                        (bool)value["commandCanStart"] &&
                        (bool)value["commandInstalled"] &&
                        (bool)value["commandStarted"] &&
                        (bool)value["commandRunningObserved"] &&
                        (bool)value["animationObserved"] &&
                        (bool)value["animationActedObserved"] &&
                        (bool)value["actedCaptureTaken"] &&
                        (bool)value["executionProcessObserved"] &&
                        (bool)value["executionProcessEndedObserved"] &&
                        (long)value["attemptsDelta"] >= 1 &&
                        (long)value["loadedDelta"] >= 1 &&
                        (long)value["rejectedDelta"] == 0 &&
                        (long)value["faultDelta"] == 0 &&
                        (int)value["loadedRoundsAfter"] >=
                            (int)value["plannedRounds"] &&
                        (int)value["powderConsumed"] ==
                            (int)value["plannedRounds"] &&
                        (int)value["ballsConsumed"] ==
                            (int)value["plannedRounds"] &&
                        (long)value["firedDelta"] == 0 &&
                        (long)value["dischargeFaultDelta"] == 0);
                bool combatBoundaryContracts =
                    combatBoundaries.Length == expectedFixtures &&
                    combatBoundaries.All(value =>
                        (bool)value["playerInCombatAfterReconcile"] ==
                            _motionPlayerCombatBefore &&
                        (int)value["partyCombatantsAfterReconcile"] ==
                            _motionPartyCombatantsBefore &&
                        (bool)value["turnBasedCombatAfterReconcile"] ==
                            _motionTurnBasedCombatBefore &&
                        (bool)value["turnBasedHasEnemyAfterReconcile"] ==
                            _motionTurnBasedHasEnemyBefore &&
                        (bool)value["turnBasedHadEnemyAfterReconcile"] ==
                            _motionTurnBasedHadEnemyBefore &&
                        (int)value["turnBasedUnitsAfterReconcile"] ==
                            _motionTurnBasedUnitsBefore &&
                        (bool)value[
                            "playerCharacterListsBeforeReconcile"] &&
                        (bool)value[
                            "playerCharacterListsAfterReconcile"] &&
                        string.Equals((string)value[
                            "nativeReconciliation"],
                            "TurnBased.Controllers.CombatController.Tick->" +
                            "Kingmaker.Controllers.Combat.UnitCombatLeaveController.Tick->" +
                            "Kingmaker.Controllers.Combat.UnitCombatJoinController.Tick",
                            StringComparison.Ordinal));

                Add(_assertions,
                    "gunslinger-outfit-production-motion-guard",
                    RuntimeTestScenarioCatalog
                        .GunslingerOutfitProductionMotion,
                    _request.Scenario,
                    string.Equals(_request.Scenario,
                        RuntimeTestScenarioCatalog
                            .GunslingerOutfitProductionMotion,
                        StringComparison.Ordinal),
                    "validated -kmgRuntimeTestRequest allowlist");
                Add(_assertions,
                    "gunslinger-outfit-production-motion-save-boundary",
                    "KMG_AUTOMATION_WORKING; no save API",
                    "saveName=" + _request.Parameters.Value<string>(
                        "saveName") + ";saveApiCalled=false",
                    string.Equals(_request.Parameters.Value<string>(
                        "saveName"), "KMG_AUTOMATION_WORKING",
                        StringComparison.Ordinal),
                    "guarded working-save load plus disposable actors");
                Add(_assertions,
                    "gunslinger-outfit-production-motion-game-identity",
                    "Kingmaker 2.1.7b exact Assembly-CSharp SHA-256 and MVID",
                    "sha256=" + _gameAssemblySha256 + ";mvid=" +
                        _gameAssemblyMvid,
                    string.Equals(_gameAssemblySha256,
                        ExpectedAssemblySha256,
                        StringComparison.Ordinal) &&
                    string.Equals(_gameAssemblyMvid,
                        ExpectedAssemblyMvid,
                        StringComparison.OrdinalIgnoreCase),
                    "live loaded Assembly-CSharp identity");

                Add(_assertions,
                    "gunslinger-outfit-production-motion-fixtures",
                    "one exact male and female Human production DollData fixture",
                    "fixtures=" + _motionFixtureRecords.Count,
                    _motionFixtureRecords.Count == expectedFixtures &&
                        _motionFixtureRecords.OfType<JObject>().All(value =>
                            (bool)value["productionOutfitExact"] &&
                            (bool)value["humanoidRigExact"] &&
                            !(bool)value["actorIsPlayerFaction"] &&
                            !(bool)value["actorSharesPlayerGroup"] &&
                            (bool)value[
                                "actorHoldingStateIsRequestLocalLoadedScene"] &&
                            (bool)value[
                                "requestLocalSceneMatchesLoadedScene"] &&
                            !(bool)value[
                                "actorInControllableCharacters"] &&
                            (bool)value["playerCharacterListsExact"] &&
                            (int)value["locomotionClipCount"] > 0 &&
                            (int)value["mainHandAttackClipCount"] > 0),
                    "production class DollState/CreateData/CreateUnitView plus live rig contracts");
                Add(_assertions,
                    "gunslinger-outfit-production-motion-captures",
                    "54 exact sidecars/PNGs and 216 labelled views",
                    "records=" + records.Length + ";captured=" +
                        _motionCaptured + ";views=" + _motionViewCount +
                        ";files=" + _evidenceFiles.Count,
                    records.Length == expectedRecords &&
                        _motionCaptured == expectedRecords &&
                        _motionViewCount == expectedRecords * 4 &&
                        exactActionCounts && recordContracts &&
                        _motionIndexWritten &&
                        _evidenceFiles.Count == expectedRecords * 2 + 1 &&
                        _evidenceFiles.All(File.Exists),
                    "four-view contact sheets plus structured per-frame sidecars");
                Add(_assertions,
                    "gunslinger-outfit-production-native-locomotion",
                    "Slow walk and Normal run per gender with accepted UnitMoveTo, live velocity/displacement, and distinct observed speeds",
                    "outcomes=" + movements.Length +
                        ";restorations=" +
                        _motionRestorationRecords.Count,
                    movementContracts,
                    "UnitMoveTo, same-area ForcedPath, MovementAgent velocity, MaxSpeedOverride, and WalkSpeedType");
                Add(_assertions,
                    "gunslinger-outfit-production-native-turn",
                    "one body-relative native turn of at least 60 degrees per gender",
                    "outcomes=" + _motionTurnOutcomes.Count,
                    turnContracts,
                    "ForceLookAt and live OrientationDirection");
                Add(_assertions,
                    "gunslinger-outfit-production-native-attacks",
                    "pistol, musket, and Shortsword UnitAttack per gender with fixed and acted frames",
                    "outcomes=" + attacks.Length,
                    attackContracts,
                    "UnitAttack.CreateAttackCommand, UnitCommands.Run, acted animation, and firearm discharge counters");
                Add(_assertions,
                    "gunslinger-outfit-production-native-reload",
                    "production musket Reload Firearm per gender through update 240 with exact ammunition delivery",
                    "outcomes=" + reloads.Length,
                    reloadContracts,
                    "AbilityData, UnitUseAbility, execution process, ReloadRuntimeDiagnostics, and exact powder/ball deltas");
                Add(_assertions,
                    "gunslinger-outfit-production-motion-restoration",
                    "exact original avatar, movement settings, and shared ammunition restored for both fixtures",
                    "avatars=" + _motionRestorations +
                        ";inventoryRestored=" +
                        _motionInventoryRestored,
                    _motionRestorations == expectedFixtures &&
                        _motionRestorationRecords.Count == expectedFixtures &&
                        _motionRestorationRecords.OfType<JObject>().All(
                            value =>
                                (bool)value[
                                    "productionOutfitExactBeforeRestore"] &&
                                (bool)value["originalAvatarRestored"] &&
                                (bool)value["savedLinksUnchanged"] &&
                                (bool)value["runDistinctFromWalk"]) &&
                        _motionInventoryRestored,
                    "saved:false Character snapshots, exact movement settings, and inventory count snapshots");
                Add(_assertions,
                    "gunslinger-outfit-production-motion-combat-boundary",
                    "exact pre-run player, party, and turn-based combat state after each disposable fixture",
                    "boundaries=" + combatBoundaries.Length +
                        ";playerInCombat=" +
                        (_motionPlayer == null ? "<missing>" :
                            _motionPlayer.IsInCombat.ToString()) +
                        ";turnBased=" +
                        TurnBased.Controllers.CombatController
                            .IsInTurnBasedCombat(),
                    combatBoundaryContracts,
                    "full UnitEntityData.LeaveCombat event, registered turn-based cache tick, group retirement, player recomputation, and party event");
                bool blueprintUnchanged = _gunslingerClass != null &&
                    ProductionBlueprintUnchanged();
                Add(_assertions,
                    "gunslinger-outfit-production-motion-blueprint-immutability",
                    "published class arrays, links, and colors remain exact",
                    "unchanged=" + blueprintUnchanged,
                    blueprintUnchanged,
                    "pre/post production BlueprintCharacterClass snapshot");
                Add(_assertions,
                    "gunslinger-outfit-production-motion-cleanup",
                    "exact party/global-unit/inventory snapshots restored; no save call",
                    "cleaned=" + cleaned + ";inventory=" +
                        _motionInventoryRestored + ";target=" +
                        (_motionTarget == null) + ";factions=" +
                        (_motionActorFaction == null &&
                            _motionTargetFaction == null) +
                        ";playerLists=" +
                        ProductionMotionPlayerListsExact() +
                        ";requestLocalSceneDisposed=" +
                        _motionSceneDisposed +
                        ";requestLocalSceneEmpty=" +
                        (_motionSceneState != null &&
                            _motionSceneState.AllEntityData.Count == 0) +
                        ";loadedScene=" + ReferenceEquals(
                            _motionLoadedSceneState,
                            _motionAreaState == null ? null :
                                _motionAreaState.MainState),
                    cleaned && _motionInventoryRestored &&
                        _motionTarget == null && _motionPlayer != null &&
                        _motionHostileBlueprint == null &&
                        _motionActorFaction == null &&
                        _motionTargetFaction == null &&
                        ProductionMotionPlayerListsExact() &&
                        ReferenceEquals(_motionAreaState,
                            Game.Instance.State.LoadedAreaState) &&
                        ReferenceEquals(_motionLoadedSceneState,
                            _motionAreaState.MainState) &&
                        _motionLoadedSceneState.IsSceneLoaded &&
                        _motionSceneState != null &&
                        !ReferenceEquals(_motionSceneState,
                            _motionLoadedSceneState) &&
                        !ReferenceEquals(_motionSceneState,
                            _motionPlayer.CrossSceneState) &&
                        string.Equals(_motionSceneState.SceneName,
                            _motionLoadedSceneState.SceneName,
                            StringComparison.Ordinal) &&
                        _motionSceneState.SkipSerialize &&
                        _motionSceneDisposed &&
                        _motionSceneState.AllEntityData.Count == 0 &&
                        _motionPlayer.IsInCombat ==
                            _motionPlayerCombatBefore &&
                        _motionPlayer.Party.Count(unit =>
                            unit != null && unit.CombatState != null &&
                            unit.CombatState.IsInCombat) ==
                                _motionPartyCombatantsBefore &&
                        TurnBased.Controllers.CombatController
                            .IsInTurnBasedCombat() ==
                                _motionTurnBasedCombatBefore &&
                        _motionTurnBasedController.HasEnemyInCombat ==
                            _motionTurnBasedHasEnemyBefore &&
                        _motionTurnBasedController.HadEnemyAtSomePoint ==
                            _motionTurnBasedHadEnemyBefore &&
                        _motionTurnBasedController.SortedUnits.Count() ==
                            _motionTurnBasedUnitsBefore,
                    "request-local loaded-scene state, actors, targets, factions, items, blueprint clones, cameras, textures, and ammunition");
                Add(_assertions, "loaded-mod-version",
                    _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    string.Equals(_request.ExpectedModVersion,
                        _context.ModEntry.Info.Version,
                        StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version");

                _warnings.Add("Direct inspection of every generated motion " +
                    "contact sheet is required before clipping or silhouette acceptance.");
                _warnings.Add("This scenario proves native motion behavior; " +
                    "save/load persistence is qualified separately.");
                RuntimeBuildIdentity build = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                bool passed = _assertions.All(value =>
                    value.Status == RuntimeTestStatuses.Pass);
                Result = new RuntimeTestResult
                {
                    SchemaVersion = 1,
                    RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    Status = passed ? RuntimeTestStatuses.Pass :
                        RuntimeTestStatuses.Fail,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = build.RuntimeIdentity + "; mvid=" +
                        build.ModuleVersionId + "; sha256=" +
                        build.LoadedModuleSha256 + "; pid=" +
                        build.ProcessId,
                    GitCommit = build.GitCommit,
                    GameVersion = Application.version ?? string.Empty,
                    StartUtc = _started.ToString("o"),
                    EndUtc = DateTime.UtcNow.ToString("o"),
                    DurationMilliseconds =
                        (long)(DateTime.UtcNow - _started)
                            .TotalMilliseconds,
                    Assertions = _assertions,
                    Diagnostics = _diagnostics,
                    Warnings = _warnings,
                    ExceptionSummary = _exceptionSummary,
                    EvidenceFiles = _evidenceFiles,
                    AutomaticExitRequested =
                        _request.ExitAfterCompletion,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
                Complete = true;
            }

        }
    }
}
