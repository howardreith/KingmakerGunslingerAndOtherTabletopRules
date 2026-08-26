using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Gunsmithing
{
    public sealed class UnitPartBatteredFirearmOwnership : UnitPart
    {
        internal const int CurrentSchemaVersion = 1;

        [JsonProperty]
        private List<BatteredFirearmOwnershipData> _records =
            new List<BatteredFirearmOwnershipData>();

        // Kingmaker 2.1.7b ItemEntity has no persistent item identifier.  Keep
        // the first-level starter receipt independently of the legacy
        // item-identity ledger rather than fabricating an item identity.
        [JsonProperty]
        private List<string> _starterReceipts = new List<string>();

        internal int Count
        {
            get
            {
                lock (this)
                {
                    EnsureValid();
                    return _records.Count + _starterReceipts.Count;
                }
            }
        }

        internal bool Bind(FirearmItemId itemId, OriginatingUnitId ownerId)
        {
            if (itemId == null) throw new ArgumentNullException("itemId");
            if (ownerId == null) throw new ArgumentNullException("ownerId");
            lock (this)
            {
                EnsureValid();
                BatteredFirearmOwnershipData existing = Find(itemId);
                if (existing != null)
                {
                    if (string.Equals(existing.OwnerId, ownerId.Value,
                        StringComparison.Ordinal)) return false;
                    throw new InvalidOperationException(
                        "A persisted battered firearm cannot be rebound to another originating unit.");
                }
                var target = new List<BatteredFirearmOwnershipData>(_records)
                {
                    new BatteredFirearmOwnershipData(itemId, ownerId)
                };
                Validate(target);
                _records = target;
                return true;
            }
        }

        internal bool TryGetOwner(FirearmItemId itemId,
            out OriginatingUnitId ownerId)
        {
            if (itemId == null) throw new ArgumentNullException("itemId");
            lock (this)
            {
                EnsureValid();
                BatteredFirearmOwnershipData existing = Find(itemId);
                ownerId = existing == null ? null :
                    new OriginatingUnitId(existing.OwnerId);
                return existing != null;
            }
        }

        internal bool HasReceipt(OriginatingUnitId ownerId)
        {
            if (ownerId == null) throw new ArgumentNullException("ownerId");
            lock (this)
            {
                EnsureValid();
                return _starterReceipts.Contains(ownerId.Value,
                        StringComparer.Ordinal) ||
                    _records.Any(record => string.Equals(record.OwnerId,
                        ownerId.Value, StringComparison.Ordinal));
            }
        }

        internal bool AddStarterReceipt(OriginatingUnitId ownerId)
        {
            if (ownerId == null) throw new ArgumentNullException("ownerId");
            lock (this)
            {
                EnsureValid();
                if (HasReceipt(ownerId)) return false;
                var target = new List<string>(_starterReceipts)
                {
                    ownerId.Value
                };
                ValidateReceipts(target);
                _starterReceipts = target;
                return true;
            }
        }

        internal bool RemoveStarterReceipt(OriginatingUnitId ownerId)
        {
            if (ownerId == null) throw new ArgumentNullException("ownerId");
            lock (this)
            {
                EnsureValid();
                int index = _starterReceipts.FindIndex(value => string.Equals(
                    value, ownerId.Value, StringComparison.Ordinal));
                if (index < 0) return false;
                var target = new List<string>(_starterReceipts);
                target.RemoveAt(index);
                ValidateReceipts(target);
                _starterReceipts = target;
                return true;
            }
        }

        internal bool Remove(FirearmItemId itemId, OriginatingUnitId ownerId)
        {
            if (itemId == null) throw new ArgumentNullException("itemId");
            if (ownerId == null) throw new ArgumentNullException("ownerId");
            lock (this)
            {
                EnsureValid();
                BatteredFirearmOwnershipData existing = Find(itemId);
                if (existing == null) return false;
                if (!string.Equals(existing.OwnerId, ownerId.Value,
                    StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "Persisted ownership removal requires the exact originating unit.");
                var target = _records.Where(record => !ReferenceEquals(
                    record, existing)).ToList();
                Validate(target);
                _records = target;
                return true;
            }
        }

        private BatteredFirearmOwnershipData Find(FirearmItemId itemId)
        {
            return _records.SingleOrDefault(record => string.Equals(
                record.ItemId, itemId.Value, StringComparison.Ordinal));
        }

        private void EnsureValid()
        {
            if (_records == null) _records = new List<BatteredFirearmOwnershipData>();
            if (_starterReceipts == null) _starterReceipts = new List<string>();
            Validate(_records);
            ValidateReceipts(_starterReceipts);
        }

        private static void ValidateReceipts(IList<string> receipts)
        {
            var owners = new HashSet<string>(StringComparer.Ordinal);
            foreach (string receipt in receipts)
            {
                string owner = new OriginatingUnitId(receipt).Value;
                if (!owners.Add(owner))
                    throw new InvalidOperationException(
                        "The battered firearm ownership carrier contains duplicate starter receipts.");
            }
        }

        private static void Validate(IList<BatteredFirearmOwnershipData> records)
        {
            var items = new HashSet<string>(StringComparer.Ordinal);
            foreach (BatteredFirearmOwnershipData record in records)
            {
                if (record == null || record.SchemaVersion != CurrentSchemaVersion)
                    throw new NotSupportedException(
                        "The battered firearm ownership carrier contains a null or unsupported record.");
                string item = new FirearmItemId(record.ItemId).Value;
                new OriginatingUnitId(record.OwnerId);
                if (!items.Add(item))
                    throw new InvalidOperationException(
                        "The battered firearm ownership carrier contains duplicate item identities.");
            }
        }
    }

    public sealed class BatteredFirearmOwnershipData
    {
        public BatteredFirearmOwnershipData() { }

        internal BatteredFirearmOwnershipData(FirearmItemId itemId,
            OriginatingUnitId ownerId)
        {
            ItemId = itemId.Value;
            OwnerId = ownerId.Value;
            SchemaVersion = UnitPartBatteredFirearmOwnership.CurrentSchemaVersion;
        }

        [JsonProperty] public int SchemaVersion { get; set; } =
            UnitPartBatteredFirearmOwnership.CurrentSchemaVersion;
        [JsonProperty] public string ItemId { get; set; }
        [JsonProperty] public string OwnerId { get; set; }
    }
}
