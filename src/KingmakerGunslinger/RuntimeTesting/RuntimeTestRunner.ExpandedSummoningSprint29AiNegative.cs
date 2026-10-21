using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using Pathfinding;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewSprint29AiNegative(
            ExpandedSummoningCorrectionFixture fixture)
        {
            BlueprintFeature electricity = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                    "cd1e5ab641a833c49994aff99db98952",
                    "native electricity immunity for Storm AI screening");
            BlueprintAbilityAreaEffect fogArea = BlueprintLibraryLookup
                .RequireExact<BlueprintAbilityAreaEffect>(
                    BlueprintBootstrap.Library,
                    "e09010a73354a794293ebc7b33c2d130",
                    "native Cloud fog AI safety area");
            UnitEntityData target = fixture.Hostile;
            foreach (string key in new[] { "cloud-giant", "storm-giant" })
            {
                ResetExpandedSummoningHostile(fixture);
                UnitEntityData owner = CastExpandedSummoningOwnTier(
                    fixture, key);
                Sprint28SetBrainActivePreservingActions(owner, false);
                UnitEntityData ally = null;
                bool addedElectricity = false;
                try
                {
                    owner.Commands.InterruptAll(true);
                    target.Commands.InterruptAll(true);
                    owner.Commands.RemoveFinishedAndUpdateQueue();
                    target.Commands.RemoveFinishedAndUpdateQueue();
                    Vector3 far; string lane, direction;
                    Vector3 origin = Sprint24AiCombatLane(fixture.Caster,
                        owner, target, 8f, out far, out lane,
                        out direction);
                    PlaceExpandedSummoningUnit(owner, origin);
                    PlaceExpandedSummoningUnit(target, far);
                    if (key == "cloud-giant")
                    {
                        ally = CastExpandedSummoningOwnTier(fixture,
                            "ettin");
                        Sprint28SetBrainActivePreservingActions(ally,
                            false);
                        Vector3 side = Vector3.Cross(Vector3.up,
                            (far - origin).normalized);
                        bool placed = false;
                        foreach (float sign in new[] { 1f, -1f })
                        {
                            Vector3 requested = far + side * sign * 2f;
                            var point = AstarPath.active.GetNearest(requested);
                            if (point.node == null ||
                                !point.node.Walkable ||
                                Vector3.Distance(point.clampedPosition,
                                    requested) > .35f) continue;
                            PlaceExpandedSummoningUnit(ally,
                                point.clampedPosition);
                            placed = true;
                            break;
                        }
                        bool near = placed &&
                            owner.IsAlly(ally) &&
                            Vector3.Distance(ally.Position,
                                target.Position) < new Feet(20).Meters;
                        Sprint25Check("s29-cloud-ai-unsafe-lane", near,
                            new JObject { ["owner"] = owner.UniqueId,
                                ["target"] = target.UniqueId,
                                ["ally"] = ally.UniqueId,
                                ["lane"] = lane, ["placed"] = placed,
                                ["ownerAllySummoner"] = owner.IsAlly(
                                    fixture.Caster),
                                ["ownerAllySibling"] = owner.IsAlly(ally),
                                ["allyDistance"] = Vector3.Distance(
                                    ally.Position, target.Position) },
                            "A real allied unit occupies the hostile-centered native fog radius before Cloud's autonomous choice.");
                        if (!near) throw new InvalidOperationException(
                            "No ally-near native Cloud AI lane.");
                    }
                    else
                    {
                        if (target.Descriptor.AddFact(electricity) == null)
                            throw new InvalidOperationException(
                                "Disposable hostile could not receive native electricity immunity.");
                        addedElectricity = true;
                    }
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
                        owner.IsEnemy(target) && owner.HasLOS(target) &&
                        owner.Memory.ContainsVisible(target) &&
                        (key == "cloud-giant" ?
                            owner.IsAlly(ally) :
                            target.Descriptor.HasFact(electricity));
                    Sprint25Check("s29-" + key + "-ai-safety-ready",
                        ready, new JObject { ["owner"] = owner.UniqueId,
                            ["target"] = target.UniqueId,
                            ["lane"] = lane,
                            ["visible"] = owner.Memory.ContainsVisible(
                                target), ["los"] = owner.HasLOS(target),
                            ["electricityImmune"] = target.Descriptor
                                .HasFact(electricity) },
                        "The exact owner sees a target with the stated native fog or electricity safety precondition.");
                    if (!ready) throw new InvalidOperationException(
                        "Sprint 29 negative AI precondition unavailable: " +
                        key);
                    var observer = new Sprint24AttackObserver {
                        Owner = owner, Target = target };
                    var commands = new HashSet<UnitCommand>();
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
                                Sprint24TickExactAi(owner);
                            foreach (UnitCommand command in owner.Commands
                                .Raw.Where(value => value != null))
                                commands.Add(command);
                            if (commands.Any(value => value.AiAction != null &&
                                value.IsFinished) &&
                                (observer.WeaponAttacks.Count > 0 ||
                                 commands.OfType<UnitUseAbility>().Any(
                                     value => value.AiAction != null &&
                                         value.IsFinished))) break;
                            Game.Instance.IsPaused = false;
                            yield return 0;
                        }
                        bool ai = commands.Any(value =>
                            value.AiAction != null && value.IsFinished &&
                            value.Result == UnitCommand.ResultType.Success);
                        bool safe = key == "cloud-giant" ?
                            !commands.OfType<UnitUseAbility>().Any(value =>
                                value.AiAction != null &&
                                value.AiAction.name.EndsWith(
                                    "CloudFogAi", StringComparison.Ordinal)) &&
                            Sprint29OwnedAreas(owner, fogArea).Length == 0 &&
                            owner.Descriptor.Resources.GetResourceAmount(
                                Sprint29MagicResource(fixture,
                                    "CloudFogResource")) == 1 :
                            !commands.OfType<UnitUseAbility>().Any(value =>
                                value.AiAction != null &&
                                (value.AiAction.name.EndsWith(
                                    "StormCallAi", StringComparison.Ordinal) ||
                                 value.AiAction.name.EndsWith(
                                    "StormBoltAi", StringComparison.Ordinal) ||
                                 value.AiAction.name.EndsWith(
                                    "StormChainAi", StringComparison.Ordinal))) &&
                            owner.Descriptor.Resources.GetResourceAmount(
                                Sprint29MagicResource(fixture,
                                    "StormCallResource")) == 1 &&
                            owner.Descriptor.Resources.GetResourceAmount(
                                Sprint29MagicResource(fixture,
                                    "StormChainResource")) == 1;
                        bool useful = key == "cloud-giant" ? ai :
                            ai && observer.WeaponAttacks.Count > 0;
                        Sprint25Check("s29-" + key + "-ai-safety",
                            useful && safe,
                            new JObject { ["owner"] = owner.UniqueId,
                                ["target"] = target.UniqueId,
                                ["attackEvents"] =
                                    observer.WeaponAttacks.Count,
                                ["commands"] = new JArray(commands.Select(
                                    Sprint27AiCommand)),
                                ["fogAreas"] = Sprint29OwnedAreas(owner,
                                    fogArea).Length,
                                ["fogUses"] = key == "cloud-giant" ?
                                    owner.Descriptor.Resources
                                        .GetResourceAmount(
                                            Sprint29MagicResource(fixture,
                                                "CloudFogResource")) : -1,
                                ["callUses"] = key == "storm-giant" ?
                                    owner.Descriptor.Resources
                                        .GetResourceAmount(
                                            Sprint29MagicResource(fixture,
                                                "StormCallResource")) : -1,
                                ["chainUses"] = key == "storm-giant" ?
                                    owner.Descriptor.Resources
                                        .GetResourceAmount(
                                            Sprint29MagicResource(fixture,
                                                "StormChainResource")) : -1 },
                            "Native AI rejects ally-obscuring fog or a known electricity-immune target and chooses a useful independent action.");
                    }
                    finally { EventBus.Unsubscribe(observer); }
                }
                finally
                {
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(true);
                    target.Commands.InterruptAll(true);
                    if (addedElectricity)
                        target.Descriptor.RemoveFact(electricity);
                    if (owner.IsInCombat) owner.LeaveCombat();
                    if (ally != null && ally.IsInCombat)
                        ally.LeaveCombat();
                    if (target.IsInCombat) target.LeaveCombat();
                    Game.Instance.Player.UpdateIsInCombat();
                    DisposeExpandedSummoningUnits(fixture.Created,
                        ally == null ? new[] { owner } :
                            new[] { owner, ally });
                    ResetExpandedSummoningHostile(fixture);
                }
            }
        }
    }
}
