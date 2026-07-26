using System;
using System.Globalization;
using Kingmaker.Items;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Immutable read-only evaluation used by both the ability availability provider
    /// and the development diagnostics. It contains no mutation capability.
    /// </summary>
    internal sealed class ReloadTestMusketAvailability
    {
        internal ReloadTestMusketAvailability(
            bool isAvailable,
            string reason,
            ItemEntityWeapon weapon,
            FirearmItemStateSnapshot firearm,
            BasicAmmunitionInventorySnapshot inventory)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("A reload-availability reason is required.", "reason");
            }

            IsAvailable = isAvailable;
            Reason = reason;
            Weapon = weapon;
            Firearm = firearm;
            Inventory = inventory;

            if (isAvailable && (weapon == null || firearm == null || inventory == null))
            {
                throw new ArgumentException(
                    "An available reload evaluation requires an exact weapon, firearm state, and inventory snapshot.");
            }
        }

        internal bool IsAvailable { get; private set; }
        internal string Reason { get; private set; }
        internal ItemEntityWeapon Weapon { get; private set; }
        internal FirearmItemStateSnapshot Firearm { get; private set; }
        internal BasicAmmunitionInventorySnapshot Inventory { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "available={0}; reason={1}; firearm=[{2}]; inventory=[{3}]",
                IsAvailable,
                Reason,
                Firearm == null ? "<unavailable>" : Firearm.ToString(),
                Inventory == null ? "<unavailable>" : Inventory.ToString());
        }
    }
}
