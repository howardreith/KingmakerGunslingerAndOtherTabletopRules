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
        private sealed class Cleanup2CrabHandle
        {
            internal UnitAnimationActionHandle Handle;
            internal AttackHandInfo Attack;
            internal ItemEntityWeapon Weapon;
            internal int Index, HandleId, Samples;
            internal bool[] Captured = new bool[3];
            internal Vector3[] FirstPosition;
            internal Quaternion[] FirstRotation;
            internal float LeftMotion, RightMotion, LeftAngle, RightAngle;
            internal readonly JArray Poses = new JArray();
        }

        // Init and OnStart both build native AttackHandInfo/handle lists. Only
        // a live command-owned chain is motion evidence; preparatory handles
        // remain bounded diagnostics and never inflate the accepted count.
        private sealed class Cleanup2CrabAttackHandles
        {
            private sealed class Seen
            {
                internal UnitAnimationActionHandle Handle;
                internal AttackHandInfo Attack;
                internal ItemEntityWeapon Weapon;
                internal int Id, Index;
                internal bool Accepted, CommandStarted;
                internal string State;
            }
            private sealed class WeaponEvent
            {
                internal AttackHandInfo Attack;
                internal UnitAnimationActionHandle Handle;
                internal ItemEntityWeapon Weapon;
                internal int Index;
            }
            private readonly UnitAnimationAction bound;
            private readonly ItemEntityWeapon[] expected;
            private readonly Dictionary<UnitAnimationActionHandle, Seen> seen = new Dictionary<UnitAnimationActionHandle, Seen>();
            private readonly Dictionary<int, Seen> latest = new Dictionary<int, Seen>();
            private readonly List<WeaponEvent> events = new List<WeaponEvent>();
            internal readonly List<Cleanup2CrabHandle> Series = new List<Cleanup2CrabHandle>();
            internal readonly JArray Transitions = new JArray(), Replacements = new JArray();
            internal readonly List<string> Failures = new List<string>();
            internal bool DiagnosticsTruncated;
            internal Cleanup2CrabAttackHandles(UnitAnimationAction action, ItemEntityWeapon[] weapons)
            { bound = action; expected = weapons; }
            private void Fail(string text)
            { if (Failures.Count < 32 && !Failures.Contains(text)) Failures.Add(text); }
            private void Row(JArray rows, JObject value)
            {
                if (Transitions.Count + Replacements.Count >= 128) { DiagnosticsTruncated = true; Fail("handle-diagnostic-bound-exceeded"); return; }
                rows.Add(value);
            }
            private static int Id(object value) { return value == null ? 0 : RuntimeHelpers.GetHashCode(value); }
            internal JObject RecordWeapon(UnitAttack command, ItemEntityWeapon weapon)
            {
                int index = command.GetAttackIndex();
                AttackHandInfo info = command.PlannedAttack;
                UnitAnimationActionHandle handle = command.Animation;
                bool exact = index >= 0 && index < expected.Length && info != null &&
                    ReferenceEquals(info.Weapon, weapon) && ReferenceEquals(expected[index], weapon) &&
                    handle != null && ReferenceEquals(info.AnimationHandle, handle) && ReferenceEquals(handle.Action, bound);
                if (!exact) Fail("native-weapon-event-has-no-exact-planned-handle-chain");
                if (events.Count < 8) events.Add(new WeaponEvent { Index = index, Attack = info, Handle = handle, Weapon = weapon });
                else Fail("native-weapon-event-bound-exceeded");
                return new JObject { ["plannedIndexAtRule"] = index, ["attackInfoIdentity"] = Id(info),
                    ["weaponEntityIdentity"] = Id(weapon), ["nativeHandleIdentity"] = Id(handle),
                    ["samePlannedAndCommandHandle"] = info != null && ReferenceEquals(info.AnimationHandle, handle),
                    ["exactAssociation"] = exact };
            }
            internal IEnumerable<Cleanup2CrabHandle> Observe(UnitAttack command)
            {
                var active = new List<Cleanup2CrabHandle>();
                bool currentListed = false;
                if (command.AllAttacks.Count > 8) { Fail("native-attack-list-bound-exceeded"); return active; }
                if (command.IsStarted && command.AllAttacks.Count != expected.Length) Fail("started-native-routine-count-changed");
                for (int index = 0; index < command.AllAttacks.Count; index++)
                {
                    AttackHandInfo info = command.AllAttacks[index];
                    UnitAnimationActionHandle handle = info?.AnimationHandle;
                    if (handle == null) continue;
                    bool current = ReferenceEquals(command.Animation, handle);
                    currentListed |= current;
                    Seen receipt;
                    if (!seen.TryGetValue(handle, out receipt))
                    {
                        if (seen.Count >= 32) { Fail("native-handle-census-bound-exceeded"); continue; }
                        receipt = new Seen { Handle = handle, Attack = info, Weapon = info.Weapon, Index = index, Id = seen.Count };
                        Seen prior;
                        if (latest.TryGetValue(index, out prior) && !ReferenceEquals(prior.Handle, handle))
                            Row(Replacements, new JObject { ["frame"] = Time.frameCount, ["index"] = index,
                                ["previousHandleId"] = prior.Id, ["replacementHandleId"] = receipt.Id,
                                ["previousInfoIdentity"] = Id(prior.Attack), ["replacementInfoIdentity"] = Id(info),
                                ["previousAccepted"] = prior.Accepted, ["previousCommandStarted"] = prior.CommandStarted,
                                ["commandStartedNow"] = command.IsStarted,
                                ["meaning"] = "Observed identities only. Native Init and OnStart rebuild the list; an accepted chain may never be silently replaced." });
                        seen.Add(handle, receipt); latest[index] = receipt;
                    }
                    if (receipt.Index != index || !ReferenceEquals(receipt.Attack, info) || !ReferenceEquals(receipt.Weapon, info.Weapon))
                        Fail("one-handle-reused-across-different-attack-info-or-weapon");
                    string state = command.IsStarted + ":" + handle.IsStarted + ":" + handle.IsFinished + ":" + (handle.ActiveAnimation != null) + ":" + current;
                    if (state != receipt.State)
                    {
                        receipt.State = state;
                        Row(Transitions, new JObject { ["frame"] = Time.frameCount, ["commandTimeSinceStart"] = command.TimeSinceStart,
                            ["handleId"] = receipt.Id, ["nativeHandleIdentity"] = Id(handle), ["attackInfoIdentity"] = Id(info),
                            ["index"] = index, ["nativePlannedIndex"] = command.GetAttackIndex(),
                            ["weapon"] = info.Weapon?.Blueprint.AssetGuid, ["weaponEntityIdentity"] = Id(info.Weapon),
                            ["commandStarted"] = command.IsStarted, ["handleStarted"] = handle.IsStarted,
                            ["handleFinished"] = handle.IsFinished, ["activeAnimation"] = handle.ActiveAnimation != null,
                            ["sameCommandHandle"] = current, ["acceptedPreviously"] = receipt.Accepted });
                    }
                    receipt.CommandStarted |= command.IsStarted;
                    bool live = handle.IsStarted && !handle.IsFinished && handle.ActiveAnimation != null;
                    if (!live) continue;
                    if (!current)
                    {
                        // A known accepted prior attack may finish its native
                        // blend after its exact weapon rule. Unknown live
                        // handles are never discarded as harmless transients.
                        if (!receipt.Accepted || index >= command.GetAttackIndex() ||
                            events.Count(value => value.Index == index && ReferenceEquals(value.Attack, info) && ReferenceEquals(value.Handle, handle)) != 1)
                            Fail("unassociated-live-handle-outside-current-command-animation");
                        continue;
                    }
                    bool beforeAct = index == command.GetAttackIndex() && ReferenceEquals(command.PlannedAttack, info);
                    bool afterAct = index + 1 == command.GetAttackIndex() && ReferenceEquals(command.LastExecutedAttack, info) &&
                        events.Count(value => value.Index == index && ReferenceEquals(value.Attack, info) && ReferenceEquals(value.Handle, handle)) == 1;
                    if (!command.IsStarted || command.IsFinished || index >= expected.Length ||
                        !ReferenceEquals(expected[index], info.Weapon) || !ReferenceEquals(handle.Action, bound) || !(beforeAct || afterAct))
                    { Fail("current-live-handle-has-mismatched-native-attack-chain"); continue; }
                    Cleanup2CrabHandle series = Series.SingleOrDefault(value => value.Index == index);
                    if (series == null)
                    {
                        series = new Cleanup2CrabHandle { Index = index, Attack = info, Weapon = info.Weapon, Handle = handle, HandleId = receipt.Id };
                        Series.Add(series); receipt.Accepted = true;
                    }
                    if (!ReferenceEquals(series.Handle, handle) || !ReferenceEquals(series.Attack, info) || !ReferenceEquals(series.Weapon, info.Weapon))
                    { Fail("duplicate-or-replaced-accepted-active-chain-for-one-attack"); continue; }
                    active.Add(series);
                }
                if (command.Animation != null && command.Animation.IsStarted && !command.Animation.IsFinished &&
                    command.Animation.ActiveAnimation != null && !currentListed) Fail("live-command-animation-absent-from-native-attack-list");
                if (active.Count > 1) Fail("multiple-current-live-attack-chains");
                return active;
            }
            internal bool Exact(UnitAttack command)
            {
                return !DiagnosticsTruncated && Failures.Count == 0 && Series.Count == expected.Length && events.Count == expected.Length &&
                    command.AllAttacks.Count == expected.Length && Enumerable.Range(0, expected.Length).All(index => {
                        var series = Series.SingleOrDefault(value => value.Index == index);
                        var linked = events.Where(value => value.Index == index).ToArray();
                        return series != null && linked.Length == 1 && ReferenceEquals(series.Attack, command.AllAttacks[index]) &&
                            ReferenceEquals(series.Weapon, expected[index]) && ReferenceEquals(linked[0].Attack, series.Attack) &&
                            ReferenceEquals(linked[0].Handle, series.Handle) && ReferenceEquals(linked[0].Weapon, series.Weapon);
                    });
            }
        }

        private IEnumerable<int> ReviewSummonCleanup2CrabAttack(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion ||
                (string)_request.Parameters?["reviewPhase"] != "crab-attack" || fixture == null || book == null ||
                _sprint25Clock == null || _expandedSummoningPlayerPathCaptureActive || _refinementUnit != null ||
                TurnBased.Controllers.CombatController.IsInTurnBasedCombat())
                throw new InvalidOperationException("Exact idle automatic-exit Cleanup 2 RTWP Crab attack fixture required.");
            bool pause = Game.Instance.IsPaused, waiting = _sprint25WaitingForUnityAppearance;
            var oldCapture = ExpandedSummoningRuleCapture.ToArray();
            var oldEvents = ExpandedSummoningPlayerPathEvents.ToArray();
            int oldCasts = _expandedSummoningPlayerPathRuleCastCount, oldSpawns = _expandedSummoningPlayerPathSpawnActionCount;
            var hooks = HarmonyInstance.Create("kmg.cleanup2.crab-attack." + _request.RunId);
            UnitEntityData actor = null;
            IDisposable bindingReview = null;
            UnitAnimationManager reviewManager = null;
            bool bindingReviewAttempted = false;
            bool bindingRestorationObserved = false;
            var watch = Stopwatch.StartNew();
            const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
            try
            {
                // Existing exact disposable hostile setup; no Crab is spawned
                // through that helper, and no target stats/rolls are changed by
                // this observer. Its declared 100000HP fixture is retained.
                if (fixture.Hostile == null) CreateExpandedSummoningCorrectionHostile(fixture);
                UnitEntityData target = fixture.Hostile;
                if (target == null || target.Destroyed || !ReferenceEquals(target.Blueprint, fixture.HostileBlueprint) ||
                    fixture.UnitsBefore.Any(value => ReferenceEquals(value, target)))
                    throw new InvalidOperationException("Exact request-created hostile receipt unavailable.");
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                var route = Cleanup2GiantRoute(fixture, Sprint14BonePolicy.CrabKey, SummonFamily.NaturesAlly);
                bool canonical = route.Tier == 3 && route.Selected.AssetGuid == CrabSummonAnimationBindingPolicy.SourceGuids[0] &&
                    route.Unit.AssetGuid == CrabSummonAnimationBindingPolicy.UnitGuid;
                Cleanup2Check("crab-attack-route", canonical, new JObject { ["family"] = route.Family.ToString(), ["tier"] = route.Tier,
                    ["parent"] = route.Parent.AssetGuid, ["selected"] = route.Selected.AssetGuid, ["unit"] = route.Unit.AssetGuid,
                    ["accessLimitation"] = "The fixture grants the real canonical SNA III parent to its disposable Wizard; this is not natural class-access evidence.",
                    ["fixtureTargetHP"] = target.Stats.HitPoints.ModifiedValue, ["fixtureTargetBlueprint"] = target.Blueprint.AssetGuid,
                    ["maximumPhaseWallSeconds"] = 150 }, "One actual canonical SNA III parent/selected child/prepared slot supplies the exact Crab.");
                if (!canonical) throw new InvalidOperationException("Exact Crab route changed.");
                Game.Instance.IsPaused = false;
                actor = Cleanup2CastGiant(fixture, book, route);
                foreach (int frame in Cleanup2WaitAppearance(actor, "crab-attack")) yield return frame;
                if (!ExpandedSummoningPlayerPathUnitExact(actor, fixture.Caster)) throw new InvalidOperationException("Positive Crab ownership lost.");
                var manager = actor.View.AnimationManager;
                reviewManager = manager;
                bool productionClawAbsent = manager != null && manager.GetAction(UnitAnimationSpecialAttackType.Claw) == null;
                Cleanup2Check("crab-production-claw-before-experiment", productionClawAbsent, new JObject {
                    ["unit"] = actor.UniqueId, ["productionClawLookupNull"] = productionClawAbsent,
                    ["scope"] = "The borrowed Bite binding is a request-only diagnostic experiment; ordinary production keeps its native missing-Claw fallback." },
                    "The actual live owned Crab's production Claw lookup is null before the exact review lease begins.");
                if (!productionClawAbsent) throw new InvalidOperationException("Production Crab Claw is already bound before the experiment.");
                bindingReviewAttempted = true;
                bindingReview = CrabSummonAnimationBindingPatch.BeginReview(_request, actor, fixture.Caster);
                if (bindingReview == null) throw new InvalidOperationException("Crab request-only binding lease unavailable.");
                var nativeBite = manager.GetAction(UnitAnimationSpecialAttackType.Bite);
                var bound = manager.GetAction(UnitAnimationSpecialAttackType.Claw);
                BlueprintUnit spider = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                    CrabSummonAnimationBindingPolicy.SpiderGuid, "native spider negative control");
                var prefab = spider.Prefab.Load(false);
                var sourceManager = prefab.GetComponentsInChildren<UnitAnimationManager>(true).Single();
                var sourceSet = sourceManager.AnimationSet;
                var sourceActions = sourceSet.Actions.ToArray();
                int beforeForeign = CrabSummonAnimationBindingPatch.BoundLookups(manager);
                bool foreignPreserved = ReferenceEquals(CrabSummonAnimationBindingPatch.Resolve(manager,
                    UnitAnimationSpecialAttackType.Claw, nativeBite), nativeBite) &&
                    beforeForeign == CrabSummonAnimationBindingPatch.BoundLookups(manager);
                bool sourceUnaffected = sourceManager.GetAction(UnitAnimationSpecialAttackType.Claw) == null &&
                    !ExpandedSummoningPteranodonViewPatch.HasLiveOriginalVisual(prefab, Sprint14BonePolicy.CrabKey);
                bool binding = nativeBite != null && ReferenceEquals(nativeBite, bound) && foreignPreserved && sourceUnaffected &&
                    CrabSummonAnimationBindingPatch.NativeReceiptUnchanged(manager) &&
                    ExpandedSummoningPteranodonViewPatch.HasLiveOriginalVisual(actor.View, Sprint14BonePolicy.CrabKey);
                Cleanup2Check("crab-missing-claw-native-binding", binding, new JObject {
                    ["owner"] = actor.UniqueId, ["target"] = target.UniqueId,
                    ["nativeSet"] = manager.AnimationSet.name, ["nativeSetInstance"] = manager.AnimationSet.GetInstanceID(),
                    ["borrowedAction"] = bound?.name, ["borrowedActionInstance"] = bound == null ? 0 : bound.GetInstanceID(),
                    ["clips"] = new JArray(bound == null ? new string[0] : bound.Clips.Select(clip => clip.name)),
                    ["bindingReceipt"] = CrabSummonAnimationBindingPatch.Describe(manager),
                    ["nonnullForeignResultPreservedWithoutBinding"] = foreignPreserved,
                    ["unchangedNativeSpiderClawLookupStillAbsent"] = sourceUnaffected,
                    ["moduleOffAndForeignBodyTests"] = "Pure scope negatives; no global module setting or foreign/native material is changed for a test.",
                    ["hitAnimation"] = manager.GetAction(UnitAnimationType.Hit) == null ? "UNSUPPORTED_NATIVE_DONOR_HAS_NO_HIT_ACTION" : "native-action-present-not-exercised",
                    ["adaptation"] = "REQUEST-ONLY DIAGNOSTIC: the missing Crab Claw borrows the identical Spider Bite action/clip through ordinary handle creation. This changes native act timing and is not a production art-only correction, authored claw clip or finger-closure proof." },
                    "Only this exact request-leased live Crab receives the experimental lookup; nonnull foreign results and the native spider remain unchanged.");
                if (!binding) throw new InvalidOperationException("Native Crab binding refused: " + CrabSummonAnimationBindingPatch.Describe(manager));
                Transform[] drivers = new[] { "L", "R" }.SelectMany(side => Enumerable.Range(1, 7)
                    .Select(index => actor.View.GetComponentsInChildren<Transform>(true).Single(value => value.name == "pedipalp" + index + "_" + side))).ToArray();
                ItemEntityWeapon[] limbs = Sprint19Limbs(actor);
                if (limbs.Length != 2 || limbs.Distinct().Count() != 2 || limbs.Any(value => value.Blueprint.AssetGuid != CrabSummonAnimationBindingPolicy.ClawGuid))
                    throw new InvalidOperationException("Released primary plus additional primary claw routine differs.");
                string baseStats = Cleanup2CrabBaseStats(actor).ToString();
                _sprint25WaitingForUnityAppearance = false;
                foreach (bool full in new[] { false, true })
                {
                    if (watch.Elapsed.TotalSeconds > 150) throw new TimeoutException("Crab phase wall bound.");
                    string stage = full ? "full" : "single";
                    actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                    target.Commands.InterruptAll(true); target.Commands.RemoveFinishedAndUpdateQueue();
                    Sprint20ReleaseOwnHold(fixture, actor);
                    actor.JoinCombat(); target.JoinCombat();
                    JObject lane = Cleanup2CrabAttackLane(fixture, actor, target, stage);
                    actor.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                    var probe = new UnitAttack(target); probe.Init(actor);
                    bool legal = actor.IsEnemy(target) && actor.HasLOS(target) && probe.IsUnitEnoughClose &&
                        !target.Descriptor.State.IsDead && !target.IsBrainActive;
                    Cleanup2Check("crab-ordinary-" + stage + "-lane", legal,
                        new JObject { ["lane"] = lane, ["direction"] = lane["direction"], ["owner"] = actor.UniqueId,
                            ["target"] = target.UniqueId, ["distance"] = actor.DistanceTo(target), ["inRange"] = probe.IsUnitEnoughClose,
                            ["approachRadius"] = probe.ApproachRadius, ["enemy"] = actor.IsEnemy(target), ["los"] = actor.HasLOS(target) },
                        "A bounded graph/sight/party-safe lane is measured from the actual command's native approach radius and passes a fresh post-placement native range check; no range or combat input is overridden.");
                    if (!legal) throw new InvalidOperationException("No native legal Crab attack lane.");
                    actor.Memory.Add(target); target.Memory.Add(actor);
                    var command = new UnitAttack(target) { ForceFullAttack = full, IsSingleAttack = !full };
                    var events = new JArray();
                    var handles = new Cleanup2CrabAttackHandles(bound, full ? limbs : new[] { limbs[0] });
                    var attackObserver = new Sprint24AttackObserver { Owner = actor, Target = target };
                    attackObserver.AfterWeapon = attack => events.Add(new JObject { ["frame"] = Time.frameCount,
                        ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds, ["commandTimeSinceStart"] = command.TimeSinceStart,
                        ["weapon"] = attack.Weapon?.Blueprint?.AssetGuid, ["weaponInstance"] = limbs.ToList().IndexOf(attack.Weapon),
                        ["target"] = attack.Target?.UniqueId, ["hit"] = attack.AttackRoll?.IsHit,
                        ["naturalRoll"] = attack.AttackRoll?.Roll.Value, ["autoMiss"] = attack.AttackRoll?.AutoMiss,
                        ["suspendLog"] = attack.AttackRoll?.SuspendCombatLog, ["damage"] = attack.MeleeDamage?.Damage,
                        ["isCharge"] = attack.IsCharge, ["isAttackOfOpportunity"] = attack.IsAttackOfOpportunity,
                        ["nativeAttackAssociation"] = handles.RecordWeapon(command, attack.Weapon) });
                    var costs = new JArray(); var endings = new JArray(); var states = new JArray();
                    var commandWatch = Stopwatch.StartNew();
                    int startPulse = _sprint25Clock.Pulses; double startGame = Game.Instance.Player.GameTime.TotalSeconds;
                    bool commandQualified = false;
                    EventBus.Subscribe(attackObserver);
                    try
                    {
                        using (var economy = new FirearmReviewCosts(actor, costs))
                        using (var trace = new Sprint26CommandTrace(actor, endings))
                        {
                            actor.Commands.Run(command);
                            int frames = 0;
                            while (!command.IsFinished && frames++ < 3000 && commandWatch.Elapsed.TotalSeconds < 35 && watch.Elapsed.TotalSeconds < 150)
                            {
                                if (!ExpandedSummoningPlayerPathUnitExact(actor, fixture.Caster) ||
                                    !CrabSummonAnimationBindingPatch.NativeReceiptUnchanged(manager) ||
                                    !ExpandedSummoningPteranodonViewPatch.HasLiveOriginalVisual(actor.View, Sprint14BonePolicy.CrabKey))
                                    throw new InvalidOperationException("Crab ownership/native action receipt changed during command.");
                                foreach (Cleanup2CrabHandle series in handles.Observe(command))
                                {
                                    UnitAnimationActionHandle handle = series.Handle;
                                    Cleanup2CrabPose(actor, drivers, series, frames);
                                    float nativeTime = handle.GetTime();
                                    float[] thresholds = { .10f, .45f, .85f };
                                    for (int image = 0; image < thresholds.Length; image++)
                                        if (!series.Captured[image] && nativeTime >= thresholds[image])
                                        {
                                            string file = "cleanup2-crab-ordinary-" + stage + "-limb" + series.Index + "-pose" + image + ".png";
                                            string result = Cleanup2CrabPoseImage(actor, file);
                                            bool saved = result.StartsWith("png=" + file + ";bytes=", StringComparison.Ordinal);
                                            Cleanup2Check("crab-native-pose-" + stage + "-" + series.Index + "-" + image, saved,
                                                new JObject { ["file"] = file, ["result"] = result, ["nativeHandleTime"] = nativeTime,
                                                    ["commandTimeSinceStart"] = command.TimeSinceStart, ["action"] = handle.Action.name,
                                                    ["plannedAttackIndex"] = series.Index, ["observedHandleId"] = series.HandleId,
                                                    ["nativeHandleIdentity"] = RuntimeHelpers.GetHashCode(handle),
                                                    ["attackInfoIdentity"] = RuntimeHelpers.GetHashCode(series.Attack),
                                                    ["weaponEntityIdentity"] = RuntimeHelpers.GetHashCode(series.Weapon),
                                                    ["nativeActiveAnimation"] = handle.ActiveAnimation.GetType().FullName,
                                                    ["forcedPoseOrAct"] = false },
                                                "A real currently active ordinary native command/borrowed clip supplies the actual camera pose; no handle is executed or acted by the observer.");
                                            series.Captured[image] = saved;
                                            break;
                                        }
                                }
                                if (frames % 4 == 0) states.Add(new JObject { ["frame"] = Time.frameCount, ["started"] = command.IsStarted,
                                    ["acted"] = command.IsActed, ["finished"] = command.IsFinished, ["timeSinceStart"] = command.TimeSinceStart,
                                    ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds, ["animationTime"] = command.Animation?.GetTime(),
                                    ["animationActed"] = command.Animation?.IsActed, ["animationFinished"] = command.Animation?.IsFinished,
                                    ["plannedAttackIndex"] = command.GetAttackIndex(),
                                    ["plannedInfoIdentity"] = command.PlannedAttack == null ? 0 : RuntimeHelpers.GetHashCode(command.PlannedAttack),
                                    ["commandHandleIdentity"] = command.Animation == null ? 0 : RuntimeHelpers.GetHashCode(command.Animation),
                                    ["nativeAttackEvents"] = attackObserver.WeaponAttacks.Count });
                                Game.Instance.IsPaused = false; yield return 0;
                            }
                            int expected = full ? 2 : 1;
                            // Observe the terminal state once as well; it may
                            // hold a finished native handle but cannot add a
                            // new accepted motion chain.
                            handles.Observe(command).ToArray();
                            bool association = handles.Exact(command);
                            bool motion = association && handles.Series.All(value => value.Samples >= 4 &&
                                (value.LeftMotion > .001f || value.LeftAngle > .1f) && (value.RightMotion > .001f || value.RightAngle > .1f) && value.Captured.All(saved => saved));
                            bool exact = command.IsFinished && (command.Result == UnitCommand.ResultType.Success || trace.Exhausted(command)) &&
                                command.AllAttacks.Count == expected && attackObserver.WeaponAttacks.Count == expected && attackObserver.OwnerWeaponAttacks.Count == expected &&
                                attackObserver.Rolls.Count == expected && attackObserver.WeaponAttacks.All(value => value.AttackRoll != null && !value.AttackRoll.AutoMiss &&
                                    !value.AttackRoll.SuspendCombatLog && !value.IsCharge && !value.IsAttackOfOpportunity) &&
                                (full ? attackObserver.WeaponAttacks.Select(value => value.Weapon).SequenceEqual(limbs) : ReferenceEquals(attackObserver.WeaponAttacks.Single().Weapon, limbs[0])) &&
                                economy.Count(command) == 1 && economy.Exact(command, false) && economy.ForcedCount(command) == 0 &&
                                !command.Cutscene && !command.IsIgnoreCooldown && motion && !target.Descriptor.State.IsDead &&
                                Cleanup2CrabBaseStats(actor).ToString() == baseStats && Sprint19Limbs(actor).SequenceEqual(limbs) && sourceSet.Actions.SequenceEqual(sourceActions) &&
                                sourceManager.GetAction(UnitAnimationSpecialAttackType.Claw) == null && CrabSummonAnimationBindingPatch.NativeReceiptUnchanged(manager);
                            Cleanup2Check("crab-ordinary-" + stage + "-resolution", exact, new JObject {
                                ["owner"] = actor.UniqueId, ["target"] = target.UniqueId, ["finished"] = command.IsFinished,
                                ["result"] = command.Result.ToString(), ["nativeListExhausted"] = trace.Exhausted(command), ["frames"] = frames,
                                ["wallSeconds"] = commandWatch.Elapsed.TotalSeconds, ["nativeCommandTimeSinceStart"] = command.TimeSinceStart,
                                ["gameTimeDelta"] = Game.Instance.Player.GameTime.TotalSeconds - startGame, ["clockPulses"] = _sprint25Clock.Pulses - startPulse,
                                ["plannedAttacks"] = command.AllAttacks.Count, ["expectedAttacks"] = expected, ["nativeWeaponEvents"] = events,
                                ["costs"] = costs, ["endings"] = endings, ["commandStates"] = states,
                                ["exactNativeAttackHandleAssociation"] = association,
                                ["handleAssociationFailures"] = new JArray(handles.Failures),
                                ["handleStateTransitions"] = handles.Transitions, ["handleReplacements"] = handles.Replacements,
                                ["handleDiagnosticsTruncated"] = handles.DiagnosticsTruncated,
                                ["handleAssociationProof"] = "Exact in-memory AttackHandInfo/weapon/handle references link each planned index to one real native weapon event. Printed identity hashes are diagnostics only. Prestart handles never count as motion; unknown/duplicate active chains fail.",
                                ["handles"] = new JArray(handles.Series.OrderBy(value => value.Index).Select(value => new JObject { ["limb"] = value.Index,
                                    ["plannedAttackIndex"] = value.Index, ["observedHandleId"] = value.HandleId,
                                    ["nativeHandleIdentity"] = RuntimeHelpers.GetHashCode(value.Handle),
                                    ["attackInfoIdentity"] = RuntimeHelpers.GetHashCode(value.Attack),
                                    ["weaponEntityIdentity"] = RuntimeHelpers.GetHashCode(value.Weapon),
                                    ["samples"] = value.Samples, ["leftViewSpaceMotion"] = value.LeftMotion, ["rightViewSpaceMotion"] = value.RightMotion,
                                    ["leftLocalAngleDegrees"] = value.LeftAngle, ["rightLocalAngleDegrees"] = value.RightAngle,
                                    ["threePoseImages"] = value.Captured.All(saved => saved), ["poses"] = value.Poses })),
                                ["positiveChelaDriverMotion"] = motion, ["borrowedNativeSourceStillExact"] = CrabSummonAnimationBindingPatch.NativeReceiptUnchanged(manager),
                                ["baseStatsAndWeaponEntitiesUnchanged"] = Cleanup2CrabBaseStats(actor).ToString() == baseStats,
                                ["forcedRolls"] = 0, ["forcedAnimationActs"] = 0, ["fixtureAttackRulesTriggered"] = 0,
                                ["timingLimit"] = "REQUEST-ONLY TIMING-CHANGING EXPERIMENT. The retained source7 single attack acted at command t=0.6670539s, earlier than native missing-animation ScheduleAct(1). This borrowed Bite is not active in ordinary production. Current act/cost/animation observations use the qualified fixture clock; timing neutrality and natural Default-mode/turn-based timing are NOT claimed.",
                                ["hitAnimation"] = "UNSUPPORTED: no native donor Hit action; not synthesized or exercised.",
                                ["brachiosaurusTail"] = "BLOCKED: no approved native tail pose; no generic alias or Tail binding exists here." },
                                "Ordinary single/full native Crab commands resolve one/two actual claws once with one real cost, finite positive live chela pose motion and unmodified source assets; no supplied hit or forced act.");
                            if (!exact) throw new InvalidOperationException("Native Crab " + stage + " attack/motion/cost proof failed; retained evidence is not timing acceptance.");
                        }
                        commandQualified = true;
                    }
                    finally
                    {
                        EventBus.Unsubscribe(attackObserver);
                        try
                        {
                            // A failed command cannot retain the experimental
                            // lookup while its cleanup interrupts native work.
                            if (!commandQualified) Cleanup2EndCrabBindingReview(ref bindingReview, actor,
                                reviewManager, ref bindingRestorationObserved);
                        }
                        finally
                        {
                            actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                            Sprint20ReleaseOwnHold(fixture, actor);
                        }
                    }
                }
            }
            finally
            {
                var failures = new List<string>();
                Action<string, Action> restore = (label, action) => { try { action(); } catch (Exception error) { failures.Add(label + ":" + error.GetType().Name + ":" + error.Message); } };
                try
                {
                    if (bindingReviewAttempted) restore("request-binding", () => Cleanup2EndCrabBindingReview(
                        ref bindingReview, actor, reviewManager, ref bindingRestorationObserved));
                    _expandedSummoningPlayerPathCaptureActive = false;
                    restore("hooks", () => hooks.UnpatchAll(hooks.Id));
                    if (actor != null && !actor.Destroyed)
                    {
                        restore("owned-command", () => { actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue(); });
                        restore("owned-hold", () => Sprint20ReleaseOwnHold(fixture, actor));
                    }
                    restore("owned-hostile", () => ResetExpandedSummoningHostile(fixture));
                    ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningRuleCapture.AddRange(oldCapture);
                    ExpandedSummoningPlayerPathEvents.Clear(); ExpandedSummoningPlayerPathEvents.AddRange(oldEvents);
                    _expandedSummoningPlayerPathRuleCastCount = oldCasts; _expandedSummoningPlayerPathSpawnActionCount = oldSpawns;
                }
                finally { _sprint25WaitingForUnityAppearance = waiting; restore("pause", () => Game.Instance.IsPaused = pause); }
                Cleanup2Check("crab-attack-observer-restored", failures.Count == 0, new JObject { ["failures"] = new JArray(failures),
                    ["wallSeconds"] = watch.Elapsed.TotalSeconds, ["sharedFixtureOwnsExactSummonAndHostileTeardown"] = true,
                    ["requestOnlyBindingDisposedAndProductionClawNull"] = !bindingReviewAttempted || bindingRestorationObserved,
                    ["humanSaveWrites"] = 0, ["newAnimationAssetsOwnedOrDestroyed"] = 0 },
                    "Only the exact disposable command/hold/capture state is cleaned; borrowed animation assets are never destroyed or changed, and the shared positive actor fixture owns final teardown.");
                if (failures.Count > 0) throw new InvalidOperationException("Crab attack observer cleanup failed: " + string.Join("|", failures.ToArray()));
            }
        }

        private void Cleanup2EndCrabBindingReview(ref IDisposable lease, UnitEntityData actor,
            UnitAnimationManager manager, ref bool observed)
        {
            if (lease != null)
            {
                IDisposable owned = lease;
                try { owned.Dispose(); }
                finally { lease = null; }
            }
            if (observed) return;
            bool liveExactManager = actor != null && !actor.Destroyed && actor.View != null &&
                manager != null && ReferenceEquals(actor.View.AnimationManager, manager);
            bool absent = liveExactManager && manager.GetAction(UnitAnimationSpecialAttackType.Claw) == null;
            Cleanup2Check("crab-production-claw-after-experiment", absent, new JObject {
                ["unit"] = actor?.UniqueId, ["sameLiveManager"] = liveExactManager,
                ["productionClawLookupNull"] = absent, ["reviewLeaseDisposedBeforeFinalCommandCleanup"] = lease == null,
                ["productionBehavior"] = "Native missing-Claw lookup/fallback restored; this request-only timing-changing experiment supplies no default production animation binding." },
                "After disposing the exact experiment lease, the same still-live owned Crab manager again returns null for production Claw lookup.");
            observed = absent;
            if (!absent) throw new InvalidOperationException("Production Crab Claw null fallback restoration was not observed on the same live manager.");
        }

        private static JObject Cleanup2CrabBaseStats(UnitEntityData actor)
        {
            var limbs = Sprint19Limbs(actor);
            return new JObject { ["size"] = actor.Descriptor.State.Size.ToString(), ["strengthBase"] = actor.Stats.Strength.BaseValue,
                ["dexterityBase"] = actor.Stats.Dexterity.BaseValue, ["constitutionBase"] = actor.Stats.Constitution.BaseValue,
                ["intelligenceBase"] = actor.Stats.Intelligence.BaseValue, ["wisdomBase"] = actor.Stats.Wisdom.BaseValue,
                ["charismaBase"] = actor.Stats.Charisma.BaseValue, ["BABBase"] = actor.Stats.BaseAttackBonus.BaseValue,
                ["HPBase"] = actor.Stats.HitPoints.BaseValue, ["speedBase"] = actor.Stats.Speed.BaseValue,
                ["weapons"] = new JArray(limbs.Select(value => new JObject { ["sessionReferenceToken"] = RuntimeHelpers.GetHashCode(value), ["blueprint"] = value.Blueprint.AssetGuid })) };
        }

        private JObject Cleanup2CrabAttackLane(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData actor, UnitEntityData target, string stage)
        {
            if (!IsSummonCleanup2Review || (string)_request.Parameters?["reviewPhase"] != "crab-attack" ||
                fixture == null || fixture.Caster == null || actor == null || target == null || actor.Destroyed || target.Destroyed ||
                !ReferenceEquals(fixture.Hostile, target) || !ReferenceEquals(target.Blueprint, fixture.HostileBlueprint) ||
                fixture.UnitsBefore.Any(value => ReferenceEquals(value, actor) || ReferenceEquals(value, target)) ||
                !ExpandedSummoningPlayerPathUnitExact(actor, fixture.Caster) ||
                TurnBased.Controllers.CombatController.IsInTurnBasedCombat() || AstarPath.active == null ||
                Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance == null)
                throw new InvalidOperationException("Exact owned RTWP Crab/hostile graph-and-sight survey required.");
            // Read the real command's native reach/corpulence calculation.
            // Never supply a guessed reach or change ApproachRadius/weapon data.
            var range = new UnitAttack(target); range.Init(actor);
            float radius = range.ApproachRadius, footprint = actor.Corpulence + target.Corpulence;
            if (!Cleanup2Finite(radius) || !Cleanup2Finite(footprint) || radius <= 0f || footprint <= 0f)
                throw new InvalidOperationException("Non-finite native Crab command range/footprint.");
            float[] gaps = new[] { .15f, .30f, .50f }.Select(margin => radius - margin)
                .Where(value => value >= footprint + .05f).Distinct().ToArray();
            // Installed 2.1.7b UnitMovementAgent.PathTo060018A3 selects mask1
            // outside turn-based combat; ConstructTags060018C3 returns -1.
            // This constraint is ours, and no native graph/path/seeker changes.
            var constraint = new Pathfinding.PathNNConstraint { graphMask = 1,
                constrainWalkability = true, walkable = true, constrainArea = false,
                constrainTags = true, tags = -1, constrainDistance = true,
                distanceXZ = false, distanceXZkeepY = false };
            Pathfinding.NNInfo anchor = AstarPath.active.GetNearest(fixture.Caster.Position, constraint);
            if (anchor.node == null || !anchor.node.Walkable || !Cleanup2Finite(anchor.clampedPosition))
                throw new InvalidOperationException("No finite native walking anchor for the Crab attack fixture.");
            constraint.SetStart(anchor.node); constraint.constrainArea = true;
            var party = Game.Instance.Player.Party.Where(value => value != null && value.IsInGame).ToArray();
            if (party.Any(value => ReferenceEquals(value, actor) || ReferenceEquals(value, target)))
                throw new InvalidOperationException("Crab survey refuses party actor/hostile ownership.");
            var offsets = Enumerable.Range(-5, 11).SelectMany(x => Enumerable.Range(-5, 11)
                .Select(z => new Vector3(x * 3f, 0f, z * 3f))).OrderBy(value => value.sqrMagnitude).ToArray();
            var watch = Stopwatch.StartNew();
            var refusals = new Dictionary<string, int>(StringComparer.Ordinal);
            var examples = new JArray();
            int attempts = 0, origins = 0, graphPairs = 0, sightPairs = 0, safePairs = 0, placedPairs = 0;
            JObject selected = null;
            Func<Pathfinding.NNInfo, Vector3, bool> connected = (point, requested) => point.node != null && point.node.Walkable &&
                point.node.Area == anchor.node.Area && point.node.GraphIndex == anchor.node.GraphIndex &&
                Cleanup2Finite(point.clampedPosition) && Vector3.Distance(point.clampedPosition, requested) <= .6f;
            Action<string> refuse = reason => { int count; refusals.TryGetValue(reason, out count); refusals[reason] = count + 1; };
            foreach (Vector3 offset in offsets)
            {
                if (selected != null || watch.Elapsed.TotalSeconds > 5 || attempts >= 4096) break;
                Vector3 requestedOrigin = anchor.clampedPosition + offset;
                var origin = AstarPath.active.GetNearest(requestedOrigin, constraint);
                if (!connected(origin, requestedOrigin)) { refuse("origin-native-graph-or-60cm-clamp"); continue; }
                origins++;
                foreach (float gap in gaps)
                for (int direction = 0; direction < CompassOffsets.Length; direction++)
                {
                    if (selected != null || watch.Elapsed.TotalSeconds > 5 || attempts >= 4096) break;
                    attempts++;
                    Vector3 requestedTarget = origin.clampedPosition + CompassOffsets[direction].normalized * gap;
                    var destination = AstarPath.active.GetNearest(requestedTarget, constraint);
                    if (!connected(destination, requestedTarget)) { refuse("target-native-graph-or-60cm-clamp"); continue; }
                    graphPairs++;
                    Vector3 graphDelta = origin.clampedPosition - destination.clampedPosition; graphDelta.y = 0f;
                    float graphSeparation = graphDelta.magnitude;
                    if (graphSeparation < footprint + .05f) { refuse("native-footprints-overlap"); continue; }
                    if (Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(
                        origin.clampedPosition + Vector3.up * 1.2f, destination.clampedPosition + Vector3.up * 1.2f, 0))
                    { refuse("native-geometry-line-of-sight"); continue; }
                    sightPairs++;
                    float partyGap = party.Length == 0 ? float.PositiveInfinity : party.Min(value => Mathf.Min(
                        Vector3.Distance(value.Position, origin.clampedPosition), Vector3.Distance(value.Position, destination.clampedPosition)));
                    if (partyGap < 3.5f) { refuse("protected-party-within-3point5m"); continue; }
                    safePairs++;
                    PlaceExpandedSummoningUnit(actor, origin.clampedPosition);
                    PlaceExpandedSummoningUnit(target, destination.clampedPosition);
                    placedPairs++;
                    actor.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                    float actualGap = actor.DistanceTo(target);
                    Vector3 actualDelta = actor.Position - target.Position; actualDelta.y = 0f;
                    float planarGap = actualDelta.magnitude;
                    bool inRange = range.IsUnitEnoughClose, mutualSight = actor.HasLOS(target) && target.HasLOS(actor);
                    bool legal = inRange && mutualSight && actor.IsEnemy(target) && !target.Descriptor.State.IsDead &&
                        !target.IsBrainActive && planarGap >= footprint + .05f;
                    var row = new JObject { ["attempt"] = attempts, ["direction"] = direction,
                        ["requestedGapFromNativeRange"] = gap, ["nativeApproachRadius"] = range.ApproachRadius,
                        ["originGraphPoint"] = SurveyVector(origin.clampedPosition), ["targetGraphPoint"] = SurveyVector(destination.clampedPosition),
                        ["ownerPostPlacement"] = SurveyVector(actor.Position), ["targetPostPlacement"] = SurveyVector(target.Position),
                        ["postPlacementDistance"] = actualGap, ["postPlacementPlanarFootprintGap"] = planarGap,
                        ["nativeInRange"] = inRange, ["mutualNativeLOS"] = mutualSight,
                        ["enemy"] = actor.IsEnemy(target), ["nativeFootprintSum"] = footprint,
                        ["protectedPartyGap"] = float.IsPositiveInfinity(partyGap) ? (JToken)JValue.CreateNull() : new JValue(partyGap),
                        ["legal"] = legal };
                    if (examples.Count < 128 || legal) examples.Add(row);
                    if (legal) selected = row;
                    else refuse(!inRange ? "post-placement-native-out-of-range" : !mutualSight ? "post-placement-native-LOS" : "post-placement-enemy-state-or-footprint");
                }
            }
            var evidence = new JObject { ["stage"] = stage, ["owner"] = actor.UniqueId, ["target"] = target.UniqueId,
                ["nativeApproachRadius"] = radius, ["nativeFootprintSum"] = footprint, ["requestedGaps"] = new JArray(gaps),
                ["queryGraphMask"] = constraint.graphMask, ["queryConnectedArea"] = constraint.area, ["queryTags"] = constraint.tags,
                ["anchor"] = SurveyVector(anchor.clampedPosition), ["nativeGraphIndex"] = anchor.node.GraphIndex,
                ["attempts"] = attempts, ["originCandidates"] = origins, ["nativeGraphPairs"] = graphPairs,
                ["nativeGeometrySightPairs"] = sightPairs, ["protectedPartySafePairs"] = safePairs, ["placedPairs"] = placedPairs,
                ["attemptLimit"] = 4096, ["wallLimitSeconds"] = 5, ["wallSeconds"] = watch.Elapsed.TotalSeconds,
                ["refusals"] = new JObject(refusals.Select(pair => new JProperty(pair.Key, pair.Value))),
                ["placedExamplesLimit"] = 128, ["placedExamples"] = examples, ["selected"] = selected,
                ["direction"] = selected?["direction"],
                ["proof"] = "Exact request-owned actors only; native RTWP walking graph/connected area, finite points and <=60cm graph clamp; native geometry sight, >=3.5m protected-party clearance and nonoverlapping native footprints. Acceptance requires the real command's post-placement IsUnitEnoughClose plus mutual LOS/enemy/state. No guessed reach, fixed gap floor, range waiver or command is issued by this survey." };
            Cleanup2Check("crab-native-range-lane-survey-" + stage, selected != null, evidence,
                "A finite Crab-specific graph/sight/party-safe survey places the exact hostile inside the unchanged real native command radius before any attack command.");
            if (selected == null) throw new InvalidOperationException("No proven native in-range Crab attack placement within the recorded bounded survey.");
            return evidence;
        }

        private static void Cleanup2CrabPose(UnitEntityData actor, Transform[] drivers, Cleanup2CrabHandle series, int frame)
        {
            Vector3[] positions = drivers.Select(value => actor.View.transform.InverseTransformPoint(value.position)).ToArray();
            Quaternion[] rotations = drivers.Select(value => value.localRotation).ToArray();
            if (positions.Any(value => !Cleanup2Finite(value))) throw new InvalidOperationException("Non-finite native chela driver pose.");
            if (series.FirstPosition == null) { series.FirstPosition = positions; series.FirstRotation = rotations; }
            series.Samples++;
            for (int i = 0; i < drivers.Length; i++)
            {
                float distance = Vector3.Distance(positions[i], series.FirstPosition[i]), angle = Quaternion.Angle(rotations[i], series.FirstRotation[i]);
                if (!Cleanup2Finite(angle)) throw new InvalidOperationException("Non-finite native chela driver rotation.");
                if (i < 7) { series.LeftMotion = Mathf.Max(series.LeftMotion, distance); series.LeftAngle = Mathf.Max(series.LeftAngle, angle); }
                else { series.RightMotion = Mathf.Max(series.RightMotion, distance); series.RightAngle = Mathf.Max(series.RightAngle, angle); }
            }
            if (frame % 4 == 0) series.Poses.Add(new JObject { ["frame"] = Time.frameCount, ["nativeHandleTime"] = series.Handle.GetTime(),
                ["plannedAttackIndex"] = series.Index, ["observedHandleId"] = series.HandleId,
                ["attackInfoIdentity"] = RuntimeHelpers.GetHashCode(series.Attack),
                ["weaponEntityIdentity"] = RuntimeHelpers.GetHashCode(series.Weapon),
                ["viewSpaceChelaDriverPoints"] = new JArray(positions.Select(SurveyVector)),
                ["basis"] = "Current unit view origin/axes; native root motion is removed. Fourteen real pedipalp driver transform points, not proprietary mesh vertices." });
        }

        private string Cleanup2CrabPoseImage(UnitEntityData actor, string file)
        {
            if (File.Exists(Path.Combine(_request.EvidenceDirectory, file))) throw new InvalidOperationException("Crab pose image already exists.");
            Bounds previous = _refinementBounds;
            try
            {
                Vector3 center = RefinementCentre(actor);
                _refinementBounds = RefinementBounds(actor, center);
                float distance = Mathf.Max(3.5f, _refinementBounds.extents.magnitude * 2.4f);
                Vector3 direction = (actor.View.transform.right * .8f - actor.View.transform.forward * 1.2f + Vector3.up).normalized;
                Vector3 eye = center + direction * distance;
                return WriteRefinementImage(eye, Quaternion.LookRotation(center - eye, Vector3.up), file);
            }
            finally { _refinementBounds = previous; }
        }
    }
}
