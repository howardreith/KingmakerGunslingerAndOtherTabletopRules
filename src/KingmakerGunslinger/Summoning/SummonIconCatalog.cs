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
        internal const string ConstrictorTraitsSymbol = "KMG.Summoning.Special.ConstrictorSnake.CombatTraits";
        internal const string SalamanderTraitsSymbol = "KMG.Summoning.Special.Salamander.CombatTraits";

        internal static string PassiveTraitIconFor(string symbol)
        {
            // Exact passive species identity, not another selectable attack.
            return symbol == ConstrictorTraitsSymbol ? "constrictor-snake" :
                symbol == SalamanderTraitsSymbol ? "salamander" : null;
        }

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
            string[] visibleCatalog = ExpandedSummoningCatalog.All
                .Where(IsRegisteredSomewhere)
                .Select(value => value.Key).ToArray();
            string[] split = { "redcap", "axiomite", "soul-eater", "bogeyman",
                "movanic-deva", "frost-giant", "thanadaemon" };
            string[] preserved = { "mite", "manticore", "nereid", "hamadryad" };
            string[] prepared = { "remove-stirge" };
            string[] expected = visibleCatalog.Concat(split).Concat(preserved)
                .Concat(prepared)
                .ToArray();
            if (Values.Length != 109 || expected.Length != 109 ||
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
            var result = ExpandedSummoningCatalog.All.Where(IsRegisteredSomewhere)
                .Select(value => new SummonProjectIconSpec(
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
            // Qualified creature icons enter through visibleCatalog, which
            // now includes the published Sprint 12 quadrupeds. The Stirge
            // removal action remains a separate concept with no creature of
            // its own, so it stays an explicit entry.
            Add(result, SummonProjectIconScope.KmgCatalog,
                "remove-stirge", "Remove Stirge");
            return result.ToArray();
        }

        /// <summary>
        /// Every creature the roster registers, published or withheld.
        ///
        /// <para>An icon is part of a creature's identity and identities are
        /// allocated once and never move, so a creature that is registered and
        /// withheld still carries its own icon: removing its key from the
        /// suppression set is all that publication should take, and a menu
        /// entry that appeared without a face would make that a two-step
        /// change.</para>
        /// </summary>
        internal static bool IsRegisteredSomewhere(SummonCreatureSpec creature)
        {
            return ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly))
                .Any(value => value.Creature.Key == creature.Key);
        }

        /// <summary>
        /// Only the creatures a player can choose today. The guarded runtime
        /// review samples this, because what it has to look at is the live
        /// menu rather than the roster.
        /// </summary>
        internal static bool IsPublishedSomewhere(SummonCreatureSpec creature)
        {
            return ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly))
                .Any(value => value.Creature.Key == creature.Key &&
                    SummonVisibilityCatalog.IsPublished(value));
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
