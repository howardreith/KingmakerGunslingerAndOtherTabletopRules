using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class BatteredFirearmOwnershipLedger
    {
        private readonly List<BatteredFirearmOwnershipRecord> _records;

        internal BatteredFirearmOwnershipLedger()
        {
            _records = new List<BatteredFirearmOwnershipRecord>();
        }

        internal int Count { get { return _records.Count; } }

        internal bool Bind(FirearmItemId itemId, OriginatingUnitId ownerId)
        {
            Require(itemId, ownerId);
            BatteredFirearmOwnershipRecord existing = Find(itemId);
            if (existing == null)
            {
                _records.Add(new BatteredFirearmOwnershipRecord(itemId, ownerId));
                return true;
            }
            if (string.Equals(existing.OwnerId, ownerId.Value, StringComparison.Ordinal))
                return false;
            throw new InvalidOperationException(
                "A battered firearm cannot be rebound to a different originating unit.");
        }

        internal bool TryGetOwner(FirearmItemId itemId, out OriginatingUnitId ownerId)
        {
            if (itemId == null) throw new ArgumentNullException("itemId");
            BatteredFirearmOwnershipRecord existing = Find(itemId);
            ownerId = existing == null ? null : new OriginatingUnitId(existing.OwnerId);
            return existing != null;
        }

        internal BatteredFirearmOwnershipRecord[] Snapshot()
        {
            return _records.Select(record => record.Clone()).ToArray();
        }

        private BatteredFirearmOwnershipRecord Find(FirearmItemId itemId)
        {
            BatteredFirearmOwnershipRecord[] matches = _records.Where(record =>
                string.Equals(record.ItemId, itemId.Value, StringComparison.Ordinal)).ToArray();
            if (matches.Length > 1)
                throw new InvalidOperationException("The battered firearm ownership ledger contains duplicate item identities.");
            return matches.Length == 0 ? null : matches[0];
        }

        private static void Require(FirearmItemId itemId, OriginatingUnitId ownerId)
        {
            if (itemId == null) throw new ArgumentNullException("itemId");
            if (ownerId == null) throw new ArgumentNullException("ownerId");
        }
    }
}
