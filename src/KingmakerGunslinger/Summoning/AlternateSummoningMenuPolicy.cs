using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>A native option's exact spawn signature, read from the live
    /// canonical child that the native option catalog names.</summary>
    internal sealed class AlternateSummoningLineageCandidate
    {
        internal AlternateSummoningLineageCandidate(string optionGuid,
            SummonMultiplicity multiplicity, IEnumerable<string> unitGuids)
        {
            if (string.IsNullOrWhiteSpace(optionGuid))
                throw new ArgumentException("A native option GUID is required.", "optionGuid");
            OptionGuid = optionGuid; Multiplicity = multiplicity;
            UnitGuids = AlternateSummoningLineagePolicy.Signature(unitGuids);
        }

        internal string OptionGuid { get; private set; }
        internal SummonMultiplicity Multiplicity { get; private set; }
        internal IReadOnlyList<string> UnitGuids { get; private set; }
    }

    /// <summary>
    /// Which canonical native option a host child descends from. A caller's
    /// child is a copy of a native child, so it spawns exactly the same set of
    /// units; that set, within the same tier and quantity, is the only
    /// evidence accepted. No match, or more than one, means "unknown", and an
    /// unknown option is always preserved.
    /// </summary>
    internal static class AlternateSummoningLineagePolicy
    {
        internal static IReadOnlyList<string> Signature(IEnumerable<string> unitGuids)
        {
            string[] values = (unitGuids ?? Enumerable.Empty<string>())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (values.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("A spawn signature contains an empty unit GUID.");
            return values;
        }

        internal static AlternateSummoningLineageCandidate Match(
            IEnumerable<string> hostUnitGuids, SummonMultiplicity? quantity,
            IEnumerable<AlternateSummoningLineageCandidate> candidates)
        {
            IReadOnlyList<string> host = Signature(hostUnitGuids);
            if (host.Count == 0 || candidates == null) return null;
            AlternateSummoningLineageCandidate[] matches = candidates.Where(value =>
                value != null && (!quantity.HasValue ||
                    value.Multiplicity == quantity.Value) &&
                value.UnitGuids.SequenceEqual(host, StringComparer.Ordinal))
                .ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }
    }

    internal sealed class AlternateSummoningMenuPlan<T> where T : class
    {
        internal AlternateSummoningMenuPlan(IReadOnlyList<T> final,
            IReadOnlyList<T> keptHost, IReadOnlyList<T> removedHost,
            IReadOnlyList<T> added, IReadOnlyList<string> replacedLineages)
        {
            Final = final; KeptHost = keptHost; RemovedHost = removedHost;
            Added = added; ReplacedLineages = replacedLineages;
        }

        internal IReadOnlyList<T> Final { get; private set; }
        internal IReadOnlyList<T> KeptHost { get; private set; }
        internal IReadOnlyList<T> RemovedHost { get; private set; }
        internal IReadOnlyList<T> Added { get; private set; }
        internal IReadOnlyList<string> ReplacedLineages { get; private set; }
    }

    /// <summary>
    /// Composes one adapter menu exactly the way the canonical publisher
    /// composes a spell parent: a host option that descends from a native
    /// umbrella the native expansion catalog splits is replaced by those
    /// split options; one that descends from an established semantic
    /// duplicate collapses only when exactly one published project creature
    /// replaces it at that quantity; everything else the host offers -
    /// including options from other mods - stays, by reference and order.
    /// </summary>
    internal static class AlternateSummoningMenuPolicy
    {
        internal static AlternateSummoningMenuPlan<T> Compose<T>(
            SummonFamily family, int tier, SummonMultiplicity? rootQuantity,
            IReadOnlyList<T> hostChildren,
            Func<T, string> hostLineage,
            Func<T, SummonMultiplicity?> hostQuantity,
            IReadOnlyList<AlternateSummoningChoice> exposedCreatures,
            Func<AlternateSummoningChoice, T> creatureChild,
            IEnumerable<SummonNativeExpansionSpec> nativeOptions,
            Func<SummonNativeExpansionSpec, T> nativeChild) where T : class
        {
            if (hostChildren == null) throw new ArgumentNullException("hostChildren");
            if (hostLineage == null || hostQuantity == null ||
                creatureChild == null || nativeChild == null)
                throw new ArgumentNullException();
            AlternateSummoningChoice[] creatures = (exposedCreatures ??
                new AlternateSummoningChoice[0]).Where(value =>
                    value.Kind == AlternateSummoningChoiceKind.Creature &&
                    value.Published && value.Tier == tier &&
                    value.Placement.Family == family &&
                    (!rootQuantity.HasValue || value.Multiplicity == rootQuantity.Value))
                .ToArray();
            var kept = new List<T>();
            var removed = new List<T>();
            var replaced = new List<string>();
            foreach (T host in hostChildren)
            {
                if (host == null) throw new InvalidOperationException(
                    "A host menu contains null.");
                string lineage = hostLineage(host);
                if (lineage == null) { kept.Add(host); continue; }
                if (SummonNativeExpansionCatalog.Replaces(family, tier, lineage))
                {
                    removed.Add(host);
                    if (!replaced.Contains(lineage)) replaced.Add(lineage);
                    continue;
                }
                SummonNativeOptionSpec native = SummonNativeOptionCatalog.Find(
                    family, tier, lineage);
                if (native == null || !native.IsSemanticDuplicate)
                { kept.Add(host); continue; }
                int matches = creatures.Count(value =>
                    value.Placement.Creature.Key == native.EquivalentCreatureKey &&
                    value.Multiplicity == native.Multiplicity);
                if (SummonVariantMergePolicy.PreserveNativeSemanticDuplicate(
                    matches, native.Guid)) kept.Add(host);
                else removed.Add(host);
            }
            var additions = new List<T>();
            var additionQuantity = new Dictionary<T, SummonMultiplicity>(
                ReferenceComparer<T>.Instance);
            foreach (AlternateSummoningChoice choice in creatures)
            {
                T child = creatureChild(choice);
                if (child == null) throw new InvalidOperationException(
                    "No adapter child for " + choice.Symbol + ".");
                additions.Add(child);
                additionQuantity.Add(child, choice.Multiplicity);
            }
            foreach (SummonNativeExpansionSpec native in (nativeOptions ??
                Enumerable.Empty<SummonNativeExpansionSpec>()).Where(value =>
                    value.Family == family && value.Tier == tier &&
                    replaced.Contains(value.SourceAbilityGuid) &&
                    (!rootQuantity.HasValue || value.Multiplicity == rootQuantity.Value)))
            {
                T child = nativeChild(native);
                if (child == null) throw new InvalidOperationException(
                    "No adapter child for native option " + native.Symbol + ".");
                additions.Add(child);
                additionQuantity.Add(child, native.Multiplicity);
            }
            IReadOnlyList<T> final = SummonDisplayOrderPolicy.Order(kept, additions,
                hostQuantity, value => additionQuantity[value]);
            if (final.Count != kept.Count + additions.Count ||
                final.Distinct(ReferenceComparer<T>.Instance).Count() != final.Count)
                throw new InvalidOperationException(
                    "Adapter menu composition lost or repeated an option.");
            return new AlternateSummoningMenuPlan<T>(final, kept.AsReadOnly(),
                removed.AsReadOnly(), additions.AsReadOnly(), replaced.AsReadOnly());
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
        {
            internal static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();
            public bool Equals(T x, T y) { return ReferenceEquals(x, y); }
            public int GetHashCode(T obj)
            { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
