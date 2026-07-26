using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Exact runtime-item adapter over the accepted item-owned state-token service.
    /// </summary>
    internal sealed class FirearmItemOverhaulStateStore : IFirearmOverhaulStateStore
    {
        private readonly FirearmItemStateService _service;
        private readonly object _item;

        internal FirearmItemOverhaulStateStore(
            FirearmItemStateService service,
            object item)
        {
            _service = service ?? throw new ArgumentNullException("service");
            _item = item ?? throw new ArgumentNullException("item");
        }

        public FirearmState Read()
        {
            return _service.GetOrCreate(_item).Repository.State;
        }

        public void Replace(
            FirearmState expectedCurrent,
            FirearmState replacement)
        {
            if (expectedCurrent == null)
            {
                throw new ArgumentNullException("expectedCurrent");
            }

            if (replacement == null)
            {
                throw new ArgumentNullException("replacement");
            }

            FirearmState current = Read();
            if (current != expectedCurrent)
            {
                throw new InvalidOperationException(
                    "The exact firearm changed before overhaul state replacement.");
            }

            FirearmState updated = _service.Set(_item, replacement).Repository.State;
            if (updated != replacement)
            {
                throw new InvalidOperationException(
                    "The exact firearm did not retain the requested overhaul state.");
            }
        }
    }
}
