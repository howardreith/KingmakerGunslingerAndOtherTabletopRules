using System;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Globalmap;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class TeleportationWorldSnapshot
    {
        private readonly TeleportationWorldMapContext _context;
        private readonly object _area;
        private readonly object _scene;
        private readonly GlobalMapPawn _pawn;
        private readonly object[] _history;
        private readonly object[] _encounters;
        private readonly object[] _locationRecords;
        private readonly object[] _edgeRecords;
        private readonly string _protectedState;
        internal readonly TeleportationTravelers Travelers;
        internal readonly object State;

        internal TeleportationWorldSnapshot(TeleportationWorldMapContext context)
        {
            if (context == null || !context.Usable) throw new InvalidOperationException("A current stationary native world-map snapshot is required.");
            _context = context;
            _area = Game.Instance.CurrentlyLoadedArea;
            _scene = Game.Instance.CurrentScene;
            _pawn = context.Rules.Pawn;
            Travelers = TeleportationTravelers.Read(context.Player);
            _history = context.Map.HistoryTravels.Cast<object>().ToArray();
            _encounters = context.Map.Encounters.Cast<object>().ToArray();
            _locationRecords = context.Map.Locations.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal)
                .Select(value => (object)value.Value).ToArray();
            _edgeRecords = context.Map.Edges.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal)
                .Select(value => (object)value.Value).ToArray();
            object protection = ProtectedState(context, Travelers);
            _protectedState = JsonConvert.SerializeObject(protection);
            var position = context.Rules.Pawn.Position;
            State = new { pointId = context.OriginId, pawnPosition = new { x = position.x, y = position.y, z = position.z },
                world = protection, roster = Travelers.Evidence() };
        }

        internal void Verify(string expectedPointId)
        {
            var current = TeleportationWorldMapAdapter.Capture(false);
            if (!current.Usable || current.OriginId != expectedPointId ||
                !ReferenceEquals(_context.Player, current.Player) || !ReferenceEquals(_context.Map, current.Map) ||
                !ReferenceEquals(_context.Rules, current.Rules) || !ReferenceEquals(_pawn, current.Rules.Pawn) ||
                !ReferenceEquals(_area, Game.Instance.CurrentlyLoadedArea) || !ReferenceEquals(_scene, Game.Instance.CurrentScene))
                throw new InvalidOperationException("Native relocation context/anchor invariant failed: " + current.Diagnostic);
            var travelers = TeleportationTravelers.Read(current.Player);
            if (!Travelers.Matches(travelers) ||
                !_history.SequenceEqual(current.Map.HistoryTravels.Cast<object>()) ||
                !_encounters.SequenceEqual(current.Map.Encounters.Cast<object>()) ||
                !_locationRecords.SequenceEqual(current.Map.Locations.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal).Select(value => (object)value.Value)) ||
                !_edgeRecords.SequenceEqual(current.Map.Edges.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal).Select(value => (object)value.Value)) ||
                _protectedState != JsonConvert.SerializeObject(ProtectedState(current, travelers)))
                throw new InvalidOperationException("Native relocation changed protected party, time, fatigue, route, encounter, map or familiarity state.");
        }

        private static object ProtectedState(TeleportationWorldMapContext context, TeleportationTravelers travelers)
        {
            var map = context.Map;
            return new { gameTimeTicks = context.Player.GameTime.Ticks,
                areaId = Game.Instance.CurrentlyLoadedArea.AssetGuid,
                sceneHandle = context.Rules.gameObject.scene.handle, mode = Game.Instance.CurrentMode.ToString(),
                loading = LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive,
                travelPending = map.TravelData != null, historyCount = map.HistoryTravels.Count,
                encounterPending = map.CurrentEncounterData != null, encounterCount = map.Encounters.Count(),
                lastEncounterId = map.LastEncounter == null ? null : map.LastEncounter.AssetGuid,
                miles = map.MilesTravelled, nextEncounterRollMiles = map.NextEncounterRollMiles,
                familiarity = context.Familiarity.Serialize(),
                fatigue = travelers.Units.Select(unit =>
                {
                    UnitPartWeariness weariness = unit.Descriptor.Get<UnitPartWeariness>();
                    return new { id = unit.UniqueId, hasWeariness = weariness != null,
                        stacks = weariness == null ? 0 : weariness.WearinessStacks,
                        fatigueHours = weariness == null ? 0 : weariness.FatigueHours,
                        lastStack = weariness == null ? 0 : weariness.LastStackTime.Ticks,
                        lastBuff = weariness == null ? 0 : weariness.LastBuffApplyTime.Ticks };
                }).ToArray(),
                points = map.Locations.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal).Select(value => new {
                    id = value.Key.AssetGuid, revealed = value.Value.IsRevealed, explored = value.Value.IsExplored,
                    seen = value.Value.IsSeen, closed = value.Value.IsClosed, edgesOpened = value.Value.EdgesOpened,
                    fake = value.Value.IsFake, lastVisited = value.Value.LastVisited.Ticks,
                    lastPerception = value.Value.LastPerceptionRolled }).ToArray(),
                edges = map.Edges.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal).Select(value => new {
                    id = value.Key.AssetGuid, first = value.Value.Explored1, second = value.Value.Explored2 }).ToArray(),
                perceptionIds = map.PerceptionRolledLocations.Select(value => value.AssetGuid).OrderBy(value => value, StringComparer.Ordinal).ToArray() };
        }
    }
}
