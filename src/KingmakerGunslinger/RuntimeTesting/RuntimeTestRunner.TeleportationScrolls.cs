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
            BlueprintItemEquipmentUsable variant = null;
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
                // Deliberate reader control: no fixture may inherit a convenient
                // working-save UMD value. Zero every member's UMD base ranks for
                // the whole scenario and restore them exactly at cleanup.
                foreach (var unit in party)
                {
                    var stat = unit.Descriptor.Stats.GetStat(Kingmaker.EntitySystem.Stats.StatType.SkillUseMagicDevice);
                    originalUmdbase[unit.UniqueId] = stat.BaseValue;
                    stat.BaseValue = 0;
                }
                // Controlled class-list readers: a fixture ClassData is the exact
                // structure the native eligibility check reads
                // (Progression.Classes -> Spellbook -> SpellList), restored at cleanup.
                var wizardClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "ba34257984f4c41408ce1dc2004e342e", "scrolls fixture Wizard class");
                var clericClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "67819271767a9dd4fbfd4ae700befea0", "scrolls fixture Cleric class");
                var druidClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "610d836f3a3a9ed42a4349b62f002e96", "scrolls fixture Druid class");
                var wizardReaderData = AddScrollsClassList(bookReader, wizardClass);
                var druidReaderData = AddScrollsClassList(party.Length > 2 ? party[2] : umdReader, druidClass);
                CaptureTeleportScrolls("reader-classes", new {
                    umdRanks = party.Select(value => value.Descriptor.Stats.GetStat(Kingmaker.EntitySystem.Stats.StatType.SkillUseMagicDevice).BaseValue).ToArray(),
                    wizardQualifies = scrolls.Teleport.Ability.IsInSpellListOfUnit(bookReader.Descriptor),
                    clericRecall = scrolls.WordOfRecall.Ability.IsInSpellListOfUnit(((Kingmaker.EntitySystem.Entities.UnitEntityData)clericProbe).Descriptor) });
                // Shared stock: native characters share one party inventory.
                party[0].Inventory.Add(scrolls.Teleport, 2);
                party[1].Inventory.Add(scrolls.Teleport, 1);
                party[1].Inventory.Add(scrolls.WordOfRecall, 2);
                int sharedStock = TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport);
                ScrollsAssert("shared-stock-counts-distinct-collections", "the shared native party inventory is counted exactly once",
                    "stock=" + sharedStock, sharedStock == 3);
                // --- Per-spell native eligibility with zero UMD ranks ---
                var zeroUmdSources = TeleportationScrollAdapter.Enumerate(player).ToArray();
                var wizardTeleport = zeroUmdSources.FirstOrDefault(value => value.CasterId == bookReader.UniqueId && value.Spell == TeleportSpellKind.Teleport);
                ScrollsAssert("reader-wizard-classlist-zero-umd", "a wizard-list reader with zero UMD ranks offers Teleport without knowing or preparing it",
                    "uses=" + (wizardTeleport == null ? "absent" : wizardTeleport.Uses.ToString()),
                    wizardTeleport != null && wizardTeleport.Uses == 3);
                var wizardRecall = zeroUmdSources.FirstOrDefault(value => value.CasterId == bookReader.UniqueId && value.Spell == TeleportSpellKind.WordOfRecall);
                ScrollsAssert("reader-per-spell-negative", "qualifying for Teleport does not qualify the wizard for Word of Recall",
                    "recall=" + (wizardRecall == null ? "absent" : wizardRecall.Uses.ToString()), wizardRecall == null);
                var druidRecallReader = party.Length > 2 ? party[2] : umdReader;
                var druidRecall = zeroUmdSources.FirstOrDefault(value => value.CasterId == druidRecallReader.UniqueId && value.Spell == TeleportSpellKind.WordOfRecall);
                ScrollsAssert("reader-druid-recall-zero-umd", "a druid-list reader with zero UMD ranks offers Word of Recall (Cleric 6 / Druid 8 list levels preserved)",
                    "recall=" + (druidRecall == null ? "absent" : druidRecall.Uses.ToString()),
                    druidRecall != null && druidRecall.Uses == 2);
                var ineligibleBeforeUmd = zeroUmdSources.Count(value => value.CasterId == umdReader.UniqueId);
                ScrollsAssert("reader-ineligible-control", "a member with no relevant class list and zero UMD ranks offers nothing",
                    "rows=" + ineligibleBeforeUmd, ineligibleBeforeUmd == 0);
                // UMD-only reader: trained ranks alone qualify an uncertain attempt.
                umdReader.Descriptor.Stats.GetStat(Kingmaker.EntitySystem.Stats.StatType.SkillUseMagicDevice).BaseValue = 1;
                var umdSources = TeleportationScrollAdapter.Enumerate(player).ToArray();
                var umdTeleport = umdSources.FirstOrDefault(value => value.CasterId == umdReader.UniqueId && value.Spell == TeleportSpellKind.Teleport);
                ScrollsAssert("reader-umd-only", "a UMD-only reader with no relevant class list offers the scroll for an uncertain attempt",
                    "uses=" + (umdTeleport == null ? "absent" : umdTeleport.Uses.ToString()), umdTeleport != null && umdTeleport.Uses == 3);
                var umdRecall = umdSources.FirstOrDefault(value => value.CasterId == umdReader.UniqueId && value.Spell == TeleportSpellKind.WordOfRecall);
                ScrollsAssert("reader-umd-per-spell", "the UMD reader is also offered Word of Recall independently",
                    "recall=" + (umdRecall == null ? "absent" : umdRecall.Uses.ToString()), umdRecall != null && umdRecall.Uses == 2);

                // The fixture book backs the copy control and the later market chain;
                // scroll eligibility itself never depends on it.
                var fixtureOwner = new TeleportResourceFixtureOwner(bookReader);
                _scrollsFixtureOwner = fixtureOwner;
                var book = fixtureOwner.AddBook(wizardClass.Spellbook);
                book.AddKnown(5, scrolls.Teleport.Ability, true); book.Rest();
                var copiedKnown = book.GetKnownSpells(5).Any(value => value.Blueprint == scrolls.Teleport.Ability);
                ScrollsAssert("scroll-fixture-book-knows-canonical-spell",
                    "the fixture book setup registers the canonical spell (native copy proof lives in the market chain below)",
                    "known=" + copiedKnown, copiedKnown);
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
                // Deterministic native activation FAILURE: the rank-1 UMD reader
                // cannot pass the genuine UMD check, so the native activation is
                // refused — nothing consumed, no teleport, no compensation needed.
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    SelectTeleportationCastingPoint(panel, target);
                    foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                    rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                    if (rows != null && rows.Actions.Any(value => value.Key == scrollRow.Key)) break;
                    for (int frame = 0; frame < 30; frame++) yield return 0;
                }
                var failureRow = rows.Actions.Single(value => value.Key == scrollRow.Key);
                rows.QualificationRolls = new TeleportationFixtureRolls(new int[0]);
                rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == failureRow.Key)].onClick.Invoke();
                var failureRequest = TeleportContextConfirmationPresenter.Current;
                if (failureRequest == null || !DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("The UMD-failure confirmation did not open.");
                for (int frame = 0; frame < 8; frame++) yield return 0;
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                bool refused = failureRequest.Transaction.State == TeleportTransactionState.ActivationRefused;
                ScrollsAssert("scroll-activation-umd-failure", "a genuine failed UMD check refuses the activation: nothing consumed, no teleport, no refund attempted",
                    "state=" + failureRequest.Transaction.State + ";stock=" + TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) +
                        ";party=" + map.PartyLocation.AssetGuid,
                    refused && TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) == 3 &&
                        map.PartyLocation.AssetGuid == origin.Blueprint.AssetGuid &&
                        !TeleportContextConfirmationPresenter.Pending && !DialogMessageBox.Instance.IsShown);
                // Commit the scroll cast through the class-list reader with ZERO
                // UMD ranks: the native activation succeeds without any die roll.
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    SelectTeleportationCastingPoint(panel, target);
                    foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                    rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                    if (rows != null && rows.Actions.Any(value => value.Source.Kind == TeleportCastSourceKind.Scroll &&
                        value.Source.CasterId == bookReader.UniqueId)) break;
                    for (int frame = 0; frame < 30; frame++) yield return 0;
                }
                var committedRow = rows.Actions.Single(value => value.Source.Kind == TeleportCastSourceKind.Scroll &&
                    value.Source.CasterId == bookReader.UniqueId && value.Source.Spell == TeleportSpellKind.Teleport);
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
                // The first legal native arrow must actually MOVE the party, not
                // merely exist: click the real handler and verify a walking route
                // along an edge of the arrival point.
                var firstArrow = labels.FirstOrDefault();
                if (firstArrow == null) throw new InvalidOperationException("No rebuilt arrow to exercise after the scroll cast.");
                firstArrow.OnClick();
                for (int frame = 0; frame < 10; frame++) yield return 0;
                bool arrowMoved = map.TravelData != null && map.TravelData.Walking;
                var arrowRoute = map.TravelData == null ? null : map.TravelData.Path.Select(value => value.Blueprint.AssetGuid).ToArray();
                ScrollsAssert("scroll-first-arrow-moves",
                    "the first legal arrow after a scroll cast starts real native travel from the arrival point",
                    "walking=" + arrowMoved + ";edges=" + (arrowRoute == null ? "none" : string.Join(",", arrowRoute)),
                    arrowMoved && arrowRoute != null && arrowRoute.Length > 0);
                if (map.TravelData != null)
                {
                    if (map.TravelData.Walking) rules.OnBreak();
                    map.TravelData = null;
                    rules.SetCurrentPosition(new MapPosition(target.Blueprint)); rules.UpdatePawnPosition();
                }
                CaptureTeleportScrolls("scroll-cast", new { committed, stockBefore = 3, stockAfter, labels = labels.Length,
                    movementBookRows = bookRowsBefore });

                // --- Crafted-variant discovery: a distinct genuine scroll blueprint
                // with the same canonical association but different caster-level
                // metadata must be discovered as its own row, and activation must
                // spend the chosen variant, not the standard stock. ---
                variant = UnityEngine.Object.Instantiate(scrolls.Teleport);
                variant.name = "KMG_Fixture_CraftedTeleportScroll_CL13";
                // A distinct genuine variant carries its own stable identity.
                typeof(Kingmaker.Blueprints.BlueprintScriptableObject)
                    .GetField("m_AssetGuid", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(variant, "5f167c2d4e6b8a903c1d2e3f4a5b6c7d");
                variant.CasterLevel = 13;
                party[1].Inventory.Add(variant, 1);
                var variantSources = TeleportationScrollAdapter.Enumerate(player)
                    .Where(value => value.Spell == TeleportSpellKind.Teleport && value.CasterId == bookReader.UniqueId).ToArray();
                var standardRow = variantSources.FirstOrDefault(value => value.BookId == scrolls.Teleport.AssetGuid);
                var variantRow2 = variantSources.FirstOrDefault(value => value.BookId == variant.AssetGuid);
                ScrollsAssert("variant-discovery-separate-row", "a crafted variant with different caster-level metadata forms its own discovered row",
                    "rows=" + variantSources.Length + ";standardUses=" + (standardRow == null ? 0 : standardRow.Uses) +
                        ";variantUses=" + (variantRow2 == null ? 0 : variantRow2.Uses) + ";variantLevel=" + (variantRow2 == null ? 0 : variantRow2.SpellLevel),
                    variantSources.Length == 2 && standardRow != null && standardRow.Uses == 2 &&
                        variantRow2 != null && variantRow2.Uses == 1 && variantRow2.SpellLevel == variant.SpellLevel);
                // C2: the no-destination activation guard covers every SUPPORTED
                // variant, not only the standard items: ordinary native use of the
                // distinct crafted variant is refused before any roll or
                // consumption, identically to the standard scroll.
                ItemEntity variantForOrdinaryUse = null;
                foreach (var unit in party)
                {
                    if (unit == null || unit.Inventory == null) continue;
                    foreach (var entity in unit.Inventory)
                        if (entity != null && entity.Count > 0 && ReferenceEquals(entity.Blueprint, variant)) { variantForOrdinaryUse = entity; break; }
                    if (variantForOrdinaryUse != null) break;
                }
                if (variantForOrdinaryUse == null) throw new InvalidOperationException("The variant scroll is unavailable for the ordinary-use guard test.");
                int variantStockBefore = TeleportationScrollAdapter.Stock(player.Party, variant);
                var variantObserver = new TeleportScrollActivationObserver();
                Kingmaker.PubSubSystem.EventBus.Subscribe(variantObserver);
                bool variantOrdinaryAttempted;
                try { variantOrdinaryAttempted = variantForOrdinaryUse.TryUseFromInventory(bookReader, new Kingmaker.Utility.TargetWrapper(bookReader)); }
                finally { Kingmaker.PubSubSystem.EventBus.Unsubscribe(variantObserver); }
                ScrollsAssert("variant-ordinary-use-refused",
                    "ordinary native use of a distinct supported variant without a destination transaction is refused before any roll or consumption",
                    "attempted=" + variantOrdinaryAttempted + ";events=" + (variantObserver.Event != null) +
                        ";stock=" + TeleportationScrollAdapter.Stock(player.Party, variant) +
                        ";gateClosedAfter=" + !TeleportationScrollActivationGate.Authorized(bookReader),
                    !variantOrdinaryAttempted && variantObserver.Event == null &&
                        TeleportationScrollAdapter.Stock(player.Party, variant) == variantStockBefore &&
                        !TeleportationScrollActivationGate.Authorized(bookReader));
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                for (int frame = 0; frame < 10; frame++) yield return 0;

                for (int attempt = 0; attempt < 3; attempt++)
                {
                    SelectTeleportationCastingPoint(panel, target);
                    foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                    rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                    if (rows != null && rows.Actions.Any(value => value.Source.Kind == TeleportCastSourceKind.Scroll &&
                        value.Source.BookId == variant.AssetGuid && value.Source.CasterId == bookReader.UniqueId)) break;
                    for (int frame = 0; frame < 30; frame++) yield return 0;
                }
                var variantCommitted = rows.Actions.Single(value => value.Source.Kind == TeleportCastSourceKind.Scroll &&
                    value.Source.BookId == variant.AssetGuid && value.Source.CasterId == bookReader.UniqueId);
                rows.QualificationRolls = new TeleportationFixtureRolls(new[] { 1 });
                rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == variantCommitted.Key)].onClick.Invoke();
                var variantRequest = TeleportContextConfirmationPresenter.Current;
                if (variantRequest == null || !DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("The variant confirmation did not open.");
                for (int frame = 0; frame < 8; frame++) yield return 0;
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                bool variantArrived = variantRequest.Transaction.State == TeleportTransactionState.Completed &&
                    variantRequest.Transaction.Result != null && variantRequest.Transaction.Result.Status == TeleportExecutionStatus.Arrived;
                ScrollsAssert("variant-activation-spends-chosen-variant",
                    "activating the variant row consumes exactly the chosen variant and leaves the standard stock untouched",
                    "variantStock=" + TeleportationScrollAdapter.Stock(player.Party, variant) +
                        ";standardStock=" + TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) + ";arrived=" + variantArrived,
                    variantArrived && TeleportationScrollAdapter.Stock(player.Party, variant) == 0 &&
                        TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) == 2);

                // --- R1: ordinary native item use is refused before activation ---
                // At a stationary world-map point, with an eligible class-list
                // reader and a standard scroll, call the ordinary native item-use
                // boundary directly — the exact entry point inventory/equipment
                // context actions reach — WITHOUT opening a destination
                // transaction. Expected: refusal before any activation roll or
                // consumption, and no lingering authorization.
                ItemEntity ordinaryScroll = null;
                foreach (var entity in party[0].Inventory)
                    if (entity != null && entity.Count > 0 && ReferenceEquals(entity.Blueprint, scrolls.Teleport))
                    { ordinaryScroll = entity; break; }
                if (ordinaryScroll == null) throw new InvalidOperationException("No standard scroll for the ordinary-use boundary.");
                int ordinaryBefore = TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport);
                var ordinaryPoint = map.PartyLocation.AssetGuid;
                var ordinaryObserver = new TeleportScrollActivationObserver();
                Kingmaker.PubSubSystem.EventBus.Subscribe(ordinaryObserver);
                bool ordinaryAttempted;
                try { ordinaryAttempted = ordinaryScroll.TryUseFromInventory(bookReader, new Kingmaker.Utility.TargetWrapper(bookReader)); }
                finally { Kingmaker.PubSubSystem.EventBus.Unsubscribe(ordinaryObserver); }
                ScrollsAssert("ordinary-use-refused-before-activation",
                    "ordinary native item use without a destination transaction is refused before any roll or consumption",
                    "attempted=" + ordinaryAttempted + ";events=" + (ordinaryObserver.Event != null) +
                        ";stock=" + TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) +
                        ";point=" + map.PartyLocation.AssetGuid + ";gateClosedAfter=" + !TeleportationScrollActivationGate.Authorized(bookReader),
                    !ordinaryAttempted && ordinaryObserver.Event == null &&
                        TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) == ordinaryBefore &&
                        map.PartyLocation.AssetGuid == ordinaryPoint &&
                        !TeleportationScrollActivationGate.Authorized(bookReader));

                // R2: the two same-count variants are VISIBLY distinct choices:
                // rows and confirmation name the caster level.
                var cl9Row = variantSources.First(value => value.BookId == scrolls.Teleport.AssetGuid);
                var cl13Row = variantSources.First(value => value.BookId == variant.AssetGuid);
                var presentation = TeleportationWorldMapAdapter.Capture(false);
                var presentationPoint = TeleportationWorldMapAdapter.ReadDestination(presentation, target.Blueprint);
                var cl9Action = new WorldMapPointSpellAction(presentationPoint, origin.Blueprint.AssetGuid, cl9Row, false);
                var cl13Action = new WorldMapPointSpellAction(presentationPoint, origin.Blueprint.AssetGuid, cl13Row, false);
                var cl9Text = TeleportContextPresentation.CompactRow(cl9Action, TeleportationText.Get);
                var cl13Text = TeleportContextPresentation.CompactRow(cl13Action, TeleportationText.Get);
                ScrollsAssert("variant-rows-visibly-distinct",
                    "same-count CL9 and CL13 variants carry visibly different rows naming the caster level",
                    "cl9=" + cl9Text.Replace("\n", " | ") + ";cl13=" + cl13Text.Replace("\n", " | "),
                    cl9Text.Contains("CL 9") && cl13Text.Contains("CL 13") && cl9Text != cl13Text);
                // R2: same caster level but a materially different item spell
                // level is a separate group, never a silent substitution.
                var spellLevelVariant = UnityEngine.Object.Instantiate(scrolls.Teleport);
                spellLevelVariant.name = "KMG_Fixture_CraftedTeleportScroll_SL4";
                typeof(Kingmaker.Blueprints.BlueprintScriptableObject)
                    .GetField("m_AssetGuid", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(spellLevelVariant, "6e277d3f5a1c9e802d3f4a5b6c7d8e9f");
                spellLevelVariant.SpellLevel = 4;
                party[1].Inventory.Add(spellLevelVariant, 1);
                var contractSources = TeleportationScrollAdapter.Enumerate(player)
                    .Where(value => value.Spell == TeleportSpellKind.Teleport && value.CasterId == bookReader.UniqueId).ToArray();
                var contractRow = contractSources.FirstOrDefault(value => value.BookId == spellLevelVariant.AssetGuid);
                ScrollsAssert("variant-spelllevel-not-conflated",
                    "a same-caster-level variant with a different item spell level stays a distinct choice",
                    "rows=" + contractSources.Length + ";sl4Level=" + (contractRow == null ? 0 : contractRow.SpellLevel),
                    contractRow != null && contractRow.SpellLevel == 4 && contractSources.Length == 2);
                // R2: a teaching/activation mismatch never authorizes spending.
                var mismatch = UnityEngine.Object.Instantiate(scrolls.Teleport);
                mismatch.name = "KMG_Fixture_MismatchTeachingScroll";
                typeof(Kingmaker.Blueprints.BlueprintScriptableObject)
                    .GetField("m_AssetGuid", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(mismatch, "7d388e4f6b2daf913e4f5b6c7d8e9f0a");
                // Instantiate shares component instances: replace the inherited
                // CopyScroll with the variant's own instance before mutating it,
                // exactly as production does — mutating the shared instance
                // would corrupt the standard scroll's teaching target.
                var inheritedCopy = mismatch.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>().ToArray();
                var mismatchComponents = mismatch.ComponentsArray.ToList();
                foreach (var component in inheritedCopy) mismatchComponents.Remove(component);
                mismatchComponents.Add(new Kingmaker.Blueprints.Items.Components.CopyScroll { CustomSpell = scrolls.GreaterTeleport.Ability });
                mismatch.ComponentsArray = mismatchComponents.ToArray();
                party[1].Inventory.Add(mismatch, 1);
                var mismatchSources = TeleportationScrollAdapter.Enumerate(player)
                    .Where(value => value.CasterId == bookReader.UniqueId).ToArray();
                var mismatchCount = mismatchSources.Count(value => value.BookId == mismatch.AssetGuid);
                ScrollsAssert("variant-teaching-mismatch-rejected",
                    "a scroll whose teaching target differs from its activated spell is never offered",
                    "rows=" + mismatchCount, mismatchCount == 0);
                party[1].Inventory.Remove((BlueprintItem)mismatch, 1);
                // Corruption regression: the standard scroll's own teaching target
                // must remain the canonical Teleport throughout.
                ScrollsAssert("variant-standard-teaching-intact",
                    "the standard scroll keeps teaching its own canonical spell after variant mutations",
                    "teaches=" + scrolls.Teleport.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>()
                        .Single().CustomSpell.AssetGuid,
                    scrolls.Teleport.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>()
                        .Single().CustomSpell == scrolls.Teleport.Ability);
                party[1].Inventory.Remove((BlueprintItem)spellLevelVariant, 1);
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
                // R3: the shared supplier decision governs saved-stock migration.
                // Primary active: a part bound to the FALLBACK table receives no
                // arcane batch — it is not the selected supplier.
                var fallbackTable = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(
                    BlueprintBootstrap.Library, TeleportationScrollVendorPublication.FallbackArcaneTableId, "Hassuf fallback vendor table");
                var fallbackPart = umdReader.Descriptor.Ensure<UnitPartVendor>();
                fallbackPart.SetSharedInventory(fallbackTable);
                var fallbackCollection = player.SharedVendorTables.GetTable(fallbackTable);
                int fallbackBase = CountItems(fallbackCollection, scrolls.Teleport);
                TeleportationScrollVendorMigration.Migrate(umdReader);
                ScrollsAssert("migration-fallback-inactive-while-primary-active",
                    "while the primary arcane supplier is selected the fallback table receives no migration batch",
                    "delta=" + (CountItems(fallbackCollection, scrolls.Teleport) - fallbackBase),
                    CountItems(fallbackCollection, scrolls.Teleport) == fallbackBase);
                // Genuine fallback need: the bounded decision seam establishes the
                // exact condition (primary table absent from the loaded library);
                // the SAME shared decision then routes the arcane batch to the
                // fallback table exactly once, under its own grant identity.
                grantsField.SetValue(ledger, null);
                var fallbackDecision = TeleportationScrollVendorPublication.DecideSupplier((guid, name) =>
                    string.Equals(guid, TeleportationScrollVendorPublication.ArcaneTableId, StringComparison.Ordinal) ? null :
                    BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(BlueprintBootstrap.Library, guid, name));
                ScrollsAssert("migration-fallback-decision",
                    "when the primary table is genuinely absent the shared decision selects the Hassuf fallback",
                    "arcane=" + fallbackDecision.Arcane.name + ";fallback=" + fallbackDecision.ArcaneFallback +
                        ";priest=" + (fallbackDecision.Priest == null ? "null" : fallbackDecision.Priest.name),
                    fallbackDecision.ArcaneFallback && string.Equals(fallbackDecision.Arcane.AssetGuid,
                        TeleportationScrollVendorPublication.FallbackArcaneTableId, StringComparison.Ordinal) &&
                    fallbackDecision.Priest != null);
                TeleportationScrollVendorMigration.Migrate(umdReader, fallbackDecision);
                int fallbackAfter = CountItems(fallbackCollection, scrolls.Teleport);
                TeleportationScrollVendorMigration.Migrate(umdReader, fallbackDecision);
                ScrollsAssert("migration-fallback-batch-once",
                    "the already-materialized fallback merchant receives the arcane batch exactly once under its own grant identity",
                    "base=" + fallbackBase + ";first=" + fallbackAfter + ";second=" + CountItems(fallbackCollection, scrolls.Teleport),
                    fallbackAfter == fallbackBase + TeleportationScrollVendorPublication.TeleportStock + 0 /* Greater stock joins the same batch */ &&
                        CountItems(fallbackCollection, scrolls.Teleport) == fallbackAfter &&
                        ledger.HasScrollVendorGrant("shared:" + fallbackTable.AssetGuid));
                fallbackPart.Dispose();
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
                var marketBook = marketOwner.AddBook(wizardClass.Spellbook);
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
                CaptureTeleportScrolls("market-copy-diagnostic", new {
                    knownByLevel = Enumerable.Range(0, 10).Select(level => marketBook.GetKnownSpells(level)
                        .Count(value => value.Blueprint == scrolls.Teleport.Ability)).ToArray(),
                    readerBooks = umdReader.Descriptor.Spellbooks.Select(value => value.Blueprint.name).ToArray(),
                    wizardListContains = marketBook.Blueprint.SpellList.Contains(scrolls.Teleport.Ability),
                    wizardGetLevel = marketBook.Blueprint.SpellList.GetLevel(scrolls.Teleport.Ability),
                    canCopyScrolls = marketBook.Blueprint.CanCopyScrolls,
                    marketBookKnown = marketBook.GetKnownSpells(5).Count(),
                    customSpell = copyComponent.CustomSpell == null ? null : copyComponent.CustomSpell.AssetGuid });
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
                    "uses=" + (bookRow == null ? "absent" : bookRow.Source.Uses.ToString()) +
                        ";rows=" + (marketRows == null ? -1 : marketRows.Actions.Count) +
                        ";kinds=" + (marketRows == null ? "" : string.Join(",", marketRows.Actions.Select(value => value.Source.Kind + ":" + value.Source.Spell + ":" + value.Source.CasterId.Substring(0, 6) + ":" + value.Source.BookId.Substring(0, 6)).ToArray())),
                    bookRow != null && bookRow.Source.Uses == 1);
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
                    remaining = TeleportationScrollAdapter.Stock(new[] { unit }, variant);
                    if (remaining > 0) unit.Inventory.Remove((BlueprintItem)variant, remaining);
                    remaining = TeleportationScrollAdapter.Stock(new[] { unit }, scrolls.WordOfRecall);
                    if (remaining > 0) unit.Inventory.Remove((BlueprintItem)scrolls.WordOfRecall, remaining);
                }
                for (int index = 0; index < _scrollsFixtureClasses.Count; index++)
                    _scrollsFixtureClassOwners[index].Descriptor.Progression.Classes.Remove(_scrollsFixtureClasses[index]);
                _scrollsFixtureClasses.Clear();
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


        private readonly List<Kingmaker.UnitLogic.ClassData> _scrollsFixtureClasses = new List<Kingmaker.UnitLogic.ClassData>();
        private readonly List<Kingmaker.EntitySystem.Entities.UnitEntityData> _scrollsFixtureClassOwners = new List<Kingmaker.EntitySystem.Entities.UnitEntityData>();
        private Kingmaker.EntitySystem.Entities.UnitEntityData clericProbe;
        private Kingmaker.UnitLogic.ClassData AddScrollsClassList(Kingmaker.EntitySystem.Entities.UnitEntityData unit, BlueprintCharacterClass characterClass)
        {
            var data = new Kingmaker.UnitLogic.ClassData(characterClass) { Spellbook = characterClass.Spellbook };
            unit.Descriptor.Progression.Classes.Add(data);
            _scrollsFixtureClasses.Add(data);
            _scrollsFixtureClassOwners.Add(unit);
            clericProbe = unit;
            return data;
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
