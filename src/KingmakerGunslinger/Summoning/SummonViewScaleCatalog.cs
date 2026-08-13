using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class SummonViewScaleSpec
    {
        internal SummonViewScaleSpec(string creatureKey, float multiplier)
        { CreatureKey = creatureKey; Multiplier = multiplier; }
        internal string CreatureKey { get; private set; }
        internal float Multiplier { get; private set; }
        internal string BlueprintName { get { return
            "KMG_Summoning_Unit_" + Token(CreatureKey); } }

        private static string Token(string value)
        {
            string[] words = value.Split('-');
            return string.Concat(words.Select(word => char.ToUpperInvariant(
                word[0]) + word.Substring(1)));
        }
    }

    /// <summary>
    /// View-only, identity-scoped adjustments for proxy silhouettes. Mechanical
    /// size, reach, footprint, and the source donor prefab remain unchanged.
    /// </summary>
    internal static class SummonViewScaleCatalog
    {
        private static readonly SummonViewScaleSpec[] Values = {
            S("eagle", 0.58f),
            S("poisonous-frog", 0.48f),
            S("dire-boar", 1.15f),
            S("pteranodon", 0.82f),
            S("dire-bear", 1.15f),
            S("elephant", 0.90f),
            S("mastodon", 1.15f),
            S("roc", 1.10f)
        };

        internal static IReadOnlyList<SummonViewScaleSpec> All
        { get { return Array.AsReadOnly(Values); } }

        internal static bool TryGetMultiplier(string blueprintName,
            out float multiplier)
        {
            SummonViewScaleSpec match = Values.SingleOrDefault(value =>
                string.Equals(value.BlueprintName, blueprintName,
                    StringComparison.Ordinal));
            multiplier = match == null ? 1f : match.Multiplier;
            return match != null;
        }

        internal static void Validate()
        {
            if (Values.Length != 8 || Values.Any(value => value.Multiplier <
                    0.40f || value.Multiplier > 1.25f) ||
                Values.Select(value => value.CreatureKey).Distinct(
                    StringComparer.Ordinal).Count() != Values.Length ||
                Values.Any(value => !ExpandedSummoningCatalog.All.Any(
                    creature => creature.Key == value.CreatureKey)))
                throw new InvalidOperationException(
                    "Summon view-scale catalog is malformed.");
        }

        private static SummonViewScaleSpec S(string key, float multiplier)
        { return new SummonViewScaleSpec(key, multiplier); }
    }
}
