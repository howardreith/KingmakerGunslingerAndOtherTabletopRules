using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.View.MapObjects;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _weaponFindabilitySteps;
        private readonly List<RuntimeTestAssertion> _weaponFindabilityAssertions = new List<RuntimeTestAssertion>();
        private readonly List<object> _weaponFindabilityRecords = new List<object>();

        private RuntimeTestResult RunWeaponFindabilityBlueprints()
        {
            RequireWeaponQualificationGame();
            var weapons = CampaignWeaponRegistry.Read();
            var allLoot = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintLoot>().ToArray();
            var before = allLoot.ToDictionary(value => value, value => value.Items);
            var snapshots = allLoot.ToDictionary(value => value, value => (value.Items ?? new LootEntry[0]).ToArray());
            var assertions = new List<RuntimeTestAssertion>();
            var records = new List<object>();
            // Read-only coexistence witness for the released v145 assignment.
            // No replacement/remapping: inspect the actual registered consumers.
            if (_context.FeatureModules.Active.EasternWeapons)
            {
                var nodachi = ProjectAssetIcons.RequireIcon("nodachi");
                foreach (string guid in new[] { "5ae9f898e45846d19d3802caf91e06b6",
                    "af205733f7fe49838edb37cdf1b90cbb", "4caf60ed8b264701a3965288a65eebc2",
                    "e17fafa6f75641f8a2e3fe4b6f71da78" })
                {
                    var feature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                        BlueprintBootstrap.Library, guid, "released Heirloom Nodachi icon consumer");
                    assertions.Add(Assertion("heirloom-nodachi-icon-" + guid,
                        "same project Nodachi sprite on the selection and all three visible choices",
                        "consumer=" + feature.name + ";icon=" + (feature.Icon == null ? "<null>" : feature.Icon.name),
                        nodachi != null && ReferenceEquals(feature.Icon, nodachi),
                        "read-only actual blueprint consumer and cached original icon; no UI/aesthetic approval claim"));
                }
            }
            try
            {
                if (_context.FeatureModules.Active.Gunslinger)
                    RareFirearmCampaignLootBlueprints.Publish(BlueprintBootstrap.Library, BlueprintBootstrap.MagicFirearms, _context.Logger).Validate();
                if (_context.FeatureModules.Active.EasternWeapons)
                    EasternWeaponCampaignBlueprints.Publish(BlueprintBootstrap.Library, BlueprintBootstrap.EasternWeapons, _context.Logger).Validate();
                assertions.Add(Assertion("weapon-repeat-publication", "Repeat publication leaves every BlueprintLoot row and array unchanged",
                    "targets=" + allLoot.Length, allLoot.All(value => ReferenceEquals(before[value], value.Items) &&
                        snapshots[value].SequenceEqual(value.Items ?? new LootEntry[0])), "live native publication routines; actual row/array references"));
                foreach (var weapon in weapons)
                {
                    var target = allLoot.SingleOrDefault(value => value.AssetGuid == weapon.Location.TargetGuid);
                    bool identity = target != null && target.name == weapon.Location.TargetName && target.Area != null && target.Area.name == weapon.Location.AreaName;
                    LootEntry[] rows = target == null ? new LootEntry[0] : target.Items ?? new LootEntry[0];
                    string native = CampaignWeaponNativeReference.Signature(rows.Where(value => value == null || value.Item != weapon.Item));
                    string expectedNative = CampaignWeaponNativeReference.Require(weapon.Location.TargetGuid);
                    int copies = allLoot.Sum(value => (value.Items ?? new LootEntry[0]).Count(row => row != null && row.Item == weapon.Item));
                    bool single = copies == (weapon.ModuleEnabled ? 1 : 0) && rows.Count(value => value != null && value.Item == weapon.Item && value.Count == 1) == (weapon.ModuleEnabled ? 1 : 0);
                    assertions.Add(Assertion("weapon-blueprint-" + weapon.Key, "Exact installed GUID/name/area and one intended module-gated assignment",
                        "item=" + weapon.Item.AssetGuid + ";target=" + weapon.Location.TargetGuid + ";name=" + (target == null ? "<absent>" : target.name) + ";copies=" + copies,
                        identity && single, "complete 29-weapon registry and every installed BlueprintLoot source"));
                    assertions.Add(Assertion("weapon-native-treasure-" + weapon.Key, "Ordered native rows match the SHA-256-bound 2.1.4 reference in installed 2.1.7b",
                        native, native == expectedNative, "native item GUID and count per row, preserving duplicate rows and gold; reference=" + CampaignWeaponNativeReference.ArchiveSha256));
                    var fresh = new LootComponent.LootPersistentData(rows);
                    try
                    {
                        assertions.Add(Assertion("weapon-fresh-generation-" + weapon.Key, "Fresh native persistent inventory contains exactly one intended weapon when module is ON",
                            "count=" + fresh.Loot.Count(weapon.Item), fresh.Loot.Count(weapon.Item) == (weapon.ModuleEnabled ? 1 : 0),
                            "actual LootPersistentData constructor and native ItemsCollection; detached inventory, not scene availability"));
                    }
                    finally { fresh.Dispose(); }
                    var old = new LootComponent.LootPersistentData(rows.Where(value => value == null || value.Item != weapon.Item));
                    try
                    {
                        assertions.Add(Assertion("weapon-old-generation-" + weapon.Key, "An already generated inventory is tested independently of fresh blueprint publication",
                            "weaponCount=" + old.Loot.Count(weapon.Item), old.Loot.Count(weapon.Item) == 0,
                            "native old-layout persistent inventory without the appended weapon; no container refill invoked"));
                        // Exercise the game's actual delta reconciliation, which
                        // OnDataSetted calls when a generated object is attached.
                        // This is not a mod refill or an old-campaign repair guarantee.
                        var update = typeof(LootComponent.LootPersistentData).GetMethod("UpdateLoot", BindingFlags.Instance | BindingFlags.NonPublic);
                        var known = typeof(LootComponent.LootPersistentData).GetField("m_KnownItems", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (update == null || known == null) throw new InvalidOperationException("Exact installed native generated-loot contract absent.");
                        var nativeBefore = old.Loot.Items.ToDictionary(value => value, value => value.Count);
                        update.Invoke(old, new object[] { rows });
                        int reconciled = old.Loot.Count(weapon.Item);
                        assertions.Add(Assertion("weapon-generated-known-items-" + weapon.Key,
                            "Native known-items delta adds only the new row; original generated treasure remains intact",
                            "count=" + reconciled, reconciled == (weapon.ModuleEnabled ? 1 : 0) &&
                                nativeBefore.All(pair => pair.Key.Collection == old.Loot && pair.Key.Count == pair.Value),
                            "installed LootPersistentData.UpdateLoot; an engine behavior observation, not guaranteed old-save recovery"));
                        if (reconciled > 0) old.Loot.Remove(weapon.Item, reconciled);
                        update.Invoke(old, new object[] { rows });
                        assertions.Add(Assertion("weapon-generated-purchased-depleted-" + weapon.Key,
                            "Native known-items reconciliation does not replace a previously extracted row",
                            "count=" + old.Loot.Count(weapon.Item), old.Loot.Count(weapon.Item) == 0,
                            "actual native repeated object-data attachment routine after extraction"));
                        known.SetValue(old, null); update.Invoke(old, new object[] { rows });
                        assertions.Add(Assertion("weapon-generated-unknown-items-" + weapon.Key,
                            "An old generated inventory without known-items history remains depleted",
                            "count=" + old.Loot.Count(weapon.Item), old.Loot.Count(weapon.Item) == 0,
                            "actual native null-history branch; publication cannot guarantee repair of an old campaign"));
                    }
                    finally { old.Dispose(); }
                    records.Add(new { itemGuid = weapon.Item.AssetGuid, itemName = weapon.Item.Name,
                        targetGuid = weapon.Location.TargetGuid, targetName = weapon.Location.TargetName,
                        areaName = target == null || target.Area == null ? null : target.Area.name,
                        areaGuid = target == null || target.Area == null ? null : target.Area.AssetGuid,
                        nativeRows = native, nativeReferenceMatch = native == expectedNative,
                        blueprintPublication = identity && single ? "PASS" : "FAIL",
                        scenePresence = "UNVERIFIED", room = "UNVERIFIED", position = "UNVERIFIED",
                        phase = "UNVERIFIED", normalRoute = "UNVERIFIED", pickup = "UNVERIFIED",
                        revisit = "UNVERIFIED", saveReload = "UNVERIFIED", provenance = "guarded installed blueprint and native inventory fixture" });
                }
                // Seed only known mod-owned rows in the nine retired definitions,
                // then execute the actual publishers. Native rows and their exact
                // order/reference identity must survive normalization and cleanup.
                var retired = RareFirearmCampaignLootBlueprints.CleanupSpecs.Where(value =>
                    value.Guid == "559739642f21aaf40847f4ddcbe3db79" || value.Guid == "2df91222314044b4da37b7ee83841873" ||
                    value.Guid == "b34367a637010f743815aed5875152bd")
                    .Select(value => value.Guid).Concat(EasternWeaponCampaignBlueprints.CleanupSpecs
                        .Where(value => value.Band == "findability cleanup").Select(value => value.Guid)).ToArray();
                foreach (string guid in retired)
                {
                    var target = allLoot.Single(value => value.AssetGuid == guid);
                    bool firearm = RareFirearmCampaignLootBlueprints.CleanupSpecs.Any(value => value.Guid == guid);
                    var weapon = weapons.FirstOrDefault(value => value.ModuleEnabled && (firearm ? value.Key.StartsWith("KMG.", StringComparison.Ordinal) : value.Key.StartsWith("eastern:", StringComparison.Ordinal)));
                    if (weapon == null) continue; // Disabled module cleanup is covered by its boundary profile.
                    var original = target.Items ?? new LootEntry[0];
                    target.Items = original.Concat(new[] { new LootEntry { Item = weapon.Item, Count = 2 }, new LootEntry { Item = weapon.Item, Count = 1 } }).ToArray();
                    if (firearm) RareFirearmCampaignLootBlueprints.Publish(BlueprintBootstrap.Library, BlueprintBootstrap.MagicFirearms, _context.Logger).Validate();
                    else EasternWeaponCampaignBlueprints.Publish(BlueprintBootstrap.Library, BlueprintBootstrap.EasternWeapons, _context.Logger).Validate();
                    assertions.Add(Assertion("weapon-retired-cleanup-" + guid, "Actual publisher removes only relevant mod rows; native rows remain byte-order/reference equivalent",
                        CampaignWeaponNativeReference.Signature(target.Items ?? new LootEntry[0]), original.SequenceEqual(target.Items ?? new LootEntry[0]),
                        "seeded duplicate mod-owned rows in exact retired native definitions; all blueprint snapshots restored afterward"));
                }
            }
            finally { foreach (var pair in before) pair.Key.Items = pair.Value; }
            WriteTeleportationForensicJson(System.IO.Path.Combine(_request.EvidenceDirectory, "weapon-findability-blueprints.json"),
                new { runId = _request.RunId, version = _context.ModEntry.Info.Version, weapons = records,
                    physicalQualification = "UNVERIFIED; blueprint and detached-inventory evidence only", assertions });
            return CreateResult(assertions.All(value => value.Status == "PASS") ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private void PollWeaponFindabilityScenes()
        {
            RequireWeaponQualificationGame();
            if (!_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Exact disposable working-save load and intact write guard required.");
            if (_weaponFindabilitySteps == null) _weaponFindabilitySteps =
                (_request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveWeaponRoute ?
                    ObserveSelectedWeaponRoute() : ObserveWeaponFindabilityScenes()).GetEnumerator();
            Exception failure = null;
            try { if (_weaponFindabilitySteps.MoveNext()) return; }
            catch (Exception error) { failure = error; }
            try { _weaponFindabilitySteps.Dispose(); }
            catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); }
            _weaponFindabilitySteps = null;
            WriteTeleportationForensicJson(System.IO.Path.Combine(_request.EvidenceDirectory, "weapon-findability-scenes.json"),
                new { runId = _request.RunId, version = _context.ModEntry.Info.Version, observations = _weaponFindabilityRecords,
                    physicalQualification = "Qualification gates are reported separately; scene loading alone cannot prove a normal route, pickup or persistence",
                    saveWriteObserved = _workingSaveSmoke.WriteObserved, error = failure == null ? null : failure.ToString(), assertions = _weaponFindabilityAssertions });
            Complete(CreateResult(failure == null && _weaponFindabilityAssertions.All(value => value.Status == "PASS") ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                _weaponFindabilityAssertions, failure == null ? null : failure.ToString()));
        }

        private static void RequireWeaponQualificationGame()
        {
            string version = Kingmaker.GameVersion.GetVersion();
            if (version != "2.1.7b")
                throw new InvalidOperationException("Weapon release qualification requires installed Kingmaker 2.1.7b; observed=" + version);
        }

        private void StopWeaponFindability(RuntimeTestResult result)
        {
            if (_weaponFindabilitySteps == null) return;
            var steps = _weaponFindabilitySteps; _weaponFindabilitySteps = null;
            try { steps.Dispose(); }
            catch (Exception error) { result.Status = RuntimeTestStatuses.Error; result.Diagnostics.Add(error.ToString()); }
            WriteTeleportationForensicJson(System.IO.Path.Combine(_request.EvidenceDirectory, "weapon-findability-scenes-progress.json"),
                new { runId = _request.RunId, observations = _weaponFindabilityRecords,
                    assertions = _weaponFindabilityAssertions, interrupted = true,
                    physicalQualification = "UNVERIFIED; interrupted scene fixture" });
        }

        private IEnumerable<int> ObserveWeaponFindabilityScenes()
        {
            var game = Game.Instance; var origin = game.CurrentlyLoadedArea;
            var weapons = CampaignWeaponRegistry.Read();
            _workingSaveSmoke.RestrictToTransactionOwnedWrites();
            foreach (var group in weapons.GroupBy(value => value.Location.AreaName))
            {
                var owners = group.Select(value => BlueprintBootstrap.Library.GetAllBlueprints()
                    .OfType<BlueprintLoot>().Single(loot => loot.AssetGuid == value.Location.TargetGuid).Area).Distinct().ToArray();
                if (owners.Length != 1 || owners[0] == null || owners[0].name != group.Key)
                    throw new InvalidOperationException("Exact loot owner mismatch: " + group.Key);
                var area = owners[0];
                if (area.name == "HouseAtTheEdgeOfTime_2ndFloor")
                {
                    // Explicit, request-local alternate-phase fixture. Record the
                    // native flag and scene instead of translating guide labels.
                    var phase = BlueprintLibraryLookup.RequireExact<BlueprintUnlockableFlag>(BlueprintBootstrap.Library,
                        "db94ef898ad95944788d3da6b22a5e31", "Phase");
                    game.Player.UnlockableFlags.SetFlagValue(phase, 2);
                }
                // This entry is not offered as route proof. Record every native
                // entry so the subsequent walking fixture can bind an ordinary
                // entrance without guessing from similarly named definitions.
                var entries = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintAreaEnterPoint>().Where(value => value.Area == area).ToArray();
                foreach (int tick in WhiteoutLoadArea(area, entries.Length == 1 ? entries[0] : null)) yield return tick;
                for (int i = 0; i < 3; i++) { game.IsPaused = true; yield return 0; }
                foreach (var weapon in group)
                {
                    var objects = CampaignWeaponSceneObservation.Find(weapon.Location.TargetGuid);
                    bool active = objects.Any(value => value.gameObject.activeInHierarchy && value.MapObject != null &&
                        value.MapObject.Data != null && !string.IsNullOrWhiteSpace(value.MapObject.Data.UniqueId) &&
                        value.MapObject.Data.IsInGame && !(value.MapObject is DroppedLoot));
                    _weaponFindabilityRecords.Add(new { key = weapon.Key, itemGuid = weapon.Item.AssetGuid,
                        lootGuid = weapon.Location.TargetGuid, lootName = weapon.Location.TargetName,
                        areaGuid = area.AssetGuid, areaName = area.name, mapName = area.AreaName.ToString(),
                        phase = CampaignWeaponSceneObservation.Phase(), chapter = game.Player.Chapter,
                        entries = entries.Select(value => new { guid = value.AssetGuid, name = value.name,
                            sceneObject = Kingmaker.View.AreaEnterPoint.FindAreaEnterPointOnScene(value) == null ? null :
                                new { scene = Kingmaker.View.AreaEnterPoint.FindAreaEnterPointOnScene(value).gameObject.scene.name,
                                    position = new[] { Kingmaker.View.AreaEnterPoint.FindAreaEnterPointOnScene(value).transform.position.x,
                                        Kingmaker.View.AreaEnterPoint.FindAreaEnterPointOnScene(value).transform.position.y,
                                        Kingmaker.View.AreaEnterPoint.FindAreaEnterPointOnScene(value).transform.position.z } } }).ToArray(),
                        dynamicScenes = new[] { (BlueprintAreaPart)area }.Concat(area.GetParts()).Select(part => new {
                            guid = part.AssetGuid, name = part.name,
                            active = part.GetActiveDynamicScenes().Select(scene => scene.SceneName).ToArray(),
                            available = part.GetAdditionalDynamicScenes().Select(scene => scene.SceneName).ToArray() }).ToArray(),
                        objects = objects.Select(CampaignWeaponSceneObservation.Capture).ToArray(),
                        scenePresence = active ? "PASS" : "UNVERIFIED", normalRoute = "UNVERIFIED",
                        pickup = "UNVERIFIED", revisit = "UNVERIFIED", saveReload = "UNVERIFIED",
                        questAndDlcConditions = "UNVERIFIED; native fixture-area load bypasses organic map progression" });
                    _weaponFindabilityAssertions.Add(Assertion("weapon-scene-presence-" + weapon.Key,
                        "At least one active persistent scene object uses the exact target; missing evidence fails this gate",
                        "matches=" + objects.Length + ";active=" + active, active,
                        "installed 2.1.7b scene LootComponent.LootTables; physical route and pickup remain UNVERIFIED"));
                }
                // Keep partial evidence durable even if a later area cannot be loaded.
                WriteTeleportationForensicJson(System.IO.Path.Combine(_request.EvidenceDirectory, "weapon-findability-scenes-progress.json"),
                    new { runId = _request.RunId, observations = _weaponFindabilityRecords, assertions = _weaponFindabilityAssertions });
            }
            foreach (int tick in WhiteoutLoadArea(origin, null)) yield return tick;
            // Scene census is its own evidence gate. Walking, pickup, revisit
            // and disk reload use separately leased native save fixtures; no
            // prologue story interruption can turn this census into route proof.
        }
    }
}
