using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Globalmap;
using Kingmaker.Localization;
using Kingmaker.UI.Constructor;
using Kingmaker.UI.GlobalMap;
using KingmakerGunslinger.Bootstrap;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class WorldMapPointSpellActionRuntime
    {
        private static readonly Dictionary<GlobalMapMessageBox, TeleportDestinationRows> Owned = new Dictionary<GlobalMapMessageBox, TeleportDestinationRows>();
        private static readonly Dictionary<GlobalMapMessageBox, TextMeshProUGUI> RelabeledSettlement = new Dictionary<GlobalMapMessageBox, TextMeshProUGUI>();
        private static readonly Dictionary<TextMeshProUGUI, string> SettlementLabelBefore = new Dictionary<TextMeshProUGUI, string>();
        private static readonly HashSet<string> Reported = new HashSet<string>(StringComparer.Ordinal);
        internal static void Append(GlobalMapMessageBox panel)
        {
            try
            {
                Clear(panel);
                if (panel == null || !panel.gameObject.activeInHierarchy || Game.Instance.IsControllerGamepad) return;
                var context = TeleportationWorldMapAdapter.Capture(TeleportContextConfirmationPresenter.Pending);
                var location = (GlobalMapLocation)WorldMapPointSpellActionPatches.LocationField.GetValue(panel);
                var actions = TeleportationWorldMapAdapter.Compose(context, location == null ? null : location.Blueprint);
                // Critical vanilla path: return without constructing or touching UI.
                if (actions.Count == 0 || !TeleportContextConfirmationPresenter.CanBegin(actions)) return;
                TeleportationTravelers.Read(context.Player); // prove canonical associated units before offering a cast
                var dialog = (CanvasGroup)WorldMapPointSpellActionPatches.DialogField.GetValue(panel);
                var label = (TextMeshProUGUI)WorldMapPointSpellActionPatches.AcceptTextField.GetValue(panel);
                Button donor = label == null ? null : label.GetComponentInParent<Button>();
                if (dialog == null || donor == null || dialog.GetComponent<LayoutGroup>() == null)
                    throw new InvalidOperationException("Native destination layout/button donor is unavailable.");
                var rows = TeleportDestinationRows.Create(panel, dialog, donor, actions);
                Owned.Add(panel, rows);
                RelabelSettlementControl(panel);
            }
            catch (Exception exception) { Clear(panel); Report(exception); }
        }
        internal static void Clear(GlobalMapMessageBox panel)
        {
            if (ReferenceEquals(panel, null)) return;
            TeleportDestinationRows rows;
            if (!Owned.TryGetValue(panel, out rows) && !RelabeledSettlement.ContainsKey(panel)) return;
            Owned.Remove(panel);
            if (rows != null) rows.Remove();
            RestoreSettlementControl(panel);
        }
        // While spell rows coexist with the native settlement-teleport control,
        // its serialized "Teleport" label is ambiguous with "Cast Teleport".
        // The exact native label is relabeled presentationally and restored
        // byte-for-byte when the rows are removed; callbacks, ownership and
        // eligibility are untouched, and no shared localization asset changes.
        private static void RelabelSettlementControl(GlobalMapMessageBox panel)
        {
            try
            {
                // The settlement teleport button is located through the panel's own
                // hierarchy by its exact serialized OnTeleportPressed callback; the
                // control's own active state is the native eligibility gate.
                var button = panel.GetComponentsInChildren<Button>(true).FirstOrDefault(value =>
                {
                    int count = value.onClick.GetPersistentEventCount();
                    for (int index = 0; index < count; index++)
                        if (string.Equals(value.onClick.GetPersistentMethodName(index), "OnTeleportPressed", StringComparison.Ordinal)) return true;
                    return false;
                });
                var label = button == null ? null : button.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label == null || RelabeledSettlement.ContainsKey(panel)) return;
                if (!button.gameObject.activeSelf) return;
                RelabeledSettlement.Add(panel, label);
                SettlementLabelBefore[label] = label.text;
                label.text = TeleportContextPresentation.SettlementTeleportLabel(TeleportationText.Get);
            }
            catch (Exception exception) { Report(exception); }
        }
        private static void RestoreSettlementControl(GlobalMapMessageBox panel)
        {
            TextMeshProUGUI label;
            if (!RelabeledSettlement.TryGetValue(panel, out label)) return;
            RelabeledSettlement.Remove(panel);
            string before;
            if (label != null && SettlementLabelBefore.TryGetValue(label, out before))
            {
                SettlementLabelBefore.Remove(label);
                label.text = before;
            }
        }
        internal static void Forget(GlobalMapMessageBox panel, TeleportDestinationRows rows)
        {
            TeleportDestinationRows owned;
            if (!ReferenceEquals(panel, null) && Owned.TryGetValue(panel, out owned) && ReferenceEquals(owned, rows)) Owned.Remove(panel);
        }
        // The horizontal extent of the dialog's currently active native action
        // buttons: the visible parchment content region. World corners of an
        // inactive control read as zero, so the donor alone is never trusted.
        internal static NativeActionExtentInfo NativeActionExtent(CanvasGroup dialog, Transform appended)
        {
            Vector3[] corners = new Vector3[4];
            float width = 0f, minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
            foreach (Button button in dialog.GetComponentsInChildren<Button>(true))
            {
                if (button == null || !button.gameObject.activeInHierarchy ||
                    button.transform.IsChildOf(appended) || appended.IsChildOf(button.transform)) continue;
                ((RectTransform)button.transform).GetWorldCorners(corners);
                float low = Math.Min(corners[0].x, corners[2].x), high = Math.Max(corners[0].x, corners[2].x);
                width = Math.Max(width, high - low);
                minX = Math.Min(minX, low);
                maxX = Math.Max(maxX, high);
            }
            if (float.IsInfinity(minX) || width <= 0f) return NativeActionExtentInfo.Unproven;
            return new NativeActionExtentInfo { Width = width, MinX = minX, MaxX = maxX };
        }
        internal struct NativeActionExtentInfo
        {
            internal float Width;
            internal float MinX;
            internal float MaxX;
            internal static NativeActionExtentInfo Unproven
            { get { return new NativeActionExtentInfo { Width = -1f, MinX = float.NegativeInfinity, MaxX = float.PositiveInfinity }; } }
        }
        internal static void Report(Exception exception)
        {
            if (!Reported.Add(exception.GetType().FullName + ":" + exception.Message)) return;
            ModContext context;
            if (ModContext.TryGet(out context)) context.Logger.Failure("teleportation", "destination.adapter-failed",
                "Magical rows omitted; native actions preserved.", exception);
        }
    }

    // Exists only as an appended child of a native destination panel with usable
    // spell actions. It owns no campaign state and never invokes ordinary travel.
    internal sealed class TeleportDestinationRows : MonoBehaviour
    {
        private GlobalMapMessageBox _panel;
        private GlobalMapLocation _location;
        private readonly List<Row> _rows = new List<Row>();
        private readonly List<GameObject> _separators = new List<GameObject>();
        private string _separatorSignature;
        private Color _tone = Color.black;
        private RectTransform _content;
        private LayoutElement _viewportLayout;
        private float _rowHeight;
        private float _maximumHeight;
        private bool _ready;
        internal ITeleportationRolls QualificationRolls { private get; set; }
        private float RowExtent { get { return _rowHeight * 2; } }
        private static float NativeLineHeight;
        internal IReadOnlyList<WorldMapPointSpellAction> Actions { get { return _rows.Select(value => value.Action).ToArray(); } }
        internal IReadOnlyList<Button> Buttons { get { return _rows.Select(value => value.Button).ToArray(); } }

        internal static TeleportDestinationRows Create(GlobalMapMessageBox panel, CanvasGroup dialog, Button donor,
            IReadOnlyList<WorldMapPointSpellAction> actions)
        {
            var container = new GameObject("KMG_DestinationSpellActions", typeof(RectTransform));
            container.SetActive(false);
            try
            {
                container.transform.SetParent(dialog.transform, false);
                var self = container.AddComponent<TeleportDestinationRows>();
                self._panel = panel;
                self._location = (GlobalMapLocation)WorldMapPointSpellActionPatches.LocationField.GetValue(panel);
                // Settle the native layout before any measurement: the appended
                // container is still inactive, so this rebuild reflects the
                // native controls only.
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)dialog.transform);
                // The donor's live rect can be stretched by the dialog layout once
                // taller rows exist; measure the native line height once, from the
                // pristine first append, and reuse it for every later container.
                if (NativeLineHeight <= 0)
                {
                    float measured = Math.Max(LayoutUtility.GetPreferredHeight((RectTransform)donor.transform), ((RectTransform)donor.transform).rect.height);
                    if (measured <= 0) throw new InvalidOperationException("Native action height is unproven.");
                    NativeLineHeight = measured;
                }
                self._rowHeight = NativeLineHeight;
                if (self._rowHeight <= 0) throw new InvalidOperationException("Native action height is unproven.");
                self._viewportLayout = container.AddComponent<LayoutElement>();
                // The settled native action buttons are the visible parchment
                // content region; the dialog canvas group can be wider than the
                // parchment art. The donor itself can be inactive at world-map
                // points (its world corners read as zero), so the extent comes
                // from the dialog's active native buttons; the donor's own rect
                // is the fallback when no native action is currently shown.
                var nativeExtent = WorldMapPointSpellActionRuntime.NativeActionExtent(dialog, container.transform);
                float scale = Math.Max(container.transform.lossyScale.x, 0.0001f);
                // Fallback when no native action is currently shown: the donor's
                // own laid-out rect (inactive controls still retain it).
                float settledWidth = nativeExtent.Width > 0f ? nativeExtent.Width :
                    ((RectTransform)donor.transform).rect.width * (Math.Max(donor.transform.lossyScale.x, 0.0001f) / scale);
                float width = TeleportContextLayoutPolicy.ActionRowsWidth(
                    settledWidth,
                    ((RectTransform)dialog.transform).rect.width - dialog.GetComponent<LayoutGroup>().padding.horizontal);
                ((RectTransform)container.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                self._viewportLayout.minWidth = width;
                self._viewportLayout.preferredWidth = width;
                self._viewportLayout.flexibleWidth = 0;
                container.AddComponent<RectMask2D>();
                var scroll = container.AddComponent<ScrollRect>();
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                scroll.scrollSensitivity = self._rowHeight * 2;
                scroll.viewport = (RectTransform)container.transform;
                var content = new GameObject("NativeSpellRows", typeof(RectTransform));
                content.transform.SetParent(container.transform, false);
                self._content = (RectTransform)content.transform;
                self._content.anchorMin = new Vector2(0, 1);
                self._content.anchorMax = new Vector2(1, 1);
                self._content.pivot = new Vector2(0.5f, 1);
                self._content.sizeDelta = Vector2.zero;
                var layout = content.AddComponent<VerticalLayoutGroup>();
                layout.childControlWidth = true; layout.childControlHeight = true;
                layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
                var fitter = content.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                scroll.content = self._content;
                var canvas = dialog.GetComponentInParent<Canvas>();
                float canvasHeight = canvas == null ? 0 : ((RectTransform)canvas.transform).rect.height;
                // Clear detached the previous source container. Recalculate the
                // native-only body before reading its cached preferred height;
                // otherwise repeated selection can include the previous rows' height.
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)dialog.transform);
                float nativeHeight = LayoutUtility.GetPreferredHeight((RectTransform)dialog.transform);
                float anchorY = Game.GetCamera().WorldToViewportPoint(self._location.LocationTooltipPoint.position).y;
                self._maximumHeight = TeleportContextLayoutPolicy.MaximumRowsHeight(canvasHeight, anchorY, nativeHeight, self._rowHeight * 2);
                var donorLabel = donor.GetComponentInChildren<TextMeshProUGUI>(true);
                self._tone = donorLabel == null ? Color.black : donorLabel.color;
                foreach (var action in actions) self.Add(donor, action);
                self.RefreshGroupSeparators();
                self.Resize();
                self._ready = true;
                container.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(self._content);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)dialog.transform);
                // Visual containment is proven on the settled render geometry,
                // never assumed from the pre-layout width choice above.
                Vector3[] rowCorners = new Vector3[4];
                foreach (Row row in self._rows)
                {
                    ((RectTransform)row.Button.transform).GetWorldCorners(rowCorners);
                    if (!TeleportContextLayoutPolicy.RowInsideNativeExtent(
                        Math.Min(rowCorners[0].x, rowCorners[2].x), Math.Max(rowCorners[0].x, rowCorners[2].x),
                        nativeExtent.MinX, nativeExtent.MaxX))
                        throw new InvalidOperationException("Appended spell rows exceed the settled native action extent.");
                }
                return self;
            }
            catch { container.SetActive(false); container.transform.SetParent(null, false); Destroy(container); throw; }
        }
        private void Add(Button donor, WorldMapPointSpellAction action)
        {
            GameObject clone = Instantiate(donor.gameObject, _content, false);
            clone.name = "KMG_DestinationSpell_" + action.Source.Key;
            // LocalizedUIText.Awake would restore the donor's serialized "Accept"
            // text on activation. Remove only this component from our inactive clone.
            foreach (var localization in clone.GetComponentsInChildren<LocalizedUIText>(true)) DestroyImmediate(localization);
            Button button = clone.GetComponent<Button>();
            // Replacing the event removes serialized native Accept/Hide callbacks,
            // including persistent listeners that RemoveAllListeners cannot remove.
            button.onClick = new Button.ButtonClickedEvent();
            ButtonPF native = button as ButtonPF;
            if (native != null) {
                native.OnRightClick = new Button.ButtonClickedEvent();
                native.OnEnter = new UnityEvent(); native.OnExit = new UnityEvent(); native.DisableWarningMessage = string.Empty;
            }
            button.interactable = true;
            var label = clone.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null) throw new InvalidOperationException("Native button clone has no label.");
            label.richText = false;
            label.enableWordWrapping = false;
            label.enableAutoSizing = true;
            label.fontSizeMax = label.fontSize;
            label.fontSizeMin = label.fontSize * 0.65f;
            // One focusable control, two lines: the full spell name above the
            // caster/cost detail, keeping each line inside the native content
            // width on compact panels.
            label.text = TeleportContextPresentation.CompactRow(action, TeleportationText.Get);
            var element = clone.GetComponent<LayoutElement>() ?? clone.AddComponent<LayoutElement>();
            element.ignoreLayout = false;
            element.minWidth = 0; element.preferredWidth = -1; element.flexibleWidth = 1;
            element.minHeight = RowExtent; element.preferredHeight = RowExtent; element.flexibleHeight = 0;
            var row = new Row { Action = action, Button = button, Label = label };
            button.onClick.AddListener(() => Choose(row));
            _rows.Add(row);
            clone.SetActive(true);
        }
        private void Choose(Row row)
        {
            try
            {
                if (!_ready || !gameObject.activeInHierarchy || !_rows.Contains(row) || TeleportContextConfirmationPresenter.Pending) return;
                var context = TeleportationWorldMapAdapter.Capture(false);
                var current = TeleportationWorldMapAdapter.Compose(context, _location.Blueprint).SingleOrDefault(value => value.Key == row.Action.Key);
                if (current == null) { WorldMapPointSpellActionRuntime.Clear(_panel); return; }
                // Hide the destination presenter before executing; its global
                // Accept handler can no longer start normal travel. Greater
                // Teleport settles directly; other spells confirm first.
                _panel.Hide();
                TeleportContextConfirmationPresenter.Begin(current, context, QualificationRolls);
            }
            catch (Exception exception) { WorldMapPointSpellActionRuntime.Report(exception); }
        }
        private void Update()
        {
            if (!_ready) return;
            try
            {
                var context = TeleportationWorldMapAdapter.Capture(TeleportContextConfirmationPresenter.Pending);
                var current = TeleportationWorldMapAdapter.Compose(context, _location == null ? null : _location.Blueprint).ToDictionary(value => value.Key, StringComparer.Ordinal);
                foreach (Row row in _rows.ToArray())
                {
                    WorldMapPointSpellAction fresh;
                    if (!current.TryGetValue(row.Action.Key, out fresh)) {
                        _rows.Remove(row); row.Button.gameObject.SetActive(false);
                        row.Button.transform.SetParent(null, false); Destroy(row.Button.gameObject);
                    } else { row.Action = fresh; row.Label.text = TeleportContextPresentation.CompactRow(fresh, TeleportationText.Get); }
                }
                if (_rows.Count == 0) WorldMapPointSpellActionRuntime.Clear(_panel);
                else { RefreshGroupSeparators(); Resize(); }
            }
            catch (Exception exception) { WorldMapPointSpellActionRuntime.Clear(_panel); WorldMapPointSpellActionRuntime.Report(exception); }
        }
        // Modest separation where road travel/settlement teleport (the native
        // controls) and magical spell/scroll actions coexist: a hairline marks
        // each change of action family inside the list. Separators are owned by
        // this container and removed with it; the native controls are untouched.
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
        private void Resize() { _viewportLayout.preferredHeight = Math.Min(_maximumHeight, _rows.Count * RowExtent); }
        internal void Remove()
        {
            _ready = false;
            gameObject.SetActive(false);
            transform.SetParent(null, false);
            Destroy(gameObject);
        }
        private void OnDestroy() { WorldMapPointSpellActionRuntime.Forget(_panel, this); }
        private sealed class Row
        {
            internal WorldMapPointSpellAction Action;
            internal Button Button;
            internal TextMeshProUGUI Label;
        }
    }
}
