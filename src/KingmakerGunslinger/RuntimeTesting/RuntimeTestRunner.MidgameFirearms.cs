using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Spells.ProtectionFromAlignment;
using Newtonsoft.Json;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Save-free fixture only. Never loads a save or changes an existing campaign.
        private RuntimeTestResult RunDisposableMidgameFirearms()
        {
            var assertions = new List<RuntimeTestAssertion>();
            Player priorPlayer = Game.Instance.Player;
            Player fixturePlayer = null;
            UnityEngine.Random.State priorRandom = UnityEngine.Random.state;
            try
            {
                if (Game.Instance.CurrentlyLoadedArea != null ||
                    priorPlayer.MainCharacter.Value != null || Game.Instance.Vendor.IsTrading)
                    throw new InvalidOperationException("Disposable mid-game fixture requires an unloaded main-menu state with no active trade.");
                fixturePlayer = new Player();
                Game.Instance.State.PlayerState = fixturePlayer;
                fixturePlayer.CreateInventory();
                fixturePlayer.GainMoney(1000000L);
                if (!_context.FeatureModules.Active.Gunslinger)
                    throw new InvalidOperationException("The focused firearm fixture requires Gunslinger enabled.");
                MagicFirearmBlueprints.Validate(BlueprintBootstrap.MagicFirearms);
                MidgameProtectionTooltips(assertions);
                MidgamePublicationContracts(assertions, true);
                foreach (SkeletalSalesmanStockTarget spec in SkeletalSalesmanStockCatalog.Targets)
                    MidgameMerchantFixture(spec, assertions);
                // This exact main-menu fixture has no BattleLogView. Capture
                // only the final UI sink, retaining native publication and
                // verifying every attempted annotation reached the sink.
                long logAttempts = Diagnostics.NativeCombatLog.Attempts;
                long logFaults = Diagnostics.NativeCombatLog.Faults;
                using (var log = new ElementalBreezeKissedScenario.FirearmLogCapture())
                {
                    MidgameWeaponRules(assertions);
                    assertions.Add(Assertion("midgame-native-log-publication", "every native message captured; no new publication fault",
                        "messages=" + log.Messages + ";attempts=" + (Diagnostics.NativeCombatLog.Attempts - logAttempts),
                        log.Messages > 0 && log.Messages == Diagnostics.NativeCombatLog.Attempts - logAttempts &&
                        Diagnostics.NativeCombatLog.Faults == logFaults,
                        "request-local absent UI sink only; unchanged native message validation and attack rules"));
                }
            }
            catch (Exception exception)
            {
                assertions.Add(Assertion("midgame-fixture-exception", "no exception",
                    exception.ToString(), false, "guarded request-local fixture"));
            }
            finally
            {
                try
                {
                    if (fixturePlayer != null)
                    {
                        if (Game.Instance.Vendor.IsTrading) Game.Instance.Vendor.EndTraiding();
                        foreach (ItemEntityWeapon item in fixturePlayer.Inventory.Items.OfType<ItemEntityWeapon>())
                            FirearmRuntimeState.Service.Forget(item);
                        fixturePlayer.Dispose();
                    }
                }
                catch (Exception cleanupException)
                {
                    assertions.Add(Assertion("midgame-cleanup-exception", "clean disposal",
                        cleanupException.ToString(), false, "request-local cleanup"));
                }
                finally { if (fixturePlayer != null) Game.Instance.State.PlayerState = priorPlayer; }
                UnityEngine.Random.state = priorRandom;
            }
            assertions.Add(Assertion("midgame-player-restored", "original PlayerState reference; no area/save loaded",
                "playerRestored=" + ReferenceEquals(Game.Instance.Player, priorPlayer) +
                ";area=" + (Game.Instance.CurrentlyLoadedArea == null ? "none" : Game.Instance.CurrentlyLoadedArea.name),
                ReferenceEquals(Game.Instance.Player, priorPlayer) && Game.Instance.CurrentlyLoadedArea == null,
                "request-local player/inventory finally rollback"));
            return CreateResult(assertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private void MidgamePublicationContracts(List<RuntimeTestAssertion> assertions, bool enabled)
        {
            BlueprintItem[] items = MidgameFirearmCatalog.Entries.Select(spec =>
                (BlueprintItem)BlueprintBootstrap.MagicFirearms.Require(spec.Symbol).Item).ToArray();
            BlueprintSharedVendorTable[] tables = SkeletalSalesmanStockCatalog.Targets.Select(spec =>
                BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(BlueprintBootstrap.Library,
                    spec.Guid, spec.Name)).ToArray();
            foreach (BlueprintSharedVendorTable table in tables)
                foreach (BlueprintItem item in items)
                {
                    LootItemsPackFixed[] rows = table.ComponentsArray.OfType<LootItemsPackFixed>()
                        .Where(row => ReferenceEquals(CapitalVendorBlueprints.ReadItem(row), item)).ToArray();
                    assertions.Add(Assertion("salesman-publication-" + table.name + "-" + item.name,
                        enabled ? "one fixed copy" : "absent", "rows=" + rows.Length,
                        enabled ? rows.Length == 1 && CapitalVendorBlueprints.ReadCount(rows[0]) == 1 : rows.Length == 0,
                        table.AssetGuid));
                }
            bool otherVendors = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintSharedVendorTable>()
                .Where(table => !tables.Contains(table)).Any(table => (table.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<LootItemsPackFixed>().Any(row => items.Contains(CapitalVendorBlueprints.ReadItem(row))));
            assertions.Add(Assertion("salesman-exclusive-acquisition", "no other vendor or fixed-loot publication",
                "otherVendors=" + otherVendors + ";fixedLoot=" + RareFirearmCampaignLootBlueprints.TargetSpecs.Length,
                !otherVendors && RareFirearmCampaignLootBlueprints.TargetSpecs.Length == 5 &&
                !RareFirearmCampaignLootBlueprints.TargetSpecs.Any(spec => MidgameFirearmCatalog.Entries.Any(item => item.Symbol == spec.ItemSymbol)) &&
                BlueprintBootstrap.MagicFirearms.GenericEntries.Length == 3 &&
                BlueprintBootstrap.MagicFirearms.NamedEntries.Length == 7,
                "all live vendor blueprints; explicit fixed-loot catalog; crafting roles"));
            if (enabled)
            {
                BlueprintComponent[][] before = tables.Select(table => table.ComponentsArray).ToArray();
                SkeletalSalesmanPublication repeated = SkeletalSalesmanBlueprints.Publish(
                    BlueprintBootstrap.Library, BlueprintBootstrap.MagicFirearms, _context.Logger);
                repeated.Validate();
                assertions.Add(Assertion("salesman-repeat-publication", "four arrays unchanged by repeated initialization",
                    "tables=" + repeated.Count, repeated.Count == 4 && tables.Select((table, i) =>
                        ReferenceEquals(table.ComponentsArray, before[i])).All(value => value), "exact array references"));
            }
        }

        private void MidgameProtectionTooltips(List<RuntimeTestAssertion> assertions)
        {
            ProtectionFromAlignmentPublicationObservation observation = ProtectionFromAlignmentPublication.Observe(BlueprintBootstrap.Library);
            bool enabled = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity;
            assertions.Add(Assertion("protection-description-publication", "15 exact owned descriptions when enabled; none when disabled",
                "resolved=" + observation.ResolvedDescriptions + ";published=" + observation.PublishedDescriptions +
                ";invalid=" + observation.InvalidDescriptions, observation.ResolvedDescriptions == 15 &&
                observation.InvalidDescriptions == 0 && observation.PublishedDescriptions == (enabled ? 15 : 0),
                "live ability/selector/buff localization keys and text"));
            // Every installed usable item whose Ability is one of the exact protected spells.
            var ids = new HashSet<string>(new[] { "433b1faf4d02cc34abb0ade5ceda47c4", "eee384c813b6d74498d1b9cc720d61f4",
                "2ac7637daeb2aa143a3bae860095b63e", "c3aafbbb6e8fc754fb8c82ede3280051", "1eaf1020e82028d4db55e6e464269e00",
                "2cadf6c6350e4684baa109d067277a45", "93f391b0c5a99e04e83bbfbe3bb6db64", "5bfd4cce1557d5744914f8f6d85959a4",
                "8b8ccc9763e3cc74bbf5acc9c98557b9", "0ec75ec95d9e39d47a23610123ba1bad" }, StringComparer.Ordinal);
            int surfaces = 0;
            foreach (BlueprintItemEquipmentUsable item in BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintItemEquipmentUsable>()
                .Where(item => item.Ability != null && ids.Contains(item.Ability.AssetGuid)))
            {
                ItemEntity entity = item.CreateEntity();
                try
                {
                    entity.Identify();
                    TooltipData tooltip = UIUtilityItem.FillTooltipData(entity, new TooltipData());
                    string description = tooltip.Texts[TooltipElement.LongDescription];
                    assertions.Add(Assertion("protection-item-tooltip-" + item.AssetGuid,
                        "native long tooltip inherits exact current spell text", item.name + ";" + description,
                        description == item.Ability.Description && (!enabled || description.Contains(ProtectionFromAlignmentDescriptions.ExistingControlLimitation)),
                        "UIUtilityItem.FillTooltipData; native usable-item ability reference"));
                    surfaces++;
                }
                finally { entity.Dispose(); }
            }
            assertions.Add(Assertion("protection-usable-surfaces-found", "at least eight installed individual/communal scroll surfaces",
                surfaces.ToString(), surfaces >= 8, "exact ability identity scan"));
        }

        private void MidgameMerchantFixture(SkeletalSalesmanStockTarget spec, List<RuntimeTestAssertion> assertions)
        {
            BlueprintSharedVendorTable table = BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(
                BlueprintBootstrap.Library, spec.Guid, spec.Name);
            BlueprintComponent[] sourceComponents = table.ComponentsArray;
            BlueprintUnit merchantBlueprint = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                SkeletalSalesmanStockCatalog.UnitGuid, "RE_Trader");
            UnitEntityData merchant = new Kingmaker.UI.LevelUp.ChargenUnit(merchantBlueprint).Unit;
            var parts = new List<UnitPartVendor>();
            BlueprintSharedVendorTable priorStock = null;
            try
            {
                UnitPartVendor stock = merchant.Ensure<UnitPartVendor>();
                stock.AddLoot(table);
                VendorLogic vendor = Game.Instance.Vendor;
                vendor.BeginTrading(merchant);
                if (!vendor.IsTrading) throw new InvalidOperationException("Native salesman trade did not begin.");
                var purchased = new List<ItemEntityWeapon>();
                foreach (MidgameFirearmSpec design in MidgameFirearmCatalog.Entries)
                {
                    BlueprintItemWeapon blueprint = BlueprintBootstrap.MagicFirearms.Require(design.Symbol).Item;
                    ItemEntityWeapon item = stock.Inventory.Items.OfType<ItemEntityWeapon>().Single(value => value.Blueprint == blueprint);
                    long price = vendor.GetItemBuyPrice(item);
                    TooltipData tooltip = UIUtilityItem.FillTooltipData(item, new TooltipData());
                    assertions.Add(Assertion("salesman-price-" + spec.Name + "-" + design.DisplayName,
                        "native merchant price and item tooltip agree; unchanged native multiplier",
                        "merchant=" + merchant.CharacterName + ";guid=" + merchantBlueprint.AssetGuid +
                        ";base=" + blueprint.Cost + ";buy=" + price + ";tooltip=" + tooltip.Texts[TooltipElement.Price] +
                        ";modifier=" + stock.PriceModifier + ";sell=" + vendor.GetItemSellPrice(item),
                        blueprint.Cost == design.Cost && price == (long)((float)item.Cost * stock.PriceModifier) &&
                        tooltip.Texts[TooltipElement.Price] == price.ToString(), "native VendorLogic.GetItemBuyPrice and UIUtilityItem.FillTooltipData"));
                    foreach (ItemsFilter.FilterType filter in new[] { ItemsFilter.FilterType.NoFilter, ItemsFilter.FilterType.Weapon })
                    {
                        List<ItemEntity> visible = ItemsFilter.ItemSorter(ItemsFilter.SorterType.TypeUp,
                            stock.Inventory.Items.ToList(), filter).Where(row => ItemsFilter.ShouldShowItem(row, filter)).ToList();
                        int index = visible.IndexOf(item);
                        string neighbors = string.Join(" | ", visible.Skip(Math.Max(0, index - 2)).Take(5)
                            .Select(row => row.Name + " [" + row.Blueprint.AssetGuid + "; " + vendor.GetItemBuyPrice(row) + " gp]").ToArray());
                        assertions.Add(Assertion("salesman-native-order-" + spec.Name + "-" + design.DisplayName + "-" + filter,
                            "one naturally sorted weapon row; native localized type/name order",
                            "index=" + index + ";neighbors=" + neighbors,
                            index >= 0 && visible.Count(row => row.Blueprint == blueprint) == 1,
                            "live ItemEntity stock through the desktop shop's native ItemsFilter.ItemSorter and weapon filter; full window separate"));
                    }
                    long money = Game.Instance.Player.Money;
                    vendor.AddForBuy(item, 1);
                    vendor.Deal();
                    assertions.Add(Assertion("salesman-purchase-" + spec.Name + "-" + design.DisplayName,
                        "one normal deal debits native price and transfers item", "debit=" + (money - Game.Instance.Player.Money),
                        Game.Instance.Player.Money == money - price && item.Collection == Game.Instance.Player.Inventory &&
                        stock.Inventory.Count(blueprint) == 0, "VendorLogic.AddForBuy/Deal"));
                    purchased.Add(item);
                }
                vendor.EndTraiding(); vendor.BeginTrading(merchant);
                assertions.Add(Assertion("salesman-reopen-" + spec.Name, "purchased items remain absent",
                    "remaining=" + purchased.Sum(item => stock.Inventory.Count(item.Blueprint)),
                    purchased.All(item => stock.Inventory.Count(item.Blueprint) == 0), "native EndTraiding/BeginTrading"));
                vendor.EndTraiding();

                // Model stock generated by 0.0.115 without modifying a live source table.
                // The clone retains the native GUID so the game's reference converter resolves
                // its saved Loot reference to today's published table during deserialization.
                priorStock = UnityEngine.Object.Instantiate(table);
                priorStock.ComponentsArray = sourceComponents.Where(component => !purchased.Any(item =>
                    ReferenceEquals(CapitalVendorBlueprints.ReadItem(component as LootItemsPackFixed), item.Blueprint))).ToArray();
                if (priorStock.AssetGuid != table.AssetGuid) throw new InvalidOperationException("Native fixture clone lost its table reference identity.");
                var old = new UnitPartVendor(); parts.Add(old); old.AddLoot(priorStock);
                BlueprintItemWeapon buyback = BlueprintBootstrap.ProductionFirearms.Musket.Item;
                old.Inventory.Add(buyback, 1);
                string originalStock = MidgameInventoryDigest(old.Inventory);
                UnitPartVendor migrated = MidgameRoundTrip(old); parts.Add(migrated);
                assertions.Add(Assertion("salesman-existing-stock-" + spec.Name,
                    "native PostLoad adds exactly one of each; all prior stock and buyback retained",
                    "before=" + originalStock + ";after=" + MidgameInventoryDigest(migrated.Inventory),
                    purchased.All(item => migrated.Inventory.Count(item.Blueprint) == 1) &&
                    MidgameInventoryDigest(migrated.Inventory, purchased.Select(item => (BlueprintItem)item.Blueprint).ToArray()) == originalStock,
                    "native default JSON reference conversion and UnitPartVendor.PostLoad; in-memory previous-version fixture"));
                foreach (ItemEntityWeapon item in purchased) migrated.Inventory.Remove(item.Blueprint, 1);
                UnitPartVendor afterPurchase = MidgameRoundTrip(migrated); parts.Add(afterPurchase);
                assertions.Add(Assertion("salesman-purchased-persistence-" + spec.Name,
                    "KnownItems prevents restocking purchases; buyback retained",
                    "stock=" + MidgameInventoryDigest(afterPurchase.Inventory),
                    purchased.All(item => afterPurchase.Inventory.Count(item.Blueprint) == 0) &&
                    MidgameInventoryDigest(afterPurchase.Inventory) == originalStock,
                    "second native JSON/PostLoad round trip after extraction"));
                // Purchased item identities and static enchantments use the same native serializer.
                foreach (ItemEntityWeapon item in purchased)
                {
                    Game.Instance.Player.Inventory.Extract(item);
                    var inventory = new ItemsCollection(); inventory.Add(item); inventory.PreSave();
                    ItemsCollection restored = MidgameNativeJsonRoundTrip(inventory);
                    try
                    {
                        restored.PostLoad();
                        ItemEntityWeapon copy = restored.Items.OfType<ItemEntityWeapon>().Single();
                        assertions.Add(Assertion("purchased-item-roundtrip-" + spec.Name + "-" + item.Blueprint.name,
                            "same native blueprint and both static properties",
                            copy.Blueprint.AssetGuid + ";enchantments=" + copy.Enchantments.Count,
                            copy.Blueprint == item.Blueprint && copy.Enchantments.Count == 2,
                            "native ItemsCollection PreSave/default JSON/PostLoad; no save archive"));
                    }
                    finally { restored.Dispose(); inventory.Dispose(); }
                }
            }
            finally
            {
                if (Game.Instance.Vendor.IsTrading) Game.Instance.Vendor.EndTraiding();
                foreach (UnitPartVendor part in parts) part.Dispose();
                merchant.Dispose();
                if (priorStock != null) UnityEngine.Object.DestroyImmediate(priorStock);
                assertions.Add(Assertion("salesman-source-preserved-" + spec.Name, "source component array unchanged",
                    table.AssetGuid, ReferenceEquals(table.ComponentsArray, sourceComponents), "exact source reference; detached fixtures disposed"));
            }
        }

        private static UnitPartVendor MidgameRoundTrip(UnitPartVendor part)
        {
            part.PreSave();
            UnitPartVendor restored = MidgameNativeJsonRoundTrip(part);
            restored.PostLoad();
            return restored;
        }

        private static T MidgameNativeJsonRoundTrip<T>(T value)
        {
            // JsonConvert.CreateDefault(settings) appends the global converters twice.
            // Use the game's configured serializer directly, once in each direction.
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                JsonSerializer.Create(DefaultJsonSettings.DefaultSettings).Serialize(writer, value);
                using (var reader = new JsonTextReader(new StringReader(writer.ToString())))
                    return JsonSerializer.Create(DefaultJsonSettings.DefaultSettings).Deserialize<T>(reader);
            }
        }

        private static string MidgameInventoryDigest(ItemsCollection inventory, params BlueprintItem[] excluded)
        {
            return string.Join("|", inventory.Items.Where(item => !excluded.Contains(item.Blueprint))
                .GroupBy(item => item.Blueprint.AssetGuid).OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => group.Key + ":" + group.Sum(item => item.Count)).ToArray());
        }

        private void MidgameWeaponRules(List<RuntimeTestAssertion> assertions)
        {
            UnitEntityData attacker = new Kingmaker.UI.LevelUp.ChargenUnit(BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            UnitEntityData target = new Kingmaker.UI.LevelUp.ChargenUnit(BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            var weapons = new List<ItemEntityWeapon>();
            try
            {
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 20;
                attacker.Descriptor.Stats.Dexterity.BaseValue = 30;
                target.Descriptor.State.Immortality.Retain();
                foreach (MidgameFirearmSpec design in MidgameFirearmCatalog.Entries)
                {
                    MagicFirearmBlueprintEntry entry = BlueprintBootstrap.MagicFirearms.Require(design.Symbol);
                    var weapon = new ItemEntityWeapon(entry.Item); weapons.Add(weapon);
                    var control = new ItemEntityWeapon(entry.Family.Item); weapons.Add(control);
                    RuleAttackRoll mundaneAttack = TriggerReliableMatrixAttack(attacker, target, control, 10, FirearmCondition.Normal);
                    RuleCalculateWeaponStats mundaneStats = Rulebook.Trigger(new RuleCalculateWeaponStats(attacker, control, null));
                    RuleAttackRoll namedAttack = TriggerReliableMatrixAttack(attacker, target, weapon, 10, FirearmCondition.Normal);
                    RuleCalculateWeaponStats namedStats = Rulebook.Trigger(new RuleCalculateWeaponStats(attacker, weapon, null));
                    FirearmState fired = FirearmRuntimeState.Service.GetOrCreate(weapon).Repository.State;
                    assertions.Add(Assertion("midgame-native-plus3-" + design.DisplayName,
                        "exact +3 attack/damage; one round discharged; canonical family",
                        "attackDelta=" + (namedAttack.AttackBonus - mundaneAttack.AttackBonus) +
                        ";damageDelta=" + (namedStats.BonusDamage - mundaneStats.BonusDamage) +
                        ";enhancement=" + namedStats.Enhancement + ";rounds=" + fired.LoadedRounds,
                        namedAttack.AttackBonus - mundaneAttack.AttackBonus == 3 &&
                        namedStats.BonusDamage - mundaneStats.BonusDamage == 3 && namedStats.Enhancement == 3 &&
                        fired.LoadedRounds == 0 && entry.Item.Type == entry.Family.WeaponType,
                        "native RuleAttackRoll/RuleCalculateWeaponStats and item-owned ammunition state"));
                    Game.Instance.Player.Inventory.Add(BlueprintBootstrap.BasicAmmunition.BlackPowder, 1);
                    Game.Instance.Player.Inventory.Add(BlueprintBootstrap.BasicAmmunition.LeadBall, 1);
                    int powder = Game.Instance.Player.Inventory.Count(BlueprintBootstrap.BasicAmmunition.BlackPowder);
                    int ball = Game.Instance.Player.Inventory.Count(BlueprintBootstrap.BasicAmmunition.LeadBall);
                    FirearmReloadResult reload = ReloadTestMusketRuntime.Execute(attacker.Descriptor,
                        entry.Item, BlueprintBootstrap.BasicAmmunition.BlackPowder, BlueprintBootstrap.BasicAmmunition.LeadBall);
                    assertions.Add(Assertion("midgame-normal-reload-" + design.DisplayName, "one loose powder/ball consumed; one loaded round",
                        "success=" + reload.Succeeded, reload.Succeeded &&
                        Game.Instance.Player.Inventory.Count(BlueprintBootstrap.BasicAmmunition.BlackPowder) == powder - 1 &&
                        Game.Instance.Player.Inventory.Count(BlueprintBootstrap.BasicAmmunition.LeadBall) == ball - 1 &&
                        FirearmRuntimeState.Service.GetOrCreate(weapon).Repository.State.LoadedRounds == 1,
                        "production exact-equipped reload planner and inventory transaction"));
                    int reduction = Enchantments.FirearmMisfireReductionResolver.Resolve(weapon);
                    bool seeking = Enchantments.SeekingExactItemResolver.IsAuthorized(weapon);
                    assertions.Add(Assertion("midgame-property-isolation-" + design.DisplayName, "only the stated special property on the exact item",
                        "reliable=" + reduction + ";seeking=" + seeking, reduction == (design.Reliable ? 1 : 0) &&
                        seeking == design.Seeking && Enchantments.FirearmMisfireReductionResolver.Resolve(control) == 0 &&
                        !Enchantments.SeekingExactItemResolver.IsAuthorized(control), "exact weapon enchantment resolvers"));
                    RuleAttackRoll natural = TriggerReliableMatrixAttack(attacker, target, weapon, 1, FirearmCondition.Normal);
                    assertions.Add(Assertion("midgame-natural-one-" + design.DisplayName, "natural 1 misses and breaks both designs",
                        "hit=" + natural.IsHit + ";condition=" + FirearmRuntimeState.Service.GetOrCreate(weapon).Repository.State.Condition,
                        !natural.IsHit && FirearmRuntimeState.Service.GetOrCreate(weapon).Repository.State.Condition == FirearmCondition.Broken,
                        "native attack/discharge/misfire runtime; Reliable musket threshold 2-1=1"));
                    if (design.Reliable)
                    {
                        TriggerReliableMatrixAttack(attacker, target, weapon, 2, FirearmCondition.Normal);
                        FirearmCondition reliableCondition = FirearmRuntimeState.Service.GetOrCreate(weapon).Repository.State.Condition;
                        TriggerReliableMatrixAttack(attacker, target, control, 2, FirearmCondition.Normal);
                        assertions.Add(Assertion("roadwarden-reliable-roll-two", "Roadwarden normal; mundane musket broken",
                            reliableCondition.ToString(), reliableCondition == FirearmCondition.Normal &&
                            FirearmRuntimeState.Service.GetOrCreate(control).Repository.State.Condition == FirearmCondition.Broken,
                            "same native natural 2, canonical musket misfire value"));
                    }
                    else
                    {
                        BlueprintBuff blur = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                            "dd3ad347240624d46a11a092b4dd4674", "Blur concealment");
                        var fact = target.Descriptor.AddFact(blur);
                        try
                        {
                            attacker.Body.PrimaryHand.RemoveItem(false); attacker.Body.PrimaryHand.InsertItem(control);
                            FirearmRuntimeState.Service.Set(control, new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                                FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal));
                            Enchantments.SeekingConcealmentRuntime.QueueForcedRoll(control, 1);
                            UnityEngine.Random.InitState(FindNativeD100ThenD20Seed(19));
                            RuleAttackRoll blocked = Rulebook.Trigger(new RuleAttackRoll(attacker, target, control, -100));
                            Enchantments.SeekingConcealmentRuntime.CancelForcedRoll();
                            attacker.Body.PrimaryHand.RemoveItem(false); attacker.Body.PrimaryHand.InsertItem(weapon);
                            FirearmRuntimeState.Service.Set(weapon, new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                                FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal));
                            Enchantments.SeekingConcealmentRuntime.QueueForcedRoll(weapon, 1);
                            UnityEngine.Random.InitState(FindNativeD100ThenD20Seed(19));
                            RuleAttackRoll bypassed = Rulebook.Trigger(new RuleAttackRoll(attacker, target, weapon, -100));
                            assertions.Add(Assertion("dead-reckoning-native-seeking", "control misses concealment; Dead Reckoning hits same concealment",
                                "control=" + blocked.Result + ";seeking=" + bypassed.Result,
                                !blocked.IsHit && bypassed.IsHit && blocked.ConcealmentCheck != null && bypassed.ConcealmentCheck != null &&
                                blocked.ConcealmentCheck.Concealment == bypassed.ConcealmentCheck.Concealment,
                                "native RuleConcealmentCheck and exact-item Seeking hook"));
                        }
                        finally { Enchantments.SeekingConcealmentRuntime.CancelForcedRoll(); target.Descriptor.RemoveFact(fact); }
                    }
                }
            }
            finally
            {
                Enchantments.SeekingConcealmentRuntime.CancelForcedRoll();
                attacker.Body.PrimaryHand.RemoveItem(false);
                foreach (ItemEntityWeapon weapon in weapons) { FirearmRuntimeState.Service.Forget(weapon); weapon.Dispose(); }
                target.Descriptor.State.Immortality.Release(); target.Dispose(); attacker.Dispose();
            }
        }
    }
}
