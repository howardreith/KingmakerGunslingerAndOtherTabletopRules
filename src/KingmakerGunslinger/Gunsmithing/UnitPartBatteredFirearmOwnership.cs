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

        internal int Count
        {
            get { lock (this) { EnsureValid(); return _records.Count; } }
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
            Validate(_records);
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
