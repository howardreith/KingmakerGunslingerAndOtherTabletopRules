using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class TeleportFamiliarityRuntime
    {
        private static ModContext Context { get { ModContext context; ModContext.TryGet(out context); return context; } }

        internal static bool Enabled
        {
            get
            {
                ModContext context = Context;
                return !_observerFailureReported && context != null && context.FeatureModules.Active.TeleportationSpells &&
                    TeleportFamiliarityPatches.Installed && BlueprintBootstrap.TeleportationPublication != null;
            }
        }

        internal static UnitPartTeleportFamiliarity EnsureLedger(Player player)
        {
            if (!Enabled || player == null || player.GlobalMap == null || player.MainCharacter.Value == null) return null;
            UnitDescriptor owner = player.MainCharacter.Value.Descriptor;
            UnitPartTeleportFamiliarity part = owner.Get<UnitPartTeleportFamiliarity>();
            try
            {
                if (part == null) part = owner.Ensure<UnitPartTeleportFamiliarity>();
                bool migrated = part.Read().LegacyMigrationComplete;
                if (!migrated)
                {
                    string[] visited = player.GlobalMap.Locations.Where(pair => pair.Key != null && pair.Value != null &&
                        ReferenceEquals(pair.Key, pair.Value.Blueprint) &&
                        TeleportDestinationPolicy.IsStableId(pair.Key.AssetGuid) &&
                        (pair.Value.EdgesOpened || pair.Value.IsExplored)).Select(pair => pair.Key.AssetGuid).ToArray();
                    part.MigrateLegacy(visited);
                    Context.Logger.Info("teleportation", "familiarity.migrated",
                        "owner=" + player.MainCharacter.Value.UniqueId + ";nativeVisited=" + visited.Length);
                }
                return part;
            }
            catch (Exception exception)
            {
                if (part == null || !part.DiagnosticEmitted)
                {
                    if (part != null) part.DiagnosticEmitted = true;
                    Context.Logger.Failure("teleportation", "familiarity.unavailable",
                        "Persisted payload retained; familiarity-dependent casting fails closed.", exception);
                }
                return null;
            }
        }

        // Both callbacks swallow their own errors; native movement always keeps its
        // original control flow. No observer exists on selection, reveal or relocation.
        internal static MovementObservation Begin(MapTravelData travel, float before)
        {
            try
            {
                if (!Enabled || Game.Instance == null || Game.Instance.CurrentMode != GameModeType.GlobalMap ||
                    LoadingProcess.Instance.IsLoadingInProcess || travel == null || !travel.Walking ||
                    !ReferenceEquals(GlobalMapRules.State.TravelData, travel)) return null;
                UnitPartTeleportFamiliarity ledger = EnsureLedger(Game.Instance.Player);
                if (ledger == null) return null;
                GlobalMapRules rules = GlobalMapRules.Instance;
                var boundaries = new List<TeleportRouteBoundary>();
                float distance = 0;
                for (int index = 0; index < travel.Path.Count; index++)
                {
                    TravelEdge path = travel.Path[index];
                    if (path == null || path.Blueprint == null || (path.Direction != 1 && path.Direction != -1))
                        throw new InvalidOperationException("Unknown native route edge/direction.");
                    GlobalMapEdge edge = rules.GetEdgeObject(path.Blueprint);
                    if (edge == null || edge.Spline == null) throw new InvalidOperationException("Missing native spline.");
                    distance += edge.Spline.WorldLength; // Same float accumulation as native MoveAlongEdge.
                    GlobalMapLocation point = path.Direction > 0 ? edge.Location2 : edge.Location1;
                    if (point == null || point.Blueprint == null) throw new InvalidOperationException("Missing native route endpoint.");
                    bool qualifying = point.gameObject.activeInHierarchy &&
                        ReferenceEquals(rules.GetLocationObject(point.Blueprint), point) &&
                        ReferenceEquals(ResourcesLibrary.TryGetBlueprint<Kingmaker.Globalmap.Blueprints.BlueprintLocation>(point.Blueprint.AssetGuid), point.Blueprint) &&
                        Enum.IsDefined(typeof(Kingmaker.Globalmap.Blueprints.LocationType), point.Blueprint.Type) &&
                        (index < travel.Path.Count - 1 || (travel.To != null && travel.To.Location != null));
                    boundaries.Add(new TeleportRouteBoundary(point.Blueprint.AssetGuid, distance, qualifying));
                }
                return new MovementObservation(Game.Instance.Player, GlobalMapRules.State, rules, travel, ledger,
                    new TeleportOrdinaryArrivalObservation(before, boundaries));
            }
            catch (Exception exception) { ReportObserverFailure(exception); return null; }
        }

        internal static void End(MovementObservation observation)
        {
            try
            {
                if (observation == null || !Enabled || Game.Instance == null ||
                    !ReferenceEquals(Game.Instance.Player, observation.Player) ||
                    !ReferenceEquals(GlobalMapRules.Instance, observation.Rules) ||
                    !ReferenceEquals(GlobalMapRules.State, observation.State)) return;
                string[] ids = observation.Progress.Complete(observation.Travel.WalkedDistance,
                    !ReferenceEquals(observation.State.TravelData, observation.Travel),
                    observation.State.PartyLocation == null ? null : observation.State.PartyLocation.AssetGuid);
                if (ids.Length == 0) return;
                if (!ReferenceEquals(observation.Player.MainCharacter.Value.Descriptor.Get<UnitPartTeleportFamiliarity>(), observation.Ledger))
                    throw new InvalidOperationException("Campaign familiarity owner changed during movement.");
                observation.Ledger.RecordOrdinaryArrivals(ids);
                Context.Logger.Info("teleportation", "familiarity.ordinary-arrival",
                    "points=" + string.Join(",", ids.Select(id => id + ":" + observation.Ledger.Read().Count(id))) +
                    ";source=MapMovementController.MoveAlongEdge;routeReplanned=false");
            }
            catch (Exception exception) { ReportObserverFailure(exception); }
        }

        private static bool _observerFailureReported;
        private static void ReportObserverFailure(Exception exception)
        {
            if (_observerFailureReported) return;
            _observerFailureReported = true;
            try
            {
                ModContext context = Context;
                if (context != null) context.Logger.Failure("teleportation", "familiarity.observer-failed",
                    "Native movement continues unchanged; unproven arrivals are not recorded.", exception);
            }
            catch { /* Diagnostics must never escape into native movement. */ }
        }

        internal sealed class MovementObservation
        {
            internal MovementObservation(Player player, GlobalMapState state, GlobalMapRules rules, MapTravelData travel,
                UnitPartTeleportFamiliarity ledger, TeleportOrdinaryArrivalObservation progress)
            { Player = player; State = state; Rules = rules; Travel = travel; Ledger = ledger; Progress = progress; }
            internal readonly Player Player;
            internal readonly GlobalMapState State;
            internal readonly GlobalMapRules Rules;
            internal readonly MapTravelData Travel;
            internal readonly UnitPartTeleportFamiliarity Ledger;
            internal readonly TeleportOrdinaryArrivalObservation Progress;
        }
    }
}
