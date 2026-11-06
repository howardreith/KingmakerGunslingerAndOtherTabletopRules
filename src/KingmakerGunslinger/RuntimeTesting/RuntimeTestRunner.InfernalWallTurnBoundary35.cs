using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using Pathfinding;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Turn-based boundary of one standing Wall section, through the
        // installed turn-based consumers only. In turn-based combat the
        // installed UnitMovementAgent routes on grid graph mask 2 with
        // ABPathWithClearance (clearance Corpulence * 2, native ConstructTags);
        // that path's CanTraverse admits any node inside the clearance box
        // around the start node on its static flag alone, ignoring
        // DynamicBlocked counters. Native movement along the route is not
        // clamped to the recast navmesh (UnitMovementAgentBase.Move only
        // grounds the step). The probe stands the mover in a claimed ring
        // cell beside the ice, asks the native route to the mirrored far side
        // and to the ring cell itself, and makes one ordinary turn-based move
        // with the native precomputed path (the fear controller's pattern).
        // The observer writes no graph state.
        private sealed class InfernalWallTurnProbe35
        {
            internal GraphNode Cell;
            internal Vector3 Start, Destination, Origin;
            internal int Side;
            internal float Along, Across;
            internal bool Standable;
        }

        private IEnumerable<int> InfernalWallTurnBoundary35(InfernalWallRun35 run, UnitEntityData sourceA,
            UnitEntityData sourceB, AbilityData dataA, InfernalWallFixturePlan35 plan, UnitEntityData[] sources,
            List<GraphNode> region, Dictionary<GraphNode, InfernalWallNodeState35> baseline,
            Action<UnitEntityData> park, Action clearFear, Action persist, Action<JObject> result)
        {
            var mover = run.Fixture.Caster;
            var row = new JObject { ["scope"] = "One Wall section from an ordinary command; native turn-based route queries and one native turn-based move; the observer writes no graph state." };
            result(row);
            park(mover); park(sourceB); clearFear();
            // A legal ordinary placement centred on a grid row, cast along a
            // grid axis: the native cut's counter block is then a single row
            // of cells (close27 Wall A; nav1's lane Wall covered two rows).
            // Falls back to the lane placement when that is not legal.
            Vector3 castFrom = plan.CastingPoint, castAt = plan.Midpoint;
            var placementRow = new JObject { ["lanePoint"] = SurveyVector(plan.Midpoint), ["laneCaster"] = SurveyVector(plan.CastingPoint) };
            {
                int laneGridIndex;
                GridGraph laneGrid = InfernalWallClaimGrid35(plan.Midpoint, out laneGridIndex);
                Vector3 axis = plan.Midpoint - plan.CastingPoint; axis.y = 0f; axis.Normalize();
                bool alongX = Math.Abs(axis.x) >= .999f, alongZ = Math.Abs(axis.z) >= .999f;
                placementRow["laneAxisAligned"] = alongX || alongZ;
                NNInfo row0 = laneGrid.GetNearest(plan.Midpoint, NNConstraint.None);
                if ((alongX || alongZ) && row0.node != null)
                {
                    Vector3 snapped = plan.Midpoint;
                    Vector3 nodePosition = (Vector3)row0.node.position;
                    if (alongZ) snapped.z = nodePosition.z; else snapped.x = nodePosition.x;
                    Vector3 shift = snapped - plan.Midpoint; shift.y = 0f;
                    Vector3 casterWanted = plan.CastingPoint + shift;
                    NNInfo casterNode = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(casterWanted);
                    NNInfo targetNode = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(snapped);
                    if (casterNode.node != null && targetNode.node != null &&
                        Vector3.Distance(casterNode.clampedPosition, casterWanted) < .3f &&
                        Vector3.Distance(targetNode.clampedPosition, snapped) < .3f)
                    {
                        Vector3 aligned = targetNode.clampedPosition;
                        Vector3 alignedCaster = casterNode.clampedPosition;
                        Vector3 before = sourceA.Position;
                        PlaceExpandedSummoningUnit(sourceA, alignedCaster);
                        yield return 0;
                        InfernalWallPlacement35 trial;
                        var target = new TargetWrapper(aligned);
                        bool resolved = InfernalWallPlacementRuntime35.TryResolve(sourceA, target, run.Section, out trial);
                        bool targetable = dataA.IsAvailable && dataA.CanTarget(target);
                        placementRow["gridRowTarget"] = SurveyVector(aligned);
                        placementRow["gridRowCaster"] = SurveyVector(alignedCaster);
                        placementRow["gridRowResolved"] = resolved; placementRow["gridRowTargetable"] = targetable;
                        if (resolved && targetable) { castFrom = alignedCaster; castAt = aligned; }
                        else PlaceExpandedSummoningUnit(sourceA, before);
                    }
                }
            }
            placementRow["usedGridRowPlacement"] = castAt != plan.Midpoint;
            row["placement"] = placementRow;
            var wall = new InfernalWallSharedSection35 { Label = "tb-a", Source = sourceA };
            JObject cast = null;
            foreach (int frame in InfernalWallSharedCast35(run, wall, dataA, castFrom, castAt, sources, region,
                value => cast = value)) yield return frame;
            row["cast"] = cast;
            park(sourceA); clearFear();
            if (wall.Section == null || wall.Binding == null)
            {
                row["probeRan"] = false; row["reason"] = "Wall did not form";
                result(row); yield break;
            }
            var formed = InfernalWallSnapshot35(region);
            foreach (var node in region)
            {
                if (formed[node].Dynamic > baseline[node].Dynamic) wall.Dynamic[node] = formed[node].Dynamic - baseline[node].Dynamic;
                if (InfernalWallNavmesh35.ClaimOwnedBy(node, wall.Binding)) wall.Claimed.Add(node);
            }
            Vector3 centre = wall.Section.Position;
            Vector3 forward = wall.Section.OrientationDirection; forward.y = 0f; forward.Normalize();
            Vector3 along = Vector3.Cross(Vector3.up, forward).normalized;
            float halfLength = InfernalRules35.WallOfIceSectionLengthMeters * .5f;
            float halfThickness = InfernalRules35.WallOfIceSectionThicknessMeters * .5f;
            int gridIndex;
            GridGraph grid = InfernalWallClaimGrid35(centre, out gridIndex);
            float nodeSize = grid.nodeSize;
            Func<Vector3, float> alongOf = point => Vector3.Dot(point - centre, along);
            Func<Vector3, float> acrossOf = point => Vector3.Dot(point - centre, forward);
            Func<Vector3, float> gap = point => InfernalWallPlacementRuntime35.DistanceToFootprint(point, centre, along, forward);
            Func<GraphNode, bool> inIce = node => gap((Vector3)node.position) <= 0f;
            var ring = wall.Claimed.Where(node => !inIce(node)).ToArray();
            row["cells"] = new JObject { ["claimed"] = wall.Claimed.Count, ["iceCentre"] = wall.Claimed.Count(inIce),
                ["counterRowsAcross"] = wall.Dynamic.Keys.Select(node => Math.Round(acrossOf((Vector3)node.position) / nodeSize))
                    .Distinct().Count(),
                ["iceCentreRowsAcross"] = wall.Claimed.Where(inIce).Select(node => Math.Round(acrossOf((Vector3)node.position) / nodeSize))
                    .Distinct().Count(),
                ["dynamicNodes"] = wall.Dynamic.Count,
                ["claimedRingWalkableAfterFormation"] = ring.Count(node => formed[node].Walkable),
                ["claimedRingStaticWalkableAfterFormation"] = ring.Count(node => formed[node].Static),
                ["nodes"] = new JArray(wall.Claimed.Concat(wall.Dynamic.Keys).Distinct().Select(node => {
                    var cell = InfernalWallNodeIdentity35(node);
                    cell["along"] = alongOf((Vector3)node.position); cell["across"] = acrossOf((Vector3)node.position);
                    cell["insideIce"] = inIce(node); cell["claimed"] = wall.Claimed.Contains(node);
                    cell["dynamicFromWall"] = wall.Dynamic.ContainsKey(node) ? wall.Dynamic[node] : 0;
                    cell["baseline"] = InfernalWallNodeDocument35(baseline[node]);
                    cell["formed"] = InfernalWallNodeDocument35(formed[node]);
                    return cell; })) };
            persist();

            // Ring cells next to the ice, inside the section's length, one or
            // two per side: the cells native updates reopened (close27).
            var probes = new List<InfernalWallTurnProbe35>();
            foreach (int side in new[] { -1, 1 })
            {
                var cells = ring.Where(node => Math.Sign(acrossOf((Vector3)node.position)) == side &&
                        Math.Abs(acrossOf((Vector3)node.position)) <= halfThickness + nodeSize &&
                        Math.Abs(alongOf((Vector3)node.position)) <= halfLength - nodeSize * .5f)
                    .OrderBy(node => Math.Abs(alongOf((Vector3)node.position))).ToList();
                var picked = new List<GraphNode>();
                foreach (var node in cells)
                {
                    if (picked.Count >= 2) break;
                    if (picked.Any(other => Math.Abs(alongOf((Vector3)other.position) - alongOf((Vector3)node.position)) < .9f)) continue;
                    picked.Add(node);
                }
                foreach (var node in picked)
                {
                    Vector3 position = (Vector3)node.position;
                    NNInfo recast = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(position);
                    Vector2 flat = new Vector2(position.x, position.z);
                    bool standable = recast.node != null && (new Vector2(recast.clampedPosition.x, recast.clampedPosition.z) - flat).magnitude < .05f;
                    float a = alongOf(position);
                    Vector3 far = centre + along * a + forward * (-side * (halfThickness + 1.25f));
                    Vector3 origin = centre + along * a + forward * (side * (halfThickness + 2.5f));
                    NNInfo farNode = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(far);
                    NNInfo originNode = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(origin);
                    probes.Add(new InfernalWallTurnProbe35 { Cell = node, Start = recast.node != null ? recast.clampedPosition : position,
                        Destination = farNode.node != null ? farNode.clampedPosition : far,
                        Origin = originNode.node != null ? originNode.clampedPosition : origin,
                        Side = side, Along = a, Across = acrossOf(position), Standable = standable });
                }
            }
            row["probeCells"] = new JArray(probes.Select(probe => new JObject { ["cell"] = InfernalWallNodeIdentity35(probe.Cell),
                ["side"] = probe.Side, ["along"] = probe.Along, ["across"] = probe.Across,
                ["iceGap"] = gap((Vector3)probe.Cell.position), ["recastStandable"] = probe.Standable,
                ["start"] = SurveyVector(probe.Start), ["farDestination"] = SurveyVector(probe.Destination),
                ["sameSideOrigin"] = SurveyVector(probe.Origin) }));
            persist();
            probes = probes.Where(probe => probe.Standable).ToList();
            if (probes.Count == 0)
            {
                row["probeRan"] = false; row["reason"] = "no recast-standable claimed ring cell beside the ice";
            }

            var controller = Game.Instance.TurnBasedCombatController;
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldScroll = SettingsRoot.Instance.CameraScrollToCurrentUnit.CurrentValue;
            bool moverWasInCombat = mover.IsInCombat;
            var turnLog = new JArray();
            var routes = new JArray();
            JObject actual = null;
            var statusLog = new JArray();
            row["entry"] = new JObject { ["playerInCombat"] = Game.Instance.Player.IsInCombat, ["moverInCombat"] = moverWasInCombat,
                ["turnBasedSettingBefore"] = oldMode, ["cameraScrollBefore"] = oldScroll,
                ["moverAvoidanceDisabled"] = mover.View.AgentASP.AvoidanceDisabled, ["moverCorpulence"] = mover.Corpulence };
            if (probes.Count > 0)
            try
            {
                // Camera scrolling would hold each turn in Scrolling; the
                // native turn then starts in Prepare directly.
                SettingsRoot.Instance.CameraScrollToCurrentUnit.CurrentValue = false;
                InfernalWallPrepareActor35(mover);
                PlaceExpandedSummoningUnit(mover, probes[0].Start);
                if (!mover.IsInCombat) { mover.JoinCombat(); Game.Instance.Player.UpdateIsInCombat(); }
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = true;
                controller.Activate();
                DateTime entryDeadline = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < entryDeadline && !CombatController.IsInTurnBasedCombat())
                { Game.Instance.IsPaused = false; yield return 0; }
                row["turnBasedEntered"] = CombatController.IsInTurnBasedCombat();
                row["moverEnrolled"] = controller.SortedUnits != null && controller.SortedUnits.Contains(mover);
                // Reach the mover's own turn; every other unit's turn is ended
                // natively (as its end-turn control does). A directly
                // controllable unit's turn stays Preparing until it is given a
                // command, which moves it to Acting (installed TurnController
                // .Tick), so the mover's turn is current in either status
                // (nav1 waited for Acting and never issued the move).
                DateTime turnDeadline = DateTime.UtcNow.AddSeconds(40);
                bool moverActing = false;
                string lastStatus = null;
                while (CombatController.IsInTurnBasedCombat() && DateTime.UtcNow < turnDeadline)
                {
                    Game.Instance.IsPaused = false; yield return 0;
                    TurnController turn = controller.CurrentTurn;
                    string status = turn == null ? "none" : turn.Unit.UniqueId + ":" + turn.Status;
                    if (status != lastStatus && statusLog.Count < 80)
                        statusLog.Add(new JObject { ["turn"] = status, ["waitingForUi"] = (bool)controller.WaitingForUI,
                            ["round"] = controller.RoundNumber, ["moverIsCurrent"] = turn != null && ReferenceEquals(turn.Unit, mover) });
                    lastStatus = status;
                    if (turn == null) continue;
                    if (ReferenceEquals(turn.Unit, mover))
                    {
                        if (turn.Status == TurnController.TurnStatus.Preparing ||
                            turn.Status == TurnController.TurnStatus.Acting) { moverActing = true; break; }
                        continue;
                    }
                    if (turn.Status == TurnController.TurnStatus.Preparing || turn.Status == TurnController.TurnStatus.Acting)
                    {
                        if (turnLog.Count < 40)
                            turnLog.Add(new JObject { ["unit"] = turn.Unit.UniqueId, ["blueprint"] = turn.Unit.Blueprint?.name,
                                ["status"] = turn.Status.ToString(), ["round"] = controller.RoundNumber });
                        turn.ForceToEnd(false);
                    }
                }
                row["moverTurnReached"] = moverActing;
                row["foreignTurnsEnded"] = turnLog;
                row["turnStatusLog"] = statusLog;
                // Native grid state once turn-based navigation has updated.
                foreach (int frame in InfernalWallRegionSettle35(region, () => true)) yield return frame;
                var turnState = InfernalWallSnapshot35(region);
                row["cellsInTurnBased"] = new JObject {
                    ["claimedRingWalkable"] = ring.Count(node => turnState[node].Walkable),
                    ["claimedRingStaticWalkable"] = ring.Count(node => turnState[node].Static),
                    ["iceCentreBlocked"] = wall.Claimed.Where(inIce).Count(node => !turnState[node].Walkable),
                    ["iceCentreStaticWalkable"] = wall.Claimed.Where(inIce).Count(node => turnState[node].Static),
                    ["iceCentre"] = wall.Claimed.Count(inIce),
                    ["cutGridBlockApplied"] = wall.Cut != null && wall.Cut.gridGuoApplied };
                persist();

                var agent = mover.View.MovementAgent as UnitMovementAgent;
                foreach (var probe in probes)
                {
                    PlaceExpandedSummoningUnit(mover, probe.Start);
                    for (int frame = 0; frame < 3; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                    var routeRow = new JObject { ["cell"] = InfernalWallNodeIdentity35(probe.Cell), ["side"] = probe.Side,
                        ["along"] = probe.Along, ["moverPosition"] = SurveyVector(mover.Position) };
                    Path through;
                    routeRow["toFarSide"] = InfernalWallTurnRoute35(agent, probe.Destination, null, wall, centre, along, forward,
                        halfLength, out through);
                    PlaceExpandedSummoningUnit(mover, probe.Origin);
                    for (int frame = 0; frame < 3; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                    Path toCell;
                    var destinationRow = InfernalWallTurnRoute35(agent, probe.Start, null, wall, centre, along, forward,
                        halfLength, out toCell);
                    var end = toCell == null || toCell.vectorPath == null || toCell.vectorPath.Count == 0 ? (Vector3?)null :
                        toCell.vectorPath[toCell.vectorPath.Count - 1];
                    destinationRow["endIceGap"] = end == null ? (float?)null : gap(end.Value);
                    destinationRow["endIsRingCell"] = toCell != null && toCell.path != null && toCell.path.Count > 0 &&
                        ReferenceEquals(toCell.path[toCell.path.Count - 1], probe.Cell);
                    routeRow["toRingCellFromSameSide"] = destinationRow;
                    routes.Add(routeRow);
                }
                row["routes"] = routes;
                persist();

                if (moverActing && agent != null)
                {
                    var chosen = probes.FirstOrDefault(probe => routes.OfType<JObject>().Any(value =>
                        (int)value["side"] == probe.Side && Math.Abs((float)value["along"] - probe.Along) < .01f &&
                        (bool?)value["toFarSide"]["throughIce"] == true)) ?? probes[0];
                    PlaceExpandedSummoningUnit(mover, chosen.Start);
                    for (int frame = 0; frame < 3; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                    var move = new UnitMoveTo(chosen.Destination, .3f);
                    Path forced = agent.FindPath(move.ApproachPoint, delegate { }, move.ApproachRadius);
                    if (forced != null) AstarPath.WaitForPath(forced);
                    if (forced != null && !forced.error) move.ForcedPath = forced;
                    actual = new JObject { ["cell"] = InfernalWallNodeIdentity35(chosen.Cell), ["side"] = chosen.Side,
                        ["start"] = SurveyVector(mover.Position), ["destination"] = SurveyVector(chosen.Destination),
                        ["forcedPath"] = forced == null ? null : new JArray(forced.vectorPath.Select(SurveyVector)),
                        ["forcedPathError"] = forced == null || forced.error };
                    mover.Commands.Run(move);
                    var samples = new JArray();
                    float minimumGap = float.PositiveInfinity, travelled = 0f;
                    int startSide = Math.Sign(acrossOf(mover.Position)), lastSide = startSide;
                    bool crossed = false;
                    Vector3 previous = mover.Position;
                    DateTime moveDeadline = DateTime.UtcNow.AddSeconds(10);
                    int quiet = 0;
                    while (DateTime.UtcNow < moveDeadline && quiet < 20)
                    {
                        Game.Instance.IsPaused = false; yield return 0;
                        Vector3 position = mover.Position;
                        travelled += Vector3.Distance(previous, position); previous = position;
                        minimumGap = Math.Min(minimumGap, gap(position));
                        int sideNow = Math.Sign(acrossOf(position));
                        if (sideNow != 0 && sideNow != lastSide && Math.Abs(alongOf(position)) < halfLength) crossed = true;
                        if (sideNow != 0) lastSide = sideNow;
                        if (samples.Count < 200) samples.Add(SurveyVector(position));
                        quiet = move.IsFinished && !agent.IsReallyMoving ? quiet + 1 : 0;
                    }
                    TurnController during = controller.CurrentTurn;
                    actual["turnStatusAfterMove"] = during == null ? "none" : during.Status.ToString();
                    actual["moverStillCurrent"] = during != null && ReferenceEquals(during.Unit, mover);
                    actual["result"] = move.Result.ToString();
                    actual["finished"] = move.IsFinished;
                    actual["end"] = SurveyVector(mover.Position);
                    actual["travelled"] = travelled;
                    actual["minimumIceGap"] = float.IsInfinity(minimumGap) ? (float?)null : minimumGap;
                    actual["centreEnteredIce"] = minimumGap <= 0f;
                    actual["crossedInsideLength"] = crossed;
                    actual["endedOnFarSide"] = Math.Sign(acrossOf(mover.Position)) == -startSide;
                    actual["samples"] = samples;
                    mover.Commands.InterruptAll(true);
                    mover.Commands.RemoveFinishedAndUpdateQueue();
                    TurnController own = controller.CurrentTurn;
                    if (own != null && ReferenceEquals(own.Unit, mover)) own.ForceToEnd(false);
                }
                row["actualMove"] = actual;
                persist();
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                controller.Activate();
                SettingsRoot.Instance.CameraScrollToCurrentUnit.CurrentValue = oldScroll;
            }
            DateTime exitDeadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < exitDeadline && CombatController.IsInTurnBasedCombat() != oldMode)
            { Game.Instance.IsPaused = false; yield return 0; }
            InfernalWallPrepareActor35(mover);
            if (!moverWasInCombat && mover.IsInCombat) { mover.LeaveCombat(); Game.Instance.Player.UpdateIsInCombat(); }
            foreach (int frame in InfernalWallRegionSettle35(region, () => true)) yield return frame;
            row["exit"] = new JObject { ["turnBasedSettingRestored"] = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue == oldMode,
                ["cameraScrollRestored"] = SettingsRoot.Instance.CameraScrollToCurrentUnit.CurrentValue == oldScroll,
                ["inTurnBasedCombat"] = CombatController.IsInTurnBasedCombat(), ["moverInCombat"] = mover.IsInCombat,
                ["moverInCombatRestored"] = mover.IsInCombat == moverWasInCombat };
            park(mover);
            JObject retirement = null;
            foreach (int frame in InfernalWallSharedRetire35(wall, region, value => retirement = value)) yield return frame;
            row["retirement"] = retirement;
            var after = InfernalWallSnapshot35(region);
            var flagsChanged = region.Where(node => !after[node].SameNavigation(baseline[node])).ToArray();
            var connectionsChanged = region.Where(node => after[node].Connections != baseline[node].Connections).ToArray();
            row["restoration"] = new JObject { ["regionNodes"] = region.Count,
                ["nodesNotAtBaseline"] = flagsChanged.Length,
                ["nodesWithConnectionsNotAtBaseline"] = connectionsChanged.Length,
                ["nodesWithTagNotAtBaseline"] = region.Count(node => after[node].Tag != baseline[node].Tag),
                ["mismatches"] = new JArray(flagsChanged.Concat(connectionsChanged).Distinct().Take(40).Select(node => {
                    var cell = InfernalWallNodeIdentity35(node);
                    cell["baseline"] = InfernalWallNodeDocument35(baseline[node]);
                    cell["after"] = InfernalWallNodeDocument35(after[node]);
                    return cell; })),
                ["claimsAfter"] = InfernalWallNavmesh35.ActiveGridClaimCount };
            row["probeRan"] = row["probeRan"] ?? true;
            persist();
            result(row);
        }

        // Selected contract: claims hold both static layers, so turn-based
        // entry and every native unit block leave the claimed cells blocked;
        // no native turn-based route or ordinary turn-based move takes the
        // mover's centre into or through the ice; leaving turn-based mode and
        // retiring the Wall restore the region exactly.
        private static void InfernalWallTurnBoundaryVerdict35(JObject row, Action<string, bool, JObject, string> check)
        {
            var cells = row?["cellsInTurnBased"] as JObject;
            var routes = row?["routes"] as JArray;
            var actual = row?["actualMove"] as JObject;
            var restoration = row?["restoration"] as JObject;
            var exit = row?["exit"] as JObject;
            bool claimsHeld = cells != null && (int?)cells["claimedRingStaticWalkable"] == 0 &&
                (int?)cells["iceCentreStaticWalkable"] == 0 && (int?)cells["claimedRingWalkable"] == 0 &&
                (int?)cells["iceCentreBlocked"] == (int?)cells["iceCentre"];
            bool routesClear = routes != null && routes.Count > 0 && routes.OfType<JObject>().All(value =>
                (bool?)value["toFarSide"]?["error"] == false && (bool?)value["toFarSide"]?["throughIce"] == false &&
                (bool?)value["toRingCellFromSameSide"]?["throughIce"] == false);
            bool moveClear = (bool?)row?["moverTurnReached"] == true && actual != null &&
                (bool?)actual["forcedPathError"] == false && (string)actual["result"] == "Success" &&
                (bool?)actual["centreEnteredIce"] == false && (bool?)actual["crossedInsideLength"] == false;
            check("wall-turn-boundary", (bool?)row?["turnBasedEntered"] == true && claimsHeld && routesClear && moveClear,
                row ?? new JObject(),
                "In native turn-based combat every cell the standing Wall claims stays blocked in both static layers; native turn-based routes from the claimed cells beside the ice, to the far side and back to the ring, and one ordinary native turn-based move along the native route never take the mover's centre into or through the ice.");
            check("wall-turn-boundary-restored", exit != null && (bool?)exit["turnBasedSettingRestored"] == true &&
                (bool?)exit["cameraScrollRestored"] == true && (bool?)exit["inTurnBasedCombat"] == false &&
                restoration != null && (int?)restoration["nodesNotAtBaseline"] == 0 &&
                (int?)restoration["nodesWithConnectionsNotAtBaseline"] == 0 && (int?)restoration["claimsAfter"] == 0,
                restoration ?? new JObject(),
                "Leaving turn-based mode restores its setting and camera preference, and retiring the probe Wall returns every region cell, including grid connections, to its pre-Wall baseline.");
        }

        // One native route request through the mover's own agent; under
        // turn-based combat this is the installed StartPathWithClerance on
        // grid mask 2. Records the path's nodes against the section.
        private static JObject InfernalWallTurnRoute35(UnitMovementAgent agent, Vector3 destination, float? approachRadius,
            InfernalWallSharedSection35 wall, Vector3 centre, Vector3 along, Vector3 forward, float halfLength, out Path path)
        {
            path = null;
            if (agent == null) return new JObject { ["available"] = false, ["reason"] = "no-agent" };
            path = agent.FindPath(destination, delegate { }, approachRadius, false, 0);
            if (path == null) return new JObject { ["available"] = false, ["reason"] = "no-path-request" };
            AstarPath.WaitForPath(path);
            Func<Vector3, float> gap = point => InfernalWallPlacementRuntime35.DistanceToFootprint(point, centre, along, forward);
            var row = new JObject { ["turnBased"] = CombatController.IsInTurnBasedCombat(), ["pathType"] = path.GetType().Name,
                ["error"] = path.error, ["destination"] = SurveyVector(destination) };
            var ab = path as ABPath;
            var clearance = agent.GetType().GetField("m_Seeker", System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)?.GetValue(agent) as Kingmaker.TurnBasedMode.Pathfinding.SeekerWithClearance;
            row["clearance"] = clearance == null ? null : SurveyVector(clearance.clearance);
            row["enabledTags"] = path.enabledTags;
            HashSet<GraphNode> exempt = new HashSet<GraphNode>();
            if (ab != null && ab.startNode != null && clearance != null)
            {
                var grid = AstarPath.active.graphs[ab.startNode.GraphIndex] as GridGraph;
                if (grid != null)
                {
                    List<GraphNode> near = grid.GetNodesInArea(new Bounds((Vector3)ab.startNode.position, clearance.clearance));
                    try { foreach (var node in near) exempt.Add(node); }
                    finally { Pathfinding.Util.ListPool<GraphNode>.Release(near); }
                }
                row["startNode"] = InfernalWallNodeIdentity35(ab.startNode);
                row["startNodeGraph"] = (int)ab.startNode.GraphIndex;
            }
            var nodes = path.path ?? new List<GraphNode>();
            row["nodes"] = new JArray(nodes.Take(80).Select(node => {
                var cell = InfernalWallNodeIdentity35(node);
                cell["insideIce"] = gap((Vector3)node.position) <= 0f;
                cell["claimedByWall"] = InfernalWallNavmesh35.ClaimOwnedBy(node, wall.Binding);
                cell["static"] = node.StaticWalkable; cell["walkable"] = node.Walkable;
                cell["dynamicBlocked"] = node.DynamicBlocked; cell["startClearanceExempt"] = exempt.Contains(node);
                return cell; }));
            var points = path.vectorPath ?? new List<Vector3>();
            row["vector"] = new JArray(points.Select(SurveyVector));
            double length = 0; float minimumGap = float.PositiveInfinity; bool crossed = false;
            int lastSide = points.Count == 0 ? 0 : Math.Sign(Vector3.Dot(points[0] - centre, forward));
            for (int index = 1; index < points.Count; index++)
            {
                length += Vector3.Distance(points[index - 1], points[index]);
                for (int step = 0; step <= 20; step++)
                {
                    Vector3 sample = Vector3.Lerp(points[index - 1], points[index], step / 20f);
                    minimumGap = Math.Min(minimumGap, gap(sample));
                    int side = Math.Sign(Vector3.Dot(sample - centre, forward));
                    if (side != 0 && lastSide != 0 && side != lastSide &&
                        Math.Abs(Vector3.Dot(sample - centre, along)) < halfLength) crossed = true;
                    if (side != 0) lastSide = side;
                }
            }
            bool iceNode = nodes.Any(node => gap((Vector3)node.position) <= 0f);
            row["length"] = length;
            row["minimumIceGap"] = float.IsInfinity(minimumGap) ? (float?)null : minimumGap;
            row["crossesInsideLength"] = crossed;
            row["routeNodeInsideIce"] = iceNode;
            // Through: the route crosses the section inside its length or
            // uses a cell whose centre lies in the ice. Boundary: the route's
            // centre line touches the ice footprint (a corner or face graze)
            // without crossing it.
            row["throughIce"] = !path.error && (crossed || iceNode);
            row["touchesIceBoundary"] = !path.error && !crossed && !iceNode && minimumGap <= .05f;
            row["claimedCellsOnRoute"] = nodes.Count(node => InfernalWallNavmesh35.ClaimOwnedBy(node, wall.Binding));
            row["throughIceViaStartExemption"] = !path.error && nodes.Any(node => gap((Vector3)node.position) <= 0f &&
                exempt.Contains(node) && !node.Walkable);
            return row;
        }
    }
}
