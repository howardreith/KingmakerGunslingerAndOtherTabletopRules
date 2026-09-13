using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

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
             request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationSpellbookUi);

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
