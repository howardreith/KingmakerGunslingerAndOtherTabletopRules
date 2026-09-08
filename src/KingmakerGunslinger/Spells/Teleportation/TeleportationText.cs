using System;
using System.Collections.Generic;
using Kingmaker.Localization;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class TeleportationText
    {
        private static readonly Dictionary<string, LocalizedString> Strings = new Dictionary<string, LocalizedString>(StringComparer.Ordinal);
        internal static string Get(string key, string english)
        {
            LocalizedString value;
            if (!Strings.TryGetValue(key, out value)) {
                value = LocalizationService.Create("KMG.Teleportation.Context." + key, english);
                Strings.Add(key, value);
            }
            return value;
        }
    }
}
