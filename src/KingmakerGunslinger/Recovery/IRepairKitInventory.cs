namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Minimal inventory port used by the same-item Wrecked-to-Broken overhaul
    /// transaction. Runtime adapters must report total quantity across stacks.
    /// </summary>
    internal interface IRepairKitInventory
    {
        int Count();

        void Add(int amount);

        void Remove(int amount);
    }
}
