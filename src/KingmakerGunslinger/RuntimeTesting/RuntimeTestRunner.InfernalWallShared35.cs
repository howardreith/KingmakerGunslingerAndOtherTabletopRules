using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private InfernalWallRun35 _infernalWallRun35;
        private readonly List<InfernalWallFixturePlan35> _infernalWallPlans35 =
            new List<InfernalWallFixturePlan35>();
        private InfernalWallFixturePlan35 _infernalWallExpiryPlan35;

        // One grid node's two native walkability layers and the Wall claim.
        private struct InfernalWallNodeState35
        {
            internal bool Static, Walkable, Erosion;
            internal int Dynamic, Owners;

            internal bool SameNavigation(InfernalWallNodeState35 other)
            {
                return Static == other.Static && Walkable == other.Walkable &&
                    Erosion == other.Erosion && Dynamic == other.Dynamic && Owners == other.Owners;
            }
        }

        private sealed class InfernalWallSharedSection35
        {
            internal string Label;
            internal UnitEntityData Source, Section;
            internal InfernalWallNavmesh35 Binding;
            internal Pathfinding.NavmeshCut Cut;
            internal int CutInstance;
            internal Bounds CutBounds;
            internal readonly HashSet<Pathfinding.GraphNode> Claimed = new HashSet<Pathfinding.GraphNode>();
            internal readonly Dictionary<Pathfinding.GraphNode, int> Dynamic =
                new Dictionary<Pathfinding.GraphNode, int>();
            internal HashSet<Pathfinding.GraphNode> Affected()
            { return new HashSet<Pathfinding.GraphNode>(Claimed.Concat(Dynamic.Keys)); }
        }

        private sealed class InfernalWallSharedGeometry35
        {
            internal Vector3 CenterB, ForwardB, CasterB, RouteStartB, RouteEndB;
            internal float AngleDegrees;
            internal int PredictedShared, PredictedSharedClaimed;
            internal bool SharedReachable;
            internal JObject Survey;
        }

        private static InfernalWallNodeState35 InfernalWallNodeOf35(Pathfinding.GraphNode node)
        {
            var grid = node as Pathfinding.GridNode;
            return new InfernalWallNodeState35 { Static = node.StaticWalkable, Walkable = node.Walkable,
                Erosion = grid != null && grid.WalkableErosion, Dynamic = node.DynamicBlocked,
                Owners = InfernalWallNavmesh35.ClaimOwnerCount(node) };
        }

        private static JObject InfernalWallNodeDocument35(InfernalWallNodeState35 state)
        {
            return new JObject { ["static"] = state.Static, ["walkable"] = state.Walkable,
                ["erosion"] = state.Erosion, ["dynamicBlocked"] = state.Dynamic, ["claimOwners"] = state.Owners };
        }

        // The grid graph Wall claims use: the nearest GridGraph to the point,
        // exactly as the section binding selects it.
        private static Pathfinding.GridGraph InfernalWallClaimGrid35(Vector3 point, out int index)
        {
            index = -1;
            Pathfinding.GridGraph best = null;
            float distance = float.PositiveInfinity;
            for (int candidate = 0; candidate < AstarPath.active.graphs.Length; candidate++)
            {
                var grid = AstarPath.active.graphs[candidate] as Pathfinding.GridGraph;
                if (grid == null || grid.nodes == null) continue;
                Pathfinding.NNInfo nearest = grid.GetNearest(point, Pathfinding.NNConstraint.None);
                if (nearest.node == null) continue;
                float gap = Vector3.Distance(nearest.clampedPosition, point);
                if (gap >= distance) continue;
                distance = gap; best = grid; index = candidate;
            }
            if (best == null) throw new InvalidOperationException("Two-Wall region has no native GridGraph.");
            return best;
        }

        private static Dictionary<Pathfinding.GraphNode, InfernalWallNodeState35> InfernalWallRegion35(
            Pathfinding.GridGraph grid, int index, Vector3 center, float half)
        {
            var result = new Dictionary<Pathfinding.GraphNode, InfernalWallNodeState35>();
            List<Pathfinding.GraphNode> nodes = grid.GetNodesInArea(new Bounds(center, new Vector3(half * 2f, 10f, half * 2f)));
            try
            {
                foreach (var node in nodes)
                    if (node != null && node.GraphIndex == (uint)index) result[node] = InfernalWallNodeOf35(node);
            }
            finally { Pathfinding.Util.ListPool<Pathfinding.GraphNode>.Release(nodes); }
            return result;
        }

        private static Dictionary<Pathfinding.GraphNode, InfernalWallNodeState35> InfernalWallSnapshot35(
            IEnumerable<Pathfinding.GraphNode> nodes)
        {
            return nodes.ToDictionary(value => value, InfernalWallNodeOf35);
        }

        private static JArray InfernalWallForeignCuts35()
        {
            return new JArray(Pathfinding.NavmeshCut.GetAll().Where(value => value != null &&
                value.GetComponent<InfernalWallNavmesh35>() == null).OrderBy(value => value.GetInstanceID()).Select(value =>
                new JObject { ["instance"] = value.GetInstanceID(), ["name"] = value.name,
                    ["enabled"] = value.enabled, ["gridGuoApplied"] = value.gridGuoApplied }));
        }

        private static long InfernalWallRegionSignature35(IEnumerable<Pathfinding.GraphNode> nodes)
        {
            long signature = 17;
            foreach (var node in nodes)
                signature = signature * 31 + node.DynamicBlocked * 7 + (node.StaticWalkable ? 3 : 0) +
                    (node.Walkable ? 1 : 0) + InfernalWallNavmesh35.ClaimOwnerCount(node) * 11;
            return signature;
        }

        // Waits until the region's native node state stops changing: queued
        // graph updates and claim work items have both been processed.
        private IEnumerable<int> InfernalWallRegionSettle35(ICollection<Pathfinding.GraphNode> nodes, Func<bool> ready)
        {
            long previous = long.MinValue;
            int stable = 0;
            DateTime deadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < deadline && (stable < 10 || !ready()))
            {
                Game.Instance.IsPaused = false; yield return 0;
                long signature = InfernalWallRegionSignature35(nodes);
                stable = signature == previous ? stable + 1 : 0;
                previous = signature;
            }
        }

        private static JObject InfernalWallNodeIdentity35(Pathfinding.GraphNode node)
        {
            return new JObject { ["graph"] = (int)node.GraphIndex, ["node"] = node.NodeIndex,
                ["position"] = SurveyVector((Vector3)node.position) };
        }

        private static AbilityData InfernalWallSharedAbility35(InfernalWallRun35 run, UnitEntityData source)
        {
            var grant = source.Descriptor.Abilities.GetAbility(run.Wall);
            if (grant == null) throw new InvalidOperationException("Two-Wall Bone Devil lacks its registered wall grant.");
            return new AbilityData(grant);
        }

        private static float InfernalWallSegmentToFootprint35(Vector3 start, Vector3 end,
            Vector3 center, Vector3 along, Vector3 forward)
        {
            float best = float.PositiveInfinity;
            for (int step = 0; step <= 40; step++)
                best = Math.Min(best, InfernalWallPlacementRuntime35.DistanceToFootprint(
                    Vector3.Lerp(start, end, step / 40f), center, along, forward));
            return best;
        }

        // Two independently sourced sections. Wall B is placed only where its
        // own ordinary target check passes; the survey looks for legal places
        // whose native cut box overlaps Wall A's affected cells.
        // A standing point for a new Wall source that the native fog of war
        // reveals. Installed FogOfWarController.Tick reveals a non-party unit
        // only within 11.7 m of a living party member (its revealers; the
        // fixture mover is a spawned caster, not one of them) with no
        // LineOfSightGeometry obstacle between the two points raised by the
        // fog's ShadowCullingHeightOffset. Lanes are chosen at least 3.5 m
        // from the party, so the mover's side can be outside every reveal.
        // The point is walkable native floor 3-6 m from that party member
        // (well inside reveal range), clear of other units by 1.5 m, with the
        // smallest floor step from the party member found (at most 1 m; the
        // 0.25 m limit first used rejected every point on a sloped area).
        // Every rejected candidate is recorded.
        private static JObject InfernalWallSharedSourcePoint35(UnitEntityData mover, UnitEntityData self, List<int> used)
        {
            float lift = 0f;
            try
            {
                var mask = Kingmaker.Visual.LocationMaskRenderer.Instance;
                if (mask != null && mask.FogOfWar != null) lift = mask.FogOfWar.ShadowCullingHeightOffset;
            }
            catch (Exception) { lift = 0f; }
            Vector3 raise = Vector3.up * lift;
            UnitEntityData[] revealers = Game.Instance.Player.Party.Where(unit => unit != null &&
                    unit.View != null && unit.IsViewActive && !unit.Descriptor.State.IsDead && unit.IsPlayerFaction)
                .OrderBy(unit => Vector3.Distance(unit.Position, mover.Position)).ToArray();
            var rejected = new JArray();
            JObject best = null; float bestStep = float.PositiveInfinity; int bestKey = 0;
            foreach (UnitEntityData revealer in revealers)
                foreach (float distance in new[] { 3f, 4.5f, 6f })
                    for (int index = 0; index < CompassOffsets.Length; index++)
                    {
                        string key = revealer.UniqueId + "/" + index;
                        if (used.Contains(key.GetHashCode())) continue;
                        Vector3 candidate = revealer.Position + CompassOffsets[index] * distance;
                        var nearest = AstarPath.active == null ? default(Pathfinding.NNInfo) : AstarPath.active.GetNearest(candidate);
                        string reason = null;
                        if (nearest.node == null || !nearest.node.Walkable) reason = "no-walkable-node";
                        else
                        {
                            Vector3 floor = nearest.clampedPosition;
                            Vector3 flat = floor - candidate; flat.y = 0f;
                            bool revealed;
                            try
                            {
                                revealed = Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance != null &&
                                    !Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(
                                        revealer.View.transform.position + raise, floor + raise, 0);
                            }
                            catch (Exception) { revealed = false; }
                            if (flat.magnitude > .3f) reason = "node-off-candidate";
                            else if (Mathf.Abs(floor.y - revealer.Position.y) > 1f) reason = "floor-step";
                            else if (!revealed) reason = "fog-line-blocked";
                            else if (Game.Instance.State.Units.All.Any(unit => unit != self && unit.IsInGame &&
                                !unit.Descriptor.State.IsFinallyDead && Vector3.Distance(unit.Position, floor) < 1.5f))
                                reason = "occupied";
                            float step = Mathf.Abs(floor.y - revealer.Position.y);
                            if (reason == null && step < bestStep)
                            {
                                bestStep = step; bestKey = key.GetHashCode();
                                best = new JObject { ["point"] = SurveyVector(floor), ["revealer"] = revealer.UniqueId,
                                    ["revealerPosition"] = SurveyVector(revealer.Position),
                                    ["revealerDistance"] = Vector3.Distance(revealer.Position, floor),
                                    ["fogHeightOffset"] = lift, ["direction"] = index, ["distance"] = distance,
                                    ["moverDistance"] = Vector3.Distance(mover.Position, floor), ["floorStep"] = step,
                                    ["rejected"] = rejected };
                            }
                            if (reason == null) continue;
                            rejected.Add(new JObject { ["revealer"] = revealer.UniqueId, ["direction"] = index,
                                ["distance"] = distance, ["reason"] = reason, ["floorStep"] = step });
                            continue;
                        }
                        rejected.Add(new JObject { ["revealer"] = revealer.UniqueId, ["direction"] = index,
                            ["distance"] = distance, ["reason"] = reason });
                    }
            if (best != null) { used.Add(bestKey); return best; }
            throw new InvalidOperationException("No fog-revealed level floor beside a party member for a Wall source: " +
                "revealers=" + revealers.Length + ";" + rejected.ToString(Formatting.None));
        }

        private IEnumerable<int> InfernalWallShared35(ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> owned, Action<string, bool, JObject, string> check)
        {
            var run = _infernalWallRun35;
            if (run == null) throw new InvalidOperationException("Two-Wall acceptance requires the completed Wall run.");
            var receipt = new JObject { ["scope"] = "Two Wall sections from two separate Bone Devils, formed by ordinary native commands; each retires by the established controlled original-deadline expiry seam. No counters, walkability or sections are written by the observer." };
            string receiptPath = Path.Combine(_request.EvidenceDirectory, "s35-wall-shared.json");
            Action persist = () => RuntimeTestResultWriter.WriteAtomic(receiptPath, receipt.ToString(Formatting.Indented));
            var mover = fixture.Caster;

            var sourceA = CastExpandedSummoningOwnTier(fixture, "bone-devil");
            var sourceB = CastExpandedSummoningOwnTier(fixture, "bone-devil");
            owned.Add(sourceA); owned.Add(sourceB);
            // The fixture summon appears at its own spawn point; the native
            // fader shows a non-party body only where the fog of war reveals
            // it. Three attempts beside the mover stayed faded out (11 m away;
            // 2.5 m across a floor step; 2.5 m on level, clear-sight floor):
            // the mover is not a fog revealer. Each source now stands where a
            // living party member's native reveal reaches it; the Wall
            // prerequisite and cast steps place it on its lane afterwards.
            var usedDirections = new List<int>();
            var placements = new JArray();
            foreach (var source in new[] { sourceA, sourceB })
            {
                JObject placement = InfernalWallSharedSourcePoint35(mover, source, usedDirections);
                Vector3 point = new Vector3((float)placement["point"][0], (float)placement["point"][1], (float)placement["point"][2]);
                // Both sources stop their own AI at once: the second source
                // otherwise walked ~19 m out of reveal range during the first
                // source's appearance wait (close9).
                InfernalWallPrepareActor35(source);
                PlaceExpandedSummoningUnit(source, point);
                placement["source"] = source.UniqueId;
                placement["placed"] = SurveyVector(source.Position);
                placements.Add(placement);
            }
            receipt["sourcesPlacedBesideParty"] = new JObject { ["mover"] = SurveyVector(mover.Position),
                ["moverInFog"] = mover.IsInFogOfWar, ["placements"] = placements };
            persist();
            yield return 0;
            foreach (JObject placement in placements)
            {
                var source = (string)placement["source"] == sourceA.UniqueId ? sourceA : sourceB;
                placement["inFogAfterFrame"] = source.IsInFogOfWar;
                placement["nativeVisibleAfterFrame"] = Kingmaker.Controllers.EntityVisibilityForPlayerController.IsVisible(source);
                placement["positionAfterFrame"] = SurveyVector(source.Position);
            }
            persist();
            foreach (var source in new[] { sourceA, sourceB })
            {
                InfernalWallPrepareActor35(source);
                foreach (int frame in Outsider30NativeAppearance(source, source.Blueprint.name)) yield return frame;
                InfernalWallPrepareActor35(source);
            }
            var sources = new[] { run.Bone, sourceA, sourceB };
            // Each source's fear aura also frightens the other source (close17:
            // source B carried source A's Frightened and never settled). The
            // established fixture clears that exact effect from the mover; the
            // two sources get the same exact-source, exact-blueprint clearing.
            Action clearFear = () => {
                foreach (var victim in new[] { mover, sourceA, sourceB })
                    InfernalBoneRemoveEffects35(victim, sources, run.Frightened);
            };
            clearFear();
            AbilityData dataA = InfernalWallSharedAbility35(run, sourceA), dataB = InfernalWallSharedAbility35(run, sourceB);
            receipt["sources"] = new JObject { ["a"] = sourceA.UniqueId, ["b"] = sourceB.UniqueId,
                ["aSummoner"] = sourceA.Get<UnitPartSummonedMonster>()?.Summoner?.UniqueId,
                ["bSummoner"] = sourceB.Get<UnitPartSummonedMonster>()?.Summoner?.UniqueId };
            persist();

            // The lane prerequisite stands every owned actor, both new sources
            // included, at the casting point and isolates only its given
            // source's fear. A first exposure to the other source's aura then
            // landed mid-walk (close18: failed DC 19 Will save, the mover fled,
            // its move was interrupted). The native aura grants its per-source
            // repeat immunity with that first exposure, as it already has for
            // the original Bone Devil. Let that exposure happen here, beside
            // each new source, then clear only the resulting fright.
            var immunity = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol[
                ExpandedSummoningSpecialBuilder.Bone35Prefix + "FearImmunity"];
            var exposures = new JArray();
            foreach (var source in new[] { sourceA, sourceB })
            {
                var exposure = new JObject { ["source"] = source.UniqueId,
                    ["immuneBefore"] = InfernalBoneBuff35(mover, immunity, source) != null };
                if (!(bool)exposure["immuneBefore"])
                {
                    Vector3 side = Vector3.Cross(Vector3.up, source.OrientationDirection); side.y = 0f;
                    if (side.sqrMagnitude < .01f) side = Vector3.right;
                    PlaceExpandedSummoningUnit(mover, source.Position + side.normalized * 1.2f);
                    DateTime until = DateTime.UtcNow.AddSeconds(12);
                    while (DateTime.UtcNow < until && InfernalBoneBuff35(mover, immunity, source) == null)
                    { Game.Instance.IsPaused = false; yield return 0; }
                }
                exposure["immuneAfter"] = InfernalBoneBuff35(mover, immunity, source) != null;
                exposures.Add(exposure);
                clearFear();
            }
            receipt["moverAuraExposures"] = exposures;
            persist();

            var plan = InfernalWallNextLane35(run, "shared", _infernalWallPlans35.ToArray());
            foreach (int frame in InfernalWallFixturePrerequisite35(fixture, sourceA, owned, plan, check, "shared")) yield return frame;
            // The prerequisite stands every owned actor at Wall A's casting
            // point. Those bystanders are adjacent creatures for any nearby
            // Wall B (close20: every candidate had two). Only source A needs
            // that point; the others stand well clear of both Walls.
            Vector3 away = plan.CastingPoint - plan.Midpoint; away.y = 0f;
            if (away.sqrMagnitude < .01f) away = Vector3.forward;
            Vector3 parkingCentre = plan.CastingPoint + away.normalized * 8f;
            Vector3 parkingSide = Vector3.Cross(Vector3.up, away.normalized);
            var parked = new JArray();
            int nextSlot = 0;
            var slots = new Dictionary<UnitEntityData, Vector3>();
            // Park keeps one slot per unit, 3 m apart (outside a 5-ft aura), on native floor.
            Action<UnitEntityData> park = unit => {
                Vector3 slot;
                if (!slots.TryGetValue(unit, out slot))
                {
                    int index = nextSlot++;
                    float offset = ((index + 1) / 2) * 3f * (index % 2 == 0 ? 1f : -1f);
                    slot = AstarPath.active.GetNearest(parkingCentre + parkingSide * offset).clampedPosition;
                    slots[unit] = slot;
                }
                InfernalWallPrepareActor35(unit);
                PlaceExpandedSummoningUnit(unit, slot);
            };
            // Both sources have cast by the time a live walk starts; their
            // 5-ft fear auras stand clear of every lane for the walk.
            Action parkSources = () => { park(sourceA); park(sourceB); clearFear(); };
            Vector3 parking = AstarPath.active.GetNearest(parkingCentre).clampedPosition;
            foreach (var bystander in owned.Where(value => value != null && !value.Destroyed && value.IsInState &&
                    !value.Descriptor.State.IsDead && value != sourceA && value != sourceB && value != mover &&
                    value.Blueprint?.AssetGuid != InfernalRules35.WallSectionUnitGuid).ToArray())
            {
                park(bystander);
                parked.Add(new JObject { ["unit"] = bystander.UniqueId, ["position"] = SurveyVector(bystander.Position) });
            }
            receipt["bystandersParked"] = new JObject { ["point"] = SurveyVector(parking),
                ["distanceFromWallA"] = Vector3.Distance(parking, plan.Midpoint), ["units"] = parked };
            persist();
            clearFear();

            int gridIndex;
            Pathfinding.GridGraph grid = InfernalWallClaimGrid35(plan.Midpoint, out gridIndex);
            var baseline = InfernalWallRegion35(grid, gridIndex, plan.Midpoint, 6.5f);
            var region = baseline.Keys.ToList();
            JArray foreignCutsBefore = InfernalWallForeignCuts35();
            receipt["lane"] = plan.Document();
            receipt["grid"] = new JObject { ["index"] = gridIndex, ["nodeSize"] = grid.nodeSize,
                ["erodeIterations"] = grid.erodeIterations, ["erosionUseTags"] = grid.erosionUseTags,
                ["regionNodes"] = baseline.Count,
                ["regionHalfExtentMeters"] = 6.5f,
                ["baselineUnwalkable"] = baseline.Values.Count(value => !value.Walkable),
                ["baselineStaticUnwalkable"] = baseline.Values.Count(value => !value.Static),
                ["baselineDynamicBlocked"] = baseline.Values.Count(value => value.Dynamic > 0),
                ["baselineClaims"] = baseline.Values.Count(value => value.Owners > 0),
                ["foreignCutsBefore"] = foreignCutsBefore };
            persist();

            InfernalWallSharedGeometry35 geometry = null;
            InfernalWallFixturePlan35 planB = null;
            foreach (bool aFirst in new[] { true, false })
            {
                string pair = aFirst ? "a-first" : "b-first";
                // Short tag for move and placement receipt names: the full
                // pair name pushed a receipt's temporary path past the
                // Windows 260-character limit (close23).
                string tag = aFirst ? "af" : "bf";
                var pairRow = new JObject();
                receipt[pair] = pairRow;
                var start = InfernalWallSnapshot35(region);
                pairRow["baselineMismatchesAtStart"] = region.Count(node => !start[node].SameNavigation(baseline[node]));

                // The mover stands in the parking area for every cast and for
                // the Wall B survey (close20: at lane A's start it was adjacent
                // to every candidate); live movers place it on their lanes.
                park(mover);
                // Source B's casting point may lie in Wall A's formation band;
                // it waits in the parking area until its own cast.
                park(sourceB);
                clearFear();
                var a = new InfernalWallSharedSection35 { Label = pair + "-a", Source = sourceA };
                JObject castA = null;
                foreach (int frame in InfernalWallSharedCast35(run, a, dataA, plan.CastingPoint, plan.Midpoint, sources, region, value => castA = value)) yield return frame;
                pairRow["castA"] = castA;
                var afterA = InfernalWallSnapshot35(region);
                foreach (var node in region)
                {
                    if (afterA[node].Dynamic > baseline[node].Dynamic) a.Dynamic[node] = afterA[node].Dynamic - baseline[node].Dynamic;
                    if (InfernalWallNavmesh35.ClaimOwnedBy(node, a.Binding)) a.Claimed.Add(node);
                }
                castA["claimedInRegion"] = a.Claimed.Count;
                castA["dynamicNodes"] = a.Dynamic.Count;
                // Source A's job is done until the next pair's cast, which
                // stands it on its casting point again; until then it is not
                // a creature adjacent to Wall B (close20).
                park(sourceA);
                clearFear();
                persist();

                if (geometry == null)
                {
                    InfernalWallSharedGeometry35 found = null;
                    foreach (int frame in InfernalWallSharedSurvey35(run, a, plan, grid, gridIndex, sourceB, dataB, value => found = value)) yield return frame;
                    geometry = found;
                    receipt["geometry"] = geometry.Survey;
                    persist();
                    planB = new InfernalWallFixturePlan35(geometry.RouteStartB, geometry.RouteEndB, geometry.CenterB,
                        geometry.CasterB, new JObject { ["source"] = "two-Wall route survey" });
                    check("wall-shared-geometry", geometry.CenterB != Vector3.zero, geometry.Survey,
                        geometry.SharedReachable ?
                            "A legal ordinary placement for a second Wall overlaps the first Wall's affected grid cells." :
                            "No legal placement shares affected grid cells; the nearest legal independent placement is used and the limit is recorded.");
                }

                clearFear();
                // Source B is stood on its casting point by the cast helper,
                // which first interrupts every command (close25: a stray
                // native UnitMoveTo moved it off a settled-placement check).
                var b = new InfernalWallSharedSection35 { Label = pair + "-b", Source = sourceB };
                JObject castB = null;
                foreach (int frame in InfernalWallSharedCast35(run, b, dataB, geometry.CasterB, geometry.CenterB, sources, region, value => castB = value)) yield return frame;
                pairRow["castB"] = castB;
                var afterAB = InfernalWallSnapshot35(region);
                foreach (var node in region)
                {
                    if (afterAB[node].Dynamic > afterA[node].Dynamic) b.Dynamic[node] = afterAB[node].Dynamic - afterA[node].Dynamic;
                    if (InfernalWallNavmesh35.ClaimOwnedBy(node, b.Binding)) b.Claimed.Add(node);
                }
                castB["claimedInRegion"] = b.Claimed.Count;
                castB["dynamicNodes"] = b.Dynamic.Count;
                var aAffected = a.Affected();
                var bAffected = b.Affected();
                var shared = new HashSet<Pathfinding.GraphNode>(aAffected.Where(bAffected.Contains));
                int doubleOwned = region.Count(node => InfernalWallNavmesh35.ClaimOwnedBy(node, a.Binding) &&
                    InfernalWallNavmesh35.ClaimOwnedBy(node, b.Binding));
                bool claimsOutsideRegion = (a.Binding?.GridNodeSnapshot() ?? new KeyValuePair<Pathfinding.GraphNode, bool>[0])
                    .Concat(b.Binding?.GridNodeSnapshot() ?? new KeyValuePair<Pathfinding.GraphNode, bool>[0])
                    .Any(value => !baseline.ContainsKey(value.Key));
                // The recorded claim originals against the independent baseline:
                // the OriginalWalkable concern needs a claim that recorded a
                // combined-walkability false where the static layer was true.
                var recordedOriginals = (a.Binding?.GridNodeSnapshot() ?? new KeyValuePair<Pathfinding.GraphNode, bool>[0])
                    .Concat(b.Binding?.GridNodeSnapshot() ?? new KeyValuePair<Pathfinding.GraphNode, bool>[0]).ToArray();
                int originalsDifferingFromStatic = recordedOriginals.Count(value =>
                    baseline.ContainsKey(value.Key) && value.Value != baseline[value.Key].Static);
                var sharedRows = new JArray(shared.Select(node => {
                    var row = InfernalWallNodeIdentity35(node);
                    row["baseline"] = InfernalWallNodeDocument35(baseline[node]);
                    row["afterA"] = InfernalWallNodeDocument35(afterA[node]);
                    row["afterAB"] = InfernalWallNodeDocument35(afterAB[node]);
                    row["claimedByA"] = a.Claimed.Contains(node); row["claimedByB"] = b.Claimed.Contains(node);
                    row["dynamicFromA"] = a.Dynamic.ContainsKey(node) ? a.Dynamic[node] : 0;
                    row["dynamicFromB"] = b.Dynamic.ContainsKey(node) ? b.Dynamic[node] : 0;
                    return row; }));
                pairRow["sharedNodes"] = sharedRows;
                pairRow["sharedNodeCount"] = shared.Count;
                pairRow["sharedClaimedByA"] = shared.Count(a.Claimed.Contains);
                pairRow["sharedClaimedByB"] = shared.Count(b.Claimed.Contains);
                pairRow["doubleOwnedClaims"] = doubleOwned;
                pairRow["claimsOutsideRegion"] = claimsOutsideRegion;
                pairRow["claimOriginalsDifferingFromBaselineStatic"] = originalsDifferingFromStatic;
                pairRow["aNodes"] = new JArray(aAffected.Select(node => InfernalWallSharedNodeRow35(node, a, b, baseline, afterAB)));
                pairRow["bNodes"] = new JArray(bAffected.Where(node => !aAffected.Contains(node)).Select(node =>
                    InfernalWallSharedNodeRow35(node, a, b, baseline, afterAB)));
                persist();
                // The ice itself: every grid node whose centre lies inside a
                // section's footprint must be blocked. Claimed ring nodes
                // (centre outside, square touching) can be rewritten by any
                // native grid update while the Wall stands: installed
                // GridGraph.UpdateArea with updateErosion and erodeIterations
                // 10 resets static walkability from the pre-erosion flag
                // within ~10 m (close21: in combat, 12 of Wall A's 24 claimed
                // nodes). That is recorded, not treated as formation failure.
                Func<InfernalWallSharedSection35, Pathfinding.GraphNode, bool> insideIce = (wall, node) =>
                    wall.Section != null && InfernalWallPlacementRuntime35.DistanceToFootprint((Vector3)node.position,
                        wall.Section.Position, Vector3.Cross(Vector3.up, wall.Section.OrientationDirection).normalized,
                        wall.Section.OrientationDirection) <= 0f;
                var iceNodes = aAffected.Where(node => insideIce(a, node)).Concat(bAffected.Where(node => insideIce(b, node))).ToArray();
                var ringOpen = aAffected.Concat(bAffected).Where(node => afterAB[node].Walkable && !iceNodes.Contains(node)).ToArray();
                pairRow["iceNodes"] = iceNodes.Length;
                pairRow["iceNodesWalkable"] = iceNodes.Count(node => afterAB[node].Walkable);
                pairRow["claimedRingNodesRewrittenWalkable"] = new JArray(ringOpen.Select(node => {
                    var row = InfernalWallNodeIdentity35(node);
                    row["afterAB"] = InfernalWallNodeDocument35(afterAB[node]);
                    return row; }));
                bool bothBlocked = iceNodes.Length > 0 && iceNodes.All(node => !afterAB[node].Walkable);
                // Legal placement never lets two claims share a node (the
                // second footprint must be walkable), and outside turn-based
                // combat no cut writes grid counters. The overlap that exists
                // natively is one Wall's claimed nodes inside the other Wall's
                // cut box: the box a native grid update rewrites.
                Func<JObject, Bounds?> cutBox = cast => {
                    var bounds = cast?["cutBounds"] as JObject;
                    if (bounds == null) return null;
                    return new Bounds(new Vector3((float)bounds["center"][0], 0f, (float)bounds["center"][2]),
                        new Vector3((float)bounds["size"][0], 2f, (float)bounds["size"][2]));
                };
                Func<Bounds?, Pathfinding.GraphNode, bool> inBox = (box, node) => {
                    if (box == null) return false;
                    Vector3 point = (Vector3)node.position; point.y = 0f;
                    return box.Value.Contains(point);
                };
                Bounds? boxA = cutBox(castA), boxB = cutBox(castB);
                var aClaimsInBBox = a.Claimed.Where(node => inBox(boxB, node)).ToArray();
                var bClaimsInABox = b.Claimed.Where(node => inBox(boxA, node)).ToArray();
                pairRow["aClaimsInsideBCutBox"] = new JArray(aClaimsInBBox.Select(InfernalWallNodeIdentity35));
                pairRow["bClaimsInsideACutBox"] = new JArray(bClaimsInABox.Select(InfernalWallNodeIdentity35));
                pairRow["cutGridBlockApplied"] = new JObject { ["a"] = castA["cutGridBlockApplied"], ["b"] = castB["cutGridBlockApplied"] };
                int crossSurface = aClaimsInBBox.Length + bClaimsInABox.Length;
                pairRow["crossSurfaceNodes"] = crossSurface;
                persist();
                check("wall-shared-formation-" + pair, castA["section"] != null && castB["section"] != null &&
                    (bool?)castA["formed"] == true && (bool?)castB["formed"] == true &&
                    a.Section != b.Section && a.CutInstance != b.CutInstance &&
                    (string)castA["summonerOfSection"] == sourceA.UniqueId &&
                    (string)castB["summonerOfSection"] == sourceB.UniqueId &&
                    a.Claimed.Count > 0 && b.Claimed.Count > 0 && doubleOwned == 0 && !claimsOutsideRegion &&
                    bothBlocked && (!geometry.SharedReachable || crossSurface > 0),
                    pairRow, "Two ordinary commands from separate Bone Devils form two independently owned sections whose native cuts and claims block every affected cell; where a legal placement reaches it, one Wall's claimed cells lie inside the other Wall's native cut box.");

                if (aFirst)
                {
                    parkSources(); // 5-ft fear aura beside a lane (close24)
                    foreach (int frame in InfernalEarlyWallMove35(fixture, plan.Start, plan.Destination, plan.Midpoint,
                        true, "shared-" + tag + "-both-a", check, plan, false, a.Section)) yield return frame;
                    parkSources(); // 5-ft fear aura beside a lane (close24)
                    foreach (int frame in InfernalEarlyWallMove35(fixture, planB.Start, planB.Destination, planB.Midpoint,
                        true, "shared-" + tag + "-both-b", check, planB, false, b.Section)) yield return frame;
                }

                var first = aFirst ? a : b;
                var survivor = aFirst ? b : a;
                var survivorPlan = aFirst ? planB : plan;
                // The state the retirement starts from. Native grid updates
                // between formation and retirement may already have rewritten
                // claimed nodes; that is recorded, not attributed to the
                // retirement.
                var beforeFirst = InfernalWallSnapshot35(region);
                bool survivorGridBlockBefore = survivor.Cut != null && survivor.Cut.gridGuoApplied;
                JObject firstRetirement = null;
                foreach (int frame in InfernalWallSharedRetire35(first, region, value => firstRetirement = value)) yield return frame;
                pairRow["firstRetired"] = first.Label;
                pairRow["firstRetirement"] = firstRetirement;
                var afterFirst = InfernalWallSnapshot35(region);
                var survivorAffected = survivor.Affected();
                var firstOnly = first.Affected().Where(node => !survivorAffected.Contains(node)).ToArray();
                var unrelated = region.Where(node => !aAffected.Contains(node) && !bAffected.Contains(node)).ToArray();
                var survivorClaimLost = survivor.Claimed.Where(node => !InfernalWallNavmesh35.ClaimOwnedBy(node, survivor.Binding) ||
                    afterFirst[node].Owners != 1).ToArray();
                var survivorIce = survivorAffected.Where(node => insideIce(survivor, node)).ToArray();
                var survivorUnblocked = survivorIce.Where(node => !beforeFirst[node].Walkable && afterFirst[node].Walkable).ToArray();
                // A retiring Wall's own native counter may overlap the survivor's
                // ice (diagonal cut box) and is released with it; the survivor
                // keeps its static block, claim and blocked state.
                var survivorChangedByRetirement = survivorIce.Where(node =>
                    afterFirst[node].Walkable != beforeFirst[node].Walkable ||
                    afterFirst[node].Static != beforeFirst[node].Static ||
                    afterFirst[node].Owners != beforeFirst[node].Owners).ToArray();
                var survivorRingRewritten = survivorAffected.Where(node => !survivorIce.Contains(node) &&
                    !afterFirst[node].SameNavigation(beforeFirst[node])).ToArray();
                var survivorWalkableBeforeRetirement = survivorAffected.Where(node => beforeFirst[node].Walkable).ToArray();
                var survivorDynamicChanged = survivorAffected.Where(node =>
                    afterFirst[node].Dynamic != beforeFirst[node].Dynamic).ToArray(); // recorded
                var firstNotRestored = firstOnly.Where(node => !afterFirst[node].SameNavigation(baseline[node])).ToArray();
                var unrelatedChanged = unrelated.Where(node => !afterFirst[node].SameNavigation(baseline[node])).ToArray();
                // Live: still the binding's own registered, enabled cut, and its
                // turn-based grid block state unchanged by the retirement (that
                // block is applied only by the turn-based combat controller, so
                // in real-time play it is legitimately absent).
                bool survivorCutLive = survivor.Binding != null && survivor.Binding.Cut != null &&
                    ReferenceEquals(survivor.Binding.Cut, survivor.Cut) && survivor.Cut.enabled &&
                    Pathfinding.NavmeshCut.GetAll().Contains(survivor.Cut) &&
                    survivor.Cut.gridGuoApplied == survivorGridBlockBefore;
                bool firstCutGone = !Pathfinding.NavmeshCut.GetAll().Any(value => value != null &&
                    value.GetInstanceID() == first.CutInstance);
                JArray foreignAfterFirst = InfernalWallForeignCuts35();
                Func<IEnumerable<Pathfinding.GraphNode>, JArray> rows = nodes => new JArray(nodes.Take(40).Select(node => {
                    var row = InfernalWallNodeIdentity35(node);
                    row["baseline"] = InfernalWallNodeDocument35(baseline[node]);
                    row["afterBoth"] = InfernalWallNodeDocument35(afterAB[node]);
                    row["beforeFirstRetired"] = InfernalWallNodeDocument35(beforeFirst[node]);
                    row["afterFirstRetired"] = InfernalWallNodeDocument35(afterFirst[node]);
                    row["claimedBySurvivor"] = survivor.Claimed.Contains(node);
                    row["dynamicFromSurvivor"] = survivor.Dynamic.ContainsKey(node) ? survivor.Dynamic[node] : 0;
                    row["sharedWithRetired"] = shared.Contains(node);
                    return row; }));
                var survivorRow = new JObject { ["survivor"] = survivor.Label, ["survivorSection"] = survivor.Section.UniqueId,
                    ["survivorCutInstance"] = survivor.CutInstance, ["survivorCutLive"] = survivorCutLive,
                    ["survivorCutGridBlockBefore"] = survivorGridBlockBefore,
                    ["survivorCutGridBlockAfter"] = survivor.Cut != null && survivor.Cut.gridGuoApplied,
                    ["retiredCutGone"] = firstCutGone,
                    ["claimsAfter"] = InfernalWallNavmesh35.ActiveGridClaimCount,
                    ["survivorClaims"] = survivor.Claimed.Count,
                    ["survivorClaimsLost"] = rows(survivorClaimLost),
                    ["survivorNodesNowWalkable"] = rows(survivorUnblocked),
                    ["survivorIceNodes"] = survivorIce.Length,
                    ["survivorNodesChangedByRetirement"] = rows(survivorChangedByRetirement),
                    ["survivorClaimedRingRewrittenByNativeUpdate"] = rows(survivorRingRewritten),
                    ["survivorNodesWalkableBeforeRetirement"] = rows(survivorWalkableBeforeRetirement),
                    ["survivorNodesWalkableBeforeRetirementCount"] = survivorWalkableBeforeRetirement.Length,
                    ["survivorDynamicChanged"] = rows(survivorDynamicChanged),
                    ["sharedNodesAfter"] = rows(shared),
                    ["retiredOnlyNodes"] = firstOnly.Length,
                    ["retiredOnlyNotAtBaseline"] = rows(firstNotRestored),
                    ["unrelatedNodes"] = unrelated.Length,
                    ["unrelatedBaselineBlocked"] = unrelated.Count(node => !baseline[node].Walkable),
                    ["unrelatedChanged"] = rows(unrelatedChanged),
                    ["foreignCutsAfter"] = foreignAfterFirst,
                    ["foreignCutsUnchanged"] = JToken.DeepEquals(foreignAfterFirst, foreignCutsBefore) };
                pairRow["survivorAfterFirstRetirement"] = survivorRow;
                persist();
                check("wall-shared-survivor-preserved-" + pair, survivorCutLive && firstCutGone &&
                    survivorClaimLost.Length == 0 && survivorUnblocked.Length == 0 &&
                    survivorChangedByRetirement.Length == 0 &&
                    firstNotRestored.Length == 0 && unrelatedChanged.Length == 0 &&
                    InfernalWallNavmesh35.ActiveGridClaimCount == survivor.Binding.GridNodeSnapshot().Length &&
                    (bool)survivorRow["foreignCutsUnchanged"], survivorRow,
                    "Retiring one Wall returns only its own cells to their baseline and leaves the other Wall's cut, claims, every layer of its cells and unrelated cells exactly as they were immediately before the retirement.");
                parkSources(); // 5-ft fear aura beside a lane (close24)
                foreach (int frame in InfernalEarlyWallMove35(fixture, survivorPlan.Start, survivorPlan.Destination,
                    survivorPlan.Midpoint, true, "shared-" + tag + "-survivor-" + (aFirst ? "b" : "a"), check,
                    survivorPlan, false, survivor.Section)) yield return frame;

                JObject secondRetirement = null;
                foreach (int frame in InfernalWallSharedRetire35(survivor, region, value => secondRetirement = value)) yield return frame;
                pairRow["secondRetirement"] = secondRetirement;
                var afterBoth = InfernalWallSnapshot35(region);
                var notBaseline = region.Where(node => !afterBoth[node].SameNavigation(baseline[node])).ToArray();
                JArray foreignAfter = InfernalWallForeignCuts35();
                var restoredRow = new JObject { ["regionNodes"] = region.Count,
                    ["nodesNotAtBaseline"] = new JArray(notBaseline.Take(40).Select(node => {
                        var row = InfernalWallNodeIdentity35(node);
                        row["baseline"] = InfernalWallNodeDocument35(baseline[node]);
                        row["after"] = InfernalWallNodeDocument35(afterBoth[node]);
                        row["inA"] = aAffected.Contains(node); row["inB"] = bAffected.Contains(node);
                        return row; })),
                    ["nodesNotAtBaselineCount"] = notBaseline.Length,
                    ["sharedNodesRestored"] = shared.All(node => afterBoth[node].SameNavigation(baseline[node])),
                    ["claimsAfter"] = InfernalWallNavmesh35.ActiveGridClaimCount,
                    ["foreignCutsAfter"] = foreignAfter,
                    ["foreignCutsUnchanged"] = JToken.DeepEquals(foreignAfter, foreignCutsBefore),
                    ["comparison"] = "Every region node's static flag, pre-erosion flag, DynamicBlocked counter, combined walkability and claim count equals its pre-Wall baseline." };
                pairRow["afterBothRetired"] = restoredRow;
                persist();
                check("wall-shared-baseline-restored-" + pair, notBaseline.Length == 0 &&
                    InfernalWallNavmesh35.ActiveGridClaimCount == 0 && (bool)restoredRow["foreignCutsUnchanged"],
                    restoredRow, "Retiring the second Wall returns every affected and unrelated cell to its recorded baseline.");
                parkSources(); // 5-ft fear aura beside a lane (close24)
                foreach (int frame in InfernalEarlyWallMove35(fixture, plan.Start, plan.Destination, plan.Midpoint,
                    false, "shared-" + tag + "-restored-a", check, plan)) yield return frame;
                parkSources(); // 5-ft fear aura beside a lane (close24)
                foreach (int frame in InfernalEarlyWallMove35(fixture, planB.Start, planB.Destination, planB.Midpoint,
                    false, "shared-" + tag + "-restored-b", check, planB)) yield return frame;
            }
        }

        private static JObject InfernalWallSharedNodeRow35(Pathfinding.GraphNode node, InfernalWallSharedSection35 a,
            InfernalWallSharedSection35 b, Dictionary<Pathfinding.GraphNode, InfernalWallNodeState35> baseline,
            Dictionary<Pathfinding.GraphNode, InfernalWallNodeState35> both)
        {
            var row = InfernalWallNodeIdentity35(node);
            row["baseline"] = InfernalWallNodeDocument35(baseline[node]);
            row["withBoth"] = InfernalWallNodeDocument35(both[node]);
            row["claimedByA"] = a.Claimed.Contains(node); row["claimedByB"] = b.Claimed.Contains(node);
            row["dynamicFromA"] = a.Dynamic.ContainsKey(node) ? a.Dynamic[node] : 0;
            row["dynamicFromB"] = b.Dynamic.ContainsKey(node) ? b.Dynamic[node] : 0;
            return row;
        }

        // The summoner's place for a cast: behind the casting source, within
        // the native 11.7 m follow distance, outside every standing section's
        // formation band.
        private static JObject InfernalWallSummonerBeside35(InfernalWallRun35 run, Vector3 casterPoint, Vector3 targetPoint)
        {
            var summoner = run.Fixture.Caster;
            Vector3 back = casterPoint - targetPoint; back.y = 0f;
            if (back.sqrMagnitude < .01f) back = Vector3.back;
            back.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, back);
            var sections = Game.Instance.State.Units.All.Where(value => value != null && !value.Destroyed && value.IsInState &&
                value.Blueprint?.AssetGuid == InfernalRules35.WallSectionUnitGuid).ToArray();
            foreach (Vector3 offset in new[] { back * 3.5f, back * 3.5f + side * 2f, back * 3.5f - side * 2f, back * 5f })
            {
                var nearest = AstarPath.active.GetNearest(casterPoint + offset);
                if (nearest.node == null || !nearest.node.Walkable) continue;
                Vector3 spot = nearest.clampedPosition;
                if (Vector3.Distance(spot, casterPoint + offset) > .5f) continue;
                if (sections.Any(section => InfernalWallPlacementRuntime35.DistanceToFootprint(spot, section.Position,
                        Vector3.Cross(Vector3.up, section.OrientationDirection).normalized, section.OrientationDirection) < 2.5f)) continue;
                InfernalWallPrepareActor35(summoner);
                PlaceExpandedSummoningUnit(summoner, spot);
                return new JObject { ["point"] = SurveyVector(summoner.Position), ["distanceToCaster"] = Vector3.Distance(summoner.Position, casterPoint) };
            }
            return new JObject { ["point"] = null, ["reason"] = "no clear spot behind the caster; summoner left in place",
                ["distanceToCaster"] = Vector3.Distance(summoner.Position, casterPoint) };
        }

        private IEnumerable<int> InfernalWallSharedCast35(InfernalWallRun35 run, InfernalWallSharedSection35 formed,
            AbilityData data, Vector3 casterPoint, Vector3 point, UnitEntityData[] sources,
            ICollection<Pathfinding.GraphNode> region, Action<JObject> result)
        {
            foreach (int frame in InfernalBoneWaitGame35(6.2, 15)) yield return frame;
            var caster = formed.Source;
            caster.Commands.InterruptAll(true);
            caster.Commands.RemoveFinishedAndUpdateQueue();
            if (Vector3.Distance(caster.Position, casterPoint) > .05f) PlaceExpandedSummoningUnit(caster, casterPoint);
            // Installed SummonedUnitsController walks an out-of-combat summon
            // back toward its summoner once it is more than 11.7 m away; that
            // native UnitMoveTo interrupted every cast while the summoner was
            // parked 12 m off (close26). The summoner stands 3.5 m behind the
            // casting source instead, clear of every standing section.
            JObject summonerSpot = InfernalWallSummonerBeside35(run, casterPoint, point);
            InfernalBoneRemoveEffects35(run.Fixture.Caster, sources, run.Frightened);
            InfernalBoneRemoveEffects35(caster, sources, run.Frightened);
            var target = new TargetWrapper(point);
            bool targetable = data.IsAvailable && data.CanTarget(target);
            var prior = new HashSet<string>(Game.Instance.State.Units.Where(value =>
                ReferenceEquals(value.Blueprint, run.Section)).Select(value => value.UniqueId));
            long feedbackAttempts = NativeCombatLog.Attempts;
            UnitUseAbility command = null;
            UnitEntityData section = null; JObject creation; int matches;
            // An ordinary command that the native command system interrupts
            // before it acts (close21: 36 ms after start, no rule outcome and
            // no feedback) is re-issued, as a player would, at most twice.
            // A rule refusal (feedback published) is never retried.
            var attempts = new JArray();
            using (var observation = new InfernalWallCreationObservation35(caster, run.Section))
            {
                for (int attempt = 1; attempt <= 3 && section == null; attempt++)
                {
                    if (attempt > 1)
                    {
                        foreach (int frame in InfernalBoneWaitGame35(1.0, 5)) yield return frame;
                        caster.Commands.InterruptAll(true);
                        caster.Commands.RemoveFinishedAndUpdateQueue();
                        if (Vector3.Distance(caster.Position, casterPoint) > .05f) PlaceExpandedSummoningUnit(caster, casterPoint);
                        InfernalWallSummonerBeside35(run, casterPoint, point);
                        InfernalBoneRemoveEffects35(caster, sources, run.Frightened);
                        InfernalBoneRemoveEffects35(run.Fixture.Caster, sources, run.Frightened);
                    }
                    long attemptFeedback = NativeCombatLog.Attempts;
                    command = new UnitUseAbility(data, target);
                    caster.Commands.Run(command);
                    DateTime deadline = DateTime.UtcNow.AddSeconds(25);
                    int framesAfterFinish = 0;
                    while (DateTime.UtcNow < deadline && section == null && framesAfterFinish < 30)
                    {
                        Game.Instance.IsPaused = false; yield return 0;
                        if (command.IsFinished) framesAfterFinish++;
                        section = Game.Instance.State.Units.FirstOrDefault(value =>
                            ReferenceEquals(value.Blueprint, run.Section) && !prior.Contains(value.UniqueId));
                    }
                    var attemptRow = new JObject { ["attempt"] = attempt, ["command"] = Sprint27AiCommand(command),
                        ["sectionFormed"] = section != null, ["feedbackAttemptsDelta"] = NativeCombatLog.Attempts - attemptFeedback,
                        ["casterPosition"] = SurveyVector(caster.Position),
                        ["casterFrightened"] = caster.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Frightened),
                        ["playerInCombat"] = Game.Instance.Player.IsInCombat };
                    attempts.Add(attemptRow);
                    bool interruptedBeforeActing = command.Result == UnitCommand.ResultType.Interrupt && !command.IsActed &&
                        NativeCombatLog.Attempts == attemptFeedback;
                    if (section != null || !interruptedBeforeActing) break;
                }
                creation = observation.Receipt; matches = observation.Matches;
            }
            if (section != null) run.Owned.Add(section);
            formed.Section = section;
            formed.Binding = section?.View == null ? null : section.View.GetComponent<InfernalWallNavmesh35>();
            formed.Cut = formed.Binding?.Cut;
            formed.CutInstance = formed.Cut == null ? 0 : formed.Cut.GetInstanceID();
            var binding = formed.Binding; var cut = formed.Cut;
            foreach (int frame in InfernalWallRegionSettle35(region, () => binding == null || cut == null ||
                (binding.GridApplyCompleted && cut.gridGuoApplied))) yield return frame;
            if (cut != null) formed.CutBounds = cut.GetBounds();
            Buff lifetime = section?.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            result(new JObject { ["label"] = formed.Label, ["source"] = caster.UniqueId,
                ["command"] = Sprint27AiCommand(command), ["targetable"] = targetable,
                ["castingPoint"] = SurveyVector(caster.Position), ["targetPoint"] = SurveyVector(point),
                ["section"] = section?.UniqueId, ["sectionPosition"] = section == null ? null : SurveyVector(section.Position),
                ["sectionForward"] = section == null ? null : SurveyVector(section.OrientationDirection),
                ["summonerOfSection"] = section?.Get<UnitPartSummonedMonster>()?.Summoner?.UniqueId,
                ["creation"] = creation, ["creationMatches"] = matches,
                ["formed"] = targetable && command.Result == UnitCommand.ResultType.Success && section != null &&
                    matches == 1 && binding != null && cut != null && binding.GridApplyCompleted && binding.GridFailure == null,
                ["lifetimeSeconds"] = lifetime?.TimeLeft.TotalSeconds,
                ["cutInstance"] = formed.CutInstance, ["cutRegistered"] = cut != null && Pathfinding.NavmeshCut.GetAll().Contains(cut),
                ["cutGridBlockApplied"] = cut != null && cut.gridGuoApplied,
                ["cutBounds"] = cut == null ? null : new JObject { ["center"] = SurveyVector(formed.CutBounds.center),
                    ["size"] = SurveyVector(formed.CutBounds.size) },
                ["gridGraphIndex"] = binding?.GridGraphIndex, ["gridNodesClaimed"] = binding?.GridNodesClaimed,
                ["gridNodesChanged"] = binding?.GridNodesChanged, ["gridFailure"] = binding?.GridFailure,
                ["feedbackAttemptsDelta"] = NativeCombatLog.Attempts - feedbackAttempts,
                ["commandAttempts"] = attempts, ["summonerSpot"] = summonerSpot,
                ["feedbackMessage"] = NativeCombatLog.Attempts != feedbackAttempts ? NativeCombatLog.LastMessage : null });
        }

        private IEnumerable<int> InfernalWallSharedRetire35(InfernalWallSharedSection35 formed,
            ICollection<Pathfinding.GraphNode> region, Action<JObject> result)
        {
            var section = formed.Section; var binding = formed.Binding;
            Buff lifetime = section.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            if (lifetime == null) throw new InvalidOperationException("Two-Wall retirement lacks the original native lifetime.");
            long deadline = lifetime.EndTime.Ticks;
            JObject expiry = InfernalWallOriginalDeadlineExpiry35(section, lifetime);
            foreach (int frame in InfernalWallRegionSettle35(region, () =>
                (section.Destroyed || !section.IsInState || section.Descriptor.State.IsDead) &&
                binding.GridRestoreCompleted && !region.Any(node => InfernalWallNavmesh35.ClaimOwnedBy(node, binding))))
                yield return frame;
            result(new JObject { ["section"] = section.UniqueId, ["route"] = "controlled original-deadline expiry",
                ["originalDeadlineTicks"] = deadline, ["expiry"] = expiry,
                ["sectionRetired"] = section.Destroyed || !section.IsInState || section.Descriptor.State.IsDead,
                ["cutGridBlockAppliedAtRelease"] = binding.CutGridBlockAppliedAtRelease,
                ["cutGridRevertQueued"] = binding.CutGridRevertQueued,
                ["gridRestoreCompleted"] = binding.GridRestoreCompleted, ["gridNodesRestored"] = binding.GridNodesRestored,
                ["gridFailure"] = binding.GridFailure,
                ["cutRegistered"] = Pathfinding.NavmeshCut.GetAll().Any(value => value != null &&
                    value.GetInstanceID() == formed.CutInstance) });
        }

        // Geometry search for Wall B. Predictions use the installed rules
        // (claim footprint, native cut bounding box); the ordinary target
        // check on the actual second source is the only legality verdict.
        private IEnumerable<int> InfernalWallSharedSurvey35(InfernalWallRun35 run, InfernalWallSharedSection35 a,
            InfernalWallFixturePlan35 planA, Pathfinding.GridGraph grid, int gridIndex, UnitEntityData sourceB,
            AbilityData dataB, Action<InfernalWallSharedGeometry35> result)
        {
            Vector3 centerA = a.Section.Position;
            Vector3 forwardA = a.Section.OrientationDirection; forwardA.y = 0f; forwardA.Normalize();
            Vector3 alongA = Vector3.Cross(Vector3.up, forwardA).normalized;
            var aClaimed = a.Claimed;
            var aAffected = a.Affected();
            float length = InfernalRules35.WallOfIceSectionLengthMeters, thickness = InfernalRules35.WallOfIceSectionThicknessMeters;
            float nodeSize = grid.nodeSize;
            var units = Game.Instance.State.Units.All.Where(value => value != null && !value.Destroyed && value.IsInState &&
                !value.Descriptor.State.IsDead && !ReferenceEquals(value, sourceB)).ToArray();
            int commonTags = -1;
            foreach (var actor in new[] { run.Fixture.Caster, sourceB })
            {
                var agent = actor.View.AgentASP;
                var construct = agent?.GetType().GetMethod("ConstructTags", Cleanup2Fields);
                if (construct != null && construct.ReturnType == typeof(int) && construct.GetParameters().Length == 0)
                    commonTags &= (int)construct.Invoke(agent, null);
            }
            var constraint = new Pathfinding.PathNNConstraint { graphMask = 1, constrainWalkability = true,
                walkable = true, constrainArea = false, constrainTags = true, tags = commonTags,
                constrainDistance = true, distanceXZ = false, distanceXZkeepY = false };
            var anchor = AstarPath.active.GetNearest(planA.Start, constraint);
            if (anchor.node == null) throw new InvalidOperationException("Two-Wall survey has no native movement anchor.");
            constraint.SetStart(anchor.node); constraint.constrainArea = true;
            var movement = AstarPath.active.graphs[anchor.node.GraphIndex] as Pathfinding.IRaycastableGraph;
            Func<Vector3, Vector3?> walk = requested => {
                var near = AstarPath.active.GetNearest(requested, constraint);
                Vector3 supported; JObject ground;
                if (near.node == null || !near.node.Walkable || !constraint.Suitable(near.node) ||
                    near.node.Area != anchor.node.Area || Vector3.Distance(near.clampedPosition, requested) > .6f ||
                    !Cleanup2CreatureNativeGroundRoot(run.Fixture.Caster, near.clampedPosition, out supported, out ground))
                    return null;
                return near.clampedPosition;
            };
            Func<Vector3, Vector3, int[]> predict = (center, forward) => {
                Vector3 along = Vector3.Cross(Vector3.up, forward).normalized;
                var corners = new[] { center + along * length * .5f + forward * thickness * .5f,
                    center + along * length * .5f - forward * thickness * .5f,
                    center - along * length * .5f + forward * thickness * .5f,
                    center - along * length * .5f - forward * thickness * .5f };
                float minX = corners.Min(value => value.x), maxX = corners.Max(value => value.x);
                float minZ = corners.Min(value => value.z), maxZ = corners.Max(value => value.z);
                Func<Pathfinding.GraphNode, bool> inside = node => {
                    Vector3 position = (Vector3)node.position;
                    return position.x >= minX && position.x <= maxX && position.z >= minZ && position.z <= maxZ; };
                return new[] { aAffected.Count(inside), aClaimed.Count(inside) };
            };
            Func<Vector3, Vector3, bool> footprintClear = (center, forward) => {
                Vector3 along = Vector3.Cross(Vector3.up, forward).normalized;
                List<Pathfinding.GraphNode> nodes = grid.GetNodesInArea(new Bounds(center,
                    new Vector3(length + nodeSize * 2f, 6f, length + nodeSize * 2f)));
                int count = 0;
                try
                {
                    foreach (var node in nodes)
                    {
                        if (node == null || node.GraphIndex != (uint)gridIndex) continue;
                        Vector3 delta = (Vector3)node.position - center;
                        if (!InfernalRules35.WallNodeIntersects(Vector3.Dot(delta, along), Vector3.Dot(delta, forward),
                                length * .5f, thickness * .5f, nodeSize)) continue;
                        if (!node.Walkable || InfernalWallNavmesh35.ClaimOwnerCount(node) > 0) return false;
                        count++;
                    }
                }
                finally { Pathfinding.Util.ListPool<Pathfinding.GraphNode>.Release(nodes); }
                if (count == 0) return false;
                foreach (var unit in units)
                    if (InfernalRules35.WallPlacementOccupied(InfernalWallPlacementRuntime35.DistanceToFootprint(
                            unit.Position, center, along, forward), Math.Max(.05f, unit.Corpulence))) return false;
                return true;
            };
            Vector3[] compass = {
                new Vector3(1f, 0f, 0f), new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, -1f),
                new Vector3(.7071f, 0f, .7071f), new Vector3(-.7071f, 0f, .7071f),
                new Vector3(.7071f, 0f, -.7071f), new Vector3(-.7071f, 0f, -.7071f) };
            var lattice = new List<KeyValuePair<float[], Vector3[]>>();
            var candidates = new List<KeyValuePair<float[], Vector3[]>>();
            var rejected = new JObject();
            Action<string> reject = reason => rejected[reason] = ((int?)rejected[reason] ?? 0) + 1;
            foreach (float angle in new[] { 45f, -45f, 0f, 90f })
            {
                Vector3 forward = Quaternion.Euler(0f, angle, 0f) * forwardA; forward.y = 0f; forward.Normalize();
                Vector3 along = Vector3.Cross(Vector3.up, forward).normalized;
                for (float offsetAlong = -4.5f; offsetAlong <= 4.51f; offsetAlong += .25f)
                for (float offsetForward = -3.5f; offsetForward <= 3.51f; offsetForward += .25f)
                {
                    Vector3 center = centerA + alongA * offsetAlong + forwardA * offsetForward;
                    int[] predicted = predict(center, forward);
                    bool diagonal = Math.Abs(Math.Abs(angle) - 45f) < 1f;
                    if (diagonal && predicted[0] == 0) { reject("diagonal-no-predicted-share"); continue; }
                    float gap = Vector3.Distance(center, centerA);
                    if (!diagonal && gap > 5.5f) { reject("independent-too-far"); continue; }
                    if (!footprintClear(center, forward)) { reject("footprint-not-clear"); continue; }
                    if (InfernalWallPlacementRuntime35.DistanceToFootprint(planA.Start, center, along, forward) < 1.2f ||
                        InfernalWallPlacementRuntime35.DistanceToFootprint(planA.Destination, center, along, forward) < 1.2f ||
                        InfernalWallPlacementRuntime35.DistanceToFootprint(planA.CastingPoint, center, along, forward) < 1.5f ||
                        InfernalWallSegmentToFootprint35(planA.Start, planA.Destination, center, along, forward) < .3f)
                    { reject("lane-a-conflict"); continue; }
                    lattice.Add(new KeyValuePair<float[], Vector3[]>(new[] { angle, predicted[0], predicted[1], gap },
                        new[] { center, forward }));
                }
            }
            // Caster and crossing-route searches run only for the best-ranked
            // clear footprints, so the survey stays bounded.
            int evaluated = 0;
            foreach (var entry in lattice.OrderByDescending(value => value.Key[1]).ThenByDescending(value => value.Key[2])
                .ThenBy(value => value.Key[3]))
            {
                int haveSharing = candidates.Count(value => value.Key[1] > 0);
                int haveOther = candidates.Count - haveSharing;
                if ((haveSharing >= 6 && haveOther >= 3) || evaluated >= 400) break;
                if (entry.Key[1] > 0 ? haveSharing >= 6 : haveOther >= 3) continue;
                evaluated++;
                {
                    float angle = entry.Key[0], gap = entry.Key[3];
                    int[] predicted = { (int)entry.Key[1], (int)entry.Key[2] };
                    Vector3 center = entry.Value[0], forward = entry.Value[1];
                    Vector3 along = Vector3.Cross(Vector3.up, forward).normalized;
                    Vector3? caster = null;
                    foreach (float sign in new[] { 1f, -1f })
                    foreach (float reach in new[] { 4f, 3f, 5f })
                    {
                        if (caster != null) break;
                        Vector3? point = walk(center - forward * sign * reach);
                        if (point == null) continue;
                        Vector3 value = point.Value;
                        if (InfernalWallPlacementRuntime35.DistanceToFootprint(value, center, along, forward) < 1.2f ||
                            InfernalWallPlacementRuntime35.DistanceToFootprint(value, centerA, alongA, forwardA) < 1.5f ||
                            Vector3.Distance(value, planA.CastingPoint) < 1.5f ||
                            InfernalEarlyDistanceToSegment35(value, planA.Start, planA.Destination) < 1.5f ||
                            // Wall A stands between a far-side caster and its
                            // target: the cast's own execution refuses it
                            // (close13), though CanTarget accepted the point.
                            InfernalWallSegmentToFootprint35(value, center, centerA, alongA, forwardA) < .3f ||
                            Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(value + Vector3.up * 1.2f,
                                center + Vector3.up * 1.2f, 0)) continue;
                        caster = value;
                    }
                    if (caster == null) { reject("no-caster-point"); continue; }
                    Vector3[] route = null;
                    foreach (Vector3 direction in compass.OrderByDescending(value => Math.Abs(Vector3.Dot(value, forward))))
                    {
                        if (route != null || Math.Abs(Vector3.Dot(direction, forward)) < .6f) continue;
                        foreach (float shift in new[] { 0f, .75f, -.75f, 1.25f, -1.25f })
                        {
                            Vector3 cross = center + along * shift;
                            Vector3? from = walk(cross - direction * 4f), to = walk(cross + direction * 4f);
                            if (from == null || to == null) continue;
                            Pathfinding.GraphHitInfo hit;
                            if (movement == null || movement.Linecast(from.Value, to.Value, null, out hit, new List<Pathfinding.GraphNode>()))
                                continue;
                            if (InfernalWallSegmentToFootprint35(from.Value, to.Value, centerA, alongA, forwardA) < .9f ||
                                InfernalWallPlacementRuntime35.DistanceToFootprint(from.Value, center, along, forward) < 1.5f ||
                                InfernalWallPlacementRuntime35.DistanceToFootprint(to.Value, center, along, forward) < 1.5f ||
                                InfernalEarlyDistanceToSegment35(caster.Value, from.Value, to.Value) < 1.5f ||
                                InfernalEarlyDistanceToSegment35(planA.CastingPoint, from.Value, to.Value) < 1.5f ||
                                Vector3.Distance(from.Value, planA.Start) < 1f || Vector3.Distance(from.Value, planA.Destination) < 1f ||
                                Vector3.Distance(to.Value, planA.Start) < 1f || Vector3.Distance(to.Value, planA.Destination) < 1f)
                                continue;
                            route = new[] { from.Value, to.Value };
                            break;
                        }
                    }
                    if (route == null) { reject("no-crossing-route"); continue; }
                    candidates.Add(new KeyValuePair<float[], Vector3[]>(
                        new[] { angle, predicted[0], predicted[1], gap },
                        new[] { center, forward, caster.Value, route[0], route[1] }));
                }
            }
            var ordered = candidates;
            var probes = new JArray();
            var survey = new JObject { ["wallA"] = new JObject { ["center"] = SurveyVector(centerA),
                    ["forward"] = SurveyVector(forwardA), ["claimed"] = aClaimed.Count, ["affected"] = aAffected.Count,
                    ["cutBounds"] = new JObject { ["center"] = SurveyVector(a.CutBounds.center), ["size"] = SurveyVector(a.CutBounds.size) } },
                ["clearFootprintCandidates"] = lattice.Count,
                ["clearFootprintSharingCandidates"] = lattice.Count(value => value.Key[1] > 0),
                ["evaluatedForCasterAndRoute"] = evaluated,
                ["viableCandidates"] = candidates.Count,
                ["viableSharingCandidates"] = candidates.Count(value => value.Key[1] > 0),
                ["rejections"] = rejected, ["probes"] = probes,
                ["method"] = "Wall B centres on a 0.25 m lattice within 4.5 m of Wall A, at 45, -45, 0 and 90 degrees to it; predicted claim footprint and native cut bounding box from installed rules; the second source's ordinary target check decides legality." };
            InfernalWallSharedGeometry35 chosen = null;
            foreach (var candidate in ordered)
            {
                Vector3 center = candidate.Value[0], caster = candidate.Value[2];
                PlaceExpandedSummoningUnit(sourceB, caster);
                yield return 0;
                var target = new TargetWrapper(center);
                InfernalWallPlacement35 placement;
                bool resolved = InfernalWallPlacementRuntime35.TryResolve(sourceB, target, run.Section, out placement);
                bool targetable = dataB.IsAvailable && dataB.CanTarget(target);
                int[] actual = resolved ? predict(placement.Center, placement.Forward) : new[] { 0, 0 };
                probes.Add(new JObject { ["angle"] = candidate.Key[0], ["center"] = SurveyVector(center),
                    ["caster"] = SurveyVector(sourceB.Position), ["predictedShared"] = candidate.Key[1],
                    ["predictedSharedClaimed"] = candidate.Key[2], ["targetable"] = targetable, ["resolved"] = resolved,
                    ["resolvedCenter"] = resolved ? SurveyVector(placement.Center) : null,
                    ["resolvedForward"] = resolved ? SurveyVector(placement.Forward) : null,
                    ["resolvedPredictedShared"] = actual[0] });
                // A creature adjacent to the forming wall may save to disrupt
                // it (close19: the mover stood adjacent). Only Wall B's own
                // source may stand there; the mover is parked at lane A's
                // start for every cast, where it stands during this probe.
                var adjacent = resolved ? InfernalWallPlacementRuntime35.AdjacentUnits(placement)
                    .Where(value => !ReferenceEquals(value, sourceB)).Select(value => value.UniqueId).ToArray() : new string[0];
                ((JObject)probes.Last)["adjacentCreatures"] = new JArray(adjacent);
                if (!targetable || !resolved || adjacent.Length > 0) continue;
                chosen = new InfernalWallSharedGeometry35 { CenterB = center, ForwardB = placement.Forward,
                    CasterB = caster, RouteStartB = candidate.Value[3], RouteEndB = candidate.Value[4],
                    AngleDegrees = candidate.Key[0], PredictedShared = actual[0], PredictedSharedClaimed = actual[1],
                    SharedReachable = actual[0] > 0 };
                break;
            }
            if (chosen == null)
                throw new InvalidOperationException("No legal second Wall placement near the first: " + survey.ToString(Formatting.None));
            survey["chosen"] = new JObject { ["angleDegrees"] = chosen.AngleDegrees, ["center"] = SurveyVector(chosen.CenterB),
                ["forward"] = SurveyVector(chosen.ForwardB), ["caster"] = SurveyVector(chosen.CasterB),
                ["routeStart"] = SurveyVector(chosen.RouteStartB), ["routeEnd"] = SurveyVector(chosen.RouteEndB),
                ["predictedShared"] = chosen.PredictedShared, ["predictedSharedClaimed"] = chosen.PredictedSharedClaimed,
                ["sharedReachable"] = chosen.SharedReachable };
            chosen.Survey = survey;
            result(chosen);
        }
    }
}
