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
            internal float[] Starts, Deadlines, Acts;
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
                        ["clockLimitation"] = "Existing qualified Dialog-host Sprint22NativeClock advances native GameTime/actions/agent by .05s per pulse; native command/rule/cost evidence under that fixture, not natural Default-mode or turn-based timing qualification.",
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
                    string stats = Cleanup2CrabBaseStats(actor).ToString(), mechanics = Cleanup2ScaleMechanics(actor).ToString();
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
                                    ReferenceEquals(info?.Weapon, attack.Weapon) && matches[0].ActTime >= matches[0].Deadline &&
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
                                                float elapsed = record.ActTime < 0 ? command.TimeSinceStart - record.Start :
                                                    1f + (float)(Game.Instance.Player.GameTime.TotalSeconds - record.ActGame);
                                                string moment = record.ActTime < 0 && elapsed >= .42f && elapsed < 1f ? "windup" :
                                                    record.ActTime >= 0 && elapsed < 1.18f ? "act" : record.ActTime >= 0 && elapsed >= 1.22f && elapsed < 1.48f ? "recover" : null;
                                                string imageKey = record.Index + "-" + moment;
                                                if (moment == null || images.Contains(imageKey)) continue;
                                                string file = "production-" + label + "-attack" + record.Index + "-" + moment + ".png";
                                                string saved = Cleanup2ProductionAttackImage(actor, file, crab);
                                                bool present = saved == file && File.Exists(Path.Combine(_request.EvidenceDirectory, file));
                                                Cleanup2Check("production-pose-" + label + "-" + imageKey, present, new JObject { ["file"] = file,
                                                    ["actualNativeScheduleSerial"] = record.Serial, ["plannedIndex"] = record.Index, ["nativeCommandTime"] = command.TimeSinceStart,
                                                    ["nativeScheduleStart"] = record.Start, ["nativeActTime"] = record.ActTime, ["renderCurveElapsed"] = elapsed,
                                                    ["geometrySample"] = visual.MeasureGestureDisplacement(), ["gesture"] = visual.Describe(), ["forcedActOrHandle"] = false },
                                                    "Actual native fallback clock/act drives this original private skin pose and normally visible native camera image.");
                                                if (!present) failures.Add("missing-native-pose-image"); images.Add(imageKey);
                                            }
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
                                            final[index].ActTime >= final[index].Deadline && final[index].ActTime - final[index].Deadline <= .055f);
                                    bool motion = posesFinite && zeroEqual && sourceInvariant && maximumProtectedMotion <= .0002f &&
                                        (enabled ? maximumSkinMotion > .01f && final.All(value => value.Samples >= 4 && value.MaximumAngle > 1f) && images.Count == expected * 3 : maximumSkinMotion <= .0002f);
                                    bool nativeProof = command.IsFinished && (command.Result == UnitCommand.ResultType.Success || trace.Exhausted(command)) &&
                                        observer.WeaponAttacks.Count == expected && observer.OwnerWeaponAttacks.Count == expected && observer.Rolls.Count == expected &&
                                        observer.WeaponAttacks.Select(value => value.Weapon).SequenceEqual(limbs.Take(expected)) &&
                                        observer.WeaponAttacks.All(value => value.AttackRoll != null && !value.AttackRoll.AutoMiss && !value.AttackRoll.SuspendCombatLog && !value.IsCharge && !value.IsAttackOfOpportunity) &&
                                        economy.Count(command) == 1 && economy.Exact(command, false) && economy.ForcedCount(command) == 0 &&
                                        !command.Cutscene && !command.IsIgnoreCooldown && command.OverrideAction == null && command.Animation == null &&
                                        !target.Descriptor.State.IsDead && Cleanup2CrabBaseStats(actor).ToString() == stats && Cleanup2ScaleMechanics(actor).ToString() == mechanics &&
                                        Sprint19Limbs(actor).SequenceEqual(limbs) && Cleanup2SharedSameView(source) && Cleanup2SharedSameView(live);
                                    var result = new Cleanup2ProductionAttackResult { Starts = final.Select(value => value.Start).ToArray(), Deadlines = final.Select(value => value.Deadline).ToArray(),
                                        Acts = final.Select(value => value.ActTime).ToArray(), Type = command.Type.ToString(), Events = observer.WeaponAttacks.Count, Costs = economy.Count(command) };
                                    bool matched = !enabled || baseline != null && baseline.Type == result.Type && baseline.Events == result.Events && baseline.Costs == result.Costs &&
                                        baseline.Acts.Length == result.Acts.Length && Enumerable.Range(0, result.Acts.Length).All(index =>
                                            Mathf.Abs((baseline.Acts[index] - baseline.Starts[index]) - (result.Acts[index] - result.Starts[index])) <= .051f &&
                                            Mathf.Abs((baseline.Deadlines[index] - baseline.Starts[index]) - (result.Deadlines[index] - result.Starts[index])) <= .0001f);
                                    bool passed = association && motion && nativeProof && matched;
                                    Cleanup2Check("production-attack-resolution-" + label, passed, new JObject { ["owner"] = actor.UniqueId, ["target"] = target.UniqueId,
                                        ["renderGestureEnabled"] = enabled, ["nativeResult"] = command.Result.ToString(), ["nativeListExhausted"] = trace.Exhausted(command),
                                        ["plannedAttacks"] = command.AllAttacks.Count, ["expectedAttacks"] = expected, ["events"] = events, ["costs"] = costs, ["endings"] = endings,
                                        ["scheduleAssociation"] = association, ["nativeRulesAndCostProof"] = nativeProof,
                                        ["matchedBaselineOnScheduleAndCost"] = matched, ["failures"] = new JArray(failures.Distinct()),
                                        ["positiveActualSkinMotionOrExactSuppression"] = motion, ["maximumSkinDisplacement"] = maximumSkinMotion,
                                        ["maximumProtectedSkinDisplacement"] = maximumProtectedMotion, ["sameFrameSourceAndColliderInvariant"] = sourceInvariant,
                                        ["actualCurrentPoseZeroOffsetEquality"] = zeroEqual, ["poses"] = poses, ["gesture"] = visual.Describe(),
                                        ["nativeCommandTime"] = command.TimeSinceStart, ["wallSeconds"] = commandWatch.Elapsed.TotalSeconds,
                                        ["frames"] = frames, ["clockPulses"] = _sprint25Clock.Pulses - startPulse, ["gameTimeDelta"] = Game.Instance.Player.GameTime.TotalSeconds - startGame,
                                        ["forcedRolls"] = 0, ["forcedActs"] = 0, ["fixtureAttackRulesTriggered"] = 0, ["nativeAnimationHandlesFabricated"] = 0,
                                        ["limits"] = "Matched native RTWP commands under the qualified fixture clock only. Timing comparison tolerance is one .05s clock quantum, with both exact stored delays still 1s. No natural Default-mode/turn-based timing, exact enemy-contact/reach, Hit animation or owner visual approval claim." },
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

        private string Cleanup2ProductionAttackImage(UnitEntityData actor, string file, bool crab)
        {
            if (File.Exists(Path.Combine(_request.EvidenceDirectory, file))) throw new InvalidOperationException("Production attack image already exists.");
            Bounds previous = _refinementBounds;
            try
            {
                var visual = SummonAttackGestureVisual.For(actor);
                _refinementBounds = visual.CurrentFramingBounds(); Vector3 center = _refinementBounds.center;
                float distance = Mathf.Max(3.5f, _refinementBounds.extents.magnitude * 2.4f);
                Vector3 direction = (actor.View.transform.right * (crab ? .8f : 1.4f) - actor.View.transform.forward * (crab ? 1.2f : -.6f) + Vector3.up * .7f).normalized;
                Vector3 eye = center + direction * distance;
                return WriteRefinementImage(eye, Quaternion.LookRotation(center - eye, Vector3.up), file);
            }
            finally { _refinementBounds = previous; }
        }
    }
}
