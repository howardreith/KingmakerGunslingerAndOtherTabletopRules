using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    // This is an observer request scope, never a publication or gameplay roster.
    internal static class Cleanup2CreatureReviewRoster
    {
        private static readonly string[] WorkingKeys = {
            "giant-crab", "griffon", "giant-stag-beetle", "girallon", "ettin", "elephant",
            "stegosaurus", "hill-giant", "stone-giant", "fire-giant", "cloud-giant",
            "storm-giant", "brachiosaurus", "gorgon", "deinonychus"
        };

        internal const string FrostDestination = "frost-giant is a retained native option, not a generated creature-review key. " +
            "Use disposable-expanded-summoning-cleanup2-native-review with reviewPhase=giant-lineup " +
            "for the existing Nature's Ally VII and Summon Monster VIII Frost routes.";

        internal static IReadOnlyList<string> WorkingSaveKeys { get { return Array.AsReadOnly(WorkingKeys); } }

        internal static string[] ValidateWorkingSaveRequest(IEnumerable<string> keys)
        {
            if (keys == null) throw new InvalidOperationException("An explicit nonempty bounded creature selection is required.");
            string[] result = keys.ToArray();
            // Inspect the whole request before returning any executable subset.
            if (result.Contains("frost-giant", StringComparer.Ordinal)) throw new InvalidOperationException(FrostDestination);
            if (result.Length < 1 || result.Length > WorkingKeys.Length ||
                result.Distinct(StringComparer.Ordinal).Count() != result.Length ||
                result.Any(key => !WorkingKeys.Contains(key, StringComparer.Ordinal)))
                throw new InvalidOperationException("Creature keys must be exact unique mission-owned strings.");
            foreach (string key in result) ResolveGeneratedVariant(key, SummonMultiplicity.One);
            return result;
        }

        // Shared with the native observer. Keep its existing crowd and publication
        // checks at the caller; this selects the actual generated catalog object.
        internal static SummonVariantSpec ResolveGeneratedVariant(string key, SummonMultiplicity quantity)
        {
            if (quantity != SummonMultiplicity.One && quantity != SummonMultiplicity.OneD4PlusOne)
                throw new InvalidOperationException("Unsupported creature-review quantity: " + quantity + ".");
            if (key == "frost-giant") throw new InvalidOperationException(FrostDestination);
            SummonCreatureSpec creature = ExpandedSummoningCatalog.All.SingleOrDefault(value => value.Key == key);
            if (creature == null) throw new InvalidOperationException("Unknown creature key for review: " + key + ".");
            SummonFamily family = creature.NaturesAllyTier.HasValue ? SummonFamily.NaturesAlly : SummonFamily.Monster;
            int tier = family == SummonFamily.NaturesAlly ? creature.NaturesAllyTier.Value : creature.MonsterTier.Value;
            if (quantity == SummonMultiplicity.OneD4PlusOne) tier += 2;
            return ExpandedSummoningCatalog.GenerateVariants(family).Single(value =>
                value.Creature.Key == key && value.ParentTier == tier && value.Multiplicity == quantity);
        }
    }
}
