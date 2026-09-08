using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal enum TeleportDistanceBasis { Graph, Route, Coordinates }
    internal sealed class TeleportAlternateCandidate
    {
        internal TeleportAlternateCandidate(TeleportDestinationSnapshot point, double? graph, double? route, double? coordinates)
        { Point = point; Graph = graph; Route = route; Coordinates = coordinates; }
        internal TeleportDestinationSnapshot Point { get; private set; }
        internal double? Graph { get; private set; }
        internal double? Route { get; private set; }
        internal double? Coordinates { get; private set; }
        internal double? Distance(TeleportDistanceBasis basis)
        { return basis == TeleportDistanceBasis.Graph ? Graph : basis == TeleportDistanceBasis.Route ? Route : Coordinates; }
    }
    internal sealed class TeleportAlternateDecision
    {
        internal TeleportAlternateDecision(string id, double distance, TeleportDistanceBasis basis,
            int count, int bucket, int firstRank, int bucketSize, double severity)
        { Id = id; Distance = distance; Basis = basis; CandidateCount = count; Bucket = bucket;
            FirstRank = firstRank; BucketSize = bucketSize; Severity = severity; }
        internal bool Found { get { return Id != null; } }
        internal string Id { get; private set; }
        internal double Distance { get; private set; }
        internal TeleportDistanceBasis Basis { get; private set; }
        internal bool CoordinateFallback { get { return Basis == TeleportDistanceBasis.Coordinates; } }
        internal int CandidateCount { get; private set; }
        internal int Bucket { get; private set; }
        internal int FirstRank { get; private set; }
        internal int BucketSize { get; private set; }
        internal double Severity { get; private set; }
    }
    internal static class TeleportAlternateDestinationPolicy
    {
        internal static TeleportAlternateDecision Choose(IEnumerable<TeleportAlternateCandidate> candidates,
            string intendedId, string originId, TeleportForbiddenDestinationCatalog catalog,
            double severity, Func<int, int> chooseIndex)
        {
            if (candidates == null) throw new ArgumentNullException("candidates");
            if (chooseIndex == null) throw new ArgumentNullException("chooseIndex");
            if (double.IsNaN(severity) || double.IsInfinity(severity)) throw new ArgumentOutOfRangeException("severity");
            if (!TeleportDestinationPolicy.IsStableId(intendedId)) throw new ArgumentException("Stable target required.", "intendedId");
            var legal = candidates.Where(value => value != null &&
                TeleportDestinationPolicy.Evaluate(value.Point, originId, catalog).Eligible && value.Point.Id != intendedId)
                .GroupBy(value => value.Point.Id, StringComparer.Ordinal)
                .Where(group => group.Count() == 1).Select(group => group.First()).ToArray();
            // Never mix graph/route/coordinate units in one ranked list. A complete native
            // distance set wins; otherwise a complete next-preference set is used explicitly.
            TeleportDistanceBasis basis = TeleportDistanceBasis.Graph;
            if (!legal.All(value => Valid(value.Graph))) basis = TeleportDistanceBasis.Route;
            if (basis == TeleportDistanceBasis.Route && !legal.All(value => Valid(value.Route))) basis = TeleportDistanceBasis.Coordinates;
            legal = legal.Where(value => Valid(value.Distance(basis)))
                .OrderBy(value => value.Distance(basis).Value).ThenBy(value => value.Point.Id, StringComparer.Ordinal).ToArray();
            if (legal.Length == 0) return new TeleportAlternateDecision(null, 0, basis, 0, -1, 0, 0, severity);
            int bucket = TeleportFailureSeverityPolicy.Bucket(severity, legal.Length);
            int first = TeleportFailureSeverityPolicy.FirstRank(bucket, legal.Length);
            int size = TeleportFailureSeverityPolicy.RankCount(bucket, legal.Length);
            int offset = chooseIndex(size);
            if (offset < 0 || offset >= size) throw new InvalidOperationException("Alternate RNG returned an out-of-bucket index.");
            var selected = legal[first + offset];
            return new TeleportAlternateDecision(selected.Point.Id, selected.Distance(basis).Value,
                basis, legal.Length, bucket, first, size, severity);
        }
        private static bool Valid(double? value)
        { return value.HasValue && value.Value >= 0 && !double.IsNaN(value.Value) && !double.IsInfinity(value.Value); }
    }
}
