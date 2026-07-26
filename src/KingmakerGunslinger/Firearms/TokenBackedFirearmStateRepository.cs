using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Repository whose source of truth is an item-owned state token. A weak metadata
    /// table supplies process-local revision and diagnostic IDs without owning items.
    /// </summary>
    internal sealed class TokenBackedFirearmStateRepository : IFirearmStateRepository
    {
        private readonly object _tableGate = new object();
        private readonly ConditionalWeakTable<object, Entry> _entries =
            new ConditionalWeakTable<object, Entry>();
        private readonly IFirearmStateTokenStore _store;
        private readonly FirearmStateTokenCatalog _catalog;
        private long _nextEntryId;
        private long _createdEntryCount;
        private long _mutationCount;
        private long _removalCount;

        internal TokenBackedFirearmStateRepository(
            IFirearmStateTokenStore store,
            FirearmStateTokenCatalog catalog)
        {
            _store = store ?? throw new ArgumentNullException("store");
            _catalog = catalog ?? throw new ArgumentNullException("catalog");
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

        public FirearmStateRepositorySnapshot GetOrCreate(object itemInstance)
        {
            Entry entry = GetOrCreateEntry(itemInstance);
            lock (entry.Gate)
            {
                return entry.Snapshot(ReadState(itemInstance));
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
                IReadOnlyList<string> observedTokenIds = _store.ReadTokenIds(itemInstance);
                if (observedTokenIds.Count == 0)
                {
                    snapshot = null;
                    return false;
                }

                entry = GetOrCreateEntry(itemInstance);
            }

            lock (entry.Gate)
            {
                snapshot = entry.Snapshot(ReadState(itemInstance));
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
                IReadOnlyList<string> currentIds = _store.ReadTokenIds(itemInstance);
                FirearmState current = _catalog.Decode(currentIds);
                if (current == state)
                {
                    return entry.Snapshot(current);
                }

                _store.ReplaceToken(
                    itemInstance,
                    SingleTokenOrNull(currentIds),
                    _catalog.Encode(state));
                FirearmState verified = ReadState(itemInstance);
                if (verified != state)
                {
                    throw new InvalidOperationException(
                        "The item-owned firearm-state token write did not verify after replacement.");
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
                IReadOnlyList<string> currentIds = _store.ReadTokenIds(itemInstance);
                FirearmState current = _catalog.Decode(currentIds);
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

                _store.ReplaceToken(
                    itemInstance,
                    SingleTokenOrNull(currentIds),
                    _catalog.Encode(next));
                FirearmState verified = ReadState(itemInstance);
                if (verified != next)
                {
                    throw new InvalidOperationException(
                        "The item-owned firearm-state token transition did not verify after replacement.");
                }

                entry.IncrementRevision();
                Interlocked.Increment(ref _mutationCount);
                return entry.Snapshot(verified);
            }
        }

        public bool Remove(object itemInstance)
        {
            RequireReferenceKey(itemInstance);
            bool cleared = _store.ClearTokens(itemInstance);
            bool removed;
            lock (_tableGate)
            {
                removed = _entries.Remove(itemInstance);
            }

            if (cleared || removed)
            {
                Interlocked.Increment(ref _removalCount);
                return true;
            }

            return false;
        }

        private FirearmState ReadState(object itemInstance)
        {
            return _catalog.Decode(_store.ReadTokenIds(itemInstance));
        }

        private static string SingleTokenOrNull(IReadOnlyList<string> tokenIds)
        {
            if (tokenIds == null)
            {
                throw new ArgumentNullException("tokenIds");
            }

            if (tokenIds.Count == 0)
            {
                return null;
            }

            if (tokenIds.Count != 1)
            {
                throw new InvalidOperationException(
                    "A token replacement requires exactly zero or one current state token.");
            }

            return tokenIds[0];
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
