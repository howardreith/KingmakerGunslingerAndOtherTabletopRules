using System;
using System.Globalization;
using KingmakerGunslinger.Rules;

namespace KingmakerGunslinger.Firearms
{
    internal static class FirearmPenetrationPresentation
    {
        internal static string Describe(FirearmDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            double range = FirearmPenetrationRangePolicy
                .EffectivePenetrationRangeFeet(definition, 0);
            string attacks = definition.Kind == FirearmKind.Blunderbuss
                ? "Attacks with a lead ball"
                : "Attacks with this firearm";
            string window = definition.Era == FirearmEra.Advanced
                ? "its first five range increments"
                : "its first range increment";
            return string.Format(CultureInfo.InvariantCulture,
                "{0} are resolved against touch AC out to {1:0} ft. ({2}), and against normal AC at greater distances.",
                attacks, range, window);
        }
    }
}
