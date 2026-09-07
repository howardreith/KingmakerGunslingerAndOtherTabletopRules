using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalRespecResourcePolicy
    {
        // A live resource is authoritative over an older suppressed-provider
        // snapshot. An unseen identity receives no invented expenditure.
        internal static Dictionary<string, int> Capture(IEnumerable<string> owned,
            IDictionary<string, int> remembered, IDictionary<string, int> present)
        {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (string guid in owned)
            {
                int amount;
                if (present.TryGetValue(guid, out amount) || remembered.TryGetValue(guid, out amount))
                    result.Add(guid, Math.Max(0, amount));
            }
            return result;
        }

        internal static int Amount(int current, int remembered)
        {
            return Math.Min(Math.Max(0, current), Math.Max(0, remembered));
        }
    }
}
