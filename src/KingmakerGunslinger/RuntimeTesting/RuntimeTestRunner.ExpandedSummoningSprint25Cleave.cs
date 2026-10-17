using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
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

        private IEnumerable<int> Sprint25Cleave(ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            Vector3 casterBefore = fixture.Caster.Position;
            UnityEngine.Random.State randomBefore = UnityEngine.Random.state;
            try
            {
                PlaceExpandedSummoningUnit(fixture.Caster, Sprint25IsolatedAnchor(fixture));
                foreach (bool missSecond in new[] { false, true })
                {
                    var created = new List<UnitEntityData>();
                    UnitUseAbility command = null;
                    var observer = new Sprint25CleaveObserver { MissSecond = missSecond };
                    try
                    {
                        UnitEntityData owner = CastExpandedSummoningQuietUnit(fixture, "brachiosaurus");
                        created.Add(owner); observer.Owner = owner;
                        UnitEntityData[] targets = Enumerable.Range(0, 3).Select(index =>
                            CastExpandedSummoningQuietUnit(fixture, "wolf", fixture.Hostile)).ToArray();
                        created.AddRange(targets);
                        UnitEntityData ally = CastExpandedSummoningQuietUnit(fixture, "wolf");
                        UnitEntityData remote = CastExpandedSummoningQuietUnit(fixture, "wolf", fixture.Hostile);
                        created.AddRange(new[] { ally, remote });
                        foreach (int frame in Sprint25NativeAppearance(owner, "brachiosaurus", false)) yield return frame;
                        var ground = AstarPath.active.GetNearest(fixture.Caster.Position);
                        if (ground.node == null || !ground.node.Walkable)
                            throw new InvalidOperationException("No native floor for the isolated Cleave fixture.");
                        Vector3 origin = ground.clampedPosition;
                        PlaceExpandedSummoningUnit(owner, origin);
                        for (int index = 0; index < targets.Length; index++)
                            PlaceExpandedSummoningUnit(targets[index], origin + Vector3.forward * 5.5f +
                                Vector3.right * (index - 1) * 1.2f);
                        PlaceExpandedSummoningUnit(ally, origin + Vector3.forward * 4.3f);
                        PlaceExpandedSummoningUnit(remote, origin + Vector3.forward * 30);
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
                        EventBus.Subscribe(observer);
                        float standardBefore = owner.CombatState.Cooldown.StandardAction;
                        float maxStandard = standardBefore;
                        command = new UnitUseAbility(ability, new TargetWrapper(targets[0]));
                        if (!command.CanStart) throw new InvalidOperationException("Native Cleave command cannot start.");
                        owner.Commands.Run(command);
                        var controller = new Kingmaker.Controllers.Units.UnitActionController();
                        for (int frame = 0; frame < 700; frame++)
                        {
                            if (!command.IsFinished && (!command.IsStarted || command.ExecutionProcess == null))
                            {
                                if (command.Animation != null && frame > 4) command.Animation.IsActed = true;
                                ElementalBreathScenario.TickCommand(controller, command);
                            }
                            if (command.ExecutionProcess != null) TickExpandedSummoningDetachedAbility(command, frame);
                            maxStandard = Math.Max(maxStandard, owner.CombatState.Cooldown.StandardAction);
                            yield return 0;
                            if (command.IsFinished) break;
                        }
                        int expectedCount = missSecond ? 2 : 3;
                        bool exact = greaterPath && command.IsStarted && command.IsFinished &&
                            !command.Cutscene && !command.IsIgnoreCooldown &&
                            blueprint.ActionType.ToString() == "Standard" &&
                            !ability.RequireFullRoundAction && maxStandard > standardBefore &&
                            observer.Attacks.Count == expectedCount &&
                            observer.Attacks.Select(value => value.Target.UniqueId).Distinct().Count() == expectedCount &&
                            observer.Attacks.All(value => targets.Contains(value.Target)) &&
                            observer.Attacks[0].AttackRoll.IsHit &&
                            (missSecond ? !observer.Attacks[1].AttackRoll.IsHit : observer.Attacks.All(value => value.AttackRoll.IsHit));
                        Sprint25Check("great-cleave-" + (missSecond ? "miss-termination" : "legal-chain") + (turnBased ? "-turn-based" : "-rtwp"), exact,
                            new JObject { ["unit"] = owner.UniqueId, ["ability"] = blueprint.AssetGuid,
                                ["nativeGreaterFeature"] = custom.GreaterFeature.AssetGuid,
                                ["greaterPath"] = greaterPath,
                                ["mode"] = turnBased ? "turn-based" : "rtwp",
                                ["commandFinished"] = command.IsFinished,
                                ["commandResult"] = command.Result.ToString(),
                                ["standardCooldown"] = new JArray(standardBefore, maxStandard),
                                ["alliesExcluded"] = ally.UniqueId, ["outOfReachExcluded"] = remote.UniqueId,
                                ["nativeTargetSelection"] = "Living remembered enemies within native IsReach, sorted from the first target; no native inter-target adjacency predicate. The positive fixture uses three adjacent foes. No global Cleave repair.",
                                ["legalTargets"] = new JArray(targets.Select(value => new JObject {
                                    ["unit"] = value.UniqueId, ["ownerDistance"] = owner.DistanceTo(value),
                                    ["position"] = SurveyVector(value.Position) })),
                                ["attacks"] = new JArray(observer.Attacks.Select(value => new JObject {
                                    ["target"] = value.Target.UniqueId, ["weapon"] = value.Weapon.Blueprint.AssetGuid,
                                    ["hit"] = value.AttackRoll?.IsHit, ["roll"] = value.AttackRoll?.Roll.Value })) },
                            "The existing native greater path executes an adjacent three-target chain, terminates on the first miss, excludes allies and out-of-reach enemies, and spends one Standard action.");
                    }
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
