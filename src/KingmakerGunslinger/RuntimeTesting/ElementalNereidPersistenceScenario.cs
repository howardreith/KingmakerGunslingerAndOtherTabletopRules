using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Rest;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Pathfinding;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private readonly bool _nereidPersistence;
            private string _nereidSavedRestCaster;
            private readonly JArray _nereidPersistenceRecords = new JArray();
            private readonly HashSet<AreaEffectEntityData> _nereidLoadedAreas = new HashSet<AreaEffectEntityData>();
            private bool IsCompletionSavedAreaFixture(object value)
            {
                return _nereidPersistence && value is AreaEffectEntityData &&
                    (_nereidLoadedAreas.Contains((AreaEffectEntityData)value) ||
                        (TreacherousPersistence && _treacherousLoadedAreas.Contains((AreaEffectEntityData)value)));
            }
            private void DrainNereidCleanupAreas()
            {
                var areaBlueprint = NereidPersistenceTrait.Mechanics().OfType<BlueprintAbilityAreaEffect>().Single();
                var ended = Game.Instance.State.AreaEffects.All.Where(value =>
                    ReferenceEquals(value.Blueprint, areaBlueprint) && value.IsEnded).ToArray();
                if (ended.Any(value => !IsFixtureUnit(value.Context?.MaybeCaster) && !_nereidLoadedAreas.Contains(value)))
                    throw new InvalidOperationException("An unowned ended aura entered Nereid fixture cleanup.");
                foreach (var area in ended) TickNereidNativeArea(area);
                if (ended.Length != 0) DrainNereidAreaDestruction(ended[0]);
            }

            private ElementalAlternateTraitBlueprints NereidPersistenceTrait =>
                _blueprintSet.Undine.AlternateTraits.Require(ElementalAlternateTraitId.NereidFascination);
            private BlueprintBuff NereidAuraBlueprint => NereidPersistenceTrait.Mechanics().OfType<BlueprintBuff>()
                .Single(value => value.GetComponent<ElementalNereidAuraState>() != null);
            private BlueprintBuff NereidTargetBlueprint => NereidPersistenceTrait.Mechanics().OfType<BlueprintBuff>()
                .Single(value => value.GetComponent<ElementalNereidFascinated>() != null);
            private BlueprintBuff NereidAssistanceBlueprint => NereidPersistenceTrait.Mechanics().OfType<BlueprintBuff>()
                .Single(value => value.GetComponent<ElementalNereidAssistance>() != null);

            private sealed class NereidPersistenceSaves : IGlobalRulebookHandler<RuleSavingThrow>
            {
                internal readonly List<RuleSavingThrow> Events = new List<RuleSavingThrow>();
                public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
                public void OnEventDidTrigger(RuleSavingThrow evt) { Events.Add(evt); }
            }

            private static AreaEffectEntityData NereidNativeArea(Buff source)
            {
                var attached = source.SelectComponents<AddAreaEffect>().Single();
                var field = typeof(AddAreaEffect).GetField("m_AreaEffectInstance", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null) throw new MissingFieldException(typeof(AddAreaEffect).FullName, "m_AreaEffectInstance");
                return (AreaEffectEntityData)field.GetValue(attached);
            }
            private static JObject ObserveNereidNativeMembership(AreaEffectEntityData area, UnitEntityData[] targets)
            {
                var shape = area.View?.Shape;
                if (shape == null || Game.Instance.CurrentScene?.Area?.InteractiveObjectGrid == null)
                    throw new InvalidOperationException("The saved native area has no exact shape/spatial-grid boundary.");
                var bounds = shape.GetBounds();
                var candidates = new List<Kingmaker.EntitySystem.EntityDataBase>();
                Game.Instance.CurrentScene.Area.InteractiveObjectGrid.GetInBounds(bounds, candidates);
                var predicate = typeof(AreaEffectEntityData).GetMethod("ShouldUnitBeInside", BindingFlags.Instance | BindingFlags.NonPublic);
                if (predicate == null) throw new MissingMethodException(typeof(AreaEffectEntityData).FullName, "ShouldUnitBeInside");
                var geometry = Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance;
                if (geometry == null) throw new InvalidOperationException("Native line-of-effect geometry is absent.");
                return new JObject { ["areaId"] = area.UniqueId,
                    ["position"] = new JArray(area.Position.x, area.Position.y, area.Position.z),
                    ["boundsCenter"] = new JArray(bounds.center.x, bounds.center.y, bounds.center.z),
                    ["boundsSize"] = new JArray(bounds.size.x, bounds.size.y, bounds.size.z),
                    ["gridCandidates"] = candidates.Count, ["insideCount"] = area.UnitsInside.Count(),
                    ["targets"] = new JArray(targets.Select(unit => new JObject {
                        ["id"] = unit.UniqueId, ["inGame"] = unit.IsInGame, ["sleeping"] = unit.IsSleeping,
                        ["dead"] = unit.Descriptor.State.IsDead, ["inStatePool"] = Game.Instance.State.Units.All.Contains(unit),
                        ["inSpatialBounds"] = candidates.Contains(unit),
                        ["shapeContains"] = shape.Contains(unit.Position, unit.View.Corpulence),
                        ["lineObstacle"] = geometry.HasObstacle(area.View.transform.position, unit.Position, 0),
                        ["nativeShouldBeInside"] = (bool)predicate.Invoke(area, new object[] { unit }),
                        ["position"] = new JArray(unit.Position.x, unit.Position.y, unit.Position.z),
                        ["viewPosition"] = new JArray(unit.View.transform.position.x, unit.View.transform.position.y, unit.View.transform.position.z),
                        ["fascinatedFacts"] = unit.Buffs.Enumerable.Count(value =>
                            value.Blueprint.AssetGuid == "e118e1e0a17a4acec001000000000003" && value.Active),
                        ["dazed"] = unit.Descriptor.State.HasCondition(UnitCondition.Dazed) })) };
            }
            private void SynchronizeNereidFixturePositions(UnitEntityData[] actors, JObject record)
            {
                // The paused save harness can inspect a restored unit before the
                // native move controller has reconciled its spatial-grid entry.
                // Run that ordinary standing-unit phase before area membership.
                // Only these exact disposable actors participate, at zero time.
                var prerequisites = new JObject { ["paused"] = Game.Instance.IsPaused,
                    ["actors"] = new JArray(actors.Select(unit => new JObject {
                        ["id"] = unit.UniqueId, ["fixture"] = IsFixtureUnit(unit), ["inGame"] = unit.IsInGame,
                        ["inCombat"] = unit.IsInCombat, ["agent"] = unit.View?.MovementAgent != null,
                        ["moving"] = unit.View?.MovementAgent?.IsReallyMoving,
                        ["position"] = new JArray(unit.Position.x, unit.Position.y, unit.Position.z),
                        ["viewPosition"] = unit.View == null ? null : new JArray(unit.View.transform.position.x,
                            unit.View.transform.position.y, unit.View.transform.position.z) })) };
                record["nativeStandingPositionPrerequisites"] = prerequisites;
                if (!Game.Instance.IsPaused || actors.Length == 0 || actors.Distinct().Count() != actors.Length ||
                    actors.Any(unit => !IsFixtureUnit(unit) || !unit.IsInGame || unit.IsInCombat ||
                        unit.View?.MovementAgent == null || unit.View.MovementAgent.IsReallyMoving ||
                        !unit.Position.Equals(unit.View.transform.position)))
                    throw new InvalidOperationException("Native position reconciliation requires exact paused standing fixture actors: " +
                        prerequisites.ToString(Formatting.None));
                var awake = Game.Instance.State.AwakeUnits.ToArray();
                var world = Game.Instance.State.Units.All.ToArray();
                var foreign = world.Where(unit => !actors.Contains(unit)).ToArray();
                var positions = world.Select(unit => unit.Position).ToArray();
                var views = world.Select(unit => unit.View?.transform.position).ToArray();
                var previous = world.Select(unit => unit.PreviousPosition).ToArray();
                var facts = foreign.Select(unit => unit.Buffs.Enumerable.ToArray()).ToArray();
                float delta = Game.Instance.TimeController.DeltaTime;
                float gameDelta = Game.Instance.TimeController.GameDeltaTime;
                var clock = Game.Instance.TimeController.GameTime;
                try {
                    Game.Instance.State.AwakeUnits.Clear();
                    Game.Instance.State.AwakeUnits.AddRange(actors);
                    Game.Instance.TimeController.SetDeltaTime(0);
                    Game.Instance.TimeController.SetGameDeltaTime(0);
                    new UnitMoveController().Tick();
                }
                finally {
                    Game.Instance.State.AwakeUnits.Clear(); Game.Instance.State.AwakeUnits.AddRange(awake);
                    Game.Instance.TimeController.SetDeltaTime(delta);
                    Game.Instance.TimeController.SetGameDeltaTime(gameDelta);
                }
                bool exact = Game.Instance.IsPaused && Game.Instance.TimeController.GameTime == clock &&
                    Game.Instance.State.AwakeUnits.SequenceEqual(awake) && Game.Instance.State.Units.All.SequenceEqual(world) &&
                    world.Select((unit, index) => unit.Position.Equals(positions[index]) &&
                        Nullable.Equals(unit.View?.transform.position, views[index]) &&
                        (actors.Contains(unit) || unit.PreviousPosition.Equals(previous[index]))).All(value => value) &&
                    foreign.Select((unit, index) => unit.Buffs.Enumerable.SequenceEqual(facts[index])).All(value => value);
                record["nativeStandingPositionReconciliation"] = new JObject {
                    ["actors"] = new JArray(actors.Select(unit => unit.UniqueId)), ["deltaTime"] = 0,
                    ["clockAndPositionsAndForeignFactsExact"] = exact, ["awakeRestored"] = true };
                if (!exact) throw new InvalidOperationException("Native standing position reconciliation changed the isolated boundary.");
            }
            private static void TickNereidNativeArea(AreaEffectEntityData area)
            {
                if (area == null) throw new InvalidOperationException("The exact native saved aura area is absent.");
                var update = typeof(AreaEffectEntityData).GetMethod("UpdateUnits", BindingFlags.Instance | BindingFlags.NonPublic);
                if (update == null) throw new MissingMethodException(typeof(AreaEffectEntityData).FullName, "UpdateUnits");
                update.Invoke(area, null);
                area.Tick();
                Game.Instance.EntityCreator.Tick();
            }
            private void EndNereidPersistenceAura(Buff source)
            {
                var area = NereidNativeArea(source);
                source.Remove();
                TickNereidNativeArea(area);
                DrainNereidAreaDestruction(area);
            }
            private void DrainNereidAreaDestruction(AreaEffectEntityData area)
            {
                // Area.Tick requests destruction; the separate native destroyer
                // removes it from its owning scene. This paused fixture must
                // drive that stage explicitly, with no foreign pending work.
                var pending = new List<Kingmaker.EntitySystem.EntityDataBase>();
                var controller = typeof(Kingmaker.Controllers.EntityDestructionController);
                var flags = BindingFlags.Static | BindingFlags.NonPublic;
                var listType = typeof(List<Kingmaker.EntitySystem.EntityDataBase>);
                var areaCollector = controller.GetMethod("AddForDestruction", flags, null,
                    new[] { typeof(Kingmaker.EntitySystem.AreaPersistentState), listType }, null);
                var sceneCollector = controller.GetMethod("AddForDestruction", flags, null,
                    new[] { typeof(Kingmaker.EntitySystem.SceneEntitiesState), listType }, null);
                if (areaCollector == null || sceneCollector == null || Game.Instance.EntityDestroyer == null)
                    throw new MissingMethodException("The exact native area destruction boundary is unavailable.");
                areaCollector.Invoke(null, new object[] { Game.Instance.State.LoadedAreaState, pending });
                sceneCollector.Invoke(null, new object[] { Game.Instance.Player.CrossSceneState, pending });
                if (!pending.Contains(area) || pending.Any(value => !(value is AreaEffectEntityData) ||
                    !ReferenceEquals(((AreaEffectEntityData)value).Blueprint, area.Blueprint) ||
                    !((AreaEffectEntityData)value).IsEnded || !value.ShouldBeDestroyed ||
                    (!IsFixtureUnit(((AreaEffectEntityData)value).Context?.MaybeCaster) &&
                        !IsCompletionSavedAreaFixture(value))))
                    throw new InvalidOperationException("Native destruction contains work outside the exact ended Nereid fixtures.");
                var unitsBefore = Game.Instance.State.Units.All.ToArray();
                var survivingAreas = Game.Instance.State.AreaEffects.All.Where(value => !pending.Contains(value)).ToArray();
                Game.Instance.EntityDestroyer.Tick();
                bool exact = area.Destroyed && !Game.Instance.State.AreaEffects.All.Contains(area) &&
                    unitsBefore.SequenceEqual(Game.Instance.State.Units.All) &&
                    survivingAreas.SequenceEqual(Game.Instance.State.AreaEffects.All);
                _nereidPersistenceRecords.Add(new JObject { ["kind"] = "native-ended-area-destruction",
                    ["areaId"] = area.UniqueId, ["pendingOwnedCount"] = pending.Count, ["exact"] = exact });
                if (!exact) throw new InvalidOperationException("Native ended-area destruction did not preserve the exact foreign pools.");
            }
            private UnitEntityData[] NereidSavedTargets()
            {
                var units = Snapshot(_allUnits).OfType<UnitEntityData>().ToArray();
                return new[] { Gender.Male, Gender.Female }.Select(sex => {
                    var fixture = _fixtures.Single(value => value.Blueprints.AlternateTraits.Race == ElementalHeritageRace.Ifrit &&
                        value.Gender == sex && value.Heritage.Definition.Id == ElementalHeritageId.GeneralIfrit);
                    var unit = units.Single(value => IsFixtureUnit(value, fixture));
                    if (unit.View == null || unit.Descriptor.State.IsDead || unit.IsInCombat)
                        throw new InvalidOperationException("Exact disposable Nereid save targets must be alive and ready.");
                    return unit;
                }).ToArray();
            }
            private Vector3 NereidPersistencePosition(UnitEntityData caster, UnitEntityData[] targets)
            {
                if (AstarPath.active == null)
                    throw new InvalidOperationException("Nereid persistence requires a real native scene navigation graph.");
                var other = Game.Instance.State.Units.All.Where(value => !ReferenceEquals(value, caster) && !targets.Contains(value)).ToArray();
                foreach (float radius in new[] { 8f, 14f, 20f, 28f })
                    for (int direction = 0; direction < 32; direction++)
                    {
                        float angle = direction * Mathf.PI / 16;
                        Vector3 requested = _fixtureStagingPosition + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                        var nearest = AstarPath.active.GetNearest(requested);
                        if (nearest.node == null || !nearest.node.Walkable || Vector3.Distance(nearest.clampedPosition, requested) > 1) continue;
                        Vector3 point = nearest.clampedPosition;
                        if (!other.All(value => Vector3.Distance(value.Position, point) > 7f + Math.Max(0, value.Corpulence))) continue;
                        bool targetGround = new[] { -.8f, .8f }.All(offset => {
                            var ground = AstarPath.active.GetNearest(point + new Vector3(0, 0, offset));
                            return ground.node != null && ground.node.Walkable &&
                                Vector3.Distance(ground.clampedPosition, point + new Vector3(0, 0, offset)) < .25f;
                        });
                        if (targetGround) return point;
                    }
                throw new InvalidOperationException("No bounded native scene location isolates the two owned Nereid targets.");
            }
            private static void PlaceNereidPersistenceUnit(UnitEntityData unit, Vector3 point)
            {
                unit.Position = point;
                if (unit.View != null) unit.View.transform.position = point;
            }

            private void EnsureNereidPersistencePause()
            {
                EnsurePreparePersistencePause();
                // Native preparation/capture phases can restore an earlier pause
                // value while this fixture still owns the outer pause scope.
                // Preserve that scope's original value, but require its actual
                // paused state again before bounded rest/cast/save preparation.
                if (Game.Instance.IsPaused) return;
                Game.Instance.IsPaused = true;
                bool paused = Game.Instance.IsPaused;
                _nereidPersistenceRecords.Add(new JObject { ["kind"] = "reassert-owned-fixture-pause",
                    ["outerScopeOwned"] = _prepareFeatPauseApplied, ["paused"] = paused });
                if (!paused || !_prepareFeatPauseApplied)
                    throw new InvalidOperationException("The exact Nereid fixture could not reassert its owned pause scope.");
            }

            private void SpendNereidForPersistence(ElementalPersistenceFixture fixture, UnitEntityData unit,
                AbilityData data, string phase, bool keepForSave)
            {
                if (!_nereidPersistence || !IsFixtureUnit(unit, fixture) || unit.IsInCombat ||
                    PersistenceSlaTrait(fixture, fixture.Heritage)?.Definition.Id != ElementalAlternateTraitId.NereidFascination)
                    throw new InvalidOperationException("Nereid persistence requires the exact scoped disposable native-selected caster.");
                EnsureNereidPersistencePause();
                var resource = PersistenceSlaResource(fixture, fixture.Heritage);
                if (data == null || resource == null || unit.Descriptor.Resources.GetResourceAmount(resource) != 1 || !data.IsAvailable)
                    throw new InvalidOperationException("An accepted Nereid persistence command requires exactly one ordinary-rest use.");
                foreach (var prior in unit.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, NereidAuraBlueprint)).ToArray())
                    EndNereidPersistenceAura(prior);
                UnitEntityData[] targets = keepForSave ? NereidSavedTargets() : new UnitEntityData[0];
                UnitEntityData[] moved = new[] { unit }.Concat(targets).ToArray();
                Vector3[] positions = moved.Select(value => value.Position).ToArray();
                int[] wills = targets.Select(value => value.Stats.SaveWill.BaseValue).ToArray();
                var foreign = Game.Instance.State.Units.All.Where(value => !IsFixtureUnit(value)).ToArray();
                var foreignBuffs = foreign.Select(value => value.Buffs.Enumerable.ToArray()).ToArray();
                var foreignPositions = foreign.Select(value => value.Position).ToArray();
                int[] foreignDamage = foreign.Select(value => value.Damage).ToArray();
                TimeSpan clock = Game.Instance.TimeController.GameTime;
                var random = UnityEngine.Random.state;
                var saves = new NereidPersistenceSaves();
                UnitUseAbility command = null;
                Buff source = null;
                bool success = false, subscribed = false;
                var record = new JObject { ["fixture"] = fixture.Label, ["phase"] = phase,
                    ["kind"] = "native-nereid-command", ["savedActivation"] = keepForSave,
                    ["cooldownsBeforeTimingIsolation"] = new JArray(unit.CombatState.Cooldown.StandardAction,
                        unit.CombatState.Cooldown.MoveAction, unit.CombatState.Cooldown.SwiftAction) };
                try
                {
                    Vector3 point = NereidPersistencePosition(unit, targets);
                    PlaceNereidPersistenceUnit(unit, point);
                    for (int index = 0; index < targets.Length; index++)
                    {
                        PlaceNereidPersistenceUnit(targets[index], point + new Vector3(0, 0, index == 0 ? -.8f : .8f));
                        targets[index].Stats.SaveWill.BaseValue = index == 0 ? -100 : 100;
                    }
                    SynchronizeNereidFixturePositions(moved, record);
                    // Only the owned actor's starting turn timing is isolated.
                    // Native UnitActionController.UpdateCooldowns returns without
                    // charging an actor outside combat. Combat RTWP/TB expenditure
                    // is qualified independently by the bounded action scenario.
                    unit.CombatState.Cooldown.StandardAction = 0;
                    unit.CombatState.Cooldown.MoveAction = 0;
                    unit.CombatState.Cooldown.SwiftAction = 0;
                    var self = new TargetWrapper(unit);
                    var canceled = new UnitUseAbility(data, self);
                    unit.Commands.Run(canceled);
                    bool queued = unit.Commands.Contains(canceled);
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                    bool canceledExact = queued && !canceled.IsStarted && !canceled.IsActed &&
                        unit.Descriptor.Resources.GetResourceAmount(resource) == 1 && unit.CombatState.Cooldown.StandardAction == 0;
                    EventBus.Subscribe(saves); subscribed = true;
                    UnityEngine.Random.InitState(7419);
                    command = new UnitUseAbility(data, self); unit.Commands.Run(command);
                    var controller = new UnitActionController();
                    for (int tick = 0; !command.IsActed && !command.IsFinished && tick < 10; tick++)
                    {
                        if (command.Animation != null) command.Animation.IsActed = true;
                        ElementalBreathScenario.TickCommand(controller, command);
                    }
                    for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 100; tick++)
                        command.ExecutionProcess.Tick();
                    source = unit.Buffs.Enumerable.SingleOrDefault(value => ReferenceEquals(value.Blueprint, NereidAuraBlueprint));
                    if (source == null) throw new InvalidOperationException("The accepted native Nereid command did not create its aura.");
                    var area = NereidNativeArea(source);
                    Game.Instance.EntityCreator.Tick(); TickNereidNativeArea(area);
                    var state = source.SelectComponents<ElementalNereidAuraState>().Single();
                    int level = unit.Descriptor.Progression.CharacterLevel;
                    bool targetsExact = targets.Length == 0 ? saves.Events.Count == 0 : saves.Events.Count == 2 &&
                        saves.Events.Any(value => ReferenceEquals(value.Initiator, targets[0]) && !value.IsPassed) &&
                        saves.Events.Any(value => ReferenceEquals(value.Initiator, targets[1]) && value.IsPassed) &&
                        state.Response(targets[0]) == ElementalNereidResponse.Affected &&
                        state.Response(targets[1]) == ElementalNereidResponse.Resisted &&
                        targets[0].Descriptor.State.HasCondition(UnitCondition.Dazed) &&
                        !targets[1].Descriptor.HasFact(NereidTargetBlueprint);
                    success = canceledExact && command.IsStarted && command.IsActed && !command.Cutscene && !command.IsIgnoreCooldown &&
                        command.ExecutionProcess != null && command.ExecutionProcess.IsEnded &&
                        unit.Descriptor.Resources.GetResourceAmount(resource) == 0 && !data.IsAvailable &&
                        command.Type == Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard &&
                        !unit.IsInCombat && unit.CombatState.Cooldown.StandardAction == 0 &&
                        Math.Abs(source.TimeLeft.TotalSeconds - ElementalNereidPolicy.DurationRounds(level) * 6) < .01 &&
                        source.Context.Params.CasterLevel == level && source.Context.Params.DC ==
                            ElementalNereidPolicy.DifficultyClass(level, unit.Stats.Charisma.Bonus) && targetsExact;
                    record["canceledExact"] = canceledExact; record["started"] = command.IsStarted; record["acted"] = command.IsActed;
                    record["standardActionCharge"] = unit.CombatState.Cooldown.StandardAction;
                    record["commandType"] = command.Type.ToString(); record["inCombat"] = unit.IsInCombat;
                    record["resourceAmount"] = unit.Descriptor.Resources.GetResourceAmount(resource);
                    record["level"] = level; record["dc"] = source.Context.Params.DC;
                    record["secondsAtActivation"] = source.TimeLeft.TotalSeconds;
                    record["areaId"] = area.UniqueId; record["targetsExact"] = targetsExact;
                    record["saves"] = new JArray(saves.Events.Select(value => new JObject {
                        ["targetId"] = value.Initiator.UniqueId, ["dc"] = value.DifficultyClass, ["passed"] = value.IsPassed }));
                    if (keepForSave && success)
                    {
                        // Age only this exact owned activation's deadlines. This
                        // isolates a partially elapsed save fixture without moving
                        // the game's clock or shortening unrelated timed effects.
                        TimeSpan oldEnd = source.EndTime;
                        foreach (var actor in Game.Instance.State.Units.All)
                            foreach (var buff in actor.Buffs.Enumerable.Where(value =>
                                ElementalNereidAuraState.IncludesContext(value.Context, source.Context)).ToArray())
                                buff.EndTime -= TimeSpan.FromSeconds(2);
                        record["deadlineAgingSeconds"] = 2; record["originalEndTimeTicks"] = oldEnd.Ticks;
                        record["agedEndTimeTicks"] = source.EndTime.Ticks;
                    }
                }
                finally
                {
                    if (subscribed) EventBus.Unsubscribe(saves);
                    if (command?.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Detach();
                    unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue();
                    for (int index = 0; index < targets.Length; index++) targets[index].Stats.SaveWill.BaseValue = wills[index];
                    if (source != null && (!keepForSave || !success)) EndNereidPersistenceAura(source);
                    if (!keepForSave || !success)
                        for (int index = 0; index < moved.Length; index++) PlaceNereidPersistenceUnit(moved[index], positions[index]);
                    UnityEngine.Random.state = random;
                    // The saved aura legitimately grants its own temporary Shake
                    // Free helper to allies. All preexisting foreign facts, wounds
                    // and positions must stay exact; no other additions are allowed.
                    bool foreignExact = foreign.Select((actor, index) => actor.Damage == foreignDamage[index] &&
                        actor.Position.Equals(foreignPositions[index]) && foreignBuffs[index].All(value => actor.Buffs.Enumerable.Contains(value)) &&
                        actor.Buffs.Enumerable.Except(foreignBuffs[index]).All(value => keepForSave && success &&
                            ReferenceEquals(value.Blueprint, NereidAssistanceBlueprint) &&
                            ElementalNereidAuraState.IncludesContext(value.Context, source.Context))).All(value => value);
                    bool cleanup = foreignExact && Game.Instance.TimeController.GameTime == clock && Game.Instance.IsPaused &&
                        targets.Select((value, index) => value.Stats.SaveWill.BaseValue == wills[index]).All(value => value);
                    record["foreignOriginalStateExact"] = foreignExact; record["cleanupExact"] = cleanup;
                    record["exact"] = success && cleanup; _nereidPersistenceRecords.Add(record);
                    Add(_assertions, "elemental-nereid-persistence-command-" + phase + "-" + fixture.Label,
                        "native queued cancellation, single standard-action/use commitment and exact source-owned state",
                        record.ToString(Formatting.None), success && cleanup,
                        "registered native-selected fixture; native UnitCommands/UnitActionController; owned animation/starting-action timing isolated");
                }
                if (!record.Value<bool>("exact")) throw new InvalidOperationException("Nereid persistence command diverged: " + record.ToString(Formatting.None));
            }

            private void PrepareNereidSavedRest(int level)
            {
                EnsureNereidPersistencePause();
                var fixture = _fixtures.Last(value => ExpectedPersistenceTraits(value, value.Heritage)
                    .Any(trait => trait.Definition.Id == ElementalAlternateTraitId.NereidFascination));
                var caster = Snapshot(_allUnits).OfType<UnitEntityData>().Single(value => IsFixtureUnit(value, fixture));
                if (caster.Descriptor.Progression.CharacterLevel != level) throw new InvalidOperationException("Saved aura caster level differs.");
                var resource = PersistenceSlaResource(fixture, fixture.Heritage);
                int before = caster.Descriptor.Resources.GetResourceAmount(resource);
                RestController.ApplyRest(caster.Descriptor);
                var sla = ObservePersistenceSla(fixture, caster.Descriptor, fixture.Heritage, 1, level);
                if (!sla.Exact) throw new InvalidOperationException("Ordinary rest did not restore exactly one saved-aura activation.");
                _nereidPersistenceRecords.Add(new JObject { ["kind"] = "ordinary-rest-before-saved-activation",
                    ["fixture"] = fixture.Label, ["before"] = before, ["after"] = sla.Amount, ["level"] = level });
                _nereidSavedRestCaster = caster.UniqueId;
            }
            private void PrepareNereidSavedCondition(int level)
            {
                if (_nereidSavedRestCaster == null) PrepareNereidSavedRest(level);
                var fixture = _fixtures.Last(value => ExpectedPersistenceTraits(value, value.Heritage)
                    .Any(trait => trait.Definition.Id == ElementalAlternateTraitId.NereidFascination));
                var caster = Snapshot(_allUnits).OfType<UnitEntityData>().Single(value => IsFixtureUnit(value, fixture));
                if (caster.UniqueId != _nereidSavedRestCaster) throw new InvalidOperationException("Saved aura ordinary-rest caster identity changed.");
                var sla = ObservePersistenceSla(fixture, caster.Descriptor, fixture.Heritage, 1, level);
                if (!sla.Exact) throw new InvalidOperationException("The one restored use changed before the saved aura activation.");
                SpendNereidForPersistence(fixture, caster, sla.Data, "save-owned-aura", true);
                _nereidSavedRestCaster = null;
            }

            private void RecordNereidSavedCondition(int expectedLevel, string phase)
            {
                var units = Snapshot(_allUnits).OfType<UnitEntityData>().ToArray();
                var auraBlueprint = NereidAuraBlueprint;
                var areaBlueprint = NereidPersistenceTrait.Mechanics().OfType<BlueprintAbilityAreaEffect>().Single();
                var allBuffs = NereidPersistenceTrait.Mechanics().OfType<BlueprintBuff>().ToArray();
                var areasBeforeSettle = Game.Instance.State.AreaEffects.All.Where(value => ReferenceEquals(value.Blueprint, areaBlueprint)).ToArray();
                var nativeAreaObservations = new JArray(areasBeforeSettle.Select(value => new JObject {
                    ["areaId"] = value.UniqueId, ["ended"] = value.IsEnded, ["destroyRequested"] = value.ShouldBeDestroyed,
                    ["destroyed"] = value.Destroyed, ["casterId"] = value.Context?.MaybeCaster?.UniqueId,
                    ["position"] = new JArray(value.Position.x, value.Position.y, value.Position.z),
                    ["contextFindsSource"] = ElementalNereidAuraState.Find(value.Context, auraBlueprint) != null,
                    ["insideCount"] = value.UnitsInside.Count() }));
                // Native AddAreaEffect participates in area unload/load events.
                // Observe the pre-tick graph, then complete only already-ended,
                // exactly owned native destruction. Never hide an extra live area.
                if (phase == "fresh-load-before-mutation")
                    foreach (var area in areasBeforeSettle.Where(value => IsFixtureUnit(value.Context?.MaybeCaster) &&
                        _crossBefore.Any(original => ReferenceEquals(original, value))))
                        _nereidLoadedAreas.Add(area);
                var endedAreas = areasBeforeSettle.Where(value => value.IsEnded).ToArray();
                if (endedAreas.Any(value => !IsFixtureUnit(value.Context?.MaybeCaster)))
                    throw new InvalidOperationException("A foreign ended Nereid area entered the scoped save fixture.");
                foreach (var ended in endedAreas) TickNereidNativeArea(ended);
                if (endedAreas.Length != 0) DrainNereidAreaDestruction(endedAreas[0]);
                var effects = units.SelectMany(unit => unit.Buffs.Enumerable.Where(buff => allBuffs.Contains(buff.Blueprint))
                    .Select(buff => new { unit, buff })).ToArray();
                var sources = effects.Where(value => ReferenceEquals(value.buff.Blueprint, auraBlueprint)).ToArray();
                var areas = Game.Instance.State.AreaEffects.All.Where(value => ReferenceEquals(value.Blueprint, areaBlueprint)).ToArray();
                var record = new JObject { ["phase"] = phase, ["kind"] = "saved-native-nereid-aura",
                    ["expectedCasterLevel"] = expectedLevel, ["gameTimeTicks"] = Game.Instance.TimeController.GameTime.Ticks,
                    ["auraCount"] = sources.Length, ["areaCount"] = areas.Length,
                    ["nativeAreasBeforeSettle"] = nativeAreaObservations,
                    ["sourcesBeforeActiveTicks"] = new JArray(sources.Select(value => new JObject {
                        ["casterId"] = value.unit.UniqueId, ["attachedAreaId"] = NereidNativeArea(value.buff)?.UniqueId,
                        ["components"] = new JArray(value.buff.SelectComponents<Kingmaker.Blueprints.GameLogicComponent>()
                            .Select(component => component.name + ":" + component.GetType().FullName)),
                        ["responses"] = new JArray(NereidSavedTargets().Select(target => new JObject {
                            ["targetId"] = target.UniqueId,
                            ["response"] = value.buff.SelectComponents<ElementalNereidAuraState>().Single().Response(target).ToString() })) })) };
                bool exact = expectedLevel == 0 ? effects.Length == 0 && areas.Length == 0 &&
                    units.All(unit => unit.Descriptor.Abilities.Enumerable.All(value => value.Blueprint.AssetGuid != ElementalNereidFactory.ShakeFreeGuid))
                    : sources.Length == 1 && areas.Length == 1;
                if (expectedLevel > 0 && sources.Length == 1 && areas.Length == 1)
                {
                    var source = sources[0].buff;
                    var caster = sources[0].unit;
                    var state = source.SelectComponents<ElementalNereidAuraState>().Single();
                    var area = NereidNativeArea(source);
                    var targets = NereidSavedTargets();
                    bool contextExact = ReferenceEquals(area, areas[0]) && !area.IsEnded &&
                        ReferenceEquals(area.Context.MaybeCaster, caster) &&
                        ReferenceEquals(source.Context.MaybeCaster, caster) &&
                        ReferenceEquals(ElementalNereidAuraState.Find(area.Context, auraBlueprint), state);
                    var saves = new NereidPersistenceSaves();
                    EventBus.Subscribe(saves);
                    try {
                        record["membershipBeforeUpdates"] = ObserveNereidNativeMembership(area, targets);
                        SynchronizeNereidFixturePositions(new[] { caster }.Concat(targets).ToArray(), record);
                        record["membershipAfterNativePositionReconciliation"] = ObserveNereidNativeMembership(area, targets);
                        TickNereidNativeArea(area);
                        record["membershipAfterFirstUpdate"] = ObserveNereidNativeMembership(area, targets);
                        TickNereidNativeArea(area);
                        record["membershipAfterSecondUpdate"] = ObserveNereidNativeMembership(area, targets);
                    }
                    finally { EventBus.Unsubscribe(saves); }
                    var conditions = units.SelectMany(unit => unit.Buffs.Enumerable.Where(value =>
                        ReferenceEquals(value.Blueprint, NereidTargetBlueprint)).Select(buff => new { unit, buff })).ToArray();
                    var helpers = units.SelectMany(unit => unit.Buffs.Enumerable.Where(value =>
                        ReferenceEquals(value.Blueprint, NereidAssistanceBlueprint)).Select(buff => new { unit, buff })).ToArray();
                    bool targetExact = conditions.Length == 1 && ReferenceEquals(conditions[0].unit, targets[0]) &&
                        conditions[0].buff.SourceAreaEffectId == area.UniqueId &&
                        conditions[0].buff.EndTime == source.EndTime &&
                        ReferenceEquals(ElementalNereidAuraState.Find(conditions[0].buff.Context, auraBlueprint), state) &&
                        targets[0].Descriptor.State.HasCondition(UnitCondition.Dazed) &&
                        !targets[1].Descriptor.HasFact(NereidTargetBlueprint) &&
                        state.Response(targets[0]) == ElementalNereidResponse.Affected &&
                        state.Response(targets[1]) == ElementalNereidResponse.Resisted && saves.Events.Count == 0;
                    bool helperExact = helpers.Length != 0 && helpers.All(value =>
                        value.buff.EndTime == source.EndTime &&
                        ReferenceEquals(ElementalNereidAuraState.Find(value.buff.Context, auraBlueprint), state) &&
                        value.unit.Descriptor.Abilities.Enumerable.Count(ability => ability.Blueprint.AssetGuid == ElementalNereidFactory.ShakeFreeGuid) == 1);
                    exact &= IsFixtureUnit(caster) && source.Active && !source.IsSuppressed && contextExact && targetExact && helperExact &&
                        source.Context.Params.CasterLevel == expectedLevel && source.Context.Params.DC ==
                            ElementalNereidPolicy.DifficultyClass(expectedLevel, caster.Stats.Charisma.Bonus) &&
                        source.TimeLeft.TotalSeconds > 0 && source.TimeLeft.TotalSeconds <= 4.01 &&
                        Vector3.Distance(area.Position, caster.Position) < .01f;
                    record["casterId"] = caster.UniqueId; record["areaId"] = area.UniqueId;
                    record["casterPosition"] = new JArray(caster.Position.x, caster.Position.y, caster.Position.z);
                    record["areaPosition"] = new JArray(area.Position.x, area.Position.y, area.Position.z);
                    record["casterLevel"] = source.Context.Params.CasterLevel; record["dc"] = source.Context.Params.DC;
                    record["endTimeTicks"] = source.EndTime.Ticks; record["secondsLeft"] = source.TimeLeft.TotalSeconds;
                    record["contextExact"] = contextExact; record["targetsExact"] = targetExact;
                    record["helpersExact"] = helperExact; record["helperCount"] = helpers.Length;
                    record["nativeTicksSavingThrows"] = saves.Events.Count;
                    record["responses"] = new JArray(targets.Select(target => new JObject {
                        ["targetId"] = target.UniqueId, ["response"] = state.Response(target).ToString(),
                        ["position"] = new JArray(target.Position.x, target.Position.y, target.Position.z) }));
                }
                // Read the live collection after the native updates; an earlier
                // snapshot can contain a fact that native exit has since removed.
                effects = units.SelectMany(unit => unit.Buffs.Enumerable.Where(buff => allBuffs.Contains(buff.Blueprint))
                    .Select(buff => new { unit, buff })).ToArray();
                record["effects"] = new JArray(effects.Select(value => new JObject {
                    ["targetId"] = value.unit.UniqueId, ["buffGuid"] = value.buff.Blueprint.AssetGuid,
                    ["casterId"] = value.buff.Context?.MaybeCaster?.UniqueId,
                    ["sourceAreaId"] = value.buff.SourceAreaEffectId, ["active"] = value.buff.Active,
                    ["ownerExact"] = ReferenceEquals(value.buff.Owner, value.unit.Descriptor),
                    ["ownerTurnedOn"] = value.unit.Descriptor.IsTurnedOn, ["inGame"] = value.unit.IsInGame,
                    ["inState"] = value.unit.IsInState, ["dead"] = value.unit.Descriptor.State.IsDead,
                    ["suppressed"] = value.buff.IsSuppressed, ["sourceContextResolves"] =
                        ElementalNereidAuraState.Find(value.buff.Context, auraBlueprint) != null,
                    ["endTimeTicks"] = value.buff.EndTime.Ticks, ["secondsLeft"] = value.buff.TimeLeft.TotalSeconds }));
                if (expectedLevel > 0 && exact) RecordNereidHydraulicInertState(phase);
                record["exact"] = exact; _nereidPersistenceRecords.Add(record);
                Add(_assertions, "elemental-nereid-saved-state-" + phase,
                    "exact native saved owner/area/remaining deadline and terminal response ledger, or final complete absence",
                    record.ToString(Formatting.None), exact,
                    "fresh native load, observed ended-area lifecycle completion and two active-area updates before trait/resource mutation; saved activation locally aged by two seconds");
                if (!exact) throw new InvalidOperationException("Nereid native saved state diverged: " + record.ToString(Formatting.None));
            }

            private void RecordNereidHydraulicInertState(string phase)
            {
                var rows = new JArray();
                var race = _blueprintSet.Undine;
                foreach (var fixture in _fixtures.Where(value => value.Blueprints.AlternateTraits.Race == ElementalHeritageRace.Undine))
                {
                    var unit = Game.Instance.State.Units.All.Single(value => IsFixtureUnit(value, fixture));
                    var owner = unit.Descriptor;
                    foreach (var id in new[] { ElementalFeatId.HydraulicManeuver, ElementalFeatId.TritonPortal })
                    {
                        var feat = _featBlueprintSet.RequireFeature(id);
                        var prerequisite = feat.GetComponents<Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteFeature>()
                            .Single(value => ReferenceEquals(value.Feature, race.SlaFeature));
                        var ability = _featBlueprintSet.RequireSymbol<BlueprintAbility>(id == ElementalFeatId.HydraulicManeuver
                            ? ElementalRaceIdentityCatalog.HydraulicManeuverAbility : ElementalRaceIdentityCatalog.TritonPortalAbility);
                        bool granted = owner.Abilities.Enumerable.Count(value => ReferenceEquals(value.Blueprint, ability)) == 1;
                        var root = new AbilityData(ability, owner);
                        var variants = ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityVariants>();
                        var data = variants == null ? new[] { root } : variants.Variants.Select(value => new AbilityData(root, value)).ToArray();
                        bool prerequisitePermitted = prerequisite.Check(null, owner, null);
                        bool inert = data.All(value => !value.IsAvailable &&
                            !value.Blueprint.GetComponents<ElementalHydraulicSharedResourceAvailability>().Single().IsAvailableFor(value));
                        bool exact = owner.Progression.Features.GetRank(feat) == 1 && granted && !prerequisitePermitted && inert &&
                            !owner.HasFact(race.SlaFeature) && owner.Abilities.GetAbility(race.SlaAbility) == null &&
                            owner.Resources.GetResourceAmount(race.SlaResource) == 0 &&
                            owner.Resources.GetResourceAmount(NereidPersistenceTrait.Mechanics().OfType<BlueprintAbilityResource>().Single()) == 0;
                        var row = new JObject { ["fixture"] = fixture.Label, ["feat"] = id.ToString(),
                            ["granted"] = granted, ["nativePrerequisite"] = prerequisitePermitted,
                            ["executableCount"] = data.Length, ["nativeInert"] = inert, ["exact"] = exact };
                        rows.Add(row);
                        if (!exact) throw new InvalidOperationException("Nereid replacement changed a retained Hydraulic feat: " + row.ToString(Formatting.None));
                    }
                }
                bool complete = rows.Count == 12;
                _nereidPersistenceRecords.Add(new JObject { ["kind"] = "native-nereid-hydraulic-inert", ["phase"] = phase,
                    ["rows"] = rows, ["exact"] = complete });
                if (!complete) throw new InvalidOperationException("The exact six-Undine Hydraulic inert-state matrix is incomplete.");
            }

            private void RecordNereidCleanupBoundary(bool cleaned)
            {
                var gates = new JObject();
                var collections = new JObject();
                Action<string, object[], object[], bool> compare = (name, before, after, references) => {
                    var expected = before.Where(value => !HasFixtureIdentity(value) &&
                        (name != "cross" || !IsCompletionSavedAreaFixture(value))).ToArray();
                    bool exact = references ? SameReferences(expected, after) : SameValues(expected, after);
                    gates[name] = exact;
                    Func<object[], JArray> describe = values => new JArray(values.Select(value => {
                        var unit = value as UnitEntityData;
                        return new JObject { ["type"] = value?.GetType().FullName,
                            ["reference"] = value == null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value),
                            ["id"] = unit != null ? unit.UniqueId : value is UnitReference
                                ? ((UnitReference)value).UniqueId : null,
                            ["fixture"] = HasFixtureIdentity(value), ["catalogFixture"] = HasCatalogFixtureIdentity(value),
                            ["descriptorPresent"] = unit?.Descriptor != null };
                    }));
                    collections[name] = new JObject { ["expectedCount"] = expected.Length, ["actualCount"] = after.Length,
                        ["expected"] = exact ? null : describe(expected), ["actual"] = exact ? null : describe(after),
                        ["originalCatalogFixtures"] = new JArray(before.Where(HasCatalogFixtureIdentity).Select(value =>
                            new JObject { ["recognizedAsFixture"] = HasFixtureIdentity(value),
                                ["descriptorPresent"] = (value as UnitEntityData)?.Descriptor != null })) };
                };
                compare("units", _unitsBefore, Snapshot(_allUnits), true);
                compare("party", _partyBefore, Snapshot(_party), true);
                compare("partyCharacters", _partyCharactersBefore, _player.PartyCharacters.Cast<object>().ToArray(), false);
                compare("remote", _remoteBefore, Snapshot(_remote), false);
                compare("cross", _crossBefore, Snapshot(_cross), true);
                gates["inventory"] = FeatPersistenceCleanupInventoryExact();
                gates["money"] = _player.Money == _moneyBefore;
                gates["startingGold"] = _gunslingerClass == null || _gunslingerClass.StartingGold == _startingGoldBefore;
                gates["characterRaces"] = CharacterRacesArrayExact();
                _nereidPersistenceRecords.Add(new JObject { ["kind"] = "native-nereid-cleanup-boundary",
                    ["updates"] = _settleUpdates, ["cleaned"] = cleaned,
                    ["gates"] = gates, ["collections"] = collections,
                    ["moneyBefore"] = _moneyBefore, ["moneyAfter"] = _player.Money,
                    ["savedOwnedAreas"] = new JArray(_nereidLoadedAreas.Select(area => new JObject {
                        ["id"] = area.UniqueId, ["destroyed"] = area.Destroyed,
                        ["inCrossScene"] = Snapshot(_cross).Any(value => ReferenceEquals(value, area)),
                        ["inAreaPool"] = Game.Instance.State.AreaEffects.All.Contains(area) })) });
            }

            private JObject RecordNereidPhysicalTransientState(string phase)
            {
                var aura = NereidAuraBlueprint;
                var target = NereidTargetBlueprint;
                var helper = NereidAssistanceBlueprint;
                var buffs = _physicalActor.Buffs.Enumerable.Where(value =>
                    ReferenceEquals(value.Blueprint, aura) || ReferenceEquals(value.Blueprint, target) || ReferenceEquals(value.Blueprint, helper)).ToArray();
                bool dead = _physicalActor.Descriptor.State.IsDead;
                int helpers = buffs.Count(value => ReferenceEquals(value.Blueprint, helper));
                int helperAbilities = _physicalActor.Descriptor.Abilities.Enumerable.Count(value =>
                    value.Blueprint.AssetGuid == ElementalNereidFactory.ShakeFreeGuid);
                bool exact = (helperAbilities == (helpers > 0 ? 1 : 0)) && (!dead || buffs.Length == 0);
                var row = new JObject { ["phase"] = phase, ["kind"] = "native-nereid-physical-transient-state",
                    ["fixtureId"] = _physicalActor.UniqueId, ["dead"] = dead, ["helperAbilities"] = helperAbilities,
                    ["effects"] = new JArray(buffs.Select(value => value.Blueprint.AssetGuid)), ["exact"] = exact };
                _nereidPersistenceRecords.Add(row);
                if (!exact) throw new InvalidOperationException("Native Nereid transient lifecycle diverged: " + row.ToString(Formatting.None));
                return row;
            }
        }
    }
}
