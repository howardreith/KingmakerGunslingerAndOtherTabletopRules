using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint29Tests
    {
        internal static void HiddenRoutesAreExactAndPublishedSurfaceIsPreserved()
        {
            SummonVisibilityCatalog.Validate();
            var all = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly)).ToArray();
            foreach (var row in new[] {
                new { Key = "ettin", Tiers = new[] { 5, 6, 7, 8, 9 } },
                new { Key = "cloud-giant", Tiers = new[] { 8, 9 } },
                new { Key = "storm-giant", Tiers = new[] { 9 } } })
            {
                var routes = all.Where(value => value.Creature.Key == row.Key)
                    .ToArray();
                Assertions.True(routes.Select(value => value.ParentTier)
                    .SequenceEqual(row.Tiers), row.Key + " has its exact SNA tiers.");
                Assertions.True(routes.All(value =>
                    value.Family == SummonFamily.NaturesAlly &&
                    !value.Creature.MonsterTemplated &&
                    !SummonVisibilityCatalog.IsPublished(value)),
                    row.Key + " has no public or templated route.");
                Assertions.True(SummonIconCatalog.All.Any(value =>
                    value.Key == row.Key), row.Key + " has a real icon record.");
            }
            Assertions.Equal(1127, all.Length, "Registered logical roots.");
            Assertions.Equal(1119, all.Count(SummonVisibilityCatalog.IsPublished),
                "Existing published roots stay visible.");
            Assertions.Equal(8, all.Count(value =>
                !SummonVisibilityCatalog.IsPublished(value)),
                "Only the eight new roots are suppressed.");
        }

        internal static void MissingSuppressionOrIdentityIsRejected()
        {
            var field = typeof(SummonVisibilityCatalog).GetField(
                "SuppressedCreatureKeys", BindingFlags.Static | BindingFlags.NonPublic);
            var suppressed = (HashSet<string>)field.GetValue(null);
            Assertions.True(suppressed.Remove("storm-giant"),
                "The Storm suppression exists before the negative check.");
            try
            {
                Assertions.Throws<InvalidOperationException>(
                    () => SummonVisibilityCatalog.Validate(),
                    "A missing required suppression must fail validation.");
            }
            finally
            {
                suppressed.Add("storm-giant");
            }
            SummonVisibilityCatalog.Validate();

            var identities = ExpandedSummoningIdentityCatalog.Build().ToList();
            Assertions.Equal(1, identities.RemoveAll(value => value.Symbol ==
                "KMG.Summoning.Giant.Sprint29.StormLightningSafety"),
                "The exact Storm AI dependency is registered.");
            Assertions.Throws<InvalidOperationException>(
                () => ExpandedSummoningIdentityCatalog.Validate(identities),
                "A missing private dependency must fail identity validation.");
        }
    }
}
