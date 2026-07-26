using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Process-local repository keyed by exact object reference through a
    /// ConditionalWeakTable. The table does not keep discarded item entities alive.
    /// </summary>
    internal sealed class WeakFirearmStateRepository : IFirearmStateRepository
    {
        private readonly object _tableGate = new object();
        private readonly ConditionalWeakTable<object, Entry> _entries =
            new ConditionalWeakTable<object, Entry>();
        private long _nextEntryId;
        private long _createdEntryCount;
        private long _mutationCount;
        private long _removalCount;

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
            return entry.ReadSnapshot();
        }

        public bool TryGet(
            object itemInstance,
            out FirearmStateRepositorySnapshot snapshot)
        {
            RequireReferenceKey(itemInstance);
            Entry entry;
            lock (_tableGate)
            {
                if (!_entries.TryGetValue(itemInstance, out entry))
                {
                    snapshot = null;
                    return false;
                }
            }

            snapshot = entry.ReadSnapshot();
            return true;
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
            bool changed;
            FirearmStateRepositorySnapshot snapshot = entry.Set(state, out changed);
            if (changed)
            {
                Interlocked.Increment(ref _mutationCount);
            }

            return snapshot;
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
            bool changed;
            FirearmStateRepositorySnapshot snapshot = entry.Transition(transition, out changed);
            if (changed)
            {
                Interlocked.Increment(ref _mutationCount);
            }

            return snapshot;
        }

        public bool Remove(object itemInstance)
        {
            RequireReferenceKey(itemInstance);
            bool removed;
            lock (_tableGate)
            {
                removed = _entries.Remove(itemInstance);
            }

            if (removed)
            {
                Interlocked.Increment(ref _removalCount);
            }

            return removed;
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

                long entryId = Interlocked.Increment(ref _nextEntryId);
                entry = new Entry(
                    entryId,
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
            private readonly object _gate = new object();
            private readonly long _entryId;
            private readonly string _runtimeTypeName;
            private readonly int _runtimeReferenceHash;
            private int _revision;
            private FirearmState _state;

            internal Entry(
                long entryId,
                string runtimeTypeName,
                int runtimeReferenceHash)
            {
                _entryId = entryId;
                _runtimeTypeName = runtimeTypeName;
                _runtimeReferenceHash = runtimeReferenceHash;
                _state = FirearmState.CreateEmpty();
            }

            internal FirearmStateRepositorySnapshot ReadSnapshot()
            {
                lock (_gate)
                {
                    return CreateSnapshot();
                }
            }

            internal FirearmStateRepositorySnapshot Set(
                FirearmState state,
                out bool changed)
            {
                lock (_gate)
                {
                    changed = _state != state;
                    if (changed)
                    {
                        _state = state;
                        _revision++;
                    }

                    return CreateSnapshot();
                }
            }

            internal FirearmStateRepositorySnapshot Transition(
                Func<FirearmState, FirearmState> transition,
                out bool changed)
            {
                lock (_gate)
                {
                    FirearmState next = transition(_state);
                    if (next == null)
                    {
                        throw new InvalidOperationException(
                            "A firearm-state transition returned null.");
                    }

                    changed = _state != next;
                    if (changed)
                    {
                        _state = next;
                        _revision++;
                    }

                    return CreateSnapshot();
                }
            }

            private FirearmStateRepositorySnapshot CreateSnapshot()
            {
                return new FirearmStateRepositorySnapshot(
                    _entryId,
                    _revision,
                    _runtimeTypeName,
                    _runtimeReferenceHash,
                    _state);
            }
        }
    }
}
