using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Save-owned persistence carrier anchored to the player character. Sprint 14
    /// primary records contain only Kingmaker-issued item identities and primitive
    /// firearm-state data. Sprint 13 direct item references remain solely as a
    /// one-way migration source and are never silently pruned.
    /// </summary>
    public sealed class UnitPartFirearmStateVault : UnitPart
    {
        internal const int CurrentLegacyRecordSchemaVersion = 1;
        internal const int CurrentIdentityRecordSchemaVersion = 1;

        // Compatibility alias retained for the serialized Sprint 13 record class.
        internal const int CurrentRecordSchemaVersion = CurrentLegacyRecordSchemaVersion;

        [JsonProperty]
        private List<FirearmStateVaultRecord> _records =
            new List<FirearmStateVaultRecord>();

        [JsonProperty]
        private List<FirearmStateIdentityVaultRecord> _identityRecords =
            new List<FirearmStateIdentityVaultRecord>();

        internal int RecordCount
        {
            get { return IdentityRecordCount; }
        }

        internal int IdentityRecordCount
        {
            get
            {
                lock (this)
                {
                    EnsureCollections();
                    ValidateNoDuplicateIdentities(_identityRecords);
                    return _identityRecords.Count;
                }
            }
        }

        internal int LegacyRecordCount
        {
            get
            {
                lock (this)
                {
                    EnsureCollections();
                    return _records.Count;
                }
            }
        }

        internal bool TryRead(
            FirearmItemId itemId,
            out FirearmStateData data)
        {
            RequireIdentity(itemId);
            lock (this)
            {
                EnsureCollections();
                FirearmStateIdentityVaultRecord record =
                    FindSingleIdentityRecord(_identityRecords, itemId);
                if (record == null)
                {
                    data = null;
                    return false;
                }

                ValidateIdentityRecord(record);
                data = FirearmStateDataUtility.Clone(record.State);
                return true;
            }
        }

        internal void Replace(
            FirearmItemId itemId,
            FirearmStateData expectedData,
            FirearmStateData targetData)
        {
            RequireIdentity(itemId);
            FirearmStateData expectedCopy = FirearmStateDataUtility.Clone(expectedData);
            FirearmStateData targetCopy = FirearmStateDataUtility.Clone(targetData);
            ValidateDataOrNull(expectedCopy);
            ValidateDataOrNull(targetCopy);

            lock (this)
            {
                EnsureCollections();
                List<FirearmStateIdentityVaultRecord> before =
                    CloneIdentityRecords(_identityRecords);
                try
                {
                    FirearmStateIdentityVaultRecord current =
                        FindSingleIdentityRecord(_identityRecords, itemId);
                    FirearmStateData currentData = current == null
                        ? null
                        : FirearmStateDataUtility.Clone(current.State);
                    if (!FirearmStateDataUtility.AreEqual(currentData, expectedCopy))
                    {
                        throw new InvalidOperationException(
                            "The engine-identity firearm-state vault changed before replacement.");
                    }

                    if (targetCopy == null)
                    {
                        if (current != null)
                        {
                            _identityRecords.Remove(current);
                        }
                    }
                    else if (current == null)
                    {
                        _identityRecords.Add(
                            new FirearmStateIdentityVaultRecord(itemId, targetCopy));
                    }
                    else
                    {
                        current.ReplaceState(targetCopy);
                    }

                    FirearmStateIdentityVaultRecord verified =
                        FindSingleIdentityRecord(_identityRecords, itemId);
                    FirearmStateData verifiedData = verified == null
                        ? null
                        : verified.State;
                    if (!FirearmStateDataUtility.AreEqual(verifiedData, targetCopy))
                    {
                        throw new InvalidOperationException(
                            "The engine-identity firearm-state vault replacement did not verify.");
                    }

                    ValidateNoDuplicateIdentities(_identityRecords);
                }
                catch
                {
                    _identityRecords = before;
                    throw;
                }
            }
        }

        internal bool Remove(FirearmItemId itemId)
        {
            RequireIdentity(itemId);
            lock (this)
            {
                EnsureCollections();
                FirearmStateIdentityVaultRecord current =
                    FindSingleIdentityRecord(_identityRecords, itemId);
                if (current == null)
                {
                    return false;
                }

                bool removed = _identityRecords.Remove(current);
                if (!removed ||
                    FindSingleIdentityRecord(_identityRecords, itemId) != null)
                {
                    throw new InvalidOperationException(
                        "The engine-identity firearm-state vault failed to remove an item record.");
                }

                return true;
            }
        }

        /// <summary>
        /// Converts all safely resolvable Sprint 13 direct-reference records into
        /// primitive identity records as one transaction. Null, missing, or
        /// unsupported identities remain untouched. Any state conflict preserves the
        /// entire pre-migration carrier set and fails closed.
        /// </summary>
        internal FirearmStateIdentityMigrationSnapshot MigrateLegacyRecords(
            IFirearmItemIdentityProvider identityProvider)
        {
            if (identityProvider == null)
            {
                throw new ArgumentNullException("identityProvider");
            }

            lock (this)
            {
                EnsureCollections();
                if (_records.Count == 0)
                {
                    return EmptyMigrationSnapshot();
                }

                List<FirearmStateVaultRecord> beforeLegacy = CloneLegacyRecords(_records);
                List<FirearmStateIdentityVaultRecord> beforeIdentity =
                    CloneIdentityRecords(_identityRecords);
                List<FirearmStateVaultRecord> remainingLegacy =
                    new List<FirearmStateVaultRecord>();
                List<FirearmStateIdentityVaultRecord> targetIdentity =
                    CloneIdentityRecords(_identityRecords);
                var observedLegacyIdentities =
                    new HashSet<string>(StringComparer.Ordinal);
                long observed = 0;
                long migrated = 0;
                long redundant = 0;
                long unresolved = 0;

                try
                {
                    ValidateNoDuplicateIdentities(targetIdentity);
                    foreach (FirearmStateVaultRecord legacy in _records)
                    {
                        observed++;
                        if (legacy == null)
                        {
                            unresolved++;
                            remainingLegacy.Add(null);
                            continue;
                        }

                        ValidateLegacyRecordPayload(legacy);
                        if (legacy.Item == null)
                        {
                            unresolved++;
                            remainingLegacy.Add(legacy.Clone());
                            continue;
                        }

                        FirearmItemId itemId;
                        string reason;
                        if (!identityProvider.TryGetIdentity(
                            legacy.Item,
                            out itemId,
                            out reason) ||
                            itemId == null)
                        {
                            unresolved++;
                            remainingLegacy.Add(legacy.Clone());
                            continue;
                        }

                        if (!observedLegacyIdentities.Add(itemId.Value))
                        {
                            throw new InvalidOperationException(
                                "Multiple Sprint 13 direct-reference records resolved to the same engine item identity. Migration was refused to prevent two physical items from being merged.");
                        }

                        FirearmStateIdentityVaultRecord existing =
                            FindSingleIdentityRecord(targetIdentity, itemId);
                        if (existing == null)
                        {
                            targetIdentity.Add(
                                new FirearmStateIdentityVaultRecord(
                                    itemId,
                                    legacy.State));
                            migrated++;
                            continue;
                        }

                        ValidateIdentityRecord(existing);
                        if (FirearmStateDataUtility.AreEqual(
                            existing.State,
                            legacy.State))
                        {
                            redundant++;
                            continue;
                        }

                        throw new FirearmStateIdentityMigrationConflictException(itemId);
                    }

                    ValidateNoDuplicateIdentities(targetIdentity);
                    _identityRecords = targetIdentity;
                    _records = remainingLegacy;
                    ValidateNoDuplicateIdentities(_identityRecords);
                    if (_records.Count != unresolved)
                    {
                        throw new InvalidOperationException(
                            "The direct-reference migration did not preserve exactly the unresolved legacy records.");
                    }

                    return new FirearmStateIdentityMigrationSnapshot(
                        observed,
                        migrated,
                        redundant,
                        unresolved,
                        0,
                        0,
                        0);
                }
                catch
                {
                    _records = beforeLegacy;
                    _identityRecords = beforeIdentity;
                    throw;
                }
            }
        }

        internal void AddLegacyRecordForDebug(
            ItemEntityWeapon item,
            FirearmStateData state)
        {
            RequireLegacyItem(item);
            FirearmStateData copy = FirearmStateDataUtility.Clone(state) ??
                throw new ArgumentNullException("state");
            ValidateDataOrNull(copy);

            lock (this)
            {
                EnsureCollections();
                if (_records.Any(record =>
                    record != null && ReferenceEquals(record.Item, item)))
                {
                    throw new InvalidOperationException(
                        "A Sprint 13 direct-reference record already exists for the exact item object.");
                }

                _records.Add(new FirearmStateVaultRecord(item, copy));
            }
        }

        private static FirearmStateIdentityVaultRecord FindSingleIdentityRecord(
            IEnumerable<FirearmStateIdentityVaultRecord> records,
            FirearmItemId itemId)
        {
            FirearmStateIdentityVaultRecord[] matches = records
                .Where(record =>
                    record != null &&
                    new FirearmItemId(record.ItemId) == itemId)
                .ToArray();
            if (matches.Length > 1)
            {
                throw new InvalidOperationException(
                    "The engine-identity firearm-state vault contains duplicate records for one item identity.");
            }

            return matches.Length == 0 ? null : matches[0];
        }

        private static void ValidateNoDuplicateIdentities(
            IList<FirearmStateIdentityVaultRecord> records)
        {
            var identities = new HashSet<string>(StringComparer.Ordinal);
            foreach (FirearmStateIdentityVaultRecord record in records)
            {
                ValidateIdentityRecord(record);
                string canonical = new FirearmItemId(record.ItemId).Value;
                if (!identities.Add(canonical))
                {
                    throw new InvalidOperationException(
                        "The engine-identity firearm-state vault contains duplicate item identities.");
                }
            }
        }

        private static void ValidateIdentityRecord(
            FirearmStateIdentityVaultRecord record)
        {
            if (record == null)
            {
                throw new InvalidOperationException(
                    "The engine-identity firearm-state vault contains a null record.");
            }

            if (record.RecordSchemaVersion != CurrentIdentityRecordSchemaVersion)
            {
                throw new NotSupportedException(
                    "The engine-identity firearm-state vault contains an unsupported record schema.");
            }

            new FirearmItemId(record.ItemId);
            if (record.State == null)
            {
                throw new InvalidOperationException(
                    "The engine-identity firearm-state vault contains a record without state data.");
            }

            ValidateDataOrNull(record.State);
        }

        private static void ValidateLegacyRecordPayload(
            FirearmStateVaultRecord record)
        {
            if (record.RecordSchemaVersion != CurrentLegacyRecordSchemaVersion)
            {
                throw new NotSupportedException(
                    "The Sprint 13 direct-reference vault contains an unsupported record schema.");
            }

            if (record.State == null)
            {
                throw new InvalidOperationException(
                    "The Sprint 13 direct-reference vault contains a record without state data.");
            }

            ValidateDataOrNull(record.State);
        }

        private static void ValidateDataOrNull(FirearmStateData data)
        {
            if (data != null && data.SchemaVersion <= 0)
            {
                throw new InvalidOperationException(
                    "A firearm-state vault record requires a positive state schema version.");
            }
        }

        private static List<FirearmStateVaultRecord> CloneLegacyRecords(
            IEnumerable<FirearmStateVaultRecord> records)
        {
            return records
                .Select(record => record == null ? null : record.Clone())
                .ToList();
        }

        private static List<FirearmStateIdentityVaultRecord> CloneIdentityRecords(
            IEnumerable<FirearmStateIdentityVaultRecord> records)
        {
            return records
                .Select(record => record == null ? null : record.Clone())
                .ToList();
        }

        private void EnsureCollections()
        {
            if (_records == null)
            {
                _records = new List<FirearmStateVaultRecord>();
            }

            if (_identityRecords == null)
            {
                _identityRecords = new List<FirearmStateIdentityVaultRecord>();
            }
        }

        private static FirearmStateIdentityMigrationSnapshot EmptyMigrationSnapshot()
        {
            return new FirearmStateIdentityMigrationSnapshot(0, 0, 0, 0, 0, 0, 0);
        }

        private static void RequireIdentity(FirearmItemId itemId)
        {
            if (itemId == null)
            {
                throw new ArgumentNullException("itemId");
            }
        }

        private static void RequireLegacyItem(ItemEntityWeapon item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
        }
    }

    /// <summary>
    /// Legacy Sprint 13 serializer record. It remains in the save schema only as a
    /// migration source. New writes never create this record except through the
    /// explicit development fixture.
    /// </summary>
    public sealed class FirearmStateVaultRecord
    {
        public FirearmStateVaultRecord()
        {
        }

        internal FirearmStateVaultRecord(
            ItemEntityWeapon item,
            FirearmStateData state)
        {
            Item = item ?? throw new ArgumentNullException("item");
            ReplaceState(state);
        }

        [JsonProperty]
        public int RecordSchemaVersion { get; set; } =
            UnitPartFirearmStateVault.CurrentLegacyRecordSchemaVersion;

        [JsonProperty]
        public ItemEntityWeapon Item { get; set; }

        [JsonProperty]
        public FirearmStateData State { get; set; }

        internal void ReplaceState(FirearmStateData state)
        {
            State = FirearmStateDataUtility.Clone(state) ??
                throw new ArgumentNullException("state");
            RecordSchemaVersion =
                UnitPartFirearmStateVault.CurrentLegacyRecordSchemaVersion;
        }

        internal FirearmStateVaultRecord Clone()
        {
            return new FirearmStateVaultRecord
            {
                RecordSchemaVersion = RecordSchemaVersion,
                Item = Item,
                State = FirearmStateDataUtility.Clone(State)
            };
        }
    }

    /// <summary>
    /// Sprint 14 serializer record. Only a canonical primitive item GUID and a
    /// defensive copy of primitive firearm-state data enter the save graph.
    /// </summary>
    public sealed class FirearmStateIdentityVaultRecord
    {
        public FirearmStateIdentityVaultRecord()
        {
        }

        internal FirearmStateIdentityVaultRecord(
            FirearmItemId itemId,
            FirearmStateData state)
        {
            if (itemId == null)
            {
                throw new ArgumentNullException("itemId");
            }

            ItemId = itemId.Value;
            ReplaceState(state);
        }

        [JsonProperty]
        public int RecordSchemaVersion { get; set; } =
            UnitPartFirearmStateVault.CurrentIdentityRecordSchemaVersion;

        [JsonProperty]
        public string ItemId { get; set; }

        [JsonProperty]
        public FirearmStateData State { get; set; }

        internal void ReplaceState(FirearmStateData state)
        {
            State = FirearmStateDataUtility.Clone(state) ??
                throw new ArgumentNullException("state");
            RecordSchemaVersion =
                UnitPartFirearmStateVault.CurrentIdentityRecordSchemaVersion;
        }

        internal FirearmStateIdentityVaultRecord Clone()
        {
            return new FirearmStateIdentityVaultRecord
            {
                RecordSchemaVersion = RecordSchemaVersion,
                ItemId = ItemId,
                State = FirearmStateDataUtility.Clone(State)
            };
        }
    }
}
