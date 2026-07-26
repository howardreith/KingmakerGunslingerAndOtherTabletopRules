using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Items;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Typed Kingmaker adapter over the shared ItemsCollection for the custom stackable
    /// Firearm Repair Kit blueprint.
    /// </summary>
    internal sealed class KingmakerRepairKitInventory : IRepairKitInventory
    {
        private readonly ItemsCollection _inventory;
        private readonly BlueprintItem _repairKit;

        internal KingmakerRepairKitInventory(
            ItemsCollection inventory,
            BlueprintItem repairKit)
        {
            _inventory = inventory ?? throw new ArgumentNullException("inventory");
            _repairKit = repairKit ?? throw new ArgumentNullException("repairKit");
        }

        internal ItemsCollection Inventory
        {
            get { return _inventory; }
        }

        internal BlueprintItem RepairKitBlueprint
        {
            get { return _repairKit; }
        }

        public int Count()
        {
            int count = _inventory.Count(_repairKit);
            if (count < 0)
            {
                throw new InvalidOperationException(
                    "Kingmaker returned a negative Firearm Repair Kit count.");
            }

            return count;
        }

        public void Add(int amount)
        {
            ValidateAmount(amount);
            _inventory.Add(_repairKit, amount);
        }

        public void Remove(int amount)
        {
            ValidateAmount(amount);
            int available = Count();
            if (available < amount)
            {
                throw new InvalidOperationException(
                    "Cannot remove " + amount + " Firearm Repair Kit item(s); only " +
                    available + " are available.");
            }

            _inventory.Remove(_repairKit, amount);
        }

        private static void ValidateAmount(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "amount",
                    amount,
                    "Repair-kit inventory mutations require a positive amount.");
            }
        }
    }
}
