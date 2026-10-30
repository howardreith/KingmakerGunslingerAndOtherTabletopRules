using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // This deliberately small hidden-candidate run answers the two new
        // engine questions before broader Sprint 35 mechanics are expanded.
        // It issues native commands and observes production components; it
        // never publishes a route, supplies a hit, or moves a wall section.
        private IEnumerable<int> ReviewInfernalEarly35(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var trace = new InfernalEvents34();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint35InfernalEarlyPhase,
                ["finalQualification"] = false,
                ["cells"] = cells,
                ["events"] = trace.Rows
            };
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint35-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint35 early native cell failed: " + id);
                };
            try
            {
                var routes = ExpandedSummoningCatalog
                    .GenerateVariants(SummonFamily.Monster)
                    .Where(value => value.Creature.Key == "barbed-devil" ||
                        value.Creature.Key == "bone-devil").ToArray();
                var registrations = ExpandedSummoningBlueprints.Registered.BySymbol
                    .Where(pair => pair.Key.StartsWith(
                            ExpandedSummoningSpecialBuilder.Barbed35Prefix,
                            StringComparison.Ordinal) ||
                        pair.Key.StartsWith(
                            ExpandedSummoningSpecialBuilder.Bone35Prefix,
                            StringComparison.Ordinal)).ToArray();
                var bootstrap = new JObject {
                    ["routeCount"] = routes.Length,
                    ["routes"] = new JArray(routes.Select(value => new JObject {
                        ["key"] = value.Creature.Key,
                        ["tier"] = value.ParentTier,
                        ["quantity"] = value.Multiplicity.ToString(),
                        ["published"] = SummonVisibilityCatalog.IsPublished(value)
                    })),
                    ["privateRegistrationCount"] = registrations.Length,
                    ["privateTypes"] = new JArray(registrations.Select(value =>
                        value.Value == null ? null : value.Value.GetType().Name)
                        .OrderBy(value => value))
                };
                check("hidden-bootstrap", routes.Length == 5 &&
                    routes.All(value => !SummonVisibilityCatalog.IsPublished(value)) &&
                    registrations.Length == 46 &&
                    registrations.All(value => value.Value != null), bootstrap,
                    "Both dependency-complete roots resolve while exactly five routes remain unpublished.");

                receipt["nativeSpellCensus"] = InfernalNativeSpellCensus35(
                    fixture.Blueprints);

                var barbed = CastExpandedSummoningOwnTier(fixture,
                    "barbed-devil");
                var bone = CastExpandedSummoningOwnTier(fixture,
                    "bone-devil");
                var reach = CastExpandedSummoningOwnTier(fixture,
                    InfernalVisualPolicy34.Barbazu);
                owned.Add(barbed); owned.Add(bone); owned.Add(reach);
                foreach (var actor in owned)
                {
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    actor.Commands.InterruptAll(true);
                    actor.Commands.RemoveFinishedAndUpdateQueue();
                    foreach (int frame in Outsider30NativeAppearance(actor,
                        actor.Blueprint.name)) yield return frame;
                }

                var profiles = new JArray(new[] { barbed, bone }.Select(actor =>
                    new JObject {
                        ["owner"] = actor.UniqueId,
                        ["blueprint"] = actor.Blueprint.AssetGuid,
                        ["size"] = actor.Descriptor.State.Size.ToString(),
                        ["hp"] = actor.Stats.HitPoints.ModifiedValue,
                        ["ac"] = actor.Stats.AC.ModifiedValue,
                        ["bab"] = actor.Stats.BaseAttackBonus.ModifiedValue,
                        ["fortitude"] = actor.Stats.SaveFortitude.ModifiedValue,
                        ["reflex"] = actor.Stats.SaveReflex.ModifiedValue,
                        ["will"] = actor.Stats.SaveWill.ModifiedValue,
                        ["speedFeet"] = actor.Stats.Speed.ModifiedValue,
                        ["body"] = ExpandedSummoningPteranodonViewPatch
                            .DescribeView(actor.View),
                        ["primary"] = actor.Body.PrimaryHand.MaybeWeapon?
                            .Blueprint?.AssetGuid,
                        ["additional"] = new JArray(actor.Body.AdditionalLimbs
                            .Select(slot => slot.MaybeWeapon?.Blueprint?.AssetGuid)),
                        ["abilities"] = new JArray(actor.Descriptor.Abilities
                            .Enumerable.Select(value => value.Blueprint.name)
                            .Where(value => value.StartsWith("KMG_", StringComparison.Ordinal)))
                    }));
                receipt["profiles"] = profiles;
                check("hidden-profile-resolution",
                    InfernalMartialProfile35.For(barbed)?.Claw != null &&
                    InfernalMartialProfile35.For(bone)?.Bite != null &&
                    InfernalMartialProfile35.For(bone)?.Claw != null &&
                    InfernalMartialProfile35.For(bone)?.Sting != null,
                    new JObject { ["profiles"] = profiles },
                    "The two hidden roots instantiate their exact production martial profiles and granted actions.");

                var oldGroup = reach.GroupId;
                reach.GroupId = "KMG_S35_Early_Reach_" + reach.UniqueId;
                reach.Descriptor.SwitchFactions(fixture.Hostile.Faction, true);
                reach.UpdateGroup();
                var reachProfile = InfernalMartialProfile34.For(reach);
                foreach (int frame in InfernalSwitch34(reach,
                    reachProfile.Claw)) yield return frame;
                foreach (int frame in InfernalEarlyContact35(fixture, reach,
                    barbed, trace, check, "ordinary-claw", 3, true))
                    yield return frame;
                foreach (int frame in InfernalSwitch34(reach,
                    reachProfile.Glaive)) yield return frame;
                foreach (int frame in InfernalEarlyContact35(fixture, reach,
                    barbed, trace, check, "reach-glaive", 8, false))
                    yield return frame;
                receipt["contactOriginalGroup"] = oldGroup;

                foreach (int frame in InfernalEarlyWall35(fixture, bone,
                    owned, check)) yield return frame;
            }
            finally
            {
                trace.Dispose();
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory, "sprint35-infernal-early.json"),
                    receipt.ToString(Formatting.Indented));
                foreach (var actor in owned.Where(value => value != null &&
                    !value.Destroyed))
                {
                    actor.Commands.InterruptAll(true);
                    actor.Commands.RemoveFinishedAndUpdateQueue();
                    if (actor.IsInCombat) actor.LeaveCombat();
                }
                Game.Instance.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created, owned);
                ResetExpandedSummoningHostile(fixture);
            }
        }

        private static JArray InfernalNativeSpellCensus35(
            IEnumerable<BlueprintScriptableObject> blueprints)
        {
            string[] needles = { "DimensionalAnchor", "MajorImage",
                "ProduceFlame", "Pyrotechnics", "OrdersWrath",
                "UnholyBlight", "WallOfIce" };
            var result = new JArray();
            foreach (string needle in needles)
            {
                string key = needle.ToLowerInvariant();
                var matches = blueprints.OfType<BlueprintAbility>()
                    .Where(value => !value.name.StartsWith("KMG_",
                            StringComparison.Ordinal) &&
                        ((value.name ?? "") + " " +
                         (value.Name == null ? "" : value.Name.ToString()))
                        .Replace("_", "").Replace(" ", "")
                        .ToLowerInvariant().Contains(key))
                    .Take(12).ToArray();
                result.Add(new JObject {
                    ["term"] = needle,
                    ["matches"] = new JArray(matches.Select(value =>
                        new JObject {
                            ["guid"] = value.AssetGuid,
                            ["name"] = value.name,
                            ["displayName"] = value.Name == null ? null :
                                value.Name.ToString(),
                            ["type"] = value.Type.ToString(),
                            ["components"] = new JArray((value.ComponentsArray ??
                                Array.Empty<BlueprintComponent>()).Select(
                                    component => component.GetType().Name))
                        }))
                });
            }
            return result;
        }

        private IEnumerable<int> InfernalEarlyContact35(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData attacker, UnitEntityData defender,
            InfernalEvents34 trace,
            Action<string, bool, JObject, string> check,
            string label, int feet, bool expectDefense)
        {
            bool observedHit = false;
            var attempts = new JArray();
            for (int attempt = 0; attempt < 8 && !observedHit; attempt++)
            {
                attacker.Commands.InterruptAll(true);
                attacker.Commands.RemoveFinishedAndUpdateQueue();
                InfernalCombatLane34(fixture, attacker, defender, feet);
                int first = trace.Rows.Count;
                var command = new UnitAttack(defender) {
                    IsSingleAttack = true, ForceFullAttack = false };
                attacker.Commands.Run(command);
                foreach (int frame in WaitInfernalCommand34(command,
                    () => true, 25)) yield return frame;
                var attackRows = trace.Rows.Skip(first).Where(row =>
                    (string)row["event"] == "attack" &&
                    (string)row["owner"] == attacker.UniqueId &&
                    (string)row["target"] == defender.UniqueId).ToArray();
                bool hit = attackRows.Any(row => (bool?)row["hit"] == true);
                var reactions = trace.Rows.Skip(first).Where(row =>
                    (string)row["event"] == "damage" &&
                    (string)row["owner"] == defender.UniqueId &&
                    (string)row["target"] == attacker.UniqueId &&
                    (bool?)row["hasAttackRoll"] == false).ToArray();
                var recursiveAttacks = trace.Rows.Skip(first).Where(row =>
                    (string)row["event"] == "attack" &&
                    (string)row["owner"] == defender.UniqueId &&
                    (string)row["target"] == attacker.UniqueId).ToArray();
                attempts.Add(new JObject {
                    ["attempt"] = attempt,
                    ["command"] = Sprint27AiCommand(command),
                    ["weapon"] = attacker.Body.PrimaryHand.MaybeWeapon?
                        .Blueprint?.AssetGuid,
                    ["rawWeaponReachFeet"] = attacker.Body.PrimaryHand
                        .MaybeWeapon?.Blueprint?.Type?.AttackRange.Value,
                    ["attacks"] = new JArray(attackRows),
                    ["reactions"] = new JArray(reactions),
                    ["recursiveAttacks"] = new JArray(recursiveAttacks)
                });
                if (!hit) continue;
                observedHit = true;
                bool pass = command.Result == UnitCommand.ResultType.Success &&
                    attackRows.Length == 1 && recursiveAttacks.Length == 0 &&
                    reactions.Length == (expectDefense ? 1 : 0) &&
                    (!expectDefense || (int?)reactions[0]["actual"] > 0);
                check("barbed-defense-" + label, pass,
                    new JObject {
                        ["attacker"] = attacker.UniqueId,
                        ["defender"] = defender.UniqueId,
                        ["expectDefense"] = expectDefense,
                        ["attempts"] = attempts,
                        ["forcedRolls"] = 0
                    }, expectDefense ?
                    "A real non-reach melee hit receives exactly one direct, nonrecursive Barbed Defense response." :
                    "A real manufactured reach-weapon hit is exempt from Barbed Defense; body distance does not replace weapon reach.");
            }
            if (!observedHit) throw new InvalidOperationException(
                "Unobserved precondition: no native " + label +
                " hit in eight bounded attempts.");
        }

        private IEnumerable<int> InfernalEarlyWall35(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData bone, List<UnitEntityData> owned,
            Action<string, bool, JObject, string> check)
        {
            Vector3 start, midpoint, destination;
            JArray laneAttempts;
            if (!InfernalEarlyWallLane35(fixture, out start, out midpoint,
                    out destination, out laneAttempts))
                throw new InvalidOperationException(
                    "No direct native floor lane for the early wall candidate.");
            PlaceExpandedSummoningUnit(fixture.Caster, start);
            Vector3 direction = (destination - start).normalized;
            Vector3 side = new Vector3(-direction.z, 0f, direction.x);
            var casterNode = ObstacleAnalyzer.GetNearestNode(
                start + side * 5f);
            if (casterNode.node == null || !casterNode.node.Walkable)
                throw new InvalidOperationException(
                    "No supported off-lane Bone Devil casting point.");
            PlaceExpandedSummoningUnit(bone, casterNode.clampedPosition);
            var before = InfernalEarlyPath35(fixture.Caster, destination);
            bool losBefore = Kingmaker.Visual.FogOfWar.LineOfSightGeometry
                .Instance.HasObstacle(start, destination, 0);

            var wallBlueprint = fixture.Blueprints.OfType<BlueprintAbility>()
                .Single(value => value.name ==
                    "KMG_Summoning_Infernal_BoneDevil_WallOfIce");
            var grant = bone.Descriptor.Abilities.GetAbility(wallBlueprint);
            if (grant == null) throw new InvalidOperationException(
                "Hidden Bone Devil lacks its registered wall grant.");
            var data = new AbilityData(grant);
            var target = new TargetWrapper(midpoint);
            bool targetable = data.IsAvailable && data.CanTarget(target);
            var sectionBlueprint = fixture.Blueprints.OfType<BlueprintUnit>()
                .Single(value => value.name ==
                    "KMG_Summoning_Infernal_BoneDevil_WallSectionUnit");
            var prior = new HashSet<string>(Game.Instance.State.Units
                .Where(value => ReferenceEquals(value.Blueprint,
                    sectionBlueprint)).Select(value => value.UniqueId));
            var command = new UnitUseAbility(data, target);
            bone.Commands.Run(command);
            DateTime deadline = DateTime.UtcNow.AddSeconds(25);
            UnitEntityData section = null;
            while (DateTime.UtcNow < deadline &&
                (!command.IsFinished || section == null))
            {
                Game.Instance.IsPaused = false;
                yield return 0;
                section = Game.Instance.State.Units.FirstOrDefault(value =>
                    ReferenceEquals(value.Blueprint, sectionBlueprint) &&
                    !prior.Contains(value.UniqueId));
            }
            if (section != null) owned.Add(section);
            var withWall = InfernalEarlyPath35(fixture.Caster, destination);
            bool losWith = Kingmaker.Visual.FogOfWar.LineOfSightGeometry
                .Instance.HasObstacle(start, destination, 0);
            float sectionOffset = section == null ? float.PositiveInfinity :
                InfernalEarlyDistanceToSegment35(section.Position, start,
                    destination);
            bool obstruction = section != null &&
                ((bool?)withWall["available"] == false ||
                 (double?)withWall["length"] >
                    (double?)before["length"] + .15 ||
                 (int?)withWall["points"] > (int?)before["points"] ||
                 (string)withWall["standingObstacleResult"] !=
                    (string)before["standingObstacleResult"]);

            bool hadSummonBuff = false;
            if (section != null)
            {
                var summoned = section.Buffs.GetBuff(BlueprintRoot.Instance
                    .SystemMechanics.SummonedUnitBuff);
                hadSummonBuff = summoned != null;
                if (summoned != null) summoned.Remove();
                deadline = DateTime.UtcNow.AddSeconds(15);
                while (DateTime.UtcNow < deadline && !section.Destroyed &&
                    section.IsInGame)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
            }
            var after = InfernalEarlyPath35(fixture.Caster, destination);
            bool losAfter = Kingmaker.Visual.FogOfWar.LineOfSightGeometry
                .Instance.HasObstacle(start, destination, 0);
            bool restored = (bool?)before["available"] == true &&
                (bool?)after["available"] == true &&
                Math.Abs((double)after["length"] -
                    (double)before["length"]) <= .15 &&
                (string)after["standingObstacleResult"] ==
                    (string)before["standingObstacleResult"];
            var row = new JObject {
                ["owner"] = bone.UniqueId,
                ["ability"] = wallBlueprint.AssetGuid,
                ["command"] = Sprint27AiCommand(command),
                ["targetable"] = targetable,
                ["start"] = Vec(start),
                ["midpoint"] = Vec(midpoint),
                ["destination"] = Vec(destination),
                ["laneAttempts"] = laneAttempts,
                ["section"] = section?.UniqueId,
                ["sectionBlueprint"] = section?.Blueprint?.AssetGuid,
                ["sectionOffsetFromLane"] = sectionOffset,
                ["sectionHitPoints"] = section?.Stats.HitPoints.ModifiedValue,
                ["summonedLifetimeBuff"] = hadSummonBuff,
                ["sectionRetired"] = section != null &&
                    (section.Destroyed || !section.IsInGame),
                ["pathBefore"] = before,
                ["pathWithWall"] = withWall,
                ["pathAfter"] = after,
                ["movementObstructionObserved"] = obstruction,
                ["navigationRestored"] = restored,
                ["lineOfSightObstacleBefore"] = losBefore,
                ["lineOfSightObstacleWithWall"] = losWith,
                ["lineOfSightObstacleAfter"] = losAfter,
                ["lineOfSightChanged"] = losWith != losBefore,
                ["formationGeometry"] = "single-target-point-section",
                ["wallBreachResidualImplemented"] = false
            };
            check("wall-section-viability", targetable &&
                command.Result == UnitCommand.ResultType.Success &&
                section != null && sectionOffset <= .75f &&
                hadSummonBuff && obstruction && restored, row,
                "A real hidden cast places one temporary attackable section on a measured lane, changes native standing-obstacle pathing, retires through native lifetime ownership, and restores navigation; targeting/LoS is recorded separately.");
        }

        private bool InfernalEarlyWallLane35(
            ExpandedSummoningCorrectionFixture fixture, out Vector3 start,
            out Vector3 midpoint, out Vector3 destination,
            out JArray attempts)
        {
            attempts = new JArray();
            start = midpoint = destination = Vector3.zero;
            Vector3 centre = Sprint25IsolatedAnchor(fixture);
            for (int index = 0; index < 24; index++)
            {
                Vector3 direction = Quaternion.Euler(0f, index * 15f, 0f) *
                    Vector3.forward;
                var from = ObstacleAnalyzer.GetNearestNode(
                    centre - direction * 5f);
                var to = ObstacleAnalyzer.GetNearestNode(
                    centre + direction * 5f);
                bool nodes = from.node != null && to.node != null &&
                    from.node.Walkable && to.node.Walkable &&
                    Vector3.Distance(from.clampedPosition,
                        centre - direction * 5f) < .3f &&
                    Vector3.Distance(to.clampedPosition,
                        centre + direction * 5f) < .3f;
                Vector3 traced = nodes ? ObstacleAnalyzer.TraceAlongNavmesh(
                    from.clampedPosition, to.clampedPosition) : Vector3.zero;
                bool direct = nodes && Vector3.Distance(traced,
                    to.clampedPosition) < .1f;
                attempts.Add(new JObject {
                    ["index"] = index,
                    ["nodes"] = nodes,
                    ["direct"] = direct,
                    ["from"] = nodes ? Vec(from.clampedPosition) : null,
                    ["to"] = nodes ? Vec(to.clampedPosition) : null,
                    ["traceGap"] = nodes ?
                        Vector3.Distance(traced, to.clampedPosition) : -1f
                });
                if (!direct) continue;
                start = from.clampedPosition;
                destination = to.clampedPosition;
                midpoint = (start + destination) * .5f;
                return true;
            }
            return false;
        }

        private static JObject InfernalEarlyPath35(UnitEntityData mover,
            Vector3 destination)
        {
            var agent = mover.View == null ? null :
                mover.View.MovementAgent as UnitMovementAgent;
            if (agent == null) return new JObject {
                ["available"] = false, ["reason"] = "no-agent" };
            Pathfinding.Path path = agent.FindPath(destination,
                new OnPathDelegate(value => { }), null, false, 0);
            if (path == null) return new JObject {
                ["available"] = false, ["reason"] = "no-path-request" };
            AstarPath.WaitForPath(path);
            string standing = "not-run";
            if (!path.error)
                standing = ObstaclePathfinder.PathAroundStandingObstacles(path,
                    agent, null).ToString();
            var points = path.vectorPath == null ?
                new List<Vector3>() : path.vectorPath;
            double length = 0;
            for (int index = 1; index < points.Count; index++)
                length += Vector3.Distance(points[index - 1], points[index]);
            return new JObject {
                ["available"] = !path.error && points.Count > 1 &&
                    standing != ObstaclePathingResult.NoPath.ToString(),
                ["error"] = path.error,
                ["errorLog"] = path.errorLog,
                ["points"] = points.Count,
                ["length"] = length,
                ["standingObstacleResult"] = standing,
                ["vector"] = new JArray(points.Select(Vec))
            };
        }

        private static float InfernalEarlyDistanceToSegment35(Vector3 point,
            Vector3 start, Vector3 end)
        {
            Vector3 line = end - start;
            line.y = 0f;
            Vector3 offset = point - start;
            offset.y = 0f;
            float fraction = line.sqrMagnitude <= .0001f ? 0f :
                Mathf.Clamp01(Vector3.Dot(offset, line) / line.sqrMagnitude);
            return Vector3.Distance(start + line * fraction,
                new Vector3(point.x, start.y, point.z));
        }
    }
}
