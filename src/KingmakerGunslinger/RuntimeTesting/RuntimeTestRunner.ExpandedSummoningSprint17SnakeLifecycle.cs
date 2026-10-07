using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewSprint17SnakeViewLifecycle(ExpandedSummoningCorrectionFixture fixture)
        {
            // Fresh exact native control: capture its actual initialized action
            // set, then retire the owned actor before any combat cell. Borrowed
            // native assets stay alive and must remain identical throughout.
            var native = fixture.Blueprints.OfType<BlueprintUnit>().Single(value =>
                value.AssetGuid == "bf2216f48b3f4d24c9c502007649340d");
            if (native.Prefab.AssetId != "130f0866af3249a4e817ec7e6e9ecd89")
                throw new InvalidOperationException("Native snake lifecycle control identity changed.");
            var control = Game.Instance.EntityCreator.SpawnUnit(native, fixture.Caster.Position,
                Quaternion.identity, fixture.Scene);
            if (control == null) throw new InvalidOperationException("Native snake lifecycle control missing.");
            fixture.Created.Add(control);
            Kingmaker.Visual.Animation.AnimationSet nativeSet;
            UnitAnimationAction nativeHit;
            Kingmaker.Visual.Animation.Actions.AnimationActionBase[] nativeActions;
            try
            {
                SetExpandedSummoningBrainActive(control, false);
                nativeSet = control.View.AnimationManager.AnimationSet;
                nativeHit = control.View.AnimationManager.GetAction(UnitAnimationType.Hit);
                nativeActions = nativeSet.Actions.ToArray();
                bool absent = nativeHit == null && nativeActions.OfType<UnitAnimationAction>().All(value =>
                    value.Type != UnitAnimationType.Hit);
                CheckSprint17Final("native-worm-hit-carrier", absent &&
                    control.View.GetComponent<SerpentineVisualAttachment>() == null &&
                    ExpandedSummoningSerpentineViewPatch.DescribeView(control.View) == "not-attempted",
                    new JObject { ["nativeBlueprint"] = native.AssetGuid, ["prefab"] = native.Prefab.AssetId,
                        ["set"] = nativeSet.name, ["nativeHitActionPresent"] = nativeHit != null,
                        ["types"] = new JArray(nativeActions.OfType<UnitAnimationAction>().Select(value => value.Type.ToString())) },
                    "exact native Worm control has no Hit action; no project hook or fabricated hit-reaction clip");
                if (!absent) throw new InvalidOperationException("Recorded native Hit absence changed; review the carrier.");
            }
            finally { control.Destroy(); Game.Instance.EntityDestroyer.Tick(); }
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                UnitEntityData owner = CastSprint17FinalSnake(fixture, key);
                string point;
                UnitEntityData attacker = CreateSprint17ContactTarget(fixture,
                    ExpandedSummoningOpenPoint(owner.Position, 1.5f, new List<int>(), out point));
                var originalFaction = owner.Faction;
                var originalAttackFactions = owner.AttackFactions.ToArray();
                string originalGroup = owner.GroupId;
                int bonus = attacker.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                var resources = new List<UnityEngine.Object>();
                var samples = new JArray();
                var observer = new Sprint16RuleObserver { Owner = attacker, Target = owner };
                bool subscribed = false, hitPlayed = false, deathPlayed = false, deadObserved = false, finite = true;
                float largestDissolve = 0f;
                var attacks = new List<UnitAttack>();
                var hitEvents = new JArray();
                JObject expiry = null;
                try
                {
                    owner.Descriptor.Stats.HitPoints.BaseValue = 1000;
                    foreach (int step in WaitSprint17FinalAppearance(new[] { owner })) yield return step;
                    var attachment = owner.View.GetComponent<SerpentineVisualAttachment>();
                    resources.AddRange(attachment.CaptureOwnedResources());
                    var animationSet = owner.View.AnimationManager.AnimationSet;
                    CheckSprint17Final(key + "-lifecycle-intact", attachment.OriginalBodyLive &&
                        Sprint17BodyIntact(owner.View, SerpentineVisualPolicy.BodyRenderer(key)),
                        Sprint17OriginalBodySample(owner, attachment.Body),
                        "native appearance ends on intact original geometry before any hit/death; no visibility or pose forcing");

                    // One request-owned inert attacker and one request-owned snake.
                    // Real native UnitAttack dispatch, not an animation invocation.
                    var faction = UnityEngine.Object.Instantiate(originalFaction);
                    _serpentineContactPrototypes.Add(faction);
                    faction.name = "KMG_Runtime_Sprint17_LifecycleSnake";
                    faction.Peaceful = faction.AlwaysEnemy = faction.Neutral = false;
                    faction.Dummy = null;
                    faction.AttackFactions = new[] { attacker.Faction };
                    owner.Descriptor.SwitchFactions(faction, false);
                    owner.GroupId = "KMG_Runtime_Sprint17_Lifecycle_" + owner.UniqueId;
                    owner.AttackFactions.Match(new[] { attacker.Faction });
                    attacker.AttackFactions.Match(new[] { faction });
                    if (!Game.Instance.State.AwakeUnits.Contains(attacker)) Game.Instance.State.AwakeUnits.Add(attacker);
                    attacker.Descriptor.Stats.AdditionalAttackBonus.BaseValue = 100;
                    attacker.Memory.Add(owner); owner.Memory.Add(attacker);
                    attacker.JoinCombat(); owner.JoinCombat();
                    new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                    Game.Instance.Player.UpdateIsInCombat();
                    JObject isolation = Sprint17LifecycleIsolation(fixture, owner, attacker);
                    if ((bool)isolation["passed"] != true)
                        throw new InvalidOperationException("Hit drill isolation: " + isolation);
                    // The native visual hit path rejects rear/side hits. Set
                    // only the owned actor's native facing while unpaused;
                    // allow the native view to settle, without touching bones,
                    // an animation handle, its clock, or its weight.
                    Game.Instance.IsPaused = false;
                    owner.ForceLookAt(attacker.Position);
                    JObject facing = null;
                    for (int frame = 0; frame < 180; frame++)
                    {
                        yield return 0;
                        facing = Sprint17NativeHitFacing(owner, attacker, true);
                        if ((bool)facing["eligible"]) break;
                    }
                    if (facing == null || !(bool)facing["eligible"])
                        throw new InvalidOperationException("Owned frontal hit setup did not settle: " + facing);
                    observer.ObserveWeaponContact = evt =>
                    {
                        if (!ReferenceEquals(evt.Target, owner)) return;
                        JObject contact = Sprint17NativeHitFacing(owner, attacker, evt.AttackRoll.IsHit);
                        contact["result"] = evt.AttackRoll.Result.ToString();
                        contact["damage"] = evt.MeleeDamage == null ? 0 : evt.MeleeDamage.Damage;
                        hitEvents.Add(contact);
                    };
                    EventBus.Subscribe(observer); subscribed = true;
                    DateTime deadline = DateTime.UtcNow.AddSeconds(25);
                    for (int frame = 0; frame < 1200 && DateTime.UtcNow < deadline; frame++)
                    {
                        Game.Instance.IsPaused = false;
                        if (attacks.Count < 4 && (attacks.Count == 0 || attacks.Last().IsFinished) &&
                            !observer.Damage.Any(value => value.Damage > 0))
                        {
                            var command = new UnitAttack(owner);
                            command.Init(attacker); attacker.Commands.Run(command); attacks.Add(command);
                        }
                        yield return 0;
                        var currentIsolation = Sprint17LifecycleIsolation(fixture, owner, attacker);
                        if ((bool)currentIsolation["passed"] != true)
                            throw new InvalidOperationException("Hit drill isolation changed: " + currentIsolation);
                        ObserveSprint17LifecyclePose(owner, animationSet, "hit", samples,
                            ref hitPlayed, ref deathPlayed, ref deadObserved, ref finite, ref largestDissolve, resources);
                        if (observer.Damage.Any(value => value.Damage > 0) && (hitPlayed || nativeHit == null)) break;
                    }
                    InterruptExpandedSummoningFixtureCommands(attacker);
                    // Observe the real post-wound view for native elapsed time,
                    // including rigs without a flinch, not only the event frame.
                    float postHitStart = Time.time;
                    for (int frame = 0; frame < 180 && Time.time - postHitStart < .5f; frame++)
                    {
                        yield return 0;
                        if (!(bool)Sprint17LifecycleIsolation(fixture, owner, attacker)["passed"])
                            throw new InvalidOperationException("Owned hit pair lost isolation after its wound.");
                        ObserveSprint17LifecyclePose(owner, animationSet, "hit", samples,
                            ref hitPlayed, ref deathPlayed, ref deadObserved, ref finite, ref largestDissolve, resources);
                    }
                    JObject postWoundPose = Sprint17OriginalBodySample(owner, attachment.Body);
                    finite &= (bool)postWoundPose["finite"] && (bool)postWoundPose["poseFinite"];
                    bool exactNative = ReferenceEquals(animationSet, nativeSet) && nativeSet != null &&
                        nativeActions.SequenceEqual(nativeSet.Actions) &&
                        ReferenceEquals(owner.View.AnimationManager.GetAction(UnitAnimationType.Hit), nativeHit);
                    bool frontalWound = attacks.Any(value => value.IsStarted) &&
                        observer.Damage.Any(value => value.Damage > 0) &&
                        hitEvents.Any(value => (bool)value["eligible"] && (int)value["damage"] > 0);
                    CheckSprint17Final(key + "-native-hit", SerpentineFinalReviewPolicy.FaithfulHitLifecycle(
                        exactNative, nativeHit != null, owner.View.AnimationManager.GetAction(UnitAnimationType.Hit) != null,
                        frontalWound, finite && Time.time - postHitStart >= .5f &&
                            Sprint17BodyIntact(owner.View, SerpentineVisualPolicy.BodyRenderer(key)),
                        !owner.Descriptor.State.IsDead, hitPlayed),
                        new JObject { ["playedHitClip"] = hitPlayed, ["finiteOriginalPose"] = finite,
                            ["exactNativeActionSet"] = exactNative, ["nativeControlHasHit"] = nativeHit != null,
                            ["hitReactionDisposition"] = nativeHit == null ? "native rig has no Hit action; no flinch claimed" : "native carrier requires playback",
                            ["postWoundNativeSeconds"] = Time.time - postHitStart,
                            ["postWoundPose"] = postWoundPose,
                            ["commands"] = attacks.Count, ["started"] = attacks.Count(value => value.IsStarted),
                            ["isolation"] = isolation,
                            ["nativeFacingSetup"] = facing, ["actualHitEvents"] = hitEvents,
                            ["weaponRules"] = observer.Attacks.Count, ["damageBundles"] = observer.Damage.Count,
                            ["damage"] = owner.Damage, ["samples"] = samples.DeepClone() },
                        "actual frontal wound preserves intact finite original geometry and the exact native carrier's hit behavior; absence is explicit, not claimed playback");

                    GameHelper.KillUnit(owner, attacker);
                    Game.Instance.IsPaused = false;
                    deadline = DateTime.UtcNow.AddSeconds(8);
                    for (int frame = 0; frame < 600 && DateTime.UtcNow < deadline && owner.View != null; frame++)
                    {
                        Game.Instance.IsPaused = false;
                        yield return 0;
                        ObserveSprint17LifecyclePose(owner, animationSet, "death", samples,
                            ref hitPlayed, ref deathPlayed, ref deadObserved, ref finite, ref largestDissolve, resources);
                    }
                    CheckSprint17Final(key + "-native-death", deadObserved && deathPlayed && finite,
                        new JObject { ["deadObserved"] = deadObserved, ["playedDeathClip"] = deathPlayed,
                            ["finiteOriginalPose"] = finite, ["samples"] = samples.DeepClone() },
                        "native KillUnit boundary selects and actually plays the native death/prone action; no IsActed-only or synthetic pose proof");

                    // A native corpse may persist until its summon expires.
                    // Move only this owned marker deadline through the qualified
                    // native scheduler, never world time or the material fader.
                    if (!owner.Destroyed)
                    {
                        Buff marker = owner.Descriptor.Buffs.Enumerable.Single(value =>
                            ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                        expiry = ExpireSprint16OwnedBuff(owner, marker);
                    }
                    deadline = DateTime.UtcNow.AddSeconds(15);
                    for (int frame = 0; frame < 1200 && DateTime.UtcNow < deadline; frame++)
                    {
                        Game.Instance.IsPaused = false;
                        yield return 0;
                        ObserveSprint17LifecyclePose(owner, animationSet, "expiry-fade", samples,
                            ref hitPlayed, ref deathPlayed, ref deadObserved, ref finite, ref largestDissolve, resources);
                        if (owner.Destroyed && owner.View == null && resources.All(value => value == null)) break;
                    }
                    CheckSprint17Final(key + "-native-fade-despawn", finite && largestDissolve > .05f &&
                        owner.Destroyed && owner.View == null && owner.HoldingState == null,
                        new JObject { ["maxOriginalDissolve"] = largestDissolve, ["nativeExpiry"] = expiry,
                            ["destroyed"] = owner.Destroyed, ["viewGone"] = owner.View == null,
                            ["samples"] = samples.DeepClone() },
                        "native death/owned-marker expiry fades the original material and despawns; no renderer/material/visibility write");
                    CheckSprint17Final(key + "-lifecycle-resources", resources.Count >= 5 && resources.All(value => value == null),
                        new JObject { ["captured"] = resources.Count, ["remaining"] = resources.Count(value => value != null) },
                        "all exact per-instance meshes/materials/controller clones/texture/components reclaimed by native teardown");
                }
                finally
                {
                    if (subscribed) EventBus.Unsubscribe(observer);
                    InterruptExpandedSummoningFixtureCommands(attacker);
                    attacker.Descriptor.Stats.AdditionalAttackBonus.BaseValue = bonus;
                    owner.Descriptor.SwitchFactions(originalFaction, false);
                    owner.AttackFactions.Match(originalAttackFactions);
                    owner.GroupId = originalGroup;
                    // Emergency/final fixture teardown cannot satisfy earlier
                    // lifecycle assertions; record them before this cleanup.
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, attacker });
                }
                for (int frame = 0; frame < 5; frame++) yield return 0;
            }
        }

        private static JObject Sprint17LifecycleIsolation(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData attacker)
        {
            bool owned = !ReferenceEquals(owner, attacker) && fixture.Created.Contains(owner) &&
                fixture.Created.Contains(attacker);
            bool player = owner.IsPlayerFaction || attacker.IsPlayerFaction;
            bool party = owner.Group.IsPlayerParty || attacker.Group.IsPlayerParty;
            int foreign = fixture.UnitsBefore.OfType<UnitEntityData>().Count(value =>
                value.IsEnemy(owner) || owner.IsEnemy(value) || value.IsEnemy(attacker) || attacker.IsEnemy(value));
            bool a = owner.IsEnemy(attacker), b = attacker.IsEnemy(owner);
            return new JObject { ["ownedDistinct"] = owned, ["eitherPlayerFaction"] = player,
                ["eitherPartyGroup"] = party, ["ownerEnemy"] = a, ["attackerEnemy"] = b,
                ["foreignEnemyRelations"] = foreign,
                ["passed"] = SerpentineFinalReviewPolicy.IsolatedLifecyclePair(owned, player, party, a, b, foreign) };
        }

        private static JObject Sprint17NativeHitFacing(UnitEntityData owner, UnitEntityData attacker, bool hit)
        {
            Vector3 direction = (attacker.View.CenterTorso.position - owner.View.CenterTorso.position).normalized;
            float dot = Vector3.Dot(direction, owner.View.transform.forward);
            return new JObject { ["frame"] = Time.frameCount, ["attackHit"] = hit,
                ["torsoFacingDot"] = dot, ["nativeThreshold"] = .3d,
                ["eligible"] = SerpentineFinalReviewPolicy.NativeFrontalHit(hit, dot),
                ["nativeHitActionPresent"] = owner.View.AnimationManager.GetAction(UnitAnimationType.Hit) != null };
        }

        private static void ObserveSprint17LifecyclePose(UnitEntityData unit,
            Kingmaker.Visual.Animation.AnimationSet expectedSet, string phase, JArray samples,
            ref bool hitPlayed, ref bool deathPlayed, ref bool deadObserved,
            ref bool finite, ref float largestDissolve, List<UnityEngine.Object> resources)
        {
            deadObserved |= unit.Descriptor.State.IsDead;
            if (unit.View == null) return;
            var attachment = unit.View.GetComponent<SerpentineVisualAttachment>();
            if (attachment == null || attachment.Body == null || attachment.Body.sharedMesh == null) return;
            foreach (var resource in attachment.CaptureOwnedResources())
                if (!resources.Any(value => ReferenceEquals(value, resource))) resources.Add(resource);
            var manager = unit.View.AnimationManager;
            finite &= ReferenceEquals(manager.AnimationSet, expectedSet);
            foreach (var material in attachment.Body.sharedMaterials)
                if (material != null && material.HasProperty("_Dissolve"))
                    largestDissolve = Math.Max(largestDissolve, material.GetFloat("_Dissolve"));
            var played = new JArray();
            foreach (var handle in manager.ActiveActions.OfType<UnitAnimationActionHandle>())
            {
                var active = handle.ActiveAnimation;
                var clip = active == null ? null : active.GetPlayableClip();
                bool isHit = phase == "hit" && ReferenceEquals(handle.Action, manager.GetAction(UnitAnimationType.Hit));
                bool isDeath = phase != "hit" && manager.IsDead &&
                    ReferenceEquals(handle.Action, manager.GetAction(UnitAnimationType.Prone));
                bool exact = SerpentineFinalReviewPolicy.PlayedNativeClip(isHit || isDeath, handle.IsStarted,
                    clip == null ? null : clip.name, clip == null ? float.NaN : clip.length,
                    active == null ? double.NaN : active.GetTime(), active == null ? float.NaN : active.GetWeight());
                if (!exact) continue;
                hitPlayed |= isHit; deathPlayed |= isDeath;
                played.Add(new JObject { ["action"] = handle.Action.name, ["clip"] = clip.name,
                    ["time"] = active.GetTime(), ["duration"] = clip.length, ["weight"] = active.GetWeight(),
                    ["nativeHit"] = isHit, ["nativeDeath"] = isDeath });
            }
            if (played.Count > 0 || Time.frameCount % 30 == 0)
            {
                var pose = Sprint17OriginalBodySample(unit, attachment.Body);
                finite &= (bool)pose["finite"] && (bool)pose["poseFinite"];
                if (samples.Count(value => (string)value["phase"] == phase) < 10)
                    samples.Add(new JObject { ["phase"] = phase, ["frame"] = Time.frameCount,
                        ["dead"] = unit.Descriptor.State.IsDead, ["managerDead"] = manager.IsDead,
                        ["maxDissolve"] = largestDissolve, ["played"] = played, ["pose"] = pose });
            }
        }
    }
}
