using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Repository whose durable source of truth is a save-owned persistence
    /// vault. Weak metadata supplies process-local revision and diagnostic identity
    /// without retaining item objects after the game releases them.
    /// </summary>
    internal sealed class VaultBackedFirearmStateRepository : IFirearmStateRepository
    {
        private readonly object _tableGate = new object();
        private readonly ConditionalWeakTable<object, Entry> _entries =
            new ConditionalWeakTable<object, Entry>();
        private readonly IFirearmStateVaultStore _store;
        private readonly FirearmStateRules _rules;
        private long _nextEntryId;
        private long _createdEntryCount;
        private long _mutationCount;
        private long _removalCount;

        internal VaultBackedFirearmStateRepository(
            IFirearmStateVaultStore store,
            FirearmStateRules rules)
        {
            _store = store ?? throw new ArgumentNullException("store");
            _rules = rules ?? throw new ArgumentNullException("rules");
        }

        public long CreatedEntryCount
        {
            get { return Interlocked.Read(ref _createdEntryCount); }
        }

        public long MutationCount
        {
            get { return Interlocked.Read(ref _mutationCount); }
        }

        public long RemovalCount
        {
            get { return Interlocked.Read(ref _removalCount); }
        }

        internal int PersistedRecordCount
        {
            get { return _store.RecordCount; }
        }

        public FirearmStateRepositorySnapshot GetOrCreate(object itemInstance)
        {
            Entry entry = GetOrCreateEntry(itemInstance);
            lock (entry.Gate)
            {
                return entry.Snapshot(ReadState(itemInstance, out _));
            }
        }

        public bool TryGet(
            object itemInstance,
            out FirearmStateRepositorySnapshot snapshot)
        {
            RequireReferenceKey(itemInstance);
            Entry entry;
            lock (_tableGate)
            {
                _entries.TryGetValue(itemInstance, out entry);
            }

            if (entry == null)
            {
                FirearmStateData persisted;
                if (!_store.TryRead(itemInstance, out persisted))
                {
                    snapshot = null;
                    return false;
                }

                entry = GetOrCreateEntry(itemInstance);
            }

            lock (entry.Gate)
            {
                FirearmState state = ReadState(itemInstance, out _);
                snapshot = entry.Snapshot(state);
                return true;
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

            Entry entry = GetOrCreateEntry(itemInstance);
            lock (entry.Gate)
            {
                FirearmStateData currentData;
                FirearmState current = ReadState(itemInstance, out currentData);
                if (current == state)
                {
                    return entry.Snapshot(current);
                }

                FirearmStateData targetData = EncodeOrAbsent(state);
                _store.Replace(itemInstance, currentData, targetData);
                FirearmState verified = ReadState(itemInstance, out _);
                if (verified != state)
                {
                    throw new InvalidOperationException(
                        "The save-owned firearm-state vault write did not verify after replacement.");
                }

                entry.IncrementRevision();
                Interlocked.Increment(ref _mutationCount);
                return entry.Snapshot(verified);
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

            Entry entry = GetOrCreateEntry(itemInstance);
            lock (entry.Gate)
            {
                FirearmStateData currentData;
                FirearmState current = ReadState(itemInstance, out currentData);
                FirearmState next = transition(current);
                if (next == null)
                {
                    throw new InvalidOperationException(
                        "A firearm-state transition returned null.");
                }

                if (next == current)
                {
                    return entry.Snapshot(current);
                }

                FirearmStateData targetData = EncodeOrAbsent(next);
                _store.Replace(itemInstance, currentData, targetData);
                FirearmState verified = ReadState(itemInstance, out _);
                if (verified != next)
                {
                    throw new InvalidOperationException(
                        "The save-owned firearm-state vault transition did not verify after replacement.");
                }

                entry.IncrementRevision();
                Interlocked.Increment(ref _mutationCount);
                return entry.Snapshot(verified);
            }
        }

        public bool Remove(object itemInstance)
        {
            RequireReferenceKey(itemInstance);
            bool removedPersisted = _store.Remove(itemInstance);
            bool removedMetadata;
            lock (_tableGate)
            {
                removedMetadata = _entries.Remove(itemInstance);
            }

            if (removedPersisted || removedMetadata)
            {
                Interlocked.Increment(ref _removalCount);
                return true;
            }

            return false;
        }

        private FirearmState ReadState(
            object itemInstance,
            out FirearmStateData data)
        {
            if (!_store.TryRead(itemInstance, out data))
            {
                data = null;
                return FirearmState.CreateEmpty();
            }

            return Decode(data);
        }

        private FirearmState Decode(FirearmStateData data)
        {
            return FirearmStateCodec.FromData(data, _rules);
        }

        private static FirearmStateData EncodeOrAbsent(FirearmState state)
        {
            return state == FirearmState.CreateEmpty()
                ? null
                : FirearmStateCodec.ToData(state);
        }

        private Entry GetOrCreateEntry(object itemInstance)
        {
            RequireReferenceKey(itemInstance);
            lock (_tableGate)
            {
                Entry entry;
                if (_entries.TryGetValue(itemInstance, out entry))
                {
                    return entry;
                }

                entry = new Entry(
                    Interlocked.Increment(ref _nextEntryId),
                    itemInstance.GetType().FullName ?? itemInstance.GetType().Name,
                    RuntimeHelpers.GetHashCode(itemInstance));
                _entries.Add(itemInstance, entry);
                Interlocked.Increment(ref _createdEntryCount);
                return entry;
            }
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

        private sealed class Entry
        {
            private readonly long _entryId;
            private readonly string _runtimeTypeName;
            private readonly int _runtimeReferenceHash;
            private int _revision;

            internal Entry(
                long entryId,
                string runtimeTypeName,
                int runtimeReferenceHash)
            {
                _entryId = entryId;
                _runtimeTypeName = runtimeTypeName;
                _runtimeReferenceHash = runtimeReferenceHash;
                Gate = new object();
            }

            internal object Gate { get; private set; }

            internal void IncrementRevision()
            {
                _revision++;
            }

            internal FirearmStateRepositorySnapshot Snapshot(FirearmState state)
            {
                return new FirearmStateRepositorySnapshot(
                    _entryId,
                    _revision,
                    _runtimeTypeName,
                    _runtimeReferenceHash,
                    state);
            }
        }
    }
}
