using System;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    internal enum ProtectionComponentPublicationDecision
    {
        Append = 0,
        AlreadyPatched = 1
    }

    internal static class ProtectionComponentPublicationPolicy
    {
        internal static ProtectionComponentPublicationDecision Decide(
            int exactComponentCount)
        {
            if (exactComponentCount < 0)
                throw new ArgumentOutOfRangeException("exactComponentCount");
            if (exactComponentCount > 1)
                throw new InvalidOperationException(
                    "A protection buff contains duplicate control-immunity components.");
            return exactComponentCount == 0 ?
                ProtectionComponentPublicationDecision.Append :
                ProtectionComponentPublicationDecision.AlreadyPatched;
        }
    }
}
