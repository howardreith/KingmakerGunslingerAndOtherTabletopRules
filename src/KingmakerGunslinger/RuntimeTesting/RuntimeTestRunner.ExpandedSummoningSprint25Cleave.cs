using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.UI.SettingsUI;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Sprint25CleaveObserver :
            IGlobalRulebookHandler<RuleAttackRoll>,
            IGlobalRulebookHandler<RuleAttackWithWeapon>
        {
            internal UnitEntityData Owner;
            internal bool MissSecond;
            internal int RollCount;
            internal readonly List<RuleAttackWithWeapon> Attacks = new List<RuleAttackWithWeapon>();
            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (!ReferenceEquals(evt.Initiator, Owner)) return;
                RollCount++;
                UnityEngine.Random.InitState(FindNativeD20Seed(MissSecond && RollCount == 2 ? 1 : 18));
            }
            public void OnEventDidTrigger(RuleAttackRoll evt) { }
            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            { if (ReferenceEquals(evt.Initiator, Owner)) Attacks.Add(evt); }
        }

        private UnitEntityData Sprint25CleaveTarget(ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> created, bool hostile)
        {
            UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(
                hostile ? fixture.HostileBlueprint : fixture.CasterBlueprint,
                fixture.Caster.Position, Quaternion.identity, fixture.Scene);
            if (unit == null) throw new InvalidOperationException("Native Cleave target creation failed.");
            fixture.Created.Add(unit); created.Add(unit);
            Game.Instance.EntityCreator.Tick();
            if (!unit.IsInState || unit.View == null)
                throw new InvalidOperationException("Native Cleave target has no loaded-area view.");
            SetExpandedSummoningBrainActive(unit, false);
            return unit;
        }

        private IEnumerable<int> Sprint25Cleave(ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            Vector3 casterBefore = fixture.Caster.Position;
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            try
            {
                foreach (bool missSecond in new[] { false, true })
                {
                    var created = new List<UnitEntityData>();
                    UnitUseAbility command = null;
                    FirearmReviewCosts costs = null;
                    var costRows = new JArray();
                    Vector3 hostileBefore = fixture.Hostile.Position;
                    var observer = new Sprint25CleaveObserver { MissSecond = missSecond };
                    try
                    {
                        UnitEntityData owner = CastExpandedSummoningQuietUnit(fixture, "brachiosaurus");
                        created.Add(owner); observer.Owner = owner;
                        // Reuse the fixture's qualified native humanoid
                        // blueprints, not a wolf footprint assumed to fit a
                        // humanoid-sized adjacent cluster.
                        UnitEntityData[] targets = Enumerable.Range(0, 3).Select(index =>
                            Sprint25CleaveTarget(fixture, created, true)).ToArray();
                        UnitEntityData ally = Sprint25CleaveTarget(fixture, created, false);
                        UnitEntityData remote = Sprint25CleaveTarget(fixture, created, true);
                        foreach (int frame in Sprint25NativeAppearance(owner, "brachiosaurus")) yield return frame;
                        Vector3 far; string lane, laneDirection;
                        Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, targets[0],
                            5.5f, out far, out lane, out laneDirection);
                        var ground = AstarPath.active.GetNearest(origin);
                        if (ground.node == null || !ground.node.Walkable)
                            throw new InvalidOperationException("No native floor for the isolated Cleave fixture.");
                        origin = ground.clampedPosition;
                        Vector3 forward = Vector3.zero;
                        Vector3[] legalTargets = null;
                        PlaceExpandedSummoningUnit(owner, origin);
                        foreach (Vector3 direction in new[] { (far - origin).normalized }.Concat(CompassOffsets))
                        {
                            Vector3 side = Vector3.Cross(direction, Vector3.up).normalized;
                            Vector3[] requested = Enumerable.Range(0, 3).Select(index =>
                                origin + direction * 5.5f + side * (index - 1) * 1.2f).ToArray();
                            var nodes = requested.Select(point => AstarPath.active.GetNearest(point)).ToArray();
                            if (nodes.Where((node, index) => node.node != null && node.node.Walkable &&
                                node.node.Area == ground.node.Area && node.node.GraphIndex == ground.node.GraphIndex &&
                                Vector3.Distance(node.clampedPosition, requested[index]) < .6f &&
                                !Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(
                                    origin + Vector3.up * 1.2f, node.clampedPosition + Vector3.up * 1.2f, 0)).Count() != 3) continue;
                            for (int index = 0; index < targets.Length; index++)
                                PlaceExpandedSummoningUnit(targets[index], nodes[index].clampedPosition);
                            owner.SightCache.ClearIfNeeded();
                            if (targets.Any(value => !UnitEngagementExtension.IsReach(owner, value,
                                owner.Body.PrimaryHand))) continue;
                            legalTargets = nodes.Select(node => node.clampedPosition).ToArray();
                            forward = direction;
                            break;
                        }
                        if (legalTargets == null)
                            throw new InvalidOperationException("No walkable, clear-sight adjacent three-target Cleave cluster: " + lane);
                        PlaceExpandedSummoningUnit(owner, origin);
                        for (int index = 0; index < targets.Length; index++)
                            PlaceExpandedSummoningUnit(targets[index], legalTargets[index]);
                        bool adjacentCluster = true;
                        var geometry = new JArray();
                        for (int first = 0; first < targets.Length; first++)
                            for (int second = first + 1; second < targets.Length; second++)
                            {
                                float radii = targets[first].Corpulence + targets[second].Corpulence;
                                float distance = targets[first].DistanceTo(targets[second]);
                                adjacentCluster &= distance >= radii - .05f && distance <= radii + 5 * .3048f + .02f;
                                geometry.Add(new JObject { ["first"] = targets[first].UniqueId,
                                    ["second"] = targets[second].UniqueId, ["radii"] = radii,
                                    ["nativeCenterDistance"] = distance });
                            }
                        Sprint25Check("great-cleave-target-geometry-" + missSecond + "-" + turnBased,
                            adjacentCluster, new JObject { ["pairs"] = geometry,
                                ["targets"] = new JArray(targets.Select(value => new JObject {
                                    ["unit"] = value.UniqueId, ["blueprint"] = value.Blueprint.AssetGuid,
                                    ["corpulence"] = value.Corpulence, ["position"] = SurveyVector(value.Position) })) },
                            "Actual native humanoid footprints are disjoint and within five feet edge-to-edge before Cleave.");
                        if (!adjacentCluster) throw new InvalidOperationException("Cleave actors are not natively disjoint/adjacent: " + geometry);
                        PlaceExpandedSummoningUnit(ally, origin + forward * 4.3f);
                        PlaceExpandedSummoningUnit(remote, origin + forward * 30);
                        // Only this request-owned extra enemy is parked. The
                        // native Cleave memory/reach filter must still exclude
                        // the in-reach ally and the remembered remote enemy.
                        PlaceExpandedSummoningUnit(fixture.Hostile, origin - forward * 30);
                        foreach (UnitEntityData unit in created)
                        {
                            unit.Stats.HitPoints.BaseValue = 100000;
                            unit.Descriptor.Damage = 0;
                            unit.CombatState.OnNewRound();
                            owner.Memory.Add(unit); unit.Memory.Add(owner);
                            if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                        }
                        owner.Stats.AdditionalAttackBonus.BaseValue = 100;
                        owner.JoinCombat(); targets[0].JoinCombat();
                        Game.Instance.Player.UpdateIsInCombat();
                        BlueprintAbility blueprint = fixture.Blueprints.OfType<BlueprintAbility>().Single(
                            value => value.AssetGuid == "6447d104a2222c14d9c9b8a36e4eb242");
                        Ability granted = owner.Descriptor.Abilities.GetAbility(blueprint);
                        if (granted == null) throw new InvalidOperationException("Brachiosaurus lacks its real native CleaveAction.");
                        var ability = new AbilityData(granted);
                        if (turnBased)
                        {
                            var admission = new JObject();
                            foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, targets[0], admission)) yield return frame;
                            int waited = 0;
                            while (!Sprint18OwnerIsActing(owner) && waited++ < 500) yield return 0;
                            if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner))
                                throw new InvalidOperationException("Cleave owner did not reach its native turn: " + admission);
                        }
                        var custom = blueprint.GetComponents<Kingmaker.UnitLogic.Abilities.Components.AbilityCustomCleave>().Single();
                        bool greaterPath = owner.Descriptor.Progression.Features.HasFact(custom.GreaterFeature);
                        // The native delivery enumerates remembered, living,
                        // in-game enemies through IsReach (including LOS),
                        // not merely the clear geometry used by the survey.
                        foreach (UnitEntityData target in targets.Concat(new[] { remote }))
                            owner.Memory.Add(target);
                        var nativeCandidates = new JArray(targets.Select(value => new JObject {
                            ["unit"] = value.UniqueId, ["enemy"] = owner.IsEnemy(value),
                            ["inGame"] = value.IsInGame,
                            ["rememberedEnemy"] = owner.Memory.Enemies.Any(info => ReferenceEquals(info.Unit, value)),
                            ["nativeReachAndLos"] = UnitEngagementExtension.IsReach(owner, value, owner.Body.PrimaryHand) }));
                        bool candidatesReady = nativeCandidates.OfType<JObject>().All(value =>
                            (bool)value["enemy"] && (bool)value["inGame"] &&
                            (bool)value["rememberedEnemy"] && (bool)value["nativeReachAndLos"]);
                        Sprint25Check("great-cleave-native-candidates-" + missSecond + "-" + turnBased,
                            candidatesReady, new JObject { ["candidates"] = nativeCandidates },
                            "All three legal targets satisfy the actual native Cleave memory, life and reach/LOS filter before dispatch.");
                        if (!candidatesReady) throw new InvalidOperationException("Native Cleave target prerequisites unmet: " + nativeCandidates);
                        EventBus.Subscribe(observer);
                        costs = new FirearmReviewCosts(owner, costRows);
                        var remoteRange = new UnitAttack(remote);
                        remoteRange.Init(owner);
                        bool remoteOutsideReach = !remoteRange.IsUnitEnoughClose;
                        bool allyIsEnemy = owner.IsEnemy(ally);
                        float standardBefore = owner.CombatState.Cooldown.StandardAction;
                        float maxStandard = standardBefore;
                        bool combatThroughout = owner.IsInCombat;
                        command = new UnitUseAbility(ability, new TargetWrapper(targets[0]));
                        if (!command.CanStart) throw new InvalidOperationException("Native Cleave command cannot start.");
                        owner.Commands.Run(command);
                        var controller = new Kingmaker.Controllers.Units.UnitActionController();
                        bool executionSeen = false;
                        int commandFinishedFrame = -1, deliveryEndedFrame = -1;
                        for (int frame = 0; frame < 700; frame++)
                        {
                            if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                            if (!command.IsFinished && (!command.IsStarted || command.ExecutionProcess == null))
                            {
                                if (command.Animation != null && frame > 4) command.Animation.IsActed = true;
                                ElementalBreathScenario.TickCommand(controller, command);
                            }
                            executionSeen |= command.ExecutionProcess != null;
                            if (command.ExecutionProcess != null) TickExpandedSummoningDetachedAbility(command, frame, true);
                            if (command.IsFinished && commandFinishedFrame < 0) commandFinishedFrame = frame;
                            if (command.ExecutionProcess != null && command.ExecutionProcess.IsEnded && deliveryEndedFrame < 0)
                                deliveryEndedFrame = frame;
                            maxStandard = Math.Max(maxStandard, owner.CombatState.Cooldown.StandardAction);
                            combatThroughout &= owner.IsInCombat;
                            yield return 0;
                            combatThroughout &= owner.IsInCombat;
                            if (command.IsFinished && commandFinishedFrame < 0) commandFinishedFrame = frame;
                            if (command.ExecutionProcess != null && command.ExecutionProcess.IsEnded && deliveryEndedFrame < 0)
                                deliveryEndedFrame = frame;
                            if (command.IsFinished && command.ExecutionProcess != null && command.ExecutionProcess.IsEnded) break;
                        }
                        int expectedCount = missSecond ? 2 : 3;
                        bool exact = adjacentCluster && candidatesReady && greaterPath && command.IsStarted && command.IsFinished &&
                            executionSeen && command.ExecutionProcess != null && command.ExecutionProcess.IsEnded &&
                            !command.Cutscene && !command.IsIgnoreCooldown &&
                            blueprint.ActionType.ToString() == "Standard" &&
                            !ability.RequireFullRoundAction && combatThroughout &&
                            costs.Count(command) == 1 && costs.Exact(command, turnBased) &&
                            !allyIsEnemy && remoteOutsideReach &&
                            observer.Attacks.Count == expectedCount &&
                            observer.Attacks.Select(value => value.Target.UniqueId).Distinct().Count() == expectedCount &&
                            observer.Attacks.All(value => targets.Contains(value.Target)) &&
                            observer.Attacks[0].AttackRoll.IsHit &&
                            (missSecond ? !observer.Attacks[1].AttackRoll.IsHit : observer.Attacks.All(value => value.AttackRoll.IsHit));
                        Sprint25Check("great-cleave-" + (missSecond ? "miss-termination" : "legal-chain") + (turnBased ? "-turn-based" : "-rtwp"), exact,
                            new JObject { ["unit"] = owner.UniqueId, ["ability"] = blueprint.AssetGuid,
                                ["nativeGreaterFeature"] = custom.GreaterFeature.AssetGuid,
                                ["greaterPath"] = greaterPath,
                                ["nativeDisjointAdjacentCluster"] = adjacentCluster,
                                ["mode"] = turnBased ? "turn-based" : "rtwp",
                                ["commandFinished"] = command.IsFinished,
                                ["commandFinishedFrame"] = commandFinishedFrame,
                                ["nativeDeliveryEndedFrame"] = deliveryEndedFrame,
                                ["nativeDeliveryEnded"] = command.ExecutionProcess?.IsEnded,
                                ["commandResult"] = command.Result.ToString(),
                                ["standardCooldown"] = new JArray(standardBefore, maxStandard),
                                ["combatThroughout"] = combatThroughout,
                                ["nativeCostApplications"] = costs.Count(command),
                                ["nativeCostExact"] = costs.Exact(command, turnBased),
                                ["nativeCostBoundary"] = costRows,
                                ["nativeClusterSurvey"] = "Same walkable native graph/area, clear native sight geometry, adjacent cluster before turn enrollment.",
                                ["legalLaneSurvey"] = lane,
                                ["allyIsEnemy"] = allyIsEnemy, ["remoteOutsideNativeReach"] = remoteOutsideReach,
                                ["remoteDistance"] = owner.DistanceTo(remote),
                                ["alliesExcluded"] = ally.UniqueId, ["outOfReachExcluded"] = remote.UniqueId,
                                ["nativeTargetSelection"] = "Living remembered enemies within native IsReach, sorted from the first target; no native inter-target adjacency predicate. The positive fixture uses three adjacent foes. No global Cleave repair.",
                                ["legalTargets"] = new JArray(targets.Select(value => new JObject {
                                    ["unit"] = value.UniqueId, ["ownerDistance"] = owner.DistanceTo(value),
                                    ["corpulence"] = value.Corpulence,
                                    ["position"] = SurveyVector(value.Position) })),
                                ["attacks"] = new JArray(observer.Attacks.Select(value => new JObject {
                                    ["target"] = value.Target.UniqueId, ["weapon"] = value.Weapon.Blueprint.AssetGuid,
                                    ["hit"] = value.AttackRoll?.IsHit, ["roll"] = value.AttackRoll?.Roll.Value })) },
                            "The existing native greater path executes an adjacent three-target chain, terminates on the first miss, excludes allies and out-of-reach enemies, and spends one Standard action.");
                    }
                    finally
                    {
                        try { costs?.Dispose(); }
                        finally
                        {
                            EventBus.Unsubscribe(observer);
                            EndExpandedSummoningDetachedAbility(command);
                            SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                            Game.Instance.TurnBasedCombatController.Activate();
                            foreach (UnitEntityData unit in created)
                            {
                                unit.Commands.InterruptAll(true);
                                if (unit.IsInCombat) unit.LeaveCombat();
                            }
                            Game.Instance.Player.UpdateIsInCombat();
                            DisposeExpandedSummoningUnits(fixture.Created, created);
                            PlaceExpandedSummoningUnit(fixture.Hostile, hostileBefore);
                        }
                    }
                }
            }
            finally
            {
                PlaceExpandedSummoningUnit(fixture.Caster, casterBefore);
                UnityEngine.Random.state = randomBefore;
            }
        }
    }
}
