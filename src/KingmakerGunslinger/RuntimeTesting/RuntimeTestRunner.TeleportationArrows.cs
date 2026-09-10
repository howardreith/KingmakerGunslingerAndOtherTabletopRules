using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.UI;
using Kingmaker.UI.GlobalMap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Gate 1 diagnostics for the post-teleport directional-arrow defect.
        // This scenario is evidence gathering: it captures the actual native
        // direction-marker state and the exact CalculatePathByMarker refusal at
        // each mission boundary, through the real native handlers only.
        private IEnumerable<int> RunTeleportationArrows(GlobalMapLocation origin, GlobalMapLocation middle, GlobalMapLocation target,
            List<TeleportResourceFixtureOwner> owners, TeleportInteractionMovementObserver movement)
        {
            var player = Game.Instance.Player; var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State;
            var panel = TeleportationFixturePanel();
            if (map.TravelData != null || map.PartyLocation != origin.Blueprint)
                throw new InvalidOperationException("The arrows fixture requires its exact stationary origin start.");
            var continueButton = typeof(GlobalMapUI).GetField("m_BtnContiune", BindingFlags.Instance | BindingFlags.NonPublic);
            var stopButton = typeof(GlobalMapUI).GetField("m_BtnStop", BindingFlags.Instance | BindingFlags.NonPublic);
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "b3a505fb61437dc4097f43c3f8f9a4cf", "native arrow audit Sorcerer");
            var owner = player.Party.FirstOrDefault(value => TeleportationSpellbookAdapter.CasterAvailable(value) && value.Descriptor.GetSpellbook(sorcerer.Spellbook) == null);
            if (owner == null) throw new InvalidOperationException("No available existing owner for the arrow audit spellbook.");
            var fixture = new TeleportResourceFixtureOwner(owner); owners.Add(fixture);
            var book = fixture.AddBook(sorcerer.Spellbook);
            book.AddKnown(7, BlueprintBootstrap.Teleportation.GreaterTeleport, true);
            book.AddKnown(5, BlueprintBootstrap.Teleportation.Teleport, true);
            // Boundary 1: stationary at the origin before casting, with the
            // native marker/path state the player would see here.
            CaptureTeleportInteraction("arrow-boundary-1-origin", DescribeArrowBoundary(rules, "origin"));
            foreach (var destination in new[] { middle, target })
            {
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                book.Rest();
                string destinationId = destination.Blueprint.AssetGuid;
                string resourcesBefore = TeleportResourceFingerprint(book);
                int castStarts = movement.Starts, castStops = movement.Stops;
                float milesBefore = map.MilesTravelled;
                var timeBefore = player.GameTime;
                SelectTeleportationCastingPoint(panel, destination);
                foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
                var rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                if (rows == null || rows.Actions.Count != 1 || rows.Actions[0].Source.Spell != TeleportSpellKind.GreaterTeleport)
                    throw new InvalidOperationException("The arrow audit cast lost its single contextual source: " + destinationId);
                rows.QualificationRolls = new TeleportationFixtureRolls(new int[0]);
                rows.Buttons[0].onClick.Invoke();
                var request = TeleportContextConfirmationPresenter.Current;
                if (request == null || !DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("The arrow audit cast did not open its owned confirmation.");
                for (int frame = 0; frame < 8; frame++) yield return 0;
                if (resourcesBefore != TeleportResourceFingerprint(book))
                    throw new InvalidOperationException("Opening the arrow audit confirmation spent a resource.");
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                bool committed = request.Transaction.State == TeleportTransactionState.Completed &&
                    request.Transaction.Result != null && request.Transaction.Result.Status == TeleportExecutionStatus.Arrived &&
                    request.Transaction.Result.DestinationId == destinationId;
                if (!committed) throw new InvalidOperationException("The arrow audit cast did not arrive: " + destinationId + ";" + request.Transaction.Diagnostic);
                // Boundary 2: immediately after relocation and the normal UI
                // lifecycle, before any player input.
                for (int frame = 0; frame < 5; frame++) yield return 0;
                CaptureTeleportInteraction("arrow-boundary-2-arrival-" + destinationId, DescribeArrowBoundary(rules, destinationId));
                // Stationary waiting frames prove no hidden recovery walk occurs
                // before the first arrow action.
                for (int frame = 0; frame < 30; frame++) yield return 0;
                TeleportInteractionAssert("arrow-stationary-" + destinationId,
                    "post-cast waiting frames add no mileage, time or travel beyond the cast single native pawn-notification pair",
                    "starts=" + movement.Starts + ";milesDelta=" + (map.MilesTravelled - milesBefore) + ";travelData=" + (map.TravelData != null),
                    movement.Starts == castStarts + 1 && movement.Stops == castStops + 1 &&
                    map.MilesTravelled == milesBefore && player.GameTime == timeBefore && map.TravelData == null);
                // Boundary 3: the first attempted native directional action.
                // The native control is CompassAvatarController's rebuilt
                // CompassDirectionLabel arrows; OnClick is the exact handler a
                // player click reaches.
                var arrival = rules.GetLocationObject(map.PartyLocation);
                if (arrival == null || arrival.Blueprint.AssetGuid != destinationId)
                    throw new InvalidOperationException("The arrow audit arrival point differs from its cast destination.");
                var labels = ArrowCompassLabels();
                var labelDetail = labels.Select(label => new {
                    id = label.GetInstanceID(), active = label.gameObject.activeInHierarchy,
                    directionId = ArrowLabelDirection(label) == null ? null : ArrowLabelDirection(label).Blueprint.AssetGuid,
                    currentPositionLocation = ArrowLabelPosition(label) == null || ArrowLabelPosition(label).Location == null ? null : ArrowLabelPosition(label).Location.AssetGuid,
                    edgeId = ArrowLabelEdge(label) == null ? null : ArrowLabelEdge(label).Blueprint.AssetGuid }).ToArray();
                bool arrowsRebuiltAtArrival = labels.Length > 0 && labels.All(label => ArrowLabelEdge(label) != null && arrival.Edges.Contains(ArrowLabelEdge(label)));
                var exercisedLabel = labels.FirstOrDefault();
                if (exercisedLabel != null) exercisedLabel.OnClick();
                for (int frame = 0; frame < 10; frame++) yield return 0;
                bool arrowStartedTravel = map.TravelData != null && map.TravelData.Walking;
                CaptureTeleportInteraction("arrow-boundary-3-first-action-" + destinationId, new {
                    labels = labelDetail, arrowsRebuiltAtArrival,
                    exercisedLabel = exercisedLabel == null ? null : (int?)exercisedLabel.GetInstanceID(),
                    arrowStartedTravel, travelData = DescribeArrowTravel(map.TravelData),
                    nativeContinueVisible = DescribeArrowButton(continueButton), nativeStopVisible = DescribeArrowButton(stopButton),
                    markers = DescribeArrowMarkers(rules), movement.Starts, movement.Stops });
                if (map.TravelData != null)
                {
                    // The diagnostic must never leave travel running.
                    if (map.TravelData.Walking) rules.OnBreak();
                    map.TravelData = null;
                    rules.SetCurrentPosition(new MapPosition(destination.Blueprint)); rules.UpdatePawnPosition();
                }
                TeleportInteractionAssert("arrow-rebuilt-at-arrival-" + destinationId,
                    "the cast's native pawn-notification pair rebuilds the compass arrows bound to the actual arrival point",
                    "labels=" + labels.Length + ";allAtArrival=" + arrowsRebuiltAtArrival,
                    arrowsRebuiltAtArrival);
                TeleportInteractionAssert("arrow-first-action-" + destinationId,
                    "the first legal arrow through its actual native handler starts real native travel without another player action",
                    "labels=" + labels.Length + ";started=" + arrowStartedTravel,
                    labels.Length > 0 && arrowStartedTravel);
            }
            // Gate 1 breadth: an off-target/similar Teleport arrival still
            // refreshes the arrows for the ACTUAL arrival point. A forced worst
            // d100 reproduces a conditional outcome deterministically.
            rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
            book.Rest();
            var conditionalRolls = new TeleportationFixtureRolls(new[] { 1, 1, 1, 1 });
            SelectTeleportationCastingPoint(panel, target);
            foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                var offRows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
                var offAction = offRows == null ? null : offRows.Actions.FirstOrDefault(value =>
                    value.Source.Spell == TeleportSpellKind.Teleport);
                if (offAction != null)
                {
                    offRows.QualificationRolls = conditionalRolls;
                    offRows.Buttons[offRows.Actions.ToList().FindIndex(value => value.Key == offAction.Key)].onClick.Invoke();
                    var offRequest = TeleportContextConfirmationPresenter.Current;
                    if (offRequest != null && DialogMessageBox.Instance.IsShown)
                    {
                        TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                        for (int frame = 0; frame < 12; frame++) yield return 0;
                        var offArrival = rules.GetLocationObject(map.PartyLocation);
                        var offLabels = ArrowCompassLabels();
                        CaptureTeleportInteraction("arrow-offtarget-arrival", new {
                            transaction = offRequest.Transaction.State.ToString(),
                            result = offRequest.Execution.LastEvidence == null ? null : TeleportationDiagnosticJson.Serialize(offRequest.Execution.LastEvidence).Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            actualArrival = map.PartyLocation == null ? null : map.PartyLocation.AssetGuid,
                            requestedArrival = target.Blueprint.AssetGuid,
                            labels = offLabels.Length });
                        TeleportInteractionAssert("arrow-offtarget-arrival",
                            "an off-target arrival rebuilds the arrows bound to the ACTUAL arrival point",
                            "actual=" + (map.PartyLocation == null ? "none" : map.PartyLocation.AssetGuid) +
                                ";labels=" + offLabels.Length,
                            offRequest.Transaction.State == TeleportTransactionState.Completed && offArrival != null &&
                                offLabels.Length > 0 && offLabels.All(label => ArrowLabelEdge(label) != null &&
                                offArrival.Edges.Contains(ArrowLabelEdge(label))));
                        break;
                    }
                }
                for (int frame = 0; frame < 30; frame++) yield return 0;
            }
            // Boundary 4: the owner's recovery workaround on the final arrival.
            // Clicking another revealed dot, moving briefly and stopping must be
            // captured so the differing state is explicit.
            var lastArrival = rules.GetLocationObject(map.PartyLocation);
            var workaroundTarget = lastArrival == origin ? middle : origin;
            SelectTeleportationCastingPoint(panel, workaroundTarget);
            foreach (int tick in WaitTeleportInteractionPanel(panel)) yield return tick;
            panel.Accept();
            for (int frame = 0; frame < 3; frame++) yield return 0;
            if (map.TravelData == null || !map.TravelData.Walking)
                throw new InvalidOperationException("The workaround native Travel did not begin.");
            for (int frame = 0; frame < 10; frame++) yield return 0;
            rules.OnBreak();
            for (int frame = 0; frame < 5; frame++) yield return 0;
            CaptureTeleportInteraction("arrow-boundary-4-workaround", new {
                stoppedTravelData = DescribeArrowTravel(map.TravelData),
                partyLocation = map.PartyLocation == null ? null : map.PartyLocation.AssetGuid,
                boundaryAfterWorkaround = (TeleportFamiliarityRuntime.EnsureLedger(player).ReadExplorationBoundary() == null).ToString(),
                markers = DescribeArrowMarkers(rules), movement.Starts, movement.Stops });
            map.TravelData = null;
            rules.SetCurrentPosition(new MapPosition(lastArrival.Blueprint)); rules.UpdatePawnPosition();
            CaptureTeleportInteraction("arrow-final-state", DescribeArrowBoundary(rules, "restored-arrival"));
        }

        private static readonly System.Reflection.FieldInfo ArrowLabelDirectionField = typeof(CompassDirectionLabel)
            .GetField("m_DirLoc", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly System.Reflection.FieldInfo ArrowLabelPositionField = typeof(CompassDirectionLabel)
            .GetField("m_CurrentPosition", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly System.Reflection.FieldInfo ArrowLabelEdgeField = typeof(CompassDirectionLabel)
            .GetField("m_Edge", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static GlobalMapLocation ArrowLabelDirection(CompassDirectionLabel label)
        { return ArrowLabelDirectionField == null ? null : ArrowLabelDirectionField.GetValue(label) as GlobalMapLocation; }
        private static MapPosition ArrowLabelPosition(CompassDirectionLabel label)
        { return ArrowLabelPositionField == null ? null : ArrowLabelPositionField.GetValue(label) as MapPosition; }
        private static Kingmaker.Globalmap.GlobalMapEdge ArrowLabelEdge(CompassDirectionLabel label)
        { return ArrowLabelEdgeField == null ? null : ArrowLabelEdgeField.GetValue(label) as Kingmaker.Globalmap.GlobalMapEdge; }

        private static CompassDirectionLabel[] ArrowCompassLabels()
        {
            var labelsField = typeof(Kingmaker.UI.GlobalMap.CompassAvatarController)
                .GetField("m_DirectionLabels", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var controllers = UnityEngine.Object.FindObjectsOfType<Kingmaker.UI.GlobalMap.CompassAvatarController>();
            if (labelsField == null || controllers == null || controllers.Length == 0) return new CompassDirectionLabel[0];
            var result = new List<CompassDirectionLabel>();
            foreach (var controller in controllers)
            {
                var list = labelsField.GetValue(controller) as List<CompassDirectionLabel>;
                if (list == null) continue;
                foreach (var label in list) if (label != null) result.Add(label);
            }
            return result.ToArray();
        }

        private static IEnumerable<GlobalMapUiDirectionMarker> ArrowVisibleMarkers(GlobalMapRules rules)
        {
            // Scene-prefab markers, including inactive ones: the defect report
            // is about visible-but-dead arrows, so visibility is recorded, not
            // assumed.
            foreach (var marker in Resources.FindObjectsOfTypeAll<GlobalMapUiDirectionMarker>())
            {
                if (marker == null || !marker.gameObject.scene.IsValid()) continue;
                yield return marker;
            }
        }
        private static object DescribeArrowBoundary(GlobalMapRules rules, string label)
        {
            var map = GlobalMapRules.State;
            return new { label, partyLocation = map.PartyLocation == null ? null : map.PartyLocation.AssetGuid,
                travelData = DescribeArrowTravel(map.TravelData),
                nativeContinueVisible = DescribeArrowButton(typeof(GlobalMapUI).GetField("m_BtnContiue", BindingFlags.Instance | BindingFlags.NonPublic)),
                nativeStopVisible = DescribeArrowButton(typeof(GlobalMapUI).GetField("m_BtnStop", BindingFlags.Instance | BindingFlags.NonPublic)),
                markers = DescribeArrowMarkers(rules) };
        }
        private static object DescribeArrowButton(FieldInfo field)
        {
            var ui = GlobalMapUI.Instance;
            var button = ui == null || field == null ? null : field.GetValue(ui) as GameObject;
            return button == null ? null : (bool?)button.activeInHierarchy;
        }
        private static List<object> DescribeArrowMarkers(GlobalMapRules rules)
        {
            var result = new List<object>();
            foreach (var marker in ArrowVisibleMarkers(rules).OrderBy(value => value.GetInstanceID()))
            {
                var position = marker.Position;
                object computed = null, computedUnexplored = null;
                try { computed = rules.CalculatePathByMarker(marker) == null ? null : DescribeArrowPath(rules.CalculatePathByMarker(marker)); } catch (Exception exception) { computed = "error:" + exception.GetType().Name; }
                try { computedUnexplored = position == null ? null : (rules.CalculatePathToPosition(position, true) == null ? null : DescribeArrowPath(rules.CalculatePathToPosition(position, true))); } catch (Exception exception) { computedUnexplored = "error:" + exception.GetType().Name; }
                result.Add(new { id = marker.GetInstanceID(), active = marker.gameObject.activeInHierarchy,
                    positionLocation = position == null || position.Location == null ? null : position.Location.AssetGuid,
                    positionEdge = position == null || position.Edge == null ? null : position.Edge.AssetGuid,
                    edgePosition = position == null ? (float?)null : position.EdgePosition,
                    directionId = marker.DirectionLocation == null ? null : marker.DirectionLocation.Blueprint.AssetGuid,
                    matchesPartyLocation = position != null && position.Location != null && position.Location == GlobalMapRules.State.PartyLocation,
                    computedPath = computed, computedPathAllowUnexplored = computedUnexplored });
            }
            return result;
        }
        private static object DescribeArrowTravel(MapTravelData travel)
        {
            if (travel == null) return null;
            return new { walking = travel.Walking, finished = travel.Finished,
                fromLocation = travel.From == null || travel.From.Location == null ? null : travel.From.Location.AssetGuid,
                toLocation = travel.To == null || travel.To.Location == null ? null : travel.To.Location.AssetGuid,
                edges = travel.Path == null ? null : travel.Path.Select(value => value.Blueprint == null ? null : value.Blueprint.AssetGuid).ToArray() };
        }
        private static object DescribeArrowPath(MapTravelData path)
        {
            return new { toLocation = path.To == null || path.To.Location == null ? null : path.To.Location.AssetGuid,
                edges = path.Path == null ? null : path.Path.Select(value => value.Blueprint == null ? null : value.Blueprint.AssetGuid).ToArray() };
        }
    }
}
