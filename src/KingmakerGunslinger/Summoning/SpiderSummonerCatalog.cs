using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One Races Unleashed spider: its exact loaded identities.</summary>
    internal sealed class SpiderSpeciesSpec
    {
        internal SpiderSpeciesSpec(string key, string token, string displayName, int baseTier, string unitGuid,
            string poisonFeatureGuid, string monsterRowName, string naturesAllyRowName)
        {
            Key = key; Token = token; DisplayName = displayName; BaseTier = baseTier; UnitGuid = unitGuid;
            PoisonFeatureGuid = poisonFeatureGuid; MonsterRowName = monsterRowName; NaturesAllyRowName = naturesAllyRowName;
        }

        internal string Key { get; private set; }
        internal string Token { get; private set; }
        internal string DisplayName { get; private set; }
        internal int BaseTier { get; private set; }
        internal string UnitGuid { get; private set; }
        internal string PoisonFeatureGuid { get; private set; }
        internal string MonsterRowName { get; private set; }
        internal string NaturesAllyRowName { get; private set; }

        internal string RowName(SummonFamily family)
        { return family == SummonFamily.Monster ? MonsterRowName : NaturesAllyRowName; }
    }

    /// <summary>One species and tier in one spell family. The eight base-tier
    /// placements are Races Unleashed's own rows; every other placement is a
    /// generated overlay row with its own stable identity.</summary>
    internal sealed class SpiderPlacement
    {
        internal SpiderPlacement(SummonFamily family, int tier, SpiderSpeciesSpec species, SummonMultiplicity quantity)
        { Family = family; Tier = tier; Species = species; Quantity = quantity; }

        internal SummonFamily Family { get; private set; }
        internal int Tier { get; private set; }
        internal SpiderSpeciesSpec Species { get; private set; }
        internal SummonMultiplicity Quantity { get; private set; }
        internal bool IsOriginal { get { return Tier == Species.BaseTier; } }

        /// <summary>The manifest symbol. Keyed by family, tier, species and
        /// quantity, never by list position or a localized label.</summary>
        internal string Symbol
        {
            get
            {
                return "KMG.Summoning.Spider." + (Family == SummonFamily.Monster ? "SM" : "SNA") + ".Tier" + Tier + "." + Species.Token + "." +
                    (Quantity == SummonMultiplicity.One ? "One" : Quantity == SummonMultiplicity.OneD3 ? "OneD3" : "OneD4PlusOne");
            }
        }

        internal string StableKey
        { get { return "spider-" + (Family == SummonFamily.Monster ? "sm" : "sna") + "-" + Tier + "-" + Species.Key + "-" + Quantity; } }
    }

    /// <summary>The Spider Summoner roster, quantity matrix and exact
    /// identities of everything the feat's benefit may touch.</summary>
    internal static class SpiderSummonerCatalog
    {
        internal const string IdentityPrefix = "130211b7fcb443e9b67d5561d75cf6a0";
        internal const string MarkerBuffSymbol = "KMG.Summoning.Spider.DcBonus";
        internal const string OrdinaryGiantSpiderUnit = "4a3cd49e751448c8b8836485b262fdf1";
        internal const string GiantSpiderPoisonFeature = "094714bb08f4e1943a8e9d2384ebe573";
        internal const string HuntingSpiderPoisonFeature = "ef60cd888b834a549898824e6b684918";
        internal const string GiantSpiderWebAbility = "ea2845894cb94af19994768602294900";
        internal const int DcBonus = 2;

        private static readonly SpiderSpeciesSpec[] Species = {
            new SpiderSpeciesSpec("giant-crab-spider", "CrabSpider", "Giant Crab Spider", 1, "b7e1252281ee46ea8dcd045aa1e56ee9", "ddf05b24c484487692764d04079cb899", "SummonMonsterIGiantCrabSpider", "SummonNaturesAllyGiantCrabSpider"),
            new SpiderSpeciesSpec("giant-black-widow", "GiantBlackWidow", "Giant Black Widow", 4, "9b1fda47e300491ab44011be9f8ef755", "b7d97e350ead49b1abd419fa4655d31a", "SummonMonsterGiantBlackWidow", "SummonNaturesAllyGiantBlackWidow"),
            new SpiderSpeciesSpec("ogre-spider", "OgreSpider", "Ogre Spider", 5, "4a205de6d159483d99eba5aa05dd8f48", "b32243a8c4c5439c92a00296fc4bb7a1", "SummonMonsterOgreSpider", "SummonNaturesAllyOgreSpider"),
            new SpiderSpeciesSpec("giant-tarantula", "GiantTarantula", "Giant Tarantula", 7, "2ea0fc9b62634a1b99073119c02e673f", "756e0e46215f4a5e8edd79659a6ad992", "SummonMonsterGiantTarantula", "SummonNaturesAllyGiantTarantula"),
        };

        internal static IReadOnlyList<SpiderSpeciesSpec> All { get { return Array.AsReadOnly(Species); } }

        internal static SummonFamily[] Families { get { return new[] { SummonFamily.Monster, SummonFamily.NaturesAlly }; } }

        /// <summary>A cast at base tier B summons one; B+1 summons 1d3; B+2 and above 1d4+1.</summary>
        internal static SummonMultiplicity QuantityAt(int baseTier, int tier)
        {
            if (tier < baseTier) throw new ArgumentOutOfRangeException("tier");
            return tier == baseTier ? SummonMultiplicity.One : tier == baseTier + 1 ? SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne;
        }

        /// <summary>A generated row's count, as the native quantity donor
        /// expresses it: one die of the matrix size plus a rank bonus. That rank
        /// is where the engine applies Superior Summoning, so it must survive
        /// unchanged rather than be replaced by a fixed bonus.</summary>
        internal static bool QuantityExpressionMatches(string dice, string diceCountKind, int diceCount, string bonusKind, SummonMultiplicity quantity)
        {
            if (quantity == SummonMultiplicity.One) return false;
            return dice == (quantity == SummonMultiplicity.OneD3 ? "D3" : "D4") && diceCountKind == "Simple" && diceCount == 1 && bonusKind == "Rank";
        }

        internal static IEnumerable<SpiderPlacement> Placements(SummonFamily family)
        {
            foreach (SpiderSpeciesSpec species in Species)
                for (int tier = species.BaseTier; tier <= 9; tier++)
                    yield return new SpiderPlacement(family, tier, species, QuantityAt(species.BaseTier, tier));
        }

        internal static IEnumerable<SpiderPlacement> AllPlacements
        { get { return Families.SelectMany(Placements); } }

        internal static IEnumerable<SpiderPlacement> Generated
        { get { return AllPlacements.Where(value => !value.IsOriginal); } }

        internal static SpiderSpeciesSpec Find(string key)
        { return Species.SingleOrDefault(value => string.Equals(value.Key, key, StringComparison.Ordinal)); }

        /// <summary>The full Races Unleashed identity. Deliberately not a
        /// conventional 32-character GUID, and never parsed as one.</summary>
        internal static string RowIdentity(SummonFamily family, SpiderSpeciesSpec species)
        { return IdentityPrefix + species.RowName(family); }

        /// <summary>The base multiplicity of a Races Unleashed original row in
        /// its own family's parent at its own base tier, read from the placement
        /// matrix (always one); null for any other row, tier or family. Menu
        /// ordering uses it so an original row joins the single-summon group by
        /// its identity, never by its label or list position.</summary>
        internal static SummonMultiplicity? OriginalRowQuantity(SummonFamily family, int tier, string rowIdentity)
        {
            if (rowIdentity == null) return null;
            SpiderSpeciesSpec species = Species.SingleOrDefault(value => value.BaseTier == tier &&
                string.Equals(RowIdentity(family, value), rowIdentity, StringComparison.Ordinal));
            return species == null ? (SummonMultiplicity?)null : QuantityAt(species.BaseTier, tier);
        }

        internal static IEnumerable<string> EligibleUnits
        { get { return Species.Select(value => value.UnitGuid).Concat(new[] { OrdinaryGiantSpiderUnit }); } }

        /// <summary>Exactly the blueprints whose calculated parameters carry
        /// the feat's bonus: each spider's poison, the donor poison the Races
        /// Unleashed units inherit, and the ordinary Giant Spider's web. A
        /// shared buff such as the native webbed state is never in this set.</summary>
        internal static IEnumerable<string> DcBlueprints
        {
            get
            {
                return Species.Select(value => value.PoisonFeatureGuid).Concat(new[] {
                    GiantSpiderPoisonFeature, HuntingSpiderPoisonFeature, GiantSpiderWebAbility });
            }
        }

        internal static void Validate()
        {
            foreach (SummonFamily family in Families)
            {
                int count = Placements(family).Count();
                if (count != 23) throw new InvalidOperationException("Spider Summoner placement count changed: " + family + "=" + count);
                if (Placements(family).Select(value => value.Symbol).Distinct(StringComparer.Ordinal).Count() != count)
                    throw new InvalidOperationException("Spider Summoner placement identities collided: " + family);
            }
        }
    }

    /// <summary>Whether one spawned creature receives the feat's benefit.</summary>
    internal static class SpiderSummonerBenefitPolicy
    {
        private static readonly HashSet<string> Units =
            new HashSet<string>(SpiderSummonerCatalog.EligibleUnits, StringComparer.Ordinal);
        private static readonly HashSet<string> Blueprints =
            new HashSet<string>(SpiderSummonerCatalog.DcBlueprints, StringComparer.Ordinal);

        /// <summary>A summoned spider of an eligible species, cast as a spell
        /// by a caster who actually holds the feat.</summary>
        internal static bool Receives(string unitGuid, bool castAsSpell, bool casterHasFeature)
        { return castAsSpell && casterHasFeature && unitGuid != null && Units.Contains(unitGuid); }

        internal static bool AdjustsParametersOf(string blueprintGuid)
        { return blueprintGuid != null && Blueprints.Contains(blueprintGuid); }
    }

    /// <summary>
    /// The native webbed state's escape (ContextActionBreakFree, CMB allowed,
    /// CMD not used as DC) rolls a grapple maneuver against the web-casting
    /// spider's CMD whenever the escapee's CMB beats its skills, and reads no
    /// DC on that branch. This decides when that one maneuver's CMD target
    /// number carries the feat's +2: never for an ordinary grapple, another
    /// maneuver, another web source or an unmarked spider. A path that reads
    /// the bonused DC instead (the native skill check, or a replacement
    /// action) triggers no maneuver, so it is never adjusted twice.
    /// </summary>
    internal static class SpiderWebEscapePolicy
    {
        internal const string WebbedState = "a719abac0ea0ce346b401060754cc1c0";

        internal static bool AdjustsEscapeCmd(bool cmdIsGrapple, bool enclosedByGrappleManeuver, bool sameEscapee, bool spiderIsManeuverTarget,
            string reasonContextBlueprint, string reasonParentBlueprint, bool spiderCastTheWeb, bool spiderMarked)
        {
            return cmdIsGrapple && enclosedByGrappleManeuver && sameEscapee && spiderIsManeuverTarget && spiderCastTheWeb && spiderMarked &&
                reasonContextBlueprint == WebbedState && reasonParentBlueprint == SpiderSummonerCatalog.GiantSpiderWebAbility;
        }

        /// <summary>
        /// The spider is marked when the handler's own fact is the marker its
        /// owner currently holds. This needs no reference set by an active
        /// integration, so a marker kept across a reload with Expanded
        /// Summoning OFF keeps its benefit until the summon expires.
        /// </summary>
        internal static bool OwnMarkerHeld(object ownFact, object heldByOwner)
        {
            return ownFact != null && ReferenceEquals(ownFact, heldByOwner);
        }
    }
}
