using System;
using System.Globalization;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Rules
{
    internal static class FirearmArmorClassPresentation
    {
        internal static string Format(FirearmDefinition definition,
            double distanceMeters, double penetrationRangeFeet,
            bool usesTouchArmorClass, string reason)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (double.IsNaN(distanceMeters) || double.IsInfinity(distanceMeters) ||
                distanceMeters < 0d) throw new ArgumentOutOfRangeException("distanceMeters");
            if (double.IsNaN(penetrationRangeFeet) ||
                double.IsInfinity(penetrationRangeFeet) ||
                penetrationRangeFeet <= 0d)
                throw new ArgumentOutOfRangeException("penetrationRangeFeet");
            if (string.Equals(reason, "touch-ac-deadeye",
                    StringComparison.Ordinal))
                return "Firearm: Touch AC (Deadeye).";
            return string.Format(CultureInfo.InvariantCulture,
                "Firearm: {0} AC ({1:0.#} ft.).",
                usesTouchArmorClass ? "Touch" : "Normal",
                distanceMeters / FirearmArmorClassService.MetersPerFoot);
        }
    }
}
