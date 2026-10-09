using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private object _weaponNativeFloorPrelude;
        private static Action<ScriptZone, UnitEntityData> _weaponZoneObservation;
        private static Action<BlueprintAreaEnterPoint, AutoSaveMode> _weaponLoadObservation;
        private static Action<BlueprintAreaEnterPoint> _weaponTeleportObservation;
        private static void ObserveWeaponNativeZone(ScriptZone __instance, UnitEntityData triggeringUnit)
        { try { if (_weaponZoneObservation != null) _weaponZoneObservation(__instance, triggeringUnit); } catch { } }
        private static void ObserveWeaponNativeLoad(BlueprintAreaEnterPoint areaEnterPoint, AutoSaveMode autoSaveMode)
        { try { if (_weaponLoadObservation != null) _weaponLoadObservation(areaEnterPoint, autoSaveMode); } catch { } }
        private static void ObserveWeaponNativeTeleport(BlueprintAreaEnterPoint areaEnterPoint)
        { try { if (_weaponTeleportObservation != null) _weaponTeleportObservation(areaEnterPoint); } catch { } }

        // These are closed, registered weapon routes. All movement, portal
        // conditions and actions belong to native scene objects. Observers do
        // not invoke actions, set zone flags, or reposition a character.
        private IEnumerable<int> ObserveWeaponNativeFloorPrelude(CampaignWeaponPlacement weapon)
        {
            _weaponNativeFloorPrelude = null;
            if (weapon.Key != "eastern:HeavensMeasure" && weapon.Key != "KMG.Firearms.DuelistsRebuttalItem" &&
                weapon.Key != "eastern:PaperLantern" && weapon.Key != "eastern:NightWithoutMoon" &&
                weapon.Key != "eastern:WorldTreeSeverer" && weapon.Key != "spear:SpearOfTheFirstBranch" &&
                weapon.Key != "spear:BriarCrownedSpear") yield break;
            var game = Game.Instance; var actor = game.Player.MainCharacter.Value;
            string actorId = actor.UniqueId;
            var start = actor.Position; var records = new List<object>();
            var zoneMethod = typeof(ScriptZone).GetMethod("OnUnitEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            var loadMethod = typeof(Game).GetMethod("LoadArea", new[] { typeof(BlueprintAreaEnterPoint), typeof(AutoSaveMode) });
            var teleportMethod = typeof(Game).GetMethod("Teleport", new[] { typeof(BlueprintAreaEnterPoint) });
            var zoneObserver = typeof(RuntimeTestRunner).GetMethod(nameof(ObserveWeaponNativeZone), BindingFlags.NonPublic | BindingFlags.Static);
            var loadObserver = typeof(RuntimeTestRunner).GetMethod(nameof(ObserveWeaponNativeLoad), BindingFlags.NonPublic | BindingFlags.Static);
            var teleportObserver = typeof(RuntimeTestRunner).GetMethod(nameof(ObserveWeaponNativeTeleport), BindingFlags.NonPublic | BindingFlags.Static);
            if (zoneMethod == null || loadMethod == null || teleportMethod == null) throw new InvalidOperationException("Exact native route observers absent.");
            int fogEntries = 0; var loads = new List<string>(); var teleports = new List<string>(); bool passed = false;
            _weaponZoneObservation = (zone, unit) => {
                if (unit.UniqueId != actorId) return;
                if (zone.Blueprint != null && (zone.Blueprint.AssetGuid == "2d6586e00682e644181c9c9bf81eda27" || zone.Blueprint.AssetGuid == "b5d50b7c439571e4c89ebdecaf32eb71")) fogEntries++;
                records.Add(new { nativeEvent = "ScriptZone.OnUnitEnter", blueprint = zone.Blueprint == null ? null : zone.Blueprint.AssetGuid,
                    name = zone.name, entity = zone.Data == null ? null : zone.Data.UniqueId,
                    instance = zone.GetInstanceID(), scene = zone.gameObject.scene.name,
                    position = new[] { actor.Position.x, actor.Position.y, actor.Position.z },
                    phaseBeforeNativeActions = CampaignWeaponSceneObservation.Phase() });
            };
            _weaponLoadObservation = (entry, mode) => {
                loads.Add(entry.AssetGuid);
                records.Add(new { nativeEvent = "Game.LoadArea", entry = entry.AssetGuid, name = entry.name,
                    autoSave = mode.ToString(), caller = Environment.StackTrace });
            };
            _weaponTeleportObservation = entry => {
                teleports.Add(entry.AssetGuid);
                records.Add(new { nativeEvent = "Game.Teleport", entry = entry.AssetGuid, name = entry.name,
                    caller = Environment.StackTrace });
            };
            _context.Harmony.Patch(zoneMethod, new HarmonyMethod(zoneObserver), null, null);
            _context.Harmony.Patch(loadMethod, new HarmonyMethod(loadObserver), null, null);
            _context.Harmony.Patch(teleportMethod, new HarmonyMethod(teleportObserver), null, null);
            bool paused = game.IsPaused;
            try
            {
                records.Add(new { nativeSceneInteractionsBeforePrelude = WeaponRouteSceneInteractions(),
                    nativeGeometryBeforePrelude = WeaponRouteWalkingGeometry(),
                    method = "Read-only installed entrance object and walking geometry observation" });
                if (weapon.Key == "spear:BriarCrownedSpear")
                {
                    var gate = Resources.FindObjectsOfTypeAll<InteractionSkillCheck>().Single(value => value != null &&
                        value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy &&
                        value.gameObject.scene.name == "BlakemoorHideoutEntrance_Mechanics" &&
                        value.MapObject.Data.UniqueId == "6f67ed9a-2c32-4ef9-a531-12da90889e73");
                    records.Add(new { nativeGate = gate.MapObject.Data.UniqueId, name = gate.name,
                        scene = gate.gameObject.scene.name, interactionType = gate.Type.ToString(),
                        configuredSkill = gate.Skill.ToString(), configuredDC = gate.DC,
                        position = new[] { gate.transform.position.x, gate.transform.position.y, gate.transform.position.z },
                        method = "Exterior native door dialog and real Trickery 35 check; no reveal, key or check outcome override" });
                    if (gate.Type != InteractionType.Approach || !gate.CanInteract() ||
                        gate.Skill != Kingmaker.EntitySystem.Stats.StatType.Unknown || gate.DC != 0)
                        throw new InvalidOperationException("Blakemoor's native gate is not an available Approach interaction.");
                    var check = BlueprintLibraryLookup.RequireExact<Kingmaker.DialogSystem.Blueprints.BlueprintCheck>(
                        BlueprintBootstrap.Library, "ecebe4d2c51debd47924bcca305f8268", "Check_0015");
                    if (check.Type != Kingmaker.EntitySystem.Stats.StatType.SkillThievery || check.DC != 35 ||
                        check.Success.AssetGuid != "8661dc38aa813a5489adf140d869a7b9")
                        throw new InvalidOperationException("Blakemoor's installed native dialog check differs from the audited contract.");
                    records.Add(new { nativeDialogCheck = check.AssetGuid, skill = check.Type.ToString(), check.DC,
                        successCue = check.Success.AssetGuid,
                        method = "Read-only native dialogue check definition; outcome must come from normal door interaction and offered answers" });
                    var successCue = BlueprintLibraryLookup.RequireExact<Kingmaker.DialogSystem.Blueprints.BlueprintCue>(
                        BlueprintBootstrap.Library, "8661dc38aa813a5489adf140d869a7b9", "Cue_0017");
                    var transfer = successCue.OnStop.Actions.OfType<Kingmaker.Designers.EventConditionActionSystem.Actions.TeleportParty>().Single();
                    if (transfer.exitPositon.AssetGuid != "1a3b0d779d9d2a0479bcff5ab9776ed5" ||
                        transfer.AfterTeleport.Actions.Length != 0)
                        throw new InvalidOperationException("Blakemoor's native success transfer differs from the audited contract.");
                    var originalSave = transfer.AutoSaveMode;
                    transfer.AutoSaveMode = AutoSaveMode.None; // leased fixture, same isolation as native stair transfers
                    try
                    {
                        foreach (int tick in ApproachWeaponNativeInteraction(weapon, gate, records)) yield return tick;
                        foreach (int tick in DriveWeaponPreludeCommand(weapon,
                            new UnitInteractWithObject(gate) { CreatedByPlayer = true },
                            () => loads.Contains("1a3b0d779d9d2a0479bcff5ab9776ed5") && game.CurrentMode == GameModeType.Default &&
                                game.CurrentlyLoadedArea == transfer.exitPositon.Area && AreaEnterPoint.Instances.Any(value =>
                                    value.Blueprint != null && value.Blueprint.AssetGuid == transfer.exitPositon.AssetGuid &&
                                    Vector3.Distance(game.Player.MainCharacter.Value.Position, value.transform.position) < 10f),
                            () => loads.Contains("1a3b0d779d9d2a0479bcff5ab9776ed5"), records)) yield return tick;
                        records.Add(new { nativeGateEntry = transfer.exitPositon.AssetGuid,
                            originalAutoSave = originalSave.ToString(), actualFixtureAutoSave = transfer.AutoSaveMode.ToString(),
                            qualification = "PASS; native offered pick-lock answer and native success-cue area load" });
                    }
                    finally { transfer.AutoSaveMode = originalSave; }
                }
                else if (weapon.Key == "eastern:HeavensMeasure")
                {
                    var phase = BlueprintLibraryLookup.RequireExact<BlueprintUnlockableFlag>(BlueprintBootstrap.Library,
                        "db94ef898ad95944788d3da6b22a5e31", "Phase");
                    var lantern = game.Player.Inventory.Items.OfType<ItemEntityUsable>().Single(value =>
                        value.Blueprint.AssetGuid == "da0b70ce16aae6149a720670761271aa");
                    if (game.Player.UnlockableFlags.GetFlagValue(phase) != 1 || lantern.ActivatableAbility.IsOn ||
                        actor.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid == "ed56d0930d7a7ab479485abda4658ee5"))
                        throw new InvalidOperationException("Native fog route needs measured Phase1 with equipped lantern off.");
                    var zones = Resources.FindObjectsOfTypeAll<ScriptZone>().Where(value => value != null && value.Blueprint != null &&
                        value.Blueprint.AssetGuid == "b5d50b7c439571e4c89ebdecaf32eb71" && value.IsActive &&
                        value.gameObject.activeInHierarchy && value.gameObject.scene.isLoaded).OrderBy(value => Vector3.Distance(start, value.transform.position)).ToArray();
                    if (zones.Length == 0) throw new InvalidOperationException("Exact active native fog transition absent.");
                    var zone = zones[0];
                    var walk = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(zone.transform.position).clampedPosition) { CreatedByPlayer = true };
                    foreach (int tick in DriveWeaponPreludeCommand(weapon, walk,
                        () => fogEntries > 0 && game.Player.UnlockableFlags.GetFlagValue(phase) == 2 &&
                            teleports.Contains("cd630d28576ad0c46a3cfdd96a64d3bf") && game.CurrentMode == GameModeType.Default &&
                            AreaEnterPoint.Instances.Any(value => value.Blueprint != null && value.Blueprint.AssetGuid == "cd630d28576ad0c46a3cfdd96a64d3bf" &&
                                Vector3.Distance(game.Player.MainCharacter.Value.Position, value.transform.position) < 8f),
                        () => teleports.Contains("cd630d28576ad0c46a3cfdd96a64d3bf"), records)) yield return tick;
                    // Stop further fog switches through the native quick-slot
                    // ability, after its own zone has actually changed phase.
                    lantern.ActivatableAbility.IsOn = true;
                    var wait = Stopwatch.StartNew();
                    while ((!actor.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid == "ed56d0930d7a7ab479485abda4658ee5") ||
                        game.CurrentMode != GameModeType.Default) && wait.Elapsed.TotalSeconds < 30) yield return 0;
                    if (game.Player.UnlockableFlags.GetFlagValue(phase) != 2 ||
                        !actor.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid == "ed56d0930d7a7ab479485abda4658ee5"))
                        throw new InvalidOperationException("Native fog/lantern phase stabilization was not measured.");
                    // Switch on the first floor, then use the actual ordinary
                    // stairs. The second-floor F01 teleport reaches a different
                    // pocket and cannot stand in for this cabinet's approach.
                    foreach (int tick in DriveWeaponNativeStairs(weapon, "b55aff9ef7331934fba99508b754281e", loads, records)) yield return tick;
                    // The measured cabinet is behind Nyrissa's room. Reach its
                    // lower hall through native fog, satisfy the real three-key
                    // zone, and open the ordinary door. Native keys are explicit
                    // disposable prerequisites, never a flag/check override.
                    foreach (int tick in DriveHeavenNativeInnerHall(weapon, lantern, phase, teleports, records)) yield return tick;
                }
                else if (weapon.Key == "eastern:PaperLantern" || weapon.Key == "eastern:NightWithoutMoon")
                {
                    string entity = weapon.Key == "eastern:PaperLantern" ?
                        "eb8515d5-e0b4-4f08-973f-bc3e1af8bff1" : "a624fb7e-280e-4812-97d7-758ff39c53ce";
                    foreach (int tick in DriveWeaponNativeSkill(weapon, entity, records)) yield return tick;
                }
                else if (weapon.Key == "eastern:WorldTreeSeverer" || weapon.Key == "spear:SpearOfTheFirstBranch")
                {
                    string entity = weapon.Key == "eastern:WorldTreeSeverer" ?
                        "7372cea5-3a85-41f2-86c1-084b520bc29f" : "13680704-4407-4862-8ad6-4e86d46793ce";
                    string[] destinations = weapon.Key == "eastern:WorldTreeSeverer" ?
                        new[] { "b102db6289008624b81c0dbb2ab46b1a", "536058d60210b824fb984e55b5daa66d", "78a0ebba49f469845abcd5c75f52bbca", "8b6988cd635856347905495d134d94eb" } :
                        new[] { "d0dd534ada15ec64996b0777da959d52", "61106363e2b2095498c997c45c7e1776" };
                    foreach (int tick in DriveWeaponNativeVine(weapon, entity, destinations, teleports, records)) yield return tick;
                    if (weapon.Key == "spear:SpearOfTheFirstBranch")
                    {
                        // Cube 1 reaches the optional lower pocket. Its native
                        // return button restores the upper walking route to the
                        // northern treasure; neither teleport is fixture-driven.
                        foreach (int tick in DriveWeaponNativeVine(weapon, "6411331f-f93c-4f3b-9cad-243d7151dc04",
                            destinations, teleports, records)) yield return tick;
                        // The installed road geometry establishes the ordinary
                        // ramp approach. A partial path to the isolated loot
                        // area cannot substitute for walking along that road.
                        foreach (var hierarchy in new[] {
                            "final_dungeon_outdoor_level_02/floor/broken_road_01RNgroup7",
                            "final_dungeon_outdoor_level_02/floor/broken_road_01RNgroup4",
                            "final_dungeon_outdoor_level_02/floor/broken_road_03RNgroup2",
                            "final_dungeon_outdoor_level_02/floor/broken_road_big_01RNgroup9" })
                        {
                            var road = Resources.FindObjectsOfTypeAll<Transform>().Single(value => value != null &&
                                value.gameObject.scene.name == "FinalDungeon2_Static" && value.gameObject.activeInHierarchy &&
                                CampaignWeaponSceneObservation.Hierarchy(value) == hierarchy);
                            var point = ObstacleAnalyzer.GetNearestNode(road.position).clampedPosition;
                            Func<UnitCommand> walk = () => new UnitMoveTo(point) { CreatedByPlayer = true };
                            foreach (int tick in DriveWeaponPreludeCommand(weapon, walk(),
                                () => Vector3.Distance(game.Player.MainCharacter.Value.Position, point) < 3f &&
                                    !game.Player.IsInCombat && game.CurrentMode == GameModeType.Default,
                                () => false, records, walk)) yield return tick;
                            records.Add(new { nativeRoadWaypoint = hierarchy,
                                point = new[] { point.x, point.y, point.z },
                                method = "Native walking to measured persistent road geometry; no path or position override" });
                        }
                    }
                }
                else
                {
                    foreach (int tick in DriveWeaponNativeStairs(weapon, "97ed5b9b2ca77234084c77518ff2c902", loads, records)) yield return tick;
                    foreach (int tick in DriveStockadeNativeRooms(weapon, records)) yield return tick;
                    foreach (int tick in DriveWeaponNativeStairs(weapon, "5be3f25ee79399a47857015432bc3a8d", loads, records)) yield return tick;
                }
                passed = true;
            }
            finally
            {
                _weaponZoneObservation = null; _weaponLoadObservation = null; _weaponTeleportObservation = null;
                _context.Harmony.Unpatch(zoneMethod, zoneObserver); _context.Harmony.Unpatch(loadMethod, loadObserver);
                _context.Harmony.Unpatch(teleportMethod, teleportObserver);
                game.IsPaused = paused;
                actor = game.Player.MainCharacter.Value;
                records.Add(new { nativeSceneInteractionsAfterPrelude = WeaponRouteSceneInteractions(),
                    nativeGeometryAfterPrelude = WeaponRouteWalkingGeometry(),
                    method = "Read-only final native route state; missing route evidence remains unverified" });
                _weaponNativeFloorPrelude = new { key = weapon.Key, start = new[] { start.x, start.y, start.z },
                    finish = new[] { actor.Position.x, actor.Position.y, actor.Position.z }, records,
                    phase = CampaignWeaponSceneObservation.Phase(), qualification = passed ? "PASS" : "UNVERIFIED",
                    method = "Native walking into actual scene zone or native group stair transition; native actions observed, never invoked by fixture" };
                _weaponFindabilityRecords.Add(new { key = weapon.Key, nativeFloorPrelude = _weaponNativeFloorPrelude });
            }
        }

        private IEnumerable<int> DriveStockadeNativeRooms(CampaignWeaponPlacement weapon, List<object> records)
        {
            var game = Game.Instance;
            // Installed ordinary doors along the documented southern-room
            // approach. The stuffed-bear shortcut must open through its native button.
            foreach (string entity in new[] {
                "4efaea98-e85b-4399-a86e-d79c26bedfa0", "f206421d-2ed2-42f6-bf82-a1d851ea6f6e",
                "30f73b8a-f7ed-461c-8cfa-0b9bcdb5ef10", "563944db-eed0-4dca-80ba-b47b545d76aa",
                "e2e04a9c-b8f8-4f4f-a382-153de7c4d539", "fea353cd-9b33-4153-a46f-bec023f168fa" })
            {
                var door = Resources.FindObjectsOfTypeAll<StandardDoor>().Single(value => value != null &&
                    value.MapObject != null && value.MapObject.Data != null && value.MapObject.Data.UniqueId == entity &&
                    value.gameObject.scene.name == "VarnholdStockade_FirstFloor_Mechanics" && value.gameObject.activeInHierarchy);
                if (door.MapObject.PerceptionCheckComponent != null || door.Trap != null ||
                    door.GetComponents<InteractionRestriction>().Length != 0)
                    throw new InvalidOperationException("The measured stockade room door differs from its ordinary scene contract: " + entity);
                var actor = game.Player.MainCharacter.Value;
                var origin = actor.Position;
                var toward = Vector3.ProjectOnPlane(door.transform.position - origin, Vector3.up);
                var near = ObstacleAnalyzer.GetNearestNode(door.transform.position - toward.normalized * 3f).clampedPosition;
                records.Add(new { nativeGuidedRoomDoor = entity, door.IsOpen,
                    actorPosition = new[] { origin.x, origin.y, origin.z },
                    componentPosition = new[] { door.transform.position.x, door.transform.position.y, door.transform.position.z },
                    point = new[] { near.x, near.y, near.z },
                    method = "Native walking along measured ordinary southern-room doors; no shortcut, path or position override" });
                Func<UnitCommand> approach = () => new UnitMoveTo(near) { CreatedByPlayer = true };
                foreach (int tick in DriveWeaponPreludeCommand(weapon, approach(),
                    () => Vector3.Distance(game.Player.MainCharacter.Value.Position, door.transform.position) < 4.1f &&
                        Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(door.MapObject.gameObject) &&
                        game.CurrentMode == GameModeType.Default && !game.Player.IsInCombat,
                    () => false, records, approach, false)) yield return tick;
                origin = game.Player.MainCharacter.Value.Position;
                if (!door.IsOpen)
                    foreach (int tick in DriveWeaponPreludeCommand(weapon, new UnitInteractWithObject(door) { CreatedByPlayer = true },
                        () => door.IsOpen, () => false, records,
                        () => new UnitInteractWithObject(door) { CreatedByPlayer = true }, false)) yield return tick;
                records.Add(new { nativeGuidedDoorOpened = entity, door.IsOpen,
                    nativePlayerInteractionAvailable = Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(door.MapObject.gameObject) });
                // The next measured door/button supplies the native walking
                // destination. An arbitrary offset across this door can lie
                // beyond another closed room and does not prove a valid path.
            }
            var bear = Resources.FindObjectsOfTypeAll<StandardButton>().Single(value => value != null &&
                value.MapObject != null && value.MapObject.Data != null &&
                value.MapObject.Data.UniqueId == "3c0e345a-2dfc-4e30-a42e-3d2573470492" &&
                value.gameObject.scene.name == "VarnholdStockade_FirstFloor_Mechanics" && value.gameObject.activeInHierarchy);
            var shortcut = Resources.FindObjectsOfTypeAll<StandardDoor>().Single(value => value != null &&
                value.MapObject != null && value.MapObject.Data != null &&
                value.MapObject.Data.UniqueId == "3343916b-c76b-43fa-aea6-5b4f3259ac2e" && value.gameObject.scene.isLoaded);
            var bearPoint = ObstacleAnalyzer.GetNearestNode(bear.transform.position).clampedPosition;
            Func<UnitCommand> walkBear = () => new UnitMoveTo(bearPoint) { CreatedByPlayer = true };
            foreach (int tick in DriveWeaponPreludeCommand(weapon, walkBear(),
                () => Vector3.Distance(game.Player.MainCharacter.Value.Position, bear.transform.position) < 4.1f &&
                    Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(bear.MapObject.gameObject) &&
                    game.CurrentMode == GameModeType.Default && !game.Player.IsInCombat,
                () => false, records, walkBear, false)) yield return tick;
            records.Add(new { nativeStuffedBearButton = bear.MapObject.Data.UniqueId, shortcutBefore = shortcut.IsOpen,
                method = "Normal walking and native interaction with installed far-side ButtonDoor; no shortcut action invoked directly" });
            if (!shortcut.IsOpen)
                foreach (int tick in DriveWeaponPreludeCommand(weapon, new UnitInteractWithObject(bear) { CreatedByPlayer = true },
                    () => shortcut.IsOpen, () => false, records,
                    () => new UnitInteractWithObject(bear) { CreatedByPlayer = true }, false)) yield return tick;
            records.Add(new { nativeReturnShortcut = shortcut.MapObject.Data.UniqueId, shortcut.IsOpen });
        }

        private IEnumerable<int> DriveHeavenNativeFog(CampaignWeaponPlacement weapon, ItemEntityUsable lantern,
            BlueprintUnlockableFlag phase, string zoneName, int expectedPhase, List<string> teleports, List<object> records)
        {
            var game = Game.Instance;
            lantern.ActivatableAbility.IsOn = false;
            var wait = Stopwatch.StartNew();
            while (game.Player.MainCharacter.Value.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid == "ed56d0930d7a7ab479485abda4658ee5") &&
                wait.Elapsed.TotalSeconds < 30) yield return 0;
            if (game.Player.MainCharacter.Value.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid == "ed56d0930d7a7ab479485abda4658ee5"))
                throw new InvalidOperationException("Native lantern did not turn off before the measured fog route.");
            var zone = Resources.FindObjectsOfTypeAll<ScriptZone>().Single(value => value != null && value.Blueprint != null &&
                value.gameObject.scene.name == "HouseAtTheEdgeOfTime_2ndFloor_Mechanics" && value.name == zoneName &&
                value.Blueprint.AssetGuid == "2d6586e00682e644181c9c9bf81eda27" && value.IsActive && value.gameObject.activeInHierarchy);
            int before = teleports.Count(value => value == "5f580fca7459cf242bb6f70e1ea9f5b4");
            var point = ObstacleAnalyzer.GetNearestNode(zone.transform.position).clampedPosition;
            records.Add(new { nativeFogApproach = zone.Data.UniqueId, name = zoneName,
                blueprint = zone.Blueprint.AssetGuid, expectedPhase,
                point = new[] { point.x, point.y, point.z }, method = "Native walking into exact active fog zone with native lantern off" });
            Func<UnitCommand> walk = () => new UnitMoveTo(point) { CreatedByPlayer = true };
            foreach (int tick in DriveWeaponPreludeCommand(weapon, walk(),
                () => teleports.Count(value => value == "5f580fca7459cf242bb6f70e1ea9f5b4") > before &&
                    game.Player.UnlockableFlags.GetFlagValue(phase) == expectedPhase && game.CurrentMode == GameModeType.Default &&
                    AreaEnterPoint.Instances.Any(value => value.Blueprint != null && value.Blueprint.AssetGuid == "5f580fca7459cf242bb6f70e1ea9f5b4" &&
                        Vector3.Distance(game.Player.MainCharacter.Value.Position, value.transform.position) < 8f),
                () => teleports.Count(value => value == "5f580fca7459cf242bb6f70e1ea9f5b4") > before, records, walk)) yield return tick;
            lantern.ActivatableAbility.IsOn = true; wait.Restart();
            while (!game.Player.MainCharacter.Value.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid == "ed56d0930d7a7ab479485abda4658ee5") &&
                wait.Elapsed.TotalSeconds < 30) yield return 0;
            if (!game.Player.MainCharacter.Value.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid == "ed56d0930d7a7ab479485abda4658ee5"))
                throw new InvalidOperationException("Native lantern did not stabilize the measured lower hall.");
        }

        private IEnumerable<int> DriveHeavenNativeInnerHall(CampaignWeaponPlacement weapon, ItemEntityUsable lantern,
            BlueprintUnlockableFlag phase, List<string> teleports, List<object> records)
        {
            var game = Game.Instance;
            foreach (int tick in DriveHeavenNativeFog(weapon, lantern, phase, "FogWallF01", 1, teleports, records)) yield return tick;
            var allKeys = BlueprintLibraryLookup.RequireExact<BlueprintUnlockableFlag>(BlueprintBootstrap.Library,
                "98d18f52a1bfb4f4fa048f2eca55f7db", "GotAllKeys");
            var keyZone = Resources.FindObjectsOfTypeAll<ScriptZone>().Single(value => value != null && value.Blueprint != null &&
                value.Blueprint.AssetGuid == "6735998245bb1454a83a177d1a533f45" && value.name == "GotAllKeys1" &&
                value.gameObject.scene.name == "HouseAtTheEdgeOfTime_2ndFloor_Mechanics" && value.IsActive && value.gameObject.activeInHierarchy);
            var keyPoint = ObstacleAnalyzer.GetNearestNode(keyZone.transform.position).clampedPosition;
            records.Add(new { nativeKeyGateZone = keyZone.Data.UniqueId, blueprint = keyZone.Blueprint.AssetGuid,
                point = new[] { keyPoint.x, keyPoint.y, keyPoint.z }, before = allKeys.IsUnlocked,
                method = "Native walking into the actual three-key trigger; native conditions/actions, no flag override" });
            Func<UnitCommand> walkKeys = () => new UnitMoveTo(keyPoint) { CreatedByPlayer = true };
            foreach (int tick in DriveWeaponPreludeCommand(weapon, walkKeys(), () => allKeys.IsUnlocked,
                () => false, records, walkKeys)) yield return tick;
            records.Add(new { nativeKeyGate = allKeys.AssetGuid, unlocked = allKeys.IsUnlocked });
            foreach (int tick in DriveHeavenNativeFog(weapon, lantern, phase, "FogWallF04", 2, teleports, records)) yield return tick;
            var door = Resources.FindObjectsOfTypeAll<StandardDoor>().Single(value => value != null && value.MapObject != null &&
                value.MapObject.Data != null && value.MapObject.Data.UniqueId == "2b804a11-3caa-459a-b9e0-b776784050bf" &&
                value.gameObject.scene.name == "HouseAtTheEdgeOfTime_2ndFloor_Mechanics" && value.gameObject.activeInHierarchy);
            foreach (int tick in ApproachWeaponNativeInteraction(weapon, door, records)) yield return tick;
            var origin = game.Player.MainCharacter.Value.Position;
            foreach (int tick in DriveWeaponPreludeCommand(weapon, new UnitInteractWithObject(door) { CreatedByPlayer = true },
                () => door.IsOpen, () => false, records, () => new UnitInteractWithObject(door) { CreatedByPlayer = true })) yield return tick;
            records.Add(new { nativeNyrissaDoor = door.MapObject.Data.UniqueId, door.IsOpen,
                restrictions = door.GetComponents<InteractionRestriction>().Select(WeaponRouteRestriction).ToArray() });
            var across = WeaponNativeDoorCrossing(origin, door);
            Func<UnitCommand> cross = () => new UnitMoveTo(across) { CreatedByPlayer = true };
            foreach (int tick in DriveWeaponPreludeCommand(weapon, cross(),
                () => Vector3.Distance(game.Player.MainCharacter.Value.Position, across) < 2f &&
                    !game.Player.IsInCombat && game.CurrentMode == GameModeType.Default,
                () => false, records, cross)) yield return tick;
        }

        private IEnumerable<int> DriveWeaponNativeStairs(CampaignWeaponPlacement weapon, string entryGuid,
            List<string> loads, List<object> records)
        {
            var game = Game.Instance; var actor = game.Player.MainCharacter.Value;
            string actorId = actor.UniqueId;
            actor = game.Player.MainCharacter.Value;
            if (actor.UniqueId != actorId) throw new InvalidOperationException("Native floor load changed the persistent main character identity.");
            var transition = Resources.FindObjectsOfTypeAll<AreaTransition>().Single(value => value != null &&
                value.gameObject.activeInHierarchy && value.gameObject.scene.isLoaded &&
                value.AreaEnterPoint != null && value.AreaEnterPoint.AssetGuid == entryGuid);
            var targetArea = transition.AreaEnterPoint.Area;
            Func<bool> atDestination = () => game.CurrentlyLoadedArea == targetArea &&
                AreaEnterPoint.Instances.Any(value => value.Blueprint != null && value.Blueprint.AssetGuid == entryGuid &&
                    Vector3.Distance(game.Player.MainCharacter.Value.Position, value.transform.position) < 10f);
            if (weapon.Key == "eastern:HeavensMeasure")
            {
                // The ordinary upstairs approach goes through the throne room.
                // A distant stair click returned a partial path into a western
                // room, so walk through the measured ordinary door first.
                var door = Resources.FindObjectsOfTypeAll<StandardDoor>().Single(value => value != null &&
                    value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy &&
                    value.MapObject.Data.UniqueId == "76960d72-6839-45c9-9c9b-9bd3027b86ae");
                foreach (int tick in ApproachWeaponNativeInteraction(weapon, door, records)) yield return tick;
                foreach (int tick in DriveWeaponPreludeCommand(weapon,
                    new UnitInteractWithObject(door) { CreatedByPlayer = true }, () => door.IsOpen,
                    () => false, records, () => new UnitInteractWithObject(door) { CreatedByPlayer = true })) yield return tick;
                records.Add(new { nativeThroneRoomDoor = door.MapObject.Data.UniqueId, door.IsOpen });
                var throneApproach = ObstacleAnalyzer.GetNearestNode(new Vector3(-0.49f, 0.55f, -98.42f)).clampedPosition;
                Func<UnitCommand> walkToThrone = () => new UnitMoveTo(throneApproach) { CreatedByPlayer = true };
                foreach (int tick in DriveWeaponPreludeCommand(weapon, walkToThrone(),
                    () => Vector3.Distance(game.Player.MainCharacter.Value.Position, throneApproach) < 3f &&
                        !game.Player.IsInCombat && game.CurrentMode == GameModeType.Default,
                    () => false, records, walkToThrone)) yield return tick;
                records.Add(new { nativeThroneRoomApproach = new[] { throneApproach.x, throneApproach.y, throneApproach.z },
                    provenance = "Measured ordinary throne-room chest position; walking only, no loot transfer" });
            }
            if (entryGuid == "5be3f25ee79399a47857015432bc3a8d" || weapon.Key == "eastern:HeavensMeasure")
            {
                Func<UnitCommand> walkToStairs = () => new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(transition.transform.position).clampedPosition)
                    { CreatedByPlayer = true };
                foreach (int tick in DriveWeaponPreludeCommand(weapon, walkToStairs(),
                    () => Vector3.Distance(game.Player.MainCharacter.Value.Position, transition.transform.position) < 4f &&
                        ObstacleAnalyzer.GetArea(game.Player.MainCharacter.Value.Position) == ObstacleAnalyzer.GetArea(transition.transform.position),
                    () => false, records, walkToStairs)) yield return tick;
            }
            records.Add(new { transition = transition.MapObject.Data.UniqueId, name = transition.name,
                position = new[] { transition.transform.position.x, transition.transform.position.y, transition.transform.position.z },
                target = entryGuid, originalAutoSave = transition.AutoSaveMode.ToString() });
            var originalSave = transition.AutoSaveMode;
            transition.AutoSaveMode = AutoSaveMode.None; // only this leased disposable fixture
            AreaTransitionGroupCommand group = null;
            var commands = new List<UnitAreaTransition>();
            Func<UnitCommand> issue = () => {
                actor = game.Player.MainCharacter.Value;
                if (group != null && !group.IsFinished) group.Interrupt();
                var units = game.Player.GetPartyCharactersForGroupCommand(transition.transform.position, true);
                records.Add(new { nativeStairGroup = units.Select(unit => unit.UniqueId).ToArray(), actor = actor.UniqueId,
                    actor.Descriptor.State.CanMove, actor.Descriptor.State.CanAct, actor.IsDirectlyControllable,
                    actorNavArea = ObstacleAnalyzer.GetArea(actor.Position), stairsNavArea = ObstacleAnalyzer.GetArea(transition.transform.position) });
                if (units.Any(unit => unit.Descriptor.State.IsDead))
                    throw new InvalidOperationException("Native stair group includes a dead fixture party member; further combat preparation is required.");
                group = new AreaTransitionGroupCommand(units, transition);
                commands.Clear();
                UnitAreaTransition mainCommand = null;
                foreach (var unit in units)
                {
                    var next = new UnitAreaTransition(group, ObstacleAnalyzer.GetDeepNavmeshPoint(transition.transform.position, 1.5f)) { CreatedByPlayer = true };
                    commands.Add(next); unit.Commands.Run(next);
                    if (unit.UniqueId == actorId) mainCommand = next;
                }
                if (mainCommand == null) throw new InvalidOperationException("Native stair group omitted the canonical fixture actor.");
                return mainCommand;
            };
            try
            {
                foreach (int tick in DriveWeaponPreludeCommand(weapon, issue(),
                    () => group.Result == UnitCommand.ResultType.Success && loads.Contains(entryGuid) &&
                        game.CurrentMode == GameModeType.Default && atDestination(),
                    () => loads.Contains(entryGuid), records, issue)) yield return tick;
            }
            finally
            {
                if (group != null && !group.IsFinished) group.Interrupt();
                foreach (var command in commands) if (!command.IsFinished) command.Interrupt();
                if (transition != null) transition.AutoSaveMode = originalSave;
            }
        }

        private IEnumerable<int> ApproachWeaponNativeInteraction(CampaignWeaponPlacement weapon, InteractionComponent interaction, List<object> records)
        {
            Func<UnitCommand> walk = () => new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(interaction.transform.position).clampedPosition) { CreatedByPlayer = true };
            try
            {
            foreach (int tick in DriveWeaponPreludeCommand(weapon, walk(),
                () => Vector3.Distance(Game.Instance.Player.MainCharacter.Value.Position, interaction.transform.position) < 5f &&
                    Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(interaction.MapObject.gameObject) &&
                    !Game.Instance.Player.IsInCombat && Game.Instance.CurrentMode == GameModeType.Default,
                () => false, records, walk)) yield return tick;
            }
            finally
            {
                records.Add(new { entity = interaction.MapObject.Data.UniqueId,
                    nativePlayerInteractionAvailable = Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(interaction.MapObject.gameObject),
                    revealed = interaction.MapObject.Data.IsRevealed, perceptionPassed = interaction.MapObject.Data.IsPerceptionCheckPassed,
                    interaction.MapObject.Data.IsInGame, canInteract = interaction.CanInteract(),
                    rootInteractions = interaction.MapObject.Interactions.Select(value => value.GetType().FullName).ToArray(),
                    position = new[] { Game.Instance.Player.MainCharacter.Value.Position.x, Game.Instance.Player.MainCharacter.Value.Position.y, Game.Instance.Player.MainCharacter.Value.Position.z } });
            }
        }

        private IEnumerable<int> DriveWeaponNativeVine(CampaignWeaponPlacement weapon, string entity, string[] destinations,
            List<string> teleports, List<object> records)
        {
            var button = Resources.FindObjectsOfTypeAll<StandardButton>().Single(value => value != null &&
                value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy && value.MapObject != null &&
                value.MapObject.Data != null && value.MapObject.Data.UniqueId == entity);
            if (button.Type != InteractionType.Approach || !button.CanInteract())
                throw new InvalidOperationException("Registered vine does not provide a normal Approach interaction.");
            foreach (int tick in ApproachWeaponNativeInteraction(weapon, button, records)) yield return tick;
            int before = teleports.Count;
            Func<string> destination = () => teleports.Skip(before).LastOrDefault(destinations.Contains);
            records.Add(new { nativeVineEntity = entity, name = button.name, interactionType = button.Type.ToString(),
                method = "Normal native Approach command on the measured persistent vine button; Game.Teleport observed read-only" });
            foreach (int tick in DriveWeaponPreludeCommand(weapon,
                new UnitInteractWithObject(button) { CreatedByPlayer = true },
                () => destination() != null && Game.Instance.CurrentMode == GameModeType.Default &&
                    Vector3.Distance(Game.Instance.Player.MainCharacter.Value.Position, AreaEnterPoint.Instances.Single(value =>
                        value.Blueprint != null && value.Blueprint.AssetGuid == destination()).transform.position) < 8f,
                () => destination() != null, records)) yield return tick;
        }

        private IEnumerable<int> DriveWeaponNativeSkill(CampaignWeaponPlacement weapon, string entity, List<object> records)
        {
            var skill = Resources.FindObjectsOfTypeAll<InteractionSkillCheck>().Single(value => value != null &&
                value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy && value.MapObject != null &&
                value.MapObject.Data != null && value.MapObject.Data.UniqueId == entity);
            var actor = Game.Instance.Player.MainCharacter.Value;
            if (skill.Type != InteractionType.Approach || !skill.CanInteract())
                throw new InvalidOperationException("Registered native skill approach is not normally interactive.");
            foreach (int tick in ApproachWeaponNativeInteraction(weapon, skill, records)) yield return tick;
            var state = skill.Data as SkillCheckData;
            if (state == null || state.AlreadyUsed) throw new InvalidOperationException("Fresh native skill state unavailable.");
            string successEntry = skill.TeleportOnSuccess == null ? null : skill.TeleportOnSuccess.AssetGuid;
            var destination = successEntry == null ? null : AreaEnterPoint.Instances.Single(value =>
                value.Blueprint != null && value.Blueprint.AssetGuid == successEntry);
            records.Add(new { nativeSkillEntity = entity, name = skill.name, skill = skill.Skill.ToString(),
                configuredDC = skill.DC, nativeDC = state.DCOverride, successEntry,
                position = new[] { skill.transform.position.x, skill.transform.position.y, skill.transform.position.z },
                method = "Native Approach interaction and actual skill roll; native actions and registered teleport only" });
            try
            {
                foreach (int tick in DriveWeaponPreludeCommand(weapon,
                    new UnitInteractWithObject(skill) { CreatedByPlayer = true },
                    () => state.AlreadyUsed && state.CheckPassed && (destination == null ||
                        Vector3.Distance(Game.Instance.Player.MainCharacter.Value.Position, destination.transform.position) < 8f),
                    () => state.AlreadyUsed && state.CheckPassed && successEntry != null, records,
                    () => new UnitInteractWithObject(skill) { CreatedByPlayer = true })) yield return tick;
            }
            finally
            {
                records.Add(new { nativeSkillEntity = entity, state.AlreadyUsed, state.CheckPassed,
                    nativeDC = state.DCOverride, successEntry,
                    finish = new[] { actor.Position.x, actor.Position.y, actor.Position.z } });
            }
        }

        private IEnumerable<int> DriveWeaponPreludeCommand(CampaignWeaponPlacement weapon, UnitCommand command,
            Func<bool> completed, Func<bool> nativeJumpExpected, List<object> observations, Func<UnitCommand> retry = null,
            bool exploreOrdinaryDoors = true)
        {
            var game = Game.Instance; var actor = game.Player.MainCharacter.Value;
            string actorId = actor.UniqueId;
            var timer = Stopwatch.StartNew(); var prior = actor.Position; float distance = 0;
            var combat = new HashSet<string>(StringComparer.Ordinal); var combatRecords = new List<object>();
            var dialogs = new List<object>(); var points = new List<object>();
            var doorAttempts = new HashSet<string>(StringComparer.Ordinal);
            var doorScouts = new HashSet<string>(StringComparer.Ordinal);
            bool success = false; int retries = 0; double lastMovement = 0;
            try
            {
                // A group transition already queues commands for the full party.
                if (!(command is UnitAreaTransition)) actor.Commands.Run(command);
                game.IsPaused = false;
                while (timer.Elapsed.TotalSeconds < (!exploreOrdinaryDoors ? 90 : weapon.Key == "KMG.Firearms.DuelistsRebuttalItem" ? 600 : 180))
                {
                    actor = game.Player.MainCharacter.Value;
                    if (actor.UniqueId != actorId) throw new InvalidOperationException("Native route changed the persistent main character identity.");
                    if (completed()) { success = true; break; }
                    if (actor.Descriptor.State.IsDead) throw new InvalidOperationException("Native route prelude actor died.");
                    if (game.CurrentMode == GameModeType.Dialog && game.DialogController.Answers.Any(value => value.CanSelect()))
                    {
                        if (dialogs.Count >= 20 || !AdvanceWeaponRouteDialog(weapon, dialogs))
                            throw new InvalidOperationException("Native floor route requires an unqualified offered story choice.");
                        for (int i = 0; i < 12; i++) yield return 0;
                        continue;
                    }
                    if (game.CurrentMode != GameModeType.Default && game.CurrentMode != GameModeType.Pause)
                    { prior = actor.Position; yield return 0; continue; }
                    if (game.Player.IsInCombat) PrepareWeaponRouteCombat(combat, combatRecords);
                    if (game.IsPaused) game.IsPaused = false;
                    // The stockade shortcut is opened from the far side.
                    // Explore only actually visible ordinary doors on native
                    // walking paths, rather than targeting that shortcut.
                    if (exploreOrdinaryDoors && (weapon.Key == "eastern:HeavensMeasure" || weapon.Key == "KMG.Firearms.DuelistsRebuttalItem") &&
                        command is UnitMoveTo && (command.IsFinished || timer.Elapsed.TotalSeconds - lastMovement > 12) &&
                        !game.Player.IsInCombat && actor.Descriptor.State.CanAct && actor.Descriptor.State.CanMove && doorAttempts.Count < 8)
                    {
                        var door = AdjacentOrdinaryWeaponRouteDoors(actor, doorAttempts).FirstOrDefault(value =>
                            Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(value.MapObject.gameObject));
                        if (door != null)
                        {
                            var doorApproachOrigin = actor.Position;
                            doorAttempts.Add(door.MapObject.Data.UniqueId);
                            // Approach the actor's side of the measured doorway
                            // before its LOS-sensitive native interaction. A
                            // partial path must not hold room exploration in a
                            // long, never-started opening command.
                            var towardDoor = Vector3.ProjectOnPlane(door.transform.position - actor.Position, Vector3.up);
                            var nearDoor = ObstacleAnalyzer.GetNearestNode(door.transform.position - towardDoor.normalized * 3f).clampedPosition;
                            var approachDoor = new UnitMoveTo(nearDoor) { CreatedByPlayer = true };
                            actor.Commands.Run(approachDoor); var doorTimer = Stopwatch.StartNew();
                            while (!approachDoor.IsFinished && doorTimer.Elapsed.TotalSeconds < 20 &&
                                game.CurrentMode == GameModeType.Default)
                            {
                                if (game.Player.IsInCombat) PrepareWeaponRouteCombat(combat, combatRecords);
                                if (game.IsPaused) game.IsPaused = false;
                                float approachStep = Vector3.Distance(prior, actor.Position);
                                if (approachStep > 10f) throw new InvalidOperationException("Unqualified native doorway approach jump.");
                                distance += approachStep; prior = actor.Position; yield return 0;
                            }
                            if (!approachDoor.IsFinished) approachDoor.Interrupt();
                            var opening = new UnitInteractWithObject(door) { CreatedByPlayer = true };
                            actor.Commands.Run(opening); doorTimer.Restart();
                            while (!door.IsOpen && !opening.IsFinished && doorTimer.Elapsed.TotalSeconds < 20 &&
                                game.CurrentMode == GameModeType.Default)
                            {
                                if (game.Player.IsInCombat) PrepareWeaponRouteCombat(combat, combatRecords);
                                if (game.IsPaused) game.IsPaused = false;
                                float openingStep = Vector3.Distance(prior, actor.Position);
                                if (openingStep > 10f) throw new InvalidOperationException("Unqualified native doorway opening jump.");
                                distance += openingStep; prior = actor.Position; yield return 0;
                            }
                            if (!opening.IsFinished) opening.Interrupt();
                            observations.Add(new { nativeOrdinaryDoor = door.MapObject.Data.UniqueId, door.IsOpen,
                                approachResult = approachDoor.Result.ToString(), openingResult = opening.Result.ToString(),
                                componentPosition = new[] { door.transform.position.x, door.transform.position.y, door.transform.position.z },
                                rootPosition = new[] { door.MapObject.transform.position.x, door.MapObject.transform.position.y, door.MapObject.transform.position.z },
                                actualDistance = Vector3.Distance(actor.Position, door.transform.position),
                                nativePlayerInteractionAvailable = Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(door.MapObject.gameObject),
                                restrictions = door.GetComponents<InteractionRestriction>().Select(WeaponRouteRestriction).ToArray() });
                            if (!door.IsOpen)
                            {
                                command = retry == null ? new UnitMoveTo(command.ApproachPoint) { CreatedByPlayer = true } : retry();
                                actor.Commands.Run(command); prior = actor.Position; lastMovement = timer.Elapsed.TotalSeconds;
                                continue;
                            }
                            // Walk through the newly opened doorway before
                            // asking for a path to the distant stairs. Otherwise
                            // a partial path can return to the closed shortcut.
                            var across = WeaponNativeDoorCrossing(doorApproachOrigin, door);
                            Func<UnitCommand> cross = () => new UnitMoveTo(across) { CreatedByPlayer = true };
                            foreach (int tick in DriveWeaponPreludeCommand(weapon, cross(),
                                () => Vector3.Distance(game.Player.MainCharacter.Value.Position, across) < 2f &&
                                    !game.Player.IsInCombat && game.CurrentMode == GameModeType.Default,
                                () => false, observations, cross)) yield return tick;
                            observations.Add(new { nativeDoorCrossing = door.MapObject.Data.UniqueId,
                                point = new[] { across.x, across.y, across.z },
                                method = "Ordinary native walking across the actually opened doorway; no position or path override" });
                            command = retry == null ? new UnitMoveTo(command.ApproachPoint) { CreatedByPlayer = true } : retry();
                            actor.Commands.Run(command); prior = actor.Position; lastMovement = timer.Elapsed.TotalSeconds;
                            continue;
                        }
                    }
                    if (exploreOrdinaryDoors && weapon.Key == "KMG.Firearms.DuelistsRebuttalItem" && command is UnitMoveTo &&
                        (command.IsFinished || timer.Elapsed.TotalSeconds - lastMovement > 12) &&
                        !game.Player.IsInCombat && actor.Descriptor.State.CanAct && actor.Descriptor.State.CanMove && doorScouts.Count < 12)
                    {
                        var scout = AdjacentOrdinaryWeaponRouteDoors(actor, doorScouts, false).FirstOrDefault();
                        if (scout != null)
                        {
                            doorScouts.Add(scout.MapObject.Data.UniqueId);
                            var toward = Vector3.ProjectOnPlane(scout.transform.position - actor.Position, Vector3.up);
                            var point = ObstacleAnalyzer.GetNearestNode(scout.transform.position - toward.normalized * 4f).clampedPosition;
                            var scouting = new UnitMoveTo(point) { CreatedByPlayer = true };
                            actor.Commands.Run(scouting);
                            var scoutTimer = Stopwatch.StartNew();
                            while (!scouting.IsFinished && scoutTimer.Elapsed.TotalSeconds < 30 &&
                                !Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(scout.MapObject.gameObject) &&
                                game.CurrentMode != GameModeType.Dialog && game.CurrentMode != GameModeType.Cutscene)
                            {
                                if (game.Player.IsInCombat) PrepareWeaponRouteCombat(combat, combatRecords);
                                if (game.IsPaused) game.IsPaused = false;
                                float scoutStep = Vector3.Distance(prior, actor.Position);
                                if (scoutStep > 10f) throw new InvalidOperationException("Unqualified native stockade scouting jump.");
                                distance += scoutStep; prior = actor.Position; yield return 0;
                            }
                            observations.Add(new { nativeDoorScout = scout.MapObject.Data.UniqueId,
                                point = new[] { point.x, point.y, point.z }, result = scouting.Result.ToString(),
                                actualDistance = Vector3.Distance(actor.Position, scout.transform.position),
                                nativePlayerInteractionAvailable = Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(scout.MapObject.gameObject),
                                method = "Native ordinary room exploration before requiring player-visible door interaction; no reveal or path override" });
                            if (!scouting.IsFinished) scouting.Interrupt();
                            command = retry == null ? new UnitMoveTo(command.ApproachPoint) { CreatedByPlayer = true } : retry();
                            actor.Commands.Run(command); prior = actor.Position; lastMovement = timer.Elapsed.TotalSeconds;
                            continue;
                        }
                    }
                    if (command.IsFinished && command.Result == UnitCommand.ResultType.Interrupt &&
                        !game.Player.IsInCombat && !nativeJumpExpected())
                    {
                        if (retry == null || retries >= 6) throw new InvalidOperationException("Native walking prelude was interrupted and no qualified retry remains.");
                        observations.Add(new { nativeCommandRetry = ++retries, position = new[] { actor.Position.x, actor.Position.y, actor.Position.z },
                            command = command.GetType().FullName,
                            method = "Reissue the interrupted native command from the unchanged current position; interruption cause is not inferred" });
                        command = retry();
                        if (!(command is UnitAreaTransition)) actor.Commands.Run(command);
                    }
                    float step = Vector3.Distance(prior, actor.Position);
                    if (step > 10f)
                    {
                        observations.Add(new { nativePositionJump = new[] { actor.Position.x, actor.Position.y, actor.Position.z }, expected = nativeJumpExpected() });
                        if (!nativeJumpExpected()) throw new InvalidOperationException("Unqualified native prelude position jump.");
                    }
                    else distance += step;
                    if (step > 0.05f) lastMovement = timer.Elapsed.TotalSeconds;
                    prior = actor.Position;
                    if (points.Count == 0 || timer.Elapsed.TotalSeconds >= points.Count)
                    {
                        points.Add(new { seconds = timer.Elapsed.TotalSeconds, position = new[] { prior.x, prior.y, prior.z },
                            currentMode = game.CurrentMode.ToString(), gameTime = game.TimeController.GameTime.Ticks,
                            result = command.Result.ToString(), command.IsFinished, navigation = WeaponRouteNavigation(actor) });
                        WriteTeleportationForensicJson(System.IO.Path.Combine(_request.EvidenceDirectory, "weapon-native-floor-progress.json"),
                            new { runId = _request.RunId, key = weapon.Key, points, observations, dialogs, combatRecords });
                    }
                    yield return 0;
                }
                if (!success) throw new InvalidOperationException("Native floor transition did not complete within its disposable fixture bound.");
            }
            finally
            {
                if (!command.IsFinished) command.Interrupt();
                observations.Add(new { walkedDistance = distance, points, dialogs, combatRecords,
                    command = command.GetType().FullName, result = command.Result.ToString(), completed = success,
                    finalPosition = new[] { actor.Position.x, actor.Position.y, actor.Position.z },
                    requestedPoint = new[] { command.ApproachPoint.x, command.ApproachPoint.y, command.ApproachPoint.z },
                    nativeNavigationAreas = new { actor = ObstacleAnalyzer.GetArea(actor.Position), requested = ObstacleAnalyzer.GetArea(command.ApproachPoint) },
                    actualGoalDistance = Vector3.Distance(actor.Position, command.ApproachPoint) });
            }
        }
    }
}
