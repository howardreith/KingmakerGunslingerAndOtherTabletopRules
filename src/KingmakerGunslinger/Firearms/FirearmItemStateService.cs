using System;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Validates exact firearm identity before any repository operation. This is the
    /// runtime service boundary that prevents native Heavy Crossbows and blueprints
    /// from accidentally receiving firearm state.
    /// </summary>
    internal sealed class FirearmItemStateService
    {
        private readonly IFirearmRuntimeItemResolver _resolver;
        private readonly IFirearmStateRepository _repository;

        internal FirearmItemStateService(
            IFirearmRuntimeItemResolver resolver,
            IFirearmStateRepository repository)
        {
            _resolver = resolver ?? throw new ArgumentNullException("resolver");
            _repository = repository ?? throw new ArgumentNullException("repository");
        }

        internal IFirearmStateRepository Repository
        {
            get { return _repository; }
        }

        internal bool TryGetOrCreate(
            object candidate,
            out FirearmItemStateSnapshot snapshot,
            out string rejectionReason)
        {
            ResolvedFirearmItem firearm;
            if (!_resolver.TryResolve(candidate, out firearm, out rejectionReason))
            {
                snapshot = null;
                return false;
            }

            snapshot = Join(firearm, _repository.GetOrCreate(firearm.ItemInstance));
            rejectionReason = null;
            return true;
        }

        internal bool TryGetExisting(
            object candidate,
            out FirearmItemStateSnapshot snapshot,
            out string rejectionReason)
        {
            ResolvedFirearmItem firearm;
            if (!_resolver.TryResolve(candidate, out firearm, out rejectionReason))
            {
                snapshot = null;
                return false;
            }

            FirearmStateRepositorySnapshot repositorySnapshot;
            if (!_repository.TryGet(firearm.ItemInstance, out repositorySnapshot))
            {
                snapshot = null;
                rejectionReason = "The exact firearm item has no existing firearm-state entry.";
                return false;
            }

            snapshot = Join(firearm, repositorySnapshot);
            rejectionReason = null;
            return true;
        }

        internal FirearmItemStateSnapshot GetOrCreate(object candidate)
        {
            FirearmItemStateSnapshot snapshot;
            string reason;
            if (!TryGetOrCreate(candidate, out snapshot, out reason))
            {
                throw new InvalidOperationException(reason);
            }

            return snapshot;
        }

        internal FirearmItemStateSnapshot Set(
            object candidate,
            FirearmState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            ResolvedFirearmItem firearm = ResolveRequired(candidate);
            return Join(firearm, _repository.Set(firearm.ItemInstance, state));
        }

        internal FirearmItemStateSnapshot Transition(
            object candidate,
            Func<FirearmState, FirearmState> transition)
        {
            if (transition == null)
            {
                throw new ArgumentNullException("transition");
            }

            ResolvedFirearmItem firearm = ResolveRequired(candidate);
            return Join(
                firearm,
                _repository.Transition(firearm.ItemInstance, transition));
        }

        internal bool Forget(object candidate)
        {
            ResolvedFirearmItem firearm;
            string ignored;
            return _resolver.TryResolve(candidate, out firearm, out ignored) &&
                _repository.Remove(firearm.ItemInstance);
        }

        private ResolvedFirearmItem ResolveRequired(object candidate)
        {
            ResolvedFirearmItem firearm;
            string reason;
            if (!_resolver.TryResolve(candidate, out firearm, out reason))
            {
                throw new InvalidOperationException(reason);
            }

            return firearm;
        }

        private static FirearmItemStateSnapshot Join(
            ResolvedFirearmItem firearm,
            FirearmStateRepositorySnapshot repository)
        {
            return new FirearmItemStateSnapshot(firearm, repository);
        }
    }
}
