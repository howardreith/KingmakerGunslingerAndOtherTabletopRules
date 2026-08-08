using System;
using System.Globalization;

namespace KingmakerGunslinger.Ammunition
{
    internal sealed class ReloadAmmunitionInventorySnapshot : IEquatable<ReloadAmmunitionInventorySnapshot>
    {
        internal ReloadAmmunitionInventorySnapshot(int powder, int balls, int paperCartridges)
        {
            if (powder < 0) throw new ArgumentOutOfRangeException("powder");
            if (balls < 0) throw new ArgumentOutOfRangeException("balls");
            if (paperCartridges < 0) throw new ArgumentOutOfRangeException("paperCartridges");
            BlackPowderCharges = powder; LeadBalls = balls; PaperCartridges = paperCartridges;
        }
        internal int BlackPowderCharges { get; private set; }
        internal int LeadBalls { get; private set; }
        internal int PaperCartridges { get; private set; }
        internal static ReloadAmmunitionInventorySnapshot Capture(IReloadAmmunitionInventory inventory)
        {
            if (inventory == null) throw new ArgumentNullException("inventory");
            return new ReloadAmmunitionInventorySnapshot(Read(inventory, ReloadInventoryComponent.BlackPowderCharge),
                Read(inventory, ReloadInventoryComponent.LeadBall), Read(inventory, ReloadInventoryComponent.PaperCartridge));
        }
        internal int AvailableLoads(ReloadAmmunitionProfile profile)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            switch (profile.SourceKind)
            {
                case ReloadAmmunitionSourceKind.LooseBasic: return Math.Min(BlackPowderCharges, LeadBalls);
                case ReloadAmmunitionSourceKind.PaperCartridge: return PaperCartridges;
                default: throw new ArgumentOutOfRangeException("profile");
            }
        }
        public bool Equals(ReloadAmmunitionInventorySnapshot other)
        { return !ReferenceEquals(other, null) && BlackPowderCharges == other.BlackPowderCharges && LeadBalls == other.LeadBalls && PaperCartridges == other.PaperCartridges; }
        public override bool Equals(object obj) { return Equals(obj as ReloadAmmunitionInventorySnapshot); }
        public override int GetHashCode() { return ((BlackPowderCharges * 397) ^ LeadBalls) * 397 ^ PaperCartridges; }
        public override string ToString() { return string.Format(CultureInfo.InvariantCulture,
            "blackPowder={0}; leadBalls={1}; paperCartridges={2}", BlackPowderCharges, LeadBalls, PaperCartridges); }
        private static int Read(IReloadAmmunitionInventory inventory, ReloadInventoryComponent component)
        { int count = inventory.Count(component); if (count < 0) throw new InvalidOperationException("Reload inventory returned a negative count for " + component + "."); return count; }
    }
}
