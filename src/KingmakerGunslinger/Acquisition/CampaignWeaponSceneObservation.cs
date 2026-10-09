using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.DirectSerialization;
using Kingmaker.Blueprints.Loot;
using Kingmaker.View.MapObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingmakerGunslinger.Acquisition
{
    internal static class CampaignWeaponSceneObservation
    {
        internal static LootComponent[] Find(string targetGuid)
        {
            return Resources.FindObjectsOfTypeAll<LootComponent>().Where(value =>
                value != null && value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded &&
                (value.LootTables ?? new BlueprintReference[0]).Any(reference =>
                {
                    var target = reference == null ? null : reference.Get() as BlueprintLoot;
                    return target != null && target.AssetGuid == targetGuid;
                })).ToArray();
        }

        internal static object Capture(LootComponent loot)
        {
            var view = loot.MapObject;
            var data = view == null ? null : view.Data;
            return new
            {
                scene = loot.gameObject.scene.name,
                entityId = data == null ? null : data.UniqueId,
                hierarchy = Hierarchy(loot.transform),
                position = new[] { loot.transform.position.x, loot.transform.position.y, loot.transform.position.z },
                mapObjectPosition = view == null ? null : new[] { view.transform.position.x, view.transform.position.y, view.transform.position.z },
                activeInHierarchy = loot.gameObject.activeInHierarchy,
                isInGame = data == null ? (bool?)null : data.IsInGame,
                isRevealed = data == null ? (bool?)null : data.IsRevealed,
                perceptionDC = data == null ? null : data.PerceptionCheckDC,
                perceptionComponentDC = view == null || view.PerceptionCheckComponent == null ?
                    (int?)null : view.PerceptionCheckComponent.DC,
                perceptionPassed = data == null ? (bool?)null : data.IsPerceptionCheckPassed,
                enabled = data == null ? (bool?)null : loot.Enabled,
                canInteract = data == null ? (bool?)null : loot.CanInteract(),
                restrictions = loot.GetComponents<InteractionRestriction>().Select(value => value.GetType().FullName).ToArray(),
                restrictionDetails = loot.GetComponents<InteractionRestriction>().Select(Restriction).ToArray(),
                trap = loot.Trap == null ? null : loot.Trap.name,
                trapDetails = loot.Trap == null ? null : new {
                    entityId = loot.Trap.Data == null ? null : loot.Trap.Data.UniqueId,
                    blueprint = loot.Trap.Blueprint == null ? null : loot.Trap.Blueprint.AssetGuid,
                    active = loot.Trap.Data == null ? (bool?)null : loot.Trap.TrapActive,
                    perceptionDC = loot.Trap.PerceptionCheckComponent == null ? (int?)null : loot.Trap.PerceptionCheckComponent.DC,
                    perceptionPassed = loot.Trap.Data == null ? (bool?)null : loot.Trap.Data.IsPerceptionCheckPassed,
                    disableDC = loot.Trap.Data == null ? (int?)null : loot.Trap.Data.DisableDC },
                persistentStaticObject = view == null ? (bool?)null : !(view is DroppedLoot),
                containerType = loot.LootContainerType.ToString(),
                viewed = data == null ? (bool?)null : loot.LootViewed,
                items = data == null ? null : loot.Loot.Items.Select(value => new {
                    guid = value.Blueprint.AssetGuid, name = value.Blueprint.name, displayName = value.Blueprint.Name,
                    count = value.Count, runtimeObjectHash = value.GetHashCode() }).ToArray(),
                room = "UNVERIFIED; hierarchy and coordinates observed",
                approachRoute = "UNVERIFIED; presence is not route qualification",
                normalPickup = "UNVERIFIED", saveReload = "UNVERIFIED"
            };
        }

        internal static object Phase()
        {
            var game = Game.Instance;
            var flag = ResourcesLibrary.TryGetBlueprint<BlueprintUnlockableFlag>("db94ef898ad95944788d3da6b22a5e31");
            bool available = game != null && game.Player != null && flag != null;
            var firstEntry = ResourcesLibrary.TryGetBlueprint<BlueprintUnlockableFlag>("c553d372960fbad4090ac4f4e55df412");
            var lantern = ResourcesLibrary.TryGetBlueprint<BlueprintUnlockableFlag>("3a10fdff8ee7fdc40a3528c97492557f");
            return new { flagGuid = flag == null ? null : flag.AssetGuid,
                flagName = flag == null ? null : flag.name,
                unlocked = available ? (bool?)game.Player.UnlockableFlags.IsUnlocked(flag) : null,
                value = available && game.Player.UnlockableFlags.IsUnlocked(flag) ?
                    (int?)game.Player.UnlockableFlags.GetFlagValue(flag) : null,
                firstEntryFlagGuid = firstEntry == null ? null : firstEntry.AssetGuid,
                firstEntryUnlocked = available && firstEntry != null ?
                    (bool?)game.Player.UnlockableFlags.IsUnlocked(firstEntry) : null,
                lanternFlagGuid = lantern == null ? null : lantern.AssetGuid,
                lanternUnlocked = available && lantern != null ? (bool?)game.Player.UnlockableFlags.IsUnlocked(lantern) : null,
                lanternValue = available && lantern != null && game.Player.UnlockableFlags.IsUnlocked(lantern) ?
                    (int?)game.Player.UnlockableFlags.GetFlagValue(lantern) : null,
                loadedMechanicsScenes = Enumerable.Range(0, SceneManager.sceneCount)
                    .Select(SceneManager.GetSceneAt).Where(value => value.isLoaded &&
                        value.name.IndexOf("Mechanics", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(value => value.name).OrderBy(value => value, StringComparer.Ordinal).ToArray() };
        }

        internal static object Restriction(InteractionRestriction restriction)
        {
            var key = restriction as Kingmaker.View.MapObjects.InteractionRestrictions.KeyRestriction;
            var flag = restriction as Kingmaker.View.MapObjects.InteractionRestrictions.UnlockRestriction;
            var skill = restriction as Kingmaker.View.MapObjects.InteractionRestrictions.DisableDeviceRestriction;
            return new { type = restriction.GetType().FullName, dc = skill == null ? (int?)null : skill.DC,
                key = key == null || key.Key == null ? null : key.Key.AssetGuid,
                keyName = key == null || key.Key == null ? null : key.Key.name,
                ownedKeys = key == null || key.Key == null || Game.Instance == null || Game.Instance.Player == null ?
                    (int?)null : Game.Instance.Player.Inventory.Count(key.Key),
                flag = flag == null || flag.Flag == null ? null : flag.Flag.AssetGuid,
                unlocked = flag == null || flag.Flag == null ? (bool?)null : flag.Flag.IsUnlocked };
        }

        internal static string Hierarchy(Transform transform)
        {
            return transform == null ? string.Empty : transform.parent == null ? transform.name :
                Hierarchy(transform.parent) + "/" + transform.name;
        }
    }
}
