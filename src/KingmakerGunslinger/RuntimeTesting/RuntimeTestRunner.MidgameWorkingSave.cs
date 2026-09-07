using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UI.Tooltip;
using Kingmaker.UI.Vendor;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private readonly List<RuntimeTestAssertion> _midgameWorkingAssertions = new List<RuntimeTestAssertion>();
        private VendorUI _midgameShop;
        private UnitEntityData _midgameMerchant;
        private int _midgameShopVariant;
        private int _midgameShopStep;
        private long _midgameMoney;
        private bool _midgameFunded;
        private bool _midgameSaveStarted;
        private bool _midgameSaveCompleted;
        private readonly Stopwatch _midgameStageElapsed = new Stopwatch();

        private bool IsMidgameWorkingScenario()
        { return RuntimeTestScenarioCatalog.IsMidgameWorkingScenario(_request.Scenario); }

        private void PollWorkingMidgameFirearms()
        {
            try
            {
                if (_workingSaveSmoke == null || !_workingSaveSmoke.Complete ||
                    Game.Instance.CurrentlyLoadedArea == null ||
                    Game.Instance.Player.Party.Count != WorkingSaveSmokeScenario.ExpectedPartyCount)
                    throw new InvalidOperationException("Exact working-save load/fingerprint is required before the shop fixture.");
                if (_workingSaveSmoke.WriteObserved)
                    throw new InvalidOperationException("Unexpected or unarmed save boundary in the mid-game fixture.");
                if (_midgameSaveStarted)
                {
                    if (_midgameSaveCompleted) { CompleteMidgameWorking(); return; }
                    if (_midgameStageElapsed.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                        throw new TimeoutException("Exact working-save write did not complete.");
                    return;
                }
                BlueprintItemWeapon[] items = MidgameFirearmCatalog.Entries.Select(design =>
                    BlueprintBootstrap.MagicFirearms.Require(design.Symbol).Item).ToArray();
                bool prepare = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMidgamePrepare;
                bool absent = _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveMidgameVerifyAbsent;
                if (_midgameShopStep == 0)
                {
                    int[] counts = items.Select(item => Game.Instance.Player.Inventory.Count(item)).ToArray();
                    if (prepare || absent)
                    {
                        if (counts.Any(count => count != 0))
                            throw new InvalidOperationException("Prepare/absence check requires both task items absent; pre-existing items will not be removed.");
                    }
                    else
                    {
                        if (counts.Any(count => count != 1))
                            throw new InvalidOperationException("Fresh-load verification requires exactly the two saved purchased items.");
                        for (int i = 0; i < items.Length; i++)
                        {
                            ItemEntityWeapon item = Game.Instance.Player.Inventory.Items.OfType<ItemEntityWeapon>().Single(value => value.Blueprint == items[i]);
                            FirearmState state = FirearmRuntimeState.Service.GetOrCreate(item).Repository.State;
                            bool valid = item.VendorBlueprint != null && item.VendorBlueprint.AssetGuid == SkeletalSalesmanStockCatalog.UnitGuid &&
                                item.Enchantments.Count(enchantment => entryBlueprint(item, enchantment.Blueprint)) == 2 &&
                                (i == 0 ? state.LoadedRounds == 1 && state.Condition == FirearmCondition.Normal &&
                                    state.LoadedAmmunition == ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition :
                                    state.LoadedRounds == 0 && state.Condition == FirearmCondition.Broken);
                            AddMidgameWorking("purchased-save-load-" + items[i].name,
                                "native salesman provenance, exact static enchantments, saved ammunition/condition",
                                "vendor=" + (item.VendorBlueprint == null ? "none" : item.VendorBlueprint.AssetGuid) +
                                ";guid=" + item.Blueprint.AssetGuid + ";state=" + state, valid,
                                "fresh Steam launch and guarded working-save load");
                        }
                        if (_midgameWorkingAssertions.Any(value => value.Status != "PASS"))
                            throw new InvalidOperationException("Purchased item persistence failed; cleanup refused before a valid observation.");
                        foreach (BlueprintItemWeapon blueprint in items)
                        {
                            ItemEntityWeapon item = Game.Instance.Player.Inventory.Items.OfType<ItemEntityWeapon>().Single(value => value.Blueprint == blueprint);
                            FirearmRuntimeState.Service.Forget(item);
                            Game.Instance.Player.Inventory.Remove(item);
                        }
                        AddMidgameWorking("purchased-fixture-cleanup", "both exact fixture items removed",
                            string.Join(",", items.Select(item => Game.Instance.Player.Inventory.Count(item).ToString()).ToArray()),
                            items.All(item => Game.Instance.Player.Inventory.Count(item) == 0), "exact task-owned items only");
                        StartMidgameWorkingSave(); return;
                    }
                    if (absent)
                    {
                        AddMidgameWorking("purchased-fixture-absent", "no task items after fresh cleanup load", "0,0", true,
                            "native shared inventory after guarded working load");
                        CompleteMidgameWorking(); return;
                    }
                    if (!_context.FeatureModules.Active.Gunslinger || Game.Instance.Vendor.IsTrading)
                        throw new InvalidOperationException("Shop prepare requires Gunslinger enabled and no active merchant session.");
                    VendorUI[] shops = UnityEngine.Resources.FindObjectsOfTypeAll<VendorUI>().Where(ui =>
                        ui.gameObject.scene.IsValid() && ui.gameObject.scene.isLoaded).ToArray();
                    if (shops.Length != 1) throw new InvalidOperationException("Normal desktop VendorUI is missing or ambiguous: " + shops.Length);
                    _midgameShop = shops[0];
                    _midgameMoney = Game.Instance.Player.Money;
                    Game.Instance.Player.GainMoney(100000L); _midgameFunded = true;
                    BeginMidgameShopVariant(); return;
                }
                if (_midgameShopStep == 1)
                {
                    if (!Game.Instance.Vendor.IsTrading || Game.Instance.Vendor.VendorUnit != _midgameMerchant ||
                        !_midgameShop.IsShow ||
                        !ReferenceEquals(_midgameShop.Store.Collection, _midgameMerchant.VendorInventory))
                    {
                        if (_midgameStageElapsed.Elapsed.TotalSeconds > 30)
                            throw new TimeoutException("Normal native trade window did not bind to the disposable salesman.");
                        return;
                    }
                    string variant = SkeletalSalesmanStockCatalog.Targets[_midgameShopVariant].Name;
                    AddMidgameWorking("normal-shop-default-" + variant, "native type/name order and all-items filter",
                        _midgameShop.Store.Filter.CurrentSorter + ";" + _midgameShop.Store.Filter.CurrentFilter,
                        _midgameShop.Store.Filter.CurrentSorter == ItemsFilter.SorterType.TypeUp &&
                        _midgameShop.Store.Filter.CurrentFilter == ItemsFilter.FilterType.NoFilter,
                        "visible VendorUI after native HandleTradeStarted/OnShow/Fill");
                    CaptureMidgameVisibleNeighbors(variant + "-all", items);
                    _midgameShop.Store.Filter.ChangeFilter(ItemsFilter.FilterType.Weapon, true, true);
                    _midgameShop.Store.Refresh();
                    CaptureMidgameVisibleNeighbors(variant + "-weapons", items);
                    _midgameShop.Store.Filter.SetSorter(ItemsFilter.SorterType.PriceDown);
                    _midgameShop.Store.Filter.ApplySortAndFilters(); _midgameShop.Store.Refresh();
                    SkeletalSalesmanBlueprints.Publish(BlueprintBootstrap.Library, BlueprintBootstrap.MagicFirearms, _context.Logger).Validate();
                    ItemEntity[] descending = _midgameShop.Store.VirtualSlots.Where(slot => slot.IsVisible && slot.Item != null).Select(slot => slot.Item).ToArray();
                    AddMidgameWorking("normal-shop-player-sort-" + variant, "chosen descending-price sort retained",
                        _midgameShop.Store.Filter.CurrentSorter.ToString(),
                        _midgameShop.Store.Filter.CurrentSorter == ItemsFilter.SorterType.PriceDown &&
                        descending.Select(item => item.Cost).SequenceEqual(descending.Select(item => item.Cost).OrderByDescending(cost => cost)),
                        "actual Store.VirtualSlots after native filter/sort and repeat publication");
                    if (_midgameShopVariant == 0)
                    {
                        foreach (BlueprintItemWeapon blueprint in items)
                        {
                            ItemEntityWeapon item = _midgameMerchant.VendorInventory.Items.OfType<ItemEntityWeapon>().Single(value => value.Blueprint == blueprint);
                            long beforeMoney = Game.Instance.Player.Money;
                            long price = Game.Instance.Vendor.GetItemBuyPrice(item);
                            TooltipData tooltip = UIUtilityItem.FillTooltipData(item, new TooltipData());
                            Game.Instance.Vendor.AddForBuy(item, 1); _midgameShop.UpdateDeal(); _midgameShop.Deal();
                            AddMidgameWorking("normal-shop-buy-" + blueprint.name, "displayed price debited and one item acquired",
                                "base=" + blueprint.Cost + ";buy=" + price + ";displayed=" + tooltip.Texts[TooltipElement.Price] +
                                ";debit=" + (beforeMoney - Game.Instance.Player.Money),
                                beforeMoney - Game.Instance.Player.Money == price && tooltip.Texts[TooltipElement.Price] == price.ToString() &&
                                item.Collection == Game.Instance.Player.Inventory && _midgameMerchant.VendorInventory.Count(blueprint) == 0,
                                "normal VendorUI.Deal, native price tooltip, and exact ownership transfer");
                        }
                    }
                    _midgameShop.HandleTradeExit(); _midgameShopStep = 2; _midgameStageElapsed.Restart(); return;
                }
                if (Game.Instance.Vendor.IsTrading || _midgameShop.IsShow)
                {
                    if (_midgameStageElapsed.Elapsed.TotalSeconds > 30) throw new TimeoutException("Native shop did not close.");
                    return;
                }
                _midgameMerchant.Dispose(); _midgameMerchant = null;
                _midgameShopVariant++;
                if (_midgameShopVariant < SkeletalSalesmanStockCatalog.Targets.Length) { BeginMidgameShopVariant(); return; }
                RestoreMidgameFunding();
                if (_midgameWorkingAssertions.Any(value => value.Status != "PASS"))
                    throw new InvalidOperationException("Shop assertions failed; working save write refused.");
                for (int i = 0; i < items.Length; i++)
                {
                    ItemEntityWeapon item = Game.Instance.Player.Inventory.Items.OfType<ItemEntityWeapon>().Single(value => value.Blueprint == items[i]);
                    FirearmRuntimeState.Service.Set(item, i == 0 ? new FirearmState(FirearmState.CurrentSchemaVersion,
                        1, ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition, FirearmCondition.Normal) :
                        new FirearmState(FirearmState.CurrentSchemaVersion, 0, null, FirearmCondition.Broken));
                }
                StartMidgameWorkingSave();
            }
            catch (Exception exception)
            {
                AddMidgameWorking("working-midgame-exception", "no exception", exception.ToString(), false, "guarded working fixture");
                CompleteMidgameWorking();
            }
        }

        private void BeginMidgameShopVariant()
        {
            SkeletalSalesmanStockTarget spec = SkeletalSalesmanStockCatalog.Targets[_midgameShopVariant];
            BlueprintUnit unit = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                SkeletalSalesmanStockCatalog.UnitGuid, SkeletalSalesmanStockCatalog.UnitName);
            _midgameMerchant = new Kingmaker.UI.LevelUp.ChargenUnit(unit).Unit;
            _midgameMerchant.Ensure<UnitPartVendor>().AddLoot(BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(
                BlueprintBootstrap.Library, spec.Guid, spec.Name));
            _midgameShop.HandleTradeStarted(_midgameMerchant);
            _midgameShopStep = 1; _midgameStageElapsed.Restart();
        }

        private void CaptureMidgameVisibleNeighbors(string context, BlueprintItemWeapon[] items)
        {
            ItemEntity[] rows = _midgameShop.Store.VirtualSlots.Where(slot => slot.IsVisible && slot.Item != null).Select(slot => slot.Item).ToArray();
            foreach (BlueprintItemWeapon blueprint in items)
            {
                int index = Array.FindIndex(rows, row => row.Blueprint == blueprint);
                string neighbors = string.Join(" | ", rows.Skip(Math.Max(0, index - 2)).Take(5).Select(row =>
                    row.Name + " [" + row.Blueprint.AssetGuid + "; " + Game.Instance.Vendor.GetItemBuyPrice(row) + " gp]").ToArray());
                AddMidgameWorking("visible-shop-neighbors-" + context + "-" + blueprint.name,
                    "one naturally integrated weapon row in the visible normal shop", "index=" + index + ";" + neighbors,
                    index >= 0 && rows.Count(row => row.Blueprint == blueprint) == 1,
                    "VendorUI.Store.VirtualSlots IsVisible and Item, after native Fill/filter; no source-array inference");
            }
        }

        private void RestoreMidgameFunding()
        {
            if (!_midgameFunded) return;
            long delta = Game.Instance.Player.Money - _midgameMoney;
            if (delta > 0) Game.Instance.Player.SpendMoney(delta);
            else if (delta < 0) Game.Instance.Player.GainMoney(-delta);
            _midgameFunded = false;
        }

        private void StartMidgameWorkingSave()
        {
            _workingSaveSmoke.ArmExactWorkingSaveWrite();
            MethodInfo save = typeof(Game).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Single(method => method.Name == "SaveGame" && method.ReturnType == typeof(void) &&
                    method.GetParameters().Length == 2 && method.GetParameters()[0].ParameterType.FullName ==
                    "Kingmaker.EntitySystem.Persistence.SaveInfo" && method.GetParameters()[1].ParameterType == typeof(Action));
            _midgameSaveStarted = true; _midgameStageElapsed.Restart();
            save.Invoke(Game.Instance, new object[] { _workingSaveSmoke.WorkingDescriptor,
                new Action(() => _midgameSaveCompleted = true) });
        }

        private void AddMidgameWorking(string name, string expected, string observed, bool pass, string evidence)
        { _midgameWorkingAssertions.Add(Assertion(name, expected, observed, pass, evidence)); }

        private void CompleteMidgameWorking()
        {
            try
            {
                if (_midgameShop != null && Game.Instance.Vendor.IsTrading) _midgameShop.Show(false);
                if (_midgameMerchant != null) { _midgameMerchant.Dispose(); _midgameMerchant = null; }
                RestoreMidgameFunding();
            }
            catch (Exception exception) { AddMidgameWorking("working-midgame-cleanup", "clean disposal", exception.ToString(), false, "fixture finally cleanup"); }
            WorkingSaveSmokeEvidence evidence = _workingSaveSmoke.Stop();
            AddMidgameWorking("exact-working-load", "one correlated working descriptor; baseline distinct",
                "working=" + evidence.WorkingMatchCount + ";baseline=" + evidence.BaselineMatchCount,
                evidence.WorkingMatchCount == 1 && evidence.BaselineMatchCount == 1 && evidence.DescriptorReferenceCorrelated,
                "canonical guarded working-save load");
            AddMidgameWorking("exact-working-write", _midgameSaveStarted ? "one authorized completed working save" : "no writes",
                "count=" + evidence.ExpectedWorkingSaveRoutineCount + ";completed=" + _midgameSaveCompleted,
                !evidence.SaveWritingApiObserved && (_midgameSaveStarted ? _midgameSaveCompleted &&
                    evidence.ExpectedWorkingSaveRoutineCount == 1 && evidence.ExpectedWorkingStashedAreaCount >= 1 :
                    evidence.ExpectedWorkingSaveRoutineCount == 0), "existing exact descriptor save sentinel");
            RuntimeTestResult result = CreateResult(_midgameWorkingAssertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, _midgameWorkingAssertions, null);
            result.WorkingSaveSmoke = evidence; Complete(result);
        }
    }
}
