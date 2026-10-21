using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The native brain owns every action. The fixture only admits an
        // exact disposable owner and hostile into a surveyed visible lane.
        private IEnumerable<int> ReviewSprint29Ai(
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
                            key == "cloud-giant" ? new Feet(20).Meters : 0f);
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
                        owner.JoinCombat(); target.JoinCombat();
                        Game.Instance.Player.UpdateIsInCombat();
                        new UnitCombatJoinController().Tick();
                        new UnitCombatPrepareController().Tick();
                        Sprint28SetBrainActivePreservingActions(owner, true);
                        owner.Brain.RestoreAvailableActions();
                        bool ready = owner.IsAIEnabled && owner.IsInCombat &&
                            owner.Descriptor.State.CanAct &&
                            owner.IsEnemy(target) && target.IsEnemy(owner) &&
                            owner.HasLOS(target) &&
                            owner.Memory.ContainsVisible(target) &&
                            owner.Brain.Actions.Count > 0 &&
                            owner.Brain.AvailableActions.Count > 0;
                        Sprint25Check("s29-" + key + "-ai-ready", ready,
                            new JObject { ["owner"] = owner.UniqueId,
                                ["target"] = target.UniqueId,
                                ["lane"] = lane,
                                ["actions"] = owner.Brain.Actions.Count,
                                ["available"] = owner.Brain.AvailableActions.Count,
                                ["visible"] = owner.Memory.ContainsVisible(target),
                                ["los"] = owner.HasLOS(target) },
                            "Exact hidden owner and disposable hostile are visible and the native brain is active.");
                        if (!ready) throw new InvalidOperationException(
                            "Sprint 29 AI owner lacks a native decision precondition.");
                        var observer = new Sprint24AttackObserver {
                            Owner = owner, Target = target };
                        var commands = new HashSet<UnitCommand>();
                        var ambient = new HashSet<string>();
                        int ticks = 0;
                        BlueprintAbilityAreaEffect fogArea = key ==
                            "cloud-giant" ? BlueprintLibraryLookup
                            .RequireExact<BlueprintAbilityAreaEffect>(
                                BlueprintBootstrap.Library,
                                "e09010a73354a794293ebc7b33c2d130",
                                "Cloud native fog AI area") : null;
                        int fogBefore = fogArea == null ? 0 :
                            Sprint29OwnedAreas(owner, fogArea).Length;
                        bool safeFogCenter = fogArea == null ||
                            !Game.Instance.State.Units.All.Any(value =>
                                value != null && !value.Destroyed &&
                                !ReferenceEquals(value, owner) &&
                                owner.IsAlly(value) &&
                                Vector3.Distance(value.Position,
                                    target.Position) <=
                                    new Feet(20).Meters + value.Corpulence);
                        if (fogArea != null)
                        {
                            Sprint25Check("s29-cloud-fog-ai-safe-lane",
                                safeFogCenter,
                                new JObject { ["owner"] = owner.UniqueId,
                                    ["target"] = target.UniqueId,
                                    ["safeCenter"] = safeFogCenter,
                                    ["lane"] = lane },
                                "The hostile-centered fog lane excludes each actual allied unit before the native AI decision.");
                            if (!safeFogCenter)
                                throw new InvalidOperationException(
                                    "Cloud fog AI fixture has no safe target center.");
                        }
                        EventBus.Subscribe(observer);
                        try
                        {
                            DateTime deadline = DateTime.UtcNow.AddSeconds(35);
                            for (int frame = 0; frame < 3000 &&
                                DateTime.UtcNow < deadline; frame++)
                            {
                                if (frame == 0 || frame % 60 == 0 &&
                                    !owner.Commands.Raw.Any(value =>
                                        value != null && !value.IsFinished))
                                {
                                    foreach (string id in Sprint24TickExactAi(owner))
                                        if (id != owner.UniqueId)
                                            ambient.Add(id);
                                    ticks++;
                                }
                                foreach (UnitCommand command in owner.Commands.Raw
                                    .Where(value => value != null))
                                    commands.Add(command);
                                bool actionFinished = commands.Any(value =>
                                    value.AiAction != null && value.IsFinished);
                                if (actionFinished &&
                                    (observer.WeaponAttacks.Count > 0 ||
                                     observer.Damage.Count > 0 ||
                                     fogArea != null &&
                                     Sprint29OwnedAreas(owner, fogArea).Length >
                                         fogBefore)) break;
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
                            bool spellDamage = observer.Damage.Count > 0;
                            bool meaningful = key == "ettin" ? attack :
                                key == "cloud-giant" ? attack || fog :
                                attack || spellDamage;
                            Sprint25Check("s29-" + key + "-ai-action",
                                aiCommand && ticks > 0 && meaningful,
                                new JObject { ["owner"] = owner.UniqueId,
                                    ["target"] = target.UniqueId,
                                    ["ticks"] = ticks,
                                    ["ambientCandidatesNeverTicked"] =
                                        new JArray(ambient),
                                    ["commands"] = new JArray(commands
                                        .Select(Sprint27AiCommand)),
                                    ["weaponAttacks"] =
                                        observer.WeaponAttacks.Count,
                                    ["damagePackets"] = observer.Damage.Count,
                                    ["ownedFogAreasBefore"] = fogBefore,
                                    ["ownedFogAreasAfter"] = fogArea == null ? 0 :
                                        Sprint29OwnedAreas(owner, fogArea).Length,
                                    ["fixtureAttackCommands"] = 0,
                                    ["forcedRolls"] = 0 },
                                "The exact native brain chooses a successful useful attack or spell without a fixture command or forced roll.");
                            if (fogArea != null)
                            {
                                var fogAreas = Sprint29OwnedAreas(owner,
                                    fogArea);
                                var fogResource = Sprint29MagicResource(fixture,
                                    "CloudFogResource");
                                bool remoteFog = fogAreas.Any(value =>
                                    Sprint29AreaDuration(value) > 9500 &&
                                    Sprint29AreaDuration(value) <= 9600.1 &&
                                    value.Context?.Params.CasterLevel == 16) &&
                                    owner.Descriptor.Resources.GetResourceAmount(
                                        fogResource) == 0 &&
                                    commands.OfType<UnitUseAbility>().Any(
                                        value => value.AiAction != null &&
                                            value.IsFinished &&
                                            value.Result ==
                                                UnitCommand.ResultType.Success);
                                Sprint25Check("s29-cloud-fog-ai-cast",
                                    remoteFog, new JObject {
                                        ["owner"] = owner.UniqueId,
                                        ["target"] = target.UniqueId,
                                        ["safeCenter"] = safeFogCenter,
                                        ["areas"] = new JArray(fogAreas.Select(
                                            value => new JObject {
                                                ["durationSeconds"] =
                                                    Sprint29AreaDuration(value),
                                                ["casterLevel"] = value
                                                    .Context?.Params.CasterLevel })),
                                        ["fogResource"] = owner.Descriptor
                                            .Resources.GetResourceAmount(
                                                fogResource),
                                        ["commands"] = new JArray(commands
                                            .Select(Sprint27AiCommand)) },
                                    "Native Cloud AI centers its distinct limited CL16 fog on a safe hostile lane and spends only its own one use.");
                            }
                            if (key == "storm-giant")
                            {
                                foreach (int frame in Sprint29StormAiLightning(
                                    fixture, owner, target, observer))
                                    yield return frame;
                            }
                        }
                        finally { EventBus.Unsubscribe(observer); }
                    }
                    finally
                    {
                        Sprint28SetBrainActivePreservingActions(owner, false);
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

        private IEnumerable<int> Sprint29StormAiLightning(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target,
            Sprint24AttackObserver observer)
        {
            var chainResource = Sprint29MagicResource(fixture,
                "StormChainResource");
            var callResource = Sprint29MagicResource(fixture,
                "StormCallResource");
            BlueprintBuff state = fixture.Blueprints.OfType<BlueprintBuff>()
                .Single(value => value.name ==
                    "KMG_Summoning_Giant_Sprint29_StormLightningState");
            bool ready = !target.Descriptor.State.IsDead &&
                owner.Descriptor.Resources.GetResourceAmount(chainResource) == 0 &&
                owner.Descriptor.Resources.GetResourceAmount(callResource) == 1 &&
                owner.Descriptor.Buffs.GetBuff(state) == null &&
                owner.Memory.ContainsVisible(target) && owner.HasLOS(target);
            Sprint25Check("s29-storm-ai-call-ready", ready,
                new JObject { ["owner"] = owner.UniqueId,
                    ["target"] = target.UniqueId,
                    ["chainUses"] = owner.Descriptor.Resources
                        .GetResourceAmount(chainResource),
                    ["callUses"] = owner.Descriptor.Resources
                        .GetResourceAmount(callResource),
                    ["targetAlive"] = !target.Descriptor.State.IsDead,
                    ["visible"] = owner.Memory.ContainsVisible(target),
                    ["los"] = owner.HasLOS(target) },
                "The first autonomous Chain Lightning left a live visible target and an unused independent Call Lightning activation.");
            if (!ready) throw new InvalidOperationException(
                "Storm AI Call Lightning precondition is unavailable.");

            owner.Commands.RemoveFinishedAndUpdateQueue();
            var later = new HashSet<UnitCommand>();
            int ticks = 0;
            DateTime deadline = DateTime.UtcNow.AddSeconds(45);
            for (int frame = 0; frame < 3600 && DateTime.UtcNow < deadline;
                frame++)
            {
                if (frame == 0 || frame % 60 == 0 &&
                    !owner.Commands.Raw.Any(value =>
                        value != null && !value.IsFinished))
                {
                    Sprint24TickExactAi(owner);
                    ticks++;
                }
                foreach (UnitCommand command in owner.Commands.Raw.Where(
                    value => value != null)) later.Add(command);
                if (owner.Descriptor.Buffs.GetBuff(state) != null &&
                    later.OfType<UnitUseAbility>().Any(value =>
                        value.AiAction != null && value.AiAction.name.EndsWith(
                            "StormCallAi", StringComparison.Ordinal) &&
                        value.IsFinished)) break;
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            Buff active = owner.Descriptor.Buffs.GetBuff(state);
            bool activated = active != null &&
                active.TimeLeft.TotalSeconds > 0 &&
                active.Context?.Params.CasterLevel == 15 &&
                owner.Descriptor.Resources.GetResourceAmount(callResource) == 0 &&
                later.OfType<UnitUseAbility>().Any(value =>
                    value.AiAction != null && value.AiAction.name.EndsWith(
                        "StormCallAi", StringComparison.Ordinal) &&
                    value.IsFinished &&
                    value.Result == UnitCommand.ResultType.Success);
            Sprint25Check("s29-storm-ai-call-activation", activated,
                new JObject { ["owner"] = owner.UniqueId,
                    ["ticks"] = ticks,
                    ["stateRemaining"] = active?.TimeLeft.TotalSeconds,
                    ["casterLevel"] = active?.Context?.Params.CasterLevel,
                    ["callUses"] = owner.Descriptor.Resources
                        .GetResourceAmount(callResource),
                    ["commands"] = new JArray(later.Select(
                        Sprint27AiCommand)) },
                "After spending Chain Lightning, native Storm AI uses its independently limited CL15 Call Lightning activation.");
            if (!activated) yield break;

            BlueprintAbility boltBlueprint = Sprint29MagicAbility(fixture,
                "StormLightningBolt");
            var grantedBolt = owner.Descriptor.Abilities.GetAbility(
                boltBlueprint);
            var boltData = grantedBolt == null ? null :
                new AbilityData(grantedBolt);
            bool boltAvailable = boltData != null &&
                boltData.IsAvailable;
            bool boltTarget = boltData != null &&
                boltData.CanTarget(new TargetWrapper(target));
            string[] availableBefore = owner.Brain.AvailableActions.Select(
                value => value.Blueprint?.name).ToArray();
            owner.Commands.RemoveFinishedAndUpdateQueue();
            later.Clear();
            int damageBefore = observer.Damage.Count;
            double stateBefore = active.TimeLeft.TotalSeconds;
            ticks = 0;
            deadline = DateTime.UtcNow.AddSeconds(45);
            for (int frame = 0; frame < 3600 && DateTime.UtcNow < deadline;
                frame++)
            {
                if (frame == 0 || frame % 60 == 0 &&
                    !owner.Commands.Raw.Any(value =>
                        value != null && !value.IsFinished))
                {
                    Sprint24TickExactAi(owner);
                    ticks++;
                }
                foreach (UnitCommand command in owner.Commands.Raw.Where(
                    value => value != null)) later.Add(command);
                if (later.OfType<UnitUseAbility>().Any(value =>
                    value.AiAction != null && value.AiAction.name.EndsWith(
                        "StormBoltAi", StringComparison.Ordinal) &&
                    value.IsFinished)) break;
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            Buff after = owner.Descriptor.Buffs.GetBuff(state);
            bool bolt = later.OfType<UnitUseAbility>().Any(value =>
                    value.AiAction != null && value.AiAction.name.EndsWith(
                        "StormBoltAi", StringComparison.Ordinal) &&
                    value.IsFinished &&
                    value.Result == UnitCommand.ResultType.Success) &&
                observer.Damage.Count > damageBefore &&
                after != null && after.TimeLeft.TotalSeconds < stateBefore &&
                owner.Descriptor.Resources.GetResourceAmount(callResource) == 0;
            Sprint25Check("s29-storm-ai-granted-bolt", bolt,
                new JObject { ["owner"] = owner.UniqueId,
                    ["ticks"] = ticks,
                    ["damageBefore"] = damageBefore,
                    ["damageAfter"] = observer.Damage.Count,
                    ["stateBefore"] = stateBefore,
                    ["stateAfter"] = after?.TimeLeft.TotalSeconds,
                    ["callUses"] = owner.Descriptor.Resources
                        .GetResourceAmount(callResource),
                    ["boltGranted"] = grantedBolt != null,
                    ["boltAvailable"] = boltAvailable,
                    ["boltCanTarget"] = boltTarget,
                    ["brainAvailableBefore"] = new JArray(availableBefore),
                    ["brainAvailableAfter"] = new JArray(
                        owner.Brain.AvailableActions.Select(value =>
                            value.Blueprint?.name)),
                    ["commands"] = new JArray(later.Select(
                        Sprint27AiCommand)) },
                "Native Storm AI spends a granted Call Lightning bolt action and finite state without spending a new daily activation.");
        }
    }
}
