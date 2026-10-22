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
            internal int Index, Samples;
            internal bool[] Captured = new bool[3];
            internal Vector3[] FirstPosition;
            internal Quaternion[] FirstRotation;
            internal float LeftMotion, RightMotion, LeftAngle, RightAngle;
            internal readonly JArray Poses = new JArray();
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
                    ["adaptation"] = "Missing Crab Claw request borrows the identical native Spider Bite action/clip through ordinary handle creation. It is not a newly authored claw clip or finger-closure proof." },
                    "Only the live provenance-owned registered Crab receives the missing Claw lookup; nonnull foreign results and the actual native spider remain unchanged.");
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
                    Vector3 far; string lane, direction;
                    Vector3 origin = Sprint24AiCombatLane(fixture.Caster, actor, target, actor.Corpulence + target.Corpulence + .8f,
                        out far, out lane, out direction);
                    PlaceExpandedSummoningUnit(actor, origin); PlaceExpandedSummoningUnit(target, far);
                    actor.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                    var probe = new UnitAttack(target); probe.Init(actor);
                    bool legal = actor.IsEnemy(target) && actor.HasLOS(target) && probe.IsUnitEnoughClose &&
                        !target.Descriptor.State.IsDead && !target.IsBrainActive;
                    Cleanup2Check("crab-ordinary-" + stage + "-lane", legal,
                        new JObject { ["lane"] = lane, ["direction"] = direction, ["owner"] = actor.UniqueId,
                            ["target"] = target.UniqueId, ["distance"] = actor.DistanceTo(target), ["inRange"] = probe.IsUnitEnoughClose,
                            ["approachRadius"] = probe.ApproachRadius, ["enemy"] = actor.IsEnemy(target), ["los"] = actor.HasLOS(target) },
                        "The established qualified hostile/lane setup supplies an actual legal visible attack target; no range or combat input is overridden.");
                    if (!legal) throw new InvalidOperationException("No native legal Crab attack lane.");
                    actor.Memory.Add(target); target.Memory.Add(actor);
                    var command = new UnitAttack(target) { ForceFullAttack = full, IsSingleAttack = !full };
                    var events = new JArray();
                    var attackObserver = new Sprint24AttackObserver { Owner = actor, Target = target };
                    attackObserver.AfterWeapon = attack => events.Add(new JObject { ["frame"] = Time.frameCount,
                        ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds, ["commandTimeSinceStart"] = command.TimeSinceStart,
                        ["weapon"] = attack.Weapon?.Blueprint?.AssetGuid, ["weaponInstance"] = limbs.ToList().IndexOf(attack.Weapon),
                        ["target"] = attack.Target?.UniqueId, ["hit"] = attack.AttackRoll?.IsHit,
                        ["naturalRoll"] = attack.AttackRoll?.Roll.Value, ["autoMiss"] = attack.AttackRoll?.AutoMiss,
                        ["suspendLog"] = attack.AttackRoll?.SuspendCombatLog, ["damage"] = attack.MeleeDamage?.Damage,
                        ["isCharge"] = attack.IsCharge, ["isAttackOfOpportunity"] = attack.IsAttackOfOpportunity });
                    var costs = new JArray(); var endings = new JArray(); var states = new JArray();
                    var handles = new Dictionary<UnitAnimationActionHandle, Cleanup2CrabHandle>();
                    var commandWatch = Stopwatch.StartNew();
                    int startPulse = _sprint25Clock.Pulses; double startGame = Game.Instance.Player.GameTime.TotalSeconds;
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
                                foreach (var info in command.AllAttacks)
                                {
                                    UnitAnimationActionHandle handle = info.AnimationHandle;
                                    if (handle == null || !ReferenceEquals(handle.Action, bound)) continue;
                                    Cleanup2CrabHandle series;
                                    if (!handles.TryGetValue(handle, out series))
                                    {
                                        series = new Cleanup2CrabHandle { Handle = handle, Index = handles.Count };
                                        handles.Add(handle, series);
                                    }
                                    if (!handle.IsStarted || handle.IsFinished || handle.ActiveAnimation == null) continue;
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
                                    ["nativeAttackEvents"] = attackObserver.WeaponAttacks.Count });
                                Game.Instance.IsPaused = false; yield return 0;
                            }
                            int expected = full ? 2 : 1;
                            bool motion = handles.Count == expected && handles.Values.All(value => value.Samples >= 4 &&
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
                                ["handles"] = new JArray(handles.Values.Select(value => new JObject { ["limb"] = value.Index,
                                    ["samples"] = value.Samples, ["leftViewSpaceMotion"] = value.LeftMotion, ["rightViewSpaceMotion"] = value.RightMotion,
                                    ["leftLocalAngleDegrees"] = value.LeftAngle, ["rightLocalAngleDegrees"] = value.RightAngle,
                                    ["threePoseImages"] = value.Captured.All(saved => saved), ["poses"] = value.Poses })),
                                ["positiveChelaDriverMotion"] = motion, ["borrowedNativeSourceStillExact"] = CrabSummonAnimationBindingPatch.NativeReceiptUnchanged(manager),
                                ["baseStatsAndWeaponEntitiesUnchanged"] = Cleanup2CrabBaseStats(actor).ToString() == baseStats,
                                ["forcedRolls"] = 0, ["forcedAnimationActs"] = 0, ["fixtureAttackRulesTriggered"] = 0,
                                ["timingLimit"] = "Native action/animation events are observed under the established request-local frame-synchronous Dialog clock. Missing-animation ScheduleAct(1) becomes native clip event timing (clip length about1.167s); timing-neutrality and natural Default-mode/turn-based timing are NOT claimed.",
                                ["hitAnimation"] = "UNSUPPORTED: no native donor Hit action; not synthesized or exercised.",
                                ["brachiosaurusTail"] = "BLOCKED: no approved native tail pose; no generic alias or Tail binding exists here." },
                                "Ordinary single/full native Crab commands resolve one/two actual claws once with one real cost, finite positive live chela pose motion and unmodified source assets; no supplied hit or forced act.");
                            if (!exact) throw new InvalidOperationException("Native Crab " + stage + " attack/motion/cost proof failed; retained evidence is not timing acceptance.");
                        }
                    }
                    finally
                    {
                        EventBus.Unsubscribe(attackObserver);
                        actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                        Sprint20ReleaseOwnHold(fixture, actor);
                    }
                }
            }
            finally
            {
                var failures = new List<string>();
                Action<string, Action> restore = (label, action) => { try { action(); } catch (Exception error) { failures.Add(label + ":" + error.GetType().Name + ":" + error.Message); } };
                try
                {
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
                    ["humanSaveWrites"] = 0, ["newAnimationAssetsOwnedOrDestroyed"] = 0 },
                    "Only the exact disposable command/hold/capture state is cleaned; borrowed animation assets are never destroyed or changed, and the shared positive actor fixture owns final teardown.");
                if (failures.Count > 0) throw new InvalidOperationException("Crab attack observer cleanup failed: " + string.Join("|", failures.ToArray()));
            }
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
