using System;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint17Tests
    {
        internal static void RigSurveyUsesOnlyExactNativeSources()
        {
            Assertions.True(SerpentineRigSurveyPolicy.Keys.SequenceEqual(
                new[] { "medium-water-elemental", "salamander", "purple-worm" }),
                "Only the three fixed pre-existing rig carriers are in the research scope; no new publication.");
            string[] copy = SerpentineRigSurveyPolicy.Keys;
            copy[0] = "foreign";
            Assertions.Equal("medium-water-elemental", SerpentineRigSurveyPolicy.Keys[0],
                "Consumers cannot change the shared research target set.");
            foreach (string key in SerpentineRigSurveyPolicy.Keys)
            {
                string native = SerpentineRigSurveyPolicy.NativeBlueprint(key);
                string prefab = SerpentineRigSurveyPolicy.Prefab(key);
                Assertions.True(SerpentineRigSurveyPolicy.MatchesNativeSource(key, native, prefab),
                    "The recorded native blueprint/view pair is accepted.");
                foreach (string bad in new[] { "", null, "foreign", native.ToUpperInvariant(),
                    "6f5db07e89834a01aa9b7e7aed6cd407" })
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key, bad, prefab),
                        "A missing, changed or optional Eidolon source is never substituted.");
                foreach (string bad in new[] { "", null, "foreign", prefab.ToUpperInvariant() })
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key, native, bad),
                        "View resolution is exact, not a permissive arbitrary-asset loader.");
                foreach (string other in SerpentineRigSurveyPolicy.Keys.Where(value => value != key))
                {
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key,
                        SerpentineRigSurveyPolicy.NativeBlueprint(other), prefab),
                        "A different allowed donor cannot substitute for the selected source.");
                    Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key,
                        native, SerpentineRigSurveyPolicy.Prefab(other)),
                        "A different allowed view cannot substitute for the selected source.");
                }
            }
            foreach (string key in new[] { null, "", "viper", "constrictor-snake", "Salamander", "foreign" })
                Assertions.False(SerpentineRigSurveyPolicy.MatchesNativeSource(key, null, null),
                    "Unknown, new or differently cased keys fail closed.");
        }
    }
}
