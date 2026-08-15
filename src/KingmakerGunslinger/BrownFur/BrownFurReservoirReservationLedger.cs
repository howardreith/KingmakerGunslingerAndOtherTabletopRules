using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>
    /// Reserves an owner's currently available reservoir points for queued
    /// Brown-Fur casts without mutating the native resource before commit.
    /// </summary>
    internal sealed class BrownFurReservoirReservationLedger<TOwner>
        where TOwner : class
    {
        private sealed class Reservation
        {
            internal TOwner Owner;
            internal int Cost;
        }

        private readonly object _gate = new object();
        private readonly Dictionary<string, Reservation> _reservations =
            new Dictionary<string, Reservation>(StringComparer.Ordinal);
        private readonly Dictionary<TOwner, int> _reservedByOwner =
            new Dictionary<TOwner, int>(ReferenceComparer.Instance);

        internal int ReservationCount
        { get { lock (_gate) return _reservations.Count; } }

        internal bool TryReserve(TOwner owner, string transactionIdentity,
            int cost, int availablePoints)
        {
            if (owner == null || string.IsNullOrWhiteSpace(transactionIdentity) ||
                cost < 0 || availablePoints < 0) return false;
            lock (_gate)
            {
                if (_reservations.ContainsKey(transactionIdentity)) return false;
                int reserved;
                _reservedByOwner.TryGetValue(owner, out reserved);
                if (reserved > availablePoints ||
                    cost > availablePoints - reserved) return false;
                _reservations.Add(transactionIdentity, new Reservation {
                    Owner = owner, Cost = cost });
                _reservedByOwner[owner] = checked(reserved + cost);
                return true;
            }
        }

        internal bool TryCommit(TOwner owner, string transactionIdentity,
            Func<int, bool> tryDebitExactly)
        {
            if (owner == null || string.IsNullOrWhiteSpace(transactionIdentity) ||
                tryDebitExactly == null) return false;
            lock (_gate)
            {
                Reservation reservation;
                if (!_reservations.TryGetValue(transactionIdentity,
                        out reservation) ||
                    !ReferenceEquals(reservation.Owner, owner)) return false;
                try
                {
                    return tryDebitExactly(reservation.Cost);
                }
                finally
                {
                    RemoveLocked(transactionIdentity, reservation);
                }
            }
        }

        internal bool Release(TOwner owner, string transactionIdentity)
        {
            if (owner == null || string.IsNullOrWhiteSpace(transactionIdentity))
                return false;
            lock (_gate)
            {
                Reservation reservation;
                if (!_reservations.TryGetValue(transactionIdentity,
                        out reservation) ||
                    !ReferenceEquals(reservation.Owner, owner)) return false;
                RemoveLocked(transactionIdentity, reservation);
                return true;
            }
        }

        private void RemoveLocked(string transactionIdentity,
            Reservation reservation)
        {
            _reservations.Remove(transactionIdentity);
            int reserved = _reservedByOwner[reservation.Owner] -
                reservation.Cost;
            if (reserved == 0) _reservedByOwner.Remove(reservation.Owner);
            else _reservedByOwner[reservation.Owner] = reserved;
        }

        internal int ReservedPoints(TOwner owner)
        {
            if (owner == null) return 0;
            lock (_gate)
            {
                int reserved;
                return _reservedByOwner.TryGetValue(owner, out reserved) ?
                    reserved : 0;
            }
        }

        internal void Clear()
        {
            lock (_gate)
            {
                _reservations.Clear();
                _reservedByOwner.Clear();
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<TOwner>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();
            public bool Equals(TOwner left, TOwner right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(TOwner value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
