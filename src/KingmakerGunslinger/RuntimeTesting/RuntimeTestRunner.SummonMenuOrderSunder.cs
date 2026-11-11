using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Owner corrections of 2026-11-10, one save-free phase: a structural
        // census of every published menu and summon grant, real popups (SM VII,
        // a feat-absent and feat-enabled mixed tier V spider menu, Lillend's SM
        // VI menu, a Druid conversion with a generated spider row and one
        // selection), one fresh Storm Giant full attack observed passively,
        // and induced legacy state retired by the real fixers. No older save is
        // cold-loaded and no summon cast is completed from a menu.
        private const string MenuOrderGreaterSunderGuid = "54d824028117e884a8f9356c7c66149b";
        private const string MenuOrderSunderActionGuid = "fa9bfb9fd997faf49a108c4b17a00504";
        private const string MenuOrderNativeGreaterEarthDonorGuid = "cda7013db24f4c547b79bfc5c617066b";

        private sealed class MenuOrderManeuverProbe : IDisposable, IGlobalRulebookHandler<RuleCombatManeuver>
        {
            internal readonly JArray Rows = new JArray();
            private readonly BlueprintUnit _attacker;
            internal MenuOrderManeuverProbe(BlueprintUnit attacker) { _attacker = attacker; EventBus.Subscribe(this); }
            public void Dispose() { EventBus.Unsubscribe(this); }
            public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }
            public void OnEventDidTrigger(RuleCombatManeuver evt)
            {
                try
                {
                    if (evt?.Initiator == null || !ReferenceEquals(evt.Initiator.Blueprint, _attacker)) return;
                    Rows.Add(new JObject { ["type"] = evt.Type.ToString(), ["target"] = evt.Target?.UniqueId,
                        ["initiator"] = evt.Initiator.UniqueId, ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds });
                }
                catch (Exception error) { Rows.Add(new JObject { ["observerError"] = error.GetType().Name + ": " + error.Message }); }
            }
        }

        private static bool MenuOrderIsSunderManeuver(string type)
        { return type != null && type.IndexOf("Sunder", StringComparison.OrdinalIgnoreCase) >= 0; }

        // Every reference to an exact Sunder source reachable from one unit
        // blueprint: its own facts, its brain's actions, and blueprint
        // references held by its components (for example AddClassLevels).
        private static JArray MenuOrderSunderSources(BlueprintUnit unit, HashSet<string> sources)
        {
            var found = new JArray();
            foreach (BlueprintUnitFact fact in unit.AddFacts ?? new BlueprintUnitFact[0])
                if (fact != null && sources.Contains(fact.AssetGuid)) found.Add("fact:" + fact.name);
            foreach (BlueprintAiAction action in unit.Brain?.Actions ?? new BlueprintAiAction[0])
            {
                if (action == null) continue;
                if (sources.Contains(action.AssetGuid)) found.Add("brain:" + action.name);
                var cast = action as BlueprintAiCastSpell;
                if (cast?.Ability != null && sources.Contains(cast.Ability.AssetGuid)) found.Add("brain-cast:" + action.name);
            }
            foreach (BlueprintComponent component in unit.ComponentsArray ?? new BlueprintComponent[0])
                MenuOrderWalk(component, sources, found, "component:" + component?.GetType().Name, 0);
            return found;
        }

        private static void MenuOrderWalk(object value, HashSet<string> sources, JArray found, string path, int depth)
        {
            if (value == null || depth > 3) return;
            var blueprint = value as BlueprintScriptableObject;
            if (blueprint != null && depth > 0) { if (sources.Contains(blueprint.AssetGuid)) found.Add(path + ":" + blueprint.name); return; }
            if (value is UnityEngine.Object && depth > 0) return;
            if (value is string || value.GetType().IsPrimitive || value.GetType().IsEnum) return;
            var array = value as Array;
            if (array != null) { foreach (object item in array) MenuOrderWalk(item, sources, found, path, depth + 1); return; }
            foreach (FieldInfo field in value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.FieldType.IsPrimitive || field.FieldType == typeof(string)) continue;
                object child; try { child = field.GetValue(value); } catch { continue; }
                MenuOrderWalk(child, sources, found, path + "." + field.Name, depth + 1);
            }
        }

        // Structural Sunder census over every unit a published canonical or
        // alternate summon row can spawn, plus the shared native controls.
        private JObject MenuOrderSunderCensus(out bool passed)
        {
            var library = BlueprintBootstrap.Library;
            var sources = new HashSet<string>(StringComparer.Ordinal) { SummonPlaytestWithdrawalPolicy.NativeImprovedSunderGuid,
                MenuOrderGreaterSunderGuid, MenuOrderSunderActionGuid, SummonPlaytestWithdrawalPolicy.NativeSunderArmorAiActionGuid,
                ProperFlankingSunderToggleGuid, ProperFlankingSunderBuffGuid };
            var roots = ExpandedSummoningPublisher.CanonicalParentGuids.Select(guid => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, guid, "census parent"))
                .Concat(library.BlueprintsByAssetId.Values.OfType<BlueprintAbility>().Where(AlternateSummoningExposure.IsPublishedRoot)).ToArray();
            var units = new Dictionary<string, BlueprintUnit>(StringComparer.Ordinal);
            int rows = 0;
            foreach (BlueprintAbility root in roots)
                foreach (BlueprintAbility row in root.Variants ?? new BlueprintAbility[0])
                {
                    if (row == null) continue; rows++;
                    foreach (string guid in AlternateSummoningGraph.SpawnUnits(row))
                    {
                        BlueprintScriptableObject found;
                        if (!units.ContainsKey(guid) && library.BlueprintsByAssetId.TryGetValue(guid, out found) && found is BlueprintUnit)
                            units.Add(guid, (BlueprintUnit)found);
                    }
                }
            var carriers = new JArray();
            foreach (BlueprintUnit unit in units.Values.OrderBy(value => value.AssetGuid, StringComparer.Ordinal))
            {
                JArray found = MenuOrderSunderSources(unit, sources);
                if (found.Count > 0) carriers.Add(new JObject { ["unit"] = unit.AssetGuid, ["name"] = unit.name, ["sources"] = found });
            }
            var symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var ordinaryBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library, SummonPlaytestWithdrawalPolicy.NativeDumbMonsterBrainGuid, "ordinary brain");
            var sunderBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library, SummonPlaytestWithdrawalPolicy.NativeGreaterEarthElementalBrainGuid, "native sunder brain");
            var sunderAi = BlueprintLibraryLookup.RequireExact<BlueprintAiCastSpell>(library, SummonPlaytestWithdrawalPolicy.NativeSunderArmorAiActionGuid, "native sunder AI");
            var improved = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SummonPlaytestWithdrawalPolicy.NativeImprovedSunderGuid, "native Improved Sunder");
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library, MenuOrderNativeGreaterEarthDonorGuid, "native Greater Earth Elemental donor");
            var elementals = SummonPlaytestWithdrawalPolicy.DonorInheritedSunderKeys.Select(key =>
                (BlueprintUnit)symbols[SummonPlaytestWithdrawalPolicy.For(key).UnitSymbol]).ToArray();
            var fixers = new JObject();
            foreach (WithdrawnCapabilitySpec spec in SummonPlaytestWithdrawalPolicy.All)
            {
                var unit = (BlueprintUnit)symbols[spec.UnitSymbol];
                fixers[spec.CreatureKey] = new JArray(unit.GetComponents<SummonWithdrawnCapabilityFixer>().Select(value => value.name));
            }
            bool elementalsCorrected = elementals.All(unit => ReferenceEquals(unit.Brain, ordinaryBrain));
            bool nativeUnchanged = (donor.AddFacts ?? new BlueprintUnitFact[0]).Contains(improved) && ReferenceEquals(donor.Brain, sunderBrain) &&
                (sunderBrain.Actions ?? new BlueprintAiAction[0]).Contains(sunderAi) &&
                improved.GetComponents<Kingmaker.UnitLogic.FactLogic.AddFacts>().SelectMany(value => value.Facts ?? new BlueprintUnitFact[0]).Any(value => value?.AssetGuid == MenuOrderSunderActionGuid);
            bool fixersAttached = new[] { "storm-giant", "greater-earth-elemental", "elder-earth-elemental" }.All(key =>
                ((JArray)fixers[key]).Select(value => (string)value).SequenceEqual(new[] { SummonPlaytestWithdrawalPolicy.FixerName(key) }));
            passed = carriers.Count == 0 && elementalsCorrected && nativeUnchanged && fixersAttached && units.Count > 0;
            return new JObject { ["rootsScanned"] = roots.Length, ["rowsScanned"] = rows, ["unitsScanned"] = units.Count,
                ["carriers"] = carriers, ["elementalBrainsOrdinary"] = elementalsCorrected,
                ["sharedNativeDefinitionsUnchanged"] = nativeUnchanged, ["fixers"] = fixers, ["newFixersAttached"] = fixersAttached,
                ["scope"] = "Structural: unit facts, brain actions and component blueprint references of every unit a published canonical or alternate row spawns." };
        }

        // Each spider placement in its published parent, labelled and grouped.
        private static JObject MenuOrderSpiderCensus(out bool passed)
        {
            var library = BlueprintBootstrap.Library;
            var problems = new JArray(); int found = 0;
            foreach (SpiderPlacement placement in SpiderSummonerCatalog.AllPlacements)
            {
                BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                    ExpandedSummoningPublisher.CanonicalParentGuids[(placement.Family == SummonFamily.Monster ? 0 : 9) + placement.Tier - 1], "spider parent");
                BlueprintAbility[] variants = parent.Variants ?? new BlueprintAbility[0];
                BlueprintAbility[] rows = variants.Where(value => value != null && (placement.IsOriginal ?
                    value.AssetGuid == SpiderSummonerCatalog.RowIdentity(placement.Family, placement.Species) :
                    SpiderSummonerIntegration.PlacementOf(value)?.Symbol == placement.Symbol)).ToArray();
                if (rows.Length != 1) { problems.Add(placement.Symbol + ":count=" + rows.Length); continue; }
                found++;
                int index = Array.IndexOf(variants, rows[0]);
                int rank = AlphaQuantityRank(rows[0].Name ?? "");
                int expected = (int)placement.Quantity;
                bool groupOk = rank == expected && variants.Take(index).All(value => value == null || !AlphaIsCreatureChoice(value) || AlphaQuantityRank(value.Name ?? "") <= rank) &&
                    variants.Skip(index + 1).All(value => value == null || !AlphaIsCreatureChoice(value) || AlphaQuantityRank(value.Name ?? "") >= rank);
                if (!groupOk) problems.Add(placement.Symbol + ":group:" + rows[0].Name);
            }
            passed = found == 46 && problems.Count == 0;
            return new JObject { ["placementsFound"] = found, ["expected"] = 46, ["problems"] = problems,
                ["integration"] = SpiderSummonerIntegration.Outcome, ["lateOrderOutcome"] = ExpandedSummoningPublisher.LateOrderOutcome };
        }

        // Opens a popup, selects the first row matching the predicate when it
        // is available, records the exact selected row object, and closes.
        private IEnumerable<int> MenuOrderSelectRow(string label, ActionBarGroupSlot source, AbilityData original, Func<AbilityData, bool> match,
            Cleanup2NativeDefaultScope mode, Stopwatch watch, Action<JObject> result)
        {
            Cleanup2MenuAdapterBoundary(mode, watch);
            foreach (int frame in Cleanup2AwaitStationaryNativeHud(source, "order-" + label)) yield return frame;
            source.OnToggleGroupClick();
            for (int frame = 0; frame < 10; frame++) yield return 0;
            try
            {
                AlphaMenu menu = AlphaReadPopup(source);
                int index = Array.FindIndex(menu.Data, value => value != null && match(value));
                var evidence = new JObject { ["menu"] = label, ["rowIndex"] = index };
                if (index >= 0)
                {
                    AbilityData data = menu.Data[index];
                    bool available = data.IsAvailableForCast;
                    if (available) menu.Rows[index].OnClick();
                    bool selected = available && ReferenceEquals(Game.Instance.SelectedAbilityHandler.Ability, data);
                    DruidSummonConversionVariantPatch.Receipt receipt = DruidSummonConversionVariantPatch.For(data);
                    evidence["label"] = data.Name; evidence["ability"] = data.Blueprint.AssetGuid; evidence["internalName"] = data.Blueprint.name;
                    evidence["available"] = available; evidence["selectedExactRowObject"] = selected;
                    evidence["convertedFrom"] = data.ConvertedFrom?.Blueprint.AssetGuid;
                    evidence["sourceIsOriginal"] = data.ConvertedFrom != null && ReferenceEquals(data.ConvertedFrom.ConvertedFrom, original);
                    evidence["receiptSourceIsOriginal"] = receipt != null && ReferenceEquals(receipt.Source, original);
                    evidence["sameBook"] = ReferenceEquals(data.Spellbook, original.Spellbook);
                    evidence["spiderPlacement"] = SpiderSummonerIntegration.PlacementOf(data.Blueprint)?.Symbol;
                    if (selected) Game.Instance.SelectedAbilityHandler.DropAbility();
                }
                result(evidence);
            }
            finally { Cleanup2MenuAdapterHide(source, original); }
        }

        private IEnumerable<int> ReviewSummonMenuOrderSunder(ExpandedSummoningCorrectionFixture fixture, Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) || fixture?.Caster == null ||
                fixture.Hostile == null || !_context.FeatureModules.Active.ExpandedSummoning || mode == null || !mode.Current || !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh menu-order/Sunder fixture with the summoning module enabled required.");
            var game = Game.Instance; var player = game.Player; var ui = game.UI; var group = GroupController.Instance; var bar = ActionBarManager.Instance;
            if (group == null || bar?.Group == null || ui?.SelectionManagerPC == null || game.IsControllerGamepad)
                throw new InvalidOperationException("Actual desktop native action bar required.");
            var library = BlueprintBootstrap.Library;

            // 1. Structural census.
            bool censusPassed; JObject census = AlphaCensus(out censusPassed);
            Sprint25Check("order-census", censusPassed, census,
                "Every published canonical and alternate menu array lists every single summon, then every 1d3, then every 1d4+1, alphabetized inside each group; Stirge stays absent.");
            bool spiderPassed; JObject spiderCensus = MenuOrderSpiderCensus(out spiderPassed);
            Sprint25Check("order-spider-census", spiderPassed, spiderCensus,
                "All 46 Spider Summoner placements (8 original, 38 generated) appear once in their own parent, inside their own matrix quantity group.");
            bool sunderPassed; JObject sunderCensus = MenuOrderSunderCensus(out sunderPassed);
            Sprint25Check("sunder-census", sunderPassed, sunderCensus,
                "No unit spawned by a published summon row carries Improved Sunder, Greater Sunder, the Sunder action, the Sunder AI action or ProperFlanking2's Sunder toggle; the two earth elementals use the native ordinary brain; the shared native donor, brain and feat are unchanged; the new fixers are attached.");
            var lillendRows = new JArray(); bool lillendLabels = true;
            for (int tier = 6; tier <= 9; tier++)
            {
                BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, ExpandedSummoningPublisher.CanonicalParentGuids[tier - 1], "Lillend parent");
                BlueprintAbility row = (parent.Variants ?? new BlueprintAbility[0]).SingleOrDefault(value => value != null && value.name.StartsWith("KMG_Summoning_Ability_SM_Tier" + tier + "_LillendAzata_", StringComparison.Ordinal));
                string expected = tier == 6 ? "Azata, Lillend" : tier == 7 ? "Azata, Lillend (1d3)" : "Azata, Lillend (1d4+1)";
                lillendLabels &= row != null && row.Name == expected;
                lillendRows.Add(new JObject { ["tier"] = tier, ["internalName"] = row?.name, ["label"] = row?.Name, ["expected"] = expected });
            }
            Sprint25Check("order-lillend-labels", lillendLabels, new JObject { ["rows"] = lillendRows },
                "The published Lillend rows read Azata, Lillend / (1d3) / (1d4+1) with unchanged identities.");

            // 2. Real popups.
            var party = player.Party.ToArray(); var selected = ui.SelectionManagerPC.SelectedUnits.ToArray(); var selectedGroup = group.GetCurrentCharacter();
            var groups = bar.Group.GroupElements.ToArray(); bool[] toggles = groups.Select(value => value.ToggleState).ToArray();
            var addedBooks = new TeleportResourceFixtureOwner(fixture.Caster); var facts = new List<Action>();
            var watch = Stopwatch.StartNew(); bool added = false; NativeIconScreenEvidence screens = null;
            try
            {
                if (toggles.Any(value => value)) throw new InvalidOperationException("Closed native groups required at intake.");
                var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, "b3a505fb61437dc4097f43c3f8f9a4cf", "native Sorcerer");
                var druidClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, "610d836f3a3a9ed42a4349b62f002e96", "native Druid");
                Func<SummonFamily, int, BlueprintAbility> canonical = (family, tier) => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                    ExpandedSummoningPublisher.CanonicalParentGuids[(family == SummonFamily.Monster ? 0 : 9) + tier - 1], "order canonical parent");
                var known = addedBooks.AddBook(sorcerer.Spellbook);
                BlueprintAbility sm5 = canonical(SummonFamily.Monster, 5), sm6 = canonical(SummonFamily.Monster, 6), sm7 = canonical(SummonFamily.Monster, 7);
                known.AddKnown(5, sm5, true); known.AddKnown(6, sm6, true); known.AddKnown(7, sm7, true); known.Rest();
                var druid = addedBooks.AddBook(druidClass.Spellbook);
                var feature = druidClass.Progression.LevelEntries.SelectMany(value => value.Features).OfType<BlueprintFeature>().Distinct()
                    .Single(value => DruidSummonConversionVariantPatch.ExactNativeFeature(value, druidClass));
                var conversionFact = fixture.Caster.Descriptor.AddFact(feature);
                facts.Add(() => fixture.Caster.Descriptor.RemoveFact(conversionFact));
                const int DruidTier = 5;
                var druidParent = canonical(SummonFamily.NaturesAlly, DruidTier);
                var ordinary = druid.Blueprint.SpellList.GetSpells(DruidTier).Where(value => value != null && !ReferenceEquals(value, druidParent) &&
                    (value.Variants == null || value.Variants.Length == 0)).OrderBy(value => value.AssetGuid, StringComparer.Ordinal).First();
                var druidPrepared = PrepareExpandedSummoningPlayerPathSpell(druid, ordinary, ordinary, DruidTier);
                druid.Rest();
                var spiderFeature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SummonForeignRowClassifier.SpiderSummonerFeatureGuid, "Spider Summoner feature");
                if (fixture.Caster.Descriptor.HasFact(spiderFeature)) throw new InvalidOperationException("The fixture caster must start without Spider Summoner.");
                if (!player.Party.Contains(fixture.Caster))
                { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                screens = new NativeIconScreenEvidence(_request);
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                if (!mode.Current) throw new InvalidOperationException("Native Default did not settle after menu-order book setup.");
                var inputs = new Cleanup2TooltipInputs(fixture.Caster);
                var notExercised = new JArray();
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement).GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup);
                Func<int, Func<AbilityData, bool>, ActionBarGroupSlot> findSource = (tier, match) => spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value =>
                    value.gameObject.activeInHierarchy && match(value.MechanicSlot?.GetContentData() as AbilityData));
                Func<int, IEnumerable<int>> openLevel = tier => OpenMenuOrderLevel(spellGroup, levels, fixture.Caster, tier);
                Func<AbilityData, string> spiderKind = data => data == null ? null : SpiderSummonerIntegration.PlacementOf(data.Blueprint) != null ? "generated" :
                    SpiderSummonerCatalog.AllPlacements.Any(value => value.IsOriginal && SpiderSummonerCatalog.RowIdentity(value.Family, value.Species) == data.Blueprint.AssetGuid) ? "original" : null;

                // Feat-absent control at Summon Monster V.
                foreach (int frame in openLevel(5)) yield return frame;
                var sm5Source = findSource(5, data => ReferenceEquals(data?.Blueprint, sm5) && ReferenceEquals(data?.Spellbook, known));
                var sm5Data = (AbilityData)sm5Source.MechanicSlot.GetContentData();
                AlphaMenu absent = null;
                foreach (int frame in R6Cell("order-sm5-feat-absent", () => AlphaObserveMenu("sm5-feat-absent", sm5Source, sm5, sm5Data, mode, watch, screens, inputs, notExercised, value => absent = value, false, -1))) yield return frame;
                int absentSpiders = absent == null ? -1 : absent.Data.Count(value => spiderKind(value) != null);
                int ordinarySpiders = absent == null ? -1 : absent.Data.Count(value => value != null && value.Blueprint.name.Contains("_GiantSpider_"));
                Sprint25Check("order-spider-feat-absent", absent != null && absentSpiders == 0 && ordinarySpiders > 0,
                    new JObject { ["rows"] = absent?.Rows.Length ?? 0, ["spiderSummonerRows"] = absentSpiders, ["ordinaryGiantSpiderRows"] = ordinarySpiders },
                    "Without Spider Summoner no feat-gated spider row is visible, and the ordinary Giant Spider row still is.");

                // Feat-enabled mixed tier V popup (feature granted to the fixture caster).
                var spiderFact = fixture.Caster.Descriptor.AddFact(spiderFeature);
                facts.Add(() => fixture.Caster.Descriptor.RemoveFact(spiderFact));
                foreach (int frame in openLevel(5)) yield return frame;
                sm5Source = findSource(5, data => ReferenceEquals(data?.Blueprint, sm5) && ReferenceEquals(data?.Spellbook, known));
                sm5Data = (AbilityData)sm5Source.MechanicSlot.GetContentData();
                AlphaMenu enabled = null;
                foreach (int frame in R6Cell("order-sm5-feat-enabled", () => AlphaObserveMenu("sm5-feat-enabled", sm5Source, sm5, sm5Data, mode, watch, screens, inputs, notExercised, value => enabled = value, false, -1))) yield return frame;
                Func<AbilityData, string> speciesOf = data =>
                {
                    SpiderPlacement placement = data == null ? null : SpiderSummonerIntegration.PlacementOf(data.Blueprint);
                    if (placement != null) return placement.Species.Key;
                    SpiderSpeciesSpec species = data == null ? null : SpiderSummonerCatalog.All.FirstOrDefault(value => SpiderSummonerCatalog.Families.Any(family =>
                        SpiderSummonerCatalog.RowIdentity(family, value) == data.Blueprint.AssetGuid));
                    return species?.Key;
                };
                Func<AlphaMenu, string, int> speciesIndex = (menu, key) => menu == null ? -1 : Array.FindIndex(menu.Data, value => speciesOf(value) == key);
                int ogre = speciesIndex(enabled, "ogre-spider"), widow = speciesIndex(enabled, "giant-black-widow"), crab = speciesIndex(enabled, "giant-crab-spider");
                Func<int, int> rankAt = index => index < 0 ? -1 : AlphaQuantityRank(enabled.Data[index].Name ?? "");
                int firstD3 = enabled == null ? -1 : Array.FindIndex(enabled.Data, value => AlphaQuantityRank(value?.Name ?? "") == 1);
                int firstD4 = enabled == null ? -1 : Array.FindIndex(enabled.Data, value => AlphaQuantityRank(value?.Name ?? "") == 2);
                int spiderRows = enabled == null ? -1 : enabled.Data.Count(value => spiderKind(value) != null);
                bool mixed = enabled != null && ogre >= 0 && widow >= 0 && crab >= 0 && rankAt(ogre) == 0 && rankAt(widow) == 1 && rankAt(crab) == 2 &&
                    ogre < firstD3 && widow >= firstD3 && widow < firstD4 && crab >= firstD4 && spiderRows == 3 &&
                    enabled.Data.Select(value => value?.Blueprint).Distinct().Count() == enabled.Data.Length;
                Sprint25Check("order-spider-tier5-mixed", mixed, new JObject { ["rows"] = enabled?.Rows.Length ?? 0,
                    ["ogre"] = ogre < 0 ? null : new JObject { ["index"] = ogre, ["label"] = enabled.Data[ogre].Name, ["ability"] = enabled.Data[ogre].Blueprint.AssetGuid, ["kind"] = spiderKind(enabled.Data[ogre]) },
                    ["widow"] = widow < 0 ? null : new JObject { ["index"] = widow, ["label"] = enabled.Data[widow].Name, ["ability"] = enabled.Data[widow].Blueprint.name, ["kind"] = spiderKind(enabled.Data[widow]) },
                    ["crab"] = crab < 0 ? null : new JObject { ["index"] = crab, ["label"] = enabled.Data[crab].Name, ["ability"] = enabled.Data[crab].Blueprint.name, ["kind"] = spiderKind(enabled.Data[crab]) },
                    ["firstOneD3"] = firstD3, ["firstOneD4PlusOne"] = firstD4, ["spiderRows"] = spiderRows, ["featSource"] = "fixture-granted exact Spider Summoner feature" },
                    "With Spider Summoner, Summon Monster V shows the original Ogre Spider in the single group, the generated Black Widow (1d3) in the 1d3 group and the generated Crab Spider (1d4+1) in the 1d4+1 group, each once.");

                // Unrestricted Summon Monster VII.
                foreach (int frame in openLevel(7)) yield return frame;
                var sm7Source = findSource(7, data => ReferenceEquals(data?.Blueprint, sm7) && ReferenceEquals(data?.Spellbook, known));
                var sm7Data = (AbilityData)sm7Source.MechanicSlot.GetContentData();
                AlphaMenu seven = null;
                foreach (int frame in R6Cell("order-sm7", () => AlphaObserveMenu("sm7", sm7Source, sm7, sm7Data, mode, watch, screens, inputs, notExercised, value => seven = value, false, -1))) yield return frame;
                int bebelith = seven == null ? -1 : Array.FindIndex(seven.Data, value => value?.Name == "Bebelith");
                int ankylosaurus = seven == null ? -1 : Array.FindIndex(seven.Data, value => value?.Name == "Ankylosaurus (1d4+1)");
                int firstGroup7 = seven == null ? -1 : Array.FindIndex(seven.Data, value => AlphaQuantityRank(value?.Name ?? "") != 0);
                int lastD3Seven = seven == null ? -1 : Array.FindLastIndex(seven.Data, value => AlphaQuantityRank(value?.Name ?? "") == 1);
                bool sevenOk = seven != null && seven.Data.Length > 0 && AlphaQuantityRank(seven.Data[0].Name ?? "") == 0 && bebelith >= 0 && bebelith < firstGroup7 &&
                    ankylosaurus > lastD3Seven && ankylosaurus > 0;
                Sprint25Check("order-sm7-single-group-first", sevenOk, new JObject { ["rows"] = seven?.Rows.Length ?? 0,
                    ["first"] = seven?.Data.FirstOrDefault()?.Name, ["bebelithIndex"] = bebelith, ["firstGroupIndex"] = firstGroup7,
                    ["ankylosaurusIndex"] = ankylosaurus, ["lastOneD3Index"] = lastD3Seven },
                    "An unrestricted Summon Monster VII starts with its alphabetized single group, which includes Bebelith; Ankylosaurus (1d4+1) is in the last group.");

                // Lillend in its actual Summon Monster VI menu.
                foreach (int frame in openLevel(6)) yield return frame;
                var sm6Source = findSource(6, data => ReferenceEquals(data?.Blueprint, sm6) && ReferenceEquals(data?.Spellbook, known));
                var sm6Data = (AbilityData)sm6Source.MechanicSlot.GetContentData();
                AlphaMenu six = null;
                foreach (int frame in R6Cell("order-sm6", () => AlphaObserveMenu("sm6", sm6Source, sm6, sm6Data, mode, watch, screens, inputs, notExercised, value => six = value, false, -1))) yield return frame;
                int lillend = six == null ? -1 : Array.FindIndex(six.Data, value => value?.Name == "Azata, Lillend");
                int firstGroup6 = six == null ? -1 : Array.FindIndex(six.Data, value => AlphaQuantityRank(value?.Name ?? "") != 0);
                Sprint25Check("order-lillend-menu", lillend >= 0 && lillend < firstGroup6 && six.Data[lillend].Blueprint.name == "KMG_Summoning_Ability_SM_Tier6_LillendAzata_One",
                    new JObject { ["index"] = lillend, ["label"] = lillend < 0 ? null : six.Data[lillend].Name, ["ability"] = lillend < 0 ? null : six.Data[lillend].Blueprint.name,
                        ["previous"] = lillend > 0 ? six.Data[lillend - 1].Name : null, ["next"] = lillend >= 0 && lillend + 1 < six.Data.Length ? six.Data[lillend + 1].Name : null },
                    "The actual Summon Monster VI popup shows \"Azata, Lillend\" with its unchanged identity, inside the single group.");

                // Druid conversion with generated spider rows.
                foreach (int frame in openLevel(DruidTier)) yield return frame;
                var druidSource = findSource(DruidTier, data => ReferenceEquals(data, druidPrepared));
                var factory = druidSource.MechanicSlot.GetConvertedAbilityData().ToArray();
                AlphaMenu flat = null;
                foreach (int frame in R6Cell("order-druid-conversion", () => AlphaObserveMenu("druid-conversion-5", druidSource, druidParent, druidPrepared, mode, watch, screens, inputs, notExercised, value => flat = value, false, -1))) yield return frame;
                bool factoryMatches = flat != null && factory.Length == flat.Data.Length && factory.Select(value => value.Blueprint).SequenceEqual(flat.Data.Select(value => value.Blueprint));
                int generated = flat == null ? -1 : flat.Data.Count(value => spiderKind(value) == "generated");
                Sprint25Check("order-druid-conversion-spiders", factoryMatches && generated >= 2 && flat.Data.Select(value => value?.Blueprint).Distinct().Count() == flat.Data.Length,
                    new JObject { ["rows"] = flat?.Rows.Length ?? 0, ["factoryMatchesPopup"] = factoryMatches, ["generatedSpiderRows"] = generated,
                        ["spiderRows"] = flat == null ? null : new JArray(flat.Data.Where(value => spiderKind(value) != null).Select(value => value.Name + " [" + spiderKind(value) + "]")),
                        ["conversionAdapter"] = DruidSummonConversionVariantPatch.Outcome },
                    "The Druid conversion popup equals the native factory, lists generated spider rows once each, and has no duplicates.");
                JObject selection = null;
                foreach (int frame in R6Cell("order-druid-select", () => MenuOrderSelectRow("druid-conversion-5", druidSource, druidPrepared,
                    data => spiderKind(data) == "generated", mode, watch, value => selection = value))) yield return frame;
                bool selectionOk = selection != null && (bool?)selection["selectedExactRowObject"] == true && (bool?)selection["sourceIsOriginal"] == true &&
                    (bool?)selection["receiptSourceIsOriginal"] == true && (bool?)selection["sameBook"] == true && (string)selection["spiderPlacement"] != null;
                Sprint25Check("order-druid-select-generated-spider", selectionOk, selection ?? new JObject(),
                    "Selecting a generated spider row in the Druid conversion selects that row's own ability, converted from the native quantity conversion of the exact prepared source in the same book. Selection only; no cast.");

                Sprint25Check("order-surface-limits", true, new JObject {
                    ["openedInGame"] = "Known-book Summon Monster V (feat absent and present), VI and VII; one Druid level 5 conversion.",
                    ["structuralOnly"] = "Every other menu, tier, family, alternate source and Herald route is covered by the census only.",
                    ["fixtureAccess"] = "Books, the conversion feature and Spider Summoner were granted to the disposable caster; not a natural character or saved hotbar.",
                    ["wallSeconds"] = watch.Elapsed.TotalSeconds },
                    "Opened and structural surfaces are reported separately.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                for (int index = facts.Count - 1; index >= 0; index--) restore("fact-" + index, facts[index]);
                restore("books", () => addedBooks.Restore());
                restore("party", () => { if (added) { player.PartyCharacters.Remove(fixture.Caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); } });
                restore("selection", () => { group.SelectUnit(selectedGroup); ui.SelectionManagerPC.MultiSelect(selected.Where(value => value?.View != null).Select(value => value.View).ToArray(), false); });
                for (int index = 0; index < groups.Length; index++) { int scoped = index; restore("toggle-" + scoped, () => groups[scoped].Toggle(toggles[scoped], true)); }
                restore("screens", () => screens?.Dispose());
                Sprint25Check("order-owned-state-restored", errors.Count == 0 && player.Party.SequenceEqual(party), new JObject { ["errors"] = new JArray(errors) },
                    "Only the disposable caster's owned books, facts and UI selection were changed, and they are restored.");
            }

            // 3. Fresh Storm Giant: Sunder state, then one native full attack.
            foreach (int frame in R6Cell("sunder-storm-giant", () => MenuOrderStormGiant(fixture))) yield return frame;
            // 4. Induced legacy state retired by the real fixers, with controls.
            foreach (int frame in R6Cell("sunder-legacy", () => MenuOrderLegacyCleanup(fixture))) yield return frame;
        }

        private IEnumerable<int> OpenMenuOrderLevel(ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, UnitEntityData caster, int tier)
        {
            if (!spellGroup.ToggleState) spellGroup.OnClick();
            spellGroup.Fill(caster);
            levels.OnClick(tier);
            for (int frame = 0; frame < 12; frame++) yield return 0;
        }

        private JObject MenuOrderSunderState(UnitEntityData unit)
        {
            var library = BlueprintBootstrap.Library;
            var improved = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SummonPlaytestWithdrawalPolicy.NativeImprovedSunderGuid, "native Improved Sunder");
            var toggle = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>(ProperFlankingSunderToggleGuid);
            var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(ProperFlankingSunderBuffGuid);
            var action = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>(MenuOrderSunderActionGuid);
            ActivatableAbility live = toggle == null ? null : unit.Descriptor.ActivatableAbilities.Enumerable.FirstOrDefault(value => ReferenceEquals(value.Blueprint, toggle));
            var buffs = buff == null ? new Kingmaker.UnitLogic.Buffs.Buff[0] : unit.Descriptor.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, buff)).ToArray();
            return new JObject { ["improvedSunder"] = unit.Descriptor.HasFact(improved), ["sunderAction"] = action != null && unit.Descriptor.HasFact(action),
                ["togglePresent"] = live != null, ["toggleOn"] = live != null && live.IsOn, ["toggleRunning"] = live != null && live.IsRunning,
                ["replacementBuffPresent"] = buffs.Length, ["replacementBuffActive"] = buffs.Count(value => value.Active && !value.IsSuppressed),
                ["brainBlueprint"] = unit.Blueprint.Brain?.AssetGuid,
                ["brainActions"] = unit.Brain?.Actions == null ? null : new JArray(unit.Brain.Actions.Where(value => value != null).Select(value => value.Blueprint?.name)),
                ["primaryWeapon"] = unit.Body.PrimaryHand.MaybeWeapon?.Blueprint?.name, ["providerToggleResolved"] = toggle != null };
        }

        private static bool MenuOrderSunderGone(JObject state)
        {
            return !(bool)state["improvedSunder"] && !(bool)state["sunderAction"] && !(bool)state["togglePresent"] &&
                (int)state["replacementBuffPresent"] == 0;
        }

        private IEnumerable<int> MenuOrderStormGiant(ExpandedSummoningCorrectionFixture fixture)
        {
            var symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var stormBlueprint = (BlueprintUnit)symbols[SummonPlaytestWithdrawalPolicy.For("storm-giant").UnitSymbol];
            UnitEntityData storm = CastExpandedSummoningOwnTier(fixture, "storm-giant");
            Sprint28SetBrainActivePreservingActions(storm, false);
            for (int frame = 0; frame < 30; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            JObject fresh = MenuOrderSunderState(storm);
            bool fixer = stormBlueprint.GetComponents<SummonWithdrawnCapabilityFixer>().Select(value => value.name)
                .SequenceEqual(new[] { SummonPlaytestWithdrawalPolicy.FixerName("storm-giant") });
            Sprint25Check("sunder-storm-fresh", MenuOrderSunderGone(fresh) && fixer && fresh["primaryWeapon"].Type != JTokenType.Null,
                new JObject { ["state"] = fresh, ["fixerAttached"] = fixer, ["source"] = "fixture cast of the published own-tier Summon Monster row" },
                "A fresh Storm Giant has no Improved Sunder, no Sunder action, no Sunder toggle and no replacement buff, and keeps its weapon.");
            CleanupExpandedSummoningUnit(storm); fixture.Created.Remove(storm);
            for (int frame = 0; frame < 2; frame++) { Game.Instance.IsPaused = false; yield return 0; }

            JObject routine = null; bool routinePass = false;
            Action<string, bool, JObject, string> capture = (id, pass, row, description) => { routine = row; routinePass = pass; };
            var routines = new JArray();
            JObject flanking = InfernalProperFlanking2State35();
            var maneuvers = new MenuOrderManeuverProbe(stormBlueprint);
            try
            {
                using (var trace = new InfernalEvents34())
                    foreach (int frame in SummonAdjacentRoutine4(fixture, "storm-giant", trace, capture, routines, flanking)) yield return frame;
            }
            finally { maneuvers.Dispose(); }
            int weaponAttacks = routine == null ? 0 : ((JArray)routine["ruleRows"] ?? new JArray()).OfType<JObject>().Count(value => (string)value["event"] == "weapon-did");
            int sunders = maneuvers.Rows.OfType<JObject>().Count(value => MenuOrderIsSunderManeuver((string)value["type"]));
            JArray replacements = routine?["attackReplacements"] as JArray ?? new JArray();
            bool sunderReplacement = replacements.OfType<JObject>().Any(value => MenuOrderIsSunderManeuver((string)value["maneuver"]) || ((string)value["buff"] ?? "").IndexOf("Sunder", StringComparison.OrdinalIgnoreCase) >= 0);
            Sprint25Check("sunder-storm-attack", routine != null && routine["setupFailure"] == null && weaponAttacks >= 1 && sunders == 0 && !sunderReplacement,
                new JObject { ["weaponAttacks"] = weaponAttacks, ["sunderManeuvers"] = sunders, ["maneuvers"] = maneuvers.Rows,
                    ["attackReplacementBuffs"] = replacements, ["properFlanking2"] = flanking,
                    ["routineVerdictFailure"] = routine?["verdictFailure"], ["routineSprint4VerdictPass"] = routinePass,
                    ["attackCount"] = routine?["attackCount"], ["replacedSlots"] = routine?["replacedSlots"], ["setupFailure"] = routine?["setupFailure"],
                    ["limit"] = "One native full-attack command on the adjacent durable hostile, observed passively; the Sprint 4 routine verdict is recorded, not asserted. A non-Sunder maneuver substitution (for example ProperFlanking2 Bull Rush) is recorded, not counted as Sunder." },
                "A fresh Storm Giant's native full attack resolves ordinary weapon attacks; no attack is converted into a Sunder maneuver and no Sunder replacement buff is active.");
        }

        private IEnumerable<int> MenuOrderLegacyCleanup(ExpandedSummoningCorrectionFixture fixture)
        {
            var library = BlueprintBootstrap.Library;
            var symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var improved = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SummonPlaytestWithdrawalPolicy.NativeImprovedSunderGuid, "native Improved Sunder");
            var sunderAi = BlueprintLibraryLookup.RequireExact<BlueprintAiCastSpell>(library, SummonPlaytestWithdrawalPolicy.NativeSunderArmorAiActionGuid, "native Sunder AI");
            var ordinaryBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library, SummonPlaytestWithdrawalPolicy.NativeDumbMonsterBrainGuid, "ordinary brain");
            Func<string, SummonWithdrawnCapabilityFixer> fixerOf = key => ((BlueprintUnit)symbols[SummonPlaytestWithdrawalPolicy.For(key).UnitSymbol])
                .GetComponents<SummonWithdrawnCapabilityFixer>().Single();
            var units = new List<UnitEntityData>();
            try
            {
                UnitEntityData storm = CastExpandedSummoningOwnTier(fixture, "storm-giant"); units.Add(storm);
                UnitEntityData earth = CastExpandedSummoningOwnTier(fixture, "greater-earth-elemental"); units.Add(earth);
                foreach (UnitEntityData unit in units) Sprint28SetBrainActivePreservingActions(unit, false);
                for (int frame = 0; frame < 20; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                JObject earthFresh = MenuOrderSunderState(earth);
                bool earthFreshOk = MenuOrderSunderGone(earthFresh) && ReferenceEquals(earth.Blueprint.Brain, ordinaryBrain) &&
                    earth.Brain.Actions.All(value => !ReferenceEquals(value?.Blueprint, sunderAi));
                Sprint25Check("sunder-earth-elemental-fresh", earthFreshOk, new JObject { ["state"] = earthFresh },
                    "A fresh Greater Earth Elemental has no Improved Sunder, no Sunder action or toggle, and the native ordinary-attack brain without the Sunder AI action.");

                // Induced pre-change state on fresh summons (not a cold load).
                storm.Descriptor.AddFact(improved);
                earth.Descriptor.AddFact(improved);
                earth.Brain.Actions.Add(new AiAction(sunderAi));
                int frames = 0;
                while (!((bool)MenuOrderSunderState(storm)["toggleRunning"] && (int)MenuOrderSunderState(storm)["replacementBuffActive"] >= 1) && frames < SummonPlaytestR1WaitFrames)
                { Game.Instance.IsPaused = false; frames++; yield return 0; }
                JObject stormBefore = MenuOrderSunderState(storm), earthBefore = MenuOrderSunderState(earth);
                // Non-summon control: the same feat on the disposable caster.
                fixture.Caster.Descriptor.AddFact(improved);
                fixerOf("storm-giant").OnPostLoad(fixture.Caster.Descriptor);
                fixerOf("greater-earth-elemental").OnPostLoad(fixture.Caster.Descriptor);
                bool casterKept = fixture.Caster.Descriptor.HasFact(improved);
                fixture.Caster.Descriptor.RemoveFact(improved);
                // A foreign fixer leaves the other summon alone.
                fixerOf("greater-earth-elemental").OnPostLoad(storm.Descriptor);
                bool foreignUntouched = storm.Descriptor.HasFact(improved);
                fixerOf("storm-giant").OnPostLoad(storm.Descriptor);
                fixerOf("greater-earth-elemental").OnPostLoad(earth.Descriptor);
                int removal = 0;
                while (!MenuOrderSunderGone(MenuOrderSunderState(storm)) && removal < SummonPlaytestR1WaitFrames)
                { Game.Instance.IsPaused = false; removal++; yield return 0; }
                JObject stormAfter = MenuOrderSunderState(storm), earthAfter = MenuOrderSunderState(earth);
                fixerOf("storm-giant").OnPostLoad(storm.Descriptor);
                fixerOf("greater-earth-elemental").OnPostLoad(earth.Descriptor);
                JObject stormRepeat = MenuOrderSunderState(storm), earthRepeat = MenuOrderSunderState(earth);
                bool stormInducedLive = (bool)stormBefore["improvedSunder"] && (bool)stormBefore["sunderAction"] &&
                    ((bool)stormBefore["toggleRunning"] && (int)stormBefore["replacementBuffActive"] >= 1 || !(bool)stormBefore["providerToggleResolved"]);
                bool earthInduced = (bool)earthBefore["improvedSunder"] && earth.Brain != null;
                bool earthClean = MenuOrderSunderGone(earthAfter) && earth.Brain.Actions.All(value => !ReferenceEquals(value?.Blueprint, sunderAi)) && earth.Brain.Actions.Count > 0;
                bool repeatStable = JToken.DeepEquals(stormAfter, stormRepeat) && JToken.DeepEquals(earthAfter, earthRepeat);
                Sprint25Check("sunder-legacy-cleanup", stormInducedLive && earthInduced && MenuOrderSunderGone(stormAfter) && earthClean && repeatStable &&
                    stormAfter["primaryWeapon"].Type != JTokenType.Null,
                    new JObject { ["stormBefore"] = stormBefore, ["stormAfter"] = stormAfter, ["stormRepeat"] = stormRepeat, ["stormWaitFrames"] = frames, ["removalFrames"] = removal,
                        ["earthBefore"] = earthBefore, ["earthAfter"] = earthAfter, ["earthRepeat"] = earthRepeat,
                        ["label"] = "Induced pre-change state on fresh summons; the real fixer's OnPostLoad called directly. No older save was cold-loaded." },
                    "The real fixers remove the induced Improved Sunder with its Sunder action, running toggle and active replacement buff from the Storm Giant, and the feat plus Sunder AI action from the Greater Earth Elemental, keeping the ordinary attack action and weapon; a repeat changes nothing.");
                Sprint25Check("sunder-fixer-exact-owner", casterKept && foreignUntouched, new JObject { ["nonSummonCasterKeptFeat"] = casterKept, ["foreignFixerLeftStormAlone"] = foreignUntouched },
                    "A non-summon unit holding Improved Sunder is untouched by the summon fixers, and one creature's fixer never touches another creature.");
            }
            finally
            {
                foreach (UnitEntityData unit in units) { CleanupExpandedSummoningUnit(unit); fixture.Created.Remove(unit); }
                if (fixture.Caster.Descriptor.HasFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SummonPlaytestWithdrawalPolicy.NativeImprovedSunderGuid, "native Improved Sunder")))
                    fixture.Caster.Descriptor.RemoveFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SummonPlaytestWithdrawalPolicy.NativeImprovedSunderGuid, "native Improved Sunder"));
            }
        }
    }
}
