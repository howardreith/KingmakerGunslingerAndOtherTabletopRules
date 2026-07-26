using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Items;

namespace KingmakerGunslinger.Ammunition
{
    /// <summary>
    /// Typed Kingmaker adapter over the shared ItemsCollection. Count is the total
    /// quantity across stacks; Add and Remove use Kingmaker's own merge/split behavior.
    /// </summary>
    internal sealed class KingmakerBasicAmmunitionInventory : IBasicAmmunitionInventory
    {
        private readonly ItemsCollection _inventory;
        private readonly BlueprintItem _blackPowder;
        private readonly BlueprintItem _leadBall;

        internal KingmakerBasicAmmunitionInventory(
            ItemsCollection inventory,
            BlueprintItem blackPowder,
            BlueprintItem leadBall)
        {
            _inventory = inventory ?? throw new ArgumentNullException("inventory");
            _blackPowder = blackPowder ?? throw new ArgumentNullException("blackPowder");
            _leadBall = leadBall ?? throw new ArgumentNullException("leadBall");
            if (ReferenceEquals(_blackPowder, _leadBall))
            {
                throw new ArgumentException(
                    "Black powder and lead ball must be distinct blueprint instances.");
            }
        }

        internal ItemsCollection Inventory
        {
            get { return _inventory; }
        }

        internal BlueprintItem BlackPowderBlueprint
        {
            get { return _blackPowder; }
        }

        internal BlueprintItem LeadBallBlueprint
        {
            get { return _leadBall; }
        }

        public int Count(BasicAmmunitionComponent component)
        {
            int count = _inventory.Count(RequireBlueprint(component));
            if (count < 0)
            {
                throw new InvalidOperationException(
                    "Kingmaker returned a negative basic-ammunition count.");
            }

            return count;
        }

        public void Add(BasicAmmunitionComponent component, int amount)
        {
            ValidateAmount(amount);
            _inventory.Add(RequireBlueprint(component), amount);
        }

        public void Remove(BasicAmmunitionComponent component, int amount)
        {
            ValidateAmount(amount);
            int available = Count(component);
            if (available < amount)
            {
                throw new InvalidOperationException(
                    "Cannot remove " + amount + " " + component +
                    " item(s); only " + available + " are available.");
            }

            _inventory.Remove(RequireBlueprint(component), amount);
        }

        private BlueprintItem RequireBlueprint(BasicAmmunitionComponent component)
        {
            switch (component)
            {
                case BasicAmmunitionComponent.BlackPowderCharge:
                    return _blackPowder;
                case BasicAmmunitionComponent.LeadBall:
                    return _leadBall;
                default:
                    throw new ArgumentOutOfRangeException(
                        "component",
                        component,
                        "Unknown basic-ammunition component.");
            }
        }

        private static void ValidateAmount(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "amount",
                    amount,
                    "Ammunition inventory mutations require a positive amount.");
            }
        }
    }
}
