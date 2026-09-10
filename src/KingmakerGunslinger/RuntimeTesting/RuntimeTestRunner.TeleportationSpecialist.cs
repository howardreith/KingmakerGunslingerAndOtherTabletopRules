using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.UI.Group;
using Kingmaker.UI;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _teleportationSpecialistSteps;
        private readonly List<RuntimeTestAssertion> _teleportationSpecialistAssertions = new List<RuntimeTestAssertion>();
        private readonly List<object> _teleportationSpecialistCaptures = new List<object>();
        private bool IsTeleportationSpecialistFixture { get { return _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationSpecialist; } }

        // Gate 2 behavioral proof: a real Conjuration-specialist spellbook
        // prepares Teleport in its fifth-level favorite slot through the native
        // spellbook UI, ordinary rest readies it, and the world map spends
        // exactly that specialist preparation. Negative controls prove the
        // favorite slot rejects non-school and other-book spells, and an
        // unspecialized book has no favorite slot at all.
        private IEnumerable<int> RunTeleportationSpecialist()
        {
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            if (!_context.FeatureModules.Active.TeleportationSpells || BlueprintBootstrap.TeleportationPublication == null || game.IsControllerGamepad ||
                game.CurrentMode != GameModeType.Default || ui.SpellBookController == null || ui.ServiceWindow == null || ui.ServiceWindow.WindowTabs.IsShow ||
                GroupController.Instance == null || ui.SelectionManagerPC == null)
                throw new InvalidOperationException("Native local-area specialist qualification prerequisites differ.");
            var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "ba34257984f4c41408ce1dc2004e342e", "native specialist Wizard class");
            var conjurationFeature = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.BlueprintFeature>(BlueprintBootstrap.Library,
                "cee0f7edbd874a042952ee150f878b84", "native Conjuration specialization feature");
            var evocationFeature = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.BlueprintFeature>(BlueprintBootstrap.Library,
                "c46512b796216b64899f26301241e4e6", "native Evocation specialization feature");
            var coneOfCold = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                "e7c530f8137630f4d9d7ee1aa7b1edc0", "native Evocation level-5 control spell");
            var teleport = BlueprintBootstrap.Teleportation.Teleport;
            var owners = player.Party.Where(value => TeleportationSpellbookAdapter.CasterAvailable(value) && value.View != null && value.IsDirectlyControllable &&
                value.Descriptor.GetSpellbook(wizard.Spellbook) == null).Take(2).ToArray();
            if (owners.Length != 2) throw new InvalidOperationException("Two available fixture owners are required.");
            var conjurer = owners[0]; var evoker = owners[1];
            var originalSelection = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var originalParty = player.Party.ToArray(); var originalArea = game.CurrentlyLoadedArea;
            bool originalPaused = game.IsPaused;
            var uiSnapshots = originalParty.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var fixtureConjurer = new TeleportResourceFixtureOwner(conjurer); var fixtureEvoker = new TeleportResourceFixtureOwner(evoker);
            bool conjurerFeatureAttached = false, evokerFeatureAttached = false;
            try
            {
                game.IsPaused = true;
                var bookConjurer = fixtureConjurer.AddBook(wizard.Spellbook);
                var bookEvoker = fixtureEvoker.AddBook(wizard.Spellbook);
                bookConjurer.AddKnown(5, teleport, true); bookConjurer.AddKnown(5, coneOfCold, true);
                bookEvoker.AddKnown(5, teleport, true);
                foreach (var book in new[] { bookConjurer, bookEvoker }) book.Rest();
                // Negative control: an unspecialized (universalist-shaped) book
                // has no favorite slot at any level.
                SpecialistAssert("universalist-no-favorite-slot", "a book without a school special list has no favorite slots",
                    "favoriteSlots=" + RawSlots(bookConjurer, 5).Count(value => value.Type == SpellSlotType.Favorite),
                    RawSlots(bookConjurer, 5).Count(value => value.Type == SpellSlotType.Favorite) == 0);
                // The school special list is attached through the exact native
                // seam the specialization feature's OnFactActivate calls. The
                // feature facts themselves are also attached so the fixture
                // exercises their native activation path.
                var conjurationList = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellList>(BlueprintBootstrap.Library,
                    "69a6eba12bc77ea4191f573d63c9df12", "Conjuration special list");
                var evocationList = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellList>(BlueprintBootstrap.Library,
                    "79e731172a2dc1f4d92ba229c6216502", "Evocation special list");
                conjurer.Descriptor.AddFact(conjurationFeature); conjurerFeatureAttached = true;
                evoker.Descriptor.AddFact(evocationFeature); evokerFeatureAttached = true;
                bookConjurer.AddSpecialList(conjurationList);
                bookEvoker.AddSpecialList(evocationList);
                // The native slot rebuild the level-up path performs after the
                // school selection changes a book's slot shape.
                bookConjurer.UpdateAllSlotsSize(false);
                bookEvoker.UpdateAllSlotsSize(false);
                bool conjurerSpecial = bookConjurer.GetSpecialSpells(5).Any(value => value.Blueprint == teleport);
                bool evokerSpecial = bookEvoker.GetSpecialSpells(5).Any(value => value.Blueprint == teleport);
                var specialListsField = typeof(Spellbook).GetField("m_SpecialLists", BindingFlags.Instance | BindingFlags.NonPublic);
                CaptureTeleportationSpecialist("after-feature-attach", new {
                    conjurerHasFact = conjurer.Descriptor.HasFact(conjurationFeature), evokerHasFact = evoker.Descriptor.HasFact(evocationFeature),
                    conjurerBooks = conjurer.Descriptor.Spellbooks.Select(value => value.Blueprint.name).ToArray(),
                    conjurerSpecialLists = ((System.Collections.IList)specialListsField.GetValue(bookConjurer)).Count,
                    conjurerSpecialNames = bookConjurer.GetSpecialSpells(5).Select(value => value.Blueprint.name).ToArray(),
                    conjurationListSpells5 = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellList>(BlueprintBootstrap.Library,
                        "69a6eba12bc77ea4191f573d63c9df12", "Conjuration special list").GetSpells(5).Select(value => value.name).ToArray(),
                    conjurerMaxSpellLevel = bookConjurer.MaxSpellLevel, conjurerCasterLevel = bookConjurer.CasterLevel,
                    conjurerFavoriteByLevel = Enumerable.Range(0, 10).Select(level => RawSlots(bookConjurer, level).Count(value => value.Type == SpellSlotType.Favorite)).ToArray(),
                    evokerFavoriteByLevel = Enumerable.Range(0, 10).Select(level => RawSlots(bookEvoker, level).Count(value => value.Type == SpellSlotType.Favorite)).ToArray() });
                SpecialistAssert("special-list-membership", "the Conjuration special list contains Teleport and the Evocation list does not",
                    "conjurer=" + conjurerSpecial + ";evoker=" + evokerSpecial, conjurerSpecial && !evokerSpecial);
                var conjurerFavorite = RawSlots(bookConjurer, 5).SingleOrDefault(value => value.Type == SpellSlotType.Favorite);
                var evokerFavorite = RawSlots(bookEvoker, 5).SingleOrDefault(value => value.Type == SpellSlotType.Favorite);
                if (conjurerFavorite == null || evokerFavorite == null) throw new InvalidOperationException("Native school specializations did not create their favorite slots.");
                bool coneRejected = !bookConjurer.PosibleMemorize(new AbilityData(coneOfCold, bookConjurer), conjurerFavorite);
                bool teleportRejectedByEvoker = !bookEvoker.PosibleMemorize(new AbilityData(teleport, bookEvoker), evokerFavorite);
                SpecialistAssert("favorite-slot-rejects-non-school", "the Conjuration favorite slot refuses an Evocation spell and the Evocation favorite slot refuses Teleport",
                    "coneRejected=" + coneRejected + ";teleportRejectedByEvoker=" + teleportRejectedByEvoker, coneRejected && teleportRejectedByEvoker);
                // Native spellbook UI preparation of Teleport into the favorite slot.
                // The same double selection change the qualified spellbook-ui
                // fixture performs: the second one composes the tab strip after
                // the fixture books exist.
                ui.SelectionManagerPC.SelectUnit(conjurer.View, true, true, false);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                ui.SelectionManagerPC.SelectUnit(conjurer.View, true, true, false);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                var controller = ui.SpellBookController;
                if (!controller.IsShow) throw new InvalidOperationException("Native spellbook did not open for the specialist.");
                GroupController.Instance.SelectUnit(conjurer);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var tabs = controller.SpellBookView.ClassToggle;
                int bookIndex = tabs.Spellbooks.IndexOf(bookConjurer);
                CaptureTeleportationSpecialist("book-tabs", new { bookIndex, tabsSpellbooks = tabs.Spellbooks.Select(value => value.Blueprint.name).ToArray(),
                    currentCharacter = controller.CurrentCharacter == null ? null : controller.CurrentCharacter.UniqueId, conjurerId = conjurer.UniqueId,
                    tabRows = tabs.GetComponentsInChildren<SpellbookClassTab>(true).Select(value => new { value.Index, active = value.gameObject.activeInHierarchy,
                        value.m_ClassLevel.text }).ToArray() });
                // A single-book character hides the native class-tab strip; the
                // controller is already bound to the only book. With several
                // books the strip appears and the exact tab is toggled.
                var tab = tabs.GetComponentsInChildren<SpellbookClassTab>(true).SingleOrDefault(value =>
                    value.Index == bookIndex && value.gameObject.activeInHierarchy);
                if (tab != null) tab.Toggle.isOn = true;
                for (int frame = 0; frame < 30 && !ReferenceEquals(controller.CurrentSpellbook, bookConjurer); frame++) yield return 0;
                if (!ReferenceEquals(controller.CurrentSpellbook, bookConjurer)) throw new InvalidOperationException("Native spellbook did not select the specialist book.");
                controller.SpellBookView.LevelBookSwitcher.m_Tabs[5].Toggle.isOn = true;
                if (controller.CurrentBookLevel != 5) throw new InvalidOperationException("Native level tab did not select spell level five.");
                Kingmaker.UI.ServiceWindow.SpellItem row = null;
                for (int page = 0; page < 30; page++)
                {
                    row = controller.SpellBookView.GetComponentsInChildren<Kingmaker.UI.ServiceWindow.SpellItem>(true).SingleOrDefault(value =>
                        value.gameObject.activeInHierarchy && value.SpellData != null && value.SpellData.Blueprint == teleport);
                    if (row != null) break;
                    controller.SpellBookView.GoNextPage();
                    if (controller.CurrentBookLevel != 5) break;
                }
                if (row == null) throw new InvalidOperationException("Native spellbook pagination has no Teleport row for the specialist.");
                // The exact native drag-drop boundary: the controller memorizes
                // into the displayed favorite slot.
                var displayedFavorite = controller.GetComponentsInChildren<Kingmaker.UI.ServiceWindow.SpellSlotItem>(true)
                    .SingleOrDefault(value => value.gameObject.activeInHierarchy && value.MechanicSlot == conjurerFavorite);
                if (displayedFavorite == null) throw new InvalidOperationException("Native memorize panel does not display the favorite slot.");
                controller.MemorizeWithSound(row.SpellData, conjurerFavorite);
                for (int frame = 0; frame < 4; frame++) yield return 0;
                var favoritePreparations = RawSlots(bookConjurer, 5).Where(value => value.Spell != null && value.Spell.Blueprint == teleport).ToArray();
                SpecialistAssert("favorite-preparation", "native controller preparation lands exactly one unready Teleport in the favorite slot",
                    "count=" + favoritePreparations.Length + ";type=" + (favoritePreparations.Length == 1 ? favoritePreparations[0].Type.ToString() : "none"),
                    favoritePreparations.Length == 1 && favoritePreparations[0].Type == SpellSlotType.Favorite && !favoritePreparations[0].Available);
                // Mixed counting: one ordinary preparation alongside the favorite one.
                row.Memorize();
                for (int frame = 0; frame < 4; frame++) yield return 0;
                var mixedPreparations = RawSlots(bookConjurer, 5).Where(value => value.Spell != null && value.Spell.Blueprint == teleport).ToArray();
                SpecialistAssert("mixed-preparation-count", "favorite plus one ordinary preparation count exactly two unready preparations",
                    "count=" + mixedPreparations.Length + ";types=" + string.Join(",", mixedPreparations.Select(value => value.Type.ToString()).ToArray()),
                    mixedPreparations.Length == 2 && mixedPreparations.Count(value => value.Type == SpellSlotType.Favorite) == 1 &&
                    mixedPreparations.All(value => !value.Available));
                // Ordinary native rest readies both preparations.
                bookConjurer.Rest();
                var rested = RawSlots(bookConjurer, 5).Where(value => value.Spell != null && value.Spell.Blueprint == teleport).ToArray();
                SpecialistAssert("rest-readies-specialist-preparation", "native rest readies the specialist and ordinary preparations",
                    "ready=" + rested.Count(value => value.Available), rested.Length == 2 && rested.All(value => value.Available));
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                ui.SelectionManagerPC.MultiSelect(originalSelection.Select(value => value.View).ToArray(), false);
                foreach (var snapshot in uiSnapshots) snapshot.Restore();
                evoker.Descriptor.RemoveFact(evocationFeature); evokerFeatureAttached = false;
                fixtureEvoker.Restore();
                // Phase B: the global map spends a favorite-only preparation.
                if (GlobalMapRules.Instance != null) throw new InvalidOperationException("A global map is already loaded before the world-map phase.");
                game.LoadArea(game.BlueprintRoot.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None);
                for (int frame = 0; frame < 600; frame++)
                {
                    yield return 0;
                    if (!LoadingProcess.Instance.IsLoadingInProcess && !LoadingProcess.Instance.IsLoadingScreenActive &&
                        GlobalMapRules.Instance != null && game.CurrentMode == GameModeType.GlobalMap) break;
                }
                if (GlobalMapRules.Instance == null || game.CurrentMode != GameModeType.GlobalMap)
                    throw new InvalidOperationException("The world-map phase did not finish loading.");
                var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State;
                var ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
                var payloadState = typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
                var originalPayload = payloadState.GetValue(ledger);
                var originalPosition = map.PartyPosition; var originalLast = map.LastLocation;
                var originalTime = player.GameTime; float originalMiles = map.MilesTravelled;
                var originalHistory = map.HistoryTravels.ToArray(); var originalPerception = map.PerceptionRolledLocations.ToArray();
                var pointRecords = map.Locations.ToArray(); var edgeRecords = map.Edges.ToArray();
                var snapshots = pointRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value))
                    .Concat(edgeRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value)))
                    .Concat(new[] { new TeleportNativeFieldSnapshot(ledger) }).ToArray();
                try
                {
                    var chain = FindTeleportInteractionChain(rules);
                    var origin = chain[0]; var target = chain[2];
                    var setRevealed = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
                    foreach (var point in chain) { setRevealed.Invoke(point.Data, new object[] { true }); point.Data.EdgesOpened = true; }
                    var familiarity = new TeleportFamiliarityState(); familiarity.MigrateLegacy(chain.Select(value => value.Blueprint.AssetGuid));
                    payloadState.SetValue(ledger, familiarity.Serialize());
                    rules.StopWhenRevealingNewEdges = false;
                    rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                    for (int frame = 0; frame < 30; frame++) yield return 0;
                    // Favorite-only preparation for the strategic cast.
                    bookConjurer.Rest();
                    var favoriteForCast = RawSlots(bookConjurer, 5).SingleOrDefault(value => value.Type == SpellSlotType.Favorite);
                    if (favoriteForCast == null) throw new InvalidOperationException("The rested specialist book lost its favorite slot.");
                    if (!bookConjurer.Memorize(new AbilityData(teleport, bookConjurer), favoriteForCast))
                        throw new InvalidOperationException("Native favorite-only preparation failed.");
                    var onlyPreparation = RawSlots(bookConjurer, 5).Where(value => value.Spell != null && value.Spell.Blueprint == teleport).ToArray();
                    if (onlyPreparation.Length != 1 || onlyPreparation[0].Type != SpellSlotType.Favorite || !onlyPreparation[0].Available)
                        throw new InvalidOperationException("The world-map fixture lacks exactly one ready favorite preparation.");
                    var panel = TeleportationFixturePanel();
                    SelectTeleportationCastingPoint(panel, target);
                    foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                    var rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                    var action = rows == null ? null : rows.Actions.SingleOrDefault(value => value.Source.Spell == TeleportSpellKind.Teleport &&
                        value.Source.BookId == bookConjurer.Blueprint.AssetGuid && value.Source.CasterId == conjurer.UniqueId);
                    SpecialistAssert("world-map-specialist-row", "the favorite-only specialist preparation is composed as a real world-map source",
                        "uses=" + (action == null ? "absent" : action.Source.Uses.ToString()), action != null && action.Source.Uses == 1);
                    if (action == null) throw new InvalidOperationException("No specialist world-map source was composed.");
                    string resourcesBefore = TeleportResourceFingerprint(bookConjurer);
                    rows.QualificationRolls = new TeleportationFixtureRolls(new int[0]);
                    rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == action.Key)].onClick.Invoke();
                    var request = TeleportContextConfirmationPresenter.Current;
                    if (request == null || !DialogMessageBox.Instance.IsShown)
                        throw new InvalidOperationException("The specialist cast did not open its owned confirmation.");
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    if (resourcesBefore != TeleportResourceFingerprint(bookConjurer))
                        throw new InvalidOperationException("Opening the specialist confirmation spent a resource.");
                    TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    bool committed = request.Transaction.State == TeleportTransactionState.Completed &&
                        request.Transaction.Result != null && request.Transaction.Result.Status == TeleportExecutionStatus.Arrived &&
                        request.Transaction.Result.DestinationId == target.Blueprint.AssetGuid;
                    var remainingReady = RawSlots(bookConjurer, 5).Count(value => value.Spell != null && value.Spell.Blueprint == teleport && value.Available);
                    SpecialistAssert("world-map-specialist-spend", "the world map spends exactly the single favorite preparation and relocates",
                        "committed=" + committed + ";expenditure=" + request.Execution.Resource.ObserveExpenditure() + ";remaining=" + remainingReady,
                        committed && request.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne && remainingReady == 0 &&
                        !TeleportContextConfirmationPresenter.Pending && !DialogMessageBox.Instance.IsShown);
                }
                finally
                {
                    // World-map fixture restoration mirrors the interaction family.
                    // A still-open owned confirmation is dismissed through its
                    // native No control before any state restoration.
                    try { if (TeleportContextConfirmationPresenter.Pending && DialogMessageBox.Instance.IsShown) TeleportationFixtureDialogButton("m_ButtonNo").onClick.Invoke(); }
                    catch { /* cleanup must continue; pending state is asserted below */ }
                    map.TravelData = null;
                    rules.StopWhenRevealingNewEdges = true;
                    map.PartyPosition = originalPosition; map.LastLocation = originalLast;
                    map.HistoryTravels.Clear(); foreach (var entry in originalHistory) map.HistoryTravels.Add(entry);
                    map.PerceptionRolledLocations.Clear(); foreach (var entry in originalPerception) map.PerceptionRolledLocations.Add(entry);
                    map.MilesTravelled = originalMiles; player.GameTime = originalTime;
                    foreach (var snapshot in snapshots) snapshot.Restore();
                    payloadState.SetValue(ledger, originalPayload);
                }
            }
            finally
            {
                if (ui.ServiceWindow != null && ui.ServiceWindow.WindowTabs != null && ui.ServiceWindow.WindowTabs.IsShow) ui.ServiceWindow.HandleOpenSpellbook();
                if (evokerFeatureAttached) evoker.Descriptor.RemoveFact(evocationFeature);
                if (conjurerFeatureAttached) conjurer.Descriptor.RemoveFact(conjurationFeature);
                fixtureEvoker.Restore(); fixtureConjurer.Restore();
                // After the world-map phase the local-area selection surfaces are
                // unloaded; selection restoration applies only there.
                var selectionManager = ui.SelectionManagerPC;
                if (selectionManager != null && originalSelection.Select(value => value.View).All(value => value != null))
                    selectionManager.MultiSelect(originalSelection.Select(value => value.View).ToArray(), false);
                foreach (var snapshot in uiSnapshots) snapshot.Restore();
                game.IsPaused = originalPaused;
                bool selectionRestored = selectionManager == null || selectionManager.SelectedUnits.SequenceEqual(originalSelection);
                bool restored = fixtureConjurer.IsRestored() && fixtureEvoker.IsRestored() && uiSnapshots.All(value => value.IsRestored()) &&
                    !conjurer.Descriptor.HasFact(conjurationFeature) && !evoker.Descriptor.HasFact(evocationFeature) &&
                    selectionRestored && player.Party.SequenceEqual(originalParty) &&
                    !TeleportContextConfirmationPresenter.Pending && !_workingSaveSmoke.WriteObserved;
                CaptureTeleportationSpecialist("cleanup", new { restored });
                SpecialistAssert("cleanup", "exact original books, features, action bars, selection, party and world state; zero writes",
                    "restored=" + restored, restored);
            }
        }

        private static readonly MethodInfo SureMemorizedSpellsMethod = typeof(Spellbook)
            .GetMethod("SureMemorizedSpells", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
        private static List<SpellSlot> RawSlots(Spellbook book, int level)
        {
            var list = SureMemorizedSpellsMethod.Invoke(book, new object[] { level }) as List<SpellSlot>;
            if (list == null) throw new InvalidOperationException("Native memorized slot list differs.");
            return list;
        }

        private void SpecialistAssert(string id, string expected, string actual, bool pass)
        { _teleportationSpecialistAssertions.Add(Assertion("teleportation-specialist-" + id, expected, actual, pass,
            Path.Combine(_request.EvidenceDirectory, "teleportation-specialist.json"))); }
        private void CaptureTeleportationSpecialist(string step, object state)
        { _teleportationSpecialistCaptures.Add(new { step, frame = Time.frameCount, state }); }

        private void PollTeleportationSpecialist()
        {
            if (!IsTeleportationSpecialistFixture || !_request.ExitAfterCompletion ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Specialist qualification requires its guarded working save, automatic exit and intact write sentinels.");
            if (_teleportationSpecialistSteps == null) _teleportationSpecialistSteps = RunTeleportationSpecialist().GetEnumerator();
            Exception failure = null;
            try { if (_teleportationSpecialistSteps.MoveNext()) return; }
            catch (Exception exception) { failure = exception; }
            try { _teleportationSpecialistSteps.Dispose(); }
            catch (Exception exception) { failure = failure == null ? exception : new AggregateException(failure, exception); }
            _teleportationSpecialistSteps = null;
            WriteTeleportationForensicJson(Path.Combine(_request.EvidenceDirectory, "teleportation-specialist.json"), new {
                schemaVersion = 1, runId = _request.RunId,
                claims = "Real Conjuration-specialist book prepares Teleport in the fifth-level favorite slot through the native spellbook UI, native rest readies it, and the world map spends exactly that preparation. Request-local books/school features/visited state; no save writes.",
                captures = _teleportationSpecialistCaptures, assertions = _teleportationSpecialistAssertions,
                saveWriteObserved = _workingSaveSmoke.WriteObserved, error = failure == null ? null : failure.ToString() });
            Complete(CreateResult(failure != null ? RuntimeTestStatuses.Error : _teleportationSpecialistAssertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, _teleportationSpecialistAssertions, failure == null ? null : failure.ToString()));
        }
    }
}
