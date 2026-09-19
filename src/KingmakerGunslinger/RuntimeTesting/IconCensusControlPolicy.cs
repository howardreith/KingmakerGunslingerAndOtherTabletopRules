using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class IconCensusControlPolicy
    {
        internal static bool IsControl(string scenario, bool exitAfterCompletion, JObject parameters) =>
            scenario == "icon-overhaul-visual-evidence" && exitAfterCompletion &&
            parameters != null && parameters.Count == 1 &&
            parameters["iconCensusControl"]?.Type == JTokenType.Boolean &&
            (bool)parameters["iconCensusControl"];
    }
}
