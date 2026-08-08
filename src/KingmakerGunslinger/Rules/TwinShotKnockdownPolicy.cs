using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Rules
{
    internal static class TwinShotKnockdownPolicy
    {
        internal const int RequiredHits = 2;
        internal const int OrdinaryGritCost = 1;

        internal static bool IsQualifyingHit(bool ownTurn, bool exactFirearm,
            int markerCount, FirearmKind kind, bool hit)
        {
            if (markerCount < 0) throw new ArgumentOutOfRangeException("markerCount");
            return ownTurn && exactFirearm && markerCount == 1 && hit &&
                FirearmHandednessPolicy.Matches(kind,
                    FirearmHandedness.OneHanded);
        }

        internal static bool CanExecute(int distinctHits, bool alreadyUsed,
            bool prone, bool proneImmune, int grit)
        {
            if (distinctHits < 0) throw new ArgumentOutOfRangeException("distinctHits");
            if (grit < 0) throw new ArgumentOutOfRangeException("grit");
            return distinctHits >= RequiredHits && !alreadyUsed && !prone &&
                !proneImmune && grit >= OrdinaryGritCost;
        }
    }
}
