using System;
using System.Collections.Generic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Audio
{
    internal static class FirearmSoundEventCatalog
    {
        internal const string BankName = "KMG_Firearms";
        internal const string BankFileName = "KMG_Firearms.bnk";
        internal const string Platform = "Windows";
        private static readonly IDictionary<FirearmKind, string> Events = new Dictionary<FirearmKind, string>
        {
            { FirearmKind.Pistol, "KMG_Firearm_Pistol_Shot" }, { FirearmKind.Musket, "KMG_Firearm_Musket_Shot" },
            { FirearmKind.Blunderbuss, "KMG_Firearm_Blunderbuss_Shot" }, { FirearmKind.Revolver, "KMG_Firearm_Revolver_Shot" },
            { FirearmKind.Rifle, "KMG_Firearm_Rifle_Shot" }
        };
        internal static IEnumerable<KeyValuePair<FirearmKind, string>> All { get { return Events; } }
        internal static bool TryResolve(FirearmKind kind, out string eventName) { return Events.TryGetValue(kind, out eventName) && !string.IsNullOrWhiteSpace(eventName); }
        internal static string Resolve(FirearmKind kind) { string value; if (!TryResolve(kind, out value)) throw new ArgumentOutOfRangeException("kind"); return value; }
    }
}
