using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    internal sealed class CraftMagicItemsQualificationCheck
    {
        internal CraftMagicItemsQualificationCheck(string name,
            string expected, string observed, bool passed, string evidence)
        {
            Name = name ?? string.Empty;
            Expected = expected ?? string.Empty;
            Observed = observed ?? string.Empty;
            Passed = passed;
            Evidence = evidence ?? string.Empty;
        }

        internal string Name { get; private set; }
        internal string Expected { get; private set; }
        internal string Observed { get; private set; }
        internal bool Passed { get; private set; }
        internal string Evidence { get; private set; }
    }

    internal sealed class CraftMagicItemsQualificationResult
    {
        internal CraftMagicItemsQualificationResult(
            IEnumerable<CraftMagicItemsQualificationCheck> checks,
            IEnumerable<string> diagnostics, int initialGeneration,
            int finalizationGeneration, IEnumerable<string> customBlueprintGuids)
        {
            Checks = (checks ?? new CraftMagicItemsQualificationCheck[0])
                .ToArray();
            Diagnostics = (diagnostics ?? new string[0]).ToArray();
            InitialGeneration = initialGeneration;
            FinalizationGeneration = finalizationGeneration;
            CustomBlueprintGuids = (customBlueprintGuids ?? new string[0])
                .ToArray();
        }

        internal CraftMagicItemsQualificationCheck[] Checks
        { get; private set; }
        internal string[] Diagnostics { get; private set; }
        internal int InitialGeneration { get; private set; }
        internal int FinalizationGeneration { get; private set; }
        internal string[] CustomBlueprintGuids { get; private set; }
        internal bool Passed
        { get { return Checks.Length > 0 && Checks.All(value => value.Passed); } }
    }
}
