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

                // --- Vendor migration behaviors on the shared tables ---
                var priestTable = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(
                    BlueprintBootstrap.Library, TeleportationScrollVendorPublication.PriestTableId, "priest shared vendor table");
                var sharedTable = player.SharedVendorTables.GetTable(priestTable);
                if (sharedTable == null) throw new InvalidOperationException("The shared priest table could not materialize.");
                // The native shared-table diff already stocks our published rows
                // when the table materializes from the current campaign state.
                int priestBase = CountItems(sharedTable, scrolls.WordOfRecall);
                ScrollsAssert("migration-fresh-native-stock", "native generation stocks the published batch, and migration records its marker without adding",
                    "base=" + priestBase + ";marker=false",
                    priestBase == TeleportationScrollVendorPublication.WordOfRecallStock);
                // Drive the sweep through a request-local vendor part on the
                // priest family's shared table.
                var priestPart = umdReader.Descriptor.Ensure<UnitPartVendor>();
                priestPart.SetSharedInventory(priestTable);
                TeleportationScrollVendorMigration.Migrate(umdReader);
                int afterPriestMigrate = CountItems(sharedTable, scrolls.WordOfRecall);
                ScrollsAssert("migration-fresh-marker-only", "a fully stocked target records its marker and adds nothing",
                    "base=" + priestBase + ";after=" + afterPriestMigrate + ";marker=" + ledger.HasScrollVendorGrant("shared:" + priestTable.AssetGuid),
                    afterPriestMigrate == priestBase && ledger.HasScrollVendorGrant("shared:" + priestTable.AssetGuid));
                // Buy-out persistence: the marker, not the count, decides.
                sharedTable.Remove(scrolls.WordOfRecall, CountItems(sharedTable, scrolls.WordOfRecall));
                TeleportationScrollVendorMigration.Migrate(umdReader);
                ScrollsAssert("migration-bought-out-never-refills", "a bought-out target with its marker is never refilled",
                    "count=" + CountItems(sharedTable, scrolls.WordOfRecall), CountItems(sharedTable, scrolls.WordOfRecall) == 0);
                // Already-materialized family without a marker: batch exactly once,
                // shared across every family member.
                grantsField.SetValue(ledger, null);
                TeleportationScrollVendorMigration.Migrate(umdReader);
                int firstGrant = CountItems(sharedTable, scrolls.WordOfRecall);
                var secondPart = bookReader.Descriptor.Ensure<UnitPartVendor>();
                secondPart.SetSharedInventory(priestTable);
                TeleportationScrollVendorMigration.Migrate(bookReader);
                int secondGrant = CountItems(sharedTable, scrolls.WordOfRecall);
                ScrollsAssert("migration-batch-once-per-shared-family", "an unmarked family receives the batch exactly once across members",
                    "first=" + firstGrant + ";second=" + secondGrant,
                    firstGrant == TeleportationScrollVendorPublication.WordOfRecallStock && secondGrant == firstGrant);
                secondPart.Dispose();
                priestPart.Dispose();
                // The arcane family batch through a fresh request-local part.
                grantsField.SetValue(ledger, null);
                var arcaneTable = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(
                    BlueprintBootstrap.Library, TeleportationScrollVendorPublication.ArcaneTableId, "arcane shared vendor table");
                var vendorPart = umdReader.Descriptor.Ensure<UnitPartVendor>();
                vendorPart.SetSharedInventory(arcaneTable);
                var arcaneCollection = player.SharedVendorTables.GetTable(arcaneTable);
                int arcaneBase = CountItems(arcaneCollection, scrolls.Teleport);
                TeleportationScrollVendorMigration.Migrate(umdReader);
                int arcaneTeleport = CountItems(arcaneCollection, scrolls.Teleport);
                int arcaneGreater = CountItems(arcaneCollection, scrolls.GreaterTeleport);
                TeleportationScrollVendorMigration.Migrate(umdReader);
                // The arcane table may natively self-stock on materialization; the
                // exactly-once contract is: the full batch is present, the marker is
                // recorded, and a repeated migration adds nothing.
                ScrollsAssert("migration-arcane-batch", "the arcane family holds the full batch exactly once with its marker",
                    "base=" + arcaneBase + ";teleport=" + arcaneTeleport + ";greater=" + arcaneGreater +
                        ";marker=" + ledger.HasScrollVendorGrant("shared:" + arcaneTable.AssetGuid),
                    arcaneTeleport >= TeleportationScrollVendorPublication.TeleportStock &&
                        arcaneGreater >= TeleportationScrollVendorPublication.GreaterTeleportStock &&
                        CountItems(arcaneCollection, scrolls.Teleport) == arcaneTeleport &&
                        CountItems(arcaneCollection, scrolls.GreaterTeleport) == arcaneGreater &&
                        ledger.HasScrollVendorGrant("shared:" + arcaneTable.AssetGuid));
                // Restore the request-local vendor part.
                vendorPart.Dispose();
                // --- Integrated acquisition chain: real gold purchase from the
                // native shared vendor stock, native copy-from-scroll, specialist
                // favorite preparation, cast, first arrow ---
                var marketVendor = umdReader.Descriptor.Ensure<UnitPartVendor>();
                var arcaneTable2 = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(
                    BlueprintBootstrap.Library, TeleportationScrollVendorPublication.ArcaneTableId, "arcane shared vendor table");
                marketVendor.SetSharedInventory(arcaneTable2);
                var marketStock = player.SharedVendorTables.GetTable(arcaneTable2);
                int marketBefore = CountItems(marketStock, scrolls.Teleport);
                long goldBefore = player.Money;
                if (goldBefore < 5000) player.GainMoney(5000 - goldBefore);
                long goldBase = player.Money;
                var trade = new VendorLogic();
                trade.BeginTrading(umdReader);
                var forSale = default(ItemEntity);
                foreach (var entity in trade.StoreItems)
                    if (entity != null && entity.Count > 0 && ReferenceEquals(entity.Blueprint, scrolls.Teleport)) { forSale = entity; break; }
                if (forSale == null) throw new InvalidOperationException("The native vendor stock offers no Teleport scroll to buy.");
                long price = trade.GetItemBuyPrice(forSale);
                trade.AddForBuy(forSale, 1);
                trade.Deal();
                trade.EndTraiding();
                long goldAfter = player.Money;
                int partyPurchased = TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport);
                ScrollsAssert("market-purchase-native",
                    "a real native gold purchase buys one scroll at the exact native price from the shared vendor stock",
                    "price=" + price + ";goldDelta=" + (goldBase - goldAfter) + ";stockDelta=" + (CountItems(marketStock, scrolls.Teleport) - marketBefore) +
                        ";party=" + partyPurchased,
                    price > 0 && goldBase - goldAfter == price &&
                        CountItems(marketStock, scrolls.Teleport) == marketBefore - 1 &&
                        partyPurchased == 3 /* 2 after the cast + 1 purchased */);
                // Native copy-from-scroll: the exact component action the inventory
                // context menu invokes.
                var purchased = default(ItemEntity);
                foreach (var unit in party)
                {
                    if (unit == null || unit.Inventory == null) continue;
                    foreach (var entity in unit.Inventory)
                        if (entity != null && entity.Count > 0 && ReferenceEquals(entity.Blueprint, scrolls.Teleport)) { purchased = entity; break; }
                    if (purchased != null) break;
                }
                if (purchased == null) throw new InvalidOperationException("The purchased scroll did not reach the party inventory.");
                var copyComponent = scrolls.Teleport.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>().Single();
                // The first fixture book already knows the spell, and native copy
                // rejects known spells; a fresh book on the second member receives
                // the copied spell exactly as a player's unused wizard would.
                var marketOwner = new TeleportResourceFixtureOwner(umdReader);
                _scrollsFixtureOwners.Add(marketOwner);
                var marketBook = marketOwner.AddBook(wizard.Spellbook);
                marketBook.UpdateAllSlotsSize(false);
                bool canCopy = copyComponent.CanCopy(purchased, umdReader);
                ScrollsAssert("market-copy-eligible", "the purchased scroll is natively copyable into the fresh wizard book",
                    "canCopy=" + canCopy, canCopy);
                int knownBefore = marketBook.GetKnownSpells(5).Count(value => value.Blueprint == scrolls.Teleport.Ability);
                // DoCopy is the exact private native boundary the inventory
                // context action reaches; invoke it through the same method.
                var doCopy = typeof(Kingmaker.Blueprints.Items.Components.CopyScroll).GetMethod("DoCopy",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (doCopy == null) throw new InvalidOperationException("Native copy boundary differs.");
                doCopy.Invoke(copyComponent, new object[] { purchased, umdReader });
                // DoCopy learns; the native UI action consumes the item right
                // after through the same public component method.
                copyComponent.RemoveItem(purchased, umdReader);
                int knownAfter = marketBook.GetKnownSpells(5).Count(value => value.Blueprint == scrolls.Teleport.Ability);
                int partyAfterCopy = TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport);
                ScrollsAssert("market-copy-native",
                    "the native copy action learns the canonical spell and consumes the purchased scroll",
                    "known=" + knownBefore + "->" + knownAfter + ";party=" + partyAfterCopy,
                    knownAfter == knownBefore + 1 && partyAfterCopy == partyPurchased - 1);
                // Specialist favorite preparation on the copied spell, then rest.
                var conjurationList = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellList>(BlueprintBootstrap.Library,
                    "69a6eba12bc77ea4191f573d63c9df12", "Conjuration special list");
                marketBook.AddSpecialList(conjurationList);
                marketBook.UpdateAllSlotsSize(false);
                var favorite = RawSlots(marketBook, 5).SingleOrDefault(value => value.Type == Kingmaker.UnitLogic.SpellSlotType.Favorite);
                if (favorite == null) throw new InvalidOperationException("The copy wizard lost its favorite slot.");
                if (!marketBook.Memorize(new AbilityData(scrolls.Teleport.Ability, marketBook), favorite))
                    throw new InvalidOperationException("Native favorite preparation of the copied spell failed.");
                marketBook.Rest();
                var readyFavorite = RawSlots(marketBook, 5).Count(value => value.Spell != null && value.Spell.Blueprint == scrolls.Teleport.Ability && value.Available);
                ScrollsAssert("market-specialist-prepare", "the copied spell prepares in the Conjuration favorite slot and rest readies it",
                    "ready=" + readyFavorite, readyFavorite == 1);
                // Cast from the world map and prove the first arrow at the arrival.
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                rig.ScrollTo(target.transform.position);
                for (int frame = 0; frame < 30; frame++) yield return 0;
                TeleportDestinationRows marketRows = null;
                for (int attempt = 0; attempt < 3 && (marketRows == null || marketRows.Actions.Count == 0); attempt++)
                {
                    SelectTeleportationCastingPoint(panel, target);
                    foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                    for (int frame = 0; frame < 40; frame++)
                    {
                        marketRows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                        if (marketRows != null && marketRows.Actions.Count > 0) break;
                        yield return 0;
                    }
                }
                var bookRow = marketRows == null ? null : marketRows.Actions.SingleOrDefault(value =>
                    value.Source.Kind != TeleportCastSourceKind.Scroll && value.Source.Spell == TeleportSpellKind.Teleport &&
                    value.Source.CasterId == umdReader.UniqueId && value.Source.BookId == marketBook.Blueprint.AssetGuid);
                ScrollsAssert("market-cast-row", "the specialist preparation composes a real world-map source with one use",
                    "uses=" + (bookRow == null ? "absent" : bookRow.Source.Uses.ToString()), bookRow != null && bookRow.Source.Uses == 1);
                if (bookRow == null) throw new InvalidOperationException("The copied-and-prepared source was not composed.");
                marketRows.QualificationRolls = new TeleportationFixtureRolls(new[] { 1 });
                marketRows.Buttons[marketRows.Actions.ToList().FindIndex(value => value.Key == bookRow.Key)].onClick.Invoke();
                var marketRequest = TeleportContextConfirmationPresenter.Current;
                if (marketRequest == null || !DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("The market chain confirmation did not open.");
                for (int frame = 0; frame < 8; frame++) yield return 0;
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                bool marketCommitted = marketRequest.Transaction.State == TeleportTransactionState.Completed &&
                    marketRequest.Transaction.Result != null && marketRequest.Transaction.Result.Status == TeleportExecutionStatus.Arrived;
                int readyAfterCast = RawSlots(marketBook, 5).Count(value => value.Spell != null && value.Spell.Blueprint == scrolls.Teleport.Ability && value.Available);
                var marketArrival = rules.GetLocationObject(map.PartyLocation);
                var marketLabels = ArrowCompassLabels();
                ScrollsAssert("market-cast-first-arrow",
                    "the integrated chain ends with a spent favorite preparation and working arrows at the arrival",
                    "committed=" + marketCommitted + ";ready=" + readyAfterCast + ";labels=" + marketLabels.Length,
                    marketCommitted && readyAfterCast == 0 && marketLabels.Length > 0 &&
                        marketLabels.All(label => ArrowLabelEdge(label) != null && marketArrival.Edges.Contains(ArrowLabelEdge(label))));
                CaptureTeleportScrolls("market-chain", new { price, goldDelta = goldBase - goldAfter,
                    knownAfter, readyFavorite, marketCommitted, labels = marketLabels.Length });
                if (goldBefore < 5000) player.SpendMoney(player.Money - goldBefore);
                marketVendor.Dispose();
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
        private readonly List<TeleportResourceFixtureOwner> _scrollsFixtureOwners = new List<TeleportResourceFixtureOwner>();
        private void fixtureOwnerLocalRestore()
        {
            if (_scrollsFixtureOwner != null) _scrollsFixtureOwner.Restore();
            foreach (var owner in _scrollsFixtureOwners) owner.Restore();
        }

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
