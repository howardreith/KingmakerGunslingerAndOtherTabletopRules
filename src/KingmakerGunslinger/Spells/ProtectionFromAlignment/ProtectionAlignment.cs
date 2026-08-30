using System;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    [Flags]
    internal enum ProtectionAlignment
    {
        None = 0,
        Evil = 1,
        Good = 2,
        Law = 4,
        Chaos = 8
    }
}
