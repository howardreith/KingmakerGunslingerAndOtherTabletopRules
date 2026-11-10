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
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Harmony12;
using System.Reflection;
using UnityEngine;
using Kingmaker.UI.ActionBar;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
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

        /// <summary>
        /// The loaded web consumer, structurally: which action the shared
        /// webbed buff runs each round and where its DC comes from, and the DC
        /// that context chain carries with and without the marker. No target
        /// is webbed and no escape is rolled; that remains a separate observation.
        /// </summary>
        private void SpiderWebConsumerEvidence(UnitEntityData spider)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            BlueprintAbility web = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, SpiderSummonerCatalog.GiantSpiderWebAbility, "giant spider web");
            BlueprintBuff webbed = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, ExpandedSummoningSpecialBuilder.NativeWebGrappledGuid, "native webbed state");
            var actions = new JArray(); bool readsDc = false;
            foreach (AddFactContextActions component in (webbed.ComponentsArray ?? new BlueprintComponent[0]).OfType<AddFactContextActions>())
                foreach (GameAction action in component.NewRound == null || component.NewRound.Actions == null ? new GameAction[0] : component.NewRound.Actions)
                {
                    if (action == null) continue;
                    var native = action as ContextActionBreakFree;
                    string source = native != null && action.GetType() == typeof(ContextActionBreakFree) ?
                        (native.UseCMD ? "grappler-cmd-no-dc" : native.UseCMB ? "params-dc-on-skill-branch;grapple-maneuver-vs-grappler-cmd-when-cmb-higher" : "params-dc") :
                        action.GetType().Name == "ContextActionBreakFreeFromSpellGrapple" ? "params-dc-on-both-branches(third-party-replacement)" : "unclassified";
                    readsDc |= source.StartsWith("params-dc", StringComparison.Ordinal);
                    actions.Add(new JObject { ["type"] = action.GetType().FullName, ["useCMB"] = native?.UseCMB, ["useCMD"] = native?.UseCMD, ["dcSource"] = source });
                }
            BlueprintBuff marker = SpiderSummonerRuntime.Marker;
            Buff held = marker == null ? null : spider.Descriptor.Buffs.GetBuff(marker);
            var cast = new MechanicsContext(spider, spider.Descriptor, web);
            var state = new MechanicsContext(spider, spider.Descriptor, webbed, cast);
            cast.Recalculate(); state.Recalculate();
            int withMarker = cast.Params.DC, stateWith = state.Params.DC, withoutMarker = -1, stateWithout = -1;
            if (held != null)
            {
                spider.Descriptor.Buffs.RemoveFact(held);
                cast.Recalculate(); state.Recalculate();
                withoutMarker = cast.Params.DC; stateWithout = state.Params.DC;
                spider.Descriptor.AddBuff(marker, spider, null);
            }
            bool ok = held != null && actions.Count > 0 && readsDc && withMarker == withoutMarker + SpiderSummonerCatalog.DcBonus && stateWith == withMarker && stateWithout == withoutMarker;
            Sprint25Check("spider-web-consumer", ok, new JObject { ["webbedNewRound"] = actions, ["webDcWithMarker"] = withMarker, ["webDcWithoutMarker"] = withoutMarker,
                ["webbedContextDcWithMarker"] = stateWith, ["webbedContextDcWithoutMarker"] = stateWithout, ["markerPresent"] = held != null,
                ["scope"] = "Structural: the loaded break-free action and the DC its context chain carries. No web hit or escape roll was observed." },
                "The webbed state's loaded break-free action reads the web's calculated DC on at least one branch, and that DC carries the +2 exactly once.");
        }

        /// <summary>Records the escape rules one escapee triggers: each grapple
        /// maneuver's CMD target number and each skill check's DC.</summary>
        private sealed class SpiderEscapeRuleObserver : IGlobalRulebookHandler<RuleCombatManeuver>, IGlobalRulebookHandler<RuleSkillCheck>
        {
            internal UnitEntityData Escapee;
            internal readonly List<JObject> Rules = new List<JObject>();
            public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }
            public void OnEventDidTrigger(RuleCombatManeuver evt)
            {
                if (evt == null || !ReferenceEquals(evt.Initiator, Escapee)) return;
                Rules.Add(new JObject { ["rule"] = "RuleCombatManeuver", ["discriminator"] = evt.Type.ToString(), ["target"] = evt.Target?.Blueprint?.AssetGuid, ["targetNumber"] = evt.TargetCMD, ["initiatorCmb"] = evt.InitiatorCMB });
            }
            public void OnEventAboutToTrigger(RuleSkillCheck evt) { }
            public void OnEventDidTrigger(RuleSkillCheck evt)
            {
                if (evt == null || !ReferenceEquals(evt.Initiator, Escapee)) return;
                Rules.Add(new JObject { ["rule"] = "RuleSkillCheck", ["discriminator"] = evt.StatType.ToString(), ["targetNumber"] = evt.DC });
            }
        }

        /// <summary>The escape implementation the webbed state actually loads:
        /// "native" for the native action configured as installed (UseCMB on,
        /// UseCMD off), "cotw" for Call of the Wild's replacement, otherwise an
        /// explicit unsupported value.</summary>
        private static string SpiderLoadedEscapeImplementation(BlueprintBuff webbed)
        {
            GameAction[] escapes = (webbed.ComponentsArray ?? new BlueprintComponent[0]).OfType<AddFactContextActions>()
                .SelectMany(value => value.NewRound == null || value.NewRound.Actions == null ? new GameAction[0] : value.NewRound.Actions)
                .Where(value => value != null && value.GetType().Name.StartsWith("ContextActionBreakFree", StringComparison.Ordinal)).ToArray();
            if (escapes.Length != 1) return "unsupported:escape-actions=" + escapes.Length;
            var native = escapes[0] as ContextActionBreakFree;
            if (native != null && escapes[0].GetType() == typeof(ContextActionBreakFree))
                return native.UseCMB && !native.UseCMD ? "native" : "unsupported:native-flags-cmb=" + native.UseCMB + "-cmd=" + native.UseCMD;
            return escapes[0].GetType().FullName == "CallOfTheWild.CombatManeuverMechanics.ContextActionBreakFreeFromSpellGrapple" ? "cotw" : "unsupported:" + escapes[0].GetType().FullName;
        }

        /// <summary>Null when the one rule triggered is the one this
        /// implementation's branch must trigger; otherwise why not.</summary>
        internal static string SpiderEscapeRuleProblem(string implementation, string branch, JObject attempt)
        {
            if (implementation == null || implementation.StartsWith("unsupported", StringComparison.Ordinal)) return "unsupported-implementation:" + implementation;
            JArray rules = attempt["rules"] as JArray;
            if (rules == null || rules.Count != 1) return "rules=" + (rules == null ? 0 : rules.Count);
            string rule = (string)rules[0]["rule"], discriminator = (string)rules[0]["discriminator"];
            bool skillStat = discriminator == "SkillMobility" || discriminator == "SkillAthletics";
            bool ok = branch == "skill" ? rule == "RuleSkillCheck" && skillStat :
                implementation == "native" ? rule == "RuleCombatManeuver" && discriminator == "Grapple" :
                rule == "RuleSkillCheck" && discriminator == "AdditionalCMB";
            return ok ? null : "unexpected:" + rule + ":" + discriminator;
        }

        /// <summary>
        /// One escape attempt from a webbed state: an existing one (a real web
        /// hit), or one applied in the spider's own web execution context. Path
        /// "native" runs the installed native action configuration in the
        /// state's own context; path "loaded" runs the state's loaded per-round
        /// actions. The escapee's skills or BAB select the branch. Returns the
        /// escape rules actually triggered and their final target numbers.
        /// </summary>
        private static JObject SpiderEscapeAttempt(UnitEntityData spider, UnitEntityData escapee, string branch, string path, Buff existing = null)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            BlueprintAbility web = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, SpiderSummonerCatalog.GiantSpiderWebAbility, "giant spider web");
            BlueprintBuff webbed = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, ExpandedSummoningSpecialBuilder.NativeWebGrappledGuid, "native webbed state");
            ModifiableValue bab = escapee.Descriptor.Stats.BaseAttackBonus, mobility = escapee.Descriptor.Stats.GetStat(StatType.SkillMobility), athletics = escapee.Descriptor.Stats.GetStat(StatType.SkillAthletics);
            int babBefore = bab.BaseValue, mobilityBefore = mobility.BaseValue, athleticsBefore = athletics.BaseValue;
            bool skill = branch == "skill";
            string implementation = path == "native" ? "native" : SpiderLoadedEscapeImplementation(webbed);
            var observer = new SpiderEscapeRuleObserver { Escapee = escapee };
            Buff state = existing;
            try
            {
                bab.BaseValue = skill ? -100 : 200;
                SetExactProperty(mobility, "BaseValue", skill ? 200 : -100);
                SetExactProperty(athletics, "BaseValue", skill ? 200 : -100);
                if (state == null)
                {
                    foreach (Buff old in escapee.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => ReferenceEquals(value.Blueprint, webbed)).ToArray())
                        escapee.Descriptor.Buffs.RemoveFact(old);
                    var cast = new MechanicsContext(spider, spider.Descriptor, web);
                    cast.Recalculate();
                    state = escapee.Descriptor.Buffs.AddBuff(webbed, cast, TimeSpan.FromSeconds(60));
                }
                if (state == null) return new JObject { ["branch"] = branch, ["path"] = path, ["implementation"] = implementation, ["error"] = "webbed state refused" };
                ActionList actions;
                if (path == "native")
                {
                    var native = ScriptableObject.CreateInstance<ContextActionBreakFree>();
                    native.UseCMB = true; native.UseCMD = false; native.Success = new ActionList(); native.Failure = new ActionList();
                    actions = new ActionList { Actions = new GameAction[] { native } };
                }
                else actions = webbed.ComponentsArray.OfType<AddFactContextActions>().Single().NewRound;
                EventBus.Subscribe(observer);
                state.RunActionInContext(actions, new TargetWrapper(escapee));
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                if (existing == null && state != null && escapee.Descriptor.Buffs.RawFacts.Contains(state)) escapee.Descriptor.Buffs.RemoveFact(state);
                bab.BaseValue = babBefore;
                SetExactProperty(mobility, "BaseValue", mobilityBefore);
                SetExactProperty(athletics, "BaseValue", athleticsBefore);
            }
            JObject decisive = observer.Rules.Count == 1 ? observer.Rules[0] : null;
            var result = new JObject { ["branch"] = branch, ["path"] = path, ["implementation"] = implementation, ["rules"] = new JArray(observer.Rules),
                ["rule"] = decisive?["rule"], ["discriminator"] = decisive?["discriminator"], ["targetNumber"] = decisive?["targetNumber"] };
            string problem = SpiderEscapeRuleProblem(implementation, branch, result);
            result["ruleProblem"] = problem == null ? JValue.CreateNull() : (JToken)problem;
            return result;
        }

        /// <summary>An ordinary grapple maneuver against the spider, with no
        /// web reason, and its final CMD target number.</summary>
        private static int SpiderOrdinaryGrappleTargetNumber(UnitEntityData spider, UnitEntityData initiator)
        {
            var observer = new SpiderEscapeRuleObserver { Escapee = initiator };
            EventBus.Subscribe(observer);
            try { Rulebook.Trigger(new RuleCombatManeuver(initiator, spider, CombatManeuver.Grapple) { IgnoreConcealment = true }); }
            finally { EventBus.Unsubscribe(observer); }
            JObject maneuver = observer.Rules.SingleOrDefault(value => (string)value["rule"] == "RuleCombatManeuver");
            return maneuver == null ? -1 : (int)maneuver["targetNumber"];
        }

        private static readonly string[] SpiderEscapeCases = { "native:skill", "native:cmb", "loaded:skill", "loaded:cmb" };

        /// <summary>
        /// Each escape case with the feat's marker and without it on the same
        /// summoned spider, plus a standalone CMD calculation as the ordinary
        /// grapple control. Exactly one escape rule per attempt; with the marker
        /// its final target number is exactly 2 higher, whichever rule it is.
        /// </summary>
        private Dictionary<string, int> SpiderWebEscapeEvidence(UnitEntityData spider, UnitEntityData escapee, Dictionary<string, int> noFeatBaseline)
        {
            BlueprintBuff marker = SpiderSummonerRuntime.Marker;
            Buff held = marker == null ? null : spider.Descriptor.Buffs.GetBuff(marker);
            var rows = new JArray(); var baseline = new Dictionary<string, int>(StringComparer.Ordinal); bool ok = true;
            if (noFeatBaseline == null)
            {
                if (held == null) ok = false;
                int controlWith = SpiderOrdinaryGrappleTargetNumber(spider, escapee);
                var with = SpiderEscapeCases.ToDictionary(value => value, value => SpiderEscapeAttempt(spider, escapee, value.Split(':')[1], value.Split(':')[0]));
                if (held != null) spider.Descriptor.Buffs.RemoveFact(held);
                int controlWithout = SpiderOrdinaryGrappleTargetNumber(spider, escapee);
                var without = SpiderEscapeCases.ToDictionary(value => value, value => SpiderEscapeAttempt(spider, escapee, value.Split(':')[1], value.Split(':')[0]));
                if (marker != null) spider.Descriptor.AddBuff(marker, spider, null);
                foreach (string key in SpiderEscapeCases)
                {
                    JObject a = with[key], b = without[key];
                    bool one = a["targetNumber"].Type != JTokenType.Null && b["targetNumber"].Type != JTokenType.Null && (string)a["rule"] == (string)b["rule"];
                    bool expectedRule = a["ruleProblem"].Type == JTokenType.Null && b["ruleProblem"].Type == JTokenType.Null;
                    bool plusTwo = one && (int)a["targetNumber"] == (int)b["targetNumber"] + SpiderSummonerCatalog.DcBonus;
                    if (one) baseline[key] = (int)b["targetNumber"];
                    ok &= one && expectedRule && plusTwo;
                    rows.Add(new JObject { ["case"] = key, ["withMarker"] = a, ["withoutMarker"] = b, ["exactlyPlusTwo"] = plusTwo, ["expectedRule"] = expectedRule });
                }
                ok &= controlWith > 0 && controlWith == controlWithout;
                Sprint25Check("spider-web-escape-with-feat", ok, new JObject { ["cases"] = rows, ["ordinaryGrappleTargetNumberWithMarker"] = controlWith, ["ordinaryGrappleTargetNumberWithoutMarker"] = controlWithout,
                    ["state"] = "Webbed state applied in the spider's own web execution context (the projectile hit is observed in the correction rules scenario, without the feat).",
                    ["scope"] = "native = the installed native action configuration run in the state's own context; loaded = the state's loaded per-round actions in this profile." },
                    "Every web escape path, native skill, native CMB and the loaded action's two branches, triggers exactly the rule its implementation and branch require, whose final target number is exactly 2 higher for the feat spider; an ordinary grapple maneuver's target number is unchanged.");
                return baseline;
            }
            foreach (string key in SpiderEscapeCases)
            {
                JObject a = SpiderEscapeAttempt(spider, escapee, key.Split(':')[1], key.Split(':')[0]);
                int expected;
                bool same = a["targetNumber"] != null && a["targetNumber"].Type != JTokenType.Null && a["ruleProblem"].Type == JTokenType.Null &&
                    noFeatBaseline.TryGetValue(key, out expected) && (int)a["targetNumber"] == expected;
                ok &= held == null && same;
                rows.Add(new JObject { ["case"] = key, ["attempt"] = a, ["matchesFeatSpiderWithoutMarker"] = same });
            }
            Sprint25Check("spider-web-escape-without-feat", ok, new JObject { ["cases"] = rows, ["marker"] = held != null },
                "A Giant Spider summoned without the feat has no marker, and each escape path's target number equals the feat spider's without its marker.");
            return null;
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

        /// <summary>Opens the real popup while the feat is held and keeps the
        /// exact choice object it offers, as a player's selection holds it.</summary>
        private IEnumerable<int> SpiderHoldSelection(Spellbook book, int tier, BlueprintAbility parent, AbilityData prepared, BlueprintAbility row,
            Cleanup2NativeDefaultScope mode, Stopwatch watch, ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, string caseKey, Action<AbilityData> keep)
        {
            AlphaMenu menu = null; ActionBarGroupSlot source = null;
            foreach (int frame in ClericOpenMenu("spider-held-" + caseKey, spellGroup, levels, tier, book, parent, prepared, mode, watch, null, (value, slot) => { menu = value; source = slot; }, false)) yield return frame;
            int index = menu == null || row == null ? -1 : Array.FindIndex(menu.Data, value => ReferenceEquals(value?.Blueprint, row));
            AbilityData choice = index < 0 ? null : menu.Data[index];
            bool castable = choice != null && choice.IsVisible() && choice.IsAvailableForCast && ReferenceEquals(choice.ConvertedFrom, prepared);
            Sprint25Check("spider-held-" + caseKey + "-obtained", castable, new JObject { ["row"] = row?.AssetGuid, ["offered"] = index >= 0, ["visible"] = choice?.IsVisible(), ["available"] = choice?.IsAvailableForCast },
                "With the feat held, the real popup offers the spider row as a castable choice, which is kept.");
            if (source != null) Cleanup2MenuAdapterHide(source, prepared);
            keep(castable ? choice : null);
        }

        private IEnumerable<int> SpiderHeldAttempt(ExpandedSummoningCorrectionFixture fixture, AbilityData held, AbilityData prepared, Cleanup2NativeDefaultScope mode, Stopwatch watch, string caseKey)
        {
            if (held == null)
            {
                Sprint25Check("spider-held-attempt-" + caseKey, false, new JObject { ["reason"] = "no choice was obtained while the feat was held" }, "A selection kept across feature loss is refused.");
                return Enumerable.Empty<int>();
            }
            return ClericRefusedAttempt(fixture, held, prepared, mode, watch, "spider-held-attempt-" + caseKey, "spider-held-" + caseKey, false,
                "The same AbilityData the real popup offered while the feat was held, kept across feature removal; native UnitUseAbility; no manual tick or forced act",
                "A selection obtained while the feat was held and executed after its removal is refused before any slot or resource is spent, and nothing is summoned.");
        }

        /// <summary>Casts exactly a kept choice through the native command
        /// path, requiring success, one spawn of the named unit and exactly one
        /// slot of its source spent.</summary>
        private IEnumerable<int> SpiderHeldCast(ExpandedSummoningCorrectionFixture fixture, Spellbook book, AbilityData held, AbilityData prepared, Cleanup2NativeDefaultScope mode, Stopwatch phase,
            string caseKey, string expectedUnit, Action<UnitEntityData[]> inspect)
        {
            if (held == null)
            {
                Sprint25Check("spider-held-cast-" + caseKey, false, new JObject { ["reason"] = "no choice was kept" }, "A kept choice casts its spider.");
                yield break;
            }
            var debit = new Cleanup2SourceDebit(fixture.Caster);
            var hooks = HarmonyInstance.Create("kmg.spider.heldcast." + caseKey + _request.RunId); UnitUseAbility command = null; string result;
            var watch = Stopwatch.StartNew();
            try
            {
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"), new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", BindingFlags.Static | BindingFlags.NonPublic)), null, null);
                ExpandedSummoningRuleCapture.Clear(); _expandedSummoningPlayerPathSpawnActionCount = 0; _expandedSummoningPlayerPathCaptureActive = true;
                command = new UnitUseAbility(held, new TargetWrapper(fixture.Caster.Position + new Vector3(2, 0, 2)));
                fixture.Caster.Commands.Run(command);
                while (!command.IsFinished && watch.Elapsed.TotalSeconds < 25 && phase.Elapsed.TotalSeconds < ClericPhaseWaitCeilingSeconds)
                {
                    if (!mode.Current || !Cleanup2NoNarrativeOverlay()) throw new InvalidOperationException("Kept-choice cast lost native mode ownership.");
                    Game.Instance.IsPaused = false; yield return 0;
                }
                result = command.IsFinished ? command.Result.ToString() : "unfinished";
            }
            finally
            {
                _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id);
                if (command != null && !command.IsFinished) command.Interrupt(true);
            }
            for (int frame = 0; frame < 12; frame++) yield return 0;
            UnitEntityData[] units = ExpandedSummoningRuleCapture.Where(value => value != null && ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster)).Distinct().ToArray();
            foreach (UnitEntityData unit in units) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
            bool ok = result == "Success" && _expandedSummoningPlayerPathSpawnActionCount == 1 && units.Length == 1 && units[0].Blueprint.AssetGuid == expectedUnit && debit.ExactlyOne(prepared.ParamSpellSlot);
            Sprint25Check("spider-held-cast-" + caseKey, ok, new JObject { ["child"] = held.Blueprint.AssetGuid, ["commandResult"] = result, ["spawnActions"] = _expandedSummoningPlayerPathSpawnActionCount,
                ["units"] = new JArray(units.Select(value => value.Blueprint.AssetGuid)), ["debit"] = debit.Describe(prepared.ParamSpellSlot), ["wallSeconds"] = watch.Elapsed.TotalSeconds,
                ["route"] = "The AbilityData the real popup offered, kept and cast later through native UnitUseAbility; no manual tick or forced act" },
                "A kept choice cast later summons its spider and spends exactly its source slot.");
            if (inspect != null && units.Length != 0) inspect(units);
            if (units.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, units);
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
            // The two species whose native rows carried their own template: fiendish under an evil caster.
            foreach (var fiendish in new[] { new { Key = "giant-crab-spider", Tier = 1 }, new { Key = "giant-black-widow", Tier = 4 } })
            {
                var scoped = fiendish;
                foreach (int frame in R6Cell("spider-sm-" + scoped.Key + "-evil", () => SpiderMenuCast(fixture, book, scoped.Tier, smParent(scoped.Tier), preparedParent(scoped.Tier), row(scoped.Key, scoped.Tier), mode, watch, spellGroup, levels,
                    "sm-" + scoped.Key + "-evil", SpiderSummonerCatalog.Find(scoped.Key).UnitGuid, units => check("sm-" + scoped.Key + "-evil", units, false, scoped.Key, true), null))) yield return frame;
            }
            // Neutral caster: the existing toggle is read at execution.
            fixture.Caster.Descriptor.Alignment.Set(Alignment.TrueNeutral);
            setDeity(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, PharasmaFeatureGuid, "neutral deity"));
            BlueprintBuff fiendishMode = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol["KMG.Summoning.AlignmentMode.FiendishMarker"];
            string tarantulaUnit = SpiderSummonerCatalog.Find("giant-tarantula").UnitGuid;
            foreach (int frame in R6Cell("spider-sm-neutral-off", () => SpiderMenuCast(fixture, book, 7, smParent(7), preparedParent(7), row("giant-tarantula", 7), mode, watch, spellGroup, levels,
                "sm-neutral-celestial", tarantulaUnit, units => check("sm-neutral-celestial", units, true, "giant-tarantula", true), null))) yield return frame;
            // A choice kept while the toggle is off, then cast after it is turned on.
            AbilityData heldNeutral = null;
            foreach (int frame in R6Cell("spider-hold-neutral", () => SpiderHoldSelection(book, 7, smParent(7), preparedParent(7), row("giant-tarantula", 7), mode, watch, spellGroup, levels,
                "neutral-tarantula", value => heldNeutral = value))) yield return frame;
            Buff modeBuff = fixture.Caster.Descriptor.AddBuff(fiendishMode, fixture.Caster, null);
            foreach (int frame in R6Cell("spider-sm-neutral-on", () => SpiderMenuCast(fixture, book, 7, smParent(7), preparedParent(7), row("giant-tarantula", 7), mode, watch, spellGroup, levels,
                "sm-neutral-fiendish", tarantulaUnit, units => check("sm-neutral-fiendish", units, false, "giant-tarantula", true), null))) yield return frame;
            foreach (int frame in R6Cell("spider-held-neutral-toggled", () => SpiderHeldCast(fixture, book, heldNeutral, preparedParent(7), mode, watch, "neutral-toggled-after-selection", tarantulaUnit,
                units => check("sm-neutral-toggled-after-selection", units, false, "giant-tarantula", true)))) yield return frame;
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
            Dictionary<string, int> escapeBaseline = null;
            Action<string, UnitEntityData[], bool> ordinaryCheck = (key, units, expect) =>
            {
                JObject[] dcs = units.Select(value => SpiderDcEvidence(value, ordinaryPoison, expect)).ToArray();
                Sprint25Check("spider-ordinary-" + key, units.Length == 1 && dcs.All(value => (bool)value["ok"]), new JObject { ["dcs"] = new JArray(dcs) }, "The ordinary Giant Spider's poison DC reflects only the caster's feat.");
                if (expect && units.Length == 1) { SpiderWebConsumerEvidence(units[0]); escapeBaseline = SpiderWebEscapeEvidence(units[0], fixture.Caster, null); }
                else if (!expect && units.Length == 1) SpiderWebEscapeEvidence(units[0], fixture.Caster, escapeBaseline ?? new Dictionary<string, int>());
            };
            foreach (int frame in R6Cell("spider-ordinary-with-feat", () => SpiderMenuCast(fixture, book, 2, smParent(2), preparedParent(2), ordinaryRow, mode, watch, spellGroup, levels,
                "ordinary-with-feat", SpiderSummonerCatalog.OrdinaryGiantSpiderUnit, units => ordinaryCheck("with-feat", units, true), null))) yield return frame;
            // Real selections, obtained while the feat is held and kept across its removal.
            AbilityData heldOriginal = null, heldGenerated = null;
            foreach (int frame in R6Cell("spider-hold-original", () => SpiderHoldSelection(book, 1, smParent(1), preparedParent(1), row("giant-crab-spider", 1), mode, watch, spellGroup, levels,
                "original-crab", value => heldOriginal = value))) yield return frame;
            foreach (int frame in R6Cell("spider-hold-generated", () => SpiderHoldSelection(book, 4, smParent(4), preparedParent(4), crabD4Row, mode, watch, spellGroup, levels,
                "generated-crab-d4", value => heldGenerated = value))) yield return frame;
            Kingmaker.Blueprints.Facts.Fact held = fixture.Caster.Descriptor.Progression.Features.Enumerable.FirstOrDefault(value => ReferenceEquals(value?.Blueprint, spiderFeature));
            if (held != null) fixture.Caster.Descriptor.RemoveFact(held);
            foreach (int frame in R6Cell("spider-ordinary-without-feat", () => SpiderMenuCast(fixture, book, 2, smParent(2), preparedParent(2), ordinaryRow, mode, watch, spellGroup, levels,
                "ordinary-without-feat", SpiderSummonerCatalog.OrdinaryGiantSpiderUnit, units => ordinaryCheck("without-feat", units, false), null))) yield return frame;
            foreach (int frame in R6Cell("spider-held-original", () => SpiderHeldAttempt(fixture, heldOriginal, preparedParent(1), mode, watch, "original-crab"))) yield return frame;
            foreach (int frame in R6Cell("spider-held-generated", () => SpiderHeldAttempt(fixture, heldGenerated, preparedParent(4), mode, watch, "generated-crab-d4"))) yield return frame;
            // A fresh choice built without the feat is refused too.
            foreach (int frame in R6Cell("spider-feature-absent-fresh", () => ClericForbiddenAttempt(fixture, row("giant-crab-spider", 1), book, preparedParent(1), mode, watch, "spider-feature-absent-fresh", false))) yield return frame;
            fixture.Caster.Descriptor.AddFact(spiderFeature);
            Sprint25Check("spider-surface-limits", true, new JObject {
                ["observed"] = "Summon Monster casts of all four original rows, repaired Ogre under evil and non-evil casters, neutral toggle, one 1d4+1 and one 1d3 quantity, ordinary Giant Spider with and without the feat, an original and a generated selection obtained with the feat and executed after its removal, a fresh no-feat choice, the loaded web consumer and its context DC, every web escape path's rule and target number with a real ordinary grapple control, and the 46-placement census.",
                ["notObserved"] = "Summon Nature's Ally casts and Druid conversion casts; Herald quantity-tier casts; natural Drow feat acquisition; multiclass and ordinary spontaneous spellbooks; Augment and Superior Summoning interaction; poison save resolution; a feat spider's real web hit followed by escape from that state; reload and module-off or RU-absent profiles; NPC selection." },
                "Observed and unobserved surfaces are reported separately.");
        }
    }
}
