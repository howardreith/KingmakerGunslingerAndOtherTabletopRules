using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    internal sealed class FirearmItemReloadStateStore : IFirearmReloadStateStore
    {
        private readonly FirearmItemStateService _service;
        private readonly object _item;

        internal FirearmItemReloadStateStore(
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
                    "The exact firearm changed before the reload state could be replaced.");
            }

            FirearmState updated = _service.Set(_item, replacement).Repository.State;
            if (updated != replacement)
            {
                throw new InvalidOperationException(
                    "The exact firearm did not retain the requested reload state.");
            }
        }
    }
}
