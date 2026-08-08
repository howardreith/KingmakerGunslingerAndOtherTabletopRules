using System;

namespace KingmakerGunslinger.Ammunition
{
    internal sealed class ReloadAmmunitionTransactionService
    {
        internal ReloadAmmunitionInventorySnapshot Consume(IReloadAmmunitionInventory inventory,
            ReloadAmmunitionProfile profile, int loads)
        {
            if (inventory == null) throw new ArgumentNullException("inventory");
            if (profile == null) throw new ArgumentNullException("profile");
            if (loads <= 0) throw new ArgumentOutOfRangeException("loads");
            ReloadAmmunitionInventorySnapshot before = ReloadAmmunitionInventorySnapshot.Capture(inventory);
            if (before.AvailableLoads(profile) < loads) return null;
            try
            {
                if (profile.SourceKind == ReloadAmmunitionSourceKind.LooseBasic)
                { inventory.Remove(ReloadInventoryComponent.BlackPowderCharge, loads); inventory.Remove(ReloadInventoryComponent.LeadBall, loads); }
                else if (profile.SourceKind == ReloadAmmunitionSourceKind.PaperCartridge)
                    inventory.Remove(ReloadInventoryComponent.PaperCartridge, loads);
                else throw new InvalidOperationException("Unknown reload inventory source.");
                ReloadAmmunitionInventorySnapshot after = ReloadAmmunitionInventorySnapshot.Capture(inventory);
                VerifyDelta(before, after, profile, loads); return after;
            }
            catch { RestoreExact(inventory, before); throw; }
        }
        internal void RestoreExact(IReloadAmmunitionInventory inventory, ReloadAmmunitionInventorySnapshot expected)
        {
            if (inventory == null) throw new ArgumentNullException("inventory");
            if (expected == null) throw new ArgumentNullException("expected");
            Restore(inventory, ReloadInventoryComponent.BlackPowderCharge, expected.BlackPowderCharges);
            Restore(inventory, ReloadInventoryComponent.LeadBall, expected.LeadBalls);
            Restore(inventory, ReloadInventoryComponent.PaperCartridge, expected.PaperCartridges);
            if (!expected.Equals(ReloadAmmunitionInventorySnapshot.Capture(inventory)))
                throw new InvalidOperationException("Reload inventory rollback did not restore the exact snapshot.");
        }
        internal static void VerifyDelta(ReloadAmmunitionInventorySnapshot before,
            ReloadAmmunitionInventorySnapshot after, ReloadAmmunitionProfile profile, int loads)
        {
            int powder = before.BlackPowderCharges - after.BlackPowderCharges;
            int balls = before.LeadBalls - after.LeadBalls;
            int paper = before.PaperCartridges - after.PaperCartridges;
            bool exact = profile.SourceKind == ReloadAmmunitionSourceKind.LooseBasic
                ? powder == loads && balls == loads && paper == 0
                : profile.SourceKind == ReloadAmmunitionSourceKind.PaperCartridge && powder == 0 && balls == 0 && paper == loads;
            if (!exact) throw new InvalidOperationException("Reload ammunition consumption did not match the bound profile.");
        }
        private static void Restore(IReloadAmmunitionInventory inventory, ReloadInventoryComponent component, int expected)
        { int current = inventory.Count(component); if (current < expected) inventory.Add(component, expected - current); else if (current > expected) inventory.Remove(component, current - expected); }
    }
}
