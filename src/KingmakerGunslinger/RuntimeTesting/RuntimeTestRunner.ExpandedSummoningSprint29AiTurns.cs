using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The native turn controller owns the decision. This fixture admits
        // only the request-owned summon and disposable hostile, and never
        // issues a substitute attack or spell command.
        private IEnumerable<int> ReviewSprint29AiTurns(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (string key in new[] { "ettin", "cloud-giant",
                    "storm-giant" })
                {
                    ResetExpandedSummoningHostile(fixture);
                    UnitEntityData owner = CastExpandedSummoningOwnTier(
                        fixture, key);
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    UnitEntityData target = fixture.Hostile;
                    try
                    {
                        owner.Commands.InterruptAll(true);
                        target.Commands.InterruptAll(true);
                        owner.Commands.RemoveFinishedAndUpdateQueue();
                        target.Commands.RemoveFinishedAndUpdateQueue();
                        Vector3 far; string lane, direction;
                        Vector3 origin = Sprint24AiCombatLane(fixture.Caster,
                            owner, target, key == "ettin" ? 5f : 8f,
                            out far, out lane, out direction,
                            key == "cloud-giant" ?
                                new Feet(20).Meters : 0f);
                        Vector3 forward = (far - origin).normalized;
                        PlaceExpandedSummoningUnit(owner, origin);
                        PlaceExpandedSummoningUnit(target,
                            key == "ettin" ? origin + forward *
                                (owner.Corpulence + target.Corpulence + .8f) :
                                far);
                        owner.Memory.Add(target);
                        target.Memory.Add(owner);
                        owner.SightCache.ClearIfNeeded();
                        target.SightCache.ClearIfNeeded();
                        if (owner.HasLOS(target))
                            owner.Memory.Add(target).Visible = true;
                        owner.JoinCombat();
                        target.JoinCombat();
                        var admission = new JObject();
                        foreach (int frame in Sprint22EnrollTurnBasedOwners(
                            new[] { owner }, target, admission))
                            yield return frame;
                        int waited = 0;
                        DateTime turnDeadline = DateTime.UtcNow
                            .AddSeconds(30);
                        while (waited++ < 500 &&
                            DateTime.UtcNow < turnDeadline &&
                            !Sprint18OwnerIsActing(owner)) yield return 0;
                        bool acting = (bool)admission["admitted"] &&
                            owner.IsVisibleForPlayer &&
                            Sprint18OwnerIsActing(owner);
                        Sprint25Check("s29-" + key + "-ai-turn-admission",
                            acting, new JObject {
                                ["owner"] = owner.UniqueId,
                                ["target"] = target.UniqueId,
                                ["admission"] = admission,
                                ["turn"] = Sprint22TurnState(owner, target),
                                ["waitedFrames"] = waited },
                            "The exact visible hidden giant receives its native active turn before its brain selects a command.");
                        if (!acting) throw new InvalidOperationException(
                            "Sprint 29 AI turn admission failed: " + key);
                        Sprint28SetBrainActivePreservingActions(owner, true);
                        owner.Brain.RestoreAvailableActions();
                        bool ready = owner.IsAIEnabled && owner.IsInCombat &&
                            owner.Descriptor.State.CanAct &&
                            owner.IsEnemy(target) && target.IsEnemy(owner) &&
                            owner.HasLOS(target) &&
                            owner.Memory.ContainsVisible(target) &&
                            owner.Brain.Actions.Count > 0 &&
                            owner.Brain.AvailableActions.Count > 0;
                        Sprint25Check("s29-" + key + "-ai-turn-ready",
                            ready, new JObject {
                                ["owner"] = owner.UniqueId,
                                ["target"] = target.UniqueId,
                                ["lane"] = lane,
                                ["actions"] = owner.Brain.Actions.Count,
                                ["available"] = owner.Brain.AvailableActions.Count,
                                ["visible"] = owner.Memory.ContainsVisible(target),
                                ["los"] = owner.HasLOS(target) },
                            "The native brain sees a legal exact hostile and has available actions on its own turn.");
                        if (!ready) throw new InvalidOperationException(
                            "Sprint 29 AI turn precondition failed: " + key);
                        BlueprintAbilityAreaEffect fogArea = key ==
                            "cloud-giant" ? BlueprintLibraryLookup
                            .RequireExact<BlueprintAbilityAreaEffect>(
                                BlueprintBootstrap.Library,
                                "e09010a73354a794293ebc7b33c2d130",
                                "Cloud native fog AI area") : null;
                        int fogBefore = fogArea == null ? 0 :
                            Sprint29OwnedAreas(owner, fogArea).Length;
                        var observer = new Sprint24AttackObserver {
                            Owner = owner, Target = target };
                        var commands = new HashSet<UnitCommand>();
                        int nativeTurnFrames = 0;
                        EventBus.Subscribe(observer);
                        try
                        {
                            DateTime deadline = DateTime.UtcNow
                                .AddSeconds(35);
                            for (int frame = 0; frame < 3000 &&
                                DateTime.UtcNow < deadline; frame++)
                            {
                                if (Sprint18OwnerIsActing(owner))
                                    nativeTurnFrames++;
                                foreach (UnitCommand command in owner.Commands
                                    .Raw.Where(value => value != null))
                                    commands.Add(command);
                                bool finished = commands.Any(value =>
                                    value.AiAction != null && value.IsFinished);
                                bool effect = observer.WeaponAttacks.Count > 0 ||
                                    observer.Damage.Count > 0 ||
                                    fogArea != null && Sprint29OwnedAreas(
                                        owner, fogArea).Length > fogBefore;
                                if (finished && effect) break;
                                Game.Instance.IsPaused = false;
                                yield return 0;
                            }
                            bool aiCommand = commands.Any(value =>
                                value.AiAction != null && value.IsFinished &&
                                value.Result == UnitCommand.ResultType.Success);
                            bool attack = observer.OwnerWeaponAttacks.Count > 0 &&
                                observer.WeaponAttacks.Count > 0;
                            bool fog = fogArea != null &&
                                Sprint29OwnedAreas(owner, fogArea).Length >
                                    fogBefore;
                            bool lightning = observer.Damage.Count > 0;
                            bool meaningful = key == "ettin" ? attack :
                                key == "cloud-giant" ? attack || fog :
                                attack || lightning;
                            UnitAttack ettinAttack = key == "ettin" ?
                                commands.OfType<UnitAttack>().FirstOrDefault(
                                    value => value.AiAction != null &&
                                        ReferenceEquals(value.TargetUnit,
                                            target)) : null;
                            bool ettinCadence = key != "ettin" ||
                                ettinAttack != null &&
                                ettinAttack.IsFullAttack &&
                                ettinAttack.AllAttacks.Count == 4 &&
                                observer.OwnerWeaponAttacks.Count == 4 &&
                                observer.WeaponAttacks.Count == 4 &&
                                observer.Rolls.Count == 4;
                            bool exact = aiCommand && nativeTurnFrames > 0 &&
                                Sprint18OwnerIsActing(owner) && meaningful &&
                                ettinCadence &&
                                commands.Count(value => value.AiAction != null &&
                                    value.IsFinished) == 1;
                            Sprint25Check("s29-" + key + "-ai-turn-action",
                                exact, new JObject {
                                    ["owner"] = owner.UniqueId,
                                    ["target"] = target.UniqueId,
                                    ["nativeTurnFrames"] = nativeTurnFrames,
                                    ["sameTurn"] = Sprint18OwnerIsActing(owner),
                                    ["commands"] = new JArray(commands.Select(
                                        Sprint27AiCommand)),
                                    ["weaponAttacks"] =
                                        observer.WeaponAttacks.Count,
                                    ["damagePackets"] = observer.Damage.Count,
                                    ["ettinFullAttack"] =
                                        ettinAttack?.IsFullAttack,
                                    ["ettinPlanned"] =
                                        ettinAttack?.AllAttacks.Count,
                                    ["ettinRolls"] = observer.Rolls.Count,
                                    ["ownedFogAreasBefore"] = fogBefore,
                                    ["ownedFogAreasAfter"] = fogArea == null ? 0 :
                                        Sprint29OwnedAreas(owner, fogArea).Length,
                                    ["fixtureAttackCommands"] = 0,
                                    ["forcedRolls"] = 0 },
                                "One native AI-selected action resolves on the exact hidden giant's turn without a fixture command or forced roll.");
                            if (!exact) throw new InvalidOperationException(
                                "Sprint 29 AI turn action did not resolve: " + key);
                        }
                        finally { EventBus.Unsubscribe(observer); }
                    }
                    finally
                    {
                        Sprint28SetBrainActivePreservingActions(owner, false);
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                            false;
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
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
