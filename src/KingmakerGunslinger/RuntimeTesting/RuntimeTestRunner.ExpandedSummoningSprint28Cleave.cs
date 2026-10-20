using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Sprint28CleaveAttacks :
            IGlobalRulebookHandler<RuleAttackWithWeapon>
        {
            internal UnitEntityData Owner;
            internal readonly List<RuleAttackWithWeapon> Rows =
                new List<RuleAttackWithWeapon>();
            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (ReferenceEquals(evt.Initiator, Owner)) Rows.Add(evt);
            }
        }

        private IEnumerable<int> ReviewSprint28Cleave(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = Game.Instance.IsPaused;
            Vector3 casterBefore = fixture.Caster.Position;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int i = 0; i < 16; i++) yield return 0;
                var action = fixture.Blueprints.OfType<BlueprintAbility>()
                    .Single(value => value.AssetGuid ==
                        "952dc5f831b2490caf0c6be723b18f0c");
                var feature = fixture.Blueprints.OfType<BlueprintFeature>()
                    .Single(value => value.AssetGuid ==
                        "9b979adb2ff640a79ebc4dda5cbced22");
                var nativeAction = fixture.Blueprints.OfType<BlueprintAbility>()
                    .Single(value => value.AssetGuid ==
                        "6447d104a2222c14d9c9b8a36e4eb242");
                var nativeCleave = fixture.Blueprints.OfType<BlueprintFeature>()
                    .Single(value => value.AssetGuid ==
                        "d809b6c4ff2aaff4fa70d712a70f7d7b");
                var nativeGreater = fixture.Blueprints.OfType<BlueprintFeature>()
                    .Single(value => value.AssetGuid ==
                        "cc9c862ef2e03af4f89be5088851ea35");
                var grant = feature.GetComponents<AddFacts>().Single();
                var delivery = action.GetComponents<SummonGiantCleaveDelivery>()
                    .Single();
                var nativeDelivery = nativeAction.GetComponents<
                    AbilityCustomCleave>().Single();
                bool graph = grant.Facts.Length == 1 &&
                    ReferenceEquals(grant.Facts[0], action) &&
                    ReferenceEquals(delivery.GreaterFeature, nativeGreater) &&
                    nativeDelivery.GreaterFeature != null &&
                    (string.Equals(nativeDelivery.GreaterFeature.AssetGuid,
                        nativeCleave.AssetGuid,
                        StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(nativeDelivery.GreaterFeature.AssetGuid,
                        nativeGreater.AssetGuid,
                        StringComparison.OrdinalIgnoreCase)) &&
                    !action.ComponentsArray.Intersect(
                        nativeAction.ComponentsArray).Any();
                Sprint25Check("s28-cleave-graph", graph, new JObject {
                    ["action"] = action.AssetGuid,
                    ["feature"] = feature.AssetGuid,
                    ["greaterFact"] = delivery.GreaterFeature?.AssetGuid,
                    ["nativeGreaterFact"] =
                        nativeDelivery.GreaterFeature?.AssetGuid,
                    ["sharedComponents"] = action.ComponentsArray.Intersect(
                        nativeAction.ComponentsArray).Count() },
                    "The private giant action has its own components and Great Cleave discriminator; native Cleave is unchanged.");
                if (!graph) throw new InvalidOperationException(
                    "Giant Cleave graph mismatch.");
                foreach (bool turnBased in new[] { false, true })
                foreach (string key in new[] { "hill-giant", "fire-giant" })
                {
                    var trials = new JArray();
                    bool passed = false;
                    for (int trial = 0; trial < 3 && !passed; trial++)
                    {
                        var created = new List<UnitEntityData>();
                        UnitUseAbility command = null;
                        var observer = new Sprint28CleaveAttacks();
                        bool subscribed = false;
                        Vector3 hostileBefore = fixture.Hostile.Position;
                        try
                        {
                            UnitEntityData owner =
                                CastExpandedSummoningOwnTier(fixture, key);
                            created.Add(owner);
                            observer.Owner = owner;
                            SetExpandedSummoningBrainActive(owner, false);
                            UnitEntityData[] targets =
                                Enumerable.Range(0, 3).Select(index =>
                                    Sprint25CleaveTarget(fixture, created,
                                        true)).ToArray();
                            for (int i = 0; i < 8; i++) yield return 0;
                            Vector3 far; string lane, direction;
                            Vector3 origin = Sprint24AiCombatLane(
                                fixture.Caster, owner, fixture.Hostile, 2.6f,
                                out far, out lane, out direction);
                            var floor = AstarPath.active.GetNearest(origin);
                            if (floor.node == null || !floor.node.Walkable)
                                throw new InvalidOperationException(
                                    "No walkable Cleave owner floor.");
                            Vector3 surveyedOrigin = floor.clampedPosition;
                            bool laneReady = false;
                            var attemptedPositions = new JArray();
                            int connectedOrigins = 0;
                            int graphClusters = 0;
                            int nativeClusters = 0;
                            foreach (Vector3 offset in Enumerable.Range(-2, 5)
                                .SelectMany(dx => Enumerable.Range(-2, 5)
                                    .Select(dz => new Vector3(dx * 3f, 0f,
                                        dz * 3f)))
                                .OrderBy(value => value.sqrMagnitude))
                            {
                                Vector3 candidate = surveyedOrigin + offset;
                                var ownerNode = AstarPath.active.GetNearest(
                                    candidate);
                                if (ownerNode.node == null ||
                                    !ownerNode.node.Walkable ||
                                    ownerNode.node.Area != floor.node.Area ||
                                    ownerNode.node.GraphIndex !=
                                        floor.node.GraphIndex ||
                                    Vector3.Distance(candidate,
                                        ownerNode.clampedPosition) > .6f ||
                                    Game.Instance.Player.Party.Any(value =>
                                        value != null && value.IsInGame &&
                                        Vector3.Distance(value.Position,
                                            ownerNode.clampedPosition) < 3.5f))
                                    continue;
                                connectedOrigins++;
                                Vector3 trialOrigin = ownerNode.clampedPosition;
                                PlaceExpandedSummoningUnit(owner, trialOrigin);
                                foreach (Vector3 forward in new[] {
                                    (far - surveyedOrigin).normalized }.Concat(
                                        CompassOffsets))
                                foreach (float distance in new[] { 2.6f, 3.4f })
                                {
                                    Vector3 side = Vector3.Cross(forward,
                                        Vector3.up).normalized;
                                    Vector3[] points = Enumerable.Range(0, 3)
                                        .Select(index => trialOrigin + forward *
                                            distance + side * (index - 1) *
                                            1.2f).ToArray();
                                    var nodes = points.Select(value =>
                                        AstarPath.active.GetNearest(value))
                                        .ToArray();
                                    if (nodes.Where((node, index) =>
                                        node.node == null ||
                                        !node.node.Walkable ||
                                        node.node.Area != floor.node.Area ||
                                        node.node.GraphIndex !=
                                            floor.node.GraphIndex ||
                                        Vector3.Distance(node.clampedPosition,
                                            points[index]) > .6f ||
                                        Kingmaker.Visual.FogOfWar
                                            .LineOfSightGeometry.Instance
                                            .HasObstacle(trialOrigin +
                                                Vector3.up * 1.2f,
                                                node.clampedPosition +
                                                Vector3.up * 1.2f, 0)).Any())
                                        continue;
                                    graphClusters++;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        targets[i].Commands.InterruptAll(true);
                                        PlaceExpandedSummoningUnit(targets[i],
                                            nodes[i].clampedPosition);
                                        targets[i].SightCache.ClearIfNeeded();
                                    }
                                    owner.SightCache.ClearIfNeeded();
                                    bool reached = targets.All(value =>
                                        owner.HasLOS(value) &&
                                        UnitEngagementExtension.IsReach(owner,
                                            value, owner.Body.PrimaryHand));
                                    for (int first = 0; first < targets.Length;
                                        first++)
                                    for (int second = first + 1;
                                        second < targets.Length; second++)
                                    {
                                        float separation = targets[first]
                                            .DistanceTo(targets[second]);
                                        float footprints = targets[first]
                                            .Corpulence + targets[second]
                                                .Corpulence;
                                        reached &= separation >= footprints -
                                            .05f && separation <= footprints +
                                            5f * .3048f + .02f;
                                    }
                                    if (attemptedPositions.Count < 8)
                                        attemptedPositions.Add(new JObject {
                                            ["origin"] = SurveyVector(
                                                owner.Position),
                                            ["requested"] = new JArray(
                                                points.Select(SurveyVector)),
                                            ["targets"] = new JArray(
                                                targets.Select(value =>
                                                    SurveyVector(
                                                        value.Position))),
                                            ["nativeReach"] = new JArray(
                                                targets.Select(value =>
                                                    UnitEngagementExtension
                                                        .IsReach(owner, value,
                                                            owner.Body
                                                                .PrimaryHand))) });
                                    if (!reached) continue;
                                    nativeClusters++;
                                    origin = trialOrigin;
                                    lane = lane + ";clusterOrigin=" + origin +
                                        ";clusterDistance=" + distance;
                                    laneReady = true;
                                    break;
                                }
                                if (laneReady) break;
                            }
                            if (!laneReady) throw new InvalidOperationException(
                                "No native three-target Cleave lane: " + lane +
                                ";connectedOrigins=" + connectedOrigins +
                                ";graphClusters=" + graphClusters +
                                ";nativeClusters=" + nativeClusters +
                                ";hand=" + owner.Body.PrimaryHand?.MaybeWeapon?.Blueprint?.AssetGuid +
                                ";weaponRangeMeters=" + owner.Body.PrimaryHand?.MaybeWeapon?.AttackRange.Meters +
                                ";reachFeet=" + owner.Stats.Reach.ModifiedValue +
                                ";ownerCorpulence=" + owner.Corpulence +
                                ";attempts=" + attemptedPositions +
                                ";targets=" + string.Join("|", targets.Select(
                                    value => value.DistanceTo(owner).ToString("F2") +
                                        "/" + value.Corpulence +
                                        "/los=" + owner.HasLOS(value) +
                                        "/reach=" + UnitEngagementExtension.IsReach(
                                            owner, value, owner.Body.PrimaryHand))));
                            PlaceExpandedSummoningUnit(fixture.Hostile,
                                origin - (far - surveyedOrigin).normalized *
                                    30f);
                            foreach (UnitEntityData target in targets)
                            {
                                target.Stats.HitPoints.BaseValue = 100000;
                                target.Descriptor.Damage = 0;
                                owner.Memory.Add(target).Visible = true;
                                target.Memory.Add(owner).Visible = true;
                                target.JoinCombat();
                            }
                            owner.JoinCombat();
                            bool legal = owner.IsVisibleForPlayer &&
                                targets.All(value => owner.IsEnemy(value) &&
                                    owner.HasLOS(value) &&
                                    UnitEngagementExtension.IsReach(owner,
                                        value, owner.Body.PrimaryHand)) &&
                                targets.Zip(targets.Skip(1),
                                    (first, second) =>
                                        first.DistanceTo(second) <=
                                        first.Corpulence +
                                        second.Corpulence +
                                        5f * .3048f + .02f).All(value => value);
                            if (!legal) throw new InvalidOperationException(
                                "Cleave visibility, hostility, reach or adjacency precondition failed.");
                            JObject admission = null;
                            if (turnBased)
                            {
                                admission = new JObject();
                                foreach (int frame in
                                    Sprint22EnrollTurnBasedOwners(
                                        new[] { owner }, targets[0],
                                        admission)) yield return frame;
                                DateTime limit = DateTime.UtcNow
                                    .AddSeconds(30);
                                for (int i = 0; i < 500 &&
                                    DateTime.UtcNow < limit &&
                                    !Sprint18OwnerIsActing(owner); i++)
                                    yield return 0;
                                if (!(bool)admission["admitted"] ||
                                    !Sprint18OwnerIsActing(owner))
                                    throw new InvalidOperationException(
                                        "Exact Cleave owner turn absent: " +
                                        admission);
                            }
                            bool greater = owner.Descriptor.Progression.Features
                                .HasFact(nativeGreater);
                            bool facts = owner.Descriptor.Progression.Features
                                .HasFact(feature) &&
                                !owner.Descriptor.Progression.Features
                                    .HasFact(nativeCleave) &&
                                greater == (key == "fire-giant") &&
                                owner.Descriptor.Abilities.GetAbility(
                                    nativeAction) == null;
                            Ability granted = owner.Descriptor.Abilities
                                .GetAbility(action);
                            if (!facts || granted == null)
                                throw new InvalidOperationException(
                                    "Cleave grant, discriminator or native-action isolation failed.");
                            owner.Stats.AdditionalAttackBonus.BaseValue += 20;
                            int acBefore = owner.Stats.AC.ModifiedValue;
                            long roundBefore = ContextActionUngulateTrample.CurrentRound();
                            var costs = new JArray();
                            command = new UnitUseAbility(
                                new AbilityData(granted),
                                new TargetWrapper(targets[0]));
                            if (!command.CanStart)
                                throw new InvalidOperationException(
                                    "Native private Cleave command cannot start.");
                            EventBus.Subscribe(observer);
                            subscribed = true;
                            using (var economy =
                                new FirearmReviewCosts(owner, costs))
                            {
                                owner.Commands.Run(command);
                                DateTime deadline = DateTime.UtcNow
                                    .AddSeconds(35);
                                int frames = 0;
                                while (frames++ < 3500 &&
                                    DateTime.UtcNow < deadline)
                                {
                                    if (command.ExecutionProcess != null &&
                                        !command.ExecutionProcess.IsEnded)
                                        command.ExecutionProcess.Tick();
                                    yield return 0;
                                    if (command.IsFinished &&
                                        command.ExecutionProcess != null &&
                                        command.ExecutionProcess.IsEnded)
                                        break;
                                }
                                int expected = key == "hill-giant" ? 2 : 3;
                                var rows = observer.Rows;
                                bool allHit = rows.Count == expected &&
                                    rows.All(value =>
                                        value.AttackRoll != null &&
                                        value.AttackRoll.IsHit);
                                int acAfter = owner.Stats.AC.ModifiedValue;
                                int penaltyCount = owner.Descriptor.Buffs
                                    .RawFacts.Count(value =>
                                        value.Blueprint.AssetGuid ==
                                        "914b04c279e3d3447848fecf5796cdcd");
                                var cleavePart = owner.Get<
                                    UnitPartSummonGiantCleave>();
                                bool roundLedger = key == "hill-giant" ?
                                    cleavePart != null &&
                                    cleavePart.LastAdditionalAttackRound >=
                                        roundBefore &&
                                    cleavePart.LastAdditionalAttackRound <=
                                        ContextActionUngulateTrample.CurrentRound() :
                                    cleavePart == null;
                                passed = command.IsStarted &&
                                    command.IsFinished &&
                                    command.ExecutionProcess != null &&
                                    command.ExecutionProcess.IsEnded &&
                                    !command.Cutscene &&
                                    !command.IsIgnoreCooldown &&
                                    allHit &&
                                    rows.Select(value => value.Target.UniqueId)
                                        .Distinct().Count() == expected &&
                                    rows.All(value =>
                                        targets.Contains(value.Target)) &&
                                    acAfter - acBefore == -2 &&
                                    penaltyCount == 1 &&
                                    roundLedger &&
                                    economy.Count(command) == 1 &&
                                    economy.Exact(command, turnBased) &&
                                    action.ActionType.ToString() == "Standard";
                                trials.Add(new JObject {
                                    ["owner"] = owner.UniqueId,
                                    ["targets"] = new JArray(targets.Select(
                                        value => value.UniqueId)),
                                    ["mode"] = turnBased ?
                                        "turn-based" : "rtwp",
                                    ["lane"] = lane,
                                    ["admission"] = admission,
                                    ["attackSetupBonus"] = 20,
                                    ["finished"] = command.IsFinished,
                                    ["executionEnded"] =
                                        command.ExecutionProcess?.IsEnded,
                                    ["frames"] = frames,
                                    ["costs"] = costs,
                                    ["acBefore"] = acBefore,
                                    ["acAfter"] = acAfter,
                                    ["penaltyBuffCount"] = penaltyCount,
                                    ["roundBefore"] = roundBefore,
                                    ["lastAdditionalAttackRound"] =
                                        cleavePart?.LastAdditionalAttackRound,
                                    ["roundLedger"] = roundLedger,
                                    ["attacks"] = new JArray(rows.Select(
                                        value => new JObject {
                                            ["target"] =
                                                value.Target.UniqueId,
                                            ["weapon"] = value.Weapon?
                                                .Blueprint?.AssetGuid,
                                            ["natural"] = value.AttackRoll?
                                                .Roll.Value,
                                            ["bonus"] = value.AttackRoll?
                                                .AttackBonus,
                                            ["hit"] = value.AttackRoll?
                                                .IsHit,
                                            ["damage"] = value.MeleeDamage?
                                                .Damage })) });
                            }
                        }
                        finally
                        {
                            if (subscribed) EventBus.Unsubscribe(observer);
                            EndExpandedSummoningDetachedAbility(command);
                            SettingsRoot.Instance.EnableTurnBasedMode
                                .CurrentValue = false;
                            Game.Instance.TurnBasedCombatController.Activate();
                            foreach (UnitEntityData unit in created)
                            {
                                unit.Commands.InterruptAll(true);
                                if (unit.IsInCombat) unit.LeaveCombat();
                            }
                            Game.Instance.Player.UpdateIsInCombat();
                            DisposeExpandedSummoningUnits(fixture.Created,
                                created);
                            PlaceExpandedSummoningUnit(fixture.Hostile,
                                hostileBefore);
                        }
                    }
                    Sprint25Check("s28-" + key + (turnBased ?
                        "-tb" : "-rtwp") + "-cleave", passed,
                        new JObject { ["attempts"] = trials,
                            ["expectedAttacks"] =
                                key == "hill-giant" ? 2 : 3 },
                        "The project-only native Standard action uses the real owner turn, attack rolls, two/three-target chain, and one -2 AC buff.");
                }
            }
            finally
            {
                PlaceExpandedSummoningUnit(fixture.Caster, casterBefore);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = mode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = pause;
            }
        }
    }
}
