using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>One selectable bardic performance target (O01).</summary>
    internal sealed class FavoredClassPerformanceTarget
    {
        internal FavoredClassPerformanceTarget(string key, string title, string featureGuid, bool provider,
            params string[] areaGuids)
        {
            Key = key;
            Title = title;
            FeatureGuid = featureGuid;
            Provider = provider;
            AreaGuids = areaGuids;
        }

        /// <summary>Stable symbol segment.</summary>
        internal string Key { get; private set; }
        internal string Title { get; private set; }

        /// <summary>The performance feature the bard must already have.</summary>
        internal string FeatureGuid { get; private set; }

        /// <summary>Created by the optional provider (Call of the Wild) rather than the game.</summary>
        internal bool Provider { get; private set; }

        /// <summary>The persistent performance area(s) whose radius the counter widens.</summary>
        internal string[] AreaGuids { get; private set; }
    }

    /// <summary>
    /// The O01 target manifest: every bardic performance the read-only audit
    /// found as a persistent cylinder area around the bard (vanilla
    /// performances, the Thundercaller and Flame Dancer performances, and
    /// Call of the Wild's Song of Fiery Gaze and Court Bard performances).
    /// One-shot or targeted performances, masterpieces and Discordant Voice
    /// are owner decisions (docs/FAVORED-CLASS-TARGET-MANIFEST.md).
    /// </summary>
    internal static class FavoredClassPerformanceManifest
    {
        /// <summary>Kingmaker's Bard class.</summary>
        internal const string BardClassGuid = "772c83a25e2268e448e841dcd548235f";

        private static readonly FavoredClassPerformanceTarget[] Entries =
        {
            new FavoredClassPerformanceTarget("InspireCourage", "Inspire Courage",
                "acb4df34b25ca9043a6aba1a4c92bc69", false, "5d4308fa344af0243b2dd3b1e500b2cc"),
            new FavoredClassPerformanceTarget("InspireCompetence", "Inspire Competence",
                "6d3fcfab6d935754c918eb0e004b5ef7", false, "c08bd33a377d5014a81be94e33ec8ce4"),
            new FavoredClassPerformanceTarget("Fascinate", "Fascinate",
                "ddaec3a5845bc7d4191792529b687d65", false, "a4fc1c0798359974e99e1d790935501d"),
            new FavoredClassPerformanceTarget("DirgeOfDoom", "Dirge of Doom",
                "1d48ab2bded57a74dad8af3da07d313a", false, "4a15b95f8e173dc4fb56924fe5598dcf"),
            new FavoredClassPerformanceTarget("InspireGreatness", "Inspire Greatness",
                "9ae0f32c72f8df84dab023d1b34641dc", false, "23ddd38738bd1d84595f3cdbb8512873"),
            new FavoredClassPerformanceTarget("FrighteningTune", "Frightening Tune",
                "cfd8940869a304f4aa9077415f93febe", false, "55c526a79761a3c48a3cc974a09bfef7"),
            new FavoredClassPerformanceTarget("InspireHeroics", "Inspire Heroics",
                "199d6fa0de149d044a8ab622a542cc79", false, "1be964f750eea8748a76e92744746efb"),
            new FavoredClassPerformanceTarget("InciteRage", "Incite Rage",
                "35ac4bd7990fa0842bfc22e80665c2f9", false, "8426523287601104085d71d410a6fc42",
                "9c423eacfb7bb9f408757e651607e125", "d63dce0f272ba2d4aa13000470398d63"),
            new FavoredClassPerformanceTarget("StormCall", "Storm Call",
                "161db4d6c4a1f4640ab52c762e15c1af", false, "85c1ea0021ce2714f8559fb618bf7ff6"),
            new FavoredClassPerformanceTarget("FireDance", "Fire Dance",
                "3c10a0069e7f110499d2e810f4861a6e", false, "0bd2c3ff0012e6b468497461448174c7"),
            new FavoredClassPerformanceTarget("SongOfFieryGaze", "Song of Fiery Gaze",
                "edf5697b6ddc42fca14d20a03affd475", true, "b556833f0a0a45738863a02c78323fed"),
            new FavoredClassPerformanceTarget("Satire", "Satire",
                "867e67a274d94c44bf6859b810745b1d", true, "b1125eb8eae649bdb441f22e3c088535"),
            new FavoredClassPerformanceTarget("Mockery", "Mockery",
                "71a3c675a44d4a8a89c9a0840cb1d92a", true, "eeb9c36c16be45dda7604b6120d1ab88"),
            new FavoredClassPerformanceTarget("GloriousEpic", "Glorious Epic",
                "d78e50c8ec9c436c82d3be6a028b4572", true, "0d961603708c4db3abf178e26d32fb1b"),
            new FavoredClassPerformanceTarget("Scandal", "Scandal",
                "88d2e41984ea4c68968197b44ec2f445", true, "164dba1be13048eab380b302e4f25b7e"),
        };

        private static readonly Dictionary<string, string> KeysByArea = Entries
            .SelectMany(target => target.AreaGuids.Select(area => new KeyValuePair<string, string>(area, target.Key)))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        internal static IList<FavoredClassPerformanceTarget> All
        {
            get { return Array.AsReadOnly(Entries); }
        }

        internal static FavoredClassPerformanceTarget For(string key)
        {
            return Entries.Single(value => string.Equals(value.Key, key, StringComparison.Ordinal));
        }

        /// <summary>The target a performance area belongs to, or null for any other area.</summary>
        internal static string KeyForArea(string areaGuid)
        {
            string key;
            return areaGuid != null && KeysByArea.TryGetValue(areaGuid, out key) ? key : null;
        }
    }
}
