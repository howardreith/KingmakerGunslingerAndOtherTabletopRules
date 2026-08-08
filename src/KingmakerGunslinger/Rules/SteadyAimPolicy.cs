using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Rules
{
    internal static class SteadyAimPolicy
    {
        internal const int RangeBonusFeet = 10;

        internal static bool IsQualifyingShot(bool exactFirearm,
            int markerCount, FirearmKind kind, bool scatterDelivery)
        {
            if (markerCount < 0) throw new ArgumentOutOfRangeException("markerCount");
            if (!exactFirearm || markerCount != 1 || scatterDelivery) return false;
            return FirearmHandednessPolicy.Matches(kind,
                FirearmHandedness.TwoHanded);
        }
    }
}
