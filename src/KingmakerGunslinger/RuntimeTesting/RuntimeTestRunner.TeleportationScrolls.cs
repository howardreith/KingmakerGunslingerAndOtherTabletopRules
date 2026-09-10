using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.Items;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _teleportationScrollsSteps;
        private System.Diagnostics.Stopwatch _teleportationScrollsMapLoad;
        private readonly List<RuntimeTestAssertion> _teleportationScrollsAssertions = new List<RuntimeTestAssertion>();
        private readonly List<object> _teleportationScrollsCaptures = new List<object>();
        private bool IsTeleportationScrollsFixture { get { return _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationScrolls; } }

        // Gate 4 behavioral proof: scroll sources from real party items, an
        // exactly-one scroll cast with rebuilt arrows, cancellation controls,
        // and the vendor migration rules (fresh natively-stocked target,
        // batch-once for already-materialized vendors, buy-out persistence, one
        // grant per shared family).
        private IEnumerable<int> RunTeleportationScrolls()
        {
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            if (!_context.FeatureModules.Active.TeleportationSpells || BlueprintBootstrap.TeleportationScrolls == null ||
                game.IsControllerGamepad || game.CurrentMode != GameModeType.GlobalMap ||
                GlobalMapRules.Instance == null || !TeleportationScrollVendorMigration.Installed)
                throw new InvalidOperationException("Native stationary desktop global map with the scroll machinery is required.");
            var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State;
            var scrolls = BlueprintBootstrap.TeleportationScrolls;
            var party = player.Party.ToArray();
            if (party.Length < 2) throw new InvalidOperationException("Two traveling party members are required.");
            var ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
            var payloadState = typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
            var grantsField = typeof(UnitPartTeleportFamiliarity).GetField("_scrollVendorGrants", BindingFlags.Instance | BindingFlags.NonPublic);
            var originalPayload = payloadState.GetValue(ledger);
            var originalGrants = grantsField.GetValue(ledger);
            var originalPosition = map.PartyPosition; var originalLast = map.LastLocation;
            var originalTime = player.GameTime; float originalMiles = map.MilesTravelled;
            var originalHistory = map.HistoryTravels.ToArray(); var originalPerception = map.PerceptionRolledLocations.ToArray();
            var pointRecords = map.Locations.ToArray(); var edgeRecords = map.Edges.ToArray();
            var snapshots = pointRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value))
                .Concat(edgeRecords.Select(value => new TeleportNativeFieldSnapshot(value.Value)))
                .Concat(new[] { new TeleportNativeFieldSnapshot(ledger) }).ToArray();
            var originalUmdbase = new Dictionary<string, int>();
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
                var rig = Resources.FindObjectsOfTypeAll<Kingmaker.View.CameraRig>().Single(value => value != null &&
                    value.gameObject.activeInHierarchy && value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded);
                rig.ScrollTo(target.transform.position);
                for (int frame = 0; frame < 60; frame++) yield return 0;

                // --- Scroll sources from real party items ---
                var bookReader = party[0]; var umdReader = party[1];
                // Native player characters share ONE party inventory (stash,
                // carried and equipped items alike), so both adds land in the
                // same collection and the stock must count it exactly once.
                party[0].Inventory.Add(scrolls.Teleport, 2);
                party[1].Inventory.Add(scrolls.Teleport, 1);
                // UMD-only reader: no book knows the spell; the trained skill alone
                // qualifies the reader.
                var umdStat = umdReader.Descriptor.Stats.GetStat(StatType.SkillUseMagicDevice);
                originalUmdbase[umdReader.UniqueId] = umdStat.BaseValue;
                umdStat.BaseValue = 1;
                int sharedStock = TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport);
                ScrollsAssert("shared-stock-counts-distinct-collections", "the shared native party inventory is counted exactly once",
                    "stock=" + sharedStock, sharedStock == 3);
                var scrollSources = TeleportationScrollAdapter.Enumerate(player)
                    .Where(value => value.Spell == TeleportSpellKind.Teleport).ToArray();
                ScrollsAssert("scroll-reader-rows", "both the item-carrying member and the trained UMD reader offer the scroll",
                    "readers=" + string.Join(",", scrollSources.Select(value => value.CasterId).ToArray()),
                    scrollSources.Length == 2 && scrollSources.All(value => value.Uses == 3 && value.Kind == TeleportCastSourceKind.Scroll));
                var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "ba34257984f4c41408ce1dc2004e342e", "scrolls fixture Wizard class");
                var fixtureOwner = new TeleportResourceFixtureOwner(bookReader);
                _scrollsFixtureOwner = fixtureOwner;
                var book = fixtureOwner.AddBook(wizard.Spellbook);
                book.AddKnown(5, scrolls.Teleport.Ability, true); book.Rest();
                // Copy control: a copied scroll association prepares through the
                // exact native copy seam used by spellbook copying.
                var copiedKnown = book.GetKnownSpells(5).Any(value => value.Blueprint == scrolls.Teleport.Ability);
                ScrollsAssert("scroll-copy-learns-canonical-spell", "the native copy association learns the canonical strategic spell",
                    "copied=" + copiedKnown, copiedKnown);
                string bookFingerprint = TeleportResourceFingerprint(book);

                // --- Composition and scroll cast through the native panel ---
                var panel = TeleportationFixturePanel();
                TeleportDestinationRows rows = null;
                for (int attempt = 0; attempt < 3 && (rows == null || rows.Actions.Count == 0); attempt++)
                {
                    SelectTeleportationCastingPoint(panel, target);
                    foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                    for (int frame = 0; frame < 40; frame++)
                    {
                        rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                        if (rows != null && rows.Actions.Count > 0) break;
                        yield return 0;
                    }
                    if (rows != null && rows.Actions.Count > 0) break;
                }
                if (rows == null) throw new InvalidOperationException("No destination rows were composed for the scroll cast.");
                var scrollRow = rows.Actions.FirstOrDefault(value => value.Source.Kind == TeleportCastSourceKind.Scroll &&
                    value.Source.Spell == TeleportSpellKind.Teleport && value.Source.CasterId == umdReader.UniqueId);
                ScrollsAssert("scroll-row-composed", "the world map composes the compact Use Teleport Scroll row for the UMD reader",
                    "uses=" + (scrollRow == null ? "absent" : scrollRow.Source.Uses.ToString()),
                    scrollRow != null && scrollRow.Source.Uses == 3);
                var bookRowsBefore = rows.Actions.Count(value => value.Source.Kind != TeleportCastSourceKind.Scroll);
                // Cancellation first: the native No control consumes nothing.
                rows.QualificationRolls = new TeleportationFixtureRolls(new[] { 1 });
                rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == scrollRow.Key)].onClick.Invoke();
                var cancelRequest = TeleportContextConfirmationPresenter.Current;
                if (cancelRequest == null || !DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("The scroll confirmation did not open.");
                for (int frame = 0; frame < 8; frame++) yield return 0;
                TeleportationFixtureDialogButton("m_ButtonNo").onClick.Invoke();
                for (int frame = 0; frame < 6; frame++) yield return 0;
                ScrollsAssert("scroll-cancellation-consumes-nothing", "native No cancels without spending a scroll or slot",
                    "stock=" + TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) +
                        ";pending=" + TeleportContextConfirmationPresenter.Pending,
                    TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) == 3 &&
                    bookFingerprint == TeleportResourceFingerprint(book) && !TeleportContextConfirmationPresenter.Pending);
                // Commit the scroll cast.
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    SelectTeleportationCastingPoint(panel, target);
                    foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                    rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                    if (rows != null && rows.Actions.Any(value => value.Key == scrollRow.Key)) break;
                    for (int frame = 0; frame < 30; frame++) yield return 0;
                }
                var committedRow = rows.Actions.Single(value => value.Key == scrollRow.Key);
                rows.QualificationRolls = new TeleportationFixtureRolls(new[] { 1 });
                rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == committedRow.Key)].onClick.Invoke();
                var request = TeleportContextConfirmationPresenter.Current;
                if (request == null || !DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("The committed scroll confirmation did not open.");
                for (int frame = 0; frame < 8; frame++) yield return 0;
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                bool committed = request.Transaction.State == TeleportTransactionState.Completed &&
                    request.Transaction.Result != null && request.Transaction.Result.Status == TeleportExecutionStatus.Arrived &&
                    request.Transaction.Result.DestinationId == target.Blueprint.AssetGuid;
                int stockAfter = TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport);
                var labels = ArrowCompassLabels();
                var arrival = rules.GetLocationObject(map.PartyLocation);
                ScrollsAssert("scroll-cast-exactly-one", "the scroll cast consumes exactly one scroll, touches no book slots and relocates",
                    "committed=" + committed + ";stock=" + stockAfter + ";book=" + (bookFingerprint == TeleportResourceFingerprint(book)) +
                        ";evidence=" + (request.Execution.Resource == null ? "none" : TeleportationDiagnosticJson.Serialize(request.Execution.Resource.Evidence()).Length.ToString(CultureInfo.InvariantCulture)),
                    committed && stockAfter == 2 && bookFingerprint == TeleportResourceFingerprint(book) &&
                        request.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne &&
                        !TeleportContextConfirmationPresenter.Pending && !DialogMessageBox.Instance.IsShown);
                ScrollsAssert("scroll-cast-rebuilds-arrows", "the Gate-1 pawn notifications rebuild the compass arrows at the scroll arrival",
                    "labels=" + labels.Length + ";allAtArrival=" + (labels.Length > 0 && labels.All(label => ArrowLabelEdge(label) != null &&
                        arrival.Edges.Contains(ArrowLabelEdge(label)))),
                    labels.Length > 0 && labels.All(label => ArrowLabelEdge(label) != null && arrival.Edges.Contains(ArrowLabelEdge(label))));
                CaptureTeleportScrolls("scroll-cast", new { committed, stockBefore = 3, stockAfter, labels = labels.Length,
                    movementBookRows = bookRowsBefore });

                // --- Vendor migration behaviors on the shared priest table ---
                var priestTable = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(
                    BlueprintBootstrap.Library, TeleportationScrollVendorPublication.PriestTableId, "priest shared vendor table");
                var sharedTable = player.SharedVendorTables.GetTable(priestTable);
                if (sharedTable == null) throw new InvalidOperationException("The shared priest table could not materialize.");
                int priestBase = CountItems(sharedTable, scrolls.WordOfRecall);
                // Fresh natively-stocked target: the batch is already present, so
                // migration only records the marker.
                sharedTable.Add(scrolls.WordOfRecall, TeleportationScrollVendorPublication.WordOfRecallStock);
                TeleportationScrollVendorMigration.Migrate(umdReader);
                ScrollsAssert("migration-fresh-native-stock", "a natively stocked target only records its grant marker",
                    "count=" + CountItems(sharedTable, scrolls.WordOfRecall),
                    CountItems(sharedTable, scrolls.WordOfRecall) == priestBase + TeleportationScrollVendorPublication.WordOfRecallStock &&
                        ledger.HasScrollVendorGrant("shared:" + priestTable.AssetGuid));
                // Buy-out: removing the stock never refills.
                sharedTable.Remove(scrolls.WordOfRecall, CountItems(sharedTable, scrolls.WordOfRecall));
                TeleportationScrollVendorMigration.Migrate(umdReader);
                ScrollsAssert("migration-bought-out-never-refills", "a bought-out target with its marker is never refilled",
                    "count=" + CountItems(sharedTable, scrolls.WordOfRecall),
                    CountItems(sharedTable, scrolls.WordOfRecall) == 0);
                // Already-materialized vendor without a marker: batch exactly once.
                grantsField.SetValue(ledger, null);
                int beforeGrant = CountItems(sharedTable, scrolls.WordOfRecall);
                TeleportationScrollVendorMigration.Migrate(umdReader);
                int afterFirst = CountItems(sharedTable, scrolls.WordOfRecall);
                TeleportationScrollVendorMigration.Migrate(bookReader);
                int afterSecond = CountItems(sharedTable, scrolls.WordOfRecall);
                ScrollsAssert("migration-batch-once-per-shared-family", "an already-materialized family receives the batch exactly once across members",
                    "before=" + beforeGrant + ";first=" + afterFirst + ";second=" + afterSecond,
                    afterFirst == beforeGrant + TeleportationScrollVendorPublication.WordOfRecallStock && afterSecond == afterFirst);
                // The arcane family batch through a request-local shared part.
                grantsField.SetValue(ledger, null);
                var arcaneTable = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(
                    BlueprintBootstrap.Library, TeleportationScrollVendorPublication.ArcaneTableId, "arcane shared vendor table");
                var vendorPart = umdReader.Descriptor.Ensure<UnitPartVendor>();
                bool partCreated = vendorPart != null;
                vendorPart.SetSharedInventory(arcaneTable);
                var arcaneCollection = player.SharedVendorTables.GetTable(arcaneTable);
                int arcaneBase = CountItems(arcaneCollection, scrolls.Teleport);
                TeleportationScrollVendorMigration.Migrate(umdReader);
                ScrollsAssert("migration-arcane-batch", "the arcane family receives 5 Teleport and 3 Greater Teleport scrolls exactly once",
                    "teleport=" + CountItems(arcaneCollection, scrolls.Teleport) + ";greater=" + CountItems(arcaneCollection, scrolls.GreaterTeleport),
                    CountItems(arcaneCollection, scrolls.Teleport) == arcaneBase + TeleportationScrollVendorPublication.TeleportStock &&
                        CountItems(arcaneCollection, scrolls.GreaterTeleport) == TeleportationScrollVendorPublication.GreaterTeleportStock);
                CaptureTeleportScrolls("vendor-migration", new {
                    priestCount = CountItems(sharedTable, scrolls.WordOfRecall),
                    arcaneTeleport = CountItems(arcaneCollection, scrolls.Teleport),
                    arcaneGreater = CountItems(arcaneCollection, scrolls.GreaterTeleport),
                    grants = ((System.Collections.IList)grantsField.GetValue(ledger)).Count });
                // Restore the request-local vendor part.
                vendorPart.Dispose();
            }
            finally
            {
                // Exact fixture restoration in reverse order.
                if (TeleportContextConfirmationPresenter.Pending && DialogMessageBox.Instance != null && DialogMessageBox.Instance.IsShown)
                { try { TeleportationFixtureDialogButton("m_ButtonNo").onClick.Invoke(); } catch { } }
                foreach (var unit in party)
                {
                    if (unit == null || unit.Inventory == null) continue;
                    int remaining = TeleportationScrollAdapter.Stock(new[] { unit }, scrolls.Teleport);
                    if (remaining > 0) unit.Inventory.Remove((BlueprintItem)scrolls.Teleport, remaining);
                }
                foreach (var entry in originalUmdbase)
                {
                    var unit = player.AllCharacters.FirstOrDefault(value => value != null && value.UniqueId == entry.Key);
                    if (unit != null) unit.Descriptor.Stats.GetStat(StatType.SkillUseMagicDevice).BaseValue = entry.Value;
                }
                grantsField.SetValue(ledger, originalGrants);
                payloadState.SetValue(ledger, originalPayload);
                fixtureOwnerLocalRestore();
                map.TravelData = null;
                rules.StopWhenRevealingNewEdges = true;
                map.PartyPosition = originalPosition; map.LastLocation = originalLast;
                map.HistoryTravels.Clear(); foreach (var entry in originalHistory) map.HistoryTravels.Add(entry);
                map.PerceptionRolledLocations.Clear(); foreach (var entry in originalPerception) map.PerceptionRolledLocations.Add(entry);
                map.MilesTravelled = originalMiles; player.GameTime = originalTime;
                foreach (var snapshot in snapshots) snapshot.Restore();
            }
        }

        private TeleportResourceFixtureOwner _scrollsFixtureOwner;
        private void fixtureOwnerLocalRestore() { if (_scrollsFixtureOwner != null) _scrollsFixtureOwner.Restore(); }

        private static int CountItems(ItemsCollection collection, BlueprintItem item)
        {
            if (collection == null) return 0;
            int total = 0;
            foreach (var entity in collection)
                if (entity != null && ReferenceEquals(entity.Blueprint, item)) total += entity.Count;
            return total;
        }

        private void ScrollsAssert(string id, string expected, string actual, bool pass)
        { _teleportationScrollsAssertions.Add(Assertion("teleportation-scrolls-" + id, expected, actual, pass,
            Path.Combine(_request.EvidenceDirectory, "teleportation-scrolls.json"))); }
        private void CaptureTeleportScrolls(string step, object state)
        { _teleportationScrollsCaptures.Add(new { step, frame = Time.frameCount, state }); }

        private void PollTeleportationScrolls()
        {
            if (!IsTeleportationScrollsFixture || !_request.ExitAfterCompletion ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Scrolls qualification requires its guarded working save, automatic exit and intact write sentinels.");
            // The working save loads a local area first; load the global map the
            // same way the interaction family does before any scroll work.
            if (_teleportationScrollsMapLoad == null)
            {
                _teleportationScrollsMapLoad = System.Diagnostics.Stopwatch.StartNew();
                Game.Instance.LoadArea(Game.Instance.BlueprintRoot.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None);
                return;
            }
            if (_teleportationScrollsMapLoad.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Scrolls qualification timed out during the world-map load.");
            if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive ||
                GlobalMapRules.Instance == null || Game.Instance.CurrentMode != GameModeType.GlobalMap) return;
            if (_teleportationScrollsSteps == null) _teleportationScrollsSteps = RunTeleportationScrolls().GetEnumerator();
            Exception failure = null;
            try { if (_teleportationScrollsSteps.MoveNext()) return; }
            catch (Exception exception) { failure = exception; }
            try { _teleportationScrollsSteps.Dispose(); }
            catch (Exception exception) { failure = failure == null ? exception : new AggregateException(failure, exception); }
            _teleportationScrollsSteps = null;
            WriteTeleportationForensicJson(Path.Combine(_request.EvidenceDirectory, "teleportation-scrolls.json"), new {
                schemaVersion = 1, runId = _request.RunId,
                claims = "Real party scroll items compose shared-stock rows for book and UMD-only readers; one scroll cast consumes exactly one item with no slot and rebuilds arrows; native cancellation consumes nothing; vendor migration proves fresh-stock marker-only, batch-once per shared family, and buy-out persistence. Request-local items/stats/parts/world state; no save writes.",
                captures = _teleportationScrollsCaptures, assertions = _teleportationScrollsAssertions,
                saveWriteObserved = _workingSaveSmoke.WriteObserved, error = failure == null ? null : failure.ToString() });
            Complete(CreateResult(failure != null ? RuntimeTestStatuses.Error : _teleportationScrollsAssertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, _teleportationScrollsAssertions, failure == null ? null : failure.ToString()));
        }
    }
}
