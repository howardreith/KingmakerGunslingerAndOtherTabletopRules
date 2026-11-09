using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string PharasmaFeatureGuid = "458750bc214ab2e44abdeae404ab22e9";

        private static BlueprintAbility SpiderRow(LibraryScriptableObject library, SummonFamily family, string key)
        {
            BlueprintScriptableObject found;
            library.BlueprintsByAssetId.TryGetValue(SpiderSummonerCatalog.RowIdentity(family, SpiderSummonerCatalog.Find(key)), out found);
            return found as BlueprintAbility;
        }

        private static BlueprintAbility SpiderGenerated(LibraryScriptableObject library, SpiderPlacement placement)
        {
            return library.BlueprintsByAssetId.Values.OfType<BlueprintAbility>()
                .SingleOrDefault(value => SpiderSummonerIntegration.PlacementOf(value) != null && SpiderSummonerIntegration.PlacementOf(value).Symbol == placement.Symbol);
        }

        /// <summary>The structural manifest: every matrix cell, every original
        /// row and every contract, in both families, read from the live game.</summary>
        private void SpiderCensus(LibraryScriptableObject library)
        {
            var rows = new JArray(); int wrong = 0, published = 0;
            foreach (SpiderPlacement placement in SpiderSummonerCatalog.AllPlacements)
            {
                BlueprintAbility row = placement.IsOriginal ? SpiderRow(library, placement.Family, placement.Species.Key) :
                    (SpiderSummonerCatalog.Generated.Any(value => value.Symbol == placement.Symbol) ? SpiderGenerated(library, placement) : null);
                int offset = placement.Family == SummonFamily.Monster ? 0 : 9;
                BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, ExpandedSummoningPublisher.CanonicalParentGuids[offset + placement.Tier - 1], "spider census parent");
                int listed = row == null ? 0 : (parent.Variants ?? new BlueprintAbility[0]).Count(value => ReferenceEquals(value, row));
                SummonSignature graph = row == null ? null : SummonSignatureReader.Read(row);
                SummonSpawn[] spawns = graph == null ? new SummonSpawn[0] : graph.Spawns().ToArray();
                BlueprintComponent[] components = row == null ? new BlueprintComponent[0] : row.ComponentsArray;
                bool gated = components.OfType<AbilityCasterHasFacts>().Any(value => value.Facts.Any(fact => fact != null && fact.AssetGuid == SummonForeignRowClassifier.SpiderSummonerFeatureGuid)) &&
                    components.OfType<AbilityShowIfCasterHasFact>().Any(value => value.UnitFact != null && value.UnitFact.AssetGuid == SummonForeignRowClassifier.SpiderSummonerFeatureGuid);
                string dynamicTemplate = SummonForeignRowClassifier.TemplateAction;
                string detail = spawns.Length == 0 ? "none" : string.Join(" || ", spawns.Select(value => string.Join(",", value.AfterSpawnDetail.ToArray())).ToArray());
                bool expectedShape = spawns.Length > 0 && spawns.All(value => value.Unit == placement.Species.UnitGuid &&
                    value.AfterSpawnDetail.Count(item => item == SummonForeignRowClassifier.BenefitAction) == 1 &&
                    (placement.Family == SummonFamily.Monster ? value.AfterSpawnDetail.Count(item => item == dynamicTemplate) == 1 :
                        !value.AfterSpawnDetail.Contains(dynamicTemplate) && value.AfterSpawnDetail.Count(item => item == SummonForeignRowClassifier.AlignmentAction) == 1));
                string quantity = string.Join(" || ", spawns.Select(value => value.CountDice + "x" + value.CountDiceCount + "+" + value.CountBonus).ToArray());
                SummonMultiplicity want = placement.Quantity;
                bool quantityOk = spawns.Length > 0 && spawns.All(value => want == SummonMultiplicity.One ? value.CountDice == "Zero" && value.CountBonus.Value == 1 :
                    SpiderSummonerCatalog.QuantityExpressionMatches(value.CountDice, value.CountDiceCount.Kind, value.CountDiceCount.Value, value.CountBonus.Kind, want));
                bool original = placement.IsOriginal && row != null && row.AssetGuid.Length != 32;
                bool ok = row != null && (placement.IsOriginal ? original : true) && listed == 1 && gated && expectedShape && quantityOk &&
                    !components.OfType<AbilityVariants>().Any();
                if (row != null) published++;
                if (!ok) wrong++;
                rows.Add(new JObject { ["symbol"] = placement.Symbol, ["family"] = placement.Family.ToString(), ["tier"] = placement.Tier, ["species"] = placement.Species.Key, ["quantity"] = want.ToString(),
                    ["row"] = row?.AssetGuid, ["name"] = row?.Name, ["listed"] = listed, ["gated"] = gated, ["unit"] = new JArray(spawns.Select(value => value.Unit).Distinct()), ["spawnBranches"] = spawns.Length, ["count"] = quantity, ["postSpawn"] = detail, ["ok"] = ok });
            }
            Sprint25Check("spider-matrix-census", wrong == 0 && published == 46, new JObject { ["rows"] = rows, ["published"] = published, ["wrong"] = wrong,
                ["outcome"] = SpiderSummonerIntegration.Outcome, ["scope"] = "Structural manifest of all 46 placements read from the loaded rows." },
                "All 46 spider placements are published once under their exact parent, feature gated, with the right unit, quantity and family template behavior.");
        }

        private static JObject SpiderDcEvidence(UnitEntityData unit, BlueprintFeature poison, bool expectBonus)
        {
            Kingmaker.UnitLogic.Feature fact = poison == null ? null : unit.Descriptor.Progression.Features.Enumerable.FirstOrDefault(value => ReferenceEquals(value?.Blueprint, poison));
            int withMarker = -1, recalculated = -1, withoutMarker = -1, restored = -1;
            BlueprintBuff marker = SpiderSummonerRuntime.Marker;
            Buff held = marker == null ? null : unit.Descriptor.Buffs.GetBuff(marker);
            bool hadMarker = held != null;
            if (fact != null && fact.MaybeContext != null)
            {
                withMarker = fact.MaybeContext.Params.DC;
                fact.MaybeContext.Recalculate();
                recalculated = fact.MaybeContext.Params.DC;
                if (held != null)
                {
                    unit.Descriptor.Buffs.RemoveFact(held);
                    fact.MaybeContext.Recalculate();
                    withoutMarker = fact.MaybeContext.Params.DC;
                    unit.Descriptor.AddBuff(marker, unit, null);
                    fact.MaybeContext.Recalculate();
                    restored = fact.MaybeContext.Params.DC;
                }
            }
            bool ok = expectBonus ? hadMarker && withMarker == recalculated && withMarker == withoutMarker + SpiderSummonerCatalog.DcBonus && restored == withMarker :
                !hadMarker;
            return new JObject { ["unit"] = unit.Blueprint.AssetGuid, ["poison"] = poison?.AssetGuid, ["marker"] = hadMarker, ["dcWithMarker"] = withMarker, ["dcAfterRecalculation"] = recalculated, ["dcWithoutMarker"] = withoutMarker, ["dcRestored"] = restored, ["expectBonus"] = expectBonus, ["ok"] = ok };
        }

        private static JObject SpiderTemplateEvidence(UnitEntityData unit, bool expectCelestial)
        {
            ExpandedSummoningBlueprintSet set = ExpandedSummoningBlueprints.Registered;
            Func<string, BlueprintBuff> buff = symbol => (BlueprintBuff)set.BySymbol[symbol];
            int hd = ExpandedSummoningAbilityBuilder.HitDiceOf(unit.Blueprint);
            SummonTemplateBand band = SummonTemplateBandPolicy.Select(hd);
            bool celestial = unit.Descriptor.Buffs.GetBuff(buff("KMG.Summoning.Template.Celestial." + band)) != null;
            bool fiendish = unit.Descriptor.Buffs.GetBuff(buff("KMG.Summoning.Template.Fiendish." + band)) != null;
            bool nativeLeftover = ExpandedSummoningAbilityBuilder.NativeTemplateIds.Any(guid => unit.Descriptor.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid == guid));
            bool ok = expectCelestial ? celestial && !fiendish && !nativeLeftover : fiendish && !celestial && !nativeLeftover;
            return new JObject { ["unit"] = unit.Blueprint.AssetGuid, ["hitDice"] = hd, ["band"] = band.ToString(), ["celestial"] = celestial, ["fiendish"] = fiendish, ["nativeTemplateLeftover"] = nativeLeftover,
                ["alignment"] = unit.Descriptor.Alignment.Value.ToString(), ["ok"] = ok };
        }

        private IEnumerable<int> SpiderMenuCast(ExpandedSummoningCorrectionFixture fixture, Spellbook book, int tier, BlueprintAbility parent, AbilityData prepared, BlueprintAbility row,
            Cleanup2NativeDefaultScope mode, Stopwatch watch, ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, string caseKey, string expectedUnit, Action<UnitEntityData[]> inspect, Func<int, bool> countOk)
        {
            AlphaMenu menu = null; ActionBarGroupSlot source = null;
            foreach (int frame in ClericOpenMenu("spider-" + caseKey, spellGroup, levels, tier, book, parent, prepared, mode, watch, null, (value, slot) => { menu = value; source = slot; }, false)) yield return frame;
            int index = menu == null || row == null ? -1 : Array.FindIndex(menu.Data, value => ReferenceEquals(value?.Blueprint, row));
            if (index < 0)
            {
                Sprint25Check("spider-" + caseKey + "-row-present", false, new JObject { ["rows"] = menu?.Rows.Length ?? 0 }, "The spider row is offered in the real popup.");
                if (source != null) Cleanup2MenuAdapterHide(source, prepared);
                yield break;
            }
            var debit = new Cleanup2SourceDebit(fixture.Caster);
            ActionBarGroupSlot close = source;
            foreach (int frame in ClericLegalCast(fixture, menu.Rows[index], menu.Data[index], prepared, debit, () => Cleanup2MenuAdapterHide(close, prepared), mode, watch, "spider-" + caseKey, expectedUnit, inspect, countOk)) yield return frame;
            Cleanup2MenuAdapterHide(source, prepared);
            book.Rest();
        }

        private IEnumerable<int> SpiderSummonerCells(ExpandedSummoningCorrectionFixture fixture, Spellbook book, BlueprintFeature spiderFeature, Cleanup2NativeDefaultScope mode, Stopwatch watch,
            ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, Func<int, BlueprintAbility> smParent, Func<int, AbilityData> preparedParent, Action<BlueprintFeature> setDeity,
            BlueprintFeature erastil, BlueprintFeature asmodeus)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            Func<string, int, BlueprintAbility> row = (key, tier) => SpiderRow(library, SummonFamily.Monster, key);
            Func<string, BlueprintFeature> poison = key => BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SpiderSummonerCatalog.Find(key).PoisonFeatureGuid, "spider poison feature");
            BlueprintUnit crabUnit = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library, SpiderSummonerCatalog.Find("giant-crab-spider").UnitGuid, "crab");
            SpiderCensus(library);
            Action<string, UnitEntityData[], bool, string, bool> check = (key, units, celestial, species, expectBonus) =>
            {
                JObject[] templates = units.Select(value => SpiderTemplateEvidence(value, celestial)).ToArray();
                JObject[] dcs = units.Select(value => SpiderDcEvidence(value, poison(species), expectBonus)).ToArray();
                Sprint25Check("spider-effect-" + key, units.Length > 0 && templates.All(value => (bool)value["ok"]) && dcs.All(value => (bool)value["ok"]),
                    new JObject { ["templates"] = new JArray(templates), ["dcs"] = new JArray(dcs), ["count"] = units.Length }, "The summoned spiders carry their one selected template and the exact DC benefit.");
            };
            fixture.Caster.Descriptor.Alignment.Set(Alignment.LawfulGood); setDeity(erastil);
            var originals = new[] { new { Key = "giant-crab-spider", Tier = 1 }, new { Key = "giant-black-widow", Tier = 4 }, new { Key = "ogre-spider", Tier = 5 }, new { Key = "giant-tarantula", Tier = 7 } };
            foreach (var original in originals)
            {
                var scoped = original;
                string unit = SpiderSummonerCatalog.Find(scoped.Key).UnitGuid;
                foreach (int frame in R6Cell("spider-sm-" + scoped.Key, () => SpiderMenuCast(fixture, book, scoped.Tier, smParent(scoped.Tier), preparedParent(scoped.Tier), row(scoped.Key, scoped.Tier), mode, watch, spellGroup, levels,
                    "sm-" + scoped.Key, unit, units => check("sm-" + scoped.Key, units, true, scoped.Key, true), null))) yield return frame;
            }
            // The formerly Evil branch, repaired: an evil caster's Ogre is still the spider.
            fixture.Caster.Descriptor.Alignment.Set(Alignment.LawfulEvil); setDeity(asmodeus);
            foreach (int frame in R6Cell("spider-sm-ogre-evil", () => SpiderMenuCast(fixture, book, 5, smParent(5), preparedParent(5), row("ogre-spider", 5), mode, watch, spellGroup, levels,
                "sm-ogre-evil", SpiderSummonerCatalog.Find("ogre-spider").UnitGuid, units => check("sm-ogre-evil", units, false, "ogre-spider", true), null))) yield return frame;
            // Neutral caster: the existing toggle is read at execution.
            fixture.Caster.Descriptor.Alignment.Set(Alignment.TrueNeutral);
            setDeity(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, PharasmaFeatureGuid, "neutral deity"));
            BlueprintBuff fiendishMode = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol["KMG.Summoning.AlignmentMode.FiendishMarker"];
            string tarantulaUnit = SpiderSummonerCatalog.Find("giant-tarantula").UnitGuid;
            foreach (int frame in R6Cell("spider-sm-neutral-off", () => SpiderMenuCast(fixture, book, 7, smParent(7), preparedParent(7), row("giant-tarantula", 7), mode, watch, spellGroup, levels,
                "sm-neutral-celestial", tarantulaUnit, units => check("sm-neutral-celestial", units, true, "giant-tarantula", true), null))) yield return frame;
            Buff modeBuff = fixture.Caster.Descriptor.AddBuff(fiendishMode, fixture.Caster, null);
            foreach (int frame in R6Cell("spider-sm-neutral-on", () => SpiderMenuCast(fixture, book, 7, smParent(7), preparedParent(7), row("giant-tarantula", 7), mode, watch, spellGroup, levels,
                "sm-neutral-fiendish", tarantulaUnit, units => check("sm-neutral-fiendish", units, false, "giant-tarantula", true), null))) yield return frame;
            if (modeBuff != null) fixture.Caster.Descriptor.Buffs.RemoveFact(modeBuff);
            // Higher-tier quantity: every spawned creature carries the template and the benefit.
            fixture.Caster.Descriptor.Alignment.Set(Alignment.LawfulGood); setDeity(erastil);
            SpiderPlacement crabD4 = SpiderSummonerCatalog.Placements(SummonFamily.Monster).Single(value => value.Species.Key == "giant-crab-spider" && value.Tier == 4);
            BlueprintAbility crabD4Row = SpiderGenerated(library, crabD4);
            foreach (int frame in R6Cell("spider-sm-crab-d4", () => SpiderMenuCast(fixture, book, 4, smParent(4), preparedParent(4), crabD4Row, mode, watch, spellGroup, levels,
                "sm-crab-d4", crabUnit.AssetGuid, units => check("sm-crab-d4", units, true, "giant-crab-spider", true), count => count >= 2 && count <= 5))) yield return frame;
            SpiderPlacement widowD3 = SpiderSummonerCatalog.Placements(SummonFamily.Monster).Single(value => value.Species.Key == "giant-black-widow" && value.Tier == 5);
            BlueprintAbility widowD3Row = SpiderGenerated(library, widowD3);
            foreach (int frame in R6Cell("spider-sm-widow-d3", () => SpiderMenuCast(fixture, book, 5, smParent(5), preparedParent(5), widowD3Row, mode, watch, spellGroup, levels,
                "sm-widow-d3", SpiderSummonerCatalog.Find("giant-black-widow").UnitGuid, units => check("sm-widow-d3", units, true, "giant-black-widow", true), count => count >= 1 && count <= 3))) yield return frame;
            // The ordinary Giant Spider gains the DC benefit only from the feat.
            SummonVariantSpec ordinary = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Single(value => value.Creature.Key == "giant-spider" && value.ParentTier == 2 && value.Multiplicity == SummonMultiplicity.One);
            BlueprintAbility ordinaryRow = ExpandedSummoningRoot(fixture.Blueprints, ordinary);
            BlueprintFeature ordinaryPoison = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SpiderSummonerCatalog.GiantSpiderPoisonFeature, "giant spider poison");
            Action<string, UnitEntityData[], bool> ordinaryCheck = (key, units, expect) =>
            {
                JObject[] dcs = units.Select(value => SpiderDcEvidence(value, ordinaryPoison, expect)).ToArray();
                Sprint25Check("spider-ordinary-" + key, units.Length == 1 && dcs.All(value => (bool)value["ok"]), new JObject { ["dcs"] = new JArray(dcs) }, "The ordinary Giant Spider's poison DC reflects only the caster's feat.");
            };
            foreach (int frame in R6Cell("spider-ordinary-with-feat", () => SpiderMenuCast(fixture, book, 2, smParent(2), preparedParent(2), ordinaryRow, mode, watch, spellGroup, levels,
                "ordinary-with-feat", SpiderSummonerCatalog.OrdinaryGiantSpiderUnit, units => ordinaryCheck("with-feat", units, true), null))) yield return frame;
            Kingmaker.Blueprints.Facts.Fact held = fixture.Caster.Descriptor.Progression.Features.Enumerable.FirstOrDefault(value => ReferenceEquals(value?.Blueprint, spiderFeature));
            if (held != null) fixture.Caster.Descriptor.RemoveFact(held);
            foreach (int frame in R6Cell("spider-ordinary-without-feat", () => SpiderMenuCast(fixture, book, 2, smParent(2), preparedParent(2), ordinaryRow, mode, watch, spellGroup, levels,
                "ordinary-without-feat", SpiderSummonerCatalog.OrdinaryGiantSpiderUnit, units => ordinaryCheck("without-feat", units, false), null))) yield return frame;
            // A held selection after the feature is gone is refused before any spend.
            foreach (int frame in R6Cell("spider-feature-lost-held", () => ClericForbiddenAttempt(fixture, row("giant-crab-spider", 1), book, preparedParent(1), mode, watch, "spider-feature-lost", false))) yield return frame;
            fixture.Caster.Descriptor.AddFact(spiderFeature);
            Sprint25Check("spider-surface-limits", true, new JObject {
                ["observed"] = "Summon Monster casts of all four original rows, repaired Ogre under evil and non-evil casters, neutral toggle, one 1d4+1 and one 1d3 quantity, ordinary Giant Spider with and without the feat, held selection after feature loss, and the 46-placement census.",
                ["notObserved"] = "Summon Nature's Ally casts and Druid conversion casts; Herald quantity-tier casts; natural Drow feat acquisition; multiclass and ordinary spontaneous spellbooks; Augment and Superior Summoning interaction; poison save resolution and webbed break-free attempts; reload and module-off or RU-absent profiles." },
                "Observed and unobserved surfaces are reported separately.");
        }
    }
}
