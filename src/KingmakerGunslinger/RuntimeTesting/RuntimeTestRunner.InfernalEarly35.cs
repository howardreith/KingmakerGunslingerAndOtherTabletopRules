using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Assets;
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
                var expectedRegistrations = ExpandedSummoningIdentityCatalog
                    .Build().Where(value => value.Symbol.StartsWith(
                            ExpandedSummoningSpecialBuilder.Barbed35Prefix,
                            StringComparison.Ordinal) ||
                        value.Symbol.StartsWith(
                            ExpandedSummoningSpecialBuilder.Bone35Prefix,
                            StringComparison.Ordinal)).OrderBy(value =>
                                value.Symbol, StringComparer.Ordinal).ToArray();
                var actualBySymbol = registrations.ToDictionary(value =>
                    value.Key, value => value.Value, StringComparer.Ordinal);
                var missingRegistrations = expectedRegistrations.Where(value =>
                    !actualBySymbol.ContainsKey(value.Symbol)).Select(value =>
                        value.Symbol).ToArray();
                var unexpectedRegistrations = registrations.Where(value =>
                    !expectedRegistrations.Any(expected => String.Equals(
                        expected.Symbol, value.Key, StringComparison.Ordinal)))
                    .Select(value => value.Key).OrderBy(value => value,
                        StringComparer.Ordinal).ToArray();
                var wrongTypes = expectedRegistrations.Where(value =>
                    actualBySymbol.ContainsKey(value.Symbol) &&
                    (actualBySymbol[value.Symbol] == null ||
                    !String.Equals(actualBySymbol[value.Symbol].GetType().Name,
                        value.PlannedType, StringComparison.Ordinal))).Select(
                            value => new JObject {
                                ["symbol"] = value.Symbol,
                                ["expected"] = value.PlannedType,
                                ["actual"] = actualBySymbol[value.Symbol] == null ?
                                    null : actualBySymbol[value.Symbol]
                                        .GetType().Name
                            }).ToArray();
                var bootstrap = new JObject {
                    ["routeCount"] = routes.Length,
                    ["routes"] = new JArray(routes.Select(value => new JObject {
                        ["key"] = value.Creature.Key,
                        ["tier"] = value.ParentTier,
                        ["quantity"] = value.Multiplicity.ToString(),
                        ["published"] = SummonVisibilityCatalog.IsPublished(value)
                    })),
                    ["expectedRegistrationCount"] =
                        expectedRegistrations.Length,
                    ["privateRegistrationCount"] = registrations.Length,
                    ["privateSymbols"] = new JArray(registrations.Select(value =>
                        value.Key).OrderBy(value => value,
                            StringComparer.Ordinal)),
                    ["privateTypes"] = new JArray(registrations.Select(value =>
                        value.Value == null ? null : value.Value.GetType().Name)
                        .OrderBy(value => value)),
                    ["missingRegistrations"] =
                        new JArray(missingRegistrations),
                    ["unexpectedRegistrations"] =
                        new JArray(unexpectedRegistrations),
                    ["wrongTypes"] = new JArray(wrongTypes)
                };
                check("hidden-bootstrap", routes.Length == 5 &&
                    routes.All(value => !SummonVisibilityCatalog.IsPublished(value)) &&
                    registrations.Length == expectedRegistrations.Length &&
                    missingRegistrations.Length == 0 &&
                    unexpectedRegistrations.Length == 0 &&
                    wrongTypes.Length == 0, bootstrap,
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

                Mesh barbedMesh, boneMesh;
                string[] barbedBones, boneBones;
                Texture2D barbedPaint, bonePaint;
                string barbedStatus, boneStatus;
                string barbedView = ExpandedSummoningPteranodonViewPatch
                    .DescribeView(barbed.View);
                string boneView = ExpandedSummoningPteranodonViewPatch
                    .DescribeView(bone.View);
                var tail = bone.View.GetComponent<BoneDevilTailVisual35>();
                bool barbedCache = PteranodonAssetRuntime
                    .TryGetInfernalVisual35(InfernalVisualPolicy35.Barbed,
                        out barbedMesh, out barbedBones, out barbedPaint,
                        out barbedStatus);
                bool boneCache = PteranodonAssetRuntime
                    .TryGetInfernalVisual35(InfernalVisualPolicy35.Bone,
                        out boneMesh, out boneBones, out bonePaint,
                        out boneStatus);
                check("original-body-attachment", barbedCache && boneCache &&
                    barbedMesh != null && boneMesh != null &&
                    !ReferenceEquals(barbedMesh, boneMesh) &&
                    barbedView.StartsWith("visual:attached;",
                        StringComparison.Ordinal) &&
                    boneView.StartsWith("visual:attached;",
                        StringComparison.Ordinal) && tail != null,
                    new JObject {
                        ["barbedView"] = barbedView,
                        ["boneView"] = boneView,
                        ["barbedCache"] = barbedStatus,
                        ["boneCache"] = boneStatus,
                        ["barbedVertices"] = barbedMesh?.vertexCount,
                        ["boneVertices"] = boneMesh?.vertexCount,
                        ["tailAttached"] = tail != null
                    },
                    "Both exact hidden roots wear distinct original bodies; Bone alone owns the private stinging-tail view.");

                var oldGroup = reach.GroupId;
                reach.GroupId = "KMG_S35_Early_Reach_" + reach.UniqueId;
                reach.Descriptor.SwitchFactions(fixture.Hostile.Faction, true);
                reach.UpdateGroup();
                var reachProfile = InfernalMartialProfile34.For(reach);
                foreach (int frame in InfernalSwitch34(reach,
                    reachProfile.Glaive)) yield return frame;
                receipt["reusedContactProof"] = new JObject {
                    ["runId"] =
                        "20261031T0029267333965Z-bf97d85fd26643959cb5e767b26e56c1",
                    ["reason"] =
                        "Barbed contact/reach production is unchanged; this changed observation is scoped to Wall formation, breach, residual, visual, and navigation consumers."
                };
                receipt["contactOriginalGroup"] = oldGroup;

                foreach (int frame in InfernalEarlyTail35(fixture, bone,
                    reach, trace, check)) yield return frame;

                foreach (int frame in InfernalEarlyWall35(fixture, bone,
                    owned, trace, check)) yield return frame;
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

        private IEnumerable<int> InfernalEarlyTail35(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData bone, UnitEntityData target,
            InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            var tail = bone.View.GetComponent<BoneDevilTailVisual35>();
            if (tail == null) throw new InvalidOperationException(
                "Bone Devil original body has no private tail consumer.");
            bone.Commands.InterruptAll(true);
            bone.Commands.RemoveFinishedAndUpdateQueue();
            InfernalCombatLane34(fixture, bone, target, 3);
            int before = tail.Contacts;
            var command = new UnitAttack(target) {
                IsSingleAttack = false, ForceFullAttack = true };
            bone.Commands.Run(command);
            foreach (int frame in WaitInfernalCommand34(command,
                () => tail.Contacts > before, 30)) yield return frame;
            int commandReference = System.Runtime.CompilerServices
                .RuntimeHelpers.GetHashCode(command);
            var attackRows = trace.Rows.Where(row =>
                (string)row["event"] == "attack" &&
                (string)row["owner"] == bone.UniqueId &&
                (string)row["target"] == target.UniqueId &&
                (int?)row["attackCommand"] == commandReference).ToArray();
            var completedAtAct = trace.Rows.Where(row =>
                (string)row["event"] == "command-act" &&
                (string)row["owner"] == bone.UniqueId &&
                (string)row["target"] == target.UniqueId &&
                (int?)row["commandReference"] == commandReference &&
                (string)row["native"]?["result"] ==
                    UnitCommand.ResultType.Success.ToString()).ToArray();
            var profile = InfernalMartialProfile35.For(bone);
            bool nativePlanCompleted = command.IsFinished &&
                completedAtAct.Length == 1 &&
                attackRows.Length == command.AllAttacks.Count &&
                attackRows.Count(row => (string)row["weapon"] ==
                    profile.Sting.AssetGuid) == 1;
            string view = ExpandedSummoningPteranodonViewPatch.DescribeView(
                bone.View);
            check("bone-sting-tail-contact",
                nativePlanCompleted &&
                tail.Contacts > before && tail.ContactGap < .15f &&
                view.StartsWith("visual:attached;", StringComparison.Ordinal),
                new JObject {
                    ["owner"] = bone.UniqueId,
                    ["target"] = target.UniqueId,
                    ["command"] = Sprint27AiCommand(command),
                    ["commandReference"] = commandReference,
                    ["nativePlanCount"] = command.AllAttacks.Count,
                    ["attackRows"] = new JArray(attackRows),
                    ["completedAtAct"] = new JArray(completedAtAct),
                    ["nativePlanCompleted"] = nativePlanCompleted,
                    ["finalResultIsPostPlanRetirement"] =
                        command.Result == UnitCommand.ResultType.Interrupt &&
                        nativePlanCompleted,
                    ["contactsBefore"] = before,
                    ["contactsAfter"] = tail.Contacts,
                    ["contactGap"] = tail.ContactGap,
                    ["contactPoint"] = Vec(tail.ContactPoint),
                    ["restingAfterCommand"] = tail.Resting,
                    ["view"] = view,
                    ["forcedRolls"] = 0
                },
                "The exact native full attack advances Bone's owned sting, and only its private authored tail reaches the real target contact path.");
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
            InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            Vector3 start, midpoint, destination, casterPoint;
            JArray laneAttempts;
            if (!InfernalEarlyWallLane35(fixture, out start, out midpoint,
                    out destination, out casterPoint, out laneAttempts))
                throw new InvalidOperationException(
                    "No direct native floor lane for the early wall candidate: " +
                    laneAttempts.ToString(Formatting.None));
            PlaceExpandedSummoningUnit(fixture.Caster, start);
            foreach (UnitEntityData actor in owned.Where(value =>
                value != null && !value.Destroyed &&
                !ReferenceEquals(value, bone)))
                PlaceExpandedSummoningUnit(actor, casterPoint);
            PlaceExpandedSummoningUnit(bone, casterPoint);
            var before = InfernalEarlyPath35(fixture.Caster, destination);
            bool losBefore = Kingmaker.Visual.FogOfWar.LineOfSightGeometry
                .Instance.HasObstacle(start, destination, 0);

            var wallBlueprint = fixture.Blueprints.OfType<BlueprintAbility>()
                .Single(value => value.name ==
                    "KMG_Summoning_Infernal_BoneDevil_WallOfIce");
            var wallRun = wallBlueprint.GetComponents<
                Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>()
                .Single();
            bool formationHasInvalidOuterSave = wallRun.SavingThrowType !=
                Kingmaker.EntitySystem.Stats.SavingThrowType.Unknown;
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
            var wallSamples = new JArray();
            var withWall = InfernalEarlyPath35(fixture.Caster, destination);
            bool obstruction = false;
            deadline = DateTime.UtcNow.AddSeconds(15);
            int nextSampleFrame = Time.frameCount;
            while (section != null && DateTime.UtcNow < deadline &&
                !obstruction)
            {
                Game.Instance.IsPaused = false;
                yield return 0;
                if (Time.frameCount < nextSampleFrame) continue;
                nextSampleFrame = Time.frameCount + 10;
                withWall = InfernalEarlyPath35(fixture.Caster, destination);
                obstruction = InfernalEarlyWallObstructs35(before, withWall);
                if (wallSamples.Count < 16) wallSamples.Add(new JObject {
                    ["frame"] = Time.frameCount,
                    ["path"] = withWall
                });
            }
            bool losWith = Kingmaker.Visual.FogOfWar.LineOfSightGeometry
                .Instance.HasObstacle(start, destination, 0);
            float sectionOffset = section == null ? float.PositiveInfinity :
                InfernalEarlyDistanceToSegment35(section.Position, start,
                    destination);
            var binding = section?.View == null ? null :
                section.View.GetComponent<InfernalWallNavmesh35>();
            var cut = binding?.Cut;
            bool bindingPresent = binding != null;
            bool wallVisualPresent = binding?.WallVisual != null;
            bool wallVisualEnabled = wallVisualPresent &&
                binding.WallVisual.GetComponent<Renderer>()?.enabled == true;
            int hiddenDonorRenderers = bindingPresent ?
                binding.HiddenDonorRendererCount : 0;
            Vector3 laneDirection = (destination - start).normalized;
            Vector3 sectionAlong = section == null ? Vector3.zero :
                Vector3.Cross(Vector3.up,
                    section.OrientationDirection).normalized;
            float sectionLaneAlignment = section == null ? 0f :
                Math.Abs(Vector3.Dot(sectionAlong, laneDirection));
            bool cutExactOwner = bindingPresent &&
                ReferenceEquals(binding.Owner, section);
            bool cutPresent = cut != null;
            string cutType = cutPresent ? cut.type.ToString() : null;
            JArray cutRectangle = cutPresent ? new JArray(
                cut.rectangleSize.x, cut.rectangleSize.y) : null;
            float? cutHeight = cutPresent ? (float?)cut.height : null;
            bool? cutRotation = cutPresent ? (bool?)cut.useRotation : null;
            bool? cutAddedGeometry = cutPresent ?
                (bool?)cut.cutsAddedGeom : null;
            string productionUpdateRoute = bindingPresent ?
                binding.UpdateRoute : null;
            int productionRecastApplyRequests = bindingPresent ?
                binding.RecastApplyRequests : 0;
            int gridGraphIndex = bindingPresent ?
                binding.GridGraphIndex : -1;
            int gridNodesConsidered = bindingPresent ?
                binding.GridNodesConsidered : 0;
            int gridNodesClaimed = bindingPresent ?
                binding.GridNodesClaimed : 0;
            int gridNodesChanged = bindingPresent ?
                binding.GridNodesChanged : 0;
            bool gridApplyQueued = bindingPresent &&
                binding.GridApplyQueued;
            bool gridApplyCompleted = bindingPresent &&
                binding.GridApplyCompleted;
            string gridFailure = bindingPresent ?
                binding.GridFailure : null;
            KeyValuePair<Pathfinding.GraphNode, bool>[] gridSnapshot =
                bindingPresent ? binding.GridNodeSnapshot() :
                    new KeyValuePair<Pathfinding.GraphNode, bool>[0];
            int gridClaimsBeforeRelease =
                InfernalWallNavmesh35.ActiveGridClaimCount;
            Bounds? cutBounds = cutPresent ? (Bounds?)cut.GetBounds() : null;
            bool cutRegistered = cutPresent &&
                Pathfinding.NavmeshCut.GetAll().Contains(cut);
            bool recastUpdatePending = cutPresent && cut.RequiresUpdate();
            var tileHandlers = UnityEngine.Object.FindObjectsOfType<
                Pathfinding.TileHandlerHelper>();
            var startNode = AstarPath.active.GetNearest(start);
            int graphIndex = startNode.node == null ? -1 :
                (int)startNode.node.GraphIndex;
            string graphType = graphIndex < 0 || graphIndex >=
                AstarPath.active.graphs.Length ? null :
                AstarPath.active.graphs[graphIndex]?.GetType().FullName;

            bool hadSummonBuff = false;
            UnitAttack breachAttack = null;
            JArray breachAttackRolls = new JArray();
            JArray physicalDamageRows = new JArray();
            JArray fireDamageRows = new JArray();
            JArray residualDamageRows = new JArray();
            bool wallAliveAfterPhysical = false;
            int damageAfterPhysical = -1;
            AreaEffectEntityData residualArea = null;
            AreaEffectEntityData fogArea = null;
            UnitEntityData attacker = owned.FirstOrDefault(value =>
                value != null && !value.Destroyed &&
                InfernalMartialProfile34.For(value)?.Glaive != null);
            if (section != null && attacker != null)
            {
                var summoned = section.Buffs.GetBuff(BlueprintRoot.Instance
                    .SystemMechanics.SummonedUnitBuff);
                hadSummonBuff = summoned != null;
                Vector3 attackPoint = ObstacleAnalyzer.GetNearestNode(
                    section.Position + section.OrientationDirection * 2.5f)
                    .clampedPosition;
                PlaceExpandedSummoningUnit(attacker, attackPoint);
                attacker.Memory.Add(section);
                int physicalStart = trace.Rows.Count;
                breachAttack = new UnitAttack(section) {
                    IsSingleAttack = true,
                    ForceFullAttack = false
                };
                attacker.Commands.Run(breachAttack);
                foreach (int frame in WaitInfernalCommand34(breachAttack,
                    () => true, 25)) yield return frame;
                breachAttackRolls = new JArray(trace.Rows.Skip(physicalStart)
                    .Where(eventRow => (string)eventRow["event"] ==
                            "attack-roll" &&
                        (string)eventRow["owner"] == attacker.UniqueId &&
                        (string)eventRow["target"] == section.UniqueId));
                physicalDamageRows = new JArray(trace.Rows.Skip(physicalStart)
                    .Where(eventRow => (string)eventRow["event"] ==
                            "damage" &&
                        (string)eventRow["owner"] == attacker.UniqueId &&
                        (string)eventRow["target"] == section.UniqueId));
                wallAliveAfterPhysical = !section.Destroyed &&
                    !section.Descriptor.State.IsDead;
                damageAfterPhysical = section.Damage;

                Vector3 witnessPoint = ObstacleAnalyzer.GetNearestNode(
                    section.Position - section.OrientationDirection * 1.2f)
                    .clampedPosition;
                PlaceExpandedSummoningUnit(fixture.Caster, witnessPoint);
                int fireStart = trace.Rows.Count;
                var fire = new EnergyDamage(new DiceFormula(0,
                    DiceType.Zero), DamageEnergyType.Fire) {
                        PreRolledValue = 1000
                    };
                Rulebook.Trigger(new RuleDealDamage(attacker, section,
                    new DamageBundle(fire)));
                var residualBlueprint = fixture.Blueprints.OfType<
                    BlueprintAbilityAreaEffect>().Single(value => value.name ==
                        "KMG_Summoning_Infernal_BoneDevil_WallResidualArea");
                var fogBlueprint = fixture.Blueprints.OfType<
                    BlueprintAbilityAreaEffect>().Single(value =>
                        value.AssetGuid ==
                            "e09010a73354a794293ebc7b33c2d130");
                deadline = DateTime.UtcNow.AddSeconds(15);
                while (DateTime.UtcNow < deadline &&
                    (residualArea == null || fogArea == null ||
                     InfernalWallNavmesh35.ActiveGridClaimCount != 0))
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                    residualArea = Game.Instance.State.AreaEffects.All
                        .FirstOrDefault(value =>
                            ReferenceEquals(value.Blueprint,
                                residualBlueprint) &&
                            ReferenceEquals(value.Context?.MaybeCaster,
                                bone));
                    fogArea = Game.Instance.State.AreaEffects.All
                        .FirstOrDefault(value =>
                            ReferenceEquals(value.Blueprint, fogBlueprint) &&
                            ReferenceEquals(value.Context?.MaybeCaster,
                                bone) &&
                            ReferenceEquals(value.Context?.SourceAbility,
                                wallBlueprint));
                }
                fireDamageRows = new JArray(trace.Rows.Skip(fireStart)
                    .Where(eventRow => (string)eventRow["event"] ==
                            "damage" &&
                        (string)eventRow["owner"] == attacker.UniqueId &&
                        (string)eventRow["target"] == section.UniqueId));
                residualDamageRows = new JArray(trace.Rows.Skip(fireStart)
                    .Where(eventRow => (string)eventRow["event"] ==
                            "damage" &&
                        (string)eventRow["owner"] == bone.UniqueId &&
                        (string)eventRow["target"] ==
                            fixture.Caster.UniqueId &&
                        (string)eventRow["ability"] ==
                            wallBlueprint.AssetGuid));
            }

            var formationAttempts = new JArray();
            bool formationDisrupted = false;
            int oldReflex = fixture.Caster.Stats.SaveReflex.BaseValue;
            try
            {
                fixture.Caster.Stats.SaveReflex.BaseValue = 100;
                Vector3 adjacent = ObstacleAnalyzer.GetNearestNode(midpoint +
                    (casterPoint - midpoint).normalized * 1.2f)
                    .clampedPosition;
                PlaceExpandedSummoningUnit(fixture.Caster, adjacent);
                for (int attempt = 0; attempt < 4 &&
                    !formationDisrupted; attempt++)
                {
                    int formationStart = trace.Rows.Count;
                    var existing = new HashSet<string>(Game.Instance.State
                        .Units.Where(value => ReferenceEquals(value.Blueprint,
                            sectionBlueprint)).Select(value => value.UniqueId));
                    bool available = data.IsAvailable &&
                        data.CanTarget(new TargetWrapper(midpoint));
                    var formationCommand = new UnitUseAbility(data,
                        new TargetWrapper(midpoint));
                    bone.Commands.Run(formationCommand);
                    foreach (int frame in WaitInfernalCommand34(
                        formationCommand, () => true, 25)) yield return frame;
                    for (int frame = 0; frame < 30; frame++) yield return 0;
                    UnitEntityData formed = Game.Instance.State.Units
                        .FirstOrDefault(value =>
                            ReferenceEquals(value.Blueprint,
                                sectionBlueprint) &&
                            !existing.Contains(value.UniqueId));
                    var srRows = new JArray(trace.Rows.Skip(formationStart)
                        .Where(eventRow => (string)eventRow["event"] ==
                                "spell-resistance" &&
                            (string)eventRow["target"] ==
                                fixture.Caster.UniqueId &&
                            (string)eventRow["contextAbility"] ==
                                wallBlueprint.AssetGuid));
                    var saveRows = new JArray(trace.Rows.Skip(formationStart)
                        .Where(eventRow => (string)eventRow["event"] ==
                                "save" &&
                            (string)eventRow["target"] ==
                                fixture.Caster.UniqueId &&
                            (string)eventRow["type"] ==
                                SavingThrowType.Reflex.ToString()));
                    formationDisrupted = available &&
                        formationCommand.Result ==
                            UnitCommand.ResultType.Success &&
                        srRows.Count == 1 && saveRows.Count == 1 &&
                        (bool?)saveRows[0]["passed"] == true &&
                        formed == null;
                    formationAttempts.Add(new JObject {
                        ["attempt"] = attempt,
                        ["available"] = available,
                        ["command"] = Sprint27AiCommand(formationCommand),
                        ["spellResistance"] = srRows,
                        ["saves"] = saveRows,
                        ["formedSection"] = formed?.UniqueId
                    });
                    if (formed == null) continue;
                    owned.Add(formed);
                    Buff cleanup = formed.Buffs.GetBuff(BlueprintRoot.Instance
                        .SystemMechanics.SummonedUnitBuff);
                    if (cleanup != null) cleanup.Remove();
                    deadline = DateTime.UtcNow.AddSeconds(10);
                    while (DateTime.UtcNow < deadline &&
                        InfernalWallNavmesh35.ActiveGridClaimCount != 0)
                    { Game.Instance.IsPaused = false; yield return 0; }
                }
            }
            finally
            {
                fixture.Caster.Stats.SaveReflex.BaseValue = oldReflex;
            }
            PlaceExpandedSummoningUnit(fixture.Caster, start);
            var after = InfernalEarlyPath35(fixture.Caster, destination);
            var restorationSamples = new JArray();
            bool restored = InfernalEarlyWallRestored35(before, after);
            bool gridNodeStateRestored = gridSnapshot.Length > 0 &&
                gridSnapshot.All(value => value.Key != null &&
                    value.Key.Walkable == value.Value);
            deadline = DateTime.UtcNow.AddSeconds(15);
            nextSampleFrame = Time.frameCount;
            while (section != null && DateTime.UtcNow < deadline &&
                (!restored || !gridNodeStateRestored))
            {
                Game.Instance.IsPaused = false;
                yield return 0;
                if (Time.frameCount < nextSampleFrame) continue;
                nextSampleFrame = Time.frameCount + 10;
                after = InfernalEarlyPath35(fixture.Caster, destination);
                restored = InfernalEarlyWallRestored35(before, after);
                gridNodeStateRestored = gridSnapshot.Length > 0 &&
                    gridSnapshot.All(value => value.Key != null &&
                        value.Key.Walkable == value.Value);
                if (restorationSamples.Count < 16)
                    restorationSamples.Add(new JObject {
                        ["frame"] = Time.frameCount,
                        ["path"] = after
                    });
            }
            int gridClaimsAfterRelease =
                InfernalWallNavmesh35.ActiveGridClaimCount;
            bool losAfter = Kingmaker.Visual.FogOfWar.LineOfSightGeometry
                .Instance.HasObstacle(start, destination, 0);
            var row = new JObject {
                ["owner"] = bone.UniqueId,
                ["ability"] = wallBlueprint.AssetGuid,
                ["outerFormationSave"] = wallRun.SavingThrowType.ToString(),
                ["formationHasInvalidOuterSave"] =
                    formationHasInvalidOuterSave,
                ["command"] = Sprint27AiCommand(command),
                ["targetable"] = targetable,
                ["start"] = Vec(start),
                ["midpoint"] = Vec(midpoint),
                ["destination"] = Vec(destination),
                ["casterPoint"] = Vec(casterPoint),
                ["laneAttempts"] = laneAttempts,
                ["section"] = section?.UniqueId,
                ["sectionBlueprint"] = section?.Blueprint?.AssetGuid,
                ["sectionOffsetFromLane"] = sectionOffset,
                ["sectionHitPoints"] = section?.Stats.HitPoints.ModifiedValue,
                ["summonedLifetimeBuff"] = hadSummonBuff,
                ["sectionRetired"] = section != null &&
                    (section.Destroyed || !section.IsInGame ||
                     section.Descriptor.State.IsDead),
                ["navmeshCutBinding"] = bindingPresent,
                ["wallVisualPresent"] = wallVisualPresent,
                ["wallVisualEnabled"] = wallVisualEnabled,
                ["hiddenDonorRenderers"] = hiddenDonorRenderers,
                ["sectionOrientation"] = section?.Orientation,
                ["sectionForward"] = section == null ? null :
                    Vec(section.OrientationDirection),
                ["sectionLaneAlignment"] = sectionLaneAlignment,
                ["navmeshCutExactOwner"] = cutExactOwner,
                ["navmeshCutPresent"] = cutPresent,
                ["navmeshCutType"] = cutType,
                ["navmeshCutRectangle"] = cutRectangle,
                ["navmeshCutHeight"] = cutHeight,
                ["navmeshCutRotation"] = cutRotation,
                ["navmeshCutAddedGeometry"] = cutAddedGeometry,
                ["navmeshCutBounds"] = cutBounds == null ? null :
                    new JObject {
                        ["center"] = Vec(cutBounds.Value.center),
                        ["size"] = Vec(cutBounds.Value.size)
                    },
                ["navmeshCutRegistered"] = cutRegistered,
                ["recastUpdatePending"] = recastUpdatePending,
                ["productionUpdateRoute"] = productionUpdateRoute,
                ["productionRecastApplyRequests"] =
                    productionRecastApplyRequests,
                ["gridGraphIndex"] = gridGraphIndex,
                ["gridNodesConsidered"] = gridNodesConsidered,
                ["gridNodesClaimed"] = gridNodesClaimed,
                ["gridNodesChanged"] = gridNodesChanged,
                ["gridApplyQueued"] = gridApplyQueued,
                ["gridApplyCompleted"] = gridApplyCompleted,
                ["gridFailure"] = gridFailure,
                ["gridOriginalWalkability"] = new JArray(gridSnapshot
                    .Select(value => value.Value)),
                ["gridClaimsBeforeRelease"] = gridClaimsBeforeRelease,
                ["gridClaimsAfterRelease"] = gridClaimsAfterRelease,
                ["gridNodeStateRestored"] = gridNodeStateRestored,
                ["pathGraphIndex"] = graphIndex,
                ["pathGraphType"] = graphType,
                ["activeGraphs"] = new JArray(AstarPath.active.graphs
                    .Select((value, index) => new JObject {
                        ["index"] = index,
                        ["type"] = value?.GetType().FullName
                    })),
                ["tileHandlers"] = new JArray(tileHandlers.Select(value =>
                    new JObject {
                        ["name"] = value.name,
                        ["enabled"] = value.enabled,
                        ["active"] = value.gameObject.activeInHierarchy,
                        ["updateInterval"] = value.updateInterval
                    })),
                ["pathBefore"] = before,
                ["pathWithWall"] = withWall,
                ["pathAfter"] = after,
                ["wallPathSamples"] = wallSamples,
                ["restorationPathSamples"] = restorationSamples,
                ["movementObstructionObserved"] = obstruction,
                ["navigationRestored"] = restored,
                ["lineOfSightObstacleBefore"] = losBefore,
                ["lineOfSightObstacleWithWall"] = losWith,
                ["lineOfSightObstacleAfter"] = losAfter,
                ["lineOfSightChanged"] = losWith != losBefore,
                ["formationGeometry"] =
                    "one grounded ten-foot plane perpendicular to caster-to-point",
                ["breachAttacker"] = attacker?.UniqueId,
                ["breachAttack"] = breachAttack == null ? null :
                    Sprint27AiCommand(breachAttack),
                ["breachAttackRolls"] = breachAttackRolls,
                ["physicalDamageRows"] = physicalDamageRows,
                ["wallAliveAfterPhysical"] = wallAliveAfterPhysical,
                ["damageAfterPhysical"] = damageAfterPhysical,
                ["fireDamageRows"] = fireDamageRows,
                ["residualArea"] = residualArea?.UniqueId,
                ["residualTimeLeft"] = residualArea == null ? 0d :
                    Sprint29AreaDuration(residualArea),
                ["fogArea"] = fogArea?.UniqueId,
                ["fogTimeLeft"] = fogArea == null ? 0d :
                    Sprint29AreaDuration(fogArea),
                ["residualDamageRows"] = residualDamageRows,
                ["formationAttempts"] = formationAttempts,
                ["formationDisrupted"] = formationDisrupted,
                ["wallBreachResidualImplemented"] = true
            };
            bool automaticPhysicalHit = breachAttackRolls.OfType<JObject>()
                .Any(value => (bool?)value["hit"] == true &&
                    (bool?)value["autoHit"] == true);
            bool physicalWasObjectHalf = physicalDamageRows.OfType<JObject>()
                .SelectMany(value => value["packets"] ?? new JArray())
                .OfType<JObject>().Any(value =>
                    (bool?)value["half"] == true);
            bool fireWasFull = fireDamageRows.OfType<JObject>()
                .SelectMany(value => value["packets"] ?? new JArray())
                .OfType<JObject>().Any(value =>
                    (string)value["energy"] ==
                        DamageEnergyType.Fire.ToString() &&
                    (bool?)value["half"] == false);
            bool residualDamagedCrosser = residualDamageRows.OfType<JObject>()
                .Any(value => (int?)value["actual"] > 0);
            check("wall-section-viability", !formationHasInvalidOuterSave &&
                targetable &&
                command.Result == UnitCommand.ResultType.Success &&
                section != null && sectionOffset <= .75f &&
                hadSummonBuff && bindingPresent && wallVisualPresent &&
                wallVisualEnabled && hiddenDonorRenderers > 0 &&
                sectionLaneAlignment >= .9f && cutPresent &&
                cutExactOwner && cutType ==
                    Pathfinding.NavmeshCut.MeshType.Rectangle.ToString() &&
                cutRegistered && gridApplyQueued && gridApplyCompleted &&
                gridFailure == null && gridGraphIndex == graphIndex &&
                gridNodesClaimed > 0 && gridNodesChanged > 0 &&
                breachAttack != null &&
                breachAttack.Result == UnitCommand.ResultType.Success &&
                automaticPhysicalHit && physicalWasObjectHalf &&
                wallAliveAfterPhysical && damageAfterPhysical > 0 &&
                fireWasFull && residualArea != null && fogArea != null &&
                residualDamagedCrosser && formationDisrupted &&
                obstruction && restored && gridNodeStateRestored &&
                gridClaimsAfterRelease == 0, row,
                "A real hidden cast forms one oriented visible section, disrupts legal formation through native SR/save ordering, auto-hits and object-halves a native weapon attack, takes full fire, transitions to owned breach cold/steam, and restores exact navigation; targeting/LoS remains separately disclosed.");
        }

        private static bool InfernalEarlyWallObstructs35(JObject before,
            JObject candidate)
        {
            return (bool?)candidate["available"] == false ||
                (double?)candidate["length"] >
                    (double?)before["length"] + .15 ||
                (int?)candidate["points"] > (int?)before["points"] ||
                (string)candidate["standingObstacleResult"] !=
                    (string)before["standingObstacleResult"];
        }

        private static bool InfernalEarlyWallRestored35(JObject before,
            JObject candidate)
        {
            return (bool?)before["available"] == true &&
                (bool?)candidate["available"] == true &&
                Math.Abs((double)candidate["length"] -
                    (double)before["length"]) <= .15 &&
                (string)candidate["standingObstacleResult"] ==
                    (string)before["standingObstacleResult"];
        }

        private bool InfernalEarlyWallLane35(
            ExpandedSummoningCorrectionFixture fixture, out Vector3 start,
            out Vector3 midpoint, out Vector3 destination,
            out Vector3 casterPoint, out JArray attempts)
        {
            attempts = new JArray();
            start = midpoint = destination = casterPoint = Vector3.zero;
            var anchor = AstarPath.active.GetNearest(fixture.Caster.Position);
            if (anchor.node == null || !anchor.node.Walkable) return false;
            UnitEntityData[] party = Game.Instance.Player.Party.Where(value =>
                value != null && value.IsInGame).ToArray();
            // Four metres on each side keeps the path witness outside the
            // printed five-foot formation-save band while the section still
            // materially changes this otherwise direct route.
            const float laneLength = 8f;
            for (int dx = -15; dx <= 15; dx += 3)
            for (int dz = -15; dz <= 15; dz += 3)
            for (int index = 0; index < CompassOffsets.Length; index++)
            {
                Vector3 requestedStart = anchor.clampedPosition +
                    new Vector3(dx, 0f, dz);
                Vector3 direction = CompassOffsets[index];
                var from = AstarPath.active.GetNearest(requestedStart);
                Vector3 requestedEnd = from.clampedPosition +
                    direction * laneLength;
                var to = AstarPath.active.GetNearest(requestedEnd);
                bool nodes = from.node != null && to.node != null &&
                    from.node.Walkable && to.node.Walkable &&
                    from.node.Area == anchor.node.Area &&
                    to.node.Area == anchor.node.Area &&
                    from.node.GraphIndex == anchor.node.GraphIndex &&
                    to.node.GraphIndex == anchor.node.GraphIndex &&
                    Vector3.Distance(from.clampedPosition,
                        requestedStart) <= .6f &&
                    Vector3.Distance(to.clampedPosition,
                        requestedEnd) <= .6f;
                Vector3 traced = nodes ? ObstacleAnalyzer.TraceAlongNavmesh(
                    from.clampedPosition, to.clampedPosition) : Vector3.zero;
                bool direct = nodes && Vector3.Distance(traced,
                    to.clampedPosition) < .1f;
                Vector3 middle = nodes ? (from.clampedPosition +
                    to.clampedPosition) * .5f : Vector3.zero;
                Vector3 side = new Vector3(-direction.z, 0f, direction.x);
                Pathfinding.NNInfo offLane = default(Pathfinding.NNInfo);
                bool caster = false;
                foreach (float sign in new[] { 1f, -1f })
                {
                    Vector3 requestedCaster = middle + side * sign * 4f;
                    var candidate = AstarPath.active.GetNearest(
                        requestedCaster);
                    bool exact = candidate.node != null &&
                        candidate.node.Walkable &&
                        candidate.node.Area == anchor.node.Area &&
                        candidate.node.GraphIndex == anchor.node.GraphIndex &&
                        Vector3.Distance(candidate.clampedPosition,
                            requestedCaster) <= .6f &&
                        !Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance
                            .HasObstacle(candidate.clampedPosition +
                                Vector3.up * 1.2f,
                                middle + Vector3.up * 1.2f, 0);
                    if (!exact) continue;
                    float partyGap = party.Length == 0 ? laneLength :
                        party.Min(value => Math.Min(Vector3.Distance(
                            value.Position, from.clampedPosition), Math.Min(
                            Vector3.Distance(value.Position,
                                to.clampedPosition), Vector3.Distance(
                                value.Position,
                                candidate.clampedPosition))));
                    if (partyGap < 3.5f) continue;
                    offLane = candidate;
                    caster = true;
                    break;
                }
                if (attempts.Count < 96) attempts.Add(new JObject {
                    ["dx"] = dx,
                    ["dz"] = dz,
                    ["index"] = index,
                    ["nodes"] = nodes,
                    ["direct"] = direct,
                    ["caster"] = caster,
                    ["from"] = nodes ? Vec(from.clampedPosition) : null,
                    ["to"] = nodes ? Vec(to.clampedPosition) : null,
                    ["casterPoint"] = caster ?
                        Vec(offLane.clampedPosition) : null,
                    ["traceGap"] = nodes ?
                        Vector3.Distance(traced, to.clampedPosition) : -1f
                });
                if (!direct || !caster) continue;
                start = from.clampedPosition;
                destination = to.clampedPosition;
                midpoint = middle;
                casterPoint = offLane.clampedPosition;
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
