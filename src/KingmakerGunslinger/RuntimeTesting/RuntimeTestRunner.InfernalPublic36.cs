using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void ReviewInfernalPublic36(
            ExpandedSummoningCorrectionFixture fixture)
        {
            string[] keys = { "babau", "hezrou" };
            var routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(route => keys.Contains(route.Creature.Key))
                .OrderBy(route => route.Creature.Key,
                    StringComparer.Ordinal)
                .ThenBy(route => route.ParentTier).ToArray();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint36InfernalPublicPhase,
                ["finalQualification"] = true,
                ["cells"] = cells
            };
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint36-" + id, pass, row,
                        description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint36 public native cell failed: " + id);
                };
            try
            {
                bool exact = routes.Length == 7 &&
                    routes.Where(route => route.Creature.Key == "babau")
                        .Select(route => route.ParentTier)
                        .SequenceEqual(new[] { 5, 6, 7, 8, 9 }) &&
                    routes.Where(route => route.Creature.Key == "hezrou")
                        .Select(route => route.ParentTier)
                        .SequenceEqual(new[] { 8, 9 }) &&
                    routes.All(route =>
                        SummonVisibilityCatalog.IsPublished(route) &&
                        !route.Creature.MonsterTemplated &&
                        route.Multiplicity == (route.ParentTier ==
                            route.Creature.MonsterTier
                                ? SummonMultiplicity.One
                                : route.ParentTier ==
                                    route.Creature.MonsterTier + 1
                                    ? SummonMultiplicity.OneD3
                                    : SummonMultiplicity.OneD4PlusOne)) &&
                    !ExpandedSummoningCatalog
                        .GenerateVariants(SummonFamily.NaturesAlly)
                        .Any(route => keys.Contains(route.Creature.Key));
                check("public-route-scope", exact, new JObject {
                    ["keys"] = new JArray(keys),
                    ["routes"] = new JArray(routes.Select(route =>
                        new JObject {
                            ["key"] = route.StableKey,
                            ["tier"] = route.ParentTier,
                            ["quantity"] = route.Multiplicity.ToString(),
                            ["published"] =
                                SummonVisibilityCatalog.IsPublished(route)
                        }))
                }, "Exactly Babau V-IX and Hezrou VIII-IX are public as seven untemplated Monster-only routes with their required quantities.");
                if (!exact) throw new InvalidOperationException(
                    "Sprint36 public route scope is not ready.");

                foreach (string key in keys)
                {
                    var icon = ExpandedSummoningProjectIcons.Require(key);
                    var consumers = routes.Where(route =>
                        route.Creature.Key == key).Select(route => {
                            var root = ExpandedSummoningRoot(
                                fixture.Blueprints, route);
                            return new JObject {
                                ["route"] = route.StableKey,
                                ["ability"] = root.AssetGuid,
                                ["sprite"] = root.Icon?.GetInstanceID(),
                                ["expectedSprite"] = icon.GetInstanceID()
                            };
                        }).ToArray();
                    bool correct = icon.rect.width == 128 &&
                        icon.rect.height == 128 && consumers.All(consumer =>
                            (int?)consumer["sprite"] ==
                                icon.GetInstanceID()) &&
                        ExpandedSummoningProjectIcons.FallbackCount == 0;
                    check("public-icon-" + key, correct,
                        new JObject {
                            ["key"] = key,
                            ["consumers"] = new JArray(consumers),
                            ["loadedSprite"] =
                                CaptureSprint26ReferenceIcon(icon,
                                    "s36-" + key),
                            ["captureScope"] =
                                "Actual loaded project sprite pixels; not menu navigation or owner aesthetic approval."
                        }, "Every public route carries its creature's actual 128px medallion without fallback.");
                }

                Sprint24PlayerRoutes(fixture, routes,
                    (id, pass, row, description) => check(
                        "public-" + id, pass, row, description));
            }
            finally
            {
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint36-infernal-public.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }
    }
}
