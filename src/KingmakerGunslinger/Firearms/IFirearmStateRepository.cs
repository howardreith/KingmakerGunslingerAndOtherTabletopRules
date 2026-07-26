using System;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Access boundary for state associated with one exact runtime firearm item object.
    /// Implementations may be process-local or backed by an item-owned persistence carrier;
    /// callers depend only on immutable state and atomic transition semantics.
    /// </summary>
    internal interface IFirearmStateRepository
    {
        FirearmStateRepositorySnapshot GetOrCreate(object itemInstance);

        bool TryGet(object itemInstance, out FirearmStateRepositorySnapshot snapshot);

        FirearmStateRepositorySnapshot Set(object itemInstance, FirearmState state);

        FirearmStateRepositorySnapshot Transition(
            object itemInstance,
            Func<FirearmState, FirearmState> transition);

        bool Remove(object itemInstance);

        long CreatedEntryCount { get; }

        long MutationCount { get; }

        long RemovalCount { get; }
    }
}
