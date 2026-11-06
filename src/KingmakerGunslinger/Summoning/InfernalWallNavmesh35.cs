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
        internal const float SectionLengthMeters =
            InfernalRules35.WallOfIceSectionLengthMeters;
        internal const float SectionThicknessMeters =
            InfernalRules35.WallOfIceSectionThicknessMeters;
        internal const float SectionHeightMeters = 3.05f;

        private sealed class NodeClaim
        {
            internal AstarPath Path;
            internal GridGraph Grid;
            // The node's own static flag and pre-erosion flag before the
            // first claim. Combined walkability is not recorded: it also
            // folds in cuts' and turn-based units' DynamicBlocked counters,
            // which the claim must never restore into the static layer.
            internal bool OriginalWalkable;
            internal bool OriginalErosion;
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
        private int _gridGeneration;
        private GameObject _wallVisual;
        private Material _wallMaterial;
        private readonly HashSet<Renderer> _hiddenRenderers =
            new HashSet<Renderer>();
        private readonly List<Renderer> _rendererScratch =
            new List<Renderer>();
        private readonly List<GraphNode> _claimedNodes =
            new List<GraphNode>();
        private readonly Dictionary<GraphNode, bool> _nodeOriginals =
            new Dictionary<GraphNode, bool>();
        // Each node this section claimed and the claim record holding its
        // original static and pre-erosion flags; kept after release so a
        // reconcile can still restore both layers once the shared claim
        // entry is gone.
        private readonly Dictionary<GraphNode, NodeClaim> _nodeRecords =
            new Dictionary<GraphNode, NodeClaim>();
        private bool _released;
        private bool _liveReconcileQueued;
        private bool _restoreReconcileQueued;
        private int _restoreExactFrames;
        private bool _retirePending;
        private int _retireDeferredFrames;
        private bool _restoreSettled;

        // One frame is enough for the pathfinder to run GridGraph.UpdateArea
        // in the ordinary case. This bound only stops an indefinitely stalled
        // update from leaking owned claims; it is a safety valve, not a
        // scheduling policy.
        internal const int RetireDeferFrameBudget = 120;

        internal NavmeshCut Cut { get { return _cut; } }
        internal UnitEntityData Owner { get { return _owner; } }
        internal GameObject WallVisual { get { return _wallVisual; } }
        internal int HiddenDonorRendererCount
        { get { return _hiddenRenderers.Count; } }
        internal int DonorRendererSuppressions { get; private set; }
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
        // Whether the native combat controller had applied this cut's grid
        // DynamicBlocked block at release, and whether its paired revert was
        // submitted (exactly once, by Release).
        internal bool CutGridBlockAppliedAtRelease { get; private set; }
        internal bool CutGridRevertQueued { get { return NativeGridCutRevertQueued; } }
        internal int GridReconcileRequests { get; private set; }
        internal int GridReconcileCompleted { get; private set; }
        internal int GridLiveReconcileRequests { get; private set; }
        internal int GridLiveReconcileCompleted { get; private set; }
        internal bool NativeCutDisabledBeforeDestroy { get; private set; }
        internal bool NativeGridCutRevertQueued { get; private set; }
        internal int RetireDeferrals { get; private set; }
        internal string NativeGridCutRevertSkipReason { get; private set; }
        internal bool ReleaseCompleted { get { return _released; } }
        internal bool RetirePending { get { return _retirePending && !_released; } }
        internal string GridFailure { get; private set; }
        // Native connection recalculations after this section's own writes.
        internal int GridConnectionRefreshes { get; private set; }
        internal int GridTagRefreshes { get; private set; }
        internal static int ActiveGridClaimCount { get { return Claims.Count; } }

        internal KeyValuePair<GraphNode, bool>[] GridNodeSnapshot()
        { return _nodeOriginals.ToArray(); }
        internal static int ClaimOwnerCount(GraphNode node)
        {
            NodeClaim claim;
            return node != null && Claims.TryGetValue(node, out claim) ?
                claim.Owners.Count : 0;
        }
        // Read-only: whether this exact section currently owns the node claim.
        internal static bool ClaimOwnedBy(GraphNode node,
            InfernalWallNavmesh35 owner)
        {
            NodeClaim claim;
            return node != null && owner != null &&
                Claims.TryGetValue(node, out claim) &&
                claim.Owners.Contains(owner);
        }
        // Read-only: a donor body the native renderer actually drew last frame.
        internal int VisibleDonorRendererCount()
        {
            GetComponentsInChildren(true, _rendererScratch);
            return _rendererScratch.Count(renderer => renderer != null &&
                renderer.isVisible && SuppressesRenderer(renderer));
        }

        internal static bool IsExactIdentity(UnitEntityView view)
        {
            UnitEntityData owner = view == null ? null :
                view.EntityData as UnitEntityData;
            return owner != null && owner.Blueprint != null &&
                (owner.Blueprint.AssetGuid == InfernalRules35.WallSectionUnitGuid &&
                    owner.Blueprint.name == InfernalRules35.WallSectionUnitName ||
                 owner.Blueprint.AssetGuid == IceDevilRules40Policy.WallSectionGuid &&
                    owner.Blueprint.name == IceDevilRules40Policy.WallSectionName);
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
            CreateWallVisual();
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
            QueueGridBlock(false);
        }

        private void CreateWallVisual()
        {
            SuppressDonorRenderers();
            DonorRendererSuppressions = 0;
            _wallVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _wallVisual.name = "KMG_S35_WallOfIceSection";
            _wallVisual.transform.SetParent(_view.transform, false);
            _wallVisual.transform.localPosition = Vector3.up *
                (SectionHeightMeters * .5f);
            _wallVisual.transform.localRotation = Quaternion.identity;
            _wallVisual.transform.localScale = new Vector3(
                SectionLengthMeters, SectionHeightMeters,
                SectionThicknessMeters * .5f);
            Collider collider = _wallVisual.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            Renderer wallRenderer = _wallVisual.GetComponent<Renderer>();
            Shader shader = Shader.Find("Standard") ??
                Shader.Find("Legacy Shaders/Diffuse");
            if (wallRenderer == null || shader == null)
                throw new InvalidOperationException(
                    "Wall section requires a supported opaque shader.");
            _wallMaterial = new Material(shader) {
                name = "KMG_S35_WallOfIceMaterial",
                color = new Color(.48f, .78f, .95f, 1f)
            };
            wallRenderer.sharedMaterial = _wallMaterial;
        }

        private void QueueGridBlock(bool replacing)
        {
            int generation = ++_gridGeneration;
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
                if (_released || generation != _gridGeneration) return true;
                List<GraphNode> candidates = null;
                var written = new List<GraphNode>();
                bool changed = false;
                try
                {
                    if (!ReferenceEquals(AstarPath.active, _astar))
                        throw new InvalidOperationException(
                            "Active path graph changed before wall admission.");
                    if (replacing)
                    {
                        changed = ReleaseClaims(true, written);
                        _nodeOriginals.Clear();
                        _nodeRecords.Clear();
                        GridNodesConsidered = 0;
                        GridNodesClaimed = 0;
                        GridNodesChanged = 0;
                        GridApplyCompleted = false;
                    }
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
                            var gridNode = node as GridNode;
                            claim = new NodeClaim {
                                Path = _astar, Grid = _grid,
                                OriginalWalkable = node.StaticWalkable,
                                OriginalErosion = gridNode != null ?
                                    gridNode.WalkableErosion : node.StaticWalkable };
                            Claims.Add(node, claim);
                        }
                        else if (!ReferenceEquals(claim.Path, _astar) ||
                            !ReferenceEquals(claim.Grid, _grid))
                            throw new InvalidOperationException(
                                "A wall node belongs to a different graph claim.");
                        if (!claim.Owners.Add(this)) continue;
                        _claimedNodes.Add(node);
                        _nodeOriginals[node] = claim.OriginalWalkable;
                        _nodeRecords[node] = claim;
                        GridNodesClaimed++;
                        bool wasWalkable = node.Walkable;
                        if (ApplyLayers(node, InfernalRules35.WallClaimNode(
                                LayersOf(node))))
                        {
                            written.Add(node);
                            if (wasWalkable) GridNodesChanged++;
                            changed = true;
                        }
                    }
                    if (GridNodesClaimed == 0)
                        throw new InvalidOperationException(
                            "Wall footprint did not claim a grid node.");
                    if (changed) RefreshGrid(written);
                    GridApplyCompleted = true;
                    UpdateRoute = UpdateRoute + "+wall-grid-work-item";
                }
                catch (Exception error)
                {
                    GridFailure = error.GetType().Name + ": " + error.Message;
                    if (ReleaseClaims(true, written) || written.Count > 0)
                        RefreshGrid(written);
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

        internal void Reorient(float orientationDegrees)
        {
            if (_released || _view == null || _owner == null ||
                float.IsNaN(orientationDegrees) ||
                float.IsInfinity(orientationDegrees)) return;
            _owner.Orientation = orientationDegrees;
            _owner.DesiredOrientation = orientationDegrees;
            _view.transform.rotation = Quaternion.Euler(0f,
                orientationDegrees, 0f);
            if (_cut != null) _cut.ForceUpdate();
            if (_tileHandler != null && _tileHandler.isActiveAndEnabled)
            {
                _tileHandler.ForceUpdate();
                RecastApplyRequests++;
            }
            GridApplyQueued = false;
            QueueGridBlock(true);
        }

        private bool ReleaseClaims(bool restoreWalkability,
            List<GraphNode> written)
        {
            bool changed = false;
            foreach (GraphNode node in _claimedNodes.ToArray())
            {
                NodeClaim claim;
                if (!Claims.TryGetValue(node, out claim) ||
                    !claim.Owners.Remove(this)) continue;
                bool remainingOwner = claim.Owners.Count != 0;
                if (restoreWalkability && ApplyLayers(node,
                        DesiredLayers(node, claim, remainingOwner)))
                {
                    GridNodesRestored++;
                    if (written != null) written.Add(node);
                    changed = true;
                }
                if (remainingOwner) continue;
                Claims.Remove(node);
            }
            _claimedNodes.Clear();
            return changed;
        }

        // The static layers a node must hold once this section no longer
        // owns it: still blocked in both layers while another section owns
        // it, otherwise exactly the static and pre-erosion flags recorded
        // before the first claim. DynamicBlocked counters are never written:
        // they belong to the native cuts and turn-based units.
        private static InfernalRules35.WallGridNodeLayers DesiredLayers(
            GraphNode node, NodeClaim claim, bool remainingOwner)
        {
            var layers = LayersOf(node);
            return remainingOwner ? InfernalRules35.WallClaimNode(layers) :
                InfernalRules35.WallRestoreNode(layers,
                    InfernalRules35.WallNodeWalkableAfterOwnerRelease(
                        claim.OriginalWalkable, false),
                    claim.OriginalErosion);
        }

        private static InfernalRules35.WallGridNodeLayers LayersOf(GraphNode node)
        {
            var grid = node as GridNode;
            return new InfernalRules35.WallGridNodeLayers {
                Static = node.StaticWalkable,
                Erosion = grid != null ? grid.WalkableErosion : node.StaticWalkable,
                Dynamic = node.DynamicBlocked };
        }

        private static bool SameLayers(GraphNode node,
            InfernalRules35.WallGridNodeLayers layers)
        {
            var grid = node as GridNode;
            return node.StaticWalkable == layers.Static &&
                (grid == null || grid.WalkableErosion == layers.Erosion);
        }

        // Writes only the static layers that differ; reports any write.
        private static bool ApplyLayers(GraphNode node,
            InfernalRules35.WallGridNodeLayers layers)
        {
            bool wrote = false;
            if (node.StaticWalkable != layers.Static)
            {
                node.Walkable = layers.Static;
                wrote = true;
            }
            var grid = node as GridNode;
            if (grid != null && grid.WalkableErosion != layers.Erosion)
            {
                grid.WalkableErosion = layers.Erosion;
                wrote = true;
            }
            return wrote;
        }

        // Installed GridGraph.UpdateArea recalculates grid connections after
        // it changes walkability, and a claim writes the flags directly, so
        // it does the same for every node it wrote and their neighbours.
        // Otherwise a node restored to walkable keeps the connections a
        // native update computed while it was claimed (close28: after
        // physical destruction the restored footprint stayed a detour on
        // the grid). It then re-erodes the tags over the same reach the
        // native update uses (written cells expanded twice by
        // erodeIterations): a native update that ran while the claim stood
        // tagged the surrounding cells around it, and restoring the flags
        // alone left those tags stale (nav5: 357-387 of 676 region cells).
        // Tag erosion is only run where the grid erodes into tags; with
        // erosion into walkability it would compound. Runs inside a
        // pathfinding work item, and only on the graph the claims were
        // captured on.
        private void RefreshGrid(List<GraphNode> written)
        {
            GridGraph grid = _grid;
            if (_astar == null || !ReferenceEquals(AstarPath.active, _astar))
                return;
            if (grid != null && grid.nodes != null &&
                grid.nodes.Length == grid.width * grid.depth)
            {
                int xmin = int.MaxValue, zmin = int.MaxValue;
                int xmax = int.MinValue, zmax = int.MinValue;
                foreach (GraphNode node in written)
                {
                    var gridNode = node as GridNode;
                    if (gridNode == null ||
                        gridNode.GraphIndex != (uint)GridGraphIndex) continue;
                    int x = gridNode.NodeInGridIndex % grid.width;
                    int z = gridNode.NodeInGridIndex / grid.width;
                    xmin = Math.Min(xmin, x); xmax = Math.Max(xmax, x);
                    zmin = Math.Min(zmin, z); zmax = Math.Max(zmax, z);
                }
                if (xmin <= xmax)
                {
                    for (int z = Math.Max(0, zmin - 1);
                        z <= Math.Min(grid.depth - 1, zmax + 1); z++)
                        for (int x = Math.Max(0, xmin - 1);
                            x <= Math.Min(grid.width - 1, xmax + 1); x++)
                        {
                            GridNode node = grid.nodes[z * grid.width + x];
                            if (node != null) grid.CalculateConnections(x, z, node);
                        }
                    GridConnectionRefreshes++;
                    int reach = grid.erodeIterations;
                    if (grid.erosionUseTags && reach > 0)
                    {
                        grid.ErodeWalkableArea(Math.Max(0, xmin - 2 * reach),
                            Math.Max(0, zmin - 2 * reach),
                            Math.Min(grid.width, xmax + 2 * reach + 1),
                            Math.Min(grid.depth, zmax + 2 * reach + 1));
                        GridTagRefreshes++;
                    }
                }
            }
            _astar.QueueWorkItemFloodFill();
        }

        // Every node this released section recorded holds its desired static
        // layers (DesiredLayers), whoever still owns it.
        private bool ReleasedGridIsExact()
        {
            return _nodeRecords.All(pair =>
            {
                NodeClaim claim;
                bool remainingOwner = Claims.TryGetValue(pair.Key,
                    out claim) && claim.Owners.Count > 0;
                return SameLayers(pair.Key, ReleasedLayers(pair.Key,
                    pair.Value, remainingOwner));
            });
        }

        private static InfernalRules35.WallGridNodeLayers ReleasedLayers(
            GraphNode node, NodeClaim recorded, bool remainingOwner)
        {
            return DesiredLayers(node, recorded, remainingOwner);
        }

        // Every live claim is still owned by this section and blocked in
        // both static layers.
        private bool LiveGridIsExact()
        {
            return _claimedNodes.All(node =>
            {
                NodeClaim claim;
                return Claims.TryGetValue(node, out claim) &&
                    claim.Owners.Contains(this) &&
                    SameLayers(node, InfernalRules35.WallClaimNode(LayersOf(node)));
            });
        }

        private void QueueLiveGridReconcile()
        {
            if (_liveReconcileQueued || _released ||
                _claimedNodes.Count == 0 || _astar == null ||
                !ReferenceEquals(AstarPath.active, _astar)) return;
            _liveReconcileQueued = true;
            GridLiveReconcileRequests++;
            _astar.AddWorkItem(new AstarPath.AstarWorkItem(force =>
            {
                var written = new List<GraphNode>();
                try
                {
                    if (_released || !ReferenceEquals(AstarPath.active, _astar))
                        return true;
                    foreach (GraphNode node in _claimedNodes)
                    {
                        NodeClaim claim;
                        if (!Claims.TryGetValue(node, out claim) ||
                            !claim.Owners.Contains(this)) continue;
                        bool wasWalkable = node.Walkable;
                        if (!ApplyLayers(node,
                                InfernalRules35.WallClaimNode(LayersOf(node))))
                            continue;
                        written.Add(node);
                        if (wasWalkable) GridNodesChanged++;
                    }
                    if (written.Count > 0) RefreshGrid(written);
                }
                catch (Exception error)
                {
                    GridFailure = error.GetType().Name + ": " +
                        error.Message;
                    Debug.LogException(error);
                }
                finally
                {
                    GridLiveReconcileCompleted++;
                    _liveReconcileQueued = false;
                }
                return true;
            }));
        }

        private void QueueReleasedGridReconcile()
        {
            if (_restoreReconcileQueued || _nodeRecords.Count == 0 ||
                _astar == null || !ReferenceEquals(AstarPath.active, _astar))
                return;
            _restoreReconcileQueued = true;
            GridReconcileRequests++;
            _astar.AddWorkItem(new AstarPath.AstarWorkItem(force =>
            {
                var written = new List<GraphNode>();
                try
                {
                    // Restoration data never reaches a replacement graph.
                    if (!ReferenceEquals(AstarPath.active, _astar)) return true;
                    foreach (var pair in _nodeRecords)
                    {
                        NodeClaim claim;
                        bool remainingOwner = Claims.TryGetValue(pair.Key,
                            out claim) && claim.Owners.Count > 0;
                        if (!ApplyLayers(pair.Key, ReleasedLayers(pair.Key,
                                pair.Value, remainingOwner))) continue;
                        GridNodesRestored++;
                        written.Add(pair.Key);
                    }
                    if (written.Count > 0) RefreshGrid(written);
                }
                catch (Exception error)
                {
                    GridFailure = error.GetType().Name + ": " +
                        error.Message;
                    Debug.LogException(error);
                }
                finally
                {
                    GridReconcileCompleted++;
                    _restoreReconcileQueued = false;
                }
                return true;
            }));
        }

        internal void ReconcileAfterNativeRetirement()
        {
            if (!_released) Release();
            QueueReleasedGridReconcile();
        }

        private void QueueGridRestore()
        {
            if (GridRestoreQueued) return;
            GridRestoreQueued = true;
            if (_astar == null || !ReferenceEquals(AstarPath.active, _astar))
            {
                ReleaseClaims(false, null);
                GridRestoreCompleted = true;
                return;
            }
            _astar.AddWorkItem(new AstarPath.AstarWorkItem(force =>
            {
                try
                {
                    var written = new List<GraphNode>();
                    if (!ReferenceEquals(AstarPath.active, _astar))
                        ReleaseClaims(false, null);
                    else if (ReleaseClaims(true, written))
                        RefreshGrid(written);
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

        internal void Release() { Release(false); }

        /// <summary>
        /// Retires this wall's navigation ownership, visuals and native cut.
        ///
        /// Retirement is resumable and idempotent. A transient in-flight
        /// native grid update leaves retirement pending (cut disabled, every
        /// claim and reference intact) and retries from <c>Update</c>; a
        /// replaced graph completes without a native revert; the paired
        /// native revert is submitted at most once, on the exact captured
        /// graph. <paramref name="force"/> is used by destruction, where no
        /// later frame exists to resume on.
        /// </summary>
        /// <returns>True when retirement completed; false when it is pending
        /// and will resume on a later frame.</returns>
        internal bool Release(bool force)
        {
            if (_released) return true;
            if (_cut != null)
            {
                // Kingmaker's combat navigation consumer applies a separate
                // GridGraph update object (DynamicBlocked counters) for each
                // native cut, and reverts it only while the cut is listed.
                // Submit the paired revert while this exact cut is still
                // available; the owned claims remain the cross-owner static
                // authority and are released by QueueGridRestore below.
                _cut.enabled = false;
                NativeCutDisabledBeforeDestroy = !_cut.enabled;
                bool graphExact = _astar != null &&
                    ReferenceEquals(AstarPath.active, _astar) &&
                    GridGraphIndex >= 0 && GridGraphIndex < 31;
                bool updateInFlight = false;
                bool cutApplied = _cut.gridGuoApplied;
                CutGridBlockAppliedAtRelease = cutApplied;
                NavmeshCut.UpdateObjectPair updatePair =
                    default(NavmeshCut.UpdateObjectPair);
                if (cutApplied && graphExact)
                {
                    bool cutEnabled;
                    updatePair = _cut.GetUpdateObjectForGrid(out cutEnabled,
                        1 << GridGraphIndex);
                    if (updatePair.guo == null || updatePair.revertGuo == null)
                        // The engine holds no paired revert object for this
                        // cut, so there is nothing of ours to submit.
                        cutApplied = false;
                    else
                        updateInFlight = updatePair.guo.updating ||
                            updatePair.revertGuo.updating;
                }
                var step = InfernalRules35.PlanWallRetirement(
                    cutApplied, graphExact, updateInFlight, force,
                    _retireDeferredFrames, RetireDeferFrameBudget);
                if (step == InfernalRules35.WallRetirementStep.Defer)
                {
                    // Queued-not-yet-processed. Leave the cut disabled, keep
                    // every claim and reference intact, and resume on a later
                    // frame; never spin the Unity thread waiting here.
                    _retirePending = true;
                    _retireDeferredFrames++;
                    RetireDeferrals++;
                    enabled = true;
                    return false;
                }
                if (step == InfernalRules35.WallRetirementStep.SkipNativeCut)
                {
                    NativeGridCutRevertSkipReason =
                        InfernalRules35.WallRetirementSkipReason(
                            cutApplied, graphExact, updateInFlight,
                            force, _retireDeferredFrames,
                            RetireDeferFrameBudget);
                    // Only the bookkeeping is cleared. No restoration data is
                    // applied, so nothing of ours can reach a replacement
                    // graph; QueueGridRestore enforces the same rule for the
                    // owned claims.
                    _cut.gridGuoApplied = false;
                }
                else
                {
                    updatePair.revertGuo.updating = true;
                    _astar.UpdateGraphs(updatePair.revertGuo);
                    _cut.gridGuoApplied = false;
                    _cut.NotifyUpdatedForGrid();
                    NativeGridCutRevertQueued = true;
                }
                UnityEngine.Object.DestroyImmediate(_cut);
                // OnDestroy queues the exact old recast bounds with the
                // installed TileHandlerHelper, so process that queue only
                // after the callback has run.
                if (_tileHandler != null &&
                    _tileHandler.isActiveAndEnabled &&
                    RecastApplyRequests > 0)
                    _tileHandler.ForceUpdate();
            }
            _released = true;
            _retirePending = false;
            if (_wallVisual != null)
                UnityEngine.Object.DestroyImmediate(_wallVisual);
            if (_wallMaterial != null)
                UnityEngine.Object.DestroyImmediate(_wallMaterial);
            // The donor body never returns: a retiring section is dead or
            // leaving the world, and this component stays enabled to keep it
            // hidden (LateUpdate) until the native view is destroyed.
            QueueGridRestore();
            _cut = null;
            _wallVisual = null;
            _wallMaterial = null;
            _tileHandler = null;
            _owner = null;
            _view = null;
            return true;
        }

        private void Update()
        {
            // A deferred retirement resumes here, on a later frame, once the
            // pathfinder has run GridGraph.UpdateArea for the in-flight update.
            if (!_released && _retirePending && !Release(false)) return;
            if (_released)
            {
                if (_restoreSettled || !GridRestoreCompleted ||
                    _restoreReconcileQueued) return;
                if (!ReleasedGridIsExact())
                {
                    _restoreExactFrames = 0;
                    QueueReleasedGridReconcile();
                    return;
                }
                // The component stays enabled for donor suppression; only
                // the restoration watch settles.
                if (++_restoreExactFrames >= 2) _restoreSettled = true;
                return;
            }
            if (GridApplyCompleted && !_liveReconcileQueued &&
                !LiveGridIsExact())
            {
                QueueLiveGridReconcile();
                return;
            }
            ModContext context;
            if (!ModContext.TryGet(out context) ||
                context.FeatureModules?.Active?.ExpandedSummoning != true)
            {
                UnitEntityData retiring = _owner;
                Release();
                InfernalWallSection35.Retire(retiring);
            }
        }

        // Native fade/visibility controllers revisit every view renderer after
        // attachment and would otherwise reveal the theropod donor inside the
        // ice, or standing in its place after a breach.
        private void LateUpdate() { SuppressDonorRenderers(); }

        private bool SuppressesRenderer(Renderer renderer)
        {
            return InfernalRules35.WallSuppressesRenderer(
                _wallVisual != null &&
                    ReferenceEquals(renderer.gameObject, _wallVisual),
                renderer is SkinnedMeshRenderer || renderer is MeshRenderer);
        }

        private void SuppressDonorRenderers()
        {
            GetComponentsInChildren(true, _rendererScratch);
            foreach (Renderer renderer in _rendererScratch)
            {
                if (renderer == null || !SuppressesRenderer(renderer)) continue;
                _hiddenRenderers.Add(renderer);
                if (!renderer.enabled) continue;
                renderer.enabled = false;
                DonorRendererSuppressions++;
            }
        }

        private void OnDestroy()
        {
            // Destruction has no later frame to resume a deferred retirement
            // on, so force completion rather than leak owned claims.
            if (!_released) Release(true);
            QueueReleasedGridReconcile();
        }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class InfernalWallNavmesh35AttachPatch
    {
        private static void Postfix(UnitEntityView __instance)
        {
            ModContext context;
            if (!InfernalWallNavmesh35.IsExactIdentity(__instance)) return;
            if (!ModContext.TryGet(out context) ||
                context.FeatureModules?.Active == null ||
                !context.FeatureModules.Active.ExpandedSummoning)
            {
                InfernalWallSection35.Retire(__instance.EntityData as
                    UnitEntityData);
                return;
            }
            if (__instance.GetComponent<InfernalWallNavmesh35>() != null)
                return;
            var owned = __instance.gameObject
                .AddComponent<InfernalWallNavmesh35>();
            try { owned.Configure(__instance); }
            catch (Exception error)
            {
                // The component is being abandoned, so force completion.
                owned.Release(true);
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
            // The view is going away, so this retirement cannot be resumed
            // on a later frame either.
            try { owned.Release(true); }
            catch (Exception)
            {
                // Never prevent the native view destruction path.
            }
        }
    }
}
