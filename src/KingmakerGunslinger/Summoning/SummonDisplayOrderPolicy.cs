using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>How one menu row sorts. Presentation only, never an identity.</summary>
    internal sealed class SummonMenuOrderKey
    {
        private SummonMenuOrderKey(string name, SummonMultiplicity? quantity,
            string identity, bool isCreatureChoice)
        {
            Name = name; Quantity = quantity; Identity = identity;
            IsCreatureChoice = isCreatureChoice;
        }

        internal string Name { get; private set; }
        internal SummonMultiplicity? Quantity { get; private set; }
        internal string Identity { get; private set; }
        internal bool IsCreatureChoice { get; private set; }

        /// <summary>A row this package names through its presentation metadata.</summary>
        internal static SummonMenuOrderKey Owned(SummonChoicePresentation presentation,
            string identity)
        {
            if (presentation == null) throw new ArgumentNullException("presentation");
            return new SummonMenuOrderKey(presentation.CreatureName, presentation.Quantity,
                RequireIdentity(identity), true);
        }

        /// <summary>A native or other-mod row, ordered by the name it already
        /// shows. A row not recognized as a creature choice, or with no visible
        /// name, is reported as unclassified and is never guessed at.</summary>
        internal static SummonMenuOrderKey Host(string visibleName,
            SummonMultiplicity? quantity, string identity, bool recognizedSummon)
        {
            return new SummonMenuOrderKey(visibleName, quantity, RequireIdentity(identity),
                recognizedSummon && !string.IsNullOrWhiteSpace(visibleName));
        }

        private static string RequireIdentity(string identity)
        {
            if (string.IsNullOrWhiteSpace(identity))
                throw new ArgumentException("A menu row needs a stable identity.", "identity");
            return identity;
        }
    }

    /// <summary>
    /// The one summon menu order (owner decision of 2026-11-10, superseding the
    /// earlier name-first rule): base multiplicity first (every single summon,
    /// then every 1d3, then every 1d4+1), then the displayed family-first
    /// creature name, then identity. The multiplicity is the row's base
    /// metadata, never a rolled or feat-increased count. A creature row whose
    /// base multiplicity is unknown follows the three groups, alphabetized
    /// among its kind. Ordinal comparison keeps it independent of the
    /// machine's culture. Unclassified rows follow the creature choices in
    /// their existing relative order.
    /// </summary>
    internal static class SummonDisplayOrderPolicy
    {
        private sealed class KeyComparer : IComparer<SummonMenuOrderKey>
        {
            internal static readonly KeyComparer Instance = new KeyComparer();

            public int Compare(SummonMenuOrderKey x, SummonMenuOrderKey y)
            {
                int result = QuantityRank(x.Quantity).CompareTo(QuantityRank(y.Quantity));
                if (result != 0) return result;
                result = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
                return result != 0 ? result :
                    string.Compare(x.Identity, y.Identity, StringComparison.Ordinal);
            }

            private static int QuantityRank(SummonMultiplicity? quantity)
            {
                if (!quantity.HasValue) return 3;
                return quantity.Value == SummonMultiplicity.One ? 0 :
                    quantity.Value == SummonMultiplicity.OneD3 ? 1 : 2;
            }
        }

        /// <summary>A new ordered array when the live rows are out of order,
        /// or null when nothing needs to change. The input is never modified.</summary>
        internal static T[] ReorderedOrNull<T>(T[] current, Func<T, SummonMenuOrderKey> orderKey)
            where T : class
        {
            if (current == null || orderKey == null) throw new ArgumentNullException();
            T[] ordered = Order(current, new T[0], orderKey).ToArray();
            return SummonVariantMergePolicy.SameReferences(current, ordered) ? null : ordered;
        }

        internal static IReadOnlyList<T> Order<T>(IEnumerable<T> originals,
            IEnumerable<T> additions, Func<T, SummonMenuOrderKey> orderKey)
        {
            if (originals == null || additions == null || orderKey == null)
                throw new ArgumentNullException();
            T[] added = additions.ToArray();
            var entries = originals.Concat(added).Select(value =>
                new { Value = value, Key = orderKey(value) }).ToArray();
            if (entries.Any(value => value.Key == null))
                throw new InvalidOperationException("A summon menu row has no order key.");
            if (added.Any(value => !orderKey(value).IsCreatureChoice))
                throw new InvalidOperationException("An owned summon choice must be a creature choice.");
            var result = new List<T>(entries.Where(value => value.Key.IsCreatureChoice)
                .OrderBy(value => value.Key, KeyComparer.Instance).Select(value => value.Value));
            result.AddRange(entries.Where(value => !value.Key.IsCreatureChoice)
                .Select(value => value.Value));
            return result.AsReadOnly();
        }
    }
}
