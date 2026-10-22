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
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Cleanup2ProductionAttackResult
        {
            internal float[] Starts, Deadlines, Acts, TickDeltas;
            internal string Type;
            internal int Events, Costs;
        }

        // Actual ordinary commands. Baseline suppresses ONLY this actor's
        // render offsets; both runs retain null handles + native ScheduleAct.
        private IEnumerable<int> ReviewSummonCleanup2ProductionAttacks(
            ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion ||
                (string)_request.Parameters?["reviewPhase"] != "production-attacks" || fixture == null || book == null ||
                _sprint25Clock == null || _expandedSummoningPlayerPathCaptureActive || _refinementUnit != null ||
                TurnBased.Controllers.CombatController.IsInTurnBasedCombat())
                throw new InvalidOperationException("Exact idle save-free RTWP production-attacks fixture/book/clock required.");
            bool pause = Game.Instance.IsPaused, waiting = _sprint25WaitingForUnityAppearance;
            var oldCapture = ExpandedSummoningRuleCapture.ToArray(); var oldEvents = ExpandedSummoningPlayerPathEvents.ToArray();
            int oldCasts = _expandedSummoningPlayerPathRuleCastCount, oldSpawns = _expandedSummoningPlayerPathSpawnActionCount;
            var hooks = HarmonyInstance.Create("kmg.cleanup2.production-attacks." + _request.RunId);
            var watch = Stopwatch.StartNew(); UnitEntityData actor = null; IDisposable suppression = null;
            var sources = new List<Cleanup2SharedViewReceipt>();
            try
            {
                if (fixture.Hostile == null) CreateExpandedSummoningCorrectionHostile(fixture);
                UnitEntityData target = fixture.Hostile;
                if (target == null || target.Destroyed || !ReferenceEquals(target.Blueprint, fixture.HostileBlueprint) ||
                    fixture.UnitsBefore.Any(value => ReferenceEquals(value, target))) throw new InvalidOperationException("Exact request-created hostile unavailable.");
                const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true; _sprint25WaitingForUnityAppearance = false;
                foreach (string key in new[] { SummonAttackGesturePolicy.Crab, SummonAttackGesturePolicy.Brachiosaurus })
                {
                    Cleanup2ProductionAttackDeadline(watch);
                    bool crab = key == SummonAttackGesturePolicy.Crab;
                    var native = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                        crab ? CrabSummonAnimationBindingPolicy.SpiderGuid : GiantBodyVisualPolicy.DonorGuid, "protected production gesture donor");
                    var sourceView = native.Prefab.Load(false);
                    Cleanup2SharedViewReceipt source = Cleanup2SharedTakeView(sourceView, true); sources.Add(source);
                    var route = Cleanup2GiantRoute(fixture, key, SummonFamily.NaturesAlly);
                    bool exactRoute = route.Tier == (crab ? 3 : 7) && route.Unit.AssetGuid == (crab ? CrabSummonAnimationBindingPolicy.UnitGuid : SummonAttackGesturePolicy.BrachiosaurusUnit) &&
                        route.Selected.AssetGuid == (crab ? CrabSummonAnimationBindingPolicy.SourceGuids[0] : SummonAttackGesturePolicy.BrachiosaurusSources[0]) &&
                        route.Unit.Prefab.AssetId == native.Prefab.AssetId;
                    Cleanup2Check("production-attack-route-" + key, exactRoute, new JObject { ["key"] = key, ["tier"] = route.Tier,
                        ["parent"] = route.Parent.AssetGuid, ["selected"] = route.Selected.AssetGuid, ["unit"] = route.Unit.AssetGuid,
                        ["donor"] = native.AssetGuid, ["prefab"] = native.Prefab.AssetId,
                        ["accessLimitation"] = "Disposable Wizard is granted the real canonical SNA parent/prepared slot; natural class access is not being proved.",
                        ["clockLimitation"] = "Existing qualified Dialog-host Sprint22NativeClock is frameSynchronous=true and advances native GameTime/actions/agent by actual Unity Time.deltaTime, recorded at each act. The nominal .05 constructor value is not the active step; not natural Default-mode or turn-based timing qualification.",
                        ["targetHPFixture"] = target.Stats.HitPoints.ModifiedValue, ["maximumWallSeconds"] = 240 },
                        "Actual canonical SNA III Crab/SNA VII Brachiosaurus parent, child, prepared slot and unchanged native donor are exact.");
                    if (!exactRoute) throw new InvalidOperationException("Production gesture route changed.");
                    Game.Instance.IsPaused = false; actor = Cleanup2CastGiant(fixture, book, route);
                    foreach (int frame in Cleanup2WaitAppearance(actor, "production-" + key)) { Cleanup2ProductionAttackDeadline(watch); yield return frame; }
                    if (!ExpandedSummoningPlayerPathUnitExact(actor, fixture.Caster)) throw new InvalidOperationException("Positive production gesture summon lost.");
                    var visual = SummonAttackGestureVisual.For(actor); var manager = actor.View.AnimationManager;
                    var special = crab ? UnitAnimationSpecialAttackType.Claw : UnitAnimationSpecialAttackType.Tail;
                    bool ready = visual != null && visual.Ready && visual.Key == key && visual.ProveCurrentPose() && visual.ProveSameFrameSourceState() &&
                        manager != null && manager.GetAction(special) == null && ExpandedSummoningPteranodonViewPatch.HasLiveOriginalVisual(actor.View, key) &&
                        sourceView.GetComponent<SummonAttackGestureVisual>() == null;
                    Cleanup2Check("production-gesture-ready-" + key, ready, new JObject { ["owner"] = actor.UniqueId, ["gesture"] = visual?.Describe(),
                        ["nativeSpecialLookupNull"] = manager != null && manager.GetAction(special) == null,
                        ["nativePrefabHasNoProjectGesture"] = sourceView.GetComponent<SummonAttackGestureVisual>() == null,
                        ["sharedPrefabBefore"] = source.Scalars.DeepClone(), ["actualShapeHierarchy"] = Cleanup2ShapeHierarchy(actor),
                        ["hitAnimation"] = manager?.GetAction(UnitAnimationType.Hit) == null ? "UNSUPPORTED_NATIVE_DONOR_HAS_NO_HIT_ACTION" : "native-present-not-exercised",
                        ["notClaimed"] = "No native clip alias, fake handle, precise enemy contact, tabletop reach or owner art approval." },
                        "Exact private current-pose skin equality, source/collider invariance and unchanged production missing-handle lookup gate gesture observation.");
                    if (!ready) throw new InvalidOperationException("Original private gesture palette refused.");
                    ItemEntityWeapon[] limbs = Sprint19Limbs(actor);
                    if (limbs.Length != (crab ? 2 : 1) || limbs.Any(value => value == null || value.Blueprint.AssetGuid != (crab ? CrabSummonAnimationBindingPolicy.ClawGuid : SummonAttackGesturePolicy.TailWeapon)))
                        throw new InvalidOperationException("Exact released native weapon routine changed.");
                    JObject stats = null, mechanics = null;
                    JObject spatialIntake = (JObject)Cleanup2ScaleMechanics(actor).DeepClone();
                    Cleanup2SharedViewReceipt live = Cleanup2SharedTakeView(actor.View);
                    foreach (bool full in new[] { false, true })
                    {
                        Cleanup2ProductionAttackResult baseline = null;
                        foreach (bool enabled in new[] { false, true })
                        {
                            Cleanup2ProductionAttackDeadline(watch);
                            string label = key + "-" + (full ? "full" : "single") + (enabled ? "-on" : "-off");
                            actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                            target.Commands.InterruptAll(true); target.Commands.RemoveFinishedAndUpdateQueue();
                            Sprint20ReleaseOwnHold(fixture, actor); actor.JoinCombat(); target.JoinCombat();
                            JObject lane = Cleanup2CrabAttackLane(fixture, actor, target, "production-" + label);
                            actor.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                            if (mechanics == null)
                            {
                                // Installed 2.1.7b UnitMovementAgentBase.Corpulence
                                // returns .5 outside CombatMode, otherwise the
                                // unit view's Corpulence (native-contract-il.txt
                                // token 0x060018D5). The qualified clock runs
                                // native join/prepare/action/movement controllers.
                                // Snapshot after that ordinary spatial transition,
                                // before either matched command is issued.
                                int pulse = _sprint25Clock.Pulses, start = pulse, stable = 0;
                                JObject previous = null;
                                for (int settle = 0; settle < 180 && stable < 2; settle++)
                                {
                                    Cleanup2ProductionAttackDeadline(watch); yield return 0;
                                    if (_sprint25Clock.Pulses == pulse) continue;
                                    pulse = _sprint25Clock.Pulses;
                                    JObject sample = Cleanup2ScaleMechanics(actor);
                                    bool spatialReady = actor.IsInCombat && actor.View?.MovementAgent != null &&
                                        actor.View.MovementAgent.Corpulence == actor.View.Corpulence;
                                    stable = spatialReady && previous != null && JToken.DeepEquals(previous, sample) ? stable + 1 : 0;
                                    previous = (JObject)sample.DeepClone();
                                }
                                bool settled = stable >= 2 && _sprint25Clock.Pulses > start;
                                Cleanup2Check("production-attack-spatial-baseline-" + key, settled,
                                    new JObject { ["beforeNativeJoinAndLane"] = spatialIntake.DeepClone(),
                                        ["afterNativeSettlement"] = previous?.DeepClone(), ["nativeClockPulses"] = _sprint25Clock.Pulses-start,
                                        ["consecutiveExactStableTransitions"] = stable,
                                        ["contract"] = "Native agent getter uses .5 outside CombatMode and view Corpulence in combat; no corpulence/agent/collider setter is used.",
                                        ["baselineScope"] = "One settled baseline retained across all render-off/on single/full commands for this actor." },
                                    "Native combat/spatial state must settle before exact mechanics comparisons, without issuing an attack or overriding initialization.");
                                if (!settled) throw new InvalidOperationException("Native production attack spatial baseline did not settle.");
                                mechanics = (JObject)previous.DeepClone();
                                stats = (JObject)Cleanup2CrabBaseStats(actor).DeepClone();
                            }
                            var probe = new UnitAttack(target); probe.Init(actor);
                            bool legal = probe.IsUnitEnoughClose && actor.HasLOS(target) && actor.IsEnemy(target) && !target.IsBrainActive && !target.Descriptor.State.IsDead;
                            Cleanup2Check("production-attack-lane-" + label, legal, new JObject { ["lane"] = lane,
                                ["nativeInRange"] = probe.IsUnitEnoughClose, ["nativeApproachRadius"] = probe.ApproachRadius,
                                ["nativeDistance"] = actor.DistanceTo(target), ["owner"] = actor.UniqueId, ["target"] = target.UniqueId },
                                "Actual native range/LOS/enemy/graph/party clearance is proved before this ordinary command; no range or rule override.");
                            if (!legal) throw new InvalidOperationException("No legal ordinary production attack lane.");
                            if (!enabled) suppression = visual.SuppressForReview();
                            actor.Memory.Add(target); target.Memory.Add(actor);
                            var command = new UnitAttack(target) { ForceFullAttack = full, IsSingleAttack = !full };
                            var events = new JArray(); var failures = new List<string>(); var images = new HashSet<string>(); var poses = new JArray();
                            var observer = new Sprint24AttackObserver { Owner = actor, Target = target };
                            observer.AfterWeapon = attack => {
                                int index = command.GetAttackIndex(); var info = command.PlannedAttack;
                                var matches = visual.Schedules.Where(value => ReferenceEquals(value.Command, command) && value.Index == index).ToArray();
                                bool linked = matches.Length == 1 && ReferenceEquals(matches[0].Info, info) && ReferenceEquals(matches[0].Weapon, attack.Weapon) &&
                                    ReferenceEquals(info?.Weapon, attack.Weapon) && matches[0].ActTime >= matches[0].Start && matches[0].EligibleTicks == 1 &&
                                    info.AnimationHandle == null && command.Animation == null;
                                if (!linked) failures.Add("actual-rule-event-native-schedule-info-weapon-association");
                                if (events.Count >= 8) { failures.Add("event-bound"); return; }
                                events.Add(new JObject { ["frame"] = Time.frameCount, ["index"] = index, ["linked"] = linked,
                                    ["commandTime"] = command.TimeSinceStart, ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                                    ["infoIdentity"] = info == null ? 0 : RuntimeHelpers.GetHashCode(info), ["weaponIdentity"] = RuntimeHelpers.GetHashCode(attack.Weapon),
                                    ["weapon"] = attack.Weapon.Blueprint.AssetGuid, ["target"] = attack.Target?.UniqueId,
                                    ["hit"] = attack.AttackRoll?.IsHit, ["naturalRoll"] = attack.AttackRoll?.Roll.Value,
                                    ["autoMiss"] = attack.AttackRoll?.AutoMiss, ["suspendLog"] = attack.AttackRoll?.SuspendCombatLog,
                                    ["charge"] = attack.IsCharge, ["attackOfOpportunity"] = attack.IsAttackOfOpportunity,
                                    ["damage"] = attack.MeleeDamage?.Damage, ["scheduleSerial"] = matches.Length == 1 ? matches[0].Serial : 0 });
                            };
                            var costs = new JArray(); var endings = new JArray(); var commandWatch = Stopwatch.StartNew();
                            int startPulse = _sprint25Clock.Pulses, frames = 0; double startGame = Game.Instance.Player.GameTime.TotalSeconds;
                            float maximumSkinMotion = 0f, maximumProtectedMotion = 0f; bool posesFinite = true, sourceInvariant = true, zeroEqual = true;
                            EventBus.Subscribe(observer);
                            try
                            {
                                using (var economy = new FirearmReviewCosts(actor, costs))
                                using (var trace = new Sprint26CommandTrace(actor, endings))
                                {
                                    if (enabled)
                                    {
                                        string file = "production-" + label + "-ready.png";
                                        bool saved = Cleanup2ProductionAttackImage(actor, target, file, crab).StartsWith("png=" + file + ";bytes=", StringComparison.Ordinal) && File.Exists(Path.Combine(_request.EvidenceDirectory, file));
                                        Cleanup2Check("production-ready-" + label, saved, new JObject { ["file"] = file,
                                            ["beforeCommandRun"] = true, ["preActWindup"] = "ABSENT: native HasAnimation=false may act on its first tick; this is a neutral ready frame, not an invented windup." },
                                            "Actual neutral own body is captured before issuing the ordinary native command.");
                                        if (!saved) throw new InvalidOperationException("Neutral ready image unavailable."); images.Add("ready");
                                    }
                                    actor.Commands.Run(command);
                                    while (frames++ < 3000 && commandWatch.Elapsed.TotalSeconds < 35 && watch.Elapsed.TotalSeconds < 240)
                                    {
                                        if (!ExpandedSummoningPlayerPathUnitExact(actor, fixture.Caster) || !visual.Ready || manager.GetAction(special) != null)
                                            throw new InvalidOperationException("Exact actor/gesture/native null-handle scope changed during command.");
                                        var records = visual.Schedules.Where(value => ReferenceEquals(value.Command, command)).ToArray();
                                        sourceInvariant &= visual.ProveSameFrameSourceState();
                                        if (frames % 4 == 0 && records.Length > 0)
                                        {
                                            zeroEqual &= visual.ProveCurrentPose(); JObject shape = visual.MeasureGestureDisplacement();
                                            posesFinite &= (bool)shape["finite"];
                                            maximumSkinMotion = Mathf.Max(maximumSkinMotion, (float)shape["maximumGestureRendererLocalDisplacement"]);
                                            maximumProtectedMotion = Mathf.Max(maximumProtectedMotion, (float)shape["maximumUnaffectedRendererLocalDisplacement"]);
                                            if (poses.Count < 256) poses.Add(new JObject { ["frame"] = Time.frameCount, ["commandTime"] = command.TimeSinceStart,
                                                ["shape"] = shape, ["gesture"] = visual.Describe() });
                                            else failures.Add("pose-bound");
                                        }
                                        if (enabled)
                                            foreach (var record in records)
                                            {
                                                float elapsed = record.ActTime < 0 ? 0f :
                                                    1f + (float)(Game.Instance.Player.GameTime.TotalSeconds - record.ActGame);
                                                string moment = record.ActTime >= 0 && elapsed < 1.18f ? "act" : record.ActTime >= 0 && elapsed >= 1.22f && elapsed < 1.48f ? "recover" : null;
                                                string imageKey = record.Index + "-" + moment;
                                                if (moment == null || images.Contains(imageKey)) continue;
                                                string file = "production-" + label + "-attack" + record.Index + "-" + moment + ".png";
                                                string saved = Cleanup2ProductionAttackImage(actor, target, file, crab);
                                                bool present = saved.StartsWith("png=" + file + ";bytes=", StringComparison.Ordinal) && File.Exists(Path.Combine(_request.EvidenceDirectory, file));
                                                JObject imageShape = visual.MeasureGestureDisplacement(); posesFinite &= (bool)imageShape["finite"];
                                                maximumSkinMotion = Mathf.Max(maximumSkinMotion, (float)imageShape["maximumGestureRendererLocalDisplacement"]);
                                                maximumProtectedMotion = Mathf.Max(maximumProtectedMotion, (float)imageShape["maximumUnaffectedRendererLocalDisplacement"]);
                                                Cleanup2Check("production-pose-" + label + "-" + imageKey, present, new JObject { ["file"] = file,
                                                    ["actualNativeScheduleSerial"] = record.Serial, ["plannedIndex"] = record.Index, ["nativeCommandTime"] = command.TimeSinceStart,
                                                    ["nativeScheduleStart"] = record.Start, ["nativeActTime"] = record.ActTime, ["renderCurveElapsed"] = elapsed,
                                                    ["geometrySample"] = imageShape, ["gesture"] = visual.Describe(), ["forcedActOrHandle"] = false },
                                                    "Actual native fallback clock/act drives this original private skin pose and normally visible native camera image.");
                                                if (!present) failures.Add("missing-native-pose-image"); images.Add(imageKey);
                                            }
                                        if (command.IsFinished && records.Any(value => value.Cancelled || value.ActGame < 0))
                                        { failures.Add("terminal-native-act-branch-refused"); break; }
                                        bool recoveryComplete = records.Length > 0 && records.All(value => value.ActGame >= 0 &&
                                            Game.Instance.Player.GameTime.TotalSeconds - value.ActGame >= .55d);
                                        if (command.IsFinished && (records.Length == 0 || recoveryComplete)) break;
                                        Game.Instance.IsPaused = false; yield return 0;
                                    }
                                    var final = visual.Schedules.Where(value => ReferenceEquals(value.Command, command)).OrderBy(value => value.Index).ToArray();
                                    int expected = full ? limbs.Length : 1;
                                    bool association = failures.Count == 0 && final.Length == expected && command.AllAttacks.Count == expected &&
                                        Enumerable.Range(0, expected).All(index => final[index].Index == index && ReferenceEquals(final[index].Info, command.AllAttacks[index]) &&
                                            ReferenceEquals(final[index].Weapon, limbs[index]) && final[index].Suppressed == !enabled && !final[index].Cancelled &&
                                            final[index].Info.AnimationHandle == null && Mathf.Abs(final[index].Delay - 1f) <= .00001f &&
                                            SummonAttackGesturePolicy.ImmediateNativeAct(final[index].HasAnimationAtSchedule, final[index].HasAnimationAtAct,
                                                final[index].OneFrameAtAct, final[index].HandledAtSchedule, final[index].HandledAtAct,
                                                final[index].Start, final[index].ActTime, final[index].NativeActTickDelta, final[index].EligibleTicks));
                                    bool motion = posesFinite && zeroEqual && sourceInvariant && maximumProtectedMotion <= .0002f &&
                                        (enabled ? maximumSkinMotion > .01f && final.All(value => value.Samples >= 4 && value.MaximumAngle > 1f) && images.Count == expected * 2 + 1 : maximumSkinMotion <= .0002f);
                                    JObject currentStats = Cleanup2CrabBaseStats(actor), currentMechanics = Cleanup2ScaleMechanics(actor);
                                    JObject currentSource = Cleanup2SharedViewScalars(source.View, source.PrefabAsset), currentLive = Cleanup2SharedViewScalars(live.View, live.PrefabAsset);
                                    var nativePredicates = new JObject {
                                        ["finished"] = command.IsFinished, ["successOrNativeExhausted"] = command.Result == UnitCommand.ResultType.Success || trace.Exhausted(command),
                                        ["targetWeaponEventCount"] = observer.WeaponAttacks.Count == expected, ["ownerWeaponEventCount"] = observer.OwnerWeaponAttacks.Count == expected,
                                        ["rollEventCount"] = observer.Rolls.Count == expected, ["exactWeaponEventOrder"] = observer.WeaponAttacks.Select(value => value.Weapon).SequenceEqual(limbs.Take(expected)),
                                        ["ordinaryNaturalRuleFlags"] = observer.WeaponAttacks.All(value => value.AttackRoll != null && !value.AttackRoll.AutoMiss && !value.AttackRoll.SuspendCombatLog && !value.IsCharge && !value.IsAttackOfOpportunity),
                                        ["oneNativeCost"] = economy.Count(command) == 1, ["exactNativeCost"] = economy.Exact(command, false), ["noForcedCostFinish"] = economy.ForcedCount(command) == 0,
                                        ["notCutscene"] = !command.Cutscene, ["notIgnoreCooldown"] = !command.IsIgnoreCooldown, ["noOverrideAction"] = command.OverrideAction == null,
                                        ["nullAnimation"] = command.Animation == null, ["targetAlive"] = !target.Descriptor.State.IsDead,
                                        ["baseStatsExact"] = JToken.DeepEquals(currentStats, stats), ["mechanicsExact"] = JToken.DeepEquals(currentMechanics, mechanics),
                                        ["weaponEntitiesExact"] = Sprint19Limbs(actor).SequenceEqual(limbs), ["protectedSourceViewExact"] = Cleanup2SharedSameView(source),
                                        ["liveActorViewExact"] = Cleanup2SharedSameView(live) };
                                    bool nativeProof = nativePredicates.Properties().All(value => (bool)value.Value);
                                    Func<JObject,JObject,JArray> changedFields = (before, after) => new JArray(before.Properties()
                                        .Where(value => !JToken.DeepEquals(value.Value, after[value.Name])).Take(32).Select(value => value.Name));
                                    var result = new Cleanup2ProductionAttackResult { Starts = final.Select(value => value.Start).ToArray(), Deadlines = final.Select(value => value.Deadline).ToArray(),
                                        Acts = final.Select(value => value.ActTime).ToArray(), TickDeltas = final.Select(value => value.NativeActTickDelta).ToArray(),
                                        Type = command.Type.ToString(), Events = observer.WeaponAttacks.Count, Costs = economy.Count(command) };
                                    bool matched = !enabled || baseline != null && baseline.Type == result.Type && baseline.Events == result.Events && baseline.Costs == result.Costs &&
                                        baseline.Acts.Length == result.Acts.Length && Enumerable.Range(0, result.Acts.Length).All(index =>
                                            baseline.TickDeltas[index] > 0 && result.TickDeltas[index] > 0 &&
                                            Mathf.Abs((baseline.Acts[index] - baseline.Starts[index]) / baseline.TickDeltas[index] -
                                                (result.Acts[index] - result.Starts[index]) / result.TickDeltas[index]) <= .005f &&
                                            Mathf.Abs((baseline.Deadlines[index] - baseline.Starts[index]) - (result.Deadlines[index] - result.Starts[index])) <= .0001f);
                                    bool passed = association && motion && nativeProof && matched;
                                    Cleanup2Check("production-attack-resolution-" + label, passed, new JObject { ["owner"] = actor.UniqueId, ["target"] = target.UniqueId,
                                        ["renderGestureEnabled"] = enabled, ["nativeResult"] = command.Result.ToString(), ["nativeListExhausted"] = trace.Exhausted(command),
                                        ["plannedAttacks"] = command.AllAttacks.Count, ["expectedAttacks"] = expected, ["events"] = events, ["costs"] = costs, ["endings"] = endings,
                                        ["scheduleAssociation"] = association, ["nativeRulesAndCostProof"] = nativeProof,
                                        ["nativePredicates"] = nativePredicates,
                                        ["actualTargetWeaponEvents"] = observer.WeaponAttacks.Count, ["actualOwnerWeaponEvents"] = observer.OwnerWeaponAttacks.Count,
                                        ["actualRollEvents"] = observer.Rolls.Count, ["actualCostApplications"] = economy.Count(command), ["actualForcedCostFinishes"] = economy.ForcedCount(command),
                                        ["changedBaseStatsFields"] = changedFields(stats, currentStats),
                                        ["changedMechanicsFields"] = changedFields(mechanics, currentMechanics),
                                        ["changedSourceViewFields"] = changedFields(source.Scalars, currentSource), ["changedLiveViewFields"] = changedFields(live.Scalars, currentLive),
                                        ["mechanicsChanges"] = new JArray(mechanics.Properties().Where(value => !JToken.DeepEquals(value.Value,currentMechanics[value.Name])).Take(32)
                                            .Select(value => new JObject { ["field"] = value.Name, ["before"] = value.Value.DeepClone(), ["after"] = currentMechanics[value.Name]?.DeepClone() })),
                                        ["matchedBaselineOnScheduleAndCost"] = matched, ["failures"] = new JArray(failures.Distinct()),
                                        ["nativeActTickDeltas"] = new JArray(result.TickDeltas),
                                        ["nativeActLatencies"] = new JArray(result.Acts.Select((value, index) => value - result.Starts[index])),
                                        ["nativeTickNormalizedLatencies"] = new JArray(result.Acts.Select((value, index) => result.TickDeltas[index] <= 0 ? (JToken)JValue.CreateNull() : new JValue((value - result.Starts[index]) / result.TickDeltas[index]))),
                                        ["preActWindup"] = "ABSENT; neutral ready-before-command plus actual native act/recovery frames only",
                                        ["positiveActualSkinMotionOrExactSuppression"] = motion, ["maximumSkinDisplacement"] = maximumSkinMotion,
                                        ["maximumProtectedSkinDisplacement"] = maximumProtectedMotion, ["sameFrameSourceAndColliderInvariant"] = sourceInvariant,
                                        ["actualCurrentPoseZeroOffsetEquality"] = zeroEqual, ["poses"] = poses, ["gesture"] = visual.Describe(),
                                        ["nativeCommandTime"] = command.TimeSinceStart, ["wallSeconds"] = commandWatch.Elapsed.TotalSeconds,
                                        ["frames"] = frames, ["clockPulses"] = _sprint25Clock.Pulses - startPulse, ["gameTimeDelta"] = Game.Instance.Player.GameTime.TotalSeconds - startGame,
                                        ["forcedRolls"] = 0, ["forcedActs"] = 0, ["fixtureAttackRulesTriggered"] = 0, ["nativeAnimationHandlesFabricated"] = 0,
                                        ["limits"] = "Matched native RTWP commands under the actual frame-synchronous fixture clock only. Exact HasAnimation=false immediate Tick/handled transition and first eligible tick are required, unchanged stored ScheduleAct(1) is not a minimum delay. Paired latency is normalized by each actual native act tick delta (tolerance .005 tick), not enlarged wall-clock tolerance. No pre-act windup, natural Default-mode/turn-based timing, exact enemy contact/reach, Hit or art approval claim." },
                                        "Ordinary missing-handle single/full commands retain exact schedules, weapon events and one native cost; only private original skin art differs in the matched render-on run.");
                                    if (!passed) throw new InvalidOperationException("Production render gesture native proof failed: " + label);
                                    if (!enabled) baseline = result;
                                }
                            }
                            finally
                            {
                                EventBus.Unsubscribe(observer);
                                try { suppression?.Dispose(); suppression = null; }
                                finally { actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue(); Sprint20ReleaseOwnHold(fixture, actor); }
                            }
                        }
                    }
                    visual.Release(); JObject released = visual.Describe();
                    bool clean = (bool)released["released"] && (int)released["ownedTransformCount"] == 0 &&
                        (bool)released["releasedNativePaletteRestored"] && (bool)released["releasedPrivateBindRestored"] &&
                        (bool)released["releasedCullingBoundsRestored"] && Cleanup2SharedSameView(source) && manager.GetAction(special) == null;
                    Cleanup2Check("production-gesture-release-" + key, clean, new JObject { ["gesture"] = released,
                        ["protectedSourceUnchanged"] = Cleanup2SharedSameView(source), ["nativeLookupStillNull"] = manager.GetAction(special) == null },
                        "Exact owned transform palette is released while its actor is live; borrowed source assets and null native lookup remain intact.");
                    if (!clean) throw new InvalidOperationException("Owned production gesture release incomplete.");
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { actor }); actor = null;
                    for (int frame = 0; frame < 4; frame++) { Cleanup2ProductionAttackDeadline(watch); yield return 0; }
                }
            }
            finally
            {
                var failures = new List<string>();
                Action<string, Action> restore = (label, action) => { try { action(); } catch (Exception error) { failures.Add(label + ":" + error.GetType().Name + ":" + error.Message); } };
                try
                {
                    restore("render-suppression", () => suppression?.Dispose()); _expandedSummoningPlayerPathCaptureActive = false;
                    restore("hooks", () => hooks.UnpatchAll(hooks.Id));
                    if (actor != null && !actor.Destroyed)
                    {
                        restore("owned-command", () => { actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue(); });
                        restore("owned-hold", () => Sprint20ReleaseOwnHold(fixture, actor));
                        restore("owned-palette", () => SummonAttackGestureVisual.For(actor)?.Release());
                    }
                    restore("own-hostile", () => ResetExpandedSummoningHostile(fixture));
                    ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningRuleCapture.AddRange(oldCapture);
                    ExpandedSummoningPlayerPathEvents.Clear(); ExpandedSummoningPlayerPathEvents.AddRange(oldEvents);
                    _expandedSummoningPlayerPathRuleCastCount = oldCasts; _expandedSummoningPlayerPathSpawnActionCount = oldSpawns;
                }
                finally { _sprint25WaitingForUnityAppearance = waiting; restore("pause", () => Game.Instance.IsPaused = pause); }
                bool sourcesExact = sources.All(Cleanup2SharedSameView);
                Cleanup2Check("production-attacks-observer-restored", failures.Count == 0 && sourcesExact, new JObject { ["failures"] = new JArray(failures),
                    ["protectedNativeSourcesExact"] = sourcesExact, ["wallSeconds"] = watch.Elapsed.TotalSeconds, ["maximumWallSeconds"] = 240,
                    ["sharedFixtureOwnsPositiveSummonAndHostileTeardown"] = true, ["humanSaveWrites"] = 0, ["nativeAssetWrites"] = 0 },
                    "Only exact disposable commands/render suppression/capture state are cleaned; positive shared fixture teardown owns its actors, with native sources untouched.");
                if (failures.Count > 0 || !sourcesExact) throw new InvalidOperationException("Production attack observer restoration failed.");
            }
        }
        private static void Cleanup2ProductionAttackDeadline(Stopwatch watch)
        { if (watch.Elapsed.TotalSeconds >= 240) throw new TimeoutException("Production attack 240-second phase bound."); }

        private string Cleanup2ProductionAttackImage(UnitEntityData actor, UnitEntityData target, string file, bool crab)
        {
            if (File.Exists(Path.Combine(_request.EvidenceDirectory, file))) throw new InvalidOperationException("Production attack image already exists.");
            Bounds previous = _refinementBounds;
            Camera camera = TeleportationCastingCamera()?.Camera ?? Camera.main;
            if(camera==null) throw new InvalidOperationException("Native contact camera absent.");
            float beforeSize=camera.orthographicSize,beforeAspect=camera.aspect;
            try
            {
                var visual = SummonAttackGestureVisual.For(actor);
                _refinementBounds = visual.CurrentFramingBounds();
                foreach(Renderer body in target.View.GetComponentsInChildren<Renderer>(true)
                    .Where(r=>r.enabled && r.gameObject.activeInHierarchy && (r is SkinnedMeshRenderer || r is MeshRenderer)))
                    _refinementBounds.Encapsulate(body.bounds);
                _refinementBounds.Encapsulate(target.Position);
                _refinementBounds.Expand(_refinementBounds.size.magnitude*.16f);
                Vector3 center = _refinementBounds.center;
                Vector3 direction = (actor.View.transform.right * (crab ? .8f : 1.4f) - actor.View.transform.forward * (crab ? 1.2f : -.6f) + Vector3.up * .7f).normalized;
                Quaternion rotation=Quaternion.LookRotation(-direction,Vector3.up);
                Vector3[] points=Cleanup2BoundsCorners(_refinementBounds).Select(p=>Quaternion.Inverse(rotation)*(p-center)).ToArray();
                camera.aspect=(float)RefinementFrameWidth/RefinementFrameHeight;
                float halfFrame=Mathf.Max(points.Max(p=>Mathf.Abs(p.y)),points.Max(p=>Mathf.Abs(p.x))/camera.aspect)*1.12f;
                float distance=points.Max(p=>Mathf.Abs(p.z))+Mathf.Max(3.5f,halfFrame/Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f));
                if(camera.orthographic) camera.orthographicSize=halfFrame;
                Vector3 eye = center + direction * distance;
                return WriteRefinementImage(eye, rotation, file);
            }
            finally { _refinementBounds = previous; camera.orthographicSize=beforeSize; camera.aspect=beforeAspect; }
        }
    }
}
