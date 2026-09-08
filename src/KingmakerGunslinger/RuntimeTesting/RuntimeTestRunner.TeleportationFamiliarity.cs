using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.State;
using Kingmaker.UI.GlobalMap;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void PollTeleportationFamiliarity()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationFamiliarity ||
                !_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Familiarity fixture requires the guarded working save, automatic exit and intact save-write sentinels.");
            if (_teleportationMapLoad == null)
            {
                _teleportationMapLoad = Stopwatch.StartNew();
                Game.Instance.LoadArea(Game.Instance.BlueprintRoot.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None);
                return;
            }
            if (_teleportationMapLoad.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Familiarity fixture world-map load timed out.");
            if (LoadingProcess.Instance.IsLoadingInProcess || GlobalMapRules.Instance == null || Game.Instance.CurrentMode != GameModeType.GlobalMap) return;
            var panels = Resources.FindObjectsOfTypeAll<GlobalMapMessageBox>().Where(value => value != null &&
                value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded && value.transform.root.gameObject.activeInHierarchy).ToArray();
            if (panels.Length == 0) return;
            if (panels.Length != 1) throw new InvalidOperationException("Native destination panel is ambiguous.");
            Complete(RunTeleportationFamiliarity(panels[0]));
        }

        private RuntimeTestResult RunTeleportationFamiliarity(GlobalMapMessageBox panel)
        {
            GlobalMapRules rules = GlobalMapRules.Instance;
            GlobalMapState map = GlobalMapRules.State;
            UnitPartTeleportFamiliarity ledger = TeleportFamiliarityRuntime.EnsureLedger(Game.Instance.Player);
            if (ledger == null || !TeleportFamiliarityPatches.Installed || map.TravelData != null || map.CurrentEncounterData != null)
                throw new InvalidOperationException("Familiarity hooks, ledger or stationary map prerequisites failed.");
            string originalLedger = ledger.Read().Serialize();
            FieldInfo payloadField = typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
            object originalPayload = payloadField.GetValue(ledger);
            var originalPosition = map.PartyPosition;
            var originalLastLocation = map.LastLocation;
            var originalTime = Game.Instance.Player.GameTime;
            float originalDelta = Game.Instance.TimeController.DeltaTime;
            float originalMiles = map.MilesTravelled;
            var originalHistory = map.HistoryTravels.ToArray();
            bool originalStop = rules.StopWhenRevealingNewEdges;
            string[] party = Game.Instance.Player.Party.Select(value => value.UniqueId).ToArray();
            var pointSnapshots = rules.AllLocations.Where(value => value != null).Select(value => new TeleportNativeFieldSnapshot(value.Data)).ToArray();
            var edgeSnapshots = rules.AllEdges.Where(value => value != null).Select(value => new TeleportNativeFieldSnapshot(value.Data)).ToArray();
            var assertions = new List<RuntimeTestAssertion>();
            var legs = new List<object>();
            var routeProbes = new List<object>();
            string path = Path.Combine(_request.EvidenceDirectory, "teleportation-familiarity.json");
            Exception failure = null;
            bool restored = false;
            try
            {
                var candidates = rules.AllLocations.Where(TeleportFamiliarityFixturePoint).OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal);
                GlobalMapLocation first = null, middle = null, last = null;
                foreach (GlobalMapLocation junction in candidates)
                {
                    var edges = junction.Edges.Where(value => value != null && !value.IsLocked && value.Spline != null &&
                        value.Spline.WorldLength > 0 && TeleportFamiliarityFixturePoint(value.GetOppositeLocation(junction)))
                        .OrderBy(value => value.Spline.WorldLength).ThenBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal).ToArray();
                    if (edges.Length < 2 || edges[0].GetOppositeLocation(junction) == edges[1].GetOppositeLocation(junction)) continue;
                    first = edges[0].GetOppositeLocation(junction); middle = junction; last = edges[1].GetOppositeLocation(junction);
                    foreach (GlobalMapEdge edge in edges.Take(2)) edge.Data.UpdateExplored(1, 1);
                    break;
                }
                if (first == null) throw new InvalidOperationException("No safe ordinary crossroads chain in the native scene.");
                MethodInfo revealSetter = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
                foreach (GlobalMapLocation point in new[] { first, middle, last })
                {
                    revealSetter.Invoke(point.Data, new object[] { true });
                    point.Data.EdgesOpened = true;
                }
                rules.StopWhenRevealingNewEdges = false;
                rules.SetCurrentPosition(new MapPosition(first.Blueprint));
                rules.UpdatePawnPosition();
                assertions.Add(Assertion("familiarity-fixture-setup-no-arrival", "ledger unchanged by fixture reveal and native placement",
                    ledger.Read().Serialize(), ledger.Read().Serialize() == originalLedger, path));
                panel.OnLocationSelect(last.Blueprint, true);
                panel.Hide();
                panel.OnLocationSelect(last.Blueprint, true);
                panel.Hide();
                assertions.Add(Assertion("familiarity-selection-cancel-no-arrival", "repeated native selection and dismissal change no counts",
                    ledger.Read().Serialize(), ledger.Read().Serialize() == originalLedger && map.TravelData == null, path));
                foreach (GlobalMapLocation target in new[] { last, first, last })
                {
                    var beforeState = ledger.Read();
                    string originId = map.PartyLocation.AssetGuid;
                    MapTravelData preview = rules.CalculatePathToLocation(target);
                    panel.OnLocationSelect(target.Blueprint, true);
                    panel.Accept();
                    MapTravelData travel = map.TravelData;
                    routeProbes.Add(new { originId, targetId = target.Blueprint.AssetGuid,
                        actualPointAfterAccept = map.PartyLocation == null ? null : map.PartyLocation.AssetGuid,
                        preview = DescribeFamiliarityRoute(preview), actual = DescribeFamiliarityRoute(travel) });
                    if (travel == null || !travel.Walking || travel.Path.Count != 2 || travel.Path.Any(value => !rules.GetEdgeObject(value.Blueprint).Data.Revealed))
                        throw new InvalidOperationException("Expected native two-edge ordinary route over already revealed edges; see routeProbes.");
                    var crossed = travel.Path.Select(value => value.Direction > 0 ? rules.GetEdgeObject(value.Blueprint).Location2 :
                        rules.GetEdgeObject(value.Blueprint).Location1).ToArray();
                    if (crossed[0] != middle || crossed[1] != target) throw new InvalidOperationException("Native route differs from the qualified chain.");
                    float speed = Game.Instance.BlueprintRoot.GlobalMap.VisualSpeedBase * (float)typeof(MapMovementController)
                        .GetMethod("CalcSpeedModifiers", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                    if (speed <= 0 || float.IsNaN(speed) || float.IsInfinity(speed)) throw new InvalidOperationException("Native movement speed is unusable.");
                    float length = travel.Path.Sum(value => rules.GetEdgeObject(value.Blueprint).Spline.WorldLength);
                    // Request-local deterministic native time input, restored in finally.
                    // No ledger method is invoked to manufacture the tested arrivals.
                    Game.Instance.TimeController.SetDeltaTime((length + 0.01f) / speed);
                    new MapMovementController().Tick();
                    var afterState = ledger.Read();
                    bool counts = crossed.All(value => afterState.Count(value.Blueprint.AssetGuid) == beforeState.Count(value.Blueprint.AssetGuid) + 1) &&
                        beforeState.Count(originId) == afterState.Count(originId);
                    legs.Add(new { originId, targetId = target.Blueprint.AssetGuid, nativeEdges = travel.Path.Select(value => new {
                        edgeId = value.Blueprint.AssetGuid, value.Direction, value.RevealPath }).ToArray(),
                        before = beforeState.Serialize(), after = afterState.Serialize(), travel.WalkedDistance,
                        actualPoint = map.PartyLocation == null ? null : map.PartyLocation.AssetGuid });
                    assertions.Add(Assertion("familiarity-native-ordinary-leg-" + legs.Count, "intermediate and destination +1; departure unchanged",
                        afterState.Serialize(), counts && map.PartyLocation == target.Blueprint && map.TravelData == null, path));
                }
                string afterTravel = ledger.Read().Serialize();
                ledger.PreSave();
                ledger.PostLoad();
                var settings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), TypeNameHandling = TypeNameHandling.None };
                var serializationCarrier = new UnitPartTeleportFamiliarity();
                payloadField.SetValue(serializationCarrier, payloadField.GetValue(ledger));
                string serialized = JsonConvert.SerializeObject(serializationCarrier, settings);
                var roundTrip = JsonConvert.DeserializeObject<UnitPartTeleportFamiliarity>(serialized, settings);
                assertions.Add(Assertion("familiarity-unitpart-serialization", "ownerless UnitPart payload round trip and live lifecycle callbacks preserve counts",
                    serialized, roundTrip != null && roundTrip.Read().Serialize() == afterTravel && ledger.Read().Serialize() == afterTravel, path));
                TeleportFamiliarityRuntime.EnsureLedger(Game.Instance.Player);
                assertions.Add(Assertion("familiarity-migration-idempotent", "repeated ledger resolution does not remigrate native visited flags",
                    ledger.Read().Serialize(), ledger.Read().Serialize() == afterTravel, path));
                rules.SetCurrentPosition(new MapPosition(first.Blueprint));
                rules.UpdatePawnPosition();
                assertions.Add(Assertion("familiarity-native-relocation-no-arrival", "canonical placement alone does not count an ordinary arrival",
                    ledger.Read().Serialize(), ledger.Read().Serialize() == afterTravel, path));
            }
            catch (Exception exception) { failure = exception; }
            finally
            {
                panel.Hide();
                map.TravelData = null;
                payloadField.SetValue(ledger, originalPayload);
                foreach (TeleportNativeFieldSnapshot snapshot in pointSnapshots.Concat(edgeSnapshots)) snapshot.Restore();
                map.HistoryTravels.Clear(); map.HistoryTravels.AddRange(originalHistory);
                rules.StopWhenRevealingNewEdges = originalStop;
                rules.SetCurrentPosition(originalPosition); rules.UpdatePawnPosition(); map.LastLocation = originalLastLocation;
                map.MilesTravelled = originalMiles;
                Game.Instance.TimeController.SetDeltaTime(originalDelta);
                Game.Instance.Player.GameTime = originalTime;
                restored = Equals(payloadField.GetValue(ledger), originalPayload) &&
                    pointSnapshots.Concat(edgeSnapshots).All(value => value.Matches()) &&
                    party.SequenceEqual(Game.Instance.Player.Party.Select(value => value.UniqueId)) &&
                    map.HistoryTravels.SequenceEqual(originalHistory) && map.CurrentEncounterData == null &&
                    !_workingSaveSmoke.WriteObserved;
            }
            assertions.Add(Assertion("familiarity-fixture-cleanup", "fixture ledger/map fields, time input, history and party restored; no save write",
                "restored=" + restored + ";saveWriteObserved=" + _workingSaveSmoke.WriteObserved, restored, path));
            WriteTeleportationForensicJson(path, new { schemaVersion = 1, runId = _request.RunId,
                claims = "Native contextual normal travel and movement-hook evidence with request-local time input. Ownerless UnitPart payload serialization, not a campaign owner-graph or disk save/reload qualification. No magical cast qualification.",
                originalLedger, legs, routeProbes, restored, saveWriteObserved = _workingSaveSmoke.WriteObserved,
                error = failure == null ? null : failure.ToString(), assertions });
            return CreateResult(failure != null ? RuntimeTestStatuses.Error : assertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, failure == null ? null : failure.ToString());
        }

        private static object DescribeFamiliarityRoute(MapTravelData route)
        {
            return route == null ? null : new { route.Walking, route.Finished, route.WalkedDistance,
                from = route.From == null || route.From.Location == null ? null : route.From.Location.AssetGuid,
                to = route.To == null || route.To.Location == null ? null : route.To.Location.AssetGuid,
                edges = route.Path.Select(value => new { id = value.Blueprint.AssetGuid, value.Direction, value.RevealPath,
                    revealed = GlobalMapRules.Instance.GetEdgeObject(value.Blueprint).Data.Revealed }).ToArray() };
        }

        private static bool TeleportFamiliarityFixturePoint(GlobalMapLocation point)
        {
            return point != null && point.Blueprint != null && point.gameObject.activeInHierarchy &&
                point.Blueprint.Type == LocationType.Waypoint && !point.Data.IsClosed &&
                point.Blueprint.ComponentsArray.Length == 0 && point.Data.AreaEntrance == null && point.Data.BookEvent == null &&
                !point.Blueprint.HasKingdomResource && TeleportDestinationPolicy.IsStableId(point.Blueprint.AssetGuid);
        }

        private sealed class TeleportNativeFieldSnapshot
        {
            private readonly object _target;
            private readonly KeyValuePair<FieldInfo, object>[] _fields;
            internal TeleportNativeFieldSnapshot(object target)
            {
                _target = target;
                _fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(value => !value.IsInitOnly).Select(value => new KeyValuePair<FieldInfo, object>(value, value.GetValue(target))).ToArray();
            }
            internal void Restore() { foreach (var pair in _fields) pair.Key.SetValue(_target, pair.Value); }
            internal bool Matches() { return _fields.All(pair => Equals(pair.Key.GetValue(_target), pair.Value)); }
        }
    }
}
