using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal static class SummonIconCatalog
    {
        private static readonly Dictionary<string, string> Categories = Build();

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
                StringComparer.Ordinal).Any()) throw new InvalidOperationException(
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
    }
}
