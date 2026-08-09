using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Items;

namespace KingmakerGunslinger.Ammunition
{
    internal sealed class KingmakerReloadAmmunitionInventory : IReloadAmmunitionInventory
    {
        private readonly ItemsCollection _inventory;
        private readonly BlueprintItem _powder;
        private readonly BlueprintItem _ball;
        private readonly BlueprintItem _paper;

        internal KingmakerReloadAmmunitionInventory(ItemsCollection inventory,
            BlueprintItem powder, BlueprintItem ball, BlueprintItem paper)
        {
            _inventory = inventory ?? throw new ArgumentNullException("inventory");
            _powder = powder ?? throw new ArgumentNullException("powder");
            _ball = ball ?? throw new ArgumentNullException("ball");
            _paper = paper ?? throw new ArgumentNullException("paper");
            if (ReferenceEquals(_powder, _ball) || ReferenceEquals(_powder, _paper) ||
                ReferenceEquals(_ball, _paper))
                throw new ArgumentException("Reload inventory blueprints must be distinct exact items.");
        }

        public int Count(ReloadInventoryComponent component)
        {
            int count = _inventory.Count(Require(component));
            if (count < 0) throw new InvalidOperationException(
                "Kingmaker returned a negative reload-ammunition count.");
            return count;
        }

        public void Add(ReloadInventoryComponent component, int amount)
        {
            ValidateAmount(amount);
            _inventory.Add(Require(component), amount);
        }

        public void Remove(ReloadInventoryComponent component, int amount)
        {
            ValidateAmount(amount);
            if (Count(component) < amount)
                throw new InvalidOperationException("The bound reload source became unavailable.");
            _inventory.Remove(Require(component), amount);
        }

        private BlueprintItem Require(ReloadInventoryComponent component)
        {
            switch (component)
            {
                case ReloadInventoryComponent.BlackPowderCharge: return _powder;
                case ReloadInventoryComponent.LeadBall: return _ball;
                case ReloadInventoryComponent.PaperCartridge: return _paper;
                default: throw new ArgumentOutOfRangeException("component");
            }
        }

        private static void ValidateAmount(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException("amount");
        }
    }
}
