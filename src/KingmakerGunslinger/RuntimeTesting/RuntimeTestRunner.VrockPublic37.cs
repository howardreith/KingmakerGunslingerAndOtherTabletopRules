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
        /// <summary>
        /// The three public Vrock routes, their quantities, their medallion
        /// and the real player path through them. This is the phase that
        /// carries final qualification for the public surface.
        /// </summary>
        private void ReviewVrockPublic37(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(route => route.Creature.Key == "vrock")
                .OrderBy(route => route.ParentTier).ToArray();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint37VrockPublicPhase,
                ["finalQualification"] = true,
                ["cells"] = cells
            };
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint37-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint37 vrock public cell failed: " + id);
                };
            try
            {
                bool exact = routes.Length == 3 &&
                    routes.Select(route => route.ParentTier)
                        .SequenceEqual(new[] { 7, 8, 9 }) &&
                    routes.All(route =>
                        SummonVisibilityCatalog.IsPublished(route) &&
                        !route.Creature.MonsterTemplated) &&
                    routes.Select(route => route.Multiplicity)
                        .SequenceEqual(new[] { SummonMultiplicity.One,
                            SummonMultiplicity.OneD3,
                            SummonMultiplicity.OneD4PlusOne }) &&
                    !ExpandedSummoningCatalog
                        .GenerateVariants(SummonFamily.NaturesAlly)
                        .Any(route => route.Creature.Key == "vrock");
                check("public-route-scope", exact, new JObject {
                    ["key"] = "vrock",
                    ["routes"] = new JArray(routes.Select(route =>
                        new JObject {
                            ["key"] = route.StableKey,
                            ["tier"] = route.ParentTier,
                            ["quantity"] = route.Multiplicity.ToString(),
                            ["published"] =
                                SummonVisibilityCatalog.IsPublished(route),
                            ["templated"] = route.Creature.MonsterTemplated
                        })),
                    ["naturesAllyRoutes"] = 0
                }, "Exactly Summon Monster VII, VIII and IX carry the Vrock as three untemplated Monster-only routes at one, 1d3 and 1d4+1.");
                if (!exact) throw new InvalidOperationException(
                    "Sprint37 public route scope is not ready.");

                var icon = ExpandedSummoningProjectIcons.Require("vrock");
                var consumers = routes.Select(route => {
                    var root = ExpandedSummoningRoot(fixture.Blueprints,
                        route);
                    return new JObject {
                        ["route"] = route.StableKey,
                        ["ability"] = root.AssetGuid,
                        ["sprite"] = root.Icon?.GetInstanceID(),
                        ["expectedSprite"] = icon.GetInstanceID()
                    };
                }).ToArray();
                check("public-icon-vrock",
                    icon.rect.width == 128 && icon.rect.height == 128 &&
                    consumers.All(consumer =>
                        (int?)consumer["sprite"] == icon.GetInstanceID()) &&
                    ExpandedSummoningProjectIcons.FallbackCount == 0,
                    new JObject {
                        ["key"] = "vrock",
                        ["consumers"] = new JArray(consumers),
                        ["loadedSprite"] = CaptureSprint26ReferenceIcon(icon,
                            "s37-vrock"),
                        ["captureScope"] =
                            "Actual loaded project sprite pixels; not menu navigation or owner aesthetic approval."
                    }, "All three public routes carry the Vrock's own 128px medallion without any fallback.");

                Sprint24PlayerRoutes(fixture, routes,
                    (id, pass, row, description) => check(
                        "public-" + id, pass, row, description));
            }
            finally
            {
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint37-vrock-public.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }
    }
}
