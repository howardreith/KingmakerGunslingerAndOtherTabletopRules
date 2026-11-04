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
        /// The one public Nalfeshnee route, its quantity, its medallion and
        /// the real player path through it. This is the phase that carries
        /// final qualification for the public surface.
        ///
        /// <para>The scope cell is deliberately exact rather than
        /// permissive. One untemplated Summon Monster IX route at a single
        /// creature is the whole charter: a second tier, a quantity variant,
        /// an alignment-templated child or a Nature's Ally entry would each
        /// be a silent widening, so each is asserted absent by name rather
        /// than left unmentioned.</para>
        /// </summary>
        private void ReviewNalfeshneePublic39(
            ExpandedSummoningCorrectionFixture fixture)
        {
            SummonVariantSpec[] routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(route => route.Creature.Key ==
                    NalfeshneeVisualPolicy39.Nalfeshnee)
                .OrderBy(route => route.ParentTier).ToArray();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint39NalfeshneePublicPhase,
                ["finalQualification"] = true,
                ["cells"] = cells
            };
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint39-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint39 nalfeshnee public cell failed: " + id);
                };
            try
            {
                bool exact = routes.Length == 1 &&
                    routes[0].ParentTier == 9 &&
                    routes[0].Multiplicity == SummonMultiplicity.One &&
                    !routes[0].Creature.MonsterTemplated &&
                    SummonVisibilityCatalog.IsPublished(routes[0]) &&
                    !ExpandedSummoningCatalog
                        .GenerateVariants(SummonFamily.NaturesAlly)
                        .Any(route => route.Creature.Key ==
                            NalfeshneeVisualPolicy39.Nalfeshnee);
                check("public-route-scope", exact, new JObject {
                    ["key"] = NalfeshneeVisualPolicy39.Nalfeshnee,
                    ["routes"] = new JArray(routes.Select(route =>
                        new JObject {
                            ["key"] = route.StableKey,
                            ["tier"] = route.ParentTier,
                            ["quantity"] = route.Multiplicity.ToString(),
                            ["published"] =
                                SummonVisibilityCatalog.IsPublished(route),
                            ["templated"] = route.Creature.MonsterTemplated
                        })),
                    ["monsterRouteCount"] = routes.Length,
                    ["naturesAllyRoutes"] = ExpandedSummoningCatalog
                        .GenerateVariants(SummonFamily.NaturesAlly)
                        .Count(route => route.Creature.Key ==
                            NalfeshneeVisualPolicy39.Nalfeshnee),
                    // Publication is asked of the one authoritative
                    // reader every consumer uses, rather than of the
                    // withheld set behind it: the reader is what the menus,
                    // the roster and the choice presentation all consult, so
                    // it is the answer that can actually be wrong.
                    ["publishedByCatalogReader"] =
                        SummonVisibilityCatalog.IsPublished(routes[0]),
                    ["charter"] = "One adult untemplated Nalfeshnee at Summon Monster IX only: no lower tier, no quantity variant, no alignment-templated child and no Nature's Ally entry."
                }, "Exactly one published, untemplated Summon Monster IX route carries a single Nalfeshnee, with no lower tier, no quantity variant and no Nature's Ally entry.");
                if (!exact) throw new InvalidOperationException(
                    "Sprint39 public route scope is not ready.");

                var icon = ExpandedSummoningProjectIcons.Require(
                    NalfeshneeVisualPolicy39.Nalfeshnee);
                var consumers = routes.Select(route => {
                    var root = ExpandedSummoningRoot(fixture.Blueprints,
                        route);
                    return new JObject {
                        ["route"] = route.StableKey,
                        ["ability"] = root.AssetGuid,
                        ["sprite"] = root.Icon == null ? null :
                            (int?)root.Icon.GetInstanceID(),
                        ["expectedSprite"] = icon.GetInstanceID()
                    };
                }).ToArray();
                check("public-icon-nalfeshnee",
                    icon.rect.width == 128 && icon.rect.height == 128 &&
                    consumers.All(consumer =>
                        (int?)consumer["sprite"] == icon.GetInstanceID()) &&
                    ExpandedSummoningProjectIcons.FallbackCount == 0,
                    new JObject {
                        ["key"] = NalfeshneeVisualPolicy39.Nalfeshnee,
                        ["consumers"] = new JArray(consumers),
                        ["loadedSprite"] = CaptureSprint26ReferenceIcon(icon,
                            "s39-nalfeshnee"),
                        ["captureScope"] =
                            "Actual loaded project sprite pixels; not menu navigation or owner aesthetic approval."
                    }, "The public route carries the Nalfeshnee's own 128px medallion with no fallback anywhere in the registry.");

                Sprint24PlayerRoutes(fixture, routes,
                    (id, pass, row, description) => check(
                        "public-" + id, pass, row, description));
            }
            finally
            {
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint39-nalfeshnee-public.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }
    }
}
