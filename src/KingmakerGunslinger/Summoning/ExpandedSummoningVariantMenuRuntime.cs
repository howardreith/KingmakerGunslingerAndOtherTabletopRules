using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Kingmaker.UI.ActionBar;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class ExpandedSummoningVariantMenuSnapshot
    {
        internal int SlotCount { get; set; }
        internal string ParentGuid { get; set; }
        internal string ParentName { get; set; }
        internal int ParentVariantCount { get; set; }
        internal int ScrollRectCount { get; set; }
        internal int ViewportMarkerCount { get; set; }
        internal bool NativePlacementRetained { get; set; }
        internal bool ScrollingRequired { get; set; }
        internal bool VerticalScrolling { get; set; }
        internal bool HorizontalScrolling { get; set; }
        internal float VerticalNormalizedPosition { get; set; }
        internal SummonVariantMenuRect SafeRect { get; set; }
        internal SummonVariantMenuRect FinalRect { get; set; }
        internal float DesiredWidth { get; set; }
        internal float DesiredHeight { get; set; }
        internal SummonVariantMenuRect AnchorRect { get; set; }
        internal bool NavigationVerified { get; set; }
        internal bool FirstEntryReachable { get; set; }
        internal bool MiddleEntryReachable { get; set; }
        internal bool LastEntryReachable { get; set; }
        internal bool ScrollInputEnabled { get; set; }
        internal int SelectableEntryCount { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "parent={0}:{1};variants={2};slots={3};scrollRects={4};viewports={5};native={6};scroll={7};vertical={8};horizontal={9};normalizedY={10:0.###};desired={11:0.###}x{12:0.###};final={13:0.###},{14:0.###},{15:0.###},{16:0.###};navigation={17}/{18}/{19};selectable={20}",
                ParentName, ParentGuid, ParentVariantCount, SlotCount,
                ScrollRectCount, ViewportMarkerCount,
                NativePlacementRetained, ScrollingRequired, VerticalScrolling,
                HorizontalScrolling, VerticalNormalizedPosition, DesiredWidth,
                DesiredHeight, FinalRect.X, FinalRect.Y, FinalRect.Width,
                FinalRect.Height, FirstEntryReachable,
                MiddleEntryReachable, LastEntryReachable,
                SelectableEntryCount);
        }
    }

    internal sealed class ExpandedSummoningVariantMenuViewportMarker :
        MonoBehaviour
    {
    }

    /// <summary>
    /// Unity adapter for the exact Kingmaker 2.1.7b PC variant-menu view. Geometry is
    /// delegated to SummonVariantMenuLayoutPolicy; this class only measures rendered
    /// RectTransforms, installs one reusable viewport when necessary, and restores the
    /// native hierarchy before the next native Fill.
    /// </summary>
    internal static class ExpandedSummoningVariantMenuRuntime
    {
        private static readonly ConditionalWeakTable<ActionBarSpellsGroup,
            LayoutState> States = new ConditionalWeakTable<ActionBarSpellsGroup,
                LayoutState>();
        private static readonly object StateGate = new object();
        private static long _attempts;
        private static long _applied;
        private static long _scrolling;
        private static long _failures;
        private static string _lastResult =
            "No Expanded Summoning variant menu has been measured.";

        internal static long Attempts { get { return Interlocked.Read(ref _attempts); } }
        internal static long Applied { get { return Interlocked.Read(ref _applied); } }
        internal static long Scrolling { get { return Interlocked.Read(ref _scrolling); } }
        internal static long Failures { get { return Interlocked.Read(ref _failures); } }

        internal static string LastResult
        {
            get { lock (StateGate) return _lastResult; }
        }

        internal static void PrepareForNativeFill(ActionBarSpellsGroup group)
        {
            if (group == null) return;
            LayoutState state;
            lock (States)
            {
                if (!States.TryGetValue(group, out state)) return;
            }

            state.RestoreNative();
        }

        internal static void RestoreNative(ActionBarSpellsGroup group)
        {
            PrepareForNativeFill(group);
        }

        internal static void Apply(ActionBarSpellsGroup group,
            IList<ActionBarSpontaneousConvertedSlot> slots,
            AbilityData sourceSpell)
        {
            if (group == null) throw new ArgumentNullException("group");
            if (slots == null) throw new ArgumentNullException("slots");
            if (sourceSpell == null || sourceSpell.Blueprint == null)
                throw new ArgumentNullException("sourceSpell");
            Interlocked.Increment(ref _attempts);

            RectTransform root = group.transform as RectTransform;
            if (root == null)
                throw new InvalidOperationException(
                    "ActionBarSpellsGroup does not use a RectTransform.");
            Canvas canvas = group.GetComponentInParent<Canvas>();
            if (canvas == null)
                throw new InvalidOperationException(
                    "ActionBarSpellsGroup has no rendered Canvas ancestor.");
            canvas = canvas.rootCanvas == null ? canvas : canvas.rootCanvas;
            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
                throw new InvalidOperationException(
                    "The active action-bar canvas has no RectTransform.");

            ActionBarSpontaneousConvertedSlot[] liveSlots = slots
                .Where(value => value != null && value.gameObject != null)
                .ToArray();
            if (liveSlots.Length == 0)
                throw new InvalidOperationException(
                    "The expanded variant menu opened without rendered slots.");

            LayoutState state;
            lock (States)
            {
                state = States.GetValue(group, value =>
                    new LayoutState(value));
            }
            state.CaptureNative(root);
            state.CaptureRuntime(canvasRect, liveSlots);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            Canvas.ForceUpdateCanvases();

            SummonVariantMenuRect safe = MeasureSafeRect(canvas, canvasRect);
            SummonVariantMenuRect anchor = MeasureAnchor(group, canvasRect);
            SummonVariantMenuRect nativeRect = MeasureRect(root, canvasRect);
            SummonVariantMenuRect slotRect = MeasureSlots(liveSlots,
                canvasRect);
            float preferredWidth = LayoutUtility.GetPreferredWidth(root);
            float preferredHeight = LayoutUtility.GetPreferredHeight(root);
            float desiredWidth = Math.Max(nativeRect.Width,
                Math.Max(slotRect.Width, preferredWidth));
            float desiredHeight = Math.Max(nativeRect.Height,
                Math.Max(slotRect.Height, preferredHeight));
            if (!FinitePositive(desiredWidth) || !FinitePositive(desiredHeight))
                throw new InvalidOperationException(
                    "The expanded variant menu reported invalid rendered dimensions.");

            SummonVariantMenuOpeningDirection preferredDirection =
                nativeRect.Y + (nativeRect.Height / 2f) >=
                anchor.Y + (anchor.Height / 2f)
                    ? SummonVariantMenuOpeningDirection.Up
                    : SummonVariantMenuOpeningDirection.Down;
            float margin = Math.Max(2f,
                Math.Min(anchor.Width, anchor.Height) * 0.1f);
            SummonVariantMenuLayoutDecision decision =
                SummonVariantMenuLayoutPolicy.Decide(
                    new SummonVariantMenuLayoutRequest(anchor, desiredWidth,
                        desiredHeight, safe, margin, preferredDirection));

            bool nativeFits = decision.SafeRect.Contains(nativeRect,
                    SummonVariantMenuLayoutPolicy.Epsilon) &&
                decision.SafeRect.Contains(slotRect,
                    SummonVariantMenuLayoutPolicy.Epsilon) &&
                !decision.RequiresScrolling;
            if (nativeFits)
            {
                RecordSuccess(group, sourceSpell, liveSlots.Length, state,
                    decision, anchor, true);
                return;
            }

            if (decision.RequiresScrolling)
                state.EnableViewport(liveSlots, decision, desiredWidth,
                    desiredHeight);
            ApplyRect(root, canvasRect, decision.FinalRect);
            if (decision.RequiresScrolling)
                state.FinalizeViewport(decision, desiredWidth, desiredHeight);
            Canvas.ForceUpdateCanvases();

            Interlocked.Increment(ref _applied);
            if (decision.RequiresScrolling)
                Interlocked.Increment(ref _scrolling);
            RecordSuccess(group, sourceSpell, liveSlots.Length, state,
                decision, anchor, false);
        }

        internal static bool TryGetSnapshot(ActionBarSpellsGroup group,
            out ExpandedSummoningVariantMenuSnapshot snapshot)
        {
            snapshot = null;
            if (group == null) return false;
            LayoutState state;
            lock (States)
            {
                if (!States.TryGetValue(group, out state)) return false;
            }

            snapshot = state.LastSnapshot;
            return snapshot != null;
        }

        internal static bool TryValidateNavigation(ActionBarSpellsGroup group,
            out ExpandedSummoningVariantMenuSnapshot snapshot)
        {
            snapshot = null;
            if (group == null) return false;
            LayoutState state;
            lock (States)
            {
                if (!States.TryGetValue(group, out state)) return false;
            }
            state.ValidateNavigation();
            snapshot = state.LastSnapshot;
            return snapshot != null && snapshot.NavigationVerified;
        }

        internal static void RecordFailure(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            Interlocked.Increment(ref _failures);
            lock (StateGate)
            {
                _lastResult = string.Format(CultureInfo.InvariantCulture,
                    "FAULT {0}: {1}", exception.GetType().Name,
                    exception.Message);
            }

            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Failure("summoning", "variant-menu.layout-failed",
                    "The exact Expanded Summoning variant menu could not be bounded; no spell or blueprint state was changed.",
                    exception);
            }
        }

        private static void RecordSuccess(ActionBarSpellsGroup group,
            AbilityData sourceSpell, int slotCount, LayoutState state,
            SummonVariantMenuLayoutDecision decision,
            SummonVariantMenuRect anchor, bool nativeRetained)
        {
            var snapshot = new ExpandedSummoningVariantMenuSnapshot
            {
                ParentGuid = sourceSpell.Blueprint.AssetGuid,
                ParentName = sourceSpell.Blueprint.name,
                ParentVariantCount = sourceSpell.Blueprint.Variants == null ?
                    0 : sourceSpell.Blueprint.Variants.Length,
                SlotCount = slotCount,
                ScrollRectCount = group.GetComponents<ScrollRect>().Length,
                ViewportMarkerCount = group.GetComponentsInChildren<
                    ExpandedSummoningVariantMenuViewportMarker>(true).Length,
                NativePlacementRetained = nativeRetained,
                ScrollingRequired = decision.RequiresScrolling,
                VerticalScrolling = decision.RequiresVerticalScrolling,
                HorizontalScrolling = decision.RequiresHorizontalScrolling,
                VerticalNormalizedPosition = state.VerticalNormalizedPosition,
                SafeRect = decision.SafeRect,
                FinalRect = decision.FinalRect,
                DesiredWidth = decision.DesiredWidth,
                DesiredHeight = decision.DesiredHeight,
                AnchorRect = anchor
            };
            state.LastSnapshot = snapshot;
            lock (StateGate) _lastResult = snapshot.ToString();
        }

        private static SummonVariantMenuRect MeasureSafeRect(Canvas canvas,
            RectTransform canvasRect)
        {
            Rect pixel = canvas.pixelRect;
            Rect screenSafe = Screen.safeArea;
            float xMin = Math.Max(pixel.xMin, screenSafe.xMin);
            float xMax = Math.Min(pixel.xMax, screenSafe.xMax);
            float yMin = Math.Max(pixel.yMin, screenSafe.yMin);
            float yMax = Math.Min(pixel.yMax, screenSafe.yMax);
            if (xMax <= xMin || yMax <= yMin)
            {
                xMin = pixel.xMin;
                xMax = pixel.xMax;
                yMin = pixel.yMin;
                yMax = pixel.yMax;
            }

            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null : canvas.worldCamera;
            Vector2 localMin;
            Vector2 localMax;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, new Vector2(xMin, yMin), camera,
                    out localMin) ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, new Vector2(xMax, yMax), camera,
                    out localMax))
            {
                throw new InvalidOperationException(
                    "The active canvas safe rectangle could not be converted to canvas space.");
            }

            return new SummonVariantMenuRect(
                Math.Min(localMin.x, localMax.x),
                Math.Min(localMin.y, localMax.y),
                Math.Abs(localMax.x - localMin.x),
                Math.Abs(localMax.y - localMin.y));
        }

        private static SummonVariantMenuRect MeasureAnchor(
            ActionBarSpellsGroup group, RectTransform canvasRect)
        {
            ActionBarGroupSlot owner = group.GetComponentInParent<
                ActionBarGroupSlot>();
            RectTransform anchor = owner == null ? null :
                owner.transform as RectTransform;
            if (anchor == null && group.transform.parent != null)
                anchor = group.transform.parent as RectTransform;
            if (anchor == null)
                throw new InvalidOperationException(
                    "The variant menu has no concrete action-bar anchor RectTransform.");
            return MeasureRect(anchor, canvasRect);
        }

        private static SummonVariantMenuRect MeasureSlots(
            IEnumerable<ActionBarSpontaneousConvertedSlot> slots,
            RectTransform canvasRect)
        {
            bool any = false;
            float xMin = float.PositiveInfinity;
            float xMax = float.NegativeInfinity;
            float yMin = float.PositiveInfinity;
            float yMax = float.NegativeInfinity;
            foreach (ActionBarSpontaneousConvertedSlot slot in slots)
            {
                RectTransform rect = slot.transform as RectTransform;
                if (rect == null) continue;
                SummonVariantMenuRect measured = MeasureRect(rect, canvasRect);
                xMin = Math.Min(xMin, measured.XMin);
                xMax = Math.Max(xMax, measured.XMax);
                yMin = Math.Min(yMin, measured.YMin);
                yMax = Math.Max(yMax, measured.YMax);
                any = true;
            }
            if (!any) throw new InvalidOperationException(
                "No variant slot exposed a RectTransform.");
            return new SummonVariantMenuRect(xMin, yMin, xMax - xMin,
                yMax - yMin);
        }

        private static SummonVariantMenuRect MeasureRect(RectTransform rect,
            RectTransform canvasRect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float xMin = float.PositiveInfinity;
            float xMax = float.NegativeInfinity;
            float yMin = float.PositiveInfinity;
            float yMax = float.NegativeInfinity;
            foreach (Vector3 corner in corners)
            {
                Vector3 local = canvasRect.InverseTransformPoint(corner);
                xMin = Math.Min(xMin, local.x);
                xMax = Math.Max(xMax, local.x);
                yMin = Math.Min(yMin, local.y);
                yMax = Math.Max(yMax, local.y);
            }
            return new SummonVariantMenuRect(xMin, yMin, xMax - xMin,
                yMax - yMin);
        }

        private static void ApplyRect(RectTransform root,
            RectTransform canvasRect, SummonVariantMenuRect finalRect)
        {
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                finalRect.Width);
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                finalRect.Height);
            Vector3 pivotLocal = new Vector3(
                finalRect.X + (finalRect.Width * root.pivot.x),
                finalRect.Y + (finalRect.Height * root.pivot.y), 0f);
            root.position = canvasRect.TransformPoint(pivotLocal);
        }

        private static bool FinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }

        private sealed class LayoutState
        {
            private readonly ActionBarSpellsGroup _group;
            private RectTransform _root;
            private Vector2 _anchorMin;
            private Vector2 _anchorMax;
            private Vector2 _pivot;
            private Vector2 _sizeDelta;
            private Vector3 _anchoredPosition;
            private LayoutGroup[] _nativeLayouts = new LayoutGroup[0];
            private bool[] _nativeLayoutEnabled = new bool[0];
            private ContentSizeFitter _nativeFitter;
            private bool _nativeFitterEnabled;
            private bool _captured;
            private bool _viewportApplied;
            private GameObject _viewportObject;
            private RectTransform _viewport;
            private RectTransform _content;
            private ScrollRect _scroll;
            private RectMask2D _mask;
            private GridLayoutGroup _contentGrid;
            private HorizontalLayoutGroup _contentHorizontal;
            private VerticalLayoutGroup _contentVertical;
            private ContentSizeFitter _contentFitter;
            private RectTransform _canvasRect;
            private ActionBarSpontaneousConvertedSlot[] _lastSlots =
                new ActionBarSpontaneousConvertedSlot[0];

            internal LayoutState(ActionBarSpellsGroup group)
            {
                _group = group ?? throw new ArgumentNullException("group");
            }

            internal ExpandedSummoningVariantMenuSnapshot LastSnapshot
            { get; set; }

            internal float VerticalNormalizedPosition
            {
                get { return _scroll == null ? 1f :
                    _scroll.verticalNormalizedPosition; }
            }

            internal void CaptureNative(RectTransform root)
            {
                if (root == null) throw new ArgumentNullException("root");
                if (_viewportApplied) RestoreNative();
                _root = root;
                _anchorMin = root.anchorMin;
                _anchorMax = root.anchorMax;
                _pivot = root.pivot;
                _sizeDelta = root.sizeDelta;
                _anchoredPosition = root.anchoredPosition3D;
                _nativeLayouts = root.GetComponents<LayoutGroup>() ??
                    new LayoutGroup[0];
                _nativeLayoutEnabled = _nativeLayouts.Select(value =>
                    value != null && value.enabled).ToArray();
                _nativeFitter = root.GetComponent<ContentSizeFitter>();
                _nativeFitterEnabled = _nativeFitter != null &&
                    _nativeFitter.enabled;
                _captured = true;
            }

            internal void CaptureRuntime(RectTransform canvasRect,
                ActionBarSpontaneousConvertedSlot[] slots)
            {
                _canvasRect = canvasRect;
                _lastSlots = slots == null ?
                    new ActionBarSpontaneousConvertedSlot[0] :
                    slots.ToArray();
            }

            internal void ValidateNavigation()
            {
                ExpandedSummoningVariantMenuSnapshot snapshot = LastSnapshot;
                ActionBarSpontaneousConvertedSlot[] live = _lastSlots.Where(
                    value => value != null && value.gameObject != null &&
                        value.gameObject.activeInHierarchy).ToArray();
                if (snapshot == null || _canvasRect == null ||
                    live.Length == 0) return;

                int middleIndex = live.Length / 2;
                bool first;
                bool middle;
                bool last;
                if (snapshot.ScrollingRequired)
                {
                    if (_scroll == null || !_scroll.enabled ||
                        _viewport == null || _content == null)
                        return;
                    SetScrollPosition(1f);
                    first = Intersects(MeasureRect(
                        live[0].transform as RectTransform, _canvasRect),
                        MeasureRect(_viewport, _canvasRect));
                    SetScrollPosition(0.5f);
                    middle = Intersects(MeasureRect(
                        live[middleIndex].transform as RectTransform,
                        _canvasRect), MeasureRect(_viewport, _canvasRect));
                    SetScrollPosition(0f);
                    last = Intersects(MeasureRect(
                        live[live.Length - 1].transform as RectTransform,
                        _canvasRect), MeasureRect(_viewport, _canvasRect));
                    SetScrollPosition(1f);
                    snapshot.ScrollInputEnabled =
                        (_scroll.vertical || _scroll.horizontal) &&
                        _scroll.movementType == ScrollRect.MovementType.Clamped;
                }
                else
                {
                    first = snapshot.SafeRect.Contains(MeasureRect(
                        live[0].transform as RectTransform, _canvasRect), 0.5f);
                    middle = snapshot.SafeRect.Contains(MeasureRect(
                        live[middleIndex].transform as RectTransform,
                        _canvasRect), 0.5f);
                    last = snapshot.SafeRect.Contains(MeasureRect(
                        live[live.Length - 1].transform as RectTransform,
                        _canvasRect), 0.5f);
                    snapshot.ScrollInputEnabled = true;
                }
                snapshot.SelectableEntryCount = live.Count(value =>
                {
                    Selectable selectable = value.GetComponentInChildren<
                        Selectable>(true);
                    // Alignment and other native availability rules may make an
                    // option non-interactable.  Its native selectable still has
                    // to survive reparenting so focus can reach it when valid.
                    return selectable != null;
                });
                snapshot.FirstEntryReachable = first;
                snapshot.MiddleEntryReachable = middle;
                snapshot.LastEntryReachable = last;
                snapshot.NavigationVerified = first && middle && last &&
                    snapshot.ScrollInputEnabled &&
                    snapshot.SelectableEntryCount == live.Length;
                snapshot.VerticalNormalizedPosition =
                    VerticalNormalizedPosition;
            }

            private void SetScrollPosition(float normalized)
            {
                if (_scroll.vertical)
                    _scroll.verticalNormalizedPosition = normalized;
                if (_scroll.horizontal)
                    _scroll.horizontalNormalizedPosition = 1f - normalized;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
                Canvas.ForceUpdateCanvases();
            }

            private static bool Intersects(SummonVariantMenuRect first,
                SummonVariantMenuRect second)
            {
                return first.XMax > second.XMin && first.XMin < second.XMax &&
                    first.YMax > second.YMin && first.YMin < second.YMax;
            }

            internal void EnableViewport(
                IList<ActionBarSpontaneousConvertedSlot> slots,
                SummonVariantMenuLayoutDecision decision,
                float desiredWidth, float desiredHeight)
            {
                if (!_captured || _root == null)
                    throw new InvalidOperationException(
                        "Native variant-menu layout was not captured.");
                LayoutGroup nativeLayout = _nativeLayouts.FirstOrDefault(
                    value => value != null && value.enabled);
                if (nativeLayout == null)
                    throw new InvalidOperationException(
                        "The exact native variant menu has no enabled LayoutGroup.");
                EnsureScaffold();
                ConfigureContentLayout(nativeLayout);

                foreach (LayoutGroup layout in _nativeLayouts)
                    if (layout != null) layout.enabled = false;
                if (_nativeFitter != null) _nativeFitter.enabled = false;
                _viewportObject.SetActive(true);
                for (int index = 0; index < slots.Count; index++)
                {
                    ActionBarSpontaneousConvertedSlot slot = slots[index];
                    if (slot == null) continue;
                    slot.transform.SetParent(_content, false);
                    slot.transform.SetSiblingIndex(index);
                }

                _scroll.horizontal = decision.RequiresHorizontalScrolling;
                _scroll.vertical = decision.RequiresVerticalScrolling;
                _scroll.movementType = ScrollRect.MovementType.Clamped;
                _scroll.inertia = true;
                _scroll.decelerationRate = 0.135f;
                _scroll.scrollSensitivity = Math.Max(20f,
                    Math.Min(decision.FinalRect.Width,
                        decision.FinalRect.Height) * 0.08f);
                _scroll.enabled = true;
                _viewportApplied = true;
            }

            internal void FinalizeViewport(
                SummonVariantMenuLayoutDecision decision,
                float desiredWidth, float desiredHeight)
            {
                if (!_viewportApplied) return;
                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
                float preferredWidth = LayoutUtility.GetPreferredWidth(_content);
                float preferredHeight = LayoutUtility.GetPreferredHeight(_content);
                float contentWidth = Math.Max(decision.FinalRect.Width,
                    Math.Max(desiredWidth, preferredWidth));
                float contentHeight = Math.Max(decision.FinalRect.Height,
                    Math.Max(desiredHeight, preferredHeight));
                _content.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal, contentWidth);
                _content.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical, contentHeight);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
                _scroll.horizontalNormalizedPosition = 0f;
                _scroll.verticalNormalizedPosition = 1f;
            }

            internal void RestoreNative()
            {
                if (!_captured || _root == null) return;
                if (_content != null)
                {
                    var children = new List<Transform>();
                    for (int index = 0; index < _content.childCount; index++)
                    {
                        Transform child = _content.GetChild(index);
                        if (child.GetComponent<
                            ActionBarSpontaneousConvertedSlot>() != null)
                            children.Add(child);
                    }
                    foreach (Transform child in children)
                        child.SetParent(_root, false);
                }
                if (_scroll != null) _scroll.enabled = false;
                if (_viewportObject != null) _viewportObject.SetActive(false);
                for (int index = 0; index < _nativeLayouts.Length; index++)
                {
                    if (_nativeLayouts[index] != null)
                        _nativeLayouts[index].enabled =
                            _nativeLayoutEnabled[index];
                }
                if (_nativeFitter != null)
                    _nativeFitter.enabled = _nativeFitterEnabled;
                _root.anchorMin = _anchorMin;
                _root.anchorMax = _anchorMax;
                _root.pivot = _pivot;
                _root.sizeDelta = _sizeDelta;
                _root.anchoredPosition3D = _anchoredPosition;
                _viewportApplied = false;
            }

            private void EnsureScaffold()
            {
                if (_scroll != null) return;
                ScrollRect existing = _root.GetComponent<ScrollRect>();
                if (existing != null)
                    throw new InvalidOperationException(
                        "The exact native variant menu already has an unowned ScrollRect.");

                _scroll = _root.gameObject.AddComponent<ScrollRect>();
                _scroll.enabled = false;
                _viewportObject = new GameObject(
                    "KMG Expanded Summoning Variant Viewport",
                    typeof(RectTransform));
                _viewportObject.hideFlags = HideFlags.DontSave;
                _viewport = (RectTransform)_viewportObject.transform;
                _viewport.SetParent(_root, false);
                Stretch(_viewport);
                _mask = _viewportObject.AddComponent<RectMask2D>();
                _viewportObject.AddComponent<
                    ExpandedSummoningVariantMenuViewportMarker>();

                GameObject contentObject = new GameObject(
                    "KMG Expanded Summoning Variant Content",
                    typeof(RectTransform));
                contentObject.hideFlags = HideFlags.DontSave;
                _content = (RectTransform)contentObject.transform;
                _content.SetParent(_viewport, false);
                _content.anchorMin = new Vector2(0f, 1f);
                _content.anchorMax = new Vector2(0f, 1f);
                _content.pivot = new Vector2(0f, 1f);
                _content.anchoredPosition = Vector2.zero;
                _contentGrid = contentObject.AddComponent<GridLayoutGroup>();
                _contentHorizontal = contentObject.AddComponent<
                    HorizontalLayoutGroup>();
                _contentVertical = contentObject.AddComponent<
                    VerticalLayoutGroup>();
                _contentFitter = contentObject.AddComponent<
                    ContentSizeFitter>();
                _contentGrid.enabled = false;
                _contentHorizontal.enabled = false;
                _contentVertical.enabled = false;
                _contentFitter.enabled = false;
                _scroll.viewport = _viewport;
                _scroll.content = _content;
                _viewportObject.SetActive(false);
            }

            private void ConfigureContentLayout(LayoutGroup source)
            {
                _contentGrid.enabled = false;
                _contentHorizontal.enabled = false;
                _contentVertical.enabled = false;
                GridLayoutGroup grid = source as GridLayoutGroup;
                HorizontalLayoutGroup horizontal = source as
                    HorizontalLayoutGroup;
                VerticalLayoutGroup vertical = source as VerticalLayoutGroup;
                if (grid != null)
                {
                    CopyCommon(grid, _contentGrid);
                    _contentGrid.cellSize = grid.cellSize;
                    _contentGrid.spacing = grid.spacing;
                    _contentGrid.startCorner = grid.startCorner;
                    _contentGrid.startAxis = grid.startAxis;
                    _contentGrid.constraint = grid.constraint;
                    _contentGrid.constraintCount = grid.constraintCount;
                    _contentGrid.enabled = true;
                }
                else if (horizontal != null)
                {
                    CopyLinear(horizontal, _contentHorizontal);
                    _contentHorizontal.enabled = true;
                }
                else if (vertical != null)
                {
                    CopyLinear(vertical, _contentVertical);
                    _contentVertical.enabled = true;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Unsupported native variant-menu LayoutGroup: " +
                        source.GetType().FullName);
                }

                if (_nativeFitter != null)
                {
                    _contentFitter.horizontalFit = _nativeFitter.horizontalFit;
                    _contentFitter.verticalFit = _nativeFitter.verticalFit;
                }
                else
                {
                    _contentFitter.horizontalFit =
                        ContentSizeFitter.FitMode.PreferredSize;
                    _contentFitter.verticalFit =
                        ContentSizeFitter.FitMode.PreferredSize;
                }
                _contentFitter.enabled = true;
            }

            private static void CopyLinear(
                HorizontalOrVerticalLayoutGroup source,
                HorizontalOrVerticalLayoutGroup target)
            {
                CopyCommon(source, target);
                target.spacing = source.spacing;
                target.childControlWidth = source.childControlWidth;
                target.childControlHeight = source.childControlHeight;
                target.childForceExpandWidth = source.childForceExpandWidth;
                target.childForceExpandHeight = source.childForceExpandHeight;
            }

            private static void CopyCommon(LayoutGroup source,
                LayoutGroup target)
            {
                RectOffset padding = source.padding;
                target.padding = padding == null ? new RectOffset() :
                    new RectOffset(padding.left, padding.right, padding.top,
                        padding.bottom);
                target.childAlignment = source.childAlignment;
            }

            private static void Stretch(RectTransform rect)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }
    }
}
