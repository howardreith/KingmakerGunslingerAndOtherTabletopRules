using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Controllers.Combat;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GraphNode = Pathfinding.GraphNode;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewIceDevilWalls40(ExpandedSummoningCorrectionFixture fixture)
        {
            var actors = new List<UnitEntityData>(); var sections = new List<UnitEntityData>();
            var originals = new Dictionary<GraphNode, bool>(); var bindings = new List<InfernalWallNavmesh35>();
            var cells = new JArray(); var trace = new InfernalEvents34();
            bool recipientFearIsolation = false;
            int baselineClaims = InfernalWallNavmesh35.ActiveGridClaimCount;
            Vector3 surveyCenter = fixture.Caster.Position;
            var ownerParking = new JArray();
            var receipt = new JObject { ["cells"] = cells, ["events"] = trace.Rows,
                ["fixtureFearIsolation"] = true, ["baselineClaims"] = baselineClaims,
                ["ownerParking"] = ownerParking,
                ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilWallsPhase };
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Sprint40 wall: " + id);
            };
            try
            {
                fixture.Hostile.Descriptor.State.AddConditionImmunity(UnitCondition.Frightened);
                recipientFearIsolation = true;
                foreach (string key in new[] { "ice-devil", "ice-devil", "bone-devil" })
                {
                    var actor = CastExpandedSummoningOwnTier(fixture, key); actors.Add(actor);
                    actor.Descriptor.State.AddConditionImmunity(UnitCondition.Frightened);
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    // Finish native materialization at the ordinary summon
                    // point while it is visible to the party, then park the
                    // settled source on the real connected walking graph. A
                    // remote fogged point cannot satisfy the appearance gate.
                    foreach (int frame in Outsider30NativeAppearance(actor, actor.Blueprint.name)) yield return frame;
                    var walking = Sprint40WalkingConstraint(actor);
                    var anchor = AstarPath.active.GetNearest(fixture.Caster.Position, walking);
                    if (anchor.node == null || !anchor.node.Walkable)
                        throw new InvalidOperationException("No native wall-owner walking anchor.");
                    walking.SetStart(anchor.node); walking.constrainArea = true;
                    float minimumRadius = 8f + actors.Count * 4f;
                    bool parked = false;
                    for (float radius = minimumRadius; radius <= minimumRadius + 12f && !parked; radius += 3f)
                    for (int angle = 0; angle < 16 && !parked; angle++)
                    {
                        Vector3 requested = fixture.Caster.Position +
                            Quaternion.Euler(0f, angle * 22.5f, 0f) * Vector3.forward * radius;
                        var point = AstarPath.active.GetNearest(requested, walking);
                        Vector3 projection = requested - point.clampedPosition; projection.y = 0f;
                        if (point.node == null || !point.node.Walkable ||
                            !walking.Suitable(point.node) || projection.magnitude > 1.5f) continue;
                        Vector3 horizontal = point.clampedPosition - fixture.Caster.Position; horizontal.y = 0f;
                        if (horizontal.magnitude < minimumRadius - 1.5f ||
                            actors.Take(actors.Count - 1).Any(other => {
                                Vector3 gap = other.Position - point.clampedPosition; gap.y = 0f;
                                return gap.magnitude < 3f;
                            })) continue;
                        PlaceExpandedSummoningUnit(actor, point.clampedPosition);
                        ownerParking.Add(new JObject {
                            ["actor"] = actor.UniqueId, ["requested"] = Vec(requested),
                            ["clamped"] = Vec(point.clampedPosition), ["radius"] = radius,
                            ["angleDegrees"] = angle * 22.5f,
                            ["horizontalProjectionGap"] = projection.magnitude,
                            ["graph"] = point.node.GraphIndex, ["tags"] = walking.tags });
                        parked = true;
                    }
                    if (!parked) throw new InvalidOperationException(
                        "No bounded connected wall-owner parking point.");
                }
                // Test the two required owner pairs sequentially. Three
                // simultaneous legal sections are not a wall contract.
                for (int index = 0; index < 2; index++)
                {
                    UnitEntityData section = null;
                    foreach (int frame in IceDevilCastWall40(fixture, actors[index], actors, surveyCenter,
                        value => section = value, check, "owner-" + index)) yield return frame;
                    sections.Add(section);
                    var binding = section.View.GetComponent<InfernalWallNavmesh35>(); bindings.Add(binding);
                    foreach (var pair in binding.GridNodeSnapshot())
                    {
                        bool prior;
                        if (originals.TryGetValue(pair.Key, out prior) && prior != pair.Value)
                            throw new InvalidOperationException("Shared node original differs between exact owners.");
                        originals[pair.Key] = pair.Value;
                    }
                }
                check("wall-two-ice-owner-claims", bindings.Count == 2 && bindings.All(x => x.GridApplyCompleted &&
                    x.GridNodesClaimed > 0 && x.GridNodeSnapshot().All(n => !n.Key.Walkable)),
                    new JObject { ["owners"] = new JArray(actors.Select(x => x.UniqueId)),
                        ["sections"] = new JArray(sections.Select(x => x.UniqueId)),
                        ["claims"] = InfernalWallNavmesh35.ActiveGridClaimCount,
                        ["uniqueOriginalNodes"] = originals.Count },
                    "Two Ice sources own distinct real casts and live node claims; shared original bits agree.");
                // Exercise the native object-damage consumer rather than
                // deleting a section or invoking its breach method.
                var physical = new PhysicalDamage(new DiceFormula(0, DiceType.Zero), PhysicalDamageForm.Bludgeoning);
                physical.AddBonus(20);
                var injury = Rulebook.Trigger(new RuleDealDamage(fixture.Hostile,
                    sections[0], new DamageBundle(physical)));
                check("wall-native-object-half", physical.Half && injury.Damage > 0 &&
                    sections[0].Damage == injury.Damage && !sections[0].Descriptor.State.IsDead,
                    new JObject { ["input"] = 20, ["half"] = physical.Half,
                        ["actual"] = injury.Damage, ["remainingHp"] = sections[0].HPLeft,
                        ["directNativeObjectDamageConsumer"] = true },
                    "Native physical object damage is halved and debits the real39 HP section.");
                var fire = new EnergyDamage(new DiceFormula(0, DiceType.Zero), DamageEnergyType.Fire);
                fire.AddBonus(1000);
                var breach = Rulebook.Trigger(new RuleDealDamage(fixture.Hostile,
                    sections[0], new DamageBundle(fire)));
                foreach (int frame in InfernalBoneWaitUntil35(() => bindings[0] == null ||
                    bindings[0].GridRestoreCompleted, 12)) yield return frame;
                var firstAbility = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.IceDevil40Prefix + "WallOfIce"];
                var residualBp = (BlueprintAbilityAreaEffect)ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.IceDevil40Prefix + "WallResidualArea"];
                var residual = Sprint29OwnedAreas(actors[0], residualBp);
                var fog = Game.Instance.State.AreaEffects.All.Where(x => !x.IsEnded &&
                    ReferenceEquals(x.Context?.MaybeCaster, actors[0]) &&
                    ReferenceEquals(x.Context?.SourceAbility, firstAbility) &&
                    !ReferenceEquals(x.Blueprint, residualBp)).ToArray();
                check("wall-fire-breach-and-other-claims", !fire.Half && breach.Damage > 0 &&
                    residual.Length == 1 && fog.Length == 1 &&
                    bindings.Skip(1).All(x => x.GridNodeSnapshot().All(n => !n.Key.Walkable)) &&
                    sections.Skip(1).All(x => !x.Destroyed && !x.Descriptor.State.IsDead),
                    new JObject { ["fireHalf"] = fire.Half, ["actualFire"] = breach.Damage,
                        ["residual"] = residual.Length, ["fog"] = fog.Length,
                        ["remainingClaims"] = InfernalWallNavmesh35.ActiveGridClaimCount,
                        ["otherSectionsAlive"] = sections.Skip(1).Count(x => !x.Destroyed && !x.Descriptor.State.IsDead) },
                    "Native fire breach releases only the first section and creates its residual cold/fire-fog branch; second Ice claims survive.");
                Vector3 center = sections[0].Position;
                Vector3 forward = sections[0].OrientationDirection;
                var from = ObstacleAnalyzer.GetNearestNode(center - forward * 3f);
                var to = ObstacleAnalyzer.GetNearestNode(center + forward * 3f);
                PlaceExpandedSummoningUnit(fixture.Hostile, from.clampedPosition);
                int crossingStart = trace.Rows.Count;
                var walk = new UnitMoveTo(to.clampedPosition); fixture.Hostile.Commands.Run(walk);
                foreach (int frame in WaitInfernalCommand34(walk, () => trace.Rows.Skip(crossingStart).Any(x =>
                    (string)x["event"] == "damage" && (string)x["owner"] == actors[0].UniqueId &&
                    (string)x["target"] == fixture.Hostile.UniqueId && (int?)x["actual"] > 0), 20)) yield return frame;
                var coldRows = trace.Rows.Skip(crossingStart).Where(x => (string)x["event"] == "damage" &&
                    (string)x["owner"] == actors[0].UniqueId &&
                    (string)x["target"] == fixture.Hostile.UniqueId).ToArray();
                check("wall-residual-native-crossing", walk.Result == UnitCommand.ResultType.Success &&
                    coldRows.Any(x => (int?)x["actual"] > 0 && x["packets"].Any(packet =>
                        (string)packet["energy"] == "Cold" && (string)packet["dice"] == "1d6" &&
                        (int?)packet["bonus"] == 13)),
                    new JObject { ["command"] = Sprint27AiCommand(walk),
                        ["events"] = new JArray(coldRows), ["directedNativeMovement"] = true },
                    "A real native move through the released breach emits its own source's1d6+13 cold damage; the observer never applies the residual effect.");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { actors[0] });
                foreach (int frame in InfernalBoneWaitUntil35(() =>
                    Sprint29OwnedAreas(actors[0], residualBp).Length == 0 &&
                    fog.All(x => x.IsEnded), 12)) yield return frame;
                check("wall-first-source-cleanup-isolated", sections.Skip(1).All(x =>
                    !x.Destroyed && !x.Descriptor.State.IsDead) &&
                    bindings.Skip(1).All(x => x.GridNodeSnapshot().All(n => !n.Key.Walkable)),
                    new JObject { ["firstSourceDestroyed"] = actors[0].Destroyed,
                        ["secondIceAlive"] = !sections[1].Destroyed },
                    "First exact source removal cleans its residual and fog without releasing the second Ice source's movement claims.");
                UnitEntityData boneSection = null;
                foreach (int frame in IceDevilCastWall40(fixture, actors[2], actors, surveyCenter,
                    value => boneSection = value, check, "bone-after-breach")) yield return frame;
                sections.Add(boneSection);
                var boneBinding = boneSection.View.GetComponent<InfernalWallNavmesh35>();
                bindings.Add(boneBinding);
                foreach (var pair in boneBinding.GridNodeSnapshot())
                {
                    bool prior;
                    if (originals.TryGetValue(pair.Key, out prior) && prior != pair.Value)
                        throw new InvalidOperationException("Cross-species shared node original differs.");
                    originals[pair.Key] = pair.Value;
                }
                check("wall-cross-species-live-claims", bindings.Skip(1).All(x =>
                    x.GridApplyCompleted && x.GridNodesClaimed > 0 &&
                    x.GridNodeSnapshot().All(n => !n.Key.Walkable)),
                    new JObject { ["ice"] = sections[1].UniqueId, ["bone"] = boneSection.UniqueId,
                        ["claims"] = InfernalWallNavmesh35.ActiveGridClaimCount },
                    "An unchanged Bone36 HP/CL12 cast and the second Ice39 HP/CL13 section hold live claims together after the first source retires.");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { actors[1] });
                foreach (int frame in InfernalBoneWaitUntil35(() => sections[1].Destroyed ||
                    !sections[1].IsInGame, 12)) yield return frame;
                check("wall-cross-species-cleanup-isolated", !sections[2].Destroyed &&
                    bindings[2].GridNodeSnapshot().All(n => !n.Key.Walkable),
                    new JObject { ["boneHp"] = sections[2].Stats.HitPoints.BaseValue,
                        ["boneClaims"] = bindings[2].GridNodesClaimed },
                    "Second Ice source cleanup leaves Bone's unchanged36 HP wall and native node claims intact.");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { actors[2] });
                foreach (int frame in InfernalBoneWaitUntil35(() =>
                    InfernalWallNavmesh35.ActiveGridClaimCount == baselineClaims &&
                    originals.All(x => x.Key.Walkable == x.Value), 12)) yield return frame;
                check("wall-all-original-bits-restored", originals.Count > 0 &&
                    originals.All(x => x.Key.Walkable == x.Value) &&
                    InfernalWallNavmesh35.ActiveGridClaimCount == baselineClaims,
                    new JObject { ["nodes"] = originals.Count, ["claims"] = InfernalWallNavmesh35.ActiveGridClaimCount },
                    "All three exact sources retire normally; every recorded original node bit and the preexisting claim count are restored.");
            }
            finally
            {
                trace.Dispose();
                foreach (var actor in actors.Where(x => !x.Destroyed))
                    actor.Descriptor.State.RemoveConditionImmunity(UnitCondition.Frightened);
                if (recipientFearIsolation)
                    fixture.Hostile.Descriptor.State.RemoveConditionImmunity(UnitCondition.Frightened);
                DisposeExpandedSummoningUnits(fixture.Created, actors.Concat(sections));
                ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "sprint40-ice-devil-walls.json"), receipt.ToString(Formatting.Indented));
            }
        }

        private IEnumerable<int> IceDevilCastWall40(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData source, List<UnitEntityData> actors, Vector3 surveyCenter, Action<UnitEntityData> capture,
            Action<string, bool, JObject, string> check, string label)
        {
            bool ice = IceDevilMartialProfile40.For(source) != null;
            string prefix = ice ? ExpandedSummoningSpecialBuilder.IceDevil40Prefix :
                ExpandedSummoningSpecialBuilder.Bone35Prefix;
            var spell = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "WallOfIce"];
            var sectionBp = (BlueprintUnit)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "WallSectionUnit"];
            var data = new AbilityData(source.Descriptor.Abilities.GetAbility(spell));
            Vector3 start, middle, end, casterPoint; JArray attempts; JObject survey;
            int admittedCandidates = 0, availableCandidates = 0, legalCandidates = 0;
            Func<Vector3, Vector3, bool> admit = (point, offLane) => {
                for (int index = 0; index < actors.Count; index++)
                    if (!actors[index].Destroyed)
                        PlaceExpandedSummoningUnit(actors[index], offLane + Vector3.forward * ((index + 1) * 4f));
                PlaceExpandedSummoningUnit(source, offLane);
                admittedCandidates++;
                bool available = data.IsAvailable;
                if (available) availableCandidates++;
                bool legal = available && data.CanTarget(new TargetWrapper(point));
                if (legal) legalCandidates++;
                return legal;
            };
            bool found = InfernalEarlyWallLane35(fixture, actors, out start, out middle, out end,
                out casterPoint, out attempts, out survey, admit, surveyCenter);
            check("wall-survey-" + label, found && legalCandidates > 0,
                new JObject { ["source"] = source.UniqueId, ["surveyCenter"] = Vec(surveyCenter),
                    ["sourcePosition"] = Vec(source.Position), ["sourceCanAct"] = source.Descriptor.State.CanAct,
                    ["nativeAvailable"] = data.IsAvailable, ["geometryCandidates"] = admittedCandidates,
                    ["availableCandidates"] = availableCandidates, ["legalCandidates"] = legalCandidates,
                    ["boundedSurvey"] = attempts, ["nativeSurvey"] = survey },
                "Existing bounded survey retains native availability/targeting; refusal records the actual admission boundary rather than forcing a placement.");
            PlaceExpandedSummoningUnit(fixture.Caster, start);
            var prior = new HashSet<string>(Game.Instance.State.Units.Select(x => x.UniqueId));
            var command = new UnitUseAbility(data, new TargetWrapper(middle)); source.Commands.Run(command);
            UnitEntityData section = null; DateTime deadline = DateTime.UtcNow.AddSeconds(25);
            while (DateTime.UtcNow < deadline && (!command.IsFinished || section == null))
            {
                Game.Instance.IsPaused = false; yield return 0;
                section = Game.Instance.State.Units.FirstOrDefault(x => !prior.Contains(x.UniqueId) &&
                    ReferenceEquals(x.Blueprint, sectionBp));
            }
            capture(section);
            var binding = section?.View?.GetComponent<InfernalWallNavmesh35>();
            if (binding != null)
                foreach (int frame in InfernalBoneWaitUntil35(() => binding.GridApplyCompleted, 12)) yield return frame;
            var lifetime = section?.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            int cl = ice ? 13 : 12;
            check("wall-cast-" + label, section != null && binding != null &&
                command.Result == UnitCommand.ResultType.Success &&
                section.Stats.HitPoints.BaseValue == cl * 3 &&
                lifetime?.Context?.Params.CasterLevel == cl &&
                ReferenceEquals(lifetime.Context.MaybeCaster, source) &&
                lifetime.TimeLeft.TotalSeconds > cl * 60 - 30 && lifetime.TimeLeft.TotalSeconds <= cl * 60,
                new JObject { ["source"] = source.UniqueId, ["section"] = section?.UniqueId,
                    ["hp"] = section?.Stats.HitPoints.BaseValue,
                    ["cl"] = lifetime?.Context?.Params.CasterLevel,
                    ["remainingSeconds"] = lifetime?.TimeLeft.TotalSeconds,
                    ["nodeClaims"] = binding?.GridNodesClaimed, ["command"] = Sprint27AiCommand(command) },
                "Actual cast retains its own source/context/CL,3 HP per CL and10 rounds per CL; Bone's released parameters remain12/36/120 rounds.");
        }
    }
}
