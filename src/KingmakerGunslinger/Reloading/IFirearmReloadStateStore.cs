using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Minimal state port used by the cross-resource reload transaction. Implementations
    /// must compare the exact expected state before replacing it so a stale reload cannot
    /// overwrite an unrelated firearm mutation.
    /// </summary>
    internal interface IFirearmReloadStateStore
    {
        FirearmState Read();

        void Replace(FirearmState expectedCurrent, FirearmState replacement);
    }
}
