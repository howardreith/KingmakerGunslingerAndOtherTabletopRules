using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.UI.Common;
using KingmakerGunslinger.Feats;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Evidence-only capture of the real rendered game. The caller must suspend
    // its native UI state machine until this iterator finishes. No input,
    // layout substitution, scene construction, pixel editing or save operation.
    internal sealed class NativeIconScreenEvidence : IDisposable
    {
        private readonly RuntimeTestRequest _request;
        private readonly JArray _records = new JArray();
        private readonly JObject _overlay = new JObject { ["status"] = "not-observed" };
        private object _modManager;
        private bool _closedModManager;
        private bool _disposed;
        private static readonly Type ModManagerUi = typeof(UnityModManagerNet.UnityModManager)
            .GetNestedType("UI", BindingFlags.Public);

        private static object ModManagerInstance => ModManagerUi.GetProperty("Instance").GetValue(null, null);
        private static bool ModManagerOpened(object value) => (bool)ModManagerUi.GetProperty("Opened").GetValue(value, null);
        private static int ModSettingsIndex(object value, string field) =>
            (int)ModManagerUi.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value);

        private void HoldUnobscuredScreen()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NativeIconScreenEvidence));
            if (_modManager != null) { AssertOverlayClosed(); return; }
            var manager = ModManagerInstance;
            if (manager == null) throw new InvalidOperationException("Native mod-manager UI instance is unavailable.");
            bool opened = ModManagerOpened(manager);
            _overlay["originallyOpened"] = opened;
            _overlay["assemblyMvid"] = ModManagerUi.Assembly.ManifestModule.ModuleVersionId.ToString();
            if (opened)
            {
                // Installed UMM's normal close releases its blocking Canvas and
                // restores the cursor. Require the plain startup list: no mod
                // settings callbacks or game-script callbacks may be dispatched.
                int current = ModSettingsIndex(manager, "mShowModSettings");
                int previous = ModSettingsIndex(manager, "mPreviousShowModSettings");
                var scriptsType = typeof(UnityModManagerNet.UnityModManager).GetNestedType("GameScripts", BindingFlags.NonPublic);
                var scripts = scriptsType.GetField("scripts", BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(null) as System.Collections.ICollection;
                if (current != -1 || previous != -1 || scripts == null || scripts.Count != 0)
                    throw new InvalidOperationException("Native capture will not close mod settings or dispatch unqualified game callbacks.");
                _overlay["settingsIndex"] = current;
                _overlay["previousSettingsIndex"] = previous;
                _overlay["gameScriptCount"] = scripts.Count;
                _modManager = manager;
                _closedModManager = true;
                ModManagerUi.GetMethod("ToggleWindow", new[] { typeof(bool) }).Invoke(manager, new object[] { false });
            }
            else _modManager = manager;
            AssertOverlayClosed();
            _overlay["status"] = "normal-window-close-held-for-native-capture";
            Write();
        }

        private void AssertOverlayClosed()
        {
            if (!ReferenceEquals(_modManager, ModManagerInstance) || ModManagerOpened(_modManager))
                throw new InvalidOperationException("Mod-manager ownership changed or its overlay reopened during native capture.");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_modManager == null) return;
            try
            {
                AssertOverlayClosed();
                if (_closedModManager)
                {
                    if (ModSettingsIndex(_modManager, "mShowModSettings") != -1 ||
                        ModSettingsIndex(_modManager, "mPreviousShowModSettings") != -1)
                        throw new InvalidOperationException("Mod settings changed during the request-owned capture.");
                    ModManagerUi.GetMethod("ToggleWindow", new[] { typeof(bool) }).Invoke(_modManager, new object[] { true });
                }
                bool restored = ModManagerOpened(_modManager) == (bool)_overlay["originallyOpened"];
                _overlay["restored"] = restored;
                if (!restored) throw new InvalidOperationException("Original mod-manager window state was not restored.");
                _overlay["status"] = "original-window-state-restored";
            }
            finally { Write(); }
        }

        internal static bool Supports(RuntimeTestRequest request) => request != null && request.ExitAfterCompletion &&
            (request.Scenario == RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationBaseline ||
             request.Scenario == RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationCase ||
             request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreation ||
             request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreationRegression ||
             request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationSpellbookUi ||
             request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationLevelUp);

        internal NativeIconScreenEvidence(RuntimeTestRequest request)
        {
            if (!Supports(request)) throw new InvalidOperationException("Exact guarded native-icon capture scenario and automatic exit required.");
            _request = request;
        }

        internal void AppendEvidence(ICollection<string> files)
        {
            if (_records.Count == 0) return;
            files.Add(Path.Combine(_request.EvidenceDirectory, "native-ui-screens.json"));
            foreach (JObject record in _records)
            {
                string path = Path.Combine(_request.EvidenceDirectory, (string)record["file"]);
                if (File.Exists(path)) files.Add(path);
            }
        }

        // Reveal an existing native row without selecting it or changing its
        // availability. The caller retains the controller/row identity, and the
        // exact scroll state is restored before its state machine may continue.
        internal IEnumerable<int> CaptureRow(string stage, RectTransform target,
            Func<JObject> describe, Func<bool> ownsStableUi)
        {
            if (!Supports(_request) || target == null || describe == null || ownsStableUi == null || !ownsStableUi())
                throw new InvalidOperationException("Native row capture requires an existing request-owned row.");
            // Kingmaker's inner lists can use its separate ScrollRectExtended.
            // Searching only Unity ScrollRect can accidentally move an outer page.
            var scroll = target.GetComponentsInParent<Component>(true)
                .Where(value => value is ScrollRect || value is ScrollRectExtended)
                .Select(value => new NativeRowScroll(value)).FirstOrDefault(value =>
                    value.Active && value.Vertical && value.Viewport != null && value.Content != null &&
                    target.IsChildOf(value.Content));
            if (scroll == null)
                throw new InvalidOperationException("The exact native row has no supported vertical scroll viewport.");
            Vector2 originalPosition = scroll.Position, originalVelocity = scroll.Velocity;
            Vector2 originalContentPosition = scroll.Content.anchoredPosition;
            bool originalHorizontal = scroll.Horizontal;
            int recordIndex = _records.Count;
            try
            {
                Canvas.ForceUpdateCanvases();
                float contentHeightBeforeLayout = RowBounds(scroll.Viewport, scroll.Content).size.y;
                // Finish the native layout calculation for pooled nested rows.
                // Never assign a fabricated content height or move a row itself.
                bool rebuildFactLayout = stage.StartsWith("native-selected-fact:", StringComparison.Ordinal);
                if (rebuildFactLayout) LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.Content);
                Canvas.ForceUpdateCanvases();
                scroll.Reveal(target);
                for (int frame = 0; frame < 4; frame++)
                {
                    yield return 0;
                    if (target == null || !scroll.Active || !ownsStableUi())
                        throw new InvalidOperationException("Native row ownership changed during viewport settlement.");
                }
                var bounds = RowBounds(scroll.Viewport, target);
                Rect view = scroll.Viewport.rect;
                bool visible = bounds.min.y >= view.yMin - 1 && bounds.max.y <= view.yMax + 1 &&
                    bounds.center.x >= view.xMin && bounds.center.x <= view.xMax;
                var state = describe();
                state["viewport"] = new JObject {
                    ["nativeApi"] = scroll.Api, ["scrollType"] = scroll.Component.GetType().FullName,
                    ["scrollObject"] = scroll.Component.name, ["contentObject"] = scroll.Content.name,
                    ["viewportObject"] = scroll.Viewport.name, ["contentHeight"] = RowBounds(scroll.Viewport, scroll.Content).size.y,
                    ["rowActive"] = target.gameObject.activeInHierarchy,
                    ["rowVerticallyVisible"] = visible,
                    ["rowMinY"] = bounds.min.y, ["rowMaxY"] = bounds.max.y,
                    ["viewportMinY"] = view.yMin, ["viewportMaxY"] = view.yMax,
                    ["originalX"] = originalPosition.x, ["originalY"] = originalPosition.y,
                    ["horizontalEnabled"] = originalHorizontal,
                    ["originalContentX"] = originalContentPosition.x, ["originalContentY"] = originalContentPosition.y,
                    ["captureX"] = scroll.Position.x, ["captureY"] = scroll.Position.y,
                    ["restored"] = false };
                state["nativeLayout"] = new JObject {
                    ["api"] = rebuildFactLayout ? "UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate" :
                        "UnityEngine.Canvas.ForceUpdateCanvases",
                    ["contentHeightBefore"] = contentHeightBeforeLayout,
                    ["contentHeightAfter"] = RowBounds(scroll.Viewport, scroll.Content).size.y,
                    ["targetAncestors"] = DescribeRowLayout(target, scroll.Viewport) };
                // Preserve the exact native image and bounds even when revealing
                // the target fails; a diagnostic capture never qualifies the row.
                foreach (int frame in Capture(stage, state, () => target != null && scroll.Active &&
                    target.gameObject.activeInHierarchy && ownsStableUi())) yield return frame;
                if (!visible) throw new InvalidOperationException("Native scrolling did not reveal " + stage +
                    "; row=" + bounds.min.y + ":" + bounds.max.y + "; viewport=" + view.yMin + ":" + view.yMax);
            }
            finally
            {
                bool restored = false;
                if (scroll.Component != null)
                {
                    scroll.RestorePosition(originalPosition);
                    scroll.Velocity = originalVelocity;
                    // A fitting, disabled horizontal axis has no meaningful
                    // normalized coordinate; Unity can report either zero or
                    // one after tiny layout rounding. Check its real position.
                    restored = scroll.Horizontal == originalHorizontal &&
                        Mathf.Abs(scroll.Position.y - originalPosition.y) < 0.001f &&
                        (!originalHorizontal || Mathf.Abs(scroll.Position.x - originalPosition.x) < 0.001f) &&
                        (scroll.Content.anchoredPosition - originalContentPosition).sqrMagnitude < 0.0001f &&
                        scroll.Velocity == originalVelocity;
                }
                if (_records.Count > recordIndex)
                {
                    _records[recordIndex]["nativeState"]["viewport"]["restored"] = restored;
                    if (scroll.Component != null)
                    {
                        _records[recordIndex]["nativeState"]["viewport"]["restoredContentX"] = scroll.Content.anchoredPosition.x;
                        _records[recordIndex]["nativeState"]["viewport"]["restoredContentY"] = scroll.Content.anchoredPosition.y;
                    }
                    Write();
                }
                if (!restored) throw new InvalidOperationException("Native row scroll state was not restored.");
            }
        }

        private static JArray DescribeRowLayout(RectTransform target, RectTransform viewport)
        {
            var rows = new JArray();
            for (Transform node = target; node != null && rows.Count < 20; node = node.parent)
            {
                var rect = node as RectTransform;
                if (rect == null) continue;
                var bounds = RowBounds(viewport, rect);
                var fitter = rect.GetComponent<ContentSizeFitterExtended>();
                var firearmFit = rect.GetComponent<FirearmNativeTotalFit>();
                rows.Add(new JObject { ["name"] = rect.name, ["height"] = rect.rect.height,
                    ["anchoredY"] = rect.anchoredPosition.y, ["localY"] = rect.localPosition.y,
                    ["scaleY"] = rect.localScale.y, ["preferredHeight"] = LayoutUtility.GetPreferredHeight(rect),
                    ["viewportMinY"] = bounds.min.y, ["viewportMaxY"] = bounds.max.y,
                    ["verticalFit"] = fitter == null ? null : FirearmNativeTotalFit.Mode(fitter).ToString(),
                    ["horizontalFit"] = fitter == null ? null : FirearmNativeTotalFit.Mode(fitter, false).ToString(),
                    ["fitterEnabled"] = fitter != null && fitter.enabled,
                    ["firearmFitApplied"] = firearmFit != null && firearmFit.Applied,
                    ["originalVerticalFit"] = firearmFit == null ? null : firearmFit.Original.ToString(),
                    ["originalHorizontalFit"] = firearmFit == null ? null : firearmFit.OriginalHorizontal.ToString(),
                    ["originalFitterEnabled"] = firearmFit == null ? null : (JToken)firearmFit.OriginalEnabled,
                    ["layoutElements"] = new JArray(rect.GetComponents<LayoutElement>().Select(element => new JObject {
                        ["ignoreLayout"] = element.ignoreLayout, ["minHeight"] = element.minHeight,
                        ["preferredHeight"] = element.preferredHeight, ["flexibleHeight"] = element.flexibleHeight })),
                    ["components"] = new JArray(rect.GetComponents<Component>()
                        .Where(component => component != null).Select(component => component.GetType().FullName)) });
            }
            return rows;
        }

        private sealed class NativeRowScroll
        {
            private readonly ScrollRect _standard;
            private readonly ScrollRectExtended _extended;
            internal readonly Component Component;
            internal NativeRowScroll(Component component)
            { Component = component; _standard = component as ScrollRect; _extended = component as ScrollRectExtended; }
            internal bool Active => Component != null && (_standard != null ? _standard.isActiveAndEnabled : _extended.isActiveAndEnabled);
            internal bool Vertical => _standard != null ? _standard.vertical : _extended.vertical;
            internal bool Horizontal => _standard != null ? _standard.horizontal : _extended.horizontal;
            internal RectTransform Viewport => _standard != null ? _standard.viewport : _extended.viewport;
            internal RectTransform Content => _standard != null ? _standard.content : _extended.content;
            internal string Api => _standard != null ? "UnityEngine.UI.ScrollRect.normalizedPosition" :
                "Kingmaker.UI.Common.ScrollRectExtended.ScrollToRectCenter";
            internal Vector2 Position => _standard != null ? _standard.normalizedPosition : _extended.normalizedPosition;
            internal void RestorePosition(Vector2 value)
            {
                if (_standard != null)
                {
                    _standard.verticalNormalizedPosition = value.y;
                    if (_standard.horizontal) _standard.horizontalNormalizedPosition = value.x;
                }
                else
                {
                    _extended.verticalNormalizedPosition = value.y;
                    if (_extended.horizontal) _extended.horizontalNormalizedPosition = value.x;
                }
            }
            internal Vector2 Velocity
            {
                get => _standard != null ? _standard.velocity : _extended.velocity;
                set { if (_standard != null) _standard.velocity = value; else _extended.velocity = value; }
            }
            internal void Reveal(RectTransform target)
            {
                if (_extended != null)
                {
                    _extended.StopMovement();
                    _extended.ScrollToRectCenter(target, Content);
                    return;
                }
                _standard.StopMovement();
                var bounds = RowBounds(Viewport, target);
                float range = RowBounds(Viewport, Content).size.y - Viewport.rect.height;
                if (range > 0)
                    _standard.verticalNormalizedPosition = Mathf.Clamp01(_standard.verticalNormalizedPosition -
                        (Viewport.rect.center.y - bounds.center.y) / range);
            }
        }

        private static Bounds RowBounds(RectTransform viewport, RectTransform target)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            var bounds = new Bounds(viewport.InverseTransformPoint(corners[0]), Vector3.zero);
            foreach (var corner in corners.Skip(1)) bounds.Encapsulate(viewport.InverseTransformPoint(corner));
            return bounds;
        }

        internal IEnumerable<int> Capture(string stage, JToken state, Func<bool> ownsStableUi)
        {
            if (!Supports(_request) || ownsStableUi == null || !ownsStableUi())
                throw new InvalidOperationException("Native capture lost its exact request-owned UI state.");
            HoldUnobscuredScreen();
            // UMM destroys its blocking Canvas at frame end. Let real rendering
            // settle before requesting a framebuffer capture.
            var settling = Stopwatch.StartNew();
            for (int settle = 0; settle < 4 || settling.Elapsed.TotalSeconds < 0.4; settle++)
            {
                SuppressNativeHover();
                yield return 0;
                AssertOverlayClosed();
                if (!ownsStableUi()) throw new InvalidOperationException("Native UI ownership changed before capture.");
            }
            int width = Screen.width, height = Screen.height;
            if (width < 640 || height < 480) throw new InvalidOperationException("Native game rendering size is unavailable.");
            string basename = "native-ui-" + _records.Count.ToString("D3") + ".png";
            string path = Path.Combine(_request.EvidenceDirectory, basename);
            if (File.Exists(path)) throw new InvalidOperationException("Native screenshot destination already exists.");
            var method = Type.GetType("UnityEngine.ScreenCapture, UnityEngine.ScreenCaptureModule", true)
                .GetMethod("CaptureScreenshot", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            if (method == null) throw new InvalidOperationException("Installed native screenshot API is unavailable.");
            var record = new JObject { ["stage"] = stage, ["file"] = basename, ["requestedFrame"] = Time.frameCount,
                ["width"] = width, ["height"] = height, ["status"] = "pending", ["nativeState"] = state.DeepClone() };
            _records.Add(record);
            Write();
            var elapsed = Stopwatch.StartNew();
            bool complete = false;
            try
            {
                SuppressNativeHover();
                method.Invoke(null, new object[] { path });
                for (int frame = 0; frame < 1000 && elapsed.Elapsed.TotalSeconds < 10; frame++)
                {
                    yield return 0;
                    SuppressNativeHover();
                    AssertOverlayClosed();
                    if (!ownsStableUi() || Screen.width != width || Screen.height != height)
                        throw new InvalidOperationException("Native UI ownership or rendering size changed during capture.");
                    if (frame < 2) continue;
                    byte[] bytes = ReadCompletedPng(path, width, height);
                    if (bytes == null) continue;
                    using (var sha = SHA256.Create())
                        record["sha256"] = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
                    record["completedFrame"] = Time.frameCount;
                    record["status"] = "captured-native-screen-awaiting-visual-inspection";
                    complete = true;
                    yield break;
                }
                throw new TimeoutException("Native screenshot did not complete while its UI was held stable.");
            }
            finally
            {
                if (!complete) record["status"] = "incomplete-native-capture";
                Write();
            }
        }

        private static void SuppressNativeHover()
        {
            var tooltips = Kingmaker.Game.Instance?.UI?.TooltipsController;
            if (tooltips == null) throw new InvalidOperationException("Native hover controller is unavailable.");
            // Native UI's own temporary cooldown clears only the hover tooltip.
            // Explicit description windows remain open. Normal hover resumes
            // after the short native delay; no input or preference is changed.
            tooltips.SetTemporaryCooldown();
        }

        private static byte[] ReadCompletedPng(string path, int width, int height)
        {
            if (!File.Exists(path)) return null;
            byte[] bytes;
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (stream.Length < 32 || stream.Length > 32 * 1024 * 1024) return null;
                    bytes = new byte[(int)stream.Length];
                    int offset = 0;
                    while (offset < bytes.Length)
                    {
                        int count = stream.Read(bytes, offset, bytes.Length - offset);
                        if (count == 0) return null;
                        offset += count;
                    }
                }
            }
            catch (IOException) { return null; }
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            for (int i = 0; i < signature.Length; i++) if (bytes[i] != signature[i]) return null;
            int end = bytes.Length - 8;
            if (bytes[end] != 'I' || bytes[end + 1] != 'E' || bytes[end + 2] != 'N' || bytes[end + 3] != 'D') return null;
            if (BigEndian(bytes, 16) != width || BigEndian(bytes, 20) != height)
                throw new InvalidOperationException("Screenshot dimensions differ from the real game framebuffer.");
            return bytes;
        }

        private static int BigEndian(byte[] bytes, int at) =>
            (bytes[at] << 24) | (bytes[at + 1] << 16) | (bytes[at + 2] << 8) | bytes[at + 3];

        private void Write() => RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "native-ui-screens.json"),
            new JObject { ["schemaVersion"] = 1, ["runId"] = _request.RunId, ["scenario"] = _request.Scenario,
                ["evidenceClass"] = "real native game screenshots; visual inspection and owner approval are separate from mechanical assertions",
                ["captureApi"] = "UnityEngine.ScreenCapture.CaptureScreenshot(string)",
                ["hoverSuppressionApi"] = "TooltipsController.SetTemporaryCooldown() during capture; native delay resumes normally",
                ["dllMvid"] = typeof(NativeIconScreenEvidence).Assembly.ManifestModule.ModuleVersionId.ToString(),
                ["modManagerOverlay"] = _overlay,
                ["records"] = _records }.ToString(Formatting.Indented));
    }
}
