using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.UI.GlobalMap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private Stopwatch _teleportationMapLoad;
        private string _teleportationProbeOriginalArea;
        private string[] _teleportationProbeOriginalParty;

        // An explicitly guarded, no-autosave scene fixture. No travel spell, resource,
        // relocation, arrival counter or gameplay-qualification claim is involved.
        private void PollTeleportationWorldMapForensics()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.ObserveTeleportationWorldMap ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete)
                throw new InvalidOperationException("World-map observation requires the verified guarded working-save load.");
            if (_workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Save-write sentinel observed an unexpected operation.");
            if (_teleportationMapLoad == null)
            {
                _teleportationProbeOriginalArea = Game.Instance.CurrentlyLoadedArea.AssetGuid;
                _teleportationProbeOriginalParty = Game.Instance.Player.Party.Select(unit => unit.UniqueId).ToArray();
                var entry = Game.Instance.BlueprintRoot.GlobalMap.GlobalMapEnterPoint;
                if (entry == null) throw new InvalidOperationException("Native global-map entry is missing.");
                _teleportationMapLoad = Stopwatch.StartNew();
                WriteLifecycleStage("teleportation-native-map-fixture-loading");
                Game.Instance.LoadArea(entry, AutoSaveMode.None);
                return;
            }
            if (_teleportationMapLoad.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Native global-map scene fixture did not become ready.");
            if (LoadingProcess.Instance.IsLoadingInProcess || GlobalMapRules.Instance == null) return;
            GlobalMapRules rules = GlobalMapRules.Instance;
            var panels = Resources.FindObjectsOfTypeAll<GlobalMapMessageBox>().Where(value =>
                value != null && value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded &&
                value.transform.root.gameObject.activeInHierarchy).ToArray();
            if (panels.Length == 0) return;
            if (panels.Length != 1) throw new InvalidOperationException("Expected one native desktop destination panel; found " + panels.Length + ".");
            var points = rules.AllLocations.Where(value => value != null && value.Blueprint != null)
                .OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal).ToArray();
            string inventoryPath = Path.Combine(_request.EvidenceDirectory, "teleportation-scene-points.json");
            WriteTeleportationForensicJson(inventoryPath, new { runId = _request.RunId,
                currentMode = Game.Instance.CurrentMode.ToString(),
                origin = GlobalMapRules.State.PartyLocation == null ? null : GlobalMapRules.State.PartyLocation.AssetGuid,
                points = points.Select(DescribeTeleportationNativePoint).ToArray() });
            GlobalMapLocation selected = points.FirstOrDefault(value => value.gameObject.activeInHierarchy && value.LocationTooltipPoint != null &&
                value.Data.IsRevealed && value.Blueprint != GlobalMapRules.State.PartyLocation &&
                (value.Data.EdgesOpened || value.Data.IsExplored));
            if (selected == null) selected = points.FirstOrDefault(value => value.gameObject.activeInHierarchy && value.LocationTooltipPoint != null &&
                value.Data.IsRevealed && value.Blueprint != GlobalMapRules.State.PartyLocation);
            if (selected == null) selected = points.SingleOrDefault(value =>
                value.Blueprint.AssetGuid == Spells.Teleportation.WordOfRecallDestinationPolicy.OlegId &&
                value.gameObject.activeInHierarchy && value.LocationTooltipPoint != null &&
                value.Blueprint != GlobalMapRules.State.PartyLocation);
            if (selected == null) throw new InvalidOperationException("No native non-origin panel fixture anchor; inspect " + inventoryPath);
            MethodInfo setRevealed = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
            if (setRevealed == null) throw new InvalidOperationException("Exact native revealed-state setter is unavailable.");
            GlobalMapMessageBox panel = panels[0];
            bool edgesOpened = selected.Data.EdgesOpened;
            bool explored = selected.Data.IsExplored;
            bool revealed = selected.Data.IsRevealed;
            object travelBefore = GlobalMapRules.State.TravelData;
            string origin = GlobalMapRules.State.PartyLocation == null ? null : GlobalMapRules.State.PartyLocation.AssetGuid;
            TimeSpan before = Game.Instance.Player.GameTime;
            var assertions = new List<RuntimeTestAssertion>();
            string path = Path.Combine(_request.EvidenceDirectory, "teleportation-world-map-forensics.json");
            try
            {
                // Request-local native visited fixture: restored in finally, never saved.
                // Native Reveal() runs campaign triggers. The proven private setter changes
                // only this request-local flag; using it avoids executing reveal scripts.
                setRevealed.Invoke(selected.Data, new object[] { true });
                selected.Data.EdgesOpened = true;
                selected.Data.IsExplored = true;
                panel.OnLocationSelect(selected.Blueprint, true);
                var payload = new {
                    schemaVersion = 1, runId = _request.RunId,
                    claims = "Native panel/anchor forensics only; no contextual spell, slot spend, or relocation qualification.",
                    originalArea = _teleportationProbeOriginalArea,
                    currentArea = Game.Instance.CurrentlyLoadedArea.AssetGuid,
                    currentMode = Game.Instance.CurrentMode.ToString(),
                    origin, selectedId = selected.Blueprint.AssetGuid,
                    fixtureVisitedFlags = new { originalEdgesOpened = edgesOpened, originalExplored = explored, originalRevealed = revealed },
                    points = points.Select(DescribeTeleportationNativePoint).ToArray(),
                    panel = DescribeTeleportationNativePanel(panel),
                    nativeSelectionPreservedTravel = ReferenceEquals(travelBefore, GlobalMapRules.State.TravelData),
                    nativeSelectionPreservedTime = before == Game.Instance.Player.GameTime,
                    saveWriteObserved = _workingSaveSmoke.WriteObserved
                };
                WriteTeleportationForensicJson(path, payload);
                var persisted = JObject.Parse(File.ReadAllText(path));
                assertions.Add(Assertion("teleportation-map-inventory-persisted", "all scene point rows", "count=" + points.Length,
                    ((JArray)persisted["points"]).Count == points.Length && points.Length > 0, path));
                var selectedField = typeof(GlobalMapMessageBox).GetField("m_Location", BindingFlags.Instance | BindingFlags.NonPublic);
                assertions.Add(Assertion("teleportation-native-panel-selection", "exact selected native point and visible panel",
                    "selected=" + selected.Blueprint.AssetGuid + ";visible=" + panel.gameObject.activeInHierarchy,
                    selectedField != null && ReferenceEquals(selectedField.GetValue(panel), selected) && panel.gameObject.activeInHierarchy, path));
                assertions.Add(Assertion("teleportation-native-selection-no-travel", "same origin/time/travel command", "origin=" + origin,
                    ReferenceEquals(travelBefore, GlobalMapRules.State.TravelData) && before == Game.Instance.Player.GameTime &&
                    origin == (GlobalMapRules.State.PartyLocation == null ? null : GlobalMapRules.State.PartyLocation.AssetGuid), path));
                assertions.Add(Assertion("teleportation-map-fixture-party-preserved", "same canonical party IDs", "party=" + Game.Instance.Player.Party.Count,
                    _teleportationProbeOriginalParty.SequenceEqual(Game.Instance.Player.Party.Select(unit => unit.UniqueId)), path));
            }
            finally
            {
                selected.Data.EdgesOpened = edgesOpened;
                selected.Data.IsExplored = explored;
                setRevealed.Invoke(selected.Data, new object[] { revealed });
                panel.Hide();
            }
            assertions.Add(Assertion("teleportation-map-fixture-cleanup", "visited fixture restored; no save writes; native panel closed",
                "saveWriteObserved=" + _workingSaveSmoke.WriteObserved,
                selected.Data.EdgesOpened == edgesOpened && selected.Data.IsExplored == explored && selected.Data.IsRevealed == revealed &&
                !_workingSaveSmoke.WriteObserved && !panel.gameObject.activeSelf,
                "Scene fixture is discarded on guarded process exit; no campaign save is written."));
            Complete(CreateResult(assertions.All(value => value.Status == "PASS") ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null));
        }

        private static void WriteTeleportationForensicJson(string path, object payload)
        {
            string json = JsonConvert.SerializeObject(payload, Formatting.Indented, new JsonSerializerSettings {
                ContractResolver = new DefaultContractResolver(), PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.None });
            RuntimeTestResultWriter.WriteAtomic(path, json);
        }

        private static object DescribeTeleportationNativePoint(GlobalMapLocation point)
        {
            return new {
                id = point.Blueprint.AssetGuid, type = point.Blueprint.Type.ToString(),
                active = point.gameObject.activeInHierarchy, revealed = point.Data.IsRevealed,
                point.Data.IsExplored, point.Data.EdgesOpened, point.Data.IsClosed, point.Data.IsFake,
                tooltipAnchor = point.LocationTooltipPoint != null,
                placementAnchor = point.transform != null,
                anchorX = point.transform.position.x, anchorY = point.transform.position.y, anchorZ = point.transform.position.z,
                visualAnchor = point.LocationVisualPostion != null,
                areaEntry = point.Data.AreaEntrance == null ? null : point.Data.AreaEntrance.AssetGuid,
                edges = point.Edges == null ? -1 : point.Edges.Count
            };
        }

        private static object DescribeTeleportationNativePanel(GlobalMapMessageBox panel)
        {
            return new {
                fields = typeof(GlobalMapMessageBox).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(field => typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)).Select(field => new {
                        field = field.Name, type = field.FieldType.FullName,
                        objectName = field.GetValue(panel) == null ? null : ((UnityEngine.Object)field.GetValue(panel)).name
                    }).ToArray(),
                hierarchy = panel.GetComponentsInChildren<Transform>(true).Select(node => {
                    RectTransform rect = node as RectTransform;
                    Button button = node.GetComponent<Button>();
                    var text = node.GetComponent<TMPro.TextMeshProUGUI>();
                    return new {
                        path = TeleportationNativeNodePath(node, panel.transform), active = node.gameObject.activeSelf,
                        components = node.GetComponents<Component>().Where(value => value != null).Select(value => value.GetType().FullName).ToArray(),
                        rect = rect == null ? null : new { width = rect.rect.width, height = rect.rect.height,
                            anchorMinX = rect.anchorMin.x, anchorMinY = rect.anchorMin.y, anchorMaxX = rect.anchorMax.x, anchorMaxY = rect.anchorMax.y,
                            positionX = rect.anchoredPosition.x, positionY = rect.anchoredPosition.y,
                            pivotX = rect.pivot.x, pivotY = rect.pivot.y, sizeX = rect.sizeDelta.x, sizeY = rect.sizeDelta.y },
                        text = text == null ? null : text.text,
                        button = button == null ? null : new { button.interactable,
                            listeners = Enumerable.Range(0, button.onClick.GetPersistentEventCount())
                                .Select(index => button.onClick.GetPersistentMethodName(index)).ToArray() }
                    };
                }).ToArray()
            };
        }
        private static string TeleportationNativeNodePath(Transform node, Transform root)
        {
            var names = new List<string>();
            for (Transform current = node; current != null; current = current.parent)
            { names.Add(current.name); if (current == root) break; }
            names.Reverse(); return string.Join("/", names);
        }
    }
}