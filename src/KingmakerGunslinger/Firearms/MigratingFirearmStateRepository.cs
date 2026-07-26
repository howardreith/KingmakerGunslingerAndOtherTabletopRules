using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Fronts the Sprint 14 engine-identity vault repository and performs one-way, fail-closed
    /// migration from the four stable Sprint 12 item-enchantment state tokens.
    /// New state is written only to the vault; token GUIDs remain registered solely
    /// so older saves can be decoded and cleaned up.
    /// </summary>
    internal sealed class MigratingFirearmStateRepository : IFirearmStateRepository
    {
        private readonly object _gateTableLock = new object();
        private readonly ConditionalWeakTable<object, ItemGate> _itemGates =
            new ConditionalWeakTable<object, ItemGate>();
        private readonly VaultBackedFirearmStateRepository _vaultRepository;
        private readonly IFirearmStateVaultStore _vaultStore;
        private readonly IFirearmStateTokenStore _legacyTokenStore;
        private readonly FirearmStateTokenCatalog _legacyCatalog;
        private long _observedLegacyTokenCount;
        private long _migratedItemCount;
        private long _redundantTokenCleanupCount;
        private long _conflictCount;
        private long _failureCount;
        private long _rollbackFailureCount;

        internal MigratingFirearmStateRepository(
            VaultBackedFirearmStateRepository vaultRepository,
            IFirearmStateVaultStore vaultStore,
            IFirearmStateTokenStore legacyTokenStore,
            FirearmStateTokenCatalog legacyCatalog)
        {
            _vaultRepository = vaultRepository ??
                throw new ArgumentNullException("vaultRepository");
            _vaultStore = vaultStore ?? throw new ArgumentNullException("vaultStore");
            _legacyTokenStore = legacyTokenStore ??
                throw new ArgumentNullException("legacyTokenStore");
            _legacyCatalog = legacyCatalog ?? throw new ArgumentNullException("legacyCatalog");
        }

        public long CreatedEntryCount
        {
            get { return _vaultRepository.CreatedEntryCount; }
        }

        public long MutationCount
        {
            get { return _vaultRepository.MutationCount; }
        }

        public long RemovalCount
        {
            get { return _vaultRepository.RemovalCount; }
        }

        internal FirearmStateMigrationSnapshot MigrationSnapshot
        {
            get
            {
                return new FirearmStateMigrationSnapshot(
                    Interlocked.Read(ref _observedLegacyTokenCount),
                    Interlocked.Read(ref _migratedItemCount),
                    Interlocked.Read(ref _redundantTokenCleanupCount),
                    Interlocked.Read(ref _conflictCount),
                    Interlocked.Read(ref _failureCount),
                    Interlocked.Read(ref _rollbackFailureCount));
            }
        }

        internal int PersistedRecordCount
        {
            get { return _vaultStore.RecordCount; }
        }

        public FirearmStateRepositorySnapshot GetOrCreate(object itemInstance)
        {
            lock (GetItemGate(itemInstance).Gate)
            {
                EnsureMigrated(itemInstance);
                return _vaultRepository.GetOrCreate(itemInstance);
            }
        }

        public bool TryGet(
            object itemInstance,
            out FirearmStateRepositorySnapshot snapshot)
        {
            lock (GetItemGate(itemInstance).Gate)
            {
                EnsureMigrated(itemInstance);
                return _vaultRepository.TryGet(itemInstance, out snapshot);
            }
        }

        public FirearmStateRepositorySnapshot Set(
            object itemInstance,
            FirearmState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            lock (GetItemGate(itemInstance).Gate)
            {
                EnsureMigrated(itemInstance);
                return _vaultRepository.Set(itemInstance, state);
            }
        }

        public FirearmStateRepositorySnapshot Transition(
            object itemInstance,
            Func<FirearmState, FirearmState> transition)
        {
            if (transition == null)
            {
                throw new ArgumentNullException("transition");
            }

            lock (GetItemGate(itemInstance).Gate)
            {
                EnsureMigrated(itemInstance);
                return _vaultRepository.Transition(itemInstance, transition);
            }
        }

        public bool Remove(object itemInstance)
        {
            lock (GetItemGate(itemInstance).Gate)
            {
                EnsureMigrated(itemInstance);
                bool removedVault = _vaultRepository.Remove(itemInstance);
                bool removedLegacy = _legacyTokenStore.ClearTokens(itemInstance);
                return removedVault || removedLegacy;
            }
        }

        internal bool HasPersistedVaultRecord(object itemInstance)
        {
            RequireReferenceKey(itemInstance);
            FirearmStateData ignored;
            return _vaultStore.TryRead(itemInstance, out ignored);
        }

        internal IReadOnlyList<string> ReadLegacyTokenIds(object itemInstance)
        {
            RequireReferenceKey(itemInstance);
            return _legacyTokenStore.ReadTokenIds(itemInstance);
        }

        private void EnsureMigrated(object itemInstance)
        {
            IReadOnlyList<string> legacyIds = _legacyTokenStore.ReadTokenIds(itemInstance);
            if (legacyIds.Count == 0)
            {
                return;
            }

            Interlocked.Increment(ref _observedLegacyTokenCount);
            FirearmState legacyState;
            try
            {
                legacyState = _legacyCatalog.Decode(legacyIds);
            }
            catch
            {
                Interlocked.Increment(ref _failureCount);
                throw;
            }

            FirearmStateData persistedData;
            bool hasPersistedVault = _vaultStore.TryRead(
                itemInstance,
                out persistedData);
            if (hasPersistedVault)
            {
                FirearmState persistedState;
                try
                {
                    persistedState = FirearmStateCodec.FromData(
                        persistedData,
                        CreateDiagnosticRules());
                }
                catch
                {
                    Interlocked.Increment(ref _failureCount);
                    throw;
                }

                if (persistedState != legacyState)
                {
                    Interlocked.Increment(ref _conflictCount);
                    throw new InvalidOperationException(
                        "The exact firearm has conflicting Sprint 12 token state and Sprint 14 identity-vault state. Both carriers were preserved for diagnosis.");
                }

                try
                {
                    _legacyTokenStore.ClearTokens(itemInstance);
                    if (_legacyTokenStore.ReadTokenIds(itemInstance).Count != 0)
                    {
                        throw new InvalidOperationException(
                            "A redundant legacy token remained after cleanup.");
                    }

                    Interlocked.Increment(ref _redundantTokenCleanupCount);
                    return;
                }
                catch
                {
                    Interlocked.Increment(ref _failureCount);
                    throw;
                }
            }

            bool wroteVault = false;
            try
            {
                _vaultRepository.Set(itemInstance, legacyState);
                wroteVault = true;

                FirearmStateData verifiedData;
                if (!_vaultStore.TryRead(itemInstance, out verifiedData))
                {
                    throw new InvalidOperationException(
                        "Legacy migration wrote no observable vault record.");
                }

                FirearmState verifiedState = FirearmStateCodec.FromData(
                    verifiedData,
                    CreateDiagnosticRules());
                if (verifiedState != legacyState)
                {
                    throw new InvalidOperationException(
                        "Legacy migration produced a vault state different from the token payload.");
                }

                _legacyTokenStore.ClearTokens(itemInstance);
                if (_legacyTokenStore.ReadTokenIds(itemInstance).Count != 0)
                {
                    throw new InvalidOperationException(
                        "The legacy token remained after migration.");
                }

                Interlocked.Increment(ref _migratedItemCount);
            }
            catch
            {
                Interlocked.Increment(ref _failureCount);
                if (wroteVault)
                {
                    try
                    {
                        _vaultRepository.Remove(itemInstance);
                    }
                    catch
                    {
                        Interlocked.Increment(ref _rollbackFailureCount);
                    }
                }

                throw;
            }
        }

        private ItemGate GetItemGate(object itemInstance)
        {
            RequireReferenceKey(itemInstance);
            lock (_gateTableLock)
            {
                ItemGate gate;
                if (_itemGates.TryGetValue(itemInstance, out gate))
                {
                    return gate;
                }

                gate = new ItemGate();
                _itemGates.Add(itemInstance, gate);
                return gate;
            }
        }

        private static FirearmStateRules CreateDiagnosticRules()
        {
            return new FirearmStateRules(
                1,
                new[] { FirearmStateTokenCatalog.DiagnosticLeadBall });
        }

        private static void RequireReferenceKey(object itemInstance)
        {
            if (itemInstance == null)
            {
                throw new ArgumentNullException("itemInstance");
            }

            if (itemInstance.GetType().IsValueType)
            {
                throw new ArgumentException(
                    "A firearm-state key must be a stable reference-type item instance.",
                    "itemInstance");
            }
        }

        private sealed class ItemGate
        {
            internal ItemGate()
            {
                Gate = new object();
            }

            internal object Gate { get; private set; }
        }
    }
}
