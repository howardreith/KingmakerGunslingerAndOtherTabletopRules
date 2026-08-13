using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum SummonIconSourceKind
    {
        UnitPortrait,
        Ability,
        Item
    }

    internal sealed class SummonIconSourceSpec
    {
        internal SummonIconSourceSpec(string creatureKey,
            SummonIconSourceKind kind, string sourceGuid)
        { CreatureKey = creatureKey; Kind = kind; SourceGuid = sourceGuid; }
        internal string CreatureKey { get; private set; }
        internal SummonIconSourceKind Kind { get; private set; }
        internal string SourceGuid { get; private set; }
    }

    internal static class SummonIconCatalog
    {
        private static readonly Dictionary<string, string> Categories = Build();
        private static readonly SummonIconSourceSpec[] ExactSources = {
            I("dog", SummonIconSourceKind.UnitPortrait,
                "77f3f2ddf1ec2da45ab956c433e3b557"),
            I("wolf", SummonIconSourceKind.UnitPortrait,
                "76597216769b0d540aafafa07edf0cec"),
            I("hyena", SummonIconSourceKind.UnitPortrait,
                "1491a6e0a13aa734ebbf720a29379e0a"),
            I("goblin-dog", SummonIconSourceKind.UnitPortrait,
                "313a17cbd273d1f40bd1654ee2ae186e"),
            I("wolverine", SummonIconSourceKind.UnitPortrait,
                "161338cf5d3c43b4aa7ebc1c9ef1eff3"),
            I("leopard", SummonIconSourceKind.UnitPortrait,
                "768275c9885dd954fb3c84ba69ac4281"),
            I("cheetah", SummonIconSourceKind.UnitPortrait,
                "54cf380dee486ff42b803174d1b9da1b"),
            I("monitor-lizard", SummonIconSourceKind.UnitPortrait,
                "4109b40f6bbb49640840644cc84ada67"),
            I("crocodile", SummonIconSourceKind.UnitPortrait,
                "331b3e572834f9f4394e27517a1b53a3"),
            I("lion", SummonIconSourceKind.UnitPortrait,
                "8a6986e17799d7d4b90f0c158b31c5b9"),
            I("dire-lion", SummonIconSourceKind.UnitPortrait,
                "beae4985629a6f64eb98081e3171e4c1"),
            I("dire-tiger", SummonIconSourceKind.Ability,
                "32f1f208ad635224f89ef158140ab509"),
            I("pteranodon", SummonIconSourceKind.Ability,
                "332ad68273db9704ab0e92518f2efd1c"),
            I("lantern-archon", SummonIconSourceKind.Ability,
                "90e59f4a4ada87243b7b3535a06d0638"),
            I("bralani-azata", SummonIconSourceKind.Ability,
                "332ad68273db9704ab0e92518f2efd1c"),
            I("erinyes-devil", SummonIconSourceKind.Item,
                "201f6150321e09048bd59e9b7f558cb0"),
            I("ghaele-azata", SummonIconSourceKind.Ability,
                "397510bf6f325034ab2a75149ba8632f")
        };

        internal static IReadOnlyList<SummonIconSourceSpec> Sources
        { get { return Array.AsReadOnly(ExactSources); } }

        internal static SummonIconSourceSpec SourceFor(string creatureKey)
        { return ExactSources.SingleOrDefault(value => value.CreatureKey ==
            creatureKey); }

        internal static string CategoryFor(string creatureKey)
        {
            string result;
            if (creatureKey == null || !Categories.TryGetValue(creatureKey,
                out result)) throw new InvalidOperationException(
                    "Summon icon catalog lacks " + creatureKey + ".");
            return result;
        }

        internal static string RepresentativeFor(string category)
        {
            return Categories.First(value => value.Value == category).Key;
        }

        internal static void Validate()
        {
            string[] expected = ExpandedSummoningCatalog.All.Select(value =>
                value.Key).ToArray();
            if (Categories.Count != 67 || Categories.Keys.Except(expected,
                StringComparer.Ordinal).Any() || expected.Except(Categories.Keys,
                StringComparer.Ordinal).Any() || ExactSources.Length != 17 ||
                ExactSources.Select(value => value.CreatureKey).Distinct(
                    StringComparer.Ordinal).Count() != ExactSources.Length ||
                ExactSources.Any(value => !Categories.ContainsKey(
                    value.CreatureKey) || value.SourceGuid == null ||
                    value.SourceGuid.Length != 32)) throw new InvalidOperationException(
                    "Immutable summon icon category catalog is incomplete.");
        }

        private static Dictionary<string, string> Build()
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            Add(result, "canine", "dog", "wolf", "goblin-dog", "hyena",
                "wolverine", "dire-wolf", "hell-hound");
            Add(result, "feline", "leopard", "cheetah", "lion", "dire-lion",
                "dire-tiger");
            Add(result, "bear", "grizzly-bear", "dire-bear");
            Add(result, "bird", "eagle", "dire-bat", "pteranodon", "roc");
            Add(result, "amphibian", "poisonous-frog", "giant-frog");
            Add(result, "reptile", "monitor-lizard", "crocodile",
                "ankylosaurus", "salamander");
            Add(result, "vermin", "giant-centipede", "giant-spider",
                "bebelith");
            Add(result, "boar", "boar", "dire-boar");
            Add(result, "elephant", "elephant", "mastodon");
            Add(result, "air-elemental", "small-air-elemental",
                "medium-air-elemental", "large-air-elemental",
                "huge-air-elemental", "greater-air-elemental",
                "elder-air-elemental", "invisible-stalker");
            Add(result, "earth-elemental", "small-earth-elemental",
                "medium-earth-elemental", "large-earth-elemental",
                "huge-earth-elemental", "greater-earth-elemental",
                "elder-earth-elemental");
            Add(result, "fire-elemental", "small-fire-elemental",
                "medium-fire-elemental", "large-fire-elemental",
                "huge-fire-elemental", "greater-fire-elemental",
                "elder-fire-elemental");
            Add(result, "water-elemental", "small-water-elemental",
                "medium-water-elemental", "large-water-elemental",
                "huge-water-elemental", "greater-water-elemental",
                "elder-water-elemental");
            Add(result, "air-mephit", "air-mephit");
            Add(result, "earth-mephit", "earth-mephit");
            Add(result, "fire-mephit", "fire-mephit");
            Add(result, "water-mephit", "water-mephit");
            Add(result, "celestial", "lantern-archon", "bralani-azata",
                "ghaele-azata");
            Add(result, "fiend", "erinyes-devil", "shadow-demon", "succubus");
            Add(result, "fey", "pixie");
            return result;
        }

        private static void Add(IDictionary<string, string> result,
            string category, params string[] keys)
        {
            foreach (string key in keys) result.Add(key, category);
        }

        private static SummonIconSourceSpec I(string key,
            SummonIconSourceKind kind, string guid)
        { return new SummonIconSourceSpec(key, kind, guid); }
    }
}
