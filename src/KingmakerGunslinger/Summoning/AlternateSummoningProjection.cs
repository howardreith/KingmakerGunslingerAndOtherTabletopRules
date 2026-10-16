using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The central roster as one read-only input: logical placements, the
    /// visibility rule that decides which are exposed, and the retained native
    /// options. Production reads the shipped catalogs; a test can supply a
    /// placement that exists nowhere in production content.
    /// </summary>
    internal sealed class AlternateSummoningRoster
    {
        internal AlternateSummoningRoster(IEnumerable<SummonVariantSpec> placements,
            Func<SummonVariantSpec, bool> isPublished,
            IEnumerable<SummonNativeExpansionSpec> nativeOptions)
        {
            if (placements == null) throw new ArgumentNullException("placements");
            if (isPublished == null) throw new ArgumentNullException("isPublished");
            Placements = placements.ToArray();
            IsPublished = isPublished;
            NativeOptions = (nativeOptions ??
                Enumerable.Empty<SummonNativeExpansionSpec>()).ToArray();
        }

        internal IReadOnlyList<SummonVariantSpec> Placements { get; private set; }
        internal Func<SummonVariantSpec, bool> IsPublished { get; private set; }
        internal IReadOnlyList<SummonNativeExpansionSpec> NativeOptions
        { get; private set; }

        internal static AlternateSummoningRoster Production()
        {
            return new AlternateSummoningRoster(
                ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Concat(ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)),
                SummonVisibilityCatalog.IsPublished,
                SummonNativeExpansionCatalog.All);
        }
    }

    internal enum AlternateSummoningChoiceKind
    {
        /// <summary>A project creature placement from the central catalog.</summary>
        Creature,
        /// <summary>A retained native wrapper split from a native umbrella.</summary>
        NativeOption,
        /// <summary>The frozen copy of a direct host root's original cast.</summary>
        Preserved
    }

    /// <summary>
    /// One caller-specific executable identity. It owns its own stable
    /// symbol; the creature, template and text it composes are shared with
    /// the canonical spell child of the same placement.
    /// </summary>
    internal sealed class AlternateSummoningChoice
    {
        internal AlternateSummoningChoice(string familyKey, int tier,
            SummonMultiplicity multiplicity, AlternateSummoningChoiceKind kind,
            SummonVariantSpec placement, SummonNativeExpansionSpec nativeOption,
            bool published, string symbol)
        {
            FamilyKey = familyKey; Tier = tier; Multiplicity = multiplicity;
            Kind = kind; Placement = placement; NativeOption = nativeOption;
            Published = published; Symbol = symbol;
        }

        internal string FamilyKey { get; private set; }
        internal int Tier { get; private set; }
        internal SummonMultiplicity Multiplicity { get; private set; }
        internal AlternateSummoningChoiceKind Kind { get; private set; }
        internal SummonVariantSpec Placement { get; private set; }
        internal SummonNativeExpansionSpec NativeOption { get; private set; }
        /// <summary>Whether the central visibility rule exposes it. A native
        /// option and a preservation clone carry no visibility of their own;
        /// whether they appear is decided by host lineage at publication.</summary>
        internal bool Published { get; private set; }
        internal string Symbol { get; private set; }

        /// <summary>The canonical spell child this choice composes from, or
        /// null for a preservation clone.</summary>
        internal string CanonicalSymbol
        {
            get
            {
                return Kind == AlternateSummoningChoiceKind.Creature ?
                    ExpandedSummoningIdentityCatalog.AbilitySymbol(Placement) :
                    Kind == AlternateSummoningChoiceKind.NativeOption ?
                        NativeOption.Symbol : null;
            }
        }
    }

    /// <summary>
    /// Projects the one central roster onto a caller. Eligibility is exactly
    /// the ordinary spell's: the same family, the same tier and the same
    /// quantity rule. No caller keeps a creature list, so a creature added to
    /// or withheld from the catalog reaches every eligible caller and only
    /// those.
    /// </summary>
    internal static class AlternateSummoningProjection
    {
        internal const string SymbolPrefix = "KMG.Summoning.Adapter.";

        /// <summary>Every identity a family owns, exposed or not. Registration
        /// covers all of them so a saved reference always resolves.</summary>
        internal static IReadOnlyList<AlternateSummoningChoice> Identities(
            AlternateSummoningFamilyIdentity family, AlternateSummoningRoster roster)
        {
            if (family == null) throw new ArgumentNullException("family");
            if (roster == null) throw new ArgumentNullException("roster");
            var result = new List<AlternateSummoningChoice>();
            foreach (int tier in family.Tiers)
            {
                foreach (SummonVariantSpec placement in roster.Placements.Where(
                    value => value.Family == family.Family &&
                        value.ParentTier == tier))
                    result.Add(new AlternateSummoningChoice(family.Key, tier,
                        placement.Multiplicity, AlternateSummoningChoiceKind.Creature,
                        placement, null, roster.IsPublished(placement),
                        CreatureSymbol(family.Key, placement)));
                foreach (SummonNativeExpansionSpec native in roster.NativeOptions
                    .Where(value => value.Family == family.Family &&
                        value.Tier == tier))
                    result.Add(new AlternateSummoningChoice(family.Key, tier,
                        native.Multiplicity, AlternateSummoningChoiceKind.NativeOption,
                        null, native, true, NativeOptionSymbol(family.Key, native)));
                foreach (SummonMultiplicity quantity in PreservedQuantities(family, tier))
                    result.Add(new AlternateSummoningChoice(family.Key, tier,
                        quantity, AlternateSummoningChoiceKind.Preserved, null,
                        null, true, PreservedSymbol(family.Key, tier, quantity)));
            }
            Validate(family, result);
            return result.AsReadOnly();
        }

        /// <summary>The project creatures a root at this tier offers, in
        /// catalog order. A direct per-quantity root offers only its own
        /// quantity; a menu root offers every quantity of its tier.</summary>
        internal static IReadOnlyList<AlternateSummoningChoice> Exposed(
            AlternateSummoningFamilyIdentity family, AlternateSummoningRoster roster,
            int tier, SummonMultiplicity? quantity)
        {
            return Identities(family, roster).Where(value =>
                value.Kind == AlternateSummoningChoiceKind.Creature &&
                value.Published && value.Tier == tier &&
                (!quantity.HasValue || value.Multiplicity == quantity.Value))
                .ToArray();
        }

        internal static string CreatureSymbol(string familyKey,
            SummonVariantSpec placement)
        {
            if (placement == null) throw new ArgumentNullException("placement");
            return SymbolPrefix + RequireKey(familyKey) + ".Tier" +
                placement.ParentTier + "." + ExpandedSummoningIdentityCatalog
                    .CreatureToken(placement.Creature.Key) + "." +
                placement.Multiplicity;
        }

        internal static string NativeOptionSymbol(string familyKey,
            SummonNativeExpansionSpec native)
        {
            if (native == null) throw new ArgumentNullException("native");
            return SymbolPrefix + RequireKey(familyKey) + ".NativeOption.Tier" +
                native.Tier + "." + native.CreatureKey + "." + native.Multiplicity;
        }

        internal static string PreservedSymbol(string familyKey, int tier,
            SummonMultiplicity quantity)
        {
            if (tier < 1 || tier > 9) throw new ArgumentOutOfRangeException("tier");
            return SymbolPrefix + RequireKey(familyKey) + ".Preserved.Tier" +
                tier + "." + quantity;
        }

        internal static IEnumerable<SummonMultiplicity> PreservedQuantities(
            AlternateSummoningFamilyIdentity family, int tier)
        {
            if (family.Preservation == AlternateSummoningPreservation.PerQuantity)
                return AlternateSummoningSourceCatalog.Quantities(tier);
            return tier == 1 ? new[] { SummonMultiplicity.One } :
                new SummonMultiplicity[0];
        }

        private static string RequireKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Any(value =>
                !char.IsLetterOrDigit(value)))
                throw new ArgumentException("Adapter family keys are alphanumeric.", "key");
            return key;
        }

        private static void Validate(AlternateSummoningFamilyIdentity family,
            IList<AlternateSummoningChoice> choices)
        {
            if (choices.Select(value => value.Symbol).Distinct(
                StringComparer.Ordinal).Count() != choices.Count)
                throw new InvalidOperationException(
                    "Adapter identities collide in family " + family.Key + ".");
            if (choices.Any(value => !value.Symbol.StartsWith(SymbolPrefix,
                StringComparison.Ordinal) || value.Symbol.StartsWith(
                    "KMG.Summoning.Ability.", StringComparison.Ordinal)))
                throw new InvalidOperationException(
                    "Adapter identities left their namespace in family " +
                    family.Key + ".");
            if (choices.Any(value => value.Kind == AlternateSummoningChoiceKind.Creature &&
                value.Placement.Family != family.Family))
                throw new InvalidOperationException(
                    "A placement crossed summon families in " + family.Key + ".");
        }
    }
}
