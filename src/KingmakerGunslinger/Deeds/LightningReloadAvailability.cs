using System;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Reloading;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class LightningReloadAvailability
    {
        internal LightningReloadAvailability(LightningReloadDecision decision,
            ExactEquippedFirearmContext firearm,
            KingmakerReloadAmmunitionInventory inventory,
            FirearmReloadPlan reloadPlan)
        {
            Decision = decision ?? throw new ArgumentNullException("decision");
            Firearm = firearm;
            Inventory = inventory;
            ReloadPlan = reloadPlan;
            if (decision.IsAvailable && (firearm == null || inventory == null ||
                reloadPlan == null || !reloadPlan.IsAvailable))
                throw new ArgumentException(
                    "Available Lightning Reload requires an exact firearm and inventory.");
        }

        internal LightningReloadDecision Decision { get; private set; }
        internal ExactEquippedFirearmContext Firearm { get; private set; }
        internal KingmakerReloadAmmunitionInventory Inventory { get; private set; }
        internal FirearmReloadPlan ReloadPlan { get; private set; }
    }
}
