using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Assets.Console.GamepadInput;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI._ConsoleUI.Common;
using Kingmaker.UI._ConsoleUI.DialogMessageBox;
using Kingmaker.UI._ConsoleUI.GlobalMap;
using Kingmaker.UI._ConsoleUI.Utils.MultiNavigationTool;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private Game.ControllerModeType? _teleportationOriginalController;
        private int _teleportationGamepadUiPhase;
        private BundledSceneLoader.AsyncOperation _teleportationGamepadUiOperation;
        private string _teleportationOriginalLoadingUi;
        private readonly List<object> _teleportationGamepadExceptions = new List<object>();
        private void ObserveTeleportGamepadException(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error && message.Contains("Exception"))
                _teleportationGamepadExceptions.Add(new { message, stack, type = type.ToString(), frame = Time.frameCount });
        }
        private bool PrepareTeleportGamepadUi()
        {
            if (_teleportationGamepadUiPhase == 4) return true;
            if (_teleportationGamepadUiPhase == 0)
            {
                if (Game.Instance.IsControllerGamepad || DialogMessageBox.Instance == null || DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("Guarded gamepad setup requires the unchanged desktop working-save load and no modal.");
                if (GlobalMapRules.Instance == null || GlobalMapRules.Instance.Pawn == null ||
                    !SceneManager.GetSceneByName(SceneName.CustomUIGlobalMap).isLoaded)
                    throw new InvalidOperationException("The native desktop global map must be fully loaded before controller qualification.");
                Application.logMessageReceived += ObserveTeleportGamepadException;
                _teleportationOriginalController = Game.Instance.ControllerMode;
                _teleportationOriginalLoadingUi = SceneName.LoadingScreenUI;
                if (!SceneManager.GetSceneByName(_teleportationOriginalLoadingUi).isLoaded)
                    throw new InvalidOperationException("The exact native desktop loading UI scene is absent.");
                Game.Instance.RootUiContext.DisposeLoadingScreen();
                Game.Instance.ControllerMode = (Game.ControllerModeType)1;
                _teleportationGamepadUiOperation = BundledSceneLoader.UnloadSceneAsync(_teleportationOriginalLoadingUi);
                _teleportationGamepadUiPhase = 1;
                return false;
            }
            if (!_teleportationGamepadUiOperation.IsDone) return false;
            if (_teleportationGamepadUiPhase == 1)
            {
                if (SceneManager.GetSceneByName(_teleportationOriginalLoadingUi).isLoaded)
                    throw new InvalidOperationException("Native desktop loading UI did not unload.");
                _teleportationGamepadUiOperation = BundledSceneLoader.LoadSceneAsync(SceneName.LoadingScreenUI, LoadSceneMode.Additive);
                _teleportationGamepadUiPhase = 2;
                return false;
            }
            Game.Instance.RootUiContext.InitializeLoadingScreen(SceneName.LoadingScreenUI);
            if (!SceneManager.GetSceneByName(SceneName.LoadingScreenUI).isLoaded || TeleportationConfirmationSurface.Available() == null)
                throw new InvalidOperationException("Native gamepad startup loading UI has no unique available modal.");
            CaptureTeleportInteraction("native-loading-ui-initialized", new { originalScene = _teleportationOriginalLoadingUi,
                scene = SceneName.LoadingScreenUI, modalScene = TeleportGamepadDialog().gameObject.scene.name,
                oldSceneUnloaded = !SceneManager.GetSceneByName(_teleportationOriginalLoadingUi).isLoaded,
                nativePath = "BundledSceneLoader.UnloadSceneAsync/LoadSceneAsync;RootUiContext.InitializeLoadingScreen", saveWriteObserved = _workingSaveSmoke.WriteObserved });
            // The already loaded global map has no local-area controls or decals.
            // Re-enter the verified full native same-area loading path so the
            // SceneLoader replaces and records its UI scene without a save write.
            // The public enter-point overload would only call Game.Teleport.
            var reload = typeof(Game).GetMethod("LoadArea", BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] { typeof(BlueprintArea), typeof(BlueprintAreaEnterPoint), typeof(AutoSaveMode), typeof(bool), typeof(SaveInfo) }, null);
            if (reload == null) throw new InvalidOperationException("Exact native same-area UI reload seam differs.");
            reload.Invoke(Game.Instance, new object[] { Game.Instance.CurrentlyLoadedArea,
                Game.Instance.BlueprintRoot.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None, false, null });
            _teleportationGamepadUiPhase = 4;
            return false;
        }
        private bool IsTeleportationGamepadFixture { get { return _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationGamepad || _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationCoexistenceGamepad; } }
        private void RestoreTeleportationController()
        {
            if (!_teleportationOriginalController.HasValue) return;
            var mode = _teleportationOriginalController.Value;
            Game.Instance.RootUiContext.DisposeUiScene();
            if (_teleportationGamepadUiPhase >= 2) Game.Instance.RootUiContext.DisposeLoadingScreen();
            Game.Instance.ControllerMode = mode;
            Application.logMessageReceived -= ObserveTeleportGamepadException;
            CaptureTeleportInteraction("native-ui-exceptions", new { exceptions = _teleportationGamepadExceptions });
            TeleportInteractionAssert("native-ui-exception-free", "no native or mod UI exceptions from controller preparation through cleanup",
                "exceptions=" + _teleportationGamepadExceptions.Count, _teleportationGamepadExceptions.Count == 0);
            _teleportationOriginalController = null;
            TeleportInteractionAssert("controller-restored", "request-local native loading UI context disposed and controller mode restored before mandatory process exit", "mode=" + Game.Instance.ControllerMode, Game.Instance.ControllerMode == mode);
        }
        private static object DescribeTeleportGamepadHosts()
        {
            return new { controller = Game.Instance.ControllerMode.ToString(), requiredScene = SceneName.CustomUIGlobalMap,
                context = TeleportationWorldMapAdapter.Capture(false).Diagnostic,
                panels = Resources.FindObjectsOfTypeAll<GlobalMapMessageBoxView>().Where(value => value != null).Select(value => new {
                    scene = value.gameObject.scene.name, validScene = value.gameObject.scene.IsValid(), loadedScene = value.gameObject.scene.isLoaded,
                    active = value.gameObject.activeInHierarchy }).ToArray(),
                modals = Resources.FindObjectsOfTypeAll<DialogMessageBoxView>().Where(value => value != null).Select(value => new {
                    scene = value.gameObject.scene.name, validScene = value.gameObject.scene.IsValid(), loadedScene = value.gameObject.scene.isLoaded,
                    active = value.gameObject.activeInHierarchy, parentActive = value.transform.parent != null && value.transform.parent.gameObject.activeInHierarchy,
                    bound = TeleportationConfirmationSurface.ConsoleViewModel.GetValue(value, null) != null }).ToArray(),
                legacyDesktopModal = DialogMessageBox.Instance != null };
        }
        private static GlobalMapMessageBoxView TeleportGamepadPanel()
        { return Resources.FindObjectsOfTypeAll<GlobalMapMessageBoxView>().Single(value => value != null && value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded); }
        private static DialogMessageBoxView TeleportGamepadDialog()
        { return Resources.FindObjectsOfTypeAll<DialogMessageBoxView>().Single(value => value != null && value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded && value.transform.parent != null && value.transform.parent.gameObject.activeInHierarchy); }
        private static DialogMessageBoxVM TeleportGamepadDialogModel()
        { return (DialogMessageBoxVM)TeleportationConfirmationSurface.ConsoleViewModel.GetValue(TeleportGamepadDialog(), null); }
        private static ConsoleMultiNavigationCollection TeleportGamepadNavigation(GlobalMapMessageBoxView panel)
        { return (ConsoleMultiNavigationCollection)WorldMapPointConsoleSpellActionPatches.NavigationField.GetValue(panel); }
        private static object InvokeTeleportGamepadInput(object target, string name)
        {
            var method = target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null || method.GetParameters().Length != 1 || method.GetParameters()[0].ParameterType.FullName != "Rewired.InputActionEventData")
                throw new InvalidOperationException("Exact native navigation handler differs: " + name);
            // Direct guarded invocation of the native handler, without OS input,
            // pointer events, controller emulation or synthetic event publication.
            return method.Invoke(target, new[] { Activator.CreateInstance(method.GetParameters()[0].ParameterType) });
        }
        private static string TeleportGamepadNativeButtons(GlobalMapMessageBoxView panel)
        {
            var callback = typeof(ConsoleButton).GetField("m_OnConfirmAction", BindingFlags.Instance | BindingFlags.NonPublic);
            return TeleportationDiagnosticJson.Serialize(panel.GetComponentsInChildren<ConsoleButton>(true)
                .Where(value => value.GetComponentInParent<TeleportConsoleDestinationRows>() == null)
                .Select(value => new { id = value.GetInstanceID(), value.name, active = value.gameObject.activeSelf, valid = value.IsValid(),
                    label = value.GetComponentInChildren<TextMeshProUGUI>(true).text,
                    callback = callback.GetValue(value) == null ? null : ((Action)callback.GetValue(value)).Method.Name }).ToArray());
        }
        private static void CloseTeleportGamepadPanels()
        {
            var dialog = TeleportGamepadDialogModel(); if (dialog != null) dialog.ForceHide();
            var panel = TeleportGamepadPanel();
            var model = WorldMapPointConsoleSpellActionPatches.Model(panel); if (model != null) model.Cancel();
        }
        private static IEnumerable<int> WaitTeleportGamepadPanel(GlobalMapMessageBoxView panel)
        {
            var watch = Stopwatch.StartNew();
            var dialog = (CanvasGroup)WorldMapPointConsoleSpellActionPatches.DialogField.GetValue(panel);
            while (dialog.alpha < 0.99f)
            {
                if (!panel.gameObject.activeInHierarchy || watch.Elapsed.TotalSeconds > 3) throw new InvalidOperationException("Native gamepad point fade did not complete.");
                yield return 0;
            }
            for (int frame = 0; frame < 4; frame++) yield return 0;
        }
        private static void SelectTeleportGamepadPoint(GlobalMapLocation location)
        {
            TeleportationCastingCamera().ScrollToImmediately(location.transform.position);
            EventBus.RaiseEvent<ILocationSelectionHandler>(handler => handler.OnLocationSelect(location.Blueprint, false));
        }
        private TeleportContextConfirmationPresenter OpenTeleportGamepadSpell(GlobalMapLocation location, TeleportSpellKind kind,
            TeleportCastSourceKind sourceKind, TeleportationFixtureRolls rolls)
        {
            SelectTeleportGamepadPoint(location);
            var panel = TeleportGamepadPanel();
            var rows = panel.GetComponentInChildren<TeleportConsoleDestinationRows>(true);
            if (rows == null) throw new InvalidOperationException("No positive native gamepad source rows.");
            rows.QualificationRolls = rolls;
            int index = rows.Actions.ToList().FindIndex(value => value.Source.Spell == kind && value.Source.Kind == sourceKind);
            if (index < 0) throw new InvalidOperationException("The requested real gamepad source is absent.");
            TeleportGamepadNavigation(panel).SetCurrentEntityManual(rows.Buttons[index]);
            InvokeTeleportGamepadInput(panel, "OnConfirmPressed");
            var cast = TeleportContextConfirmationPresenter.Current;
            if (cast == null || TeleportGamepadDialogModel() == null || panel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Native gamepad selection did not open its owned confirmation.");
            CaptureTeleportInteraction("console-confirmation", new { message = cast.Message,
                displayed = TeleportGamepadDialog().GetComponentsInChildren<TextMeshProUGUI>(true).Select(value => value.text).ToArray(),
                layers = GamePad.Instance.Layers.Select(value => value.ContextName).ToArray() });
            return cast;
        }
        private static bool TeleportGamepadTextOnScreen(TextMeshProUGUI label)
        {
            // The loading-screen modal owns a different canvas/camera from the
            // global-map destination. UIUtility assumes Game.UI.UICamera there.
            var canvas = label.GetComponentInParent<Canvas>().rootCanvas;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && camera == null) return false;
            var corners = new Vector3[4]; ((RectTransform)label.transform).GetWorldCorners(corners);
            return corners.Select(value => RectTransformUtility.WorldToScreenPoint(camera, value)).All(value =>
                value.x >= 0 && value.y >= 0 && value.x <= Screen.width && value.y <= Screen.height);
        }
        private static IEnumerable<int> WaitTeleportGamepadModal()
        {
            var view = TeleportGamepadDialog();
            var animator = (WindowAnimator)typeof(DialogMessageBoxView).GetField("m_WindowAnimator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
            var group = (CanvasGroup)typeof(WindowAnimator).GetField("m_CanvasGroup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(animator);
            var watch = Stopwatch.StartNew();
            while (group.alpha < 0.999f || Quaternion.Angle(animator.transform.localRotation, Quaternion.identity) > 0.05f)
            {
                if (!view.gameObject.activeInHierarchy || watch.Elapsed.TotalSeconds > 3)
                    throw new InvalidOperationException("Native modal appearance did not complete.");
                yield return 0;
            }
            for (int frame = 0; frame < 4; frame++) yield return 0;
        }
        private string[] CaptureTeleportGamepadRendered(TeleportContextConfirmationPresenter cast)
        {
            var labels = TeleportGamepadDialog().GetComponentsInChildren<TextMeshProUGUI>(true);
            CaptureTeleportInteraction("console-rendered-text", new { expected = cast.Message,
                labels = labels.Select(value => new { value.name, value.text, parsed = value.GetParsedText(), active = value.gameObject.activeInHierarchy,
                    value.isActiveAndEnabled, value.fontStyle, onScreen = TeleportGamepadTextOnScreen(value), canvasMode = value.GetComponentInParent<Canvas>().rootCanvas.renderMode.ToString(),
                    canvasCamera = value.GetComponentInParent<Canvas>().rootCanvas.worldCamera == null ? null : value.GetComponentInParent<Canvas>().rootCanvas.worldCamera.name, value.textInfo.characterCount, value.isTextOverflowing, value.isTextTruncated,
                    value.overflowMode, value.maxVisibleCharacters, value.maxVisibleLines, rect = ((RectTransform)value.transform).rect.ToString() }).ToArray() });
            // Native console labels render uppercase/small caps. Preserve every
            // number, line break and other character while accepting that styling.
            return labels.Where(value => value.isActiveAndEnabled && !value.isTextTruncated && !value.isTextOverflowing &&
                TeleportGamepadTextOnScreen(value)).Select(value => value.GetParsedText()).ToArray();
        }
        private static void ConfirmTeleportGamepadSpell()
        {
            var view = TeleportGamepadDialog();
            var nav = (ConsoleMultiNavigationCollection)typeof(DialogMessageBoxView).GetField("m_NavigationCollection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
            var yes = (ConsoleButton)typeof(DialogMessageBoxView).GetField("m_YesButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
            nav.SetCurrentEntityManual(yes);
            InvokeTeleportGamepadInput(view, "OnConfirmClick");
        }
        private IEnumerable<int> RunTeleportationGamepad(GlobalMapLocation origin, GlobalMapLocation middle, GlobalMapLocation target,
            List<TeleportResourceFixtureOwner> owners, TeleportInteractionMovementObserver movement)
        {
            var player = Game.Instance.Player; var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State;
            var ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
            var panel = TeleportGamepadPanel();
            var modal = TeleportGamepadDialog();
            var baseLayers = GamePad.Instance.Layers.ToArray();
            var time = player.GameTime;
            TeleportInteractionAssert("native-scene", "native SceneLoader loaded the real gamepad global-map UI", "scene=" + panel.gameObject.scene.name,
                Game.Instance.IsControllerGamepad && panel.gameObject.scene.name == SceneName.CustomUIGlobalMap &&
                panel.gameObject.scene.name == "UI_Globalmap_Scene_Console" && WorldMapPointConsoleSpellActionPatches.Installed);
            CaptureTeleportInteraction("console-native-hosts", new { panelScene = panel.gameObject.scene.name, modalScene = modal.gameObject.scene.name,
                panelType = panel.GetType().FullName, modalType = modal.GetType().FullName, baseLayers = baseLayers.Select(value => value.ContextName).ToArray() });
            string beforeLedger = ledger.Read().Serialize();
            SelectTeleportGamepadPoint(target);
            foreach (int tick in WaitTeleportGamepadPanel(panel)) yield return tick;
            var dialog = (CanvasGroup)WorldMapPointConsoleSpellActionPatches.DialogField.GetValue(panel);
            var nav = TeleportGamepadNavigation(panel);
            var nativeEntities = nav.EntitiesList.ToArray();
            var nativeDefault = nav.CurrentEntity;
            string nativeButtons = TeleportGamepadNativeButtons(panel);
            TeleportInteractionAssert("vanilla-no-source", "native entities/default only, no source container or extra confirmation", "entities=" + nativeEntities.Length,
                panel.GetComponentsInChildren<TeleportConsoleDestinationRows>(true).Length == 0 && !TeleportContextConfirmationPresenter.Pending &&
                TeleportGamepadDialogModel() == null && UIUtility.IsTransformInScreen(dialog.transform) && beforeLedger == ledger.Read().Serialize());
            TeleportInteractionAssert("vanilla-default", "native default still invokes the original Travel button",
                "current=" + ((Component)nativeDefault).name, ReferenceEquals(nativeDefault, WorldMapPointConsoleSpellActionPatches.ConfirmField.GetValue(panel)));
            InvokeTeleportGamepadInput(panel, "OnCancelPressed"); yield return 0;
            TeleportInteractionAssert("vanilla-cancel", "native cancel pops only its input layer and starts no travel", "starts=" + movement.Starts,
                !panel.gameObject.activeInHierarchy && map.TravelData == null && movement.Starts == 0 && GamePad.Instance.Layers.SequenceEqual(baseLayers));
            RunTeleportGamepadTravel(origin, middle, target, ledger, movement, "vanilla", () => string.Empty, 0);
            rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition(); player.GameTime = time;
            var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "ba34257984f4c41408ce1dc2004e342e", "native gamepad Wizard");
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "b3a505fb61437dc4097f43c3f8f9a4cf", "native gamepad Sorcerer");
            var units = player.Party.Where(TeleportationSpellbookAdapter.CasterAvailable).Where(value => value.Descriptor.GetSpellbook(wizard.Spellbook) == null &&
                value.Descriptor.GetSpellbook(sorcerer.Spellbook) == null).Take(2).ToArray();
            if (units.Length != 2) throw new InvalidOperationException("Two suitable real native gamepad fixture owners are required.");
            foreach (var unit in units) owners.Add(new TeleportResourceFixtureOwner(unit));
            var books = new[] { owners[0].AddBook(wizard.Spellbook), owners[0].AddBook(sorcerer.Spellbook), owners[1].AddBook(wizard.Spellbook) };
            foreach (var book in books) PrepareTeleportGamepadBook(book);
            Func<string> slots = () => string.Join("|", books.Select(TeleportResourceFingerprint));
            string beforeSlots = slots(); beforeLedger = ledger.Read().Serialize();
            SelectTeleportGamepadPoint(target);
            foreach (int tick in WaitTeleportGamepadPanel(panel)) yield return tick;
            var rows = panel.GetComponentInChildren<TeleportConsoleDestinationRows>(true);
            nav = TeleportGamepadNavigation(panel);
            TeleportInteractionAssert("native-actions-preserved", "native controls/callbacks/order/default precede six distinct real sources",
                "rows=" + (rows == null ? 0 : rows.Actions.Count), rows != null && rows.Actions.Count == 6 &&
                nav.EntitiesList.Take(nativeEntities.Length).SequenceEqual(nativeEntities) && ReferenceEquals(nav.CurrentEntity, nativeDefault) &&
                nativeButtons == TeleportGamepadNativeButtons(panel) && nav.EntitiesList.Skip(nativeEntities.Length).SequenceEqual(rows.Buttons.Cast<IConsoleMultiNavigationEntity>()));
            TeleportInteractionAssert("live-labels", "each usable source has its exact current caster/book/count label and deterministic order", "rows=" + rows.Actions.Count,
                rows.Actions.Select(value => value.Key).Distinct().Count() == 6 && rows.Actions.Take(3).All(value => value.Source.Spell == TeleportSpellKind.GreaterTeleport) &&
                rows.Buttons.Select((button, index) => button.IsValid() && button.GetComponentInChildren<TextMeshProUGUI>(true).text ==
                    TeleportContextPresentation.Row(rows.Actions[index], TeleportationText.Get)).All(value => value));
            var traversed = new List<int>();
            for (int index = 0; index < rows.Actions.Count; index++)
            {
                InvokeTeleportGamepadInput(nav, "TryDoDown"); yield return 0;
                traversed.Add(((Component)nav.CurrentEntity).GetInstanceID());
            }
            TeleportInteractionAssert("native-direction-navigation", "native downward navigation reaches every appended source in spell/source order", "traversed=" + traversed.Count,
                traversed.SequenceEqual(rows.Buttons.Select(value => value.GetInstanceID())) && slots() == beforeSlots && ledger.Read().Serialize() == beforeLedger);
            var firstViewport = rows.GetComponent<ScrollRect>().viewport.rect.height;
            for (int repeat = 0; repeat < 6; repeat++)
            {
                InvokeTeleportGamepadInput(panel, "OnCancelPressed");
                SelectTeleportGamepadPoint(target); foreach (int tick in WaitTeleportGamepadPanel(panel)) yield return tick;
            }
            rows = panel.GetComponentInChildren<TeleportConsoleDestinationRows>(true);
            nav = TeleportGamepadNavigation(panel);
            TeleportInteractionAssert("reopen-no-duplicates", "six reopens retain one source container, six rows, unchanged native default, stable geometry and one input layer",
                "entities=" + nav.EntitiesList.Count, panel.GetComponentsInChildren<TeleportConsoleDestinationRows>(true).Length == 1 && rows.Actions.Count == 6 &&
                nav.EntitiesList.Count == nativeEntities.Length + 6 && ReferenceEquals(nav.CurrentEntity, nativeDefault) &&
                Math.Abs(firstViewport - rows.GetComponent<ScrollRect>().viewport.rect.height) < 0.01f && GamePad.Instance.Layers.Count == baseLayers.Length + 1);
            InvokeTeleportGamepadInput(panel, "OnCancelPressed"); yield return 0;
            TeleportInteractionAssert("augmented-cancel", "native cancel spends zero, removes appended navigation and restores input layers", "slotsUnchanged=" + (slots() == beforeSlots),
                slots() == beforeSlots && map.TravelData == null && GamePad.Instance.Layers.SequenceEqual(baseLayers) && nav.EntitiesList.SequenceEqual(nativeEntities));
            var payload = typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
            var savedPayload = payload.GetValue(ledger); bool explored = target.Data.IsExplored;
            var unknown = new TeleportFamiliarityState(); unknown.MigrateLegacy(new[] { origin.Blueprint.AssetGuid, middle.Blueprint.AssetGuid });
            payload.SetValue(ledger, unknown.Serialize()); target.Data.EdgesOpened = false; target.Data.IsExplored = false;
            SelectTeleportGamepadPoint(target); foreach (int tick in WaitTeleportGamepadPanel(panel)) yield return tick;
            TeleportInteractionAssert("unvisited-omitted", "unvisited native point has no magical rows or extra modal", "containers=" + panel.GetComponentsInChildren<TeleportConsoleDestinationRows>(true).Length,
                panel.GetComponentsInChildren<TeleportConsoleDestinationRows>(true).Length == 0 && slots() == beforeSlots && TeleportGamepadDialogModel() == null);
            InvokeTeleportGamepadInput(panel, "OnCancelPressed");
            payload.SetValue(ledger, savedPayload); target.Data.EdgesOpened = true; target.Data.IsExplored = explored;
            SelectTeleportGamepadPoint(origin); yield return 0;
            TeleportInteractionAssert("current-point-omitted", "the current native point offers no no-op magical expenditure", "containers=" + panel.GetComponentsInChildren<TeleportConsoleDestinationRows>(true).Length,
                panel.GetComponentsInChildren<TeleportConsoleDestinationRows>(true).Length == 0 && slots() == beforeSlots && !TeleportContextConfirmationPresenter.Pending);
            var currentModel = WorldMapPointConsoleSpellActionPatches.Model(panel); if (currentModel != null) currentModel.Cancel();
            target.Data.IsClosed = true;
            SelectTeleportGamepadPoint(target); foreach (int tick in WaitTeleportGamepadPanel(panel)) yield return tick;
            TeleportInteractionAssert("closed-point-omitted", "a native closed point keeps native actions with no magical row", "containers=" + panel.GetComponentsInChildren<TeleportConsoleDestinationRows>(true).Length,
                panel.GetComponentsInChildren<TeleportConsoleDestinationRows>(true).Length == 0 && slots() == beforeSlots && TeleportGamepadDialogModel() == null);
            InvokeTeleportGamepadInput(panel, "OnCancelPressed"); target.Data.IsClosed = false;
            var cancel = OpenTeleportGamepadSpell(target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, new TeleportationFixtureRolls(new[] { 1 }));
            foreach (int tick in WaitTeleportGamepadModal()) yield return tick;
            var renderedMessage = CaptureTeleportGamepadRendered(cancel);
            TeleportInteractionAssert("confirmation-rendered-text", "native rendered modal contains the exact selected source/destination/odds and localized Cast/Cancel actions",
                "message=" + cancel.Message, renderedMessage.Contains(cancel.Message, StringComparer.OrdinalIgnoreCase) && renderedMessage.Contains(TeleportationText.Get("Confirm", "Cast"), StringComparer.OrdinalIgnoreCase) &&
                renderedMessage.Contains(TeleportationText.Get("Cancel", "Cancel"), StringComparer.OrdinalIgnoreCase) && cancel.Message.Contains("On target: 88%") && cancel.Message.Contains("Ordinary visits: 2"));
            InvokeTeleportGamepadInput(modal, "OnDeclineClicked"); yield return 0;
            TeleportInteractionAssert("confirmation-native-cancel", "native modal decline cancels the request with zero debit or movement and restores input layers",
                "transaction=" + cancel.Transaction.State, cancel.Transaction.State == TeleportTransactionState.Cancelled && slots() == beforeSlots &&
                map.PartyLocation == origin.Blueprint && !TeleportContextConfirmationPresenter.Pending && TeleportGamepadDialogModel() == null && GamePad.Instance.Layers.SequenceEqual(baseLayers));
            var stale = OpenTeleportGamepadSpell(target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, new TeleportationFixtureRolls(new[] { 1 }));
            target.Data.IsClosed = true;
            for (int frame = 0; frame < 4; frame++) yield return 0;
            target.Data.IsClosed = false;
            TeleportInteractionAssert("stale-destination-cancel", "invalid destination closes only the owned native modal before expenditure", "transaction=" + stale.Transaction.State,
                stale.Transaction.State == TeleportTransactionState.Cancelled && slots() == beforeSlots && TeleportGamepadDialogModel() == null && !TeleportContextConfirmationPresenter.Pending);
            var replaced = OpenTeleportGamepadSpell(target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, new TeleportationFixtureRolls(new[] { 1 }));
            Action<DialogMessageBoxBase.BoxButton> unrelated = button => { };
            EventBus.RaiseEvent<IDialogMessageBoxUIHandler>(handler => handler.HandleOpen("Guarded native replacement control", DialogMessageBoxBase.BoxType.Dialog, unrelated, "Yes", "No", null, null));
            for (int frame = 0; frame < 4; frame++) yield return 0;
            TeleportInteractionAssert("replacement-ownership", "native modal replacement cancels old cast and retains the unrelated replacement", "transaction=" + replaced.Transaction.State,
                replaced.Transaction.State == TeleportTransactionState.Cancelled && slots() == beforeSlots && TeleportGamepadDialogModel() != null &&
                ReferenceEquals(TeleportationConfirmationSurface.ConsoleCallback.GetValue(TeleportGamepadDialogModel()), unrelated));
            TeleportGamepadDialogModel().ForceHide(); yield return 0;
            SelectTeleportGamepadPoint(target); foreach (int tick in WaitTeleportGamepadPanel(panel)) yield return tick;
            rows = panel.GetComponentInChildren<TeleportConsoleDestinationRows>(true); nav = TeleportGamepadNavigation(panel);
            var live = rows.Actions.First(value => value.Source.Spell == TeleportSpellKind.Teleport &&
                value.Source.BookId == books[0].Blueprint.AssetGuid && value.Source.CasterId == units[0].UniqueId);
            var resource = TeleportationSpellbookAdapter.Resolve(live.Source);
            if (resource == null || !resource.Book.Spend(resource.Ability, false)) throw new InvalidOperationException("Native gamepad external debit failed.");
            for (int frame = 0; frame < 4; frame++) yield return 0;
            var fresh = rows.Actions.Single(value => value.Key == live.Key);
            int liveIndex = rows.Actions.ToList().FindIndex(value => value.Key == live.Key);
            var exhaustedButton = rows.Buttons[liveIndex]; nav.SetCurrentEntityManual(exhaustedButton);
            TeleportInteractionAssert("live-source-count", "open native row displays the new prepared count after a real native debit",
                "before=" + live.Source.Uses + ";after=" + fresh.Source.Uses, fresh.Source.Uses == live.Source.Uses - 1 &&
                exhaustedButton.GetComponentInChildren<TextMeshProUGUI>(true).text == TeleportContextPresentation.Row(fresh, TeleportationText.Get));
            resource = TeleportationSpellbookAdapter.Resolve(fresh.Source);
            if (resource == null || !resource.Book.Spend(resource.Ability, false)) throw new InvalidOperationException("Native gamepad final debit failed.");
            for (int frame = 0; frame < 4; frame++) yield return 0;
            TeleportInteractionAssert("exhausted-navigation-removed", "exhausted selected row disappears from UI and native navigation without disabling remaining actions",
                "rows=" + rows.Actions.Count, rows.Actions.Count == 5 && rows.Actions.All(value => value.Key != live.Key) &&
                nav.EntitiesList.Count == nativeEntities.Length + 5 && !nav.EntitiesList.Any(value => ReferenceEquals(value, exhaustedButton)) &&
                nav.CurrentEntity != null && nav.EntitiesList.Contains(nav.CurrentEntity) && rows.Buttons.All(value => value.IsValid()));
            InvokeTeleportGamepadInput(panel, "OnCancelPressed"); foreach (var book in books) book.Rest();
            beforeSlots = slots();
            RunTeleportGamepadTravel(origin, middle, target, ledger, movement, "augmented", slots, 6);
            rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition(); player.GameTime = time;
            var rolls = new TeleportationFixtureRolls(new[] { 1 });
            int castStarts = movement.Starts, castStops = movement.Stops;
            var cast = OpenTeleportGamepadSpell(target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, rolls);
            var duplicate = (Action<DialogMessageBoxBase.BoxButton>)TeleportationConfirmationSurface.ConsoleCallback.GetValue(TeleportGamepadDialogModel());
            for (int frame = 0; frame < 8; frame++) yield return 0;
            ConfirmTeleportGamepadSpell(); duplicate(DialogMessageBoxBase.BoxButton.Yes);
            for (int frame = 0; frame < 4; frame++) yield return 0;
            CaptureTeleportInteraction("console-teleport-result", new { transaction = cast.Transaction.State.ToString(), cast.Execution.LastEvidence, rolls.D100Count, movement.Starts });
            TeleportInteractionAssert("teleport-real-commit", "native gamepad confirmation spends exactly one preparation and relocates through the production path once",
                "transaction=" + cast.Transaction.State, cast.Transaction.State == TeleportTransactionState.Completed && cast.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne &&
                rolls.D100Count == 1 && map.PartyLocation == target.Blueprint && movement.Starts == castStarts + 1 && movement.Stops == castStops + 1 && map.TravelData == null && player.GameTime == time && GamePad.Instance.Layers.SequenceEqual(baseLayers));
            rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
            castStarts = movement.Starts; castStops = movement.Stops;
            var exactRolls = new TeleportationFixtureRolls(new int[0]);
            var exact = OpenTeleportGamepadSpell(target, TeleportSpellKind.GreaterTeleport, TeleportCastSourceKind.Spontaneous, exactRolls);
            foreach (int tick in WaitTeleportGamepadModal()) yield return tick;
            TeleportInteractionAssert("greater-confirmation-rendered-text", "native modal presents the complete exact-arrival and seventh-level slot statement",
                exact.Message, CaptureTeleportGamepadRendered(exact).Contains(exact.Message, StringComparer.OrdinalIgnoreCase));
            ConfirmTeleportGamepadSpell(); yield return 0;
            TeleportInteractionAssert("greater-real-commit", "native Greater Teleport uses one seventh-level slot, no destination roll, no route/time change", "transaction=" + exact.Transaction.State,
                exact.Transaction.State == TeleportTransactionState.Completed && exact.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne && exactRolls.D100Count == 0 &&
                map.PartyLocation == target.Blueprint && movement.Starts == castStarts + 1 && movement.Stops == castStops + 1 && map.TravelData == null && player.GameTime == time);
            rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
            foreach (var book in books) book.Rest();
            var druid = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "610d836f3a3a9ed42a4349b62f002e96", "native gamepad long-list Druid");
            foreach (var book in new[] { owners[1].AddBook(sorcerer.Spellbook), owners[0].AddBook(druid.Spellbook), owners[1].AddBook(druid.Spellbook) }) PrepareTeleportGamepadBook(book);
            SelectTeleportGamepadPoint(target); foreach (int tick in WaitTeleportGamepadPanel(panel)) yield return tick;
            rows = panel.GetComponentInChildren<TeleportConsoleDestinationRows>(true); nav = TeleportGamepadNavigation(panel);
            var scroll = rows.GetComponent<ScrollRect>();
            bool everyReachable = rows.Actions.Count == 12;
            for (int index = 0; index < rows.Actions.Count; index++)
            {
                InvokeTeleportGamepadInput(nav, "TryDoDown");
                for (int frame = 0; frame < 3; frame++) yield return 0;
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, rows.Buttons[index].transform);
                everyReachable &= ReferenceEquals(nav.CurrentEntity, rows.Buttons[index]) && scroll.viewport.rect.Contains(bounds.center);
            }
            CaptureTeleportInteraction("console-long-source-navigation", new { count = rows.Actions.Count, everyReachable,
                contentHeight = scroll.content.rect.height, viewportHeight = scroll.viewport.rect.height, scroll.verticalNormalizedPosition });
            TeleportInteractionAssert("long-list-native-navigation", "native navigation reaches all twelve sources while scrolling only the appended viewport", "allReachable=" + everyReachable,
                everyReachable && scroll.content.rect.height > scroll.viewport.rect.height && UIUtility.IsTransformInScreen(dialog.transform) && nativeButtons == TeleportGamepadNativeButtons(panel));
            InvokeTeleportGamepadInput(panel, "OnCancelPressed"); yield return 0;
            foreach (int tick in RunTeleportGamepadRecall(origin, target, owners[0], movement)) yield return tick;
            TeleportInteractionAssert("final-native-input-cleanup", "no appended navigation/confirmation/input layer remains", "layers=" + GamePad.Instance.Layers.Count,
                GamePad.Instance.Layers.SequenceEqual(baseLayers) && !TeleportContextConfirmationPresenter.Pending && TeleportGamepadDialogModel() == null && nav.EntitiesList.SequenceEqual(nativeEntities));
        }
        private IEnumerable<int> RunTeleportGamepadRecall(GlobalMapLocation origin, GlobalMapLocation other,
            TeleportResourceFixtureOwner owner, TeleportInteractionMovementObserver movement)
        {
            var player = Game.Instance.Player; var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State;
            var ledger = TeleportFamiliarityRuntime.EnsureLedger(player);
            var payload = typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
            var originalPayload = payload.GetValue(ledger); var originalKingdom = player.Kingdom;
            var kingdom = originalKingdom;
            var capitalBlueprint = KingdomRoot.Instance.BlueprintRegionCapital;
            if (kingdom == null)
            {
                var constructor = typeof(KingdomState).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Single(value => value.GetParameters().Length == 1 && value.GetParameters()[0].ParameterType.Name == "JsonConstructorMark");
                kingdom = (KingdomState)constructor.Invoke(new[] { Activator.CreateInstance(constructor.GetParameters()[0].ParameterType) });
                typeof(KingdomState).GetField("Regions").SetValue(kingdom, new List<RegionState> { new RegionState(capitalBlueprint) });
            }
            var region = kingdom.Regions.Single(value => ReferenceEquals(value.Blueprint, capitalBlueprint));
            var settlement = region.Settlement; bool claimed = region.IsClaimed;
            if (!capitalBlueprint.SettlementIsPrebuilt || settlement == null || !ReferenceEquals(settlement.Region, region) ||
                settlement.Location == null || settlement.Location.AssetGuid != WordOfRecallDestinationPolicy.CapitalId)
                throw new InvalidOperationException("Native prebuilt Recall capital contract differs.");
            var setClaimed = typeof(RegionState).GetProperty("IsClaimed").GetSetMethod(true);
            var reveal = typeof(LocationData).GetProperty("IsRevealed").GetSetMethod(true);
            var oleg = rules.AllLocations.Single(value => value.Blueprint.AssetGuid == WordOfRecallDestinationPolicy.OlegId);
            var capital = rules.AllLocations.Single(value => value.Blueprint.AssetGuid == WordOfRecallDestinationPolicy.CapitalId);
            var panel = TeleportGamepadPanel();
            var time = player.GameTime;
            try
            {
                player.Kingdom = kingdom;
                foreach (var point in new[] { oleg, capital })
                { reveal.Invoke(point.Data, new object[] { true }); point.Data.EdgesOpened = true; point.Data.IsClosed = false; }
                var counts = new TeleportFamiliarityState(); counts.MigrateLegacy(new[] { origin, other, oleg, capital }.Select(value => value.Blueprint.AssetGuid));
                payload.SetValue(ledger, counts.Serialize());
                var cleric = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "67819271767a9dd4fbfd4ae700befea0", "native gamepad Recall Cleric");
                var book = owner.AddBook(cleric.Spellbook);
                if (!book.IsKnown(BlueprintBootstrap.Teleportation.WordOfRecall)) book.AddKnown(6, BlueprintBootstrap.Teleportation.WordOfRecall, true);
                if (!book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.WordOfRecall, book), null))
                    throw new InvalidOperationException("Native gamepad Recall preparation failed.");
                foreach (bool established in new[] { false, true })
                {
                    int starts = movement.Starts;
                    setClaimed.Invoke(region, new object[] { established }); book.Rest();
                    rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                    var required = established ? capital : oleg;
                    foreach (var selected in new[] { oleg, capital, other })
                    {
                        SelectTeleportGamepadPoint(selected); foreach (int tick in WaitTeleportGamepadPanel(panel)) yield return tick;
                        var rows = panel.GetComponentInChildren<TeleportConsoleDestinationRows>(true);
                        var recalls = rows == null ? new WorldMapPointSpellAction[0] : rows.Actions.Where(value => value.Source.Spell == TeleportSpellKind.WordOfRecall).ToArray();
                        TeleportInteractionAssert("recall-" + (established ? "capital" : "precapital") + "-" + selected.Blueprint.AssetGuid,
                            "Recall appears first only at the exact required stable destination", "recallRows=" + recalls.Length,
                            recalls.Length == (selected == required ? 1 : 0) && (recalls.Length == 0 || rows.Actions[0] == recalls[0]));
                        InvokeTeleportGamepadInput(panel, "OnCancelPressed");
                    }
                    var dice = new TeleportationFixtureRolls(new int[0]);
                    var cast = OpenTeleportGamepadSpell(required, TeleportSpellKind.WordOfRecall, TeleportCastSourceKind.Prepared, dice);
                    foreach (int tick in WaitTeleportGamepadModal()) yield return tick;
                    bool displayed = CaptureTeleportGamepadRendered(cast).Contains(cast.Message, StringComparer.OrdinalIgnoreCase);
                    ConfirmTeleportGamepadSpell(); yield return 0;
                    CaptureTeleportInteraction("console-recall-result", new { established, displayed, transaction = cast.Transaction.State.ToString(), cast.Execution.LastEvidence });
                    TeleportInteractionAssert("recall-" + (established ? "capital" : "precapital") + "-real-commit",
                        "native Recall confirmation spends one preparation, exact world-map point, no dice, route, time or familiarity change",
                        "transaction=" + cast.Transaction.State, displayed && cast.Transaction.State == TeleportTransactionState.Completed &&
                        cast.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne && dice.D100Count == 0 && dice.D10Count == 0 &&
                        map.PartyLocation == required.Blueprint && movement.Starts == starts + 1 && map.TravelData == null && player.GameTime == time &&
                        ledger.Read().Serialize() == counts.Serialize());
                }
            }
            finally
            {
                CloseTeleportGamepadPanels();
                region.Settlement = settlement; setClaimed.Invoke(region, new object[] { claimed }); player.Kingdom = originalKingdom;
                if (!ReferenceEquals(kingdom, originalKingdom)) kingdom.Dispose();
                payload.SetValue(ledger, originalPayload);
                rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                TeleportInteractionAssert("recall-campaign-cleanup", "exact original kingdom, claim, settlement and familiarity restored without saving",
                    "restored=" + ReferenceEquals(player.Kingdom, originalKingdom), ReferenceEquals(player.Kingdom, originalKingdom) &&
                    region.IsClaimed == claimed && ReferenceEquals(region.Settlement, settlement) && Equals(payload.GetValue(ledger), originalPayload) && !_workingSaveSmoke.WriteObserved);
            }
        }
        private static void PrepareTeleportGamepadBook(Kingmaker.UnitLogic.Spellbook book)
        {
            book.AddKnown(5, BlueprintBootstrap.Teleportation.Teleport, true); book.AddKnown(7, BlueprintBootstrap.Teleportation.GreaterTeleport, true);
            if (!book.Blueprint.Spontaneous && (!book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.Teleport, book), null) ||
                !book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.Teleport, book), null) || !book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.GreaterTeleport, book), null)))
                throw new InvalidOperationException("Native gamepad fixture preparation failed.");
            book.Rest();
        }
        private void RunTeleportGamepadTravel(GlobalMapLocation origin, GlobalMapLocation middle, GlobalMapLocation target,
            UnitPartTeleportFamiliarity ledger, TeleportInteractionMovementObserver movement, string name, Func<string> slots, int expectedRows)
        {
            var rules = GlobalMapRules.Instance; var map = GlobalMapRules.State; var time = Game.Instance.Player.GameTime;
            float delta = Game.Instance.TimeController.DeltaTime; var before = ledger.Read(); string beforeSlots = slots(); int starts = movement.Starts;
            SelectTeleportGamepadPoint(target); var panel = TeleportGamepadPanel();
            int rowCount = panel.GetComponentsInChildren<TeleportConsoleDestinationRows>(true).Sum(value => value.Actions.Count);
            InvokeTeleportGamepadInput(panel, "OnConfirmPressed"); var travel = map.TravelData;
            if (travel == null || !travel.Walking || travel.Path.Count != 2) throw new InvalidOperationException("Native gamepad travel did not start the expected two-edge route.");
            TeleportInteractionAssert(name + "-travel-once", "unchanged native default starts exactly one route and spends zero spell uses", "startsDelta=" + (movement.Starts - starts),
                movement.Starts == starts + 1 && rowCount == expectedRows && slots() == beforeSlots && !panel.gameObject.activeInHierarchy && !TeleportContextConfirmationPresenter.Pending);
            try
            {
                float speed = Game.Instance.BlueprintRoot.GlobalMap.VisualSpeedBase * (float)typeof(MapMovementController).GetMethod("CalcSpeedModifiers", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                if (speed <= 0 || float.IsNaN(speed) || float.IsInfinity(speed)) throw new InvalidOperationException("Native movement speed is unproven.");
                float length = travel.Path.Sum(value => rules.GetEdgeObject(value.Blueprint).Spline.WorldLength);
                Game.Instance.TimeController.SetDeltaTime((length + 0.01f) / speed); new MapMovementController().Tick();
                TeleportInteractionAssert(name + "-ordinary-arrivals", "ordinary route counts intermediate and final arrival once, leaves origin count and spell uses unchanged",
                    "ledger=" + ledger.Read().Serialize(), map.TravelData == null && map.PartyLocation == target.Blueprint &&
                    new[] { middle, target }.All(value => ledger.Read().Count(value.Blueprint.AssetGuid) == before.Count(value.Blueprint.AssetGuid) + 1) &&
                    ledger.Read().Count(origin.Blueprint.AssetGuid) == before.Count(origin.Blueprint.AssetGuid) && beforeSlots == slots());
            }
            finally { Game.Instance.TimeController.SetDeltaTime(delta); Game.Instance.Player.GameTime = time; }
        }
    }
}
