using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.State;
using Kingmaker.UI;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool IsTeleportationDestinationsFixture { get { return _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationDestinations; } }
        private readonly List<object> _teleportationDestinationExceptions = new List<object>();
        private void ObserveTeleportDestinationException(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error && message.Contains("Exception"))
                _teleportationDestinationExceptions.Add(new { message, stack, type = type.ToString(), frame = Time.frameCount });
        }
        private IEnumerable<int> RunTeleportationDestinations(GlobalMapLocation origin, List<TeleportResourceFixtureOwner> owners,
            TeleportInteractionMovementObserver movement)
        {
            if (!IsTeleportationDestinationsFixture || !_request.ExitAfterCompletion || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Destination audit requires its exact guarded disposable request and intact save sentinels.");
            var player = Game.Instance.Player; var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State;
            var panel = TeleportationFixturePanel();
            var ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
            var payload = typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
            if (TeleportationSpellbookAdapter.Enumerate(player).Any())
                throw new InvalidOperationException("The no-source baseline requires no available original project spell source.");
            var context = TeleportationWorldMapAdapter.Capture(false);
            var points = rules.AllLocations.OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal).ToArray();
            CaptureTeleportInteraction("native-point-inventory", points.Select(point => {
                var snapshot = TeleportationWorldMapAdapter.ReadDestination(context, point.Blueprint);
                var decision = TeleportDestinationPolicy.Evaluate(snapshot, origin.Blueprint.AssetGuid, TeleportationWorldMapAdapter.Forbidden);
                return new { id = point.Blueprint.AssetGuid, nativeType = point.Blueprint.Type.ToString(),
                    components = point.Blueprint.ComponentsArray.Select(value => value.GetType().FullName).ToArray(),
                    hasBookEvent = point.Blueprint.BookEvent != null, hasArea = point.Blueprint.AreaEntrance != null,
                    eligible = decision.Eligible, reason = decision.Reason.ToString(), diagnostic = decision.Diagnostic };
            }).ToArray());
            // Audit membership uses exact native structure. Display labels and
            // asset names do not decide eligibility, arrival, or catalog entries.
            var audit = points.Where(value => value.Blueprint.BookEvent != null || value.Blueprint.ComponentsArray.Length > 0 ||
                value.Blueprint.AssetGuid == WordOfRecallDestinationPolicy.CapitalId || value.Blueprint.AssetGuid == WordOfRecallDestinationPolicy.OlegId)
                .Concat(points.GroupBy(value => value.Blueprint.Type).Select(group => group.FirstOrDefault(value =>
                    value != origin && value.isActiveAndEnabled && value.gameObject.activeInHierarchy && !value.Data.IsClosed &&
                    value.Blueprint.GetComponents<LocationRestriction>().All(restriction => !restriction.IsRestricted()))).Where(value => value != null))
                .Where(value => value != origin).Distinct().OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal).ToArray();
            var reveal = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
            var familiarity = new TeleportFamiliarityState();
            familiarity.MigrateLegacy(audit.Select(value => value.Blueprint.AssetGuid).Concat(new[] { origin.Blueprint.AssetGuid }));
            foreach (var point in audit) { reveal.Invoke(point.Data, new object[] { true }); point.Data.EdgesOpened = true; }
            payload.SetValue(ledger, familiarity.Serialize());
            var nativeActions = new Dictionary<string, string>(StringComparer.Ordinal);
            var allowed = new List<GlobalMapLocation>();
            int rejected = 0;
            foreach (var point in audit)
            {
                context = TeleportationWorldMapAdapter.Capture(false);
                var snapshot = TeleportationWorldMapAdapter.ReadDestination(context, point.Blueprint);
                var decision = TeleportDestinationPolicy.Evaluate(snapshot, origin.Blueprint.AssetGuid, TeleportationWorldMapAdapter.Forbidden);
                CaptureTeleportInteraction("audit-eligibility-" + point.Blueprint.AssetGuid, new { id = point.Blueprint.AssetGuid,
                    type = point.Blueprint.Type.ToString(), eligible = decision.Eligible, reason = decision.Reason.ToString(),
                    diagnostic = decision.Diagnostic, nativeClosed = point.Data.IsClosed,
                    nativeRestricted = point.Blueprint.GetComponents<LocationRestriction>().Select(value => value.IsRestricted()).ToArray() });
                if (!decision.Eligible) { rejected++; continue; }
                allowed.Add(point);
                SelectTeleportationCastingPoint(panel, point);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                nativeActions.Add(point.Blueprint.AssetGuid, TeleportationNativeButtons(panel));
                TeleportInteractionAssert("native-no-source-" + point.Blueprint.AssetGuid,
                    "special destination retains its actual native controls without any spell UI",
                    "rows=" + panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length,
                    panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 0 && map.TravelData == null &&
                    map.PartyLocation == origin.Blueprint && !DialogMessageBox.Instance.IsShown && ledger.Read().Serialize() == familiarity.Serialize());
                panel.Hide();
            }
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "b3a505fb61437dc4097f43c3f8f9a4cf", "native destination audit Sorcerer");
            var owner = player.Party.FirstOrDefault(value => TeleportationSpellbookAdapter.CasterAvailable(value) && value.Descriptor.GetSpellbook(sorcerer.Spellbook) == null);
            if (owner == null) throw new InvalidOperationException("No available existing owner for the real destination audit spellbook.");
            var fixture = new TeleportResourceFixtureOwner(owner); owners.Add(fixture);
            var book = fixture.AddBook(sorcerer.Spellbook);
            book.AddKnown(7, BlueprintBootstrap.Teleportation.GreaterTeleport, true); book.Rest();
            foreach (var point in audit.Except(allowed))
                TeleportInteractionAssert("native-prohibition-" + point.Blueprint.AssetGuid,
                    "real spell resources do not promote a natively ineligible point",
                    "rows=" + TeleportationWorldMapAdapter.Compose(TeleportationWorldMapAdapter.Capture(false), point.Blueprint).Count,
                    TeleportationWorldMapAdapter.Compose(TeleportationWorldMapAdapter.Capture(false), point.Blueprint).Count == 0);
            foreach (var point in allowed.OrderBy(value => value.Blueprint.Type == LocationType.SystemWaypoint ? 1 : 0))
            {
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                book.Rest();
                int castStarts = movement.Starts, castStops = movement.Stops;
                var before = new TeleportationWorldSnapshot(TeleportationWorldMapAdapter.Capture(false));
                string resourcesBefore = TeleportResourceFingerprint(book);
                SelectTeleportationCastingPoint(panel, point);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                var rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                bool nativePreserved = nativeActions[point.Blueprint.AssetGuid] == TeleportationNativeButtons(panel);
                if (rows == null || rows.Actions.Count != 1 || rows.Actions[0].Source.Spell != TeleportSpellKind.GreaterTeleport ||
                    rows.Actions[0].Source.CasterId != owner.UniqueId || rows.Actions[0].Source.BookId != book.Blueprint.AssetGuid)
                    throw new InvalidOperationException("The special point lacks its single current real contextual source: " + point.Blueprint.AssetGuid);
                var dice = new TeleportationFixtureRolls(new int[0]); rows.QualificationRolls = dice;
                rows.Buttons[0].onClick.Invoke();
                var request = TeleportContextConfirmationPresenter.Current;
                if (request == null || !DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("Native destination action did not open its owned spell confirmation.");
                for (int frame = 0; frame < 8; frame++) yield return 0;
                if (resourcesBefore != TeleportResourceFingerprint(book)) throw new InvalidOperationException("Opening confirmation spent a resource.");
                var atCommit = new TeleportationWorldSnapshot(TeleportationWorldMapAdapter.Capture(false));
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var deferred = new TeleportationWorldSnapshot(TeleportationWorldMapAdapter.Capture(false));
                CaptureTeleportInteraction("deferred-differences-" + point.Blueprint.AssetGuid, new {
                    duringSelection = TeleportDestinationWorldDifferences(before.State, atCommit.State),
                    afterCommit = TeleportDestinationWorldDifferences(atCommit.State, deferred.State)
                });
                bool committed = request.Transaction.State == TeleportTransactionState.Completed && request.Transaction.Result != null &&
                    request.Transaction.Result.Status == TeleportExecutionStatus.Arrived && request.Transaction.Result.DestinationId == point.Blueprint.AssetGuid;
                if (committed) before.Verify(point.Blueprint.AssetGuid);
                CaptureTeleportInteraction("special-point-cast-" + point.Blueprint.AssetGuid, new { id = point.Blueprint.AssetGuid,
                    type = point.Blueprint.Type.ToString(), hasBookEvent = point.Blueprint.BookEvent != null, nativePreserved,
                    transaction = request.Transaction.State.ToString(), request.Transaction.Diagnostic, result = request.Execution.LastEvidence,
                    deferredProtectedStateUnchanged = committed, movement.Starts, movement.Stops });
                TeleportInteractionAssert("contextual-arrival-" + point.Blueprint.AssetGuid,
                    "native controls retained; actual confirmation spends one real slot; exact dot arrival, exactly one native pawn-notification pair and no other movement hold across frames",
                    "transaction=" + request.Transaction.State + ";nativePreserved=" + nativePreserved,
                    committed && nativePreserved && request.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne &&
                    dice.D100Count == 0 && dice.D10Count == 0 && movement.Starts == castStarts + 1 && movement.Stops == castStops + 1 &&
                    !TeleportContextConfirmationPresenter.Pending && !DialogMessageBox.Instance.IsShown);
                if (!committed) throw new InvalidOperationException("Special point cast failed: " + point.Blueprint.AssetGuid + ";" + request.Transaction.Diagnostic);
            }
            // Actual native Travel releases exploration without spending a spell.
            // The final SystemWaypoint is adjacent to a native hidden point that
            // reproduced the deferred perception roll in the rejected probe.
            var arrivalBoundary = ledger.ReadExplorationBoundary();
            var hidden = rules.GetLocationObject(BlueprintLibraryLookup.RequireExact<BlueprintLocation>(BlueprintBootstrap.Library,
                "312bf36ac8bc4c74cb0969908c876cce", "native deferred exploration control"));
            if (arrivalBoundary == null || hidden == null || hidden.Data.IsRevealed || hidden.Data.LastPerceptionRolled != 0)
                throw new InvalidOperationException("The native deferred exploration positive control lost its unvisited/unchecked state.");
            string beforeTravelSlots = TeleportResourceFingerprint(book);
            int travelStartsBase = movement.Starts;
            var arrival = rules.GetLocationObject(map.PartyLocation);
            var ordinaryEdge = arrival.Edges.Where(value => value != null && !value.IsLocked && value.Spline != null &&
                value.Spline.WorldLength > 0 && value.GetOppositeLocation(arrival) != hidden &&
                TeleportFamiliarityFixturePoint(value.GetOppositeLocation(arrival)))
                .OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
            if (ordinaryEdge == null) throw new InvalidOperationException("No safe native adjacent ordinary-travel control.");
            var ordinaryTarget = ordinaryEdge.GetOppositeLocation(arrival);
            // Request-local positive-control setup after every cast snapshot has
            // passed. Opening this one edge is fixture state, never a spell effect.
            ordinaryEdge.Data.UpdateExplored(1, 1);
            reveal.Invoke(ordinaryTarget.Data, new object[] { true }); ordinaryTarget.Data.EdgesOpened = true;
            var ordinaryPreview = rules.CalculatePathToLocation(ordinaryTarget);
            CaptureTeleportInteraction("ordinary-travel-fixture", new { originId = arrival.Blueprint.AssetGuid,
                targetId = ordinaryTarget.Blueprint.AssetGuid, edgeId = ordinaryEdge.Blueprint.AssetGuid,
                preview = DescribeFamiliarityRoute(ordinaryPreview) });
            if (ordinaryPreview == null || ordinaryPreview.Path.Count != 1 || ordinaryPreview.Path[0].Blueprint != ordinaryEdge.Blueprint)
                throw new InvalidOperationException("The native ordinary control must use its one revealed edge.");
            SelectTeleportationCastingPoint(panel, ordinaryTarget);
            foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
            panel.Accept();
            if (map.TravelData == null || !map.TravelData.Walking) throw new InvalidOperationException("Native ordinary Travel did not begin.");
            new Kingmaker.Controllers.GlobalMap.LocationRevealController().Tick();
            TeleportInteractionAssert("ordinary-travel-releases-exploration", "native Travel resumes the real exploration tick with no spell expenditure",
                "boundaryCleared=" + (ledger.ReadExplorationBoundary() == null) + ";nativePerception=" + hidden.Data.LastPerceptionRolled,
                ledger.ReadExplorationBoundary() == null && hidden.Data.LastPerceptionRolled > 0 &&
                beforeTravelSlots == TeleportResourceFingerprint(book) && movement.Starts == travelStartsBase + 1);
            CaptureTeleportInteraction("ordinary-exploration-control", new { arrivalBoundary = arrivalBoundary.Serialize(),
                targetId = hidden.Blueprint.AssetGuid, hidden.Data.LastPerceptionRolled, hidden.Data.IsRevealed, movement.Starts });
            rules.OnBreak();
            if (map.TravelData == null || map.TravelData.Walking || map.TravelData.WalkedDistance != 0)
                throw new InvalidOperationException("The first exploration control must stop before any ordinary movement.");
            // Native Stop retains the paused command and clears PartyPosition at
            // travel start. Reset this completed control before selecting again.
            map.TravelData = null;
            rules.SetCurrentPosition(new MapPosition(arrival.Blueprint)); rules.UpdatePawnPosition();
            SelectTeleportationCastingPoint(panel, ordinaryTarget);
            foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
            // The fault is request-local and injected only after the UI settles;
            // no stationary game tick runs before native Travel starts below.
            typeof(UnitPartTeleportFamiliarity).GetField("_explorationBoundary", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ledger, "corrupt-request-local-boundary");
            reveal.Invoke(hidden.Data, new object[] { false }); hidden.Data.LastPerceptionRolled = 0;
            bool invalidCastAbsent = TeleportationWorldMapAdapter.Compose(TeleportationWorldMapAdapter.Capture(false), ordinaryTarget.Blueprint).Count == 0;
            panel.Accept();
            if (map.TravelData == null || !map.TravelData.Walking) throw new InvalidOperationException("Native recovery Travel did not begin.");
            new Kingmaker.Controllers.GlobalMap.LocationRevealController().Tick();
            TeleportInteractionAssert("ordinary-travel-recovers-corrupt-boundary", "invalid spell state hides casting and cannot permanently block native exploration",
                "invalidCastAbsent=" + invalidCastAbsent + ";nativePerception=" + hidden.Data.LastPerceptionRolled,
                invalidCastAbsent && ledger.ReadExplorationBoundary() == null && hidden.Data.LastPerceptionRolled > 0 &&
                beforeTravelSlots == TeleportResourceFingerprint(book) && movement.Starts == travelStartsBase + 2);
            CaptureTeleportInteraction("corrupt-boundary-ordinary-control", new { invalidCastAbsent,
                hidden.Data.LastPerceptionRolled, movement.Starts, exactResources = beforeTravelSlots == TeleportResourceFingerprint(book) });
            rules.OnBreak();
            CaptureTeleportInteraction("audit-coverage", new { inventoryCount = points.Length, selectedCount = audit.Length,
                castCount = allowed.Count, rejectedCount = rejected,
                typesCast = allowed.Select(value => value.Blueprint.Type.ToString()).Distinct().OrderBy(value => value).ToArray(),
                bookEventsCast = allowed.Count(value => value.Blueprint.BookEvent != null),
                componentPointsCast = allowed.Count(value => value.Blueprint.ComponentsArray.Length > 0) });
            TeleportInteractionAssert("point-type-coverage", "all five stable native point types receive actual contextual casts",
                "types=" + string.Join(",", allowed.Select(value => value.Blueprint.Type).Distinct()),
                allowed.Select(value => value.Blueprint.Type).Distinct().Count() == 5);
        }
        private static List<object> TeleportDestinationWorldDifferences(object before, object after)
        {
            var result = new List<object>();
            CollectTeleportDestinationDifferences(JObject.Parse(TeleportationDiagnosticJson.Serialize(before))["world"],
                JObject.Parse(TeleportationDiagnosticJson.Serialize(after))["world"], "world", result);
            return result;
        }
        private static void CollectTeleportDestinationDifferences(JToken before, JToken after, string path, List<object> result)
        {
            if (JToken.DeepEquals(before, after)) return;
            var firstObject = before as JObject; var secondObject = after as JObject;
            if (firstObject != null && secondObject != null)
            {
                foreach (string key in firstObject.Properties().Select(value => value.Name).Union(secondObject.Properties().Select(value => value.Name)))
                    CollectTeleportDestinationDifferences(firstObject[key], secondObject[key], path + "." + key, result);
                return;
            }
            var firstArray = before as JArray; var secondArray = after as JArray;
            if (firstArray != null && secondArray != null && firstArray.Count == secondArray.Count)
            {
                for (int index = 0; index < firstArray.Count; index++)
                    CollectTeleportDestinationDifferences(firstArray[index], secondArray[index], path + "[" + index + "]", result);
                return;
            }
            result.Add(new { path, before, after });
        }
    }
}
