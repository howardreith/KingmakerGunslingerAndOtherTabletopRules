using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // These counters are already scalar evidence, not game objects. Never
    // send them through the active game's process-global save converters.
    internal static class Sprint17GripEvidence
    {
        internal static JObject Counters(IDictionary<string, int> counters)
        {
            if (counters == null) throw new ArgumentNullException("counters");
            return new JObject(counters.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new JProperty(pair.Key, pair.Value)));
        }
    }
}
