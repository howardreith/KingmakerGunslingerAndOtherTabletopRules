using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.GameModes;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Blueprints.Root;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.UI.Loot;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private readonly List<object> _weaponRouteRecords = new List<object>();
        private static Action<UnitCommand> _weaponInterruptObservation;
        private static Action<UnitEntityData, Vector3> _weaponPositionObservation;
        private readonly List<object> _weaponNativePositionChanges = new List<object>();
        private int _weaponNativeStoryPositionChanges;
        private static void ObserveWeaponPositionChange(UnitEntityData __instance, Vector3 value)
        { try { if (_weaponPositionObservation != null) _weaponPositionObservation(__instance, value); } catch { } }
        private static void ObserveWeaponCommandInterrupt(UnitCommand __instance)
        {
            // Observation must never affect native command execution.
            try { if (_weaponInterruptObservation != null) _weaponInterruptObservation(__instance); }
            catch { }
        }
        private static object PrepareWeaponRouteCharacter()
        {
            var owner = Game.Instance.Player.MainCharacter.Value.Descriptor;
            int before = owner.Progression.CharacterLevel;
            var fighter = BlueprintBootstrap.Library.GetAllBlueprints().OfType<Kingmaker.Blueprints.Classes.BlueprintCharacterClass>()
                .Single(value => value.name == "FighterClass");
            // Use the existing native controller fixture. The NPC AddClassLevels
            // helper respects the player's blueprint level-plan cap and cannot
            // advance this seed. No production blueprint or source save changes.
            object controller = null;
            try { if (before < 20) AdvanceDisposableSpellcaster(owner, fighter, 20 - before, ref controller); }
            finally { if (controller != null) ((Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)controller).Cancel(); }
            if (owner.Progression.CharacterLevel != 20) throw new InvalidOperationException("Native route fixture level=" + owner.Progression.CharacterLevel + ";expected=20.");
            // The controller fixture advances class mechanics without choosing
            // a complete skill build. Prepare an explicit expert interaction
            // fixture; native locks still roll and no restriction is bypassed.
            foreach (var skill in new[] { owner.Stats.SkillThievery, owner.Stats.SkillAthletics })
                if (skill.BaseValue < 20)
                {
                    skill.BaseValue = 20;
                    skill.AddModifier(20, null, "weapon-route disposable expert skill fixture", Kingmaker.Enums.ModifierDescriptor.UntypedStackable);
                }
            return new { before, after = owner.Progression.CharacterLevel, addedClass = fighter.AssetGuid,
                trickeryRanks = owner.Stats.SkillThievery.BaseValue, trickery = owner.Stats.SkillThievery.ModifiedValue,
                athleticsRanks = owner.Stats.SkillAthletics.BaseValue, athletics = owner.Stats.SkillAthletics.ModifiedValue,
                method = "existing native LevelUpController fixture; disposable class mechanics, not organic campaign progress or a full player build" };
        }
        private static readonly Dictionary<string, string> WeaponOrdinaryEntries = new Dictionary<string, string>(StringComparer.Ordinal) {
            { "b54aad6aa2844fa4c87f46088cde018b", "GateInside" },
            { "e113fb75d9461924ab64df78c019991a", "GateInside" },
            { "3172f82c9f21b8a439a6552850b39b4c", "WhiteRoseAbbey_Enter" },
            { "732080f3aa72fd14cb520902e4b4db89", "Littletown_Enter" },
            { "4dc53495b10c62f4a90d4ae094d232c3", "TrollhoundLair_RightCaveEntrance" },
            { "d03682992a98a614c9d149fca2bab853", "SilverstepLake_Outdoor_Enter" },
            { "dd50c5c9d07eaaf49869308ce8720aec", "ArmagsTomb_Enter" },
            { "020246502ff864f4aab19e2fc00e63ee", "TrollLair_Exterior" },
            { "a2d14c56093720947a6ca4978c6a5985", "MainEntrance" },
            { "2bffac36ed3499f4f9a1e6456e96a0f6", "CandlemereTower" },
            { "2aa7aa5c2df96b143bd2fc62a8547c9c", "HodagLair" },
            { "2d95232e6fc0b594bb6e13e3d3ea0dc3", "VarnholdBefore_Enter" },
            { "c0f1626bb1a0b3b47ad452ce75c7f0e2", "PitaxTown_Enter" },
            { "decb6060ab534294eb6d35510e45d317", "BlakemoorHideout_Enter" } };

        private static object WeaponRouteQuestStates()
        {
            var book = Game.Instance.Player.QuestBook;
            var mirror = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Quests.BlueprintQuest>(BlueprintBootstrap.Library,
                "c8ddb62b377af0c4a86c63ec5367e988", "WhatTheMirrorsMemorize");
            var third = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Quests.BlueprintQuestObjective>(BlueprintBootstrap.Library,
                "84c34803e545ff94ea91b05c6d697db1", "Addendum_ThirdKey");
            return new { mirrorQuestGuid = mirror.AssetGuid, mirrorQuestState = book.GetQuestState(mirror).ToString(),
                thirdKeyObjectiveGuid = third.AssetGuid, thirdKeyObjectiveState = book.GetObjectiveState(third).ToString() };
        }

        private static object WeaponRouteDialogState()
        {
            var dialog = Game.Instance.DialogController;
            return new { dialog = dialog.Dialog == null ? null : dialog.Dialog.AssetGuid,
                name = dialog.Dialog == null ? null : dialog.Dialog.name,
                cue = dialog.CurrentCue == null ? null : dialog.CurrentCue.AssetGuid,
                text = dialog.CurrentCue == null ? null : dialog.CurrentCue.DisplayText,
                answers = dialog.Answers.Select(value => new { guid = value.AssetGuid,
                    name = value.name, text = value.DisplayText }).ToArray() };
        }

        private static object WeaponRouteNavigation(UnitEntityData actor)
        {
            var agent = actor.View.AgentASP;
            var path = agent.Path;
            return new { currentMode = Game.Instance.CurrentMode.ToString(), timeScale = Time.timeScale,
                playerMap = Game.Instance.CurrentlyLoadedArea.AreaName.ToString(),
                actor.Descriptor.State.CanAct, actor.Descriptor.State.CanMove,
                blinded = actor.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Blindness),
                gameTime = Game.Instance.TimeController.GameTime.Ticks, pathFailed = agent.PathFailed,
                pathError = path == null ? null : path.errorLog,
                path = path == null || path.vectorPath == null ? null :
                    path.vectorPath.Select(value => new[] { value.x, value.y, value.z }).ToArray(),
                commands = actor.Commands.Raw.Where(value => value != null).Select(value => new {
                    type = value.GetType().FullName, result = value.Result.ToString(), value.IsFinished,
                    value.IsStarted, value.FinishedApproaching, value.ShouldUnitApproach,
                    value.IsUnitEnoughClose, value.CanStart, value.NeedLoS,
                    approachPoint = new[] { value.ApproachPoint.x, value.ApproachPoint.y, value.ApproachPoint.z } }).ToArray(),
                party = Game.Instance.Player.Party.Select(unit => new { entity = unit.UniqueId,
                    level = unit.Descriptor.Progression.CharacterLevel, dead = unit.Descriptor.State.IsDead,
                    combat = unit.IsInCombat, position = new[] { unit.Position.x, unit.Position.y, unit.Position.z },
                    commands = unit.Commands.Raw.Where(value => value != null).Select(value => new {
                        type = value.GetType().Name, result = value.Result.ToString(), value.IsStarted,
                        value.IsFinished, value.ShouldUnitApproach }).ToArray() }).ToArray(),
                fogNavmeshObjects = Resources.FindObjectsOfTypeAll<MapObjectView>().Where(value => value != null &&
                    value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded && value.Data != null &&
                    (value.Data.UniqueId == "6ca000f4-15ab-479a-a367-388348536a0e" ||
                        value.Data.UniqueId == "1c330e25-562c-47af-8e1e-a1a2df2c64ed" ||
                        value.Data.UniqueId == "436d7162-d72d-4e62-a7af-e83eaba6e083"))
                    .Select(value => new { entity = value.Data.UniqueId, name = value.name, scene = value.gameObject.scene.name,
                        value.Data.IsInGame, active = value.gameObject.activeInHierarchy,
                        position = new[] { value.transform.position.x, value.transform.position.y, value.transform.position.z } }).ToArray(),
                doors = Resources.FindObjectsOfTypeAll<StandardDoor>().Where(value => value != null &&
                    value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded && value.MapObject != null &&
                    value.MapObject.Data != null).Select(value => new { scene = value.gameObject.scene.name,
                        entity = value.MapObject.Data.UniqueId, name = value.name,
                        position = new[] { value.transform.position.x, value.transform.position.y, value.transform.position.z },
                        active = value.gameObject.activeInHierarchy, value.IsOpen,
                        value.MapObject.Data.IsInGame, value.MapObject.Data.IsRevealed,
                        nativePlayerInteractionAvailable = Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(value.MapObject.gameObject),
                        rootPosition = new[] { value.MapObject.transform.position.x, value.MapObject.transform.position.y, value.MapObject.transform.position.z },
                        trap = value.Trap == null ? null : value.Trap.GetType().FullName,
                        value.DisableNavmeshCutWhenOpen,
                        perception = value.MapObject.PerceptionCheckComponent == null ? (int?)null : value.MapObject.PerceptionCheckComponent.DC,
                        restrictions = value.GetComponents<InteractionRestriction>().Select(WeaponRouteRestriction).ToArray(),
                        canInteract = value.CanInteract() }).ToArray() };
        }

        private static object WeaponRouteWalkingGeometry()
        {
            return Resources.FindObjectsOfTypeAll<Transform>().Where(value => value != null &&
                value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded &&
                value.gameObject.scene.name != "UI_Ingame_Scene" &&
                new[] { "ramp", "stair", "road", "path", "navmesh", "ladder", "climb" }
                    .Any(term => value.name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(value => CampaignWeaponSceneObservation.Hierarchy(value)).Take(256).Select(value => new {
                    scene = value.gameObject.scene.name, hierarchy = CampaignWeaponSceneObservation.Hierarchy(value),
                    active = value.gameObject.activeInHierarchy,
                    position = new[] { value.position.x, value.position.y, value.position.z },
                    componentTypes = value.GetComponents<Component>().Where(component => component != null)
                        .Select(component => component.GetType().FullName).ToArray(),
                    nativeArea = ObstacleAnalyzer.GetArea(value.position) }).ToArray();
        }

        private static object WeaponRouteSceneInteractions()
        {
            return new {
                interactions = Resources.FindObjectsOfTypeAll<InteractionComponent>().Where(value => value != null &&
                    value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded).Select(value => new {
                        scene = value.gameObject.scene.name, entity = value.MapObject == null || value.MapObject.Data == null ? null : value.MapObject.Data.UniqueId, name = value.name,
                        type = value.GetType().FullName, active = value.gameObject.activeInHierarchy,
                        interactionType = value.Type.ToString(), value.ShowOvertip,
                        nativeSkill = value is InteractionSkillCheck ? ((InteractionSkillCheck)value).Skill.ToString() : null,
                        nativeDC = value is InteractionSkillCheck ? (int?)((InteractionSkillCheck)value).DC : null,
                        teleportOnSuccess = value is InteractionSkillCheck && ((InteractionSkillCheck)value).TeleportOnSuccess != null ?
                            ((InteractionSkillCheck)value).TeleportOnSuccess.AssetGuid : null,
                        teleportOnFail = value is InteractionSkillCheck && ((InteractionSkillCheck)value).TeleportOnFail != null ?
                            ((InteractionSkillCheck)value).TeleportOnFail.AssetGuid : null,
                        position = new[] { value.transform.position.x, value.transform.position.y, value.transform.position.z },
                        perception = value.MapObject == null || value.MapObject.PerceptionCheckComponent == null ? (int?)null : value.MapObject.PerceptionCheckComponent.DC,
                        restrictions = value.GetComponents<InteractionRestriction>().Select(WeaponRouteRestriction).ToArray(),
                        dialog = value.Dialog == null ? null : value.Dialog.AssetGuid,
                        canInteract = value.MapObject == null || value.MapObject.Data == null ? (bool?)null : value.CanInteract() }).ToArray(),
                entries = AreaEnterPoint.Instances.Where(value => value != null && value.Blueprint != null &&
                    value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded).Select(value => new {
                        guid = value.Blueprint.AssetGuid, name = value.Blueprint.name,
                        scene = value.gameObject.scene.name,
                        position = new[] { value.transform.position.x, value.transform.position.y, value.transform.position.z },
                        nativeArea = ObstacleAnalyzer.GetArea(value.transform.position), active = value.gameObject.activeInHierarchy }).ToArray(),
                transitions = Resources.FindObjectsOfTypeAll<AreaTransition>().Where(value => value != null &&
                    value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded).Select(value => new {
                        scene = value.gameObject.scene.name, name = value.name,
                        entity = value.MapObject == null || value.MapObject.Data == null ? null : value.MapObject.Data.UniqueId,
                        position = new[] { value.transform.position.x, value.transform.position.y, value.transform.position.z },
                        entry = value.AreaEnterPoint == null ? null : value.AreaEnterPoint.AssetGuid,
                        entryName = value.AreaEnterPoint == null ? null : value.AreaEnterPoint.name,
                        area = value.AreaEnterPoint == null ? null : value.AreaEnterPoint.Area.AssetGuid,
                        blueprint = value.Blueprint == null ? null : value.Blueprint.AssetGuid,
                        autoSave = value.AutoSaveMode.ToString(), active = value.gameObject.activeInHierarchy }).ToArray(),
                zones = Resources.FindObjectsOfTypeAll<Kingmaker.View.MapObjects.SriptZones.ScriptZone>().Where(value => value != null &&
                    value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded).Select(value => new {
                        scene = value.gameObject.scene.name, name = value.name, active = value.gameObject.activeInHierarchy,
                        blueprint = value.Blueprint == null ? null : value.Blueprint.AssetGuid,
                        blueprintName = value.Blueprint == null ? null : value.Blueprint.name,
                        position = new[] { value.transform.position.x, value.transform.position.y, value.transform.position.z } }).ToArray()
            };
        }

        private static object WeaponRouteRestriction(InteractionRestriction restriction)
        {
            var key = restriction as Kingmaker.View.MapObjects.InteractionRestrictions.KeyRestriction;
            var flag = restriction as Kingmaker.View.MapObjects.InteractionRestrictions.UnlockRestriction;
            var skill = restriction as Kingmaker.View.MapObjects.InteractionRestrictions.DisableDeviceRestriction;
            return new { type = restriction.GetType().FullName, dc = skill == null ? (int?)null : skill.DC,
                key = key == null || key.Key == null ? null : key.Key.AssetGuid,
                keyName = key == null || key.Key == null ? null : key.Key.name,
                ownedKeys = key == null || key.Key == null ? (int?)null : Game.Instance.Player.Inventory.Count(key.Key),
                flag = flag == null || flag.Flag == null ? null : flag.Flag.AssetGuid,
                unlocked = flag == null || flag.Flag == null ? (bool?)null : flag.Flag.IsUnlocked };
        }

        private static bool AdvanceWeaponRouteDialog(CampaignWeaponPlacement weapon, List<object> observations)
        {
            var controller = Game.Instance.DialogController;
            if (controller.Dialog == null)
                return false;
            var offered = controller.Answers.ToArray();
            string selected = null;
            if (weapon.Key == "eastern:ThunderAtTheGate" && controller.Dialog.AssetGuid == "cc5e585912015cb4b9a76b217ed47422")
                selected = "30862eb247f4ec740a74a211c9ecff5d";
            if (weapon.Key == "eastern:PaperLantern" && controller.Dialog.AssetGuid == "56fa5a30ed5ca0b4eb87ccf477edd366")
                selected = "ad288e1366e4bcd429c8d9fbdf909645";
            if ((weapon.Key == "KMG.Firearms.TheLastWordItem" || weapon.Key == "eastern:HeavensMeasure") &&
                controller.Dialog.AssetGuid == "cd9c00bf52e94ce4697b5e1958a05656")
                selected = "fc5da4f8f958b934b86ed00a183e52a0";
            if (weapon.Key == "eastern:WayfarersOath" && controller.Dialog.AssetGuid == "a15c903418e7e524bbbf9b4e7f3cf47a")
                selected = "51c664fb19d62f948bf45a312dea0646";
            if (weapon.Key == "KMG.Firearms.DuelistsRebuttalItem" && controller.Dialog.AssetGuid == "15c17d335613b0447b881fc03c4be5b2")
                selected = new[] { "fec3f046be1dd114a859f2ea86edf7d3", "146fa432f41a5d740896820e0d5bdd99" }
                    .FirstOrDefault(guid => offered.Any(value => value.AssetGuid == guid && value.CanSelect()));
            if (weapon.Key == "eastern:FallingPetal" && controller.Dialog.AssetGuid == "04584fbb4537cb4428fd10145c5bbb80")
                selected = "e93e77e7272e7bc41a1ad0ee474760f1";
            if (weapon.Key == "KMG.Firearms.IrovettisOvationItem" && controller.Dialog.AssetGuid == "b7ca4168229a4e04eb2bd3284f5c0550")
                selected = "d5cebbd9eef3e7b41bb886ef0f4dfe75";
            if (weapon.Key == "eastern:MountainSunder" && controller.Dialog.AssetGuid == "234dc74843e671d4b9cb1f7e99220ad7")
                selected = new[] { "caf2d5f2cbd54e94baf54e1bbe2ef1a4", "6d77b7710114e024d8c96fd55c704ceb",
                    "830a991da46f88847b6b7a2a65a97b9a", "d8c34fdc915ff7f498648e7163eec981",
                    "45677310853fa36408249a9dff6f8068", "6a78484e04e3def4a9743feb4bd26ff5",
                    "fd0366da37e6ea94482c905098a25d65" }
                    .FirstOrDefault(guid => offered.Any(value => value.AssetGuid == guid && value.CanSelect()));
            if (weapon.Key == "eastern:MountainSunder" && controller.Dialog.AssetGuid == "0a2aae726137c89488919f3b7b8160fd")
                selected = "97159411913360f4f8fe16f40d9bac14";
            if (weapon.Key == "spear:BriarCrownedSpear" && controller.Dialog.AssetGuid == "d4c76faeb0e307b419eb0a162c6373cd")
                selected = "155e9f89b90256e49adf9db2df9ec61c"; // Native Trickery 35 check; no key or outcome supplied.
            var answer = offered.Length == 1 && offered[0].IsSystem() && offered[0].CanSelect() ? offered[0] :
                offered.SingleOrDefault(value => value.AssetGuid == selected && value.CanSelect());
            if (answer == null) return false;
            bool manual = answer.CharacterSelection.SelectionType == Kingmaker.DialogSystem.CharacterSelection.Type.Manual;
            var actingUnit = manual ? Game.Instance.Player.MainCharacter.Value : null;
            if (manual && !Game.Instance.Player.ControllableCharacters.Contains(actingUnit)) return false;
            observations.Add(new { dialog = controller.Dialog.AssetGuid, cue = controller.CurrentCue == null ? null : controller.CurrentCue.AssetGuid,
                answer = answer.AssetGuid, text = answer.DisplayText,
                selectionType = answer.CharacterSelection.SelectionType.ToString(), actingUnit = actingUnit == null ? null : actingUnit.UniqueId,
                method = "Native offered selectable answer and native character-selection argument; rolls and outcomes remain native" });
            controller.SelectAnswer(answer, actingUnit);
            return true;
        }

        private static StandardDoor[] AdjacentOrdinaryWeaponRouteDoors(UnitEntityData actor, HashSet<string> attempted, bool requirePlayerAvailable = true)
        {
            return Resources.FindObjectsOfTypeAll<StandardDoor>().Where(value => value != null &&
                value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy &&
                value.MapObject != null && value.MapObject.Data != null && value.MapObject.Data.IsInGame &&
                !value.IsOpen && value.MapObject.PerceptionCheckComponent == null && value.Trap == null &&
                !value.GetComponents<InteractionRestriction>().Any(restriction => restriction is Kingmaker.View.MapObjects.InteractionRestrictions.ScriptRestriction) &&
                !attempted.Contains(value.MapObject.Data.UniqueId) && value.CanInteract() &&
                (!requirePlayerAvailable || Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(value.MapObject.gameObject)) &&
                Vector3.Distance(actor.Position, value.transform.position) <= 35f)
                .OrderBy(value => Vector3.Distance(actor.Position, value.transform.position)).ToArray();
        }

        private static Vector3 WeaponNativeDoorCrossing(Vector3 approachOrigin, StandardDoor door)
        {
            var toward = Vector3.ProjectOnPlane(door.transform.position - approachOrigin, Vector3.up);
            if (toward.sqrMagnitude < 0.01f)
                throw new InvalidOperationException("The native doorway approach does not establish a crossing direction.");
            return ObstacleAnalyzer.GetNearestNode(door.transform.position + toward.normalized * 6f).clampedPosition;
        }

        private static void PrepareWeaponRouteCombat(HashSet<string> attempted, List<object> observations)
        {
            var game = Game.Instance;
            foreach (var enemy in game.State.Units.All.Where(unit => unit.IsInGame && unit.IsPlayersEnemy &&
                !unit.Descriptor.State.IsDead && unit.HoldingState != game.Player.CrossSceneState &&
                Vector3.Distance(unit.Position, game.Player.MainCharacter.Value.Position) <= 18f).ToArray())
            {
                string state = enemy.UniqueId + ":combat=" + enemy.IsInCombat + ":untargetable=" + (bool)enemy.Descriptor.State.IsUntargetable;
                if (!attempted.Add(state)) continue;
                var result = ApplyWeaponFixtureDamage(enemy, Kingmaker.Enums.Damage.DamageEnergyType.Fire, observations);
                // The palace fixture measured zero applied fire damage. Use
                // another ordinary native energy rule only when its actual
                // Source.Immune result confirms immunity. Neither rule changes
                // immunity, immortality, untargetability or damage reduction.
                if (result.Damage == 0 && result.ResultDamage != null && result.ResultDamage.Any(value => value.Source.Immune) &&
                    !enemy.Descriptor.State.IsDead && !enemy.Descriptor.State.Immortality && !enemy.Descriptor.State.IsUntargetable)
                    ApplyWeaponFixtureDamage(enemy, Kingmaker.Enums.Damage.DamageEnergyType.Acid, observations);
            }
        }

        private static RuleDealDamage ApplyWeaponFixtureDamage(UnitEntityData enemy,
            Kingmaker.Enums.Damage.DamageEnergyType energy, List<object> observations)
        {
                int before = enemy.Damage;
                var result = Rulebook.Trigger(new RuleDealDamage(Game.Instance.Player.MainCharacter.Value, enemy,
                    new DamageBundle(new EnergyDamage(new DiceFormula(0, DiceType.D6), energy) { PreRolledValue = 100000 })));
                observations.Add(new { entity = enemy.UniqueId, blueprint = enemy.Blueprint.AssetGuid,
                    energy = energy.ToString(),
                    nativeDamageSources = result.ResultDamage == null ? null : result.ResultDamage.Select(value => new {
                        value.Source.Immune, value.FinalValue, type = value.Source.GetType().FullName }).ToArray(),
                    deadAtRuleReturn = enemy.Descriptor.State.IsDead,
                    calculatedDamage = result.ResultDamage == null ? (int?)null : result.ResultDamage.Sum(value => value.FinalValue),
                    appliedDamage = result.Damage, result.MinHPAfterDamage, damageBefore = before,
                    damageAfter = enemy.Damage, hitPoints = enemy.Stats.HitPoints.ModifiedValue,
                    immortal = (bool)enemy.Descriptor.State.Immortality,
                    regeneration = (bool)enemy.Descriptor.State.IsRegenerate,
                    untargetable = (bool)enemy.Descriptor.State.IsUntargetable,
                    position = new[] { enemy.Position.x, enemy.Position.y, enemy.Position.z },
                    method = "Native RuleDealDamage on nearby fixture enemies only; native immunity, immortality and untargetability remain in force" });
                return result;
        }

        private IEnumerable<int> ObserveSelectedWeaponRoute()
        { return ObserveSelectedWeaponRoute((string)_request.Parameters["weaponKey"]); }

        private IEnumerable<int> ObserveSelectedWeaponRoute(string key)
        {
            bool overlayWasOpen = SetModManagerOverlay(false);
            var setter = typeof(UnitEntityData).GetProperty("Position").GetSetMethod();
            var observer = typeof(RuntimeTestRunner).GetMethod(nameof(ObserveWeaponPositionChange), BindingFlags.NonPublic | BindingFlags.Static);
            _weaponPositionObservation = (unit, position) => {
                if (unit != Game.Instance.Player.MainCharacter.Value || Vector3.Distance(unit.Position, position) < 8f) return;
                string caller = Environment.StackTrace;
                var scenes = Game.Instance.State.Cutscenes.Where(value => !value.IsFinished).Select(value => new {
                    guid = value.Cutscene.AssetGuid, name = value.Cutscene.name, value.Paused }).ToArray();
                // The Womb of Lamashtu Kesten encounter translocates the player in a native
                // background cutscene while the top mode remains Default. This
                // is measured story movement, never part of walked distance.
                bool nativeStory = key == "eastern:FallingPetal" && scenes.Any(value => value.guid == "80154b45ff528ca459af95877a816f57") &&
                    caller.Contains("TranslocateUnit.RunAction") && caller.Contains("Cutscenes.CommandAction.OnRun");
                if (nativeStory) _weaponNativeStoryPositionChanges++;
                _weaponNativePositionChanges.Add(new { from = new[] { unit.Position.x, unit.Position.y, unit.Position.z },
                    to = new[] { position.x, position.y, position.z }, caller, cutscenes = scenes, measuredNativeStory = nativeStory,
                    gameTime = Game.Instance.TimeController.GameTime.Ticks, mode = Game.Instance.CurrentMode.ToString() });
            };
            _context.Harmony.Patch(setter, new HarmonyMethod(observer), null, null);
            try
            {
                foreach (int tick in ObserveSelectedWeaponRouteCore(key))
                {
                    // Use the existing supported overlay scope. Native UMM
                    // startup may reopen it on area load and block game input.
                    SetModManagerOverlay(false);
                    yield return tick;
                }
            }
            finally {
                _weaponPositionObservation = null;
                _context.Harmony.Unpatch(setter, observer);
                if (overlayWasOpen) SetModManagerOverlay(true);
            }
        }

        private IEnumerable<int> ObserveSelectedWeaponRouteCore(string key)
        {
            var game = Game.Instance;
            var weapon = CampaignWeaponRegistry.Require(key);
            var target = BlueprintLibraryLookup.RequireExact<BlueprintLoot>(BlueprintBootstrap.Library,
                weapon.Location.TargetGuid, weapon.Location.TargetName);
            _workingSaveSmoke.RestrictToTransactionOwnedWrites();
            var characterPreparation = PrepareWeaponRouteCharacter();
            if (weapon.Key == "eastern:QuietCurrent")
            {
                var keyItem = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintItemKey>(BlueprintBootstrap.Library,
                    "a041939cf49a5ba45b487e0fa0bb357c", "HragulkaExitKey");
                int count = game.Player.Inventory.Count(keyItem);
                if (count == 0) game.Player.Inventory.Add(keyItem.CreateEntity());
                _weaponFindabilityRecords.Add(new { key, nativeKeyFixture = new { item = keyItem.AssetGuid, name = keyItem.name,
                    before = count, after = game.Player.Inventory.Count(keyItem), suppliedAsDisposableStoryPrerequisite = count == 0,
                    provenance = "Iron Dwarven Key from the native Troll Trouble boss/throne treasure; fixture prerequisite, not organic boss progression" } });
            }
            if (weapon.Key == "eastern:HeavensMeasure")
            {
                var keyItem = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintItemKey>(BlueprintBootstrap.Library,
                    "0cb09dcb53daa684583536cf68583cdc", "HornedHunterKey");
                int count = game.Player.Inventory.Count(keyItem);
                if (count == 0) game.Player.Inventory.Add(keyItem.CreateEntity());
                _weaponFindabilityRecords.Add(new { key, nativeKeyFixture = new { item = keyItem.AssetGuid, name = keyItem.name,
                    before = count, after = game.Player.Inventory.Count(keyItem), suppliedAsDisposableStoryPrerequisite = count == 0,
                    provenance = "Native Horned Hunter key for the ordinary first-floor stair corridor; disposable story prerequisite, not organic Hunter progression" } });
            }
            if (weapon.Key == "eastern:HeavensMeasure")
            {
                var storyKeys = new List<object>();
                foreach (var nativeKey in new[] {
                    new { guid = "7a5e6bbafe3b81a46bdef45d039ab603", name = "WrigglingManKey" },
                    new { guid = "d80d79e6f026e3f4eb55ed10118d3aa4", name = "KnurlyWitchKey" },
                    new { guid = "e50eb543498587d4992715f963a670c9", name = "ThirdNyrissaKey" } })
                {
                    var nativeItem = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintItemKey>(
                        BlueprintBootstrap.Library, nativeKey.guid, nativeKey.name);
                    int before = game.Player.Inventory.Count(nativeItem);
                    if (before == 0) game.Player.Inventory.Add(nativeItem.CreateEntity());
                    storyKeys.Add(new { item = nativeItem.AssetGuid, nativeItem.name, before,
                        after = game.Player.Inventory.Count(nativeItem), suppliedAsDisposableStoryPrerequisite = before == 0 });
                }
                _weaponFindabilityRecords.Add(new { key, nativeNyrissaKeysFixture = storyKeys,
                    provenance = "Exact keys required by native GotAllKeys trigger; organic key acquisition and Third Key puzzle are not exercised. No gate or quest flag is overridden." });
            }
            if (target.Area.name.StartsWith("HouseAtTheEdgeOfTime", StringComparison.Ordinal))
            {
                game.Player.UnlockableFlags.SetFlagValue(BlueprintLibraryLookup.RequireExact<BlueprintUnlockableFlag>(BlueprintBootstrap.Library,
                    "db94ef898ad95944788d3da6b22a5e31", "Phase"), 1);
                var lantern = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>(
                    BlueprintBootstrap.Library, "da0b70ce16aae6149a720670761271aa", "FirstWorldLanternNew");
                var owner = game.Player.MainCharacter.Value.Descriptor;
                var item = game.Player.Inventory.Items.OfType<ItemEntityUsable>().SingleOrDefault(value => value.Blueprint == lantern);
                bool supplied = item == null;
                if (supplied) { item = new ItemEntityUsable(lantern); game.Player.Inventory.Add(item); }
                if (item.HoldingSlot == null) owner.Body.QuickSlots.First(value => !value.HasItem && value.IsPossibleInsertItems()).InsertItem(item);
                if (item.ActivatableAbility == null || item.ActivatableAbility.Blueprint.AssetGuid != "b8a7fd52feb905149a9d32955cc9f6a3")
                    throw new InvalidOperationException("The native equipped lantern ability did not bind.");
                item.ActivatableAbility.IsOn = !target.Area.name.EndsWith("2ndFloor", StringComparison.Ordinal);
                _weaponFindabilityRecords.Add(new { key, nativeLanternFixture = new { item = lantern.AssetGuid,
                    ability = item.ActivatableAbility.Blueprint.AssetGuid, suppliedAsDisposableStoryPrerequisite = supplied,
                    slot = item.HoldingSlot.GetType().FullName, activationRequested = item.ActivatableAbility.IsOn } });
            }
            string ordinaryName;
            WeaponOrdinaryEntries.TryGetValue(weapon.Location.TargetGuid, out ordinaryName);
            var ordinaryEntries = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintAreaEnterPoint>()
                .Where(value => value.Area == target.Area && (ordinaryName == null ?
                    value.name.EndsWith("_Enter", StringComparison.Ordinal) : value.name == ordinaryName)).ToArray();
            var loadArea = target.Area;
            if (weapon.Key == "eastern:HeavensMeasure")
            {
                loadArea = BlueprintLibraryLookup.RequireExact<BlueprintLoot>(BlueprintBootstrap.Library,
                    "e113fb75d9461924ab64df78c019991a", "FirstWorld_PoorLoot02#1").Area;
                ordinaryEntries = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintAreaEnterPoint>()
                    .Where(value => value.Area == loadArea && value.name == "GateInside").ToArray();
                if (ordinaryEntries.Length != 1) throw new InvalidOperationException("Exact House first-floor ordinary entrance absent.");
            }
            if (weapon.Key == "spear:BriarCrownedSpear")
            {
                var exterior = BlueprintLibraryLookup.RequireExact<BlueprintAreaEnterPoint>(BlueprintBootstrap.Library,
                    "d2b101558d07be343b803836da0efc6a", "BlakemoorHideoutEntrance_Enter");
                loadArea = exterior.Area; ordinaryEntries = new[] { exterior };
            }
            foreach (int tick in WhiteoutLoadArea(loadArea, ordinaryEntries.Length == 1 ? ordinaryEntries[0] : null)) yield return tick;
            if (loadArea.name == "HouseAtTheEdgeOfTime")
            {
                // Native HATEOT activation starts FromPortal2House, whose own
                // five-round Prone buff and fade sequence can begin after the
                // load callback. Let that ordinary introduction finish before
                // issuing a walking command. Do not cancel it or remove buffs.
                var intro = Stopwatch.StartNew(); var introStates = new List<object>();
                game.IsPaused = false;
                try
                {
                    while (intro.Elapsed.TotalSeconds < 35 || game.IsModeActive(GameModeType.Cutscene) ||
                        game.Player.MainCharacter.Value.Descriptor.State.Prone.Active)
                    {
                        if (intro.Elapsed.TotalSeconds > 90 || game.IsModeActive(GameModeType.Dialog) ||
                            game.Player.MainCharacter.Value.Descriptor.State.IsDead)
                            throw new InvalidOperationException("Native House entrance introduction requires further story preparation; modes=" +
                                string.Join(",", Enum.GetValues(typeof(GameModeType)).Cast<GameModeType>().Where(game.IsModeActive)));
                        if (introStates.Count == 0 || intro.Elapsed.TotalSeconds >= introStates.Count)
                            introStates.Add(new { seconds = intro.Elapsed.TotalSeconds,
                                modes = Enum.GetValues(typeof(GameModeType)).Cast<GameModeType>().Where(game.IsModeActive).Select(value => value.ToString()).ToArray(),
                                prone = game.Player.MainCharacter.Value.Descriptor.State.Prone.Active });
                        yield return 0;
                    }
                }
                finally
                {
                    game.IsPaused = true;
                    _weaponFindabilityRecords.Add(new { key, nativeEntranceIntroduction = introStates,
                        phase = CampaignWeaponSceneObservation.Phase(), questState = WeaponRouteQuestStates() });
                }
            }
            // Prepare ordinary combat as an explicit native fixture prerequisite.
            // Native immunity, immortality, door, reveal and skill conditions
            // remain active. Closed fixture prerequisites and offered dialog
            // choices are recorded separately from organic campaign progress.
            var defeats = new List<object>();
            PrepareWeaponRouteCombat(new HashSet<string>(StringComparer.Ordinal), defeats);
            for (int i = 0; i < 8; i++) yield return 0;
            if (weapon.Key == "spear:SpearOfTheFirstBranch")
                _weaponFindabilityRecords.Add(new { key, nativeWalkingGeometry = Resources.FindObjectsOfTypeAll<Transform>().Where(value =>
                    value != null && value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded &&
                    value.gameObject.scene.name.StartsWith("FinalDungeon2_", StringComparison.Ordinal) &&
                    new[] { "ramp", "stair", "roof", "road", "path", "navmesh", "ladder", "climb" }
                        .Any(term => value.name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => CampaignWeaponSceneObservation.Hierarchy(value)).Take(256).Select(value => new {
                        scene = value.gameObject.scene.name, hierarchy = CampaignWeaponSceneObservation.Hierarchy(value),
                        active = value.gameObject.activeInHierarchy,
                        position = new[] { value.position.x, value.position.y, value.position.z },
                        nativeNearest = ObstacleAnalyzer.GetNearestNode(value.position).clampedPosition.ToString(),
                        nativeArea = ObstacleAnalyzer.GetArea(value.position) }).ToArray(),
                    method = "Read-only native static walking geometry; names and points do not qualify a walking route" });
            foreach (int tick in ObserveWeaponNativeFloorPrelude(weapon)) yield return tick;
            if (target.Area.name.StartsWith("HouseAtTheEdgeOfTime", StringComparison.Ordinal) &&
                !game.Player.MainCharacter.Value.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid == "ed56d0930d7a7ab479485abda4658ee5"))
                throw new InvalidOperationException("Native lantern activation is not measured; do not assume the phase is stable.");
            var objects = CampaignWeaponSceneObservation.Find(target.AssetGuid);
            bool active = objects.Any(value => value.gameObject.activeInHierarchy && value.MapObject != null &&
                value.MapObject.Data != null && value.MapObject.Data.IsInGame);
            _weaponFindabilityRecords.Add(new { key = weapon.Key, target = target.AssetGuid, area = target.Area.AssetGuid,
                phase = CampaignWeaponSceneObservation.Phase(), objects = objects.Select(CampaignWeaponSceneObservation.Capture).ToArray(),
                interactions = WeaponRouteSceneInteractions(),
                questState = WeaponRouteQuestStates(),
                characterPreparation, combatPreparation = defeats, organicProgress = "UNVERIFIED; explicit native leveled combat fixture on a load-only seed",
                sources = Resources.FindObjectsOfTypeAll<LootComponent>().Where(value => value != null &&
                    value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded).Select(value => new {
                        tables = (value.LootTables ?? new Kingmaker.Blueprints.DirectSerialization.BlueprintReference[0]).Select(reference => {
                            var table = reference == null ? null : reference.Get();
                            return table == null ? null : new { guid = table.AssetGuid, name = table.name }; }).ToArray(),
                        observation = CampaignWeaponSceneObservation.Capture(value) }).ToArray() });
            _weaponFindabilityAssertions.Add(Assertion("weapon-route-scene-" + weapon.Key, "Exact active persistent target object",
                "matches=" + objects.Length + ";active=" + active, active, "installed scene after native combat fixture preparation"));
            if (active)
                foreach (int tick in WalkAndPickUpCampaignWeapon(weapon, objects.Single(value => value.gameObject.activeInHierarchy))) yield return tick;
        }

        private IEnumerable<int> WalkAndPickUpCampaignWeapon(CampaignWeaponPlacement weapon, LootComponent loot)
        {
            var game = Game.Instance; var actor = game.Player.MainCharacter.Value;
            string entryName;
            if (!WeaponOrdinaryEntries.TryGetValue(weapon.Location.TargetGuid, out entryName))
            {
                var ordinary = AreaEnterPoint.Instances.Where(value => value != null && value.Blueprint != null &&
                    value.Blueprint.Area == game.CurrentlyLoadedArea && value.Blueprint.name.EndsWith("_Enter", StringComparison.Ordinal)).ToArray();
                if (ordinary.Length == 1) entryName = ordinary[0].Blueprint.name;
            }
            var entries = AreaEnterPoint.Instances.Where(value => value != null && value.Blueprint != null &&
                value.Blueprint.Area == game.CurrentlyLoadedArea && value.Blueprint.name == entryName).ToArray();
            if (entries.Length != 1)
            {
                _weaponRouteRecords.Add(new { key = weapon.Key, route = "UNVERIFIED", pickup = "UNVERIFIED",
                    blocker = "Expected ordinary entry did not resolve uniquely", entryName, matches = entries.Length });
                _weaponFindabilityAssertions.Add(Assertion("weapon-ordinary-entry-" + weapon.Key,
                    "One exact ordinarily accessible entrance", "matches=" + entries.Length, false, "No substitute entry or target-side teleportation"));
                yield break;
            }
            var entry = entries[0];
            // The selected route scenario loads through this actual entrance.
            // PositionCharacters alone does not run the native loading/transition
            // lifecycle; use it only for the earlier scene census fixture.
            if (_request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveWeaponFindabilityScenes) entry.PositionCharacters();
            var start = actor.Position; var points = new List<object>();
            int initialCount = game.Player.Inventory.Count(weapon.Item);
            var original = loot.Loot.Items.Where(value => value.Blueprint != weapon.Item).ToDictionary(value => value, value => value.Count);
            var command = new UnitInteractWithObject(loot) { CreatedByPlayer = true };
            // A normal movement command establishes the walking approach first.
            // The subsequent native interaction still requires its own line of
            // sight and visible loot window; a reachable navmesh point alone
            // cannot qualify the chest. Never supply a forced path or position.
            var approach = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(loot.transform.position).clampedPosition)
                { CreatedByPlayer = true };
            var interruptions = new List<object>();
            var doorSteps = new List<object>(); var dialogSteps = new List<object>(); var cutsceneSteps = new List<object>();
            var combatSteps = new List<object>(); var interactionSteps = new List<object>();
            var combatAttempts = new HashSet<string>(StringComparer.Ordinal);
            var doorAttempts = new HashSet<string>(StringComparer.Ordinal);
            var skillAttempts = new HashSet<string>(StringComparer.Ordinal);
            var doorScouts = new HashSet<string>(StringComparer.Ordinal);
            UnitCommand doorCommand = null;
            var interruptMethod = typeof(UnitCommand).GetMethod("Interrupt", new[] { typeof(bool) });
            var observer = typeof(RuntimeTestRunner).GetMethod(nameof(ObserveWeaponCommandInterrupt), BindingFlags.NonPublic | BindingFlags.Static);
            if (interruptMethod == null || observer == null) throw new InvalidOperationException("Native interruption observer could not bind.");
            _weaponInterruptObservation = value => {
                if (ReferenceEquals(value, command) || ReferenceEquals(value, approach) || ReferenceEquals(value, doorCommand)) interruptions.Add(new {
                    commandType = value.GetType().FullName,
                    value.ShouldBeInterrupted, value.InterruptAsSoonAsPossible,
                    canAct = actor.Descriptor.State.CanAct, actor.IsInCombat, playerCombat = game.Player.IsInCombat,
                    stack = Environment.StackTrace, phase = CampaignWeaponSceneObservation.Phase(),
                    position = new[] { actor.Position.x, actor.Position.y, actor.Position.z },
                    navigation = WeaponRouteNavigation(actor) }); };
            _context.Harmony.Patch(interruptMethod, new HarmonyMethod(observer), null, null);
            var timer = Stopwatch.StartNew(); string blocker = null; float distance = 0f;
            int proneRetries = 0, trapRetries = 0, commandRetries = 0;
            double calmSince = -1;
            bool proneInterrupted = false;
            int observedStoryChanges = _weaponNativeStoryPositionChanges;
            var prior = actor.Position;
            double lastMovement = 0;
            bool paused = game.IsPaused;
            try
            {
                actor.Commands.Run(approach); game.IsPaused = false;
                bool interactionIssued = false;
                double routeBound = weapon.Key == "KMG.Firearms.IrovettisOvationItem" ? 600 : 300;
                while (timer.Elapsed.TotalSeconds < routeBound)
                {
                    if (loot == null || !loot.gameObject.activeInHierarchy)
                    { blocker = "The exact phase-specific loot object unloaded or became inactive during the route"; break; }
                    if (game.CurrentMode == GameModeType.Dialog)
                    {
                        // Native Exit clears the controller before the dialog
                        // mode stack finishes unwinding. Observe that transition
                        // instead of treating a null cue as a missing choice.
                        var settling = Stopwatch.StartNew();
                        while (game.CurrentMode == GameModeType.Dialog &&
                            (game.DialogController.Dialog == null || (game.DialogController.CurrentCue == null &&
                                !game.DialogController.Answers.Any(value => value.CanSelect()))) &&
                            settling.Elapsed.TotalSeconds < 10) yield return 0;
                        if (game.CurrentMode != GameModeType.Dialog)
                        {
                            prior = actor.Position;
                            approach = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(loot.transform.position).clampedPosition) { CreatedByPlayer = true };
                            command = new UnitInteractWithObject(loot) { CreatedByPlayer = true };
                            actor.Commands.Run(approach); interactionIssued = false;
                            continue;
                        }
                        if (dialogSteps.Count >= 20 || !AdvanceWeaponRouteDialog(weapon, dialogSteps))
                        { blocker = "A native dialogue needs unqualified story preparation or a player choice"; break; }
                        for (int i = 0; i < 12; i++) yield return 0;
                        if (game.CurrentMode != GameModeType.Dialog)
                        {
                            approach = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(loot.transform.position).clampedPosition) { CreatedByPlayer = true };
                            command = new UnitInteractWithObject(loot) { CreatedByPlayer = true };
                            actor.Commands.Run(approach); interactionIssued = false;
                        }
                        continue;
                    }
                    if (game.CurrentMode == GameModeType.Cutscene)
                    {
                        cutsceneSteps.Add(new { phase = CampaignWeaponSceneObservation.Phase(),
                            cutscenes = game.State.Cutscenes.Select(value => new { guid = value.Cutscene.AssetGuid,
                                name = value.Cutscene.name, value.IsFinished, value.Paused }).ToArray() });
                        var cutsceneTimer = Stopwatch.StartNew();
                        while (game.CurrentMode == GameModeType.Cutscene &&
                            cutsceneTimer.Elapsed.TotalSeconds < 90 && !actor.Descriptor.State.IsDead) yield return 0;
                        if (game.CurrentMode == GameModeType.Cutscene)
                        { blocker = "Native story cutscene did not return control within the fixture bound"; break; }
                        prior = actor.Position; // Native story movement is recorded separately from walking.
                        if (game.CurrentMode != GameModeType.Dialog)
                        {
                            approach = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(loot.transform.position).clampedPosition) { CreatedByPlayer = true };
                            command = new UnitInteractWithObject(loot) { CreatedByPlayer = true };
                            actor.Commands.Run(approach); interactionIssued = false;
                        }
                        continue;
                    }
                    if (actor.Descriptor.State.IsDead)
                    { blocker = "Story dialogue/cutscene or incapacitation interrupted the entry-to-object fixture"; break; }
                    // Native proximity spawns can start combat after the entry
                    // preparation. Record and defeat those fixture enemies with
                    // the same native damage rule, then resume an ordinary order.
                    if (game.Player.IsInCombat) PrepareWeaponRouteCombat(combatAttempts, combatSteps);
                    if (game.IsPaused) game.IsPaused = false; // Native combat autopause; no story mode is cancelled.
                    bool prone = actor.Descriptor.State.Prone.Active || Kingmaker.Controllers.Units.UnitProneController.ShouldBeProne(actor);
                    if (game.Player.IsInCombat || actor.IsInCombat || !actor.Descriptor.State.CanAct || prone)
                    { calmSince = -1; proneInterrupted |= prone; }
                    else if (calmSince < 0) calmSince = timer.Elapsed.TotalSeconds;
                    bool nativeReady = calmSince >= 0 && timer.Elapsed.TotalSeconds - calmSince >= 2;
                    bool stalled = timer.Elapsed.TotalSeconds - lastMovement > 12;
                    if (nativeReady && ((!interactionIssued && approach.IsFinished) || stalled))
                    {
                        // A native MoveTo can finish a partial path outside the
                        // room. Continue ordinary door/barrier exploration when
                        // the later interaction also stalls; success of MoveTo
                        // alone is never evidence of a reachable chest.
                        if (approach.Result != UnitCommand.ResultType.Success || stalled ||
                            Vector3.Distance(actor.Position, loot.transform.position) > 5f)
                        {
                            // The fort's ordinary crate barrier uses its real
                            // Athletics check (or native time-spending fallback).
                            // Never run its success actions directly.
                            var obstacle = weapon.Key == "eastern:PaperLantern" ?
                                Resources.FindObjectsOfTypeAll<InteractionSkillCheck>().SingleOrDefault(value =>
                                    value != null && value.MapObject != null && value.MapObject.Data != null &&
                                    value.MapObject.Data.UniqueId == "16f9b2b9-f291-4b31-9c6a-dc6eaa0f7846" &&
                                    value.gameObject.activeInHierarchy && value.CanInteract() &&
                                    !skillAttempts.Contains(value.MapObject.Data.UniqueId)) : null;
                            if (obstacle != null)
                            {
                                var clearing = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(obstacle.transform.position).clampedPosition) { CreatedByPlayer = true };
                                doorCommand = clearing; actor.Commands.Run(clearing);
                                var clearingTimer = Stopwatch.StartNew();
                                while (!clearing.IsFinished && clearingTimer.Elapsed.TotalSeconds < 30 &&
                                    game.CurrentMode != GameModeType.Dialog && game.CurrentMode != GameModeType.Cutscene)
                                {
                                    float clearingDistance = Vector3.Distance(prior, actor.Position);
                                    if (clearingDistance > 10f) { blocker = "An unqualified jump occurred during the crate approach"; break; }
                                    distance += clearingDistance; prior = actor.Position; yield return 0;
                                }
                                var state = obstacle.Data as SkillCheckData;
                                bool clicked = false;
                                if (blocker == null && Vector3.Distance(actor.Position, obstacle.transform.position) < 5f &&
                                    obstacle.CanInteract())
                                {
                                    clicked = Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.Interact(
                                        obstacle.MapObject.gameObject, new List<UnitEntityData> { actor }, true, false);
                                    if (clicked) skillAttempts.Add(obstacle.MapObject.Data.UniqueId);
                                    var nativeCheck = Stopwatch.StartNew();
                                    while (state != null && !state.AlreadyUsed && nativeCheck.Elapsed.TotalSeconds < 20)
                                        yield return 0;
                                }
                                interactionSteps.Add(new { entity = obstacle.MapObject.Data.UniqueId,
                                    skill = obstacle.Skill.ToString(), configuredDC = obstacle.DC,
                                    nativeDC = state == null ? (int?)null : state.DCOverride,
                                    passed = state == null ? (bool?)null : state.CheckPassed,
                                    used = state == null ? (bool?)null : state.AlreadyUsed,
                                    result = clearing.Result.ToString(), clicked,
                                    interactionType = obstacle.Type.ToString(),
                                    method = "Native walking then ClickMapObjectHandler.Interact using the actual scene interaction type; native check and actions" });
                                if (blocker != null) break;
                                approach = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(loot.transform.position).clampedPosition) { CreatedByPlayer = true };
                                command = new UnitInteractWithObject(loot) { CreatedByPlayer = true };
                                interactionIssued = false;
                                actor.Commands.Run(approach); lastMovement = timer.Elapsed.TotalSeconds; continue;
                            }
                            var doors = AdjacentOrdinaryWeaponRouteDoors(actor, doorAttempts);
                            if (doors.Length > 0 && doorSteps.Count < 16)
                            {
                                var door = doors[0];
                                var doorApproachOrigin = actor.Position;
                                doorAttempts.Add(door.MapObject.Data.UniqueId);
                                var opening = new UnitInteractWithObject(door) { CreatedByPlayer = true };
                                doorCommand = opening; actor.Commands.Run(opening);
                                var doorTimer = Stopwatch.StartNew();
                                while (!opening.IsFinished && doorTimer.Elapsed.TotalSeconds < 15 &&
                                    game.CurrentMode != GameModeType.Dialog && game.CurrentMode != GameModeType.Cutscene)
                                {
                                    float doorDistance = Vector3.Distance(prior, actor.Position);
                                    if (doorDistance > 10f) { blocker = "A position jump occurred during the door approach"; break; }
                                    distance += doorDistance; prior = actor.Position;
                                    yield return 0;
                                }
                                bool opened = opening.IsFinished && opening.Result == UnitCommand.ResultType.Success && door.IsOpen;
                                doorSteps.Add(new { entity = door.MapObject.Data.UniqueId, scene = door.gameObject.scene.name,
                                    position = new[] { door.transform.position.x, door.transform.position.y, door.transform.position.z },
                                    result = opening.Result.ToString(), door.IsOpen, opening.FinishedApproaching,
                                    nativePlayerInteractionAvailable = Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(door.MapObject.gameObject),
                                    revealed = door.MapObject.Data.IsRevealed,
                                    perceptionComponent = "absent", restrictions = door.GetComponents<InteractionRestriction>().Select(value => value.GetType().FullName).ToArray(), trap = "absent",
                                    method = "native UnitInteractWithObject walking, any native lock roll and normal opening", qualification = opened ? "PASS" : "UNVERIFIED" });
                                if (!opening.IsFinished) opening.Interrupt();
                                // Opening one room can make a previously
                                // unfinished door approach reachable. Retrying
                                // still uses native visibility, locks and LOS;
                                // the total exploration remains bounded above.
                                if (opened)
                                    foreach (var earlier in Resources.FindObjectsOfTypeAll<StandardDoor>().Where(value =>
                                        value != null && value.MapObject != null && value.MapObject.Data != null &&
                                        value.gameObject.scene.isLoaded && !value.IsOpen))
                                        doorAttempts.Remove(earlier.MapObject.Data.UniqueId);
                                if (blocker != null) break;
                                if (opened)
                                {
                                    var across = WeaponNativeDoorCrossing(doorApproachOrigin, door);
                                    var crossing = new UnitMoveTo(across) { CreatedByPlayer = true };
                                    doorCommand = crossing; actor.Commands.Run(crossing);
                                    var crossingTimer = Stopwatch.StartNew();
                                    while (!crossing.IsFinished && crossingTimer.Elapsed.TotalSeconds < 30 &&
                                        game.CurrentMode != GameModeType.Dialog && game.CurrentMode != GameModeType.Cutscene)
                                    {
                                        if (game.Player.IsInCombat) PrepareWeaponRouteCombat(combatAttempts, combatSteps);
                                        if (game.IsPaused) game.IsPaused = false;
                                        float crossingDistance = Vector3.Distance(prior, actor.Position);
                                        if (crossingDistance > 10f) { blocker = "Unqualified doorway crossing jump"; break; }
                                        distance += crossingDistance; prior = actor.Position; yield return 0;
                                    }
                                    interactionSteps.Add(new { nativeDoorCrossing = door.MapObject.Data.UniqueId,
                                        point = new[] { across.x, across.y, across.z }, result = crossing.Result.ToString(),
                                        actualDistance = Vector3.Distance(actor.Position, across),
                                        method = "Native walking across an actually opened doorway; no position or path override" });
                                    if (!crossing.IsFinished) crossing.Interrupt();
                                    if (blocker != null) break;
                                }
                                for (int i = 0; i < 12; i++) yield return 0;
                                approach = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(loot.transform.position).clampedPosition) { CreatedByPlayer = true };
                                command = new UnitInteractWithObject(loot) { CreatedByPlayer = true };
                                interactionIssued = false;
                                actor.Commands.Run(approach);
                                lastMovement = timer.Elapsed.TotalSeconds;
                                continue;
                                }
                            // Walk toward a nearby ordinary doorway before
                            // requiring its native reveal/interaction state.
                            // This explores rooms without forcing visibility,
                            // opening an unavailable object or overriding paths.
                            if (weapon.Key == "eastern:QuietCurrent" && doorScouts.Count < 12)
                            {
                                var scout = AdjacentOrdinaryWeaponRouteDoors(actor, doorScouts, false).FirstOrDefault();
                                if (scout != null)
                                {
                                    doorScouts.Add(scout.MapObject.Data.UniqueId);
                                    var towardDoor = Vector3.ProjectOnPlane(scout.transform.position - actor.Position, Vector3.up);
                                    var scoutPoint = ObstacleAnalyzer.GetNearestNode(scout.transform.position - towardDoor.normalized * 4f).clampedPosition;
                                    var scouting = new UnitMoveTo(scoutPoint)
                                        { CreatedByPlayer = true };
                                    doorCommand = scouting; actor.Commands.Run(scouting);
                                    var scoutTimer = Stopwatch.StartNew();
                                    while (!scouting.IsFinished && scoutTimer.Elapsed.TotalSeconds < 30 &&
                                        !Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(scout.MapObject.gameObject) &&
                                        game.CurrentMode != GameModeType.Dialog && game.CurrentMode != GameModeType.Cutscene)
                                    {
                                        if (game.Player.IsInCombat) PrepareWeaponRouteCombat(combatAttempts, combatSteps);
                                        if (game.IsPaused) game.IsPaused = false;
                                        float scoutDistance = Vector3.Distance(prior, actor.Position);
                                        if (scoutDistance > 10f) { blocker = "Unqualified ordinary doorway scouting jump"; break; }
                                        distance += scoutDistance; prior = actor.Position; yield return 0;
                                    }
                                    interactionSteps.Add(new { nativeDoorScout = scout.MapObject.Data.UniqueId,
                                        point = new[] { scoutPoint.x, scoutPoint.y, scoutPoint.z },
                                        actorNavArea = ObstacleAnalyzer.GetArea(actor.Position), pointNavArea = ObstacleAnalyzer.GetArea(scoutPoint),
                                        result = scouting.Result.ToString(), actualDistance = Vector3.Distance(actor.Position, scout.transform.position),
                                        nativePlayerInteractionAvailable = Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(scout.MapObject.gameObject),
                                        method = "Native walking toward a persistent ordinary doorway; no reveal, interaction or path override" });
                                    if (!scouting.IsFinished) scouting.Interrupt();
                                    if (blocker != null) break;
                                    approach = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(loot.transform.position).clampedPosition) { CreatedByPlayer = true };
                                    command = new UnitInteractWithObject(loot) { CreatedByPlayer = true };
                                    interactionIssued = false; actor.Commands.Run(approach);
                                    lastMovement = timer.Elapsed.TotalSeconds;
                                    continue;
                                }
                            }
                            // Corpses and chest colliders can stop MoveTo just
                            // short of their centre. Issue the normal interaction
                            // from the measured position; its own approach, LOS
                            // and visible window remain mandatory evidence.
                        }
                        if (!interactionIssued)
                        { actor.Commands.Run(command); interactionIssued = true; }
                    }
                    if (interactionIssued && command.IsFinished)
                    {
                        if (command.Result == UnitCommand.ResultType.Success && loot.Trap != null &&
                            !loot.Trap.TrapActive && trapRetries == 0 &&
                            !Resources.FindObjectsOfTypeAll<LootWindowController>().Any(value => value != null && value.IsShow))
                        {
                            interactionSteps.Add(new { result = command.Result.ToString(), trapActive = loot.Trap.TrapActive,
                                method = "First native interaction completed the trap check; issue a second normal loot interaction" });
                            trapRetries++; command = new UnitInteractWithObject(loot) { CreatedByPlayer = true };
                            actor.Commands.Run(command); continue;
                        }
                        if (command.Result != UnitCommand.ResultType.Interrupt || commandRetries >= 6 || interruptions.Count == 0) break;
                        // Let native combat, death and get-up controllers settle
                        // before an ordinary retry. A retry never removes a buff,
                        // alters an interaction condition or moves the actor.
                        if (nativeReady)
                        {
                            if (proneInterrupted) proneRetries++;
                            interactionSteps.Add(new { nativeCommandRetry = ++commandRetries, proneInterrupted,
                                canAct = actor.Descriptor.State.CanAct, calmSeconds = timer.Elapsed.TotalSeconds - calmSince,
                                method = "Reissue native walking and interaction after measured control recovery; no buff or position mutation" });
                            proneInterrupted = false; calmSince = -1;
                            approach = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(loot.transform.position).clampedPosition) { CreatedByPlayer = true };
                            command = new UnitInteractWithObject(loot) { CreatedByPlayer = true };
                            actor.Commands.Run(approach); interactionIssued = false;
                        }
                    }
                    float step = Vector3.Distance(prior, actor.Position);
                    if (_weaponNativeStoryPositionChanges != observedStoryChanges)
                    {
                        observedStoryChanges = _weaponNativeStoryPositionChanges;
                        cutsceneSteps.Add(new { measuredNativeStoryPositionChange = observedStoryChanges,
                            method = "Observed native background cutscene TranslocateUnit; excluded from walked distance" });
                        prior = actor.Position;
                        approach = new UnitMoveTo(ObstacleAnalyzer.GetNearestNode(loot.transform.position).clampedPosition) { CreatedByPlayer = true };
                        command = new UnitInteractWithObject(loot) { CreatedByPlayer = true };
                        actor.Commands.Run(approach); interactionIssued = false; lastMovement = timer.Elapsed.TotalSeconds;
                        continue;
                    }
                    if (step > 10f) { blocker = "A position jump occurred during the walking observation"; break; }
                    if (step > 0.005f) lastMovement = timer.Elapsed.TotalSeconds;
                    distance += step; prior = actor.Position;
                    if (points.Count == 0 || timer.Elapsed.TotalSeconds >= points.Count)
                    {
                        points.Add(new { seconds = timer.Elapsed.TotalSeconds, position = new[] { prior.x, prior.y, prior.z },
                            command = command.Result.ToString(), combat = actor.IsInCombat, playerCombat = game.Player.IsInCombat,
                            currentMode = game.CurrentMode.ToString(), timeScale = Time.timeScale,
                            gameTime = game.TimeController.GameTime.Ticks,
                            canInteract = loot.CanInteract(), enabled = loot.Enabled,
                            prone = actor.Descriptor.State.Prone.Active, proneRetries,
                            modes = Enum.GetValues(typeof(GameModeType)).Cast<GameModeType>().Where(game.IsModeActive).Select(value => value.ToString()).ToArray() });
                        WriteTeleportationForensicJson(System.IO.Path.Combine(_request.EvidenceDirectory, "weapon-native-route-progress.json"),
                            new { runId = _request.RunId, key = weapon.Key, points, navigation = WeaponRouteNavigation(actor),
                                interactions = WeaponRouteSceneInteractions(), phase = CampaignWeaponSceneObservation.Phase(),
                                nativeFloorPrelude = _weaponNativeFloorPrelude, doorSteps, dialogSteps, combatSteps,
                                interactionSteps,
                                nativePositionChanges = _weaponNativePositionChanges });
                    }
                    yield return 0;
                }
                game.IsPaused = true;
                var windows = Resources.FindObjectsOfTypeAll<LootWindowController>().Where(value => value != null && value.IsShow).ToArray();
                bool route = blocker == null && command.IsFinished && command.Result == UnitCommand.ResultType.Success &&
                    command.FinishedApproaching && windows.Length == 1 && distance > 0.1f &&
                    Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(loot.MapObject.gameObject);
                bool pickup = false;
                if (route)
                {
                    // Bind the actual visible native item slot for this exact
                    // container. Invoke its existing handler; no inventory Add.
                    for (int i = 0; i < 10; i++) yield return 0;
                    var collector = Resources.FindObjectsOfTypeAll<LootCollector>().SingleOrDefault(value =>
                        value != null && value.gameObject.activeInHierarchy && value.GetLootableObjects()
                            .Any(group => ReferenceEquals(group.Collection, loot.Loot)));
                    var slots = collector == null ? new Kingmaker.UI.Vendor.ItemTypicalSlot[0] : collector.GetLootableObjects()
                        .Where(group => ReferenceEquals(group.Collection, loot.Loot)).SelectMany(group => group.Slots)
                        .Where(slot => slot.Item != null && slot.Item.Blueprint == weapon.Item).ToArray();
                    if (slots.Length == 1)
                    {
                        windows[0].HandleSlotClick(slots[0]);
                        pickup = game.Player.Inventory.Count(weapon.Item) == initialCount + 1 && loot.Loot.Count(weapon.Item) == 0 &&
                            original.All(pair => pair.Key.Collection == loot.Loot && pair.Key.Count == pair.Value);
                    }
                    windows[0].OnButtonClose();
                    for (int i = 0; i < 4; i++) yield return 0;
                }
                _weaponRouteRecords.Add(new { key = weapon.Key, loot = weapon.Location.TargetGuid, item = weapon.Item.AssetGuid,
                    entryGuid = entry.Blueprint.AssetGuid, entryName, start = new[] { start.x, start.y, start.z },
                    finish = new[] { actor.Position.x, actor.Position.y, actor.Position.z }, walkedDistance = distance,
                    points, approachResult = approach.Result.ToString(), commandResult = command.Result.ToString(), command.IsFinished, command.FinishedApproaching,
                    commandApproachPoint = new[] { command.ApproachPoint.x, command.ApproachPoint.y, command.ApproachPoint.z },
                    commandCloseEnough = command.Executor == null ? (bool?)null : command.IsUnitEnoughClose, nativeFloorPrelude = _weaponNativeFloorPrelude,
                    actorState = new { dead = actor.Descriptor.State.IsDead, prone = actor.Descriptor.State.Prone.Active,
                        damage = actor.Descriptor.Damage, hp = actor.Stats.HitPoints.ModifiedValue,
                        buffs = actor.Buffs.Enumerable.Select(value => new { guid = value.Blueprint.AssetGuid, name = value.Blueprint.name,
                            remainingSeconds = value.TimeLeft.TotalSeconds }).ToArray() },
                    interruptions, doorSteps, dialogSteps, cutsceneSteps, combatSteps, interactionSteps, proneRetries, trapRetries, commandRetries,
                    questState = WeaponRouteQuestStates(),
                    dialogState = WeaponRouteDialogState(), navigation = WeaponRouteNavigation(actor),
                    nativeNavigationAreas = new { actor = ObstacleAnalyzer.GetArea(actor.Position),
                        target = ObstacleAnalyzer.GetArea(loot.transform.position),
                        targetPoint = ObstacleAnalyzer.GetNearestNode(loot.transform.position).clampedPosition.ToString() },
                    interactions = WeaponRouteSceneInteractions(),
                    finalModes = Enum.GetValues(typeof(GameModeType)).Cast<GameModeType>().Where(game.IsModeActive).Select(value => value.ToString()).ToArray(),
                    route = route ? "PASS" : "UNVERIFIED", pickup = pickup ? "PASS" : "UNVERIFIED",
                    nativeTreasurePreserved = original.All(pair => pair.Key.Collection == loot.Loot && pair.Key.Count == pair.Value),
                    blocker = blocker ?? (route ? pickup ? null : "Exact native visible loot slot did not transfer one item" :
                        "Native walking/interaction command did not establish an ordinary route"),
                    combatAndStory = "Closed disposable combat/key/lantern prerequisites and actual offered dialog choices recorded; native door, reveal, skill and interaction conditions remain active",
                    nativePositionChanges = _weaponNativePositionChanges,
                    visibilityAtInteraction = CampaignWeaponSceneObservation.Capture(loot),
                    nativePlayerInteractionAvailable = Kingmaker.Controllers.Clicks.Handlers.ClickMapObjectHandler.HasAvailableInteractions(loot.MapObject.gameObject),
                    phase = CampaignWeaponSceneObservation.Phase(), saveReload = "UNVERIFIED" });
                _weaponFindabilityAssertions.Add(Assertion("weapon-native-walking-" + weapon.Key,
                    "Native walking from the recorded ordinary entry reaches and interacts with the exact object",
                    "result=" + command.Result + ";distance=" + distance, route, "Native player command; no target-side teleportation, door unlock or reveal override"));
                _weaponFindabilityAssertions.Add(Assertion("weapon-native-pickup-" + weapon.Key,
                    "Actual native visible item-slot handler transfers exactly one weapon and preserves every original item/count",
                    "before=" + initialCount + ";after=" + game.Player.Inventory.Count(weapon.Item), pickup,
                    "LootWindowController.HandleSlotClick for the exact native container collection; no inventory Add"));
            }
            finally {
                if (!approach.IsFinished) approach.Interrupt();
                if (!command.IsFinished) command.Interrupt();
                if (doorCommand != null && !doorCommand.IsFinished) doorCommand.Interrupt();
                _weaponInterruptObservation = null;
                _context.Harmony.Unpatch(interruptMethod, observer);
                game.IsPaused = paused;
            }
            WriteTeleportationForensicJson(System.IO.Path.Combine(_request.EvidenceDirectory, "weapon-findability-routes.json"),
                new { runId = _request.RunId, routes = _weaponRouteRecords });
        }
    }
}
