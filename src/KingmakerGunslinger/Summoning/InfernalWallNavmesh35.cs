using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using Pathfinding;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Gives only the private Wall of Ice section a native dynamic navmesh
    /// cut.  The section unit remains the serialized lifetime/HP authority;
    /// this view-owned component is recreated by OnDataAttached after load.
    /// </summary>
    public sealed class InfernalWallNavmesh35 : MonoBehaviour
    {
        internal const float SectionLengthMeters = 3.05f;
        internal const float SectionThicknessMeters = .75f;
        internal const float SectionHeightMeters = 3.05f;

        private sealed class NodeClaim
        {
            internal AstarPath Path;
            internal GridGraph Grid;
            internal bool OriginalWalkable;
            internal readonly HashSet<InfernalWallNavmesh35> Owners =
                new HashSet<InfernalWallNavmesh35>();
        }

        // This is derived coordination, never saved gameplay truth. The
        // serialized section units recreate their view components and claims
        // after load. Reference counts only prevent overlapping live sections
        // from restoring a node still owned by another section.
        private static readonly Dictionary<GraphNode, NodeClaim> Claims =
            new Dictionary<GraphNode, NodeClaim>();

        private UnitEntityView _view;
        private UnitEntityData _owner;
        private NavmeshCut _cut;
        private TileHandlerHelper _tileHandler;
        private AstarPath _astar;
        private GridGraph _grid;
        private readonly List<GraphNode> _claimedNodes =
            new List<GraphNode>();
        private readonly Dictionary<GraphNode, bool> _nodeOriginals =
            new Dictionary<GraphNode, bool>();
        private bool _released;

        internal NavmeshCut Cut { get { return _cut; } }
        internal UnitEntityData Owner { get { return _owner; } }
        internal string UpdateRoute { get; private set; }
        internal int RecastApplyRequests { get; private set; }
        internal int GridGraphIndex { get; private set; } = -1;
        internal int GridNodesConsidered { get; private set; }
        internal int GridNodesClaimed { get; private set; }
        internal int GridNodesChanged { get; private set; }
        internal int GridNodesRestored { get; private set; }
        internal bool GridApplyQueued { get; private set; }
        internal bool GridApplyCompleted { get; private set; }
        internal bool GridRestoreQueued { get; private set; }
        internal bool GridRestoreCompleted { get; private set; }
        internal string GridFailure { get; private set; }
        internal static int ActiveGridClaimCount { get { return Claims.Count; } }

        internal KeyValuePair<GraphNode, bool>[] GridNodeSnapshot()
        { return _nodeOriginals.ToArray(); }

        internal static bool IsExactIdentity(UnitEntityView view)
        {
            UnitEntityData owner = view == null ? null :
                view.EntityData as UnitEntityData;
            return owner != null && owner.Blueprint != null &&
                owner.Blueprint.AssetGuid == InfernalRules35.WallSectionUnitGuid &&
                owner.Blueprint.name == InfernalRules35.WallSectionUnitName;
        }

        internal void Configure(UnitEntityView view)
        {
            if (!IsExactIdentity(view))
                throw new InvalidOperationException(
                    "Wall navmesh cut requires the exact private section identity.");
            if (view.GetComponent<NavmeshCut>() != null)
                throw new InvalidOperationException(
                    "Wall section donor unexpectedly already owns a navmesh cut.");
            _view = view;
            _owner = view.EntityData as UnitEntityData;
            _cut = view.gameObject.AddComponent<NavmeshCut>();
            _cut.type = NavmeshCut.MeshType.Rectangle;
            _cut.rectangleSize = new Vector2(SectionLengthMeters,
                SectionThicknessMeters);
            _cut.height = SectionHeightMeters;
            _cut.center = Vector3.zero;
            _cut.isDual = false;
            _cut.cutsAddedGeom = true;
            _cut.useRotation = true;
            _cut.updateDistance = .05f;
            _cut.updateRotationDistance = 1f;
            _cut.ForceUpdate();
            _tileHandler = UnityEngine.Object
                .FindObjectOfType<TileHandlerHelper>();
            if (_tileHandler != null && _tileHandler.isActiveAndEnabled)
            {
                _tileHandler.ForceUpdate();
                RecastApplyRequests++;
                UpdateRoute = "existing-native-tile-handler";
            }
            else UpdateRoute = "native-consumer-pending";
            QueueGridBlock();
        }

        private void QueueGridBlock()
        {
            _astar = AstarPath.active;
            if (_astar == null || _astar.graphs == null)
                throw new InvalidOperationException(
                    "Wall section requires the active native path graph.");
            float best = float.PositiveInfinity;
            for (int index = 0; index < _astar.graphs.Length; index++)
            {
                var grid = _astar.graphs[index] as GridGraph;
                if (grid == null || grid.nodes == null) continue;
                NNInfo nearest = grid.GetNearest(_view.transform.position,
                    NNConstraint.None);
                if (nearest.node == null) continue;
                float distance = Vector3.Distance(nearest.clampedPosition,
                    _view.transform.position);
                if (distance >= best) continue;
                best = distance;
                _grid = grid;
                GridGraphIndex = index;
            }
            if (_grid == null || best > Math.Max(1.5f, _grid.nodeSize * 2f))
                throw new InvalidOperationException(
                    "Wall section has no nearby native GridGraph.");

            Bounds bounds = _cut.GetBounds();
            bounds.Expand(new Vector3(_grid.nodeSize * 2f, 2f,
                _grid.nodeSize * 2f));
            Vector3 center = _view.transform.TransformPoint(_cut.center);
            Vector3 alongAxis = _view.transform.right.normalized;
            Vector3 acrossAxis = _view.transform.forward.normalized;
            float halfLength = _view.transform.TransformVector(
                Vector3.right * (SectionLengthMeters * .5f)).magnitude;
            float halfThickness = _view.transform.TransformVector(
                Vector3.forward * (SectionThicknessMeters * .5f)).magnitude;
            float nodeSize = _grid.nodeSize;
            GridApplyQueued = true;
            _astar.AddWorkItem(new AstarPath.AstarWorkItem(force =>
            {
                if (_released) return true;
                List<GraphNode> candidates = null;
                bool changed = false;
                try
                {
                    if (!ReferenceEquals(AstarPath.active, _astar))
                        throw new InvalidOperationException(
                            "Active path graph changed before wall admission.");
                    candidates = _grid.GetNodesInArea(bounds);
                    if (candidates == null)
                        throw new InvalidOperationException(
                            "GridGraph returned no wall footprint collection.");
                    GridNodesConsidered = candidates.Count;
                    foreach (GraphNode node in candidates)
                    {
                        if (node == null || node.GraphIndex !=
                                (uint)GridGraphIndex) continue;
                        Vector3 delta = (Vector3)node.position - center;
                        if (!InfernalRules35.WallNodeIntersects(
                                Vector3.Dot(delta, alongAxis),
                                Vector3.Dot(delta, acrossAxis), halfLength,
                                halfThickness, nodeSize)) continue;
                        NodeClaim claim;
                        if (!Claims.TryGetValue(node, out claim))
                        {
                            claim = new NodeClaim {
                                Path = _astar, Grid = _grid,
                                OriginalWalkable = node.Walkable };
                            Claims.Add(node, claim);
                        }
                        else if (!ReferenceEquals(claim.Path, _astar) ||
                            !ReferenceEquals(claim.Grid, _grid))
                            throw new InvalidOperationException(
                                "A wall node belongs to a different graph claim.");
                        if (!claim.Owners.Add(this)) continue;
                        _claimedNodes.Add(node);
                        _nodeOriginals[node] = claim.OriginalWalkable;
                        GridNodesClaimed++;
                        if (node.Walkable)
                        {
                            node.Walkable = false;
                            GridNodesChanged++;
                            changed = true;
                        }
                    }
                    if (GridNodesClaimed == 0)
                        throw new InvalidOperationException(
                            "Wall footprint did not claim a grid node.");
                    if (changed) _astar.QueueWorkItemFloodFill();
                    GridApplyCompleted = true;
                    UpdateRoute = UpdateRoute + "+wall-grid-work-item";
                }
                catch (Exception error)
                {
                    GridFailure = error.GetType().Name + ": " + error.Message;
                    if (ReleaseClaims(true))
                        _astar.QueueWorkItemFloodFill();
                    Debug.LogException(error);
                }
                finally
                {
                    if (candidates != null)
                        Pathfinding.Util.ListPool<GraphNode>.Release(candidates);
                }
                return true;
            }));
        }

        private bool ReleaseClaims(bool restoreWalkability)
        {
            bool changed = false;
            foreach (GraphNode node in _claimedNodes.ToArray())
            {
                NodeClaim claim;
                if (!Claims.TryGetValue(node, out claim) ||
                    !claim.Owners.Remove(this)) continue;
                if (claim.Owners.Count != 0) continue;
                if (restoreWalkability && node.Walkable !=
                    claim.OriginalWalkable)
                {
                    node.Walkable = claim.OriginalWalkable;
                    GridNodesRestored++;
                    changed = true;
                }
                Claims.Remove(node);
            }
            _claimedNodes.Clear();
            return changed;
        }

        private void QueueGridRestore()
        {
            if (GridRestoreQueued) return;
            GridRestoreQueued = true;
            if (_astar == null || !ReferenceEquals(AstarPath.active, _astar))
            {
                ReleaseClaims(false);
                GridRestoreCompleted = true;
                return;
            }
            _astar.AddWorkItem(new AstarPath.AstarWorkItem(force =>
            {
                try
                {
                    if (ReleaseClaims(true))
                        _astar.QueueWorkItemFloodFill();
                    GridRestoreCompleted = true;
                }
                catch (Exception error)
                {
                    GridFailure = error.GetType().Name + ": " + error.Message;
                    Debug.LogException(error);
                }
                return true;
            }));
        }

        internal void Release()
        {
            if (_released) return;
            _released = true;
            enabled = false;
            if (_cut != null)
            {
                UnityEngine.Object.DestroyImmediate(_cut);
                if (_tileHandler != null &&
                    _tileHandler.isActiveAndEnabled &&
                    RecastApplyRequests > 0)
                    _tileHandler.ForceUpdate();
            }
            QueueGridRestore();
            _cut = null;
            _tileHandler = null;
            _owner = null;
            _view = null;
        }

        private void Update()
        {
            if (_released) return;
            ModContext context;
            if (!ModContext.TryGet(out context) ||
                context.FeatureModules?.Active?.ExpandedSummoning != true)
                Release();
        }

        private void OnDestroy() { Release(); }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class InfernalWallNavmesh35AttachPatch
    {
        private static void Postfix(UnitEntityView __instance)
        {
            ModContext context;
            if (!InfernalWallNavmesh35.IsExactIdentity(__instance) ||
                !ModContext.TryGet(out context) ||
                context.FeatureModules?.Active == null ||
                !context.FeatureModules.Active.ExpandedSummoning ||
                __instance.GetComponent<InfernalWallNavmesh35>() != null)
                return;
            var owned = __instance.gameObject
                .AddComponent<InfernalWallNavmesh35>();
            try { owned.Configure(__instance); }
            catch (Exception error)
            {
                owned.Release();
                Debug.LogException(error);
            }
        }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class InfernalWallNavmesh35TeardownPatch
    {
        private static void Prefix(UnitEntityView __instance)
        {
            var owned = __instance == null ? null :
                __instance.GetComponent<InfernalWallNavmesh35>();
            if (owned == null) return;
            try { owned.Release(); }
            catch (Exception)
            {
                // Never prevent the native view destruction path.
            }
        }
    }
}
