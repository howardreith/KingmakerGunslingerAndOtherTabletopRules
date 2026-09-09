using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Visual.FogOfWar;
using Pathfinding;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal struct ElementalGroundPoint
    {
        internal NavGraph Graph;
        internal TriangleMeshNode Node;
        internal Vector3 Position;
    }

    // Owner-delegated CRPG adaptation: actual walkable ground, including built
    // floors. This does not classify material or claim that navigation is earth.
    internal static class ElementalTreacherousGroundRuntime
    {
        // Contact tolerance for native millimetre-quantized triangles. Never
        // project an arbitrary height or a point outside the triangle footprint.
        internal const float ContactToleranceMeters = .1f;
        private const float EndpointToleranceMeters = .005f;

        internal static bool TryTarget(UnitEntityData caster, TargetWrapper target,
            BlueprintAbility ability, out ElementalGroundPoint ground)
        {
            ground = default(ElementalGroundPoint);
            if (caster == null || caster.View == null || caster.Descriptor.State.IsDead ||
                target == null || target.Unit != null || ability == null ||
                ability.Range != AbilityRange.Touch || ability.Type != AbilityType.Supernatural)
                return false;
            ElementalGroundPoint origin;
            if (!TryGround(caster.Position, out origin) || !TryGround(target.Point, out ground)) return false;
            // Use the actual native touch command's distance, including the
            // caster's current size. No spell range or metamagic is introduced.
            float reach = new AbilityData(ability, caster.Descriptor).GetApproachDistance();
            return ElementalTreacherousPolicy.Eligible(true, Finite(reach) && reach >= 0 &&
                Vector3.Distance(caster.Position, ground.Position) <= reach,
                LineOfSightGeometry.Instance != null &&
                !LineOfSightGeometry.Instance.HasObstacle(caster.Position, ground.Position, 0),
                ConnectedSurface(origin, ground));
        }

        internal static bool SamePatchGround(Vector3 center, Vector3 position)
        {
            ElementalGroundPoint first, second;
            return TryGround(center, out first) && TryGround(position, out second) &&
                ConnectedSurface(first, second);
        }

        internal static bool TryGround(Vector3 point, out ElementalGroundPoint ground)
        {
            ground = default(ElementalGroundPoint);
            if (!Finite(point) || AstarPath.active == null || AstarPath.active.graphs == null) return false;
            // This constraint is request-owned. The native shared constraint,
            // graph flags, navigation and audio grids are never mutated.
            var constraint = new NNConstraint { constrainWalkability = true, walkable = true,
                constrainArea = false, constrainTags = false, constrainDistance = false,
                distanceXZ = false, distanceXZkeepY = false };
            float best = float.PositiveInfinity;
            foreach (NavGraph graph in AstarPath.active.graphs)
            {
                if (!(graph is IRaycastableGraph)) continue;
                var mesh = graph as NavMeshGraph;
                var recast = graph as RecastGraph;
                NNInfo found = new NNInfo(null);
                float distance = float.PositiveInfinity;
                // QueryClosest uses the installed native triangle's full 3D
                // closest point, unlike the game's XZ-only inside-navmesh check.
                if (mesh != null && mesh.bbTree != null)
                    found = mesh.bbTree.QueryClosest(point, constraint, out distance);
                else if (recast != null)
                {
                    var tiles = recast.GetTiles();
                    if (tiles == null) continue;
                    foreach (var tile in tiles)
                        if (tile != null && tile.bbTree != null)
                            found = tile.bbTree.QueryClosest(point, constraint, ref distance, found);
                }
                else continue; // unavailable or unsupported geometry fails closed
                var node = found.constrainedNode as TriangleMeshNode;
                if (node == null || !node.Walkable || !node.ContainsPoint((Int3)point)) continue;
                Vector3 contact = node.ClosestPointOnNode(point);
                float error = (point - contact).sqrMagnitude;
                if (!Finite(contact) || error > ContactToleranceMeters * ContactToleranceMeters || error >= best) continue;
                best = error;
                ground = new ElementalGroundPoint { Graph = graph, Node = node, Position = contact };
            }
            return ground.Node != null;
        }

        internal static bool ConnectedSurface(ElementalGroundPoint origin, ElementalGroundPoint target)
        {
            if (origin.Node == null || target.Node == null || !origin.Node.Walkable || !target.Node.Walkable ||
                !ReferenceEquals(origin.Graph, target.Graph)) return false;
            var raycast = origin.Graph as IRaycastableGraph;
            if (raycast == null) return false;
            // Installed Linecast follows real triangle portals but its terminal
            // ContainsPoint is XZ-only. Verify the terminal triangle's 3D point
            // explicitly, so a clear line on another floor cannot qualify.
            var trace = new List<GraphNode>();
            GraphHitInfo hit;
            if (raycast.Linecast(origin.Position, target.Position, origin.Node, out hit, trace)) return false;
            var terminal = (trace.Count == 0 ? hit.node : trace[trace.Count - 1]) as TriangleMeshNode;
            if (terminal == null || !terminal.Walkable ||
                (terminal.ClosestPointOnNode(target.Position) - target.Position).sqrMagnitude >
                    EndpointToleranceMeters * EndpointToleranceMeters) return false;
            foreach (var node in trace) if (node == null || !node.Walkable) return false;
            return true;
        }

        private static bool Finite(Vector3 value) { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
