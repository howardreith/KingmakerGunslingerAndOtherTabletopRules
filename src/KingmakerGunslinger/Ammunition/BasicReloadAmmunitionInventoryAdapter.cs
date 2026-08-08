using System;

namespace KingmakerGunslinger.Ammunition
{
    internal sealed class BasicReloadAmmunitionInventoryAdapter : IReloadAmmunitionInventory
    {
        private readonly IBasicAmmunitionInventory _inner;
        internal BasicReloadAmmunitionInventoryAdapter(IBasicAmmunitionInventory inner)
        { _inner = inner ?? throw new ArgumentNullException("inner"); }
        public int Count(ReloadInventoryComponent component)
        { return component == ReloadInventoryComponent.PaperCartridge ? 0 : _inner.Count(Convert(component)); }
        public void Add(ReloadInventoryComponent component, int amount)
        { _inner.Add(Convert(component), amount); }
        public void Remove(ReloadInventoryComponent component, int amount)
        { _inner.Remove(Convert(component), amount); }
        private static BasicAmmunitionComponent Convert(ReloadInventoryComponent component)
        {
            switch (component)
            {
                case ReloadInventoryComponent.BlackPowderCharge: return BasicAmmunitionComponent.BlackPowderCharge;
                case ReloadInventoryComponent.LeadBall: return BasicAmmunitionComponent.LeadBall;
                default: throw new ArgumentOutOfRangeException("component", component,
                    "The basic inventory adapter has no Paper Cartridge source.");
            }
        }
    }
}
