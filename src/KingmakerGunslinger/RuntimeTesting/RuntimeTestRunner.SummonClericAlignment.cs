using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Save-free cleric alignment observation. A disposable caster gains real
        // cleric levels through native level-up, including the native
        // ApplySpellbook action. Deity facts, the Herald Caller conversion
        // feature and alignment changes are induced fixture state.
        private const string ClericSpellbookGuid = "4673d19a0cf2fab4f885cc4d1353da33";
        private const string ClericSpellListGuid = "8443ce803d2d31347897a3d85cc32f53";
        private const string ClericErastilGuid = "afc775188deb7a44aa4cbde03512c671";
        private const string ClericAsmodeusGuid = "a3a5ccc9c670e6f4ca4a686d23b89900";
        private const string ClericAbadarGuid = "6122dacf418611540a3c91e67197ee4e";
        private const string ClericHeraldForbiddenNativeRow = "e96593e67d206ab49ad1b567327d1e75";
        private static readonly string[] KnownThirdPartySummonMonsterRows =
        { "SummonMonsterIGiantCrabSpider", "SummonMonsterGiantBlackWidow", "SummonMonsterOgreSpider", "SummonMonsterGiantTarantula" };

        private static SummonClericAlignmentGate ClericGateOf(BlueprintAbility ability)
        { return ability == null ? null : (ability.ComponentsArray ?? new BlueprintComponent[0]).OfType<SummonClericAlignmentGate>().FirstOrDefault(); }

        private static SummonAlignmentAxes ClericLiveAxes(BlueprintAbility ability)
        {
            SpellDescriptorComponent component = (ability.ComponentsArray ?? new BlueprintComponent[0]).OfType<SpellDescriptorComponent>().FirstOrDefault();
            SpellDescriptor value = component == null ? SpellDescriptor.None : component.Descriptor.Value;
            SummonAlignmentAxes axes = SummonAlignmentAxes.None;
            if ((value & SpellDescriptor.Good) != 0) axes |= SummonAlignmentAxes.Good;
            if ((value & SpellDescriptor.Evil) != 0) axes |= SummonAlignmentAxes.Evil;
            if ((value & SpellDescriptor.Law) != 0) axes |= SummonAlignmentAxes.Law;
            if ((value & SpellDescriptor.Chaos) != 0) axes |= SummonAlignmentAxes.Chaos;
            return axes;
        }

        private void ClericCensus(BlueprintCharacterClass cleric)
        {
            var library = BlueprintBootstrap.Library;
            BlueprintSpellList list = cleric.Spellbook == null ? null : cleric.Spellbook.SpellList;
            bool identities = cleric.Spellbook != null && cleric.Spellbook.AssetGuid == ClericSpellbookGuid && list != null && list.AssetGuid == ClericSpellListGuid;
            var links = new JArray(); int linked = 0, children = 0, gated = 0; var unresolved = new JArray(); var verified = new JArray(); var mismatches = new JArray();
            for (int tier = 1; tier <= 9; tier++)
            {
                BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, ExpandedSummoningPublisher.CanonicalParentGuids[tier - 1], "cleric census parent");
                bool onList = list != null && list.GetLevel(parent) == tier;
                if (onList) linked++;
                BlueprintAbility[] rows = parent.Variants ?? new BlueprintAbility[0];
                links.Add(new JObject { ["tier"] = tier, ["parent"] = parent.AssetGuid, ["onClericList"] = onList, ["children"] = rows.Length });
                foreach (BlueprintAbility row in rows)
                {
                    children++;
                    SummonClericAlignmentGate gate = ClericGateOf(row);
                    if (gate == null)
                    {
                        int units = AlternateSummoningGraph.SpawnUnits(row).Count;
                        bool visibilityGate = (row.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityShowIfCasterHasFact>().Any();
                        SummonClericThirdPartyOutcome outcome = SummonClericObservationVerdict.ClassifyThirdParty(
                            KnownThirdPartySummonMonsterRows.Contains(row.name, StringComparer.Ordinal), ClericLiveAxes(row) == SummonAlignmentAxes.None, units, visibilityGate);
                        var entry = new JObject { ["tier"] = tier, ["ability"] = row.AssetGuid, ["name"] = row.Name, ["internalName"] = row.name, ["liveAlignmentDescriptors"] = ClericLiveAxes(row).ToString(),
                            ["spawnedUnits"] = units, ["hasVisibilityGate"] = visibilityGate, ["outcome"] = outcome.ToString() };
                        if (outcome == SummonClericThirdPartyOutcome.VerifiedLegal) verified.Add(entry); else unresolved.Add(entry);
                        continue;
                    }
                    gated++;
                    SummonSpellAlignment spell = SummonClericAlignmentCatalog.For(gate.CreatureKey, gate.MonsterTemplated);
                    SummonAlignmentAxes live = ClericLiveAxes(row);
                    if (!spell.CasterDependent && live != spell.Descriptors && mismatches.Count < 30)
                        mismatches.Add(new JObject { ["key"] = gate.CreatureKey, ["tier"] = tier, ["policy"] = spell.Descriptors.ToString(), ["blueprintDescriptors"] = live.ToString() });
                }
            }
            Sprint25Check("cleric-route-census", identities && linked == 9 && unresolved.Count == 0, new JObject {
                ["clericClass"] = cleric.AssetGuid, ["spellbook"] = cleric.Spellbook?.AssetGuid, ["spellList"] = list?.AssetGuid, ["identitiesExact"] = identities, ["linkedParents"] = linked, ["links"] = links,
                ["children"] = children, ["carryingTheNewGate"] = gated, ["coveredByVerifiedEquivalentContract"] = verified, ["unresolved"] = unresolved, ["policyVersusBlueprintDescriptorDifferences"] = mismatches,
                ["scope"] = "Structural census of the real cleric spell list and published children." },
                "All nine Summon Monster parents are on the actual cleric spell list, and every child carries the gate or has a verified equivalent contract.");
            BlueprintFeatureSelection deities = BlueprintLibraryLookup.RequireExact<BlueprintFeatureSelection>(library, SummonClericAlignmentPolicy.DeitySelectionGuid, "deity selection");
            SummonAlignmentAxes ignored;
            string[] unmapped = deities.AllFeatures.Where(value => value != null && !SummonClericDeityCatalog.TryGetAxes(value.AssetGuid, out ignored)).Select(value => value.name + ":" + value.AssetGuid).ToArray();
            Sprint25Check("cleric-deity-census", unmapped.Length == 0, new JObject { ["options"] = deities.AllFeatures.Length, ["unmapped"] = new JArray(unmapped) },
                "Every installed deity selection option has an exact mapped alignment.");
            BlueprintScriptableObject heraldObject; bool hasHerald = library.BlueprintsByAssetId.TryGetValue(DruidSummonConversionPolicy.HeraldCallerFeatureGuid, out heraldObject);
            BlueprintFeature herald = heraldObject as BlueprintFeature;
            var routes = herald == null ? new SpontaneousSpellConversion[0] : (herald.ComponentsArray ?? new BlueprintComponent[0]).OfType<SpontaneousSpellConversion>().ToArray();
            string[] converted = routes.SelectMany(value => value.SpellsByLevel ?? new BlueprintAbility[0]).Where(value => value != null).Select(value => value.AssetGuid).Distinct().ToArray();
            var conversions = converted.Select(guid => {
                int tier; SummonMultiplicity quantity;
                bool resolved = DruidSummonConversionPolicy.TryResolve(DruidSummonConversionPolicy.Herald, guid, out tier, out quantity);
                bool projects = resolved && ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Any(value => value.ParentTier == tier && value.Multiplicity == quantity && SummonVisibilityCatalog.IsPublished(value));
                BlueprintScriptableObject found;
                bool governed = library.BlueprintsByAssetId.TryGetValue(guid, out found) && found is BlueprintAbility && SummonClericAlignmentRuntime.GovernedNativeRows().ContainsKey((BlueprintAbility)found);
                return new SummonClericObservationVerdict.HeraldConversion { Guid = guid, SharedParent = guid == DruidSummonConversionPolicy.HeraldTierOneParentGuid, Projects = projects, ExecutionGoverned = governed };
            }).ToArray();
            IReadOnlyList<string> uncovered = SummonClericObservationVerdict.UncoveredHeraldConversions(conversions);
            Sprint25Check("cleric-herald-route-identified", hasHerald && routes.Length == 3 && conversions.Length == 24 && DruidSummonConversionVariantPatch.ExactNativeFeature(herald, cleric),
                new JObject { ["installed"] = hasHerald, ["components"] = routes.Length, ["conversions"] = conversions.Length, ["exactNativeFeature"] = herald != null && DruidSummonConversionVariantPatch.ExactNativeFeature(herald, cleric) },
                "The exact installed Herald Caller feature has its 24 tier and quantity conversions for this class.");
            Sprint25Check("cleric-herald-enforcement-coverage", uncovered.Count == 0 && conversions.Length == 24, new JObject { ["uncovered"] = new JArray(uncovered), ["sharedParents"] = conversions.Count(value => value.SharedParent),
                ["projectedAndGoverned"] = conversions.Count(value => value.Projects && value.ExecutionGoverned) },
                "Every governed Herald conversion either uses a shared gated parent or projects onto gated children with its original native row execution-governed.");
        }

        private IEnumerable<int> ClericOpenMenu(string label, ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, int tier, Spellbook book,
            BlueprintAbility parent, AbilityData original, Cleanup2NativeDefaultScope mode, Stopwatch watch, NativeIconScreenEvidence screens,
            Action<AlphaMenu, ActionBarGroupSlot> result, bool closeAfter)
        {
            Cleanup2MenuAdapterBoundary(mode, watch);
            if (!spellGroup.ToggleState) spellGroup.OnClick();
            spellGroup.Fill(Game.Instance.UI.SelectionManagerPC.SelectedUnits.Single());
            levels.OnClick(tier);
            for (int frame = 0; frame < 12; frame++) yield return 0;
            ActionBarGroupSlot source = spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value =>
                value.gameObject.activeInHierarchy && ReferenceEquals(value.MechanicSlot?.GetContentData(), original));
            foreach (int frame in Cleanup2AwaitStationaryNativeHud(source, "cleric-" + label)) yield return frame;
            source.OnToggleGroupClick();
            for (int frame = 0; frame < 10; frame++) yield return 0;
            AlphaMenu menu = AlphaReadPopup(source);
            try
            {
                BlueprintAbility[] rows = menu.Data.Select(value => value?.Blueprint).ToArray();
                BlueprintAbility[] variants = parent.Variants ?? new BlueprintAbility[0];
                BlueprintAbility[] expected = variants.Where(value => new AbilityData(value, book).IsVisible() && SummonClericAlignmentRuntime.PermitsRow(value, book.Owner, book)).ToArray();
                bool exact = rows.SequenceEqual(expected) && rows.Distinct().Count() == rows.Length;
                Sprint25Check("cleric-menu-" + label, exact, new JObject { ["menu"] = label, ["parent"] = parent.AssetGuid, ["published"] = variants.Length, ["expected"] = expected.Length,
                    ["rows"] = AlphaRowsEvidence(menu), ["hiddenForThisCaster"] = new JArray(variants.Except(expected).Select(value => value.Name)) },
                    "The real popup lists exactly this caster's permitted rows, in the published alphabetical order, with no duplicates.");
                result(menu, source);
                if (screens != null)
                    foreach (int frame in screens.Capture("cleric-" + label, new JObject { ["menu"] = label, ["rows"] = rows.Length },
                        () => mode.Current && Cleanup2NoNarrativeOverlay() && menu.Popup.gameObject.activeInHierarchy)) yield return frame;
            }
            finally { if (closeAfter) Cleanup2MenuAdapterHide(source, original); }
        }

        // A forbidden row built the way a saved shortcut holds it, through the
        // native availability and command path. No tick or outcome is supplied.
        private IEnumerable<int> ClericForbiddenAttempt(ExpandedSummoningCorrectionFixture fixture, BlueprintAbility child, Spellbook book, AbilityData prepared,
            Cleanup2NativeDefaultScope mode, Stopwatch phase, string caseKey, bool visibleExpected)
        {
            var data = new AbilityData(child, book) { ConvertedFrom = prepared };
            var debit = new Cleanup2SourceDebit(fixture.Caster);
            bool visible = data.IsVisible(), available = data.IsAvailableForCast;
            string reason = data.GetUnavailableReason();
            var hooks = HarmonyInstance.Create("kmg.cleric.forbidden." + caseKey + _request.RunId); UnitUseAbility command = null;
            var watch = Stopwatch.StartNew(); string result;
            try
            {
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"), new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", BindingFlags.Static | BindingFlags.NonPublic)), null, null);
                ExpandedSummoningRuleCapture.Clear(); _expandedSummoningPlayerPathSpawnActionCount = 0; _expandedSummoningPlayerPathCaptureActive = true;
                command = new UnitUseAbility(data, new TargetWrapper(fixture.Caster.Position + new Vector3(2, 0, 2)));
                fixture.Caster.Commands.Run(command);
                while (!command.IsFinished && watch.Elapsed.TotalSeconds < 15 && phase.Elapsed.TotalSeconds < 240)
                {
                    if (!mode.Current || !Cleanup2NoNarrativeOverlay()) throw new InvalidOperationException("Forbidden attempt lost native mode ownership.");
                    Game.Instance.IsPaused = false; yield return 0;
                }
                result = command.IsFinished ? command.Result.ToString() : "unfinished";
            }
            finally
            {
                _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id);
                if (command != null && !command.IsFinished) command.Interrupt(true);
            }
            for (int frame = 0; frame < 6; frame++) yield return 0;
            var summons = ExpandedSummoningRuleCapture.Where(value => value != null).Distinct().ToArray();
            foreach (var unit in summons) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
            if (summons.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, summons);
            bool refused = visible == visibleExpected && !available && result == "Fail" && debit.Unspent && _expandedSummoningPlayerPathSpawnActionCount == 0 && summons.Length == 0;
            Sprint25Check("cleric-forbidden-attempt-" + caseKey, refused, new JObject { ["child"] = child.AssetGuid, ["name"] = child.Name, ["visible"] = visible, ["availableForCast"] = available,
                ["nativeReason"] = reason, ["commandResult"] = result, ["slotOrResourceSpent"] = !debit.Unspent, ["spawnActions"] = _expandedSummoningPlayerPathSpawnActionCount, ["summons"] = summons.Length,
                ["debit"] = debit.Describe(prepared.ParamSpellSlot), ["route"] = "AbilityData built as a saved shortcut holds it; native UnitUseAbility; no manual tick or forced act", ["wallSeconds"] = watch.Elapsed.TotalSeconds },
                "A forbidden selected choice is refused before any slot or resource is spent, and nothing is summoned.");
        }

        private IEnumerable<int> ClericLegalCast(ExpandedSummoningCorrectionFixture fixture, ActionBarSlot row, AbilityData child, AbilityData prepared, Cleanup2SourceDebit debit, Action closePopup, Cleanup2NativeDefaultScope mode, Stopwatch phase)
        {
            var hooks = HarmonyInstance.Create("kmg.cleric.legal." + _request.RunId); UnitUseAbility command = null;
            const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
            try
            {
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null, new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("Cleanup2DruidCastRulePostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"), new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningPlayerPathEvents.Clear(); Cleanup2DruidCastRules.Clear();
                _expandedSummoningPlayerPathRuleCastCount = 0; _expandedSummoningPlayerPathSpawnActionCount = 0;
                _cleanup2DruidCastExpected = child; _expandedSummoningPlayerPathCaptureActive = true;
                row.OnClick();
                bool selected = ReferenceEquals(Game.Instance.SelectedAbilityHandler.Ability, child) && debit.Unspent && fixture.Caster.Commands.Empty;
                if (selected) closePopup();
                bool targeted = selected && Game.Instance.SelectedAbilityHandler.OnClick(null, fixture.Caster.Position + new Vector3(2, 0, 2), 0);
                command = fixture.Caster.Commands.GetCommand<UnitUseAbility>();
                bool issued = targeted && command != null && ReferenceEquals(command.Spell, child);
                var watch = Stopwatch.StartNew();
                while (issued && watch.Elapsed.TotalSeconds < 25 && phase.Elapsed.TotalSeconds < 240 && (_expandedSummoningPlayerPathRuleCastCount == 0 || !fixture.Caster.Commands.Empty))
                {
                    if (!mode.Current || !Cleanup2NoNarrativeOverlay() || Game.Instance.IsControllerGamepad) throw new InvalidOperationException("Native cleric cast lost input ownership.");
                    Game.Instance.IsPaused = false; yield return 0;
                }
                var units = ExpandedSummoningRuleCapture.Where(value => value != null && ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster)).Distinct().ToArray();
                foreach (var unit in units) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                var expected = SummonSignatureReader.Read(child.Blueprint).Spawns().ToArray();
                bool exact = issued && command.IsFinished && command.Result.ToString() == "Success" && fixture.Caster.Commands.Empty && Cleanup2DruidCastRules.Count == 1 && Cleanup2DruidCastRules[0].Success &&
                    ReferenceEquals(Cleanup2DruidCastRules[0].Spell, child) && _expandedSummoningPlayerPathRuleCastCount == 1 && _expandedSummoningPlayerPathSpawnActionCount == 1 &&
                    expected.Length == 1 && units.Length == 1 && units[0].Blueprint.AssetGuid == expected[0].Unit && debit.ExactlyOne(prepared.ParamSpellSlot) &&
                    ReferenceEquals(child.ConvertedFrom, prepared) && ReferenceEquals(child.Spellbook, prepared.Spellbook);
                Sprint25Check("cleric-legal-cast", exact, new JObject { ["selected"] = selected, ["child"] = child.Blueprint.AssetGuid, ["name"] = child.Name, ["source"] = prepared.Blueprint.AssetGuid,
                    ["commandResult"] = command == null ? null : command.Result.ToString(), ["castRules"] = Cleanup2DruidCastRules.Count, ["castCount"] = _expandedSummoningPlayerPathRuleCastCount,
                    ["spawnActions"] = _expandedSummoningPlayerPathSpawnActionCount, ["units"] = new JArray(units.Select(value => value.Blueprint.AssetGuid)), ["expectedUnit"] = expected.FirstOrDefault()?.Unit, ["quantity"] = units.Length,
                    ["debit"] = debit.Describe(prepared.ParamSpellSlot), ["manualTicks"] = 0, ["forcedAct"] = false, ["wallSeconds"] = watch.Elapsed.TotalSeconds },
                    "Selecting a permitted row from the cleric's own menu casts that ability, summons its creature, and spends only its prepared slot.");
                if (units.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, units);
            }
            finally
            {
                _cleanup2DruidCastExpected = null; _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id); Cleanup2DruidCastRules.Clear();
                if (ReferenceEquals(Game.Instance.SelectedAbilityHandler?.Ability, child)) Game.Instance.SelectedAbilityHandler.DropAbility();
                if (command != null && !command.IsFinished) command.Interrupt(true);
            }
            for (int frame = 0; frame < 12; frame++) yield return 0;
        }

        private static bool ClericDeityAdmits(BlueprintFeature deity, Alignment alignment)
        {
            PrerequisiteAlignment[] gates = (deity.ComponentsArray ?? new BlueprintComponent[0]).OfType<PrerequisiteAlignment>().ToArray();
            return gates.Length == 0 || gates.Any(value => (value.Alignment & alignment.ToMask()) != 0);
        }

        private void ClericNonClericControl(BlueprintAbility forbidden, BlueprintAbility spiderRow, UnitEntityData anchor, BlueprintFeature deity)
        {
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "b3a505fb61437dc4097f43c3f8f9a4cf", "non-cleric control class");
            var owner = new TeleportResourceFixtureOwner(anchor);
            var fact = anchor.Descriptor.AddFact(deity);
            try
            {
                Spellbook book = owner.AddBook(sorcerer.Spellbook);
                var data = new AbilityData(forbidden, book);
                SummonClericFacts facts = SummonClericAlignmentRuntime.Read(anchor.Descriptor);
                bool spiderHidden = spiderRow == null || !new AbilityData(spiderRow, book).IsVisible();
                Sprint25Check("cleric-non-cleric-control", !SummonClericAlignmentPolicy.IsCleric(facts) && data.IsVisible() && ClericGateOf(forbidden).IsAvailableFor(data) && spiderHidden, new JObject {
                    ["classes"] = new JArray(facts.ClassGuids), ["deityFact"] = deity.AssetGuid, ["ability"] = forbidden.AssetGuid, ["visible"] = data.IsVisible(), ["spiderRowHiddenWithoutItsFeature"] = spiderHidden,
                    ["scope"] = "Provider-level control on a real non-cleric unit with a deity fact and a Sorcerer book; a popup was not opened for this unit." },
                    "Holding a deity fact without cleric levels is unrestricted, and the third-party row keeps its own visibility requirement.");
            }
            finally { owner.Restore(); anchor.Descriptor.RemoveFact(fact); }
        }

        private IEnumerable<int> ReviewSummonClericAlignment(ExpandedSummoningCorrectionFixture fixture)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) || fixture?.Caster == null || !_context.FeatureModules.Active.ExpandedSummoning)
                throw new InvalidOperationException("Exact fresh cleric alignment fixture with the summoning module enabled required.");
            var library = BlueprintBootstrap.Library;
            var game = Game.Instance; var player = game.Player; var ui = game.UI; var group = GroupController.Instance; var bar = ActionBarManager.Instance;
            if (group == null || bar?.Group == null || ui?.SelectionManagerPC == null || game.IsControllerGamepad) throw new InvalidOperationException("Actual desktop native action bar required.");
            var cleric = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, SummonClericAlignmentPolicy.ClericClassGuid, "native cleric class");
            Func<string, BlueprintFeature> feature = guid => BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, guid, "cleric fixture feature");
            BlueprintFeature erastil = feature(ClericErastilGuid), asmodeus = feature(ClericAsmodeusGuid), abadar = feature(ClericAbadarGuid), herald = feature(DruidSummonConversionPolicy.HeraldCallerFeatureGuid);
            Func<int, BlueprintAbility> smParent = tier => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, ExpandedSummoningPublisher.CanonicalParentGuids[tier - 1], "Summon Monster parent");
            Func<string, int, SummonVariantSpec> spec = (key, tier) => ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Single(value =>
                value.Creature.Key == key && value.ParentTier == tier && value.Multiplicity == SummonMultiplicity.One);
            var party = player.Party.ToArray(); var selected = ui.SelectionManagerPC.SelectedUnits.ToArray(); var selectedGroup = group.GetCurrentCharacter();
            var groups = bar.Group.GroupElements.ToArray(); bool[] toggles = groups.Select(value => value.ToggleState).ToArray();
            object levelController = null; bool added = false; NativeIconScreenEvidence screens = null; Cleanup2NativeDefaultScope mode = null;
            Spellbook book = null; AbilityData tierThree = null, tierFive = null, tierFiveOrdinary = null, tierNine = null; Feature deityFact = null, heraldFact = null;
            var watch = Stopwatch.StartNew();
            Action<BlueprintFeature> setDeity = deity => { if (deityFact != null) fixture.Caster.Descriptor.RemoveFact(deityFact); deityFact = fixture.Caster.Descriptor.AddFact(deity) as Feature; };
            try
            {
                fixture.Caster.Stats.Wisdom.BaseValue = 30;
                AdvanceDisposableSpellcaster(fixture.Caster.Descriptor, cleric, 20, ref levelController, true);
                ClassData classData = fixture.Caster.Descriptor.Progression.GetClassData(cleric);
                book = fixture.Caster.Descriptor.GetSpellbook(cleric);
                if (book != null) book.UpdateAllSlotsSize(false);
                fixture.Caster.Descriptor.Alignment.Set(Alignment.LawfulGood);
                setDeity(erastil);
                var ordinary = cleric.Spellbook.SpellList.GetSpells(5).Where(value => value != null && !ReferenceEquals(value, smParent(5)) &&
                    (value.Variants == null || value.Variants.Length == 0)).OrderBy(value => value.AssetGuid, StringComparer.Ordinal).First();
                if (book != null)
                {
                    PrepareExpandedSummoningPlayerPathSpell(book, smParent(3), smParent(3), 3);
                    PrepareExpandedSummoningPlayerPathSpell(book, smParent(5), smParent(5), 5);
                    PrepareExpandedSummoningPlayerPathSpell(book, ordinary, ordinary, 5);
                    PrepareExpandedSummoningPlayerPathSpell(book, smParent(9), smParent(9), 9);
                    book.Rest();
                }
                Func<int, BlueprintAbility, AbilityData> preparedSpell = (level, spell) => book == null ? null :
                    book.GetMemorizedSpellSlots(level).Where(value => value.Spell != null && ReferenceEquals(value.Spell.Blueprint, spell)).Select(value => value.Spell).FirstOrDefault();
                tierThree = preparedSpell(3, smParent(3)); tierFive = preparedSpell(5, smParent(5)); tierFiveOrdinary = preparedSpell(5, ordinary); tierNine = preparedSpell(9, smParent(9));
                var problems = SummonClericObservationVerdict.FixtureProblems(new SummonClericObservationVerdict.FixtureReading {
                    ClassLevel = classData == null ? 0 : classData.Level, ExpectedLevel = 20, ClassBook = classData?.Spellbook?.AssetGuid,
                    RegisteredBooks = classData?.Spellbook == null ? 0 : fixture.Caster.Descriptor.Spellbooks.Count(value => ReferenceEquals(value.Blueprint, classData.Spellbook)),
                    RegisteredBook = book?.Blueprint.AssetGuid, BookIsClassBook = book != null && classData?.Spellbook != null && ReferenceEquals(book.Blueprint, classData.Spellbook),
                    BookList = book?.Blueprint.SpellList?.AssetGuid, ExpectedList = ClericSpellListGuid, PreparedSlot = tierThree != null && tierFive != null && tierFiveOrdinary != null && tierNine != null &&
                        tierThree.ParamSpellSlot != null && tierFiveOrdinary.ParamSpellSlot != null && tierNine.ParamSpellSlot != null,
                    DeityMatchesPrerequisite = ClericDeityAdmits(erastil, Alignment.LawfulGood),
                    AlignmentIsExact = fixture.Caster.Descriptor.Alignment.Value == Alignment.LawfulGood });
                Sprint25Check("cleric-fixture-prerequisites", problems.Count == 0, new JObject { ["problems"] = new JArray(problems), ["classLevel"] = classData?.Level ?? 0, ["classBook"] = classData?.Spellbook?.AssetGuid,
                    ["spellbooks"] = new JArray(fixture.Caster.Descriptor.Spellbooks.Select(value => value.Blueprint.AssetGuid)), ["casterLevel"] = book?.CasterLevel ?? 0, ["spellList"] = book?.Blueprint.SpellList?.AssetGuid,
                    ["authority"] = "Native level-up with the native ApplySpellbook action; deity and alignment are induced on the disposable caster." },
                    "The caster is a real cleric with the class data's own book, its cleric list, valid prepared slots and a valid deity and alignment state.");
                if (problems.Count != 0) throw new InvalidOperationException("Cleric fixture prerequisites failed: " + string.Join(",", problems.ToArray()));
                ClericCensus(cleric);
                if (!player.Party.Contains(fixture.Caster)) { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                mode = Cleanup2EnterNativeDefault(fixture, "summon-cleric-alignment");
                screens = new NativeIconScreenEvidence(_request);
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                if (!mode.Current) throw new InvalidOperationException("Native Default did not settle after cleric setup.");
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement).GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup);
                BlueprintAbility lantern = ExpandedSummoningRoot(fixture.Blueprints, spec("lantern-archon", 3));
                BlueprintAbility iceDevil = ExpandedSummoningRoot(fixture.Blueprints, spec("ice-devil", 9));
                BlueprintAbility astral = ExpandedSummoningRoot(fixture.Blueprints, spec("astral-deva", 9));

                AlphaMenu nineLawfulGood = null;
                foreach (int frame in R6Cell("cleric-menu-nine-lg", () => ClericOpenMenu("lawful-good-erastil-tier9", spellGroup, levels, 9, book, smParent(9), tierNine, mode, watch, screens, (menu, source) => nineLawfulGood = menu, true))) yield return frame;
                if (nineLawfulGood != null)
                {
                    string[] names = nineLawfulGood.Data.Select(value => value.Name).ToArray();
                    Sprint25Check("cleric-menu-filter-expectations", names.Any(value => value.StartsWith("Archon, ", StringComparison.Ordinal)) && names.Any(value => value.StartsWith("Deva, Astral", StringComparison.Ordinal)) &&
                        !names.Any(value => value.StartsWith("Demon, ", StringComparison.Ordinal) || value.StartsWith("Devil, ", StringComparison.Ordinal) || value.StartsWith("Azata, ", StringComparison.Ordinal)),
                        new JObject { ["rows"] = names.Length, ["first"] = names.FirstOrDefault(), ["last"] = names.LastOrDefault() },
                        "A lawful good cleric of a lawful good deity keeps archons and the Good-only Astral Deva, and loses demons, devils and chaotic azatas.");
                }

                AlphaMenu threeLawfulGood = null; ActionBarGroupSlot threeSource = null;
                foreach (int frame in R6Cell("cleric-menu-three-lg", () => ClericOpenMenu("lawful-good-erastil-tier3", spellGroup, levels, 3, book, smParent(3), tierThree, mode, watch, screens, (menu, source) => { threeLawfulGood = menu; threeSource = source; }, false))) yield return frame;
                if (threeLawfulGood != null && threeSource != null)
                {
                    int index = Array.FindIndex(threeLawfulGood.Data, value => ReferenceEquals(value?.Blueprint, lantern));
                    if (index >= 0)
                    {
                        var debit = new Cleanup2SourceDebit(fixture.Caster);
                        ActionBarGroupSlot castSource = threeSource;
                        foreach (int frame in R6Cell("cleric-legal", () => ClericLegalCast(fixture, threeLawfulGood.Rows[index], threeLawfulGood.Data[index], tierThree, debit, () => Cleanup2MenuAdapterHide(castSource, tierThree), mode, watch))) yield return frame;
                    }
                    else Sprint25Check("cleric-legal-row-present", false, new JObject { ["rows"] = threeLawfulGood.Rows.Length }, "The legal Lantern Archon is offered.");
                    Cleanup2MenuAdapterHide(threeSource, tierThree);
                }
                // A forbidden child of an unspent parent, as a saved shortcut holds it.
                foreach (int frame in R6Cell("cleric-forbidden-child", () => ClericForbiddenAttempt(fixture, iceDevil, book, tierNine, mode, watch, "selected-child", false))) yield return frame;

                // Herald Caller: the exact native conversion feature on the real cleric.
                heraldFact = fixture.Caster.Descriptor.AddFact(herald) as Feature;
                Sprint25Check("cleric-herald-fixture", heraldFact != null && DruidSummonConversionVariantPatch.ExactNativeFeature(herald, cleric),
                    new JObject { ["feature"] = herald.AssetGuid, ["scope"] = "Exact native conversion feature granted to the disposable cleric, as the established Druid route was; not a naturally levelled archetype." },
                    "The exact Herald Caller conversion feature is active on the real cleric.");
                AlphaMenu heraldMenu = null; ActionBarGroupSlot heraldSource = null;
                foreach (int frame in R6Cell("cleric-herald-popup", () => ClericOpenMenu("herald-conversion-tier5", spellGroup, levels, 5, book, smParent(5), tierFiveOrdinary, mode, watch, screens, (menu, source) => { heraldMenu = menu; heraldSource = source; }, false))) yield return frame;
                if (heraldMenu != null && heraldSource != null)
                {
                    AbilityData[] heraldRows = heraldMenu.Data;
                    bool projected = heraldRows.All(value => value.ConvertedFrom != null && ReferenceEquals(value.ConvertedFrom.ConvertedFrom, tierFiveOrdinary)) && heraldRows.All(value => ReferenceEquals(value.Spellbook, book));
                    Sprint25Check("cleric-herald-projection", projected && heraldRows.Length > 0 && heraldRows.All(value => DruidSummonConversionVariantPatch.For(value) != null), new JObject {
                        ["rows"] = heraldRows.Length, ["conversionAdapter"] = DruidSummonConversionVariantPatch.Outcome },
                        "Each conversion row is a published child, preserved as child to native quantity conversion to the original prepared spell.");
                    int legal = Array.FindLastIndex(heraldRows, value => AlphaQuantityRank(value.Name) == 0 && SummonSignatureReader.Read(value.Blueprint).Spawns().Count() == 1);
                    if (legal >= 0)
                    {
                        var heraldDebit = new Cleanup2SourceDebit(fixture.Caster);
                        ActionBarGroupSlot closeSource = heraldSource;
                        foreach (int frame in R6Cell("cleric-herald-cast", () => Cleanup2MenuAdapterDruidCast(fixture, heraldRows[legal], tierFiveOrdinary, smParent(5), heraldMenu.Rows[legal], heraldDebit, mode, watch,
                            () => Cleanup2MenuAdapterHide(closeSource, tierFiveOrdinary), "herald-legal"))) yield return frame;
                    }
                    else Sprint25Check("cleric-herald-legal-row-present", false, new JObject { ["rows"] = heraldRows.Length }, "A single legal conversion row exists.");
                    Cleanup2MenuAdapterHide(heraldSource, tierFiveOrdinary);
                }
                // A retained original native conversion: its native spawn here is the Ghaele, forbidden to this lawful cleric.
                BlueprintAbility nativeRow = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, ClericHeraldForbiddenNativeRow, "retained native Herald conversion");
                foreach (int frame in R6Cell("cleric-herald-forbidden", () => ClericForbiddenAttempt(fixture, nativeRow, book, tierNine, mode, watch, "retained-native-conversion", true))) yield return frame;
                AlphaMenu heraldParent = null;
                foreach (int frame in R6Cell("cleric-herald-prepared-parent", () => ClericOpenMenu("herald-prepared-parent-tier9", spellGroup, levels, 9, book, smParent(9), tierNine, mode, watch, screens, (menu, source) => heraldParent = menu, true))) yield return frame;
                Sprint25Check("cleric-herald-prepared-parent-no-duplicates", heraldParent != null && heraldParent.Data.Select(value => value.Blueprint).Distinct().Count() == heraldParent.Data.Length,
                    new JObject { ["rows"] = heraldParent?.Rows.Length ?? 0, ["conversionAdapter"] = DruidSummonConversionVariantPatch.Outcome }, "A prepared Summon Monster with the Herald feature lists each creature once.");
                if (heraldFact != null) { fixture.Caster.Descriptor.RemoveFact(heraldFact); heraldFact = null; }

                // Induced valid pairings at one character alignment.
                fixture.Caster.Descriptor.Alignment.Set(Alignment.LawfulNeutral);
                setDeity(abadar);
                AbilityData heldAstral = new AbilityData(astral, book) { ConvertedFrom = tierNine };
                bool controlVisible = heldAstral.IsVisible(), controlAvailable = heldAstral.IsAvailableForCast;
                AlphaMenu abadarMenu = null;
                foreach (int frame in R6Cell("cleric-menu-nine-abadar", () => ClericOpenMenu("lawful-neutral-abadar-tier9", spellGroup, levels, 9, book, smParent(9), tierNine, mode, watch, screens, (menu, source) => abadarMenu = menu, true))) yield return frame;
                bool controlInMenu = abadarMenu != null && abadarMenu.Data.Any(value => ReferenceEquals(value.Blueprint, astral));
                setDeity(asmodeus);
                bool heldVisible = heldAstral.IsVisible(), heldAvailable = heldAstral.IsAvailableForCast;
                AlphaMenu asmodeusMenu = null;
                foreach (int frame in R6Cell("cleric-menu-nine-asmodeus", () => ClericOpenMenu("lawful-neutral-asmodeus-tier9", spellGroup, levels, 9, book, smParent(9), tierNine, mode, watch, screens, (menu, source) => asmodeusMenu = menu, true))) yield return frame;
                bool absentForDeity = asmodeusMenu != null && !asmodeusMenu.Data.Any(value => ReferenceEquals(value.Blueprint, astral));
                bool pairingsAdmitted = ClericDeityAdmits(asmodeus, Alignment.LawfulNeutral) && ClericDeityAdmits(abadar, Alignment.LawfulNeutral);
                Sprint25Check("cleric-deity-derived-restriction", pairingsAdmitted && controlVisible && controlAvailable && controlInMenu && !heldVisible && !heldAvailable && absentForDeity, new JObject {
                    ["nativeDeityPrerequisitesAdmitPairings"] = pairingsAdmitted, ["casterAlignment"] = "LawfulNeutral", ["controlDeity"] = abadar.AssetGuid, ["controlVisible"] = controlVisible, ["controlAvailable"] = controlAvailable, ["astralInControlMenu"] = controlInMenu,
                    ["restrictingDeity"] = asmodeus.AssetGuid, ["heldVisibleAfterChange"] = heldVisible, ["heldAvailableAfterChange"] = heldAvailable, ["astralAbsentForDeity"] = absentForDeity,
                    ["state"] = "Induced deity change on the fixture; not an old save or a cold load." },
                    "A lawful neutral cleric may summon the Good-only Astral Deva under a lawful neutral deity, and loses it, including an already held choice, under a lawful evil deity.");

                BlueprintAbility spiderRow = (smParent(5).Variants ?? new BlueprintAbility[0]).FirstOrDefault(value => value.name == "SummonMonsterOgreSpider");
                ClericNonClericControl(iceDevil, spiderRow, player.MainCharacter.Value, asmodeus);
                AbilityShowIfCasterHasFact spiderGate = spiderRow == null ? null : (spiderRow.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityShowIfCasterHasFact>().FirstOrDefault();
                if (spiderGate != null && spiderGate.UnitFact is BlueprintFeature)
                {
                    var spiderFact = fixture.Caster.Descriptor.AddFact((BlueprintFeature)spiderGate.UnitFact);
                    AlphaMenu spiderMenu = null;
                    foreach (int frame in R6Cell("cleric-menu-five-spider", () => ClericOpenMenu("spider-summoner-feature-tier5", spellGroup, levels, 5, book, smParent(5), tierFive, mode, watch, screens, (menu, source) => spiderMenu = menu, true))) yield return frame;
                    int spider = spiderMenu == null ? -1 : Array.FindIndex(spiderMenu.Data, value => ReferenceEquals(value.Blueprint, spiderRow));
                    Sprint25Check("cleric-third-party-visible-with-feature", spider >= 0, new JObject { ["index"] = spider, ["rows"] = spiderMenu?.Rows.Length ?? 0, ["label"] = spider < 0 ? null : spiderMenu.Data[spider].Name },
                        "With its own feature, the third-party row appears in the cleric's sorted menu.");
                    fixture.Caster.Descriptor.RemoveFact(spiderFact);
                }
                Sprint25Check("cleric-surface-limits", true, new JObject { ["openedInGame"] = "Prepared Summon Monster III, V and IX popups and a Herald conversion popup on a levelled cleric.",
                    ["execution"] = "Legal casts and forbidden commands, unassisted, under the fixture's induced state.",
                    ["notObserved"] = "Console navigation, saved-hotbar reload, cold load, autonomous AI, a multiclass book popup.", ["wallSeconds"] = watch.Elapsed.TotalSeconds },
                    "Observed and unobserved surfaces are reported separately.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                if (levelController != null) restore("level-controller", () => levelController.GetType().GetMethod("Cancel").Invoke(levelController, null));
                restore("herald-fact", () => { if (heraldFact != null) fixture.Caster.Descriptor.RemoveFact(heraldFact); });
                restore("party", () => { if (added) { player.PartyCharacters.Remove(fixture.Caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); } });
                restore("selection", () => { group.SelectUnit(selectedGroup); ui.SelectionManagerPC.MultiSelect(selected.Where(value => value?.View != null).Select(value => value.View).ToArray(), false); });
                for (int index = 0; index < groups.Length; index++) { int scoped = index; restore("toggle-" + scoped, () => groups[scoped].Toggle(toggles[scoped], true)); }
                restore("screens", () => screens?.Dispose());
                restore("native-default", () => mode?.Dispose());
                Sprint25Check("cleric-owned-state-restored", errors.Count == 0 && player.Party.SequenceEqual(party), new JObject { ["errors"] = new JArray(errors), ["caster"] = "disposable; its levels, book and facts end with the fixture" },
                    "Party, selection and native UI are restored; the disposable caster is removed by the fixture.");
            }
        }
    }
}
