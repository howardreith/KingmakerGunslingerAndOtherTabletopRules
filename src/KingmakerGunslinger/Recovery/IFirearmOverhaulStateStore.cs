using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Minimal exact-item state port for the Wrecked-to-Broken overhaul transaction.
    /// Implementations must compare the expected current state before replacement.
    /// </summary>
    internal interface IFirearmOverhaulStateStore
    {
        FirearmState Read();

        void Replace(FirearmState expectedCurrent, FirearmState replacement);
    }
}
