using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private TeleportPersistencePlan _teleportPersistencePlan;
        private IEnumerator<int> _teleportPersistenceSteps;
        private readonly List<RuntimeTestAssertion> _teleportPersistenceAssertions = new List<RuntimeTestAssertion>();
        private readonly List<object> _teleportPersistenceEvents = new List<object>();
        private string[] _teleportPersistenceChain;
        private JObject _teleportPersistenceFinal, _teleportPersistenceSaved;
        private string TeleportPersistencePath { get { return Path.Combine(_request.EvidenceDirectory, "teleportation-persistence.json"); } }
        private void PersistenceAssert(string id, string expectation, bool pass, object evidence)
        {
            _teleportPersistenceEvents.Add(new { id, evidence });
            _teleportPersistenceAssertions.Add(Assertion("teleportation-persistence-" + id, expectation,
                TeleportationDiagnosticJson.Serialize(evidence), pass, TeleportPersistencePath));
            if (!pass) throw new InvalidOperationException("Persistence assertion failed: " + id);
        }
        private void PollTeleportationPersistence()
        {
            if (_teleportPersistencePlan == null || !_request.ExitAfterCompletion || _workingSaveSmoke == null ||
                !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Persistence phase lacks its guarded exact input or intact write boundary.");
            if (_teleportationMapLoad == null)
            {
                _teleportationMapLoad = Stopwatch.StartNew();
                if (_teleportPersistencePlan.Phase == "A")
                { Game.Instance.LoadArea(Game.Instance.BlueprintRoot.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None); return; }
            }
            if (_teleportationMapLoad.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Native persistence phase timed out.");
            if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive) return;
            if (GlobalMapRules.Instance == null || Game.Instance.CurrentMode != GameModeType.GlobalMap)
                throw new InvalidOperationException("Persistence input did not restore the real native world map.");
            if (_teleportPersistenceSteps == null) _teleportPersistenceSteps = RunTeleportationPersistence().GetEnumerator();
            Exception failure = null;
            try { if (_teleportPersistenceSteps.MoveNext()) return; }
            catch (Exception exception) { failure = exception; }
            try { _teleportPersistenceSteps.Dispose(); }
            catch (Exception exception) { failure = failure == null ? exception : new AggregateException(failure, exception); }
            _teleportPersistenceSteps = null;
            WriteTeleportPersistence(failure == null ? null : failure.ToString());
            Complete(CreateResult(failure == null ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Error,
                _teleportPersistenceAssertions, failure == null ? null : failure.ToString()));
        }
        private void StopTeleportPersistence(RuntimeTestResult result)
        {
            if (_teleportPersistenceSteps == null) return;
            var steps = _teleportPersistenceSteps; _teleportPersistenceSteps = null;
            try { steps.Dispose(); }
            catch (Exception exception) { result.Status = RuntimeTestStatuses.Error; result.Diagnostics.Add(exception.ToString()); }
            WriteTeleportPersistence("Runner stopped before the native persistence phase completed.");
        }
        private void WriteTeleportPersistence(string error)
        {
            WriteTeleportationForensicJson(TeleportPersistencePath, new { schemaVersion = 1, runId = _request.RunId,
                transactionId = _teleportPersistencePlan.Transaction, phase = _teleportPersistencePlan.Phase,
                processId = Process.GetCurrentProcess().Id, dllSha256 = TeleportPersistencePlan.Hash(typeof(RuntimeTestRunner).Assembly.Location),
                input = _teleportPersistencePlan.Input, finalSnapshot = _teleportPersistenceFinal, savedInfo = _teleportPersistenceSaved,
                events = _teleportPersistenceEvents, assertions = _teleportPersistenceAssertions, error });
        }
        private IEnumerable<int> RunTeleportationPersistence()
        {
            var plan = _teleportPersistencePlan; var game = Game.Instance; var map = GlobalMapRules.State;
            var rules = GlobalMapRules.Instance; var main = game.Player.MainCharacter.Value;
            bool enabled = plan.Phase != "C";
            PersistenceAssert("module-state", "Only phase C disables Teleportation; publication and all hooks agree",
                _context.FeatureModules.Active.TeleportationSpells == enabled && TeleportFamiliarityRuntime.Enabled == enabled &&
                TeleportFamiliarityPatches.Installed == enabled && TeleportExplorationGuardPatches.Installed == enabled &&
                WorldMapPointSpellActionPatches.Installed == enabled && WorldMapPointConsoleSpellActionPatches.Installed == enabled &&
                (BlueprintBootstrap.TeleportationPublication != null) == enabled, new { enabled });
            if (main == null || map.TravelData != null || map.CurrentEncounterData != null)
                throw new InvalidOperationException("Native stationary campaign prerequisites differ.");
            var ledger = plan.Phase == "A" ? TeleportFamiliarityRuntime.EnsureLedger(game.Player) : main.Descriptor.Get<UnitPartTeleportFamiliarity>();
            if (ledger == null) throw new InvalidOperationException("Canonical persisted familiarity owner is absent.");
            GlobalMapLocation[] chain;
            if (plan.Phase == "A") chain = FindTeleportInteractionChain(rules);
            else chain = ((JArray)plan.Expected["chainIds"]).Select(id => rules.AllLocations.Single(value => value.Blueprint.AssetGuid == (string)id)).ToArray();
            _teleportPersistenceChain = chain.Select(value => value.Blueprint.AssetGuid).ToArray();
            JObject initial = CaptureTeleportPersistence();
            if (plan.Phase != "A") PersistenceAssert("fresh-owner-payload", "Fresh native load restores exact owner, migration, counts and exploration boundary",
                JToken.DeepEquals(initial, plan.Expected), new { expected = plan.Expected, actual = initial });
            if (plan.Phase == "D") { _teleportPersistenceFinal = initial; yield break; }
            var panel = TeleportationFixturePanel();
            if (plan.Phase == "C")
            {
                SelectTeleportationCastingPoint(panel, chain[0]);
                foreach (int tick in WaitTeleportDisabledPanel(panel)) yield return tick;
                PersistenceAssert("disabled-native-panel", "Native Travel remains available with no KMG rows or payload change",
                    panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 0 &&
                    panel.GetComponentsInChildren<UnityEngine.UI.Button>(true).Any(button => button.interactable && button.gameObject.activeInHierarchy) &&
                    JToken.DeepEquals(initial, CaptureTeleportPersistence()), new { native = TeleportationNativeButtons(panel) });
                var movement = new TeleportInteractionMovementObserver();
                var cancelledPosition = map.PartyPosition; var cancelledLast = map.LastLocation; float cancelledMiles = map.MilesTravelled;
                Kingmaker.PubSubSystem.EventBus.Subscribe(movement);
                try
                {
                    var accept = ((TMPro.TextMeshProUGUI)typeof(Kingmaker.UI.GlobalMap.GlobalMapMessageBox)
                        .GetField("m_AcceptText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(panel)).GetComponentInParent<UnityEngine.UI.Button>();
                    accept.onClick.Invoke();
                    PersistenceAssert("disabled-native-travel-once", "The unchanged native default Travel callback starts one real route without touching saved familiarity",
                        movement.Starts == 1 && map.TravelData != null && map.TravelData.Walking &&
                        ledger.Read().Serialize() == (string)initial["canonicalState"] &&
                        ledger.ReadExplorationBoundary().Serialize() == (string)initial["boundary"],
                        new { movement.Starts, route = DescribeFamiliarityRoute(map.TravelData), ledger = ledger.Read().Serialize() });
                }
                finally
                {
                    if (map.TravelData != null) map.TravelData.Stop();
                    map.TravelData = null; // Dispose only this request's cancelled route before the native save.
                    rules.SetCurrentPosition(cancelledPosition); rules.UpdatePawnPosition(); map.LastLocation = cancelledLast; map.MilesTravelled = cancelledMiles;
                    Kingmaker.PubSubSystem.EventBus.Unsubscribe(movement); panel.Hide();
                }
                PersistenceAssert("disabled-cancelled-route", "Cancelled native route restores its origin with the exact saved fields untouched",
                    JToken.DeepEquals(initial, CaptureTeleportPersistence()), CaptureTeleportPersistence());
            }
            else
            {
                foreach (int tick in EstablishTeleportPersistence(ledger, chain, plan.Phase == "A")) yield return tick;
            }
            _teleportPersistenceFinal = CaptureTeleportPersistence();
            foreach (int tick in SaveTeleportPersistence()) yield return tick;
        }
    }
}
