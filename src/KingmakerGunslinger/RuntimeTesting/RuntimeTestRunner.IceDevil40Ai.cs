using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewIceDevilAi40(ExpandedSummoningCorrectionFixture fixture)
        {
            var trace = new InfernalEvents34(); var cells = new JArray();
            var receipt = new JObject { ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilAiPhase,
                ["cells"] = cells, ["events"] = trace.Rows };
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Sprint40 AI: " + id);
            };
            try
            {
                foreach (string spell in new[] { "ConeOfCold", "IceStorm", "WallOfIce" })
                    foreach (int frame in IceDevilAiCase40(fixture, spell,
                        spell == "IceStorm", trace, check)) yield return frame;
            }
            finally
            {
                trace.Dispose();
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "sprint40-ice-devil-ai.json"), receipt.ToString(Formatting.Indented));
            }
        }

        private IEnumerable<int> IceDevilAiCase40(ExpandedSummoningCorrectionFixture fixture,
            string suffix, bool turnBased, InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            string id = "ai-" + suffix + (turnBased ? "-turn" : "-rtwp");
            CreateSprint32TurnHostile(fixture);
            UnitEntityData owner = null, friend = null; var target = fixture.Hostile;
            var anchor = Sprint32OwnedQuietAnchor(fixture, id);
            BlueprintFeature cold = null; bool addedCold = false, isolatedFear = false;
            bool sourceRetired = false;
            BlueprintFaction friendFaction = null;
            var commands = new Lillend32CommandStarts();
            string p = ExpandedSummoningSpecialBuilder.IceDevil40Prefix;
            var stormArea = (BlueprintAbilityAreaEffect)ExpandedSummoningBlueprints.Registered.BySymbol[p + "IceStormArea"];
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                target.Descriptor.State.AddConditionImmunity(UnitCondition.Frightened); isolatedFear = true;
                owner = CastExpandedSummoningOwnTier(fixture, IceDevilVisualPolicy40.Key);
                Sprint28SetBrainActivePreservingActions(owner, false);
                foreach (int frame in Outsider30NativeAppearance(owner, id)) yield return frame;
                var hostility = Nalfeshnee39EstablishHostility(fixture, owner, target);
                target.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                if (suffix == "WallOfIce") target.Stats.SaveReflex.BaseValue = -100;
                if (suffix != "ConeOfCold")
                {
                    cold = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.BlueprintFeature>(
                        BlueprintBootstrap.Library, "9ae23798a9284e044ad2716a772a410e", "native cold immunity AI recipient boundary");
                    if (!target.Descriptor.HasFact(cold)) { target.Descriptor.AddFact(cold); addedCold = true; }
                }
                // The private NPC's native sight expands from8.5m to11.7m
                // in combat. Establish the real combat state before the9m
                // mutual-sight survey, not after a survey that cannot pass.
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target,
                    suffix == "WallOfIce" ? 5.5f : 9f, out far, out lane, out direction,
                    targetAllyExclusionRadius: 7f, originSearchRadiusMeters: 30);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                foreach (var actor in new[] { owner, target })
                {
                    actor.Wake(0); if (!Game.Instance.State.AwakeUnits.Contains(actor)) Game.Instance.State.AwakeUnits.Add(actor);
                    actor.SightCache.ClearIfNeeded();
                }
                owner.Memory.Add(target).Visible = owner.HasLOS(target);
                target.Memory.Add(owner).Visible = target.HasLOS(owner);
                var spell = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[p + suffix];
                var ai = (InfernalSpellAi35)ExpandedSummoningBlueprints.Registered.BySymbol[p + suffix + "Ai"];
                var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(spell));
                var decision = new DecisionContext { Unit = owner, Target = new TargetInfo { Unit = target }, Ability = data };
                TargetWrapper castTarget = new TargetWrapper(target);
                if (suffix == "WallOfIce")
                {
                    InfernalWallPlacement35 placement;
                    if (!InfernalWallPlacementRuntime35.TryPlanAi(owner, target, ai.WallSection, out placement))
                        throw new InvalidOperationException("No legal initial production AI wall plan.");
                    castTarget = new TargetWrapper(placement.Center);
                }
                check(id + "-ready", owner.IsEnemy(target) && target.IsEnemy(owner) &&
                    !ReferenceEquals(owner.Group, target.Group) && owner.HasLOS(target) &&
                    data.IsAvailable && data.CanTarget(castTarget) &&
                    ai.IsAvailable(decision) && ai.IsTargetValid(decision),
                    new JObject { ["hostility"] = hostility, ["lane"] = lane,
                        ["distance"] = owner.DistanceTo(target), ["selectedSpell"] = spell.AssetGuid,
                        ["recipientAlignment"] = target.Descriptor.Alignment.Value.ToString(),
                        ["coldImmuneDefender"] = suffix != "ConeOfCold", ["turnBased"] = turnBased },
                    "Production actor and per-target consumers admit this exact useful cast on a visible, mutually hostile private recipient.");
                var wrongActor = new DecisionContext { Unit = fixture.Caster, Target = decision.Target, Ability = data };
                check(id + "-exact-actor", !ai.IsAvailable(wrongActor), new JObject { ["wrongActor"] = fixture.Caster.UniqueId },
                    "The actual production action refuses a different actor even when supplied the Ice owner's ability context.");
                var cone = (InfernalSpellAi35)ExpandedSummoningBlueprints.Registered.BySymbol[p + "ConeOfColdAi"];
                var coneData = new AbilityData(owner.Descriptor.Abilities.GetAbility(cone.Ability));
                var coneDecision = new DecisionContext { Unit = owner, Target = decision.Target, Ability = coneData };
                if (suffix != "ConeOfCold")
                {
                    var coldFact = target.Descriptor.GetFact(cold);
                    var progressionMatches = target.Descriptor.Progression.Features.Enumerable
                        .Where(fact => ReferenceEquals(fact.Blueprint, cold)).ToArray();
                    var blueprintImmunities = cold.ComponentsArray
                        .OfType<Kingmaker.UnitLogic.FactLogic.AddEnergyDamageImmunity>().ToArray();
                    var runtimeImmunities = coldFact == null ?
                        Array.Empty<Kingmaker.UnitLogic.FactLogic.AddEnergyDamageImmunity>() :
                        coldFact.SelectComponents<Kingmaker.UnitLogic.FactLogic.AddEnergyDamageImmunity>().ToArray();
                    check(id + "-cold-immunity-selection", !cone.IsTargetValid(coneDecision) &&
                        coneData.IsAvailable,
                        new JObject { ["nativeImmunity"] = cold.AssetGuid, ["playerConeStillAvailable"] = coneData.IsAvailable,
                            ["nativePlayerCanTarget"] = coneData.CanTarget(new TargetWrapper(target)),
                            ["productionImmunityFact"] = cone.ColdImmunityFact?.AssetGuid,
                            ["descriptorHasFact"] = target.Descriptor.HasFact(cold),
                            ["runtimeFactPresent"] = coldFact != null,
                            ["runtimeFactTurnedOn"] = coldFact?.IsTurnedOn,
                            ["progressionMatchCount"] = progressionMatches.Length,
                            ["progressionTurnedOn"] = new JArray(progressionMatches.Select(x => x.IsTurnedOn)),
                            ["blueprintImmunityComponents"] = blueprintImmunities.Length,
                            ["runtimeImmunityComponents"] = runtimeImmunities.Length,
                            ["runtimeImmunityTypes"] = new JArray(runtimeImmunities.Select(x => x.EnergyType.ToString())) },
                        "AI declines cold-only offense against native cold immunity without removing the player's at-will ability.");
                }
                // Save-free private-faction seam used by the hostile witness,
                // with no attack relations. No registered/persisted listener
                // faction or intake group is changed for this friendly control.
                friendFaction = UnityEngine.Object.Instantiate(fixture.Caster.Blueprint.Faction);
                friendFaction.name = "KMG_Runtime_Sprint40_FriendlyWitnessFaction";
                friendFaction.Peaceful = friendFaction.Neutral = true;
                friendFaction.AlwaysEnemy = friendFaction.IsDirectlyControllable = false;
                friendFaction.Dummy = null;
                friendFaction.AttackFactions = Array.Empty<BlueprintFaction>();
                Vector3 friendPosition = ObstacleAnalyzer.GetNearestNode(origin + Vector3.right * 14f).clampedPosition;
                friend = Game.Instance.EntityCreator.SpawnUnit(Kingmaker.Blueprints.Root.BlueprintRoot.Instance.DefaultPlayerCharacter,
                    friendPosition, Quaternion.identity, fixture.Scene);
                fixture.Created.Add(friend); Game.Instance.EntityCreator.Tick();
                friend.GroupId = friend.UniqueId; friend.Descriptor.SwitchFactions(friendFaction, true);
                friend.AttackFactions.Match(Array.Empty<BlueprintFaction>());
                friend.Group.ResetFactionSet(); friend.Group.UpdateAttackFactionsCache();
                Sprint28SetBrainActivePreservingActions(friend, false);
                friend.Descriptor.State.AddConditionImmunity(UnitCondition.Frightened);
                friend.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                try
                {
                    if (suffix == "WallOfIce")
                    {
                        InfernalWallPlacement35 placement;
                        if (!InfernalWallPlacementRuntime35.TryPlanAi(owner, target, ai.WallSection, out placement))
                            throw new InvalidOperationException("Useful wall placement disappeared before refusal control.");
                        PlaceExpandedSummoningUnit(friend, placement.Center);
                    }
                    else PlaceExpandedSummoningUnit(friend, target.Position);
                    check(id + "-friendly-refusal", !owner.IsEnemy(friend) && !friend.IsEnemy(owner) &&
                        !Game.Instance.Player.Party.Contains(friend) && !friend.Group.IsPlayerParty &&
                        !ai.IsTargetValid(decision),
                        new JObject { ["ownedFriend"] = friend.UniqueId, ["friendPosition"] = Vec(friend.Position),
                            ["realProductionPredicate"] = true, ["partyActorsMoved"] = 0 },
                        "The actual native shape/occupied-wall checks refuse a friendly actor in the affected footprint; no party actor is moved.");
                }
                finally
                {
                    PlaceExpandedSummoningUnit(friend, friendPosition);
                }

                var admission = new JObject(); var passedTurns = new JArray();
                var idle = new[] { fixture.Caster, target, anchor, friend };
                if (turnBased)
                {
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                    DateTime ready = DateTime.UtcNow.AddSeconds(30);
                    while (DateTime.UtcNow < ready && !Sprint18OwnerIsActing(owner))
                    { PassInfernalWitness34(owner, idle, passedTurns); Game.Instance.IsPaused = false; yield return 0; }
                    check(id + "-native-turn", (bool?)admission["admitted"] == true && Sprint18OwnerIsActing(owner),
                        admission, "The selected owner receives its real native turn; only idle owned witnesses may pass.");
                }
                commands.Owner = owner; int first = trace.Rows.Count;
                var costs = new JArray(); var brain = new JObject();
                Func<UnitUseAbility> selected = () => commands.Commands.OfType<UnitUseAbility>().FirstOrDefault(x =>
                    ReferenceEquals(x.Spell?.Blueprint, spell) && x.AiAction is InfernalSpellAi35 &&
                    x.IsFinished && x.Result == UnitCommand.ResultType.Success &&
                    x.ExecutionProcess != null && x.ExecutionProcess.IsEnded);
                Func<bool> witnessed = () => selected() != null && (suffix != "WallOfIce" ||
                    Game.Instance.State.Units.All.Any(x => !x.Destroyed && x.IsInState &&
                        ReferenceEquals(x.Blueprint, ai.WallSection) &&
                        ReferenceEquals(x.Get<Kingmaker.UnitLogic.Parts.UnitPartSummonedMonster>()?.Summoner, owner)));
                EventBus.Subscribe(commands);
                try
                {
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                        if (!turnBased)
                            foreach (int frame in VrockDriveBrain37(owner, witnessed, 60, brain)) yield return frame;
                        else
                        {
                            DateTime deadline = DateTime.UtcNow.AddSeconds(60); int nativeFrames = 0;
                            while (DateTime.UtcNow < deadline && !witnessed())
                            {
                                if (Sprint18OwnerIsActing(owner)) nativeFrames++;
                                else PassInfernalWitness34(owner, idle, passedTurns);
                                Game.Instance.IsPaused = false; yield return 0;
                            }
                            brain["nativeTurnFrames"] = nativeFrames; brain["fixtureAiTicks"] = 0;
                        }
                        Sprint28SetBrainActivePreservingActions(owner, false);
                        var picked = selected();
                        var completedCasts = commands.Commands.OfType<UnitUseAbility>().Where(x =>
                            ReferenceEquals(x.Spell?.Blueprint, spell) && x.IsFinished &&
                            x.Result == UnitCommand.ResultType.Success).ToArray();
                        check(id + "-native-brain-cast", witnessed() && picked != null && completedCasts.Length > 0 &&
                            completedCasts.All(x => !x.Cutscene && !x.IsIgnoreCooldown &&
                                economy.Count(x) == 1 && economy.Exact(x, turnBased)),
                            new JObject { ["brain"] = brain, ["costs"] = costs,
                                ["commands"] = new JArray(commands.Commands.Select(Sprint27AiCommand)),
                                ["passedIdleTurns"] = passedTurns, ["fixtureAbilityCommands"] = 0 },
                            "Native brain selection completes the useful cast with its real standard action cost; the observer supplies no cast or extra attack.");
                    }
                }
                finally { EventBus.Unsubscribe(commands); Sprint28SetBrainActivePreservingActions(owner, false); }
                var damage = trace.Rows.Skip(first).Where(x => (string)x["event"] == "damage" &&
                    (string)x["owner"] == owner.UniqueId && (string)x["target"] == target.UniqueId &&
                    (string)x["ability"] == spell.AssetGuid).ToArray();
                if (suffix == "WallOfIce")
                {
                    var wall = Game.Instance.State.Units.All.SingleOrDefault(x => !x.Destroyed &&
                        ReferenceEquals(x.Blueprint, ai.WallSection) &&
                        ReferenceEquals(x.Get<Kingmaker.UnitLogic.Parts.UnitPartSummonedMonster>()?.Summoner, owner));
                    var binding = wall?.View?.GetComponent<InfernalWallNavmesh35>();
                    if (binding != null) foreach (int frame in InfernalBoneWaitUntil35(() => binding.GridApplyCompleted, 10)) yield return frame;
                    check(id + "-actual-owned-wall", wall != null && binding != null &&
                        binding.GridNodesClaimed > 0 &&
                        !ai.IsTargetValid(decision), new JObject { ["section"] = wall?.UniqueId,
                            ["claims"] = binding?.GridNodesClaimed,
                            ["aiRefusesRedundantPlacement"] = !ai.IsTargetValid(decision) },
                        "The brain's legal placement creates an actual owned navigation wall; its production redundancy filter then declines another wall at that lane.");
                    var originalNodes = binding.GridNodeSnapshot().ToArray();
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                    sourceRetired = true;
                    DateTime retirementDeadline = DateTime.UtcNow.AddSeconds(12);
                    Func<bool> retired = () =>
                        wall.Buffs.GetBuff(Kingmaker.Blueprints.Root.BlueprintRoot.Instance
                            .SystemMechanics.SummonedUnitBuff) == null &&
                        trace.Rows.Any(row => (string)row["event"] == "effect-application" &&
                            (string)row["target"] == wall.UniqueId &&
                            (string)row["blueprint"] == "c98d765d063f57a49a03f13d4f697c33") &&
                        originalNodes.All(node => node.Key.Walkable == node.Value);
                    while (DateTime.UtcNow < retirementDeadline && !retired())
                    { Game.Instance.IsPaused = false; yield return 0; }
                    var wallLifetime = wall.Buffs.GetBuff(
                        Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
                    bool nativeRetirement = trace.Rows.Any(row =>
                        (string)row["event"] == "effect-application" &&
                        (string)row["target"] == wall.UniqueId &&
                        (string)row["blueprint"] == "c98d765d063f57a49a03f13d4f697c33");
                    var nodeMismatches = originalNodes.Where(node =>
                        node.Key.Walkable != node.Value).Select(node => new JObject {
                            ["node"] = node.Key.position.ToString(),
                            ["graph"] = node.Key.GraphIndex,
                            ["original"] = node.Value,
                            ["current"] = node.Key.Walkable
                        }).ToArray();
                    check(id + "-source-removal", wallLifetime == null && nativeRetirement &&
                        originalNodes.Length > 0 && nodeMismatches.Length == 0,
                        new JObject { ["sourceDestroyed"] = owner.Destroyed,
                            ["sectionDestroyed"] = wall.Destroyed, ["sectionDead"] = wall.Descriptor.State.IsDead,
                            ["sectionInGame"] = wall.IsInGame, ["lifetimeRemoved"] = wallLifetime == null,
                            ["nativeDismembermentTransition"] = nativeRetirement,
                            ["bindingRestoreCompleted"] = binding == null || binding.GridRestoreCompleted,
                            ["nativeCutDisabledBeforeDestroy"] = binding?.NativeCutDisabledBeforeDestroy,
                            ["bindingClaims"] = binding?.GridNodesClaimed,
                            ["reconcileRequests"] = binding?.GridReconcileRequests,
                            ["reconcileCompleted"] = binding?.GridReconcileCompleted,
                            ["activeClaims"] = InfernalWallNavmesh35.ActiveGridClaimCount,
                            ["nodes"] = originalNodes.Length,
                            ["allOriginalBitsRestored"] = nodeMismatches.Length == 0,
                            ["nodeMismatches"] = new JArray(nodeMismatches) },
                        "Actual source removal removes the wall lifetime, enters the native dismemberment transition and restores every claimed navigation bit before fixture teardown.");
                    // The production contract above has completed. Remove its
                    // request-owned native corpse only to keep the shared
                    // fixture's global reference census exact.
                    if (!wall.Destroyed)
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { wall });
                }
                else
                {
                    // The retained native receipt emits two separate Storm
                    // damage rules, one cold and one physical, once each.
                    var results = damage.SelectMany(x => x["results"]).ToArray();
                    var packetsSeen = damage.SelectMany(x => x["packets"]).ToArray();
                    bool packets = suffix == "ConeOfCold" ? damage.Length == 1 &&
                        (int?)damage[0]["actual"] > 0 && packetsSeen.Any(x => (string)x["dice"] == "13d6") :
                        damage.Length == 2 && packetsSeen.Any(x => (string)x["dice"] == "3d6" &&
                            (string)x["type"] == "PhysicalDamage") &&
                        packetsSeen.Any(x => (string)x["dice"] == "2d6" && (string)x["energy"] == "Cold") &&
                        results.Any(x => (string)x["type"] == "PhysicalDamage" && (int?)x["actual"] > 0) &&
                        damage.Where(x => ((JArray)x["packets"]).Any(packet =>
                            (string)packet["energy"] == "Cold")).All(x =>
                                (int?)x["actual"] == 0 && (int?)x["before"] == (int?)x["after"] &&
                                ((JArray)x["results"]).Any(result =>
                                    (string)result["energy"] == "Cold" && (bool?)result["immune"] == true)) &&
                        !trace.Rows.Skip(first).Any(x => (string)x["event"] == "save" && (string)x["owner"] == owner.UniqueId);
                    check(id + "-actual-damage", packets, new JObject { ["damage"] = new JArray(damage) },
                        "Actual native damage retains CL13 cone dice, or Storm's physical injury through native immunity with zero applied cold damage and no save.");
                    if (suffix == "IceStorm")
                    {
                        int initial = damage.Length;
                        // The cast/cost above is a genuine TB action. Observe
                        // the area cadence afterward in RTWP so a paused own
                        // turn cannot masquerade as an elapsed native round.
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                        Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                        foreach (int frame in InfernalBoneWaitGame35(6.1, 15)) yield return frame;
                        check(id + "-native-terrain-once", target.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain) &&
                            Sprint29OwnedAreas(owner, stormArea).Length > 0 &&
                            trace.Rows.Skip(first).Count(x => (string)x["event"] == "damage" &&
                                (string)x["owner"] == owner.UniqueId && (string)x["ability"] == spell.AssetGuid) == initial,
                            new JObject { ["nativeDifficultTerrain"] = true, ["initialDamageEvents"] = initial,
                                ["cadenceObservedInRtwpAfterNativeTurnCast"] = true,
                                ["activeAreas"] = Sprint29OwnedAreas(owner, stormArea).Length },
                            "The real native area persists with difficult terrain across a round without inventing repeated damage.");
                    }
                }
            }
            finally
            {
                EventBus.Unsubscribe(commands);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                foreach (var actor in new[] { owner, target }.Where(x => x != null && !x.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(true);
                    if (actor.IsInCombat) actor.LeaveCombat();
                }
                if (owner != null && !sourceRetired)
                {
                    // Isolated fixture cleanup only; normal source removal is
                    // allowed to leave Storm for its native finite duration.
                    foreach (var area in Sprint29OwnedAreas(owner, stormArea)) area.ForceEnd();
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                }
                if (friend != null) DisposeExpandedSummoningUnits(fixture.Created, new[] { friend });
                if (friendFaction != null) UnityEngine.Object.Destroy(friendFaction);
                if (addedCold) target.Descriptor.RemoveFact(cold);
                if (isolatedFear) target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Frightened);
                if (anchor != null && !anchor.Destroyed && anchor.IsInCombat) anchor.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();
                RetireInfernalHostile34(fixture);
            }
        }
    }
}
