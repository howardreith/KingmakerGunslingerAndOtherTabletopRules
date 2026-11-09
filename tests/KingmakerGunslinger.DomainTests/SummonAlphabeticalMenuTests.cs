using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>The alphabetical menu contract across the generated roster.</summary>
    internal static class SummonAlphabeticalMenuTests
    {
        private sealed class Row
        {
            internal Row(string id, string name, SummonMultiplicity? quantity, string creature)
            { Id = id; Name = name; Quantity = quantity; Creature = creature; }
            internal string Id { get; private set; }
            internal string Name { get; private set; }
            internal SummonMultiplicity? Quantity { get; private set; }
            internal string Creature { get; private set; }
        }

        private static SummonMenuOrderKey HostOrder(Row row)
        { return SummonMenuOrderKey.Host(row.Name, row.Quantity, row.Id, row.Creature != null); }

        private static readonly AlternateSummoningRoster Production =
            AlternateSummoningRoster.Production();

        // Label-derived wording, so family-first names cannot be flattened
        // back into the catalog's "Steam Mephit" form.
        internal static void FamilyFirstMephitsFollowTheirDisplayedNames()
        {
            SummonVariantSpec[] mephits = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key.EndsWith("-mephit", StringComparison.Ordinal))
                .ToArray();
            Assertions.True(mephits.Length > 0, "The census found no mephit placements.");
            foreach (var tier in mephits.GroupBy(value => value.ParentTier))
            {
                string[] labels = SummonDisplayOrderPolicy.Order(new SummonVariantSpec[0], tier,
                    value => SummonMenuOrderKey.Owned(SummonChoicePresentationPolicy.For(
                        value.Creature.Key, value.Multiplicity), value.StableKey))
                    .Select(value => SummonChoicePresentationPolicy.For(value.Creature.Key,
                        value.Multiplicity).Label).ToArray();
                Assertions.True(labels.All(value => value.StartsWith("Mephit, ",
                    StringComparison.Ordinal)), "A mephit lost its family-first label.");
                string[] names = labels.Select(value => value.Split('(')[0].Trim()).ToArray();
                Assertions.True(names.SequenceEqual(names.OrderBy(value => value,
                    StringComparer.OrdinalIgnoreCase)), "Mephits are out of order at tier " + tier.Key + ".");
            }
            Assertions.Equal("Mephit, Steam (1d3)", SummonChoicePresentationPolicy.For(
                "steam-mephit", SummonMultiplicity.OneD3).Label,
                "The displayed label must stay unchanged by the ordering key.");
            Assertions.Equal("Mephit, Steam", SummonChoicePresentationPolicy.For(
                "steam-mephit", SummonMultiplicity.OneD3).CreatureName,
                "The ordering name is the label without its quantity.");
            Assertions.Equal("Elemental, Air, Huge", SummonChoicePresentationPolicy.For(
                "huge-air-elemental", SummonMultiplicity.One).CreatureName,
                "Size wording belongs to the ordering name.");
        }

        // Every alternate family at every tier, against the canonical roster.
        internal static void EveryAlternateMenuMatchesTheCanonicalOrder()
        {
            int menus = 0;
            foreach (AlternateSummoningFamilyIdentity family in AlternateSummoningSourceCatalog.Identities)
            foreach (int tier in family.Tiers)
            {
                IReadOnlyList<AlternateSummoningChoice> exposed =
                    AlternateSummoningProjection.Exposed(family, Production, tier, null);
                var children = exposed.ToDictionary(value => value.Symbol, value =>
                    new Row(value.Symbol, null, value.Multiplicity, value.Placement.Creature.Key));
                var natives = new Dictionary<string, Row>();
                var foreign = new Row("foreign-control", "Zzz Control", null, null);
                var other = new Row("other-mod", "Bear, Other", null, "other");
                AlternateSummoningMenuPlan<Row> plan = AlternateSummoningMenuPolicy.Compose(
                    family.Family, tier, null, new[] { foreign, other },
                    value => null, HostOrder, exposed, choice => children[choice.Symbol],
                    SummonNativeExpansionCatalog.All, native => natives.ContainsKey(native.Symbol) ?
                        natives[native.Symbol] : (natives[native.Symbol] = new Row(native.Symbol, null,
                            native.Multiplicity, native.IconKey)));
                Assertions.True(ReferenceEquals(plan.Final.Last(), foreign),
                    "A non-creature control must stay after the creature choices.");
                Assertions.Equal(exposed.Count + natives.Count + 2, plan.Final.Count,
                    "The composed menu lost or repeated a choice.");
                Assertions.True(exposed.All(choice => plan.Final.Count(value =>
                    ReferenceEquals(value, children[choice.Symbol])) == 1),
                    "An adapter identity was not retained exactly once.");

                Row[] creatures = plan.Final.Where(value => !ReferenceEquals(value, foreign)).ToArray();
                string[] names = creatures.Select(value => ReferenceEquals(value, other) ? other.Name :
                    SummonChoicePresentationPolicy.For(value.Creature, value.Quantity.Value).CreatureName).ToArray();
                Assertions.True(names.SequenceEqual(names.OrderBy(value => value,
                    StringComparer.OrdinalIgnoreCase)), family.Key + " tier " + tier + " is out of order.");
                for (int index = 1; index < creatures.Length; index++)
                    if (string.Equals(names[index - 1], names[index], StringComparison.OrdinalIgnoreCase))
                        Assertions.True(Rank(creatures[index - 1]) <= Rank(creatures[index]),
                            "Equal names must order one, 1d3, then 1d4+1.");
                Assertions.False(exposed.Any(value => value.Placement.Creature.Key == "stirge"),
                    "Stirge must remain withheld at " + family.Key + " tier " + tier + ".");
                menus++;
            }
            Assertions.True(menus >= 54, "The census covered too few generated menus: " + menus);
        }

        private static int Rank(Row row)
        {
            return row.Quantity == SummonMultiplicity.One ? 0 :
                row.Quantity == SummonMultiplicity.OneD3 ? 1 : 2;
        }

        // A direct per-quantity root keeps its root and only its quantity.
        internal static void DirectQuantityRootsSortWithoutCombiningRoots()
        {
            AlternateSummoningFamilyIdentity tactician = AlternateSummoningSourceCatalog
                .Identities.Single(value => value.Key == "MonsterTactician");
            foreach (SummonMultiplicity quantity in new[] {
                SummonMultiplicity.One, SummonMultiplicity.OneD3, SummonMultiplicity.OneD4PlusOne })
            {
                IReadOnlyList<AlternateSummoningChoice> exposed =
                    AlternateSummoningProjection.Exposed(tactician, Production, 5, quantity);
                var host = new Row("preserved", "Summon Monster V", quantity, "preserved");
                AlternateSummoningMenuPlan<Row> plan = AlternateSummoningMenuPolicy.Compose(
                    SummonFamily.Monster, 5, quantity, new[] { host }, value => null, HostOrder,
                    exposed, choice => new Row(choice.Symbol, null, choice.Multiplicity,
                        choice.Placement.Creature.Key),
                    SummonNativeExpansionCatalog.All, native => null);
                Assertions.True(plan.Added.All(value => value.Quantity == quantity),
                    "A per-quantity root gained another quantity.");
                string[] names = plan.Final.Select(value => value.Name ??
                    SummonChoicePresentationPolicy.For(value.Creature, value.Quantity.Value).CreatureName).ToArray();
                Assertions.True(names.SequenceEqual(names.OrderBy(value => value,
                    StringComparer.OrdinalIgnoreCase)), "A per-quantity root is out of order.");
            }
        }

        // The flattened Druid list interleaves quantities by creature name,
        // keeping every row object and the slot of every unranked row.
        internal static void DruidFlattenedListInterleavesQuantitiesByParentOrder()
        {
            var single = new Row("native-single", "single", null, null);
            var d3 = new Row("native-d3", "d3", null, null);
            var foreign = new Row("foreign", "foreign", null, null);
            var wolf = new Row("wolf", "Wolf", SummonMultiplicity.One, "wolf");
            var bear = new Row("bear", "Bear", SummonMultiplicity.One, "bear");
            var wolfGroup = new Row("wolf-d3", "Wolf", SummonMultiplicity.OneD3, "wolf");
            var boarGroup = new Row("boar-d3", "Boar", SummonMultiplicity.OneD3, "boar");
            Row[] original = { single, foreign, d3, null };
            var replacements = new Dictionary<Row, Row[]> {
                { single, new[] { wolf, bear } }, { d3, new[] { wolfGroup, boarGroup } } };
            Row[] projected = DruidSummonConversionPolicy.Project(original, replacements);
            Row[] parentOrder = { bear, boarGroup, wolf, wolfGroup };
            Row[] before = projected.ToArray();
            Row[] ordered = DruidSummonConversionPolicy.OrderProjectedChoices(projected, row => {
                int index = Array.IndexOf(parentOrder, row);
                return index < 0 ? (int?)null : index; });
            Assertions.Equal("bear|boar-d3|foreign|wolf|wolf-d3|<null>",
                string.Join("|", ordered.Select(value => value == null ? "<null>" : value.Id)),
                "Ranked rows take their shared positions in parent order; a foreign row keeps its index.");
            Assertions.True(projected.SequenceEqual(before),
                "Ordering must not mutate the projected input.");
            Assertions.Equal(before.Count(value => value != null),
                ordered.Where(value => value != null).Distinct().Count(),
                "Ordering lost or repeated a Druid row.");
            Assertions.Equal(ordered.Single(value => value != null && value.Id == "wolf-d3").Quantity.ToString(),
                SummonMultiplicity.OneD3.ToString(), "A reordered row keeps its own quantity identity.");
        }

        // A row another mod appends after publication joins the order on the
        // final pass; an unclassified one stays last, and a repeat is stable.
        internal static void LateHostRowsJoinTheOrderOnTheFinalPass()
        {
            var bear = new Row("bear", "Bear", SummonMultiplicity.One, "bear");
            var wolf = new Row("wolf", "Wolf", SummonMultiplicity.One, "wolf");
            var control = new Row("control", "Aaa Control", null, null);
            var late = new Row("late", "Summon Monster I (Giant Crab Spider)", null, "spider");
            Row[] published = { bear, wolf, control, late };
            Row[] before = published.ToArray();
            IReadOnlyList<Row> ordered = SummonDisplayOrderPolicy.Order(published, new Row[0], HostOrder);
            Assertions.Equal("bear|late|wolf|control", string.Join("|", ordered.Select(value => value.Id)),
                "A late creature row sorts by its visible name; an unclassified row stays last.");
            Assertions.True(published.SequenceEqual(before), "The appended array must not be mutated.");
            Assertions.True(ordered.SequenceEqual(SummonDisplayOrderPolicy.Order(ordered, new Row[0], HostOrder)),
                "A repeated final pass must change nothing.");
        }

        // The real published Summon Nature's Ally rows, across all three
        // quantity groups of one tier, give one interleaved order.
        internal static void DruidOrderMatchesCanonicalNaturesAllyRoster()
        {
            for (int tier = 2; tier <= 9; tier++)
            {
                int scoped = tier;
                SummonVariantSpec[] specs = ExpandedSummoningCatalog
                    .GenerateVariants(SummonFamily.NaturesAlly)
                    .Where(value => value.ParentTier == scoped && SummonVisibilityCatalog.IsPublished(value))
                    .ToArray();
                Func<SummonVariantSpec, SummonMenuOrderKey> key = value =>
                    SummonMenuOrderKey.Owned(SummonChoicePresentationPolicy.For(value.Creature.Key,
                        value.Multiplicity), value.StableKey);
                SummonVariantSpec[] parent = SummonDisplayOrderPolicy.Order(
                    new SummonVariantSpec[0], specs, key).ToArray();
                SummonVariantSpec[] grouped = specs.OrderBy(value => (int)value.Multiplicity).ToArray();
                SummonVariantSpec[] flattened = DruidSummonConversionPolicy.OrderProjectedChoices(grouped,
                    row => Array.IndexOf(parent, row));
                Assertions.True(flattened.SequenceEqual(parent),
                    "Druid tier " + tier + " flattened order differs from the canonical parent.");
                Assertions.False(specs.Any(value => value.Creature.Key == "stirge"),
                    "Stirge must remain withheld from Nature's Ally tier " + tier + ".");
            }
        }
    }
}
