using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static void Sprint28SetBrainActivePreservingActions(
            UnitEntityData unit, bool active)
        {
            // The quiet-unit helper clears its action lists when disabling
            // AI. This fixture suspends only its disposable owner during
            // native turn admission, then restores the original lists.
            PropertyInfo property = typeof(UnitEntityData).GetProperty(
                "IsBrainActive", BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo setter = property?.GetSetMethod(true);
            if (setter != null) setter.Invoke(unit, new object[] { active });
            else
            {
                FieldInfo backing = typeof(UnitEntityData).GetField(
                    "<IsBrainActive>k__BackingField",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (backing == null) throw new MissingFieldException(
                    "UnitEntityData.IsBrainActive backing field");
                backing.SetValue(unit, active);
            }
            if (unit.IsBrainActive != active)
                throw new InvalidOperationException(
                    "Disposable giant AI brain state did not change.");
        }

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
                    if (turnBased)
                    {
                        int actionsBefore = owner.Brain?.Actions?.Count ?? 0;
                        int availableBefore = owner.Brain?.AvailableActions?.Count ?? 0;
                        Sprint28SetBrainActivePreservingActions(owner, false);
                        Sprint25Check("s28-" + key +
                            (set == 2 ? "-rock" : "-weapon") +
                            "-ai-brain-suspended", !owner.IsBrainActive &&
                            (owner.Brain?.Actions?.Count ?? 0) == actionsBefore &&
                            (owner.Brain?.AvailableActions?.Count ?? 0) == availableBefore,
                            new JObject { ["owner"] = owner.UniqueId,
                                ["actionsBefore"] = actionsBefore,
                                ["actionsAfter"] = owner.Brain?.Actions?.Count ?? 0,
                                ["availableBefore"] = availableBefore,
                                ["availableAfter"] =
                                    owner.Brain?.AvailableActions?.Count ?? 0 },
                            "Only the disposable giant brain pauses during native turn admission; its action lists remain intact.");
                    }
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
                        // The proven native turn command fixture places its
                        // melee target adjacent to the surveyed owner lane.
                        // The wider AI melee separation enlisted four
                        // protected first-area neutrals into turn order.
                        // RTWP still observes the real approach; turn-based
                        // AI needs its own selected attack, not that travel.
                        Vector3 forward = (far - origin).normalized;
                        float adjacent = owner.Corpulence +
                            target.Corpulence + .8f;
                        PlaceExpandedSummoningUnit(target,
                            turnBased && set != 2
                                ? origin + forward * adjacent : far);
                        // The proven turn-command fixture enrolls only the
                        // disposable combatants without global wake/memory
                        // ticks. Keep those AI-only setup steps in RTWP while
                        // turn-based admission stays request-owned.
                        if (!turnBased)
                        {
                            foreach (UnitEntityData unit in new[] { owner, target })
                            {
                                unit.Wake(0f);
                                if (!Game.Instance.State.AwakeUnits.Contains(unit))
                                    Game.Instance.State.AwakeUnits.Add(unit);
                            }
                        }
                        owner.Memory.Add(target);
                        target.Memory.Add(owner);
                        owner.SightCache.ClearIfNeeded();
                        target.SightCache.ClearIfNeeded();
                        if (!turnBased)
                            new Kingmaker.Controllers.Units.UnitMemoryController()
                                .Tick();
                        else if (owner.HasLOS(target))
                        {
                            // Native geometry confirms sight. Mark only this
                            // disposable owner's exact target as noticed;
                            // no global memory tick or protected NPC changes.
                            owner.Memory.Add(target).Visible = true;
                        }
                        owner.JoinCombat();
                        target.JoinCombat();
                        // The proven turn command fixture lets the native
                        // enrollment helper perform combat preparation.
                        // An earlier extra Join/Prepare tick here admitted
                        // four actors outside the request-local clock.
                        if (!turnBased)
                        {
                            Game.Instance.Player.UpdateIsInCombat();
                            new UnitCombatJoinController().Tick();
                            new UnitCombatPrepareController().Tick();
                        }
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
                        if (turnBased)
                            Sprint28SetBrainActivePreservingActions(owner, true);
                        else SetExpandedSummoningBrainActive(owner, true);
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
                        UnitAttack firstAiAttack = null;
                        int expectedCadence = set == 2 ?
                            key == "stone-giant" ? 2 : 1 :
                            key == "fire-giant" ? 3 : 2;
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
                                if (turnBased && firstAiAttack == null)
                                    firstAiAttack = commands.OfType<UnitAttack>()
                                        .FirstOrDefault(value =>
                                            value.AiAction != null &&
                                            ReferenceEquals(value.TargetUnit, target));
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
                                if (turnBased ? firstAiAttack != null &&
                                    firstAiAttack.IsFinished :
                                    observer.WeaponAttacks.Count > 0) break;
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
                                (turnBased || set == 2 || travel > .1f);
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
                            if (turnBased)
                            {
                                bool cadence = firstAiAttack != null &&
                                    firstAiAttack.IsFinished &&
                                    firstAiAttack.IsFullAttack &&
                                    firstAiAttack.AllAttacks.Count ==
                                        expectedCadence &&
                                    observer.OwnerWeaponAttacks.Count ==
                                        expectedCadence &&
                                    observer.WeaponAttacks.Count ==
                                        expectedCadence &&
                                    observer.Rolls.Count ==
                                        expectedCadence &&
                                    Sprint18OwnerIsActing(owner) &&
                                    commands.OfType<UnitAttack>().Count(
                                        value => value.AiAction != null &&
                                            ReferenceEquals(value.TargetUnit,
                                                target)) == 1;
                                Sprint25Check(label + "-ai-full-cadence",
                                    cadence, new JObject {
                                        ["owner"] = owner.UniqueId,
                                        ["target"] = target.UniqueId,
                                        ["weapon"] = selected.Blueprint.AssetGuid,
                                        ["expected"] = expectedCadence,
                                        ["firstCommand"] = firstAiAttack == null ?
                                            null : Sprint27AiCommand(firstAiAttack),
                                        ["isFullAttack"] = firstAiAttack?.IsFullAttack,
                                        ["planned"] = firstAiAttack?.AllAttacks.Count,
                                        ["ownerAttacks"] =
                                            observer.OwnerWeaponAttacks.Count,
                                        ["targetAttacks"] =
                                            observer.WeaponAttacks.Count,
                                        ["rolls"] = observer.Rolls.Count,
                                        ["sameTurn"] = Sprint18OwnerIsActing(owner),
                                        ["aiAttacks"] = new JArray(commands
                                            .OfType<UnitAttack>()
                                            .Where(value => value.AiAction != null)
                                            .Select(Sprint27AiCommand)),
                                        ["fixtureAttackCommands"] = 0,
                                        ["forcedRolls"] = 0
                                    }, "One native AI-selected attack command completes its printed weapon or rock cadence on the original active turn.");
                            }
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
