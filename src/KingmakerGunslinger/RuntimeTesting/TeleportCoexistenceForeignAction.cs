using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Assets.Console.GamepadInput;
using Kingmaker.Localization;
using Kingmaker.UI.GlobalMap;
using Kingmaker.UI._ConsoleUI.Common;
using Kingmaker.UI._ConsoleUI.GlobalMap;
using Kingmaker.UI._ConsoleUI.Utils.MultiNavigationTool;
using KingmakerGunslinger.Spells.Teleportation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-local synthetic foreign owner. Never installed outside the guarded
    // coexistence scenario; it never changes a native or KMG callback.
    internal sealed class TeleportCoexistenceForeignAction : IDisposable
    {
        private static TeleportCoexistenceForeignAction _active;
        private readonly HarmonyInstance _harmony;
        private readonly bool _console;
        private readonly List<MethodInfo> _methods = new List<MethodInfo>();
        private Component _control;
        private Transform _parent;
        private int _sibling;
        private object _callback;
        private object[] _listeners;
        private Action _consoleAction;
        private ConsoleMultiNavigationCollection _navigation;
        private IConsoleMultiNavigationEntity[] _nativeNavigation;
        private object _default;
        private InputLayer[] _layers;
        internal int Fires { get; private set; }
        internal int Constructions { get; private set; }
        internal int NavigationCompositions { get; private set; }
        internal int KmgConstructions { get; private set; }
        internal bool NavigationPreserved { get; private set; } = true;
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string Label = "Guarded foreign action";
        internal TeleportCoexistenceForeignAction(bool console, string runId)
        {
            if (_active != null || string.IsNullOrEmpty(runId)) throw new InvalidOperationException("A foreign coexistence owner is already active or unguarded.");
            _console = console; _harmony = HarmonyInstance.Create("KingmakerGunslinger.Runtime.ForeignCoexistence." + runId);
            _active = this;
            try
            {
                var type = console ? typeof(GlobalMapMessageBoxView) : typeof(GlobalMapMessageBox);
                Patch(type.GetMethod("FillDialogInfoLocation", Instance, null, new[] { typeof(bool) }, null), "BeforeFill", null);
                if (console)
                {
                    var navigation = type.GetMethod("UpdateNavigation", Instance);
                    Patch(navigation, null, "BeforeKmgNavigation", Priority.First);
                    Patch(navigation, null, "AfterKmgNavigation", Priority.Last);
                }
                var create = (console ? typeof(TeleportConsoleDestinationRows) : typeof(TeleportDestinationRows))
                    .GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic);
                Patch(create, "BeforeKmgCreate", null);
            }
            catch { Dispose(); throw; }
        }
        private void Patch(MethodInfo method, string prefix, string postfix, int priority = Priority.Normal)
        {
            if (method == null) throw new InvalidOperationException("Missing exact native coexistence seam.");
            _methods.Add(method);
            Func<string, HarmonyMethod> hook = name => name == null ? null : new HarmonyMethod(typeof(TeleportCoexistenceForeignAction)
                .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)) { prioritiy = priority };
            _harmony.Patch(method, hook(prefix), hook(postfix), null);
        }
        private static void BeforeFill(object __instance) { _active?.EnsureControl((Component)__instance); }
        private static void BeforeKmgCreate() { if (_active != null) _active.KmgConstructions++; }
        private static void BeforeKmgNavigation(GlobalMapMessageBoxView __instance)
        {
            var self = _active; if (self == null || self._control == null) return;
            var nav = (ConsoleMultiNavigationCollection)typeof(GlobalMapMessageBoxView).GetField("m_NavigationCollection", Instance).GetValue(__instance);
            if (nav == null) throw new InvalidOperationException("Native navigation was not constructed before composition.");
            // Native UpdateNavigation rebuilds its own entries with SetEntities.
            // This independent owner registers its same foreign control before
            // KMG's normal-priority postfix appends KMG entries.
            var foreign = (ConsoleButton)self._control;
            if (!nav.EntitiesList.Any(value => ReferenceEquals(value, foreign))) nav.AddRow(foreign);
            self._navigation = nav; self._nativeNavigation = nav.EntitiesList.ToArray();
            self._default = nav.CurrentEntity; self._layers = GamePad.Instance.Layers.ToArray();
            self.NavigationCompositions++;
        }
        private static void AfterKmgNavigation(GlobalMapMessageBoxView __instance)
        {
            var self = _active; if (self == null) return;
            self.NavigationPreserved &= self.NavigationMatches(true);
        }
        private void EnsureControl(Component panel)
        {
            if (_control != null) return;
            var type = _console ? typeof(GlobalMapMessageBoxView) : typeof(GlobalMapMessageBox);
            var dialog = (CanvasGroup)type.GetField("m_Dialog", Instance).GetValue(panel);
            Component donor = _console ? (Component)type.GetField("m_ConfirmButton", Instance).GetValue(panel) : null;
            if (!_console)
            {
                var label = (TextMeshProUGUI)type.GetField("m_AcceptText", Instance).GetValue(panel);
                // Fill has not activated the native panel yet. GetComponentInParent
                // omits inactive ancestors in this Unity version; inspect the exact
                // native label ancestry without activating or modifying anything.
                for (var cursor = label == null ? null : label.transform; cursor != null && donor == null; cursor = cursor.parent)
                    donor = cursor.GetComponent<Button>();
            }
            if (dialog == null || donor == null) throw new InvalidOperationException("The exact inactive native foreign-control donor is unavailable.");
            var clone = UnityEngine.Object.Instantiate(donor.gameObject, dialog.transform, false);
            clone.name = "KMG_RUNTIME_FOREIGN_ACTION"; clone.SetActive(false);
            foreach (var localization in clone.GetComponentsInChildren<LocalizedUIText>(true)) UnityEngine.Object.DestroyImmediate(localization);
            if (_console)
            {
                var button = clone.GetComponent<ConsoleButton>();
                _consoleAction = () => Fires++;
                button.SetInteractable(true); button.SetSelected(false); button.SetLabel(Label); button.SetConfirmAction(_consoleAction);
                foreach (var pointer in clone.GetComponentsInChildren<Button>(true)) pointer.onClick = new Button.ButtonClickedEvent();
                _control = button; _callback = typeof(ConsoleButton).GetField("m_OnConfirmAction", Instance).GetValue(button);
            }
            else
            {
                var button = clone.GetComponent<Button>(); button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(() => Fires++); button.interactable = true;
                button.GetComponentInChildren<TextMeshProUGUI>(true).text = Label;
                _control = button; _callback = button.onClick; _listeners = DesktopListeners(button);
            }
            clone.SetActive(true); _parent = clone.transform.parent; _sibling = clone.transform.GetSiblingIndex(); Constructions++;
        }
        private static object[] DesktopListeners(Button button)
        {
            var calls = typeof(UnityEngine.Events.UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(button.onClick);
            return ((IEnumerable)calls.GetType().GetField("m_RuntimeCalls", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(calls)).Cast<object>().ToArray();
        }
        internal bool ControlMatches()
        {
            if (_control == null || _control.gameObject.name != "KMG_RUNTIME_FOREIGN_ACTION" || Constructions != 1 ||
                !ReferenceEquals(_control.transform.parent, _parent) || _control.transform.GetSiblingIndex() != _sibling ||
                !_control.gameObject.activeSelf || !((Behaviour)_control).enabled ||
                _control.GetComponentInChildren<TextMeshProUGUI>(true).text != Label) return false;
            if (_console) return (bool)typeof(ConsoleSelectableEntity).GetField("m_IsInteractable", Instance).GetValue(_control) && ReferenceEquals(_callback,
                typeof(ConsoleButton).GetField("m_OnConfirmAction", Instance).GetValue(_control)) && ReferenceEquals(_callback, _consoleAction);
            var button = (Button)_control;
            return button.interactable && ReferenceEquals(_callback, button.onClick) && button.onClick.GetPersistentEventCount() == 0 &&
                _listeners.SequenceEqual(DesktopListeners(button));
        }
        internal bool NavigationMatches(bool requireCurrent)
        {
            if (!_console) return true;
            if (_navigation == null || _nativeNavigation == null) return false;
            var retained = _navigation.EntitiesList.Where(value => !(value is Component) ||
                ((Component)value).GetComponentInParent<TeleportConsoleDestinationRows>() == null).ToArray();
            return retained.SequenceEqual(_nativeNavigation) && (!requireCurrent || ReferenceEquals(_navigation.CurrentEntity, _default)) &&
                (!requireCurrent || GamePad.Instance.Layers.SequenceEqual(_layers));
        }
        internal void Fire()
        { if (_console) ((ConsoleButton)_control).OnConfirmClick(); else ((Button)_control).onClick.Invoke(); }
        internal object Evidence()
        {
            return new { console = _console, Fires, Constructions, KmgConstructions, NavigationCompositions, NavigationPreserved,
                controlId = _control == null ? 0 : _control.GetInstanceID(), parentId = _parent == null ? 0 : _parent.GetInstanceID(),
                sibling = _control == null ? -1 : _control.transform.GetSiblingIndex(), matches = ControlMatches(), navigationMatches = NavigationMatches(false) };
        }
        public void Dispose()
        {
            if (ReferenceEquals(_active, this)) _active = null;
            foreach (var method in _methods.Distinct()) _harmony.Unpatch(method, HarmonyPatchType.All, _harmony.Id);
            if (_navigation != null && _control is ConsoleButton) _navigation.RemoveEntity((ConsoleButton)_control);
            if (_control != null) { _control.gameObject.SetActive(false); _control.transform.SetParent(null, false); UnityEngine.Object.Destroy(_control.gameObject); }
        }
    }
}
