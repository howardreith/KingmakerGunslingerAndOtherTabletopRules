using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.GlobalMap;
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
        private bool IsTeleportationDisabledFixture { get { return _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationDisabled; } }
        private readonly List<object> _teleportationDisabledExceptions = new List<object>();
        private void ObserveTeleportDisabledException(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error && message.Contains("Exception"))
                _teleportationDisabledExceptions.Add(new { message, stack, type = type.ToString(), frame = Time.frameCount });
        }
        private IEnumerable<int> RunTeleportationDisabled()
        {
            var game = Game.Instance; var player = game.Player;
            var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State;
            if (!IsTeleportationDisabledFixture || !_request.ExitAfterCompletion || _workingSaveSmoke.WriteObserved ||
                _context.FeatureModules.Active.TeleportationSpells || BlueprintBootstrap.TeleportationPublication != null ||
                TeleportFamiliarityRuntime.Enabled || TeleportFamiliarityPatches.Installed || TeleportExplorationGuardPatches.Installed ||
                WorldMapPointSpellActionPatches.Installed || WorldMapPointConsoleSpellActionPatches.Installed ||
                game.IsControllerGamepad || map.TravelData != null || map.CurrentEncounterData != null)
                throw new InvalidOperationException("Disabled interaction requires its guarded stationary working save with Teleportation OFF and no installed feature hooks.");
            var panel = TeleportationFixturePanel(); var rig = TeleportationCastingCamera();
            var main = player.MainCharacter.Value;
            if (main == null) throw new InvalidOperationException("No canonical save owner.");
            var originalLedger = main.Descriptor.Get<UnitPartTeleportFamiliarity>();
            var originalPosition = map.PartyPosition; var originalLast = map.LastLocation;
            var originalCamera = rig.GetPosition(); var originalTime = player.GameTime;
            float originalDelta = game.TimeController.DeltaTime, originalMiles = map.MilesTravelled;
            bool originalStop = rules.StopWhenRevealingNewEdges;
            var originalHistory = map.HistoryTravels.ToArray(); var originalPerception = map.PerceptionRolledLocations.ToArray();
            var originalRoster = TeleportationTravelers.Read(player);
            var pointRecords = map.Locations.ToArray(); var edgeRecords = map.Edges.ToArray();
            var snapshots = pointRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value))
                .Concat(edgeRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value))).ToArray();
            var ledgerSnapshot = originalLedger == null ? null : new TeleportNativeFieldSnapshot(originalLedger);
            var owners = new List<TeleportResourceFixtureOwner>();
            var movement = new TeleportInteractionMovementObserver();
            EventBus.Subscribe(movement); Application.logMessageReceived += ObserveTeleportDisabledException;
            try
            {
                var chain = FindTeleportInteractionChain(rules);
                var origin = chain[0]; var middle = chain[1]; var target = chain[2];
                var reveal = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
                foreach (var point in chain) { reveal.Invoke(point.Data, new object[] { true }); point.Data.EdgesOpened = true; }
                foreach (var edge in middle.Edges.Where(value => value.GetOppositeLocation(middle) == origin || value.GetOppositeLocation(middle) == target))
                    edge.Data.UpdateExplored(1, 1);
                rules.StopWhenRevealingNewEdges = false;
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                // Simulate existing serialized fields, without invoking an arrival
                // observer or persisting any request-local fixture data.
                var ledger = originalLedger ?? main.Descriptor.Ensure<UnitPartTeleportFamiliarity>();
                var counts = new TeleportFamiliarityState(); counts.MigrateLegacy(chain.Select(value => value.Blueprint.AssetGuid));
                typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ledger, counts.Serialize());
                ledger.MarkMagicalArrival(new TeleportExplorationBoundary(game.CurrentlyLoadedArea.AssetGuid, origin.Blueprint.AssetGuid, map.MilesTravelled));
                string boundary = ledger.ReadExplorationBoundary().Serialize();
                var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "b3a505fb61437dc4097f43c3f8f9a4cf", "native OFF interaction source");
                var caster = player.Party.FirstOrDefault(value => TeleportationSpellbookAdapter.CasterAvailable(value) && value.Descriptor.GetSpellbook(sorcerer.Spellbook) == null);
                if (caster == null) throw new InvalidOperationException("No available existing owner for the OFF source control.");
                var fixture = new TeleportResourceFixtureOwner(caster); owners.Add(fixture);
                var book = fixture.AddBook(sorcerer.Spellbook);
                book.AddKnown(5, BlueprintBootstrap.Teleportation.Teleport, true);
                book.AddKnown(7, BlueprintBootstrap.Teleportation.GreaterTeleport, true); book.Rest();
                string slots = TeleportResourceFingerprint(book);
                bool realSources = book.IsKnown(BlueprintBootstrap.Teleportation.Teleport) && book.IsKnown(BlueprintBootstrap.Teleportation.GreaterTeleport) &&
                    book.GetSpontaneousSlots(5) > 0 && book.GetSpontaneousSlots(7) > 0 && ReferenceEquals(caster.Descriptor.GetSpellbook(book.Blueprint), book);
                TeleportInteractionAssert("real-source-control", "real known spells and native slots exist while the disabled enumerator returns no actions", "realSources=" + realSources,
                    realSources && TeleportationSpellbookAdapter.Enumerate(player).Count == 0);
                CaptureTeleportInteraction("disabled-entry", new { casterId = caster.UniqueId, bookId = book.Blueprint.AssetGuid,
                    knownTeleport = book.IsKnown(BlueprintBootstrap.Teleportation.Teleport), teleportSlots = book.GetSpontaneousSlots(5),
                    knownGreater = book.IsKnown(BlueprintBootstrap.Teleportation.GreaterTeleport), greaterSlots = book.GetSpontaneousSlots(7),
                    counts = counts.Serialize(), boundary, originalPartPresent = originalLedger != null });
                string nativeActions = null;
                for (int repeat = 0; repeat < 3; repeat++)
                {
                    SelectTeleportationCastingPoint(panel, target);
                    foreach (int frame in WaitTeleportDisabledPanel(panel)) yield return frame;
                    string currentActions = TeleportationNativeButtons(panel);
                    if (nativeActions == null) nativeActions = currentActions;
                    TeleportInteractionAssert("native-selection-" + repeat, "native controls remain stable with no magical rows, modal, route or resource change",
                        "rows=" + panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length,
                        panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 0 && nativeActions == currentActions &&
                        !DialogMessageBox.Instance.IsShown && map.TravelData == null && map.PartyLocation == origin.Blueprint && movement.Starts == 0 &&
                        slots == TeleportResourceFingerprint(book) && ledger.Read().Serialize() == counts.Serialize() && ledger.ReadExplorationBoundary().Serialize() == boundary);
                    panel.Hide();
                    for (int frame = 0; frame < 5; frame++) yield return 0;
                }
                SelectTeleportationCastingPoint(panel, target);
                foreach (int frame in WaitTeleportDisabledPanel(panel)) yield return frame;
                var accept = ((TextMeshProUGUI)typeof(GlobalMapMessageBox).GetField("m_AcceptText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel)).GetComponentInParent<Button>();
                accept.onClick.Invoke();
                var travel = map.TravelData;
                CaptureTeleportInteraction("native-travel", new { movement.Starts, route = DescribeFamiliarityRoute(travel), nativeActions });
                if (travel == null || !travel.Walking || travel.Path.Count != 2) throw new InvalidOperationException("Native OFF Travel did not start its revealed two-edge route.");
                var crossed = travel.Path.Select(value => value.Direction > 0 ? rules.GetEdgeObject(value.Blueprint).Location2 : rules.GetEdgeObject(value.Blueprint).Location1).ToArray();
                if (crossed[0] != middle || crossed[1] != target) throw new InvalidOperationException("The native OFF route differs from its safe chain.");
                TeleportInteractionAssert("native-travel-once", "original native Travel starts once and spends no spell", "starts=" + movement.Starts,
                    movement.Starts == 1 && slots == TeleportResourceFingerprint(book) && !panel.gameObject.activeInHierarchy && !TeleportContextConfirmationPresenter.Pending);
                float speed = game.BlueprintRoot.GlobalMap.VisualSpeedBase * (float)typeof(MapMovementController)
                    .GetMethod("CalcSpeedModifiers", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                if (speed <= 0 || float.IsNaN(speed) || float.IsInfinity(speed)) throw new InvalidOperationException("Unusable native OFF travel speed.");
                float length = travel.Path.Sum(value => rules.GetEdgeObject(value.Blueprint).Spline.WorldLength);
                game.TimeController.SetDeltaTime((length + 0.01f) / speed);
                new MapMovementController().Tick();
                TeleportInteractionAssert("ordinary-arrival-does-not-write-ledger", "native arrival completes while both existing serialized fields remain untouched OFF",
                    "point=" + map.PartyLocation.AssetGuid + ";ledger=" + ledger.Read().Serialize(),
                    map.PartyLocation == target.Blueprint && map.TravelData == null && movement.Starts == 1 &&
                    ledger.Read().Serialize() == counts.Serialize() && ledger.ReadExplorationBoundary().Serialize() == boundary && slots == TeleportResourceFingerprint(book));
                CaptureTeleportInteraction("disabled-arrival", new { pointId = map.PartyLocation.AssetGuid, movement.Starts, movement.Stops,
                    ledger = ledger.Read().Serialize(), boundary = ledger.ReadExplorationBoundary().Serialize(), exactResources = slots == TeleportResourceFingerprint(book) });
            }
            finally
            {
                panel.Hide(); map.TravelData = null;
                foreach (var owner in owners.AsEnumerable().Reverse()) owner.Restore();
                foreach (var snapshot in snapshots) snapshot.Restore();
                foreach (var key in map.Locations.Keys.Where(value => !pointRecords.Any(pair => pair.Key == value)).ToArray()) map.Locations.Remove(key);
                foreach (var key in map.Edges.Keys.Where(value => !edgeRecords.Any(pair => pair.Key == value)).ToArray()) map.Edges.Remove(key);
                if (originalLedger == null) main.Descriptor.Remove<UnitPartTeleportFamiliarity>(); else ledgerSnapshot.Restore();
                map.HistoryTravels.Clear(); map.HistoryTravels.AddRange(originalHistory);
                map.PerceptionRolledLocations.Clear(); map.PerceptionRolledLocations.UnionWith(originalPerception);
                rules.SetCurrentPosition(originalPosition); rules.UpdatePawnPosition(); map.LastLocation = originalLast;
                map.MilesTravelled = originalMiles; rules.StopWhenRevealingNewEdges = originalStop;
                game.TimeController.SetDeltaTime(originalDelta); player.GameTime = originalTime; rig.ScrollTo(originalCamera);
                EventBus.Unsubscribe(movement); Application.logMessageReceived -= ObserveTeleportDisabledException;
                bool restored = snapshots.All(value => value.Matches()) && owners.All(value => value.IsRestored()) &&
                    ReferenceEquals(main.Descriptor.Get<UnitPartTeleportFamiliarity>(), originalLedger) && (ledgerSnapshot == null || ledgerSnapshot.Matches()) &&
                    map.HistoryTravels.SequenceEqual(originalHistory) && map.PerceptionRolledLocations.SequenceEqual(originalPerception) &&
                    originalRoster.Matches(TeleportationTravelers.Read(player)) && map.TravelData == null && map.CurrentEncounterData == null &&
                    player.GameTime == originalTime && !_workingSaveSmoke.WriteObserved && !TeleportContextConfirmationPresenter.Pending;
                CaptureTeleportInteraction("cleanup", new { restored, movement.Starts, movement.Stops, originalPartPresent = originalLedger != null });
                TeleportInteractionAssert("exceptions", "zero fixture exceptions", "count=" + _teleportationDisabledExceptions.Count, _teleportationDisabledExceptions.Count == 0);
                TeleportInteractionAssert("cleanup", "exact original map, sources, part presence/data, roster, time and UI restored", "restored=" + restored, restored);
            }
        }
        private static IEnumerable<int> WaitTeleportDisabledPanel(GlobalMapMessageBox panel)
        {
            // The production module is OFF: do not initialize or reuse its patch
            // reflection cache just to run the observation.
            var dialog = (CanvasGroup)typeof(GlobalMapMessageBox).GetField("m_Dialog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel);
            var watch = Stopwatch.StartNew();
            while (dialog.alpha < 0.99f)
            {
                if (!panel.gameObject.activeInHierarchy || watch.Elapsed.TotalSeconds > 3) throw new InvalidOperationException("Native OFF destination panel did not finish its fade.");
                yield return 0;
            }
            for (int frame = 0; frame < 4; frame++) yield return 0;
            if (!UIUtility.IsTransformInScreen(dialog.transform)) throw new InvalidOperationException("Native OFF destination panel is outside the current viewport.");
        }
    }
}
