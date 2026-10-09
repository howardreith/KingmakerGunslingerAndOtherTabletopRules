using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Items;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.CraftMagicItemsCompatibility;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private RuntimeTestResult RunWeaponRecoveryFixtures()
        {
            RequireWeaponQualificationGame();
            if (!_request.ExitAfterCompletion || _workingSaveSmoke == null ||
                !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Exact read-only working seed and automatic exit required.");
            _workingSaveSmoke.RestrictToTransactionOwnedWrites();
            var game = Game.Instance; var player = game.Player;
            var owner = player.MainCharacter.Value.Descriptor;
            if (owner.Get<UnitPartCampaignWeaponRecovery>() != null || player.IsInCombat || game.Vendor.IsTrading)
                throw new InvalidOperationException("Recovery fixtures require a clean ledger and idle disposable seed.");
            var choices = CampaignWeaponRegistry.Read().Where(value => value.Relocated && value.ModuleEnabled).ToArray();
            var checks = new List<RuntimeTestAssertion>(); var observations = new List<object>();
            var collections = player.AllCharacters.Select(value => value.Inventory)
                .Concat(new[] { player.Inventory, player.SharedStash }).Distinct().ToArray();
            var before = collections.ToDictionary(value => value, value => value.Items.ToArray());
            var counts = before.Values.SelectMany(value => value).Distinct().ToDictionary(value => value, value => value.Count);
            long money = player.Money;
            var fixtureItems = new List<ItemEntity>();
            var visited = new Dictionary<BlueprintArea, List<string>>(player.VisitedAreasData);
            Action<string, bool, string> check = (name, pass, observed) => {
                checks.Add(Assertion("weapon-recovery-" + name, "bounded native recovery fixture", observed, pass,
                    "production Inspect/Grant, native ItemsCollection, main-character UnitPart; no save or container refill"));
                if (!pass) throw new InvalidOperationException("Recovery fixture failed: " + name + ";" + observed);
            };
            Func<bool> inventoriesSame = () => collections.All(value => before[value].SequenceEqual(value.Items)) &&
                counts.All(value => value.Key.Count == value.Value) && player.Money == money;
            var equipmentSlots = player.AllCharacters.Where(value => value.Body != null)
                .SelectMany(value => value.Body.AllSlots).OfType<Kingmaker.Items.Slots.HandSlot>()
                .Where(value => value.IsPrimaryHand && !value.HasItem && !value.PairSlot.HasItem &&
                    value.IsPossibleInsertItems()).ToArray();
            if (equipmentSlots.Length == 0)
                throw new InvalidOperationException("An empty ordinary weapon set is required for bounded equipped-copy fixtures.");
            Action<CampaignWeaponPlacement, BlueprintItemWeapon, string> equippedRefusal = (weapon, blueprint, label) => {
                var slot = equipmentSlots[0]; var item = new ItemEntityWeapon(blueprint); fixtureItems.Add(item);
                try
                {
                    slot.InsertItem(item);
                    var snapshot = slot.Owner.Inventory.Items.ToArray();
                    var inspected = CampaignWeaponRecovery.Inspect(weapon.Key);
                    string refused = CampaignWeaponRecovery.Grant(weapon.Key, player.GameId, true);
                    check(label + "-equipped-refusal", ReferenceEquals(slot.MaybeItem, item) &&
                        inspected.OwnedCopies.Any(value => value.StartsWith("equipment ", StringComparison.Ordinal) && value.Contains(blueprint.AssetGuid)) &&
                        refused.Contains("Owned canonical or supported upgraded copy found") &&
                        snapshot.SequenceEqual(slot.Owner.Inventory.Items), inspected.Describe());
                }
                finally
                {
                    if (item.HoldingSlot != null) item.HoldingSlot.RemoveItem();
                    if (item.Collection != null) item.Collection.Remove(item); item.Dispose(); fixtureItems.Remove(item);
                }
                check(label + "-equipment-restoration", inventoriesSame() && !slot.HasItem && !slot.PairSlot.HasItem,
                    "empty native hand set and every original inventory restored");
            };
            try
            {
                foreach (var weapon in choices)
                {
                    var target = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Loot.BlueprintLoot>(
                        BlueprintBootstrap.Library, weapon.Location.TargetGuid, weapon.Location.TargetName);
                    player.VisitedAreasData.Remove(target.Area);
                    var missingProgress = CampaignWeaponRecovery.Inspect(weapon.Key);
                    check(weapon.Key + "-progress-refusal", !missingProgress.CanRecover && inventoriesSame(), missingProgress.Describe());
                    equippedRefusal(weapon, weapon.Item, weapon.Key + "-canonical");
                    foreach (var collection in collections)
                    {
                        var canonical = new ItemEntityWeapon(weapon.Item); fixtureItems.Add(canonical);
                        collection.Add(canonical);
                        var inspection = CampaignWeaponRecovery.Inspect(weapon.Key);
                        var existing = collection.Items.ToArray();
                        string refused = CampaignWeaponRecovery.Grant(weapon.Key, player.GameId, true);
                        check(weapon.Key + "-owned-" + Array.IndexOf(collections, collection), !inspection.CanRecover &&
                            inspection.OwnedCopies.Count > 0 && refused.StartsWith("Recovery refused.", StringComparison.Ordinal) &&
                            existing.SequenceEqual(collection.Items) && canonical.Count == 1,
                            inspection.Describe() + ";invocation=" + refused);
                        collection.Remove(canonical); canonical.Dispose(); fixtureItems.Remove(canonical);
                        check(weapon.Key + "-owned-restoration-" + Array.IndexOf(collections, collection), inventoriesSame(), "native exact fixture item removed; foreign items and counts retained");
                    }
                    // Progress is an explicitly prepared fixture state, never
                    // offered as evidence of organic story/map accessibility.
                    player.VisitedAreasData[target.Area] = new List<string>();
                    var sceneLoot = UnityEngine.Resources.FindObjectsOfTypeAll<Kingmaker.View.MapObjects.LootComponent>()
                        .First(value => value != null && value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded &&
                            value.MapObject != null && value.MapObject.Data != null && value.MapObject.Data.IsInGame);
                    var nativeTreasure = sceneLoot.Loot.Items.ToDictionary(value => value, value => value.Count);
                    var pending = new ItemEntityWeapon(weapon.Item); fixtureItems.Add(pending); sceneLoot.Loot.Add(pending);
                    try
                    {
                        var inspection = CampaignWeaponRecovery.Inspect(weapon.Key);
                        string refused = CampaignWeaponRecovery.Grant(weapon.Key, player.GameId, true);
                        check(weapon.Key + "-loaded-historical-container-refusal", !inspection.CanRecover &&
                            refused.Contains("remains available in a known container") &&
                            refused.Contains(sceneLoot.MapObject.Data.UniqueId) && pending.Collection == sceneLoot.Loot &&
                            pending.Count == 1 && inventoriesSame() &&
                            nativeTreasure.All(pair => pair.Key.Collection == sceneLoot.Loot && pair.Key.Count == pair.Value), inspection.Describe());
                    }
                    finally { sceneLoot.Loot.Remove(pending); pending.Dispose(); fixtureItems.Remove(pending); }
                    var eligible = CampaignWeaponRecovery.Inspect(weapon.Key);
                    check(weapon.Key + "-missing-admission", eligible.CanRecover && inventoriesSame(), eligible.Describe());
                    check(weapon.Key + "-ack-refusal", CampaignWeaponRecovery.Grant(weapon.Key, player.GameId, false)
                        .StartsWith("Recovery refused:", StringComparison.Ordinal) && inventoriesSame() &&
                        (owner.Get<UnitPartCampaignWeaponRecovery>() == null || !owner.Get<UnitPartCampaignWeaponRecovery>().HasRecord(weapon.Item.AssetGuid)), "No acknowledgement produces no grant or record");
                    check(weapon.Key + "-campaign-refusal", CampaignWeaponRecovery.Grant(weapon.Key, "different-campaign", true)
                        .StartsWith("Recovery refused:", StringComparison.Ordinal) && inventoriesSame(), "Inspected campaign identity is mandatory");
                    var initial = player.Inventory.Items.ToArray();
                    string grant = CampaignWeaponRecovery.Grant(weapon.Key, player.GameId, true);
                    var additions = player.Inventory.Items.Except(initial).ToArray(); fixtureItems.AddRange(additions);
                    var ledger = owner.Get<UnitPartCampaignWeaponRecovery>();
                    check(weapon.Key + "-one-grant", additions.Length == 1 && additions[0].Blueprint == weapon.Item &&
                        additions[0].Count == 1 && ledger != null && ledger.HasRecord(weapon.Item.AssetGuid), grant);
                    player.Inventory.Remove(additions[0]); additions[0].Dispose(); fixtureItems.Remove(additions[0]);
                    string repeat = CampaignWeaponRecovery.Grant(weapon.Key, player.GameId, true);
                    check(weapon.Key + "-repeat-absent-refusal", repeat.Contains("already recorded") && inventoriesSame(), repeat);
                    observations.Add(new { key = weapon.Key, item = weapon.Item.AssetGuid, target = weapon.Location.TargetGuid,
                        canonicalRefusal = "PASS", missingProgress = "PASS", explicitSingleGrant = "PASS",
                        repeatAfterRemoval = "PASS", history = CampaignWeaponRecoveryPolicy.HistoricalUncertainty,
                        organicProgress = "UNVERIFIED; destination visit prepared in request-local fixture", saveReload = "UNVERIFIED" });
                }
                // The supported integration constructs a genuine upgraded item,
                // not a fabricated GUID or a domain-only stand-in.
                var upgradeBase = choices.Single(value => value.Key == "eastern:WinterReed");
                var enhancement = BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(
                    BlueprintBootstrap.Library, Firearms.MidgameFirearmCatalog.EnhancementThreeGuid, "native +3 enhancement");
                BlueprintItemWeapon upgraded = CraftMagicItemsReflectionBridge.BuildQualificationClone(upgradeBase.Item, enhancement);
                check("supported-upgrade-identity", upgraded != null && upgraded != upgradeBase.Item &&
                    upgraded.Enchantments.Contains(enhancement) && !upgradeBase.Item.Enchantments.Contains(enhancement) &&
                    CampaignWeaponRecoveryPolicy.IsOwnedIdentity(upgradeBase.Item.AssetGuid, upgraded.AssetGuid),
                    upgraded == null ? "UNVERIFIED: supported Craft Magic Items clone unavailable" : upgraded.AssetGuid);
                equippedRefusal(upgradeBase, upgraded, "supported-upgrade");
                foreach (var collection in collections)
                {
                    var item = new ItemEntityWeapon(upgraded); fixtureItems.Add(item); collection.Add(item);
                    var snapshot = collection.Items.ToArray();
                    var inspected = CampaignWeaponRecovery.Inspect(upgradeBase.Key);
                    string refused = CampaignWeaponRecovery.Grant(upgradeBase.Key, player.GameId, true);
                    check("upgraded-owned-" + Array.IndexOf(collections, collection), inspected.OwnedCopies.Any(value => value.Contains(upgraded.AssetGuid)) &&
                        refused.Contains("Owned canonical or supported upgraded copy found") && snapshot.SequenceEqual(collection.Items), inspected.Describe());
                    collection.Remove(item); item.Dispose(); fixtureItems.Remove(item);
                }
                check("foreign-inventory-retained", inventoriesSame(), "Every native item, inventory row, quantity and money is unchanged");
                check("ledger-selected-set", owner.Get<UnitPartCampaignWeaponRecovery>().Records.Length == choices.Length &&
                    owner.Get<UnitPartCampaignWeaponRecovery>().Records.All(value => value.Status == "Granted" && value.GrantedCount == 1),
                    "records=" + owner.Get<UnitPartCampaignWeaponRecovery>().Records.Length);
            }
            finally
            {
                foreach (var item in fixtureItems)
                { if (item.HoldingSlot != null) item.HoldingSlot.RemoveItem(); if (item.Collection != null) item.Collection.Remove(item); item.Dispose(); }
                owner.Remove<UnitPartCampaignWeaponRecovery>();
                player.VisitedAreasData.Clear(); foreach (var entry in visited) player.VisitedAreasData.Add(entry.Key, entry.Value);
                check("request-local-fixture-restored", inventoriesSame() && owner.Get<UnitPartCampaignWeaponRecovery>() == null &&
                    player.VisitedAreasData.Count == visited.Count && !_workingSaveSmoke.WriteObserved,
                    "no native save writes; original visits, inventory and absence of ledger restored");
                WriteTeleportationForensicJson(System.IO.Path.Combine(_request.EvidenceDirectory, "weapon-recovery-fixtures.json"),
                    new { runId = _request.RunId, observations, checks, saveReload = "UNVERIFIED; separate disk persistence gate required" });
            }
            return CreateResult(checks.All(value => value.Status == "PASS") ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, checks, null);
        }
    }
}
