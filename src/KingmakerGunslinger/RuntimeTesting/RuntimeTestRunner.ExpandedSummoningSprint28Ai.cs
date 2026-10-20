using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The native brain owns every attack. This fixture only selects an
        // owned hand set, legal lane and exact disposable hostile target.
        private IEnumerable<int> ReviewSprint28Ai(
            ExpandedSummoningCorrectionFixture fixture, bool turnBased)
        {
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (string key in new[] { "hill-giant", "stone-giant",
                    "fire-giant" })
                foreach (int set in new[] { 0, 2 })
                {
                    ResetExpandedSummoningHostile(fixture);
                    UnitEntityData owner = CastExpandedSummoningOwnTier(
                        fixture, key);
                    UnitEntityData target = fixture.Hostile;
                    // Preserve the summoned brain's native action lists.
                    // The quiet-unit helper clears them irreversibly.
                    try
                    {
                        owner.Commands.InterruptAll(true);
                        target.Commands.InterruptAll(true);
                        owner.Commands.RemoveFinishedAndUpdateQueue();
                        target.Commands.RemoveFinishedAndUpdateQueue();
                        if (set == 2)
                        {
                            var switchCommand =
                                new UnitSwitchHandEquipmentSet(set);
                            owner.Commands.Run(switchCommand);
                            DateTime switchDeadline =
                                DateTime.UtcNow.AddSeconds(20);
                            for (int frame = 0; frame < 1000 &&
                                !switchCommand.IsFinished &&
                                DateTime.UtcNow < switchDeadline; frame++)
                                yield return 0;
                            if (!switchCommand.IsFinished ||
                                owner.Body.CurrentHandEquipmentSetIndex != set)
                                throw new InvalidOperationException(
                                    "Giant AI native rock hand switch failed.");
                        }
                        ItemEntityWeapon selected =
                            owner.Body.PrimaryHand.MaybeWeapon;
                        Vector3 far; string lane, direction;
                        Vector3 origin = Sprint24AiCombatLane(fixture.Caster,
                            owner, target, set == 2 ? 8f : 5f,
                            out far, out lane, out direction);
                        PlaceExpandedSummoningUnit(owner, origin);
                        PlaceExpandedSummoningUnit(target, far);
                        foreach (UnitEntityData unit in new[] { owner, target })
                        {
                            unit.Wake(0f);
                            if (!Game.Instance.State.AwakeUnits.Contains(unit))
                                Game.Instance.State.AwakeUnits.Add(unit);
                        }
                        owner.Memory.Add(target);
                        target.Memory.Add(owner);
                        owner.SightCache.ClearIfNeeded();
                        target.SightCache.ClearIfNeeded();
                        new Kingmaker.Controllers.Units.UnitMemoryController()
                            .Tick();
                        owner.JoinCombat();
                        target.JoinCombat();
                        Game.Instance.Player.UpdateIsInCombat();
                        new UnitCombatJoinController().Tick();
                        new UnitCombatPrepareController().Tick();
                        JObject admission = null;
                        if (turnBased)
                        {
                            admission = new JObject();
                            foreach (int frame in Sprint22EnrollTurnBasedOwners(
                                new[] { owner }, target, admission)) yield return frame;
                            int waited = 0;
                            DateTime turnDeadline = DateTime.UtcNow.AddSeconds(30);
                            while (waited++ < 500 &&
                                DateTime.UtcNow < turnDeadline &&
                                !Sprint18OwnerIsActing(owner)) yield return 0;
                            bool acting = (bool)admission["admitted"] &&
                                owner.IsVisibleForPlayer &&
                                Sprint18OwnerIsActing(owner);
                            Sprint25Check("s28-" + key +
                                (set == 2 ? "-rock" : "-weapon") +
                                "-ai-turn-admission", acting,
                                new JObject { ["owner"] = owner.UniqueId,
                                    ["target"] = target.UniqueId,
                                    ["waitedFrames"] = waited,
                                    ["admission"] = admission,
                                    ["turn"] = Sprint22TurnState(owner, target) },
                                "The exact visible giant receives its native active turn before its own brain selects an attack.");
                            if (!acting) throw new InvalidOperationException(
                                "Giant AI native turn admission failed.");
                        }
                        SetExpandedSummoningBrainActive(owner, true);
                        owner.Brain.RestoreAvailableActions();
                        int actions = owner.Brain.Actions == null ? 0 :
                            owner.Brain.Actions.Count;
                        int available = owner.Brain.AvailableActions == null ?
                            0 : owner.Brain.AvailableActions.Count;
                        bool ready = selected != null && actions > 0 &&
                            available > 0 && owner.IsAIEnabled &&
                            owner.IsInCombat && owner.Descriptor.State.CanAct &&
                            owner.IsEnemy(target) && target.IsEnemy(owner) &&
                            owner.HasLOS(target) &&
                            owner.Memory.ContainsVisible(target);
                        string label = "s28-" + key +
                            (set == 2 ? "-rock" : "-weapon") +
                            (turnBased ? "-ai-turn" : "");
                        Sprint25Check(label + "-ai-ready", ready,
                            new JObject {
                                ["owner"] = owner.UniqueId,
                                ["target"] = target.UniqueId,
                                ["handSet"] = set,
                                ["weapon"] = selected?.Blueprint?.AssetGuid,
                                ["brain"] = owner.Blueprint.Brain?.name,
                                ["turnBased"] = turnBased,
                                ["admission"] = admission,
                                ["actions"] = actions,
                                ["available"] = available,
                                ["aiEnabled"] = owner.IsAIEnabled,
                                ["combat"] = owner.IsInCombat,
                                ["enemy"] = owner.IsEnemy(target),
                                ["visible"] = owner.Memory.ContainsVisible(target),
                                ["los"] = owner.HasLOS(target),
                                ["lane"] = lane,
                                ["distance"] = owner.DistanceTo(target)
                            }, "A genuinely visible legal exact-owner combat lane and native action list precede AI observation.");
                        if (!ready)
                            throw new InvalidOperationException(
                                "Giant native AI prerequisites missing.");
                        var observer = new Sprint24AttackObserver {
                            Owner = owner, Target = target };
                        var commands = new HashSet<UnitCommand>();
                        var ambient = new HashSet<string>();
                        var movement = new JArray();
                        float travel = 0f;
                        int ticks = 0;
                        int nativeTurnFrames = 0;
                        EventBus.Subscribe(observer);
                        try
                        {
                            DateTime deadline =
                                DateTime.UtcNow.AddSeconds(35);
                            for (int frame = 0; frame < 3000 &&
                                DateTime.UtcNow < deadline; frame++)
                            {
                                if (!turnBased && (frame == 0 || frame % 60 == 0 &&
                                    !owner.Commands.Raw.Any(value =>
                                        value != null && !value.IsFinished)))
                                {
                                    foreach (string id in
                                        Sprint24TickExactAi(owner))
                                        if (id != owner.UniqueId)
                                            ambient.Add(id);
                                    ticks++;
                                }
                                if (turnBased && Sprint18OwnerIsActing(owner))
                                    nativeTurnFrames++;
                                foreach (UnitCommand command in
                                    owner.Commands.Raw.Where(value =>
                                        value != null))
                                    commands.Add(command);
                                travel = Math.Max(travel,
                                    Vector3.Distance(origin, owner.Position));
                                if (frame % 60 == 0 ||
                                    observer.WeaponAttacks.Count > 0)
                                    movement.Add(new JObject {
                                        ["frame"] = Time.frameCount,
                                        ["position"] = SurveyVector(owner.Position),
                                        ["target"] = SurveyVector(target.Position),
                                        ["path"] = owner.View.AgentASP.Path != null,
                                        ["moving"] = owner.View.MovementAgent
                                            .IsReallyMoving,
                                        ["commands"] = new JArray(owner.Commands
                                            .Raw.Where(value => value != null)
                                            .Select(Sprint27AiCommand)) });
                                if (observer.WeaponAttacks.Count > 0) break;
                                Game.Instance.IsPaused = false;
                                yield return 0;
                            }
                            bool aiCommand = commands.OfType<UnitAttack>()
                                .Any(value => value.AiAction != null &&
                                    ReferenceEquals(value.TargetUnit, target));
                            bool exact = aiCommand &&
                                (turnBased ? nativeTurnFrames > 0 : ticks > 0) &&
                                observer.WeaponAttacks.Count > 0 &&
                                observer.WeaponAttacks.All(value =>
                                    ReferenceEquals(value.Weapon.Blueprint,
                                        selected.Blueprint)) &&
                                (set == 2 || travel > .1f);
                            Sprint25Check(label + "-ai-attack", exact,
                                new JObject {
                                    ["owner"] = owner.UniqueId,
                                    ["target"] = target.UniqueId,
                                    ["selectedWeapon"] =
                                        selected.Blueprint.AssetGuid,
                                    ["ticks"] = ticks,
                                    ["nativeTurnFrames"] = nativeTurnFrames,
                                    ["turnBased"] = turnBased,
                                    ["admission"] = admission,
                                    ["travel"] = travel,
                                    ["ambientCandidatesNeverTicked"] =
                                        new JArray(ambient),
                                    ["commands"] = new JArray(commands
                                        .Select(Sprint27AiCommand)),
                                    ["movement"] = movement,
                                    ["attacks"] = new JArray(observer
                                        .WeaponAttacks.Select(value =>
                                            new JObject {
                                                ["weapon"] = value.Weapon?
                                                    .Blueprint?.AssetGuid,
                                                ["natural"] = value.AttackRoll?
                                                    .Roll.Value,
                                                ["hit"] = value.AttackRoll?
                                                    .IsHit })),
                                    ["fixtureAttackCommands"] = 0,
                                    ["forcedRolls"] = 0
                                }, "The unchanged exact giant brain selects its native melee or owned-rock attack, and real movement/weapon events reach only the disposable target.");
                        }
                        finally { EventBus.Unsubscribe(observer); }
                    }
                    finally
                    {
                        SetExpandedSummoningBrainActive(owner, false);
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                        Game.Instance.TurnBasedCombatController.Activate();
                        owner.Commands.InterruptAll(true);
                        target.Commands.InterruptAll(true);
                        if (owner.IsInCombat) owner.LeaveCombat();
                        if (target.IsInCombat) target.LeaveCombat();
                        Game.Instance.Player.UpdateIsInCombat();
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
                        ResetExpandedSummoningHostile(fixture);
                    }
                }
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = mode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = pause;
            }
        }
    }
}
