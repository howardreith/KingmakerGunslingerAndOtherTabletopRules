using System;
using System.Globalization;
using Kingmaker.Items;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Immutable read-only evaluation used by ability availability and diagnostics.
    /// Reason is concise player text; TechnicalReason remains structured-log data.
    /// </summary>
    internal sealed class ReloadTestMusketAvailability
    {
        internal ReloadTestMusketAvailability(bool isAvailable, string reason,
            ItemEntityWeapon weapon, FirearmItemStateSnapshot firearm,
            ReloadAmmunitionInventorySnapshot inventory, FirearmReloadPlan plan,
            string technicalReason = null)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException(
                "A reload-availability reason is required.", "reason");
            IsAvailable = isAvailable;
            Reason = reason;
            TechnicalReason = technicalReason ?? reason;
            Weapon = weapon;
            Firearm = firearm;
            Inventory = inventory;
            Plan = plan;
            if (isAvailable && (weapon == null || firearm == null ||
                inventory == null || plan == null || !plan.IsAvailable))
                throw new ArgumentException(
                    "An available reload evaluation requires an exact weapon, firearm state, and inventory snapshot.");
        }

        internal bool IsAvailable { get; private set; }
        internal string Reason { get; private set; }
        internal string TechnicalReason { get; private set; }
        internal ItemEntityWeapon Weapon { get; private set; }
        internal FirearmItemStateSnapshot Firearm { get; private set; }
        internal ReloadAmmunitionInventorySnapshot Inventory { get; private set; }
        internal FirearmReloadPlan Plan { get; private set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "available={0}; playerReason={1}; technicalReason={2}; firearm=[{3}]; inventory=[{4}]",
                IsAvailable, Reason, TechnicalReason,
                Firearm == null ? "<unavailable>" : Firearm.ToString(),
                Inventory == null ? "<unavailable>" : Inventory.ToString());
        }
    }
}
