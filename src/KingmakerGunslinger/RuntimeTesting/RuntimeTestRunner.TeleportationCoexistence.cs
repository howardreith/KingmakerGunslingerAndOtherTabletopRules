using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Assets.Console.GamepadInput;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.GlobalMap;
using Kingmaker.UI._ConsoleUI.GlobalMap;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool IsTeleportationCoexistenceFixture { get { return _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationCoexistence ||
            _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationCoexistenceGamepad; } }
        private IEnumerable<int> RunTeleportationCoexistence()
        {
            var game = Game.Instance; var player = game.Player; var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State;
            bool console = game.IsControllerGamepad, enabled = _context.FeatureModules.Active.TeleportationSpells;
            var desktop = console ? null : TeleportationFixturePanel(); var pad = console ? TeleportGamepadPanel() : null;
            Component host = console ? (Component)pad : desktop;
            var main = player.MainCharacter.Value;
            var ledger = main.Descriptor.Get<UnitPartTeleportFamiliarity>();
            if (!IsTeleportationCoexistenceFixture || !_request.ExitAfterCompletion || _workingSaveSmoke.WriteObserved ||
                map.TravelData != null || map.CurrentEncounterData != null || enabled && ledger == null ||
                WorldMapPointSpellActionPatches.Installed != enabled || WorldMapPointConsoleSpellActionPatches.Installed != enabled)
                throw new InvalidOperationException("Coexistence requires the guarded stationary map and matching module hook state.");
            var position = map.PartyPosition; var last = map.LastLocation; float miles = map.MilesTravelled;
            var history = map.HistoryTravels.ToArray(); var perception = map.PerceptionRolledLocations.ToArray();
            var roster = TeleportationTravelers.Read(player);
            var gameTime = player.GameTime; var camera = TeleportationCastingCamera(); var cameraPosition = camera.GetPosition();
            var points = map.Locations.ToArray(); var edges = map.Edges.ToArray();
            var snapshots = points.Select(value => new TeleportNativeFieldSnapshot(value.Value)).Concat(edges.Select(value => new TeleportNativeFieldSnapshot(value.Value)))
                .Concat(ledger == null ? new TeleportNativeFieldSnapshot[0] : new[] { new TeleportNativeFieldSnapshot(ledger) }).ToArray();
            var owners = new List<TeleportResourceFixtureOwner>();
            var movement = new TeleportInteractionMovementObserver();
            var exceptions = new List<string>();
            Application.LogCallback observe = (message, stack, type) => { if (type == LogType.Exception || type == LogType.Error && message.Contains("Exception")) exceptions.Add(message + "\n" + stack); };
            EventBus.Subscribe(movement); Application.logMessageReceived += observe;
            TeleportCoexistenceForeignAction foreign = null;
            var baseLayers = console ? GamePad.Instance.Layers.ToArray() : null;
            try
            {
                var chain = FindTeleportInteractionChain(rules); var origin = chain[0]; var target = chain[2];
                var reveal = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
                foreach (var point in chain) { reveal.Invoke(point.Data, new object[] { true }); point.Data.EdgesOpened = true; }
                foreach (var edge in chain[1].Edges.Where(value => value.GetOppositeLocation(chain[1]) == origin || value.GetOppositeLocation(chain[1]) == target)) edge.Data.UpdateExplored(1, 1);
                if (enabled)
                {
                    var state = new TeleportFamiliarityState(); state.MigrateLegacy(chain.Select(point => point.Blueprint.AssetGuid));
                    typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ledger, state.Serialize());
                }
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                string payload = ledger == null ? null : ledger.Read().Serialize();
                foreign = new TeleportCoexistenceForeignAction(console, _request.RunId);
                Action select = () => { if (console) SelectTeleportGamepadPoint(target); else SelectTeleportationCastingPoint(desktop, target); };
                Func<IEnumerable<int>> wait = () => console ? WaitTeleportGamepadPanel(pad) : WaitTeleportDisabledPanel(desktop);
                Action hide = () => { if (console) WorldMapPointConsoleSpellActionPatches.Model(pad)?.Cancel(); else desktop.Hide(); };
                Func<int> containers = () => console ? host.GetComponentsInChildren<TeleportConsoleDestinationRows>(true).Length : host.GetComponentsInChildren<TeleportDestinationRows>(true).Length;
                Func<string> native = () => console ? TeleportGamepadNativeButtons(pad) : TeleportationNativeButtons(desktop);
                select(); foreach (int tick in wait()) yield return tick;
                string originalNative = native();
                TeleportInteractionAssert("foreign-no-source", "Foreign and native controls exist before KMG augmentation; no KMG UI is constructed without a source",
                    "created=" + foreign.KmgConstructions, foreign.ControlMatches() && foreign.KmgConstructions == 0 && containers() == 0 && movement.Starts == 0);
                foreign.Fire();
                TeleportInteractionAssert("foreign-callback-once", "Invoking the independent foreign callback fires exactly once and never starts native Travel", "fires=" + foreign.Fires,
                    foreign.Fires == 1 && movement.Starts == 0 && foreign.ControlMatches());
                hide(); yield return 0;
                TeleportInteractionAssert("foreign-no-source-hide", "Native dismissal leaves the foreign control, callback and navigation intact", "matches=" + foreign.ControlMatches(),
                    foreign.ControlMatches() && foreign.NavigationMatches(false) && containers() == 0 && (!console || GamePad.Instance.Layers.SequenceEqual(baseLayers)));
                var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "ba34257984f4c41408ce1dc2004e342e", "foreign coexistence native book");
                var caster = player.Party.First(unit => TeleportationSpellbookAdapter.CasterAvailable(unit) && unit.Descriptor.GetSpellbook(wizard.Spellbook) == null);
                var owner = new TeleportResourceFixtureOwner(caster); owners.Add(owner); var book = owner.AddBook(wizard.Spellbook);
                foreach (var spell in new[] { BlueprintBootstrap.Teleportation.Teleport, BlueprintBootstrap.Teleportation.GreaterTeleport })
                {
                    book.AddKnown(spell == BlueprintBootstrap.Teleportation.Teleport ? 5 : 7, spell, true);
                    if (!book.Memorize(new AbilityData(spell, book), null)) throw new InvalidOperationException("Real coexistence preparation failed.");
                }
                book.Rest(); string slots = TeleportResourceFingerprint(book);
                for (int repeat = 0; repeat < 3; repeat++)
                {
                    select(); foreach (int tick in wait()) yield return tick;
                    TeleportInteractionAssert("foreign-append-" + repeat, "Native and foreign references/order/labels/enabled state/callbacks remain unchanged; KMG appends only its own container",
                        "containers=" + containers(), containers() == (enabled ? 1 : 0) && foreign.ControlMatches() && originalNative == native() && foreign.NavigationPreserved &&
                        foreign.NavigationMatches(true) && foreign.Constructions == 1 && slots == TeleportResourceFingerprint(book));
                    CaptureTeleportInteraction("foreign-append-" + repeat, foreign.Evidence());
                    if (repeat == 1) { if (console) InvokeTeleportGamepadInput(pad, "OnCancelPressed"); else game.UI.EscManager.OnEscPressed(); }
                    else hide();
                    yield return 0;
                    TeleportInteractionAssert("foreign-dismiss-" + repeat, "Hide/Escape removes KMG rows only and retains every pre-existing foreign/native input entry", "matches=" + foreign.ControlMatches(),
                        containers() == 0 && foreign.ControlMatches() && foreign.NavigationMatches(false) && foreign.Fires == 1 && slots == TeleportResourceFingerprint(book) &&
                        (!console || GamePad.Instance.Layers.SequenceEqual(baseLayers)));
                }
                if (enabled && !console)
                {
                    // Gate 3: compact two-line rows stay within the native inner
                    // content width, and the native settlement-teleport control is
                    // distinguished while spell rows coexist.
                    select(); foreach (int tick in wait()) yield return tick;
                    var kmgRows = desktop.GetComponentInChildren<TeleportDestinationRows>(true);
                    float innerWidth = ((RectTransform)kmgRows.transform).rect.width;
                    TeleportInteractionAssert("compact-rows-fit", "two-line title/detail rows render within the native inner content width",
                        "rows=" + kmgRows.Actions.Count + ";width=" + innerWidth.ToString("0.##"),
                        kmgRows.Actions.Count > 0 && kmgRows.Buttons.All(value => {
                            var rowLabel = value.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                            return rowLabel != null && rowLabel.preferredWidth <= innerWidth + 0.5f; }) &&
                        kmgRows.Buttons.Select((value, index) => value.GetComponentInChildren<TMPro.TextMeshProUGUI>(true).text ==
                            TeleportContextPresentation.CompactRow(kmgRows.Actions[index], TeleportationText.Get)).All(value => value));
                    var controllers = (GameObject)typeof(GlobalMapMessageBox)
                        .GetField("m_TeleportControllers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(desktop);
                    bool controllersActive = controllers.activeSelf;
                    var teleportButton = controllers.GetComponentsInChildren<UnityEngine.UI.Button>(true).FirstOrDefault(value => {
                        int events = value.onClick.GetPersistentEventCount();
                        for (int index = 0; index < events; index++)
                            if (string.Equals(value.onClick.GetPersistentMethodName(index), "OnTeleportPressed", StringComparison.Ordinal)) return true;
                        return false; });
                    var teleportLabel = teleportButton == null ? null : teleportButton.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                    string originalTeleportLabel = teleportLabel == null ? null : teleportLabel.text;
                    // The fixture activates the native settlement control the way
                    // native FillDialogInfoLocation does when a settlement circle
                    // covers the point, then re-runs the exact production append.
                    controllers.SetActive(true);
                    WorldMapPointSpellActionRuntime.Append(desktop);
                    CaptureTeleportInteraction("settlement-relabel-immediate", new {
                        productionFieldNull = WorldMapPointSpellActionPatches.TeleportControllersField == null,
                        productionValueNull = WorldMapPointSpellActionPatches.TeleportControllersField == null ?
                            null : WorldMapPointSpellActionPatches.TeleportControllersField.GetValue(desktop) == null,
                        productionFieldType = WorldMapPointSpellActionPatches.TeleportControllersField == null ?
                            null : WorldMapPointSpellActionPatches.TeleportControllersField.FieldType.FullName,
                        controllersActiveInHierarchy = controllers.activeInHierarchy,
                        labelAfterAppend = teleportLabel == null ? null : teleportLabel.text,
                        labelObjectFound = teleportLabel != null,
                        desiredLabel = TeleportContextPresentation.SettlementTeleportLabel(TeleportationText.Get),
                        teleportTextFromGet = TeleportationText.Get("SettlementTeleport", "Settlement Teleport") });
                    yield return 0;
                    TeleportInteractionAssert("settlement-label-coexists",
                        "the native settlement-teleport button is relabeled while spell rows coexist, with its callback untouched",
                        "label=" + (teleportLabel == null ? "absent" : teleportLabel.text) + ";callbacks=" +
                            (teleportButton == null ? 0 : teleportButton.onClick.GetPersistentEventCount()),
                        containers() == 1 && teleportLabel != null &&
                            teleportLabel.text == TeleportContextPresentation.SettlementTeleportLabel(TeleportationText.Get) &&
                            teleportButton.onClick.GetPersistentEventCount() > 0);
                    hide(); yield return 0;
                    TeleportInteractionAssert("settlement-label-restored",
                        "removing the spell rows restores the exact native settlement label and control state",
                        "label=" + (teleportLabel == null ? "absent" : teleportLabel.text),
                        containers() == 0 && teleportLabel.text == originalTeleportLabel);
                    controllers.SetActive(controllersActive);
                }
                if (enabled)
                {
                    foreach (int tick in RunTeleportCoexistenceModals(desktop, pad, target, foreign, () => TeleportResourceFingerprint(book))) yield return tick;
                    select(); foreach (int tick in wait()) yield return tick;
                    foreach (var source in TeleportationSpellbookAdapter.Enumerate(player).Where(source => ReferenceEquals(source.Book, book)).ToArray())
                        if (!book.Spend(source.Ability, false)) throw new InvalidOperationException("Native coexistence exhaustion fixture failed.");
                    for (int frame = 0; frame < 6; frame++) yield return 0;
                    TeleportInteractionAssert("foreign-exhaustion-refresh", "Live exhaustion removes only KMG rows and leaves foreign callback/content/navigation unchanged",
                        "containers=" + containers(), containers() == 0 && foreign.ControlMatches() && foreign.NavigationMatches(false) && originalNative == native());
                    hide(); book.Rest(); slots = TeleportResourceFingerprint(book);
                }
                select(); foreach (int tick in wait()) yield return tick;
                if (console) InvokeTeleportGamepadInput(pad, "OnConfirmPressed");
                else ((TMPro.TextMeshProUGUI)typeof(GlobalMapMessageBox).GetField("m_AcceptText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(desktop))
                    .GetComponentInParent<UnityEngine.UI.Button>().onClick.Invoke();
                TeleportInteractionAssert("foreign-native-travel-once", "The native first/default action starts exactly one route and spends no spell; foreign callback remains unchanged",
                    "starts=" + movement.Starts, movement.Starts == 1 && map.TravelData != null && map.TravelData.Walking && slots == TeleportResourceFingerprint(book) && foreign.Fires == 1 && foreign.ControlMatches());
                map.TravelData.Stop(); map.TravelData = null; rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                select(); foreach (int tick in wait()) yield return tick;
                if (console) WorldMapPointConsoleSpellActionPatches.Model(pad).Dispose(); else desktop.Dispose();
                yield return 0;
                TeleportInteractionAssert("foreign-dispose", "Native disposal removes KMG-owned controls while preserving foreign control identity/content", "matches=" + foreign.ControlMatches(),
                    containers() == 0 && foreign.ControlMatches() && foreign.NavigationMatches(false) && foreign.Fires == 1);
                TeleportInteractionAssert("foreign-off-inert", "Disabled module constructs no KMG UI even with a real prepared source", "created=" + foreign.KmgConstructions,
                    enabled || foreign.KmgConstructions == 0 && !TeleportFamiliarityRuntime.Enabled && !TeleportFamiliarityPatches.Installed && BlueprintBootstrap.TeleportationPublication == null);
                TeleportInteractionAssert("foreign-no-arrival", "Selection/modal/cancellation/fixture route starts add no ordinary familiarity", "payload=" + payload,
                    (ledger == null ? null : ledger.Read().Serialize()) == payload);
                CaptureTeleportInteraction("foreign-final", foreign.Evidence());
            }
            finally
            {
                if (console) CloseTeleportGamepadPanels(); else CloseTeleportationFixturePanels();
                foreign?.Dispose();
                if (map.TravelData != null) map.TravelData.Stop(); map.TravelData = null;
                foreach (var owner in owners.AsEnumerable().Reverse()) owner.Restore();
                foreach (var snapshot in snapshots) snapshot.Restore();
                foreach (var key in map.Locations.Keys.Where(key => !points.Any(pair => pair.Key == key)).ToArray()) map.Locations.Remove(key);
                foreach (var key in map.Edges.Keys.Where(key => !edges.Any(pair => pair.Key == key)).ToArray()) map.Edges.Remove(key);
                map.HistoryTravels.Clear(); map.HistoryTravels.AddRange(history); map.PerceptionRolledLocations.Clear(); map.PerceptionRolledLocations.UnionWith(perception);
                rules.SetCurrentPosition(position); rules.UpdatePawnPosition(); map.LastLocation = last; map.MilesTravelled = miles; player.GameTime = gameTime; camera.ScrollTo(cameraPosition);
                EventBus.Unsubscribe(movement); Application.logMessageReceived -= observe;
                TeleportInteractionAssert("foreign-zero-ui-exceptions", "Zero native/mod UI exceptions during the foreign coexistence fixture", "count=" + exceptions.Count, exceptions.Count == 0);
                CaptureTeleportInteraction("foreign-ui-exceptions", exceptions);
                TeleportInteractionAssert("foreign-fixture-cleanup", "Exact source/ledger/native field cleanup, no remaining foreign control and no campaign save write",
                    "writeObserved=" + _workingSaveSmoke.WriteObserved, snapshots.All(snapshot => snapshot.Matches()) && owners.All(owner => owner.IsRestored()) &&
                    ReferenceEquals(ledger, main.Descriptor.Get<UnitPartTeleportFamiliarity>()) && roster.Matches(TeleportationTravelers.Read(player)) &&
                    map.HistoryTravels.SequenceEqual(history) && map.PerceptionRolledLocations.SequenceEqual(perception) && !_workingSaveSmoke.WriteObserved &&
                    host.GetComponentsInChildren<Transform>(true).All(item => item.name != "KMG_RUNTIME_FOREIGN_ACTION"));
            }
        }
    }
}
