using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.CraftMagicItemsCompatibility;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class CraftMagicItemsAmmunitionUiObserver
    {
        internal static Session Begin(ModContext context,
            RuntimeTestRequest request)
        {
            return new Session(context, request);
        }

        internal sealed class Session
        {
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly DateTime _started;
            private readonly Stopwatch _elapsed;
            private readonly List<string> _guiFailures = new List<string>();
            private readonly List<string> _phaseEvidence = new List<string>();
            private readonly CraftMagicItemsGraphSnapshot _graphBefore;
            private readonly CraftMagicItemsAmmunitionUiSnapshot _uiBefore;
            private readonly long _moneyBefore;
            private readonly Dictionary<ItemEntity, int> _inventoryBefore;
            private readonly List<CraftMagicItemsAmmunitionCraftObservation>
                _craftObservations =
                    new List<CraftMagicItemsAmmunitionCraftObservation>();
            private UnitEntityData _firstCrafter;
            private UnitEntityData _secondCrafter;
            private UnitEntityData _reloadCrafter;
            private CraftMagicItemsAmmunitionUiRuntimeAdapter _adapter;
            private CraftMagicItemsAmmunitionUiProbeHost _host;
            private int _phase = -1;
            private int _pauseUpdates;
            private int _phaseRouteStart;
            private int _phaseHostStart;
            private bool _unrelatedStatePreserved = true;
            private bool _invalidCrafterSafe;
            private bool _insufficientFundsSafe;
            private bool _looseReloadConsumed;
            private bool _paperReloadConsumed;
            private int _craftRecipeIndex;
            private int _craftInventoryBefore;
            private long _craftMoneyBefore;
            private bool _cleanupExact;
            private string _cleanupEvidence = string.Empty;
            private RuntimeTestResult _result;

            internal Session(ModContext context, RuntimeTestRequest request)
            {
                _context = context ?? throw new ArgumentNullException("context");
                _request = request ?? throw new ArgumentNullException("request");
                _started = DateTime.UtcNow;
                _elapsed = Stopwatch.StartNew();
                _graphBefore = CraftMagicItemsReflectionBridge.Snapshot;
                _uiBefore = CraftMagicItemsReflectionBridge
                    .AmmunitionUiSnapshot;
                if (!_context.IsReady ||
                    !CraftMagicItemsReflectionBridge.IsFinalized)
                    throw new InvalidOperationException(
                        "The real CMI graph is not finalized for UI observation.");
                _moneyBefore = Game.Instance.Player.Money;
                _inventoryBefore = SnapshotInventory();
                _firstCrafter = CreateCrafter();
                _adapter = CraftMagicItemsAmmunitionUiRuntimeAdapter.Begin(
                    _firstCrafter, request.RunId);
                CraftMagicItemsAmmunitionUiProbeHost.TotalInvokeCount = 0;
                var hostObject = new GameObject(
                    "KMG_CMI_Ammunition_UI_Observer");
                hostObject.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(hostObject);
                _host = hostObject.AddComponent<
                    CraftMagicItemsAmmunitionUiProbeHost>();
                _host.Adapter = _adapter;
                _host.Failures = _guiFailures;
                Application.logMessageReceived += ObserveLog;
            }

            internal bool Complete { get { return _result != null; } }
            internal RuntimeTestResult Result { get { return _result; } }

            internal void Poll()
            {
                if (Complete) return;
                try
                {
                    if (_elapsed.Elapsed.TotalSeconds > 45)
                        throw new TimeoutException(
                            "The actual CMI IMGUI route did not complete in 45 seconds.");
                    if (_guiFailures.Count != 0)
                        throw new InvalidOperationException(
                            "The actual CMI IMGUI route emitted a GUI failure: " +
                            _guiFailures[0]);
                    if (_phase < 0)
                    {
                        StartPhase(0);
                        return;
                    }
                    if (_pauseUpdates > 0)
                    {
                        _pauseUpdates--;
                        if (_pauseUpdates == 0)
                        {
                            QueuePhaseConfiguration(_phase);
                        }
                        return;
                    }
                    if (!PhaseObserved(_phase)) return;
                    _unrelatedStatePreserved &=
                        _adapter.UnrelatedStatePreserved;
                    RecordPhase(_phase);
                    if (_phase >= 9)
                        CaptureCraft(_phase == 12 || _phase == 13,
                            _phase == 13);
                    if (_phase == 7) _invalidCrafterSafe = true;
                    if (_phase == 8)
                        _insufficientFundsSafe =
                            Game.Instance.Player.Money == 0 &&
                            SameInventory(_inventoryBefore,
                                SnapshotInventory());
                    if (_phase == 12)
                        VerifyReloadConsumption();
                    if (_phase == 13)
                    {
                        Finish(null);
                        return;
                    }
                    StartPhase(_phase + 1);
                }
                catch (Exception exception)
                {
                    Finish(exception);
                }
            }

            private void StartPhase(int phase)
            {
                _phase = phase;
                _host.RenderEnabled = false;
                _phaseRouteStart = CraftMagicItemsReflectionBridge
                    .AmmunitionUiSnapshot.RouteObservations.Length;
                _phaseHostStart = _host.InvokeCount;
                if (phase == 5 || phase == 6)
                {
                    // Models closing/reopening the renderer host and leaving /
                    // returning to a mod tab without changing the selected data.
                    _pauseUpdates = 2;
                    return;
                }
                QueuePhaseConfiguration(phase);
            }

            private void QueuePhaseConfiguration(int phase)
            {
                _host.ConfigureAtNextLayout = () => ConfigurePhase(phase);
                _host.RenderEnabled = true;
            }

            private void ConfigurePhase(int phase)
            {
                switch (phase)
                {
                    case 0:
                    case 4:
                        _adapter.SelectOrdinary();
                        break;
                    case 1:
                        _adapter.SelectAmmunition(0, true);
                        break;
                    case 2:
                        if (_secondCrafter == null)
                        {
                            _secondCrafter = CreateCrafter();
                            _adapter.SetCrafter(_secondCrafter);
                        }
                        _adapter.SelectAmmunition(1, false);
                        break;
                    case 3:
                    case 5:
                    case 6:
                        _adapter.SelectAmmunition(2, false);
                        break;
                    case 7:
                        _adapter.SetNoCrafter();
                        _adapter.SelectAmmunition(0, false);
                        break;
                    case 8:
                        _adapter.SetCrafter(_firstCrafter);
                        if (Game.Instance.Player.Money > 0 &&
                            !Game.Instance.Player.SpendMoney(
                                Game.Instance.Player.Money))
                            throw new InvalidOperationException(
                                "The insufficient-funds UI fixture could not isolate money.");
                        _adapter.SelectAmmunition(0, false);
                        break;
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                        BeginCraft(phase == 12 ? 2 : phase - 9,
                            phase != 12);
                        break;
                    case 13:
                        BeginCraft(0, false);
                        break;
                    default:
                        throw new InvalidOperationException(
                            "Unknown CMI ammunition UI observer phase.");
                }
            }

            private bool PhaseObserved(int phase)
            {
                CraftMagicItemsAmmunitionUiSnapshot snapshot =
                    CraftMagicItemsReflectionBridge.AmmunitionUiSnapshot;
                string expected = phase == 0 || phase == 4 ?
                    "ordinary-cmi:" :
                    CraftMagicItemsReflectionBridge.AmmunitionIdentity + ":";
                string[] entries = snapshot.RouteObservations.Skip(
                        _phaseRouteStart).Where(value => value.StartsWith(
                            expected, StringComparison.Ordinal)).ToArray();
                bool eventPair = _host.InvokeCount - _phaseHostStart >= 2 &&
                    entries.Any(value => RouteObservedOn(value, "Layout")) &&
                    entries.Any(value => RouteObservedOn(value, "Repaint"));
                return eventPair && (phase < 9 ||
                    _adapter.CraftClickTriggered);
            }

            private static bool RouteObservedOn(string route,
                string eventType)
            {
                int separator = string.IsNullOrEmpty(route) ? -1 :
                    route.LastIndexOf(':');
                return separator >= 0 &&
                    CraftMagicItemsMundaneUiEventPolicy.Is(
                        route.Substring(separator + 1), eventType);
            }

            private void BeginCraft(int recipeIndex, bool takesNoTime)
            {
                CraftMagicItemsAmmunitionRegistration registration =
                    CraftMagicItemsReflectionBridge.Catalog
                        .Ammunition[recipeIndex];
                int remainingCost = CraftMagicItemsReflectionBridge.Catalog
                    .Ammunition.Sum(value => value.Plan.GoldCost(1f)) +
                    CraftMagicItemsReflectionBridge.Catalog.Ammunition[2]
                        .Plan.GoldCost(1f);
                if (Game.Instance.Player.Money < remainingCost)
                    Game.Instance.Player.GainMoney(remainingCost -
                        Game.Instance.Player.Money);
                _craftRecipeIndex = recipeIndex;
                _craftInventoryBefore = Game.Instance.Player.Inventory.Count(
                    registration.Item);
                _craftMoneyBefore = Game.Instance.Player.Money;
                _adapter.ArmCraft(recipeIndex, takesNoTime);
            }

            private void CaptureCraft(bool timed, bool cancel)
            {
                CraftMagicItemsAmmunitionCraftObservation observation =
                    _adapter.ObserveCraft(_craftRecipeIndex,
                        _craftInventoryBefore, _craftMoneyBefore, timed);
                if (timed)
                {
                    _host.RenderEnabled = false;
                    if (cancel) _adapter.CancelTimedProject(observation);
                    else _adapter.CompleteTimedProject(_firstCrafter,
                        observation);
                }
                _craftObservations.Add(observation);
                _phaseEvidence.Add("craft=" + observation.ItemGuid +
                    ";timed=" + timed + ";button=" +
                    observation.ButtonTriggered + ";inventory=" +
                    observation.InventoryBefore + "->" +
                    observation.InventoryAfter + ";money=" +
                    observation.MoneyBefore + "->" +
                    observation.MoneyAfter + ";target=" +
                    observation.ProjectTarget + ";projectGold=" +
                    observation.ProjectGold + ";projectResult=" +
                    observation.ProjectResultGuid + ":" +
                    observation.ProjectResultCount + ";completed=" +
                    observation.ProjectCompleted + ";cancelled=" +
                    observation.ProjectCancelled);
            }

            private void VerifyReloadConsumption()
            {
                BasicAmmunitionBlueprintSet ammunition =
                    BlueprintBootstrap.BasicAmmunition;
                _reloadCrafter = CreateCrafter();
                _reloadCrafter.Descriptor.AddFact(
                    BlueprintBootstrap.FirearmProficiency);
                ActivatableAbility paperMode = _reloadCrafter.Descriptor
                    .ActivatableAbilities.Enumerable.Single(value =>
                        ReferenceEquals(value.Blueprint,
                            BlueprintBootstrap.PaperCartridgeMode.Ability));
                ItemEntityWeapon weapon = null;
                try
                {
                    if (paperMode.IsOn) paperMode.IsOn = false;
                    weapon = new ItemEntityWeapon(
                        BlueprintBootstrap.ProductionFirearms.Pistol.Item);
                    _reloadCrafter.Body.PrimaryHand.InsertItem(weapon);
                    FirearmRuntimeState.Service.Set(weapon,
                        FirearmState.CreateEmpty());
                    int powderBefore = Game.Instance.Player.Inventory.Count(
                        ammunition.BlackPowder);
                    int ballBefore = Game.Instance.Player.Inventory.Count(
                        ammunition.LeadBall);
                    FirearmReloadResult loose = ReloadTestMusketRuntime.Execute(
                        _reloadCrafter.Descriptor, weapon.Blueprint,
                        ammunition.BlackPowder, ammunition.LeadBall);
                    _looseReloadConsumed = loose.Succeeded &&
                        Game.Instance.Player.Inventory.Count(
                            ammunition.BlackPowder) == powderBefore - 1 &&
                        Game.Instance.Player.Inventory.Count(
                            ammunition.LeadBall) == ballBefore - 1;

                    FirearmRuntimeState.Service.Forget(weapon);
                    _reloadCrafter.Body.PrimaryHand.RemoveItem(false);
                    Game.Instance.Player.Inventory.Remove(weapon);
                    weapon.Dispose();
                    weapon = new ItemEntityWeapon(
                        BlueprintBootstrap.ProductionFirearms.Pistol.Item);
                    _reloadCrafter.Body.PrimaryHand.InsertItem(weapon);
                    FirearmRuntimeState.Service.Set(weapon,
                        FirearmState.CreateEmpty());
                    paperMode.IsOn = true;
                    int paperBefore = Game.Instance.Player.Inventory.Count(
                        ammunition.PaperCartridge);
                    powderBefore = Game.Instance.Player.Inventory.Count(
                        ammunition.BlackPowder);
                    ballBefore = Game.Instance.Player.Inventory.Count(
                        ammunition.LeadBall);
                    FirearmReloadResult paper = ReloadTestMusketRuntime.Execute(
                        _reloadCrafter.Descriptor, weapon.Blueprint,
                        ammunition.BlackPowder, ammunition.LeadBall);
                    FirearmState state = FirearmRuntimeState.Service
                        .GetOrCreate(weapon).Repository.State;
                    _paperReloadConsumed = paper.Succeeded &&
                        Game.Instance.Player.Inventory.Count(
                            ammunition.PaperCartridge) == paperBefore - 1 &&
                        Game.Instance.Player.Inventory.Count(
                            ammunition.BlackPowder) == powderBefore &&
                        Game.Instance.Player.Inventory.Count(
                            ammunition.LeadBall) == ballBefore &&
                        state.LoadedAmmunition ==
                            ReloadAmmunitionProfileCatalog.PaperCartridge
                                .LoadedAmmunition;
                }
                finally
                {
                    if (paperMode != null && paperMode.IsOn)
                        paperMode.IsOn = false;
                    if (weapon != null)
                    {
                        FirearmRuntimeState.Service.Forget(weapon);
                        if (_reloadCrafter.Body.PrimaryHand.MaybeItem != null)
                            _reloadCrafter.Body.PrimaryHand.RemoveItem(false);
                        Game.Instance.Player.Inventory.Remove(weapon);
                        weapon.Dispose();
                    }
                }
            }

            private void RecordPhase(int phase)
            {
                CraftMagicItemsAmmunitionUiSnapshot snapshot =
                    CraftMagicItemsReflectionBridge.AmmunitionUiSnapshot;
                _phaseEvidence.Add("phase=" + phase + ";hostCalls=" +
                    (_host.InvokeCount - _phaseHostStart) + ";routes=" +
                    string.Join(",", snapshot.RouteObservations.Skip(
                        _phaseRouteStart).ToArray()));
            }

            private void ObserveLog(string condition, string stackTrace,
                LogType type)
            {
                string value = (condition ?? string.Empty) + "\n" +
                    (stackTrace ?? string.Empty);
                if (value.IndexOf("GUILayout", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("LayoutGroup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("GUIClip", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("SelectionGrid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("Error rendering GUI", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("bridge.incompatible", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("TargetInvocationException", StringComparison.OrdinalIgnoreCase) >= 0)
                    _guiFailures.Add(type + ":" + value.Replace("\r", " ")
                        .Replace("\n", " | "));
            }

            private void Finish(Exception exception)
            {
                Exception cleanupFailure = null;
                try
                {
                    Application.logMessageReceived -= ObserveLog;
                    if (_host != null)
                    {
                        _host.RenderEnabled = false;
                        UnityEngine.Object.Destroy(_host.gameObject);
                        _host = null;
                    }
                    if (_adapter != null)
                    {
                        _adapter.Dispose();
                        _adapter = null;
                    }
                    RestorePlayerState();
                    if (_firstCrafter != null)
                    {
                        _firstCrafter.Dispose();
                        _firstCrafter = null;
                    }
                    if (_secondCrafter != null)
                    {
                        _secondCrafter.Dispose();
                        _secondCrafter = null;
                    }
                    if (_reloadCrafter != null)
                    {
                        _reloadCrafter.Dispose();
                        _reloadCrafter = null;
                    }
                    Dictionary<ItemEntity, int> inventoryAfter =
                        SnapshotInventory();
                    _cleanupExact = Game.Instance.Player.Money == _moneyBefore &&
                        SameInventory(_inventoryBefore, inventoryAfter);
                    _cleanupEvidence = DescribeCleanup(_inventoryBefore,
                        inventoryAfter, _moneyBefore,
                        Game.Instance.Player.Money);
                }
                catch (Exception failure)
                {
                    cleanupFailure = failure;
                }
                if (exception == null) exception = cleanupFailure;
                else if (cleanupFailure != null)
                    exception = new AggregateException(exception,
                        cleanupFailure);
                _result = BuildResult(exception);
            }

            private RuntimeTestResult BuildResult(Exception exception)
            {
                CraftMagicItemsGraphSnapshot graphAfter =
                    CraftMagicItemsReflectionBridge.Snapshot;
                CraftMagicItemsAmmunitionUiSnapshot uiAfter =
                    CraftMagicItemsReflectionBridge.AmmunitionUiSnapshot;
                int ordinaryDelta = uiAfter.OrdinaryRouteCount -
                    _uiBefore.OrdinaryRouteCount;
                int bypassDelta = uiAfter.OrdinaryBodyBypassCount -
                    _uiBefore.OrdinaryBodyBypassCount;
                int lowerDelta = uiAfter.LowerPanelRenderCount -
                    _uiBefore.LowerPanelRenderCount;
                string[] expectedGuids = CraftMagicItemsReflectionBridge.Catalog
                    .Ammunition.Select(value => value.Item.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                string[] observedGuids = uiAfter.SelectedRecipeGuids
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                bool graphSame = SameGraph(_graphBefore, graphAfter);
                bool noFailure = exception == null && _guiFailures.Count == 0 &&
                    uiAfter.UiFailureCount == _uiBefore.UiFailureCount &&
                    !uiAfter.DeferredFailurePending;
                bool routingExact = ordinaryDelta > 0 && bypassDelta > 0 &&
                    lowerDelta == bypassDelta &&
                    ordinaryDelta + bypassDelta ==
                        CraftMagicItemsAmmunitionUiProbeHost.TotalInvokeCount &&
                    uiAfter.EventTypes.Any(value =>
                        CraftMagicItemsMundaneUiEventPolicy.Is(value,
                            "Layout")) &&
                    uiAfter.EventTypes.Any(value =>
                        CraftMagicItemsMundaneUiEventPolicy.Is(value,
                            "Repaint"));
                var assertions = new List<RuntimeTestAssertion>();
                Add(assertions, "patched-inner-seam",
                    "one Harmony 2 transpiler at post-selected crafting data before NewItemBaseIDs",
                    "applications=" + CraftMagicItemsMundaneUiTranspiler
                        .ApplicationCount + ";seam=" + uiAfter.PatchSeam,
                    CraftMagicItemsMundaneUiTranspiler.ApplicationCount == 1 &&
                    uiAfter.PatchSeam.Contains("ordinary=IL_014d") &&
                    uiAfter.PatchSeam.Contains("new-item-bases=IL_0186") &&
                    uiAfter.PatchSeam.Contains("footer=IL_0774"),
                    "exact live CMI MethodBody probe plus applied transpiler diagnostics");
                Add(assertions, "outer-selector-owner",
                    "CraftMagicItems", uiAfter.OuterSelectorOwner,
                    uiAfter.OuterSelectorOwner == "CraftMagicItems",
                    "KMG helper is injected after CMI outer and subtype selection");
                Add(assertions, "event-stable-routing",
                    "ordinary and exact ammunition routes each observed under Layout and Repaint",
                    "ordinary=" + ordinaryDelta + ";bypass=" + bypassDelta +
                    ";lower=" + lowerDelta + ";events=" +
                    string.Join(",", uiAfter.EventTypes), routingExact,
                    "actual patched CMI renderer hosted by Unity OnGUI; one inner helper route per invocation");
                Add(assertions, "ammunition-recipes-selectable",
                    string.Join(",", expectedGuids),
                    string.Join(",", observedGuids),
                    expectedGuids.SequenceEqual(observedGuids),
                    "actual CMI Item selection and normal lower-panel control for all three recipes");
                Add(assertions, "unrelated-ui-state",
                    "upgradingBlueprint and selectedCustomName preserved",
                    "preserved=" + _unrelatedStatePreserved,
                    _unrelatedStatePreserved,
                    "exact sentinel references around ammunition selection");
                Add(assertions, "invalid-crafter-and-insufficient-funds",
                    "lower panel stays balanced with no crafter and with zero money",
                    "invalidCrafter=" + _invalidCrafterSafe +
                    ";insufficientFunds=" + _insufficientFundsSafe,
                    _invalidCrafterSafe && _insufficientFundsSafe,
                    "actual CMI lower control under Layout/Repaint without a craft click");
                CraftMagicItemsAmmunitionCraftObservation[] immediate =
                    _craftObservations.Where(value => !value.Timed).ToArray();
                bool immediateExact = immediate.Length == 3 && immediate.All(
                    value => value.ButtonTriggered && !value.ProjectCreated &&
                    value.InventoryAfter == value.InventoryBefore +
                        value.ExpectedCount && value.MoneyAfter ==
                        value.MoneyBefore - value.ExpectedGold);
                Add(assertions, "immediate-ammunition-crafting",
                    "each exact recipe spends 34/4/40 gp and creates 20 exact units immediately",
                    DescribeCrafts(immediate), immediateExact,
                    "actual CMI RenderRecipeBasedCraftItemControl button path with Crafting Takes No Time");
                CraftMagicItemsAmmunitionCraftObservation timed =
                    _craftObservations.SingleOrDefault(value => value.Timed &&
                        value.ProjectCompleted);
                bool timedExact = timed != null && timed.ButtonTriggered &&
                    timed.ProjectCreated && timed.ProjectTarget ==
                        timed.ExpectedProgress && timed.ProjectGold ==
                        timed.ExpectedGold && timed.ProjectResultCount ==
                        timed.ExpectedCount && timed.ProjectResultGuid ==
                        timed.ItemGuid && timed.ProjectCompleted &&
                    timed.InventoryAfter == timed.InventoryBefore +
                        timed.ExpectedCount && timed.MoneyAfter ==
                        timed.MoneyBefore - timed.ExpectedGold;
                Add(assertions, "timed-ammunition-project",
                    "Paper Cartridge project target=5, gold=40, result count=20, then normal completion",
                    timed == null ? "missing" : DescribeCrafts(new[] { timed }),
                    timedExact,
                    "actual CMI project constructor, timer component, WorkOnProjects, and CraftItem lifecycle");
                CraftMagicItemsAmmunitionCraftObservation cancelled =
                    _craftObservations.SingleOrDefault(value => value.Timed &&
                        value.ProjectCancelled);
                bool cancellationExact = cancelled != null &&
                    cancelled.ProjectTarget == 5 &&
                    cancelled.ProjectGold == cancelled.ExpectedGold &&
                    cancelled.MoneyAfter == cancelled.MoneyBefore &&
                    cancelled.InventoryAfter == cancelled.InventoryBefore &&
                    !cancelled.ProjectCompleted;
                Add(assertions, "timed-ammunition-cancellation",
                    "one target-5 project refunds its exact original GoldSpent and creates no result",
                    cancelled == null ? "missing" : DescribeCrafts(new[] {
                        cancelled }), cancellationExact,
                    "actual CMI CancelCraftingProject lifecycle after KMG target normalization");
                Add(assertions, "crafted-ammunition-consumption",
                    "KMG reload consumes exact crafted powder/ball and Paper mode consumes exact crafted cartridge",
                    "loose=" + _looseReloadConsumed + ";paper=" +
                    _paperReloadConsumed,
                    _looseReloadConsumed && _paperReloadConsumed,
                    "KMG authoritative reload planner/executor against CMI-created inventory identities");
                Add(assertions, "no-gui-failure-or-rollback",
                    "no GUI/TargetInvocation exception, deferred failure, or graph rollback",
                    "guiFailures=" + _guiFailures.Count + ";uiFailures=" +
                    (uiAfter.UiFailureCount - _uiBefore.UiFailureCount) +
                    ";rollback=" + (uiAfter.GraphRollbackCount -
                        _uiBefore.GraphRollbackCount),
                    noFailure && uiAfter.GraphRollbackCount ==
                        _uiBefore.GraphRollbackCount,
                    "Unity log callback, host exception capture, and bridge lifecycle counters");
                Add(assertions, "graph-unchanged",
                    "generation/counts unchanged and three item types remain unique",
                    DescribeGraph(graphAfter), graphSame &&
                        graphAfter.ItemTypes == 3,
                    "before/after exact CMI graph snapshot");
                Add(assertions, "request-local-cleanup",
                    "money and inventory unchanged; disposable crafters and patch removed",
                    "cleanupExact=" + _cleanupExact, _cleanupExact,
                    "request-local ChargenUnit entities, Harmony owner rollback, exact inventory snapshot");

                var diagnostics = new List<string>(_phaseEvidence)
                {
                    "patchedTarget=CraftMagicItems.Main.RenderCraftMundaneItemsSection",
                    "innerSeam=" + uiAfter.PatchSeam,
                    "selectedCategoryIdentity=" +
                        uiAfter.SelectedCategoryIdentity,
                    "lowerPanelRenderCount=" + lowerDelta,
                    "originalOrdinaryBodyBypassCount=" + bypassDelta,
                    "graphBefore=" + DescribeGraph(_graphBefore),
                    "graphAfter=" + DescribeGraph(graphAfter),
                    "cleanup=" + _cleanupEvidence
                };
                if (exception != null) diagnostics.Add("observerException=" +
                    exception);
                diagnostics.AddRange(_guiFailures.Select(value =>
                    "guiFailure=" + value));
                bool pass = assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass);
                RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                    _context.Assembly, _context.ModEntry.Info.Version);
                return new RuntimeTestResult
                {
                    SchemaVersion = 1,
                    RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    Status = pass ? RuntimeTestStatuses.Pass :
                        RuntimeTestStatuses.Fail,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = _context.Assembly.FullName + ";mvid=" +
                        _context.Assembly.ManifestModule.ModuleVersionId +
                        ";sha256=" + HashFile(_context.Assembly.Location) +
                        ";pid=" + Process.GetCurrentProcess().Id,
                    GitCommit = identity.GitCommit,
                    GameVersion = Application.version ?? string.Empty,
                    StartUtc = _started.ToString("o"),
                    Assertions = assertions,
                    Diagnostics = diagnostics,
                    Warnings = new List<string>
                    {
                        "This is mechanical actual-route evidence, not visual UMM acceptance.",
                        "A human must complete the fresh-process ammunition UI checklist."
                    },
                    ExceptionSummary = exception == null ? string.Empty :
                        exception.ToString(),
                    EvidenceFiles = new List<string>(),
                    AutomaticExitRequested = _request.ExitAfterCompletion,
                    EvidenceDirectory = _request.EvidenceDirectory
                };
            }

            private static UnitEntityData CreateCrafter()
            {
                UnitEntityData result = new ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                if (result == null || result.Descriptor == null)
                    throw new InvalidOperationException(
                        "A request-local CMI UI crafter could not be created.");
                ModifiableValue knowledge = result.Descriptor.Stats.GetStat(
                    StatType.SkillKnowledgeWorld);
                if (knowledge == null) throw new InvalidOperationException(
                    "The request-local CMI crafter has no Knowledge (World) stat.");
                knowledge.BaseValue = 100;
                return result;
            }

            private static Dictionary<ItemEntity, int> SnapshotInventory()
            {
                return ((IEnumerable)Game.Instance.Player.Inventory)
                    .Cast<ItemEntity>().ToDictionary(value => value,
                        value => value.Count);
            }

            private void RestorePlayerState()
            {
                BasicAmmunitionBlueprintSet ammunition =
                    BlueprintBootstrap.BasicAmmunition;
                foreach (BlueprintItem item in new[] { ammunition.BlackPowder,
                    ammunition.LeadBall, ammunition.PaperCartridge })
                {
                    ItemEntity[] current = ((IEnumerable)Game.Instance.Player
                            .Inventory).Cast<ItemEntity>().Where(value =>
                            ReferenceEquals(value.Blueprint, item)).ToArray();
                    foreach (ItemEntity value in current.Where(value =>
                        !_inventoryBefore.ContainsKey(value)).ToArray())
                        Game.Instance.Player.Inventory.Remove(value);
                    foreach (KeyValuePair<ItemEntity, int> value in
                        _inventoryBefore.Where(value => ReferenceEquals(
                            value.Key.Blueprint, item)))
                    {
                        if (!((IEnumerable)Game.Instance.Player.Inventory)
                                .Cast<ItemEntity>().Contains(value.Key))
                            throw new InvalidOperationException(
                                "CMI ammunition observation removed a pre-existing inventory entity.");
                        value.Key.SetCount(value.Value);
                    }
                }
                long moneyDelta = Game.Instance.Player.Money - _moneyBefore;
                if (moneyDelta < 0)
                    Game.Instance.Player.GainMoney(-moneyDelta);
                else if (moneyDelta > 0 &&
                    !Game.Instance.Player.SpendMoney(moneyDelta))
                    throw new InvalidOperationException(
                        "The guarded CMI observer could not restore player money.");
            }

            private static string DescribeCrafts(IEnumerable<
                CraftMagicItemsAmmunitionCraftObservation> observations)
            {
                return string.Join("|", observations.Select(value =>
                    value.ItemGuid + ":count=" + value.InventoryBefore +
                    "->" + value.InventoryAfter + ":money=" +
                    value.MoneyBefore + "->" + value.MoneyAfter +
                    ":expectedGold=" + value.ExpectedGold + ":target=" +
                    value.ProjectTarget + ":completed=" +
                    value.ProjectCompleted + ":cancelled=" +
                    value.ProjectCancelled).ToArray());
            }

            private static bool SameInventory(
                IDictionary<ItemEntity, int> left,
                IDictionary<ItemEntity, int> right)
            {
                return left.Count == right.Count && left.All(value =>
                    right.ContainsKey(value.Key) && right[value.Key] ==
                        value.Value);
            }

            private static string DescribeCleanup(
                IDictionary<ItemEntity, int> before,
                IDictionary<ItemEntity, int> after, long moneyBefore,
                long moneyAfter)
            {
                string[] removed = before.Where(value =>
                        !after.ContainsKey(value.Key))
                    .Select(value => DescribeItem(value.Key, value.Value))
                    .ToArray();
                string[] added = after.Where(value =>
                        !before.ContainsKey(value.Key))
                    .Select(value => DescribeItem(value.Key, value.Value))
                    .ToArray();
                string[] changed = before.Where(value =>
                        after.ContainsKey(value.Key) &&
                        after[value.Key] != value.Value)
                    .Select(value => DescribeItem(value.Key, value.Value) +
                        "->" + after[value.Key]).ToArray();
                return "money=" + moneyBefore + "->" + moneyAfter +
                    ";before=" + before.Count + ";after=" + after.Count +
                    ";removed=" + string.Join("|", removed) +
                    ";added=" + string.Join("|", added) +
                    ";changed=" + string.Join("|", changed);
            }

            private static string DescribeItem(ItemEntity item, int count)
            {
                return (item == null || item.Blueprint == null ? "<null>" :
                    item.Blueprint.AssetGuid) + ":" + count;
            }

            private static bool SameGraph(CraftMagicItemsGraphSnapshot left,
                CraftMagicItemsGraphSnapshot right)
            {
                return left != null && right != null &&
                    left.Generation == right.Generation &&
                    left.ItemTypes == right.ItemTypes &&
                    left.FirearmCreationBases ==
                        right.FirearmCreationBases &&
                    left.FirearmRecognitionIdentities ==
                        right.FirearmRecognitionIdentities &&
                    left.MartialBases == right.MartialBases &&
                    left.ExoticBases == right.ExoticBases &&
                    left.CustomFamilyMagicItemTypes ==
                        right.CustomFamilyMagicItemTypes &&
                    left.CustomFamilyRecognitionIdentities ==
                        right.CustomFamilyRecognitionIdentities &&
                    left.AmmunitionRecipes == right.AmmunitionRecipes &&
                    left.ReliableRecipes == right.ReliableRecipes &&
                    left.OrdinaryWeaponRecipes == right.OrdinaryWeaponRecipes &&
                    left.FirearmCreationBaseGuids.SequenceEqual(
                        right.FirearmCreationBaseGuids) &&
                    left.FirearmRecognitionGuids.SequenceEqual(
                        right.FirearmRecognitionGuids) &&
                    left.MartialBaseGuids.SequenceEqual(
                        right.MartialBaseGuids) &&
                    left.ExoticBaseGuids.SequenceEqual(
                        right.ExoticBaseGuids) &&
                    left.CustomFamilyRecognitionGuids.SequenceEqual(
                        right.CustomFamilyRecognitionGuids);
            }

            private static string DescribeGraph(
                CraftMagicItemsGraphSnapshot value)
            {
                return value == null ? "missing" : "generation=" +
                    value.Generation + ";itemTypes=" + value.ItemTypes +
                    ";firearmCreationBases=" +
                    value.FirearmCreationBases +
                    ";firearmRecognitionIdentities=" +
                    value.FirearmRecognitionIdentities +
                    ";martialBases=" + value.MartialBases +
                    ";exoticBases=" + value.ExoticBases +
                    ";customFamilyMagicItemTypes=" +
                    value.CustomFamilyMagicItemTypes +
                    ";customFamilyRecognitionIdentities=" +
                    value.CustomFamilyRecognitionIdentities +
                    ";ordinaryWeaponRecipes=" +
                    value.OrdinaryWeaponRecipes + ";reliableRecipes=" +
                    value.ReliableRecipes + ";ammunitionRecipes=" +
                    value.AmmunitionRecipes;
            }

            private static void Add(
                ICollection<RuntimeTestAssertion> assertions, string name,
                string expected, string observed, bool pass, string evidence)
            {
                assertions.Add(new RuntimeTestAssertion
                {
                    Name = name,
                    Expected = expected,
                    Observed = observed,
                    Status = pass ? RuntimeTestStatuses.Pass :
                        RuntimeTestStatuses.Fail,
                    Evidence = evidence
                });
            }

            private static string HashFile(string path)
            {
                using (SHA256 hash = SHA256.Create())
                using (FileStream stream = File.OpenRead(path))
                    return BitConverter.ToString(hash.ComputeHash(stream))
                        .Replace("-", string.Empty);
            }
        }
    }

    public sealed class CraftMagicItemsAmmunitionUiProbeHost : MonoBehaviour
    {
        internal static int TotalInvokeCount;
        internal CraftMagicItemsAmmunitionUiRuntimeAdapter Adapter;
        internal List<string> Failures;
        internal bool RenderEnabled;
        internal int InvokeCount;
        internal Action ConfigureAtNextLayout;

        private void OnGUI()
        {
            if (!RenderEnabled || Adapter == null ||
                (Failures != null && Failures.Count != 0)) return;
            try
            {
                Action configure = ConfigureAtNextLayout;
                string eventType =
                    CraftMagicItemsAmmunitionUiRuntimeAdapter
                        .CurrentEventType();
                if (configure != null)
                {
                    if (!CraftMagicItemsMundaneUiEventPolicy
                            .ShouldApplyPendingPhase(true, eventType)) return;
                    ConfigureAtNextLayout = null;
                    configure();
                }
                InvokeCount++;
                TotalInvokeCount++;
                Adapter.InvokeRenderer();
            }
            catch (Exception exception)
            {
                if (Failures != null) Failures.Add(exception.ToString());
                RenderEnabled = false;
            }
        }
    }
}
