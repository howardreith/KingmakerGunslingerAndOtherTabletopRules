using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class TeleportRouteBoundary
    {
        internal TeleportRouteBoundary(string pointId, double distance, bool qualifying)
        { PointId = pointId; Distance = distance; Qualifying = qualifying; }
        internal string PointId { get; private set; }
        internal double Distance { get; private set; }
        internal bool Qualifying { get; private set; }
    }

    // One native MoveAlongEdge invocation, after its partial-edge start initialization.
    // Distances and ordered boundaries are observations of the native planned path.
    // This policy neither plans a path nor advances movement.
    internal sealed class TeleportOrdinaryArrivalObservation
    {
        private readonly TeleportRouteBoundary[] _boundaries;
        private readonly double _before;
        private int _completed;

        internal TeleportOrdinaryArrivalObservation(double before,
            IEnumerable<TeleportRouteBoundary> boundaries)
        {
            if (!Finite(before) || before < 0) throw new ArgumentOutOfRangeException("before");
            if (boundaries == null) throw new ArgumentNullException("boundaries");
            _boundaries = boundaries.ToArray();
            double previous = 0;
            foreach (TeleportRouteBoundary boundary in _boundaries)
            {
                if (boundary == null || !TeleportDestinationPolicy.IsStableId(boundary.PointId) ||
                    !Finite(boundary.Distance) || boundary.Distance <= previous)
                    throw new ArgumentException("Native route boundaries must have stable IDs and strictly increasing finite distances.");
                previous = boundary.Distance;
            }
            _before = before;
        }

        internal string[] Complete(double after, bool nativeRouteEnded, string settledPointId)
        {
            if (Interlocked.Exchange(ref _completed, 1) != 0) return new string[0];
            if (!Finite(after) || after <= _before) return new string[0];
            if (nativeRouteEnded)
            {
                // Native reveal handling may settle at a crossed junction before the
                // frame's nominal distance. Never count points beyond that actual stop.
                TeleportRouteBoundary stop = _boundaries.FirstOrDefault(value =>
                    value.PointId == settledPointId && value.Distance > _before && value.Distance <= after);
                if (stop == null) return new string[0];
                after = stop.Distance;
            }
            return _boundaries.Where(value => value.Qualifying &&
                value.Distance > _before && value.Distance <= after).Select(value => value.PointId).ToArray();
        }

        private static bool Finite(double value) { return !double.IsNaN(value) && !double.IsInfinity(value); }
    }
}
