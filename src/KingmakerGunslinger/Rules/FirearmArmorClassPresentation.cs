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
            string qualification = string.Equals(reason, "touch-ac-deadeye",
                StringComparison.Ordinal) ? " (Deadeye)" : string.Empty;
            return string.Format(CultureInfo.InvariantCulture,
                "{0} attack: {1:0.#} ft.; penetration range {2:0.#} ft.; {3} AC{4}.",
                definition.Kind,
                distanceMeters / FirearmArmorClassService.MetersPerFoot,
                penetrationRangeFeet,
                usesTouchArmorClass ? "Touch" : "Normal",
                qualification);
        }
    }
}
