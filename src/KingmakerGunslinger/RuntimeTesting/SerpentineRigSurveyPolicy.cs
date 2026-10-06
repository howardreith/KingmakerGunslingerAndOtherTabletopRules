using System;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Fixed research targets, not a request-supplied asset loader.</summary>
    internal static class SerpentineRigSurveyPolicy
    {
        internal static string[] Keys { get { return new[] { "medium-water-elemental", "salamander" }; } }

        internal static string NativeBlueprint(string key)
        {
            if (key == "medium-water-elemental") return "62a3e860e6e72e6499c38bb8b2fe303e";
            if (key == "salamander") return "e8276e28b2234a745900fed80670bfdb";
            throw new ArgumentException("Not a Sprint 17 native rig survey target.", "key");
        }

        internal static string Prefab(string key)
        {
            if (key == "medium-water-elemental") return "dc296683c2a3d2648afa516aeb030fb8";
            if (key == "salamander") return "9b1744531a4428e44aa9837ca984513a";
            throw new ArgumentException("Not a Sprint 17 native rig survey target.", "key");
        }

        internal static bool MatchesNativeSource(string key, string blueprint, string prefab)
        {
            return Array.IndexOf(Keys, key) >= 0 &&
                blueprint == NativeBlueprint(key) && prefab == Prefab(key);
        }
    }
}
