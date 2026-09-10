using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.GlobalMap;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _teleportationInteractionSteps;
        private readonly List<RuntimeTestAssertion> _teleportationInteractionAssertions = new List<RuntimeTestAssertion>();
        private readonly List<object> _teleportationInteractionCaptures = new List<object>();
        private bool IsTeleportationTravelersFixture { get { return _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationTravelers; } }
        private bool IsTeleportationArrowsFixture { get { return _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationArrows; } }
        private string TeleportationInteractionPath { get { return Path.Combine(_request.EvidenceDirectory, IsTeleportationCoexistenceFixture ? "teleportation-coexistence.json" : IsTeleportationDisabledFixture ? "teleportation-disabled.json" : IsTeleportationDestinationsFixture ? "teleportation-destinations.json" : IsTeleportationTravelersFixture ? "teleportation-travelers.json" : IsTeleportationGamepadFixture ? "teleportation-gamepad.json" : IsTeleportationArrowsFixture ? "teleportation-arrows.json" : "teleportation-interaction.json"); } }

        private void PollTeleportationInteraction()
        {
            if ((_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationInteraction && !IsTeleportationTravelersFixture && !IsTeleportationGamepadFixture && !IsTeleportationDestinationsFixture && !IsTeleportationDisabledFixture && !IsTeleportationCoexistenceFixture && !IsTeleportationArrowsFixture) || !_request.ExitAfterCompletion ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Multi-frame interaction requires its guarded named working save, automatic exit and intact write sentinels.");
            if (_teleportationMapLoad == null)
            {
                _teleportationMapLoad = Stopwatch.StartNew();
                Game.Instance.LoadArea(Game.Instance.BlueprintRoot.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None);
                return;
            }
            if (_teleportationMapLoad.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Multi-frame interaction timed out.");
            if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive ||
                GlobalMapRules.Instance == null || Game.Instance.CurrentMode != GameModeType.GlobalMap) return;
            if (IsTeleportationGamepadFixture && !PrepareTeleportGamepadUi()) return;
            if (_teleportationInteractionSteps == null) _teleportationInteractionSteps = (IsTeleportationCoexistenceFixture ? RunTeleportationCoexistence() : IsTeleportationDisabledFixture ? RunTeleportationDisabled() : RunTeleportationInteraction()).GetEnumerator();
            Exception failure = null;
            try { if (_teleportationInteractionSteps.MoveNext()) return; }
            catch (Exception exception) { failure = exception; }
            try { _teleportationInteractionSteps.Dispose(); }
            catch (Exception exception) { failure = failure == null ? exception : new AggregateException(failure, exception); }
            _teleportationInteractionSteps = null;
            RestoreTeleportationController();
            WriteTeleportationInteraction(failure == null ? null : failure.ToString());
            Complete(CreateResult(failure != null ? RuntimeTestStatuses.Error : _teleportationInteractionAssertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, _teleportationInteractionAssertions, failure == null ? null : failure.ToString()));
        }

        // Complete also calls this on an outer timeout/error before closing the
        // save-write sentinels. Iterator disposal executes the fixture's finally.
        private void StopTeleportationInteraction(RuntimeTestResult result)
        {
            if (_teleportationInteractionSteps == null) { RestoreTeleportationController(); return; }
            var steps = _teleportationInteractionSteps;
            _teleportationInteractionSteps = null;
            string error = "Runner completed before the multi-frame fixture finished.";
            try { steps.Dispose(); }
            catch (Exception exception) { error += " " + exception; result.Status = RuntimeTestStatuses.Error; }
            RestoreTeleportationController();
            WriteTeleportationInteraction(error);
            result.Diagnostics.Add(error);
        }
        private void WriteTeleportationInteraction(string error)
        {
            WriteTeleportationForensicJson(TeleportationInteractionPath, new { schemaVersion = 1, runId = _request.RunId,
                claims = IsTeleportationCoexistenceFixture ? "Request-local independent foreign action inserted before KMG augmentation; same control, callback and navigation survive native/KMG lifecycle. Real native prepared sources; exact cleanup; no save writes." : IsTeleportationDisabledFixture ? "Actual native destination selection, dismissal and Travel with the module OFF, real request-local book resources and existing ledger fields. No spell casting or save writes; exact fixture cleanup." : IsTeleportationDestinationsFixture ? "Real contextual Greater Teleport at native book-event/component points and all stable point types, preserving native prohibitions and deferred relocation invariants. Request-local real book/visited-state fixture; no save writes or campaign prohibition changes." : IsTeleportationGamepadFixture ? "Native gamepad UI scene, original navigation/input handlers, real contextual casting, modal ownership and input-layer cleanup across Unity frames. No OS input or controller emulation. Request-local controller mode, map/book/ledger fixture, no save writes." : IsTeleportationTravelersFixture ? "Request-local native associated pets, real contextual casting, native damage/life events and exact cleanup across Unity frames. No save writes or life-state threshold replacement." : IsTeleportationArrowsFixture ? "Post-teleport directional-arrow diagnostics: native direction-marker inventory, exact CalculatePathByMarker results and real-handler first-action behavior at each mission boundary. Request-local book/visited-state fixture; no save writes." :
                    "Native panel, button, Escape stack, confirmation and movement-event evidence across actual Unity frames. No synthetic input or screen coordinates. Ordinary travel uses request-local native time input; magical casting uses the production path.",
                captures = _teleportationInteractionCaptures, assertions = _teleportationInteractionAssertions,
                destinationExceptions = IsTeleportationDestinationsFixture ? _teleportationDestinationExceptions : null,
                disabledExceptions = IsTeleportationDisabledFixture ? _teleportationDisabledExceptions : null,
                saveWriteObserved = _workingSaveSmoke.WriteObserved, error });
        }
        private void TeleportInteractionAssert(string id, string expected, string actual, bool pass)
        { _teleportationInteractionAssertions.Add(Assertion((IsTeleportationCoexistenceFixture ? "teleportation-coexistence-" : IsTeleportationDisabledFixture ? "teleportation-disabled-" : IsTeleportationDestinationsFixture ? "teleportation-destinations-" : IsTeleportationTravelersFixture ? "teleportation-travelers-" : IsTeleportationGamepadFixture ? "teleportation-gamepad-" : IsTeleportationArrowsFixture ? "teleportation-arrows-" : "teleportation-interaction-") + id, expected, actual, pass, TeleportationInteractionPath)); }
        private void CaptureTeleportInteraction(string step, object state)
        { _teleportationInteractionCaptures.Add(new { step, frame = Time.frameCount, state }); }

        private IEnumerable<int> RunTeleportationInteraction()
        {
            var player = Game.Instance.Player;
            var rules = GlobalMapRules.Instance;
            var map = GlobalMapRules.State;
            var ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
            var panel = IsTeleportationGamepadFixture ? null : TeleportationFixturePanel();
            var rig = Resources.FindObjectsOfTypeAll<CameraRig>().Single(value => value != null && value.gameObject.activeInHierarchy &&
                value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded);
            if (IsTeleportationGamepadFixture) CaptureTeleportInteraction("native-controller-prerequisites", DescribeTeleportGamepadHosts());
            if (ledger == null || !WorldMapPointSpellActionPatches.Installed || map.TravelData != null || map.CurrentEncounterData != null ||
                TeleportationConfirmationSurface.Available() == null || TeleportContextConfirmationPresenter.Pending || Game.Instance.IsControllerGamepad != IsTeleportationGamepadFixture ||
                IsTeleportationGamepadFixture && !WorldMapPointConsoleSpellActionPatches.Installed)
                throw new InvalidOperationException("Native stationary interaction/controller prerequisites differ.");
            var originalCamera = rig.GetPosition();
            var originalPosition = map.PartyPosition;
            var originalLast = map.LastLocation;
            var originalTime = player.GameTime;
            float originalDelta = Game.Instance.TimeController.DeltaTime;
            float originalMiles = map.MilesTravelled;
            bool originalStop = rules.StopWhenRevealingNewEdges;
            var originalHistory = map.HistoryTravels.ToArray();
            var originalPerception = map.PerceptionRolledLocations.ToArray();
            var originalRoster = TeleportationTravelers.Read(player);
            var pointRecords = map.Locations.ToArray();
            var edgeRecords = map.Edges.ToArray();
            var snapshots = pointRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value))
                .Concat(edgeRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value)))
                .Concat(new[] { new TeleportNativeFieldSnapshot(ledger) }).ToArray();
            var payload = typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
            var originalPayload = payload.GetValue(ledger);
            var owners = new List<TeleportResourceFixtureOwner>();
            var movement = new TeleportInteractionMovementObserver();
            EventBus.Subscribe(movement);
            if (IsTeleportationDestinationsFixture) Application.logMessageReceived += ObserveTeleportDestinationException;
            try
            {
                var chain = FindTeleportInteractionChain(rules);
                GlobalMapLocation origin = chain[0], middle = chain[1], target = chain[2];
                var setRevealed = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
                foreach (var point in chain) { setRevealed.Invoke(point.Data, new object[] { true }); point.Data.EdgesOpened = true; }
                foreach (var edge in middle.Edges.Where(value => value.GetOppositeLocation(middle) == origin || value.GetOppositeLocation(middle) == target))
                    edge.Data.UpdateExplored(1, 1);
                var familiarity = new TeleportFamiliarityState(); familiarity.MigrateLegacy(chain.Select(value => value.Blueprint.AssetGuid));
                payload.SetValue(ledger, familiarity.Serialize());
                rules.StopWhenRevealingNewEdges = false;
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                rig.ScrollTo(target.transform.position);
                int cameraFrame = Time.frameCount;
                // Real camera updates settle the native point anchor. No hard-coded
                // screen position, transform mutation, or synthetic pointer input.
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (IsTeleportationGamepadFixture)
                {
                    foreach (int tick in RunTeleportationGamepad(origin, middle, target, owners, movement)) yield return tick;
                    yield break;
                }
                if (IsTeleportationDestinationsFixture)
                {
                    foreach (int tick in RunTeleportationDestinations(origin, owners, movement)) yield return tick;
                    yield break;
                }
                if (IsTeleportationArrowsFixture)
                {
                    foreach (int tick in RunTeleportationArrows(origin, middle, target, owners, movement)) yield return tick;
                    yield break;
                }
                if (IsTeleportationTravelersFixture)
                {
                    foreach (int tick in RunTeleportationTravelers(origin, target, owners)) yield return tick;
                    yield break;
                }
                panel.OnLocationSelect(target.Blueprint, false);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                var dialog = (CanvasGroup)WorldMapPointSpellActionPatches.DialogField.GetValue(panel);
                var accept = ((TextMeshProUGUI)WorldMapPointSpellActionPatches.AcceptTextField.GetValue(panel)).GetComponentInParent<Button>();
                string nativeActions = TeleportationNativeButtons(panel);
                CaptureTeleportInteraction("no-source-native-panel", new { nativeActions, onScreen = UIUtility.IsTransformInScreen(dialog.transform),
                    dialog.alpha, panel = DescribeTeleportationNativePanel(panel), context = TeleportationWorldMapAdapter.Capture(false).Diagnostic });
                TeleportInteractionAssert("vanilla-no-source", "native panel only after actual frames; no spell UI, no movement, no familiarity increment",
                    "frameDelta=" + (Time.frameCount - cameraFrame), Time.frameCount > cameraFrame && panel.gameObject.activeInHierarchy &&
                    panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 0 && !TeleportContextConfirmationPresenter.Pending &&
                    !DialogMessageBox.Instance.IsShown && movement.Starts == 0 && map.TravelData == null && ledger.Read().Serialize() == familiarity.Serialize());
                TeleportInteractionAssert("vanilla-visible", "native point panel is on screen after native camera scrolling",
                    "onScreen=" + UIUtility.IsTransformInScreen(dialog.transform) + ";alpha=" + dialog.alpha,
                    UIUtility.IsTransformInScreen(dialog.transform) && dialog.alpha > 0.99f && accept.IsActive() && accept.IsInteractable());
                Game.Instance.UI.EscManager.OnEscPressed();
                yield return 0;
                TeleportInteractionAssert("native-escape", "native Escape dismisses the destination without travel",
                    "visible=" + panel.gameObject.activeInHierarchy, !panel.gameObject.activeInHierarchy && movement.Starts == 0 && map.TravelData == null);
                RunTeleportInteractionTravel(panel, origin, middle, target, ledger, movement, "vanilla", () => string.Empty);
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                player.GameTime = originalTime;
                var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "ba34257984f4c41408ce1dc2004e342e", "native interaction Wizard");
                var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "b3a505fb61437dc4097f43c3f8f9a4cf", "native interaction Sorcerer");
                var units = player.Party.Where(TeleportationSpellbookAdapter.CasterAvailable).Where(value =>
                    value.Descriptor.GetSpellbook(wizard.Spellbook) == null && value.Descriptor.GetSpellbook(sorcerer.Spellbook) == null).Take(2).ToArray();
                if (units.Length != 2) throw new InvalidOperationException("Two existing active-party owners without fixture books required.");
                foreach (var unit in units) owners.Add(new TeleportResourceFixtureOwner(unit));
                var books = new[] { owners[0].AddBook(wizard.Spellbook), owners[0].AddBook(sorcerer.Spellbook), owners[1].AddBook(wizard.Spellbook) };
                foreach (var book in books)
                {
                    book.AddKnown(5, BlueprintBootstrap.Teleportation.Teleport, true);
                    book.AddKnown(7, BlueprintBootstrap.Teleportation.GreaterTeleport, true);
                    if (!book.Blueprint.Spontaneous && (!book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.Teleport, book), null) ||
                        !book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.Teleport, book), null) ||
                        !book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.GreaterTeleport, book), null)))
                        throw new InvalidOperationException("Native fixture preparation failed.");
                    book.Rest();
                }
                Func<string> slots = () => string.Join("|", books.Select(TeleportResourceFingerprint));
                string beforeSlots = slots();
                string beforeFamiliarity = ledger.Read().Serialize();
                panel.OnLocationSelect(target.Blueprint, false);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                var rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                CaptureTeleportInteraction("augmented-native-panel", new { nativeActions = TeleportationNativeButtons(panel),
                    panel = DescribeTeleportationNativePanel(panel), onScreen = UIUtility.IsTransformInScreen(dialog.transform),
                    rows = rows == null ? 0 : rows.Actions.Count, dialog.alpha });
                TeleportInteractionAssert("augmented-visible", "six usable native rows remain laid out and on screen across frames",
                    "rows=" + (rows == null ? 0 : rows.Actions.Count), rows != null && rows.Actions.Count == 6 && dialog.alpha > 0.99f &&
                    UIUtility.IsTransformInScreen(dialog.transform) && rows.Buttons.All(value => value.IsActive() && value.IsInteractable()) &&
                    rows.Buttons.Select(value => ((RectTransform)value.transform).anchoredPosition.y).Distinct().Count() == 6 &&
                    rows.Buttons.Select((value, index) => value.GetComponentInChildren<TextMeshProUGUI>(true).text ==
                        TeleportContextPresentation.CompactRow(rows.Actions[index], TeleportationText.Get)).All(value => value));
                TeleportInteractionAssert("native-actions-retained", "native action order, labels, flags and serialized callbacks unchanged",
                    "same=" + (nativeActions == TeleportationNativeButtons(panel)), nativeActions == TeleportationNativeButtons(panel));
                float firstViewportHeight = rows.GetComponent<ScrollRect>().viewport.rect.height;
                var reopenedHeights = new List<float>();
                for (int repeat = 0; repeat < 8; repeat++)
                {
                    panel.OnLocationSelect(target.Blueprint, false); yield return 0;
                    reopenedHeights.Add(panel.GetComponentInChildren<TeleportDestinationRows>(true).GetComponent<ScrollRect>().viewport.rect.height);
                }
                CaptureTeleportInteraction("reopen-viewport-heights", new { firstViewportHeight, reopenedHeights });
                TeleportInteractionAssert("reopen-viewport-stable", "each reopen measures the native body independently of previous spell rows",
                    "heights=" + string.Join(",", reopenedHeights), reopenedHeights.All(value => Math.Abs(value - firstViewportHeight) < 0.01f));
                rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                TeleportInteractionAssert("reopen-deferred-cleanup", "reopening across deferred Unity destruction keeps exactly one container and six distinct rows",
                    "containers=" + panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length,
                    rows != null && panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 1 && rows.Actions.Count == 6 &&
                    rows.Actions.Select(value => value.Key).Distinct().Count() == 6 && beforeSlots == slots() && beforeFamiliarity == ledger.Read().Serialize());
                Game.Instance.UI.EscManager.OnEscPressed();
                yield return 0;
                TeleportInteractionAssert("augmented-escape", "native Escape removes spell rows without spending or starting travel",
                    "visible=" + panel.gameObject.activeInHierarchy, !panel.gameObject.activeInHierarchy &&
                    panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 0 && beforeSlots == slots() && map.TravelData == null);

                bool targetExplored = target.Data.IsExplored;
                string targetLedger = ledger.Read().Serialize();
                target.Data.EdgesOpened = false; target.Data.IsExplored = false;
                var unseen = new TeleportFamiliarityState(); unseen.MigrateLegacy(new[] { origin.Blueprint.AssetGuid, middle.Blueprint.AssetGuid });
                payload.SetValue(ledger, unseen.Serialize());
                panel.OnLocationSelect(target.Blueprint, false);
                for (int frame = 0; frame < 4; frame++) yield return 0;
                TeleportInteractionAssert("unvisited-native-panel", "unvisited point retains native interaction with no magical row or confirmation",
                    "rows=" + panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length,
                    panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 0 && !TeleportContextConfirmationPresenter.Pending && beforeSlots == slots());
                panel.Hide(); target.Data.EdgesOpened = true; target.Data.IsExplored = targetExplored; payload.SetValue(ledger, targetLedger);
                panel.OnLocationSelect(origin.Blueprint, false);
                yield return 0;
                TeleportInteractionAssert("current-point-native-path", "current empty crossroads retains native no-dialog path without a no-op spell action",
                    "visible=" + panel.gameObject.activeInHierarchy, !panel.gameObject.activeInHierarchy &&
                    panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 0 && beforeSlots == slots());

                foreach (string cancel in new[] { "escape", "force-close", "stale-destination", "replacement-dialog" })
                {
                    var request = OpenTeleportationFixtureConfirmation(panel, target, TeleportSpellKind.Teleport,
                        TeleportCastSourceKind.Prepared, new TeleportationFixtureRolls(new[] { 1 }));
                    int start = Time.frameCount;
                    for (int frame = 0; frame < 20; frame++) yield return 0;
                    TeleportInteractionAssert(cancel + "-confirmation-stable", "selected source confirmation remains pending without spending across frames",
                        "state=" + request.Transaction.State, ReferenceEquals(TeleportContextConfirmationPresenter.Current, request) &&
                        DialogMessageBox.Instance.IsShown && !panel.gameObject.activeInHierarchy && beforeSlots == slots() && Time.frameCount > start);
                    bool replacementCalled = false;
                    if (cancel == "escape") Game.Instance.UI.EscManager.OnEscPressed();
                    else if (cancel == "force-close") DialogMessageBox.Instance.HandleForceClose();
                    else if (cancel == "stale-destination") target.Data.IsClosed = true;
                    else
                    {
                        Action openReplacement = () => EventBus.RaiseEvent<IDialogMessageBoxUIHandler>(handler => handler.HandleOpen("Guarded unrelated native dialog",
                            DialogMessageBoxBase.BoxType.Dialog, button => replacementCalled = true, "OK", "Cancel", null, null));
                        openReplacement();
                        TeleportInteractionAssert("native-overlap-refused", "native HandleOpen refuses a second dialog while the spell confirmation is shown",
                            "state=" + request.Transaction.State, ReferenceEquals(TeleportContextConfirmationPresenter.Current, request) &&
                            request.Transaction.State == TeleportTransactionState.Pending && !replacementCalled && beforeSlots == slots());
                        // Native replacement requires closing first. Reopen within
                        // the same frame so the production ownership guard, rather
                        // than a prior Update, must cancel the old request.
                        DialogMessageBox.Instance.HandleForceClose();
                        openReplacement();
                    }
                    for (int frame = 0; frame < 4; frame++) yield return 0;
                    CaptureTeleportInteraction(cancel, new { transaction = request.Transaction.State.ToString(), pending = TeleportContextConfirmationPresenter.Pending,
                        dialogShown = DialogMessageBox.Instance.IsShown, replacementCalled, slotsUnchanged = beforeSlots == slots() });
                    TeleportInteractionAssert(cancel, "dismissal/invalidity cancels only the owned request, spends nothing and starts no route",
                        "state=" + request.Transaction.State, request.Transaction.State == TeleportTransactionState.Cancelled &&
                        !TeleportContextConfirmationPresenter.Pending && beforeSlots == slots() && map.TravelData == null &&
                        map.PartyLocation == origin.Blueprint && (cancel == "replacement-dialog" ? DialogMessageBox.Instance.IsShown && !replacementCalled : !DialogMessageBox.Instance.IsShown));
                    target.Data.IsClosed = false;
                    if (cancel == "replacement-dialog") TeleportationFixtureDialogButton("m_ButtonNo").onClick.Invoke();
                    for (int frame = 0; frame < 4; frame++) yield return 0;
                }

                panel.OnLocationSelect(target.Blueprint, false);
                yield return 0;
                rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                var action = rows.Actions.First(value => value.Source.Spell == TeleportSpellKind.Teleport && value.Source.BookId == books[0].Blueprint.AssetGuid && value.Source.CasterId == units[0].UniqueId);
                var resource = TeleportationSpellbookAdapter.Resolve(action.Source);
                if (resource == null || !resource.Book.Spend(resource.Ability, false)) throw new InvalidOperationException("Native external fixture debit failed.");
                for (int frame = 0; frame < 4; frame++) yield return 0;
                var fresh = rows.Actions.Single(value => value.Key == action.Key);
                TeleportInteractionAssert("live-source-count", "already-open row reads the current prepared use count after an actual native debit",
                    "before=" + action.Source.Uses + ";after=" + fresh.Source.Uses, fresh.Source.Uses == action.Source.Uses - 1 &&
                    rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == action.Key)].GetComponentInChildren<TextMeshProUGUI>(true).text ==
                    TeleportContextPresentation.CompactRow(fresh, TeleportationText.Get));
                resource = TeleportationSpellbookAdapter.Resolve(fresh.Source);
                if (resource == null || !resource.Book.Spend(resource.Ability, false)) throw new InvalidOperationException("Final native prepared fixture debit failed.");
                for (int frame = 0; frame < 4; frame++) yield return 0;
                TeleportInteractionAssert("exhausted-row-removed", "the exhausted source row disappears entirely while remaining real sources stay usable",
                    "rows=" + rows.Actions.Count, rows.Actions.Count == 5 && rows.Actions.All(value => value.Key != action.Key) &&
                    rows.Buttons.All(value => value.IsActive() && value.IsInteractable()));
                foreach (var book in books) book.Rest();
                panel.Hide();
                beforeSlots = slots();
                RunTeleportInteractionTravel(panel, origin, middle, target, ledger, movement, "augmented", slots);
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition(); player.GameTime = originalTime;
                for (int frame = 0; frame < 4; frame++) yield return 0;
                int castStarts = movement.Starts, castStops = movement.Stops;
                var cast = OpenTeleportationFixtureConfirmation(panel, target, TeleportSpellKind.GreaterTeleport,
                    TeleportCastSourceKind.Spontaneous, new TeleportationFixtureRolls(new int[0]));
                for (int frame = 0; frame < 20; frame++) yield return 0;
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                CaptureTeleportInteraction("cast-after-frames", new { transaction = cast.Transaction.State.ToString(), cast.Transaction.Diagnostic,
                    evidence = cast.Execution.LastEvidence, movement.Starts, movement.Stops });
                TeleportInteractionAssert("cast-after-frames", "real native Cast after UI updates spends one slot and completes exact relocation raising exactly the native pawn-notification pair",
                    "state=" + cast.Transaction.State, cast.Transaction.State == TeleportTransactionState.Completed &&
                    cast.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne && map.PartyLocation == target.Blueprint &&
                    movement.Starts == castStarts + 1 && movement.Stops == castStops + 1 && !TeleportContextConfirmationPresenter.Pending && !DialogMessageBox.Instance.IsShown && map.TravelData == null);

                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                var druid = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "610d836f3a3a9ed42a4349b62f002e96", "native long-list fixture Druid");
                // Request-local real books supply enough distinct resource pools
                // to exercise overflow. No production spell is granted to a unit.
                var extraBooks = new[] { owners[1].AddBook(sorcerer.Spellbook), owners[0].AddBook(druid.Spellbook), owners[1].AddBook(druid.Spellbook) };
                foreach (var book in extraBooks)
                {
                    book.AddKnown(5, BlueprintBootstrap.Teleportation.Teleport, true);
                    book.AddKnown(7, BlueprintBootstrap.Teleportation.GreaterTeleport, true);
                    if (!book.Blueprint.Spontaneous && (!book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.Teleport, book), null) ||
                        !book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.GreaterTeleport, book), null)))
                        throw new InvalidOperationException("Extra real native source preparation failed.");
                    book.Rest();
                }
                panel.OnLocationSelect(target.Blueprint, false);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                var scroll = rows.GetComponent<ScrollRect>();
                CaptureTeleportInteraction("long-source-list", new { rows = rows.Actions.Count, contentHeight = scroll.content.rect.height,
                    viewportHeight = scroll.viewport.rect.height, onScreen = UIUtility.IsTransformInScreen(dialog.transform) });
                TeleportInteractionAssert("long-source-list", "twelve distinct real rows use an on-screen native scroll viewport while keeping all native actions",
                    "rows=" + rows.Actions.Count + ";viewport=" + scroll.viewport.rect.height, rows.Actions.Count == 12 &&
                    rows.Actions.Select(value => value.Key).Distinct().Count() == 12 && UIUtility.IsTransformInScreen(dialog.transform) &&
                    scroll.content.rect.height > scroll.viewport.rect.height && nativeActions == TeleportationNativeButtons(panel));
                scroll.verticalNormalizedPosition = 0;
                for (int frame = 0; frame < 4; frame++) yield return 0;
                var lastBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, rows.Buttons.Last().transform);
                TeleportInteractionAssert("long-source-last-row-reachable", "native ScrollRect reaches the final real source without another screen or duplicate actions",
                    "normalized=" + scroll.verticalNormalizedPosition + ";lastCenter=" + lastBounds.center,
                    scroll.viewport.rect.Contains(lastBounds.center) && rows.Buttons.Last().IsInteractable() &&
                    panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 1 && !TeleportContextConfirmationPresenter.Pending);
            }
            finally
            {
                EventBus.Unsubscribe(movement);
                if (IsTeleportationGamepadFixture) CloseTeleportGamepadPanels(); else CloseTeleportationFixturePanels();
                foreach (var owner in owners.AsEnumerable().Reverse()) owner.Restore();
                map.TravelData = null;
                foreach (var snapshot in snapshots) snapshot.Restore();
                foreach (var key in map.Locations.Keys.Where(value => !pointRecords.Any(pair => pair.Key == value)).ToArray()) map.Locations.Remove(key);
                foreach (var key in map.Edges.Keys.Where(value => !edgeRecords.Any(pair => pair.Key == value)).ToArray()) map.Edges.Remove(key);
                payload.SetValue(ledger, originalPayload);
                map.HistoryTravels.Clear(); map.HistoryTravels.AddRange(originalHistory);
                map.PerceptionRolledLocations.Clear(); map.PerceptionRolledLocations.UnionWith(originalPerception);
                rules.SetCurrentPosition(originalPosition); rules.UpdatePawnPosition(); map.LastLocation = originalLast;
                map.MilesTravelled = originalMiles; rules.StopWhenRevealingNewEdges = originalStop;
                Game.Instance.TimeController.SetDeltaTime(originalDelta); player.GameTime = originalTime;
                rig.ScrollTo(originalCamera);
                bool restored = snapshots.All(value => value.Matches()) && owners.All(value => value.IsRestored()) &&
                    Equals(payload.GetValue(ledger), originalPayload) && map.HistoryTravels.SequenceEqual(originalHistory) &&
                    map.PerceptionRolledLocations.SequenceEqual(originalPerception) && originalRoster.Matches(TeleportationTravelers.Read(player)) &&
                    map.TravelData == null && map.CurrentEncounterData == null && !_workingSaveSmoke.WriteObserved &&
                    !TeleportContextConfirmationPresenter.Pending && TeleportationConfirmationSurface.Available() != null && player.GameTime == originalTime;
                CaptureTeleportInteraction("cleanup", new { restored, movement.Starts, movement.Stops, cameraTargetRestored = rig.GetPosition() == originalCamera });
                if (IsTeleportationDestinationsFixture)
                {
                    Application.logMessageReceived -= ObserveTeleportDestinationException;
                    TeleportInteractionAssert("exceptions", "zero exceptions during destination fixture setup, casting and cleanup",
                        "count=" + _teleportationDestinationExceptions.Count, _teleportationDestinationExceptions.Count == 0);
                }
                TeleportInteractionAssert("cleanup", "exact original resource owners, map fields, ledger, time input, roster and UI restored before save sentinel closes",
                    "restored=" + restored, restored);
            }
        }

        private static IEnumerable<int> WaitTeleportInteractionPanel(GlobalMapMessageBox panel)
        {
            var watch = Stopwatch.StartNew();
            var dialog = (CanvasGroup)WorldMapPointSpellActionPatches.DialogField.GetValue(panel);
            // Frame count alone is not an animation deadline at uncapped FPS.
            // Wait for the native fade's observable completion, then let layout
            // and deferred destruction settle through additional real updates.
            while (dialog.alpha < 0.99f)
            {
                if (!panel.gameObject.activeInHierarchy || watch.Elapsed.TotalSeconds > 3)
                    throw new InvalidOperationException("Native point panel did not finish its fade.");
                yield return 0;
            }
            for (int frame = 0; frame < 4; frame++) yield return 0;
        }
        private static GlobalMapLocation[] FindTeleportInteractionChain(GlobalMapRules rules)
        {
            foreach (var middle in rules.AllLocations.Where(TeleportFamiliarityFixturePoint).OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal))
            {
                var edges = middle.Edges.Where(value => value != null && !value.IsLocked && value.Spline != null && value.Spline.WorldLength > 0 &&
                    TeleportFamiliarityFixturePoint(value.GetOppositeLocation(middle)))
                    .OrderBy(value => value.Spline.WorldLength).ThenBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal).ToArray();
                if (edges.Length > 1 && edges[0].GetOppositeLocation(middle) != edges[1].GetOppositeLocation(middle))
                    return new[] { edges[0].GetOppositeLocation(middle), middle, edges[1].GetOppositeLocation(middle) };
            }
            throw new InvalidOperationException("No safe native crossroads chain for interaction fixture.");
        }
        private void RunTeleportInteractionTravel(GlobalMapMessageBox panel, GlobalMapLocation origin, GlobalMapLocation middle,
            GlobalMapLocation target, UnitPartTeleportFamiliarity ledger, TeleportInteractionMovementObserver movement, string name, Func<string> slots)
        {
            var rules = GlobalMapRules.Instance;
            var map = GlobalMapRules.State;
            var time = Game.Instance.Player.GameTime;
            float delta = Game.Instance.TimeController.DeltaTime;
            string beforeSlots = slots();
            var before = ledger.Read();
            int starts = movement.Starts;
            panel.OnLocationSelect(target.Blueprint, false);
            int rowCount = panel.GetComponentsInChildren<TeleportDestinationRows>(true).Sum(value => value.Actions.Count);
            var accept = ((TextMeshProUGUI)WorldMapPointSpellActionPatches.AcceptTextField.GetValue(panel)).GetComponentInParent<Button>();
            accept.onClick.Invoke();
            var travel = map.TravelData;
            CaptureTeleportInteraction(name + "-native-travel-start", new { startsBefore = starts, startsAfter = movement.Starts,
                rowCount, route = DescribeFamiliarityRoute(travel), slotsUnchanged = beforeSlots == slots() });
            if (travel == null || !travel.Walking || travel.Path.Count != 2) throw new InvalidOperationException("Native action did not create the expected two-edge travel.");
            TeleportInteractionAssert(name + "-travel-once", "original native Travel button starts one route, without spell debit or recursive UI",
                "startsDelta=" + (movement.Starts - starts), movement.Starts == starts + 1 && beforeSlots == slots() &&
                (name == "vanilla" ? rowCount == 0 : rowCount == 6) && !panel.gameObject.activeInHierarchy && !TeleportContextConfirmationPresenter.Pending);
            var crossed = travel.Path.Select(value => value.Direction > 0 ? rules.GetEdgeObject(value.Blueprint).Location2 : rules.GetEdgeObject(value.Blueprint).Location1).ToArray();
            if (crossed[0] != middle || crossed[1] != target) throw new InvalidOperationException("Native route differs from the safe chain.");
            try
            {
                float speed = Game.Instance.BlueprintRoot.GlobalMap.VisualSpeedBase * (float)typeof(MapMovementController)
                    .GetMethod("CalcSpeedModifiers", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                if (speed <= 0 || float.IsNaN(speed) || float.IsInfinity(speed)) throw new InvalidOperationException("Unusable native movement speed.");
                float length = travel.Path.Sum(value => rules.GetEdgeObject(value.Blueprint).Spline.WorldLength);
                Game.Instance.TimeController.SetDeltaTime((length + 0.01f) / speed);
                new MapMovementController().Tick();
                TeleportInteractionAssert(name + "-ordinary-arrival", "native movement completes; intermediate/target counts increase once and origin count is unchanged",
                    "ledger=" + ledger.Read().Serialize(), map.TravelData == null && map.PartyLocation == target.Blueprint &&
                    crossed.All(value => ledger.Read().Count(value.Blueprint.AssetGuid) == before.Count(value.Blueprint.AssetGuid) + 1) &&
                    ledger.Read().Count(origin.Blueprint.AssetGuid) == before.Count(origin.Blueprint.AssetGuid) && beforeSlots == slots());
            }
            finally { Game.Instance.TimeController.SetDeltaTime(delta); Game.Instance.Player.GameTime = time; panel.Hide(); }
        }
        private sealed class TeleportInteractionMovementObserver : IPawnMovementHandler
        {
            internal int Starts, Stops;
            public void OnPawnMovementStarted() { Starts++; }
            public void OnPawnMovementStopped() { Stops++; }
        }
    }
}
