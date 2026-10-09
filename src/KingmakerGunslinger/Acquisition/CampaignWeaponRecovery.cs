using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Loot;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.View.MapObjects;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.Acquisition
{
    internal sealed class CampaignWeaponRecoveryInspection
    {
        internal CampaignWeaponPlacement Placement;
        internal string GameId, Refusal;
        internal int Chapter;
        internal bool DestinationVisited, Recorded, OwnershipInspectionComplete;
        internal readonly List<string> OwnedCopies = new List<string>();
        internal readonly List<string> ContainerStates = new List<string>();
        internal bool CanRecover { get { return Refusal == null; } }
        internal string Describe()
        {
            return Placement.Item.Name + ";item=" + Placement.Item.AssetGuid + ";chapter=" + Chapter +
                ";destinationVisited=" + DestinationVisited + ";ledgerRecorded=" + Recorded +
                ";ownershipInspectionComplete=" + OwnershipInspectionComplete + ";owned=" +
                string.Join(" | ", OwnedCopies) + ";containers=" + string.Join(" | ", ContainerStates) +
                ";decision=" + (Refusal ?? "Eligible for one explicitly acknowledged recovery") +
                ". " + CampaignWeaponRecoveryPolicy.HistoricalUncertainty;
        }
    }

    internal static class CampaignWeaponRecovery
    {
        internal static CampaignWeaponRecoveryInspection Inspect(string key)
        {
            var game = Game.Instance;
            var player = game == null ? null : game.Player;
            var main = player == null ? null : player.MainCharacter.Value;
            if (main == null || player.Inventory == null || player.SharedStash == null)
                throw new InvalidOperationException("An active campaign with its canonical owner, inventory and stash is required.");
            var spec = CampaignWeaponRegistry.Require(key);
            var result = new CampaignWeaponRecoveryInspection { Placement = spec,
                GameId = player.GameId, Chapter = player.Chapter, OwnershipInspectionComplete = true };
            var target = BlueprintLibraryLookup.RequireExact<BlueprintLoot>(BlueprintBootstrap.Library,
                spec.Location.TargetGuid, spec.Location.TargetName);
            if (target.Area == null || target.Area.name != spec.Location.AreaName || target.name != spec.Location.TargetName)
                throw new InvalidOperationException("Recovery target identity differs from publication.");
            result.DestinationVisited = player.VisitedAreasData.ContainsKey(target.Area);
            var part = main.Descriptor.Get<UnitPartCampaignWeaponRecovery>();
            result.Recorded = part != null && part.HasRecord(spec.Item.AssetGuid);
            Action<ItemsCollection, string> scan = (collection, where) =>
            {
                if (collection == null) { result.OwnershipInspectionComplete = false; return; }
                foreach (ItemEntity item in collection.Items)
                    if (CampaignWeaponRecoveryPolicy.IsOwnedIdentity(spec.Item.AssetGuid, item.Blueprint.AssetGuid))
                        result.OwnedCopies.Add(where + ";item=" + item.Blueprint.AssetGuid + ";count=" + item.Count);
            };
            scan(player.Inventory, "party shared inventory"); scan(player.SharedStash, "shared stash");
            var references = player.RemoteCompanions.Concat(player.ExCompanions).ToArray();
            if (references.Any(value => value.Value == null)) result.OwnershipInspectionComplete = false;
            UnitEntityData[] units = player.AllCharacters.Concat(player.AllCrossSceneUnits)
                .Concat(references.Select(value => value.Value).Where(value => value != null)).Concat(new[] { main }).Distinct().ToArray();
            foreach (UnitEntityData unit in units)
            {
                scan(unit.Inventory, "companion inventory " + unit.CharacterName + ":" + unit.UniqueId);
                if (unit.Body == null) { result.OwnershipInspectionComplete = false; continue; }
                foreach (var slot in unit.Body.AllSlots)
                    if (slot.HasItem && CampaignWeaponRecoveryPolicy.IsOwnedIdentity(spec.Item.AssetGuid, slot.Item.Blueprint.AssetGuid))
                        result.OwnedCopies.Add("equipment " + unit.CharacterName + ":" + unit.UniqueId +
                            ";slot=" + slot.GetType().Name + ";item=" + slot.Item.Blueprint.AssetGuid + ";count=" + slot.Item.Count);
            }
            string available = null;
            LootComponent[] objects = CampaignWeaponSceneObservation.Find(spec.Location.TargetGuid);
            foreach (LootComponent loot in objects)
            {
                if (loot.MapObject == null || loot.MapObject.Data == null)
                { result.ContainerStates.Add("UNVERIFIED: scene object has no persistent entity"); continue; }
                bool present = loot.Loot.Items.Any(value => CampaignWeaponRecoveryPolicy.IsOwnedIdentity(spec.Item.AssetGuid, value.Blueprint.AssetGuid));
                string description = loot.gameObject.scene.name + ":" + loot.MapObject.Data.UniqueId +
                    ";viewed=" + loot.LootViewed + ";weaponPresent=" + present + ";canInteract=" + loot.CanInteract();
                result.ContainerStates.Add(description);
                if (present) available = description;
            }
            // A generated retired source can still hold a valid copy in this
            // loaded area. Do not look only at the new destination, and do not
            // regenerate inventories while inspecting the scene.
            foreach (var loot in UnityEngine.Resources.FindObjectsOfTypeAll<LootComponent>().Where(value =>
                value != null && value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded &&
                value.MapObject != null && value.MapObject.Data != null && !objects.Contains(value)))
                if (loot.Loot.Items.Any(value => CampaignWeaponRecoveryPolicy.IsOwnedIdentity(spec.Item.AssetGuid, value.Blueprint.AssetGuid)))
                {
                    available = "loaded historical scene " + loot.gameObject.scene.name + ":" + loot.MapObject.Data.UniqueId;
                    result.ContainerStates.Add(available);
                }
            // Examine cached persistent container inventories without loading an
            // area, running loot generation or resolving a save's old history.
            foreach (var area in game.State.SavedAreaStates)
                foreach (var entity in area.AllEntityData.OfType<MapObjectEntityData>())
                {
                    var data = entity.GetComponentData<LootComponent.LootPersistentData>();
                    if (data == null || data.Loot == null) continue;
                    if (data.Loot.Items.Any(value => CampaignWeaponRecoveryPolicy.IsOwnedIdentity(spec.Item.AssetGuid, value.Blueprint.AssetGuid)))
                        available = "cached scene " + area.AreaGuid + ":" + entity.UniqueId;
                }
            if (objects.Length == 0) result.ContainerStates.Add("UNVERIFIED: destination object is not loaded; old generation and historical ownership unknown");
            result.Refusal = CampaignWeaponRecoveryPolicy.Refusal(spec.Relocated, spec.ModuleEnabled,
                result.OwnershipInspectionComplete, result.DestinationVisited, result.Recorded,
                result.OwnedCopies.FirstOrDefault(), available);
            return result;
        }

        internal static string Grant(string key, string expectedGameId, bool acknowledgeHistoricalUncertainty)
        {
            var inspection = Inspect(key); // Always repeat immediately before mutation.
            if (!inspection.CanRecover) return "Recovery refused. " + inspection.Describe();
            if (!acknowledgeHistoricalUncertainty || expectedGameId != inspection.GameId)
                return "Recovery refused: explicit historical-uncertainty acknowledgement and the inspected campaign are required.";
            var player = Game.Instance.Player;
            if (player.IsInCombat || Game.Instance.Vendor.IsTrading || Game.Instance.SaveManager.CommitInProgress)
                return "Recovery refused during combat, trading or a save commit.";
            var item = new ItemEntityWeapon(inspection.Placement.Item);
            var ledger = player.MainCharacter.Value.Descriptor.Ensure<UnitPartCampaignWeaponRecovery>();
            var record = ledger.Reserve(inspection.Placement.Item.AssetGuid,
                inspection.Placement.Location.TargetGuid, player.Chapter);
            // Reserve before Add: even an event-handler fault cannot make a
            // repeat invocation an unbounded grant. Failed records stay closed.
            try
            {
                player.Inventory.Add(item);
                if (item.Collection != player.Inventory || item.Count != 1)
                    throw new InvalidOperationException("Native one-item recovery transfer failed.");
                record.GrantedCount = item.Count; record.Status = "Granted";
                return "Recovered exactly one " + item.Blueprint.Name + ". Save normally to persist this weapon and its recovery record. " +
                    CampaignWeaponRecoveryPolicy.HistoricalUncertainty;
            }
            catch
            {
                record.Status = "FailedClosed";
                if (item.Collection == player.Inventory) player.Inventory.Remove(item);
                item.Dispose();
                throw;
            }
        }
    }
}
