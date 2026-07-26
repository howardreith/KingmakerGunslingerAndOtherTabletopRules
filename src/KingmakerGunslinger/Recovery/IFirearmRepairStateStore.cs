using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Minimal exact-item state port for the ordinary Broken-to-Normal repair
    /// transaction. Implementations must compare the expected current state before
    /// replacement.
    /// </summary>
    internal interface IFirearmRepairStateStore
    {
        FirearmState Read();

        void Replace(FirearmState expectedCurrent, FirearmState replacement);
    }
}
