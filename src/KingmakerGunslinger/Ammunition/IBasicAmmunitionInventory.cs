namespace KingmakerGunslinger.Ammunition
{
    /// <summary>
    /// Minimal inventory port used by the atomic powder-and-projectile transaction.
    /// Runtime adapters must report total stack counts, not the number of stack objects.
    /// </summary>
    internal interface IBasicAmmunitionInventory
    {
        int Count(BasicAmmunitionComponent component);

        void Add(BasicAmmunitionComponent component, int amount);

        void Remove(BasicAmmunitionComponent component, int amount);
    }
}
