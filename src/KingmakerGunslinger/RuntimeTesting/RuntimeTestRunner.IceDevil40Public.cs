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
        private void ReviewIceDevilPublic40(ExpandedSummoningCorrectionFixture fixture)
        {
            var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Where(x => x.Creature.Key == IceDevilVisualPolicy40.Key).ToArray();
            var cells = new JArray();
            var receipt = new JObject { ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilPublicPhase,
                ["cells"] = cells };
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Sprint40 public route: " + id);
            };
            try
            {
                bool exact = routes.Length == 1 && routes[0].ParentTier == 9 &&
                    routes[0].Multiplicity == SummonMultiplicity.One && !routes[0].Creature.MonsterTemplated &&
                    SummonVisibilityCatalog.IsPublished(routes[0]) &&
                    !ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                        .Any(x => x.Creature.Key == IceDevilVisualPolicy40.Key);
                check("public-route-scope", exact,
                    new JObject { ["routes"] = new JArray(routes.Select(x => new JObject {
                        ["key"] = x.StableKey, ["tier"] = x.ParentTier,
                        ["quantity"] = x.Multiplicity.ToString(), ["templated"] = x.Creature.MonsterTemplated,
                        ["published"] = SummonVisibilityCatalog.IsPublished(x) })) },
                    "Exactly one public untemplated SM IX Ice Devil, quantity one; no lower tier, template child or Nature route.");
                var icon = ExpandedSummoningProjectIcons.Require(IceDevilVisualPolicy40.Key);
                var root = ExpandedSummoningRoot(fixture.Blueprints, routes[0]);
                check("public-icon-ice-devil", icon.rect.width == 128 && icon.rect.height == 128 &&
                    ReferenceEquals(root.Icon, icon) && ExpandedSummoningProjectIcons.FallbackCount == 0,
                    new JObject { ["ability"] = root.AssetGuid, ["sprite"] = icon.GetInstanceID(),
                        ["loadedSprite"] = CaptureSprint26ReferenceIcon(icon, "s40-ice-devil"),
                        ["visualApproval"] = "Actual loaded pixels; no owner aesthetic approval claimed." },
                    "The real public root uses its own loaded128px medallion without donor fallback.");
                // Existing native player-route consumer checks slot admission,
                // actual cast, one summoned result and native caster duration.
                Sprint24PlayerRoutes(fixture, routes,
                    (id, pass, row, why) => check("public-" + id, pass, row, why));
            }
            finally
            {
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "sprint40-ice-devil-public.json"), receipt.ToString(Formatting.Indented));
            }
        }
    }
}
