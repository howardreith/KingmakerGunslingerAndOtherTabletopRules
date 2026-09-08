using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UI.Tooltip;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using TMPro;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _teleportationSpellbookUiSteps;
        private Stopwatch _teleportationSpellbookUiWatch;
        private readonly List<RuntimeTestAssertion> _teleportationSpellbookUiAssertions = new List<RuntimeTestAssertion>();
        private readonly List<object> _teleportationSpellbookUiCaptures = new List<object>();
        private readonly List<object> _teleportationSpellbookUiExceptions = new List<object>();
        private string TeleportationSpellbookUiPath { get { return Path.Combine(_request.EvidenceDirectory, "teleportation-spellbook-ui.json"); } }
        private void PollTeleportationSpellbookUi()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationSpellbookUi || !_request.ExitAfterCompletion ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Spellbook UI qualification requires its guarded working save, mandatory exit and intact write sentinels.");
            if (_teleportationSpellbookUiWatch == null) _teleportationSpellbookUiWatch = Stopwatch.StartNew();
            if (_teleportationSpellbookUiWatch.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Spellbook UI qualification timed out.");
            if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive) return;
            if (_teleportationSpellbookUiSteps == null) _teleportationSpellbookUiSteps = RunTeleportationSpellbookUi().GetEnumerator();
            Exception failure = null;
            try { if (_teleportationSpellbookUiSteps.MoveNext()) return; }
            catch (Exception exception) { failure = exception; }
            try { _teleportationSpellbookUiSteps.Dispose(); }
            catch (Exception exception) { failure = failure == null ? exception : new AggregateException(failure, exception); }
            _teleportationSpellbookUiSteps = null;
            WriteTeleportationSpellbookUi(failure == null ? null : failure.ToString());
            Complete(CreateResult(failure != null ? RuntimeTestStatuses.Error : _teleportationSpellbookUiAssertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, _teleportationSpellbookUiAssertions, failure == null ? null : failure.ToString()));
        }
        private void StopTeleportationSpellbookUi(RuntimeTestResult result)
        {
            if (_teleportationSpellbookUiSteps == null) return;
            var steps = _teleportationSpellbookUiSteps; _teleportationSpellbookUiSteps = null;
            string error = "Runner completed before native spellbook UI qualification finished.";
            try { steps.Dispose(); }
            catch (Exception exception) { error += " " + exception; result.Status = RuntimeTestStatuses.Error; }
            WriteTeleportationSpellbookUi(error); result.Diagnostics.Add(error);
        }
        private void WriteTeleportationSpellbookUi(string error)
        {
            WriteTeleportationForensicJson(TeleportationSpellbookUiPath, new { schemaVersion = 1, runId = _request.RunId,
                claims = "Native local-area service window, class/level toggles, spell rows, description builder, preparation and action-bar auto-fill across Unity frames. Request-local real books only; no level-up completion, casting, save writes or campaign persistence claim.",
                captures = _teleportationSpellbookUiCaptures, exceptions = _teleportationSpellbookUiExceptions,
                assertions = _teleportationSpellbookUiAssertions, saveWriteObserved = _workingSaveSmoke.WriteObserved, error });
        }
        private void TeleportSpellbookUiAssert(string id, string expected, string actual, bool pass)
        { _teleportationSpellbookUiAssertions.Add(Assertion("teleportation-spellbook-ui-" + id, expected, actual, pass, TeleportationSpellbookUiPath)); }
        private void CaptureTeleportSpellbookUi(string step, object state)
        { _teleportationSpellbookUiCaptures.Add(new { step, frame = Time.frameCount, state }); }
        private void ObserveTeleportSpellbookUiException(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error && message.Contains("Exception"))
                _teleportationSpellbookUiExceptions.Add(new { message, stack, type = type.ToString(), frame = Time.frameCount });
        }
        private IEnumerable<int> RunTeleportationSpellbookUi()
        {
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            if (!_context.FeatureModules.Active.TeleportationSpells || BlueprintBootstrap.TeleportationPublication == null || game.IsControllerGamepad ||
                game.CurrentMode != GameModeType.Default || ui.SpellBookController == null || ui.ServiceWindow == null || ui.ServiceWindow.WindowTabs.IsShow ||
                ui.DescriptionController == null || ui.DescriptionController.DescWindow.gameObject.activeInHierarchy || GroupController.Instance == null ||
                ui.SelectionManagerPC == null || ActionBarManager.Instance == null || !SettingsRoot.Instance.AutofillActionbarSlots.CurrentValue)
                throw new InvalidOperationException("Native stationary desktop local-area UI with enabled native auto-fill is required.");
            var classes = new[] { "ba34257984f4c41408ce1dc2004e342e", "b3a505fb61437dc4097f43c3f8f9a4cf", "67819271767a9dd4fbfd4ae700befea0", "610d836f3a3a9ed42a4349b62f002e96" }
                .Select(id => BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, id, "native spellbook UI fixture class")).ToArray();
            var owner = player.Party.FirstOrDefault(value => TeleportationSpellbookAdapter.CasterAvailable(value) && value.View != null && value.IsDirectlyControllable &&
                classes.All(type => type.Spellbook != null && value.Descriptor.GetSpellbook(type.Spellbook) == null));
            if (owner == null) throw new InvalidOperationException("No available native traveling owner with unused fixture spellbooks.");
            var originalSelection = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var originalParty = player.Party.ToArray(); var originalArea = game.CurrentlyLoadedArea;
            var originalTime = player.GameTime; bool originalPaused = game.IsPaused;
            var originalPositions = originalParty.Select(value => value.Position).ToArray();
            var uiSnapshots = originalParty.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var fixture = new TeleportResourceFixtureOwner(owner); bool fixtureRestored = false;
            var nativeControl = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, TeleportationSpellBlueprints.DimensionDoorId, "native action-bar positive control");
            var spells = BlueprintBootstrap.Teleportation;
            var projectSpells = new[] { spells.Teleport, spells.GreaterTeleport, spells.WordOfRecall };
            Application.logMessageReceived += ObserveTeleportSpellbookUiException;
            try
            {
                game.IsPaused = true;
                ui.SelectionManagerPC.SelectUnit(owner.View, true, true, false);
                var books = classes.Select(type => fixture.AddBook(type.Spellbook)).ToArray();
                books[1].AddKnown(4, nativeControl, true);
                foreach (var book in books.Take(2))
                { book.AddKnown(5, spells.Teleport, true); book.AddKnown(7, spells.GreaterTeleport, true); }
                foreach (var book in books) book.Rest();
                // Fixture AddKnown(isCopy: true) intentionally emits no learn event.
                // A real native selection change composes the actual action bar.
                ui.SelectionManagerPC.SelectUnit(owner.View, true, true, false);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                TeleportSpellbookUiAssert("autofill-positive-control", "native spontaneous Dimension Door reaches the actual action bar",
                    "nativeControl=" + TeleportUiBarSpells(owner).Contains(nativeControl), TeleportUiBarSpells(owner).Contains(nativeControl));
                TeleportSpellbookUiAssert("spontaneous-not-autofilled", "known project spells stay out of native auto-fill",
                    "projectSlots=" + TeleportUiBarSpells(owner).Count(projectSpells.Contains), !TeleportUiBarSpells(owner).Any(projectSpells.Contains));
                CaptureTeleportSpellbookUi("native-bar-after-known-spells", DescribeTeleportUiBar(owner));
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                var controller = ui.SpellBookController;
                if (!ui.ServiceWindow.WindowTabs.IsShow || !controller.IsShow || !controller.gameObject.activeInHierarchy)
                    throw new InvalidOperationException("Native service-window spellbook did not open.");
                GroupController.Instance.SelectUnit(owner);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var tabs = controller.SpellBookView.ClassToggle;
                CaptureTeleportSpellbookUi("native-book-tabs", new { ownerId = owner.UniqueId, scene = controller.gameObject.scene.name,
                    tabs = tabs.GetComponentsInChildren<SpellbookClassTab>(true).Select(value => new { value.Index, active = value.gameObject.activeInHierarchy,
                        text = value.m_TitleLabel.text, level = value.m_ClassLevel.text }).ToArray() });
                TeleportSpellbookUiAssert("native-books", "real Wizard, Sorcerer, Cleric and Druid spellbooks appear as distinct class tabs",
                    "bookCount=" + tabs.Spellbooks.Count, books.All(tabs.Spellbooks.Contains) && tabs.Spellbooks.Distinct().Count() == tabs.Spellbooks.Count);
                var entries = new[] {
                    new TeleportUiSpellEntry(books[0], spells.Teleport, 5), new TeleportUiSpellEntry(books[0], spells.GreaterTeleport, 7),
                    new TeleportUiSpellEntry(books[1], spells.Teleport, 5), new TeleportUiSpellEntry(books[1], spells.GreaterTeleport, 7),
                    new TeleportUiSpellEntry(books[2], spells.WordOfRecall, 6), new TeleportUiSpellEntry(books[3], spells.WordOfRecall, 8)
                };
                foreach (var entry in entries) foreach (int frame in QualifyTeleportUiEntry(controller, entry)) yield return frame;
                for (int frame = 0; frame < 8; frame++) yield return 0;
                CaptureTeleportSpellbookUi("native-bar-after-preparation", DescribeTeleportUiBar(owner));
                TeleportSpellbookUiAssert("prepared-not-autofilled", "native rest and preparation callbacks retain the ordinary spell while omitting every strategic spell",
                    "projectSlots=" + TeleportUiBarSpells(owner).Count(projectSpells.Contains), !TeleportUiBarSpells(owner).Any(projectSpells.Contains) && TeleportUiBarSpells(owner).Contains(nativeControl));
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                var serviceCanvas = ui.ServiceWindow.WindowTabs.GetComponent<CanvasGroup>();
                CaptureTeleportSpellbookUi("native-service-close", new { mode = game.CurrentMode.ToString(),
                    tabsShown = ui.ServiceWindow.WindowTabs.IsShow, controller.IsShow, active = controller.gameObject.activeInHierarchy,
                    serviceCanvas.alpha, serviceCanvas.blocksRaycasts, barActive = ActionBarManager.Instance.Slots.gameObject.activeInHierarchy });
                TeleportSpellbookUiAssert("normal-close", "native service-window close releases its input surface and restores the paused local HUD/action bar",
                    "mode=" + game.CurrentMode + ";tabsShown=" + ui.ServiceWindow.WindowTabs.IsShow,
                    !ui.ServiceWindow.WindowTabs.IsShow && !serviceCanvas.blocksRaycasts &&
                    ActionBarManager.Instance.Slots.gameObject.activeInHierarchy && game.CurrentMode == GameModeType.Pause);
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                TeleportSpellbookUiAssert("normal-reopen", "normal spellbook reopening remains functional with the same real books",
                    "shown=" + controller.IsShow, controller.IsShow && controller.CurrentCharacter == owner && books.All(controller.SpellBookView.ClassToggle.Spellbooks.Contains));
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                fixture.Restore(); fixtureRestored = true;
                ui.SelectionManagerPC.MultiSelect(originalSelection.Select(value => value.View).ToArray(), false);
                foreach (var snapshot in uiSnapshots) snapshot.Restore();
                // Observe the native queued action-bar refresh after book removal.
                for (int frame = 0; frame < 30; frame++) yield return 0;
                TeleportSpellbookUiAssert("cleanup-events", "native deferred UI refresh preserves the restored original action-bar state",
                    "restored=" + uiSnapshots.All(value => value.IsRestored()), uiSnapshots.All(value => value.IsRestored()));
            }
            finally
            {
                ui.DescriptionController.HandleCloseDescriptionWindow(ui.DescriptionController.DescWindow);
                if (ui.ServiceWindow.WindowTabs.IsShow) ui.ServiceWindow.HandleOpenSpellbook();
                if (!fixtureRestored) fixture.Restore();
                ui.SelectionManagerPC.MultiSelect(originalSelection.Select(value => value.View).ToArray(), false);
                foreach (var snapshot in uiSnapshots) snapshot.Restore();
                game.IsPaused = originalPaused;
                Application.logMessageReceived -= ObserveTeleportSpellbookUiException;
                bool restored = fixture.IsRestored() && uiSnapshots.All(value => value.IsRestored()) &&
                    ui.SelectionManagerPC.SelectedUnits.SequenceEqual(originalSelection) && player.Party.SequenceEqual(originalParty) &&
                    originalPositions.SequenceEqual(originalParty.Select(value => value.Position)) && ReferenceEquals(game.CurrentlyLoadedArea, originalArea) &&
                    player.GameTime == originalTime && game.IsPaused == originalPaused && !ui.ServiceWindow.WindowTabs.IsShow &&
                    !TeleportContextConfirmationPresenter.Pending && !_workingSaveSmoke.WriteObserved;
                CaptureTeleportSpellbookUi("cleanup", new { restored, resourcesRestored = fixture.IsRestored(),
                    uiSettingsRestored = uiSnapshots.All(value => value.IsRestored()), timeUnchanged = player.GameTime == originalTime,
                    selectionRestored = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(originalSelection), exceptions = _teleportationSpellbookUiExceptions.Count });
                TeleportSpellbookUiAssert("cleanup", "exact original books, resources, action-bar references, selection, stats, party, area, time and pause state; zero writes", "restored=" + restored, restored);
                TeleportSpellbookUiAssert("exceptions", "zero native or mod UI exceptions through fixture cleanup", "count=" + _teleportationSpellbookUiExceptions.Count, _teleportationSpellbookUiExceptions.Count == 0);
            }
        }
        private IEnumerable<int> QualifyTeleportUiEntry(SpellBookController controller, TeleportUiSpellEntry entry)
        {
            string caseId = entry.Book.Blueprint.AssetGuid + "-" + entry.Level;
            var tabs = controller.SpellBookView.ClassToggle;
            int bookIndex = tabs.Spellbooks.IndexOf(entry.Book);
            var tab = tabs.GetComponentsInChildren<SpellbookClassTab>(true).Single(value => value.Index == bookIndex && value.gameObject.activeInHierarchy);
            tab.Toggle.isOn = true;
            if (!ReferenceEquals(controller.CurrentSpellbook, entry.Book)) throw new InvalidOperationException("Native class toggle did not select its physical book.");
            var levelTab = controller.SpellBookView.LevelBookSwitcher.m_Tabs[entry.Level];
            if (!levelTab.gameObject.activeInHierarchy || !levelTab.Toggle.interactable) throw new InvalidOperationException("Native required spell-level tab is unavailable.");
            levelTab.Toggle.isOn = true;
            if (controller.CurrentBookLevel != entry.Level) throw new InvalidOperationException("Native level tab selected a different spell level.");
            while (controller.CurrentPageIndex > 0) controller.SpellBookView.GoPrevPage();
            SpellItem row = null;
            for (int page = 0; page < 30; page++)
            {
                row = controller.SpellBookView.GetComponentsInChildren<SpellItem>(true).SingleOrDefault(value =>
                    value.gameObject.activeInHierarchy && value.SpellData != null && value.SpellData.Blueprint == entry.Spell);
                if (row != null) break;
                controller.SpellBookView.GoNextPage();
                if (controller.CurrentBookLevel != entry.Level) break;
            }
            if (row == null) throw new InvalidOperationException("Native spellbook pagination has no project spell row: " + caseId);
            controller.SpellBookView.ResetSelection(); row.Toggle.isOn = true;
            for (int frame = 0; frame < 8; frame++) yield return 0;
            TeleportSpellbookUiAssert("row-" + caseId, "native row selects the exact spell, book, level and icon",
                "selected=" + (controller.SelectedSpell == null ? "none" : controller.SelectedSpell.Blueprint.AssetGuid),
                controller.SelectedSpell != null && controller.SelectedSpell.Blueprint == entry.Spell &&
                ReferenceEquals(controller.SelectedSpell.Spellbook, entry.Book) && row.SpellImage.sprite == entry.Spell.Icon &&
                row.GetComponentsInChildren<TextMeshProUGUI>(true).Any(value => value.isActiveAndEnabled && string.Equals(value.GetParsedText(), entry.Spell.Name, StringComparison.OrdinalIgnoreCase)));
            var tooltip = row.GetComponent<TooltipTrigger>();
            if (tooltip == null) throw new InvalidOperationException("Native spell row has no description trigger.");
            tooltip.OpenDescriptionWindow();
            for (int frame = 0; frame < 8; frame++) yield return 0;
            var description = Game.Instance.UI.DescriptionController.DescWindow;
            var labels = description.GetComponentsInChildren<TextMeshProUGUI>(true).Where(value => value.isActiveAndEnabled).ToArray();
            CaptureTeleportSpellbookUi("native-spell-description-" + caseId, new { spell = entry.Spell.AssetGuid, book = entry.Book.Blueprint.AssetGuid,
                entry.Level, controller.CurrentPageIndex, descriptionShown = description.gameObject.activeInHierarchy,
                labels = labels.Select(value => new { value.name, value.text, parsed = value.GetParsedText(), value.isTextTruncated, value.isTextOverflowing }).ToArray() });
            TeleportSpellbookUiAssert("description-" + caseId, "native description builder renders the complete world-map instructions without a material-component exception",
                "labels=" + labels.Length, description.gameObject.activeInHierarchy && labels.Any(value => !value.isTextTruncated &&
                    string.Equals(value.GetParsedText(), entry.Spell.Description, StringComparison.OrdinalIgnoreCase)));
            tooltip.CloseDescriptionWindow();
            if (!entry.Book.Blueprint.Spontaneous)
            {
                int before = entry.Book.GetMemorizedSpells(entry.Level).Count(value => value.Spell != null && value.Spell.Blueprint == entry.Spell);
                row.Memorize();
                for (int frame = 0; frame < 4; frame++) yield return 0;
                var prepared = entry.Book.GetMemorizedSpells(entry.Level).Where(value => value.Spell != null && value.Spell.Blueprint == entry.Spell).ToArray();
                var shownSlots = controller.GetComponentsInChildren<SpellSlotItem>(true).Where(value => value.gameObject.activeInHierarchy &&
                    value.MechanicSlot != null && prepared.Contains(value.MechanicSlot)).ToArray();
                TeleportSpellbookUiAssert("prepare-" + caseId, "native row preparation allocates one real preparation and displays its unready slot",
                    "before=" + before + ";after=" + prepared.Length + ";shown=" + shownSlots.Length,
                    prepared.Length == before + 1 && shownSlots.Length > 0 && !prepared.Last().Available);
                entry.Book.Rest();
            }
            TeleportSpellbookUiAssert("local-unusable-" + caseId, "rested strategic spell is unavailable for local-area casting and absent from native metamagic selection",
                "available=" + row.SpellData.IsAvailable, !row.SpellData.IsAvailable && !controller.SpellBookView.GetMetamagicSpells().Contains(entry.Spell));
        }
        private static BlueprintAbility[] TeleportUiBarSpells(UnitEntityData owner)
        {
            return (owner.UISettings.Slots ?? new MechanicActionBarSlot[0]).Select(value =>
                value is MechanicActionBarSlotMemorizedSpell ? ((MechanicActionBarSlotMemorizedSpell)value).Spell :
                value is MechanicActionBarSlotSpontaneusSpell ? ((MechanicActionBarSlotSpontaneusSpell)value).Spell :
                value is MechanicActionBarSlotAbility ? ((MechanicActionBarSlotAbility)value).Ability : null)
                .Where(value => value != null).Select(value => value.Blueprint).ToArray();
        }
        private static object DescribeTeleportUiBar(UnitEntityData owner)
        { return new { ownerId = owner.UniqueId, slots = (owner.UISettings.Slots ?? new MechanicActionBarSlot[0]).Select((value, index) => new {
            index, type = value == null ? null : value.GetType().Name }).ToArray(), spellIds = TeleportUiBarSpells(owner).Select(value => value.AssetGuid).ToArray() }; }
        private sealed class TeleportUiSpellEntry
        {
            internal readonly Spellbook Book; internal readonly BlueprintAbility Spell; internal readonly int Level;
            internal TeleportUiSpellEntry(Spellbook book, BlueprintAbility spell, int level) { Book = book; Spell = spell; Level = level; }
        }
        // Only this guarded disposable fixture restores native action-bar owners.
        private sealed class TeleportationUiSettingsFixture
        {
            private readonly UnitUISettings _settings;
            private readonly MechanicActionBarSlot[] _slots, _contents;
            private readonly FieldInfo[] _fields;
            private readonly object[] _values;
            private readonly System.Collections.IList[] _lists;
            private readonly object[][] _listContents;
            internal TeleportationUiSettingsFixture(UnitUISettings settings)
            {
                _settings = settings; _slots = settings.Slots; _contents = _slots == null ? null : _slots.ToArray();
                _fields = new[] { "m_Phase", "m_ShowAdditionalActionBarOnce", "<Dirty>k__BackingField", "m_AlreadyAutomaniclyAdded", "m_ActivatableAbilityAlreadyAutomaniclyAdded" }
                    .Select(name => typeof(UnitUISettings).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)).ToArray();
                if (_fields.Any(value => value == null)) throw new InvalidOperationException("Native action-bar restoration fields differ.");
                _values = _fields.Select(value => value.GetValue(settings)).ToArray();
                _lists = _values.OfType<System.Collections.IList>().ToArray();
                _listContents = _lists.Select(value => value.Cast<object>().ToArray()).ToArray();
            }
            internal void Restore()
            {
                if (_slots != null) Array.Copy(_contents, _slots, _contents.Length);
                _settings.Slots = _slots;
                for (int index = 0; index < _fields.Length; index++) _fields[index].SetValue(_settings, _values[index]);
                for (int index = 0; index < _lists.Length; index++) { _lists[index].Clear(); foreach (var value in _listContents[index]) _lists[index].Add(value); }
            }
            internal bool IsRestored()
            { return ReferenceEquals(_settings.Slots, _slots) && (_slots == null || _slots.SequenceEqual(_contents)) &&
                _fields.Select((value, index) => Equals(value.GetValue(_settings), _values[index])).All(value => value) &&
                _lists.Select((value, index) => value.Cast<object>().SequenceEqual(_listContents[index])).All(value => value); }
        }
    }
}
