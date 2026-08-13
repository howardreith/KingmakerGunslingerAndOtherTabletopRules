using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum SummonProjectIconScope
    {
        KmgCatalog,
        SplitNative,
        PreservedNative
    }

    internal sealed class SummonProjectIconSpec
    {
        internal SummonProjectIconSpec(string key, string displayName,
            SummonProjectIconScope scope)
        { Key = key; DisplayName = displayName; Scope = scope; }
        internal string Key { get; private set; }
        internal string DisplayName { get; private set; }
        internal SummonProjectIconScope Scope { get; private set; }
    }

    internal static class SummonIconCatalog
    {
        private static readonly SummonProjectIconSpec[] Values = Build();
        internal static IReadOnlyList<SummonProjectIconSpec> All
        { get { return Array.AsReadOnly(Values); } }

        internal static SummonProjectIconSpec For(string key)
        {
            SummonProjectIconSpec result = Values.SingleOrDefault(value =>
                string.Equals(value.Key, key, StringComparison.Ordinal));
            if (result == null) throw new InvalidOperationException(
                "Project summon icon catalog lacks " + key + ".");
            return result;
        }

        internal static void Validate()
        {
            string[] visibleCatalog = ExpandedSummoningCatalog.All.Where(value =>
                !string.Equals(value.Key, "dire-bat", StringComparison.Ordinal))
                .Select(value => value.Key).ToArray();
            string[] split = { "redcap", "axiomite", "soul-eater", "bogeyman",
                "movanic-deva", "frost-giant", "thanadaemon" };
            string[] preserved = { "mite", "manticore", "nereid", "hamadryad" };
            string[] expected = visibleCatalog.Concat(split).Concat(preserved)
                .ToArray();
            if (Values.Length != 77 || expected.Length != 77 ||
                Values.Any(value => value == null ||
                    string.IsNullOrWhiteSpace(value.Key) ||
                    string.IsNullOrWhiteSpace(value.DisplayName)) ||
                Values.Select(value => value.Key).Distinct(StringComparer.Ordinal)
                    .Count() != Values.Length ||
                expected.Except(Values.Select(value => value.Key),
                    StringComparer.Ordinal).Any() ||
                Values.Select(value => value.Key).Except(expected,
                    StringComparer.Ordinal).Any())
                throw new InvalidOperationException(
                    "Immutable project-owned summon icon catalog is incomplete.");
        }

        private static SummonProjectIconSpec[] Build()
        {
            var result = ExpandedSummoningCatalog.All.Where(value =>
                value.Key != "dire-bat").Select(value => new SummonProjectIconSpec(
                    value.Key, value.DisplayName, SummonProjectIconScope.KmgCatalog))
                .ToList();
            Add(result, SummonProjectIconScope.SplitNative,
                "redcap", "Redcap", "axiomite", "Axiomite", "soul-eater",
                "Soul Eater", "bogeyman", "Bogeyman", "movanic-deva",
                "Movanic Deva", "frost-giant", "Frost Giant", "thanadaemon",
                "Thanadaemon");
            Add(result, SummonProjectIconScope.PreservedNative,
                "mite", "Mite", "manticore", "Manticore", "nereid",
                "Nereid", "hamadryad", "Hamadryad");
            return result.ToArray();
        }

        private static void Add(ICollection<SummonProjectIconSpec> values,
            SummonProjectIconScope scope, params string[] pairs)
        {
            if (pairs.Length % 2 != 0) throw new ArgumentException("Icon pairs.");
            for (int index = 0; index < pairs.Length; index += 2)
                values.Add(new SummonProjectIconSpec(pairs[index],
                    pairs[index + 1], scope));
        }
    }
}
