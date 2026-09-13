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
            var blockedReaders = new List<Kingmaker.EntitySystem.Entities.UnitEntityData>();
            ModifiableValue.Modifier failurePenalty = null;
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
                // --- Optional Oracle (Call of the Wild) positive control ---
                // The owner's exact defect: a genuine CotW Oracle with zero UMD
                // ranks who does NOT know Word of Recall. The fixture only
                // attaches the class structure the native predicate reads; the
                // production final-live reconciliation must supply the level-6
                // list membership. Nothing here seeds the list or the spell.
                var oracleClass = TeleportationFinalLiveReconciler.ResolveOracleClass(BlueprintBootstrap.Library);
                var oracleReader = bookReader;
                if (oracleClass == null)
                    ScrollsAssert("oracle-optional-absent-safe", "a profile without the optional Oracle class records its absence and stays safe",
                        "absent", true);
                else
                {
                    AddScrollsClassList(oracleReader, oracleClass);
                    var oracleLevels = oracleClass.Spellbook.SpellList.SpellsByLevel
                        .Where(value => value != null && value.SpellLevel == TeleportationFinalLiveReconciler.OracleWordOfRecallLevel).ToArray();
                    int oracleRefs = oracleLevels.Length == 1 ? oracleLevels[0].Spells.Count(value => ReferenceEquals(value, scrolls.WordOfRecall.Ability)) : 0;
                    int oracleGuids = oracleLevels.Length == 1 ? oracleLevels[0].Spells.Count(value => value != null &&
                        value.AssetGuid == scrolls.WordOfRecall.Ability.AssetGuid) : 0;
                    ScrollsAssert("oracle-final-list-recall-exactly-once", "the production final-live reconciliation places the canonical Word of Recall exactly once at Oracle level 6",
                        "levels=" + oracleLevels.Length + ";refs=" + oracleRefs + ";guids=" + oracleGuids,
                        oracleLevels.Length == 1 && oracleRefs == 1 && oracleGuids == 1);
                    bool oracleEligible = scrolls.WordOfRecall.Ability.IsInSpellListOfUnit(oracleReader.Descriptor);
                    ScrollsAssert("oracle-native-classlist-eligibility", "an Oracle reader with zero UMD ranks passes the native class-list predicate for Word of Recall",
                        "eligible=" + oracleEligible + ";umd=" + oracleReader.Descriptor.Stats.GetStat(StatType.SkillUseMagicDevice).BaseValue,
                        oracleEligible && oracleReader.Descriptor.Stats.GetStat(StatType.SkillUseMagicDevice).BaseValue == 0);
                    var oracleRow = TeleportationScrollAdapter.Enumerate(player).FirstOrDefault(value =>
                        value.CasterId == oracleReader.UniqueId && value.Spell == TeleportSpellKind.WordOfRecall);
                    ScrollsAssert("oracle-reader-row-offered", "the Oracle reader offers the Word of Recall scroll row through shared stock with zero UMD",
                        "uses=" + (oracleRow == null ? "absent" : oracleRow.Uses.ToString()), oracleRow != null && oracleRow.Uses == 2);
                    bool oracleKnows = oracleReader.Descriptor.Spellbooks.Any(ownedBook =>
                        Enumerable.Range(0, 10).Any(spellLevel => ownedBook.GetKnownSpells(spellLevel).Any(value =>
                            ReferenceEquals(value.Blueprint, scrolls.WordOfRecall.Ability))));
                    ScrollsAssert("oracle-reader-not-know-spell", "scroll eligibility neither requires nor grants knowing Word of Recall",
                        "knows=" + oracleKnows, !oracleKnows);
                    ScrollsAssert("oracle-scroll-teaching-intact", "the standard Word of Recall scroll keeps teaching its own canonical spell",
                        "teaches=" + scrolls.WordOfRecall.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>()
                            .Single().CustomSpell.AssetGuid,
                        scrolls.WordOfRecall.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>()
                            .Single().CustomSpell == scrolls.WordOfRecall.Ability);
                }
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
                CaptureTeleportScrolls("native-reader-forecasts", umdSources.Select(value => new {
                    value.CasterId, value.Spell, value.ScrollGroupId, value.Uses,
                    chance = value.ActivationChance == null ? null : new { value.ActivationChance.Supported,
                        value.ActivationChance.NoCheck, value.ActivationChance.Probability, value.ActivationChance.Diagnostic }
                }).ToArray());
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
                    value.Source.Spell == TeleportSpellKind.Teleport);
                ScrollsAssert("scroll-row-composed", "one compact scroll group automatically binds the guaranteed class-list reader",
                    "uses=" + (scrollRow == null ? "absent" : scrollRow.Source.Uses.ToString()),
                    scrollRow != null && scrollRow.Source.Uses == 3 && scrollRow.ReaderResolved &&
                    scrollRow.Source.CasterId == bookReader.UniqueId && scrollRow.Source.ActivationChance.Supported &&
                    scrollRow.Source.ActivationChance.NoCheck && rows.Actions.Count(value => value.Source.Kind == TeleportCastSourceKind.Scroll &&
                        value.Source.Spell == TeleportSpellKind.Teleport) == 1);
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
                foreach (int tick in QualifyAutomaticScrollReaders(panel, target, bookReader, umdReader)) yield return tick;
                // Remove the guaranteed reader from native item eligibility for
                // this controlled failure only; no selection row names a reader.
                foreach (var reader in party.Where(value => !ReferenceEquals(value, umdReader)))
                { reader.Descriptor.State.MagicItemsForbidden.Retain(); blockedReaders.Add(reader); }
                var failureStat = umdReader.Stats.GetStat(StatType.SkillUseMagicDevice);
                failurePenalty = failureStat.AddModifier(1 - failureStat.ModifiedValue, (Kingmaker.Blueprints.GameLogicComponent)null,
                    Kingmaker.Enums.ModifierDescriptor.UntypedStackable);
                // Deterministic native activation FAILURE: the rank-1 UMD reader
                // has a request-local penalty making even twenty insufficient;
                // it cannot pass the genuine UMD check, so native activation is
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
                if (!failureRow.ReaderResolved || failureRow.Source.CasterId != umdReader.UniqueId ||
                    failureRow.Source.ActivationChance == null || !failureRow.Source.ActivationChance.Supported || failureRow.Source.ActivationChance.Probability != 0m)
                    throw new InvalidOperationException("The controlled native UMD failure must bind the sole eligible zero-chance reader.");
                var failureEvent = rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == failureRow.Key)].onClick;
                failureEvent.Invoke(); failureEvent.Invoke(); // Original event twice in the SAME FRAME.
                var failureRequest = TeleportContextConfirmationPresenter.Current;
                if (failureRequest == null || !DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("The UMD-failure confirmation did not open.");
                for (int frame = 0; frame < 8; frame++) yield return 0;
                using (var notifications = new TeleportNotificationObserver())
                {
                    TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    failureEvent.Invoke(); // Later stale callback must not try another reader.
                    string expected = TeleportContextPresentation.ScrollActivationFailure(umdReader.CharacterName,
                        TeleportSpellKind.Teleport, TeleportExpenditure.None, TeleportationText.Get);
                    CaptureTeleportScrolls("named-failure-notifications", new { expected, actual = notifications.Text.ToArray() });
                    ScrollsAssert("scroll-failure-native-notification", "production warning boundary emits one actual-reader failure with verified no consumption",
                        "messages=" + string.Join("|", notifications.Text), notifications.Text.Count == 1 && notifications.Text[0] == expected &&
                        !expected.Contains("{0}") && bookFingerprint == TeleportResourceFingerprint(book));
                }
                bool refused = failureRequest.Transaction.State == TeleportTransactionState.ActivationRefused;
                ScrollsAssert("scroll-activation-umd-failure", "a genuine failed UMD check refuses the activation: nothing consumed, no teleport, no refund attempted",
                    "state=" + failureRequest.Transaction.State + ";stock=" + TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) +
                        ";party=" + map.PartyLocation.AssetGuid,
                    refused && TeleportationScrollAdapter.Stock(player.Party, scrolls.Teleport) == 3 &&
                        map.PartyLocation.AssetGuid == origin.Blueprint.AssetGuid &&
                        !TeleportContextConfirmationPresenter.Pending && !DialogMessageBox.Instance.IsShown);
                var failedNative = (TeleportationScrollCastResource)failureRequest.Execution.Resource;
                ScrollsAssert("automatic-failure-one-native-attempt", "same-frame and later original callbacks make exactly one native attempt by the automatically selected reader",
                    "reader=" + failedNative.ActualReaderId + ";events=" + failedNative.NativeEventCount,
                    failedNative.ActualReaderId == umdReader.UniqueId && failedNative.NativeEventCount == 1 &&
                    failedNative.ObserveExpenditure() == TeleportExpenditure.None);
                CaptureTeleportScrolls("automatic-native-failure", failedNative.Evidence());
                foreach (var reader in blockedReaders) reader.Descriptor.State.MagicItemsForbidden.Release();
                blockedReaders.Clear();
                umdReader.Stats.GetStat(StatType.SkillUseMagicDevice).RemoveModifier(failurePenalty); failurePenalty = null;
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
                var successEvent = rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == committedRow.Key)].onClick;
                successEvent.Invoke(); successEvent.Invoke();
                var request = TeleportContextConfirmationPresenter.Current;
                if (request == null || !DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("The committed scroll confirmation did not open.");
                for (int frame = 0; frame < 8; frame++) yield return 0;
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                successEvent.Invoke();
                var successfulNative = (TeleportationScrollCastResource)request.Execution.Resource;
                ScrollsAssert("automatic-success-native-reader", "the selected guaranteed reader performs exactly one real native activation with no UMD roll",
                    "reader=" + successfulNative.ActualReaderId + ";events=" + successfulNative.NativeEventCount,
                    successfulNative.ActualReaderId == bookReader.UniqueId && successfulNative.NativeEventCount == 1 && !successfulNative.RequiredUmd);
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

                // Variant qualifiers come from real group composition, not
                // fixture-created per-reader actions. Replenish only the owned
                // variant so both choices are live during native rendering.
                party[1].Inventory.Add(variant, 1);
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                SelectTeleportationCastingPoint(panel, target);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                var cl9Row = variantSources.First(value => value.BookId == scrolls.Teleport.AssetGuid);
                var cl13Row = variantSources.First(value => value.BookId == variant.AssetGuid);
                var cl9Action = rows.Actions.Single(value => value.Source.Kind == TeleportCastSourceKind.Scroll && value.Source.BookId == cl9Row.BookId);
                var cl13Action = rows.Actions.Single(value => value.Source.Kind == TeleportCastSourceKind.Scroll && value.Source.BookId == cl13Row.BookId);
                var cl9Text = rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == cl9Action.Key)].GetComponentInChildren<TMPro.TextMeshProUGUI>(true).text;
                var cl13Text = rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == cl13Action.Key)].GetComponentInChildren<TMPro.TextMeshProUGUI>(true).text;
                ScrollsAssert("variant-rows-visibly-distinct",
                    "live native CL9 and CL13 variant rows carry distinct compact caster-level qualifiers",
                    "cl9=" + cl9Text.Replace("\n", " | ") + ";cl13=" + cl13Text.Replace("\n", " | "),
                    cl9Text.Contains("CL 9") && cl13Text.Contains("CL 13") && cl9Text != cl13Text &&
                        !party.Any(value => cl9Text.Contains(value.CharacterName) || cl13Text.Contains(value.CharacterName)));
                party[1].Inventory.Remove((BlueprintItem)variant, 1);
                panel.Hide();
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

                // --- Oracle sanctuary activation: the owner's exact path ---
                // Only when the production reconciliation actually offered the
                // Oracle the Word of Recall row; a pre-repair or Oracle-absent
                // process records the absence above and skips this block.
                if (oracleClass != null)
                {
                    var oracleSourcesNow = TeleportationScrollAdapter.Enumerate(player).ToArray();
                    var oracleRowNow = oracleSourcesNow.FirstOrDefault(value =>
                        value.CasterId == oracleReader.UniqueId && value.Spell == TeleportSpellKind.WordOfRecall);
                    if (oracleRowNow == null)
                        CaptureTeleportScrolls("oracle-activation-skipped", new { reason = "row-absent" });
                    else
                    {
                        var oleg = rules.AllLocations.Single(value => value.Blueprint.AssetGuid == WordOfRecallDestinationPolicy.OlegId);
                        LocationData olegData;
                        if (!map.Locations.TryGetValue(oleg.Blueprint, out olegData)) throw new InvalidOperationException("Oleg's Trading Post has no native map record.");
                        setRevealed.Invoke(olegData, new object[] { true }); olegData.EdgesOpened = true; olegData.IsClosed = false;
                        var recallState = TeleportationWorldMapAdapter.ReadRecall(player);
                        ScrollsAssert("oracle-sanctuary-destination-known", "the working save resolves the Word of Recall sanctuary to Oleg's Trading Post before the capital",
                            "known=" + recallState.Known + ";established=" + recallState.Established +
                                ";destination=" + (recallState.DestinationId == null ? "null" : recallState.DestinationId),
                            recallState.Known && !recallState.Established && recallState.DestinationId == WordOfRecallDestinationPolicy.OlegId);
                        rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                        rig.ScrollTo(oleg.transform.position);
                        for (int frame = 0; frame < 30; frame++) yield return 0;
                        TeleportDestinationRows oracleRows = null;
                        for (int attempt = 0; attempt < 3 && (oracleRows == null || oracleRows.Actions.Count == 0); attempt++)
                        {
                            SelectTeleportationCastingPoint(panel, oleg);
                            foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                            for (int frame = 0; frame < 40; frame++)
                            {
                                oracleRows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                                if (oracleRows != null && oracleRows.Actions.Count > 0) break;
                                yield return 0;
                            }
                        }
                        var recallRow = oracleRows == null ? null : oracleRows.Actions.SingleOrDefault(value =>
                            value.Source.Kind == TeleportCastSourceKind.Scroll && value.Source.Spell == TeleportSpellKind.WordOfRecall &&
                            value.Source.CasterId == oracleReader.UniqueId);
                        ScrollsAssert("oracle-sanctuary-row-composed", "one sanctuary scroll action automatically binds the guaranteed Oracle reader",
                            "row=" + (recallRow == null ? "absent" : recallRow.Source.Uses.ToString(CultureInfo.InvariantCulture)),
                            recallRow != null && recallRow.Source.Uses == 2);
                        if (recallRow == null) throw new InvalidOperationException("The Oracle Word of Recall row was not composed at the sanctuary.");
                        int recallStockBefore = TeleportationScrollAdapter.Stock(player.Party, scrolls.WordOfRecall);
                        string oracleBooksBefore = string.Join("|", oracleReader.Descriptor.Spellbooks
                            .Select(value => TeleportResourceFingerprint(value)).ToArray());
                        oracleRows.QualificationRolls = new TeleportationFixtureRolls(new[] { 1 });
                        TeleportContextConfirmationPresenter.ResetDirectCastDiagnostics();
                        var recallEvent = oracleRows.Buttons[oracleRows.Actions.ToList().FindIndex(value => value.Key == recallRow.Key)].onClick;
                        using (var notices = new TeleportNotificationObserver())
                        {
                            recallEvent.Invoke();
                            recallEvent.Invoke();
                            ScrollsAssert("recall-direct-success-quiet", "native warning boundary publishes no successful Recall announcement",
                                "count=" + notices.Text.Count, notices.Text.Count == 0);
                        }
                        var recallRequest = TeleportContextConfirmationPresenter.LastDirectCast;
                        if (recallRequest == null || DialogMessageBox.Instance.IsShown)
                            throw new InvalidOperationException("The Oracle Word of Recall did not cast directly.");
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        recallEvent.Invoke(); // Later stale callback cannot authorize another attempt.
                        for (int frame = 0; frame < 12; frame++) yield return 0;
                        bool recallCommitted = recallRequest.Transaction.State == TeleportTransactionState.Completed &&
                            recallRequest.Transaction.Result != null &&
                            recallRequest.Transaction.Result.Status == TeleportExecutionStatus.Arrived &&
                            recallRequest.Transaction.Result.DestinationId == oleg.Blueprint.AssetGuid;
                        int recallStockAfter = TeleportationScrollAdapter.Stock(player.Party, scrolls.WordOfRecall);
                        string oracleBooksAfter = string.Join("|", oracleReader.Descriptor.Spellbooks
                            .Select(value => TeleportResourceFingerprint(value)).ToArray());
                        bool oracleStillUnknown = !oracleReader.Descriptor.Spellbooks.Any(ownedBook =>
                            Enumerable.Range(0, 10).Any(slotLevel => ownedBook.GetKnownSpells(slotLevel).Any(value =>
                                ReferenceEquals(value.Blueprint, scrolls.WordOfRecall.Ability))));
                        ScrollsAssert("oracle-sanctuary-cast-exactly-one", "the Oracle's Word of Recall scroll cast relocates the party to the sanctuary, consumes exactly one scroll and no book resource",
                            "committed=" + recallCommitted + ";stock=" + recallStockBefore + "->" + recallStockAfter +
                                ";booksUntouched=" + (oracleBooksBefore == oracleBooksAfter) + ";stillUnknown=" + oracleStillUnknown +
                                ";party=" + map.PartyLocation.AssetGuid,
                            recallCommitted && recallStockAfter == recallStockBefore - 1 &&
                                oracleBooksBefore == oracleBooksAfter && oracleStillUnknown &&
                                !TeleportContextConfirmationPresenter.Pending && !DialogMessageBox.Instance.IsShown);
                        CaptureTeleportScrolls("oracle-sanctuary-cast", new {
                            committed = recallCommitted, destination = oleg.Blueprint.AssetGuid,
                            stockBefore = recallStockBefore, stockAfter = recallStockAfter });
                        foreach (int tick in QualifyConsumedRecallFailure(panel, origin, oleg, oracleReader)) yield return tick;
                    }
                }
            }

            finally
            {
                // Exact fixture restoration in reverse order.
                foreach (var reader in blockedReaders) reader.Descriptor.State.MagicItemsForbidden.Release();
                blockedReaders.Clear();
                if (failurePenalty != null) party[1].Stats.GetStat(StatType.SkillUseMagicDevice).RemoveModifier(failurePenalty);
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


        // Every mutation below belongs to this guarded, disposable request and
        // is reversed before returning. The production forecast itself is read-only.
        private IEnumerable<int> QualifyAutomaticScrollReaders(Kingmaker.UI.GlobalMap.GlobalMapMessageBox panel,
            GlobalMapLocation target, Kingmaker.EntitySystem.Entities.UnitEntityData first,
            Kingmaker.EntitySystem.Entities.UnitEntityData trained)
        {
            var player = Game.Instance.Player;
            var party = player.Party.ToArray();
            var scroll = BlueprintBootstrap.TeleportationScrolls.Teleport;
            var second = party.FirstOrDefault(value => !ReferenceEquals(value, trained) &&
                scroll.IsUnitNeedUMDForUse(value.Descriptor));
            if (second == null) second = party.FirstOrDefault(value => !ReferenceEquals(value, first) && !ReferenceEquals(value, trained));
            if (second == null) throw new InvalidOperationException("A second disposable reader is required.");
            var originalClasses = second.Descriptor.Progression.Classes.ToArray();
            bool isolatedClasses = false;
            var secondStat = second.Stats.GetStat(StatType.SkillUseMagicDevice);
            int secondBase = secondStat.BaseValue;
            var modifiers = new List<ModifiableValue.Modifier>();
            var blocked = new List<Kingmaker.EntitySystem.Entities.UnitEntityData>();
            var affected = new List<Kingmaker.EntitySystem.Entities.UnitEntityData>();
            var failure = ScriptableObject.CreateInstance<BlueprintFeature>();
            failure.name = "KMG_Disposable_ScrollActivationFailure"; failure.Ranks = 1;
            var failureComponent = ScriptableObject.CreateInstance<Kingmaker.UnitLogic.FactLogic.AddSpellFailureChance>(); failureComponent.Chance = 100;
            failure.ComponentsArray = new Kingmaker.Blueprints.BlueprintComponent[] { failureComponent };
            Func<WorldMapPointSpellAction> current = () => TeleportationWorldMapAdapter.Compose(
                TeleportationWorldMapAdapter.Capture(false), target.Blueprint).Single(value =>
                    value.Source.Kind == TeleportCastSourceKind.Scroll && value.Source.Spell == TeleportSpellKind.Teleport);
            try
            {
                // The working party has only one naturally check-dependent
                // member. Temporarily isolate the other's class-list eligibility
                // within this synchronous control, restoring the exact native
                // ClassData references before yielding or exercising the UI.
                if (!scroll.IsUnitNeedUMDForUse(second.Descriptor))
                { second.Descriptor.Progression.Classes.Clear(); isolatedClasses = true; }
                if (!scroll.IsUnitNeedUMDForUse(second.Descriptor))
                    throw new InvalidOperationException("The isolated native reader still bypasses UMD.");
                foreach (var reader in party.Where(value => !scroll.IsUnitNeedUMDForUse(value.Descriptor)))
                {
                    // Native buff application is disabled on the strategic
                    // map. A request-local native feature owns the same exact
                    // AddSpellFailureChance subscriber without changing that gate.
                    if (reader.Descriptor.AddFact(failure) == null) throw new InvalidOperationException("The native owned failure fact was rejected.");
                    affected.Add(reader);
                }
                var winner = current();
                ScrollsAssert("native-item-failure-changes-best-reader", "a trained UMD reader beats class-list readers subject to native 100% spell failure",
                    "winner=" + winner.Source.CasterId + ";chance=" + winner.Source.ActivationChance.Probability,
                    winner.ReaderResolved && winner.Source.CasterId == trained.UniqueId && winner.Source.ActivationChance.Probability > 0m &&
                    TeleportationScrollAdapter.Enumerate(player).Where(value => value.Spell == TeleportSpellKind.Teleport &&
                        !scroll.IsUnitNeedUMDForUse(party.Single(unit => unit.UniqueId == value.CasterId).Descriptor))
                        .All(value => value.ActivationChance.Supported && value.ActivationChance.Probability == 0m));
                secondStat.BaseValue = 1;
                modifiers.Add(secondStat.AddModifier(trained.Stats.GetStat(StatType.SkillUseMagicDevice).ModifiedValue + 4 - secondStat.ModifiedValue,
                    (Kingmaker.Blueprints.GameLogicComponent)null, Kingmaker.Enums.ModifierDescriptor.UntypedStackable));
                winner = current();
                var supported = TeleportationScrollAdapter.Enumerate(player).Where(value => value.Spell == TeleportSpellKind.Teleport).ToArray();
                ScrollsAssert("native-best-fallible-reader", "among fallible readers native effective UMD/DC/failure chance selects the highest probability",
                    "winner=" + winner.Source.CasterId + ";chance=" + winner.Source.ActivationChance.Probability,
                    winner.ReaderResolved && winner.Source.CasterId == second.UniqueId && winner.Source.ActivationChance.Probability < 1m &&
                    supported.All(value => value.ActivationChance.Supported && value.ActivationChance.Probability <= winner.Source.ActivationChance.Probability));
                CaptureTeleportScrolls("native-fallible-forecasts", supported.Select(value => new { value.CasterId, value.ActivationChance.Supported, value.ActivationChance.NoCheck,
                    value.ActivationChance.Probability, value.ActivationChance.Diagnostic }).ToArray());
                secondStat.RemoveModifier(modifiers[0]); modifiers.Clear();
                modifiers.Add(secondStat.AddModifier(trained.Stats.GetStat(StatType.SkillUseMagicDevice).ModifiedValue - secondStat.ModifiedValue,
                    (Kingmaker.Blueprints.GameLogicComponent)null, Kingmaker.Enums.ModifierDescriptor.UntypedStackable));
                var expectedTie = Array.IndexOf(party, trained) < Array.IndexOf(party, second) ? trained : second;
                ScrollsAssert("native-reader-ties-stable", "equal native activation probabilities choose stable traveling-party order on every refresh",
                    "winner=" + current().Source.CasterId, Enumerable.Range(0, 12).All(index => current().Source.CasterId == expectedTie.UniqueId));
                foreach (var reader in affected) reader.Descriptor.RemoveFact(failure); affected.Clear();
                foreach (var modifier in modifiers) secondStat.RemoveModifier(modifier); modifiers.Clear();
                secondStat.BaseValue = secondBase;
                if (isolatedClasses)
                { foreach (var data in originalClasses) second.Descriptor.Progression.Classes.Add(data); isolatedClasses = false; }
                SelectTeleportationCastingPoint(panel, target);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                var rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                var action = current();
                int indexOf = rows.Actions.ToList().FindIndex(value => value.Key == action.Key);
                var originalButton = rows.Buttons[indexOf];
                var labels = originalButton.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                string routineText = string.Join("|", labels.Select(value => value.text));
                ScrollsAssert("group-row-native-presentation", "one reader-free native row stays inside the parchment with compact shared stock",
                    routineText, rows.Actions.Count(value => value.Key == action.Key) == 1 && routineText.Contains("3 available") &&
                    !party.Any(value => routineText.Contains(value.CharacterName)) &&
                    labels.All(value => !value.isTextTruncated && !value.isTextOverflowing && Kingmaker.UI.Common.UIUtility.IsTransformInScreen(value.transform)));
                var rng = UnityEngine.Random.state;
                var rules = Game.Instance.Rulebook.Context;
                var events = rules.AllEvents.ToArray();
                string before = ScrollReaderResources(player);
                var refresh = typeof(TeleportDestinationRows).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
                for (int read = 0; read < 12; read++)
                { current(); TeleportContextPresentation.CompactRow(action, TeleportationText.Get); refresh.Invoke(rows, null); }
                ScrollsAssert("ranking-render-refresh-read-only", "ranking, composition and native row refresh change no RNG, rule history, item charges, buffs, parts, optional resources or spell slots",
                    "rng=" + rng.Equals(UnityEngine.Random.state) + ";ruleContext=" + ReferenceEquals(rules, Game.Instance.Rulebook.Context) +
                        ";resources=" + (before == ScrollReaderResources(player)), rng.Equals(UnityEngine.Random.state) &&
                    ReferenceEquals(rules, Game.Instance.Rulebook.Context) && events.SequenceEqual(rules.AllEvents) && before == ScrollReaderResources(player));
                // A changed eligible reader keeps the existing group widget.
                first.Descriptor.State.MagicItemsForbidden.Retain(); blocked.Add(first);
                for (int frame = 0; frame < 4; frame++) yield return 0;
                var changed = rows.Actions.Single(value => value.Key == action.Key);
                ScrollsAssert("reader-change-preserves-native-row", "reader availability changes replace the internal binding without replacing the group button",
                    "reader=" + changed.Source.CasterId, changed.ReaderResolved && changed.Source.CasterId != first.UniqueId &&
                    ReferenceEquals(originalButton, rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == action.Key)]));
                first.Descriptor.State.MagicItemsForbidden.Release(); blocked.Clear();
                for (int frame = 0; frame < 4; frame++) yield return 0;
                var sourceEvent = originalButton.onClick;
                sourceEvent.Invoke();
                var confirmation = TeleportContextConfirmationPresenter.Current;
                if (confirmation == null) throw new InvalidOperationException("The changed-reader confirmation did not open.");
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var confirmEvent = TeleportationFixtureDialogButton("m_ButtonYes").onClick;
                first.Descriptor.State.MagicItemsForbidden.Retain(); blocked.Add(first);
                confirmEvent.Invoke(); // Revalidation must also work before the next Update.
                for (int frame = 0; frame < 4; frame++) yield return 0;
                ScrollsAssert("changed-reader-cancels-confirmation", "a different best reader cancels ordinary Teleport before spending or changing the displayed risk consent",
                    "state=" + confirmation.Transaction.State, confirmation.Transaction.State == TeleportTransactionState.Cancelled &&
                    confirmation.Execution.Resource == null && !TeleportContextConfirmationPresenter.Pending &&
                    TeleportationScrollAdapter.Stock(player.Party, scroll) == 3);
                first.Descriptor.State.MagicItemsForbidden.Release(); blocked.Clear();
                SelectTeleportationCastingPoint(panel, target);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                sourceEvent = rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == action.Key)].onClick;
                foreach (var reader in party) { reader.Descriptor.State.MagicItemsForbidden.Retain(); blocked.Add(reader); }
                using (var notifications = new TeleportNotificationObserver())
                {
                    var contextBefore = Game.Instance.Rulebook.Context;
                    sourceEvent.Invoke(); sourceEvent.Invoke();
                    ScrollsAssert("no-reader-at-click-unspent", "losing all eligible readers emits one honest no-reader notification and performs no native attempt",
                        "messages=" + string.Join("|", notifications.Text), notifications.Text.Count == 1 &&
                        notifications.Text[0].Contains("No eligible traveling-party member") &&
                        !party.Any(value => notifications.Text[0].Contains(value.CharacterName)) &&
                        TeleportationScrollAdapter.Stock(player.Party, scroll) == 3 && !TeleportContextConfirmationPresenter.Pending &&
                        ReferenceEquals(contextBefore, Game.Instance.Rulebook.Context));
                }
            }
            finally
            {
                foreach (var reader in blocked) reader.Descriptor.State.MagicItemsForbidden.Release();
                foreach (var reader in affected) reader.Descriptor.RemoveFact(failure);
                foreach (var modifier in modifiers) secondStat.RemoveModifier(modifier);
                secondStat.BaseValue = secondBase;
                if (isolatedClasses) foreach (var data in originalClasses) second.Descriptor.Progression.Classes.Add(data);
                UnityEngine.Object.Destroy(failure); UnityEngine.Object.Destroy(failureComponent);
                if (TeleportContextConfirmationPresenter.Pending && DialogMessageBox.Instance.IsShown)
                    TeleportationFixtureDialogButton("m_ButtonNo").onClick.Invoke();
                panel.Hide();
            }
        }
        private IEnumerable<int> QualifyConsumedRecallFailure(Kingmaker.UI.GlobalMap.GlobalMapMessageBox panel,
            GlobalMapLocation origin, GlobalMapLocation sanctuary, Kingmaker.EntitySystem.Entities.UnitEntityData reader)
        {
            var player = Game.Instance.Player; var party = player.Party.ToArray();
            var scroll = BlueprintBootstrap.TeleportationScrolls.WordOfRecall;
            var blocked = new List<Kingmaker.EntitySystem.Entities.UnitEntityData>();
            var blueprint = ScriptableObject.CreateInstance<BlueprintFeature>();
            blueprint.name = "KMG_Disposable_ConsumedRecallFailure"; blueprint.Ranks = 1;
            var component = ScriptableObject.CreateInstance<Kingmaker.UnitLogic.FactLogic.AddSpellFailureChance>(); component.Chance = 100;
            blueprint.ComponentsArray = new Kingmaker.Blueprints.BlueprintComponent[] { component };
            bool factAdded = false;
            try
            {
                foreach (var unit in party.Where(value => !ReferenceEquals(value, reader)))
                { unit.Descriptor.State.MagicItemsForbidden.Retain(); blocked.Add(unit); }
                factAdded = reader.Descriptor.AddFact(blueprint) != null;
                if (!factAdded) throw new InvalidOperationException("Native Recall failure fact was rejected.");
                GlobalMapRules.Instance.SetCurrentPosition(new MapPosition(origin.Blueprint)); GlobalMapRules.Instance.UpdatePawnPosition();
                SelectTeleportationCastingPoint(panel, sanctuary); foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                var rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                var action = rows.Actions.Single(value => value.Source.Kind == TeleportCastSourceKind.Scroll && value.Source.Spell == TeleportSpellKind.WordOfRecall);
                int beforeStock = TeleportationScrollAdapter.Stock(player.Party, scroll);
                string beforeBooks = string.Join("|", party.SelectMany(value => value.Descriptor.Spellbooks).Select(TeleportResourceFingerprint));
                var callback = rows.Buttons[rows.Actions.ToList().FindIndex(value => value.Key == action.Key)].onClick;
                TeleportContextConfirmationPresenter.ResetDirectCastDiagnostics();
                using (var notifications = new TeleportNotificationObserver())
                {
                    callback.Invoke(); callback.Invoke();
                    var cast = TeleportContextConfirmationPresenter.LastDirectCast;
                    yield return 0; callback.Invoke();
                    var resource = cast == null ? null : cast.Execution.Resource as TeleportationScrollCastResource;
                    string expected = TeleportContextPresentation.ScrollActivationFailure(reader.CharacterName, TeleportSpellKind.WordOfRecall,
                        TeleportExpenditure.ExactlyOne, TeleportationText.Get);
                    ScrollsAssert("recall-failed-activation-consumed-notification", "a native non-UMD activation failure consumes one scroll, names the actual reader once, and never reports an arrival or retries",
                        "messages=" + string.Join("|", notifications.Text), cast != null && cast.Transaction.State == TeleportTransactionState.ActivationFailedSpent &&
                        resource != null && resource.ActualReaderId == reader.UniqueId && resource.NativeEventCount == 1 && !resource.RequiredUmd &&
                        resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne && TeleportationScrollAdapter.Stock(player.Party, scroll) == beforeStock - 1 &&
                        beforeBooks == string.Join("|", party.SelectMany(value => value.Descriptor.Spellbooks).Select(TeleportResourceFingerprint)) &&
                        notifications.Text.Count == 1 && notifications.Text[0] == expected && GlobalMapRules.State.PartyLocation == origin.Blueprint &&
                        GlobalMapRules.State.TravelData == null && !TeleportContextConfirmationPresenter.Pending && !DialogMessageBox.Instance.IsShown);
                    CaptureTeleportScrolls("native-consumed-recall-failure", new { evidence = resource == null ? null : resource.Evidence(), notifications = notifications.Text.ToArray() });
                }
            }
            finally
            {
                if (factAdded) reader.Descriptor.RemoveFact(blueprint);
                foreach (var unit in blocked) unit.Descriptor.State.MagicItemsForbidden.Release();
                UnityEngine.Object.Destroy(blueprint); UnityEngine.Object.Destroy(component);
                panel.Hide();
            }
        }

        private static string ScrollReaderResources(Player player)
        {
            return TeleportationDiagnosticJson.Serialize(new {
                stock = player.Party.Select(value => value.Inventory).Distinct().SelectMany(value => value).Select(value => new {
                    id = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value), value.Count, value.Charges, value.IsIdentified }).ToArray(),
                units = player.Party.Select(value => new { value.UniqueId,
                    umdBase = value.Stats.GetStat(StatType.SkillUseMagicDevice).BaseValue,
                    umd = value.Stats.GetStat(StatType.SkillUseMagicDevice).ModifiedValue,
                    books = value.Descriptor.Spellbooks.Select(TeleportResourceFingerprint).ToArray(),
                    features = value.Descriptor.Progression.Features.Enumerable.Select(fact => new { id = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(fact), fact.Active }).ToArray(),
                    buffs = value.Descriptor.Buffs.RawFacts.Select(fact => new { id = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(fact), fact.Active }).ToArray(),
                    resources = ((System.Collections.IDictionary)typeof(UnitAbilityResourceCollection).GetField("m_Resources", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(value.Descriptor.Resources)).Values.Cast<UnitAbilityResource>().Select(resource => new { id = resource.Blueprint.AssetGuid, resource.Amount }).ToArray(),
                    parts = ScrollReaderPartIds(value.Descriptor) }).ToArray() });
        }
        private static string[] ScrollReaderPartIds(UnitDescriptor unit)
        {
            var manager = typeof(UnitDescriptor).GetField("m_Parts", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(unit);
            var parts = (System.Collections.IDictionary)manager.GetType().GetField("m_Parts", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);
            return parts.Keys.Cast<object>().Select(key => key.ToString() + ":" +
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(parts[key])).OrderBy(value => value, StringComparer.Ordinal).ToArray();
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
                claims = "Real party scroll items compose shared-stock rows for book and UMD-only readers; one scroll cast consumes exactly one item with no slot and rebuilds arrows; native cancellation consumes nothing; vendor migration proves fresh-stock marker-only, batch-once per shared family, and buy-out persistence; a genuine optional CotW Oracle with zero UMD ranks and no known Word of Recall reads the scroll through the production level-6 list reconciliation and casts it once at the sanctuary. Request-local items/stats/parts/world state; no save writes.",
                captures = _teleportationScrollsCaptures, assertions = _teleportationScrollsAssertions,
                saveWriteObserved = _workingSaveSmoke.WriteObserved, error = failure == null ? null : failure.ToString() });
            Complete(CreateResult(failure != null ? RuntimeTestStatuses.Error : _teleportationScrollsAssertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, _teleportationScrollsAssertions, failure == null ? null : failure.ToString()));
        }
    }
}
