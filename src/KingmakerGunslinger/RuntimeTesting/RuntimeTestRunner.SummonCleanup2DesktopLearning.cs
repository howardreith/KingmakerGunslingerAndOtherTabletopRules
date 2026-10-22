using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI;
using Kingmaker.UI.Group;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Spells.Teleportation;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Real desktop rows and their existing callbacks. Native class/XP
        // seeding is disposable-fixture setup; only the observed allocation,
        // cancellation and preparation are UI claims. No Commit is called.
        private IEnumerable<int> ReviewSummonCleanup2DesktopLearning(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters == null ||
                _request.Parameters.Count != 1 || (string)_request.Parameters["reviewPhase"] != "elementals-learning" ||
                !ElementalSourceChooserRuntime.Active)
                throw new InvalidOperationException("Exact active automatic-exit desktop learning phase required.");
            var game = Game.Instance; var ui = game.UI; var player = game.Player; var group = GroupController.Instance;
            var build = ui.CharacterBuildController;
            if (game.IsControllerGamepad || !Cleanup2NoNarrativeOverlay() || group == null || ui.SelectionManagerPC == null ||
                ui.ServiceWindow.WindowTabs.IsShow || ui.SpellBookController.IsShow || build == null || build.IsShow ||
                build.LevelUpController != null || ui.LevelUpController != null || DialogMessageBox.Instance == null ||
                DialogMessageBox.Instance.IsShown || ui.TooltipsController.ContextTooltipData != null || !fixture.Caster.Commands.Empty)
                throw new InvalidOperationException("Closed idle native desktop book/creator and no foreign modal required.");
            var party = player.Party.ToArray(); var selected = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var originalGroup = group.GetCurrentCharacter(); var buildUnit = build.Unit;
            var inputs = party.Select(value => new Cleanup2TooltipInputs(value)).ToArray();
            var bars = party.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            bool added = false, selectedFixture = false;
            var mode = Cleanup2EnterNativeDefault(fixture, "elementals-learning");
            var screens = new NativeIconScreenEvidence(_request); var watch = Stopwatch.StartNew();
            try
            {
                game.IsPaused = true;
                if (!party.Contains(fixture.Caster))
                { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster); selectedFixture = true;
                for (int frame = 0; frame < 8; frame++) yield return 0;
                foreach (int frame in Cleanup2DesktopPrepareAndUndo(fixture, book, screens)) yield return frame;
                if (!mode.Current || watch.Elapsed.TotalSeconds > 120) throw new InvalidOperationException("Desktop learning phase boundary expired or changed.");
                foreach (int frame in Cleanup2DesktopLearnAndCancel(fixture, book, screens)) yield return frame;
                if (!mode.Current || watch.Elapsed.TotalSeconds > 180) throw new InvalidOperationException("Desktop learning phase exceeded its bounded native UI scope.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                try
                {
                    bool ownedUi = ReferenceEquals(group, GroupController.Instance) &&
                        ((!selectedFixture && ReferenceEquals(group.GetCurrentCharacter(), originalGroup) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected)) ||
                        (ReferenceEquals(group.GetCurrentCharacter(), fixture.Caster) && ui.SelectionManagerPC.SelectedUnits.Count == 1 &&
                            ReferenceEquals(ui.SelectionManagerPC.SelectedUnits.First(), fixture.Caster)));
                    if (!ownedUi) errors.Add("foreign-selection-owner");
                    else
                    {
                        restore("party", () => { if (added) { player.PartyCharacters.Remove(fixture.Caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); } });
                        if (selectedFixture)
                        {
                            restore("group", () => group.SelectUnit(originalGroup));
                            restore("selection", () => ui.SelectionManagerPC.MultiSelect(selected.Where(value => value?.View != null).Select(value => value.View).ToArray(), false));
                        }
                    }
                    bool exact = errors.Count == 0 && player.Party.SequenceEqual(party) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected) &&
                        ReferenceEquals(group.GetCurrentCharacter(), originalGroup) && inputs.All(value => value.Unchanged()) && bars.All(value => value.IsRestored()) &&
                        !build.IsShow && build.LevelUpController == null && ui.LevelUpController == null && ReferenceEquals(build.Unit, buildUnit) &&
                        !ui.ServiceWindow.WindowTabs.IsShow && !ui.SpellBookController.IsShow && !DialogMessageBox.Instance.IsShown;
                    Cleanup2Check("elemental-desktop-learning-restored", exact, new JObject {
                        ["partyExact"] = player.Party.SequenceEqual(party), ["selectionExact"] = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected),
                        ["originalSourcesAndPinsExact"] = inputs.All(value => value.Unchanged()) && bars.All(value => value.IsRestored()),
                        ["creatorClosed"] = !build.IsShow && build.LevelUpController == null && ui.LevelUpController == null,
                        ["creatorUnitExact"] = ReferenceEquals(build.Unit, buildUnit), ["errors"] = new JArray(errors),
                        ["scope"] = "Save-free native PC discovery, one pending original-source allocation/cancel and one original-source preparation/unprepare. No committed character, save hydration, console or casting claim." },
                        "Owned native screens close and original party, selection, creator owner, source objects and pins remain exact.");
                    if (!exact) throw new InvalidOperationException("Desktop learning restoration is unverified.");
                }
                finally { try { screens.Dispose(); } finally { mode.Dispose(); } }
            }
        }

        private static Button Cleanup2DesktopDisclosure(Component owner)
        {
            var fold = owner.GetComponent<ElementalNativeSourceFold>();
            var header = fold == null ? null : typeof(ElementalNativeSourceFold).GetField("_header", Cleanup2Fields).GetValue(fold) as GameObject;
            var button = header == null ? null : header.GetComponent<Button>();
            if (fold == null || !fold.HasFold || button == null || !header.activeInHierarchy || !button.interactable)
                throw new InvalidOperationException("Exact live native PC original-source disclosure missing: " + ElementalNativeSourceFold.Outcome);
            return button;
        }

        private static object Cleanup2DesktopNativeCallback(CharBuildSelectorItem row)
        { return typeof(CharBuildSelectorItem).GetField("Action", Cleanup2Fields).GetValue(row); }

        private static void Cleanup2DesktopNativeDoubleClick(SpellItemBase row)
        {
            Type owner = row is SpellSlotItem ? typeof(SpellSlotItem) : typeof(SpellItem);
            if (row == null || !row.gameObject.activeInHierarchy || !owner.IsInstanceOfType(row))
                throw new InvalidOperationException("Exact active native desktop spell row required.");
            // Both native double-click callbacks are protected in 2.1.7b.
            // Invoke that callback; never reproduce its spellbook writes here.
            owner.GetMethod("OnDoubleClick", Cleanup2Fields).Invoke(row, null);
        }

        private IEnumerable<int> Cleanup2DesktopPrepareAndUndo(ExpandedSummoningCorrectionFixture fixture, Spellbook book, NativeIconScreenEvidence screens)
        {
            var game = Game.Instance; var ui = game.UI; var controller = ui.SpellBookController;
            var sourceBlueprint = Cleanup2ElementalAbility(fixture, ElementalSourceChooserPolicy.Rank(2).Guid);
            if (!book.IsKnown(sourceBlueprint)) book.AddKnown(2, sourceBlueprint, true);
            var known = book.GetAllKnownSpells().ToArray(); var source = known.Single(value => ReferenceEquals(value.Blueprint, sourceBlueprint) && !value.HasMetamagic(Metamagic.Extend));
            var slots = Enumerable.Range(0, 10).SelectMany(level => RawSlots(book, level)).ToArray();
            var spells = slots.Select(value => value.Spell).ToArray(); var available = slots.Select(value => value.Available).ToArray();
            var resources = Cleanup2ElementalResources(fixture.Caster); var pins = new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
            SpellSlot written = null; bool opened = false;
            try
            {
                if (!slots.Any(value => value.SpellLevel == 2 && value.Type == SpellSlotType.Common && value.Spell == null))
                    throw new InvalidOperationException("Existing empty common level2 fixture preparation slot required.");
                ui.ServiceWindow.HandleOpenSpellbook(); opened = true;
                foreach (int frame in WaitTeleportLevelUpUi(() => controller.IsShow && ui.ServiceWindow.WindowTabs.IsShow, "desktop native spellbook")) yield return frame;
                var view = controller.SpellBookView; int bookIndex = view.ClassToggle.Spellbooks.IndexOf(book);
                var classTab = view.ClassToggle.GetComponentsInChildren<SpellbookClassTab>(true).Single(value => value.Index == bookIndex && value.gameObject.activeInHierarchy);
                classTab.Toggle.isOn = true;
                var level = view.LevelBookSwitcher.m_Tabs[2];
                if (!level.gameObject.activeInHierarchy || !level.Toggle.interactable) throw new InvalidOperationException("Native desktop level2 tab unavailable.");
                level.Toggle.isOn = true;
                for (int frame = 0; frame < 8; frame++) yield return 0;
                if (!ReferenceEquals(controller.CurrentSpellbook, book) || controller.CurrentBookLevel != 2) throw new InvalidOperationException("Native desktop source book/level differs.");
                for (int page = 0; page < 30 && !view.GetComponentsInChildren<SpellItem>(true).Any(value => ReferenceEquals(value.SpellData, source)); page++)
                {
                    int previousPage = controller.CurrentPageIndex;
                    view.GoNextPage();
                    for (int frame = 0; frame < 4; frame++) yield return 0;
                    if (!ReferenceEquals(controller.CurrentSpellbook, book) || controller.CurrentBookLevel != 2 || controller.CurrentPageIndex <= previousPage)
                        throw new InvalidOperationException("Native PC pagination cannot reach the original source on its exact level.");
                }
                var header = Cleanup2DesktopDisclosure(view);
                var nativeRows = view.GetComponentsInChildren<SpellItem>(true).Where(value => value.SpellData != null).ToArray();
                var rowData = nativeRows.Select(value => value.SpellData).ToArray();
                header.onClick.Invoke(); for (int frame = 0; frame < 4; frame++) yield return 0;
                bool disclosure = book.GetAllKnownSpells().SequenceEqual(known) && slots.Select(value => value.Spell).SequenceEqual(spells) &&
                    slots.Select(value => value.Available).SequenceEqual(available) && Cleanup2ElementalResources(fixture.Caster) == resources &&
                    nativeRows.Select(value => value.SpellData).SequenceEqual(rowData);
                Cleanup2Check("elemental-desktop-preparation-disclosure", disclosure, new JObject { ["source"] = sourceBlueprint.AssetGuid,
                    ["sourceBook"] = book.Blueprint.AssetGuid, ["knownObjectsExact"] = book.GetAllKnownSpells().SequenceEqual(known),
                    ["nativeRowAbilityReferencesExact"] = nativeRows.Select(value => value.SpellData).SequenceEqual(rowData),
                    ["slotsAndAvailabilityExact"] = slots.Select(value => value.Spell).SequenceEqual(spells) && slots.Select(value => value.Available).SequenceEqual(available),
                    ["foldOutcome"] = ElementalNativeSourceFold.Outcome }, "Native PC source disclosure changes visibility without spending or replacing native source/preparation objects.");
                if (!disclosure) throw new InvalidOperationException("Native PC preparation disclosure changed source authority.");
                var row = nativeRows.Single(value => ReferenceEquals(value.SpellData, source) && value.gameObject.activeInHierarchy);
                row.Toggle.isOn = true;
                if (!ReferenceEquals(row.SpellData, source) || !ReferenceEquals(controller.CurrentSpellbook, book))
                    throw new InvalidOperationException("Native PC preparation row changed after selection.");
                Cleanup2DesktopNativeDoubleClick(row); for (int frame = 0; frame < 8; frame++) yield return 0;
                var changed = slots.Where((value, index) => !ReferenceEquals(value.Spell, spells[index])).ToArray();
                written = changed.Length == 1 ? changed[0] : null;
                bool prepared = written != null && written.SpellLevel == 2 && written.Type == SpellSlotType.Common && ReferenceEquals(written.Spell, source) &&
                    !written.Available && slots.Where(value => !ReferenceEquals(value, written)).All(value => {
                        int index = Array.IndexOf(slots, value); return ReferenceEquals(value.Spell, spells[index]) && value.Available == available[index]; }) &&
                    book.GetAllKnownSpells().SequenceEqual(known) && Cleanup2ElementalResources(fixture.Caster) == resources;
                Cleanup2Check("elemental-desktop-native-source-preparation", prepared, new JObject { ["source"] = sourceBlueprint.AssetGuid,
                    ["exactKnownAbilityReference"] = written != null && ReferenceEquals(written.Spell, source), ["changedSlotCount"] = changed.Length,
                    ["slotIndex"] = written?.Index, ["slotType"] = written?.Type.ToString(), ["slotAvailable"] = written?.Available,
                    ["nativeInput"] = "SpellItem.OnDoubleClick -> native TryMemorize -> SpellBookController.MemorizeWithSound", ["manualMemorizeCalls"] = 0 },
                    "Double-clicking the original native PC source row assigns its exact AbilityData to one common preparation and preserves every other source/resource.");
                if (!prepared) throw new InvalidOperationException("Native PC source preparation failed.");
                var preparedRow = controller.GetComponentsInChildren<SpellSlotItem>(true).Single(value => value.gameObject.activeInHierarchy && ReferenceEquals(value.MechanicSlot, written));
                foreach (int frame in screens.Capture("elementals-desktop-native-prepared-slot", new JObject { ["source"] = sourceBlueprint.AssetGuid,
                    ["book"] = book.Blueprint.AssetGuid, ["slotIndex"] = written.Index, ["nativeSlotReferenceExact"] = ReferenceEquals(preparedRow.MechanicSlot, written),
                    ["nativeText"] = new JArray(view.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value => value.text)) },
                    () => controller.IsShow && ReferenceEquals(controller.CurrentSpellbook, book) && ReferenceEquals(written.Spell, source) &&
                        preparedRow != null && preparedRow.gameObject.activeInHierarchy && ReferenceEquals(preparedRow.MechanicSlot, written) && Cleanup2NoNarrativeOverlay())) yield return frame;
                if (!ReferenceEquals(preparedRow.MechanicSlot, written) || !ReferenceEquals(controller.CurrentSpellbook, book))
                    throw new InvalidOperationException("Native PC prepared-slot row changed before unprepare.");
                Cleanup2DesktopNativeDoubleClick(preparedRow); for (int frame = 0; frame < 8; frame++) yield return 0;
                bool undone = written.Spell == null && book.GetAllKnownSpells().SequenceEqual(known) &&
                    slots.Where(value => !ReferenceEquals(value, written)).All(value => {
                        int index = Array.IndexOf(slots, value); return ReferenceEquals(value.Spell, spells[index]) && value.Available == available[index]; }) &&
                    Cleanup2ElementalResources(fixture.Caster) == resources;
                Cleanup2Check("elemental-desktop-native-preparation-undo", undone, new JObject { ["slotEmpty"] = written.Spell == null,
                    ["sourceKnownExact"] = book.GetAllKnownSpells().SequenceEqual(known), ["slotAvailableBefore"] = available[Array.IndexOf(slots, written)],
                    ["slotAvailableAfter"] = written.Available, ["nativeInput"] = "SpellSlotItem.OnDoubleClick -> native TryForgetSpell",
                    ["scope"] = "Native Forget may reset empty-slot metadata and automatic hotbar bookkeeping on the disposable fixture. Other source slots/resources remain exact." },
                    "The real prepared-slot double-click clears only its own original-source preparation without removing the known source.");
                if (!undone) throw new InvalidOperationException("Native PC preparation undo failed.");
            }
            finally
            {
                if (opened && controller.IsShow && ReferenceEquals(controller.CurrentSpellbook, book) && ui.ServiceWindow.WindowTabs.IsShow)
                    ui.ServiceWindow.HandleOpenSpellbook();
                if (written != null && ReferenceEquals(written.Spell, source)) book.ForgetMemorized(written);
                pins.Restore();
            }
            for (int frame = 0; frame < 8; frame++) yield return 0;
        }

        private static Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility[] Cleanup2DesktopPending(SpellSelectionData selection)
        { return (selection.ExtraSelected ?? new Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility[0]).Concat(
            (selection.LevelCount ?? new SpellSelectionData.SpellSelectionState[0]).Where(value => value != null && value.SpellSelections != null)
                .SelectMany(value => value.SpellSelections)).ToArray(); }

        private IEnumerable<int> Cleanup2DesktopLearnAndCancel(ExpandedSummoningCorrectionFixture fixture, Spellbook fixtureBook, NativeIconScreenEvidence screens)
        {
            var game = Game.Instance; var ui = game.UI; var build = ui.CharacterBuildController; var originalBuildUnit = build.Unit;
            var globalBefore = ui.LevelUpController; BlueprintUnit blueprint = null; UnitEntityData unit = null;
            LevelUpController backend = null; int commits = 0;
            try
            {
                var wizard = fixture.Blueprints.OfType<BlueprintCharacterClass>().Single(value => ReferenceEquals(value.Spellbook, fixtureBook.Blueprint));
                var human = BlueprintRoot.Instance.Progression.CharacterRaces.Single(value => value.AssetGuid == "0a5d473ead98b0646b94495af250fdc4");
                blueprint = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
                blueprint.name = "KMG_Cleanup2_DesktopLearningWizard"; blueprint.IsCheater = true; blueprint.Race = human;
                unit = game.EntityCreator.SpawnUnit(blueprint, fixture.Caster.Position + new Vector3(-2, 0, -2), Quaternion.identity, fixture.Scene);
                if (unit?.View == null || fixture.UnitsBefore.Any(value => ReferenceEquals(value, unit))) throw new InvalidOperationException("Independent native desktop learning actor required.");
                fixture.Created.Add(unit); game.EntityCreator.Tick(); SetExpandedSummoningBrainActive(unit, false);
                unit.Stats.Intelligence.BaseValue = 18; unit.Stats.Wisdom.BaseValue = 12; unit.Stats.Charisma.BaseValue = 14;
                for (int level = 0; level < 2; level++)
                {
                    backend = LevelUpController.StartWithoutAssigningStaticInstance(unit.Descriptor, false, null, null,
                        level == 0 ? LevelUpState.CharBuildMode.CharGen : LevelUpState.CharBuildMode.LevelUp);
                    if (level == 0) { backend.SelectRace(human); backend.SelectGender(fixture.Caster.Descriptor.Gender); backend.SelectAlignment(fixture.Caster.Descriptor.Alignment.Value); }
                    if (!backend.SelectClass(wizard)) throw new InvalidOperationException("Native Wizard fixture seed refused.");
                    FillOracleNativeChoices(backend);
                    typeof(LevelUpController).GetMethod("ApplyLevelup", Cleanup2Fields).Invoke(backend, new object[] { unit.Descriptor });
                    backend.Cancel(); backend = null;
                    if (unit.Descriptor.Progression.GetClassLevel(wizard) != level + 1) throw new InvalidOperationException("Native Wizard fixture seed level differs.");
                }
                var book = unit.Descriptor.GetSpellbook(wizard.Spellbook);
                var source = Cleanup2ElementalAbility(fixture, ElementalSourceChooserPolicy.Rank(2).Guid);
                if (book == null || book.IsKnown(source) || unit.Descriptor.Progression.CharacterLevel != 2)
                    throw new InvalidOperationException("Unknown original source on native Wizard2 required.");
                typeof(UnitProgressionData).GetProperty("Experience").SetValue(unit.Descriptor.Progression, game.BlueprintRoot.Progression.XPTable.GetBonus(3), null);
                if (!LevelUpController.CanLevelUp(unit.Descriptor)) throw new InvalidOperationException("Native Wizard3 XP prerequisite absent.");
                var known = book.GetAllKnownSpells().ToArray(); var resources = Cleanup2ElementalResources(unit);
                build.HandleLevelUpStart(unit.Descriptor, null, () => commits++, LevelUpState.CharBuildMode.LevelUp);
                backend = build.LevelUpController;
                if (backend == null || backend.AutoCommit || ReferenceEquals(backend.Preview, unit.Descriptor) ||
                    !ReferenceEquals(backend.Unit, unit.Descriptor) || !ReferenceEquals(ui.LevelUpController, backend))
                    throw new InvalidOperationException("Actual PC creator did not create an independent owned preview.");
                for (int frame = 0; frame < 15; frame++) yield return 0;
                build.SetClass(wizard); CompleteTeleportLevelUpPrerequisites(build);
                build.SetPhase((int)CharBPhase.Type.Spells);
                foreach (int frame in WaitTeleportLevelUpUi(() => build.GetComponentsInChildren<CharBSelectionSwitchItem>(true).Any(value =>
                    value.gameObject.activeInHierarchy && ReferenceEquals(value.SpellSelectionData?.Spellbook, wizard.Spellbook) &&
                    (value.SpellSelectionData.ExtraMaxLevel == 2 || value.SpellLevel == 2)), "native PC original-source learning collection")) yield return frame;
                var collection = build.GetComponentsInChildren<CharBSelectionSwitchItem>(true).Where(value => value.gameObject.activeInHierarchy &&
                    ReferenceEquals(value.SpellSelectionData?.Spellbook, wizard.Spellbook) &&
                    (value.SpellSelectionData.ExtraMaxLevel == 2 || value.SpellLevel == 2)).OrderBy(value => value.SpellIndex).First();
                collection.Toggle.isOn = true;
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var selection = build.Spells.CurrentSpellSelectionData;
                if (!ReferenceEquals(selection, collection.SpellSelectionData) || !ReferenceEquals(selection.Spellbook, wizard.Spellbook) ||
                    !ReferenceEquals(selection.SpellList, wizard.Spellbook.SpellList) || selection.ExtraByStat)
                    throw new InvalidOperationException("Actual PC normal native learning collection changed.");
                var layer = build.GetComponentsInChildren<CharBSelectorLayer>(true).Single(value => value.gameObject.activeInHierarchy &&
                    ReferenceEquals(typeof(CharBSelectorLayer).GetField("CurrentSpellSelectionData", Cleanup2Fields).GetValue(value), selection) &&
                    value.GetComponentsInChildren<CharBuildSelectorItem>(true).Any(candidate => ReferenceEquals(candidate.BlueprintAbility, source)));
                var pending = Cleanup2DesktopPending(selection); var nativeRows = layer.SelectorItems.ToArray();
                var blueprints = nativeRows.Select(value => value.BlueprintAbility).ToArray(); var callbacks = nativeRows.Select(Cleanup2DesktopNativeCallback).ToArray();
                var header = Cleanup2DesktopDisclosure(layer);
                header.onClick.Invoke(); for (int frame = 0; frame < 4; frame++) yield return 0;
                bool expanded = Cleanup2DesktopPending(selection).SequenceEqual(pending) && !book.IsKnown(source) &&
                    layer.SelectorItems.SequenceEqual(nativeRows) && nativeRows.Select(value => value.BlueprintAbility).SequenceEqual(blueprints) &&
                    nativeRows.Select((value, index) => ReferenceEquals(Cleanup2DesktopNativeCallback(value), callbacks[index])).All(value => value) &&
                    book.GetAllKnownSpells().SequenceEqual(known) && Cleanup2ElementalResources(unit) == resources;
                Cleanup2Check("elemental-desktop-learning-disclosure-no-allocation", expanded, new JObject { ["source"] = source.AssetGuid,
                    ["nativeSelectionObjectExact"] = ReferenceEquals(build.Spells.CurrentSpellSelectionData, selection), ["pendingExact"] = Cleanup2DesktopPending(selection).SequenceEqual(pending),
                    ["nativeRowAndCallbackReferencesExact"] = layer.SelectorItems.SequenceEqual(nativeRows) &&
                        nativeRows.Select((value, index) => ReferenceEquals(Cleanup2DesktopNativeCallback(value), callbacks[index])).All(value => value),
                    ["normalExtraMaxLevel"] = selection.ExtraMaxLevel, ["extraByStat"] = selection.ExtraByStat, ["foldOutcome"] = ElementalNativeSourceFold.Outcome },
                    "Real PC original-source disclosure reveals the existing selector rows without replacing callbacks, consuming a pending learning choice or altering the original actor.");
                if (!expanded) throw new InvalidOperationException("Native PC learning disclosure changed allocation authority.");
                Func<CharBuildSelectorItem> currentRow = () => layer.GetComponentsInChildren<CharBuildSelectorItem>(true).SingleOrDefault(value =>
                    value.gameObject.activeInHierarchy && ReferenceEquals(value.BlueprintAbility, source) && value.Toggle.interactable);
                foreach (int frame in WaitTeleportLevelUpUi(() => currentRow() != null, "native PC disclosed original source", 4)) yield return frame;
                var row = currentRow();
                foreach (int frame in screens.CaptureRow("elementals-desktop-native-learning-discovery", (RectTransform)row.transform,
                    () => new JObject { ["source"] = source.AssetGuid, ["book"] = book.Blueprint.AssetGuid, ["spellLevel"] = row.SpellLevel,
                        ["nativeText"] = new JArray(row.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value => value.text)),
                        ["previewOnly"] = !backend.AutoCommit && !ReferenceEquals(backend.Preview, unit.Descriptor), ["pendingAllocations"] = pending.Count(value => value != null) },
                    () => build.IsShow && ReferenceEquals(build.LevelUpController, backend) && ReferenceEquals(currentRow(), row) &&
                        ReferenceEquals(build.Spells.CurrentSpellSelectionData, selection) && Cleanup2NoNarrativeOverlay())) yield return frame;
                // Toggle.isOn invokes the exact native callback captured above;
                // no SelectSpell, source substitution or artificial allocation.
                if (!ReferenceEquals(currentRow(), row) || row.Toggle.isOn) throw new InvalidOperationException("Native source row recycled or already selected before click.");
                row.Toggle.isOn = true;
                foreach (int frame in WaitTeleportLevelUpUi(() => backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(source), "native PC original-source preview allocation")) yield return frame;
                bool allocated = !book.IsKnown(source) && book.GetAllKnownSpells().SequenceEqual(known) && Cleanup2ElementalResources(unit) == resources &&
                    Cleanup2DesktopPending(selection).Length == pending.Length &&
                    Cleanup2DesktopPending(selection).Where((value, index) => !ReferenceEquals(value, pending[index])).Count() == 1 &&
                    Cleanup2DesktopPending(selection).Where((value, index) => !ReferenceEquals(value, pending[index])).All(value => ReferenceEquals(value, source)) &&
                    Cleanup2DesktopPending(selection).Count(value => value != null) == pending.Count(value => value != null) + 1 &&
                    Cleanup2DesktopPending(selection).Count(value => ReferenceEquals(value, source)) == pending.Count(value => ReferenceEquals(value, source)) + 1 && commits == 0;
                Cleanup2Check("elemental-desktop-native-learning-allocation", allocated, new JObject { ["source"] = source.AssetGuid,
                    ["previewKnowsOriginal"] = backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(source), ["originalKnows"] = book.IsKnown(source),
                    ["allocatedBefore"] = pending.Count(value => value != null), ["allocatedAfter"] = Cleanup2DesktopPending(selection).Count(value => value != null),
                    ["commits"] = commits, ["nativeInput"] = "CharBuildSelectorItem.Toggle -> native OnValueChanged -> original Action -> CharacterBuildController.SetSpell" },
                    "One real PC selector click allocates exactly one original source in the native independent preview; the original actor and budget remain unchanged.");
                if (!allocated) throw new InvalidOperationException("Native PC original-source allocation failed.");
                foreach (int frame in screens.Capture("elementals-desktop-native-learning-allocated", new JObject { ["source"] = source.AssetGuid,
                    ["previewKnows"] = backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(source), ["originalKnows"] = book.IsKnown(source),
                    ["nativeText"] = new JArray(layer.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value => value.text)) },
                    () => build.IsShow && ReferenceEquals(build.LevelUpController, backend) && backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(source) &&
                        !book.IsKnown(source) && Cleanup2NoNarrativeOverlay())) yield return frame;
                build.OnHotKeyEscPressed();
                foreach (int frame in WaitTeleportLevelUpUi(() => DialogMessageBox.Instance.IsShown, "native PC learning cancellation confirmation")) yield return frame;
                var callback = (Action<DialogMessageBoxBase.BoxButton>)WorldMapPointSpellActionPatches.ConfirmationCallbackField.GetValue(DialogMessageBox.Instance);
                if (callback == null || !ReferenceEquals(callback.Target, build)) throw new InvalidOperationException("Foreign PC cancellation dialog owner.");
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                foreach (int frame in WaitTeleportLevelUpUi(() => !build.IsShow && !DialogMessageBox.Instance.IsShown, "native PC learning cancelled")) yield return frame;
                backend.Cancel();
                if (ReferenceEquals(ui.LevelUpController, backend)) ui.LevelUpController = globalBefore;
                backend = null; build.Unit = originalBuildUnit;
                bool cancelled = commits == 0 && unit.Descriptor.Progression.GetClassLevel(wizard) == 2 && !book.IsKnown(source) &&
                    book.GetAllKnownSpells().SequenceEqual(known) && Cleanup2ElementalResources(unit) == resources && build.LevelUpController == null && ReferenceEquals(ui.LevelUpController, globalBefore);
                Cleanup2Check("elemental-desktop-native-learning-cancel", cancelled, new JObject { ["commits"] = commits,
                    ["classLevel"] = unit.Descriptor.Progression.GetClassLevel(wizard), ["originalKnownReferencesExact"] = book.GetAllKnownSpells().SequenceEqual(known),
                    ["originalResourcesExact"] = Cleanup2ElementalResources(unit) == resources, ["nativeInput"] = "Native Esc close + exact owned confirmation Yes; owned native preview Cancel after presenter closes",
                    ["scope"] = "Cancellation undoes the pending native allocation. No claim that turning an already-selected PC candidate Toggle off is a native unlearn action." },
                    "Actual native PC cancellation commits nothing, retains Wizard2 and original source/budget references, and closes only its owned preview.");
                if (!cancelled) throw new InvalidOperationException("Native PC original-source cancellation failed.");
            }
            finally
            {
                bool foreign = false;
                if (backend != null)
                {
                    if ((ui.LevelUpController != null && !ReferenceEquals(ui.LevelUpController, backend)) ||
                        (build.LevelUpController != null && !ReferenceEquals(build.LevelUpController, backend))) foreign = true;
                    else
                    {
                        var dialog = DialogMessageBox.Instance;
                        var callback = dialog == null ? null : (Action<DialogMessageBoxBase.BoxButton>)WorldMapPointSpellActionPatches.ConfirmationCallbackField.GetValue(dialog);
                        if (dialog != null && dialog.IsShown && callback != null && ReferenceEquals(callback.Target, build)) dialog.HandleForceClose();
                        if (build.IsShow && ReferenceEquals(build.LevelUpController, backend)) build.Show(false);
                        backend.Cancel();
                        if (ReferenceEquals(ui.LevelUpController, backend)) ui.LevelUpController = globalBefore;
                        if (build.LevelUpController == null && ReferenceEquals(build.Unit, unit?.Descriptor)) build.Unit = originalBuildUnit;
                    }
                }
                if (!foreign)
                {
                    if (unit != null) { CleanupExpandedSummoningUnit(unit); fixture.Created.Remove(unit); game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick(); }
                    if (blueprint != null) UnityEngine.Object.Destroy(blueprint);
                }
                else throw new InvalidOperationException("Native PC learning ownership changed; foreign creator and referenced actor remain untouched for fixture refusal cleanup.");
            }
            for (int frame = 0; frame < 8; frame++) yield return 0;
        }
    }
}
