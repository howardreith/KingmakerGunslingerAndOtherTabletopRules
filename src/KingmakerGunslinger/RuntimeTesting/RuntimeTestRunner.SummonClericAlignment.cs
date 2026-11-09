using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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
        // cleric levels through the native level-up route. Deity facts and
        // alignment are induced fixture state, not an old save or a cold load.
        private const string ClericSpellbookGuid = "4673d19a0cf2fab4f885cc4d1353da33";
        private const string ClericSpellListGuid = "8443ce803d2d31347897a3d85cc32f53";
        private const string HeraldCallerConversionGuid = "f6c112a579a81c0439c597f9ee222243";
        private const string ClericErastilGuid = "afc775188deb7a44aa4cbde03512c671";
        private const string ClericAsmodeusGuid = "a3a5ccc9c670e6f4ca4a686d23b89900";
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
            var links = new JArray(); int linked = 0, children = 0, gated = 0, known = 0; var unclassified = new JArray(); var mismatches = new JArray();
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
                        if (KnownThirdPartySummonMonsterRows.Contains(row.name, StringComparer.Ordinal)) known++;
                        else unclassified.Add(new JObject { ["tier"] = tier, ["ability"] = row.AssetGuid, ["name"] = row.Name, ["internalName"] = row.name });
                        continue;
                    }
                    gated++;
                    SummonSpellAlignment spell = SummonClericAlignmentCatalog.For(gate.CreatureKey, gate.MonsterTemplated);
                    SummonAlignmentAxes live = ClericLiveAxes(row);
                    if (!spell.CasterDependent && live != spell.Descriptors && mismatches.Count < 60)
                        mismatches.Add(new JObject { ["key"] = gate.CreatureKey, ["tier"] = tier, ["policy"] = spell.Descriptors.ToString(), ["blueprintDescriptors"] = live.ToString() });
                }
            }
            Sprint25Check("cleric-route-census", identities && linked == 9 && unclassified.Count == 0, new JObject {
                ["clericClass"] = cleric.AssetGuid, ["spellbook"] = cleric.Spellbook?.AssetGuid, ["spellList"] = list?.AssetGuid, ["identitiesExact"] = identities,
                ["linkedParents"] = linked, ["children"] = children, ["gatedChildren"] = gated, ["knownThirdPartyRows"] = known,
                ["unclassifiedRows"] = unclassified, ["policyVersusBlueprintDescriptorDifferences"] = mismatches, ["links"] = links,
                ["scope"] = "Structural census of the real cleric spell list and published children." },
                "All nine Summon Monster parents are on the actual cleric spell list, and every child is gated or a named known third-party row.");
            BlueprintFeatureSelection deities = BlueprintLibraryLookup.RequireExact<BlueprintFeatureSelection>(library, SummonClericAlignmentPolicy.DeitySelectionGuid, "deity selection");
            SummonAlignmentAxes ignored;
            string[] unmapped = deities.AllFeatures.Where(value => value != null && !SummonClericDeityCatalog.TryGetAxes(value.AssetGuid, out ignored)).Select(value => value.name + ":" + value.AssetGuid).ToArray();
            Sprint25Check("cleric-deity-census", unmapped.Length == 0, new JObject { ["options"] = deities.AllFeatures.Length, ["unmapped"] = new JArray(unmapped) },
                "Every installed deity selection option has an exact mapped alignment.");
            BlueprintScriptableObject heraldObject; bool hasHerald = library.BlueprintsByAssetId.TryGetValue(HeraldCallerConversionGuid, out heraldObject);
            BlueprintFeature herald = heraldObject as BlueprintFeature;
            var routes = herald == null ? new SpontaneousSpellConversion[0] : (herald.ComponentsArray ?? new BlueprintComponent[0]).OfType<SpontaneousSpellConversion>().ToArray();
            string[] converted = routes.SelectMany(value => value.SpellsByLevel ?? new BlueprintAbility[0]).Where(value => value != null).Select(value => value.AssetGuid).Distinct().ToArray();
            string[] canonical = ExpandedSummoningPublisher.CanonicalParentGuids.Take(9).ToArray();
            Sprint25Check("cleric-herald-caller-route", !hasHerald || converted.All(value => canonical.Contains(value, StringComparer.Ordinal)), new JObject {
                ["installed"] = hasHerald, ["components"] = routes.Length, ["conversions"] = new JArray(converted),
                ["scope"] = "Structural inspection of the installed conversion feature; a Herald Caller menu was not opened." },
                "Installed Herald Caller conversions reach the shared parents, so the same gated children apply.");
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
                BlueprintAbility[] expected = variants.Where(value => new AbilityData(value, book) { ConvertedFrom = original }.IsVisible()).ToArray();
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
        private IEnumerable<int> ClericForbiddenAttempt(ExpandedSummoningCorrectionFixture fixture, BlueprintAbility child, Spellbook book, AbilityData prepared, Cleanup2NativeDefaultScope mode, Stopwatch phase)
        {
            var data = new AbilityData(child, book) { ConvertedFrom = prepared };
            var debit = new Cleanup2SourceDebit(fixture.Caster);
            bool visible = data.IsVisible(), available = data.IsAvailableForCast;
            string reason = data.GetUnavailableReason();
            var hooks = HarmonyInstance.Create("kmg.cleric.forbidden." + _request.RunId); UnitUseAbility command = null;
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
            bool refused = !visible && !available && result == "Fail" && debit.Unspent && _expandedSummoningPlayerPathSpawnActionCount == 0 && summons.Length == 0;
            Sprint25Check("cleric-forbidden-attempt", refused, new JObject { ["child"] = child.AssetGuid, ["name"] = child.Name, ["visible"] = visible, ["availableForCast"] = available,
                ["nativeReason"] = reason, ["commandResult"] = result, ["slotOrResourceSpent"] = !debit.Unspent, ["spawnActions"] = _expandedSummoningPlayerPathSpawnActionCount, ["summons"] = summons.Length,
                ["debit"] = debit.Describe(prepared.ParamSpellSlot), ["route"] = "AbilityData built as a saved shortcut holds it; native UnitUseAbility; no manual tick or forced act", ["wallSeconds"] = watch.Elapsed.TotalSeconds },
                "A forbidden selected child is refused before any slot or resource is spent, and nothing is summoned.");
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

        private void ClericNonClericControl(BlueprintAbility forbidden, UnitEntityData anchor, BlueprintFeature deity)
        {
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "b3a505fb61437dc4097f43c3f8f9a4cf", "non-cleric control class");
            var owner = new TeleportResourceFixtureOwner(anchor);
            var fact = anchor.Descriptor.AddFact(deity);
            try
            {
                Spellbook book = owner.AddBook(sorcerer.Spellbook);
                var data = new AbilityData(forbidden, book);
                SummonClericFacts facts = SummonClericAlignmentRuntime.Read(anchor.Descriptor);
                Sprint25Check("cleric-non-cleric-control", !SummonClericAlignmentPolicy.IsCleric(facts) && data.IsVisible() && ClericGateOf(forbidden).IsAvailableFor(data), new JObject {
                    ["classes"] = new JArray(facts.ClassGuids), ["deityFact"] = deity.AssetGuid, ["ability"] = forbidden.AssetGuid, ["visible"] = data.IsVisible(),
                    ["scope"] = "Gate-level control on a real non-cleric unit with a deity fact and a Sorcerer book; a popup was not opened for this unit." },
                    "Holding a deity fact and casting the same row without cleric levels is unrestricted.");
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
            ClericCensus(cleric);
            var erastil = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, ClericErastilGuid, "Erastil deity");
            var asmodeus = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, ClericAsmodeusGuid, "Asmodeus deity");
            Func<int, BlueprintAbility> smParent = tier => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, ExpandedSummoningPublisher.CanonicalParentGuids[tier - 1], "Summon Monster parent");
            Func<string, int, SummonVariantSpec> spec = (key, tier) => ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Single(value =>
                value.Creature.Key == key && value.ParentTier == tier && value.Multiplicity == SummonMultiplicity.One);
            var party = player.Party.ToArray(); var selected = ui.SelectionManagerPC.SelectedUnits.ToArray(); var selectedGroup = group.GetCurrentCharacter();
            var groups = bar.Group.GroupElements.ToArray(); bool[] toggles = groups.Select(value => value.ToggleState).ToArray();
            object levelController = null; bool added = false; NativeIconScreenEvidence screens = null; Cleanup2NativeDefaultScope mode = null;
            Spellbook book = null; AbilityData tierThree = null, tierNine = null; Feature deityFact = null;
            var watch = Stopwatch.StartNew();
            try
            {
                fixture.Caster.Stats.Wisdom.BaseValue = 30;
                AdvanceDisposableSpellcaster(fixture.Caster.Descriptor, cleric, 20, ref levelController);
                book = fixture.Caster.Descriptor.GetSpellbook(cleric);
                while (book.CasterLevel < 20) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false);
                Sprint25Check("cleric-actual-class-route", book != null && ReferenceEquals(book.Blueprint, cleric.Spellbook) && SummonClericAlignmentPolicy.IsCleric(SummonClericAlignmentRuntime.Read(fixture.Caster.Descriptor)),
                    new JObject { ["book"] = book?.Blueprint.AssetGuid, ["casterLevel"] = book?.CasterLevel ?? 0, ["classes"] = new JArray(fixture.Caster.Descriptor.Progression.Classes.Select(value => value.CharacterClass.AssetGuid + ":" + value.Level)) },
                    "The fixture is a real cleric from native level-up, with the native cleric spellbook.");
                fixture.Caster.Descriptor.Alignment.Set(Alignment.LawfulGood);
                deityFact = fixture.Caster.Descriptor.AddFact(erastil) as Feature;
                tierThree = PrepareExpandedSummoningPlayerPathSpell(book, smParent(3), smParent(3), 3);
                tierNine = PrepareExpandedSummoningPlayerPathSpell(book, smParent(9), smParent(9), 9);
                book.Rest();
                tierThree = book.GetMemorizedSpellSlots(3).First(value => value.Spell != null && ReferenceEquals(value.Spell.Blueprint, smParent(3))).Spell;
                tierNine = book.GetMemorizedSpellSlots(9).First(value => value.Spell != null && ReferenceEquals(value.Spell.Blueprint, smParent(9))).Spell;
                if (!player.Party.Contains(fixture.Caster)) { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                mode = Cleanup2EnterNativeDefault(fixture, "summon-cleric-alignment");
                screens = new NativeIconScreenEvidence(_request);
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                if (!mode.Current) throw new InvalidOperationException("Native Default did not settle after cleric setup.");
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement).GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup);
                BlueprintAbility lantern = ExpandedSummoningRoot(fixture.Blueprints, spec("lantern-archon", 3)), dretch = ExpandedSummoningRoot(fixture.Blueprints, spec("dretch", 3));

                AlphaMenu nineLawfulGood = null; AbilityData heldLegal = null;
                foreach (int frame in R6Cell("cleric-menu-nine-lg", () => ClericOpenMenu("lawful-good-erastil-tier9", spellGroup, levels, 9, book, smParent(9), tierNine, mode, watch, screens, (menu, source) => nineLawfulGood = menu, true))) yield return frame;
                if (nineLawfulGood != null)
                {
                    string[] names = nineLawfulGood.Data.Select(value => value.Name).ToArray();
                    heldLegal = nineLawfulGood.Data.FirstOrDefault(value => value.Name.StartsWith("Archon, ", StringComparison.Ordinal));
                    Sprint25Check("cleric-menu-filter-expectations", names.Any(value => value.StartsWith("Archon, ", StringComparison.Ordinal)) && names.Any(value => value.StartsWith("Deva, Astral", StringComparison.Ordinal)) &&
                        !names.Any(value => value.StartsWith("Demon, ", StringComparison.Ordinal) || value.StartsWith("Devil, ", StringComparison.Ordinal) || value.StartsWith("Azata, ", StringComparison.Ordinal)),
                        new JObject { ["rows"] = names.Length, ["first"] = names.FirstOrDefault(), ["last"] = names.LastOrDefault() },
                        "A lawful good cleric of a lawful good deity keeps archons and the Good-only Astral Deva, and loses demons, devils and chaotic azatas.");
                }

                // The decisive refusal and legal cast run before the optional menus.
                foreach (int frame in R6Cell("cleric-forbidden", () => ClericForbiddenAttempt(fixture, dretch, book, tierThree, mode, watch))) yield return frame;

                AlphaMenu threeLawfulGood = null; ActionBarGroupSlot threeSource = null;
                foreach (int frame in R6Cell("cleric-menu-three-lg", () => ClericOpenMenu("lawful-good-erastil-tier3", spellGroup, levels, 3, book, smParent(3), tierThree, mode, watch, screens, (menu, source) => { threeLawfulGood = menu; threeSource = source; }, false))) yield return frame;
                if (threeLawfulGood != null && threeSource != null)
                {
                    int index = Array.FindIndex(threeLawfulGood.Data, value => ReferenceEquals(value?.Blueprint, lantern));
                    Sprint25Check("cleric-legal-row-present", index >= 0 && !threeLawfulGood.Data.Any(value => ReferenceEquals(value?.Blueprint, dretch)), new JObject { ["index"] = index, ["rows"] = threeLawfulGood.Rows.Length },
                        "The legal Lantern Archon is offered and the forbidden Dretch is absent.");
                    if (index >= 0)
                    {
                        var debit = new Cleanup2SourceDebit(fixture.Caster);
                        ActionBarGroupSlot castSource = threeSource;
                        foreach (int frame in R6Cell("cleric-legal", () => ClericLegalCast(fixture, threeLawfulGood.Rows[index], threeLawfulGood.Data[index], tierThree, debit, () => Cleanup2MenuAdapterHide(castSource, tierThree), mode, watch))) yield return frame;
                    }
                    Cleanup2MenuAdapterHide(threeSource, tierThree);
                }

                // Induced change: a neutral cleric of a lawful evil deity.
                bool heldBefore = heldLegal != null && heldLegal.IsAvailableForCast;
                fixture.Caster.Descriptor.Alignment.Set(Alignment.TrueNeutral);
                fixture.Caster.Descriptor.RemoveFact(deityFact); deityFact = fixture.Caster.Descriptor.AddFact(asmodeus) as Feature;
                bool heldAfter = heldLegal != null && heldLegal.IsAvailableForCast;
                Sprint25Check("cleric-held-choice-reevaluated", heldLegal != null && heldBefore && !heldAfter, new JObject { ["ability"] = heldLegal?.Blueprint.AssetGuid, ["availableBeforeChange"] = heldBefore, ["availableAfterChange"] = heldAfter, ["state"] = "induced alignment and deity change on the fixture" },
                    "A choice obtained before the change is refused after it.");
                AlphaMenu deityMenu = null;
                foreach (int frame in R6Cell("cleric-menu-nine-deity", () => ClericOpenMenu("neutral-asmodeus-tier9", spellGroup, levels, 9, book, smParent(9), tierNine, mode, watch, screens, (menu, source) => deityMenu = menu, true))) yield return frame;
                if (deityMenu != null)
                {
                    string[] names = deityMenu.Data.Select(value => value.Name).ToArray();
                    Sprint25Check("cleric-deity-derived-restriction", names.Any(value => value.StartsWith("Devil, ", StringComparison.Ordinal)) && !names.Any(value => value.StartsWith("Archon, ", StringComparison.Ordinal) || value.StartsWith("Demon, ", StringComparison.Ordinal) || value.StartsWith("Azata, ", StringComparison.Ordinal)),
                        new JObject { ["rows"] = names.Length, ["casterAlignment"] = "TrueNeutral", ["deity"] = asmodeus.AssetGuid },
                        "A true neutral cleric of a lawful evil deity loses Good and Chaotic rows from the deity alone, and keeps devils.");
                }
                ClericNonClericControl(lantern, player.MainCharacter.Value, asmodeus);
                Sprint25Check("cleric-surface-limits", true, new JObject { ["openedInGame"] = "Prepared Summon Monster III and IX popups on a levelled cleric.", ["execution"] = "One forbidden command and one legal cast, unassisted.",
                    ["notObserved"] = "Herald Caller menu, console navigation, saved-hotbar reload, cold load, autonomous AI, multiclass book popup.", ["wallSeconds"] = watch.Elapsed.TotalSeconds },
                    "Observed and unobserved surfaces are reported separately.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
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
