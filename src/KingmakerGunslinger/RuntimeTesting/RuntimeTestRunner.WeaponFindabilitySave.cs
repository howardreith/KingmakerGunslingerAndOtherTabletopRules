using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.CraftMagicItemsCompatibility;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json.Linq;
using BetterVendors = KingmakerGunslinger.Acquisition.BetterVendors;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Closed, leased, test-only fixture. Production recovery has no save or
    // automatic grant hook. Only the owned disposable descriptor may be written.
    internal sealed partial class RuntimeTestRunner
    {
        private WeaponFindabilitySavePlan _weaponSavePlan;
        private IEnumerator<int> _weaponSaveSteps;
        private GuardedDisposableSaveLease _weaponSaveLease;
        private JObject _weaponSaveWitness, _weaponSavedInfo;
        private readonly List<RuntimeTestAssertion> _weaponSaveChecks = new List<RuntimeTestAssertion>();
        private readonly List<object> _weaponSaveEvents = new List<object>();
        private string WeaponSaveEvidence { get { return Path.Combine(_request.EvidenceDirectory, "weapon-findability-save.json"); } }

        private void WeaponSaveCheck(string name, bool pass, object observed)
        {
            _weaponSaveEvents.Add(new { name, observed });
            _weaponSaveChecks.Add(Assertion("weapon-save-" + name, "exact native owned-save fixture contract",
                Newtonsoft.Json.JsonConvert.SerializeObject(observed), pass, WeaponSaveEvidence));
            if (!pass) throw new InvalidOperationException("Weapon persistence invariant failed: " + name);
        }
        private void PollWeaponFindabilitySave()
        {
            RequireWeaponQualificationGame();
            if (!_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved || !_request.ExitAfterCompletion)
                throw new InvalidOperationException("Exact guarded seed/owned load and automatic exit required.");
            if (_weaponSaveSteps == null) _weaponSaveSteps = RunWeaponFindabilitySave().GetEnumerator();
            Exception failure = null;
            try { if (_weaponSaveSteps.MoveNext()) return; } catch (Exception error) { failure = error; }
            try { _weaponSaveSteps.Dispose(); } catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); }
            _weaponSaveSteps = null;
            WriteWeaponSaveReceipt(failure);
            Complete(CreateResult(failure == null ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Error,
                _weaponSaveChecks, failure == null ? null : failure.ToString()));
        }
        private void StopWeaponFindabilitySave(RuntimeTestResult result)
        {
            if (_weaponSaveSteps == null) return;
            var steps = _weaponSaveSteps; _weaponSaveSteps = null;
            try { steps.Dispose(); } catch (Exception error) { result.Status = RuntimeTestStatuses.Error; result.Diagnostics.Add(error.ToString()); }
            WriteWeaponSaveReceipt(new InvalidOperationException("Stopped before persistence completion."));
        }
        private void WriteWeaponSaveReceipt(Exception failure)
        {
            WriteTeleportationForensicJson(WeaponSaveEvidence, new {
                schemaVersion = 1, runId = _request.RunId, transactionId = _weaponSavePlan.Transaction,
                phase = _weaponSavePlan.Phase, weaponKey = _weaponSavePlan.WeaponKey, processId = Process.GetCurrentProcess().Id,
                dllSha256 = TeleportPersistencePlan.Hash(typeof(RuntimeTestRunner).Assembly.Location),
                witness = _weaponSaveWitness, savedInfo = _weaponSavedInfo, events = _weaponSaveEvents,
                saveWrites = _weaponSaveLease == null ? 0 : _weaponSaveLease.RoutineCount,
                unexpectedSaveWritingApiObserved = _workingSaveSmoke.WriteObserved,
                organicProgress = "UNVERIFIED: destination visits are explicit disposable fixture prerequisites",
                physicalLootRoutes = _weaponSavePlan.WeaponKey == null ? (object)"UNVERIFIED: separate scene/walking/pickup fixtures" :
                    new { routes = _weaponRouteRecords, scenes = _weaponFindabilityRecords,
                        assertions = _weaponFindabilityAssertions },
                optionalBetterVendors = new { availability = BetterVendors.BetterVendorsIntegrationStatusRegistry.Current.Availability.ToString(),
                    detail = BetterVendors.BetterVendorsIntegrationStatusRegistry.Current.Detail,
                    patched = BetterVendors.BetterVendorsCompatibilityCoordinator.IsPatched,
                    profile = BetterVendors.BetterVendorsCompatibilityCoordinator.Observation == null ? null :
                        new { version = BetterVendors.BetterVendorsCompatibilityCoordinator.Observation.ModVersion,
                            mvid = BetterVendors.BetterVendorsCompatibilityCoordinator.Observation.ModuleVersionId,
                            sha256 = BetterVendors.BetterVendorsCompatibilityCoordinator.Observation.FileSha256 } },
                error = failure == null ? null : failure.ToString() });
        }
        private static JArray WeaponInventoryWitness(ItemsCollection collection)
        {
            return new JArray(collection.Items.Select((item, index) => new JObject {
                ["index"] = index, ["guid"] = item.Blueprint.AssetGuid, ["count"] = item.Count,
                ["slot"] = item.InventorySlotIndex, ["identified"] = item.IsIdentified }));
        }
        private JObject WeaponPersistenceWitness(string merchantId)
        {
            var player = Game.Instance.Player; var owner = player.MainCharacter.Value.Descriptor;
            var merchant = player.AllCrossSceneUnits.Single(value => value.UniqueId == merchantId);
            return new JObject {
                ["owner"] = owner.Unit.UniqueId, ["characterLevel"] = owner.Progression.CharacterLevel,
                ["merchant"] = merchantId, ["money"] = player.Money,
                ["party"] = new JArray(player.Party.Select(value => value.UniqueId)),
                ["inventory"] = WeaponInventoryWitness(player.Inventory), ["stash"] = WeaponInventoryWitness(player.SharedStash),
                ["merchantStock"] = WeaponInventoryWitness(merchant.Get<UnitPartVendor>().Inventory),
                ["oldGeneratedContainer"] = WeaponOldGeneratedWitness(),
                ["ledger"] = JArray.FromObject(owner.Get<UnitPartCampaignWeaponRecovery>().Records),
                ["equipment"] = new JArray(player.AllCharacters.SelectMany(unit => unit.Body.AllSlots)
                    .Where(slot => slot.MaybeItem != null).Select(slot => new JObject {
                        ["owner"] = slot.Owner.Unit.UniqueId, ["slot"] = slot.GetType().FullName,
                        ["guid"] = slot.MaybeItem.Blueprint.AssetGuid })) };
        }
        private IEnumerable<int> RunWeaponFindabilitySave()
        {
            var game = Game.Instance; var player = game.Player; var owner = player.MainCharacter.Value.Descriptor;
            var plan = _weaponSavePlan; plan.RequireLease();
            bool paused = game.IsPaused; game.IsPaused = true;
            try
            {
                if (plan.WeaponKey != null)
                {
                    foreach (int tick in RunWeaponWorldLootPersistence()) yield return tick;
                }
                else if (plan.Phase == "prepare")
                {
                    _weaponSaveEvents.Add(new { name = "leveled-disposable-fixture", observed = PrepareWeaponRouteCharacter() });
                    foreach (int tick in PrepareWeaponOldGeneratedScene()) yield return tick;
                    // Exercise all production refusal/grant branches first, restoring
                    // every original item and visit before preparing persistence.
                    var fixtureResult = RunWeaponRecoveryFixtures();
                    _weaponSaveChecks.AddRange(fixtureResult.Assertions);
                    WeaponSaveCheck("recovery-fixtures", fixtureResult.Status == RuntimeTestStatuses.Pass, fixtureResult.Status);
                    var weapons = CampaignWeaponRegistry.Read().Where(value => value.Relocated && value.ModuleEnabled).ToArray();
                    foreach (var weapon in weapons)
                    {
                        var area = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintLoot>()
                            .Single(value => value.AssetGuid == weapon.Location.TargetGuid).Area;
                        if (!player.VisitedAreasData.ContainsKey(area)) player.VisitedAreasData.Add(area, new List<string>());
                        int before = player.Inventory.Count(weapon.Item);
                        string granted = CampaignWeaponRecovery.Grant(weapon.Key, player.GameId, true);
                        WeaponSaveCheck("selected-grant-" + weapon.Key, player.Inventory.Count(weapon.Item) == before + 1 &&
                            owner.Get<UnitPartCampaignWeaponRecovery>().HasRecord(weapon.Item.AssetGuid), granted);
                        // Retain Winter Reed for saved equipment refusal. All other
                        // fixture grants are removed so reload tests a missing copy.
                        if (weapon.Key != "eastern:WinterReed")
                        {
                            var item = player.Inventory.Items.Single(value => value.Blueprint == weapon.Item);
                            player.Inventory.Remove(item); item.Dispose();
                        }
                    }
                    var winter = weapons.Single(value => value.Key == "eastern:WinterReed");
                    var hand = player.AllCharacters.SelectMany(value => value.Body.AllSlots).OfType<HandSlot>()
                        .First(value => value.IsPrimaryHand && !value.HasItem && !value.PairSlot.HasItem && value.IsPossibleInsertItems());
                    hand.InsertItem(player.Inventory.Items.OfType<ItemEntityWeapon>().Single(value => value.Blueprint == winter.Item));
                    var enhancement = BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(BlueprintBootstrap.Library,
                        MidgameFirearmCatalog.EnhancementThreeGuid, "Enhancement3");
                    BlueprintItemWeapon upgraded = CraftMagicItemsReflectionBridge.BuildQualificationClone(winter.Item, enhancement);
                    WeaponSaveCheck("genuine-upgrade", upgraded != winter.Item && upgraded.Enchantments.Contains(enhancement) &&
                        !winter.Item.Enchantments.Contains(enhancement), upgraded.AssetGuid);
                    player.SharedStash.Add(new ItemEntityWeapon(upgraded));

                    // Native separate storage carrier, registered through the real
                    // cross-scene pool and companion-reference API. No existing NPC
                    // or companion is modified or used as a disposable carrier.
                    var blueprint = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                        SkeletalSalesmanStockCatalog.UnitGuid, "RE_Trader");
                    var merchant = new Kingmaker.UI.LevelUp.ChargenUnit(blueprint).Unit;
                    player.CrossSceneState.AddEntityData(merchant);
                    player.ExCompanions.Add(merchant);
                    var storageWeapon = weapons.Single(value => value.Key == "eastern:ThunderAtTheGate");
                    var owned = new ItemEntityWeapon(storageWeapon.Item); merchant.Inventory.Add(owned);
                    string refusal = CampaignWeaponRecovery.Grant(storageWeapon.Key, player.GameId, true);
                    var inspection = CampaignWeaponRecovery.Inspect(storageWeapon.Key);
                    WeaponSaveCheck("companion-storage-refusal", !ReferenceEquals(merchant.Inventory, player.Inventory) &&
                        inspection.OwnedCopies.Any(value => value.Contains(merchant.UniqueId)) &&
                        refusal.Contains("Owned canonical or supported upgraded copy found") && refusal.Contains(merchant.UniqueId), inspection.Describe());
                    merchant.Inventory.Remove(owned); owned.Dispose();
                    // The carrier is also a real native vendor. Buy each merchant-only
                    // firearm through VendorLogic, then persist its depleted stock.
                    var target = SkeletalSalesmanStockCatalog.Targets[0];
                    var table = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(BlueprintBootstrap.Library, target.Guid, target.Name);
                    var stock = merchant.Ensure<UnitPartVendor>(); stock.AddLoot(table);
                    long money = player.Money; player.GainMoney(1000000L);
                    try
                    {
                        game.Vendor.BeginTrading(merchant);
                        foreach (var design in MidgameFirearmCatalog.Entries)
                        {
                            var itemBlueprint = BlueprintBootstrap.MagicFirearms.Require(design.Symbol).Item;
                            var item = stock.Inventory.Items.OfType<ItemEntityWeapon>().Single(value => value.Blueprint == itemBlueprint);
                            long price = game.Vendor.GetItemBuyPrice(item), beforeMoney = player.Money;
                            game.Vendor.AddForBuy(item, 1); game.Vendor.Deal();
                            WeaponSaveCheck("merchant-purchase-" + design.Symbol, player.Money == beforeMoney - price &&
                                item.Collection == player.Inventory && stock.Inventory.Count(itemBlueprint) == 0,
                                new { guid = itemBlueprint.AssetGuid, price, debit = beforeMoney - player.Money, table = table.AssetGuid });
                        }
                    }
                    finally
                    {
                        if (game.Vendor.IsTrading) game.Vendor.EndTraiding();
                        player.SpendMoney(player.Money - money);
                    }
                    _weaponSaveWitness = WeaponPersistenceWitness(merchant.UniqueId);
                    foreach (int tick in SaveWeaponFindabilityOwned()) yield return tick;
                }
                else
                {
                    var expected = plan.Expected;
                    var actual = WeaponPersistenceWitness((string)expected["merchant"]);
                    WeaponSaveCheck("native-fresh-process-witness", JToken.DeepEquals(expected, actual), actual);
                    WeaponSaveCheck("old-generated-reload-remains-missing", RequireWeaponWorldObject(
                        CampaignWeaponRegistry.Require("eastern:WinterReed")).Loot.Count(
                        CampaignWeaponRegistry.Require("eastern:WinterReed").Item) == 0, actual["oldGeneratedContainer"]);
                    foreach (var weapon in CampaignWeaponRegistry.Read().Where(value => value.Relocated && value.ModuleEnabled))
                    {
                        var before = WeaponInventoryWitness(player.Inventory); var stash = WeaponInventoryWitness(player.SharedStash);
                        var inspection = CampaignWeaponRecovery.Inspect(weapon.Key);
                        string refused = CampaignWeaponRecovery.Grant(weapon.Key, player.GameId, true);
                        bool owned = inspection.OwnedCopies.Count > 0;
                        WeaponSaveCheck("reload-refusal-" + weapon.Key,
                            refused.Contains(owned ? "Owned canonical or supported upgraded copy found" : "already recorded") &&
                            JToken.DeepEquals(before, WeaponInventoryWitness(player.Inventory)) &&
                            JToken.DeepEquals(stash, WeaponInventoryWitness(player.SharedStash)), refused);
                    }
                    var merchant = player.AllCrossSceneUnits.Single(value => value.UniqueId == (string)expected["merchant"]);
                    game.Vendor.BeginTrading(merchant);
                    try
                    {
                        foreach (var design in MidgameFirearmCatalog.Entries)
                        {
                            var item = BlueprintBootstrap.MagicFirearms.Require(design.Symbol).Item;
                            WeaponSaveCheck("merchant-reload-" + design.Symbol, merchant.Get<UnitPartVendor>().Inventory.Count(item) == 0 &&
                                player.Inventory.Count(item) == 1, new { guid = item.AssetGuid, ownedCount = player.Inventory.Count(item) });
                        }
                    }
                    finally { if (game.Vendor.IsTrading) game.Vendor.EndTraiding(); }
                    _weaponSaveWitness = actual; _weaponSavedInfo = plan.Input;
                }
                WeaponSaveCheck("no-unrelated-save-writes", !_workingSaveSmoke.WriteObserved, _workingSaveSmoke.WriteObserved);
            }
            finally { game.IsPaused = paused; }
        }

        private LootComponent RequireWeaponWorldObject(CampaignWeaponPlacement weapon)
        {
            var objects = CampaignWeaponSceneObservation.Find(weapon.Location.TargetGuid).Where(value =>
                value.gameObject.activeInHierarchy && value.MapObject != null && value.MapObject.Data != null &&
                value.MapObject.Data.IsInGame).ToArray();
            if (objects.Length != 1) throw new InvalidOperationException("The exact active world object is not unique: " + weapon.Key);
            return objects[0];
        }

        private JObject WeaponOldGeneratedWitness()
        {
            var weapon = CampaignWeaponRegistry.Require("eastern:WinterReed");
            var loot = RequireWeaponWorldObject(weapon);
            var data = loot.MapObject.Data.GetComponentData<LootComponent.LootPersistentData>();
            var known = typeof(LootComponent.LootPersistentData).GetField("m_KnownItems", BindingFlags.Instance | BindingFlags.NonPublic);
            if (data == null || known == null) throw new InvalidOperationException("Native generated container history is unavailable.");
            return new JObject { ["weaponKey"] = weapon.Key, ["lootGuid"] = weapon.Location.TargetGuid,
                ["scene"] = loot.gameObject.scene.name, ["entityId"] = loot.MapObject.Data.UniqueId,
                ["areaGuid"] = Game.Instance.CurrentlyLoadedArea.AssetGuid,
                ["knownHistory"] = known.GetValue(data) == null ? "unknown" : "known",
                ["weaponCount"] = loot.Loot.Count(weapon.Item), ["contents"] = WeaponInventoryWitness(loot.Loot) };
        }

        private IEnumerable<int> PrepareWeaponOldGeneratedScene()
        {
            var game = Game.Instance; var weapon = CampaignWeaponRegistry.Require("eastern:WinterReed");
            var target = BlueprintLibraryLookup.RequireExact<BlueprintLoot>(BlueprintBootstrap.Library,
                weapon.Location.TargetGuid, weapon.Location.TargetName);
            if (game.Player.VisitedAreasData.ContainsKey(target.Area))
                throw new InvalidOperationException("The old-generation fixture requires a never-visited exact destination.");
            var published = target.Items;
            try
            {
                // Generate this one real scene inventory with its native treasure
                // and old layout. This is a leased test fixture, never production
                // recovery or a mutation of an existing save/container.
                target.Items = published.Where(value => value == null || value.Item != weapon.Item).ToArray();
                var entry = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintAreaEnterPoint>().Single(value =>
                    value.Area == target.Area && value.name == "TrollhoundLair_RightCaveEntrance");
                foreach (int tick in WhiteoutLoadArea(target.Area, entry)) yield return tick;
            }
            finally { target.Items = published; }
            var loot = RequireWeaponWorldObject(weapon);
            var data = loot.MapObject.Data.GetComponentData<LootComponent.LootPersistentData>();
            var native = loot.Loot.Items.ToDictionary(value => value, value => value.Count);
            WeaponSaveCheck("old-generated-real-scene", data != null && loot.Loot.Count(weapon.Item) == 0 &&
                CampaignWeaponNativeReference.Signature(target.Items.Where(value => value == null || value.Item != weapon.Item)) ==
                    CampaignWeaponNativeReference.Require(target.AssetGuid), CampaignWeaponSceneObservation.Capture(loot));
            var update = typeof(LootComponent.LootPersistentData).GetMethod("UpdateLoot", BindingFlags.Instance | BindingFlags.NonPublic);
            var known = typeof(LootComponent.LootPersistentData).GetField("m_KnownItems", BindingFlags.Instance | BindingFlags.NonPublic);
            if (update == null || known == null) throw new InvalidOperationException("Native generated-loot history contract absent.");
            update.Invoke(data, new object[] { published });
            WeaponSaveCheck("old-generated-known-scene-delta", loot.Loot.Count(weapon.Item) == 1 &&
                native.All(pair => pair.Key.Collection == loot.Loot && pair.Key.Count == pair.Value),
                "actual generated scene inventory; native UpdateLoot preserves original object/count references");
            var added = loot.Loot.Items.Single(value => value.Blueprint == weapon.Item);
            loot.Loot.Remove(added); added.Dispose();
            update.Invoke(data, new object[] { published });
            WeaponSaveCheck("old-generated-extracted-scene-no-respawn", loot.Loot.Count(weapon.Item) == 0 &&
                native.All(pair => pair.Key.Collection == loot.Loot && pair.Key.Count == pair.Value), CampaignWeaponSceneObservation.Capture(loot));
            known.SetValue(data, null);
            update.Invoke(data, new object[] { published });
            WeaponSaveCheck("old-generated-unknown-scene-no-refill", loot.Loot.Count(weapon.Item) == 0 &&
                native.All(pair => pair.Key.Collection == loot.Loot && pair.Key.Count == pair.Value), WeaponOldGeneratedWitness());
            // Reconstruct the actual scene through the native reload lifecycle,
            // then persist this missing-history container alongside recovery.
            var before = WeaponOldGeneratedWitness(); var scene = new CircleSceneObservation(); scene.Start();
            try
            {
                game.ReloadArea(); var watch = Stopwatch.StartNew();
                while (!scene.Ready)
                {
                    if (watch.Elapsed.TotalSeconds > 120) throw new InvalidOperationException("Old generated scene revisit did not complete.");
                    game.IsPaused = true; yield return 0;
                }
                WeaponSaveCheck("old-generated-native-revisit", scene.ActualReload && !_workingSaveSmoke.WriteObserved &&
                    JToken.DeepEquals(before, WeaponOldGeneratedWitness()), WeaponOldGeneratedWitness());
            }
            finally { scene.Stop(); }
        }

        private JObject WeaponWorldWitness(CampaignWeaponPlacement weapon)
        {
            var player = Game.Instance.Player; var owner = player.MainCharacter.Value.Descriptor;
            var loot = RequireWeaponWorldObject(weapon);
            return new JObject {
                ["weaponKey"] = weapon.Key, ["itemGuid"] = weapon.Item.AssetGuid,
                ["lootGuid"] = weapon.Location.TargetGuid, ["lootName"] = weapon.Location.TargetName,
                ["areaGuid"] = Game.Instance.CurrentlyLoadedArea.AssetGuid,
                ["scene"] = loot.gameObject.scene.name, ["entityId"] = loot.MapObject.Data.UniqueId,
                ["owner"] = owner.Unit.UniqueId, ["characterLevel"] = owner.Progression.CharacterLevel,
                ["money"] = player.Money, ["party"] = new JArray(player.Party.Select(value => value.UniqueId)),
                ["inventory"] = WeaponInventoryWitness(player.Inventory), ["stash"] = WeaponInventoryWitness(player.SharedStash),
                ["container"] = WeaponInventoryWitness(loot.Loot),
                ["phase"] = JObject.FromObject(CampaignWeaponSceneObservation.Phase()),
                ["equipment"] = new JArray(player.AllCharacters.SelectMany(unit => unit.Body.AllSlots)
                    .Where(slot => slot.MaybeItem != null).Select(slot => new JObject {
                        ["owner"] = slot.Owner.Unit.UniqueId, ["slot"] = slot.GetType().FullName,
                        ["guid"] = slot.MaybeItem.Blueprint.AssetGuid })) };
        }

        private IEnumerable<int> RunWeaponWorldLootPersistence()
        {
            var plan = _weaponSavePlan; var game = Game.Instance;
            var weapon = CampaignWeaponRegistry.Require(plan.WeaponKey);
            if (plan.Phase == "prepare")
            {
                foreach (int tick in ObserveSelectedWeaponRoute(weapon.Key)) yield return tick;
                _weaponSaveChecks.AddRange(_weaponFindabilityAssertions);
                WeaponSaveCheck("normal-world-pickup", _weaponFindabilityAssertions.Count == 3 &&
                    _weaponFindabilityAssertions.All(value => value.Status == RuntimeTestStatuses.Pass), weapon.Key);
                var before = WeaponWorldWitness(weapon);
                WeaponSaveCheck("one-picked-up-depleted-source", game.Player.Inventory.Count(weapon.Item) == 1 &&
                    RequireWeaponWorldObject(weapon).Loot.Count(weapon.Item) == 0, before);
                // Native force unload and scene reconstruction, without an autosave,
                // refill, a second pickup, or target-side positioning.
                var scene = new CircleSceneObservation(); scene.Start();
                try
                {
                    game.ReloadArea(); var clock = Stopwatch.StartNew();
                    while (!scene.Ready)
                    {
                        if (clock.Elapsed.TotalSeconds > 120) throw new InvalidOperationException("Native world revisit did not finish.");
                        game.IsPaused = true; yield return 0;
                    }
                    WeaponSaveCheck("native-revisit-unload", scene.ActualReload && !_workingSaveSmoke.WriteObserved, scene.Events);
                    var after = WeaponWorldWitness(weapon);
                    WeaponSaveCheck("native-revisit-preserves-pickup-and-treasure", JToken.DeepEquals(before, after), after);
                    _weaponSaveWitness = after;
                }
                finally { scene.Stop(); }
                foreach (int tick in SaveWeaponFindabilityOwned()) yield return tick;
            }
            else
            {
                var actual = WeaponWorldWitness(weapon);
                WeaponSaveCheck("world-native-fresh-process-witness", JToken.DeepEquals(plan.Expected, actual), actual);
                WeaponSaveCheck("world-reload-no-respawn", game.Player.Inventory.Count(weapon.Item) == 1 &&
                    RequireWeaponWorldObject(weapon).Loot.Count(weapon.Item) == 0,
                    CampaignWeaponSceneObservation.Capture(RequireWeaponWorldObject(weapon)));
                _weaponSaveWitness = actual; _weaponSavedInfo = plan.Input;
            }
        }
        private IEnumerable<int> SaveWeaponFindabilityOwned()
        {
            var game = Game.Instance; var plan = _weaponSavePlan; plan.RequireLease();
            if (!WeaponFindabilitySaveContract.MayWrite(plan.Transaction, plan.Phase, plan.OutputName, true, true) ||
                !game.SaveManager.IsSaveAllowed() || game.SaveManager.CommitInProgress ||
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance.OnlyOneSave.CurrentValue)
                throw new InvalidOperationException("One new leased manual descriptor required.");
            var requested = game.SaveManager.CreateNewSave(plan.OutputName);
            var lease = new GuardedDisposableSaveLease(requested, plan.OutputName, game.SaveManager.SavePath, save => {
                plan.RequireLease();
                WriteTeleportationForensicJson(Path.Combine(_request.EvidenceDirectory, "weapon-findability-owned-save.json"), new {
                    schemaVersion = 1, runId = _request.RunId, transactionId = plan.Transaction, phase = plan.Phase,
                    name = save.Name, file = save.FileName, path = Path.GetFullPath(save.FolderName), gameId = save.GameId,
                    nativePreparedPath = Path.GetFullPath(save.FolderName), nativePreparedInitiallyAbsent = true,
                    existedBeforePreparation = false, lifecycle = "native-prepared-before-write" });
            });
            _weaponSaveLease = lease; _workingSaveSmoke.ArmDisposableSave(lease);
            bool completed = false; game.SaveGame(requested, () => { completed = true; });
            var watch = Stopwatch.StartNew();
            while (!completed || lease.Saved == null || game.SaveManager.CommitInProgress ||
                lease.Saved.OperationState != SaveInfo.StateType.None || LoadingProcess.Instance.IsLoadingInProcess || !File.Exists(lease.Saved.FolderName))
            { if (watch.Elapsed.TotalSeconds > 120) throw new InvalidOperationException("Owned native save did not complete."); yield return 0; }
            var saved = lease.Saved;
            _weaponSavedInfo = new JObject { ["name"] = saved.Name, ["file"] = saved.FileName, ["path"] = saved.FolderName,
                ["sha256"] = TeleportPersistencePlan.Hash(saved.FolderName), ["gameName"] = saved.GameName,
                ["gameId"] = saved.GameId, ["areaName"] = saved.Area.name, ["partyCount"] = saved.PartyPortraits.Count };
            WeaponSaveCheck("native-owned-commit", lease.RoutineCount == 1 && !_workingSaveSmoke.WriteObserved &&
                saved.Name == plan.OutputName && saved.GameId == game.Player.GameId && saved.Area == game.CurrentlyLoadedArea &&
                saved.PartyPortraits.Count == game.Player.Party.Count,
                new { name = saved.Name, file = saved.FileName, writes = lease.RoutineCount, lease.StashedAreaCount });
        }
    }
}
