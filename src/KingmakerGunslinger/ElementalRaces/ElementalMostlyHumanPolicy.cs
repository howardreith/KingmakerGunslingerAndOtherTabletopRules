using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>One parent race's Mostly Human choice (charter section 5.2).</summary>
    internal sealed class ElementalMostlyHumanDefinition
    {
        internal ElementalMostlyHumanDefinition(ElementalHeritageRace race,
            string elementalLanguage)
        {
            Race = race;
            ElementalLanguage = elementalLanguage;
        }

        internal ElementalHeritageRace Race { get; private set; }
        internal string ElementalLanguage { get; private set; }
        internal string RaceName { get { return Race.ToString(); } }

        internal string SelectionSymbol
        {
            get { return ElementalMostlyHumanPolicy.Prefix + Race + ".Selection"; }
        }

        internal string StandardSymbol
        {
            get { return ElementalMostlyHumanPolicy.Prefix + Race + ".Standard"; }
        }

        internal string TraitSymbol
        {
            get { return ElementalMostlyHumanPolicy.Prefix + Race + ".Trait"; }
        }

        internal string SelectionName { get { return "Mostly Human"; } }

        internal string SelectionDescription
        {
            get
            {
                return "Choose whether this " + RaceName + " is mostly human. Either " +
                    "choice keeps every " + RaceName + " racial trait, heritage and " +
                    "alternate trait.";
            }
        }

        internal string StandardName { get { return "Standard " + RaceName + " Ancestry"; } }

        internal string StandardDescription
        {
            get
            {
                return "Your elemental ancestry shows plainly. You count as a " + RaceName +
                    " (native outsider) for effects related to race.";
            }
        }

        internal string TraitName { get { return "Mostly Human"; } }

        internal string TraitDescription
        {
            get
            {
                return "Your elemental heritage shows only faintly, and you can pass for " +
                    "human. You count as both a human (humanoid) and a " + RaceName +
                    " (native outsider) for effects related to race, such as the human " +
                    "favored class bonus choices when the Favored Class mod is installed. " +
                    "You still receive only one favored class bonus per level. Every " +
                    RaceName + " racial trait, heritage and alternate trait is kept, and " +
                    "your ability scores do not change. Kingmaker already treats geniekin " +
                    "as humanoids for spells such as Hold Person, so that is unchanged. " +
                    "The tabletop trait also changes whether you automatically speak " +
                    ElementalLanguage + "; Kingmaker has no language system, so that part " +
                    "has no effect.";
            }
        }
    }

    /// <summary>A committed identity of the Mostly Human block.</summary>
    internal sealed class ElementalMostlyHumanIdentity
    {
        internal ElementalMostlyHumanIdentity(string symbol, string guid, string plannedType)
        {
            Symbol = symbol;
            Guid = guid;
            PlannedType = plannedType;
        }

        internal string Symbol { get; private set; }
        internal string Guid { get; private set; }
        internal string PlannedType { get; private set; }
    }

    /// <summary>
    /// The four-race Mostly Human companion racial trait: a distinct identity
    /// path beside (never inside) the alternate-trait slot framework. Each
    /// parent race receives one Heritage-phase selection whose first entry
    /// keeps the standard ancestry; the Mostly Human entry grants one shared
    /// hidden identity fact that the favored-class permission graph and the
    /// scoped host ancestry bridge recognize. The race blueprint, racial stats,
    /// heritages, alternate traits and creature-type facts are untouched.
    /// </summary>
    internal static class ElementalMostlyHumanPolicy
    {
        internal const string Prefix = "KMG.MostlyHuman.";
        internal const string IdentitySymbol = "KMG.MostlyHuman.Identity";
        internal const string Milestone = "Mostly Human companion trait";
        internal const int RaceCount = 4;
        internal const int IdentityCount = 1 + 3 * RaceCount;

        private static readonly ElementalMostlyHumanDefinition[] Definitions =
        {
            new ElementalMostlyHumanDefinition(ElementalHeritageRace.Ifrit, "Ignan"),
            new ElementalMostlyHumanDefinition(ElementalHeritageRace.Oread, "Terran"),
            new ElementalMostlyHumanDefinition(ElementalHeritageRace.Sylph, "Auran"),
            new ElementalMostlyHumanDefinition(ElementalHeritageRace.Undine, "Aquan")
        };

        // The exact ordered manifest block appended after the Favored Class
        // block. Each race's Standard entry sorts before its Mostly Human
        // entry, so a deterministic automated chooser keeps the standard
        // ancestry.
        private static readonly ElementalMostlyHumanIdentity[] Identities =
        {
            new ElementalMostlyHumanIdentity(IdentitySymbol, "d71a4b4250914a7aa6d4bec53dfc768f", "BlueprintFeature"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Ifrit.Selection",
                "c0485fc76e514ad38c09961dbdc6efc7", "BlueprintFeatureSelection"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Ifrit.Standard",
                "7c508ee3415942bb91f584306d70bf70", "BlueprintFeature"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Ifrit.Trait",
                "8060afe7da3b40bbbc77b315ee23fdd3", "BlueprintFeature"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Oread.Selection",
                "e5bcb7c34a7b4b048c0d877019edac63", "BlueprintFeatureSelection"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Oread.Standard",
                "5c39e1237c4d4bbd9bfd0f1eaacb18c3", "BlueprintFeature"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Oread.Trait",
                "65aedd35c5a54273a9a33c2d58683a21", "BlueprintFeature"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Sylph.Selection",
                "2c93d16297764ce9a0f73622b9825c05", "BlueprintFeatureSelection"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Sylph.Standard",
                "0be790c9af23490e8f3ed46f5a59d00c", "BlueprintFeature"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Sylph.Trait",
                "21451d96af0843d088b04331ac7a5a99", "BlueprintFeature"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Undine.Selection",
                "bb0bae8a5720452eb16a1a6629e8fd4d", "BlueprintFeatureSelection"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Undine.Standard",
                "66868270c36a48c89947fbad8e6d979c", "BlueprintFeature"),
            new ElementalMostlyHumanIdentity("KMG.MostlyHuman.Undine.Trait",
                "f3f6f6e074114b8da0a94a49b3cf4b47", "BlueprintFeature"),
        };

        internal static IList<ElementalMostlyHumanDefinition> Ordered()
        {
            return Array.AsReadOnly(Definitions);
        }

        internal static ElementalMostlyHumanDefinition For(ElementalHeritageRace race)
        {
            return Definitions.Single(value => value.Race == race);
        }

        internal static IList<ElementalMostlyHumanIdentity> All
        {
            get { return Array.AsReadOnly(Identities); }
        }

        internal static ElementalMostlyHumanIdentity ForSymbol(string symbol)
        {
            return Identities.Single(value => string.Equals(value.Symbol, symbol,
                StringComparison.Ordinal));
        }

        /// <summary>Symbols in registration order: identity, then per race selection, standard, trait.</summary>
        internal static IList<string> Symbols()
        {
            var symbols = new List<string> { IdentitySymbol };
            foreach (ElementalMostlyHumanDefinition definition in Definitions)
            {
                symbols.Add(definition.SelectionSymbol);
                symbols.Add(definition.StandardSymbol);
                symbols.Add(definition.TraitSymbol);
            }
            return symbols.AsReadOnly();
        }

        internal static void Validate()
        {
            IList<string> symbols = Symbols();
            if (symbols.Count != IdentityCount ||
                symbols.Distinct(StringComparer.Ordinal).Count() != IdentityCount ||
                Identities.Length != IdentityCount ||
                !symbols.SequenceEqual(Identities.Select(value => value.Symbol)) ||
                Identities.Select(value => value.Guid).Distinct(StringComparer.Ordinal).Count() !=
                    IdentityCount ||
                Identities.Any(value => value.Guid == null || value.Guid.Length != 32 ||
                    value.Guid.Any(c => !((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))) ||
                Definitions.Select(value => value.Race).Distinct().Count() != RaceCount)
                throw new InvalidOperationException("Mostly Human identity inventory drifted.");
            foreach (ElementalMostlyHumanDefinition definition in Definitions)
                if (string.CompareOrdinal(ForSymbol(definition.StandardSymbol).Guid,
                        ForSymbol(definition.TraitSymbol).Guid) >= 0)
                    throw new InvalidOperationException(
                        "The standard ancestry entry must sort before Mostly Human for " +
                        definition.RaceName + ".");
        }
    }
}
