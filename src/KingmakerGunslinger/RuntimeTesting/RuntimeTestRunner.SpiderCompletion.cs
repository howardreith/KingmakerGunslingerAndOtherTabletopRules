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
        private sealed class SpiderSaveObserver : IGlobalRulebookHandler<RuleSavingThrow>
        {
            internal UnitEntityData Target;
            internal readonly List<RuleSavingThrow> Saves = new List<RuleSavingThrow>();
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt) { if (evt != null && ReferenceEquals(evt.Initiator, Target)) Saves.Add(evt); }
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
                    string input = reading == null ? "absent" : SummonForeignRowClassifier.LegacyInputProblem(reading);
                    originalsIntact &= input == null;
                    rows.Add(new JObject { ["row"] = row?.AssetGuid, ["legacyInputProblem"] = SpiderEscapeObservationVerdict.Optional(input) });
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
            }
            finally
            {
                bab.BaseValue = babBefore; SetExactProperty(mobility, "BaseValue", mobilityBefore); SetExactProperty(athletics, "BaseValue", athleticsBefore);
                hostile.Descriptor.Stats.AC.BaseValue = acBefore;
            }
            bool ok = true; var cases = new JArray();
            foreach (string branch in new[] { "skill", "cmb" })
            {
                JObject m = results["marked:" + branch], u = results["unmarked:" + branch];
                string hit = (string)m["hitProblem"] ?? (string)u["hitProblem"];
                // A DC is fixed in the web's context at the hit, so DC-reading
                // escapes compare the marked summon with the naturally unmarked one.
                string loaded = hit ?? SpiderEscapeObservationVerdict.PairProblem(m["loaded"] as JObject, u["loaded"] as JObject, SpiderSummonerCatalog.DcBonus);
                // A CMD-reading escape is decided within the same spider and state;
                // the pair across two summons is kept as a diagnostic.
                bool cmdBranch = (string)(m["nativeConfiguration"] as JObject)?["rule"] == "RuleCombatManeuver";
                string nativeConfig = hit ?? (cmdBranch ?
                    SpiderEscapeObservationVerdict.PairProblem(m["nativeConfiguration"] as JObject, m["nativeConfigurationWithoutMarker"] as JObject, SpiderSummonerCatalog.DcBonus) :
                    SpiderEscapeObservationVerdict.PairProblem(m["nativeConfiguration"] as JObject, u["nativeConfiguration"] as JObject, SpiderSummonerCatalog.DcBonus));
                string acrossSummons = hit ?? SpiderEscapeObservationVerdict.PairProblem(m["nativeConfiguration"] as JObject, u["nativeConfiguration"] as JObject, SpiderSummonerCatalog.DcBonus);
                ok &= loaded == null && nativeConfig == null;
                cases.Add(new JObject { ["branch"] = branch, ["loadedProblem"] = SpiderEscapeObservationVerdict.Optional(loaded),
                    ["nativeConfigurationProblem"] = SpiderEscapeObservationVerdict.Optional(nativeConfig),
                    ["nativeConfigurationAcrossSummons"] = SpiderEscapeObservationVerdict.Optional(acrossSummons), ["marked"] = m, ["unmarked"] = u });
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
                ["scope"] = "Fresh real web hit per escape; loaded = the state's own loaded per-round escape; nativeConfiguration = the installed native action configuration in the same live state. DC-reading escapes compare the marked and unmarked summons; CMD-reading escapes and the ordinary grapple compare the same spider with and without its marker, with the cross-summon pair kept as a diagnostic." },
                "From a real web hit's own held state, each escape triggers its expected rule and its final target number carries exactly +2 from the feat; an ordinary grapple against the same spider is unchanged by the marker.");
            DisposeExpandedSummoningUnits(fixture.Created, new[] { marked[0], unmarked[0] });
        }

        /// <summary>One fresh real web hit from this spider, then the native
        /// configuration attempt (no payload, the state stays) and the loaded
        /// escape from that same live state.</summary>
        private IEnumerable<int> SpiderWebHitAndEscape(UnitEntityData spider, UnitEntityData hostile, BlueprintAbility web, BlueprintBuff webbed, string branch, Action<JObject> report)
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
            if (hitProblem == null)
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
                    "Actual first and continuing Giant Spider poison saves: the feat summon's resolving DC is the matched unmarked summon's plus exactly 2, and continuing saves add nothing for a single dose.");
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
                        "Every poison that resolves from this species' bite (its own and the inherited Hunting poison) saves against its unbonused parameter plus exactly 2, once, with continuing saves unchanged.");
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
                PlaceExpandedSummoningUnit(hostile, spider.Position + Vector3.forward * 1.5f);
                var features = spider.Descriptor.Progression.Features.Enumerable.Where(value => value?.Blueprint != null && SpiderSummonerBenefitPolicy.AdjustsParametersOf(value.Blueprint.AssetGuid)).ToArray();
                var contexts = new JObject();
                foreach (Feature poison in features) contexts[poison.Blueprint.AssetGuid] = poison.MaybeContext == null ? -1 : poison.MaybeContext.Params.DC;
                ItemEntityWeapon weapon = spider.Body.PrimaryHand.MaybeWeapon ?? spider.Body.EmptyHandWeapon;
                var saves = new SpiderSaveObserver { Target = hostile };
                int attackBonus = spider.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                EventBus.Subscribe(saves);
                try
                {
                    spider.Descriptor.Stats.AdditionalAttackBonus.BaseValue = 100;
                    Rulebook.Trigger(new RuleAttackWithWeapon(spider, hostile, weapon, 0));
                }
                finally { spider.Descriptor.Stats.AdditionalAttackBonus.BaseValue = attackBonus; }
                var first = saves.Saves.Select(SpiderSaveRecord).ToArray(); saves.Saves.Clear();
                Buff[] states = hostile.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => ReferenceEquals(value.Context?.MaybeCaster, spider) &&
                    value.Blueprint.ComponentsArray.OfType<BuffPoisonStatDamage>().Any()).ToArray();
                foreach (Buff state in states) state.TickMechanics();
                var continuing = saves.Saves.Select(SpiderSaveRecord).ToArray();
                EventBus.Unsubscribe(saves);
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
                    ["continuingSaves"] = new JArray(continuing) };
            }
            finally
            {
                foreach (Buff old in hostile.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => ReferenceEquals(value.Context?.MaybeCaster, spider)).ToArray())
                    hostile.Descriptor.Buffs.RemoveFact(old);
                DisposeExpandedSummoningUnits(fixture.Created, units);
            }
        }

        private static IEnumerable<JObject> SpiderFirstSaves(JObject exposure, string feature)
        {
            JArray saves = exposure?["firstSaves"] as JArray;
            return saves == null ? Enumerable.Empty<JObject>() : saves.OfType<JObject>().Where(value => (string)value["reasonFact"] == feature);
        }

        /// <summary>Null when the marked exposure's first save from this feature
        /// is the unmarked one's plus the bonus, both exactly once, and each
        /// state's continuing saves equal its first save.</summary>
        internal static string SpiderFirstSaveDelta(JObject marked, JObject unmarked, string feature, int bonus)
        {
            if (marked?["error"] != null) return "marked:" + marked["error"];
            if (unmarked?["error"] != null) return "unmarked:" + unmarked["error"];
            JObject[] m = SpiderFirstSaves(marked, feature).ToArray(), u = SpiderFirstSaves(unmarked, feature).ToArray();
            if (m.Length != 1 || u.Length != 1) return "first-saves:" + m.Length + "/" + u.Length;
            int with = (int)m[0]["dc"], without = (int)u[0]["dc"];
            if (with != without + bonus) return "first-dc:" + without + "->" + with;
            return SpiderContinuingProblem(marked) ?? SpiderContinuingProblem(unmarked);
        }

        private static string SpiderContinuingProblem(JObject exposure)
        {
            JArray first = exposure["firstSaves"] as JArray, continuing = exposure["continuingSaves"] as JArray;
            if (continuing == null || continuing.Count == 0) return "no-continuing-save:" + exposure["case"];
            int[] firstDcs = first.OfType<JObject>().Select(value => (int)value["dc"]).ToArray();
            return continuing.OfType<JObject>().All(value => firstDcs.Contains((int)value["dc"])) ? null : "continuing-dc-differs:" + exposure["case"];
        }

        private static string SpiderSpeciesPoisonProblem(JObject exposure, SpiderSpeciesSpec species)
        {
            if (exposure?["error"] != null) return "error:" + exposure["error"];
            if ((int)exposure["markers"] != 1) return "markers:" + exposure["markers"];
            JArray first = exposure["firstSaves"] as JArray;
            if (first == null || !first.OfType<JObject>().Any(value => (string)value["reasonFact"] == species.PoisonFeatureGuid)) return "own-poison-did-not-resolve";
            foreach (JObject save in first.OfType<JObject>())
            {
                string source = (string)save["reasonFact"];
                JToken unbonused = (exposure["unbonusedDc"] as JObject)?[source ?? ""];
                if (unbonused == null) return "unattributed-save:" + source;
                if ((int)save["dc"] != (int)unbonused + SpiderSummonerCatalog.DcBonus) return "dc:" + source + ":" + unbonused + "->" + save["dc"];
            }
            if (first.OfType<JObject>().GroupBy(value => (string)value["reasonFact"]).Any(group => group.Count() != 1)) return "repeated-first-save";
            return SpiderContinuingProblem(exposure);
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
                    npc.Commands.Run(ordinaryCommand);
                    while (!ordinaryCommand.IsFinished && watch.Elapsed.TotalSeconds < 20) { Game.Instance.IsPaused = false; yield return 0; }
                    result = ordinaryCommand.IsFinished ? ordinaryCommand.Result.ToString() : "unfinished";
                }
                finally
                {
                    _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id);
                    if (!ordinaryCommand.IsFinished) ordinaryCommand.Interrupt(true);
                }
                for (int frame = 0; frame < 12; frame++) yield return 0;
                UnitEntityData[] units = ExpandedSummoningRuleCapture.Where(value => value != null).Distinct().ToArray();
                spawned = units.Length;
                foreach (UnitEntityData unit in units) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                if (units.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, units);
            }
            bool ok = namingSpiders == 0 && spider != null && ordinaryVariant != null && ordinaryDecision?.Item1 is UnitUseAbility && ordinaryCommand != null && result == "Success" && spawned >= 1 &&
                spiderCommand == null && spiderData != null && !spiderData.IsAvailableForCast && !spiderData.IsVisible();
            Sprint25Check("spider-npc-selection", ok, new JObject { ["aiCastActions"] = actions.Length, ["actionsNamingSpiderRows"] = namingSpiders,
                ["actionsNamingSummonParents"] = namingParents, ["ordinaryVariant"] = ordinaryVariant?.AssetGuid, ["ordinaryAiCommand"] = ordinaryDecision?.Item1?.GetType().Name, ["ordinaryAiAbility"] = ordinaryDecision?.Item2?.Blueprint?.AssetGuid,
                ["ordinaryResult"] = result, ["ordinarySpawned"] = spawned, ["spiderRow"] = spider?.AssetGuid, ["spiderCommand"] = spiderCommand?.GetType().Name,
                ["spiderAvailable"] = spiderData?.IsAvailableForCast, ["spiderVisible"] = spiderData?.IsVisible() },
                "An NPC without the feat uses the changed Summon Monster V graph through the native AI cast action: an ordinary variant yields a command and a real cast; a gated spider row yields no command, so no selection or refusal loop; no loaded AI action names a spider row.");
        }
    }
}
