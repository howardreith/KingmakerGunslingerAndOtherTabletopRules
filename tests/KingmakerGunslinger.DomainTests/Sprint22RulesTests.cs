using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Sprint22RulesTests
    {
        internal static void PublishedRoutesRemainAtFiveThroughNine()
        {
            ExpandedSummoningCatalog.Validate();
            foreach (var family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
            {
                var routes = ExpandedSummoningCatalog.GenerateVariants(family).Where(value => value.Creature.Key == "ankylosaurus").OrderBy(value => value.ParentTier).ToArray();
                Assertions.Equal(5, routes.Length, "Five existing routes in " + family);
                for (int i = 0; i < routes.Length; i++)
                {
                    Assertions.Equal(i + 5, routes[i].ParentTier, "Existing parent level.");
                    Assertions.Equal(5, routes[i].SourceTier, "Creature remains at tier V.");
                    Assertions.Equal(i == 0 ? SummonMultiplicity.One : i == 1 ? SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne, routes[i].Multiplicity, "Existing quantities.");
                }
            }
            var creature = ExpandedSummoningCatalog.All.Single(value => value.Key == "ankylosaurus");
            Assertions.Equal(SummonTemplatePolicy.CelestialOrFiendish, creature.TemplatePolicy(SummonFamily.Monster), "SM children retained.");
            Assertions.Equal(SummonTemplatePolicy.CasterAlignment, creature.TemplatePolicy(SummonFamily.NaturesAlly), "SNA behavior retained.");
        }

        internal static void DonorProbeHasAClosedSelection()
        {
            Sprint22DonorSurveyPolicy.Validate();
            Assertions.True(Sprint22DonorSurveyPolicy.Donors.Select(value => value[0]).SequenceEqual(new[] { "hodag", "monitor", "mastodon" }), "Only relevant quadrupeds.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(RuntimeTestScenarioCatalog.ObserveExpandedSummoningSprint22Contracts), "Production request allowlist resolves.");
            Assertions.True(!RuntimeTestScenarioCatalog.IsAllowed("observe-expanded-summoning-sprint22-census-all"), "Unselected broad census rejected.");
        }
    }
}
