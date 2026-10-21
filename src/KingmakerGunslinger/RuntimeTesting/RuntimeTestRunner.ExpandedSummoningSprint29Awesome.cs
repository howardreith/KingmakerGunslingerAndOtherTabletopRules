using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Newtonsoft.Json.Linq;
using Pathfinding;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Sprint29AwesomeEvents :
            IGlobalRulebookHandler<RuleCombatManeuver>,
            IGlobalRulebookHandler<RuleDealDamage>
        {
            internal UnitEntityData Owner, Target;
            internal readonly List<RuleCombatManeuver> Maneuvers =
                new List<RuleCombatManeuver>();
            internal readonly List<RuleDealDamage> Damage =
                new List<RuleDealDamage>();
            public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }
            public void OnEventDidTrigger(RuleCombatManeuver evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, Target)) Maneuvers.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner) &&
                    ReferenceEquals(evt.Target, Target)) Damage.Add(evt);
            }
        }

        private sealed class Sprint29AwesomeLane
        {
            internal Vector3 Caster;
            internal Vector3 Target;
            internal Vector3 Direction;
            internal Vector3 Traced;
            internal float Available;
            internal string Label;
        }

        private IEnumerable<int> ReviewSprint29Awesome(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            UnitEntityData target = fixture.Hostile;
            int oldCmd = target.Stats.AdditionalCMD.BaseValue;
            int oldHp = target.Stats.HitPoints.BaseValue;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                foreach (var spec in new[] {
                    new { Key="cloud-giant", Case="open" },
                    new { Key="storm-giant", Case="blocked" },
                    new { Key="cloud-giant", Case="failure" },
                    new { Key="storm-giant", Case="immune" }
                })
                {
                    ResetExpandedSummoningHostile(fixture);
                    // Every case starts with the same disposable standing
                    // target; a prior native stand-up transition must not
                    // make the next knockdown appear already consumed.
                    target.Descriptor.State.RemoveCondition(
                        UnitCondition.Prone);
                    target.Descriptor.State.Prone.Active = false;
                    target.Descriptor.State.Prone.ShouldBeActive = false;
                    target.Stats.HitPoints.BaseValue = 100000;
                    target.Descriptor.Damage = 0;
                    target.Stats.AdditionalCMD.BaseValue = oldCmd;
                    UnitEntityData owner = CastExpandedSummoningOwnTier(
                        fixture, spec.Key);
                    SetExpandedSummoningBrainActive(owner, false);
                    try
                    {
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        bool blocked = spec.Case == "blocked";
                        var survey = new JArray();
                        Sprint29AwesomeLane lane = Sprint29FindAwesomeLane(
                            fixture, owner, target, blocked, survey);
                        bool available = lane != null;
                        Sprint25Check("s29-awesome-" + spec.Case + "-lane",
                            available, new JObject {
                                ["owner"] = owner.UniqueId,
                                ["target"] = target.UniqueId,
                                ["survey"] = survey,
                                ["selected"] = lane == null ? null :
                                    new JObject {
                                        ["label"] = lane.Label,
                                        ["nativeAvailableMeters"] =
                                            lane.Available } },
                            "A request-owned visible melee lane has the intended native open or obstructed force-move geometry.");
                        if (!available) throw new InvalidOperationException(
                            "No bounded native Awesome Blow lane: " +
                            spec.Case);
                        PlaceExpandedSummoningUnit(owner, lane.Caster);
                        PlaceExpandedSummoningUnit(target, lane.Target);
                        owner.SightCache.ClearIfNeeded();
                        target.SightCache.ClearIfNeeded();
                        BlueprintAbility blueprint = Sprint29MagicAbility(
                            fixture, "AwesomeBlowAction");
                        var ability = new AbilityData(owner.Descriptor
                            .Abilities.GetAbility(blueprint));
                        var wrapper = new TargetWrapper(target);
                        bool canCast = ability.IsAvailable &&
                            ability.CanTarget(wrapper) &&
                            owner.HasLOS(target) &&
                            owner.IsEnemy(target);
                        Sprint25Check("s29-awesome-" + spec.Case +
                            "-ready", canCast, new JObject {
                                ["owner"] = owner.UniqueId,
                                ["ability"] = blueprint.AssetGuid,
                                ["available"] = ability.IsAvailable,
                                ["canTarget"] = ability.CanTarget(wrapper),
                                ["los"] = owner.HasLOS(target),
                                ["distance"] = owner.DistanceTo(target) },
                            "The exact hidden owner has a directly usable native standard action against a visible disposable target.");
                        if (!canCast) throw new InvalidOperationException(
                            "Awesome Blow direct command is unavailable: " +
                            spec.Case);
                        if (spec.Case == "failure")
                            target.Stats.AdditionalCMD.BaseValue = 1000;
                        else
                            target.Stats.AdditionalCMD.BaseValue = -100;
                        if (spec.Case == "immune")
                            target.Descriptor.State.AddCondition(
                                UnitCondition.ImmuneToCombatManeuvers, null);
                        owner.JoinCombat(); target.JoinCombat();
                        owner.Memory.Add(target); target.Memory.Add(owner);
                        // A preexisting disposable-target command must not
                        // contaminate the post-impact displacement sample.
                        target.Commands.InterruptAll(true);
                        int damageBefore = target.Descriptor.Damage;
                        Vector3 start = target.Position;
                        var events = new Sprint29AwesomeEvents {
                            Owner = owner, Target = target };
                        var costs = new JArray();
                        var movement = new JArray();
                        bool sawForce = false, forceFinished = false;
                        bool chunkBounded = true;
                        bool proneSeen = false, priorForce = false;
                        bool proneActiveSeen = false;
                        float forceMoveMeters = float.NaN;
                        int sampledFrames = 0, priorDamage = 0;
                        Action<UnitUseAbility> sample = value =>
                        {
                            UnitPartForceMove part = target
                                .Get<UnitPartForceMove>();
                            UnitPartForceMove.Chunk active = part == null ?
                                null : part.Chunks.FirstOrDefault();
                            bool force = part != null &&
                                part.Chunks.Any();
                            if (force) sawForce = true;
                            if (force)
                                chunkBounded &= part.Chunks.Count() == 1 &&
                                    active.MaxDistance.HasValue &&
                                    Mathf.Abs(active.MaxDistance.Value -
                                        new Feet(10).Meters) < .01f;
                            float displacement = Vector3.Dot(
                                target.Position - start, lane.Direction);
                            bool proneRequested = target.Descriptor.State
                                .Prone.ShouldBeActive;
                            bool proneActive = target.Descriptor.State.Prone
                                .Active;
                            bool proneNow = proneRequested || proneActive;
                            proneSeen |= proneNow;
                            proneActiveSeen |= proneActive;
                            if (priorForce && !force && !forceFinished)
                            {
                                forceFinished = true;
                                forceMoveMeters = displacement;
                            }
                            if (sampledFrames < 12 ||
                                priorForce != force ||
                                priorDamage != events.Damage.Count ||
                                (sampledFrames % 30 == 0 &&
                                    movement.Count < 48))
                                movement.Add(new JObject {
                                    ["frame"] = sampledFrames,
                                    ["displacement"] = displacement,
                                    ["force"] = force,
                                    ["chunkCount"] = part == null ? 0 :
                                        part.Chunks.Count(),
                                    ["chunkPassed"] = active == null ?
                                        (float?)null : active.PassedDistance,
                                    ["chunkMaximum"] = active == null ?
                                        (float?)null : active.MaxDistance,
                                    ["chunkSpeed"] = active == null ?
                                        (float?)null : active.Speed,
                                    ["prone"] = proneNow,
                                    ["proneRequested"] = proneRequested,
                                    ["proneActive"] = proneActive,
                                    ["damageCount"] =
                                        events.Damage.Count });
                            priorForce = force;
                            priorDamage = events.Damage.Count;
                            sampledFrames++;
                        };
                        UnitUseAbility command = null;
                        EventBus.Subscribe(events);
                        try
                        {
                            using (var economy = new FirearmReviewCosts(owner,
                                costs))
                            {
                                foreach (int frame in Sprint26Cast(owner,
                                    blueprint, wrapper,
                                    value => command = value,
                                    sample)) yield return frame;
                                DateTime deadline = DateTime.UtcNow
                                    .AddSeconds(5);
                                for (int frame = 0; frame < 500 &&
                                    DateTime.UtcNow < deadline &&
                                    target.Get<UnitPartForceMove>() != null &&
                                    target.Get<UnitPartForceMove>().Chunks.Any();
                                    frame++)
                                {
                                    sample(command);
                                    yield return 0;
                                }
                                for (int frame = 0; frame < 8; frame++)
                                {
                                    sample(command);
                                    yield return 0;
                                }
                                float finalMoved = Vector3.Dot(target.Position -
                                    start, lane.Direction);
                                float moved = forceFinished ?
                                    forceMoveMeters : finalMoved;
                                bool success = events.Maneuvers.Count == 1 &&
                                    events.Maneuvers[0].Success;
                                bool cost = command != null &&
                                    command.IsFinished &&
                                    command.Result ==
                                        UnitCommand.ResultType.Success &&
                                    economy.Count(command) == 1 &&
                                    economy.Exact(command, false);
                                bool expected = spec.Case == "open" ?
                                    success && events.Damage.Count == 1 &&
                                    moved >= 2.85f && moved <=
                                        new Feet(10).Meters + .1f &&
                                    proneSeen :
                                    spec.Case == "blocked" ?
                                    success && events.Damage.Count == 2 &&
                                    moved >= 0f && moved < 2.85f &&
                                    proneSeen :
                                    spec.Case == "failure" ?
                                    events.Maneuvers.Count == 1 &&
                                    !success && events.Damage.Count == 0 &&
                                    moved < .15f :
                                    events.Maneuvers.Count == 0 &&
                                    events.Damage.Count == 0 &&
                                    moved < .15f;
                                bool forceExpected = success ?
                                    sawForce && forceFinished :
                                    !sawForce;
                                bool exact = cost && expected &&
                                    forceExpected && chunkBounded &&
                                    events.Damage.All(value =>
                                        ReferenceEquals(value.Initiator,
                                            owner) &&
                                        ReferenceEquals(value.Target,
                                            target)) &&
                                    !target.Descriptor.State.IsDead;
                                Sprint25Check("s29-awesome-" + spec.Case +
                                    "-native", exact,
                                    new JObject {
                                        ["owner"] = owner.UniqueId,
                                        ["target"] = target.UniqueId,
                                        ["case"] = spec.Case,
                                        ["nativeAvailableMeters"] =
                                            lane.Available,
                                        ["movedMeters"] = moved,
                                        ["finalMovedMeters"] = finalMoved,
                                        ["sawForce"] = sawForce,
                                        ["forceFinished"] = forceFinished,
                                        ["chunkBounded"] = chunkBounded,
                                        ["proneSeen"] = proneSeen,
                                        ["proneActiveSeen"] =
                                            proneActiveSeen,
                                        ["movement"] = movement,
                                        ["maneuvers"] = new JArray(
                                            events.Maneuvers.Select(value =>
                                                new JObject {
                                                    ["success"] =
                                                        value.Success,
                                                    ["autoFailure"] =
                                                        value.AutoFailure })),
                                        ["damage"] = new JArray(events.Damage
                                            .Select(Sprint16DamageEvent)),
                                        ["damageBefore"] = damageBefore,
                                        ["damageAfter"] =
                                            target.Descriptor.Damage,
                                        ["proneRequested"] = target.Descriptor
                                            .State.Prone.ShouldBeActive,
                                        ["costs"] = costs,
                                        ["commandFinished"] = command
                                            ?.IsFinished,
                                        ["commandResult"] = command
                                            ?.Result.ToString() },
                                    "A real native standard action preserves maneuver success or failure, exact damage attribution and obstacle-bounded movement.");
                                if (!exact) throw new InvalidOperationException(
                                    "Awesome Blow native outcome differs: " +
                                    spec.Case);
                            }
                        }
                        finally { EventBus.Unsubscribe(events); }
                    }
                    finally
                    {
                        target.Descriptor.State.RemoveCondition(
                            UnitCondition.ImmuneToCombatManeuvers);
                        target.Stats.AdditionalCMD.BaseValue = oldCmd;
                        if (!owner.Destroyed)
                            owner.Commands.InterruptAll(true);
                        if (!owner.Destroyed && owner.IsInCombat)
                            owner.LeaveCombat();
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
                    }
                }
            }
            finally
            {
                target.Descriptor.State.RemoveCondition(
                    UnitCondition.ImmuneToCombatManeuvers);
                target.Stats.AdditionalCMD.BaseValue = oldCmd;
                target.Stats.HitPoints.BaseValue = oldHp;
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                    oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private Sprint29AwesomeLane Sprint29FindAwesomeLane(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target, bool blocked,
            JArray survey)
        {
            var anchor = AstarPath.active.GetNearest(
                Sprint25IsolatedAnchor(fixture));
            if (anchor.node == null || !anchor.node.Walkable)
                return null;
            float distance = new Feet(10).Meters;
            foreach (float radius in new[] { 0f, 2f, 4f, 6f, 8f, 10f })
            foreach (Vector3 offset in CompassOffsets)
            foreach (Vector3 direction in CompassOffsets)
            {
                Vector3 requested = anchor.clampedPosition + offset *
                    radius;
                var targetNode = AstarPath.active.GetNearest(requested);
                if (targetNode.node == null ||
                    !targetNode.node.Walkable ||
                    targetNode.node.Area != anchor.node.Area ||
                    targetNode.node.GraphIndex != anchor.node.GraphIndex ||
                    Vector3.Distance(targetNode.clampedPosition,
                        requested) > .35f) continue;
                Vector3 targetPoint = targetNode.clampedPosition;
                Vector3 requestedCaster = targetPoint - direction *
                    (owner.Corpulence + target.Corpulence + .8f);
                var casterNode = AstarPath.active.GetNearest(
                    requestedCaster);
                if (casterNode.node == null ||
                    !casterNode.node.Walkable ||
                    casterNode.node.Area != anchor.node.Area ||
                    casterNode.node.GraphIndex != anchor.node.GraphIndex ||
                    Vector3.Distance(casterNode.clampedPosition,
                        requestedCaster) > .35f ||
                    fixture.UnitsBefore.OfType<UnitEntityData>().Any(value =>
                        Vector3.Distance(value.Position, targetPoint) < 8f ||
                        Vector3.Distance(value.Position,
                            casterNode.clampedPosition) < 8f)) continue;
                Vector3 traced = ObstacleAnalyzer.TraceAlongNavmesh(
                    targetPoint, targetPoint + direction * distance);
                float available = Vector3.Dot(traced - targetPoint,
                    direction);
                if (survey.Count < 16)
                    survey.Add(new JObject {
                        ["radius"] = radius,
                        ["direction"] = direction.ToString(),
                        ["availableMeters"] = available });
                if (blocked ? available < .5f || available > 2.6f :
                    available < distance - .08f) continue;
                return new Sprint29AwesomeLane {
                    Caster = casterNode.clampedPosition,
                    Target = targetPoint, Direction = direction,
                    Traced = traced, Available = available,
                    Label = radius + "/" + direction.ToString() };
            }
            return null;
        }
    }
}
