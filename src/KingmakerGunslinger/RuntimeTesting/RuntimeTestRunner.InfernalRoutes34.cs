using System;
using System.Linq;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.Blueprints;
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
            foreach (string key in keys) {
                var icon = ExpandedSummoningProjectIcons.Require(key);
                var consumers = routes.Where(route => route.Creature.Key == key).Select(route => {
                    var root = ExpandedSummoningRoot(fixture.Blueprints, route);
                    return new JObject { ["route"] = route.StableKey, ["ability"] = root.AssetGuid,
                        ["sprite"] = root.Icon?.GetInstanceID(), ["expectedSprite"] = icon.GetInstanceID() };
                }).ToArray();
                bool correct = icon.rect.width == 128 && icon.rect.height == 128 &&
                    consumers.All(consumer => (int?)consumer["sprite"] == icon.GetInstanceID()) && ExpandedSummoningProjectIcons.FallbackCount == 0;
                var row = new JObject { ["key"] = key, ["consumers"] = new JArray(consumers),
                    ["loadedSprite"] = CaptureSprint26ReferenceIcon(icon, "s34-" + key),
                    ["captureScope"] = "Actual loaded project sprite pixels; not menu navigation or owner aesthetic approval." };
                Sprint25Check("sprint34-icon-" + key, correct, row,
                    "Every public route carries its creature's real 128px medallion, without fallback.");
                if (!correct) throw new InvalidOperationException("Sprint34 route icon consumer differs: " + key);
            }
            Sprint24PlayerRoutes(fixture, routes, (id, passed, row, description) =>
                Sprint25Check("sprint34-" + id, passed, row, description));
        }
    }
}
