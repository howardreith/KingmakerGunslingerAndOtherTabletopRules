using System;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void ReviewInfernalRoutes34(ExpandedSummoningCorrectionFixture fixture)
        {
            string[] keys = { "bearded-devil", "kyton-chain-devil" };
            var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Where(route => keys.Contains(route.Creature.Key)).OrderBy(route => route.Creature.Key)
                .ThenBy(route => route.ParentTier).ToArray();
            bool exact = keys.All(key => routes.Where(route => route.Creature.Key == key)
                .Select(route => route.ParentTier).SequenceEqual(Enumerable.Range(5, 5))) &&
                routes.All(route => SummonVisibilityCatalog.IsPublished(route) && !route.Creature.MonsterTemplated &&
                    route.Multiplicity == (route.ParentTier == 5 ? SummonMultiplicity.One : route.ParentTier == 6 ?
                        SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne)) &&
                !ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Any(route => keys.Contains(route.Creature.Key));
            Sprint25Check("sprint34-public-route-scope", exact, new JObject { ["keys"] = new JArray(keys),
                ["routes"] = new JArray(routes.Select(route => route.StableKey)) },
                "Exactly two untemplated SM V-IX keys are public with their required quantities and no Nature's Ally route.");
            if (!exact) throw new InvalidOperationException("Sprint34 public route scope is not ready.");
            Sprint24PlayerRoutes(fixture, routes, (id, passed, row, description) =>
                Sprint25Check("sprint34-" + id, passed, row, description));
        }
    }
}
