using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Globalmap;
using Kingmaker.Localization;
using Kingmaker.UI.Constructor;
using Kingmaker.UI._ConsoleUI.Common;
using Kingmaker.UI._ConsoleUI.GlobalMap;
using Kingmaker.UI._ConsoleUI.Utils.MultiNavigationTool;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class WorldMapPointConsoleSpellActionRuntime
    {
        private static readonly Dictionary<GlobalMapMessageBoxView, TeleportConsoleDestinationRows> Owned = new Dictionary<GlobalMapMessageBoxView, TeleportConsoleDestinationRows>();
        internal static void Append(GlobalMapMessageBoxView panel)
        {
            try
            {
                Clear(panel);
                if (panel == null || !panel.gameObject.activeInHierarchy || !Game.Instance.IsControllerGamepad) return;
                var model = WorldMapPointConsoleSpellActionPatches.Model(panel);
                var context = TeleportationWorldMapAdapter.Capture(TeleportContextConfirmationPresenter.Pending);
                var offered = TeleportationWorldMapAdapter.Compose(context, model == null || model.Location == null ? null : model.Location.Blueprint);
                // No executable action means no UI allocation, navigation change
                // or extra route; executability is evaluated per action.
                if (!TeleportContextConfirmationPresenter.CanBegin(offered)) return;
                var actions = offered.Where(TeleportContextConfirmationPresenter.CanExecute).ToArray();
                if (actions.Length == 0) return;
                TeleportationTravelers.Read(context.Player);
                var dialog = (CanvasGroup)WorldMapPointConsoleSpellActionPatches.DialogField.GetValue(panel);
                var donor = (ConsoleButton)WorldMapPointConsoleSpellActionPatches.ConfirmField.GetValue(panel);
                if (dialog == null || donor == null || dialog.GetComponent<LayoutGroup>() == null)
                    throw new InvalidOperationException("Native gamepad destination layout/button donor is unavailable.");
                Owned.Add(panel, TeleportConsoleDestinationRows.Create(panel, model, dialog, donor, actions));
            }
            catch (Exception exception) { Clear(panel); WorldMapPointSpellActionRuntime.Report(exception); }
        }
        internal static void Register(GlobalMapMessageBoxView panel)
        {
            try { TeleportConsoleDestinationRows rows; if (Owned.TryGetValue(panel, out rows) && rows != null) rows.RegisterNavigation(); }
            catch (Exception exception) { Clear(panel); WorldMapPointSpellActionRuntime.Report(exception); }
        }
        internal static void Clear(GlobalMapMessageBoxView panel)
        {
            if (ReferenceEquals(panel, null)) return;
            TeleportConsoleDestinationRows rows;
            if (!Owned.TryGetValue(panel, out rows)) return;
            Owned.Remove(panel);
            if (rows != null) rows.Remove();
        }
        internal static void Forget(GlobalMapMessageBoxView panel, TeleportConsoleDestinationRows rows)
        {
            TeleportConsoleDestinationRows current;
            if (!ReferenceEquals(panel, null) && Owned.TryGetValue(panel, out current) && ReferenceEquals(current, rows)) Owned.Remove(panel);
        }
    }

    internal sealed class TeleportConsoleDestinationRows : MonoBehaviour
    {
        private GlobalMapMessageBoxView _panel;
        private GlobalMapMessageBoxVM _model;
        private GlobalMapLocation _location;
        private ConsoleMultiNavigationCollection _navigation;
        private readonly List<Row> _rows = new List<Row>();
        private readonly List<GameObject> _separators = new List<GameObject>();
        private string _separatorSignature;
        private Color _tone = Color.black;
        private ScrollRect _scroll;
        private RectTransform _content;
        private LayoutElement _viewportLayout;
        private float _rowHeight;
        private float _maximumHeight;
        private bool _ready;
        internal ITeleportationRolls QualificationRolls { private get; set; }
        internal IReadOnlyList<WorldMapPointSpellAction> Actions { get { return _rows.Select(value => value.Action).ToArray(); } }
        internal IReadOnlyList<ConsoleButton> Buttons { get { return _rows.Select(value => value.Button).ToArray(); } }
        internal static TeleportConsoleDestinationRows Create(GlobalMapMessageBoxView panel, GlobalMapMessageBoxVM model, CanvasGroup dialog,
            ConsoleButton donor, IReadOnlyList<WorldMapPointSpellAction> actions)
        {
            var container = new GameObject("KMG_DestinationSpellActions", typeof(RectTransform));
            container.SetActive(false);
            try
            {
                container.transform.SetParent(dialog.transform, false);
                var self = container.AddComponent<TeleportConsoleDestinationRows>();
                self._panel = panel; self._model = model; self._location = model.Location;
                // Settle the native gamepad layout before any measurement; the
                // appended container is still inactive.
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)dialog.transform);
                self._rowHeight = Math.Max(LayoutUtility.GetPreferredHeight((RectTransform)donor.transform), ((RectTransform)donor.transform).rect.height);
                if (self._rowHeight <= 0) throw new InvalidOperationException("Native gamepad action height is unproven.");
                self._viewportLayout = container.AddComponent<LayoutElement>();
                // The settled active native confirm/actions are the visible
                // parchment content region; the donor alone can be inactive.
                var nativeExtent = WorldMapPointSpellActionRuntime.NativeActionExtent(dialog, container.transform,
                    value => value is Button || value is ConsoleButton);
                float scale = Math.Max(container.transform.lossyScale.x, 0.0001f);
                float settledWidth = nativeExtent.Width > 0f ? (nativeExtent.Width - 8f) / scale :
                    ((RectTransform)donor.transform).rect.width * (Math.Max(donor.transform.lossyScale.x, 0.0001f) / scale);
                float width = TeleportContextLayoutPolicy.ActionRowsWidth(
                    settledWidth,
                    ((RectTransform)dialog.transform).rect.width - dialog.GetComponent<LayoutGroup>().padding.horizontal);
                ((RectTransform)container.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                self._viewportLayout.minWidth = width; self._viewportLayout.preferredWidth = width; self._viewportLayout.flexibleWidth = 0;
                container.AddComponent<RectMask2D>();
                self._scroll = container.AddComponent<ScrollRect>();
                self._scroll.horizontal = false; self._scroll.vertical = true;
                self._scroll.movementType = ScrollRect.MovementType.Clamped;
                self._scroll.scrollSensitivity = self._rowHeight;
                self._scroll.viewport = (RectTransform)container.transform;
                var content = new GameObject("NativeSpellRows", typeof(RectTransform));
                content.transform.SetParent(container.transform, false);
                self._content = (RectTransform)content.transform;
                self._content.anchorMin = new Vector2(0, 1); self._content.anchorMax = new Vector2(1, 1);
                self._content.pivot = new Vector2(0.5f, 1); self._content.sizeDelta = Vector2.zero;
                var layout = content.AddComponent<VerticalLayoutGroup>();
                layout.childControlWidth = true; layout.childControlHeight = true;
                layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
                content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                self._scroll.content = self._content;
                var canvas = dialog.GetComponentInParent<Canvas>();
                float canvasHeight = canvas == null ? 0 : ((RectTransform)canvas.transform).rect.height;
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)dialog.transform);
                float nativeHeight = LayoutUtility.GetPreferredHeight((RectTransform)dialog.transform);
                float anchorY = Game.GetCamera().WorldToViewportPoint(model.Location.LocationTooltipPoint.position).y;
                self._maximumHeight = TeleportContextLayoutPolicy.MaximumRowsHeight(canvasHeight, anchorY, nativeHeight, self._rowHeight);
                var donorLabel = donor.GetComponentInChildren<TextMeshProUGUI>(true);
                self._tone = donorLabel == null ? Color.black : donorLabel.color;
                foreach (var action in actions) self.Add(donor, action);
                self.RefreshGroupSeparators();
                self.Resize(); self._ready = true; container.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(self._content);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)dialog.transform);
                // Containment proven on the settled render geometry.
                Vector3[] rowCorners = new Vector3[4];
                foreach (Row row in self._rows)
                {
                    ((RectTransform)row.Button.transform).GetWorldCorners(rowCorners);
                    if (!TeleportContextLayoutPolicy.RowInsideNativeExtent(
                        Math.Min(rowCorners[0].x, rowCorners[2].x), Math.Max(rowCorners[0].x, rowCorners[2].x),
                        nativeExtent.MinX, nativeExtent.MaxX))
                        throw new InvalidOperationException("Appended gamepad spell rows exceed the settled native action extent.");
                }
                return self;
            }
            catch { container.SetActive(false); container.transform.SetParent(null, false); Destroy(container); throw; }
        }
        private void Add(ConsoleButton donor, WorldMapPointSpellAction action)
        {
            var clone = Instantiate(donor.gameObject, _content, false);
            clone.name = "KMG_DestinationSpell_" + action.Source.Key;
            foreach (var localization in clone.GetComponentsInChildren<LocalizedUIText>(true)) DestroyImmediate(localization);
            var button = clone.GetComponent<ConsoleButton>();
            button.SetInteractable(true); button.SetSelected(false);
            // Clear any serialized pointer listener on the native donor clone too.
            foreach (var pointer in clone.GetComponentsInChildren<Button>(true))
            {
                pointer.onClick = new Button.ButtonClickedEvent(); pointer.onClick.AddListener(button.OnConfirmClick);
                var native = pointer as ButtonPF;
                if (native != null) { native.OnRightClick = new Button.ButtonClickedEvent(); native.OnEnter = new UnityEvent(); native.OnExit = new UnityEvent(); native.DisableWarningMessage = string.Empty; }
            }
            var label = clone.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null) throw new InvalidOperationException("Native gamepad button clone has no label.");
            label.richText = false; label.enableWordWrapping = false; label.enableAutoSizing = true;
            label.fontSizeMax = label.fontSize; label.fontSizeMin = label.fontSize * 0.65f;
            button.SetLabel(TeleportContextPresentation.Row(action, TeleportationText.Get));
            var element = clone.GetComponent<LayoutElement>() ?? clone.AddComponent<LayoutElement>();
            element.ignoreLayout = false; element.minWidth = 0; element.preferredWidth = -1; element.flexibleWidth = 1;
            element.minHeight = _rowHeight; element.preferredHeight = _rowHeight; element.flexibleHeight = 0;
            var row = new Row { Action = action, Button = button };
            button.SetConfirmAction(() => Choose(row)); _rows.Add(row); clone.SetActive(true);
        }
        internal void RegisterNavigation()
        {
            if (!_ready) return;
            var navigation = (ConsoleMultiNavigationCollection)WorldMapPointConsoleSpellActionPatches.NavigationField.GetValue(_panel);
            if (navigation == null) return; // Native Bind has not created its input layer yet.
            if (!ReferenceEquals(_navigation, navigation)) { RemoveNavigation(); _navigation = navigation; }
            foreach (var row in _rows)
                if (!_navigation.EntitiesList.Any(value => ReferenceEquals(value, row.Button))) _navigation.AddRow(row.Button);
            // AddRow preserves every native entity, native order and current/default selection.
        }
        private void Choose(Row row)
        {
            try
            {
                if (!_ready || !gameObject.activeInHierarchy || !_rows.Contains(row) || TeleportContextConfirmationPresenter.Pending) return;
                var context = TeleportationWorldMapAdapter.Capture(false);
                var fresh = TeleportationWorldMapAdapter.Compose(context, _location.Blueprint).SingleOrDefault(value => value.Key == row.Action.Key);
                if (fresh == null) { WorldMapPointConsoleSpellActionRuntime.Clear(_panel); return; }
                var rolls = QualificationRolls;
                _model.Cancel(); // Native disposal pops only the destination input layer.
                TeleportContextConfirmationPresenter.Begin(fresh, context, rolls);
            }
            catch (Exception exception) { WorldMapPointSpellActionRuntime.Report(exception); }
        }
        private void Update()
        {
            if (!_ready) return;
            try
            {
                // An unrelated modal covering the popup invalidates the rows.
                if (TeleportationConfirmationSurface.UnrelatedModalShown())
                { WorldMapPointConsoleSpellActionRuntime.Clear(_panel); return; }
                if (_panel == null || !Game.Instance.IsControllerGamepad || !ReferenceEquals(_model, WorldMapPointConsoleSpellActionPatches.Model(_panel)))
                { WorldMapPointConsoleSpellActionRuntime.Clear(_panel); return; }
                var context = TeleportationWorldMapAdapter.Capture(TeleportContextConfirmationPresenter.Pending);
                var current = TeleportationWorldMapAdapter.Compose(context, _location.Blueprint).ToDictionary(value => value.Key, StringComparer.Ordinal);
                foreach (var row in _rows.ToArray())
                {
                    WorldMapPointSpellAction fresh;
                    if (!current.TryGetValue(row.Action.Key, out fresh))
                    {
                        _rows.Remove(row); row.Button.gameObject.SetActive(false);
                        if (_navigation != null) _navigation.RemoveEntity(row.Button);
                        row.Button.transform.SetParent(null, false); Destroy(row.Button.gameObject);
                    }
                    else { row.Action = fresh; row.Button.SetLabel(TeleportContextPresentation.Row(fresh, TeleportationText.Get)); }
                }
                if (_rows.Count == 0) WorldMapPointConsoleSpellActionRuntime.Clear(_panel);
                else { RefreshGroupSeparators(); Resize(); RegisterNavigation(); }
            }
            catch (Exception exception) { WorldMapPointConsoleSpellActionRuntime.Clear(_panel); WorldMapPointSpellActionRuntime.Report(exception); }
        }
        // Same modest hairline separation between spell and scroll action
        // families as the desktop rows; owned by this container only.
        private void RefreshGroupSeparators()
        {
            var signature = string.Join(",", _rows.Select(value => ((int)value.Action.Source.Kind).ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray());
            if (string.Equals(signature, _separatorSignature, StringComparison.Ordinal)) return;
            _separatorSignature = signature;
            foreach (GameObject separator in _separators) if (separator != null) Destroy(separator);
            _separators.Clear();
            for (int index = 1; index < _rows.Count; index++)
            {
                if (_rows[index].Action.Source.Kind == _rows[index - 1].Action.Source.Kind) continue;
                GameObject separator = TeleportationUiDivider.CreateRowSeparator(_content, "KMG_DestinationGroupRule", _tone);
                separator.transform.SetSiblingIndex(_rows[index].Button.transform.GetSiblingIndex());
                _separators.Add(separator);
            }
        }
        private void LateUpdate()
        {
            if (!_ready || _navigation == null) return;
            try
            {
                var row = _rows.SingleOrDefault(value => ReferenceEquals(value.Button, _navigation.CurrentEntity));
                if (row == null) return;
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_content, row.Button.transform);
                float height = _content.rect.height, viewport = _scroll.viewport.rect.height;
                float maximum = Math.Max(0, height - viewport);
                float offset = maximum * (1 - _scroll.verticalNormalizedPosition);
                float top = Math.Max(0, _content.rect.yMax - bounds.max.y), bottom = Math.Min(height, _content.rect.yMax - bounds.min.y);
                float next = TeleportContextScrollPolicy.Reveal(height, viewport, offset, top, bottom);
                _scroll.verticalNormalizedPosition = maximum > 0 ? 1 - next / maximum : 1;
            }
            catch (Exception exception) { WorldMapPointConsoleSpellActionRuntime.Clear(_panel); WorldMapPointSpellActionRuntime.Report(exception); }
        }
        private void Resize()
        {
            // The complete laid-out content — every active child (rows AND
            // group separators) plus the group's spacing and padding. Children
            // are read directly because the content's own driven preferred
            // height is unproven while its hierarchy is still inactive.
            float preferred = ContentPreferredHeight();
            if (preferred > 0f) _viewportLayout.preferredHeight = TeleportContextLayoutPolicy.ViewportHeight(preferred, _maximumHeight);
        }
        private float ContentPreferredHeight()
        {
            var group = _content.GetComponent<VerticalLayoutGroup>();
            if (group == null) return 0f;
            float total = 0f; int count = 0;
            foreach (Transform child in _content)
            {
                if (child == null || !child.gameObject.activeSelf) continue;
                float height = LayoutUtility.GetPreferredHeight((RectTransform)child);
                if (height <= 0f) continue;
                total += height;
                count++;
            }
            return total + (count > 0 ? group.spacing * (count - 1) : 0f) + group.padding.vertical;
        }
        internal float MaximumHeight { get { return _maximumHeight; } }
        private void RemoveNavigation()
        {
            if (_navigation == null) return;
            foreach (var row in _rows) _navigation.RemoveEntity(row.Button);
            _navigation = null;
        }
        internal void Remove()
        {
            _ready = false; gameObject.SetActive(false); RemoveNavigation();
            transform.SetParent(null, false); Destroy(gameObject);
        }
        private void OnDestroy() { RemoveNavigation(); WorldMapPointConsoleSpellActionRuntime.Forget(_panel, this); }
        private sealed class Row { internal WorldMapPointSpellAction Action; internal ConsoleButton Button; }
    }
}
