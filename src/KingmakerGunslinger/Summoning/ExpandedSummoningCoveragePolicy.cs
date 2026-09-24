using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Where a creature's live unit identity comes from today.</summary>
    internal enum SummonUnitProvenance
    {
        /// <summary>No unit identity exists yet anywhere.</summary>
        None = 0,
        /// <summary>A project-owned summon-safe unit in ExpandedSummoningCatalog.</summary>
        ProjectOwned = 1,
        /// <summary>A retained native Kingmaker unit exposed through a wrapper.</summary>
        NativeWrapper = 2
    }

    /// <summary>
    /// How one creature stands in one spell family right now. This is placement
    /// coverage, deliberately separate from whether a unit identity exists: a
    /// creature can own a live unit in one family and still be unplaced in the
    /// other, which is exactly Frost Giant's situation.
    /// </summary>
    internal enum SummonFamilyCoverage
    {
        /// <summary>The ideal roster does not place this creature in this family.</summary>
        NotOffered = 0,
        /// <summary>Wanted by the ideal roster; nothing offers it yet.</summary>
        Planned = 1,
        /// <summary>Registered so saves deserialize, but withheld from menus.</summary>
        Registered = 2,
        /// <summary>A player can select it in this family today.</summary>
        Published = 3
    }

    /// <summary>
    /// Derives current coverage from the union of the shipped catalogs rather
    /// than from a hand-maintained column.
    ///
    /// An earlier revision recorded coverage as literal data on each manifest
    /// row and consulted only <see cref="ExpandedSummoningCatalog"/> to fill it.
    /// That marked the eleven creatures which ship solely as retained native
    /// wrappers as though no identity existed for them, and because the tests
    /// read the same single catalog they encoded the omission instead of
    /// catching it. Deriving coverage here means a catalog change moves the
    /// counts, and a test that pins those counts fails when reality moves.
    /// </summary>
    internal static class ExpandedSummoningCoveragePolicy
    {
        /// <summary>
        /// Canonical creature key for a native wrapper's PascalCase key, so one
        /// creature keeps one identity across both catalogs (charter D-01).
        /// </summary>
        internal static string CanonicalKey(string nativeWrapperKey)
        {
            if (string.IsNullOrEmpty(nativeWrapperKey))
                throw new ArgumentException("nativeWrapperKey");
            var result = new System.Text.StringBuilder();
            for (int index = 0; index < nativeWrapperKey.Length; index++)
            {
                char value = nativeWrapperKey[index];
                if (index > 0 && char.IsUpper(value)) result.Append('-');
                result.Append(char.ToLowerInvariant(value));
            }

            return result.ToString();
        }

        /// <summary>Canonical keys of every creature exposed by a native wrapper.</summary>
        internal static IReadOnlyList<string> NativeWrapperCreatures
        {
            get
            {
                return SummonNativeExpansionCatalog.All
                    .Select(value => CanonicalKey(value.CreatureKey))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList().AsReadOnly();
            }
        }

        /// <summary>Canonical keys of every creature with a live unit identity.</summary>
        internal static IReadOnlyList<string> RepresentedCreatures
        {
            get
            {
                return ExpandedSummoningCatalog.All.Select(value => value.Key)
                    .Concat(NativeWrapperCreatures)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList().AsReadOnly();
            }
        }

        internal static SummonUnitProvenance Provenance(string creatureKey)
        {
            if (string.IsNullOrEmpty(creatureKey))
                throw new ArgumentException("creatureKey");
            if (ExpandedSummoningCatalog.All.Any(value =>
                    string.Equals(value.Key, creatureKey, StringComparison.Ordinal)))
                return SummonUnitProvenance.ProjectOwned;
            return NativeWrapperCreatures.Contains(creatureKey, StringComparer.Ordinal)
                ? SummonUnitProvenance.NativeWrapper
                : SummonUnitProvenance.None;
        }

        /// <summary>True when any wrapper places this creature in this family.</summary>
        internal static bool HasNativeWrapper(string creatureKey, SummonFamily family)
        {
            return SummonNativeExpansionCatalog.All.Any(value =>
                value.Family == family && string.Equals(
                    CanonicalKey(value.CreatureKey), creatureKey,
                    StringComparison.Ordinal));
        }

        /// <summary>
        /// Coverage of one creature in one family. <paramref name="idealTier"/>
        /// is the ideal roster's placement; null means the roster does not want
        /// this creature in this family at all.
        /// </summary>
        internal static SummonFamilyCoverage Coverage(string creatureKey,
            SummonFamily family, int? idealTier)
        {
            if (string.IsNullOrEmpty(creatureKey))
                throw new ArgumentException("creatureKey");
            if (!idealTier.HasValue) return SummonFamilyCoverage.NotOffered;

            SummonCreatureSpec owned = ExpandedSummoningCatalog.All
                .SingleOrDefault(value => string.Equals(value.Key, creatureKey,
                    StringComparison.Ordinal));
            bool ownedInFamily = owned != null && (family == SummonFamily.Monster
                ? owned.MonsterTier.HasValue : owned.NaturesAllyTier.HasValue);

            if (ownedInFamily)
            {
                // Registered identities that every menu withholds stay Registered
                // so old saves keep deserializing without claiming a live choice.
                bool visible = ExpandedSummoningCatalog.GenerateVariants(family)
                    .Where(value => string.Equals(value.Creature.Key, creatureKey,
                        StringComparison.Ordinal))
                    .Any(SummonVisibilityCatalog.IsPublished);
                return visible ? SummonFamilyCoverage.Published
                    : SummonFamilyCoverage.Registered;
            }

            // A retained native wrapper is a real, selectable option.
            if (HasNativeWrapper(creatureKey, family))
                return SummonFamilyCoverage.Published;

            return SummonFamilyCoverage.Planned;
        }

        /// <summary>
        /// Creatures a player can select somewhere today, in either family.
        /// </summary>
        internal static IReadOnlyList<string> PublishedSomewhere
        {
            get
            {
                return RepresentedCreatures.Where(key =>
                {
                    IdealRosterEntry entry =
                        ExpandedSummoningIdealRosterCatalog.Find(key);
                    int? monster = entry == null ? null : entry.MonsterTier;
                    int? allies = entry == null ? null : entry.NaturesAllyTier;
                    return Coverage(key, SummonFamily.Monster, monster) ==
                            SummonFamilyCoverage.Published ||
                        Coverage(key, SummonFamily.NaturesAlly, allies) ==
                            SummonFamilyCoverage.Published;
                }).ToList().AsReadOnly();
            }
        }

        internal static void Validate()
        {
            foreach (SummonNativeExpansionSpec wrapper in
                SummonNativeExpansionCatalog.All)
            {
                string key = CanonicalKey(wrapper.CreatureKey);
                if (ExpandedSummoningIdealRosterCatalog.Find(key) == null)
                    throw new InvalidOperationException(
                        "A retained native wrapper has no ideal roster entry: " + key);
                if (Provenance(key) == SummonUnitProvenance.None)
                    throw new InvalidOperationException(
                        "A retained native wrapper is not counted as represented: " + key);
            }
        }
    }
}
