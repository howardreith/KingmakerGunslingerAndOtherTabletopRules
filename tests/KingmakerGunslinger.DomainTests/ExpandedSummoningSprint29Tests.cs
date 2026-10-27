using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint29Tests
    {
        internal static void PublicRoutesAreExactAndPriorSurfaceIsPreserved()
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
                    SummonVisibilityCatalog.IsPublished(value)),
                    row.Key + " has only public untemplated SNA routes.");
                Assertions.True(SummonIconCatalog.All.Any(value =>
                    value.Key == row.Key), row.Key + " has a real icon record.");
            }
            Assertions.Equal(SummoningCurrentSurface.RegisteredLogicalPlacements, all.Length, "Current registered logical roots derive from the catalog; exact Sprint29 tiers remain checked above.");
            Assertions.Equal(SummoningCurrentSurface.FinalPublishedLogicalPlacements, all.Count(SummonVisibilityCatalog.IsPublished),
                "All eight new roots join the preserved published roots.");
            Assertions.True(all.Where(value => !SummoningCurrentSurface.IsWithheld(value.Creature.Key))
                .All(SummonVisibilityCatalog.IsPublished),
                "No released root may become suppressed by a later hidden candidate.");
        }

        internal static void UnexpectedSuppressionOrMissingIdentityIsRejected()
        {
            var field = typeof(SummonVisibilityCatalog).GetField(
                "SuppressedCreatureKeys", BindingFlags.Static | BindingFlags.NonPublic);
            var suppressed = (HashSet<string>)field.GetValue(null);
            Assertions.True(suppressed.Add("storm-giant"),
                "Storm is public before the negative check.");
            try
            {
                Assertions.Throws<InvalidOperationException>(
                    () => SummonVisibilityCatalog.Validate(),
                    "A reintroduced suppression must fail validation.");
            }
            finally
            {
                suppressed.Remove("storm-giant");
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

        internal static void PublicPersistenceScopeRejectsUnrelatedKeys()
        {
            string scope = ExpandedSummoningRosterPersistencePolicy
                .GiantHumanoidsTwoScope;
            foreach (string key in new[] { "ettin", "cloud-giant",
                "storm-giant" })
            {
                var route = ExpandedSummoningRosterPersistencePolicy
                    .SelectCreatureVariant(scope, key);
                Assertions.True(route.Family == SummonFamily.NaturesAlly &&
                    route.Multiplicity == SummonMultiplicity.One &&
                    SummonVisibilityCatalog.IsPublished(route),
                    "The named persistence fixture selects only its published one-creature route.");
            }
            Assertions.True(ExpandedSummoningRosterPersistencePolicy
                .ExactSelectedKeys(scope, new[] { "storm-giant", "ettin",
                    "cloud-giant" }),
                "Load order may change while exact membership stays fixed.");
            Assertions.False(ExpandedSummoningRosterPersistencePolicy
                .ExactSelectedKeys(scope, new[] { "ettin", "ettin",
                    "cloud-giant" }),
                "A duplicate cannot replace Storm.");
            Assertions.Throws<InvalidOperationException>(() =>
                ExpandedSummoningRosterPersistencePolicy
                    .SelectCreatureVariant(scope, "hill-giant"),
                "The new scope must refuse a released neighboring giant.");
            Assertions.False(ExpandedSummoningRosterPersistencePolicy
                .ObserveLoad("working-save-expanded-summoning-prepare",
                    "Giant-humanoids-ii"),
                "Scope matching stays case-sensitive.");
        }
    }
}
