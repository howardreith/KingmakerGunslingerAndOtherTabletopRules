using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class RuntimeTestScenarioCatalog
    {
        internal const string ModLoadSmoke = "mod-load-smoke";
        internal const string ObserveManualSaveLoad = "observe-manual-save-load";

        private static readonly HashSet<string> Allowed =
            new HashSet<string>(StringComparer.Ordinal)
            {
                ModLoadSmoke,
                ObserveManualSaveLoad
            };

        internal static bool IsAllowed(string scenario)
        {
            return scenario != null && Allowed.Contains(scenario);
        }

        internal static string[] Names()
        {
            var names = new string[Allowed.Count];
            Allowed.CopyTo(names);
            Array.Sort(names, StringComparer.Ordinal);
            return names;
        }
    }
}
