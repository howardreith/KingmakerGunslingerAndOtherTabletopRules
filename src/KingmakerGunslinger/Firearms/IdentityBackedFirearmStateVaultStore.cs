using System;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Adapts concrete runtime item objects to a primitive identity-keyed record
    /// store. Reconstructed runtime objects with the same verified engine identity
    /// observe the same persisted firearm state.
    /// </summary>
    internal sealed class IdentityBackedFirearmStateVaultStore : IFirearmStateVaultStore
    {
        private readonly IFirearmItemIdentityProvider _identityProvider;
        private readonly IFirearmStateIdentityRecordStore _recordStore;

        internal IdentityBackedFirearmStateVaultStore(
            IFirearmItemIdentityProvider identityProvider,
            IFirearmStateIdentityRecordStore recordStore)
        {
            _identityProvider = identityProvider ??
                throw new ArgumentNullException("identityProvider");
            _recordStore = recordStore ??
                throw new ArgumentNullException("recordStore");
        }

        public int RecordCount
        {
            get { return _recordStore.RecordCount; }
        }

        public bool TryRead(object itemInstance, out FirearmStateData data)
        {
            return _recordStore.TryRead(RequireIdentity(itemInstance), out data);
        }

        public void Replace(
            object itemInstance,
            FirearmStateData expectedData,
            FirearmStateData targetData)
        {
            _recordStore.Replace(
                RequireIdentity(itemInstance),
                expectedData,
                targetData);
        }

        public bool Remove(object itemInstance)
        {
            return _recordStore.Remove(RequireIdentity(itemInstance));
        }

        internal FirearmItemId RequireIdentity(object itemInstance)
        {
            FirearmItemId identity;
            string reason;
            if (!_identityProvider.TryGetIdentity(
                itemInstance,
                out identity,
                out reason) ||
                identity == null)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(reason)
                        ? "Kingmaker exposed no usable engine identity for the firearm item."
                        : reason);
            }

            return identity;
        }
    }
}
