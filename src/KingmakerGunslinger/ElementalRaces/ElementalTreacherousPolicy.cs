using System;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalTreacherousPolicy
    {
        internal const int RadiusFeet = 10;
        internal static int DurationMinutes(int totalLevel) { return Math.Max(1, totalLevel); }
        internal static bool Eligible(bool legalGroundContact, bool touchReachable,
            bool lineOfEffect, bool continuousGroundPath)
        {
            return legalGroundContact && touchReachable && lineOfEffect && continuousGroundPath;
        }
        // The owner delegated this CRPG adaptation. Enabling publication still
        // requires actual-ground and full native trait integration qualification.
        internal static bool EligibilityQualified { get { return false; } }
        internal const string Description = "Once per ordinary rest, as a standard action, touch a walkable ground location " +
            "to turn a 10-foot-radius patch into difficult terrain. The patch lasts 1 minute per total character level. " +
            "It deals no damage and allows no saving throw. This Kingmaker adaptation also works on built floors, " +
            "including wood and worked stone. The location must be within touch reach along an unblocked ground path.";
    }
}
