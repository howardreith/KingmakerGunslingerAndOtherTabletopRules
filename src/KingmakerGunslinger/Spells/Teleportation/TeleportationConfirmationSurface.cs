using System;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.UI;
using Kingmaker.UI._ConsoleUI.DialogMessageBox;
using UnityEngine;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Captures one existing native modal host. It never creates a separate window
    // or replaces a native event handler. Reflection only reads exact ownership.
    internal sealed class TeleportationConfirmationSurface
    {
        internal static readonly PropertyInfo ConsoleViewModel = typeof(DialogMessageBoxView).BaseType.GetProperty("ViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static readonly FieldInfo ConsoleCallback = typeof(DialogMessageBoxVM).GetField("m_OnClose", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly DialogMessageBox _desktop;
        private readonly DialogMessageBoxView _console;
        private TeleportationConfirmationSurface(DialogMessageBox desktop, DialogMessageBoxView console)
        { _desktop = desktop; _console = console; }
        internal GameObject Host { get { return _desktop != null ? _desktop.gameObject : _console.gameObject; } }
        internal bool ControllerMatches { get { return Game.Instance != null && Game.Instance.IsControllerGamepad == (_console != null); } }
        internal DialogMessageBoxVM Model { get { return _console == null ? null : (DialogMessageBoxVM)ConsoleViewModel.GetValue(_console, null); } }
        internal bool Shown { get { return ControllerMatches && (_desktop != null ? _desktop.IsShown : _console != null && _console.gameObject.activeInHierarchy && Model != null); } }
        internal bool Owns(Action<DialogMessageBoxBase.BoxButton> callback)
        {
            if (!ControllerMatches) return false;
            if (_desktop != null) return ReferenceEquals(WorldMapPointSpellActionPatches.ConfirmationCallbackField.GetValue(_desktop), callback);
            var model = Model;
            return model != null && ReferenceEquals(ConsoleCallback.GetValue(model), callback);
        }
        internal void Close()
        {
            if (_desktop != null) _desktop.HandleForceClose();
            else { var model = Model; if (model != null) model.ForceHide(); }
        }
        internal static bool ConsoleContractValid { get { return ConsoleViewModel != null && ConsoleViewModel.PropertyType == typeof(DialogMessageBoxVM) &&
            ConsoleCallback != null && ConsoleCallback.FieldType == typeof(Action<DialogMessageBoxBase.BoxButton>); } }
        internal static TeleportationConfirmationSurface Available()
        {
            if (Game.Instance == null) return null;
            if (!Game.Instance.IsControllerGamepad)
                return DialogMessageBox.Instance == null || DialogMessageBox.Instance.IsShown ? null : new TeleportationConfirmationSurface(DialogMessageBox.Instance, null);
            if (!ConsoleContractValid) return null;
            var views = Resources.FindObjectsOfTypeAll<DialogMessageBoxView>().Where(value => value != null && value.gameObject.scene.IsValid() &&
                value.gameObject.scene.isLoaded && value.transform.parent != null && value.transform.parent.gameObject.activeInHierarchy).ToArray();
            if (views.Length != 1 || ConsoleViewModel.GetValue(views[0], null) != null) return null;
            return new TeleportationConfirmationSurface(null, views[0]);
        }
    }
}
