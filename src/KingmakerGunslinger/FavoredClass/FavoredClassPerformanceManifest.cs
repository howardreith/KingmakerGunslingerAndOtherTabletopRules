using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>One bardic performance target of O01 and its exact native read points.</summary>
    internal sealed class FavoredClassPerformanceTarget
    {
        internal FavoredClassPerformanceTarget(string key, string title, string featureGuid, bool provider,
            int baseFeet, string[] toggleGuids, string[] areaGuids, string ringAssetId, string exclusion)
        {
            Key = key;
            Title = title;
            FeatureGuid = featureGuid;
            Provider = provider;
            BaseFeet = baseFeet;
            ToggleGuids = toggleGuids;
            AreaGuids = areaGuids;
            RingAssetId = ringAssetId;
            Exclusion = exclusion;
        }

        /// <summary>Stable symbol segment.</summary>
        internal string Key { get; private set; }
        internal string Title { get; private set; }

        /// <summary>The performance feature the bard must already have (its description shows the range).</summary>
        internal string FeatureGuid { get; private set; }

        /// <summary>Created by the optional provider (Call of the Wild) rather than the game.</summary>
        internal bool Provider { get; private set; }

        /// <summary>The native radius: the area blueprints' Size, in feet (never written).</summary>
        internal int BaseFeet { get; private set; }

        /// <summary>The performance's toggles (their descriptions show the range).</summary>
        internal string[] ToggleGuids { get; private set; }

        /// <summary>The persistent performance areas whose per-instance radius the counter widens.</summary>
        internal string[] AreaGuids { get; private set; }

        /// <summary>
        /// The area's ring effect prefab (<c>BlueprintAbilityAreaEffect.Fx</c>), authored at the base
        /// radius; null when the provider's link resolves to no effect.
        /// </summary>
        internal string RingAssetId { get; private set; }

        /// <summary>Why the registered counter is never published; null for a published target.</summary>
        internal string Exclusion { get; private set; }

        internal bool Published
        {
            get { return Exclusion == null; }
        }
    }

    /// <summary>A bardic performance that is not an O01 target, and why.</summary>
    internal sealed class FavoredClassPerformanceNonTarget
    {
        internal FavoredClassPerformanceNonTarget(string title, string featureGuid, string reason)
        {
            Title = title;
            FeatureGuid = featureGuid;
            Reason = reason;
        }

        internal string Title { get; private set; }
        internal string FeatureGuid { get; private set; }
        internal string Reason { get; private set; }
    }

    /// <summary>
    /// The O01 target manifest, classified by mechanics (charter 8.7). A
    /// published target is a maintained performance whose effect is a
    /// persistent cylinder area around the bard: its per-instance radius, its
    /// ring (an effect authored at the native radius) and its displayed
    /// description all follow the owner's investment. Two registered counters
    /// are never published because no truthful display exists, and one-shot,
    /// personal, masterpiece and add-on entries are not targets at all
    /// (docs/FAVORED-CLASS-TARGET-MANIFEST.md).
    /// </summary>
    internal static class FavoredClassPerformanceManifest
    {
        /// <summary>Kingmaker's Bard class.</summary>
        internal const string BardClassGuid = "772c83a25e2268e448e841dcd548235f";

        /// <summary>Feet added per earned step, and the steps' cap (+30 feet).</summary>
        internal const int FeetPerStep = 5;
        internal const int MaximumSteps = 6;

        private const string CourageRing = "2f93a2909cb766f4d961aee34a3c84c2";
        private const string CompetenceRing = "79665f3d500fdf44083feccf4cbfc00a";
        private const string DirgeRing = "20caf000cd4c3434da00a74f4a49dccc";

        private static readonly FavoredClassPerformanceTarget[] Entries =
        {
            T("InspireCourage", "Inspire Courage", "acb4df34b25ca9043a6aba1a4c92bc69", false, 50,
                new[] { "5250fe10c377fdb49be449dfe050ba70" }, new[] { "5d4308fa344af0243b2dd3b1e500b2cc" }, CourageRing),
            T("InspireCompetence", "Inspire Competence", "6d3fcfab6d935754c918eb0e004b5ef7", false, 30,
                new[] { "430ab3bb57f2cfc46b7b3a68afd4f74e" }, new[] { "c08bd33a377d5014a81be94e33ec8ce4" }, CompetenceRing),
            T("Fascinate", "Fascinate", "ddaec3a5845bc7d4191792529b687d65", false, 30,
                new[] { "993908ad3fb81f34ba0ed168b7c61f58" }, new[] { "a4fc1c0798359974e99e1d790935501d" },
                "725b02acb7286094688c0d5da974dcdc"),
            T("DirgeOfDoom", "Dirge of Doom", "1d48ab2bded57a74dad8af3da07d313a", false, 30,
                new[] { "d99d63f84e180d44e8f92b9a832c609d" }, new[] { "4a15b95f8e173dc4fb56924fe5598dcf" }, DirgeRing),
            T("InspireGreatness", "Inspire Greatness", "9ae0f32c72f8df84dab023d1b34641dc", false, 30,
                new[] { "be36959e44ac33641ba9e0204f3d227b" }, new[] { "23ddd38738bd1d84595f3cdbb8512873" },
                "3a0228650295f6a40bc335385a929a07"),
            T("FrighteningTune", "Frightening Tune", "cfd8940869a304f4aa9077415f93febe", false, 30,
                new[] { "ad8a93dfa2db7ac4e85133b5e4f14a5f" }, new[] { "55c526a79761a3c48a3cc974a09bfef7" }, DirgeRing),
            T("InspireHeroics", "Inspire Heroics", "199d6fa0de149d044a8ab622a542cc79", false, 30,
                new[] { "a4ce06371f09f504fa86fcf6d0e021e4" }, new[] { "1be964f750eea8748a76e92744746efb" }, CompetenceRing),
            T("InciteRage", "Incite Rage", "35ac4bd7990fa0842bfc22e80665c2f9", false, 30,
                new[] { "dbd7c54ba43e1d54592e037d63117f7b", "b1d8fdffd132bfd428a8045b7b8b363c",
                    "32d247b6e6b65794ab47fc372c444a96" },
                new[] { "8426523287601104085d71d410a6fc42", "9c423eacfb7bb9f408757e651607e125",
                    "d63dce0f272ba2d4aa13000470398d63" }, DirgeRing),
            X("StormCall", "Storm Call", "161db4d6c4a1f4640ab52c762e15c1af", false, 30,
                new[] { "d5ee8a2e5bf46c549988e9b09a59acd4" }, new[] { "85c1ea0021ce2714f8559fb618bf7ff6" }, DirgeRing,
                "its native description promises bolts on enemies within 50 feet, but its native area is 30 feet; " +
                "no owner range can be displayed truthfully without rewriting the native text"),
            T("FireDance", "Fire Dance", "3c10a0069e7f110499d2e810f4861a6e", false, 30,
                new[] { "1b28d456a5b1b4744a1d87cf24309ad1" }, new[] { "0bd2c3ff0012e6b468497461448174c7" }, CompetenceRing),
            T("SongOfFieryGaze", "Song of Fiery Gaze", "edf5697b6ddc42fca14d20a03affd475", true, 30,
                new[] { "6f528fdd236b464795546db489d10f3b" }, new[] { "b556833f0a0a45738863a02c78323fed" }, CompetenceRing),
            T("Satire", "Satire", "867e67a274d94c44bf6859b810745b1d", true, 50,
                new[] { "c23fed3a6e4b4caa82199a847d1b3fa5" }, new[] { "b1125eb8eae649bdb441f22e3c088535" }, CourageRing),
            X("Mockery", "Mockery", "71a3c675a44d4a8a89c9a0840cb1d92a", true, 30,
                new[] { "a608ff966e98442ba7718f290fc349ec" }, new[] { "eeb9c36c16be45dda7604b6120d1ab88" }, CompetenceRing,
                "its description is a single selected target with no range, while Call of the Wild implements a " +
                "30-foot area on every creature; widening that area would change a target-count rule, not a range"),
            T("GloriousEpic", "Glorious Epic", "d78e50c8ec9c436c82d3be6a028b4572", true, 30,
                new[] { "5fa0caff7bbe47399af61d16fb9620ab" }, new[] { "0d961603708c4db3abf178e26d32fb1b" }, DirgeRing),
            // Call of the Wild links Scandal's effect to an area GUID, so no
            // ring exists for any bard; its text states the range.
            T("Scandal", "Scandal", "88d2e41984ea4c68968197b44ec2f445", true, 50,
                new[] { "a13ad8cc3fc545278b41d652aa3c1ca9" }, new[] { "164dba1be13048eab380b302e4f25b7e" }, null),
            T("DanceOfTheDead", "Dance of the Dead", "92d80172888643328f1638a4293fb3d8", true, 50,
                new[] { "06c88e8ce5be4235bb61d0d1c2655655" }, new[] { "86e88e1394694fa6953e1bb82c76bc40" },
                "baa268c6db5723b4fa43c1b65f99bf0f"),
        };

        private static readonly FavoredClassPerformanceNonTarget[] NonTargetEntries =
        {
            N("Soothing Performance", "546698146e02d1e4ea00581a3ea7fe58",
                "instantaneous: a one-shot mass cure burst, not a maintained performance area"),
            N("Deadly Performance", "a6e13797b0a20d2458a086a8a511fd8c",
                "instantaneous: a one-shot single-target ability"),
            N("Thunder Call", "5ebb5d1f76b602d44818a21e9b6e31b8", "instantaneous: a one-shot burst ability"),
            N("Archaeologist's Luck", "03bf87dd753cd4f48a47eaf0ea6da9fe", "personal: a self buff with no range"),
            N("Blazing Rondo", "6e1f8dd4e17b41808e9f49e5a71dd9fc",
                "masterpiece: a Call of the Wild masterpiece feat shared with Skalds, not a Bard performance"),
            N("Banshee's Requiem", "22602cf4d9954ba2ae0223bcc27ff744",
                "masterpiece: a Call of the Wild masterpiece feat shared with Skalds, not a Bard performance"),
            N("Symphony of the Elysian Heart", "3dc71cb4cbaf467f8ba5c4dbab401603",
                "masterpiece: a Call of the Wild masterpiece feat shared with Skalds, not a Bard performance"),
            N("Clamor of the Heavens", "4f1a14d4d9314fd49a42ff2cef1b542b",
                "masterpiece: a Call of the Wild masterpiece feat shared with Skalds, not a Bard performance"),
            N("Triple Time", "b6e876fbe9a84eaaa80dc64c077dea49", "masterpiece and instantaneous"),
            N("Dance of 23 Steps", "a8059b931829424c902777b70cd6fa84", "masterpiece and personal"),
            N("Discordant Voice", "8064adc641c74e4cb821ce048ecd83a2",
                "inert: a feat that adds damage to other performances, not a performance"),
            N("Bardic Performance (Move Action)", "36931765983e96d4bb07ce7844cd897e", "inert: action economy only"),
            N("Bardic Performance (Swift Action)", "fd4ec50bc895a614194df6b9232004b9", "inert: action economy only"),
            N("Lingering Performance", "17239b298065efc459cffe2220ecb559", "inert: duration only"),
        };

        private static readonly Dictionary<string, string> KeysByArea = Entries.Where(target => target.Published)
            .SelectMany(target => target.AreaGuids.Select(area => new KeyValuePair<string, string>(area, target.Key)))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        private static readonly Dictionary<string, string> KeysByFact = Entries.Where(target => target.Published)
            .SelectMany(target => target.ToggleGuids.Concat(new[] { target.FeatureGuid })
                .Select(fact => new KeyValuePair<string, string>(fact, target.Key)))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        internal static IList<FavoredClassPerformanceTarget> All
        {
            get { return Array.AsReadOnly(Entries); }
        }

        internal static IList<FavoredClassPerformanceNonTarget> NonTargets
        {
            get { return Array.AsReadOnly(NonTargetEntries); }
        }

        internal static FavoredClassPerformanceTarget For(string key)
        {
            return Entries.Single(value => string.Equals(value.Key, key, StringComparison.Ordinal));
        }

        /// <summary>The published target a performance area belongs to, or null for any other area.</summary>
        internal static string KeyForArea(string areaGuid)
        {
            string key;
            return areaGuid != null && KeysByArea.TryGetValue(areaGuid, out key) ? key : null;
        }

        /// <summary>The published target whose feature or toggle this is, or null.</summary>
        internal static string KeyForFact(string blueprintGuid)
        {
            string key;
            return blueprintGuid != null && KeysByFact.TryGetValue(blueprintGuid, out key) ? key : null;
        }

        /// <summary>The owner's range in feet after its earned steps (capped).</summary>
        internal static int OwnerFeet(FavoredClassPerformanceTarget target, int steps)
        {
            return target.BaseFeet + FeetPerStep * Math.Max(0, Math.Min(MaximumSteps, steps));
        }

        private static FavoredClassPerformanceTarget T(string key, string title, string feature, bool provider,
            int baseFeet, string[] toggles, string[] areas, string ring)
        {
            return new FavoredClassPerformanceTarget(key, title, feature, provider, baseFeet, toggles, areas, ring,
                null);
        }

        private static FavoredClassPerformanceTarget X(string key, string title, string feature, bool provider,
            int baseFeet, string[] toggles, string[] areas, string ring, string exclusion)
        {
            return new FavoredClassPerformanceTarget(key, title, feature, provider, baseFeet, toggles, areas, ring,
                exclusion);
        }

        private static FavoredClassPerformanceNonTarget N(string title, string feature, string reason)
        {
            return new FavoredClassPerformanceNonTarget(title, feature, reason);
        }
    }
}
