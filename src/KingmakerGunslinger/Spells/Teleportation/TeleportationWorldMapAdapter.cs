using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.State;
using Kingmaker.Kingdom.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // An interaction-local read of the current campaign. No selection, route,
    // reveal, persistence creation, resource expenditure or relocation occurs here.
    internal sealed class TeleportationWorldMapContext
    {
        internal Player Player;
        internal GlobalMapRules Rules;
        internal GlobalMapState Map;
        internal TeleportFamiliarityState Familiarity;
        internal TeleportCastBlock Blocks;
        internal string OriginId;
        internal WordOfRecallCapitalState Recall;
        internal string Diagnostic;
        internal bool Usable { get { return Blocks == TeleportCastBlock.None; } }
    }

    internal static class TeleportationWorldMapAdapter
    {
        // No unconditional point-specific exclusion was established in inventory v1.
        // Every candidate must still pass the live native state and anchor checks.
        internal static readonly TeleportForbiddenDestinationCatalog Forbidden =
            new TeleportForbiddenDestinationCatalog(1, new KeyValuePair<string, string>[0]);

        internal static TeleportationWorldMapContext Capture(bool relocationPending)
        {
            var result = new TeleportationWorldMapContext { Recall = WordOfRecallCapitalState.Unknown };
            if (!TeleportFamiliarityRuntime.Enabled)
            {
                result.Blocks = TeleportCastBlock.ModuleDisabled;
                result.Diagnostic = result.Blocks.ToString();
                return result;
            }
            try
            {
                Game game = Game.Instance;
                if (game == null || game.Player == null || game.State == null)
                    throw new InvalidOperationException("No current native campaign.");
                result.Player = game.Player;
                result.Map = game.Player.GlobalMap;
                result.Rules = GlobalMapRules.Instance;
                if (relocationPending) result.Blocks |= TeleportCastBlock.RelocationPending;
                if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive)
                    result.Blocks |= TeleportCastBlock.Loading | TeleportCastBlock.AreaTransition;
                if (game.CurrentMode != GameModeType.GlobalMap || game.CurrentlyLoadedArea == null ||
                    !game.CurrentlyLoadedArea.HasCustomUI || result.Rules == null ||
                    !result.Rules.isActiveAndEnabled || !result.Rules.gameObject.scene.IsValid() ||
                    !result.Rules.gameObject.scene.isLoaded || game.CurrentlyLoadedArea.GetStaticScene() == null ||
                    game.CurrentlyLoadedArea.GetStaticScene().SceneName != result.Rules.gameObject.scene.name)
                    result.Blocks |= TeleportCastBlock.NotWorldMap;
                if (game.Player.IsInCombat) result.Blocks |= TeleportCastBlock.Combat;
                if (game.DialogController.Dialog != null || game.Player.Dialog.Scheduled != null ||
                    game.IsModeActive(GameModeType.Dialog)) result.Blocks |= TeleportCastBlock.Dialogue;
                if (game.CutsceneLock || game.IsModeActive(GameModeType.Cutscene) ||
                    game.IsModeActive(GameModeType.CutsceneGlobalMap)) result.Blocks |= TeleportCastBlock.Cutscene;
                foreach (var cutscene in game.State.Cutscenes)
                    if (cutscene != null && !cutscene.IsFinished) result.Blocks |= TeleportCastBlock.Cutscene;
                if (game.IsModeActive(GameModeType.Kingdom) || game.IsModeActive(GameModeType.KingdomSettlement))
                    result.Blocks |= TeleportCastBlock.KingdomOperation;
                if (result.Map == null || !ReferenceEquals(GlobalMapRules.State, result.Map))
                    throw new InvalidOperationException("No canonical world-map state.");
                if (result.Map.CurrentEncounterData != null || (game.CurrentScene != null && game.CurrentScene.Encounter != null))
                    result.Blocks |= TeleportCastBlock.Encounter;
                if (result.Map.TravelData != null)
                {
                    result.Blocks |= TeleportCastBlock.AdvancingTravel;
                    if (result.Map.TravelData.Walking) result.Blocks |= TeleportCastBlock.Moving;
                }
                var party = game.Player.Party;
                if (party.Count == 0 || game.Player.MainCharacter.Value == null ||
                    !party.Any(unit => ReferenceEquals(unit, game.Player.MainCharacter.Value)) ||
                    party.Any(unit => unit == null || unit.IsDetached || string.IsNullOrWhiteSpace(unit.UniqueId)) ||
                    party.Select(unit => unit.UniqueId).Distinct(StringComparer.Ordinal).Count() != party.Count ||
                    result.Rules == null || result.Rules.Pawn == null || !result.Rules.Pawn.isActiveAndEnabled)
                    result.Blocks |= TeleportCastBlock.MissingCanonicalParty;
                MapPosition position = result.Map.PartyPosition;
                if (position == null || position.Location == null || position.Edge != null ||
                    !TeleportDestinationPolicy.IsStableId(position.Location.AssetGuid) || result.Rules == null ||
                    !HasCurrentAnchor(result.Rules.GetLocationObject(position.Location), result.Rules, position.Location))
                    result.Blocks |= TeleportCastBlock.UnanchoredOrigin;
                else
                {
                    result.OriginId = position.Location.AssetGuid;
                    GlobalMapLocation origin = result.Rules.GetLocationObject(position.Location);
                    if (result.Rules.Pawn == null || !Finite(result.Rules.Pawn.Position) ||
                        (result.Rules.Pawn.Position - origin.transform.position).sqrMagnitude > 0.0001f)
                        result.Blocks |= TeleportCastBlock.UnanchoredOrigin;
                }
                // Migration belongs to native area-load/ordinary movement, never a menu read.
                var owner = game.Player.MainCharacter.Value;
                UnitPartTeleportFamiliarity ledger = owner == null ? null : owner.Descriptor.Get<UnitPartTeleportFamiliarity>();
                if (ledger == null || !(result.Familiarity = ledger.Read()).LegacyMigrationComplete)
                    result.Blocks |= TeleportCastBlock.UnknownState;
                result.Recall = ReadRecall(game.Player);
                result.Diagnostic = "blocks=" + result.Blocks + ";origin=" + result.OriginId +
                    ";area=" + (game.CurrentlyLoadedArea == null ? null : game.CurrentlyLoadedArea.AssetGuid) +
                    ";mode=" + game.CurrentMode + ";recall=" + result.Recall.Diagnostic;
            }
            catch (Exception exception)
            {
                result.Blocks |= TeleportCastBlock.UnknownState;
                result.Diagnostic = "blocks=" + result.Blocks + ";nativeRead=" + exception.GetType().FullName + ": " + exception.Message;
            }
            return result;
        }

        internal static TeleportDestinationSnapshot ReadDestination(TeleportationWorldMapContext context, BlueprintLocation blueprint)
        {
            if (context == null || blueprint == null) return null;
            string id = blueprint.AssetGuid;
            var facts = TeleportDestinationFacts.None;
            TeleportPointKind kind = Kind(blueprint.Type);
            LocationData data = null;
            GlobalMapLocation anchor = null;
            bool nativeVisited = false;
            int count = 0;
            try
            {
                if (TeleportDestinationPolicy.IsStableId(id) && blueprint.GetType() == typeof(BlueprintLocation) &&
                    ReferenceEquals(ResourcesLibrary.TryGetBlueprint<BlueprintLocation>(id), blueprint))
                    facts |= TeleportDestinationFacts.Persistent;
                if (context.Map != null) context.Map.Locations.TryGetValue(blueprint, out data);
                if (data != null && ReferenceEquals(data.Blueprint, blueprint))
                {
                    nativeVisited = data.EdgesOpened || data.IsExplored;
                    if (data.IsRevealed) facts |= TeleportDestinationFacts.Revealed;
                    if (!data.IsClosed && blueprint.GetComponents<LocationRestriction>().All(value => !value.IsRestricted()))
                        facts |= TeleportDestinationFacts.CampaignAllowed;
                }
                if (context.Familiarity != null && TeleportDestinationPolicy.IsStableId(id)) count = context.Familiarity.Count(id);
                if (context.Rules != null) anchor = context.Rules.GetLocationObject(blueprint);
                if (anchor != null && anchor.gameObject.activeInHierarchy && anchor.enabled)
                    facts |= TeleportDestinationFacts.Active;
                if (anchor != null && anchor.GetType() == typeof(GlobalMapLocation) &&
                    ReferenceEquals(anchor.Blueprint, blueprint) && GlobalMapLocation.Instances.Count(value =>
                        value != null && value.Blueprint != null && value.Blueprint.AssetGuid == id) == 1)
                    facts |= TeleportDestinationFacts.Current;
                if (HasCurrentAnchor(anchor, context.Rules, blueprint))
                    facts |= TeleportDestinationFacts.PlacementSupported | TeleportDestinationFacts.SameGlobalMap;
                if (anchor != null && anchor.LocationTooltipPoint != null)
                    facts |= TeleportDestinationFacts.Selectable;
                string name = data == null ? (string)blueprint.Name : data.Name;
                return new TeleportDestinationSnapshot(id, name, kind, facts, nativeVisited, count);
            }
            catch
            {
                // An unknown native restriction or presentation provider cannot make an action available.
                return new TeleportDestinationSnapshot(id, null, kind,
                    facts & ~TeleportDestinationFacts.CampaignAllowed, nativeVisited, count);
            }
        }

        internal static IReadOnlyList<WorldMapPointSpellAction> Compose(TeleportationWorldMapContext context,
            BlueprintLocation destination)
        {
            if (context == null || !context.Usable) return new WorldMapPointSpellAction[0];
            TeleportDestinationSnapshot point = ReadDestination(context, destination);
            if (!TeleportDestinationPolicy.Evaluate(point, context.OriginId, Forbidden).Eligible)
                return new WorldMapPointSpellAction[0];
            var sources = TeleportationSpellbookAdapter.Enumerate(context.Player).Select(value => value.Snapshot)
                // A native visited flag with no recorded/migrated count cannot supply
                // Teleport odds. Exact spells can still use proven native visited state.
                .Where(value => value.Spell != TeleportSpellKind.Teleport ||
                    (TeleportationCastExecution.FamiliarityFor(context, point.Id) != TeleportFamiliarity.Unvisited &&
                    (TeleportRollTable.For(TeleportationCastExecution.FamiliarityFor(context, point.Id)).MishapPercent == 0 ||
                    TeleportationMishapDamageTarget.CanApply(TeleportationTravelers.Read(context.Player)))))
                .Where(value => context.Recall.Known || value.Spell != TeleportSpellKind.WordOfRecall);
            return WorldMapPointSpellActionComposer.Compose(new object[0], point, context.OriginId,
                context.Blocks, sources, Forbidden, context.Recall.Established, context.Recall.DestinationId).SpellActions;
        }

        internal static WordOfRecallCapitalState ReadRecall(Player player)
        {
            var root = KingdomRoot.Instance;
            if (player == null || root == null || root.BlueprintRegionCapital == null ||
                !root.BlueprintRegionCapital.SettlementIsPrebuilt)
                return WordOfRecallCapitalState.Unknown;
            if (player.Kingdom == null) return WordOfRecallCapitalState.Before;
            var regions = player.Kingdom.Regions.Where(value => value != null &&
                ReferenceEquals(value.Blueprint, root.BlueprintRegionCapital)).ToArray();
            if (regions.Length != 1) return WordOfRecallCapitalState.Unknown;
            var region = regions[0];
            var settlement = region.Settlement;
            return WordOfRecallCapitalState.Resolve(region.IsClaimed,
                settlement != null && ReferenceEquals(settlement.Region, region),
                settlement == null || settlement.Location == null ? null : settlement.Location.AssetGuid);
        }

        private static bool HasCurrentAnchor(GlobalMapLocation anchor, GlobalMapRules rules, BlueprintLocation blueprint)
        {
            return anchor != null && rules != null && blueprint != null && anchor.GetType() == typeof(GlobalMapLocation) &&
                ReferenceEquals(anchor.Blueprint, blueprint) && ReferenceEquals(rules.GetLocationObject(blueprint), anchor) &&
                ReferenceEquals(ResourcesLibrary.TryGetBlueprint<BlueprintLocation>(blueprint.AssetGuid), blueprint) &&
                anchor.isActiveAndEnabled && anchor.gameObject.scene.IsValid() && anchor.gameObject.scene.isLoaded &&
                anchor.gameObject.scene == rules.gameObject.scene && anchor.transform != null && Finite(anchor.transform.position) &&
                GlobalMapLocation.Instances.Count(value => value != null && value.Blueprint != null &&
                    value.Blueprint.AssetGuid == blueprint.AssetGuid) == 1;
        }
        private static bool Finite(Vector3 value)
        { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        private static TeleportPointKind Kind(LocationType value)
        {
            switch (value)
            {
                case LocationType.Location: return TeleportPointKind.Location;
                case LocationType.HiddenLocation: return TeleportPointKind.HiddenLocation;
                case LocationType.Landmark: return TeleportPointKind.Landmark;
                case LocationType.Waypoint: return TeleportPointKind.Waypoint;
                case LocationType.SystemWaypoint: return TeleportPointKind.SystemWaypoint;
                default: return TeleportPointKind.Unknown;
            }
        }
    }
}
