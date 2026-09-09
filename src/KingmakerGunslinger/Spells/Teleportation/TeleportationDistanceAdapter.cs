using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using QuickGraph;
using QuickGraph.Algorithms;
using UnityEngine;
using NativeEdge = QuickGraph.TaggedEdge<Kingmaker.Globalmap.GlobalMapLocation, Kingmaker.Globalmap.GlobalMapEdge>;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class TeleportationDistanceAdapter
    {
        private static readonly FieldInfo GraphField = typeof(GlobalMapRules).GetField("m_Graph", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo WeightMethod = typeof(GlobalMapRules).GetMethod("GetGraphWeights", BindingFlags.Static | BindingFlags.NonPublic,
            null, new[] { typeof(NativeEdge) }, null);

        internal static IReadOnlyList<TeleportAlternateCandidate> Read(TeleportationWorldMapContext context,
            GlobalMapLocation intended, out string diagnostic)
        {
            if (context == null || !context.Usable || intended == null) throw new ArgumentException("Current native map and intended anchor required.");
            var legal = context.Rules.AllLocations.Where(value => value != null && value.Blueprint != null)
                .Select(value => new { Anchor = value, Point = TeleportationWorldMapAdapter.ReadDestination(context, value.Blueprint) })
                .Where(value => value.Point.Id != intended.Blueprint.AssetGuid &&
                    TeleportDestinationPolicy.Evaluate(value.Point, context.OriginId, TeleportationWorldMapAdapter.Forbidden).Eligible).ToArray();
            var graphDistances = new Dictionary<string, double>(StringComparer.Ordinal);
            diagnostic = "nativeGraph=available";
            try
            {
                if (GraphField == null || GraphField.FieldType != typeof(BidirectionalGraph<GlobalMapLocation, NativeEdge>) ||
                    WeightMethod == null || WeightMethod.ReturnType != typeof(double))
                    throw new InvalidOperationException("Native graph/weight contract differs.");
                var graph = (BidirectionalGraph<GlobalMapLocation, NativeEdge>)GraphField.GetValue(context.Rules);
                if (graph == null || !graph.ContainsVertex(intended)) throw new InvalidOperationException("Intended anchor is absent from the native graph.");
                // The native weight function reads Edge.Data, whose fallback creates
                // persistence. Require every record first so this remains read-only.
                foreach (NativeEdge edge in graph.Edges)
                {
                    MapEdgeData data;
                    if (edge.Tag == null || edge.Tag.Blueprint == null ||
                        !context.Map.Edges.TryGetValue(edge.Tag.Blueprint, out data) || data == null ||
                        !ReferenceEquals(data.Blueprint, edge.Tag.Blueprint))
                        throw new InvalidOperationException("Native graph has an edge without existing persistent state.");
                }
                var weight = (Func<NativeEdge, double>)Delegate.CreateDelegate(typeof(Func<NativeEdge, double>), WeightMethod);
                // Use the game's graph and its own Dijkstra implementation/weights.
                // This neither creates a route command nor traverses any route.
                var paths = AlgorithmExtensions.ShortestPathsDijkstra(graph, weight, intended);
                foreach (var candidate in legal)
                {
                    IEnumerable<NativeEdge> path;
                    if (paths(candidate.Anchor, out path)) graphDistances[candidate.Point.Id] = path.Sum(weight);
                }
                if (graphDistances.Count != legal.Length) diagnostic = "nativeGraph=incomplete;coordinateFallback=true";
            }
            catch (Exception exception)
            {
                graphDistances.Clear();
                diagnostic = "nativeGraph=unavailable;coordinateFallback=true;reason=" + exception.GetType().FullName + ": " + exception.Message;
            }
            // Native FindPath uses this same graph/weight operation. Its route
            // wrapper provides no independent distance when this graph is absent.
            return Array.AsReadOnly(legal.Select(value => new TeleportAlternateCandidate(value.Point,
                graphDistances.ContainsKey(value.Point.Id) ? (double?)graphDistances[value.Point.Id] : null,
                null, Vector3.Distance(intended.transform.position, value.Anchor.transform.position))).ToArray());
        }
    }
}
