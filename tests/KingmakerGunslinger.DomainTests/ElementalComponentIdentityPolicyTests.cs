using System;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalComponentIdentityPolicyTests
    {
        internal static void PreservesLegacyStateAndRejectsAmbiguity()
        {
            string[] keys = { "Controller", "AddFacts", "AddAbilityResources", "Parameters" };
            string[] original = { "", "", "", "" };
            bool[] state = { false, true, false, false };
            string[] names = ElementalComponentIdentityPolicy.Plan(original, keys, state);
            Assertions.Equal("", names[1], "Persisted AddFacts ownership retains the exact legacy empty key.");
            Assertions.Equal(4, names.Distinct(StringComparer.Ordinal).Count(), "Native SingleOrDefault has one match per key.");
            Assertions.True(original.All(value => value == ""), "Planning never mutates its inputs.");
            Assertions.True(names.SequenceEqual(ElementalComponentIdentityPolicy.Plan(names, keys, state)), "Replay preserves every exact name.");
            string[] statKeys = { "AddStatBonus.Strength", "AddStatBonus.Wisdom", "AddStatBonus.Charisma", "Controller" };
            string[] stats = ElementalComponentIdentityPolicy.Plan(original, statKeys, new bool[4]);
            Assertions.Equal("", stats[0], "A stateless legacy race retains a single empty alias.");
            Assertions.Equal(4, stats.Distinct(StringComparer.Ordinal).Count(), "Repeated native stat types have semantic stat-specific identities.");
            string[] named = { "native-owned-clone-name", "" };
            Assertions.True(named.SequenceEqual(ElementalComponentIdentityPolicy.Plan(named,
                new[] { "A", "B" }, new[] { true, false })), "Unique existing names remain byte-for-byte unchanged.");
            Assertions.Throws<InvalidOperationException>(() => ElementalComponentIdentityPolicy.Plan(
                new[] { "", "" }, new[] { "A", "B" }, new[] { true, true }), "Two stateful legacy aliases fail closed.");
            Assertions.Throws<InvalidOperationException>(() => ElementalComponentIdentityPolicy.Plan(
                new[] { "same", "same" }, new[] { "A", "B" }, new bool[2]), "A named collision needs explicit migration.");
            Assertions.Throws<InvalidOperationException>(() => ElementalComponentIdentityPolicy.Plan(
                new[] { "", "", "$KMG.Elemental.B" }, new[] { "A", "B", "C" }, new bool[3]), "Generated names cannot overwrite a preexisting key.");
            Assertions.Throws<ArgumentException>(() => ElementalComponentIdentityPolicy.Plan(
                new[] { "", "" }, new[] { "A", "A" }, new bool[2]), "Unstable indistinguishable semantic keys fail closed.");
            Assertions.Equal(0, ElementalComponentIdentityPolicy.Plan(new string[0], new string[0], new bool[0]).Length,
                "Component-free owned facts remain unchanged.");
        }
    }
}
