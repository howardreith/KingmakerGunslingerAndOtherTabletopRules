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
                !Cleanup2LearningModuleState(Cleanup2OffState()))
                throw new InvalidOperationException("Exact stable ON or fresh-OFF automatic-exit desktop learning phase required.");
            bool off = Cleanup2OffState();
            var game = Game.Instance; var ui = game.UI; var player = game.Player; var group = GroupController.Instance;
            var build = ui.CharacterBuildController;
            if (game.IsControllerGamepad || !Cleanup2NoNarrativeOverlay() || group == null || ui.SelectionManagerPC == null ||
                ui.ServiceWindow.WindowTabs.IsShow || ui.SpellBookController.IsShow || build == null || build.IsShow ||
                build.LevelUpController != null || DialogMessageBox.Instance == null ||
                DialogMessageBox.Instance.IsShown || ui.TooltipsController.ContextTooltipData != null || !fixture.Caster.Commands.Empty)
                throw new InvalidOperationException("Closed idle native desktop book/creator and no foreign modal required.");
            var party = player.Party.ToArray(); var selected = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var originalGroup = group.GetCurrentCharacter(); var buildUnit = build.Unit;
            var inputs = party.Select(value => new Cleanup2TooltipInputs(value)).ToArray();
            var bars = party.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var levelUpIntake = new Cleanup2SourcesLevelUpIntake(game);
            bool added = false, selectedFixture = false;
            // In-memory only; FeatureModules.json is never written by this phase.
            bool showOriginalElementals = OriginalSummonElementalVisibility.Show;
            var mode = Cleanup2EnterNativeDefault(fixture, "elementals-learning");
            var screens = new NativeIconScreenEvidence(_request); var watch = Stopwatch.StartNew();
            Exception primaryFailure = null; string primaryPhase = null;
            Action<string, Exception> observeFailure = (phase, error) => {
                if (primaryFailure == null) { primaryFailure = error; primaryPhase = phase; }
                Cleanup2Check("elemental-desktop-ui-execution-" + phase, false, new JObject {
                    ["phase"] = phase, ["exception"] = error.ToString(), ["freshModuleOff"] = off,
                    ["scope"] = "Original owned desktop UI step exception recorded before outer restoration; no state repaired or assertion waived." },
                    "Owned native desktop preparation and learning steps complete without an exception.");
            };
            try
            {
                try
                {
                    JObject levelUpReceipt;
                    bool completedIntake = levelUpIntake.Current(out levelUpReceipt);
                    Cleanup2Check("elemental-desktop-native-levelup-intake", completedIntake, levelUpReceipt,
                        "The exact original native controller field may retain only a completed disposed preview; active builders, producer work or replacements refuse before learning.");
                    if (!completedIntake) throw new InvalidOperationException("Native desktop learning intake is not demonstrably completed/idle; inspect exact lifecycle predicates.");
                    // Native Pause is a separate mode and would invalidate the
                    // exact Default lease. Native service/creator screens own
                    // their normal child modes; no observer Pause is requested.
                    if (!party.Contains(fixture.Caster))
                    { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                    ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster); selectedFixture = true;
                }
                catch (Exception error) { observeFailure("intake-selection", error); throw; }
                for (int frame = 0; frame < 8; frame++) yield return 0;
                // Targeted itinerary (owner, 2026-11-12): the still-unobserved
                // mixed book/metamagic cell runs first and must restore its own
                // fixture; then one revealed level-up session for the live-state
                // replacement and normal cancellation. Hidden learning and
                // reveal-after-reopen already have evidence on 0d944a262f.
                _cleanup2MixedFixtureRestored = off;
                foreach (int frame in Cleanup2DesktopObservedSteps(Cleanup2DesktopPrepareAndUndo(fixture, book, screens), "preparation", observeFailure)) yield return frame;
                if (!_cleanup2MixedFixtureRestored)
                { var error = new InvalidOperationException("Mixed book fixture restoration is unverified; learning is not started."); observeFailure("mixed-fixture-boundary", error); throw error; }
                if (!mode.Current || !Cleanup2LearningModuleState(off) || watch.Elapsed.TotalSeconds > 150)
                { var error = new InvalidOperationException("Desktop book/metamagic boundary expired or changed."); observeFailure("preparation-boundary", error); throw error; }
                foreach (int frame in Cleanup2DesktopObservedSteps(Cleanup2DesktopLearnAndCancel(fixture, book, screens, levelUpIntake), "learning", observeFailure)) yield return frame;
                if (!mode.Current || !Cleanup2LearningModuleState(off) || watch.Elapsed.TotalSeconds > 260)
                { var error = new InvalidOperationException("Desktop learning phase exceeded its bounded native UI scope."); observeFailure("learning-boundary", error); throw error; }
            }
            finally
            {
                    var errors = new List<string>();
                OriginalSummonElementalVisibility.Show = showOriginalElementals;
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
                    JObject levelUpReceipt;
                    bool levelUpRestored = levelUpIntake.Current(out levelUpReceipt);
                    var predicates = new JObject {
                        ["noRestorationErrors"] = errors.Count == 0, ["partyExact"] = player.Party.SequenceEqual(party),
                        ["selectionExact"] = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected),
                        ["groupCurrentExact"] = ReferenceEquals(group.GetCurrentCharacter(), originalGroup),
                        ["originalSourcesExact"] = inputs.All(value => value.Unchanged()), ["originalPinsExact"] = bars.All(value => value.IsRestored()),
                        ["creatorHidden"] = !build.IsShow, ["creatorBackendAbsent"] = build.LevelUpController == null,
                        ["originalCompletedControllerExact"] = levelUpRestored, ["creatorUnitExact"] = ReferenceEquals(build.Unit, buildUnit),
                        ["nativeServiceTabsHidden"] = !ui.ServiceWindow.WindowTabs.IsShow, ["nativeBookHidden"] = !ui.SpellBookController.IsShow,
                        ["confirmationModalHidden"] = !DialogMessageBox.Instance.IsShown, ["moduleStateStable"] = Cleanup2LearningModuleState(off),
                        ["originalElementalOptionRestored"] = OriginalSummonElementalVisibility.Show == showOriginalElementals };
                    var rejected = predicates.Properties().Where(value => !(bool)value.Value).Select(value => value.Name).ToArray();
                    bool exact = rejected.Length == 0;
                    Cleanup2Check("elemental-desktop-learning-restored", exact, new JObject {
                        ["partyExact"] = player.Party.SequenceEqual(party), ["selectionExact"] = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected),
                        ["originalSourcesAndPinsExact"] = inputs.All(value => value.Unchanged()) && bars.All(value => value.IsRestored()),
                        ["creatorClosed"] = !build.IsShow && build.LevelUpController == null && levelUpRestored,
                        ["originalCompletedNativeControllerExact"] = levelUpReceipt,
                        ["creatorUnitExact"] = ReferenceEquals(build.Unit, buildUnit), ["errors"] = new JArray(errors), ["freshModuleOff"] = off,
                        ["predicates"] = predicates, ["rejectedPredicates"] = new JArray(rejected),
                        ["originalExecutionPhase"] = primaryPhase, ["originalExecutionException"] = primaryFailure?.ToString(),
                        ["optionShowOriginalSummonElemental"] = OriginalSummonElementalVisibility.Show,
                        ["scope"] = "Save-free targeted itinerary: the mixed book and metamagic tab (hidden, revealed), one original-source preparation/unprepare and the mixed-fixture restoration first; then one revealed level-up session on a disposable Wizard2 (ordinary row, native selection, live-state native replacement, normal cancellation). No committed character, save hydration, console or casting claim." },
                        "Owned native screens close and original party, selection, creator owner, source objects and pins remain exact.");
                    if (!exact) throw new InvalidOperationException("Desktop learning restoration is unverified: " + string.Join("|", rejected), primaryFailure);
                }
                finally { try { screens.Dispose(); } finally { mode.Dispose(); } }
            }
        }

        private static IEnumerable<int> Cleanup2DesktopObservedSteps(IEnumerable<int> steps,string phase,Action<string,Exception> observe)
        {
            using(var iterator=steps.GetEnumerator())
            {
                while(true)
                {
                    bool more;
                    try{more=iterator.MoveNext();}
                    catch(Exception error){observe(phase,error);throw;}
                    if(!more)yield break;
                    yield return iterator.Current;
                }
            }
        }

        private bool Cleanup2LearningModuleState(bool off)
        { return off ? Cleanup2OffState() : _context.FeatureModules.Active.ExpandedSummoning &&
            ExpandedSummoningVisualVariantPatch.IsModuleActive && ElementalSourceChooserRuntime.Active; }

        // The fold component type was deleted with the learning disclosure
        // (2026-11-12); its header object name is the stable absence probe.
        private static bool Cleanup2DesktopNoOwnedDisclosure(Component owner)
        { return owner != null && !owner.GetComponentsInChildren<Transform>(true).Any(value => value.name == "KMG_ElementalSourceDisclosure"); }

        // Native level tab change and return: SpellBookView rebuilds its list
        // through GetSpellsForLevel; no observer list or visibility writes.
        private static IEnumerable<int> Cleanup2DesktopBookLevelRoundTrip(SpellBookView view, SpellBookController controller, Spellbook book)
        {
            var other = view.LevelBookSwitcher.m_Tabs[1]; var level = view.LevelBookSwitcher.m_Tabs[2];
            if (!other.gameObject.activeInHierarchy || !other.Toggle.interactable) throw new InvalidOperationException("Native desktop level1 tab unavailable.");
            other.Toggle.isOn = true; for (int frame = 0; frame < 8; frame++) yield return 0;
            if (controller.CurrentBookLevel != 1) throw new InvalidOperationException("Native desktop level1 tab did not select.");
            level.Toggle.isOn = true; for (int frame = 0; frame < 8; frame++) yield return 0;
            // Native level selection may retain a valid page index; rewind.
            for (int back = 0; controller.CurrentPageIndex > 0 && back < 30; back++) { view.GoPrevPage(); for (int frame = 0; frame < 3; frame++) yield return 0; }
            if (!ReferenceEquals(controller.CurrentSpellbook, book) || controller.CurrentBookLevel != 2 || controller.CurrentPageIndex != 0)
                throw new InvalidOperationException("Native desktop level2 return differs.");
        }

        // Native favorites (metamagic) tab through its real Toggle.
        private static IEnumerable<int> Cleanup2DesktopFavorites(SpellBookView view, SpellBookController controller)
        {
            var toggle = view.LevelBookSwitcher.ToggleFavorites;
            if (toggle == null || !toggle.gameObject.activeInHierarchy || !toggle.interactable) throw new InvalidOperationException("Native favorites tab unavailable.");
            toggle.isOn = true;
            foreach (int frame in WaitTeleportLevelUpUi(() => view.CurrentBookLevel == view.FavoriteBookmarkIndex, "native PC favorites tab")) yield return frame;
            for (int frame = 0; frame < 8; frame++) yield return 0;
        }

        private static List<AbilityData> Cleanup2DesktopFavoriteRows(SpellBookView view)
        { return ((List<AbilityData>)typeof(SpellBookView).GetField("m_ShowSpellsList", Cleanup2Fields).GetValue(view) ?? new List<AbilityData>()).ToList(); }

        private static List<AbilityData> Cleanup2DesktopRenderedRows(SpellBookView view)
        { return view.GetComponentsInChildren<SpellItem>(true).Where(value => value.gameObject.activeInHierarchy && value.SpellData != null).Select(value => value.SpellData).ToList(); }

        // Visits every native page of the current level once, from page 0,
        // then returns to page 0.
        private static IEnumerable<int> Cleanup2DesktopBookPages(SpellBookView view, SpellBookController controller, Action visit)
        {
            int level = controller.CurrentBookLevel;
            for (int back = 0; controller.CurrentPageIndex > 0 && back < 30; back++) { view.GoPrevPage(); for (int frame = 0; frame < 3; frame++) yield return 0; }
            int last = (int)typeof(SpellBookView).GetMethod("MaxPageOnLevelIndex", Cleanup2Fields).Invoke(view, new object[] { level });
            for (int page = 0; ; page++)
            {
                if (controller.CurrentBookLevel != level || controller.CurrentPageIndex != page) throw new InvalidOperationException("Native desktop page walk lost its level/page.");
                visit();
                if (page >= last || page >= 30) break;
                view.GoNextPage(); for (int frame = 0; frame < 4; frame++) yield return 0;
            }
            for (int back = 0; controller.CurrentPageIndex > 0 && back < 30; back++) { view.GoPrevPage(); for (int frame = 0; frame < 3; frame++) yield return 0; }
            if (controller.CurrentBookLevel != level || controller.CurrentPageIndex != 0) throw new InvalidOperationException("Native desktop page walk did not return to page 0.");
        }

        // Native slot change and return (a second spell slot of the same
        // selection, then the original). Installed FillDataAllSpells only runs
        // FillSpellLightUpdate for an unchanged spell list: this is a native
        // selection refresh of cached rows, not a list rebuild, and it does
        // not rerun the learning filter (native finding on 312bb462e8).
        private static IEnumerable<int> Cleanup2DesktopLearningRoundTrip(CharacterBuildController build, CharBSelectionSwitchItem other,
            CharBSelectionSwitchItem target, Func<CharBSelectorLayer> layer)
        {
            foreach (var item in new[] { other, target })
            {
                if (item == null || !item.gameObject.activeInHierarchy || !item.Toggle.interactable)
                    throw new InvalidOperationException("Native learning collection switch unavailable.");
                item.Toggle.isOn = true;
                foreach (int frame in WaitTeleportLevelUpUi(() => build.Spells.CurrentSpellsCollectionIndex == item.SpellIndex && layer() != null &&
                    layer().SelectorItems.Any(value => value.gameObject.activeInHierarchy && value.BlueprintAbility != null), "native PC learning collection switch")) yield return frame;
                // FillSpellLevel rebuilds rows inside the native fade callback.
                for (int frame = 0; frame < 30; frame++) yield return 0;
            }
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

        private static int Cleanup2DesktopScreenIndex(ServiceWindowTabs tabs)
        { return (int)typeof(FullScreenTabsWindow).GetProperty("ScreenIndex", Cleanup2Fields).GetValue(tabs, null); }

        private IEnumerable<int> Cleanup2DesktopPrepareAndUndo(ExpandedSummoningCorrectionFixture fixture, Spellbook book, NativeIconScreenEvidence screens)
        {
            var game = Game.Instance; var ui = game.UI; var controller = ui.SpellBookController;
            var service = ui.ServiceWindow; var tabs = service.WindowTabs; var group = GroupController.Instance;
            var selection = ui.SelectionManagerPC; bool off = Cleanup2OffState(), closeRequested = false;
            Func<bool> ownsUi = () => ReferenceEquals(Game.Instance, game) && ReferenceEquals(game.UI, ui) &&
                ReferenceEquals(ui.ServiceWindow, service) && ReferenceEquals(service.WindowTabs, tabs) &&
                ReferenceEquals(ui.SpellBookController, controller) && ReferenceEquals(ui.SelectionManagerPC, selection) &&
                ReferenceEquals(GroupController.Instance, group) && group != null && ReferenceEquals(group.GetCurrentCharacter(), fixture.Caster) &&
                selection.SelectedUnits.Count == 1 && ReferenceEquals(selection.SelectedUnits.First(), fixture.Caster) && !game.IsControllerGamepad;
            Action requestClose = () => {
                if (ownsUi() && controller.IsShow && ReferenceEquals(controller.CurrentCharacter, fixture.Caster) &&
                    ReferenceEquals(controller.CurrentSpellbook, book) && tabs.IsShow && Cleanup2DesktopScreenIndex(tabs) == 2)
                { closeRequested = true; service.HandleOpenSpellbook(); }
            };
            System.Runtime.ExceptionServices.ExceptionDispatchInfo failure = null;
            using (var iterator = Cleanup2DesktopPrepareAndUndoSteps(fixture, book, screens, requestClose).GetEnumerator())
            {
                while (true)
                {
                    bool more = false;
                    try { more = iterator.MoveNext(); }
                    catch (Exception error) { failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error); }
                    if (failure != null || !more) break;
                    yield return iterator.Current;
                }
            }
            if (closeRequested)
            {
                // Installed ServiceWindowTabs.Show(false) clears only its own
                // IsShow. DisappearAnimation's unscaled 0.4s OnComplete -> Hide
                // then calls the child book's native Show(false)/OnHideInternal.
                // Keep the original exception pending while that owned callback
                // completes; never assign IsShow or invoke Hide ahead of it.
                Func<bool> ownsClose = () => ownsUi() && Cleanup2LearningModuleState(off) && !tabs.IsShow && Cleanup2DesktopScreenIndex(tabs) == -1 &&
                    (!controller.IsShow || ReferenceEquals(controller.CurrentCharacter, fixture.Caster) && ReferenceEquals(controller.CurrentSpellbook, book));
                var watch = Stopwatch.StartNew(); int frames = 0;
                while (ownsClose() && controller.IsShow && frames < 600 && watch.Elapsed.TotalSeconds < 8)
                { frames++; yield return 0; }
                bool owned = ownsClose(), closed = owned && !controller.IsShow;
                Cleanup2Check("elemental-desktop-native-book-close", closed, new JObject {
                    ["nativeCloseRequested"] = closeRequested, ["ownershipExact"] = owned,
                    ["nativeServiceTabsHidden"] = !tabs.IsShow, ["nativeBookHidden"] = !controller.IsShow,
                    ["waitedFrames"] = frames, ["waitedMilliseconds"] = watch.Elapsed.TotalMilliseconds,
                    ["originalExecutionException"] = failure?.SourceException.ToString(),
                    ["contract"] = "ServiceWindowTabs.DisappearAnimation 06004779 -> native unscaled 0.4s completion -> Hide 0600477A -> child Show(false)/OnHideInternal; no observer UI state assignment." },
                    "The exact owned native service-window close finishes before outer party/selection restoration, including after a preparation failure.");
                if (!closed) throw new InvalidOperationException("Owned native desktop spellbook close did not complete within its frame/wall bound.", failure?.SourceException);
            }
            if (failure != null) failure.Throw();
        }

        private IEnumerable<int> Cleanup2DesktopPrepareAndUndoSteps(ExpandedSummoningCorrectionFixture fixture, Spellbook book,
            NativeIconScreenEvidence screens, Action requestClose)
        {
            bool off = Cleanup2OffState();
            var game = Game.Instance; var ui = game.UI; var controller = ui.SpellBookController;
            var sourceBlueprint = Cleanup2ElementalAbility(fixture, ElementalSourceChooserPolicy.Rank(2).Guid);
            if (!book.IsKnown(sourceBlueprint)) book.AddKnown(2, sourceBlueprint, true);
            // Disposable mixed fixture (module ON only): one ordinary level-2
            // control spell plus Extend metamagic copies of the source and the
            // control for the favorites tab. Removed explicitly in finally.
            Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility controlBlueprint = null; bool controlAdded = false;
            AbilityData customSource = null, customControl = null;
            var preMixedKnown = book.GetAllKnownSpells().ToArray();
            Func<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility, AbilityData> extended = blueprint => {
                var metamagic = new MetamagicData { SpellLevelCost = Metamagic.Extend.DefaultCost() }; metamagic.Add(Metamagic.Extend);
                return new AbilityData(blueprint, book) { MetamagicData = metamagic }; };
            if (!off)
            {
                controlBlueprint = book.Blueprint.SpellList.GetSpells(2).Where(value => value != null &&
                    !StandaloneElementalPresentationPolicy.IsOriginalSummonElemental(value.AssetGuid) && !book.IsKnown(value) &&
                    !(value.ComponentsArray ?? new BlueprintComponent[0]).Any(component => component != null && component.GetType().Name == "UniqueSpellComponent"))
                    .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
                if (controlBlueprint == null) throw new InvalidOperationException("No ordinary unknown level-2 Wizard control spell for the mixed book fixture.");
                book.AddKnown(2, controlBlueprint, true); controlAdded = true;
                customSource = extended(sourceBlueprint); book.AddCustomSpell(customSource);
                customControl = extended(controlBlueprint); book.AddCustomSpell(customControl);
            }
            var known = book.GetAllKnownSpells().ToArray(); var source = known.Single(value => ReferenceEquals(value.Blueprint, sourceBlueprint) && !value.HasMetamagic(Metamagic.Extend));
            var control = controlBlueprint == null ? null : known.Single(value => ReferenceEquals(value.Blueprint, controlBlueprint) && value.MetamagicData == null);
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
                var view = controller.SpellBookView; var classToggle = view.ClassToggle;
                int bookIndex = classToggle.Spellbooks.IndexOf(book); bool singleton = classToggle.Spellbooks.Count == 1;
                bool nativeSelection = false;
                if (ReferenceEquals(controller.CurrentCharacter, fixture.Caster) && bookIndex >= 0)
                {
                    // Installed SpellBookToggle.SetUp 0600481E hides the entire
                    // class strip when Count <= 1. Its native current book must
                    // already be our sole exact book; there is no visible click.
                    if (singleton)
                        nativeSelection = bookIndex == 0 && ReferenceEquals(classToggle.Spellbooks[0], book) &&
                            !classToggle.gameObject.activeSelf && !classToggle.gameObject.activeInHierarchy &&
                            ReferenceEquals(controller.CurrentSpellbook, book);
                    else
                    {
                        var matches = classToggle.GetComponentsInChildren<SpellbookClassTab>(true)
                            .Where(value => value.Index == bookIndex && value.gameObject.activeInHierarchy).ToArray();
                        if (matches.Length == 1 && matches[0].Toggle != null && matches[0].Toggle.interactable)
                        { matches[0].Toggle.isOn = true; nativeSelection = ReferenceEquals(controller.CurrentSpellbook, book); }
                    }
                }
                Cleanup2Check("elemental-desktop-native-book-selection", nativeSelection, new JObject {
                    ["nativeBookCount"] = classToggle.Spellbooks.Count, ["sourceBookIndex"] = bookIndex,
                    ["currentCharacterExact"] = ReferenceEquals(controller.CurrentCharacter, fixture.Caster),
                    ["currentBookExact"] = ReferenceEquals(controller.CurrentSpellbook, book),
                    ["classToggleActiveSelf"] = classToggle.gameObject.activeSelf, ["classToggleActiveInHierarchy"] = classToggle.gameObject.activeInHierarchy,
                    ["nativeInput"] = singleton ? "Native open selected the sole exact book; no class-tab click." : "Exact visible class-tab Toggle -> native OnToggle." },
                    "The native PC book belongs to the exact fixture; a single-book class strip is natively hidden, and multiple books use their visible original tab.");
                if (!nativeSelection) throw new InvalidOperationException("Native desktop source book/class-tab ownership differs.");
                var level = view.LevelBookSwitcher.m_Tabs[2];
                if (!level.gameObject.activeInHierarchy || !level.Toggle.interactable) throw new InvalidOperationException("Native desktop level2 tab unavailable.");
                level.Toggle.isOn = true;
                for (int frame = 0; frame < 8; frame++) yield return 0;
                if (!ReferenceEquals(controller.CurrentSpellbook, book) || controller.CurrentBookLevel != 2) throw new InvalidOperationException("Native desktop source book/level differs.");
                if (!off)
                {
                    // Owner directive (2026-11-12): the default option hides
                    // every original Summon Elemental row, here including the
                    // known source just granted to this disposable book.
                    OriginalSummonElementalVisibility.Show = false;
                    foreach (int frame in Cleanup2DesktopBookLevelRoundTrip(view, controller, book)) yield return frame;
                    var listed = view.GetSpellsForLevel(2);
                    var rendered = new List<AbilityData>(); int pages = 0;
                    foreach (int frame in Cleanup2DesktopBookPages(view, controller, () => {
                        pages++;
                        rendered.AddRange(view.GetComponentsInChildren<SpellItem>(true).Where(value => value.gameObject.activeInHierarchy && value.SpellData != null)
                            .Select(value => value.SpellData)); })) yield return frame;
                    Func<IEnumerable<AbilityData>, int> originals = rows => rows.Count(value => value?.Blueprint != null &&
                        StandaloneElementalPresentationPolicy.IsOriginalSummonElemental(value.Blueprint.AssetGuid));
                    // Identities are checked directly; BookOutcome is diagnostic only.
                    bool hidden = book.IsKnown(sourceBlueprint) && book.IsKnown(controlBlueprint) && book.GetAllKnownSpells().SequenceEqual(known) &&
                        !listed.Any(value => ReferenceEquals(value, source)) && originals(listed) == 0 &&
                        listed.Count(value => ReferenceEquals(value, control)) == 1 &&
                        !view.GetSpellsForLevel(3).Any(value => ReferenceEquals(value, customSource)) &&
                        view.GetSpellsForLevel(3).Count(value => ReferenceEquals(value, customControl)) == 1 &&
                        originals(rendered) == 0 && rendered.Count(value => ReferenceEquals(value, control)) == 1 && pages > 0 &&
                        Cleanup2DesktopNoOwnedDisclosure(view) && slots.Select(value => value.Spell).SequenceEqual(spells) && Cleanup2LearningModuleState(off);
                    Cleanup2Check("elemental-desktop-book-mixed-hidden", hidden, new JObject { ["source"] = sourceBlueprint.AssetGuid,
                        ["control"] = controlBlueprint.AssetGuid, ["sourceKnown"] = book.IsKnown(sourceBlueprint), ["controlKnown"] = book.IsKnown(controlBlueprint),
                        ["knownObjectsExact"] = book.GetAllKnownSpells().SequenceEqual(known), ["nativeListed"] = new JArray(listed.Select(value => value.Blueprint.AssetGuid)),
                        ["nativeListedOriginalElementals"] = originals(listed), ["sourceObjectListed"] = listed.Any(value => ReferenceEquals(value, source)),
                        ["controlObjectListed"] = listed.Count(value => ReferenceEquals(value, control)),
                        ["renderedOriginalElementals"] = originals(rendered), ["renderedControl"] = rendered.Count(value => ReferenceEquals(value, control)),
                        ["pagesInspected"] = pages, ["noOwnedDisclosure"] = Cleanup2DesktopNoOwnedDisclosure(view), ["bookOutcome"] = OriginalSummonElementalVisibility.BookOutcome,
                        ["level3CustomSourceListed"] = view.GetSpellsForLevel(3).Any(value => ReferenceEquals(value, customSource)),
                        ["level3CustomControlListed"] = view.GetSpellsForLevel(3).Count(value => ReferenceEquals(value, customControl)),
                        ["level3Scope"] = "Level-3 (metamagic copy) list read through the patched native method; level-2 rows are rendered and page-walked." },
                        "With the default option the PC book's level-2 list and rendered pages omit the known original source object while the ordinary control remains exactly once.");
                    if (!hidden) throw new InvalidOperationException("Native PC mixed book hiding failed.");
                    foreach (int frame in screens.Capture("elementals-desktop-book-mixed-hidden", new JObject { ["source"] = sourceBlueprint.AssetGuid, ["control"] = controlBlueprint.AssetGuid,
                        ["nativeText"] = new JArray(view.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value => value.text)) },
                        () => controller.IsShow && ReferenceEquals(controller.CurrentSpellbook, book) && controller.CurrentBookLevel == 2 && Cleanup2NoNarrativeOverlay())) yield return frame;
                    // Metamagic (favorites) tab through its real native toggle.
                    foreach (int frame in Cleanup2DesktopFavorites(view, controller)) yield return frame;
                    var favorites = Cleanup2DesktopFavoriteRows(view); var favoriteRendered = Cleanup2DesktopRenderedRows(view);
                    bool favoritesHidden = favorites.Count(value => ReferenceEquals(value, customControl)) == 1 && !favorites.Any(value => ReferenceEquals(value, customSource)) &&
                        originals(favorites) == 0 && originals(favoriteRendered) == 0 &&
                        favoriteRendered.Count(value => ReferenceEquals(value, customControl)) == 1 &&
                        book.GetCustomSpells(3).Count(value => ReferenceEquals(value, customSource)) == 1 && book.GetCustomSpells(3).Count(value => ReferenceEquals(value, customControl)) == 1 &&
                        OriginalSummonElementalVisibility.FavoritesSeamState == "installed-exact-favorites-call-copy" && Cleanup2DesktopNoOwnedDisclosure(view);
                    Cleanup2Check("elemental-desktop-favorites-mixed-hidden", favoritesHidden, new JObject { ["customSourceLevel"] = book.GetSpellLevel(customSource),
                        ["favoriteRows"] = new JArray(favorites.Select(value => value.Blueprint.AssetGuid + (value.MetamagicData == null ? "" : ":" + value.MetamagicData.MetamagicMask))),
                        ["customControlListed"] = favorites.Count(value => ReferenceEquals(value, customControl)), ["customSourceListed"] = favorites.Any(value => ReferenceEquals(value, customSource)),
                        ["renderedOriginalElementals"] = originals(favoriteRendered), ["renderedCustomControl"] = favoriteRendered.Count(value => ReferenceEquals(value, customControl)),
                        ["customDataRetained"] = book.GetCustomSpells(3).Count(value => ReferenceEquals(value, customSource)) == 1,
                        ["seam"] = OriginalSummonElementalVisibility.FavoritesSeamState, ["outcome"] = OriginalSummonElementalVisibility.FavoritesOutcome,
                        ["nativeInput"] = "SpellBookLevelTabs.ToggleFavorites.isOn -> native SetUpFavorite" },
                        "With the default option the PC metamagic tab omits the original source's Extend copy while the ordinary control copy remains and both custom objects stay in the book.");
                    if (!favoritesHidden) throw new InvalidOperationException("Native PC metamagic tab hiding failed.");
                    foreach (int frame in screens.Capture("elementals-desktop-favorites-mixed-hidden", new JObject {
                        ["nativeText"] = new JArray(view.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value => value.text)) },
                        () => controller.IsShow && view.CurrentBookLevel == view.FavoriteBookmarkIndex && Cleanup2NoNarrativeOverlay())) yield return frame;
                    // Revealed: rebuild the tab natively (level tab, then favorites).
                    OriginalSummonElementalVisibility.Show = true;
                    view.LevelBookSwitcher.m_Tabs[2].Toggle.isOn = true; for (int frame = 0; frame < 8; frame++) yield return 0;
                    foreach (int frame in Cleanup2DesktopFavorites(view, controller)) yield return frame;
                    favorites = Cleanup2DesktopFavoriteRows(view); favoriteRendered = Cleanup2DesktopRenderedRows(view);
                    bool favoritesRevealed = favorites.Count(value => ReferenceEquals(value, customSource)) == 1 &&
                        favorites.Count(value => ReferenceEquals(value, customControl)) == 1 &&
                        favoriteRendered.Count(value => ReferenceEquals(value, customSource)) == 1 &&
                        favoriteRendered.Count(value => ReferenceEquals(value, customControl)) == 1 && Cleanup2DesktopNoOwnedDisclosure(view);
                    Cleanup2Check("elemental-desktop-favorites-revealed", favoritesRevealed, new JObject {
                        ["favoriteRows"] = new JArray(favorites.Select(value => value.Blueprint.AssetGuid + (value.MetamagicData == null ? "" : ":" + value.MetamagicData.MetamagicMask))),
                        ["customSourceListed"] = favorites.Count(value => ReferenceEquals(value, customSource)), ["customControlListed"] = favorites.Count(value => ReferenceEquals(value, customControl)),
                        ["renderedCustomSource"] = favoriteRendered.Count(value => ReferenceEquals(value, customSource)), ["renderedCustomControl"] = favoriteRendered.Count(value => ReferenceEquals(value, customControl)),
                        ["outcome"] = OriginalSummonElementalVisibility.FavoritesOutcome },
                        "Revealed, the metamagic tab lists the same original custom object and the control exactly once each.");
                    if (!favoritesRevealed) throw new InvalidOperationException("Native PC metamagic tab reveal failed.");
                    foreach (int frame in screens.Capture("elementals-desktop-favorites-revealed", new JObject {
                        ["nativeText"] = new JArray(view.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value => value.text)) },
                        () => controller.IsShow && view.CurrentBookLevel == view.FavoriteBookmarkIndex && Cleanup2NoNarrativeOverlay())) yield return frame;
                    // Back to the level-2 page: the same objects return as ordinary rows.
                    view.LevelBookSwitcher.m_Tabs[2].Toggle.isOn = true; for (int frame = 0; frame < 8; frame++) yield return 0;
                    foreach (int frame in Cleanup2DesktopBookLevelRoundTrip(view, controller, book)) yield return frame;
                    var revealedListed = view.GetSpellsForLevel(2); var revealedRendered = new List<AbilityData>(); int revealedPages = 0;
                    foreach (int frame in Cleanup2DesktopBookPages(view, controller, () => { revealedPages++; revealedRendered.AddRange(Cleanup2DesktopRenderedRows(view)); })) yield return frame;
                    bool bookRevealed = revealedListed.Count(value => ReferenceEquals(value, source)) == 1 && revealedListed.Count(value => ReferenceEquals(value, control)) == 1 &&
                        revealedRendered.Count(value => ReferenceEquals(value, source)) == 1 && revealedRendered.Count(value => ReferenceEquals(value, control)) == 1 &&
                        view.GetSpellsForLevel(3).Count(value => ReferenceEquals(value, customSource)) == 1 &&
                        view.GetSpellsForLevel(3).Count(value => ReferenceEquals(value, customControl)) == 1 && Cleanup2DesktopNoOwnedDisclosure(view);
                    Cleanup2Check("elemental-desktop-book-mixed-revealed", bookRevealed, new JObject { ["source"] = sourceBlueprint.AssetGuid, ["control"] = controlBlueprint.AssetGuid,
                        ["sourceListed"] = revealedListed.Count(value => ReferenceEquals(value, source)), ["controlListed"] = revealedListed.Count(value => ReferenceEquals(value, control)),
                        ["sourceRendered"] = revealedRendered.Count(value => ReferenceEquals(value, source)), ["controlRendered"] = revealedRendered.Count(value => ReferenceEquals(value, control)),
                        ["level3CustomSourceListed"] = view.GetSpellsForLevel(3).Count(value => ReferenceEquals(value, customSource)),
                        ["level3CustomControlListed"] = view.GetSpellsForLevel(3).Count(value => ReferenceEquals(value, customControl)),
                        ["pagesInspected"] = revealedPages, ["noOwnedDisclosure"] = Cleanup2DesktopNoOwnedDisclosure(view) },
                        "Revealed, the same original source and control objects are listed and rendered exactly once each on the level-2 pages, and both metamagic copies are listed once at level 3.");
                    if (!bookRevealed) throw new InvalidOperationException("Native PC mixed book reveal failed.");
                    foreach (int frame in screens.Capture("elementals-desktop-book-mixed-revealed", new JObject { ["source"] = sourceBlueprint.AssetGuid, ["control"] = controlBlueprint.AssetGuid,
                        ["nativeText"] = new JArray(view.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value => value.text)) },
                        () => controller.IsShow && ReferenceEquals(controller.CurrentSpellbook, book) && controller.CurrentBookLevel == 2 && Cleanup2NoNarrativeOverlay())) yield return frame;
                }
                for (int page = 0; page < 30 && !view.GetComponentsInChildren<SpellItem>(true).Any(value => ReferenceEquals(value.SpellData, source)); page++)
                {
                    int previousPage = controller.CurrentPageIndex;
                    view.GoNextPage();
                    for (int frame = 0; frame < 4; frame++) yield return 0;
                    if (!ReferenceEquals(controller.CurrentSpellbook, book) || controller.CurrentBookLevel != 2 || controller.CurrentPageIndex <= previousPage)
                        throw new InvalidOperationException("Native PC pagination cannot reach the original source on its exact level.");
                }
                var nativeRows = view.GetComponentsInChildren<SpellItem>(true).Where(value => value.SpellData != null).ToArray();
                var rowData = nativeRows.Select(value => value.SpellData).ToArray();
                // The spellbook disclosure was withdrawn (2026-11-11): module ON
                // with the option revealed, and fresh OFF, show the original
                // source as an ordinary visible native row with no disclosure.
                bool sourceRowVisible = nativeRows.Any(value => ReferenceEquals(value.SpellData, source) && value.gameObject.activeInHierarchy);
                bool disclosure = book.GetAllKnownSpells().SequenceEqual(known) && slots.Select(value => value.Spell).SequenceEqual(spells) &&
                    slots.Select(value => value.Available).SequenceEqual(available) && Cleanup2ElementalResources(fixture.Caster) == resources &&
                    nativeRows.Select(value => value.SpellData).SequenceEqual(rowData) && Cleanup2LearningModuleState(off) &&
                    Cleanup2DesktopNoOwnedDisclosure(view) && sourceRowVisible &&
                    (off || view.GetSpellsForLevel(2).Count(value => ReferenceEquals(value, source)) == 1 &&
                        view.GetSpellsForLevel(2).Count(value => ReferenceEquals(value, control)) == 1);
                Cleanup2Check("elemental-desktop-preparation-no-disclosure", disclosure, new JObject { ["source"] = sourceBlueprint.AssetGuid,
                    ["sourceBook"] = book.Blueprint.AssetGuid, ["knownObjectsExact"] = book.GetAllKnownSpells().SequenceEqual(known),
                    ["nativeRowAbilityReferencesExact"] = nativeRows.Select(value => value.SpellData).SequenceEqual(rowData),
                    ["slotsAndAvailabilityExact"] = slots.Select(value => value.Spell).SequenceEqual(spells) && slots.Select(value => value.Available).SequenceEqual(available),
                    ["freshModuleOff"] = off, ["noOwnedDisclosure"] = Cleanup2DesktopNoOwnedDisclosure(view), ["originalSourceRowVisible"] = sourceRowVisible,
                    ["optionRevealed"] = OriginalSummonElementalVisibility.Show, ["bookOutcome"] = OriginalSummonElementalVisibility.BookOutcome },
                    "The native PC book shows the original source as an ordinary visible row with no owned disclosure, without spending or replacing sources.");
                if (!disclosure) throw new InvalidOperationException("Native PC spellbook shows a withdrawn disclosure or hides the original source row.");
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
                if (opened) requestClose();
                if (written != null && ReferenceEquals(written.Spell, source)) book.ForgetMemorized(written);
                pins.Restore();
                var mixedErrors = new List<string>();
                try { if (customSource != null) book.RemoveCustomSpell(customSource); } catch (Exception error) { mixedErrors.Add("custom-source:" + error.Message); }
                try { if (customControl != null) book.RemoveCustomSpell(customControl); } catch (Exception error) { mixedErrors.Add("custom-control:" + error.Message); }
                try { if (controlAdded) book.RemoveSpell(controlBlueprint); } catch (Exception error) { mixedErrors.Add("control:" + error.Message); }
                if (!off)
                {
                    bool mixedRestored = mixedErrors.Count == 0 && !book.IsKnown(controlBlueprint) && book.IsKnown(sourceBlueprint) &&
                        !Enumerable.Range(0, 10).Any(value => book.GetCustomSpells(value).Any(row => ReferenceEquals(row, customSource) || ReferenceEquals(row, customControl))) &&
                        book.GetAllKnownSpells().SequenceEqual(preMixedKnown);
                    _cleanup2MixedFixtureRestored = mixedRestored;
                    Cleanup2Check("elemental-desktop-mixed-fixture-restored", mixedRestored, new JObject { ["errors"] = new JArray(mixedErrors),
                        ["controlKnown"] = controlBlueprint != null && book.IsKnown(controlBlueprint), ["knownExactPreMixed"] = book.GetAllKnownSpells().SequenceEqual(preMixedKnown) },
                        "The disposable control spell and both custom copies are removed; the fixture book returns to its pre-cell known objects.");
                }
            }
        }

        // One coherent reading of the live native level-up state for the
        // replacement cells: the current LevelUpState selection (exactly one
        // match required), current preview, current slot and current rows.
        private sealed class Cleanup2ReplacementSnapshot
        {
            internal int Matches, Slot = -1, SourceRows;
            internal Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility[] Extra;
            internal bool Owned, PreviewKnowsSource, PreviewKnowsReplacement, SourceRowOn, SourceRowInteractable, ReplacementRowOn;
            internal int SelectionIdentity, PreviewIdentity;

            internal static Cleanup2ReplacementSnapshot Read(LevelUpController backend, CharacterBuildController build,
                Func<SpellSelectionData, bool> wizardSelection, BlueprintCharacterClass wizard,
                Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility source, Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility replacement,
                Func<CharBSelectorLayer> liveLayer, Func<bool> owns)
            {
                var result = new Cleanup2ReplacementSnapshot();
                result.Owned = owns();
                var matches = backend?.State?.SpellSelections?.Where(wizardSelection).ToArray() ?? new SpellSelectionData[0];
                result.Matches = matches.Length;
                if (matches.Length == 1)
                {
                    result.Extra = matches[0].ExtraSelected?.ToArray();
                    result.SelectionIdentity = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(matches[0]);
                }
                var preview = backend?.Preview;
                var previewBook = preview?.GetSpellbook(wizard.Spellbook);
                result.PreviewIdentity = preview == null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(preview);
                result.PreviewKnowsSource = previewBook != null && previewBook.IsKnown(source);
                result.PreviewKnowsReplacement = replacement != null && previewBook != null && previewBook.IsKnown(replacement);
                try { result.Slot = build.Spells.CurrentSpellsCollectionIndex; } catch (Exception) { result.Slot = -1; }
                var layer = liveLayer();
                var rows = layer == null ? new CharBuildSelectorItem[0] : layer.GetComponentsInChildren<CharBuildSelectorItem>(true)
                    .Where(value => value.gameObject.activeInHierarchy && ReferenceEquals(value.BlueprintAbility, source)).ToArray();
                result.SourceRows = rows.Length;
                result.SourceRowOn = rows.Length == 1 && rows[0].Toggle.isOn;
                result.SourceRowInteractable = rows.Length == 1 && rows[0].Toggle.interactable;
                var replacementRows = layer == null || replacement == null ? new CharBuildSelectorItem[0] : layer.GetComponentsInChildren<CharBuildSelectorItem>(true)
                    .Where(value => value.gameObject.activeInHierarchy && ReferenceEquals(value.BlueprintAbility, replacement)).ToArray();
                result.ReplacementRowOn = replacementRows.Length == 1 && replacementRows[0].Toggle.isOn;
                return result;
            }

            internal JObject ToJson()
            {
                return new JObject { ["owned"] = Owned, ["matchingSelections"] = Matches, ["currentSlot"] = Slot,
                    ["pending"] = Extra == null ? null : new JArray(Extra.Select(value => value?.AssetGuid)),
                    ["freeSlots"] = Extra == null ? -1 : Extra.Count(value => value == null),
                    ["selectionIdentity"] = SelectionIdentity, ["previewIdentity"] = PreviewIdentity,
                    ["previewKnowsSource"] = PreviewKnowsSource, ["previewKnowsReplacement"] = PreviewKnowsReplacement,
                    ["sourceRows"] = SourceRows, ["sourceRowOn"] = SourceRowOn, ["sourceRowInteractable"] = SourceRowInteractable,
                    ["replacementRowOn"] = ReplacementRowOn };
            }
        }

        private bool _cleanup2MixedFixtureRestored;

        private static Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility[] Cleanup2DesktopPending(SpellSelectionData selection)
        { return (selection.ExtraSelected ?? new Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility[0]).Concat(
            (selection.LevelCount ?? new SpellSelectionData.SpellSelectionState[0]).Where(value => value != null && value.SpellSelections != null)
                .SelectMany(value => value.SpellSelections)).ToArray(); }

        // One disposable Wizard2 actor (created and destroyed once) and up to
        // two genuinely separate native level-up sessions on it. Installed
        // CharBFeatureSelector.FillDataAllSpells only light-updates a layer
        // whose spell list is unchanged, so the option is applied by closing
        // and reopening level-up, never by a same-list slot switch (native
        // finding on candidate 312bb462e8).
        private IEnumerable<int> Cleanup2DesktopLearnAndCancel(ExpandedSummoningCorrectionFixture fixture, Spellbook fixtureBook,
            NativeIconScreenEvidence screens, Cleanup2SourcesLevelUpIntake levelUpIntake)
        {
            bool off = Cleanup2OffState();
            var game = Game.Instance; var ui = game.UI; var build = ui.CharacterBuildController; var originalBuildUnit = build.Unit;
            var globalBefore = ui.LevelUpController; BlueprintUnit blueprint = null; UnitEntityData unit = null;
            LevelUpController seed = null;
            try
            {
                JObject intakeReceipt;
                if (!levelUpIntake.Current(out intakeReceipt))
                    throw new InvalidOperationException("Original native controller/producer changed before the owned PC learning actor is created: " + intakeReceipt);
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
                    seed = LevelUpController.StartWithoutAssigningStaticInstance(unit.Descriptor, false, null, null,
                        level == 0 ? LevelUpState.CharBuildMode.CharGen : LevelUpState.CharBuildMode.LevelUp);
                    if (level == 0) { seed.SelectRace(human); seed.SelectGender(fixture.Caster.Descriptor.Gender); seed.SelectAlignment(fixture.Caster.Descriptor.Alignment.Value); }
                    if (!seed.SelectClass(wizard)) throw new InvalidOperationException("Native Wizard fixture seed refused.");
                    FillOracleNativeChoices(seed);
                    typeof(LevelUpController).GetMethod("ApplyLevelup", Cleanup2Fields).Invoke(seed, new object[] { unit.Descriptor });
                    seed.Cancel(); seed = null;
                    if (unit.Descriptor.Progression.GetClassLevel(wizard) != level + 1) throw new InvalidOperationException("Native Wizard fixture seed level differs.");
                }
                var book = unit.Descriptor.GetSpellbook(wizard.Spellbook);
                var source = Cleanup2ElementalAbility(fixture, ElementalSourceChooserPolicy.Rank(2).Guid);
                if (book == null || book.IsKnown(source) || unit.Descriptor.Progression.CharacterLevel != 2)
                    throw new InvalidOperationException("Unknown original source on native Wizard2 required.");
                typeof(UnitProgressionData).GetProperty("Experience").SetValue(unit.Descriptor.Progression, game.BlueprintRoot.Progression.XPTable.GetBonus(3), null);
                if (!LevelUpController.CanLevelUp(unit.Descriptor)) throw new InvalidOperationException("Native Wizard3 XP prerequisite absent.");
                // The hidden session ("hidden") is retained in the method but not
                // replayed: candidate 0d944a262f observed it. Session B (revealed):
                // option on before opening. A fresh module-OFF run has nothing to
                // hide and runs the same session.
                if (!off) OriginalSummonElementalVisibility.Show = true;
                foreach (int frame in Cleanup2DesktopLearningSession(fixture, wizard, unit, book, source, screens, levelUpIntake, "revealed", globalBefore, originalBuildUnit)) yield return frame;
            }
            finally
            {
                if (seed != null && seed.Preview?.Unit != null && !seed.Preview.Unit.Destroyed) seed.Cancel();
                bool foreign = DialogMessageBox.Instance != null && DialogMessageBox.Instance.IsShown ||
                    !ReferenceEquals(ui.LevelUpController, globalBefore) || build.LevelUpController != null ||
                    (!ReferenceEquals(build.Unit, originalBuildUnit) && !ReferenceEquals(build.Unit, unit?.Descriptor));
                if (!foreign)
                {
                    if (ReferenceEquals(build.Unit, unit?.Descriptor)) build.Unit = originalBuildUnit;
                    if (unit != null) { CleanupExpandedSummoningUnit(unit); fixture.Created.Remove(unit); game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick(); }
                    if (blueprint != null) UnityEngine.Object.Destroy(blueprint);
                }
                else throw new InvalidOperationException("Native PC learning ownership changed; foreign creator and referenced actor remain untouched for fixture refusal cleanup.");
            }
            for (int frame = 0; frame < 8; frame++) yield return 0;
        }

        // One native level-up session on the disposable actor: open through
        // the real creator, observe, cancel through the owned confirmation, and
        // restore only this session's controller. Every reference (controller,
        // preview, spellbook, selection, collection, rows) is acquired anew.
        private IEnumerable<int> Cleanup2DesktopLearningSession(ExpandedSummoningCorrectionFixture fixture, BlueprintCharacterClass wizard,
            UnitEntityData unit, Spellbook book, Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility source, NativeIconScreenEvidence screens,
            Cleanup2SourcesLevelUpIntake levelUpIntake, string session, LevelUpController globalBefore, UnitDescriptor originalBuildUnit)
        {
            bool off = Cleanup2OffState(), revealed = session == "revealed";
            var game = Game.Instance; var ui = game.UI; var build = ui.CharacterBuildController;
            LevelUpController backend = null; int commits = 0; bool presented = false;
            string id = revealed ? "" : "-hidden-session";
            try
            {
                JObject intakeReceipt;
                if (!levelUpIntake.Current(out intakeReceipt) || build.IsShow || build.LevelUpController != null || !ReferenceEquals(ui.LevelUpController, globalBefore))
                    throw new InvalidOperationException("Native level-up is not closed/idle before the " + session + " session: " + intakeReceipt);
                if (!LevelUpController.CanLevelUp(unit.Descriptor) || book.IsKnown(source)) throw new InvalidOperationException("Disposable Wizard2 level-up prerequisites changed before the " + session + " session.");
                var known = book.GetAllKnownSpells().ToArray(); var resources = Cleanup2ElementalResources(unit);
                try { build.HandleLevelUpStart(unit.Descriptor, null, () => commits++, LevelUpState.CharBuildMode.LevelUp); }
                finally
                {
                    // Retain the exact native result even if later native UI
                    // setup throws. Never adopt the retained intake controller
                    // or a backend for another actor.
                    var candidate = build.LevelUpController;
                    if (candidate != null && !ReferenceEquals(candidate, globalBefore) && ReferenceEquals(candidate.Unit, unit.Descriptor) &&
                        ReferenceEquals(ui.LevelUpController, candidate) && ReferenceEquals(build.Unit, unit.Descriptor))
                    { backend = candidate; presented = true; }
                }
                if (backend == null || backend.AutoCommit || ReferenceEquals(backend.Preview, unit.Descriptor) ||
                    !ReferenceEquals(backend.Unit, unit.Descriptor) || !ReferenceEquals(ui.LevelUpController, backend))
                    throw new InvalidOperationException("Actual PC creator did not create an independent owned preview (" + session + ").");
                Func<bool> ownsCreator = () => presented && ReferenceEquals(Game.Instance, game) && ReferenceEquals(game.UI, ui) &&
                    ReferenceEquals(ui.CharacterBuildController, build) && build.IsShow && ReferenceEquals(build.Unit, unit.Descriptor) &&
                    ReferenceEquals(build.LevelUpController, backend) && ReferenceEquals(ui.LevelUpController, backend) &&
                    ReferenceEquals(backend.Unit, unit.Descriptor) && backend.Preview?.Unit != null && !backend.Preview.Unit.Destroyed && Cleanup2LearningModuleState(off);
                for (int frame = 0; frame < 15; frame++) yield return 0;
                if (!ownsCreator()) throw new InvalidOperationException("Owned native PC creator changed before class/phase input.");
                build.SetClass(wizard); CompleteTeleportLevelUpPrerequisites(build);
                build.SetPhase((int)CharBPhase.Type.Spells);
                foreach (int frame in WaitTeleportLevelUpUi(() => build.GetComponentsInChildren<CharBSelectionSwitchItem>(true).Any(value =>
                    value.gameObject.activeInHierarchy && ReferenceEquals(value.SpellSelectionData?.Spellbook, wizard.Spellbook) &&
                    (value.SpellSelectionData.ExtraMaxLevel == 2 || value.SpellLevel == 2)), "native PC original-source learning collection")) yield return frame;
                var collection = build.GetComponentsInChildren<CharBSelectionSwitchItem>(true).Where(value => value.gameObject.activeInHierarchy &&
                    ReferenceEquals(value.SpellSelectionData?.Spellbook, wizard.Spellbook) &&
                    (value.SpellSelectionData.ExtraMaxLevel == 2 || value.SpellLevel == 2)).OrderBy(value => value.SpellIndex).First();
                if (!ownsCreator()) throw new InvalidOperationException("Owned native PC creator changed before collection selection.");
                collection.Toggle.isOn = true;
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var selection = build.Spells.CurrentSpellSelectionData;
                if (!ReferenceEquals(selection, collection.SpellSelectionData) || !ReferenceEquals(selection.Spellbook, wizard.Spellbook) ||
                    !ReferenceEquals(selection.SpellList, wizard.Spellbook.SpellList) || selection.ExtraByStat)
                    throw new InvalidOperationException("Actual PC normal native learning collection changed.");
                Func<CharBSelectorLayer> currentLayer = () => build.GetComponentsInChildren<CharBSelectorLayer>(true).SingleOrDefault(value => value.gameObject.activeInHierarchy &&
                    ReferenceEquals(typeof(CharBSelectorLayer).GetField("CurrentSpellSelectionData", Cleanup2Fields).GetValue(value), selection));
                Func<CharBSelectorLayer, CharBuildSelectorItem[]> elementalRows = owner => owner == null ? new CharBuildSelectorItem[0] :
                    owner.GetComponentsInChildren<CharBuildSelectorItem>(true).Where(value => value.gameObject.activeInHierarchy && value.BlueprintAbility != null &&
                        StandaloneElementalPresentationPolicy.IsOriginalSummonElemental(value.BlueprintAbility.AssetGuid)).ToArray();
                var pending = Cleanup2DesktopPending(selection); var layer = currentLayer();
                if (!ownsCreator() || layer == null) throw new InvalidOperationException("Owned native PC creator/learning layer unavailable (" + session + ").");
                var listed = layer.SelectorItems.Where(value => value.gameObject.activeInHierarchy && value.BlueprintAbility != null).ToArray();
                string canonical = ElementalSourceChooserPolicy.Rank(2).MonsterParentGuid;
                if (!revealed)
                {
                    bool hidden = !OriginalSummonElementalVisibility.Show && elementalRows(layer).Length == 0 && listed.Length >= 10 &&
                        listed.Count(value => value.BlueprintAbility.AssetGuid == canonical) == 1 &&
                        Cleanup2DesktopNoOwnedDisclosure(layer) && Cleanup2DesktopPending(selection).SequenceEqual(pending) && !book.IsKnown(source) &&
                        commits == 0 && StandaloneElementalDiscovery.PcSeamState == "installed-exact-learning-call-copy";
                    Cleanup2Check("elemental-desktop-learning-hidden-by-default", hidden, new JObject { ["session"] = session, ["source"] = source.AssetGuid,
                        ["optionRevealed"] = OriginalSummonElementalVisibility.Show, ["activeOriginalElementalRows"] = elementalRows(layer).Length,
                        ["activeNativeRows"] = listed.Length, ["canonicalSummonMonsterIIRows"] = listed.Count(value => value.BlueprintAbility.AssetGuid == canonical),
                        ["noOwnedDisclosure"] = Cleanup2DesktopNoOwnedDisclosure(layer), ["pendingExact"] = Cleanup2DesktopPending(selection).SequenceEqual(pending),
                        ["freePendingSlots"] = pending.Count(value => value == null),
                        ["pcSeam"] = StandaloneElementalDiscovery.PcSeamState, ["normalExtraMaxLevel"] = selection.ExtraMaxLevel, ["extraByStat"] = selection.ExtraByStat,
                        ["dispositions"] = new JArray(StandaloneElementalDiscovery.SnapshotDispositions().Where(value => value.Contains(":2:")).Take(24)) },
                        "With the option off before opening, the real PC Wizard learning list offers no original Summon Elemental row and no disclosure, keeps the ordinary rows (Summon Monster II once) and allocates nothing.");
                    if (!hidden) throw new InvalidOperationException("Native PC learning still offers an original Summon Elemental row with the option off.");
                    foreach (int frame in screens.Capture("elementals-desktop-learning-hidden-default", new JObject { ["source"] = source.AssetGuid,
                        ["nativeText"] = new JArray(layer.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value => value.text)) },
                        () => build.IsShow && ReferenceEquals(build.LevelUpController, backend) && ReferenceEquals(build.Spells.CurrentSpellSelectionData, selection) &&
                            Cleanup2NoNarrativeOverlay())) yield return frame;
                }
                else
                {
                    var sourceRows = layer.GetComponentsInChildren<CharBuildSelectorItem>(true).Where(value => value.gameObject.activeInHierarchy &&
                        ReferenceEquals(value.BlueprintAbility, source)).ToArray();
                    bool ordinary = (off || OriginalSummonElementalVisibility.Show) && sourceRows.Length == 1 && sourceRows[0].Toggle.interactable && !sourceRows[0].Toggle.isOn &&
                        Cleanup2DesktopNoOwnedDisclosure(layer) && Cleanup2DesktopPending(selection).SequenceEqual(pending) && !book.IsKnown(source) &&
                        book.GetAllKnownSpells().SequenceEqual(known) && Cleanup2ElementalResources(unit) == resources && commits == 0 &&
                        listed.Count(value => value.BlueprintAbility.AssetGuid == canonical) == 1 &&
                        elementalRows(layer).Select(value => value.BlueprintAbility).Distinct().Count() == elementalRows(layer).Length;
                    Cleanup2Check("elemental-desktop-learning-revealed-ordinary-row", ordinary, new JObject { ["session"] = session, ["source"] = source.AssetGuid,
                        ["optionRevealed"] = OriginalSummonElementalVisibility.Show, ["sourceRows"] = sourceRows.Length,
                        ["interactable"] = sourceRows.Length == 1 && sourceRows[0].Toggle.interactable, ["selectedBeforeClick"] = sourceRows.Length == 1 && sourceRows[0].Toggle.isOn,
                        ["activeNativeRows"] = listed.Length, ["originalElementalRows"] = new JArray(elementalRows(layer).Select(value => value.BlueprintAbility.AssetGuid)),
                        ["noOwnedDisclosure"] = Cleanup2DesktopNoOwnedDisclosure(layer), ["pendingExact"] = Cleanup2DesktopPending(selection).SequenceEqual(pending),
                        ["freePendingSlots"] = pending.Count(value => value == null), ["freshModuleOff"] = off },
                        "With the option on before opening (or the module fresh OFF), the original spell is exactly one ordinary, selectable native learning row with no disclosure or duplicate.");
                    if (!ordinary) throw new InvalidOperationException("Native PC learning does not show the original spell as one ordinary row.");
                    // Live native state (UnselectSpell -> RemoveAction rebuilds
                    // LevelUpState; captured selections and layers go stale).
                    Func<SpellSelectionData, bool> wizardSelection = value => value != null && ReferenceEquals(value.Spellbook, wizard.Spellbook) &&
                        ReferenceEquals(value.SpellList, wizard.Spellbook.SpellList) && value.ExtraMaxLevel == 2 && !value.ExtraByStat;
                    Func<CharBSelectorLayer> liveLayer = () => build.GetComponentsInChildren<CharBSelectorLayer>(true).Where(value => value.gameObject.activeInHierarchy &&
                        value.GetComponentsInChildren<CharBuildSelectorItem>(true).Any(item => item.gameObject.activeInHierarchy && ReferenceEquals(item.BlueprintAbility, source))).SingleOrDefault();
                    var row = sourceRows[0];
                    if (!ownsCreator() || row.Toggle.isOn || !row.Toggle.interactable) throw new InvalidOperationException("Native source row/controller recycled or already selected before click.");
                    // Toggle off -> on dispatches the row's native callback; no
                    // SelectSpell, pending-array write or allocation shortcut.
                    row.Toggle.isOn = true;
                    foreach (int frame in WaitTeleportLevelUpUi(() => backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(source), "native PC original-source preview allocation")) yield return frame;
                    var allocation = Cleanup2ReplacementSnapshot.Read(backend, build, wizardSelection, wizard, source, null, liveLayer, ownsCreator);
                    int intendedSlot = allocation.Extra == null ? -1 : Array.FindIndex(allocation.Extra, value => ReferenceEquals(value, source));
                    bool allocated = allocation.Matches == 1 && intendedSlot >= 0 && allocation.Extra.Count(value => ReferenceEquals(value, source)) == 1 &&
                        allocation.Extra.Length == pending.Length && allocation.Extra.Count(value => value == null) == pending.Count(value => value == null) - 1 &&
                        allocation.PreviewKnowsSource && !book.IsKnown(source) && book.GetAllKnownSpells().SequenceEqual(known) &&
                        Cleanup2ElementalResources(unit) == resources && commits == 0;
                    Cleanup2Check("elemental-desktop-native-learning-allocation", allocated, new JObject { ["source"] = source.AssetGuid,
                        ["pendingBefore"] = new JArray(pending.Select(value => value?.AssetGuid)), ["intendedSlot"] = intendedSlot,
                        ["live"] = allocation.ToJson(), ["originalKnows"] = book.IsKnown(source), ["commits"] = commits,
                        ["nativeInput"] = "CharBuildSelectorItem.Toggle off->on -> native OnValueChanged -> original Action -> CharacterBuildController.SetSpell" },
                        "One real PC selector click allocates exactly one original source into one native slot of the live selection; the preview knows it and the actor's committed known spells are unchanged.");
                    if (!allocated) throw new InvalidOperationException("Native PC original-source allocation failed.");
                    // Require the intended slot as the native current slot.
                    if (allocation.Slot != intendedSlot)
                    {
                        var slotItem = build.GetComponentsInChildren<CharBSelectionSwitchItem>(true).SingleOrDefault(value => value.gameObject.activeInHierarchy &&
                            value.SpellIndex == intendedSlot && wizardSelection(value.SpellSelectionData));
                        if (slotItem != null && !slotItem.Toggle.isOn && slotItem.Toggle.interactable)
                        {
                            slotItem.Toggle.isOn = true;
                            var slotWatch = Stopwatch.StartNew(); int slotFrames = 0;
                            while (build.Spells.CurrentSpellsCollectionIndex != intendedSlot && slotFrames < 300 && slotWatch.Elapsed.TotalSeconds < 5) { slotFrames++; yield return 0; }
                            for (int frame = 0; frame < 8; frame++) yield return 0;
                        }
                    }
                    var replacementRow = liveLayer()?.GetComponentsInChildren<CharBuildSelectorItem>(true).FirstOrDefault(value => value.gameObject.activeInHierarchy &&
                        value.Toggle.interactable && !value.Toggle.isOn && value.SpellLevel == 2 && value.BlueprintAbility != null &&
                        !StandaloneElementalPresentationPolicy.IsOriginalSummonElemental(value.BlueprintAbility.AssetGuid) &&
                        !(backend.Preview.GetSpellbook(wizard.Spellbook)?.IsKnown(value.BlueprintAbility) ?? true) &&
                        !(allocation.Extra ?? new Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility[0]).Contains(value.BlueprintAbility));
                    var replacementSpell = replacementRow?.BlueprintAbility;
                    var before = Cleanup2ReplacementSnapshot.Read(backend, build, wizardSelection, wizard, source, replacementSpell, liveLayer, ownsCreator);
                    bool ready = replacementRow != null && before.Owned && before.Matches == 1 && before.Slot == intendedSlot && before.Extra != null &&
                        ReferenceEquals(before.Extra[intendedSlot], source) && before.PreviewKnowsSource && !before.PreviewKnowsReplacement &&
                        before.SourceRows == 1 && before.SourceRowOn && !replacementRow.Toggle.isOn && replacementRow.Toggle.interactable;
                    Cleanup2Check("elemental-desktop-learning-replacement-preconditions", ready, new JObject { ["source"] = source.AssetGuid,
                        ["replacement"] = replacementSpell?.AssetGuid, ["intendedSlot"] = intendedSlot, ["before"] = before.ToJson(),
                        ["replacementRowOn"] = replacementRow != null && replacementRow.Toggle.isOn, ["replacementRowInteractable"] = replacementRow != null && replacementRow.Toggle.interactable },
                        "Immediately before input, the intended native slot is current and holds the original spell, the live preview knows it, and an ordinary unselected replacement row is ready.");
                    if (!ready) throw new InvalidOperationException("Native replacement preconditions were not met.");
                    // Native PC replacement: another ordinary row for the same slot
                    // runs CharacterBuildController.SetSpell (UnselectSpell, SelectSpell).
                    replacementRow.Toggle.isOn = true;
                    var replaceWatch = Stopwatch.StartNew(); int replaceFrames = 0;
                    Func<Cleanup2ReplacementSnapshot, bool> replaced = snapshot => snapshot.Owned && snapshot.Matches == 1 && snapshot.Extra != null &&
                        snapshot.Extra.Length == before.Extra.Length && ReferenceEquals(snapshot.Extra[intendedSlot], replacementSpell) &&
                        Enumerable.Range(0, snapshot.Extra.Length).Where(index => index != intendedSlot).All(index => ReferenceEquals(snapshot.Extra[index], before.Extra[index])) &&
                        snapshot.Extra.Count(value => value == null) == before.Extra.Count(value => value == null) &&
                        snapshot.PreviewKnowsReplacement && !snapshot.PreviewKnowsSource &&
                        snapshot.SourceRows == 1 && !snapshot.SourceRowOn && snapshot.SourceRowInteractable;
                    while (!replaced(Cleanup2ReplacementSnapshot.Read(backend, build, wizardSelection, wizard, source, replacementSpell, liveLayer, ownsCreator)) &&
                        replaceFrames < 600 && replaceWatch.Elapsed.TotalSeconds < 8)
                    { replaceFrames++; yield return 0; }
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    var after = Cleanup2ReplacementSnapshot.Read(backend, build, wizardSelection, wizard, source, replacementSpell, liveLayer, ownsCreator);
                    bool deselected = replaced(after) && !book.IsKnown(source) && book.GetAllKnownSpells().SequenceEqual(known) &&
                        Cleanup2ElementalResources(unit) == resources && commits == 0 && OriginalSummonElementalVisibility.Show;
                    Cleanup2Check("elemental-desktop-learning-native-deselect", deselected, new JObject { ["source"] = source.AssetGuid,
                        ["replacement"] = replacementSpell.AssetGuid, ["intendedSlot"] = intendedSlot, ["before"] = before.ToJson(), ["after"] = after.ToJson(),
                        ["capturedSelectionStalePending"] = new JArray(Cleanup2DesktopPending(selection).Select(value => value?.AssetGuid)),
                        ["committedKnownExact"] = book.GetAllKnownSpells().SequenceEqual(known), ["commits"] = commits,
                        ["waitFrames"] = replaceFrames, ["waitSeconds"] = replaceWatch.Elapsed.TotalSeconds,
                        ["nativeInput"] = "Ordinary CharBuildSelectorItem.Toggle off->on for the current slot -> native SetSpell (UnselectSpell, SelectSpell); every reading from the live native LevelUpState/preview/layer" },
                        "Native replacement moves the intended slot from the original spell to the replacement, keeps the other slot and the allowance, the live preview knows only the replacement, the original stays one ordinary unselected row, and nothing is committed.");
                    if (!deselected) throw new InvalidOperationException("Native PC learning replacement of the original spell failed.");
                    foreach (int frame in screens.Capture("elementals-desktop-learning-replaced", new JObject { ["source"] = source.AssetGuid, ["replacement"] = replacementSpell.AssetGuid,
                        ["nativeText"] = new JArray((liveLayer()?.GetComponentsInChildren<TextMeshProUGUI>(false) ?? new TextMeshProUGUI[0]).Select(value => value.text)) },
                        () => build.IsShow && ReferenceEquals(build.LevelUpController, backend) && Cleanup2NoNarrativeOverlay())) yield return frame;
                }
                if (!ownsCreator()) throw new InvalidOperationException("Owned native PC creator changed before cancellation.");
                build.OnHotKeyEscPressed();
                foreach (int frame in WaitTeleportLevelUpUi(() => DialogMessageBox.Instance.IsShown, "native PC learning cancellation confirmation")) yield return frame;
                var callback = (Action<DialogMessageBoxBase.BoxButton>)WorldMapPointSpellActionPatches.ConfirmationCallbackField.GetValue(DialogMessageBox.Instance);
                if (!ownsCreator() || callback == null || !ReferenceEquals(callback.Target, build)) throw new InvalidOperationException("Foreign PC cancellation dialog/controller owner.");
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                foreach (int frame in WaitTeleportLevelUpUi(() => !build.IsShow && !DialogMessageBox.Instance.IsShown, "native PC learning cancelled")) yield return frame;
                if (!presented || !ReferenceEquals(ui.LevelUpController, backend) || build.LevelUpController != null ||
                    !ReferenceEquals(build.Unit, unit.Descriptor) || !ReferenceEquals(backend.Unit, unit.Descriptor))
                    throw new InvalidOperationException("Native PC cancellation no longer owns the exact closed creator/backend; original field is not overwritten.");
                var preview = backend.Preview;
                if (preview?.Unit != null && !preview.Unit.Destroyed) backend.Cancel();
                // Native Cancel/Commit leave UIAccess's plain field populated.
                // Restore it only while it still contains our exact backend.
                if (!ReferenceEquals(ui.LevelUpController, backend)) throw new InvalidOperationException("Native PC backend changed during cancellation.");
                ui.LevelUpController = globalBefore;
                if (build.LevelUpController == null && ReferenceEquals(build.Unit, unit.Descriptor)) build.Unit = originalBuildUnit;
                backend = null; presented = false;
                // Native38: the preview thread may finish its last queued preview
                // in the Cancel frame. Poll the same non-blocking reads only.
                JObject restoredLevelUpReceipt; bool restoredLevelUp;
                var producerWatch = Stopwatch.StartNew(); int producerFrames = 0;
                while (!(restoredLevelUp = levelUpIntake.Current(out restoredLevelUpReceipt)) && producerFrames < 600 &&
                    producerWatch.Elapsed.TotalSeconds < 8)
                { producerFrames++; yield return 0; }
                restoredLevelUpReceipt["producerSettleFrames"] = producerFrames;
                restoredLevelUpReceipt["producerSettleSeconds"] = producerWatch.Elapsed.TotalSeconds;
                bool previewDisposed = preview?.Unit == null || preview.Unit.Destroyed;
                bool cancelled = commits == 0 && unit.Descriptor.Progression.GetClassLevel(wizard) == 2 && !book.IsKnown(source) &&
                    book.GetAllKnownSpells().SequenceEqual(known) && Cleanup2ElementalResources(unit) == resources && build.LevelUpController == null &&
                    ReferenceEquals(ui.LevelUpController, globalBefore) && restoredLevelUp && previewDisposed && !build.IsShow;
                Cleanup2Check("elemental-desktop-native-learning-cancel" + id, cancelled, new JObject { ["session"] = session, ["commits"] = commits,
                    ["classLevel"] = unit.Descriptor.Progression.GetClassLevel(wizard), ["originalKnownReferencesExact"] = book.GetAllKnownSpells().SequenceEqual(known),
                    ["originalResourcesExact"] = Cleanup2ElementalResources(unit) == resources, ["previewDisposed"] = previewDisposed,
                    ["nativeInput"] = "Native Esc close + exact owned confirmation Yes; owned native preview Cancel after presenter closes",
                    ["originalCompletedNativeControllerExact"] = restoredLevelUpReceipt,
                    ["scope"] = "Cancellation undoes this session's pending native allocation; the next session (if any) reacquires every reference." },
                    "Actual native PC cancellation commits nothing, retains Wizard2 and its known spells, disposes the session preview and restores the original controller field.");
                if (!cancelled) throw new InvalidOperationException("Native PC learning cancellation failed (" + session + ").");
            }
            finally
            {
                // Restore only this session's own controller if it is still open.
                // This is fallback cleanup after a failure, never normal cancellation.
                if (backend != null && presented && ReferenceEquals(backend.Unit, unit.Descriptor) && !ReferenceEquals(backend, globalBefore) &&
                    !backend.AutoCommit && !ReferenceEquals(backend.Preview, unit.Descriptor))
                {
                    try { Cleanup2Check("elemental-desktop-learning-fallback-cleanup" + id, false, new JObject { ["session"] = session, ["commits"] = commits,
                        ["scope"] = "Fallback controller cleanup after an earlier failure; recorded as not-a-normal-cancellation." },
                        "Normal cancellation completes without fallback cleanup."); } catch (Exception) { }
                    var dialog = DialogMessageBox.Instance;
                    var callback = dialog == null ? null : (Action<DialogMessageBoxBase.BoxButton>)WorldMapPointSpellActionPatches.ConfirmationCallbackField.GetValue(dialog);
                    if (dialog != null && dialog.IsShown && callback != null && ReferenceEquals(callback.Target, build)) dialog.HandleForceClose();
                    if (build.IsShow && ReferenceEquals(build.LevelUpController, backend)) build.Show(false);
                    if (backend.Preview?.Unit != null && !backend.Preview.Unit.Destroyed) backend.Cancel();
                    if (ReferenceEquals(ui.LevelUpController, backend)) ui.LevelUpController = globalBefore;
                    if (build.LevelUpController == null && ReferenceEquals(build.Unit, unit.Descriptor)) build.Unit = originalBuildUnit;
                }
            }
            for (int frame = 0; frame < 8; frame++) yield return 0;
        }
    }
}
