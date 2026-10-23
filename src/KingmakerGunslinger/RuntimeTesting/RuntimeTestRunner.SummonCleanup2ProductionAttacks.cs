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
                    // One additional render-enabled command uses actual native
                    // Default progression, after the matched fixture-clock proof.
                    _cleanup2BodyOrdinaryAttackProved=false;
                    foreach(int frame in Cleanup2BodyOrdinaryAttack(fixture,actor,key))yield return frame;
                    if(!_cleanup2BodyOrdinaryAttackProved)
                        throw new InvalidOperationException("Ordinary Default production gesture command failed: "+key);
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
                finally { _sprint25WaitingForUnityAppearance = Cleanup2NativeDefaultPending || waiting; restore("pause", () => Game.Instance.IsPaused = pause); }
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
                _refinementBounds = visual == null ? new Bounds(actor.Position,Vector3.one*.1f) : visual.CurrentFramingBounds();
                if(visual==null)
                    foreach(Renderer body in actor.View.GetComponentsInChildren<Renderer>(true)
                        .Where(r=>r.enabled && r.gameObject.activeInHierarchy && (r is SkinnedMeshRenderer || r is MeshRenderer)))
                        _refinementBounds.Encapsulate(body.bounds);
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
        private string Cleanup2OrdinaryAttackImage(UnitEntityData actor,UnitEntityData target,string file)
        { return Cleanup2ProductionAttackImage(actor,target,file,SummonAttackGestureVisual.For(actor)?.Key==SummonAttackGesturePolicy.Crab); }

        // Exact creature-commands capture lease only. Normal native rendering
        // must update the occlusion command buffer at this same held pose;
        // synchronous Camera.Render at a relocated pose cannot establish that.
        private sealed class Cleanup2CreatureCameraObstructed : InvalidOperationException
        {
            internal readonly bool BudgetExceeded;
            internal Cleanup2CreatureCameraObstructed(bool budgetExceeded) : base(budgetExceeded ?
                "No native creature camera admitted before the bounded joint lane/camera survey expired." :
                "All bounded native creature camera candidates have demonstrated obstructions.") { BudgetExceeded = budgetExceeded; }
        }

        private sealed class Cleanup2CreatureCommandCamera : IDisposable
        {
            private readonly RuntimeTestRunner _owner;
            private readonly ExpandedSummoningCorrectionFixture _fixture;
            private readonly UnitEntityData _actor, _targetUnit;
            private readonly string _key;
            private readonly int _candidateIndex;
            private readonly Func<bool> _modeCurrent;
            private readonly Kingmaker.View.CameraRig _rig;
            private readonly Kingmaker.Controllers.Rest.CameraController _controller;
            private readonly Kingmaker.Utility.CountingGuard _guard;
            private readonly Camera _camera;
            private readonly Kingmaker.Visual.OccludedObjectHighlighting.OccludedObjectHighlight _occlusion;
            private readonly object _follower, _zoom;
            private readonly FieldInfo[] _followerFields, _zoomFields, _controlFields, _rigFields;
            private readonly object[] _followerValues, _zoomValues, _controlValues, _rigValues;
            private readonly Vector3 _beforeRigPosition, _beforeRigTarget, _beforeCameraPosition;
            private readonly Quaternion _beforeRigRotation, _beforeCameraRotation;
            private readonly Transform _cameraParent;
            private readonly float _beforeSize, _beforeAspect, _beforeFieldOfView;
            private readonly RenderTexture _beforeTarget;
            private readonly UnityEngine.PostProcessing.PostProcessingBehaviour _post;
            private readonly UnityEngine.PostProcessing.PostProcessingProfile _profile;
            private readonly UnityEngine.PostProcessing.DepthOfFieldModel _depthOfField;
            private readonly bool _beforeDepthOfField;
            private readonly Camera.CameraCallback _rendered;
            private readonly SkinnedMeshRenderer _bodySkin;
            private readonly Mesh _bodyMesh;
            private Mesh _renderBoundsMesh;
            private RenderTexture _target;
            private RenderTexture _renderedTarget;
            private Bounds _renderedBounds;
            private bool _renderedVisible;
            private Vector3 _position;
            private Quaternion _rotation;
            private float _size;
            private bool _guardOwned, _subscribed, _disposed, _candidateRefused;
            private string _renderError;
            internal int RenderCount, RenderedFrame = -1;
            internal double RenderedGameSeconds = double.NegativeInfinity;
            internal Vector3 RenderedActorPosition;
            internal readonly JObject Evidence = new JObject();

            internal Cleanup2CreatureCommandCamera(RuntimeTestRunner owner, ExpandedSummoningCorrectionFixture fixture,
                UnitEntityData actor, UnitEntityData target, string key, Func<bool> modeCurrent, int candidateIndex,
                Vector3[] pathSurfaceRoots, Stopwatch jointSurvey)
            {
                _owner = owner; _fixture = fixture; _actor = actor; _targetUnit = target; _key = key; _modeCurrent = modeCurrent; _candidateIndex = candidateIndex;
                if (!owner.IsSummonCleanup2Review || (string)owner._request?.Parameters?["reviewPhase"] != "creature-commands" ||
                    !owner._request.ExitAfterCompletion || fixture == null || !fixture.Created.Contains(actor) || !ReferenceEquals(fixture.Hostile, target) ||
                    !ReferenceEquals(target.Blueprint, fixture.HostileBlueprint) ||
                    !((key == SummonAttackGesturePolicy.Crab && actor.Blueprint.AssetGuid == CrabSummonAnimationBindingPolicy.UnitGuid) ||
                        (key == SummonAttackGesturePolicy.Brachiosaurus && actor.Blueprint.AssetGuid == SummonAttackGesturePolicy.BrachiosaurusUnit)) ||
                    !ExpandedSummoningPlayerPathUnitExact(actor, fixture.Caster) || actor.Commands.Empty == false || target.Commands.Empty == false ||
                    modeCurrent == null || !modeCurrent() || candidateIndex < 1 || candidateIndex > 8 || jointSurvey == null ||
                    pathSurfaceRoots == null || pathSurfaceRoots.Length != 3 || pathSurfaceRoots.Any(value => !Cleanup2Finite(value)) ||
                    Vector3.Distance(pathSurfaceRoots[0], actor.Position) > .01f ||
                    new Vector2(pathSurfaceRoots[2].x - target.Position.x, pathSurfaceRoots[2].z - target.Position.z).magnitude > .01f ||
                    Mathf.Abs((pathSurfaceRoots[2].y - actor.FlyHeight) - (target.Position.y - target.FlyHeight)) > .01f)
                    throw new InvalidOperationException("Exact owned idle creature-command camera fixture and native path support required.");
                _rig = TeleportationCastingCamera(); _controller = Game.Instance.CameraController; _camera = _rig?.Camera;
                _guard = _controller?.FixCamera; _occlusion = _camera?.GetComponent<Kingmaker.Visual.OccludedObjectHighlighting.OccludedObjectHighlight>();
                if (_camera == null || !_camera.enabled || !_camera.gameObject.activeInHierarchy || _camera.targetTexture != null ||
                    _guard == null || _guard.GuardCount != 0 || _rig.IsShaking() || _occlusion == null || !_occlusion.enabled || !_occlusion.gameObject.activeInHierarchy)
                    throw new InvalidOperationException("Exact enabled native camera/occlusion, empty target and idle FixCamera guard required.");
                _follower = _controller.Follower; _zoom = _rig.CameraZoom;
                if (_follower == null || _zoom == null) throw new InvalidOperationException("Native camera follower/zoom authority absent.");
                _followerFields = new[] { "m_IsOn", "m_Unit" }.Select(name => _follower.GetType().GetField(name, Cleanup2Fields)).ToArray();
                _zoomFields = new[] { "m_PlayerScrollPosition", "m_GameScrollPosition", "m_ScrollPosition", "m_SmoothScrollPosition" }
                    .Select(name => _zoom.GetType().GetField(name, Cleanup2Fields)).ToArray();
                _controlFields = new[] { "AllowScroll", "AllowZoom", "Clamp" }.Select(name => _controller.GetType().GetField(name, Cleanup2Fields)).ToArray();
                _rigFields = new[] { "m_LocalMapArea", "m_SkipClampOneFrame", "m_PrevSmoothedDt" }
                    .Select(name => typeof(Kingmaker.View.CameraRig).GetField(name, Cleanup2Fields)).ToArray();
                if (_followerFields.Concat(_zoomFields).Concat(_controlFields).Concat(_rigFields).Any(field => field == null))
                    throw new MissingFieldException("Installed native creature camera restoration contract differs.");
                _followerValues = _followerFields.Select(field => field.GetValue(_follower)).ToArray();
                _zoomValues = _zoomFields.Select(field => field.GetValue(_zoom)).ToArray();
                _controlValues = _controlFields.Select(field => field.GetValue(_controller)).ToArray();
                _rigValues = _rigFields.Select(field => field.GetValue(_rig)).ToArray();
                _beforeRigPosition = _rig.transform.position; _beforeRigTarget = _rig.GetPosition(); _beforeRigRotation = _rig.transform.rotation;
                _beforeCameraPosition = _camera.transform.position; _beforeCameraRotation = _camera.transform.rotation; _cameraParent = _camera.transform.parent;
                _beforeSize = _camera.orthographicSize; _beforeAspect = _camera.aspect; _beforeFieldOfView = _camera.fieldOfView; _beforeTarget = _camera.targetTexture;
                _post = _camera.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>(); _profile = _post == null ? null : _post.profile;
                _depthOfField = _profile == null ? null : _profile.depthOfField; _beforeDepthOfField = _depthOfField != null && _depthOfField.enabled;
                var skins = actor.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(value => value.enabled &&
                    value.gameObject.activeInHierarchy && value.sharedMesh != null && value.sharedMesh.name == "KMG_" + key + "_Original").ToArray();
                if (skins.Length != 1 || !ExpandedSummoningPteranodonViewPatch.HasLiveOriginalVisual(actor.View, key))
                    throw new InvalidOperationException("Exact original creature skin required for completed-render bounds.");
                _bodySkin = skins[0]; _bodyMesh = _bodySkin.sharedMesh;
                _rendered = camera => {
                    if (!ReferenceEquals(camera, _camera) || _disposed) return;
                    try
                    {
                        RequireCurrent();
                        // No ApplyCurrentPose/animator/command call here. Bake
                        // only the already-rendered private skin in this same
                        // native post-render callback, before another Update.
                        _renderedBounds = ReadCompletedRenderBounds();
                        _renderedVisible = Cleanup2BoundsCorners(_renderedBounds).All(point => {
                            Vector3 v = _camera.WorldToViewportPoint(point);
                            return v.z > _camera.nearClipPlane && v.x > .01f && v.x < .99f && v.y > .01f && v.y < .99f;
                        });
                        _renderedTarget = _target; RenderCount++; RenderedFrame = Time.frameCount;
                        RenderedGameSeconds = Game.Instance.Player.GameTime.TotalSeconds; RenderedActorPosition = _actor.Position;
                    }
                    catch (Exception error) { _renderError = error.GetType().Name + ":" + error.Message; }
                };
                try
                {
                    // Same installed Tick060091BB counted guard as the lineup
                    // lease: no follower, zoom, clamp or input object is replaced.
                    _guard.Value = true; _guardOwned = true;
                    if (_guard.GuardCount != 1) throw new InvalidOperationException("Native camera FixCamera acquisition changed.");
                    var visual = SummonAttackGestureVisual.For(actor);
                    Bounds nativeBody = visual.CurrentFramingBounds();
                    Vector3[] bodyCorners = Cleanup2BoundsCorners(nativeBody).ToArray();
                    float radius = bodyCorners.Max(point => new Vector2(point.x - actor.Position.x, point.z - actor.Position.z).magnitude);
                    float margin = Mathf.Max(.35f, nativeBody.size.magnitude * .16f);
                    var atStart = new Bounds(new Vector3(actor.Position.x, nativeBody.center.y, actor.Position.z),
                        new Vector3(radius * 2, nativeBody.size.y, radius * 2)); atStart.Expand(margin * 2);
                    var atTarget = atStart; atTarget.center += target.Position - actor.Position;
                    Bounds targetBounds = new Bounds(target.Position, Vector3.one * .1f);
                    foreach (Renderer body in target.View.GetComponentsInChildren<Renderer>(true).Where(value => value.enabled && value.gameObject.activeInHierarchy &&
                        (value is SkinnedMeshRenderer || value is MeshRenderer))) targetBounds.Encapsulate(body.bounds);
                    // Framing padding/yaw spheres contain empty space below
                    // feet and at unused corners. They are not visibility
                    // targets (29 hit terrain/grass there from every angle).
                    // Survey actual baked surface landmarks instead, retaining
                    // the existing12% sole/ground-contact exclusion. Full body,
                    // including feet, remains in the padded projection below.
                    Vector3[] actorSurface = SurfaceLandmarks(new[] { _bodySkin });
                    Vector3[] targetSurface = SurfaceLandmarks(target.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                        .Where(value => value.enabled && value.gameObject.activeInHierarchy && value.sharedMesh != null).ToArray());
                    var metrics = new List<Cleanup2ActorMetric>(); var units = new List<UnitEntityData>();
                    foreach (Vector3 pathRoot in pathSurfaceRoots)
                    foreach (Vector3 surface in actorSurface)
                    { Vector3 point = surface + pathRoot - actor.Position;
                        metrics.Add(new Cleanup2ActorMetric { Min = point, Max = point, Crown = point, Sole = point }); units.Add(actor); }
                    foreach (Vector3 point in targetSurface)
                    { metrics.Add(new Cleanup2ActorMetric { Min = point, Max = point, Crown = point, Sole = point }); units.Add(target); }
                    var sightMetrics = metrics.ToArray();
                    Evidence["actualActorSurfaceLandmarks"] = actorSurface.Length; Evidence["actualTargetSurfaceLandmarks"] = targetSurface.Length;
                    Evidence["sightSampleCount"] = sightMetrics.Length; Evidence["groundContactSightExclusionFraction"] = .12f;
                    Evidence["nativePathSurfaceRoots"] = new JArray(pathSurfaceRoots.Select(SurveyVector));
                    Evidence["sightScope"] = "Actual baked surface extrema/nearest-center samples above native lower12% ground contact, with actor samples translated along the qualified lane. No padded volume corner is a sight target; no terrain/grass/collider name is ignored. Future gesture occlusion is not inferred from this sampled survey.";
                    Bounds envelope = atStart; envelope.Encapsulate(atTarget); envelope.Encapsulate(targetBounds);
                    var corners = Cleanup2BoundsCorners(envelope).ToArray(); Vector3 centre = envelope.center;
                    _rig.ScrollToImmediately(centre); _camera.aspect = 1280f / 800f;
                    if (!_camera.orthographic) _camera.fieldOfView = 60f;
                    Vector3 lane = target.Position - actor.Position; lane.y = 0; lane.Normalize();
                    if (lane.sqrMagnitude < .5f) throw new InvalidOperationException("Finite native approach camera axis required.");
                    Vector3 across = Vector3.Cross(lane, Vector3.up).normalized;
                    var colliderRefs = Cleanup2NativeSelectionColliderReferences(); var meshCache = new Dictionary<Mesh, Tuple<Vector3[], int[]>>();
                    var survey = Stopwatch.StartNew(); var attempts = new JArray(); bool found = false;
                    bool mansionStage = Game.Instance.CurrentlyLoadedArea?.AssetGuid == "0a89ba823d7eab045949d98cf22dcd2c";
                    Evidence["stageCameraCandidateSource"] = mansionStage
                        ? "Review28 native mansion giant stage:20-degree downward view, side-1, no lane-yaw bias; fit current whole swept bodies rather than copy its camera position."
                        : "Existing bounded native Oleg camera candidates.";
                    foreach (float pitch in mansionStage ? new[] { 20f, 25f, 35f, 45f, 50f, 60f } : new[] { 35f, 25f, 45f, 50f, 60f })
                    {
                        foreach (float side in mansionStage ? new[] { -1f, 1f } : new[] { 1f, -1f })
                        {
                            if (jointSurvey.Elapsed.TotalSeconds > 5) break;
                            Vector3 horizontal = (across * side + lane * (mansionStage ? 0f : .25f)).normalized;
                            Vector3 direction = horizontal * Mathf.Cos(pitch * Mathf.Deg2Rad) - Vector3.up * Mathf.Sin(pitch * Mathf.Deg2Rad);
                            _rotation = Quaternion.LookRotation(direction, Vector3.up); _rig.SetRotation(_rotation.eulerAngles.y);
                            Vector3[] projected = corners.Select(point => Quaternion.Inverse(_rotation) * (point - centre)).ToArray();
                            float halfFrame = Mathf.Max(projected.Max(point => Mathf.Abs(point.y)), projected.Max(point => Mathf.Abs(point.x)) / _camera.aspect) * 1.12f;
                            float distance = projected.Max(point => Mathf.Abs(point.z)) + Mathf.Max(4f, halfFrame / Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad * .5f));
                            if (_camera.orthographic) _camera.orthographicSize = halfFrame;
                            _position = centre - direction * distance; _camera.transform.SetPositionAndRotation(_position, _rotation);
                            JArray ignored; var blockers = Cleanup2LineupSightBlockers(_camera, units, sightMetrics, colliderRefs, out ignored);
                            // A demonstrated physics obstruction already
                            // rejects this candidate. Do not spend the joint
                            // budget testing millions of mesh triangles on a
                            // camera that cannot be admitted in any event.
                            var rendered = blockers.Count == 0
                                ? Cleanup2LineupRenderedObstructions(_camera, units, sightMetrics, meshCache, survey)
                                : new JObject { ["qualified"] = false, ["actualTriangleHits"] = new JArray(),
                                    ["unknownCandidates"] = new JArray(new JObject { ["reason"] = "not-surveyed-demonstrated-physics-obstruction" }),
                                    ["triangleTests"] = 0, ["limits"] = "Physical blockers already reject this exact candidate; no rendered-clear claim." };
                            attempts.Add(new JObject { ["pitch"] = pitch, ["side"] = side, ["position"] = SurveyVector(_position),
                                ["physicalBlockers"] = blockers, ["ignoredExactSelectionColliders"] = ignored, ["renderedSurvey"] = rendered });
                            if (blockers.Count == 0 && ((JArray)rendered["actualTriangleHits"]).Count == 0)
                            { found = true; Evidence["environmentSightProven"] = (bool)rendered["qualified"]; break; }
                        }
                        if (found || survey.Elapsed.TotalSeconds > 15 || jointSurvey.Elapsed.TotalSeconds > 5) break;
                    }
                    Evidence["cameraAttempts"] = attempts;
                    Evidence["jointCandidateAdmissionBudgetExceeded"] = jointSurvey.Elapsed.TotalSeconds > 5;
                    if (!found) throw new Cleanup2CreatureCameraObstructed(jointSurvey.Elapsed.TotalSeconds > 5 || survey.Elapsed.TotalSeconds > 15);
                    _size = _camera.orthographicSize;
                    _target = new RenderTexture(1280, 800, 24, RenderTextureFormat.ARGB32); _camera.targetTexture = _target;
                    _renderBoundsMesh = new Mesh();
                    if (_depthOfField != null) _depthOfField.enabled = false;
                    Camera.onPostRender += _rendered; _subscribed = true;
                    Evidence["unit"] = actor.UniqueId; Evidence["target"] = target.UniqueId; Evidence["cameraInstance"] = _camera.GetInstanceID();
                    Evidence["fixedCameraPosition"] = SurveyVector(_position); Evidence["fieldOfView"] = _camera.fieldOfView;
                    Evidence["fullSweptEnvelopeMin"] = SurveyVector(envelope.min); Evidence["fullSweptEnvelopeMax"] = SurveyVector(envelope.max);
                    Evidence["scope"] = "Owned body maximum horizontal radius through start-to-target travel plus16% native-body diagonal gesture margin. Same fixed normal native camera through ready/approach/act/recovery; no actor pose, scene, collider, highlighter or material override.";
                    Evidence["visibilityLimit"] = "Sampled physics/readable-triangle survey only. Unreadable/overbudget meshes remain UNKNOWN, not clear. Actual native PNG inspection required.";
                    RequireCurrent();
                }
                catch (Exception error)
                {
                    _candidateRefused = error is Cleanup2CreatureCameraObstructed;
                    Evidence["initializationError"] = error.ToString();
                    try { Dispose(); } catch (Exception cleanup) { throw new AggregateException(error, cleanup); }
                    throw;
                }
            }

            private static Vector3[] SurfaceLandmarks(SkinnedMeshRenderer[] skins)
            {
                if (skins.Length == 0 || skins.Sum(value => value.sharedMesh.vertexCount) > 250000)
                    throw new InvalidOperationException("Bounded actual native surface mesh survey unavailable.");
                var vertices = new List<Vector3>(); var baked = new Mesh();
                try
                {
                    foreach (var skin in skins)
                    {
                        baked.Clear(); skin.BakeMesh(baked);
                        if (baked.vertexCount == 0 || baked.vertexCount != skin.sharedMesh.vertexCount)
                            throw new InvalidOperationException("Actual camera-surface vertex count differs.");
                        foreach (Vector3 vertex in baked.vertices)
                        {
                            Vector3 world = skin.transform.TransformPoint(vertex);
                            if (!Cleanup2Finite(world)) throw new InvalidOperationException("Nonfinite native camera-surface point.");
                            vertices.Add(world);
                        }
                    }
                }
                finally { UnityEngine.Object.DestroyImmediate(baked); }
                Vector3 min = vertices.Aggregate(Vector3.Min), max = vertices.Aggregate(Vector3.Max);
                float lowerContact = Mathf.Lerp(min.y, max.y, .12f);
                Vector3[] surface = vertices.Where(value => value.y >= lowerContact).ToArray();
                if (surface.Length == 0) throw new InvalidOperationException("No native body surface above ground-contact boundary.");
                var chosen = new HashSet<int>(); Vector3 centre = (min + max) * .5f;
                foreach (int x in new[] { -1, 0, 1 }) foreach (int y in new[] { -1, 0, 1 }) foreach (int z in new[] { -1, 0, 1 })
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    Vector3 direction = new Vector3(x, y, z); int index = 0; float support = float.NegativeInfinity;
                    for (int i = 0; i < surface.Length; i++)
                    { float distance = Vector3.Dot(surface[i] - centre, direction); if (distance > support) { support = distance; index = i; } }
                    chosen.Add(index);
                }
                foreach (float x in new[] { .2f, .5f, .8f }) foreach (float z in new[] { .2f, .5f, .8f })
                {
                    Vector3 desired = new Vector3(Mathf.Lerp(min.x, max.x, x), centre.y, Mathf.Lerp(min.z, max.z, z));
                    int index = 0; float nearest = float.PositiveInfinity;
                    for (int i = 0; i < surface.Length; i++)
                    { float distance = (surface[i] - desired).sqrMagnitude; if (distance < nearest) { nearest = distance; index = i; } }
                    chosen.Add(index);
                }
                return chosen.OrderBy(value => value).Select(value => surface[value]).ToArray();
            }

            internal void RequireCurrent()
            {
                if (_disposed || !_guardOwned || !_modeCurrent() || !ReferenceEquals(Game.Instance.CameraController, _controller) ||
                    !ReferenceEquals(TeleportationCastingCamera(), _rig) || !ReferenceEquals(_rig.Camera, _camera) || !ReferenceEquals(_controller.FixCamera, _guard) || _guard.GuardCount != 1 ||
                    !ReferenceEquals(_camera.targetTexture, _target) || _camera.transform.parent != _cameraParent || !_camera.enabled || !_camera.gameObject.activeInHierarchy ||
                    Vector3.Distance(_camera.transform.position, _position) > .02f || Quaternion.Angle(_camera.transform.rotation, _rotation) > .01f ||
                    _camera.aspect != 1280f / 800f || _camera.orthographicSize != _size || (!_camera.orthographic && _camera.fieldOfView != 60f) ||
                    !_occlusion.enabled || !_occlusion.gameObject.activeInHierarchy ||
                    (_post != null && (!ReferenceEquals(_post.profile, _profile) || !ReferenceEquals(_profile == null ? null : _profile.depthOfField, _depthOfField))) ||
                    (_depthOfField != null && _depthOfField.enabled) || !ReferenceEquals(_controller.Follower, _follower) || !ReferenceEquals(_rig.CameraZoom, _zoom) ||
                    !_followerFields.Select((field, i) => Equals(field.GetValue(_follower), _followerValues[i])).All(value => value) ||
                    !_zoomFields.Select((field, i) => Equals(field.GetValue(_zoom), _zoomValues[i])).All(value => value) ||
                    !_controlFields.Select((field, i) => Equals(field.GetValue(_controller), _controlValues[i])).All(value => value) ||
                    _actor.Destroyed || _targetUnit.Destroyed || !_fixture.Created.Contains(_actor) || !ReferenceEquals(_fixture.Hostile, _targetUnit) ||
                    _bodySkin == null || !ReferenceEquals(_bodySkin.sharedMesh, _bodyMesh) || !_bodySkin.enabled || !_bodySkin.gameObject.activeInHierarchy ||
                    !ExpandedSummoningPteranodonViewPatch.HasLiveOriginalVisual(_actor.View, _key) ||
                    !ExpandedSummoningPlayerPathUnitExact(_actor, _fixture.Caster) || _renderError != null)
                    throw new InvalidOperationException("Owned normal-frame creature camera context changed: " + _renderError);
            }

            private Bounds ReadCompletedRenderBounds()
            {
                _renderBoundsMesh.Clear(); _bodySkin.BakeMesh(_renderBoundsMesh);
                Vector3[] vertices = _renderBoundsMesh.vertices;
                if (vertices.Length == 0 || vertices.Length != _bodyMesh.vertexCount)
                    throw new InvalidOperationException("Completed-render original skin vertex count differs.");
                var bounds = new Bounds(_bodySkin.transform.TransformPoint(vertices[0]), Vector3.zero);
                foreach (Vector3 vertex in vertices)
                {
                    Vector3 world = _bodySkin.transform.TransformPoint(vertex);
                    if (!Cleanup2Finite(world)) throw new InvalidOperationException("Nonfinite completed-render body framing point.");
                    bounds.Encapsulate(world);
                }
                foreach (Renderer body in _targetUnit.View.GetComponentsInChildren<Renderer>(true).Where(value => value.enabled &&
                    value.gameObject.activeInHierarchy && (value is SkinnedMeshRenderer || value is MeshRenderer))) bounds.Encapsulate(body.bounds);
                return bounds; // Framing only; no geometry is exported.
            }

            internal string Capture(string file)
            {
                RequireCurrent();
                if (RenderCount < 3 || RenderedFrame < 0 || Time.frameCount - RenderedFrame < 0 || Time.frameCount - RenderedFrame > 1 ||
                    !ReferenceEquals(_renderedTarget, _target))
                    throw new InvalidOperationException("A fresh completed render of the exact owned native target is required.");
                if (!_renderedVisible || ScreenFadeAlpha() > .02f) throw new InvalidOperationException("Completed-render full native body clips the fixed view or screen is faded.");
                Evidence["readbackRenderedFrame"] = RenderedFrame;
                Evidence["readbackRenderedGameSeconds"] = RenderedGameSeconds;
                Evidence["readbackBoundsMin"] = SurveyVector(_renderedBounds.min); Evidence["readbackBoundsMax"] = SurveyVector(_renderedBounds.max);
                Evidence["readbackTargetInstance"] = _renderedTarget.GetInstanceID();
                return _owner.WriteRefinementImage(_position, _rotation, file, _target);
            }

            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                if (_subscribed) { Camera.onPostRender -= _rendered; _subscribed = false; }
                var errors = new List<string>();
                try
                {
                    if (_guardOwned)
                    {
                        if (!ReferenceEquals(Game.Instance.CameraController, _controller) || !ReferenceEquals(TeleportationCastingCamera(), _rig) ||
                            !ReferenceEquals(_rig.Camera, _camera) || !ReferenceEquals(_controller.FixCamera, _guard) || _guard.GuardCount != 1 ||
                            _camera.transform.parent != _cameraParent) throw new InvalidOperationException("Foreign native camera/guard authority; restoration refused.");
                        if (ReferenceEquals(_camera.targetTexture, _target)) _camera.targetTexture = _beforeTarget;
                        else if (!ReferenceEquals(_camera.targetTexture, _beforeTarget)) throw new InvalidOperationException("Foreign native render target; not overwritten.");
                        if (_target != null && (Vector3.Distance(_camera.transform.position, _position) > .02f || Quaternion.Angle(_camera.transform.rotation, _rotation) > .01f ||
                            _camera.aspect != 1280f / 800f || _camera.orthographicSize != _size || (!_camera.orthographic && _camera.fieldOfView != 60f)))
                            throw new InvalidOperationException("Foreign native camera pose/lens; not overwritten.");
                        if (_post != null && (!ReferenceEquals(_post.profile, _profile) || !ReferenceEquals(_profile == null ? null : _profile.depthOfField, _depthOfField)))
                            throw new InvalidOperationException("Foreign native camera post profile; not overwritten.");
                        if (_target != null && _depthOfField != null && _depthOfField.enabled)
                            throw new InvalidOperationException("Foreign native depth-of-field value; not overwritten.");
                        _rig.ScrollToImmediately(_beforeRigPosition); _rig.ScrollTo(_beforeRigTarget);
                        _rig.transform.position = _beforeRigPosition; _rig.transform.rotation = _beforeRigRotation;
                        for (int i = 0; i < _rigFields.Length; i++) _rigFields[i].SetValue(_rig, _rigValues[i]);
                        _camera.transform.SetPositionAndRotation(_beforeCameraPosition, _beforeCameraRotation);
                        _camera.orthographicSize = _beforeSize; _camera.aspect = _beforeAspect; _camera.fieldOfView = _beforeFieldOfView;
                        if (_depthOfField != null) _depthOfField.enabled = _beforeDepthOfField;
                        if (_rig.transform.position != _beforeRigPosition || _rig.GetPosition() != _beforeRigTarget ||
                            Quaternion.Angle(_rig.transform.rotation, _beforeRigRotation) > .001f ||
                            Vector3.Distance(_camera.transform.position, _beforeCameraPosition) > .001f || Quaternion.Angle(_camera.transform.rotation, _beforeCameraRotation) > .001f ||
                            _camera.orthographicSize != _beforeSize || _camera.aspect != _beforeAspect || _camera.fieldOfView != _beforeFieldOfView ||
                            !ReferenceEquals(_camera.targetTexture, _beforeTarget) || (_depthOfField != null && _depthOfField.enabled != _beforeDepthOfField) ||
                            !ReferenceEquals(_controller.Follower, _follower) || !ReferenceEquals(_rig.CameraZoom, _zoom) ||
                            !_rigFields.Select((field, i) => Equals(field.GetValue(_rig), _rigValues[i])).All(value => value) ||
                            !_followerFields.Select((field, i) => Equals(field.GetValue(_follower), _followerValues[i])).All(value => value) ||
                            !_zoomFields.Select((field, i) => Equals(field.GetValue(_zoom), _zoomValues[i])).All(value => value) ||
                            !_controlFields.Select((field, i) => Equals(field.GetValue(_controller), _controlValues[i])).All(value => value))
                            throw new InvalidOperationException("Native creature camera state restoration is not exact.");
                    }
                }
                catch (Exception error) { errors.Add(error.GetType().Name + ":" + error.Message); }
                finally
                {
                    try
                    {
                        if (_guardOwned)
                        {
                            if (!ReferenceEquals(_controller.FixCamera, _guard) || _guard.GuardCount < 1) errors.Add("foreign-camera-guard-release-refused");
                            else { int before = _guard.GuardCount; _guard.Value = false; if (_guard.GuardCount != before - 1) errors.Add("camera-guard-release-not-exact"); }
                            _guardOwned = false;
                        }
                    }
                    catch (Exception error) { errors.Add("guard-release:" + error.GetType().Name + ":" + error.Message); }
                    try
                    {
                        if (_target != null && _camera != null && !ReferenceEquals(_camera.targetTexture, _target))
                        { _target.Release(); UnityEngine.Object.Destroy(_target); }
                    }
                    catch (Exception error) { errors.Add("target-release:" + error.GetType().Name + ":" + error.Message); }
                    try
                    { if (_renderBoundsMesh != null) { UnityEngine.Object.DestroyImmediate(_renderBoundsMesh); _renderBoundsMesh = null; } }
                    catch (Exception error) { errors.Add("bounds-mesh-release:" + error.GetType().Name + ":" + error.Message); }
                }
                Evidence["restored"] = errors.Count == 0; Evidence["restorationErrors"] = new JArray(errors);
                Evidence["ordinaryNativeRenderCount"] = RenderCount; Evidence["lastRenderedFrame"] = RenderedFrame;
                Evidence["observerManualCameraRenderCalled"] = false; Evidence["actorsSceneMaterialsOrOcclusionFlagsChanged"] = false;
                _owner.Cleanup2Check("creature-command-fixed-native-camera-restored-" + _key +
                    (_candidateRefused ? "-rejected-camera-" + _candidateIndex : ""), errors.Count == 0, Evidence,
                    "The exact native rig/pose/lens/DOF/target and counted FixCamera lease restore without changing unrelated actor/UI/scene state.");
                if (errors.Count != 0) throw new InvalidOperationException("Native creature camera cleanup failed: " + string.Join("|", errors.ToArray()));
            }
        }
    }
}
