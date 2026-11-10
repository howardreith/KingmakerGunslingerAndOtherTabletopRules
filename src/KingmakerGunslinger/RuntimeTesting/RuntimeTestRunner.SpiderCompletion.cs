using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Spider Summoner completion phases, under the sprint32 manual clock with
    /// an owned hostile: a real web hit followed by escape from that state
    /// (spider-web), summoning feats with actual poison saves and the NPC
    /// selection control (spider-mechanics), and the Expanded Summoning OFF
    /// control (spider-module-off). With Races Unleashed absent, the web phase
    /// is the RU-absent control.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class SpiderSaveObserver : IGlobalRulebookHandler<RuleSavingThrow>, IGlobalRulebookHandler<RuleApplyBuff>
        {
            internal UnitEntityData Target;
            internal readonly List<RuleSavingThrow> Saves = new List<RuleSavingThrow>();
            internal readonly List<JObject> Applications = new List<JObject>();
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt) { if (evt != null && ReferenceEquals(evt.Initiator, Target)) Saves.Add(evt); }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (evt == null || !ReferenceEquals(evt.Initiator, Target)) return;
                Applications.Add(new JObject { ["buff"] = evt.Blueprint?.AssetGuid, ["name"] = evt.Blueprint?.name, ["applied"] = evt.AppliedBuff != null,
                    ["immunity"] = evt.Immunity, ["source"] = evt.Context?.MaybeCaster?.UniqueId, ["seconds"] = evt.Duration?.TotalSeconds });
            }
        }

        /// <summary>Forces the first roll of one die type by one initiator, so a
        /// count is compared with its known die value instead of a range.</summary>
        private sealed class SpiderDieOverride : IGlobalRulebookHandler<RuleRollDice>
        {
            internal UnitEntityData Initiator; internal DiceType Die; internal int Value;
            internal readonly List<string> Rolls = new List<string>();
            public void OnEventAboutToTrigger(RuleRollDice evt)
            {
                if (evt == null || !ReferenceEquals(evt.Initiator, Initiator) || evt.DiceFormula.Dice != Die) return;
                if (Rolls.Count == 0) evt.Override(Value);
                Rolls.Add(evt.DiceFormula.Rolls + "x" + evt.DiceFormula.Dice);
            }
            public void OnEventDidTrigger(RuleRollDice evt) { }
        }

        private static JObject SpiderSaveRecord(RuleSavingThrow save)
        {
            return new JObject { ["dc"] = save.DifficultyClass, ["type"] = save.Type.ToString(), ["passed"] = save.IsPassed,
                ["reasonFact"] = save.Reason?.Fact?.Blueprint?.AssetGuid, ["reasonFactName"] = save.Reason?.Fact?.Blueprint?.name,
                ["reasonContext"] = save.Reason?.Context?.AssociatedBlueprint?.AssetGuid };
        }

        /// <summary>Casts exactly this published row through the real spell
        /// execution path (granted fact, native ability execution), so its own
        /// run and after-spawn actions run with the row as the source spell.</summary>
        private static UnitEntityData[] SpiderCastRow(ExpandedSummoningCorrectionFixture fixture, UnitEntityData caster, BlueprintAbility row, int tier)
        {
            ExpandedSummoningRuleCapture.Clear();
            bool granted = !caster.Descriptor.HasFact(row);
            if (granted) caster.Descriptor.AddFact(row);
            try { ExecuteExpandedSummoningRuntimeAbility(caster, row, tier); }
            finally { if (granted && caster.Descriptor.HasFact(row)) caster.Descriptor.RemoveFact(row); }
            UnitEntityData[] units = ExpandedSummoningRuleCapture.Where(value => value != null).Distinct().ToArray();
            Game.Instance.EntityCreator.Tick();
            foreach (UnitEntityData unit in units)
            {
                if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                Sprint28SetBrainActivePreservingActions(unit, false);
                unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
            }
            return units;
        }

        private static int SpiderMarkerCount(UnitEntityData unit)
        {
            BlueprintBuff marker = SpiderSummonerRuntime.Marker;
            return marker == null ? 0 : unit.Descriptor.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, marker));
        }

        private static IEnumerable<BlueprintAbility> SpiderPublishedParents(LibraryScriptableObject library)
        {
            return ExpandedSummoningPublisher.CanonicalParentGuids.Select(guid =>
                BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, guid, "summon parent"));
        }

        private static bool SpiderRowIdentity(BlueprintAbility value)
        {
            return value != null && (value.AssetGuid.StartsWith(SpiderSummonerCatalog.IdentityPrefix, StringComparison.Ordinal) ||
                SpiderSummonerIntegration.PlacementOf(value) != null || (value.name ?? "").StartsWith("KMG_Summoning_Spider_", StringComparison.Ordinal));
        }

        private IEnumerable<int> ReviewSpiderSummonerCompletion(ExpandedSummoningCorrectionFixture fixture, string phase)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null)
                throw new InvalidOperationException("Exact fresh Spider Summoner completion fixture required.");
            bool moduleOn = _context.FeatureModules.Active.ExpandedSummoning;
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            BlueprintScriptableObject found;
            BlueprintFeature feature = library.BlueprintsByAssetId.TryGetValue(SummonForeignRowClassifier.SpiderSummonerFeatureGuid, out found) ? found as BlueprintFeature : null;
            BlueprintBuff webbed = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, ExpandedSummoningSpecialBuilder.NativeWebGrappledGuid, "native webbed state");
            Sprint25Check("spider-completion-profile", true, new JObject { ["phase"] = phase, ["moduleActive"] = moduleOn,
                ["racesUnleashedFeature"] = feature != null, ["integration"] = SpiderSummonerIntegration.Outcome,
                ["loadedEscape"] = SpiderLoadedEscapeImplementation(webbed),
                ["callOfTheWildLoaded"] = AppDomain.CurrentDomain.GetAssemblies().Any(value => value.GetName().Name == "CallOfTheWild") },
                "The profile this phase ran in, read from the loaded game.");
            if (phase == ExpandedSummoningNativeProfilePolicy.Sprint39SpiderModuleOffPhase)
            {
                if (moduleOn) throw new InvalidOperationException("The module-off control requires Expanded Summoning OFF.");
                SpiderModuleOffControl(fixture, feature);
                yield break;
            }
            if (!moduleOn) throw new InvalidOperationException("Spider Summoner completion requires Expanded Summoning active.");
            if (feature == null)
            {
                if (phase != ExpandedSummoningNativeProfilePolicy.Sprint39SpiderWebPhase) throw new InvalidOperationException("Races Unleashed is absent; only the web phase is the RU-absent control.");
                SpiderRacesUnleashedAbsentControl(fixture, library);
                yield break;
            }
            IEnumerable<int> cells = phase == ExpandedSummoningNativeProfilePolicy.Sprint39SpiderWebPhase ? SpiderRealWebCells(fixture, feature, webbed) :
                phase == ExpandedSummoningNativeProfilePolicy.Sprint39SpiderRoutesPhase ? ReviewSpiderRoutes(fixture, feature) : SpiderMechanicsCells(fixture, feature);
            foreach (int frame in cells) yield return frame;
        }

        // --- RU-absent and module-off controls ---------------------------------------------------------------

        private void SpiderRacesUnleashedAbsentControl(ExpandedSummoningCorrectionFixture fixture, LibraryScriptableObject library)
        {
            string outcome = SpiderSummonerIntegration.Outcome ?? "null";
            int listed = SpiderPublishedParents(library).Sum(parent => (parent.Variants ?? new BlueprintAbility[0]).Count(SpiderRowIdentity));
            int foreign = library.BlueprintsByAssetId.Keys.Count(key => key.StartsWith(SpiderSummonerCatalog.IdentityPrefix, StringComparison.Ordinal));
            BlueprintAbility ordinary = ExpandedSummoningRoot(fixture.Blueprints, ExpandedSummoningVariant(SummonFamily.Monster, "giant-spider", 2, SummonMultiplicity.One));
            UnitEntityData[] units = SpiderCastRow(fixture, fixture.Caster, ordinary, 2);
            bool cast = units.Length == 1 && units[0].Blueprint.AssetGuid == SpiderSummonerCatalog.OrdinaryGiantSpiderUnit && SpiderMarkerCount(units[0]) == 0;
            if (units.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, units);
            bool ok = outcome.StartsWith("absent:", StringComparison.Ordinal) && listed == 0 && foreign == 0 && cast && SpiderSummonerRuntime.Marker == null;
            Sprint25Check("spider-races-unleashed-absent", ok, new JObject { ["integration"] = outcome, ["spiderRowsListed"] = listed,
                ["racesUnleashedIdentities"] = foreign, ["ordinaryGiantSpiderCast"] = cast, ["markerActive"] = SpiderSummonerRuntime.Marker != null },
                "Without Races Unleashed the integration is a no-op without errors: no spider rows, no foreign identities, and the ordinary Giant Spider casts with no benefit.");
        }

        private void SpiderModuleOffControl(ExpandedSummoningCorrectionFixture fixture, BlueprintFeature feature)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            string outcome = SpiderSummonerIntegration.Outcome ?? "null";
            if (feature == null) { Sprint25Check("spider-module-off", false, new JObject { ["integration"] = outcome }, "The module-off control needs Races Unleashed installed."); return; }
            var rows = new JArray(); bool originalsIntact = true;
            foreach (SummonFamily family in SpiderSummonerCatalog.Families)
                foreach (SpiderSpeciesSpec species in SpiderSummonerCatalog.All)
                {
                    BlueprintScriptableObject found;
                    BlueprintAbility row = library.BlueprintsByAssetId.TryGetValue(SpiderSummonerCatalog.RowIdentity(family, species), out found) ? found as BlueprintAbility : null;
                    SummonForeignRowReading reading = row == null ? null : SummonForeignRowRuntime.Read(row);
                    string attachment = reading == null ? "absent" : reading.PublishedTier == species.BaseTier ? "parent-variants" : "none";
                    if (reading != null && reading.PublishedTier == 0 && species.BaseTier == 1)
                    {
                        // Races Unleashed's own route when the native tier I parent has no variants (CreateSpiderSummon).
                        BlueprintAbility nativeParent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                            ExpandedSummoningPublisher.CanonicalParentGuids[family == SummonFamily.Monster ? 0 : 9], "tier I parent");
                        bool attached = (nativeParent.ComponentsArray ?? new BlueprintComponent[0]).Any(component => component != null &&
                            component.GetType().Name == "CustomSpontaneousSpell" && ReferenceEquals(component.GetType().GetField("Spell")?.GetValue(component), row));
                        if (attached) { reading.PublishedTier = 1; attachment = "races-unleashed-custom-spontaneous-spell"; }
                    }
                    string input = reading == null ? "absent" : SummonForeignRowClassifier.LegacyInputProblem(reading);
                    originalsIntact &= input == null;
                    rows.Add(new JObject { ["row"] = row?.AssetGuid, ["attachment"] = attachment, ["legacyInputProblem"] = SpiderEscapeObservationVerdict.Optional(input) });
                }
            int generatedListed = SpiderPublishedParents(library).Sum(parent => (parent.Variants ?? new BlueprintAbility[0]).Count(value =>
                value != null && (SpiderSummonerIntegration.PlacementOf(value) != null || (value.name ?? "").StartsWith("KMG_", StringComparison.Ordinal))));
            // The original Races Unleashed behavior with the feat: its own SM IV row summons its own unit with its own native template step and no KMG action.
            Fact featFact = fixture.Caster.Descriptor.AddFact(feature);
            UnitEntityData[] units;
            BlueprintAbility widow = (BlueprintAbility)library.BlueprintsByAssetId[SpiderSummonerCatalog.RowIdentity(SummonFamily.Monster, SpiderSummonerCatalog.Find("giant-black-widow"))];
            try { units = SpiderCastRow(fixture, fixture.Caster, widow, 4); }
            finally { if (featFact != null) fixture.Caster.Descriptor.RemoveFact(featFact); }
            bool nativeCast = units.Length == 1 && units[0].Blueprint.AssetGuid == SpiderSummonerCatalog.Find("giant-black-widow").UnitGuid && SpiderMarkerCount(units[0]) == 0 &&
                units[0].Descriptor.Buffs.Enumerable.Any(value => ExpandedSummoningAbilityBuilder.NativeTemplateIds.Contains(value.Blueprint.AssetGuid));
            JArray unitBuffs = new JArray(units.SelectMany(value => value.Descriptor.Buffs.Enumerable.Select(buff => buff.Blueprint.name)));
            if (units.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, units);
            bool ok = outcome == "registered-module-off" && originalsIntact && generatedListed == 0 && nativeCast && SpiderSummonerRuntime.Marker == null;
            Sprint25Check("spider-module-off", ok, new JObject { ["integration"] = outcome, ["originalRows"] = rows, ["kmgRowsListed"] = generatedListed,
                ["originalWidowCast"] = nativeCast, ["widowBuffs"] = unitBuffs },
                "With Expanded Summoning OFF and Races Unleashed installed, the eight original rows keep their Races Unleashed shape and behavior, the new identities stay dormant and unlisted, and no benefit is active.");
        }

        // --- Real web hit followed by escape -----------------------------------------------------------------

        private IEnumerable<int> SpiderRealWebCells(ExpandedSummoningCorrectionFixture fixture, BlueprintFeature feature, BlueprintBuff webbed)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            BlueprintAbility web = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, SpiderSummonerCatalog.GiantSpiderWebAbility, "giant spider web");
            BlueprintAbility row = ExpandedSummoningRoot(fixture.Blueprints, ExpandedSummoningVariant(SummonFamily.Monster, "giant-spider", 2, SummonMultiplicity.One));
            UnitEntityData caster = fixture.Caster, hostile = fixture.Hostile;
            Fact featFact = caster.Descriptor.AddFact(feature);
            UnitEntityData[] marked = SpiderCastRow(fixture, caster, row, 2);
            if (featFact != null) caster.Descriptor.RemoveFact(featFact);
            UnitEntityData[] unmarked = SpiderCastRow(fixture, caster, row, 2);
            bool summons = marked.Length == 1 && unmarked.Length == 1 && SpiderMarkerCount(marked[0]) == 1 && SpiderMarkerCount(unmarked[0]) == 0 && row.Type == AbilityType.Spell;
            Sprint25Check("spider-web-summons", summons, new JObject { ["row"] = row.AssetGuid, ["rowType"] = row.Type.ToString(),
                ["markedCount"] = marked.Length, ["unmarkedCount"] = unmarked.Length, ["markedMarkers"] = marked.Length == 1 ? SpiderMarkerCount(marked[0]) : -1,
                ["unmarkedMarkers"] = unmarked.Length == 1 ? SpiderMarkerCount(unmarked[0]) : -1 },
                "The normal summon path's after-spawn action gives the Giant Spider summoned by a feat holder exactly one marker, and none without the feat.");
            if (!summons) yield break;
            Sprint28SetBrainActivePreservingActions(hostile, false);
            hostile.Commands.InterruptAll(command => true); hostile.Commands.RemoveFinishedAndUpdateQueue();
            ModifiableValue bab = hostile.Descriptor.Stats.BaseAttackBonus, mobility = hostile.Descriptor.Stats.GetStat(StatType.SkillMobility), athletics = hostile.Descriptor.Stats.GetStat(StatType.SkillAthletics);
            int babBefore = bab.BaseValue, mobilityBefore = mobility.BaseValue, athleticsBefore = athletics.BaseValue, acBefore = hostile.Descriptor.Stats.AC.BaseValue;
            var results = new Dictionary<string, JObject>(StringComparer.Ordinal);
            AbilityResourceLogic webCost = web.ComponentsArray.OfType<AbilityResourceLogic>().FirstOrDefault();
            int webUsesStart = webCost == null ? -1 : marked[0].Descriptor.Resources.GetResourceAmount(webCost.RequiredResource), webUsesRestored = 0;
            try
            {
                // Hopeless escape between the hit and the observed attempt, so no
                // clock tick can resolve the state before it is examined.
                bab.BaseValue = -100; SetExactProperty(mobility, "BaseValue", -100); SetExactProperty(athletics, "BaseValue", -100);
                for (int frame = 0; frame < 30; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                foreach (var pair in new[] { new { Key = "marked", Spider = marked[0] }, new { Key = "unmarked", Spider = unmarked[0] } })
                    foreach (string branch in new[] { "skill", "cmb" })
                    {
                        JObject result = null;
                        foreach (int frame in SpiderWebHitAndEscape(pair.Spider, hostile, web, webbed, branch, value => result = value)) yield return frame;
                        results[pair.Key + ":" + branch] = result;
                    }
                // The same marked spider's own fresh real hits, with its marker removed
                // after each hit: the baseline for a loaded escape that reads the CMD.
                // Its web uses are restored to their starting amount first.
                if (webCost != null)
                {
                    int now = marked[0].Descriptor.Resources.GetResourceAmount(webCost.RequiredResource);
                    if (now < webUsesStart) { webUsesRestored = webUsesStart - now; marked[0].Descriptor.Resources.Restore(webCost.RequiredResource, webUsesRestored); }
                }
                foreach (string branch in new[] { "skill", "cmb" })
                {
                    JObject result = null;
                    foreach (int frame in SpiderWebHitAndEscape(marked[0], hostile, web, webbed, branch, value => result = value, true)) yield return frame;
                    results["marked-without-marker:" + branch] = result;
                }
            }
            finally
            {
                bab.BaseValue = babBefore; SetExactProperty(mobility, "BaseValue", mobilityBefore); SetExactProperty(athletics, "BaseValue", athleticsBefore);
                hostile.Descriptor.Stats.AC.BaseValue = acBefore;
            }
            bool ok = true; var cases = new JArray();
            foreach (string branch in new[] { "skill", "cmb" })
            {
                JObject m = results["marked:" + branch], u = results["unmarked:" + branch], mb = results["marked-without-marker:" + branch];
                string hit = (string)m["hitProblem"] ?? (string)u["hitProblem"];
                // The loaded escape's comparison follows the rule it actually used: a
                // CMD read at the attempt against the same spider's own real-hit escape
                // with its marker removed; a DC fixed in the web context at the hit
                // against the matched unmarked summon's fresh cast.
                string loadedComparison = "none";
                string loaded = hit ?? SpiderEscapeObservationVerdict.LoadedProblem(m["loaded"] as JObject,
                    (string)mb?["hitProblem"] == null ? mb?["loaded"] as JObject : null, u["loaded"] as JObject, SpiderSummonerCatalog.DcBonus, out loadedComparison);
                if (loaded != null && loadedComparison == "same-spider" && (string)mb?["hitProblem"] != null) loaded = "same-spider-baseline-hit:" + (string)mb["hitProblem"];
                string loadedAcrossSummons = hit ?? SpiderEscapeObservationVerdict.PairProblem(m["loaded"] as JObject, u["loaded"] as JObject, SpiderSummonerCatalog.DcBonus);
                string loadedSameSpider = hit ?? ((string)mb?["hitProblem"] != null ? "baseline-hit:" + (string)mb["hitProblem"] :
                    SpiderEscapeObservationVerdict.PairProblem(m["loaded"] as JObject, mb?["loaded"] as JObject, SpiderSummonerCatalog.DcBonus));
                // A CMD-reading escape is decided within the same spider and state;
                // the pair across two summons is kept as a diagnostic.
                bool cmdBranch = (string)(m["nativeConfiguration"] as JObject)?["rule"] == "RuleCombatManeuver";
                string nativeConfig = hit ?? (cmdBranch ?
                    SpiderEscapeObservationVerdict.PairProblem(m["nativeConfiguration"] as JObject, m["nativeConfigurationWithoutMarker"] as JObject, SpiderSummonerCatalog.DcBonus) :
                    SpiderEscapeObservationVerdict.PairProblem(m["nativeConfiguration"] as JObject, u["nativeConfiguration"] as JObject, SpiderSummonerCatalog.DcBonus));
                string acrossSummons = hit ?? SpiderEscapeObservationVerdict.PairProblem(m["nativeConfiguration"] as JObject, u["nativeConfiguration"] as JObject, SpiderSummonerCatalog.DcBonus);
                ok &= loaded == null && nativeConfig == null;
                cases.Add(new JObject { ["branch"] = branch, ["loadedComparison"] = loadedComparison, ["loadedProblem"] = SpiderEscapeObservationVerdict.Optional(loaded),
                    ["loadedAcrossSummonsDiagnostic"] = SpiderEscapeObservationVerdict.Optional(loadedAcrossSummons),
                    ["loadedSameSpiderDiagnostic"] = SpiderEscapeObservationVerdict.Optional(loadedSameSpider),
                    ["nativeConfigurationProblem"] = SpiderEscapeObservationVerdict.Optional(nativeConfig),
                    ["nativeConfigurationAcrossSummons"] = SpiderEscapeObservationVerdict.Optional(acrossSummons), ["marked"] = m, ["unmarked"] = u, ["markedWithoutMarkerAfterHit"] = mb });
            }
            JObject defenseMarked, defenseUnmarked, defenseMarkedWithout = null;
            int controlMarked = SpiderOrdinaryGrappleTargetNumber(marked[0], hostile, out defenseMarked), controlUnmarked = SpiderOrdinaryGrappleTargetNumber(unmarked[0], hostile, out defenseUnmarked);
            int controlMarkedWithout = -1;
            Buff controlHeld = SpiderSummonerRuntime.Marker == null ? null : marked[0].Descriptor.Buffs.GetBuff(SpiderSummonerRuntime.Marker);
            if (controlHeld != null)
            {
                marked[0].Descriptor.Buffs.RemoveFact(controlHeld);
                controlMarkedWithout = SpiderOrdinaryGrappleTargetNumber(marked[0], hostile, out defenseMarkedWithout);
                marked[0].Descriptor.AddBuff(SpiderSummonerRuntime.Marker, marked[0], null);
            }
            ok &= controlMarked > 0 && controlMarked == controlMarkedWithout;
            Sprint25Check("spider-web-real-hit-escape", ok, new JObject { ["cases"] = cases, ["loadedEscape"] = SpiderLoadedEscapeImplementation(webbed),
                ["ordinaryGrappleTargetNumberMarked"] = controlMarked, ["ordinaryGrappleTargetNumberMarkedWithoutMarker"] = controlMarkedWithout,
                ["ordinaryGrappleTargetNumberUnmarkedSummon"] = controlUnmarked,
                ["ordinaryGrappleDefenses"] = new JObject { ["marked"] = defenseMarked, ["markedWithoutMarker"] = defenseMarkedWithout, ["unmarkedSummon"] = defenseUnmarked },
                ["webUsesStart"] = webUsesStart, ["webUsesRestoredForBaseline"] = webUsesRestored,
                ["scope"] = "Fresh real web hit per escape; loaded = the state's own loaded per-round escape; nativeConfiguration = the installed native action configuration in the same live state. Each comparison follows the rule actually used: a DC fixed in the web context compares the marked summon with the matched unmarked summon's fresh cast; a CMD read at the attempt (the native CMB escape, loaded or configured) and the ordinary grapple compare the same spider with and without its marker, the loaded case from the same spider's own second real hit with the marker removed after the hit. Cross-summon CMD pairs are diagnostics only." },
                "From a real web hit's own held state, each escape triggers its expected rule and its final target number carries exactly +2 from the feat; an ordinary grapple against the same spider is unchanged by the marker.");
            DisposeExpandedSummoningUnits(fixture.Created, new[] { marked[0], unmarked[0] });
        }

        /// <summary>One fresh real web hit from this spider, then the native
        /// configuration attempt (no payload, the state stays) and the loaded
        /// escape from that same live state.</summary>
        private IEnumerable<int> SpiderWebHitAndEscape(UnitEntityData spider, UnitEntityData hostile, BlueprintAbility web, BlueprintBuff webbed, string branch, Action<JObject> report,
            bool markerRemovedAfterHit = false)
        {
            foreach (Buff old in hostile.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => ReferenceEquals(value.Blueprint, webbed)).ToArray())
                hostile.Descriptor.Buffs.RemoveFact(old);
            var directions = new List<int>(); string spot;
            PlaceExpandedSummoningUnit(hostile, ExpandedSummoningOpenPoint(spider.Position, 3f, directions, out spot));
            int ac = hostile.Descriptor.Stats.AC.BaseValue;
            hostile.Descriptor.Stats.AC.BaseValue = -100;
            AbilityResourceLogic cost = web.ComponentsArray.OfType<AbilityResourceLogic>().FirstOrDefault();
            int usesBefore = cost == null ? -1 : spider.Descriptor.Resources.GetResourceAmount(cost.RequiredResource);
            var rolls = new ExpandedSummoningAttackRollObserver { Initiator = spider };
            var saves = new SpiderSaveObserver { Target = hostile };
            UnitUseAbility command = null; int frames = 0; bool ended = false; string startError = null;
            EventBus.Subscribe(rolls); EventBus.Subscribe(saves);
            try
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                try { command = BeginExpandedSummoningDetachedAbility(spider, web, new TargetWrapper(hostile)); }
                catch (InvalidOperationException error) { startError = error.Message; }
                for (; command != null && frames < 420; frames++)
                {
                    if (ended = TickExpandedSummoningDetachedAbility(command, frames)) break;
                    Game.Instance.IsPaused = false; yield return 0;
                }
            }
            finally
            {
                EventBus.Unsubscribe(rolls); EventBus.Unsubscribe(saves);
                if (command != null) EndExpandedSummoningDetachedAbility(command);
                hostile.Descriptor.Stats.AC.BaseValue = ac;
            }
            Buff state = hostile.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(value => ReferenceEquals(value.Blueprint, webbed) && ReferenceEquals(value.Context?.MaybeCaster, spider));
            MechanicsContext context = state == null ? null : state.Context;
            bool ancestry = context != null && ReferenceEquals(context.AssociatedBlueprint, webbed) && context.ParentContext != null &&
                ReferenceEquals(context.ParentContext.AssociatedBlueprint, web) && ReferenceEquals(context.MaybeCaster, spider);
            int usesAfter = cost == null ? -1 : spider.Descriptor.Resources.GetResourceAmount(cost.RequiredResource);
            string hitProblem = startError != null ? "cast-not-started:" + startError : state == null ? "no-webbed-state-after-cast" : !ancestry ? "state-ancestry-differs" :
                saves.Saves.Count != 0 ? "initial-save-rolled" : usesAfter != usesBefore - 1 ? "uses:" + usesBefore + "->" + usesAfter : null;
            var result = new JObject { ["spider"] = spider.UniqueId, ["branch"] = branch, ["spot"] = spot, ["frames"] = frames, ["ended"] = ended,
                ["rolls"] = new JArray(rolls.Rolls), ["initialSaves"] = saves.Saves.Count, ["usesBefore"] = usesBefore, ["usesAfter"] = usesAfter,
                ["state"] = state == null ? null : new JObject { ["buff"] = state.Blueprint.AssetGuid, ["source"] = context?.MaybeCaster?.UniqueId,
                    ["parent"] = context?.ParentContext?.AssociatedBlueprint?.AssetGuid, ["remainingSeconds"] = state.TimeLeft.TotalSeconds },
                ["stateAncestry"] = ancestry, ["hitProblem"] = SpiderEscapeObservationVerdict.Optional(hitProblem) };
            if (hitProblem == null && markerRemovedAfterHit)
            {
                // The same spider's own real hit, then its marker removed before
                // the loaded escape: the baseline for an escape that reads the
                // spider's CMD at the attempt.
                BlueprintBuff marker = SpiderSummonerRuntime.Marker;
                Buff held = marker == null ? null : spider.Descriptor.Buffs.GetBuff(marker);
                if (held == null) result["loaded"] = new JObject { ["error"] = "no marker to remove" };
                else
                {
                    spider.Descriptor.Buffs.RemoveFact(held);
                    try
                    {
                        JObject loaded = SpiderEscapeAttempt(spider, hostile, branch, "loaded", state);
                        loaded["outcome"] = hostile.Descriptor.Buffs.RawFacts.Contains(state) ? "held" : "escaped";
                        result["loaded"] = loaded;
                    }
                    finally { spider.Descriptor.AddBuff(marker, spider, null); }
                }
                result["markerRemovedAfterHit"] = true;
                result["markersAfterRestore"] = SpiderMarkerCount(spider);
            }
            else if (hitProblem == null)
            {
                result["nativeConfiguration"] = SpiderEscapeAttempt(spider, hostile, branch, "native", state);
                // The CMD is calculated per rule, so the same spider and the same
                // live state with its marker removed isolates the feat's effect.
                BlueprintBuff marker = SpiderSummonerRuntime.Marker;
                Buff held = marker == null ? null : spider.Descriptor.Buffs.GetBuff(marker);
                if (held != null)
                {
                    spider.Descriptor.Buffs.RemoveFact(held);
                    result["nativeConfigurationWithoutMarker"] = SpiderEscapeAttempt(spider, hostile, branch, "native", state);
                    spider.Descriptor.AddBuff(marker, spider, null);
                }
                bool present = hostile.Descriptor.Buffs.RawFacts.Contains(state);
                JObject loaded = present ? SpiderEscapeAttempt(spider, hostile, branch, "loaded", state) :
                    new JObject { ["error"] = "state removed before the loaded escape" };
                loaded["outcome"] = hostile.Descriptor.Buffs.RawFacts.Contains(state) ? "held" : "escaped";
                result["loaded"] = loaded;
            }
            if (state != null && hostile.Descriptor.Buffs.RawFacts.Contains(state)) hostile.Descriptor.Buffs.RemoveFact(state);
            report(result);
        }

        // --- Summoning feats, actual poison saves, ineligible sources, NPC control ----------------------------

        private IEnumerable<int> SpiderMechanicsCells(ExpandedSummoningCorrectionFixture fixture, BlueprintFeature feature)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            UnitEntityData caster = fixture.Caster, hostile = fixture.Hostile;
            Sprint28SetBrainActivePreservingActions(hostile, false);
            hostile.Commands.InterruptAll(command => true); hostile.Commands.RemoveFinishedAndUpdateQueue();
            BlueprintAbility ordinary = ExpandedSummoningRoot(fixture.Blueprints, ExpandedSummoningVariant(SummonFamily.Monster, "giant-spider", 2, SummonMultiplicity.One));
            BlueprintFeature augment = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, "38155ca9e4055bb48a89240a2055dcc3", "Augment Summoning");
            BlueprintScriptableObject superiorFound;
            BlueprintFeature superior = library.BlueprintsByAssetId.TryGetValue("0477936c0f74841498b5c8753a8062a3", out superiorFound) ? superiorFound as BlueprintFeature : null;
            int fortitude = hostile.Descriptor.Stats.SaveFortitude.BaseValue;
            SpiderRestoreTarget(hostile);
            Fact featFact = null, augmentFact = null, superiorFact = null;
            try
            {
                hostile.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                // Ordinary Giant Spider, matched clean summons with and without the feat.
                featFact = caster.Descriptor.AddFact(feature);
                JObject markedPoison = SpiderPoisonExposure(fixture, caster, hostile, ordinary, 2, "ordinary-marked");
                caster.Descriptor.RemoveFact(featFact); featFact = null;
                JObject unmarkedPoison = SpiderPoisonExposure(fixture, caster, hostile, ordinary, 2, "ordinary-unmarked");
                string ordinaryProblem = SpiderFirstSaveDelta(markedPoison, unmarkedPoison, SpiderSummonerCatalog.GiantSpiderPoisonFeature, SpiderSummonerCatalog.DcBonus);
                Sprint25Check("spider-poison-ordinary", ordinaryProblem == null, new JObject { ["problem"] = SpiderEscapeObservationVerdict.Optional(ordinaryProblem),
                    ["marked"] = markedPoison, ["unmarked"] = unmarkedPoison },
                    "Actual Giant Spider poison saves attributed by identity: in both matched summons the poison's one failed first save applies its own state, whose continuing saves come from that state at the same DC; the feat summon's DC is the unmarked summon's plus exactly 2.");
                yield return 0;

                // Each Races Unleashed species with its legitimate feat: every poison that actually resolves
                // is its own verified unbonused parameter plus exactly 2.
                featFact = caster.Descriptor.AddFact(feature);
                foreach (SpiderSpeciesSpec species in SpiderSummonerCatalog.All)
                {
                    BlueprintAbility row = (BlueprintAbility)library.BlueprintsByAssetId[SpiderSummonerCatalog.RowIdentity(SummonFamily.Monster, species)];
                    JObject exposure = SpiderPoisonExposure(fixture, caster, hostile, row, species.BaseTier, species.Key);
                    string problem = SpiderSpeciesPoisonProblem(exposure, species);
                    Sprint25Check("spider-poison-" + species.Key, problem == null, new JObject { ["problem"] = SpiderEscapeObservationVerdict.Optional(problem), ["exposure"] = exposure },
                        "Both expected poisons of this species' bite, its own and the inherited Hunting poison, each by identity: one failed first save at its own unbonused parameter plus exactly 2, its own state applied once, and that state's continuing saves (one per poison stat-damage component) at the same DC.");
                    yield return 0;
                }

                // Augment Summoning: Constitution and its DC interaction, with and without the feat.
                augmentFact = caster.Descriptor.AddFact(augment);
                JObject augMarked = SpiderPoisonExposure(fixture, caster, hostile, ordinary, 2, "augment-marked");
                caster.Descriptor.RemoveFact(featFact); featFact = null;
                JObject augUnmarked = SpiderPoisonExposure(fixture, caster, hostile, ordinary, 2, "augment-unmarked");
                string augProblem = SpiderFirstSaveDelta(augMarked, augUnmarked, SpiderSummonerCatalog.GiantSpiderPoisonFeature, SpiderSummonerCatalog.DcBonus) ??
                    ((int)augMarked["constitution"] != (int)markedPoison["constitution"] + 4 ? "augment-constitution:" + markedPoison["constitution"] + "->" + augMarked["constitution"] : null) ??
                    (!(bool)augMarked["augmentBuffOnce"] || !(bool)augUnmarked["augmentBuffOnce"] ? "augment-buff-not-once" : null);
                Sprint25Check("spider-poison-augment", augProblem == null, new JObject { ["problem"] = SpiderEscapeObservationVerdict.Optional(augProblem),
                    ["marked"] = augMarked, ["unmarked"] = augUnmarked, ["withoutAugmentMarked"] = markedPoison["constitution"] },
                    "With Augment Summoning, the summon gains +4 Constitution, and the feat summon's actual poison DC is still the matched unmarked summon's plus exactly 2, with no cumulative bonus.");
                caster.Descriptor.RemoveFact(augmentFact); augmentFact = null;
                yield return 0;

                // Superior Summoning on a multi-summon, with the die forced to the same value.
                featFact = caster.Descriptor.AddFact(feature);
                SpiderPlacement crabD3 = SpiderSummonerCatalog.Placements(SummonFamily.Monster).Single(value => value.Species.Key == "giant-crab-spider" && value.Tier == 2);
                BlueprintAbility crabRow = library.BlueprintsByAssetId.Values.OfType<BlueprintAbility>().Single(value => SpiderSummonerIntegration.PlacementOf(value)?.Symbol == crabD3.Symbol);
                JObject without = SpiderSuperiorCast(fixture, caster, crabRow, 2, crabD3.Species.UnitGuid);
                if (superior != null) superiorFact = caster.Descriptor.AddFact(superior);
                JObject with = superior == null ? new JObject { ["error"] = "Superior Summoning is not installed" } : SpiderSuperiorCast(fixture, caster, crabRow, 2, crabD3.Species.UnitGuid);
                string superiorProblem = with["error"] != null ? (string)with["error"] : without["error"] != null ? (string)without["error"] :
                    (int)without["count"] != 2 ? "base-count:" + without["count"] : (int)with["count"] != 3 ? "superior-count:" + with["count"] :
                    !(bool)without["everyUnitOnce"] || !(bool)with["everyUnitOnce"] ? "per-unit-buffs" : null;
                Sprint25Check("spider-superior-summoning", superiorProblem == null, new JObject { ["problem"] = SpiderEscapeObservationVerdict.Optional(superiorProblem),
                    ["without"] = without, ["with"] = with },
                    "With the 1d3 die forced to 2, the generated row summons 2 without Superior Summoning and exactly 3 with it, and every spider, the extra one included, has its summon buff, one template and exactly one benefit.");
                yield return 0;

                // Ineligible sources with the feat held: no marker.
                var ineligible = new JArray(); bool none = true; int sourcesFound = 0;
                foreach (string key in new[] { "bebelith", "giant-scorpion", "giant-crab", "giant-ant", "fire-beetle" })
                {
                    SummonVariantSpec spec = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Where(value => value.Creature.Key == key && value.Multiplicity == SummonMultiplicity.One)
                        .OrderBy(value => value.ParentTier).FirstOrDefault();
                    if (spec == null) { ineligible.Add(new JObject { ["creature"] = key, ["available"] = false }); continue; }
                    UnitEntityData[] units = SpiderCastRow(fixture, caster, ExpandedSummoningRoot(fixture.Blueprints, spec), spec.ParentTier);
                    int markers = units.Sum(SpiderMarkerCount); sourcesFound++;
                    none &= units.Length > 0 && markers == 0;
                    ineligible.Add(new JObject { ["creature"] = key, ["tier"] = spec.ParentTier, ["units"] = units.Length, ["markers"] = markers });
                    if (units.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, units);
                }
                Sprint25Check("spider-ineligible-sources", none && sourcesFound >= 1, new JObject { ["sources"] = ineligible, ["sourcesFound"] = sourcesFound },
                    "With the feat held, ineligible summons (Bebelith, Giant Scorpion and shared-donor creatures that are summonable) receive no benefit.");
            }
            finally
            {
                if (featFact != null) caster.Descriptor.RemoveFact(featFact);
                if (augmentFact != null) caster.Descriptor.RemoveFact(augmentFact);
                if (superiorFact != null) caster.Descriptor.RemoveFact(superiorFact);
                hostile.Descriptor.Stats.SaveFortitude.BaseValue = fortitude;
            }
            foreach (int frame in SpiderNpcSelectionControl(fixture)) yield return frame;
        }

        /// <summary>
        /// One isolated exposure: summon one spider, clear every poison state on
        /// the hostile, land exactly one forced-hit bite through the native
        /// attack rule, then tick each resulting poison state once. Records every
        /// resolving save with its source effect and DC.
        /// </summary>
        private JObject SpiderPoisonExposure(ExpandedSummoningCorrectionFixture fixture, UnitEntityData caster, UnitEntityData hostile, BlueprintAbility row, int tier, string key)
        {
            UnitEntityData[] units = SpiderCastRow(fixture, caster, row, tier);
            if (units.Length != 1) return new JObject { ["case"] = key, ["error"] = "units:" + units.Length };
            UnitEntityData spider = units[0];
            try
            {
                foreach (Buff old in hostile.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => value.Blueprint.ComponentsArray.OfType<BuffPoisonStatDamage>().Any()).ToArray())
                    hostile.Descriptor.Buffs.RemoveFact(old);
                SpiderRestoreTarget(hostile);
                if (hostile.Descriptor.State.IsDead || !hostile.Descriptor.State.IsConscious) return new JObject { ["case"] = key, ["error"] = "target-not-alive" };
                PlaceExpandedSummoningUnit(hostile, spider.Position + Vector3.forward * 1.5f);
                var features = spider.Descriptor.Progression.Features.Enumerable.Where(value => value?.Blueprint != null && SpiderSummonerBenefitPolicy.AdjustsParametersOf(value.Blueprint.AssetGuid)).ToArray();
                var contexts = new JObject();
                foreach (Feature poison in features) contexts[poison.Blueprint.AssetGuid] = poison.MaybeContext == null ? -1 : poison.MaybeContext.Params.DC;
                JArray expected = SpiderExpectedPoisons(features);
                ItemEntityWeapon weapon = spider.Body.PrimaryHand.MaybeWeapon ?? spider.Body.EmptyHandWeapon;
                var saves = new SpiderSaveObserver { Target = hostile };
                // Every save the target makes fails (d20 forced to 2, Fortitude -100),
                // so each expected poison reaches its own state and continuing save;
                // a natural 20 had left one inherited poison without a state.
                var failed = new SpiderFailedSaveRolls { Target = hostile };
                int attackBonus = spider.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                EventBus.Subscribe(saves); EventBus.Subscribe(failed);
                int attempts = 0;
                try
                {
                    spider.Descriptor.Stats.AdditionalAttackBonus.BaseValue = 100;
                    while (attempts < 6 && saves.Saves.Count == 0)
                    {
                        attempts++;
                        RuleAttackWithWeapon attack = Rulebook.Trigger(new RuleAttackWithWeapon(spider, hostile, weapon, 0));
                        if (attack.AttackRoll != null && attack.AttackRoll.IsHit) break;
                    }
                }
                finally { spider.Descriptor.Stats.AdditionalAttackBonus.BaseValue = attackBonus; }
                var first = saves.Saves.Select(SpiderSaveRecord).ToArray(); saves.Saves.Clear();
                Buff[] states = hostile.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => ReferenceEquals(value.Context?.MaybeCaster, spider) &&
                    value.Blueprint.ComponentsArray.OfType<BuffPoisonStatDamage>().Any()).ToArray();
                foreach (Buff state in states) state.TickMechanics();
                var continuing = saves.Saves.Select(SpiderSaveRecord).ToArray();
                EventBus.Unsubscribe(saves); EventBus.Unsubscribe(failed);
                var targetState = new JObject { ["conscious"] = hostile.Descriptor.State.IsConscious, ["dead"] = hostile.Descriptor.State.IsDead,
                    ["damage"] = hostile.Descriptor.Damage, ["strength"] = hostile.Stats.Strength.ModifiedValue, ["constitution"] = hostile.Stats.Constitution.ModifiedValue,
                    ["buffs"] = new JArray(hostile.Descriptor.Buffs.Enumerable.Select(value => value.Blueprint.name)) };
                // The verified unbonused parameters: the same contexts without the marker, recalculated, then restored.
                var unbonused = new JObject();
                BlueprintBuff marker = SpiderSummonerRuntime.Marker;
                Buff held = marker == null ? null : spider.Descriptor.Buffs.GetBuff(marker);
                if (held != null)
                {
                    spider.Descriptor.Buffs.RemoveFact(held);
                    foreach (Feature poison in features) { poison.MaybeContext?.Recalculate(); unbonused[poison.Blueprint.AssetGuid] = poison.MaybeContext == null ? -1 : poison.MaybeContext.Params.DC; }
                    spider.Descriptor.AddBuff(marker, spider, null);
                    foreach (Feature poison in features) poison.MaybeContext?.Recalculate();
                }
                BlueprintBuff augmentBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library, "169d03bbccdbdc542ae1a57d83673d80", "Augment Summoning buff");
                return new JObject { ["case"] = key, ["unit"] = spider.Blueprint.AssetGuid, ["markers"] = SpiderMarkerCount(spider),
                    ["constitution"] = spider.Stats.Constitution.ModifiedValue,
                    ["augmentBuffOnce"] = spider.Descriptor.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, augmentBuff)) == 1,
                    ["poisonFeatures"] = new JArray(features.Select(value => value.Blueprint.AssetGuid)), ["contextDc"] = contexts, ["unbonusedDc"] = unbonused,
                    ["weapon"] = weapon?.Blueprint?.AssetGuid, ["firstSaves"] = new JArray(first), ["poisonStates"] = new JArray(states.Select(value => value.Blueprint.AssetGuid)),
                    ["continuingSaves"] = new JArray(continuing), ["applications"] = new JArray(saves.Applications), ["target"] = targetState, ["attackAttempts"] = attempts,
                    ["expected"] = expected, ["forcedSaveRolls"] = failed.Forced };
            }
            finally
            {
                foreach (Buff old in hostile.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => ReferenceEquals(value.Context?.MaybeCaster, spider)).ToArray())
                    hostile.Descriptor.Buffs.RemoveFact(old);
                DisposeExpandedSummoningUnits(fixture.Created, units);
            }
        }

        /// <summary>The first native mechanics run killed the target with
        /// accumulated bites and poison damage; every isolated exposure and the
        /// NPC control now start from a living, unharmed target.</summary>
        private static void SpiderRestoreTarget(UnitEntityData target)
        {
            if (target.Descriptor.Stats.HitPoints.BaseValue < 100000) target.Descriptor.Stats.HitPoints.BaseValue = 100000;
            target.Descriptor.Damage = 0;
            foreach (StatType stat in new[] { StatType.Strength, StatType.Dexterity, StatType.Constitution })
            {
                var attribute = target.Descriptor.Stats.GetStat(stat) as ModifiableValueAttributeStat;
                if (attribute != null) attribute.Damage = 0;
            }
        }

        /// <summary>Forces the d20 of each saving throw the target makes to 2.
        /// Armed by the target's own RuleSavingThrow, so attack and other rolls
        /// are untouched.</summary>
        private sealed class SpiderFailedSaveRolls : IGlobalRulebookHandler<RuleSavingThrow>, IGlobalRulebookHandler<RuleRollD20>
        {
            internal UnitEntityData Target; internal int Forced;
            private RuleSavingThrow armed;
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { if (evt != null && ReferenceEquals(evt.Initiator, Target)) armed = evt; }
            public void OnEventDidTrigger(RuleSavingThrow evt) { if (ReferenceEquals(evt, armed)) armed = null; }
            public void OnEventAboutToTrigger(RuleRollD20 evt)
            {
                if (armed == null || evt == null || !ReferenceEquals(evt.Initiator, Target)) return;
                evt.Override(2); Forced++; armed = null;
            }
            public void OnEventDidTrigger(RuleRollD20 evt) { }
        }

        /// <summary>The poison state each feature applies on a failed save, per
        /// Races Unleashed 1.0.11's CreateSpiderPoison (feature, buff) pairs.</summary>
        private static readonly Dictionary<string, string> SpiderRacesUnleashedPoisonStates = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "ddf05b24c484487692764d04079cb899", "37ccf50a465642958b7902cd149c9025" },
            { "b7d97e350ead49b1abd419fa4655d31a", "16b27023541d4f47b599e00847877515" },
            { "b32243a8c4c5439c92a00296fc4bb7a1", "14b7bca42b094ab8979d936136b633be" },
            { "756e0e46215f4a5e8edd79659a6ad992", "f13868529f754fcc8c96d9b503a2ae75" },
        };

        /// <summary>Each poison feature's expected state by identity, read from
        /// the feature's own action tree (the buff it applies that carries poison
        /// stat damage), and that state's continuing saves per tick.</summary>
        private static JArray SpiderExpectedPoisons(IEnumerable<Feature> features)
        {
            var expected = new JArray();
            foreach (Feature poison in features)
            {
                var found = new List<BlueprintBuff>();
                foreach (BlueprintComponent component in poison.Blueprint.ComponentsArray ?? new BlueprintComponent[0])
                    SpiderCollectAppliedBuffs(component, found, new HashSet<object>(), 0);
                BlueprintBuff[] states = found.Where(value => value != null && (value.ComponentsArray ?? new BlueprintComponent[0]).OfType<BuffPoisonStatDamage>().Any()).Distinct().ToArray();
                BlueprintBuff state = states.Length == 1 ? states[0] : null;
                string source;
                bool sourceKnown = SpiderRacesUnleashedPoisonStates.TryGetValue(poison.Blueprint.AssetGuid, out source);
                bool agrees = !sourceKnown || (state != null && state.AssetGuid == source);
                int perTick = state == null ? 0 : state.ComponentsArray.OfType<BuffPoisonStatDamage>().Count(value => value.SaveType != SavingThrowType.Unknown);
                expected.Add(new JObject { ["feature"] = poison.Blueprint.AssetGuid, ["featureName"] = poison.Blueprint.name,
                    ["state"] = agrees ? (JToken)state?.AssetGuid : JValue.CreateNull(), ["stateName"] = state?.name, ["perTick"] = perTick,
                    ["statesFound"] = states.Length, ["racesUnleashedSourceState"] = sourceKnown ? (JToken)source : JValue.CreateNull(), ["agreesWithSource"] = agrees });
            }
            return expected;
        }

        private static void SpiderCollectAppliedBuffs(object node, List<BlueprintBuff> found, HashSet<object> seen, int depth)
        {
            if (node == null || depth > 16 || !seen.Add(node)) return;
            var apply = node as ContextActionApplyBuff;
            if (apply != null && apply.Buff != null) found.Add(apply.Buff);
            var list = node as Kingmaker.ElementsSystem.ActionList;
            if (list != null)
            {
                foreach (Kingmaker.ElementsSystem.GameAction action in list.Actions ?? new Kingmaker.ElementsSystem.GameAction[0]) SpiderCollectAppliedBuffs(action, found, seen, depth + 1);
                return;
            }
            if (!(node is Kingmaker.ElementsSystem.GameAction) && !(node is BlueprintComponent)) return;
            foreach (FieldInfo field in node.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (typeof(Kingmaker.ElementsSystem.ActionList).IsAssignableFrom(field.FieldType) || typeof(Kingmaker.ElementsSystem.GameAction).IsAssignableFrom(field.FieldType))
                    SpiderCollectAppliedBuffs(field.GetValue(node), found, seen, depth + 1);
                else if (field.FieldType == typeof(Kingmaker.ElementsSystem.GameAction[]))
                    foreach (Kingmaker.ElementsSystem.GameAction action in (Kingmaker.ElementsSystem.GameAction[])field.GetValue(node) ?? new Kingmaker.ElementsSystem.GameAction[0])
                        SpiderCollectAppliedBuffs(action, found, seen, depth + 1);
            }
        }

        /// <summary>The ordinary Giant Spider: matched marked and unmarked
        /// summons, each poison fully attributed, first DC exactly +2.</summary>
        internal static string SpiderFirstSaveDelta(JObject marked, JObject unmarked, string feature, int bonus)
        {
            return SpiderPoisonObservationVerdict.MatchedProblem(marked, unmarked, new[] { feature }, bonus);
        }

        /// <summary>A Races Unleashed species: its own poison and the inherited
        /// Hunting poison are both required, each fully attributed, each first
        /// DC its own unbonused parameter plus exactly 2.</summary>
        private static string SpiderSpeciesPoisonProblem(JObject exposure, SpiderSpeciesSpec species)
        {
            if (exposure?["error"] != null) return "error:" + exposure["error"];
            if ((int)exposure["markers"] != 1) return "markers:" + exposure["markers"];
            return SpiderPoisonObservationVerdict.UnbonusedProblem(exposure,
                new[] { species.PoisonFeatureGuid, SpiderSummonerCatalog.HuntingSpiderPoisonFeature }, SpiderSummonerCatalog.DcBonus);
        }

        private JObject SpiderSuperiorCast(ExpandedSummoningCorrectionFixture fixture, UnitEntityData caster, BlueprintAbility row, int tier, string unit)
        {
            var die = new SpiderDieOverride { Initiator = caster, Die = DiceType.D3, Value = 2 };
            UnitEntityData[] units;
            EventBus.Subscribe(die);
            try { units = SpiderCastRow(fixture, caster, row, tier); }
            finally { EventBus.Unsubscribe(die); }
            BlueprintBuff summonBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library, SummonForeignRowClassifier.SummonBuffFor(SummonFamily.Monster, tier), "summon buff");
            ExpandedSummoningBlueprintSet set = ExpandedSummoningBlueprints.Registered;
            Func<UnitEntityData, int> templates = value => value.Descriptor.Buffs.Enumerable.Count(buff => set.BySymbol.Any(pair => pair.Key.StartsWith("KMG.Summoning.Template.", StringComparison.Ordinal) && ReferenceEquals(pair.Value, buff.Blueprint)));
            bool everyUnit = units.Length > 0 && units.All(value => value.Blueprint.AssetGuid == unit && SpiderMarkerCount(value) == 1 && templates(value) == 1 &&
                value.Descriptor.Buffs.Enumerable.Count(buff => ReferenceEquals(buff.Blueprint, summonBuff)) == 1);
            var record = new JObject { ["count"] = units.Length, ["rolls"] = new JArray(die.Rolls), ["forcedDie"] = die.Value, ["everyUnitOnce"] = everyUnit,
                ["markers"] = new JArray(units.Select(SpiderMarkerCount)), ["templates"] = new JArray(units.Select(templates)) };
            if (die.Rolls.Count == 0) record["error"] = "no-d3-roll-observed";
            if (units.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, units);
            return record;
        }

        /// <summary>
        /// The native AI names one exact variant per cast action
        /// (BlueprintAiCastSpell.Variant); its brain never chooses among a
        /// parent's variants. So: no loaded AI action names a spider row, an NPC
        /// without the feat gets a real command for an ordinary variant and
        /// casts it, and gets no command for a gated spider row.
        /// </summary>
        private IEnumerable<int> SpiderNpcSelectionControl(ExpandedSummoningCorrectionFixture fixture)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            BlueprintAiCastSpell[] actions = library.GetAllBlueprints().OfType<BlueprintAiCastSpell>().ToArray();
            int namingSpiders = actions.Count(value => SpiderRowIdentity(value.Variant) || SpiderRowIdentity(value.Ability));
            var parents = new HashSet<string>(ExpandedSummoningPublisher.CanonicalParentGuids, StringComparer.Ordinal);
            int namingParents = actions.Count(value => value.Ability != null && parents.Contains(value.Ability.AssetGuid));
            UnitEntityData npc = fixture.Hostile;
            SpiderRestoreTarget(npc);
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, "b3a505fb61437dc4097f43c3f8f9a4cf", "NPC sorcerer");
            var owner = new TeleportResourceFixtureOwner(npc);
            Spellbook book = owner.AddBook(sorcerer.Spellbook);
            while (book.CasterLevel < 20) book.AddCasterLevel();
            BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, ExpandedSummoningPublisher.CanonicalParentGuids[4], "Summon Monster V");
            book.AddKnown(5, parent, true);
            book.UpdateAllSlotsSize(false); book.Rest();
            BlueprintAbility spider = (parent.Variants ?? new BlueprintAbility[0]).FirstOrDefault(SpiderRowIdentity);
            BlueprintAbility ordinaryVariant = (parent.Variants ?? new BlueprintAbility[0]).FirstOrDefault(value => !SpiderRowIdentity(value) &&
                new AbilityData(value, book) { ConvertedFrom = new AbilityData(parent, book) }.IsAvailableForCast);
            // The native AI action's own context and command decision.
            Func<BlueprintAbility, Tuple<UnitCommand, AbilityData>> decide = variant =>
            {
                var ai = ScriptableObject.CreateInstance<BlueprintAiCastSpell>();
                ai.Ability = parent; ai.Variant = variant; ai.Locators = new EntityReference[0];
                var context = new DecisionContext { Unit = npc };
                ai.InitContext(context);
                return Tuple.Create(ai.CreateCommand(context, fixture.Caster), context.Ability);
            };
            Tuple<UnitCommand, AbilityData> ordinaryDecision = ordinaryVariant == null ? null : decide(ordinaryVariant);
            Tuple<UnitCommand, AbilityData> spiderDecision = spider == null ? null : decide(spider);
            UnitCommand spiderCommand = spiderDecision?.Item1;
            // Summon spells target a point: cast exactly the AI-built ability there.
            UnitUseAbility ordinaryCommand = ordinaryDecision?.Item1 is UnitUseAbility && ordinaryDecision.Item2 != null ?
                UnitUseAbility.CreateCastCommand(ordinaryDecision.Item2, new TargetWrapper(npc.Position + Vector3.right * 2f)) as UnitUseAbility : null;
            var spiderData = spider == null ? null : new AbilityData(spider, book) { ConvertedFrom = new AbilityData(parent, book) };
            string result = "not-run"; int spawned = 0;
            if (ordinaryCommand != null)
            {
                var hooks = HarmonyInstance.Create("kmg.spider.npc." + _request.RunId);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"), new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", BindingFlags.Static | BindingFlags.NonPublic)), null, null);
                    ExpandedSummoningRuleCapture.Clear(); _expandedSummoningPlayerPathSpawnActionCount = 0; _expandedSummoningPlayerPathCaptureActive = true;
                    // The manual clock does not run this NPC's command queue; the AI-built
                    // ability itself is executed through the native ability executor.
                    AbilityData built = ordinaryDecision.Item2;
                    var process = built.Cast(built.CreateExecutionContext(new TargetWrapper(npc.Position + Vector3.right * 2f)));
                    int frames = 0;
                    for (; process != null && !process.IsEnded && frames < 600; frames++) { process.Tick(); Game.Instance.IsPaused = false; yield return 0; }
                    result = _expandedSummoningPlayerPathSpawnActionCount > 0 ? "Success" : "no-spawn:process=" + (process != null) + ";ended=" + (process?.IsEnded) + ";frames=" + frames;
                }
                finally
                {
                    _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id);

                }
                for (int frame = 0; frame < 12; frame++) yield return 0;
                UnitEntityData[] units = ExpandedSummoningRuleCapture.Where(value => value != null).Distinct().ToArray();
                spawned = units.Length;
                foreach (UnitEntityData unit in units) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                if (units.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, units);
            }
            bool ok = namingSpiders == 0 && spider != null && ordinaryVariant != null && ordinaryDecision?.Item1 is UnitUseAbility && ordinaryCommand != null && result == "Success" && spawned >= 1 &&
                !(spiderCommand is UnitUseAbility) && spiderData != null && !spiderData.IsAvailableForCast && !spiderData.IsVisible();
            Sprint25Check("spider-npc-selection", ok, new JObject { ["aiCastActions"] = actions.Length, ["actionsNamingSpiderRows"] = namingSpiders,
                ["actionsNamingSummonParents"] = namingParents, ["ordinaryVariant"] = ordinaryVariant?.AssetGuid, ["ordinaryAiCommand"] = ordinaryDecision?.Item1?.GetType().Name, ["ordinaryAiAbility"] = ordinaryDecision?.Item2?.Blueprint?.AssetGuid,
                ["ordinaryResult"] = result, ["ordinarySpawned"] = spawned, ["execution"] = "AI-built AbilityData executed by the native ability executor; the command-layer spend is not part of this control", ["spiderRow"] = spider?.AssetGuid, ["spiderCommand"] = spiderCommand?.GetType().Name,
                ["spiderAvailable"] = spiderData?.IsAvailableForCast, ["spiderVisible"] = spiderData?.IsVisible() },
                "An NPC without the feat uses the changed Summon Monster V graph through the native AI cast action: an ordinary variant yields a cast command and a real summon; a gated spider row never yields a cast command, so no selection or refusal loop; no loaded AI action names a spider row.");
        }
    }
}
