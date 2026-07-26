using System;
using System.Threading;
using Kingmaker.Items;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Kingmaker adapter over the Sprint 14 identity-record collection in
    /// UnitPartFirearmStateVault. Before every read or write it attempts one-way
    /// migration of any resolvable Sprint 13 direct-reference records.
    /// </summary>
    internal sealed class KingmakerFirearmStateVaultStore : IFirearmStateIdentityRecordStore
    {
        private readonly KingmakerFirearmStateVaultPartProvider _provider;
        private readonly IFirearmItemIdentityProvider _identityProvider;
        private long _observedLegacyRecordCount;
        private long _migratedRecordCount;
        private long _redundantRecordCleanupCount;
        private long _unresolvedRecordCount;
        private long _conflictCount;
        private long _failureCount;
        private long _rollbackFailureCount;

        internal KingmakerFirearmStateVaultStore(
            KingmakerFirearmStateVaultPartProvider provider,
            IFirearmItemIdentityProvider identityProvider)
        {
            _provider = provider ?? throw new ArgumentNullException("provider");
            _identityProvider = identityProvider ??
                throw new ArgumentNullException("identityProvider");
        }

        public int RecordCount
        {
            get
            {
                UnitPartFirearmStateVault vault;
                if (!_provider.TryGetExisting(out vault))
                {
                    return 0;
                }

                EnsureLegacyMigrated(vault);
                return vault.IdentityRecordCount;
            }
        }

        internal int LegacyRecordCount
        {
            get
            {
                UnitPartFirearmStateVault vault;
                if (!_provider.TryGetExisting(out vault))
                {
                    return 0;
                }

                EnsureLegacyMigrated(vault);
                return vault.LegacyRecordCount;
            }
        }

        internal FirearmStateIdentityMigrationSnapshot MigrationSnapshot
        {
            get
            {
                return new FirearmStateIdentityMigrationSnapshot(
                    Interlocked.Read(ref _observedLegacyRecordCount),
                    Interlocked.Read(ref _migratedRecordCount),
                    Interlocked.Read(ref _redundantRecordCleanupCount),
                    Interlocked.Read(ref _unresolvedRecordCount),
                    Interlocked.Read(ref _conflictCount),
                    Interlocked.Read(ref _failureCount),
                    Interlocked.Read(ref _rollbackFailureCount));
            }
        }

        public bool TryRead(FirearmItemId itemId, out FirearmStateData data)
        {
            RequireIdentity(itemId);
            UnitPartFirearmStateVault vault;
            if (!_provider.TryGetExisting(out vault))
            {
                data = null;
                return false;
            }

            EnsureLegacyMigrated(vault);
            return vault.TryRead(itemId, out data);
        }

        public void Replace(
            FirearmItemId itemId,
            FirearmStateData expectedData,
            FirearmStateData targetData)
        {
            RequireIdentity(itemId);
            UnitPartFirearmStateVault vault = targetData == null
                ? RequireExistingOrValidateAbsent(expectedData)
                : _provider.RequireForWrite();
            if (vault == null)
            {
                return;
            }

            EnsureLegacyMigrated(vault);
            vault.Replace(itemId, expectedData, targetData);
        }

        public bool Remove(FirearmItemId itemId)
        {
            RequireIdentity(itemId);
            UnitPartFirearmStateVault vault;
            if (!_provider.TryGetExisting(out vault))
            {
                return false;
            }

            EnsureLegacyMigrated(vault);
            return vault.Remove(itemId);
        }

        internal void SeedLegacyRecordForDebug(
            object itemInstance,
            FirearmStateData state)
        {
            ItemEntityWeapon item = RequireWeapon(itemInstance);
            FirearmItemId itemId = RequireItemIdentity(item);
            UnitPartFirearmStateVault vault = _provider.RequireForWrite();
            EnsureLegacyMigrated(vault);

            FirearmStateData existing;
            if (vault.TryRead(itemId, out existing))
            {
                throw new InvalidOperationException(
                    "A Sprint 14 engine-identity record already exists for this firearm; a Sprint 13 migration fixture would create an artificial conflict.");
            }

            vault.AddLegacyRecordForDebug(item, state);
        }

        private void EnsureLegacyMigrated(UnitPartFirearmStateVault vault)
        {
            if (vault == null || vault.LegacyRecordCount == 0)
            {
                return;
            }

            try
            {
                FirearmStateIdentityMigrationSnapshot delta =
                    vault.MigrateLegacyRecords(_identityProvider);
                Add(ref _observedLegacyRecordCount, delta.ObservedLegacyRecords);
                Add(ref _migratedRecordCount, delta.MigratedRecords);
                Add(
                    ref _redundantRecordCleanupCount,
                    delta.RedundantRecordsRemoved);
                Add(
                    ref _unresolvedRecordCount,
                    delta.UnresolvedRecordsPreserved);
            }
            catch (FirearmStateIdentityMigrationConflictException)
            {
                Interlocked.Increment(ref _conflictCount);
                Interlocked.Increment(ref _failureCount);
                throw;
            }
            catch
            {
                Interlocked.Increment(ref _failureCount);
                throw;
            }
        }

        private UnitPartFirearmStateVault RequireExistingOrValidateAbsent(
            FirearmStateData expectedData)
        {
            UnitPartFirearmStateVault vault;
            if (_provider.TryGetExisting(out vault))
            {
                return vault;
            }

            if (expectedData != null)
            {
                throw new InvalidOperationException(
                    "The engine-identity firearm-state vault disappeared before record removal.");
            }

            return null;
        }

        private FirearmItemId RequireItemIdentity(ItemEntityWeapon item)
        {
            FirearmItemId itemId;
            string reason;
            if (!_identityProvider.TryGetIdentity(item, out itemId, out reason) ||
                itemId == null)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(reason)
                        ? "Kingmaker exposed no usable identity for the firearm."
                        : reason);
            }

            return itemId;
        }

        private static ItemEntityWeapon RequireWeapon(object itemInstance)
        {
            if (itemInstance == null)
            {
                throw new ArgumentNullException("itemInstance");
            }

            ItemEntityWeapon weapon = itemInstance as ItemEntityWeapon;
            if (weapon == null)
            {
                throw new ArgumentException(
                    "The legacy migration fixture accepts only ItemEntityWeapon instances.",
                    "itemInstance");
            }

            return weapon;
        }

        private static void RequireIdentity(FirearmItemId itemId)
        {
            if (itemId == null)
            {
                throw new ArgumentNullException("itemId");
            }
        }

        private static void Add(ref long target, long value)
        {
            if (value != 0)
            {
                Interlocked.Add(ref target, value);
            }
        }
    }
}
