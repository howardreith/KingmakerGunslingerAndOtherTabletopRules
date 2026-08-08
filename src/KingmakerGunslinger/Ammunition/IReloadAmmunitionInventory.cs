namespace KingmakerGunslinger.Ammunition
{
    internal interface IReloadAmmunitionInventory
    {
        int Count(ReloadInventoryComponent component);
        void Add(ReloadInventoryComponent component, int amount);
        void Remove(ReloadInventoryComponent component, int amount);
    }
}
