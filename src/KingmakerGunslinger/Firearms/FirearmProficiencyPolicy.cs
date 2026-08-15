using System;

namespace KingmakerGunslinger.Firearms
{
    internal static class FirearmProficiencyPolicy
    {
        internal static bool CanUse(int markerCount, FirearmKind kind,
            bool hasFullProficiency, bool hasOneHandedProficiency,
            bool hasTwoHandedProficiency)
        {
            if (markerCount != 1) return false;
            FirearmHandedness handedness;
            if (!FirearmHandednessPolicy.TryGet(kind, out handedness)) return false;
            if (hasFullProficiency) return true;
            return handedness == FirearmHandedness.OneHanded
                ? hasOneHandedProficiency
                : handedness == FirearmHandedness.TwoHanded &&
                    hasTwoHandedProficiency;
        }

        internal static bool GrantsScatter(FirearmHandedness scope)
        {
            if (scope == FirearmHandedness.OneHanded) return false;
            if (scope == FirearmHandedness.TwoHanded) return true;
            throw new ArgumentOutOfRangeException("scope");
        }

    }
}
