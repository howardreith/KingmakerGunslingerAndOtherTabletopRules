using System;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Actions;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class LightningReloadAvailability
    {
        internal LightningReloadAvailability(LightningReloadDecision decision,
            ExactEquippedFirearmContext firearm,
            KingmakerBasicAmmunitionInventory inventory)
        {
            Decision = decision ?? throw new ArgumentNullException("decision");
            Firearm = firearm;
            Inventory = inventory;
            if (decision.IsAvailable && (firearm == null || inventory == null))
                throw new ArgumentException(
                    "Available Lightning Reload requires an exact firearm and inventory.");
        }

        internal LightningReloadDecision Decision { get; private set; }
        internal ExactEquippedFirearmContext Firearm { get; private set; }
        internal KingmakerBasicAmmunitionInventory Inventory { get; private set; }
    }
}
