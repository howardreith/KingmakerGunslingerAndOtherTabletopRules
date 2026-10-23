using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Pathfinding;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Cleanup2CreatureApproach
        {
            internal UnitAttack Range;
            internal JObject Evidence;
        }

        // Exactly two actual canonical summons. A native UnitAttack starts
        // outside its own measured range and owns approach, act and recovery.
        // No optional Claw/Tail/Hit animation handle or synthetic packet is
        // requested, and no command/action/animation tick is supplied here.
        private IEnumerable<int> ReviewSummonCleanup2CreatureCommands(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters == null || _request.Parameters.Count != 1 ||
                (string)_request.Parameters["reviewPhase"] != "creature-commands" || fixture == null || book == null || _sprint25Clock == null ||
                _expandedSummoningPlayerPathCaptureActive || _refinementUnit != null || _refinementAction != null ||
                _refinementChangedPause || _creatureReviewQueue != null ||
                TurnBased.Controllers.CombatController.IsInTurnBasedCombat())
                throw new InvalidOperationException("Exact exclusive save-free RTWP creature-commands fixture required.");
            bool pause = Game.Instance.IsPaused, waiting = _sprint25WaitingForUnityAppearance;
            var oldCapture = ExpandedSummoningRuleCapture.ToArray(); var oldDuration = ExpandedSummoningRuleDurationCapture.ToArray();
            var oldEvents = ExpandedSummoningPlayerPathEvents.ToArray();
            int oldCasts = _expandedSummoningPlayerPathRuleCastCount, oldSpawns = _expandedSummoningPlayerPathSpawnActionCount;
            var hooks = HarmonyInstance.Create("kmg.cleanup2.creature-commands." + _request.RunId);
            UnitEntityData actor = null; var completed = new List<string>(); var phase = Stopwatch.StartNew();
            try
            {
                if (fixture.Hostile == null || fixture.Hostile.Destroyed) CreateExpandedSummoningCorrectionHostile(fixture);
                var target = fixture.Hostile;
                if (target == null || target.Destroyed || !ReferenceEquals(target.Blueprint, fixture.HostileBlueprint) ||
                    fixture.UnitsBefore.Any(value => ReferenceEquals(value, target)))
                    throw new InvalidOperationException("Exact disposable hostile required for creature command proof.");
                const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                foreach (string key in new[] { SummonAttackGesturePolicy.Crab, SummonAttackGesturePolicy.Brachiosaurus })
                {
                    if (phase.Elapsed.TotalSeconds > 180) throw new TimeoutException("Two-creature command phase exceeded180seconds.");
                    var route = Cleanup2GiantRoute(fixture, key, SummonFamily.NaturesAlly);
                    bool crab = key == SummonAttackGesturePolicy.Crab;
                    bool exactRoute = route.Tier == (crab ? 3 : 7) && route.Unit.AssetGuid ==
                        (crab ? CrabSummonAnimationBindingPolicy.UnitGuid : SummonAttackGesturePolicy.BrachiosaurusUnit) && route.Selected.AssetGuid ==
                        (crab ? CrabSummonAnimationBindingPolicy.SourceGuids[0] : SummonAttackGesturePolicy.BrachiosaurusSources[0]);
                    Cleanup2Check("creature-command-route-" + key, exactRoute, new JObject { ["key"] = key, ["parent"] = route.Parent.AssetGuid,
                        ["child"] = route.Selected.AssetGuid, ["tier"] = route.Tier, ["unit"] = route.Unit.AssetGuid,
                        ["fixtureAccess"] = "Real canonical SNAIII Crab/SNAVII Brachiosaurus parent/prepared-slot cast on the disposable Wizard; not natural class-learning proof." },
                        "Only the exact two canonical single-creature roots enter the bounded ordinary command slice.");
                    if (!exactRoute) throw new InvalidOperationException("Creature command route changed.");
                    actor = Cleanup2CastGiant(fixture, book, route);
                    foreach (int frame in Cleanup2WaitAppearance(actor, "creature-command-" + key)) yield return frame;
                    Cleanup2BodyReadback(actor, key, "before-ordinary-approach");
                    foreach (int frame in Cleanup2CreatureNativeApproach(fixture, actor, target, key)) yield return frame;
                    Cleanup2BodyReadback(actor, key, "after-ordinary-approach");
                    completed.Add(key); DisposeExpandedSummoningUnits(fixture.Created, new[] { actor }); actor = null;
                }
                Cleanup2Check("two-creature-native-command-slice", completed.SequenceEqual(new[] { SummonAttackGesturePolicy.Crab, SummonAttackGesturePolicy.Brachiosaurus }),
                    new JObject { ["completed"] = new JArray(completed), ["wallSeconds"] = phase.Elapsed.TotalSeconds,
                        ["scope"] = "Native Default ordinary single-command approach, actual weapon/cost and owned render gesture/recovery only. No117-frame synthetic-clock packet, full-attack retiming, optional Hit clip, save/reload or owner art acceptance claim." },
                    "Both exact creatures complete bounded native approach, contact and recovery with the unchanged production missing-animation path.");
            }
            finally
            {
                var failures = new List<string>();
                Action<string, Action> restore = (label, action) => {
                    try { action(); } catch (Exception error) { failures.Add(label + ":" + error.GetType().Name + ":" + error.Message); }
                };
                // Each independent restore runs even if an owned actor has
                // already failed teardown. Never leave capture hooks active.
                _expandedSummoningPlayerPathCaptureActive = false;
                restore("capture-hooks", () => hooks.UnpatchAll(hooks.Id));
                restore("owned-actor", () => {
                    if (actor != null && !actor.Destroyed) {
                        actor.Commands.InterruptAll(true); DisposeExpandedSummoningUnits(fixture.Created, new[] { actor });
                    }
                });
                restore("capture-state", () => {
                    ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningRuleCapture.AddRange(oldCapture);
                    ExpandedSummoningRuleDurationCapture.Clear(); ExpandedSummoningRuleDurationCapture.AddRange(oldDuration);
                    ExpandedSummoningPlayerPathEvents.Clear(); ExpandedSummoningPlayerPathEvents.AddRange(oldEvents);
                    _expandedSummoningPlayerPathRuleCastCount = oldCasts; _expandedSummoningPlayerPathSpawnActionCount = oldSpawns;
                });
                _sprint25WaitingForUnityAppearance = Cleanup2NativeDefaultPending || waiting;
                restore("pause", () => Game.Instance.IsPaused = pause);
                Cleanup2Check("creature-commands-owned-state-restored", failures.Count == 0 && !Cleanup2NativeDefaultPending,
                    new JObject { ["failures"] = new JArray(failures), ["nativeModeLeaseStillPending"] = Cleanup2NativeDefaultPending,
                        ["positiveActorStillRegistered"] = actor != null && fixture.Created.Contains(actor), ["foreignActorsClaimed"] = false },
                    "Owned capture and command state restores independently; shared fixture teardown retains only its exact positive actors.");
                if (failures.Count != 0 || Cleanup2NativeDefaultPending)
                    throw new InvalidOperationException("Creature command cleanup incomplete: " + string.Join("|", failures.ToArray()));
            }
        }

        private IEnumerable<int> Cleanup2CreatureNativeApproach(ExpandedSummoningCorrectionFixture fixture, UnitEntityData actor, UnitEntityData target, string key)
        {
            bool actorBrain = actor.IsBrainActive, actorCombat = actor.IsInCombat, targetCombat = target.IsInCombat;
            var targetPosition = target.Position; float targetOrientation = target.Orientation;
            Cleanup2NativeDefaultScope mode = null; UnitAttack command = null;
            var observer = new Sprint24AttackObserver { Owner = actor, Target = target };
            var costs = new JArray(); var endings = new JArray(); var images = new JArray(); var events = new JArray(); var paths = new JArray();
            try
            {
                ResetExpandedSummoningHostile(fixture); SetExpandedSummoningBrainActive(actor, false);
                actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                if (target.IsBrainActive || !actor.Commands.Empty || !target.Commands.Empty) throw new InvalidOperationException("Idle owned actor/hostile required.");
                mode = Cleanup2EnterNativeDefault(fixture, "creature-approach-" + key);
                actor.JoinCombat(); target.JoinCombat(); actor.Memory.Add(target); target.Memory.Add(actor);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var visual = SummonAttackGestureVisual.For(actor);
                var special = key == SummonAttackGesturePolicy.Crab ? UnitAnimationSpecialAttackType.Claw : UnitAnimationSpecialAttackType.Tail;
                if (visual == null || !visual.Ready || visual.Key != key || !visual.ProveCurrentPose() || !visual.ProveSameFrameSourceState() ||
                    !ExpandedSummoningPteranodonViewPatch.HasLiveOriginalVisual(actor.View, key) || actor.View.AnimationManager.GetAction(special) != null)
                    throw new InvalidOperationException("Exact private production body/gesture and unchanged null special-action lookup required.");
                var route = Cleanup2CreatureApproachLane(fixture, actor, target, key);
                var mechanics = Cleanup2ScaleMechanics(actor); var stats = Cleanup2CrabBaseStats(actor);
                command = new UnitAttack(target) { IsSingleAttack = true, ForceFullAttack = false };
                int pulseBefore = _sprint25Clock.Pulses;
                Vector3 start = actor.Position; double gameStart = Game.Instance.Player.GameTime.TotalSeconds;
                float maxVelocity = 0, maximumTravel = 0; bool approachImage = false, actImage = false, nativePathObserved = false;
                var pathField = actor.View.AgentASP.GetType().GetField("m_Path", Cleanup2Fields);
                if (pathField == null) throw new MissingFieldException("Actual native movement path unavailable.");
                observer.AfterWeapon = attack => {
                    if (events.Count >= 4) return;
                    var info = command.PlannedAttack; int index = command.GetAttackIndex();
                    var schedules = visual.Schedules.Where(value => ReferenceEquals(value.Command, command) && value.Index == index).ToArray();
                    bool associated = schedules.Length == 1 && ReferenceEquals(schedules[0].Info, info) && ReferenceEquals(schedules[0].Weapon, attack.Weapon) &&
                        ReferenceEquals(info?.Weapon, attack.Weapon) && schedules[0].ActGame >= 0 && info.AnimationHandle == null && command.Animation == null;
                    events.Add(new JObject { ["frame"] = Time.frameCount, ["index"] = index, ["exactScheduleInfoWeapon"] = associated,
                        ["nativeInRangeAtRule"] = route.Range.IsUnitEnoughClose, ["nativeLOSAtRule"] = actor.HasLOS(target),
                        ["weapon"] = attack.Weapon?.Blueprint.AssetGuid, ["weaponEntity"] = attack.Weapon == null ? 0 : RuntimeHelpers.GetHashCode(attack.Weapon),
                        ["hit"] = attack.AttackRoll?.IsHit, ["automaticMiss"] = attack.AttackRoll?.AutoMiss, ["attackOfOpportunity"] = attack.IsAttackOfOpportunity,
                        ["charge"] = attack.IsCharge, ["travelAtRule"] = Vector3.Distance(start, actor.Position), ["commandSeconds"] = command.TimeSinceStart,
                        ["actualNativeActTickDelta"] = schedules.Length == 1 ? (JToken)schedules[0].NativeActTickDelta : JValue.CreateNull() });
                };
                EventBus.Subscribe(observer);
                using (var economy = new FirearmReviewCosts(actor, costs))
                using (var trace = new Sprint26CommandTrace(actor, endings))
                {
                    Action<string> capture = stage => { string file = "creature-command-" + key + "-" + stage + ".png";
                        string result = Cleanup2OrdinaryAttackImage(actor, target, file);
                        var actual = new FileInfo(System.IO.Path.Combine(_request.EvidenceDirectory, file));
                        bool saved = result.StartsWith("png=" + file + ";bytes=", StringComparison.Ordinal) && actual.Exists && actual.Length >= 4096;
                        images.Add(new JObject { ["file"] = file, ["stage"] = stage, ["result"] = result, ["saved"] = saved,
                            ["frame"] = Time.frameCount, ["travel"] = Vector3.Distance(start, actor.Position), ["weaponEvents"] = observer.WeaponAttacks.Count }); };
                    capture("ready");
                    actor.Commands.Run(command);
                    bool issued = ReferenceEquals(command.Executor, actor) && actor.Commands.Contains(command);
                    var watch = Stopwatch.StartNew(); int frames = 0;
                    while (issued && (!command.IsFinished || observer.WeaponAttacks.Count < 1) && watch.Elapsed.TotalSeconds < 20 && frames++ < 2400)
                    {
                        if (!mode.Current || !Cleanup2NoNarrativeOverlay() || !ExpandedSummoningPlayerPathUnitExact(actor, fixture.Caster) ||
                            !ReferenceEquals(SummonAttackGestureVisual.For(actor), visual) || !visual.Ready)
                            throw new InvalidOperationException("Native creature approach lost input/provenance/private-body ownership.");
                        float velocity = actor.View.MovementAgent.Velocity.magnitude;
                        if (!Cleanup2Finite(velocity) || !Cleanup2Finite(actor.Position)) throw new InvalidOperationException("Nonfinite native approach motion.");
                        maxVelocity = Mathf.Max(maxVelocity, velocity); maximumTravel = Mathf.Max(maximumTravel, Vector3.Distance(start, actor.Position));
                        if (paths.Count < 8 && (frames == 1 || frames % 8 == 0))
                        {
                            var path = Cleanup2GroundPath(pathField.GetValue(actor.View.AgentASP) as Pathfinding.Path, actor.Position);
                            paths.Add(new JObject { ["frame"] = Time.frameCount, ["velocity"] = velocity,
                                ["travel"] = maximumTravel, ["path"] = path });
                            nativePathObserved |= (bool)path["present"] && (bool)path["isDone"] && !(bool)path["error"] &&
                                (int)path["nodeCount"] > 0 && ((JArray)path["graphIndices"]).Count == 1 &&
                                (uint)path["graphIndices"][0] == (uint)route.Evidence["selected"]["graph"] &&
                                ((JArray)path["connectedAreas"]).Count == 1 &&
                                (uint)path["connectedAreas"][0] == (uint)route.Evidence["selected"]["connectedArea"];
                        }
                        if (!approachImage && maximumTravel >= .75f && velocity > .01f && observer.WeaponAttacks.Count == 0)
                        { capture("approach"); approachImage = true; }
                        if (!actImage && observer.WeaponAttacks.Count != 0) { capture("contact"); actImage = true; }
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    // The exact native act may complete before the next UI
                    // frame. Retain its first current-pose contact frame.
                    if (!actImage && observer.WeaponAttacks.Count != 0) { capture("contact"); actImage = true; }
                    double recoveryStart = Game.Instance.Player.GameTime.TotalSeconds; var recovery = Stopwatch.StartNew();
                    while (Game.Instance.Player.GameTime.TotalSeconds - recoveryStart < 1.6 && recovery.Elapsed.TotalSeconds < 8)
                    { if (!mode.Current || !Cleanup2NoNarrativeOverlay()) throw new InvalidOperationException("Native creature recovery mode changed."); yield return 0; }
                    capture("recovery");
                    var schedules = visual.Schedules.Where(value => ReferenceEquals(value.Command, command)).ToArray();
                    var state = visual.Describe();
                    bool recovered = visual.Ready && visual.ProveCurrentPose() && visual.ProveSameFrameSourceState() &&
                        ((JArray)state["palettePoints"]).All(value => (float)value["ownedAngle"] < .001f);
                    bool nativeRules = issued && command.IsFinished && (command.Result == UnitCommand.ResultType.Success || trace.Exhausted(command)) &&
                        command.AllAttacks.Count == 1 && observer.OwnerWeaponAttacks.Count == 1 && observer.WeaponAttacks.Count == 1 && observer.Rolls.Count == 1 &&
                        observer.Damage.Count == observer.WeaponAttacks.Count(value => value.AttackRoll != null && value.AttackRoll.IsHit) &&
                        events.Count == 1 && (bool)events[0]["exactScheduleInfoWeapon"] && (bool)events[0]["nativeInRangeAtRule"] && (bool)events[0]["nativeLOSAtRule"] &&
                        economy.Count(command) == 1 && economy.Exact(command, false) && economy.ForcedCount(command) == 0 && !command.Cutscene && !command.IsIgnoreCooldown;
                    bool nativeTravel = nativePathObserved && maximumTravel >= 1.5f && maxVelocity > .01f && approachImage;
                    bool gestureMotion =
                        schedules.Length == 1 && !schedules[0].Cancelled && schedules[0].Samples > 0 && schedules[0].MaximumAngle > .05f &&
                        actImage && images.Count == 4 && images.All(value => (bool)value["saved"]) && recovered &&
                        Game.Instance.Player.GameTime.TotalSeconds - recoveryStart >= 1.6;
                    bool invariants = _sprint25Clock.Pulses == pulseBefore && JToken.DeepEquals(stats, Cleanup2CrabBaseStats(actor)) &&
                        JToken.DeepEquals(mechanics, Cleanup2ScaleMechanics(actor));
                    bool exact = nativeRules && nativeTravel && gestureMotion && invariants;
                    Cleanup2Check("creature-ordinary-approach-command-" + key, exact, new JObject { ["key"] = key, ["unit"] = actor.UniqueId,
                        ["target"] = target.UniqueId, ["route"] = route.Evidence, ["issued"] = issued, ["finished"] = command.IsFinished,
                        ["nativeResult"] = command.Result.ToString(), ["nativeListExhausted"] = trace.Exhausted(command), ["frames"] = frames,
                        ["wallSeconds"] = watch.Elapsed.TotalSeconds, ["actualGameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds - gameStart,
                        ["nativeTravelMeters"] = maximumTravel, ["minimumNativeTravelMeters"] = 1.5, ["maximumNativeVelocity"] = maxVelocity,
                        ["nativeRulesAndCostProof"] = nativeRules, ["nativeTravelProof"] = nativeTravel,
                        ["nativeCurrentPathObserved"] = nativePathObserved, ["privateGestureMotionAndRecoveryProof"] = gestureMotion,
                        ["sourceMechanicsAndClockExact"] = invariants, ["plannedAttacks"] = command.AllAttacks.Count,
                        ["ownerWeaponEvents"] = observer.OwnerWeaponAttacks.Count, ["targetWeaponEvents"] = observer.WeaponAttacks.Count,
                        ["rollEvents"] = observer.Rolls.Count, ["damageEvents"] = observer.Damage.Count,
                        ["nativeCostCount"] = economy.Count(command), ["exactNativeCost"] = economy.Exact(command, false),
                        ["images"] = images, ["events"] = events, ["paths"] = paths, ["costs"] = costs, ["endings"] = endings,
                        ["gesture"] = state, ["gestureRecovered"] = recovered, ["nativeMode"] = mode.Describe(), ["manualClockPulses"] = _sprint25Clock.Pulses - pulseBefore,
                        ["baseStatsExact"] = JToken.DeepEquals(stats, Cleanup2CrabBaseStats(actor)), ["nativeMechanicsExact"] = JToken.DeepEquals(mechanics, Cleanup2ScaleMechanics(actor)),
                        ["scope"] = "Actual native Default UnitAttack owns out-of-range approach and one ordinary attack; ready/approach/contact/recovery full-body frames. Contact frame is visual context, not measured mesh-to-enemy collision or owner art acceptance. No matched OFF/full routine/timing-neutral inference, forced act/hit, clip alias or synthetic locomotion." },
                        "One exact ordinary native command walks at least1.5m from proved out-of-range placement, reaches native range/LOS, resolves one weapon rule/cost and the owned visual gesture, then recovers without changing source mechanics.");
                    if (!exact) throw new InvalidOperationException("Native creature approach/action/recovery proof failed: " + key);
                }
            }
            finally
            {
                var failures = new List<string>();
                Action<string, Action> restore = (label, action) => {
                    try { action(); } catch (Exception error) { failures.Add(label + ":" + error.GetType().Name + ":" + error.Message); }
                };
                restore("rule-observer", () => EventBus.Unsubscribe(observer));
                restore("owned-command", () => {
                    if (command != null && !command.IsFinished && ReferenceEquals(command.Executor, actor) && actor.Commands.Contains(command)) command.Interrupt(true);
                    actor.Commands.RemoveFinishedAndUpdateQueue();
                });
                restore("owned-brain", () => SetExpandedSummoningBrainActive(actor, actorBrain));
                restore("owned-combat", () => { if (!actorCombat && actor.IsInCombat) actor.LeaveCombat(); });
                restore("hostile-combat", () => { if (!targetCombat && target.IsInCombat) target.LeaveCombat(); });
                restore("owned-hostile", () => {
                    ResetExpandedSummoningHostile(fixture); PlaceExpandedSummoningUnit(target, targetPosition); target.Orientation = targetOrientation;
                });
                restore("native-mode", () => { if (mode != null) mode.Dispose(); });
                Cleanup2Check("creature-command-local-state-restored-" + key, failures.Count == 0,
                    new JObject { ["failures"] = new JArray(failures), ["nativeModeLeaseStillPending"] = Cleanup2NativeDefaultPending },
                    "Only owned commands, brain/combat, hostile placement and exact native mode lease restore; independent cleanup attempts survive a prior failure.");
                if (failures.Count != 0) throw new InvalidOperationException("Creature native approach cleanup failed: " + string.Join("|", failures.ToArray()));
            }
        }

        private Cleanup2CreatureApproach Cleanup2CreatureApproachLane(ExpandedSummoningCorrectionFixture fixture, UnitEntityData actor, UnitEntityData target, string key)
        {
            if (AstarPath.active == null || Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance == null || !ExpandedSummoningPlayerPathUnitExact(actor, fixture.Caster) ||
                !ReferenceEquals(target, fixture.Hostile) || !ReferenceEquals(target.Blueprint, fixture.HostileBlueprint) || target.IsBrainActive ||
                fixture.UnitsBefore.Any(value => ReferenceEquals(value, actor) || ReferenceEquals(value, target)) || TurnBased.Controllers.CombatController.IsInTurnBasedCombat())
                throw new InvalidOperationException("Exact owned RTWP creature/hostile route survey required.");
            var agent = actor.View.AgentASP;
            if (agent == null || agent.Unit != actor.View) throw new InvalidOperationException("Exact native walking agent required.");
            var constructTags = agent.GetType().GetMethod("ConstructTags", Cleanup2Fields);
            if (constructTags == null || constructTags.ReturnType != typeof(int) || constructTags.GetParameters().Length != 0)
                throw new MissingMethodException("Actual native ConstructTags walking contract unavailable.");
            int tags = (int)constructTags.Invoke(agent, null);
            var constraint = new PathNNConstraint { graphMask = 1, constrainWalkability = true, walkable = true,
                constrainArea = false, constrainTags = true, tags = tags, constrainDistance = true, distanceXZ = false, distanceXZkeepY = false };
            var anchor = AstarPath.active.GetNearest(fixture.Caster.Position, constraint);
            if (anchor.node == null || !anchor.node.Walkable || !Cleanup2Finite(anchor.clampedPosition)) throw new InvalidOperationException("No native walking anchor.");
            constraint.SetStart(anchor.node); constraint.constrainArea = true;
            var graph = AstarPath.active.graphs[anchor.node.GraphIndex] as IRaycastableGraph;
            if (graph == null) throw new InvalidOperationException("Actual native walking graph lacks direct-path linecast.");
            var probe = new UnitAttack(target) { IsSingleAttack = true }; probe.Init(actor);
            float radius = probe.ApproachRadius, footprint = actor.Corpulence + target.Corpulence;
            if (!Cleanup2Finite(radius) || !Cleanup2Finite(footprint) || radius <= 0 || footprint <= 0) throw new InvalidOperationException("Native range/footprint is invalid.");
            var protectedUnits = Game.Instance.State.Units.All.Where(value => value != null && value.IsInGame && !value.Destroyed &&
                !ReferenceEquals(value, actor) && !ReferenceEquals(value, target)).ToArray();
            var offsets = Enumerable.Range(-5, 11).SelectMany(x => Enumerable.Range(-5, 11).Select(z => new Vector3(x * 3, 0, z * 3))).OrderBy(value => value.sqrMagnitude).ToArray();
            Func<NNInfo, Vector3, bool> connected = (point, requested) => point.node != null && point.node.Walkable && point.node.Area == anchor.node.Area &&
                point.node.GraphIndex == anchor.node.GraphIndex && Cleanup2Finite(point.clampedPosition) && Vector3.Distance(point.clampedPosition, requested) <= .6f;
            var refusals = new Dictionary<string, int>(); int attempts = 0; var watch = Stopwatch.StartNew(); JObject selected = null;
            Action<string> refuse = reason => { int count; refusals.TryGetValue(reason, out count); refusals[reason] = count + 1; };
            foreach (var offset in offsets)
            {
                if (selected != null || attempts >= 4096 || watch.Elapsed.TotalSeconds > 5) break;
                var start = AstarPath.active.GetNearest(anchor.clampedPosition + offset, constraint);
                if (!connected(start, anchor.clampedPosition + offset)) { refuse("origin-connected-walking-graph-clamp"); continue; }
                foreach (Vector3 heading in CompassOffsets)
                foreach (float extra in new[] { 3.5f, 3f, 2.5f })
                {
                    if (selected != null || attempts >= 4096 || watch.Elapsed.TotalSeconds > 5) break;
                    attempts++; var requested = start.clampedPosition + heading.normalized * (Mathf.Max(radius, footprint) + extra);
                    var end = AstarPath.active.GetNearest(requested, constraint);
                    if (!connected(end, requested)) { refuse("target-connected-walking-graph-clamp"); continue; }
                    if (protectedUnits.Any(value => Cleanup2CreatureSegmentDistance(value.Position, start.clampedPosition, end.clampedPosition) <
                        Mathf.Max(3.5f, actor.Corpulence + value.Corpulence + .25f))) { refuse("protected-unit-path-clearance"); continue; }
                    if (Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(start.clampedPosition + Vector3.up * 1.2f,
                        end.clampedPosition + Vector3.up * 1.2f, 0)) { refuse("native-geometry-LOS"); continue; }
                    bool supported = true; int supports = 0;
                    Vector3 side = Vector3.Cross((end.clampedPosition - start.clampedPosition).normalized, Vector3.up).normalized;
                    foreach (float width in new[] { -actor.Corpulence, 0f, actor.Corpulence })
                    {
                        NNInfo first = default(NNInfo), last = default(NNInfo);
                        for (int step = 0; step <= 12 && supported; step++)
                        {
                            var point = Vector3.Lerp(start.clampedPosition, end.clampedPosition, step / 12f) + side * width;
                            var support = AstarPath.active.GetNearest(point, constraint);
                            if (!connected(support, point) || new Vector2(support.clampedPosition.x - point.x, support.clampedPosition.z - point.z).magnitude > .15f ||
                                Mathf.Abs(support.clampedPosition.y - point.y) > .35f) { supported = false; break; }
                            if (step == 0) first = support; if (step == 12) last = support; supports++;
                        }
                        if (!supported) break;
                        GraphHitInfo hit; var nodes = new List<GraphNode>();
                        if (graph.Linecast(first.clampedPosition, last.clampedPosition, first.node, out hit, nodes) ||
                            Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(first.clampedPosition + Vector3.up * 1.2f,
                                last.clampedPosition + Vector3.up * 1.2f, 0)) { supported = false; break; }
                    }
                    if (!supported) { refuse("native-footprint-support-samples"); continue; }
                    PlaceExpandedSummoningUnit(actor, start.clampedPosition); PlaceExpandedSummoningUnit(target, end.clampedPosition);
                    actor.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                    float distance = actor.DistanceTo(target);
                    bool legal = !probe.IsUnitEnoughClose && distance > radius + 1.5f && actor.HasLOS(target) && target.HasLOS(actor) &&
                        actor.IsEnemy(target) && !target.Descriptor.State.IsDead && actor.Descriptor.State.CanMove;
                    if (!legal) { refuse("post-placement-native-range-LOS-enemy-state"); continue; }
                    selected = new JObject { ["start"] = SurveyVector(actor.Position), ["target"] = SurveyVector(target.Position), ["nativeRange"] = radius,
                        ["nativeStartingDistance"] = distance, ["nativeInitiallyInRange"] = probe.IsUnitEnoughClose, ["nativeFootprintSum"] = footprint,
                        ["graph"] = anchor.node.GraphIndex, ["connectedArea"] = anchor.node.Area, ["graphMask"] = constraint.graphMask, ["constructTags"] = tags,
                        ["protectedUnitCount"] = protectedUnits.Length, ["nativeFootprintSupportSamples"] = supports,
                        ["fullCenterAndShoulderLinecastsClear"] = true };
                }
            }
            var evidence = new JObject { ["key"] = key, ["selected"] = selected, ["attempts"] = attempts, ["refusals"] = JObject.FromObject(refusals),
                ["surveyWallSeconds"] = watch.Elapsed.TotalSeconds,
                ["contract"] = "Installed PathTo060018A3/ConstructTags060018C3 RTWP mask1; query-owned PathNNConstraint.SetStart + connected-area proof, <=60cm endpoint clamp,39 footprint samples with <=15cm horizontal/35cm vertical support error, full native graph/geometry center-and-shoulder linecasts and protected-unit segment clearance. Real UnitAttack.IsUnitEnoughClose must initially be false and must become true at its actual rule event. No native graph, range, collider or path is changed." };
            Cleanup2Check("creature-native-approach-lane-" + key, selected != null, evidence,
                "A finite bounded actual walking-graph lane proves a safe out-of-range start for native command-owned approach.");
            if (selected == null) throw new InvalidOperationException("No proved safe straight creature approach lane; no placement or command proof is inferred.");
            return new Cleanup2CreatureApproach { Range = probe, Evidence = evidence };
        }

        private static float Cleanup2CreatureSegmentDistance(Vector3 point, Vector3 start, Vector3 end)
        {
            point.y = start.y = end.y = 0;
            Vector3 delta = end - start;
            float fraction = delta.sqrMagnitude <= .000001f ? 0 : Mathf.Clamp01(Vector3.Dot(point - start, delta) / delta.sqrMagnitude);
            return Vector3.Distance(point, start + delta * fraction);
        }
    }
}
