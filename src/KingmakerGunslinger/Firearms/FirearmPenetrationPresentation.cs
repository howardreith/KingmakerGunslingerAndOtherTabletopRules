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
            string rule = definition.Era == FirearmEra.Advanced
                ? string.Format(CultureInfo.InvariantCulture,
                    "Penetration: Touch AC within the first five range increments ({0:0} ft. base); Normal AC beyond.",
                    range)
                : string.Format(CultureInfo.InvariantCulture,
                    "Penetration: Touch AC within the first range increment ({0:0} ft. base); Normal AC beyond.",
                    range);
            if (definition.Kind == FirearmKind.Blunderbuss)
                rule += " This applies to ordinary direct fire; Scatter Shot retains its separate cone rules.";
            return rule;
        }
    }
}
