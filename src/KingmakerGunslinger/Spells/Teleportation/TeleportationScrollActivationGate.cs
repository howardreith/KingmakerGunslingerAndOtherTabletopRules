using System;
using Kingmaker.Items;
using Kingmaker.EntitySystem.Entities;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // The strategic spell abilities' native caster checker authorizes exactly
    // one reader/item pair for exactly the duration of the native activation
    // call inside a confirmed contextual request. Ordinary inventory,
    // equipment or action-bar use keeps the checker closed: the native item
    // path refuses before any activation roll or consumption and the guidance
    // text tells the player to select a destination on the world map. The
    // authorization is request-bound (actual reader and item references), is
    // always released by its finally scope, refuses reentrancy, and leaves no
    // process-global permission behind.
    internal static class TeleportationScrollActivationGate
    {
        private sealed class OpenLease
        {
            internal UnitEntityData Reader;
            internal ItemEntity Item;
        }
        private static OpenLease _current;

        internal sealed class Authorization : IDisposable
        {
            private readonly OpenLease _lease;
            private bool _disposed;
            private Authorization(OpenLease lease) { _lease = lease; }
            internal static Authorization TryOpen(UnitEntityData reader, ItemEntity item)
            {
                if (reader == null || item == null || _current != null) return null;
                var lease = new OpenLease { Reader = reader, Item = item };
                _current = lease;
                return new Authorization(lease);
            }
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                if (ReferenceEquals(_current, _lease)) _current = null;
            }
        }

        internal static bool Authorized(UnitEntityData reader)
        {
            var lease = _current;
            return lease != null && reader != null && ReferenceEquals(lease.Reader, reader);
        }

        internal static bool Matches(UnitEntityData reader, ItemEntity item)
        {
            var lease = _current;
            return lease != null && reader != null && item != null &&
                ReferenceEquals(lease.Reader, reader) && ReferenceEquals(lease.Item, item);
        }
    }
}
