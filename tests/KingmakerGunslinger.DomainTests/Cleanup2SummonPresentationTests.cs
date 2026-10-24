using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Cleanup2SummonPresentationTests
    {
        internal static void ExplicitLabelsCoverPublishedRoutesAndKeepNaturalNames()
        {
            SummonChoicePresentationPolicy.Validate();
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)))
            {
                string key = variant.StableKey;
                string name = variant.Creature.DisplayName;
                string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(variant);
                SummonChoicePresentation choice = SummonChoicePresentationPolicy.For(
                    variant.Creature.Key, variant.Multiplicity);
                Assertions.Equal(variant.Creature.Key, choice.Key,
                    "Choice presentation lost its explicit creature association.");
                Assertions.Equal(variant.Multiplicity, choice.Quantity,
                    "Presentation changed quantity metadata.");
                Assertions.True(choice.Label.StartsWith(choice.Family,
                    StringComparison.Ordinal), "A choice lost its family-first label.");
                Assertions.Equal(key, variant.StableKey, "Presentation changed saved identity.");
                Assertions.Equal(symbol, ExpandedSummoningIdentityCatalog.AbilitySymbol(variant),
                    "Presentation changed blueprint symbol.");
                Assertions.Equal(name, variant.Creature.DisplayName,
                    "Menu presentation changed natural-language creature identity.");
            }
            foreach (SummonNativeExpansionSpec native in SummonNativeExpansionCatalog.All)
                Assertions.Equal(native.Multiplicity, SummonChoicePresentationPolicy.For(
                    native.IconKey, native.Multiplicity).Quantity,
                    "Retained-native wrapper lacks shared presentation metadata.");
            Assertions.Equal("Small Fire Elemental", ExpandedSummoningCatalog.All
                .Single(value => value.Key == "small-fire-elemental").DisplayName,
                "Natural description input must keep its ordinary word order.");
            Assertions.Throws<InvalidOperationException>(() =>
                SummonChoicePresentationPolicy.For("unpublished-invented-species"),
                "A new species requires a deliberate presentation mapping.");
        }

        internal static void FamilySubtypeSizeTemplateAndQuantityComposeIndependently()
        {
            var expected = new Dictionary<string, string> {
                { "air-mephit", "Mephit, Air" }, { "steam-mephit", "Mephit, Steam" },
                { "small-fire-elemental", "Elemental, Fire, Small" },
                { "large-fire-elemental", "Elemental, Fire, Large" },
                { "small-air-elemental", "Elemental, Air, Small" },
                { "giant-wasp", "Wasp, Giant" }, { "dire-crocodile", "Crocodile, Dire" },
                { "giant-crab", "Crab, Giant" }, { "giant-stag-beetle", "Beetle, Stag, Giant" },
                { "fire-beetle", "Beetle, Fire" }, { "hill-giant", "Giant, Hill" },
                { "storm-giant", "Giant, Storm" }, { "frost-giant", "Giant, Frost" },
                { "dire-rat", "Rat, Dire" }, { "woolly-rhinoceros", "Rhinoceros, Woolly" },
                { "lantern-archon", "Archon, Lantern" }, { "dire-tiger", "Smilodon" },
                { "giant-ant-worker", "Ant, Giant (Worker)" },
                { "horse", "Horse" }, { "girallon", "Girallon" }, { "ettin", "Ettin" },
                { "griffon", "Griffon" }, { "gorgon", "Gorgon" }, { "elephant", "Elephant" },
                { "brachiosaurus", "Brachiosaurus" }, { "stegosaurus", "Stegosaurus" }
            };
            foreach (KeyValuePair<string, string> pair in expected)
                Assertions.Equal(pair.Value, SummonChoicePresentationPolicy.For(pair.Key).Label,
                    "Requested family mapping differs: " + pair.Key);
            SummonChoicePresentation celestial = SummonChoicePresentationPolicy.For(
                "dire-rat", SummonMultiplicity.OneD3, SummonChoiceTemplate.Celestial);
            Assertions.Equal("Rat, Dire (Celestial) (1d3)", celestial.Label,
                "Template and quantity distinctions must retain the leading family.");
            Assertions.Equal("Rat", celestial.Family, "Template polluted sort family.");
            Assertions.Equal("Dire", celestial.Subtype, "Template polluted subtype.");
            Assertions.Equal(SummonChoiceTemplate.Celestial, celestial.Template,
                "Template metadata was lost.");
            Assertions.Equal("Horse (Fiendish) (1d4+1)",
                SummonChoicePresentationPolicy.For("horse", SummonMultiplicity.OneD4PlusOne,
                    SummonChoiceTemplate.Fiendish).Label,
                "Largest quantity band or fiendish distinction changed.");
            string[] sizes = { "small", "medium", "large", "huge", "greater", "elder" };
            Assertions.True(sizes.Select(size => SummonChoicePresentationPolicy.For(
                size + "-fire-elemental").SizeRank).SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }),
                "Future elemental sorting must use semantic size rank.");
            Assertions.Equal("Wasp", SummonChoicePresentationPolicy.For("giant-wasp").Family,
                "Giant Wasp must not enter the humanoid Giant family.");
        }

        internal static void EveryLegacySourceRequiresCompleteBehaviorCoverage()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(SummonVisibilityCatalog.IsPublished).ToArray();
            SummonNativeExpansionSpec[] native = SummonNativeExpansionCatalog.All.ToArray();
            foreach (SummonNativeOptionSpec original in SummonNativeOptionCatalog.All)
            {
                Assertions.True(SummonNativeOptionCatalog.ReplacementCoverageIsComplete(
                    original, all, native), "Legacy option has no complete replacement: " + original.Guid);
                foreach (string required in SummonNativeOptionCatalog.RequiredGeneratedCreatureKeys(original))
                    Assertions.False(SummonNativeOptionCatalog.ReplacementCoverageIsComplete(original,
                        all.Where(value => value.Creature.Key != required), native),
                        "A source was hidden with its generated replacement absent: " + original.Guid);
                foreach (SummonNativeExpansionSpec required in native.Where(value =>
                    value.Family == original.Family && value.Tier == original.Tier &&
                    value.SourceAbilityGuid == original.Guid && !value.ReplacesSpawnUnit))
                    Assertions.False(SummonNativeOptionCatalog.ReplacementCoverageIsComplete(original,
                        all, native.Where(value => value.Symbol != required.Symbol)),
                        "A source was hidden with its native creature wrapper absent: " + original.Guid);
            }

            SummonNativeOptionSpec mastodon = SummonNativeOptionCatalog.All.Single(value =>
                value.Family == SummonFamily.NaturesAlly && value.Tier == 7 &&
                value.Multiplicity == SummonMultiplicity.One);
            Assertions.False(SummonNativeOptionCatalog.ReplacementCoverageIsComplete(mastodon,
                all.Where(value => value.Creature.Key != "mastodon"), native),
                "An added Frost Giant cannot justify removing an uncovered Mastodon.");
            SummonNativeOptionSpec redcap = SummonNativeOptionCatalog.Find(
                SummonFamily.Monster, 5, "0964bf88b582bed41b74e79596c4f6d9");
            Assertions.False(SummonNativeOptionCatalog.ReplacementCoverageIsComplete(redcap,
                all.Where(value => value.Creature.Key != "bralani-azata"), native),
                "Redcap alone cannot cover the native umbrella's non-evil Bralani branch.");
            SummonNativeOptionSpec thanadaemon = SummonNativeOptionCatalog.Find(
                SummonFamily.Monster, 9, "e96593e67d206ab49ad1b567327d1e75");
            Assertions.False(SummonNativeOptionCatalog.ReplacementCoverageIsComplete(thanadaemon,
                all.Where(value => value.Creature.Key != "ghaele-azata"), native),
                "Thanadaemon alone cannot cover the native Ghaele branch.");
            SummonNativeOptionSpec mite = SummonNativeOptionCatalog.All.Single(value =>
                value.Family == SummonFamily.NaturesAlly && value.Tier == 1);
            Assertions.False(SummonNativeOptionCatalog.ReplacementCoverageIsComplete(mite,
                all, native.Where(value => value.IconKey != "mite")),
                "The SNA I preservation clone must remain if its Mite wrapper is absent.");
            Assertions.Throws<InvalidOperationException>(() =>
                SummonNativeOptionCatalog.ReplacementCoverageIsComplete(mastodon,
                    all.Concat(all.Where(value => value.Creature.Key == "mastodon")), native),
                "Ambiguous replacements must fail before publication.");
        }

        internal static void RepeatPublicationKeepsForeignChoicesAndRejectsIdentityConflicts()
        {
            var foreignA = new Fake("foreign-a"); var foreignB = new Fake("foreign-b");
            var known = new Fake("owned");
            IList<Fake> originals = SummonVariantMergePolicy.UnownedOriginals(
                new[] { foreignA, known, foreignB }, new[] { known }, value => value.Guid);
            Assertions.True(originals.SequenceEqual(new[] { foreignA, foreignB }),
                "Repeat publication must keep unknown third-party references and order.");
            IReadOnlyList<Fake> first = SummonDisplayOrderPolicy.Order(originals,
                new[] { known }, value => (SummonMultiplicity?)null,
                value => SummonMultiplicity.One);
            IList<Fake> again = SummonVariantMergePolicy.UnownedOriginals(first,
                new[] { known }, value => value.Guid);
            IReadOnlyList<Fake> second = SummonDisplayOrderPolicy.Order(again,
                new[] { known }, value => (SummonMultiplicity?)null,
                value => SummonMultiplicity.One);
            Assertions.True(SummonVariantMergePolicy.SameReferences(first.ToArray(), second.ToArray()),
                "Repeated publication changed choices or third-party placement.");
            Assertions.Throws<InvalidOperationException>(() =>
                SummonVariantMergePolicy.UnownedOriginals(new[] { new Fake("owned") },
                    new[] { known }, value => value.Guid),
                "Matching GUID on an unrelated object must not be silently claimed.");
            Assertions.Throws<InvalidOperationException>(() =>
                SummonVariantMergePolicy.UnownedOriginals(new[] { foreignA, foreignA },
                    new[] { known }, value => value.Guid),
                "Malformed original choices must fail closed.");
        }

        private sealed class Fake
        {
            internal Fake(string guid) { Guid = guid; }
            internal string Guid;
        }
    }
}
