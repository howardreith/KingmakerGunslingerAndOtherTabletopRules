using System;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Reversible native spellcasting, prone, death/resurrection, and
    /// polymorph transitions for the elemental production-motion fixtures.
    /// </summary>
    internal static partial class GunslingerOutfitRenderScenario
    {
        private const string BeastShapeTwoSpellGuid =
            "5d4028eb28a106d4691ed1b92bbb1915";
        private const string BeastShapeTwoBuffGuid =
            "8dc6510d31614345a8c718208fbac1f8";

        private static readonly string[] ElementalTransitionActions =
        {
            "racial-sla-native-cast",
            "native-prone-recovery",
            "native-death-resurrection",
            "beast-shape-ii-polymorph-return"
        };

        internal sealed partial class ProductionCompatibilitySession
        {
            private const int ElementalTransitionMinimumUpdates = 18;
            private const int ElementalTransitionMaximumUpdates = 600;
            private readonly JArray _elementalTransitionRecords =
                new JArray();
            private readonly JArray _elementalSpellcastOutcomes =
                new JArray();
            private readonly JArray _elementalProneOutcomes =
                new JArray();
            private readonly JArray _elementalDeathOutcomes =
                new JArray();
            private readonly JArray _elementalPolymorphOutcomes =
                new JArray();
            private bool _elementalTransitionFixtureComplete;
            private int _elementalTransitionStep;
            private int _elementalTransitionPhase;
            private int _elementalTransitionUpdates;
            private int _elementalTransitionCaptured;
            private int _elementalTransitionViews;
            private ElementalRaceBlueprints _elementalTransitionRace;
            private AbilityData _elementalTransitionAbilityData;
            private UnitUseAbility _elementalTransitionAbilityCommand;
            private BlueprintAbility _elementalTransitionAbility;
            private BlueprintAbilityResource _elementalTransitionResource;
            private Buff[] _elementalActorBuffsBefore = new Buff[0];
            private Buff[] _elementalTargetBuffsBefore = new Buff[0];
            private bool _elementalSpellCommandInstalled;
            private bool _elementalSpellCommandStarted;
            private bool _elementalSpellCommandRunning;
            private bool _elementalSpellAnimationObserved;
            private bool _elementalSpellAnimationActed;
            private bool _elementalSpellProcessObserved;
            private bool _elementalSpellProcessEnded;
            private bool _elementalSpellLiveCaptured;
            private bool _elementalSpellCleanupStarted;
            private int _elementalSpellResourceInitial;
            private int _elementalSpellResourceBefore;
            private int _elementalSpellResourceAfter;
            private int _elementalSpellResourceMaximum;
            private int _elementalSpellResourceRecordsBefore;
            private int _elementalSpellResourceRecordsAfter;
            private bool _elementalSpellInitialAvailabilityCaptured;
            private bool _elementalSpellInitialAvailability;
            private bool _elementalSpellFinalAvailability;
            private int _elementalSpellAvailabilitySettleUpdates;
            private string _elementalSpellAvailabilityGates =
                string.Empty;
            private string _elementalSpellTargetPlacement = string.Empty;
            private int _elementalSpellHostilePlacementCount;
            private bool _elementalSpellTargetInState;
            private bool _elementalSpellCommandCanStart;
            private bool _elementalSpellCommandCloseEnough;
            private bool _elementalSpellReadinessProbeDetached;
            private float _elementalSpellTargetDistance;
            private float _elementalSpellApproachRadius;
            private bool _elementalActorCheaterCaptured;
            private bool _elementalActorCheaterBefore;
            private int _elementalDamageBefore;
            private int _elementalHpBefore;
            private int _elementalDamageAfterLethal;
            private int _elementalHpAfterLethal;
            private int _elementalConstitutionAtLethal;
            private bool _elementalDeathCheaterCaptured;
            private bool _elementalDeathCheaterBefore;
            private bool _elementalDeathObserved;
            private bool _elementalProneApplied;
            private bool _elementalProneCaptured;
            private bool _elementalDeathTriggered;
            private bool _elementalDeathCaptured;
            private bool _elementalResurrectionStarted;
            private bool _elementalImmortalityReacquired;
            private bool _elementalPolymorphCaptured;
            private bool _elementalPolymorphRemovalStarted;
            private BlueprintAbility _elementalPolymorphSpell;
            private BlueprintBuff _elementalPolymorphBlueprint;
            private Buff _elementalPolymorphBuff;

            private void BeginElementalRaceTransitions()
            {
                if (!IsElementalRaceMotion ||
                    _elementalTransitionFixtureComplete)
                    throw new InvalidOperationException(
                        "Elemental transitions began outside their exact motion boundary.");
                _motionPhase = 10;
                _elementalTransitionStep = 0;
                _elementalTransitionPhase = 0;
                _elementalTransitionUpdates = 0;
                ResetElementalTransitionAction();
                WriteProgress("elemental-transitions-begin");
            }

            private void PollElementalRaceTransitions()
            {
                if (_elementalTransitionStep < 0 ||
                    _elementalTransitionStep >=
                        ElementalTransitionActions.Length)
                    throw new InvalidOperationException(
                        "Elemental transition index is outside its exact catalog.");
                switch (_elementalTransitionPhase)
                {
                    case 0:
                        PrepareElementalTransition();
                        return;
                    case 1:
                        PollElementalSpellcast();
                        return;
                    case 2:
                        PollElementalProne();
                        return;
                    case 3:
                        PollElementalDeathAndResurrection();
                        return;
                    case 4:
                        PollElementalPolymorphAndReturn();
                        return;
                    default:
                        throw new InvalidOperationException(
                            "Unknown elemental transition phase.");
                }
            }

            private void PrepareElementalTransition()
            {
                string action =
                    ElementalTransitionActions[_elementalTransitionStep];
                _stage = "prepare-elemental-transition-" +
                    _fixtures[_fixtureIndex].Label + "-" + action;
                if (!ElementalTransitionBaselineExact())
                    throw new InvalidOperationException(action +
                        " did not begin from the exact production baseline.");
                ResetElementalTransitionAction();
                _elementalTransitionUpdates = 0;
                _elementalTransitionPhase = _elementalTransitionStep + 1;
                WriteProgress("elemental-transition-prepared");
            }

            private bool ElementalTransitionBaselineExact()
            {
                return _actor != null && _actor.Descriptor != null &&
                    _actor.View != null &&
                    !_actor.Descriptor.State.IsDead &&
                    !_actor.Descriptor.State.HasCondition(
                        UnitCondition.Prone) &&
                    !_actor.Body.IsPolymorphed &&
                    ProductionMotionTransientStateCleared() &&
                    ProductionMotionOutfitExact();
            }

            private ElementalRaceBlueprints ResolveElementalTransitionRace()
            {
                ElementalRaceBlueprintSet set =
                    BlueprintBootstrap.ElementalRaces;
                if (set == null)
                    throw new InvalidOperationException(
                        "Production elemental blueprints are unavailable.");
                ElementalRaceBlueprints result = set.OrderedBlueprints()
                    .SingleOrDefault(value => ReferenceEquals(
                        value.Race, _fixtures[_fixtureIndex].Race));
                if (result == null)
                    throw new InvalidOperationException(
                        "The motion fixture race has no exact elemental blueprint set.");
                return result;
            }

            private TargetWrapper ElementalSpellTarget()
            {
                if (_elementalTransitionRace.Definition.Kind ==
                    ElementalRaceKind.Ifrit)
                {
                    Vector3 forward = _actor.OrientationDirection;
                    forward.y = 0f;
                    if (forward.sqrMagnitude < 0.5f)
                        forward = Vector3.forward;
                    _elementalSpellTargetPlacement =
                        "point-4m-forward";
                    return new TargetWrapper(_actor.Position +
                        forward.normalized * 4f);
                }
                if (_elementalTransitionRace.Definition.Kind ==
                    ElementalRaceKind.Undine)
                {
                    if (_motionTarget == null)
                    {
                        CreateProductionMotionTarget();
                        Vector3 forward = _actor.OrientationDirection;
                        forward.y = 0f;
                        if (forward.sqrMagnitude < 0.5f)
                            forward = Vector3.forward;
                        Vector3 position = NearestNavigable(
                            _actor.Position +
                            forward.normalized * 2f);
                        SetProductionMotionUnitPosition(_motionTarget,
                            position);
                        _actor.ForceLookAt(position);
                        _motionTarget.ForceLookAt(_actor.Position);
                        Game.Instance.EntityCreator.Tick();
                        _elementalSpellTargetPlacement =
                            "hostile-2m-forward-navmesh";
                        _elementalSpellHostilePlacementCount++;
                    }
                    if (_elementalSpellHostilePlacementCount != 1)
                        throw new InvalidOperationException(
                            "The Undine SLA target was not placed exactly once.");
                    return new TargetWrapper(_motionTarget);
                }
                _elementalSpellTargetPlacement = "self";
                return new TargetWrapper(_actor);
            }

            private void StartElementalSpellcast()
            {
                _stage = "start-elemental-racial-sla-" +
                    _fixtures[_fixtureIndex].Label;
                _elementalTransitionRace =
                    ResolveElementalTransitionRace();
                _elementalTransitionAbility =
                    _elementalTransitionRace.SlaAbility;
                _elementalTransitionResource =
                    _elementalTransitionRace.SlaResource;
                if (!_actor.Descriptor.HasFact(
                        _elementalTransitionRace.SlaFeature))
                    _actor.Descriptor.AddFact(
                        _elementalTransitionRace.SlaFeature);
                _elementalSpellResourceInitial = _actor.Descriptor.Resources
                    .GetResourceAmount(_elementalTransitionResource);
                _elementalSpellResourceRecordsBefore = _actor.Descriptor
                    .Resources.PersistantResources.Count(value =>
                        value != null && ReferenceEquals(value.Blueprint,
                            _elementalTransitionResource));
                if (_elementalSpellResourceRecordsBefore == 0)
                    _actor.Descriptor.Resources.Add(
                        _elementalTransitionResource, true);
                int resourceAfterRegistration = _actor.Descriptor.Resources
                    .GetResourceAmount(_elementalTransitionResource);
                if (resourceAfterRegistration < 1)
                    _actor.Descriptor.Resources.Restore(
                        _elementalTransitionResource,
                        1 - resourceAfterRegistration);
                _elementalSpellResourceBefore = _actor.Descriptor.Resources
                    .GetResourceAmount(_elementalTransitionResource);
                _elementalSpellResourceMaximum =
                    _elementalTransitionResource.GetMaxAmount(
                        _actor.Descriptor);
                _elementalSpellResourceRecordsAfter = _actor.Descriptor
                    .Resources.PersistantResources.Count(value =>
                        value != null && ReferenceEquals(value.Blueprint,
                            _elementalTransitionResource));
                if (_elementalSpellResourceBefore != 1 ||
                    _elementalSpellResourceMaximum != 1 ||
                    _elementalSpellResourceRecordsAfter != 1)
                    throw new InvalidOperationException(
                        "The disposable actor's racial SLA resource could not " +
                        "be initialized exactly; initial=" +
                        _elementalSpellResourceInitial + ";prepared=" +
                        _elementalSpellResourceBefore + ";maximum=" +
                        _elementalSpellResourceMaximum + ";records=" +
                        _elementalSpellResourceRecordsBefore + "->" +
                        _elementalSpellResourceRecordsAfter + ".");
                Ability ability = _actor.Descriptor.Abilities.GetAbility(
                    _elementalTransitionAbility);
                if (ability == null)
                    throw new InvalidOperationException(
                        "The disposable actor did not receive its racial SLA.");
                if (!_elementalActorCheaterCaptured)
                {
                    _elementalActorCheaterBefore =
                        _actor.Blueprint.IsCheater;
                    _elementalActorCheaterCaptured = true;
                }
                _actor.Blueprint.IsCheater = false;
                _elementalTransitionAbilityData =
                    new AbilityData(ability);
                bool available = _elementalTransitionAbilityData
                    .IsAvailable;
                int availableCount = _elementalTransitionAbilityData
                    .GetAvailableForCastCount();
                if (!_elementalSpellInitialAvailabilityCaptured)
                {
                    _elementalSpellInitialAvailability = available;
                    _elementalSpellInitialAvailabilityCaptured = true;
                }
                _elementalSpellAvailabilityGates =
                    DescribeElementalSpellAvailability();
                if (availableCount != 1)
                    throw new InvalidOperationException(
                        "The racial SLA did not retain exactly one available " +
                        "resource use while settling; availableCount=" +
                        availableCount + ";resource=" +
                        _elementalSpellResourceBefore + ";maximum=" +
                        _elementalSpellResourceMaximum + ";records=" +
                        _elementalSpellResourceRecordsAfter + ";" +
                        _elementalSpellAvailabilityGates + ".");
                if (!available)
                {
                    if (_elementalSpellAvailabilitySettleUpdates == 0)
                        _diagnostics.Add("elementalSlaReadiness=" +
                            _fixtures[_fixtureIndex].Label + ";update=0;" +
                            "initial=" + _elementalSpellResourceInitial +
                            ";prepared=" + _elementalSpellResourceBefore +
                            ";maximum=" + _elementalSpellResourceMaximum +
                            ";records=" +
                            _elementalSpellResourceRecordsBefore + "->" +
                            _elementalSpellResourceRecordsAfter +
                            ";availableCount=" + availableCount +
                            ";available=False;targetDeferred=True;" +
                            _elementalSpellAvailabilityGates);
                    TickProductionMotionRuntime();
                    _elementalTransitionUpdates++;
                    _elementalSpellAvailabilitySettleUpdates++;
                    RequireElementalTransitionTime(
                        "racial SLA availability (" +
                        _elementalSpellAvailabilityGates + ")");
                    return;
                }
                TargetWrapper target = ElementalSpellTarget();
                _elementalActorBuffsBefore = _actor.Descriptor.Buffs
                    .RawFacts.OfType<Buff>().ToArray();
                _elementalTargetBuffsBefore = _motionTarget == null
                    ? new Buff[0] : _motionTarget.Descriptor.Buffs
                        .RawFacts.OfType<Buff>().ToArray();
                bool targetable = _elementalTransitionAbilityData
                    .CanTarget(target);
                var readiness = new UnitUseAbility(
                    _elementalTransitionAbilityData, target);
                readiness.IgnoreCooldown(TimeSpan.Zero);
                readiness.Init(_actor);
                _elementalSpellReadinessProbeDetached =
                    !_actor.Commands.Contains(readiness);
                if (!_elementalSpellReadinessProbeDetached)
                    throw new InvalidOperationException(
                        "The racial SLA readiness probe became a live command.");
                _elementalSpellTargetInState = target.Unit == null ||
                    target.Unit.IsInState;
                _elementalSpellCommandCanStart = readiness.CanStart;
                _elementalSpellCommandCloseEnough =
                    readiness.IsUnitEnoughClose;
                _elementalSpellApproachRadius = readiness.ApproachRadius;
                _elementalSpellTargetDistance = Vector3.Distance(
                    _actor.Position, target.Point);
                if (_elementalSpellAvailabilitySettleUpdates == 0 ||
                    available)
                    _diagnostics.Add("elementalSlaReadiness=" +
                        _fixtures[_fixtureIndex].Label + ";update=" +
                        _elementalSpellAvailabilitySettleUpdates +
                        ";initial=" + _elementalSpellResourceInitial +
                        ";prepared=" + _elementalSpellResourceBefore +
                        ";maximum=" + _elementalSpellResourceMaximum +
                        ";records=" +
                        _elementalSpellResourceRecordsBefore + "->" +
                        _elementalSpellResourceRecordsAfter +
                        ";availableCount=" + availableCount +
                        ";available=" + available + ";targetable=" +
                        targetable + ";canStart=" +
                        _elementalSpellCommandCanStart +
                        ";targetPlacement=" +
                        _elementalSpellTargetPlacement +
                        ";hostilePlacements=" +
                        _elementalSpellHostilePlacementCount +
                        ";targetInState=" +
                        _elementalSpellTargetInState +
                        ";closeEnough=" +
                        _elementalSpellCommandCloseEnough +
                        ";targetDistance=" +
                        _elementalSpellTargetDistance.ToString("R") +
                        ";approachRadius=" +
                        _elementalSpellApproachRadius.ToString("R") +
                        ";probeDetached=" +
                        _elementalSpellReadinessProbeDetached + ";" +
                        _elementalSpellAvailabilityGates);
                if (availableCount != 1 || !targetable ||
                    !_elementalSpellCommandCanStart ||
                    !_elementalSpellTargetInState ||
                    !_elementalSpellCommandCloseEnough)
                    throw new InvalidOperationException(
                        "The racial SLA native command was not ready; " +
                        "available=" + available + ";targetable=" +
                        targetable + ";canStart=" +
                        _elementalSpellCommandCanStart +
                        ";targetPlacement=" +
                        _elementalSpellTargetPlacement +
                        ";hostilePlacements=" +
                        _elementalSpellHostilePlacementCount +
                        ";targetInState=" +
                        _elementalSpellTargetInState +
                        ";closeEnough=" +
                        _elementalSpellCommandCloseEnough +
                        ";targetDistance=" +
                        _elementalSpellTargetDistance.ToString("R") +
                        ";approachRadius=" +
                        _elementalSpellApproachRadius.ToString("R") +
                        ";probeDetached=" +
                        _elementalSpellReadinessProbeDetached +
                        ";availableCount=" + availableCount +
                        ";resource=" + _elementalSpellResourceBefore +
                        ";maximum=" + _elementalSpellResourceMaximum +
                        ";records=" +
                        _elementalSpellResourceRecordsAfter + ";" +
                        _elementalSpellAvailabilityGates + ".");
                _elementalSpellFinalAvailability = true;
                _elementalTransitionAbilityCommand = new UnitUseAbility(
                    _elementalTransitionAbilityData, target);
                _elementalTransitionAbilityCommand.IgnoreCooldown(
                    TimeSpan.Zero);
                _actor.Commands.Run(
                    _elementalTransitionAbilityCommand);
                _elementalTransitionAbilityCommand.Start();
                ObserveElementalSpellcast();
                if (!_elementalSpellCommandStarted ||
                    !_elementalSpellCommandRunning)
                    throw new InvalidOperationException(
                        "The racial SLA UnitUseAbility did not enter running " +
                        "state; placement=" +
                        _elementalSpellTargetPlacement + ";targetInState=" +
                        _elementalSpellTargetInState + ";closeEnough=" +
                        _elementalSpellCommandCloseEnough + ";distance=" +
                        _elementalSpellTargetDistance.ToString("R") +
                        ";approachRadius=" +
                        _elementalSpellApproachRadius.ToString("R") +
                        ";installed=" +
                        _elementalSpellCommandInstalled + ";started=" +
                        _elementalTransitionAbilityCommand.IsStarted +
                        ";running=" +
                        _elementalTransitionAbilityCommand.IsRunning +
                        ";finished=" +
                        _elementalTransitionAbilityCommand.IsFinished +
                        ";result=" +
                        _elementalTransitionAbilityCommand.Result +
                        ";animation=" +
                        (_elementalTransitionAbilityCommand.Animation != null) +
                        ";process=" +
                        (_elementalTransitionAbilityCommand.ExecutionProcess !=
                            null) + ";processEnded=" +
                        (_elementalTransitionAbilityCommand.ExecutionProcess !=
                            null &&
                         _elementalTransitionAbilityCommand.ExecutionProcess
                            .IsEnded) + ".");
            }

            private void ObserveElementalSpellcast()
            {
                if (_elementalTransitionAbilityCommand == null) return;
                _elementalSpellCommandInstalled |= _actor.Commands
                    .Contains(_elementalTransitionAbilityCommand);
                _elementalSpellCommandStarted |=
                    _elementalTransitionAbilityCommand.IsStarted;
                _elementalSpellCommandRunning |=
                    _elementalTransitionAbilityCommand.IsRunning;
                _elementalSpellAnimationObserved |=
                    _elementalTransitionAbilityCommand.Animation != null;
                _elementalSpellAnimationActed |=
                    _elementalTransitionAbilityCommand.Animation != null &&
                    _elementalTransitionAbilityCommand.Animation.IsActed;
                _elementalSpellProcessObserved |=
                    _elementalTransitionAbilityCommand.ExecutionProcess !=
                        null;
                _elementalSpellProcessEnded |=
                    _elementalTransitionAbilityCommand.ExecutionProcess !=
                        null &&
                    _elementalTransitionAbilityCommand.ExecutionProcess
                        .IsEnded;
            }

            private void PollElementalSpellcast()
            {
                if (_elementalTransitionAbilityCommand == null &&
                    !_elementalSpellCleanupStarted)
                {
                    StartElementalSpellcast();
                    return;
                }
                TickProductionMotionRuntime();
                _elementalTransitionUpdates++;
                if (_elementalSpellCleanupStarted)
                {
                    if (!ElementalTransitionBaselineExact())
                    {
                        RequireElementalTransitionTime("racial SLA cleanup");
                        return;
                    }
                    CaptureElementalTransition(
                        "racial-sla-native-cast-restored",
                        "exact production doll after native racial SLA command cleanup",
                        true, false, false, false);
                    _elementalSpellcastOutcomes.Add(new JObject
                    {
                        { "fixture", _fixtures[_fixtureIndex].Label },
                        { "raceGuid", _elementalTransitionRace.Race.AssetGuid },
                        { "abilityGuid", _elementalTransitionAbility.AssetGuid },
                        { "resourceGuid", _elementalTransitionResource.AssetGuid },
                        { "resourceInitial",
                            _elementalSpellResourceInitial },
                        { "resourceBefore", _elementalSpellResourceBefore },
                        { "resourceAfter", _elementalSpellResourceAfter },
                        { "resourceMaximum",
                            _elementalSpellResourceMaximum },
                        { "resourceRecordsBefore",
                            _elementalSpellResourceRecordsBefore },
                        { "resourceRecordsAfter",
                            _elementalSpellResourceRecordsAfter },
                        { "initialAvailability",
                            _elementalSpellInitialAvailability },
                        { "finalAvailability",
                            _elementalSpellFinalAvailability },
                        { "availabilitySettleUpdates",
                            _elementalSpellAvailabilitySettleUpdates },
                        { "availabilityGates",
                            _elementalSpellAvailabilityGates },
                        { "targetPlacement",
                            _elementalSpellTargetPlacement },
                        { "hostilePlacementCount",
                            _elementalSpellHostilePlacementCount },
                        { "targetInState",
                            _elementalSpellTargetInState },
                        { "commandCanStart",
                            _elementalSpellCommandCanStart },
                        { "commandCloseEnough",
                            _elementalSpellCommandCloseEnough },
                        { "readinessProbeDetached",
                            _elementalSpellReadinessProbeDetached },
                        { "targetDistance",
                            _elementalSpellTargetDistance },
                        { "approachRadius",
                            _elementalSpellApproachRadius },
                        { "resourceDebitBoundary",
                            "request-local actor BlueprintUnit.IsCheater=false" },
                        { "actorCheaterRestored",
                            _actor.Blueprint.IsCheater ==
                                _elementalActorCheaterBefore },
                        { "commandInstalled", _elementalSpellCommandInstalled },
                        { "commandStarted", _elementalSpellCommandStarted },
                        { "commandRunningObserved", _elementalSpellCommandRunning },
                        { "animationObserved", _elementalSpellAnimationObserved },
                        { "animationActedObserved", _elementalSpellAnimationActed },
                        { "executionProcessObserved", _elementalSpellProcessObserved },
                        { "executionProcessEndedObserved", _elementalSpellProcessEnded }
                    });
                    AdvanceElementalTransition();
                    return;
                }

                ObserveElementalSpellcast();
                if (_elementalSpellAnimationActed &&
                    !_elementalSpellLiveCaptured)
                {
                    CaptureElementalTransition(
                        "racial-sla-native-cast-acted",
                        "event-aligned acted frame from the race-owned UnitUseAbility",
                        true, false, false, false);
                    _elementalSpellLiveCaptured = true;
                }
                if (_elementalTransitionAbilityCommand.IsRunning &&
                    _elementalSpellAnimationActed &&
                    _elementalTransitionAbilityCommand.Result ==
                        UnitCommand.ResultType.None)
                    _elementalTransitionAbilityCommand.Tick();
                if (_elementalTransitionAbilityCommand.ExecutionProcess !=
                        null &&
                    !_elementalTransitionAbilityCommand.ExecutionProcess
                        .IsEnded)
                    _elementalTransitionAbilityCommand.ExecutionProcess.Tick();
                ObserveElementalSpellcast();
                if (!_elementalSpellLiveCaptured ||
                    !_elementalSpellProcessObserved ||
                    !_elementalSpellProcessEnded)
                {
                    RequireElementalTransitionTime("racial SLA execution");
                    return;
                }

                _elementalSpellResourceAfter = _actor.Descriptor.Resources
                    .GetResourceAmount(_elementalTransitionResource);
                if (_elementalSpellResourceBefore != 1 ||
                    _elementalSpellResourceAfter != 0)
                    throw new InvalidOperationException(
                        "The live racial SLA did not spend exactly one daily use.");
                _actor.Blueprint.IsCheater =
                    _elementalActorCheaterBefore;
                _actor.Commands.InterruptAll(true);
                RemoveElementalIntroducedBuffs(_actor,
                    _elementalActorBuffsBefore);
                RemoveElementalIntroducedBuffs(_motionTarget,
                    _elementalTargetBuffsBefore);
                RetireProductionMotionTarget();
                if (_actor.CombatState.IsInCombat)
                    _actor.LeaveCombat();
                _elementalSpellCleanupStarted = true;
                _elementalTransitionUpdates = 0;
            }

            private void PollElementalProne()
            {
                _stage = "sample-native-prone-" +
                    _fixtures[_fixtureIndex].Label;
                if (!_elementalProneApplied)
                {
                    _actor.Descriptor.State.AddCondition(
                        UnitCondition.Prone, null);
                    _elementalProneApplied = true;
                    _elementalTransitionUpdates = 0;
                    if (!_actor.Descriptor.State.HasCondition(
                            UnitCondition.Prone))
                        throw new InvalidOperationException(
                            "The disposable actor rejected native Prone.");
                    return;
                }
                TickProductionMotionRuntime();
                _elementalTransitionUpdates++;
                if (!_elementalProneCaptured)
                {
                    if (!_actor.Descriptor.State.HasCondition(
                            UnitCondition.Prone))
                        throw new InvalidOperationException(
                            "Native Prone cleared before its evidence frame.");
                    if (_elementalTransitionUpdates <
                        ElementalTransitionMinimumUpdates) return;
                    CaptureElementalTransition("native-prone",
                        "live UnitCondition.Prone presentation",
                        true, false, false, true);
                    _actor.Descriptor.State.RemoveCondition(
                        UnitCondition.Prone);
                    _elementalProneCaptured = true;
                    _elementalTransitionUpdates = 0;
                    return;
                }
                if (!ElementalTransitionBaselineExact())
                {
                    RequireElementalTransitionTime("native prone recovery");
                    return;
                }
                CaptureElementalTransition("native-prone-restored",
                    "exact production doll after native Prone removal",
                    true, false, false, false);
                _elementalProneOutcomes.Add(new JObject
                {
                    { "fixture", _fixtures[_fixtureIndex].Label },
                    { "proneApplied", _elementalProneApplied },
                    { "proneCaptured", _elementalProneCaptured },
                    { "proneRemoved", !_actor.Descriptor.State.HasCondition(
                        UnitCondition.Prone) },
                    { "productionBaselineRestored",
                        ElementalTransitionBaselineExact() }
                });
                AdvanceElementalTransition();
            }

            private void PollElementalDeathAndResurrection()
            {
                _stage = "sample-native-death-resurrection-" +
                    _fixtures[_fixtureIndex].Label;
                if (!_elementalDeathTriggered)
                {
                    CreateProductionMotionTarget();
                    _elementalDamageBefore = _actor.Descriptor.Damage;
                    _elementalHpBefore = _actor.HPLeft;
                    _elementalConstitutionAtLethal = Math.Max(1,
                        _actor.Descriptor.Stats.Constitution.ModifiedValue);
                    _elementalDeathCheaterBefore =
                        _actor.Blueprint.IsCheater;
                    _elementalDeathCheaterCaptured = true;
                    _actor.Blueprint.IsCheater = false;
                    _actor.Descriptor.State.Immortality.ReleaseAll();
                    var lethal = new RuleDealDamage(_motionTarget, _actor,
                        new DamageBundle(new DirectDamage(
                            new DiceFormula(0, DiceType.D6),
                            _actor.MaxHP +
                                _elementalConstitutionAtLethal + 10)))
                    {
                        DisablePrecisionDamage = true,
                        IgnoreDamageReduction = true
                    };
                    Rulebook.Trigger(lethal);
                    _elementalDamageAfterLethal =
                        _actor.Descriptor.Damage;
                    _elementalHpAfterLethal = _actor.HPLeft;
                    _elementalDeathTriggered = true;
                    _elementalDeathObserved |=
                        _actor.Descriptor.State.IsDead;
                    _diagnostics.Add("elementalLethal=" +
                        _fixtures[_fixtureIndex].Label +
                        ";initiator=request-local-hostile;damage=" +
                        _elementalDamageBefore + "->" +
                        _elementalDamageAfterLethal + ";hp=" +
                        _elementalHpBefore + "->" +
                        _elementalHpAfterLethal + ";dead=" +
                        _elementalDeathObserved + ";actorCheater=" +
                        _elementalDeathCheaterBefore + "->" +
                        _actor.Blueprint.IsCheater + ";constitution=" +
                        _elementalConstitutionAtLethal);
                    _elementalTransitionUpdates = 0;
                    return;
                }
                TickProductionMotionRuntime();
                _elementalTransitionUpdates++;
                _elementalDeathObserved |=
                    _actor.Descriptor.State.IsDead;
                if (!_elementalDeathCaptured)
                {
                    if (!_elementalDeathObserved)
                    {
                        if (_elementalTransitionUpdates >=
                            ElementalTransitionMaximumUpdates)
                            throw new InvalidOperationException(
                                "Native lethal damage did not enter dead state; " +
                                "damage=" + _elementalDamageBefore + "->" +
                                _elementalDamageAfterLethal + ";hp=" +
                                _elementalHpBefore + "->" +
                                _elementalHpAfterLethal +
                                ";constitution=" +
                                _elementalConstitutionAtLethal +
                                ";actorCheater=" +
                                _actor.Blueprint.IsCheater +
                                ";initiatorPresent=" +
                                (_motionTarget != null) + ".");
                        return;
                    }
                    if (_elementalTransitionUpdates <
                        ElementalTransitionMinimumUpdates) return;
                    CaptureElementalTransition("native-death",
                        "live dead-state presentation after native RuleDealDamage",
                        true, false, true, false);
                    _elementalDeathCaptured = true;
                    _actor.Descriptor.ResurrectAndFullRestore();
                    _elementalResurrectionStarted = true;
                    _elementalTransitionUpdates = 0;
                    return;
                }
                if (!_elementalImmortalityReacquired &&
                    !_actor.Descriptor.State.IsDead &&
                    _actor.HPLeft == _actor.MaxHP)
                {
                    if (_elementalDeathCheaterCaptured)
                        _actor.Blueprint.IsCheater =
                            _elementalDeathCheaterBefore;
                    _actor.Descriptor.State.Immortality.Retain();
                    _elementalImmortalityReacquired = true;
                    if (_actor.CombatState.IsInCombat)
                        _actor.LeaveCombat();
                    if (_motionTarget != null &&
                        _motionTarget.CombatState.IsInCombat)
                        _motionTarget.LeaveCombat();
                    RetireProductionMotionTarget();
                }
                if (!_elementalImmortalityReacquired ||
                    !ElementalTransitionBaselineExact())
                {
                    RequireElementalTransitionTime(
                        "native resurrection and doll rebuild");
                    return;
                }
                CaptureElementalTransition("native-resurrected",
                    "exact production doll after ResurrectAndFullRestore",
                    true, false, false, false);
                _elementalDeathOutcomes.Add(new JObject
                {
                    { "fixture", _fixtures[_fixtureIndex].Label },
                    { "damageBefore", _elementalDamageBefore },
                    { "hpBefore", _elementalHpBefore },
                    { "damageAfterLethal",
                        _elementalDamageAfterLethal },
                    { "hpAfterLethal", _elementalHpAfterLethal },
                    { "constitutionAtLethal",
                        _elementalConstitutionAtLethal },
                    { "lethalInitiator",
                        "request-local-hostile" },
                    { "deathObserved", _elementalDeathObserved },
                    { "deathCaptured", _elementalDeathCaptured },
                    { "resurrectionStarted",
                        _elementalResurrectionStarted },
                    { "immortalityReacquired",
                        _elementalImmortalityReacquired },
                    { "actorCheaterRestored",
                        _actor.Blueprint.IsCheater ==
                            _elementalDeathCheaterBefore },
                    { "hpAfter", _actor.HPLeft },
                    { "maxHpAfter", _actor.MaxHP },
                    { "damageAfter", _actor.Descriptor.Damage }
                });
                AdvanceElementalTransition();
            }

            private void PollElementalPolymorphAndReturn()
            {
                _stage = "sample-beast-shape-ii-" +
                    _fixtures[_fixtureIndex].Label;
                if (_elementalPolymorphBuff == null &&
                    !_elementalPolymorphRemovalStarted)
                {
                    _elementalPolymorphSpell = BlueprintLibraryLookup
                        .RequireExact<BlueprintAbility>(
                            BlueprintBootstrap.Library,
                            BeastShapeTwoSpellGuid,
                            "elemental-transition-beast-shape-ii-spell");
                    _elementalPolymorphBlueprint = BlueprintLibraryLookup
                        .RequireExact<BlueprintBuff>(
                            BlueprintBootstrap.Library,
                            BeastShapeTwoBuffGuid,
                            "elemental-transition-beast-shape-ii-buff");
                    var context = new MechanicsContext(_actor,
                        _actor.Descriptor, _elementalPolymorphSpell, null,
                        new TargetWrapper(_actor));
                    context.Params.CasterLevel = 20;
                    _elementalPolymorphBuff = _actor.Descriptor.Buffs
                        .AddBuff(_elementalPolymorphBlueprint, context,
                            TimeSpan.FromMinutes(20d));
                    if (_elementalPolymorphBuff == null)
                        throw new InvalidOperationException(
                            "The exact native Beast Shape II buff was rejected.");
                    _elementalTransitionUpdates = 0;
                    return;
                }
                TickProductionMotionRuntime();
                _elementalTransitionUpdates++;
                if (!_elementalPolymorphCaptured)
                {
                    if (!_actor.Body.IsPolymorphed ||
                        ActiveRenderers(_actor).Length == 0)
                    {
                        RequireElementalTransitionTime(
                            "native Beast Shape II presentation");
                        return;
                    }
                    if (_elementalTransitionUpdates <
                        ElementalTransitionMinimumUpdates) return;
                    CaptureElementalTransition(
                        "beast-shape-ii-polymorphed",
                        "live native Beast Shape II body replacement",
                        false, true, false, false);
                    _elementalPolymorphCaptured = true;
                    _elementalPolymorphBuff.Remove();
                    _elementalPolymorphRemovalStarted = true;
                    _elementalTransitionUpdates = 0;
                    return;
                }
                if (_actor.Body.IsPolymorphed ||
                    !ElementalTransitionBaselineExact())
                {
                    RequireElementalTransitionTime(
                        "native Beast Shape II return");
                    return;
                }
                CaptureElementalTransition(
                    "beast-shape-ii-restored",
                    "exact production doll after native polymorph removal",
                    true, false, false, false);
                _elementalPolymorphOutcomes.Add(new JObject
                {
                    { "fixture", _fixtures[_fixtureIndex].Label },
                    { "spellGuid", _elementalPolymorphSpell.AssetGuid },
                    { "buffGuid",
                        _elementalPolymorphBlueprint.AssetGuid },
                    { "polymorphCaptured",
                        _elementalPolymorphCaptured },
                    { "buffRemoved",
                        _elementalPolymorphRemovalStarted },
                    { "bodyReturned", !_actor.Body.IsPolymorphed },
                    { "productionBaselineRestored",
                        ElementalTransitionBaselineExact() }
                });
                AdvanceElementalTransition();
            }

            private void CaptureElementalTransition(string state,
                string claim, bool requireProductionOutfit,
                bool expectPolymorphed, bool expectDead, bool expectProne)
            {
                _stage = "capture-elemental-transition-" +
                    _fixtures[_fixtureIndex].Label + "-" + state;
                bool productionExact = ProductionMotionOutfitExact();
                bool polymorphed = _actor.Body.IsPolymorphed;
                bool dead = _actor.Descriptor.State.IsDead;
                bool prone = _actor.Descriptor.State.HasCondition(
                    UnitCondition.Prone);
                if ((requireProductionOutfit && !productionExact) ||
                    polymorphed != expectPolymorphed ||
                    dead != expectDead || prone != expectProne ||
                    !ProductionMotionPlayerBoundaryExact())
                    throw new InvalidOperationException(state +
                        " crossed its exact transition boundary.");
                Renderer[] renderers = ActiveRenderers(_actor);
                int materialSlots = renderers.Sum(value =>
                    value.sharedMaterials == null ? 0 :
                        value.sharedMaterials.Length);
                int nullMaterials = renderers.Sum(value =>
                    value.sharedMaterials == null ? 0 :
                        value.sharedMaterials.Count(material =>
                            material == null));
                int nullShaders = renderers.Sum(value =>
                    value.sharedMaterials == null ? 0 :
                        value.sharedMaterials.Count(material =>
                            material != null && material.shader == null));
                if (renderers.Length == 0 || materialSlots == 0 ||
                    nullMaterials != 0 || nullShaders != 0)
                    throw new InvalidOperationException(state +
                        " exposed an incomplete renderer/material/shader set.");
                string stem = SafeFileName("elemental-transition-" +
                    _fixtures[_fixtureIndex].Label + "-" + state);
                string pngPath = Path.Combine(_request.EvidenceDirectory,
                    stem + ".png");
                string jsonPath = Path.Combine(_request.EvidenceDirectory,
                    stem + ".json");
                WeaponPresentationEvidenceScenario.CaptureSummary capture =
                    WeaponPresentationEvidenceScenario.CaptureContactSheet(
                        _actor, null, renderers, pngPath, true);
                var record = new JObject
                {
                    { "schemaVersion", 1 },
                    { "scenario", _request.Scenario },
                    { "fixture", _fixtures[_fixtureIndex].Label },
                    { "gender", _fixtures[_fixtureIndex].Gender.ToString() },
                    { "raceGuid", _fixtures[_fixtureIndex].Race.AssetGuid },
                    { "raceId",
                        _fixtures[_fixtureIndex].Race.RaceId.ToString() },
                    { "action",
                        ElementalTransitionActions[
                            _elementalTransitionStep] },
                    { "state", state },
                    { "claimBoundary", claim },
                    { "updates", _elementalTransitionUpdates },
                    { "polymorphed", polymorphed },
                    { "dead", dead },
                    { "prone", prone },
                    { "productionOutfitExact", productionExact },
                    { "productionBlueprintUnchanged",
                        ProductionBlueprintUnchanged() },
                    { "activeRendererCount", renderers.Length },
                    { "materialSlotCount", materialSlots },
                    { "nullMaterialCount", nullMaterials },
                    { "nullShaderCount", nullShaders },
                    { "playerBoundaryExact",
                        ProductionMotionPlayerBoundaryExact() },
                    { "playerCharacterListsExact",
                        ProductionMotionPlayerListsExact() },
                    { "saveApiCalled", false },
                    { "preview", CaptureSummaryJson(capture) }
                };
                WriteJsonAtomic(jsonPath, record);
                _elementalTransitionRecords.Add(record);
                _evidenceFiles.Add(capture.PngPath);
                _evidenceFiles.Add(jsonPath);
                _elementalTransitionCaptured++;
                _elementalTransitionViews += 4;
            }

            private static JObject CaptureSummaryJson(
                WeaponPresentationEvidenceScenario.CaptureSummary value)
            {
                return new JObject
                {
                    { "file", Path.GetFileName(value.PngPath) },
                    { "bytes", value.Bytes },
                    { "sha256", value.Sha256 },
                    { "meaningfulPixels", value.MeaningfulPixels },
                    { "framing", value.Framing },
                    { "lowPixelDensity", value.LowPixelDensity }
                };
            }

            private void RequireElementalTransitionTime(string label)
            {
                if (_elementalTransitionUpdates <
                    ElementalTransitionMaximumUpdates) return;
                throw new InvalidOperationException(label +
                    " did not settle inside " +
                    ElementalTransitionMaximumUpdates + " updates.");
            }

            private static void RemoveElementalIntroducedBuffs(
                UnitEntityData unit, Buff[] before)
            {
                if (unit == null || unit.Descriptor == null) return;
                Buff[] baseline = before ?? new Buff[0];
                foreach (Buff buff in unit.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Where(value => !baseline.Any(
                        prior => ReferenceEquals(prior, value))).ToArray())
                    buff.Remove();
            }

            private void AdvanceElementalTransition()
            {
                ResetElementalTransitionAction();
                _elementalTransitionStep++;
                _elementalTransitionPhase = 0;
                _elementalTransitionUpdates = 0;
                if (_elementalTransitionStep <
                    ElementalTransitionActions.Length)
                {
                    WriteProgress("elemental-transition-advanced");
                    return;
                }
                _elementalTransitionFixtureComplete = true;
                WriteProgress("elemental-transitions-complete");
                FinishProductionMotionFixture();
            }

            private void ResetElementalTransitionAction()
            {
                _elementalTransitionRace = null;
                _elementalTransitionAbilityData = null;
                _elementalTransitionAbilityCommand = null;
                _elementalTransitionAbility = null;
                _elementalTransitionResource = null;
                _elementalActorBuffsBefore = new Buff[0];
                _elementalTargetBuffsBefore = new Buff[0];
                _elementalSpellCommandInstalled = false;
                _elementalSpellCommandStarted = false;
                _elementalSpellCommandRunning = false;
                _elementalSpellAnimationObserved = false;
                _elementalSpellAnimationActed = false;
                _elementalSpellProcessObserved = false;
                _elementalSpellProcessEnded = false;
                _elementalSpellLiveCaptured = false;
                _elementalSpellCleanupStarted = false;
                _elementalSpellResourceInitial = 0;
                _elementalSpellResourceBefore = 0;
                _elementalSpellResourceAfter = 0;
                _elementalSpellResourceMaximum = 0;
                _elementalSpellResourceRecordsBefore = 0;
                _elementalSpellResourceRecordsAfter = 0;
                _elementalSpellInitialAvailabilityCaptured = false;
                _elementalSpellInitialAvailability = false;
                _elementalSpellFinalAvailability = false;
                _elementalSpellAvailabilitySettleUpdates = 0;
                _elementalSpellAvailabilityGates = string.Empty;
                _elementalSpellTargetPlacement = string.Empty;
                _elementalSpellHostilePlacementCount = 0;
                _elementalSpellTargetInState = false;
                _elementalSpellCommandCanStart = false;
                _elementalSpellCommandCloseEnough = false;
                _elementalSpellReadinessProbeDetached = false;
                _elementalSpellTargetDistance = 0f;
                _elementalSpellApproachRadius = 0f;
                _elementalActorCheaterCaptured = false;
                _elementalActorCheaterBefore = false;
                _elementalDamageBefore = 0;
                _elementalHpBefore = 0;
                _elementalDamageAfterLethal = 0;
                _elementalHpAfterLethal = 0;
                _elementalConstitutionAtLethal = 0;
                _elementalDeathCheaterCaptured = false;
                _elementalDeathCheaterBefore = false;
                _elementalDeathObserved = false;
                _elementalProneApplied = false;
                _elementalProneCaptured = false;
                _elementalDeathTriggered = false;
                _elementalDeathCaptured = false;
                _elementalResurrectionStarted = false;
                _elementalImmortalityReacquired = false;
                _elementalPolymorphCaptured = false;
                _elementalPolymorphRemovalStarted = false;
                _elementalPolymorphSpell = null;
                _elementalPolymorphBlueprint = null;
                _elementalPolymorphBuff = null;
            }

            private string DescribeElementalSpellAvailability()
            {
                IAbilityCasterChecker[] checkers =
                    _elementalTransitionAbility.CasterCheckers ??
                        new IAbilityCasterChecker[0];
                IAbilityAvailabilityProvider[] providers =
                    _elementalTransitionAbility.AvailabilityProviders ??
                        new IAbilityAvailabilityProvider[0];
                string checkerResults = string.Join("|", checkers.Select(
                    value => value.GetType().FullName + ":" +
                        value.CorrectCaster(_actor)));
                string providerResults = string.Join("|", providers.Select(
                    value => value.GetType().FullName + ":" +
                        value.IsAvailableFor(
                            _elementalTransitionAbilityData)));
                return "factPresent=" +
                    (_elementalTransitionAbilityData.Fact != null) +
                    ";factActive=" +
                    (_elementalTransitionAbilityData.Fact != null &&
                        _elementalTransitionAbilityData.Fact.Active) +
                    ";hasRequiredParams=" +
                    _elementalTransitionAbilityData.HasRequiredParams +
                    ";isAvailableForCast=" +
                    _elementalTransitionAbilityData.IsAvailableForCast +
                    ";spellbookPresent=" +
                    (_elementalTransitionAbilityData.Spellbook != null) +
                    ";sourceItemPresent=" +
                    (_elementalTransitionAbilityData.SourceItem != null) +
                    ";spellCastingForbidden=" +
                    (bool)_actor.Descriptor.State.SpellCastingForbidden +
                    ";magicItemsForbidden=" +
                    (bool)_actor.Descriptor.State.MagicItemsForbidden +
                    ";exAndSuForbidden=" +
                    (bool)_actor.Descriptor.State.ExAndSuAbilitiesForbidden +
                    ";polymorphed=" + _actor.Body.IsPolymorphed +
                    ";casterCheckers=" + checkerResults +
                    ";availabilityProviders=" + providerResults;
            }

            private void CleanupElementalRaceTransitions()
            {
                if (!IsElementalRaceMotion) return;
                if (_actor != null && _actor.Descriptor != null)
                {
                    _actor.Commands.InterruptAll(true);
                    if (_elementalActorCheaterCaptured)
                        _actor.Blueprint.IsCheater =
                            _elementalActorCheaterBefore;
                    if (_elementalDeathCheaterCaptured)
                        _actor.Blueprint.IsCheater =
                            _elementalDeathCheaterBefore;
                    RemoveElementalIntroducedBuffs(_actor,
                        _elementalActorBuffsBefore);
                    if (_actor.Descriptor.State.HasCondition(
                            UnitCondition.Prone))
                        _actor.Descriptor.State.RemoveCondition(
                            UnitCondition.Prone);
                    if (_elementalPolymorphBuff != null &&
                        _actor.Descriptor.Buffs.RawFacts.OfType<Buff>()
                            .Any(value => ReferenceEquals(value,
                                _elementalPolymorphBuff)))
                        _elementalPolymorphBuff.Remove();
                    if (_actor.Descriptor.State.IsDead)
                        _actor.Descriptor.ResurrectAndFullRestore();
                    if (_elementalDeathTriggered &&
                        !_elementalImmortalityReacquired)
                    {
                        _actor.Descriptor.State.Immortality.Retain();
                        _elementalImmortalityReacquired = true;
                    }
                    if (_actor.CombatState.IsInCombat)
                        _actor.LeaveCombat();
                }
                RemoveElementalIntroducedBuffs(_motionTarget,
                    _elementalTargetBuffsBefore);
                RetireProductionMotionTarget();
            }
        }
    }
}
