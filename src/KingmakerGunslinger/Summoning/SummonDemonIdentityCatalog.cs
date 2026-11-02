using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The one authoritative list of this project's own demon-subtype
    /// creatures. The Vrock's Stunning Screech exempts demons, and both the
    /// real effect and its AI safety consideration resolve their identities
    /// from here, so the two can never drift apart.
    ///
    /// <para>Membership comes from each creature's own printed type line, not
    /// from alignment, the outsider type, wings, faction, menu family or the
    /// native view donor a project creature happens to borrow. Three of those
    /// would each give a different and wrong answer:</para>
    /// <list type="bullet">
    /// <item>Chaotic evil alignment would admit the Bebilith, whose printed
    /// line is "CE Huge outsider (chaotic, evil, extraplanar)" with no demon
    /// subtype at all.</item>
    /// <item>The outsider type would admit every devil, kyton, archon, azata,
    /// mephit and elemental in the roster.</item>
    /// <item>The view donor would make the Succubus a nymph and the Shadow
    /// Demon a tiefling, which says nothing about the separate project-owned
    /// creature's subtype.</item>
    /// </list>
    ///
    /// <para><b>Recorded limitation.</b> Kingmaker 2.1.7b has no demon subtype
    /// fact. The installed library carries eighteen subtype features including
    /// <c>SubtypeDevil</c> and <c>SubtypeDaemon</c>, but none for demons — this
    /// was observed, not assumed, by the Sprint 37 installed-library audit
    /// (run 20261102T0730259540236Z). So this set covers the project's own
    /// demons and cannot cover a native campaign demon or a demon added by
    /// another mod. That interoperability gap is a named limitation for owner
    /// review, not an accepted waiver, and it is disclosed in the screech's own
    /// player-facing text rather than hidden.</para>
    /// </summary>
    internal static class SummonDemonIdentityCatalog
    {
        // Printed type lines, each from that creature's own entry:
        //   Dretch        CE Small  outsider (chaotic, demon, evil, extraplanar)
        //   Shadow Demon  CE Medium outsider (chaotic, demon, evil, extraplanar, incorporeal)
        //   Succubus      CE Medium outsider (chaotic, demon, evil, extraplanar)
        //   Babau         CE Medium outsider (chaotic, demon, evil, extraplanar)
        //   Hezrou        CE Large  outsider (aquatic, chaotic, demon, evil, extraplanar)
        //   Vrock         CE Large  outsider (chaotic, demon, evil, extraplanar)
        private static readonly string[] Keys = {
            "dretch", "shadow-demon", "succubus", "babau", "hezrou", "vrock"
        };

        // Registered outsiders that are deliberately NOT demons, kept beside
        // the set so a future reader sees the exclusions were decided rather
        // than overlooked. Each line is that creature's own printed subtype.
        //   bebelith          (chaotic, evil, extraplanar)  - no demon subtype
        //   lemure            (devil, evil, lawful, extraplanar)
        //   erinyes-devil     (devil, evil, lawful, extraplanar)
        //   bearded-devil     (devil, evil, lawful, extraplanar)
        //   barbed-devil      (devil, evil, lawful, extraplanar)
        //   bone-devil        (devil, evil, lawful, extraplanar)
        //   kyton-chain-devil (evil, extraplanar, kyton)
        //   hell-hound        (evil, extraplanar, fire)
        //   shadow-mastiff    (evil, extraplanar)
        //   xill              (evil, extraplanar)
        private static readonly string[] NotDemons = {
            "bebelith", "lemure", "erinyes-devil", "bearded-devil",
            "barbed-devil", "bone-devil", "kyton-chain-devil", "hell-hound",
            "shadow-mastiff", "xill"
        };

        internal static IReadOnlyList<string> CreatureKeys
        { get { return Array.AsReadOnly(Keys); } }

        internal static IReadOnlyList<string> ExcludedOutsiderKeys
        { get { return Array.AsReadOnly(NotDemons); } }

        internal static bool IsDemonKey(string key)
        { return Array.IndexOf(Keys, key) >= 0; }

        /// <summary>
        /// The exact registered unit symbols, resolved through the catalog's
        /// own creature records so one creature has one identity everywhere.
        /// </summary>
        internal static string[] UnitSymbols()
        {
            return Keys.Select(key => ExpandedSummoningIdentityCatalog
                .UnitSymbol(Creature(key))).ToArray();
        }

        internal static SummonCreatureSpec Creature(string key)
        {
            SummonCreatureSpec creature = ExpandedSummoningCatalog.All
                .SingleOrDefault(value => string.Equals(value.Key, key,
                    StringComparison.Ordinal));
            if (creature == null) throw new InvalidOperationException(
                "The demon identity catalog names an unregistered creature: " +
                key);
            return creature;
        }

        internal static void Validate()
        {
            if (Keys.Length != 6 ||
                Keys.Distinct(StringComparer.Ordinal).Count() != Keys.Length ||
                NotDemons.Any(IsDemonKey) ||
                !Keys.Contains("succubus", StringComparer.Ordinal) ||
                !Keys.Contains("shadow-demon", StringComparer.Ordinal) ||
                IsDemonKey("bebelith"))
                throw new InvalidOperationException(
                    "The project demon identity set changed.");
            foreach (string key in Keys.Concat(NotDemons)) Creature(key);
            if (UnitSymbols().Distinct(StringComparer.Ordinal).Count() !=
                Keys.Length)
                throw new InvalidOperationException(
                    "Demon unit symbols are not one-to-one.");
        }
    }
}
