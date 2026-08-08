using System;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Canonical fail-closed mapping between stable firearm kind and family.
    /// Every rule that distinguishes pistols from long guns must use this map.
    /// </summary>
    internal static class FirearmHandednessPolicy
    {
        internal static bool TryGet(FirearmKind kind,
            out FirearmHandedness handedness)
        {
            switch (kind)
            {
                case FirearmKind.Pistol:
                case FirearmKind.Revolver:
                    handedness = FirearmHandedness.OneHanded;
                    return true;
                case FirearmKind.Musket:
                case FirearmKind.Blunderbuss:
                case FirearmKind.Rifle:
                    handedness = FirearmHandedness.TwoHanded;
                    return true;
                default:
                    handedness = FirearmHandedness.Unknown;
                    return false;
            }
        }

        internal static FirearmHandedness Require(FirearmKind kind)
        {
            FirearmHandedness handedness;
            if (!TryGet(kind, out handedness))
                throw new ArgumentOutOfRangeException("kind",
                    "Unknown firearm kinds have no authorized handedness.");
            return handedness;
        }

        internal static bool Matches(FirearmKind kind,
            FirearmHandedness required)
        {
            if (required != FirearmHandedness.OneHanded &&
                required != FirearmHandedness.TwoHanded)
                return false;
            FirearmHandedness actual;
            return TryGet(kind, out actual) && actual == required;
        }
    }
}
